using RP.Enterprise.Foundation.Audit.WebApi.Component;
using RP.Enterprise.Foundation.Audit.WebApi.Component.Filters;
using RP.Enterprise.Foundation.Audit.WebApi.Component.Handler;
using System;
using System.Configuration;
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


			// global performace handling / logging
			if (Convert.ToBoolean(ConfigurationManager.AppSettings["ShouldLogPerformance"]))
				config.Filters.Add(new ApiPerformanceFilter());

			// global error handling / logging
			config.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());
			config.Services.Add(typeof(IExceptionLogger), new ApiExceptionLogger());
		}
	}
}