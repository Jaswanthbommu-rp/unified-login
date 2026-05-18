---
name: pre-pr-check
description: Run a pre-PR validation pass on changed work in unified-login-main — build the affected solutions, run LandingAPI.Test, run the ProductEnum↔BlueBookProductConstants lockstep guard, confirm AuthorizeScope on provisioning/export endpoints, and confirm Swagger annotations on new endpoints. Use before opening a PR to catch the rare-but-loud regressions that this repo's CI does catch — but that get expensive to discover late (Fortify SAST queue, build agents busy, reviewer round-trips).
---

# Pre-PR validation

Run this before opening a PR. It catches the four classes of regression that the repo's tests and reviewers will reject:

1. Build / link errors in any affected solution.
2. `LandingAPI.Test` failures (especially the cross-file lockstep tests).
3. Missing `AuthorizeScope` on partner/enterprise routes.
4. Missing Swagger annotations on new endpoints.

Treat this as a sequence — early failures gate later steps.

## Step 1 — Inventory what changed

```powershell
git status
git diff --stat
git diff main...HEAD --stat   # for a topic branch
```

Bucket the changes by affected solution using the table in the [`build-solution`](../build-solution/SKILL.md) skill:

- `Component/Landing/`, `Service/LandingAPI*`, `Service/SharedObjects/` → `MasterProjectSolution.sln`
- `WinService/BatchProcessor/` → `UnityBatchProcessor.sln`
- `WinService/UserNotification/` → `UserNotification.sln`
- `Database/Identity*/` → `IdentityDatabase.sln`
- `Database/UPReporting/` → `UPReporting.sln`
- `Enterprise/Foundation/*` → matching foundation solution

If your diff crosses buckets, build each in sequence.

## Step 2 — Build each affected solution

Use the resolution snippet from the [`build-solution`](../build-solution/SKILL.md) skill (handles VS 2017 / 2022 path differences):

```powershell
$msbuild = (Get-Command msbuild -ErrorAction SilentlyContinue).Source
if (-not $msbuild) {
    foreach ($edition in @("Enterprise","Professional","Community")) {
        $candidate = "C:\Program Files\Microsoft Visual Studio\2022\$edition\MSBuild\Current\Bin\MSBuild.exe"
        if (Test-Path $candidate) { $msbuild = $candidate; break }
    }
    if (-not $msbuild) {
        $msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
    }
}

$sln = "Enterprise\Subsystem\ProductLauncher\MasterProjectSolution.sln"
& $msbuild /t:restore $sln
& $msbuild $sln /m /property:Configuration=Debug
```

**Fail criteria:** any `error CS####`, `error MSB####`, or non-zero exit code. Treat warnings as informational unless the project's csproj has `<TreatWarningsAsErrors>` (a few here do — don't ship new warnings in those projects).

## Step 3 — Run `LandingAPI.Test`

```powershell
dotnet test Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\LandingAPI.Test.csproj
```

For a focused run on the classes you touched:

```powershell
dotnet test Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\LandingAPI.Test.csproj `
  --filter "FullyQualifiedName~Manage{Entity}Tests|FullyQualifiedName~{Entity}ControllerTests"
```

If `dotnet test` won't load the .NET 4.8 assembly, fall back to `vstest.console.exe` from the VS install:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" `
  "Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\bin\Debug\LandingAPI.Test.dll"
```

**Fail criteria:** any red `[Fact]`. Don't merge with a `[Skip(...)]` workaround unless the user explicitly authorizes it.

## Step 4 — `ProductEnum` ↔ `BlueBookProductConstants` lockstep

This is a `LandingAPI.Test` test (covered by Step 3) but worth singling out because it surfaces a specific reflection-driven assertion. The guard lives at [OrganizationTests.cs:1541](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/ControllerTest/OrganizationTests.cs) and iterates every field of both static classes.

If you added or changed a product:

- Verify the new entry appears in **both** [Service/SharedObjects/Enum/ProductEnum.cs](../../Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Enum/ProductEnum.cs) and [Service/SharedObjects/Constants/BlueBookProductConstants.cs](../../Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Constants/BlueBookProductConstants.cs) (same name, matching code).
- If you intentionally added a product **only** to the enum (UI-only, no Books registration), make sure its name appears in the `ignoreProductList` at `OrganizationTests.cs:1554`.
- Targeted run:
  ```powershell
  dotnet test Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\LandingAPI.Test.csproj `
    --filter "FullyQualifiedName~OrganizationTests"
  ```

See the [`add-product-enum-mapping`](../add-product-enum-mapping/SKILL.md) skill for the full pattern.

## Step 5 — `AuthorizeScope` on provisioning/export routes

`Service/LandingAPIEnterprise` endpoints — and any LandingAPI endpoint that does **provisioning** (create user, assign role, push to product) or **export** (bulk pull of users/orgs/personas) — must use `[AuthorizeScope("...")]`, not bare `[Authorize]`. Examples of scopes already in use: `enterpriseapi`, `userinfoapi`, `companyfunctions`.

Find new/modified endpoints in the diff:

```powershell
git diff main...HEAD -- "Enterprise\Subsystem\ProductLauncher\Service\LandingAPI*\Controllers\*.cs"
```

