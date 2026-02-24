using Microsoft.Extensions.Configuration;
using Serilog;

namespace UnifiedLogin.Core;

public static class LoggingExtensions
{
    public static void ConfigureLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
    //    loggerConfiguration
    //          .WithRealPageConfiguration(configuration, "UnifiedLogin Landing API");

    //    loggerConfiguration
    //        .WriteTo.Async(sinkConfiguration => sinkConfiguration.Console(LogEventLevel.Error));

        //loggerConfiguration
        //    .WriteTo.OpenTelemetry();
    }
}
