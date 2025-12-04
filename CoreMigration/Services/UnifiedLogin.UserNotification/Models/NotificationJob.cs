namespace UnifiedLogin.UserNotification.Models;

/// <summary>
/// Represents a notification job to be processed.
/// </summary>
public class NotificationJob
{
    public long NotificationId { get; set; }
    public long UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }
    public string? DeviceToken { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Notification type enumeration.
/// </summary>
public enum NotificationType
{
    Email = 1,
    Sms = 2,
    Push = 3,
    InApp = 4
}

/// <summary>
/// Notification priority levels.
/// </summary>
public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Notification status enumeration.
/// </summary>
public enum NotificationStatus
{
    Pending = 1,
    Processing = 2,
    Sent = 3,
    Failed = 4,
    Cancelled = 5
}

/// <summary>
/// Result of a notification processing operation.
/// </summary>
public class NotificationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
