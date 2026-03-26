using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageSecurityAsync"/>.
/// The sole DB call (<c>ListRightsAndActionsByPersonaIdAsync</c>) is truly async via
/// <see cref="IPersonaRightRepositoryAsync"/>. All claims/HTTP-context access remains synchronous
/// since it reads in-memory request state.
/// </summary>
public sealed class ManageSecurityAsync(
    IPersonaRightRepositoryAsync _personaRightRepository,
    IUserClaimsAccessor          _userClaimsAccessor,
    IHttpContextAccessor         _httpContextAccessor) : IManageSecurityAsync
{
    /// <inheritdoc/>
    public async Task<ObjectOutput<RouteSecurity, IErrorData>> GetPersonaRightsAndActionsByRouteAsync(
        long personaId,
        string routeId,
        CancellationToken cancellationToken = default)
    {
        var output       = new ObjectOutput<RouteSecurity, IErrorData>();
        var status       = output.Status = new Status<IErrorData>();
        var routeSecurity = output.obj   = new RouteSecurity();

        if (personaId == 0 || string.IsNullOrWhiteSpace(routeId))
        {
            status.ErrorCode = "100.1";
            status.ErrorMsg  = "Invalid persona Id or route id.";
            status.Success   = false;
            return output;
        }

        var impersonatedBy           = _userClaimsAccessor.ImpersonatedBy;
        var organizationRealPageGuid = _userClaimsAccessor.OrganizationRealPageGuid;

        if (routeId.Equals("adgroups", StringComparison.OrdinalIgnoreCase) && impersonatedBy != Guid.Empty)
        {
            ClaimsPrincipal? currentClaimPrincipal = _httpContextAccessor.HttpContext?.User;
            output.obj.RouteId = routeId;
            output.obj.Rights  = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaimsAccessor.GetUserClaim());
        }
        else
        {
            var actionRights = (await _personaRightRepository
                .ListRightsAndActionsByPersonaIdAsync(personaId, routeId, cancellationToken))
                .ToList();

            if (actionRights.Count > 0)
            {
                routeSecurity.RouteId = actionRights
                    .SingleOrDefault(ar => ar.ObjectType.Equals("Route", StringComparison.OrdinalIgnoreCase))
                    ?.Action;

                routeSecurity.Rights = (impersonatedBy != Guid.Empty
                    ? actionRights.Where(ar =>
                        ar.ObjectType.Equals("Right", StringComparison.OrdinalIgnoreCase)
                        && ar.IsExcludeRightFromImpersonation != true)
                    : actionRights.Where(ar =>
                        ar.ObjectType.Equals("Right", StringComparison.OrdinalIgnoreCase)))
                    .Select(ar => ar.Action)
                    .ToList();

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
