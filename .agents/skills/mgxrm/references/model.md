# Models and model tests

The model is where the business rules live. Each public method is one rule, named for what it does: `MakeSurnameUppercase`, `EnforceEmailIsUnique`, `ApplyCreditHoldOverThreshold`. Methods usually return nothing. They either:
- change the record being saved, through `Images.SetOrUpdate`
- write to other records, through a repository
- throw `InvalidPluginExecutionException` to stop the operation

## The interface and class

```csharp
using System.Linq;
using <EarlyBoundNamespace>;
using <FrameworkNamespace>.Interfaces;
using <FrameworkNamespace>.Model;
using <FrameworkNamespace>.Repositories;
using <ModuleNamespace>.Repositories;
using Microsoft.Xrm.Sdk;

namespace <ModuleNamespace>.Models
{
    public interface IAccountModel
    {
        void ApplyCreditHoldOverThreshold();
    }

    public class AccountModel : ModelBase<Account>, IAccountModel
    {
        public const string CreditLimitThresholdVariable = "mgxrm_CreditLimitApprovalThreshold";

        private readonly IAccountRepository _accounts;

        public AccountModel(IImageManager<Account> images, IContextManager<Account> context, IRepository repository)
            : this(images, context, repository, new AccountRepository(repository))
        {
        }

        public AccountModel(IImageManager<Account> images, IContextManager<Account> context, IRepository repository,
            IAccountRepository accounts)
            : this(images, context, repository, accounts, new EnvironmentVariableRepository(repository))
        {
        }

        public AccountModel(IImageManager<Account> images, IContextManager<Account> context, IRepository repository,
            IAccountRepository accounts, IEnvironmentVariableRepository environmentVariables)
            : base(images, context, repository, environmentVariables)
        {
            _accounts = accounts;
        }

        public void ApplyCreditHoldOverThreshold()
        {
            if (!Images.IsBeingSetOrUpdated(Account.Fields.CreditLimit)) return;

            var creditLimit = Images.GetLatestMoneyValue(Account.Fields.CreditLimit);
            var threshold = EnvironmentVariables.GetDecimal(CreditLimitThresholdVariable);
            if (creditLimit == null || threshold == null) return;

            Images.SetOrUpdate(Account.Fields.CreditOnHold, creditLimit > threshold);
        }
    }
}
```

The interface lives in the same file as the model, which is the existing convention. The controller depends on the interface, so controller tests can fake it.

**Constructor chain.** The shortest constructor is the one the controller uses. Each longer one exists so a test can inject a fake:
1. `(images, context, repository)`
2. adds the module's own repositories
3. adds `IEnvironmentVariableRepository`

Every module repository takes the base `IRepository` the model was given. If the model needs no module repository, the chain is just the two `ModelBase` shapes. Drop the middle constructor.

**Rules of thumb.**
- **Read the record through `Images`.**
  - `GetLatestString`, `GetLatestInt`, `GetLatestBool`, `GetLatestDate`, `GetLatestGuid`, `GetLatestMoney` and `GetLatestMoneyValue`, `GetLatestOptionSet` and `GetLatestEntityReference` return the newest value across target, pre and post.
  - `IsBeingSetOrUpdated(attr)` means the target contains the attribute.
  - `IsBeingSetAsNull(attr)` means it is being cleared.
  - `CombinedImage` merges all the images.
- **Change the record being saved** with `Images.SetOrUpdate(attr, value)`. This works in PreValidation and PreOperation only. After the operation, update through a repository instead.
- **Read the pipeline through `Context`:** `Message`, `Stage`, `Mode`, `Depth`, `UserId`, `PrimaryEntityId`, `InputParams` and `OutputParams`. On messages whose `Target` is an `EntityReference`, such as Delete or Assign, `TargetImage` is `null`, so use `Context.PrimaryEntityId`.
- **Configuration comes from environment variables.** Name each schema name as a `public const` on the model, so tests and the plugin's documentation can refer to it. Read a variable once per method. Decide explicitly what an unset variable (`null`) means, and test that case.
- **Queries go in repositories.** The model asks `_accounts.GetOverdueFor(id)` and never builds a `QueryExpression` itself.
- **Exceptions are documentation.** Say what was wrong and with which value: `$"{email} is already used by another contact."`.
- **Return early** when the attribute the rule is about isn't being touched. That keeps rules cheap, and it is the first thing tested.

