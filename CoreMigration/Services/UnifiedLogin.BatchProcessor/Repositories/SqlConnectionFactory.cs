using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace UnifiedLogin.BatchProcessor.Repositories;

/// <summary>
/// Creates a new SqlConnection per call using the "DBConnection" connection string.
/// Registered as a singleton — the connection string is read once at startup.
/// Each created connection should be disposed by the caller (use 'await using') so
/// ADO.NET's connection pool can reclaim the underlying socket promptly.
/// </summary>
public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DBConnection")
            ?? throw new InvalidOperationException("Connection string 'DBConnection' is not configured.");
    }

    public SqlConnection CreateConnection() => new(_connectionString);
}
