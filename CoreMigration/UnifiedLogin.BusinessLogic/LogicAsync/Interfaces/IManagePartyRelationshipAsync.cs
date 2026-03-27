using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for party relationship management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManagePartyRelationship"/>.
/// </summary>
public interface IManagePartyRelationshipAsync
{
    /// <summary>
    /// Returns the relationship between two parties enriched with role and relationship types,
    /// or <c>null</c> when no matching record exists.
    /// </summary>
    Task<PartyRelationship?> GetPartyRelationshipAsync(
        Guid realPageIdFrom,
        Guid realPageIdTo,
        string? roleTypeNameFrom,
        string? roleTypeNameTo,
        string? relationshipTypeName,
        CancellationToken cancellationToken = default);

    /// <summary>Links one organisation to another.</summary>
    Task<RepositoryResponse> LinkOrganizationToOrganizationAsync(
        Guid realPageIdFrom,
        PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default);

    /// <summary>Links a person to an organisation.</summary>
    Task<RepositoryResponse> LinkPersonToOrganizationAsync(
        Guid realPageIdFrom,
        PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default);
}
