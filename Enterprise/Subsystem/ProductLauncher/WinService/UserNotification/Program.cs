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
        /// The main entry point for the application
        /// </summary>
        static void Main()
        {
            SerilogHelpers.ConfigureSerilog("Unified Login");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var userNotificationService = new UserNotificationService();

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
