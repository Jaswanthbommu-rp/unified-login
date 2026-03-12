using Microsoft.Extensions.Logging;
using UnifiedLogin.SharedObjects.Cache;

namespace UnifiedLogin.BusinessLogic.CacheHelper;

public class NoOpCacheService(ILogger<NoOpCacheService> logger) : ICacheService
{
    /// <summary>
    /// Do not cache
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <param name="token">Not used</param>
    /// <returns></returns>
    public TValue? GetOrDefault<TValue>(string key, TValue? defaultValue = default, CancellationToken token = default)
    {
        return defaultValue;
    }

    /// <summary>
    /// Do not cache
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="cacheEntryOptions">Not used</param>
    /// <returns></returns>
    public TValue GetOrSet<TValue>(string key, Func<TValue?> factory, CacheEntryOptions cacheEntryOptions)
    {
        logger.LogDebug("Skipped log for key : {key}", key);
        return factory();
    }

    /// <summary>
    /// Do not cache
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="cacheEntryOptions">Not used</param>
    /// <param name="cancellationToken">Not used</param>
    /// <returns></returns>
    public async ValueTask<TValue?> GetOrSetAsync<TValue>(string key, Func<CancellationToken, Task<TValue?>> factory, CacheEntryOptions cacheEntryOptions, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Skipped log for key : {key}", key);
        return await factory(cancellationToken);
    }

    /// <summary>
    /// Do not cache
    /// </summary>
    /// <param name="key">Not used</param>
    /// <param name="cancellationToken">Not used</param>
    /// <returns></returns>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

