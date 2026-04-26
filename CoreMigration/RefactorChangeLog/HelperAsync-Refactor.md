# HelperAsync Refactor Changelog

**Date:** 2026-04-18  
**Branch:** `feature/UserRepositoryRefactor`  
**Target:** .NET 10 / C# 13  

---

## Assessment: What Was Already Refactored

| File | Status before this session |
|------|---------------------------|
| `Logic/Helper/LogActivity.cs` | **Not refactored.** Partially async (two public async methods existed), but still static, still uses `new ProductInternalSettingRepository()`, `lock()` for token refresh, and `.Result` anti-patterns. |
| `Logic/Helper/BatchHelper.cs` | **Not refactored.** Fully synchronous static class; two methods create inline `new Xxx(userClaim)` service instances. |

---

## Files Created — `LogicAsync/Helper/`

### Activity Log

| File | Replaces | Key change |
|------|----------|------------|
| `IActivityLogServiceAsync.cs` | `LogActivity` (static) | DI-injectable interface; all methods `Task`/`Task<bool>` with `CancellationToken` |
| `ActivityLogServiceAsync.cs` | `LogActivity` (static) | Singleton; `IHttpClientFactory`; `IServiceScopeFactory` for scoped settings repo; `SemaphoreSlim` replaces `lock()`; no `.Result` |

### Batch Helpers — Pure Computation

| File | Replaces | Key change |
|------|----------|------------|
| `BatchHelperAsync.cs` | `BatchHelper` (static) | Same static pattern; all I/O-free methods retained; C# 13 modernizations applied throughout |

### Batch Helpers — I/O Dependent (now injectable)

| File | Replaces | Key change |
|------|----------|------------|
| `IDocManagementBatchServiceAsync.cs` | `BatchHelper.CreateDocManagementBatchRecords` | Injectable interface; `DefaultUserClaim` parameter removed |
| `DocManagementBatchServiceAsync.cs` | `BatchHelper.CreateDocManagementBatchRecords` | Uses `IManageProductRPDocumentManagementAsync`; fully async |
| `IAoBatchServiceAsync.cs` | `BatchHelper.CreateAoBatchRecords` | Injectable interface; `DefaultUserClaim` parameter removed |
| `AoBatchServiceAsync.cs` | `BatchHelper.CreateAoBatchRecords` | Uses `ISamlRepositoryAsync` (async); `CopyRegularUser` annotated `// SYNC` |

---

## `ActivityLogServiceAsync` — Design Details

### Problems fixed vs. `LogActivity`

| Problem | Fix |
|---------|-----|
| `static class` — cannot accept DI | Converted to `sealed class` implementing `IActivityLogServiceAsync`, registered as Singleton |
| `new ProductInternalSettingRepository()` (sync, no DI) | `IServiceScopeFactory` creates a transient scope to resolve `IProductInternalSettingRepositoryAsync` — avoids the Singleton→Scoped captive-dependency problem |
| Single static `HttpClient` field | `IHttpClientFactory.CreateClient("ActivityLog")` per request — prevents socket exhaustion and handles DNS changes |
| `lock(_settingsLock)` — cannot be held across `await` | `SemaphoreSlim(1, 1)` with double-check pattern inside `await _settingsSemaphore.WaitAsync()` |
| `lock(_tokenLock)` — same issue | `SemaphoreSlim(1, 1)` with double-check inside `await _tokenSemaphore.WaitAsync()` |
| `_httpClient.Send(request)` (sync HTTP in `TryAcquireToken`) | `await client.SendAsync(request, ct)` |
| `response.Content.ReadAsStringAsync().Result` in `TryAcquireToken` | `await response.Content.ReadAsStringAsync(ct)` |
| `PostAsync("api/write", content).Result` in `LogActivityToApi` | `await client.SendAsync(request, ct)` |
| Duplicate 400-char chunking per overload | Single `PostChunkedAsync` helper takes a `Func<string, ActivityDetails>` builder; called by both overloads |
| `AddActivityRecord` (sync) — makes HTTP calls synchronously | Removed; callers upgrade to `AddActivityRecordAsync` |
| `AddActivityRecordWithoutClaims` (sync) — same | Removed; callers upgrade to `AddActivityRecordWithoutClaimsAsync` |

