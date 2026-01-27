using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RealPage.DataAccess.Dapper;

namespace UnifiedLogin.BatchProcessor.HealthChecks;

/// <summary>
/// Health check for SQL Server database connectivity.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly SqlConnection _sql;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        [FromKeyedServices("DBConnection")] SqlConnection sql,
        ILogger<DatabaseHealthCheck> logger)
    {
        _sql = sql;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Open connection if not already open
            if (_sql.State != System.Data.ConnectionState.Open)
            {
                await _sql.OpenAsync(cancellationToken);
            }

            // Execute a simple query to verify database connectivity
            using var command = _sql.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 5; // 5 second timeout

            await command.ExecuteScalarAsync(cancellationToken);

            _logger.LogDebug("Database health check passed");

            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database health check failed with SQL error");
            return HealthCheckResult.Unhealthy(
                $"Database connection failed: {ex.Message}",
                ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy(
                $"Database health check failed: {ex.Message}",
                ex);
        }
    }
}
