using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using UnifiedLogin.UserNotification.Configuration;
using UnifiedLogin.UserNotification.Services;

// Configure Serilog early to catch startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "UnifiedLogin.UserNotification")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting UnifiedLogin.UserNotification");

    var builder = Host.CreateApplicationBuilder(args);

    // ✅ Configure Serilog from configuration
    builder.Services.AddSerilog((services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "UnifiedLogin.UserNotification")
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/user-notification-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}");
    });

    // ✅ Configure UserNotification options with validation
    builder.Services
        .AddOptions<UserNotificationOptions>()
        .BindConfiguration(UserNotificationOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // ✅ Register the background worker service
    builder.Services.AddHostedService<UserNotificationWorker>();

    // ✅ Add health checks
    builder.Services.AddHealthChecks()
        .AddCheck<NotificationHealthCheck>("notification_service");

    // ✅ Configure OpenTelemetry
    var otelSection = builder.Configuration.GetSection("OpenTelemetry");
    if (otelSection.Exists())
    {
        var serviceName = otelSection.GetValue<string>("ServiceName") ?? "UnifiedLogin.UserNotification";
        var serviceVersion = otelSection.GetValue<string>("ServiceVersion") ?? "1.0.0";
        var otlpEndpoint = otelSection.GetValue<string>("OtlpEndpoint");

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddOtlpExporter(options =>
                {
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    }
                }));
    }

    builder.AddServiceDefaults();

    var host = builder.Build();

    // ✅ Validate configuration at startup
    var options = host.Services.GetRequiredService<IOptions<UserNotificationOptions>>().Value;
    Log.Information(
        "Configuration loaded: Interval={Interval}s, Workers={Workers}, BatchSize={BatchSize}",
        options.IntervalSeconds, options.WorkerThreads, options.BatchSize);

    await host.RunAsync();

    Log.Information("UnifiedLogin.UserNotification stopped cleanly");
    return 0;
}
catch (OptionsValidationException ex)
{
    Log.Fatal(ex, "Configuration validation failed");
    foreach (var failure in ex.Failures)
    {
        Log.Fatal("  - {Failure}", failure);
    }
    return 1;
}
catch (Exception ex)
{
    Log.Fatal(ex, "UnifiedLogin.UserNotification terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