### Settings cache flow

```
GetCachedSettingsAsync()
  → fast path: field check (no semaphore)
  → slow path: await _settingsSemaphore
    → double-check
    → await using scope = _scopeFactory.CreateAsyncScope()
    → await repo.GetProductInternalSettingsAsync(productId:3, ct)
    → store _cachedSettings + _settingsCacheExpiry (1 day)
    → on failure with stale cache: extend by 1 hour (same resilience as original)
```

### DI Registration

```csharp
// Named HttpClient — base address set dynamically from settings, so no BaseAddress here
services.AddHttpClient("ActivityLog", client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Singleton — owns settings cache + token lifecycle
services.AddSingleton<IActivityLogServiceAsync, ActivityLogServiceAsync>();
```

---

## `BatchHelperAsync` — C# 13 Improvements

### De-duplication: flag-check helpers

The three identical helpers `CheckForAllProperties`, `CheckForAllRegions`, and
`CheckForIsAssignedNewPropertyFlag` all cast `additionalInfo` to `Dictionary<string, bool>`
and look up a key. Collapsed to a single helper:

```csharp
private static bool GetBoolFlag(object additionalInfo, string key)
    => additionalInfo is Dictionary<string, bool> d
       && d.TryGetValue(key, out bool val)
       && val;
```

### De-duplication: per-type property filtering

`GetUserAssignedPropertiesData` had ~180 lines of repeated
```csharp
IEnumerable<X> propResponse = propertiesCollection.Cast<X>().Where(m => m.IsAssigned == true);
translatedPrimaryPropertiesUserResult = new ListResponse() { Records = propResponse.Cast<object>().ToList(), ... };
```
for every supported property type. Replaced with:

```csharp
return records[0] switch
{
    ProductProperty  _ => FilterAssigned<ProductProperty>(records, p => p.IsAssigned == true),
    ACProperty       _ => FilterAssigned<ACProperty>(records, p => p.IsAssigned),
    // ...
};

private static ListResponse FilterAssigned<T>(IList<object> source, Func<T, bool> predicate)
    where T : class
{
    var filtered = source.Cast<T>().Where(predicate).ToList();
    return new ListResponse { Records = [..filtered.Cast<object>()], TotalRows = filtered.Count, ... };
}
```

### Property-type switch expressions

Replace `.GetType() == typeof(X)` + `((X)item).Property` chains with property-pattern `switch`:

```csharp
// Before
if (productPropertyType == typeof(ProductProperty)) { PropertyList.Add(((ProductProperty)item).ID); }
else if (productPropertyType == typeof(ACProperty)) { if (((ACProperty)item).IsAssigned) PropertyList.Add(((ACProperty)item).Id); }
// ...

// After
string? id = item switch
{
    ProductProperty p                       => integrationType == ProductIntegrationTypeEnum.UPFM ? p.Alias : p.ID,
    ACProperty      { IsAssigned: true } p  => p.Id,
    AssetGroup      { IsAssigned: true } p  => p.AssetID,
    OnSiteProperty  { IsAssigned: true } p  => p.GetPropertyId.ToString(),
    RumPropertyGroup{ IsAssigned: true } p  => p.Id.ToString(),
    ProductProperties{ IsAssigned: true } p => p.GetPropertyId,
    Portfolio       { IsAssigned: true } p  => p.ID,
    _                                       => null
};
if (id is not null) propertyList.Add(id);
```

### Role-type switch

```csharp
// Before — two if-blocks with .GetType() checks
// After
switch (item)
{
    case ProductIntegration.Model.ProductRole { IsAssigned: true } pr:
        roleList.Add(pr.GetRoleId); break;
    case ProductRole { IsAssigned: true } pr:
        roleList.Add(pr.ID); roleType = pr.Roletype; break;
}
```

### Collection expressions `[]`

All `new List<string>()` → `[]`; inline single-item lists like `new List<string> { id.ToString() }` → `[id.ToString()]`.

### LINQ `.AddRange` replaces `foreach` + `.Add`

Uniform patterns like iterating a collection to filter-and-add are replaced with `.AddRange(source.OfType<T>().Where(...).Select(...))`.

### Expression-bodied factory methods

Simple factories that just construct and return a `ProductBatch` use `=> new() { ... }` syntax:

