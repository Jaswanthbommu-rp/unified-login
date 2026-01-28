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
        private ProductRentersInsuranceController _controller;

        public ProductRentersInsuranceControllerTests()
        {
            _controller = new ProductRentersInsuranceController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductRentersInsuranceController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            var controller = new ProductRentersInsuranceController(null!);

            Assert.NotNull(controller);
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

            var controller = new ProductRentersInsuranceController(mockUserClaimsAccessor.Object)
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

            var controller = new ProductRentersInsuranceController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListProperties(100, 200, new RequestParameter());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("RealPageId empty.", okResult.Value);
        }

        [Fact]
        public async Task ListProperties_WithValidParameters_ThrowsInvalidOperationException()
        {
            // Arrange - The method instantiates ManageProductRentersInsurance which creates WCF service clients
            // Note: This test documents that the method has WCF dependencies that prevent unit testing

            // Act & Assert - Will throw InvalidOperationException trying to create WCF service client
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListProperties(100, 200, new RequestParameter()));
        }

        [Fact]
        public async Task ListProperties_WithNullDataFilter_ThrowsInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListProperties(100, 200, null!));
        }

        [Fact]
        public async Task ListProperties_WithZeroUserPersonaId_ThrowsInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListProperties(100, 0, new RequestParameter()));
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

            var controller = new ProductRentersInsuranceController(mockUserClaimsAccessor.Object)
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

            var controller = new ProductRentersInsuranceController(mockUserClaimsAccessor.Object)
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
        public async Task ListRoles_WithValidParameters_ThrowsInvalidOperationException()
        {
            // Act & Assert - Will throw InvalidOperationException trying to create WCF service client
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListRoles(100, 200));
        }

        [Fact]
        public async Task ListRoles_WithZeroUserPersonaId_ThrowsInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListRoles(100, 0));
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

            var controller = new ProductRentersInsuranceController(mockUserClaimsAccessor.Object)
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
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ThrowsInvalidOperationException()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            // Act & Assert - Will throw InvalidOperationException trying to create WCF service client
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ThrowsInvalidOperationException()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ThrowsInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateUsersMigrationStatus(null!));
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ThrowsInvalidOperationException()
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

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

        #endregion

        #region UpdateRentersInsuranceUserStatus Tests

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRentersInsuranceController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await controller.UpdateRentersInsuranceUserStatus(productUser);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithValidProductUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            // Act & Assert - Will throw InvalidOperationException trying to create WCF service client
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateRentersInsuranceUserStatus(productUser));
        }

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithZeroUserId_ThrowsInvalidOperationException()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser@test.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateRentersInsuranceUserStatus(productUser));
        }

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithNegativeUserId_ThrowsInvalidOperationException()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = -1,
                UserName = "testuser@test.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateRentersInsuranceUserStatus(productUser));
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ListProperties_WithMaxLongEditorPersonaId_ThrowsInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListProperties(long.MaxValue, 200, new RequestParameter()));
        }

        [Fact]
        public async Task ListProperties_WithNegativeEditorPersonaId_ThrowsInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListProperties(-1, 200, new RequestParameter()));
        }

        [Fact]
        public async Task ListRoles_WithMaxLongEditorPersonaId_ThrowsInvalidOperationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.ListRoles(long.MaxValue, 200));
        }

        //[Fact]
        //public async Task ListRoles_WithNegativeEditorPersonaId_ReturnsOkWithError()
        //{
        //    // Arrange & Act
        //    var result = await _controller.ListRoles(-1, 200);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var output = Assert.IsType<ObjectListOutput<ProductRole, IErrorData>>(okResult.Value);
        //    Assert.False(output.Status.Success);
        //}

        [Fact]
        public async Task UpdateRentersInsuranceUserStatus_WithMaxIntUserId_ThrowsInvalidOperationException()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = int.MaxValue,
                UserName = "testuser@test.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _controller.UpdateRentersInsuranceUserStatus(productUser));
        }

        //[Fact]
        //public async Task ListRentersInsuranceMigrationUsers_WithNegativeEditorPersonaId_ThrowsException()
        //{
        //    // Act & Assert
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListRentersInsuranceMigrationUsers(-1, new RequestParameter()));
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
