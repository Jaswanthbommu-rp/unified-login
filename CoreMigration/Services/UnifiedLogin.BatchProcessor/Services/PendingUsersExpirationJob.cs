using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Recurring background service for processing pending users and updating their status.
/// Migrated from UserNotification.ProcessPendingUsers.
/// Runs every 15 minutes (configurable).
/// </summary>
public class PendingUsersExpirationJob(IServiceProvider serviceProvider, IOptions<BatchProcessorSettings> settings, ILogger<PendingUsersExpirationJob> logger, IConfiguration configuration) : BackgroundService
{
    private readonly string _jobName = "PendingUsersExpiration";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobSettings = settings.Value.PendingUsersExpiration;

        if (!jobSettings.Enabled)
        {
            logger.LogInformation("{JobName} is disabled. Service will not run.", _jobName);
            return;
        }

        logger.LogInformation("{JobName} started. MaxDegreeOfParallelism: {MaxParallelism}", _jobName, jobSettings.MaxDegreeOfParallelism);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("{JobName} cycle running at: {Time}", _jobName, DateTimeOffset.Now);

                await ProcessPendingUsersAsync(jobSettings, stoppingToken);

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
                // Continue running despite errors
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Brief delay before retry
            }
        }

        logger.LogInformation("{JobName} stopped.", _jobName);
    }

    /// <summary>
    /// Processes pending users by fetching from database and calling API endpoint.
    /// </summary>
    private async Task ProcessPendingUsersAsync(UserNotificationJobSettings jobSettings, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var batchRepository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();
        var apiClient = scope.ServiceProvider.GetRequiredService<IProductApiClient>();

        try
        {
            var usersList = await batchRepository.GetPendingUsersToProcessAsync(cancellationToken);
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
            logger.LogError(ex, "{JobName}: Error processing pending users", _jobName);
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
            var result = await apiClient.ProcessUserLoginsAsync(userList, endpoint, cancellationToken);

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new Exception($"{_jobName}: Null or empty response posting to API");
            }

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
