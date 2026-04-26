using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using static UnifiedLogin.BusinessLogic.Logic.Product.SAML.RealPageSAML;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for ProductController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageProductAsync> _mockManageProduct;
        private readonly Mock<IUserLoginRepositoryAsync> _mockUserLoginRepo;
        private readonly Mock<ISamlRepositoryAsync> _mockSamlRepo;
        private readonly Mock<IProductRepositoryAsync> _mockProductRepo;
        private readonly Mock<IManageBlueBookAsync> _mockManageBlueBook;
        private readonly Mock<IManagePersonaAsync> _mockManagePersona;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private ProductController _controller;

        #endregion

        #region Constructor

        public ProductControllerTests()
        {
            _mockManageProduct = new Mock<IManageProductAsync>();
            _mockUserLoginRepo = new Mock<IUserLoginRepositoryAsync>();
            _mockSamlRepo = new Mock<ISamlRepositoryAsync>();
            _mockProductRepo = new Mock<IProductRepositoryAsync>();
            _mockManageBlueBook = new Mock<IManageBlueBookAsync>();
            _mockManagePersona = new Mock<IManagePersonaAsync>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ProductController>>();

            MockUserClaimsAccessor
                .Setup(x => x.UserRealPageGuid)
                .Returns(Guid.NewGuid());
            MockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.NewGuid() });
            MockUserClaimsAccessor
                .Setup(x => x.ClientCode)
                .Returns("BBA");

            _controller = CreateController();
        }

        private ProductController CreateController() =>
            new ProductController(
                _mockManageProduct.Object,
                _mockUserLoginRepo.Object,
                _mockSamlRepo.Object,
                _mockProductRepo.Object,
                _mockManageBlueBook.Object,
                _mockManagePersona.Object,
                MockUserClaimsAccessor.Object,
                _mockMemoryCache.Object,
                _mockHttpClientFactory.Object,
                _mockLogger.Object)
            {
                ControllerContext = CreateControllerContext()
            };

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            Assert.NotNull(_controller);
        }

        [Fact]
        public void Constructor_WithNullManageProduct_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductController(
                null!, _mockUserLoginRepo.Object, _mockSamlRepo.Object, _mockProductRepo.Object,
                _mockManageBlueBook.Object, _mockManagePersona.Object, MockUserClaimsAccessor.Object,
                _mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullUserLoginRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductController(
                _mockManageProduct.Object, null!, _mockSamlRepo.Object, _mockProductRepo.Object,
                _mockManageBlueBook.Object, _mockManagePersona.Object, MockUserClaimsAccessor.Object,
                _mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullSamlRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductController(
                _mockManageProduct.Object, _mockUserLoginRepo.Object, null!, _mockProductRepo.Object,
                _mockManageBlueBook.Object, _mockManagePersona.Object, MockUserClaimsAccessor.Object,
                _mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullProductRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductController(
                _mockManageProduct.Object, _mockUserLoginRepo.Object, _mockSamlRepo.Object, null!,
                _mockManageBlueBook.Object, _mockManagePersona.Object, MockUserClaimsAccessor.Object,
                _mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullManageBlueBook_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductController(
                _mockManageProduct.Object, _mockUserLoginRepo.Object, _mockSamlRepo.Object, _mockProductRepo.Object,
                null!, _mockManagePersona.Object, MockUserClaimsAccessor.Object,
                _mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductController(
                _mockManageProduct.Object, _mockUserLoginRepo.Object, _mockSamlRepo.Object, _mockProductRepo.Object,
                _mockManageBlueBook.Object, null!, MockUserClaimsAccessor.Object,
                _mockMemoryCache.Object, _mockHttpClientFactory.Object, _mockLogger.Object));
        }

        #endregion

        #region ListProductUsers Tests

        [Fact]
        public async Task ListProductUsers_WithEmptyGuidAndNoBBACode_ReturnsOkWithError()
        {
            MockUserClaimsAccessor.Setup(x => x.ClientCode).Returns("OTHER");
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.ListProductUsers((int)ProductEnum.OneSite, 100);

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Product.ListProductUsers.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListProductUsers_WithInvalidProductId_ReturnsOkWithError()
        {
            var result = await _controller.ListProductUsers(9999, 100);

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Product.ListProductUsers.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListProductUsers_WithZeroCompanyInstanceId_ReturnsOkWithError()
        {
            var result = await _controller.ListProductUsers((int)ProductEnum.OneSite, 0);

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Product.ListProductUsers.3", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListProductUsers_WithNegativePersonaId_ReturnsOkWithError()
        {
            var result = await _controller.ListProductUsers((int)ProductEnum.OneSite, 100, -1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Product.ListProductUsers.4", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListProductUsers_WithNullResult_ReturnsOkWithError()
        {
            _mockManageProduct
                .Setup(x => x.GetProductUsersAsync((int)ProductEnum.OneSite, 100, 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<ProductUsers>)null!);

            var result = await _controller.ListProductUsers((int)ProductEnum.OneSite, 100);

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Product.ListProductUsers.5", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListProductUsers_WithValidData_ReturnsOkWithList()
        {
            var users = new List<ProductUsers> { new ProductUsers() };
            _mockManageProduct
                .Setup(x => x.GetProductUsersAsync((int)ProductEnum.OneSite, 100, 0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            var result = await _controller.ListProductUsers((int)ProductEnum.OneSite, 100);

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUsers, IErrorData>>(ok.Value);
            Assert.Same(users, output.list);
        }

        #endregion

        #region GetProductFamilies Tests

        [Fact]
        public async Task GetProductFamilies_WhenNullResult_ReturnsOkWithError()
        {
            _mockManageProduct
                .Setup(x => x.GetProductFamiliesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<ProductFamily>)null!);

            var result = await _controller.GetProductFamilies();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductFamily, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Product.GetProductFamilies.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetProductFamilies_WithData_ReturnsOkWithList()
        {
            var families = new List<ProductFamily> { new ProductFamily() };
            _mockManageProduct
                .Setup(x => x.GetProductFamiliesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(families);

            var result = await _controller.GetProductFamilies();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductFamily, IErrorData>>(ok.Value);
            Assert.Same(families, output.list);
        }

        #endregion

        #region GetProductInternalSettings Tests

        [Fact]
        public async Task GetProductInternalSettings_WithWrongKey_ReturnsEmptyList()
        {
            var result = await _controller.GetProductInternalSettings(1, Guid.NewGuid());

            var ok = Assert.IsType<OkObjectResult>(result);
            var settings = Assert.IsType<List<ProductInternalSetting>>(ok.Value);
            Assert.Empty(settings);
        }

        [Fact]
        public async Task GetProductInternalSettings_WithCorrectKey_ReturnsSettings()
        {
            var correctKey = new Guid("4AD12A31-680A-476F-863E-26749D2E7DD4");
            var expected = new List<ProductInternalSetting> { new ProductInternalSetting() };

            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetProductInternalSettings(1, correctKey);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetProductNonSensitiveSettings Tests

        [Fact]
        public async Task GetProductNonSensitiveSettings_FiltersSensitiveData()
        {
            var settings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "Public", SensitiveData = false },
                new ProductInternalSetting { Name = "Secret", SensitiveData = true }
            };
            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);

            var result = await _controller.GetProductNonSensitiveSettings(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsType<List<ProductInternalSetting>>(ok.Value);
            Assert.Single(list);
            Assert.Equal("Public", list[0].Name);
        }

        #endregion

        #region GetAllProductNonSensitiveSettingsByType Tests

        [Fact]
        public async Task GetAllProductNonSensitiveSettingsByType_ReturnsFilteredResult()
        {
            var settings = new List<ProductInternalSettingByType>
            {
                new ProductInternalSettingByType { ProductName = "ProductA", SensitiveData = false },
                new ProductInternalSettingByType { ProductName = "ProductB", SensitiveData = true }
            };
            _mockManageProduct
                .Setup(x => x.GetProductSettingByTypeAsync("TypeA", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);

            var result = await _controller.GetAllProductNonSensitiveSettingsByType("TypeA");

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductInternalSettingByType, IErrorData>>(ok.Value);
            Assert.Single(output.list);
            Assert.Equal(1, output.pagingSummary.TotalRecords);
        }

        #endregion

        #region UpdateProductSettingAndLinkToConfiguration Tests

        [Fact]
        public async Task UpdateProductSettingAndLinkToConfiguration_WhenError_ReturnsBadRequest()
        {
            _mockManageProduct
                .Setup(x => x.CreateProductSettingAndLinkToConfigurationAsync(1, It.IsAny<ProductInternalSetting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { ErrorMessage = "Failed to update" });

            var result = await _controller.UpdateProductSettingAndLinkToConfiguration(1, new ProductInternalSetting());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update", badRequest.Value);
        }

        [Fact]
        public async Task UpdateProductSettingAndLinkToConfiguration_WhenSuccess_ReturnsOk()
        {
            _mockManageProduct
                .Setup(x => x.CreateProductSettingAndLinkToConfigurationAsync(1, It.IsAny<ProductInternalSetting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { ErrorMessage = null });

            var result = await _controller.UpdateProductSettingAndLinkToConfiguration(1, new ProductInternalSetting());

            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region ListProductSettingType Tests

        [Fact]
        public async Task ListProductSettingType_ReturnsOkWithResult()
        {
            var expected = new List<ProductSettingType> { new ProductSettingType() };
            _mockManageProduct
                .Setup(x => x.ListProductSettingTypeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.ListProductSettingType();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetProductLoginInfo Tests

        [Fact]
        public void GetProductLoginInfo_ReturnsOkNull()
        {
            var result = _controller.GetProductLoginInfo(1, 100);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Null(ok.Value);
        }

        #endregion

        #region GetProductTypes Tests

        [Fact]
        public async Task GetProductTypes_ReturnsOkWithList()
        {
            var productTypes = new List<ProductType> { new ProductType() };
            _mockManageProduct
                .Setup(x => x.GetProductTypesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productTypes);

            var result = await _controller.GetProductTypes();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductType, IErrorData>>(ok.Value);
            Assert.Same(productTypes, output.list);
        }

        #endregion

        #region GetBooksProductMap Tests

        [Fact]
        public async Task GetBooksProductMap_ReturnsOkWithList()
        {
            var expected = new List<GbProductMap> { new GbProductMap() };
            _mockManageProduct
                .Setup(x => x.ListProductsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetBooksProductMap();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetUDMSourceList Tests

        [Fact]
        public async Task GetUDMSourceList_ReturnsOkWithResult()
        {
            var expected = new List<UDMSource> { new UDMSource() };
            _mockManageBlueBook
                .Setup(x => x.GetUDMSourceListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetUDMSourceList();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetUDMOperators Tests

        [Fact]
        public async Task GetUDMOperators_ReturnsOkWithResult()
        {
            var expected = new List<UPFMOperators> { new UPFMOperators() };
            _mockManageBlueBook
                .Setup(x => x.GetOperatorListForUPFMCompanyAsync(It.IsAny<Guid>(), "UPFM", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetUDMOperators();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetProductLoginDetailsFromProductCode Tests

        [Fact]
        public async Task GetProductLoginDetailsFromProductCode_WhenExceptionThrown_ReturnsOkWithErrorMessage()
        {
            _mockProductRepo
                .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Product not found"));

            var result = await _controller.GetProductLoginDetailsFromProductCode("UNKNOWN", 100);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProductLoginResponse>(ok.Value);
            Assert.Equal("Product not found", response.ErrorMessage);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _controller = null!;
            base.Dispose();
        }

        #endregion
    }
}
