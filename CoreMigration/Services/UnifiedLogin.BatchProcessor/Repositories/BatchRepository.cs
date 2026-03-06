using Dapper;
using Microsoft.Extensions.Logging;
using RealPage.DataAccess.Dapper;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// Repository implementation for batch database operations using Dapper.
/// Uses IDbConnectionFactory to create a short-lived SqlConnection per operation,
/// ensuring connections are returned to the ADO.NET pool promptly and preventing socket exhaustion.
/// </summary>
public class BatchRepository : IBatchRepository
{
    private readonly ILogger<BatchRepository> _logger;
    private readonly IHybridCacheService _hybridCache;
    private readonly IDbConnectionFactory _connectionFactory;

    public BatchRepository(ILogger<BatchRepository> logger,
                           IHybridCacheService hybridCache,
                           IDbConnectionFactory connectionFactory)
    {
        _logger = logger;
        _hybridCache = hybridCache;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Gets batch records to process from the database using stored procedure.
    /// </summary>
    public async Task<IList<Batch>> GetBatchToProcessAsync(int batchSize, bool shouldIncludeErrorRecords, int retryCount = 3, CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
        {
            _logger.LogWarning("Invalid batchSize provided: {BatchSize}. Using default of 100.", batchSize);
            batchSize = 30;
        }

        if (retryCount < 0)
        {
            _logger.LogWarning("Invalid retryCount provided: {RetryCount}. Using default of 3.", retryCount);
            retryCount = 3;
        }

        try
        {
            var parameters = new
            {
                IncludeErrorRecord = shouldIncludeErrorRecords,
                BatchSize = batchSize,
                RetryCount = retryCount,
                UseAPIV2 = true
            };
            await using var connection = _connectionFactory.CreateConnection();
            var results = await connection.GetManyAsync<Batch>(StoredProcNameConstants.SP_ListBatch, parameters, null, cancellationToken);
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user records. BatchSize: {BatchSize}", batchSize);
            throw;
        }
    }

    /// <summary>
    /// Gets company property batch records by status.
    /// </summary>
    public async Task<IList<CompanyPropertyBatch>> GetCompanyBatchByStatusAsync(int batchSize, BatchStatusType statusType, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new
            {
                batchSize,
                statusTypeId = (int)statusType
            };
            await using var connection = _connectionFactory.CreateConnection();
            var results = await connection.GetManyAsync<CompanyPropertyBatch>(StoredProcNameConstants.SP_ListCompanyBatchData, parameters, null, cancellationToken);
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching company batch records. BatchSize: {BatchSize}, StatusType: {StatusType}", batchSize, statusType);
            throw;
        }
    }

