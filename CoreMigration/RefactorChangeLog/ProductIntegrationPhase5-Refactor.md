# Product Integration – Phase 5 Refactor

**Date:** 2026-04-15  
**Branch:** `feature/UserRepositoryRefactor`  
**Author:** CoreMigration async refactor series

---

## Summary

Phase 5 delivers the three integration-type wrapper classes that implement `IIntegrationTypeAsync`
(Phase 1).  These wrappers sit between the `IntegrationTypeFactoryAsync` (Phase 6, pending) and
the product-specific async services / StandardV1 base class (Phases 2-4).

---

## New Files

All three files are placed in:

```
UnifiedLogin.BusinessLogic/LogicAsync/Helpers/IntegrationTypes/
```

### 1. `StandardV1IntegrationTypeAsync.cs`

Replaces `StandardV1IntegrationType` (sync).

**Responsibilities**
- Holds references to the 10 dependencies required by `StandardV1ProductIntegrationAsync`.
- Creates a **fresh** `StandardV1ProductIntegrationAsync` per method call via `CreateAndInitAsync`,
  calling `InitAsync(ct)` before use — mirrors the sync pattern of
  `new StandardV1ProductIntegration(…)` per-method.
- All 16 `IIntegrationTypeAsync` methods delegate through `CreateAndInitAsync`.

**Notable behaviour**
| Method | Behaviour |
|--------|-----------|
| `GetRightsForRoleAsync(long roleId, …)` | Returns `new ListResponse()` — numeric role IDs not supported by StandardV1 |
| `GetRightsForRoleAsync(string roleId, …)` | Delegates to `GetProductRightsForRoleAsync` |
| `CreateUserAsync` | Deserialises `InputJson` as `ProductUserRolePropertiesGroups`; if `IsAssigned` → `CreateUpdateProductUserAsync`, else → `UnassignUserAsync` |
| `ChangeUserTypeAsync` | Deserialises `InputJson`; delegates to `ChangeProductUserTypeAsync` |
| `UpdateUserProfileAsync` / `UpdateUserDetailsAsync` | Both delegate to `UpdateProductUserProfileAsync(ct)` |
| `GetEnterprisePropertiesAsync` | Falls back to `GetPropertiesAsync(userPersonaId, userPersonaId, …)` |

---

### 2. `UPFMIntegrationTypeAsync.cs`

Replaces `UPFMIntegrationType` (sync).

**Responsibilities**
- Thin wrapper over `IManageUPFMProductsIntegrationAsync` and `IProductRepositoryAsync`.
- Routes the subset of operations that UPFM products support.

**Notable behaviour**
| Method | Behaviour |
|--------|-----------|
| `GetPropertiesAsync` | `GetUPFMPropertiesAsync(…, assignedOnly: false, …)` |
| `GetEnterprisePropertiesAsync` | Resolves `BooksProductCode` from product list then calls `GetEnterpriseUPFMPropertiesAsync` |
| `GetRolesAsync` | `GetRolesAsync(editor, user, partyId, ct)` |
| `GetRightsForRoleAsync(long)` | `GetRightsByRoleAsync(editor, partyId, roleId, ct)` |
| `GetRightsForRoleAsync(string)` | `throw NotSupportedException` |
| `CreateUserAsync` | Deserialises `InputJson` as `UPFMProductPropertyRole`; delegates to `ManageUPFMProductUserAsync` |
| `ChangeUserTypeAsync` | Same deserialise + delegate, discards audit params |
| All other methods | `throw NotSupportedException("… not applicable for UPFM products.")` |

`NotSupportedException` replaces the sync `NotImplementedException` stubs — semantically
correct (the operation is intentionally unsupported, not merely unimplemented).

---

### 3. `LegacyIntegrationTypeAsync.cs`

Replaces `LegacyIntegrationType` (sync, ~1 180 lines).

**Responsibilities**
- Per-product dispatcher for every `IIntegrationTypeAsync` method.
- 26-parameter constructor: 15 product-specific async services + 3 cross-product services +
  8 `StandardV1ProductIntegrationAsync` dependencies for the StandardV1 fallback path.
- Private helper `CreateStandardV1Async(editor, subject, ct)` constructs and inits a base-class
  `StandardV1ProductIntegrationAsync` for products that use the StandardV1 HTTP API but do not
  yet have Phase 4 concrete overrides (ClickPay, LeadManagement, LeadAnalytics,
  PortfolioManagement, DepositAlternativeManagement).

**Injected services**

| Parameter | Interface | Products served |
|-----------|-----------|-----------------|
| `oneSite` | `IManageProductOneSiteAsync` | OneSite |
| `marketingCenter` | `IManageProductMarketingCenterAsync` | MarketingCenter |
| `oneSiteAccounting` | `IManageProductOneSiteAccountingAsync` | OneSiteAccounting (FinancialSuite) |
| `ops` | `IManageProductOpsAsync` | Ops |
| `vendorServices` | `IManageProductVendorServicesAsync` | VendorServices |
| `lead2Lease` | `IManageProductLead2LeaseAsync` | Lead2Lease |
| `residentPortal` | `IManageProductResidentPortalAsync` | ResidentPortal |
| `onSite` | `IManageProductOnSiteAsync` | OnSite (1S REST) |
| `rentersInsurance` | `IManageProductRentersInsuranceAsync` | RentersInsurance |
| `rum` | `IManageProductRumAsync` | RUM |
| `assetOptimization` | `IManageProductAssetOptimizationAsync` | All AO products (13 variants) |
| `adminSupportPortal` | `IManageProductAdminSupportPortalAsync` | AdminSupportPortal, ClientPortal, SalesForce |
| `realConnect` | `IManageProductRealConnectAsync` | RealConnect |
| `prospectContact` | `IManageProductProspectContactAsync` | ProspectContact |
| `rpDocumentMgmt` | `IManageProductRPDocumentManagementAsync` | RPDocumentManagement |
| `manageUnifiedLogin` | `IManageUnifiedLoginAsync` | UnifiedPlatform |
| `manageProduct` | `IManageProductAsync` | UnifiedPlatform (settings check) |
| `productRepository` | `IProductRepositoryAsync` | AO product code resolution |
| _(8 StandardV1 deps)_ | see StandardV1IntegrationTypeAsync | StandardV1 fallback path |

