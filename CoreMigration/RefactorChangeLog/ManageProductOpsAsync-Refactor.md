# ManageProductOpsAsync Refactor Changelog

**Date:** 2026-04-13
**Branch:** feature/UserRepositoryRefactor

---

## Overview

This refactor transforms `ManageProductOps.cs` (~1 737 lines, two constructors, `DefaultUserClaim` bound, synchronous REST calls) into `ManageProductOpsAsync.cs` (~1 720 lines), a fully async, DI-first implementation of the Ops (Spend Management) product user-management service.

The refactor covers two phases:

**Phase 1 (prior session):** Async/await, IDbConnectionFactory-backed repository pattern, `IHttpClientFactory`, `ICacheService` session caching, `ILogger` structured logging, `out` parameter → tuple return, mutable instance state eliminated.

**Phase 2 (this session):** .NET 10 idioms — `dynamic` eliminated in favour of `System.Text.Json.JsonDocument`, `Encoding.Default` replaced with `Encoding.UTF8`, all empty-collection initialisers converted to C# 12 collection expressions.

---

## Constructor Expansion

### Legacy (two constructors)

```csharp
// Production ctor — newed up services internally, accepted DefaultUserClaim
public ManageProductOps(DefaultUserClaim userClaims) : base(...) { ... }

// Test/override ctor — accepted externally-created HttpClient
public ManageProductOps(Guid, DefaultUserClaim, HttpMessageHandler, HttpClient,
    IProductInternalSettingRepository, IManagePersona, ISamlRepository,
    IManageBlueBook, IProductRepository, IRepository) : base(...) { ... }
```

Both constructors populated mutable instance fields (`_opsBuyerUrl`, `_currentSessionId`, `_moduleAssetGroups`, `_userClaims`, etc.) during construction.

### Refactored (single DI constructor, 12 deps)

| Dependency | Replaces |
|---|---|
| `IProductContextServiceAsync` | `GetCompanyEditorAndUserDetails` / `DefaultUserClaim` |
| `IProductSettingServiceAsync` | `GetProductSettings` / `GetProductInternalSettings` |
| `IManagePersonaAsync` | `ManagePersona.GetPersona(...)` |
| `IManagePersonAsync` | `ManagePerson.GetPerson(...)` |
| `IManageContactMechanismAsync` | `new ManageContactMechanism()` per call |
| `IManageUserLoginAsync` | `ManageUserLogin.GetUserLoginOnly(...)` |
| `ISamlRepositoryAsync` | `_samlRepository.CreateSamlUserAttribute(...)` |
| `IUserLoginPersonaRepositoryAsync` | `_userLoginPersonaRepository.ListUserLoginPersona(...)` |
| `IUserRepositoryAsync` | `_userRepository.GetUserEmployeeId(...)` |
| `IHttpClientFactory` | `new HttpClient()` per call + shared `_client` field |
| `ICacheService` | `MemoryCache.Default` (System.Runtime.Caching) for SID |
| `ILogger<ManageProductOpsAsync>` | `WriteToDiagnosticLog` / `WriteToErrorLog` |

All guards use `ArgumentNullException.ThrowIfNull` (.NET 6+).

---

## Mutable State Eliminated

| Legacy mutable field | Replaced by |
|---|---|
| `_opsBuyerUrl` | `OpsSession.BaseUrl` (immutable per-call, from `IProductSettingServiceAsync`) |
| `_currentSessionId` | `OpsSession.Sid` (immutable per-call; cached in `ICacheService`) |
| `_moduleAssetGroups` | `GetModuleAssetGroupsConfigAsync` result (local, per-call) |
| `_userClaims` | Removed; context resolved via `IProductContextServiceAsync` |
| `_client` (shared HttpClient) | `IHttpClientFactory.CreateClient()` per logical request |

---

## `out` Parameter Removed

The legacy `ManageOpsUser(long, long, List<int>, List<int>, out List<AdditionalParameters>)` used an `out` parameter which is incompatible with `async`. Replaced by:

```csharp
Task<(string error, List<AdditionalParameters> additionalParameters)> ManageOpsUserAsync(...)
```

---

## `OpsSession` Value Record

```csharp
private readonly record struct OpsSession(string BaseUrl, string Sid, string LoginName);
```

Encapsulates the three per-call session values. Replaces `_opsBuyerUrl`, `_currentSessionId`, and the `loginName` state needed for cache invalidation on 401.

---

## Session Management: `MemoryCache.Default` → `ICacheService`

The legacy `GetOpsSessionGuid()` used `MemoryCache.Default["opsSid_" + loginName]` with a `CacheItemPolicy` (90-minute TTL). The refactored `GetOpsSessionAsync` + `AcquireOpsSessionSidAsync` pair uses:

