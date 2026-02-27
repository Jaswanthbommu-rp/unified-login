using Microsoft.Extensions.Options;
using System.Diagnostics;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Recurring background service for processing future user logins and sending notifications with improved parallel processing.
/// Migrated from UserNotification.SendRegularUserNotification.
/// Runs every 15 minutes (configurable).
///
/// Improvements:
/// - Error isolation in parallel loops (no cascade failures)
/// - Endpoint pre-calculated once per cycle (performance)
/// - Rate limiting for API calls (prevents throttling)
/// - Metrics collection (observability)
/// - Proper scope management (thread safety)
/// </summary>
public class FutureUserLoginsJob(
    IServiceProvider serviceProvider,
    IOptions<BatchProcessorSettings> settings,
    ILogger<FutureUserLoginsJob> logger,
    IConfiguration configuration,
    BatchProcessingMetrics metrics,
    IFeatureFlagService featureFlagService) : BackgroundService
{
    private readonly string _jobName = "FutureUserLogins";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobSettings = settings.Value.FutureUserLogins;

        if (!jobSettings.Enabled)
        {
            logger.LogInformation("{JobName} is disabled. Service will not run.", _jobName);
            return;
        }

        logger.LogInformation("{JobName} started. MaxDegreeOfParallelism: {MaxParallelism}, BatchSize: {BatchSize}, UserSplitSize: {UserSplitSize}",
            _jobName, jobSettings.MaxDegreeOfParallelism, jobSettings.BatchSize, jobSettings.UserSplitSize);

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

                await ProcessFutureUserLoginsAsync(jobSettings, stoppingToken);

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
    /// Processes future user logins with improved error handling and performance.
    /// </summary>
    private async Task ProcessFutureUserLoginsAsync(UserNotificationJobSettings jobSettings, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            // Phase 1: Fetch users and calculate endpoint (single scope)
            List<ProcessUserLogin> usersList;
            string endpoint;

            using (var fetchScope = serviceProvider.CreateScope())
            {
                var batchRepository = fetchScope.ServiceProvider.GetRequiredService<IBatchRepository>();

                usersList = await batchRepository.GetFutureUsersToProcessAsync(jobSettings.BatchSize, cancellationToken);

                if (usersList == null || !usersList.Any())
                {
                    logger.LogInformation("{JobName}: No users to process", _jobName);
                    return;
                }

                metrics.RecordBatchesRetrieved(_jobName, usersList.Count);
                logger.LogInformation("{JobName}: Found {Count} users to process", _jobName, usersList.Count);

                // ✅ CRITICAL: Calculate endpoint ONCE before parallel processing
                string apiurl = configuration["ApiBaseUrl"].EndsWith("/") ? configuration["ApiBaseUrl"] : configuration["ApiBaseUrl"] + "/";
                endpoint = apiurl + jobSettings.ApiEndpoint;

                logger.LogDebug("{JobName}: Using endpoint: {Endpoint}", _jobName, endpoint);
            }

            // Split users into smaller batches for parallel processing
            var splitUserList = SplitList(usersList, jobSettings.UserSplitSize).ToList();
            logger.LogInformation("{JobName}: Split {TotalUsers} users into {BatchCount} batches of size {UserSplitSize}",
                _jobName, usersList.Count, splitUserList.Count, jobSettings.UserSplitSize);

            // Phase 2: Process batches in parallel (each with own scope)
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = jobSettings.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(splitUserList, parallelOptions, async (userBatch, ct) =>
            {
                // ✅ CRITICAL: Wrap in try-catch to prevent cascade failures
                try
                {
                    // ✅ CRITICAL: Create new scope per batch for thread safety
                    using var batchScope = serviceProvider.CreateScope();
                    var apiClient = batchScope.ServiceProvider.GetRequiredService<IProductApiClient>();
                    var rateLimiter = batchScope.ServiceProvider.GetRequiredService<IApiRateLimiter>();

                    await ProcessUserBatchAsync(userBatch, apiClient, rateLimiter, endpoint, ct);

                    Interlocked.Increment(ref successCount);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failureCount);
                    logger.LogError(ex, "{JobName}: Critical error processing user batch of {Count} users. Continuing with remaining batches.",
                        _jobName, userBatch.Count);
                }
            });

            logger.LogInformation("{JobName}: Completed processing {TotalUsers} users in {BatchCount} batches (Success: {SuccessCount}, Failed: {FailureCount})",
                _jobName, usersList.Count, splitUserList.Count, successCount, failureCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error in processing cycle", _jobName);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            metrics.RecordBatchProcessed(_jobName, successCount, failureCount, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Processes a single batch of users with rate limiting and resilience.
    /// </summary>
    private async Task ProcessUserBatchAsync(
        List<ProcessUserLogin> userList,
        IProductApiClient apiClient,
        IApiRateLimiter rateLimiter,
        string endpoint,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("{JobName}: Processing {Count} users", _jobName, userList.Count);

            // ✅ Acquire rate limit before API call
            using var lease = await rateLimiter.AcquireAsync("api", cancellationToken);

            if (!lease.IsAcquired)
            {
                logger.LogWarning("{JobName}: Rate limit exceeded for user batch of {Count}. Will retry in next cycle.",
                    _jobName, userList.Count);
                return; // Batch will be picked up in next cycle
            }

            // Call the API (with Polly resilience handling)
            var result = await apiClient.ProcessUserLoginsAsync(userList, endpoint, cancellationToken);

            logger.LogInformation("{JobName}: Completed processing {Count} users successfully - {Result}",
                _jobName, userList.Count, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error processing {Count} users", _jobName, userList.Count);
            throw; // Re-throw to be caught by parallel loop handler
        }
    }

    /// <summary>
    /// Splits a list into smaller batches.
    /// </summary>
    private static IEnumerable<List<T>> SplitList<T>(List<T> source, int batchSize)
    {
        for (int i = 0; i < source.Count; i += batchSize)
        {
            yield return source.GetRange(i, Math.Min(batchSize, source.Count - i));
        }
    }
}
