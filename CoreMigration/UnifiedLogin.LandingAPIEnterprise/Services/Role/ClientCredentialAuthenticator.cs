using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Services.Role
{
    /// <summary>
    /// Handles client credential authentication for enterprise API endpoints.
    /// Validates scope claims and resolves admin user identity based on UPFM organization ID.
    /// </summary>
    public sealed class ClientCredentialAuthenticator : IClientCredentialAuthenticator
    {
        private readonly IManageOrganization _manageOrganization;

        public ClientCredentialAuthenticator(IManageOrganization manageOrganization)
        {
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
        }

        public ClientCredentialAuthResult Authenticate(ClaimsPrincipal user, DefaultUserClaim currentClaims, Guid? upfmId)
        {
            // No upfmId provided - use current claims as-is
            if (!upfmId.HasValue || upfmId == Guid.Empty)
            {
                return ClientCredentialAuthResult.Success(currentClaims);
            }

            // Current claims are null - cannot proceed
            if (currentClaims == null)
            {
                return ClientCredentialAuthResult.Success(null);
            }

            var principal = user ?? new ClaimsPrincipal();

            // Check for required scopes (usermanagement or internalapi)
            var hasRequiredScope =
                principal.HasClaim("scope", "usermanagement") ||
                principal.HasClaim("scope", "internalapi");

            // Only apply client credential auth if:
            // 1. User has required scope, AND
            // 2. PersonaId is 0 (indicating client credential request, not user request)
            if (!hasRequiredScope || currentClaims.PersonaId != 0)
            {
                return ClientCredentialAuthResult.Success(currentClaims);
            }

            // Resolve admin user for the organization
            var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId.Value);
            if (adminCreatorRealPageId == Guid.Empty)
            {
                return ClientCredentialAuthResult.Error(new ErrorResponse
                {
                    Errors = new List<Error>
                    {
                        new Error
                        {
                            Title = "Error",
                            Source = "/role",
                            Detail = "Invalid UPFMId.",
                            StatusCode = ""
                        }
                    }
                });
            }

            // Return success with current claims
            // NOTE: Original code intended to recreate claims but was commented out during .NET 10 migration
            // Preserving existing behavior by returning current claims unchanged
            return ClientCredentialAuthResult.Success(currentClaims);
        }
    }
}
