using Microsoft.Extensions.Options;
using System.Text.Json;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Scheduled background service for processing user activation records
/// Runs once per day at the configured time
/// </summary>
public class UserActivationJob(IServiceProvider serviceProvider, IOptions<BatchProcessorSettings> settings, ILogger<UserActivationJob> logger) : BackgroundService
{
    private readonly string _jobName = "UserActivation";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobSettings = settings.Value.UserActivation;

        if (!jobSettings.Enabled)
        {
            logger.LogInformation("{JobName} is disabled. Service will not run.", _jobName);
            return;
        }

        logger.LogInformation("{JobName} started. MaxDegreeOfParallelism: {MaxParallelism}", _jobName, jobSettings.MaxDegreeOfParallelism);

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
            await ProcessPendingBatchesAsync(jobSettings, stoppingToken);
        }

        logger.LogInformation("{JobName} stopped.", _jobName);
    }

    private TimeSpan ParseScheduledTime(string scheduledTime)
    {
        if (TimeSpan.TryParse(scheduledTime, out var time))
        {
            return time;
        }

        logger.LogWarning("{JobName}: Invalid scheduled time format '{ScheduledTime}'. Using default 02:00:00", _jobName, scheduledTime);
        return new TimeSpan(2, 0, 0); // Default to 2 AM
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
    /// Processes pending batch records by fetching from database and calling API endpoints.
    /// </summary>
    private async Task ProcessPendingBatchesAsync(JobSettings jobSettings, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var batchRepository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();
        var apiClient = scope.ServiceProvider.GetRequiredService<IProductApiClient>();

        try
        {
            // Fetch batch configurations to get batch size
            var batchConfigs = await batchRepository.GetBatchConfigurationsAsync(cancellationToken);
            var batchSizeConfig = batchConfigs.FirstOrDefault(c =>
                                            c.ConfigurationTypeName == "BatchSize" ||
                                            c.ProcessName.Contains("Pending", StringComparison.OrdinalIgnoreCase));

            var batchSize = batchSizeConfig != null && int.TryParse(batchSizeConfig.Value, out var size) ? size : 10;

            // Fetch pending batches (excluding error records for now)
            var pendingBatches = await batchRepository.GetBatchToProcessAsync(batchSize, shouldIncludeErrorRecords: false, retryCount: 3, cancellationToken);

            if (pendingBatches == null || !pendingBatches.Any())
            {
                logger.LogInformation("{JobName}: No pending batches to process", _jobName);
                return;
            }

            logger.LogInformation("{JobName}: Found {Count} pending batches to process", _jobName, pendingBatches.Count);

            // Process batches in parallel with controlled degree of parallelism
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = jobSettings.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(pendingBatches, parallelOptions, async (batch, ct) =>
            {
                await ProcessSingleBatchAsync(batch, batchRepository, apiClient, jobSettings, ct);
            });

            logger.LogInformation("{JobName}: Completed processing {Count} batches", _jobName, pendingBatches.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error processing pending batches", _jobName);
            throw;
        }
    }

    /// <summary>
    /// Processes a single batch record by calling the appropriate API endpoint and updating status.
    /// </summary>
    private async Task ProcessSingleBatchAsync(Batch batch, IBatchRepository batchRepository, IProductApiClient apiClient, JobSettings jobSettings, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("{JobName}: Processing batch {BatchId}, ProductId: {ProductId}, CorrelationId: {CorrelationId}", _jobName, batch.BatchProcessorId, batch.ProductId, batch.CorrelationId);

            // Update status to Running
            await batchRepository.UpdateBatchRecordAsync(batch.BatchProcessorId, BatchStatusType.Running, batch.InputJson, null, cancellationToken);

            // Get API endpoint - use jobSettings.ApiEndpoint or get from configuration
            var apiEndpoint = jobSettings.ApiEndpoint;
            if (string.IsNullOrWhiteSpace(apiEndpoint))
            {
                var configs = await batchRepository.GetBatchConfigurationsAsync(cancellationToken);
                var endpointConfig = configs.FirstOrDefault(c =>
                                            c.BatchProcessTypeId == batch.BatchProcessTypeId &&
                                            c.ConfigurationTypeName == "ProcessApiEndpoint");

                apiEndpoint = endpointConfig?.Value ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(apiEndpoint))
            {
                throw new InvalidOperationException($"API endpoint not configured for batch type {batch.BatchProcessTypeId}");
            }

            // Create batch processor input
            var batchInput = new BatchProcessorInput
            {
                ProductBatchId = batch.BatchProcessorId,
                RealPageId = batch.EditorUserRealPageId,
                ProductId = batch.ProductId,
                CreateUserPersonaId = batch.EditorUserPersonaId,
                AssignUserPersonaId = batch.SubjectUserPersonaId,
                InputJson = batch.InputJson,
                BatchProcessType = batch.BatchProcessTypeId,
                CorrelationId = batch.CorrelationId,
                ProcessApiEndPoint = apiEndpoint,
                BatchProcessorGroupId = batch.BatchProcessorGroupId,
                ImpersonatorUserId = batch.ImpersonatorUserId
            };

            // Call the API
            var response = await apiClient.ProcessBatchAsync(batchInput, cancellationToken);

            // Parse response to determine success
            bool success = false;
            string? errorMessage = null;

            if (!string.IsNullOrWhiteSpace(response))
            {
                try
                {
                    var responseObj = JsonSerializer.Deserialize<BatchProcessorResponse>(response);
                    success = responseObj?.Success ?? false;
                    errorMessage = responseObj?.Message;
                }
                catch
                {
                    // If response can't be parsed, treat as success if no exception was thrown
                    success = true;
                }
            }

            // Update status based on result
            var finalStatus = success ? BatchStatusType.Success : BatchStatusType.Error;
            await batchRepository.UpdateBatchRecordAsync(batch.BatchProcessorId, finalStatus, batch.InputJson, errorMessage, cancellationToken);

            logger.LogInformation("{JobName}: Batch {BatchId} processed with status: {Status}", _jobName, batch.BatchProcessorId, finalStatus);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error processing batch {BatchId}", _jobName, batch.BatchProcessorId);

            try
            {
                // Update batch status to Error
                await batchRepository.UpdateBatchRecordAsync(batch.BatchProcessorId, BatchStatusType.Error, batch.InputJson, ex.Message, cancellationToken);
            }
            catch (Exception updateEx)
            {
                logger.LogError(updateEx, "{JobName}: Failed to update error status for batch {BatchId}", _jobName, batch.BatchProcessorId);
            }
        }
    }
}
