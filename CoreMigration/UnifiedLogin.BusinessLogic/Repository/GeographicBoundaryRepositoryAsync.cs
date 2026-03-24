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
/// Async-first Geographic Boundary Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class GeographicBoundaryRepositoryAsync : IGeographicBoundaryRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<GeographicBoundaryRepositoryAsync> _logger;

    public GeographicBoundaryRepositoryAsync(
        IDbConnection db,
        ILogger<GeographicBoundaryRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateGeographicBoundaryAsync(
        IGeographicBoundary geographicBoundary,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateGeographicBoundary,
                new
                {
                    TypeName     = geographicBoundary.GeographicBoundaryType.TypeName,
                    Value        = geographicBoundary.Name,
                    Code         = geographicBoundary.GeographicBoundaryCode,
                    Abbreviation = geographicBoundary.Abbreviation
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }
}