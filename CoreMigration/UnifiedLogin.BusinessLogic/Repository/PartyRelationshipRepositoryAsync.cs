using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Party Relationship Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class PartyRelationshipRepositoryAsync : IPartyRelationshipRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly IRoleTypeRepositoryAsync _roleTypeRepository;
    private readonly IRelationshipTypeRepositoryAsync _relationshipTypeRepository;
    private readonly ILogger<PartyRelationshipRepositoryAsync> _logger;

    public PartyRelationshipRepositoryAsync(
        IDbConnection db,
        IRoleTypeRepositoryAsync roleTypeRepository,
        IRelationshipTypeRepositoryAsync relationshipTypeRepository,
        ILogger<PartyRelationshipRepositoryAsync> logger)
    {
        _db                      = db                      ?? throw new ArgumentNullException(nameof(db));
        _roleTypeRepository      = roleTypeRepository      ?? throw new ArgumentNullException(nameof(roleTypeRepository));
        _relationshipTypeRepository = relationshipTypeRepository ?? throw new ArgumentNullException(nameof(relationshipTypeRepository));
        _logger                  = logger                  ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<PartyRelationship> GetPartyRelationshipAsync(
        Guid realPageIdFrom, Guid realPageIdTo,
        string roleTypeNameFrom, string roleTypeNameTo, string relationshipTypeName,
        CancellationToken cancellationToken = default)
    {
        // Replaces: four branching dynamic param blocks
        object param = (!string.IsNullOrEmpty(roleTypeNameFrom) && !string.IsNullOrEmpty(relationshipTypeName))
            ? new { RealPageIdFrom = realPageIdFrom, RealPageIdTo = realPageIdTo, RoleTypeName = roleTypeNameFrom, RelationshipTypeName = relationshipTypeName }
            : !string.IsNullOrEmpty(relationshipTypeName)
                ? new { RealPageIdFrom = realPageIdFrom, RealPageIdTo = realPageIdTo, RelationshipTypeName = relationshipTypeName }
                : (object)new { RealPageIdFrom = realPageIdFrom, RealPageIdTo = realPageIdTo };

        // Fetch relationship row + lookup lists concurrently
        var relationshipTask   = _db.QuerySingleOrDefaultAsync<PartyRelationship>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var roleTypesFromTask  = _roleTypeRepository.GetRoleTypeAsync(roleTypeNameFrom, null, cancellationToken);
        var roleTypesToTask    = _roleTypeRepository.GetRoleTypeAsync(roleTypeNameTo, null, cancellationToken);
        var relTypesTask       = _relationshipTypeRepository.GetRelationshipTypeAsync(relationshipTypeName, cancellationToken);

        await Task.WhenAll(relationshipTask, roleTypesFromTask, roleTypesToTask, relTypesTask);

        var result            = await relationshipTask;
        if (result is null) return null!;

        var roleTypesFrom     = await roleTypesFromTask;
        var roleTypesTo       = await roleTypesToTask;
        var relationshipTypes = await relTypesTask;

        // Replaces: .First(...) (throws) → .FirstOrDefault(...) (safe)
        result.RoleTypeFrom          = roleTypesFrom.FirstOrDefault(r => r.PartyRoleTypeId == result.RoleTypeIdFrom);
        result.RoleTypeTo            = roleTypesTo.FirstOrDefault(r => r.PartyRoleTypeId == result.RoleTypeIdTo);
        result.PartyRelationshipType = relationshipTypes.FirstOrDefault(r => r.RelationshipTypeId == result.PartyRelationshipTypeId);

        return result;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkPersonToOrganizationAsync(
        Guid realPageIdFrom, PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonToOrganization,
                new
                {
                    PersonRealPageId       = realPageIdFrom,
                    OrganizationRealPageId = partyRelationship.RealPageIdTo,
                    RoleTypeIdFrom         = partyRelationship.RoleTypeIdFrom,
                    RoleTypeIdTo           = partyRelationship.RoleTypeIdTo
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkOrganizationToOrganizationAsync(
        Guid realPageIdFrom, PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkOrganizationToOrganization,
                new
                {
                    OrganizationRealPageIdFrom = realPageIdFrom,
                    OrganizationRealPageIdTo   = partyRelationship.RealPageIdTo,
                    RoleTypeIdFrom             = partyRelationship.RoleTypeIdFrom,
                    RoleTypeIdTo               = partyRelationship.RoleTypeIdTo
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }
}