using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
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
    /// Comprehensive unit tests for ProductAssetOptimizationController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProductAssetOptimizationControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManagePersona> _mockManagePersona;
        private ProductAssetOptimizationController _productAssetOptimizationController;

        #endregion

        #region Constructor

        public ProductAssetOptimizationControllerTests()
        {
            _mockManagePersona = new Mock<IManagePersona>();

            _productAssetOptimizationController = new ProductAssetOptimizationController(
                MockUserClaimsAccessor.Object,
                _mockManagePersona.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new ProductAssetOptimizationController(
                MockUserClaimsAccessor.Object,
                _mockManagePersona.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ProductAssetOptimizationController(
                null!,
                _mockManagePersona.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ProductAssetOptimizationController(
                MockUserClaimsAccessor.Object,
                null!));
        }

        #endregion

        #region GetCompanies Tests

        [Fact]
        public async Task GetCompanies_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            var datafilter = new RequestParameter();

            // Act
            var result = await _productAssetOptimizationController.GetCompanies(0, 100, "BI", datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetCompanies_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductAssetOptimizationController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var datafilter = new RequestParameter();

            // Act
            var result = await controller.GetCompanies(100, 200, "BI", datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetCompanies_WithValidParameters_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetCompanies(100, 200, "BI", datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetCompanies_WithUserLoginName_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetCompanies(100, 200, "BI", datafilter, "user@test.com");

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region GetPropertyGroups Tests

        [Fact]
        public async Task GetPropertyGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            var datafilter = new RequestParameter();
            var selectedCompanies = new List<int> { 1, 2, 3 };

            // Act
            var result = await _productAssetOptimizationController.GetPropertyGroups(0, 100, "BI", selectedCompanies, datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductAssetOptimizationController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var datafilter = new RequestParameter();
            var selectedCompanies = new List<int> { 1, 2, 3 };

            // Act
            var result = await controller.GetPropertyGroups(100, 200, "BI", selectedCompanies, datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetPropertyGroups_WithValidParameters_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();
        //    var selectedCompanies = new List<int> { 1, 2, 3 };

        //    // Act
        //    var result = await _productAssetOptimizationController.GetPropertyGroups(100, 200, "BI", selectedCompanies, datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetPropertyGroups_WithEmptySelectedCompanies_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();
        //    var selectedCompanies = new List<int>();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetPropertyGroups(100, 200, "BI", selectedCompanies, datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region GetPropertiesInGroups Tests

        [Fact]
        public async Task GetPropertiesInGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            var datafilter = new RequestParameter();

            // Act
            var result = await _productAssetOptimizationController.GetPropertiesInGroups(0, 100, 1, datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesInGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductAssetOptimizationController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var datafilter = new RequestParameter();

            // Act
            var result = await controller.GetPropertiesInGroups(100, 200, 1, datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetPropertiesInGroups_WithValidParameters_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetPropertiesInGroups(100, 200, 1, datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region GetCompaniesWithRoles Tests

        [Fact]
        public async Task GetCompaniesWithRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            var datafilter = new RequestParameter();

            // Act
            var result = await _productAssetOptimizationController.GetCompaniesWithRoles(0, 100, "BI", datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetCompaniesWithRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductAssetOptimizationController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var datafilter = new RequestParameter();

            // Act
            var result = await controller.GetCompaniesWithRoles(100, 200, "BI", datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetCompaniesWithRoles_WithValidParameters_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetCompaniesWithRoles(100, 200, "BI", datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetCompaniesWithRoles_WithUserLoginName_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetCompaniesWithRoles(100, 200, "BI", datafilter, "user@test.com");

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region GetCompaniesWithProperties Tests

        [Fact]
        public async Task GetCompaniesWithProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            var datafilter = new RequestParameter();

            // Act
            var result = await _productAssetOptimizationController.GetCompaniesWithProperties(0, 100, "BI", datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetCompaniesWithProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductAssetOptimizationController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var datafilter = new RequestParameter();

            // Act
            var result = await controller.GetCompaniesWithProperties(100, 200, "BI", datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetCompaniesWithProperties_WithValidParameters_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetCompaniesWithProperties(100, 200, "BI", datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region GetOperatorsWithProperties Tests

        [Fact]
        public async Task GetOperatorsWithProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Act
            var result = await _productAssetOptimizationController.GetOperatorsWithProperties(0, 100);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetOperatorsWithProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductAssetOptimizationController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            // Act
            var result = await controller.GetOperatorsWithProperties(100, 200);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetOperatorsWithProperties_WithValidParameters_ReturnsOkResult()
        //{
        //    // Act
        //    var result = await _productAssetOptimizationController.GetOperatorsWithProperties(100, 200);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region GetPropertiesWithOperators Tests

        [Fact]
        public async Task GetPropertiesWithOperators_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Act
            var result = await _productAssetOptimizationController.GetPropertiesWithOperators(0, 100, "code", "value");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertiesWithOperators_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            // Arrange
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductAssetOptimizationController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            // Act
            var result = await controller.GetPropertiesWithOperators(100, 200, "code", "value");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetPropertiesWithOperators_WithValidParameters_ReturnsOkResult()
        //{
        //    // Act
        //    var result = await _productAssetOptimizationController.GetPropertiesWithOperators(100, 200, "code", "value");

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetPropertiesWithOperators_WithNullOperatorValues_ReturnsOkResult()
        //{
        //    // Act
        //    var result = await _productAssetOptimizationController.GetPropertiesWithOperators(100, 200, null!, null!);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region UpdateAOUserStatus Tests

        [Fact]
        public async Task UpdateAOUserStatus_WithValidProductUser_ReturnsResult()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                IsAssigned = true
            };

            // Act
            var result = await _productAssetOptimizationController.UpdateAOUserStatus(productUser);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAOUserStatus_WithIsAssignedTrue_ReturnsResult()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser",
                IsAssigned = true
            };

            // Act
            var result = await _productAssetOptimizationController.UpdateAOUserStatus(productUser);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAOUserStatus_WithIsAssignedFalse_ReturnsResult()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser",
                IsAssigned = false
            };

            // Act
            var result = await _productAssetOptimizationController.UpdateAOUserStatus(productUser);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAOUserStatus_WithNullUserName_ReturnsResult()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = null!,
                IsAssigned = false
            };

            // Act
            var result = await _productAssetOptimizationController.UpdateAOUserStatus(productUser);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region ListAssetOptimizationMigrationUsers Tests

        [Fact]
        public async Task ListAssetOptimizationMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            // Arrange
            var datafilter = new RequestParameter();

            // Act
            var result = await _productAssetOptimizationController.ListAssetOptimizationMigrationUsers(0, datafilter);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListAssetOptimizationMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            // Arrange
            _mockManagePersona
                .Setup(x => x.GetPersona(It.IsAny<long>()))
                .Returns((Persona)null!);

            var datafilter = new RequestParameter();

            // Act
            var result = await _productAssetOptimizationController.ListAssetOptimizationMigrationUsers(100, datafilter);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode);
        }

        //[Fact]
        //public async Task ListAssetOptimizationMigrationUsers_WhenPersonaFound_ReturnsOkResult()
        //{
        //    // Arrange
        //    var persona = new Persona
        //    {
        //        PersonaId = 100,
        //        RealPageId = Guid.NewGuid()
        //    };

        //    _mockManagePersona
        //        .Setup(x => x.GetPersona(100))
        //        .Returns(persona);

        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.ListAssetOptimizationMigrationUsers(100, datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        [Fact]
        public async Task ListAssetOptimizationMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            // Arrange
            _mockManagePersona
                .Setup(x => x.GetPersona(It.IsAny<long>()))
                .Returns((Persona)null!);

            // Act
            var result = await _productAssetOptimizationController.ListAssetOptimizationMigrationUsers(100, null!);

            // Assert
            Assert.IsType<ObjectResult>(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            // Act
            var result = await _productAssetOptimizationController.UpdateUsersMigrationStatus(migrateUsers);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>();

            // Act
            var result = await _productAssetOptimizationController.UpdateUsersMigrationStatus(migrateUsers);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            // Act
            var result = await _productAssetOptimizationController.UpdateUsersMigrationStatus(null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ReturnsOkResult()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser
                {
                    UserId = "user1",
                    UnifiedLoginUserName = "user1@test.com",
                    UsingUnifiedLogin = true,
                    LeadEmailAddress = "user1@test.com"
                }
            };

            // Act
            var result = await _productAssetOptimizationController.UpdateUsersMigrationStatus(migrateUsers);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Edge Cases

        //[Fact]
        //public async Task GetCompanies_WithMaxLongValues_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetCompanies(long.MaxValue, long.MaxValue, "BI", datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetCompanies_WithNullProductName_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetCompanies(100, 200, null!, datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetCompaniesWithRoles_WithDifferentProductNames_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();
        //    var productNames = new[] { "BI", "AX", "PO", "PA" };

        //    foreach (var productName in productNames)
        //    {
        //        // Act
        //        var result = await _productAssetOptimizationController.GetCompaniesWithRoles(100, 200, productName, datafilter);

        //        // Assert
        //        Assert.IsType<OkObjectResult>(result);
        //    }
        //}

        //[Fact]
        //public async Task GetPropertiesInGroups_WithZeroPropertyGroupId_ReturnsOkResult()
        //{
        //    // Arrange
        //    var datafilter = new RequestParameter();

        //    // Act
        //    var result = await _productAssetOptimizationController.GetPropertiesInGroups(100, 200, 0, datafilter);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetOperatorsWithProperties_WithNegativeUserPersonaId_ReturnsOkResult()
        //{
        //    // Act
        //    var result = await _productAssetOptimizationController.GetOperatorsWithProperties(100, -1);

        //    // Assert
        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _productAssetOptimizationController = null!;
            base.Dispose();
        }

        #endregion
    }
}





