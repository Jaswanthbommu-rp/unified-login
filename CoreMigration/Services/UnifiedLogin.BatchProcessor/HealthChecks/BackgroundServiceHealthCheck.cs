using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using UnifiedLogin.BatchProcessor.Configuration;

namespace UnifiedLogin.BatchProcessor.HealthChecks;

/// <summary>
/// Health check for monitoring background service status.
/// Tracks which services are enabled and running.
/// </summary>
public class BackgroundServiceHealthCheck : IHealthCheck
{
    private readonly BatchProcessorSettings _settings;
    private readonly ILogger<BackgroundServiceHealthCheck> _logger;

    public BackgroundServiceHealthCheck(
        IOptions<BatchProcessorSettings> settings,
        ILogger<BackgroundServiceHealthCheck> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var enabledServices = new List<string>();
            var disabledServices = new List<string>();

            // Check each service configuration
            CheckService("PendingBatch", _settings.PendingBatch.Enabled, enabledServices, disabledServices);
            CheckService("RetryBatch", _settings.RetryBatch.Enabled, enabledServices, disabledServices);
            CheckService("PrimaryProperties", _settings.PrimaryProperties.Enabled, enabledServices, disabledServices);
            CheckService("EnterpriseRoles", _settings.EnterpriseRoles.Enabled, enabledServices, disabledServices);
            CheckService("BulkUserUpdate", _settings.BulkUserUpdate.Enabled, enabledServices, disabledServices);
            CheckService("CompanyAndPropertiesUpdate", _settings.CompanyAndPropertiesUpdate.Enabled, enabledServices, disabledServices);
            CheckService("UserActivation", _settings.UserActivation.Enabled, enabledServices, disabledServices);
            CheckService("FutureUserLogins", _settings.FutureUserLogins.Enabled, enabledServices, disabledServices);
            CheckService("PendingUsersExpiration", _settings.PendingUsersExpiration.Enabled, enabledServices, disabledServices);
            CheckService("DisableExpiredUsers", _settings.DisableExpiredUsers.Enabled, enabledServices, disabledServices);

            var data = new Dictionary<string, object>
            {
                { "enabledServicesCount", enabledServices.Count },
                { "disabledServicesCount", disabledServices.Count },
                { "enabledServices", string.Join(", ", enabledServices) },
                { "disabledServices", string.Join(", ", disabledServices) }
            };

            if (enabledServices.Count == 0)
            {
                _logger.LogWarning("No background services are enabled");
                return Task.FromResult(HealthCheckResult.Degraded(
                    "No background services are enabled",
                    data: data));
            }

            _logger.LogDebug("Background services health check passed. Enabled: {Count}", enabledServices.Count);

            return Task.FromResult(HealthCheckResult.Healthy(
                $"{enabledServices.Count} background services are enabled and running",
                data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background services health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Background services health check failed: {ex.Message}",
                ex));
        }
    }

    private static void CheckService(string serviceName, bool enabled, List<string> enabledServices, List<string> disabledServices)
    {
        if (enabled)
        {
            enabledServices.Add(serviceName);
        }
        else
        {
            disabledServices.Add(serviceName);
        }
    }
}
