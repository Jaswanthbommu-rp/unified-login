# ManageProductVendorServicesAsync Refactor Changelog

**Source file:** `UnifiedLogin.BusinessLogic/Logic/Product/ManageProductVendorServices.cs`  
**Output file:** `UnifiedLogin.BusinessLogic/LogicAsync/Product/ManageProductVendorServicesAsync.cs`  
**Interface:** `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductVendorServicesAsync.cs`  
**Target framework:** .NET 10 / C# 13  
**Date:** 2026-04-14

---

## Breaking Changes

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `DefaultUserClaim` passed by caller at every call site | Removed entirely — context resolved internally via `IProductContextServiceAsync.GetUserContextAsync` |
| 2 | `ManageVendorServicesUser(out List<AdditionalParameters> additionalParameters)` — `out` param | Returns `Task<(string result, List<AdditionalParameters> auditParams)>` — tuple return, no `out` |
| 3 | `System.Runtime.Caching.MemoryCache.Default` static mutable singleton for token cache | `IMemoryCache` injected, no shared static state |
| 4 | `ObjectContent<dynamic>` + `JsonMediaTypeFormatter` (System.Net.Http.Formatting) | `StringContent` with `application/json`; `JsonConvert.SerializeObject` via `Serialize()` helper |
| 5 | `new HttpClient()` instantiated per call (socket exhaustion risk) | `IHttpClientFactory.CreateClient()` + `CreateBearerClient(token)` helper |
| 6 | `.Result` / `.GetAwaiter().GetResult()` blocking throughout | All call sites `await`-ed; no sync-over-async |

---

## Constructor: from 2 to 11 Dependencies

| # | Dependency | Purpose |
|---|-----------|---------|
| 1 | `IProductContextServiceAsync` | Resolves editor/user/company context, replaces `DefaultUserClaim` |
| 2 | `IProductRepositoryAsync` | `GetProductInternalSettingsAsync`, `UpdateProductSettingProductStatusAsync` |
| 3 | `ISamlAttributeServiceAsync` | SAML `UpsertAttributesAsync` (username + userId) after user creation |
| 4 | `IManageBlueBookAsync` | `GetProductCompanyMappingAsync` (source id), `GetCompanyMapAsync` (translated id), `GetCompanyPropertyInstanceAsync` |
| 5 | `IManagePersonaAsync` | `GetPersonaAsync` to resolve `RealPageId` from `personaId` |
| 6 | `IManagePersonAsync` | `GetPersonAsync` for `FirstName`, `LastName` |
| 7 | `IManageUserLoginAsync` | `GetUserLoginOnlyAsync` for `LoginName` |
| 8 | `IManageContactMechanismAsync` | `ListContactMechanismForPersonAsync` for email address |
| 9 | `IHttpClientFactory` | Named/unnamed HTTP client creation |
| 10 | `IMemoryCache` | Bearer token cache (9-min TTL) + product config cache (1-hour TTL) |
| 11 | `ILogger<ManageProductVendorServicesAsync>` | Structured logging throughout |

All 11 guarded with `ArgumentNullException.ThrowIfNull`.

---

## Eliminated Mutable Fields

| Old mutable field | Replacement |
|------------------|-------------|
| `_accessToken` (string field) | `IMemoryCache` under key `"VS_AccessToken"` (9-min TTL) |
| `_productConfig` (field populated lazily) | `IMemoryCache` under key `"VS_ProductConfig"` (1-hour TTL) |
| Instance state used across calls | No mutable instance fields; class is thread-safe |

---

## Configuration Record

```csharp
internal sealed record VsConfig(
    string ApiEndpoint,
    string ApiSecret,
    string ClientId,
    string TokenIssueUri,
    string GetRoleEndpoint);
```

Loaded from `GetProductInternalSettingsAsync(ProductId)` using setting names:
`APIENDPOINT`, `APISECRET`, `CLIENTID`, `TOKENENDPOINT`, `GETROLEENDPOINT`.  
Cache key: `"VS_ProductConfig"`, TTL: 1 hour.

---

## OAuth2 Token Management

| Aspect | Old | New |
|--------|-----|-----|
| Cache store | `System.Runtime.Caching.MemoryCache.Default` (static) | `IMemoryCache` via DI |
| Cache key | `"VendorServicesToken"` | `"VS_AccessToken"` |
| TTL | 9 minutes (hardcoded string) | `TimeSpan.FromMinutes(9)` (`TokenCacheTtl` constant) |
| HTTP client | `new HttpClient()` | `_httpClientFactory.CreateClient()` (unnamed, for token endpoint only) |
| Scope | `config.ClientId` | Same — scope == clientId per VC token contract |

