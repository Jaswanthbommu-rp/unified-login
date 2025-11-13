using RealPage.Logging.Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace UnifiedLogin.Core;

public static class LoggingExtensions
{
    public static void ConfigureLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        loggerConfiguration
              .WithRealPageConfiguration(configuration, "UnifiedLogin Landing API");

        loggerConfiguration
            .WriteTo.Async(sinkConfiguration => sinkConfiguration.Console(LogEventLevel.Error));

        loggerConfiguration
            .WriteTo.OpenTelemetry();
    }
}
