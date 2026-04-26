using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductMarketingCenterControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageProductMarketingCenterAsync> _mockManageProductMarketingCenter;
        private readonly Mock<IManageOrganization> _mockManageOrganization;
        private readonly Mock<IManagePersonaAsync> _mockManagePersona;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IManageUserRoleRight> _mockManageUserRoleRight;
        private ProductMarketingCenterController _controller;

        #endregion

        #region Constructor

        public ProductMarketingCenterControllerTests()
        {
            _mockManageProductMarketingCenter = new Mock<IManageProductMarketingCenterAsync>();
            _mockManageOrganization = new Mock<IManageOrganization>();
            _mockManagePersona = new Mock<IManagePersonaAsync>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockManageUserRoleRight = new Mock<IManageUserRoleRight>();

            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse());

            _mockManageProductMarketingCenter
                .Setup(x => x.GetPropertiesAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse());

            _mockManageProductMarketingCenter
                .Setup(x => x.ManageMarketingCenterUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string.Empty, new List<AdditionalParameters>()));

            _mockManageProductMarketingCenter
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesCountAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.DeleteRoleAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse());

            _mockManageProductMarketingCenter
                .Setup(x => x.UpdateRoleStatusAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse());

            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesForRightIdAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.UpdateRolesForRightAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsForRoleIdAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.CreateNewMCRoleWithRightsAsync(It.IsAny<long>(), It.IsAny<MCRole>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.UpdateMCRoleWithRightsAsync(It.IsAny<long>(), It.IsAny<MCRole>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.GetMigrationUsersAsync(It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            _mockManageProductMarketingCenter
                .Setup(x => x.UpdateUsersMigrationStatusAsync(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MigrateResponse());

            _controller = CreateController();
        }

        private ProductMarketingCenterController CreateController() =>
            new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object,
                _mockManageProductMarketingCenter.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
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
        public void Constructor_WithNullManageProductMarketingCenter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object, null!, _mockManageOrganization.Object,
                _mockManagePersona.Object, _mockManagePerson.Object,
                _mockManageUserLogin.Object, _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageOrganization_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object, _mockManageProductMarketingCenter.Object, null!,
                _mockManagePersona.Object, _mockManagePerson.Object,
                _mockManageUserLogin.Object, _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object, _mockManageProductMarketingCenter.Object,
                _mockManageOrganization.Object, null!, _mockManagePerson.Object,
                _mockManageUserLogin.Object, _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePerson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object, _mockManageProductMarketingCenter.Object,
                _mockManageOrganization.Object, _mockManagePersona.Object, null!,
                _mockManageUserLogin.Object, _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserLogin_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object, _mockManageProductMarketingCenter.Object,
                _mockManageOrganization.Object, _mockManagePersona.Object, _mockManagePerson.Object,
                null!, _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserRoleRight_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object, _mockManageProductMarketingCenter.Object,
                _mockManageOrganization.Object, _mockManagePersona.Object, _mockManagePerson.Object,
                _mockManageUserLogin.Object, null!));
        }

        #endregion

        #region GetMarketingCenterRoles Tests

        [Fact]
        public async Task GetMarketingCenterRoles_WithValidParameters_ReturnsOk()
        {
            var result = await _controller.GetMarketingCenterRoles(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterRoles_ReturnsListResponse()
        {
            var result = await _controller.GetMarketingCenterRoles(100, 200, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(ok.Value);
        }

        [Fact]
        public async Task GetMarketingCenterRoles_WithZeroEditorPersonaId_ReturnsOk()
        {
            var result = await _controller.GetMarketingCenterRoles(0, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetMarketingCenterProperties Tests

        [Fact]
        public async Task GetMarketingCenterProperties_WithValidParameters_ReturnsOk()
        {
            var result = await _controller.GetMarketingCenterProperties(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterProperties_ReturnsListResponse()
        {
            var result = await _controller.GetMarketingCenterProperties(100, 200, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(ok.Value);
        }

        #endregion

        #region CreateMarketingCenterUser Tests

        [Fact]
        public async Task CreateMarketingCenterUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.CreateMarketingCenterUser(0, 200, new MarketingCenterRoleAndPropertyList());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId or userPersonaId", bad.Value);
        }

        [Fact]
        public async Task CreateMarketingCenterUser_WithZeroUserPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.CreateMarketingCenterUser(100, 0, new MarketingCenterRoleAndPropertyList());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId or userPersonaId", bad.Value);
        }

        [Fact]
        public async Task CreateMarketingCenterUser_WhenSuccess_ReturnsCreated()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.ManageMarketingCenterUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string.Empty, new List<AdditionalParameters>()));

            var result = await _controller.CreateMarketingCenterUser(100, 200, new MarketingCenterRoleAndPropertyList());

            Assert.Equal(201, ((StatusCodeResult)result).StatusCode);
        }

        [Fact]
        public async Task CreateMarketingCenterUser_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.ManageMarketingCenterUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(("User creation failed", new List<AdditionalParameters>()));

            var result = await _controller.CreateMarketingCenterUser(100, 200, new MarketingCenterRoleAndPropertyList());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateMarketingCenterUser Tests

        [Fact]
        public async Task UpdateMarketingCenterUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateMarketingCenterUser(0, 200, new MarketingCenterRoleAndPropertyList());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId or userPersonaId", bad.Value);
        }

        [Fact]
        public async Task UpdateMarketingCenterUser_WhenSuccess_ReturnsOk()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.ManageMarketingCenterUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string.Empty, new List<AdditionalParameters>()));

            var result = await _controller.UpdateMarketingCenterUser(100, 200, new MarketingCenterRoleAndPropertyList());

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateMarketingCenterUser_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.ManageMarketingCenterUserAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<int>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(("Update failed", new List<AdditionalParameters>()));

            var result = await _controller.UpdateMarketingCenterUser(100, 200, new MarketingCenterRoleAndPropertyList());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateMarketingCenterUserStatus Tests

        [Fact]
        public async Task UpdateMarketingCenterUserStatus_WhenSuccess_ReturnsOk()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateMarketingCenterUserStatus(
                new ProductUser { UserId = 123, UserName = "test@test.com" });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully disabled product user.", ok.Value);
        }

        [Fact]
        public async Task UpdateMarketingCenterUserStatus_WhenFails_WithIsAssignedFalse_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateMarketingCenterUserStatus(
                new ProductUser { UserId = 123, UserName = "test@test.com", IsAssigned = false });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Deactivate MarketingCenter user failed.", bad.Value);
        }

        [Fact]
        public async Task UpdateMarketingCenterUserStatus_WhenFails_WithIsAssignedTrue_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.ChangeUserStatusAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateMarketingCenterUserStatus(
                new ProductUser { UserId = 123, UserName = "test@test.com", IsAssigned = true });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Activate MarketingCenter user failed.", bad.Value);
        }

        #endregion

        #region GetRolesCount Tests

        [Fact]
        public async Task GetRolesCount_WithValidEditorPersonaId_WhenNotError_ReturnsOk()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesCountAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            var result = await _controller.GetRolesCount(100);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRolesCount_WhenIsError_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesCountAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.GetRolesCount(100);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public async Task GetRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRights(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", bad.Value);
        }

        [Fact]
        public async Task GetRights_WithValidEditorPersonaId_WhenNotError_ReturnsOk()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            var result = await _controller.GetRights(100);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRights_WhenIsError_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.GetRights(100);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region DeleteMarketingCenterRole Tests

        [Fact]
        public async Task DeleteMarketingCenterRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteMarketingCenterRole(0, 1);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", bad.Value);
        }

        [Fact]
        public async Task DeleteMarketingCenterRole_WithValidParameters_ReturnsOk()
        {
            var result = await _controller.DeleteMarketingCenterRole(100, 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateMarketingCenterRoleStatus Tests

        [Fact]
        public async Task UpdateMarketingCenterRoleStatus_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateMarketingCenterRoleStatus(0, 1, true);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", bad.Value);
        }

        [Fact]
        public async Task UpdateMarketingCenterRoleStatus_WithIsActiveTrue_ReturnsOk()
        {
            var result = await _controller.UpdateMarketingCenterRoleStatus(100, 1, true);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateMarketingCenterRoleStatus_WithIsActiveFalse_ReturnsOk()
        {
            var result = await _controller.UpdateMarketingCenterRoleStatus(100, 1, false);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRolesForRightId Tests

        [Fact]
        public async Task GetRolesForRightId_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRolesForRightId(0, 1);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", bad.Value);
        }

        [Fact]
        public async Task GetRolesForRightId_WithValidParameters_ReturnsOk()
        {
            var result = await _controller.GetRolesForRightId(100, 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateRolesForRight Tests

        [Fact]
        public async Task UpdateRolesForRight_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateRolesForRight(0, 1, new List<string> { "role1" });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", bad.Value);
        }

        [Fact]
        public async Task UpdateRolesForRight_WhenSuccess_ReturnsOk()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.UpdateRolesForRightAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            var result = await _controller.UpdateRolesForRight(100, 1, new List<string> { "role1" });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Roles Updated", ok.Value);
        }

        [Fact]
        public async Task UpdateRolesForRight_WhenIsError_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.UpdateRolesForRightAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.UpdateRolesForRight(100, 1, new List<string> { "role1" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetRightsForRoleId Tests

        [Fact]
        public async Task GetRightsForRoleId_WithValidEditorPersonaId_ReturnsOk()
        {
            var result = await _controller.GetRightsForRoleId(100, 1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRightsForRoleId_WhenIsError_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsForRoleIdAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.GetRightsForRoleId(100, 1);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region CreateNewMCRoleWithRights Tests

        [Fact]
        public async Task CreateNewMCRoleWithRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.CreateNewMCRoleWithRights(0, new MCRole { Name = "Test" });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", bad.Value);
        }

        [Fact]
        public async Task CreateNewMCRoleWithRights_WhenSuccess_ReturnsOk()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.CreateNewMCRoleWithRightsAsync(It.IsAny<long>(), It.IsAny<MCRole>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            var result = await _controller.CreateNewMCRoleWithRights(100, new MCRole { Name = "TestRole" });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateNewMCRoleWithRights_WhenIsError_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.CreateNewMCRoleWithRightsAsync(It.IsAny<long>(), It.IsAny<MCRole>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.CreateNewMCRoleWithRights(100, new MCRole { Name = "TestRole" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateMCRoleWithRights Tests

        [Fact]
        public async Task UpdateMCRoleWithRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateMCRoleWithRights(0, new MCRole { Id = 1 });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", bad.Value);
        }

        [Fact]
        public async Task UpdateMCRoleWithRights_WhenSuccess_ReturnsOk()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.UpdateMCRoleWithRightsAsync(It.IsAny<long>(), It.IsAny<MCRole>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            var result = await _controller.UpdateMCRoleWithRights(100, new MCRole { Id = 1, Name = "Updated" });

            Assert.Equal(200, ((ObjectResult)result).StatusCode);
        }

        #endregion

        #region ListMarketingCenterMigrationUsers Tests

        [Fact]
        public async Task ListMarketingCenterMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListMarketingCenterMigrationUsers(0, new RequestParameter());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", bad.Value);
        }

        [Fact]
        public async Task ListMarketingCenterMigrationUsers_WhenPersonaNull_ReturnsBadRequest()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListMarketingCenterMigrationUsers(100, new RequestParameter());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not found.", bad.Value);
        }

        [Fact]
        public async Task ListMarketingCenterMigrationUsers_WhenSuccess_ReturnsOk()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            _mockManageProductMarketingCenter
                .Setup(x => x.GetMigrationUsersAsync(It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = false });

            var result = await _controller.ListMarketingCenterMigrationUsers(100, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListMarketingCenterMigrationUsers_WhenIsError_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            _mockManageProductMarketingCenter
                .Setup(x => x.GetMigrationUsersAsync(It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.ListMarketingCenterMigrationUsers(100, new RequestParameter());

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

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
