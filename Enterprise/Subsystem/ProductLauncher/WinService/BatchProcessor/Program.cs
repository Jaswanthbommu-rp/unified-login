using System;
using System.ServiceProcess;
using RealPage.Logging.Serilog;
//using RP.Enterprise.Foundation.Audit.Core.Component;
//using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper;
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
            var productAssignService = new BatchProcessorService();

            SerilogHelpers.ConfigureSerilog("unitybatchprocessor");

            if (Environment.UserInteractive)
            {
                //Logger.Write(LogType.Information, new LogDetails { Message = "BatchProcessorService Running in console mode"});
                Log.Information("BatchProcessorService Running in console mode");

                productAssignService.Start();
                //Logger.Write("Press any key to stop program");
                Log.Information("Press any key to stop program");
                Console.Read();
                productAssignService.Stop();
            }
            else
            {
                //Logger.Write(LogType.Information, new LogDetails { Message = "BatchProcessorService Running in windows Service mode" });
                Log.Information("BatchProcessorService Running in windows Service mode");
                ServiceBase.Run(productAssignService);
            }

        }

    }
}
