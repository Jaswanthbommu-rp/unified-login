# ManageProductRumAsync – Refactor Changelog

## Source
`UnifiedLogin.BusinessLogic/Logic/Product/ManageProductRum.cs`

## Output
| File | Action |
|------|--------|
| `LogicAsync/Interfaces/IManageProductRumAsync.cs`     | Overwritten – true-async interface (no `DefaultUserClaim`) |
| `LogicAsync/Product/ManageProductRumAsync.cs`         | Created – native async implementation |
| `LogicAsync/ManageProductRumAsync.cs`                 | Replaced – old stepping-stone stub → namespace-only placeholder |

---

## Breaking Changes

### `DefaultUserClaim` removed from all public APIs
The old stub required `DefaultUserClaim` as the first parameter on every method.
Context (editor persona, user persona, product user ID/username) is now resolved internally by
`IProductContextServiceAsync.GetUserContextAsync`.

### `ManageRumUser` `out` parameter → tuple return
`ManageRumUser(…, out List<AdditionalParameters> additionalParameters)` is incompatible with `async`.
`ManageRumUserAsync` now returns `Task<(string result, List<AdditionalParameters> auditParams)>`.

### Four methods added to interface (were missing from old stub)
| Method | Notes |
|--------|-------|
| `GetUMGlobalRolesAsync` | Was not in the stepping-stone interface |
| `UnassignRumUserAsync`  | Was not in the stepping-stone interface |
| `UpdateUserProfileAsync`| Was not in the stepping-stone interface |
| `ManageRumUserAsync`    | Was not in the stepping-stone interface |

---

## Constructor — from 2 to 11 dependencies

| # | Dependency | Replaces |
|---|-----------|---------|
| 1 | `IProductContextServiceAsync` | `DefaultUserClaim` + `GetCompanyEditorAndUserDetails` |
| 2 | `IProductRepositoryAsync` | Sync `_productRepository` + `UpdateProductSettingProductStatus` base helper |
| 3 | `ISamlAttributeServiceAsync` | `_samlRepository.CreateSamlUserAttribute` (sync, direct DB) |
| 4 | `IManageBlueBookAsync` | `GetProductCompanyInstanceId` + `_blueBook.GetCompanyMap` (sync) |
| 5 | `IManagePersonaAsync` | `_managePersona.GetPersona(personaId)` — needed to resolve `RealPageId` |
| 6 | `IManagePersonAsync` | `_managePerson.GetPerson(realPageId)` |
| 7 | `IManageUserLoginAsync` | `_manageUserLogin.GetUserLoginOnly(realPageId)` |
| 8 | `IManageContactMechanismAsync` | `new ManageElectronicAddress().ListElectronicAddressForPerson(…)` (inline construction) |
| 9 | `IMemoryCache` | `MemoryCache.Default` (static `ObjectCache`) for OAuth token + config |
| 10 | `IHttpClientFactory` | `new HttpClient()` / `new HttpClient(_messageHandler, false)` inline constructions |
| 11 | `ILogger<ManageProductRumAsync>` | `WriteToDiagnosticLog` / `WriteToErrorLog` base helpers |

`ArgumentNullException.ThrowIfNull` guard applied to each dependency.

---

## Mutable instance fields eliminated

| Old field | Problem | New approach |
|-----------|---------|-------------|
| `string _productUserId` | Set per-call via `GetCompanyEditorAndUserDetails` | `ctx.ProductUserId` per call from `IProductContextServiceAsync` |
| `string _productUsername` | Set per-call | `ctx.ProductUsername` per call |
| `string _accessToken` | Set in constructor, shared across requests | Per-call from `GetRumTokenAsync` (cached in `IMemoryCache`) |
| `string _apiEndPoint` / `_apiSecret` / `_clientId` / `_nwpIssueUri` | Constructor-time sync DB call | Cached in `IMemoryCache` for 1 hr as `RumConfig` record |
| `Persona _editorPersona` | Set per-call via `GetCompanyEditorAndUserDetails` | `ctx.EditorPersona` per call |

---

## Blocking `.Result` calls — all eliminated

