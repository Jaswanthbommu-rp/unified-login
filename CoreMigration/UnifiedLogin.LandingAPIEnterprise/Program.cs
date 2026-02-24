using Microsoft.AspNetCore.HttpOverrides;
using System.Reflection;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.Core;
using UnifiedLogin.Core.Filters;
using UnifiedLogin.LandingAPIEnterprise.Services;
using UnifiedLogin.LandingAPIEnterprise.Services.Role;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults("unified-login-coreenterpriseapiv2");

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

builder.Services.AddLaunchDarkly(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new RequestParameterModelBinderProvider());
    options.Filters.Add<InitializeUserRightsFilter>();
}).AddNewtonsoftJsonConfiguration();

builder.Services.AddApiProblemDetails();

builder.Services
    .AddMvcCoreWithAddOns()
    .AddUnifiedPlatformAuthentication(builder.Configuration)
    .AddApiIntegrations(builder.Configuration)
    .AddSwaggerDocumentation()
    .AddRepositories(builder.Configuration)
    .AddBusinessLogicServices();


// Register DefaultUserClaim factory - required by business logic classes
builder.Services.AddScoped(sp =>
{
    var userClaimsAccessor = sp.GetRequiredService<IUserClaimsAccessor>();
    return userClaimsAccessor.GetUserClaim();
});
// Register IManagePersona with DefaultUserClaim
builder.Services.AddScoped<IManagePersona>(sp =>
{
    var userClaim = sp.GetRequiredService<DefaultUserClaim>();
    return new ManagePersona(userClaim);
});
// Register UserManagement - required by UserManagementService and UserQueryService
builder.Services.AddScoped<UserManagement>(provider =>
{
    var userClaims = provider.GetRequiredService<DefaultUserClaim>();
    return new UserManagement(userClaims);
});

// Register SamlRepository - required by UserQueryService
builder.Services.AddScoped<SamlRepository>();
// Register UPFM Products Integration Factory instead of direct service registration
builder.Services.AddScoped<IManageUPFMProductsIntegrationFactory, ManageUPFMProductsIntegrationFactory>();

// Register Enterprise Role services
builder.Services.AddScoped<IRoleQueryService, RoleQueryService>();
builder.Services.AddScoped<IClientCredentialAuthenticator, ClientCredentialAuthenticator>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IUserValidationService, UserValidationService>();
builder.Services.AddScoped<ISuperUserValidationService, SuperUserValidationService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IProductFormattingService, ProductFormattingService>();
builder.Services.AddScoped<IClientAuthenticationService, ClientAuthenticationService>();
builder.Services.AddSingleton<ILoggingService, LoggingService>();

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
    .UseSwaggerDocumentation(builder.Configuration, "UnifiedLogin_LandingAPI Enterprise", "apienterprisev2")
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