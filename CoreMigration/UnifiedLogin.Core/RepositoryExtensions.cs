namespace UnifiedLogin.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedLogin.BusinessLogic.Repository.Security;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        // Register repositories here
        services.AddScoped<IPersonaRightRepository, PersonaRightRepository>();

        return services;
    }
}
