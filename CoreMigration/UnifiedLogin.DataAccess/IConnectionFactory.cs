using System.Data;

namespace UnifiedLogin.DataAccess;

/// <summary>
/// Factory interface for creating database connections.
/// Provides abstraction over connection creation to support dependency injection and testability.
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Creates a new database connection using the specified connection string.
    /// </summary>
    /// <param name="connectionString">The database connection string to use for the connection</param>
    /// <returns>A new <see cref="IDbConnection"/> instance configured with the provided connection string</returns>
    /// <exception cref="ArgumentException">Thrown when connectionString is null, empty, or whitespace</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection cannot be created</exception>
    IDbConnection GetConnection(string connectionString);
    
    /// <summary>
    /// Creates a new database connection using the configured default connection string.
    /// Uses the connection string from application configuration (DataAccessOptions).
    /// </summary>
    /// <returns>A new <see cref="IDbConnection"/> instance configured with the default connection string</returns>
    /// <exception cref="InvalidOperationException">Thrown when no default connection string is configured or the connection cannot be created</exception>
    IDbConnection GetConnection();
}