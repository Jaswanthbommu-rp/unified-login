using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.ServiceDefaults
{
    /// <summary>
    /// Implementation of IUserClaimsAccessor that extracts user claims from the current HttpContext.
    /// This service is registered as scoped in DI and automatically resolves claims from the authenticated user.
    /// </summary>
    public class UserClaimsAccessor : IUserClaimsAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Lazy<DefaultUserClaim> _userClaim;

        public UserClaimsAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            // Use lazy initialization to parse claims only when needed
            _userClaim = new Lazy<DefaultUserClaim>(() =>
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    return new DefaultUserClaim(); // Return empty claim for unauthenticated requests
                }
                return new DefaultUserClaim(user);
            });
        }

        /// <summary>
        /// Gets the user claim object, lazily initialized from HttpContext
        /// </summary>
        private DefaultUserClaim UserClaim => _userClaim.Value;

        public int UserId => UserClaim.UserId;

        public Guid CorrelationId => UserClaim.CorrelationId;

        public Guid UserRealPageGuid => UserClaim.UserRealPageGuid;

        public string LoginName => UserClaim.LoginName;

        public Guid OrganizationRealPageGuid => UserClaim.OrganizationRealPageGuid;

        public long OrganizationPartyId => UserClaim.OrganizationPartyId;

        public string OrganizationName => UserClaim.OrganizationName;

        public string OrganizationType => UserClaim.OrganizationType;

        public long OrganizationMasterId => UserClaim.OrganizationMasterId;

        public long CustomerMasterId => UserClaim.CustomerMasterId;

        public string Roles => UserClaim.Roles;

        public List<string> Rights => UserClaim.Rights;

        public string FirstName => UserClaim.FirstName;

        public string LastName => UserClaim.LastName;

        public string ClientCode => UserClaim.ClientCode;

        public long PersonaId => UserClaim.PersonaId;

        public bool RealPageEmployee => UserClaim.RealPageEmployee;

        public Guid ImpersonatedBy => UserClaim.ImpersonatedBy;

        public string ImpersonatedByName => UserClaim.ImpersonatedByName;

        public bool IsRPEmployee => UserClaim.IsRPEmployee;

        /// <summary>
        /// Gets the complete DefaultUserClaim object for backward compatibility.
        /// This enables gradual migration from legacy code.
        /// </summary>
        public DefaultUserClaim GetUserClaim() => UserClaim;
    }
}
