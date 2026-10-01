# Registering files, building, testing and proving the tests

## Register every new file

The projects are old-style: `packages.config`, .NET Framework, and explicit file lists. A `.cs` file that is not listed is not compiled. Nothing warns you, and the tests in it silently never run.

- **Code in a shared project** (`.projitems`) is listed as `<Compile Include="$(MSBuildThisFileDirectory)Path\To\File.cs" />`.
- **Code in a normal project** (`.csproj`) is listed as `<Compile Include="Path\To\File.cs" />`.

Insert each entry in alphabetical position within the existing `<ItemGroup>` of `Compile` items, and use backslashes, as the existing entries do. Make these changes with a direct file edit, not a sed one-liner: shell escaping mangles the backslashes.

The plugin test project references the plugin assembly, and gets the shared code only through that assembly. Never add the shared project's `.projitems` import to the plugin test project. If you do, it compiles its own copy of every controller, and the IL-based delegation assertions start comparing two different types.

## Line endings and encoding

Match the files around you. Check them with `git ls-files --eol <file>`. In this repository, working copies are CRLF, and many C# files start with a UTF-8 BOM. Many file-writing tools produce LF, so check new files and convert them when the neighbours are CRLF. For example, in PowerShell:

```powershell
$t = [IO.File]::ReadAllText($f); [IO.File]::WriteAllText($f, ($t -replace "(?<!`r)`n", "`r`n"), (New-Object Text.UTF8Encoding $true))
```

Don't edit existing files with Git Bash's `sed -i`, which rewrites CRLF files as LF.

## Build

Use MSBuild, not `dotnet build`: these are .NET Framework projects with `packages.config`. Find MSBuild fresh each run, because it is machine-specific:

1. **Visual Studio:** `"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find MSBuild\**\Bin\MSBuild.exe`
2. **Rider's bundled MSBuild:** look for `C:\Program Files\JetBrains\*\tools\MSBuild\Current\Bin\**\MSBuild.exe`. Prefer the folder matching the machine's architecture, such as `arm64` or `amd64`.
3. **Otherwise:** tell the user what is missing.

If `packages\` is missing, the build fails. It's gitignored, so it needs `nuget restore <solution>.sln`, or opening the solution in an IDE.

```
"<MSBuild.exe>" <solution>.sln -t:Build -p:Configuration=Debug -v:minimal -nologo
```

The build must produce no errors. FakeXrmEasy obsolescence warnings (CS0618) are expected. Any new warning in code you wrote is not acceptable, and above all CS0436 (a type conflicting with an imported type).

## Run the tests

Use `vstest.console.exe`, found with vswhere `-find **\vstest.console.exe`, or `dotnet vstest`, which runs .NET Framework test DLLs fine:

```
dotnet vstest <CommonTests>\bin\Debug\<CommonTests>.dll <PluginTests>\bin\Debug\<PluginTests>.dll
```

To run a subset, use `--TestCaseFilter:"FullyQualifiedName~AccountModelTest"`. For per-test results, use `--logger:"console;verbosity=normal"`.

Run the whole suite at the end, not just the new tests. The convention tests and other modules' tests must still pass. Note the before and after totals: the count should rise by exactly the number of tests you added. That is how you know every new test file was registered.

## Prove each test can fail

A green test that cannot go red is worthless, and this step has caught real bugs that passing suites hid. For each new behaviour, make one deliberate breakage in the code under test. Rebuild, run that test class, and check two things:

- **the test that covers the behaviour goes red**
- **its neighbours stay green**

Then restore the code exactly, with `git checkout -- <file>` for tracked files, or re-apply your saved version for new files.

Good breakages:

| Layer | Breakage |
|---|---|
| Repository | Query a different attribute, or drop `ToEntity` |
| Model | Invert the condition, remove the early return, or change the attribute passed to `SetOrUpdate` |
| Controller | Remove a model call, or swap the order of two |
| Plugin | Change the filtering attributes, or call a different controller operation |
| Early bound | Misspell a logical name |

You don't need one breakage per test. You need at least one per behaviour, and you must see each new test class fail at least once. Report what you broke and what went red.

If a breakage turns nothing red, a test is missing or asserting the wrong thing. Fix the test, not the breakage.

## Deploying

Plugins are deployed with spkl, which reads the `[CrmPluginRegistration]` attributes, via the batch files beside the plugin project's `spkl.json`, e.g. `<PluginProject>\spkl\deploy-plugins.bat`. The connection string is supplied when the script runs. Don't deploy unless the user asks.

Remind the user of anything the deployment doesn't create:
- environment variable definitions, which belong in the solution
- new custom columns
- security roles the plugin's queries depend on
