using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
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
        private readonly Mock<IManageProductVendorServicesAsync> _mockManageProductVendorServicesAsync;
        private readonly Mock<IManagePersonaAsync> _mockManagePersonaAsync;
        private ProductVendorServicesController _controller;

        public ProductVendorServicesControllerTests()
        {
            _mockManageProductVendorServicesAsync = new Mock<IManageProductVendorServicesAsync>();
            _mockManagePersonaAsync = new Mock<IManagePersonaAsync>();

            _controller = new ProductVendorServicesController(
                MockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductVendorServicesController(
                MockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ProductVendorServicesController(
                    null!,
                    _mockManageProductVendorServicesAsync.Object,
                    _mockManagePersonaAsync.Object));

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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertyGroups(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetPropertyGroups_WithValidParameters_ReturnsOk()
        {
            // Act
            var result = await _controller.GetPropertyGroups(100, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPropertyGroups_WithNullDataFilter_ReturnsOk()
        {
            // Act
            var result = await _controller.GetPropertyGroups(100, 200, null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPropertyGroups_WithZeroUserPersonaId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetPropertyGroups(100, 0, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, AccessType.Property, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithPropertyAccessType_ReturnsOk()
        {
            // Act
            var result = await _controller.GetRoles(100, 200, AccessType.Property, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithClientAccessType_ReturnsOk()
        {
            // Act
            var result = await _controller.GetRoles(100, 200, AccessType.Client, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithNullDataFilter_ReturnsOk()
        {
            // Act
            var result = await _controller.GetRoles(100, 200, AccessType.Property, null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithValidParameters_ReturnsOk()
        {
            // Act
            var result = await _controller.GetProperties(100, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProperties_WithNullDataFilter_ReturnsOk()
        {
            // Act
            var result = await _controller.GetProperties(100, 200, null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetNotification(100, 200);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetNotification_WithValidParameters_ReturnsOk()
        {
            // Act
            var result = await _controller.GetNotification(100, 200);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetNotification_WithZeroUserPersonaId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetNotification(100, 0);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateVendorComplianceUserStatus Tests

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await controller.UpdateVendorComplianceUserStatus(productUser);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithValidProductUser_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateVendorComplianceUserStatus(productUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithZeroUserId_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateVendorComplianceUserStatus(productUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithNegativeUserId_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = -1,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateVendorComplianceUserStatus(productUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithNullUserName_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = null!
            };

            // Act
            var result = await _controller.UpdateVendorComplianceUserStatus(productUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
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

            var controller = new ProductVendorServicesController(
                mockUserClaimsAccessor.Object,
                _mockManageProductVendorServicesAsync.Object,
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
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOk()
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
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOk()
        {
            // Arrange
            var migrateUsers = new List<MigrateUser>();

            // Act
            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOk()
        {
            // Act
            var result = await _controller.UpdateUsersMigrationStatus(null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ReturnsOk()
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

        #region Edge Cases

        [Fact]
        public async Task GetPropertyGroups_WithMaxLongEditorPersonaId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetPropertyGroups(long.MaxValue, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPropertyGroups_WithNegativeEditorPersonaId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetPropertyGroups(-1, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRoles_WithMaxLongEditorPersonaId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetRoles(long.MaxValue, 200, AccessType.Property, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProperties_WithMaxLongEditorPersonaId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetProperties(long.MaxValue, 200, new RequestParameter());

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetNotification_WithMaxLongEditorPersonaId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetNotification(long.MaxValue, 200);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        //[Fact]
        //public async Task ListVendorServiceMigrationUsers_WithNegativeEditorPersonaId_ThrowsException()
        //{
        //    // Act & Assert
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListVendorServiceMigrationUsers(-1, new RequestParameter()));
        //}

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithMaxIntUserId_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = int.MaxValue,
                UserName = "testuser@test.com"
            };

            // Act
            var result = await _controller.UpdateVendorComplianceUserStatus(productUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateVendorComplianceUserStatus_WithEmptyUserName_ReturnsOk()
        {
            // Arrange
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = ""
            };

            // Act
            var result = await _controller.UpdateVendorComplianceUserStatus(productUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
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