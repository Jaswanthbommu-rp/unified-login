using UnifiedLogin.BatchProcessor.Models;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// Repository interface for batch database operations.
/// </summary>
public interface IBatchRepository
{
    /// <summary>
    /// Gets batch records to process from the database.
    /// </summary>
    /// <param name="batchSize">Number of records to fetch.</param>
    /// <param name="isRetry">Whether to fetch retry batches.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of batch records.</returns>
    Task<IList<Batch>> GetBatchToProcessAsync(int batchSize, bool isRetry, CancellationToken cancellationToken);

    /// <summary>
    /// Gets enterprise role batch records to process.
    /// </summary>
    Task<IList<EnterpriseRoleBatch>> GetEnterpriseRoleBatchToProcessAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets primary property batch records to process.
    /// </summary>
    Task<IList<PrimaryPropertyBatch>> GetPrimaryPropertyBatchToProcessAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets bulk user batch records to process.
    /// </summary>
    Task<IList<BulkUserBatch>> GetBulkUserBatchToProcessAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Gets company property batch records to process.
    /// </summary>
    Task<IList<CompanyPropertyBatch>> GetCompanyPropertyBatchToProcessAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the status of a batch record.
    /// </summary>
    Task UpdateBatchStatusAsync(long batchId, BatchStatusType status, string? message, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the status of an enterprise role batch record.
    /// </summary>
    Task UpdateEnterpriseRoleBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the status of a primary property batch record.
    /// </summary>
    Task UpdatePrimaryPropertyBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the status of a bulk user batch record.
    /// </summary>
    Task UpdateBulkUserBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the status of a company property batch record.
    /// </summary>
    Task UpdateCompanyPropertyBatchStatusAsync(long batchId, BatchStatusType status, CancellationToken cancellationToken);
}
