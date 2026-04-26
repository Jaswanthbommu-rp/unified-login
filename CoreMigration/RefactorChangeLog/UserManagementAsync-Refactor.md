# UserManagementAsync Refactor Changelog

**Date:** 2026-04-18  
**Branch:** `feature/UserRepositoryRefactor`  
**Target:** .NET 10 / C# 13  

---

## Summary

Full async refactor of `UnifiedLogin.BusinessLogic/Logic/Enterprise/User/UserManagement.cs`
into `UnifiedLogin.BusinessLogic/LogicAsync/Enterprise/`.

Removes every instance of:
- `new Xxx(_userClaims)` inline repository instantiation in logic methods
- `DefaultUserClaim` constructor dependency in the service class
- `new ProductRepository(_userClaims)` cross-repository coupling inside the data layer
- `new EntUserRepository(_userClaims)` inline in logic methods

Replaces with:
- 15 DI-injected dependencies (14 async services + `IManageUserRegistrationEmail` pending async port)
- `IUserClaimsAccessor` for all caller-identity access
- `IDbConnectionFactory` + Dapper for transactional data access in `EntUserRepositoryAsync`
- Product code→ID resolution lifted to the logic layer (`BuildProductCodeToIdMapAsync`)
- Fully async method signatures with `CancellationToken` on every I/O path

---

## Files Created

### Repository Layer

| File | Replaces | Key change |
|------|----------|------------|
| `Repository/Enterprise/IEntUserRepositoryAsync.cs` | `Repository/Interfaces/IEntUserRepository.cs` | Accepts pre-resolved `IReadOnlyDictionary<string, int> productCodeToIdMap` — removes cross-repository dependency |
| `Repository/Enterprise/EntUserRepositoryAsync.cs` | `Repository/Enterprise/EntUserRepository.cs` | `IDbConnectionFactory` + Dapper; single `SqlConnection` per operation; transactional `CreateEnterpriseUserAsync` with `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync` |

#### `EntUserRepositoryAsync` — method signatures

| Method | Notes |
|--------|-------|
| `CreateEnterpriseUserAsync(UserProductDetails, IReadOnlyDictionary<string,int>, CT)` | Opens connection, begins `ReadCommitted` transaction; creates user → resolves persona → optionally resolves impersonator → queues product batch; commits atomically |
| `ListUsersAsync(orgPartyId, productIdList, statusTypeId, realPageId?, name?, rowsPerPage, pageNumber, CT)` | Short-lived connection per call; errors logged and return `[]` instead of propagating |
| `ListUserProductDetailsLoginByPersonaIdAsync(personaId, CT)` | Short-lived connection |
| `ListUserProductDetailsLoginByLoginNameAsync(loginName, CT)` | Short-lived connection |

#### Design note: `productCodeToIdMap`

The sync `EntUserRepository.SaveProductBatch` called `new ProductRepository(_userClaims)` internally
to resolve product codes to IDs — cross-repository coupling. In the async design this lookup is the
caller's responsibility: the logic layer calls `BuildProductCodeToIdMapAsync` and passes a read-only
map, keeping the repository a pure data-access component.

---

### Logic Layer

| File | Replaces | Key change |
|------|----------|------------|
| `LogicAsync/Enterprise/IUserManagementAsync.cs` | `Logic/Enterprise/Interfaces/IUserManagement.cs` | Async signatures; all methods accept `CancellationToken`; `ImpersonatedBy` identity flows via `IUserClaimsAccessor` |
| `LogicAsync/Enterprise/UserManagementAsync.cs` | `Logic/Enterprise/User/UserManagement.cs` | 15-dependency constructor; all I/O fully awaited; `DefaultUserClaim` removed |

#### `IUserManagementAsync` — interface

```csharp
Task<ObjectResponse> CreateEnterpriseUnityUserAsync(UserProductDetails, CT);
Task<ObjectResponse> UpdateEnterpriseUnityUserAsync(UserProductDetails, CT);
Task<ObjectResponse> ActivateDeactivateUserAsync(Guid unityRealPageUserId, UserUiStatusType, CT);
Task<IList<UsersData>> ListUsersAsync(long orgPartyId, int statusTypeId, Guid? realPageId, string? name, int rowsPerPage, int pageNumber, CT);
Task<IList<UserProductDetailLogin>> ListUserProductDetailsLoginByPersonaIdAsync(long personaId, CT);
Task<IList<UserProductDetailLogin>> ListUserProductDetailsLoginByLoginNameAsync(string loginName, CT);
```

