# ManageProductOnSiteAsync Refactor Changelog

**Date:** 2026-04-13
**Branch:** feature/UserRepositoryRefactor

---

## Overview

This refactor creates `ManageProductOnSiteAsync` and `IManageProductOnSiteAsync` as the true-async replacements for the legacy synchronous `ManageProductOnSite.cs` (~1 749 lines). The On-Site product (**no 'e'**) is a **REST/OAuth2** product (On-Site.com property management SaaS) — it is entirely distinct from `ManageProductOneSite.cs` which is a Yardi SOAP product.

The rewrite adopts the `IDbConnectionFactory`-backed repository pattern established throughout the async layer: all external dependencies are injected via a single constructor (11 deps vs the legacy two-constructor pattern that accepted a `DefaultUserClaim`), mutable per-call state is eliminated via immutable sealed records, and .NET 10 idioms are applied throughout.

---

## Legacy Constructor Pattern → Single DI Constructor

### Legacy (two constructors)

```csharp
// Production constructor — took DefaultUserClaim + newed up services internally
public ManageProductOnSite(DefaultUserClaim userClaim) { ... }

// Test/override constructor
public ManageProductOnSite(DefaultUserClaim userClaim, HttpClient client) { ... }
```

Both constructors initialised mutable instance fields (`_companyName`, `_productUserId`, `_productUsername`, `_client`, `_token`, `_apiEndPoint`, `_apiSecret`, `_clientId`, `_tokenEndPoint`, `_companyInstanceSourceId`) populated during the first method call.

### Refactored (single DI constructor, 11 deps)

| Dependency | Replaces |
|---|---|
| `IProductContextServiceAsync` | `GetCompanyEditorAndUserDetails` / `DefaultUserClaim` |
| `IProductSettingServiceAsync` | `GetProductSettings` / `GetProductInternalSettings` |
| `IManageBlueBookAsync` | Direct `ManageBlueBook` instantiation for company instance ID |
| `IHttpClientFactory` | `new HttpClient()` per call + shared `_client` field |
| `IMemoryCache` | `MemoryCache.Default` (System.Runtime.Caching) |
| `ILogger<ManageProductOnSiteAsync>` | `WriteToDiagnosticLog` / `WriteToErrorLog` |
| `ISamlRepositoryAsync` | `SamlRepository` for `CreateSamlUserAttributeAsync` |
| `IManagePersonaAsync` | `ManagePersona.ListActivePersona` |
| `IManagePersonAsync` | `ManagePerson.GetPerson` |
| `IManageUserLoginAsync` | `ManageUserLogin.GetUserLoginOnly` |
| `IManageElectronicAddressAsync` | `ManageElectronicAddress.GetElectronicAddress` |

All guards use `ArgumentNullException.ThrowIfNull` (.NET 6+). The test constructor accepting an `HttpClient` override is eliminated — `IHttpClientFactory` handles both production and test scenarios.

---

## New Private Context Records

### `OnSiteCtx`

```csharp
private sealed record OnSiteCtx(
    Persona EditorPersona,
    string  ProductUserId,
    string  ProductUsername,
    string  AccessToken,
    string  ApiEndPoint,
    int     CompanyInstanceSourceId);
```

Replaces the seven mutable instance fields that `GetToken()` and `GetCompanyEditorAndUserDetails` populated (`_token`, `_apiEndPoint`, `_productUserId`, `_productUsername`, `_companyInstanceSourceId`, etc.). One `OnSiteCtx` is resolved per public call via `GetOnSiteContextAsync` and flows immutably through the call chain. `ChangeUserStatusAsync` uses C# record `with` expressions (`ctx with { ProductUserId = productUserId }`) to override the product user ID without mutating shared state.

### `OnSiteApiConfig`

```csharp
private sealed record OnSiteApiConfig(
    string ApiEndPoint,
    string ApiSecret,
    string ClientId,
    string TokenEndPoint);
```

Encapsulates the four API credential settings loaded from `IProductSettingServiceAsync`. Replaces the four mutable instance fields (`_apiEndPoint`, `_apiSecret`, `_clientId`, `_tokenEndPoint`).

---

## Public Methods (11 total — matching legacy surface area with modernised signatures)

