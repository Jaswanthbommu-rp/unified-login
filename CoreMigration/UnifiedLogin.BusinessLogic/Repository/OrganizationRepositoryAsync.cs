using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Maintenance;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Organization Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class OrganizationRepositoryAsync : IOrganizationRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ICacheService _cache;
    private readonly ILogger<OrganizationRepositoryAsync> _logger;

    // Cache TTLs (minutes) — replaces RPObjectCache inline values
    private static readonly CacheEntryOptions OrgTypeCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions OrgDomainCacheOptions = new() { ExpirationTimeInMinutes = 1 };
    private static readonly CacheEntryOptions ProductCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions AdminUserCacheOptions = new() { ExpirationTimeInMinutes = 3 };
    private static readonly CacheEntryOptions OrgSettingCacheOptions = new() { ExpirationTimeInMinutes = 3 };

    #endregion

    #region Constructor

    /// <summary>
    /// Primary DI constructor — all dependencies injected, no <c>new</c>.
    /// </summary>
    public OrganizationRepositoryAsync(
        IDbConnection db,
        ICacheService cache,
        ILogger<OrganizationRepositoryAsync> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Organization CRUD

    /// <inheritdoc/>
    public async Task<RepositoryResponse> InsertOrganizationAsync(Organization organization)
    {
        var orgTypes = await ListOrganizationTypeAsync();
        var defaultTypeId = orgTypes
            .Find(t => t.Name?.Equals("Multifamily", StringComparison.OrdinalIgnoreCase) == true)
            ?.OrganizationTypeId ?? 0;

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var param = new
            {
                OrganizationName = organization.Name,
                BlueBookId = organization.BooksCustomerMasterId,
                BlackBookId = organization.BooksMasterId,
                OrganizationTypeId = organization?.organizationType?.OrganizationTypeId ?? defaultTypeId,
                OrganizationDomainId = organization.OrganizationDomain.OrganizationDomainId,
                OrganizationStatus = organization.IsActive
            };

            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_SetupOrganization,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure));

            tx.Commit();
            return result ?? new RepositoryResponse();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "InsertOrganization failed");
            return new RepositoryResponse { ErrorMessage = "Failed to create organization" };
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateOrganizationAsync(Organization organization)
    {
        // Sequential: all repositories share the same IDbConnection — concurrent queries cause a
        // "connection does not support MultipleActiveResultSets" error. Cache-backed lookups make
        // the sequential cost negligible after the first cold-cache miss.
        var orgTypes = await ListOrganizationTypeAsync();
        var orgDomains = await ListOrganizationDomainAsync();

        var defaultTypeId = orgTypes.Find(t => t.Name?.Equals("Multifamily", StringComparison.OrdinalIgnoreCase) == true)?.OrganizationTypeId ?? 0;
        var defaultDomainId = orgDomains.Find(d => d.Name?.Equals("Primary", StringComparison.OrdinalIgnoreCase) == true)?.OrganizationDomainId ?? 0;

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var param = new
            {
                organizationId = organization?.RealPageId,
                organizationName = organization?.Name,
                OrganizationTypeId = organization?.organizationType?.OrganizationTypeId ?? defaultTypeId,
                OrganizationDomainId = organization?.OrganizationDomain?.OrganizationDomainId ?? defaultDomainId,
                OrganizationStatus = organization?.IsActive
            };

            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateOrganization,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure));

            tx.Commit();
            return result ?? new RepositoryResponse();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "UpdateOrganization failed");
            return new RepositoryResponse { ErrorMessage = "There was a problem updating the Organization" };
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateOrganizationThirdPartyIDPAsync(Organization organization)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateOrganizationThirdPartyIDP,
                    new { organizationPartyId = organization?.PartyId, ThirdPartyIDP = organization?.ThirdPartyIDP },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure));

            tx.Commit();
            return result ?? new RepositoryResponse();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "UpdateOrganizationThirdPartyIDP failed");
            return new RepositoryResponse { ErrorMessage = "There was a problem updating the Organization" };
        }
    }

    /// <inheritdoc/>
    public async Task<Organization> GetOrganizationAsync(Guid? realPageId = null, long? organizationPartyId = null)
    {
        var param = new
        {
            RealPageId = (realPageId == Guid.Empty) ? null : realPageId,
            PartyId = organizationPartyId
        };

        var organization = await _db.QuerySingleOrDefaultAsync<Organization>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetOrganization,
                param,
                commandType: CommandType.StoredProcedure));

        if (organization is not null)
            await EnrichOrganizationAsync(organization);

        return organization;
    }

    /// <inheritdoc/>
    public async Task<IList<Organization>> GetOrganizationListAsync()
    {
        var orgs = (await _db.QueryAsync<Organization>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetOrganization,
                commandType: CommandType.StoredProcedure))).ToList();

        return await EnrichOrganizationListAsync(orgs);
    }

    /// <inheritdoc/>
    public async Task<IList<Organization>> GetOrganizationListByBooksCustomerMasterIdAsync(long blueBookId)
    {
        var orgs = (await _db.QueryAsync<Organization>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetOrganization,
                new { BlueBookId = blueBookId },
                commandType: CommandType.StoredProcedure))).ToList();

        return await EnrichOrganizationListAsync(orgs);
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateOrganizationBooksCompanyMasterIdAsync(
        Organization oldOrganization,
        Organization newOrganization)
    {
        var param = new
        {
            ApplicationId = BookMasterType.CustomerMasterId,
            PartyId = oldOrganization.PartyId,
            Original_SourceId = oldOrganization.BooksCustomerMasterId,
            SourceId = newOrganization.BooksCustomerMasterId
        };

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_DataImportMappingUpdate,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure));

            tx.Commit();
            return result ?? new RepositoryResponse { Id = 0 };
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "UpdateOrganizationBooksCompanyMasterId failed");
            return new RepositoryResponse { Id = 0, ErrorMessage = ex.Message };
        }
    }

    #endregion

    #region Organization Types and Domains

    /// <inheritdoc/>
    public async Task<List<OrganizationType>> ListOrganizationTypeAsync()
    {
        return await _cache.GetOrSetAsync(
            "getListOrganizationType",
            async _ =>
            {
                var result = await _db.QueryAsync<OrganizationType>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListOrganizationType,
                        commandType: CommandType.StoredProcedure));
                return result.ToList();
            },
            OrgTypeCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<List<OrganizationDomain>> ListOrganizationDomainAsync()
    {
        return await _cache.GetOrSetAsync(
            "getListOrganizationDomain",
            async _ =>
            {
                var result = await _db.QueryAsync<OrganizationDomain>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListOrganizationDomain,
                        commandType: CommandType.StoredProcedure));
                return result.ToList();
            },
            OrgDomainCacheOptions) ?? [];
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateOrganizationDomainAsync(OrganizationDomain organizationDomain)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateOrganizationDomain,
                new { DomainName = organizationDomain.Name },
                commandType: CommandType.StoredProcedure))
            ?? new RepositoryResponse();
    }

    #endregion

    #region Identity / IDP

    /// <inheritdoc/>
    public async Task<Guid> GetOrganizationAdminUserRealPageIdAsync(Guid organizationRealPageId)
    {
        var cacheKey = $"getOrganizationAdminUserRealPageId_{organizationRealPageId}";

        // Replaces: new RPObjectCache().GetFromCache(...)
        var idString = await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                var rows = (await _db.QueryAsync<dynamic>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListOrganizations,
                        new { RealPageId = organizationRealPageId },
                        commandType: CommandType.StoredProcedure))).ToList();

                // Replaces: foreach loop that overwrites on each iteration — keeps last item
                return rows.Count > 0
                    ? rows[^1].PersonRealPageId?.ToString() ?? string.Empty
                    : string.Empty;
            },
            AdminUserCacheOptions);

        return !string.IsNullOrEmpty(idString) ? new Guid(idString) : Guid.Empty;
    }

    /// <inheritdoc/>
    public async Task<IList<IdentityProviderType>> GetOrganizationIdentityProviderTypeAsync(Guid realPageId)
    {
        var result = await _db.QueryAsync<IdentityProviderType>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetOrganizationIdentityProviderType,
                new { RealPageId = realPageId },
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<List<IDPNames>> GetCompanyIDPListAsync(int organizationPartyId)
    {
        var result = await _db.QueryAsync<IDPNames>(
            new CommandDefinition(
                StoredProcNameConstants.SP_OrganizationIDPList,
                new { OrganizationPartyId = organizationPartyId },
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    #endregion

    #region Settings

    /// <inheritdoc/>
    public async Task<string> GetOrganizationSettingValueAsync(string settingName, long partyId)
    {
        try
        {
            var param = new DynamicParameters();
            param.Add("@PartyId", partyId, dbType: DbType.Int32, direction: ParameterDirection.Input);
            param.Add("@SettingName", settingName, dbType: DbType.String, direction: ParameterDirection.Input);
            param.Add("@SettingValue", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);

            await _db.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetOrganizationSettingValue,
                    param,
                    commandType: CommandType.StoredProcedure));

            return param.Get<string>("@SettingValue") ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetOrganizationSettingValueByPersonaIdAsync(string settingName, long personaId)
    {
        var cacheKey = $"getOrganizationSettingValueByPersonaId_{personaId}_{settingName}";

        // Replaces: new RPObjectCache().GetFromCache(...)
        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                var param = new DynamicParameters();
                param.Add("@PersonaId", personaId, dbType: DbType.Int32, direction: ParameterDirection.Input);
                param.Add("@SettingName", settingName, dbType: DbType.String, direction: ParameterDirection.Input);
                param.Add("@SettingValue", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);

                await _db.ExecuteAsync(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_GetOrganizationSettingValueByPersonaId,
                        param,
                        commandType: CommandType.StoredProcedure));

                return param.Get<string>("@SettingValue") ?? string.Empty;
            },
            OrgSettingCacheOptions) ?? string.Empty;
    }

    #endregion

    #region Super User / Products

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateInitialOrgSuperUserAsync(
        long organizationId,
        string firstName,
        string middleName,
        string lastName,
        string title,
        string suffix,
        string email,
        bool defaultIDP,
        int? idpTypeId,
        IList<int> productIdList)
    {
        var param = new DynamicParameters();
        param.Add("OrganizationId", organizationId);
        param.Add("FirstName", firstName);
        param.Add("MiddleName", middleName);
        param.Add("LastName", lastName);
        param.Add("Title", title);
        param.Add("Suffix", suffix);
        param.Add("Email", email);
        param.Add("DefaultIDP", defaultIDP);
        param.Add("IDPTypeId", idpTypeId);
        // Replaces: TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
        param.Add("AssignedProductId",
            TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype"));

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            await _db.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_SetupSuperUser,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure));

            tx.Commit();
            return new RepositoryResponse { Id = 1 };
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "CreateInitialOrgSuperUser failed");
            return new RepositoryResponse { ErrorMessage = "Failed to create the super user" };
        }
    }

    /// <inheritdoc/>
    public async Task<IList<ProductUI>> GetProductsByCompanyAsync(Guid organizationRealPageId)
    {
        var cacheKey = $"getProductsByCompany_{organizationRealPageId}";

        // Replaces: new RPObjectCache().GetFromCache(...)
        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ =>
            {
                var result = await _db.QueryAsync<ProductUI>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListProductsByOrganization,
                        new { OrganizationRealPageId = organizationRealPageId },
                        commandType: CommandType.StoredProcedure));
                return (IList<ProductUI>)result.ToList();
            },
            ProductCacheOptions) ?? [];
    }

    #endregion

    #region Company List / Setup

    /// <inheritdoc/>
    public async Task<List<UnifiedLoginCompany>> GetUnifiedLoginCompanyListAsync()
    {
        var rows = (await _db.QueryAsync<dynamic>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListOrganizations,
                commandType: CommandType.StoredProcedure))).ToList();

        return rows
            .Select(item => new UnifiedLoginCompany
            {
                CompanyId = long.Parse(item.BooksMasterId.ToString()),
                BooksCustomerMasterId = long.Parse(string.IsNullOrEmpty(item.BooksCustomerMasterId?.ToString()) ? "0" : item.BooksCustomerMasterId.ToString()),
                CompanyName = item.Name,
                IsActive = true,
                PartyId = item.PartyId,
                CompanyRealPageId = item.OrganizationRealPageId?.ToString(),
                UserRealPageId = item.PersonRealPageId?.ToString(),
                UserLoginAs = item.LoginName,
                Domain = item.Domain
            })
            .OrderBy(c => c.CompanyName)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<List<CompanySetup>> GetCompanyListAsync(
        string organizationName,
        int domain,
        int? blueId,
        int organizationId,
        RequestParameter dataFilterSort = null)
    {
        string sortBy = "OrganizationName";
        string sortDirection = "Asc";
        string filterByProduct = null, filterByDomain = null, filterByType = null, filterByStatus = null;

        if (dataFilterSort?.FilterBy is not null)
        {
            foreach (var key in dataFilterSort.FilterBy.Keys)
            {
                switch (key.ToLowerInvariant())
                {
                    case "product": filterByProduct = dataFilterSort.FilterBy[key]; break;
                    case "domain": filterByDomain = dataFilterSort.FilterBy[key]; break;
                    case "type": filterByType = dataFilterSort.FilterBy[key]; break;
                    case "status": filterByStatus = dataFilterSort.FilterBy[key]; break;
                }
            }
        }

        if (dataFilterSort?.SortBy is not null)
        {
            foreach (var key in dataFilterSort.SortBy.Keys)
            {
                sortBy = key;
                sortDirection = dataFilterSort.SortBy[key];
            }
        }

        int rowsPerPage = dataFilterSort?.Pages?.ResultsPerPage == 100 ? 0 : (dataFilterSort?.Pages?.ResultsPerPage ?? 0);
        int pageNumber = (dataFilterSort?.Pages?.ResultsPerPage == 100 || dataFilterSort?.Pages?.StartRow <= 0) ? 1 : (dataFilterSort?.Pages?.StartRow ?? 1);

        var param = new
        {
            OrganizationName = organizationName,
            OrganizationId = organizationId,
            Domain = domain,
            BooksCustomerMasterId = blueId,
            FilterByProduct = filterByProduct,
            FilterByDomain = filterByDomain,
            FilterByType = filterByType,
            FilterByStatus = filterByStatus,
            SortColumn = sortBy,
            SortDirection = sortDirection,
            RowsPerPage = rowsPerPage,
            PageNumber = pageNumber
        };

        var result = await _db.QueryAsync<CompanySetup>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListCompanySetup,
                param,
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> AddCompanyToJobAsync(
        string companyInstanceSourceId,
        long createdBy,
        long createUserPersonaId,
        int organizationIsActive)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var param = new
            {
                CompanyInstanceSourceId = companyInstanceSourceId,
                StatusTypeId = 5,
                CreatedBy = createdBy,
                CreateUserPersonaId = createUserPersonaId,
                IsActive = organizationIsActive,
                BatchProcessTypeId = BatchProcessType.CompanyPropertyUpdate
            };

            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_InsertBatchCompanyJob,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure));

            tx.Commit();
            return result ?? new RepositoryResponse();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "AddCompanyToJob failed");
            return new RepositoryResponse { ErrorMessage = "There was a problem adding the company to the job" };
        }
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateCompanyStatus(
        long companyBatchJobId,
        int statusTypeId,
        string errorMessage)
    {
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateCompanyStatus,
                    new { CompanyBatchJobId = companyBatchJobId, StatusTypeId = statusTypeId, errorMessage },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure));

            tx.Commit();
            return result ?? new RepositoryResponse();
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "UpdateCompanyStatus failed");
            return new RepositoryResponse { ErrorMessage = "There was a problem updating the company status" };
        }
    }

    #endregion

    #region Organization Removal Queue

    /// <inheritdoc/>
    public async Task<List<OrganizationRemovalQueue>> GetOrganizationToDeleteAsync(
        int batchSize,
        int retryCount,
        bool includeErrorRecord)
    {
        var result = await _db.QueryAsync<OrganizationRemovalQueue>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListOrganizationToDelete,
                new { BatchSize = batchSize, RetryCount = retryCount },
                commandType: CommandType.StoredProcedure));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<long> DeleteOrganizationAsync(
        int organizationRemovalQueueId,
        long partyId,
        Guid organizationRealPageId)
    {
        return await _db.QuerySingleOrDefaultAsync<long>(
            new CommandDefinition(
                StoredProcNameConstants.SP_DeleteOrganization,
                new
                {
                    OrganizationRemovalQueueId = organizationRemovalQueueId,
                    OrganizationPartyId = partyId,
                    OrganizationRealPageId = organizationRealPageId
                },
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<int> UpdateOrganizationRemovalQueueStatusAsync(
        int organizationRemovalQueueId,
        string organizationRemovalQueueStatus)
    {
        return await _db.QuerySingleOrDefaultAsync<int>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateOrganizationRemovalQueueStatus,
                new
                {
                    OrganizationRemovalQueueId = organizationRemovalQueueId,
                    OrganizationRemovalQueueStatus = organizationRemovalQueueStatus
                },
                commandType: CommandType.StoredProcedure));
    }

    /// <inheritdoc/>
    public async Task<OrganizationRemovalQueue> InsertOrganizationRemovalQueueAsync(
        OrganizationRemovalQueue orgRemovalQueue)
    {
        return await _db.QuerySingleOrDefaultAsync<OrganizationRemovalQueue>(
            new CommandDefinition(
                StoredProcNameConstants.SP_InsertOrganizationRemovalQueue,
                new
                {
                    OrganizationPartyId = orgRemovalQueue.OrganizationPartyId,
                    OrganizationRealPageId = orgRemovalQueue.OrganizationRealPageId,
                    OrganizationCustomerMasterId = orgRemovalQueue.OrganizationCustomerMasterId,
                    OrganizationDomain = orgRemovalQueue.OrganizationDomain,
                    OrganizationName = orgRemovalQueue.OrganizationName,
                    OrganizationRemoveUDMData = orgRemovalQueue.OrganizationRemoveUDMData,
                    OrganizationRemovalQueueStatusId = orgRemovalQueue.OrganizationRemovalQueueStatusId,
                    OrganizationRemovalRetryCount = orgRemovalQueue.OrganizationRemovalRetryCount,
                    RequestedBy = orgRemovalQueue.RequestedBy
                },
                commandType: CommandType.StoredProcedure));
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Opens the connection if it is not already open.
    /// Required before calling <see cref="IDbConnection.BeginTransaction"/>.
    /// </summary>
    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }

    /// <summary>
    /// Enriches a single organization with its type and domain metadata.
    /// Fetches both reference lists concurrently.
    /// </summary>
    private async Task EnrichOrganizationAsync(Organization org)
    {
        // Sequential: shared IDbConnection does not support concurrent active result sets.
        var orgTypes = await ListOrganizationTypeAsync();
        var orgDomains = await ListOrganizationDomainAsync();

        ApplyTypeAndDomain(org, orgTypes, orgDomains);
    }

    /// <summary>
    /// Enriches a list of organizations with type and domain metadata.
    /// Fetches both reference lists once concurrently — replaces the per-item
    /// calls to <c>ListOrganizationType()</c> and <c>ListOrganizationDomain()</c>
    /// that existed inside the original <c>ForEach</c> loops.
    /// </summary>
    private async Task<IList<Organization>> EnrichOrganizationListAsync(List<Organization> orgs)
    {
        if (orgs.Count == 0) return orgs;

        // Sequential: shared IDbConnection does not support concurrent active result sets.
        var orgTypes = await ListOrganizationTypeAsync();
        var orgDomains = await ListOrganizationDomainAsync();

        orgs.ForEach(o => ApplyTypeAndDomain(o, orgTypes, orgDomains));
        return orgs;
    }

    /// <summary>
    /// Applies pre-fetched type and domain data to a single organization.
    /// </summary>
    private static void ApplyTypeAndDomain(
        Organization org,
        IList<OrganizationType> orgTypes,
        IList<OrganizationDomain> orgDomains)
    {
        var orgType = orgTypes.FirstOrDefault(t => t.OrganizationTypeId == org.OrganizationTypeId);
        org.organizationType = orgType is not null
            ? new OrganizationType { Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate }
            : new OrganizationType();

        var orgDomain = orgDomains.FirstOrDefault(d => d.OrganizationDomainId == org.OrganizationDomainId);
        org.OrganizationDomain = orgDomain is not null
            ? new OrganizationDomain { OrganizationDomainId = orgDomain.OrganizationDomainId, Name = orgDomain.Name, CreateDate = orgDomain.CreateDate }
            : new OrganizationDomain();
    }

    #endregion
}