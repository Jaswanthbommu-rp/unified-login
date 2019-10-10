using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader
{
    /// <summary>
    /// Used to allow only certain domains to use the apis
    /// </summary>
    public class AllowCorsAttribute : Attribute, ICorsPolicyProvider
    {
        private CorsPolicy _policy;

        /// <summary>
        /// Used to get the allowed domains for the system
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retval = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = false,
                PreflightMaxAge = 600,
                SupportsCredentials = true
            };

            IList<string> corsUrls = ConfigurationManager.AppSettings["CorsUrls"].Split(',');

            foreach (string corsUrl in corsUrls)
            {
                if (!string.IsNullOrEmpty(corsUrl))
                {
                    retval.Origins.Add(corsUrl.Trim());
                }
            }
             
            _policy = retval;
            return Task.FromResult(_policy);
        }
    }
}