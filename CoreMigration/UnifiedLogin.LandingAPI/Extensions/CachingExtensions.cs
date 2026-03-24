using Microsoft.Extensions.Caching.StackExchangeRedis;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
namespace UnifiedLogin.LandingAPI.Extensions;

internal static class CachingExtensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<INoCacheRepository, NoCacheRepository>();
        //var serializeOptions = new JsonSerializerOptions() { Converters = { new ClaimConverter() } };
        if (GetSettingBoolean(services, "FusionCacheEnabledDuende"))
        {
            services.AddSingleton<ICacheService, FusionCacheService>();
            var cacheDurationSetting = GetSettingString(services, "FusionCacheDurationDuende");
            var cacheDuration = 2;
            if (cacheDurationSetting != null)
            {
                cacheDuration = int.Parse(cacheDurationSetting);
            }
            if (GetSettingBoolean(services, "FusionCacheWithRedisEnabledDuende"))
            {
                services.AddFusionCache()
                    .WithOptions(options =>
                    {
                        options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(2);
                        // CUSTOM LOG LEVELS
                        options.FailSafeActivationLogLevel = LogLevel.Debug;
                        options.SerializationErrorsLogLevel = LogLevel.Warning;
                        options.DistributedCacheSyntheticTimeoutsLogLevel = LogLevel.Debug;
                        options.DistributedCacheErrorsLogLevel = LogLevel.Error;
                        options.FactorySyntheticTimeoutsLogLevel = LogLevel.Debug;
                        options.FactoryErrorsLogLevel = LogLevel.Error;
                    })
                    .WithDefaultEntryOptions(new FusionCacheEntryOptions()
                    {
                        Duration = TimeSpan.FromMinutes(cacheDuration)
                    })
                    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                    .WithDistributedCache(new RedisCache(new RedisCacheOptions()
                    {
                        Configuration = config.GetConnectionString("RedisConnection"),
                        InstanceName = "Duende_" + config.GetValue<string>("Logging:Environment")
                    }))
                    .WithBackplane(new RedisBackplane(
                        new RedisBackplaneOptions
                        {
                            Configuration = config.GetConnectionString("RedisConnection")
                        }));
            }
            else
            {
                services.AddFusionCache()
                    .WithDefaultEntryOptions(new FusionCacheEntryOptions()
                    {
                        Duration = TimeSpan.FromMinutes(cacheDuration)
                    })
                    .WithSerializer(new FusionCacheSystemTextJsonSerializer());
            }
        }
        else
        {
            if (!GetSettingBoolean(services, "CacheDisabledDuende"))
            {
                services.AddSingleton<ICacheService, MemoryCacheService>()
                    .AddMemoryCache();
            }
            else
            {
                services.AddSingleton<ICacheService, NoOpCacheService>();
            }
        }
        return services;
    }
    private static bool GetSettingBoolean(IServiceCollection services, string settingType)
    {
        var repo = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<INoCacheRepository>();
        var productInternalSettingList = repo.GetProductInternalSettings(3);
        var setting = productInternalSettingList.FirstOrDefault(p => p.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase))?.Value;
        return setting is not null && setting.Equals("1");
    }
    private static string GetSettingString(IServiceCollection services, string settingType)
    {
        var repo = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<INoCacheRepository>();
        var productInternalSettingList = repo.GetProductInternalSettings(3);
        return productInternalSettingList.FirstOrDefault(p => p.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}