```csharp
string? sid = await _cache.GetOrSetAsync<string>(
    cacheKey,
    token => AcquireOpsSessionSidAsync(settings, loginName, apiKey, baseUrl, token),
    new CacheEntryOptions { ExpirationTimeInMinutes = SidCacheMinutes, SkipDistributedCache = true },
    ct);
```

The cache key `opsSid_{loginName}` is identical to the legacy key to allow shared SID reuse during any rolling migration window.

---

## HTTP Helpers

| Helper | Purpose |
|---|---|
| `OpsGetWithRetryAsync` | Authenticated GET with automatic 401 session-refresh and up to `MaxRetryCount` retries; replaces the legacy `GetAsync` blocking loop |
| `OpsRequest` | Builds `HttpRequestMessage` with `sid` header and optional JSON body |
| `CreateClient` | Creates `HttpClient` from `IHttpClientFactory` with 60 s timeout |
| `PatchUserInfoAsync` | Sends PATCH to `/api/v1.0/users/{userId}`; returns empty string on success |
| `OpsGetUserByIdAsync` | GET user by numeric ID; returns `null` on 404 |
| `OpsLoginNameInUseAsync` | GET user by login name; returns `true` if found |
| `GetModuleAssetGroupsConfigAsync` | GET `/api/v1.0/company/configs?config_name=module_asset_groups`; returns `MODULE_ASSET_GROUPS` flag |
| `GetAllRolesAsync` | GET `/api/v1.0/roles`; returns raw `Role` list |
| `GetRolesInternalAsync` | Resolves role list scoped to optional `assetCode`, marks assigned role |
| `GetCompanyAssetDetailsAsync` | Resolves asset type (Portfolio/AssetGroups), marks assigned asset; replaces legacy private `GetCompanyAssetDetails` |
| `EnableComplianceRights` | Static helper: forces `isAssigned = true` for all Compliance Setup rights |
| `ValidateAndReturnEmailAddress` | Static helper: validates email via `System.Net.Mail.MailAddress` |
| `SettingValue` | Static helper: looks up `ProductInternalSetting` by name |

---

## .NET 10 Improvements (Phase 2)

### `dynamic` Eliminated → `System.Text.Json.JsonDocument`

All six `dynamic?` usages backed by `JsonConvert.DeserializeObject<dynamic>(...)` (Newtonsoft `JObject` under the hood) have been replaced with `JsonDocument.Parse`:

| Location | Legacy | Refactored |
|---|---|---|
| `GetModuleAssetGroupsConfigAsync` | `config?.MODULE_ASSET_GROUPS` | `configDoc.RootElement.TryGetProperty("MODULE_ASSET_GROUPS", out var el) ? el.GetInt16() : (short)0` |
| `AcquireOpsSessionSidAsync` | `result?.session?.sid` | `sessionDoc.RootElement.TryGetProperty("session", out var s) && s.TryGetProperty("sid", out var sid) ? sid.GetString() : null` |
| `OpsGetUserByIdAsync` | `result.id`, `result.first_name`, etc. | `userDoc.RootElement.TryGetProperty(...)` per field; local `GetProp` closure |
| `OpsLoginNameInUseAsync` | `user?.login_name` | `loginDoc.RootElement.TryGetProperty("login_name", ...)` |
| `ManageOpsUserAsync` (create) | `userResult?.id`, `userResult?.Value<string>("login_name")` | `createDoc.RootElement.TryGetProperty("id", ...)`, `TryGetProperty("login_name", ...)` |
| `ManageOpsUserAsync` (update) | `userResult?.id` | `updateDoc.RootElement.TryGetProperty("id", ...)` |

`using System.Text.Json;` added. Newtonsoft.Json is retained for typed `JsonConvert.DeserializeObject<T>` / `JsonConvert.SerializeObject` calls against SharedObjects types (`OpsUser`, `OpsUsers`, `AssetGroup`, `Portfolio`, `Role`, `RightGroup`, `RightGroupRole`, `MigrateResponse`, etc.) which carry `[JsonProperty]` Newtonsoft attributes that cannot be changed without cross-project impact.

### `dynamic newRole` → `var newRole`

The anonymous-type role body in `CreateRoleAsync` was declared `dynamic`. Since no dynamic dispatch is used (it is immediately serialised and passed as `object`), the keyword is replaced with `var`:

```csharp
// Before
dynamic newRole = new { name = ..., description = ..., ... };
// After
var newRole = new { name = ..., description = ..., ... };
```

### `Encoding.Default` → `Encoding.UTF8`

All four `Encoding.Default` usages replaced with `Encoding.UTF8`:

| Location | Change |
|---|---|
| `AcquireOpsSessionSidAsync` — MD5 hash input | `Encoding.Default.GetBytes(...)` → `Encoding.UTF8.GetBytes(...)` |
| `AcquireOpsSessionSidAsync` — session POST body | `new StringContent(..., Encoding.Default, ...)` → `Encoding.UTF8` |
| `OpsRequest` helper — all request bodies | `new StringContent(..., Encoding.Default, ...)` → `Encoding.UTF8` |
| `UpdateUsersMigrationStatusAsync` — PATCH body | `new StringContent(..., Encoding.Default, ...)` → `Encoding.UTF8` |

