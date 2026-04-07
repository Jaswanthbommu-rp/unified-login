using StackExchange.Redis;
using System;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.CacheHelper
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _cache;

        public RedisCacheService()
        {
            _cache = RedisCacheManager.Cache;
        }

        public void SetCacheValue<T>(string key, T value, TimeSpan? expiry = null)
        {
            if (_cache == null) return;
            var serializedValue = JsonConvert.SerializeObject(value);
            _cache.StringSet(key, serializedValue, expiry);
        }

        public T GetCacheValue<T>(string key)
        {
            if (_cache == null) return default(T);
            var serializedValue = _cache.StringGet(key);
            return serializedValue.HasValue ? JsonConvert.DeserializeObject<T>(serializedValue) : default(T);
        }

        public void RemoveCacheValue(string key)
        {
            if (_cache == null) return;
            _cache.KeyDelete(key);
        }
    }
}