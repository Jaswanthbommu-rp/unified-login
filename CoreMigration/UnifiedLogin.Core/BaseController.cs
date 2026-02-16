using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text.Json;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.Core
{
    /// <summary>
    /// Base controller providing common functionality for all API controllers.
    /// Handles user claims initialization, logging, and shared utilities.
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Holds default user claim related information
        /// </summary>
        protected readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Logger instance for structured logging
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Base constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor service</param>
        protected BaseController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
            _logger = Log.Logger;
        }

        /// <summary>
        /// Gets the current user's claims
        /// </summary>
        protected DefaultUserClaim UserClaims => _userClaimsAccessor.GetUserClaim();

        
        #region Logging

        /// <summary>
        /// Used to write data to the information log for diagnostic
        /// </summary>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="messageProperties">Message properties for the template</param>
        protected void WriteToInformationLog(string message, Dictionary<string, object> logData = null, params object[] messageProperties)
        {
            WriteToLog(logType: LogEventLevel.Information, message: message, logData: logData, messageProperties: messageProperties);
        }

        /// <summary>
        /// Used to write data to the error log
        /// </summary>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties for the template</param>
        protected void WriteToErrorLog(string message, Dictionary<string, object> logData = null, Exception exception = null, params object[] messageProperties)
        {
            WriteToLog(logType: LogEventLevel.Error, message: message, logData: logData, exception: exception, messageProperties: messageProperties);
        }

        /// <summary>
        /// Used to write data to the diagnostic log
        /// </summary>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="messageProperties">Message properties for the template</param>
        protected void WriteToDiagnosticLog(string message, Dictionary<string, object> logData = null, params object[] messageProperties)
        {
            WriteToLog(logType: LogEventLevel.Debug, message: message, logData: logData, messageProperties: messageProperties);
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, params object[] messageProperties)
        {
            string correlationId = "";
            if (_userClaimsAccessor != null)
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                correlationId = (userClaim?.CorrelationId != Guid.Empty) ? userClaim.CorrelationId.ToString() : "";
            }

            var logger = _logger;

            if (logData?.Keys != null && logData.Keys.Any())
            {
                logger = logger.ForContext("AdditionalInfo", JsonSerializer.Serialize(logData, new JsonSerializerOptions { WriteIndented = true }), false);
            }

            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);

            // Handle message properties array safely
            if (messageProperties != null && messageProperties.Length > 0)
            {
                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValues: messageProperties);
            }
            else
            {
                logger.Write(level: logType, exception: exception, messageTemplate: message);
            }
        }

        #endregion
    }
}
