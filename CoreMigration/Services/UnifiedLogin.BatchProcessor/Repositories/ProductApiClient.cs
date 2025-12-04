using System.Net.Http.Json;
using System.Text.Json;
using UnifiedLogin.BatchProcessor.Models;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// HTTP client implementation for product API calls.
/// </summary>
public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<BatchProcessorResponse> ProcessBatchAsync(BatchProcessorInput input, CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = input.ProcessApiEndPoint ?? "/api/batch/process";

            _logger.LogDebug(
                "Calling batch API. Endpoint: {Endpoint}, BatchId: {BatchId}, ProductId: {ProductId}, CorrelationId: {CorrelationId}",
                endpoint, input.ProductBatchId, input.ProductId, input.CorrelationId);

            var response = await _httpClient.PostAsJsonAsync(endpoint, input, _jsonOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BatchProcessorResponse>(_jsonOptions, cancellationToken);
                return result ?? new BatchProcessorResponse { Success = true, Message = "Processed successfully" };
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "API call failed. StatusCode: {StatusCode}, Content: {Content}",
                response.StatusCode, errorContent);

            return new BatchProcessorResponse
            {
                Success = false,
                Message = $"API returned {response.StatusCode}: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling batch API for BatchId: {BatchId}", input.ProductBatchId);
            return new BatchProcessorResponse
            {
                Success = false,
                Message = $"Exception: {ex.Message}"
            };
        }
    }

    public async Task<BatchProcessorResponse> ProcessEnterpriseRoleBatchAsync(
        EnterpriseRoleBatch batch,
        string endpoint,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug(
                "Calling enterprise role API. Endpoint: {Endpoint}, BatchId: {BatchId}",
                endpoint, batch.EnterpriseRoleBatchProcessId);

            var response = await _httpClient.PostAsJsonAsync(endpoint, batch, _jsonOptions, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BatchProcessorResponse>(_jsonOptions, cancellationToken);
                return result ?? new BatchProcessorResponse { Success = true };
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return new BatchProcessorResponse
            {
                Success = false,
                Message = $"API returned {response.StatusCode}: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling enterprise role API for BatchId: {BatchId}", batch.EnterpriseRoleBatchProcessId);
            throw;
        }
    }

    public async Task<BatchProcessorResponse> ProcessPrimaryPropertyBatchAsync(
        PrimaryPropertyBatch batch,
        string endpoint,
        CancellationToken cancellationToken)
    {
        // Placeholder implementation - similar pattern to above
        await Task.Delay(100, cancellationToken); // Simulate API call
        return new BatchProcessorResponse { Success = true, Message = "Processed primary property batch" };
    }

    public async Task<BatchProcessorResponse> ProcessBulkUserBatchAsync(
        BulkUserBatch batch,
        string endpoint,
        CancellationToken cancellationToken)
    {
        // Placeholder implementation - similar pattern to above
        await Task.Delay(100, cancellationToken); // Simulate API call
        return new BatchProcessorResponse { Success = true, Message = "Processed bulk user batch" };
    }

    public async Task<BatchProcessorResponse> ProcessCompanyPropertyBatchAsync(
        CompanyPropertyBatch batch,
        string endpoint,
        CancellationToken cancellationToken)
    {
        // Placeholder implementation - similar pattern to above
        await Task.Delay(100, cancellationToken); // Simulate API call
        return new BatchProcessorResponse { Success = true, Message = "Processed company property batch" };
    }
}
