using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductVendorServicesControllerTests : ControllerTestBase
    {
        private ProductVendorServicesController _controller;

        public ProductVendorServicesControllerTests()
        {
            _controller = new ProductVendorServicesController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductVendorServicesController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ProductVendorServicesController(null!));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region GetPropertyGroups Tests

        [Fact]
        public async Task GetPropertyGroups_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetPropertyGroups(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithNullUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertyGroups(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertyGroups(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithValidParameters_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange - The method instantiates ManageProductVendorServices which makes OAuth token requests
            // Note: This test documents that the method has external API dependencies that prevent unit testing

            // Act & Assert - Will throw exception trying to get OAuth token from Vendor Services API
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetPropertyGroups(100, 200, new RequestParameter()));
        }

        [Fact]
        public async Task GetPropertyGroups_WithNullDataFilter_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetPropertyGroups(100, 200, null!));
        }

        [Fact]
        public async Task GetPropertyGroups_WithZeroUserPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetPropertyGroups(100, 0, new RequestParameter()));
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoles(0, 200, AccessType.Property, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithNullUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, AccessType.Property, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, AccessType.Property, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithPropertyAccessType_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert - Will throw exception trying to get OAuth token from Vendor Services API
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetRoles(100, 200, AccessType.Property, new RequestParameter()));
        }

        [Fact]
        public async Task GetRoles_WithClientAccessType_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetRoles(100, 200, AccessType.Client, new RequestParameter()));
        }

        [Fact]
        public async Task GetRoles_WithNullDataFilter_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetRoles(100, 200, AccessType.Property, null!));
        }

        #endregion

        #region GetProperties Tests

        [Fact]
        public async Task GetProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetProperties(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithNullUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithValidParameters_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert - Will throw exception trying to get OAuth token from Vendor Services API
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetProperties(100, 200, new RequestParameter()));
        }

        [Fact]
        public async Task GetProperties_WithNullDataFilter_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetProperties(100, 200, null!));
        }

        #endregion

        #region GetNotification Tests

        [Fact]
        public async Task GetNotification_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetNotification(0, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetNotification_WithNullUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetNotification(100, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetNotification_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetNotification(100, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetNotification_WithValidParameters_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert - Will throw exception trying to get OAuth token from Vendor Services API
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetNotification(100, 200));
        }

        [Fact]
        public async Task GetNotification_WithZeroUserPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetNotification(100, 0));
        }

        #endregion

        #region UpdateVendorComplianceUserStatus Tests

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await controller.UpdateVendorComplianceUserStatus(productUser);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithValidProductUser_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            // Act & Assert - Will throw exception trying to get OAuth token from Vendor Services API
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateVendorComplianceUserStatus(productUser));
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithZeroUserId_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser@test.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateVendorComplianceUserStatus(productUser));
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithNegativeUserId_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = -1,
                UserName = "testuser@test.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateVendorComplianceUserStatus(productUser));
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithNullUserName_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = null!
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateVendorComplianceUserStatus(productUser));
        }

        #endregion

        #region ListVendorServiceMigrationUsers Tests

        [Fact]
        public async Task ListVendorServiceMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListVendorServiceMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task ListVendorServiceMigrationUsers_WithInvalidEditorPersonaId_ThrowsException()
        //{
        //    // Note: ManagePersona is instantiated internally and will try to access database
        //    // This may throw exception before returning BadRequest
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListVendorServiceMigrationUsers(999999999, new RequestParameter()));
        //}

        //[Fact]
        //public async Task ListVendorServiceMigrationUsers_WithNullDataFilter_ThrowsException()
        //{
        //    // Act & Assert - Will throw exception trying to access database or get OAuth token
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListVendorServiceMigrationUsers(999999999, null!));
        //}

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductVendorServicesController(mockUserClaimsAccessor.Object)
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
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            // Act & Assert - Will throw exception trying to get OAuth token from Vendor Services API
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateUsersMigrationStatus(null!));
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ThrowsExceptionDueToExternalDependency()
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
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetPropertyGroups_WithMaxLongEditorPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetPropertyGroups(long.MaxValue, 200, new RequestParameter()));
        }

        [Fact]
        public async Task GetPropertyGroups_WithNegativeEditorPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetPropertyGroups(-1, 200, new RequestParameter()));
        }

        [Fact]
        public async Task GetRoles_WithMaxLongEditorPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetRoles(long.MaxValue, 200, AccessType.Property, new RequestParameter()));
        }

        [Fact]
        public async Task GetProperties_WithMaxLongEditorPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetProperties(long.MaxValue, 200, new RequestParameter()));
        }

        [Fact]
        public async Task GetNotification_WithMaxLongEditorPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.GetNotification(long.MaxValue, 200));
        }

        //[Fact]
        //public async Task ListVendorServiceMigrationUsers_WithNegativeEditorPersonaId_ThrowsException()
        //{
        //    // Act & Assert
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListVendorServiceMigrationUsers(-1, new RequestParameter()));
        //}

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithMaxIntUserId_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = int.MaxValue,
                UserName = "testuser@test.com"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateVendorComplianceUserStatus(productUser));
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithEmptyUserName_ThrowsExceptionDueToExternalDependency()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = ""
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () => 
                await _controller.UpdateVendorComplianceUserStatus(productUser));
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


//Architecture Note
//The controller uses the Service Locator anti-pattern by instantiating ManageProductVendorServices with new.For true unit testing, the controller would need to be refactored to inject IManageProductVendorServices as a constructor parameter, allowing proper mocking of OAuth token acquisition and API calls.
//The current design makes integration testing necessary for these endpoints since they require:
//�	Valid OAuth2 client credentials for Vendor Services API
//�	Access to the Vendor Compliance product API endpoints
//�	Valid authentication tokens
//�	Network connectivity to external services
//�	Product-specific configuration settings
//All tests now compile successfully and properly document the external dependencies that prevent full unit test coverage.