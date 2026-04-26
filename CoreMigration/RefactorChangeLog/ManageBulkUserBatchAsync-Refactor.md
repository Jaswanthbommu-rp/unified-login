# ManageBulkUserBatchAsync Refactor Changelog

**Date:** 2026-04-18  
**Branch:** `feature/UserRepositoryRefactor`  
**Target:** .NET 10 / C# 13  

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/ManageBulkUserBatch.cs` and its
primary dependency `Logic/ManageBulkUsers.cs` into `LogicAsync/`.

Removes every instance of:
- `new Xxx(_userClaim)` inline service / repository instantiation
- `DefaultUserClaim` constructor parameter on business logic classes
- `BaseRepository.GetRepository()` sync data-access pattern
- `RPObjectCache` static caching for product settings
- `BatchHelper.CreateAoBatchRecords(DefaultUserClaim, …)` static call with inline dependencies

Replaces with:
- 9-dependency constructor injection on `ManageBulkUsersAsync` (4 on `ManageBulkUserBatchAsync`)
- `IDbConnectionFactory` + Dapper multi-map for `GetUserBatchRecordAsync`
- `IProductInternalSettingRepositoryAsync` direct async call replacing `RPObjectCache`
- `IAoBatchServiceAsync.CreateAoBatchRecordsAsync` (injectable, no `DefaultUserClaim`)
- `BulkUserBatch.ImpersonatorUserId` (`long?`) explicit parameter — no `IUserClaimsAccessor` needed
- Fully async call chain with `CancellationToken` propagation

---

## Files Created

### Interfaces

| File | Replaces |
|------|----------|
| `LogicAsync/Interfaces/IManageBulkUsersAsync.cs` | `ManageBulkUsers` (no interface existed) |
| `LogicAsync/Interfaces/IManageBulkUserBatchAsync.cs` | `ManageBulkUserBatch` (no interface existed) |

### Implementations

| File | Replaces |
|------|----------|
| `LogicAsync/ManageBulkUsersAsync.cs` | `Logic/ManageBulkUsers.cs` |
| `LogicAsync/ManageBulkUserBatchAsync.cs` | `Logic/ManageBulkUserBatch.cs` |

---

## Dependency Mapping

### `ManageBulkUsersAsync` — 9 DI dependencies

| Dependency | Replaces |
|------------|---------|
| `IManagePersonaAsync` | `new ManagePersona(_userClaim)` |
| `IProductRepositoryAsync` | `new ProductRepository()` |
| `IUserLoginRepositoryAsync` | `new UserLoginRepository()` |
| `IManagePartyRelationshipAsync` | `new ManagePartyRelationship()` |
| `IBatchProductBulkUpdateRepositoryAsync` | `new BatchProductBulkUpdateRepository(_userClaim)` |
| `IProductInternalSettingRepositoryAsync` | `new ProductInternalSettingRepository()` + `RPObjectCache` |
| `IAoBatchServiceAsync` | `BatchHelper.CreateAoBatchRecords(DefaultUserClaim, …)` (static) |
| `IDbConnectionFactory` | `BaseRepository.GetRepository()` for multi-map query |
| `ILogger<ManageBulkUsersAsync>` | `Serilog.Log.Write(LogEventLevel.Debug/Error, …)` |

> `IUserClaimsAccessor` was removed — see **Design Decision #4** below.

Dependencies NOT injected in the sync constructor that are no longer needed:
- `IntegrationTypeFactory` — not used inside `ProcessProductUnAssignBatchData`
- `new ManageProduct / ManageUnifiedLogin / ManageProductOneSite` — only needed to construct `IntegrationTypeFactory`
- `new PersonaRepository` (duplicated twice in original)
- `IUserRoleRightRepository` — not used in `ProcessProductUnAssignBatchData`
- `IManageBlueBook` — not used in `ProcessProductUnAssignBatchData`

### `ManageBulkUserBatchAsync` — 4 DI dependencies

| Dependency | Replaces |
|------------|---------|
| `IManagePersonaAsync` | `new ManagePersona(_userClaim)` |
| `IManageBulkUsersAsync` | `new ManageBulkUsers(_repository, _userClaim)` (inline) |
| `IBatchProductBulkUpdateRepositoryAsync` | `new BatchProductBulkUpdateRepository(_userClaim)` |
| `ILogger<ManageBulkUserBatchAsync>` | `Serilog.Log.Write(…)` |

> `IUserClaimsAccessor` was removed — see **Design Decision #4** below.

---

## Key Design Decisions

### 1. `GetUserBatchRecord` → `GetUserBatchRecordAsync` with `IDbConnectionFactory`

The sync version used `BaseRepository.GetRepository().GetManyWithSpliOn<TFirst, TSecond, TReturn>`:

```csharp
// Sync (before)
using (var repository = GetRepository())
{
    IList<BulkUserBatch> items = repository.GetManyWithSpliOn<BulkUserBatch, BulkUserProduct, BulkUserBatch>(
        SP_GetBulkUserBatchRecords,
        (bulkuser, bulkUserProduct) => { ... },
        new { BulkUserBatchProcessId = bulkUserBatchProcessId },
        splitOn: "BulkUserBatchProcessId,ProductId");
}
```

Replaced with Dapper's `CommandDefinition`-based multi-map overload:

```csharp
// Async (after)
await using var connection = _dbFactory.CreateConnection();
await connection.QueryAsync<BulkUserBatch, BulkUserProduct, BulkUserBatch>(
    new CommandDefinition(SP_GetBulkUserBatchRecords,
        new { BulkUserBatchProcessId = bulkUserBatchProcessId },
        commandType: CommandType.StoredProcedure,
        cancellationToken: cancellationToken),
    (bulkUser, product) =>
    {
        bulkUsers.TryAdd(product.BulkUserBatchProcessId, bulkUser);
        bulkUsers[product.BulkUserBatchProcessId].BulkUserProducts.Add(product);
        return bulkUsers[product.BulkUserBatchProcessId];
    },
    splitOn: "BulkUserBatchProcessId,ProductId");
