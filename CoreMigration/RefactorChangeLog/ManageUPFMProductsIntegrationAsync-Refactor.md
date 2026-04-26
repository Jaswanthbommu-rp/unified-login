# ManageUPFMProductsIntegrationAsync – Refactor Changelog

## Source
`UnifiedLogin.BusinessLogic/Logic/Product/ManageUPFMProductsIntegration.cs`  
_(File is excluded from compilation: `<Compile Remove="Logic\Product\ManageUPFMProductsIntegration.cs" />`)_

## Output
| File | Action |
|------|--------|
| `LogicAsync/Interfaces/IManageUPFMProductsIntegrationAsync.cs` | Created – true-async interface (no `DefaultUserClaim`) |
| `LogicAsync/Product/ManageUPFMProductsIntegrationAsync.cs`     | Created – native async implementation |
| `LogicAsync/ManageUPFMProductsIntegrationAsync.cs`             | Created – namespace-only placeholder |

---

## Production Fixes Ported from .NET 4.8

Three production bug fixes applied to the .NET 4.8 sync code have been carried forward into the async implementation.

### Fix 1 — Sentinel `-1` routed outside the TVP (`ProcessPropertyOperationsBatchedAsync`)

**Root cause**: The TVP stored procedure (`BulkCreateDeleteUPFMPropertyInstanceMapping`) rejects `-1` as a
`PropertyInstanceID`. Passing it caused a stored-procedure-level error in production.

**Fix**: Any `-1` entries in `unassignedProperties` or `assignedProperties` are pulled out before building
the TVP batch and routed through `InsertRemoveAssignedPropertyInstanceToUserAsync` individually.
Only real property IDs (`id > 0`) enter the TVP dictionary.

```csharp
foreach (var prop in unassignedProperties.Where(p => p == "-1"))
    await _propertyRepository.InsertRemoveAssignedPropertyInstanceToUserAsync(
        userPersonaId, effectiveProductId, -1, remove: 1, ct);
// … then build TVP batch with id > 0 only
```

### Fix 2 — TVP deduplication prevents PRIMARY KEY violation (`ProcessPropertyOperationsBatchedAsync`)

**Root cause**: The original code aliased `RemovedPropertyList` without copying it, so `AddRange` could add
the same property ID twice (once from `RemovedPropertyList`, once from the contains-`-1` expansion path).
The TVP stored procedure enforces a PRIMARY KEY on `PropertyInstanceID`, causing SQL error 3602.

**Fix**: Use a `Dictionary<long, UPFMPropertyInstanceMapping>` keyed on `PropertyInstanceID` to deduplicate
before calling the TVP. Assign takes priority over unassign for the same ID (last-write wins):

```csharp
var seen = new Dictionary<long, UPFMPropertyInstanceMapping>();
foreach (var prop in unassignedProperties)
    if (long.TryParse(prop, out long id) && id > 0)
        seen[id] = new UPFMPropertyInstanceMapping { PropertyInstanceID = id, IsDeleted = true };
foreach (var prop in assignedProperties)
    if (long.TryParse(prop, out long id) && id > 0)
        seen[id] = new UPFMPropertyInstanceMapping { PropertyInstanceID = id, IsDeleted = false };
```

### Fix 3 — `allProperties` sentinel respected in property merge (`MergeUPFMBooksPropertiesWithProductPropertyAsync`)

**Root cause**: When a user had the `-1` all-properties row in the DB, properties were never marked
`IsAssigned = true` because the `allProperties` flag was computed but then never used in the predicate.

**Fix**: The condition now checks `allProperties` first:

```csharp
// Before (bug):
if (userPropertySet.Contains(upfm.PropertyInstanceId))
    pp.IsAssigned = true;

// After (fix):
if (allProperties || userPropertySet.Contains(upfm.PropertyInstanceId))
    pp.IsAssigned = true;
```

---

## Breaking Changes

