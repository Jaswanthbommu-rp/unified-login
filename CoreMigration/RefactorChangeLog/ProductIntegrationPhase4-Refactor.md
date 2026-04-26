# ProductIntegration — Phase 4 Refactor Changelog

**Scope:** Concrete product integration implementations  
**Date:** 2026-04-15  
**Target framework:** .NET 10 / C# 13

---

## Files Created

| New file | Replaces |
|----------|---------|
| `LogicAsync/Helpers/ProductImplementation/SelfGuidedTourAsync.cs` | `Logic/ProductIntegration/ProductImplementation/SelfGuidedTour.cs` |
| `LogicAsync/Helpers/ProductImplementation/LeadManagementAsync.cs` | `Logic/ProductIntegration/ProductImplementation/LeadManagement.cs` |
| `LogicAsync/Helpers/ProductImplementation/DepositAlternativeManagementAsync.cs` | `Logic/ProductIntegration/ProductImplementation/DepositAlternativeManagement.cs` |
| `LogicAsync/Helpers/ProductImplementation/ClickPayManagementAsync.cs` | `Logic/ProductIntegration/ProductImplementation/ClickPayManagement.cs` |
| `LogicAsync/Helpers/ProductImplementation/PortfolioManagementAsync.cs` + `PortfolioRoleProperty` | `Logic/ProductIntegration/ProductImplementation/PortfolioManagement.cs` |

## Files Modified (base class adjustments)

| File | Change |
|------|--------|
| `LogicAsync/Helpers/StandardV1ProductIntegrationAsync.cs` | `CreateApi`, `ToListResponse`, `LogDebug`, `LogError` promoted from `private` to `protected` |
| `LogicAsync/Helpers/StandardV1ProductIntegrationAsync.cs` | `_dataCollector`, `_productRepository`, `_managePersona`, `_manageUserLogin`, `_tokenHelper` promoted from `private readonly` to `protected readonly` |

---

## Breaking Changes Per Implementation

### `SelfGuidedTourAsync`

No behavioural changes. Removed the two sync constructors (one with full injected dependencies,
one minimal) — replaced by a single constructor matching the base class `protected` constructor.

---

### `LeadManagementAsync`

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `ApplySuperUserData` — sets role list, `productUser.Properties = new List<string>() { "all" }` | `ApplySuperUserDataAsync` — uses C# 13 collection expressions `["18"]` / `["17"]` / `[]` |
| 2 | `UpdateSamlUserAttribute` — direct `_dataCollector.UpdateSamlUserAttribute(...)` (sync, void) | `UpdateSamlUserAttributeAsync` — `await _dataCollector.UpdateSamlUserAttributeAsync(...)` |
| 3 | `CreateUpdateProductUser(…, out List<AdditionalParameters> additionalParameters, …)` — `out` param, blocks | `CreateUpdateProductUserAsync(…)` — returns `Task<(string, List<AdditionalParameters>)>` |
| 4 | Login name: `firstName[..1] + lastName + "_" + _productDetails.BooksProductCode + "_" + personaId` | Same format, `_productDetails.BooksProductCode` → `BlueBookGbProductMap.BooksProductCode` (protected property) |
| 5 | No guard for empty `ProductUserName` on update path | Explicit `LogDebug` + direct branch to `UpdateUserAsync` |

---

### `DepositAlternativeManagementAsync`

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `GetProductRoles` — calls `base.GetProductRoles(dataFilter)` then adds `CanReceiveMonthlyReport` | `GetProductRolesAsync` — awaits `base.GetProductRolesAsync(dataFilter, ...)` |
| 2 | `MergeUserPropertyGroups` — filters groups by role (sync, pure logic, no HTTP) | Unchanged logic; `override void` retained as sync (no async needed — pure in-memory operation) |
| 3 | `UpdateSamlUserAttribute` — `_dataCollector.UpdateSamlUserAttribute(...)` (void, blocking) | `UpdateSamlUserAttributeAsync` — awaited |
| 4 | `GetProductPropertyGroups` — uses `GetPropertyGroupsEndpoint` (plural) explicitly | `GetProductPropertyGroupsAsync` — same endpoint key; returns `new ListResponse { IsError = true }` on exception (not re-throw) to match sync behaviour |
| 5 | `UnassignUser` — manual `new ManageUserLogin()`, `new ManagePersona()`, `new UserLoginRepository()`, `new SamlRepository()` | `UnassignUserAsync` — calls `base.UnassignUserAsync(ct)` then appends SAML deletion via injected `ISamlAttributeServiceAsync.DeleteProductInfoAndStatusAsync` |
| 6 | Extra constructor parameter: none (SAML used via `new SamlRepository()`) | Extra constructor parameter: `ISamlAttributeServiceAsync samlAttributeService` |

