# Plugin classes, registration and plugin tests

A plugin class is a registration with a one-line body. All behaviour lives below it, in the controller and model. That makes the plugin class trivially reviewable, and lets its tests read the registration directly instead of running anything.

## Choosing the registration

Choose the stage by what the logic does:

| Stage | Number | Use it when | Notes |
|---|---|---|---|
| PreValidation | 10 | Rejecting a request cheaply, before the database transaction starts | Images exist, but `SetOrUpdate` changes may be overwritten. Rarely needed |
| PreOperation | 20 | Changing values on the record being saved, or validating inside the transaction | The default for validation and defaulting. `Images.SetOrUpdate` writes into the target, so no extra update is needed |
| PostOperation | 40 | Creating or updating other records, or anything needing the saved record's id on Create | Synchronous runs in the transaction, so an exception rolls back the save. Asynchronous runs later and cannot fail the save |

Choose the mode: synchronous whenever the user should see a failure, or the record must change before saving. Asynchronous only for post-operation side effects that may happen later.

**Filtering attributes (Update steps).** List every attribute whose change should trigger the step. This must include every attribute the model checks with `IsBeingSetOrUpdated` or `IsBeingSetAsNull`. If you leave one out, the plugin will not run when only that attribute changes, and that bug is invisible in unit tests. An Update step with no filtering attributes runs on every update of the table, and is almost never what you want.

**Images.** Add a pre image only when the model needs values that are not in the target. On Update the target holds only the changed attributes. The image manager's `GetLatest...` methods fall back to the pre image automatically. Add a post image only for post-operation logic that needs the final values. List only the attributes needed. Image names must be exactly `PreImage` and `PostImage`, because that is what the context manager reads, and a convention test fails otherwise.

**Depth.** Guard with `Depth > 1` when the plugin, or anything it calls, could update the same table and retrigger itself. This guard is standard on Update steps here. Leave it out when the plugin must also run for updates made by other plugins or workflows, and say why.

## Naming

- Class: `<Stage><Table><Message>`, e.g. `PreContactUpdate`, `PostAccountCreate`, `PostInvoiceSetState`. A class with several steps is named for what they share; see "Several steps in one class".
- Step name: the same, in words, e.g. `"Pre Contact Update"`.
- Execution order: use `10` unless other steps on the same message and table need ordering. Search the existing registrations for the table first.
- Id: a brand-new GUID for every step. Generate it with `[guid]::NewGuid()` or `uuidgen`; never copy one. The convention tests reject duplicates, because spkl would overwrite one step with another.

## The class

```csharp
using System;
using <EarlyBoundNamespace>;
using <ModuleNamespace>.Controllers;
using Microsoft.Xrm.Sdk;

namespace <PluginNamespace>
{
    [CrmPluginRegistration(MessageNameEnum.Update, Account.EntityLogicalName, StageEnum.PreOperation,
        ExecutionModeEnum.Synchronous, Account.Fields.CreditLimit + "," + Account.Fields.CreditOnHold,
        "Pre Account Update", 10, IsolationModeEnum.Sandbox, Id = "<new guid>",
        Image1Name = "PreImage", Image1Type = ImageTypeEnum.PreImage, Image1Attributes = Account.Fields.CreditLimit)]
    public class PreAccountUpdate : Plugin
    {
        public PreAccountUpdate()
            : base(typeof(PreAccountUpdate))
        {
            base.RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(20, "Update",
                Account.EntityLogicalName, ExecutePreAccountUpdate));
        }

        protected void ExecutePreAccountUpdate(LocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new ArgumentNullException(nameof(localContext));
            }

            if (localContext.PluginExecutionContext.Depth > 1)
                return;

            new AccountController(localContext.ServiceProvider).PreUpdate();
        }
    }
}
```

The `RegisteredEvents` tuple must agree with the attribute:
- **Stage number:** 10, 20 or 40.
- **Message name:** as a string.
- **Table:** the same table as the attribute.

The base `Plugin.Execute` dispatches on all three, and `IsConsistent()` checks that they match the attribute.

For a post-operation step, call the controller's dispatcher, e.g. `PostUpdate()`. The base class routes it to `PostUpdateSync` or `PostUpdateAsync` from the step's mode. The controller overrides the `...Sync` or `...Async` method.

