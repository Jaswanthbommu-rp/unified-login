---
date: 2026-05-11T00:00:00-05:00
researcher: Jaswanth Bommu
git_commit: 8b2ea3b4c49008617a163cbee35e23375a08a2ce
branch: master
repository: unified-login-main
topic: "Unified Login System Architecture"
tags: [research, codebase, architecture, landing-api, batch-processor, user-notification, identity-server, kafka, sendgrid, serilog, redis, dapper, sql-server]
status: complete
last_updated: 2026-05-11
last_updated_by: Jaswanth Bommu
---

# Research: Unified Login System Architecture

**Date**: 2026-05-11
**Researcher**: Jaswanth Bommu
**Git Commit**: 8b2ea3b4c49008617a163cbee35e23375a08a2ce
**Branch**: master
**Repository**: unified-login-main (TFS: `tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main`)

## Research Question
Give me the unified login system architecture — the overall shape of the system, the layers and projects in this repo, how they connect to the sibling repos, and the major integrations.

## Summary

Unified Login is a multi-tenant identity and product-provisioning platform spread across **four sibling repositories**, with `unified-login-main` (this repo, .NET Framework 4.8) acting as the **product/persona/organization brain and the database master**. The other three are `unified-login-core` (Duende IdentityServer 7, ASP.NET Core 8), `unified-login-coreapi` (ASP.NET Core 6 APIs), and `unified-login-landing` (Angular 16 UI).

Inside this repo, the architecture is a classic **N-tier OWIN/Web API 2 stack**:

1. **Two Web APIs** — `LandingAPI` (60 controllers under `/api`, scope `rplandingapi`) and `LandingAPIEnterprise` (6 controllers under `/apienterprise`, scope `enterpriseapi`). Both validate IdentityServer bearer tokens; claims flow through `DefaultUserClaim`.
2. **One MVC web host** — `Web/Landing/` that serves a pre-built Angular 16 SPA from `unified-login-landing` plus a handful of server-rendered MVC controllers.
3. **One business assembly** — `Component.Landing.dll` containing ~50 `Manage{Entity}` logic classes (the orchestration layer), ~89 Dapper repository classes (stored-proc-only), a `ProductIntegration` framework with three integration types (`Legacy`, `StandardV1`, `UPFM`), and a Kafka producer for user-status events.
4. **Two Windows services** — `BatchProcessor` (6 parallel SQL-polling loops, fan-out via `Parallel.ForEach` to HTTP APIs) and `UserNotification` (single timer, three scheduled jobs: future logins, pending users, expiring users).
5. **Four SQL Server database projects** — `Identity` (the main UL DB with schemas `Ident`, `Enterprise`, `Person`, `Batch`, `Security`, `Settings`, `CustomField`, `Hots`, `Maintenance`), `Identity-RR` (read-replica variant), `UPReporting` (audit reporting), and `Migration`. Plus an MSMQ-fed `Activity` DB for activity logging.
6. **Foundation libraries** — `DataAccess` (Dapper wrapper with UnitOfWork + TVP support), `Audit` (perf filters), `Activity` (MSMQ → WCF → SQL pipeline).

External integrations: IdentityServer 7 (bearer token validation), LaunchDarkly (feature flags), SendGrid + Unified Email + legacy CES (three email paths), Kafka (`UserStatus` topic producer; Serilog sink), Redis (cache wrapper, defined but lightly used), Books/UDM API (`ManageBlueBook`), Settings/UPFM API (`ManageUnifiedSettings`), MSMQ + WCF for activity logging, Elastic APM, Serilog → Elasticsearch + Kafka.

## Detailed Findings

### A. Repository & Solution Topology

This repo contains **14 solution files**. There is no "build everything" solution — each owns a slice:

| Solution | What it builds |
|---|---|
| `Enterprise/Subsystem/ProductLauncher/MasterProjectSolution.sln` | Primary: LandingAPI, LandingAPIEnterprise, Web/Landing, Component.Landing, SharedObjects, LandingAPI.Test |
| `Enterprise/Subsystem/ProductLauncher/UnityBatchProcessor.sln` | BatchProcessor Windows service |
| `Enterprise/Subsystem/ProductLauncher/UserNotification.sln` | UserNotification Windows service |
| `Enterprise/Subsystem/ProductLauncher/IdentityDatabase.sln` | Identity SQL DB |
| `Enterprise/Subsystem/ProductLauncher/Database/Identity-RR/Identity-RR.sln` | Identity read-replica DB |
| `Enterprise/Subsystem/ProductLauncher/UPReporting.sln` | UPReporting SQL DB |
| `Enterprise/Subsystem/ProductLauncher/Audit.sln` | Audit attributes for product-launcher |
| `Enterprise/Subsystem/ProductLauncher/QaAutoGreenBookApiTests.sln` | GreenBook API automation |
| `Activity.sln` (repo root) | Activity DB v2 + foundation libs |
| `Enterprise/Foundation/Activity/Database/AuditDb.sln` | Legacy Audit SQL DB |
| `Enterprise/Foundation/Activity/DatabaseV2/Audit.Databases.sln` | AuditArchiveDB + AuditDBV2 |
| `Enterprise/Foundation/Audit/Audit.sln` | Audit.Core/MvcWeb/WebApi libs |
| `Enterprise/Foundation/DataAccess/DataAccess.sln` | Dapper data-access lib |
| `SSRSReports/Unified Login SSRS Reports.sln` | SSRS reports (single .rdl: `PlatformUsers.rdl`) |

Top-level directories under `Enterprise/Subsystem/ProductLauncher/`: `Service/` (APIs + SharedObjects + tests), `Component/` (single `Landing.csproj` for logic + repositories), `Web/Landing/` (MVC host), `WinService/` (BatchProcessor + UserNotification), `Database/` (Identity, Identity-RR, UPReporting, MigrationTool), `AutomationTest/GreenBookApiTests/`.

### B. API Layer (LandingAPI + LandingAPIEnterprise)

**Hosting model.** Both APIs are OWIN-hosted ASP.NET Web API 2 apps. Bearer-token middleware comes from `RealPage.IdentityServer4.AccessTokenValidation`:

```
app.UseIdentityServerBearerTokenAuthentication(...)
  Authority = ConfigReader.GetIssuerUri   // /login/identity
  RequiredScopes = "userinfoapi rplandingapi companyfunctions"
  DelayLoadMetadata = true
  EnableValidationResultCache = true
```

Wired in `Service/LandingAPI/Startup.cs:33-45` and `Service/LandingAPIEnterprise/Startup.cs:29-39`. Both base controllers carry the class-level scope guard:

- `Service/LandingAPI/BaseApiController.cs:24` — `[AllowCors("LandingAPICORSAllowedOrigins"), AuthorizeScope("rplandingapi")]`
- `Service/LandingAPIEnterprise/BaseApiController.cs:24` — `[AllowCors("LandingAPICORSAllowedOrigins"), AuthorizeScope("enterpriseapi")]`

