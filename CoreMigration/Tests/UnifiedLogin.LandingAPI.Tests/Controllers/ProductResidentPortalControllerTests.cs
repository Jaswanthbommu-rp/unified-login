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
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class ProductResidentPortalControllerTests : ControllerTestBase
    {
        private ProductResidentPortalController _controller;

        public ProductResidentPortalControllerTests()
        {
            _controller = new ProductResidentPortalController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new ProductResidentPortalController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            var controller = new ProductResidentPortalController(null!);

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
        public async Task ListProperties_WithEmptyUserRealPageGuid_ReturnsOkWithMessage()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductResidentPortalController(mockUserClaimsAccessor.Object)
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
            var result = await _controller.ListProperties(100, 200, new RequestParameter());

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ListResponse>(okResult.Value);
        }

        [Fact]
        public async Task ListProperties_WithNullDataFilter_ReturnsOkResult()
        {
            var result = await _controller.ListProperties(100, 200, null!);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListProperties_WithZeroUserPersonaId_ReturnsOkResult()
        {
            var result = await _controller.ListProperties(100, 0, new RequestParameter());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetNotificationSettings Tests

        [Fact]
        public async Task GetNotificationSettings_WithZeroEditorPersonaId_ReturnsOkWithError()
        {
            var result = await _controller.GetNotificationSettings(0, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<INotifications, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.GetResidentPortalNotificationSettings.1", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter - editorPersonaId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetNotificationSettings_WithEmptyUserRealPageGuid_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductResidentPortalController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetNotificationSettings(100, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<INotifications, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.GetResidentPortalNotificationSettings.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetNotificationSettings_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetNotificationSettings(100, 200);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetNotificationSettings_WithZeroUserPersonaId_ReturnsOkResult()
        {
            var result = await _controller.GetNotificationSettings(100, 0);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetNotificationSettings_WithDefaultUserPersonaId_ReturnsOkResult()
        {
            var result = await _controller.GetNotificationSettings(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetResidentPortalUser Tests

        [Fact]
        public async Task GetResidentPortalUser_WithZeroEditorPersonaId_ReturnsOkWithError()
        {
            var result = await _controller.GetResidentPortalUser(0, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IResidentPortalUser, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.GetResidentPortalUser.1", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter - editorPersonaId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetResidentPortalUser_WithZeroUserPersonaId_ReturnsOkWithError()
        {
            var result = await _controller.GetResidentPortalUser(100, 0);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IResidentPortalUser, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.GetResidentPortalUser.2", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter - userPersonaId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetResidentPortalUser_WithEmptyUserRealPageGuid_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductResidentPortalController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.GetResidentPortalUser(100, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IResidentPortalUser, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.GetResidentPortalUser.3", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetResidentPortalUser_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.GetResidentPortalUser(100, 200);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListLevels Tests

        [Fact]
        public async Task ListLevels_WithZeroEditorPersonaId_ReturnsOkWithError()
        {
            var result = await _controller.ListLevels(0, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ILevel, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.ListLevel.1", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter - editorPersonaId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListLevels_WithEmptyUserRealPageGuid_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductResidentPortalController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListLevels(100, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ILevel, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.ListLevel.2", output.Status.ErrorCode);
        }

        //[Fact]
        //public async Task ListLevels_WithValidParameters_ReturnsOkResult()
        //{
        //    var result = await _controller.ListLevels(100, 200);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        //[Fact]
        //public async Task ListLevels_WithDefaultUserPersonaId_ReturnsOkResult()
        //{
        //    var result = await _controller.ListLevels(100);

        //    Assert.IsType<OkObjectResult>(result);
        //}

        #endregion

        #region ListMessagingGroups Tests

        [Fact]
        public async Task ListMessagingGroups_WithZeroEditorPersonaId_ReturnsOkWithError()
        {
            var result = await _controller.ListMessagingGroups(0, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<IMessagingGroups, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.ListMessagingGroup.1", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter - editorPersonaId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListMessagingGroups_WithEmptyUserRealPageGuid_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductResidentPortalController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListMessagingGroups(100, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<IMessagingGroups, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.ListMessagingGroup.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListMessagingGroups_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.ListMessagingGroups(100, 200);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListMessagingGroups_WithDefaultUserPersonaId_ReturnsOkResult()
        {
            var result = await _controller.ListMessagingGroups(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListTitles Tests

        [Fact]
        public async Task ListTitles_WithZeroEditorPersonaId_ReturnsOkWithError()
        {
            var result = await _controller.ListTitles(0, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ITitle, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.ListTitles.1", output.Status.ErrorCode);
            Assert.Equal("Invalid parameter - editorPersonaId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListTitles_WithEmptyUserRealPageGuid_ReturnsOkWithError()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 100,
                LoginName = "test@test.com",
                UserRealPageGuid = Guid.Empty
            });

            var controller = new ProductResidentPortalController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var result = await controller.ListTitles(100, 200);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ITitle, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ProductResidentPortal.ListTitles.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task ListTitles_WithValidParameters_ReturnsOkResult()
        {
            var result = await _controller.ListTitles(100, 200);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ListTitles_WithDefaultUserPersonaId_ReturnsOkResult()
        {
            var result = await _controller.ListTitles(100);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region ListResidentPortalMigrationUsers Tests

        [Fact]
        public async Task ListResidentPortalMigrationUsers_WithZeroEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListResidentPortalMigrationUsers(0, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not supplied.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListResidentPortalMigrationUsers_WithInvalidEditorPersonaId_ReturnsBadRequest()
        {
            var result = await _controller.ListResidentPortalMigrationUsers(999999999, new RequestParameter());

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("editorPersonaId not found.", badRequestResult.Value);
        }

        [Fact]
        public async Task ListResidentPortalMigrationUsers_WithNullDataFilter_ReturnsResult()
        {
            var result = await _controller.ListResidentPortalMigrationUsers(999999999, null!);

            Assert.NotNull(result);
        }

        #endregion

        #region UpdateUsersMigrationStatus Tests

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

        #region UpdateResidentPortalUserStatus Tests

        [Fact]
        public async Task UpdateResidentPortalUserStatus_WithValidProductUser_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateResidentPortalUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateResidentPortalUserStatus_WithZeroUserId_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 0,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateResidentPortalUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateResidentPortalUserStatus_WithNegativeUserId_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = -1,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateResidentPortalUserStatus(productUser);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateResidentPortalUserStatus_WithNullUserName_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = 123,
                UserName = null!
            };

            var result = await _controller.UpdateResidentPortalUserStatus(productUser);

            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task ListProperties_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.ListProperties(long.MaxValue, 200, new RequestParameter());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListProperties_WithNegativeEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.ListProperties(-1, 200, new RequestParameter());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetNotificationSettings_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.GetNotificationSettings(long.MaxValue, 200);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetResidentPortalUser_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.GetResidentPortalUser(long.MaxValue, 200);

            Assert.NotNull(result);
        }

        //[Fact]
        //public async Task ListLevels_WithMaxLongEditorPersonaId_ReturnsResult()
        //{
        //    var result = await _controller.ListLevels(long.MaxValue, 200);

        //    Assert.NotNull(result);
        //}

        [Fact]
        public async Task ListMessagingGroups_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.ListMessagingGroups(long.MaxValue, 200);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListTitles_WithMaxLongEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.ListTitles(long.MaxValue, 200);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ListResidentPortalMigrationUsers_WithNegativeEditorPersonaId_ReturnsResult()
        {
            var result = await _controller.ListResidentPortalMigrationUsers(-1, new RequestParameter());

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateResidentPortalUserStatus_WithMaxIntUserId_ReturnsResult()
        {
            var productUser = new ProductUser
            {
                UserId = int.MaxValue,
                UserName = "testuser@test.com"
            };

            var result = await _controller.UpdateResidentPortalUserStatus(productUser);

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
