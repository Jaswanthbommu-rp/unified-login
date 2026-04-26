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
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.OneSite;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductOneSiteControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductOneSite> _mockManageProductOneSite;
        private readonly Mock<IManageOrganization> _mockManageOrganization;
        private readonly Mock<IManagePersonaAsync> _mockManagePersonaAsync;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IManageUserRoleRight> _mockManageUserRoleRight;
        private readonly Mock<IManageProductOneSiteAsync> _mockManageProductOneSiteAsync;
        private ProductOneSiteController _controller;

        public ProductOneSiteControllerTests()
        {
            _mockManageProductOneSite = new Mock<IManageProductOneSite>();
            _mockManageOrganization = new Mock<IManageOrganization>();
            _mockManagePersonaAsync = new Mock<IManagePersonaAsync>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockManageUserRoleRight = new Mock<IManageUserRoleRight>();
            _mockManageProductOneSiteAsync = new Mock<IManageProductOneSiteAsync>();

            _controller = new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSite.Object,
                _mockManageOrganization.Object,
                _mockManagePersonaAsync.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSite.Object,
                _mockManageOrganization.Object,
                _mockManagePersonaAsync.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteController(
                null!,
                _mockManageProductOneSite.Object,
                _mockManageOrganization.Object,
                _mockManagePersonaAsync.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductOneSite_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockManageOrganization.Object,
                _mockManagePersonaAsync.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManageOrganization_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSite.Object,
                null!,
                _mockManagePersonaAsync.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSite.Object,
                _mockManageOrganization.Object,
                null!,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePerson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSite.Object,
                _mockManageOrganization.Object,
                _mockManagePersonaAsync.Object,
                null!,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserLogin_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSite.Object,
                _mockManageOrganization.Object,
                _mockManagePersonaAsync.Object,
                _mockManagePerson.Object,
                null!,
                _mockManageUserRoleRight.Object,
                _mockManageProductOneSiteAsync.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserRoleRight_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOneSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSite.Object,
                _mockManageOrganization.Object,
                _mockManagePersonaAsync.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                null!,
                _mockManageProductOneSiteAsync.Object));
        }

        #endregion

        #region GetOneSitePropertyList Tests

        [Fact]
        public async Task GetOneSitePropertyList_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetOneSitePropertyList(0, 200, true, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetOneSitePropertyList_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetOneSitePropertyList(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSitePropertyList(100, 200, true, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetOneSitePropertyUsersList Tests

        [Fact]
        public async Task GetOneSitePropertyUsersList_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetUsersForProperty(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSitePropertyUsersList(100, new RequestParameter(), 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateOneSiteUserProperties Tests

        [Fact]
        public async Task UpdateOneSiteUserProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteUserProperties(0, 200, new List<string> { "prop1" });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUserProperties_WithEmptyPropertyList_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteUserProperties(100, 200, new List<string>());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUserProperties_WithValidParameters_ReturnsOkResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSite
                .Setup(x => x.UpdatePropertiesForUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), out additionalParams))
                .Returns("5");

            var result = await _controller.UpdateOneSiteUserProperties(100, 200, new List<string> { "prop1" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("5 Records Updated", okResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUserProperties_WhenNoRecordsUpdated_ReturnsNoContent()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSite
                .Setup(x => x.UpdatePropertiesForUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), out additionalParams))
                .Returns(string.Empty);

            var result = await _controller.UpdateOneSiteUserProperties(100, 200, new List<string> { "prop1" });

            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region GetOneSiteRoleUsersList Tests

        [Fact]
        public async Task GetOneSiteRoleUsersList_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetUsersForRole(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSiteRoleUsersList(100, new RequestParameter(), 1);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetOneSiteRoleList Tests

        [Fact]
        public async Task GetOneSiteRoleList_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetOneSiteRoleList(0, 200, true, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetOneSiteRoleList_WithUserPersonaId_ReturnsRoleList()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetOneSiteRoleList(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSiteRoleList(100, 200, true, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetOneSiteRoleList_WithZeroUserPersonaId_ReturnsAllRoles()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetOneSiteRoleListAll(It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSiteRoleList(100, 0, false, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateOneSiteUserRoles Tests

        [Fact]
        public async Task UpdateOneSiteUserRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteUserRoles(0, 200, new List<string> { "role1" });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUserRoles_WithEmptyRoleList_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteUserRoles(100, 200, new List<string>());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUserRoles_WithValidParameters_ReturnsOkResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSite
                .Setup(x => x.UpdateRolesForUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), out additionalParams))
                .Returns("3");

            var result = await _controller.UpdateOneSiteUserRoles(100, 200, new List<string> { "role1" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("3 Records Updated", okResult.Value);
        }

        #endregion

        #region GetOneSiteRoleListAll Tests

        [Fact]
        public async Task GetOneSiteRoleListAll_WithValidEditorPersonaId_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetOneSiteRoleListAll(It.IsAny<long>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSiteRoleListAll(100, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateRoleRights Tests

        [Fact]
        public async Task UpdateRoleRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ProductOneSiteController.RightsAddRemoveList();

            var result = await _controller.UpdateRoleRights(0, 1, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRoleRights_WithNullRightsList_ReturnsBadRequest()
        {
            var rightsToAddRemove = new ProductOneSiteController.RightsAddRemoveList { RightsToAdd = null, RightsToDelete = null };

            var result = await _controller.UpdateRoleRights(100, 1, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateRoleRights_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.UpdateRoleToRights(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
                .Returns(string.Empty);

            var rightsToAddRemove = new ProductOneSiteController.RightsAddRemoveList { RightsToAdd = new List<string> { "right1" } };

            var result = await _controller.UpdateRoleRights(100, 1, rightsToAddRemove);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateRoleRights_WhenRoleNotFound_ReturnsNotFound()
        {
            _mockManageProductOneSite
                .Setup(x => x.UpdateRoleToRights(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
                .Returns("ROLE NOT FOUND");

            var rightsToAddRemove = new ProductOneSiteController.RightsAddRemoveList { RightsToAdd = new List<string> { "right1" } };

            var result = await _controller.UpdateRoleRights(100, 1, rightsToAddRemove);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateRoleRights_WhenError_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.UpdateRoleToRights(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
                .Returns("Some error");

            var rightsToAddRemove = new ProductOneSiteController.RightsAddRemoveList { RightsToAdd = new List<string> { "right1" } };

            var result = await _controller.UpdateRoleRights(100, 1, rightsToAddRemove);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Some error", badRequestResult.Value);
        }

        #endregion

        #region UpdateOneSiteUserStatus Tests

        [Fact]
        public async Task UpdateOneSiteUserStatus_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteUserStatus(0, 200, true);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUserStatus_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.EnableOneSiteUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(string.Empty);

            var result = await _controller.UpdateOneSiteUserStatus(100, 200, true);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateOneSiteUserStatus_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.EnableOneSiteUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns("Error");

            var result = await _controller.UpdateOneSiteUserStatus(100, 200, true);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region CreateOneSiteUser Tests

        [Fact]
        public async Task CreateOneSiteUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.CreateOneSiteUser(0, 200, new OneSiteRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateOneSiteUser_WhenSuccess_ReturnsCreated()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSite
                .Setup(x => x.ManageOneSiteUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), out additionalParams, It.IsAny<bool>()))
                .Returns(string.Empty);

            var result = await _controller.CreateOneSiteUser(100, 200, new OneSiteRoleAndPropertyList());

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task CreateOneSiteUser_WhenFails_ReturnsBadRequest()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSite
                .Setup(x => x.ManageOneSiteUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), out additionalParams, It.IsAny<bool>()))
                .Returns("Error creating user");

            var result = await _controller.CreateOneSiteUser(100, 200, new OneSiteRoleAndPropertyList());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateOneSiteUser Tests

        [Fact]
        public async Task UpdateOneSiteUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteUser(0, 200, new OneSiteRoleAndPropertyList());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUser_WithNullRolePropertyList_ReturnsResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSite
                .Setup(x => x.ManageOneSiteUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), out additionalParams, It.IsAny<bool>()))
                .Returns(string.Empty);

            var result = await _controller.UpdateOneSiteUser(100, 200, null!);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateOneSiteUser_WhenSuccess_ReturnsOkResult()
        {
            List<AdditionalParameters> additionalParams;
            _mockManageProductOneSite
                .Setup(x => x.ManageOneSiteUser(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), out additionalParams, It.IsAny<bool>()))
                .Returns(string.Empty);

            var result = await _controller.UpdateOneSiteUser(100, 200, new OneSiteRoleAndPropertyList());

            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region DeleteOneSiteUser Tests

        [Fact]
        public async Task DeleteOneSiteUser_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteOneSiteUser(0, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId or userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteOneSiteUser_WhenSuccess_ReturnsNoContent()
        {
            _mockManageProductOneSite
                .Setup(x => x.DeleteOneSiteUser(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(string.Empty);

            var result = await _controller.DeleteOneSiteUser(100, 200);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOneSiteUser_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.DeleteOneSiteUser(It.IsAny<long>(), It.IsAny<long>()))
                .Returns("Error");

            var result = await _controller.DeleteOneSiteUser(100, 200);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetPMCURL Tests

        [Fact]
        public async Task GetPMCURL_WithZeroUserPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetPMCURL(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPMCURL_WhenPMCNotFound_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetPMCURL(It.IsAny<long>()))
                .Returns((PMCInfo)null!);

            var result = await _controller.GetPMCURL(100);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("PMC URL not found.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPMCURL_WhenPMCURLEmpty_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetPMCURL(It.IsAny<long>()))
                .Returns(new PMCInfo { PMCURL = "" });

            var result = await _controller.GetPMCURL(100);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("PMC URL not found.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPMCURL_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetPMCURL(It.IsAny<long>()))
                .Returns(new PMCInfo { PMCURL = "https://pmc.example.com" });

            var result = await _controller.GetPMCURL(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateOneSiteRolesWithRight Tests

        [Fact]
        public async Task UpdateOneSiteRolesWithRight_WithZeroRightId_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteRolesWithRight(100, 0, new List<string> { "role1" }, true);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("rightId or editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteRolesWithRight_WithEmptyRoleList_ReturnsBadRequest()
        {
            var result = await _controller.UpdateOneSiteRolesWithRight(100, 1, new List<string>(), true);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No Data", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteRolesWithRight_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.UpdateRightToRoles(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<string>>(), It.IsAny<bool>()))
                .Returns(string.Empty);

            var result = await _controller.UpdateOneSiteRolesWithRight(100, 1, new List<string> { "role1" }, true);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Roles Updated", okResult.Value);
        }

        #endregion

        #region GetRolesForRight Tests

        [Fact]
        public async Task GetRolesForRight_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetRolesForRight(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRolesForRight(100, new RequestParameter(), 1, true);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetOneSiteRightCenters Tests

        [Fact]
        public async Task GetOneSiteRightCenters_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetOneSiteRightsCenters(It.IsAny<long>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSiteRightCenters(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetOneSiteRights Tests

        [Fact]
        public async Task GetOneSiteRights_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.GetOneSiteRights(It.IsAny<long>(), It.IsAny<RequestParameter>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetOneSiteRights(100, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region AddRole Tests

        [Fact]
        public async Task AddRole_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.AddUpdateRole(It.IsAny<long>(), 0, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.AddRole(100, "NewRole");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddRole_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.AddUpdateRole(It.IsAny<long>(), 0, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = true, ErrorReason = "Role exists" });

            var result = await _controller.AddRole(100, "ExistingRole");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateRole Tests

        [Fact]
        public async Task UpdateRole_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.AddUpdateRole(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.UpdateRole(100, 1, "UpdatedRole");

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region DeleteRole Tests

        [Fact]
        public async Task DeleteRole_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.DeleteRole(It.IsAny<long>(), It.IsAny<int>()))
                .Returns(string.Empty);

            var result = await _controller.DeleteRole(100, 1);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteRole_WhenRoleNotFound_ReturnsNotFound()
        {
            _mockManageProductOneSite
                .Setup(x => x.DeleteRole(It.IsAny<long>(), It.IsAny<int>()))
                .Returns("ROLE NOT FOUND");

            var result = await _controller.DeleteRole(100, 1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteRole_WhenError_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.DeleteRole(It.IsAny<long>(), It.IsAny<int>()))
                .Returns("Error");

            var result = await _controller.DeleteRole(100, 1);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region ResetVerificationCode Tests

        [Fact]
        public async Task ResetVerificationCode_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.ResetVerificationCode(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(string.Empty);

            var result = await _controller.ResetVerificationCode(100, 200);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ResetVerificationCode_WhenNoResult_ReturnsNotFound()
        {
            _mockManageProductOneSite
                .Setup(x => x.ResetVerificationCode(It.IsAny<long>(), It.IsAny<long>()))
                .Returns("NO RESULT");

            var result = await _controller.ResetVerificationCode(100, 200);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ResetVerificationCode_WhenError_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.ResetVerificationCode(It.IsAny<long>(), It.IsAny<long>()))
                .Returns("Error");

            var result = await _controller.ResetVerificationCode(100, 200);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region UpdateOneSiteUserStatusMT Tests

        [Fact]
        public async Task UpdateOneSiteUserStatusMT_WhenSuccess_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.ChangeUserStatus(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(true);

            var productUser = new ProductUser { UserName = "testuser@test.com" };

            var result = await _controller.UpdateOneSiteUserStatusMT(productUser);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task UpdateOneSiteUserStatusMT_WhenFails_ReturnsBadRequest()
        {
            _mockManageProductOneSite
                .Setup(x => x.ChangeUserStatus(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(false);

            var productUser = new ProductUser { UserName = "testuser@test.com" };

            var result = await _controller.UpdateOneSiteUserStatusMT(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Enabling OneSite user failed.", badRequestResult.Value);
        }

        #endregion

        #region ListOneSiteMigrationUsers Tests

        [Fact]
        public async Task ListOneSiteMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListOneSiteMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListOneSiteMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListOneSiteMigrationUsers(999999, new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        {
            _mockManageProductOneSite
                .Setup(x => x.UpdateUsersMigrationStatus(It.IsAny<long>(), It.IsAny<IList<MigrateUser>>()))
                .Returns(new MigrateResponse { Status = true });

            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true }
            };

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region RightsAddRemoveList Class Tests

        [Fact]
        public void RightsAddRemoveList_CanSetProperties()
        {
            var rightsAddRemoveList = new ProductOneSiteController.RightsAddRemoveList
            {
                RightsToAdd = new List<string> { "right1", "right2" },
                RightsToDelete = new List<string> { "right3" }
            };

            Assert.Equal(2, rightsAddRemoveList.RightsToAdd.Count);
            Assert.Single(rightsAddRemoveList.RightsToDelete);
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
