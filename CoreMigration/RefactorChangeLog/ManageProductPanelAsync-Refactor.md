# ManageProductPanelAsync Refactor Changelog

**Date:** 2026-04-13
**Branch:** feature/UserRepositoryRefactor

---

## Overview

This refactor transforms `ManageProductPanel.cs` (~693 lines, two constructors, `DefaultUserClaim`-bound, synchronous factory dispatch) into `ManageProductPanelAsync.cs` (~380 lines), a fully async, DI-first implementation of the product-panel orchestration service.

The panel service is an orchestrator: it routes each request to the correct `IIntegrationTypeFactory` integration or to a specialised async service (`IManageProductOneSiteAccountingAsync`, `IManageBlueBookAsync`, etc.). Because `IIntegrationTypeFactory` has no async counterpart, all factory calls are wrapped in `Task.Run` to offload blocking I/O from ASP.NET request threads.

---

## Constructor Expansion

### Legacy (two constructors)

```csharp
// Primary constructor — newed up services from DefaultUserClaim
public ManageProductPanel(DefaultUserClaim userClaims) : base(userClaims) { ... }

// Test/override constructor — accepted externally-created dependencies
public ManageProductPanel(IIntegrationTypeFactory, IManagePersona,
    IPersonaRepository, IManageBlueBook, IManageProductOneSiteAccounting,
    DefaultUserClaim) : base(userClaims) { ... }
```

Both constructors stored `DefaultUserClaim` as a field (`_userClaims`) and used it throughout methods (e.g. `_userClaims.OrganizationRealPageGuid`).

### Refactored (single DI constructor, 7 deps)

| Dependency | Replaces |
|---|---|
| `IIntegrationTypeFactory` | `GetIntegration(productId)` sync calls (retained, wrapped in `Task.Run`) |
| `IProductRepositoryAsync` | `_productRepository.GetAllProducts(...)` |
| `IPersonaRepositoryAsync` | `_personaRepository.GetPersonaProductSettings(...)` |
| `IManagePersonaAsync` | `_userClaims.OrganizationRealPageGuid` (resolved via editor persona) |
| `IManageBlueBookAsync` | `_manageBlueBook.TranslateProductPrimaryPropertiesData(...)` |
| `IManageProductOneSiteAccountingAsync` | `_oneSiteAccounting.GetUserPropertyGroups(...)` (FinancialSuite location groups) |
| `ILogger<ManageProductPanelAsync>` | `WriteToDiagnosticLog` / `WriteToErrorLog` |

All guards use `ArgumentNullException.ThrowIfNull` (.NET 6+).

---

## `DefaultUserClaim` Removal

`_userClaims.OrganizationRealPageGuid` was the only field-state access inside method bodies (used in `GetProductProperties` for the `UsePrimaryProperties` merge logic). It is replaced by:

```csharp
var editorPersona = await _managePersona.GetPersonaAsync(editorPersonaId, withRights: false, ct);
var orgRealPageGuid = editorPersona.Organization.RealPageId;
```

The editor persona fetch is parallelised with `GetPersonaProductSettingsAsync` via `Task.WhenAll` to avoid sequential latency.

---

## `IIntegrationTypeFactory` — `Task.Run` Wrapping

`IIntegrationTypeFactory.GetIntegration(productId)` is a synchronous factory with no async counterpart. All usages are wrapped in `Task.Run` to avoid blocking the ASP.NET thread pool:

```csharp
var result = await Task.Run(() =>
{
    var integration = _integrationTypeFactory.GetIntegration(productId);
    return integration.GetProperties(editorPersonaId, userPersonaId, datafilter);
}, ct);
```

The `CancellationToken` is passed to `Task.Run` so the offloaded task is cancellable before it starts.

---

## `Task.WhenAll` for Parallel Async Calls

`GetProductPropertiesAsync` fetches two independent data sources before the factory call:

```csharp
var personaSettingsTask = _personaRepository.GetPersonaProductSettingsAsync(userPersonaId, ct);
var editorPersonaTask   = _managePersona.GetPersonaAsync(editorPersonaId, withRights: false, ct);
await Task.WhenAll(personaSettingsTask, editorPersonaTask);

var personaSettings = await personaSettingsTask;
var orgRealPageGuid = (await editorPersonaTask).Organization.RealPageId;
```

These two async calls are fully independent (different IDs, different repositories) and are therefore fired concurrently.

---

## `switch` Expression in `GetProductLocationGroupsAsync`

The legacy `switch` statement with `case` branches is replaced by a `switch` expression:

```csharp
ListResponse result = productId switch
{
    (int)ProductEnum.FinancialSuite =>
        await _oneSiteAccounting.GetUserPropertyGroupsAsync(
            editorPersonaId, userPersonaId, datafilter, ct),
    (int)ProductEnum.UtilityManagement =>
        LogAndReturnEmpty(
            $"GetProductLocationGroupsAsync - UtilityManagement (RUM) not yet fully refactored " +
            $"(productId={productId})"),
    _ => new ListResponse()
};
```

---

## Static `ExtractAssignedRoles` Helper

The legacy `GetUserProductRoles` contained 5 sequential `if/foreach` blocks dispatching on the runtime type of `integration.GetRoles(...)` records:

