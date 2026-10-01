# MGXRM architecture

## The flow of one pipeline event

```
Dataverse pipeline
   │  Update of contact, PreOperation, synchronous
   ▼
Plugin class          MGXRM.Plugins/PluginExecution/PreContactUpdate.cs
   │  registered with [CrmPluginRegistration] and RegisteredEvents; does nothing but hand off
   ▼
Controller            Modules/<Module>/Controllers/ContactController.cs : PluginControllerBase<Contact>
   │  overrides PreUpdate(), PostUpdateSync() ...; each override is a list of model calls
   ▼
Model                 Modules/<Module>/Models/ContactModel.cs : ModelBase<Contact>, IContactModel
   │  the business rules; reads and changes the record through Images, reads the pipeline through Context
   ▼
Repositories          IRepository (CRUD), IEnvironmentVariableRepository (configuration),
                      Modules/<Module>/Repositories/ContactRepository.cs : RepositoryDecorator
   ▼
IOrganizationService
```

Each layer has one job, and that is what makes every layer testable on its own.

| Layer | Job | Must not |
|---|---|---|
| Plugin class | Declare when it runs, then construct the controller and call one operation | Contain logic or touch the service |
| Controller | Map a pipeline event to the model calls that should happen, in order | Contain conditions about data, or query anything |
| Model | Decide and act: validate, default, calculate, throw `InvalidPluginExecutionException` | Build queries itself (that's a repository's job) or reach for the raw service |
| Repository | Turn a question ("contacts with this email") into a query, and back into early bound records | Make business decisions |
| Early bound class | Give tables and columns typed names (`Contact.Fields.EmailAddress1`) | Hold behaviour |

## What the base classes provide

**`PluginControllerBase<T>`** (via `ControllerBase<T>`):
- Builds a `ContextManager` (`PluginContextManager<T>`) from the service provider.
- Builds an `ImageManager` from the context's pre, target and post images.
- Builds a `Repository` over the organization service.
- Has a virtual method for each plugin event, such as `PreCreate`, `PreUpdate` and `PostUpdateSync`. Post-operation events dispatch to a `...Sync` or `...Async` override, depending on the step's mode.
- Has a protected parameterless constructor, so a controller can be built around a fake model in tests.

**`ModelBase<T>`** gives every model:

| Member | Type | Use it to |
|---|---|---|
| `Images` | `IImageManager<T>` | Read the latest value of an attribute across target, pre and post (`GetLatestString`, `GetLatestInt`, `GetLatestOptionSet`, ...), ask `IsBeingSetOrUpdated` or `IsBeingSetAsNull`, and change the record being saved with `SetOrUpdate` (pre-operation only) |
| `Context` | `IContextManager<T>` | Read message, stage, mode, depth, user, `PrimaryEntityId`, input and output parameters |
| `Repository` | `IRepository` | Do CRUD and generic queries |
| `EnvironmentVariables` | `IEnvironmentVariableRepository` | Read configuration with `GetString`, `GetBoolean`, `GetWholeNumber`, `GetDecimal`, `GetGuid`. Value records are outer-joined with the default value |

Models always go through `Images` and `Context` rather than reading the raw execution context. That is what lets `ModelSetup` test them, and what keeps the alias names and image merging in one place.

**`RepositoryDecorator`** implements `IRepository` by delegating to another `IRepository`. A new repository:
- inherits it
- takes an `IRepository` in its constructor
- adds intention-revealing methods such as `GetByEmail`

Because every repository wraps the base `IRepository`, models can be given:
- a real chain over the real service in production
- a fake in model tests
- a real chain over a fake service in repository tests

## Where things live

Framework code sits in a `Framework` folder. Business code sits in module folders, for example `Modules/CustomerSupport/{Controllers,Models,Repositories}`, so the framework stays visibly separate. Tests mirror the source layout. Early bound classes are shared across modules in their own folder. Plugin classes live in the plugin project, because that is the assembly that gets registered.

Always describe the real paths found by discovery, not these.

## How each layer is tested

| Layer | Test style | Tool |
|---|---|---|
| Plugin class | Registration and hand-off, read straight from the attribute and the IL. Nothing executes | `PluginTestBase<TPlugin>` with fluent `PluginUnderTest....` assertions |
| Convention tests | Every plugin: registered, unique step ids, images named the way the context manager reads them, attributes consistent with `RegisteredEvents` | Already present. They pick up new plugins automatically |
| Controller | Each override calls the right model methods, in the right order | FakeItEasy fake of the model interface |
| Model | Behaviour in a described pipeline position | `ModelSetup.For<T>().Update().PreOperation().WithPreImage(...).WithTarget(...)`, with repositories faked, or an in-memory CRM via `WithFakeCrm` when the query itself matters |
| Repository | How the organization service is called, rather than outcomes | `RepositoryTestBase<TRepository>`, which also checks the class is a `RepositoryDecorator` over the repository it's given |
| Early bound class | Constructors set the logical name and id, and properties write the right attribute keys | Plain xUnit |

## Supporting facts worth knowing

- **Image aliases.** The context manager reads images named exactly `PreImage` and `PostImage`. A convention test enforces this on every plugin registration.
- **`Target` is not always an `Entity`.** For Delete, Assign, Associate, Disassociate, SetState and entity-bound Custom APIs it is an `EntityReference`. In those cases `TargetImage` is `null`, so use `Context.PrimaryEntityId` or the input parameters instead.
- **Environment variables are two records.** A definition, with its default, and an optional value. The repository handles the join. Yes/No variables are stored as `"yes"` and `"no"`.
- **spkl deploys plugins** from the registration attributes. It does not manage Custom API request parameters or response properties.
