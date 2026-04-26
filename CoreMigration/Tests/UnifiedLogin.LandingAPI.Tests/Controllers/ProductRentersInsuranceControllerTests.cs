using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

//Architecture Note
//For true unit testing, the controller would need to be refactored to inject IManageProductRentersInsurance as a constructor parameter instead of creating it with new.This would allow proper mocking of the WCF service interactions. The current design uses the Service Locator anti-pattern which prevents isolation in unit tests.
namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductRentersInsuranceControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductRentersInsuranceAsync> _mockManageProductRentersInsuranceAsync;
        private readonly Mock<IManagePersonaAsync> _mockManagePersonaAsync;
        private ProductRentersInsuranceController _controller;

        public ProductRentersInsuranceControllerTests()
        {
            _mockManageProductRentersInsuranceAsync = new Mock<IManageProductRentersInsuranceAsync>();
            _mockManagePersonaAsync = new Mock<IManagePersonaAsync>();

            _controller = new ProductRentersInsuranceController(
                MockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductRentersInsuranceController(
                MockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ProductRentersInsuranceController(
                    null!,
                    _mockManageProductRentersInsuranceAsync.Object,
                    _mockManagePersonaAsync.Object));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region ListProperties Tests

        [Fact]
        public async Task ListProperties_WithZeroEditorPersonaId_ReturnsOkWithMessage()
        {
            var result = await _controller.ListProperties(0, 200, new RequestParameter());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", okResult.Value);
        }

        [Fact]
        public async Task ListProperties_WithNullUserClaim_ReturnsOkWithMessage()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRentersInsuranceController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListProperties(100, 200, new RequestParameter());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("RealPageId empty.", okResult.Value);
        }

        [Fact]
        public async Task ListProperties_WithEmptyUserRealPageGuid_ReturnsOkWithMessage()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductRentersInsuranceController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListProperties(100, 200, new RequestParameter());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("RealPageId empty.", okResult.Value);
        }

        [Fact]
        public async Task ListProperties_WithValidParameters_ReturnsOkResult()
        {
            // Arrange - Set up necessary parameters and mock behavior

            // Act
            var result = await _controller.ListProperties(100, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListProperties_WithNullDataFilter_ReturnsOkResult()
        {
            // Act
            var result = await _controller.ListProperties(100, 200, null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListProperties_WithZeroUserPersonaId_ReturnsOkResult()
        {
            // Act
            var result = await _controller.ListProperties(100, 0, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListRoles Tests

        [Fact]
        public async Task ListRoles_WithZeroEditorPersonaId_ReturnsOkWithError()
        {
            var result = await _controller.ListRoles(0, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductRole, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductRentersInsurance.ListRoles.1", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter - editorPersonaId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListRoles_WithNullUserClaim_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRentersInsuranceController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListRoles(100, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductRole, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductRentersInsurance.ListRoles.2", output.Status.ErrorCode);
            Assert.Equal("Invalid - Enterprise User Id", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListRoles_WithEmptyUserRealPageGuid_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductRentersInsuranceController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListRoles(100, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductRole, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductRentersInsurance.ListRoles.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListRoles_WithValidParameters_ReturnsOkResult()
        {
            // Arrange - Set up any necessary parameters or mock behavior

            // Act
            var result = await _controller.ListRoles(100, 200);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoles_WithZeroUserPersonaId_ReturnsOkResult()
        {
            // Act
            var result = await _controller.ListRoles(100, 0);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListRentersInsuranceMigrationUsers Tests

        [Fact]
        public async Task ListRentersInsuranceMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListRentersInsuranceMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task ListRentersInsuranceMigrationUsers_WithInvalidEditorPersonaId_ReturnsBadRequest()
        //{
        //    // Note: ManagePersona is instantiated internally and will try to access database
        //    // This may throw exception before returning BadRequest
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListRentersInsuranceMigrationUsers(999999999, new RequestParameter()));
        //}

        //[Fact]
        //public async Task ListRentersInsuranceMigrationUsers_WithNullDataFilter_ThrowsException()
        //{
        //    // Act & Assert - Will throw exception trying to access database or create WCF client
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListRentersInsuranceMigrationUsers(999999999, null!));
        //}

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRentersInsuranceController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true }
            };

            var result = await controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<UnauthorizedResult>(result);
        }

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
            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>();

            // Act
            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            // Act
            var result = await _controller.UpdateUsersMigrationStatus(null!);

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
                    UsingUnifiedLogin = true
                }
            };

            // Act
            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateRentersInsuranceUserStatus Tests

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRentersInsuranceController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRentersInsuranceAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await controller.UpdateRentersInsuranceUserStatus(productUser);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithValidProductUser_ReturnsOkOrBadRequest()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateRentersInsuranceUserStatus(productUser);

            // Assert
            Assert.True(result is OkObjectResult || result is BadRequestObjectResult);
        }

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithZeroUserId_ReturnsOkOrBadRequest()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateRentersInsuranceUserStatus(productUser);

            // Assert
            Assert.True(result is OkObjectResult || result is BadRequestObjectResult);
        }

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithNegativeUserId_ReturnsOkOrBadRequest()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = -1,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateRentersInsuranceUserStatus(productUser);

            // Assert
            Assert.True(result is OkObjectResult || result is BadRequestObjectResult);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ListProperties_WithMaxLongEditorPersonaId_ReturnsOkResult()
        {
            // Act
            var result = await _controller.ListProperties(long.MaxValue, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListProperties_WithNegativeEditorPersonaId_ReturnsOkResult()
        {
            // Act
            var result = await _controller.ListProperties(-1, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListRoles_WithMaxLongEditorPersonaId_ReturnsOkResult()
        {
            // Act
            var result = await _controller.ListRoles(long.MaxValue, 200);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithMaxIntUserId_ReturnsOkOrBadRequest()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = int.MaxValue,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateRentersInsuranceUserStatus(productUser);

            // Assert
            Assert.True(result is OkObjectResult || result is BadRequestObjectResult);
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
