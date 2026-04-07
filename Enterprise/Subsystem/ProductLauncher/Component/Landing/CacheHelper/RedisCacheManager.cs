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
            var connectionStringSetting = ConfigurationManager.ConnectionStrings["RedisConnection"];
            if (connectionStringSetting == null || string.IsNullOrWhiteSpace(connectionStringSetting.ConnectionString))
            {
                return null;
            }

            string redisConnectionString = connectionStringSetting.ConnectionString;

            // Ensure abortConnect=false so the multiplexer retries instead of throwing immediately
            if (redisConnectionString.IndexOf("abortConnect", StringComparison.OrdinalIgnoreCase) < 0)
            {
                redisConnectionString += ",abortConnect=false";
            }

            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        public static ConnectionMultiplexer Connection => lazyConnection.Value;

        public static IDatabase Cache => Connection?.GetDatabase();
    }
}