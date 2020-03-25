using IdentityServer3.Core.Configuration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    public class ManageLoggingOptions
    {
        private IIdentityServerRepository _identityServerRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;

        public ManageLoggingOptions()
        {
            _identityServerRepository = new IdentityServerRepository();
            _productInternalSettingRepository = new Component.Landing.Repository.ProductInternalSettingRepository();
        }

        public ManageLoggingOptions(IIdentityServerRepository identityServerRepository)
        {
            _identityServerRepository = identityServerRepository;
        }

        public static int GetIdentityServerLogEventLevel()
        {
            ManageLoggingOptions manageOptions = new ManageLoggingOptions();
            var settingList = manageOptions.GetProductInternalSettings((int) ProductEnum.UnifiedLogin, 30);
            if (settingList != null && settingList.Any(p => p.Name.Equals("IdentityServerLogEventLevel", StringComparison.OrdinalIgnoreCase)))
            {
                return Convert.ToInt16(settingList.FirstOrDefault(p => p.Name.Equals("IdentityServerLogEventLevel", StringComparison.OrdinalIgnoreCase))?.Value);
            }

            return 2;
        }

        public static LoggingOptions GetLoggingOptions()
        {
            var correlationId = Guid.NewGuid().ToString();
            LoggingOptions options = new LoggingOptions();
            ManageLoggingOptions manageOptions= new ManageLoggingOptions();

            options.EnableWebApiDiagnostics = manageOptions.GetLoggingSetting("IDS_EnableWebApiDiagnostics");
            options.EnableKatanaLogging = manageOptions.GetLoggingSetting("IDS_EnableKatanaLogging");
            options.EnableHttpLogging = manageOptions.GetLoggingSetting("IDS_EnableHttpLogging");
            options.WebApiDiagnosticsIsVerbose = manageOptions.GetLoggingSetting("IDS_WebApiDiagnosticsIsVerbose");

            return options;
        }

        private IList<ProductInternalSetting> GetProductInternalSettings(int productId, int expirationSeconds)
        {
            IList<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>();

            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "productInternalSetting_" + productId;
            productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, expirationSeconds, () => _productInternalSettingRepository.GetProductInternalSettings(productId));
            return productInternalSettingList;
        }

        public bool GetLoggingSetting(string settingName)
        {
            ManageLoggingOptions manageOptions = new ManageLoggingOptions();
            var settingList = manageOptions.GetProductInternalSettings((int) ProductEnum.UnifiedLogin, 30);
            if (settingList != null && settingList.Any(p => p.Name.Equals(settingName, StringComparison.OrdinalIgnoreCase)))
            {
                var settingValue = settingList.FirstOrDefault(p => p.Name.Equals(settingName, StringComparison.OrdinalIgnoreCase))?.Value;
                if (settingValue == "1")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
