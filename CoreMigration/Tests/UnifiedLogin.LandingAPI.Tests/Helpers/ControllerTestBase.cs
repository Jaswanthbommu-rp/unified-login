using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Tests.Helpers
{
    /// <summary>
    /// Base class for controller unit tests providing common setup and mock helpers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ControllerTestBase : TestBase, IDisposable
    {
        #region Protected Fields

        protected readonly Mock<IUserClaimsAccessor> MockUserClaimsAccessor;
        protected readonly Mock<IMemoryCache> MockMemoryCache;
        protected readonly Mock<ILogger> MockLogger;
        protected readonly DefaultUserClaim DefaultUserClaim;
        protected readonly ClaimsPrincipal TestUser;

        #endregion

        #region Constructor

        protected ControllerTestBase()
        {
            // Setup default user claim
            DefaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationRealPageGuid = Guid.Parse("A5C090FA-78AB-452F-B504-98AAFEE09122"),
                OrganizationMasterId = 379,
                OrganizationPartyId = 1000,
                CorrelationId = Guid.NewGuid(),
                PersonaId = 100
            };

            // Setup user claims accessor mock
            MockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();
            MockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(DefaultUserClaim);
            
            // Setup individual property mocks for IUserClaimsAccessor
            MockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(DefaultUserClaim.UserRealPageGuid);
            MockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(DefaultUserClaim.PersonaId);
            MockUserClaimsAccessor.Setup(x => x.UserId).Returns(DefaultUserClaim.UserId);
            MockUserClaimsAccessor.Setup(x => x.LoginName).Returns(DefaultUserClaim.LoginName);
            MockUserClaimsAccessor.Setup(x => x.FirstName).Returns(DefaultUserClaim.FirstName);
            MockUserClaimsAccessor.Setup(x => x.LastName).Returns(DefaultUserClaim.LastName);
            MockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid).Returns(DefaultUserClaim.OrganizationRealPageGuid);
            MockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(DefaultUserClaim.OrganizationPartyId);
            MockUserClaimsAccessor.Setup(x => x.OrganizationMasterId).Returns(DefaultUserClaim.OrganizationMasterId);
            MockUserClaimsAccessor.Setup(x => x.CorrelationId).Returns(DefaultUserClaim.CorrelationId);
            MockUserClaimsAccessor.Setup(x => x.Rights).Returns(DefaultUserClaim.Rights ?? new List<string>());

            // Setup memory cache mock
            MockMemoryCache = new Mock<IMemoryCache>();
            var mockCacheEntry = new Mock<ICacheEntry>();
            mockCacheEntry.Setup(x => x.Value).Returns(new object());
            MockMemoryCache
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(mockCacheEntry.Object);

            // Setup logger mock
            MockLogger = new Mock<ILogger>();

            // Setup test user claims principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, DefaultUserClaim.UserId.ToString()),
                new Claim(ClaimTypes.Name, DefaultUserClaim.LoginName),
                new Claim(ClaimTypes.GivenName, DefaultUserClaim.FirstName),
                new Claim(ClaimTypes.Surname, DefaultUserClaim.LastName),
                new Claim("UserRealPageGuid", DefaultUserClaim.UserRealPageGuid.ToString()),
                new Claim("OrganizationRealPageGuid", DefaultUserClaim.OrganizationRealPageGuid.ToString())
            };
            TestUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a controller context with the test user
        /// </summary>
        protected ControllerContext CreateControllerContext()
        {
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = TestUser
                }
            };
        }

        /// <summary>
        /// Creates a mock repository response
        /// </summary>
        protected IRepositoryResponse CreateSuccessRepositoryResponse(long id = 1, string message = "Success")
        {
            var response = new Mock<IRepositoryResponse>();
            response.Setup(x => x.Id).Returns(id);
            response.Setup(x => x.ErrorMessage).Returns(string.Empty);
          //  response.Setup(x => x.Success).Returns(true);
            return response.Object;
        }

        /// <summary>
        /// Creates a mock repository response with error
        /// </summary>
        protected IRepositoryResponse CreateErrorRepositoryResponse(string errorMessage = "Error occurred")
        {
            var response = new Mock<IRepositoryResponse>();
            response.Setup(x => x.Id).Returns(0);
            response.Setup(x => x.ErrorMessage).Returns(errorMessage);
           // response.Setup(x => x.Success).Returns(false);
            return response.Object;
        }

        /// <summary>
        /// Creates default product internal settings
        /// </summary>
        protected List<ProductInternalSetting> CreateDefaultProductSettings()
        {
            return new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "BooksUseDomains", Value = "1" },
                new ProductInternalSetting { Name = "BooksUseUPFMId", Value = "1" },
                new ProductInternalSetting { Name = "UpdateProductInUDM", Value = "1" },
                new ProductInternalSetting { Name = "SettingsApiEndPoint", Value = "http://localhost" },
                new ProductInternalSetting { Name = "UnifiedLoginServerClientName", Value = "unifiedlogin-server" },
                new ProductInternalSetting { Name = "UnifiedLoginServerClientSecret", Value = "abcdefgh" },
                new ProductInternalSetting { Name = "productintegrationtype", Value = "Legacy" },
                new ProductInternalSetting { Name = "TiboWebHookSigningSecret", Value = _mockTiboWebHookSigningSecret },
                new ProductInternalSetting { Name = "IsCloneUsersProcessEnabledForHOTS", Value = "1" },
                new ProductInternalSetting { Name = "ExcludeProductFromOrgSupportUser", Value = "3,4,8,14,28,36,56" },
                new ProductInternalSetting { Name = "PlatformAdminRole", Value = "Platform Administrator" }
            };
        }

        /// <summary>
        /// Verifies that an action result is an OkObjectResult with expected status
        /// </summary>
        protected void AssertOkResult(IActionResult result, bool expectedSuccess = true)
        {
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        /// <summary>
        /// Verifies that an action result is a BadRequestResult
        /// </summary>
        protected void AssertBadRequestResult(IActionResult result)
        {
            Assert.NotNull(result);
            Assert.IsType<BadRequestResult>(result);
        }

        /// <summary>
        /// Verifies that an action result is an UnauthorizedResult
        /// </summary>
        protected void AssertUnauthorizedResult(IActionResult result)
        {
            Assert.NotNull(result);
            Assert.IsType<UnauthorizedResult>(result);
        }

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            // Cleanup if needed
        }

        #endregion
    }
}
