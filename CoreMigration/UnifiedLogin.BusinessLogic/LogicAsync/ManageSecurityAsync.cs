using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper around IManageSecurity.
/// Task.FromResult() will be replaced with truly-async repo calls once IPersonaRightRepositoryAsync exists.
/// </summary>
public sealed class ManageSecurityAsync : IManageSecurityAsync
{
    private readonly IManageSecurity _manageSecurity;

    public ManageSecurityAsync(IManageSecurity manageSecurity)
    {
        _manageSecurity = manageSecurity ?? throw new ArgumentNullException(nameof(manageSecurity));
    }

    public Task<ObjectOutput<RouteSecurity, IErrorData>> GetPersonaRightsAndActionsByRouteAsync(long personaId, string routeId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageSecurity.GetPersonaRightsAndActionsByRoute(personaId, routeId));
}
