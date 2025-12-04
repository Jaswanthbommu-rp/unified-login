using System.Threading.Channels;
using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Background service that polls for retry batch records and processes them.
/// </summary>
public class RetryBatchPollingService : BackgroundService
{
    private readonly ILogger<RetryBatchPollingService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<BatchProcessorOptions> _options;
    private readonly Channel<Batch> _workQueue;

    public RetryBatchPollingService(
        ILogger<RetryBatchPollingService> logger,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<BatchProcessorOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options;

        _workQueue = Channel.CreateBounded<Batch>(new BoundedChannelOptions(_options.CurrentValue.MaxQueueSize)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Retry Batch Polling Service starting...");

        try
        {
            var workers = Enumerable.Range(0, _options.CurrentValue.ThreadCount)
                .Select(workerId => ProcessWorkAsync(workerId, stoppingToken))
                .ToList();

            var pollingTask = PollDatabaseAsync(stoppingToken);

            await Task.WhenAll(workers.Append(pollingTask));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Retry Batch Polling Service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in Retry Batch Polling Service");
            throw;
        }
        finally
        {
            _logger.LogInformation("Retry Batch Polling Service stopped");
        }
    }

    private async Task PollDatabaseAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.CurrentValue.RetryPollingIntervalSeconds));

        try
        {
            await PollAndEnqueueAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PollAndEnqueueAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Retry database polling cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in retry polling loop");
            throw;
        }
        finally
        {
            _workQueue.Writer.Complete();
        }
    }

    private async Task PollAndEnqueueAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

            var batches = await repository.GetBatchToProcessAsync(
                _options.CurrentValue.BatchSize,
                isRetry: true,
                stoppingToken);

            if (batches.Count == 0)
            {
                _logger.LogDebug("No retry batches to process");
                return;
            }

            _logger.LogInformation("Enqueuing {Count} retry batches for processing", batches.Count);

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
            _logger.LogError(ex, "Error polling retry batches - will retry on next interval");
        }
    }

    private async Task ProcessWorkAsync(int workerId, CancellationToken stoppingToken)
    {
        _logger.LogDebug("Retry Worker {WorkerId} started", workerId);

        try
        {
            await foreach (var batch in _workQueue.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessBatchAsync(batch, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Retry Worker {WorkerId} cancelled", workerId);
        }
        finally
        {
            _logger.LogDebug("Retry Worker {WorkerId} stopped", workerId);
        }
    }

    private async Task ProcessBatchAsync(Batch batch, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var apiClient = scope.ServiceProvider.GetRequiredService<IProductApiClient>();
        var repository = scope.ServiceProvider.GetRequiredService<IBatchRepository>();

        try
        {
            _logger.LogDebug(
                "Processing retry batch {BatchId} (RetryCount: {RetryCount})",
                batch.BatchProcessorId, batch.RetryCount);

            await repository.UpdateBatchStatusAsync(
                batch.BatchProcessorId,
                BatchStatusType.Processing,
                null,
                stoppingToken);

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

                _logger.LogInformation("Successfully processed retry batch {BatchId}", batch.BatchProcessorId);
            }
            else
            {
                await repository.UpdateBatchStatusAsync(
                    batch.BatchProcessorId,
                    BatchStatusType.Error,
                    result.Message,
                    stoppingToken);

                _logger.LogWarning("Retry batch {BatchId} failed: {Message}", batch.BatchProcessorId, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing retry batch {BatchId}", batch.BatchProcessorId);

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
                _logger.LogError(updateEx, "Failed to update retry batch status for {BatchId}", batch.BatchProcessorId);
            }
        }
    }
}
