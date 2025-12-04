using System.Threading.Channels;
using Microsoft.Extensions.Options;
using UnifiedLogin.UserNotification.Configuration;
using UnifiedLogin.UserNotification.Models;

namespace UnifiedLogin.UserNotification.Services;

/// <summary>
/// Background worker service that processes user notifications using PeriodicTimer.
/// This service demonstrates modern .NET 8 patterns with non-blocking execution.
/// </summary>
public class UserNotificationWorker : BackgroundService
{
    private readonly ILogger<UserNotificationWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<UserNotificationOptions> _options;
    private readonly Channel<NotificationJob> _notificationQueue;

    public UserNotificationWorker(
        ILogger<UserNotificationWorker> logger,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<UserNotificationOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options;

        // Create bounded channel for notification queue
        _notificationQueue = Channel.CreateBounded<NotificationJob>(
            new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "User Notification Worker starting. Interval: {Interval}s, Workers: {Workers}, BatchSize: {BatchSize}",
            _options.CurrentValue.IntervalSeconds,
            _options.CurrentValue.WorkerThreads,
            _options.CurrentValue.BatchSize);

        try
        {
            // Start worker tasks for concurrent processing
            var workers = Enumerable.Range(0, _options.CurrentValue.WorkerThreads)
                .Select(workerId => ProcessNotificationsAsync(workerId, stoppingToken))
                .ToList();

            // Start the periodic job scheduler
            var schedulerTask = ScheduleJobsAsync(stoppingToken);

            // Wait for all tasks to complete
            await Task.WhenAll(workers.Append(schedulerTask));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("User Notification Worker cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in User Notification Worker");
            throw;
        }
        finally
        {
            _logger.LogInformation("User Notification Worker stopped");
        }
    }

    /// <summary>
    /// Schedules periodic job execution using PeriodicTimer (non-blocking).
    /// </summary>
    private async Task ScheduleJobsAsync(CancellationToken stoppingToken)
    {
        // ✅ Modern PeriodicTimer - non-blocking, efficient
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(_options.CurrentValue.IntervalSeconds));

        try
        {
            // Execute immediately on startup
            await ProcessJobsAsync(stoppingToken);

            // Then execute periodically
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessJobsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Job scheduling cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in job scheduler");
            throw;
        }
        finally
        {
            // Signal no more jobs will be added
            _notificationQueue.Writer.Complete();
        }
    }

    /// <summary>
    /// Main job processing method - fetches pending notifications and enqueues them.
    /// This is the placeholder method requested in requirements.
    /// </summary>
    private async Task ProcessJobsAsync(CancellationToken stoppingToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Starting job execution at {StartTime}", startTime);

            // Simulate fetching pending notifications from database
            var pendingNotifications = await FetchPendingNotificationsAsync(stoppingToken);

            if (pendingNotifications.Count == 0)
            {
                _logger.LogDebug("No pending notifications to process");
                return;
            }

            _logger.LogInformation(
                "Found {Count} pending notifications. Enqueuing for processing...",
                pendingNotifications.Count);

            // Enqueue notifications for worker threads to process
            foreach (var notification in pendingNotifications)
            {
                await _notificationQueue.Writer.WriteAsync(notification, stoppingToken);
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Job execution completed in {Duration}ms. Enqueued {Count} notifications",
                duration.TotalMilliseconds,
                pendingNotifications.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(
                ex,
                "Job execution failed after {Duration}ms",
                duration.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Fetches pending notifications from the database.
    /// In a real implementation, this would query the database using Dapper or EF Core.
    /// </summary>
    private async Task<List<NotificationJob>> FetchPendingNotificationsAsync(CancellationToken stoppingToken)
    {
        // Simulate database query delay
        await Task.Delay(100, stoppingToken);

        // For demonstration, return mock data
        // In production, this would be:
        // using var scope = _scopeFactory.CreateScope();
        // var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        // return await repository.GetPendingNotificationsAsync(_options.CurrentValue.BatchSize, stoppingToken);

        var notifications = new List<NotificationJob>();

        // Simulate finding some pending notifications
        var random = new Random();
        var count = random.Next(0, 5); // Random between 0-4 notifications

        for (int i = 0; i < count; i++)
        {
            notifications.Add(new NotificationJob
            {
                NotificationId = random.Next(1000, 9999),
                UserId = random.Next(100, 999),
                Type = (NotificationType)random.Next(1, 4),
                Subject = $"Test Notification {i + 1}",
                Message = $"This is a test notification message {i + 1}",
                Priority = NotificationPriority.Normal,
                Status = NotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ScheduledFor = DateTime.UtcNow
            });
        }

        return notifications;
    }

    /// <summary>
    /// Worker task that processes notifications from the queue.
    /// </summary>
    private async Task ProcessNotificationsAsync(int workerId, CancellationToken stoppingToken)
    {
        _logger.LogDebug("Worker {WorkerId} started", workerId);

        try
        {
            // Process notifications as they arrive in the queue
            await foreach (var notification in _notificationQueue.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessSingleNotificationAsync(notification, workerId, stoppingToken);
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
    /// Processes a single notification.
    /// </summary>
    private async Task ProcessSingleNotificationAsync(
        NotificationJob notification,
        int workerId,
        CancellationToken stoppingToken)
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["NotificationId"] = notification.NotificationId,
            ["UserId"] = notification.UserId,
            ["Type"] = notification.Type,
            ["WorkerId"] = workerId
        });

        try
        {
            _logger.LogInformation(
                "Worker {WorkerId} processing notification {NotificationId} (Type: {Type}, Priority: {Priority})",
                workerId,
                notification.NotificationId,
                notification.Type,
                notification.Priority);

            // Simulate notification processing
            using var timeout = new CancellationTokenSource(
                TimeSpan.FromSeconds(_options.CurrentValue.ProcessingTimeoutSeconds));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                timeout.Token);

            var result = await SendNotificationAsync(notification, linkedCts.Token);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Worker {WorkerId} successfully processed notification {NotificationId}",
                    workerId,
                    notification.NotificationId);
            }
            else
            {
                _logger.LogWarning(
                    "Worker {WorkerId} failed to process notification {NotificationId}: {Message}",
                    workerId,
                    notification.NotificationId,
                    result.Message);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Worker {WorkerId} cancelled while processing notification {NotificationId}",
                workerId,
                notification.NotificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Worker {WorkerId} error processing notification {NotificationId}",
                workerId,
                notification.NotificationId);
        }
    }

    /// <summary>
    /// Sends a notification based on its type.
    /// In a real implementation, this would call external notification services.
    /// </summary>
    private async Task<NotificationResult> SendNotificationAsync(
        NotificationJob notification,
        CancellationToken stoppingToken)
    {
        try
        {
            // Simulate network delay for sending notification
            var delay = notification.Type switch
            {
                NotificationType.Email => 500,  // Email takes longer
                NotificationType.Sms => 200,    // SMS is faster
                NotificationType.Push => 100,   // Push is fastest
                NotificationType.InApp => 50,   // In-app is instant
                _ => 200
            };

            await Task.Delay(delay, stoppingToken);

            // Simulate 90% success rate
            var random = new Random();
            var success = random.Next(0, 10) < 9;

            return new NotificationResult
            {
                Success = success,
                Message = success
                    ? $"{notification.Type} notification sent successfully"
                    : $"Failed to send {notification.Type} notification - service unavailable",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new NotificationResult
            {
                Success = false,
                Message = $"Exception: {ex.Message}",
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("User Notification Worker is stopping - completing in-flight work...");
        await base.StopAsync(cancellationToken);
    }
}
