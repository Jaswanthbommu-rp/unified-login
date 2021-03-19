using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public class LegacyIntegrationType : IIntegrationType
    {
        private readonly int _productId;

        private readonly DefaultUserClaim _userClaims;

        private readonly IManageUnifiedLogin _manageUnifiedLogin;

        private readonly IManageProductOneSite _manageProductOneSite;

        private readonly IProductInternalSettingRepository _productInternalSettingRepository;

        public LegacyIntegrationType(int productId, DefaultUserClaim userClaims, IManageUnifiedLogin manageUnifiedLogin,
            IManageProductOneSite manageProductOneSite, IProductInternalSettingRepository productInternalSettingRepository)
        {
            _productId = productId;
            _userClaims = userClaims;
            _manageUnifiedLogin = manageUnifiedLogin;
            _manageProductOneSite = manageProductOneSite;
            _productInternalSettingRepository = productInternalSettingRepository;
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter)
        {
            // These were params that were never used by callers, removed to simplify method signature
            bool assignedOnly = false;
            string userLoginName = "";

            ListResponse result = new ListResponse();

            string productcode = ProductEnumHelper.StringValueOf((ProductEnum)_productId);
            switch (_productId)
            {
                case (int)ProductEnum.OneSite:
                    if (userPersonaId > 0)
                    {
                        result = _manageProductOneSite.GetOneSiteRoleList(editorPersonaId, userPersonaId, assignedOnly, dataFilter);
                    }
                    else
                    {
                        result = _manageProductOneSite.GetOneSiteRoleListAll(editorPersonaId, dataFilter);
                    }
                    break;

                case (int)ProductEnum.MarketingCenter:
                    ManageProductMarketingCenter mg = new ManageProductMarketingCenter(_userClaims);
                    result = mg.GetRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.FinancialSuite:
                    IManageProductOneSiteAccounting mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    result = mangeProductOneSiteAccounting.GetUserRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.OpsBuyer:
                    IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
                    result = manageProductOps.GetRoles(editorPersonaId, userPersonaId, "", dataFilter);
                    break;

                case (int)ProductEnum.VendorServices:
                    IManageProductVendorServices manageProductVendorServices = new ManageProductVendorServices(_userClaims);
                    result = manageProductVendorServices.GetRoles(editorPersonaId, userPersonaId, (accessType ?? AccessType.Property), dataFilter);
                    break;

                case (int)ProductEnum.ClientPortal:
                    IManageProductClientPortal _manageProductClientPortal = new ManageProductClientPortal(_userClaims);
                    result = _manageProductClientPortal.GetRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.Lead2Lease:
                    IManageProductLead2Lease manageProductLead2Lease = new ManageProductLead2Lease(_userClaims);
                    result = manageProductLead2Lease.GetRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.ResidentPortal:
                    ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
                    result = manageProductResidentPortal.ListLevelsResponse(editorPersonaId, userPersonaId);
                    break;

                case (int)ProductEnum.OnSite:
                    var manageProductOnSite = new ManageProductOnSite(_userClaims);
                    result = manageProductOnSite.GetRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.Insurance:
                    ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(_userClaims);
                    result = manageProductRentersInsurance.ListRolesResponse(editorPersonaId, userPersonaId);
                    break;

                case (int)ProductEnum.UtilityManagement:
                    var manageProductRum = new ManageProductRum(_userClaims);
                    result = manageProductRum.GetUMGlobalRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.UnifiedAmenities:
                    IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
                    result = manageUnifiedAmenities.GetRoles(editorPersonaId, userPersonaId, partyId);
                    break;

                case (int)ProductEnum.ResearchApplication:
                    ManageResearchApplication manageResearchApplication = new ManageResearchApplication(_userClaims);
                    result = manageResearchApplication.GetRoles(editorPersonaId, userPersonaId, partyId);
                    break;

                case (int)ProductEnum.AoBusinessIntelligence:
                case (int)ProductEnum.AoInvestmentAnalytics:
                case (int)ProductEnum.AoPerformanceAnalytics:
                case (int)ProductEnum.AoRevenueManagement:
                case (int)ProductEnum.AoBenchmarking:
                case (int)ProductEnum.AoLeaseRentOption:
                case (int)ProductEnum.AoAmenityOptimization:
                case (int)ProductEnum.AoAIRevenueManagement:
                case (int)ProductEnum.AoRentControl:
                case (int)ProductEnum.AoMarketAnalytics:
                case (int)ProductEnum.AoAxiometrics:
                    var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                    result = manageProductAo.GetProductRoles(editorPersonaId, userPersonaId, productcode, dataFilter, userLoginName);
                    break;

                case (int)ProductEnum.LeadManagement:
                    var productLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productLMLogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.LeadAnalytics:

                    var productLALogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadAnalytics, editorPersonaId, userPersonaId, _userClaims);
                    result = productLALogic.GetProductRoles(dataFilter);

                    break;

                case (int)ProductEnum.IntegrationMarketplace:
                    var manageProductIntegartionMarketplace = new ManageProductIntegrationMarketplace(_userClaims);
                    result = manageProductIntegartionMarketplace.GetRoles(editorPersonaId, userPersonaId, partyId);
                    break;

                case (int)ProductEnum.RPDocumentManagement:
                    var manageProductRPDocumentManagement = new ManageProductRPDocumentManagement(_userClaims);
                    result = manageProductRPDocumentManagement.GetPropertyRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.PortfolioManagement:
                    var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productPMLogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.DepositAlternative:
                    var productDALogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, editorPersonaId, userPersonaId, _userClaims);
                    result = productDALogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.UnifiedPlatform:
                    result = _manageUnifiedLogin.GetUserRolesWithRights(editorPersonaId, userPersonaId, partyId);
                    break;

                case (int)ProductEnum.RenovationManager:
                    var productRMLogic = ManageProductFactory.GetProductLogic(ProductEnum.RenovationManager, editorPersonaId, userPersonaId, _userClaims);
                    result = productRMLogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.SeniorLeadManagement:
                    var productSLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.SeniorLeadManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productSLMLogic.GetAllRights(dataFilter);
                    break;

                case (int)ProductEnum.ClickPay:
                    var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.ClickPay, editorPersonaId, userPersonaId, _userClaims);
                    result = productLogic.GetProductRoles(null);
                    break;

                case (int)ProductEnum.AssetOptimizer:
                case (int)ProductEnum.CIMPL:
                case (int)ProductEnum.EasyLMS:
                case (int)ProductEnum.HelpCenter:
                case (int)ProductEnum.MigrationTool:
                case (int)ProductEnum.OmniChannel:
                case (int)ProductEnum.OneSiteConversions:
                case (int)ProductEnum.OpsBid:
                case (int)ProductEnum.ProductLearningPortal:
                case (int)ProductEnum.ProductUpdates:
                case (int)ProductEnum.PropertyPhotos:
                case (int)ProductEnum.Propertyware:
                case (int)ProductEnum.ProspectContactCenter:
                case (int)ProductEnum.SalesForce:
                case (int)ProductEnum.SelfProvisioningPortal:
                case (int)ProductEnum.SettingsManagement:
                case (int)ProductEnum.SiteSpendManagement:
                case (int)ProductEnum.Social:
                case (int)ProductEnum.SupportTool:
                case (int)ProductEnum.UnifiedSettings:
                case (int)ProductEnum.UnifiedUI:
                case (int)ProductEnum.VendorMarketplace:
                case (int)ProductEnum.Yieldstar:
                case (int)ProductEnum.PMEDasboard:
                    //pending implementation
                    throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);
                default:
                    break;
            }

            return result;
        }

        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter)
        {
            // These were params that were never used by callers, removed to simplify method signature
            bool assignedOnly = false;
            string userLoginName = "";

            ListResponse result = new ListResponse();

            string productcode = ProductEnumHelper.StringValueOf((ProductEnum)_productId);
            switch (_productId)
            {
                case (int)ProductEnum.OneSite:
                    result = _manageProductOneSite.GetOneSitePropertyList(editorPersonaId, userPersonaId, assignedOnly, dataFilter);
                    break;

                case (int)ProductEnum.MarketingCenter:
                    ManageProductMarketingCenter mg = new ManageProductMarketingCenter(_userClaims);
                    result = mg.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.FinancialSuite:
                    IManageProductOneSiteAccounting mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    result = mangeProductOneSiteAccounting.GetUserPropertiesNew(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.OpsBuyer:
                    IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
                    result = manageProductOps.GetCompanyAssets(editorPersonaId, userPersonaId, assignedOnly, dataFilter);
                    break;

                case (int)ProductEnum.VendorServices:
                    IManageProductVendorServices manageProductVendorServices = new ManageProductVendorServices(_userClaims);
                    result = manageProductVendorServices.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.ClientPortal:
                    IManageProductClientPortal _manageProductClientPortal = new ManageProductClientPortal(_userClaims);
                    result = _manageProductClientPortal.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.ProspectContactCenter:
                    IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(_userClaims);
                    result = manageProductProspectContact.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.Lead2Lease:
                    IManageProductLead2Lease manageProductLead2Lease = new ManageProductLead2Lease(_userClaims);
                    result = manageProductLead2Lease.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.ResidentPortal:
                    ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
                    result = manageProductResidentPortal.ListProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.OnSite:
                    var manageProductOnSite = new ManageProductOnSite(_userClaims);
                    result = manageProductOnSite.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.Insurance:
                    ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(_userClaims);
                    result = manageProductRentersInsurance.ListProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.UtilityManagement:
                    var manageProductRum = new ManageProductRum(_userClaims);
                    result = manageProductRum.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.UnifiedAmenities:
                    IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
                    result = manageUnifiedAmenities.GetProperties(editorPersonaId, userPersonaId, assignedOnly, dataFilter);
                    break;

                case (int)ProductEnum.AoBusinessIntelligence:
                case (int)ProductEnum.AoInvestmentAnalytics:
                case (int)ProductEnum.AoAxiometrics:
                case (int)ProductEnum.AoPerformanceAnalytics:
                case (int)ProductEnum.AoRevenueManagement:
                case (int)ProductEnum.AoBenchmarking:
                case (int)ProductEnum.AoLeaseRentOption:
                case (int)ProductEnum.AoAmenityOptimization:
                case (int)ProductEnum.AoAIRevenueManagement:
                case (int)ProductEnum.AoRentControl:
                    var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                    result = manageProductAo.GetProductProperties(editorPersonaId, userPersonaId, productcode, dataFilter, userLoginName);
                    break;

                case (int)ProductEnum.LeadManagement:
                    var productLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productLMLogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.LeadAnalytics:
                    var productLALogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadAnalytics, editorPersonaId, userPersonaId, _userClaims);
                    result = productLALogic.GetProductProperties(dataFilter);
                    break;
                //case (int)ProductEnum.RPDocumentManagement:

                //	break;
                case (int)ProductEnum.PortfolioManagement:
                    var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productPMLogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.DepositAlternative:
                    var productDALogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, editorPersonaId, userPersonaId, _userClaims);
                    result = productDALogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.UnifiedPlatform:
                    bool usePropertyInstanceUnifiedLogin = false;

                    var rpcache = new RPObjectCache();
                    var cacheKey = $"productInternalSettingPanel_{(int)ProductEnum.UnifiedPlatform}";
                    var _productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60,
                        () => _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform).ToList()
                    );

                    if (_productInternalSettingList.Any(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase)))
                    {
                        usePropertyInstanceUnifiedLogin = (_productInternalSettingList.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase))?.Value == "1");
                    }

                    if (!usePropertyInstanceUnifiedLogin)
                    {
                        result = _manageUnifiedLogin.GetProperties(editorPersonaId, userPersonaId, false, dataFilter);
                    }
                    else
                    {
                        result = _manageUnifiedLogin.GetUPFMProperties(editorPersonaId, userPersonaId, false, ProductEnum.UnifiedPlatform, dataFilter);
                    }

                    break;

                case (int)ProductEnum.RenovationManager:
                    var productRMLogic = ManageProductFactory.GetProductLogic(ProductEnum.RenovationManager, editorPersonaId, userPersonaId, _userClaims);
                    result = productRMLogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.SeniorLeadManagement:
                    var productSLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.SeniorLeadManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productSLMLogic.GetProductProperties(dataFilter);
                    break;

                default:
                    break;
            }

            return result;
        }
    }
}