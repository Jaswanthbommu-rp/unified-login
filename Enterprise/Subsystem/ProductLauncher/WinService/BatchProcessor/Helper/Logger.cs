using System;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityBatchProcessor.Helper
{
    public static class Logger
    {
        public static void Write(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

        internal static void Write(LogType logType,   LogDetails logDetails)
        {
            Log.Write(logType, logDetails);

#if DEBUG
            Console.WriteLine(logDetails.Message + logDetails.Exception);
#endif
        }
    }
}
