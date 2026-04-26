using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductOneSiteAccountingControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductOneSiteAccounting> _mockManageProductOneSiteAccounting;
        private readonly Mock<IManageProductOneSiteAccountingAsync> _mockManageProductOneSiteAccountingAsync;
        private readonly Mock<IManageOrganization> _mockManageOrganization;
        private readonly Mock<IManagePersona> _mockManagePersona;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IManageUserRoleRight> _mockManageUserRoleRight;
        private ProductOneSiteAccountingController _controller;

        public ProductOneSiteAccountingControllerTests()
        {
            _mockManageProductOneSiteAccounting = new Mock<IManageProductOneSiteAccounting>();
            _mockManageProductOneSiteAccountingAsync = new Mock<IManageProductOneSiteAccountingAsync>();
            _mockManageOrganization = new Mock<IManageOrganization>();
            _mockManagePersona = new Mock<IManagePersona>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockManageUserRoleRight = new Mock<IManageUserRoleRight>();

            _controller = new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
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
            var controller = new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
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
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                null!,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductOneSiteAccounting_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockManageProductOneSiteAccountingAsync.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductOneSiteAccountingAsync_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
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
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
                null!,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
                _mockManageOrganization.Object,
                null!,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePerson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                null!,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserLogin_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                null!,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserRoleRight_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteAccountingController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAccounting.Object,
                _mockManageProductOneSiteAccountingAsync.Object,
                _mockManageOrganization.Object,
                _mockManagePersona.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                null!));
        }

        #endregion

        #region GetUserProperties Tests

        [Fact]
        public async Task GetUserProperties_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetUserPropertiesNew(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetUserProperties(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUserProperties_WithZeroPersonaIds_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetUserPropertiesNew(0, 0, It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetUserProperties(0, 0, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetUserCompanies Tests

        [Fact]
        public async Task GetUserCompanies_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetUserCompanies(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetUserCompanies(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetUserRoles Tests

        [Fact]
        public async Task GetUserRoles_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetUserRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetUserRoles(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateUserProperties Tests

        [Fact]
        public async Task UpdateUserProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.UpdateUserProperties(0, 200, new List<string> { "prop1" });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor and user persona IDs are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserProperties_WithZeroUserPersonaId_ReturnsBadRequest()
        {
            var result = _controller.UpdateUserProperties(100, 0, new List<string> { "prop1" });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor and user persona IDs are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserProperties_WithNullPropertyList_ReturnsBadRequest()
        {
            var result = _controller.UpdateUserProperties(100, 200, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserProperties_WithEmptyPropertyList_ReturnsBadRequest()
        {
            var result = _controller.UpdateUserProperties(100, 200, new List<string>());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserProperties_WithValidParameters_ReturnsOkResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSiteAccounting
                .Setup(x => x.UpdatePropertiesToUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<bool>(), out additionalParams, It.IsAny<BatchProcessType>()))
                .Returns(string.Empty);

            var result = _controller.UpdateUserProperties(100, 200, new List<string> { "prop1", "prop2" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Records Updated", okResult.Value);
        }

        [Fact]
        public async Task UpdateUserProperties_WhenUpdateFails_ReturnsNoContent()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSiteAccounting
                .Setup(x => x.UpdatePropertiesToUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<bool>(), out additionalParams, It.IsAny<BatchProcessType>()))
                .Returns("Error occurred");

            var result = _controller.UpdateUserProperties(100, 200, new List<string> { "prop1" });

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(204, statusCodeResult.StatusCode);
        }

        #endregion

        #region UpdateUserRoles Tests

        [Fact]
        public async Task UpdateUserRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.UpdateUserRoles(0, 200, new List<string> { "role1" });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor and user persona IDs are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserRoles_WithNullRoleList_ReturnsBadRequest()
        {
            var result = _controller.UpdateUserRoles(100, 200, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUserRoles_WithValidParameters_ReturnsOkResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSiteAccounting
                .Setup(x => x.UpdateRolesToUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<bool>(), out additionalParams, It.IsAny<BatchProcessType>()))
                .Returns(string.Empty);

            var result = _controller.UpdateUserRoles(100, 200, new List<string> { "role1" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Records Updated", okResult.Value);
        }

        #endregion

        #region CreateAccountingUser Tests

        [Fact]
        public async Task CreateAccountingUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.CreateAccountingUser(0, 200, new AccountingRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor and user persona IDs are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateAccountingUser_WithNullRolePropertyList_ReturnsResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSiteAccounting
                .Setup(x => x.ManageAccountingUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), out additionalParams, It.IsAny<BatchProcessType>()))
                .Returns(string.Empty);

            var result = _controller.CreateAccountingUser(100, 200, null!);

            Assert.IsType<StatusCodeResult>(result);
        }

        [Fact]
        public async Task CreateAccountingUser_WhenSuccess_ReturnsCreated()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSiteAccounting
                .Setup(x => x.ManageAccountingUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), out additionalParams, It.IsAny<BatchProcessType>()))
                .Returns(string.Empty);

            var result = _controller.CreateAccountingUser(100, 200, new AccountingRoleAndPropertyList());

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task CreateAccountingUser_WhenFails_ReturnsBadRequest()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSiteAccounting
                .Setup(x => x.ManageAccountingUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), out additionalParams, It.IsAny<BatchProcessType>()))
                .Returns("Error creating user");

            var result = _controller.CreateAccountingUser(100, 200, new AccountingRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error creating user", badRequestResult.Value);
        }

        #endregion

        #region UpdateAccountingUser Tests

        [Fact]
        public async Task UpdateAccountingUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.UpdateAccountingUser(0, 200, new AccountingRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor and user persona IDs are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAccountingUser_WhenSuccess_ReturnsOk()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSiteAccounting
                .Setup(x => x.ManageAccountingUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), out additionalParams, It.IsAny<BatchProcessType>()))
                .Returns(string.Empty);

            var result = _controller.UpdateAccountingUser(100, 200, new AccountingRoleAndPropertyList());

            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region DeleteAccountingUser Tests

        [Fact]
        public async Task DeleteAccountingUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.DeleteAccountingUser(0, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor and user persona IDs are required.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteAccountingUser_WhenSuccess_ReturnsNoContent()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.DeleteAccountingUser(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(string.Empty);

            var result = _controller.DeleteAccountingUser(100, 200);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAccountingUser_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.DeleteAccountingUser(It.IsAny<long>(), It.IsAny<long>()))
                .Returns("Error deleting user");

            var result = _controller.DeleteAccountingUser(100, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error deleting user", badRequestResult.Value);
        }

        #endregion

        #region UpdateAccountingUserStatus Tests

        [Fact]
        public async Task UpdateAccountingUserStatus_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateAccountingUserStatus(0, 200, true);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor persona ID is required.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task UpdateAccountingUserStatus_WithValidParameters_ReturnsResult()
        //{
        //    var result = await _controller.UpdateAccountingUserStatus(100, 200, true);

        //    Assert.NotNull(result);
        //}

        #endregion

        #region UpdateAccountingUserClaimStatus Tests

        [Fact]
        public async Task UpdateAccountingUserClaimStatus_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateAccountingUserClaimStatus(0, 200, true);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Editor and user persona IDs are required.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task UpdateAccountingUserClaimStatus_WithValidParameters_ReturnsResult()
        //{
        //    var result = await _controller.UpdateAccountingUserClaimStatus(100, 200, true);

        //    Assert.NotNull(result);
        //}

        #endregion

        #region ListOneSiteAccountingMigrationUsers Tests

        [Fact]
        public async Task ListOneSiteAccountingMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListOneSiteAccountingMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListOneSiteAccountingMigrationUsers_WhenPersonaNotFound_ReturnsBadRequest()
        {
            _mockManagePersona
                .Setup(x => x.GetPersona(It.IsAny<long>()))
                .Returns((Persona)null!);

            var result = await _controller.ListOneSiteAccountingMigrationUsers(999999, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not found.", badRequestResult.Value);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        //[Fact]
        //public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        //{
        //    var migrateUsers = new List<MigrateUser>
        //    {
        //        new MigrateUser { UserId = "user1", UsingUnifiedLogin = true }
        //    };

        //    var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        //{
        //    var result = await _controller.UpdateUsersMigrationStatus(new List<MigrateUser>());

        //    Assert.IsType<OkObjectResult>(result);
        //}

        //#endregion

        //#region GetRolesCount Tests

        //[Fact]
        //public async Task GetRolesCount_WithValidEditorPersonaId_ReturnsOkResult()
        //{
        //    var result = await _controller.GetRolesCount(100, new RequestParameter());

        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetRolesCount_WithUpfmId_ReturnsResult()
        //{
        //    var result = await _controller.GetRolesCount(100, new RequestParameter(), Guid.NewGuid());

        //    Assert.NotNull(result);
        //}

        #endregion

        #region GetAllRoles Tests

        [Fact]
        public async Task GetAllRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.GetAllRoles(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAllRoles_WithValidEditorPersonaId_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetAllRoles(It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetAllRoles(100, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public async Task GetRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.GetRights(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithValidEditorPersonaId_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetRights(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetRights(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetApplications Tests

        [Fact]
        public async Task GetApplications_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.GetApplications(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetApplications_WithValidEditorPersonaId_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetApplications(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetApplications(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRolesForRight Tests

        [Fact]
        public async Task GetRolesForRight_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.GetRolesForRight(0, new RequestParameter(), 1, true, "{}");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesForRight_WithZeroRightId_ReturnsBadRequest()
        {
            var result = _controller.GetRolesForRight(100, new RequestParameter(), 0, true, "{}");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("rightId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesForRight_WithNullRight_ReturnsBadRequest()
        {
            var result = _controller.GetRolesForRight(100, new RequestParameter(), 1, true, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("right not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesForRight_WithEmptyRight_ReturnsBadRequest()
        {
            var result = _controller.GetRolesForRight(100, new RequestParameter(), 1, true, "   ");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("right not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRolesForRight_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.GetRolesForRight(It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<ProductRightAcct>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.GetRolesForRight(100, new RequestParameter(), 1, true, "{}");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateRolesForRight Tests

        [Fact]
        public async Task UpdateRolesForRight_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rolesToAddRemove = new RolesAddRemove { RolesToAdd = new List<ProductRoleAcct>(), RolesToDelete = new List<ProductRoleAcct>() };

            var result = _controller.UpdateRolesForRight(0, 1, rolesToAddRemove, "{}");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRolesForRight_WithZeroRightId_ReturnsBadRequest()
        {
            var rolesToAddRemove = new RolesAddRemove { RolesToAdd = new List<ProductRoleAcct>(), RolesToDelete = new List<ProductRoleAcct>() };

            var result = _controller.UpdateRolesForRight(100, 0, rolesToAddRemove, "{}");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("rightId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRolesForRight_WithEmptyRoleLists_ReturnsBadRequest()
        {
            var rolesToAddRemove = new RolesAddRemove { RolesToAdd = new List<ProductRoleAcct>(), RolesToDelete = new List<ProductRoleAcct>() };

            var result = _controller.UpdateRolesForRight(100, 1, rolesToAddRemove, "{}");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Roles not supplied to Add or Remove.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRolesForRight_WithNullRight_ReturnsBadRequest()
        {
            var rolesToAddRemove = new RolesAddRemove { RolesToAdd = new List<ProductRoleAcct> { new ProductRoleAcct() }, RolesToDelete = new List<ProductRoleAcct>() };

            var result = _controller.UpdateRolesForRight(100, 1, rolesToAddRemove, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("right not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRolesForRight_WithValidParameters_ReturnsOkResult()
        {
            var rolesToAddRemove = new RolesAddRemove { RolesToAdd = new List<ProductRoleAcct> { new ProductRoleAcct() }, RolesToDelete = new List<ProductRoleAcct>() };

            _mockManageProductOneSiteAccounting
                .Setup(x => x.UpdateRolesForRight(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<ProductRoleAcct>>(), It.IsAny<List<ProductRoleAcct>>(), It.IsAny<ProductRightAcct>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.UpdateRolesForRight(100, 1, rolesToAddRemove, "{}");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRightsForRole Tests

        [Fact]
        public async Task GetRightsForRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsForRole(100, new RequestParameter(), 0, "RoleName");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithNullRoleName_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsForRole(100, new RequestParameter(), 1, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithEmptyRoleName_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsForRole(100, new RequestParameter(), 1, "   ");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleName not supplied.", badRequestResult.Value);
        }

        #endregion

        #region UpdateRightsForRole Tests

        [Fact]
        public async Task UpdateRightsForRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rightsToAddRemove = new RightsAddRemove { RightsToAdd = new List<ProductRightAcct>(), RightsToRemove = new List<ProductRightAcct>() };

            var result = _controller.UpdateRightsForRole(0, 1, "RoleName", rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRightsForRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var rightsToAddRemove = new RightsAddRemove { RightsToAdd = new List<ProductRightAcct>(), RightsToRemove = new List<ProductRightAcct>() };

            var result = _controller.UpdateRightsForRole(100, 0, "RoleName", rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRightsForRole_WithEmptyRightsLists_ReturnsBadRequest()
        {
            var rightsToAddRemove = new RightsAddRemove { RightsToAdd = new List<ProductRightAcct>(), RightsToRemove = new List<ProductRightAcct>() };

            var result = _controller.UpdateRightsForRole(100, 1, "RoleName", rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Rights not supplied to Add or Remove.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRightsForRole_WithValidParameters_ReturnsOkResult()
        {
            var rightsToAddRemove = new RightsAddRemove { RightsToAdd = new List<ProductRightAcct> { new ProductRightAcct() }, RightsToRemove = new List<ProductRightAcct>() };

            _mockManageProductOneSiteAccounting
                .Setup(x => x.UpdateRightsForRole(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<List<ProductRightAcct>>(), It.IsAny<List<ProductRightAcct>>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.UpdateRightsForRole(100, 1, "RoleName", rightsToAddRemove);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region CreateRole Tests

        [Fact]
        public async Task CreateRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.CreateRole(0, "NewRole");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateRole_WithNullRoleName_ReturnsBadRequest()
        {
            var result = _controller.CreateRole(100, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateRole_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.CreateRole(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.CreateRole(100, "NewRole");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateRole_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.CreateRole(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = true, ErrorReason = "Role already exists" });

            var result = _controller.CreateRole(100, "ExistingRole");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region CloneRole Tests

        [Fact]
        public async Task CloneRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.CloneRole(0, "InheritedRole", "NewRole");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CloneRole_WithNullInheritedRoleName_ReturnsBadRequest()
        {
            var result = _controller.CloneRole(100, null!, "NewRole");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("inheritRoleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CloneRole_WithNullRoleName_ReturnsBadRequest()
        {
            var result = _controller.CloneRole(100, "InheritedRole", null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CloneRole_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.CloneRole(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.CloneRole(100, "InheritedRole", "NewRole");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region DeleteRole Tests

        [Fact]
        public async Task DeleteRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = _controller.DeleteRole(0, 1, "RoleName");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = _controller.DeleteRole(100, 0, "RoleName");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRole_WithNullRoleName_ReturnsBadRequest()
        {
            var result = _controller.DeleteRole(100, 1, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleName not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRole_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSiteAccounting
                .Setup(x => x.DeleteRole(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = _controller.DeleteRole(100, 1, "RoleName");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateAccountingUserStatus (MT) Tests

        //[Fact]
        //public async Task UpdateAccountingUserStatusMT_WithValidProductUser_ReturnsResult()
        //{
        //    var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

        //    var result = await _controller.UpdateAccountingUserStatus(productUser);

        //    Assert.NotNull(result);
        //}

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
