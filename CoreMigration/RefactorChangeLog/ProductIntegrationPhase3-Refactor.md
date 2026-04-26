# ProductIntegration — Phase 3 Refactor Changelog

**Scope:** `StandardV1ProductIntegration` base class  
**Date:** 2026-04-14  
**Target framework:** .NET 10 / C# 13

---

## Files Created / Modified

| Action | File | Replaces / Notes |
|--------|------|-----------------|
| **Created** | `LogicAsync/Helpers/StandardV1ProductIntegrationAsync.cs` | `Logic/ProductIntegration/ProductImplementation/StandardV1ProductIntegration.cs` |
| **Modified** | `LogicAsync/Helpers/ApiIntegrationAsync.cs` | Added `IReadOnlyDictionary<string, string>? additionalHeaders` constructor param |
| **Existing** | `LogicAsync/Interfaces/ITokenHelperAsync.cs` | Already present from prior work |
| **Existing** | `LogicAsync/Helpers/TokenHelperAsync.cs` | Already present — `ITokenHelperAsync` implementation |

---

## Breaking Changes

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `DefaultUserClaim _userClaims` — passed to constructor, used throughout | `IUserClaimsAccessor` injected — all `_userClaims.*` reads route through accessor; `CorrelationId` from `_userClaimsAccessor.CorrelationId` |
| 2 | `HttpClient _httpClient` — stored as field, mutated by `ApplyApiSecurity`, shared across all calls | Eliminated — per-call `ApiIntegrationAsync` instances created via `IHttpClientFactory` + stored `_authHeader` and `_apiAdditionalHeaders` |
| 3 | `ApplyApiSecurity()` — 2 × `.Result` blocking calls in password-grant branch + `new HttpClient()` | `ResolveApiSecurityAsync()` — all HTTP calls awaited; returns `(AuthenticationHeaderValue?, IReadOnlyDictionary<string,string>)` |
| 4 | `RPObjectCache` in `CheckForOverrideCompanyIdForProduct` — 300s TTL, static shared state | `ICacheService.GetOrSetAsync` — same TTL, DI-managed, no static state |
| 5 | `new ProductRepository()`, `new ProductInternalSettingRepository()`, `new ManagePersona()`, `new ManageUserLogin()`, `new UserLoginRepository()` | All replaced with injected `IProductRepositoryAsync`, `IManagePersonaAsync`, `IManageUserLoginAsync` |
| 6 | `new TokenHelper()` in `ApplyApiSecurity` — blocking `RequestClientCredentialsToken` via `HttpClient.Send()` | `ITokenHelperAsync.GetUnifiedLoginServerTokenAsync` — fully async, cached in `ICacheService` |
| 7 | `string CreateUpdateProductUser(..., out List<AdditionalParameters> additionalParameters, ...)` — `out` incompatible with async | `Task<(string result, List<AdditionalParameters> auditParams)> CreateUpdateProductUserAsync(...)` |
| 8 | `string CreateUser(..., out List<AdditionalParameters> additionalParameters, ...)` | `Task<(string result, List<AdditionalParameters> auditParams)> CreateUserAsync(...)` |
| 9 | `string UpdateUser(..., out List<AdditionalParameters> additionalParameters, ...)` | `Task<(string result, List<AdditionalParameters> auditParams)> UpdateUserAsync(...)` |
| 10 | `string CreateMultiCompanyUser(..., out ...)` | `Task<(string, List<AdditionalParameters>)> CreateMultiCompanyUserAsync(...)` |
| 11 | Serilog `Log.Logger.ForContext("ProductModule", ...).ForContext("CorrelationId", ...).Write(...)` | `ILogger<StandardV1ProductIntegrationAsync>` structured logging via `ILoggerFactory` |
| 12 | Constructor calls `Init()` which performs all expensive initialisation synchronously | Protected constructor assigns dependencies only; `InitAsync()` must be awaited by the caller (Phase-6 factory) |

---

## Constructor Dependencies

### Old — `StandardV1ProductIntegration` (2 constructors, fields newed internally)

```csharp
public StandardV1ProductIntegration(
    int productId, long editorPersonaId, long subjectPersonaId,
    DefaultUserClaim userClaims)     // all repos instantiated with new() inside
```

### New — `StandardV1ProductIntegrationAsync` (1 protected constructor + 9 injected deps)

| # | Dependency | Purpose |
|---|-----------|---------|
| 1 | `int productId` | Per-operation product identifier |
| 2 | `long editorPersonaId` | Persona of the acting editor |
| 3 | `long subjectPersonaId` | Persona of the target user (0 = editor-only operation) |
| 4 | `IDataCollectorAsync dataCollector` | User details, SAML, BlueBook lookups |
| 5 | `IProductRepositoryAsync productRepository` | Internal settings, AD groups, all-products list |
| 6 | `IManagePersonaAsync managePersona` | Persona validation + employee persona lookup |
| 7 | `IManageUserLoginAsync manageUserLogin` | `UnassignUser` — org status, user login lookup |
| 8 | `IUserClaimsAccessor userClaimsAccessor` | Replaces `DefaultUserClaim` — org GUID, correlation ID |
| 9 | `IHttpClientFactory httpClientFactory` | Per-call `ApiIntegrationAsync` clients + password-grant HTTP |
| 10 | `ITokenHelperAsync tokenHelper` | Bearer token for `TokenAuthScopes` products |
| 11 | `ICacheService cacheService` | OverridePMCID org settings cache (300s TTL) |
| 12 | `ILoggerFactory loggerFactory` | Creates `ILogger<StandardV1ProductIntegrationAsync>` + `ILogger<ApiIntegrationAsync>` |

