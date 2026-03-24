namespace UnifiedLogin.Core;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Security;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        // Unkeyed — kept for legacy repositories that inject plain IDbConnection
        services.AddScoped<IDbConnection>(
            _ => new SqlConnection(config.GetConnectionString("DBConnection")));

        // Keyed "rw" — read-write connection (PersonaRepositoryAsync and other async repos)
        services.AddKeyedScoped<IDbConnection>("rw",
            (_, _) => new SqlConnection(config.GetConnectionString("DBConnection")));

        // Keyed "ro" — read-only replica; falls back to the same string if no replica is configured
        services.AddKeyedScoped<IDbConnection>("ro",
            (_, _) => new SqlConnection(
                config.GetConnectionString("DBConnectionReadOnly")
                ?? config.GetConnectionString("DBConnection")));

        // Register repositories here
        services.AddScoped<IPersonaRightRepository, PersonaRightRepository>();

        return services;
    }
}
