using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using RP.Enterprise.Foundation.Audit.WebApi.Component;
using RP.Enterprise.Foundation.Audit.WebApi.Component.Filters;
using RP.Enterprise.Foundation.Audit.WebApi.Component.Handler;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            // global performace handling / logging
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]))
                config.Filters.Add(new ApiPerformanceFilter()); // used for WebApi only (ApiController) & not MVC based API (Controller)

            // global error handling / logging
            // used for WebApi only (ApiController) & not MVC based API (Controller)
            config.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());
            config.Services.Add(typeof(IExceptionLogger), new ApiExceptionLogger());
        }
    }
}
