using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.AdminSupportPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ClientPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProspectContactCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RentersInsurance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResearchApplication;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.SelfProvisioningPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.VendorServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    [Obsolete]
    public class LegacyIntegrationType : IIntegrationType
    {
        private readonly int _productId;

        private readonly DefaultUserClaim _userClaims;

        private readonly IManageUnifiedLogin _manageUnifiedLogin;

        private readonly IManageProductOneSite _manageProductOneSite;

        private readonly IManageProduct _manageProduct;

        private readonly IProductRepository _productRepository;

        private readonly IProductInternalSettingRepository _productInternalSettingRepository;

        public LegacyIntegrationType(int productId, DefaultUserClaim userClaims, IManageUnifiedLogin manageUnifiedLogin,
            IManageProductOneSite manageProductOneSite, IManageProduct manageProduct, IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository)
        {
            _productId = productId;
            _userClaims = userClaims;
            _manageUnifiedLogin = manageUnifiedLogin;
            _manageProductOneSite = manageProductOneSite;
            _manageProduct = manageProduct;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter)
        {
            // These were params that were never used by callers, removed to simplify method signature
            bool assignedOnly = false;
            string userLoginName = "";

            ListResponse result = new ListResponse();

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

                case (int)ProductEnum.AdminSupportPortal:
                    IManageProductAdminSupportPortal _manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(_userClaims);
                    result = _manageProductAdminSupportPortal.GetRoles(editorPersonaId, userPersonaId, dataFilter);
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
                    result = manageProductRum.GetRoles(editorPersonaId, userPersonaId, dataFilter);
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
                case (int)ProductEnum.AoBIX:
                    var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                    var products = _productRepository.GetAllProducts();
                    string productCode = ProductEnumHelper.GetBooksSourceCodeByProductId(_productId, products);
                    result = manageProductAo.GetProductRoles(editorPersonaId, userPersonaId, productCode, dataFilter, userLoginName);
                    break;

                case (int)ProductEnum.LeadManagement:
                    var productLMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLMLogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.LeadAnalytics:

                    var productLALogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLALogic.GetProductRoles(dataFilter);

                    break;

                case (int)ProductEnum.RPDocumentManagement:
                    var manageProductRPDocumentManagement = new ManageProductRPDocumentManagement(_userClaims);
                    result = manageProductRPDocumentManagement.GetPropertyRoles(editorPersonaId, userPersonaId, dataFilter);
                    break;

                case (int)ProductEnum.PortfolioManagement:
                    var productPMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productPMLogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.DepositAlternative:
                    var productDALogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productDALogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.UnifiedPlatform:
                    result = _manageUnifiedLogin.GetUserRolesWithRights(editorPersonaId, userPersonaId, partyId);
                    break;

                case (int)ProductEnum.RenovationManager:
                    var productRMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productRMLogic.GetProductRoles(dataFilter);
                    break;

                case (int)ProductEnum.SeniorLeadManagement:
                    var productSLMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productSLMLogic.GetAllRights(dataFilter);
                    break;

                case (int)ProductEnum.ClickPay:
                    var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLogic.GetProductRoles(null);
                    break;
                case (int)ProductEnum.RealConnect:
                    ManageProductRealConnect rc = new ManageProductRealConnect(_userClaims);
                    result = rc.GetRoles(editorPersonaId, userPersonaId, dataFilter);
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
                case (int)ProductEnum.ProductUpdatesDashboard:
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
                case (int)ProductEnum.ESupply:
                case (int)ProductEnum.ManagedServices:
                case (int)ProductEnum.TrustDashboard:
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

                case (int)ProductEnum.AdminSupportPortal:
                    IManageProductAdminSupportPortal _manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(_userClaims);
                    result = _manageProductAdminSupportPortal.GetProperties(editorPersonaId, userPersonaId, dataFilter);
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
                case (int)ProductEnum.AoBIX:
                    var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                    var productList = _productRepository.GetAllProducts();
                    string productcode = ProductEnumHelper.GetBooksSourceCodeByProductId(_productId, productList);
                    result = manageProductAo.GetProductProperties(editorPersonaId, userPersonaId, productcode, dataFilter, userLoginName);
                    break;

                case (int)ProductEnum.LeadManagement:
                    var productLMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLMLogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.LeadAnalytics:
                    var productLALogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLALogic.GetProductProperties(dataFilter);
                    break;
                //case (int)ProductEnum.RPDocumentManagement:

                //	break;
                case (int)ProductEnum.PortfolioManagement:
                    var productPMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productPMLogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.DepositAlternative:
                    var productDALogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productDALogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.UnifiedPlatform:
                    bool usePropertyInstanceUnifiedLogin = false;

                    var _productInternalSettingList = _manageProduct.GetProductInternalSettings(_productId);

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
                    var productRMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productRMLogic.GetProductProperties(dataFilter);
                    break;

                case (int)ProductEnum.SeniorLeadManagement:
                    var productSLMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productSLMLogic.GetProductProperties(dataFilter);
                    break;
                case (int)ProductEnum.RealConnect:
                    ManageProductRealConnect rc = new ManageProductRealConnect(_userClaims);
                    result = rc.GetProperties(editorPersonaId, userPersonaId, dataFilter);
                    break;

                default:
                    break;
            }

            return result;
        }

        public ListResponse GetEnterpriseProperties(long userPersonaId, RequestParameter dataFilter) => GetProperties(userPersonaId, userPersonaId, dataFilter);

        public ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter)
        {
            ListResponse result = new ListResponse();

            switch (_productId)
            {
                case (int)ProductEnum.OneSite:
                    result = _manageProductOneSite.GetOneSiteRights(editorPersonaId, dataFilter, roleId, assignedToRoleOnly);
                    break;

                case (int)ProductEnum.UnifiedPlatform:
                    result = _manageUnifiedLogin.GetRightsByRole(editorPersonaId, partyId, roleId);
                    break;

                //case (int)ProductEnum.UnifiedAmenities:
                //    IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
                //    result = manageUnifiedAmenities.GetRightsByRole(editorPersonaId, partyId, roleId);
                //    break;

                default:
                    break;
            }
            return result;
        }

        public ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, string roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter)
        {
            ListResponse result = new ListResponse();

            return result;
        }

        public string CreateUser(ProductUserProperitiesRoles productUser, out List<AdditionalParameters> additionalParameters)
        {
            additionalParameters = null;
            string result;
            IProduct product;
            object productPropertiesRoles;

            switch (_productId)
            {
                case (int)ProductEnum.OneSite:

                    product = new OneSiteProduct(_userClaims);

                    if (ValidateDictionaryMapping(productUser.InputJson))
                    {
                        productPropertiesRoles =
                           JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(productUser.InputJson.Trim());
                    }
                    else
                    {
                        productPropertiesRoles =
                            DeserializeJSON<RolePropertyList>(productUser.InputJson);
                    }

                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);

                    break;

                case (int)ProductEnum.MarketingCenter:
                    product = new MarketingCenterProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<MarketingCenterRoleAndPropertyList>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.FinancialSuite:
                    product = new OneSiteAccountingProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<AccountingRoleAndPropertyList>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.OpsBuyer:
                    product = new OpsProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<OpsRoleAndPropertyList>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.VendorServices:
                    product = new VendorServicesProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<UserProductPropertyNotification>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.ClientPortal:
                    product = new ClientPortalProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ClientPortalPropertyRole>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.AdminSupportPortal:
                    product = new AdminSupportPortalProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<AdminSupportPortalPropertyRole>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.SalesForce:
                    product = new SalesForceProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ClientPortalPropertyRole>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.ProspectContactCenter:
                    product = new ProspectContactCenterProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProspectContactPropertyRole>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.Lead2Lease:
                    product = new Lead2LeaseProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<RolePropertyList>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.ResidentPortal:
                    product = new ResidentPortalProduct(_userClaims);
                    productPropertiesRoles = DeserializeJSON<ResidentPortal>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.OnSite:
                    product = new OnSiteProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<OnSiteUserPropertyRegionRole>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.Insurance:
                    product = new RentersInsuranceProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<RentersInsuranceRoleAndPropertyList>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.UtilityManagement:
                    product = new UtilityManagementProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<RumUserPropertyRegionRole>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.ResearchApplication:
                    product = new ResearchApplicationProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ResearchAppRoleAndPropertyList>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.SelfProvisioningPortal:
                    product = new SelfProvisioningPortalProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<SelfProvisioningPortal>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.AssetOptimizer:
                    product = new AssetOptimizerProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<AoUserCompanyPropertyRoleDetails>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.LeadManagement:
                    product = new LeadManagementProduct(_userClaims, (ProductEnum)productUser.ProductId);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.LeadAnalytics:
                    product = new LeadManagementProduct(_userClaims, (ProductEnum)productUser.ProductId);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.RPDocumentManagement:
                    product = new RPDocumentManagementProduct(_userClaims);
                    productPropertiesRoles = DeserializeJSON<RolePropertyList>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.PortfolioManagement:
                    product = new PortfolioManagementProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.DepositAlternative:
                    product = new DepositAlternativeProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.ClickPay:
                    product = new ClickPayProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.SeniorLeadManagement:
                    product = new SeniorLeadManagementProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                case (int)ProductEnum.RenovationManager:
                    product = new RenovationManagerProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
                        productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;
                case (int)ProductEnum.RealConnect:
                    product = new RealConnectProduct(_userClaims);
                    productPropertiesRoles = DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
                    result = product.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, productPropertiesRoles);
                    break;

                default:
                    result = ""; // just ignore the product batch
                    break;
            }

            return result;
        }

        public string ChangeUserType(ProductUserProperitiesRoles batchRecord)
        {
            string result;
            object productPropertiesRoles;

            IProduct product;
            switch (_productId)
            {
                case (int)ProductEnum.OneSite:
                    product = new OneSiteProduct(_userClaims);

                    if (ValidateDictionaryMapping(batchRecord.InputJson))
                    {
                        productPropertiesRoles =
                           JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(batchRecord.InputJson.Trim());
                    }
                    else
                    {
                        productPropertiesRoles =
                            DeserializeJSON<RolePropertyList>(batchRecord.InputJson);
                    }

                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.MarketingCenter:
                    product = new MarketingCenterProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<MarketingCenterRoleAndPropertyList>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.FinancialSuite:
                    product = new OneSiteAccountingProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<AccountingRoleAndPropertyList>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.OpsBuyer:
                    product = new OpsProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<OpsRoleAndPropertyList>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.VendorServices:
                    product = new VendorServicesProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<UserProductPropertyNotification>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.ClientPortal:
                    product = new ClientPortalProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ClientPortalPropertyRole>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.AdminSupportPortal:
                    product = new AdminSupportPortalProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<AdminSupportPortalPropertyRole>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.SalesForce:
                    product = new SalesForceProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ClientPortalPropertyRole>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.ProspectContactCenter:
                    product = new ProspectContactCenterProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProspectContactPropertyRole>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.Lead2Lease:
                    product = new Lead2LeaseProduct(_userClaims);
                    productPropertiesRoles =
                            DeserializeJSON<RolePropertyList>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.ResidentPortal:
                    product = new ResidentPortalProduct(_userClaims);
                    productPropertiesRoles = DeserializeJSON<ResidentPortal>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.OnSite:
                    product = new OnSiteProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<OnSiteUserPropertyRegionRole>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.Insurance:
                    product = new RentersInsuranceProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<RentersInsuranceRoleAndPropertyList>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.UtilityManagement:
                    product = new UtilityManagementProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<RumUserPropertyRegionRole>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.ResearchApplication:
                    product = new ResearchApplicationProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ResearchAppRoleAndPropertyList>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.SelfProvisioningPortal:
                    product = new SelfProvisioningPortalProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<SelfProvisioningPortal>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.AssetOptimizer:
                    product = new AssetOptimizerProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<AoUserCompanyPropertyRoleDetails>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.LeadManagement:
                    product = new LeadManagementProduct(_userClaims, ProductEnum.LeadManagement);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.LeadAnalytics:
                    product = new LeadManagementProduct(_userClaims, ProductEnum.LeadAnalytics);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.RPDocumentManagement:
                    product = new RPDocumentManagementProduct(_userClaims);
                    productPropertiesRoles = DeserializeJSON<RolePropertyList>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.PortfolioManagement:
                    product = new PortfolioManagementProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.DepositAlternative:
                    product = new DepositAlternativeProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.ClickPay:
                    product = new ClickPayProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.SeniorLeadManagement:
                    product = new SeniorLeadManagementProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                    result = product.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
                    break;
                case (int)ProductEnum.RenovationManager:
                    product = new RenovationManagerProduct(_userClaims);
                    productPropertiesRoles =
                        DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
                    result = product.CreateUser(batchRecord.RealPageId, batchRecord.CreateUserPersonaId,
                        batchRecord.AssignUserPersonaId, productPropertiesRoles);
                    break;
                default:
                    result = ""; // just ignore the product batch
                    break;
            }

            return result;
        }

        public ListResponse GetAllRights(long editorPersonaId, long userPersonaId, RequestParameter dataFilter)
        {
            var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productLogic.GetAllRights(dataFilter);
        }

        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter, string userLoginName = "")
        {
            ListResponse result = null;

            switch (_productId)
            {
                case (int)ProductEnum.OnSite:
                    var manageProductOnSite = new ManageProductOnSite(_userClaims);
                    result = manageProductOnSite.GetRegions(editorPersonaId, userPersonaId, datafilter);
                    break;
                case (int)ProductEnum.ResidentPortal:
                    List<IMessagingGroups> messageGroupsList = new List<IMessagingGroups>();
                    ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
                    messageGroupsList = manageProductResidentPortal.ListMessageGroups(editorPersonaId, userPersonaId);
                    if (messageGroupsList?.Count > 0)
                    {
                        result = new ListResponse();
                        result.Records = messageGroupsList.Cast<object>().ToList();
                        result.TotalRows = messageGroupsList.Count;
                        result.RowsPerPage = messageGroupsList.Count;
                        result.TotalPages = 1;
                        result.ErrorReason = string.Empty;
                        result.Additional = null;
                    }
                    break;
                case (int)ProductEnum.VendorServices:
                    IManageProductVendorServices manageProductVendorServices = new ManageProductVendorServices(_userClaims);
                    result = manageProductVendorServices.GetPropertyGroups(editorPersonaId, userPersonaId, datafilter);
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
                case (int)ProductEnum.AoBIX:
                    var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                    var productList = _productRepository.GetAllProducts();
                    string productcode = ProductEnumHelper.GetBooksSourceCodeByProductId(_productId, productList);
                    result = manageProductAo.GetProductPropertyGroups(editorPersonaId, userPersonaId, productcode, userLoginName);
                    break;
                case (int)ProductEnum.UtilityManagement:
                    var manageProductRum = new ManageProductRum(_userClaims);
                    result = manageProductRum.GetPropertyGroups(editorPersonaId, userPersonaId, datafilter);
                    break;
                case (int)ProductEnum.DepositAlternative:
                    var productDALogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productDALogic.GetProductPropertyGroups(datafilter);
                    break;
                case (int)ProductEnum.LeadAnalytics:
                    var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLogic.GetProductPropertyGroups(datafilter);
                    break;
                case (int)ProductEnum.FinancialSuite:
                    var manageProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    result = manageProductOneSiteAccounting.GetUserCompanies(editorPersonaId, userPersonaId, datafilter);
                    break;
                default:
                    break;
            }

            return result;
        }

        public ListResponse GetPropertiesByGroup(long editorPersonaId, long userPersonaId, string propertyGroupId, RequestParameter dataFilter)
        {
            ListResponse result = new ListResponse();
            switch (_productId)
            {
                case (int)ProductEnum.AoBusinessIntelligence:
                case (int)ProductEnum.AoInvestmentAnalytics:
                case (int)ProductEnum.AoPerformanceAnalytics:
                case (int)ProductEnum.AoRevenueManagement:
                case (int)ProductEnum.AoBenchmarking:
                case (int)ProductEnum.AoLeaseRentOption:
                case (int)ProductEnum.AoAmenityOptimization:
                case (int)ProductEnum.AoAIRevenueManagement:
                case (int)ProductEnum.AoRentControl:
                case (int)ProductEnum.AoBIX:
                    var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                    result = manageProductAo.GetGroupProperties(editorPersonaId, userPersonaId, Convert.ToInt32(propertyGroupId),_productId);
                    break;
                case (int)ProductEnum.PortfolioManagement:
                    var productPMLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productPMLogic.GetProductPropertiesByGroup(propertyGroupId, dataFilter);
                    break;
                case (int)ProductEnum.LeadAnalytics:
                    var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLogic.GetProductPropertiesByGroup(propertyGroupId, dataFilter);
                    break;
                case (int)ProductEnum.FinancialSuite:
                    var manageProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    result = manageProductOneSiteAccounting.GetPropertyGroupEntities(editorPersonaId, userPersonaId, propertyGroupId, dataFilter);
                    break;
                default:
                    break;
            }
            return result;
        }

        public ListResponse GetOrganizations(long editorPersonaId, long userPersonaId, string organizationRoleId, string organizationType)
        {
            ListResponse result = new ListResponse();

            switch (_productId)
            {
                case (int)ProductEnum.ClickPay:
                    var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, userPersonaId, _userClaims);
                    result = productLogic.GetProductOrganizations(organizationRoleId, organizationType, null);
                    break;
                default:
                    break;
            }

            return result;
        }

        public ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter dataFilter)
        {
            var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, editorPersonaId, _userClaims);
            return productLogic.GetMigrationUsers(dataFilter);
        }

        public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
        {
            var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, editorPersonaId, _userClaims);
            return productLogic.UpdateUsersMigrationStatus(migrateUsers);
        }

        public bool ExternalUserProfileChange(long editorPersonaId, ProductUserProfile productUserProfile)
        {
            var productLogic = ManageProductFactory.GetProductLogic(_productId, editorPersonaId, editorPersonaId, _userClaims);
            return productLogic.ExternalProductUserProfileChange(productUserProfile);
        }

        public string UpdateUserProfile(ProductUserProperitiesRoles productUser)
        {
            string result;

            IProduct product;

            switch (productUser.ProductId)
            {
                case (int)ProductEnum.OneSite:
                    product = new OneSiteProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.MarketingCenter:
                    product = new MarketingCenterProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.FinancialSuite:
                    product = new OneSiteAccountingProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.OpsBuyer:
                    product = new OpsProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.VendorServices:
                    product = new VendorServicesProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.ClientPortal:
                    product = new ClientPortalProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.AdminSupportPortal:
                    product = new AdminSupportPortalProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.SalesForce:
                    product = new SalesForceProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.ProspectContactCenter:
                    product = new ProspectContactCenterProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.Lead2Lease:
                    product = new Lead2LeaseProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.ResidentPortal:
                    product = new ResidentPortalProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.OnSite:
                    product = new OnSiteProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.Insurance:
                    product = new RentersInsuranceProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.UtilityManagement:
                    product = new UtilityManagementProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.ResearchApplication:
                    product = new ResearchApplicationProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.SelfProvisioningPortal:
                    product = new SelfProvisioningPortalProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.AssetOptimizer:
                    product = new AssetOptimizerProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.LeadManagement:
                    product = new LeadManagementProduct(_userClaims, ProductEnum.LeadManagement);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.LeadAnalytics:
                    product = new LeadManagementProduct(_userClaims, ProductEnum.LeadAnalytics);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.RPDocumentManagement:
                    product = new RPDocumentManagementProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.PortfolioManagement:
                    product = new PortfolioManagementProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.IntegrationMarketplace:
                    _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Stop, null, "Batch Process stopped since IntegrationMarketplace doesn't required update profile.");
                    return string.Empty;
                case (int)ProductEnum.DepositAlternative:
                    product = new DepositAlternativeProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.ClickPay:
                    product = new ClickPayProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.SeniorLeadManagement:
                    product = new SeniorLeadManagementProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.RenovationManager:
                    product = new RenovationManagerProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.RealConnect:
                    product = new RealConnectProduct(_userClaims);
                    result = product.UpdateProductUserProfile(productUser.RealPageId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId);
                    break;
                case (int)ProductEnum.IntelligentBuildingEnergy:
                case (int)ProductEnum.IntelligentBuildingTrash:
                case (int)ProductEnum.IntelligentBuildingWater:
                case (int)ProductEnum.HandsOnTrainingSystem:
                case (int)ProductEnum.LeaseLabs:
                case (int)ProductEnum.HospitalityService:
                case (int)ProductEnum.SelfGuidedTour:
                case (int)ProductEnum.LeadScoring:
                case (int)ProductEnum.SmartWasteCommercial:
                    result = "User Profile Change not implemented for this Product.";
                    break;
                default:
                    result = ""; // just ignore the product batch
                    break;
            }

            return result;
        }

        /// <summary>
        /// Validate if the input value can be serialized as dictionary
        /// </summary>
        /// <param name="productUserInputJson">Json payload</param>
        /// <returns>A boolean indicating if its valid or not</returns>
        private bool ValidateDictionaryMapping(string productUserInputJson)
        {
            bool result;

            try
            {
                JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(productUserInputJson.Trim());

                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        private T DeserializeJSON<T>(string productUserInputJson)
        {
            if (string.IsNullOrEmpty(productUserInputJson))
                return default(T); //throw new Exception("productUserInputJson is null or empty");

            try
            {
                return JsonConvert.DeserializeObject<T>(productUserInputJson.Trim());
            }
            catch (Exception)
            {
                // if the parser fails return an empty object so the product call can catch the error
                return default(T);
            }
        }

        public string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false)
        {
            string result = string.Empty;
            IProduct product;

            switch (productUserAccountDetails.ProductId)
            {
                case (int)ProductEnum.OneSite:
                    product = new OneSiteProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.MarketingCenter:
                    product = new MarketingCenterProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.FinancialSuite:
                    product = new OneSiteAccountingProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.OpsBuyer:
                    product = new OpsProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.VendorServices:
                    product = new VendorServicesProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.ClientPortal:
                    product = new ClientPortalProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.AdminSupportPortal:
                    product = new AdminSupportPortalProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.SalesForce:
                    product = new ClientPortalProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.ProspectContactCenter:
                    product = new ProspectContactCenterProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.Lead2Lease:
                    product = new Lead2LeaseProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.ResidentPortal:
                    product = new ResidentPortalProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.OnSite:
                    product = new OnSiteProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.Insurance:
                    product = new RentersInsuranceProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.ResearchApplication:
                    product = new ResearchApplicationProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.SelfProvisioningPortal:
                    product = new SelfProvisioningPortalProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.UtilityManagement:
                    product = new UtilityManagementProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.AssetOptimizer:
                    product = new AssetOptimizerProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.AoBusinessIntelligence:
                    product = new AoBusinessIntelligenceProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.LeadManagement:
                    product = new LeadManagementProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.LeadAnalytics:
                    product = new LeadManagementProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.RPDocumentManagement:
                    product = new RPDocumentManagementProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.PortfolioManagement:
                    product = new PortfolioManagementProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.DepositAlternative:
                    product = new DepositAlternativeProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.ClickPay:
                    product = new ClickPayProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.SeniorLeadManagement:
                    product = new SeniorLeadManagementProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.RenovationManager:
                    product = new RenovationManagerProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.EasyLMS:
                    product = new EasyLMSProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                case (int)ProductEnum.RealConnect:
                    product = new RealConnectProduct(_userClaims, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails, internalChange);
                    break;
                default:
                    result = ""; // just ignore the product batch
                    break;
            }

            return result;
        }

        public ListResponse GetUserGroups(long editorPersonaId, long userPersonaId, long partyId, RequestParameter dataFilter)
        {
            throw new NotImplementedException();
        }
    }
}