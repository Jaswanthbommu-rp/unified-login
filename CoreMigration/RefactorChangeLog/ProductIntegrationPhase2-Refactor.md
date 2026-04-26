# ProductIntegration — Phase 2 Refactor Changelog

**Scope:** Core infrastructure helpers  
**Date:** 2026-04-14  
**Target framework:** .NET 10 / C# 13

---

## Files Created

| New file | Replaces |
|----------|---------|
| `LogicAsync/Interfaces/IApiIntegrationAsync.cs` | _(no prior interface existed)_ |
| `LogicAsync/Helpers/ApiIntegrationAsync.cs` | `Logic/ProductIntegration/Helpers/ApiIntegration.cs` |
| `LogicAsync/Helpers/DataCollectorAsync.cs` | `Logic/ProductIntegration/Helpers/DataCollector.cs` |

---

## Breaking Changes

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `new ApiIntegration(HttpClient client, string url)` — caller owns & mutates `HttpClient` lifetime | `IHttpClientFactory` injected; client created per-call inside the class — **IHttpClientFactory pattern** |
| 2 | `.GetAsync().Result`, `.SendAsync().Result`, `.DeleteAsync().Result`, `.ReadAsStringAsync().Result` — 5 blocking calls across `ApiIntegration` | All replaced with `await` — eliminates thread-pool starvation |
| 3 | `ProcessApiResponse` reads content synchronously | `BuildApiResponseAsync` uses `await response.Content.ReadAsStringAsync(ct)` |
| 4 | `new DataCollector()` — all 4 repository fields set with `new ProductRepository()`, `new SamlRepository()`, etc. | **IDbConnectionFactory pattern**: 4 async repositories injected via constructor; repositories internally use `IDbConnectionFactory` for short-lived `SqlConnection`s |
| 5 | `GetProductCompanyMap(…, DefaultUserClaim userClaims, …)` — builds `new ManageBlueBook(userClaims)` per call | `DefaultUserClaim` removed; `IManageBlueBookAsync` injected; org GUID from `IUserClaimsAccessor.OrganizationRealPageGuid` |
| 6 | Sync `void` write operations (`CreateProductUserInGreenBook`, `UpdateProductUserInGreenBook`, `CreateSamlUserAttribute`, `UpdateSamlUserAttribute`, `UpdateProductSettingProductStatus`, `AddUpdateEmployeeProductADGroupMapping`) | All return `Task` — failures are observable and awaitable |
| 7 | `new HttpMethod("PATCH")` | `HttpMethod.Patch` — static property available since .NET 5 |
| 8 | Silent exception swallow in `GetProductCompanyMap` — re-throws a bare `new Exception(ex.Message, ex)` losing the stack type | Logs via `ILogger<DataCollectorAsync>` then re-throws original exception (stack preserved) |
| 9 | `T GetEntityFromApi<T>` returns `null` on failure without annotation | `T? GetEntityFromApiAsync<T>` — nullable return type declared explicitly (NRT compliance) |

---

## Constructor: Dependencies

### `ApiIntegrationAsync` — from 1 to 4

| # | Dependency | Purpose |
|---|-----------|---------|
| 1 | `IHttpClientFactory httpClientFactory` | Creates named `"ProductIntegration"` clients per-call; replaces storing a shared `HttpClient` |
| 2 | `string baseUrlAndQuery` | Target URL (same as sync version) |
| 3 | `ILogger<ApiIntegrationAsync> logger` | Structured error/warning logging |
| 4 | `AuthenticationHeaderValue? authHeader = null` | Optional pre-built Basic/Bearer auth applied to each factory-created client |

### `DataCollectorAsync` — from 0 to 7