```

`Dictionary.TryAdd` replaces the `ContainsKey` + `Add` two-step (C# 6 — available throughout).

### 2. `RPObjectCache` → `IProductInternalSettingRepositoryAsync`

The sync `GetProductsWithNoProperties` cached settings for 120 seconds using the static `RPObjectCache`:

```csharp
// Sync (before)
var rpcache = new RPObjectCache();
var cacheKey = $"productInternalSetting_{productId}";
var productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () =>
    _productInternalSettingRepository.GetProductInternalSettings(productId).ToList());
```

Replaced with a direct async read. Since `ProcessProductUnAssignBatchDataAsync` is a one-shot batch operation
(not a hot request path), the 120-second cache provides no throughput benefit:

```csharp
// Async (after)
var settings = await _productInternalSettingRepository
    .GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken)
    .ConfigureAwait(false);
```

### 3. `BatchHelper.CreateAoBatchRecords` → `IAoBatchServiceAsync`

The sync call passed `DefaultUserClaim` into a static helper:

```csharp
// Sync (before)
BatchHelper.CreateAoBatchRecords(_userClaim, editorUserPersonaId, subjectUserPersonaId,
    isExternalUser, true, null, productId, null, productListToCreate, true);
```

Replaced with the injectable `IAoBatchServiceAsync` (created in the `Helper` refactor):

```csharp
// Async (after)
await _aoBatchService.CreateAoBatchRecordsAsync(
    editorUserPersonaId, subjectUserPersonaId, isExternalUser,
    usePrimaryProperties: true, propertiesResponse: new ListResponse(),
    productId, productRoles: null, productBatchList: productListToCreate,
    isDeleted: true, cancellationToken);
