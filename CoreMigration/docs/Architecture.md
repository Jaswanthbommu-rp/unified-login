# Architecture — Unified Login CoreMigration

| Field | Value |
|---|---|
| **Version** | 2.0 (net10.0 / CoreMigration branch) |
| **Date** | 2026-02-26 |
| **Branch** | `CoreMigration` |
| **Original Baseline** | .NET Framework 4.8 |

---

## Table of Contents

1. [System Context (C4 Level 1)](#1-system-context-c4-level-1)
2. [Container View (C4 Level 2)](#2-container-view-c4-level-2)
3. [Component View (C4 Level 3)](#3-component-view-c4-level-3)
4. [Runtime Views — Sequence Diagrams](#4-runtime-views--sequence-diagrams)
5. [Deployment View](#5-deployment-view)
6. [Quality Attributes](#6-quality-attributes)
7. [Migration Notes from .NET Framework 4.8](#7-migration-notes-from-net-framework-48)
8. [Changelog](#changelog)

---

## 1. System Context (C4 Level 1)

The **Unified Login** system is the identity and user-management hub for the RealPage SaaS platform. It authenticates users, manages product access, and synchronizes user/role data to downstream product systems.

```mermaid
C4Context
    title System Context — Unified Login (CoreMigration)

    Person(user, "End User / Employee", "Authenticates via browser or product client")
    Person(admin, "System Administrator", "Manages user provisioning and roles")

    System(ul, "Unified Login", "Identity gateway, user provisioning, and batch sync for the RealPage platform")

    System_Ext(idp, "RealPage Identity Provider", "OIDC / OAuth2 authority (UnifiedPlatform:Authority). Issues JWT tokens and handles SAML federation.")
    System_Ext(sql, "SQL Server", "Primary relational data store for user, product, batch, and configuration data")
    System_Ext(redis, "Redis Cache", "Distributed cache for session data and batch state")
    System_Ext(ld, "LaunchDarkly", "Feature flag evaluation — controls job activation and rollout")
    System_Ext(product, "RealPage Product Systems", "Downstream product APIs (OneSite, Ops, Panel, Lead2Lease, etc.) — receive user-sync payloads")
    System_Ext(otel, "Elastic APM / OTLP Backend", "Receives traces, metrics, and structured logs via OTLP")
    System_Ext(nuget, "Artifactory (NuGet)", "Private NuGet feed for RealPage internal packages")

    Rel(user, ul, "HTTP/HTTPS — REST API calls with JWT Bearer token")
    Rel(admin, ul, "HTTP/HTTPS — Management API calls")
    Rel(ul, idp, "HTTPS — Token validation (JWKS), OIDC discovery, client credentials token request")
    Rel(ul, sql, "TCP 1433 — Dapper / stored procedures")
    Rel(ul, redis, "TCP 6379/6480 — StackExchange.Redis")
    Rel(ul, ld, "HTTPS — LaunchDarkly streaming / SDK polling")
    Rel(ul, product, "HTTPS — REST batch payloads (ProductApiClient)")
    Rel(ul, otel, "HTTPS OTLP — Traces, metrics, logs")
```

---

## 2. Container View (C4 Level 2)

The solution deploys as three independent containers, backed by shared infrastructure.

```mermaid
C4Container
    title Container View — Unified Login CoreMigration

    Person(user, "End User / Client System")

    System_Boundary(ul, "Unified Login") {
        Container(landing, "UnifiedLogin.LandingAPI", "ASP.NET Core 10 Web API", "Standard public REST API; handles user login, product access, credential management (65+ controllers)")
        Container(enterprise, "UnifiedLogin.LandingAPIEnterprise", "ASP.NET Core 10 Web API", "Enterprise REST API; SAML SSO flows, enterprise role management, user provisioning")
        Container(batch, "UnifiedLogin.BatchProcessor", "NET 10 Worker Service", "Background batch processor; syncs users, roles, products, and properties to downstream systems")
        Container(apphost, "UnifiedLogin.AppHost", "NET Aspire AppHost", "Local dev orchestration only — NOT deployed to production")
    }

    ContainerDb(sql, "SQL Server", "Microsoft SQL Server", "Primary store: users, products, batch records, configuration, credentials")
    ContainerDb(redis, "Redis", "StackExchange.Redis", "Distributed cache: session tokens, batch state, feature flag cache")

    System_Ext(idp, "RealPage Identity Provider", "OIDC/OAuth2/SAML authority")
    System_Ext(product, "Downstream Product APIs", "OneSite, Ops, Panel, EasyLMS, etc.")
    System_Ext(ld, "LaunchDarkly", "Feature flags")
    System_Ext(otel, "OTLP Backend (Elastic APM)", "Observability")

    Rel(user, landing, "HTTPS — JWT Bearer", "REST/JSON")
    Rel(user, enterprise, "HTTPS — JWT Bearer / SAML", "REST/JSON")

    Rel(landing, sql, "TCP — Dapper/SPs", "SQL")
    Rel(landing, redis, "TCP — StackExchange.Redis", "Redis protocol")
    Rel(landing, idp, "HTTPS — JWKS, token introspection", "OIDC")
    Rel(landing, ld, "HTTPS — SDK", "LaunchDarkly streaming")
    Rel(landing, otel, "HTTPS OTLP", "Traces/Metrics/Logs")

    Rel(enterprise, sql, "TCP — Dapper/SPs", "SQL")
    Rel(enterprise, redis, "TCP — StackExchange.Redis", "Redis protocol")
    Rel(enterprise, idp, "HTTPS — JWKS, SAML, OIDC", "OIDC/SAML")
    Rel(enterprise, ld, "HTTPS — SDK", "LaunchDarkly")
    Rel(enterprise, otel, "HTTPS OTLP", "Traces/Metrics/Logs")

    Rel(batch, sql, "TCP — Dapper/SPs", "SQL")
    Rel(batch, redis, "TCP — HybridCache", "Redis protocol")
    Rel(batch, idp, "HTTPS — Client Credentials token", "OAuth2")
    Rel(batch, product, "HTTPS — REST payloads", "JSON via ProductApiClient")
    Rel(batch, ld, "HTTPS — SDK", "LaunchDarkly")
    Rel(batch, otel, "HTTPS OTLP", "Traces/Metrics/Logs")
```

---

## 3. Component View (C4 Level 3)

### 3.1 LandingAPI Components

```mermaid
C4Component
    title Component View — UnifiedLogin.LandingAPI

    Container_Boundary(landing, "UnifiedLogin.LandingAPI") {
        Component(prog, "Program.cs", "Top-level statements", "Builds WebApplication; registers services; configures middleware pipeline")
        Component(mw_fwd, "ForwardedHeaders Middleware", "ASP.NET Core Middleware", "Extracts X-Forwarded-* headers from reverse proxy")
        Component(mw_auth, "Authentication Middleware", "JwtBearer / OIDC", "Validates JWT tokens against UnifiedPlatform Authority")
        Component(mw_authz, "Authorization Middleware", "AuthorizeFilter", "Enforces RequireAuthenticatedUser globally")
        Component(mw_scope, "UnifiedLoginUserScopeMiddleware", "Custom Middleware", "Enriches ambient scope with user identity/claims")
        Component(mw_exc, "ExceptionHandler Middleware", "ASP.NET Core", "Catches unhandled exceptions; returns RFC 7807 Problem Details")
        Component(ctrl, "Controllers (65+)", "ASP.NET Core MVC Controllers", "Domain controllers: User, Product, Access, Credential, MFA, Organization, etc.")
        Component(filter_init, "InitializeUserRightsFilter", "IActionFilter", "Pre-populates user rights context before action execution")
        Component(binder, "RequestParameterModelBinder", "IModelBinderProvider", "Custom binding for RequestParameter objects")
        Component(swagger, "SwaggerDocumentation", "Swashbuckle + OAuth2 PKCE", "OpenAPI spec with OAuth2 PKCE flow")
        Component(base_ctrl, "BaseController", "Abstract MVC Controller", "Provides IUserClaimsAccessor, PersonaId, UserId to all controllers")
    }

    Container_Boundary(core, "UnifiedLogin.Core (library)") {
        Component(auth_ext, "AuthorizationExtensions", "IServiceCollection extension", "Adds global AuthorizeFilter")
        Component(biz_ext, "BusinessLogicExtensions", "IServiceCollection extension", "Registers 100+ IManage*, IRepository services")
        Component(ld_ext, "LaunchDarklyExtensions", "IServiceCollection extension", "Registers ILdClient singleton")
        Component(json_ext, "JsonSerializationExtensions", "Newtonsoft.Json config", "CamelCase, GuidConverter, NullValueHandling.Include")
        Component(kafka_stub, "KafkaExtensions", "Stub — NOT active", "Empty AddKafka() method")
    }

    Container_Boundary(bl, "UnifiedLogin.BusinessLogic (library)") {
        Component(manage, "IManage* Services (100+)", "Business logic", "ManageUser, ManageProduct, ManageCredential, ManageSaml, etc.")
        Component(repos, "IRepository implementations (40+)", "Repository pattern", "UserRepository, ProductRepository, PersonaRepository, etc.")
        Component(redis_svc, "IRedisCacheService", "Cache abstraction", "Wraps StackExchange.Redis")
    }

    Container_Boundary(da, "UnifiedLogin.DataAccess (library)") {
        Component(dapper, "Dapper extensions", "RealPage.DataAccess.Dapper", "GetManyAsync, GetAsync, ExecuteAsync wrappers")
    }

    Container_Boundary(sd, "UnifiedLogin.ServiceDefaults (library)") {
        Component(otel_cfg, "OpenTelemetry Config", "OTLP exporter", "Traces, metrics, OTLP export")
        Component(serilog_cfg, "Serilog Config", "OTLP + Console + File", "Structured logging")
        Component(health_ep, "Health Endpoints", "/health, /alive", "ASP.NET Core HealthChecks")
        Component(prob_details, "Problem Details", "RFC 7807", "Standardized error format with traceId/spanId")
    }

    Rel(prog, mw_fwd, "registers")
    Rel(prog, mw_auth, "registers")
    Rel(prog, mw_authz, "registers")
    Rel(prog, mw_scope, "registers")
    Rel(prog, mw_exc, "registers")
    Rel(prog, ctrl, "maps endpoints")
    Rel(prog, auth_ext, "calls")
    Rel(prog, biz_ext, "calls")
    Rel(prog, ld_ext, "calls")

    Rel(ctrl, base_ctrl, "inherits")
    Rel(ctrl, manage, "uses via DI")
    Rel(manage, repos, "uses")
    Rel(repos, dapper, "uses")
    Rel(manage, redis_svc, "uses")
```

### 3.2 BatchProcessor Components

```mermaid
C4Component
    title Component View — UnifiedLogin.BatchProcessor

    Container_Boundary(batch, "UnifiedLogin.BatchProcessor") {
        Component(bp_prog, "Program.cs", "Host builder", "CreateApplicationBuilder; AddKeyedSqlServerClient; AddRequiredServices; AddHostedServices")
        Component(prog_ext, "ProgramExtensions", "IServiceCollection extensions", "AddRequiredServices() + AddHostedServices() + AddHybridCacheServices() + AddHealthCheckServices()")
        Component(pending_job, "PendingBatchJob", "BackgroundService (ACTIVE)", "Polls DB for pending batches; processes in parallel with rate limiting; calls ProductApiClient")
        Component(retry_job, "RetryBatchJob", "BackgroundService (commented out)", "Retries failed batch records")
        Component(ent_job, "EnterpriseRolesJob", "BackgroundService (commented out)", "Syncs enterprise role assignments")
        Component(bulk_job, "BulkUserUpdateJob", "BackgroundService (commented out)", "Bulk user attribute updates")
        Component(disable_job, "DisableExpiredUsersJob", "BackgroundService (commented out)", "Disables expired user accounts at scheduled time")
        Component(ff_svc, "FeatureFlagService", "Singleton — ILdClient wrapper", "Evaluates LaunchDarkly flags; 30-min local cache")
        Component(rate, "ApiRateLimiter", "Singleton — IApiRateLimiter", "Token bucket: 10 API req/s, 50 DB req/s")
        Component(metrics, "BatchProcessingMetrics", "Singleton", "Records batches retrieved, processed, durations")
        Component(hc_server, "HealthCheckWebServer", "BackgroundService + HttpListener", "Serves /health/ready, /health/live on port 5000")
        Component(batch_repo, "BatchRepository", "Scoped — IBatchRepository", "GetBatchToProcessAsync, GetBatchConfigurationsAsync, UpdateBatchRecordAsync via Dapper")
        Component(api_client, "ProductApiClient", "Transient — IProductApiClient", "HTTP client to downstream product APIs; Polly resilience handler")
        Component(hybrid_cache, "HybridCacheService", "Singleton — IHybridCacheService", "L1 (memory) + L2 (Redis) with graceful fallback")
        Component(token_mgmt, "Access Token Management", "IdentityModel.AspNetCore", "Client credentials flow; token cached in IDistributedCache")
    }

    Rel(bp_prog, prog_ext, "calls AddRequiredServices + AddHostedServices")
    Rel(pending_job, ff_svc, "checks batchProcessorV2 flag each cycle")
    Rel(pending_job, batch_repo, "GetBatchToProcessAsync, UpdateBatchRecordAsync")
    Rel(pending_job, api_client, "ProcessBatchAsync per batch item")
    Rel(pending_job, rate, "AcquireAsync before each API call")
    Rel(pending_job, metrics, "RecordBatchesRetrieved, RecordBatchProcessed")
    Rel(api_client, token_mgmt, "attaches client_credentials Bearer token")
    Rel(batch_repo, hybrid_cache, "caches config lookups")
    Rel(hc_server, batch_repo, "DatabaseHealthCheck")
    Rel(hc_server, hybrid_cache, "RedisHealthCheck")
```

### 3.3 Cross-Cutting Concerns

```mermaid
graph TB
    subgraph "Cross-Cutting Concerns"
        Auth["Authentication<br/>(JwtBearer / OIDC / SAML)"]
        Authz["Authorization<br/>(RequireAuthenticatedUser global filter)"]
        Logging["Structured Logging<br/>(Serilog → OTLP / Console / File)"]
        Tracing["Distributed Tracing<br/>(OpenTelemetry → OTLP)"]
        Metrics["Metrics<br/>(OTel AspNetCore + HttpClient + Runtime)"]
        FF["Feature Flags<br/>(LaunchDarkly — 30-min cache)"]
        Cache["Caching<br/>(IMemoryCache / Redis / HybridCache)"]
        Resilience["Resilience<br/>(Polly retry + circuit breaker + timeout)"]
        Validation["Input Validation<br/>(FluentValidation + Model binders)"]
        Health["Health Checks<br/>(/health, /alive, port 5000)"]
        ProbDetails["Error Responses<br/>(RFC 7807 Problem Details + traceId)"]
    end

    subgraph "Services"
        LA["LandingAPI"]
        EA["LandingAPIEnterprise"]
        BP["BatchProcessor"]
    end

    Auth --> LA
    Auth --> EA
    Authz --> LA
    Authz --> EA
    Logging --> LA
    Logging --> EA
    Logging --> BP
    Tracing --> LA
    Tracing --> EA
    Tracing --> BP
    Metrics --> LA
    Metrics --> EA
    Metrics --> BP
    FF --> LA
    FF --> EA
    FF --> BP
    Cache --> LA
    Cache --> EA
    Cache --> BP
    Resilience --> LA
    Resilience --> EA
    Resilience --> BP
    Validation --> LA
    Validation --> EA
    Health --> LA
    Health --> EA
    Health --> BP
    ProbDetails --> LA
    ProbDetails --> EA
```

---

## 4. Runtime Views — Sequence Diagrams

### 4.1 Authentication Flow (JWT Bearer / OIDC)

This diagram shows the flow for an authenticated API request to `UnifiedLogin.LandingAPI`.

```mermaid
sequenceDiagram
    autonumber
    participant C as Client (Browser / API Consumer)
    participant RP as Reverse Proxy / Ingress
    participant API as LandingAPI (ASP.NET Core)
    participant MWAuth as JWT Bearer Middleware
    participant IDP as RealPage Identity Provider (OIDC)
    participant MWScope as UnifiedLoginUserScopeMiddleware
    participant Filter as InitializeUserRightsFilter
    participant Ctrl as Controller (e.g., UserController)
    participant BL as Business Logic (IManage*)
    participant DB as SQL Server

    C->>RP: HTTPS request with Authorization: Bearer {jwt}
    RP->>API: Forward request (X-Forwarded-For/Proto/Prefix headers)
    API->>API: ForwardedHeadersMiddleware — extract real client IP/scheme
    API->>MWAuth: JWT Bearer handler invoked
    MWAuth->>IDP: Fetch JWKS from {Authority}/.well-known/openid-configuration (cached)
    IDP-->>MWAuth: Signing keys
    MWAuth->>MWAuth: Validate JWT signature, expiry, audience
    alt JWT invalid
        MWAuth-->>C: HTTP 401 Unauthorized
    end
    MWAuth->>API: HttpContext.User populated with claims
    API->>API: AuthorizationMiddleware — RequireAuthenticatedUser check
    API->>MWScope: UnifiedLoginUserScopeMiddleware — extract PersonaId, UserId from claims
    API->>Filter: InitializeUserRightsFilter.OnActionExecuting — load user rights
    Filter->>DB: SP: GetUserRights (via IBatchRepository or equivalent)
    DB-->>Filter: Rights data
    Filter->>API: User rights added to context
    API->>Ctrl: Route matched → Action method invoked
    Ctrl->>BL: IManage*.SomeOperationAsync(DefaultUserClaim)
    BL->>DB: Dapper stored procedure call
    DB-->>BL: Result set
    BL-->>Ctrl: Domain object
    Ctrl-->>API: IActionResult (200 OK + JSON body)
    API-->>RP: HTTP response
    RP-->>C: HTTPS response
```

### 4.2 SAML Enterprise SSO Flow (LandingAPIEnterprise)

```mermaid
sequenceDiagram
    autonumber
    participant U as Enterprise User (Browser)
    participant EA as LandingAPIEnterprise
    participant SAML as Sustainsys.Saml2 Handler
    participant IdP as Enterprise Identity Provider (SAML)
    participant SamlRepo as SamlRepository (DB)
    participant RoleQ as IRoleQueryService
    participant DB as SQL Server

    U->>EA: Access enterprise resource (no session)
    EA->>SAML: AuthN challenge issued
    SAML->>SamlRepo: Load SAML SP configuration for tenant
    SamlRepo->>DB: SP: GetSamlConfiguration
    DB-->>SamlRepo: SP metadata, cert, binding
    SAML->>IdP: SAML AuthnRequest (redirect binding)
    IdP->>U: Render IdP login form
    U->>IdP: Credentials submitted
    IdP->>EA: SAML Response (POST binding) with assertions
    SAML->>SAML: Validate signature, conditions, audience
    SAML->>EA: ClaimsPrincipal from SAML assertions
    EA->>RoleQ: IRoleQueryService.GetRolesAsync(enterpriseUserId)
    RoleQ->>DB: SP: GetEnterpriseRoles
    DB-->>RoleQ: Role assignments
    RoleQ-->>EA: Roles as claims
    EA-->>U: Authenticated session + redirect to resource
```

### 4.3 Batch Processing Flow (PendingBatchJob)

```mermaid
sequenceDiagram
    autonumber
    participant Job as PendingBatchJob (BackgroundService)
    participant FF as FeatureFlagService (LaunchDarkly)
    participant LD as LaunchDarkly
    participant Rate as ApiRateLimiter
    participant DB as SQL Server
    participant Cache as HybridCacheService (Redis + Memory)
    participant IdP as RealPage Identity Provider
    participant TokenMgr as AccessTokenManagement
    participant ProdAPI as Downstream Product API

    loop Every TimeIntervalInSeconds (10s)
        Job->>FF: GetBoolFlagAsync("batchProcessorV2")
        FF->>Cache: Check flag cache (30-min TTL)
        alt Cache miss
            Cache-->>FF: miss
            FF->>LD: Evaluate flag
            LD-->>FF: bool value
            FF->>Cache: Store result
        end
        Cache-->>FF: bool value
        FF-->>Job: isBatchProcessorV2Enabled

        alt Feature flag disabled
            Job->>Job: Delay(TimeInterval); continue
        end

        Job->>DB: GetBatchToProcessAsync(batchSize=5, SP_ListBatch)
        DB-->>Job: List<Batch> (pending records)

        Job->>Cache: GetBatchConfigurationsAsync (endpoint configs, cached)
        Cache-->>Job: Dictionary<BatchProcessTypeId, endpoint>

        par Parallel.ForEachAsync (max 5 concurrent)
            Job->>Rate: AcquireAsync("api") — token bucket
            Rate-->>Job: Lease (acquired/denied)
            alt Lease denied
                Job->>Job: Skip batch, retry next cycle
            end

            Job->>TokenMgr: Get cached client_credentials token
            alt Token expired
                TokenMgr->>IdP: POST /connect/token (client_credentials)
                IdP-->>TokenMgr: access_token (cached in IDistributedCache)
            end

            Job->>ProdAPI: POST {ProcessApiEndPoint} with BatchProcessorInput + Bearer token
            Note over ProdAPI: Polly: 3 retries, exp backoff, circuit breaker
            ProdAPI-->>Job: HTTP 200 (success response)

            alt API call failed after retries
                Job->>DB: UpdateBatchRecordAsync(Error, SP_UpdateBatch)
            end
        end

        Job->>Job: RecordMetrics(success, failure, duration)
        Job->>Job: Delay(TimeInterval)
    end
```

### 4.4 Cache Read Flow (HybridCache)

```mermaid
sequenceDiagram
    autonumber
    participant Caller as BatchRepository / Service
    participant HCS as HybridCacheService
    participant L1 as IMemoryCache (In-Process)
    participant L2 as Redis (IDistributedCache)
    participant DB as SQL Server

    Caller->>HCS: GetOrCreateAsync(key, factory, options)
    HCS->>L1: Lookup key in memory
    alt L1 hit
        L1-->>HCS: Cached value (local expiry: 15 min)
        HCS-->>Caller: Cached value
    else L1 miss
        HCS->>L2: GetAsync(key) from Redis
        alt L2 hit
            L2-->>HCS: Serialized value
            HCS->>L1: Populate L1 (local expiry)
            HCS-->>Caller: Deserialized value
        else L2 miss (or Redis unavailable)
            HCS->>DB: factory() — execute Dapper SP
            DB-->>HCS: Fresh data
            HCS->>L2: SetAsync(key, value, absolute expiry: 60 min)
            HCS->>L1: SetAsync(key, value, local expiry: 15 min)
            HCS-->>Caller: Fresh value
        end
    end
```

---

## 5. Deployment View

### 5.1 Environment Topology

```mermaid
graph TB
    subgraph "Developer Workstation"
        AppHost["UnifiedLogin.AppHost<br/>(NET Aspire — local only)"]
        AppHost --> LandingDev["LandingAPI (localhost)"]
        AppHost --> EnterpriseDev["LandingAPIEnterprise (localhost)"]
        AppHost --> BatchDev["BatchProcessor (localhost)"]
    end

    subgraph "Kubernetes Cluster (RKE2 / Rancher)"
        subgraph "Namespace: unified-login-{env}"
            Ingress["Gateway / Ingress<br/>(HTTPRoute)"]

            subgraph "LandingAPI Deployment"
                API1["Pod: unified-login-coreapiv2"]
                API2["Pod: unified-login-coreapiv2"]
                APIHPA["HPA (autoscaling)"]
            end

            subgraph "Enterprise API Deployment"
                EA1["Pod: unified-login-coreenterpriseapiv2"]
                EAHPA["HPA (autoscaling)"]
            end

            subgraph "BatchProcessor Deployment"
                BP1["Pod: unifiedlogin-batchprocessorv2"]
            end

            CFGMAP["ConfigMap (non-secret config)"]
            SEC["K8s Secrets (connection strings, client secrets)"]
        end
    end

    subgraph "Shared Infrastructure"
        SQLDB["SQL Server (RCDUSODBSQL001)"]
        RedisDB["Redis (rcauneaprds101)"]
        IDP2["RealPage IdP"]
        LAUNCHDARKLY["LaunchDarkly (SaaS)"]
        ELASTIC["Elastic APM (OTLP)"]
    end

    Ingress --> API1
    Ingress --> API2
    Ingress --> EA1
    CFGMAP --> API1
    CFGMAP --> EA1
    CFGMAP --> BP1
    SEC --> API1
    SEC --> EA1
    SEC --> BP1
    APIHPA --> API1
    APIHPA --> API2
    EAHPA --> EA1
    API1 --> SQLDB
    API2 --> SQLDB
    EA1 --> SQLDB
    BP1 --> SQLDB
    API1 --> RedisDB
    EA1 --> RedisDB
    BP1 --> RedisDB
    API1 --> IDP2
    EA1 --> IDP2
    BP1 --> IDP2
    API1 --> LAUNCHDARKLY
    EA1 --> LAUNCHDARKLY
    BP1 --> LAUNCHDARKLY
    API1 --> ELASTIC
    EA1 --> ELASTIC
    BP1 --> ELASTIC
```

### 5.2 Kubernetes Resource Summary

| Resource | Type | Service | Key Settings |
|---|---|---|---|
| `unified-login-coreapiv2` | Deployment | LandingAPI | `replicas: $(K8s.LandingApiReplicas)`, CPU: 200m–500m, Mem: 256Mi–1Gi |
| `unified-login-coreenterpriseapiv2` | Deployment | LandingAPIEnterprise | Similar to LandingAPI |
| `unifiedlogin-batchprocessorv2` | Deployment | BatchProcessor | Single replica (worker service pattern) |
| `*-hpa` | HorizontalPodAutoscaler | LandingAPI, EnterpriseAPI | Auto-scales based on CPU/memory |
| `*-configmap` | ConfigMap | All | Non-secret environment config |
| `*-secrets` (from template) | Secret | All | DB connection strings, client secrets, Redis auth |
| `*-httproute` | HTTPRoute (Gateway API) | LandingAPI, EnterpriseAPI | L7 routing via gateway ingress |
| `*-service` | Service (ClusterIP) | All | Internal cluster routing |

**Security context (all pods):**
```yaml
securityContext:
  runAsNonRoot: true
  runAsUser: 10000
```

**Docker base image:** `mcr.microsoft.com/dotnet/aspnet:10.0-noble`
**Additional packages in image:** `gss-ntlmssp` (Kerberos/NTLM support for SQL Server auth)

Source: `CoreMigration/K8s/rancher/`

### 5.3 Container Image Build

Multi-stage Dockerfile (`Services/UnifiedLogin.BatchProcessor/Dockerfile`):

```mermaid
flowchart LR
    S1["Stage 1: base<br/>dotnet/aspnet:10.0-noble<br/>+ gss-ntlmssp"]
    S2["Stage 2: restore<br/>dotnet/sdk:10.0-noble<br/>dotnet restore linux-x64"]
    S3["Stage 3: publish<br/>dotnet publish -c Release<br/>linux-x64"]
    S4["Stage 4: final<br/>FROM base<br/>COPY published output"]

    S2 --> S3
    S3 --> S4
    S1 -.->|FROM base| S4
```

### 5.4 CI/CD Pipeline (Azure Pipelines)

Entry point: `CoreMigration/azure-pipelines.yml`

```mermaid
flowchart TD
    Trigger["Trigger: push to main / release/* / CoreMigration/*"]

    subgraph "Build Stage"
        Test["test-job.yml<br/>dotnet test + coverage"]
        Docker["docker-job.yml<br/>Build & push images to Artifactory"]
        K8sManifest["k8s-job.yml<br/>Process K8s manifest templates"]
        DB["db-job.yml<br/>Build DACPAC"]
    end

    subgraph "Deploy: DEV"
        DevDacpac["int-dacpac-job.yml<br/>Deploy DB schema"]
        DevK8s["k8s-job.yml<br/>kubectl apply manifests"]
    end

    subgraph "Deploy: QA"
        QADeploy["k8s-job.yml"]
    end

    subgraph "Deploy: SAT"
        SATDeploy["k8s-job.yml"]
    end

    subgraph "Deploy: LOAD"
        LoadDeploy["k8s-job.yml"]
    end

    subgraph "Deploy: PROD"
        ProdDacpac["dacpac-job.yml<br/>Deploy DB schema (PROD)"]
        ProdK8s["k8s-job.yml<br/>Deploy to PROD cluster"]
    end

    Trigger --> Test
    Trigger --> Docker
    Trigger --> K8sManifest
    Trigger --> DB

    Test --> DevDacpac
    Docker --> DevDacpac
    K8sManifest --> DevK8s
    DB --> DevDacpac
    DevDacpac --> DevK8s
    DevK8s --> QADeploy
    QADeploy --> SATDeploy
    SATDeploy --> LoadDeploy
    SATDeploy --> ProdDacpac
    ProdDacpac --> ProdK8s
```

**Build variables:**
- `BuildConfiguration: Release`
- `ArtifactoryDockerRegistry: artifacts.realpage.com/docker-virtual/uty/public/unifiedloginmaincore`
- `ArtifactoryDeployablesPath: rp-deployables-local/uty/unifiedloginmaincore/$(Build.BuildNumber)`

**Image tag format:** `$(K8s.LandingApiImage):$(BUILD_BUILDNUMBER)`

**SonarQube scan:** Enabled by default (parameter: `SonarQubeScan: true`)

---

## 6. Quality Attributes

### 6.1 Performance

| Tactic | Implementation | Location |
|---|---|---|
| Three-tier caching | IMemoryCache → Redis → DB | HybridCacheService; LandingAPI in-memory |
| Parallel batch processing | `Parallel.ForEachAsync` (configurable parallelism) | PendingBatchJob |
| Configuration pre-fetch | Batch configs loaded once per job cycle | PendingBatchJob:111-115 |
| Async throughout | All DB, HTTP, and cache operations are async | All repositories and services |
| Output caching for health | 10-second cache on /health, /alive | ServiceDefaults/Extensions.cs:125-128 |
| Sampling (tracing) | `TraceIdRatioBasedSampler` (1% default in DEV) | ServiceDefaults/Extensions.cs:77 |

### 6.2 Scalability

| Tactic | Implementation |
|---|---|
| Stateless API pods | No in-process session state; JWT tokens carry identity |
| Horizontal Pod Autoscaler | CPU/memory-based autoscaling for LandingAPI and EnterpriseAPI |
| Configurable batch parallelism | `MaxDegreeOfParallelism` and `InstanceCount` per job |
| Rate limiting | `IApiRateLimiter` prevents API throttling under load |
| Distributed cache (Redis) | Shared cache cluster-wide; L1 reduces Redis pressure |

### 6.3 Security

| Tactic | Implementation |
|---|---|
| Token-based auth | JWT Bearer + OIDC; no cookie-based sessions for APIs |
| SAML federation | Sustainsys.Saml2 for enterprise IdP federation |
| Client credentials (machine-to-machine) | IdentityModel.AspNetCore for BatchProcessor outbound calls |
| Non-root containers | `runAsUser: 10000` in K8s security context |
| Global authorization | `RequireAuthenticatedUser` applied to all routes |
| Secret injection | K8s secrets → env variables; not stored in images |
| CORS allowlisting | Explicit origin allowlist from config |
| Forwarded headers | Trusted proxy header extraction |

### 6.4 Reliability

| Tactic | Implementation |
|---|---|
| Retry with jitter | Polly exponential backoff: 2s, 4s, 8s + jitter |
| Circuit breaker | Opens at 10% failure rate; 30s break |
| Per-attempt + total timeout | 15s per attempt; 90s total |
| Error isolation | Try/catch per batch item in `Parallel.ForEachAsync` |
| Graceful shutdown | `OperationCanceledException` handling in all BackgroundServices |
| Cache fallback | HybridCache falls back to L1 if Redis unavailable |
| Health probes | K8s readiness/liveness probes → automatic pod restart/traffic removal |

### 6.5 Operability / Observability

| Tactic | Implementation |
|---|---|
| Structured logs (Serilog) | JSON-structured logs with `Enrich.FromLogContext()` |
| Distributed traces (OTel) | W3C TraceContext propagation; OTLP export |
| Metrics (OTel) | Request duration, throughput, runtime (GC, thread pool) |
| Custom batch metrics | `BatchProcessingMetrics` tracks job-level KPIs |
| Problem Details | RFC 7807 errors with `traceId` and `spanId` |
| Feature flag kill switch | LaunchDarkly flags disable jobs without deployment |
| Multiple health check endpoints | `/health`, `/alive` (ASP.NET Core); `/health/ready`, `/health/live` (BatchProcessor) |
| 13-environment configs | Per-env log levels, OTLP endpoints, CORS |

---

## 7. Migration Notes from .NET Framework 4.8

### 7.1 Hosting Model

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| Host | IIS / `System.Web.HttpRuntime` | Kestrel + `WebApplicationBuilder` (minimal APIs style) |
| Startup | `Global.asax` + `Startup.cs` | Top-level statements in `Program.cs` |
| Pipeline | `HttpModule` / `HttpHandler` chain | ASP.NET Core middleware pipeline |
| Worker | `IIS Application Initialization` or Windows Service | `BackgroundService` (IHostedService) |
| Orchestration | IIS App Pools | .NET Aspire (local), Kubernetes (production) |

**Key files:**
- [LandingAPI/Program.cs](../CoreMigration/UnifiedLogin.LandingAPI/Program.cs)
- [BatchProcessor/Program.cs](../CoreMigration/Services/UnifiedLogin.BatchProcessor/Program.cs)

### 7.2 Configuration

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| Primary config | `web.config` / `app.config` | `appsettings.json` + environment-specific overrides |
| Transforms | `web.{env}.config` XML transforms | `appsettings.{env}.json` JSON overrides (13 environments) |
| Runtime overrides | `appSettings` XML | Environment variables (double-underscore nesting) |
| Secrets | `web.config` encryption / Azure Key Vault | Kubernetes Secrets → environment variables |
| Static access | `ConfigurationManager.AppSettings["key"]` | `ConfigReader.Initialize(IConfiguration)` static shim (retained — see tech debt #7) |

### 7.3 Authentication & Authorization

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| Framework | OWIN / Katana middleware | ASP.NET Core auth middleware |
| JWT validation | `Microsoft.Owin.Security.Jwt` | `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.0` |
| OIDC | `Microsoft.Owin.Security.OpenIdConnect` | `Microsoft.AspNetCore.Authentication.OpenIdConnect 10.0.0` |
| SAML | (Assumption: legacy library) | `Sustainsys.Saml2 2.11.0` |
| Authorization | `[Authorize]` + custom HTTP modules | Global `AuthorizeFilter` + `[Authorize]` attributes |
| Token acquisition | Manual `HttpClient` | `IdentityModel.AspNetCore` (client credentials flow) |

### 7.4 Serialization

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| JSON library | Newtonsoft.Json | **Newtonsoft.Json retained** (backward compat) — `System.Text.Json` NOT adopted |
| Contract resolver | `CamelCasePropertyNamesContractResolver` | Same (unchanged) |
| Date format | `DateFormatConverter` (MS date format) | Same converter retained |
| Null handling | `NullValueHandling.Include` | Same (retained for API contract compat) |

### 7.5 HTTP Clients

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| Pattern | `new HttpClient()` per request / `ServicePointManager` | `HttpClientFactory` + `IProductApiClient` typed client |
| Resilience | Manual retry loops / custom Polly wiring | `Microsoft.Extensions.Http.Resilience` (Polly v8 internally) |
| Service discovery | Hard-coded base URLs | `Microsoft.Extensions.ServiceDiscovery` (Aspire) |
| `ServicePointManager` | Used for SSL / connection pool | Removed — Kestrel/HttpClientHandler handles this |

### 7.6 Logging

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| Framework | log4net / NLog (Assumption) | Serilog 4.3.0 |
| Integration | `log4net.config` or `NLog.config` | `Serilog.Settings.Configuration` (reads from `appsettings.json`) |
| Sinks | File / EventLog | OTLP (Elastic APM), Console, File (optional) |
| Enrichment | Manual properties | `Enrich.FromLogContext()` |
| Correlation | Manual / none | OpenTelemetry `traceId` propagation |

### 7.7 Dependency Injection

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| Container | Unity / Autofac / SimpleInjector (Assumption) | `Microsoft.Extensions.DependencyInjection` (built-in) |
| Registration | Bootstrapper / Global.asax | Extension methods (`.AddRepositories()`, `.AddBusinessLogicServices()`) |
| Lifetimes | Per-request / singleton / transient | Scoped / Singleton / Transient |
| Keyed services | Not available | `AddKeyedSqlServerClient("DBConnection")` (Aspire) |

### 7.8 Data Access

| Aspect | .NET Framework 4.8 | CoreMigration (net10.0) |
|---|---|---|
| Driver | `System.Data.SqlClient` | `Microsoft.Data.SqlClient 6.0.2` |
| Query library | Dapper or direct ADO.NET (Assumption) | Dapper 2.1.66 + `RealPage.DataAccess.Dapper 1.3.1` |
| Async | Synchronous / limited async | Full async (`GetManyAsync`, `ExecuteAsync`, cancellation tokens) |
| Connection management | Manual `Open()` / `Close()` | Keyed transient via Aspire `AddKeyedSqlServerClient` |

### 7.9 WCF / SOAP

| Aspect | Status |
|---|---|
| `System.ServiceModel.*` NuGet packages | Retained in `UnifiedLogin.SharedObjects` |
| CoreWCF | Not adopted |
| REST wrappers for WCF services | Not confirmed from reviewed code |
| **Recommendation** | Audit `SharedObjects` for actual WCF client usage; if unused, remove packages; if used, evaluate CoreWCF migration or REST wrapper |

### 7.10 Recommended Next Steps

| Priority | Recommendation |
|---|---|
| High | Activate remaining batch jobs (`RetryBatchJob`, `EnterpriseRolesJob`, `BulkUserUpdateJob`, etc.) once validated |
| High | Migrate dev credentials out of `appsettings.json` into `.NET User Secrets` or Azure Key Vault dev tier |
| High | Configure Kafka (`AddKafka()`) or remove `Confluent.Kafka` package if not in roadmap |
| Medium | Enable `Nullable` in LandingAPI, LandingAPIEnterprise, BatchProcessor to reduce NullReferenceException risk |
| Medium | Evaluate migrating from Newtonsoft.Json to `System.Text.Json` for performance improvements |
| Medium | Replace `ConfigReader.Initialize()` static shim with proper `IOptions<T>` injection |
| Medium | Audit `System.ServiceModel.*` usage in `SharedObjects`; remove or migrate to CoreWCF |
| Medium | Fix `BuildServiceProvider()` anti-pattern in `AddHostedServices()` — use `IOptions<T>` deferred resolution instead |
| Low | Upgrade from Polly 7.2.3 direct dependency to Polly v8 (align with `Microsoft.Extensions.Http.Resilience` internals) |
| Low | Enable `OTEL_EXPORTER_OTLP_HEADERS` in AppHost for local developer telemetry |

---

## Changelog

| Date | Author | Summary |
|---|---|---|
| 2026-02-26 | Claude Sonnet 4.6 (AI-generated) | Initial architecture documentation of `CoreMigration` branch. Includes C4 Context, Container, and Component views (Mermaid); four sequence diagrams (JWT auth, SAML SSO, batch processing, cache read); deployment view with K8s topology and Azure Pipelines CI/CD; quality attribute tactics table; and comprehensive .NET Framework 4.8 → net10.0 migration delta analysis with prioritized next steps. |
