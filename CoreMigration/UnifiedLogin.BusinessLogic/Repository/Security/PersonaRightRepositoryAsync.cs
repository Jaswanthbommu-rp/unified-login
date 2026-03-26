using Dapper;
using System.Data;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.Repository.Security;

/// <summary>
/// Async-first persona rights repository using Dapper + <see cref="IDbConnectionFactory"/>.
/// Mirrors the 120-second <c>RPObjectCache</c> TTL of the sync <see cref="PersonaRightRepository"/>
/// via <see cref="ICacheService"/>.
/// </summary>
public sealed class PersonaRightRepositoryAsync : IPersonaRightRepositoryAsync
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ICacheService        _cache;

    private static readonly CacheEntryOptions PersonaRightsCacheOptions =
        new() { ExpirationTimeInMinutes = 2 }; // mirrors RPObjectCache 120s TTL

    public PersonaRightRepositoryAsync(IDbConnectionFactory dbFactory, ICacheService cache)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _cache     = cache     ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PersonaActionRight>> ListRightsAndActionsByPersonaIdAsync(
        long personaId,
        string routeId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"listRightsAndActionsByPersonaId_{personaId}_{routeId}";

        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                using var db = _dbFactory.CreateConnection();
                var result = await db.QueryAsync<PersonaActionRight>(new CommandDefinition(
                    StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute,
                    new { personaId, routeId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

                return result.AsEnumerable();
            },
            PersonaRightsCacheOptions) ?? [];
    }
}