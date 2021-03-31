using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    public class ManageProductPanel : IManageProductPanel
    {
        #region Private Variables		
        private DefaultUserClaim _userClaims;
        readonly IProductInternalSettingRepository _productInternalSettingRepository;
        readonly IManageUnifiedLogin _manageUnifiedLogin;
        private readonly IManageProductOneSite _manageProductOneSite;
        protected IPropertyRepository _propertyRepository;
        private IManageBlueBook _manageBlueBook;

        private readonly IIntegrationTypeFactory _integrationTypeFactory;

        #endregion

        #region Constructors
        /// <summary>
        /// Manages Product panel constructor
        /// </summary>
        public ManageProductPanel(DefaultUserClaim userClaims)
        {
            _userClaims = userClaims;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
            _manageProductOneSite = new ManageProductOneSite(_userClaims);
            _propertyRepository = new PropertyRepository();
            _manageBlueBook = new ManageBlueBook(_userClaims);

            _integrationTypeFactory = new DefaultIntegrationTypeFactory(_productInternalSettingRepository, _manageUnifiedLogin, _manageProductOneSite, _userClaims);
        }

		/// <summary>
		/// Unit Test Product panel constructor
		/// </summary>
		/// <param name="userClaims"></param>
		/// <param name="repository"></param>
		/// <param name="manageBlueBook"></param>
		/// <param name="messageHandler"></param>
		/// <param name="manageProductOneSite"></param>
		public ManageProductPanel(DefaultUserClaim userClaims, IRepository repository, IManageBlueBook manageBlueBook, HttpMessageHandler messageHandler, IManageProductOneSite manageProductOneSite)
        {
            _userClaims = userClaims;
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            ProductRepository productRepository = new ProductRepository(repository, userClaims);
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims, _productInternalSettingRepository, productRepository, manageBlueBook);
            _manageProductOneSite = manageProductOneSite;
            _propertyRepository = new PropertyRepository(repository);
            _manageBlueBook = new ManageBlueBook(_userClaims, _productInternalSettingRepository, messageHandler);

            _integrationTypeFactory = new DefaultIntegrationTypeFactory(_productInternalSettingRepository, _manageUnifiedLogin, _manageProductOneSite, _userClaims);
        }

        #endregion

        #region public methods
        public ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter)
        {
            ListResponse result;
            try
            {
                var integration = _integrationTypeFactory.GetIntegration(productId);
                result = integration.GetProperties(editorPersonaId, userPersonaId, datafilter);

                if (result.IsError)
                {
                    throw new Exception(result.ErrorReason);
                }
                else
                {
                    IPersonaRepository personaRepository = new PersonaRepository();

                    bool usePrimaryProperty = false;
                    if (userPersonaId > 0)
                    {
                        var personaProductSettings = personaRepository.GetPersonaProductSettings(userPersonaId);
                        var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == productId);
                        if (productSetting != null)
                        {
                            usePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
                        }
                    }

                    Dictionary<string, bool> additionalInfo = new Dictionary<string, bool>();
                    Dictionary<string, bool> additionalDataCollection = (Dictionary<string, bool>)result.Additional;

                    additionalInfo.Add("usePrimaryProperties", usePrimaryProperty);

                    if (result.Additional != null)
                    {
                        foreach (KeyValuePair<string, bool> pair in additionalDataCollection)
                        {
                            if (!pair.Key.Equals("usePrimaryProperties", StringComparison.OrdinalIgnoreCase))
                            {
                                additionalInfo.Add(pair.Key, pair.Value);
                            }
                        }
                    }


                    result.Additional = additionalInfo;
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

        public ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter, AccessType? accessType)
        {
            ListResponse result;
            try
            {
                var integration = _integrationTypeFactory.GetIntegration(productId);
                result = integration.GetRoles(editorPersonaId, userPersonaId, partyId, accessType, datafilter);

                if (result.IsError)
                {
                    throw new Exception(result.ErrorReason);
                }

            }
            catch (Exception ex)
            {
                result = new ListResponse {IsError = true};

                if (ex is BlueBookException || (ex?.InnerException is BlueBookException))
                {
                    result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                }
                else
                {
                    //UI calls GetRoles but sometimes it displays the data in Right tab for some products, that's why this validation was added
                    if (ex.Message.Equals(CommonMessageConstants.RightErrorMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ErrorReason = ex.Message;
                        return result;
                    }

                    if (ex.Message.Equals(CommonMessageConstants.CompanyErrorMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                    }
                    else
                    {
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.Message.Equals(CommonMessageConstants.CompanyErrorMessage, StringComparison.OrdinalIgnoreCase))
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
            var integration = _integrationTypeFactory.GetIntegration(productId);
            return integration.GetRightsForRole(editorPersonaId, roleId, partyId, assignedToRoleOnly, datafilter);
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
                    case (int)ProductEnum.AoAxiometrics:
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

        public ListResponse GetProductGroupProperties(long editorPersonaId, long userPersonaId, int productId, string propertyGroupId, RequestParameter datafilter)
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
                    result = manageProductAo.GetGroupProperties(editorPersonaId, userPersonaId, Convert.ToInt32(propertyGroupId));
                    break;
                case (int)ProductEnum.PortfolioManagement:
                    var productPMLogic = ManageProductFactory.GetProductLogic(ProductEnum.PortfolioManagement, editorPersonaId, userPersonaId, _userClaims);
                    result = productPMLogic.GetProductPropertiesByGroup(propertyGroupId, datafilter);
                    break;
                case (int)ProductEnum.FinancialSuite:
                    var manageProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    result = manageProductOneSiteAccounting.GetPropertyGroupEntities(editorPersonaId, userPersonaId, propertyGroupId, datafilter);
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

        /// <summary>
        /// Compare Product and Primary properties
        /// </summary>
        /// <param name="upfmProperty"></param>
        /// <param name="productId"></param>
        /// <param name="productResult"></param>
        /// <returns></returns>
        public ListResponse CompareProductAndPrimaryProperties(UPFMProperty upfmProperty, int productId, ListResponse productResult)
        {
            if (productResult == null || productResult.Records.Count == 0)
            {
                return productResult;
            }

            productResult = _manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, productId, productResult);
            return productResult;
        }

        public UPFMProperty TranslateProductProperties(UPFMProperty upfmProperty, int productId)
        {
            UPFMProperty primaryPropertyIds = new UPFMProperty
            {
                id = upfmProperty.id.ConvertAll(d => d.ToLower())
            };
            string productcode = ProductEnumHelper.StringValueOf((ProductEnum)productId);
            var translatedData = _manageBlueBook.GetTranslatePropertiesFromProductToUPFM(primaryPropertyIds, productcode);
            List<string> translatedUPFMInstances = new List<string>();
            if (translatedData != null)
            {
                foreach (var attributs in translatedData.Data.Attributes)
                {
                    foreach (var propertyData in attributs.TranslatedPropertyInstances)
                    {
                        translatedUPFMInstances.Add(propertyData.PropertyInstanceSourceId);                        
                    }
                }
            }
            return new UPFMProperty()
            {
                id = translatedUPFMInstances
            };
                
        }
        #endregion
    }
}
