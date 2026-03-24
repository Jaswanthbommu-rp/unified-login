using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Custom Fields Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class CustomFieldsRepositoryAsync : ICustomFieldsRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ILogger<CustomFieldsRepositoryAsync> _logger;

    #endregion

    #region Constructor

    public CustomFieldsRepositoryAsync(
        IDbConnection db,
        ILogger<CustomFieldsRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region ICustomFieldsRepositoryAsync Implementation

    /// <inheritdoc/>
    /// <remarks>
    /// Pure transformation layer — delegates to <see cref="GetCustomFieldAsync"/>
    /// and wraps the result as a JSON-valued <see cref="Setting"/>.
    /// No additional DB call is made here.
    /// </remarks>
    public async Task<IList<Setting>> GetCustomFieldsAsync(
        long partyId,
        RequestParameter? dataFilterSort = null,
        CancellationToken cancellationToken = default)
    {
        var customFields = await GetCustomFieldAsync(partyId, dataFilterSort, cancellationToken);

        var json = customFields.Count > 0
            ? JsonConvert.SerializeObject(new { customField = customFields })
            : string.Empty;

        return
        [
            new Setting { Name = "customfields", Value = json, Right = 0 }
        ];
    }

    /// <inheritdoc/>
    public async Task<IList<CustomField>> GetCustomFieldAsync(
        long partyId,
        RequestParameter? dataFilterSort = null,
        CancellationToken cancellationToken = default)
    {
        // Replaces: two ForEach loops building JSON filter/sort strings
        var filterByJson = BuildFilterJson(dataFilterSort);
        var sortByJson   = BuildSortJson(dataFilterSort);

        var param = new
        {
            PartyId      = partyId,
            FilterBy     = filterByJson,
            SortBy       = sortByJson,
            RowsPerPage  = dataFilterSort?.Pages?.ResultsPerPage ?? 0,
            PageNumber   = (dataFilterSort?.Pages?.StartRow ?? 0) <= 0 ? 1 : dataFilterSort!.Pages.StartRow
        };

        var result = await _db.QueryAsync<CustomField>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetFieldsByPartyId,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<CustomFieldValue>> GetCustomFieldsValuesAsync(
        long organizationPartyId,
        long? userLoginPersonaId = null,
        bool? enabled = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<CustomFieldValue>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetFieldsValuesByUserLoginPersonaId,
                new { OrganizationPartyId = organizationPartyId, UserLoginPersonaId = userLoginPersonaId, Enabled = enabled },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> AddUpdateFieldValueAsync(
        string customFieldsValuesJson,
        long createdBy,
        CancellationToken cancellationToken = default)
    {
        var response = new RepositoryResponse { Id = 0 };

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_AddUpdateFieldValue,
                    new { JSON = customFieldsValuesJson, CreatedBy = createdBy },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (response.Id == 0 && !string.IsNullOrWhiteSpace(response.ErrorMessage))
                response.ErrorMessage = $"Add/Update custom fields values {customFieldsValuesJson} Error: {response.ErrorMessage}.";

            tx.Commit();
            return response;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} failed for JSON={Json}", nameof(AddUpdateFieldValueAsync), customFieldsValuesJson);
            response.Id           = 0;
            response.ErrorMessage = $"Update Custom Fields values {customFieldsValuesJson} Exception: {ex.Message}";
            return response;
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Builds the FilterBy JSON string from <see cref="RequestParameter.FilterBy"/>.
    /// Only "Enabled" keys are accepted — matches the original whitelist logic.
    /// </summary>
    private static string BuildFilterJson(RequestParameter? dataFilterSort)
    {
        if (dataFilterSort?.FilterBy is null) return string.Empty;

        var filterBy = dataFilterSort.FilterBy
            .Where(f => f.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
            .Select(f => new FilterTableType
            {
                ColumnName   = f.Key,
                SearchValue  = f.Value[..Math.Min(128, f.Value.Length)]
            })
            .ToList();

        return filterBy.Count > 0
            ? JsonConvert.SerializeObject(new { filterBy })
            : string.Empty;
    }

    /// <summary>
    /// Builds the SortBy JSON string from <see cref="RequestParameter.SortBy"/>.
    /// </summary>
    private static string BuildSortJson(RequestParameter? dataFilterSort)
    {
        if (dataFilterSort?.SortBy is null) return string.Empty;

        var sortBy = dataFilterSort.SortBy
            .Select(s => new SortTableType
            {
                ColumnName    = s.Key,
                SortDirection = s.Value[..Math.Min(128, s.Value.Length)]
            })
            .ToList();

        return sortBy.Count > 0
            ? JsonConvert.SerializeObject(new { sortBy })
            : string.Empty;
    }

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
