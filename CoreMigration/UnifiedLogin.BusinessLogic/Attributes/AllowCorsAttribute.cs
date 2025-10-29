using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace UnifiedLogin.BusinessLogic.Attributes
{
    /// <summary>
    /// Used to allow only certain domains to use the apis
    /// </summary>
    public class AllowCorsAttribute : Attribute, ICorsPolicyProvider
    {
        private CorsPolicy _policy;
        private readonly string[] _originsToAllow;

        /// <summary>
        /// AllowCors Attribute
        /// </summary>
        public AllowCorsAttribute(params string[] origins)
        {
            this._originsToAllow = origins;
        }

        /// <summary>
        /// Used to get the allowed domains for the system
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corsAllowedPolicy = new CorsPolicy
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                AllowAnyOrigin = false,
                PreflightMaxAge = 600,
                SupportsCredentials = true
            };

            IEnumerable<string> originValues;
            request.Headers.TryGetValues("Origin", out originValues);

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "apicors_globalSettings";
            IEnumerable<GlobalSetting> globalSettings = rpcache.GetFromCache<IEnumerable<GlobalSetting>>(cacheKey, 300, () =>
            {
                // load from api
                GlobalSettingRepository gsr = new GlobalSettingRepository();
                IEnumerable<GlobalSetting> allSettings = gsr.GetGlobalSettings();

                return allSettings.Where(p => _originsToAllow.Contains(p.SettingName, StringComparer.OrdinalIgnoreCase));
            });

            foreach (var gs in globalSettings)
            {
                if (!gs.Value.Contains("*"))
                {
                    if (!corsAllowedPolicy.Origins.Contains(gs.Value))
                    {
                        corsAllowedPolicy.Origins.Add(gs.Value);
                    }
                }
                else
                {
                    foreach (var origin in originValues)
                    {
                        if (origin.Contains(gs.Value.Replace("*", "")))
                        {
                            if (!corsAllowedPolicy.Origins.Contains(origin))
                            {
                                corsAllowedPolicy.Origins.Add(origin);
                            }
                        }
                    }
                }
            }

            _policy = corsAllowedPolicy;
            return Task.FromResult(_policy);
        }
    }
}