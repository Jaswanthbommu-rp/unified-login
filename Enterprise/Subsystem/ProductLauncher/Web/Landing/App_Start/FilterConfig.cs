using System.Web.Mvc;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            //if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]))
            //    filters.Add(new TrackPerformanceAttribute()); // used for MVC API controller 
        }
    }
}
