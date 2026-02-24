using Microsoft.AspNetCore.HttpOverrides;
using System.Reflection;
using UnifiedLogin.Core;
using UnifiedLogin.Core.Filters;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults("unified-login-coreapiv2");

var environmentName = builder.Environment.EnvironmentName.ToLower();

builder.Configuration
    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{environmentName}.json", false, true)
    .AddEnvironmentVariables();

// Initialize ConfigReader with IConfiguration for static access throughout the application
ConfigReader.Initialize(builder.Configuration);

builder.AddKeyedSqlServerClient("DBConnection");

builder.Services.AddDistributedMemoryCache(); // used for caching access token for remote api call
builder.Services.AddMemoryCache();
builder.Services.AddLaunchDarkly(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new RequestParameterModelBinderProvider());
    options.Filters.Add<InitializeUserRightsFilter>();
}).AddNewtonsoftJsonConfiguration();

// Register DefaultUserClaim factory - required by business logic classes
builder.Services.AddScoped(sp =>
{
    var userClaimsAccessor = sp.GetRequiredService<IUserClaimsAccessor>();
    return userClaimsAccessor.GetUserClaim();
});

builder.Services.AddApiProblemDetails();

builder.Services
    .AddMvcCoreWithAddOns()
    .AddUnifiedPlatformAuthentication(builder.Configuration)
    .AddApiIntegrations(builder.Configuration)
    .AddSwaggerDocumentation()
    .AddRepositories(builder.Configuration)
    .AddBusinessLogicServices();


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

// Log authentication configuration for debugging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var authority = builder.Configuration.GetValue<string>("UnifiedPlatform:Authority");
var apiName = builder.Configuration.GetValue<string>("UnifiedPlatform:ApiName");
logger.LogInformation("JWT Authority: {Authority}", authority);
logger.LogInformation("JWT ApiName/Audiences: {ApiName}", apiName);
var forwardHeaderOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedPrefix, };

app
    .UseForwardedHeaders(forwardHeaderOptions)
    .UseSwaggerDocumentation(builder.Configuration, "UnifiedLogin_LandingAPI", "apiv2")
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