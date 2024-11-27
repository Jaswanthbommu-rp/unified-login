using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using System;
using System.Linq;
using System.Security.Claims;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class ManageSecurity : IManageSecurity
    {
        private readonly IPersonaRightRepository _personaRightRepository;
        private readonly DefaultUserClaim _userClaim;

        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        public ManageSecurity(DefaultUserClaim userClaim)
        {
            _personaRightRepository = new PersonaRightRepository();
            _userClaim = userClaim;
        }

        /// <summary>
        /// 
        /// </summary>
        public ManageSecurity(DefaultUserClaim userClaim, IPersonaRightRepository personaRightRepository)
        {
            _personaRightRepository = personaRightRepository;
            _userClaim = userClaim;
        }
        #endregion

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

            if ((routeId.ToLower() == "adgroups" || routeId.ToLower() == "editusers") && _userClaim.ImpersonatedBy != Guid.Empty)
            {
                ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
                output.obj.RouteId = routeId;
                output.obj.Rights = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaim);
            }
            else
            {
                var actionRights = _personaRightRepository.ListRightsAndActionsByPersonaId(personaId, routeId);
                if (actionRights.Any())
                {
                    routeSecurity.RouteId = actionRights
                        .SingleOrDefault(ar => ar.ObjectType.Equals("Route", StringComparison.OrdinalIgnoreCase))
                        .Action;


                    if (_userClaim.ImpersonatedBy != Guid.Empty)
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

                    if (_userClaim.OrganizationRealPageGuid == DefaultUserClaim.ExternalCompanyRealPageId)
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