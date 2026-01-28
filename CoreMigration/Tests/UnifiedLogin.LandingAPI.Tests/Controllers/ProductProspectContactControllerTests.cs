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
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductProspectContactControllerTests : ControllerTestBase
    {
        private ProductProspectContactController _controller;

        public ProductProspectContactControllerTests()
        {
            _controller = new ProductProspectContactController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductProspectContactController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            // The constructor doesn't have null checks, so this should work
            // but the methods will fail when called
            var controller = new ProductProspectContactController(null!);

            Assert.NotNull(controller);
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

            var controller = new ProductProspectContactController(mockUserClaimsAccessor.Object)
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

            var controller = new ProductProspectContactController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetProperties(100, 200, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProperties_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.GetProperties(100, 200, null!);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetProperties_WithZeroUserPersonaId_ReturnsResult()
        {
            var result = await _controller.GetProperties(100, 0, new RequestParameter());

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateProspectContactCenterUserStatus Tests

        [Fact]
        public async Task UpdateProspectContactCenterUserStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductProspectContactController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await controller.UpdateProspectContactCenterUserStatus(productUser);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateProspectContactCenterUserStatus_WithValidProductUser_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateProspectContactCenterUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateProspectContactCenterUserStatus_WithZeroUserId_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateProspectContactCenterUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateProspectContactCenterUserStatus_WithNegativeUserId_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = -1,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateProspectContactCenterUserStatus(productUser);

            Assert.NotNull(result);
        }

        #endregion

        #region ListProspectContactMigrationUsers Tests

        [Fact]
        public async Task ListProspectContactMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListProspectContactMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListProspectContactMigrationUsers_WithInvalidEditorPersonaId_ReturnsBadRequest()
        {
            // When persona is not found, it should return BadRequest
            var result = await _controller.ListProspectContactMigrationUsers(999999999, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not found.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListProspectContactMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.ListProspectContactMigrationUsers(999999999, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductProspectContactController(mockUserClaimsAccessor.Object)
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
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>();

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            var result = await _controller.UpdateUsersMigrationStatus(null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser
                {
                    UserId = "user1",
                    UnifiedLoginUserName = "user1@test.com",
                    UsingUnifiedLogin = true
                }
            };

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetProperties_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.GetProperties(long.MaxValue, 200, new RequestParameter());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetProperties_WithNegativeEditorPersonaId_ReturnsResult()
        {
            // Negative values are not zero, so they pass the first check
            var result = await _controller.GetProperties(-1, 200, new RequestParameter());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateProspectContactCenterUserStatus_WithMaxIntUserId_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = int.MaxValue,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateProspectContactCenterUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListProspectContactMigrationUsers_WithNegativeEditorPersonaId_ReturnsResult()
        {
            // Negative values are not zero, so they pass the first check
            var result = await _controller.ListProspectContactMigrationUsers(-1, new RequestParameter());

            Assert.NotNull(result);
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
