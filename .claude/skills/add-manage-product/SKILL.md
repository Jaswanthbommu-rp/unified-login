---
name: add-manage-product
description: Scaffold a new ManageProduct{Name} + IManageProduct{Name} pair under Component/Landing/Logic/Product/ for a new product integration in unified-login-main. Use when the user wants to add a new RealPage product to Unified Login (per-product user-management logic). Captures the ManageProductBase inheritance, dual-constructor (default + unit test), DefaultUserClaim wiring, ProductInternalSetting URL lookup, and matching xUnit test scaffold with RPObjectCache.BustCache().
---

# Add a new ManageProduct{Name}

Per-product integration logic in this repo lives under `Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Product/` as `ManageProduct{Name}` paired with `IManageProduct{Name}`. This skill scaffolds that pair plus the test.

## Inputs to confirm with the user

Before generating files, ask for:

1. **Product name** in PascalCase (e.g., `ContactCenter`). This drives the class name `ManageProductContactCenter` and interface `IManageProductContactCenter`.
2. **`ProductEnum` value** — confirm the entry exists in `Enterprise/Subsystem/ProductLauncher/Service/SharedObjects/Enum/ProductEnum.cs`. If missing, stop and tell the user to add it (the test that enforces `ProductEnum`↔`BlueBookProductConstants` lockstep will otherwise break the build).
3. **Integration shape** — does this product talk to an external API (like `ManageProductOps`, which loads `APIENDPOINT` from `ProductInternalSetting` and calls a base URL with `HttpClient`), or is it purely DB-driven (like `ManageProductBase` defaults)? This decides whether the constructor wires an `HttpClient`/`_productUrl`.
4. **Per-product `Audit.Common` namespace** — most products import `Component.SharedObjects.Audit.Common` and have a product-specific subfolder under `SharedObjects/Landing/Product/{Name}/`. Confirm whether the user wants those DTOs scaffolded too, or will reuse existing ones.

## Files to create

For product `ContactCenter`:

- **Interface:** `Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Product/Interfaces/IManageProductContactCenter.cs`
- **Implementation:** `Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Product/ManageProductContactCenter.cs`
- **Test:** `Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/Logic/Product/ManageProductContactCenterTests.cs`

If the test project's `LandingAPI.Test.csproj` does not auto-include `*.cs`, add the new test file to it.

## Implementation template

The implementation inherits `ManageProductBase`, accepts `DefaultUserClaim` in the default constructor, and exposes a unit-test constructor accepting `HttpMessageHandler`, repository interfaces, `IManagePersona`, `ISamlRepository`, `IManageBlueBook`, `IProductRepository`, and `IRepository`. Mirror `ManageProductOps` ([Component/Landing/Logic/Product/ManageProductOps.cs:37](../../Enterprise/Subsystem/ProductLauncher/Component/Landing/Logic/Product/ManageProductOps.cs)) for an API-backed product, or the simpler shape used by `ManageProductRum` for a thinner one.

```csharp
namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Used to manage {ProductName} users and roles
    /// </summary>
    public class ManageProduct{Name} : ManageProductBase, IManageProduct{Name}
    {
        private string _{name}Url;
        private DefaultUserClaim _userClaims;

        #region Ctor
        public ManageProduct{Name}(DefaultUserClaim userClaims)
            : base((int)ProductEnum.{Name}, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _editorRealPageId = userClaims.UserRealPageGuid;
            _userClaims = userClaims;
            _blueBook = new ManageBlueBook(userClaims);

            // Only if the product uses an external API:
            _{name}Url = _productInternalSettingList
                .First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
            _client.BaseAddress = new Uri(_{name}Url);
        }

        /// <summary>Unit test constructor</summary>
        public ManageProduct{Name}(
            Guid editorRealPageId,
            DefaultUserClaim userClaim,
            HttpMessageHandler httpMessageHandler,
            HttpClient client,
            IProductInternalSettingRepository productInternalSettingRepository,
            IManagePersona managePersona,
            ISamlRepository samlRepository,
            IManageBlueBook blueBook,
            IProductRepository productRepository,
            IRepository repository)
            : base((int)ProductEnum.{Name}, userClaim, repository, httpMessageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _userClaims = userClaim;
            _client = client;
            _productInternalSettingRepository = productInternalSettingRepository;
            _blueBook = blueBook;
            _managePersona = managePersona;
            _samlRepository = samlRepository;
            _productRepository = productRepository;
            _{name}Url = _productInternalSettingList
                .First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
            _client.BaseAddress = new Uri(_{name}Url);
        }
        #endregion
    }
}
```

