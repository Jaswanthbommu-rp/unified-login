using Microsoft.Extensions.Options;
using System.Diagnostics;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Background service for processing company and properties update records with improved parallel processing.
///
/// Improvements:
/// - Error isolation in parallel loops (no cascade failures)
/// - Configuration pre-fetched once per cycle (performance)
/// - Rate limiting for API calls (prevents throttling)
/// - Metrics collection (observability)
/// - Proper scope management (thread safety)
/// </summary>
public class CompanyAndPropertiesUpdateJob(
    IServiceProvider serviceProvider,
    IOptions<BatchProcessorSettings> settings,
    ILogger<CompanyAndPropertiesUpdateJob> logger,
    BatchProcessingMetrics metrics,
    IFeatureFlagService featureFlagService) : BackgroundService
{
    private readonly string _jobName = "CompanyAndPropertiesUpdateJob";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobSettings = settings.Value.CompanyAndPropertiesUpdate;

        if (!jobSettings.Enabled)
        {
            logger.LogInformation("{JobName} is disabled. Service will not run.", _jobName);
            return;
        }

        logger.LogInformation("{JobName} started. MaxDegreeOfParallelism: {MaxParallelism}, BatchSize: {BatchSize}",
            _jobName, jobSettings.MaxDegreeOfParallelism, jobSettings.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("{JobName} cycle running at: {Time}", _jobName, DateTimeOffset.Now);

                // Check the LaunchDarkly feature flag before each cycle.
                // The evaluated value is cached for 30 minutes; LD is only called on cache miss.
                var isBatchProcessorV2Enabled = await featureFlagService.GetBoolFlagAsync(FeatureFlagKeys.BatchProcessorV2, defaultValue: false, stoppingToken);

                if (!isBatchProcessorV2Enabled)
                {
                    logger.LogInformation("{JobName} is disabled by feature flag 'batchProcessorV2'. Skipping cycle.", _jobName);
                    await Task.Delay(TimeSpan.FromSeconds(jobSettings.TimeIntervalInSeconds), stoppingToken);
                    continue;
                }

                await ProcessPendingBatchesAsync(jobSettings, stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(jobSettings.TimeIntervalInSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("{JobName} was cancelled", _jobName);
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{JobName} encountered an error during processing cycle", _jobName);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        logger.LogInformation("{JobName} stopped.", _jobName);
    }

    /// <summary>
    /// Processes company and properties batch records with improved error handling and performance.
    /// </summary>
    private async Task ProcessPendingBatchesAsync(JobSettings jobSettings, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            // Phase 1: Fetch batches and configuration (single scope)
            IList<CompanyPropertyBatch> pendingBatches;
            Dictionary<int, string> configLookup;

            using (var fetchScope = serviceProvider.CreateScope())
            {
                var batchRepository = fetchScope.ServiceProvider.GetRequiredService<IBatchRepository>();

                pendingBatches = await batchRepository.GetCompanyBatchByStatusAsync(
                    jobSettings.BatchSize,
                    BatchStatusType.Waiting,
                    cancellationToken);

                if (pendingBatches == null || !pendingBatches.Any())
                {
                    logger.LogInformation("{JobName}: No pending batches to process", _jobName);
                    return;
                }

                metrics.RecordBatchesRetrieved(_jobName, pendingBatches.Count);
                logger.LogInformation("{JobName}: Found {Count} pending batches to process", _jobName, pendingBatches.Count);

                // ✅ CRITICAL: Fetch configuration ONCE before parallel processing
                var configs = await batchRepository.GetBatchConfigurationsAsync(cancellationToken);
                configLookup = configs
                    .Where(c => c.ConfigurationTypeName == "CompanyPropertiesApiEndpoint")
                    .ToDictionary(c => c.BatchProcessTypeId, c => c.Value);

                logger.LogDebug("{JobName}: Loaded {ConfigCount} endpoint configurations", _jobName, configLookup.Count);
            }

            // Phase 2: Process batches in parallel (each with own scope)
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = jobSettings.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(pendingBatches, parallelOptions, async (batch, ct) =>
            {
                // ✅ CRITICAL: Wrap in try-catch to prevent cascade failures
                try
                {
                    // ✅ CRITICAL: Create new scope per batch for thread safety
                    using var batchScope = serviceProvider.CreateScope();
                    var batchRepository = batchScope.ServiceProvider.GetRequiredService<IBatchRepository>();
                    var apiClient = batchScope.ServiceProvider.GetRequiredService<IProductApiClient>();
                    var rateLimiter = batchScope.ServiceProvider.GetRequiredService<IApiRateLimiter>();

                    await ProcessSingleBatchAsync(batch, batchRepository, apiClient, rateLimiter,
                        configLookup, ct);

                    Interlocked.Increment(ref successCount);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failureCount);
                    logger.LogError(ex, "{JobName}: Critical error processing batch {BatchId}. Continuing with remaining batches.",
                        _jobName, batch.CompanyBatchJobId);

                    // ✅ Try to log error for tracking
                    logger.LogError(ex, "{JobName}: Failed to process batch {BatchId}. Error: {ErrorMessage}",
                        _jobName, batch.CompanyBatchJobId, ex.Message);
                }
            });

            logger.LogInformation("{JobName}: Completed processing {TotalCount} batches (Success: {SuccessCount}, Failed: {FailureCount})",
                _jobName, pendingBatches.Count, successCount, failureCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error in batch processing cycle", _jobName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            metrics.RecordBatchProcessed(_jobName, successCount, failureCount, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Processes a single company and properties batch record with rate limiting and resilience.
    /// </summary>
    private async Task ProcessSingleBatchAsync(
        CompanyPropertyBatch batch,
        IBatchRepository batchRepository,
        IProductApiClient apiClient,
        IApiRateLimiter rateLimiter,
        Dictionary<int, string> configLookup,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("{JobName}: Processing batch {BatchId}, ProductId: {ProductId}, CorrelationId: {CorrelationId}",
                _jobName, batch.CompanyBatchJobId, batch.CompanyInstanceSourceId, batch.CreateUserPersonaId);

            // ✅ Acquire rate limit before API call
            using var lease = await rateLimiter.AcquireAsync("api", cancellationToken);

            if (!lease.IsAcquired)
            {
                logger.LogWarning("{JobName}: Rate limit exceeded for batch {BatchId}. Will retry in next cycle.",
                    _jobName, batch.CompanyBatchJobId);
                return; // Batch will be picked up in next cycle
            }

            // ✅ Use pre-fetched configuration
            if (!configLookup.TryGetValue(batch.BatchProcessorTypeId, out var endpoint))
            {
                throw new InvalidOperationException(
                    $"Endpoint configuration not found for BatchProcessTypeId: {batch.BatchProcessorTypeId}");
            }

            // Call the API (with Polly resilience handling)
            var response = await apiClient.ProcessCompanyPropertyBatchAsync(batch, endpoint, cancellationToken);

            logger.LogInformation("{JobName}: Batch {BatchId} processed successfully: {Response}",
                _jobName, batch.CompanyBatchJobId, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error processing batch {BatchId}", _jobName, batch.CompanyBatchJobId);
            throw; // Re-throw to be caught by parallel loop handler
        }
    }
}
