using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Net;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder, string appName) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureLoggingAndOpenTelemetry(appName);

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        // Register HttpContextAccessor for accessing HttpContext in services
        builder.Services.AddHttpContextAccessor();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    private static readonly string[] _excludeFromTraces = [
        "aspnetcore-browser-refresh.js",
        "browserLink",
        "health",
    ];
    public static TBuilder ConfigureLoggingAndOpenTelemetry<TBuilder>(this TBuilder builder, string appName) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });
        
        var traceRatio = double.TryParse(builder.Configuration["TraceIdRatioBasedSampler"] ?? "", out var parsed) ? parsed : 0.1;

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(appName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.SetSampler(new TraceIdRatioBasedSampler(traceRatio));
                tracing.AddSource(appName)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = (httpContext) =>
                        {
                            var excludeFromTraces = _excludeFromTraces;
                            return !excludeFromTraces.Any(ex => httpContext.Request.Path.Value?.Contains(ex) ?? false);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.FilterHttpRequestMessage = (request) =>
                            !request.RequestUri?.ToString().Contains("getScriptTag", StringComparison.InvariantCultureIgnoreCase) ?? true;
                    });
            });

        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (!useOtlpExporter) return builder;

        if (builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"] != null)
        {
            if (!builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"]!.Contains("service.instance.id"))
            {
                builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"] += ",service.instance.id=" + Dns.GetHostName();
            }
        }

        builder.Services.AddOpenTelemetry().UseOtlpExporter();
        //builder.Services.AddSingleton(new ActivitySource(builder.Environment.ApplicationName));

        return builder;
    }
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        //builder.Services.AddRequestTimeouts();
        //builder.Services.AddOutputCache();

        builder.Services.AddRequestTimeouts(
            configure: static timeouts =>
                timeouts.AddPolicy("HealthChecks", TimeSpan.FromSeconds(5)));

        builder.Services.AddOutputCache(
            configureOptions: static caching =>
                caching.AddPolicy("HealthChecks",
                build: static policy => policy.Expire(TimeSpan.FromSeconds(10))));

        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        var healthChecks = app.MapGroup("");

        healthChecks
            .CacheOutput("HealthChecks")
            .WithRequestTimeout("HealthChecks");

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        app.UseRequestTimeouts();
        app.UseOutputCache();

        return app;
    }

    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(opts => // built-in problem details support
            opts.CustomizeProblemDetails = (ctx) =>
            {
                ctx.ProblemDetails.Extensions.Remove("traceId");
                ctx.ProblemDetails.Extensions.Add(new KeyValuePair<string, object?>("traceId", Activity.Current?.RootId));
                ctx.ProblemDetails.Extensions.Add(new KeyValuePair<string, object?>("spanId", Activity.Current?.SpanId.ToString()));

                if (ctx.ProblemDetails.Status == 404)
                {
                    ctx.ProblemDetails.Detail = "No item was found using the route and query you provided.";
                    ctx.ProblemDetails.Extensions.Add(new KeyValuePair<string, object?>("url",
                        $"{ctx.HttpContext.Request.Path}{ctx.HttpContext.Request.QueryString}"));
                }
                else
                {
                    ctx.ProblemDetails.Detail = "An error occurred in our API. Use the trace id when contacting us.";
                }
            });

        return services;
    }

    public static ProblemDetails ToProblemDetails(this ValidationResult result)
    {
        var problemDetails = new ProblemDetails();

        problemDetails.Extensions.Add(new KeyValuePair<string, object?>("traceId", Activity.Current?.RootId));
        problemDetails.Extensions.Add(new KeyValuePair<string, object?>("spanId", Activity.Current?.SpanId.ToString()));
        problemDetails.Status = 400;
        problemDetails.Title = "One or more validation errors occurred.";
        problemDetails.Extensions.Add("errors", result.Errors.Select(e =>
                                        new { e.PropertyName, e.ErrorMessage }));

        return problemDetails;
    }
}