```

### 4. `ImpersonatorUserId` — explicit parameter replaces `IUserClaimsAccessor` + `GetUserLoginOnly` lookup

**Root cause of the gap:** The batch processor (`BulkUserUpdateJob`) is a separate Windows-service-style background process that calls the LandingAPI over HTTP using a service-account token — not the original user's JWT. When the API handler resolves `IUserClaimsAccessor`, it sees the service account's claims, not the claims of the user who initiated the batch. `ImpersonatedBy` is `Guid.Empty` in both the legacy and the original async versions in this context.

**Legacy behaviour (broken in batch context):**
```csharp
// ManageBulkUsers.ProcessProductUnAssignBatchData
if (_userClaim.ImpersonatedBy != Guid.Empty)   // always false in batch context
    impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
// impersonatorUserId = 0 every time; audit trail is always empty in batch context
```

**Fix — three-part change:**

1. `BulkUserBatch.ImpersonatorUserId` (`long?`) added to `UnifiedLogin.SharedObjects/Batch/PrimaryPropertyBatch.cs`.
   Maps directly to the existing DB column `ImpersonatorUserId bigint null`.
   The batch-creation endpoint must populate it at creation time (HTTP context):
   ```csharp
   // In the service that creates the batch record:
   batch.ImpersonatorUserId = _userClaims.ImpersonatorUserId; // long? from claims
   ```

2. `IUserClaimsAccessor` removed from both `ManageBulkUsersAsync` (9 deps, was 10) and
   `ManageBulkUserBatchAsync` (4 deps, was 5 — it was injected but **never called**).

3. `ProcessProductUnAssignBatchDataAsync` gains an explicit `long? impersonatorUserId` parameter.
   `ManageBulkUserBatchAsync` forwards `batch.ImpersonatorUserId` directly — no secondary
   `GetUserLoginOnlyAsync` lookup needed because the DB already stores the `UserId`:
   ```csharp
   await _manageBulkUsers.ProcessProductUnAssignBatchDataAsync(
       batch.EditorUserPersonaId,
       batch.SubjectUserPersonaId,
       batch.BulkUserBatchProcessId,
       batch.ImpersonatorUserId,      // ← long? (bigint null), survives service boundary
       cancellationToken);
   ```
   At the `SaveProductBatchAsync` call site (which takes `long`), the value is unwrapped:
   ```csharp
   impersonatorUserId ?? 0L
   ```
   This matches the app-wide convention — `0` means "no impersonation" (same as every other
   caller: `UserServiceAsync`, `EntUserRepositoryAsync`, `HOTSCloneUserRepositoryAsync`).

**No schema change required** — `ImpersonatorUserId bigint null` already exists on the table.
`NULL` rows (existing records) unwrap to `0L`, giving the same no-op behaviour as the legacy code.

### 5. `GetPersonaRoleRights` intentionally omitted

`ManageBulkUserBatch.GenerateProductUnAssignProductBatch` called:

```csharp
_userClaim.Rights = _manageProductBatch.GetPersonaRoleRights(
    batch.EditorUserPersonaId, editorPersona.OrganizationPartyId);