```csharp
public static ProductBatch CreateProductBatchRecordForClickPay(
    List<OrganizationRole> userOrganizationRole, bool usePrimaryProperties)
    => new()
    {
        ProductId = (int)ProductEnum.ClickPay, StatusTypeId = 5, RetryCount = 0,
        InputJson = new RolePropertyList { OrganizationRoleList = userOrganizationRole, ... }
    };
```

---

## `DocManagementBatchServiceAsync` — Design Details

Replaces `BatchHelper.CreateDocManagementBatchRecords(DefaultUserClaim userClaim, long createUserPersonaId, long personaId, bool usePrimaryProperties)`.

```
CreateDocManagementBatchRecordAsync(editorPersonaId, subjectPersonaId, usePrimaryProperties, ct)
  → await _rpDocManagement.GetPropertyRolesAsync(editorPersonaId, subjectPersonaId, null!, ct)
  → for each assigned role:
      if role.Roletype is not null:
          → await _rpDocManagement.GetRoleClassifierDatasetAsync(editorPersonaId, subjectPersonaId, role.ID, null!, ct)
          → collect assigned ProductProperty.ID values
      → append PAMRolePropertyList to lstRoleProperties
  → return ProductBatch { ProductId = RPDocumentManagement, InputJson = { RolePropertiesList, UsePrimaryProperties } }
```

**Key change:** `RequestParameter datafilter` passed as `null!` to match the existing
sync call `GetPropertyRoles(createUserPersonaId, personaId, null)`.

### DI Registration

```csharp
services.AddScoped<IDocManagementBatchServiceAsync, DocManagementBatchServiceAsync>();
```

---

## `AoBatchServiceAsync` — Design Details

Replaces `BatchHelper.CreateAoBatchRecords(DefaultUserClaim userClaim, long editorPersonaId, long newUserPersonaId, …)`.

| Sync call | Replacement |
|-----------|-------------|
| `new SamlRepository().GetProductSamlDetails(newUserPersonaId, productId)` | `await _samlRepository.GetProductSamlDetailsAsync(newUserPersonaId, productId, ct)` |
| `new ManageProductAssetOptimization(userClaim)` constructor | `_userClaims.GetUserClaim()` resolves `DefaultUserClaim` without constructor injection |
| `manageAo.CopyRegularUser(editorPersonaId, newUserPersonaId)` | **// SYNC** — `CopyRegularUserAsync` does not exist in `IManageProductAssetOptimizationAsync` |

### DI Registration

```csharp
services.AddScoped<IAoBatchServiceAsync, AoBatchServiceAsync>();
```

---

## Remaining SYNC Annotations

| Location | Method | Blocked on |
|----------|--------|------------|
| `AoBatchServiceAsync.CreateAoBatchRecordsAsync` | `ManageProductAssetOptimization.CopyRegularUser` | `IManageProductAssetOptimizationAsync.CopyRegularUserAsync` |

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| Collection expressions `[]` | All `new List<T>()` initializers in `BatchHelperAsync`; `FormUrlEncodedContent` entries in `ActivityLogServiceAsync` |
| Property patterns in `switch` | Property-type dispatch in `BatchHelperAsync` replaces `GetType() == typeof(X)` |
| `switch` expressions | `allProperties` sentinel value selection in `CreateProductBatchRecord`; `GetUserAssignedPropertiesData` type dispatch |
| `is not null` / `is not { Count: > 0 }` patterns | Guard clauses throughout |
| `await using` scopes | `_scopeFactory.CreateAsyncScope()` in `ActivityLogServiceAsync` |
| `SemaphoreSlim(1, 1)` | Replaces `lock()` in all async-safe double-check patterns |
| `ArgumentNullException.ThrowIfNull` | All constructor guards |
| File-scoped namespaces | All new files |
| `ConfigureAwait(false)` | All `await` sites in async implementations |
| Expression-bodied static factory methods `=> new() { … }` | Simple `ProductBatch` factories in `BatchHelperAsync` |
| Nullable reference types | `string?`, `IList<ProductBatch>?`, `IList<ProductRole>?` throughout |
| `IServiceScopeFactory.CreateAsyncScope()` | Singleton→Scoped dependency resolution in `ActivityLogServiceAsync` |
