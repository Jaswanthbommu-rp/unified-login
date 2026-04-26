using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductLearningPortalControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageProductAsync> _mockManageProduct;
        private readonly Mock<ISamlRepositoryAsync> _mockSamlRepository;
        private readonly Mock<IProductRepositoryAsync> _mockProductRepository;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private ProductLearningPortalController _controller;

        #endregion

        #region Constructor

        public ProductLearningPortalControllerTests()
        {
            _mockManageProduct = new Mock<IManageProductAsync>();
            _mockSamlRepository = new Mock<ISamlRepositoryAsync>();
            _mockProductRepository = new Mock<IProductRepositoryAsync>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>());

            _controller = CreateController();
        }

        private ProductLearningPortalController CreateController() =>
            new ProductLearningPortalController(
                MockUserClaimsAccessor.Object,
                _mockManageProduct.Object,
                _mockSamlRepository.Object,
                _mockProductRepository.Object,
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
        public void Constructor_WithNullManageProduct_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductLearningPortalController(
                MockUserClaimsAccessor.Object, null!, _mockSamlRepository.Object,
                _mockProductRepository.Object, _mockHttpClientFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullSamlRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductLearningPortalController(
                MockUserClaimsAccessor.Object, _mockManageProduct.Object, null!,
                _mockProductRepository.Object, _mockHttpClientFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullProductRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductLearningPortalController(
                MockUserClaimsAccessor.Object, _mockManageProduct.Object, _mockSamlRepository.Object,
                null!, _mockHttpClientFactory.Object));
        }

        [Fact]
        public void Constructor_WithNullHttpClientFactory_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductLearningPortalController(
                MockUserClaimsAccessor.Object, _mockManageProduct.Object, _mockSamlRepository.Object,
                _mockProductRepository.Object, null!));
        }

        #endregion

        #region ProductLearningPortalUrl Tests

        [Fact]
        public async Task ProductLearningPortalUrl_WhenUserClaimNull_ReturnsUnauthorized()
        {
            MockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);
            _controller = CreateController();

            var result = await _controller.ProductLearningPortalUrl();

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task ProductLearningPortalUrl_WhenInternalSettingsEmpty_ReturnsOk()
        {
            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>());

            var result = await _controller.ProductLearningPortalUrl();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ProductLearningPortalUrl_WhenProductUrlEmpty_ReturnsOkWithError()
        {
            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>
                {
                    new ProductInternalSetting { Name = "PRODUCTURL", Value = string.Empty },
                    new ProductInternalSetting { Name = "APICODE", Value = "code123" },
                    new ProductInternalSetting { Name = "APIKEY", Value = "key456" }
                });

            var result = await _controller.ProductLearningPortalUrl();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ProductLearningPortalUrl_WhenApiCodeEmpty_ReturnsOkWithError()
        {
            _mockManageProduct
                .Setup(x => x.GetProductInternalSettingsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductInternalSetting>
                {
                    new ProductInternalSetting { Name = "PRODUCTURL", Value = "https://portal.example.com" },
                    new ProductInternalSetting { Name = "APICODE", Value = string.Empty },
                    new ProductInternalSetting { Name = "APIKEY", Value = "key456" }
                });

            var result = await _controller.ProductLearningPortalUrl();

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Result Class Tests

        [Fact]
        public void Result_DefaultValues_AreCorrect()
        {
            var result = new ProductLearningPortalController.Result();

            Assert.Equal(0, result.status);
            Assert.Equal(string.Empty, result.loginURL);
            Assert.Equal(string.Empty, result.statusmsg);
            Assert.Equal(0, result.UserID);
        }

        [Fact]
        public void Result_CanSetProperties()
        {
            var result = new ProductLearningPortalController.Result
            {
                status = 1,
                loginURL = "https://example.com/login?token=abc123",
                statusmsg = "Success",
                UserID = 12345
            };

            Assert.Equal(1, result.status);
            Assert.Equal("https://example.com/login?token=abc123", result.loginURL);
            Assert.Equal("Success", result.statusmsg);
            Assert.Equal(12345, result.UserID);
        }

        [Fact]
        public void Result_WithFailureStatus_HasCorrectValues()
        {
            var result = new ProductLearningPortalController.Result
            {
                status = 0,
                statusmsg = "User Already Exists"
            };

            Assert.Equal(0, result.status);
            Assert.Equal("User Already Exists", result.statusmsg);
        }

        [Fact]
        public void Result_WithSuccessStatus_HasCorrectValues()
        {
            var result = new ProductLearningPortalController.Result
            {
                status = 1,
                loginURL = "https://learningportal.example.com/session?token=xyz789",
                statusmsg = "Login successful"
            };

            Assert.Equal(1, result.status);
            Assert.Contains("token=", result.loginURL);
            Assert.Equal("Login successful", result.statusmsg);
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
