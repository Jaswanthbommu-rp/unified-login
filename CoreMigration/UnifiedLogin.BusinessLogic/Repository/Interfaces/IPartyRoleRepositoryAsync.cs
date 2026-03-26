using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async data-access interface for party role records.
/// Replaces: sync <see cref="IPartyRoleRepository"/>.
/// </summary>
public interface IPartyRoleRepositoryAsync
{
    /// <summary>Creates a party role linked to the given user.</summary>
    Task<RepositoryResponse> CreatePartyRoleEnterpriseUserIDAsync(
        Guid realPageId,
        IPartyRole partyRole,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the single party role for the given user, or <c>null</c> when not found.</summary>
    Task<PartyRole?> GetPartyRoleByEnterpriseUserIDAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all party roles for the given party.</summary>
    Task<IList<PartyRole>> GetPartyRolesAsync(
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>Updates an existing party role.</summary>
    Task<RepositoryResponse> UpdatePartyRoleEnterpriseUserIDAsync(
        IPartyRole partyRole,
        CancellationToken cancellationToken = default);
}