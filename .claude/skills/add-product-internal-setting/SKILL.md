---
name: add-product-internal-setting
description: Add a new ProductInternalSetting toggle / endpoint URL row in unified-login-main and wire it through logic + tests. Use when adding a configurable per-product flag (like IsSendGridEnabled / IsUnifiedEmailEnabled) or a per-product URL (like APIENDPOINT, SendGridApiEndPoint). Captures the DB seed, the GetProductInternalSettings lookup with FirstOrDefault by Name, the boolean "1" / "0" string convention, and the test setup that injects a stubbed ProductInternalSetting list.
---

# Add a ProductInternalSetting toggle

`ProductInternalSetting` is the repo's per-product runtime config store — used for both **boolean toggles** (`IsSendGridEnabled`, `IsUnifiedEmailEnabled`, `BooksUseDomains`) and **URL/endpoint values** (`APIENDPOINT`, `SendGridApiEndPoint`, `SettingsApiEndPoint`). Each setting is keyed on `(ProductId, Name)` and stored as a string `Value`.

Settings are loaded via [Component/Landing/Repository/ProductInternalSettingRepository.cs](../../Enterprise/Subsystem/ProductLauncher/Component/Landing/Repository/ProductInternalSettingRepository.cs) — `GetProductInternalSettings((int)ProductEnum.UnifiedPlatform)` returns a `List<ProductInternalSetting>`, cached in `RPObjectCache` for ~120 seconds by the consumers.

## Inputs to confirm

1. **Setting name** in PascalCase, no spaces (e.g., `IsContactCenterEnabled`, `ContactCenterApiEndPoint`).
2. **Owning product** — almost always `ProductEnum.UnifiedPlatform` (the umbrella). Per-product API URLs typically live under the product itself (e.g., `ProductEnum.OpsBuyer` for the Ops `APIENDPOINT`).
3. **Setting type** — boolean toggle (string `"1"` / `"0"`), URL/endpoint (string), or numeric (string). The DB column is `nvarchar`; everything is stringified.
4. **Default value** for the seed row. Booleans usually start at `"0"` (off).
5. **DB project** — confirm the new row is added to the Identity DB project under [Database/Identity/](../../Enterprise/Subsystem/ProductLauncher/Database/Identity/) post-deploy seed scripts, or whichever mechanism this repo uses to populate `Identity.ProductSetting`. Check existing rows for `IsSendGridEnabled` to find the seed file.

## Steps

### 1. Add the row to the DB project

Find where existing `ProductSetting` rows are seeded — search the Identity DB project (`IdentityDatabase.sln`) for the literal `IsSendGridEnabled` to locate the seed script:

```powershell
# from repo root
# Grep tool: pattern = 'IsSendGridEnabled', glob = 'Database/Identity/**/*.sql'
```

Add a row using the same INSERT / MERGE pattern that file uses. Do **not** insert directly into the live DB.

### 2. Read the setting from logic

The consumer pattern is — in every logic class — to look the setting up via `FirstOrDefault` on the cached list. Mirror [Component/Landing/Logic/ManageEmail.cs:322](../../Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/ManageEmail.cs):

```csharp
var productSettingList = _productInternalSettingRepository
    .GetProductInternalSettings(productId: (int)ProductEnum.UnifiedPlatform);

bool isContactCenterEnabled = false;
if (productSettingList.Any(s => s.Name.Equals("IsContactCenterEnabled", StringComparison.OrdinalIgnoreCase)))
{
    isContactCenterEnabled = productSettingList
        .FirstOrDefault(s => s.Name.Equals("IsContactCenterEnabled", StringComparison.OrdinalIgnoreCase))
        .Value.Equals("1");
}
```

For an endpoint URL value, use:

```csharp
string apiEndpoint = productSettingList
    .FirstOrDefault(s => s.Name.Equals("ContactCenterApiEndPoint", StringComparison.OrdinalIgnoreCase))?
    .Value;
```

In `ManageProduct{Name}` subclasses, the base already populates `_productInternalSettingList` — read directly off that field (see [Logic/Product/ManageProductOps.cs:66](../../Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Product/ManageProductOps.cs)).

### 3. (Optional) Define a name constant

The repo does **not** centralize ProductInternalSetting names today — call sites use string literals (`"IsSendGridEnabled"`, `"APIENDPOINT"`). This is intentional: each consumer owns the name it cares about, and the strings rarely overlap.

If your new setting is read from 3+ call sites, add a `public const string` near the first consumer (e.g., a `MyFeatureSettings` static class) — do not invent a global `ProductInternalSettingConstants` file.

### 4. Test

Mock `IProductInternalSettingRepository.GetProductInternalSettings` to return a hand-crafted list including your new setting. Mirror [LandingAPI.Test/Logic/ManageEmailTests.cs:188](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/Logic/ManageEmailTests.cs):

```csharp
var productInternalSettingList = new List<ProductInternalSetting>
{
    new ProductInternalSetting { Name = "IsContactCenterEnabled",       Value = "1" },
    new ProductInternalSetting { Name = "ContactCenterApiEndPoint",     Value = "https://staging-cc.realpage.com" },
};

_mockProductInternalSettingRepository
    .Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
    .Returns(productInternalSettingList);

// Bust cache before the assertion so the SUT does not see a stale prior list:
new RPObjectCache().BustCache();

var sut = new MyLogic(_userClaims, _mockEmailRepository.Object, _mockProductInternalSettingRepository.Object);
```

Cover both branches (toggle on `"1"` and off `"0"`/missing) in separate `[Fact]`s.

## After adding — checklist

1. Confirm the row exists in your local Dev DB after running the Identity DB project (`IdentityDatabase.sln`). Without the seed, the consumer's `FirstOrDefault` returns `null` and the code path silently falls through to the default branch.
2. Verify all consumers use `StringComparison.OrdinalIgnoreCase` on the name — case-sensitive lookups miss when the DB has slightly different casing.
3. For URL settings, also confirm the env-specific value is provisioned in QA/Staging/Prod seed scripts (or via PME ticket if those DBs are managed outside this repo).
4. For boolean toggles, the convention is `"1"` / `"0"` strings. Avoid `"true"` / `"false"` — every existing call site uses `.Value.Equals("1")` and will read `"true"` as `false`.

## Important

- **The cache TTL is ~120 seconds** (`RPObjectCache.GetFromCache(cacheKey, 120, ...)`). Toggling a setting in DB takes up to 2 minutes to take effect; tests must call `new RPObjectCache().BustCache();` to see updated values.
- **Do not use `ProductInternalSetting` as a feature flag** with frequent toggling — flip-flopping during traffic is unsafe. For runtime gating, use LaunchDarkly via the `add-launchdarkly-flag` skill.
- **Do not store secrets** in `ProductInternalSetting`. The table is meant for non-secret config. Secrets belong in Web.config under encrypted sections or in the RealPage secret store referenced by `ConfigReader`.
- **Do not introduce a parallel settings repository.** All product config goes through `ProductInternalSettingRepository`. Adding a sibling class would split lookups and silently miss the cache.
- **Do not bypass the `IsXxxEnabled` check.** When a toggle exists, every code path that calls the gated feature must consult it — even logs that mention the feature should respect the toggle so the toggle remains the single source of truth.
