using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Build.Framework;
using System.Reflection.PortableExecutable;


namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Providers
{
    public class DistributedCacheProvider : IDistributedCacheProvider
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheProvider> _logger;
        //private readonly ILdClient _featureToggleProvider;

        public DistributedCacheProvider(IDistributedCache cache, ILogger<DistributedCacheProvider> logger
            //ILdClient featureToggleProvider
            )
        {
            _cache = cache;
            _logger = logger;
            //_featureToggleProvider = featureToggleProvider;
        }
        public async Task<T> ReadThroughCacheAsync<T>(string key, int expirationSeconds, Func<Task<T>> handler)
        {
            var ret = await GetAsync<T>(key);
            try
            {
                if (ret == null)
                {
                    ret = await handler();
                    await SetAsync(key, ret, expirationSeconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - {state}", "ReadThroughCacheAsync", $"Unable to read through cache for Key {key}");
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
                _logger.LogError(ex, "{ActionName} - {state}", "ReadThroughCache", $"Unable to read through cache for Key {key}");
            }

            return ret;
        }

        public void Set<T>(string key, T value, int expirationSeconds)
        {
            try
            {
                //if (IsDistributedCacheEnabled())
                if(true)
                {
                    var rawString = ToJSON(value);
                    _cache.SetString(key, rawString, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - {state}", "Set", $"Unable to set cache for Key {key}");
            }
        }

        public T Get<T>(string key)
        {
            T ret = default;
            try
            {
                //if (IsDistributedCacheEnabled())
                if (true)
                    ret = FromJSON<T>(_cache.GetString(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - {state}", "Get", $"Unable to get cache for Key {key}");
            }
            return ret;
        }

        public async Task SetAsync<T>(string key, T value, int expirationSeconds)
        {
            try
            {
                //if (IsDistributedCacheEnabled())
                if (true)
                {
                    var rawString = ToJSON(value);
                    await _cache.SetStringAsync(key, rawString, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationSeconds)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - {state}", "SetAsync", $"Unable to set cache for Key {key}");
            }
        }

        public async Task<T> GetAsync<T>(string key)
        {
            T ret = default;
            try
            {
                //if (IsDistributedCacheEnabled())
                    if (true)
                        ret = FromJSON<T>(await _cache.GetStringAsync(key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - {state}", "GetAsync", $"Unable to get cache for Key {key}");
            }
            return ret;
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                //if (IsDistributedCacheEnabled())
                if (true)
                    await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - {state}", "RemoveAsync", $"Unable to remove cache for Key {key}");
            }
        }

        public void Remove(string key)
        {
            try
            {
                //if (IsDistributedCacheEnabled())
                if (true)
                    _cache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ActionName} - {state}", "Remove", $"Unable to remove cache for Key {key}");
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

        // ... existing code ...

        //private bool IsDistributedCacheEnabled()
        //{
        //    return _featureToggleProvider != null && _featureToggleProvider.BoolVariation("distributed-cache-enabled", User.WithKey("app"), true);
        //}
    }
}