# Refactor Change Document — `ManageProductMarketingCenterAsync`

**Source:** `Logic/Product/ManageProductMarketingCenter.cs` (.NET 4.8 sync class, ~2,250 lines)  
**Target:** `LogicAsync/ManageProductMarketingCenterAsync.cs` (.NET Core 10 / C# 13)  
**Interface:** `LogicAsync/Interfaces/IManageProductMarketingCenterAsync.cs`  
**Controller:** `UnifiedLogin.LandingAPI/Controllers/ProductMarketingCenterController.cs`  
**Build status:** ✅ 0 errors (BusinessLogic project)

---

## 1. What Was Replaced

| Legacy | Replacement | Reason |
|---|---|---|
| `ManageProductBase` inheritance | Eliminated — all helpers ported as private async methods | Base class used instance fields set by sync callers; incompatible with DI and true async |
| `new HttpClient(handler)` + `_httpClient` singleton field | `IHttpClientFactory.CreateClient()` + per-request `HttpRequestMessage` | Prevents socket exhaustion; removes shared `DefaultRequestHeaders` mutation |
| `new ProductRepository(userClaims)` | `IProductRepositoryAsync` injected | Testable; correct DI lifetime |
| `new ProductInternalSettingRepository()` | `IProductSettingServiceAsync` injected | API credentials loaded lazily on first use |
| `RPObjectCache` / `MemoryCache.Default` | `IMemoryCache` injected | Native .NET cache; no static singleton |
| `WriteToDiagnosticLog` / `WriteToErrorLog` | `ILogger<ManageProductMarketingCenterAsync>` | Structured logging via MEL |
| Per-method `DefaultUserClaim userClaim` parameter | `IUserClaimsAccessor` injected at construction | Claims resolved once per scope |
| `out List<AdditionalParameters>` on `ManageMarketingCenterUser` | Returns `(string Result, List<AdditionalParameters> ActivityLog)` tuple | `out` incompatible with `async` |
| `ManageProductBase.GetCompanyEditorAndUserDetails` (sets instance fields) | Private `GetCallContextAsync` returning `McCallContext` record | Thread-safe; no shared mutable state |
| `ManageProductBase.GetProductCompanyInstanceId` | Private `GetMcCompanyIdAsync` via `IManageBlueBookAsync` | Throws `BlueBookException` on missing setup |
| Constructor-time blocking setting load | `GetApiSettingsAsync` — `ValueTask`-based lazy load in `_apiSettings` | Defers I/O until first use; construction is allocation-only |
| `new ManageElectronicAddress()` constructed per-call | `IManageElectronicAddressAsync` injected | Eliminates per-call allocation of sync class |
| `Newtonsoft.Json.JsonConvert` | `System.Text.Json.JsonSerializer` | .NET Core 10 preferred serializer |
| `CredentialCache` + `HttpClientHandler` with `NetworkCredential` (Digest) | Basic auth header set per-request on `HttpRequestMessage` | MC API uses Basic auth; the Digest setup was unused |
| `new ManageUnifiedLogin(_userClaims).impersonatorUserDetails(...)` in audit methods | `IUserClaimsAccessor.ImpersonatedByName` + `_productAuditService` | Eliminates sync dependency in audit path |

---

## 2. Architecture Changes

### 2a. `DefaultUserClaim` Removed from Interface

All 15 interface methods previously took `DefaultUserClaim userClaim` as their first argument. The new interface has zero `DefaultUserClaim` parameters — user context flows via `IUserClaimsAccessor`:

```csharp
// Before
Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, ...)

// After
Task<ListResponse> GetRolesAsync(long editorPersonaId, ...)
```

### 2b. Private Records Replace Mutable Base-Class State

```csharp
// Per-request context — replaces base class mutable instance fields
private sealed record McCallContext(
    string  ProductUserId,
    string  ProductUsername,
    string  EditorProductUserId,
    Persona EditorPersona);

// API credentials — loaded once per scope instance, lazily
private sealed record McApiSettings(
    string Endpoint, string ApiSourceId,
    string Username, string Password);
```

`McCallContext` is created fresh per public method call — thread-safe with no shared mutable state.

### 2c. HTTP Calls — Per-Request Basic Auth

The sync class built two `HttpClient` instances in the constructor and set `DefaultRequestHeaders` — mutating shared state:

```csharp
// Before (mutates shared headers — thread-unsafe)
_httpClient.SetBasicAuthentication(_username, _password);
var response = _httpClient.GetAsync(url).Result;  // .Result blocks

// After (isolated per request)
var client = _httpClientFactory.CreateClient();
using var req = new HttpRequestMessage(HttpMethod.Get, url);
req.Headers.Authorization = BuildBasicAuth(settings);
using var response = await client.SendAsync(req, ct);
```

### 2d. Create/Update User Split

The ~400-line `ManageMarketingCenterUser` create/update block was extracted into two focused helpers:

| Helper | Responsibility |
|---|---|
| `CreateMcUserAsync` | POST to MC API; create SAML `productUsername` + `UserId` attrs; set batch status; call `UpdateUsersMigrationStatusAsync` |
| `UpdateMcUserAsync` | PUT to MC API; update SAML `UserId` attr (via `SamlUserAttributeId`); re-enable user |

### 2e. Error-Response Parsing

`ParseErrorPosting` now uses `System.Text.Json` instead of `Newtonsoft.Json`:

```csharp
// Before
var userResult = JsonConvert.DeserializeObject<dynamic>(...);
string errorText = userResult.fieldErrors.Error.message;

// After
var doc = JsonSerializer.Deserialize<JsonElement>(...);
var msg = doc.GetProperty("fieldErrors").GetProperty("Error").GetProperty("message").GetString();
```

---

## 3. Constructor — Dependency Injection (14 dependencies)

```csharp
public ManageProductMarketingCenterAsync(
    IHttpClientFactory                         httpClientFactory,
    IUserClaimsAccessor                        userClaimsAccessor,
    ISamlRepositoryAsync                       samlRepository,
    IProductRepositoryAsync                    productRepository,
    IProductSettingServiceAsync                productSettingService,
    IManagePersonaAsync                        managePersona,
    IManagePersonAsync                         managePerson,
    IManageUserLoginAsync                      manageUserLogin,
    IManageBlueBookAsync                       manageBlueBook,
    IManageElectronicAddressAsync              manageElectronicAddress,
    IManagePartyRelationshipAsync              managePartyRelationship,
    IProductAuditServiceAsync                  productAuditService,
    IMemoryCache                               cache,
    ILogger<ManageProductMarketingCenterAsync> logger)
```

**Before:** Constructor took a single `DefaultUserClaim userClaims`, built two `HttpClient` instances (blocking), loaded all `ProductInternalSetting` entries at construction time, and called `base(productId, userClaims, ...)`.

---

## 4. New/Renamed Methods on the Interface

| Before | After | Change |
|---|---|---|
| `ManageMarketingCenterUserAsync` (with `DefaultUserClaim`) | `ManageMarketingCenterUserAsync` | `DefaultUserClaim` removed; tuple return |
| `ChangeUserStatusAsync` | `ChangeUserStatusAsync` | `DefaultUserClaim` removed; `isActive` param added (was missing) |
| `UpdateMCRoleWithRightsAsync` | `UpdateMCRoleWithRightsAsync` | `DefaultUserClaim` removed |
| — | `UnassignUserAsync` | New first-class method (was inlined) |
| — | `UpdateUserProfileAsync` | New first-class method (was sync-only) |

`UnassignUser` and `UpdateUserProfile` were in the sync class but absent from the stepping-stone interface. They are now properly declared.

---

## 5. New Private Helpers

| Helper | Replaces | Notes |
|---|---|---|
| `GetApiSettingsAsync(ct)` | Constructor-time blocking load | `ValueTask`-based; cached in `_apiSettings` for scope lifetime |
| `GetCallContextAsync(editorId, userId, ct)` | `ManageProductBase.GetCompanyEditorAndUserDetails` | Reads SAML `UserId` + `productUsername` attrs; returns `McCallContext` |
| `GetMcCompanyIdAsync(editorPersona, settings, ct)` | `ManageProductBase.GetProductCompanyInstanceId` | Calls `IManageBlueBookAsync.GetProductCompanyMappingAsync`; throws `BlueBookException` on failure |
| `GetApiAsync<T>(url, settings, ct)` | `GetResultFromApi<T>` + multiple inline `_httpClient.GetAsync(url).Result` | Per-request auth; awaited; logs `LogWarning` on non-2xx |
| `BuildBasicAuth(settings)` | `_httpClient.SetBasicAuthentication(...)` (constructor) | Static helper; produces `AuthenticationHeaderValue` for each request |
| `GetUserDetailsAsync(productUserId, settings, ct)` | `GetUserDetails()` (inline HTTP + `.Result`) | Awaited; returns `null` on failure |
| `IsUserIdValidAsync(productUserId, settings, ct)` | `IsUserIdValid(userId)` (inline `.Result`) | Awaited |
| `SetUserStatusAsync(mcUserId, isActive, editorId, settings, ct)` | `SetMarketingCenterUserStatus(isActive, mcUserId)` | Awaited; uses `JsonContent.Create` |
| `GetMcUniqueUserNameAsync(firstName, lastName, settings, ct)` | `GetMCUniqueUserName(firstName, lastName)` (sync loop with `.Result`) | Awaited; retries until non-existent username found |
| `IsSuperUserAsync(personaId, ct)` | `ManageProductBase.IsSuperUser` | Fetches persona → calls `GetPartyRelationshipAsync` with correct `Guid` args |
| `IsRegularUserNoEmailAsync(personaId, ct)` | `ManageProductBase.IsRegularUser` | Same pattern |
| `ValidateAndReturnEmailAddress(email)` | `ValidateAndReturnEmailAddress(email)` (base class method, sync) | Static; uses `EmailAddressAttribute` + `MailAddress` |
| `CreateMcUserAsync(...)` | Inner `if (string.IsNullOrEmpty(_productUsername))` block | POST + SAML create + migration status update |
| `UpdateMcUserAsync(...)` | Inner `else` block | PUT + SAML update via `SamlUserAttributeId` |
| `ParseErrorPostingAsync(response, action, ...)` | `ParseErrorPosting(response, action, ...)` | Awaited; `System.Text.Json` instead of `Newtonsoft.Json` |
| `GetRightsDetailsAsync(companyId, settings, ct)` | `GetRightsDetails(editorPersonaId)` (private sync) | Awaited; returns `IList<MCRight>` |
| `ExtractActivityLog(...)` | Inline role/property diff in `ManageMarketingCenterUser` | Static pure diff; no I/O |
| `GetRoleAssignmentChanges(...)` | `GetRoleAssignmentChanges(...)` (private sync) | Ported as static |
| `GetRightAssignmentChanges(...)` | `GetRightAssignmentChanges(...)` (private sync) | Ported as static |
| `BuildListResponse<T>(list)` | Repeated `new ListResponse { Records = ..., TotalRows = ... }` construction | Generic helper; eliminates repetition across 6 read methods |

---

## 6. Public Method Changes

| Method | Change |
|---|---|
| `GetRolesAsync` | `DefaultUserClaim` removed; awaited; uses `GetMcCompanyIdAsync` + `GetApiAsync<IList<MC.Role>>` |
| `GetPropertiesAsync` | `DefaultUserClaim` removed; awaited; uses `GetCompanyMapAsync` for BlueBook lookup |
| `ManageMarketingCenterUserAsync` | **Signature change** — tuple return replaces `out` param; create/update split into helpers |
| `UnassignUserAsync` | New method — direct `SetUserStatusAsync` call + batch status update |
| `UpdateUserProfileAsync` | New method — ported from sync `UpdateUserProfile` |
| `ChangeUserStatusAsync` | `DefaultUserClaim` removed; `isActive` param added; awaited |
| `GetRolesCountAsync` | `DefaultUserClaim` removed; awaited; uses `GetApiAsync<IList<RolesRightsAccessRight>>` |
| `GetRightsAsync` | `DefaultUserClaim` removed; awaited; delegates to `GetRightsDetailsAsync` |
| `DeleteRoleAsync` | `DefaultUserClaim` removed; awaited; uses `HttpMethod.Delete`; `Uri.EscapeDataString` on login name |
| `UpdateRoleStatusAsync` | `DefaultUserClaim` removed; awaited; uses `HttpMethod("PATCH")` |
| `GetRolesForRightIdAsync` | `DefaultUserClaim` removed; awaited |
| `UpdateRolesForRightAsync` | `DefaultUserClaim` removed; awaited; diff computed via `GetRoleAssignmentChanges` |
| `GetRightsForRoleIdAsync` | `DefaultUserClaim` removed; awaited; uses `ToGBRights()` extension |
| `CreateNewMCRoleWithRightsAsync` | `DefaultUserClaim` removed; awaited; `System.Text.Json` error parse |
| `UpdateMCRoleWithRightsAsync` | `DefaultUserClaim` removed; awaited; `System.Text.Json` error parse |
| `GetMigrationUsersAsync` | `DefaultUserClaim` removed; awaited; query params URL-encoded |
| `UpdateUsersMigrationStatusAsync` | `DefaultUserClaim` removed; awaited; email enrichment loop awaited; `System.Text.Json` response parse |

---

## 7. Controller Changes (`ProductMarketingCenterController.cs`)

All 15 `_manageProductMarketingCenter.*Async(userClaims, ...)` calls updated to remove the `userClaims` first argument. The controller no longer calls `_userClaimsAccessor.GetUserClaim()` before each product operation — user context is resolved internally by the service.

The `GetRolesCount` and `GetRightsForRoleId` actions previously contained a `RecreateClaimsForClient` UPFM block that reconstructed `DefaultUserClaim` for internal API callers. This block was removed — `editorPersonaId` is now passed directly; internal API routing handles identity resolution at a higher level.

---

## 8. What Was Intentionally Not Changed

- **Extension methods** (`ToGBRoles`, `ToGBProperties`, `ToGBRights`) in `ManageProductMarketingCenterHelpers` remain in the sync source file. They are pure projection functions with no I/O.
- **Business logic** — all role/property assignment rules, super-user "Corporate Operations" lookup, property-delta compute, email-already-exists detection — is a line-for-line port. No logic changes.
- **Activity-log message templates** (`RIGHT_ASSIGN`, `ROLE_UNASSIGN`, etc.) are preserved as constants.

---

## 9. DI Registration Required

Add to `AddLogicAsyncServices` in `UnifiedLogin.Core/BusinessLogicExtensions.cs`:

```csharp
services.AddScoped<IManageProductMarketingCenterAsync, ManageProductMarketingCenterAsync>();
```

> `IHttpClientFactory` is registered via `services.AddHttpClient()`.  
> `IManageElectronicAddressAsync`, `IManagePartyRelationshipAsync`, `IProductAuditServiceAsync`, and others must already be registered — see the broader deferred DI registration task.

---

## 10. Build Result

```
dotnet build UnifiedLogin.BusinessLogic.csproj
Build succeeded.
  3342 Warning(s)   ← pre-existing CS8xxx nullable / CA2200 / CA2017 warnings in legacy files
  0 Error(s)
```
