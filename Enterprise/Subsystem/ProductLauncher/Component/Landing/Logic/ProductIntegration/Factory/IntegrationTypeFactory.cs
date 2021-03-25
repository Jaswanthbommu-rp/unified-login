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

        private readonly IManageUnifiedLogin _manageUnifiedLogin;

        private readonly IManageProductOneSite _manageProductOneSite;

        private readonly DefaultUserClaim _userClaims;

        public DefaultIntegrationTypeFactory(IProductInternalSettingRepository productInternalSettingRepository, IManageUnifiedLogin manageUnifiedLogin,
            IManageProductOneSite manageProductOneSite, DefaultUserClaim userClaims)
        {
            _productInternalSettingRepository = productInternalSettingRepository;
            _manageUnifiedLogin = manageUnifiedLogin;
            _manageProductOneSite = manageProductOneSite;
            _userClaims = userClaims;
        }

        private delegate IIntegrationType FactoryInitMethod(int productId, DefaultUserClaim userClaims, IManageUnifiedLogin manageUnifiedLogin,
            IManageProductOneSite manageProductOneSite, IProductInternalSettingRepository productInternalSettingRepository);

        private readonly IReadOnlyDictionary<string, FactoryInitMethod> _integrationTypeMap =
            new Dictionary<string, FactoryInitMethod>(StringComparer.OrdinalIgnoreCase)
            {
                ["Legacy"] = (productId, userClaims, manageUnifiedLogin, manageProductOneSite, productInternalSettingRepository) =>
                    new LegacyIntegrationType(productId, userClaims, manageUnifiedLogin, manageProductOneSite, productInternalSettingRepository),
                ["UPFM"] = (productId, userClaims, _1, _2, _3) => new UPFMIntegrationType(productId, userClaims),
                ["Standard v1"] = (_1, _2, _3, _4, _5) => new StandardV1IntegrationType()
            };

        public IIntegrationType GetIntegration(int productId)
        {
            string integrationType = GetIntegrationTypeForProductId(productId);

            IIntegrationType ret = null;
            if (_integrationTypeMap.ContainsKey(integrationType))
            {
                ret = _integrationTypeMap[integrationType](productId, _userClaims, _manageUnifiedLogin, _manageProductOneSite, _productInternalSettingRepository);
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