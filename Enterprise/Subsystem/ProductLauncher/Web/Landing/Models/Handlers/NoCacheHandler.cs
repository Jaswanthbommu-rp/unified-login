using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models.Handlers
{
    public class NoCacheHandler : DelegatingHandler
    {
        /// <summary>
        /// Add a custom delegation handler to override the caching set by the web server so the webapi calls are not cached
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // add the no-cache header to the response

            return base.SendAsync(request, cancellationToken)
             .ContinueWith(task =>
             {
                 // work on the response
                 var response = task.Result;
                 System.Net.Http.Headers.CacheControlHeaderValue cache = new System.Net.Http.Headers.CacheControlHeaderValue();
                 cache.NoCache = true;
                 cache.NoStore = true;
                 response.Headers.CacheControl = cache;
                 return response;
             });
        }
    }
}