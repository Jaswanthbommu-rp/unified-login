using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
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
		protected void WriteToInformationLog(string message, Dictionary<string, object> logData = null)
		{
			WriteToLog(LogType.Information, message, logData);
		}

		/// <summary>
		/// Used to write data to the error log
		/// </summary>
		protected void WriteToErrorLog(string message, Dictionary<string, object> logData = null, Exception exception = null)
		{
			WriteToLog(LogType.Error, message, logData, exception);
		}

		/// <summary>
		/// Used to write data to the diagnostic log
		/// </summary>
		protected void WriteToDiagnosticLog(string message, Dictionary<string, object> logData = null)
		{
			WriteToLog(LogType.Diagnostic, message, logData);
		}

		private void WriteToLog(LogType logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
		{
			Log.Write(logType, new LogDetails
			{
				Message = message,
				AdditionalInfo = logData,
				ProductModule = this.GetType().ToString(),
				UserId = _userClaims?.UserId.ToString(),
				PmcId = _userClaims?.OrganizationPartyId.ToString(),
				UserName = _userClaims?.LoginName,
				Exception = exception,
				CorrelationId = _userClaims?.CorrelationId.ToString(),
			});
		}

		#endregion
	}
}