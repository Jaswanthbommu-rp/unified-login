# Build / Test / Perf Commands

## Repository: Jaswanthbommu-rp/unified-login

This is the .NET Framework 4.8 half of Unified Login. The Linux GitHub Actions runner CANNOT build it.

## What works on the Linux perf-improver runner

| Tool | Status | Notes |
|---|---|---|
| `git` operations | works | repo cloned, master fetched on demand |
| `dotnet` SDKs 8/9/10 | installed | useless for .NET 4.8 targets — no mono either |
| Static code analysis (Grep/Read) | works | how we find opportunities |
| `safeoutputs create_pull_request` | works | only when HEAD is ≤ N commits ahead of GITHUB_SHA (see workflow_quirks) |

## What does NOT work on Linux runner

- `msbuild` for .NET Framework 4.8 — no `mono` installed and `dotnet build` cannot target net48
- `Build.Net.bat` — hard-codes `c:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\` (Windows-only, and even on Windows VS 2017 path is stale per CLAUDE.md)
- `BuildUI.bat` — depends on `grunt`/`npm` (could work but UI changes weren't in scope)
- xUnit tests in `LandingAPI.Test` — require the .NET 4.8 build to produce assemblies first

## Windows-only commands maintainers should run before merging perf-improver PRs

```
# From Enterprise/Subsystem/ProductLauncher/
1. Open MasterProjectSolution.sln in VS 2022 (substituting MSBuild 17 path if using Build.Net.bat)
2. Build → Build Solution (Debug)
3. Test Explorer → Run All in Service/LandingAPI/LandingAPI.Test
```

## Solutions, per CLAUDE.md

- `MasterProjectSolution.sln` — Landing API, Enterprise API, Web, Logic, Shared, Tests (primary)
- `UnityBatchProcessor.sln` — batch service
- `UserNotification.sln` — notification service
- `IdentityDatabase.sln` — main DB project
- `UPReporting.sln` — UP Reporting DB
- `Audit.sln`, `QaAutoGreenBookApiTests.sln`, `Activity.sln`, `SSRSReports/...sln`

There is no "build everything" solution.
