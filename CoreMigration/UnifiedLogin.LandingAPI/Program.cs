using RealPage.Logging.Serilog;
using Serilog;
using System.Reflection;
using UnifiedLogin.Core;

var builder = WebApplication.CreateBuilder(args);
// Configure Serilog
builder.Host
   .UseSerilog((context, loggerConfiguration) => loggerConfiguration.ConfigureLogging(context.Configuration));

var environmentName = builder.Environment.EnvironmentName.ToLower();

builder.Configuration
    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{environmentName}.json", false, true)
    .AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddMvcCoreWithAddOns()
                .AddConfiguredCors(builder.Configuration)
                .AddUnifiedPlatformAuthentication(builder.Configuration)
                .AddBusinessLogic(builder.Configuration)
                .AddKafka(builder.Configuration)
                .AddEndpointsApiExplorer()
                .AddSwaggerDocumentation()
                .AddRepositories(builder.Configuration)
                .AddDistributedMemoryCache()
                .AddHealthChecks();

var app = builder.Build();

app.UseSwaggerDocumentation(builder.Configuration)
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization()
    .UseCors("AllowedOrigins")
    .UseRouting()
    .UseSerilogRealPageRequestLogging();
    //.UseExceptionHandler();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

public partial class Program { } // needed for integration testing