### `DefaultUserClaim` removed from all public APIs
Constructor no longer accepts `DefaultUserClaim`. Editor/user context (personas, product user ID/username) is
resolved internally per-call via `IProductContextServiceAsync.GetUserContextAsync`.

### `GetEnterpriseUPFMPropertiesAsync` — new `editorPersonaId` parameter
The sync version obtained the editor org real page ID from `_userClaims.OrganizationRealPageGuid`.
The async interface adds `editorPersonaId` as the first parameter to enable per-call context resolution.

### `GetUPFMMultiCompanyPropertiesAsync` — new `editorPersonaId` parameter
The sync version used `_userClaims.LoginName` to look up company memberships.
The async version derives the login name from the editor persona passed in.

### `ManageUPFMProductUserAsync` — `out` parameter → tuple return
Original: `bool ManageUPFMProductUser(..., out List<AdditionalParameters> additionalParameters)`  
Async: `Task<(string result, List<AdditionalParameters> auditParams)> ManageUPFMProductUserAsync(...)`  
`out` parameters are incompatible with `async/await`; the audit list is now part of the return value.

### `GetProductIdsByOrg()` dead code eliminated
The sync `GetRoles` and `GetRightsByRole` called `GetProductIdsByOrg()` (using the editor's
`OrganizationRealPageGuid`) but immediately discarded the result. The subsequent
`pr.GetProductIdsByCompany(partyId)` (the explicit parameter) was the one actually used.
The dead call is removed from the async version.

---

## Constructor — from 1 to 10 dependencies

| # | Dependency | Replaces |
|---|-----------|---------|
| 1 | `int productId` (constructor param) | `_productId` / `_upfmProductId` mutable instance fields |
| 2 | `IProductContextServiceAsync` | `DefaultUserClaim` + `GetCompanyEditorAndUserDetails` |
| 3 | `IProductRepositoryAsync` | `new ProductRepository()` inline constructions |
| 4 | `IProductInternalSettingRepositoryAsync` | `new ProductInternalSettingRepository()` inline + `ManageProductBase._productSettings` |
| 5 | `IManageBlueBookAsync` | `new ManageBlueBook()` inline constructions |
| 6 | `IPropertyRepositoryAsync` | `new PropertyRepository()` inline constructions |
| 7 | `IManageUserRoleRightAsync` | `new UserRoleRightRepository()` inline + base helper `GetAssignedRoleForPersona` |
| 8 | `IManageUserLoginAsync` | `_manageUserLogin.GetUserLoginOnly` (sync base helper) |
| 9 | `IUnifiedLoginRepositoryAsync` | `new UnifiedLoginRepository()` inline in `GetRightsByRole` |
| 10 | `ILogger<ManageUPFMProductsIntegrationAsync>` | `Serilog.Log.Write(...)` / `WriteToDiagnosticLog` / `WriteToErrorLog` |

`ArgumentNullException.ThrowIfNull` guard applied to each service dependency.

---

## Mutable instance fields eliminated

| Old field | Problem | New approach |
|-----------|---------|-------------|
| `int _productId` | Set in constructor, mutated by `GetSharedProductDetails` | Immutable constructor parameter; shared ID resolved via `GetSharedProductIdAsync` (returns value) |
| `int _upfmProductId` | Secondary mutable alias set alongside `_productId` | Removed; `GetSharedProductIdAsync` returns the effective ID |
| `string _udmSourceCode` | Set once in constructor via sync DB fetch | Resolved per-call via `ResolveUdmSourceCodeAsync` |
| `DefaultUserClaim _userClaims` | Per-request context tied to instance | `ProductCallContext` returned per-call by `IProductContextServiceAsync` |

---

## `GetSharedProductDetails` mutable side-effect → pure async helper

### Original
```csharp
private void GetSharedProductDetails(List<int> productIdList)
{
    var sharedProducts = _productRepository.GetProductSettingByType(SettingConstants.SharedProductSettingName);
    var match = sharedProducts?.FirstOrDefault(m => m.ProductId == _productId);
    if (match is not null && int.TryParse(match.Value, out int id))
    {
        _upfmProductId = id;   // mutates instance state
        _productId     = id;   // mutates instance state
        if (!productIdList.Contains(id)) productIdList.Add(id);
    }
}
```

