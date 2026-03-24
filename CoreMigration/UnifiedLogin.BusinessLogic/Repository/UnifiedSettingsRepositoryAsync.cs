using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

public sealed class UnifiedSettingsRepositoryAsync : IUnifiedSettingsRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<UnifiedSettingsRepositoryAsync> _logger;

    public UnifiedSettingsRepositoryAsync(IDbConnection db, ILogger<UnifiedSettingsRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<Setting>> GetUnifiedSettingsAsync(
        long partyId, string category, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<Setting>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUnifiedSetting,
                new { PartyId = partyId, Category = category },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }
}