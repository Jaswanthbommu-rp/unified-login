using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductOpsControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductOps> _mockManageProductOps;
        private readonly Mock<IManageOrganization> _mockManageOrganization;
        private readonly Mock<IManagePersona> _mockManagePersona;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IManageUserRoleRight> _mockManageUserRoleRight;
        private ProductOpsController _controller;

        public ProductOpsControllerTests()
        {
            _mockManageProductOps = new Mock<IManageProductOps>();
            _mockManageOrganization = new Mock<IManageOrganization>();
            _mockManagePersona = new Mock<IManagePersona>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockManageUserRoleRight = new Mock<IManageUserRoleRight>();

            _controller = new ProductOpsController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOps.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductOpsController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOps.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOpsController(
                null!,
                _mockManageProductOps.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductOps_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOpsController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageOrganization_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOpsController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOps.Object,
                null!,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOpsController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOps.Object,
                _mockManageOrganization.Object,
                null!,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePerson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOpsController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOps.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                null!,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserLogin_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOpsController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOps.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                null!,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserRoleRight_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOpsController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOps.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                null!));
        }

        #endregion

        #region GetOpsRoles Tests

        [Fact]
        public async Task GetOpsRoles_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOpsRoles(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetOpsRoles_WithAssetGroup_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOpsRoles(100, 200, new RequestParameter(), "AssetGroup1");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetOpsRoles_WithZeroEditorPersonaId_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOpsRoles(0, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetOpsAssets Tests

        [Fact]
        public async Task GetOpsAssets_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetCompanyAssets(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOpsAssets(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetOpsAssets_WithIncludeDisabled_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetCompanyAssets(It.IsAny<long>(), It.IsAny<long>(), true, It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOpsAssets(100, 200, new RequestParameter(), true);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRolesCount Tests

        [Fact]
        public async Task GetRolesCount_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesCount(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesCount_WithValidEditorPersonaId_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRolesCount(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRolesCount(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public async Task GetRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRights(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithValidEditorPersonaId_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRights(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRights(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region CreateOpsUser Tests

        [Fact]
        public async Task CreateOpsUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.CreateOpsUser(0, 200, new OpsRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateOpsUser_WithZeroUserPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.CreateOpsUser(100, 0, new OpsRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateOpsUser_WithNullRolePropertyList_ReturnsResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOps
                .Setup(x => x.ManageOpsUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), out additionalParams))
                .Returns(string.Empty);

            var result = await _controller.CreateOpsUser(100, 200, null!);

            Assert.IsType<StatusCodeResult>(result);
        }

        [Fact]
        public async Task CreateOpsUser_WhenSuccess_ReturnsCreated()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOps
                .Setup(x => x.ManageOpsUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), out additionalParams))
                .Returns(string.Empty);

            var result = await _controller.CreateOpsUser(100, 200, new OpsRoleAndPropertyList());

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task CreateOpsUser_WhenFails_ReturnsBadRequest()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOps
                .Setup(x => x.ManageOpsUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), out additionalParams))
                .Returns("Error creating user");

            var result = await _controller.CreateOpsUser(100, 200, new OpsRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error creating user", badRequestResult.Value);
        }

        #endregion

        #region UpdateOpsUser Tests

        [Fact]
        public async Task UpdateOpsUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOpsUser(0, 200, new OpsRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOpsUser_WithNullRolePropertyList_ReturnsResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOps
                .Setup(x => x.ManageOpsUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), out additionalParams))
                .Returns(string.Empty);

            var result = await _controller.UpdateOpsUser(100, 200, null!);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateOpsUser_WhenSuccess_ReturnsOk()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOps
                .Setup(x => x.ManageOpsUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), out additionalParams))
                .Returns(string.Empty);

            var result = await _controller.UpdateOpsUser(100, 200, new OpsRoleAndPropertyList());

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateOpsUser_WhenFails_ReturnsBadRequest()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOps
                .Setup(x => x.ManageOpsUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), out additionalParams))
                .Returns("Error updating user");

            var result = await _controller.UpdateOpsUser(100, 200, new OpsRoleAndPropertyList());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region DeleteOpsUser Tests

        [Fact]
        public async Task DeleteOpsUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteOpsUser(0, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteOpsUser_WhenSuccess_ReturnsNoContent()
        {
            _mockManageProductOps
                .Setup(x => x.EnableUser(It.IsAny<long>(), It.IsAny<long>(), false, true))
                .Returns(string.Empty);

            var result = await _controller.DeleteOpsUser(100, 200);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOpsUser_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOps
                .Setup(x => x.EnableUser(It.IsAny<long>(), It.IsAny<long>(), false, true))
                .Returns("Error deleting user");

            var result = await _controller.DeleteOpsUser(100, 200);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateOpsUserStatus Tests

        [Fact]
        public async Task UpdateOpsUserStatus_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOpsUserStatus(0, 200, true);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOpsUserStatus_WhenSuccess_ReturnsOk()
        {
            _mockManageProductOps
                .Setup(x => x.EnableUser(It.IsAny<long>(), It.IsAny<long>(), true, false))
                .Returns(string.Empty);

            var result = await _controller.UpdateOpsUserStatus(100, 200, true);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateOpsUserStatus_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOps
                .Setup(x => x.EnableUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), false))
                .Returns("Error updating status");

            var result = await _controller.UpdateOpsUserStatus(100, 200, true);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateOpsUserStatusMT Tests

        [Fact]
        public async Task UpdateOpsUserStatusMT_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.ChangeUserStatus(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(true);

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await _controller.UpdateOpsUserStatusMT(productUser);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully disabled product user.", okResult.Value);
        }

        [Fact]
        public async Task UpdateOpsUserStatusMT_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOps
                .Setup(x => x.ChangeUserStatus(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(false);

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await _controller.UpdateOpsUserStatusMT(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Disabling Ops user failed.", badRequestResult.Value);
        }

        #endregion

        #region ListOpsMigrationUsers Tests

        [Fact]
        public async Task ListOpsMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListOpsMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListOpsMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetPersona(It.IsAny<long>()))
                .Returns((Persona)null!);

            var result = await _controller.ListOpsMigrationUsers(999999, new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        //[Fact]
        //public async Task ListOpsMigrationUsers_WithValidParameters_ReturnsOkResult()
        //{
        //    _mockManagePersona
        //        .Setup(x => x.GetPersona(It.IsAny<long>()))
        //        .Returns(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

        //    var result = await _controller.ListOpsMigrationUsers(100, new RequestParameter());

        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(new MigrateResponse { Status = true });

            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true }
            };

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(new MigrateResponse { Status = true });

            var result = await _controller.UpdateUsersMigrationStatus(new List<MigrateUser>());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRolesForRight Tests

        [Fact]
        public async Task GetRolesForRight_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesForRight(0, 1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesForRight_WithZeroRightId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesForRight(100, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("rightId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesForRight_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRolesForRight(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRolesForRight(100, 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRightsByRole Tests

        [Fact]
        public async Task GetRightsByRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsByRole(100, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsByRole_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRightsByRole(100, 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region CreateRole Tests

        [Fact]
        public async Task CreateRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.CreateRole(0, new OpsInput { RoleName = "Test" }, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateRole_WithEmptyRoleName_ReturnsBadRequest()
        {
            var result = await _controller.CreateRole(100, new OpsInput { RoleName = "" }, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RoleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateRole_WithWhitespaceRoleName_ReturnsBadRequest()
        {
            var result = await _controller.CreateRole(100, new OpsInput { RoleName = "   " }, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RoleName not supplied.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task CreateRole_WithNullInput_ReturnsResult()
        //{
        //    var result = await _controller.CreateRole(100, null!, 0);

        //    Assert.IsType<BadRequestObjectResult>(result);
        //}

        [Fact]
        public async Task CreateRole_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.CreateRole(It.IsAny<long>(), It.IsAny<OpsInput>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.CreateRole(100, new OpsInput { RoleName = "NewRole" }, 0);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateRole Tests

        [Fact]
        public async Task UpdateRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRole(0, new OpsInput { RoleName = "Test" }, 1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRole(100, new OpsInput { RoleName = "Test" }, 0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRole_WithEmptyRoleName_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRole(100, new OpsInput { RoleName = "" }, 1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RoleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRole_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOps
                .Setup(x => x.CreateRole(It.IsAny<long>(), It.IsAny<OpsInput>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.UpdateRole(100, new OpsInput { RoleName = "UpdatedRole" }, 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetOpsRoles_WithUpfmId_ReturnsResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOpsRoles(100, 200, new RequestParameter(), "", Guid.NewGuid());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetRightsByRole_WithUpfmId_ReturnsResult()
        {
            _mockManageProductOps
                .Setup(x => x.GetRightsByRole(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRightsByRole(100, 1, Guid.NewGuid());

            Assert.NotNull(result);
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