#### `UserManagementAsync` — constructor dependencies

| Dependency | Role |
|------------|------|
| `IUserClaimsAccessor` | Replaces `DefaultUserClaim` constructor parameter |
| `IEntUserRepositoryAsync` | Transactional user creation + list queries |
| `IManageUserLoginAsync` | Login existence checks, username validation, status updates |
| `IProductRepositoryAsync` | Product code→ID resolution, company product lookup |
| `IProductInternalSettingRepositoryAsync` | Shared-product setting (`SharedProductId`) |
| `ICustomFieldsRepositoryAsync` | Custom field fetch, validate, persist |
| `IUserRepositoryAsync` | Fetch current user details by RealPageId |
| `IUserLoginPersonaRepositoryAsync` | Resolve persona IDs for custom field assignment |
| `IManageOrganizationAsync` | Company existence + IdP configuration checks |
| `IManageCredentialAsync` | Password policy validation |
| `IManagePersonaAsync` | Resolve editor's first available persona for status cascade |
| `IManageProfileAsync` | Fetch profile for invitation email |
| `IManageProductOpsAsync` | Validate OPS property and role IDs |
| `IManageUserRegistrationEmail` *(SYNC)* | Email dispatch — no async port yet |
| `ILogger<UserManagementAsync>` | Structured logging |

---

## Method Implementation Details

### `CreateEnterpriseUnityUserAsync`

12-step pipeline (all I/O awaited):

1. `ValidateUserProductDetailsAsync` — password presence, company existence, IdP config, product codes, password policy
2. `IsLoginNameExistsAsync` — duplicate login + domain allow-list check
3. `ValidateProductDataAsync` — OPS property/role ID validation, shared-product code remapping
4. Password hash (pure CPU — `PasswordHash()` extension, no I/O)
5. `ValidateAndAssignCustomFieldValuesAsync` — required-field check, max/min char-length validation
6. Name trimming (`TrimWhiteSpace()` extension)
7. `BuildProductCodeToIdMapAsync` — pre-resolves product codes for repository layer
8. `_entUserRepository.CreateEnterpriseUserAsync` — atomic DB transaction
9. `SendInvitationEmailAsync` — optional invitation email via `IManageProfileAsync` + sync email service
10. `_userRepository.GetUserDetailsAsync` + `_userLoginPersonaRepository.ListUserLoginPersonaAsync` — resolve new user details
11. `_customFieldsRepository.AddUpdateFieldValueAsync` — persist custom field values
12. `LogAuditActivity` (×2 if email sent)

### `UpdateEnterpriseUnityUserAsync`

1. `ValidateUsernameAsync` — confirm login name matches RealPageId
2. `ValidateUserProductDetailsAsync`
3. `ValidateProductDataAsync`
4. Password hash
5. `GetUserDetailsAsync` + `ListUserLoginPersonaAsync` — fetch current state
6. `ValidateAndAssignCustomFieldValuesAsync`
7. Builds `IProfileDetail` update object with org/persona context from `IUserClaimsAccessor`
8. `GetProductBatchDataAsync` — resolves product batch for update payload
9. **SYNC:** `ManageUser.UpdateUser` — pending `IManageUserAsync` async port

### `ActivateDeactivateUserAsync`

1. `GetUserLoginOnlyAsync` — verify user exists
2. `CreateUpdateUserStatusAsync` — update login status
3. `UpdateUserProductStatusAsync` — cascade to product assignments (uses **SYNC** `ManageUser.UpdateUserStatus`)

### `ListUsersAsync`

- Filters product list to `ProductEnum.OpsBuyer` (if company has it) + `ProductEnum.UnifiedPlatform`
- Delegates to `_entUserRepository.ListUsersAsync`

### `ListUserProductDetailsLogin*`

Both methods project `UserProductDetailAttribute` (repository type) to `UserProductDetailLogin`
(interface type). The `ByLoginName` variant also maps `UserType` integer codes to
`UserRoleType` enum descriptions using a `switch` expression.

---

## Private Helpers

