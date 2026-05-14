---
name: build-solution
description: Pick and build the right .sln in unified-login-main for the files that changed, and run msbuild correctly given the VS 2017 hard-coded path quirk in Build.Net.bat. Use when the user wants to compile after a code edit, before running tests, or to verify a refactor still links. Captures the solution-to-folder mapping, the Fortify configuration carve-out, and the VS 2017 → VS 2022 MSBuild path substitution.
---

# Build the right solution

There is **no single "build everything"** in this repo. Each solution covers a slice of the codebase. The build entry point in [BuildAll.bat](../../Enterprise/Subsystem/ProductLauncher/BuildAll.bat) only fans out to `Build.Net.bat` (.NET) and `BuildUI.bat` (Angular). `Build.Net.bat` hard-codes the **VS 2017 Enterprise MSBuild path** — on a VS 2022 machine you must substitute the path.

## Inputs to confirm

1. **What changed** — get `git status` or the diff in scope, then pick the solution that covers those folders. The table below maps folder → solution.
2. **Configuration** — `Debug` (default), `Release`, or `Fortify` (SAST scan). Use `Debug` for local iteration; `Fortify` is only invoked by the CI pipeline ([fortify-scan-sast-pipeline.yml](../../fortify-scan-sast-pipeline.yml)).
3. **MSBuild version available** — check `where msbuild` or look for VS 2017 vs VS 2022 install paths. The hard-coded `Build.Net.bat` path will fail on a VS 2022-only machine.

## Solution → folder map

All paths relative to repo root.

| Solution | Covers | Use when editing |
|---|---|---|
| `Enterprise/Subsystem/ProductLauncher/MasterProjectSolution.sln` | LandingAPI, LandingAPIEnterprise, Web/Landing MVC, Component.Landing, SharedObjects, DataAccess, LandingAPI.Test | **Default for API/web work** — almost any change under `Enterprise/Subsystem/ProductLauncher/Service/`, `Component/Landing/`, or `Service/SharedObjects/` |
| `Enterprise/Subsystem/ProductLauncher/UnityBatchProcessor.sln` | `WinService/BatchProcessor/` | Batch processor Windows service |
| `Enterprise/Subsystem/ProductLauncher/UserNotification.sln` | `WinService/UserNotification/` | Kafka-driven notification service |
| `Enterprise/Subsystem/ProductLauncher/IdentityDatabase.sln` | `Database/Identity/` + `Database/Identity-RR/` | Main UL SQL database project |
| `Enterprise/Subsystem/ProductLauncher/UPReporting.sln` | `Database/UPReporting/` | UP Reporting SQL DB |
| `Enterprise/Subsystem/ProductLauncher/Audit.sln` | Local Audit foundation slice | Rare — usually consumed via `Foundation/Audit/Audit.sln` |
| `Enterprise/Subsystem/ProductLauncher/QaAutoGreenBookApiTests.sln` | `AutomationTest/GreenBookApiTests/` | API automation tests |
| `Enterprise/Foundation/Audit/Audit.sln` | `Enterprise/Foundation/Audit/` MVC/WebApi attribute library | Audit foundation lib |
| `Enterprise/Foundation/DataAccess/DataAccess.sln` | `Enterprise/Foundation/DataAccess/` Dapper helpers | DataAccess base |
| `Enterprise/Foundation/Activity/Database/AuditDb.sln`, `Enterprise/Foundation/Activity/DatabaseV2/Audit.Databases.sln` | Activity DBs | Activity DB schema |
| `Activity.sln` (repo root) | Activity DB v2 + activity web/db | Activity surface end-to-end |
| `SSRSReports/Unified Login SSRS Reports.sln` | SSRS reports | Report changes |

### Quick decision rules

- Changed a file under `Component/Landing/`, `Service/LandingAPI*`, or `Service/SharedObjects/` → `MasterProjectSolution.sln`.
- Changed a file under `WinService/BatchProcessor/` → `UnityBatchProcessor.sln`.
- Changed a file under `WinService/UserNotification/` → `UserNotification.sln`.
- Changed a `.sql` file under `Database/Identity*/` → `IdentityDatabase.sln`.
- Changed a `.sql` file under `Database/UPReporting/` → `UPReporting.sln`.
- Changed something in `Enterprise/Foundation/*` → the matching foundation solution (usually `DataAccess.sln` or `Audit.sln`).

