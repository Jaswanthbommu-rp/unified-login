using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for AccessController.
    /// Tests all endpoints, error cases, and edge cases.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AccessControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageSecurity> _mockManageSecurity;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private AccessController _accessController;

        #endregion

        #region Constructor

        public AccessControllerTests()
        {
            _mockManageSecurity = new Mock<IManageSecurity>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _accessController = new AccessController(
                _mockManageSecurity.Object,
                _mockUserClaimsAccessor.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullManageSecurity_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new AccessController(null!, _mockUserClaimsAccessor.Object));

            Assert.Equal("manageSecurity", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new AccessController(_mockManageSecurity.Object, null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new AccessController(
                _mockManageSecurity.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public void GetRights_WithValidRouteId_ReturnsOkResult()
        {
            // Arrange
            const string routeId = "user-management";
            const long personaId = 123;

            var expectedRouteSecurity = new RouteSecurity
            {
                RouteId = routeId,
                Rights = new List<string> { "Read", "Write", "Delete" }
            };

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = expectedRouteSecurity
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(expectedOutput);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.Equal(routeId, returnedOutput.obj.RouteId);
            Assert.Equal(3, returnedOutput.obj.Rights.Count);
        }

        [Fact]
        public void GetRights_WithValidRouteId_CallsManageSecurityWithCorrectPersonaId()
        {
            // Arrange
            const string routeId = "dashboard";
            const long personaId = 456;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>());

            // Act
            _accessController.GetRights(routeId);

            // Assert
            _mockManageSecurity.Verify(
                x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void GetRights_WithInvalidRouteId_ReturnsBadRequest(string? invalidRouteId)
        {
            // Act
            var result = _accessController.GetRights(invalidRouteId!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid routeId", badRequestResult.Value);
        }

        [Fact]
        public void GetRights_WithEmptyRights_ReturnsOkResultWithEmptyRights()
        {
            // Arrange
            const string routeId = "restricted-route";
            const long personaId = 789;

            var expectedRouteSecurity = new RouteSecurity
            {
                RouteId = routeId,
                Rights = new List<string>()
            };

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = expectedRouteSecurity
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(expectedOutput);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.Empty(returnedOutput.obj.Rights);
        }

        [Fact]
        public void GetRights_WithProductRights_ReturnsOkResultWithProductRights()
        {
            // Arrange
            const string routeId = "product-management";
            const long personaId = 100;

            var expectedRouteSecurity = new RouteSecurity
            {
                RouteId = routeId,
                Rights = new List<string> { "ViewProducts" },
                ProductRights = new List<ProductRights>
                {
                    new ProductRights { ProductId = 1, RightName = "Read" },
                    new ProductRights { ProductId = 2, RightName = "Write" }
                }
            };

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = expectedRouteSecurity
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(expectedOutput);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.Equal(2, returnedOutput.obj.ProductRights.Count);
        }

        [Fact]
        public void GetRights_WithSpecialCharactersInRouteId_ReturnsOkResult()
        {
            // Arrange
            const string routeId = "route-with-special_chars.123";
            const long personaId = 200;

            var expectedRouteSecurity = new RouteSecurity
            {
                RouteId = routeId,
                Rights = new List<string> { "Access" }
            };

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = expectedRouteSecurity
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(expectedOutput);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void GetRights_WithLongRouteId_ReturnsOkResult()
        {
            // Arrange
            var routeId = new string('a', 500); // Very long route ID
            const long personaId = 300;

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity { RouteId = routeId }
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(expectedOutput);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void GetRights_WhenManageSecurityReturnsNull_ReturnsOkResultWithNull()
        {
            // Arrange
            const string routeId = "unknown-route";
            const long personaId = 400;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns((ObjectOutput<RouteSecurity, IErrorData>)null!);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public void GetRights_WithZeroPersonaId_StillCallsManageSecurity()
        {
            // Arrange
            const string routeId = "test-route";
            const long personaId = 0;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>());

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            _mockManageSecurity.Verify(
                x => x.GetPersonaRightsAndActionsByRoute(0, routeId),
                Times.Once);
        }

        [Fact]
        public void GetRights_WithNegativePersonaId_StillCallsManageSecurity()
        {
            // Arrange
            const string routeId = "test-route";
            const long personaId = -1;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>());

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            _mockManageSecurity.Verify(
                x => x.GetPersonaRightsAndActionsByRoute(-1, routeId),
                Times.Once);
        }

        [Fact]
        public void GetRights_WithStatusInResponse_ReturnsOkResultWithStatus()
        {
            // Arrange
            const string routeId = "status-route";
            const long personaId = 500;

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity { RouteId = routeId },
                Status = new Status<IErrorData>()
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(expectedOutput);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.NotNull(returnedOutput.Status);
        }

        [Fact]
        public void GetRights_WithMultipleRights_ReturnsAllRights()
        {
            // Arrange
            const string routeId = "multi-rights-route";
            const long personaId = 600;

            var expectedRights = new List<string>
            {
                "Create",
                "Read",
                "Update",
                "Delete",
                "Admin",
                "Export",
                "Import"
            };

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity
                {
                    RouteId = routeId,
                    Rights = expectedRights
                }
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(personaId, routeId))
                .Returns(expectedOutput);

            // Act
            var result = _accessController.GetRights(routeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.Equal(7, returnedOutput.obj.Rights.Count);
            Assert.Contains("Create", returnedOutput.obj.Rights);
            Assert.Contains("Delete", returnedOutput.obj.Rights);
            Assert.Contains("Admin", returnedOutput.obj.Rights);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _accessController = null!;
            base.Dispose();
        }

        #endregion
    }
}





