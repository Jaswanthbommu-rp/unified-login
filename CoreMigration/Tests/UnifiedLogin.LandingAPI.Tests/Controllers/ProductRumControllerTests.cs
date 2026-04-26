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
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductRumControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductRumAsync> _mockManageProductRumAsync;
        private readonly Mock<IManagePersonaAsync> _mockManagePersonaAsync;
        private ProductRumController _controller;

        public ProductRumControllerTests()
        {
            _mockManageProductRumAsync = new Mock<IManageProductRumAsync>();
            _mockManagePersonaAsync = new Mock<IManagePersonaAsync>();

            _controller = new ProductRumController(
                MockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductRumController(
                MockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ProductRumController(
                    null!,
                    _mockManageProductRumAsync.Object,
                    _mockManagePersonaAsync.Object));

            Assert.Equal("userClaimsAccessor", exception.ParamName);
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoles(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRoles_WithNullUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, new RequestParameter());

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

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
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

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
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

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        #endregion

        #region GetRegionss Tests (Obsolete endpoint)

        [Fact]
        public async Task GetRegionss_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var result = await _controller.GetRegionss(0, 200, new RequestParameter());
#pragma warning restore CS0618

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRegionss_WithNullUserClaim_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

#pragma warning disable CS0618 // Type or member is obsolete
            var result = await controller.GetRegionss(100, 200, new RequestParameter());
#pragma warning restore CS0618

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRegionss_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

#pragma warning disable CS0618 // Type or member is obsolete
            var result = await controller.GetRegionss(100, 200, new RequestParameter());
#pragma warning restore CS0618

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
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

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
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

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetPropertyGroups(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

        #endregion

        #region ListRUMMigrationUsers Tests

        [Fact]
        public async Task ListRUMMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListRUMMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        //[Fact]
        //public async Task ListRUMMigrationUsers_WithInvalidEditorPersonaId_ThrowsException()
        //{
        //    // Note: ManagePersona is instantiated internally and will try to access database
        //    // This may throw exception before returning BadRequest
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListRUMMigrationUsers(999999999, new RequestParameter()));
        //}

        //[Fact]
        //public async Task ListRUMMigrationUsers_WithNullDataFilter_ThrowsException()
        //{
        //    // Act & Assert - Will throw exception trying to access database or get OAuth token
        //    await Assert.ThrowsAnyAsync<Exception>(async () =>
        //        await _controller.ListRUMMigrationUsers(999999999, null!));
        //}

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
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

        #endregion

        #region UpdateRUMUserStatus Tests

        [Fact]
        public async Task UpdateRUMUserStatus_WithNullUserClaim_ReturnsUnauthorized()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new ProductRumController(
                mockUserClaimsAccessor.Object,
                _mockManageProductRumAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var productUser = new ProductUser { UserId = 123, UserName = "testuser@test.com" };

            var result = await controller.UpdateRUMUserStatus(productUser);

            Assert.IsType<UnauthorizedResult>(result);
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
//The controller uses the Service Locator anti-pattern by instantiating ManageProductRum with new.For true unit testing, the controller would need to be refactored to inject IManageProductRum as a constructor parameter, allowing proper mocking of OAuth token acquisition and API calls.
//The current design makes integration testing necessary for these endpoints since they require:
//�	Valid OAuth2 client credentials
//�	Access to the RUM product API
//�	Valid authentication tokens
//�	Network connectivity to external services