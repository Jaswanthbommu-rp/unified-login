using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.BusinessLogic.CacheHelper;

public interface IDistributedCacheProvider
{
    T Get<T>(string key);
    void Remove(string key);
    Task RemoveAsync(string key);
    void Set<T>(string key, T value, int expirationSeconds);
    Task<T> ReadThroughCacheAsync<T>(string key, int expirationSeconds, Func<Task<T>> handler);
    T ReadThroughCache<T>(string key, int expirationSeconds, Func<T> handler);
}
