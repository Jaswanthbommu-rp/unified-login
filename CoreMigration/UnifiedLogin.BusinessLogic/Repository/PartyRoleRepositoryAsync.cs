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
/// Async-first Party Role Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class PartyRoleRepositoryAsync : IPartyRoleRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<PartyRoleRepositoryAsync> _logger;

    public PartyRoleRepositoryAsync(
        IDbConnection db,
        ILogger<PartyRoleRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreatePartyRoleEnterpriseUserIdAsync(
        Guid realPageId, IPartyRole partyRole,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePartyRoleByRealPageId,
                new { realPageId, partyRole.RoleTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<PartyRole> GetPartyRoleByEnterpriseUserIdAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<PartyRole>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPartyRoleByRealPageId,
                new { realPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<PartyRole>> GetPartyRolesAsync(
        long partyId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<PartyRole>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPartyRole,
                new { partyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePartyRoleEnterpriseUserIdAsync(
        IPartyRole partyRole,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePartyRoleByRealPageId,
                new { PartyRoleId = partyRole.PartyRoleId, RoleTypeID = partyRole.RoleTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }
}