# ManageProductOneSiteAccountingAsync Refactor Changelog

**Date:** 2026-04-13
**Branch:** feature/UserRepositoryRefactor

---

## Overview

This refactor rewrites `ManageProductOneSiteAccountingAsync` and expands `IManageProductOneSiteAccountingAsync` from a thin 7-method SOAP proxy stub into a full 21-method async service covering the entire Accounting user-management lifecycle. The rewrite adopts the `IDbConnectionFactory`-backed repository pattern established throughout the async layer: all external dependencies are injected via constructor (12 deps vs 5 in the original), mutable per-call state is eliminated, and .NET 10 idioms are applied throughout.

The legacy synchronous source of truth is `ManageProductOneSiteAccounting.cs` (~3 000 lines). This changelog documents what was ported, what was modernised, and what was intentionally omitted.

---

## Constructor Expansion

| | Original (5 deps) | Refactored (12 deps) |
|---|---|---|
| `IProductContextServiceAsync` | yes | yes |
| `IProductSettingServiceAsync` | yes | yes |
| `IManageBlueBookAsync` | yes | yes |
| `IOneSiteAccountingProductService` | yes | yes |
| `ILogger<...>` | yes | yes |
| `ISamlRepositoryAsync` | — | **added** |
| `IManagePersonaAsync` | — | **added** |
| `IManagePersonAsync` | — | **added** |
| `IManageUserLoginAsync` | — | **added** |
| `IManageElectronicAddressAsync` | — | **added** |
| `IUserRepositoryAsync` | — | **added** |
| `IProductRepositoryAsync` | — | **added** |

All guards use `ArgumentNullException.ThrowIfNull` (.NET 6+).

---

## New Private Context Record

```csharp
private sealed record AccountingCtx(
    Persona EditorPersona,
    string  CompanyName,
    string  IntactLogin,
    string  IntactPassword,
    string  ProductUserId   = "",
    string  ProductUsername = "");
```

Replaces the five mutable instance fields that `GetCompanyEditorAndUserDetails` populated in the legacy class (`_companyName`, `_intactLogin`, `_intactPassword`, `_productUserId`, `_productUsername`). One `AccountingCtx` is resolved per public call via `GetAccountingContextAsync` and flows immutably through the call chain — no shared state between concurrent requests.

---

## New Public Methods Added (14)

The interface grew from 7 to 21 method signatures.

### Properties

| Method | Returns | Notes |
|---|---|---|
| `GetUserPropertiesAsync` | `Task<ListResponse>` | Assigned + available Accounting properties for the user; falls back to full company list when none assigned |
| `GetUserPropertyGroupsAsync` | `Task<ListResponse>` | Location / property groups for the user via `GetAllPropertyGroupsByUser` |
| `GetPropertyGroupEntitiesAsync` | `Task<ListResponse>` | Entities within a specific location group; accepts optional `locationGrpId` filter appended to the NVP array |
| `GetUserPropertiesNewAsync` | `Task<ListResponse>` | "Entities" tab view — sourced from all company properties, filtered to non-empty entries |

### Companies / User Detail

| Method | Returns | Notes |
|---|---|---|
| `GetUserCompaniesAsync` | `Task<ListResponse>` | Associated companies + `AccountingUser` aggregate (admin flag, MConsole flag) returned in `ListResponse.Additional` |

### Roles

| Method | Returns | Notes |
|---|---|---|
| `GetUserRolesAsync` | `Task<ListResponse>` | Assigned + available Accounting roles for the user via `GetRolesByUser` / `GetRolesList` |

### Assignment Helpers

| Method | Returns | Notes |
|---|---|---|
| `AssignAllCurrentCompaniesToUserAsync` | `Task<(string error, List<AdditionalParameters>)>` | "Allow all current and future companies" toggle; forces `isUnRestrictedAccessToProp = true` before delegating to `UpdatePropertiesToUserInternalAsync` |
| `UpdatePropertiesToUserAsync` | `Task<(string error, List<AdditionalParameters>)>` | Public entry point for property delta; delegates to `UpdatePropertiesToUserInternalAsync` |
| `UpdateRolesToUserAsync` | `Task<(string error, List<AdditionalParameters>)>` | Public entry point for role delta; delegates to `UpdateRolesToUserInternalAsync` |

