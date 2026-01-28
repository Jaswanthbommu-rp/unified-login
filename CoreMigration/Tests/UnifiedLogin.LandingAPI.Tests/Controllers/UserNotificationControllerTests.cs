using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class UserNotificationControllerTests : ControllerTestBase
    {
        private UserNotificationController _controller;

        public UserNotificationControllerTests()
        {
            _controller = new UserNotificationController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new UserNotificationController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_DoesNotThrow()
        {
            var controller = new UserNotificationController(null!);

            Assert.NotNull(controller);
        }

        #endregion

        #region SendWelcomeEmail Tests

        [Fact]
        public async Task SendWelcomeEmail_WithEmptyList_ReturnsOkResult()
        {
            var userLogins = new List<UserLogin>();

            var result = await _controller.SendWelcomeEmail(userLogins);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithValidUserLogins_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = "test@test.com"
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithMultipleUserLogins_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = "user1@test.com"
                },
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = "user2@test.com"
                },
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = "user3@test.com"
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithSingleUserLogin_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = "single@test.com",
                    UserId = 100
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        #endregion

        #region SendWelcomeEmail Tests - User Claims Variations

        [Fact]
        public async Task SendWelcomeEmail_WithDifferentUserClaim_ReturnsResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim
            {
                PersonaId = 200,
                LoginName = "different@test.com",
                UserRealPageGuid = Guid.NewGuid(),
                OrganizationPartyId = 5000
            });

            var controller = new UserNotificationController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var userLogins = new List<UserLogin>
            {
                new UserLogin { RealPageId = Guid.NewGuid(), LoginName = "test@test.com" }
            };

            var result = await controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithNullUserClaim_ReturnsResult()
        {
            var mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns((DefaultUserClaim)null!);

            var controller = new UserNotificationController(mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };

            var userLogins = new List<UserLogin>();

            var result = await controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task SendWelcomeEmail_WithUserLoginHavingEmptyLoginName_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = ""
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithUserLoginHavingNullLoginName_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = null!
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithUserLoginHavingEmptyRealPageId_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.Empty,
                    LoginName = "test@test.com"
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithMaxGuidRealPageId_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    LoginName = "test@test.com"
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithSpecialCharactersInLoginName_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = "user+special@test.com"
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithLongLoginName_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = new string('a', 100) + "@test.com"
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SendWelcomeEmail_CalledMultipleTimes_ReturnsConsistentResults()
        {
            var userLogins = new List<UserLogin>();

            var result1 = await _controller.SendWelcomeEmail(userLogins);
            var result2 = await _controller.SendWelcomeEmail(userLogins);

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
        }

        [Fact]
        public async Task SendWelcomeEmail_WithCompleteUserLogin_ReturnsResult()
        {
            var userLogins = new List<UserLogin>
            {
                new UserLogin
                {
                    RealPageId = Guid.NewGuid(),
                    LoginName = "complete@test.com",
                    UserId = 100,
                    IsActive = true,
                    IsLocked = false,
                    FromDate = DateTime.UtcNow,
                    ThruDate = DateTime.UtcNow.AddYears(1)
                }
            };

            var result = await _controller.SendWelcomeEmail(userLogins);

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
