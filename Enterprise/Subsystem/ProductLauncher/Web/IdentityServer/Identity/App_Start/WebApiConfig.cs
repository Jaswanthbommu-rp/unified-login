using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Handlers;
using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configure Web API to use only bearer token authentication.
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // global performace handling / logging
            //if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]))
            //    config.Filters.Add(new ApiPerformanceFilter()); // used for WebApi only (ApiController) & not MVC based API (Controller)

            // global error handling / logging
            // used for WebApi only (ApiController) & not MVC based API (Controller)
            config.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());
            config.Services.Add(typeof(IExceptionLogger), new ApiExceptionLogger());
        }
    }
}
