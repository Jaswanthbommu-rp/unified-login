using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Helper
{
    public class RedisCache : IDistributedCache
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _cache;

        public RedisCache(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _cache = _redis.GetDatabase();
        }

        public byte[] Get(string key)
        {
            return _cache.StringGet(key);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return await _cache.StringGetAsync(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var expiration = options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration;
            _cache.StringSet(key, value, expiration);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            var expiration = options.AbsoluteExpirationRelativeToNow ?? options.SlidingExpiration;
            await _cache.StringSetAsync(key, value, expiration);
        }

        public void Refresh(string key)
        {
            // Redis does not have a direct way to refresh, so we can re-set the value to refresh the expiration
            var value = _cache.StringGet(key);
            if (value.HasValue)
            {
                _cache.KeyExpire(key, _cache.KeyTimeToLive(key));
            }
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            var value = await _cache.StringGetAsync(key);
            if (value.HasValue)
            {
                await _cache.KeyExpireAsync(key, _cache.KeyTimeToLive(key));
            }
        }

        public void Remove(string key)
        {
            _cache.KeyDelete(key);
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await _cache.KeyDeleteAsync(key);
        }
    }
}