using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Base
{
    /// <summary>
    /// Used to cache objects
    /// </summary>
    public class RPObjectCache
    {
        private static ObjectCache cache = MemoryCache.Default;


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// /// <param name="expirationseconds"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public T GetFromCache<T>(string key, int expirationseconds, Func<T> valueFactory) where T : class
        {
            var newValue = new Lazy<T>(valueFactory);
            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expirationseconds) };
            //return newValue.Value; // enable to remove caching for testing
            var value = cache.AddOrGetExisting(key, newValue, policy) as Lazy<T>;
            try
            {
                var result = (value ?? newValue).Value;
                if (result == null)
                {
                    cache.Remove(key);
                }

                return result;
            }
            catch (Exception ex)
            {
                // if the function to get the data ever fails, it will cache the exception. We need to remove the bad cached data so the next attempt will cache valid data
                // throw new Exception(ex.Message);// enable for testing
                var logData = new Dictionary<string, object>{
                    { "key", key }
                };

                var logger = Log.Logger;
				logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
				logger = logger.ForContext("ProductModule", this.GetType());
                logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "GetFromCache", propertyValue1: $"Error: {ex.Message}");

                cache.Remove(key);
                return null;
            }
        }

        public async Task<T> GetFromCacheAsync<T>(string key, int expirationseconds, Func<T> valueFactory) where T : class
        {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expirationseconds) };

            var newValue = new AsyncLazy<T>(valueFactory);
            var value = cache.AddOrGetExisting(key, newValue, policy) as AsyncLazy<T>;
            try
            {
                var result = value != null ? (T)await value.Value : (T)await newValue.Value;
                if (result == null)
                {
                    cache.Remove(key);
                }
                return result;
            }
            catch (Exception ex)
            {
                // if the function to get the data ever fails, it will cache the exception. We need to remove the bad cached data so the next attempt will cache valid data
                Dictionary<string, object> logData = new Dictionary<string, object>{
                    { "key", key }
                };

                var logger = Log.Logger;
				logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
				logger = logger.ForContext("ProductModule", this.GetType());
                logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", propertyValue0: "GetFromCacheAsync", propertyValue1: $"Error: {ex.Message}");
                cache.Remove(key);
                return null;
            }
        }

        /// <summary>
        /// Removes the key from the cache
        /// </summary>
        /// <param name="cacheKey"></param>
        public static void RemoveFromCache(string cacheKey)
        {
            MemoryCache.Default.Remove(cacheKey);
        }

        /// <summary>
        /// 
        /// </summary>
        public void BustCache()
        {
            foreach (var element in MemoryCache.Default)
            {
                MemoryCache.Default.Remove(element.Key);
            }
        }
    }

    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        { }
        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(taskFactory).Unwrap())
        { }
    }
}
