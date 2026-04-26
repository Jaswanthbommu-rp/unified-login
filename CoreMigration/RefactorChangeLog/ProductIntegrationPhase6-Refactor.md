# Product Integration – Phase 6 Refactor

**Date:** 2026-04-15  
**Branch:** `feature/UserRepositoryRefactor`  
**Author:** CoreMigration async refactor series

---

## Summary

Phase 6 completes the ProductIntegration async refactor by delivering:

1. **`ManageProductFactoryAsync`** — creates the correct Phase 4
   `StandardV1ProductIntegrationAsync` subclass for each product (replaces the
   hard-coded `SelfGuidedTourAsync` fallback in `LegacyIntegrationTypeAsync`).
2. **`IntegrationTypeFactoryAsync`** — implements `IIntegrationTypeFactoryAsync`;
   reads the `ProductIntegrationType` database setting and instantiates the
   appropriate Phase 5 wrapper (`Legacy` / `UPFM` / `StandardV1`).

---

## New Files

### 1. `ManageProductFactoryAsync.cs`

**Location:**
```
UnifiedLogin.BusinessLogic/LogicAsync/Helpers/ProductImplementation/
```

Replaces the sync `ManageProductFactory` (static class, `Activator.CreateInstance`).

**Responsibilities**
- Internal static class; single method `CreateAndInitAsync(productId, editorId, subjectId, …, ct)`.
- Selects the Phase 4 subclass based on `productId` cast to `ProductEnum`:

| Product | Class created |
|---------|--------------|
| `LeadManagement` / `LeadAnalytics` | `LeadManagementAsync` |
| `PortfolioManagement` | `PortfolioManagementAsync` |
| `DepositAlternative` | `DepositAlternativeManagementAsync` (requires `ISamlAttributeServiceAsync`) |
| `ClickPay` | `ClickPayManagementAsync` |
| _(all others)_ | `SelfGuidedTourAsync` |

- Calls `InitAsync(ct)` before returning the instance (matches the Phase 4 init pattern).

---

### 2. `IntegrationTypeFactoryAsync.cs`

**Location:**
```
UnifiedLogin.BusinessLogic/LogicAsync/Helpers/IntegrationTypes/
```

Replaces `IntegrationTypeFactory` (sync).

**Responsibilities**
- Implements `IIntegrationTypeFactoryAsync` (Phase 1 interface).
- Builds a `productId → ProductIntegrationTypeEnum` dictionary **once per factory
  instance**, lazily and thread-safely, via
  `IProductInternalSettingRepositoryAsync.GetProductSettingByTypeAsync("ProductIntegrationType")`.
- String-to-enum map (case-insensitive, identical to sync factory):

| DB value | Enum |
|----------|------|
| `"Legacy"` | `ProductIntegrationTypeEnum.Legacy` |
| `"UPFM"` | `ProductIntegrationTypeEnum.UPFM` |
| `"Standard v1"` | `ProductIntegrationTypeEnum.StandardV1` |

- Products with no entry default to `ProductIntegrationTypeEnum.Legacy`.

**`IIntegrationTypeFactoryAsync` method implementations:**

| Method | Behaviour |
|--------|-----------|
| `GetIntegration(productId)` | Resolves type, instantiates `Legacy` / `UPFM` / `StandardV1` wrapper |
| `GetIntegrationStandardV1(productId)` | Always instantiates `StandardV1IntegrationTypeAsync` |
| `GetIntegrationTypeForProductId(productId)` | Lookup from lazy map; defaults to `Legacy` |

**Constructor parameters (29 total):**

