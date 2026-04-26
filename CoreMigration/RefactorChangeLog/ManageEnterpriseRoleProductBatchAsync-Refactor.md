# ManageEnterpriseRoleProductBatchAsync Refactor Changelog

**Date:** 2026-04-20  
**Branch:** `feature/UserRepositoryRefactor`  
**Target:** .NET 10 / C# 13  

---

## Summary

Full async refactor of `Logic/ManageEnterpriseRoleProductBatch.cs` and
`Logic/ManageEnterpriseRolesPrimaryProperties.cs` into `LogicAsync/`.

Eliminates all `DefaultUserClaim` mutations, inline `new Xxx(_userClaim)` instantiations,
`RPObjectCache`/`MemoryCache.Default` usage, Serilog static logging, and synchronous
repository calls. The original classes are retained as-is for legacy consumers; the async
versions are new files.

---

## Files Created

| File | Notes |
|------|-------|
| `LogicAsync/Interfaces/IManageEnterpriseRoleProductBatchAsync.cs` | New interface — `GenerateEnterpriseRoleUserProductBatchAsync` |
| `LogicAsync/Interfaces/IManageEnterpriseRolesPrimaryPropertiesAsync.cs` | New interface — `ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync` |
| `LogicAsync/ManageEnterpriseRoleProductBatchAsync.cs` | Thin async orchestrator |
| `LogicAsync/ManageEnterpriseRolesPrimaryPropertiesAsync.cs` | Full async port of primary-properties logic |
| `ManageEnterpriseRoleProductBatchAsync-Refactor.md` | This file |

---

## Key Changes

### 1. `DefaultUserClaim` mutation pattern removed

The original `GenerateEnterpriseRoleUserProductBatch` mutated a shared `_userClaim` object
before passing control to the inner class:

```csharp
// Before: shared mutable state, fragile in multi-threaded batch contexts
_userClaim.UserRealPageGuid          = editorPersona.RealPageId;
_userClaim.OrganizationRealPageGuid  = editorPersona.Organization.RealPageId;
_userClaim.Rights                    = _manageProductBatch.GetPersonaRoleRights(...);
_userClaim.OrganizationPartyId       = editorPersona.OrganizationPartyId;
_manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(_userClaim);
```

The async version loads editor persona context directly inside
`ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync` and passes it through local variables.
`IUserClaimsAccessor` is used for `ImpersonatedBy`; persona data comes from `GetPersonaAsync`.

---

### 2. 15+ inline `new Xxx(_userClaim)` instantiations replaced with DI

**Before:**
```csharp
_manageProductBatch          = new ManageProductBatch(_userClaim);
_productRepository           = new ProductRepository();
_propertyRepository          = new PropertyRepository();
_integrationTypeFactory      = new IntegrationTypeFactory(manageProduct, ...);
_productInternalSettingRepo  = new ProductInternalSettingRepository();
_unifiedSettingsRepository   = new UnifiedSettingsRepository();
_managePersona               = new ManagePersona(_userClaim);
_personaRepository           = new PersonaRepository(_userClaim);
_userLoginRepository         = new UserLoginRepository();
_enterpriseRoleProductRepository = new BatchProductBulkUpdateRepository(_userClaim);
_userRoleRightRepository     = new UserRoleRightRepository();
// + ManageProductAdminSupportPortal, ManageProductRealConnect created inline in methods
```

**After:** 15 DI-injected async interfaces on the constructor — all repositories and logic
classes resolved by the container.

---

### 3. 3 synchronous DB queries per loop iteration eliminated (performance)

`GetPrimaryPropertySettingsForCompanyAndProduct` was called inside the product `foreach` loop,
issuing 3 DB queries per product:
- `GetProductSettingByType("UsePrimaryProperties")`
- `GetProductSettings(orgRealPageGuid)`
- `GetUnifiedSettings(orgPartyId, "Company")`

**After:** All three are fetched **once before the loop** using `Task.WhenAll`, then evaluated
with the pure static `ComputePrimaryPropertyEnabled` helper per product.  
Result: N×3 DB calls → 3 DB calls regardless of product count.

---

### 4. Parallel async data fetching in Phase 1–3

Where loads are independent they are now parallel:

| Phase | What runs in parallel |
|-------|-----------------------|
| Phase 1 | `GetPersonaAsync(editor)` + `GetPersonaAsync(user)` + `GetPersonaProductSettingsAsync` |
| Phase 2 | `ListOrganizationByEnterpriseUserIdAsync` + `GetUserLoginOnlyAsync` (impersonator) |
| Phase 3 (enterprise role) | `GetEnterpriseRoleNewProductsAsync` + `GetEnterpriseRoleUpdatedProductsAsync` + `GetEnterpriseRoleDeletedProductsAsync` + `GetRoleTemplateProductRoleMappingAsync` |
| Phase 4 | `GetProductSettingByTypeAsync` + `GetProductSettingsAsync` + `GetUnifiedSettingsAsync` |

---

### 5. `RPObjectCache` / `MemoryCache.Default` replaced with `IMemoryCache`

```csharp
// Before: static singleton cache
var rpcache = new RPObjectCache();
var productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () => ...);

// Before: direct MemoryCache.Default mutation
MemoryCache.Default.Remove(cacheKey);
```

```csharp
// After: injected IMemoryCache
if (!_cache.TryGetValue(cacheKey, out List<ProductInternalSetting> settings))
{
    settings = ...;
    _cache.Set(cacheKey, settings, TimeSpan.FromMinutes(2));
}

// cache invalidation
_cache.Remove(cacheKey);
```

