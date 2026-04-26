# ManageProductBatchAsync Refactor Changelog

**Date:** 2026-04-21
**Branch:** `feature/UserRepositoryRefactor`
**Target:** .NET 10 / C# 13

---

## Summary

Full async refactor of `Logic/ManageProductBatch.cs` into `LogicAsync/`.
Original class retained as-is for legacy consumers. Two new files created.

---

## Files Created

| File | Notes |
|------|-------|
| `LogicAsync/Interfaces/IManageProductBatchAsync.cs` | New async interface — 8 methods |
| `LogicAsync/ManageProductBatchAsync.cs` | Full async implementation |
| `ManageProductBatchAsync-Refactor.md` | This file |

---

## Key Changes

### 1. All inline `new Xxx(...)` instantiations replaced with DI

**Before:** Two constructors, collectively containing 12 inline `new` calls:
```csharp
var manageProduct = new ManageProduct(_userClaims);
var manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
_productRepository = new ProductRepository();
_integrationTypeFactory = new IntegrationTypeFactory(manageProduct, ...);
_userRoleRightRepository = new UserRoleRightRepository();
_manageProductPanel = new ManageProductPanel(_userClaims);
// ...
```

**After:** Single DI constructor with 23 injected interfaces — every dependency is testable, mockable,
and lifetime-managed by the container. Parameterless and `IRepository`-accepting constructors removed.

---

### 2. `DefaultUserClaim` dependency eliminated from all method signatures

**Before:**
```csharp
public ListResponse GetProductProperties(long editorPersonaId, long userPersonaId,
    int productId, DefaultUserClaim userClaim)
public ListResponse GetProductRoles(long editorPersonaId, long userPersonaId,
    int productId, long partyId, DefaultUserClaim userClaim)
```

**After:** `DefaultUserClaim` parameter removed from both methods — ambient context is provided
by the injected `IUserClaimsAccessor`.

---

### 3. `GetProductBatchRecordAsync` — product dispatch modernised

**Before:** Long if/else-if chain with inline `new ManageProductXxx(_userClaims)` per branch:
```csharp
if (product == (int)ProductEnum.FinancialSuite)
{
    ManageProductOneSiteAccounting accounting = new ManageProductOneSiteAccounting(_userClaims);
    ...
}
else if (product == (int)ProductEnum.VendorServices)
{
    ManageProductVendorServices vs = new ManageProductVendorServices(_userClaims);
    ...
}
```

**After:** `switch ((ProductEnum)product)` with each case using the pre-injected async service.
No inline construction, no `DefaultUserClaim` threading.

---

### 4. Parallel I/O inside product-specific branches — `Task.WhenAll`

Five product branches make two independent async calls that were previously sequential. All five
now run concurrently:

| Product | Parallel calls |
|---------|---------------|
| `FinancialSuite` | `GetUserPropertyGroupsAsync` + `GetUserCompaniesAsync` |
| `VendorServices` | `GetNotificationSettingsAsync` + `GetPropertyGroupsAsync` |
| `ResidentPortal` | `GetNotificationSettingsAsync` + `ListMessageGroupsAsync` |
| `UtilityManagement` | `GetPropertyGroupsAsync` + `GetRegionsAsync` |

Latency reduction: max(t₁, t₂) instead of t₁ + t₂ for each branch.

---

### 5. `ManageProductFactory` → `ManageProductFactoryAsync.CreateAndInitAsync`

**Before:**
```csharp
var productLogic = ManageProductFactory.GetProductLogic(product, editorPersonaId, subjectPersonaId, _userClaims);
var productUser  = productLogic.GetProductUser();
```
`GetProductLogic` blocks while fetching product data inside the constructor.

**After:**
```csharp
var productLogic = await ManageProductFactoryAsync.CreateAndInitAsync(
    product, editorPersonaId, subjectPersonaId,
    _dataCollector, _productRepo, _managePersona, _manageUserLogin,
    _userClaims, _httpClientFactory, _tokenHelper, _cacheService,
    _loggerFactory, _samlAttributeService, cancellationToken);
var productUser = productLogic.GetProductUser();
```
`InitAsync` performs all I/O asynchronously before returning the ready-to-query instance.
Applies to: `DepositAlternative`, `LeadManagement`, `LeadAnalytics`, `PortfolioManagement`.

---

### 6. `GetUserPrimaryPropertiesData` — concurrent user-props + panel call

**Before:**
```csharp
var userProperties = _propertyRepository.ListUPFMPropertyInstanceByPersona(...); // blocks
result = manageProductPanel.GetProductProperties(...);                           // blocks
```
Sequential blocking. Also created a redundant `new ManageProductPanel(_userClaims)` inline despite
having `_manageProductPanel` as a field.

**After:**
```csharp
var userPropsTask = _propertyRepo.ListUPFMPropertyInstanceByPersonaAsync(...);
var panelTask     = _manageProductPanel.GetProductPropertiesAsync(...);
await Task.WhenAll(userPropsTask, panelTask).ConfigureAwait(false);
```
Both I/O calls run concurrently; inline panel construction removed.

---

### 7. `GetEnterpriseRoleUserPrimaryPropertiesData` — KnockCRM branch restructured

**Before:** Interleaved if/else with duplicated `userProperties` loading for KnockCRM and non-KnockCRM
paths, with intermediate dead variable `productPropertyIdList`.

**After:** Early-return for KnockCRM, followed by the concurrent pattern for all other products.
Dead variable removed.

---

### 8. `GetExistingUserPrimaryPropertiesData` — zero-overhead pass-through

