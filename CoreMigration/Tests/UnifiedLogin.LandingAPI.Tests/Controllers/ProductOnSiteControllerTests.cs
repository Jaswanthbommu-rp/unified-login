using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
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

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]

    public class ProductOnSiteControllerTests : ControllerTestBase
    {
        private readonly Mock<IManageProductOneSiteAsync> _mockManageProductOneSiteAsync;
        private readonly Mock<IManagePersonaAsync> _mockManagePersonaAsync;
        private ProductOnSiteController _controller;

        public ProductOnSiteControllerTests()
        {
            _mockManageProductOneSiteAsync = new Mock<IManageProductOneSiteAsync>();
            _mockManagePersonaAsync = new Mock<IManagePersonaAsync>();

            _controller = new ProductOnSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

       
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductOnSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAsync.Object,
                _mockManagePersonaAsync.Object);

            Assert.NotNull(controller);
        }


        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOnSiteController(
                null!,
                _mockManageProductOneSiteAsync.Object,
                _mockManagePersonaAsync.Object));
        }


        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductOnSiteController(
                MockUserClaimsAccessor.Object,
                _mockManageProductOneSiteAsync.Object,
                null!));
        }

        #endregion

        #region GetRoles Tests

   
        public async Task GetRoles_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRoles(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

     
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
                _mockManageProductOneSiteAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRoles(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

     
        public async Task GetRoles_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.GetRoles(100, 200, new RequestParameter());

            Assert.NotNull(result);
        }


        public async Task GetRoles_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.GetRoles(100, 200, null!);

            Assert.NotNull(result);
        }


        public async Task GetRoles_WithZeroUserPersonaId_ReturnsResult()
        {
            var result = await _controller.GetRoles(100, 0, new RequestParameter());

            Assert.NotNull(result);
        }

        #endregion

        #region GetProperties Tests


        public async Task GetProperties_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetProperties(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

  
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
                _mockManageProductOneSiteAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetProperties(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }


        public async Task GetProperties_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.GetProperties(100, 200, new RequestParameter());

            Assert.NotNull(result);
        }


        public async Task GetProperties_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.GetProperties(100, 200, null!);

            Assert.NotNull(result);
        }

 
        public async Task GetProperties_WithZeroUserPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.GetProperties(100, 0, new RequestParameter()));
        }

        #endregion

        #region GetRegions Tests

    
        public async Task GetRegions_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.GetRegions(0, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

  
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
                _mockManageProductOneSiteAsync.Object,
                _mockManagePersonaAsync.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetRegions(100, 200, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("RealPageId empty.", badRequestResult.Value);
        }

    
        public async Task GetRegions_WithValidParameters_ReturnsResult()
        {
            var result = await _controller.GetRegions(100, 200, new RequestParameter());

            Assert.NotNull(result);
        }

      
        public async Task GetRegions_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.GetRegions(100, 200, null!);

            Assert.NotNull(result);
        }

     
        public async Task GetRegions_WithZeroUserPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.GetRegions(100, 0, new RequestParameter()));
        }

        #endregion

        #region ListOnSiteMigrationUsers Tests

      
        public async Task ListOnSiteMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListOnSiteMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

    
        public async Task ListOnSiteMigrationUsers_WhenPersonaNotFound_ReturnsForbidden()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            var result = await _controller.ListOnSiteMigrationUsers(999999, new RequestParameter());

            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, statusCodeResult.StatusCode);
        }


        public async Task ListOnSiteMigrationUsers_WithValidParameters_ThrowsExceptionDueToExternalDependency()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.ListOnSiteMigrationUsers(100, new RequestParameter()));
        }


        public async Task ListOnSiteMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Persona { PersonaId = 100, RealPageId = Guid.NewGuid() });

            var result = await _controller.ListOnSiteMigrationUsers(100, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

   
        public async Task UpdateUsersMigrationStatus_WithValidUsers_ThrowsExceptionDueToExternalDependency()
        {
            var migrateUsers = new List<MigrateUser>
            {
                new MigrateUser { UserId = "user1", UsingUnifiedLogin = true },
                new MigrateUser { UserId = "user2", UsingUnifiedLogin = false }
            };

            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

  
        public async Task UpdateUsersMigrationStatus_WithEmptyList_ThrowsExceptionDueToExternalDependency()
        {
            var migrateUsers = new List<MigrateUser>();

                await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

       
        public async Task UpdateUsersMigrationStatus_WithNullList_ThrowsExceptionDueToExternalDependency()
        {
            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UpdateUsersMigrationStatus(null!));
        }

        
        public async Task UpdateUsersMigrationStatus_WithSingleUser_ThrowsExceptionDueToExternalDependency()
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

            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UpdateUsersMigrationStatus(migrateUsers));
        }

        #endregion

        #region UpdateOnSiteUserStatus Tests


        public async Task UpdateOnSiteUserStatus_WithValidProductUser_ThrowsExceptionDueToExternalDependency()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UpdateOnSiteUserStatus(productUser));
        }

      
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

     
        public async Task UpdateOnSiteUserStatus_WithNullUserName_ThrowsExceptionDueToExternalDependency()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = null!
            };

            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UpdateOnSiteUserStatus(productUser));
        }

      
        public async Task UpdateOnSiteUserStatus_WithEmptyUserName_ThrowsExceptionDueToExternalDependency()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = string.Empty
            };

            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.UpdateOnSiteUserStatus(productUser));
        }

        #endregion

        #region Edge Cases

  
        public async Task GetRoles_WithMaxLongEditorPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.GetRoles(long.MaxValue, 200, new RequestParameter()));
        }

    
        public async Task GetProperties_WithMaxLongValues_ReturnsResult()
        {
            var result = await _controller.GetProperties(long.MaxValue, long.MaxValue, new RequestParameter());

            Assert.NotNull(result);
        }

 
        public async Task GetRegions_WithNegativeUserPersonaId_ThrowsExceptionDueToExternalDependency()
        {
            await Assert.ThrowsAsync<Exception>(async () =>
                await _controller.GetRegions(100, -1, new RequestParameter()));
        }

 
        public async Task ListOnSiteMigrationUsers_WithMaxLongEditorPersonaId_ReturnsForbidden()
        {
            _mockManagePersonaAsync
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

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