**Note on SAML ordering (DepositAlternative)**: in the sync version, `samlRepository.DeleteSamlUserProductInfoAndStatus` was called
*before* `_dataCollector.UpdateProductSettingProductStatus`. In the async version, the base handles
the status update first; the SAML deletion follows. The order is functionally equivalent.

---

### `ClickPayManagementAsync`

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `GetProductRoles` — `GetResultFromApi<ClickPayRoles>(url)` | `GetProductRolesAsync` — `await GetResultFromApiAsync<ClickPayRoles>(url, ct: ct)` |
| 2 | `GetProductRoles` — `GetProductOrganizations(item.Id, item.OrgType).Records.Cast<ClickPayOrganization>()` (sync) | Awaits `GetProductOrganizationsAsync(item.Id, item.OrgType, ct: ct)` |
| 3 | `GetProductOrganizations` — `GetResultFromApi<ClickPayOrganizations>(url)` (sync) | Awaits `GetResultFromApiAsync<ClickPayOrganizations>(url, ct: ct)` |
| 4 | `GetProductUser` — returns `null` on empty list (undeclared nullability) | `GetProductUserAsync` — returns `null` with `Task<IntegrationProductUser?>` (NRT compliant) |
| 5 | `CheckUserExistInProduct` — calls `base.CheckUserExistInProduct("", baseUrlAndQuery)` | `CheckUserExistInProductAsync` — calls `GetProductUserAsync(url, isThrowOnError: false, ct)` directly |
| 6 | `GenerateProductUserObject` — calls `GetProductRoles(null, "")` sync | `GenerateProductUserObjectAsync` — awaits `GetProductRolesAsync(null!, ct: ct)` |
| 7 | `GetMigrationUsers` — `new ApiIntegration(_httpClient, url).GetEntityFromApi<ClickPayUsers>()` (direct instantiation, blocking) | `GetMigrationUsersAsync` — `await GetResultFromApiAsync<ClickPayUsers>(url, false, ct)` |
| 8 | `UnassignUser` — `new ManageUserLogin()`, `new ManagePersona()`, `new UserLoginRepository()` | `UnassignUserAsync` — injected `_manageUserLogin`, `_managePersona`; no `new()` instantiation |
| 9 | `UnassignUser` calls private `DeleteUser()` which issues a PUT | `UnassignUserAsync` — shared `ClickPayPutUserAsync` private helper (PUT to `PutUserEndpoint`) |
| 10 | `ExternalProductUserProfileChange` — `new ApiIntegration(_httpClient, url).PutEntity<>()` | `ExternalProductUserProfileChangeAsync` — `await ClickPayPutUserAsync(...)` |
| 11 | `ProductUserProfileChange` — `new ApiIntegration(_httpClient, url).PutEntity<>()` | `ProductUserProfileChangeAsync` — `await ClickPayPutUserAsync(...)` |
| 12 | `CreateUpdateProductUser(…, out …)` | `CreateUpdateProductUserAsync(…)` → `Task<(string, List<AdditionalParameters>)>` |
| 13 | Private `DeleteUser(IntegrationProductUser)` — duplicated PUT logic | Eliminated — consolidated into `ClickPayPutUserAsync(user, ct)` private helper |
| 14 | Static `_managePersona` field + null guard (`if (_managePersona == null) _managePersona = new ManagePersona()`) | Field removed — `_managePersona` now comes from base class `protected readonly` field |

---

