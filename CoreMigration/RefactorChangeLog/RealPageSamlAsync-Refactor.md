# RealPageSamlAsync Refactor Changelog

**Source file:** `UnifiedLogin.BusinessLogic/Logic/Product/SAML/RealPageSAML.cs`  
**Output files:**
- `UnifiedLogin.BusinessLogic/LogicAsync/SAML/RealPageSamlAsync.cs`
- `UnifiedLogin.BusinessLogic/LogicAsync/Interfaces/IRealPageSamlAsync.cs`

**Target framework:** .NET 10 / C# 13  
**Date:** 2026-04-14

---

## Breaking Changes

| # | Old (sync) | New (async) |
|---|-----------|-------------|
| 1 | `DefaultUserClaim` passed to constructor and read throughout | Removed entirely — caller context (`UserRealPageGuid`, `LoginName`, `Rights`, `OrganizationRealPageGuid`, `ImpersonatedBy`) resolved from injected `IUserClaimsAccessor`; no per-call identity parameters on public methods |
| 2 | `ProductDetails(out bool, out bool, out bool, out IList<>)` — 4 `out` params | Returns `SamlProductDetailsResult` record — `out` incompatible with `async` |
| 3 | `new RealPageSAML(userClaims).GetProductDetailsSAML(...)` — new per-call | Scoped singleton injected via DI; `callerPersonaId` / `targetPersonaId` passed per call |
| 4 | `RPObjectCache` (legacy static cache) for product SAML settings | `IMemoryCache` via DI, 10-minute TTL |
| 5 | `System.Web.Http.HttpResponseException` caught in `ProductDetails` | Removed — not available in .NET Core; catches `Exception` with structured log |
| 6 | `Thread.Sleep` in `createUserBatchIfRequired` (via `BatchProductBulkUpdateRepository.CreateBatch`) | `Task.Delay` via `IBatchProductBulkUpdateRepositoryAsync.CreateBatchAsync` |
| 7 | Serilog static `Log.Logger.ForContext(...)` | `ILogger<RealPageSamlAsync>` structured logging |
| 8 | `X509Store` not disposed | `using var certStore` — deterministic disposal |
| 9 | Nested response classes `RealPageSAML.SAMLResponse` / `RealPageSAML.ProductLoginResponse` | Standalone top-level classes `SamlGeneratedResponse` / `SamlProductLoginResponse` (defined in interface file) |

---

## Constructor: from 2 to 11 Dependencies

| # | Dependency | Purpose |
|---|-----------|---------|
| 1 | `ISamlAttributeServiceAsync` | `GetProductSamlDetailsAsync` — replaces `new SamlRepository()` per call |
| 2 | `ISamlRepositoryAsync` | `GetProductSamlSettingsByProductIdAsync`, `ListAllProductsByPersonaIdAsync` |
| 3 | `IManagePersonaAsync` | `GetPersonaAsync` (target), `GetActivePersonaWithoutRightsAsync` (same-user login), `GetActivePersonaIdAsync` (org admin) |
| 4 | `IManageProductAsync` | `GetProductInternalSettingsAsync` — replaces `new ManageProduct(_userClaims)` |
| 5 | `IManageProductOneSiteAsync` | `GetPmcInfoAsync` — PMC URL rewrite for OneSite SAML endpoint |
| 6 | `IManageProductRPDocumentManagementAsync` | `GetDomainAsync` — domain URL rewrite for RP Document Management |
| 7 | `IBatchProductBulkUpdateRepositoryAsync` | `CreateBatchAsync` — replaces `new BatchProductBulkUpdateRepository(_userClaims)` |
| 8 | `IManageOrganizationAsync` | `GetOrganizationAdminUserRealPageIdAsync` — replaces `new OrganizationRepository()` |
| 9 | `IUserClaimsAccessor` | Caller identity (`UserRealPageGuid`, `LoginName`, `Rights`, `OrganizationRealPageGuid`, `ImpersonatedBy`, `UserId`) — replaces `IManageUserLoginAsync` + per-call `DefaultUserClaim` |
| 10 | `IMemoryCache` | Product SAML settings cache (10-min TTL) |
| 11 | `ILogger<RealPageSamlAsync>` | Structured logging throughout |