---

## HTTP Client Pattern

- `CreateBearerClient(token)` factory method: `_httpClientFactory.CreateClient()` + sets `Authorization: Bearer <token>` header before returning.
- No named client registered; all VC API calls use `CreateBearerClient` for per-request headers with no shared mutable state.
- `Serialize(payload)` helper: `JsonConvert.SerializeObject` + `StringContent("application/json")` — replaces `ObjectContent<dynamic>` + `JsonMediaTypeFormatter`.

**DI registration:**
```csharp
services.AddScoped<IManageProductVendorServicesAsync, ManageProductVendorServicesAsync>();
// No named HttpClient required — uses factory.CreateClient() (unnamed)
```

---

## Parallel Property-Group Fetches

Replaces 3 serial synchronous calls:

```csharp
// Old: 3 sequential blocking API calls
var divisions     = GetDivisions(token, config, companyId);
var regions       = GetRegions(token, config, companyId);
var ownershipGrps = GetOwnershipGroups(token, config, companyId);

// New: concurrent async
var divisionsTask  = GetDivisionsAsync(token, config, companyId, ct);
var regionsTask    = GetRegionsAsync(token, config, companyId, ct);
var ownershipTask  = GetOwnershipGroupsAsync(token, config, companyId, ct);
await Task.WhenAll(divisionsTask, regionsTask, ownershipTask);

IList<VendorServicesPropertyGroup> allGroups =
[
    .. divisionsTask.Result,
    .. regionsTask.Result,
    .. ownershipTask.Result
];
```

---

## Parallel Identity Lookups

`GetPersonAsync`, `GetUserLoginOnlyAsync`, and `ListContactMechanismForPersonAsync` run concurrently in both `ManageVendorServicesUserAsync` and `UpdateVendorServicesUserProfileAsync`:

```csharp
var personTask  = _managePerson.GetPersonAsync(realPageId, ct);
var loginTask   = _manageUserLogin.GetUserLoginOnlyAsync(realPageId, ct);
var contactTask = _manageContactMechanism.ListContactMechanismForPersonAsync(realPageId, string.Empty, ct);
await Task.WhenAll(personTask, loginTask, contactTask);
```

---

## Two BlueBook Call Patterns

VendorServices requires two distinct BlueBook IDs:

| Use case | Method | Returns | Field |
|---------|--------|---------|-------|
| All methods except `GetProperties` | `GetProductCompanyMappingAsync(orgRealPageId, "CD", ct)` | External string parsed to `int` | `CompanyInstanceSourceId` |
| `GetProperties` only | `GetCompanyMapAsync(orgRealPageId, booksCustomerMasterId, "CD", domain, useTranslate: false, ct)` | Internal translated `int` | `CompanyInstanceId` |

`useTranslate: false` is intentional and preserves the original sync behaviour.  
`UdmSourceCode = BlueBookProductConstants.VendorServices` = `"CD"`.

---

## Post-Create Migration Flag

After a successful `InsertVendorServicesProductUserAsync` (POST `/api/Users`):

1. SAML attributes upserted (`productUsername` + `UserId`)
2. `UpdateProductSettingProductStatusAsync` → `ProductBatchStatusType.Success`
3. `UpdateUsersMigrationStatusAsync` called to set `UsingUnifiedLogin = true` for the new user in the VC product

This was present in the sync version and is preserved in the async implementation.

---

## `ManageVendorServicesUserAsync` Flow Summary

1. `GetUserContextAsync` → `(ctx, error)`
2. Config + token (cached)
3. `GetPersonaAsync` → `RealPageId`
4. `Task.WhenAll(GetPersonAsync, GetUserLoginOnlyAsync, ListContactMechanismForPersonAsync)`
5. `GetCompanyInstanceSourceIdAsync` (BlueBook)
6. `GetUserAccessGroupsByAccessTypeAsync` (all roles, incl. super-user set)
7. Super-user override: `IProductContextServiceAsync.IsSuperUserAsync` → assign all non-client roles (`HashSet<string>` exclusion of `"User"`, `"CliVndOnly"`, `"CliVndRO"`), `PropertyList = ["-1"]`
8. Username dedup loop: `IsUsernameAvailableAsync` inline HTTP; fallback pattern `{first}{last}{personaId}[{n}]`
9. `InsertVendorServicesProductUserAsync` (POST) or `UpdateVendorServicesProductUserAsync` (PUT)
10. On success: `BuildActivityDetailsAsync` → `auditParams`

