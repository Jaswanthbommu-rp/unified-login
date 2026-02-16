using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductMarketingCenterControllerTests : ControllerTestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<IManageOrganization> _mockManageOrganization;
        private readonly Mock<IManageProductMarketingCenter> _mockManageProductMarketingCenter;
        private ProductMarketingCenterController _productMarketingCenterController;

        public ProductMarketingCenterControllerTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockManageOrganization = new Mock<IManageOrganization>();
            _mockManageProductMarketingCenter = new Mock<IManageProductMarketingCenter>();

            _productMarketingCenterController = new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object,
                _mockRepository.Object,
                _mockManageOrganization.Object,
                _mockManageProductMarketingCenter.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object,
                _mockRepository.Object,
                _mockManageOrganization.Object,
                _mockManageProductMarketingCenter.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                null!,
                _mockRepository.Object,
                _mockManageOrganization.Object,
                _mockManageProductMarketingCenter.Object));
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockManageOrganization.Object,
                _mockManageProductMarketingCenter.Object));
        }

        [Fact]
        public void Constructor_WithNullManageOrganization_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object,
                _mockRepository.Object,
                null!,
                _mockManageProductMarketingCenter.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductMarketingCenter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductMarketingCenterController(
                MockUserClaimsAccessor.Object,
                _mockRepository.Object,
                _mockManageOrganization.Object,
                null!));
        }

        #endregion

        #region GetMarketingCenterRoles Tests

        [Fact]
        public async Task GetMarketingCenterRoles_WithValidParameters_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.GetMarketingCenterRoles(100, 200, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterRoles_WithZeroEditorPersonaId_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.GetMarketingCenterRoles(0, 200, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterRoles_WithNullDataFilter_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.GetMarketingCenterRoles(100, 200, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterRoles_ReturnsListResponse()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.GetMarketingCenterRoles(100, 200, dataFilter);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(okResult.Value);
        }

        #endregion

        #region GetMarketingCenterProperties Tests

        [Fact]
        public async Task GetMarketingCenterProperties_WithValidParameters_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.GetMarketingCenterProperties(100, 200, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterProperties_WithZeroPersonaIds_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.GetMarketingCenterProperties(0, 0, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterProperties_WithNullDataFilter_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.GetMarketingCenterProperties(100, 200, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region CreateMarketingCenterUser Tests

        [Fact]
        public async Task CreateMarketingCenterUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rolePropertyList = new MarketingCenterRoleAndPropertyList();

            var result = await _productMarketingCenterController.CreateMarketingCenterUser(0, 200, rolePropertyList);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId or userPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateMarketingCenterUser_WithZeroUserPersonaId_ReturnsBadRequest()
        {
            var rolePropertyList = new MarketingCenterRoleAndPropertyList();

            var result = await _productMarketingCenterController.CreateMarketingCenterUser(100, 0, rolePropertyList);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId or userPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateMarketingCenterUser_WithNullRolePropertyList_ReturnsResult()
        {
            var result = await _productMarketingCenterController.CreateMarketingCenterUser(100, 200, null!);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateMarketingCenterUser_WithValidParameters_ReturnsResult()
        {
            var rolePropertyList = new MarketingCenterRoleAndPropertyList
            {
                RoleList = new List<int> { 1, 2, 3 },
                PropertyList = new List<string> { "prop1", "prop2" },
                IsAssignedNewPropertyByDefault = true
            };

            var result = await _productMarketingCenterController.CreateMarketingCenterUser(100, 200, rolePropertyList);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateMarketingCenterUser Tests

        [Fact]
        public async Task UpdateMarketingCenterUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rolePropertyList = new MarketingCenterRoleAndPropertyList();

            var result = await _productMarketingCenterController.UpdateMarketingCenterUser(0, 200, rolePropertyList);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId or userPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateMarketingCenterUser_WithZeroUserPersonaId_ReturnsBadRequest()
        {
            var rolePropertyList = new MarketingCenterRoleAndPropertyList();

            var result = await _productMarketingCenterController.UpdateMarketingCenterUser(100, 0, rolePropertyList);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId or userPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateMarketingCenterUser_WithNullRolePropertyList_ReturnsResult()
        {
            var result = await _productMarketingCenterController.UpdateMarketingCenterUser(100, 200, null!);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateMarketingCenterUser_WithValidParameters_ReturnsResult()
        {
            var rolePropertyList = new MarketingCenterRoleAndPropertyList
            {
                RoleList = new List<int> { 1, 2 },
                PropertyList = new List<string> { "prop1" },
                IsAssignedNewPropertyByDefault = false
            };

            var result = await _productMarketingCenterController.UpdateMarketingCenterUser(100, 200, rolePropertyList);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateMarketingCenterUserStatus Tests

        [Fact]
        public async Task UpdateMarketingCenterUserStatus_WithValidProductUser_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com",
                IsAssigned = false
            };

            var result = await _productMarketingCenterController.UpdateMarketingCenterUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateMarketingCenterUserStatus_WithIsAssignedTrue_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com",
                IsAssigned = true
            };

            var result = await _productMarketingCenterController.UpdateMarketingCenterUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateMarketingCenterUserStatus_WithNullUserName_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = null!
            };

            var result = await _productMarketingCenterController.UpdateMarketingCenterUserStatus(productUser);

            Assert.NotNull(result);
        }

        #endregion

        #region GetRolesCount Tests

        [Fact]
        public async Task GetRolesCount_WithValidEditorPersonaId_ReturnsResult()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesCount(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _productMarketingCenterController.GetRolesCount(100);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRolesCount_WhenResultIsError_ReturnsBadRequest()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesCount(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = true });

            var result = await _productMarketingCenterController.GetRolesCount(100);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        //[Fact]
        //public async Task GetRolesCount_WithZeroEditorPersonaIdAndNoUpfmId_ReturnsResult()
        //{
        //    _mockManageProductMarketingCenter
        //        .Setup(x => x.GetRolesCount(It.IsAny<long>()))
        //        .Returns(new ListResponse { IsError = false });

        //    var result = await _productMarketingCenterController.GetRolesCount(0);

        //    Assert.NotNull(result);
        //}

        [Fact]
        public async Task GetRolesCount_WithUpfmId_ReturnsResult()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRolesCount(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _productMarketingCenterController.GetRolesCount(100, Guid.NewGuid());

            Assert.NotNull(result);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public async Task GetRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productMarketingCenterController.GetRights(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithValidEditorPersonaId_ReturnsResult()
        {
            var result = await _productMarketingCenterController.GetRights(100);

            Assert.NotNull(result);
        }

        #endregion

        #region DeleteMarketingCenterRole Tests

        [Fact]
        public async Task DeleteMarketingCenterRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productMarketingCenterController.DeleteMarketingCenterRole(0, 1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteMarketingCenterRole_WithValidParameters_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.DeleteMarketingCenterRole(100, 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateMarketingCenterRoleStatus Tests

        [Fact]
        public async Task UpdateMarketingCenterRoleStatus_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productMarketingCenterController.UpdateMarketingCenterRoleStatus(0, 1, true);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateMarketingCenterRoleStatus_WithValidParameters_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.UpdateMarketingCenterRoleStatus(100, 1, true);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateMarketingCenterRoleStatus_WithIsActiveFalse_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.UpdateMarketingCenterRoleStatus(100, 1, false);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRolesForRightId Tests

        [Fact]
        public async Task GetRolesForRightId_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _productMarketingCenterController.GetRolesForRightId(0, 1);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesForRightId_WithValidParameters_ReturnsResult()
        {
            var result = await _productMarketingCenterController.GetRolesForRightId(100, 1);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateRolesForRight Tests

        [Fact]
        public async Task UpdateRolesForRight_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var roleList = new List<string> { "role1", "role2" };

            var result = await _productMarketingCenterController.UpdateRolesForRight(0, 1, roleList);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRolesForRight_WithValidParameters_ReturnsResult()
        {
            var roleList = new List<string> { "role1", "role2" };

            var result = await _productMarketingCenterController.UpdateRolesForRight(100, 1, roleList);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRolesForRight_WithEmptyRoleList_ReturnsResult()
        {
            var roleList = new List<string>();

            var result = await _productMarketingCenterController.UpdateRolesForRight(100, 1, roleList);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRolesForRight_WithNullRoleList_ReturnsResult()
        {
            var result = await _productMarketingCenterController.UpdateRolesForRight(100, 1, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region GetRightsForRoleId Tests

        [Fact]
        public async Task GetRightsForRoleId_WithValidEditorPersonaId_ReturnsResult()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsForRoleId(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _productMarketingCenterController.GetRightsForRoleId(100, 1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetRightsForRoleId_WithZeroRoleId_ReturnsResult()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsForRoleId(It.IsAny<long>(), 0))
                .Returns(new ListResponse { IsError = false });

            var result = await _productMarketingCenterController.GetRightsForRoleId(100, 0);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetRightsForRoleId_WithUpfmId_ReturnsResult()
        {
            _mockManageProductMarketingCenter
                .Setup(x => x.GetRightsForRoleId(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _productMarketingCenterController.GetRightsForRoleId(100, 1, Guid.NewGuid());

            Assert.NotNull(result);
        }

        #endregion

        #region CreateNewMCRoleWithRights Tests

        [Fact]
        public async Task CreateNewMCRoleWithRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var mcRole = new MCRole { Name = "Test Role", Rights = new List<int> { 1, 2 } };

            var result = await _productMarketingCenterController.CreateNewMCRoleWithRights(0, mcRole);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateNewMCRoleWithRights_WithValidParameters_ReturnsResult()
        {
            var mcRole = new MCRole
            {
                Name = "Test Role",
                Description = "Test Description",
                Rights = new List<int> { 1, 2, 3 },
                Active = true
            };

            var result = await _productMarketingCenterController.CreateNewMCRoleWithRights(100, mcRole);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateNewMCRoleWithRights_WithNullMcRole_ReturnsResult()
        {
            var result = await _productMarketingCenterController.CreateNewMCRoleWithRights(100, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateMCRoleWithRights Tests

        [Fact]
        public async Task UpdateMCRoleWithRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var mcRole = new MCRole { Id = 1, Name = "Updated Role" };

            var result = await _productMarketingCenterController.UpdateMCRoleWithRights(0, mcRole);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid editorPersonaId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateMCRoleWithRights_WithValidParameters_ReturnsResult()
        {
            var mcRole = new MCRole
            {
                Id = 1,
                Name = "Updated Role",
                Description = "Updated Description",
                Rights = new List<int> { 1, 2 },
                Active = true
            };

            var result = await _productMarketingCenterController.UpdateMCRoleWithRights(100, mcRole);

            Assert.NotNull(result);
        }

        #endregion

        #region ListMarketingCenterMigrationUsers Tests

        [Fact]
        public async Task ListMarketingCenterMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.ListMarketingCenterMigrationUsers(0, dataFilter);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListMarketingCenterMigrationUsers_WhenPersonaNotFound_ReturnsBadRequest()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.ListMarketingCenterMigrationUsers(999999, dataFilter);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not found.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListMarketingCenterMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            var result = await _productMarketingCenterController.ListMarketingCenterMigrationUsers(100, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            var result = await _productMarketingCenterController.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>();

            var result = await _productMarketingCenterController.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.UpdateUsersMigrationStatus(null!);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetMarketingCenterRoles_WithMaxLongValues_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.GetMarketingCenterRoles(long.MaxValue, long.MaxValue, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetMarketingCenterProperties_WithNegativePersonaIds_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productMarketingCenterController.GetMarketingCenterProperties(-1, -1, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteMarketingCenterRole_WithNegativeRoleId_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.DeleteMarketingCenterRole(100, -1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateMarketingCenterRoleStatus_WithZeroRoleId_ReturnsOkResult()
        {
            var result = await _productMarketingCenterController.UpdateMarketingCenterRoleStatus(100, 0, true);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _productMarketingCenterController = null!;
            base.Dispose();
        }

        #endregion
    }
}
