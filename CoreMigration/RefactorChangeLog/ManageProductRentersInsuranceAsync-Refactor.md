# Refactor Change Document — `ManageProductRentersInsuranceAsync`

**Source:** `Logic/Product/ManageProductRentersInsurance.cs` (.NET 4.8 sync class, ~1,270 lines)  
**Target:** `LogicAsync/Product/ManageProductRentersInsuranceAsync.cs` (.NET 10 / C# 13)  
**Interface:** `LogicAsync/Interfaces/IManageProductRentersInsuranceAsync.cs` (overwritten stub)  
**Product:** Renters Insurance — `ProductEnum.Insurance`, source code `BlueBookProductConstants.Insurance`

---

## 1. What Was Replaced

| Legacy | Replacement | Reason |
|---|---|---|
| `ManageProductBase` inheritance | Eliminated — all helpers ported as private async methods | Base class used mutable instance fields set by sync callers; incompatible with DI and true async |
| `new InsuranceService(_url, _user, _pass)` at construction time | `IInsuranceService` injected via DI | Testable; credentials loaded lazily from DB on first call |
| Constructor-time blocking `_productInternalSettingList` load | `GetInsuranceConfigAsync` — cached in `IMemoryCache` (1-hour TTL, key `RI_ProductConfig`) | Defers I/O until first use; construction is allocation-only |
| `RPObjectCache` / `MemoryCache.Default` | `IMemoryCache` injected | Native .NET cache; no static singleton |
| `WriteToDiagnosticLog` / `WriteToErrorLog` | `ILogger<ManageProductRentersInsuranceAsync>` | Structured logging via MEL |
| `DefaultUserClaim userClaim` on every method | `IProductContextServiceAsync` injected | Claims resolved once per DI scope; callers pass only persona IDs |
| `out List<AdditionalParameters>` on `ManageRentersInsuranceUser` / `ChangeRentersInsuranceUserType` | Tuple returns `(result, auditParams)` | `out` is incompatible with `async` |
| `ManageProductBase.GetCompanyEditorAndUserDetails` (mutable side-effects) | `IProductContextServiceAsync.GetUserContextAsync` → immutable `ProductCallContext` | Thread-safe; no shared mutable state between concurrent requests |
| `GetProductCompanyInstanceId(_udmSourceCode)` | `IManageBlueBookAsync.GetProductCompanyMappingAsync(orgRealPageId, source, ct)` | Async; no hidden sync fallback |
| `_blueBook.GetCompanyPropertyInstance(id)` (sync) | `await _manageBlueBook.GetCompanyPropertyInstanceAsync(id, ct)` | True async |
| `_blueBook.GetPropertyInstance(id)` (sync) | `await _manageBlueBook.GetPropertyInstanceAsync(id, ct)` | True async |
| `_insuranceService.GetListPropertyByPMCID(id)` and `_blueBook.GetPropertyInstance(id)` called sequentially | Both awaited with `Task.WhenAll` in `ListPropertiesByPMCIDAsync` | Parallel I/O — eliminates sequential wait |
| `new ManageElectronicAddress().ListElectronicAddressForPerson(...)` constructed per-call | `IManageElectronicAddressAsync.ListElectronicAddressForPersonAsync(realPageId, "EMAIL", ct)` | DI-injected; no per-call allocation of sync class |
| `IsRegularUserNoEmail(userPersonaId)` (sync, base-class) | `await _contextService.IsRegularUserNoEmailAsync(userPersona, ct)` | Consistent with other async product managers |
| `ValidateAndReturnEmailAddress(loginName)` (sync, base-class) | Inline `new EmailAddressAttribute().IsValid(loginName) ? loginName : string.Empty` | Eliminates sync base dependency; identical behaviour |
| `UpdateSamlUserAttributes(userPersonaId, dict)` (sync, base-class) | `await _samlService.UpsertAttributesAsync(userPersonaId, ProductId, dict, ct)` | True async SAML upsert |
| `UpdateProductSettingProductStatus(...)` (sync, base-class) | `await _productRepository.UpdateProductSettingProductStatusAsync(...)` | True async product-status write |
| Role name mapping via `switch` statement with `ForEach` mutation | `FrozenDictionary<int, string> RoleDisplayNames` | O(1) lookup; zero allocation; immutable |
| `person.FirstName.Substring(0, Math.Min(x.Length, 50))` inline duplicated in two places | `private static string Truncate(string? s, int max)` helper | DRY; null-safe |
| `email.Substring(0, Math.Min(email.Length, 155))` inline | `emailAddress[..Math.Min(emailAddress.Length, 155)]` (C# range operator) | Same semantic; more idiomatic |
| Enable/Disable/Unlock copy-paste blocks | `SimpleUserActionAsync` private helper | Eliminates ~150 lines of structural duplication |
| `new List<>()` / `new Dictionary<>()` initializers | Collection expressions `[]` | C# 13 idiomatic |
| `IInsuranceService` credentials accessed directly from constructor fields | `InsuranceConfig` file-scoped record cached in `IMemoryCache` | Consistent with other async product managers; credentials are never stale for >1 hour |

---

## 2. Architecture Changes

### 2a. `DefaultUserClaim` Removed from Interface

All interface methods that previously held `DefaultUserClaim userClaim` as their first argument now omit it entirely.  User context flows via the injected `IProductContextServiceAsync`:

```csharp
// Before (stub)
Task<ListResponse> ListPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, ...);

// After (true-async interface)
Task<ListResponse> ListPropertiesAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken ct = default);
```

### 2b. `out` Parameters Replaced with Tuple Returns

`out List<AdditionalParameters>` on `ManageRentersInsuranceUser` and `ChangeRentersInsuranceUserType` are replaced with tuple returns — `out` is incompatible with `async`:

```csharp
// Before
ObjectOutput<UserAPIResponse, IErrorData> ManageRentersInsuranceUser(
    ..., out List<AdditionalParameters> additionalParameters, ...);

// After
Task<(ObjectOutput<UserAPIResponse, IErrorData> result, List<AdditionalParameters> auditParams)>
    ManageRentersInsuranceUserAsync(...);
```

### 2c. Internal Record for Cached Config

```csharp
// internal to the assembly — not part of the public API
internal sealed record InsuranceConfig(
    string ApiEndpoint,
    string Username,
    string Password,
    int    RequestedBy);
```

> **Note:** Originally declared as `file sealed record` (C# 11 file-local type), but the C# compiler (CS9051) prevents file-local types from appearing in method signatures of non-file-local types — even in private methods. Changed to `internal sealed record` which achieves the same "not public API" intent while satisfying the compiler.

### 2d. `FrozenDictionary` for Role Display-Name Overrides

```csharp
private static readonly FrozenDictionary<int, string> RoleDisplayNames =
    new Dictionary<int, string>
    {
        [2]  = "Corporate User",
        [21] = "Corporate User with RPX",
        [22] = "Property Manager with RPX"
    }.ToFrozenDictionary();
```

Replaces the `switch` statement inside a `ForEach` mutation loop.  O(1) lookup; evaluated once at class load time.

### 2e. Parallel I/O in `ListPropertiesByPMCIDAsync`

```csharp
// Both BlueBook and insurance API calls fire concurrently
var bbPropertiesTask = _manageBlueBook.GetPropertyInstanceAsync(companyInstanceId, ct);
var riPropertiesTask = _insuranceService.GetListPropertyByPMCIDAsync((int)companyInstanceId);

await Task.WhenAll(bbPropertiesTask, riPropertiesTask);
```

### 2f. `SimpleUserActionAsync` Shared Helper

Enable, Disable, and Unlock all follow the same pattern (resolve context → parse product user ID → call API → return `ObjectOutput`).  Extracted into one private helper to eliminate ~150 lines of structural duplication.

---

## 3. Behaviour Preserved

| Behaviour | Location |
|---|---|
| Username uniqueness loop (up to 10 retries; `local1@domain`, `local2@domain` …) | `ManageRentersInsuranceUserAsync` — `CheckIfUserLoginIsUsedAsync` called in `while` loop |
| `ErrorCode == "-1"` means "login in use" | `CheckIfUserLoginIsUsedAsync` private helper |
| Email truncated to 155 characters | `userEmailAddress[..Math.Min(len, 155)]` |
| FirstName/LastName truncated to 50 characters | `Truncate(string? s, int max)` static helper |
| `IsRegularUserNoEmail` path: use login name + fetch EMAIL electronic address | `await _contextService.IsRegularUserNoEmailAsync` → `await _manageElectronicAddress.ListElectronicAddressForPersonAsync` |
| `ProfileUpdate` batch type re-fetches current role from API | `batchProcessType == BatchProcessType.ProfileUpdate` branch in `ManageRentersInsuranceUserAsync` |
| `ProfileUpdate` retains existing property list | `case BatchProcessType.ProfileUpdate when getUserByIDResponse?.UserInfo?.PropertyList is not null` |
| All-properties ("ALL" / empty list) assigns all BlueBook properties | Default branch in property resolution `switch` |
| `UserTypeRegularToAdmin` / `UserTypeExternalToAdmin` assign all properties | Explicit `case` in property resolution `switch` |
| `UserTypeAdminToRegular` / `UserTypeAdminToExternal` assign selected properties | Explicit `case` in property resolution `switch` |
| `PasswordGenerator.GeneratePassword(20, 5)` for new users | `ManageRentersInsuranceUserAsync` create path |
| No password on update (`userInfo.Password = null`) | `ManageRentersInsuranceUserAsync` update path |
| SAML upsert after successful create/update | `await _samlService.UpsertAttributesAsync(userPersonaId, ProductId, { productUsername, UserId }, ct)` |
| `ProductBatchStatusType.Success` after successful create/update | `await _productRepository.UpdateProductSettingProductStatusAsync(...)` |
| `ProductBatchStatusType.Deleted` before Disable in Unassign | `UnassignRentersInsuranceUserAsync` — status written before API call |
| Activity log diff (role delta + property added/removed) | Best-effort `try/catch` block in `ManageRentersInsuranceUserAsync` — never fails the main operation |
| `BlueBookException` returns its `.Message` as `ErrorReason` | `catch (Exception ex) when (ex is BlueBookException)` first, then generic handler |

---

## 4. Compile Fixes Applied

| Error | File | Fix Applied |
|---|---|---|
| `CS0246: PropertyInstance not found` | `IManageProductRentersInsuranceAsync.cs` | Added `using UnifiedLogin.SharedObjects.BlackBook;` |
| `CS0535: ManageProductRentersInsuranceAsync does not implement interface member *` (12 members) | `LogicAsync/ManageProductRentersInsuranceAsync.cs` (old stub) | Replaced old stub with empty placeholder — implementation lives in `LogicAsync/Product/ManageProductRentersInsuranceAsync.cs` |
| `CS9051: File-local type InsuranceConfig cannot be used in member signature of non-file-local type` | `LogicAsync/Product/ManageProductRentersInsuranceAsync.cs` | Changed `file sealed record InsuranceConfig` → `internal sealed record InsuranceConfig` |
| `CS0738: does not implement ListPropertiesByPMCIDAsync — return type mismatch` | `LogicAsync/ManageProductRentersInsuranceAsync.cs` (old stub) | Resolved by removal of old stub (same root cause as CS0535 above) |

**Final build result: 0 errors, 8 warnings (all NuGet NU1701/NU1507/NU1903 compatibility warnings, not code issues)**

---

## 5. Known TODOs

| Item | Tracking note |
|---|---|
| `WriteUpdateUserTypeActivityLog` for `UserType*` batch processes | Pending `IProductAuditServiceAsync` — see commented block in `ManageRentersInsuranceUserAsync` |

---

## 5. Dependency Injection Registration Required

```csharp
services.AddScoped<IManageProductRentersInsuranceAsync, ManageProductRentersInsuranceAsync>();
```

All other dependencies (`IProductContextServiceAsync`, `IProductRepositoryAsync`, `ISamlAttributeServiceAsync`, `IManageBlueBookAsync`, `IManagePersonaAsync`, `IManagePersonAsync`, `IManageUserLoginAsync`, `IManageElectronicAddressAsync`, `IMemoryCache`, `ILogger<T>`) are expected to be registered by the host application's service-registration bootstrapping code.

`IInsuranceService` (WCF-generated proxy) must be registered as `Scoped` or `Transient`; **do not register as Singleton** since the WCF proxy holds per-call state.
