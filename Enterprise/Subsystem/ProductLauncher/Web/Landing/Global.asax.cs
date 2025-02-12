using System;
using System.IdentityModel.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.AspNetFullFramework;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using RealPage.Logging.Serilog;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Logging;
using StackExchange.Redis;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public class MultiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var redisConnectionString = "localhost";  // Your Redis server connection string
                                                      // Use fully qualified name to avoid ambiguity
            var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString);

            // Configure Redis as distributed cache
            var redisCache = new RedisCacheOptions
            {
                Configuration = redisConnectionString,
                InstanceName = "MyApp:"
            };

            // Assign the Redis Cache to a global variable (or use Dependency Injection if possible)
            var distributedCache = new RedisCache(redisCache);

            // Example: Using the cache in the application
            Application["Cache"] = distributedCache;

            SerilogHelpers.ConfigureSerilog("Unified Login");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            // set up agent with components
            var agentComponents = ElasticApmModule.CreateAgentComponents();
            Agent.Setup(agentComponents);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();

            int httpStatus;
            string errorControllerAction;

            AuditLogHelper.GetHttpStatus(ex, out httpStatus);
            switch (httpStatus)
            {
                case 404:
                    errorControllerAction = "NotFound";
                    break;
                default:
                    AuditLogHelper.LogWebError(ex);
                    errorControllerAction = "Index";
                    break;
            }

            var httpContext = ((MultiApplication)sender).Context;
            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 500;
            httpContext.Response.TrySkipIisCustomErrors = true;

            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = errorControllerAction;

            var controller = new ErrorController();
            //controller.ViewData.Model = new HandleErrorInfo(ex, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }
    }
}
