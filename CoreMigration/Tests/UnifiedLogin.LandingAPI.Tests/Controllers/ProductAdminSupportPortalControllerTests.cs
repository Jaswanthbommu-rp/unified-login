using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for ProductAdminSupportPortalController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductAdminSupportPortalControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageProductAdminSupportPortalAsync> _mockManagePortal;
        private readonly Mock<IManagePersonaAsync> _mockManagePersona;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private ProductAdminSupportPortalController _controller;

        #endregion

        #region Constructor

        public ProductAdminSupportPortalControllerTests()
        {
            _mockManagePortal = new Mock<IManageProductAdminSupportPortalAsync>();
            _mockManagePersona = new Mock<IManagePersonaAsync>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;

            _mockUserClaimsAccessor
                .Setup(x => x.UserRealPageGuid)
                .Returns(Guid.NewGuid());
            _mockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.NewGuid() });
            _mockUserClaimsAccessor
                .Setup(x => x.PersonaId)
                .Returns(999L);

            _controller = new ProductAdminSupportPortalController(
                _mockUserClaimsAccessor.Object,
                _mockManagePortal.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductAdminSupportPortalController(
                _mockUserClaimsAccessor.Object,
                _mockManagePortal.Object,
                _mockManagePersona.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullPortalService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductAdminSupportPortalController(
                _mockUserClaimsAccessor.Object,
                null!,
                _mockManagePersona.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductAdminSupportPortalController(
                _mockUserClaimsAccessor.Object,
                _mockManagePortal.Object,
                null!));
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoles(0, 100, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetRoles(100, 200, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetRoles_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManagePortal
                .Setup(x => x.GetRolesAsync(It.IsAny<DefaultUserClaim>(), 100, 200, It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetRoles(100, 200, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetProperties Tests

        [Fact]
        public async Task GetProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetProperties(0, 100, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetProperties(100, 200, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetProperties_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManagePortal
                .Setup(x => x.GetPropertiesAsync(It.IsAny<DefaultUserClaim>(), 100, 200, It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetProperties(100, 200, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region ListClientPortalMigrationUsers Tests

        [Fact]
        public async Task ListClientPortalMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListClientPortalMigrationUsers(0, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task ListClientPortalMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(100L, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListClientPortalMigrationUsers(100, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task ListClientPortalMigrationUsers_WhenPersonaFound_ReturnsOkWithList()
        {
            var persona = new Persona { RealPageId = Guid.NewGuid() };
            var expected = new ListResponse();

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(100L, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(persona);
            _mockManagePortal
                .Setup(x => x.GetMigrationUsersAsync(It.IsAny<DefaultUserClaim>(), 100L, It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.ListClientPortalMigrationUsers(100, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task ListClientPortalMigrationUsers_SetsUserClaimRealPageIdFromPersona()
        {
            var personaRealPageId = Guid.NewGuid();
            var persona = new Persona { RealPageId = personaRealPageId };
            DefaultUserClaim capturedClaim = null!;

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(100L, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(persona);
            _mockManagePortal
                .Setup(x => x.GetMigrationUsersAsync(It.IsAny<DefaultUserClaim>(), 100L, It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .Callback<DefaultUserClaim, long, RequestParameter, CancellationToken>((claim, _, _, _) => capturedClaim = claim)
                .ReturnsAsync(new ListResponse());

            await _controller.ListClientPortalMigrationUsers(100, new RequestParameter());

            Assert.NotNull(capturedClaim);
            Assert.Equal(personaRealPageId, capturedClaim.UserRealPageGuid);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true }
            };
            var expected = new MigrateResponse();

            _mockManagePortal
                .Setup(x => x.UpdateUsersMigrationStatusAsync(It.IsAny<DefaultUserClaim>(), 999L, migrateUsers, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            _mockManagePortal
                .Setup(x => x.UpdateUsersMigrationStatusAsync(It.IsAny<DefaultUserClaim>(), 999L, null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MigrateResponse());

            var result = await _controller.UpdateUsersMigrationStatus(null!);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateClientPortalUserStatus Tests

        [Fact]
        public async Task UpdateClientPortalUserStatus_WhenSucceeds_ReturnsOkWithMessage()
        {
            var productUser = new ProductUser { UserId = 123, UserLogin = "user@test.com" };

            _mockManagePortal
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<DefaultUserClaim>(), 999L, "user@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateClientPortalUserStatus(productUser);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully disabled product user.", ok.Value);
        }

        [Fact]
        public async Task UpdateClientPortalUserStatus_WhenFails_ReturnsBadRequest()
        {
            var productUser = new ProductUser { UserId = 123, UserLogin = "user@test.com" };

            _mockManagePortal
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<DefaultUserClaim>(), 999L, "user@test.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateClientPortalUserStatus(productUser);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Disabling Client Portal user failed.", badRequest.Value);
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
