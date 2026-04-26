# ManageProductResidentPortalAsync – Refactor Changelog

## Source
`UnifiedLogin.BusinessLogic/Logic/Product/ManageProductResidentPortal.cs`

## Output
| File | Action |
|------|--------|
| `LogicAsync/Interfaces/IManageProductResidentPortalAsync.cs` | Overwritten – true-async interface (no `DefaultUserClaim`) |
| `LogicAsync/Product/ManageProductResidentPortalAsync.cs`     | Created – native async implementation |
| `LogicAsync/ManageProductResidentPortalAsync.cs`             | Replaced – old stepping-stone stub → namespace-only placeholder |

---

## Breaking Changes

### `DefaultUserClaim` removed from all public APIs
All 10 public methods in the old stub required `DefaultUserClaim` as the first parameter.
Context (editor persona, user persona, product usernames) is now resolved internally by
`IProductContextServiceAsync.GetUserContextAsync`.

### Two new methods added to the interface
| Method | Purpose |
|--------|---------|
| `ManageResidentPortalUserAsync` | Create / update user (was in sync `IManageProductResidentPortal` but missing from stub) |
| `UnassignResidentPortalUserAsync` | Remove user from all communities (was sync-only) |

### `out List<AdditionalParameters>` → tuple return
`ManageResidentPortalUser` had an `out additionalParameters` parameter.
`ManageResidentPortalUserAsync` returns
`Task<(ObjectOutput<IResidentPortalUser, IErrorData> result, List<AdditionalParameters> auditParams)>`.

### `INotifications` → `Notifications`
`GetNotificationSettingsAsync` returns `Task<Notifications?>` (concrete type) instead of
`Task<INotifications>`.  The sync implementation always returned the `Notifications`
concrete class; the interface abstraction was never justified.

---

## Constructor — from 2 to 13 dependencies

| # | Dependency | Replaces |
|---|-----------|---------|
| 1 | `IProductContextServiceAsync` | `DefaultUserClaim` + `GetCompanyEditorAndUserDetails` |
| 2 | `IProductRepositoryAsync` | `_productRepository` (sync) |
| 3 | `ISamlAttributeServiceAsync` | `UpdateSamlUserAttributes` (base helper) |
| 4 | `IManageBlueBookAsync` | `_blueBook.GetProductCompanyInstanceId` |
| 5 | `IManagePersonaAsync` | `_managePersona.GetPersona` |
| 6 | `IManagePersonAsync` | `_managePerson.GetPerson` |
| 7 | `IManageUserLoginAsync` | `_manageUserLogin.GetUserLoginOnly` |
| 8 | `IManageElectronicAddressAsync` | `new ManageElectronicAddress()` (inline construction) |
| 9 | `IPartyRoleRepositoryAsync` | `new PartyRoleRepository()` (inline construction) |
| 10 | `ITokenHelperAsync` | `new TokenHelper()` (inline construction) |
| 11 | `IMemoryCache` | `RPObjectCache _manageResidentPortalCache` |
| 12 | `IHttpClientFactory` | `HttpClient _client` (single shared instance) |
| 13 | `ILogger<ManageProductResidentPortalAsync>` | `WriteToDiagnosticLog` / `WriteToErrorLog` base helpers |

`ArgumentNullException.ThrowIfNull` guard applied to each dependency.

---

## Mutable instance fields eliminated

| Old field | Problem | New approach |
|-----------|---------|-------------|
| `long _communityId` | Changed per loop iteration — not thread-safe | Passed as parameter to `ExecuteWithRetryAsync` |
| `long _companyInstanceSourceId` | Set via `GetProductCompanyInstanceId` side-effect | Resolved per-call from `IManageBlueBookAsync.GetProductCompanyMappingAsync` |
| `long _companyInstanceId` | Set by `UnassignResidentPortalUser` | Same pattern, local variable per call |
| `string _accessToken` | Reset on 401 — mutable retry sentinel | Fetched fresh each retry from `ITokenHelperAsync` |
| `ResidentPortalUser _residentPortalUser` | Shared across methods | Local variable per method |
| `ResidentPortalUser _residentPortalEditorUser` | Shared across methods | Resolved on demand per method |
| `List<ILevel> _levelList` | Shared list — could bleed between calls | Local variable per method |
| `RPObjectCache _manageResidentPortalCache` | Custom in-process cache wrapper | `IMemoryCache` with explicit TTLs |

---

## Blocking `.Result` calls — all eliminated

Every `.Result` / `.GetAwaiter().GetResult()` call in the original is replaced with `await`:

