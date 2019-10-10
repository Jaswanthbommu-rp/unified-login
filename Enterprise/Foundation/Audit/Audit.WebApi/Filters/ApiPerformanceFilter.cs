using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;

namespace RP.Enterprise.Foundation.Audit.WebApi.Component.Filters
{
    public class ApiPerformanceFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]))
            {
                var routeData = actionContext.RequestContext.RouteData.Route.RouteTemplate;

                var dict = new Dictionary<string, object>
                {
                    {"AuthenticationType", actionContext.RequestContext.Principal.Identity.AuthenticationType}
                };

                var user = actionContext.RequestContext.Principal as ClaimsPrincipal;
                if (user != null)
                {
                    var userNmClaim = user.Claims.FirstOrDefault(a => a.Type == "loginName");
                    if (userNmClaim != null && !dict.ContainsKey("loginName"))
                        dict.Add("loginName", userNmClaim.Value);

                    var orgIdClaim = user.Claims.FirstOrDefault(a => a.Type == "orgPartyId");
                    if (orgIdClaim != null && !dict.ContainsKey("pmcId"))
                        dict.Add("pmcId", orgIdClaim.Value);

                    var realPageClaim = user.Claims.FirstOrDefault(a => a.Type == "realPageId");
                    if (realPageClaim != null && !dict.ContainsKey("realPageId"))
                        dict.Add("realPageId", realPageClaim.Value);

                    var orgNameClaim = user.Claims.FirstOrDefault(a => a.Type == "orgName");
                    if (orgNameClaim != null && !dict.ContainsKey("pmcName"))
                        dict.Add("pmcName", orgNameClaim.Value);
                }

                //var identity = actionContext.RequestContext.Principal as ClaimsPrincipal;
                //if (identity != null && identity.HasClaim(a => a.Type == RealPageClaims.IdentityProvider) && !dict.ContainsKey("Id Provider"))
                //{
                //    dict.Add("Id Provider", identity.Claims.First(a => a.Type == RealPageClaims.IdentityProvider).Value);
                //}

                var referrer = actionContext.Request.Headers.Referrer;
                if (referrer != null)
                {
                    var source = actionContext.Request.Headers.Referrer.OriginalString;
                    if (source.ToLower().Contains("swagger"))
                        source = "Swagger";
                    if (!dict.ContainsKey("Referrer"))
                        dict.Add("Referrer", source);
                }

                actionContext.Request.Properties["PerfTracker"] = new PerformanceLogger(routeData, dict);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]))
            {
                try
                {
                    var perfTracker = actionExecutedContext.Request.Properties["PerfTracker"] as PerformanceLogger;
                    if (perfTracker != null)
                        perfTracker.Stop(actionExecutedContext);
                }
                catch (Exception ex)
                {
                    // ignoring logging exceptions -- don't want an API call to fail if we run into logging problems. 
                    Log.Write(LogType.Error,
                        new LogDetails { Exception = ex });
                }
            }
        }
    }
}
