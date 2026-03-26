using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManagePartyRoleAsync"/>.
/// All I/O is awaited via <see cref="IPartyRoleRepositoryAsync"/>.
/// No <c>new</c> keyword; no parameterless constructor.
/// </summary>
public sealed class ManagePartyRoleAsync : IManagePartyRoleAsync
{
    private readonly IPartyRoleRepositoryAsync _repository;

    public ManagePartyRoleAsync(IPartyRoleRepositoryAsync repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreatePartyRoleEnterpriseUserIDAsync(
        Guid realPageId,
        IPartyRole partyRole,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));
        ArgumentNullException.ThrowIfNull(partyRole);

        return _repository.CreatePartyRoleEnterpriseUserIDAsync(realPageId, partyRole, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<PartyRole?> GetPartyRoleAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));

        return _repository.GetPartyRoleByEnterpriseUserIDAsync(realPageId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IList<PartyRole>> GetPartyRolesAsync(
        long? partyId,
        CancellationToken cancellationToken = default)
    {
        if (partyId is null)
            throw new ArgumentNullException(nameof(partyId), "Invalid parameter partyId.");

        return _repository.GetPartyRolesAsync(partyId.Value, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> UpdatePartyRoleAsync(
        IPartyRole partyRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(partyRole);

        return _repository.UpdatePartyRoleEnterpriseUserIDAsync(partyRole, cancellationToken);
    }
}