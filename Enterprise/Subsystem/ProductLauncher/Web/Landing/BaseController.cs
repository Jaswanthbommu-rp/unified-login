using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Routing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public abstract class BaseController : Controller
	{
		/// <summary>
		/// Holds default user claim related information
		/// </summary>
		public DefaultUserClaim _userClaims;

		/// <summary>
		/// base constructor
		/// </summary>
		protected BaseController() { }

		protected override void Initialize(RequestContext requestContext)
		{
			base.Initialize(requestContext);

			ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
			if (currentClaimPrincipal.Identity.IsAuthenticated)
			{
				_userClaims = new DefaultUserClaim(currentClaimPrincipal);
				List<string> userRights = BaseUserRights.GetUserRightsBy(currentClaimPrincipal, _userClaims);

				if (userRights != null && userRights.Count > 0)
				{
					var identity = (ClaimsIdentity)currentClaimPrincipal.Identity;
					identity.AddClaims(userRights.Select(a => new Claim("right", a)).ToList());
				}
				_userClaims.Rights = userRights;
			}
			else
			{
				// if the call is anonymous, build a default guid and later the code may override it if one has been stored somewhere else
				_userClaims = new DefaultUserClaim() { CorrelationId = Guid.NewGuid() };
			}
		}

		#region Logging

		/// <summary>
		/// Used to write data to the information log for diagnostic
		/// </summary>
		protected void WriteToInformationLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
			WriteToLog(logType: LogEventLevel.Information, message: message, logData: logData, messageProperties: messageProperties);

		}

		/// <summary>
		/// Used to write data to the error log
		/// </summary>
		protected void WriteToErrorLog(string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
		{
			WriteToLog(logType: LogEventLevel.Error, message: message, logData: logData, exception: exception, messageProperties: messageProperties);
		}

		/// <summary>
		/// Used to write data to the diagnostic log
		/// </summary>
		protected void WriteToDiagnosticLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
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
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            string correlationId = "";
            if (_userClaims != null)
            {
                correlationId = (_userClaims.CorrelationId != Guid.Empty) ? _userClaims.CorrelationId.ToString() : "";
            }
            var logger = Log.Logger;
			if (logData?.Keys != null)
			{
				logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
			}
			logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValues: messageProperties);
        }

		#endregion
	}
}