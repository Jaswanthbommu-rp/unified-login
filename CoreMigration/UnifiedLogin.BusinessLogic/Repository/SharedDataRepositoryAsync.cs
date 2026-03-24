using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Shared Data Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// <para>
/// Replaces: three constructors (+DefaultUserClaim, +RPObjectCache) with
/// single DI constructor. Cache is provided by <see cref="ICacheService"/>.
/// </para>
/// </summary>
public sealed class SharedDataRepositoryAsync : ISharedDataRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ICacheService _cache;
    private readonly ILogger<SharedDataRepositoryAsync> _logger;

    // Replaces: RPObjectCache TTL of 180 s
    private static readonly CacheEntryOptions ProductIdCacheOptions = new() { ExpirationTimeInMinutes = 3 };

    public SharedDataRepositoryAsync(
        IDbConnection db,
        ICacheService cache,
        ILogger<SharedDataRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _cache  = cache  ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<int>> GetProductIdsByCompanyAsync(
        Guid organizationRealPageId, CancellationToken cancellationToken = default)
    {
        // Replaces: RPObjectCache + foreach pui loop
        return await _cache.GetOrSetAsync(
            $"getProductIdsByCompanyGuid_{organizationRealPageId}",
            async ct =>
            {
                var rows = await _db.QueryAsync<ProductUI>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListProductsByOrganization,
                        new { OrganizationRealPageId = organizationRealPageId },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));

                return (IList<int>)rows.Select(p => p.ProductId).ToList();
            },
            ProductIdCacheOptions,
            cancellationToken) ?? [];
    }

    /// <inheritdoc/>
    public async Task<IList<int>> GetProductIdsByCompanyAsync(
        long organizationPartyId, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(
            $"getProductIdsByCompanyPartyId_{organizationPartyId}",
            async ct =>
            {
                var rows = await _db.QueryAsync<ProductUI>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListProductsByOrganization,
                        new { PartyId = organizationPartyId },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));

                return (IList<int>)rows.Select(p => p.ProductId).ToList();
            },
            ProductIdCacheOptions,
            cancellationToken) ?? [];
    }
}