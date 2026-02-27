using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Text;
using System.Text.Json;

namespace UnifiedLogin.BatchProcessor.HealthChecks;

/// <summary>
/// Hosted service that runs a simple HTTP listener for health check endpoints.
/// </summary>
public class HealthCheckWebServer : BackgroundService
{
    private readonly ILogger<HealthCheckWebServer> _logger;
    private readonly HealthCheckService _healthCheckService;
    private readonly int _port;

    public HealthCheckWebServer(
        ILogger<HealthCheckWebServer> logger,
        HealthCheckService healthCheckService,
        IConfiguration configuration)
    {
        _logger = logger;
        _healthCheckService = healthCheckService;
        _port = configuration.GetValue<int>("HealthCheck:Port", 5000);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new HttpListener();

        try
        {
            // Register health check endpoints
            listener.Prefixes.Add($"http://+:{_port}/health/");
            listener.Start();

            _logger.LogInformation("Health check web server started on port {Port}", _port);
            _logger.LogInformation("  - http://localhost:{Port}/health/ (detailed health status)", _port);
            _logger.LogInformation("  - http://localhost:{Port}/health/ready (readiness probe)", _port);
            _logger.LogInformation("  - http://localhost:{Port}/health/live (liveness probe)", _port);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(async () => await HandleRequest(context, stoppingToken), stoppingToken);
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995) // Operation aborted
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting HTTP request");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start health check web server");
        }
        finally
        {
            if (listener.IsListening)
            {
                listener.Stop();
            }
            listener.Close();
        }
    }

    private async Task HandleRequest(HttpListenerContext context, CancellationToken cancellationToken)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var path = request.Url?.AbsolutePath?.ToLowerInvariant();

            switch (path)
            {
                case "/health/":
                case "/health":
                    await HandleDetailedHealthCheck(response, cancellationToken);
                    break;

                case "/health/ready":
                    await HandleReadinessCheck(response, cancellationToken);
                    break;

                case "/health/live":
                    await HandleLivenessCheck(response);
                    break;

                default:
                    response.StatusCode = 404;
                    await WriteJsonResponse(response, new { error = "Not found" });
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling health check request for {Path}", request.Url?.AbsolutePath);
            response.StatusCode = 500;
            await WriteJsonResponse(response, new { error = "Internal server error" });
        }
        finally
        {
            response.Close();
        }
    }

    private async Task HandleDetailedHealthCheck(HttpListenerResponse response, CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var statusCode = report.Status switch
        {
            HealthStatus.Healthy => 200,
            HealthStatus.Degraded => 200,
            HealthStatus.Unhealthy => 503,
            _ => 500
        };

        response.StatusCode = statusCode;

        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                tags = e.Value.Tags,
                data = e.Value.Data
            })
        };

        await WriteJsonResponse(response, result);
    }

    private async Task HandleReadinessCheck(HttpListenerResponse response, CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            check => check.Tags.Contains("ready"),
            cancellationToken);

        var statusCode = report.Status switch
        {
            HealthStatus.Healthy => 200,
            HealthStatus.Degraded => 200,
            HealthStatus.Unhealthy => 503,
            _ => 500
        };

        response.StatusCode = statusCode;

        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow
        };

        await WriteJsonResponse(response, result);
    }

    private async Task HandleLivenessCheck(HttpListenerResponse response)
    {
        response.StatusCode = 200;

        var result = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow
        };

        await WriteJsonResponse(response, result);
    }

    private static async Task WriteJsonResponse(HttpListenerResponse response, object data)
    {
        response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
    }
}
