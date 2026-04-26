# Refactor Change Document — `ManageProductLead2LeaseAsync`

**Source:** `Logic/Product/ManageProductLead2Lease.cs` (.NET 4.8 sync class)  
**Target:** `LogicAsync/ManageProductLead2LeaseAsync.cs` (.NET Core 10 / C# 13, ~700 lines)  
**Interface:** `LogicAsync/Interfaces/IManageProductLead2LeaseAsync.cs`  
**Build status:** ✅ 0 errors

---

## 1. What Was Replaced

| Legacy | Replacement | Reason |
|---|---|---|
| `ManageProductBase` inheritance | Eliminated — all helpers ported as private async methods | Base class used instance fields set by sync callers; incompatible with DI and true async |
| `new HttpClient()` per REST call | `IHttpClientFactory.CreateClient()` + per-request `HttpRequestMessage` | Prevents socket exhaustion; removes shared `DefaultRequestHeaders` mutation |
| `new ProductRepository(userClaims)` | `IProductRepositoryAsync` injected | Testable; DI lifetime management |
| `new OrganizationRepository(userClaims)` | Removed — organization context flows via `IManagePersonaAsync` | Same as above |
| `new ProductInternalSettingRepository()` | `IProductSettingServiceAsync` injected | API credentials now loaded lazily on first use |
| `RPObjectCache.GetFromCache<T>(key, ttl, factory)` | `IMemoryCache.GetOrCreateAsync(key, entry => { ... })` | Native .NET cache; no static singleton |
| `WriteToDiagnosticLog` / `WriteToErrorLog` | `ILogger<ManageProductLead2LeaseAsync>` | Structured logging via MEL with `{ActionName}` + `{State}` template |
| Per-method `DefaultUserClaim userClaim` parameter | `IUserClaimsAccessor` injected at construction | Claims resolved once per scope; callers no longer pass user context on every call |
| `out List<AdditionalParameters> additionalParameters` on `ManageLead2LeaseUser` | Returns `(string Error, List<AdditionalParameters> ActivityLog)` tuple | `out` parameters are incompatible with `async` methods |
| `ManageProductBase.GetCompanyEditorAndUserDetails` (sets instance fields) | Private `GetCallContextAsync` returning `L2LCallContext` record | Thread-safe; no shared mutable state between concurrent calls on the same scoped instance |
| `ManageProductBase.GetProductCompanyInstanceId` | Private `GetL2LCompanyAsync` calling `IManageBlueBookAsync.GetProductCompanyMappingAsync` | Replaces base class helper with injected async service |
| Constructor-time blocking `ProductInternalSetting` load | `GetApiSettingsAsync` — `ValueTask`-based lazy load in `_apiSettings` field | Defers I/O until first use; construction is allocation-only |
| Direct `ISamlRepository` reads (sync) | `ISamlRepositoryAsync.GetSamlUserAttributesAsync` | Full async |
| `Newtonsoft.Json.JsonConvert` for migration responses | `System.Text.Json.JsonSerializer` | .NET Core 10 preferred serializer |
| Static `EmailAddressAttribute` / regex email check | `System.ComponentModel.DataAnnotations.EmailAddressAttribute` + `MailAddress` | Framework-native validation without custom regex |

---

## 2. Architecture Changes

### 2a. `DefaultUserClaim` Removed from Interface

Every method on the old interface took `DefaultUserClaim userClaim` as a first argument (because the sync class required it to initialise the base class). The new interface has zero `DefaultUserClaim` parameters:

```csharp
// Before
Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, ...)

// After
Task<ListResponse> GetRolesAsync(long editorPersonaId, long userPersonaId, ...)
```

### 2b. Private Records Replace Mutable Base-Class State

```csharp
// Per-request context — replaces base class instance fields
// set by ManageProductBase.GetCompanyEditorAndUserDetails
private sealed record L2LCallContext(
    string  ProductUserId,
    string  ProductUsername,
    string  EditorProductUserId,
    Persona EditorPersona);

// API credentials — loaded once per scope instance, lazily
private sealed record L2LApiSettings(string ApiEndpoint, string MtApiEndpoint);
```

`L2LCallContext` is created fresh per public method call — no shared mutable state between concurrent requests on the same scoped instance.

### 2c. HTTP Calls — Per-Request Auth Headers

The L2L API uses HTTP Basic Auth. The sync class set `DefaultRequestHeaders` on a shared `HttpClient`, making concurrent calls unsafe. The async implementation uses per-request headers:

```csharp
// Before (mutates shared state — thread-unsafe)
_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
var result = _client.GetAsync(url).Result;  // .Result blocks

// After (isolated per request)
using var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
using var response = await client.SendAsync(request, ct);
```

### 2d. Cross-Product OneSite Integration

Lead2Lease requires OneSite site-ID and leasing-agent data when a property has a `PMSystemID`. Previously this was resolved via direct `new ManageProductOneSite()` construction. The async class injects `IManageProductOneSiteAsync` and calls two new methods that were promoted to the interface:

```csharp
// In BuildPropertyListAsync — async enrichment of each property's PMSystemID:
var oneSiteUser = await _manageOneSite.GetOneSiteUserInfoAsync(property.PMSystemID, ct);
bool isLeasingAgent = await _manageOneSite.UserInLeasingAgentListAsync(property.PMSystemID, siteId, ct);
```

### 2e. Super-User Admin Rights — Static `HashSet`

The 15 admin permission strings used for super-user assignment are now a `private static readonly IReadOnlySet<string>` — created once at class load, not rebuilt per call:

```csharp
private static readonly IReadOnlySet<string> AdminRights = new HashSet<string>(
    StringComparer.OrdinalIgnoreCase)
{
    "ALLOW USER TO CHANGE PASSWORDS MANUALLY",
    "FULL ACCESS",
    "SUPER USER",
    // ... 12 more
};
```

---

## 3. Constructor — Dependency Injection (15 dependencies)

```csharp
public ManageProductLead2LeaseAsync(
    IHttpClientFactory                      httpClientFactory,
    IUserClaimsAccessor                     userClaimsAccessor,
    ISamlRepositoryAsync                    samlRepository,
    IProductRepositoryAsync                 productRepository,
    IProductSettingServiceAsync             productSettingService,
    IManagePersonaAsync                     managePersona,
    IManagePersonAsync                      managePerson,
    IManageUserLoginAsync                   manageUserLogin,
    IManageBlueBookAsync                    manageBlueBook,
    IManageElectronicAddressAsync           manageElectronicAddress,
    IManagePartyRelationshipAsync           managePartyRelationship,
    IManageProductOneSiteAsync              manageOneSite,
    IProductAuditServiceAsync               productAuditService,
    IMemoryCache                            cache,
    ILogger<ManageProductLead2LeaseAsync>   logger)
```

**Before:** Constructor took a single `DefaultUserClaim userClaims`, instantiated `new ProductRepository()`, `new OrganizationRepository(userClaims)`, `new ProductInternalSettingRepository()` directly, and called `base(userClaims)` for `ManageProductBase`.

---

## 4. Cache

| Data | TTL | Key pattern |
|---|---|---|
| Deactivated user batch (property list for disabled users) | 600 s (10 min) | `l2l_deact_{userPersonaId}` |

> Roles and properties come directly from the L2L API per call — no caching required as the API is the system of record.

---

## 5. New Private Helpers

| Helper | Replaces | Notes |
|---|---|---|
| `GetApiSettingsAsync(ct)` | Constructor-time `GetProductSetting` blocking load | `ValueTask`-based; cached in `_apiSettings` field for the scope lifetime |
| `GetCallContextAsync(editorId, userId, ct)` | `ManageProductBase.GetCompanyEditorAndUserDetails` | Reads `USERID` + `PRODUCTUSERNAME` SAML attrs for editor + subject; returns `L2LCallContext` |
| `GetL2LCompanyAsync(orgRealPageId, ct)` | `ManageProductBase.GetProductCompanyInstanceId` | Calls `IManageBlueBookAsync.GetProductCompanyMappingAsync` with source `"L2L"`, uses `.FirstOrDefault()` |
| `GetRolesMainAsync(settings, ct)` | Inline API fetch + null check | Throws `BlueBookException` if API returns null or empty presets |
| `GetPropertyMainAsync(companyId, settings, ct)` | Inline API fetch | Returns `null` on failure (caller surfaces setup error) |
| `GetUserAsync(productUserId, settings, ct)` | Inline `GetUserInfo` HTTP call | Returns `null` if not found; caller checks |
| `GetApiAsync<T>(url, ct)` | `GetResultFromApi<T>()` | Per-request `HttpRequestMessage` with Basic auth; no shared state |
| `PutApiAsync(url, body, ct)` | `PutApi()` | JSON-serialises body via `System.Text.Json`; per-request auth |
| `EnableDisableUserAsync(userId, isActive, settings, ct)` | Inline enable/disable API call | Returns error string or empty on success |
| `BuildPropertyListAsync(propList, allProps, isSuperUser, ...)` | Inline property enrichment loop | Async — calls `_manageOneSite` per property to resolve `PMSystemID` |
| `BuildPermissionsListAsync(props, roleList, isSuperUser, roleInfo, ct)` | Inline permissions cross-product | Combines property × role pairs; super-user gets all admin rights |
| `CreateL2LUserAsync(l2lUser, ..., activityLog, ct)` | Inner `if (!hasUser)` block in `ManageLead2LeaseUser` | Extracted: POST to API, create SAML attrs, update SAML username, emit audit log |
| `UpdateL2LUserAsync(l2lUser, ..., activityLog, ct)` | Inner `else` block in `ManageLead2LeaseUser` | Extracted: PUT to API, update SAML username, emit diff-based audit log |
| `UpdateSamlUsernameAsync(loginName, newEmail, ct)` | Inline attribute read + update | Reads current `USERNAME` attr, updates only if changed |
| `GetEmailAddressAsync(realPageId, loginName, ct)` | Inline `GetElectronicAddress` call | Returns primary email or falls back to login name |
| `GetDeactivatedBatchDataAsync(userPersonaId, ct)` | `RPObjectCache`-backed sync call | `IMemoryCache` with 600 s TTL; used in `GetPropertiesAsync` to show previously assigned props for disabled users |
| `IsSuperUserAsync(personaId, ct)` | `ManageProductBase.IsSuperUser` | Calls `IManagePartyRelationshipAsync.GetPartyRelationshipAsync` — checks `"SuperUser"` role type |
| `IsRegularUserNoEmailAsync(personaId, ct)` | `ManageProductBase.IsRegularUser` | Checks `"USER (NO EMAIL)"` role type |
| `ExtractActivityDetailLogs(before, after, roleInfo, allProps)` | Inline diff logic in sync `ManageLead2LeaseUser` | Static — pure diff: compares role and property lists before/after, returns `AdditionalParameters` records |
| `ValidateAndReturnEmailAddress(email)` | Inline email regex check | Static — `EmailAddressAttribute.IsValid` + `MailAddress` parse; returns empty string on invalid |

---

## 6. Public Method Changes

| Method | Change |
|---|---|
| `GetRolesAsync` | `DefaultUserClaim` removed; all I/O awaited; calls `GetCallContextAsync` → `GetRolesMainAsync` → `GetUserAsync` |
| `GetPropertiesAsync` | `DefaultUserClaim` removed; awaited; deactivated-batch fallback via `IMemoryCache` |
| `ManageLead2LeaseUserAsync` | **Signature change** — `out List<AdditionalParameters>` removed; returns `(string Error, List<AdditionalParameters> ActivityLog)` tuple; full async port of ~400-line sync method; split into `CreateL2LUserAsync` / `UpdateL2LUserAsync` helpers |
| `UnassignUserAsync` | New method — previously inlined disable logic; calls `EnableDisableUserAsync` + updates `ProductBatchStatusType` |
| `UpdateLead2LeaseUserProfileAsync` | `DefaultUserClaim` removed; full async; handles `RegularUserNoEmail` branch for email / product login name resolution |
| `ChangeUserStatusAsync` | `DefaultUserClaim` removed; awaited; uses caller-supplied `productUserId` (migration path) |
| `GetMigrationUsersAsync` | `DefaultUserClaim` removed; awaited; query string built with `Uri.EscapeDataString` |
| `UpdateUsersMigrationStatusAsync` | `DefaultUserClaim` removed; awaited; `System.Text.Json` deserialisation of response |

---

## 7. Interface Changes (`IManageProductLead2LeaseAsync`)

All `DefaultUserClaim userClaim` parameters removed. `ManageLead2LeaseUserAsync` return type changed:

```csharp
// Before
Task<string> ManageLead2LeaseUserAsync(
    DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId,
    List<string> roleList, List<string> propertyList,
    out List<AdditionalParameters> additionalParameters);  // out — incompatible with async

// After
Task<(string Error, List<AdditionalParameters> ActivityLog)> ManageLead2LeaseUserAsync(
    long editorPersonaId, long userPersonaId,
    List<string> roleList, List<string> propertyList,
    CancellationToken cancellationToken = default);
```

`UnassignUserAsync` and `UpdateLead2LeaseUserProfileAsync` added as first-class interface members (were previously inlined in callers).

---

## 8. Side-Effect Changes to Dependent Files

### `LogicAsync/Interfaces/IManageProductOneSiteAsync.cs`

Two new cross-product methods added for Lead2Lease's `PMSystemID` resolution:

```csharp
Task<OneSiteUser?> GetOneSiteUserInfoAsync(
    string systemIdentifier, CancellationToken ct = default);

Task<bool> UserInLeasingAgentListAsync(
    string systemIdentifier, int siteId, CancellationToken ct = default);
```

### `LogicAsync/ManageProductOneSiteAsync.cs`

- `GetOneSiteUserInfoAsync` promoted from `private` to `public` and declared on the interface.
- `UserInLeasingAgentListAsync` added as a new public method — offloads the sync `_service.GetUserInLeasingAgentList` to `Task.Run` to avoid blocking the request thread.

---

## 9. What Was Intentionally Not Changed

- **Inner model classes** (`Lead2LeaseUser`, `Property`, `Permission`, `Preset`, `RoleInfo`, `MigrationUser`, etc.) remain in the sync source file. They are data-only types with no sync I/O and need no migration.
- **Business logic** — all role/property assignment rules, super-user branching, migration filtering, activity-log diffing — is a line-for-line port. No logic changes.
- **SAML attribute names** (`USERID`, `PRODUCTUSERNAME`, `USERNAME`) are left as string literals — they match the values used by the sync class and the SAML provider.

---

## 10. DI Registration Required

Add to `AddLogicAsyncServices` in `UnifiedLogin.Core/BusinessLogicExtensions.cs`:

```csharp
services.AddScoped<IManageProductLead2LeaseAsync, ManageProductLead2LeaseAsync>();
```

> `IHttpClientFactory` is registered via `services.AddHttpClient()`.  
> `IManageProductOneSiteAsync` must already be registered (it was a pre-existing dependency).  
> All other interface dependencies must be registered — see the broader deferred DI registration task.

---

## 11. Build Result

```
dotnet build UnifiedLogin.BusinessLogic.csproj
Build succeeded.
  3342 Warning(s)   ← pre-existing CS8xxx nullable / CA2200 / CA2017 warnings in legacy files
  0 Error(s)
```
