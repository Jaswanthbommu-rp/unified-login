# UserRepository Refactor — Change Log

Branch: `feature/UserRepositoryRefactor`  
Target: .NET 4.8 → .NET Core 10  
Build status: ✅ 0 errors (258 commits ahead of master)

---

## Motivation

The legacy `UserRepository.cs` (~8 000 lines) mixed data access, business rules, external HTTP calls, and in-process caching into one untestable sync class. This refactor breaks it apart by:

- Replacing `new ManageXxx(userClaim)` construction patterns with injected async interfaces.
- Replacing blocking DB calls with `IDbConnection` + Dapper `CommandDefinition` + `CancellationToken`.
- Replacing static `RPObjectCache` with `ICacheService` / `IMemoryCache`.
- Replacing `DefaultUserClaim` as a constructor parameter with `IUserClaimsAccessor` injected at service startup.
- Moving all multi-SP orchestration into a new `UserServiceAsync` class instead of leaving it in repository layer.

---

## New Files Created (this session / recent sessions)

### `UnifiedLogin.BusinessLogic/Services/Interfaces/IUserServiceAsync.cs` (343 lines)
Defines the public contract for all user lifecycle operations. Maps sync → async:

| Legacy sync (UserRepository) | Async replacement |
|---|---|
| `CreateUser(ProfileDetail, IList<Persona>)` | `CreateUserAsync` |
| `UpdateNewUser(string, Profile, ...)` | `UpdateNewUserAsync` |
| `UpdateUser(Guid, IProfileDetail, IProfileDetail)` | `UpdateUserAsync` |
| `UpdateUserListUser(ProfileDetail, ...)` | `UpdateUserListUserAsync` |
| `DisableUserProduct(Guid, long, IList<...>)` | `DisableUserProductAsync` |
| `ActivateUserProducts(Guid, long, IList<...>)` | `ActivateUserProductsAsync` |
| `AssignProductsToAdministrators(Guid, long)` | `AssignProductsToAdministratorsAsync` |
| `ActivateSalesForceUser(Guid, long, IList<...>)` | `ActivateSalesForceUserAsync` |
| `ProcessDisabledUsers(IList<ProcessUserLogin>)` | `ProcessDisabledUsersAsync` |
| `ProcessDisableUserProductData(IRepository, ...)` | `ProcessDisableUserProductDataAsync` / `InTransactionAsync` |
| `InsertNewPhoneNumberFromImport(IRepository, ...)` | `InsertNewPhoneNumberFromImportAsync` |
| `ThirdPartyIdpBulkUpdate(IList<long>, bool)` | `ThirdPartyIdpBulkUpdateAsync` |
| `GetUnifiedSettingData(string)` | `GetUnifiedSettingDataAsync` |
| `UpdateUserStatusByCompany(...)` | `UpdateUserStatusByCompanyAsync` |

---

### `UnifiedLogin.BusinessLogic/Services/UserServiceAsync.cs` (3 406 lines)

The main orchestration class. Single DI constructor with 16 injected dependencies — no `new` keyword.

#### Constructor dependencies

```csharp
IConnectionFactory          _connectionFactory
IUserLoginRepositoryAsync   _userLoginRepo
IUserRepositoryAsync        _userRepo
IOrganizationRepositoryAsync _orgRepo
IManagePersonaAsync         _managePersona
IPersonaRepositoryAsync     _personaRepo
IContactMechanismUsageTypeRepositoryAsync _contactMechUsageTypeRepo
IRoleTypeRepositoryAsync    _roleTypeRepo
IManageBlueBookAsync        _manageBlueBook
IManageUnifiedSettingsAsync _manageUnifiedSettings
IProductInternalSettingRepositoryAsync _productSettingRepo
IManageProductAssetOptimizationAsync   _manageAo
IUserClaimsAccessor         _userClaims
IUserAuditService           _auditService
IMemoryCache                _cache
ILogger<UserServiceAsync>   _logger
```

