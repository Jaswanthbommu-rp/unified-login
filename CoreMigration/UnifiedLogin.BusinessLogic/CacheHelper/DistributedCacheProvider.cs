using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace UnifiedLogin.BusinessLogic.CacheHelper;

public class DistributedCacheProvider(IDistributedCache cache, ILogger<DistributedCacheProvider> logger)
    : IDistributedCacheProvider
{
    public async Task<T> ReadThroughCacheAsync<T>(string key, int expirationSeconds, Func<Task<T>> handler)
    {
        var ret = Get<T>(key);
        try
        {
            if (ret == null)
            {
                ret = await handler();
                Set(key, ret, expirationSeconds);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to read through cache for Key {key}", key);
        }

        return ret;
    }

    public T ReadThroughCache<T>(string key, int expirationSeconds, Func<T> handler)
    {
        var ret = Get<T>(key);
        try
        {
            if (ret == null)
            {
                ret = handler();
                Set(key, ret, expirationSeconds);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to read through cache for Key {key}", key);
        }

        return ret;
    }

    public void Set<T>(string key, T value, int expirationSeconds)
    {
        try
        {
            var rawString = ToJSON(value);
            cache.SetString(key, rawString, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to set cache for Key {key}", key);
        }
    }

    public T Get<T>(string key)
    {
        T ret = default;
        try
        {
            ret = FromJSON<T>(cache.GetString(key));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to get cache for Key {key}", key);
        }
        return ret;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        T ret = default;
        try
        {
            ret = FromJSON<T>(await cache.GetStringAsync(key));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to get cache for Key {key}", key);
        }
        return ret;
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to remove cache for Key {key}", key);
        }
    }

    public void Remove(string key)
    {
        try
        {
            cache.Remove(key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to remove cache for Key {key}", key);
        }
    }

    private T FromJSON<T>(string json)
    {
        if (!string.IsNullOrWhiteSpace(json))
            return JsonSerializer.Deserialize<T>(json);
        else
            return default;
    }

    private string ToJSON(object obj)
    {
        return JsonSerializer.Serialize(obj);
    }
}
