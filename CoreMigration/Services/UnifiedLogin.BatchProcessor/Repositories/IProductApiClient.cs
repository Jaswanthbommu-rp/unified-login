using UnifiedLogin.BatchProcessor.Models;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// API client interface for batch processing operations.
/// </summary>
public interface IProductApiClient
{
    /// <summary>
    /// Processes a batch record by calling the appropriate API endpoint.
    /// </summary>
    Task<BatchProcessorResponse> ProcessBatchAsync(BatchProcessorInput input, CancellationToken cancellationToken);

    /// <summary>
    /// Processes an enterprise role batch record.
    /// </summary>
    Task<BatchProcessorResponse> ProcessEnterpriseRoleBatchAsync(EnterpriseRoleBatch batch, string endpoint, CancellationToken cancellationToken);

    /// <summary>
    /// Processes a primary property batch record.
    /// </summary>
    Task<BatchProcessorResponse> ProcessPrimaryPropertyBatchAsync(PrimaryPropertyBatch batch, string endpoint, CancellationToken cancellationToken);

    /// <summary>
    /// Processes a bulk user batch record.
    /// </summary>
    Task<BatchProcessorResponse> ProcessBulkUserBatchAsync(BulkUserBatch batch, string endpoint, CancellationToken cancellationToken);

    /// <summary>
    /// Processes a company property batch record.
    /// </summary>
    Task<BatchProcessorResponse> ProcessCompanyPropertyBatchAsync(CompanyPropertyBatch batch, string endpoint, CancellationToken cancellationToken);
}
