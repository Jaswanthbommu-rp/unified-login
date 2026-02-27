namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Centralised LaunchDarkly feature flag key constants used across all batch processor jobs.
/// </summary>
public static class FeatureFlagKeys
{
    /// <summary>
    /// Controls whether the v2 batch processor logic runs.
    /// When false, all jobs skip their processing cycle.
    /// </summary>
    public const string BatchProcessorV2 = "use-core-api-v2-for-service";
}
