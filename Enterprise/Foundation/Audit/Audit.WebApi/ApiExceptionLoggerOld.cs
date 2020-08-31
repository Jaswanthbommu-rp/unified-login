using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Http.ExceptionHandling;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using System.Linq;

namespace RP.Enterprise.Foundation.Audit.WebApi.Component
{
    /// <summary>
    /// Implementation for a global exception *logger* that is injected to the API via the WebApiConfig.cs in App_Start
    /// </summary>
    public class ApiExceptionLoggerOld : ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            var logDetails = new LogDetails
            {
                AdditionalInfo = new Dictionary<string, object>
                {
                    {"RequestURI", context.Request.RequestUri},
                    {"CatchBlockName", context.CatchBlock.Name},
                    {"Principal Name", context.RequestContext.Principal.Identity.Name}
                },
                ServerName = Environment.MachineName,
                CorrelationId = Guid.NewGuid().ToString(),
                Exception = context.Exception
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
                if (userNmClaim != null) logDetails.UserName = userNmClaim.Value;
                //if (userNmClaim != null && !dict.ContainsKey("loginName"))
                //    dict.Add("loginName", userNmClaim.Value);

                var orgIdClaim = user.Claims.FirstOrDefault(a => a.Type == "orgPartyId");
                if (orgIdClaim != null) logDetails.PmcId = orgIdClaim.Value;
                //if (orgIdClaim != null && !dict.ContainsKey("pmcId"))
                //    dict.Add("pmcId", orgIdClaim.Value);

                var orgNameClaim = user.Claims.FirstOrDefault(a => a.Type == "orgName");
                if (orgNameClaim != null) logDetails.PmcName = orgNameClaim.Value;

                var realPageClaim = user.Claims.FirstOrDefault(a => a.Type == "realPageId");
                if (realPageClaim != null && !dict.ContainsKey("realPageId")) dict.Add("realPageId", realPageClaim.Value);

                var userIdClaim = user.Claims.FirstOrDefault(a => a.Type == "sub");
                if (userIdClaim != null) logDetails.UserId = userIdClaim.Value;
            }

            Audit.Core.Component.Log.Write(LogType.Error, logDetails);

            // CorrelationId used as a key to search exception in the database
            context.Exception.Data.Add("CorrelationId", logDetails.CorrelationId);
        }
    }
}
