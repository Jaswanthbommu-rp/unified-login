using System.Threading.RateLimiting;

namespace UnifiedLogin.BatchProcessor.Services;

/// <summary>
/// Interface for API rate limiting to prevent overwhelming downstream services.
/// </summary>
public interface IApiRateLimiter
{
    /// <summary>
    /// Acquires a rate limit lease for the specified resource.
    /// </summary>
    /// <param name="resourceName">Name of the resource to rate limit (e.g., "api", "database")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rate limit lease that must be disposed after use</returns>
    ValueTask<RateLimitLease> AcquireAsync(string resourceName, CancellationToken cancellationToken = default);
}