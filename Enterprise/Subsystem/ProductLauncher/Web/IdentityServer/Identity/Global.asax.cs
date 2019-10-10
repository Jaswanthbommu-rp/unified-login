using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using RP.Enterprise.Foundation.Audit.MvcWeb.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Identity.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
	        Sustainsys.Saml2.Configuration.Options.GlobalEnableSha256XmlSignatures();
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

			var httpContext = ((WebApiApplication)sender).Context;
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
