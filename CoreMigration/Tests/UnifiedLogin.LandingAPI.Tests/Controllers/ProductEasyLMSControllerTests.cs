using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Saml;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for ProductEasyLMSController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductEasyLMSControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageProductEasyLMSAsync> _mockManageProductEasyLMS;
        private readonly Mock<IManageProductAsync> _mockManageProduct;
        private readonly Mock<ISamlRepositoryAsync> _mockSamlRepo;
        private readonly Mock<IProductRepositoryAsync> _mockProductRepo;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private ProductEasyLMSController _controller;

        #endregion

        #region Constructor

        public ProductEasyLMSControllerTests()
        {
            _mockManageProductEasyLMS = new Mock<IManageProductEasyLMSAsync>();
            _mockManageProduct = new Mock<IManageProductAsync>();
            _mockSamlRepo = new Mock<ISamlRepositoryAsync>();
            _mockProductRepo = new Mock<IProductRepositoryAsync>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            MockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim
                {
                    PersonaId = 100L,
                    LoginName = "user@test.com",
                    UserRealPageGuid = Guid.NewGuid()
                });

            _controller = CreateController();
        }

        private ProductEasyLMSController CreateController() =>
            new ProductEasyLMSController(
                MockUserClaimsAccessor.Object,
                _mockManageProductEasyLMS.Object,
                _mockManageProduct.Object,
                _mockSamlRepo.Object,
                _mockProductRepo.Object,
                _mockHttpClientFactory.Object)
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
        public void Constructor_WithNullManageProductEasyLMS_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductEasyLMSController(
                MockUserClaimsAccessor.Object, null!, _mockManageProduct.Object,
                _mockSamlRepo.Object, _mockProductRepo.Object, _mockHttpClientFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProduct_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductEasyLMSController(
                MockUserClaimsAccessor.Object, _mockManageProductEasyLMS.Object, null!,
                _mockSamlRepo.Object, _mockProductRepo.Object, _mockHttpClientFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullSamlRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductEasyLMSController(
                MockUserClaimsAccessor.Object, _mockManageProductEasyLMS.Object, _mockManageProduct.Object,
                null!, _mockProductRepo.Object, _mockHttpClientFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullProductRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductEasyLMSController(
                MockUserClaimsAccessor.Object, _mockManageProductEasyLMS.Object, _mockManageProduct.Object,
                _mockSamlRepo.Object, null!, _mockHttpClientFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductEasyLMSController(
                MockUserClaimsAccessor.Object, _mockManageProductEasyLMS.Object, _mockManageProduct.Object,
                _mockSamlRepo.Object, _mockProductRepo.Object, null!));
        }

        #endregion

        #region ProductEasyLMSUrl Tests

        [Fact]
        public async Task ProductEasyLMSUrl_WhenUserClaimNull_ReturnsUnauthorized()
        {
            MockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var result = await _controller.ProductEasyLMSUrl();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task ProductEasyLMSUrl_WhenCompanyMapNull_ReturnsOkWithError1()
        {
            _mockManageProductEasyLMS
                .Setup(x => x.GetCompanyAPICodeAndKeyAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CustomerCompanyMap)null!);

            var result = await _controller.ProductEasyLMSUrl();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProductEasyLMS, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductEasyLMS.PELMSUrl.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ProductEasyLMSUrl_WhenCompanyInstanceSourceIdZero_ReturnsOkWithError1()
        {
            _mockManageProductEasyLMS
                .Setup(x => x.GetCompanyAPICodeAndKeyAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CustomerCompanyMap { CompanyInstanceSourceId = "0" });

            var result = await _controller.ProductEasyLMSUrl();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProductEasyLMS, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductEasyLMS.PELMSUrl.1", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ProductEasyLMSUrl_WhenNoApiCodeOrKeyAttributes_ReturnsOkWithError3()
        {
            var companyMap = new CustomerCompanyMap
            {
                CompanyInstanceSourceId = "123",
                CompanyInstance = new List<CompanyInstance> { new CompanyInstance { Attributes = new List<InstanceAttribute>() } }
            };

            _mockManageProductEasyLMS
                .Setup(x => x.GetCompanyAPICodeAndKeyAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(companyMap);

            var result = await _controller.ProductEasyLMSUrl();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProductEasyLMS, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductEasyLMS.PELMSUrl.3", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ProductEasyLMSUrl_WhenEmptyApiCodeAndKey_ReturnsOkWithError2()
        {
            var companyMap = BuildCompanyMapWithAttributes("", "");

            _mockManageProductEasyLMS
                .Setup(x => x.GetCompanyAPICodeAndKeyAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(companyMap);

            var result = await _controller.ProductEasyLMSUrl();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProductEasyLMS, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductEasyLMS.PELMSUrl.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ProductEasyLMSUrl_WhenNoProductInternalSettings_ReturnsOkWithError4()
        {
            _mockManageProductEasyLMS
                .Setup(x => x.GetCompanyAPICodeAndKeyAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildCompanyMapWithAttributes("testCode", "testKey"));

            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>());

            var result = await _controller.ProductEasyLMSUrl();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProductEasyLMS, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductEasyLMS.PELMSUrl.4", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ProductEasyLMSUrl_WhenProductUrlEmpty_ReturnsOkWithError5()
        {
            _mockManageProductEasyLMS
                .Setup(x => x.GetCompanyAPICodeAndKeyAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildCompanyMapWithAttributes("testCode", "testKey"));

            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting> { new ProductInternalSetting { Name = "PRODUCTURL", Value = "" } });

            _mockSamlRepo
                .Setup(x => x.GetProductSamlDetailsAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SamlAttributes>());

            var result = await _controller.ProductEasyLMSUrl();

            var ok = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IProductEasyLMS, IErrorData>>(ok.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductEasyLMS.PELMSUrl.5", output.Status.ErrorCode);
        }

        #endregion

        #region Helpers

        private static CustomerCompanyMap BuildCompanyMapWithAttributes(string apiCode, string apiKey) =>
            new CustomerCompanyMap
            {
                CompanyInstanceSourceId = "123",
                CompanyInstance = new List<CompanyInstance>
                {
                    new CompanyInstance
                    {
                        Attributes = new List<InstanceAttribute>
                        {
                            new InstanceAttribute { AttributeName = "API CODE", AttributeValue = apiCode },
                            new InstanceAttribute { AttributeName = "API KEY", AttributeValue = apiKey }
                        }
                    }
                }
            };

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
