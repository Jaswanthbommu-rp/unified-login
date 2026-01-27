namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Configuration settings for User Notification jobs (migrated from old UserNotification service).
/// </summary>
public class UserNotificationJobSettings : JobSettings
{
    /// <summary>
    /// Number of users to process in a single batch split.
    /// Default: 20 users per batch.
    /// </summary>
    public int UserSplitSize { get; set; } = 20;
}

/// <summary>
/// Configuration settings for scheduled User Notification jobs.
/// </summary>
public class UserNotificationScheduledJobSettings : ScheduledJobSettings
{
    /// <summary>
    /// Number of users to process in a single batch split.
    /// Default: 20 users per batch.
    /// </summary>
    public int UserSplitSize { get; set; } = 20;
}
