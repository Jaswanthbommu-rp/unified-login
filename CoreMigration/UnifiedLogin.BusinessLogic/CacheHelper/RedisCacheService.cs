using StackExchange.Redis;
using System;
using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.CacheHelper
{
    public class RedisCacheService: IRedisCacheService
    {
        private readonly IDatabase _cache;

        public RedisCacheService()
        {
            _cache = RedisCacheManager.Cache;
        }

        public void SetCacheValue<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            _cache.StringSet(key, serializedValue, expiry);
        }

        public T GetCacheValue<T>(string key)
        {
            var serializedValue = _cache.StringGet(key);
            return serializedValue.HasValue ? JsonConvert.DeserializeObject<T>(serializedValue) : default(T);
        }

        public void RemoveCacheValue(string key)
        {
            _cache.KeyDelete(key);
        }
    }
}
