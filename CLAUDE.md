# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Role

`unified-login-main` is the **.NET Framework 4.8** half of the Unified Login product. It is one of four sibling repos:

- `unified-login-main` (this repo) — .NET 4.8 APIs, web, services, **and all SQL database projects**
- `unified-login-core` — ASP.NET Core 8.0 (IdentityServer / Duende)
- `unified-login-coreapi` — ASP.NET Core 6.0 APIs (`UnifiedLogin.Api`, `UnifiedLogin.ApiEnterprise`)
- `unified-login-landing` — Angular 16 UI

Local IIS routes work cross-repo: `wwwlocal2/apicore` → coreapi, `wwwlocal/login` → core. When making changes, be aware the runtime app spans all four repos.

Compliance: **PCI, SOX** (per `.realpage`). Production branches are `master` and `release`.

## Solutions and What They Build

All solutions live under [Enterprise/Subsystem/ProductLauncher/](Enterprise/Subsystem/ProductLauncher/) unless noted. There is no single "build everything" solution — pick the one matching your change:

| Solution | Builds |
|---|---|
| `MasterProjectSolution.sln` | Landing API, Landing Enterprise API, Web/Landing MVC, Component.Landing, SharedObjects, DataAccess, LandingAPI.Test (the primary solution for API/web work) |
| `UnityBatchProcessor.sln` | Batch processor Windows service ([WinService/BatchProcessor/](Enterprise/Subsystem/ProductLauncher/WinService/BatchProcessor/)) |
| `UserNotification.sln` | User notification Windows service ([WinService/UserNotification/](Enterprise/Subsystem/ProductLauncher/WinService/UserNotification/)) |
| `IdentityDatabase.sln` | Main UL SQL DB project ([Database/Identity/](Enterprise/Subsystem/ProductLauncher/Database/Identity/) and `Identity-RR`) |
| `UPReporting.sln` | UP Reporting SQL DB ([Database/UPReporting/](Enterprise/Subsystem/ProductLauncher/Database/UPReporting/)) |
| `Audit.sln` | Audit foundation libraries |
| `QaAutoGreenBookApiTests.sln` | API automation tests ([AutomationTest/GreenBookApiTests/](Enterprise/Subsystem/ProductLauncher/AutomationTest/GreenBookApiTests/)) |
| `Activity.sln` (repo root) | Activity DB v2 + activity web/db |
| `SSRSReports/Unified Login SSRS Reports.sln` | SSRS report project |

Solution `Configuration` includes `Debug`, `Release`, and a `Fortify` configuration used by the SAST pipeline.

## Build / Run / Test Commands

All commands run from [Enterprise/Subsystem/ProductLauncher/](Enterprise/Subsystem/ProductLauncher/) unless noted.

- **Full build (.NET + UI in parallel):** `BuildAll.bat` — fans out to `Build.Net.bat` and `BuildUI.bat`.
- **.NET only:** `Build.Net.bat` — runs `msbuild /t:restore` then `msbuild MasterProjectSolution.sln /m /property:Configuration=Debug`. The script hard-codes the VS 2017 MSBuild path; on a VS 2022 machine, build from the IDE or substitute the correct `msbuild.exe`.
- **UI only:** `BuildUI.bat` — runs `npm update && grunt clean && grunt` in `Web/identity-ui-src` and `Web/landing-ui-src`.
- **Local site setup (Docker):** `&'WebSiteSetup-wwwlocal-docker.ps1'` (admin PowerShell, after `Set-ExecutionPolicy RemoteSigned`).
- **Local site setup (IIS):** `&'WebSiteSetup-wwwlocal.ps1'` — provisions certs and IIS sites for `wwwlocal`/`wwwlocal2`.
- **Tests:** Open `MasterProjectSolution.sln` in VS → Test Explorer → run [Service/LandingAPI/LandingAPI.Test/](Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/). Stack: **xUnit + Moq**. To run a single test from the CLI: `dotnet test path\to\LandingAPI.Test.csproj --filter "FullyQualifiedName~SomeTestName"` (or use `vstest.console.exe` for the .NET 4.8 assembly).
- **Local Swagger:** `https://www-local2.realpage.com/api/swagger/ui/index` (and `/apienterprise/swagger/...`). Other env URLs are in [README.md](README.md).

## Code Architecture

### Layered architecture (per request flow)

