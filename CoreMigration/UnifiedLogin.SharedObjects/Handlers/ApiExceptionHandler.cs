using System.Net;

namespace UnifiedLogin.SharedObjects.Handlers
{
	// Minimal placeholder interfaces/classes to replace removed System.Web.Http types
	public interface IExceptionHandler
	{
		Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken);
	}
	public interface IHttpActionResult
	{
		Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken);
	}
	public class ExceptionHandlerContext
	{
		public Exception Exception { get; set; } = new Exception();
		public ExceptionContext ExceptionContext { get; set; } = new ExceptionContext();
		public IHttpActionResult Result { get; set; }
	}
	public class ExceptionContext
	{
		public HttpRequestMessage Request { get; set; } = new HttpRequestMessage();
		public CatchBlock CatchBlock { get; set; } = new CatchBlock();
	}
	public class CatchBlock
	{
		public bool IsTopLevel { get; set; } = true;
	}
	/// <summary>
	/// Class moved from Audit.WebApi project
	/// </summary>
	public class ApiExceptionHandler : IExceptionHandler
	{
		public virtual Task HandleAsync(ExceptionHandlerContext context,
									CancellationToken cancellationToken)
		{
			if (!ShouldHandle(context))
			{
				return Task.CompletedTask;
			}

			return HandleAsyncCore(context, cancellationToken);
		}

		public virtual Task HandleAsyncCore(ExceptionHandlerContext context,
										   CancellationToken cancellationToken)
		{
			HandleCore(context);
			return Task.CompletedTask;
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