| Method | Returns | Notes |
|---|---|---|
| `GetPropertiesAsync` | `Task<ListResponse>` | Returns the company property list from the On-Site REST API; delegates to `GetResultFromApiAsync<List<OnSitePropertyModel>>` |
| `GetRegionsAsync` | `Task<ListResponse>` | Returns the company region list from the On-Site REST API |
| `GetRolesAsync` | `Task<ListResponse>` | Returns the company role list from the On-Site REST API |
| `GetUsersAsync` | `Task<ListResponse>` | Returns On-Site users, paged and filtered by `datafilter`; calls `GetResultFromApiAsync<OnSiteApiUserWrapper>` |
| `GetMigrationUsersAsync` | `Task<ListResponse>` | Returns paged non-migrated users for the migration workflow |
| `UpdateUsersMigrationStatusAsync` | `Task<MigrateResponse>` | POSTs a batch of `MigrateUser` entries to the migration endpoint; uses `JsonDocument.Parse` to extract the `count` field |
| `ManageOnSiteUserAsync` | `Task<(string error, List<AdditionalParameters> auditParams)>` | Creates or updates an On-Site user; resolves persona, person, login, email; generates unique login via `while` loop; activates user before update; 30 s propagation delay; runs role/property/region audit diff |
| `ChangeOnSiteServiceUserTypeAsync` | `Task<(string error, List<AdditionalParameters> auditParams)>` | Pure delegation to `ManageOnSiteUserAsync` with the supplied `BatchProcessType` |
| `UpdateOnSiteUserProfileAsync` | `Task<string>` | Updates display name and email without touching roles or properties |
| `ChangeUserStatusAsync` | `Task<string>` | Activates or deactivates the On-Site user identified by `productUserId`; uses `ctx with { ProductUserId = productUserId }` |
| `UnassignUserAsync` | `Task<string>` | Deactivates the On-Site user and marks product status as `Deleted`; optionally deletes SAML product info |

### Signature changes from legacy

- **`ManageOnSiteUser`** — legacy accepted an `OnSiteUserPropertyRegionRole` DTO and had an `out List<AdditionalParameters>` parameter. Refactored to `ManageOnSiteUserAsync(long editorPersonaId, long userPersonaId, List<int> propertyList, List<int> regionList, List<int> roleList, BatchProcessType batchProcessType, CancellationToken ct)` returning `Task<(string error, List<AdditionalParameters> auditParams)>`. The DTO was eliminated to avoid cross-layer type coupling in the interface.
- **`DefaultUserClaim` removed** from all signatures. Per-call context resolved internally via `IProductContextServiceAsync`.

---

## Private Helpers Added (13)

| Helper | Purpose |
|---|---|
| `GetOnSiteContextAsync` | Resolves `OnSiteCtx` once per public call: loads API config, acquires token, resolves editor persona + product credentials, resolves company instance source ID |
| `GetApiConfigAsync` | Loads the four credential settings from `IProductSettingServiceAsync`; returns `OnSiteApiConfig` |
| `GetAccessTokenAsync` | Acquires OAuth2 Bearer token via `HttpClient`; caches result for 9 minutes in `IMemoryCache` with key `"OnSite_AccessToken_{editorPersonaId}"`; uses `JsonDocument.Parse` to extract `access_token` |
| `GetCompanyInstanceIdAsync` | Looks up the editor's company BlueBook source ID via `IManageBlueBookAsync` with `BlueBookProductConstants.OnSite = "ONST"` |
| `GetResultFromApiAsync<T>` | GET helper: sends authenticated request, deserialises JSON via STJ, returns `default(T)` on non-success; logs errors via `ILogger` |
| `ActivateDeactivateUserAsync` | PUT helper: sends activate/deactivate request to `{apiEndPoint}/users/{userId}/active`; returns error string |
| `InsertOnSiteUserAsync` | POST helper: creates a new On-Site user; returns `(string userId, string error)` tuple |
| `UpdateOnSiteUserAsync` | PUT helper: updates an existing On-Site user |
| `UpdateOnSiteUserProfileApiAsync` | PUT helper: updates only the user profile (name + email) |
| `GetOnSiteUserAsync` | GET helper: fetches a single On-Site user by login name; returns `null` on 404; used in login uniqueness loop |
| `CreateProductUserInGreenbookAsync` | Chains `IManagePersonaAsync` → `IManagePersonAsync` → `IManageUserLoginAsync` → `ISamlRepositoryAsync` to create persona, person, login, and SAML attributes |
| `ResolveEmailAddressAsync` | Resolves primary/fallback email via `IManageElectronicAddressAsync` |
| `TryReadContentAsync` | Safely reads `HttpResponseMessage.Content` as string; returns empty string on failure |

