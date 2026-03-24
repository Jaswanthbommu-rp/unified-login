using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class RoleTypeRepositoryAsync : IRoleTypeRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ICacheService _cache;
    private readonly ILogger<RoleTypeRepositoryAsync> _logger;

    // Replaces: RPObjectCache with TTL 180 s
    private static readonly CacheEntryOptions RoleTypeCacheOptions = new() { ExpirationTimeInMinutes = 3 };

    public RoleTypeRepositoryAsync(
        IDbConnection db,
        ICacheService cache,
        ILogger<RoleTypeRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _cache  = cache  ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<RoleType>> GetRoleTypeAsync(
        string roleTypeName, long? partyId, CancellationToken cancellationToken = default)
    {
        // Replaces: RPObjectCache inline lambda
        return await _cache.GetOrSetAsync(
            $"sp_ListRoleType_{roleTypeName}_{partyId}",
            async ct =>
            {
                var param = string.IsNullOrEmpty(roleTypeName)
                    ? null
                    : (object)new { RoleTypeName = roleTypeName, OrganizationPartyID = partyId };

                var rows = await _db.QueryAsync<RoleType>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListRoleType,
                        param,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));

                return (IList<RoleType>)rows.ToList();
            },
            RoleTypeCacheOptions,
            cancellationToken) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<RoleType>> GetRoleTypeDependencyAsync(
        long? roleTypeId, long? partyId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<RoleType>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListRoleTypeDependency,
                new { PartyId = partyId, ParentRoleTypeID = roleTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }
}