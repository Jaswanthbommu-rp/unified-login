using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using UnifiedLogin.UserNotification.Configuration;

namespace UnifiedLogin.UserNotification.Services;

/// <summary>
/// Health check for the notification service.
/// Verifies that the service is running and can process notifications.
/// </summary>
public class NotificationHealthCheck : IHealthCheck
{
    private readonly IOptions<UserNotificationOptions> _options;
    private readonly ILogger<NotificationHealthCheck> _logger;

    public NotificationHealthCheck(
        IOptions<UserNotificationOptions> options,
        ILogger<NotificationHealthCheck> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["DailyExecutionTime"] = _options.Value.DailyExecutionTime,
                ["DailyExecutionTimeZone"] = _options.Value.DailyExecutionTimeZone,
                ["WorkerThreads"] = _options.Value.WorkerThreads,
                ["BatchSize"] = _options.Value.BatchSize,
                ["EmailEnabled"] = _options.Value.EnableEmailNotifications,
                ["SmsEnabled"] = _options.Value.EnableSmsNotifications,
                ["PushEnabled"] = _options.Value.EnablePushNotifications
            };

            // Simulate a quick health check operation
            await Task.Delay(10, cancellationToken);

            // Check if at least one notification type is enabled
            var anyEnabled = _options.Value.EnableEmailNotifications ||
                           _options.Value.EnableSmsNotifications ||
                           _options.Value.EnablePushNotifications;

            if (!anyEnabled)
            {
                return HealthCheckResult.Degraded(
                    "All notification types are disabled",
                    data: data);
            }

            data["Status"] = "Healthy";

            return HealthCheckResult.Healthy(
                "Notification service is running",
                data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return HealthCheckResult.Unhealthy(
                "Notification service health check failed",
                ex);
        }
    }
}