| Original | Async replacement |
|----------|-------------------|
| `client.GetAsync(url).Result` | `await client.GetAsync(url, ct)` |
| `client.PostAsJsonAsync(url, obj).Result` | `await client.PostAsync(url, Serialize(obj), ct)` |
| `client.PutAsJsonAsync(url, obj).Result` | `await client.PutAsync(url, Serialize(obj), ct)` |
| `client.DeleteAsync(url).Result` | `await client.DeleteAsync(url, ct)` |
| `response.Content.ReadAsStringAsync().Result` | `await response.Content.ReadAsStringAsync(ct)` |
| `httpClient.PostAsync(tokenUrl, form).Result` | `await httpClient.PostAsync(tokenUrl, form, ct)` |

---

## OAuth2 token — `MemoryCache.Default` → `IMemoryCache`

### Original
```csharp
ObjectCache tokenCache = MemoryCache.Default;
_accessToken = tokenCache["access_token_RUM"] as string;
// ...
tokenCache.Set("access_token_RUM", _accessToken, new CacheItemPolicy { AbsoluteExpiration = … });
```

### New
```csharp
if (_cache.TryGetValue(TokenCacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
    return cachedToken!;
// POST to token endpoint...
_cache.Set(TokenCacheKey, token, TokenCacheTtl);
```

Cache key: `"RUM_AccessToken"`, TTL: 9 minutes (matching the original 10-minute token expiry with 1-minute margin).

---

## Config caching

Product settings (`APIENDPOINT`, `CLIENTID`, `APISECRET`, `TOKENURL`) are loaded once from
`IProductRepositoryAsync.GetProductInternalSettingsAsync` and cached for 1 hour as:

```csharp
internal sealed record RumConfig(
    string ApiEndpoint,
    string ApiSecret,
    string ClientId,
    string TokenUrl,
    string SuperUserRoles,
    Guid   ContractCompanyRealPageId);
```

Cache key: `"RUM_ProductConfig"`, TTL: 1 hour.
`ContractCompanyRealPageId` identifies NWP's subcontractor tenant (loaded from `ContractCompanyRealPageId` setting, defaults to `Guid.Empty` if absent).
`SuperUserRoles` is a comma-separated list appended to the user's role list when `IsSuperUserAsync` returns `true`.

---

## HTTP client

### Named client `"RUM"`
```csharp
services.AddHttpClient("RUM");   // product API calls
```

Token acquisition uses `_httpClientFactory.CreateClient()` (no named client — the token
endpoint is a separate identity server, not the RUM API).
Per-request `Authorization: Bearer {token}` header is set on each short-lived `HttpClient`
instance via `CreateBearerClient(token)` — no shared mutable auth state.

### `PostAsJsonAsync` / `PutAsJsonAsync` replaced with `StringContent` + Newtonsoft
```csharp
private static StringContent Serialize<T>(T value)
    => new(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
```

---

## Two BlueBook call patterns

| Method | BlueBook call | Purpose |
|--------|--------------|---------|
| `GetPropertyGroupsAsync`, `GetRegionsAsync`, `GetRolesAsync`, `GetUMGlobalRolesAsync`, `UnassignRumUserAsync`, `ManageRumUserAsync`, `GetMigrationUsersAsync`, `UpdateUsersMigrationStatusAsync` | `GetProductCompanyMappingAsync(orgRealPageId, "NWP", ct)` | Resolve `companyInstanceSourceId` |
| `GetPropertiesAsync` | `GetCompanyMapAsync(orgRealPageId, booksCustomerMasterId, "NWP", domainName, ct)` | Resolve `rumCompanyId` (PMCID) for property list |

---

## `GetUserAccountableDataAsync` — parallelised

### Original (serial, 4 blocking calls)
```csharp
var oldRolesResponse      = GetRoles(editorPersonaId, userPersonaId, …);
var oldPropertiesResponse = GetProperties(editorPersonaId, userPersonaId, …);
var oldPropertyGroupsResponse = GetPropertyGroups(editorPersonaId, userPersonaId, …);
var oldAccessTypeResponse = GetUMGlobalRoles(editorPersonaId, userPersonaId, …);
```