**Special cases**

| Product / Method | Special handling |
|-----------------|-----------------|
| ResidentPortal `UpdateUserProfileAsync` | `ManageResidentPortalUserAsync(…, new ResidentPortal(), BatchProcessType.ProfileChange, ct)` — no standalone update method |
| RentersInsurance `UpdateUserProfileAsync` | `ManageRentersInsuranceUserAsync(…, new RentersInsuranceRoleAndPropertyList(), BatchProcessType.ProfileChange, ct)` — same |
| UnifiedPlatform `GetPropertiesAsync` | Reads `UsePropertyInstanceUnifiedLogin` setting; routes to `GetUPFMPropertiesAsync` or `GetPropertiesAsync` accordingly |
| AO products | `GetAoProductCodeAsync` resolves `BooksProductCode` from repository; product name string passed to all AO methods |
| EasyLMS | Not yet migrated (still requires `DefaultUserClaim`); falls to `default: return string.Empty` |
| ClientPortal / SalesForce | Route to `adminSupportPortal` service (both Salesforce-backed) |
| SelfProvisioningPortal | Returns empty `ListResponse` / empty string (no async service yet) |

**Private helpers**

```csharp
private Task<StandardV1ProductIntegrationAsync> CreateStandardV1Async(
    long editorPersonaId, long subjectPersonaId, CancellationToken ct)

private Task<string> GetAoProductCodeAsync(CancellationToken ct)

private static ListResponse ToListResponse<T>(IList<T> items) where T : class

private static T? TryDeserialize<T>(string? json)   // System.Text.Json
```

---

## Breaking Changes vs Sync Versions

| Area | Sync | Async |
|------|------|-------|
| `DefaultUserClaim` / `IUserClaims` | Passed into constructor | Removed; `IProductContextServiceAsync` used by services internally |
| `out` parameters | Multiple `out` params in sync `IIntegrationType` | Named-tuple returns `(string result, List<AdditionalParameters> auditParams)` |
| `NotImplementedException` stubs (UPFM) | `throw new NotImplementedException()` | `throw new NotSupportedException("… not applicable for UPFM products.")` |
| `ManageProductFactory` (sync) | Static factory consulted for migration/rights lookups | Removed; each service injected directly |
| JSON deserialisation | `JsonConvert.DeserializeObject<T>` (Newtonsoft) | `System.Text.Json.JsonSerializer.Deserialize<T>` |
| `CancellationToken` | Not present | All methods accept `ct = default` |

---

## What Phase 5 Does NOT Change

- `IIntegrationTypeAsync` interface (Phase 1) — no modifications
- `IIntegrationTypeFactoryAsync` interface (Phase 1) — no modifications
- Any Phase 1-4 files (interfaces, base class, concrete implementations)
- `ProductEntityEndpointKeyEnum` — no additions

---

## DI Registration Guidance

These classes are **not** registered directly in DI — they are instantiated by
`IntegrationTypeFactoryAsync` (Phase 6) based on the product's `ProductIntegrationType`
database setting.  The factory resolves all dependencies from the container and passes them
to the appropriate constructor.

For tests or manual wiring:

```csharp
// StandardV1
var standardV1 = new StandardV1IntegrationTypeAsync(
    productId, dataCollector, productRepository, managePersona, manageUserLogin,
    userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory);

// UPFM
var upfm = new UPFMIntegrationTypeAsync(productId, upfmService, productRepository);

// Legacy (abbreviated — 26 params total)
var legacy = new LegacyIntegrationTypeAsync(
    productId,
    oneSite, marketingCenter, oneSiteAccounting, ops, vendorServices, lead2Lease,
    residentPortal, onSite, rentersInsurance, rum, assetOptimization,
    adminSupportPortal, realConnect, prospectContact, rpDocumentMgmt,
    manageUnifiedLogin, manageProduct, productRepository,
    dataCollector, managePersona, manageUserLogin, userClaimsAccessor,
    httpClientFactory, tokenHelper, cacheService, loggerFactory);
```

---

## Phase 6 – Pending

| Item | Description |
|------|-------------|
| `IntegrationTypeFactoryAsync` | Implements `IIntegrationTypeFactoryAsync`; reads `ProductIntegrationType` db setting; instantiates Legacy / UPFM / StandardV1 wrapper |
| `ManageProductFactoryAsync` | Creates Phase 4 concrete implementations (ClickPayManagementAsync, PortfolioManagementAsync, LeadManagementAsync, DepositAlternativeManagementAsync, SelfGuidedTourAsync) for products on the StandardV1 path inside LegacyIntegrationTypeAsync |
| EasyLMS migration | `IManageProductEasyLMSAsync` still depends on `DefaultUserClaim`; needs a separate async interface before it can be included in LegacyIntegrationTypeAsync |
