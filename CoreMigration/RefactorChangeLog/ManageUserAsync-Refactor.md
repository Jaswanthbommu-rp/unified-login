# ManageUserAsync — Refactor Changelog

**Date:** 2026-04-23  
**Branch:** `feature/UserRepositoryRefactor`  
**Author:** Vijayasekher Manellore  
**Framework target:** .NET 10 / C# 13.0

---

## Files Created

| File | Role |
|------|------|
| `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageUserAsync.cs` | New async-first interface |
| `UnifiedLogin.BusinessLogic/LogicAsync/ManageUserAsync.cs` | Stepping-stone async implementation |

## Files Modified

| File | Change |
|------|--------|
| `UnifiedLogin.BusinessLogic/LogicAsync/Enterprise/UserManagementAsync.cs` | Replaced two `new ManageUser(userClaim)` SYNC stubs with injected `IManageUserAsync` |

---

## Problem

`UserManagementAsync` had two `// SYNC` stubs that bypassed async execution by directly
instantiating the sync `ManageUser` class and calling blocking methods:

```csharp
// UpdateEnterpriseUnityUserAsync — line ~353
var userClaim  = _userClaims.GetUserClaim();
var manageUser = new ManageUser(userClaim);
var result     = manageUser.UpdateUser(userProductDetails.EditorRealPageId, updateObject);

// UpdateUserProductStatusAsync — line ~844
var userClaim  = _userClaims.GetUserClaim();
var manageUser = new ManageUser(userClaim);
manageUser.UpdateUserStatus(_userClaims.UserRealPageGuid, persona.PersonaId, userLogins, statusType);
```

Both called `ManageUser` from `UnifiedLogin.BusinessLogic.Logic` — a namespace that was not even
imported in `UserManagementAsync.cs`, meaning the file had a latent compile error.

---

## Solution

### `IManageUserAsync` — two methods

```
UpdateUserAsync(Guid loggedInUserRealPageId, IProfileDetail profile, CT)
    → Task<RepositoryResponse>

UpdateUserStatusAsync(Guid editorRealPageId, long editorPersonaId,
                      IList<UserLoginOnly> userLogins, UserUiStatusType?, CT)
    → Task<RepositoryResponse>
```

### `ManageUserAsync` — what is fully async vs. stepping-stone

#### `UpdateUserAsync`

| Step | Before (sync `ManageUser`) | After (`ManageUserAsync`) |
|------|---------------------------|--------------------------|
| Get old profile snapshot | `GetUserProfile(...)` — sync, creates 8+ objects inline | `IManageProfileAsync.GetProfileDetailAsync(...)` — native async |
| Get persona list | `new UserLoginPersonaRepository().ListUserLoginPersona(...)` | `IUserLoginPersonaRepositoryAsync.ListUserLoginPersonaAsync(...)` — native async |
| Get employee ID | `GetUserEmployeeId(...)` sync call | `IUserRepositoryAsync.GetUserEmployeeIdAsync(...)` — native async |
| **Profile + persona list** | Sequential | **Parallel via `Task.WhenAll`** |
| Persist update | `_userRepository.UpdateUser(...)` — **no async version** | `Task.Run(() => new UserRepository(claim).UpdateUser(...))` — stepping stone |
| Notification email | Sync `ManageUserRegistrationEmail.SendNewUserRegistrationEmail` | `IManageUserRegistrationEmailAsync.SendNewUserRegistrationEmailAsync` — native async |

#### `UpdateUserStatusAsync`

| Step | Before | After |
|------|--------|-------|
| `DisableUserProduct` | Sync call | `Task.Run(...)` — stepping stone |
| `ActivateUserProducts` | Sync call | `Task.Run(...)` — stepping stone |

---

## Stepping-Stone Note

`UserRepository.UpdateUser`, `DisableUserProduct`, and `ActivateUserProducts` have no
counterparts in `IUserRepositoryAsync` yet. They are wrapped with `Task.Run` and a fresh
`UserRepository(userClaim)` instance — exactly the same pattern the previous inline code used,
but now encapsulated behind the interface.

**TODO:** When `IUserRepositoryAsync` gains these three methods, replace the `Task.Run` blocks
in `ManageUserAsync` and remove the `IUserClaimsAccessor.GetUserClaim()` calls.

---

## Changes to `UserManagementAsync.cs`

| Location | Before | After |
|----------|--------|-------|
| `using` | `using UnifiedLogin.BusinessLogic.Repository;` (unused — `ManageUser` is in `Logic` not `Repository`) | removed |
| Field | none | `private readonly IManageUserAsync _manageUser` added |
| Constructor param | none | `IManageUserAsync manageUser` added (15th param) |
| Constructor body | none | `_manageUser = manageUser ?? throw ...` added |
| `UpdateEnterpriseUnityUserAsync` step 8 | 4-line `new ManageUser(...)` block | `await _manageUser.UpdateUserAsync(...)` |
| `UpdateUserProductStatusAsync` | 3-line `new ManageUser(...)` block | `await _manageUser.UpdateUserStatusAsync(...)` |
| Class XML doc | `Methods annotated // SYNC call a sync service pending an async port.` | paragraph removed — no SYNC stubs remain |

---

## Pending Work (still in `IUserRepositoryAsync`)

These three methods need to be added to unblock the `Task.Run` removal in `ManageUserAsync`:

```csharp
Task<RepositoryResponse> UpdateUserAsync(Guid loggedInUserRealPageId, IProfileDetail newProfile, IProfileDetail oldProfile, CT);
Task DisableUserProductAsync(Guid editorRealPageId, long editorPersonaId, IList<UserLoginOnly> userLogins, CT);
Task ActivateUserProductsAsync(Guid editorRealPageId, long editorPersonaId, IList<UserLoginOnly> userLogins, CT);
```

---

## Registration (DI)

```csharp
services.AddScoped<IManageUserAsync, ManageUserAsync>();
```
