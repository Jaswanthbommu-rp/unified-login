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
		private static HttpConfiguration _config;
		private static ApiControllerActionSelector _actionSelector = new ApiControllerActionSelector();
		private static DefaultHttpControllerSelector _controllerSelector;
		private static HttpControllerContext _controllerContext;
		private static HttpRequestMessage _request;
		private static HttpControllerDescriptor _controllerDescriptor;
		private static HttpActionDescriptor _actionDescriptor;
		private static Type _type;

		/// <summary>
		/// RouteTestBase Constructor
		/// </summary>
		/// <param name="Config">Represents a configuration of System.Web.Http.HttpServer instances.</param>
		/// <param name="ControllerSelector">Represents a default System.Web.Http.Dispatcher.IHttpControllerSelector instance for choosing a System.Web.Http.Controllers.HttpControllerDescriptor given a System.Net.Http.HttpRequestMessage.</param>
		public RouteTestBase(HttpConfiguration Config, DefaultHttpControllerSelector ControllerSelector)
		{
			_config = Config;
			_controllerSelector = ControllerSelector;
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
				_request = new HttpRequestMessage(method, url);
				var routeData = _config.Routes.GetRouteData(_request);
				_request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
				_controllerContext = new HttpControllerContext(_config, routeData, _request);
				_controllerDescriptor = _controllerSelector.SelectController(_request);
				_controllerContext.ControllerDescriptor = _controllerDescriptor;
				_type = _controllerDescriptor.ControllerType;
				_actionDescriptor = _actionSelector.SelectAction(_controllerContext);
			}
			catch (Exception e)
			{
				return "";
			}
			return _actionDescriptor.ActionName;
		}
	}
}
