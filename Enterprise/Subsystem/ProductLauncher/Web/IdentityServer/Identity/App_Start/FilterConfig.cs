using System;
using System.Configuration;
using System.Web.Mvc;
using RP.Enterprise.Foundation.Audit.MvcWeb.Component;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]))
                filters.Add(new TrackPerformanceAttribute()); // used for MVC API controller 
        }
    }
}
