using System;
using System.Threading.Tasks;

namespace UnifiedLogin.Core.Interfaces.Providers
{
    public interface ICacheProvider
    {
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        void Remove(string key);
        Task RemoveAsync(string key);
        void Set<T>(string key, T value, int expirationSeconds);
        T ReadThroughCache<T>(string key, int expirationSeconds, Func<T> handler);

        Task<T> ReadThroughCacheAsync<T>(string key, int expirationSeconds, Func<Task<T>> handler);
    }
}