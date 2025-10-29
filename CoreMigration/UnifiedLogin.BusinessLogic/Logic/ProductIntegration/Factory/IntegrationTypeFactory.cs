using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Types;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory
{
    public class IntegrationTypeFactory : IIntegrationTypeFactory
    {
        private readonly IManageProduct _manageProduct;

        private readonly IManageUnifiedLogin _manageUnifiedLogin;

        private readonly IManageProductOneSite _manageProductOneSite;

        private readonly IProductRepository _productRepository;

        private readonly IProductInternalSettingRepository _productInternalSettingRepository;

        private readonly DefaultUserClaim _userClaims;

        public IntegrationTypeFactory(IManageProduct manageProduct, IManageUnifiedLogin manageUnifiedLogin,
            IManageProductOneSite manageProductOneSite, IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository, DefaultUserClaim userClaims)
        {
            _manageProduct = manageProduct;
            _manageUnifiedLogin = manageUnifiedLogin;
            _manageProductOneSite = manageProductOneSite;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _userClaims = userClaims;
        }

        private delegate IIntegrationType FactoryInitMethod(int productId, DefaultUserClaim userClaims, IManageUnifiedLogin manageUnifiedLogin,
            IManageProductOneSite manageProductOneSite, IManageProduct manageProduct, IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository);

        private static readonly IReadOnlyDictionary<ProductIntegrationTypeEnum, FactoryInitMethod> _integrationTypeMap =
            new Dictionary<ProductIntegrationTypeEnum, FactoryInitMethod>()
            {
                [ProductIntegrationTypeEnum.Legacy] = (productId, userClaims, manageUnifiedLogin, manageProductOneSite, manageProduct, productRepository, productInternalSettingRepository) =>
                    new LegacyIntegrationType(productId, userClaims, manageUnifiedLogin, manageProductOneSite, manageProduct, productRepository, productInternalSettingRepository),
                [ProductIntegrationTypeEnum.UPFM] = (productId, userClaims, _1, _2, _3, _4, productInternalSettingRepository) => new UPFMIntegrationType(productId, userClaims, productInternalSettingRepository),
                [ProductIntegrationTypeEnum.StandardV1] = (productId, userClaims, _1, _2, _3, productRepository, productInternalSettingRepository) => new StandardV1IntegrationType(productId, userClaims, productRepository, productInternalSettingRepository)
            };

        public IIntegrationType GetIntegration(int productId)
        {
            var integrationType = GetIntegrationTypeForProductId(productId);

            IIntegrationType ret = null;
            if (_integrationTypeMap.ContainsKey(integrationType))
            {
                ret = _integrationTypeMap[integrationType](productId, _userClaims, _manageUnifiedLogin, _manageProductOneSite, _manageProduct, _productRepository, _productInternalSettingRepository);
            }
            return ret;
        }

        public IIntegrationType GetIntegrationStandardV1(int productId)
        {
            var integrationType = ProductIntegrationTypeEnum.StandardV1;

            IIntegrationType ret = null;
            if (_integrationTypeMap.ContainsKey(integrationType))
            {
                ret = _integrationTypeMap[integrationType](productId, _userClaims, _manageUnifiedLogin, _manageProductOneSite, _manageProduct, _productRepository, _productInternalSettingRepository);
            }
            return ret;
        }

        private static readonly IReadOnlyDictionary<string, ProductIntegrationTypeEnum> _integrationTypeEnumMap =
            new Dictionary<string, ProductIntegrationTypeEnum>(StringComparer.OrdinalIgnoreCase)
            {
                ["Legacy"] = ProductIntegrationTypeEnum.Legacy,
                ["UPFM"] = ProductIntegrationTypeEnum.UPFM,
                ["Standard v1"] = ProductIntegrationTypeEnum.StandardV1
            };

        public ProductIntegrationTypeEnum GetIntegrationTypeForProductId(int productId)
        {
            var productIntegrationTypeList = _manageProduct.GetProductSettingByType("ProductIntegrationType");
            var integrationTypeSettingValue = productIntegrationTypeList.FirstOrDefault(w => w.ProductId == productId)?.Value;

            var integrationType = ProductIntegrationTypeEnum.Legacy;
            if (integrationTypeSettingValue != null
                && _integrationTypeEnumMap.ContainsKey(integrationTypeSettingValue))
            {
                integrationType = _integrationTypeEnumMap[integrationTypeSettingValue];
            }

            return integrationType;
        }
    }
}