| Helper | Purpose |
|--------|---------|
| `ValidateUserProductDetailsAsync` | Validates password, company, IdP, per-product codes and OPS constraints |
| `ValidateProductDataAsync` | Validates OPS property/role IDs via `IManageProductOpsAsync`; calls `GetProductSharedWithOtherProductAsync` |
| `GetProductSharedWithOtherProductAsync` | Resolves `SharedProductId` internal setting to remap product codes (e.g., "LeadAnalytics" → "ClickPay") |
| `ValidateAndAssignCustomFieldValuesAsync` | Fetches enabled company custom fields; checks required/length constraints; assigns caller values |
| `BuildProductCodeToIdMapAsync` | Deduplicates and resolves product codes to IDs via `IProductRepositoryAsync.ListProductsAsync` |
| `GetProductBatchDataAsync` | Builds `IList<ProductBatch>` for the update payload |
| `SendInvitationEmailAsync` | Fetches profile via `IManageProfileAsync`; dispatches via sync `IManageUserRegistrationEmail` |
| `UpdateUserProductStatusAsync` | Resolves editor's persona via `IManagePersonaAsync`; calls sync `ManageUser.UpdateUserStatus` |
| `LogAuditActivity` | Calls `LogActivity.WriteActivity` (static — no async port exists) |

---

## `file static class ObjectResponseExtensions`

Added a C# 13 `file`-scoped static class in `UserManagementAsync.cs`:

```csharp
file static class ObjectResponseExtensions
{
    internal static ObjectResponse WithError(this ObjectResponse response, string reason)
    {
        response.IsError     = true;
        response.ErrorReason = reason;
        return response;
    }
}
```

This avoids a public helper leaking into the assembly surface and keeps early-return error paths
concise: `return response.WithError("reason")` instead of the 3-line set-and-return pattern used
throughout the sync `UserManagement`.

---

## Remaining SYNC Annotations

| Location | Method | Blocked on |
|----------|--------|------------|
| `UserManagementAsync.UpdateEnterpriseUnityUserAsync` | `ManageUser.UpdateUser` | `IManageUserAsync` async port |
| `UserManagementAsync.UpdateUserProductStatusAsync` | `ManageUser.UpdateUserStatus` | `IManageUserAsync` async port |
| `UserManagementAsync.SendInvitationEmailAsync` | `_manageUserRegistrationEmail.SendNewUserRegistrationEmail` | `IManageUserRegistrationEmailAsync` port |
| `UserManagementAsync.LogAuditActivity` | `LogActivity.WriteActivity` | Static helper — no async variant exists |

---

## DI Registration Template

```csharp
// Repository (Scoped — IDbConnectionFactory is Singleton; logger is Scoped)
services.AddScoped<IEntUserRepositoryAsync, EntUserRepositoryAsync>();

// Logic (Scoped)
services.AddScoped<IUserManagementAsync, UserManagementAsync>();

// Sync dependency still required until IManageUserRegistrationEmailAsync exists
services.AddScoped<IManageUserRegistrationEmail, ManageUserRegistrationEmail>();
```

---

## C# 13 / .NET 10 Features Used

| Feature | Usage |
|---------|-------|
| `file static class` | `ObjectResponseExtensions` — restricts fluent helper to the declaring file |
| Collection expressions `[...]` | `Persona` list initializer in `UpdateEnterpriseUnityUserAsync`; `UserLoginOnly` inline list in `ActivateDeactivateUserAsync`; empty `Role` list in persona construction |
| `ArgumentNullException.ThrowIfNull` | All constructor null-guards |
| `ConfigureAwait(false)` | All `await` sites in public and private async methods |
| File-scoped namespaces | All new files use `namespace X.Y;` syntax |
| Nullable reference types (`string?`, `Guid?`, `long?`) | Throughout — method parameters and local variables |
| `is not { Count: > 0 }` pattern | Null-and-count guard in `ValidateAndAssignCustomFieldValuesAsync` |
| `is null or { Count: 0 }` pattern | Early-out in `ValidateProductDataAsync` and `BuildProductCodeToIdMapAsync` |
| `switch` expression | `UserType` integer → `UserRoleType` enum description mapping in `ListUserProductDetailsLoginByLoginNameAsync` |
| `await using` | `SqlConnection` and `DbTransaction` in `EntUserRepositoryAsync` |
| `IsolationLevel.ReadCommitted` transaction | Explicit isolation on `BeginTransactionAsync` |
| `StringComparer.OrdinalIgnoreCase` dictionary | `productCodeToIdMap` in `BuildProductCodeToIdMapAsync` |
