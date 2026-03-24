using Dapper;
using RealPage.DataAccess.Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Product Internal Setting Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class ProductInternalSettingRepositoryAsync : IProductInternalSettingRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductInternalSettingRepositoryAsync> _logger;

    // Replaces: RPObjectCache inline TTL of 180 seconds = 3 minutes
    private static readonly CacheEntryOptions SettingByTypeCacheOptions = new() { ExpirationTimeInMinutes = 3 };

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public ProductInternalSettingRepositoryAsync(
        IDbConnection db,
        ICacheService cache,
        ILogger<ProductInternalSettingRepositoryAsync> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IProductInternalSettingRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<IList<ProductInternalSetting>> GetProductInternalSettingsAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<ProductInternalSetting>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                new { ProductId = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<ProductInternalSettingByType>> GetProductSettingByTypeAsync(
        string productSettingType,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"productInternalSettingByType_{productSettingType}";

        // Replaces: new RPObjectCache().GetFromCache(cacheKey, 180, factory)
        return await _cache.GetOrSetAsync(
            cacheKey,
            async ct =>
            {
                var result = await _db.QueryAsync<ProductInternalSettingByType>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListProductGlobalSettingsBySettingType,
                        new { ProductSettingType = productSettingType },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: ct));

                return (IList<ProductInternalSettingByType>)result.ToList();
            },
            SettingByTypeCacheOptions,
            cancellationToken) ?? [];
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateProductSettingAndLinkToConfigurationAsync(
        int productId,
        ProductInternalSetting productInternalSetting,
        CancellationToken cancellationToken = default)
    {
        var response = new RepositoryResponse();

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            // Step 1 — resolve the productSettingTypeId via output parameter
            // Replaces: sync repository.Execute(SP_GetProductSettingType, dynparam)
            var typeParam = new DynamicParameters();
            typeParam.Add("@Name", productInternalSetting.Name, dbType: DbType.String, direction: ParameterDirection.Input);
            typeParam.Add("@ProductSettingTypeId", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            try
            {
                await _db.ExecuteAsync(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetProductSettingType,
                        typeParam,
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken));
            }
            catch
            {
                // SP not found — productSettingTypeId remains 0, caught below
            }

            var productSettingTypeId = typeParam.Get<int>("@ProductSettingTypeId");

            if (productSettingTypeId == 0)
                throw new InvalidOperationException("Only known product setting types are supported");

            // Step 2 — create the product setting
            var createParam = new
            {
                ProductId = productId,
                ProductSettingTypeId = productSettingTypeId,
                Value = productInternalSetting.Value,
                FromDate = DateTime.UtcNow,
                ProductSettingId = 0
            };

            var createResponse = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateProductSetting,
                    createParam,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            _logger.LogDebug(
                "{ActionName} - {State}",
                "CreateProductSettingAndLinkToConfiguration",
                $"Adding setting productid:{productId} ProductSettingTypeId:{productSettingTypeId} Value:{productInternalSetting.Value} Response:{JsonConvert.SerializeObject(createResponse)}");

            if (createResponse.Id == 0)
            {
                _logger.LogError(
                    "{ActionName} - {State}",
                    "CreateProductSettingAndLinkToConfiguration",
                    "Error: CreateProductSetting failed.");

                createResponse.ErrorMessage = "CreateProductSettingAndLinkToConfiguration.CreateProductSetting Error: CreateProductSetting failed.";
                tx.Rollback();
                return createResponse;
            }

            // Step 3 — link setting to configuration
            response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_LinkProductSettingToConfiguration,
                    new
                    {
                        ConfigurationId = productInternalSetting.ConfigurationId,
                        ProductSettingId = (int)createResponse.Id
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            tx.Commit();
            return response;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{ActionName} - {State}",
                "CreateProductSettingAndLinkToConfiguration",
                "Unhandled exception — transaction rolled back.");

            response.ErrorMessage = "There was a problem updating the product setting";
            return response;
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Opens the connection if not already open.
    /// Required before calling <see cref="IDbConnection.BeginTransaction"/>.
    /// </summary>
    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }

    #endregion
}