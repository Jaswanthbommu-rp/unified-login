namespace UnifiedLogin.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class WebApiExtensions
{
    public static IServiceCollection AddMvcCoreWithAddOns(this IServiceCollection services)
    {
        services
            .AddMvcCore(options =>
            {
                // Requires authenticated user with RequiredScope from appSettings.json
                options.AddBaseAuthorizationFilters();
            })
            .AddCors()
            .AddApiExplorer();

        return services;
    }
}
