using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// RouteTestBase
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class RouteTestBase
	{
		private readonly HttpConfiguration _config;
		private readonly DefaultHttpControllerSelector _controllerSelector;

		/// <summary>
		/// RouteTestBase Constructor
		/// </summary>
		/// <param name="config">Represents a configuration of System.Web.Http.HttpServer instances.</param>
		/// <param name="controllerSelector">Represents a default System.Web.Http.Dispatcher.IHttpControllerSelector instance for choosing a System.Web.Http.Controllers.HttpControllerDescriptor given a System.Net.Http.HttpRequestMessage.</param>
		public RouteTestBase(HttpConfiguration config, DefaultHttpControllerSelector controllerSelector)
		{
			_config = config;
			_controllerSelector = controllerSelector;
		}

		/// <summary>
		/// Verify Route To Action
		/// </summary>
		/// <param name="method">Method name</param>
		/// <param name="url">route url</param>
		/// <returns></returns>
		public string VerifyRouteToAction(HttpMethod method, string url)
		{
			try
			{
				// remove the leading /api as the services are now running under their own application
				url = url.Replace("/api", "");
                HttpRequestMessage request = new HttpRequestMessage(method, url);
				var routeData = _config.Routes.GetRouteData(request);
				request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
                HttpControllerContext controllerContext = new HttpControllerContext(_config, routeData, request);
                HttpControllerDescriptor controllerDescriptor = _controllerSelector.SelectController(request);
				controllerContext.ControllerDescriptor = controllerDescriptor;
                Type type = controllerDescriptor.ControllerType;
                ApiControllerActionSelector actionSelector = new ApiControllerActionSelector();
                HttpActionDescriptor actionDescriptor = actionSelector.SelectAction(controllerContext);
                return actionDescriptor.ActionName;
			}
			catch (Exception e)
			{
				return "";
			}
        }
	}
}
