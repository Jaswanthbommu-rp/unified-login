using System.Web.Mvc;
using System.Web.Routing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
	            name: "Home main",
	            url: "home",
	            defaults: new { controller = "Home", action = "Index", msgId = UrlParameter.Optional }
            );

			routes.MapRoute(
	            name: "Signout",
	            url: "signout/{msgId}",
	            defaults: new { controller = "Home", action = "Signout", msgId = UrlParameter.Optional }
            );

			//AzureAuth
			routes.MapRoute(
				name: "AzureAuth",
				url: "azureauth",
				defaults: new { controller = "Home", action = "AzureAuth" }
			);

			routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