### User Lifecycle

| Method | Returns | Notes |
|---|---|---|
| `ManageAccountingUserAsync` | `Task<(string error, List<AdditionalParameters>)>` | Creates or updates an Accounting user; resolves persona, person, login, email, and supervisor; generates unique login name via `CheckIfUserLoginIsUsedAsync` loop; persists SAML attrs on create; calls `EnableGreenBookUser`; then runs role/property/company assignment |
| `UpdateAccountingUserProfileAsync` | `Task<string>` | Updates name, email, and supervisor for an existing user without touching roles or properties |
| `ChangeAccountingServiceUserTypeAsync` | `Task<(string error, List<AdditionalParameters>)>` | Thin delegation to `ManageAccountingUserAsync` with the supplied `BatchProcessType` (e.g. `UserTypeRegularToAdmin`) |
| `UnassignUserAsync` | `Task<string>` | Disables the Accounting user and marks product status as `Deleted` |
| `DeleteAccountingUserAsync` | `Task<string>` | Permanently deletes the Accounting user and removes their product status record |

---

## Private Helpers Added (13)

| Helper | Purpose |
|---|---|
| `GetAccountingContextAsync` | Resolves `AccountingCtx` once per call; replaces `GetCompanyEditorAndUserDetails` |
| `ResolveCompanyNameAsync` | Extracts company portion from editor's SAML USERID (`COMPANY\|USERID`) or falls back to BlueBook FinancialSuite lookup |
| `UpdateRolesToUserInternalAsync` | Resolves role delta, calls `RemoveRolesFromUser` / `AssignRolesToUser` via `Task.Run`, builds structured audit trail |
| `UpdatePropertiesToUserInternalAsync` | Resolves property delta (handles MConsole, location groups, companies), calls `RemovePropertiesFromUser` / `AssignPropertiesToUser` via `Task.Run` |
| `GetUserAsync` | `Task.Run` wrapper for `_service.GetUser(NameValuePair[])` |
| `GetAllCompanyPropertiesAsync` | `Task.Run` wrapper for `_service.getPropertiesAPI` |
| `GetAllPropertyGroupsAsync` | `Task.Run` wrapper for `_service.GetAllPropertyGroups` |
| `GetUserCompaniesDetailsAsync` | `Task.Run` wrapper for `_service.getCompaniesAPI` |
| `CheckIfUserLoginIsUsedAsync` | `Task.Run` wrapper for `_service.CheckIfUserIDIsUsed`; used in login uniqueness loop inside `ManageAccountingUserAsync` |
| `ComputeFlagBasedOnCompanyAndPropertySelectedAsync` | Computes whether unrestricted property access applies given company/property selections |
| `GetPropertiesAdditionalParametersAsync` | Builds property audit trail by comparing before/after `ACProperty` and `ProductPropertyGroup` lists |
| `GetSupervisorUserDetailsAsync` | Chains `IUserRepositoryAsync.GetSuperVisorInformationAsync` → `IManageUserLoginAsync` → `IManagePersonaAsync` → `ISamlRepositoryAsync` to resolve supervisor's Accounting product username |
| `ResolveEmailAddressAsync` | Resolves primary/fallback email via `IManageElectronicAddressAsync` |

Plus two static NVP builders:

| Static Helper | Purpose |
|---|---|
| `BuildLoginInfo(AccountingCtx)` | Builds `NameValuePair[]` with `CompanyID`, `Login`, `Password`, and optional `SystemIdentifier` |
| `BuildEditorLoginInfo(AccountingCtx)` | Builds `NameValuePair[]` without `SystemIdentifier` (editor-level company operations) |

---

## Interface Additions

The following signatures were added to `IManageProductOneSiteAccountingAsync`:

