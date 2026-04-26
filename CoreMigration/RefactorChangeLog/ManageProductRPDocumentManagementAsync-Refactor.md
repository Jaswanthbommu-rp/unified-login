# ManageProductRPDocumentManagementAsync – Refactor Changelog

## Source
`UnifiedLogin.BusinessLogic/Logic/Product/ManageProductRPDocumentManagement.cs`

## Output
| File | Action |
|------|--------|
| `LogicAsync/Interfaces/IManageProductRPDocumentManagementAsync.cs` | Already existed — 9-method true-async interface (no `DefaultUserClaim`) |
| `LogicAsync/Product/ManageProductRPDocumentManagementAsync.cs`     | Created — native async implementation (~400 lines) |
| `LogicAsync/ManageProductRPDocumentManagementAsync.cs`             | Namespace-only placeholder (was already in place) |

---

## Breaking Changes

### `DefaultUserClaim` removed from all public APIs
All public methods previously received `DefaultUserClaim` either as a constructor arg or
implicit via the base class.  Context (editor persona, user persona, product usernames) is
now resolved internally by `IProductContextServiceAsync.GetUserContextAsync`.

### `out List<AdditionalParameters>` → tuple return
`ManageRPDMUser` had an `out additionalParameters` parameter (incompatible with `async`).
`ManageRPDMUserAsync` returns
`Task<(string result, List<AdditionalParameters> auditParams)>`.

---

## Constructor — from 2 to 11 dependencies

| # | Dependency | Replaces |
|---|-----------|---------|
| 1 | `IProductContextServiceAsync` | `DefaultUserClaim` + `GetCompanyEditorAndUserDetails` |
| 2 | `IProductRepositoryAsync` | `_productRepository` (sync) + `UpdateProductSettingProductStatus` (base) |
| 3 | `ISamlAttributeServiceAsync` | `_samlRepository.CreateSamlUserAttribute` (inline calls) |
| 4 | `IManageBlueBookAsync` | `_blueBook.GetProductCompanyInstanceId` (base helper) |
| 5 | `IManagePersonaAsync` | `_managePersona.GetPersona` |
| 6 | `IManagePersonAsync` | `_managePerson.GetPerson` |
| 7 | `IManageUserLoginAsync` | `_manageUserLogin.GetUserLoginOnly` |
| 8 | `IManageContactMechanismAsync` | `_manageContactMechanism.ListContactMechanismForPerson` |
| 9 | `IHttpClientFactory` | `HttpClient _client` (shared instance with mutable auth header) |
| 10 | `IMemoryCache` | `RPObjectCache` (custom in-process cache wrapper) |
| 11 | `ILogger<>` | `WriteToDiagnosticLog` / `WriteToErrorLog` base helpers |

`ArgumentNullException.ThrowIfNull` guard applied to every dependency.

---

## Mutable instance fields eliminated

| Old field | Problem | New approach |
|-----------|---------|-------------|
| `string _productUrl` | Set in constructor, shared | Resolved per-call from `RpdmConfig` |
| `string _productUserId` | Mutated by `GetCompanyEditorAndUserDetails` | `ctx.ProductUserId` from `ProductCallContext` |
| `string _productUsername` | Mutated during username-generation loop | Local variable per call |
| `DefaultUserClaim _userClaims` | Source of editor/user identity | Replaced by `IProductContextServiceAsync` |
| `List<ProductInternalSetting> _unifiedLoginSettings` | Constructor-loaded, stale after deploy | Loaded per `ResolveDomainAsync` call, cached in `IMemoryCache` |

---

## Blocking `.Result` calls — all eliminated

| Original | Async replacement |
|----------|-------------------|
| `_client.GetAsync(url).Result` | `await client.GetAsync(url, ct)` in `GetFromApiAsync<T>` |
| `_client.PostAsJsonAsync(url, obj).Result` | `await client.PostAsync(url, content, ct)` in `PostToApiAsync` |
| `postResponse.Content.ReadAsStringAsync().Result` | `await response.Content.ReadAsStringAsync(ct)` |
| `GetResultFromApi<T>(...)` (sync, blocks internally) | `await GetFromApiAsync<T>(...)` |
| `ApiIntegration.PatchEntity<T>(...)` (sync) | `await client.PatchAsync(url, content, ct)` |
| `RPObjectCache.GetFromCache(...)` blocking callback | `IMemoryCache.GetOrCreateAsync(...)` async factory |

---

## HTTP client

### `IHttpClientFactory` with named client `"RPDocumentManagement"`
The shared `HttpClient _client` (with `SetBasicAuthentication` applied globally in the
constructor) is replaced with a per-request client from `IHttpClientFactory`.

```csharp
services.AddHttpClient("RPDocumentManagement");
services.AddScoped<IManageProductRPDocumentManagementAsync, ManageProductRPDocumentManagementAsync>();
```

Basic auth is set on the per-request instance:
```csharp
var creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.ApiUsername}:{config.ApiPassword}"));
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);
```

