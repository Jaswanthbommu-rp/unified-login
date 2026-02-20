using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using UnifiedLogin.Core.Filters;

namespace UnifiedLogin.Core;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            options.UseInlineDefinitionsForEnums();
        });
        services.AddHttpClient(); // Register IHttpClientFactory for proper HTTP client management

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(
        this IApplicationBuilder app,
        IConfiguration config,
        string appName,
        string basePath)
    {
        var env = config.GetValue<string>("Logging:Environment");
        if (env == "PROD")
        {
            return app;
        }

        var clientId = config.GetValue<string>("UnifiedPlatform:SwaggerClientId");
        
        app
            .UseSwagger(c =>
            {
                // Configure Swagger to handle reverse proxy scenarios
                c.PreSerializeFilters.Add((swagger, httpReq) =>
                {
                    // Force HTTPS for non-localhost environments
                    var scheme = httpReq.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                        ? httpReq.Scheme
                        : "https";

                    // Dont append basepath for localhost to avoid issues
                    var pathBase = httpReq.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase) ? "" : basePath;

                    swagger.Servers = new List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{scheme}://{httpReq.Host.Value}/{pathBase}" }
                    };
                });
            })
            .UseSwaggerUI(options =>
            {
                // Use relative path to prevent duplication when accessed through reverse proxy
                options.SwaggerEndpoint(
                    "./v1/swagger.json",
                    appName);

                options.RoutePrefix = "swagger";
                options.DocumentTitle = $"{appName} Documentation";
                options.OAuthClientId(clientId);
                options.OAuthAppName(appName);
                options.OAuthUsePkce();
                options.DocExpansion(DocExpansion.None);

                // Enable deep linking for better UX
                options.EnableDeepLinking();

                // Enable request duration display
                options.DisplayRequestDuration();

                // Enable filter for better navigation
                options.EnableFilter();
            });

        return app;
    }
}

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public ConfigureSwaggerOptions(
        IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public void Configure(SwaggerGenOptions options)
    {
        var disco = GetDiscoveryDocument();

        var apiScope = _config.GetValue<string>("UnifiedPlatform:ApiName");
        if (string.IsNullOrWhiteSpace(apiScope))
        {
            throw new InvalidOperationException("UnifiedPlatform:ApiName configuration is missing or empty");
        }

        var scopes = apiScope.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        var additionalScopes = _config.GetValue<string>("UnifiedPlatform:AdditionalScopes");
        if (!string.IsNullOrWhiteSpace(additionalScopes))
        {
            scopes.AddRange(additionalScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        var oauthScopeDic = scopes.ToDictionary(
            scope => scope, 
            scope => $"Resource access: {scope}");

        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "UnifiedLogin Landing API",
                Version = "v1",
                Description = "UnifiedLogin Landing API for managing user authentication and authorization"
            });

        // ✅ CRITICAL: Use UseAllOfToExtendReferenceSchemas to control schema expansion
        options.UseAllOfToExtendReferenceSchemas();

        // ✅ Register schema filter FIRST to control how RequestParameter is represented
        options.SchemaFilter<RequestParameterSchemaFilter>();

        // ✅ Register operation filter to replace auto-generated params with custom ones
        options.OperationFilter<RequestParameterOperationFilter>();
        // Enable annotations
        options.EnableAnnotations();

        // ✅ Custom schema ID to avoid type conflicts
        options.CustomSchemaIds(type => type.FullName?.Replace("+", "_"));

        // ✅ CRITICAL: Use custom parameter filter to suppress RequestParameter expansion
        options.ParameterFilter<RequestParameterParameterFilter>();
        // Add XML comments if available
        AddXmlComments(options);

        // Configure OAuth2 security
        ConfigureOAuth2Security(options, disco, oauthScopeDic);
        
        // Customize schema IDs to avoid conflicts
        options.CustomSchemaIds(type => type.FullName);
    }

    private void ConfigureOAuth2Security(
        SwaggerGenOptions options, 
        DiscoveryDocumentResponse disco, 
        Dictionary<string, string> oauthScopeDic)
    {
        if (disco.IsError)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve discovery document: {disco.Error}");
        }

        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "OAuth2 authorization code flow with PKCE",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(disco.AuthorizeEndpoint 
                        ?? throw new InvalidOperationException("AuthorizeEndpoint is null")),
                    TokenUrl = new Uri(disco.TokenEndpoint 
                        ?? throw new InvalidOperationException("TokenEndpoint is null")),
                    Scopes = oauthScopeDic
                }
            }
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference 
                    { 
                        Type = ReferenceType.SecurityScheme, 
                        Id = "oauth2" 
                    }
                },
                oauthScopeDic.Keys.ToArray()
            }
        });
    }

    private void AddXmlComments(SwaggerGenOptions options)
    {
        // Try to load XML documentation if it exists
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }

    private DiscoveryDocumentResponse GetDiscoveryDocument()
    {
        var authority = _config.GetValue<string>("UnifiedPlatform:Authority");
        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new InvalidOperationException("UnifiedPlatform:Authority configuration is missing or empty");
        }

        var client = _httpClientFactory.CreateClient("IdentityServer");
        client.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            return client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = authority,
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = !authority.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase),
                    ValidateIssuerName = true,
                    ValidateEndpoints = true
                }
            })
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve discovery document from {authority}", ex);
        }
    }
}

