# ManageProductOneSiteAsync Refactor Changelog

**Date:** 2026-04-13
**Branch:** feature/UserRepositoryRefactor

---

## Overview

This refactor extends `ManageProductOneSiteAsync` and `IManageProductOneSiteAsync` with 12 new public methods ported from the legacy synchronous `ManageProductOneSite.cs`. The migration adopts the `IDbConnectionFactory`-backed repository pattern already established throughout the async layer: all external dependencies are injected via constructor (e.g., `ISamlRepositoryAsync`, `IProductSettingServiceAsync`, `IProductContextServiceAsync`) rather than newed up directly as in the legacy constructors. Additionally, several .NET 10 improvements were applied to the existing implementation to remove Newtonsoft.Json and modernise random/collection usage.

---

## New Public Methods Added

| Method Name | Returns | Notes |
|---|---|---|
| `GetUsersForPropertyAsync` | `Task<ListResponse>` | Gets OneSite users for a given property ID; delegates to `_service.GetUsersForPropertyAsync` |
| `GetUsersForRoleAsync` | `Task<ListResponse>` | Gets OneSite users for a given role ID; delegates to `_service.GetUsersForRoleAsync` |
| `GetRightsCentersAsync` | `Task<ListResponse>` | Returns rights centers for the editor's PMC; delegates to `_service.GetRightsCentersListAsync` |
| `GetRightsAsync` | `Task<ListResponse>` | Returns OneSite rights list, optionally filtered by role; delegates to `GetOneSiteRightsMainAsync` helper |
| `GetRolesForRightAsync` | `Task<ListResponse>` | Returns roles associated with a given right ID; delegates to `_service.GetRolesForRightAsync` |
| `UpdateRightToRolesAsync` | `Task<string>` | Assigns or removes a right from a list of roles; delegates to `_service.ModifyRightToRolesAsync` |
| `UpdateRoleToRightsAsync` | `Task<string>` | Adds and/or removes rights from a role in one call; delegates to `_service.ModifyRoleToRightsAsync` |
| `AddUpdateRoleAsync` | `Task<ListResponse>` | Creates or updates an OneSite role; delegates to `_service.AddUpdateRoleAsync` |
| `DeleteRoleAsync` | `Task<string>` | Deletes an OneSite role; delegates to `_service.DeleteRoleAsync` |
| `UnassignUserAsync` | `Task<string>` | Disables the OneSite user, optionally deletes SAML product info, updates product status to Deleted |
| `UpdateRolesForUserAsync` | `Task<(string, List<AdditionalParameters>)>` | Resolves context, checks super-user, resolves PMC ID, delegates to `UpdateRolesForUserInternalAsync` |
| `UpdatePropertiesForUserAsync` | `Task<(string, List<AdditionalParameters>)>` | Resolves context, resolves PMC ID, delegates to `UpdatePropertiesForUserInternalAsync` |

---

## Private Helpers Added

- **`GetOneSiteRightsMainAsync`** — Async counterpart to the legacy `GetOneSiteRightsMain`. Calls `_service.GetRightsListAsync` with a generated `FilterSortParameters` (sorted by `RightDescription`). Returns an empty `RightList` on exception rather than throwing, keeping the call-site error-path consistent with the other list helpers.
- **`ParseSoapErrorMessage`** — Static utility that extracts the inner `<ErrorMessage>` text from a SOAP fault envelope string. Falls back to the full message if the XML tags are absent. Used in all write-path catch blocks across the new methods.

---

## Interface Additions

The following method signatures were added to `IManageProductOneSiteAsync`:

