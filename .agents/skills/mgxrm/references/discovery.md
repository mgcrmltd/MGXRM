# Discovering the repository layout

The framework's types have stable names, but where they live, and where business code goes, varies by repository and changes as teams reorganise. Discovery finds the real locations, confirms them with the user once, and records them in `.agents/mgxrm-layout.md` so that later runs only need to verify them.

## The layout file

`.agents/mgxrm-layout.md` at the repository root is committed with the code, so the whole team shares it. It records repository facts only. Machine-specific things, such as where MSBuild is installed, are detected fresh each run and never written here.

```markdown
# MGXRM layout
Last verified: 2026-10-01

## Projects
| Role | Project file | Root folder | Root namespace |
|---|---|---|---|
| Shared business and framework code | MGXRM.Common/MGXRM.Common.projitems | MGXRM.Common | MGXRM.Common |
| Plugin assembly | MGXRM.Plugins/MGXRM.Plugins.csproj | MGXRM.Plugins | MGXRM.Plugins |
| Common tests | MGXRM.Common.Tests/MGXRM.Common.Tests.csproj | MGXRM.Common.Tests | MGXRM.Common.Tests |
| Plugin tests | MGXRM.Plugins.Tests/MGXRM.Plugins.Tests.csproj | MGXRM.Plugins.Tests | MGXRM.Plugins.Tests |

## Framework
| Type | File |
|---|---|
| Plugin base | MGXRM.Plugins/Plugin.cs |
| PluginControllerBase | MGXRM.Common/Framework/Controller/PluginControllerBase.cs |
| ModelBase | MGXRM.Common/Framework/Model/ModelBase.cs |
| RepositoryDecorator | MGXRM.Common/Framework/Repositories/RepositoryDecorator.cs |
| PluginTestBase | MGXRM.Plugins.Tests/Framework/PluginTestBase.cs |
| ModelSetup | MGXRM.Common.Tests/TestCore/ModelSetup.cs |
| RepositoryTestBase | MGXRM.Common.Tests/TestCore/RepositoryTestBase.cs |

## Where new code goes
| Kind | Folder | Namespace |
|---|---|---|
| Early bound classes | MGXRM.Common/EarlyBounds | MGXRM.Common.EarlyBounds |
| Modules | MGXRM.Common/Modules/<Module>/{Controllers,Models,Repositories} | MGXRM.Common.Modules.<Module>.{Controllers,Models,Repositories} |
| Plugin classes | MGXRM.Plugins/PluginExecution | MGXRM.Plugins.PluginExecution |
| Early bound tests | MGXRM.Common.Tests/EarlyBounds | MGXRM.Common.Tests.EarlyBounds |
| Module tests | MGXRM.Common.Tests/Modules/<Module>/{Controllers,Models,Repositories} | MGXRM.Common.Tests.Modules.<Module>.{...} |
| Plugin tests | MGXRM.Plugins.Tests/PluginExecution | MGXRM.Plugins.Tests.PluginExecution |

## Modules
| Module | Tables |
|---|---|
| CustomerSupport | contact |

## Notes
- MGXRM.Common is a shared project, compiled into each project that imports it. MGXRM.Plugins.Tests must not import it.
```

The example values above are what this repository looked like when the skill was written. Treat them as a shape to fill in, not as facts.

## When the file exists: verify

For each row, check that the file or folder exists and that the type is really there. A text search for `class ModelBase` in the recorded file is enough. Also check the Modules table against the folders on disk, so that new or renamed modules are noticed.

If everything checks out, update `Last verified` and carry on without bothering the user. If something has moved, re-scan just that piece. Then tell the user in one line, for example "ModelBase moved to Core/Model; layout updated", and rewrite the file.

## When there is no file: scan

Use file-name and text searches, not guesses. Search the whole repository, excluding `packages/`, `bin/`, `obj/` and `.git/`.

| To find | Search for |
|---|---|
| Plugin base | `class \w+ : IPlugin` together with `RegisteredEvents` |
| Plugin classes | `[CrmPluginRegistration(` on classes deriving from the plugin base. Their folder is where new plugins go |
| Controller base | `class PluginControllerBase` |
| Model base | `class ModelBase` |
| Repository decorator | `class RepositoryDecorator` |
| Early bound classes | `[EntityLogicalName(` |
| Existing modules | classes deriving `ModelBase<` or `PluginControllerBase<` outside the framework folder. Their parent folder structure is the module convention |
| Plugin test base | `class PluginTestBase` |
| Model test builder | `class ModelSetup` |
| Repository test base | `class RepositoryTestBase` |
| Project files | `*.csproj` and `*.projitems`. For each source folder, the owning project is the one whose `<Compile Include>` entries list files in that folder |
| Namespaces | read the `namespace` line of one file in each folder. Do not derive namespaces from folder names, because teams sometimes diverge |

Then show the user a short summary of what you found. Ask only about what you could not determine. The common gap is where modules go in a repository with no modules yet. Suggest `<CommonRoot>/Modules/<Module>/{Controllers,Models,Repositories}`, mirrored in the test project, because that keeps business code visibly separate from the framework folder.

Once the user confirms, write the file.

## Things that make discovery stop

- **A framework type is missing**, for example there is no `RepositoryTestBase`. Tell the user what is missing and what it is used for. Offer to fall back, for repository tests, to the plain FakeItEasy style used by the framework's own `RepositoryTest`. Don't silently invent a replacement base class.
- **Two candidates for one role**, for example two plugin projects. Ask which one to use, and record the answer.