```csharp
Task<ListResponse> GetUserPropertiesAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);
Task<ListResponse> GetUserPropertyGroupsAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);
Task<ListResponse> GetPropertyGroupEntitiesAsync(long editorPersonaId, long userPersonaId, string locationGrpId, RequestParameter datafilter, CancellationToken cancellationToken = default);
Task<ListResponse> GetUserPropertiesNewAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);
Task<ListResponse> GetUserCompaniesAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);
Task<ListResponse> GetUserRolesAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);
Task<(string error, List<AdditionalParameters> auditParams)> AssignAllCurrentCompaniesToUserAsync(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, bool isAccountingAdmin, BatchProcessType batchProcessType, List<ACProperty>? beforeUpdatePropertiesList = null, List<ProductPropertyGroup>? beforeUpdateLocationGrpList = null, List<ACProperty>? beforeUpdateEntitiesList = null, CancellationToken cancellationToken = default);
Task<(string error, List<AdditionalParameters> auditParams)> UpdatePropertiesToUserAsync(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, bool isAccountingAdmin, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser, bool isUnRestrictedAccessToProp = false, List<ACProperty>? beforeUpdatePropertiesList = null, List<ProductPropertyGroup>? beforeUpdateLocationGrpList = null, List<ACProperty>? beforeUpdateEntitiesList = null, CancellationToken cancellationToken = default);
Task<(string error, List<AdditionalParameters> auditParams)> UpdateRolesToUserAsync(long editorPersonaId, long userPersonaId, List<string> rolesToAssign, bool isAccountingAdmin, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser, ListResponse? currentRolesList = null, CancellationToken cancellationToken = default);
Task<(string error, List<AdditionalParameters> auditParams)> ManageAccountingUserAsync(long editorPersonaId, long userPersonaId, List<string> roleList, List<string> propertyList, List<string> companyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser, CancellationToken cancellationToken = default);
Task<string> UpdateAccountingUserProfileAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);
Task<(string error, List<AdditionalParameters> auditParams)> ChangeAccountingServiceUserTypeAsync(long editorPersonaId, long userPersonaId, List<string> roleList, List<string> propertyList, List<string> companyList, bool isAccountingAdmin, bool isSiteSpendManagementUser, bool isUnRestrictedAccessToProp, BatchProcessType batchProcessType, CancellationToken cancellationToken = default);
Task<string> UnassignUserAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);
Task<string> DeleteAccountingUserAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);
```

All new signatures carry `CancellationToken cancellationToken = default` as the last parameter and are grouped under XML doc-comment region headers (`// ── Properties`, `// ── Companies / User detail`, `// ── Roles`, `// ── Assignment helpers`, `// ── User lifecycle`).

---

## .NET 10 Improvements

- **Collection expressions `[]`**: All empty-collection initialisers use C# 12 collection expressions (`[]`) instead of `new List<T>()` or `Array.Empty<T>()`. NVP array spread syntax used in `GetPropertyGroupEntitiesAsync` (`[.. loginInfo, new NameValuePair { ... }]`).
- **`ArgumentNullException.ThrowIfNull`**: Replaces manual null checks and `throw new ArgumentNullException(nameof(...))` throughout the constructor.
- **`StringComparison.OrdinalIgnoreCase`**: All string equality checks use `StringComparison.OrdinalIgnoreCase` instead of `.ToUpper() ==` or `.ToLower() ==` (e.g. `DecodeBase64Setting`, `ChangeUserStatusAsync` response check, SOAP error guards).
- **`Interlocked.Increment` for login name uniqueness**: The legacy `incrementor++` inside the `while (loginIsUsed)` loop is replaced with `Interlocked.Increment(ref _loginNameIncrementor)` so concurrent calls do not share the counter unsafely.
- **`Task.Run` for all blocking SOAP overloads**: Every `IOneSiteAccountingProductService` method with a `NameValuePair[]` signature (no native async proxy counterpart) is wrapped in `Task.Run(...)` so blocking SOAP I/O is off-loaded to the thread pool rather than consuming an ASP.NET request thread.
- **`EmailAddressAttribute` + `MailAddress` for email validation**: `ValidateAndReturnEmail` (ported from `ManageProductBase.ValidateAndReturnEmailAddress`) uses `System.ComponentModel.DataAnnotations.EmailAddressAttribute` and `System.Net.Mail.MailAddress` rather than hand-rolled string splitting.
- **`Math.Min` replaces ternary for page-size clamp**: Where applicable, `Math.Min(x, y)` is used instead of `x > max ? max : x`.

