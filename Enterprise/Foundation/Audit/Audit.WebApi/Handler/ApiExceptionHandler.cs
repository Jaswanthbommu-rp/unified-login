using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace RP.Enterprise.Foundation.Audit.WebApi.Component.Handler
{
    /// <summary>
    /// Implementation of custom global *handler* so that we can provide graceful error and tracing information when callers encounter exceptions when making API calls to us.
    /// Full details are shielded from the caller (but logged internally)
    /// </summary>
    public class ApiExceptionHandler : IExceptionHandler
    {
        public virtual Task HandleAsync(ExceptionHandlerContext context,
                                    CancellationToken cancellationToken)
        {
            if (!ShouldHandle(context))
            {
                return Task.FromResult(0);
            }

            return HandleAsyncCore(context, cancellationToken);
        }

        public virtual Task HandleAsyncCore(ExceptionHandlerContext context,
                                           CancellationToken cancellationToken)
        {
            HandleCore(context);
            return Task.FromResult(0);
        }

        public virtual void HandleCore(ExceptionHandlerContext context)
        {
            var errorInfo = string.Empty;
            var refId = string.Empty;

            if (context.Exception.Data.Contains("ErrorId")) // this is set within the custom *logger* which is called BEFORE this in the exception pipeline
                errorInfo = $"Error ID: {context.Exception.Data["ErrorId"]}";

            if (context.Exception.Data.Contains("CorrelationId"))
                refId = context.Exception.Data["CorrelationId"].ToString();

            context.Result = new TextPlainErrorResult
            {
                Request = context.ExceptionContext.Request,
                Content =
                $"Internal System Error. Please contact RealPage support with error reference Id - {refId}"
            };
        }

        public virtual bool ShouldHandle(ExceptionHandlerContext context)
        {
            return context.ExceptionContext.CatchBlock.IsTopLevel;
        }

        private class TextPlainErrorResult : IHttpActionResult
        {
            public HttpRequestMessage Request { get; set; }

            public string Content { get; set; }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(Content),
                    RequestMessage = Request
                };
                return Task.FromResult(response);
            }
        }
    }
}


///// <summary>
///// This method does the custom handling -- instead of an exception being sent, just a "server error" status code with a message that something went wrong, along with 
///// an ID that can be used to associate the error with an exact log entry if they contact our support team.
///// </summary>
///// <param name="context">Automatially passed by the framework</param>
//public override void Handle(ExceptionHandlerContext context)
//{
//    var errorInfo = string.Empty;

//    if (context.Exception.Data.Contains("ErrorId")) // this is set within the custom *logger* which is called BEFORE this in the exception pipeline
//        errorInfo = $"Error ID: {context.Exception.Data["ErrorId"]}";

//    context.Result = new TextPlainErrorResult
//    {
//        Request = context.ExceptionContext.Request,
//        Content =
//            $"Oops! Sorry! Something went wrong.Please contact RealPage support so we can fix it. {errorInfo}"
//    };
//}

//public override async Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
//{
//    // Access Exception
//    // var exception = context.Exception;

//    const string genericErrorMessage = "An unexpected error occured.";
//    var response = context.Request.CreateResponse(HttpStatusCode.InternalServerError,
//        new
//        {
//            Message = genericErrorMessage
//        });

//    response.Headers.Add("Error ", genericErrorMessage);
//    context.Result = new ResponseMessageResult(response);
//}

//public override bool ShouldHandle(ExceptionHandlerContext context)
//{
//    // not needed, but I left it to debug and find out why it never reaches Handle() method
//    return context.CatchBlock.IsTopLevel;
//}