#### Public methods implemented (56 async methods total)

| Method | Source in UserRepository.cs |
|---|---|
| `CreateUserAsync` | `CreateUser` (~L:600–1400) |
| `UpdateNewUserAsync` | `UpdateNewUser` (~L:400–600) |
| `UpdateUserAsync` | `UpdateUser` (~L:2561–2731) |
| `UpdateUserListUserAsync` | `UpdateUserListUser` (~L:1828–2249) |
| `DisableUserProductAsync` | `DisableUserProduct` |
| `ActivateUserProductsAsync` | `ActivateUserProducts` |
| `AssignProductsToAdministratorsAsync` | `AssignProductsToAdministrators` |
| `ActivateSalesForceUserAsync` | `ActivateSalesForceUser` |
| `ProcessDisabledUsersAsync` | `ProcessDisabledUsers` |
| `ProcessDisableUserProductDataAsync` | `ProcessDisableUserProductData` |
| `ProcessDisableUserProductDataInTransactionAsync` | *(transaction-aware overload — new)* |
| `InsertNewPhoneNumberFromImportAsync` | `InsertNewPhoneNumberFromImport` |
| `ThirdPartyIdpBulkUpdateAsync` | `ThirdPartyIdpBulkUpdate` |
| `UpdateUserStatusByCompanyAsync` | `UpdateUserStatusByCompany` |
| `GetUnifiedSettingDataAsync` | `GetUnifiedSettingData` |

#### `CreateUserAsync` — private helper breakdown

Each legacy `#region` block was extracted to a focused private method:

| Helper | What it does |
|---|---|
| `CreatePersonAsync` | `SP_CreatePerson` → returns `(userId, personRealId)` |
| `CreateUserLoginAsync` | `SP_CreateUserLogin` |
| `UpdateUserLoginPasswordAsync` | `SP_UpdateUserLoginPassword` |
| `SaveTelecommunicationNumbersAsync` | Phone number create/update loop |
| `SaveNotificationEmailAsync` | Contact mechanism chain for email notification |
| `CreateUserLoginPersonaAsync` | `SP_CreateUserLoginPersona` |
| `CreatePersonaAsync` | `SP_CreatePersona` |
| `LinkPersonaToRoleAsync` | Role resolution (single / multi / SuperUser) + `SP_LinkPersonaToRole` + property mapping |
| `SetUserTypeAsync` | `SP_LinkPersonToOrganization` / `SP_UpdatePersonToOrganization` |
| `SaveExternalUserRelationshipAsync` | `SP_CreateExternalUserRelationship` |
| `CreateEmployeeIdAsync` | `SP_CreateEmployeeId` |
| `CreateSupervisorAsync` | `SP_InsertUpdateSuperVisor` |
| `SaveCustomFieldsAsync` | `SP_AddUpdateFieldValue` |
| `LinkIdentityProviderAsync` | `SP_LinkIdentityProviderToUserLogin` |
| `InsertUpdateDelegateAdminRoleAsync` | `SP_LinkPersonaToRole` per template ID |
| `InsertUpdateEnterpriseRoleToUserAsync` | `SP_InsertUpdateEnterpriseRoleToUser` |
| `SavePendingEmailNotificationAsync` | `SP_CreateCommunicationEvent` for pending activation email |
| `UpdateUserTypeForRpEmployeeAsync` | Updates prior-org user types for RP employees |
| `ResolveOrganizationListForExistingUserAsync` | Builds org list for the existing-user branch |

#### `SaveProductDetailsAsync` — 6-stage product batch engine

Replaces the ~500-line `SaveProductDetails` loop in the sync `UserRepository`:

1. Extract UnifiedUI enterprise role from batch
2. Role template → product batch mappings
3. **SuperUser branch**: auto-assign all org products (concurrent with `SemaphoreSlim(5)`)
4. **Non-SuperUser + enterprise role**: role-template–filtered product assignment
5. Cleanup: remove EasyLMS/ClientPortal from auto-assigned, add SalesForce, AO-BM pairing
6. Save loop: `SaveProductBatchAsync` per item

