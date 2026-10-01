# Controllers and controller tests

A controller translates "this pipeline event happened" into "these model operations run, in this order". It holds no conditions about data. If you find yourself writing an `if` in a controller, it belongs in the model.

There is one controller per table per module. If the table's controller already exists, add overrides to it rather than creating another.

## The class

```csharp
using System;
using <EarlyBoundNamespace>;
using <FrameworkNamespace>.Controller;
using <ModuleNamespace>.Models;

namespace <ModuleNamespace>.Controllers
{
    public class AccountController : PluginControllerBase<Account>
    {
        private readonly IAccountModel _model;

        public AccountController(IServiceProvider provider) : base(provider)
        {
            _model = new AccountModel(ImageManager, ContextManager, Repository);
        }

        public AccountController(IAccountModel model)
        {
            _model = model;
        }

        public override void PreUpdate()
        {
            _model.ApplyCreditHoldOverThreshold();
        }
    }
}
```

- **The service-provider constructor** is what the plugin calls. It hands the base class's `ImageManager`, `ContextManager` and `Repository` to the model. Build the model's specific repositories inside the model's own constructor chain, not here, so the controller stays a pure mapping.
- **The model constructor** is for tests. It uses the protected parameterless base constructor. Check the existing controllers for how this parameter is named and match it.
- **Events to override:**
  - `PreCreate`, `PreUpdate`, `PreAssign`, `PreSetState`, `PreSetStateDynamicEntity` and `PreClose`.
  - For post-operation, override the `...Sync` or `...Async` variant, e.g. `PostUpdateSync` or `PostCreateAsync`. The non-virtual `PostUpdate()` is the dispatcher that the plugin calls.
- **Order matters:** defaulting before validation, and validation before side effects. The tests pin the order.

## Controller tests

Controller tests fake the model interface with FakeItEasy, call the override, and assert which model calls happened and in what order. They need no pipeline at all.

```csharp
using FakeItEasy;
using <ModuleNamespace>.Controllers;
using <ModuleNamespace>.Models;
using Xunit;

namespace <ModuleTestNamespace>.Controllers
{
    public class AccountControllerTest
    {
        private readonly IAccountModel _model = A.Fake<IAccountModel>();
        private readonly AccountController _controller;

        public AccountControllerTest()
        {
            _controller = new AccountController(_model);
        }

        [Fact]
        public void PreUpdate_Applies_Credit_Hold_Over_The_Threshold()
        {
            _controller.PreUpdate();

            A.CallTo(() => _model.ApplyCreditHoldOverThreshold()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void PreCreate_Defaults_Before_Validating()
        {
            _controller.PreCreate();

            A.CallTo(() => _model.DefaultPaymentTerms()).MustHaveHappenedOnceExactly()
                .Then(A.CallTo(() => _model.EnforceCreditLimitIsPositive()).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void PostUpdateSync_Does_Not_Touch_The_Model()
        {
            _controller.PostUpdateSync();

            A.CallTo(_model).MustNotHaveHappened();
        }
    }
}
```

- **Write one test per override.** For each, assert every expected call. Chain with `.Then(...)` whenever more than one model call happens, so that reordering fails the test.
- **Test any event that must stay quiet** with `A.CallTo(_model).MustNotHaveHappened()`. That catches logic accidentally added to the wrong event.
- **Name tests `<Override>_<What_It_Does>`.** Match the names already used in the test project if they differ.
