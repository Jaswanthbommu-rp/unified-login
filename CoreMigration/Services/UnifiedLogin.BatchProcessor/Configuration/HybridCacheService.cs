using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Resilient caching service that uses HybridCache with Redis as primary and in-memory as fallback.
/// Automatically falls back to in-memory cache when Redis is unavailable.
/// </summary>
public interface IHybridCacheService
{
    /// <summary>
    /// Gets a cached value or creates it using the provided factory function.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value if not in cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached value or creates it using the provided factory function with custom expiration.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value if not in cache.</param>
    /// <param name="absoluteExpirationMinutes">Absolute expiration in minutes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, int absoluteExpirationMinutes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached entry by key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current cache mode (Redis or InMemory).
    /// </summary>
    CacheMode CurrentCacheMode { get; }
}

/// <summary>
/// Enum representing the current cache mode.
/// </summary>
public enum CacheMode
{
    Redis,
    InMemory
}

/// <summary>
/// Implementation of resilient hybrid cache service with automatic fallback.
/// </summary>
public class HybridCacheService : IHybridCacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly HybridCacheSettings _settings;
    private readonly IConnectionMultiplexer? _redisConnection;

    // Fix #1: volatile ensures reads/writes are not cached in registers across threads.
    private volatile bool _redisAvailable;
    // Fix #1: long (ticks) + Interlocked for atomic reads/writes of the timestamp.
    private long _lastRedisCheckTicks = DateTime.MinValue.Ticks;
    private readonly TimeSpan _redisCheckInterval = TimeSpan.FromSeconds(30);

    public CacheMode CurrentCacheMode => _redisAvailable ? CacheMode.Redis : CacheMode.InMemory;

    public HybridCacheService(HybridCache hybridCache, ILogger<HybridCacheService> logger, IOptions<HybridCacheSettings> settings, IConnectionMultiplexer? redisConnection = null)
    {
        _hybridCache = hybridCache ?? throw new ArgumentNullException(nameof(hybridCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _redisConnection = redisConnection;

        // Fix #2: Removed Thread.Sleep — StackExchange.Redis reconnects in the background automatically.
        _redisAvailable = CheckRedisAvailability();

        if (_redisAvailable)
            _logger.LogInformation("ResilientHybridCacheService initialized with Redis cache enabled");
        else
            _logger.LogWarning("ResilientHybridCacheService initialized with in-memory cache only (Redis unavailable)");
    }

    /// <summary>
    /// Gets a cached value or creates it using the factory function.
    /// Automatically falls back to factory execution if both cache layers fail.
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken cancellationToken = default)
    {
        return await GetOrCreateAsync(key, factory, _settings.DefaultOptions.AbsoluteExpirationMinutes, cancellationToken);
    }

    /// <summary>
    /// Gets a cached value or creates it using the factory function with custom expiration.
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, Task<T>> factory, int absoluteExpirationMinutes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        ArgumentNullException.ThrowIfNull(factory);

        // Fix #6: Only run the periodic Redis check when Redis was previously marked unavailable.
        if (!_redisAvailable)
            CheckRedisAvailabilityPeriodically();

        // Fix #5: Build options once, outside of try/catch to avoid duplication.
        var options = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(absoluteExpirationMinutes),
            LocalCacheExpiration = TimeSpan.FromMinutes(_settings.DefaultOptions.SlidingExpirationMinutes ?? 15)
        };

        try
        {
            // Fix #4: Wrap Task<T> in ValueTask<T> to avoid async state machine allocation.
            var result = await _hybridCache.GetOrCreateAsync<T>(key, cancel => new ValueTask<T>(factory(cancel)), options, cancellationToken: cancellationToken);

            _logger.LogDebug("Cache entry retrieved/created successfully. Key: {Key}, Mode: {Mode}", key, CurrentCacheMode);

            return result;
        }
        catch (RedisConnectionException redisEx)
        {
            // Fix #3: Removed redundant _hybridCache retry — it would hit the same failing Redis.
            // Fall through to factory directly.
            _logger.LogError(redisEx, "Redis connection failed for key: {Key}. Executing factory directly.", key);
            _redisAvailable = false;
            return await factory(cancellationToken);
        }
        catch (RedisTimeoutException timeoutEx)
        {
            _logger.LogWarning(timeoutEx, "Redis timeout for key: {Key}. Executing factory directly.", key);
            _redisAvailable = false;
            return await factory(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error accessing cache for key: {Key}. Executing factory directly.", key);
            return await factory(cancellationToken);
        }
    }

    /// <summary>
    /// Removes a cached entry by key.
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));

        try
        {
            await _hybridCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache entry removed successfully. Key: {Key}", key);
        }
        catch (RedisConnectionException redisEx)
        {
            _logger.LogWarning(redisEx,
                "Failed to remove cache entry from Redis (key: {Key}), but in-memory cache should be cleared.", key);
            _redisAvailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry. Key: {Key}", key);
        }
    }

    /// <summary>
    /// Checks if Redis is currently available by inspecting the connection state.
    /// </summary>
    private bool CheckRedisAvailability()
    {
        if (!_settings.Redis.Enabled || _redisConnection == null)
            return false;

        try
        {
            return _redisConnection.IsConnected;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check Redis availability");
            return false;
        }
    }

    /// <summary>
    /// Periodically re-checks Redis so the service can self-heal after an outage.
    /// Only called when Redis is currently marked unavailable.
    /// Thread-safe via Interlocked on the timestamp field.
    /// </summary>
    private void CheckRedisAvailabilityPeriodically()
    {
        var lastTicks = Interlocked.Read(ref _lastRedisCheckTicks);
        if (DateTime.UtcNow.Ticks - lastTicks < _redisCheckInterval.Ticks)
            return;

        Interlocked.Exchange(ref _lastRedisCheckTicks, DateTime.UtcNow.Ticks);

        var isNowAvailable = CheckRedisAvailability();
        if (isNowAvailable)
            _logger.LogInformation("Redis connection restored. Switching back to Redis cache.");

        _redisAvailable = isNowAvailable;
    }
}
