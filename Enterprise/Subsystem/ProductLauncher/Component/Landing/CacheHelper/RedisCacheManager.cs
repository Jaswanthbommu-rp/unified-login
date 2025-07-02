using StackExchange.Redis;
using System;
using System.Configuration;
using System.Threading;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.CacheHelper
{
    public class RedisCacheManager
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string redisConnectionString = ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString;
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        public static ConnectionMultiplexer Connection => lazyConnection.Value;

        public static IDatabase Cache => Connection.GetDatabase();
    }
}
