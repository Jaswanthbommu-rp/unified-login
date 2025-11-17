using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace UnifiedLogin.Core;

public static class UnifiedPlatformExtensions
{
    public static IServiceCollection AddUnifiedPlatformAuthentication(this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = config.GetValue<string>("UnifiedPlatform:Authority");
                options.RequireHttpsMetadata = false; // Set to true in production

                // Remove single Audience assignment; support multiple space-delimited resource audiences
                var apiNamesRaw = config.GetValue<string>("UnifiedPlatform:ApiName") ?? string.Empty;
                var audiences = apiNamesRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Configure token validation to accept any of the listed audiences
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudiences = audiences,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // Enable detailed logging for debugging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"OnChallenge error: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}