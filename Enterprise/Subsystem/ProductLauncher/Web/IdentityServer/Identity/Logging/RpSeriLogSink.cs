using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
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
                LogLevel = (Foundation.Audit.Core.Component.Enums.LogLevel)((int)logEvent.Level),
                Exception = exception,
            };

            if (message.Contains("IDX10500") || (exception != null && exception.Message.Contains("IDX10500")))
            {
                if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogDiagnostic"]))
                {
                    Foundation.Audit.Core.Component.Log.Write(LogType.Diagnostic, logDetails);
                }
                return; // no need to log the "signature validation errors
            }

            if (exception != null)
            {
                Foundation.Audit.Core.Component.Log.Write(LogType.Error, logDetails);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogDiagnostic"]))
            {
                // add dignostic info including exception
                Foundation.Audit.Core.Component.Log.Write(LogType.Diagnostic, logDetails);
            }
        }
    }
}