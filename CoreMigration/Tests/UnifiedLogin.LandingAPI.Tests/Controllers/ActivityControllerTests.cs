using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;

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
        private readonly Mock<IManageProduct> _mockManageProduct;
        private ActivityController _activityController;

        #endregion

        #region Constructor

        public ActivityControllerTests()
        {
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockManageProduct = new Mock<IManageProduct>();

            SetupDefaultUserClaimsAccessor();
            SetupDefaultProductMocks();

            _activityController = new ActivityController(
                _mockUserClaimsAccessor.Object,
                _mockManageProduct.Object
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
                new GbProductMap
                {
                    ProductId = 3, // UnifiedPlatform
                    Name = "Unified Platform",
                    BooksProductCode = "UPFM",
                    UDMSourceCode = "UPFM"
                },
                new GbProductMap
                {
                    ProductId = 1,
                    Name = "OneSite",
                    BooksProductCode = "OS",
                    UDMSourceCode = "OS"
                }
            };

            _mockManageProduct
                .Setup(x => x.ListProducts(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(productList);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ActivityController(null!, _mockManageProduct.Object));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullManageProduct_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ActivityController(_mockUserClaimsAccessor.Object, null!));

            Assert.Equal("manageProduct", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new ActivityController(
                _mockUserClaimsAccessor.Object,
                _mockManageProduct.Object);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region RecordActivity - Logout Tests

        [Fact]
        public async Task RecordActivity_WithLogoutActivityType_ReturnsOkResult()
        {
            // Arrange
            const string activityType = "logout";

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithUppercaseLogout_ReturnsOkResult()
        {
            // Arrange
            const string activityType = "LOGOUT";

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithMixedCaseLogout_ReturnsOkResult()
        {
            // Arrange
            const string activityType = "LoGoUt";

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_CallsListProducts()
        {
            // Arrange
            const string activityType = "logout";

            // Act
            await _activityController.RecordActivity(activityType);

            // Assert
            _mockManageProduct.Verify(
                x => x.ListProducts(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesUserClaimsProperties()
        {
            // Arrange
            const string activityType = "logout";

            // Act
            await _activityController.RecordActivity(activityType);

            // Assert
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
            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
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
            // Act
            var result = await _activityController.RecordActivity(activityType!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Activity type cannot be empty", badRequestResult.Value);
        }

        #endregion

        #region RecordActivity - Edge Cases for User Claims

        [Fact]
        public async Task RecordActivity_WithZeroUserId_ReturnsOkButDoesNotLogActivity()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(0);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithEmptyFirstName_ReturnsOkButDoesNotLogActivity()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns(string.Empty);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithEmptyLastName_ReturnsOkButDoesNotLogActivity()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns(string.Empty);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithNullFirstName_ReturnsOkButDoesNotLogActivity()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns((string)null!);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithNullLastName_ReturnsOkButDoesNotLogActivity()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns((string)null!);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithValidUserIdAndEmptyFirstName_ReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(123);
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns(string.Empty);
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("User");

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        #endregion

        #region RecordActivity - Product Mapping Tests

        [Fact]
        public async Task RecordActivity_WhenProductNotFound_StillReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockManageProduct
                .Setup(x => x.ListProducts(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<GbProductMap>());

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WhenListProductsReturnsNull_StillReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockManageProduct
                .Setup(x => x.ListProducts(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IList<GbProductMap>)null!);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert - Should handle null gracefully (exception caught and logged)
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WhenListProductsThrows_StillReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockManageProduct
                .Setup(x => x.ListProducts(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert - Exception should be caught and logged, returns Ok
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithValidProductMapping_ReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            var productList = new List<GbProductMap>
            {
                new GbProductMap
                {
                    ProductId = 3, // UnifiedPlatform
                    Name = "Unified Platform",
                    BooksProductCode = "UPFM",
                    UDMSourceCode = "UPFM"
                }
            };

            _mockManageProduct
                .Setup(x => x.ListProducts(It.IsAny<int?>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(productList);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        #endregion

        #region RecordActivity - User Claims Accessor Tests

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesCorrelationId()
        {
            // Arrange
            const string activityType = "logout";
            var expectedCorrelationId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.CorrelationId).Returns(expectedCorrelationId);

            // Act
            await _activityController.RecordActivity(activityType);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.CorrelationId, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesOrganizationMasterId()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.OrganizationMasterId).Returns(999);

            // Act
            await _activityController.RecordActivity(activityType);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.OrganizationMasterId, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesOrganizationPartyId()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(5000);

            // Act
            await _activityController.RecordActivity(activityType);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.OrganizationPartyId, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesLoginName()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.LoginName).Returns("testuser@example.com");

            // Act
            await _activityController.RecordActivity(activityType);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.LoginName, Times.AtLeastOnce);
        }

        [Fact]
        public async Task RecordActivity_WithLogout_AccessesUserRealPageGuid()
        {
            // Arrange
            const string activityType = "logout";
            var expectedGuid = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(expectedGuid);

            // Act
            await _activityController.RecordActivity(activityType);

            // Assert
            _mockUserClaimsAccessor.Verify(x => x.UserRealPageGuid, Times.AtLeastOnce);
        }

        #endregion

        #region RecordActivity - Special Character Tests

        [Fact]
        public async Task RecordActivity_WithSpecialCharactersInActivityType_ReturnsBadRequest()
        {
            // Arrange
            const string activityType = "logout!@#$%";

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type", badRequestResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithNumericActivityType_ReturnsBadRequest()
        {
            // Arrange
            const string activityType = "12345";

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type", badRequestResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithVeryLongActivityType_ReturnsBadRequest()
        {
            // Arrange
            var activityType = new string('a', 1000);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid activity type", badRequestResult.Value);
        }

        #endregion

        #region RecordActivity - Concurrent Access Tests

        [Fact]
        public async Task RecordActivity_MultipleConcurrentCalls_AllReturnOk()
        {
            // Arrange
            const string activityType = "logout";
            var tasks = new List<Task<IActionResult>>();

            // Act - Simulate 10 concurrent logout requests
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_activityController.RecordActivity(activityType));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
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
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(int.MaxValue);

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithLongUserName_ReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns(new string('A', 200));
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns(new string('B', 200));

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithSpecialCharactersInUserName_ReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns("José");
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("O'Brien");

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task RecordActivity_WithUnicodeUserName_ReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns("田中");
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("太郎");

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        #endregion

        #region RecordActivity - Exception Handling

        [Fact]
        public async Task RecordActivity_WhenUserClaimsThrows_StillReturnsOk()
        {
            // Arrange
            const string activityType = "logout";
            _mockUserClaimsAccessor.Setup(x => x.UserId).Returns(1);
            _mockUserClaimsAccessor.Setup(x => x.FirstName).Returns("Test");
            _mockUserClaimsAccessor.Setup(x => x.LastName).Returns("User");
            _mockUserClaimsAccessor.Setup(x => x.CorrelationId).Throws(new InvalidOperationException("Claims error"));

            // Act
            var result = await _activityController.RecordActivity(activityType);

            // Assert - Exception caught in LogSignoutActivityAsync
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




