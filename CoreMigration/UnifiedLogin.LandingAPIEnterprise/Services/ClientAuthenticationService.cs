using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for handling client credential authentication
    /// </summary>
    public interface IClientAuthenticationService
    {
        Task<ErrorResponse> AuthenticateClientAsync(Guid? upfmId, ClaimsPrincipal user, DefaultUserClaim userClaims);
    }

    public class ClientAuthenticationService : IClientAuthenticationService
    {
        private readonly IManageOrganization _manageOrganization;
        private readonly IManagePerson _managePerson;
        private readonly IManagePersona _managePersona;
        private readonly IManageUserLogin _manageUserLogin;

        public ClientAuthenticationService(
            IManageOrganization manageOrganization,
            IManagePerson managePerson,
            IManagePersona managePersona,
            IManageUserLogin manageUserLogin)
        {
            _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
            _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
        }

        public async Task<ErrorResponse> AuthenticateClientAsync(Guid? upfmId, ClaimsPrincipal user, DefaultUserClaim userClaims)
        {
            // If no upfmId provided, no client authentication needed
            if (!upfmId.HasValue || upfmId == Guid.Empty)
                return null;

            // Check if already authenticated with persona
            if (!HasValidScope(user) || userClaims.PersonaId != 0)
                return null;

            // Get admin user for organization
            var adminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId.Value);
            
            if (adminCreatorRealPageId == Guid.Empty)
            {
                return new ErrorResponse
                {
                    Errors = new List<Error>
                    {
                        new Error
                        {
                            Title = "Error",
                            Source = "/user",
                            Detail = "Invalid UPFMId.",
                            StatusCode = ""
                        }
                    }
                };
            }

            // Recreate claims for client
            return await RecreateClaimsForClientAsync(adminCreatorRealPageId, upfmId, userClaims);
        }

        private bool HasValidScope(ClaimsPrincipal user)
        {
            return user.HasClaim("scope", "usermanagement") || user.HasClaim("scope", "internalapi");
        }

        private async Task<ErrorResponse> RecreateClaimsForClientAsync(Guid realpageUserId, Guid? upfmId, DefaultUserClaim userClaims)
        {
            try
            {
                // Get person information
                var person = _managePerson.GetPerson(realpageUserId);
                if (person == null)
                {
                    return CreateErrorResponse($"Missing persona information for client_info user while Recreation of Claims For Client. realPageId: {realpageUserId}");
                }

                // Get user login
                var userLogin = _manageUserLogin.GetUserLoginOnly(realpageUserId);
                if (userLogin == null)
                {
                    return CreateErrorResponse($"User login not found for realPageId: {realpageUserId}");
                }

                // Get appropriate persona based on upfmId
                Persona persona;
                bool isRealPageEmployee = false;

                if (upfmId.HasValue)
                {
                    // Get persona for specific company
                    var personas = _managePersona.ListPersona(realpageUserId);
                    persona = personas.FirstOrDefault(p => p.Organization.RealPageId == upfmId.Value);
                    
                    if (persona == null)
                    {
                        return CreateErrorResponse($"No persona found for upfmId: {upfmId.Value}");
                    }

                    // Check if user is RealPage employee
                    isRealPageEmployee = personas.Any(p => 
                        string.Equals(p.Organization.Name, "REALPAGE EMPLOYEE", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    // Get active persona (single company user)
                    persona = _managePersona.GetActivePersonaWithoutRights(realpageUserId);
                    
                    if (persona == null)
                    {
                        return CreateErrorResponse($"No active persona found for realPageId: {realpageUserId}");
                    }

                    // Check if organization is RealPage Employee
                    isRealPageEmployee = string.Equals(
                        persona.Organization.Name, 
                        "REALPAGE EMPLOYEE", 
                        StringComparison.OrdinalIgnoreCase);
                }

                // Update user claims with new identity (matching BaseApiController logic)
                userClaims.UserId = (int)userLogin.UserId;
                userClaims.LoginName = userLogin.LoginName;
             //   userClaims.Email = userLogin.LoginName;
                userClaims.FirstName = person.FirstName;
                userClaims.LastName = person.LastName;
                userClaims.OrganizationMasterId = persona.Organization.BooksMasterId;
                userClaims.CustomerMasterId = persona.Organization.BooksMasterId;
                userClaims.OrganizationPartyId = persona.Organization.PartyId;
                userClaims.OrganizationName = persona.Organization.Name;
                userClaims.OrganizationRealPageGuid = persona.Organization.RealPageId;
                userClaims.UserRealPageGuid = realpageUserId;
                userClaims.PersonaId = persona.PersonaId;
                userClaims.IsRPEmployee = isRealPageEmployee;
                userClaims.RealPageEmployee = isRealPageEmployee;

                // Generate new correlation ID if not set
                if (userClaims.CorrelationId == Guid.Empty)
                {
                    userClaims.CorrelationId = Guid.NewGuid();
                }

                return null; // Success
            }
            catch (Exception ex)
            {
                return CreateErrorResponse($"Error recreating claims: {ex.Message}");
            }
        }

        private ErrorResponse CreateErrorResponse(string detail)
        {
            return new ErrorResponse
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Title = "Authentication Error",
                        Source = "/authentication",
                        Detail = detail,
                        StatusCode = "401"
                    }
                }
            };
        }
    }
}