| # | Dependency | Purpose |
|---|-----------|---------|
| 1 | `IProductRepositoryAsync` | `GetBooksMasterProductDetailAsync`, `GetProductInternalSettingsAsync`, `ListProductSettingTypeAsync`, `CreateProductSettingAsync` |
| 2 | `ISamlRepositoryAsync` | `GetSamlProductAttributesAsync`, `GetProductSamlDetailsAsync`, `CreateSamlUserAttributeAsync`, `UpdateSamlUserAttributeAsync` |
| 3 | `IUserRepositoryAsync` | `GetUserDetailsAsync`, `GetAzureUserDetailsAsync`, `GetEmployeeProductADGroupMappingAsync`, `AddUpdateEmployeeProductADGroupMappingAsync` |
| 4 | `ITelecommunicationNumberRepositoryAsync` | `ListTelecommunicationNumberForPersonAsync` — phone number enrichment for user details |
| 5 | `IManageBlueBookAsync` | `GetCompanyMapAsync` — replaces `new ManageBlueBook(userClaims)` per call |
| 6 | `IUserClaimsAccessor` | `OrganizationRealPageGuid` — replaces `DefaultUserClaim.OrganizationRealPageGuid` in `GetProductCompanyMapAsync` |
| 7 | `ILogger<DataCollectorAsync>` | Structured logging for lookup failures |

All 7 guarded with `ArgumentNullException.ThrowIfNull`.

---

## IHttpClientFactory Pattern

```csharp
// Old — HttpClient stored as field, mutated externally, risk of socket exhaustion
public class ApiIntegration
{
    private readonly HttpClient _client;  // owned by caller, long-lived
    public T GetEntityFromApi<T>()
        => JsonConvert.DeserializeObject<T>(_client.GetAsync(url).Result.Content.ReadAsStringAsync().Result);
}

// New — factory creates short-lived clients from a pooled handler
public sealed class ApiIntegrationAsync : IApiIntegrationAsync
{
    private readonly IHttpClientFactory _httpClientFactory;

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("ProductIntegration");
        if (_authHeader is not null)
            client.DefaultRequestHeaders.Authorization = _authHeader;
        return client;
    }

    public async Task<T?> GetEntityFromApiAsync<T>(bool isThrowOnError = true, CancellationToken ct = default)
    {
        var client   = CreateClient();
        var response = await client.GetAsync(_baseUrlAndQuery, ct);
        var content  = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<T>(content, JsonSettings);
    }
}
```

**DI registration:**
```csharp
services.AddHttpClient(ApiIntegrationAsync.ClientName);   // "ProductIntegration"
services.AddTransient<IApiIntegrationAsync, ApiIntegrationAsync>();
```

---

## IDbConnectionFactory Pattern (DataCollectorAsync)

```csharp
// Old — hard-coded new X() repository instantiation, each holds its own connection config
private readonly IProductRepository _productRepository = new ProductRepository();
private readonly ISamlRepository    _samlRepository    = new SamlRepository();
private readonly IUserRepository    _userRepository    = new UserRepository();
private readonly ITelecommunicationNumberRepository _telecomRepository = new TelecommunicationNumberRepository();

// New — injected async repositories; each internally uses IDbConnectionFactory
//       to get a connection from the ADO.NET pool only for the duration of a query
public DataCollectorAsync(
    IProductRepositoryAsync                productRepository,
    ISamlRepositoryAsync                   samlRepository,
    IUserRepositoryAsync                   userRepository,
    ITelecommunicationNumberRepositoryAsync telecomRepository,
    IManageBlueBookAsync                   manageBlueBook,
    IUserClaimsAccessor                    userClaimsAccessor,
    ILogger<DataCollectorAsync>             logger)
```

**DI registration:**
```csharp
services.AddScoped<IDataCollectorAsync, DataCollectorAsync>();
// All 6 repository/service dependencies already registered by other product-service registrations.
```

---

## `ResolveAttributeValue` — Extracted Helper (C# 13 `switch` expression)

The duplicated `if/else if` chains in `CreateProductUserInGreenBook` and `UpdateProductUserInGreenBook` (~40 lines each) are collapsed into a single `private static` `switch` expression used by both methods:

```csharp
// Old — duplicated in both methods
if (samlEnum.Equals(SamlAttributeEnum.productUsername))
    samlAttributeValue = newProductLoginName;
else if (samlEnum.Equals(SamlAttributeEnum.UserId))
    samlAttributeValue = newid;
else if (samlEnum.Equals(SamlAttributeEnum.organization_id))
{
    var organizationId = productInternalSettingList.FirstOrDefault(...)?.Value;
    if (organizationId != null) samlAttributeValue = organizationId;
    else continue;
}
// ... 10 more lines ...

// New — single static switch expression shared by both methods
private static string? ResolveAttributeValue(
    SamlAttributeEnum samlEnum,
    string userId, string loginName,
    IntegrationProductUser productUser,
    IList<ProductInternalSetting> settings) => samlEnum switch
{
    SamlAttributeEnum.productUsername => loginName,
    SamlAttributeEnum.UserId          => userId,
    SamlAttributeEnum.RoleCode        => productUser.RoleType,
    SamlAttributeEnum.organization_id => settings.FirstOrDefault(s => s.Name.Equals("OrganizationId", ...))?.Value,
    SamlAttributeEnum.portal_id       => settings.FirstOrDefault(s => s.Name.Equals("PortalId", ...))?.Value,
    _                                 => null   // unknown — caller skips via null check
};
```

