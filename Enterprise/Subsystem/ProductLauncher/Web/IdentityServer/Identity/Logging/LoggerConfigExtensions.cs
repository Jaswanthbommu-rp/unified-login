using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity.Logging
{
    public static class LoggerConfigExtensions
    {
        public static LoggerConfiguration RpEntLibQueue(
            this LoggerSinkConfiguration loggerConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null,
            bool supplyContextMessage = false)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");

            return loggerConfiguration.Sink(new RpSeriLogSink(formatProvider), restrictedToMinimumLevel);
        }
    }
}