## Model tests with `ModelSetup`

`ModelSetup` describes the pipeline position and images. It hands back a real context manager and image manager, built exactly the way the controller builds them in production, plus a repository.

```csharp
public class AccountModelTest
{
    private static AccountModel ModelFor(ModelSetup<Account> setup, IAccountRepository accounts,
        IEnvironmentVariableRepository environmentVariables)
    {
        return new AccountModel(setup.Images, setup.Context, setup.Repository, accounts, environmentVariables);
    }

    [Fact]
    public void ApplyCreditHoldOverThreshold_Puts_An_Account_Over_The_Threshold_On_Hold()
    {
        var setup = ModelSetup.For<Account>()
            .Update().PreOperation().Synchronous()
            .WithPreImage(a => a.CreditLimit = new Money(1000))
            .WithTarget(a => a.CreditLimit = new Money(6000));

        ModelFor(setup, A.Fake<IAccountRepository>(), Threshold(5000)).ApplyCreditHoldOverThreshold();

        Assert.True(setup.Images.TargetImage.CreditOnHold);
    }

    private static IEnvironmentVariableRepository Threshold(decimal? value)
    {
        var variables = A.Fake<IEnvironmentVariableRepository>();
        A.CallTo(() => variables.GetDecimal(AccountModel.CreditLimitThresholdVariable)).Returns(value);
        return variables;
    }
}
```

`ModelSetup` API:

| Area | Methods |
|---|---|
| Message | `Create()`, `Update()`, `Delete()`, `Assign()`, `SetState()`, `SetStateDynamicEntity()`, `Close()`, `Associate()`, `Disassociate()`, `Message(string)` |
| Stage | `PreValidation()`, `PreOperation()`, `PostOperation()`, `Stage(...)` |
| Mode | `Synchronous()`, `Asynchronous()` |
| Context | `Depth(int)`, `WithId(Guid)`, `WithUser(Guid)`, `WithCorrelationId`, `WithOrganisation` |
| Images | `WithPreImage(a => ...)`, `WithTarget(a => ...)`, `WithPostImage(a => ...)`, or pass an instance |
| Parameters | `WithInputParameter(name, value)`, `WithOutputParameter` |
| Data | `WithRepository(fake)`, `WithService(fake)`, `WithFakeCrm(params existingRecords)` |
| Read back | `Images`, `Context`, `Repository`, `Service`, `FakeCrm`, `ExecutionContext` |

`ModelSetup` refuses pipeline positions Dataverse never produces. Respect them; don't work around them:
- an asynchronous PreOperation step
- a pre image on Create
- a post image before the operation
- an `Entity` target on Delete or Assign

**Use the early bound type as `T`.** `ModelSetup.For<Entity>()` cannot seed early bound records into the fake CRM.

**Choosing how to supply data:**
- **When the point of the test is what the model asks for or decides,** fake the module repository and the environment variable repository with FakeItEasy. Then assert on the outcome (`setup.Images.TargetImage...`, a thrown exception) or on the calls (`A.CallTo(() => accounts.Something(...)).MustHaveHappened()`). This is the default.
- **When the point is that the real query finds the right records,** use `.WithFakeCrm(records...)`, so `setup.Repository` runs over an in-memory CRM. Build the model with real repositories over `setup.Repository`. Use this sparingly. Repository tests already cover query shape.

**Cover every branch of each rule.** For each rule, the minimum is:
- the attribute isn't being touched, so nothing happens, and if the rule queries, assert that no query was made
- the attribute is being cleared
- the condition is met, so the record changes or an exception is thrown with the value in its message
- the condition is not met
- each environment variable value, including unset (`null`)
- on Update, the record being updated is not treated as its own duplicate, where relevant

Group the tests per rule with `#region <MethodName>`, as the existing model tests do. Name them `<Method>_<Behaviour>`.