`Encoding.Default` is platform-dependent (Windows-1252 on Windows) and can corrupt non-ASCII characters. `Encoding.UTF8` is the correct encoding for JSON payloads.

### Collection Expressions `[]`

All empty-collection initialisers and several single-element collection constructors replaced with C# 12 collection expressions:

| Before | After |
|---|---|
| `var additionalParameters = new List<AdditionalParameters>()` | `List<AdditionalParameters> additionalParameters = []` |
| `var rightsToAdd = new List<string>()` | `List<string> rightsToAdd = []` |
| `var rightsToRemove = new List<string>()` | `List<string> rightsToRemove = []` |
| `var rightsInput = new List<object>()` | `List<object> rightsInput = []` |
| `IList<Role> emptyRoles = new List<Role>()` | `IList<Role> emptyRoles = []` |
| `IList<AssetGroup> assetGroupList = new List<AssetGroup>()` (×2) | `IList<AssetGroup> assetGroupList = []` |
| `IList<AssetGroupPatch> patchList = new List<AssetGroupPatch> { assetGroup }` | `IList<AssetGroupPatch> patchList = [assetGroup]` |
| `new List<AssetGroup> { single }` | `[single]` |
| `?? new List<Role>()` fallbacks (×2) | `?? []` |
| `?? new List<AssetGroup>()` fallbacks (×3) | `?? []` |
| `?? new List<Portfolio>()` fallbacks (×2) | `?? []` |
| `?? new List<ProductRole>()` fallbacks | `?? []` |

---

## Already Present (Phase 1 Improvements)

The following improvements were applied in Phase 1 and are documented here for completeness:

- **`Math.Min` for page-size clamp**: `datafilter.Pages.ResultsPerPage <= 100 ? ... : 100` → `Math.Min(datafilter.Pages.ResultsPerPage, 100)`.
- **`is not null` pattern matching**: Throughout instead of `!= null`.
- **`is { Length: > 0 }` patterns**: For null-and-empty combined checks.
- **`StringComparison.OrdinalIgnoreCase`**: All string equality comparisons.
- **`ManageUnifiedLogin.PushToQueue` removed**: `UpdateRightsToRoleLogMessage` and `updateRoleLogMessage` (which called `PushToQueue` via sync `DefaultUserClaim`-bound `ManageUnifiedLogin`) are marked TODO in `CreateRoleAsync` pending `IManageUnifiedLoginAsync.PushToQueueAsync` availability on the interface.
- **`IsSuperUserAsync`** via `IProductContextServiceAsync.IsSuperUserAsync` replaces private `IsSuperUser(personaId)` backed by `ManagePartyRelationship`.
- **`_settingService.UpdateProductStatusAsync`** replaces `UpdateProductSettingProductStatus` backed by `DefaultUserClaim`.

---

## Removed / Not Ported

- **`ManageProductBase` base class**: Eliminated. No inheritance. All previously-inherited methods are resolved via injected services or private helpers.
- **`MemoryCache.Default` (System.Runtime.Caching)**: Replaced by `ICacheService`.
- **`WriteToDiagnosticLog` / `WriteToErrorLog`**: Replaced by `ILogger<ManageProductOpsAsync>` structured logging.
- **`DefaultUserClaim` constructor parameter**: Removed from all method signatures.
- **Legacy test constructor**: Eliminated. `IHttpClientFactory` and `ICacheService` support test doubles without a separate constructor.
- **`DoAdditional` override**: Not ported — it was a `ManageProductBase` hook that called `GetOpsSessionGuid()`. Session management is now entirely internal to `GetOpsSessionAsync`.
- **`new ManageUnifiedLogin(_userClaims)`**: All occurrences removed. Audit data returned as `List<AdditionalParameters>` in tuple returns for consumption by the calling service layer.
- **`new ManageContactMechanism()` per call**: Replaced by `IManageContactMechanismAsync` injection.
- **`new UserRepository(_userClaims)`** in `UpdateOPSUserProfileAsync`: Replaced by `IUserRepositoryAsync.GetUserDetailsAsync(personaId, ct)`.

---

## Files Changed

- `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductOpsAsync.cs` — interface with 15 method signatures across `Asset Groups`, `Roles and Rights`, `User Management`, `Migration` sections; `out` parameter replaced by tuple return on `ManageOpsUserAsync`
- `UnifiedLogin.BusinessLogic/LogicAsync/ManageProductOpsAsync.cs` — ~1 720 lines; single 12-dep DI constructor; 15 public methods; 11 private helpers; `OpsSession` value record; `using System.Text.Json` added; all `dynamic` removed; all `Encoding.Default` → `Encoding.UTF8`; all empty collections → `[]`
