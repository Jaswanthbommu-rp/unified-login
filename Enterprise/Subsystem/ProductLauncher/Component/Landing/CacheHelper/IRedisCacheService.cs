using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.CacheHelper
{
    public interface IRedisCacheService
    {
        void SetCacheValue<T>(string key, T value, TimeSpan? expiry = null);
        T GetCacheValue<T>(string key);
        void RemoveCacheValue(string key);
    }
}
