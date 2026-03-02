using IdentityModel.Client;
using LaunchDarkly.Logging;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Data;
using UnifiedLogin.BatchProcessor.HealthChecks;
using UnifiedLogin.BatchProcessor.Models;
using UnifiedLogin.BatchProcessor.Repositories;
using UnifiedLogin.BatchProcessor.Services;

namespace UnifiedLogin.BatchProcessor.Configuration;

public static class ProgramExtensions
{
    public static IServiceCollection AddRequiredServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<BatchProcessorSettings>(config.GetSection(BatchProcessorSettings.SectionName));
        services.Configure<HybridCacheSettings>(config.GetSection(HybridCacheSettings.SectionName));
        services.Configure<RateLimitSettings>(config.GetSection(RateLimitSettings.SectionName));

        services.AddScoped<IBatchRepository, BatchRepository>();

        // Register LaunchDarkly client
        var sdkKey = config.GetValue<string>("LaunchDarkly:SdkKey");
        if (!string.IsNullOrEmpty(sdkKey))
        {
            services.AddSingleton<ILdClient>(p => new LdClient(sdkKey));
        }

        // Register feature flag service (singleton — ILdClient and IHybridCacheService are both singletons)
        services.AddSingleton<IFeatureFlagService, FeatureFlagService>();

        // Register API client with Polly resilience
        services.AddHttpClient<IProductApiClient, ProductApiClient>()
            .AddStandardResilienceHandler(options =>
            {
                // Per-attempt timeout: each individual attempt (including retries) must complete within this.
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15);

                // Configure retry policy.
                // 3 retries with exponential backoff: ~2s, ~4s, ~8s delays = 14s waiting total.
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;

                // Configure circuit breaker.
                // MinimumThroughput=10 suits sporadic batch traffic.
                // FailureRatio=0.1: open circuit after 10% failure rate to protect downstream sooner.
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.FailureRatio = 0.1;
                options.CircuitBreaker.MinimumThroughput = 10;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);

                // Total timeout must cover all attempts + retry delays.
                // Budget: 4 attempts × 15s + (2+4+8)s delays = 74s → rounded to 90s.
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);
            });

        // Register rate limiter
        services.AddSingleton<IApiRateLimiter, ApiRateLimiter>();

        // Register metrics
        services.AddSingleton<BatchProcessingMetrics>();

        // Register cache services
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
        var redisSettings = config.GetSection(HybridCacheSettings.SectionName).Get<HybridCacheSettings>();
        var redisConnectionString = config.GetConnectionString("RedisConnection");
        if (redisSettings?.Redis?.Enabled == true && !string.IsNullOrWhiteSpace(redisConnectionString))
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
        var redisConnectionString = config.GetConnectionString("RedisConnection");

        // Configure in-memory cache with size limit
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = cacheSettings.Memory.SizeLimitMB * 1024 * 1024; // Convert MB to bytes
            options.CompactionPercentage = cacheSettings.Memory.CompactionPercentage;
        });

        // Register Redis connection as singleton (optional, only if Redis is enabled).
        // An invalid connection string is a startup misconfiguration and will throw — fail fast is correct here.
        // Runtime Redis unavailability is handled gracefully by HybridCacheService.
        if (cacheSettings.Redis.Enabled && !string.IsNullOrWhiteSpace(redisConnectionString))
        {
            var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
            redisOptions.ConnectTimeout = cacheSettings.Redis.ConnectTimeout;
            redisOptions.SyncTimeout = cacheSettings.Redis.SyncTimeout;
            redisOptions.AbortOnConnectFail = cacheSettings.Redis.AbortOnConnectFail;
            redisOptions.AllowAdmin = false;
            redisOptions.ConnectRetry = cacheSettings.Redis.ConnectRetry;

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                try
                {
                    // ConnectAsync establishes the connection before returning.
                    // StackExchange.Redis reconnects automatically in the background if it drops.
                    logger.LogInformation("Connecting to Redis: {Endpoints}", redisOptions.EndPoints.First());
                    var connection = ConnectionMultiplexer.ConnectAsync(redisOptions).GetAwaiter().GetResult();
                    logger.LogInformation("Redis connection established successfully: {Endpoints}",
                        string.Join(", ", connection.GetEndPoints().Select(e => e.ToString())));
                    return connection;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to connect to Redis. Application will use in-memory cache only.");
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

        // Get settings to determine instance counts
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var settings = config.GetSection(BatchProcessorSettings.SectionName).Get<BatchProcessorSettings>()
            ?? new BatchProcessorSettings();

        // Register hosted services for batch jobs with configurable instance counts

        // Recurring jobs (run on interval)
        RegisterJobInstances<PendingBatchJob>(services, settings.PendingBatch.InstanceCount); //RunPendingProcess
        RegisterJobInstances<RetryBatchJob>(services, settings.RetryBatch.InstanceCount); //RunRetryProcess
        RegisterJobInstances<EnterpriseRolesJob>(services, settings.EnterpriseRoles.InstanceCount); //RunEnterpriseRoleUpdateProcess
        RegisterJobInstances<PrimaryPropertiesJob>(services, settings.PrimaryProperties.InstanceCount); //RunPrimaryPropertiesUpdateProcess
        RegisterJobInstances<BulkUserUpdateJob>(services, settings.BulkUserUpdate.InstanceCount); //RunBulkUserUpdateProcess
        RegisterJobInstances<CompanyAndPropertiesUpdateJob>(services, settings.CompanyAndPropertiesUpdate.InstanceCount); //RunCompanyAndPropertiesUpdateProcess

        RegisterJobInstances<FutureUserLoginsJob>(services, settings.FutureUserLogins.InstanceCount); //SendRegularUserNotification
        RegisterJobInstances<PendingUsersExpirationJob>(services, settings.PendingUsersExpiration.InstanceCount); //ProcessPendingUsers

        // Scheduled jobs (run once per day at specific time)
        RegisterJobInstances<DisableExpiredUsersJob>(services, settings.DisableExpiredUsers.InstanceCount); //ProcessDisableUsersinProducts

        return services;
    }

    /// <summary>
    /// Registers multiple instances of a hosted service based on the specified count.
    /// This allows running multiple parallel instances of the same job for better throughput.
    /// </summary>
    /// <typeparam name="TJob">The type of hosted service to register</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="instanceCount">Number of instances to register (default: 1)</param>
    private static void RegisterJobInstances<TJob>(IServiceCollection services, int instanceCount = 1)
        where TJob : class, IHostedService
    {
        for (int i = 0; i < instanceCount; i++)
        {
            services.AddHostedService<TJob>();
        }
    }
}

