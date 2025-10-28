using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Handlers
{
    public class NoCacheHandler : DelegatingHandler
    {
        /// <summary>
        /// Add a custom delegation handler to override the caching set by the web server so the webapi calls are not cached
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // add the no-cache header to the response
            var response = await base.SendAsync(request, cancellationToken);
                System.Net.Http.Headers.CacheControlHeaderValue cache =
                    new System.Net.Http.Headers.CacheControlHeaderValue
                    {
                        NoCache = true,
                        NoStore = true
                    };
                if (response != null)
                {
                    response.Headers.CacheControl = cache;
                }

                return response;
        }
    }
}