### New
```csharp
private async Task<int> GetSharedProductIdAsync(List<int> productIdList, CancellationToken ct)
{
    var sharedProducts = await _internalSettingRepository.GetProductSettingByTypeAsync(
        SettingConstants.SharedProductSettingName, ct);

    if (sharedProducts?.FirstOrDefault(m => m.ProductId == _productId) is { } match &&
        int.TryParse(match.Value, out int resolvedId))
    {
        if (!productIdList.Contains(resolvedId))
            productIdList.Add(resolvedId);
        return resolvedId;
    }
    return _productId;
}
```

Pure function: returns the effective ID; does not mutate any instance fields.

---

## UDM source code — mutable field → per-call async resolution

### Original
```csharp
// Set once in constructor:
var detail = _productRepository.GetBooksMasterProductDetail(_productId);
_udmSourceCode = detail?.UDMSourceCode ?? detail?.BooksProductCode ?? string.Empty;
```

### New
```csharp
private async Task<string> ResolveUdmSourceCodeAsync(CancellationToken ct)
{
    var detail = await _productRepository.GetBooksMasterProductDetailAsync(_productId, ct);
    return detail?.UDMSourceCode?.Length > 0
        ? detail.UDMSourceCode
        : detail?.BooksProductCode ?? string.Empty;
}
```

Resolved per-call, not cached in an instance field. Eliminates the constructor's sync DB hit and the
state mutation race condition in concurrent scenarios.

---

## `ManageUPFMProductUserAsync` — 7-way parallel initial fetch

All independent initial data fetches run concurrently via `Task.WhenAll`:

```csharp
await Task.WhenAll(
    userLoginTask,       // IManageUserLoginAsync.GetUserLoginOnlyAsync(userPersona.RealPageId)
    editorLoginTask,     // IManageUserLoginAsync.GetUserLoginOnlyAsync(editorPersona.RealPageId)
    productIdsTask,      // IProductRepositoryAsync.GetProductIdsByCompanyAsync(partyId)
    productSettingsTask, // IProductInternalSettingRepositoryAsync.GetProductInternalSettingsAsync(_productId)
    upPlatSettingsTask,  // IProductInternalSettingRepositoryAsync.GetProductInternalSettingsAsync(UnifiedPlatform)
    existingRolesTask,   // IManageUserRoleRightAsync.GetAssignedRoleForPersonaAsync(...)
    userPropIdsTask      // IPropertyRepositoryAsync.ListUPFMPropertyInstanceIdByPersonaAsync(...)
);
```

Previously these were sequential sync calls scattered across the method body.

---

## `ProcessPropertyOperationsBatched` → single TVP batch call

### Original
```csharp
// Multiple individual repository calls per property:
foreach (var prop in unassignedProperties)
    repo.RemovePropertyInstanceMapping(personaId, productId, id);
foreach (var prop in assignedProperties)
    repo.InsertPropertyInstanceMapping(personaId, productId, id);
```

### New
```csharp
List<UPFMPropertyInstanceMapping> mappings = new(total);
foreach (var prop in unassignedProperties)
    if (long.TryParse(prop, out long id))
        mappings.Add(new UPFMPropertyInstanceMapping { PropertyInstanceID = id, IsDeleted = true });
foreach (var prop in assignedProperties)
    if (long.TryParse(prop, out long id))
        mappings.Add(new UPFMPropertyInstanceMapping { PropertyInstanceID = id, IsDeleted = false });

var result = await _propertyRepository.BulkInsertRemovePropertyInstanceMappingsAsync(
    userPersonaId, effectiveProductId, mappings, ct);
```

Single TVP-backed stored procedure call handles the entire batch. Scales to 2000+ properties without
N round trips to the database.

---

