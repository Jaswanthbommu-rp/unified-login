using System;
using System.Diagnostics;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using Sustainsys.Saml2;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logging
{
	public class TestLogger : ILoggerAdapter
	{
		public void WriteError(string message, Exception ex)
		{
			var infoDetails = new LogDetails()
			{
				ProductName = "Saml2AuthenticationOptions",
				ElapsedMilliseconds = 0,
				//CorrelationId = logDetails.CorrelationId,
				Exception = ex,
				Message = message,
				AdditionalInfo = null,
				ServerName = Environment.MachineName,
				UserId = null,
				UserName = null,
				ProductLocation = null,
				PmcId = null,
				PmcName = null,
				SiteId = null,
				SiteName = null,
				Environment = "local",
				ProductModule = null,
				ProductWorkflow = null,
				ProductStep = null,
			};
			Log.Write(LogType.Error, infoDetails);
		}

		public void WriteInformation(string message)
		{
			var infoDetails = new LogDetails()
			{
				ProductName = "Saml2AuthenticationOptions",
				ElapsedMilliseconds = 0,
				//CorrelationId = logDetails.CorrelationId,
				Message = message,
				AdditionalInfo = null,
				ServerName = Environment.MachineName,
				UserId = null,
				UserName = null,
				ProductLocation = null,
				PmcId = null,
				PmcName = null,
				SiteId = null,
				SiteName = null,
				Environment = "DEVWWW",
				ProductModule = null,
				ProductWorkflow = null,
				ProductStep = null,
			};
			Log.Write(LogType.Diagnostic, infoDetails);
		}

		public void WriteVerbose(string message)
		{
			var infoDetails = new LogDetails()
			{
				ProductName = "Saml2AuthenticationOptions",
				ElapsedMilliseconds = 0,
				//CorrelationId = logDetails.CorrelationId,
				Message = message,
				AdditionalInfo = null,
				ServerName = Environment.MachineName,
				UserId = null,
				UserName = null,
				ProductLocation = null,
				PmcId = null,
				PmcName = null,
				SiteId = null,
				SiteName = null,
				Environment = "DEVWWW",
				ProductModule = null,
				ProductWorkflow = null,
				ProductStep = null,
			};
			Log.Write(LogType.Diagnostic, infoDetails);
		}
	}
}