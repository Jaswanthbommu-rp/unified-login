using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.ClickPay;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
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
        //readonly IProductInternalSettingRepository _productInternalSettingRepository;
        readonly IManageUnifiedLogin _manageUnifiedLogin;
        private readonly IManageProductOneSite _manageProductOneSite;
        protected IPropertyRepository _propertyRepository;
        private IManageBlueBook _manageBlueBook;

        private readonly IIntegrationTypeFactory _integrationTypeFactory;

        private readonly IProductRepository _productRepository;

        private readonly IPersonaRepository _personaRepository;

        private readonly IManageUnifiedSettings _manageUnifiedSettings;
        #endregion

        #region Constructors
        /// <summary>
        /// Manages Product panel constructor
        /// </summary>
        public ManageProductPanel(DefaultUserClaim userClaims)
        {
            _userClaims = userClaims;
            var manageProduct = new ManageProduct(_userClaims, this);
            _manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
            _manageProductOneSite = new ManageProductOneSite(_userClaims);
            _propertyRepository = new PropertyRepository();
            _manageBlueBook = new ManageBlueBook(_userClaims);

            _productRepository = new ProductRepository(_userClaims);
            _personaRepository = new PersonaRepository(_userClaims);
            _manageUnifiedSettings = new ManageUnifiedSettings(_userClaims);
            var productInternalSettingRepository = new ProductInternalSettingRepository();
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, _manageUnifiedLogin, _manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaims);
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
            var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var productRepository = new ProductRepository(repository, userClaims);
            var manageProduct = new ManageProduct(repository, _userClaims, messageHandler);

            _manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
            _manageProductOneSite = manageProductOneSite;
            _propertyRepository = new PropertyRepository(repository);
            _manageBlueBook = new ManageBlueBook(_userClaims, repository, productInternalSettingRepository, messageHandler);
            _productRepository = new ProductRepository(repository, _userClaims);
            _personaRepository = new PersonaRepository(repository, _userClaims);
            _manageUnifiedSettings = new ManageUnifiedSettings(repository, userClaims, messageHandler);
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, _manageUnifiedLogin, _manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaims);
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
                    //IPersonaRepository personaRepository = new PersonaRepository(_userClaims);

                    bool usePrimaryProperty = false;
                    if (userPersonaId > 0)
                    {
                        ProductSettingList usePrimaryPropertiesOrgFlag = null;
                        string primaryPropertiesValue = null;
                        // Get primary properties from Unified Settings
                        var primaryPropertySettings = _manageUnifiedSettings.GetUnifiedSettings("Company", _userClaims.OrganizationPartyId);
                        if (primaryPropertySettings != null && primaryPropertySettings.Count > 0)
                        {
                            primaryPropertiesValue = primaryPropertySettings.FirstOrDefault(s => s.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase))?.Value;
                        }
                        var personaProductSettings = _personaRepository.GetPersonaProductSettings(userPersonaId);
                        var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == productId);

                        List<ProductSettingList> productSettingByOrg = null;
                        if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId))
                        {
                            productSettingByOrg = _productRepository.GetProductSettings(_userClaims.OrganizationRealPageGuid, (int)ProductEnum.AssetOptimizer).ToList();
                            usePrimaryPropertiesOrgFlag = productSettingByOrg.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == (int)ProductEnum.AssetOptimizer);
                        }
                        else 
                        {
                            productSettingByOrg = _productRepository.GetProductSettings(_userClaims.OrganizationRealPageGuid, productId).ToList();
                            usePrimaryPropertiesOrgFlag = productSettingByOrg.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == productId);
                        }
                        if (productSetting != null)
                        {
                            usePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
                        }

                        usePrimaryProperty = usePrimaryPropertiesOrgFlag == null ? false : usePrimaryPropertiesOrgFlag.Value.Trim() == "0" ? false : usePrimaryProperty;
                        // Override usePrimaryProperty to false if no primary properties are configured in Unified Settings
                        if (usePrimaryProperty && (string.IsNullOrEmpty(primaryPropertiesValue) || primaryPropertiesValue.Trim() == "0"))
                        {
                            usePrimaryProperty = false;
                        }

                    }

                    Dictionary<string, bool> additionalInfo = new Dictionary<string, bool>();
                    Dictionary<string, bool> additionalDataCollection = result.Additional as Dictionary<string, bool>;

                    if (productId==18 && result.Additional != null)
                    {
                        foreach (KeyValuePair<string, string> pair in result.Additional as Dictionary<string,string>)
                        {
                            if (pair.Key.Equals("accessType", StringComparison.OrdinalIgnoreCase) )
                            {
                                additionalInfo.Add(pair.Value, true);
                            }
                        }
                    }

                    additionalInfo.Add("usePrimaryProperties", usePrimaryProperty);

                    if (additionalDataCollection != null)
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

        public ListResponse GetProductUserGroups(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter)
        {
            ListResponse result;
            try
            {
                var integration = _integrationTypeFactory.GetIntegration(productId);
                result = integration.GetUserGroups(editorPersonaId, userPersonaId, partyId, datafilter);

                if (result.IsError)
                {
                    throw new Exception(result.ErrorReason);
                }

            }
            catch (Exception ex)
            {
                result = new ListResponse { IsError = true };

                if (ex is BlueBookException || (ex?.InnerException is BlueBookException))
                {
                    result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                }
                else
                {
                    //UI calls Get User Groups but sometimes it displays the data in Right tab for some products, that's why this validation was added
                    if (ex.Message.Equals(CommonMessageConstants.UserGroupsErrorMessage, StringComparison.OrdinalIgnoreCase))
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
                                result.ErrorReason = CommonMessageConstants.UserGroupsErrorMessage;
                            }
                        }
                        else
                        {
                            result.ErrorReason = CommonMessageConstants.UserGroupsErrorMessage;
                        }
                    }
                }
            }

            return result;
        }

        public RoleTemplateProductRoleMapping GetUserProductRoles(long editorPersonaId, long userPersonaId, long partyId)
        {
            RequestParameter datafilter = new RequestParameter();           
            RoleTemplateProductRoleMapping enterpriseRoleTemplate = new RoleTemplateProductRoleMapping();
            List<RoleTemplateProduct> userProducts = new List<RoleTemplateProduct>();
            List<string> productError = new List<string>();
            try
            {
                //First get unified platform product roles
                var upIntegration = _integrationTypeFactory.GetIntegration((int)ProductEnum.UnifiedPlatform);
                var upResult = upIntegration.GetRoles(editorPersonaId, userPersonaId, partyId, null, datafilter);
                if (!upResult.IsError)
                {
                    RoleTemplateProduct userProduct = new RoleTemplateProduct();
                    List<RoleTemplateRoles> roleTemplateRoles = new List<RoleTemplateRoles>();
                    IList<UnifiedLoginRoleRights> roleList = upResult.Records.Cast<UnifiedLoginRoleRights>().ToList().FindAll(p => p.IsAssigned == true);
                    foreach (var role in roleList)
                    {
                        RoleTemplateRoles templateRole = new RoleTemplateRoles
                        {
                            RoleId =  role.RoleId.ToString(),
                            RoleName = role.Role,
                            RoleTemplateProductRoleMappingID = 0
                        };
                        roleTemplateRoles.Add(templateRole);
                    }
                    if (roleList?.Count > 0){
                        userProduct.ProductId = (int)ProductEnum.UnifiedPlatform;
                        userProduct.Roles = roleTemplateRoles;
                        userProduct.RoleTemplateProductId = 0;
                        userProducts.Add(userProduct);
                    }                    
                }
                //Then get persona assigned products
                var personaProducts = _productRepository.ListProductsByPersonaId(userPersonaId, (Int32)UserUiStatusType.AccountCreationSuccessful).ToList();
                foreach (var product in personaProducts)
                {
                    ListResponse result;
                    RoleTemplateProduct userProduct = new RoleTemplateProduct();
                       
                    try {
                        var integration = _integrationTypeFactory.GetIntegration(product.ProductId);
                        result = integration.GetRoles(editorPersonaId, userPersonaId, partyId, AccessType.Property, datafilter);
                        if (!result.IsError)
                        {
                            userProduct.ProductId = product.ProductId;
                            if (result != null && result.Records.Count > 0)
                            {
                                List<RoleTemplateRoles> roleTemplateRoles = new List<RoleTemplateRoles>();
                                var baseProductRoleType = result.Records[0].GetType();

                                if (baseProductRoleType == typeof(SharedObjects.Product.ProductRole))                                {
                                    var roleList = result.Records.Cast<SharedObjects.Product.ProductRole>().ToList().FindAll(p => p.IsAssigned == true);                                  
                                    foreach (var role in roleList)
                                    {
                                        RoleTemplateRoles templateRole = new RoleTemplateRoles
                                        {
                                            RoleId = role.ID,
                                            RoleName = role.Name,
                                            RoleTemplateProductRoleMappingID = 0
                                        };
                                        roleTemplateRoles.Add(templateRole);
                                    }
                                }
                                else if (baseProductRoleType == typeof(ClickPayRole))
                                {
                                    var roleList = result.Records.Cast<ClickPayRole>().ToList().FindAll(p => p.IsAssigned == true);
                                    foreach (var role in roleList)
                                    {
                                        RoleTemplateRoles templateRole = new RoleTemplateRoles
                                        {
                                            RoleId = role.Id,
                                            RoleName = role.Name,
                                            RoleTemplateProductRoleMappingID = 0
                                        };
                                        roleTemplateRoles.Add(templateRole);
                                    }
                                }
                                else if (baseProductRoleType == typeof(ProductIntegration.Model.ProductRole) )
                                {
                                    var roleList = result.Records.Cast<ProductIntegration.Model.ProductRole>().ToList().FindAll(p => p.IsAssigned == true);
                                    foreach (var role in roleList)
                                    {
                                        RoleTemplateRoles templateRole = new RoleTemplateRoles
                                        {
                                            RoleId = role.GetRoleId,
                                            RoleName = role.GetName,
                                            RoleTemplateProductRoleMappingID = 0
                                        };
                                        roleTemplateRoles.Add(templateRole);
                                    }
                                }
                                else if (baseProductRoleType == typeof(Level))
                                {
                                    var roleList = result.Records.Cast<ILevel>().ToList().FindAll(p => p.IsAssigned == true);
                                    foreach (var role in roleList)
                                    {
                                        RoleTemplateRoles templateRole = new RoleTemplateRoles
                                        {
                                            RoleId = role.Id,
                                            RoleName = role.Name,
                                            RoleTemplateProductRoleMappingID = 0
                                        };
                                        roleTemplateRoles.Add(templateRole);
                                    }
                                }
                                else if (baseProductRoleType == typeof(SharedObjects.Product.Rum.Role) )
                                {
                                    var roleList = result.Records.Cast<SharedObjects.Product.Rum.Role>().ToList().FindAll(p => p.IsAssigned == true);
                                    foreach (var role in roleList)
                                    {
                                        RoleTemplateRoles templateRole = new RoleTemplateRoles
                                        {
                                            RoleId = role.Id.ToString(),
                                            RoleName = role.Name,
                                            RoleTemplateProductRoleMappingID = 0
                                        };
                                        roleTemplateRoles.Add(templateRole);
                                    }
                                }
                                userProduct.Roles = roleTemplateRoles;
                                userProduct.RoleTemplateProductId = 0;
                                userProducts.Add(userProduct);
                            }
                        }
                        else
                        {
                            productError.Add(product.ProductName);
                        }
                    }
                    catch{
                        productError.Add(product.ProductName);
                    }                  
                }
               
                enterpriseRoleTemplate.PartyId = partyId;
                enterpriseRoleTemplate.RoleTemplateId = 0;
                enterpriseRoleTemplate.Products = userProducts;
                enterpriseRoleTemplate.ProductsError = productError;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return enterpriseRoleTemplate;
        }

		public ListResponse GetProductRightsForRole(long editorPersonaId, int roleId, long partyId, int productId, RequestParameter datafilter, bool assignedToRoleOnly = false)
		{
			var integration = _integrationTypeFactory.GetIntegration(productId);
			return integration.GetRightsForRole(editorPersonaId, 0, roleId, partyId, assignedToRoleOnly, datafilter);
		}

		public ListResponse GetProductRightsForRole(long editorPersonaId, string roleId, long partyId, int productId, RequestParameter datafilter, bool assignedToRoleOnly = false)
        {
            var integration = _integrationTypeFactory.GetIntegration(productId);
            if (_integrationTypeFactory.GetIntegrationTypeForProductId(productId) == ProductIntegrationTypeEnum.StandardV1)
            {
                return integration.GetRightsForRole(editorPersonaId, 0, roleId, partyId, assignedToRoleOnly, datafilter);
            }
            else
            {
                return integration.GetRightsForRole(editorPersonaId, 0, Int32.Parse(roleId), partyId, assignedToRoleOnly, datafilter);
            }

        }

        public ListResponse GetProductPropertyGroups(long editorPersonaId, long userPersonaId, int productId, RequestParameter datafilter, bool assignedOnly = false, string userLoginName = "")
        {
            ListResponse result = new ListResponse();
            try
            {
                var integrationType = _integrationTypeFactory.GetIntegration(productId);
                result = integrationType.GetPropertyGroups(editorPersonaId, userPersonaId, datafilter, userLoginName);
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
            var integrationType = _integrationTypeFactory.GetIntegration(productId);
            return integrationType.GetPropertiesByGroup(editorPersonaId, userPersonaId, propertyGroupId, datafilter);
        }

        public ListResponse GetProductRights(long editorPersonaId, long userPersonaId, long partyId, int productId, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
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
            ListResponse result;

            var integrationType = _integrationTypeFactory.GetIntegration(productId);
            result = integrationType.GetOrganizations(editorPersonaId, userPersonaId, organizationRoleId, organizationType);

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
                case (int)ProductEnum.UtilityManagement:
                    var manageProductRum = new ManageProductRum(_userClaims);
                    result = manageProductRum.GetUMGlobalRoles(editorPersonaId, userPersonaId, datafilter);
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
            if (productResult == null || productResult.Records == null || productResult?.Records?.Count == 0 )
            {
                return productResult;
            }

            productResult = _manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, productId, productResult);
            return productResult;
        }

        /// <summary>
        /// TranslateProductProperties
        /// </summary>
        /// <param name="upfmProperty"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public UPFMProperty TranslateProductProperties(UPFMProperty upfmProperty, int productId)
        {
            UPFMProperty primaryPropertyIds = new UPFMProperty
            {
                id = upfmProperty.id.ConvertAll(d => d.ToLower())
            };
            var productList = _productRepository.GetAllProducts();
            var udmSourceCode = ProductEnumHelper.GetUDMSourceCodeByProductId(productId, productList);
            var productcode = ProductEnumHelper.GetProductCodeByProductId(productId, productList);
            if (!string.IsNullOrEmpty(udmSourceCode))
            {
                productcode = udmSourceCode;
            }

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

        /// <summary>
        /// Get Persona Product Primary Properties
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        public List<PersonaProductProperty> GetPersonaProductPrimaryProperties(long userPersonaId)
        {
            return _productRepository.GetPersonaProductPrimaryProperties(userPersonaId);
        }
        #endregion
    }
}
