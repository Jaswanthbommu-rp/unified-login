using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductRPDMControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductRPDocumentManagement> _mockManageProductRPDocumentManagement;
        private readonly Mock<IManagePersonaAsync> _mockManagePersonaAsync;
        private readonly Mock<IManageProductRPDocumentManagementAsync> _mockManageProductRPDocumentManagementAsync;
        private ProductRPDMController _controller;

        public ProductRPDMControllerTests()
        {
            _mockManageProductRPDocumentManagement = new Mock<IManageProductRPDocumentManagement>();
            _mockManagePersonaAsync = new Mock<IManagePersonaAsync>();
            _mockManageProductRPDocumentManagementAsync = new Mock<IManageProductRPDocumentManagementAsync>();

            _controller = new ProductRPDMController(
                MockUserClaimsAccessor.Object,
                _mockManageProductRPDocumentManagement.Object,
                _mockManagePersonaAsync.Object,
                _mockManageProductRPDocumentManagementAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductRPDMController(
                MockUserClaimsAccessor.Object,
                _mockManageProductRPDocumentManagement.Object,
                _mockManagePersonaAsync.Object,
                _mockManageProductRPDocumentManagementAsync.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductRPDMController(
                null!,
                _mockManageProductRPDocumentManagement.Object,
                _mockManagePersonaAsync.Object,
                _mockManageProductRPDocumentManagementAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductRPDocumentManagement_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductRPDMController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockManagePersonaAsync.Object,
                _mockManageProductRPDocumentManagementAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductRPDMController(
                MockUserClaimsAccessor.Object,
                _mockManageProductRPDocumentManagement.Object,
                null!,
                _mockManageProductRPDocumentManagementAsync.Object));
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoles(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductRPDMController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRPDocumentManagement.Object,
                _mockManagePersonaAsync.Object,
                _mockManageProductRPDocumentManagementAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WhenResultIsError_ReturnsInternalServerError()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = true });

            var result = await _controller.GetRoles(100, 200, new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetRoles_WithNullDataFilter_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(100, 200, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRoleClassifierDataset Tests

        [Fact]
        public async Task GetRoleClassifierDataset_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoleClassifierDataset(0, 200, "role1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoleClassifierDataset_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductRPDMController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRPDocumentManagement.Object,
                _mockManagePersonaAsync.Object,
                _mockManageProductRPDocumentManagementAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoleClassifierDataset(100, 200, "role1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoleClassifierDataset_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoleClassifierDataset(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoleClassifierDataset(100, 200, "role1", new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoleClassifierDataset_WhenResultIsError_ReturnsForbidden()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoleClassifierDataset(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = true });

            var result = await _controller.GetRoleClassifierDataset(100, 200, "role1", new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetRoleClassifierDataset_WithNullRoleId_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoleClassifierDataset(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoleClassifierDataset(100, 200, null!, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetDomain Tests

        [Fact]
        public async Task GetDomain_WithZeroPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetDomain(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("personaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetDomain_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductRPDMController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRPDocumentManagement.Object,
                _mockManagePersonaAsync.Object,
                _mockManageProductRPDocumentManagementAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetDomain(100);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetDomain_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetDomain(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetDomain(100);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetDomain_WhenResultIsError_ReturnsForbidden()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetDomain(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = true });

            var result = await _controller.GetDomain(100);

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        #endregion

        #region UpdateRPDUserStatus Tests

        [Fact]
        public async Task UpdateRPDUserStatus_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UnassignUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(string.Empty);

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await _controller.UpdateRPDUserStatus(productUser);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully disabled product user.", okResult.Value);
        }

        [Fact]
        public async Task UpdateRPDUserStatus_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UnassignUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns("Error");

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await _controller.UpdateRPDUserStatus(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Deactivate DocumentDirectory product user failed.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRPDUserStatus_WithZeroUserId_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UnassignUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(string.Empty);

            var productUser = new ProductUser { UserId = 0, UserName = "testuser@test.com" };

            var result = await _controller.UpdateRPDUserStatus(productUser);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRPDUserStatus_WithNegativeUserId_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UnassignUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(string.Empty);

            var productUser = new ProductUser { UserId = -1, UserName = "testuser@test.com" };

            var result = await _controller.UpdateRPDUserStatus(productUser);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListRPDMigrationUsers Tests

        [Fact]
        public async Task ListRPDMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListRPDMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListRPDMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListRPDMigrationUsers(999999, new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        //[Fact]
        //public async Task ListRPDMigrationUsers_WithValidParameters_ReturnsOkResult()
        //{
        //    _mockManagePersona
        //        .Setup(x => x.GetPersona(It.IsAny<long>()))
        //        .Returns(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

        //    var result = await _controller.ListRPDMigrationUsers(100, new RequestParameter());

        //    Assert.IsType<OkObjectResult>(result);
        //}

        [Fact]
        public async Task ListRPDMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            var result = await _controller.ListRPDMigrationUsers(100, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(new MigrateResponse { Status = true });

            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(new MigrateResponse { Status = true });

            var migrateUsers = new List<MigrateUser>();

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(new MigrateResponse { Status = true });

            var result = await _controller.UpdateUsersMigrationStatus(null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ReturnsOkResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(new MigrateResponse { Status = true });

            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser
                {
                    UserId = "user1",
                    UnifiedLoginUserName = "user1@test.com",
                    UsingUnifiedLogin = true
                }
            };

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetRoles_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(long.MaxValue, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoleClassifierDataset_WithEmptyRoleId_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetRoleClassifierDataset(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoleClassifierDataset(100, 200, "", new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetDomain_WithMaxLongPersonaId_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.GetDomain(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetDomain(long.MaxValue);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRPDMigrationUsers_WithNegativeEditorPersonaId_ReturnsResult()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListRPDMigrationUsers(-1, new RequestParameter());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRPDUserStatus_WithMaxIntUserId_ReturnsResult()
        {
            _mockManageProductRPDocumentManagement
                .Setup(x => x.UnassignUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(string.Empty);

            var productUser = new ProductUser
            {
                UserId = int.MaxValue,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateRPDUserStatus(productUser);

            Assert.IsType<OkObjectResult>(result);
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
