using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Events;
using UnifiedLogin.SharedObjects.ResponseObject;
using Newtonsoft.Json;

namespace UnifiedLogin.LandingAPIEnterprise.Services
{
    /// <summary>
    /// Service for logging and error handling
    /// </summary>
    public interface ILoggingService
    {
        void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, 
            Exception exception = null, object[] messageProperties = null, Guid correlationId = default);
        
        void LogMethodEntry(string methodName, Guid correlationId);
        void LogMethodExit(string methodName, Guid correlationId);
    }

    public class LoggingService : ILoggingService
    {
        public void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null,
            Exception exception = null, object[] messageProperties = null, Guid correlationId = default)
        {
            try
            {
                var logger = Log.Logger;

                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }

                logger = logger.ForContext("CorrelationId", correlationId.ToString());

                logger.Write(
                    level: logType,
                    exception: exception,
                    messageTemplate: message,
                    propertyValue0: messageProperties?[0],
                    propertyValue1: messageProperties?[1]);
            }
            catch
            {
                // Silently fail to prevent logging errors from breaking the application
            }
        }

        public void LogMethodEntry(string methodName, Guid correlationId)
        {
            WriteToLog(LogEventLevel.Debug, "Entering {MethodName}", 
                messageProperties: new object[] { methodName }, 
                correlationId: correlationId);
        }

        public void LogMethodExit(string methodName, Guid correlationId)
        {
            WriteToLog(LogEventLevel.Debug, "Exiting {MethodName}", 
                messageProperties: new object[] { methodName }, 
                correlationId: correlationId);
        }
    }
}
