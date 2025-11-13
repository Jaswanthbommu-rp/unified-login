namespace UnifiedLogin.LandingAPI.Middleware
{
    /// <summary>
    /// Middleware to add no-cache headers to all API responses
    /// Prevents browser and proxy caching of API responses
    /// </summary>
    public class NoCacheMiddleware
    {
        private readonly RequestDelegate _next;

        public NoCacheMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Call the next middleware in the pipeline
            await _next(context);

            // Add no-cache headers to the response
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            context.Response.Headers.Pragma = "no-cache";
            context.Response.Headers.Expires = "0";
        }
    }

    /// <summary>
    /// Extension method to register the NoCacheMiddleware
    /// </summary>
    public static class NoCacheMiddlewareExtensions
    {
        public static IApplicationBuilder UseNoCache(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NoCacheMiddleware>();
        }
    }
}
