using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Configuration options for the batch processor service.
/// </summary>
public class BatchProcessorOptions
{
    public const string SectionName = "BatchProcessor";

    /// <summary>
    /// Number of concurrent worker threads for processing batches.
    /// </summary>
    [Range(1, 50)]
    public int ThreadCount { get; set; } = 5;

    /// <summary>
    /// Polling interval in seconds for checking pending batches.
    /// </summary>
    [Range(1, 3600)]
    public int PollingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Polling interval in seconds for retry batches.
    /// </summary>
    [Range(1, 3600)]
    public int RetryPollingIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Number of batch records to fetch per polling cycle.
    /// </summary>
    [Range(1, 1000)]
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// Wait interval in seconds after an exception occurs.
    /// </summary>
    [Range(1, 7200)]
    public int ExceptionWaitIntervalSeconds { get; set; } = 120;

    /// <summary>
    /// Maximum number of items that can be queued in the work channel.
    /// </summary>
    [Range(10, 10000)]
    public int MaxQueueSize { get; set; } = 100;

    /// <summary>
    /// Base URL for the Landing API.
    /// </summary>
    [Required]
    public string LandingApiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds for API calls.
    /// </summary>
    [Range(1, 600)]
    public int ApiTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Connection string for the IdP Configuration database.
    /// </summary>
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
}