```csharp
if (records[0] is ProductRole)       { foreach (...) ... }
else if (records[0] is ClickPayRole) { foreach (...) ... }
else if (records[0] is Level)        { foreach (...) ... }
// etc.
```

Replaced by a single static helper that uses LINQ per type branch:

```csharp
private static List<RoleTemplateRoles> ExtractAssignedRoles(List<object> records)
{
    var type = records[0].GetType();

    if (type == typeof(SharedObjects.Product.ProductRole))
        return records.Cast<SharedObjects.Product.ProductRole>()
            .Where(p => p.IsAssigned)
            .Select(r => new RoleTemplateRoles {
                RoleId = r.ID, RoleName = r.Name,
                RoleTemplateProductRoleMappingID = 0 })
            .ToList();

    if (type == typeof(SharedObjects.Product.ClickPay.ClickPayRole))
        return records.Cast<SharedObjects.Product.ClickPay.ClickPayRole>()
            .Where(p => p.IsAssigned)
            .Select(r => new RoleTemplateRoles {
                RoleId = r.ID.ToString(), RoleName = r.Name,
                RoleTemplateProductRoleMappingID = 0 })
            .ToList();

    // ProductIntegration.Model.ProductRole, Level/ILevel, Rum.Role cases ...

    return [];
}
```

---

## `additionalInfo` Indexer Assignment

The legacy `GetProductProperties` used `additionalInfo.Add(pair.Value, true)` inside a loop, which throws `ArgumentException` on duplicate keys. Replaced with the indexer:

```csharp
additionalInfo[pair.Value] = true;
```

---

## Pending / Deferred Paths

### UtilityManagement (RUM)

`IManageProductRumAsync` still requires a `DefaultUserClaim` parameter on `GetRolesAsync` and `GetProductRightsAsync`. Until `IManageProductRumAsync` is fully refactored:

- `GetProductRightsAsync` returns `Task.FromResult(new ListResponse())` with a debug-level log.
- The UtilityManagement branch in `GetProductLocationGroupsAsync` returns `LogAndReturnEmpty(...)`.
- The UtilityManagement branch in `GetProductRolesAsync` (via `IIntegrationTypeFactory`) is handled by `Task.Run` wrapping but the factory itself delegates to the still-sync RUM layer.

These paths are marked `// TODO: replace once IManageProductRumAsync removes DefaultUserClaim` in the implementation.

---

## .NET 10 / C# 12 Improvements

### Collection Expressions `[]`

All empty-collection initialisers replaced with C# 12 collection expressions:

| Before | After |
|---|---|
| `var failedProducts = new List<string>()` | `List<string> failedProducts = []` |
| `var roles = new List<RoleTemplateRoles>()` | `List<RoleTemplateRoles> roles = []` |
| `List<string> translatedInstances = new List<string>()` | `List<string> translatedInstances = []` |
| `return new List<RoleTemplateRoles>()` (fallback) | `return []` |

### `is not null` Pattern Matching

`!= null` checks replaced with `is not null` throughout:

```csharp
// Before
if (personaSettings != null && ...) { ... }

// After
if (personaSettings is not null && ...) { ... }
```

### `is not { Count: > 0 }` Combined Null-and-Empty Check

```csharp
// Before
if (productResult == null || productResult.Records == null || productResult.Records.Count == 0)
    return productResult;

// After
if (productResult?.Records is not { Count: > 0 })
    return Task.FromResult(productResult!);
```

### `StringComparison.OrdinalIgnoreCase`

All string equality comparisons use `StringComparison.OrdinalIgnoreCase` or `.Equals(..., StringComparison.OrdinalIgnoreCase)`.

### `when (ex is not OperationCanceledException)` Catch Guards

Exception handlers use the `when` filter to let cancellation propagate naturally:

```csharp
catch (Exception ex) when (ex is not OperationCanceledException)
{
    _logger.LogError(ex, "...");
    return new ListResponse { Error = ex.Message };
}
```

---

## Removed / Not Ported

- **`ManageProductBase` base class**: Eliminated. No inheritance. All previously-inherited helpers resolved via injected services or private instance methods.
- **`DefaultUserClaim` constructor parameter**: Removed from all method signatures. Context resolved per-call via `IManagePersonaAsync` and `IPersonaRepositoryAsync`.
- **Legacy test constructor**: Eliminated. DI constructor supports test doubles via standard mocking.
- **`WriteToDiagnosticLog` / `WriteToErrorLog`**: Replaced by `ILogger<ManageProductPanelAsync>` structured logging.
- **Mutable `_userClaims` field**: Removed. No mutable instance state remains.

---

## Files Changed

- `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductPanelAsync.cs` — new file; 14 async method signatures grouped under Properties, Roles, Rights, Organizations, Property translation, Persona product properties; all methods accept `CancellationToken ct = default`
- `UnifiedLogin.BusinessLogic/LogicAsync/ManageProductPanelAsync.cs` — new file, ~380 lines; single 7-dep DI constructor; 14 public methods; static `ExtractAssignedRoles` helper; `LogAndReturnEmpty` instance helper; `Task.Run` wrapping for all `IIntegrationTypeFactory` calls; `Task.WhenAll` parallelism in `GetProductPropertiesAsync`