    /// <summary>
    /// Gets enterprise role batch records to process.
    /// </summary>
    public async Task<IList<EnterpriseRoleBatch>> GetEnterpriseRoleProductUpdateBatchToProcessAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new { batchSize };
            await using var connection = _connectionFactory.CreateConnection();
            var results = await connection.GetManyAsync<EnterpriseRoleBatch>(StoredProcNameConstants.SP_EnterpriseRoleListBatch, parameters, null, cancellationToken);
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching enterprise role batch records. BatchSize: {BatchSize}", batchSize);
            throw;
        }
    }

    /// <summary>
    /// Gets primary property batch records to process.
    /// </summary>
    public async Task<IList<PrimaryPropertyBatch>> GetPrimaryPropertyProductUpdateBatchToProcessAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new { batchSize };
            await using var connection = _connectionFactory.CreateConnection();
            var results = await connection.GetManyAsync<PrimaryPropertyBatch>(StoredProcNameConstants.SP_PrimaryPropertiesBatch, parameters, null, cancellationToken);
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching primary property batch records. BatchSize: {BatchSize}", batchSize);
            throw;
        }
    }

    /// <summary>
    /// Gets bulk user batch records to process.
    /// </summary>
    public async Task<IList<BulkUserBatch>> GetBulkUsersUpdateBatchToProcessAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new { batchSize };
            await using var connection = _connectionFactory.CreateConnection();
            var results = await connection.GetManyAsync<BulkUserBatch>(StoredProcNameConstants.SP_BulkUserBatch, parameters, null, cancellationToken);
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching bulk user batch records. BatchSize: {BatchSize}", batchSize);
            throw;
        }
    }

    /// <summary>
    /// Updates a single batch record status and error details.
    /// </summary>
    public async Task<int> UpdateBatchRecordAsync(int productBatchId, BatchStatusType batchStatusType, string? inputJson = null, string? errorDetails = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new
            {
                productBatchId,
                statusTypeId = (int)batchStatusType,
                inputJson,
                errorDetails
            };
            await using var connection = _connectionFactory.CreateConnection();
            var result = await connection.GetOneAsync<int>(StoredProcNameConstants.SP_UpdateBatch, parameters, null, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch record. BatchId: {BatchId}, Status: {Status}", productBatchId, batchStatusType);
            throw;
        }
    }

    /// <summary>
    /// Updates the status of a batch record with optional message.
    /// Simplified version for use by polling services.
    /// </summary>
    public async Task UpdateBatchStatusAsync(long batchId, BatchStatusType status, string? message = null, CancellationToken cancellationToken = default)
    {
        await UpdateBatchRecordAsync((int)batchId, status, inputJson: null, errorDetails: message, cancellationToken);
    }

    /// <summary>
    /// Updates multiple batch records with the same status.
    /// </summary>
    public async Task UpdateBatchAsync(IList<Batch> batch, BatchStatusType batchStatusType, string? inputJson = null, string? errorDetails = null, CancellationToken cancellationToken = default)
    {
        if (batch == null || !batch.Any())
        {
            return;
        }

        try
        {
            foreach (var record in batch)
            {
                await UpdateBatchRecordAsync((int)record.BatchProcessorId, batchStatusType, inputJson, errorDetails, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating multiple batch records. Status: {Status}", batchStatusType);
            throw;
        }
    }

    /// <summary>
    /// Updates an enterprise role product batch status.
    /// </summary>
    public async Task UpdateEnterpriseRoleProductBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new { productBatchId, statusTypeId };
            await using var connection = _connectionFactory.CreateConnection();
            await connection.GetOneAsync<int>(StoredProcNameConstants.SP_UpdateEnterpriseRoleProductBatch, parameters, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating enterprise role product batch. BatchId: {BatchId}, StatusTypeId: {StatusTypeId}", productBatchId, statusTypeId);
            throw;
        }
    }

    /// <summary>
    /// Updates a primary property product batch status.
    /// </summary>
    public async Task UpdatePrimaryPropertyProductBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new { productBatchId, statusTypeId };
            await using var connection = _connectionFactory.CreateConnection();
            await connection.GetOneAsync<int>(StoredProcNameConstants.SP_UpdatePrimaryPropertyProductBatch, parameters, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating primary property product batch. BatchId: {BatchId}, StatusTypeId: {StatusTypeId}", productBatchId, statusTypeId);
            throw;
        }
    }

    /// <summary>
    /// Updates a bulk user batch status.
    /// </summary>
    public async Task UpdateBulkUserBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new { productBatchId, statusTypeId };
            await using var connection = _connectionFactory.CreateConnection();
            await connection.GetOneAsync<int>(StoredProcNameConstants.SP_UpdateBulkUserBatch, parameters, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bulk user batch. BatchId: {BatchId}, StatusTypeId: {StatusTypeId}", productBatchId, statusTypeId);
            throw;
        }
    }

    /// <summary>
    /// Updates multiple company property batch records.
    /// </summary>
    public async Task UpdateCompanyPropertyBatchesAsync(IList<CompanyPropertyBatch> batch, BatchStatusType batchStatusType, CancellationToken cancellationToken = default)
    {
        if (batch == null || !batch.Any())
        {
            return;
        }

        try
        {
            foreach (var record in batch)
            {
                await UpdateCompanyPropertyBatchAsync(record.CompanyBatchJobId, (int)batchStatusType, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating multiple company property batches. Status: {Status}", batchStatusType);
            throw;
        }
    }

    /// <summary>
    /// Updates a company property batch status.
    /// </summary>
    public async Task UpdateCompanyPropertyBatchAsync(long companyBatchJobId, int statusTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new { companyBatchJobId, statusTypeId };
            await using var connection = _connectionFactory.CreateConnection();
            await connection.GetOneAsync<int>(StoredProcNameConstants.SP_UpdateCompanyPropertyBatch, parameters, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company property batch. CompanyBatchJobId: {CompanyBatchJobId}, StatusTypeId: {StatusTypeId}", companyBatchJobId, statusTypeId);
            throw;
        }
    }

    /// <summary>
    /// Gets batch configurations with HybridCache (Redis primary, in-memory fallback).
    /// </summary>
    public async Task<IList<BatchConfiguration>> GetBatchConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "batch_configs";
        try
        {
            var batchConfigs = await _hybridCache.GetOrCreateAsync(
                cacheKey,
                async ct =>
                {
                    _logger.LogInformation("Cache miss for batch configurations. Fetching from database...");
                    await using var connection = _connectionFactory.CreateConnection();
                    var results = await connection.GetManyAsync<BatchConfiguration>(StoredProcNameConstants.SP_ListBatchConfiguration, null, null, ct);
                    var configs = results.ToList();
                    _logger.LogInformation("Batch configurations loaded from database. Count: {Count}", configs.Count);
                    return configs;
                },
                absoluteExpirationMinutes: 60,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Batch configurations retrieved. Count: {Count}, Cache Mode: {CacheMode}", batchConfigs.Count, _hybridCache.CurrentCacheMode);
            return batchConfigs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching batch configurations");
            throw;
        }
    }

    /// <summary>
    /// Gets future users to process for notification.
    /// </summary>
    public async Task<List<ProcessUserLogin>> GetFutureUsersToProcessAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var parameters = new { batchSize };
        await using var connection = _connectionFactory.CreateConnection();
        var result = await connection.GetManyAsync<ProcessUserLogin>(StoredProcNameConstants.SP_ListFutureLogins, parameters, null, cancellationToken);
        return result.ToList();
    }

    /// <summary>
    /// Gets pending users to process for status update.
    /// </summary>
    public async Task<List<ProcessUserLogin>> GetPendingUsersToProcessAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var result = await connection.GetManyAsync<ProcessUserLogin>(StoredProcNameConstants.SP_ListPendingUsers, null, null, cancellationToken);
        return result.ToList();
    }

    /// <summary>
    /// Gets expired users to process for disabling.
    /// </summary>
    public async Task<List<ProcessUserLogin>> GetExpiredUsersToProcessAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var result = await connection.GetManyAsync<ProcessUserLogin>(StoredProcNameConstants.SP_ListExpiringUsers, null, null, cancellationToken);
        return result.ToList();
    }
}
