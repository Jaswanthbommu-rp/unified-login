using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http.ExceptionHandling;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions
{
    /// <summary>
    /// Refactored class comes from Audit.WebApi
    /// </summary>
    public class ApiExceptionLogger : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {

            Dictionary<string, object> AdditionalInfo = new Dictionary<string, object>
                {
                    {"RequestURI", context.Request.RequestUri},
                    {"CatchBlockName", context.CatchBlock.Name},
                    {"Principal Name", context.RequestContext.Principal.Identity.Name}
                };

            //TODO: Get Organization & other info
            var dict = new Dictionary<string, object>
                {
                    {"AuthenticationType", context.RequestContext.Principal.Identity.AuthenticationType}
                };

            var user = context.RequestContext.Principal as ClaimsPrincipal;
            if (user != null)
            {
                var userNmClaim = user.Claims.FirstOrDefault(a => a.Type == "loginName");
                //if (userNmClaim != null) logDetails.UserName = userNmClaim.Value;
                //if (userNmClaim != null && !dict.ContainsKey("loginName"))
                //    dict.Add("loginName", userNmClaim.Value);

                var orgIdClaim = user.Claims.FirstOrDefault(a => a.Type == "orgPartyId");
                //if (orgIdClaim != null) logDetails.PmcId = orgIdClaim.Value;
                //if (orgIdClaim != null && !dict.ContainsKey("pmcId"))
                //    dict.Add("pmcId", orgIdClaim.Value);

                var orgNameClaim = user.Claims.FirstOrDefault(a => a.Type == "orgName");
                //if (orgNameClaim != null) logDetails.PmcName = orgNameClaim.Value;

                var realPageClaim = user.Claims.FirstOrDefault(a => a.Type == "realPageId");
                if (realPageClaim != null && !dict.ContainsKey("realPageId")) dict.Add("realPageId", realPageClaim.Value);

                var userIdClaim = user.Claims.FirstOrDefault(a => a.Type == "sub");
                //if (userIdClaim != null) logDetails.UserId = userIdClaim.Value;
            }

            Serilog.Log.ForContext("AdditionalInfo", AdditionalInfo).Write( LogEventLevel.Error, context.Exception, context.Exception.Message);

            // CorrelationId used as a key to search exception in the database
            //context.Exception.Data.Add("CorrelationId", logDetails.CorrelationId);
        }
    }
}
