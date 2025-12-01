using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

using Microsoft.AspNetCore.Hosting;
using UnifiedLogin.LandingAPI;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// RouteTestBase
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class RouteTestBase
	{
		private readonly HttpClient _config;
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
		/// <param name="Config">Represents a configuration of Microsoft.AspNetCore.Mvc.HttpServer instances.</param>
		/// <param name="ControllerSelector">Represents a default Microsoft.AspNetCore.Mvc.Dispatcher.IHttpControllerSelector instance for choosing a Microsoft.AspNetCore.Mvc.Controllers.HttpControllerDescriptor given a System.Net.Http.HttpRequestMessage.</param>
		public RouteTestBase(HttpClient Config, DefaultHttpControllerSelector ControllerSelector)
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
