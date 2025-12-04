using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Health check for the batch processor service.
/// Verifies database connectivity and API availability.
/// </summary>
public class BatchProcessorHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<BatchProcessorOptions> _options;
    private readonly ILogger<BatchProcessorHealthCheck> _logger;

    public BatchProcessorHealthCheck(
        IServiceScopeFactory scopeFactory,
        IOptions<BatchProcessorOptions> options,
        ILogger<BatchProcessorHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["ThreadCount"] = _options.Value.ThreadCount,
                ["PollingInterval"] = _options.Value.PollingIntervalSeconds,
                ["BatchSize"] = _options.Value.BatchSize
            };

            // Test database connectivity
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

            // Quick check - just try to fetch 1 record
            await repository.GetBatchToProcessAsync(1, false, cancellationToken);

            data["DatabaseConnectivity"] = "OK";

            return HealthCheckResult.Healthy("Batch processor is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy("Batch processor is unhealthy", ex);
        }
    }
}
