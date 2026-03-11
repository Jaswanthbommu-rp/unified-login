using UnifiedLogin.SharedObjects.Cache;

namespace UnifiedLogin.BusinessLogic.Interfaces;

public interface ICacheService
{
    /// <summary>
    /// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="defaultValue"/> will be returned.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the cache.</typeparam>
    /// <param name="key">The cache key which identifies the entry in the cache.</param>
    /// <param name="defaultValue">The default value to return if the value for the given <paramref name="key"/> is not in the cache.</param>
    /// <param name="token">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns></returns>
    public TValue? GetOrDefault<TValue>(string key, TValue? defaultValue = default, CancellationToken token = default);

    /// <summary>
    /// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="factory"/> will be called and the returned value saved.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the cache.</typeparam>
    /// <param name="key">The cache key which identifies the entry in the cache.</param>
    /// <param name="factory">The function which will be called if the value is not found in the cache.</param>
    /// <param name="cacheEntryOptions">Settings related to the cache request</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The value in the cache, either already there or generated using the provided <paramref name="factory"/> .</returns>
    public ValueTask<TValue?> GetOrSetAsync<TValue>(string key, Func<CancellationToken, Task<TValue?>> factory, CacheEntryOptions cacheEntryOptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the value of type <typeparamref name="TValue"/> in the cache for the specified <paramref name="key"/>: if not there, the <paramref name="factory"/> will be called and the returned value saved.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="cacheEntryOptions">Settings related to the cache request</param>
    /// <returns></returns>
    public TValue GetOrSet<TValue>(string key, Func<TValue?> factory, CacheEntryOptions cacheEntryOptions);

    /// <summary>
    /// Remove key from cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}