### New (parallel with Task.WhenAll)
```csharp
var rolesTask         = GetRolesAsync(editorPersonaId, userPersonaId, rp, ct);
var propertiesTask    = GetPropertiesAsync(editorPersonaId, userPersonaId, rp, ct);
var propertyGroupTask = GetPropertyGroupsAsync(editorPersonaId, userPersonaId, rp, ct);
var accessTypeTask    = GetUMGlobalRolesAsync(editorPersonaId, userPersonaId, rp, ct);

await Task.WhenAll(rolesTask, propertiesTask, propertyGroupTask, accessTypeTask);
```

---

## Username deduplication — sync loop → async loop

### Original (blocking)
```csharp
while (!foundNewUserName)
{
    bool result = CheckUserExistsInRum(productLoginName);  // blocking
    if (result) { incrementor++; productLoginName = productLoginName + incrementor.ToString(); }
    else foundNewUserName = true;
}
```

### New (async)
```csharp
int incrementor = 0;
while (await CheckUserExistsInRumAsync(productLoginName, token, config, ct))
    productLoginName = productLoginName + (++incrementor).ToString();
```

---

## `ManageElectronicAddress` inline construction → `IManageContactMechanismAsync`

### Original
```csharp
var manageElectronicAddress = new ManageElectronicAddress();
var addresses = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);
```

### New
```csharp
var addresses = await _contactMechanism.ListContactMechanismForPersonAsync(realPageId, string.Empty, ct);
```

---

## Merge helpers — LINQ improvements

All `foreach` + manual assignment loops replaced with LINQ-based `HashSet` lookups for O(1) membership tests:

```csharp
var assignedIds = rumUser.Claims
    .Where(c => c.Type == claimType)
    .Select(c => c.Value)
    .ToHashSet();

foreach (var rpg in allPropertyGroups.Where(g => assignedIds.Contains(g.Id)))
    rpg.IsAssigned = true;
```

---

## C# / .NET improvements

| Feature | Usage |
|---------|-------|
| C# 12 collection expressions `[]` | `new List<>()` → `[]` throughout |
| `when (ex is not OperationCanceledException)` | All catch guards |
| `is not null` / `?.` null-conditional | Null guards and claim lookups |
| `string.Equals(…, OrdinalIgnoreCase)` | All case-insensitive string comparisons |
| `switch` expression | `claimType` resolution in `MergeRumPropertiesWithGreenbookAsync` |
| `GetValueOrDefault` | `Dictionary` access in `GetActivityLogs` |
| `StringSplitOptions.TrimEntries \| RemoveEmptyEntries` | Super-user role list parsing |
| `internal sealed record RumConfig` | Immutable config value object |
| `HashSet<string>` lookups | O(1) membership checks in merge helpers |
| `Task.WhenAll` | `GetPersonAsync` + `GetUserLoginOnlyAsync` parallelised in `UpdateUserProfileAsync` / `ManageRumUserAsync` |

---

## DI Registration (required)

```csharp
// Program.cs / ServiceCollectionExtensions.cs

services.AddHttpClient("RUM");

services.AddScoped<IManageProductRumAsync, ManageProductRumAsync>();
```

---

## Pending / Notes

- **`WriteUpdateUserTypeActivityLog`**: The base class method (called with `BatchProcessType.ProfileUpdate` in
  `UpdateUserProfile`) has no handler for `ProfileUpdate` in its if/else chain — it was a no-op.
  The async implementation omits this call entirely.
- **`ReActivateRumUser`**: Preserved as `ReActivateRumUserAsync` — called by `UpdateInactiveUserAsync` before
  every PUT update. Verify the `/user/reactivateuser` endpoint still accepts a Bearer-authenticated empty-body POST.
- **Dynamic deserialization** in `GetRumPropertiesDataAsync` and `GetRumRolesAsync`: The RUM API returns
  anonymous JSON shapes — `IList<dynamic>` deserialization is preserved from the source.
  Consider defining strongly-typed DTOs if the schema is stable.
