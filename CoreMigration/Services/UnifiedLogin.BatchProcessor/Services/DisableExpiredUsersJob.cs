using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Scheduled background service for disabling expired users in products.
/// Migrated from UserNotification.ProcessDisableUsersinProducts.
/// Runs once per day at the configured time (default: 3:00 PM).
/// </summary>
public class DisableExpiredUsersJob(IServiceProvider serviceProvider, IOptions<BatchProcessorSettings> settings, ILogger<DisableExpiredUsersJob> logger, IConfiguration configuration) : BackgroundService
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
    /// Processes expired users by fetching from database and calling API endpoint.
    /// </summary>
    private async Task ProcessExpiredUsersAsync(UserNotificationScheduledJobSettings jobSettings, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var batchRepository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();
        var apiClient = scope.ServiceProvider.GetRequiredService<IProductApiClient>();

        try
        {
            var usersList = await batchRepository.GetExpiredUsersToProcessAsync(cancellationToken);
            if (usersList == null || !usersList.Any())
            {
                logger.LogInformation("{JobName}: No users to process", _jobName);
                return;
            }

            logger.LogInformation("{JobName}: Found {Count} users to process", _jobName, usersList.Count);

            // Split users into smaller batches for parallel processing
            var splitUserList = SplitList(usersList, jobSettings.UserSplitSize);

            // Process batches in parallel with controlled degree of parallelism
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = jobSettings.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(splitUserList, parallelOptions, async (userBatch, ct) =>
            {
                await ProcessUserBatchAsync(userBatch, apiClient, jobSettings, ct);
            });

            logger.LogInformation("{JobName}: Completed processing {Count} users", _jobName, usersList.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error processing expired users", _jobName);
            throw;
        }
    }

    /// <summary>
    /// Processes a single batch of users by calling the API endpoint.
    /// </summary>
    private async Task ProcessUserBatchAsync(List<ProcessUserLogin> userList, IProductApiClient apiClient, JobSettings jobSettings, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("{JobName}: Processing {Count} users", _jobName, userList.Count);
            string apiurl = configuration["ApiBaseUrl"].EndsWith("/") ? configuration["ApiBaseUrl"] : configuration["ApiBaseUrl"] + "/";
            string endpoint = apiurl + jobSettings.ApiEndpoint;
            var result = await apiClient.DisableExpiredUsersAsync(userList, endpoint, cancellationToken);

            logger.LogInformation("{JobName}: Completed processing {Count} users - {Result}", _jobName, userList.Count, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error processing {Count} users", _jobName, userList.Count);
            throw;
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
