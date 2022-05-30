using Elastic.Apm;
using Elastic.Apm.AspNetFullFramework;
using RealPage.Logging.Serilog;
using System;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Command
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            SerilogHelpers.ConfigureSerilog("Unified Login");

            // set up agent with components
            var agentComponents = ElasticApmModule.CreateAgentComponents();
            Agent.Setup(agentComponents);
        }

    }
}