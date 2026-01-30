using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using UnifiedLogin.Core;
using UnifiedLogin.LandingAPIEnterprise.Configuration;
using UnifiedLogin.LandingAPIEnterprise.Services.Role;
using UnifiedLogin.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.AddKeyedSqlServerClient("DBConnection");

builder.Services.AddDistributedMemoryCache(); // used for caching access token for remote api call

builder.Services.AddLaunchDarkly(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow enum string values in JSON (e.g., "CreateUpdateProductUser" instead of just 1)
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());

        // Use camelCase for property names in JSON responses
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddApiProblemDetails();

builder.Services
    .AddMvcCoreWithAddOns()
    .AddVersioning()
    .AddUnifiedPlatformAuthentication(builder.Configuration)
    .AddApiIntegrations(builder.Configuration)
    .AddSwaggerDocumentation()
    .AddRepositories(builder.Configuration)
    .AddUserControllerServices();

// Register Enterprise Role services
builder.Services.AddScoped<IRoleQueryService, RoleQueryService>();
builder.Services.AddScoped<IClientCredentialAuthenticator, ClientCredentialAuthenticator>();


var app = builder.Build();

app.MapDefaultEndpoints();

var allCorsOrigins = builder.Configuration.GetValue<string>("AllCORSOrigins");
if (!string.IsNullOrEmpty(allCorsOrigins))
{
    var origins = allCorsOrigins.Split(",");
    app.UseCors(bld => bld
           .WithOrigins(origins)
           .AllowAnyHeader()
           .AllowAnyMethod());
}

var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Log authentication configuration for debugging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var authority = builder.Configuration.GetValue<string>("UnifiedPlatform:Authority");
var apiName = builder.Configuration.GetValue<string>("UnifiedPlatform:ApiName");
logger.LogInformation("JWT Authority: {Authority}", authority);
logger.LogInformation("JWT ApiName/Audiences: {ApiName}", apiName);

var forwardHeaderOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedPrefix, };

app
    .UseForwardedHeaders(forwardHeaderOptions)
    .UseSwaggerDocumentation(builder.Configuration, apiVersionProvider, "UnifiedLogin_LandingAPI Enterprise", "apienterprisev2")
    .UseRouting() // routing should come before authentication/authorization
    .UseAuthentication()
    .UseAuthorization()
    .UseMiddleware<UnifiedLoginUserScopeMiddleware>()
    .UseExceptionHandler() // put here so we can capture logged in user with exceptions
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.Run();

public partial class Program { } // needed for integration testing