Specific endpoints stack on additional scopes such as `userinfoapi`, `companyfunctions`, `migrationapi`. The scope check itself is in `Component/Landing/Attributes/AuthorizeScopeAttribute.cs:55-112`, which extends `AuthorizeAttribute`, iterates `ClaimsPrincipal.Current.Claims` for `Scope` claims (case-insensitive), returns 403 on scope mismatch, 401 on missing auth.

**Controller inventory.** LandingAPI has 60 controllers split into:
- **Core entities**: `AccessController`, `UserController`, `UserLoginController`, `PersonController`, `PersonaController`, `ProfileController`, `OrganizationController`, `CredentialController`, `MultiFactorAuthController`, `PasswordPolicyController`, `SamlController`, `UnifiedLoginController`, `UnifiedSettingsController`, contact/address/lookup controllers, etc.
- **Per-product** (flat): `ProductOneSiteAccountingController`, `ProductMarketingCenterController`, `ProductResidentPortalController`, `ProductRumController`, `ProductLead2LeaseController`, `ProductLearningPortalController`, `ProductOmniChannelController`, `ProductClientPortalController`, `ProductAdminSupportPortalController`, `ProductRentersInsuranceController`, `ProductIntegrationMarketplaceController`, `ProductProspectContactController`, `ProductEasyLMSController`, `ProductVendorServicesController`, `ProductUserController`.
- **Per-product** (`Controllers/Product/` subfolder): `ProductOneSiteController`, `ProductOnSiteController`, `ProductAssetOptimizationController`, `ProductOpsController`, `ProductPanelController`, `ProductRPDMController`, `ProductInvokerController`, `ResearchApplicationController`, `UnifiedAmenitiesController`.
- **Webhooks**: `WebHookController.cs` exposes `POST webhook/books` (Books/Tibco), `[AllowAnonymous]` at line 100, with HMAC-SHA256 signature verification (lines 120-140) using the raw body captured by `TibcoRequestHandler` (`Service/SharedObjects/Handlers/TibcoRequestHandler.cs:23`) into `request.Properties["TibcoPostData"]`. **Note**: the SendGrid Event Webhook endpoint `/api/sendgrid/events` referenced in CLAUDE.md is NOT present in this checkout — only `EmailController.cs:36 POST /sendemail` (outbound send) exists here.

LandingAPIEnterprise has 6 controllers: `UserController` (~1,400 LOC — heaviest), `PropertyController`, `RoleController`, `ShellController` (`[RoutePrefix("shell")]`), `ProductController`, `HotsUserCloneController`.

**Routing.** Pure attribute routing — `config.MapHttpAttributeRoutes()` in both `WebApiConfig.cs`. No conventional routes, no API versioning (declared `SingleApiVersion("v1", …)` in Swagger only).

**Claims → `DefaultUserClaim`.** `Service/LandingAPI/BaseApiController.cs:84-155` reads `ClaimsPrincipal.Current` in `Initialize()`. For machine-to-machine tokens carrying a `client_info` claim (lines 91-128), the controller looks up the person/persona/roles from the DB and synthesizes additional claims (`sub`, `orgPartyId`, `ORGID`, `LOGINNAME`, `PERSONAID`, `roleid`) onto the identity before constructing `new DefaultUserClaim(currentClaimPrincipal)` at line 130. `Service/SharedObjects/Landing/DefaultUserClaim.cs:28-73` then surfaces: `UserId`, `OrganizationPartyId`, `LoginName`, `OrganizationMasterId`, `CustomerMasterId`, `UserRealPageGuid`, `OrganizationRealPageGuid`, `CorrelationId` (auto-generated if absent, line 56), `PersonaId`, `RealPageEmployee` (true if org GUID equals the hardcoded employee company GUID, line 64), `ImpersonatedBy`/`ImpersonatedByName`.

**CORS.** Two-layer: a global wildcard `new EnableCorsAttribute("*", "*", "*")` at `Service/LandingAPI/Startup.cs:60-61` acts as backstop; the real enforcement comes from `AllowCorsAttribute.cs` which is an `ICorsPolicyProvider` that pulls allowed origins from the DB setting `LANDINGAPICORSALLOWEDORIGINS` (cached 5 min) and sets `AllowAnyOrigin = false`, `SupportsCredentials = true`.

**DI.** No IoC container. The "DI" pattern is **constructor overloading**: a production parameterless ctor that news up concrete dependencies in `Initialize()`, plus a test ctor accepting `IRepository`, `DefaultUserClaim`, and `HttpMessageHandler` mocks. Examples: `WebHookController` line 44-49, `ShellController` line 45, `LandingAPIEnterprise/BaseApiController` line 98.

**Cross-cutting filters.** Global `IExceptionHandler` (`Service/SharedObjects/Handlers/ApiExceptionHandler.cs:33`) shapes 500 responses with a reference ID. Global `IExceptionLogger` (`Service/SharedObjects/Exceptions/ApiExceptionLogger.cs:16-50`) writes to Serilog with `AdditionalInfo`, `ProductModule`, `CorrelationId` context. `NoCacheHandler` + `TibcoRequestHandler` are registered as `MessageHandlers` in LandingAPI only. Elastic APM is bootstrapped in `Global.asax.cs:17-19`.

**Swagger.** Registered only when `ConfigReader.Environment != "PROD"` (`Service/LandingAPI/Startup.cs:67`). OAuth2 implicit flow with scopes `rplandingapi`, `userinfoapi`, `unifiedsettingsapi`, `companyfunctions` for LandingAPI; `userinfoapi`, `enterpriseapi` for LandingAPIEnterprise. XML docs from all `RP.Enterprise*.xml` files in `\bin`.

### C. Logic Layer (`Component/Landing/Logic/`)

**The `Manage{Entity}` pattern.** ~50 classes implementing `IManage{Entity}`. Three-constructor convention: production ctor `(DefaultUserClaim)`, test ctor v1 (repository interfaces), test ctor v2 (`IRepository + HttpMessageHandler`). `DefaultUserClaim` is injected everywhere and carries `CorrelationId`, `UserId`, `OrganizationRealPageGuid`, `CustomerMasterId`. No transactions at this layer — each repository call opens/closes its own DB scope.

Major Manage classes (selection):

| Class | File | Domain |
|---|---|---|
| `ManageUser` | `Logic/ManageUser.cs` | User CRUD, validation, clone, registration email, delegate admin |
| `ManageOrganization` | `Logic/ManageOrganization.cs` | Company provisioning, product panel, Blue Book integration |
| `ManageUnifiedSettings` | `Logic/ManageUnifiedSettings.cs` | UPFM/Settings API client with bearer-token auth |
| `ManageBlueBook` | `Logic/ManageBlueBook.cs` | Books/UDM API HTTP client |
| `ManageEmail` | `Logic/ManageEmail.cs` | SendGrid + Unified Email + legacy CES dispatch |
| `ManageUserRegistrationEmail` | `Logic/ManageUserRegistrationEmail.cs` | Routes registration emails by toggle |
| `ManageBulkUsers` / `ManageBulkUserBatch` | `Logic/ManageBulkUser*.cs` | Bulk import orchestration |
| `ManageCommunicationEvents` | `Logic/ManageCommunicationEvents.cs` | Communication/audit event persistence |
| `ManageSaml` | `Logic/ManageSaml.cs` | SAML IDP config |
| `ManageSecuritySettings` | `Logic/ManageSecuritySettings.cs` | Password policy, 2FA |

