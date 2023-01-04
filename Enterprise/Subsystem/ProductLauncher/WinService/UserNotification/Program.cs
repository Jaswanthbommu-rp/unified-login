using Elastic.Apm;
using Elastic.Apm.AspNetFullFramework;
using RealPage.Logging.Serilog;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification.Helper;
using Serilog;
using System;
using System.Net;
using System.ServiceProcess;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UserNotification
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            SerilogHelpers.ConfigureSerilog("Unified Login");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var userNotificationService = new UserNotificationService();

            // set up agent with components
            var agentComponents = ElasticApmModule.CreateAgentComponents();
            Agent.Setup(agentComponents);

            if (Environment.UserInteractive)
            {
                Log.Information("UserServiceMC Running in console mode");

                userNotificationService.Start();
                Logger.ConsoleOut("Press any key to stop program");
                Console.Read();
                userNotificationService.Stop();
            }
            else
            {
                Log.Information("UserServiceMC Running in windows Service mode");
                ServiceBase.Run(userNotificationService);
            }
        }
    }
}
