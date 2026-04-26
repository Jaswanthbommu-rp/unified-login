# ManageCloneProductBatchAsync Refactor Changelog

**Date:** 2026-04-19  
**Branch:** `feature/UserRepositoryRefactor`  
**Target:** .NET 10 / C# 13  

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/ManageCloneProductBatch.cs`
into `LogicAsync/`.

Removes every instance of:
- `new ManageProductXxx(_userClaim)` inline service instantiation (17 products)
- `DefaultUserClaim` constructor parameter on business logic classes (kept only as explicit `// SYNC` dependency)
- `new ProductInternalSettingRepository()` created N times in `IsProductEnabledForUsePrimaryProperty`
- 20+ `if/else if` product branches for dispatch
- Three identical flag-extraction helpers (`CheckForAllProperties`, `CheckForAllRegions`, `CheckForIsAssignedNewPropertyFlag`)

Replaces with:
- 19-dependency constructor injection on `ManageCloneProductBatchAsync`
- `PrefetchProductEnabledFlagsAsync` — single `Task.WhenAll` parallel fan-out replacing N serial DB calls
- `ResolveProductBatchAsync` — `switch` expression dispatch with one arm per product
- `GetBoolFlag(object?, string)` — single static helper replacing the three duplicates
- `ConfigureAwait(false)` on all `await` sites
- C# 13 / .NET 10 features throughout

---

## Files Created

### Interface

| File | Notes |
|------|-------|
| `LogicAsync/Interfaces/IManageCloneProductBatchAsync.cs` | New async interface |

### Implementation

| File | Replaces |
|------|----------|
| `LogicAsync/ManageCloneProductBatchAsync.cs` | `Logic/ManageCloneProductBatch.cs` |

---

## Dependency Mapping

### `ManageCloneProductBatchAsync` — 19 DI dependencies

| Dependency | Replaces |
|------------|---------|
| `IManageProductOneSiteAsync` | `new ManageProductOneSite(_userClaim)` |
| `IManageProductOneSiteAccountingAsync` | `new ManageProductOneSiteAccounting(_userClaim)` |
| `IManageProductMarketingCenterAsync` | `new ManageProductMarketingCenter(_userClaim)` |
| `IManageProductOpsAsync` | `new ManageProductOps(_userClaim)` |
| `IManageProductVendorServicesAsync` | `new ManageProductVendorServices(_userClaim)` |
| `IManageProductProspectContactAsync` | `new ManageProductProspectContact(_userClaim)` |
| `IManageProductLead2LeaseAsync` | `new ManageProductLead2Lease(_userClaim)` |
| `IManageProductResidentPortalAsync` | `new ManageProductResidentPortal(_userClaim)` |
| `IManageProductRentersInsuranceAsync` | `new ManageProductRentersInsurance(_userClaim)` |
| `IManageProductOnSiteAsync` | `new ManageProductOnSite(_userClaim)` *(OnSite product, not OneSite)* |
| `IManageProductRumAsync` | `new ManageProductRum(_userClaim)` |
| `IManageProductAdminSupportPortalAsync` | `new ManageProductAdminSupportPortal(_userClaim)` |
| `IManageBlueBookAsync` | `new ManageBlueBook(_userClaim)` |
| `IProductInternalSettingRepositoryAsync` | `new ProductInternalSettingRepository()` × N |
| `IIntegrationTypeFactoryAsync` | `new IntegrationTypeFactory(…, _userClaim)` |
| `IDocManagementBatchServiceAsync` | `new ManageProductRPDocumentManagement(_userClaim)` inline |
| `ISamlRepositoryAsync` | `new SamlRepository()` in `CreateAoBatchRecords` |
| `DefaultUserClaim` | Kept for three `// SYNC` blockers (see below) |
| `ILogger<ManageCloneProductBatchAsync>` | `Serilog.Log.Write(LogEventLevel.Error, …)` |

---

## Key Design Decisions

### 1. `IsProductEnabledForUsePrimaryProperty` → `PrefetchProductEnabledFlagsAsync`

The legacy called `IsProductEnabledForUsePrimaryProperty(productId)` once per product
inside the loop, each time constructing a `new ProductInternalSettingRepository()` and
executing a synchronous DB query:

```csharp
// Sync (before) — called N times, one per product in the loop
private bool IsProductEnabledForUsePrimaryProperty(int productId)
{
    var repo = new ProductInternalSettingRepository();   // new object every call
    var list = repo.GetProductInternalSettings(productId);
    return list.FirstOrDefault(i => i.Name == "UsePrimaryProperties")?.Value.Trim() == "1";
}
```

Replaced with a single parallel pre-fetch executed **once before the loop**:

```csharp
// Async (after) — all product IDs fetched in parallel before the loop
var enabledFlags = await PrefetchProductEnabledFlagsAsync(userProducts, cancellationToken);
// Inside loop:
bool productEnabledForPrimaryProperty = enabledFlags.GetValueOrDefault(product.ProductId);
```