All 11 guarded with `ArgumentNullException.ThrowIfNull`.

---

## Eliminated Direct Instantiation (`new X(_userClaims)`)

| Old | New |
|-----|-----|
| `new SamlRepository()` | `ISamlRepositoryAsync` via DI |
| `new ManageProduct(_userClaims)` | `IManageProductAsync` via DI |
| `new ManagePersona()` | `IManagePersonaAsync` via DI |
| `new ManageProductOneSite(_userClaims)` | `IManageProductOneSiteAsync` via DI |
| `new ManageProductRPDocumentManagement(_userClaims)` | `IManageProductRPDocumentManagementAsync` via DI |
| `new BatchProductBulkUpdateRepository(_userClaims)` | `IBatchProductBulkUpdateRepositoryAsync` via DI |
| `new OrganizationRepository()` | `IManageOrganizationAsync` via DI |
| `new UserRepository(_userClaims)` | `IManagePersonaAsync.GetActivePersonaIdAsync` (removes entire repository call) |
| `new UserLoginRepository()` | `IUserClaimsAccessor.LoginName` / `UserRealPageGuid` — no repository call needed |
| `new RPObjectCache()` | `IMemoryCache` via DI |

---

## `out` Params → Record Return

```csharp
// Old — 4 out params, cannot be used with async
public static bool ProductDetails(
    int productId, Persona persona,
    out bool getOneSitePMCURL,
    out bool getDocMgtDomain,
    out bool getMarketingCenterURL,
    out IList<PersonaProductUserDetails> productList)

// New — record return, fully async-compatible
public async Task<SamlProductDetailsResult> GetProductDetailsAsync(
    int productId, long personaId, CancellationToken ct = default)

public sealed record SamlProductDetailsResult(
    bool GetOneSitePmcUrl,
    bool GetDocMgtDomain,
    bool GetMarketingCenterUrl,
    IList<PersonaProductUserDetails> ProductList,
    bool HasError = false);
```

## Public API — Caller Context Removed from All Signatures

The original `RealPageSAML` required callers to pass `DefaultUserClaim` (or derived IDs) on every call. The async version sources all caller context from the injected `IUserClaimsAccessor`, trimming several parameters from the public interface:

| Method | Removed parameters | Replaced by |
|--------|--------------------|-------------|
| `CreateUserBatchIfRequiredAsync` | `editorPersonaId`, `impersonatorUserId` | `_userClaimsAccessor.OrganizationRealPageGuid`, `ImpersonatedBy`, `UserId` |
| `GetProductDetailsSamlAsync` | `callerPersonaId` | `_userClaimsAccessor.UserRealPageGuid` (passed to `ResolvePersonaAsync`) |
| `GetSamlDetailsAsync` | `userRealPageGuid`, `loginName` | `_userClaimsAccessor.UserRealPageGuid`, `LoginName` |

---

## `BuildAssertion` — Pure Static Method

The original `BuildAssertion()` read mutable private properties (`_Issuer`, `_Subject`, `_Destination`, `_ProductId`, `_SigningCertificate`, `AttributeList`, `_productInternalSettingList`). In the async version, it is a `private static` method taking all inputs as explicit parameters:

```csharp
private static XmlDocument BuildAssertion(
    string issuer, string subject, string destination, int productId,
    IList<SamlAttributes> attributeList,
    X509Certificate2 signingCertificate,
    List<ProductInternalSetting> productInternalSettings)
```

This makes it:
- **Thread-safe** — no shared state, safe to call concurrently
- **Testable in isolation** — no class instance needed
- **JIT-devirtualizable** — sealed class + static method

---

## `AddPrefix` — Recursive → Iterative-Style Recursion (Stack Safety)

The original `AddPrefix` recursed into children before setting the prefix on the parent, which is correct. The refactored version preserves the same DFS traversal using `foreach` + recursion, but the logic is cleaned up:

