# Technical Documentation — Unified Login CoreMigration

| Field | Value |
|---|---|
| **Version** | 2.0 (net10.0 / CoreMigration branch) |
| **Date** | 2026-02-26 |
| **Branch** | `CoreMigration` |
| **Solution** | `CoreMigration/UnifiedLoginMasterSolution.sln` |
| **Baseline** | .NET Framework 4.8 |
| **Target Framework** | net10.0 |

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Environment Matrix](#2-environment-matrix)
3. [Project Inventory](#3-project-inventory)
4. [Dependency Injection Map](#4-dependency-injection-map)
5. [Endpoint Catalog](#5-endpoint-catalog)
6. [Data Access](#6-data-access)
7. [Messaging & Background Jobs](#7-messaging--background-jobs)
8. [Caching](#8-caching)
9. [Configuration Reference](#9-configuration-reference)
10. [Observability](#10-observability)
11. [Security & Compliance](#11-security--compliance)
12. [Error Handling & Resiliency](#12-error-handling--resiliency)
13. [Testing](#13-testing)
14. [Known Issues / Tech Debt](#14-known-issues--tech-debt)
15. [Appendices](#15-appendices)
16. [Changelog](#changelog)

---

## 1. Executive Summary

The **Unified Login CoreMigration** is a multi-service identity and user-management platform migrated from **.NET Framework 4.8** to **net10.0**. It serves as the authentication gateway, user provisioning engine, and product access management system for RealPage's SaaS ecosystem.

The solution consists of three deployable services and five supporting libraries, all orchestrated locally via **.NET Aspire 13.0** and deployed to **Kubernetes (RKE2)** in production via **Azure Pipelines**.

### Current Migration State

| Concern | Status |
|---|---|
| Hosting model | ✅ Kestrel / `WebApplicationBuilder` (minimal APIs style) |
| Middleware pipeline | ✅ Fully converted |
| DI container | ✅ `Microsoft.Extensions.DependencyInjection` throughout |
| Configuration | ✅ `appsettings.*.json` + env-variable overrides |
| Authentication | ✅ JWT Bearer + OIDC; SAML 2.0 via Sustainsys |
| Serialization | ⚠️ Newtonsoft.Json retained (backward-compat); `System.Text.Json` not adopted |
| HTTP clients | ✅ `HttpClientFactory` + `Microsoft.Extensions.Http.Resilience` |
| Logging | ✅ Serilog with OTLP sink; log4net removed |
| Observability | ✅ OpenTelemetry traces + metrics (OTLP exporter) |
| Dapper / data access | ✅ Dapper 2.1.66 + stored procedures |
| Kafka/messaging | ⚠️ Package installed (`Confluent.Kafka 2.8.0`), NOT configured |
| WCF | ⚠️ `System.ServiceModel.*` NuGet packages retained in `SharedObjects`; usage scope unknown |
| Background jobs | ✅ `BackgroundService` pattern; only `PendingBatchJob` active (others commented out) |
| Feature flags | ✅ LaunchDarkly server SDK 8.10.4 |
| Health checks | ✅ ASP.NET Core health checks + custom HTTP listener in BatchProcessor |

---

## 2. Environment Matrix

Shared environment configs live in `CoreMigration/.shared/appsettings/` and are linked into each web project as content files.

| Environment | Config File | `UnifiedPlatform:Authority` | CORS Origins | Log Environment |
|---|---|---|---|---|
| **local** | `appsettings.local.json` | (Assumption: localhost IdP) | (Assumption: localhost) | LOCAL |
| **dev** | `appsettings.dev.json` | `https://www-dev.realpage.com/login/identity` | (dev domain) | DEV |
| **qa** | `appsettings.qa.json` | (QA IdP URL) | (qa domain) | QA |
| **sat** | `appsettings.sat.json` | (SAT IdP URL) | (sat domain) | SAT |
| **load** | `appsettings.load.json` | (Load IdP URL) | (load domain) | LOAD |
| **uat** | `appsettings.uat.json` | (UAT IdP URL) | (uat domain) | UAT |
| **demo** | `appsettings.demo.json` | (Demo IdP URL) | (demo domain) | DEMO |
| **sandbox** | `appsettings.sandbox.json` | (Sandbox IdP URL) | (sandbox domain) | SANDBOX |
| **preprod** | `appsettings.preprod.json` | (PreProd IdP URL) | (preprod domain) | PREPROD |
| **training** | `appsettings.training.json` | (Training IdP URL) | (training domain) | TRAINING |
| **prod** | `appsettings.prod.json` | `https://www.realpage.com/login/identity` | `https://www.realpage.com` | PROD |
| **euprod** | `appsettings.euprod.json` | (EU prod IdP URL) | (eu domain) | EUPROD |
| **eusat** | `appsettings.eusat.json` | (EU SAT IdP URL) | (eu sat domain) | EUSAT |

> **Note:** Most environment-specific values (connection strings, secrets) are injected via Kubernetes secrets / environment variables at runtime. The shared JSON files contain non-secret overrides only. Production connection strings are **not** present in source.

**Key config deltas per environment:**

| Config Key | DEV (base) | PROD |
|---|---|---|
| `UnifiedPlatform:Authority` | `https://www-dev.realpage.com/login/identity` | `https://www.realpage.com/login/identity` |
| `AllCORSOrigins` | `http://localhost:4200` | `https://www.realpage.com` |
| `Logging:Environment` | `DEV` | `PROD` |
| `TraceIdRatioBasedSampler` | `0.01` (1%) | (per env config) |
| `LaunchDarkly:SdkKey` | `sdk-0768eae2-...` | overridden per env |

Sources: [CoreMigration/UnifiedLogin.LandingAPI/appsettings.json](../CoreMigration/UnifiedLogin.LandingAPI/appsettings.json) · [CoreMigration/.shared/appsettings/appsettings.prod.json](../CoreMigration/.shared/appsettings/appsettings.prod.json)

---

## 3. Project Inventory

| Project Name | Type | TargetFramework | Top Dependencies | Purpose |
|---|---|---|---|---|
| **UnifiedLogin.LandingAPI** | Web (Exe) | net10.0 | AspNetCore.Authentication.JwtBearer, Swashbuckle, Serilog, Aspire.SqlClient, Newtonsoft.Json | Standard public REST API (v2); main consumer-facing login gateway |
| **UnifiedLogin.LandingAPIEnterprise** | Web (Exe) | net10.0 | Same as LandingAPI + Sustainsys.Saml2, OpenIdConnect | Enterprise-specific REST API; SAML/SSO flows, role management |
| **UnifiedLogin.BatchProcessor** | Worker (Exe) | net10.0 | HybridCache, StackExchange.Redis, Polly, LaunchDarkly, IdentityModel.AspNetCore | Background batch processing worker; user-sync, role-sync, product provisioning |
| **UnifiedLogin.AppHost** | Aspire AppHost (Exe) | net10.0 | Aspire.Hosting.AppHost 13.0.0 | Local orchestration host; registers all 3 services for Aspire dev dashboard |
| **UnifiedLogin.ServiceDefaults** | Library | net10.0 | OpenTelemetry 1.12.0, Serilog, Microsoft.Extensions.Http.Resilience, ServiceDiscovery | Cross-cutting defaults: OTel, Serilog, health checks, resilience, service discovery |
| **UnifiedLogin.Core** | Library | net10.0 | JwtBearer, OpenIdConnect, Swashbuckle, LaunchDarkly, IdentityModel | DI extension methods, base controller, auth extensions, Swagger config, Kafka stub |
| **UnifiedLogin.BusinessLogic** | Library | net10.0 | Dapper, StackExchange.Redis, Polly, LaunchDarkly, Sustainsys.Saml2 | All business-logic `IManage*` implementations and 40+ repositories |
| **UnifiedLogin.DataAccess** | Library | net10.0 | Dapper 2.1.66, Microsoft.Data.SqlClient 6.0.2, Serilog | Raw data-access helpers, SQL connection utilities, HealthChecks extensions |
| **UnifiedLogin.SharedObjects** | Library | net10.0 | Newtonsoft.Json, Aspose.Cells, System.ServiceModel.*, SkiaSharp | Shared DTOs, models, legacy WCF type compatibility, Excel utilities |
| **UnifiedLogin.LandingAPI.Tests** | Test (xUnit) | net10.0 | xunit 2.9.3, Moq 4.20.72, FluentAssertions 7.0.0, WireMock.Net | Unit + integration test suite (208 test files) |

> **Package versioning:** All package versions are centrally managed via `CoreMigration/Directory.Packages.props`.

**Build properties per project:**

| Project | Nullable | LangVersion | GenerateDocFile |
|---|---|---|---|
| LandingAPI | disable | (default) | true |
| LandingAPIEnterprise | disable | (default) | false |
| BatchProcessor | disable | (default) | false |
| ServiceDefaults | enable | (default) | false |
| Core | disable | (default) | false |
| BusinessLogic | enable | (default) | false |
| DataAccess | enable | 13.0 | false |
| SharedObjects | disable | (default) | false |

---

## 4. Dependency Injection Map

### 4.1 LandingAPI & LandingAPIEnterprise (Core registrations)

Registrations flow through extension methods in `UnifiedLogin.Core` and `UnifiedLogin.BusinessLogic`.

| Service (Interface → Implementation) | Lifetime | Registration File |
|---|---|---|
| `IUserClaimsAccessor` → *(framework-provided via HttpContext)* | Scoped | [UnifiedLogin.Core/AuthorizationExtensions.cs](../CoreMigration/UnifiedLogin.Core/AuthorizationExtensions.cs) |
| `DefaultUserClaim` → factory from `IUserClaimsAccessor` | Scoped | [LandingAPI/Program.cs:38-42](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L38-L42) |
| `ILdClient` → `LdClient` | Singleton | [UnifiedLogin.Core/LaunchDarklyExtensions.cs](../CoreMigration/UnifiedLogin.Core/LaunchDarklyExtensions.cs) |
| `IRedisCacheService` → `RedisCacheService` | Singleton | [UnifiedLogin.Core/BusinessLogicExtensions.cs](../CoreMigration/UnifiedLogin.Core/BusinessLogicExtensions.cs) |
| `IManageUser` → `ManageUser` | Scoped | BusinessLogicExtensions.cs |
| `IManageProduct` → `ManageProduct` | Scoped | BusinessLogicExtensions.cs |
| `IManageCredential` → `ManageCredential` | Scoped | BusinessLogicExtensions.cs |
| `IManageSecurity` → `ManageSecurity` | Scoped | BusinessLogicExtensions.cs |
| `IManageSaml` → `ManageSaml` | Scoped | BusinessLogicExtensions.cs |
| `IManagePersona` → `ManagePersona` | Scoped | BusinessLogicExtensions.cs / [EnterpriseProgram.cs:61-65](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L61-L65) |
| `IIntegrationTypeFactory` → `IntegrationTypeFactory` | Scoped | BusinessLogicExtensions.cs |
| All `IRepository` implementations (40+) | Scoped | [UnifiedLogin.Core/BusinessLogicExtensions.cs](../CoreMigration/UnifiedLogin.Core/BusinessLogicExtensions.cs) via `.AddRepositories()` |
| `IManageUPFMProductsIntegrationFactory` → `ManageUPFMProductsIntegrationFactory` | Scoped | [EnterpriseProgram.cs:76](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L76) |
| `IRoleQueryService` → `RoleQueryService` | Scoped | [EnterpriseProgram.cs:79](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L79) |
| `IUserManagementService` → `UserManagementService` | Scoped | [EnterpriseProgram.cs:81](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L81) |
| `IUserQueryService` → `UserQueryService` | Scoped | [EnterpriseProgram.cs:82](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L82) |
| `IUserValidationService` → `UserValidationService` | Scoped | [EnterpriseProgram.cs:83](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L83) |
| `ISuperUserValidationService` → `SuperUserValidationService` | Scoped | [EnterpriseProgram.cs:84](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L84) |
| `IClientAuthenticationService` → `ClientAuthenticationService` | Scoped | [EnterpriseProgram.cs:87](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L87) |
| `ILoggingService` → `LoggingService` | Singleton | [EnterpriseProgram.cs:88](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L88) |
| `SamlRepository` (concrete) | Scoped | [EnterpriseProgram.cs:74](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L74) |
| `UserManagement` (concrete) | Scoped | [EnterpriseProgram.cs:67-71](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L67-L71) |
| `IMemoryCache` | Singleton | [LandingAPI/Program.cs:28](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L28) via `AddMemoryCache()` |
| `IDistributedCache` (memory) | Scoped | [LandingAPI/Program.cs:27](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L27) via `AddDistributedMemoryCache()` |
| `SqlConnection` (keyed "DBConnection") | Transient | [LandingAPI/Program.cs:25](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L25) via `AddKeyedSqlServerClient` |

### 4.2 BatchProcessor

| Service (Interface → Implementation) | Lifetime | Registration File |
|---|---|---|
| `IBatchRepository` → `BatchRepository` | Scoped | [ProgramExtensions.cs:24](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L24) |
| `ILdClient` → `LdClient` | Singleton | [ProgramExtensions.cs:27-35](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L27-L35) |
| `IFeatureFlagService` → `FeatureFlagService` | Singleton | [ProgramExtensions.cs:38](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L38) |
| `IProductApiClient` → `ProductApiClient` | Transient (HttpClient) | [ProgramExtensions.cs:41-65](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L41-L65) |
| `IApiRateLimiter` → `ApiRateLimiter` | Singleton | [ProgramExtensions.cs:68](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L68) |
| `BatchProcessingMetrics` | Singleton | [ProgramExtensions.cs:71](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L71) |
| `IConnectionMultiplexer` → `ConnectionMultiplexer` | Singleton | [ProgramExtensions.cs:154-172](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L154-L172) |
| `IDistributedCache` → Redis | Singleton | [ProgramExtensions.cs:175-179](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L175-L179) |
| `HybridCache` | Singleton | [ProgramExtensions.cs:183-192](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L183-L192) |
| `IHybridCacheService` → `HybridCacheService` | Singleton | [ProgramExtensions.cs:195](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L195) |
| `IHostedService` → `HealthCheckWebServer` | Singleton (hosted) | [ProgramExtensions.cs:203](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L203) |
| `IHostedService` → `PendingBatchJob` | Singleton (hosted) | [ProgramExtensions.cs:214](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L214) |
| `SqlConnection` (keyed "DBConnection") | Transient | [BatchProcessor/Program.cs:4](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Program.cs#L4) |

---

## 5. Endpoint Catalog

### 5.1 LandingAPI (`UnifiedLogin.LandingAPI`)

All controllers inherit from `BaseController` which provides `IUserClaimsAccessor`. All endpoints require authentication (`RequireAuthenticatedUser` via global `AuthorizeFilter`).

> **Note:** The full controller list (65+ controllers) is extensive. The table below enumerates confirmed controller classes; individual route/verb details require reading each controller file, which was not done exhaustively. Routes follow convention-based `[controller]` routing unless otherwise noted.

| Controller | Route Prefix | Verbs (Assumption) | AuthZ | Notes |
|---|---|---|---|---|
| `AccessController` | `/api/access` | GET, POST | Authenticated | Security rights and user actions |
| `ActivityController` | `/api/activity` | GET, POST | Authenticated | User activity logging |
| `BatchProcessController` | `/api/batchprocess` | GET, POST, PUT | Authenticated | Batch operation management |
| `BlueBookController` | `/api/bluebook` | GET | Authenticated | Blue book data retrieval |
| `ConfigurationSettingController` | `/api/configurationsetting` | GET, POST, PUT | Authenticated | System settings CRUD |
| `ContactMechanismController` | `/api/contactmechanism` | GET, POST, PUT, DELETE | Authenticated | Contact management |
| `CredentialController` | `/api/credential` | GET, POST, PUT | Authenticated | Credential lifecycle |
| `DashboardController` | `/api/dashboard` | GET | Authenticated | Dashboard aggregation |
| `EmailController` | `/api/email` | POST | Authenticated | Email dispatch |
| `EmployeeAccessController` | `/api/employeeaccess` | GET, POST, PUT | Authenticated | Employee access control |
| `MultiFactorAuthController` | `/api/multifactorauth` | GET, POST, PUT | Authenticated | MFA/2FA management |
| `OrganizationController` | `/api/organization` | GET, POST, PUT | Authenticated | Org management |
| `ProductController` | `/api/product` | GET, POST, PUT | Authenticated | Product access management |
| `ProductEasyLMSController` | `/api/producteasyLMS` | GET, POST | Authenticated | LMS product integration |
| `UserController` | `/api/user` | GET, POST, PUT, DELETE | Authenticated | User CRUD |
| *(50+ additional controllers)* | `/api/[controller]` | Mixed | Authenticated | Various domain features |

**Global middleware/filters applied:**

- `InitializeUserRightsFilter` — pre-populates user rights context per request ([LandingAPI/Program.cs:34](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L34))
- `RequestParameterModelBinderProvider` — custom model binding ([LandingAPI/Program.cs:33](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L33))
- `UnifiedLoginUserScopeMiddleware` — enriches ambient scope with user identity
- `AuthorizeFilter(RequireAuthenticatedUser)` — global auth enforcement ([UnifiedLogin.Core/AuthorizationExtensions.cs](../CoreMigration/UnifiedLogin.Core/AuthorizationExtensions.cs))

**Swagger/OpenAPI:**

- Endpoint: `/swagger` (or env-configured path)
- OAuth2 PKCE flow integrated; scopes from `UnifiedPlatform:ApiName`
- Discovery document fetched from `UnifiedPlatform:Authority/.well-known/openid-configuration`
- XML documentation included (LandingAPI has `GenerateDocumentationFile=true`)

**Health endpoints (mapped by ServiceDefaults):**

| Endpoint | Purpose | Tags Filter |
|---|---|---|
| `/health` | Full health check | All checks |
| `/alive` | Liveness probe | `live` tag only |

### 5.2 LandingAPIEnterprise (`UnifiedLogin.LandingAPIEnterprise`)

Same structure as LandingAPI with additional enterprise-specific controllers for:
- SAML-based SSO flows
- Enterprise role management
- User provisioning for enterprise clients

Swagger title: `"UnifiedLogin_LandingAPI Enterprise"`, route prefix: `apienterprisev2` ([EnterpriseProgram.cs:115](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L115))

### 5.3 BatchProcessor Health Endpoints

Served by `HealthCheckWebServer` (custom `HttpListener`) on port `5000` (configurable via `HealthCheck:Port`):

| Path | Description |
|---|---|
| `http://[host]:5000/health/` | Full JSON health report |
| `http://[host]:5000/health/ready` | Readiness probe (db + cache + services) |
| `http://[host]:5000/health/live` | Liveness probe |

---

## 6. Data Access

### 6.1 Technology Stack

| Component | Technology | Version |
|---|---|---|
| ORM / Query | Dapper | 2.1.66 |
| DB Driver | Microsoft.Data.SqlClient | 6.0.2 |
| Custom extension | RealPage.DataAccess.Dapper | 1.3.1 |
| Connection type | `IDbConnection` (SQL Server) | — |

### 6.2 Connection Management

Connections are registered as **keyed transient** services under the key `"DBConnection"` using Aspire's `AddKeyedSqlServerClient`:

- LandingAPI: [Program.cs:25](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L25)
- LandingAPIEnterprise: [Program.cs:31](../CoreMigration/UnifiedLogin.LandingAPIEnterprise/Program.cs#L31)
- BatchProcessor: [Program.cs:4](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Program.cs#L4)

Connection strings are provided via:
1. `appsettings.json` (dev baseline, contains dev credentials)
2. `appsettings.{environment}.json` override
3. Environment variables / Kubernetes secrets (runtime override)

> **Security note:** Dev credentials exist in the base `appsettings.json` files in source control. Production connection strings are injected at runtime and are NOT stored in source.

### 6.3 Repository Pattern

All repositories follow a base pattern defined in `UnifiedLogin.BusinessLogic/Repository/BaseRepository.cs`. Each repository:

- Accepts `IDbConnection` via constructor injection
- Uses `Dapper`/`RealPage.DataAccess.Dapper` extension methods (`GetManyAsync`, `GetAsync`, `ExecuteAsync`)
- Calls stored procedures exclusively — no inline SQL found in reviewed files
- Returns typed model objects

**Example — BatchRepository** ([Repositories/BatchRepository.cs](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Repositories/BatchRepository.cs)):

```csharp
var results = await _sql.GetManyAsync<Batch>(
    StoredProcNameConstants.SP_ListBatch,
    parameters,
    null,
    cancellationToken);
```

### 6.4 Stored Procedures

Stored procedure names are centralized in `StoredProcConstants.cs` within each service. BatchProcessor confirms the following SPs:

| Constant | Purpose |
|---|---|
| `SP_ListBatch` | Retrieve pending batch records |
| `SP_GetBatchConfigurations` | Fetch endpoint configuration per batch type |
| `SP_UpdateBatch` | Update batch status |

> Assumption: The full SP list for LandingAPI/Enterprise is large (40+ repositories × ~3 SPs average); not enumerated here.

### 6.5 Transaction Handling

No explicit transaction scope (e.g., `IDbTransaction`) was observed in reviewed files. The Dapper extension methods from `RealPage.DataAccess.Dapper` may handle transactions internally — **Assumption**.

### 6.6 Migrations / Schema Management

- No EF Core migrations directory found.
- Database schema changes are managed via **DACPAC** files, deployed by Azure Pipelines (`db-job.yml`, `dacpac-job.yml`).
- No ORM-managed schema evolution.

---

## 7. Messaging & Background Jobs

### 7.1 Kafka (Not Yet Active)

| Property | Value |
|---|---|
| Package | `Confluent.Kafka 2.8.0` |
| Status | **Installed, NOT configured** |
| Stub file | [UnifiedLogin.Core/KafkaExtensions.cs](../CoreMigration/UnifiedLogin.Core/KafkaExtensions.cs) |
| Topics | None defined |
| Producers / Consumers | None registered |

The `AddKafka()` extension method exists but the body is empty. No `IProducer<>` or `IConsumer<>` registrations are present.

### 7.2 Background Jobs (BatchProcessor)

Jobs are implemented as `BackgroundService` instances and registered via `AddHostedService<T>()`.

| Job Class | SP / API Target | Interval | Parallelism | Status | Feature Flag |
|---|---|---|---|---|---|
| `PendingBatchJob` | `SP_ListBatch` → `ProcessBatchAsync` | 10 s | 5 | ✅ **ACTIVE** | `batchProcessorV2` |
| `RetryBatchJob` | (retry SPs) | 10 s | 5 | ⚠️ Commented out | unknown |
| `EnterpriseRolesJob` | (enterprise role SPs) | 30 s | 5 | ⚠️ Commented out | unknown |
| `PrimaryPropertiesJob` | (property SPs) | 30 s | 5 | ⚠️ Commented out | unknown |
| `BulkUserUpdateJob` | (bulk user SPs) | 10 s | 5 | ⚠️ Commented out | unknown |
| `CompanyAndPropertiesUpdateJob` | (company SPs) | 30 s | 5 | ⚠️ Commented out | unknown |
| `FutureUserLoginsJob` | `api/userlogins/processfutureuserlogins` | 9000 s | 5 | ⚠️ Commented out | unknown |
| `PendingUsersExpirationJob` | `api/userlogins/processfutureuserlogins` | 900 s | 5 | ⚠️ Commented out | unknown |
| `DisableExpiredUsersJob` | `api/disableexpiredusers` | scheduled 15:00 | 5 | ⚠️ Commented out | unknown |
| `HealthCheckWebServer` | HTTP listener | — | — | ✅ ACTIVE | — |

Source: [ProgramExtensions.cs:200-228](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L200-L228)

### 7.3 PendingBatchJob Processing Flow

1. Check LaunchDarkly `batchProcessorV2` feature flag (30-min cached result)
2. If disabled → delay and continue loop
3. Open DB scope → call `GetBatchToProcessAsync(batchSize, ...)` via `SP_ListBatch`
4. Pre-fetch endpoint configs → `GetBatchConfigurationsAsync()` (one call per cycle)
5. Close fetch scope; enter `Parallel.ForEachAsync` with `MaxDegreeOfParallelism = 5`
6. Per batch item (own DI scope): acquire rate-limit lease → call `ProductApiClient.ProcessBatchAsync()`
7. On error: update batch status to `Error` via `UpdateBatchRecordAsync()`
8. Record metrics (`BatchProcessingMetrics`); delay `TimeIntervalInSeconds`

Source: [Services/PendingBatchJob.cs:28-251](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Services/PendingBatchJob.cs)

### 7.4 Error Handling in Jobs

- Each parallel batch item is wrapped in individual try/catch — no cascade failures ([PendingBatchJob.cs:128-166](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Services/PendingBatchJob.cs#L128-L166))
- Failed items update DB status to `BatchStatusType.Error`
- Top-level cycle catch logs error and delays 30 seconds before retrying ([PendingBatchJob.cs:67-71](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Services/PendingBatchJob.cs#L67-L71))
- `OperationCanceledException` causes clean shutdown

### 7.5 Rate Limiting

`IApiRateLimiter` (singleton) enforces two separate rate limits:

| Limit Type | Default Value | Config Key |
|---|---|---|
| API calls per second | 10 | `RateLimitSettings:ApiCallsPerSecond` |
| DB calls per second | 50 | `RateLimitSettings:DatabaseCallsPerSecond` |
| Queue limit | 100 | `RateLimitSettings:QueueLimit` |

Each batch item calls `rateLimiter.AcquireAsync("api", ct)` before invoking the external API. If the lease is not acquired, the batch item is skipped and will be retried in the next cycle.

---

## 8. Caching

### 8.1 Cache Architecture

The system uses a **three-tier caching strategy**:

| Layer | Technology | Scope | Config |
|---|---|---|---|
| L1 (process-local) | `IMemoryCache` | Per-process | `HybridCache:Memory:SizeLimitMB = 256` |
| L2 (distributed) | Redis via `IDistributedCache` | Cluster-wide | `ConnectionStrings:RedisConnection` |
| L3 (hybrid abstraction) | `Microsoft.Extensions.Caching.Hybrid` | Wraps L1+L2 | `HybridCache:DefaultOptions` |

### 8.2 HybridCache (BatchProcessor)

| Property | Value |
|---|---|
| Package | `Microsoft.Extensions.Caching.Hybrid 10.0.0` |
| Redis key prefix | `UnifiedLogin:BatchProcessor:` |
| Max payload | 1 MB |
| Max key length | 512 characters |
| Default absolute expiry | 60 minutes |
| Local cache (L1) expiry | 15 minutes |
| Redis connection timeout | 5000 ms |
| Sync timeout | 5000 ms |
| Connect retry | 3 |
| AbortOnConnectFail | `false` |

Source: [ProgramExtensions.cs:129-198](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L129-L198) · [BatchProcessor/appsettings.json:19-37](../CoreMigration/Services/UnifiedLogin.BatchProcessor/appsettings.json#L19-L37)

**Resilience:** If Redis connection fails at startup, the application throws (fail-fast for misconfiguration). Runtime Redis failures are handled gracefully by `HybridCacheService` — falls back to in-memory L1.

### 8.3 Redis (LandingAPI / LandingAPIEnterprise)

- `IRedisCacheService` (Singleton) registered in `BusinessLogicExtensions.cs`
- Uses `StackExchange.Redis 2.8.29`
- `ConnectionStrings:RedisConnection` drives endpoint configuration

### 8.4 In-Memory Cache (LandingAPI / LandingAPIEnterprise)

- `AddDistributedMemoryCache()` — used for **access token caching** for remote API calls ([LandingAPI/Program.cs:27](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L27))
- `AddMemoryCache()` — general-purpose in-memory cache ([LandingAPI/Program.cs:28](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L28))

### 8.5 Feature Flag Cache

`FeatureFlagService` caches LaunchDarkly evaluations for **30 minutes** (configured internally).

### 8.6 Health Check Cache

`/health` and `/alive` endpoints are cached for **10 seconds** via output caching ([ServiceDefaults/Extensions.cs:125-128](../CoreMigration/UnifiedLogin.ServiceDefaults/Extensions.cs#L125-L128)).

---

## 9. Configuration Reference

### 9.1 Key Configuration Sections

#### Authentication (`UnifiedPlatform`)

| Key | Purpose | Example Value |
|---|---|---|
| `UnifiedPlatform:Authority` | OIDC/JWT issuer endpoint | `https://www.realpage.com/login/identity` |
| `UnifiedPlatform:ApiName` | JWT audiences / Swagger scopes | `rplandingapi userinfoapi companyfunctions` |
| `UnifiedPlatform:AdditionalScopes` | Extra OAuth scopes | `""` |
| `UnifiedPlatform:SwaggerClientId` | OAuth client for Swagger UI | `greenbookpkce` |
| `UnifiedPlatform:ClientId` | Batch processor client ID | `batchprocessor` |
| `UnifiedPlatform:ClientSecret` | Batch processor secret | *(injected at runtime)* |
| `UnifiedPlatform:Scope` | Batch processor OAuth scope | `api` |

#### Database

| Key | Purpose |
|---|---|
| `ConnectionStrings:DBConnection` | SQL Server connection string |
| `ConnectionStrings:RedisConnection` | Redis connection string (endpoint + credentials) |

#### Caching (`HybridCache` — BatchProcessor only)

| Key | Default | Purpose |
|---|---|---|
| `HybridCache:Redis:Enabled` | `true` | Enable Redis distributed layer |
| `HybridCache:Redis:ConnectTimeout` | `5000` | Redis connect timeout (ms) |
| `HybridCache:Memory:SizeLimitMB` | `256` | In-memory L1 size limit |
| `HybridCache:DefaultOptions:AbsoluteExpirationMinutes` | `60` | Default cache TTL |
| `HybridCache:DefaultOptions:SlidingExpirationMinutes` | `15` | Sliding (local L1) TTL |

#### Batch Processing (`BatchProcessorSettings`)

| Key | Default | Purpose |
|---|---|---|
| `BatchProcessorSettings:PendingBatch:Enabled` | `true` | Enable/disable job |
| `BatchProcessorSettings:PendingBatch:InstanceCount` | `1` | Number of parallel job instances |
| `BatchProcessorSettings:PendingBatch:MaxDegreeOfParallelism` | `5` | Parallel items per cycle |
| `BatchProcessorSettings:PendingBatch:TimeIntervalInSeconds` | `10` | Sleep interval between cycles |
| `BatchProcessorSettings:PendingBatch:BatchSize` | `5` | Records fetched per cycle |

#### Rate Limiting (`RateLimitSettings`)

| Key | Default | Purpose |
|---|---|---|
| `RateLimitSettings:ApiCallsPerSecond` | `10` | Max API calls per second |
| `RateLimitSettings:DatabaseCallsPerSecond` | `50` | Max DB calls per second |
| `RateLimitSettings:QueueLimit` | `100` | Rate limiter queue depth |

#### Feature Flags (`LaunchDarkly`)

| Key | Purpose |
|---|---|
| `LaunchDarkly:SdkKey` | LaunchDarkly server SDK key |
| `LaunchDarkly:IncludeDebugLogs` | Enable verbose LD logging (`"false"` default) |

#### Observability

| Key | Purpose |
|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OTLP/gRPC or HTTP endpoint for traces/metrics/logs |
| `OTEL_EXPORTER_OTLP_HEADERS` | Auth headers for OTLP endpoint |
| `OTEL_SERVICE_NAME` | Service name label in traces |
| `OTEL_RESOURCE_ATTRIBUTES` | Additional resource tags (`service.instance.id`, etc.) |
| `TraceIdRatioBasedSampler` | Sampling ratio (0.01 = 1%, 1 = 100%) |

#### Logging

| Key | Purpose |
|---|---|
| `Logging:Environment` | Environment label attached to logs |
| `Logging:FilePath` | Optional file sink path |
| `Serilog:MinimumLevel:Default` | Default log level |
| `Serilog:MinimumLevel:Override:Microsoft` | Microsoft namespace override |

#### CORS

| Key | Purpose |
|---|---|
| `AllCORSOrigins` | Comma-separated allowed origins |

### 9.2 Configuration Loading Order

1. `appsettings.json` (base defaults, committed to source)
2. `appsettings.{ASPNETCORE_ENVIRONMENT}.json` (environment override)
3. Environment variables (Kubernetes `ConfigMap` / `Secret`)

Source: [LandingAPI/Program.cs:16-20](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L16-L20)

---

## 10. Observability

### 10.1 Logging

| Property | Value |
|---|---|
| Framework | Serilog 4.3.0 |
| Integration | `Serilog.Extensions.Hosting` + `Microsoft.Extensions.Logging` bridge |
| Configuration | `ReadFrom.Configuration()` + `Enrich.FromLogContext()` |
| Sinks | OTLP (if endpoint configured), Console (fallback), File (if `Logging:FilePath` set) |
| File rolling | Daily; 31-day retention |
| Min level (default) | Information |
| Min level (Microsoft.*) | Warning |
| Min level (System.*) | Warning |

Source: [ServiceDefaults/Extensions.cs:198-271](../CoreMigration/UnifiedLogin.ServiceDefaults/Extensions.cs#L198-L271)

### 10.2 Traces

| Property | Value |
|---|---|
| Framework | OpenTelemetry SDK 1.12.0 |
| Instrumentation | `AspNetCore` + `HttpClient` |
| Sampler | `TraceIdRatioBasedSampler` (ratio from `TraceIdRatioBasedSampler` config; default 1% in DEV) |
| Exporter | OTLP (`UseOtlpExporter()`) if `OTEL_EXPORTER_OTLP_ENDPOINT` is set |
| Excluded paths | `aspnetcore-browser-refresh.js`, `browserLink`, `health` |
| Source name | App name passed to `AddServiceDefaults(appName)` |

Source: [ServiceDefaults/Extensions.cs:54-96](../CoreMigration/UnifiedLogin.ServiceDefaults/Extensions.cs#L54-L96)

### 10.3 Metrics

| Metric Group | Source |
|---|---|
| ASP.NET Core request metrics | `AddAspNetCoreInstrumentation()` |
| HTTP client metrics | `AddHttpClientInstrumentation()` |
| .NET runtime metrics | `AddRuntimeInstrumentation()` |
| Custom batch metrics | `BatchProcessingMetrics` (singleton, custom) |

Source: [ServiceDefaults/Extensions.cs:65-73](../CoreMigration/UnifiedLogin.ServiceDefaults/Extensions.cs#L65-L73)

### 10.4 Custom Batch Metrics (`BatchProcessingMetrics`)

`BatchProcessingMetrics` (registered as Singleton in BatchProcessor) tracks:
- Batches retrieved per job
- Batches processed (success/failure)
- Processing duration (milliseconds)

Source: [ProgramExtensions.cs:71](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L71)

### 10.5 Problem Details (Error Responses)

All API errors return RFC 7807 Problem Details with:
- `traceId` (root Activity ID)
- `spanId` (current span ID)
- `url` (on 404)
- Standard `status`, `title`, `detail` fields

Source: [ServiceDefaults/Extensions.cs:160-182](../CoreMigration/UnifiedLogin.ServiceDefaults/Extensions.cs#L160-L182)

### 10.6 Correlation IDs

Correlation flows via OpenTelemetry `Activity` propagation. `traceId` is embedded in error responses and Serilog log context (`Enrich.FromLogContext()`).

---

## 11. Security & Compliance

### 11.1 Authentication Schemes

| Scheme | Technology | Used By |
|---|---|---|
| JWT Bearer | `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0` | LandingAPI, LandingAPIEnterprise |
| OpenID Connect | `Microsoft.AspNetCore.Authentication.OpenIdConnect 10.0.0` | LandingAPI, LandingAPIEnterprise |
| SAML 2.0 | `Sustainsys.Saml2 2.11.0` | LandingAPIEnterprise |
| Client Credentials | `IdentityModel.AspNetCore 4.3.0` | BatchProcessor (outbound token mgmt) |

**OIDC Authority:** Configured per environment via `UnifiedPlatform:Authority`.

### 11.2 Authorization

| Mechanism | Details | File |
|---|---|---|
| Global `AuthorizeFilter` | `RequireAuthenticatedUser` applied to all MVC routes | [UnifiedLogin.Core/AuthorizationExtensions.cs](../CoreMigration/UnifiedLogin.Core/AuthorizationExtensions.cs) |
| `InitializeUserRightsFilter` | Populates user rights context before action execution | [LandingAPI/Program.cs:34](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L34) |
| `UnifiedLoginUserScopeMiddleware` | Enriches request scope with user identity claims | [LandingAPI/Program.cs:83](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L83) |
| `IUserClaimsAccessor` | Per-request claims accessor used by all business logic | BusinessLogicExtensions.cs |

### 11.3 Secrets Management

| Secret | Handling |
|---|---|
| DB connection strings | Dev: `appsettings.json` (source controlled); Prod: K8s secrets |
| Redis credentials | Dev: `appsettings.json`; Prod: K8s secrets |
| `UnifiedPlatform:ClientSecret` | Dev: `appsettings.json`; Prod: K8s secrets |
| `LaunchDarkly:SdkKey` | In `appsettings.json` per environment (dev key in source) |
| OTLP auth headers | `OTEL_EXPORTER_OTLP_HEADERS` env variable |

> **Risk:** Dev credentials and a LaunchDarkly SDK key are present in source-controlled `appsettings.json` files. While dev-only, this is a credential exposure risk. See [Known Issues](#14-known-issues--tech-debt).

### 11.4 CORS

Configured from `AllCORSOrigins` config key. Origins are comma-separated. Applied conditionally (only if non-empty).
- DEV: `http://localhost:4200`
- PROD: `https://www.realpage.com`

Source: [LandingAPI/Program.cs:59-67](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L59-L67)

### 11.5 Container Security

Kubernetes deployments run as **non-root** with UID `10000`:
```yaml
securityContext:
  runAsNonRoot: true
  runAsUser: 10000
```

### 11.6 Input Validation

- `RequestParameterModelBinderProvider` — custom model binding for query/form parameters
- `FluentValidation.DependencyInjectionExtensions 12.0.0` — registered in `ServiceDefaults`; used for DTO validation (specific validators not enumerated here)
- DataAnnotations used via ASP.NET Core model validation pipeline

### 11.7 Forwarded Headers

Both APIs accept `X-Forwarded-For`, `X-Forwarded-Proto`, `X-Forwarded-Prefix` to operate correctly behind a reverse proxy/ingress:

```csharp
ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedPrefix
```

Source: [LandingAPI/Program.cs:75-78](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L75-L78)

---

## 12. Error Handling & Resiliency

### 12.1 Global Exception Handling

`UseExceptionHandler()` middleware is registered in both APIs, positioned after `UseAuthorization()` and `UseMiddleware<UnifiedLoginUserScopeMiddleware>()` to capture the authenticated user in error context.

Problem Details format returned for all unhandled exceptions (RFC 7807).

Source: [LandingAPI/Program.cs:84](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L84)

### 12.2 HTTP Resilience (Polly via `Microsoft.Extensions.Http.Resilience`)

**Applied to:** `IProductApiClient` in BatchProcessor

| Policy | Configuration |
|---|---|
| Per-attempt timeout | 15 seconds |
| Retry | 3 attempts, exponential backoff: 2 s → 4 s → 8 s, with jitter |
| Circuit breaker | Opens after 10% failure ratio over 30-second window (min 10 requests); breaks for 30 seconds |
| Total request timeout | 90 seconds (covers 4 attempts + all delays) |

Source: [ProgramExtensions.cs:41-65](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L41-L65)

**Applied to all HTTP clients via ServiceDefaults:**

`AddStandardResilienceHandler()` is applied globally to all `HttpClient` instances via `ConfigureHttpClientDefaults()`:

Source: [ServiceDefaults/Extensions.cs:39-43](../CoreMigration/UnifiedLogin.ServiceDefaults/Extensions.cs#L39-L43)

### 12.3 Resilience Patterns Summary

| Pattern | Where Applied |
|---|---|
| Retry with exponential backoff | HTTP clients (Polly via `AddStandardResilienceHandler`) |
| Circuit breaker | `ProductApiClient` in BatchProcessor |
| Timeout (per-attempt + total) | `ProductApiClient` in BatchProcessor |
| Cache fallback (Redis → in-memory) | `HybridCacheService` in BatchProcessor |
| Rate limiting | `IApiRateLimiter` in BatchProcessor parallel loops |
| Error isolation | `Parallel.ForEachAsync` try/catch per item in `PendingBatchJob` |
| Graceful shutdown | `OperationCanceledException` handling in all `BackgroundService` classes |

### 12.4 Health Checks as Resiliency Indicators

Health checks are used by Kubernetes to:
- Stop routing traffic on failed readiness probe
- Restart pods on failed liveness probe

---

## 13. Testing

### 13.1 Test Project

| Property | Value |
|---|---|
| Project | `UnifiedLogin.LandingAPI.Tests` |
| Framework | xUnit 2.9.3 |
| Location | `CoreMigration/Tests/UnifiedLogin.LandingAPI.Tests/` |
| Test file count | 208 |

### 13.2 Dependencies

| Package | Version | Purpose |
|---|---|---|
| xunit | 2.9.3 | Test runner |
| Moq | 4.20.72 | Mocking |
| FluentAssertions | 7.0.0 | Assertion DSL |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.0 | Integration test host |
| WireMock.Net | 1.12.0 | HTTP stubbing |
| Serilog.Sinks.TestCorrelator | 3.2.0 | Log assertion |
| RealPage.XUnit.Utilities | 1.0.0 | Custom RealPage test helpers |
| coverlet.collector | 6.0.4 | Code coverage collection |

### 13.3 Coverage Configuration

Coverage settings: `CoreMigration/CodeCoverage.runsettings`

| Setting | Value |
|---|---|
| ExcludeByFile | `**/ProductIntegration/Model/**/*.cs` |
| ExcludeByAttribute | `ExcludeFromCodeCoverageAttribute` |

### 13.4 Removed Tests

The following test files were removed from the solution (deleted from csproj item groups):
- `Controllers\ProductClientPortalControllerTests.cs`
- `Controllers\ResearchApplicationControllerTests.cs`

This corresponds to the removal of the matching controllers from LandingAPI.

### 13.5 Test Categories

Tests cover:
- Controller-level tests (enterprise + standard)
- Product-specific controllers (EasyLMS, LearningPortal)
- *(Assumption: business logic unit tests likely exist but not fully enumerated)*

### 13.6 Build Command

```bash
dotnet test CoreMigration/UnifiedLoginMasterSolution.sln \
  --collect:"XPlat Code Coverage" \
  --settings CoreMigration/CodeCoverage.runsettings
```

---

## 14. Known Issues / Tech Debt

| # | Issue | File / Location | Impact |
|---|---|---|---|
| 1 | **Kafka not configured** — `Confluent.Kafka 2.8.0` installed, `AddKafka()` stub exists but body is empty | [UnifiedLogin.Core/KafkaExtensions.cs](../CoreMigration/UnifiedLogin.Core/KafkaExtensions.cs) | No event streaming capability; any planned async decoupling is blocked |
| 2 | **8 of 9 batch jobs commented out** — Only `PendingBatchJob` is active; all others are disabled via code comments | [ProgramExtensions.cs:215-225](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L215-L225) | User sync, role sync, company/property sync, expiration processing not running |
| 3 | **Dev credentials in source** — `appsettings.json` files contain dev DB credentials, Redis credentials, and a LaunchDarkly SDK key | [LandingAPI/appsettings.json:3-4](../CoreMigration/UnifiedLogin.LandingAPI/appsettings.json#L3-L4) · [BatchProcessor/appsettings.json:3-4](../CoreMigration/Services/UnifiedLogin.BatchProcessor/appsettings.json#L3-L4) | Credential exposure; should migrate to dev secrets manager or user secrets |
| 4 | **`Nullable` disabled in most projects** — LandingAPI, LandingAPIEnterprise, BatchProcessor, Core, SharedObjects all have `<Nullable>disable</Nullable>` | Multiple `.csproj` files | NullReferenceException risk; incremental enablement recommended |
| 5 | **Newtonsoft.Json retained** — `System.Text.Json` not adopted; `AddNewtonsoftJsonConfiguration()` used for backward compatibility | [LandingAPI/Program.cs:35](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L35) | Increased dependency; performance gap vs STJ; blocks future STJ-only ecosystem compatibility |
| 6 | **`System.ServiceModel.*` in SharedObjects** — WCF interop packages retained; actual usage scope not confirmed | `UnifiedLogin.SharedObjects/UnifiedLogin.SharedObjects.csproj` | Legacy WCF compatibility; should audit and remove if unused |
| 7 | **`ConfigReader.Initialize()` static pattern** — Static config reader initialized in both Programs | [LandingAPI/Program.cs:23](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs#L23) | Anti-pattern; bypasses DI; makes testing harder |
| 8 | **`AddHostedServices()` builds intermediate `ServiceProvider`** — A `BuildServiceProvider()` call inside `AddHostedServices` creates an intermediate container | [ProgramExtensions.cs:206](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Configuration/ProgramExtensions.cs#L206) | Anti-pattern (double container build); can cause warnings and subtle DI issues |
| 9 | **`OTEL_EXPORTER_OTLP_HEADERS` commented out in AppHost** — Auth headers for OTLP export not configured for local dev | [AppHost.cs:10](../CoreMigration/UnifiedLogin.AppHost/AppHost.cs#L10) | Local development traces may not reach the observability backend |
| 10 | **Aspose.Cells 23.2.0** — Commercial library in SharedObjects; licensing compliance requires verification | `UnifiedLogin.SharedObjects.csproj` | License cost / compliance risk |
| 11 | **`ManageRedBook` not DI-registered** — Requires a `gbtoken` parameter that doesn't fit standard DI | [UnifiedLogin.Core/BusinessLogicExtensions.cs](../CoreMigration/UnifiedLogin.Core/BusinessLogicExtensions.cs) | Feature may not be fully accessible via DI; ad-hoc instantiation likely |
| 12 | **Polly v7.2.3 used** — Polly v8 (breaking changes) not yet adopted; `Microsoft.Extensions.Http.Resilience` wraps Polly v8 internally but `Polly 7.2.3` is also a direct dependency | `Directory.Packages.props` | Mixed Polly versions; potential conflict |

---

## 15. Appendices

### A. Build Commands

```bash
# Restore packages
dotnet restore CoreMigration/UnifiedLoginMasterSolution.sln

# Build (Release)
dotnet build CoreMigration/UnifiedLoginMasterSolution.sln -c Release

# Run tests with coverage
dotnet test CoreMigration/UnifiedLoginMasterSolution.sln \
  --collect:"XPlat Code Coverage" \
  --settings CoreMigration/CodeCoverage.runsettings

# Publish LandingAPI
dotnet publish CoreMigration/UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj \
  -c Release -r linux-x64 --no-self-contained

# Run via Aspire (local orchestration)
dotnet run --project CoreMigration/UnifiedLogin.AppHost/UnifiedLogin.AppHost.csproj
```

### B. Local Development Setup

1. **Prerequisites:**
   - .NET 10.0 SDK
   - Docker Desktop (for Redis and SQL Server containers)
   - .NET Aspire workload: `dotnet workload install aspire`

2. **Environment:**
   - Set `ASPNETCORE_ENVIRONMENT=local` or `dev`
   - Or override via `appsettings.local.json`

3. **Run via Aspire:**
   ```bash
   cd CoreMigration
   dotnet run --project UnifiedLogin.AppHost/UnifiedLogin.AppHost.csproj
   ```
   This starts all three services and the Aspire dashboard at `http://localhost:18888`.

4. **User secrets (recommended to avoid dev creds in config):**
   ```bash
   dotnet user-secrets set "ConnectionStrings:DBConnection" "..." \
     --project UnifiedLogin.LandingAPI/UnifiedLogin.LandingAPI.csproj
   ```

### C. Docker Build

```bash
# Build BatchProcessor image
docker build -f CoreMigration/Services/UnifiedLogin.BatchProcessor/Dockerfile \
  -t unified-login-batchprocessorv2:latest .
```

### D. Key Environment Variables

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Selects `appsettings.{env}.json` |
| `ConnectionStrings__DBConnection` | SQL Server override (double-underscore for nested keys) |
| `ConnectionStrings__RedisConnection` | Redis override |
| `UnifiedPlatform__Authority` | OIDC authority override |
| `UnifiedPlatform__ClientSecret` | Batch processor OAuth secret |
| `LaunchDarkly__SdkKey` | Feature flag SDK key |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Telemetry endpoint |
| `OTEL_EXPORTER_OTLP_HEADERS` | Telemetry auth headers |
| `OTEL_SERVICE_NAME` | Service name for telemetry |
| `TraceIdRatioBasedSampler` | Sampling ratio (0.0–1.0) |
| `AllCORSOrigins` | Comma-separated CORS allowed origins |

### E. NuGet Sources

Configured via `CoreMigration/NuGet.config`. Includes the private RealPage Artifactory feed for internal packages:
- `RealPage.DataAccess.Dapper`
- `RealPage.XUnit.Utilities`
- `RealPage.UnifiedNotifications`

---

## Changelog

| Date | Author | Summary |
|---|---|---|
| 2026-02-26 | Claude Sonnet 4.6 (AI-generated) | Initial documentation of `CoreMigration` branch as-is state. Covers all 10 projects in `UnifiedLoginMasterSolution.sln`, middleware pipeline, DI map, endpoint catalog, data access (Dapper/stored procs), batch jobs, caching (HybridCache + Redis), observability (Serilog/OTel), security (JWT/OIDC/SAML), resiliency (Polly), and known tech debt. Delta from .NET Framework 4.8 baseline noted throughout. |
