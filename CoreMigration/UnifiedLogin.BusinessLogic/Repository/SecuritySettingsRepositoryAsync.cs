using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Security Settings Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class SecuritySettingsRepositoryAsync : ISecuritySettingsRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<SecuritySettingsRepositoryAsync> _logger;

    public SecuritySettingsRepositoryAsync(IDbConnection db, ILogger<SecuritySettingsRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<Setting>> GetSecuritySettingsAsync(
        long booksCustomerMasterId,
        int bookMasterTypeId = (int)BookMasterType.CustomerMasterId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<Setting>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetSecuritySetting,
                new { SourceId = booksCustomerMasterId, DataImportApplicationId = bookMasterTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateSecuritySettingsAsync(
        IList<Setting> settings,
        long booksCustomerMasterId,
        int bookMasterTypeId = (int)BookMasterType.CustomerMasterId,
        CancellationToken cancellationToken = default)
    {
        // Replaces: early-return guard blocks before BeginTransaction
        if (settings is null || settings.Count == 0)
            return new RepositoryResponse { Id = 0, ErrorMessage = "Update Security Settings Error: No settings provided." };

        if (booksCustomerMasterId <= 0)
            return new RepositoryResponse { Id = 0, ErrorMessage = "Update Security Settings Error: Invalid booksCustomerMasterId." };

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateSecuritySetting,
                    new
                    {
                        SourceId                = booksCustomerMasterId,
                        DataImportApplicationId = bookMasterTypeId,
                        JsonSecuritySettings    = JsonConvert.SerializeObject(settings)
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            // Replaces: null-check + ErrorMessage guard before Commit
            if (response is null || response.Id == 0 || !string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                tx.Rollback();
                return response ?? new RepositoryResponse
                {
                    Id           = 0,
                    ErrorMessage = "Update Security Settings Error: Stored procedure returned null response."
                };
            }

            tx.Commit();
            return response;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("transaction"))
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} transaction error for masterId={Id}", nameof(UpdateSecuritySettingsAsync), booksCustomerMasterId);
            return new RepositoryResponse { Id = 0, ErrorMessage = $"Update Security Settings Transaction Error: {ex.Message}." };
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} failed for masterId={Id}", nameof(UpdateSecuritySettingsAsync), booksCustomerMasterId);
            return new RepositoryResponse { Id = 0, ErrorMessage = $"Update Security Settings Unexpected Error: {ex.Message}" };
        }
    }

    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }
}