using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UnifiedLogin.SharedObjects.Cache;

namespace UnifiedLogin.BusinessLogic.CacheHelper;

public class MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger) : ICacheService
{
    private T Get<T>(string key)
    {
        return cache.Get<T>(key);
    }

    private Task<T> GetAsync<T>(string key)
    {
        return Task.FromResult(cache.Get<T>(key));
    }

    private void Set<T>(string key, T value, int expirationSeconds)
    {
        cache.Set(key, value, new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
        });
    }

    /// <summary>
    /// Gets or Sets a value in the cache
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="key">The cache key which identifies the entry in the cache.</param>
    /// <param name="factory">The function which will be called if the value is not found in the cache.</param>
    /// <param name="cacheEntryOptions">Settings related to the cache request</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns></returns>
    public async ValueTask<TValue?> GetOrSetAsync<TValue>(string key, Func<CancellationToken, Task<TValue?>> factory, CacheEntryOptions cacheEntryOptions, CancellationToken cancellationToken = default)
    {
        var ret = await GetAsync<TValue>(key);

        try
        {
            if (ret == null)
            {
                ret = await factory(cancellationToken);
                Set(key, ret, (cacheEntryOptions.ExpirationTimeInMinutes * 60));
            }
        }
        catch (Exception ex)
        {
            if (ex.InnerException is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to cache key {key}", key);
                // failed to cache
            }
        }

        return ret;
    }

    /// <summary>
    /// Get or Set a value in the cache
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="key">The cache key which identifies the entry in the cache.</param>
    /// <param name="factory">The function which will be called if the value is not found in the cache.</param>
    /// <param name="cacheEntryOptions">Settings related to the cache request</param>
    /// <returns></returns>
    public TValue GetOrSet<TValue>(string key, Func<TValue?> factory, CacheEntryOptions cacheEntryOptions)
    {
        var ret = Get<TValue>(key);

        if (ret != null) return ret;

        ret = factory();
        Set(key, ret, (cacheEntryOptions.ExpirationTimeInMinutes * 60));

        return ret;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cache.Remove(key);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Get or Set a value in the cache
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="key">The cache key which identifies the entry in the cache.</param>
    /// <param name="defaultValue"></param>
    /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns></returns>
    public TValue? GetOrDefault<TValue>(string key, TValue? defaultValue = default, CancellationToken token = default)
    {
        var ret = Get<TValue>(key);

        return ret ?? defaultValue;
    }
}