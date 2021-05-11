using System;
using System.Net;
using System.ServiceProcess;
using RealPage.Logging.Serilog;
using Serilog;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var productAssignService = new BatchProcessorService();

            SerilogHelpers.ConfigureSerilog("Unified Login");

            if (Environment.UserInteractive)
            {
                Log.Information("BatchProcessorService Running in console mode");

                productAssignService.Start();

                Console.WriteLine("Press any key to stop program");
                Console.Read();
                productAssignService.Stop();
            }
            else
            {
                Log.Information("BatchProcessorService Running in windows Service mode");
                ServiceBase.Run(productAssignService);
            }

        }

    }
}
