using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Property Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class PropertyRepositoryAsync : IPropertyRepositoryAsync
{
    private readonly IDbConnection _db;
    private readonly ILogger<PropertyRepositoryAsync> _logger;

    public PropertyRepositoryAsync(IDbConnection db, ILogger<PropertyRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<List<ProductProperty>> ListPropertiesByPersonaAsync(
        long userPersonaId, int productId, CancellationToken cancellationToken = default)
    {
        // Replaces: GetMany<dynamic> + foreach mapping
        var rows = await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListPropertyMapping,
                new { PersonaID = userPersonaId, ProductID = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return rows
            .Select(item => new ProductProperty { ID = item.PropertyID.ToString() })
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<UPFMPropertyInstance>> ListUPFMPropertyInstanceByPersonaAsync(
        long userPersonaId, ProductEnum productId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<UPFMPropertyInstance>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPropertyInstanceByPersonaId,
                new { PersonaID = userPersonaId, ProductID = (int)productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<int>> ListUPFMPropertyInstanceIdByPersonaAsync(
        long userPersonaId, ProductEnum productId, CancellationToken cancellationToken = default)
        => await ListUPFMPropertyInstanceIdByPersonaAsync(userPersonaId, (int)productId, cancellationToken);

    /// <inheritdoc/>
    public async Task<List<int>> ListUPFMPropertyInstanceIdByPersonaAsync(
        long userPersonaId, int productId, CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<int>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPropertyInstanceIdsByPersonaId,
                new { PersonaID = userPersonaId, ProductID = productId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<UPFMPropertyInstance>> ListUPFMPropertyInstanceIdByInstanceIdsAsync(
        List<Guid> propertyInstanceIds, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@InstanceList",
            TableValueParamHelper.ConvertToTableValuedParameter(propertyInstanceIds, "Enterprise.PropertyInstanceType"));

        var result = await _db.QueryAsync<UPFMPropertyInstance>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPropertyInstanceListById,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertRemoveAssignedPropertyToUserAsync(
        long userPersonaId, ProductEnum productId, long propertyId,
        int remove = 0, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePropertyMapping,
                new { PersonaID = userPersonaId, ProductID = (int)productId, PropertyID = propertyId, Deleted = remove },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse { Id = rows };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertRemoveAssignedPropertyInstanceToUserAsync(
        long userPersonaId, int productId, long propertyInstanceId,
        int remove = 0, CancellationToken cancellationToken = default)
    {
        var rows = await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePropertyInstanceMapping,
                new { PersonaID = userPersonaId, ProductID = productId, PropertyInstanceID = propertyInstanceId, Deleted = remove },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse { Id = rows };
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePropertyMappingReMapAsync(
        long originalPropertyId, long newPropertyId, CancellationToken cancellationToken = default)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdatePropertyMappingReMap,
                    new { OriginalPropertyID = originalPropertyId, NewPropertyID = newPropertyId },
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
            _logger.LogError(ex, "{Method} failed for originalId={O} newId={N}", nameof(UpdatePropertyMappingReMapAsync), originalPropertyId, newPropertyId);
            return new RepositoryResponse { ErrorMessage = ex.Message };
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertUPFMPropertyInstanceAsync(
        UPFMPropertyInstance propertyInstance, CancellationToken cancellationToken = default)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreatePropertyInstance,
                    new
                    {
                        Name               = propertyInstance.Name,
                        Address            = propertyInstance.Address,
                        City               = propertyInstance.City,
                        State              = propertyInstance.State,
                        PostalCode         = propertyInstance.PostalCode,
                        Country            = propertyInstance.Country,
                        County             = propertyInstance.County,
                        Latitude           = propertyInstance.Latitude,
                        Longitude          = propertyInstance.Longitude,
                        CustomerPropertyId = propertyInstance.CustomerPropertyId,
                        Domain             = propertyInstance.Domain
                    },
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
            _logger.LogError(ex, "{Method} failed for property={Name}", nameof(InsertUPFMPropertyInstanceAsync), propertyInstance.Name);
            return new RepositoryResponse { ErrorMessage = ex.Message };
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: GetManyWithTvp — TVP is added via <see cref="DynamicParameters"/>
    /// and the SP returns a single result row with SuccessCount / ErrorMessage.
    /// </remarks>
    public async Task<RepositoryResponse> BulkInsertRemovePropertyInstanceMappingsAsync(
        long userPersonaId, int productId,
        List<UPFMPropertyInstanceMapping> propertyMappings,
        CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@PersonaID", userPersonaId);
        param.Add("@ProductID", productId);
        param.Add("@PropertyMappings",
            TableValueParamHelper.ConvertToTableValuedParameter(
                propertyMappings,
                "Enterprise.UPFMPropertyInstanceMapping",
                new List<string> { "PropertyInstanceID", "IsDeleted" }));

        var result = await _db.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_BulkCreateDeleteUPFMPropertyInstanceMapping,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new RepositoryResponse
        {
            Id           = result?.SuccessCount ?? 0,
            ErrorMessage = result?.ErrorMessage ?? string.Empty
        };
    }

    /// <inheritdoc/>
    public async Task<List<PropertySetup>> GetPropertiesForCompanyAsync(
        List<Guid> propertyInstanceIds,
        string propertyName = null,
        int? propertyMasterid = null,
        int? status = null,
        RequestParameter dataFilterSort = null,
        CancellationToken cancellationToken = default)
    {
        // Replaces: manual SortBy key iteration
        string sortBy        = "Name";
        string sortDirection = "Asc";

        if (dataFilterSort?.SortBy is not null)
        {
            foreach (var key in dataFilterSort.SortBy.Keys)
            {
                sortBy        = key;
                sortDirection = dataFilterSort.SortBy[key];
                break;
            }
        }

        var param = new DynamicParameters();
        param.Add("@InstanceList",
            TableValueParamHelper.ConvertToTableValuedParameter(propertyInstanceIds, "Enterprise.PropertyInstanceType"));
        param.Add("@Name",             propertyName);
        param.Add("@PropertyMasterid", propertyMasterid);
        param.Add("@Status",           status);
        param.Add("@SortColumn",       sortBy);
        param.Add("@SortDirection",    sortDirection);
        param.Add("@RowsPerPage",      dataFilterSort?.Pages?.ResultsPerPage == 100 ? 0 : (dataFilterSort?.Pages?.ResultsPerPage ?? 0));
        param.Add("@PageNumber",       (dataFilterSort?.Pages?.ResultsPerPage == 100 || dataFilterSort?.Pages?.StartRow <= 0) ? 1 : (dataFilterSort?.Pages?.StartRow ?? 1));

        var result = await _db.QueryAsync<PropertySetup>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPropertyInstanceListByIdWithPaging,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdatePropertyAsync(
        UPFMPropertyInstance property, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePropertyInstance,
                new
                {
                    InstanceId = property.InstanceId,
                    Name       = property.Name,
                    Active     = property.IsActive,
                    Address    = property.Address,
                    City       = property.City,
                    State      = property.State,
                    PostalCode = property.PostalCode,
                    Country    = property.Country,
                    County     = property.County
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUPFMPropertyListAsync(
        List<UPFMPropertyInstance> propertyInstanceIds, CancellationToken cancellationToken = default)
    {
        var param = new DynamicParameters();
        param.Add("@InstanceList",
            TableValueParamHelper.ConvertToTableValuedParameter(
                propertyInstanceIds.Select(m => m.InstanceId).ToList(),
                "Enterprise.PropertyInstanceType"));
        param.Add("@Active", propertyInstanceIds.FirstOrDefault()?.IsActive);

        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePropertyInstances,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeleteUPFMPropertyInstanceAsync(
        Guid instanceId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_DeletePropertyInstance,
                new { instanceId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> StageUserProductPrimaryPropertiesAsync(
        string stagingData, long userPersonaId, int productId, long createdBy,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_AddPersonaProductMatchedPrimaryProperties,
                new { ProductId = productId, PersonaId = userPersonaId, ModifiedBy = createdBy, PropertyInstanceJSON = stagingData },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> DeleteStagedUserProductPrimaryPropertiesAsync(
        long userPersonaId, int productId, CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_DeletePersonaProductMatchedPrimaryProperties,
                new { ProductId = productId, PersonaId = userPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }
}