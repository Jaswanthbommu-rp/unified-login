using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for AccessController.
    /// Tests all endpoints, error cases, and edge cases.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AccessControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageSecurityAsync> _mockManageSecurity;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private AccessController _accessController;

        #endregion

        #region Constructor

        public AccessControllerTests()
        {
            _mockManageSecurity = new Mock<IManageSecurityAsync>();
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
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new AccessController(null!, _mockUserClaimsAccessor.Object));

            Assert.Equal("manageSecurity", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new AccessController(_mockManageSecurity.Object, null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new AccessController(
                _mockManageSecurity.Object,
                _mockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public async Task GetRights_WithValidRouteId_ReturnsOkResult()
        {
            const string routeId = "user-management";
            const long personaId = 123;

            var expectedRouteSecurity = new RouteSecurity
            {
                RouteId = routeId,
                Rights = new List<string> { "Read", "Write", "Delete" }
            };
            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData> { obj = expectedRouteSecurity };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOutput);

            var result = await _accessController.GetRights(routeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.Equal(routeId, returnedOutput.obj.RouteId);
            Assert.Equal(3, returnedOutput.obj.Rights.Count);
        }

        [Fact]
        public async Task GetRights_WithValidRouteId_CallsManageSecurityWithCorrectPersonaId()
        {
            const string routeId = "dashboard";
            const long personaId = 456;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ObjectOutput<RouteSecurity, IErrorData>());

            await _accessController.GetRights(routeId);

            _mockManageSecurity.Verify(
                x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public async Task GetRights_WithInvalidRouteId_ReturnsBadRequest(string? invalidRouteId)
        {
            var result = await _accessController.GetRights(invalidRouteId!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid routeId", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithEmptyRights_ReturnsOkResultWithEmptyRights()
        {
            const string routeId = "restricted-route";
            const long personaId = 789;

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity { RouteId = routeId, Rights = new List<string>() }
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOutput);

            var result = await _accessController.GetRights(routeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.Empty(returnedOutput.obj.Rights);
        }

        [Fact]
        public async Task GetRights_WithProductRights_ReturnsOkResultWithProductRights()
        {
            const string routeId = "product-management";
            const long personaId = 100;

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity
                {
                    RouteId = routeId,
                    Rights = new List<string> { "ViewProducts" },
                    ProductRights = new List<ProductRights>
                    {
                        new ProductRights { ProductId = 1, RightName = "Read" },
                        new ProductRights { ProductId = 2, RightName = "Write" }
                    }
                }
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOutput);

            var result = await _accessController.GetRights(routeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.Equal(2, returnedOutput.obj.ProductRights.Count);
        }

        [Fact]
        public async Task GetRights_WithSpecialCharactersInRouteId_ReturnsOkResult()
        {
            const string routeId = "route-with-special_chars.123";
            const long personaId = 200;

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity { RouteId = routeId, Rights = new List<string> { "Access" } }
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOutput);

            var result = await _accessController.GetRights(routeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetRights_WithLongRouteId_ReturnsOkResult()
        {
            var routeId = new string('a', 500);
            const long personaId = 300;

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity { RouteId = routeId }
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOutput);

            var result = await _accessController.GetRights(routeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetRights_WhenManageSecurityReturnsNull_ReturnsOkResultWithNull()
        {
            const string routeId = "unknown-route";
            const long personaId = 400;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ObjectOutput<RouteSecurity, IErrorData>)null!);

            var result = await _accessController.GetRights(routeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        [Fact]
        public async Task GetRights_WithZeroPersonaId_StillCallsManageSecurity()
        {
            const string routeId = "test-route";
            const long personaId = 0;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ObjectOutput<RouteSecurity, IErrorData>());

            await _accessController.GetRights(routeId);

            _mockManageSecurity.Verify(
                x => x.GetPersonaRightsAndActionsByRouteAsync(0, routeId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRights_WithNegativePersonaId_StillCallsManageSecurity()
        {
            const string routeId = "test-route";
            const long personaId = -1;

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ObjectOutput<RouteSecurity, IErrorData>());

            await _accessController.GetRights(routeId);

            _mockManageSecurity.Verify(
                x => x.GetPersonaRightsAndActionsByRouteAsync(-1, routeId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetRights_WithStatusInResponse_ReturnsOkResultWithStatus()
        {
            const string routeId = "status-route";
            const long personaId = 500;

            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity { RouteId = routeId },
                Status = new Status<IErrorData>()
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOutput);

            var result = await _accessController.GetRights(routeId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOutput = Assert.IsType<ObjectOutput<RouteSecurity, IErrorData>>(okResult.Value);
            Assert.NotNull(returnedOutput.Status);
        }

        [Fact]
        public async Task GetRights_WithMultipleRights_ReturnsAllRights()
        {
            const string routeId = "multi-rights-route";
            const long personaId = 600;

            var expectedRights = new List<string> { "Create", "Read", "Update", "Delete", "Admin", "Export", "Import" };
            var expectedOutput = new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity { RouteId = routeId, Rights = expectedRights }
            };

            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(personaId);
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRouteAsync(personaId, routeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOutput);

            var result = await _accessController.GetRights(routeId);

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
