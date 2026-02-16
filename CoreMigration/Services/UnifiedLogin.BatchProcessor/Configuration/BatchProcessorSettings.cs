namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Configuration options for the batch processor service.
/// </summary>
public class BatchProcessorSettings
{
    public const string SectionName = "BatchProcessorSettings";
    public JobSettings PendingBatch { get; set; } = new();
    public JobSettings RetryBatch { get; set; } = new();
    public JobSettings PrimaryProperties { get; set; } = new();
    public JobSettings EnterpriseRoles { get; set; } = new();
    public JobSettings BulkUserUpdate { get; set; } = new();
    public JobSettings CompanyAndPropertiesUpdate { get; set; } = new();
    public ScheduledJobSettings UserActivation { get; set; } = new();
    public UserNotificationJobSettings FutureUserLogins { get; set; } = new();
    public UserNotificationJobSettings PendingUsersExpiration { get; set; } = new();
    public UserNotificationScheduledJobSettings DisableExpiredUsers { get; set; } = new();
}
public class JobSettings
{
    public bool Enabled { get; set; } = true;
    public int TimeIntervalInSeconds { get; set; }
    public int MaxDegreeOfParallelism { get; set; } = 5;
    public int BatchSize { get; set; } = 2;

    public string ApiEndpoint { get; set; } // remove later, value is from db
}

public class ScheduledJobSettings : JobSettings
{
    /// <summary>
    /// Time of day to run the job in HH:mm:ss format (e.g., "02:00:00" for 2 AM)
    /// </summary>
    public string ScheduledTime { get; set; } = "02:00:00";
}