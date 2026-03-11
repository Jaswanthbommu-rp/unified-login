using System.Data;
using Microsoft.Data.SqlClient;

namespace UnifiedLogin.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedLogin.BusinessLogic.Repository.Security;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        // Register repositories here
        services
            .AddScoped<IDbConnection>(p => new SqlConnection(config.GetConnectionString("DBConnection")));
        services.AddScoped<IPersonaRightRepository, PersonaRightRepository>();

        return services;
    }
}