All 9 DI deps guarded with `ArgumentNullException.ThrowIfNull`.

---

## Async Initialization Pattern

```csharp
// Old — constructor blocks synchronously on 4 expensive operations
public StandardV1ProductIntegration(int productId, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims)
{
    _dataCollector = new DataCollector();       // new()
    Init(productId, editorPersonaId, subjectPersonaId, userClaims);  // blocks here
}

// New — constructor assigns only; InitAsync() performs the 4 async operations
protected StandardV1ProductIntegrationAsync(...deps...) { /* assign fields */ }

public async Task InitAsync(CancellationToken ct = default)
{
    CorrelationId = _userClaimsAccessor.CorrelationId;
    await ValidateAndLoadUserDetailsAsync(editorPersonaId, subjectPersonaId, ct);
    await LoadProductEndpointDetailsAsync(ct);
    await LoadBlueBookAndCompanyDetailsAsync(subjectPersonaId, ct);
    (_authHeader, _apiAdditionalHeaders) = await ResolveApiSecurityAsync(ct);
}
```

**Phase-6 factory usage:**
```csharp
var integration = new SomeProductIntegrationAsync(productId, editorId, subjectId, ...deps...);
await integration.InitAsync(ct);
return integration;
```

---

## `ResolveApiSecurityAsync` — Replaces `ApplyApiSecurity`

```csharp
// Old — 2 × .Result blocking + new HttpClient() + RPObjectCache
protected virtual void ApplyApiSecurity()
{
    _httpClient = new HttpClient();
    // ...
    if (tokenScopes != null)
    {
        _tokenHelper = new TokenHelper();                          // new()
        var token = _tokenHelper.GetUnifiedLoginServerToken(...); // blocking
        _httpClient.SetBearerToken(token);
    }
    else if (!string.IsNullOrEmpty(apiSecret) ...)
    {
        using var client = new HttpClient();                        // new HttpClient per call
        var response = client.PostAsync(tokenURL, request).Result; // .Result blocking
        var json     = response.Content.ReadAsStringAsync().Result; // .Result blocking
        _httpClient.SetBearerToken(values["access_token"]);
    }
}

// New — fully async; returns auth header + additional headers dict
protected virtual async Task<(AuthenticationHeaderValue?, IReadOnlyDictionary<string, string>)>
    ResolveApiSecurityAsync(CancellationToken ct)
{
    if (tokenScopes != null)
    {
        string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync(tokenScopes, ct); // await
        return (new AuthenticationHeaderValue("Bearer", token), additionalHeaders);
    }
    else if (apiSecret + clientId present)
    {
        using var client = _httpClientFactory.CreateClient(...);   // IHttpClientFactory
        var resp = await client.SendAsync(req, ct);               // await
        var json = await resp.Content.ReadAsStringAsync(ct);      // await
        return (new AuthenticationHeaderValue("Bearer", values["access_token"]), additionalHeaders);
    }
    else if (apiUser + apiPassword)
        return (new AuthenticationHeaderValue("Basic", base64), additionalHeaders);

    return (null, additionalHeaders);
}
```

---

## `ApiIntegrationAsync` — Additional Headers Support

Phase 3 adds product-specific header support (e.g. `apikey`, `company-id`) to `ApiIntegrationAsync`:

```csharp
// Added optional parameter to ApiIntegrationAsync constructor
public ApiIntegrationAsync(
    IHttpClientFactory                     httpClientFactory,
    string                                 baseUrlAndQuery,
    ILogger<ApiIntegrationAsync>           logger,
    AuthenticationHeaderValue?             authHeader        = null,
    IReadOnlyDictionary<string, string>?   additionalHeaders = null)   // NEW in Phase 3

// Applied in CreateClient():
private HttpClient CreateClient()
{
    var client = _httpClientFactory.CreateClient(ClientName);
    if (_authHeader is not null)
        client.DefaultRequestHeaders.Authorization = _authHeader;
    if (_additionalHeaders is not null)                              // NEW in Phase 3
        foreach (var (key, value) in _additionalHeaders)
            client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
    return client;
}
```

---

## `out` Parameters → Named Tuples

| Sync method | Async replacement |
|------------|------------------|
| `string CreateUpdateProductUser(…, out List<AdditionalParameters> ap, …)` | `Task<(string result, List<AdditionalParameters> auditParams)> CreateUpdateProductUserAsync(…)` |
| `string CreateUser(…, out List<AdditionalParameters> ap, …)` | `Task<(string, List<AdditionalParameters>)> CreateUserAsync(…)` |
| `string UpdateUser(…, out List<AdditionalParameters> ap, …)` | `Task<(string, List<AdditionalParameters>)> UpdateUserAsync(…)` |
| `string CreateMultiCompanyUser(…, out List<AdditionalParameters> ap)` | `Task<(string, List<AdditionalParameters>)> CreateMultiCompanyUserAsync(…)` |

