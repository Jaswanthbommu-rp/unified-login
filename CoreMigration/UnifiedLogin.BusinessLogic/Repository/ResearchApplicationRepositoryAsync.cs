using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResearchApplication;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Research Application Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class ResearchApplicationRepositoryAsync : IResearchApplicationRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<ResearchApplicationRepositoryAsync> _logger;

    public ResearchApplicationRepositoryAsync(IDbConnection db, ILogger<ResearchApplicationRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<List<ProductRole>> ListRolesByPartyAsync(
        long partyId, CancellationToken cancellationToken = default)
    {
        // Replaces: GetMany<dynamic> + foreach mapping
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesByParty,
                new { partyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductRole>> ListRolesAssignedToPersonaAsync(
        long userPersonaId, CancellationToken cancellationToken = default)
    {
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRolesByParty,
                new { userPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertDelAssignedPropRoleToUserAsync(
        long userPersonaId, long productId, UserLocation property, UserAccessGroup role,
        long del = 0, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePropertyMapping,
                new
                {
                    PersonaID  = userPersonaId,
                    ProductID  = productId,
                    RoleID     = int.Parse(role.AccessGroupCode),
                    PropertyID = int.Parse(property.PropertyId),
                    Deleted    = del
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse { Id = rows };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertDelAssignedPropRoleToUserNewAsync(
        long userPersonaId, int productId, long propertyId, long roleId,
        long del = 0, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePropertyMapping,
                new
                {
                    PersonaID  = userPersonaId,
                    ProductID  = productId,
                    RoleID     = roleId,
                    PropertyID = propertyId,
                    Deleted    = del
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse { Id = rows };
    }
}