## `BuildAuditParameters` / `AssignedRoleandPropertyNameList` — O(n²) → O(1) HashSet lookups

### Original
```csharp
// Inner-loop `Any()` calls — O(n²)
foreach (var role in gbAllRoles)
{
    if (addedRoleList.Any(r => r == rid)) ...
    if (removedRoleList.Any(r => r == rid)) ...
}
```

### New
```csharp
var addedRoleSet   = new HashSet<long>(addedRoleList);
var removedRoleSet = new HashSet<long>(removedRoleList);
var addedPropSet   = new HashSet<string>(addedPropertyList, StringComparer.OrdinalIgnoreCase);
var removedPropSet = new HashSet<string>(removedPropertyList, StringComparer.OrdinalIgnoreCase);

foreach (var role in gbAllRoles)
{
    if (addedRoleSet.Contains(rid)) ...   // O(1)
    if (removedRoleSet.Contains(rid)) ... // O(1)
}
```

---

## `MergeSelRolesWithGreenbook` — O(n²) LINQ → O(1) HashSet lookup

### Original
```csharp
foreach (var role in roleList)
{
    if (allRoles.Any(a => a.ID == role.RoleID.ToString()))  // O(n)
    {
        ProductRole selrole = (from a in allRoles
                               where a.ID == role.RoleID.ToString()
                               select a).FirstOrDefault();  // second O(n) pass
        if (selrole != null) selrole.IsAssigned = true;
    }
}
```

### New
```csharp
var assignedIds = new HashSet<string>(assignedRoles.Select(r => r.RoleID.ToString()));
foreach (var role in allRoles)
    if (assignedIds.Contains(role.ID))  // O(1)
        role.IsAssigned = true;
```

Also applies default role when nothing is assigned (mirrors existing sync behaviour).

---

## `MergeUPFMBooksPropertiesWithProductPropertyAsync` — O(n²) → O(1) HashSet

### Original
```csharp
foreach (var upfm in blueBookPropertyList)
{
    var pp = ConvertToProductProperty(upfm);
    if (userPropertyIdList.Contains(upfm.PropertyInstanceId))  // List.Contains = O(n)
        pp.IsAssigned = true;
    ...
}
```

### New
```csharp
var userPropertySet = new HashSet<int>(userPropertyIdList);
foreach (var upfm in blueBookPropertyList)
{
    var pp = ConvertUPFMPropertyInstanceToProductProperty(upfm, false);
    if (userPropertySet.Contains(upfm.PropertyInstanceId))  // O(1)
        pp.IsAssigned = true;
    ...
}
```

---

## `GetEnterpriseUPFMProperties` — O(n) user-property filter → O(1) HashSet

### Original
```csharp
foreach (var cp in customerPropertyList)
{
    if (userPropertyIdList.Contains(cp.PropertyInstanceId))  // List.Contains = O(n)
        userPropertyList.Add(ConvertToProductProperty(cp, true));
}
```

### New
```csharp
var userPropertySet = new HashSet<int>(userPropertyIdList);
foreach (var cp in customerPropertyList)
{
    if (userPropertySet.Contains(cp.PropertyInstanceId))  // O(1)
        userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true));
}
```

---

## `GetProductPropertyInstancesAsync` — `DirectUDMTranslateProperty` branching

The sync `GetProductPropertyInstancesBasedOnUPFMProperties` contained the same branching logic.
The async version preserves the branch structure and adds `Dictionary<string, Attribute>` for the
`CustomerPropertyId` attachment step:

```csharp
var translatedDict = translated.Data.Attributes.ToDictionary(
    a => a.PropertyInstanceSourceId,
    a => a,
    StringComparer.OrdinalIgnoreCase);

foreach (var cp in customerPropertyList)
{
    if (translatedDict.TryGetValue(cp.InstanceId.ToString(), out var attr))
        cp.CustomerPropertyId = attr.TranslatedPropertyInstances[0].PropertyInstanceSourceId;
}
```

---