**Per-product orchestrators** (`Logic/Product/`) — one per product, all extending `ManageProductBase` (`Logic/Product/ManageProductBase.cs:33`, holding protected fields for editor/user persona, product ID/URL, product usernames, shared repositories). Examples: `ManageProductOneSite`, `ManageProductOneSiteAccounting`, `ManageProductAssetOptimization`, `ManageProductMarketingCenter`, `ManageProductResidentPortal`, `ManageProductRealConnect`, `ManageProductOps`, `ManageProductPanel`, `ManageProductRum`, `ManageIntelligentBuilding`, `ManageProductAdminSupportPortal`, `ManageProductClientPortal`, `ManageProductVendorServices`, `ManageProductEasyLMS`, `ManageProductOmniChannel`, `ManageProductProspectContact`, `ManageUPFMProductsIntegration`.

**ProductIntegration framework** (`Logic/ProductIntegration/`):

- Contract: `Types/IIntegrationType.cs` — methods `GetProperties`, `GetRoles`, `GetRightsForRole`, `GetPropertyGroups`, `CreateUser`, `ChangeUserType`, `UpdateUserProfile`, `UpdateUserDetails`, `GetMigrationUsers`, `ExternalUserProfileChange`, `GetUserGroups`.
- Three implementations:
  - `LegacyIntegrationType` (`Types/LegacyIntegrationType.cs:33`) — dispatches via `switch(_productId)` to product-specific Manage classes (OneSite & legacy).
  - `StandardV1IntegrationType` (`Types/StandardV1IntegrationType.cs:16`) — delegates to `StandardV1ProductIntegration`, a generic implementation for modern products.
  - `UPFMIntegrationType` (`Types/UPFMIntegrationType.cs:18`) — delegates to `ManageUPFMProductsIntegration` / `UPFMProductIntegration` for UPFM-family products.
- Factory: `Factory/IntegrationTypeFactory.cs:13` holds a static `Dictionary<ProductIntegrationTypeEnum, FactoryInitMethod>` (lines 43-50). `GetIntegration(productId)` reads `ProductIntegrationType` setting from the DB, maps to enum, instantiates via factory delegate.

**Batch processor logic library** (`Logic/BatchProcessor/`):
- `BatchProcessorLogic.cs`
- Process implementations: `ChangeProductUserType`, `CreateUpdateProductUser`, `DeactivateProductUser`, `EnterpriseCreateUpdateProductUser`, `UpdateProductUserProfile`
- Factory pattern: `Factory/ProcessExecutionFactory.cs`, `Factory/ProductFactory.cs` with `IProcessExecution` + `IProduct` contracts.

**Messaging** (`Logic/Messaging/`):
- `KafkaConfiguration.cs`, `KafkaProducerBase.cs`, `KafkaProducerServiceFactory.cs`, `UserStatusKafkaProducer.cs`, `IKafkaProducerService.cs`
- Producer (not consumer) — UnifiedLogin publishes UserStatus events; downstream consumers live elsewhere.

**Security** (`Logic/Security/ManageSecurity.cs`), **helpers** (`Logic/Helper/BatchHelper.cs`), and **Enterprise** sub-area (`Logic/Enterprise/User/UserManagement.cs`) round out the package.

### D. Repository Layer (`Component/Landing/Repository/`)

~89 repository classes, all inheriting `BaseRepository` (`Repository/BaseRepository.cs`). The `GetRepository()` method at line 32 either returns the injected mock `IRepository` (test mode) or constructs `DapperUnitOfWork` + `DapperRepository` over a connection string named by `dbConnectionEnum` (line 41).

Every repository call follows the pattern:

```csharp
using (var repo = GetRepository())
{
    return repo.GetOne<SO.User>(StoredProcNameConstants.SP_GetUserByLoginId, new { loginid = enterpriseUserName });
}
```

All SPs are referenced by constants from `Service/SharedObjects/Constants/StoredProcNameConstants.cs` (~150 constants in `StoredProcNameConstants`, plus 8 in `EnterpriseStoredProcNameConstants`). SP names follow `Schema.ProcedureName[_VerN]` — versioned suffixes appear when SPs are kept side-by-side for backward compatibility (e.g., `Enterprise.GetOrganization_Ver03`, `Person.ListPersons_Ver04`).

**TVPs** (`TableValueParamHelper.ConvertToTableValuedParameter`) for bulk ops:
- `PropertyRepository.cs:143` — `Enterprise.PropertyInstanceType` for bulk UPFM property lookup
- `UnifiedLoginRepository.cs:148` — `enterprise.productidtype` for role/rights listing
- `UserRepository.cs:2926, 2959` — `Enterprise.BigIntListType` for user ID lists

**External HTTP clients** — there is no dedicated "Books client" or "Settings client" class. Instead:
- `ManageBlueBook.cs:54` holds `readonly HttpClient _httpClient` directly. Base URL from `ProductInternalSetting` keys `BlueBookAPIEndPoint` or via Kong (`UDMViaKong` flag, lines 87-100). Test mode uses `new HttpClient(messageHandler, false) { BaseAddress = new Uri("http://localhost") }`.
- `ManageUnifiedSettings.cs:34, 49-70` — same pattern; adds Bearer token from `ITokenHelper.GetToken()`.
- `Logic/ProductIntegration/Helpers/ApiIntegration.cs:11-26` — thin generic wrapper used by per-product orchestrators, with `GetEntityFromApi<T>`, `PostEntity`, `PutEntity`, `DeleteEntity`, `PatchEntity` (all `.Result`-blocking sync).

**Caching at the repository level** — `RPObjectCache` (in-process `MemoryCache`). Pattern: `RPObjectCache.GetFromCache<T>(key, expirySeconds, () => SP call)`. Examples:
- `OrganizationRepository.cs:338-341` — `GetOrganizationAdminUserRealPageId`, 180s
- `OrganizationRepository.cs:514-517` — `GetProductsByOrganization`, 180s
- `ConfigurationSettingRepository.cs:72-75` — org configuration settings, 360s
- `ManageBlueBook.cs` — ~20 sites, 300s (`CacheTimeSeconds = 300`)

Tests bust this cache via `RPObjectCache.BustCache()`.

### E. Windows Services

**BatchProcessor** (`WinService/BatchProcessor/`) — inherits `ServiceBase` directly (no Topshelf). `Program.cs:21` wires Serilog (`SerilogHelpers.ConfigureSerilog("Unified Login")`), Elastic APM, then `ServiceBase.Run(productAssignService)`.

`OnStart` (`BatchProcessorService.cs:68-111`) spawns **six parallel `LongRunning` tasks**, each a SQL-polling loop:

