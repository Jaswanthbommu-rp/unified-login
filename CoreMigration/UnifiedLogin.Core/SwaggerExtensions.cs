using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
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
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(type => type.ToString());
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IConfiguration config)
    {
        var env = config.GetValue<string>("Logging:Environment");
        var clientId = config.GetValue<string>("UnifiedPlatform:SwaggerClientId");
        if (env == "PROD" || env == "EUPROD" || env == "DEMO" || env == "TRAINING")
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.DocumentTitle = "UnifiedLogin Landing API";
            o.OAuthClientId(clientId);
            o.OAuthAppName("UnifiedLoginLandingAPI");
            o.OAuthUsePkce();
            o.DocExpansion(DocExpansion.None);
            o.RoutePrefix = string.Empty;
            o.SwaggerEndpoint("/swagger/v1/swagger.json", "ActivityLog Api Doc");
        });

        return app;
    }
}

public class ConfigureSwaggerOptions(IConfiguration config) : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        var disco = GetDiscoveryDocument();

        var apiScope = config.GetValue<string>("UnifiedPlatform:RequiredScopes");
        var scopes = apiScope!.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        //var additionalScopes = config.GetValue<string>("UnifiedPlatform:AdditionalScopes");
        //scopes.AddRange(additionalScopes!.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList());

        var oauthScopeDic = scopes.ToDictionary(scope => scope, scope => $"Resource access: {scope}");

       // options.EnableAnnotations(); //TODO

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

