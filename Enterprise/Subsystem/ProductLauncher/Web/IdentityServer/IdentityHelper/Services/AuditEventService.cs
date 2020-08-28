using IdentityServer3.Core.Events;
using IdentityServer3.Core.Services;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    public class AuditEventService : IEventService
	{
		public Task RaiseAsync<T>(Event<T> evt)
		{
			try
			{
				switch (evt.Category)
				{
					case EventConstants.Categories.Authentication:
						{
							var logDetails = new Dictionary<string, object>();
							logDetails.Add("evt.Details.GetType()", evt.Details.GetType());
							if (evt.Details.GetType() == typeof(LocalLoginDetails))
							{
								var clientDetails = evt.Details as LocalLoginDetails;
								logDetails.Add("clientid", clientDetails?.SignInMessage.ClientId);
                                logDetails.Add("loginUserName", clientDetails?.LoginUserName);
								logDetails.Add("clienttype", "LocalLoginDetails");
							}
							if (evt.Details.GetType() == typeof(ExternalLoginDetails))
							{
								var clientDetails = evt.Details as ExternalLoginDetails;
								logDetails.Add("clientid", clientDetails?.SignInMessage.ClientId);
								logDetails.Add("provider", clientDetails?.Provider);
                                logDetails.Add("subjectId", clientDetails?.SubjectId);
                                logDetails.Add("loginUserName", clientDetails?.Name);
								logDetails.Add("clienttype", "ExternalLoginDetails");
							}
							if (evt.Details.GetType() == typeof(LoginDetails))
							{
								var clientDetails = evt.Details as LoginDetails;
								logDetails.Add("clientid", clientDetails?.SignInMessage.ClientId);
                                logDetails.Add("loginUserName", clientDetails?.Name);
								logDetails.Add("clienttype", "ExternalLoginDetails");
							}
							WriteToLog(LogEventLevel.Debug, "Client authenticated", evt.Context.ActivityId, logDetails);
							break;
						}

					case EventConstants.Categories.ClientAuthentication:
						{
							var logDetails = new Dictionary<string, object>();
							var clientDetails = evt.Details as ClientAuthenticationDetails;
							logDetails.Add("clientid", clientDetails?.ClientId);
							logDetails.Add("clienttype", "ClientAuthentication");
							WriteToLog(LogEventLevel.Debug, "Client authenticated", evt.Context.ActivityId, logDetails);
							break;
						}
				}
			}
			catch (Exception ex) { }

			return Task.FromResult(evt);
		}

		private void WriteToLog(LogEventLevel logType, string message, string correlationId, Dictionary<string, object> logData = null, Exception exception = null)
		{
            var logger = Log.Logger;
            foreach (var key in logData?.Keys)
            {
                logger = logger.ForContext($"AdditionalInfo-{key}", logData[key], true);
            }
            logger.Write(logType, exception, message );
		}
	}
}