Before writing, open an existing plugin class in the discovered plugin folder and match it. Some repositories use string message names or a different attribute constructor.

## Plugin tests

Each plugin class has a test class deriving from `PluginTestBase<TPlugin>`. The test class lives in the discovered plugin test folder and is named after the plugin with `Tests` appended.

```csharp
using <EarlyBoundNamespace>;
using <ModuleNamespace>.Controllers;
using <PluginNamespace>;
using <PluginTestFrameworkNamespace>;
using Xunit;

namespace <PluginTestNamespace>
{
    public class PreAccountUpdateTests : PluginTestBase<PreAccountUpdate>
    {
        [Fact]
        public void Is_Registered_As_Expected()
        {
            PluginUnderTest
                .HasRegistrationCount(1)
                .IsRegisteredFor(MessageNameEnum.Update, Account.EntityLogicalName)
                .HasStepName("Pre Account Update")
                .IsPreOperation()
                .IsSynchronous()
                .IsSandbox()
                .HasOrder(10)
                .HasFilteringAttributes(Account.Fields.CreditLimit, Account.Fields.CreditOnHold)
                .HasPreImage(Account.Fields.CreditLimit)
                .IsConsistent();
        }

        [Fact]
        public void Delegates_To_The_Account_Controller()
        {
            PluginUnderTest
                .InvokesControllerOperation<AccountController>(nameof(AccountController.PreUpdate))
                .DoesNotInvokeControllerOperation<AccountController>(nameof(AccountController.PreCreate));
        }
    }
}
```

Assertions available on `PluginUnderTest`:

| Area | Assertions |
|---|---|
| Registration | `HasRegistrationCount`, `IsRegisteredFor`, `HasStepName` |
| Stage and mode | `IsPreValidation` / `IsPreOperation` / `IsPostOperation` / `HasStage`, `IsSynchronous` / `IsAsynchronous` |
| Isolation and order | `IsSandbox` / `IsNotSandbox`, `HasOrder` |
| Filtering attributes | `HasNoFilteringAttributes`, `HasSingleFilteringAttribute(attr)`, `HasFilteringAttributes(params attrs)` |
| Images | `HasPreImage(params attrs)`, `HasPostImage(params attrs)`, `HasNoImages` |
| Consistency and hand-off | `IsConsistent`, `InvokesControllerOperation<TController>(op)`, `DoesNotInvokeControllerOperation<TController>(op)` |

For a plugin with several steps, select one with `Step("Step name")` or `ForStep(...)` before asserting. See the next section.

`InvokesControllerOperation` reads the plugin's IL, so it requires the plugin to construct the controller and call the operation directly. Add the `DoesNotInvokeControllerOperation` line for the most likely wrong operation. For example, PreCreate when testing a PreUpdate step guards against copy-paste errors.

The assembly-wide convention tests already cover the new plugin. No changes are needed there, but they must pass.

## Several steps in one class

One plugin class can carry several steps. Each step has three parts that must agree:
- its own `[CrmPluginRegistration]` attribute. The attribute allows multiples, and spkl deploys each one as a separate step
- its own `RegisteredEvents` entry
- its own handler method

### When to do it

Use one step per class by default. Use one class for several steps when:
- the user asks for it, or
- the repository already groups steps this way (check the existing plugin classes), or
- the steps are the same table's events handed to the same controller, e.g. PreOperation Create and PreOperation Update running the same rules.

Never group steps for different tables. Each table has its own controller, and the plugin class should stay a single, obvious entry point into it.

### The one hard constraint

`Plugin.Execute` picks the handler by **stage + message + table**, and the delegation test does the same. Two steps in one class must therefore differ in message or stage. Two Update PreOperation steps on the same table, e.g. with different filtering attributes, cannot share a class. Put them in separate classes.

### Naming

- **Class:** name it for what the steps share, e.g. `PreContactCreateUpdate` for PreOperation Create and Update, or `ContactPostOperation` for several post-operation messages. Match any existing multi-step class.
- **Steps:** each step keeps its own step name, e.g. `"Pre Contact Create"` and `"Pre Contact Update"`, and its own new GUID.
- **Handler methods:** name each one for its step, e.g. `ExecutePreContactCreate` and `ExecutePreContactUpdate`.

### The class