---

## Performance Improvements

| Area | Improvement |
|------|-------------|
| Constructor blocking | `Init()` was 4 synchronous blocking operations — now fully async via `InitAsync()` |
| `ApplyApiSecurity` 2 × `.Result` | `ResolveApiSecurityAsync` — both `PostAsync` and `ReadAsStringAsync` are awaited |
| `new HttpClient()` per password-grant call | `IHttpClientFactory.CreateClient(...)` with handler pooling |
| `new TokenHelper()` + blocking token HTTP | `ITokenHelperAsync` — async + `ICacheService` with 5-min TTL |
| `new DataCollector()`, `new ProductRepository()`, etc. | Eliminated — all injected |
| `RPObjectCache` (static, non-cancellable) | `ICacheService.GetOrSetAsync` — DI-managed, cancellation-aware |
| Serilog `Log.Logger.ForContext(...)` per log call | `ILogger<T>` structured logging — no per-call logger chain building |
| `new ManagePersona()` in `ValidateEditorUser` | `IManagePersonaAsync` injected — reuses DI lifetime |

---

## C# 13 Features Used

| Feature | Where |
|---------|-------|
| File-scoped namespaces | `StandardV1ProductIntegrationAsync.cs` |
| `sealed` (on `CreateApi` return) | `ApiIntegrationAsync` is `sealed` — no virtual dispatch |
| `ArgumentNullException.ThrowIfNull` | All 9 constructor parameters |
| Collection expression `[]` | `ProductInternalSettingList = []`, empty audit lists, `usedGroupIds` HashSet |
| Target-typed `new()` | `CacheEntryOptions`, `Dictionary<string,string>`, `HashSet<int>` |
| Pattern `is { Count: > 0 }` | Null-and-empty checks throughout |
| Pattern `is { LoginName: { Length: > 0 } }` | User-exists check in `CreateUpdateProductUserAsync` |
| `is not null` pattern | Persona / result guards |
| `??=` + `??` null-coalescing | `ProductApiBaseUrl`, `GetValueOrDefault`, empty fallbacks |
| Switch expression for `batchProcessType` | `UpdateUserAsync` batch type check uses `is ... or ...` pattern |
| `foreach var (key, value) in` deconstruction | `_additionalHeaders` iteration in `CreateClient()` |
| `string?` nullable annotations | All return types where null is a valid signal |
| `List<T>` collection expression initialiser `[...]` | `productUser.Roles = [roleId]` in `ApplySuperUserSettingsAsync` |
| `params object?[]` → structured log | `_logger.LogDebug/Error` calls |

---

## Logging Migration (Serilog → ILogger)

```csharp
// Old — builds a per-call logger chain via Serilog statics
var logger = Log.Logger;
logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData), false);
logger = logger.ForContext("ProductModule", this.GetType());
logger = logger.ForContext("CorrelationId", CorrelationId.ToString());
logger.Write(level: logType, exception: exception, messageTemplate: message, ...);

// New — ILogger<T> structured logging; context in message template
_logger.LogDebug("[{Method}] Product={ProductId} Editor={EditorId} {State}",
    method, ProductId, EditorUserDetails?.PersonaId, state);

_logger.LogError(ex, "[{Method}] Product={ProductId} Editor={EditorId} {State}",
    method, ProductId, EditorUserDetails?.PersonaId, state);
```

---

## DI Registration Summary

```csharp
// Token + caching (already registered by Phase 2 / TokenHelper registration)
services.AddHttpClient("TokenHelper");
services.AddScoped<ITokenHelperAsync, TokenHelperAsync>();

// Phase 3 — StandardV1ProductIntegrationAsync is NOT directly DI-registered.
// It is an abstract base class; Phase-6 factory creates concrete instances
// with the per-operation (productId, editorPersonaId, subjectPersonaId) params
// and calls InitAsync() before use.
//
// Concrete implementations (Phase 4) inherit from StandardV1ProductIntegrationAsync:
//   services.AddTransient<ISelfGuidedTourIntegrationAsync, SelfGuidedTourAsync>();
//   ... etc.
```

---

## What Phase 3 Does NOT Change

- `IManageProductIntegrationAsync` interface — defined in Phase 1, unchanged.
- `DataCollectorAsync`, `ApiIntegrationAsync` — Phase 2, only minor `additionalHeaders` addition to `ApiIntegrationAsync`.
- `ProductActivityLogger` — fire-and-forget audit sink, not changed.
- `ApiResponse` DTO — reused from `Logic/ProductIntegration/Helpers/ApiIntegration.cs`.
- `ProductEntityEndpointKeyEnum` — reused from sync layer.
- Merge helpers (`MergeUserRoles`, `MergeUserProperties`, etc.) — ported as-is (pure logic, no async needed).
