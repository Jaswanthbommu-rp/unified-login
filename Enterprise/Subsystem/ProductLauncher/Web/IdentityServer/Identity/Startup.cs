using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Services.Default;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Identity;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Identity.Logging;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Configuration;
using Serilog;
using System;
using System.Net;

[assembly: OwinStartup(typeof(Startup))]
namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity
{
	public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			// Okta has deprecated the usage of older TLS versions so we need to tell the code to use a newer version by default or it will fail to connect
	        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			try
			{
				Serilog.Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    //.Enrich.WithProperty("UserName", System.Security.Principal.WindowsIdentity.GetCurrent().Name)
                    .Enrich.WithProperty("MachineName", Environment.MachineName)
                    .WriteTo.RpEntLibQueue()
                    .CreateLogger();

                var options = IdentityServerConfig.GetIdentityServerOptions();

				//app.UseCookieAuthentication(new CookieAuthenticationOptions());
                app.SetDefaultSignInAsAuthenticationType(ConfigReader.Environment+"-IDCookie");
                app.Map("/identity", idsrvApp =>
                {
                    SetFactoryAndViewOptions(options);
                    IdentityServerConfig.SetupCustomImplementationHooks(options);
                    idsrvApp.UseIdentityServer(options);
                });

	            app.Map("/signoutcleanup", cleanup =>
	            {
		            cleanup.Run(
			            async ctx =>
			            {
				            await ctx.Environment.ProcessFederatedSignoutAsync();
						});
	            });

                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = ConfigReader.Environment + "-IDCookie",
                });

                // try to catch any operation was cancelled errors and ignore them so they aren't logged in kibana
                app.Use(async (ctx, next) =>
                {
                    try
                    {
                        await next();
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });
			}
			catch (Exception ex)
			{
				throw new Exception("There was a problem starting the server. " + ex.Message);
				// no longer throwing the owin error because it caused the server to be unusable. throwing a normal exception still allows the server to retry the failed startup
				// so it can fix itself without requiring an IIS reset
			}
		}

        private static void SetFactoryAndViewOptions(IdentityServerOptions options)
        {
            Uri idServer = new Uri(ConfigReader.GetIssuerUri);
            string idServerHost = idServer.Scheme + "://" + idServer.DnsSafeHost;
			
			options.CspOptions.ScriptSrc = idServerHost + " " + ConfigReader.GetCspOptions_ScriptSrcAdditional;
            options.CspOptions.StyleSrc = idServerHost + " " + ConfigReader.GetCspOptions_StyleSrcAdditional;
            options.CspOptions.FontSrc = idServerHost + " " + ConfigReader.GetCspOptions_FontSrcAdditional;
            options.CspOptions.ConnectSrc = idServerHost + " " + ConfigReader.GetLandingAPIUri + " " + ConfigReader.GetReturnUri + " " + ConfigReader.GetCspOptions_ConnectSrcAdditional;
            options.CspOptions.ImgSrc = idServerHost + " " + ConfigReader.GetCspOptions_ImageSrcAdditional;
            options.CspOptions.FrameSrc = ConfigReader.GetReturnUri + " " + ConfigReader.GetCspOptions_FrameSrcAdditional;

            //options.CspOptions.Enabled = false;
            options.EnableWelcomePage = false;

			var viewOptions = new DefaultViewServiceOptions
			{
				CustomViewDirectory = System.Web.Hosting.HostingEnvironment.MapPath("~/Content"),

				//#if DEBUG
				//            options.EnableWelcomePage = true;
				//#endif

				//#if DEBUG
				CacheViews = false,
			};
			//#endif

			// include IdentityServer signout logic for multiple client logins
			viewOptions.Scripts.Add("/login/scripts/additional.min.js");

			options.Factory.ConfigureDefaultViewService(viewOptions);
			
            //viewOptions.Scripts.Add("/scripts/simon-learn.js");

            options.Factory.ViewService = new DefaultViewServiceRegistration<CustomLoginPageUserService>(viewOptions);
        }
    }
}