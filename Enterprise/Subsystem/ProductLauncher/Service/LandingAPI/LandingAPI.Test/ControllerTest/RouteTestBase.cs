using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// RouteTestBase
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class RouteTestBase
	{
		private readonly HttpConfiguration _config;
		private readonly ApiControllerActionSelector _actionSelector = new ApiControllerActionSelector();
		private readonly DefaultHttpControllerSelector _controllerSelector;
		private HttpControllerContext _controllerContext;
		private HttpRequestMessage _request;
		private HttpControllerDescriptor _controllerDescriptor;
		private HttpActionDescriptor _actionDescriptor;
		private Type _type;

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
                var uri = new Uri(url);
				_request = new HttpRequestMessage(method, uri);
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
				throw new Exception(e.Message);
			}
			return _actionDescriptor.ActionName;
		}
	}
}
