using Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first party relationship repository using Dapper + <see cref="IDbConnectionFactory"/>.
/// Each method obtains its own connection from the factory so concurrent callers never share a connection.
/// The enrichment lookups (RoleType × 2, RelationshipType) in
/// <see cref="GetPartyRelationshipAsync"/> run concurrently via <see cref="Task.WhenAll"/>.
/// </summary>
public sealed class PartyRelationshipRepositoryAsync : IPartyRelationshipRepositoryAsync
{
    private readonly IDbConnectionFactory            _dbFactory;
    private readonly IRoleTypeRepositoryAsync        _roleTypeRepository;
    private readonly IRelationshipTypeRepositoryAsync _relationshipTypeRepository;

    public PartyRelationshipRepositoryAsync(
        IDbConnectionFactory             dbFactory,
        IRoleTypeRepositoryAsync         roleTypeRepository,
        IRelationshipTypeRepositoryAsync  relationshipTypeRepository)
    {
        _dbFactory                  = dbFactory                  ?? throw new ArgumentNullException(nameof(dbFactory));
        _roleTypeRepository         = roleTypeRepository         ?? throw new ArgumentNullException(nameof(roleTypeRepository));
        _relationshipTypeRepository = relationshipTypeRepository ?? throw new ArgumentNullException(nameof(relationshipTypeRepository));
    }

    /// <inheritdoc/>
    public async Task<PartyRelationship?> GetPartyRelationshipAsync(
        Guid realPageIdFrom,
        Guid realPageIdTo,
        string? roleTypeNameFrom,
        string? roleTypeNameTo,
        string? relationshipTypeName,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Build SP parameter — mirrors sync branching logic exactly ──
        object param = BuildGetParam(realPageIdFrom, realPageIdTo, roleTypeNameFrom, relationshipTypeName);

        // ── 2. Fetch the raw relationship record ──────────────────────────
        PartyRelationship? result;
        using (var db = _dbFactory.CreateConnection())
        {
            result = await db.QuerySingleOrDefaultAsync<PartyRelationship>(new CommandDefinition(
                StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        }

        if (result is null) return null;

        // ── 3. Enrich with RoleType (from/to) and RelationshipType ────────
        // All three lookups are independent — run concurrently, each on its own connection.
        var roleTypeFromTask         = _roleTypeRepository.GetRoleTypeAsync(roleTypeNameFrom, null, cancellationToken);
        var roleTypeToTask           = _roleTypeRepository.GetRoleTypeAsync(roleTypeNameTo,   null, cancellationToken);
        var relationshipTypesTask    = _relationshipTypeRepository.GetRelationshipTypeAsync(relationshipTypeName, cancellationToken);

        await Task.WhenAll(roleTypeFromTask, roleTypeToTask, relationshipTypesTask);

        var roleTypesFrom       = await roleTypeFromTask;
        var roleTypesTo         = await roleTypeToTask;
        var relationshipTypes   = await relationshipTypesTask;

        result.RoleTypeFrom = roleTypesFrom
            .FirstOrDefault(i => i.PartyRoleTypeId == result.RoleTypeIdFrom);

        result.RoleTypeTo = roleTypesTo
            .FirstOrDefault(i => i.PartyRoleTypeId == result.RoleTypeIdTo);

        result.PartyRelationshipType = relationshipTypes
            .FirstOrDefault(i => i.RelationshipTypeId == result.PartyRelationshipTypeId);

        return result;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkOrganizationToOrganizationAsync(
        Guid realPageIdFrom,
        PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_LinkOrganizationToOrganization,
            new
            {
                OrganizationRealPageIdFrom = realPageIdFrom,
                OrganizationRealPageIdTo   = partyRelationship.RealPageIdTo,
                partyRelationship.RoleTypeIdFrom,
                partyRelationship.RoleTypeIdTo
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> LinkPersonToOrganizationAsync(
        Guid realPageIdFrom,
        PartyRelationship partyRelationship,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_LinkPersonToOrganization,
            new
            {
                PersonRealPageId       = realPageIdFrom,
                OrganizationRealPageId = partyRelationship.RealPageIdTo,
                partyRelationship.RoleTypeIdFrom,
                partyRelationship.RoleTypeIdTo
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result ?? new RepositoryResponse();
    }

    // ── Private helper — mirrors PartyRelationshipRepository parameter branching ──

    private static object BuildGetParam(
        Guid realPageIdFrom, Guid realPageIdTo,
        string? roleTypeNameFrom, string? relationshipTypeName)
    {
        bool noRoleType         = string.IsNullOrEmpty(roleTypeNameFrom);
        bool noRelationshipType = string.IsNullOrEmpty(relationshipTypeName);

        if (noRoleType && noRelationshipType)
            return new { RealPageIdFrom = realPageIdFrom, RealPageIdTo = realPageIdTo };

        if (noRoleType)
            return new { RealPageIdFrom = realPageIdFrom, RealPageIdTo = realPageIdTo, RelationshipTypeName = relationshipTypeName };

        if (noRelationshipType)
            // Preserves sync behaviour: passes RelationshipTypeName = null when empty
            return new { RealPageIdFrom = realPageIdFrom, RealPageIdTo = realPageIdTo, RelationshipTypeName = relationshipTypeName };

        return new { RealPageIdFrom = realPageIdFrom, RealPageIdTo = realPageIdTo, RoleTypeName = roleTypeNameFrom, RelationshipTypeName = relationshipTypeName };
    }
}