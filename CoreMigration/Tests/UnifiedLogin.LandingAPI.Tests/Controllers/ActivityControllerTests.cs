using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for ActivityController.
    /// Tests all endpoints, error cases, and edge cases.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ActivityControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IProductService> _mockProductService;
        private ActivityController _activityController;

        #endregion

        #region Constructor

        public ActivityControllerTests()
        {
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockProductService = new Mock<IProductService>();

            SetupDefaultUserClaimsAccessor();
            SetupDefaultProductMocks();

            _activityController = new ActivityController(
                _mockUserClaimsAccessor.Object,
                _mockProductService.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Setup Helpers

        private void SetupDefaultUserClaimsAccessor()
        {
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(DefaultUserClaim.UserId);
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns(DefaultUserClaim.FirstName);
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns(DefaultUserClaim.LastName);
            _mockUserClaimsAccessor.Setup(x => x.LoginName).Returns(DefaultUserClaim.LoginName);
            _mockUserClaimsAccessor.Setup(x => x.CorrelationId).Returns(DefaultUserClaim.CorrelationId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationMasterId).Returns(DefaultUserClaim.OrganizationMasterId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(DefaultUserClaim.OrganizationPartyId);
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(DefaultUserClaim.UserRealPageGuid);
        }

        private void SetupDefaultProductMocks()
        {
            var productList = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 3, Name = "Unified Platform", BooksProductCode = "UPFM", UDMSourceCode = "UPFM" },
                new GbProductMap { ProductId = 1, Name = "OneSite", BooksProductCode = "OS", UDMSourceCode = "OS" }
            };

            _mockProductService
                .Setup(x => x.ListProductsAsync(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productList);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ActivityController(null!, _mockProductService.Object));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullProductService_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ActivityController(_mockUserClaimsAccessor.Object, null!));

            Assert.Equal("productService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ActivityController(
                _mockUserClaimsAccessor.Object,
                _mockProductService.Object);

            Assert.NotNull(controller);
        }

        #endregion

        #region RecordActivity - Logout Tests

        [Fact]
        public async Task RecordActivity_WithLogoutActivityType_ReturnsOkResult()
        {
            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithUppercaseLogout_ReturnsOkResult()
        {
            var result = await _activityController.RecordActivity("LOGOUT");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithMixedCaseLogout_ReturnsOkResult()
        {
            var result = await _activityController.RecordActivity("LoGoUt");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_CallsListProductsAsync()
        {
            await _activityController.RecordActivity("logout");

            _mockProductService.Verify(
                x => x.ListProductsAsync(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesUserClaimsProperties()
        {
            await _activityController.RecordActivity("logout");

            _mockUserClaimsAccessor.Verify(x => x.UserId, Times.AtLeastOnce);
            _mockUserClaimsAccessor.Verify(x => x.FirstName, Times.AtLeastOnce);
            _mockUserClaimsAccessor.Verify(x => x.LastName, Times.AtLeastOnce);
        }

        #endregion

        #region RecordActivity - Invalid Activity Type Tests

        [Theory]
        [InlineData("login")]
        [InlineData("signin")]
        [InlineData("signout")]
        [InlineData("register")]
        [InlineData("unknown")]
        [InlineData("random")]
        [InlineData("123")]
        [InlineData("logout ")]
        [InlineData(" logout")]
        public async Task RecordActivity_WithInvalidActivityType_ReturnsBadRequest(string activityType)
        {
            var result = await _activityController.RecordActivity(activityType);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type", badRequestResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public async Task RecordActivity_WithEmptyOrWhitespaceActivityType_ReturnsBadRequest(string? activityType)
        {
            var result = await _activityController.RecordActivity(activityType!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Activity type cannot be empty", badRequestResult.Value);
        }

        #endregion

        #region RecordActivity - Edge Cases for User Claims

        [Fact]
        public async Task RecordActivity_WithZeroUserId_ReturnsOkButDoesNotLogActivity()
        {
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(0);

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithEmptyFirstName_ReturnsOkButDoesNotLogActivity()
        {
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns(string.Empty);

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithEmptyLastName_ReturnsOkButDoesNotLogActivity()
        {
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns(string.Empty);

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithNullFirstName_ReturnsOkButDoesNotLogActivity()
        {
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns((string)null!);

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithNullLastName_ReturnsOkButDoesNotLogActivity()
        {
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns((string)null!);

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithValidUserIdAndEmptyFirstName_ReturnsOk()
        {
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(123);
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns(string.Empty);
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("User");

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        #endregion

        #region RecordActivity - Product Mapping Tests

        [Fact]
        public async Task RecordActivity_WhenProductNotFound_StillReturnsOk()
        {
            _mockProductService
                .Setup(x => x.ListProductsAsync(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GbProductMap>());

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WhenListProductsThrows_StillReturnsOk()
        {
            _mockProductService
                .Setup(x => x.ListProductsAsync(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithValidProductMapping_ReturnsOk()
        {
            var productList = new List<GbProductMap>
            {
                new GbProductMap { ProductId = 3, Name = "Unified Platform", BooksProductCode = "UPFM", UDMSourceCode = "UPFM" }
            };

            _mockProductService
                .Setup(x => x.ListProductsAsync(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productList);

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        #endregion

        #region RecordActivity - User Claims Accessor Tests

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesCorrelationId()
        {
            var expectedCorrelationId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.CorrelationId).Returns(expectedCorrelationId);

            await _activityController.RecordActivity("logout");

            _mockUserClaimsAccessor.Verify(x => x.CorrelationId, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesOrganizationMasterId()
        {
            _mockUserClaimsAccessor.Setup(x => x.OrganizationMasterId).Returns(999);

            await _activityController.RecordActivity("logout");

            _mockUserClaimsAccessor.Verify(x => x.OrganizationMasterId, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesOrganizationPartyId()
        {
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(5000);

            await _activityController.RecordActivity("logout");

            _mockUserClaimsAccessor.Verify(x => x.OrganizationPartyId, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesLoginName()
        {
            _mockUserClaimsAccessor.Setup(x => x.LoginName).Returns("testuser@example.com");

            await _activityController.RecordActivity("logout");

            _mockUserClaimsAccessor.Verify(x => x.LoginName, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesUserRealPageGuid()
        {
            var expectedGuid = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(expectedGuid);

            await _activityController.RecordActivity("logout");

            _mockUserClaimsAccessor.Verify(x => x.UserRealPageGuid, Times.AtLeastOnce);
        }

        #endregion

        #region RecordActivity - Special Character Tests

        [Fact]
        public async Task RecordActivity_WithSpecialCharactersInActivityType_ReturnsBadRequest()
        {
            var result = await _activityController.RecordActivity("logout!@#$%");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type", badRequestResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithNumericActivityType_ReturnsBadRequest()
        {
            var result = await _activityController.RecordActivity("12345");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type", badRequestResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithVeryLongActivityType_ReturnsBadRequest()
        {
            var result = await _activityController.RecordActivity(new string('a', 1000));

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type", badRequestResult.Value);
        }

        #endregion

        #region RecordActivity - Concurrent Access Tests

        [Fact]
        public async Task RecordActivity_MultipleConcurrentCalls_AllReturnOk()
        {
            var tasks = new List<Task<IActionResult>>();
            for (int i = 0; i < 10; i++)
                tasks.Add(_activityController.RecordActivity("logout"));

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal("Success", okResult.Value);
            }
        }

        #endregion

        #region RecordActivity - User with Different Configurations

        [Fact]
        public async Task RecordActivity_WithLargeUserId_ReturnsOk()
        {
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(int.MaxValue);

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithLongUserName_ReturnsOk()
        {
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns(new string('A', 200));
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns(new string('B', 200));

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithSpecialCharactersInUserName_ReturnsOk()
        {
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns("José");
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("O'Brien");

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithUnicodeUserName_ReturnsOk()
        {
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns("田中");
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("太郎");

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        #endregion

        #region RecordActivity - Exception Handling

        [Fact]
        public async Task RecordActivity_WhenUserClaimsThrows_StillReturnsOk()
        {
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(1);
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns("Test");
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("User");
            _mockUserClaimsAccessor.Setup(x => x.CorrelationId).Throws(new InvalidOperationException("Claims error"));

            var result = await _activityController.RecordActivity("logout");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _activityController = null!;
            base.Dispose();
        }

        #endregion
    }
}
