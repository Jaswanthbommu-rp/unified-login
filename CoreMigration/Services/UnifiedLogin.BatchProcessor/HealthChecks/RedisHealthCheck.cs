using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace UnifiedLogin.BatchProcessor.HealthChecks;

/// <summary>
/// Health check for Redis cache connectivity.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(
        IConnectionMultiplexer redis,
        ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis == null || !_redis.IsConnected)
            {
                _logger.LogWarning("Redis connection is not available");
                return HealthCheckResult.Degraded(
                    "Redis connection is not available. Service is running with in-memory cache only.");
            }

            // Try to ping Redis
            var pingResult = await _redis.GetDatabase().PingAsync();

            _logger.LogDebug("Redis health check passed. Ping: {PingTime}ms", pingResult.TotalMilliseconds);

            var data = new Dictionary<string, object>
            {
                { "pingMs", pingResult.TotalMilliseconds },
                { "endpoints", string.Join(", ", _redis.GetEndPoints().Select(e => e.ToString())) },
                { "isConnected", _redis.IsConnected }
            };

            return HealthCheckResult.Healthy(
                $"Redis connection is healthy (ping: {pingResult.TotalMilliseconds}ms)",
                data);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis health check failed - connection error");
            return HealthCheckResult.Degraded(
                $"Redis connection failed: {ex.Message}. Service is running with in-memory cache only.",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Degraded(
                $"Redis health check failed: {ex.Message}. Service is running with in-memory cache only.",
                ex);
        }
    }
}
