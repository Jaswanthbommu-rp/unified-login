using IdentityServer3.Core.Services;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Configuration
{
    public class CorsPolicyService : ICorsPolicyService
    {
        private static string IdentityServerCORSAllowedOrigins = "IdentityServerCorsAllowedOrigins";

        private readonly IIdentityServerRepository _identityServerRepository;

        public CorsPolicyService(IIdentityServerRepository identityServerRepository)
        {
            _identityServerRepository = identityServerRepository;
        }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            bool allowedOrigin = false;

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "identityserver_globalSettings";
            IEnumerable<GlobalSetting> globalSettings = rpcache.GetFromCache<IEnumerable<GlobalSetting>>(cacheKey, 300, () =>
            {
                // load from api
                return _identityServerRepository.GetGlobalSettings().Where(p => p.SettingName.Equals(IdentityServerCORSAllowedOrigins, StringComparison.OrdinalIgnoreCase));
            });

            // check to see if the incoming origin is in the allowed list of domains for this server
            foreach (var gs in globalSettings)
            {
                if (!gs.Value.Contains("*") && gs.Value.Equals(origin, StringComparison.OrdinalIgnoreCase))
                {
                    allowedOrigin = true;
                }
                else
                {
                    if (origin.Contains(gs.Value.Replace("*", "")))
                    {
                        allowedOrigin = true;
                    }
                }
            }

            return Task.FromResult<bool>(allowedOrigin);
        }
    }
}