namespace UnifiedLogin.LandingAPI.Middleware
{
    /// <summary>
    /// Middleware to handle Tibco requests by reading and storing request body data
    /// </summary>
    public class TibcoRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TibcoRequestMiddleware> _logger;

        public TibcoRequestMiddleware(RequestDelegate next, ILogger<TibcoRequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is from Tibco by looking for the signature header
            if (context.Request.Headers.ContainsKey("signature"))
            {
                _logger.LogDebug("Tibco request detected with signature header");

                // Enable buffering to allow multiple reads of the request body
                context.Request.EnableBuffering();

                // Read the request body
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: System.Text.Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();

                // Reset the request body stream position for the next middleware
                context.Request.Body.Position = 0;

                // Store the Tibco post data in HttpContext.Items for later access
                context.Items["TibcoPostData"] = body;

                _logger.LogInformation("Tibco post data captured and stored in context");
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    /// <summary>
    /// Extension method to register the TibcoRequestMiddleware
    /// </summary>
    public static class TibcoRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseTibcoRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TibcoRequestMiddleware>();
        }
    }
}