### Merge helpers (3)

| Helper | Purpose |
|---|---|
| `MergePropertiesWithAssignedAsync` | Fetches company property list + user's assigned properties in parallel; merges `IsAssigned` flags for audit diff |
| `MergeRegionsWithAssignedAsync` | Same pattern for regions |
| `MergeRolesWithAssignedAsync` | Same pattern for roles |

### Static / pure helpers

| Helper | Purpose |
|---|---|
| `GetUserCodeFromLogin` | Extracts the user code portion from a qualified login string (e.g. `"COMPANY\|USERID"` → `"USERID"`) |
| `MapUserRoles` | Converts `List<int> roleList` to `OnSiteRoleModel[]` for the insert/update API body |
| `MapUserPropertyAccess` | Converts `List<int> propertyList` + `List<int> regionList` + super-user flag to `OnSiteApiPropertyAccess`; `propertyList = [-1]` (super-user sentinel) maps to `CompanyIdList = [companyId]` + empty PropertyIdList/RegionIdList |

---

## Interface Additions (`IManageProductOnSiteAsync`)

New interface file created at `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductOnSiteAsync.cs`:

```csharp
Task<ListResponse> GetPropertiesAsync(long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default);
Task<ListResponse> GetRegionsAsync(long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default);
Task<ListResponse> GetRolesAsync(long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default);
Task<ListResponse> GetUsersAsync(long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default);
Task<ListResponse> GetMigrationUsersAsync(long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default);
Task<MigrateResponse> UpdateUsersMigrationStatusAsync(long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default);
Task<(string error, List<AdditionalParameters> auditParams)> ManageOnSiteUserAsync(long editorPersonaId, long userPersonaId, List<int> propertyList, List<int> regionList, List<int> roleList, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser, CancellationToken ct = default);
Task<(string error, List<AdditionalParameters> auditParams)> ChangeOnSiteServiceUserTypeAsync(long editorPersonaId, long userPersonaId, List<int> propertyList, List<int> regionList, List<int> roleList, BatchProcessType batchProcessType, CancellationToken ct = default);
Task<string> UpdateOnSiteUserProfileAsync(long editorPersonaId, long userPersonaId, CancellationToken ct = default);
Task<string> ChangeUserStatusAsync(long editorPersonaId, string productUserId, bool isActive, CancellationToken ct = default);
Task<string> UnassignUserAsync(long editorPersonaId, long userPersonaId, bool deleteSamlUserProductInfoAndStatus = false, CancellationToken ct = default);
```

All signatures carry `CancellationToken ct = default` as the last parameter and are grouped under XML doc-comment region headers.

---

## .NET 10 Improvements

