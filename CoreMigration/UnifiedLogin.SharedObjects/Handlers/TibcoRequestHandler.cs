using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Handlers
{
    /// <summary>
    /// Tibco Request Delegate Handler
    /// </summary>
    public class TibcoRequestHandler : DelegatingHandler
    {
        /// <summary>
        /// Add a custom delegation handler to read the request sent from Tibco
        /// </summary>
        /// <param name="request">Http Request</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>HttpResponseMessage</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //If the request is from Tibco, read the Request body to extract P and S Ids and set the context
            if (request.Headers != null && request.Headers.Contains("signature"))
            {
                request.Properties.Add("TibcoPostData", request.Content.ReadAsStringAsync().Result);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
