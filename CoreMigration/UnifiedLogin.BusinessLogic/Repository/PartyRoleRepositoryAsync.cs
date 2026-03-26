using Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first party role repository using Dapper + <see cref="IDbConnectionFactory"/>.
/// Each method obtains its own connection from the factory so concurrent callers never share a connection.
/// </summary>
public sealed class PartyRoleRepositoryAsync : IPartyRoleRepositoryAsync
{
    private readonly IDbConnectionFactory _dbFactory;

    public PartyRoleRepositoryAsync(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreatePartyRoleEnterpriseUserIDAsync(
        Guid realPageId,
        IPartyRole partyRole,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_CreatePartyRoleByRealPageId,
            new { realPageId, partyRole.RoleTypeId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<PartyRole?> GetPartyRoleByEnterpriseUserIDAsync(
        Guid realPageId,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        return await db.QuerySingleOrDefaultAsync<PartyRole>(new CommandDefinition(
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
        using var db = _dbFactory.CreateConnection();
        var result = await db.QueryAsync<PartyRole>(new CommandDefinition(
            StoredProcNameConstants.SP_GetPartyRole,
            new { partyId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePartyRoleEnterpriseUserIDAsync(
        IPartyRole partyRole,
        CancellationToken cancellationToken = default)
    {
        using var db = _dbFactory.CreateConnection();
        var result = await db.QuerySingleOrDefaultAsync<RepositoryResponse>(new CommandDefinition(
            StoredProcNameConstants.SP_UpdatePartyRoleByRealPageId,
            new { PartyRoleId = partyRole.PartyRoleId, RoleTypeID = partyRole.RoleTypeId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return result ?? new RepositoryResponse();
    }
}