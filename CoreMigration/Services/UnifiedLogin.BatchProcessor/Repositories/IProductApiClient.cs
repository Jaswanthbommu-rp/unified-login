using UnifiedLogin.BatchProcessor.Models;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// API client interface for batch processing operations.
/// Converted from .NET Framework 4.8 IProductApiCaller to .NET Core 10.
/// </summary>
public interface IProductApiClient
{
    /// <summary>
    /// Processes a batch record by calling the appropriate API endpoint.
    /// Original method: ProcessBatchRecord(BatchProcessorInput batchProcessorInput)
    /// </summary>
    /// <param name="input">The batch processor input containing the endpoint and data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    Task<string> ProcessBatchAsync(BatchProcessorInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an enterprise role batch record.
    /// Original method: ProcessEnterpriseRoleBatchRecord(EnterpriseRoleBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The enterprise role batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    Task<string> ProcessEnterpriseRoleBatchAsync(EnterpriseRoleBatch batch, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a primary property batch record.
    /// Original method: ProcessPrimaryPropertyBatchRecord(PrimaryPropertyBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The primary property batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    Task<string> ProcessPrimaryPropertyBatchAsync(PrimaryPropertyBatch batch, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a bulk user batch record.
    /// Original method: ProcessBulkUserBatchRecord(BulkUserBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The bulk user batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    Task<string> ProcessBulkUserBatchAsync(BulkUserBatch batch, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a company property batch record.
    /// Original method: ProcessCompanyBatchRecord(CompanyPropertyBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The company property batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    Task<string> ProcessCompanyPropertyBatchAsync(CompanyPropertyBatch batch, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes user logins by calling the API endpoint.
    /// API Endpoint: api/userlogins/processfutureuserlogins
    /// </summary>
    /// <param name="userLogins">List of users to process</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response string</returns>
    Task<string> ProcessUserLoginsAsync(List<ProcessUserLogin> userLogins, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables expired users by calling the API endpoint.
    /// API Endpoint: api/disableexpiredusers
    /// </summary>
    /// <param name="userLogins">List of expired users to disable</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response string</returns>
    Task<string> DisableExpiredUsersAsync(List<ProcessUserLogin> userLogins, string endpoint, CancellationToken cancellationToken = default);
}