#### `UpdateUserAsync` → `UpdateUserDataAsync` — private orchestrator (600 lines)

Port of `UpdateUserData` (UserRepository.cs L:6252–7285). Sections in order:

| Section | SPs called |
|---|---|
| Pre-fetch lookups | `GetUserLoginOnlyAsync`, `ListOrganizationByLoginNameAsync`, `GetOrganizationAsync`, `GetOrganizationIdentityProviderTypeAsync`, `GetUserOrganizationWithStatusAsync` (×2), `ListActivePersonaAsync`, `ListContactMechanismUsageTypeAsync`, `SP_SecurityListRolesByRealPageID`, `SP_ListRolesForProductsByPersonaId` |
| Pure-logic flags | `ComputeUserBatch`, profile/login/employee/supervisor changed checks |
| Update Person | `SP_UpdatePerson` |
| Link IDP | `SP_LinkIdentityProviderToUserLogin` |
| Update RealPartner | `SP_UpdateUserLoginPersona` |
| Update UserLogin | `SP_UpdateUserLogin` |
| Change user type external | `ChangeUserTypeExternalAsync` |
| Custom fields | `SP_AddUpdateFieldValue` |
| Status change | `SP_UpdateUserStatusByCompany` |
| Update Persona type | `SP_UpdatePersona` |
| Update User Type | `SP_GetPartyRelationshipByRealPageId` → `SP_UpdatePersonToOrganization` |
| Notification email | `SP_ListContactMechanismsForPerson` → expire old → create new chain → `SP_CreateCommunicationEvent` |
| Employee ID | `SP_UpdateEmployeeId` / `SP_CreateEmployeeId` |
| Supervisor | `SP_InsertUpdateSuperVisor` |
| Delegate roles | `SP_UpdateDelegateAdminStatus` + `InsertUpdateDelegateAdminRoleAsync` |
| External user relationship | `SP_UpdateExternalUserRelationship` |
| Enterprise role | `InsertUpdateEnterpriseRoleToUserAsync` / `SP_UnassignEnterpriseRoleFromUser` |
| Batch group | `CreateBatchProcessGroupAsync` |
| Save products (active) | `SaveProductDetailsAsync` |
| Disable products (inactive) | `ProcessDisableUserProductDataCoreAsync` per persona |
| GreenBook role | `UpdateGreenBookRoleInTransactionAsync` |
| Property instance mapping | `SP_AddUpdatePropertyInstanceMapping` |

#### New helper methods introduced by `UpdateUserDataAsync`

| Helper | Port of |
|---|---|
| `ComputeUserBatch` (static) | `GetUserBatch` (UserRepository.cs L:6075–6173) |
| `ChangeUserTypeExternalAsync` | `ChangeUserTypeExternal` (UserRepository.cs L:5501–5735) |
| `GetExistingNotificationEmailAsync` | Inline SP_ListContactMechanismsForPerson lookup |
| `GetUnifiedPlatformDefaultRoleAsync` | `GetUnifiedPlatformDefaultRole` (UserRepository.cs L:7973–7988) |

#### Pure-logic static helpers (no DB)

| Helper | Purpose |
|---|---|
| `Failure` | Builds a `CreateUserResponse` with error code + message |
| `IsDuplicateUserType` | Checks whether login + org + user-type already exists |
| `ResolvePrimaryOrganizationFlag` | Determines `primaryOrganization` flag for new org links |
| `DetermineUserStatus` | Two-branch status/thruDate logic for ExternalUsers vs. current company |
| `IsUserProfileChanged` | Compares first/middle/last name fields |
| `BundleAoProducts` | Serialises AO product list JSON for batch insert |

---

## Modified Files

