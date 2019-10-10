using System;
using System.Diagnostics;
using System.ServiceProcess;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var productAssignService = new ProductAssignService();

            if (Environment.UserInteractive)
            {
                Log.Write(LogType.Information, new LogDetails { Message = "ProductAssignService Running in console mode"});

                productAssignService.Start();
                Logger.ConsoleOut("Press any key to stop program");
                Console.Read();
                productAssignService.Stop();
            }
            else
            {
                Log.Write(LogType.Information, new LogDetails { Message = "ProductAssignService Running in windows Service mode" });
                ServiceBase.Run(productAssignService);
            }
        }
    }
}
