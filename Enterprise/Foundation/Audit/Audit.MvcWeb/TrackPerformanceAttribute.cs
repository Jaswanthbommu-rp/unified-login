using System.Web.Mvc;
using RP.Enterprise.Foundation.Audit.Core.Component;

namespace RP.Enterprise.Foundation.Audit.MvcWeb.Component
{
    public class TrackPerformanceAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string userId, userName, location, pmcId, pmcName;
            var dict = AuditLogHelper.GetWebLoggingData(out userId, out userName, out location, out pmcId, out pmcName);

            var type = filterContext.HttpContext.Request.RequestType;
            var perfName = filterContext.ActionDescriptor.ActionName + "_" + type;

            var stopwatch = new PerformanceLogger(perfName, dict);
            filterContext.HttpContext.Items["Stopwatch"] = stopwatch;
        }
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var stopwatch = (PerformanceLogger)filterContext.HttpContext.Items["Stopwatch"];
            if (stopwatch != null)
                stopwatch.Stop();
        }
    }
}
