using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class RelationshipTypeRepositoryAsync : IRelationshipTypeRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<RelationshipTypeRepositoryAsync> _logger;

    public RelationshipTypeRepositoryAsync(IDbConnection db, ILogger<RelationshipTypeRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<RelationshipType>> GetRelationshipTypeAsync(
        string relationshipTypeName, CancellationToken cancellationToken = default)
    {
        // Replaces: dynamic param = null + conditional assignment
        var param = string.IsNullOrEmpty(relationshipTypeName)
            ? null
            : (object)new { RelationshipTypeName = relationshipTypeName };

        var result = await _db.QueryAsync<RelationshipType>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRelationshipType,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<UserRelationShipType>> GetUserRelationShipTypesAsync(
        long partyId, CancellationToken cancellationToken = default)
    {
        var param = partyId != 0 ? (object)new { PartyId = partyId } : null;

        var result = await _db.QueryAsync<UserRelationShipType>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListUserRelationshipTypes,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }
}