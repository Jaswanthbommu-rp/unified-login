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

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductOnSiteControllerTests : ControllerTestBase
    {
        private readonly Mock<IManagePersona> _mockManagePersona;
        private ProductOnSiteController _controller;

        public ProductOnSiteControllerTests()
        {
            _mockManagePersona = new Mock<IManagePersona>();

            _controller = new ProductOnSiteController(
                MockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductOnSiteController(
                MockUserClaimsAccessor.Object,
                _mockManagePersona.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOnSiteController(
                null!,
                _mockManagePersona.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOnSiteController(
                MockUserClaimsAccessor.Object,
                null!));
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
        public async Task GetRoles_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });
            mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(100L);

            var controller = new ProductOnSiteController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

     

        [Fact]
        public async Task GetRoles_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.GetRoles(100, 200, null!);

            Assert.NotNull(result);
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
        public async Task GetProperties_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });
            mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(100L);

            var controller = new ProductOnSiteController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

     

        [Fact]
        public async Task GetProperties_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.GetProperties(100, 200, null!);

            Assert.NotNull(result);
        }

      

        #endregion

        #region GetRegions Tests

        [Fact]
        public async Task GetRegions_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRegions(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRegions_WithEmptyUserRealPageGuid_ReturnsBadRequest()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });
            mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(100L);

            var controller = new ProductOnSiteController(
                mockUserClaimsAccessor.Object,
                _mockManagePersona.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRegions(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

       

        [Fact]
        public async Task GetRegions_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.GetRegions(100, 200, null!);

            Assert.NotNull(result);
        }

       

        #endregion

        #region ListOnSiteMigrationUsers Tests

        [Fact]
        public async Task ListOnSiteMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListOnSiteMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListOnSiteMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetPersona(It.IsAny<long>()))
                .Returns((Persona)null!);

            var result = await _controller.ListOnSiteMigrationUsers(999999, new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

     

        [Fact]
        public async Task ListOnSiteMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            _mockManagePersona
                .Setup(x => x.GetPersona(It.IsAny<long>()))
                .Returns(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            var result = await _controller.ListOnSiteMigrationUsers(100, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ReturnsOk()
        {
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ReturnsOk()
        {
            var migrateUsers = new List<MigrateUser>();

            var result = await _controller.UpdateUsersMigrationStatus(migrateUsers);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }

      

        [Fact]
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ReturnsOk()
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

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region UpdateOnSiteUserStatus Tests

       

        [Fact]
        public async Task UpdateOnSiteUserStatus_WithZeroUserId_ReturnsBadRequest()
        {
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateOnSiteUserStatus(productUser);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

      


        #endregion

        #region Edge Cases


        [Fact]
        public async Task GetProperties_WithMaxLongValues_ReturnsResult()
        {
            var result = await _controller.GetProperties(long.MaxValue, long.MaxValue, new RequestParameter());

            Assert.NotNull(result);
        }

     

        [Fact]
        public async Task ListOnSiteMigrationUsers_WithMaxLongEditorPersonaId_ReturnsForbidden()
        {
            _mockManagePersona
                .Setup(x => x.GetPersona(It.IsAny<long>()))
                .Returns((Persona)null!);

            var result = await _controller.ListOnSiteMigrationUsers(long.MaxValue, new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
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