**Before:** One-liner wrapper that could not be `async` without a state-machine:
```csharp
public List<int> GetExistingUserPrimaryPropertiesData(long userPersonaId, int productId)
    => _propertyRepository.ListUPFMPropertyInstanceIdByPersona(userPersonaId, productId);
```

**After:** Returns the `Task` directly — no `async/await`, no state machine allocation:
```csharp
public Task<List<int>> GetExistingUserPrimaryPropertiesDataAsync(...) =>
    _propertyRepo.ListUPFMPropertyInstanceIdByPersonaAsync(userPersonaId, productId, cancellationToken);
```

---

### 9. `RPObjectCache` → `IMemoryCache` in `GetPersonaRoleRightsAsync`

**Before:**
```csharp
RPObjectCache rpCache = new RPObjectCache();
IList<UserRoleRights> roleList = rpCache.GetFromCache<IList<UserRoleRights>>(cacheKey, 60, () =>
{
    IList<int> productList = _sharedDataRepository.GetProductIdsByCompany(orgPartyId);
    return _userRoleRightRepository.GetAllRoleRights(orgPartyId, productList, ...);
});
```
`RPObjectCache` wraps `MemoryCache.Default` (static, legacy). The factory callback is synchronous,
blocking while performing two DB calls.

**After:**
```csharp
if (!_memoryCache.TryGetValue(cacheKey, out IList<UserRoleRights> roleList))
{
    var productIds = await _sharedDataRepo.GetProductIdsByCompanyAsync(orgPartyId, cancellationToken)
        .ConfigureAwait(false);
    roleList = await _userRoleRightRepo.GetAllRoleRightsAsync(orgPartyId, productIds, ...)
        .ConfigureAwait(false);
    _memoryCache.Set(cacheKey, roleList, TimeSpan.FromMinutes(60));
}
```
`IMemoryCache` is DI-injected, test-replaceable, and integrated with the ASP.NET Core lifetime.

---

### 10. `GetPersonaRoleRightsAsync` — null-safety bug fix

**Before:**
```csharp
foreach (Right right in roleList.FirstOrDefault(r => r.RoleId == userRole.RoleID).UserRights)
```
`FirstOrDefault` returns `null` when no matching role is found; `.UserRights` then throws
`NullReferenceException` silently in production.

**After:**
```csharp
var matchedRole = roleList?.FirstOrDefault(r => r.RoleId == userRole.RoleID);
if (matchedRole?.UserRights is null) continue;
foreach (var right in matchedRole.UserRights) { ... }
```

---

### 11. `GetPersonaRoleRightsAsync` — `List<string>` dedup → `HashSet<string>`

**Before:**
```csharp
List<string> userRights = new List<string>();
...
if (!userRights.Contains(right.RightNickName))  // O(n) scan
    userRights.Add(right.RightNickName);
```
`List.Contains` is O(n); for N roles × M rights per role, total work is O(N·M·R).

**After:**
```csharp
var userRights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
...
userRights.Add(right.RightNickName);  // O(1) dedup, case-insensitive
return [.. userRights];               // C# 13 collection expression
```

---

### 12. `IsProductEnabledForUsePrimaryPropertyAsync` — simplified boolean

**Before:**
```csharp
return productInternalSetting.Value.Trim() == "1" ? true : false;
```

**After:**
```csharp
return setting?.Value?.Trim() == "1";
```
Ternary `? true : false` is redundant; null-propagation removes the preceding null check.

---

### 13. Dead code removed

- `productPropertyIdList = new List<string>()` in both `GetUserPrimaryPropertiesData` and
  `GetEnterpriseRoleUserPrimaryPropertiesData` — declared and never written or read.
- Redundant `new ManageProductPanel(_userClaims)` inside `GetUserPrimaryPropertiesData` when
  `_manageProductPanel` field already existed.

---

### 14. Commented-out code not carried forward

`ClickPay` and `RPDocumentManagement` branches were commented out in the original with `// Don't know how it works`. These are omitted from the async implementation; the `default` branch handles them via `IIntegrationTypeFactoryAsync`.

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespace | Both new files |
| `sealed` class | `ManageProductBatchAsync` |
| Collection expression `[.. set]` | `GetPersonaRoleRightsAsync` return |
| `switch` on enum | `GetProductBatchRecordAsync` product dispatch |
| `is null` pattern | Null-safety in role-rights loop |
| `ConfigureAwait(false)` | All `await` sites |
| Expression-bodied methods | `GetExistingUserPrimaryPropertiesDataAsync`, `GetProductPropertiesAsync`, `GetProductRolesAsync` |

---

## DI Registration Template

```csharp
services.AddScoped<IManageProductBatchAsync, ManageProductBatchAsync>();
services.AddMemoryCache(); // required for IMemoryCache

// Prerequisites (must already be registered):
// All IManageProduct*Async product services
// IManageProductPanelAsync, IIntegrationTypeFactoryAsync
// IPropertyRepositoryAsync, IProductInternalSettingRepositoryAsync
// IUserRoleRightRepositoryAsync, ISharedDataRepositoryAsync, IProductRepositoryAsync
// IDataCollectorAsync, IManagePersonaAsync, IManageUserLoginAsync
// IHttpClientFactory, ITokenHelperAsync, ICacheService, ILoggerFactory
// ISamlAttributeServiceAsync, IUserClaimsAccessor
```

---

## Known Limitations

### `ManageProductFactoryAsync` is `internal`

`ManageProductFactoryAsync.CreateAndInitAsync` is marked `internal` to the
`UnifiedLogin.BusinessLogic` assembly. `ManageProductBatchAsync` lives in the same assembly so the
call is valid, but the coupling is implicit. A future refactor could extract a public
`IStandardV1ProductIntegrationFactoryAsync` to make this boundary explicit and testable.