```csharp
[CrmPluginRegistration(MessageNameEnum.Create, Contact.EntityLogicalName, StageEnum.PreOperation,
    ExecutionModeEnum.Synchronous, "", "Pre Contact Create", 10, IsolationModeEnum.Sandbox,
    Id = "<new guid>")]
[CrmPluginRegistration(MessageNameEnum.Update, Contact.EntityLogicalName, StageEnum.PreOperation,
    ExecutionModeEnum.Synchronous, Contact.Fields.LastName + "," + Contact.Fields.EmailAddress1,
    "Pre Contact Update", 10, IsolationModeEnum.Sandbox, Id = "<another new guid>")]
public class PreContactCreateUpdate : Plugin
{
    public PreContactCreateUpdate()
        : base(typeof(PreContactCreateUpdate))
    {
        base.RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(20, "Create",
            Contact.EntityLogicalName, ExecutePreContactCreate));
        base.RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(20, "Update",
            Contact.EntityLogicalName, ExecutePreContactUpdate));
    }

    protected void ExecutePreContactCreate(LocalPluginContext localContext)
    {
        if (localContext == null)
        {
            throw new ArgumentNullException(nameof(localContext));
        }

        new ContactController(localContext.ServiceProvider).PreCreate();
    }

    protected void ExecutePreContactUpdate(LocalPluginContext localContext)
    {
        if (localContext == null)
        {
            throw new ArgumentNullException(nameof(localContext));
        }

        if (localContext.PluginExecutionContext.Depth > 1)
            return;

        new ContactController(localContext.ServiceProvider).PreUpdate();
    }
}
```

**Each handler calls exactly one controller operation.** Don't share a handler between steps, and don't branch on the message inside one. The controller's events already separate Create from Update, and a shared handler would make each step's delegation test meaningless.

**Choose the images per step.** A Create step can have no pre image, because the record doesn't exist yet, and a PreOperation step can have no post image. Each attribute declares only what its own step needs.

**Filtering attributes per step.** Create steps don't filter, so pass `""`. Update steps list their own attributes, as above.

### Tests for a multi-step class

Write one test pair per step: a registration test and a delegation test. Select each step by name, and finish the registration chain with `IsConsistent()`, so each attribute is checked against its own `RegisteredEvents` entry. Also pin the total count once, so an extra or missing attribute fails a test.

```csharp
public class PreContactCreateUpdateTests : PluginTestBase<PreContactCreateUpdate>
{
    [Fact]
    public void Declares_A_Create_And_An_Update_Step()
    {
        PluginUnderTest.HasRegistrationCount(2);
    }

    [Fact]
    public void Create_Step_Is_Registered_As_Expected()
    {
        Step("Pre Contact Create")
            .IsRegisteredFor(MessageNameEnum.Create, Contact.EntityLogicalName)
            .IsPreOperation()
            .IsSynchronous()
            .IsSandbox()
            .HasOrder(10)
            .HasNoFilteringAttributes()
            .HasNoImages()
            .IsConsistent();
    }

    [Fact]
    public void Create_Step_Delegates_To_PreCreate()
    {
        Step("Pre Contact Create")
            .InvokesControllerOperation<ContactController>(nameof(ContactController.PreCreate))
            .DoesNotInvokeControllerOperation<ContactController>(nameof(ContactController.PreUpdate));
    }

    [Fact]
    public void Update_Step_Is_Registered_As_Expected()
    {
        Step("Pre Contact Update")
            .IsRegisteredFor(MessageNameEnum.Update, Contact.EntityLogicalName)
            .IsPreOperation()
            .IsSynchronous()
            .IsSandbox()
            .HasOrder(10)
            .HasFilteringAttributes(Contact.Fields.LastName, Contact.Fields.EmailAddress1)
            .HasNoImages()
            .IsConsistent();
    }

    [Fact]
    public void Update_Step_Delegates_To_PreUpdate()
    {
        Step("Pre Contact Update")
            .InvokesControllerOperation<ContactController>(nameof(ContactController.PreUpdate))
            .DoesNotInvokeControllerOperation<ContactController>(nameof(ContactController.PreCreate));
    }
}
```

Without `Step(...)`, any step-level assertion on a multi-step class fails with a message asking you to select a step. `ForMessage(MessageNameEnum.Update)` works too, when each message appears only once.

**Mutation check.** Swap the controller operations between the two handlers. Both delegation tests should go red.