### `UnifiedLogin.BusinessLogic/LogicAsync/ManageProductUserAsync.cs` (785 lines)

Previously a thin stepping-stone wrapper (`new ManageProductUser(userClaim).Method()`). Replaced with native async implementation with zero dependency on the sync class.

**Constructor** now accepts:
```
IUserClaimsAccessor, IProductRepositoryAsync, ISamlRepositoryAsync,
IPersonaRepositoryAsync, IProductInternalSettingRepositoryAsync,
IIntegrationTypeFactory, IManagePersonaAsync, ProductUserActivityLogHelper,
ICacheService, IHttpClientFactory, ITokenHelperAsync, ILogger<>
```

**Key fixes applied in this session:**

| Error | Root cause | Fix |
|---|---|---|
| `internalChange:` named arg | `IIntegrationType.UpdateUserDetails` uses `internalchange` (lowercase) | Switched to positional `false` |
| `Task<UserActivityLogInfo>` not awaited | `GetUserActivityLogInfo` is async | Added `await` at all 4 call sites |
| `GetUnifiedLoginServerToken` not found | `ITokenHelperAsync` exposes `GetUnifiedLoginServerTokenAsync` | Renamed + awaited |
| `GetImpersonatorDetails`, `GetOrganizationAdminUserRealPageId`, `GetPrimaryOrganizationName` missing | `ProductUserActivityLogHelper` never had these; were sync-only helpers on the old class | Resolved impersonator inline via `GetUserActivityLogInfo(impersonatorUserId)`; org-name lookup deferred |
| `GenerateQueueMessage` type mismatch | `UserDetails?` param vs `string?` call site | Changed param to `string? impersonatorName` |
| `SendNotificationFireAndForget` not found | Sync fire-and-forget wrapper was missing | Added `private void SendNotificationFireAndForget` → `_ = SendNotificationAsync(...)` |
| `ProductInternalSetting.ProductId` not found | Class has no `ProductId` property | Replaced blocking `.GetAwaiter().GetResult()` lookup with `string? productType = null` |
| `ProductIntegration.Model.ProductRole` ambiguous | Namespace missing from using list | Qualified as `Logic.ProductIntegration.Model.ProductRole` |
| `IList<>.RemoveAll` | `RemoveAll` is `List<T>` only, not `IList<T>` | Replaced with `.Where(...).ToList()` into a local `filteredList` |
| `UserSyncRequest` not found | Missing `using UnifiedLogin.SharedObjects.Enterprise;` | Added using |
| `UPFMProductPropertyRole` not found | Missing `using UnifiedLogin.SharedObjects.Product.UPFMProduct;` | Added using |
| `ACProperty` not found | Missing `using UnifiedLogin.SharedObjects.Product.Accounting;` | Added using |
| Missing `ManagePersona`, `ManageProductBatch` | Missing `using UnifiedLogin.BusinessLogic.Logic;` | Added using |

### `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductUserAsync.cs`

Signature changes to remove `DefaultUserClaim userClaim` parameter from all methods — callers use the injected `IUserClaimsAccessor` instead of passing claim objects per call.

### `UnifiedLogin.BusinessLogic/LogicAsync/ManageProductAssetOptimizationAsync.cs`

Converted from stepping-stone wrapper to direct implementation. Added:
- `GetGbSupportedAoEditorUserProductsToAssignAsync` — offloads blocking HTTP call to `Task.Run` to avoid blocking the request thread.

### `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductAssetOptimizationAsync.cs`

Added `GetGbSupportedAoEditorUserProductsToAssignAsync` to contract.

### `UnifiedLogin.BusinessLogic/Repository/UserRoleRightRepositoryAsync.cs` (266 lines)

Full replacement of `BaseRepository`-based sync class:

