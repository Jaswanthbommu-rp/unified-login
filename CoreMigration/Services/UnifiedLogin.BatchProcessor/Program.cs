using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using UnifiedLogin.BatchProcessor.Configuration;
using UnifiedLogin.BatchProcessor.Repositories;
using UnifiedLogin.BatchProcessor.Services;

// Configure Serilog early to catch startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "UnifiedLogin.BatchProcessor")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting UnifiedLogin.BatchProcessor");

    var builder = Host.CreateApplicationBuilder(args);

    // Configure Serilog from configuration
    builder.Services.AddSerilog((services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "UnifiedLogin.BatchProcessor")
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/batch-processor-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}");
    });

    // Configure BatchProcessor options with validation
    builder.Services
        .AddOptions<BatchProcessorOptions>()
        .BindConfiguration(BatchProcessorOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    // Register repositories
    builder.Services.AddScoped<IBatchRepository, BatchRepository>();

    // Register HTTP client with resilience policies
    builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<BatchProcessorOptions>>().Value;
        client.BaseAddress = new Uri(options.LandingApiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(options.ApiTimeoutSeconds);
        client.DefaultRequestHeaders.Add("User-Agent", "UnifiedLogin.BatchProcessor/1.0");
    })
    .AddStandardResilienceHandler(options =>
    {
        // Configure retry policy
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;

        // Configure circuit breaker
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 10;

        // Configure timeout
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
    });

    // Register background services
    builder.Services.AddHostedService<BatchPollingService>();
    builder.Services.AddHostedService<RetryBatchPollingService>();

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddCheck<BatchProcessorHealthCheck>("batch_processor");

    // Configure OpenTelemetry
    var otelSection = builder.Configuration.GetSection("OpenTelemetry");
    if (otelSection.Exists())
    {
        var serviceName = otelSection.GetValue<string>("ServiceName") ?? "UnifiedLogin.BatchProcessor";
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

    // Validate configuration at startup
    var options = host.Services.GetRequiredService<IOptions<BatchProcessorOptions>>().Value;
    Log.Information(
        "Configuration loaded: ThreadCount={ThreadCount}, PollingInterval={PollingInterval}s, BatchSize={BatchSize}",
        options.ThreadCount, options.PollingIntervalSeconds, options.BatchSize);

    await host.RunAsync();

    Log.Information("UnifiedLogin.BatchProcessor stopped cleanly");
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
    Log.Fatal(ex, "UnifiedLogin.BatchProcessor terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
