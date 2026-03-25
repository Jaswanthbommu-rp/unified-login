using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for contact mechanism management operations.
/// </summary>
public interface IManageContactMechanismAsync
{
    Task<RepositoryResponse> CreateContactMechanismAsync(CancellationToken cancellationToken = default);

    Task<RepositoryResponse> LinkContactMechanismToPartyAsync(Guid realPageId, IPartyContactMechanism partyContactMechanism, CancellationToken cancellationToken = default);

    Task<RepositoryResponse> LinkUsageTypeToPartyContactMechanismAsync(long partyContactMechanismId, int? contactMechanismUsageTypeId, CancellationToken cancellationToken = default);

    Task<RepositoryResponse> LinkGeographicBoundaryToContactMechanismAsync(IContactMechanismBoundary contactMechanismBoundary, CancellationToken cancellationToken = default);
}
