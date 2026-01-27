using OpenTelemetry;
using System.Net.Http;
using System.Text;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// API client for product batch processing operations.
/// Uses static ApiCaller helper for HTTP operations.
/// </summary>
public class ProductApiClient : IProductApiClient
{
    private readonly ILogger<ProductApiClient> _logger;

    public ProductApiClient(ILogger<ProductApiClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a batch record by calling the appropriate API endpoint.
    /// Original method: ProcessBatchRecord(BatchProcessorInput batchProcessorInput)
    /// </summary>
    /// <param name="input">The batch processor input containing the endpoint and data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    public async Task<string> ProcessBatchAsync(BatchProcessorInput input, CancellationToken cancellationToken = default)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.ProcessApiEndPoint))
        {
            throw new ArgumentException("ProcessApiEndPoint is required", nameof(input));
        }

        try
        {
            _logger.LogInformation("Processing batch record. Endpoint: {Endpoint}, BatchId: {BatchId}, ProductId: {ProductId}, CorrelationId: {CorrelationId}", input.ProcessApiEndPoint, input.ProductBatchId, input.ProductId, input.CorrelationId);
            var result = await ApiCaller.PostApi<string, BatchProcessorInput>(input, input.ProcessApiEndPoint);
            if (result != null)
            {
                _logger.LogInformation("Batch record processed successfully. BatchId: {BatchId}", input.ProductBatchId);
            }
            else
            {
                _logger.LogError("Batch record processing returned null. BatchId: {BatchId}, response: {result}", input.ProductBatchId, result);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch record. BatchId: {BatchId}", input.ProductBatchId);
            throw;
        }
    }

    /// <summary>
    /// Processes an enterprise role batch record.
    /// Original method: ProcessEnterpriseRoleBatchRecord(EnterpriseRoleBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The enterprise role batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    public async Task<string> ProcessEnterpriseRoleBatchAsync(EnterpriseRoleBatch batch, string endpoint, CancellationToken cancellationToken = default)
    {
        if (batch == null)
        {
            throw new ArgumentNullException(nameof(batch));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint is required", nameof(endpoint));
        }

        try
        {
            _logger.LogInformation("Processing enterprise role batch. Endpoint: {Endpoint}, BatchId: {BatchId}", endpoint, batch.EnterpriseRoleBatchProcessId);

            var result = await ApiCaller.PostApi<string, EnterpriseRoleBatch>(batch, endpoint);
            if (result != null)
            {
                _logger.LogInformation("Enterprise role batch processed successfully. BatchId: {BatchId}", batch.EnterpriseRoleBatchProcessId);
            }
            else
            {
                _logger.LogWarning("Enterprise role batch processing returned null. BatchId: {BatchId}", batch.EnterpriseRoleBatchProcessId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing enterprise role batch. BatchId: {BatchId}", batch.EnterpriseRoleBatchProcessId);
            throw;
        }
    }

    /// <summary>
    /// Processes a primary property batch record.
    /// Original method: ProcessPrimaryPropertyBatchRecord(PrimaryPropertyBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The primary property batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    public async Task<string> ProcessPrimaryPropertyBatchAsync(PrimaryPropertyBatch batch, string endpoint, CancellationToken cancellationToken = default)
    {
        if (batch == null)
        {
            throw new ArgumentNullException(nameof(batch));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint is required", nameof(endpoint));
        }

        try
        {
            _logger.LogInformation("Processing primary property batch. Endpoint: {Endpoint}, BatchId: {BatchId}", endpoint, batch.PrimaryPropertyBatchProcessId);

            var result = await ApiCaller.PostApi<string, PrimaryPropertyBatch>(batch, endpoint);

            if (result != null)
            {
                _logger.LogInformation("Primary property batch processed successfully. BatchId: {BatchId}", batch.PrimaryPropertyBatchProcessId);
            }
            else
            {
                _logger.LogWarning("Primary property batch processing returned null. BatchId: {BatchId}", batch.PrimaryPropertyBatchProcessId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing primary property batch. BatchId: {BatchId}", batch.PrimaryPropertyBatchProcessId);
            throw;
        }
    }

    /// <summary>
    /// Processes a bulk user batch record.
    /// Original method: ProcessBulkUserBatchRecord(BulkUserBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The bulk user batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    public async Task<string> ProcessBulkUserBatchAsync(BulkUserBatch batch, string endpoint, CancellationToken cancellationToken = default)
    {
        if (batch == null)
        {
            throw new ArgumentNullException(nameof(batch));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint is required", nameof(endpoint));
        }

        try
        {
            _logger.LogInformation("Processing bulk user batch. Endpoint: {Endpoint}, BatchId: {BatchId}", endpoint, batch.BulkUserBatchProcessId);
            var result = await ApiCaller.PostApi<string, BulkUserBatch>(batch, endpoint);
            if (result != null)
            {
                _logger.LogInformation("Bulk user batch processed successfully. BatchId: {BatchId}", batch.BulkUserBatchProcessId);
            }
            else
            {
                _logger.LogWarning("Bulk user batch processing returned null. BatchId: {BatchId}", batch.BulkUserBatchProcessId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk user batch. BatchId: {BatchId}", batch.BulkUserBatchProcessId);
            throw;
        }
    }

    /// <summary>
    /// Processes a company property batch record.
    /// Original method: ProcessCompanyBatchRecord(CompanyPropertyBatch batchProcessorInput, string processApiEndPoint)
    /// </summary>
    /// <param name="batch">The company property batch data.</param>
    /// <param name="endpoint">The API endpoint URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response string from the API.</returns>
    public async Task<string> ProcessCompanyPropertyBatchAsync(CompanyPropertyBatch batch, string endpoint, CancellationToken cancellationToken = default)
    {
        if (batch == null)
        {
            throw new ArgumentNullException(nameof(batch));
        }

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Endpoint is required", nameof(endpoint));
        }

        try
        {
            _logger.LogInformation("Processing company property batch. Endpoint: {Endpoint}, BatchId: {BatchId}", endpoint, batch.CompanyBatchJobId);

            var result = await ApiCaller.PostApi<string, CompanyPropertyBatch>(batch, endpoint);

            if (result != null)
            {
                _logger.LogInformation("Company property batch processed successfully. BatchId: {BatchId}", batch.CompanyBatchJobId);
            }
            else
            {
                _logger.LogWarning("Company property batch processing returned null. BatchId: {BatchId}", batch.CompanyBatchJobId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing company property batch. BatchId: {BatchId}", batch.CompanyBatchJobId);
            throw;
        }
    }

    /// <summary>
    /// Processes user logins by calling the API endpoint.
    /// API Endpoint: api/userlogins/processfutureuserlogins
    /// </summary>
    public async Task<string> ProcessUserLoginsAsync(List<Models.ProcessUserLogin> userLogins, string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ApiCaller.PostApi<string, List<Models.ProcessUserLogin>>(userLogins, endpoint);
            if (result != null)
            {
                _logger.LogInformation("Processed future user logins successfully. Endpoint: {Endpoint}, Count: {Count}", endpoint, userLogins?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("Processing future user logins returned null. Endpoint: {Endpoint}, Count: {Count}", endpoint, userLogins?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing future user logins. Endpoint: {Endpoint}, Count: {Count}", endpoint, userLogins?.Count ?? 0);
            throw;
        }
    }

    /// <summary>
    /// Disables expired users by calling the API endpoint.
    /// API Endpoint: api/disableexpiredusers
    /// </summary>
    public async Task<string> DisableExpiredUsersAsync(List<Models.ProcessUserLogin> userLogins, string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ApiCaller.PostApi<string, List<Models.ProcessUserLogin>>(userLogins, endpoint);//$"api/disableexpiredusers"
            if (result != null)
            {
                _logger.LogInformation("Disabled expired users successfully. Endpoint: {Endpoint}, Count: {Count}", endpoint, userLogins?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("Disabling expired users returned null. Endpoint: {Endpoint}, Count: {Count}", endpoint, userLogins?.Count ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling expired users. Endpoint: {Endpoint}, Count: {Count}", endpoint, userLogins?.Count ?? 0);
            throw;
        }
    }
}
