using IdentityModel.Client;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Data;
using UnifiedLogin.BatchProcessor.HealthChecks;
using UnifiedLogin.BatchProcessor.Repositories;
using UnifiedLogin.BatchProcessor.Services;

namespace UnifiedLogin.BatchProcessor.Configuration;

public static class ProgramExtensions
{
    public static IServiceCollection AddRequiredServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<BatchProcessorSettings>(config.GetSection(BatchProcessorSettings.SectionName));
        services.Configure<HybridCacheSettings>(config.GetSection(HybridCacheSettings.SectionName));

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<IProductApiClient, ProductApiClient>();

        services.AddHybridCacheServices(config);

        services.AddAccessTokenManagement(options =>
        {
            options.Client.Clients.Add("unifiedlogin", new ClientCredentialsTokenRequest
            {
                Address = $"{config["UnifiedPlatform:Authority"]}/connect/token",
                ClientId = config["UnifiedPlatform:ClientId"],
                ClientSecret = config["UnifiedPlatform:ClientSecret"],
                Scope = config["UnifiedPlatform:Scope"]
            });
        });

        services.AddClientAccessTokenHttpClient("apicaller",
            configureClient: client => { client.BaseAddress = new Uri(config["ApiBaseUrl"]); });

        // Add health checks
        services.AddHealthCheckServices(config);

        return services;
    }

    /// <summary>
    /// Registers health check services for monitoring application health.
    /// </summary>
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration config)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Add database health check
        healthChecksBuilder.AddCheck<DatabaseHealthCheck>(
            "database",
            tags: new[] { "ready", "db" });

        // Add Redis health check (only if enabled)
        var cacheSettings = config.GetSection(HybridCacheSettings.SectionName).Get<HybridCacheSettings>();
        if (cacheSettings?.Redis?.Enabled == true && !string.IsNullOrWhiteSpace(cacheSettings.Redis.ConnectionString))
        {
            healthChecksBuilder.AddCheck<RedisHealthCheck>(
                "redis",
                tags: new[] { "ready", "cache" });
        }

        // Add background service health check
        healthChecksBuilder.AddCheck<BackgroundServiceHealthCheck>(
            "background-services",
            tags: new[] { "ready", "services" });

        return services;
    }

    /// <summary>
    /// Registers HybridCache with Redis as primary and in-memory as fallback.
    /// </summary>
    private static IServiceCollection AddHybridCacheServices(this IServiceCollection services, IConfiguration config)
    {
        var cacheSettings = config.GetSection(HybridCacheSettings.SectionName).Get<HybridCacheSettings>()
            ?? new HybridCacheSettings();

        // Configure in-memory cache with size limit
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = cacheSettings.Memory.SizeLimitMB * 1024 * 1024; // Convert MB to bytes
            options.CompactionPercentage = cacheSettings.Memory.CompactionPercentage;
        });

        // Register Redis connection as singleton (optional, only if Redis is enabled)
        if (cacheSettings.Redis.Enabled && !string.IsNullOrWhiteSpace(cacheSettings.Redis.ConnectionString))
        {
            try
            {
                var redisOptions = ConfigurationOptions.Parse(cacheSettings.Redis.ConnectionString);
                redisOptions.ConnectTimeout = cacheSettings.Redis.ConnectTimeout;
                redisOptions.SyncTimeout = cacheSettings.Redis.SyncTimeout;
                redisOptions.AbortOnConnectFail = cacheSettings.Redis.AbortOnConnectFail;
                redisOptions.AllowAdmin = false;
                redisOptions.ConnectRetry = cacheSettings.Redis.ConnectRetry;

                // Register IConnectionMultiplexer as singleton (nullable to handle connection failures)
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                    try
                    {
                        var connection = ConnectionMultiplexer.Connect(redisOptions);
                        logger.LogInformation("Redis connection established successfully: {Endpoints}",
                            string.Join(", ", connection.GetEndPoints().Select(e => e.ToString())));
                        return connection;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to connect to Redis. Application will use in-memory cache only.");
                        // Return a null connection - the service will handle this gracefully
                        throw;
                    }
                });

                // Add Redis distributed cache for HybridCache serialization layer
                services.AddStackExchangeRedisCache(options =>
                {
                    options.ConfigurationOptions = redisOptions;
                    options.InstanceName = "UnifiedLogin:BatchProcessor:";
                });
            }
            catch (Exception ex)
            {
                // Log error but continue - service will work with in-memory only
                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<IConnectionMultiplexer>>();
                logger?.LogWarning(ex, "Redis configuration failed. Continuing with in-memory cache only.");
            }
        }

        // Register HybridCache with default options
        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024; // 1 MB max payload
            options.MaximumKeyLength = 512; // Max key length
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(cacheSettings.DefaultOptions.AbsoluteExpirationMinutes),
                LocalCacheExpiration = TimeSpan.FromMinutes(cacheSettings.DefaultOptions.SlidingExpirationMinutes ?? 15)
            };
        });

        // Register the resilient cache service
        services.AddSingleton<IHybridCacheService, HybridCacheService>();

        return services;
    }

    public static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        // Register health check web server
        services.AddHostedService<HealthCheckWebServer>();

        // Register hosted services for batch jobs

        // Recurring jobs (run on interval)
        //services.AddHostedService<PendingBatchJob>(); //RunPendingProcess
        //services.AddHostedService<RetryBatchJob>(); //RunRetryProcess
        //services.AddHostedService<EnterpriseRolesJob>(); //RunEnterpriseRoleUpdateProcess
        //services.AddHostedService<PrimaryPropertiesJob>(); //RunPrimaryPropertiesUpdateProcess
        //services.AddHostedService<BulkUserUpdateJob>(); //RunBulkUserUpdateProcess
        //services.AddHostedService<CompanyAndPropertiesUpdateJob>(); //RunCompanyAndPropertiesUpdateProcess

        //services.AddHostedService<FutureUserLoginsJob>(); //SendRegularUserNotification
        //services.AddHostedService<PendingUsersExpirationJob>(); //ProcessPendingUsers

        //// Scheduled jobs (run once per day at specific time)
        services.AddHostedService<DisableExpiredUsersJob>(); //ProcessDisableUsersinProducts
        ////services.AddHostedService<UserActivationJob>();

        return services;
    }
}