The interface lives in `Component/Landing/Logic/Product/Interfaces/`. Keep XML docs on every public method — they surface in Swagger when the controller wraps them.

## Test scaffold

Use `xUnit` + `Moq`. Mirror `ManageProductTest` ([LandingAPI.Test/Logic/ManageProductTest.cs:26](../../Enterprise/Subsystem/ProductLauncher/Service/LandingAPI/LandingAPI.Test/Logic/ManageProductTest.cs)) for the field layout: mocks for `IProductRepository`, `IProductInternalSettingRepository`, `IUnifiedLoginRepository`, `IManagePersona`, `IManageBlueBook`, `IManagePartyRelationship`, `HttpMessageHandler`. Inherit `TestBase` and decorate with `[ExcludeFromCodeCoverage]`.

Reset cached SP results between assertions with `new RPObjectCache().BustCache();`.

```csharp
[ExcludeFromCodeCoverage]
public class ManageProduct{Name}Tests : TestBase
{
    private readonly Mock<IRepository> _mockRepository = new Mock<IRepository>();
    private readonly Mock<IProductRepository> _mockProductRepository = new Mock<IProductRepository>();
    private readonly Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
    private readonly Mock<IManagePersona> _mockManagePersona = new Mock<IManagePersona>();
    private readonly Mock<IManageBlueBook> _mockBlueBook = new Mock<IManageBlueBook>();
    private readonly Mock<ISamlRepository> _mockSamlRepository = new Mock<ISamlRepository>();
    private readonly Mock<HttpMessageHandler> _mockMessageHandler = new Mock<HttpMessageHandler>();
    private readonly DefaultUserClaim _userClaim = new DefaultUserClaim { CorrelationId = Guid.Empty };

    [Fact]
    public void {Method}_HappyPath_ReturnsExpected()
    {
        // Arrange
        new RPObjectCache().BustCache();
        var sut = new ManageProduct{Name}(
            Guid.NewGuid(), _userClaim, _mockMessageHandler.Object,
            new HttpClient(_mockMessageHandler.Object),
            _mockProductInternalSettingRepository.Object,
            _mockManagePersona.Object, _mockSamlRepository.Object,
            _mockBlueBook.Object, _mockProductRepository.Object,
            _mockRepository.Object);

        // Act
        // var result = sut.{Method}(...);

        // Assert
        // Assert.True(...);
    }
}
```

## After scaffolding — checklist for the user

1. Confirm `ProductEnum.{Name}` exists and the matching `BlueBookProductConstants` entry is in lockstep.
2. Add the `APIENDPOINT` (or equivalent) row to `ProductInternalSetting` in DB seed if this product needs one.
3. Decide whether a matching `Product{Name}Controller` should be added under `Service/LandingAPI/Controllers/` (use the `add-controller-endpoint` skill).
4. Build `MasterProjectSolution.sln` and run the new test from VS Test Explorer or `dotnet test --filter "FullyQualifiedName~ManageProduct{Name}Tests"`.

## Important

- **Do not** introduce a new base class or abstraction; inherit `ManageProductBase`.
- **Do not** add the test file outside `LandingAPI.Test/Logic/Product/` — controller tests live elsewhere and have different mocking expectations.
- Keep the namespace as `RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product` for the implementation and `...Logic.Product.Interfaces` for the interface — do not invent new namespaces.
