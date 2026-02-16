namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Configuration settings for HybridCache with Redis primary and in-memory fallback.
/// </summary>
public class HybridCacheSettings
{
    public const string SectionName = "HybridCache";

    /// <summary>
    /// Redis connection configuration.
    /// </summary>
    public RedisSettings Redis { get; set; } = new();

    /// <summary>
    /// In-memory cache configuration (used as fallback).
    /// </summary>
    public MemoryCacheSettings Memory { get; set; } = new();

    /// <summary>
    /// Default cache entry options.
    /// </summary>
    public DefaultCacheOptions DefaultOptions { get; set; } = new();
}

/// <summary>
/// Redis connection settings.
/// </summary>
public class RedisSettings
{
    /// <summary>
    /// Redis connection string (e.g., "localhost:6379,password=mypassword").
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Enable Redis caching (if false, only in-memory cache is used).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Connection timeout in milliseconds.
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Sync timeout for operations in milliseconds.
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Abort connection on connection failure.
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Enable automatic reconnection.
    /// </summary>
    public bool AllowReconnect { get; set; } = true;

    /// <summary>
    /// Number of connection retry attempts.
    /// </summary>
    public int ConnectRetry { get; set; } = 3;
}

/// <summary>
/// In-memory cache settings.
/// </summary>
public class MemoryCacheSettings
{
    /// <summary>
    /// Maximum size limit (in MB) for in-memory cache.
    /// </summary>
    public int SizeLimitMB { get; set; } = 256;

    /// <summary>
    /// Compaction percentage (0.0 to 1.0) when size limit is reached.
    /// </summary>
    public double CompactionPercentage { get; set; } = 0.25;
}

/// <summary>
/// Default cache entry expiration options.
/// </summary>
public class DefaultCacheOptions
{
    /// <summary>
    /// Default absolute expiration time in minutes.
    /// </summary>
    public int AbsoluteExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Default sliding expiration time in minutes.
    /// </summary>
    public int? SlidingExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Enable distributed cache serialization.
    /// </summary>
    public bool EnableDistributedCache { get; set; } = true;
}
