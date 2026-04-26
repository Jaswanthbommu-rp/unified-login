# Refactor Change Document — `ManageProductAssetOptimizationAsync`

**Source:** `Logic/Product/ManageProductAssetOptimization.cs` (3,715 lines, .NET 4.8)  
**Target:** `LogicAsync/ManageProductAssetOptimizationAsync.cs` (2,417 lines, .NET 9 / C# 13)  
**Interface:** `LogicAsync/Interfaces/IManageProductAssetOptimizationAsync.cs`

---

## 1. What Was Replaced

| Legacy | Replacement | Reason |
|---|---|---|
| `ManageProductBase` inheritance | Eliminated — helpers ported as private async methods | Base class used instance fields set by sync callers; incompatible with DI and async |
| `new HttpClient()` per REST call | `IHttpClientFactory.CreateClient()` + per-request `HttpRequestMessage` | Prevents socket exhaustion; avoids shared `DefaultRequestHeaders` mutation |
| `new ProductRepository(userClaims)` in constructor | `IProductRepositoryAsync` injected | Enables testability and DI lifetime management |
| `new OrganizationRepository(userClaims)` in constructor | `IOrganizationRepositoryAsync` injected | Same as above |
| `new ProductInternalSettingRepository()` in constructor | `IProductSettingServiceAsync` injected | API credentials now loaded lazily via the service rather than at construction time |
| `RPObjectCache.GetFromCache<T>(key, ttl, factory)` | `IMemoryCache.GetOrCreateAsync(key, entry => { ... })` | Native .NET cache; avoids static `MemoryCache.Default` |
| `WriteToDiagnosticLog` / `WriteToErrorLog` (base class) | `ILogger<ManageProductAssetOptimizationAsync>` | Structured logging via MEL |
| Per-method `DefaultUserClaim userClaim` parameter | `IUserClaimsAccessor` injected at construction | Claims resolved once per scope; callers no longer pass user context on every call |
| `out List<AdditionalParameters> additionalParameters` on `ManageAssetOptimizationUser` | Returns `(string Result, List<AdditionalParameters> ActivityLog)` tuple | `out` parameters are incompatible with `async` methods |
| `ManageProductBase.GetCompanyEditorAndUserDetails` (sets instance fields) | Private `GetCallContextAsync` returning `AoCallContext` record | Thread-safe; no shared mutable state between concurrent calls |
| `ManageProductBase.GetProductCompanyInstanceId` | Private `GetAoCompanyAsync` calling `IManageBlueBookAsync.GetProductCompanyMappingAsync` | Replaces base class helper with injected async service |
| `ManageProductBase.UpdateProductSettingProductStatus` | `IProductSettingServiceAsync.UpdateProductStatusAsync` | Clean abstraction; no base class coupling |
| Direct SAML repository writes (`CreateSamlUserAttributeAsync`, `DeleteSamlUserProductInfoAndStatusAsync`) | `ISamlAttributeServiceAsync.UpsertAttributeAsync` / `.DeleteProductInfoAndStatusAsync` | Coordinates SAML write through a service that encapsulates validation and multi-step logic |
| `_productInternalSettingList` loaded at construction (blocking DB call) | `GetApiSettingsAsync` — `ValueTask`-based lazy load cached in `_apiSettings` field | Defers I/O until first use; construction is now allocation-only |
| Sequential `foreach` across companies in `GetCompaniesWithRolesAsync` | `Task.WhenAll` + `SemaphoreSlim(4)` | Parallel company-role fetches; semaphore caps concurrent AO API calls to 4 |
| Cache mutation bug — `GetRoles` marked `IsAssigned` in-place on the cached list | Deep-clone cached list before marking assignments | The sync class corrupted the cache for every subsequent call to that company/product combination |

---

## 2. Architecture Changes

### 2a. `DefaultUserClaim` Removed from Interface

Every method in the old stepping-stone interface took a `DefaultUserClaim userClaim` first argument (because the sync constructor required it). The new interface has zero `DefaultUserClaim` parameters on any method — claims are resolved internally via `IUserClaimsAccessor`.

```csharp
// Before
Task<ListResponse> GetCompaniesAsync(DefaultUserClaim userClaim, long editorPersonaId, ...)

// After
Task<ListResponse> GetCompaniesAsync(long editorPersonaId, ...)
```

### 2b. Private Records Replace Mutable Base-Class State

```csharp
// Two private records replace the base-class instance fields
private sealed record AoApiSettings(
    string Endpoint, string Username, string Password,
    string SuperUser, string SpecialEditorUser);

private sealed record AoCallContext(
    string EditorProductUserId, string ProductUserId,
    string ProductUsername, Persona EditorPersona);
```

`AoCallContext` is created fresh per public method call — no shared mutable state between concurrent requests on the same scoped instance.

### 2c. HTTP Calls — Per-Request Auth Headers

```csharp
// Before (mutates shared headers — thread-unsafe)
_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encoded);
var response = _httpClient.GetAsync(url).Result;   // .Result blocks

// After (isolated per request)
using var request = new HttpRequestMessage(HttpMethod.Get, url);
request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
using var response = await client.SendAsync(request, ct);
```

### 2d. Cache Pattern — Clone Before Mutate

```csharp
// Before — mutates cached list directly (bug)
var cached = RPObjectCache.GetFromCache<List<AORoles>>(key, 10800, () => FetchFromApi());
cached.Find(r => r.Name == roleName).IsAssigned = true;  // corrupts cache for all subsequent callers

// After — clone, then mark
var cached = await _cache.GetOrCreateAsync(key, entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(RoleCacheSeconds);
    return FetchFromApiAsync();
});
var workingCopy = cached.Select(r => r with { IsAssigned = false }).ToList();
// mark IsAssigned on workingCopy only — cached list is never touched
```

---

## 3. Constructor — Dependency Injection (16 dependencies)

```csharp
public ManageProductAssetOptimizationAsync(
    IHttpClientFactory                           httpClientFactory,
    IUserClaimsAccessor                          userClaimsAccessor,
    ISamlAttributeServiceAsync                   samlAttributeService,
    ISamlRepositoryAsync                         samlRepository,
    IProductRepositoryAsync                      productRepository,
    IProductSettingServiceAsync                  productSettingService,
    IOrganizationRepositoryAsync                 organizationRepository,
    IManagePersonaAsync                          managePersona,
    IManagePersonAsync                           managePerson,
    IManageUserLoginAsync                        manageUserLogin,
    IManageBlueBookAsync                         manageBlueBook,
    IManageElectronicAddressAsync                manageElectronicAddress,
    IManagePartyRelationshipAsync                managePartyRelationship,
    IUserLoginRepositoryAsync                    userLoginRepository,
    IMemoryCache                                 cache,
    ILogger<ManageProductAssetOptimizationAsync> logger)
```

**Before:** Constructor took a single `DefaultUserClaim userClaims` and instantiated `new ProductRepository()`, `new OrganizationRepository(userClaims)`, `new ProductInternalSettingRepository()` directly, plus called `base(...)` for `ManageProductBase`.

---

## 4. Cache TTLs

| Data | TTL | Key pattern |
|---|---|---|
| Roles per company/product | 3 hours (10,800 s) | `ao_roles_{companyId}_{product}` |
| Properties per company/product | 2 hours (7,200 s) | `ao_props_{companyId}_{product}` |
| All property groups | 5 minutes (300 s) | `ao_all_groups_{editorId}_{product}` |
| Assignable property groups | 5 minutes (300 s) | `ao_assign_groups_{editorId}_{product}_{companies}` |

---

## 5. New Private Helpers (Replacing Base-Class Methods)

| Helper | Replaces | Notes |
|---|---|---|
| `GetApiSettingsAsync(ct)` | Constructor-time `_productInternalSettingList` loading | Lazy; cached in `_apiSettings` field for the scope lifetime |
| `GetCallContextAsync(editorId, userId, ct)` | `ManageProductBase.GetCompanyEditorAndUserDetails` | Reads `USERID` SAML attribute for editor + subject; returns `AoCallContext` |
| `GetAoCompanyAsync(orgRealPageId, ct)` | `ManageProductBase.GetProductCompanyInstanceId` | Calls `IManageBlueBookAsync.GetProductCompanyMappingAsync` with source `"AO"` |
| `GetApiAsync<T>(url, settings, ct)` | `GetResultFromApi<T>()` | Per-request `HttpRequestMessage`; no shared state |
| `PostApiAsync(url, body, settings, ct)` | `PostApi()` | `StringContent` + per-request auth |
| `PutApiAsync(url, body, settings, ct)` | `PutApi()` | Same pattern |
| `GetRolesAsync(companyId, product, ...)` | Inline role fetch + `RPObjectCache` | `IMemoryCache` with 3-hour TTL; returns deep-cloned list |
| `GetPropertiesAsync(companyId, product, ...)` | Inline property fetch + `RPObjectCache` | `IMemoryCache` with 2-hour TTL |
| `GetAllPropertyGroupsAsync(...)` | Inline group fetch + `RPObjectCache` | `IMemoryCache` with 5-minute TTL |
| `IsSuperUserAsync(personaId, ct)` | `ManageProductBase.IsSuperUser` | `IManagePartyRelationshipAsync.GetPartyRelationshipAsync` — checks `"SuperUser"` role type |
| `IsRegularUserNoEmailAsync(personaId, ct)` | `ManageProductBase.IsRegularUser` | Same pattern — checks `"USER (NO EMAIL)"` |
| `CopyRegularUserAsync(...)` | `CopyRegularUser` | Full async — reads active-authorities API, reconstructs `AoUserCompanyPropertyRoleDetail` list |
| `CreateProductUserInGreenBookAsync(...)` | `ManageProductBase.UpdateProductStatus + CreateSamlUserAttribute` | Calls `ISamlAttributeServiceAsync.UpsertAttributeAsync` + `IProductSettingServiceAsync.UpdateProductStatusAsync` |

---

## 6. Public Method Changes

| Method | Change |
|---|---|
| `GetCompaniesAsync` | Added `CancellationToken`; `DefaultUserClaim` removed; all I/O awaited |
| `GetCompaniesWithRolesAsync` | Per-company role fetches now run in parallel via `Task.WhenAll` + `SemaphoreSlim(4)` |
| `GetCompaniesWithPropertiesAsync` | Sequential (API does not support parallel property lookups per company) |
| `GetProductRolesAsync` | Awaited; uses `GetAoCompanyAsync` instead of base-class method |
| `GetProductPropertiesAsync` | Awaited; cache mutation bug fixed |
| `GetOperatorsAsync` | Awaited; uses `GetCallContextAsync` |
| `GetPropertiesWithOperatorsAsync` | Awaited; query-string values URL-encoded with `Uri.EscapeDataString` |
| `GetPropertyGroupsAsync` | Awaited; uses cached `GetAssignablePropertyGroupsAsync` |
| `GetProductPropertyGroupsAsync` | Awaited; reads active-authorities API |
| `GetPropertiesInGroupAsync` | Awaited |
| `GetGroupPropertiesAsync` | Awaited |
| `GetMigrationUsersAsync` | Awaited; uses `GetProductUsersByCompanyAsync` (now declared on `IProductRepositoryAsync`) |
| `UpdateUsersMigrationStatusAsync` | Awaited |
| `ChangeUserStatusAsync` | Awaited |
| `UpdateUserProfileAsync` | Awaited |
| `ManageAssetOptimizationUserAsync` | **Signature change** — `out List<AdditionalParameters>` → returns `(string Result, List<AdditionalParameters> ActivityLog)` tuple; full async port of ~400-line sync method |
| `GetGbSupportedAoEditorUserProductsToAssignAsync` | `DefaultUserClaim` param removed; awaited |
| `GetGbSupportedAoProductsWithUserAdminRoleAsync` | `DefaultUserClaim` param removed; awaited |
| `GetAOProductsForNewMultiCompanyUserAsync` | `DefaultUserClaim` param removed; awaited |

---

## 7. Interface Additions

Three methods added to `IManageProductAssetOptimizationAsync` that were previously not exposed (they were inlined inside `ManageAssetOptimizationUser`):

```csharp
// Previously inlined — now independently callable
Task<IList<string>> GetGbSupportedAoEditorUserProductsToAssignAsync(
    long editorPersonaId, CancellationToken cancellationToken = default);

Task<IList<string>> GetGbSupportedAoProductsWithUserAdminRoleAsync(
    long editorPersonaId, CancellationToken cancellationToken = default);

Task<List<string>> GetAOProductsForNewMultiCompanyUserAsync(
    long editorPersonaId, string loginName, CancellationToken cancellationToken = default);
```

---

## 8. Side-Effect Fixes Applied During Refactor

| File | Fix |
|---|---|
| `Repository/Interfaces/IProductRepositoryAsync.cs` | Added missing `GetProductUsersByCompanyAsync` declaration — the implementation existed in `ProductRepositoryAsync.cs` but was never declared in the interface |
| `LogicAsync/ManageOrganizationAsync.cs` | Removed stale `DefaultUserClaim` first argument from `GetPropertiesWithOperatorsAsync` call (stepping-stone artefact) |
| `Services/UserServiceAsync.cs` | Removed stale `_userClaims.GetUserClaim()` first argument from `GetGbSupportedAoEditorUserProductsToAssignAsync` call |

---

## 9. What Was Intentionally Not Changed

- **Inner model classes** (`AOUser`, `AORoles`, `AoCompany`, `AoProperty`, `AoPropertyGroup`, `AoPropertyGroups`, `Model`, `GroupModel`, etc.) remain in the sync source file `ManageProductAssetOptimization.cs`. The async class references them via the `Logic.Product` namespace. They are data-only types with no sync I/O and need no migration.
- **Business logic** (role/property assignment rules, SuperUser branching, migration filtering) is a line-for-line port — no logic changes.
- **Static pure helpers** (`FilterAssignedCompanies`, `GetModel`, `GetBundledGroups`, `ExtractActivityDetailLogs`, `CheckAuthorities`, etc.) remain `static` with no async — they perform no I/O.

---

## 10. DI Registration Required

Add to `AddLogicAsyncServices` in `UnifiedLogin.Core/BusinessLogicExtensions.cs`:

```csharp
services.AddScoped<IManageProductAssetOptimizationAsync, ManageProductAssetOptimizationAsync>();
```

> Note: `IHttpClientFactory` is registered automatically via `services.AddHttpClient()`. All other interface dependencies (`ISamlAttributeServiceAsync`, `IProductSettingServiceAsync`, etc.) must already be registered — see the broader deferred DI registration task.
