using System;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async data-access interface for party relationship records.
/// Replaces: sync <see cref="IPartyRelationshipRepository"/>.
/// </summary>
public interface IPartyRelationshipRepositoryAsync
{
    /// <summary>
    /// Returns the relationship between two parties, enriched with
    /// <see cref="RoleType"/> (from/to) and <see cref="RelationshipType"/>.
    /// Returns <c>null</c> when no matching relationship is found.
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