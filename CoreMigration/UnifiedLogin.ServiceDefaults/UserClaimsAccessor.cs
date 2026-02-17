using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
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
        private DefaultUserClaim? _currentClaim;

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
                
                var userClaim = new DefaultUserClaim(user);
                
                // Load role names and rights from database if not present in JWT token
                // This mirrors the .NET 4.8 BaseApiController.Initialize() pattern (lines 107-112)
                if (userClaim.OrganizationPartyId > 0 && userClaim.PersonaId > 0)
                {
                    try
                    {
                        var identity = (ClaimsIdentity)user.Identity;
                        
                        // Check if role name claims already exist (not just roleId)
                        var existingRoleClaims = identity.Claims.Where(p => 
                            p.Type.Equals("roleid", StringComparison.OrdinalIgnoreCase) && 
                            !int.TryParse(p.Value, out _) // Check if value is NOT a number (i.e., it's a role name)
                        ).ToList();
                        
                        // Load role names from database and add as "roleid" claims
                        // This matches .NET 4.8 pattern: identity.AddClaims((userRoles.Select(a => new Claim("roleid", a.Name)).ToList()));
                        if (!existingRoleClaims.Any())
                        {
                            var rpCache = new RPObjectCache();
                            var cacheKey = $"getRoleByPersona_{userClaim.OrganizationPartyId}_{userClaim.PersonaId}";
                            var userRoles = rpCache.GetFromCache(cacheKey, 30, () =>
                            {
                                var urr = new UserRoleRightRepository();
                                return urr.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, userClaim.PersonaId, userClaim.OrganizationPartyId);
                            });
                            
                            // Add role NAME claims (not IDs) - BaseUserRights expects role names in "roleid" claims
                            if (userRoles != null && userRoles.Count > 0)
                            {
                                var roleNameClaims = userRoles.Select(r => new Claim("roleid", r.Name)).ToList();
                                identity.AddClaims(roleNameClaims);
                                
                                // Also populate Roles property for backward compatibility
                                var roleNames = userRoles.Select(r => r.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();
                                if (roleNames.Any())
                                {
                                    userClaim.Roles = string.Join(",", roleNames);
                                }
                                
                                // Set RoleId from first role if not already set
                                if (userClaim.RoleId == 0)
                                {
                                    userClaim.RoleId = (int)userRoles.First().RoleID;
                                }
                            }
                        }
                        else
                        {
                            // Role name claims already exist, just populate Roles property
                            var roleNames = existingRoleClaims.Select(c => c.Value).ToList();
                            userClaim.Roles = string.Join(",", roleNames);
                        }
                        
                        // Load rights from database if not present in JWT token
                        // BaseUserRights.GetUserRightsBy() now has role name claims to work with
                        if (userClaim.Rights == null || userClaim.Rights.Count == 0)
                        {
                            userClaim.Rights = BaseUserRights.GetUserRightsBy(user, userClaim);
                        }
                    }
                    catch (Exception)
                    {
                        // If loading fails, initialize with safe defaults to prevent null reference
                        userClaim.Rights ??= new List<string>();
                        userClaim.Roles ??= string.Empty;
                    }
                }
                
                return userClaim;
            });
        }

        /// <summary>
        /// Gets or sets the user claim object, lazily initialized from HttpContext.
        /// Setting this property overrides the lazily built claim.
        /// </summary>
        public DefaultUserClaim UserClaim
        {
            get => _currentClaim ?? _userClaim.Value;
            set => _currentClaim = value ?? new DefaultUserClaim();
        }

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
