using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.UserNotification.Configuration;

/// <summary>
/// Configuration options for the user notification service.
/// </summary>
public class UserNotificationOptions
{
    public const string SectionName = "UserNotification";

    /// <summary>
    /// Number of concurrent worker threads for processing notifications.
    /// </summary>
    [Range(1, 20)]
    public int WorkerThreads { get; set; } = 3;

    /// <summary>
    /// Batch size for notification processing.
    /// </summary>
    [Range(1, 500)]
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Maximum number of retry attempts for failed notifications.
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Timeout in seconds for notification processing operations.
    /// </summary>
    [Range(5, 300)]
    public int ProcessingTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Connection string for the notification database.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for the notification API.
    /// </summary>
    [Required]
    public string NotificationApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Enable or disable email notifications.
    /// </summary>
    public bool EnableEmailNotifications { get; set; } = true;

    /// <summary>
    /// Enable or disable SMS notifications.
    /// </summary>
    public bool EnableSmsNotifications { get; set; } = true;

    /// <summary>
    /// Enable or disable push notifications.
    /// </summary>
    public bool EnablePushNotifications { get; set; } = true;

    /// <summary>
    /// Time of day to execute the daily job (24-hour format, e.g., "02:30:00").
    /// </summary>
    [Required]
    public TimeSpan DailyExecutionTime { get; set; } = TimeSpan.FromHours(2); // Default: 2:00 AM

    /// <summary>
    /// Timezone for daily execution (e.g., "Eastern Standard Time", "UTC").
    /// </summary>
    [Required]
    public string DailyExecutionTimeZone { get; set; } = "UTC";
}
