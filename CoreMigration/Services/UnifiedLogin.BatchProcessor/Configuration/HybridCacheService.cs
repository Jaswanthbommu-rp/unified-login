using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
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
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cached value or creates it using the provided factory function with custom expiration.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value if not in cache.</param>
    /// <param name="absoluteExpirationMinutes">Absolute expiration in minutes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        int absoluteExpirationMinutes,
        CancellationToken cancellationToken = default);

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
    private bool _redisAvailable;
    private DateTime _lastRedisCheck = DateTime.MinValue;
    private readonly TimeSpan _redisCheckInterval = TimeSpan.FromSeconds(30);

    public CacheMode CurrentCacheMode => _redisAvailable ? CacheMode.Redis : CacheMode.InMemory;

    public HybridCacheService(
        HybridCache hybridCache,
        ILogger<HybridCacheService> logger,
        IOptions<HybridCacheSettings> settings,
        IConnectionMultiplexer? redisConnection = null)
    {
        _hybridCache = hybridCache ?? throw new ArgumentNullException(nameof(hybridCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _redisConnection = redisConnection;

        // Initialize Redis availability status with retry
        _redisAvailable = CheckRedisAvailability();

        // If Redis is not immediately available but connection exists, wait briefly and retry
        if (!_redisAvailable && _redisConnection != null && _settings.Redis.Enabled)
        {
            _logger.LogInformation("Redis connection exists but not yet connected. Waiting briefly for connection to establish...");
            System.Threading.Thread.Sleep(1000); // Wait 1 second
            _redisAvailable = CheckRedisAvailability();
        }

        if (_redisAvailable)
        {
            _logger.LogInformation("ResilientHybridCacheService initialized with Redis cache enabled");
        }
        else
        {
            _logger.LogWarning("ResilientHybridCacheService initialized with in-memory cache only (Redis unavailable)");
            if (_redisConnection != null && _settings.Redis.Enabled)
            {
                _logger.LogInformation("Redis will be rechecked periodically and automatically reconnect when available");
            }
        }
    }

    /// <summary>
    /// Gets a cached value or creates it using the factory function.
    /// Automatically falls back to in-memory cache if Redis fails.
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        return await GetOrCreateAsync(
            key,
            factory,
            _settings.DefaultOptions.AbsoluteExpirationMinutes,
            cancellationToken);
    }

    /// <summary>
    /// Gets a cached value or creates it using the factory function with custom expiration.
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        int absoluteExpirationMinutes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        // Periodically check Redis availability
        CheckRedisAvailabilityPeriodically();

        try
        {
            // Configure cache entry options
            var options = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(absoluteExpirationMinutes),
                LocalCacheExpiration = TimeSpan.FromMinutes(
                    _settings.DefaultOptions.SlidingExpirationMinutes ?? 15)
            };

            // Use HybridCache.GetOrCreateAsync which handles both Redis and in-memory automatically
            var result = await _hybridCache.GetOrCreateAsync(
                key,
                async cancel => await factory(cancel),
                options,
                cancellationToken: cancellationToken);

            _logger.LogDebug(
                "Cache entry retrieved/created successfully. Key: {Key}, Mode: {Mode}",
                key, CurrentCacheMode);

            return result;
        }
        catch (RedisConnectionException redisEx)
        {
            _logger.LogError(redisEx,
                "Redis connection failed for key: {Key}. Falling back to in-memory cache and bypassing distributed cache.",
                key);

            _redisAvailable = false;

            // When Redis fails, still use HybridCache which will use in-memory only
            try
            {
                var options = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(absoluteExpirationMinutes),
                    LocalCacheExpiration = TimeSpan.FromMinutes(
                        _settings.DefaultOptions.SlidingExpirationMinutes ?? 15)
                };

                var result = await _hybridCache.GetOrCreateAsync(
                    key,
                    async cancel => await factory(cancel),
                    options,
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Cache entry retrieved using in-memory fallback. Key: {Key}",
                    key);

                return result;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx,
                    "In-memory fallback also failed for key: {Key}. Executing factory directly.",
                    key);

                // If both fail, execute factory directly
                return await factory(cancellationToken);
            }
        }
        catch (RedisTimeoutException timeoutEx)
        {
            _logger.LogWarning(timeoutEx,
                "Redis timeout for key: {Key}. Using in-memory cache.",
                key);

            _redisAvailable = false;

            // Execute factory directly on timeout
            return await factory(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error accessing cache for key: {Key}. Executing factory directly.",
                key);

            // On any other error, execute factory directly
            return await factory(cancellationToken);
        }
    }

    /// <summary>
    /// Removes a cached entry by key.
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty", nameof(key));
        }

        try
        {
            await _hybridCache.RemoveAsync(key, cancellationToken);

            _logger.LogDebug("Cache entry removed successfully. Key: {Key}", key);
        }
        catch (RedisConnectionException redisEx)
        {
            _logger.LogWarning(redisEx,
                "Failed to remove cache entry from Redis (key: {Key}), but in-memory cache should be cleared.",
                key);

            _redisAvailable = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error removing cache entry. Key: {Key}",
                key);
        }
    }

    /// <summary>
    /// Checks if Redis is currently available.
    /// </summary>
    private bool CheckRedisAvailability()
    {
        if (!_settings.Redis.Enabled)
        {
            return false;
        }

        if (_redisConnection == null)
        {
            return false;
        }

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
    /// Periodically checks Redis availability to enable reconnection.
    /// </summary>
    private void CheckRedisAvailabilityPeriodically()
    {
        if (DateTime.UtcNow - _lastRedisCheck < _redisCheckInterval)
        {
            return;
        }

        _lastRedisCheck = DateTime.UtcNow;

        var wasAvailable = _redisAvailable;
        _redisAvailable = CheckRedisAvailability();

        if (!wasAvailable && _redisAvailable)
        {
            _logger.LogInformation("Redis connection restored. Switching back to Redis cache.");
        }
        else if (wasAvailable && !_redisAvailable)
        {
            _logger.LogWarning("Redis connection lost. Falling back to in-memory cache.");
        }
    }
}