- **`System.Text.Json` replaces `Newtonsoft.Json`**: All JSON serialisation uses `System.Text.Json` with a shared `private static readonly JsonSerializerOptions _jsonOptions` configured with `JsonNamingPolicy.SnakeCaseLower`, `PropertyNameCaseInsensitive = true`, and `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`. The `using Newtonsoft.Json` import is removed entirely.
- **`JsonNamingPolicy.SnakeCaseLower`** handles the majority of the API's snake_case field names (`property_id`, `property_name`, `region_id`, `region_name`, `company_id`, `first_name`, `last_name`, `email_address`, `is_active`, `company_id_list`, etc.) automatically without per-property `[JsonPropertyName]` attributes.
- **`[JsonPropertyName]` overrides**: Used only for fields where the JSON key diverges from the STJ snake_case convention — e.g. `[JsonPropertyName("active")]` for `IsActive` in `OnSitePropertyModel` (API returns `"active"` not `"is_active"`), and dual-name properties in `OnSitePropertyModel` / `OnSiteRegionModel` that alias `id`/`property_id` and `name`/`property_name` on the same backing field.
- **`JsonDocument.Parse`**: Replaces `JsonConvert.DeserializeObject<dynamic>` for single-field extractions — `access_token` in `GetAccessTokenAsync` and `count` in `UpdateUsersMigrationStatusAsync`.
- **`JsonContent.Create(body, options: _jsonOptions)`**: Used for HTTP POST/PUT bodies instead of `PostAsJsonAsync` with Newtonsoft, ensuring consistent STJ serialisation of the `_jsonOptions` snake_case policy.
- **`IHttpClientFactory` replaces `new HttpClient()`**: The legacy code created a new `HttpClient` per call in methods like `GetResultFromApi<T>` and cached one in `_client`. The refactored code injects `IHttpClientFactory` and calls `_httpClientFactory.CreateClient()` per logical request, which reuses pooled `HttpMessageHandler` instances and avoids socket exhaustion.
- **`IMemoryCache` replaces `MemoryCache.Default`**: Token caching moved from `System.Runtime.Caching.MemoryCache.Default` to `Microsoft.Extensions.Caching.Memory.IMemoryCache`, consistent with the rest of the async layer. Cache key: `"OnSite_AccessToken_{editorPersonaId}"`. TTL: 9 minutes (same as legacy).
- **`await Task.Delay(TimeSpan.FromSeconds(30), ct)` replaces `Thread.Sleep(30000)`**: The On-Site API requires a 30-second propagation delay between user create/update and the subsequent audit diff read. The refactored code uses `await Task.Delay` so the request thread is released during the wait and the delay is cancellable.
- **`ArgumentNullException.ThrowIfNull`**: Replaces manual null checks and `throw new ArgumentNullException(nameof(...))` throughout the constructor.
- **Collection expressions `[]`**: All empty-collection initialisers use C# 12 collection expressions (`[]`) instead of `new List<T>()` or `Array.Empty<T>()`.
- **`when (ex is not OperationCanceledException)` catch guards**: All `catch (Exception ex)` blocks include this guard so cancellation exceptions propagate correctly rather than being swallowed.

---

## IDbConnectionFactory Pattern

Business logic exclusively uses injected repository and service interfaces backed by `IDbConnectionFactory` in the data-access layer:

- `IProductContextServiceAsync` — replaces `GetCompanyEditorAndUserDetails` / `DefaultUserClaim`
- `IProductSettingServiceAsync` — replaces direct `ProductSetting` DB calls
- `ISamlRepositoryAsync` — replaces direct SAML DB calls via `SamlRepository`
- `IManagePersonaAsync` — replaces `ManagePersona.ListActivePersona(...)`
- `IManagePersonAsync` — replaces `ManagePerson.GetPerson(...)`
- `IManageUserLoginAsync` — replaces `ManageUserLogin.GetUserLoginOnly(...)`
- `IManageElectronicAddressAsync` — replaces `ManageElectronicAddress.GetElectronicAddress(...)`

No `new Repository(...)` or `new ManageUnifiedLogin(userClaims)` instantiation occurs anywhere in the refactored class. DI manages all lifetimes and connection pooling.

---

## Mutable State Eliminated

| Legacy mutable field | Replaced by |
|---|---|
| `_token` | `OnSiteCtx.AccessToken` (immutable, per-call; also cached in `IMemoryCache`) |
| `_apiEndPoint` | `OnSiteCtx.ApiEndPoint` / `OnSiteApiConfig.ApiEndPoint` (immutable, per-call) |
| `_apiSecret` | `OnSiteApiConfig.ApiSecret` (immutable, per-call) |
| `_clientId` | `OnSiteApiConfig.ClientId` (immutable, per-call) |
| `_tokenEndPoint` | `OnSiteApiConfig.TokenEndPoint` (immutable, per-call) |
| `_productUserId` | `OnSiteCtx.ProductUserId` (immutable, per-call; `with` expression for overrides) |
| `_productUsername` | `OnSiteCtx.ProductUsername` (immutable, per-call) |
| `_companyInstanceSourceId` | `OnSiteCtx.CompanyInstanceSourceId` (immutable, per-call) |
| `_client` (shared `HttpClient`) | `IHttpClientFactory.CreateClient()` per logical request |

---

## Internal Model Classes

All JSON model classes are defined as `private sealed` classes at the bottom of `ManageProductOnSiteAsync.cs` to avoid polluting the shared objects layer with API-specific shapes:

