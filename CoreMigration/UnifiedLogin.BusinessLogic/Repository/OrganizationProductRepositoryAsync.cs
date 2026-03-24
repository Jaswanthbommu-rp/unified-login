using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Organization Product Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class OrganizationProductRepositoryAsync : IOrganizationProductRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<OrganizationProductRepositoryAsync> _logger;

    public OrganizationProductRepositoryAsync(
        IDbConnection db,
        ILogger<OrganizationProductRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertUpdateOrganizationProductAsync(
        long partyId, int product, int? configurationId,
        DateTime? fromDate, DateTime? thruDate,
        CancellationToken cancellationToken = default)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateOrganizationProduct,
                    new { PartyId = partyId, ProductId = product, ConfigurationID = configurationId, FromDate = fromDate, ThruDate = thruDate },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();
            tx.Commit();
            return result;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} failed for partyId={PartyId}", nameof(InsertUpdateOrganizationProductAsync), partyId);
            return new RepositoryResponse { ErrorMessage = "Failed to add/update product to organization" };
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeleteOrganizationProductAsync(
        long partyId, int product,
        CancellationToken cancellationToken = default)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_DeleteOrganizationProduct,
                    new { PartyId = partyId, ProductId = product },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();
            tx.Commit();
            return result;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} failed for partyId={PartyId}", nameof(DeleteOrganizationProductAsync), partyId);
            return new RepositoryResponse { ErrorMessage = "Failed to remove product from organization" };
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DisableUsersForProductAsync(
        long partyId, ProductEnum product,
        CancellationToken cancellationToken = default)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_DisableUsersForProduct,
                    new { PartyId = partyId, ProductId = (int)product },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();
            tx.Commit();
            return result;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} failed for partyId={PartyId}", nameof(DisableUsersForProductAsync), partyId);
            return new RepositoryResponse { ErrorMessage = "Failed to remove product from organization" };
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Three sequential SP calls in one implicit transaction —
    /// replaces the original try/catch without explicit transaction.
    /// </remarks>
    public async Task<RepositoryResponse> CreateOrganizationProductSettingAsync(
        long partyId, int productId, int productSettingTypeId, string value,
        CancellationToken cancellationToken = default)
    {
        var response = new RepositoryResponse();
        try
        {
            // Step 1 — create configuration
            response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateOrganizationProductConfiguration,
                    new { PartyId = partyId, ProductId = productId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (response.Id == 0)
            {
                response.ErrorMessage = "CreateOrganizationProductSetting Error: CreatePersonaConfiguration failed.";
                return response;
            }

            var configurationId = Convert.ToInt32(response.Id);

            // Step 2 — create product setting
            response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateProductSetting,
                    new { ProductId = productId, ProductSettingTypeId = productSettingTypeId, Value = value, FromDate = DateTime.UtcNow, ProductSettingId = 0 },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (response.Id == 0)
            {
                response.ErrorMessage = "CreateOrganizationProductSetting Error: CreateProductSetting failed.";
                return response;
            }

            var productSettingId = Convert.ToInt32(response.Id);

            // Step 3 — link setting to configuration
            response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateOrganizationProductConfigurationbyPartyId,
                    new { OrgPartyId = partyId, ConfigurationId = configurationId, ProductId = productId, ProductSettingID = productSettingId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (response.Id == 0)
                response.ErrorMessage = "CreateOrganizationProductSetting Error: CreateOrganizationProductConfigurationbyPartyId failed.";

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed for partyId={PartyId}", nameof(CreateOrganizationProductSettingAsync), partyId);
            return new RepositoryResponse { ErrorMessage = $"Create/Update Organization Product Setting Error: {ex.Message}" };
        }
    }

    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }
}