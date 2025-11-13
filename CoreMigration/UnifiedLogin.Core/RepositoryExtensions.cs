namespace UnifiedLogin.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
    {
        // Register repositories here

        return services;
    }
}