### `PortfolioManagementAsync`

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `ApplyApiSecurity` — `new HttpClient()` + `SetBasicAuthentication` + `.Result` blocking `SendAsync` / `ReadAsStringAsync` | `ResolveApiSecurityAsync` — `await _tokenHelper.GetExternalClientCredentialServerTokenAsync(...)` |
| 2 | Token: credentials sent as HTTP **Basic auth header** | Token: `GetExternalClientCredentialServerTokenAsync` sends credentials in the **request body** (OAuth 2.0 spec compliant). **⚠ Potential breaking change** if the portfolio token endpoint requires header credentials — see note below. |
| 3 | `GetProductRoles` — `GetResultFromApi<IList<ProductRole>>(url)` + inline `MergeUserRoles(roleList, user.RoleList)` private shadow | `GetProductRolesAsync` — awaited; calls base `MergeUserRoles` directly (the private shadow removed) |
| 4 | `GetProductUser` — `base.GetProductUser(baseUrlAndQuery)` forwarding | `GetProductUserAsync` — directly calls `GetResultFromApiAsync<IntegrationProductUser>(url, ...)` |
| 5 | `GetProductProperties` — sequential `GetPortfolioProperties()`, `GetPortfolioPropertyGroups()`, `GetPortfolioPropertySpecificRoles()` | `GetProductPropertiesAsync` — `Task.WhenAll(...)` parallel fetch for all three |
| 6 | `GetProductPropertiesByGroup` — public non-override method | **Not present** in `IManageProductIntegrationAsync` — the base `GetProductPropertiesByGroupAsync` handles this via `GetPropertiesByGroupEndpoint` |
| 7 | `CheckUserExistInProduct` — calls `base.GetProductUser(baseUrlAndQuery, false)` | `CheckUserExistInProductAsync` — calls `GetResultFromApiAsync<IntegrationProductUser>` directly (avoids user-wrapper ambiguity) |
| 8 | `CreateAdditionalSamlUserAttribute` — `_dataCollector.CreateSamlUserAttribute(...)` (void) | `CreateAdditionalSamlUserAttributeAsync` — `await _dataCollector.CreateSamlUserAttributeAsync(...)` |
| 9 | Private `GetPortfolioProperties/Groups/PropertySpecificRoles` — sync `GetResultFromApi<T>` | Private `GetPortfolio*Async` — async; return `?? []` fallback (C# 13 collection expression) |
| 10 | `PortfolioRoleProperty` nested class — `public class` in same file | Retained in same file; changed to `public sealed class` |
| 11 | Private `MergePropertyRoles(IList<PortfolioRoleProperty>, List<PAMRolePropertyList>)` | Retained as `private static` (pure logic) |

**⚠ Portfolio token protocol note**: the sync `GetPortfolioManagementAccessToken` sets `Basic` auth in the
`Authorization` header before posting `grant_type=client_credentials` as form-URL-encoded body.
`ITokenHelperAsync.GetExternalClientCredentialServerTokenAsync` puts `client_id` and `client_secret`
**in the request body** (RFC 6749 §2.3.1 recommendation). If the Portfolio Management token endpoint
**requires** the credentials in the `Authorization` header (not body), this override needs a custom
`HttpClient` call instead of `_tokenHelper`. Verify with the Portfolio Management vendor.

---

## Base Class Changes (StandardV1ProductIntegrationAsync)

| Change | Reason |
|--------|--------|
| `CreateApi` promoted `private` → `protected` | ClickPay and DepositAlternative need direct `ApiIntegrationAsync` access |
| `ToListResponse<T>` promoted `private static` → `protected static` | Concrete implementations need consistent `ListResponse` construction |
| `LogDebug` / `LogError` promoted `private` → `protected` | Concrete implementations use structured logging via the same logger |
| `_dataCollector` promoted `private readonly` → `protected readonly` | LeadManagement, DepositAlternative, ClickPay use `_dataCollector` directly |
| `_productRepository` promoted `private readonly` → `protected readonly` | Future implementations may need direct repository access |
| `_managePersona` promoted `private readonly` → `protected readonly` | ClickPay `UnassignUserAsync` uses `_managePersona.GetPersonaAsync` |
| `_manageUserLogin` promoted `private readonly` → `protected readonly` | ClickPay `UnassignUserAsync` uses `_manageUserLogin` |
| `_tokenHelper` promoted `private readonly` → `protected readonly` | Portfolio `ResolveApiSecurityAsync` uses `_tokenHelper` |

---

## Performance Improvements

| Area | Improvement |
|------|-------------|
| ClickPay `UnassignUser` — `new ManageUserLogin()`, `new ManagePersona()`, `new UserLoginRepository()` per call | Eliminated — injected base-class fields reused |
| ClickPay `ExternalProductUserProfileChange` — `new ApiIntegration(_httpClient, url)` per call | `ClickPayPutUserAsync` helper reuses `CreateApi(url)` via `IHttpClientFactory` pool |
| ClickPay duplicate `DeleteUser` / `ProductUserProfileChange` / `ExternalProductUserProfileChange` PUT code (~50 lines) | Collapsed into single `ClickPayPutUserAsync(user, ct)` private helper |
| Portfolio `GetProductProperties` — three sequential API calls | `Task.WhenAll(propertiesTask, propertyGroupsTask, propertyRolesTask)` — all three fetched in parallel |
| Portfolio `GetPortfolioManagementAccessToken` — `new HttpClient()` + `.Result` blocking | `ITokenHelperAsync.GetExternalClientCredentialServerTokenAsync` — async, no blocking |
| Portfolio `_httpClient` stored as mutable field, mutated in `ApplyApiSecurity` | Eliminated — `ResolveApiSecurityAsync` returns `AuthenticationHeaderValue` used per-call by `ApiIntegrationAsync` |
| DepositAlternative `UnassignUser` — `new SamlRepository()`, `new ManageUserLogin()`, `new ManagePersona()`, `new UserLoginRepository()` | Eliminated — `base.UnassignUserAsync(ct)` + injected `ISamlAttributeServiceAsync` |

---

## C# 13 Features Used

| Feature | Where |
|---------|-------|
| Collection expressions `[]` | `LeadManagementAsync.ApplySuperUserDataAsync` — `["18"]`, `["17"]`, `[]` |
| Collection expressions `[]` | `ClickPayManagementAsync.GenerateProductUserObjectAsync` — `orgRoleList = []`, `OrganizationRoles = [new OrganizationRole {...}]` |
| `??= []` and `?? []` | `PortfolioManagement` async helpers — `return ... ?? []` fallback |
| `is { Count: > 0 }` pattern | `ClickPayManagementAsync.GetProductUserAsync` — wrapper null + count check |
| `is { UserId: { Length: > 0 } }` pattern | `PortfolioManagementAsync.CheckUserExistInProductAsync` |
| Target-typed `new()` | `ListResponse` and `Dictionary` initialisers throughout |
| File-scoped namespaces | All 5 implementation files |
| `sealed class` | All 5 implementation files + `PortfolioRoleProperty` |
| `ArgumentNullException.ThrowIfNull` | `DepositAlternativeManagementAsync` ctor guard on `ISamlAttributeServiceAsync` |
| `is not null` / `is null` patterns | Throughout — persona/user guard checks |
| `??` null-coalescing | `PortfolioManagementAsync.ResolveApiSecurityAsync`, `GetPortfolio*Async` return fallbacks |
| Switch expression (`? :`) with pattern matching | `ClickPayManagementAsync.UnassignUserAsync` — `orgStatus.Status` → status value |
| LINQ method chaining | `PortfolioManagementAsync.GetProductPropertiesAsync` — `allPropertyRoles.Select(role => ...).ToList()` |
| `where` clause filter LINQ | `ClickPayManagementAsync.GenerateProductUserObjectAsync` — `Where(r => r.IsAssigned && ...)` |

---

## DI Registration (Phase 4)

```csharp
// Phase 4 concrete implementations — register as Transient.
// Factory (Phase 6) is responsible for instantiation with per-operation parameters
// (productId, editorPersonaId, subjectPersonaId) and calling InitAsync().

services.AddTransient<SelfGuidedTourAsync>();
services.AddTransient<LeadManagementAsync>();
services.AddTransient<DepositAlternativeManagementAsync>();
services.AddTransient<ClickPayManagementAsync>();
services.AddTransient<PortfolioManagementAsync>();

// DepositAlternativeManagement requires ISamlAttributeServiceAsync
// (already registered from the product-manager refactor):
services.AddScoped<ISamlAttributeServiceAsync, SamlAttributeServiceAsync>();
```

---

## What Phase 4 Does NOT Change

- `StandardV1ProductIntegrationAsync` public API (`IManageProductIntegrationAsync`) — unchanged.
- `ApiIntegrationAsync`, `DataCollectorAsync`, `TokenHelperAsync` — unchanged.
- `IManageProductIntegrationAsync` interface — defined in Phase 1, unchanged.
- `ClickPayRoles`, `ClickPayUsers`, `ClickPayOrganizations` DTO models — reused from sync layer.
- `ProductActivityLogger` — fire-and-forget audit sink, unchanged.
- `MergeUserRoles`, `MergeUserGroup`, `MergeUserProperties` — protected static helpers on base, reused as-is.
- Phase 5 (integration-type wrappers) and Phase 6 (factory layer) — still pending.
