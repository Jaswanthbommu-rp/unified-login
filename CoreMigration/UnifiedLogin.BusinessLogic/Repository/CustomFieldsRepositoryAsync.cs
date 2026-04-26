using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
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
/// <para>
/// Uses <see cref="IDbConnectionFactory"/> to open a short-lived <see cref="SqlConnection"/>
/// per call so connections are returned to the ADO.NET pool immediately — no long-lived
/// connection is held on the instance.
/// </para>
/// <para>
/// Replaces the previous implementation that injected a single <see cref="IDbConnection"/>
/// (held for the scope lifetime) and required the <c>OpenIfClosed()</c> hack before
/// calling <c>BeginTransaction()</c>. The transactional write now uses
/// <c>BeginTransactionAsync</c> on a freshly opened <see cref="SqlConnection"/>.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class CustomFieldsRepositoryAsync : ICustomFieldsRepositoryAsync
{
    private readonly IDbConnectionFactory                    _dbFactory;
    private readonly ILogger<CustomFieldsRepositoryAsync>   _logger;

    public CustomFieldsRepositoryAsync(
        IDbConnectionFactory                  dbFactory,
        ILogger<CustomFieldsRepositoryAsync>  logger)
    {
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logger    = logger    ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IList<Setting>> GetCustomFieldsAsync(
        long              partyId,
        RequestParameter? dataFilterSort    = null,
        CancellationToken cancellationToken = default)
    {
        var customFields = await GetCustomFieldAsync(partyId, dataFilterSort, cancellationToken)
            .ConfigureAwait(false);

        var json = customFields.Count > 0
            ? JsonConvert.SerializeObject(new { customField = customFields })
            : string.Empty;

        return [new Setting { Name = "customfields", Value = json, Right = 0 }];
    }

    /// <inheritdoc/>
    public async Task<IList<CustomField>> GetCustomFieldAsync(
        long              partyId,
        RequestParameter? dataFilterSort    = null,
        CancellationToken cancellationToken = default)
    {
        var param = new
        {
            PartyId     = partyId,
            FilterBy    = BuildFilterJson(dataFilterSort),
            SortBy      = BuildSortJson(dataFilterSort),
            RowsPerPage = dataFilterSort?.Pages?.ResultsPerPage ?? 0,
            PageNumber  = (dataFilterSort?.Pages?.StartRow ?? 0) <= 0 ? 1 : dataFilterSort!.Pages.StartRow
        };

        await using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var result = await connection.QueryAsync<CustomField>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetFieldsByPartyId,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<CustomFieldValue>> GetCustomFieldsValuesAsync(
        long              organizationPartyId,
        long?             userLoginPersonaId = null,
        bool?             enabled            = null,
        CancellationToken cancellationToken  = default)
    {
        await using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var result = await connection.QueryAsync<CustomFieldValue>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetFieldsValuesByUserLoginPersonaId,
                new { OrganizationPartyId = organizationPartyId, UserLoginPersonaId = userLoginPersonaId, Enabled = enabled },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            .ConfigureAwait(false);

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> AddUpdateFieldValueAsync(
        string            customFieldsValuesJson,
        long              createdBy,
        CancellationToken cancellationToken = default)
    {
        var response = new RepositoryResponse { Id = 0 };

        await using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            response = await connection.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_AddUpdateFieldValue,
                    new { JSON = customFieldsValuesJson, CreatedBy = createdBy },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                .ConfigureAwait(false)
                ?? new RepositoryResponse();

            if (response.Id == 0 && !string.IsNullOrWhiteSpace(response.ErrorMessage))
                response.ErrorMessage = $"Add/Update custom fields values {customFieldsValuesJson} Error: {response.ErrorMessage}.";

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return response;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogError(ex, "{Method} failed for JSON={Json}", nameof(AddUpdateFieldValueAsync), customFieldsValuesJson);
            response.Id           = 0;
            response.ErrorMessage = $"Update Custom Fields values {customFieldsValuesJson} Exception: {ex.Message}";
            return response;
        }
    }

    #region Private Helpers

    private static string BuildFilterJson(RequestParameter? dataFilterSort)
    {
        if (dataFilterSort?.FilterBy is null) return string.Empty;

        var filterBy = dataFilterSort.FilterBy
            .Where(f => f.Key.Equals("Enabled", StringComparison.OrdinalIgnoreCase))
            .Select(f => new FilterTableType
            {
                ColumnName  = f.Key,
                SearchValue = f.Value[..Math.Min(128, f.Value.Length)]
            })
            .ToList();

        return filterBy.Count > 0 ? JsonConvert.SerializeObject(new { filterBy }) : string.Empty;
    }

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

        return sortBy.Count > 0 ? JsonConvert.SerializeObject(new { sortBy }) : string.Empty;
    }

    #endregion
}
