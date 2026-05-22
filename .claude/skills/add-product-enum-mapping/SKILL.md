---
name: add-product-enum-mapping
description: Add a new RealPage product to ProductEnum and BlueBookProductConstants in unified-login-main. Use when wiring a new product (e.g., a new ManageProduct{Name}) — these two files must stay in lockstep, and a test in OrganizationTests asserts that every BlueBookProductConstants entry maps back to a ProductEnum value. Skill captures the [Description] attribute pairing, the ignore-list escape hatch, and the test path that catches a missing entry.
---

# Add a product to ProductEnum + BlueBookProductConstants

Two files must change together for any new product:

- **Enum:** [Service/SharedObjects/Enum/ProductEnum.cs](../../Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Enum/ProductEnum.cs) — assigns a numeric `ProductId` and pins the BlueBook code via `[Description("XYZ")]`.
- **Constants:** [Service/SharedObjects/Constants/BlueBookProductConstants.cs](../../Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Constants/BlueBookProductConstants.cs) — exposes the same BlueBook code as a string `const`.

The pair is **enforced at test time** by [LandingAPI.Test/ControllerTest/OrganizationTests.cs:1541](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/ControllerTest/OrganizationTests.cs), which iterates every `BlueBookProductConstants` field and asserts that `ProductEnumHelper.GetProductIdByProductCode(code, _gbProductMap)` resolves it — and that every `ProductEnum` member has a matching BB entry (unless explicitly ignored). Missing one half = red test = broken build.

## Inputs to confirm

1. **Product name** in PascalCase — must be identical in both files (e.g., `ContactCenter` becomes `ProductEnum.ContactCenter` and `BlueBookProductConstants.ContactCenter`).
2. **BlueBook product code** — the short string used by the Books/UDM API (e.g., `"CC"`). Confirm this with whoever owns the product registration in Books before adding; the wrong code will silently route all callers to the wrong external API.
3. **`ProductId` (int)** — pick the next unused integer. `ProductEnum` uses non-contiguous IDs assigned by Books — don't pick a number near an existing range without checking it isn't reserved.
4. **Is the product going through BlueBook?** If the product is **not** registered in Books (e.g., UI-only products like `UnifiedUI`, `SelfProvisioningPortal`), add the `ProductEnum` entry **only** and append the name to the `ignoreProductList` in the lockstep test (`OrganizationTests.cs:1554`). Otherwise both files must be updated.

## Edits

### 1. `ProductEnum.cs`

Add to the `ProductEnum` enum block, near the end of the file. Keep XML doc + `[Description]` attribute:

```csharp
/// <summary>
/// Contact Center
/// </summary>
[Description("CC")]
ContactCenter = 105,
```

If the product surfaces a manageable user-right, also add a matching entry to `ProductRightEnum` further down the same file.

### 2. `BlueBookProductConstants.cs`

Add a `public const string` with the same name as the enum member and the BB code as the value:

```csharp
/// <summary>
/// Contact Center
/// </summary>
public const string ContactCenter = "CC";
```

**The name must match `ProductEnum.{Name}` exactly** (case-sensitive). The lockstep test uses `Enum.Parse(typeof(ProductEnum), pi.Name)` — a typo here surfaces as `Missing product {name} in BlueBookProductConstants.cs`.

### 3. (Only when the product is not in Books) `OrganizationTests.cs` ignore list

If the new product is intentionally absent from `BlueBookProductConstants`, add its enum name to the inline list at [OrganizationTests.cs:1554](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/ControllerTest/OrganizationTests.cs):

```csharp
var ignoreProductList = new List<int>()
{
    (int)ProductEnum.UnifiedUI,
    /* existing entries */
    (int)ProductEnum.ContactCenter,   // new
};
```

Justify the exclusion in PR description — it should be rare.

## After edits — checklist

1. Run the lockstep test:
   ```powershell
   dotnet test Enterprise\Subsystem\ProductLauncher\Service\LandingAPI\LandingAPI.Test\LandingAPI.Test.csproj `
     --filter "FullyQualifiedName~OrganizationTests"
   ```
   The specific guard is the test that loops `typeof(BlueBookProductConstants).GetFields()` and `typeof(ProductEnum).GetFields()` — if it passes, the pair is consistent.
2. If you also added a `ManageProduct{Name}`, the constructor calls `base((int)ProductEnum.{Name}, ...)` — make sure that compiles.
3. If the product needs an `APIENDPOINT` (external API), follow the `add-product-internal-setting` skill to register the URL.
4. Search call sites that switch on `ProductEnum` (`Grep "case ProductEnum\\."`) — many helpers like `ProductEnumHelper.GetAoProductId`, `GetAoDivisionName`, `GetAoProductDescription` may need updating if the new product belongs to an existing umbrella group (e.g., AO).

## Important

- **Both files or neither.** Adding only one half is the most common failure mode — the lockstep test exists precisely because this has bitten the team before.
- **Do not reuse a `ProductId`.** Every existing `Persona`/`Product` row in DB references the int — repurposing the number silently rewrites history.
- **Do not change an existing `[Description("XYZ")]` value.** External callers (Books, integrations) lookup by code; renaming `"OS"` → `"OSITE"` breaks them.
- **Do not add a product to the `ignoreProductList`** just to make the test pass. Skipping the BB constant means `ProductEnumHelper.GetProductIdByProductCode` will throw at runtime for any caller that needs it.
- This skill does **not** alter `ProductRightEnum` automatically — only update it if the product needs a manageable user-right (most new products do not).
