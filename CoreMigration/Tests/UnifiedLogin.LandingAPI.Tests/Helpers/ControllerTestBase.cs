using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Tests.Helpers
{
    /// <summary>
    /// Base class for controller unit tests providing common setup and mock helpers.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ControllerTestBase : IDisposable
    {
        protected Mock<IUserClaimsAccessor> MockUserClaimsAccessor { get; }
        protected DefaultUserClaim DefaultUserClaim { get; }

        private static readonly Guid DefaultRealPageGuid = Guid.NewGuid();

        protected ControllerTestBase()
        {
            DefaultUserClaim = new DefaultUserClaim
            {
                UserId = 100,
                PersonaId = 100,
                FirstName = "Test",
                LastName = "User",
                LoginName = "test@test.com",
                UserRealPageGuid = DefaultRealPageGuid,
                CorrelationId = Guid.NewGuid(),
                OrganizationMasterId = 1,
                OrganizationPartyId = 1
            };

            MockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();

            MockUserClaimsAccessor
                .Setup(x => x.UserRealPageGuid)
                .Returns(DefaultRealPageGuid);

            MockUserClaimsAccessor
                .Setup(x => x.PersonaId)
                .Returns(100L);

            MockUserClaimsAccessor
                .Setup(x => x.UserId)
                .Returns(DefaultUserClaim.UserId);

            MockUserClaimsAccessor
                .Setup(x => x.FirstName)
                .Returns(DefaultUserClaim.FirstName);

            MockUserClaimsAccessor
                .Setup(x => x.LastName)
                .Returns(DefaultUserClaim.LastName);

            MockUserClaimsAccessor
                .Setup(x => x.LoginName)
                .Returns(DefaultUserClaim.LoginName);

            MockUserClaimsAccessor
                .Setup(x => x.CorrelationId)
                .Returns(DefaultUserClaim.CorrelationId);

            MockUserClaimsAccessor
                .Setup(x => x.OrganizationMasterId)
                .Returns(DefaultUserClaim.OrganizationMasterId);

            MockUserClaimsAccessor
                .Setup(x => x.OrganizationPartyId)
                .Returns(DefaultUserClaim.OrganizationPartyId);

            MockUserClaimsAccessor
                .Setup(x => x.GetUserClaim())
                .Returns(DefaultUserClaim);
        }

        protected ControllerContext CreateControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, DefaultUserClaim.LoginName),
                new Claim(ClaimTypes.NameIdentifier, DefaultUserClaim.UserId.ToString())
            ], "TestAuth"));

            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        public virtual void Dispose() { }
    }
}
