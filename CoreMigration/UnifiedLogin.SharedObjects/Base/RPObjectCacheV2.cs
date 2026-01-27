using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;

namespace UnifiedLogin.SharedObjects.Base
{
    /// <summary>
    /// Object cache implementation using MemoryCache
    /// </summary>
    public class RPObjectCacheV2 : IRPObjectCache
    {
        private readonly MemoryCache _cache;

        /// <summary>
        /// Default constructor using default MemoryCache
        /// </summary>
        public RPObjectCacheV2()
        {
            _cache = MemoryCache.Default;
        }

        /// <summary>
        /// Constructor with custom MemoryCache instance
        /// </summary>
        /// <param name="cache">Custom memory cache instance</param>
        public RPObjectCacheV2(MemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <inheritdoc />
        public T GetFromCache<T>(string key, int cacheMinutes, Func<T> factory)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            // Try to get from cache
            if (_cache.Contains(key))
            {
                var cachedValue = _cache.Get(key);
                if (cachedValue is T typedValue)
                {
                    return typedValue;
                }
            }

            // Not in cache or wrong type, generate new value
            var value = factory();

            // Add to cache with expiration
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheMinutes)
            };

            _cache.Set(key, value, policy);

            return value;
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _cache.Remove(key);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            // MemoryCache.Default doesn't support clearing all items
            // This is a limitation of MemoryCache.Default
            // For a custom MemoryCache, you could track keys and remove them
            throw new NotSupportedException("Clear is not supported on MemoryCache.Default. Use Remove(key) for specific keys.");
        }

        /// <inheritdoc />
        public bool Contains(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _cache.Contains(key);
        }
    }
}
