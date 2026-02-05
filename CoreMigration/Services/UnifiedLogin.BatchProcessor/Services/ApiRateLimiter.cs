using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Models;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Rate limiter for external API calls to prevent overwhelming downstream services.
/// Uses .NET's built-in RateLimiter for efficient concurrency control.
/// </summary>
public class ApiRateLimiter : IApiRateLimiter, IDisposable
{
    private readonly Dictionary<string, RateLimiter> _limiters = new();
    private readonly ILogger<ApiRateLimiter> _logger;

    public ApiRateLimiter(ILogger<ApiRateLimiter> logger, IOptions<RateLimitSettings> settings)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var rateLimitSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        // Create concurrency limiter for API calls
        _limiters["api"] = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = rateLimitSettings.ApiCallsPerSecond,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = rateLimitSettings.QueueLimit
        });

        // Create concurrency limiter for database operations
        _limiters["database"] = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = rateLimitSettings.DatabaseCallsPerSecond,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = rateLimitSettings.QueueLimit * 5 // Higher queue for DB
        });

        _logger.LogInformation("Rate limiters initialized: API={ApiLimit}/s, DB={DbLimit}/s, QueueLimit={QueueLimit}",
            rateLimitSettings.ApiCallsPerSecond,
            rateLimitSettings.DatabaseCallsPerSecond,
            rateLimitSettings.QueueLimit);
    }

    public async ValueTask<RateLimitLease> AcquireAsync(string resourceName, CancellationToken cancellationToken = default)
    {
        if (!_limiters.TryGetValue(resourceName, out var limiter))
        {
            _logger.LogWarning("No rate limiter configured for resource: {ResourceName}. Allowing request without limit.", resourceName);
            return new DummyLease();
        }

        return await limiter.AcquireAsync(permitCount: 1, cancellationToken);
    }

    public void Dispose()
    {
        foreach (var limiter in _limiters.Values)
        {
            limiter.Dispose();
        }
    }

    /// <summary>
    /// Dummy lease that always allows requests (used when limiter not configured)
    /// </summary>
    private class DummyLease : RateLimitLease
    {
        public override bool IsAcquired => true;

        public override IEnumerable<string> MetadataNames
        {
            get { yield break; }
        }

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing) { }
    }
}