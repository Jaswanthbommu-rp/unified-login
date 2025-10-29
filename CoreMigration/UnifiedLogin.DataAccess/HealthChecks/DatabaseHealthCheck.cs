using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnifiedLogin.DataAccess.Configuration;
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace UnifiedLogin.DataAccess.HealthChecks;

/// <summary>
/// Health check for database connectivity
/// </summary>
public sealed class DatabaseHealthCheck(
    IConnectionFactory connectionFactory,
    IOptions<DataAccessOptions> options,
    ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    private readonly DataAccessOptions _options = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            logger.LogDebug("Starting database health check");
            
            using var connection = connectionFactory.GetConnection(_options.ConnectionString) as SqlConnection;
            
            if (connection == null)
                throw new InvalidOperationException("Connection factory must return SqlConnection for health check");
            
            // Attempt to open connection
            await connection.OpenAsync(cancellationToken);
            
            // Simple connectivity test
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = _options.HealthCheckTimeoutSeconds;
            
            var result = await command.ExecuteScalarAsync(cancellationToken);
            
            stopwatch.Stop();
            
            if (result?.ToString() == "1")
            {
                logger.LogDebug("Database health check passed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                return HealthCheckResult.Healthy($"Database connection successful. Response time: {stopwatch.ElapsedMilliseconds}ms");
            }
            else
            {
                logger.LogWarning("Database health check returned unexpected result: {Result}", result);
                return HealthCheckResult.Degraded($"Database connection established but returned unexpected result: {result}");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Database health check was cancelled after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return HealthCheckResult.Unhealthy("Database health check was cancelled");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Database health check failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}", ex);
        }
    }
}