**Returns `null` for unknown attributes** — the `foreach` loop skips them, matching the `continue` behaviour of the original.

---

## Performance Improvements

| Area | Improvement |
|------|-------------|
| Thread-pool starvation | 5 × `.Result` blocking calls in `ApiIntegration` → `await` |
| Socket exhaustion | `IHttpClientFactory` named client with handler pooling replaces stored `HttpClient` field |
| Per-call `new ManageBlueBook(userClaims)` | Eliminated — `IManageBlueBookAsync` singleton injected |
| Per-call `new ProductRepository()` etc. (4 types) | Eliminated — async repos injected, connections pooled via `IDbConnectionFactory` |
| Duplicated SAML attribute loop (~80 lines) | Collapsed to `ResolveAttributeValue` static switch — one allocation path |
| `new Dictionary<SamlAttributeEnum, string> { {attrType, newValue} }` per `UpdateSamlUserAttribute` call | C# 13 collection expression: `new Dictionary<…> { [key] = value }` — same perf, cleaner syntax |
| `productSettingTypes.Any(a => ...) + FirstOrDefault(...)` — double-scan | Single `FirstOrDefault` in `UpdateProductSettingProductStatusAsync` |

---

## C# 13 Features Used

| Feature | Where |
|---------|-------|
| File-scoped namespaces | Both files |
| `sealed class` | `ApiIntegrationAsync`, no virtual dispatch overhead |
| `ArgumentNullException.ThrowIfNull` | All constructor parameters |
| `ArgumentException.ThrowIfNullOrWhiteSpace` | `_baseUrlAndQuery` in `ApiIntegrationAsync` ctor |
| Target-typed `new()` | `JsonSerializerSettings`, `ApiResponse`, `Dictionary` initialisers |
| Collection expressions `[]` and `??= []` | `companyList ??= []` in `GetProductCompanyMapAsync` |
| `switch` expression | `ResolveAttributeValue` — maps `SamlAttributeEnum` → value |
| Pattern: `samlAttrs is { Count: > 0 }` | Null + empty check in one pattern |
| `foreach (var (attrEnum, value) in settingList)` | Deconstruction of `Dictionary<K,V>` entries |
| `?. FirstOrDefault()?.Value` | Null-conditional chains replacing nested `if` blocks |
| `string?` nullable annotations | `GetBlueBookProductMapAsync`, `GetProductCompanyMapAsync`, `GetUserDetailsByPersonaAsync`, `GetAzureUserDetailsAsync`, `ResolveAttributeValue` |
| `HttpMethod.Patch` | Replaces `new HttpMethod("PATCH")` |

---

## What Phase 2 Does NOT Change

- `HttpClientExtensions` (`SetBasicAuthentication`, `SetBearerToken`) — still used to build the `AuthenticationHeaderValue` passed to `ApiIntegrationAsync` constructor; no changes needed.
- `PasswordGenerator` — pure static utility, no async required.
- `ProductActivityLogger` — calls `LogActivity.WriteActivity()` which is a fire-and-forget audit sink; not changed in Phase 2.
- `ApiResponse` DTO class — reused from `Logic/ProductIntegration/Helpers/ApiIntegration.cs`; no duplication.

---

## DI Registration Summary

```csharp
// HTTP
services.AddHttpClient(ApiIntegrationAsync.ClientName);  // "ProductIntegration"

// Phase 2 services
services.AddScoped<IDataCollectorAsync, DataCollectorAsync>();

// ApiIntegrationAsync is instantiated per-call (not DI-registered directly)
// because it holds a per-call URL. StandardV1ProductIntegrationAsync (Phase 3)
// injects IHttpClientFactory and constructs ApiIntegrationAsync with the
// resolved URL and auth header per operation.
```
