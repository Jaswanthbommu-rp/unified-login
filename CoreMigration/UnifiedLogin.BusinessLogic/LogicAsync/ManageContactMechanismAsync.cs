using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for contact mechanism operations.
/// Delegates to the existing sync <see cref="IManageContactMechanism"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageContactMechanismAsync : IManageContactMechanismAsync
{
    private readonly IManageContactMechanism _manageContactMechanism;

    public ManageContactMechanismAsync(IManageContactMechanism manageContactMechanism)
    {
        _manageContactMechanism = manageContactMechanism ?? throw new ArgumentNullException(nameof(manageContactMechanism));
    }

    public Task<RepositoryResponse> CreateContactMechanismAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_manageContactMechanism.CreateContactMechanism());

    public Task<RepositoryResponse> LinkContactMechanismToPartyAsync(Guid realPageId, IPartyContactMechanism partyContactMechanism, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageContactMechanism.LinkContactMechanismToParty(realPageId, partyContactMechanism));

    public Task<RepositoryResponse> LinkUsageTypeToPartyContactMechanismAsync(long partyContactMechanismId, int? contactMechanismUsageTypeId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageContactMechanism.LinkUsageTypeToPartyContactMechanism(partyContactMechanismId, contactMechanismUsageTypeId));

    public Task<RepositoryResponse> LinkGeographicBoundaryToContactMechanismAsync(IContactMechanismBoundary contactMechanismBoundary, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageContactMechanism.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary));
}
