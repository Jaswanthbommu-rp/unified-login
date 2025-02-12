using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Helper
{
    public class RedisCache : ObjectCache
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _cache;

        public RedisCache(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _cache = _redis.GetDatabase();
        }

        public override object Get(string key, string regionName = null)
        {
            var value = _cache.StringGet(key);
            return value.HasValue ? value.ToString() : null;
        }

        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            _cache.StringSet(key, value.ToString(), policy.AbsoluteExpiration - DateTimeOffset.Now);
        }

        public override object Remove(string key, string regionName = null)
        {
            var value = _cache.StringGet(key);
            _cache.KeyDelete(key);
            return value.HasValue ? value.ToString() : null;
        }

        public override CacheItem AddOrGetExisting(CacheItem value, CacheItemPolicy policy)
        {
            var existingValue = _cache.StringGet(value.Key);
            if (existingValue.HasValue)
            {
                return new CacheItem(value.Key, existingValue.ToString());
            }
            Set(value.Key, value.Value, policy);
            return value;
        }

        public override object AddOrGetExisting(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            var existingValue = _cache.StringGet(key);
            if (existingValue.HasValue)
            {
                return existingValue.ToString();
            }
            Set(key, value, policy, regionName);
            return value;
        }

        public override object AddOrGetExisting(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            var existingValue = _cache.StringGet(key);
            if (existingValue.HasValue)
            {
                return existingValue.ToString();
            }
            _cache.StringSet(key, value.ToString(), absoluteExpiration - DateTimeOffset.Now);
            return value;
        }

        public override bool Contains(string key, string regionName = null)
        {
            return _cache.KeyExists(key);
        }

        public override CacheEntryChangeMonitor CreateCacheEntryChangeMonitor(IEnumerable<string> keys, string regionName = null)
        {
            throw new NotImplementedException();
        }

        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get { return DefaultCacheCapabilities.AbsoluteExpirations | DefaultCacheCapabilities.SlidingExpirations; }
        }

        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            var value = _cache.StringGet(key);
            return value.HasValue ? new CacheItem(key, value.ToString()) : null;
        }

        public override long GetCount(string regionName = null)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> GetValues(IEnumerable<string> keys, string regionName = null)
        {
            var result = new Dictionary<string, object>();
            foreach (var key in keys)
            {
                var value = _cache.StringGet(key);
                if (value.HasValue)
                {
                    result[key] = value.ToString();
                }
            }
            return result;
        }

        public override string Name
        {
            get { return "RedisCache"; }
        }

        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            Set(item.Key, item.Value, policy);
        }

        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            _cache.StringSet(key, value.ToString(), absoluteExpiration - DateTimeOffset.Now);
        }

        public override object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value, new CacheItemPolicy()); }
        }
    }

}