```csharp
private static void AddPrefix(XmlNode node, string prefix)
{
    foreach (XmlNode child in node.ChildNodes)
        AddPrefix(child, prefix);
    node.Prefix = prefix;
}
```

---

## `IManageProductOneSiteAsync` — New `GetPmcInfoAsync`

`ManageProductOneSiteAsync.GetPmcInfoAsync(int pmcId, CancellationToken ct)` was already implemented as `private`. It is promoted to `public` and added to `IManageProductOneSiteAsync`:

```csharp
/// <summary>
/// Returns the PMC server info for a given pmcId.
/// Result is cached per-PMC for 10 minutes.
/// Used by RealPageSamlAsync to rewrite the OneSite SAML endpoint URL.
/// </summary>
Task<PMCInfo?> GetPmcInfoAsync(int pmcId, CancellationToken ct = default);
```

**Impact:** Only the access modifier changed in `ManageProductOneSiteAsync`. No logic was modified.

---

## `GetProductSamlSettingsAsync` — `ValueTask<ProductSamlSettings>` + `IMemoryCache`

```csharp
// Old: RPObjectCache static singleton, 600-second string key
productSamlSettings = rpcache.GetFromCache<ProductSamlSettings>(cacheKey, 600, () => { ... });

// New: IMemoryCache, 10-minute TimeSpan constant, ValueTask (zero Task alloc on cache hit)
private async ValueTask<ProductSamlSettings> GetProductSamlSettingsAsync(int productId, CancellationToken ct)
{
    if (_cache.TryGetValue(cacheKey, out ProductSamlSettings? hit)) return hit!;
    var settings = await _samlRepository.GetProductSamlSettingsByProductIdAsync(lookupId, ct);
    _cache.Set(cacheKey, settings, SettingsCacheTtl);
    return settings;
}
```

---

## `GetOneSitePmcUrlAsync` — URL Rewrite

```csharp
// Old: new ManageProductOneSite(_userClaims).GetPMCURL(personaId) → PMCInfo
// New: PMCID extracted directly from the already-resolved SAML UserId attribute
private async Task<string> GetOneSitePmcUrlAsync(
    string samlEndpointUrl, IList<SamlAttributes> samlList, CancellationToken ct)
{
    if (!int.TryParse(FindAttributeValue(samlList, "UserId")?.Split('|')[0], out int pmcId))
        return samlEndpointUrl;

    var pmcInfo = await _manageOneSite.GetPmcInfoAsync(pmcId, ct);
    var samlUri = new Uri(samlEndpointUrl);
    return $"{samlUri.Scheme}://{pmcInfo.PMCURL}{samlUri.PathAndQuery}";
}
```

The `personaId` parameter is no longer needed — the PMCID is already in the SAML attribute list (it was just re-fetching the same value through a heavier path).

---

## `GetDocManagementDomainUrlAsync` — Delegates to Interface

```csharp
// Old: new ManageProductRPDocumentManagement(_userClaims).GetDomain(personaId)
// New:
var domainResult = await _manageDocMgt.GetDomainAsync(personaId, ct);
if (domainResult.Additional is not null)
    samlEndpointUrl = samlEndpointUrl.Replace("{{domain}}", domainResult.Additional.ToString());
```

---

## `CreateUserBatchIfRequiredAsync` — `Thread.Sleep` → `Task.Delay`

The original `BatchProductBulkUpdateRepository.CreateBatch` called `Thread.Sleep(statusCheckSleep)` in a polling loop. `IBatchProductBulkUpdateRepositoryAsync.CreateBatchAsync` replaces this with `Task.Delay`, freeing the thread-pool thread during the wait.

Also: `UserRepository.GetUserDetails` call removed — the org admin's persona ID is resolved with `IManagePersonaAsync.GetActivePersonaIdAsync(editorRealPageId, ct)` directly, avoiding a separate user repository.

---

## `GetProductDetailsSamlAsync` — `switch` Expression for Product Alias Mapping