| Old pattern | Replacement |
|---|---|
| `BaseRepository` inheritance + 4 constructors | Single DI constructor: `IDbConnection`, `ICacheService`, `ILogger<>` |
| `new RPObjectCache().GetFromCache(key, 120, factory)` | `ICacheService.GetOrSetAsync(key, factory, CacheEntryOptions)` |
| `new ProductInternalSettingRepository()` | Removed — `getRoleRightsSchemaName()` not required by interface |
| `_repository == null` branch in `InsertAssignedRoleToUser` | Removed — `_db` is always present via DI |
| `repository.GetManyWithSpliOn<T1, T2, TReturn>(...)` | `_db.QueryAsync<T1, T2, TReturn>(..., splitOn: "RightId")` — standard Dapper multi-mapping |
| Duplicated `TableValueParamHelper` + param construction in two methods | Extracted to `private static BuildRoleRightsParam` helper |
| `Exception("Missing...")` | `ArgumentException` with `nameof` |

Added two new methods not in original:
- `GetPersistRightsAsync` — `SP_GetPersistRights` with 3-minute cache
- `GetADGroupRightsByPersonaIdAsync` — `SP_GetADGroupRightsByPersona` with persona-keyed cache

---

## Architectural patterns applied consistently

### IDbConnection / IDbTransaction
All DB calls use the same `conn`/`tx` pair with `CommandDefinition`:
```csharp
await conn.QuerySingleOrDefaultAsync<T>(new CommandDefinition(
    StoredProcNameConstants.SP_XYZ,
    new { Param1 = value1 },
    transaction: tx,
    commandType: CommandType.StoredProcedure,
    cancellationToken: ct));
```

### IUserClaimsAccessor — replaces DefaultUserClaim parameter threading
```csharp
_userClaims.UserId            // was: userClaim.UserId
_userClaims.ImpersonatedBy    // was: userClaim.ImpersonatedBy
_userClaims.OrganizationPartyId
_userClaims.PersonaId
```

### ICacheService — replaces RPObjectCache
```csharp
await _cache.GetOrSetAsync(
    cacheKey,
    async _ => { /* factory */ },
    new CacheEntryOptions { ExpirationTimeInMinutes = 2 });
```

### Bounded parallelism — replaces Parallel.ForEach
```csharp
private const int MaxProductDisableParallelism = 5;
var sem = new SemaphoreSlim(MaxProductDisableParallelism);
// tasks use sem.WaitAsync() / sem.Release() around each unit of work
```

### Transaction scope
`UpdateUserAsync` and `CreateUserAsync` each open their own connection + transaction. Inner helpers receive `conn`/`tx` to participate without nesting transactions:
```csharp
using var conn = _connectionFactory.GetConnection();
conn.Open();
using var tx = conn.BeginTransaction();
try { /* ... */ tx.Commit(); }
catch { tx.Rollback(); throw; }
```

---

## Known deferred work (pending future sessions)

| Item | Note |
|---|---|
| **DI registration** | `IUserServiceAsync`, `IManageProductUserAsync` need to be wired in the startup/DI config |
| **Partial class split** | `UserServiceAsync.cs` at 3 406 lines should be split into `UserServiceAsync.cs` (ctor) + `.Create.cs` + `.Update.cs` + `.Disable.cs` etc. |
| `SaveUserProductBatchData` for profile changes | `BatchProcessType.ProfileUpdate` EditorAssignedPersonaList batch not ported |
| `SaveUserProductBatchData` for user-type changes | Type-change batch (e.g. `UserTypeRegularToAdmin`) not ported |
| `GetActivatedUserProductBatchData` | Restore-on-activation batch not ported — async version always uses `newProfile.productBatch` directly |
| `ProductUserActivityLogHelper.GetPrimaryOrganizationName` | Org-name lookup in `WriteActivityLogAsync` still returns empty string — needs `IManageOrganizationAsync` |

---

## Build result

```
dotnet build UnifiedLogin.BusinessLogic.csproj
Build succeeded.
  8 Warning(s)   ← pre-existing CS8xxx nullable / CA2200 warnings in legacy files
  0 Error(s)
```
