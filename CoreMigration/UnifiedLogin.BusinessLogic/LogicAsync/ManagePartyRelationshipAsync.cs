using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManagePartyRelationshipAsync"/>.
/// Replaces the stepping-stone that used <c>Task.FromResult(sync.GetPartyRelationship(...))</c>
/// and was missing the two link methods entirely.
/// All I/O is awaited via <see cref="IPartyRelationshipRepositoryAsync"/>.
/// </summary>
public sealed class ManagePartyRelationshipAsync : IManagePartyRelationshipAsync
{
    private readonly IPartyRelationshipRepositoryAsync _repository;

    public ManagePartyRelationshipAsync(IPartyRelationshipRepositoryAsync repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public Task<PartyRelationship?> GetPartyRelationshipAsync(
        Guid realPageIdFrom,
        Guid realPageIdTo,
        string? roleTypeNameFrom,
        string? roleTypeNameTo,
        string? relationshipTypeName,
        CancellationToken cancellationToken = default)
    {
        if (realPageIdFrom == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageIdFrom.", nameof(realPageIdFrom));
        if (realPageIdTo == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageIdTo.", nameof(realPageIdTo));

        return _repository.GetPartyRelationshipAsync(
            realPageIdFrom, realPageIdTo, roleTypeNameFrom, roleTypeNameTo, relationshipTypeName, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> LinkOrganizationToOrganizationAsync(
        Guid realPageIdFrom,
        PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default)
    {
        if (realPageIdFrom == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageIdFrom.", nameof(realPageIdFrom));
        ArgumentNullException.ThrowIfNull(partyRelationship);
        if (partyRelationship.RealPageIdTo == Guid.Empty)
            throw new ArgumentException("Invalid parameter partyRelationship.RealPageIdTo.", nameof(partyRelationship));
        if (partyRelationship.RoleTypeIdFrom <= 0)
            throw new ArgumentOutOfRangeException(nameof(partyRelationship), "Invalid parameter partyRelationship.RoleTypeIdFrom.");
        if (partyRelationship.RoleTypeIdTo <= 0)
            throw new ArgumentOutOfRangeException(nameof(partyRelationship), "Invalid parameter partyRelationship.RoleTypeIdTo.");

        return _repository.LinkOrganizationToOrganizationAsync(realPageIdFrom, partyRelationship, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> LinkPersonToOrganizationAsync(
        Guid realPageIdFrom,
        PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default)
    {
        if (realPageIdFrom == Guid.Empty)
            throw new ArgumentException("Invalid parameter realPageIdFrom.", nameof(realPageIdFrom));
        ArgumentNullException.ThrowIfNull(partyRelationship);
        if (partyRelationship.RealPageIdTo == Guid.Empty)
            throw new ArgumentException("Invalid parameter partyRelationship.RealPageIdTo.", nameof(partyRelationship));
        if (partyRelationship.RoleTypeIdFrom <= 0)
            throw new ArgumentOutOfRangeException(nameof(partyRelationship), "Invalid parameter partyRelationship.RoleTypeIdFrom.");
        if (partyRelationship.RoleTypeIdTo <= 0)
            throw new ArgumentOutOfRangeException(nameof(partyRelationship), "Invalid parameter partyRelationship.RoleTypeIdTo.");

        return _repository.LinkPersonToOrganizationAsync(realPageIdFrom, partyRelationship, cancellationToken);
    }
}
