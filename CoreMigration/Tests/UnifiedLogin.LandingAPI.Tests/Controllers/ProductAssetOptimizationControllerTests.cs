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
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for ProductAssetOptimizationController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductAssetOptimizationControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageProductAssetOptimizationAsync> _mockManageAo;
        private readonly Mock<IManagePersonaAsync> _mockManagePersona;
        private ProductAssetOptimizationController _controller;

        #endregion

        #region Constructor

        public ProductAssetOptimizationControllerTests()
        {
            _mockManageAo = new Mock<IManageProductAssetOptimizationAsync>();
            _mockManagePersona = new Mock<IManagePersonaAsync>();

            MockUserClaimsAccessor
                .Setup(x => x.UserRealPageGuid)
                .Returns(Guid.NewGuid());
            MockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(new DefaultUserClaim { UserRealPageGuid = Guid.NewGuid() });
            MockUserClaimsAccessor
                .Setup(x => x.PersonaId)
                .Returns(999L);

            _controller = new ProductAssetOptimizationController(
                MockUserClaimsAccessor.Object,
                _mockManageAo.Object,
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
            var controller = new ProductAssetOptimizationController(
                MockUserClaimsAccessor.Object,
                _mockManageAo.Object,
                _mockManagePersona.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullManageAo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductAssetOptimizationController(
                MockUserClaimsAccessor.Object,
                null!,
                _mockManagePersona.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductAssetOptimizationController(
                MockUserClaimsAccessor.Object,
                _mockManageAo.Object,
                null!));
        }

        #endregion

        #region GetCompanies Tests

        [Fact]
        public async Task GetCompanies_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetCompanies(0, 100, "BI", new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetCompanies_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetCompanies(100, 200, "BI", new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetCompanies_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManageAo
                .Setup(x => x.GetCompaniesAsync(100, 200, "BI", It.IsAny<RequestParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetCompanies(100, 200, "BI", new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task GetCompanies_WhenIsError_ReturnsForbidden()
        {
            _mockManageAo
                .Setup(x => x.GetCompaniesAsync(100, 200, "BI", It.IsAny<RequestParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse { IsError = true });

            var result = await _controller.GetCompanies(100, 200, "BI", new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Forbidden, objectResult.StatusCode);
        }

        #endregion

        #region GetPropertyGroups Tests

        [Fact]
        public async Task GetPropertyGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetPropertyGroups(0, 100, "BI", new List<string>(), new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetPropertyGroups(100, 200, "BI", new List<string>(), new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManageAo
                .Setup(x => x.GetPropertyGroupsAsync(100, 200, "BI", It.IsAny<IList<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetPropertyGroups(100, 200, "BI", new List<string> { "1", "2" }, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetPropertiesInGroups Tests

        [Fact]
        public async Task GetPropertiesInGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetPropertiesInGroups(0, 100, 1, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetPropertiesInGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetPropertiesInGroups(100, 200, 1, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetPropertiesInGroups_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManageAo
                .Setup(x => x.GetPropertiesInGroupAsync(100, 200, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetPropertiesInGroups(100, 200, 1, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetCompaniesWithRoles Tests

        [Fact]
        public async Task GetCompaniesWithRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetCompaniesWithRoles(0, 100, "BI", new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetCompaniesWithRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetCompaniesWithRoles(100, 200, "BI", new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetCompaniesWithRoles_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManageAo
                .Setup(x => x.GetCompaniesWithRolesAsync(100, 200, "BI", It.IsAny<RequestParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetCompaniesWithRoles(100, 200, "BI", new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetCompaniesWithProperties Tests

        [Fact]
        public async Task GetCompaniesWithProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetCompaniesWithProperties(0, 100, "BI", new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetCompaniesWithProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetCompaniesWithProperties(100, 200, "BI", new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetCompaniesWithProperties_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManageAo
                .Setup(x => x.GetCompaniesWithPropertiesAsync(100, 200, "BI", It.IsAny<RequestParameter>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetCompaniesWithProperties(100, 200, "BI", new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetOperatorsWithProperties Tests

        [Fact]
        public async Task GetOperatorsWithProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetOperatorsWithProperties(0, 100);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetOperatorsWithProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetOperatorsWithProperties(100, 200);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetOperatorsWithProperties_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManageAo
                .Setup(x => x.GetOperatorsAsync(100, 200, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetOperatorsWithProperties(100, 200);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region GetPropertiesWithOperators Tests

        [Fact]
        public async Task GetPropertiesWithOperators_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetPropertiesWithOperators(0, 100, "code", "value");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task GetPropertiesWithOperators_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var result = await _controller.GetPropertiesWithOperators(100, 200, "code", "value");

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequest.Value);
        }

        [Fact]
        public async Task GetPropertiesWithOperators_WithValidParameters_ReturnsOkResult()
        {
            var expected = new ListResponse();
            _mockManageAo
                .Setup(x => x.GetPropertiesWithOperatorsAsync(100, 200, "code", "value", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetPropertiesWithOperators(100, 200, "code", "value");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        #endregion

        #region UpdateAOUserStatus Tests

        [Fact]
        public async Task UpdateAOUserStatus_WhenSucceeds_ReturnsOk()
        {
            var productUser = new ProductUser { UserName = "user1", FirstName = "Test", LastName = "User", IsAssigned = false };
            _mockManageAo
                .Setup(x => x.ChangeUserStatusAsync(999L, "user1", "Test", "User", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateAOUserStatus(productUser);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully disabled product user.", ok.Value);
        }

        [Fact]
        public async Task UpdateAOUserStatus_WhenFailsAndIsAssignedTrue_ReturnsActivateBadRequest()
        {
            var productUser = new ProductUser { UserName = "user1", FirstName = "Test", LastName = "User", IsAssigned = true };
            _mockManageAo
                .Setup(x => x.ChangeUserStatusAsync(999L, "user1", "Test", "User", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateAOUserStatus(productUser);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Activate ao user failed.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateAOUserStatus_WhenFailsAndIsAssignedFalse_ReturnsDeactivateBadRequest()
        {
            var productUser = new ProductUser { UserName = "user1", FirstName = "Test", LastName = "User", IsAssigned = false };
            _mockManageAo
                .Setup(x => x.ChangeUserStatusAsync(999L, "user1", "Test", "User", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateAOUserStatus(productUser);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Deactivate ao user failed.", badRequest.Value);
        }

        #endregion

        #region ListAssetOptimizationMigrationUsers Tests

        [Fact]
        public async Task ListAssetOptimizationMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListAssetOptimizationMigrationUsers(0, new RequestParameter());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequest.Value);
        }

        [Fact]
        public async Task ListAssetOptimizationMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(100L, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListAssetOptimizationMigrationUsers(100, new RequestParameter());

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task ListAssetOptimizationMigrationUsers_WhenPersonaFound_ReturnsOkResult()
        {
            var persona = new Persona { RealPageId = Guid.NewGuid() };
            var expected = new ListResponse();

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(100L, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(persona);
            _mockManageAo
                .Setup(x => x.GetMigrationUsersAsync(100L, It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.ListAssetOptimizationMigrationUsers(100, new RequestParameter());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task ListAssetOptimizationMigrationUsers_WhenPersonaFound_CallsGetMigrationUsers()
        {
            var personaRealPageId = Guid.NewGuid();
            var persona = new Persona { RealPageId = personaRealPageId };

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(100L, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(persona);
            _mockManageAo
                .Setup(x => x.GetMigrationUsersAsync(100L, It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListResponse());

            await _controller.ListAssetOptimizationMigrationUsers(100, new RequestParameter());

            _mockManageAo.Verify(
                x => x.GetMigrationUsersAsync(100L, It.IsAny<RequestParameter>(), It.IsAny<CancellationToken>()),
                Times.Once);
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

            _mockManageAo
                .Setup(x => x.UpdateUsersMigrationStatusAsync(999L, migrateUsers, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expected, ok.Value);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            _mockManageAo
                .Setup(x => x.UpdateUsersMigrationStatusAsync(999L, null!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MigrateResponse());

            var result = await _controller.UpdateUsersMigrationStatus(null!);

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
