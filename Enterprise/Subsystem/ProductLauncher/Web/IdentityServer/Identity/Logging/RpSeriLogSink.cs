using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity.Logging
{
    public class RpSeriLogSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public RpSeriLogSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            var exception = logEvent.Exception;

            var dict = new Dictionary<string, object> { { "Level", logEvent.Level.ToString() } };

            foreach (var prop in logEvent.Properties)
            {
                if (prop.Key == "1" && logEvent.Level == LogEventLevel.Error)
                {
                    dynamic dyn = JsonConvert.DeserializeObject(prop.Value.ToString());
                    var newStr = Regex.Replace(((string)dyn), "\"password\": \".*\"", "\"password\": \"***\"",
                        RegexOptions.RightToLeft);
                    dict.Add(prop.Key, newStr);
                }
                else
                    dict.Add(prop.Key, prop.Value.ToString());
            }

            dict.Add("Message", message);

            var logDetails = new LogDetails
            {
                Message = message,
                CorrelationId = Guid.NewGuid().ToString(),
                ServerName = Environment.MachineName,
                AdditionalInfo = dict,
                LogLevel = (LogEventLevel)((int)logEvent.Level),
                Exception = exception,
            };

            if (message.Contains("IDX10500") || (exception != null && exception.Message.Contains("IDX10500")))
            {
                Log.Write(LogEventLevel.Debug, message, logDetails);

                return; // no need to log the "signature validation errors
            }

            if (exception != null)
            {
                Log.Write(LogEventLevel.Error, exception, exception.Message, logDetails);
            }

            // add dignostic info including exception
            Log.Write(LogEventLevel.Debug, exception, message, logDetails);
        }
    }
}