using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// Repository implementation for batch database operations using Dapper.
/// </summary>
public class BatchRepository : IBatchRepository
{
    private readonly string _connectionString;
    private readonly ILogger<BatchRepository> _logger;

    public BatchRepository(IOptions<BatchProcessorOptions> options, ILogger<BatchRepository> logger)
    {
        _connectionString = options.Value.ConnectionString;
        _logger = logger;
    }

    public async Task<IList<Batch>> GetBatchToProcessAsync(int batchSize, bool isRetry, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT TOP (@BatchSize)
                BatchProcessorId,
                CorrelationId,
                EditorUserPersonaId,
                SubjectUserPersonaId,
                EditorUserRealPageId,
                ProductId,
                RetryCount,
                StatusTypeId,
                BatchProcessTypeId,
                InputJson,
                BatchProcessorGroupId,
                ImpersonatorUserId,
                CreatedDateTime,
                ModifiedDateTime
            FROM BatchProcessor
            WHERE StatusTypeId = @StatusType
            ORDER BY CreatedDateTime";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var statusType = isRetry ? (int)BatchStatusType.Retry : (int)BatchStatusType.Waiting;

            var results = await connection.QueryAsync<Batch>(
                new CommandDefinition(sql, new { BatchSize = batchSize, StatusType = statusType }, cancellationToken: cancellationToken));

            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching batch records. IsRetry: {IsRetry}", isRetry);
            throw;
        }
    }

    public async Task<IList<EnterpriseRoleBatch>> GetEnterpriseRoleBatchToProcessAsync(int batchSize, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT TOP (@BatchSize)
                EnterpriseRoleBatchProcessId,
                EditorUserPersonaId,
                SubjectUserPersonaId,
                EnterpriseRoleTemplateId,
                StatusTypeId,
                BatchProcessTypeId,
                CreatedDateTime,
                ModifiedDateTime
            FROM EnterpriseRoleBatchProcess
            WHERE StatusTypeId = @StatusType
            ORDER BY CreatedDateTime";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            var results = await connection.QueryAsync<EnterpriseRoleBatch>(
                new CommandDefinition(sql, new { BatchSize = batchSize, StatusType = (int)BatchStatusType.Waiting }, cancellationToken: cancellationToken));

            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching enterprise role batch records");
            throw;
        }
    }

    public async Task<IList<PrimaryPropertyBatch>> GetPrimaryPropertyBatchToProcessAsync(int batchSize, CancellationToken cancellationToken)
    {
        // Placeholder implementation - adjust SQL based on actual schema
        return await Task.FromResult(new List<PrimaryPropertyBatch>());
    }

    public async Task<IList<BulkUserBatch>> GetBulkUserBatchToProcessAsync(int batchSize, CancellationToken cancellationToken)
    {
        // Placeholder implementation - adjust SQL based on actual schema
        return await Task.FromResult(new List<BulkUserBatch>());
    }

    public async Task<IList<CompanyPropertyBatch>> GetCompanyPropertyBatchToProcessAsync(int batchSize, CancellationToken cancellationToken)
    {
        // Placeholder implementation - adjust SQL based on actual schema
        return await Task.FromResult(new List<CompanyPropertyBatch>());
    }

    public async Task UpdateBatchStatusAsync(long batchId, BatchStatusType status, string? message, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE BatchProcessor
            SET StatusTypeId = @StatusType,
                ModifiedDateTime = GETUTCDATE(),
                ErrorMessage = @Message
            WHERE BatchProcessorId = @BatchId";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                new CommandDefinition(sql, new { BatchId = batchId, StatusType = (int)status, Message = message }, cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch status. BatchId: {BatchId}, Status: {Status}", batchId, status);
            throw;
        }
    }

    public async Task UpdateEnterpriseRoleBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE EnterpriseRoleBatchProcess
            SET StatusTypeId = @StatusType,
                ModifiedDateTime = GETUTCDATE()
            WHERE EnterpriseRoleBatchProcessId = @BatchId";

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                new CommandDefinition(sql, new { BatchId = batchId, StatusType = (int)status }, cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating enterprise role batch status. BatchId: {BatchId}", batchId);
            throw;
        }
    }

    public async Task UpdatePrimaryPropertyBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken)
    {
        // Placeholder implementation
        await Task.CompletedTask;
    }

    public async Task UpdateBulkUserBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken)
    {
        // Placeholder implementation
        await Task.CompletedTask;
    }

    public async Task UpdateCompanyPropertyBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken)
    {
        // Placeholder implementation
        await Task.CompletedTask;
    }
}