| Task | Driver SP | Purpose |
|---|---|---|
| `RunPendingProcess` | `Batch.ListBatchProcessor` | Pending product-assign records |
| `RunRetryProcess` | Same SP, `isRetry=true` | Retry failed records |
| `RunEnterpriseRoleUpdateProcess` | `Batch.ListEnterpriseRoleBatchProcessor` | Enterprise role → product role sync |
| `RunPrimaryPropertiesUpdateProcess` | `Batch.ListPrimaryPropertiesBatchProcessor` | Primary property updates |
| `RunBulkUserUpdateProcess` | `Batch.ListBulkUserBatchProcessor` | Bulk user updates |
| `RunCompanyAndPropertiesUpdateProcess` | `Batch.ListCompanyBatchData` | Company + property updates |

Each loop sleeps via `CancellationToken.WaitHandle.WaitOne(interval)` (`BatchProcessorService.cs:136`) — `PollingInterval` on success, `ExceptionWaitInterval` on error, `RetryPollingInterval` for the retry loop. Records are processed with `Parallel.ForEach` capped by `ThreadCount` from `ProductInternalSettings`. Each record dispatches to `Helper/ApiCaller.cs::ProductApiCaller.ProcessBatchRecord`, an HTTP POST to a URL looked up from `MemoryCache["batch_configs"]` (populated from `Batch.ListBatchProcessConfiguration`, cached 60 min). Status writes go through `Enterprise.UpdateProductBatch` / `Batch.UpdateEnterpriseRoleProductBatch`.

Four of six loops short-circuit on LaunchDarkly flag `use-core-api-v2-for-service` (`BatchProcessorService.cs:275`) — Pending and Retry do not. Connection string: `IdpConfigurationDb`. LD SDK key: `AppSettings["LaunchDarklySdkKey"]`.

**UserNotification** (`WinService/UserNotification/`) — also `ServiceBase` subclass. Uses a single `System.Timers.Timer` (initial 60 000 ms, then reconfigured each tick from `AppSettings["callInterval"]`). The `Elapsed` handler `ServiceTimer_Tick` runs three scheduled jobs:

- Every tick where `DateTime.Now.Minute % 15 == 0`: `SendRegularUserNotification()` (SP `Ident.ListFutureLogins`) + `ProcessPendingUsers()` (SP `Ident.ListPendingUsers`)
- Every tick where UTC `HH:mm` equals `AppSettings["ScheduledTime"]`: `ProcessDisableUsersinProducts()` (SP `Ident.ListExpiringUsers`)

All three respect `use-core-api-v2-for-service`. Each fans out with `Parallel.ForEach` (max `_threadCount`), chunked into 20- or 50-user sublists, posting to `AppSettings["LandingAPIUri"]`.

**UserNotification does NOT consume Kafka.** That repo's `packages.config` references `Confluent.Kafka 1.5.0` + `librdkafka.redist 1.5.0` (`net452`) only because `RealPage.SerilogSinks.Kafka v1.0.1` depends on it. BatchProcessor references the same Serilog Kafka sink but with newer `Confluent.Kafka 2.0.2` + `librdkafka.redist 2.0.2` (`net48`). The actual Kafka producer for `UserStatus` events lives in `Component/Landing/Logic/Messaging/UserStatusKafkaProducer.cs` and is used by LandingAPI, not the Windows services.

### F. External Integrations

**Identity Server / Duende.** The actual IdentityServer 7 lives in the sibling `unified-login-core` repo (ASP.NET Core 8). This repo only **validates** tokens via `RealPage.IdentityServer4.AccessTokenValidation`. SAT issuer URI: `https://www-sat.realpage.com/login/identity`. Required scopes: `userinfoapi rplandingapi companyfunctions`. SAML 2.0 assertion building for SSO hand-off lives in `Service/LandingAPI/Saml/` (`ProductSaml.cs`, `RealPageSaml2.cs`, `SamlConstants.cs`); `SamlController` exposes `GET saml/persona/product` and `GET saml/persona/product/attributes`.

**LaunchDarkly.** Static singleton in `Component/Landing/ThirdParty/FeatureFlag.cs:26-28` (`new LdClient(Configuration.Default(ConfigReader.GetLaunchdarklySdkKey))`). Two flag keys actually checked:
- `"user-company-association"` (`FeatureFlag.cs:30`) — used in `OrganizationController.cs:1270`, `PersonController.cs:383`, `UserController.cs:359`
- `"use-core-api-v2-for-service"` — checked by `WinService/*/Helper/FeatureFlagService.cs` (5-min `MemoryCache` wrapper around `ILdClient.BoolVariation`)

**Email — three paths:**

1. **SendGrid** (`ManageEmail.SendGridEmail`, lines 319-376) — reads `ProductInternalSettings` for `UnifiedPlatform` product, checks `IsSendGridEnabled == "1"`, then `SendGridApiEndPoint` + `SendGridSendEmailEndPoint`, POSTs JSON via `HttpClient`.
2. **Unified Email** (`ManageEmail.SendEmailAsync`, lines 382-435) — reads `UnifiedEmailBaseAddress`, `UnifiedEmailEndPoint`, `UseDefaultTemplate`. Gets bearer token via `_tokenHelper.GetUnifiedLoginServerToken("emailsapi")`. POSTs `EmailModel`.
3. **Legacy CES SOAP** (`ManageEmail.SendEmail`, lines 196-276) — SOAP envelope to `IdentityConfig/CESURL` (`http://smtp.lettersandnotices.residentemails.com/emailservice.asmx`).

`ManageUserRegistrationEmail.EmailStatus()` at line 268 routes based on both toggles: Unified Email if `IsUnifiedEmailEnabled == "1"`, else SendGrid if `IsSendGridEnabled == "1"`, else CES.

**Note:** CLAUDE.md and `.github/copilot-instructions.md` reference a SendGrid Event Webhook endpoint `/api/sendgrid/events` with `X-Twilio-Email-Event-Webhook-Signature` verification. **That endpoint is NOT present in the current `Controllers/` directory** — the only webhook implemented is `WebHookController.cs::POST webhook/books` with HMAC-SHA256 against a `signature` header (`SHA.GenerateHMACSHA256String`, lines 105-136).

**Serilog.** Every entry point calls `SerilogHelpers.ConfigureSerilog("Unified Login")` from `RealPage.Logging.Serilog`. Config keys in `appSettings`:
- `logging:environment`, `logging:filePath`, `logging:elasticIndexFormatRoot` (`rp-logs-unity`), `logging:elasticAdditionalTags` (`ule-app-log`), `logging:kafkaCluster` (three `rceeitapkaf2xx.realpage.com:9092` brokers), `logging:excludedProperties` (`EventId`), `serilog:minimum-level` (`Debug`)

Sinks in `bin/`: `Serilog.Sinks.Elasticsearch`, `Serilog.Sinks.File`, `Serilog.Sinks.Console`, `RealPage.SerilogSinks.Kafka`. Kibana queries are scoped via `rp-logs-unity` index + `ule-app-log` tag (hence the "ULE" Kibana query convention).

Per-log enrichment is applied inline in logic code: `Log.Logger.ForContext("CorrelationId", _userClaim.CorrelationId).ForContext("ProductModule", this.GetType()).ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData))`. Canonical implementation: `ManageUser.cs:879-895`, replicated across all Manage classes.

