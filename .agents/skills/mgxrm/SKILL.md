---
name: mgxrm
description: Build Dataverse / Dynamics 365 plugins with the MGXRM framework in this repository - plugin execution class, controller, model, repositories, early bound classes and a full set of fluent unit tests - and explain how the framework works. Use this whenever someone wants a new plugin, plugin step, business rule, validation, field defaulting, or "when X changes do Y" behaviour on a CRM/Dataverse table; asks to add a repository, model method or controller event; asks "show me an example plugin"; or asks how to use this framework, library or code, how plugins/controllers/models/repositories/tests fit together, or how to test any of them. Use it even when the user does not say "MGXRM" - any plugin or CRM business logic work in this repo should go through this skill.
---

# MGXRM plugin development

MGXRM is a layered framework for Dataverse plugins. Each plugin is a thin registration class that hands off to a controller, the controller turns a pipeline event into calls on a model, the model holds the business rules, and repositories do all data access. Every class gets unit tests in a fluent style. This skill writes that code and those tests, or explains how it all works.

The skill has three modes. Work out which one the user wants, then follow it:

| The user says something like | Mode |
|---|---|
| "When an account's credit limit changes, put it on hold if it's over the threshold" / "add a plugin that..." / "add a repository method to find..." | **Build** |
| "Show me an example plugin" / "generate the example" | **Example** |
| "How do I use this framework?" / "how do repositories work?" / "how do I test a controller?" | **Explain** |

If the request is ambiguous, ask.

## Step 1, every time: discover the layout

Folder structures differ between repositories and change over time. Never assume paths from memory or from these instructions. Every run begins with discovery, as described in `references/discovery.md`:

1. Read `.agents/mgxrm-layout.md` at the repository root, if it exists.
2. Verify that each recorded path still exists and still contains what it claims, using quick file-name and text searches.
3. If there is no layout file, or anything has drifted, scan, show the user what you found, ask about anything you could not determine, and write the updated file.

Discovery takes seconds and prevents writing code into the wrong project or namespace. In Explain mode, a light version is enough: just confirm that the paths you are about to cite exist.

## Ground rules for all generated code

These come from how the team works. They are the difference between code that fits in and code that has to be rewritten.

- **Match the code around you.** Before writing a class, open its nearest existing sibling: another controller, model, repository or test. Copy its namespaces, `using` order, constructor shapes and naming. The patterns in `references/` describe the framework. The live code is the source of truth wherever the two disagree.
- **No comments.** The team treats comments as a sign the code needed explaining. Put the meaning in names, such as well-named private methods and named constants. That means no XML doc comments, no explanatory blocks and no inline notes, in tests as well. Exception messages are the one place to explain, so make them say what went wrong and with which value.
- **Every class gets tests, and every test must be able to fail.** After the suite is green, break each piece of new behaviour once: change a field name, remove a call, or flip a condition. Confirm that the matching test goes red and the others stay green, then restore the code. A test that cannot fail proves nothing. See `references/build-and-test.md`.
- **Old-style project files.** Every new `.cs` file needs a `<Compile Include>` entry in its `.csproj` or shared `.projitems`, or it silently isn't compiled. Discovery records which project file owns which folder.
- **Framework code is not yours to change casually.** Base classes such as `ModelBase`, `PluginControllerBase`, `RepositoryDecorator` and the test bases are shared by everyone. If a feature seems to need a framework change, stop and propose it to the user rather than making it.
- **Don't commit or push** unless the user asks.

## Build mode

Read `references/architecture.md` once per session, then the reference for each layer as you reach it.

1. **Pin down the behaviour.** From the request, establish:
   - the table
   - the message, such as Create, Update, Delete, Assign or SetState
   - the stage: PreValidation, PreOperation or PostOperation
   - sync or async
   - the attributes that should trigger the plugin (filtering attributes)
   - any images needed
   - any configuration, such as environment variables

   Infer what you sensibly can and state your choices. For example: "PreOperation, synchronous, so the change happens in the same transaction and the user sees the error". Ask only about what changes the design. `references/plugin.md` has the stage and image guidance.
2. **Choose the module.** New business code lives in a module folder, such as `Modules/<Module>/Controllers|Models|Repositories`, with tests mirrored in the test project. Use the module the user names or the one that already owns the table. Otherwise suggest a name and confirm it.
3. **Work through the layers, bottom up, each with its tests:**
   1. Early bound class: create it or add the attributes you need. See `references/early-bound.md`.
   2. Repository, if the model needs data access beyond what `IRepository` provides directly. See `references/repository.md`.
   3. Model interface and methods. See `references/model.md`.
   4. Controller overrides. See `references/controller.md`.
   5. Plugin class and registration. See `references/plugin.md`.

   If the table already has a controller or model, extend it rather than creating a second one. One controller per table per module keeps every event for that table in one place.
4. **Register every new file** in the right project file.
5. **Build, run all the tests, and prove the new tests can fail.** See `references/build-and-test.md`.
6. **Report.** List:
   - the files created and changed
   - the registration you chose and why
   - the test results, including the mutation checks
   - anything the user must still do, such as creating the environment variable in Dataverse or deploying with spkl

## Example mode

Generates a complete worked example: a plugin, controller, model, repository and early bound class, each with its tests. Follow `references/example.md` exactly. Two things matter most:

- **A branch first.** Before creating anything, ask the user to create a new git branch for the example, and wait until HEAD is on a new branch. The example is meant to be read, run and probably thrown away. It should never end up on a shared branch by accident.
- **`EXAMPLE` in every name, and nothing existing touched.** Every file the example creates has `EXAMPLE` in its file name, and the classes inside match, so the example can never collide with or be confused for real code. Existing files are never modified. The single unavoidable exception is the `<Compile Include>` entries in the project files, because old-style projects don't compile files they don't list. Say so up front and list the project files you will touch.

## Explain mode

1. Run light discovery so that every path and class you mention is real in this repository.
2. Give the high-level overview from `references/architecture.md`, adapted to the discovered layout. Cover:
   - the layers and how a pipeline event flows through them
   - where each kind of class lives here
   - how each layer is tested

   Keep it to a screen or so, and show a real file from this repository for the main layers.
3. End with a numbered menu of topics the user can expand, for example:
   1. Plugin classes and registration
   2. Controllers and plugin events
   3. Models
   4. The image manager and context manager
   5. Repositories and the decorator pattern
   6. Environment variables
   7. Early bound classes
   8. Testing plugins
   9. Testing controllers
   10. Testing models with `ModelSetup`
   11. Testing repositories
   12. Building, testing and deploying
4. When the user picks a topic, read the matching reference file and the real code it covers. Explain with snippets from this repository rather than invented ones, and offer to go deeper or to build something.

## Reference files

| File | Read it when |
|---|---|
| `references/discovery.md` | At the start of every run |
| `references/architecture.md` | Building, or giving the overview |
| `references/plugin.md` | Writing or explaining plugin classes, registration and plugin tests |
| `references/controller.md` | Writing or explaining controllers and controller tests |
| `references/model.md` | Writing or explaining models, image and context managers, environment variables and model tests |
| `references/repository.md` | Writing or explaining repositories and repository tests |
| `references/early-bound.md` | Creating or extending early bound classes and their tests |
| `references/build-and-test.md` | Registering files, building, running tests, mutation checks and deployment |
| `references/example.md` | Example mode |
