using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using Serilog;
using Serilog.Events;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    public class ManageSameSite
    {
        #region Private Variables

        private IIdentityServerRepository _identityServerRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;

        #endregion

        public ManageSameSite()
        {
            _identityServerRepository = new IdentityServerRepository();
            _productInternalSettingRepository = new Component.Landing.Repository.ProductInternalSettingRepository();
        }

        public ManageSameSite(IIdentityServerRepository identityServerRepository)
        {
            _identityServerRepository = identityServerRepository;
        }

        private List<SameSiteExclusion> GetSameSiteExclusionList()
        {
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "getSameSiteExclusionList";

            List<SameSiteExclusion> scopes = rpcache.GetFromCache<List<SameSiteExclusion>>(cacheKey, 30, () => _identityServerRepository.GetSameSiteExclusionList().ToList());

            return scopes;
        }

        private IList<ProductInternalSetting> GetProductInternalSettings(int productId, int expirationSeconds)
        {
            IList<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>();

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSetting_" + productId;
            productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, expirationSeconds, () => _productInternalSettingRepository.GetProductInternalSettings(productId));
            return productInternalSettingList;
        }

        private static bool CompareAgent(string userAgent, string comparator, string compareValue)
        {
            switch (comparator.ToLowerInvariant())
            {
                case "contains":
                    if (userAgent.Contains(compareValue))
                    {
                        return true;
                    }

                    break;
                case "startswith":
                    if (userAgent.StartsWith(compareValue))
                    {
                        return true;
                    }

                    break;
                case "equals":
                    if (userAgent.Equals(compareValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    break;
                case "endswith":
                    if (userAgent.EndsWith(compareValue, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }

        public static bool SuppressSameSiteNoneCookies(OwinContext context, string userAgentInput)
        {
            var correlationId = Guid.NewGuid().ToString();
            ManageSameSite manageSameSite = new ManageSameSite();
            var settingList = manageSameSite.GetProductInternalSettings((int) ProductEnum.UnifiedPlatform, 30);
            if (settingList.Any(p => p.Name.Equals("IsSuppressSameSiteEnabled", StringComparison.OrdinalIgnoreCase)))
            {
                var IsSuppressSameSiteEnabled = settingList.FirstOrDefault(p => p.Name.Equals("IsSuppressSameSiteEnabled", StringComparison.OrdinalIgnoreCase)).Value;
                if (IsSuppressSameSiteEnabled == "0")
                {
                    return true;
                }
            }
            else
            {
                return true;
            }

            List<SameSiteExclusion> excludeBrowserDetails = manageSameSite.GetSameSiteExclusionList();
            Dictionary<string, object> info = new Dictionary<string, object>();

            string userAgent = null;
            if (context != null)
            {
                userAgent = context.Request.Headers["User-Agent"].ToString();
                info.Add("context.Request.Headers", context.Request.Headers);
            }
            else
            {
                userAgent = userAgentInput;
            }

            info.Add("userAgent", userAgent);
#if (DEBUG)
            Log.Write(LogEventLevel.Debug, "IdentityServerConfig.SuppressSameSiteNoneCookies", new LogDetails() {CorrelationId = correlationId, Message = $"IdentityServerConfig.SuppressSameSiteNoneCookies", AdditionalInfo = info});
#endif
            List<bool> andResults = new List<bool>();

            for (var i = 0; i < excludeBrowserDetails.Count; i++)
            {
                SameSiteExclusion current = excludeBrowserDetails[i];
                if (i < excludeBrowserDetails.Count && current.LogicalOperator != null)
                {
                    SameSiteExclusion next = null;
                    andResults.Add(CompareAgent(userAgent, current.ComparatorLeft, current.SameSiteValueLeft));
                    while (i < (excludeBrowserDetails.Count - 1) && excludeBrowserDetails[i + 1].SameSiteValueLeft == current.SameSiteValueRight)
                    {
                        next = excludeBrowserDetails[i + 1];
                        andResults.Add(CompareAgent(userAgent, next.ComparatorLeft, next.SameSiteValueLeft));
                        i++;
                    }

                    if (i < excludeBrowserDetails.Count - 1)
                    {
                        next = excludeBrowserDetails[i + 1];
                        andResults.Add(CompareAgent(userAgent, next.ComparatorLeft, next.SameSiteValueLeft));
                    }

                    i++;

                    if (andResults.All(p => p == true))
                    {
#if (DEBUG)
                        Log.Write(LogEventLevel.Debug, "IdentityServerConfig.SuppressSameSiteNoneCookies - true andResults.All", new LogDetails() {CorrelationId = correlationId, Message = $"IdentityServerConfig.SuppressSameSiteNoneCookies - true andResults.All", AdditionalInfo = info});
#endif
                        return true;
                    }

                    andResults = new List<bool>();
                }

                if (current.LogicalOperator == null && CompareAgent(userAgent, current.ComparatorLeft, current.SameSiteValueLeft))
                {
#if (DEBUG)
                    Log.Write(LogEventLevel.Debug, "IdentityServerConfig.SuppressSameSiteNoneCookies - true CompareAgent", new LogDetails() {CorrelationId = correlationId, Message = $"IdentityServerConfig.SuppressSameSiteNoneCookies - true CompareAgent", AdditionalInfo = info});
#endif
                    return true;
                }
            }
#if (DEBUG)
            Log.Write(LogEventLevel.Debug, "IdentityServerConfig.SuppressSameSiteNoneCookies - false", new LogDetails() {CorrelationId = correlationId, Message = $"IdentityServerConfig.SuppressSameSiteNoneCookies - false", AdditionalInfo = info});
#endif
            return false;
        }
    }
}
