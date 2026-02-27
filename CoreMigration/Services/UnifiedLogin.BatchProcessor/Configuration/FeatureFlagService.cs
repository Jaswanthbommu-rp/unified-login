using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Evaluates LaunchDarkly boolean feature flags using an application-level anonymous context.
/// Evaluated values are cached for 30 minutes via IHybridCacheService to reduce LD traffic.
/// After expiry the value is re-fetched from LaunchDarkly and the cache is refreshed automatically.
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    // Single anonymous context representing the batch-processor application.
    // Consistent with the existing "user-company-association" flag pattern in the codebase.
    private static readonly Context _appContext = Context.Builder("batch-processor").Build();

    private const int CacheExpirationMinutes = 30;

    private readonly ILdClient _ldClient;
    private readonly IHybridCacheService _cache;
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(
        ILdClient ldClient,
        IHybridCacheService cache,
        ILogger<FeatureFlagService> logger)
    {
        _ldClient = ldClient ?? throw new ArgumentNullException(nameof(ldClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<bool> GetBoolFlagAsync(
        string flagKey,
        bool defaultValue = false,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"ld:flag:bool:{flagKey}";

        return _cache.GetOrCreateAsync(
            cacheKey,
            ct =>
            {
                try
                {
                    var value = _ldClient.BoolVariation(flagKey, _appContext, defaultValue);
                    _logger.LogDebug("LaunchDarkly flag '{FlagKey}' evaluated to {Value}. Cached for {Minutes} minutes.",
                        flagKey, value, CacheExpirationMinutes);
                    return Task.FromResult(value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to evaluate LaunchDarkly flag '{FlagKey}'. Returning default: {Default}.",
                        flagKey, defaultValue);
                    return Task.FromResult(defaultValue);
                }
            },
            CacheExpirationMinutes,
            cancellationToken);
    }
}
