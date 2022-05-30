using Elastic.Apm;
using Elastic.Apm.AspNetFullFramework;
using RealPage.Logging.Serilog;
using System;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            SerilogHelpers.ConfigureSerilog("Unified Login");

            GlobalConfiguration.Configure(WebApiConfig.Register);

            // set up agent with components
            var agentComponents = ElasticApmModule.CreateAgentComponents();
            Agent.Setup(agentComponents);
        }
    }
}
