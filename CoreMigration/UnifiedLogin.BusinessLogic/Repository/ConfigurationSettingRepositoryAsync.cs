using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Configuration Setting Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class ConfigurationSettingRepositoryAsync : IConfigurationSettingRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ICacheService _cache;
    private readonly ILogger<ConfigurationSettingRepositoryAsync> _logger;

    // Replaces: RPObjectCache inline TTL of 360 seconds = 6 minutes
    private static readonly CacheEntryOptions OrgSettingCacheOptions = new() { ExpirationTimeInMinutes = 6 };

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public ConfigurationSettingRepositoryAsync(
        IDbConnection db,
        ICacheService cache,
        ILogger<ConfigurationSettingRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _cache  = cache  ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IConfigurationSettingRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<IList<ConfigurationSetting>> ListUserLoginConfigurationSettingAsync(
        long PartyId,
        string SettingName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _db.QueryAsync<ConfigurationSetting>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_ListUserLoginSettings,
                    new { PartyId, SettingName },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for PartyId={PartyId} SettingName={SettingName}",
                nameof(ListUserLoginConfigurationSettingAsync), PartyId, SettingName);
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<IList<ConfigurationSetting>> ListOrganizationConfigurationSettingAsync(
        long PartyId,
        string SettingName,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"sp_ListOrganizationSettings_{PartyId}_{SettingName}";

        // Replaces: new RPObjectCache().GetFromCache(cacheKey, 360, factory)
        try
        {
            return await _cache.GetOrSetAsync(
                cacheKey,
                async ct =>
                {
                    var result = await _db.QueryAsync<ConfigurationSetting>(
                        new CommandDefinition(
                            StoredProcNameConstants.SP_ListOrganizationSettings,
                            new { PartyId, SettingName },
                            commandType: CommandType.StoredProcedure,
                            cancellationToken: ct));

                    return (IList<ConfigurationSetting>)result.ToList();
                },
                OrgSettingCacheOptions,
                cancellationToken) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for PartyId={PartyId} SettingName={SettingName}",
                nameof(ListOrganizationConfigurationSettingAsync), PartyId, SettingName);
            return [];
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateConfigurationSettingAsync(
        ConfigurationSetting configurationSetting,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateMasterConfigurationSetting,
                new
                {
                    MasterConfigurationSettingId = configurationSetting.MasterConfigurationSettingId,
                    Value                        = configurationSetting.Value
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateMasterConfigurationSettingAsync(
        MasterConfigurationSetting masterSetting,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateMasterConfigurationSetting,
                new
                {
                    MasterConfigurationType = masterSetting.ConfigurationType,
                    MasterSettingType       = masterSetting.SettingType,
                    PartyId                 = masterSetting.PartyId,
                    Value                   = masterSetting.Value
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSettingAsync(
        MasterConfigurationSetting masterSetting,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting,
                new
                {
                    PartyId     = masterSetting.PartyId,
                    MappingName = masterSetting.MappingName,
                    Value       = masterSetting.Value,
                    CreatedBy   = masterSetting.CreatedBy
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<int> GetOrganizationMasterConfigurationIdAsync(
        string Name,
        long PartyId,
        CancellationToken cancellationToken = default)
    {
        // Replaces: sync repository.Execute(...) with DynamicParameters output param
        var param = new DynamicParameters();
        param.Add("@PartyId",                PartyId, dbType: DbType.Int32,  direction: ParameterDirection.Input);
        param.Add("@Name",                   Name,    dbType: DbType.String, direction: ParameterDirection.Input);
        param.Add("@MasterConfigurationId",  0,       dbType: DbType.Int32,  direction: ParameterDirection.Output);

        try
        {
            await _db.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetOrganizationMasterConfiguration,
                    param,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return param.Get<int>("@MasterConfigurationId");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for Name={Name} PartyId={PartyId}",
                nameof(GetOrganizationMasterConfigurationIdAsync), Name, PartyId);
            return 0;
        }
    }

    #endregion
}