using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductPanelControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductPanel> _mockManageProductPanel;
        private readonly Mock<IManagePersona> _mockManagePersona;
        private readonly Mock<IManageOrganization> _mockManageOrganization;
        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IManageUserRoleRight> _mockManageUserRoleRight;
        private ProductPanelController _controller;

        public ProductPanelControllerTests()
        {
            _mockManageProductPanel = new Mock<IManageProductPanel>();
            _mockManagePersona = new Mock<IManagePersona>();
            _mockManageOrganization = new Mock<IManageOrganization>();
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockManageUserRoleRight = new Mock<IManageUserRoleRight>();

            _controller = new ProductPanelController(
                MockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
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
            var controller = new ProductPanelController(
                MockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductPanelController(
                null!,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProductPanel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductPanelController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductPanelController(
                MockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                null!,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageOrganization_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductPanelController(
                MockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                null!,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePerson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductPanelController(
                MockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                null!,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserLogin_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductPanelController(
                MockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                null!,
                _mockManageUserRoleRight.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserRoleRight_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductPanelController(
                MockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                null!));
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, 100, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>(), It.IsAny<AccessType?>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(100, 200, 100, 1, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithAccessType_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>(), It.IsAny<AccessType?>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRoles(100, 200, 100, 1, new RequestParameter(), AccessType.Property);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetUserGroups Tests

        [Fact]
        public async Task GetUserGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetUserGroups(0, 200, 100, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetUserGroups(100, 200, 100, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserGroups_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductUserGroups(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetUserGroups(100, 200, 100, 1, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetUserProductRoles Tests

        [Fact]
        public async Task GetUserProductRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetUserProductRoles(0, 100, Guid.NewGuid());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserProductRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetUserProductRoles(100, 100, Guid.NewGuid());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserProductRoles_WithEmptyRealPageId_ReturnsBadRequest()
        {
            var result = await _controller.GetUserProductRoles(100, 100, Guid.Empty);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUserProductRoles_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetFirstAvailablePersonaByCompany(It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns((Persona)null!);

            var result = await _controller.GetUserProductRoles(100, 100, Guid.NewGuid());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetUserProductRoles_WhenPersonaIdIsZero_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetFirstAvailablePersonaByCompany(It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns(new Persona { PersonaId = 0 });

            var result = await _controller.GetUserProductRoles(100, 100, Guid.NewGuid());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetUserProductRoles_WithValidParameters_ReturnsOkResult()
        {
            _mockManagePersona
                .Setup(x => x.GetFirstAvailablePersonaByCompany(It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns(new Persona { PersonaId = 100 });

            _mockManageProductPanel
                .Setup(x => x.GetUserProductRoles(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new RoleTemplateProductRoleMapping());

            var result = await _controller.GetUserProductRoles(100, 100, Guid.NewGuid());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRights Tests

        [Fact]
        public async Task GetRights_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRights(0, 200, 100, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRights(100, 200, 100, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRights_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductRights(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRights(100, 200, 100, 1, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }
        #endregion

        #region GetProperties Tests

        [Fact]
        public async Task GetProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetProperties(0, 200, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetProperties(100, 200, 1, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetPropertiesPost Tests

        [Fact]
        public async Task GetPropertiesPost_WithZeroEditorPersonaIdAndNoUpfmId_ReturnsBadRequest()
        {
            var result = await _controller.GetPropertiesPost(0, 200, 1, new RequestParameter(), new UPFMProperty());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesPost_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertiesPost(100, 200, 1, new RequestParameter(), new UPFMProperty());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesPost_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });
            _mockManageProductPanel
                .Setup(x => x.CompareProductAndPrimaryProperties(It.IsAny<UPFMProperty>(), It.IsAny<int>(), It.IsAny<ListResponse>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetPropertiesPost(100, 200, 1, new RequestParameter(), new UPFMProperty());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPropertiesPost_WithDoNotTranslate_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetPropertiesPost(100, 200, 1, new RequestParameter(), new UPFMProperty(), true);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetPersonaProductPrimaryProperties Tests

        [Fact]
        public async Task GetPersonaProductPrimaryProperties_WithZeroUserPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetPersonaProductPrimaryProperties(0);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("userPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPersonaProductPrimaryProperties_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetPersonaProductPrimaryProperties(It.IsAny<long>()))
                .Returns(new List<PersonaProductProperty>());

            var result = await _controller.GetPersonaProductPrimaryProperties(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetTranslatedProperties Tests

        [Fact]
        public async Task GetTranslatedProperties_WithValidParameters_ReturnsOkResult()
        {
            var upfmProperty = new UPFMProperty { id = new List<string> { "prop1" } };

            _mockManageProductPanel
                .Setup(x => x.TranslateProductProperties(It.IsAny<UPFMProperty>(), It.IsAny<int>()))
                .Returns(new UPFMProperty());

#pragma warning disable CS0618 // Type or member is obsolete
            var result = await _controller.GetTranslatedProperties(upfmProperty, 1);
#pragma warning restore CS0618

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTranslatedProperties_WithNullUpfmProperty_ReturnsEmptyResult()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = await _controller.GetTranslatedProperties(null!, 1);
#pragma warning restore CS0618

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetTranslatedProperties_WithNullId_ReturnsEmptyResult()
        {
            var upfmProperty = new UPFMProperty { id = null };

#pragma warning disable CS0618 // Type or member is obsolete
            var result = await _controller.GetTranslatedProperties(upfmProperty, 1);
#pragma warning restore CS0618

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region GetPropertyGroups Tests

        [Fact]
        public async Task GetPropertyGroups_WithZeroEditorPersonaIdAndNoUpfmId_ReturnsBadRequest()
        {
            var result = await _controller.GetPropertyGroups(0, 200, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertyGroups(100, 200, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductPropertyGroups(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetPropertyGroups(100, 200, 1, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetRightsForRole Tests

        [Fact]
        public async Task GetRightsForRole_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsForRole(0, 1, "1", 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRightsForRole(100, 1, "1", 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithNullRoleId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsForRole(100, 1, null!, 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithEmptyRoleId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsForRole(100, 1, "", 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithZeroRoleId_ReturnsBadRequest()
        {
            var result = await _controller.GetRightsForRole(100, 1, "0", 100, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("roleId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRightsForRole_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductRightsForRole(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>(), It.IsAny<bool>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetRightsForRole(100, 1, "1", 100, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetProductGroupProperties Tests

        [Fact]
        public async Task GetProductGroupProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductGroupProperties(0, 200, 1, "1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductGroupProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProductGroupProperties(100, 200, 1, "1", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductGroupProperties_WithNullPropertyGroupId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductGroupProperties(100, 200, 1, null!, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Group Id.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductGroupProperties_WithEmptyPropertyGroupId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductGroupProperties(100, 200, 1, "", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Group Id.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductGroupProperties_WithZeroPropertyGroupId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductGroupProperties(100, 200, 1, "0", new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Group Id.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductGroupProperties_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductGroupProperties(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RequestParameter>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetProductGroupProperties(100, 200, 1, "1", new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }
        #endregion

        #region GetProductOrganizations Tests

        [Fact]
        public async Task GetProductOrganizations_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductOrganizations(0, 200, 1, "roleId", "type");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProductOrganizations(100, 200, 1, "roleId", "type");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithNullOrganizationRoleId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductOrganizations(100, 200, 1, null!, "type");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Organization Role Id.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithEmptyOrganizationRoleId_ReturnsBadRequest()
        {
            var result = await _controller.GetProductOrganizations(100, 200, 1, "", "type");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Organization Role Id.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithNullOrganizationType_ReturnsBadRequest()
        {
            var result = await _controller.GetProductOrganizations(100, 200, 1, "roleId", null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Organization Type", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithEmptyOrganizationType_ReturnsBadRequest()
        {
            var result = await _controller.GetProductOrganizations(100, 200, 1, "roleId", "");

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Organization Type", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductOrganizations_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductOrganizations(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetProductOrganizations(100, 200, 1, "roleId", "type");

            Assert.IsType<OkObjectResult>(result);
        }
        #endregion

        #region GetLocationGroups Tests

        [Fact]
        public async Task GetLocationGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetLocationGroups(0, 200, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetLocationGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductPanelController(
                mockUserClaimsAccessor.Object,
                _mockManageProductPanel.Object,
                _mockManagePersona.Object,
                _mockManageOrganization.Object,
                _mockManagePerson.Object,
                _mockManageUserLogin.Object,
                _mockManageUserRoleRight.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetLocationGroups(100, 200, 1, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetLocationGroups_WithValidParameters_ReturnsOkResult()
        {
            _mockManageProductPanel
                .Setup(x => x.GetProductLocationGroups(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<RequestParameter>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(new ListResponse { IsError = false });

            var result = await _controller.GetLocationGroups(100, 200, 1, new RequestParameter());

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