---

## `IsUsernameAvailableAsync` — Inline HTTP (No Generic Helper)

`IsUsernameAvailableAsync` returns `bool?` (unavailable = false, server error = null). Because `GetFromApiAsync<T>` has a `where T : class` constraint, `bool?` (`Nullable<bool>`, a value type) cannot use it. The method uses an inline HTTP call instead:

```csharp
private async Task<bool?> IsUsernameAvailableAsync(
    string token, VsConfig config, string userName, CancellationToken ct)
{
    string url = $"{config.ApiEndpoint}/api/Users/IsUsernameAvailable/{userName}/";
    using var client   = CreateBearerClient(token);
    using var response = await client.GetAsync(url, ct);
    if (!response.IsSuccessStatusCode) return null;
    var body = await response.Content.ReadAsStringAsync(ct);
    return bool.TryParse(body.Trim().Trim('"'), out bool result) ? result : null;
}
```

---

## `DisableProductUserAsync` — PATCH Payload

Replaces `ObjectContent<dynamic>` + `JsonMediaTypeFormatter`:

```csharp
var payload = new { username, locked = isLocked };
var content = Serialize(payload);   // StringContent("application/json")
var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
var response = await client.SendAsync(request, ct);
```

---

## `BuildActivityDetailsAsync` — Async Audit Diff

Diffs the before/after state of the user and produces `List<AdditionalParameters>`:

| Diff item | Method |
|-----------|--------|
| Access type change | `GetAccessType` switch expression |
| Role changes | `HashSet<string>.Except` (old roles → new roles) |
| Property changes | Loop over `UserLocations` before/after |
| Property group change | `CompanyDivisionId` comparison |
| Notification flags (×3) | `AddNotificationAudit` static helper |

`HashSet<string>` used for O(1) membership checks in all merge helpers and audit diff.

---

## `GeneratePassword` — Stack-Allocated Random Bytes

```csharp
private static string GeneratePassword(int length, int specialCharCount)
{
    Span<byte> rndBytes = stackalloc byte[length];
    RandomNumberGenerator.Fill(rndBytes);
    // char selection from rndBytes — no heap allocation for the byte buffer
}
```

`Span<byte>` on stack avoids a heap allocation for the random byte buffer on every new-user create.

---

## `GetProductConfigAsync` — `ValueTask<VsConfig>`

```csharp
private async ValueTask<VsConfig> GetProductConfigAsync(CancellationToken ct)
{
    if (_cache.TryGetValue(ConfigCacheKey, out VsConfig? hit)) return hit!;
    // ... load from DB, cache, return
}
```

Returns `ValueTask<VsConfig>` so the common cache-hit path allocates zero `Task` objects.

---

## C# 13 Features Used

| Feature | Where |
|---------|-------|
| Collection expressions `[]` | Empty list literals: `List<AdditionalParameters> auditParams = []`, `UserAccessGroups = []`, `PropertyList = ["-1"]` |
| Spread operator `.. expr` | `[.. divisionsTask.Result, .. regionsTask.Result, .. ownershipTask.Result]` |
| `sealed record` | `VsConfig` — immutable, JIT-devirtualizable |
| `sealed class` | `ManageProductVendorServicesAsync` — JIT devirtualization |
| `switch` expression | `GetAccessType` dispatch |
| `when` exception filter | `catch (Exception ex) when (ex is not OperationCanceledException)` throughout |
| `ArgumentNullException.ThrowIfNull` | All 11 constructor dependencies |

---

## .NET 10 / Performance Improvements

| Area | Improvement |
|------|-------------|
| Thread-pool pressure | No `.Result`/`.GetAwaiter().GetResult()` — all `await`-ed |
| Socket exhaustion | `IHttpClientFactory` replaces `new HttpClient()` per call |
| Static caching | Eliminated `System.Runtime.Caching.MemoryCache.Default` (app-domain singleton) |
| Parallel I/O | 3 property-group fetches + 3 identity lookups run concurrently |
| Stack allocation | `GeneratePassword` uses `stackalloc byte[]` via `Span<byte>` |
| Zero-alloc cache hit | `ValueTask<VsConfig>` on config cache hits |
| O(1) lookups | `HashSet<string>` for role/property membership tests in merge and audit |
| Immutable config | `sealed record VsConfig` — safe to cache and share across concurrent requests |