For each new `[Route(...)]` block, confirm:

- It has either `[Authorize]` (end-user) **or** `[AuthorizeScope("...")]` (partner / provisioning / export).
- LandingAPIEnterprise endpoints always use `AuthorizeScope`.
- Bulk endpoints (`/bulk/...`, `/export/...`, `/provision/...`) always use `AuthorizeScope` even if served from LandingAPI.
- The scope string matches an existing scope in sibling controllers — don't invent a new scope without confirming IdentityServer config in `unified-login-core` accepts it.

```powershell
# Quick visual: every Route in the diff with its preceding attributes
git diff main...HEAD -U10 -- "**/Controllers/*.cs" | Select-String -Pattern "Authorize|Route|HttpGet|HttpPost|HttpPut|HttpDelete"
```

**Fail criteria:** a new provisioning/export route with bare `[Authorize]` (or no auth attribute at all).

## Step 6 — Swagger annotations on new endpoints

For each new endpoint, confirm:

- A `[SwaggerResponse(HttpStatusCode.OK, ...)]` with `Type = typeof(...)`.
- A `[SwaggerResponse(HttpStatusCode.BadRequest, ...)]`.
- A `[SwaggerResponse(HttpStatusCode.Unauthorized, ...)]`.
- A `[SwaggerResponse(HttpStatusCode.InternalServerError, ...)]`.
- For DTOs with non-trivial shape: a `[SwaggerResponseExamples(typeof(MyDto), typeof(MyDtoExample))]` and the matching example provider class.
- XML doc comment on the method (Swashbuckle picks this up).

```powershell
git diff main...HEAD -U5 -- "**/Controllers/*.cs" | Select-String -Pattern "Route|SwaggerResponse"
```

After build, sanity-check Swagger renders at `https://www-local2.realpage.com/api/swagger/ui/index` (or `/apienterprise/swagger/...`) and your endpoint appears.

**Fail criteria:** an endpoint with no Swagger annotations or with only a partial set.

## Step 7 — Serilog enrichment

For any new log line, confirm it goes through a `WriteToLog` helper that attaches `CorrelationId`, `ProductModule`, `AdditionalInfo` (see [`add-serilog-context`](../add-serilog-context/SKILL.md)). Quick sniff:

```powershell
git diff main...HEAD -U2 -- "**/*.cs" | Select-String -Pattern "Log\.|WriteToLog|ForContext"
```

If you see `Log.Information(...)` / `Log.Error(...)` without `ForContext("CorrelationId"...)` nearby, the line will be invisible in Kibana — route it through `WriteToLog`.

## Step 8 — Diff hygiene

```powershell
git status
git diff --stat
```

Confirm:

- No accidentally committed `bin/`, `obj/`, `*.user`, `.vs/`, `packages/` folders.
- No secrets (`.config` files with real connection strings or LD SDK keys — these belong in environment-specific configs, not source).
- No unrelated whitespace churn (tabs ↔ spaces in files the rest of the team did not touch).
- `AssemblyInfo.cs` version bump if your team's convention requires it (this repo has assembly-version files that are sometimes manually bumped per-release).

## Step 9 — UI changes (if any)

If your diff includes `Web/identity-ui-src/` or `Web/landing-ui-src/`:

```powershell
Push-Location Enterprise\Subsystem\ProductLauncher\Web\landing-ui-src
npm update
& "$env:APPDATA\npm\grunt.cmd" clean
& "$env:APPDATA\npm\grunt.cmd"
Pop-Location
```

(Match the `BuildUI.bat` invocation.) Run UI changes through a browser at the local site set up by `WebSiteSetup-wwwlocal.ps1` — the type checker and Karma tests don't catch every visual regression.

## Step 10 — Final summary for the PR description

Run a last:

```powershell
git diff main...HEAD --stat
git log main...HEAD --oneline
```

Use the result to fill out the PR template. Mention any of:

- New `ProductEnum` entry (referenced TFS work item / Books ticket).
- New `AuthorizeScope` (referenced IdentityServer ticket).
- New LaunchDarkly flag (referenced LD project / flag key).
- New `ProductInternalSetting` row (referenced DB seed change in `IdentityDatabase.sln`).

## Important

- **Never skip Step 4 if you touched any file under `Service/SharedObjects/Enum/` or `Service/SharedObjects/Constants/`.** The lockstep guard fires deep in the suite; failing fast saves time.
- **Never claim "ready for review" with `[Skip]`'d tests.** If a test is genuinely obsolete, delete it in a separate commit and explain.
- **Never run `git reset --hard` or `git clean -fdx`** to "clean up" before opening the PR — you will lose untracked work and the stash is not automatic.
- **Don't push** at the end of this skill. Surface the result to the user (`build: pass`, `tests: pass`, `auth: needs scope on /api/foo`, etc.) and let them decide. If they confirm, follow the [`commit`](../commit/) and `describe_pr` flows next.
- **CI catches these too**, but the SAST (Fortify) leg can run 30+ minutes and the build agents are shared — finding failures locally is dramatically faster than discovering them in TFS.
