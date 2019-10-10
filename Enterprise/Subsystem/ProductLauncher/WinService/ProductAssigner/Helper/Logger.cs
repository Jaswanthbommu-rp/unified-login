using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.ProductAssigner.Helper
{
    public static class Logger
    {
        public static void ConsoleOut(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }
    }
}
