# Example mode: generate the worked example

The example shows every layer of the framework working together on a small, believable feature: contact housekeeping. Each layer comes with the tests the team expects. People read it to learn the patterns, run its tests, and then usually delete it. Everything here is designed so it can't be mistaken for real code, and can't damage real code.

## 1. Branch first: don't skip this

Run `git branch --show-current` and `git status --porcelain`, then ask the user to create a new branch for the example, e.g.:

> The example adds about 15 files. Please create a new branch for it, so it stays out of your real work, e.g. `git switch -c example/contact-plugin`. Or tell me to create that branch for you.

- **Wait for the answer.** Only create the branch yourself if the user says to.
- **Check before going on:** HEAD must be on a branch other than the one you started on, and not `main`, `master` or `develop`.
- **Uncommitted changes:** if there are any, point out that they will travel to the new branch, and let the user decide.

## 2. Tell the user what will and won't be touched

Before creating anything, say:
- **New files:** every one has `EXAMPLE` in its name, and is listed below.
- **Existing files:** none are modified, apart from adding `<Compile Include>` lines to the project files. Name those project files. Old-style projects compile only the files they list, so without those lines the example wouldn't build or run.
- **Existing early bound class:** if the repository already has a `Contact` class, the example uses it as-is.

## 3. Check what already exists

- **An existing early bound class for `contact`:** if it declares `firstname`, `lastname` and `emailaddress1`, use it and don't create `EXAMPLE_Contact`. If it exists but lacks any of them, stop and ask the user. Don't edit it, and don't create a second class mapped to `contact`: two early bound types for one table confuse the proxy-type resolution in FakeXrmEasy and in Dataverse.
- **Earlier example files**, i.e. anything named `EXAMPLE_*`: don't overwrite them. Tell the user, and suggest a fresh branch from a base without them.
- **The publisher prefix** for the environment variable's schema name: search existing environment variable constants (`Variable = "`) or ask. The schema name is `<prefix>_EXAMPLE_EnforceUniqueContactEmail`.

## 4. Generate

Naming:
- Prefix every file and the type inside it with `EXAMPLE_`. An interface becomes `IEXAMPLE_ContactModel`.
- The module folder is `EXAMPLE_CustomerSupport`, and namespaces follow the folders.
- Step names start with `EXAMPLE`, e.g. `"EXAMPLE Pre Contact Update"`.

Below, `Contact` means whichever contact early bound class step 3 settled on: the existing one, or `EXAMPLE_Contact`. Use the discovered layout for every path and namespace. Follow the layer references for the code shapes. Here are the specifics:

### Early bound class (only if needed)
`<EarlyBounds>/EXAMPLE_Contact.cs` with `EntityLogicalName = "contact"`. Columns: `contactid`, `firstname`, `lastname`, `emailaddress1`. Test: `<EarlyBoundTests>/EXAMPLE_ContactTest.cs`, covering the full set from `early-bound.md`.

### Repository
`Modules/EXAMPLE_CustomerSupport/Repositories/EXAMPLE_ContactRepository.cs`. `IEXAMPLE_ContactRepository` has one method:
- `List<Contact> GetByEmail(string email)`: a query by attribute on `emailaddress1`, mapped to early bound contacts.

Test: `EXAMPLE_ContactRepositoryTest : RepositoryTestBase<EXAMPLE_ContactRepository>`, with the query, writes-nothing, mapping and empty tests.

### Model
`Modules/EXAMPLE_CustomerSupport/Models/EXAMPLE_ContactModel.cs` defines `IEXAMPLE_ContactModel` and the model, with the full constructor chain:
- `(images, context, repository)`
- `+ IEXAMPLE_ContactRepository`
- `+ IEnvironmentVariableRepository`

It has three rules:

| Method | Behaviour |
|---|---|
| `MakeSurnameUppercase()` | If `lastname` is being set to a value, set it to upper case. Do nothing if it isn't being touched or is being cleared |
| `EnforceSurnameRequired()` | If `lastname` is being cleared, throw `InvalidPluginExecutionException("A contact must have a surname.")` |
| `EnforceEmailIsUnique()` | Do nothing if `emailaddress1` isn't being set or is blank. Read the environment variable once: `false` means skip, while `true` or unset means enforce. Ask the repository for contacts with that email, ignoring the record being saved (`Context.PrimaryEntityId`). Throw `"<email> is already used by another contact."` if any remain |

Test: `EXAMPLE_ContactModelTest`, grouped per method with `#region`, using `ModelSetup`. Cover every branch in the table, including:
- the variable being `true`, `false` and unset
- the record not being its own duplicate on Update
- the email check running no query when the email isn't being set

Fake the repository and the variables in most tests. Include one `WithFakeCrm` test, to show the in-memory CRM style.

### Controller
`Modules/EXAMPLE_CustomerSupport/Controllers/EXAMPLE_ContactController.cs : PluginControllerBase<Contact>`, with the service-provider constructor and the model constructor. The overrides:

| Override | Model calls, in order |
|---|---|
| `PreCreate` | `MakeSurnameUppercase`, then `EnforceEmailIsUnique` |
| `PreUpdate` | `MakeSurnameUppercase`, then `EnforceSurnameRequired`, then `EnforceEmailIsUnique` |

Test: `EXAMPLE_ContactControllerTest`. Write one test per override, asserting the order with `.Then(...)`. Add one test showing that an event left alone, e.g. `PostUpdateSync`, never touches the model.

### Plugins
In the discovered plugin folder:

| Plugin | Message | Stage and mode | Filtering attributes | Images | Calls |
|---|---|---|---|---|---|
| `EXAMPLE_PreContactCreate` | Create | PreOperation, synchronous | none | none | `PreCreate` |
| `EXAMPLE_PreContactUpdate` | Update | PreOperation, synchronous | `lastname`, `emailaddress1` | none | `PreUpdate` |

`EXAMPLE_PreContactUpdate` also has the `Depth > 1` guard. Each step gets a new GUID id.

Tests: `EXAMPLE_PreContactCreateTests` and `EXAMPLE_PreContactUpdateTests`. Each covers the full registration chain, ending in `IsConsistent()`, then delegation, plus a `DoesNotInvokeControllerOperation` for the other event.

## 5. Register, build, test, prove

Follow `build-and-test.md`:
1. Add the `<Compile Include>` entries.
2. Build.
3. Run the whole suite. The total should rise by exactly the number of example tests.
4. Do the mutation checks: at least one breakage per layer.

## 6. Walk the user through it

Finish with a short tour, in the order a pipeline event travels:
- plugin, then controller, then model, then repository
- then the tests, layer by layer

Give one or two sentences per file on what it demonstrates, with `path:line` references. Point out:
- **Why each rule is a separate model method:** each one is testable, and the controller can order them.
- **Why the email check needs `emailaddress1` in the filtering attributes:** otherwise it would never fire when only the email changes.
- **What the mutation checks proved.**

End by offering the next steps:
- **Explore a part in depth:** explain mode.
- **Adapt the example into real code:** build mode, which creates properly named files and never renames the example's.
- **Remove it:** switch back to the original branch and delete the example branch.
