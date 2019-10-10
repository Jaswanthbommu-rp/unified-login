using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;

namespace RP.Enterprise.Foundation.Audit.Core.Component
{
    // Handles exception occured in Owin Startup
    public class OwinStartupExceptionHandler
    {
        private readonly Exception _exception;
        private string _browserMessage = string.Empty;

        public OwinStartupExceptionHandler(Exception exception)
        {
            _exception = exception;
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            if (!context.Request.Path.Value.Contains("favicon")) // this is to prevent logging message two times
            {
                string correlationId = Guid.NewGuid().ToString();

                _browserMessage =
                    $"Error in OwinStartup. Please contact RealPage support with reference Id - {correlationId} ";

                LogDetails logDetails = new LogDetails
                {
                    CorrelationId = correlationId,
                    Exception = _exception,
                    Message = _exception.Message,
                    //HostName = Environment.MachineName,
                };

                Foundation.Audit.Core.Component.Log.Write(LogType.Error, logDetails);
            }

            return context.Response.WriteAsync(_browserMessage);
        }
    }
}