`PostAsJsonAsync` is replaced with
`new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")`.

---

## Domain resolution

`GetDomain()` (sync, read `_unifiedLoginSettings` populated at construction time) is
replaced by `ResolveDomainAsync(ctx, ct)`:

1. Checks `IMemoryCache` under `RPDM_Domain_{orgRealPageId}` — returns cached value without I/O.
2. On miss: loads `UnifiedPlatform` product settings via `IProductRepositoryAsync.GetProductInternalSettingsAsync`.
3. Checks `BooksUseUPFMId` flag.
4. Calls `IManageBlueBookAsync.GetCompanyMapAsync(orgRealPageId, orgPartyId, "DOC", "", "companyInstance.attributes")` to include instance attributes.
5. If `BooksUseUPFMId == "1"` → uses `CompanyInstanceSourceId` directly.
6. Otherwise → finds the `"DOMAIN ID"` attribute in `CompanyInstance[0].Attributes`.
7. Caches result for 5 minutes.

---

## N+1 API call pattern eliminated — `GetAdditionalRoleDetailsAsync`

### Original (N+1 serial)
```csharp
foreach (RPDMRole role in rpdmRoleResult.Page)
{
    RPDMRoleDetail detail = GetResultFromApi<RPDMRoleDetail>("/roles/" + role.ID);  // blocks per role
    ...
}
```

### New (parallel)
```csharp
var tasks = rpdmRoleResult.Page.Select(async role =>
{
    var detail = await GetFromApiAsync<RPDMRoleDetail>(config, domain, $"/roles/{role.ID}", ct);
    ...
});
var results = await Task.WhenAll(tasks);  // all role-detail calls in flight simultaneously
```

For a company with 20 roles this reduces wall-clock latency from `20 × RTT` to `1 × RTT`.
The same parallel pattern is applied to role-detail fetches inside `ManageRPDMUserAsync`.

---

## Caching improvements

| Cache key | TTL | Replaces |
|-----------|-----|---------|
| `RPDM_ProductConfig` | 1 hour | Constructor-loaded `_productInternalSettingList` |
| `RPDM_Domain_{orgRealPageId}` | 5 min | `GetDomain()` (no caching in sync) |
| `RPDM_Classifier_{orgPartyId}_{roleId}` | 5 min | `RPObjectCache "DocumentDirector_Roles_..."` (300 s) |

`GetProductConfigAsync` returns `ValueTask<RpdmConfig>` — avoids `Task` allocation on every
method call when config is already cached.

---

## Config snapshot

```csharp
internal sealed record RpdmConfig(string ApiEndpoint, string ApiUsername, string ApiPassword);
```

Credentials are decoded from Base64 once (on cache miss) and stored in the immutable record.
`sealed record` for value-equality and JIT devirtualisation.

---

## Person / login / contact — parallel lookup

In both `ManageRPDMUserAsync` and `UpdateRPDMUserProfileAsync`, the three identity
lookups now run concurrently:
```csharp
var personTask  = _managePerson.GetPersonAsync(realPageId, ct);
var loginTask   = _manageUserLogin.GetUserLoginOnlyAsync(realPageId, ct);
var contactTask = _manageContactMechanism.ListContactMechanismForPersonAsync(realPageId, null, ct);
await Task.WhenAll(personTask, loginTask, contactTask);
```

---

## C# 13 / .NET 10 improvements

| Feature | Usage |
|---------|-------|
| C# 13 collection expressions `[]` | All `new List<>()` initializers; `[.. list.OrderBy(...)]` for sorted copy |
| `ValueTask<RpdmConfig>` | `GetProductConfigAsync` — zero Task allocation on cache hit |
| `when (ex is not OperationCanceledException)` | All catch guards — avoids swallowing cancellation |
| `is not null` / `is null` patterns | Null guards throughout |
| `HashSet<string>` for audit diff | Role/property diff in `ManageRPDMUserAsync` — O(1) set operations |
| Structured logging `{Placeholder}` | All `_logger` calls — no string interpolation allocations |
| `sealed record RpdmConfig` | Immutable, cache-friendly, value-equality |
| `sealed class` | `ManageProductRPDocumentManagementAsync` — JIT devirtualisation |
| `[^1]` index-from-end | Parse last HREF segment: `href.Split('/')[^1]` |
| `Task.WhenAll` | Parallel role-detail fetches and parallel identity lookups |
| `GetOrCreateAsync` async factory | Classifier cache — no hidden `.Result` inside cache callback |
| `string.Equals(..., StringComparison.OrdinalIgnoreCase)` | All case-insensitive compares |
| `Math.Min` + `Substring` | Safe username truncation to 19 chars |

---

## DI Registration (required)

```csharp
// Program.cs / ServiceCollectionExtensions.cs

services.AddHttpClient("RPDocumentManagement");

services.AddScoped<IManageProductRPDocumentManagementAsync, ManageProductRPDocumentManagementAsync>();
```
