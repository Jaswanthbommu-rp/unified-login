using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace UnifiedLogin.BusinessLogic.Attributes
{
    /// <summary>
    /// Attribute container used to define which GlobalSetting keys correspond to allowed CORS origins.
    /// Provides a method to build a CorsPolicy dynamically for ASP.NET Core.
    /// </summary>
    public class AllowCorsAttribute : Attribute
    {
        public string[] OriginsSettingKeys { get; }

        public AllowCorsAttribute(params string[] originsSettingKeys)
        {
            OriginsSettingKeys = originsSettingKeys ?? Array.Empty<string>();
        }

        /// <summary>
        /// Builds a dynamic CorsPolicy based on GlobalSettings and the incoming request Origin header.
        /// </summary>
        public Task<CorsPolicy> BuildPolicyAsync(HttpContext context)
        {
            var allowedOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            context.Request.Headers.TryGetValue("Origin", out var originHeaderValues);
            var requestOrigins = originHeaderValues.Count > 0 ? originHeaderValues.ToArray() : Array.Empty<string>();

            RPObjectCache rpcache = new RPObjectCache();
            const string cacheKey = "apicors_globalSettings";
            var globalSettings = rpcache.GetFromCache<IEnumerable<GlobalSetting>>(cacheKey, 300, () =>
            {
                var gsr = new GlobalSettingRepository();
                var allSettings = gsr.GetGlobalSettings() ?? Enumerable.Empty<GlobalSetting>();
                return allSettings.Where(p => OriginsSettingKeys.Contains(p.SettingName, StringComparer.OrdinalIgnoreCase));
            }) ?? Enumerable.Empty<GlobalSetting>();

            foreach (var gs in globalSettings)
            {
                var value = gs.Value?.Trim();
                if (string.IsNullOrWhiteSpace(value)) continue;

                if (value.Contains('*'))
                {
                    var pattern = value.Replace("*", ""); // simple wildcard removal
                    foreach (var origin in requestOrigins)
                    {
                        if (origin.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            allowedOrigins.Add(origin);
                        }
                    }
                }
                else
                {
                    allowedOrigins.Add(value);
                }
            }

            var builder = new CorsPolicyBuilder();
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
            builder.SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            builder.AllowCredentials();

            if (allowedOrigins.Count > 0)
            {
                builder.WithOrigins(allowedOrigins.ToArray());
            }
            // else: do not call AllowAnyOrigin when credentials are allowed

            return Task.FromResult(builder.Build());
        }
    }
}