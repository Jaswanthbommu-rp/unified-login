using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for ProductLead2LeaseController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductLead2LeaseControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageProductLead2LeaseAsync> _mockManageProductLead2Lease;
        private readonly Mock<IManagePersonaAsync> _mockManagePersona;
        private ProductLead2LeaseController _controller;

        #endregion

        #region Constructor

        public ProductLead2LeaseControllerTests()
        {
            _mockManageProductLead2Lease = new Mock<IManageProductLead2LeaseAsync>();
            _mockManagePersona = new Mock<IManagePersonaAsync>();

            _mockManageProductLead2Lease
                .Setup(x => x.GetRolesAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse());

            _mockManageProductLead2Lease
                .Setup(x => x.GetPropertiesAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse());

            _mockManageProductLead2Lease
                .Setup(x => x.UpdateUsersMigrationStatusAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<IList<MigrateUser>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MigrateResponse());

            _controller = CreateController();
        }

        private ProductLead2LeaseController CreateController() =>
            new ProductLead2LeaseController(
                MockUserClaimsAccessor.Object,
                _mockManageProductLead2Lease.Object,
                _mockManagePersona.Object)
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
        public void Constructor_WithNullManageProductLead2Lease_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductLead2LeaseController(
                MockUserClaimsAccessor.Object, null!, _mockManagePersona.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductLead2LeaseController(
                MockUserClaimsAccessor.Object, _mockManageProductLead2Lease.Object, null!));
        }

        #endregion

        #region GetLead2LeaseRoles Tests

        [Fact]
        public async Task GetLead2LeaseRoles_WithValidParameters_ReturnsOk()
        {
            var result = await _controller.GetLead2LeaseRoles(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseRoles_ReturnsListResponse()
        {
            var result = await _controller.GetLead2LeaseRoles(100, 200, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(ok.Value);
        }

        #endregion

        #region GetLead2LeaseProperties Tests

        [Fact]
        public async Task GetLead2LeaseProperties_WithValidParameters_ReturnsOk()
        {
            var result = await _controller.GetLead2LeaseProperties(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseProperties_ReturnsListResponse()
        {
            var result = await _controller.GetLead2LeaseProperties(100, 200, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(ok.Value);
        }

        #endregion

        #region UpdateLead2LeaseUserStatus Tests

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WhenUserClaimNull_ReturnsUnauthorized()
        {
            MockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);
            _controller = CreateController();

            var result = await _controller.UpdateLead2LeaseUserStatus(new ProductUser { UserId = 1, UserName = "test" });

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WhenChangeStatusSucceeds_ReturnsOk()
        {
            _mockManageProductLead2Lease
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateLead2LeaseUserStatus(new ProductUser { UserId = 1, UserName = "test" });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully disabled product user.", ok.Value);
        }

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WhenChangeStatusFails_ReturnsBadRequest()
        {
            _mockManageProductLead2Lease
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateLead2LeaseUserStatus(new ProductUser { UserId = 1, UserName = "test" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region ListLead2LeaseMigrationUsers Tests

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WhenEditorPersonaIdZero_ReturnsBadRequest()
        {
            var result = await _controller.ListLead2LeaseMigrationUsers(0, new RequestParameter());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", bad.Value);
        }

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WhenPersonaNull_ReturnsBadRequest()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListLead2LeaseMigrationUsers(100, new RequestParameter());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not found.", bad.Value);
        }

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WhenSuccess_ReturnsOk()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            _mockManageProductLead2Lease
                .Setup(x => x.GetMigrationUsersAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            var result = await _controller.ListLead2LeaseMigrationUsers(100, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WhenIsError_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            _mockManageProductLead2Lease
                .Setup(x => x.GetMigrationUsersAsync(It.IsAny<DefaultUserClaim>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.ListLead2LeaseMigrationUsers(100, new RequestParameter());

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WhenUserClaimNull_ReturnsUnauthorized()
        {
            MockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);
            _controller = CreateController();

            var result = await _controller.UpdateUsersMigrationStatus(new List<MigrateUser>());

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOk()
        {
            var result = await _controller.UpdateUsersMigrationStatus(new List<MigrateUser>());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_ReturnsMigrateResponse()
        {
            var result = await _controller.UpdateUsersMigrationStatus(new List<MigrateUser>());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<MigrateResponse>(ok.Value);
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
