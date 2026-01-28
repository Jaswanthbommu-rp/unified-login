using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UnifiedLogin.LandingAPIEnterprise.Configuration
{
    /// <summary>
    /// Extension methods for configuring health checks
    /// </summary>
    public static class HealthCheckConfiguration
    {
        /// <summary>
        /// Adds health checks for the application
        /// </summary>
        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            // Add database health check
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                healthChecksBuilder.AddSqlServer(
                    connectionString,
                    name: "database",
                    tags: new[] { "db", "sql", "ready" },
                    timeout: TimeSpan.FromSeconds(5));
            }

            // Add custom health checks
            healthChecksBuilder.AddCheck<ApiHealthCheck>("api-health", tags: new[] { "ready" });

            return services;
        }

        /// <summary>
        /// Maps health check endpoints
        /// </summary>
        public static IEndpointRouteBuilder MapCustomHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            // Liveness probe - Is the app running?
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false, // Don't run any checks, just return 200 if app is running
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"status\":\"Alive\",\"timestamp\":\"" + DateTime.UtcNow.ToString("o") + "\"}");
                }
            });

            // Readiness probe - Is the app ready to accept traffic?
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        timestamp = DateTime.UtcNow.ToString("o"),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            duration = e.Value.Duration.TotalMilliseconds,
                            description = e.Value.Description,
                            exception = e.Value.Exception?.Message
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });

            // Startup probe - Has the app finished starting?
            endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"status\":\"Started\",\"timestamp\":\"" + DateTime.UtcNow.ToString("o") + "\"}");
                }
            });

            // Detailed health check (for monitoring/debugging)
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        timestamp = DateTime.UtcNow.ToString("o"),
                        duration = report.TotalDuration.TotalMilliseconds,
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            duration = e.Value.Duration.TotalMilliseconds,
                            description = e.Value.Description,
                            data = e.Value.Data,
                            exception = e.Value.Exception?.Message,
                            tags = e.Value.Tags
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });

            return endpoints;
        }
    }

    /// <summary>
    /// Custom health check for API-specific checks
    /// </summary>
    public class ApiHealthCheck : IHealthCheck
    {
        private readonly ILogger<ApiHealthCheck> _logger;

        public ApiHealthCheck(ILogger<ApiHealthCheck> logger)
        {
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Add your custom health check logic here
                // For example: check external service availability, cache status, etc.

                var isHealthy = true; // Replace with actual health check logic

                if (isHealthy)
                {
                    return Task.FromResult(
                        HealthCheckResult.Healthy("API is healthy",
                            new Dictionary<string, object>
                            {
                                { "timestamp", DateTime.UtcNow }
                            }));
                }

                return Task.FromResult(
                    HealthCheckResult.Unhealthy("API is unhealthy"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return Task.FromResult(
                    HealthCheckResult.Unhealthy("An error occurred during health check", ex));
            }
        }
    }
}
