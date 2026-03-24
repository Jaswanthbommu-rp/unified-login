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
/// Async-first Global Setting Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class GlobalSettingRepositoryAsync : IGlobalSettingRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<GlobalSettingRepositoryAsync> _logger;

    public GlobalSettingRepositoryAsync(
        IDbConnection db,
        ILogger<GlobalSettingRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<GlobalSetting>> GetGlobalSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<GlobalSetting>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetGlobalSettings,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateGlobalSettingAsync(
        GlobalSetting setting,
        CancellationToken cancellationToken = default)
    {
        // Replaces: dynamic param with @ prefixes (unnecessary in Dapper)
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateMasterConfigurationSetting,
                new
                {
                    MasterConfigurationSettingId = setting.MasterConfigurationSettingId,
                    Value                        = setting.Value
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }
}