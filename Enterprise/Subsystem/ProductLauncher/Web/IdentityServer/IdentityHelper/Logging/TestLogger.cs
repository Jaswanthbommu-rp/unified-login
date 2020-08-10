using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using Serilog;
using Serilog.Events;
using Sustainsys.Saml2;
using System;

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
				Environment = ConfigReader.Environment,
				ProductModule = null,
				ProductWorkflow = null,
				ProductStep = null,
			};
			Log.Write(LogEventLevel.Error, message, infoDetails);
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
				Environment = ConfigReader.Environment,
				ProductModule = null,
				ProductWorkflow = null,
				ProductStep = null,
			};
			Log.Write(LogEventLevel.Debug, message, infoDetails);
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
				Environment = ConfigReader.Environment,
				ProductModule = null,
				ProductWorkflow = null,
				ProductStep = null,
			};
			Log.Write(LogEventLevel.Debug, message, infoDetails);
		}
	}
}