namespace UnifiedLogin.BatchProcessor.Configuration;

public interface IFeatureFlagService
{
    /// <summary>
    /// Evaluates a boolean LaunchDarkly feature flag, returning a cached value for up to 30 minutes.
    /// </summary>
    Task<bool> GetBoolFlagAsync(string flagKey, bool defaultValue = false, CancellationToken cancellationToken = default);
}
