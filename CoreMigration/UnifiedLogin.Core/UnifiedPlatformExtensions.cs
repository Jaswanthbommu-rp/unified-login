using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UnifiedLogin.Core;

public static class UnifiedPlatformExtensions
{
    public static IServiceCollection AddUnifiedPlatformAuthentication(this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.Authority = config.GetValue<string>("UnifiedPlatform:Authority");
                options.Audience = config.GetValue<string>("UnifiedPlatform:ApiName");
            });

        return services;
    }
}