---

## IDbConnectionFactory Pattern

Business logic exclusively uses injected repository and service interfaces that are themselves backed by `IDbConnectionFactory` in the data-access layer:

- `IProductContextServiceAsync` — replaces `GetCompanyEditorAndUserDetails` / `verifyPersona`
- `IProductSettingServiceAsync` — replaces `GetProductSettings` / `GetProductInternalSettings`
- `ISamlRepositoryAsync` — replaces `SamlRepository.GetProductSamlDetails` called via `DefaultUserClaim`
- `IManagePersonaAsync` — replaces `ManagePersona.ListActivePersona(...)`
- `IManagePersonAsync` — replaces `ManagePerson.GetPerson(...)`
- `IManageUserLoginAsync` — replaces `ManageUserLogin.GetUserLoginOnly(...)`
- `IManageElectronicAddressAsync` — replaces `ManageElectronicAddress.GetElectronicAddress(...)`
- `IUserRepositoryAsync` — replaces `ManageProductBase.GetSuperVisorInformation(...)`
- `IProductRepositoryAsync` — replaces direct DB calls for product status record management

No `new Repository(...)` or `new ManageUnifiedLogin(userClaims)` instantiation occurs anywhere in the refactored class. DI manages all lifetimes and connection pooling.

---

## Mutable State Eliminated

| Legacy mutable field | Replaced by |
|---|---|
| `_companyName` | `AccountingCtx.CompanyName` (immutable, per-call) |
| `_intactLogin` | `AccountingCtx.IntactLogin` (immutable, per-call) |
| `_intactPassword` | `AccountingCtx.IntactPassword` (immutable, per-call) |
| `_productUserId` | `AccountingCtx.ProductUserId` (immutable, per-call) |
| `_productUsername` | `AccountingCtx.ProductUsername` (immutable, per-call) |
| `_isUnRestrictedAccessToProp` | Explicit parameter threaded through call chain |
| `incrementor` (login name loop) | `_loginNameIncrementor` via `Interlocked.Increment` |

---

## Removed / Not Ported

- **`RPObjectCache`**: Thread-safety concerns with async; cache invalidation is handled at the repository/service layer. Not ported.
- **`WriteToDiagnosticLog` / `WriteToErrorLog`**: Replaced entirely by `ILogger<ManageProductOneSiteAccountingAsync>` structured logging (`LogDebug`, `LogError`).
- **`DefaultUserClaim` constructor parameter**: Removed from all method signatures. Per-call context is resolved internally via `IProductContextServiceAsync`.
- **Sync overloads of `DefaultUserClaim`-bound `ManageUnifiedLogin` calls** (e.g. `PushToQueue`): Audit data is instead returned as `List<AdditionalParameters>` in the tuple return values and consumed by the calling service layer.

---

## Files Changed

- `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductOneSiteAccountingAsync.cs` — interface expanded from 7 to 21 method signatures; added usings for `UnifiedLogin.SharedObjects.Audit.Common`, `UnifiedLogin.SharedObjects.Enum`, `UnifiedLogin.SharedObjects.Product`, `UnifiedLogin.SharedObjects.Product.Accounting`
- `UnifiedLogin.BusinessLogic/LogicAsync/ManageProductOneSiteAccountingAsync.cs` — rewritten from ~591 lines to ~2 115 lines; constructor expanded from 5 to 12 dependencies; 14 new public methods + 13 new private helpers; new usings for `System.ComponentModel.DataAnnotations`, `System.Net.Mail`, `System.Text.RegularExpressions`, `UnifiedLogin.BusinessLogic.Repository.Interfaces`, `UnifiedLogin.SharedObjects.Audit.Common`, `UnifiedLogin.SharedObjects.Extensions`, `UnifiedLogin.SharedObjects.Product`, `UnifiedLogin.SharedObjects.Product.Accounting`, `UnifiedLogin.SharedObjects.Saml`
