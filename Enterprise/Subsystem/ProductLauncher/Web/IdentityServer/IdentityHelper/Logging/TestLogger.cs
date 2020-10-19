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
			Log.Write(LogEventLevel.Error, ex, message);
		}

		public void WriteInformation(string message)
		{
			Log.Write(LogEventLevel.Debug, message);
		}

		public void WriteVerbose(string message)
		{
			Log.Write(LogEventLevel.Debug, message);
		}
	}
}