If your diff spans **multiple** of the above, build them in series — there is no master solution that captures them all.

## How to build

### Preferred — direct msbuild (handles VS 2017 / 2022)

```powershell
# Locate msbuild (works on either VS version)
$msbuild = (Get-Command msbuild -ErrorAction SilentlyContinue).Source
if (-not $msbuild) {
    # Fallback: try VS 2022 path
    $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    if (-not (Test-Path $msbuild)) {
        $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    }
    if (-not (Test-Path $msbuild)) {
        $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    }
    if (-not (Test-Path $msbuild)) {
        # Last fallback: VS 2017 (the path hard-coded in Build.Net.bat)
        $msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
    }
}

# Restore packages, then build
& $msbuild /t:restore "Enterprise\Subsystem\ProductLauncher\MasterProjectSolution.sln"
& $msbuild "Enterprise\Subsystem\ProductLauncher\MasterProjectSolution.sln" /m /property:Configuration=Debug
```

Substitute the solution path for the slice you are building.

### Using `Build.Net.bat` (only works on VS 2017)

[Build.Net.bat](../../Enterprise/Subsystem/ProductLauncher/Build.Net.bat) `cd`s to `c:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\` before invoking `msbuild`. On a VS 2022 machine this will fail with "directory not found." Two options:

1. **Build from Visual Studio 2022 IDE** — open `MasterProjectSolution.sln` and Ctrl-Shift-B.
2. **Run msbuild directly** as shown above, **not** through `Build.Net.bat`.

Do **not** edit `Build.Net.bat` for personal-path reasons — the CI pipeline still references it. If the user has VS 2022 only, prefer the direct msbuild path or the IDE.

### `BuildAll.bat`

[BuildAll.bat](../../Enterprise/Subsystem/ProductLauncher/BuildAll.bat) just fans out to `Build.Net.bat` + `BuildUI.bat` in separate windows. Same VS 2017 quirk applies. The UI half (`BuildUI.bat`) runs `npm update && grunt clean && grunt` in `Web/identity-ui-src` and `Web/landing-ui-src` — only invoke if your change touched the Angular surface.

## Configurations

| Configuration | When |
|---|---|
| `Debug` | Local iteration, default |
| `Release` | Release-shape verification, rare locally |
| `Fortify` | SAST pipeline only — [fortify-scan-sast-pipeline.yml](../../fortify-scan-sast-pipeline.yml). Don't use locally; build is slower and assemblies are intentionally instrumented |

Pass via `/property:Configuration=Debug` (default in `Build.Net.bat`) or `/p:Configuration=Release`.

## After a successful build — checklist

1. If the build emitted **CS warnings** that look related to your change, fix them — the repo treats some warnings as errors via per-project `<TreatWarningsAsErrors>`.
2. For LandingAPI changes, run the tests (see `add-xunit-test` skill — or `dotnet test Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\LandingAPI.Test.csproj`).
3. For UI changes, also run `npm test` in the affected `Web/*-ui-src` folder.
4. For SQL DB project builds, the `.dacpac` is produced under `Database/Identity/bin/{Configuration}/` — deploy via SqlPackage or the IDE's Publish flow, not by hand-running scripts.

## Important

- **Do not** modify `Build.Net.bat` to switch to VS 2022 unless coordinating with the build pipeline owners — TFS build agents likely still use VS 2017.
- **Do not** use `dotnet build` on `MasterProjectSolution.sln`. This is **.NET Framework 4.8** with classic csproj/packages.config in several places; `dotnet build` will get confused by the legacy projects. Stick with `msbuild`.
- **Do not** build all sibling solutions just to be safe — they have overlapping `bin/` outputs and will trample each other. Build only the slice you changed.
- **Do not** build with `/m` (parallel) the first time after a NuGet restore on a cold cache — race conditions on shared package folders have bitten the team. Run `/t:restore` first as a separate step.
- The repo uses **packages.config** (not PackageReference) in many projects — NuGet restore is invoked via msbuild `/t:restore`, not `nuget.exe restore`. The `Build.Net.bat` script does this implicitly.
