using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Handlers;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise
{
    public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services

			// Web API routes
			config.MapHttpAttributeRoutes();

			//config.Routes.MapHttpRoute(
			// name: "LandingApi",
			// routeTemplate: "{controller}/{id}",
			// defaults: new { id = RouteParameter.Optional }
			//);

			// global error handling / logging
			config.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());
			config.Services.Add(typeof(IExceptionLogger), new ApiExceptionLogger());
		}
	}
}