**Redis.** `Component/Landing/CacheHelper/RedisCacheManager.cs:10-13` initializes `Lazy<ConnectionMultiplexer>` from connection string `RedisConnection` (`Web.config:22` — `rcauneaprds101:6479,password=...,syncTimeout=20000,asyncTimeout=20000,allowAdmin=True`). `RedisCacheService.SetCacheValue<T>` / `GetCacheValue<T>` / `RemoveCacheValue` (`RedisCacheService.cs`) JSON-serialize via `StringSet`/`StringGet`/`KeyDelete` on the default DB (no DB index). **The `IRedisCacheService` is defined but lightly used** — call sites are limited to a few product orchestrators (e.g., `ManageProductResidentPortal`, `ManageProductOneSiteAccounting`, `UserRepository`). The dominant caching layer is in-process `RPObjectCache`.

**MSMQ + Activity logging.** `LogActivity.WriteActivity(ActivityDetails)` (`Audit.Core/LogActivity.cs:15-41` and `SharedObjects/Extensions/LogActivity.cs:32`) serializes an `ActivityDetailMessage` and sends to the MSMQ queue named by `AppSettings["ActivityMQName"]` (`Web.config:37`, default `.\private$\greenbook_activity`). A WCF service `Writer.svc.cs` implements `IWriter.ReadMqActivity` (`[OperationContract(IsOneWay = true)]`, `IWriter.cs:10`) — it's an MSMQ binding endpoint that dequeues and calls `ActivityRepository.InsertActivity()` → `[Logging].[InsertActivity]` SP → `AuditDbCnn` SQL DB (`Logging.Activity` + `Logging.ActivityDetail` tables in `Enterprise/Foundation/Activity/Database/`).

**Kafka.** Producer-only from this repo. `LandingAPI/Web.config:45` defines topic `unified-login-user-status-dev`. `Component/Landing/Logic/Messaging/UserStatusKafkaProducer.cs` is the only first-party Kafka producer; downstream consumers (e.g., the actual notification flow if migrated to Kafka under the `use-core-api-v2-for-service` flag) live outside this repo.

### G. Database Projects

**`Identity.sqlproj`** (`Database/Identity/Identity.sqlproj`) — primary OLTP DB. SQL Server 2016 target, `ReadCommittedSnapshot=True`, organized `BySchemaAndSchemaType`. Schemas: `Ident`, `Enterprise`, `Person`, `Batch`, `CustomField`, `Security`, `Settings`, `Hots`, `Maintenance`.

Key tables (central to the multi-tenant model):

| Table | Purpose |
|---|---|
| `Ident.UserLogin` | Primary credential record per user |
| `Ident.UserLoginPersona` | Joins UserLogin to Persona (multi-tenant user identity) |
| `Ident.PasswordPolicy` / `Ident.PasswordHistory` | Password policy + history |
| `Ident.SamlProductSettings` / `Ident.SamlUserAttribute` | Per-product SAML config |
| `Ident.IdentityProviderType` / `Ident.IdentityProviderSetting` | IDP catalog |
| `Enterprise.Organization` | Tenant (company) root entity |
| `Enterprise.Party` / `Enterprise.PartyRole` | Party model linking persons + orgs |
| `Enterprise.Product` / `Enterprise.ProductSetting` / `Enterprise.ProductSettingType` | Product catalog + per-tenant settings |
| `Enterprise.ProductBatch` | Async batch provisioning tracking |
| `Enterprise.PropertyInstance` / `Enterprise.PropertyInstanceMapping` | Black Book property → persona mapping |
| `Person.Person` / `Person.Persona` | Person + per-org persona context |
| `Batch.BatchProcessorGroup` / `Batch.BatchProcessorLog` | Batch run tracking |

Representative SPs: `Ident.CreateUserLogin`, `Ident.GetPasswordPolicy`, `Ident.GetProductSamlDetails`, `Enterprise.GetOrganization_Ver03`, `Enterprise.ListProductsByOrganization`, `Enterprise.CreateOrganizationProduct`, `Security.ListRolesForProductsByPersonaId`, `Security.LinkPersonaToRole`, `Batch.GetUserBatchRecords`, `Enterprise.BulkCreateDeleteUPFMPropertyInstanceMapping` (TVP). Post-deployment is driven by `Scripts/PostDeployment/Script.PostDeployment.sql` with ~10 current `:r` includes (work-item-numbered), always ending in `UpdateStatistics.sql`, `RecompileAllProcs.sql`, `UL_Identity_DataCleanup.sql`.

**`Identity-RR`** — parallel SSDT project (read-replica/older variant). Contains an `Auth` schema (`Auth.GetUserById`, `Auth.GetPasswordPolicy`, `Auth.GetProducts`, etc.) alongside mirrored `Enterprise` SPs. `Migrations/0001_20170727-1259_cford.sql` indicates the seed point in July 2017.

**`UPReporting.sqlproj`** — audit reporting DB with the `UserAudit` schema only. Tables: `UserAudit.Request` (BIGINT identity, `ReportKey`, `OrgPartyId`, `Status`), `UserAudit.User` (snapshot: `PersonaId`, `UserName`, names, type, relationship, `LastLoginDate`, status, `OrganizationRealPageId`), plus `UserProduct`, `UserRole`, `UserProperty`, `PrimaryPropertyAudit`, `ReportParameter`, `RequestReportParameter`, `UserNotification`. Seeded by `UserAuditMasterData.sql`.

**Activity DBs** — `Enterprise/Foundation/Activity/Database/Activity.sqlproj` (legacy) and `Enterprise/Foundation/Activity/DatabaseV2/` (`AuditDBV2.sqlproj`, `AuditArchiveDB.sqlproj`) — the MSMQ-consumed activity log destination.

**SSRS** — `SSRSReports/PlatformUsers.rdl` is the only report; `DS_Users.rds` points at UPReporting's `UserAudit` credentials.

### H. SharedObjects / Contracts (`Service/SharedObjects/`)

Folder layout: `All/`, `Attribute/`, `Audit/`, `Base/`, `Batch/`, `BlackBook/`, `Constants/`, `DapperMappingGuides/`, `Enterprise/`, `EnterpriseRole/`, `Enum/` (27 enum files), `Helper/`, `IdentityConfig/`, `Landing/`.

**`ProductEnum`** (`Enum/ProductEnum.cs:216`) — 60+ values with non-contiguous IDs, each carrying a `[Description]` matching the BlueBook code. Key entries: `OneSite=1[OS]`, `UnifiedUI=2[UI]`, `UnifiedPlatform=3[UPFM]`, `AssetOptimizer=4[AO]`, `Propertyware=5[PW]`, `Lead2Lease=6[L2L]`, `FinancialSuite=8[ACCT]`, `MarketingCenter=9[LS]`, `ResidentPortal=17[AB]`, `AoBusinessIntelligence=29[BI]`, `AoRevenueManagement=32[PO]`, `AoAxiometrics=33[AX]`, `AoLeaseRentOption=51[LRO]`, `AoAIRevenueManagement=53[AIRM]`, `IntelligentBuildingTrash=57[SMS-T]`, `RealConnect=94[RCLMS]`, `AoBIX=95[BIX]`, `TrustDashboard=97[TD]`, `AoLuminaAscent=103[LA]`, `AdminSupportPortalStandard=104[RPISF]`, and ~40 more. Companion `ProductEnumHelper` provides `GetAoProductList()`, `GetProductIdByProductCode()`, `GetProductCodeByProductId()`. A parallel `ProductRightEnum` (line 712) uses the same IDs.