| Group | Params |
|-------|--------|
| Settings | `IProductInternalSettingRepositoryAsync` |
| Legacy product services | 15 (`IManageProductOneSiteAsync` … `IManageProductRPDocumentManagementAsync`) |
| Cross-product (Legacy) | `IManageUnifiedLoginAsync`, `IManageProductAsync`, `IProductRepositoryAsync` |
| DepositAlternative extra | `ISamlAttributeServiceAsync` |
| UPFM | `IManageUPFMProductsIntegrationAsync` |
| StandardV1 / fallback | `IDataCollectorAsync`, `IManagePersonaAsync`, `IManageUserLoginAsync`, `IUserClaimsAccessor`, `IHttpClientFactory`, `ITokenHelperAsync`, `ICacheService`, `ILoggerFactory` |

---

## Modified Files

### `LegacyIntegrationTypeAsync.cs`

**Changes (backward-compatible with Phase 5):**

1. **New field** — `private readonly ISamlAttributeServiceAsync _samlAttributeService`
   added to the StandardV1 fallback dependencies section.

2. **Constructor** — `ISamlAttributeServiceAsync samlAttributeService` added as the
   last parameter (after `ILoggerFactory loggerFactory`).

3. **`CreateStandardV1Async` replaced** — Previously always created `SelfGuidedTourAsync`.
   Now delegates to `ManageProductFactoryAsync.CreateAndInitAsync(…)`, which selects the
   correct Phase 4 subclass per product:
   ```csharp
   private Task<StandardV1ProductIntegrationAsync> CreateStandardV1Async(
       long editorPersonaId, long subjectPersonaId, CancellationToken ct)
       => ManageProductFactoryAsync.CreateAndInitAsync(
              _productId, editorPersonaId, subjectPersonaId,
              _dataCollector, _productRepository, _managePersona, _manageUserLogin,
              _userClaimsAccessor, _httpClientFactory, _tokenHelper, _cacheService, _loggerFactory,
              _samlAttributeService, ct);
   ```

---

## Breaking Changes vs Phase 5

| Area | Phase 5 | Phase 6 |
|------|---------|---------|
| `LegacyIntegrationTypeAsync` constructor | 26 params | 27 params (+`ISamlAttributeServiceAsync`) |
| `CreateStandardV1Async` | always `SelfGuidedTourAsync` | product-specific Phase 4 subclass |

---

## What Phase 6 Does NOT Change

- `IIntegrationTypeFactoryAsync` interface (Phase 1) — no modifications
- Phase 5 wrappers (`StandardV1IntegrationTypeAsync`, `UPFMIntegrationTypeAsync`) — no modifications
- Phase 4 product implementations — no modifications
- `IIntegrationTypeAsync` interface — no modifications

---

## DI Registration Guidance

`IntegrationTypeFactoryAsync` should be registered as **scoped** (once per HTTP request)
so the lazy `ProductIntegrationType` map is built at most once per request.  All 15
product services it holds are also scoped.

```csharp
services.AddScoped<IIntegrationTypeFactoryAsync, IntegrationTypeFactoryAsync>();
```

`ManageProductFactoryAsync` is `internal static` — it requires no DI registration.

---

## ProductIntegration Refactor — Complete

All six phases are now delivered:

| Phase | Deliverable |
|-------|-------------|
| 1 | `IIntegrationTypeAsync`, `IManageProductIntegrationAsync`, `IDataCollectorAsync`, `IIntegrationTypeFactoryAsync` |
| 2 | `ApiIntegrationAsync`, `DataCollectorAsync` |
| 3 | `StandardV1ProductIntegrationAsync` (abstract base, `InitAsync` pattern) |
| 4 | 5 concrete subclasses (`SelfGuidedTourAsync`, `LeadManagementAsync`, `DepositAlternativeManagementAsync`, `ClickPayManagementAsync`, `PortfolioManagementAsync`) |
| 5 | 3 integration-type wrappers (`LegacyIntegrationTypeAsync`, `UPFMIntegrationTypeAsync`, `StandardV1IntegrationTypeAsync`) |
| 6 | `ManageProductFactoryAsync`, `IntegrationTypeFactoryAsync`; `LegacyIntegrationTypeAsync` updated to use correct Phase 4 subclass |