| Class | Purpose |
|---|---|
| `OnSiteApiUserWrapper` | Top-level GET users response: `List<OnSiteApiUserProfile> Users`, `int Total` |
| `OnSiteApiUserProfile` | Per-user profile: `UserId`, `Username`, `FirstName`, `LastName`, `EmailAddress`, `IsActive`, `OnSiteApiPropertyAccess PropertyAccess`, `List<OnSiteRoleModel> Roles` |
| `OnSiteApiPropertyAccess` | User's property access: `List<int> CompanyIdList`, `List<int> PropertyIdList`, `List<int> RegionIdList` |
| `OnSiteRoleModel` | Role: `int Id`, `string Name` |
| `OnSitePropertyModel` | Property with dual-name aliasing: `Id`/`PropertyId` → same backing field; `Name`/`PropertyName` → same backing field; `[JsonPropertyName("active")] bool IsActive`; `[JsonIgnore] bool IsAssigned` |
| `OnSiteRegionModel` | Region with dual-name aliasing: `Id`/`RegionId` → same backing field; `Name`/`RegionName` → same backing field; `[JsonIgnore] bool IsAssigned` |
| `OnSiteInsertUpdateModel` | POST/PUT user body: `Username`, `FirstName`, `LastName`, `EmailAddress`, `OnSiteApiPropertyAccess PropertyAccess`, `List<OnSiteRoleModel> Roles` |
| `OnSiteProfileUpdateModel` | Profile-only PUT body: `FirstName`, `LastName`, `EmailAddress` |
| `OnSiteMigrateUsersRequest` | Migration batch POST body: `List<OnSiteMigrateUserItem> Users` |
| `OnSiteMigrateUserItem` | Per-user migration item: `string Username`, `bool EnableGreenbookLogin` |

The dual-property pattern in `OnSitePropertyModel` and `OnSiteRegionModel` handles inconsistencies in the On-Site REST API where some endpoints return `id`/`name` and others return `property_id`/`property_name` for the same entity type.

---

## Removed / Not Ported

- **`MemoryCache.Default` (System.Runtime.Caching)**: Replaced by `IMemoryCache` (Microsoft.Extensions.Caching.Memory). The `using System.Runtime.Caching` import is removed.
- **`Thread.Sleep(30000)`**: Replaced by `await Task.Delay(TimeSpan.FromSeconds(30), ct)`.
- **`new HttpClient()` per call**: Replaced by `IHttpClientFactory`. The shared `_client` instance field is eliminated.
- **`JsonConvert.DeserializeObject<dynamic>`**: Replaced by `JsonDocument.Parse` (for single-field extraction) and `JsonSerializer.Deserialize<T>` with STJ options (for typed deserialization). The `using Newtonsoft.Json` import is removed.
- **`WriteToDiagnosticLog` / `WriteToErrorLog`**: Replaced entirely by `ILogger<ManageProductOnSiteAsync>` structured logging (`LogDebug`, `LogWarning`, `LogError`).
- **`DefaultUserClaim` constructor parameter**: Removed from all method signatures. Per-call context is resolved internally via `IProductContextServiceAsync`.
- **Test constructor `ManageProductOnSite(DefaultUserClaim, HttpClient)`**: Eliminated. `IHttpClientFactory` enables test doubles via the standard named-client pattern without a separate constructor.
- **`OnSiteUserPropertyRegionRole` DTO**: Not ported as an interface parameter. The three lists (`propertyList`, `regionList`, `roleList`) are passed individually in `ManageOnSiteUserAsync` to avoid cross-layer type coupling in the interface definition.
- **`PushToQueue` / sync audit dispatch**: Audit data is returned as `List<AdditionalParameters>` in the tuple return values and consumed by the calling service layer, consistent with the rest of the async product management layer.

---

## Files Changed

- `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IManageProductOnSiteAsync.cs` — **new file**; 11 method signatures; usings for `UnifiedLogin.SharedObjects.Audit.Common`, `UnifiedLogin.SharedObjects.Enum`, `UnifiedLogin.SharedObjects.Product.Migration`
- `UnifiedLogin.BusinessLogic/LogicAsync/ManageProductOnSiteAsync.cs` — **new file**; ~900 lines; single 11-dep constructor; 11 public methods; 13 private helpers (including 3 merge helpers and 3 static/pure helpers); 10 internal STJ model classes; usings for `System.Net.Http`, `System.Text.Json`, `System.Text.Json.Serialization`, `Microsoft.Extensions.Caching.Memory`, `Microsoft.Extensions.Logging`, `UnifiedLogin.BusinessLogic.Repository.Interfaces`, `UnifiedLogin.SharedObjects.Product.Migration`