**`BlueBookProductConstants`** (`Constants/BlueBookProductConstants.cs`) — one `public const string` per product, e.g. `OneSite = "OS"` matching `ProductEnum.OneSite[Description("OS")]`. Header comment: "Product string names MUST match the `ProductEnum`." Consistency is enforced functionally by mock setup in multiple test files (e.g., `ManageProductTest.cs`, `WebHookTests.cs`).

**`StoredProcNameConstants`** (`Constants/StoredProcNameConstants.cs`) — ~150 constants in the main class plus 8 in `EnterpriseStoredProcNameConstants`. Schema-qualified SP names with optional `_VerN` suffix for versioned coexistence. The Windows services maintain **separate** `Constants/StoredProcNameConstants.cs` files of their own.

**`EmailFormatValidation`** (`Helper/EmailFormatValidation.cs:15`) — single static `IsValidEmail(string)` delegating to `new EmailAddressAttribute().IsValid(email)`.

Enum files in `Enum/`: `AccessType`, `ActivityType`, `BatchProcessType`, `BookMasterType`, `CommunicationEventAudienceType`, `CommunicationEventPurposeType`, `CreateUserSourceType`, `DeviceType`, `EmailStatusType`, `JobTitleType`, `LogActivityCategoryType`, `ParentUserRoleType`, `PersonaType`, `PhoneType`, `ProductBatchStatusType`, `ProductIntegrationTypeEnum`, `ProductSelectType`, `ProviderEnum`, `RoleCategoryType`, `SamlAttributeEnum`, `SearchUserSortOrderType`, `SeverityLevelType`, `TabEnum`, `UserLoginStatusType`, `UserLoginUpdateType`, `UserRoleType`, `UserStatusType`, `ProductEnum`.

### I. Foundation Libraries (`Enterprise/Foundation/`)

**`DataAccess.csproj`** — Dapper wrapper:
- `IConnectionFactory` / `ConnectionFactory` (`ConnectionFactory.cs:10-18`) — creates `SqlConnection`.
- `IUnitOfWork` / `DapperUnitOfWork` — manages `IDbConnection`, `TransactionScope` with `ReadCommitted` (line 63-65).
- `IRepository` / `DapperRepository` — `ExecuteNonQuery`, `Execute<T>`, `GetOne<T>`, `GetMany<T>` (multi-type up to 5), `QueryMultiple`, TVP overloads. All overloads use `CommandType.StoredProcedure`. Exceptions are rethrown with SP name + parameters attached to `Exception.Data`.
- `Helper/TableValueParamHelper.cs` + `Model/TableValueParmInfo.cs` for TVP construction.

**`Audit.Core.csproj`** — `LogActivity` (MSMQ writer), `Log` (Serilog wrapper), `PerfLogger`/`PerformanceLogger`, `ActivityDetails`/`ActivityDetailMessage`.
**`Audit.WebApi.csproj`** — `ApiPerformanceFilter` (`Filters/ApiPerformanceFilter.cs`) — Web API `ActionFilterAttribute`, gated by `AppSettings["ShouldLogPerformance"]`. Stores `PerformanceLogger` in `Request.Properties["PerfTracker"]`, stops on `OnActionExecuted`. Does **not** audit bodies — only identity claims + route + duration.
**`Audit.MvcWeb.csproj`** — `TrackPerformanceAttribute` — MVC equivalent, stores stopwatch in `HttpContext.Items["Stopwatch"]`.

**`Activity/Service/Command/`** — WCF MSMQ consumer. `IWriter.ReadMqActivity` is `[OperationContract(IsOneWay = true)]`. `Writer.svc.cs` calls `ActivityRepository.InsertActivity` → SP `[Logging].[InsertActivity]` on the Activity DB.

### J. UI Layer + Cross-Repo Topology

**`Web/Landing/`** — the only checked-in web project. ASP.NET MVC 4.8 (assembly `RP.Enterprise.Subsystem.ProductLauncher.Web.Landing`). It serves:
- **Pre-built Angular 16 SPA** — `Web/Landing/index.html` has `<base href="/home/">` and hashed chunk filenames (e.g., `main-KTOLKH34.js`, `polyfills-PFJVKUYQ.js`). This is the compiled output of the `unified-login-landing` repo, **committed here** as static files. A parallel copy lives in `Web/Landing/unityui/`.
- **Server-rendered MVC controllers** — `AccountController`, `AuthController`, `HomeController`, `DashboardController`, `ProductController`, `ManageController`, `SettingController`, `CompaniesController`, `ErrorController`.
- **OWIN OIDC** — `App_Start/Startup.Auth.cs`, `Startup.cs` configures OpenID Connect against the IdentityServer.
- Routes: `RouteConfig.cs:12-35` maps `/home` → `HomeController.Index`, `/signout/{msgId}` → `Signout`, `/azureauth` → `AzureAuth`.

**`Web/identity-ui-src/` and `Web/landing-ui-src/`** — referenced in `BuildUI.bat:6-13` (runs `npm update && grunt clean && grunt` in each) but **not present in this checkout**. Excluded from TFS via `Web/.tfignore:51-52`. Source for these Grunt-based UI builds is managed locally on dev machines or in a sibling area; only their compiled output is committed.

**Local IIS topology (`WebSiteSetup-wwwlocal.ps1`):**

Two sites: `wwwlocal` (`www-local.realpage.com:443`, path `web\wwwlocal`) and `wwwlocal2` (`www-local2.realpage.com:443`, path `web\landingmaster`). Applications (lines 195-203):

| IIS App Path | App Pool | Physical Path | Source Repo |
|---|---|---|---|
| `wwwlocal/home` | `wwwhome` | `web\landing` | this repo (`Web/Landing`) |
| `wwwlocal/login` | `wwwlogin` | (mapped manually) | `unified-login-core` |
| `wwwlocal2/api` | `wwwlocal2api` | `service\landingapi` | this repo |
| `wwwlocal2/apienterprise` | `wwwlocal2apienterprise` | `service\landingapienterprise` | this repo |
| `wwwlocal2/apicore` | `wwwlocal2apicore` | `service\apicore` | `unified-login-coreapi` |
| `wwwlocal2/apicoreenterprise` | `wwwlocal2apicoreenterprise` | `service\apicoreenterprise` | `unified-login-coreapi` |

The `apicore`, `apicoreenterprise`, and `login` app pools set `managedRuntimeVersion=""` (line 124) for .NET Core hosting. The Docker variant (`WebSiteSetup-wwwlocal-docker.ps1:138-143`) maps everything under `"Default Web Site"` and references `..\..\..\..\unified-login-core\UnifiedLogin.IdentityServer` for the login app.

