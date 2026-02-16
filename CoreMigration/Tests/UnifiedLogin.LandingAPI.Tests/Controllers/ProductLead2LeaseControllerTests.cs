using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    public class ProductLead2LeaseControllerTests : ControllerTestBase
    {
        private ProductLead2LeaseController _productLead2LeaseController;

        public ProductLead2LeaseControllerTests()
        {
            _productLead2LeaseController = new ProductLead2LeaseController(
                MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidUserClaimsAccessor_CreatesInstance()
        {
            var controller = new ProductLead2LeaseController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsException()
        {
            // Note: Controller doesn't have explicit null guard but will throw on GetUserClaim
            Assert.ThrowsAny<Exception>(() => new ProductLead2LeaseController(null!));
        }

        #endregion

        #region GetLead2LeaseRoles Tests

        [Fact]
        public async Task GetLead2LeaseRoles_WithValidParameters_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseRoles(100, 200, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseRoles_WithZeroEditorPersonaId_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseRoles(0, 200, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseRoles_WithZeroUserPersonaId_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseRoles(100, 0, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseRoles_WithNullDataFilter_ReturnsOkResult()
        {
            var result = await _productLead2LeaseController.GetLead2LeaseRoles(100, 200, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseRoles_ReturnsListResponse()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseRoles(100, 200, dataFilter);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(okResult.Value);
        }

        #endregion

        #region GetLead2LeaseProperties Tests

        [Fact]
        public async Task GetLead2LeaseProperties_WithValidParameters_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseProperties(100, 200, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseProperties_WithZeroEditorPersonaId_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseProperties(0, 200, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseProperties_WithZeroUserPersonaId_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseProperties(100, 0, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseProperties_WithNullDataFilter_ReturnsOkResult()
        {
            var result = await _productLead2LeaseController.GetLead2LeaseProperties(100, 200, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseProperties_ReturnsListResponse()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseProperties(100, 200, dataFilter);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(okResult.Value);
        }

        #endregion

        #region UpdateLead2LeaseUserStatus Tests

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WhenUserClaimIsNull_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            // This will throw during construction since it calls GetUserClaim
            // Testing with a valid construction but null on second call
            var userClaim = new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.NewGuid()
            };

            mockUserClaimsAccessor.SetupSequence(x => x.GetUserClaim())
                .Returns(userClaim)  // First call in constructor
                .Returns((DefaultUserClaim)null!);  // Second call in method

            var controller = new ProductLead2LeaseController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser"
            };

            var result = await controller.UpdateLead2LeaseUserStatus(productUser);

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WithValidProductUser_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            var result = await _productLead2LeaseController.UpdateLead2LeaseUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WithNullUserName_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = null!
            };

            var result = await _productLead2LeaseController.UpdateLead2LeaseUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WithEmptyUserName_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = string.Empty
            };

            var result = await _productLead2LeaseController.UpdateLead2LeaseUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateLead2LeaseUserStatus_WithZeroUserId_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser"
            };

            var result = await _productLead2LeaseController.UpdateLead2LeaseUserStatus(productUser);

            Assert.NotNull(result);
        }

        #endregion

        #region ListLead2LeaseMigrationUsers Tests

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.ListLead2LeaseMigrationUsers(0, dataFilter);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WhenPersonaNotFound_ReturnsBadRequest()
        {
            // This will call ManagePersona.GetPersona which returns null for non-existent personas
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.ListLead2LeaseMigrationUsers(999999, dataFilter);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not found.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            // Will return BadRequest because persona won't be found
            var result = await _productLead2LeaseController.ListLead2LeaseMigrationUsers(100, null!);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListLead2LeaseMigrationUsers_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.ListLead2LeaseMigrationUsers(long.MaxValue, dataFilter);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WhenUserClaimIsNull_ReturnsUnauthorized()
        {
            var userClaim = new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.NewGuid()
            };

            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.SetupSequence(x => x.GetUserClaim())
                .Returns(userClaim)  // First call in constructor
                .Returns((DefaultUserClaim)null!);  // Second call in method

            var controller = new ProductLead2LeaseController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var migrateUsers = new List<MigrateUser>();

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

            var result = await _productLead2LeaseController.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>();

            var result = await _productLead2LeaseController.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            var result = await _productLead2LeaseController.UpdateUsersMigrationStatus(null!);

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
                    UsingUnifiedLogin = true,
                    LeadEmailAddress = "user1@test.com"
                }
            };

            var result = await _productLead2LeaseController.UpdateUsersMigrationStatus(migrateUsers);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_ReturnsMigrateResponse()
        {
            var migrateUsers = new List<MigrateUser>();

            var result = await _productLead2LeaseController.UpdateUsersMigrationStatus(migrateUsers);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<MigrateResponse>(okResult.Value);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task GetLead2LeaseRoles_WithMaxLongValues_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseRoles(long.MaxValue, long.MaxValue, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseProperties_WithMaxLongValues_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseProperties(long.MaxValue, long.MaxValue, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseRoles_WithNegativePersonaIds_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseRoles(-1, -1, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetLead2LeaseProperties_WithNegativePersonaIds_ReturnsOkResult()
        {
            var dataFilter = new RequestParameter();

            var result = await _productLead2LeaseController.GetLead2LeaseProperties(-1, -1, dataFilter);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _productLead2LeaseController = null!;
            base.Dispose();
        }

        #endregion
    }
}