1. **API layer** — `Service/LandingAPI/Controllers/` and `Service/LandingAPIEnterprise/Controllers/`. ASP.NET Web API 2 controllers. Decorated with `Authorize`/`AuthorizeScope`; claims are surfaced into `DefaultUserClaim` (UserId, CorrelationId, OrganizationRealPageGuid, CustomerMasterId).
2. **Logic layer** — [Component/Landing/Logic/](Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/). Orchestration classes named `Manage{Entity}` paired with `IManage{Entity}` (e.g., `ManageOrganization`, `ManageUser`, `ManageUnifiedSettings`). Per-product logic is `ManageProduct{ProductName}` (OneSite, AssetOptimization, MarketingCenter, ResidentPortal, RealConnect, Ops, Panel, OneSiteAccounting, Rum, Intelligent Building, etc.).
3. **Repository layer** — `Component/Landing/Repository/`. Dapper + stored procs (names centralized in `StoredProcNameConstants`) and external HTTP clients built over `HttpMessageHandler` (Books API, Translate API, Settings API). Use TVPs for bulk ops.
4. **Shared contracts** — `Service/SharedObjects/` (DTOs, enums, constants, helpers like `EmailFormatValidation`). `ProductEnum` and `BlueBookProductConstants` mappings have test coverage that enforces consistency — keep them in lockstep.

### DI and testability

Constructor injection throughout. Controllers expose **test constructors** that accept mocks for `IRepository`, `HttpMessageHandler`, `ILdClient` (LaunchDarkly), and `DefaultUserClaim`. Tests mock SP parameters via `It.Is<object>(predicate)` and use `RPObjectCache.BustCache()` to reset cached SP calls between assertions.

### Adjacent / supporting projects

- [Enterprise/Foundation/DataAccess/](Enterprise/Foundation/DataAccess/) — base data-access library (Dapper helpers, SP execution).
- [Enterprise/Foundation/Activity/Service/Command/](Enterprise/Foundation/Activity/Service/Command/) — activity logging command library.
- [Enterprise/Foundation/Audit/](Enterprise/Foundation/Audit/) — audit MVC/WebApi attribute library.
- [WinService/BatchProcessor/](Enterprise/Subsystem/ProductLauncher/WinService/BatchProcessor/) — long-running batch service. `BatchProcessor` logic lives in `Component/Landing/Logic/BatchProcessor/`.
- [WinService/UserNotification/](Enterprise/Subsystem/ProductLauncher/WinService/UserNotification/) — Kafka-driven notification service.

### Operational integrations (where to look first)

- **Auth:** Identity Server 7 (lives in `unified-login-core`, but APIs here validate tokens). Provider type lookup and SAML are in `Service/LandingAPI`.
- **Feature flags:** LaunchDarkly via `ILdClient` injected into controllers/logic.
- **Messaging:** Kafka (`librdkafka.redist` packages — both 1.5.0 and 2.0.2 are present).
- **Email:** Toggleable between Unified Email and SendGrid via `ProductInternalSetting` (`IsSendGridEnabled`, `IsUnifiedEmailEnabled`). SendGrid Event Webhook endpoint is `/api/sendgrid/events` and verifies `X-Twilio-Email-Event-Webhook-Signature` against a configured public key.
- **Logging:** RealPage Serilog. Always attach contextual properties: `CorrelationId`, `ProductModule`, `AdditionalInfo`. Communication events persist via `IManageCommunicationEvents`. Central Kibana queries are tagged `ULE`.
- **Caching:** `RedisCacheService` in `Component/Landing`.

## Conventions

- **C# 13 syntax** is used where compatible with .NET 4.8 — `var` for obvious types, `async`/`await` for I/O.
- **Single namespace per file**, XML docs on public controller and logic methods.
- **Naming:** `{Entity}Controller`, `Manage{Entity}` + `IManage{Entity}`, `{Entity}Repository` + `I{Entity}Repository`. Product integrations: `ManageProduct{ProductName}` and `ProductIntegration/Types/{Legacy|StandardV1|UPFM}`.
- **Routes** consistently under `/api`. Use `SwaggerOperation`/`SwaggerResponse` annotations and example providers.
- **Validate inputs** — guard against default/empty `Guid`s; apply `AuthorizeScope` to provisioning/export operations.
- **CORS** is configured via `AllowCors` attributes, not globally.

For the full house style on patterns, mock setup, ProductInternalSetting toggles, and per-product behavior, see [.github/copilot-instructions.md](.github/copilot-instructions.md) — it is treated as authoritative for this repo and should not be duplicated here.

## CI / Pipelines

Build/release pipelines are in TFS (links in [README.md](README.md)). YAML steps for DB and Activity deployments live in [.azure-pipelines/](.azure-pipelines/). The Fortify SAST pipeline is driven by [fortify-scan-sast-pipeline.yml](fortify-scan-sast-pipeline.yml) and uses the `Fortify` build configuration.
