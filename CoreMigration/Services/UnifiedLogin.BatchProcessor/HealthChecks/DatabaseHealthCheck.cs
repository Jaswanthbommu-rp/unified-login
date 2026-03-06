using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UnifiedLogin.BatchProcessor.Repositories;

namespace UnifiedLogin.BatchProcessor.HealthChecks;

/// <summary>
/// Health check for SQL Server database connectivity.
/// Creates a fresh connection per check via IDbConnectionFactory so it is
/// returned to the ADO.NET pool immediately after the probe completes.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnectionFactory connectionFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 5;

            await command.ExecuteScalarAsync(cancellationToken);

            _logger.LogDebug("Database health check passed");
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database health check failed with SQL error");
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}", ex);
        }
    }
}
