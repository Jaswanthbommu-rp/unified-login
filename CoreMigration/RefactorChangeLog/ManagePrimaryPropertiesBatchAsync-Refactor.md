# ManagePrimaryPropertiesBatchAsync Refactor Changelog

**Date:** 2026-04-21
**Branch:** `feature/UserRepositoryRefactor`
**Target:** .NET 10 / C# 13

---

## Summary

Full async refactor of `Logic/ManagePrimaryPropertiesBatch.cs` into `LogicAsync/`.
Original class retained as-is for legacy consumers. Two new files created.

---

## Files Created

| File | Notes |
|------|-------|
| `LogicAsync/Interfaces/IManagePrimaryPropertiesBatchAsync.cs` | New async interface |
| `LogicAsync/ManagePrimaryPropertiesBatchAsync.cs` | Full async implementation |
| `ManagePrimaryPropertiesBatchAsync-Refactor.md` | This file |

---

## Key Changes

### 1. `DefaultUserClaim` mutation eliminated

**Before:** The original class mutated `_userClaim` fields mid-method to provide context to
`ManageEnterpriseRolesPrimaryProperties`, which required the claim to be pre-populated before
construction:
```csharp
_userClaim.UserRealPageGuid          = editorPersona.RealPageId;
_userClaim.OrganizationRealPageGuid  = editorPersona.Organization.RealPageId;
_userClaim.Rights                    = _manageProductBatch.GetPersonaRoleRights(...);
_userClaim.OrganizationPartyId       = editorPersona.OrganizationPartyId;
_manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(_userClaim);
```
This is a shared-state mutation pattern — thread-unsafe under concurrent requests.

**After:** `IManageEnterpriseRolesPrimaryPropertiesAsync` resolves its own context internally via
injected `IUserClaimsAccessor` and `IPersonaRepositoryAsync`. The caller passes persona IDs
directly:
```csharp
await _enterpriseRoles.ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
    batch.EditorUserPersonaId, batch.SubjectUserPersonaId,
    batchProcessTypeId: batch.BatchProcessTypeId, cancellationToken: cancellationToken);
```

---

### 2. All inline `new Xxx(...)` instantiations replaced with DI

**Before:**
```csharp
var manageProduct    = new ManageProduct(_userClaim);
var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
var manageProductOneSite = new ManageProductOneSite(_userClaim);
_productRepository   = new ProductRepository();
_propertyRepository  = new PropertyRepository();
_integrationTypeFactory = new IntegrationTypeFactory(...);
_productInternalSettingRepository = new ProductInternalSettingRepository();
_unifiedSettingsRepository = new UnifiedSettingsRepository();
_productBulkUpdateRepository = new BatchProductBulkUpdateRepository(_userClaim);
_managePersona       = new ManagePersona(_userClaim);
_manageProductBatch  = new ManageProductBatch(_userClaim);
```
11 inline instantiations — untestable, hidden dependencies, socket/connection leaks.

**After:** 7 DI-injected interfaces — all testable, mockable, lifetime-managed by the container.

---

### 3. `IManagePersona` and `IManageProductBatch` removed entirely

**Before:** `GetPersona` and `GetPersonaRoleRights` were called solely to populate the
`DefaultUserClaim` fields that `ManageEnterpriseRolesPrimaryProperties` needed.

**After:** `ManageEnterpriseRolesPrimaryPropertiesAsync` resolves persona and rights internally.
Neither dependency is needed in `ManagePrimaryPropertiesBatchAsync`.

---

### 4. `IntegrationTypeFactory` removed

**Before:** `IntegrationTypeFactory` was constructed inline but never used inside
`ManagePrimaryPropertiesBatch` — it was passed as a constructor argument only to satisfy
`ManageEnterpriseRolesPrimaryProperties`, which wired it internally.

**After:** Dependency managed entirely inside `ManageEnterpriseRolesPrimaryPropertiesAsync` via
`IIntegrationTypeFactoryAsync`.

---

### 5. Serilog static `Log.Write` replaced with `ILogger<T>`

**Before:**
```csharp
Log.Write(LogEventLevel.Error, exception: ex,
    messageTemplate: "{ActionName} - {state}",
    propertyValue0: "GeneratePrimaryPropertiesUserProductBatch",
    propertyValue1: $"Error: {ex.Message}");
```

**After:**
```csharp
_logger.LogError(ex,
    "{ActionName} failed for batch {BatchId}",
    nameof(GeneratePrimaryPropertiesUserProductBatchAsync),
    batch.PrimaryPropertyBatchProcessId);
```
- Exception passed as first argument (correct structured-log pattern)
- `nameof(...)` avoids magic string
- `BatchId` added to log context for traceability
- `ex.Message` removed — the exception object already carries the message; duplicating it inflates the log entry

---

### 6. `UpdatePrimaryPropertyProductBatch` → `UpdatePrimaryPropertyProductBatchAsync`

**Before:** `_productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(...)` — blocking DB call
inside an otherwise async path.

