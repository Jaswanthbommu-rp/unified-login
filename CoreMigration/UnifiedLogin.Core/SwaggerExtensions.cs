using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace UnifiedLogin.Core;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IConfiguration config,
        IApiVersionDescriptionProvider provider, string routeString)
    {
        var env = config.GetValue<string>("Logging:Environment");
        if (env == "PROD")
        {
            return app;
        }

        var clientId = config.GetValue<string>("UnifiedPlatform:SwaggerClientId");
        app
            .UseSwagger()
            .UseSwaggerUI(options =>
            {
                foreach (var groupName in provider.ApiVersionDescriptions
                             .Select(vd => vd.GroupName))
                {
                    options.SwaggerEndpoint($"/swagger/{groupName}/swagger.json",
                        $"UnifiedLogin Landing API {groupName.ToUpperInvariant()}");
                    //options.RoutePrefix = string.Empty;
                }
                options.RoutePrefix = routeString;
                options.DocumentTitle = "UnifiedLogin_LandingAPI Documentation";
                options.OAuthClientId(clientId);
                options.OAuthAppName("UnifiedLogin_LandingAPI");
                options.OAuthUsePkce();
                options.DocExpansion(DocExpansion.None);
            });

        return app;
    }
}

public class ConfigureSwaggerOptions(IConfiguration config, IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        var disco = GetDiscoveryDocument();

        var apiScope = config.GetValue<string>("UnifiedPlatform:ApiName");
        var scopes = apiScope!.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        var additionalScopes = config.GetValue<string>("UnifiedPlatform:AdditionalScopes");
        scopes.AddRange(additionalScopes!.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList());

        var oauthScopeDic = scopes.ToDictionary(scope => scope, scope => $"Resource access: {scope}");

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = $"UnifiedLogin_LandingAPI {description.ApiVersion}",
                    Version = description.ApiVersion.ToString(),
                });
        }

        options.EnableAnnotations();

        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(disco.AuthorizeEndpoint!),
                    TokenUrl = new Uri(disco.TokenEndpoint!),
                    Scopes = oauthScopeDic
                }
            }
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                },
                oauthScopeDic.Keys.ToArray()
            }
        });
    }

    private DiscoveryDocumentResponse GetDiscoveryDocument()
    {
        var client = new HttpClient();
        var authority = config.GetValue<string>("UnifiedPlatform:Authority");
        return client.GetDiscoveryDocumentAsync(authority)
            .GetAwaiter()
            .GetResult();
    }
}

