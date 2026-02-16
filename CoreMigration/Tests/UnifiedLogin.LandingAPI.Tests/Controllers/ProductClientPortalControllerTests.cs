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
    [ExcludeFromCodeCoverage]
    public class ProductClientPortalControllerTests : ControllerTestBase
    {
        private readonly Mock<IManagePersona> _mockManagePersona;
        private ProductClientPortalController _productClientPortalController;

        public ProductClientPortalControllerTests()
        {
            _mockManagePersona = new Mock<IManagePersona>();

            _productClientPortalController = new ProductClientPortalController(
                MockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductClientPortalController(
                MockUserClaimsAccessor.Object,
                _mockManagePersona.Object);
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductClientPortalController(
                null!,
                _mockManagePersona.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductClientPortalController(
                MockUserClaimsAccessor.Object,
                null!));
        }

        [Fact]
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var datafilter = new RequestParameter();
            var result = await _productClientPortalController.GetRoles(0, 100, datafilter);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductClientPortalController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var datafilter = new RequestParameter();
            var result = await controller.GetRoles(100, 200, datafilter);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetRoles_WithValidParameters_ReturnsOkResult()
        //{
        //    var datafilter = new RequestParameter();
        //    var result = await _productClientPortalController.GetRoles(100, 200, datafilter);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetRoles_WithNullDataFilter_ReturnsOkResult()
        //{
        //    var result = await _productClientPortalController.GetRoles(100, 200, null!);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetRoles_WithZeroUserPersonaId_ReturnsOkResult()
        //{
        //    var datafilter = new RequestParameter();
        //    var result = await _productClientPortalController.GetRoles(100, 0, datafilter);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        [Fact]
        public async Task GetProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var datafilter = new RequestParameter();
            var result = await _productClientPortalController.GetProperties(0, 100, datafilter);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            var controller = new ProductClientPortalController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var datafilter = new RequestParameter();
            var result = await controller.GetProperties(100, 200, datafilter);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task GetProperties_WithValidParameters_ReturnsOkResult()
        //{
        //    var datafilter = new RequestParameter();
        //    var result = await _productClientPortalController.GetProperties(100, 200, datafilter);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetProperties_WithNullDataFilter_ReturnsOkResult()
        //{
        //    var result = await _productClientPortalController.GetProperties(100, 200, null!);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        [Fact]
        public async Task ListClientPortalMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var datafilter = new RequestParameter();
            var result = await _productClientPortalController.ListClientPortalMigrationUsers(0, datafilter);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListClientPortalMigrationUsers_WithValidEditorPersonaId_ReturnsResult()
        {
            var datafilter = new RequestParameter();
            var result = await _productClientPortalController.ListClientPortalMigrationUsers(100, datafilter);
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task ListClientPortalMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            var result = await _productClientPortalController.ListClientPortalMigrationUsers(100, null!);
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };
            var result = await _productClientPortalController.UpdateUsersMigrationStatus(migrateUsers);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOkResult()
        {
            var migrateUsers = new List<MigrateUser>();
            var result = await _productClientPortalController.UpdateUsersMigrationStatus(migrateUsers);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullList_ReturnsOkResult()
        {
            var result = await _productClientPortalController.UpdateUsersMigrationStatus(null!);
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
            var result = await _productClientPortalController.UpdateUsersMigrationStatus(migrateUsers);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateClientPortalUserStatus_WithValidProductUser_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserLogin = "testuser@test.com"
            };
            var result = await _productClientPortalController.UpdateClientPortalUserStatus(productUser);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateClientPortalUserStatus_WithNullUserLogin_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserLogin = null!
            };
            var result = await _productClientPortalController.UpdateClientPortalUserStatus(productUser);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateClientPortalUserStatus_WithEmptyUserLogin_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserLogin = string.Empty
            };
            var result = await _productClientPortalController.UpdateClientPortalUserStatus(productUser);
            Assert.NotNull(result);
        }

        //[Fact]
        //public async Task GetRoles_WithMaxLongValues_ReturnsOkResult()
        //{
        //    var datafilter = new RequestParameter();
        //    var result = await _productClientPortalController.GetRoles(long.MaxValue, long.MaxValue, datafilter);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetProperties_WithMaxLongValues_ReturnsOkResult()
        //{
        //    var datafilter = new RequestParameter();
        //    var result = await _productClientPortalController.GetProperties(long.MaxValue, long.MaxValue, datafilter);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task GetRoles_WithNegativeUserPersonaId_ReturnsOkResult()
        //{
        //    var datafilter = new RequestParameter();
        //    var result = await _productClientPortalController.GetRoles(100, -1, datafilter);
        //    Assert.IsType<OkObjectResult>(result);
        //}

        public override void Dispose()
        {
            _productClientPortalController = null!;
            base.Dispose();
        }
    }
}
