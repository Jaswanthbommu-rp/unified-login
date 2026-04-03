using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Reflection;
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
				// Always build a fresh config restricted to the LandingAPI assembly so that
				// LandingAPIEnterprise controllers (loaded by other tests in the same AppDomain)
				// do not create conflicting routes and return null routeData.
				var config = new HttpConfiguration();
				config.Services.Replace(typeof(IAssembliesResolver),
					new SingleAssemblyResolver(typeof(WebApiConfig).Assembly));
				WebApiConfig.Register(config);
				config.EnsureInitialized();
				var controllerSelector = new DefaultHttpControllerSelector(config);

				// remove the leading /api as the services are now running under their own application
				url = url.Replace("/api", "");
                var uri = new Uri(url);
				_request = new HttpRequestMessage(method, uri);
				var routeData = config.Routes.GetRouteData(_request);
				_request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
				_controllerContext = new HttpControllerContext(config, routeData, _request);
				_controllerDescriptor = controllerSelector.SelectController(_request);
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

	[ExcludeFromCodeCoverage]
	internal sealed class SingleAssemblyResolver : DefaultAssembliesResolver
	{
		private readonly Assembly _assembly;

		public SingleAssemblyResolver(Assembly assembly)
		{
			_assembly = assembly;
		}

		public override ICollection<Assembly> GetAssemblies()
		{
			return new[] { _assembly };
		}
	}
}
