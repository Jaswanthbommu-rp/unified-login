using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory
{
    public class DefaultIntegrationTypeFactory : IIntegrationTypeFactory
    {
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;

        public DefaultIntegrationTypeFactory(IProductInternalSettingRepository productInternalSettingRepository, IManageUnifiedLogin manageUnifiedLogin,
            IManageProductOneSite manageProductOneSite)
        {
            _productInternalSettingRepository = productInternalSettingRepository;

            // TODO: Would be more performant to move this init to a static field instead
            _integrationTypeMap = new Dictionary<string, Func<int, DefaultUserClaim, IIntegrationType>>(StringComparer.OrdinalIgnoreCase)
            {
                ["Legacy"] = (productId, userClaims) => new LegacyIntegrationType(productId, userClaims, manageUnifiedLogin, manageProductOneSite, productInternalSettingRepository),
                ["UPFM"] = (productId, userClaims) => new UPFMIntegrationType(productId, userClaims),
                ["Standard v1"] = (productId, userClaims) => new StandardV1IntegrationType()
            };
        }

        private readonly IReadOnlyDictionary<string, Func<int, DefaultUserClaim, IIntegrationType>> _integrationTypeMap;

        public IIntegrationType GetIntegration(int productId, DefaultUserClaim userClaims)
        {
            string integrationType = GetIntegrationTypeForProductId(productId);

            IIntegrationType ret = null;
            if (_integrationTypeMap.ContainsKey(integrationType))
            {
                ret = _integrationTypeMap[integrationType](productId, userClaims);
            }
            return ret;
        }

        private string GetIntegrationTypeForProductId(int productId)
        {
            var productIntegrationTypeList = _productInternalSettingRepository.GetProductSettingByType("ProductIntegrationType");
            var integrationType = productIntegrationTypeList.FirstOrDefault(w => w.ProductId == productId)?.Value;

            if (string.IsNullOrWhiteSpace(integrationType))
            {
                integrationType = "Legacy";
            }

            return integrationType;
        }
    }
}