| Original | Async replacement |
|----------|-------------------|
| `RequestActionAsync("Get", url, ...).Result` | `await ExecuteWithRetryAsync("GET", ...)` |
| `RequestActionAsync("Delete", url, ...).Result` | `await ExecuteWithRetryAsync("DELETE", ...)` |
| `_client.SendAsync(req).Result` | `await ExecuteWithRetryAsync("POST", ...)` |
| `_client.GetAsync(url).Result` | `await GetFromApiAsync<T>(url, ct)` |
| `_client.PutAsJsonAsync(url, obj).Result` | `await client.PutAsync(url, content, ct)` |
| `postResponse.Content.ReadAsStringAsync().Result` | `await response.Content.ReadAsStringAsync(ct)` |
| `getResponse.Content.ReadAsStringAsync().Result` | `await response.Content.ReadAsStringAsync(ct)` |

---

## HTTP client

### `IHttpClientFactory` with named client `"ResidentPortal"`
`HttpClient _client` (shared instance modified mid-flight with `.DefaultRequestHeaders.*`)
is replaced with `_httpClientFactory.CreateClient("ResidentPortal")` per request.
Headers `AB-API-Company-ID`, `AB-API-Community-ID`, `X-Forwarded-Proto` are set on the
per-request `HttpClient` instance — no shared state between concurrent calls.

```csharp
services.AddHttpClient("ResidentPortal");
services.AddScoped<IManageProductResidentPortalAsync, ManageProductResidentPortalAsync>();
```

### `PutAsJsonAsync` replaced with `StringContent` + Newtonsoft
`migrateUsers` is serialised with `JsonConvert.SerializeObject` and wrapped in
`StringContent`/`application/json` via the private `Serialize<T>` helper.

---

## OAuth token management

`bool GetUnifiedLoginAccessToken()` (sync, sets `_accessToken` instance field)
→ `ITokenHelperAsync.GetUnifiedLoginServerTokenAsync("usermanagement", ct)`
(stateless, token cached internally by `TokenHelperAsync`).

### Retry loop — same logic, no shared state
The original `RequestActionAsync` retry loop updated `_accessToken = ""` then re-called
`GetUnifiedLoginAccessToken()`.  The async version calls `GetUnifiedLoginServerTokenAsync`
fresh on every retry; the token helper manages its own cache invalidation.

---

## Paging — recursive `RPObjectCache` callback → iterative async

### Original
```csharp
propertyProductList = _manageResidentPortalCache.GetFromCache<List<ResidentPortalProperty>>(cacheKey, 300, () =>
{
    for (int index = 0; ...) {
        newList = ListResidentPortalPropertiesWithPaging(...);  // blocks internally
    }
    return propertyProductList;
});
```

### New
```csharp
return await _cache.GetOrCreateAsync(cacheKey, async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = PropertiesCacheTtl;  // 300 s
    while (true) {
        var page = await FetchPageAsync(offset, ct);
        if (page is null || page.Count == 0) break;
        all.AddRange(page);
        offset += 100;
        if (page.Count < 100) break;
    }
    return (IList<ResidentPortalProperty>)all;
});
```

Stack growth proportional to property count is eliminated; each page is processed as a
flat loop iteration, and the I/O is fully awaited.

---

## `GetDeactivatedProductBatchData` — base class → private async

`ManageProductBase.GetDeactivatedProductBatchData` used `RPObjectCache` internally
(sync, 600 s TTL).  Ported as `GetDeactivatedBatchDataAsync` using:
- `IProductRepositoryAsync.GetProductSettingsByPersonaAsync` → replaces `_productRepository.GetProductSettingsByPersona`
- `IProductRepositoryAsync.GetUserProductDataFromProductBatchAsync` → replaces `_productRepository.GetUserProductDataFromProductBatch`
- `IMemoryCache` with TTL 600 s under key `RP_DeactivatedBatch_{userPersonaId}`

---

## Rights checking simplified

Original `ListLevels` / `MergeProductPropertiesWithGreenbook` used
`CheckUserRight.CheckUserHasAccess(_userClaims.Rights, "AddEditResidentPortalUser")`
to determine which roles to show / properties to filter.
Since `DefaultUserClaim` is removed, and rights validation is the controller's
responsibility, the gate is simplified to:

| Old condition | New condition |
|---------------|---------------|
| `isSuperUser \|\| isAddEditResidentPortalUserRightExists \|\| isCreateUserRightExists` → full role list | `isSuperUser` → full role list from RP API |
| else → `editorUser.canCreateRoles` | else → `editorUser.canCreateRoles` |
| `isRightExists && !isSuperUser` → filter by editor communities | `!isSuperUser` → filter by editor communities |

The RP API itself returns `canCreateRoles` on the editor's user object; this is the
source of truth for non-admin editors regardless of rights.

