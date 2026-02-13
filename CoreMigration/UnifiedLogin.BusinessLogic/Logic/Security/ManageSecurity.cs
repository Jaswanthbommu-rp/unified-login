using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.Logic.Security
{
    /// <summary>
    /// Manages security rights and actions for personas.
    /// Refactored to use dependency injection and IUserClaimsAccessor for decoupled, testable code.
    /// </summary>
    public class ManageSecurity(IPersonaRightRepository _personaRightRepository, IUserClaimsAccessor _userClaimsAccessor, IHttpContextAccessor _httpContextAccessor) : IManageSecurity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public ObjectOutput<RouteSecurity, IErrorData> GetPersonaRightsAndActionsByRoute(long personaId, string routeId)
        {
            var output = new ObjectOutput<RouteSecurity, IErrorData>();
            var status = output.Status = new Status<IErrorData>();
            var routeSecurity = output.obj = new RouteSecurity();
            if (personaId == 0 || string.IsNullOrWhiteSpace(routeId))
            {
                status.ErrorCode = "100.1";
                status.ErrorMsg = "Invalid persona Id or route id.";
                status.Success = false;
                return output;
            }

            // Use the abstracted user claims accessor instead of direct DefaultUserClaim dependency
            var impersonatedBy = _userClaimsAccessor.ImpersonatedBy;
            var organizationRealPageGuid = _userClaimsAccessor.OrganizationRealPageGuid;

            if (routeId.ToLower() == "adgroups" && impersonatedBy != Guid.Empty)
            {
                ClaimsPrincipal currentClaimPrincipal = _httpContextAccessor.HttpContext?.User;
                output.obj.RouteId = routeId;
                output.obj.Rights = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaimsAccessor.GetUserClaim());
            }
            else
            {
                var actionRights = _personaRightRepository.ListRightsAndActionsByPersonaId(personaId, routeId);
                if (actionRights.Any())
                {
                    routeSecurity.RouteId = actionRights.SingleOrDefault(ar => ar.ObjectType.Equals("Route", StringComparison.OrdinalIgnoreCase))?.Action;

                    if (impersonatedBy != Guid.Empty)
                    {
                        routeSecurity.Rights = actionRights
                                                        .Where(ar => ar.ObjectType.Equals("Right", StringComparison.OrdinalIgnoreCase) && ar.IsExcludeRightFromImpersonation != true)
                                                        .Select(ar => ar.Action)
                                                        .ToList();
                    }
                    else
                    {
                        routeSecurity.Rights = actionRights
                                                        .Where(ar => ar.ObjectType.Equals("Right", StringComparison.OrdinalIgnoreCase))
                                                        .Select(ar => ar.Action)
                                                        .ToList();
                    }

                    routeSecurity.ProductRights = actionRights
                                                        .Where(ar => ar.ObjectType.Equals("Right", StringComparison.OrdinalIgnoreCase))
                                                        .Select(ar => new ProductRights { RightName = ar.Action, ProductId = ar.ProductId })
                                                        .ToList();

                    if (organizationRealPageGuid == DefaultUserClaim.ExternalCompanyRealPageId)
                    {
                        routeSecurity.Rights.Remove("Clone User");
                        routeSecurity.Rights.Remove("Create User");
                    }
                }
            }
            return output;
        }
    }
}