**Environment URLs** (`README.md:77-89`):

| Env | API | UI |
|---|---|---|
| LOCAL | `https://www-local2.realpage.com` | `https://www-local.realpage.com/home` |
| DEV | `https://www-dev2.realpage.com` | `https://www-dev.realpage.com/home` |
| QA | `https://my2qa.realpage.com` | `https://www-qa.realpage.com/home` |
| SAT | `https://my2sat.realpage.com` | `https://www-sat.realpage.com/home` |
| PROD | `https://my2.realpage.com` | `https://www.realpage.com/home` |
| EUPROD | `https://my2.realpage.co.uk` | `https://www.realpage.co.uk/home` |

Swagger paths: `/api/swagger/ui/index`, `/apienterprise/swagger/ui/index`, `/apicore/swagger/index.html`. OIDC discovery: `/login/identity/.well-known/openid-configuration`.

**Cross-repo HTTP calls from this repo:** None directly to the sibling repos. The IdentityServer URI is only consumed by the token validation middleware via OIDC discovery. Outbound HTTP from the logic layer goes to **external platform services** (Books/UDM API, Settings/UPFM API, per-product APIs) — all addressed by `ProductInternalSetting` values, not by hardcoded sibling-repo URLs.

## Code References

### API layer
- `Service/LandingAPI/Startup.cs:33-67` — OWIN pipeline, bearer auth, message handlers, global CORS, conditional Swagger
- `Service/LandingAPI/BaseApiController.cs:24` — class-level `[AuthorizeScope("rplandingapi")]`
- `Service/LandingAPI/BaseApiController.cs:84-155` — claims extraction, `client_info` synthetic-claim path, `DefaultUserClaim` construction
- `Service/LandingAPIEnterprise/BaseApiController.cs:24` — class-level `[AuthorizeScope("enterpriseapi")]`
- `Service/LandingAPI/AllowCorsAttribute.cs:34-58` — DB-backed CORS policy with 5-min `MemoryCache`
- `Service/SharedObjects/Landing/DefaultUserClaim.cs:28-73` — claim → property mapping
- `Component/Landing/Attributes/AuthorizeScopeAttribute.cs:55-112` — scope enforcement
- `Service/SharedObjects/Handlers/ApiExceptionHandler.cs:33` — 500 response shaping
- `Service/SharedObjects/Exceptions/ApiExceptionLogger.cs:16-50` — exception logging with Serilog context
- `Service/SharedObjects/Handlers/TibcoRequestHandler.cs:23` — request-body pre-read for webhook signature verification
- `Service/LandingAPI/Controllers/WebHookController.cs:100-140` — Books webhook + HMAC-SHA256

### Logic + repository
- `Component/Landing/Logic/ManageUser.cs:40-84` — three-constructor pattern (production + 2 test)
- `Component/Landing/Logic/ManageUser.cs:96-173` — `ValidateUser` orchestration example
- `Component/Landing/Logic/ManageUser.cs:259-366` — `CreateUser` orchestration with audit + email + delegate-admin
- `Component/Landing/Logic/ManageUser.cs:879-895` — canonical `WriteToLog` pattern (CorrelationId/ProductModule/AdditionalInfo)
- `Component/Landing/Logic/Product/ManageProductBase.cs:33` — per-product orchestrator base class
- `Component/Landing/Logic/ProductIntegration/Types/IIntegrationType.cs` — integration contract
- `Component/Landing/Logic/ProductIntegration/Factory/IntegrationTypeFactory.cs:13-82` — factory dispatch
- `Component/Landing/Logic/ProductIntegration/Helpers/ApiIntegration.cs:11-26` — HTTP verb wrapper for product APIs
- `Component/Landing/Logic/ManageBlueBook.cs:54, 87-100, 161` — Books API client setup + caching constant
- `Component/Landing/Logic/ManageUnifiedSettings.cs:34, 49-70, 353-357` — Settings/UPFM API client with bearer token
- `Component/Landing/Repository/BaseRepository.cs:26-41` — Dapper wiring
- `Component/Landing/Repository/UserRepository.cs:135-141` — canonical SP-call pattern
- `Component/Landing/Repository/PropertyRepository.cs:143` — TVP example (`Enterprise.PropertyInstanceType`)
- `Component/Landing/Repository/UnifiedLoginRepository.cs:148` — TVP example (`enterprise.productidtype`)

### Windows services
- `WinService/BatchProcessor/Program.cs:21-41` — service entry point
- `WinService/BatchProcessor/BatchProcessorService.cs:68-111` — six parallel loops
- `WinService/BatchProcessor/BatchProcessorService.cs:136, 275` — sleep gate + LD short-circuit
- `WinService/UserNotification/UserNotificationService.cs:32, 359` — timer setup
- `WinService/BatchProcessor/packages.config` — Confluent.Kafka 2.0.2 / librdkafka 2.0.2
- `WinService/UserNotification/packages.config` — Confluent.Kafka 1.5.0 / librdkafka 1.5.0

### Integrations
- `Component/Landing/ThirdParty/FeatureFlag.cs:26-30` — LaunchDarkly singleton + `user-company-association` flag
- `WinService/*/Helper/FeatureFlagService.cs` — cached LD wrapper
- `Component/Landing/Logic/ManageEmail.cs:196-276, 319-376, 382-435` — three email paths
- `Component/Landing/Logic/ManageUserRegistrationEmail.cs:268` — toggle-driven routing
- `Component/Landing/CacheHelper/RedisCacheManager.cs:10-13` — Redis lazy multiplexer
- `Component/Landing/CacheHelper/RedisCacheService.cs` — JSON serializer wrapper
- `Component/Landing/Logic/Messaging/UserStatusKafkaProducer.cs` — sole Kafka producer
- `Service/LandingAPI/Web.config:7, 22, 37, 45` — IssuerUri, RedisConnection, ActivityMQName, Kafka topic
- `Enterprise/Foundation/Activity/Service/Command/IWriter.cs:10` — MSMQ-bound WCF contract
- `Enterprise/Foundation/Activity/Service/Command/Writer.svc.cs` — `ReadMqActivity` → SP
- `Enterprise/Foundation/Audit/Audit.WebApi/Filters/ApiPerformanceFilter.cs` — perf filter

### Contracts
- `Service/SharedObjects/Constants/StoredProcNameConstants.cs` — ~150 SP name constants
- `Service/SharedObjects/Constants/BlueBookProductConstants.cs:4-186, 334` — product code constants + `SettingConstants`
- `Service/SharedObjects/Enum/ProductEnum.cs:13-712` — `ProductEnumHelper` + `ProductEnum` + `ProductRightEnum`
- `Service/SharedObjects/Helper/EmailFormatValidation.cs:15` — single-line wrapper

### Local dev / topology
- `Enterprise/Subsystem/ProductLauncher/WebSiteSetup-wwwlocal.ps1:122-203` — IIS site + app + cert provisioning
- `Enterprise/Subsystem/ProductLauncher/WebSiteSetup-wwwlocal-docker.ps1:65-68, 138-143` — Docker variant
- `Enterprise/Subsystem/ProductLauncher/BuildUI.bat:6-13` — UI source build (identity-ui-src + landing-ui-src)
- `Enterprise/Subsystem/ProductLauncher/Web/.tfignore:51-52` — UI source excluded from TFS
- `README.md:77-89` — environment URLs