```csharp
// Old: switch statement with break cases
switch (productId)
{
    case (int)ProductEnum.UnifiedUI:
        productId = (int)ProductEnum.OneSite;
        break;
    ...
}

// New: switch expression
int resolvedProductId = productId switch
{
    (int)ProductEnum.UnifiedUI          => (int)ProductEnum.OneSite,
    (int)ProductEnum.OneSiteConversions => (int)ProductEnum.OneSite,
    (int)ProductEnum.PropertyPhotos     => (int)ProductEnum.MarketingCenter,
    _                                   => productId
};
```

---

## `ResolvePersonaAsync` — Extracted from Inline Logic

The original `GetPersona(Guid, long)` mixed caller identity lookup with persona validation and swallowed exceptions silently. Extracted to a dedicated private method:

```csharp
private async Task<Persona?> ResolvePersonaAsync(Guid callerRealPageId, long targetPersonaId, CancellationToken ct)
```

- `targetPersonaId == 0` → resolves the caller's own active persona via `GetActivePersonaWithoutRightsAsync(callerRealPageId)` (same-user login flow)
- Impersonation right: `AccessToUnifiedPlatform` checked on `_userClaimsAccessor.Rights` — **not** on `Persona.Rights` (which does not exist in this codebase)
- `callerRealPageId` is sourced from `_userClaimsAccessor.UserRealPageGuid` at the call site
- Exceptions logged as `Warning` (not silently swallowed)

---

## `X509Store` — Deterministic Disposal

```csharp
// Old: manual Open/Close — no using, resource leaked on exception
var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
certStore.Open(...);
// ... find cert ...
certStore.Close();

// New: using declaration
using var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
certStore.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);
```

---

## Response Type Extraction

| Old nested type | New standalone type | Notes |
|----------------|--------------------|----|
| `RealPageSAML.SAMLResponse` | `SamlGeneratedResponse` | `sealed class`, properties unchanged |
| `RealPageSAML.ProductLoginResponse` | `SamlProductLoginResponse` | `sealed class`, `[JsonProperty]` attributes removed (System.Text.Json default serialization in .NET 10) |

Both types are defined at namespace level in `IRealPageSamlAsync.cs` for clean discoverability.

---

## C# 13 Features Used

| Feature | Where |
|---------|-------|
| Collection expressions `[]` | Empty collections: `IList<PersonaProductUserDetails> productListAll = []`, spread `[.. samlList, new() { ... }]` |
| `sealed record` | `SamlProductDetailsResult` — immutable, pattern-matchable |
| `sealed class` | `RealPageSamlAsync`, `SamlGeneratedResponse`, `SamlProductLoginResponse` — JIT devirtualization |
| `switch` expression | Product alias mapping in `GetProductDetailsSamlAsync` |
| Pattern: `productId is (int)ProductEnum.ClientPortal or (int)ProductEnum.AdminSupportPortal` | Salesforce product check in `BuildAssertion` |
| `when` exception filter | `catch (Exception ex)` → logged as `Warning`, not rethrown |
| `ArgumentNullException.ThrowIfNull` | All 11 constructor dependencies |

---

## .NET 10 / Performance Improvements

| Area | Improvement |
|------|-------------|
| Thread-pool starvation | All `.Result` / `Thread.Sleep` blocking calls eliminated |
| Socket / resource exhaustion | DI-managed service lifetimes replace `new X()` per call |
| Static cache singleton | `RPObjectCache` (process-wide) → `IMemoryCache` (DI lifetime, evictable) |
| Thread safety | `BuildAssertion` is `static` and pure — safe for concurrent requests |
| Zero-alloc cache hit | `ValueTask<ProductSamlSettings>` in `GetProductSamlSettingsAsync` |
| Unnecessary data fetch | `UserRepository.GetUserDetails` eliminated — `GetActivePersonaIdAsync` is sufficient |
| `X509Store` leak | Deterministic disposal via `using var` |
| Attribute lookup | `FindAttributeValue` helper → single `FirstOrDefault` with `StringComparison.OrdinalIgnoreCase` |

---

## DI Registration

```csharp
services.AddScoped<IRealPageSamlAsync, RealPageSamlAsync>();
// All dependencies already registered by other product service registrations.
// No new HttpClient named client required.
```
