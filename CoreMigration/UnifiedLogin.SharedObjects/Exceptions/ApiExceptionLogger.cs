using Newtonsoft.Json;
using Serilog.Events;
using System.Security.Claims;

namespace UnifiedLogin.SharedObjects.Exceptions
{
	// Minimal placeholder types to satisfy references formerly provided by System.Web.Http.ExceptionHandling
	public abstract class ExceptionLogger
	{
		public abstract void Log(ExceptionLoggerContext context);
	}
	public class ExceptionLoggerContext
	{
		public HttpRequestMessage Request { get; set; } = new HttpRequestMessage();
		public CatchBlock CatchBlock { get; set; } = new CatchBlock();
		public RequestContext RequestContext { get; set; } = new RequestContext();
		public Exception Exception { get; set; } = new Exception();
	}
	public class CatchBlock { public string Name { get; set; } = string.Empty; }
	public class RequestContext { public ClaimsPrincipal Principal { get; set; } = new ClaimsPrincipal(new ClaimsIdentity()); }
	/// <summary>
	/// Refactored class comes from Audit.WebApi
	/// </summary>
	public class ApiExceptionLogger : ExceptionLogger
	{
		public override void Log(ExceptionLoggerContext context)
		{
			var additionalInfo = new Dictionary<string, object>
			{
				{"RequestURI", context.Request.RequestUri}, 
				{"CatchBlockName", context.CatchBlock.Name}, 
				{"AuthenticationType", context.RequestContext.Principal.Identity.AuthenticationType}
			};

			//TODO: Get Organization & other info
			string correlationId = "";

			var user = context.RequestContext.Principal as ClaimsPrincipal;
			if (user != null)
			{
				var userNmClaim = user.Claims.FirstOrDefault(a => a.Type.Equals("loginName", StringComparison.OrdinalIgnoreCase));
				if (userNmClaim != null) additionalInfo.Add("loginName", userNmClaim.Value);

				var personaClaim = user.Claims.FirstOrDefault(a => a.Type.Equals("personaId", StringComparison.OrdinalIgnoreCase));
				if (personaClaim != null) additionalInfo.Add("personaId", personaClaim.Value);

				var correlationIdClaim = user.Claims.FirstOrDefault(a => a.Type == "correlationId");
				if (correlationIdClaim != null)
				{
					correlationId = correlationIdClaim.ToString();
				}
			}

			var logger = Serilog.Log.Logger;
			logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(additionalInfo, Formatting.Indented), true);
			logger = logger.ForContext("ProductModule", this.GetType());
			logger = logger.ForContext("CorrelationId", correlationId);
			logger.Write(LogEventLevel.Error, context.Exception, "{ActionName} - {state}", propertyValue0: "ApiExceptionLogger", propertyValue1: $"Error : {context.Exception.Message}");
			// CorrelationId used as a key to search exception in the database
			//context.Exception.Data.Add("CorrelationId", logDetails.CorrelationId);
		}
	}
}
