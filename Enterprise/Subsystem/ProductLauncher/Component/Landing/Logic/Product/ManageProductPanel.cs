using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    public class ManageProductPanel : IManageProductPanel
    {
        #region Private Variables		
        private DefaultUserClaim _userClaims;
        readonly IProductInternalSettingRepository _productInternalSettingRepository;
        readonly IList<ProductInternalSetting> _productInternalSettingList;
        readonly IManageUnifiedLogin _manageUnifiedLogin;
        private readonly IManageProductOneSite _manageProductOneSite;
        
        #endregion

        #region Constructors
        /// <summary>
        /// Manages Product panel constructor
        /// </summary>
        public ManageProductPanel(DefaultUserClaim userClaims)
        {
            _userClaims = userClaims;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSettingPanel_{(int)ProductEnum.UnifiedPlatform}";
            _productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60, () =>
            {
                // load from database
                return _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform).ToList();
            });
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
            _manageProductOneSite = new ManageProductOneSite(_userClaims);
        }
        
        /// <summary>
        /// Unit Test Product panel constructor
        /// </summary>
        /// <param name="userClaims"></param>
        /// <param name="repository"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="manageProductOneSite"></param>
        public ManageProductPanel(DefaultUserClaim userClaims, IRepository repository, IManageBlueBook manageBlueBook, IManageProductOneSite manageProductOneSite)
        {
            _userClaims = userClaims;
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSettingPanel_{(int)ProductEnum.UnifiedPlatform}";
            _productInternalSettingList = rpcache.GetFromCache<IList<ProductInternalSetting>>(cacheKey, 60, () =>
            {
                // load from database
                return _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform).ToList();
            });
            ProductRepository productRepository = new ProductRepository(repository);
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims, _productInternalSettingRepository, productRepository, manageBlueBook);
            _manageProductOneSite = manageProductOneSite;
        }

        #endregion

        #region public methods
        public ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "")
        {
            ListResponse result = new ListResponse();
            try
            {
                //IProduct product;
                string productName = Enum.GetName(typeof(ProductEnum), productId);
                string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
                switch (productId)
                {
                    case (int)ProductEnum.OneSite:
                        result = _manageProductOneSite.GetOneSitePropertyList(editorPersonaId, userPersonaId, assignedOnly, datafilter);
                        break;
                    case (int)ProductEnum.MarketingCenter:
                        ManageProductMarketingCenter mg = new ManageProductMarketingCenter(_userClaims);
                        result = mg.GetProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.FinancialSuite:
                        IManageProductOneSiteAccounting mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                        result = mangeProductOneSiteAccounting.GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.OpsBuyer:
                        IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
                        result = manageProductOps.GetCompanyAssets(editorPersonaId, userPersonaId, assignedOnly, datafilter);
                        break;
                    case (int)ProductEnum.VendorServices:
                        IManageProductVendorServices manageProductVendorServices = new ManageProductVendorServices(_userClaims);
                        result = manageProductVendorServices.GetProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.ClientPortal:
                        IManageProductClientPortal _manageProductClientPortal = new ManageProductClientPortal(_userClaims);
                        result = _manageProductClientPortal.GetProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.ProspectContactCenter:
                        IManageProductProspectContact manageProductProspectContact = new ManageProductProspectContact(_userClaims);
                        result = manageProductProspectContact.GetProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.Lead2Lease:
                        IManageProductLead2Lease manageProductLead2Lease = new ManageProductLead2Lease(_userClaims);
                        result = manageProductLead2Lease.GetProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.ResidentPortal:
                        ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
                        result = manageProductResidentPortal.ListProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.OnSite:
                        var manageProductOnSite = new ManageProductOnSite(_userClaims);
                        result = manageProductOnSite.GetProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.Insurance:
                        ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(_userClaims);
                        result = manageProductRentersInsurance.ListProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.UtilityManagement:
                        var manageProductRum = new ManageProductRum(_userClaims);
                        result = manageProductRum.GetProperties(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.UnifiedAmenities:
                        IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
                        result = manageUnifiedAmenities.GetProperties(editorPersonaId, userPersonaId, assignedOnly, datafilter);
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
                        var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                        result = manageProductAo.GetProductProperties(editorPersonaId, userPersonaId, productcode, datafilter, userLoginName);
                        break;
                    case (int)ProductEnum.LeadManagement:
                        var productLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadManagement, editorPersonaId, userPersonaId, _userClaims);
                        result = productLMLogic.GetProductProperties(datafilter);
                        break;
                    case (int)ProductEnum.LeadAnalytics:
                        var productLALogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadAnalytics, editorPersonaId, userPersonaId, _userClaims);
                        result = productLALogic.GetProductProperties(datafilter);
                        break;
                    //case (int)ProductEnum.RPDocumentManagement:

                    //	break;
                    case (int)ProductEnum.PortfolioManagement:
                        var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
                        result = productPMLogic.GetProductProperties(datafilter);
                        break;
                    case (int)ProductEnum.DepositAlternative:
                        var productDALogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, editorPersonaId, userPersonaId, _userClaims);
                        result = productDALogic.GetProductProperties(datafilter);
                        break;
                    case (int)ProductEnum.UnifiedPlatform:
                        bool usePropertyInstanceUnifiedLogin = false;
                        if (_productInternalSettingList.Any(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase)))
                        {
                            usePropertyInstanceUnifiedLogin = (_productInternalSettingList.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase))?.Value == "1");
                        }

                        if (!usePropertyInstanceUnifiedLogin)
                        {
                            result = _manageUnifiedLogin.GetProperties(editorPersonaId, userPersonaId, false, datafilter);
                        }
                        else
                        {
                            result = _manageUnifiedLogin.GetUPFMProperties(editorPersonaId, userPersonaId, false, ProductEnum.UnifiedPlatform, datafilter);
                        }

                        break;
                    case (int)ProductEnum.RenovationManager:
                        var productRMLogic = ManageProductFactory.GetProductLogic(ProductEnum.RenovationManager, editorPersonaId, userPersonaId, _userClaims);
                        result = productRMLogic.GetProductProperties(datafilter);
                        break;
                    case (int)ProductEnum.SeniorLeadManagement:
                        var productSLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.SeniorLeadManagement, editorPersonaId, userPersonaId, _userClaims);
                        result = productSLMLogic.GetProductProperties(datafilter);
                        break;                   
                    case (int)ProductEnum.IntelligentBuildingTrash:
                    case (int)ProductEnum.IntelligentBuildingEnergy:
                    case (int)ProductEnum.IntelligentBuildingWater:
                    case (int)ProductEnum.HospitalityService:
                         var upfmProductIntegration = new ManageUPFMProductsIntegration(productId, _userClaims);
                         var upfmProduct = ProductEnumHelper.GetUPFMProductEnum(productId);
                         result = upfmProductIntegration.GetUPFMProperties(editorPersonaId, userPersonaId, false, upfmProduct, null);
                         break;
                    default:
                        break;
                }

                if (result.IsError)
                {
                    throw new Exception(result.ErrorReason);
                }
            }
            catch (Exception ex)
            {

                result = new ListResponse();
                result.IsError = true;

                if (ex is BlueBookException)
                {
                    result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                }
                else
                {
                    //UI calls GetProperty but sometimes it diplays the data in PropertyGroup tab for some products, that's why this validation was added
                    if (ex.Message == CommonMessageConstants.PropertyGroupErrorMessage)
                    {
                        result.ErrorReason = ex.Message;
                        return result;
                    }

                    //UI calls GetProperty but sometimes it diplays the data in Entities tab for some products, that's why this validation was added
                    if (ex.Message == CommonMessageConstants.EntityErrorMessage)
                    {
                        result.ErrorReason = ex.Message;
                        return result;
                    }

                    if (ex.Message == CommonMessageConstants.CompanyErrorMessage)
                    {
                        result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                    }
                    else
                    {
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.Message == CommonMessageConstants.CompanyErrorMessage)
                            {
                                result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                            }
                            else
                            {
                                result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                            }
                        }
                        else
                        {
                            result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                        }
                    }
                }
            }
            return result;
        }

        public ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter, AccessType? accessType, bool assignedOnly = false, string userLoginName = "")
        {
            ListResponse result = new ListResponse();
            try
            {
                string productName = Enum.GetName(typeof(ProductEnum), productId);
                string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
                switch (productId)
                {
                    case (int)ProductEnum.OneSite:
                        if (userPersonaId > 0)
                        {
                            result = _manageProductOneSite.GetOneSiteRoleList(editorPersonaId, userPersonaId, assignedOnly, datafilter);
                        }
                        else
                        {
                            result = _manageProductOneSite.GetOneSiteRoleListAll(editorPersonaId, datafilter);
                        }
                        break;
                    case (int)ProductEnum.MarketingCenter:
                        ManageProductMarketingCenter mg = new ManageProductMarketingCenter(_userClaims);
                        result = mg.GetRoles(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.FinancialSuite:
                        IManageProductOneSiteAccounting mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                        result = mangeProductOneSiteAccounting.GetUserRoles(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.OpsBuyer:
                        IManageProductOps manageProductOps = new ManageProductOps(_userClaims);
                        result = manageProductOps.GetRoles(editorPersonaId, userPersonaId, "", datafilter);
                        break;
                    case (int)ProductEnum.VendorServices:
                        IManageProductVendorServices manageProductVendorServices = new ManageProductVendorServices(_userClaims);
                        result = manageProductVendorServices.GetRoles(editorPersonaId, userPersonaId, (accessType ?? AccessType.Property), datafilter);
                        break;
                    case (int)ProductEnum.ClientPortal:
                        IManageProductClientPortal _manageProductClientPortal = new ManageProductClientPortal(_userClaims);
                        result = _manageProductClientPortal.GetRoles(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.Lead2Lease:
                        IManageProductLead2Lease manageProductLead2Lease = new ManageProductLead2Lease(_userClaims);
                        result = manageProductLead2Lease.GetRoles(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.ResidentPortal:
                        ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);
                        result = manageProductResidentPortal.ListLevelsResponse(editorPersonaId, userPersonaId);
                        break;
                    case (int)ProductEnum.OnSite:
                        var manageProductOnSite = new ManageProductOnSite(_userClaims);
                        result = manageProductOnSite.GetRoles(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.Insurance:
                        ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(_userClaims);
                        result = manageProductRentersInsurance.ListRolesResponse(editorPersonaId, userPersonaId);
                        break;
                    case (int)ProductEnum.UtilityManagement:
                        var manageProductRum = new ManageProductRum(_userClaims);
                        result = manageProductRum.GetUMGlobalRoles(editorPersonaId, userPersonaId, datafilter);
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
                        var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                        result = manageProductAo.GetProductRoles(editorPersonaId, userPersonaId, productcode, datafilter, userLoginName);
                        break;
                    case (int)ProductEnum.LeadManagement:
                        var productLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadManagement, editorPersonaId, userPersonaId, _userClaims);
                        result = productLMLogic.GetProductRoles(datafilter);
                        break;
                    case (int)ProductEnum.LeadAnalytics:

                        var productLALogic = ManageProductFactory.GetProductLogic(ProductEnum.LeadAnalytics, editorPersonaId, userPersonaId, _userClaims);
                        result = productLALogic.GetProductRoles(datafilter);

                        break;
                    case (int)ProductEnum.IntegrationMarketplace:
                        var manageProductIntegartionMarketplace = new ManageProductIntegrationMarketplace(_userClaims);
                        result = manageProductIntegartionMarketplace.GetRoles(editorPersonaId, userPersonaId, partyId);
                        break;
                    case (int)ProductEnum.RPDocumentManagement:
                        var manageProductRPDocumentManagement = new ManageProductRPDocumentManagement(_userClaims);
                        result = manageProductRPDocumentManagement.GetPropertyRoles(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.PortfolioManagement:
                        var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
                        result = productPMLogic.GetProductRoles(datafilter);
                        break;
                    case (int)ProductEnum.DepositAlternative:
                        var productDALogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, editorPersonaId, userPersonaId, _userClaims);
                        result = productDALogic.GetProductRoles(datafilter);
                        break;
                    case (int)ProductEnum.UnifiedPlatform:
                        result = _manageUnifiedLogin.GetUserRolesWithRights(editorPersonaId, userPersonaId, partyId);
                        break;
                    case (int)ProductEnum.RenovationManager:
                        var productRMLogic = ManageProductFactory.GetProductLogic(ProductEnum.RenovationManager, editorPersonaId, userPersonaId, _userClaims);
                        result = productRMLogic.GetProductRoles(datafilter);
                        break;
                    case (int)ProductEnum.SeniorLeadManagement:
                        var productSLMLogic = ManageProductFactory.GetProductLogic(ProductEnum.SeniorLeadManagement, editorPersonaId, userPersonaId, _userClaims);
                        result = productSLMLogic.GetAllRights(datafilter);
                        break;                   
                    case (int)ProductEnum.IntelligentBuildingTrash:
                    case (int)ProductEnum.IntelligentBuildingEnergy:
                    case (int)ProductEnum.IntelligentBuildingWater:
                    case (int)ProductEnum.HandsOnTrainingSystem:
                    case (int)ProductEnum.HospitalityService:
                         var upfmProductIntegration = new ManageUPFMProductsIntegration(productId, _userClaims);
                         var upfmProduct = ProductEnumHelper.GetUPFMProductEnum(productId);
                         result = upfmProductIntegration.GetRoles(editorPersonaId, userPersonaId, _userClaims.OrganizationPartyId, upfmProduct);
                         break;
                    case (int)ProductEnum.ClickPay:
                        var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.ClickPay, editorPersonaId, userPersonaId, _userClaims);
                        result = productLogic.GetProductRoles(null);
                        break;
                    case (int)ProductEnum.AoAxiometrics:
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

                if (result.IsError)
                {
                    throw new Exception(result.ErrorReason);
                }

            }
            catch (Exception ex)
            {
                result = new ListResponse();
                result.IsError = true;

                if (ex is BlueBookException)
                {
                    result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                }
                else
                {
                    //UI calls GetRoles but sometimes it diplays the data in Right tab for some products, that's why this validation was added
                    if (ex.Message == CommonMessageConstants.RightErrorMessage)
                    {
                        result.ErrorReason = ex.Message;
                        return result;
                    }

                    if (ex.Message == CommonMessageConstants.CompanyErrorMessage)
                    {
                        result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                    }
                    else
                    {
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.Message == CommonMessageConstants.CompanyErrorMessage)
                            {
                                result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                            }
                            else
                            {
                                result.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                            }
                        }
                        else
                        {
                            result.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                        }
                    }
                }
            }

            return result;
        }

        public ListResponse GetProductRightsForRole(long editorPersonaId, int roleId, long partyId, int productId, RequestParameter datafilter, bool assignedToRoleOnly = false)
        {
            ListResponse result = new ListResponse();

            switch (productId)
            {
                case (int)ProductEnum.OneSite:
                    result = _manageProductOneSite.GetOneSiteRights(editorPersonaId, datafilter, roleId, assignedToRoleOnly);
                    break;
                case (int)ProductEnum.UnifiedPlatform:
                    result = _manageUnifiedLogin.GetRightsByRole(editorPersonaId, partyId, roleId);
                    break;
                case (int)ProductEnum.UnifiedAmenities:
                    IManageUnifiedAmenities manageUnifiedAmenities = new ManageUnifiedAmenities(_userClaims);
                    result = manageUnifiedAmenities.GetRightsByRole(editorPersonaId, partyId, roleId);
                    break;
                case (int)ProductEnum.IntelligentBuildingTrash:
                case (int)ProductEnum.IntelligentBuildingEnergy:
                case (int)ProductEnum.IntelligentBuildingWater:
                case (int)ProductEnum.HospitalityService:
                         var upfmProductIntegration = new ManageUPFMProductsIntegration(productId, _userClaims);
                        var upfmProduct = ProductEnumHelper.GetUPFMProductEnum(productId);
                        result = upfmProductIntegration.GetRightsByRole(editorPersonaId, partyId, roleId, upfmProduct);
                    break;              
                default:
                    break;
            }
            return result;
        }

        public ListResponse GetProductPropertyGroups(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "")
        {
            ListResponse result = new ListResponse();
            try
            {
                IProduct product;
                string productName = Enum.GetName(typeof(ProductEnum), productId);
                string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
                switch (productId)
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
                        var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                        result = manageProductAo.GetProductPropertyGroups(editorPersonaId, userPersonaId, productcode, userLoginName);
                        break;
                    case (int)ProductEnum.UtilityManagement:
                        var manageProductRum = new ManageProductRum(_userClaims);
                        result = manageProductRum.GetPropertyGroups(editorPersonaId, userPersonaId, datafilter);
                        break;
                    case (int)ProductEnum.DepositAlternative:
                        var productDALogic = ManageProductFactory.GetProductLogic(ProductEnum.DepositAlternative, editorPersonaId, userPersonaId, _userClaims);
                        result = productDALogic.GetProductPropertyGroups(datafilter);
                        break;
                    case (int)ProductEnum.FinancialSuite:
                        var manageProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                        result = manageProductOneSiteAccounting.GetUserCompanies(editorPersonaId, userPersonaId, datafilter);
                        break;
                    default:
                        break;
                }
                if (result.IsError)
                {
                    throw new Exception(result.ErrorReason);
                }
            }
            catch (Exception ex)
            {
                result = new ListResponse();
                result.IsError = true;

                if (ex is BlueBookException)
                {
                    result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                }
                else
                {
                    //UI calls GetPropertyGroups but sometimes it diplays the data in Region tab for some products, that's why this validation was added
                    if (ex.Message == CommonMessageConstants.RegionErrorMessage)
                    {
                        result.ErrorReason = ex.Message;
                        return result;
                    }

                    //UI calls GetPropertyGroups but sometimes it diplays the data in Companies tab for some products, that's why this validation was added
                    if (ex.Message == CommonMessageConstants.CompanyTabErrorMessage)
                    {
                        result.ErrorReason = ex.Message;
                        return result;
                    }

                    if (ex.Message == CommonMessageConstants.CompanyErrorMessage)
                    {
                        result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                    }
                    else
                    {
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.Message == CommonMessageConstants.CompanyErrorMessage)
                            {
                                result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                            }
                            else
                            {
                                result.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                            }
                        }
                        else
                        {
                            result.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                        }
                    }
                }
            }
            return result;
        }

        public ListResponse GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId, int propertyGroupId, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
            IProduct product;
            string productName = Enum.GetName(typeof(ProductEnum), productId);
            string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
            switch (productId)
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
                    var manageProductAo = new ManageProductAssetOptimization(_userClaims);
                    result = manageProductAo.GetGroupProperties(editorPersonaId, userPersonaId, propertyGroupId);
                    break;
                case (int)ProductEnum.PortfolioManagement:
                    var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productPMLogic.GetProductPropertiesByGroup(propertyGroupId.ToString(), datafilter);
                    break;
                case (int)ProductEnum.FinancialSuite:
                    var manageProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    result = manageProductOneSiteAccounting.GetUserPropertyGroups(editorPersonaId, userPersonaId, datafilter);
                    break;
                default:
                    break;
            }
            return result;
        }

        public ListResponse GetProductRights(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
            string productName = Enum.GetName(typeof(ProductEnum), productId);
            string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
            switch (productId)
            {
                case (int)ProductEnum.UtilityManagement:
                    var manageProductRum = new ManageProductRum(_userClaims);
                    result = manageProductRum.GetRoles(editorPersonaId, userPersonaId, datafilter);
                    break;

                default:
                    break;
            }
            return result;
        }

        public ListResponse GetProductOrganizations(long editorPersonaId, long userPersonaId, int productId, string organizationRoleId, string organizationType)
        {
            ListResponse result = new ListResponse();
            IProduct product;
            string productName = Enum.GetName(typeof(ProductEnum), productId);
            string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
            switch (productId)
            {
                case (int)ProductEnum.ClickPay:
                    var productLogic = ManageProductFactory.GetProductLogic(ProductEnum.ClickPay, editorPersonaId, userPersonaId, _userClaims);
                    result = productLogic.GetProductOrganizations(organizationRoleId, organizationType, null);
                    break;
                default:
                    break;
            }
            return result;
        }

        public ListResponse GetProductLocationGroups(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "")
        {
            ListResponse result = new ListResponse();
           
            switch (productId)
            {
                case (int)ProductEnum.FinancialSuite:
                    var manageProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    result = manageProductOneSiteAccounting.GetUserPropertyGroups(editorPersonaId, userPersonaId, datafilter);
                    break;
                default:
                    break;
            }
            if (result.IsError)
            {
                throw new Exception(result.ErrorReason);
            }           
           
            return result;
        }

        #endregion
    }
}
