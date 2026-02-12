using Microsoft.Extensions.Options;
using System.Diagnostics;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Scheduled background service for disabling expired users in products with improved parallel processing.
/// Migrated from UserNotification.ProcessDisableUsersinProducts.
/// Runs once per day at the configured time (default: 3:00 PM).
///
/// Improvements:
/// - Error isolation in parallel loops (no cascade failures)
/// - Endpoint pre-calculated once per cycle (performance)
/// - Rate limiting for API calls (prevents throttling)
/// - Metrics collection (observability)
/// - Proper scope management (thread safety)
/// </summary>
public class DisableExpiredUsersJob(
    IServiceProvider serviceProvider,
    IOptions<BatchProcessorSettings> settings,
    ILogger<DisableExpiredUsersJob> logger,
    IConfiguration configuration,
    BatchProcessingMetrics metrics) : BackgroundService
{
    private readonly string _jobName = "DisableExpiredUsers";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobSettings = settings.Value.DisableExpiredUsers;

        if (!jobSettings.Enabled)
        {
            logger.LogInformation("{JobName} is disabled. Service will not run.", _jobName);
            return;
        }

        logger.LogInformation("{JobName} started. ScheduledTime: {ScheduledTime}, MaxDegreeOfParallelism: {MaxParallelism}", _jobName, jobSettings.ScheduledTime, jobSettings.MaxDegreeOfParallelism);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var scheduledTime = ParseScheduledTime(jobSettings.ScheduledTime);

            // Calculate next run time
            var nextRunTime = GetNextRunTime(now, scheduledTime);
            var delay = nextRunTime - now;

            logger.LogInformation("{JobName}: Next scheduled run at {NextRunTime} (in {Delay})", _jobName, nextRunTime, delay);

            // Wait until scheduled time
            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("{JobName}: Service stopping before scheduled run.", _jobName);
                break;
            }

            // Execute the job
            logger.LogInformation("{JobName}: Starting scheduled execution at {Time}", _jobName, DateTime.Now);
            await ProcessExpiredUsersAsync(jobSettings, stoppingToken);
        }

        logger.LogInformation("{JobName} stopped.", _jobName);
    }

    private TimeSpan ParseScheduledTime(string scheduledTime)
    {
        if (TimeSpan.TryParse(scheduledTime, out var time))
        {
            return time;
        }

        logger.LogWarning("{JobName}: Invalid scheduled time format '{ScheduledTime}'. Using default 15:00:00", _jobName, scheduledTime);
        return new TimeSpan(15, 0, 0); // Default to 3 PM
    }

    private DateTime GetNextRunTime(DateTime now, TimeSpan scheduledTime)
    {
        var today = now.Date.Add(scheduledTime);

        // If the scheduled time has already passed today, schedule for tomorrow
        if (now >= today)
        {
            return today.AddDays(1);
        }

        return today;
    }

    /// <summary>
    /// Processes expired users with improved error handling and performance.
    /// </summary>
    private async Task ProcessExpiredUsersAsync(UserNotificationScheduledJobSettings jobSettings, CancellationToken cancellationToken)
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

                usersList = await batchRepository.GetExpiredUsersToProcessAsync(cancellationToken);

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
            var result = await apiClient.DisableExpiredUsersAsync(userList, endpoint, cancellationToken);

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