---

### 6. Serilog static `Log.Write` replaced with `ILogger<T>`

```csharp
// Before
Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", ...);
Log.Write(LogEventLevel.Error, exception: ex, ...);

// After
_logger.LogDebug("{CorrelationId} {Type} NewProducts={Products}", ...);
_logger.LogError(ex, "{CorrelationId} Exception during {Type} ...", ...);
```

Structured logging with correlation ID; zero dictionary allocation.

---

### 7. `ManageProductBatch` methods inlined or wrapped

`ManageProductBatch` has no async interface yet. Its methods are handled as follows:

| Method | Async treatment |
|--------|----------------|
| `IsProductEnabledForUsePrimaryProperty` | Inlined with `IProductInternalSettingRepositoryAsync.GetProductInternalSettingsAsync` + `IMemoryCache` |
| `GetEnterpriseRoleUserPrimaryPropertiesData` | Inlined with `IManageProductPanelAsync.GetProductPropertiesAsync` + `CompareProductAndPrimaryPropertiesAsync` |
| `GetProductRoles` | Replaced with `IManageProductPanelAsync.GetProductRolesAsync` |
| `GetExistingUserPrimaryPropertiesData` | Replaced with `IPropertyRepositoryAsync.ListUPFMPropertyInstanceIdByPersonaAsync` |
| `GetPersonaRoleRights` | Removed — was used only to populate `_userClaim.Rights`, which is no longer mutated |
| `GetProductBatchRecord` | Async-over-sync adapter via `Task.Run(() => new ManageProductBatch(claim).GetProductBatchRecord(...))` — see Known Limitations |

---

### 8. `ManageProductAdminSupportPortal.GetProperties` replaced

```csharp
// Before: inline instantiation
ManageProductAdminSupportPortal asp = new ManageProductAdminSupportPortal(_userClaim);
var userProperties = asp.GetProperties(editorPersonaId, subjectPersonaId, null);

// After: DI-injected async interface
var userProperties = await _adminSupportPortal
    .GetPropertiesAsync(editorPersonaId, subjectPersonaId, null!, cancellationToken)
    .ConfigureAwait(false);
```

---

### 9. KnockCRM property groups ported to async

```csharp
// Before: sync factory call
var propertyGroupResponse = integrationTypeFac.GetPropertyGroups(editorId, userId, null, null);

// After: async integration type
var knockIntegration = _integrationTypeFactory.GetIntegration(product);
var propertyGroupResponse = await knockIntegration
    .GetPropertyGroupsAsync(editorId, userId, null!, cancellationToken: ct)
    .ConfigureAwait(false);
```

---

### 10. `_userClaim.PersonaId = editorUserPersonaId` mutation removed

The original RealConnect block mutated `_userClaim.PersonaId` before creating
`ManageProductRealConnect`. This mutation is removed. The impersonation guard in
`GetUserLoginOnlyAsync` uses `IUserClaimsAccessor.ImpersonatedBy` instead.

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespace | All new files |
| `sealed` class | `ManageEnterpriseRoleProductBatchAsync`, `ManageEnterpriseRolesPrimaryPropertiesAsync` |
| `ConfigureAwait(false)` | All `await` sites |
| Collection expressions `[]` | Empty list initialization throughout |
| `??=` null-coalescing assignment | `roleProp.PropertyGroupList ??= []` |
| `is not null` / `is null` pattern matching | Null checks throughout |
| `..` spread operator | `[.. roleTemplateNewProducts.Distinct()]` |
| `Task.WhenAll` with collection spread | `[organizationsTask, .. (impersonatorTask is not null ? [impersonatorTask] : [])]` |
| `ArgumentNullException.ThrowIfNull` | Batch parameter guard |
| Tuple deconstruction | Phase 3 parallel fetch |

---

## Known Limitations

### RealConnect position assignment not fully ported

`ManageProductRealConnect.GetUser(learnerId)` and `GetClientLicenseDetailsCaching()` are
internal HTTP calls not exposed on `IManageProductRealConnectAsync`. The async version creates
the `RCProductBatch` with an empty `LearnerLicenseId`, preserving batch-record structure but
without position pre-selection.

**Resolution:** Extend `IManageProductRealConnectAsync` with `GetLearnerInfoAsync` and
`GetClientLicensesAsync` methods, then populate `positionsToAdd` from those results.

### `GetProductBatchRecord` uses async-over-sync adapter

`ManageProductBatch.GetProductBatchRecord` has product-specific branches (FinancialSuite,
VendorServices, ResidentPortal, OnSite, UtilityManagement, etc.) that use inline
`new ManageProductXxx(_userClaim)` sync instances. Until `IManageProductBatchAsync` is
introduced with per-product async implementations, the adapter wraps the sync call in
`Task.Run` to avoid blocking request threads.

The generic `BatchHelper.CreateProductBatchRecord` path — used by most enterprise role
products — involves no I/O and is unaffected by this limitation.

---

## DI Registration Template

```csharp
services.AddScoped<IManageEnterpriseRoleProductBatchAsync, ManageEnterpriseRoleProductBatchAsync>();
services.AddScoped<IManageEnterpriseRolesPrimaryPropertiesAsync, ManageEnterpriseRolesPrimaryPropertiesAsync>();
```

All injected interfaces are already registered by existing infrastructure. No additional
registrations required.
