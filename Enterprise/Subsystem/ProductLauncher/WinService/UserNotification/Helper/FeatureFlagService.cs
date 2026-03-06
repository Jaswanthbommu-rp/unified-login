using LaunchDarkly.Sdk.Server;
using Serilog;
using System;
using System.Configuration;
using System.Runtime.Caching;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Helper
{
    /// <summary>
    /// Evaluates LaunchDarkly boolean feature flags using an application-level anonymous context.
    /// Evaluated values are cached for 5 minutes via MemoryCache to reduce LD traffic.
    /// After expiry the value is re-fetched from LaunchDarkly and the cache is refreshed automatically.
    /// </summary>
    public class FeatureFlagService : IDisposable
    {
        // Single anonymous context representing the user-notification application.
        private static readonly LaunchDarkly.Sdk.User _appUser = LaunchDarkly.Sdk.User.WithKey("use-core-api-v2-for-service");

        private const int CacheExpirationMinutes = 5;

        private readonly LdClient _ldClient;
        private readonly MemoryCache _cache = MemoryCache.Default;

        public FeatureFlagService()
        {
            try
            {
            var sdkKey = ConfigurationManager.AppSettings["LaunchDarklySdkKey"] ?? string.Empty;
            _ldClient = new LdClient(LaunchDarkly.Sdk.Server.Configuration.Default(sdkKey));
        }
            catch (Exception ex)
            {
                Log.Warning(ex, "{ActionName} - {state}",
                    new object[] { "FeatureFlagService", "Failed to initialize LaunchDarkly client. Feature flags will return default values." });
            }
        }

        /// <summary>
        /// Evaluates a boolean LaunchDarkly feature flag.
        /// The result is cached for <see cref="CacheExpirationMinutes"/> minutes.
        /// Returns <paramref name="defaultValue"/> if the flag cannot be evaluated.
        /// </summary>
        public bool GetBoolFlag(string flagKey, bool defaultValue = false)
        {
            if (_ldClient == null)
            {
                return defaultValue;
            }

            var cacheKey = $"ld:flag:bool:{flagKey}";

            if (_cache[cacheKey] is bool cachedValue)
            {
                Log.Debug("{ActionName} - {state}",
                    new object[] { "GetBoolFlag", $"LaunchDarkly flag '{flagKey}' returned {cachedValue} from cache." });
                return cachedValue;
            }

            try
            {
                var value = _ldClient.BoolVariation(flagKey, _appUser, defaultValue);

                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(CacheExpirationMinutes)
                };
                _cache.Set(cacheKey, value, policy);

                Log.Debug("{ActionName} - {state}",
                    new object[] { "GetBoolFlag", $"LaunchDarkly flag '{flagKey}' evaluated to {value}. Cached for {CacheExpirationMinutes} minutes." });

                return value;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "{ActionName} - {state}",
                    new object[] { "GetBoolFlag", $"Failed to evaluate LaunchDarkly flag '{flagKey}'. Returning default: {defaultValue}." });
                return defaultValue;
            }
        }

        public void Dispose()
        {
            _ldClient?.Dispose();
        }
    }
}