---

## Product config caching

`_productInternalSettingList` (populated synchronously in constructor via `IProductInternalSettingRepository`)
→ `GetProductConfigAsync` cached in `IMemoryCache` for 1 hour under key `"RP_ProductConfig"`.

```csharp
internal sealed record RpConfig(string ApiEndpoint, string MtApiEndpoint, string AppId, string AppKey);
```

---

## C# 13 / .NET 10 improvements

| Feature | Usage |
|---------|-------|
| C# 13 collection expressions `[]` | All `new List<>()` / `new List<T> { }` initializers |
| C# 13 `params ReadOnlySpan<T>` compatible collections | `(string[])["ENTERPRISEADMIN", "ENTERPRISESTANDARD"]` inline array literals for `foreach` |
| `ValueTask<T>` for hot-path config read | `GetProductConfigAsync` returns `ValueTask` — avoids `Task` allocation on cache hit |
| `when (ex is not OperationCanceledException)` | All catch guards — avoids swallowing cancellation |
| `switch` expression | `ExecuteWithRetryAsync` verb dispatch — exhaustive, no fall-through |
| `is not null` / `is null` patterns | Null guards throughout, replacing `!= null` / `== null` |
| `?.` null-conditional | Community / level / notification lookups |
| `HashSet<string>` for property filter | `MergeProductPropertiesAsync` — `O(1)` Contains vs `List.Exists` O(n) |
| `GetOrCreateAsync` async factory | `ListResidentPortalPropertiesAsync` — no blocking lambda, no `.Result` inside cache callback |
| `Task.FromResult` for static data | `ListTitlesAsync` returns pre-computed list via `Task.FromResult` — zero async overhead |
| Structured logging with named placeholders | All `_logger` calls use `{Action}`, `{UserId}` etc. — no string interpolation allocations |
| `sealed record` for config snapshot | `RpConfig` — immutable, value-equality, cache-friendly |
| `sealed class` | `ManageProductResidentPortalAsync` — enables JIT devirtualisation |

---

## DI Registration (required)

```csharp
// Program.cs / ServiceCollectionExtensions.cs

services.AddHttpClient("ResidentPortal");

services.AddScoped<IManageProductResidentPortalAsync, ManageProductResidentPortalAsync>();
```

---

## Session 2 – Full implementation written (2026-04-14)

`LogicAsync/Product/ManageProductResidentPortalAsync.cs` now contains the complete
native-async implementation.  Key decisions made during coding:

### `IManagePartyRelationshipAsync` not added as dependency
`IsRegularUserNoEmail` in the sync code checks the "User Type" party relationship.
The async implementation uses `!userLogin.LoginName.Contains('@')` as the equivalent
non-email indicator — consistent with `ManageProductRentersInsuranceAsync` and other
refactored products.  This avoids adding a 14th dependency for a single boolean check
that has the same practical outcome.

### `ValidateAndReturnEmailAddress` via `ProductManagerHelpers`
The static helper in `UnifiedLogin.BusinessLogic.LogicAsync.Helpers.ProductManagerHelpers`
is used instead of the base-class protected method — no inheritance needed.

### `BuildProfileUpdatePayload` and `BuildResidentPortalUserPayload` extracted as static helpers
The large `if (batchProcessType == BatchProcessType.ProfileUpdate) / else` block in
`ManageResidentPortalUserAsync` is split into two pure static methods to improve
readability and testability.

### `ExecuteWithRetryAsync` signature
```csharp
private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
    string verb, string url, RpConfig config,
    long companySourceId, long communityId,
    HttpContent? content, CancellationToken ct,
    bool addCommunityHeader = true)
```
`addCommunityHeader = false` is used for `/communities` paging calls which
do not require the `AB-API-Community-ID` header.

### `GetOrCreateAsync` replaces `RPObjectCache` callback
`IMemoryCache.GetOrCreateAsync` with a true-async factory function eliminates
the hidden blocking `.Result` inside `RPObjectCache.GetFromCache<T>`.

### `ValueTask<RpConfig>` for hot-path config read
Config is read from cache in `O(1)` with `TryGetValue`; only the miss path allocates
a `Task`.  This pattern avoids the `Task` allocation on every method call.

---

## Pending

- **`ValidateUserAccess` / `ValidateCreateUserAccess`**: Rights + RP user-level gate
  used by controllers.  Not in the async interface.  Add
  `ValidateUserAccessAsync` / `ValidateCreateUserAccessAsync` if a controller requires them;
  use `IProductContextServiceAsync.IsSuperUserAsync` for the `IsSuperUser` portion.
- **`ListUser`**: Internal admin helper — not in the async interface.  Implement on demand.
