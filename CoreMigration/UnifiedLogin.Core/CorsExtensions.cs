using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UnifiedLogin.Core;

public static class CorsExtensions
{
    private const string PolicyName = "AllowedOrigins";

    public static IServiceCollection AddConfiguredCors(this IServiceCollection services, IConfiguration configuration)
    {
        var originsValue = configuration.GetValue<string>("AllCORSOrigins");
        var origins = originsValue?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (origins is { Length: > 0 })
                {
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });
        return services;
    }
}
