using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for party role management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManagePartyRole"/>.
/// </summary>
public interface IManagePartyRoleAsync
{
    /// <summary>Creates a party role linked to the given user.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="realPageId"/> is <see cref="Guid.Empty"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="partyRole"/> is null.</exception>
    Task<RepositoryResponse> CreatePartyRoleEnterpriseUserIDAsync(
        Guid realPageId,
        IPartyRole partyRole,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the party role for the given user, or <c>null</c> when not found.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="realPageId"/> is <see cref="Guid.Empty"/>.</exception>
    Task<PartyRole?> GetPartyRoleAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all party roles for the given party.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="partyId"/> is null.</exception>
    Task<IList<PartyRole>> GetPartyRolesAsync(
        long? partyId,
        CancellationToken cancellationToken = default);

    /// <summary>Updates an existing party role.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="partyRole"/> is null.</exception>
    Task<RepositoryResponse> UpdatePartyRoleAsync(
        IPartyRole partyRole,
        CancellationToken cancellationToken = default);
}