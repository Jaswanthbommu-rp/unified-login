using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageSecurityAsync
{
    Task<ObjectOutput<RouteSecurity, IErrorData>> GetPersonaRightsAndActionsByRouteAsync(long personaId, string routeId, CancellationToken cancellationToken = default);
}
