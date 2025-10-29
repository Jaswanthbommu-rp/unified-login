using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnifiedLogin.DataAccess.Configuration;

namespace UnifiedLogin.DataAccess;

/// <summary>
/// Modern SQL Server connection factory implementation with comprehensive logging and configuration support.
/// Uses Microsoft.Data.SqlClient for optimal performance and security.
/// Supports both explicit connection strings and configuration-based connection management.
/// </summary>
public sealed class ConnectionFactory(
    IOptions<DataAccessOptions> options,
    ILogger<ConnectionFactory> logger) : IConnectionFactory
{
    #region Private Fields

    private readonly DataAccessOptions _options = options.Value;

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates a new SQL Server connection using the specified connection string.
    /// Includes enhanced error handling and diagnostic logging for troubleshooting.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string to use</param>
    /// <returns>A new <see cref="SqlConnection"/> instance ready for use</returns>
    /// <exception cref="ArgumentException">Thrown when connectionString is null, empty, or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection cannot be created</exception>
    /// <exception cref="SqlException">Thrown when there are SQL Server-specific connection errors</exception>
    public IDbConnection GetConnection(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));
        
        try
        {
            logger.LogDebug("Creating new SQL Server connection with connection string length: {Length}", 
                connectionString.Length);
            
            var connection = new SqlConnection(connectionString);
            
            logger.LogDebug("Successfully created SQL Server connection instance");
            
            // Note: ConnectionTimeout is read-only for SqlConnection and cannot be modified after creation.
            // Individual command timeouts should be set per SqlCommand, not per connection.
            // The connection timeout is controlled by the "Connection Timeout" parameter in the connection string.
            
            return connection;
        }
        catch (ArgumentException)
        {
            logger.LogError("Invalid connection string format provided");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create SQL Server connection. Connection string length: {Length}", 
                connectionString?.Length ?? 0);
            throw new InvalidOperationException("Unable to create database connection. Please check the connection string and database availability.", ex);
        }
    }

    /// <summary>
    /// Creates a new SQL Server connection using the configured default connection string.
    /// The connection string is retrieved from the application's DataAccessOptions configuration.
    /// </summary>
    /// <returns>A new <see cref="SqlConnection"/> instance configured with the default connection string</returns>
    /// <exception cref="InvalidOperationException">Thrown when no default connection string is configured</exception>
    /// <exception cref="SqlException">Thrown when there are SQL Server-specific connection errors</exception>
    public IDbConnection GetConnection()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            const string errorMessage = "No default connection string configured. Please check DataAccessOptions.ConnectionString in application configuration.";
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        logger.LogDebug("Creating connection using configured default connection string");
        return GetConnection(_options.ConnectionString);
    }

    #endregion
}