- `Task<ListResponse> GetUsersForPropertyAsync(long editorPersonaId, int propertyId, bool assignedOnly, RequestParameter datafilter, CancellationToken ct = default)`
- `Task<ListResponse> GetUsersForRoleAsync(long editorPersonaId, int roleId, bool assignedOnly, RequestParameter datafilter, CancellationToken ct = default)`
- `Task<ListResponse> GetRightsCentersAsync(long editorPersonaId, CancellationToken ct = default)`
- `Task<ListResponse> GetRightsAsync(long editorPersonaId, RequestParameter datafilter, long roleId = 0, bool assignedToRoleOnly = false, CancellationToken ct = default)`
- `Task<ListResponse> GetRolesForRightAsync(long editorPersonaId, int rightId, bool assignedOnly, RequestParameter datafilter, CancellationToken ct = default)`
- `Task<string> UpdateRightToRolesAsync(long editorPersonaId, int rightId, List<string> roles, bool assignRight, CancellationToken ct = default)`
- `Task<string> UpdateRoleToRightsAsync(long editorPersonaId, int roleId, List<string> rightsToAdd, List<string> rightsToRemove, CancellationToken ct = default)`
- `Task<ListResponse> AddUpdateRoleAsync(long editorPersonaId, int roleId, string roleName, string inheritRoleId, CancellationToken ct = default)`
- `Task<string> DeleteRoleAsync(long editorPersonaId, int roleId, CancellationToken ct = default)`
- `Task<string> UnassignUserAsync(long editorPersonaId, long userPersonaId, bool deleteSamlUserProductInfoAndStatus = false, CancellationToken ct = default)`
- `Task<(string resultCount, List<AdditionalParameters> auditParams)> UpdateRolesForUserAsync(long editorPersonaId, long userPersonaId, List<string> rolesToAssign, CancellationToken ct = default)`
- `Task<(string resultCount, List<AdditionalParameters> auditParams)> UpdatePropertiesForUserAsync(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, CancellationToken ct = default)`

All new signatures carry `CancellationToken ct = default` as the last parameter and are grouped under region comments (`// ── Property / Role / User list helpers`, `// ── Rights`, `// ── Role admin`, `// ── User management`) with one-sentence XML `<summary>` docs.

---

## .NET 10 Improvements

- **`System.Text.Json` replaces `Newtonsoft.Json`**: The `using Newtonsoft.Json` import was replaced with `using System.Text.Json`. Token extraction in `GetMtTokenAsync` now uses `JsonDocument.Parse` with `doc.RootElement.GetProperty("access_token").GetString()` instead of deserialising to `dynamic`. Generic API deserialization in `GetResultFromApiAsync<T>` uses `JsonSerializer.Deserialize<T>(json, _jsonOptions)` with a shared `private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)` field.
- **`Random.Shared.Next` replaces `new Random().Next`**: The PIN generation expression `new Random().Next(1, 10000)` in `ManageOneSiteUserAsync` is replaced with `Random.Shared.Next(1, 10000)`, avoiding per-call allocations and the thread-safety caveat of `new Random()`.
- **Collection expressions `[]` replace `new List<T>()`**: Empty list initialisers in the new methods use the C# 12 collection expression `[]` rather than `new List<T>()` / `new List<AdditionalParameters>()`.
- **All SOAP operations use native `...Async()` generated proxy methods**: Every new SOAP call (`GetUsersForPropertyAsync`, `GetUsersForRoleAsync`, `GetRightsCentersListAsync`, `GetRightsListAsync`, `GetRolesForRightAsync`, `ModifyRightToRolesAsync`, `ModifyRoleToRightsAsync`, `AddUpdateRoleAsync`, `DeleteRoleAsync`) has a proper generated async overload on the proxy and is awaited directly. No `Task.Run` wrappers are introduced for these. The pre-existing `Task.Run` usages for the `NameValuePair[]` overloads (`CreateUser`, `UpdateUser`, `GetUser`) remain unchanged, as those specific overloads have no async counterpart on the proxy.

---

## IDbConnectionFactory Pattern

Business logic in this layer exclusively uses injected repository interfaces (`ISamlRepositoryAsync`, `IProductSettingServiceAsync`, `IProductContextServiceAsync`, `IUserRepositoryAsync`, `IUserLoginPersonaRepositoryAsync`, etc.) that are themselves backed by `IDbConnectionFactory` in the data-access layer. This eliminates all direct `new Repository(...)` and `new ManageUnifiedLogin(userClaims)` instantiation patterns found in the legacy constructors of `ManageProductOneSite`. Dependency lifetime and connection pooling are now fully managed by the DI container.

---

## Removed / Not Ported

The legacy audit queue methods `UpdateRightsToRoleLogMessage` and `UpdateRolesByRightLogMessage` (which called `ManageUnifiedLogin.PushToQueue` with a sync `DefaultUserClaim`-bound `ManageUnifiedLogin` instance) are intentionally not ported. Audit logging is handled by the product audit service pattern used throughout the async layer; the `List<AdditionalParameters>` return values from `UpdateRolesForUserAsync` and `UpdatePropertiesForUserAsync` provide the structured audit data for consumption by the calling service.

---

## Files Changed

- `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductOneSiteAsync.cs`
- `UnifiedLogin.BusinessLogic/LogicAsync/ManageProductOneSiteAsync.cs`