```

This mutated the shared `DefaultUserClaim` object before passing it to `new ManageBulkUsers(_, _userClaim)`.
In the async design, `ProcessProductUnAssignBatchDataAsync` does not use `Rights` in its logic — the
mutation was only needed because downstream sync code read rights back out of the shared claim.
The omission is safe for the current operation. If rights must be re-populated in a background
context in the future, inject `IManageProductBatchAsync` (pending async port) and call
`GetPersonaRoleRightsAsync`.

### 6. Duplicate constructor dependencies removed

`ManageBulkUsers(IRepository repository, DefaultUserClaim userClaim)` contained:
```csharp
_personaRepository = new PersonaRepository(_userClaim); // ← duplicated twice
```
This was a bug in the original. The async constructor has no duplicates.

### 7. `ManageBulkUserBatchAsync` — best-effort status update on exception

The sync `GenerateProductUnAssignProductBatch` called `UpdateBulkUserProductBatch` in the
`catch` block (synchronous, could throw and mask the original exception). The async version
wraps the status update in its own `try/catch` and logs a warning on update failure without
re-throwing, preserving the original exception as the primary error signal.

---

## `ProcessProductUnAssignBatchDataAsync` — Step-by-Step

| Step | Async call | Sync replacement |
|------|-----------|-----------------|
| 1 | `_managePersona.GetPersonaAsync(editorPersonaId)` | `_managePersona.GetPersona(editorPersonaId)` |
| 2 | `_managePersona.GetPersonaAsync(subjectPersonaId)` | `_managePersona.GetPersona(subjectPersonaId)` |
| 3 | `_userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(realPageId, null)` | `_userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null)` |
| 4 | `_managePartyRelationship.GetPartyRelationshipAsync(…)` | `_managePartyRelationship.GetPartyRelationship(…)` |
| 5 | `_productRepository.ListProductsByPersonaIdAsync(personaId, status)` | `_productRepository.ListProductsByPersonaId(personaId, status)` |
| 6 | `GetUserBatchRecordAsync(bulkUserBatchProcessId)` | `GetUserBatchRecord(bulkUserBatchProcessId)` via `BaseRepository` |
| 7 | `_productRepository.GetProductSamlDetailsAsync(personaId, productId)` | `_productRepository.GetProductSamlDetails(personaId, productId)` |
| 8 | *(eliminated)* `batch.ImpersonatorUserId` read directly from model | `_userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy)` — sync lookup by GUID |
| 9 | `GetProductsWithNoPropertiesAsync()` | `GetProductsWithNoProperties()` via `RPObjectCache` |
| 10 | `_aoBatchService.CreateAoBatchRecordsAsync(…)` | `BatchHelper.CreateAoBatchRecords(_userClaim, …)` |
| 11 | `_batchRepository.SaveProductBatchAsync(…)` | `_enterpriseRoleProductRepository.SaveProductBatch(…)` |

---

## Remaining SYNC Annotations

| Location | Blocked on |
|----------|-----------|
| `ManageBulkUserBatchAsync` — `GetPersonaRoleRights` omitted | `IManageProductBatchAsync.GetPersonaRoleRightsAsync` (no async port yet) |
| `AoBatchServiceAsync.CreateAoBatchRecordsAsync` — `CopyRegularUser` | `IManageProductAssetOptimizationAsync.CopyRegularUserAsync` (from Helper refactor) |

---

## DI Registration Template

```csharp
// Repository (already registered from BatchProductBulkUpdateRepository refactor)
services.AddScoped<IBatchProductBulkUpdateRepositoryAsync, BatchProductBulkUpdateRepositoryAsync>();

// Logic
services.AddScoped<IManageBulkUsersAsync, ManageBulkUsersAsync>();
services.AddScoped<IManageBulkUserBatchAsync, ManageBulkUserBatchAsync>();
```

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| Collection expressions `[]` | Empty `PropertyList`, `RoleList`, `PropertyGroupList` initializers; `[..bulkUsers.Values]` spread |
| `ArgumentNullException.ThrowIfNull` | `ManageBulkUserBatchAsync` constructor guard (batch arg); `?? throw new ArgumentNullException` for DI deps in both classes |
| `ConfigureAwait(false)` | All `await` sites |
| File-scoped namespaces | All new files |
| Nullable reference types (`string?`, `PartyRelationship?`) | Method parameters and return types |
| Nullable value type (`long?`) | `BulkUserBatch.ImpersonatorUserId`; `ProcessProductUnAssignBatchDataAsync` parameter; unwrapped with `?? 0L` at call site |
| `is not null` / `?.` null-conditional | Guard clauses in `ProcessProductUnAssignBatchDataAsync` |
| `is { Count: 0 }` property pattern | `productAttributes is { Count: 0 }` guard |
| `string.Empty` over `""` | All empty-string literals |
| `StringSplitOptions.RemoveEmptyEntries` | `GetProductsWithNoPropertiesAsync` CSV split |
| `Dictionary.TryAdd` | Multi-map callback in `GetUserBatchRecordAsync` |
| `?.` propagation on `partyRelationship` | `partyRelationship?.RoleTypeFrom?.Name` null-safe chain |
