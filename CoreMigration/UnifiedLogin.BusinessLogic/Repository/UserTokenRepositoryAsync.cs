using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class UserTokenRepositoryAsync : IUserTokenRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<UserTokenRepositoryAsync> _logger;

    public UserTokenRepositoryAsync(IDbConnection db, ILogger<UserTokenRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: sync guard + empty-string fallback with early return on invalid input.
    /// </remarks>
    public async Task<string> GetUserActivityTokenAsync(
        Guid realPageId, int activityTypeId, long partyId,
        CancellationToken cancellationToken = default)
    {
        // Replaces: if (realPageId != Guid.Empty && activityTypeId > 0) { ... } return ""
        if (realPageId == Guid.Empty || activityTypeId <= 0)
            return string.Empty;

        return await _db.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateActivityToken,
                new { partyId, realPageId, activityTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? string.Empty;
    }
}