### Databases
- `Database/Identity/Identity.sqlproj` — primary OLTP DB
- `Database/Identity/Scripts/PostDeployment/Script.PostDeployment.sql:13-26` — post-deploy includes
- `Database/Identity-RR/Identity/Identity.sqlproj` — read-replica variant
- `Database/UPReporting/UPReporting.sqlproj` — audit reporting DB
- `Database/UPReporting/UserAudit/Tables/Request.sql`, `User.sql` — reporting schema
- `SSRSReports/PlatformUsers.rdl` — only SSRS report

## Architecture Insights

1. **Multi-repo coordination is loose.** This repo never calls the sibling repos by URL — the binding is via the shared IdentityServer issuer (token validation only), shared IIS host names (`wwwlocal/login`, `wwwlocal2/apicore`), and a shared database. The Angular UI from `unified-login-landing` is even committed here as a compiled artifact rather than fetched at deploy time. Cross-repo coupling is thus: identity tokens + DB + static-artifact handoff.

2. **No DI container.** The codebase uses **constructor overloading** as a poor-man's DI: production parameterless ctors that new up concrete dependencies, plus test ctors taking mocks. This is consistent across controllers, Manage classes, and per-product orchestrators. It works because the layering is strict and no class has more than a handful of collaborators.

3. **Stored-procedure-first data access.** Every data access goes through a named SP looked up via `StoredProcNameConstants`. Inline SQL is absent. SPs are versioned by `_VerN` suffix (`Enterprise.GetOrganization_Ver03`, `Person.ListPersons_Ver04`) so old call sites can be migrated incrementally. Bulk operations use TVPs (`Enterprise.PropertyInstanceType`, `Enterprise.BigIntListType`, `enterprise.productidtype`).

4. **Product extensibility via a 3-strategy `ProductIntegration` framework.** New products are added by:
   - Picking an integration type (`Legacy` for custom OneSite-style, `StandardV1` for generic modern, `UPFM` for the financial-mgmt family) and configuring it in the DB (`ProductIntegrationType` setting).
   - Adding a `ManageProduct{Name}` orchestrator extending `ManageProductBase`, plus a controller.
   - Adding entries to `ProductEnum` + `BlueBookProductConstants` (the two must stay in sync — enforced by mock setup across tests).

5. **Caching is layered.** Three caches, used for different reasons:
   - `RPObjectCache` (in-process `MemoryCache`) — the workhorse, used in repositories and Manage classes with 180-360s TTLs.
   - `RedisCacheService` — defined but only used by a handful of product orchestrators (ResidentPortal, OneSiteAccounting). Not a system-wide L2 cache.
   - `MemoryCache.Default` in Windows services — for batch configs (60 min) and LD flag values (5 min).

6. **Email has three forks.** SendGrid (modern), Unified Email (RealPage internal service with bearer auth), and legacy CES (SOAP). Routing is via two `ProductInternalSetting` toggles (`IsSendGridEnabled`, `IsUnifiedEmailEnabled`), with CES as the implicit fallback when both are off.

7. **Activity logging uses MSMQ + WCF**, while application logs use Serilog → Elasticsearch + Kafka. Both flows enrich with `CorrelationId`, `ProductModule`, `AdditionalInfo` consistently. Kibana queries use the `ULE` tag, which corresponds to the `ule-app-log` `elasticAdditionalTags` config.

8. **The BatchProcessor is the system's async backbone.** Six parallel SQL polling loops (10s intervals) each fan out via `Parallel.ForEach` to HTTP APIs configured in `Batch.ListBatchProcessConfiguration`. The `use-core-api-v2-for-service` LD flag is the migration switch for moving four of the six loops to a Core API v2 path (presumably in `unified-login-coreapi`). Pending + Retry loops are flag-independent, which suggests they're the most stable / least likely to migrate.

9. **Two Windows services, two librdkafka versions.** UserNotification pins `librdkafka.redist 1.5.0` because of the `RealPage.SerilogSinks.Kafka 1.0.1` transitive dependency under `net452`, while BatchProcessor was upgraded to `net48` and the 2.0.2 series. Both services log to the same Kafka cluster — only the producer client version differs. A future consolidation would target a single SDK + framework version.

10. **Scope-based authorization is granular.** Beyond the class-level `rplandingapi` / `enterpriseapi` defaults, individual methods stack additional scopes (`userinfoapi`, `companyfunctions`, `migrationapi`). This lets a client credential be issued for a narrow surface (e.g., a migration tool that only needs `migrationapi`) without touching the rest of the API.

## Historical Context (from thoughts/)

The `thoughts/` directory is scaffolded but **empty** — only `.gitkeep` files in `thoughts/shared/{research,plans,handoffs,tickets}/`. No prior research, plans, or handoffs exist yet. The only prior analysis artifacts in this repo are root-level:

- `BatchProcessor-Bottlenecks.md` — performance + reliability analysis of the BatchProcessor's six polling loops and `Parallel.ForEach` fan-out. Aligns with the architecture findings here.
- `Legacy-Errors-Analysis-05.05.2026.md` (2026-05-05) — analysis of 2,824 unique error rows (~16,781 occurrences) from batch processing runs, by ProductId / error type / remediation notes.
- `legacy-errors-05.05.2026.csv` — raw error data underlying the above.

`.claude/` contains the slash-command + sub-agent framework (the RPI workflow), not architecture documentation.

## Related Research

None yet — this is the first research document in `thoughts/shared/research/`.

## Open Questions

1. **SendGrid Event Webhook gap.** CLAUDE.md and `.github/copilot-instructions.md` document a `/api/sendgrid/events` endpoint with `X-Twilio-Email-Event-Webhook-Signature` verification, but no such endpoint exists in this checkout. Is it deferred work, has it moved to `unified-login-coreapi`, or is the documentation stale?

2. **Identity-RR purpose.** The `Identity-RR` project has both an `Auth` schema (parallel to `Ident`) and a 2017 migration seed. Is this an active read-replica project still being deployed, or a frozen historical fork retained for compatibility?

3. **`use-core-api-v2-for-service` rollout scope.** The flag short-circuits four BatchProcessor loops and all three UserNotification jobs. What's the target end-state — full retirement of the Windows services, or just rerouting their HTTP calls through `unified-login-coreapi`? The "Pending" + "Retry" loops not being gated suggests something specific about those paths.

4. **Redis underutilization.** `RedisCacheService` is defined and connected (`Web.config:22`) but used by only a few product orchestrators. Is this an in-progress migration target, or has the team settled on in-process `RPObjectCache` as the primary cache layer with Redis reserved for specific high-cost external API responses?

5. **UI source visibility.** `Web/identity-ui-src/` and `Web/landing-ui-src/` are excluded from TFS but referenced by `BuildUI.bat`. Where do the sources actually live for developers — local-only, in a different repo, or replaced by the committed Angular SPA in `Web/Landing/`?
