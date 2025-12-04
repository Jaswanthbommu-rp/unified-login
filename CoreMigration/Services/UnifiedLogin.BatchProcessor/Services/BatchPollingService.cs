using System.Threading.Channels;
using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Background service that polls for pending batch records and processes them.
/// Uses PeriodicTimer for efficient non-blocking polling.
/// </summary>
public class BatchPollingService : BackgroundService
{
    private readonly ILogger<BatchPollingService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<BatchProcessorOptions> _options;
    private readonly Channel<Batch> _workQueue;

    public BatchPollingService(
        ILogger<BatchPollingService> logger,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BatchProcessorOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options;

        // Create bounded channel to prevent memory issues
        _workQueue = Channel.CreateBounded<Batch>(new BoundedChannelOptions(_options.CurrentValue.MaxQueueSize)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Batch Polling Service starting. ThreadCount: {ThreadCount}, BatchSize: {BatchSize}, PollingInterval: {PollingInterval}s",
            _options.CurrentValue.ThreadCount,
            _options.CurrentValue.BatchSize,
            _options.CurrentValue.PollingIntervalSeconds);

        try
        {
            // Start worker tasks
            var workers = Enumerable.Range(0, _options.CurrentValue.ThreadCount)
                .Select(workerId => ProcessWorkAsync(workerId, stoppingToken))
                .ToList();

            // Start polling task
            var pollingTask = PollDatabaseAsync(stoppingToken);

            // Wait for all tasks to complete
            await Task.WhenAll(workers.Append(pollingTask));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Batch Polling Service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in Batch Polling Service");
            throw;
        }
        finally
        {
            _logger.LogInformation("Batch Polling Service stopped");
        }
    }

    /// <summary>
    /// Polls the database for pending batches at regular intervals using PeriodicTimer.
    /// </summary>
    private async Task PollDatabaseAsync(CancellationToken stoppingToken)
    {
        // Use PeriodicTimer for modern, non-blocking periodic execution
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.CurrentValue.PollingIntervalSeconds));

        try
        {
            // Execute immediately on startup
            await PollAndEnqueueAsync(stoppingToken);

            // Then execute periodically
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PollAndEnqueueAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Database polling cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in polling loop");
            throw;
        }
        finally
        {
            // Signal that no more work will be added
            _workQueue.Writer.Complete();
        }
    }

    /// <summary>
    /// Fetches batches from database and enqueues them for processing.
    /// </summary>
    private async Task PollAndEnqueueAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

            var batches = await repository.GetBatchToProcessAsync(
                _options.CurrentValue.BatchSize,
                isRetry: false,
                stoppingToken);

            if (batches.Count == 0)
            {
                _logger.LogDebug("No pending batches to process");
                return;
            }

            _logger.LogInformation("Enqueuing {Count} batches for processing", batches.Count);

            foreach (var batch in batches)
            {
                await _workQueue.Writer.WriteAsync(batch, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling database - will retry on next interval");
            // Don't throw - allow the timer to continue
        }
    }

    /// <summary>
    /// Worker task that processes batches from the queue.
    /// </summary>
    private async Task ProcessWorkAsync(int workerId, CancellationToken stoppingToken)
    {
        _logger.LogDebug("Worker {WorkerId} started", workerId);

        try
        {
            await foreach (var batch in _workQueue.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessBatchAsync(batch, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Worker {WorkerId} cancelled", workerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker {WorkerId} encountered an error", workerId);
        }
        finally
        {
            _logger.LogDebug("Worker {WorkerId} stopped", workerId);
        }
    }

    /// <summary>
    /// Processes a single batch record by calling the API and updating status.
    /// </summary>
    private async Task ProcessBatchAsync(Batch batch, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IProductApiClient>();
        var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["BatchId"] = batch.BatchProcessorId,
            ["ProductId"] = batch.ProductId,
            ["CorrelationId"] = batch.CorrelationId
        });

        try
        {
            _logger.LogDebug(
                "Processing batch {BatchId} for product {ProductId} and user {UserId}",
                batch.BatchProcessorId, batch.ProductId, batch.SubjectUserPersonaId);

            // Mark as processing
            await repository.UpdateBatchStatusAsync(
                batch.BatchProcessorId,
                BatchStatusType.Processing,
                null,
                stoppingToken);

            // Call the API
            var input = new BatchProcessorInput
            {
                ProductBatchId = batch.BatchProcessorId,
                ProductId = batch.ProductId,
                AssignUserPersonaId = batch.SubjectUserPersonaId,
                CreateUserPersonaId = batch.EditorUserPersonaId,
                RealPageId = batch.EditorUserRealPageId,
                InputJson = batch.InputJson,
                CorrelationId = batch.CorrelationId,
                BatchProcessType = batch.BatchProcessTypeId,
                BatchProcessorGroupId = batch.BatchProcessorGroupId,
                ImpersonatorUserId = batch.ImpersonatorUserId
            };

            var result = await apiClient.ProcessBatchAsync(input, stoppingToken);

            if (result.Success)
            {
                await repository.UpdateBatchStatusAsync(
                    batch.BatchProcessorId,
                    BatchStatusType.Completed,
                    result.Message,
                    stoppingToken);

                _logger.LogInformation(
                    "Successfully processed batch {BatchId} for product {ProductId}",
                    batch.BatchProcessorId, batch.ProductId);
            }
            else
            {
                await repository.UpdateBatchStatusAsync(
                    batch.BatchProcessorId,
                    BatchStatusType.Error,
                    result.Message,
                    stoppingToken);

                _logger.LogWarning(
                    "Batch {BatchId} processing failed: {Message}",
                    batch.BatchProcessorId, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch {BatchId}", batch.BatchProcessorId);

            try
            {
                await repository.UpdateBatchStatusAsync(
                    batch.BatchProcessorId,
                    BatchStatusType.Error,
                    ex.Message,
                    stoppingToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update batch status for {BatchId}", batch.BatchProcessorId);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Batch Polling Service is stopping - completing in-flight work...");
        await base.StopAsync(cancellationToken);
    }
}
