using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ===================================================================
// CONFIGURATION SETUP
// ===================================================================

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

// ===================================================================
// SERVICE REGISTRATION (Migrated from Startup.ConfigureServices)
// ===================================================================

// Add Controllers with JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Camel case property names (matching old Newtonsoft.Json CamelCasePropertyNamesContractResolver)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .AddNewtonsoftJson(options =>
    {
        // For backwards compatibility with Newtonsoft.Json
        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
    });

// Add API Explorer for endpoint discovery
builder.Services.AddEndpointsApiExplorer();

// ===================================================================
// AUTHENTICATION & AUTHORIZATION (Migrated from OWIN IdentityServer)
// ===================================================================

// Get configuration values
var issuerUri = builder.Configuration["IdentityConfig:IssuerUri"] 
    ?? throw new InvalidOperationException("IssuerUri configuration is required");
var requiredScopes = builder.Configuration["IdentityConfig:RequiredScope"]?.Split(' ', StringSplitOptions.RemoveEmptyEntries) 
    ?? new[] { "enterpriseapi" };

// Note: TLS 1.2+ is enabled by default in .NET 9 (no need for ServicePointManager configuration)

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuerUri;
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuerUri,
            ValidateAudience = true,
            ValidAudiences = requiredScopes,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Optional: Load signing certificate if configured
        var certThumbprint = builder.Configuration["IdentityConfig:SigningCertThumbprint"];
        if (!string.IsNullOrEmpty(certThumbprint))
        {
            options.TokenValidationParameters.IssuerSigningKey = GetSigningCertificateKey(certThumbprint);
        }

        // Event handlers for troubleshooting
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("Token validated for: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Add scope-based authorization policies
    foreach (var scope in requiredScopes)
    {
        options.AddPolicy(scope, policy => 
            policy.RequireClaim("scope", scope));
    }
});

// ===================================================================
// CORS CONFIGURATION (Migrated from EnableCors)
// ===================================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // Add named CORS policy for specific origins if configured
    var allowedOrigins = builder.Configuration.GetSection("LandingAPICORSAllowedOrigins").Get<string[]>();
    if (allowedOrigins != null && allowedOrigins.Length > 0)
    {
        options.AddPolicy("LandingAPICORSAllowedOrigins", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

// ===================================================================
// SWAGGER/OPENAPI CONFIGURATION (Migrated from SwaggerConfig)
// ===================================================================

var environment = builder.Configuration["logging:environment"] ?? builder.Environment.EnvironmentName;
var isProdEnvironment = string.Equals(environment, "PROD", StringComparison.OrdinalIgnoreCase);

if (!isProdEnvironment)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "RealPage Unified Login Enterprise API",
            Version = "v1",
            Description = "Enterprise API for RealPage Unified Login Platform"
        });

        // Configure OAuth2 Implicit Flow (matching legacy config)
        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{issuerUri}/connect/authorize"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "userinfoapi", "Access to the User Info API" },
                        { "enterpriseapi", "Access to the Enterprise APIs" }
                    }
                }
            }
        });

        // Configure Bearer Token authentication
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Enable annotations support
        c.EnableAnnotations();

        // Include XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });
}

// ===================================================================
// HEALTH CHECKS
// ===================================================================

builder.Services.AddHealthChecks();

// ===================================================================
// HTTP CLIENT CONFIGURATION
// ===================================================================

builder.Services.AddHttpClient();

// ===================================================================
// PROBLEM DETAILS
// ===================================================================

builder.Services.AddProblemDetails();

// ===================================================================
// BUILD APPLICATION
// ===================================================================

var app = builder.Build();

// ===================================================================
// MIDDLEWARE PIPELINE (Migrated from Startup.Configure)
// ===================================================================

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline
if (!isProdEnvironment)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RealPage Unified Login Enterprise API v1");
        c.OAuthClientId("swagger-ui");
        c.OAuthAppName("Swagger UI");
    });
}

// Global exception handler
app.UseExceptionHandler();
app.UseStatusCodePages();

// HTTPS Redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS - Must be before Authentication/Authorization
app.UseCors("AllowAll");

// Routing
app.UseRouting();

// Authentication & Authorization (matching OWIN app.UseIdentityServerBearerTokenAuthentication)
app.UseAuthentication();
app.UseAuthorization();

// No-cache headers (matching NoCacheHandler functionality)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
    context.Response.Headers.Append("Pragma", "no-cache");
    context.Response.Headers.Append("Expires", "0");
    await next();
});

// Map Controllers (matching WebApiConfig.Register with attribute routing)
app.MapControllers();

// Health checks endpoint
app.MapHealthChecks("/health");

// ===================================================================
// RUN APPLICATION
// ===================================================================

try
{
    Log.Information("Starting UnifiedLogin.LandingAPIEnterprise");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

// ===================================================================
// HELPER METHODS
// ===================================================================

static SecurityKey? GetSigningCertificateKey(string thumbprint)
{
    try
    {
        var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        certStore.Open(OpenFlags.ReadOnly);
        
        var certCollection = certStore.Certificates.Find(
            X509FindType.FindByThumbprint, 
            thumbprint, 
            validOnly: false);
        
        certStore.Close();
        
        if (certCollection.Count > 0)
        {
            var cert = certCollection[0];
            Log.Information("Loaded signing certificate: {Subject}", cert.Subject);
            return new X509SecurityKey(cert);
        }
        
        Log.Warning("No certificate found with thumbprint: {Thumbprint}", thumbprint);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error loading signing certificate with thumbprint: {Thumbprint}", thumbprint);
    }
    
    return null;
}
