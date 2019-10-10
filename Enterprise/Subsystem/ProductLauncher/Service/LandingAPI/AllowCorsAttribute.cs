using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI
{
	/// <summary>
	/// Used to allow only certain domains to use the apis
	/// </summary>
	public class AllowCorsAttribute : Attribute, ICorsPolicyProvider
    {
        private static string LandingApiCorsAllowedOrigins = "LANDINGAPICORSALLOWEDORIGINS";
        private CorsPolicy _policy;

		/// <summary>
		/// Used to get the allowed domains for the system
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var retval = new CorsPolicy();
			retval.AllowAnyHeader = true;
			retval.AllowAnyMethod = true;
			retval.AllowAnyOrigin = false;
			retval.PreflightMaxAge = 600;
			retval.SupportsCredentials = true;

			IEnumerable<GlobalSetting> globalSettings = new List<GlobalSetting>();
			ObjectCache globalSettingsCache = MemoryCache.Default;
			globalSettings = globalSettingsCache["landingapi_globalSettings"] as IList<GlobalSetting>;
			if (globalSettings == null)
			{
				GlobalSettingRepository gsr = new GlobalSettingRepository();
				globalSettings = gsr.GetGlobalSettings();
				CacheItemPolicy policy = new CacheItemPolicy();
				policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(300);
				globalSettingsCache.Set("landingapi_globalSettings", globalSettings, policy);
			}

			if (globalSettings.Any(a => (a.SettingName.ToUpper() == LandingApiCorsAllowedOrigins)))
			{
				foreach (GlobalSetting gs in globalSettings)
				{
					if (gs.SettingName.ToUpper() == LandingApiCorsAllowedOrigins)
					{
						retval.Origins.Add(gs.Value);
					}
				}
			}

			_policy = retval;
			return Task.FromResult(_policy);
		}
    }
}
