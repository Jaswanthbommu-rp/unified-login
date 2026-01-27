using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Background service for processing enterprise roles records
/// </summary>
public class EnterpriseRolesJob(IServiceProvider serviceProvider, IOptions<BatchProcessorSettings> settings, ILogger<EnterpriseRolesJob> logger) : BackgroundService
{
    private readonly string _jobName = "EnterpriseRolesJob";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobSettings = settings.Value.EnterpriseRoles;

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
                // Continue running despite errors
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Brief delay before retry
            }
        }

        logger.LogInformation("{JobName} stopped.", _jobName);
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
            var pendingBatches = await batchRepository.GetEnterpriseRoleProductUpdateBatchToProcessAsync(jobSettings.BatchSize, cancellationToken);

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
    private async Task ProcessSingleBatchAsync(EnterpriseRoleBatch batch, IBatchRepository batchRepository, IProductApiClient apiClient, JobSettings jobSettings, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("{JobName}: Processing batch {BatchId}, EnterpriseRoleTemplateId: {EnterpriseRoleTemplateId}, SubjectUserPersonaId: {SubjectUserPersonaId}", _jobName, batch.EnterpriseRoleBatchProcessId, batch.EnterpriseRoleTemplateId, batch.SubjectUserPersonaId);
            var configs = await batchRepository.GetBatchConfigurationsAsync(cancellationToken);
            if (configs == null || !configs.Any())
            {
                throw new InvalidOperationException($"Configurations not found for job: {_jobName} for BatchProcessorId: {batch.BatchProcessTypeId}");
            }
            var endpointConfig = configs.FirstOrDefault(c => c.BatchProcessTypeId == batch.BatchProcessTypeId && c.ConfigurationTypeName == "EnterpriseRoleProcessApiEndpoint");

            // Call the API
            var response = await apiClient.ProcessEnterpriseRoleBatchAsync(batch, endpointConfig.Value, cancellationToken);

            logger.LogInformation("{JobName}: Batch {BatchId} processed with status: {Status}", _jobName, batch.EnterpriseRoleBatchProcessId, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{JobName}: Error processing batch {BatchId}", _jobName, batch.EnterpriseRoleBatchProcessId);
        }
    }
}