`Task.WhenAll` issues all N repository reads concurrently; `AssetOptimizer` is appended
automatically when any AO sub-product is present (the AO path uses the same flag check).

### 2. `if/else if` chain → `switch` expression (`ResolveProductBatchAsync`)

The legacy had 20+ sequential `else if (product.ProductId == (int)ProductEnum.Xxx)`
branches. Replaced by a single `switch` expression:

```csharp
return productEnum switch
{
    ProductEnum.OneSite               => HandleOneSiteAsync(…),
    ProductEnum.FinancialSuite        => HandleFinancialSuiteAsync(…),
    // … one arm per product
    _ when !ProductEnumHelper.GetAoProductList().Contains(productEnum)
                                      => HandleIntegrationTypeAsync(…),
    _                                 => Task.FromResult<ProductBatch?>(null)
};
```

AO products fall to the final `_` arm (returning `null`) because they are skipped
at the top of the main loop via `continue` and handled separately by
`CreateAoCloneBatchRecordsAsync`.

### 3. Three duplicate flag helpers → single `GetBoolFlag`

The legacy had three near-identical private methods:

| Legacy method | Key looked for |
|---|---|
| `CheckForAllProperties` | `"allProperties"` |
| `CheckForAllRegions` | `"allProperties"` *(same key, different context)* |
| `CheckForIsAssignedNewPropertyFlag` | `"IsAssignedNewPropertyByDefault"` |

All three parsed a `Dictionary<string, bool>` cast from `ListResponse.Additional`.
Replaced by one static helper:

```csharp
private static bool GetBoolFlag(object? additional, string key) =>
    additional is Dictionary<string, bool> dict && dict.TryGetValue(key, out var val) && val;
```

Usage:
```csharp
bool allProps   = GetBoolFlag(propsResponse.Additional,   "allProperties");
bool allRegions = GetBoolFlag(regionResponse.Additional,  "allProperties");
bool isAssign   = GetBoolFlag(propsResponse.Additional,   "IsAssignedNewPropertyByDefault");
```

### 4. `CreateAoBatchRecords` → `CreateAoCloneBatchRecordsAsync`

The sync version called `manageProductAssetOptimization.CopyRegularUser(…)` and
re-ran `IsProductEnabledForUsePrimaryProperty((int)ProductEnum.AssetOptimizer)` inside
the method. The async version:

- Receives the pre-fetched `aoEnabled` flag (avoiding the extra DB call)
- Calls `ISamlRepositoryAsync.GetProductSamlDetailsAsync` for the external-user BI path
  (replaces the sync `new SamlRepository()`)
- `CopyRegularUser` remains sync (`// SYNC` — see blockers below)

### 5. `DefaultUserClaim` — scoped to three SYNC blockers

`DefaultUserClaim` is retained as a DI dependency **only** because three sync call sites
cannot yet be removed:

| Call site | Why it needs `_userClaim` |
|-----------|--------------------------|
| `new ManageProductClientPortal(_userClaim)` | `IManageProductClientPortalAsync` not yet created |
| `ManageProductFactory.GetProductLogic(…, _userClaim)` | No async port for `LeadManagement`, `LeadAnalytics`, `PortfolioManagement`, `DepositAlternative`, `ClickPay` |
| `new ManageProductAssetOptimization(_userClaim).CopyRegularUser(…)` | `CopyRegularUserAsync` not yet added to `IManageProductAssetOptimizationAsync` |

When these three blockers are resolved, `DefaultUserClaim` can be removed from the
constructor entirely.

### 6. `OnSite` property/region record types — `dynamic` dispatch

`OnSiteProperty`, `OnSiteRegion`, and `OnSiteRole` records returned by
`IManageProductOnSiteAsync` are read via `dynamic` cast in `BuildOnSiteBatch`
to avoid a hard dependency on the `UnifiedLogin.SharedObjects.Product.OnSite` namespace
(which the async infrastructure layer does not otherwise import). This is a deliberate
trade-off: if the concrete types change, the failure surfaces at runtime rather than
compile-time. A follow-up refactor should introduce a typed cast once the namespace
dependency is confirmed.

### 7. `VendorServices` — all-unassigned detection preserved

The legacy `CreateVendorServiceProductBatchRecord` contained:

```csharp
var unselectedCount = propertiesCollection.Where(p => !((ProductProperty)p).IsAssigned).Count();
if (unselectedCount == propertiesCollection.Count())
    allProperties = true;
```

This detects the edge case where the source user's properties are all unassigned
(meaning they had access to all properties). Preserved in `BuildVendorServicesBatch`.

### 8. `RUM` — all-unassigned detection preserved

Same edge case exists for RUM: when all properties are unassigned and there are no
property groups, the legacy added `"All"` to the property list. Preserved in
`BuildRumBatch`.

### 9. `ResidentPortal` — null-safe level lookup

Legacy: `rolesResponse.Find(item => item.IsAssigned == true).Id.ToUpper()` — throws
`NullReferenceException` if no level is assigned. Async version uses null-safe pattern:

```csharp
var assignedLevel = levelList.Find(l => l.IsAssigned);
if (assignedLevel is not null)
    roleList.Add(assignedLevel.Id.ToUpper());
```

---

## Resolved SYNC blockers (2026-04-19 update)

`ClientPortal` product is deprecated — handler removed entirely (no async port needed).

The five StandardV1 products previously dispatched via `ManageProductFactory.GetProductLogic` are
now fully async using `IIntegrationTypeFactory.GetIntegration(productId)`:

| Product | Method | Replaces |
|---------|--------|----------|
| `LeadManagement` | `HandleLeadManagementAsync` | `ManageProductFactory.GetProductLogic` + `GetProductUser().Roles` |
| `LeadAnalytics` | `HandleLeadAnalyticsAsync` | same + `GetProductUser().PropertyGroups` via `GetPropertyGroupsAsync` |
| `PortfolioManagement` | `HandlePortfolioManagementAsync` | `GetProductUser().PropertyRoleList` via `GetPropertiesAsync` → `PortfolioRoleProperty` cast; `GetProductUser().RoleList` via `GetRolesAsync` |
| `DepositAlternative` | `HandleDepositAlternativeAsync` | `GetProductUser()` fields via `GetRolesAsync` + `GetPropertiesAsync` + `GetPropertyGroupsAsync`; `CanReceiveMonthlyReport` extracted from `rolesResp.Additional` |
| `ClickPay` | `HandleClickPayAsync` | `GetProductUser().OrganizationRoles` via `GetRolesAsync` → `ClickPayRole.SelectedItems` unwrapped into `OrganizationRole` list |

New helpers introduced: `ExtractStandardV1Roles(ListResponse)` and
`ExtractStandardV1Groups(ListResponse)` — cast `Records` to
`Logic.ProductIntegration.Model.ProductRole` / `ProductPropertyGroups` and collect assigned IDs.

`BuildDepositAlternativeBatch` signature updated from `IntegrationProductUser` parameter to
individual `roles`, `canReceiveMonthlyReport`, `properties`, `propertyGroups` parameters.

## AO clone path fully resolved (2026-04-19 update)

`IManageProductAssetOptimizationAsync.CopyRegularUserAsync` added as a public method:

```csharp
Task<IList<AoUserCompanyPropertyRoleDetail>> CopyRegularUserAsync(
    long editorPersonaId, long subjectPersonaId,
    string? productUserName = null, CancellationToken cancellationToken = default);
```

`ManageProductAssetOptimizationAsync` implementation delegates to the existing private
`CopyRegularUserAsync(editorPersonaId, subjectPersonaId, settings, editorProductUserId, …)`
after resolving `AoApiSettings` and `EditorProductUserId` via `GetApiSettingsAsync` /
`GetCallContextAsync` internally.

`ManageCloneProductBatchAsync` now injects `IManageProductAssetOptimizationAsync` and calls
`CopyRegularUserAsync` in `CreateAoCloneBatchRecordsAsync` — the last `new ManageProductAssetOptimization(_userClaim)` instantiation is removed.

`DefaultUserClaim` is retained for:
- `HandleKnockCRMAsync` / `HandleIntegrationTypeAsync` — `_userClaim.OrganizationPartyId` passed as `partyId` to `GetRolesAsync`

---

## DI Registration Template

```csharp
services.AddScoped<IManageCloneProductBatchAsync, ManageCloneProductBatchAsync>();
```

All 18 non-`DefaultUserClaim` dependencies are already registered individually by their
respective refactor PRs. `DefaultUserClaim` is registered per-request by the auth
middleware (existing registration — no change needed).

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| `switch` expression | `ResolveProductBatchAsync` product dispatch (replaces 20+ `if/else if` branches) |
| Collection expressions `[]` | Empty `IList<AoUserCompanyPropertyRoleDetail>` init; `PropertyList = []` |
| Spread `[.. detail.SelectedRoleValues]` | AO role list construction in `CreateAoCloneBatchRecordsAsync` |
| `??=` null-coalescing assignment | `detail.SelectedPortfolioValues ??= []`; `detail.PropertyGroups ??= []` |
| `?.` null-conditional | `productSetting?.Value.Trim()`, `assignedLevel?.Id`, `notification?.IsInsuranceExpired` |
| `is not null` pattern | `if (assignedLevel is not null)`, `if (batch is not null)` |
| `is { }` pattern | `if (companiesResponse.Additional is AccountingUser accountingUser)` |
| `ArgumentNullException.ThrowIfNull` style (`?? throw`) | Constructor guards on all 19 deps |
| `ConfigureAwait(false)` | All `await` sites |
| File-scoped namespaces | All new files |
| `GetValueOrDefault` | `enabledFlags.GetValueOrDefault(product.ProductId)` — safe dict lookup |
| Primary expression `new()` | Target-typed `new()` in all static batch builders |
| `Task.WhenAll` with LINQ `.Select(async …)` | Parallel pre-fetch in `PrefetchProductEnabledFlagsAsync` |
