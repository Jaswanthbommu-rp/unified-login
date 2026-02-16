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
    /// <param name="shouldIncludeErrorRecords">Whether to include error records.</param>
    /// <param name="retryCount">Number of retry attempts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of batch records.</returns>
    Task<IList<Batch>> GetBatchToProcessAsync(int batchSize, bool shouldIncludeErrorRecords, int retryCount = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets company property batch records by status.
    /// </summary>
    Task<IList<CompanyPropertyBatch>> GetCompanyBatchByStatusAsync(int batchSize, BatchStatusType statusType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets enterprise role batch records to process.
    /// </summary>
    Task<IList<EnterpriseRoleBatch>> GetEnterpriseRoleProductUpdateBatchToProcessAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets primary property batch records to process.
    /// </summary>
    Task<IList<PrimaryPropertyBatch>> GetPrimaryPropertyProductUpdateBatchToProcessAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets bulk user batch records to process.
    /// </summary>
    Task<IList<BulkUserBatch>> GetBulkUsersUpdateBatchToProcessAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a single batch record status and error details.
    /// </summary>
    Task<int> UpdateBatchRecordAsync(int productBatchId, BatchStatusType batchStatusType, string? inputJson = null, string? errorDetails = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a batch record with optional message.
    /// </summary>
    Task UpdateBatchStatusAsync(long batchId, BatchStatusType status, string? message = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple batch records with the same status.
    /// </summary>
    Task UpdateBatchAsync(IList<Batch> batch, BatchStatusType batchStatusType, string? inputJson = null, string? errorDetails = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an enterprise role product batch status.
    /// </summary>
    Task UpdateEnterpriseRoleProductBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a primary property product batch status.
    /// </summary>
    Task UpdatePrimaryPropertyProductBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a bulk user batch status.
    /// </summary>
    Task UpdateBulkUserBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default); 

    /// <summary>
    /// Updates multiple company property batch records.
    /// </summary>
    Task UpdateCompanyPropertyBatchesAsync(IList<CompanyPropertyBatch> batch, BatchStatusType batchStatusType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a company property batch status.
    /// </summary>
    Task UpdateCompanyPropertyBatchAsync(long companyBatchJobId, int statusTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets batch configurations with memory caching (60 minutes).
    /// </summary>
    Task<IList<BatchConfiguration>> GetBatchConfigurationsAsync(CancellationToken cancellationToken = default);

    ///// <summary>
    ///// Gets product internal settings with memory caching (15 minutes).
    ///// </summary>
    //Task<IList<ProductInternalSetting>> GetProductInternalSettingsAsync(
    //    int productId,
    //    CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets future users to process for notification.
    /// Calls stored procedure: Ident.ListFutureLogins
    /// </summary>
    /// <param name="batchSize">Number of users to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of users to process</returns>
    Task<List<ProcessUserLogin>> GetFutureUsersToProcessAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending users to process for status update.
    /// Calls stored procedure: Ident.ListPendingUsers
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending users</returns>
    Task<List<ProcessUserLogin>> GetPendingUsersToProcessAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired users to process for disabling.
    /// Calls stored procedure: Ident.ListExpiringUsers
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of expired users</returns>
    Task<List<ProcessUserLogin>> GetExpiredUsersToProcessAsync(CancellationToken cancellationToken = default);
}