**After:**
```csharp
await _batchRepo.UpdatePrimaryPropertyProductBatchAsync(
    batch.PrimaryPropertyBatchProcessId, statusTypeId, cancellationToken).ConfigureAwait(false);
```
`CancellationToken` threaded all the way through.

---

### 7. Parameterless constructor and second partial constructor removed

**Before:** Two extra constructors for testing and legacy creation:
```csharp
public ManagePrimaryPropertiesBatch() { /* 11 inline new() calls */ }
public ManagePrimaryPropertiesBatch(IProductRepository, IPropertyRepository) { ... }
```

**After:** Single DI constructor — the only legitimate entry point.

---

### 8. `GetPrimaryPropertySettingsForCompanyAndProductAsync` — three concurrent repo calls

**Before:** Three sequential blocking calls:
```csharp
var productGlobalSettingType = _productInternalSettingRepository.GetProductSettingByType("UsePrimaryProperties");
var companyProductSettings   = _productRepository.GetProductSettings(_userClaim.OrganizationRealPageGuid);
var settings                 = _unifiedSettingsRepository.GetUnifiedSettings(_userClaim.OrganizationPartyId, "Company");
```

**After:** All three fired concurrently with `Task.WhenAll` — latency is max(t1, t2, t3) instead of t1+t2+t3:
```csharp
var globalSettingsTask  = _productInternalSettingRepo.GetProductSettingByTypeAsync("UsePrimaryProperties", cancellationToken);
var companySettingsTask = _productRepo.GetProductSettingsAsync(orgRealPageId, cancellationToken);
var orgSettingsTask     = _unifiedSettingsRepo.GetUnifiedSettingsAsync(orgPartyId, "Company", cancellationToken);

await Task.WhenAll(globalSettingsTask, companySettingsTask, orgSettingsTask).ConfigureAwait(false);
```

---

### 9. `.ToLower() ==` replaced with `StringComparison.OrdinalIgnoreCase`

**Before:**
```csharp
p.Name.ToLower() == "useprimaryproperties"
```
Allocates a new string on every comparison.

**After:**
```csharp
p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
```
Zero allocation, culture-invariant, correct under all locales.

---

### 10. Null-check pattern modernised — `is null` instead of `!= null ? false : true`

**Before:**
```csharp
if (productUsePrimaryPropertiesGlobalStr != null && ...)
```

**After:** Early-return guard clauses using `is null`:
```csharp
if (globalEntry is null || !int.TryParse(...) || globalValue < 0)
    return false;
```
Eliminates nested branching; fail-fast pattern is easier to read.

---

### 11. `ArgumentNullException.ThrowIfNull` guard on public method

**Before:** No null-check on `batch` parameter — would throw `NullReferenceException` deep inside
the call with no actionable message.

**After:**
```csharp
ArgumentNullException.ThrowIfNull(batch);
```

---

### 12. Return value semantics clarified

**Before:** Returns `""` on success, `"Error"` from catch. The non-empty status message from
`ProcessEnterpriseRolesAndPrimaryPropertiesData` was discarded.

**After:** Returns `string.Empty` on full success, the actual `statusMessage` when enterprise-role
processing fails (non-empty but no exception), and `"Error"` only from the exception path — giving
callers actionable diagnostic information.

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| File-scoped namespace | Both new files |
| `sealed` class | `ManagePrimaryPropertiesBatchAsync` |
| `is null` pattern | Guard clauses in `GetPrimaryPropertySettingsForCompanyAndProductAsync` |
| `ArgumentNullException.ThrowIfNull` | Constructor guards + public method guard |
| `ConfigureAwait(false)` | All `await` sites |
| Named argument `batchProcessTypeId:` | `ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync` call |
| `nameof(...)` | Log message action name |

---

## DI Registration Template

```csharp
services.AddScoped<IManagePrimaryPropertiesBatchAsync, ManagePrimaryPropertiesBatchAsync>();

// Prerequisites (must already be registered):
// services.AddScoped<IManageEnterpriseRolesPrimaryPropertiesAsync, ManageEnterpriseRolesPrimaryPropertiesAsync>();
// services.AddScoped<IBatchProductBulkUpdateRepositoryAsync, BatchProductBulkUpdateRepositoryAsync>();
// services.AddScoped<IProductRepositoryAsync, ProductRepositoryAsync>();
// services.AddScoped<IProductInternalSettingRepositoryAsync, ProductInternalSettingRepositoryAsync>();
// services.AddScoped<IUnifiedSettingsRepositoryAsync, UnifiedSettingsRepositoryAsync>();
// services.AddScoped<IUserClaimsAccessor, UserClaimsAccessor>();
```

---

## Known Limitations

### `GetPrimaryPropertySettingsForCompanyAndProductAsync` is dead code

The private helper method was never called in the original `ManagePrimaryPropertiesBatch` either.
It is retained in the async refactor for completeness, but should be wired up or removed in a
follow-up pass.
