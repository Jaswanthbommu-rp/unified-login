using Serilog;
using RealPage.Logging.Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
 //builder.Host
 //   .UseSerilog((context, loggerConfiguration) => loggerConfiguration.ConfigureLogging(context.Configuration));

var environmentName = builder.Environment.EnvironmentName.ToLower();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add API documentation (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Unified Login API",
        Version = "v1",
        Description = "ASP.NET Core 8.0 Web API for Unified Login"
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Unified Login API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Log application startup
app.Lifetime.ApplicationStarted.Register(() =>
{
    Log.Information("Unified Login API started successfully");
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("Unified Login API is stopping");
});

try
{
    Log.Information("Starting Unified Login API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
