# mgxrm: an AI agent skill for MGXRM plugin development

This folder teaches an AI coding agent how this repository builds Dataverse plugins, so it can write most of the code for you and explain the framework. It follows the open [Agent Skills](https://agentskills.io) format: plain Markdown with a short YAML header. No tool or model is assumed.

## What it does

| Ask something like | And the agent will |
|---|---|
| "When an account's credit limit goes over the threshold in an environment variable, put it on credit hold" | **Build** the plugin class, controller, model, repositories and early bound class as needed, each with its unit tests. It registers the files in the project files, builds, runs the whole suite, and deliberately breaks each new behaviour once to prove the tests can fail |
| "Show me an example plugin" | Ask you to create a new git branch, then generate a complete worked **example**. Every file it creates is prefixed with `EXAMPLE_`, and no existing file is changed apart from the project files' `<Compile Include>` lines |
| "How do I use this framework?" | **Explain** the architecture with real files from this repository, then offer a menu of topics to expand, such as repositories or controller tests |

Each run starts by checking the repository's layout: where the framework, modules, plugins and tests live. The layout is recorded in `.agents/mgxrm-layout.md`, so the agent adapts if folders move. Commit that file, so the whole team shares the confirmed layout.

## Using it with your tool

The skill itself is `.agents/skills/mgxrm/`. Tools that support Agent Skills pick it up automatically and use it whenever your request matches its description. You can also ask for it by name.

| Tool | How it finds the skill |
|---|---|
| OpenAI Codex, Gemini CLI, Cursor, OpenCode and other Agent Skills tools | Read `.agents/skills/` in the repository directly |
| GitHub Copilot in VS Code (agent mode) | Reads `.agents/skills/` (also `.github/skills/` and `.claude/skills/`) |
| Claude Code | Reads `.claude/skills/`. `.claude/skills/mgxrm/SKILL.md` is a small pointer to this folder, so nothing needs copying |

Tool support for these folders changes quickly. If your tool doesn't find the skill, check its documentation for where it reads project skills from. Then either copy this folder there, or add a pointer `SKILL.md` like the Claude Code one.

**Tools without skill support.** Add one line to whatever always-on instructions the tool reads, such as `AGENTS.md`, `.github/copilot-instructions.md` or a rules file:

> For plugin development or questions about the MGXRM framework, read and follow `.agents/skills/mgxrm/SKILL.md`.

**Chat-only assistants** (no file access) can still use **Explain** mode if you paste in `SKILL.md`, `references/architecture.md` and the reference for the topic you're asking about. Build and Example need an agent that can work in the repository.

## What the agent needs

- Read, write and search access to the repository
- A shell, to run git, MSBuild and the test runner. The skill finds MSBuild through Visual Studio's `vswhere` or Rider's bundled copy, and runs tests with `vstest.console.exe` or `dotnet vstest`
- NuGet packages restored (`packages/` is not committed)
- A capable coding model. The skill describes conventions and judgement calls, such as choosing a plugin stage or deciding what to test, and doesn't fill in templates, so weaker models will follow it less reliably

## What's in the folder

| File | Contents |
|---|---|
| `SKILL.md` | The entry point: the three modes, the layout check, and the team's ground rules (match surrounding code, no comments, every class tested, prove tests can fail) |
| `references/architecture.md` | How a pipeline event flows through plugin, controller, model and repository, and how each layer is tested |
| `references/discovery.md` | How the layout is found, confirmed and verified |
| `references/plugin.md`, `controller.md`, `model.md`, `repository.md`, `early-bound.md` | One per layer: the code shape, the decisions, and the tests |
| `references/build-and-test.md` | Project file registration, building, running tests, mutation checks, deployment |
| `references/example.md` | The specification for the generated example |

Agents load only what a task needs: `SKILL.md` first, then the references it points to.

## Changing the skill

Edit the files in this folder. The pointer at `.claude/skills/mgxrm/SKILL.md` repeats only the `name` and `description` lines. If you change the description here, which is what tools use to decide when to apply the skill, copy it there too.

When the framework changes, update the matching reference. For example, if a new assertion is added to `RepositoryTestBase`, add it to `references/repository.md`. Agents are told to treat the live code as the source of truth when the two disagree, but accurate references save them from rediscovering it each time.