## Serilog `Log.Write` → `ILogger`

| Original | Async replacement |
|----------|------------------|
| `Serilog.Log.Write(LogEventLevel.Information, msg)` | `_logger.LogInformation(msg, ...)` |
| `Serilog.Log.Write(LogEventLevel.Error, ex, msg)`   | `_logger.LogError(ex, msg, ...)` |
| `Serilog.Log.Write(LogEventLevel.Debug, msg)`       | `_logger.LogDebug(msg, ...)` |
| `WriteToDiagnosticLog(msg)` (base helper)           | `_logger.LogDebug(msg, ...)` |
| `WriteToErrorLog(msg)` (base helper)                | `_logger.LogError(ex, msg, ...)` |

Structured logging with named placeholders; `Stopwatch` added to `ManageUPFMProductUserAsync` and
`ProcessPropertyOperationsBatchedAsync` for elapsed-time telemetry.

---

## Blocking `.Result` calls — all eliminated

All async equivalents for previously-sync or `.Result`-based calls are now `await`-based:

| Original | Async replacement |
|----------|------------------|
| `_productRepository.GetProductIdsByCompany(partyId)` | `await _productRepository.GetProductIdsByCompanyAsync(partyId, ct)` |
| `_productRepository.ListRolesForProductByParty(...)` | `await _productRepository.ListRolesForProductByPartyAsync(...)` |
| `_blueBook.GetCompanyMap(...)` | `await _blueBook.GetCompanyMapAsync(...)` |
| `_blueBook.GetUPFMPropertyInstances(orgId)` | `await _blueBook.GetUPFMPropertyInstancesAsync(orgId, ct)` |
| `_propertyRepository.ListUPFMPropertyInstanceIdByPersona(...)` | `await _propertyRepository.ListUPFMPropertyInstanceIdByPersonaAsync(...)` |
| `_userLogin.GetUserLoginOnly(realPageId)` | `await _userLogin.GetUserLoginOnlyAsync(realPageId, ct)` |
| `_userLogin.GetUserPersonaOrganization(loginName)` | `await _userLogin.GetUserPersonaOrganizationAsync(loginName, ct)` |

---

## `ConvertUPFMPropertyInstanceToProductProperty` — C# 9+ target-typed `new()`

### Original
```csharp
private ProductProperty ConvertUPFMPropertyInstanceToProductProperty(UPFMPropertyInstance upfm, bool isAssigned)
{
    return new ProductProperty
    {
        ID         = upfm.CustomerPropertyId.ToString(),
        Name       = upfm.Name,
        ...
    };
}
```

### New
```csharp
private static ProductProperty ConvertUPFMPropertyInstanceToProductProperty(
    UPFMPropertyInstance upfm, bool isAssigned) => new()
{
    ID         = upfm.CustomerPropertyId.ToString(),
    Name       = upfm.Name,
    ...
};
```

Expression body + target-typed `new()` removes redundant type name. Method made `static` — no instance capture.

---

## `MapGbObjectToProduct` — `foreach` copy → C# 12 spread

### Original
```csharp
result.RoleList = new List<string>();
foreach (var roleId in userAssignProductPropertyRole.RoleList)
    result.RoleList.Add(roleId);
```

### New
```csharp
result.RoleList = [.. source.RoleList];
```

---

## Super-user VendorMarketplace override — `LINQ` settings lookups → direct

The super-user block performs several product-settings lookups. The async version uses
`.FirstOrDefault(...)?.Value` and `.Any(...)` directly on the already-fetched `productSettings` list
(no additional async calls needed during this block) and eliminates `string.IsNullOrEmpty` guards
where `OrdinalIgnoreCase` string comparison already handles empty strings correctly.

---

## DI Registration pattern

Because the constructor takes a non-DI `int productId` parameter, a factory or named registration
is required:

```csharp
// Option A — Factory delegate per product
services.AddScoped<IManageUPFMProductsIntegrationAsync>(sp =>
    new ManageUPFMProductsIntegrationAsync(
        productId: (int)ProductEnum.VendorMarketplace,
        contextService:           sp.GetRequiredService<IProductContextServiceAsync>(),
        productRepository:        sp.GetRequiredService<IProductRepositoryAsync>(),
        internalSettingRepository: sp.GetRequiredService<IProductInternalSettingRepositoryAsync>(),
        blueBook:                 sp.GetRequiredService<IManageBlueBookAsync>(),
        propertyRepository:       sp.GetRequiredService<IPropertyRepositoryAsync>(),
        userRoleRight:            sp.GetRequiredService<IManageUserRoleRightAsync>(),
        userLogin:                sp.GetRequiredService<IManageUserLoginAsync>(),
        unifiedLoginRepository:   sp.GetRequiredService<IUnifiedLoginRepositoryAsync>(),
        logger:                   sp.GetRequiredService<ILogger<ManageUPFMProductsIntegrationAsync>>()));

// Option B — Factory interface  IManageUPFMProductsIntegrationFactory (recommended for multi-product)
// factory.Create(productId) returns the correctly-configured async instance.
```

---

## C# / .NET improvements

| Feature | Usage |
|---------|-------|
| `Task.WhenAll` (7 tasks) | Parallel initial fetch in `ManageUPFMProductUserAsync` |
| `Task.WhenAll` (2 tasks) | Parallel BlueBook + product detail fetch in `GetEnterpriseUPFMPropertiesAsync` |
| `HashSet<T>` | Role/property membership checks in merge, audit, and property filter helpers |
| `Dictionary<TKey, TValue>` | `translatedDict` for O(1) `CustomerPropertyId` attachment |
| `new HashSet<int>(list)` | Pre-built from fetched list, zero allocation per iteration |
| C# 12 collection expressions `[]` | `List<>` and array initialisers throughout |
| C# 12 spread `[.. source]` | `MapGbObjectToProduct` role-list copy |
| Target-typed `new()` | `ConvertUPFMPropertyInstanceToProductProperty` expression body |
| `static` pure method | `MapGbObjectToProduct`, `ConvertUPFMPropertyInstanceToProductProperty` |
| `when (ex is not OperationCanceledException)` | All catch guards |
| `is null or { Count: 0 }` pattern | Null/empty list guards |
| `is { Count: > 0 }` pattern | Collection presence checks |
| `ArgumentNullException.ThrowIfNull` | Constructor guards |
| `Stopwatch.StartNew()` | Elapsed-time telemetry in `ManageUPFMProductUserAsync` + batch ops |
| `string.Equals(…, OrdinalIgnoreCase)` | All case-insensitive setting name comparisons |
| `StringComparer.OrdinalIgnoreCase` | `HashSet` and `Dictionary` constructors |
| Removed mutable instance fields | `_productId` (mutation), `_upfmProductId`, `_udmSourceCode` |
| Removed dead `GetProductIdsByOrg()` | Calls discarded immediately in `GetRoles`/`GetRightsByRole` |

---

## Pending / Notes

- **`IManageUPFMProductsIntegrationFactory`**: Recommended factory pattern for multi-product DI registration.
  Each product (VendorMarketplace, UPFM, etc.) needs its own `productId`-scoped instance.
- **`DynamicContractResolver` (sync)**: Used in `GetEnterpriseUPFMPropertiesAsync` for JSON field filtering.
  This is a synchronous JSON round-trip and is intentionally kept as-is — no async alternative needed.
- **`GetUPFMMultiCompanyPropertiesAsync` sequential per-company calls**: Each company calls
  `GetProductCompanyInstanceIdAsync` + `GetEnterpriseUPFMPropertiesAsync` sequentially. A future
  improvement could parallelize these with `Task.WhenAll` across companies, but the current behaviour
  matches the original sync implementation.
- **`UnassignUserAsync` property partial-failure**: If bulk property removal fails, the method logs a
  warning and continues to set product status to `Deleted`. This matches the original forgiving behaviour.
