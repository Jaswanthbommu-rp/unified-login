using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.AdminSupportPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ClientPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntelligentBuilding;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProspectContactCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RentersInsurance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResearchApplication;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.SelfProvisioningPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.VendorServices;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Manages Product User
    /// </summary>
    public class ManageProductUser : IManageProductUser
    {
        #region Private Variables
        private IProductRepository _productRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private DefaultUserClaim _defaultUserClaim;
        private ISamlRepository _samlRepository;
        private IPropertyRepository _propertyRepository;
        IManageProduct _manageProduct;
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private SaveInteralSamlAttrLog _activityLogHelper;
        readonly ITokenHelper _tokenHelper;
        private IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly IPersonaRepository _personaRepository;
        #endregion

        #region Constructors
        /// <summary>
        /// Manages Product User constructor
        /// </summary>
        public ManageProductUser(IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository, ISamlRepository samlRepository, IManageProduct manageProduct,
            IOrganizationRepository organizationRepository, IUserRepository userRepository, IUserLoginRepository userLoginRepository, IPersonaRepository personaRepository)
        {
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _samlRepository = samlRepository;
            _manageProduct = manageProduct;
            _tokenHelper = new TokenHelper();
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _userLoginRepository = userLoginRepository;
            _personaRepository = personaRepository;
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaims"></param>
        /// <param name="messageHandler"></param>
        public ManageProductUser(IRepository repository, DefaultUserClaim userClaims, HttpMessageHandler messageHandler, IOneSiteProductService oneSiteProductService)
        {
            _productRepository = new ProductRepository(repository, userClaims);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _samlRepository = new SamlRepository(repository);
            _manageProduct = new ManageProduct(repository, userClaims, messageHandler);
            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
            var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);
            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, _productInternalSettingRepository, userClaims);
            _tokenHelper = new TokenHelper(repository);
            _organizationRepository = new OrganizationRepository(repository);
            _userRepository = new UserRepository(repository, userClaims, messageHandler);
            _userLoginRepository = new UserLoginRepository(repository);
        }

        /// <summary>
        /// Manages Product User constructor
        /// </summary>
        public ManageProductUser(DefaultUserClaim userClaims)
        {
            _productRepository = new ProductRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _samlRepository = new SamlRepository();
            _propertyRepository = new PropertyRepository();
            _defaultUserClaim = userClaims;
            _manageProduct = new ManageProduct(_defaultUserClaim);
            var manageUnifiedLogin = new ManageUnifiedLogin(_defaultUserClaim);
            var manageProductOneSite = new ManageProductOneSite(_defaultUserClaim);
            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, _productInternalSettingRepository, _defaultUserClaim);

            _activityLogHelper = new SaveInteralSamlAttrLog(_defaultUserClaim);
            _tokenHelper = new TokenHelper();
            _organizationRepository = new OrganizationRepository();
            _userRepository = new UserRepository();
            _userLoginRepository = new UserLoginRepository();
            _personaRepository = new PersonaRepository();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Used to delete all SAML product information and status for a user
        /// </summary>
        /// <param name="productUserAccountDetails">product User Account Details</param>
        /// <param name="internalChange">Tells if it is called internally</param>
        /// <returns>String.empty if success else error</returns>
        public string DeleteSamlUserProductInfoAndStatus(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false)
        {
            long assignUserPersonaId = productUserAccountDetails.PersonaId;

            var manageProductBase = new ManageProductBase(productUserAccountDetails.ProductId, _defaultUserClaim, _productInternalSettingRepository, _productRepository);

            manageProductBase.DeleteSamlUserProductInfoAndStatus(assignUserPersonaId, productUserAccountDetails.ProductId);

            var fromuserInfo = _activityLogHelper.GetUserActivityLogInfo(_defaultUserClaim.PersonaId);
            var touserInfo = _activityLogHelper.GetUserActivityLogInfo(assignUserPersonaId);
            var product = _productRepository.ListProducts(productUserAccountDetails.ProductId, null, null, null).First();
            string userName = string.IsNullOrEmpty(_defaultUserClaim.ImpersonatedByName) ? _defaultUserClaim.FirstName + " " + _defaultUserClaim.LastName : " RealPage Access (" + _defaultUserClaim.ImpersonatedByName + ") ";

            if (_defaultUserClaim.OrganizationRealPageGuid != DefaultUserClaim.EmployeeCompanyRealPageId 
                  && productUserAccountDetails.ProductId == (int)ProductEnum.EasyLMS)
            {
                var logMessage = $"{touserInfo.FirstName} {touserInfo.LastName}'s {product.Name} data is removed by {userName}";
                _activityLogHelper.PushToQueue(fromuserInfo, touserInfo, logMessage, "PRODUCT_ACCESS");
            }
            else if (internalChange)
            {
                var logMessage = $" {userName} " +
                    $"deleted user information of {touserInfo.FirstName} {touserInfo.LastName} " +
                    $"for {product.Name}.";

                _activityLogHelper.PushToQueue(fromuserInfo, touserInfo, logMessage, "USER_UPDATE_INTERNAL");
            }

            return string.Empty;
        }

        private string UpdateProductPrimaryPropertyProductStatus(long userPersonaId, int productId, int settingvalue)
        {
            var manageProductBase = new ManageProductBase(productId, _defaultUserClaim, _productInternalSettingRepository, _productRepository);
            manageProductBase.UpdateProductSettingProductStatus(userPersonaId, "UsePrimaryProperties", productId, settingvalue);
            return string.Empty;
        }

        private void SavePersonaProductPrimaryProperties(bool usePrimaryProperties, long assignUserPersonaId, int productId, RolePropertyList roleProp, string inputJson)
        {
            Dictionary<string, object> logData = new Dictionary<string, object> { { "ProductPrimaryProperties", JsonConvert.SerializeObject(roleProp.ProductPrimaryProperties) } };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "SavePersonaProductPrimaryProperties", $"Product: {productId} and persona: {assignUserPersonaId} and usePrimaryProperties: {usePrimaryProperties}" });

            if (productId != 4)
            {
                if (usePrimaryProperties == true && roleProp.ProductPrimaryProperties != null && roleProp.ProductPrimaryProperties.Count > 0)
                {
                    string jsonSecuritySettings = JsonConvert.SerializeObject(roleProp.ProductPrimaryProperties);
                    _productRepository.SavePersonaProductProperties(assignUserPersonaId, productId, jsonSecuritySettings);
                }
                UpdateProductPrimaryPropertyProductStatus(assignUserPersonaId, productId, usePrimaryProperties == true ? 1 : 0);
            }
            else if (productId == 4)// Handle AO products seperatly
            {
                Object productPropertiesRoles =
                        JsonConvert.DeserializeObject<AoUserCompanyPropertyRoleDetails>(inputJson);

                var aoRoleProp = productPropertiesRoles as AoUserCompanyPropertyRoleDetails;
                IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails = aoRoleProp.AoUserCompanyPropertyRoleDetailList;
                if (aoGbUserCompanyPropertyRoleDetails != null)
                {
                    foreach (var data in aoGbUserCompanyPropertyRoleDetails)
                    {
                        if (data.ProductPrimaryProperties != null && data.ProductId != 0)
                        {
                            if (data.UsePrimaryProperties == true)
                            {
                                if (data.ProductPrimaryProperties == null || data.ProductPrimaryProperties.Count == 0)
                                {
                                    data.IsAssigned = false;
                                }
                                string jsonSecuritySettings = JsonConvert.SerializeObject(data.ProductPrimaryProperties);
                                _productRepository.SavePersonaProductProperties(assignUserPersonaId, data.ProductId, jsonSecuritySettings);
                            }
                        }
                        UpdateProductPrimaryPropertyProductStatus(assignUserPersonaId, data.ProductId, data.UsePrimaryProperties == true ? 1 : 0);
                    }
                }
            }
        }

        /// <summary>
        /// Creates Product User
        /// </summary> 
        /// <param name="productUser">Product details for a user</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateProductUser(ProductUserProperitiesRoles productUser)
        {
            string result = string.Empty;
            int productId = productUser.ProductId;

            bool isUpdateUser = false;
            bool usePrimaryProperties = false;
            bool isCreateUserWithNoProperties = true;

            Dictionary<int, RolePropertyList> rolePropDictionary = new Dictionary<int, RolePropertyList>();
            Dictionary<int, RolePropertyList> rolePrimaryPropDictionary = new Dictionary<int, RolePropertyList>();
            Dictionary<int, bool> usePrimaryPropertyFlags = new Dictionary<int, bool>();
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            var productsWithNoProperties = GetProductsWithNoProperties();
            string prodUserInputJson = string.Empty;

            if (ValidateDictionaryMapping(productUser.InputJson))
            {
                prodUserInputJson = productUser.InputJson;
                var roleProp = JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(productUser.InputJson.Trim());
                foreach (var rolePropertyList in roleProp)
                {
                    //rolePropertyList.Key Convert to enum to get product id;
                    rolePropDictionary.Add((int)Enum.Parse(typeof(ProductEnum), rolePropertyList.Key), rolePropertyList.Value);
                }
            }
            else
            {
                var roleProp = JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson);
                rolePropDictionary.Add(productUser.ProductId, roleProp);
            }
            try
            {
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(productUser.AssignUserPersonaId, productUser.ProductId);
                if (productAttributes.Any())
                {
                    isUpdateUser = true;
                }

                if (productUser.AssignUserPersonaId > 0)
                {
                    var personaProductSettings = _personaRepository.GetPersonaProductSettings(productUser.AssignUserPersonaId);
                    var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == productId);
                    if (productSetting != null)
                    {
                        usePrimaryProperties = productSetting.Value.Trim() == "1" ? true : false;
                    }
                }

                foreach (var rolePropertyList in rolePropDictionary)
                {
                        usePrimaryPropertyFlags.Add(rolePropertyList.Key, rolePropertyList.Value.UsePrimaryProperties);
                        var foundPrimaryProperties = AssignPrimaryPropertiesToProductBatchOnUserCreate(productUser, rolePropertyList.Value, productsWithNoProperties);
                        if (foundPrimaryProperties != null)
                        {
                            rolePrimaryPropDictionary.Add(rolePropertyList.Key, foundPrimaryProperties);
                        }               
                    
                    if(rolePropertyList.Value.UsePrimaryProperties && !productsWithNoProperties.Contains(productId) && (rolePropertyList.Value?.IsAssigned == true && rolePropertyList.Value.PropertyList?.Count == 0))
                    {
                        //Create user (not update) but translation has no properties
                        var userProducts = _samlRepository.ListActiveProductsByPersonaId(productUser.AssignUserPersonaId, 0, "");
                        bool userHasProduct = userProducts.Any(a => a.ProductId == productId);
                        if (!userHasProduct)
                        {
                            isCreateUserWithNoProperties = false;
                        }                        
                        
                        //Primary properties translation did not result any properties. Un-assign product
                        if (ValidateDictionaryMapping(productUser.InputJson))
                        {
                            prodUserInputJson = productUser.InputJson;
                            var roleProp = JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(productUser.InputJson.Trim());
                            foreach (var rpl in roleProp)
                            {
                                rpl.Value.IsAssigned = false;
                            }
                            productUser.InputJson = JsonConvert.SerializeObject(roleProp);
                        }
                        else
                        {
                            var roleProp = JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson);
                            roleProp.IsAssigned = false;
                            productUser.InputJson = JsonConvert.SerializeObject(roleProp);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(prodUserInputJson))
                {
                    productUser.InputJson = prodUserInputJson;
                }
                if (isCreateUserWithNoProperties)
                {
                    var integration = _integrationTypeFactory.GetIntegration(productUser.ProductId);
                    _productRepository.UpdateBatchProcessorLog(productUser.ProductBatchId, DateTime.UtcNow, null);
                    result = integration.CreateUser(productUser, out additionalParameters);
                }
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;

                result = realError.Message;
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateProductUser", $"User Sync Request process for product: {productUser.ProductId} settings and persona: {productUser.AssignUserPersonaId} and realerror : {realError.Message}" });

            }
            finally
            {
                _productRepository.UpdateBatchProcessorLog(productUser.ProductBatchId, null, DateTime.UtcNow);
            }

            var isBatchCompleted = false;
            try
            {
                // If result OK then update Success status else Error
                if (string.IsNullOrEmpty(result))
                {
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"User Sync Request process for product: {productUser.ProductId} settings and persona: {productUser.AssignUserPersonaId}" });
                    foreach (var rolePropertyList in rolePrimaryPropDictionary)
                    {
                        var thisProductUserPrimaryProperty = usePrimaryPropertyFlags.FirstOrDefault(p => p.Key == rolePropertyList.Key).Value;
                        SavePersonaProductPrimaryProperties(thisProductUserPrimaryProperty, productUser.AssignUserPersonaId, rolePropertyList.Key, rolePropertyList.Value, productUser.InputJson);
                    }
                    //Updating inputjson, It may change if no properties are translated - unassign product.
                    isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Success, productUser.InputJson);
                    // Insert into the product activity log
                    if (additionalParameters.Count > 0)
                    {
                        _productRepository.UpdateProductActivityLog(productUser.BatchProcessorGroupId, productUser.ProductId, additionalParameters);
                    }

                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"UpdateProductBatch - product: {productUser.ProductId} ,persona: {productUser.AssignUserPersonaId} ,isBatchCompleted: {isBatchCompleted} ,User Sync Request process for Success ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });
                    //call apicore kafka publish to sync translated properties
                    var roleProp = JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson);
                    var productInternalSettingList = GetProductInternalSettings(productUser.ProductId);
                    var doesNotUseProperties = productInternalSettingList.FirstOrDefault(a => a.Name.Equals("DoesNotUseProperties", StringComparison.OrdinalIgnoreCase))?.Value;
                    if ((doesNotUseProperties == null || doesNotUseProperties != "1") && roleProp.IsAssigned)
                    {
                        //product combination check
                        if (rolePropDictionary?.Count > 1)
                        {
                            foreach (var product in rolePropDictionary)
                            {
                                SyncUserProductProperties(product.Key, productUser.AssignUserPersonaId, productUser.CreateUserPersonaId);
                            }
                        }
                        else
                        {
                            SyncUserProductProperties(productUser.ProductId, productUser.AssignUserPersonaId, productUser.CreateUserPersonaId);
                        }
                    }
                }
                else
                {
                    if (result.ToUpper() == ProductBatchStatusType.Stop.ToString().ToUpper())
                    {
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"User Sync Request process for Stop, product: {productUser.ProductId} settings and persona: {productUser.AssignUserPersonaId}" });
                        isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Stop, null, "Batch Process stopped due to internal error for this product.");
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"UpdateProductBatch - product: {productUser.ProductId} , persona: {productUser.AssignUserPersonaId} , isBatchCompleted: {isBatchCompleted}, User Sync Request process for Stop, DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });
                    }
                    else
                    {
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"User Sync Request process for Error, product: {productUser.ProductId} settings and persona: {productUser.AssignUserPersonaId}" });

                        isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"UpdateProductBatch - product: {productUser.ProductId} , persona: {productUser.AssignUserPersonaId} , isBatchCompleted: {isBatchCompleted}, User Sync Request process for Error, DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });

                        if (!isUpdateUser && isCreateUserWithNoProperties)
                        {
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"User Sync Request process for Error, product: {productUser.ProductId} settings and persona: {productUser.AssignUserPersonaId}" });
                            _productRepository.UpdateProductSettingProductStatus(productUser.AssignUserPersonaId, productId, "ProductStatus", (int)ProductBatchStatusType.Error);
                        }
                        else
                        {
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"User Sync Request process during the update process, product: {productUser.ProductId} settings and persona: {productUser.AssignUserPersonaId}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateProductUser", $"Batch process for activity log isBatchCompleted: {isBatchCompleted}" });
            }

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"Batch process for activity log isBatchCompleted: {isBatchCompleted} ,product: {productUser.ProductId} ,CreateUserPersonaId: {productUser.CreateUserPersonaId} ,AssignUserPersonaId: {productUser.AssignUserPersonaId} ,BatchProcessorGroupId: {productUser.BatchProcessorGroupId}" });

            if (isBatchCompleted)
            {
                try
                {
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"Batch process for inner isBatchCompleted: {isBatchCompleted}, product: {productUser.ProductId} , CreateUserPersonaId : {productUser.CreateUserPersonaId} ,AssignUserPersonaId: {productUser.AssignUserPersonaId} ,BatchProcessorGroupId: {productUser.BatchProcessorGroupId}" });
                    //Get the product activity log data associated with batchgroupprocessorid
                    var productActivityLog = _productRepository.GetProductActivityLog(productUser.BatchProcessorGroupId);
                    WriteActivityLog(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, productUser.BatchProcessorGroupId, productUser.ImpersonatorUserId, productActivityLog.ToList());
                    //Clear the product activity log associated with batchgroupprocessorid
                    _productRepository.DeleteProductActivityLog(productUser.BatchProcessorGroupId);
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateProductUser", $"Product Activity log for BatchprocessorGroupId: {productUser.AssignUserPersonaId} is completed" });
                }
                catch (Exception ex)
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateProductUser", $"Product Activity log for BatchprocessorGroupId: {productUser.AssignUserPersonaId} is failed" });
                }

            }

            return result;
        }

        /// <summary>
        /// Used to create an employee in the given product
        /// </summary>
        /// <param name="productUser"></param>
        /// <returns></returns>
        public string CreateEmployeeProductUser(ProductUserProperitiesRoles productUser)
        {
            List<AdditionalParameters> additionalParameters;
            string result = string.Empty;
            int productId = productUser.ProductId;

            bool isUpdateUser = false;
            bool usePrimaryProperties = false;
            RolePropertyList roleProp = new RolePropertyList();
            try
            {
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(productUser.AssignUserPersonaId, productUser.ProductId);
                if (productAttributes.Any())
                {
                    isUpdateUser = true;
                }
                var productsWithNoProperties = GetProductsWithNoProperties();
                roleProp = GetProductPropertiesRoles<RolePropertyList>(productUser.InputJson) as RolePropertyList;
                usePrimaryProperties = roleProp.UsePrimaryProperties;
                roleProp = AssignPrimaryPropertiesToProductBatchOnUserCreate(productUser, roleProp, productsWithNoProperties);

                var integration = _integrationTypeFactory.GetIntegrationStandardV1(productUser.ProductId);
                result = integration.CreateUser(productUser, out additionalParameters);
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;

                result = realError.Message;
            }

            var employeeInfo = _activityLogHelper.GetUserActivityLogInfo(productUser.AssignUserPersonaId);
            // If result OK then update Success status else Error
            if (string.IsNullOrEmpty(result))
            {
                SavePersonaProductPrimaryProperties(usePrimaryProperties, productUser.AssignUserPersonaId, productUser.ProductId, roleProp, productUser.InputJson);
                WriteActivityLogWithMessage(productUser.RealPageEmployeePersonaId, 0, "Employee {3} {4} added/updated to product {2} in company " + employeeInfo.OrganizationName, productId, productUser.ImpersonatorUserId);
                return "";
            }

            if (!isUpdateUser)
            {
                _productRepository.UpdateProductSettingProductStatus(productUser.AssignUserPersonaId, productId, "ProductStatus", (int)ProductBatchStatusType.Error);
            }
            else
            {
                //Activity log
                var activityMessageResult = "An error occurred during the update process for employee {3} {4} to product {2} in company " + employeeInfo.OrganizationName + ".";
                WriteActivityLogWithMessage(productUser.RealPageEmployeePersonaId, 0, activityMessageResult, productId, productUser.ImpersonatorUserId);
                return result;
            }

            return result;
        }

        /// <summary>
        /// Creates Product User
        /// </summary> 
        /// <param name="productUser">Product details for a user</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateEnterpriseRoleProductUser(ProductUserProperitiesRoles productUser)
        {
            List<AdditionalParameters> additionalParameters;
            string result = string.Empty;
            int productId = productUser.ProductId;

            bool isUpdateUser = false;
            bool usePrimaryProperties = false;
            bool isRolesExists = false;
            RolePropertyList roleProp = new RolePropertyList();
            try
            {
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(productUser.AssignUserPersonaId, productUser.ProductId);
                if (productAttributes.Any())
                {
                    isUpdateUser = true;
                }

                var productInternalSettings = _manageProduct.GetProductInternalSettings(productUser.ProductId);
                var updateinUDM = productInternalSettings.Where(x => x.Name.ToUpper() == "UPDATEPRODUCTINUDM").FirstOrDefault();

                roleProp = GetProductPropertiesRoles<RolePropertyList>(productUser.InputJson) as RolePropertyList;
                usePrimaryProperties = roleProp.UsePrimaryProperties;

                if (roleProp.PropertyList.Count == 0 && (updateinUDM != null && updateinUDM.Value == "1"))
                {
                    result = "No Product Properties are found for Enterprise Role";
                }
                else if (roleProp.RoleList.Count == 0)
                {
                    result = "No Product Roles are found for Enterprise Role";
                }
                else
                {

                    var integration = _integrationTypeFactory.GetIntegration(productUser.ProductId);
                    result = integration.CreateUser(productUser, out additionalParameters);
                }
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;

                result = realError.Message;
            }
            var isBatchCompleted = false;
            // If result OK then update Success status else Error
            if (string.IsNullOrEmpty(result))
            {
                isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Success);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateEnterpriseRoleProductUser", $"UpdateProductBatch - product: {productUser.ProductId} , persona: {productUser.AssignUserPersonaId} , isBatchCompleted: {isBatchCompleted} ,Enterprise roles Success ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });

                SavePersonaProductPrimaryProperties(usePrimaryProperties, productUser.AssignUserPersonaId, productUser.ProductId, roleProp, productUser.InputJson);
            }
            else
            {
                if (result.ToUpper() == ProductBatchStatusType.Stop.ToString().ToUpper())
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Stop, null, "Batch Process stopped due to internal error for this product.");
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateEnterpriseRoleProductUser", $"UpdateProductBatch - product: {productUser.ProductId} , persona: {productUser.AssignUserPersonaId} , isBatchCompleted: {isBatchCompleted} ,Enterprise roles Stop ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });

                }
                else
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);

                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "CreateEnterpriseRoleProductUser", $"UpdateProductBatch - product: {productUser.ProductId} , persona: {productUser.AssignUserPersonaId} , isBatchCompleted: {isBatchCompleted} ,Enterprise roles Error ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });

                    if (!isUpdateUser)
                    {
                        _productRepository.UpdateProductSettingProductStatus(productUser.AssignUserPersonaId, productId, "ProductStatus", (int)ProductBatchStatusType.Error);
                    }
                    else
                    {
                        //Activity log
                        result = "An error occurred during the enterprise role product update process";
                        WriteActivityLogWithMessage(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, result, productId, productUser.ImpersonatorUserId);
                    }
                }
            }

            if (isBatchCompleted)
            {
                WriteActivityLog(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, productUser.BatchProcessorGroupId, productUser.ImpersonatorUserId);
            }


            return result;
        }

        /// <summary>
        /// Update product details for a user
        /// </summary> 
        /// <param name="productUserAccountDetails">Product User Account Details</param>
        /// <param name="internalChange">Tells if this change is from internally or not</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserAccountDetails(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false)
        {
            var integration = _integrationTypeFactory.GetIntegration(productUserAccountDetails.ProductId);
            return integration.UpdateUserDetails(productUserAccountDetails, internalChange);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary>
        /// <param name="productUser">Product details for a user</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(ProductUserProperitiesRoles productUser)
        {
            string result = string.Empty;
            var isBatchCompleted = false;
            try
            {
                var integrationType = _integrationTypeFactory.GetIntegration(productUser.ProductId);
                result = integrationType.UpdateUserProfile(productUser);
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                {
                    realError = realError.InnerException;
                }

                result = realError.Message;
            }

            // If result OK then update Success status else Error
            if (string.IsNullOrEmpty(result))
            {
                isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Success);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", $"UpdateProductBatch result: Success ,product: {productUser.ProductId} ,persona: {productUser.AssignUserPersonaId} ,isBatchCompleted: {isBatchCompleted} ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });
            }
            else
            {
                isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", $"UpdateProductBatch result: Error ,product: {productUser.ProductId} ,persona: {productUser.AssignUserPersonaId} ,isBatchCompleted: {isBatchCompleted} ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });
            }
           

            return result;
        }

        /// <summary>
        /// Returns List of Product Batch Statuses
        /// </summary>
        /// <param name="realPageId">User Enterprise Id</param>
        /// <param name="assignUserId">Assigned User PersonaId</param>
        /// <returns>List of ProductBatchStatus</returns>
        public IList<ProductBatchStatus> GetProductStatuses(Guid realPageId, long assignUserId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Empty realpage Id");
            }

            if (assignUserId == 0)
            {
                throw new Exception("assignUserId not supplied");
            }

            ProductRepository repo = new ProductRepository();
            return repo.ListProductBatchStatuses(realPageId, assignUserId);
        }

        /// <summary>
        /// Change Product User Type from admin to regular or vice versa
        /// </summary>
        /// <param name="batchRecord">Product batch details for a user</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeUserType(ProductUserProperitiesRoles batchRecord)
        {
            string result = string.Empty;
            var isBatchCompleted = false;
            try
            {
                var integration = _integrationTypeFactory.GetIntegration(batchRecord.ProductId);
                result = integration.ChangeUserType(batchRecord);
            }
            catch (Exception ex)
            {
                Exception realError = ex;
                while (realError.InnerException != null)
                    realError = realError.InnerException;

                result = realError.Message;
            }

            // If result OK then update Success status else Error
            if (string.IsNullOrEmpty(result))
            {
                isBatchCompleted= _productRepository.UpdateProductBatch(batchRecord.ProductBatchId, (int)ProductBatchStatusType.Success);
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "ChangeUserType", $"UpdateProductBatch - result: Success ,product: {batchRecord.ProductId} ,persona: {batchRecord.AssignUserPersonaId} ,isBatchCompleted: {isBatchCompleted} ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });

            }
            else
            {
                if (result.ToUpper() == ProductBatchStatusType.Stop.ToString().ToUpper())
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(batchRecord.ProductBatchId, (int)ProductBatchStatusType.Stop, null, "Batch Process stopped due to internal error for this product.");
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "ChangeUserType", $"UpdateProductBatch - result: Stop ,product: {batchRecord.ProductId} ,persona: {batchRecord.AssignUserPersonaId} ,isBatchCompleted: {isBatchCompleted} ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });

                }
                else
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(batchRecord.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "ChangeUserType", $"UpdateProductBatch - result: Error ,product: {batchRecord.ProductId} ,persona: {batchRecord.AssignUserPersonaId} ,isBatchCompleted: {isBatchCompleted} ,DateTime {DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:ffff")}" });
                }
            }

            if (isBatchCompleted)
            {
                WriteActivityLog(batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, batchRecord.BatchProcessorGroupId, batchRecord.ImpersonatorUserId);
            }

            return result;
        }
        #endregion

        #region Private Methods
        private T GetProductPropertiesRoles<T>(string productUserInputJson)
        {
            if (string.IsNullOrEmpty(productUserInputJson))
                return default(T); //throw new Exception("productUserInputJson is null or empty");

            try
            {
                return JsonConvert.DeserializeObject<T>(productUserInputJson.Trim());
            }
            catch (Exception ex)
            {
                // if the parser fails return an empty object so the product call can catch the error
                return default(T);
            }
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

        private void WriteActivityLogWithMessage(long fromPersonaId, long toPersonaId, string message, int productId, long impersonatorUserId)
        {
            //UserActivityLogInfo toUserLogDetail = null;
            // log product user updated activity
            var fromUserLogDetail = _activityLogHelper.GetUserActivityLogInfo(fromPersonaId);
            var toUserLogDetail = toPersonaId != 0 ? _activityLogHelper.GetUserActivityLogInfo(toPersonaId) : null;
            var booksProductDetail = _productRepository.GetBooksMasterProductDetail(productId);
            UserDetails impersonatorUserInfo = null;

            if (impersonatorUserId > 0)
            {
                var impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(impersonatorUserId);
                impersonatorUserInfo = _userRepository.GetUserDetails(null, impersonatorUserLoginOnly.RealPageId.ToString());
            }
            string logMessage = impersonatorUserInfo != null
                ? string.Format(message, toUserLogDetail?.FirstName, toUserLogDetail?.LastName, booksProductDetail.Name, " RealPage Access ("+impersonatorUserInfo.FirstName, impersonatorUserInfo.LastName + ")")
                : string.Format(message, toUserLogDetail?.FirstName, toUserLogDetail?.LastName, booksProductDetail.Name, fromUserLogDetail.FirstName, fromUserLogDetail.LastName);

            WriteActivityLog(fromUserLogDetail, toUserLogDetail, booksProductDetail.BooksProductCode, logMessage);
        }

        private void WriteActivityLog(UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo, string booksProductCode, string message)
        {
            long booksMasterOrgId = toUserLogInfo?.BooksOrganizationMasterId ?? fromUserLogInfo.BooksOrganizationMasterId;
            long orgPartyId = toUserLogInfo?.OrganizationPartyId ?? fromUserLogInfo.OrganizationPartyId;

            // log product user updated activity
            try
            {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.PRODUCT_ACCESS,
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    BooksMasterOrganizationId = booksMasterOrgId,
                    OrganizationPartyId = orgPartyId,
                    Message = message,

                    FromUserLoginName = fromUserLogInfo.LoginName,
                    FromUserLoginId = fromUserLogInfo.UserId,
                    FromUserFirstName = fromUserLogInfo.FirstName,
                    FromUserLastName = fromUserLogInfo.LastName,
                    FromUserRealpageId = fromUserLogInfo.RealPageId.ToString(),

                    ToUserLoginId = toUserLogInfo?.UserId,
                    ToUserLoginName = toUserLogInfo?.LoginName,
                    ToUserFirstName = toUserLogInfo?.FirstName,
                    ToUserLastName = toUserLogInfo?.LastName,
                    ToUserRealpageId = toUserLogInfo?.RealPageId.ToString(),

                    BooksProductCode = booksProductCode
                });
            }
            catch (Exception ex)
            {
            }
        }

        private void WriteActivityLog(long fromPersonaId, long toPersonaId, int batchGroupId, long impersonatorUserId, List<AdditionalParameters> additionalParameters = null)
        {
            var fromUserLogInfo = _activityLogHelper.GetUserActivityLogInfo(fromPersonaId);
            var toUserLogInfo = _activityLogHelper.GetUserActivityLogInfo(toPersonaId);
            UserDetails impersonatorUserInfo = null;
            string primaryOrganizationCompanyName = string.Empty;
            Guid realPageEmployeeAccessID = _organizationRepository.GetOrganizationAdminUserRealPageId(fromUserLogInfo.OrganizationRealpageId);

            if (impersonatorUserId > 0)
            {
                var impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(impersonatorUserId);
                impersonatorUserInfo = _userRepository.GetUserDetails(null, impersonatorUserLoginOnly.RealPageId.ToString());
            }
            if (impersonatorUserId == 0 && fromUserLogInfo.RealPageId == realPageEmployeeAccessID)
            {
                var userOrganizationList = _userLoginRepository.ListAllOrganizationByLoginName(toUserLogInfo.LoginName);
                primaryOrganizationCompanyName = userOrganizationList.FirstOrDefault(p => p.PrimaryOrganization).OrganizationName;
            }
            var data = _productRepository.GetUserBatchDetails(batchGroupId, fromPersonaId, toPersonaId);
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "WriteActivityLog", $"Batch process for results count : {(data != null && data.Count > 0 ? data.Count : 0)}" });
            if (data != null && data.Count > 0)
            {
                foreach (var item in data)
                {
                    var role = JsonConvert.DeserializeObject<UPFMProductPropertyRole>(item.InputJSON.Trim());
                    item.IsAssigned = role.IsAssigned;
                }

                bool activityLogged = data[0].BatchProcessorGroupActivityLogged;
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "WriteActivityLog", $"Batch process for activityLogged : {activityLogged}" });
                if (!activityLogged)
                {
                    var successRecords = data.Where(x => x.StatusTypeId == 8).ToList();
                    if (successRecords != null && successRecords.Count > 0)
                    {
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "WriteActivityLog", $"Batch process for success count : {successRecords.Count}" });
                        GenerateQueueMessage(fromUserLogInfo, toUserLogInfo, successRecords, true, impersonatorUserInfo, primaryOrganizationCompanyName, fromPersonaId, additionalParameters);
                    }

                    var failedRecords = data.Where(x => x.StatusTypeId == 7).ToList();
                    if (failedRecords != null && failedRecords.Count > 0)
                    {
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "WriteActivityLog", $"Batch process for failed count : {successRecords.Count}" });
                        GenerateQueueMessage(fromUserLogInfo, toUserLogInfo, failedRecords, false, impersonatorUserInfo, primaryOrganizationCompanyName, fromPersonaId);
                    }

                    //update status
                    _productRepository.UpdateBatchGroupStatus(batchGroupId, true);
                }
            }
        }

        private void GenerateQueueMessage(UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo, List<UserBatchProductDetail> userBatchProductDetails, bool IsSuccess, UserDetails impersonatorUserInfo, string primaryOrganizationCompanyName, long fromPersonaId = 0, List<AdditionalParameters> additionalParameters = null)
        {
           
            List<string> assignedProducts = new List<string>();
            List<string> unassignedProducts = new List<string>();

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "GenerateQueueMessage", $"Batch process for GenerateQueueMessage : {IsSuccess} userBatchProductDetails {userBatchProductDetails.Count}" });
            if (IsSuccess)
            {
                foreach (var item in userBatchProductDetails)
                {
                    if (item.IsAssigned)
                    {
                        if (item.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            assignedProducts.AddRange(GetAOProductsForActivity(item, true, 8));
                            unassignedProducts.AddRange(GetAOProductsForActivity(item, false, 8));
                        }
                        else
                        {
                            assignedProducts.Add(item.Name);
                        }
                    }
                    if (!item.IsAssigned)
                    {
                            unassignedProducts.Add(item.Name);
                    }
                }
                if (assignedProducts.Count > 0)
                {
                    var assign = impersonatorUserInfo != null
                   ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName}:"
                   : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName}:";

                    assign  += " Access was granted to " + string.Join(", ", assignedProducts) + ".";
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "GenerateQueueMessage", $"Batch process for success message : {assign}" });
                    _activityLogHelper.PushToQueue(fromUserLogInfo, toUserLogInfo, assign, "PRODUCT_ACCESS", additionalParameters);
                }

                if (unassignedProducts.Count > 0)
                {
                    string unassign = string.Empty;
                    if (!string.IsNullOrEmpty(primaryOrganizationCompanyName))
                    {
                        unassign = $"Owner Company ({primaryOrganizationCompanyName}) Deactivated user and updated access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName}:";
                    }
                    else
                    {
                        unassign = impersonatorUserInfo != null
                            ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName}:"
                            : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName}:";
                    }

                    unassign += " Access was unassigned from " + string.Join(", ", unassignedProducts) + ".";
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "GenerateQueueMessage", $"Batch process for success message : {unassign}" });
                    _activityLogHelper.PushToQueue(fromUserLogInfo, toUserLogInfo, unassign, "PRODUCT_ACCESS");
                }
            }
            else
            {
                List<string> failedProducts = new List<string>();
                foreach (var item in userBatchProductDetails)
                {
                    if (item.ProductId == (int)ProductEnum.AssetOptimizer)
                    {
                        failedProducts.AddRange(GetAOProductsForActivity(item, true, 7));
                    }
                    else
                    {
                        failedProducts.Add(item.Name);
                    }
                }

                var commaString = string.Join(", ", failedProducts);
                var  message = impersonatorUserInfo != null
                    ? $"An exception occurred when RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) attempted to update product access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName} in {commaString}."
                    : $"An exception occurred when {fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} attempted to update product access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName} in {commaString}.";

                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "GenerateQueueMessage", $"Batch process for failed message : {message}" });
                _activityLogHelper.PushToQueue(fromUserLogInfo, toUserLogInfo, message, "PRODUCT_ACCESS");
                SendNotification(message + " Please contact RealPage Support for assistance.", fromPersonaId);
            }
        }

        private List<string> GetAOProductsForActivity(UserBatchProductDetail inputAOItem, bool isAssigned, int statusTypeId)
        {
            List<string> aoProducts = new List<string>();
            var aoProductList = JsonConvert.DeserializeObject<AoUserCompanyPropertyRoleDetails>(inputAOItem.InputJSON.Trim());
            if (aoProductList != null && aoProductList.AoUserCompanyPropertyRoleDetailList != null)
            {
                var aoBenchMarkingProduct = aoProductList.AoUserCompanyPropertyRoleDetailList.FirstOrDefault(m => m.ProductId == (int)ProductEnum.AoBenchmarking);
                if (aoBenchMarkingProduct != null)
                {
                    aoProductList.AoUserCompanyPropertyRoleDetailList.Remove(aoBenchMarkingProduct);
                }
                var aoAssignProducts = statusTypeId == 8 ? aoProductList.AoUserCompanyPropertyRoleDetailList.Where(m => m.IsAssigned == isAssigned).ToList() : aoProductList.AoUserCompanyPropertyRoleDetailList;
                foreach (var aoAssignProduct in aoAssignProducts)
                {
                    aoProducts.Add(ProductEnumHelper.GetAoProductDescription((ProductEnum)aoAssignProduct.ProductId));
                }
            }
            return aoProducts;
        }
        private void SendNotification(string message, long notificationTo)
        {
            string title = "User Update Exception";
            List<string> users = new List<string>() { notificationTo.ToString() };

            var productInternalSettingList = GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);

            var notificationsApiEndPoint = productInternalSettingList.First(a => a.Name.Equals("NotificationsApiEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            var notificationsEventsEndPoint = productInternalSettingList.First(a => a.Name.Equals("NotificationsEventsEndPoint", StringComparison.OrdinalIgnoreCase)).Value;
            var tokenEndpoint = productInternalSettingList.First(a => a.Name.Equals("TokenEndPoint", StringComparison.OrdinalIgnoreCase)).Value;

            var clientId = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientName", StringComparison.OrdinalIgnoreCase)).Value;
            var apiSecret = Encoding.UTF8.GetString(Convert.FromBase64String(productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value));
            var categoryCode = productInternalSettingList.First(a => a.Name.Equals("NotificationCategoryCode", StringComparison.OrdinalIgnoreCase)).Value;

            RealPage.UnifiedNotifications.Notification notification = new RealPage.UnifiedNotifications.Notification(clientId, apiSecret, tokenEndpoint, notificationsApiEndPoint + "/v1/notifications", notificationsApiEndPoint + "/" + notificationsEventsEndPoint);
            var result = Task.Run(() => notification.SendNotification(title, message, users, categoryCode)).Result;
        }

        private List<ProductInternalSetting> GetProductInternalSettings(int productId)
        {
            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{productId}";
            var productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () =>
            {
                return _productInternalSettingRepository.GetProductInternalSettings(productId).ToList();
            });

            return productInternalSettingList;
        }

        private List<int> GetProductsWithNoProperties()
        {
            var _productsWithNoProperties = new List<int>();
            var upSettingList = GetProductInternalSettings(3);
            var productsWithNoProperties = upSettingList?.FirstOrDefault(ps => ps.Name.Equals("UserAccessDetails_ProductsWithNoProperties", StringComparison.InvariantCultureIgnoreCase))?.Value;
            if (!string.IsNullOrEmpty(productsWithNoProperties))
            {
                foreach (var pId in productsWithNoProperties.Split(','))
                {
                    if (!_productsWithNoProperties.Contains(Convert.ToInt32(pId)))
                    {
                        _productsWithNoProperties.Add(Convert.ToInt32(pId));
                    }
                }
            }
            return _productsWithNoProperties;
        }

        private RolePropertyList AssignPrimaryPropertiesToProductBatchOnUserCreate(ProductUserProperitiesRoles productUser, RolePropertyList roleProp, List<int> productsWithNoProperties)
        {
            IManagePersona _managePersona = new ManagePersona(_defaultUserClaim);
            
            var editorPersona = _managePersona.GetPersona(productUser.CreateUserPersonaId);
            var userPersona = _managePersona.GetPersona(productUser.AssignUserPersonaId);
            _defaultUserClaim.UserRealPageGuid = editorPersona.RealPageId;
            _defaultUserClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
            _defaultUserClaim.OrganizationPartyId = editorPersona.OrganizationPartyId;
            ManageProductBatch manageProductBatch = new ManageProductBatch(_defaultUserClaim);
            _defaultUserClaim.Rights = manageProductBatch.GetPersonaRoleRights(productUser.CreateUserPersonaId, editorPersona.OrganizationPartyId);

            var productInternalSettingsByType = _productInternalSettingRepository.GetProductSettingByType("ProductIntegrationType");
            var productType = productInternalSettingsByType?.FirstOrDefault(p => p.ProductId == productUser.ProductId)?.Value;

            if (productUser.ProductId != 4 && roleProp.UsePrimaryProperties)
            {
                ListResponse propertyList = manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, productUser.ProductId);
                if (propertyList.Records?.Count > 0)
                {
                    roleProp.PropertyList = new List<string>();
                    roleProp.ProductPrimaryProperties = GetSelectedProperties(propertyList, productType);
                    roleProp.PropertyList = roleProp.ProductPrimaryProperties?.Select(p => p.ProductPropertyId).ToList<string>();
                    productUser.InputJson = JsonConvert.SerializeObject(roleProp);
                }
            }
            else if (productUser.ProductId == 4)
            {
                Object productPropertiesRoles =
                        JsonConvert.DeserializeObject<AoUserCompanyPropertyRoleDetails>(productUser.InputJson);

                var aoRoleProp = productPropertiesRoles as AoUserCompanyPropertyRoleDetails;
                IList<AoUserCompanyPropertyRoleDetail> aoGbUserCompanyPropertyRoleDetails = aoRoleProp.AoUserCompanyPropertyRoleDetailList;
                if (aoRoleProp.AoUserCompanyPropertyRoleDetailList != null)
                {
                    foreach (var data in aoGbUserCompanyPropertyRoleDetails)
                    {
                        if (data.UsePrimaryProperties == true && !productsWithNoProperties.Contains(data.ProductId))
                        {
                            ListResponse propertyList = manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, data.ProductId);
                            if (propertyList.Records.Count > 0)
                            {
                                data.ProductPrimaryProperties = GetSelectedProperties(propertyList, productType);
                                List<string> aoPropList = data.ProductPrimaryProperties?.Select(p => p.ProductPropertyId).ToList<string>();
                                data.SelectedPortfolioValues = aoPropList.Select(int.Parse).ToList();
                                if (data.ProductPrimaryProperties == null || data.ProductPrimaryProperties.Count == 0)
                                {
                                    data.IsAssigned = false;
                                }
                            }
                        }
                    }
                }
                productUser.InputJson = JsonConvert.SerializeObject(aoRoleProp);
            }
            return roleProp;
        }

        private List<ProductPrimaryProperties> GetSelectedProperties(ListResponse productResult, string integrationType)
        {
            List<ProductPrimaryProperties> selectedProperties = new List<ProductPrimaryProperties>();
            var productPropertyType = productResult.Records[0].GetType();

            if (productPropertyType == typeof(ProductProperty))
            {
                var productList = productResult.Records.Cast<ProductProperty>();
                foreach (var property in productList)
                {
                    if (property.IsAssigned == true)
                    {
                        ProductPrimaryProperties productPrimaryProperties = new ProductPrimaryProperties
                        {
                            PropertyInstanceId = property.InstanceId
                        };

                        productPrimaryProperties.ProductPropertyId = integrationType.Equals("UPFM", StringComparison.OrdinalIgnoreCase) ? property.Alias : property.ID;

                        selectedProperties.Add(productPrimaryProperties);
                    }
                }
            }
            else if (productPropertyType == typeof(ACProperty))
            {
                foreach (var property in productResult.Records.Cast<ACProperty>())
                {
                    if (property.IsAssigned == true)
                    {
                        ProductPrimaryProperties productPrimaryProperties = new ProductPrimaryProperties
                        {
                            ProductPropertyId = property.Id,
                            PropertyInstanceId = property.InstanceId
                        };
                        selectedProperties.Add(productPrimaryProperties);
                    }
                }
            }
            else if (productPropertyType == typeof(AssetGroup))
            {
                foreach (var property in productResult.Records.Cast<AssetGroup>())
                {
                    if (property.IsAssigned)
                    {
                        ProductPrimaryProperties productPrimaryProperties = new ProductPrimaryProperties
                        {
                            ProductPropertyId = property.AssetID,
                            PropertyInstanceId = property.InstanceId
                        };
                        selectedProperties.Add(productPrimaryProperties);
                    }
                }
            }
            else if (productPropertyType == typeof(OnSiteProperty))
            {
                foreach (var property in productResult.Records.Cast<OnSiteProperty>())
                {
                    if (property.IsAssigned == true)
                    {
                        ProductPrimaryProperties productPrimaryProperties = new ProductPrimaryProperties
                        {
                            ProductPropertyId = property.GetPropertyId.ToString(),
                            PropertyInstanceId = property.InstanceId
                        };
                        selectedProperties.Add(productPrimaryProperties);
                    }
                }
            }
            else if (productPropertyType == typeof(RumPropertyGroup))
            {
                foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
                {
                    if (property.IsAssigned == true)
                    {
                        ProductPrimaryProperties productPrimaryProperties = new ProductPrimaryProperties
                        {
                            ProductPropertyId = property.Id.ToString(),
                            PropertyInstanceId = property.InstanceId
                        };
                        selectedProperties.Add(productPrimaryProperties);
                    }
                }
            }
            else if (productPropertyType == typeof(ProductProperties))
            {
                foreach (var property in productResult.Records.Cast<ProductProperties>())
                {
                    if (property.IsAssigned == true)
                    {
                        ProductPrimaryProperties productPrimaryProperties = new ProductPrimaryProperties
                        {
                            ProductPropertyId = property.GetPropertyId.ToString(),
                            PropertyInstanceId = property.InstanceId
                        };
                        selectedProperties.Add(productPrimaryProperties);
                    }
                }
            }
            else if (productPropertyType == typeof(Portfolio))
            {
                foreach (var property in productResult.Records.Cast<Portfolio>())
                {
                    if (property.IsAssigned == true)
                    {
                        ProductPrimaryProperties productPrimaryProperties = new ProductPrimaryProperties
                        {
                            ProductPropertyId = property.ID.ToString(),
                            PropertyInstanceId = property.InstanceId
                        };
                        selectedProperties.Add(productPrimaryProperties);
                    }
                }
            }
            return selectedProperties;
        }

        private void SyncUserProductProperties(int productId, long personaId, long editorPersonaId)
        {

            try
            {
                var productInternalSettingList = GetProductInternalSettings(3);
                var baseApiUri = productInternalSettingList.First(a => a.Name.Equals("UnifiedLoginApiBaseUri", StringComparison.OrdinalIgnoreCase)).Value;
                string ulInternalClientTokenScopes = productInternalSettingList.First(a => a.Name.Equals("ULInternalClientTokenScopes", StringComparison.OrdinalIgnoreCase)).Value;

                var uri = $"/apicore/v2/UserSync?syncJobType=2&forceCreate=false&editorPersonaId={editorPersonaId}";

                var products = _productRepository.GetAllProducts();
                string productCode = ProductEnumHelper.GetBooksSourceCodeByProductId(productId, products);

                List<UserSyncRequest> userSyncRequest = new List<UserSyncRequest>();
                List<string> sources = new List<string>();
                sources.Add(productCode);
                UserSyncRequest syncRequest = new UserSyncRequest
                {
                    PersonaId = personaId,
                    Sources = sources,
                    ForceCreate = false
                };
                userSyncRequest.Add(syncRequest);

                Dictionary<string, object> logData = new Dictionary<string, object>()
                {
                    {"UserSyncRequest",  userSyncRequest}
                };

                var ulClientToken = _tokenHelper.GetUnifiedLoginServerToken(ulInternalClientTokenScopes);
                logData.Add("UlClientToken", ulClientToken);
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ulClientToken);
                    httpClient.BaseAddress = new Uri(baseApiUri);

                    var payload = new StringContent(JsonConvert.SerializeObject(userSyncRequest), Encoding.UTF8, "application/json");

                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "SyncUserProductProperties", $"Sending User Sync Request from {baseApiUri} {uri}" });
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        Content = payload,
                        RequestUri = new Uri(String.Concat(baseApiUri, uri)),
                    };
                    var response = httpClient.SendAsync(request).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;

                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        var logErrorData = new Dictionary<string, object>()
                        {
                            { "UserSyncRequest responseContent", responseContent }
                        };
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", logErrorData, messageProperties: new object[] { "SyncUserProductProperties", "Error during User Sync Request." });
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error,
                    "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "SyncUserProductProperties", $"Error while posting SyncUserProductProperties for persona {personaId} and product {productId}." });
            }

        }
        #endregion
        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        /// <param name="exception"></param>
        public void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            string correlationId = "";
            if (_defaultUserClaim != null)
            {
                correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";

            }
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId);
            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }

    }


    #region Interfaces
    /// <summary>
    /// The 'Product' abstract class
    /// </summary>
    interface IProduct
    {
        string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolepropList);

        string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails, bool internalChange);

        /// <summary>
        /// Update product user profile
        /// </summary>
        /// <returns>String.empty if success else error</returns>
        string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId);

        /// <summary>
        /// Change Product User Type from admin to regular or vice versa
        /// </summary>
        /// <returns>String.empty if success else error</returns>
        string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList);
    }

    /// <summary>
    /// The 'Product' abstract class
    /// </summary>
    interface IUPFMProduct
    {
        string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolepropList, out List<AdditionalParameters> additionalParameters);

        string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false);

        /// <summary>
        /// Update product user profile
        /// </summary>
        /// <returns>String.empty if success else error</returns>
        string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId);

        /// <summary>
        /// Change Product User Type from admin to regular or vice versa
        /// </summary>
        /// <returns>String.empty if success else error</returns>
        //string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList, ProductEnum productName);
        string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object productPropertiesRoles);
    }
    #endregion

    #region ProductBase
    /// <summary>
    /// ProductBase
    /// </summary>
    public class ProductBase
    {
        /// <summary>
        /// Product Id
        /// </summary>
        public int _productId;

        IProductInternalSettingRepository _productInternalSettingRepository;
        IProductRepository _productRepository;
        DefaultUserClaim _userClaim;
        private SaveInteralSamlAttrLog _activityLogHelper;
        private SamlRepository _samlRepository;

        /// <summary>
        /// ProductBase
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public ProductBase(int productId, DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository)
        {
            _productId = productId;
            _userClaim = userClaim;
            _productInternalSettingRepository = productInternalSettingRepository;
            _productRepository = productRepository;
            _activityLogHelper = new SaveInteralSamlAttrLog(_userClaim);
            _samlRepository = new SamlRepository();
        }

        /// <summary>
        /// Update product identifiers for a given user
        /// </summary>
        public string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false) //long assignUserPersonaId, ProductBatchStatusType productStatus, Dictionary<SamlAttributeEnum, string> settingList)
        {
            string updates = string.Empty;

            // Handle all other products than AO
            long assignUserPersonaId = productUserAccountDetails.PersonaId;
            var userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var manageProductBase = new ManageProductBase(productUserAccountDetails.ProductId, userClaim, _productInternalSettingRepository, _productRepository);

            StringBuilder messageTolog = new StringBuilder();
            UserActivityLogInfo fromuserInfo = _activityLogHelper.GetUserActivityLogInfo(_userClaim.PersonaId, _userClaim);
            UserActivityLogInfo touserInfo = _activityLogHelper.GetUserActivityLogInfo(assignUserPersonaId);
            GbProductMap product = _productRepository.ListProducts(productUserAccountDetails.ProductId, null, null, null).First();
            List<string> changedAttribute = new List<string>();
            List<string> changedAttrValues = new List<string>();

            // Update user Employee Id
            if (!string.IsNullOrEmpty(productUserAccountDetails.EmployeeId))
            {
                manageProductBase.UpdateUserEmployeeId(assignUserPersonaId, productUserAccountDetails.EmployeeId);
            }

            //GetProductSamlDetails before update
            IList<SamlAttributes> oldSamlAttributes = _samlRepository.GetProductSamlDetails(assignUserPersonaId, productUserAccountDetails.ProductId);
            var productsWithStatus = _samlRepository.ListAllProductsByPersonaId(productUserAccountDetails.PersonaId, productUserAccountDetails.ProductId, null);

            // Handle AO user products separately 
            if (productUserAccountDetails.ProductId == (int)ProductEnum.AssetOptimizer)
            {
                if(productUserAccountDetails.Origin.Equals("UL"))
                {
                    var productlist = ProductEnumHelper.GetAoProductList();
                    var subProuctsSelected = productUserAccountDetails.SubProducts.ToList();
                    foreach(var personaproduct in productsWithStatus)
                    {
                        //Deleting AO User SAML Attribute details not in subProducts list
                        if (productlist.Contains((ProductEnum)personaproduct.ProductId))
                        {
                            string productCode = ProductEnumHelper.GetAoProductId((ProductEnum)personaproduct.ProductId);
                            if (!subProuctsSelected.Contains(productCode))
                            {
                                manageProductBase.DeleteSamlUserProductInfoAndStatus(assignUserPersonaId, (int)ProductEnumHelper.GetAoProductEnum(productCode));
                                var logMessage = $"{fromuserInfo.FirstName} {fromuserInfo.LastName} " +
                                    $"deleted user information of {touserInfo.FirstName} {touserInfo.LastName} " +
                                    $"for {personaproduct.ProductName}.";
                                _activityLogHelper.PushToQueue(fromuserInfo, touserInfo, logMessage, "USER_UPDATE_INTERNAL");
                            }
                        }
                    }
                }

                updates = UpdateAoUserDetails(productUserAccountDetails);
                if (internalChange)
                {
                    var productNameString = product.Name;
                    GenerateInternalUpdateAttrLogMessage(assignUserPersonaId, changedAttribute, changedAttrValues, oldSamlAttributes, productUserAccountDetails.ProductId);
                    GenerateInternalUpdateStatusLogMessage(productUserAccountDetails, changedAttribute, changedAttrValues, productsWithStatus);

                    var aoProducts = _productRepository.ListProducts(null, null, null, null);
                    var subProucts = productUserAccountDetails.SubProducts.ToList();
                    var newlySelectedAOProducts = aoProducts.Where(p => subProucts.Any(p2 => p2 == p.BooksProductCode)).ToList();

                    string aoProductString = " ( " + String.Join(", ", newlySelectedAOProducts.Select(p => p.Name)) + " )";

                    if (newlySelectedAOProducts.Count > 0)
                    {
                        productNameString += aoProductString.ToString();
                    }

                    CreateInternalUpdateLogMessage(messageTolog, fromuserInfo, touserInfo, changedAttribute, changedAttrValues, productNameString);
                }
            }
            else
            {
                manageProductBase.UpdateSamlUserAttributes(assignUserPersonaId, productUserAccountDetails.ProductSettings);
                manageProductBase.UpdateProductSettingProductStatus(assignUserPersonaId,
                    ManageProductBase._productSettingType_ProductStatus, (int)productUserAccountDetails.ProductStatus);

                if (internalChange)
                {
                    GenerateInternalUpdateAttrLogMessage(assignUserPersonaId, changedAttribute, changedAttrValues, oldSamlAttributes, productUserAccountDetails.ProductId);
                    GenerateInternalUpdateStatusLogMessage(productUserAccountDetails, changedAttribute, changedAttrValues, productsWithStatus);
                    if (changedAttrValues.Count > 0)
                    {
                        CreateInternalUpdateLogMessage(messageTolog, fromuserInfo, touserInfo, changedAttribute, changedAttrValues, product.Name);
                    }
                }

            }
            if (messageTolog.Length > 0)
            {
                _activityLogHelper.PushToQueue(fromuserInfo, touserInfo, messageTolog.ToString(), "USER_UPDATE_INTERNAL");
            }

            return updates;
        }

        private void CreateInternalUpdateLogMessage(StringBuilder messageTolog, UserActivityLogInfo fromuserInfo,
            UserActivityLogInfo touserInfo, List<string> changedAttribute, List<string> changedAttrValues, string productName)
        {
            string commaAttributes = string.Join(", ", changedAttribute);

            var lastComma = commaAttributes.LastIndexOf(',');

            if (lastComma != -1)
                commaAttributes = commaAttributes.Remove(lastComma, 1).Insert(lastComma, " and");

            if (commaAttributes.Length > 0)
            {
                messageTolog.Append($"{fromuserInfo.FirstName} {fromuserInfo.LastName} updated {commaAttributes} " +
                    $"of {touserInfo.FirstName} {touserInfo.LastName} for {productName}");
                if (!string.IsNullOrEmpty(fromuserInfo.ClientCode))
                {
                    messageTolog.Append($" by Client {fromuserInfo.ClientCode}");
                }
                messageTolog.Append($". \n");
            }
            else
            {
                messageTolog.Append($"{fromuserInfo.FirstName} {fromuserInfo.LastName} updated {touserInfo.FirstName} {touserInfo.LastName} for {productName}.");
            }

            foreach (var item in changedAttrValues)
            {
                messageTolog.AppendLine(item);
            }
        }

        private void GenerateInternalUpdateStatusLogMessage(ProductUserAccountDetails productUserAccountDetails, List<string> changedAttribute, List<string> changedAttrValues, IList<PersonaProductUserDetails> productsWithStatus)
        {
            // var productsWithStatus = _samlRepository.ListAllProductsByPersonaId(productUserAccountDetails.PersonaId, productUserAccountDetails.ProductId, null);

            if (productsWithStatus.Count > 0)
            {
                var productWithStatus = productsWithStatus.FirstOrDefault(x => x.ProductId == productUserAccountDetails.ProductId);

                if (productWithStatus == null)
                {
                    if (!string.IsNullOrWhiteSpace(productUserAccountDetails.ProductStatus.ToString()))
                    {
                        changedAttribute.Add("status");
                        changedAttrValues.Add($"From Status : \"NONE\" to \"{productUserAccountDetails.ProductStatus}\".");
                    }
                }
                else
                {
                    int oldStatusId = productWithStatus.ProductStatus;
                    int newStatusId = (int)productUserAccountDetails.ProductStatus;

                    if (oldStatusId != newStatusId)
                    {
                        var oldstatus = (ProductBatchStatusType)oldStatusId;
                        var newStatus = productUserAccountDetails.ProductStatus;

                        changedAttribute.Add("status");
                        changedAttrValues.Add($"From Status : \"{oldstatus.ToString()}\" to \"{newStatus.ToString()}\".");
                    }
                }
            }
        }

        private void GenerateInternalUpdateAttrLogMessage(long assignUserPersonaId, List<string> changedAttribute, List<string> changedAttrValues, IList<SamlAttributes> oldSamlAttributes, int productId)
        {
            var newSamlAttributes = _samlRepository.GetProductSamlDetails(assignUserPersonaId, productId);

            if (oldSamlAttributes.Count == 0 && newSamlAttributes.Count > 0)
            {
                foreach (var item in newSamlAttributes)
                {
                    changedAttribute.Add(item.DisplayName);
                    changedAttrValues.Add($"From {item.DisplayName} : \"NONE\" to \"{item.Value}\".");
                }

            }
            else //else: it will be updating the existing values
            {
                if (newSamlAttributes.Count > 0)
                {
                    foreach (var newAttribute in newSamlAttributes)
                    {
                        var a = oldSamlAttributes.Where(x => x.Name.Equals(newAttribute.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        if (a != null)
                        {
                            var oldAttribute = oldSamlAttributes.FirstOrDefault(x => x.Name.Equals(newAttribute.Name, StringComparison.OrdinalIgnoreCase));

                            if (!oldAttribute.Value.Equals(newAttribute.Value, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrWhiteSpace(oldAttribute.Value))
                                {
                                    //updated
                                    changedAttribute.Add(oldAttribute.DisplayName);
                                    changedAttrValues.Add($"From {oldAttribute.DisplayName} : \"{oldAttribute.Value}\" to \"{newAttribute.Value}\". ");
                                }
                            }
                        }
                        else
                        {
                            //added
                            changedAttribute.Add(newAttribute.DisplayName);
                            changedAttrValues.Add($"From {newAttribute.DisplayName} : \"None\" to \"{newAttribute.Value}\". ");
                        }
                    }
                }
            }
        }

        private string UpdateAoUserDetails(ProductUserAccountDetails productUserAccountDetails)
        {
            // Default AO record
            long assignUserPersonaId = productUserAccountDetails.PersonaId;

            var manageProductBase = new ManageProductBase(_productId, _userClaim, _productInternalSettingRepository, _productRepository);

            manageProductBase.UpdateSamlUserAttributes(assignUserPersonaId, productUserAccountDetails.ProductSettings);
            manageProductBase.UpdateProductSettingProductStatus(assignUserPersonaId,
                ManageProductBase._productSettingType_ProductStatus, (int)ProductEnum.AssetOptimizer, (int)productUserAccountDetails.ProductStatus);

            foreach (var product in productUserAccountDetails.SubProducts)
            {
                manageProductBase.UpdateSamlUserAttributes(assignUserPersonaId, productUserAccountDetails.ProductSettings, (int)ProductEnumHelper.GetAoProductEnum(product));
                manageProductBase.UpdateProductSettingProductStatus(assignUserPersonaId,
                    ManageProductBase._productSettingType_ProductStatus, (int)ProductEnumHelper.GetAoProductEnum(product), (int)productUserAccountDetails.ProductStatus);
            }

            return string.Empty;
        }

        /// <summary>
        /// User claim related information
        /// </summary>
        public DefaultUserClaim UserClaim
        {
            get { return _userClaim; }
        }
    }

    public class SaveInteralSamlAttrLog
    {
        private IManagePersona _managePersona;
        private IManagePerson _managePerson;
        private IManageUserLogin _manageUserLogin;
        private DefaultUserClaim _defaultUserClaim;
        private IManageOrganization _manageOrganization;

        public SaveInteralSamlAttrLog(DefaultUserClaim defaultUserClaim)
        {
            _defaultUserClaim = defaultUserClaim;
            _managePersona = new ManagePersona(_defaultUserClaim);
            _managePerson = new ManagePerson();
            _manageUserLogin = new ManageUserLogin(_defaultUserClaim);
            _manageOrganization = new ManageOrganization(_defaultUserClaim);
        }

        public UserActivityLogInfo GetUserActivityLogInfo(long personaId, DefaultUserClaim userClaim = null)
        {
            if (personaId == 0)
            {
                Guid employeeRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(DefaultUserClaim.EmployeeCompanyRealPageId);
                var person = _managePerson.GetPerson(employeeRealPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(employeeRealPageId);
                var persona = _managePersona.GetActivePersona(employeeRealPageId);
                return new UserActivityLogInfo
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    RealPageId = userLogin.RealPageId,
                    LoginName = userLogin.LoginName,
                    BooksOrganizationMasterId = persona.Organization.BooksMasterId,
                    OrganizationPartyId = persona.OrganizationPartyId,
                    OrganizationName = persona.Organization.Name,
                    UserId = userLogin.UserId,
                    ClientCode = userClaim?.ClientCode
                };
            }
            else
            {
                var persona = _managePersona.GetPersona(personaId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(persona.RealPageId);
                var person = _managePerson.GetPerson(persona.RealPageId);
                return new UserActivityLogInfo
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    RealPageId = userLogin.RealPageId,
                    LoginName = userLogin.LoginName,
                    BooksOrganizationMasterId = persona.Organization.BooksMasterId,
                    OrganizationPartyId = persona.OrganizationPartyId,
                    OrganizationName = persona.Organization.Name,
                    UserId = userLogin.UserId,
                    OrganizationRealpageId = persona.Organization.RealPageId
                };
            }
        }

        public void PushToQueue(UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo, String message, string logActivityType, List<AdditionalParameters> additionalParameters = null)
        {
            try
            {
                string activityName = string.Empty;
                string categoryName = string.Empty;

                if (logActivityType == "PRODUCT_ACCESS")
                {
                    activityName = LogActivityTypeConstants.PRODUCT_ACCESS;
                    categoryName = LogActivityCategoryType.ProductAccess.ToString();
                }
                else if (logActivityType == "USER_UPDATE_INTERNAL")
                {
                    activityName = LogActivityTypeConstants.USER_UPDATE_INTERNAL;
                    categoryName = LogActivityCategoryType.CompanySetup.ToString();
                }

                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = activityName,
                    LogCategoryName = categoryName,
                    CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = fromUserLogInfo.BooksOrganizationMasterId,
                    OrganizationPartyId = fromUserLogInfo.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = fromUserLogInfo.LoginName,
                    FromUserLoginId = fromUserLogInfo.UserId,
                    FromUserFirstName = fromUserLogInfo.FirstName,
                    FromUserLastName = fromUserLogInfo.LastName,
                    FromUserRealpageId = fromUserLogInfo.RealPageId.ToString(),

                    ToUserLoginId = toUserLogInfo.UserId,
                    ToUserLoginName = toUserLogInfo.LoginName,
                    ToUserFirstName = toUserLogInfo.FirstName,
                    ToUserLastName = toUserLogInfo.LastName,
                    ToUserRealpageId = toUserLogInfo.RealPageId.ToString(),
                    AdditionalInformation = additionalParameters
                });
            }
            catch (Exception ex)
            {

            }
        }
    }
    #endregion

    #region OneSite
    /// <summary>
    /// A 'Concrete Product OneSite' class
    /// </summary>
    public class OneSiteProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public OneSiteProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.OneSite, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public OneSiteProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.OneSite, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create OneSite user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">OneSite Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            // try initally getting just the Lead2Lease data
            var roleProp = rolePropList as RolePropertyList;
            var combinedRoleProp = new Dictionary<string, RolePropertyList>();
            string productResult = "";

            if (roleProp == null)
            {
                // the single data failed so attempt to parse Lead2Lease and OneSite as a combined product
                combinedRoleProp = rolePropList as Dictionary<string, RolePropertyList>;

                if (!combinedRoleProp.Any())
                {
                    return "Input JSON parsing issue; Null object.";
                }

                if (combinedRoleProp.Any(p => p.Key == ProductEnum.OneSite.ToString()))
                {
                    roleProp = combinedRoleProp.Where(p => p.Key == ProductEnum.OneSite.ToString()).First().Value;
                }
            }

            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var oneSite = new ManageProductOneSite(base.UserClaim);

            //OneSite
            if (roleProp.IsAssigned)
            {
                productResult = oneSite.ManageOneSiteUser(createUserPersonaId, assignUserPersonaId, roleProp.RoleList, roleProp.PropertyList, false);
            }
            else
            {
                //Unassign Usr
                productResult = oneSite.UnassignUser(createUserPersonaId, assignUserPersonaId);
            }

            if (!string.IsNullOrEmpty(productResult))
            {
                return productResult;
            }

            //Lead2Lease
            if (combinedRoleProp.Any(p => p.Key == ProductEnum.Lead2Lease.ToString()) && string.IsNullOrEmpty(productResult))
            {
                var productLead2Lease = new ManageProductLead2Lease(base.UserClaim);

                RolePropertyList lead2Lease = combinedRoleProp.Where(p => p.Key == ProductEnum.Lead2Lease.ToString()).First().Value;

                // assign user
                if (roleProp.IsAssigned)
                {
                    productResult = productLead2Lease.ManageLead2LeaseUser(createUserPersonaId, assignUserPersonaId, lead2Lease.RoleList, lead2Lease.PropertyList);
                }
                else
                {
                    // Unassign User
                    productResult = productLead2Lease.UnassignUser(createUserPersonaId, assignUserPersonaId);
                }
            }

            if (!string.IsNullOrEmpty(productResult))
            {
                return productResult;
            }

            //SeniorLeadManagement
            if (combinedRoleProp.Any(p => p.Key == ProductEnum.SeniorLeadManagement.ToString()) && string.IsNullOrEmpty(productResult))
            {
                var productSeniorLeadManagement = new SeniorLeadManagementProduct(base.UserClaim);

                // assign user
                if (roleProp.IsAssigned)
                {
                    productResult = productSeniorLeadManagement.CreateUser(createUserRealPageId, createUserPersonaId, assignUserPersonaId, rolePropList);
                }
                else
                {
                    // Unassign User
                    productResult = productSeniorLeadManagement.UpdateProductUserProfile(createUserRealPageId, createUserPersonaId, assignUserPersonaId);
                }
            }

            return productResult;
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var os = new ManageProductOneSite(base.UserClaim);
            List<string> RoleList = new List<string>();
            List<string> PropertyList = new List<string>();
            return os.ManageOneSiteUser(createUserPersonaId, assignUserPersonaId, RoleList, PropertyList, true);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">OneSite Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            string changeProductUserTypeResponse = string.Empty;
            bool isUserDemoted = false;
            // try initially getting just the OneSite data
            var rpList = rolePropList as RolePropertyList;
            var combinedRoleProp = new Dictionary<string, RolePropertyList>();
            if (rpList == null)
            {
                // the single data failed so attempt to parse combined product data
                combinedRoleProp = rolePropList as Dictionary<string, RolePropertyList>;
                if (combinedRoleProp == null)
                {
                    return "Input JSON parsing issue; Null object.";
                }

                rpList = combinedRoleProp.Where(p => p.Key == ProductEnum.OneSite.ToString()).First().Value;
            }

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal) &&
                rpList.PropertyList.Count == 0 && rpList.RoleList.Count == 0 && !rpList.IsAssigned)
            {
                isUserDemoted = true;
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a OneSite user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a OneSite user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var os = new ManageProductOneSite(base.UserClaim);

            os.WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "rolePropList", rolePropList } }, messageProperties: new object[] { "ChangeProductUserType", "OneSite Begin" });
            // Unassign User
            bool deleteSamlUserProductInfoAndStatus = true;
            changeProductUserTypeResponse = os.UnassignUser(createUserPersonaId, assignUserPersonaId, deleteSamlUserProductInfoAndStatus);
            if (string.IsNullOrWhiteSpace(changeProductUserTypeResponse) && !isUserDemoted)
            {
                changeProductUserTypeResponse = os.ManageOneSiteUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList, false);
            }

            var lead2leaseresult = "";
            if (combinedRoleProp.Any(p => p.Key == ProductEnum.Lead2Lease.ToString()))
            {
                os.WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeProductUserType", "Adding Lead2Lease" });
                rpList = combinedRoleProp.Where(p => p.Key == ProductEnum.Lead2Lease.ToString()).First().Value;
                var productLead2Lease = new ManageProductLead2Lease(base.UserClaim);
                productLead2Lease.WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeProductUserType", "UnassignUser user in lead2ease" });
                // Unassign User
                lead2leaseresult = productLead2Lease.UnassignUser(createUserPersonaId, assignUserPersonaId);
                if (string.IsNullOrEmpty(lead2leaseresult) && !isUserDemoted)
                {
                    productLead2Lease.WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeProductUserType", "Reassign User" });
                    // assign user
                    lead2leaseresult = productLead2Lease.ManageLead2LeaseUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList);
                }

                if (!string.IsNullOrEmpty(lead2leaseresult))
                {
                    changeProductUserTypeResponse += lead2leaseresult;
                }
                productLead2Lease.WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeProductUserType", $"Lead2Lease result: {lead2leaseresult}" });
            }

            return changeProductUserTypeResponse;
        }
    }
    #endregion

    #region Marketing Center
    /// <summary>
    /// A 'Concrete Product Marketing Center' class
    /// </summary>
    public class MarketingCenterProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public MarketingCenterProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.MarketingCenter, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public MarketingCenterProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.MarketingCenter, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        ///  Create Marketing Center User
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Marketing Center Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as MarketingCenterRoleAndPropertyList;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductMarketingCenter(base.UserClaim);

            if (rpList.IsAssigned)
            {
                return mc.ManageMarketingCenterUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList,
                    rpList.PropertyList, rpList.IsAssignedNewPropertyByDefault);
            }

            // Unassign User
            return mc.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductMarketingCenter(base.UserClaim);

            return mc.UpdateUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">OneSite Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            string changeProductUserTypeResponse = string.Empty;

            var rpList = rolePropList as MarketingCenterRoleAndPropertyList;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Marketing Center user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Marketing Center user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductMarketingCenter(base.UserClaim);

            // Unassign User
            changeProductUserTypeResponse = mc.UnassignUser(createUserPersonaId, assignUserPersonaId);
            if (string.IsNullOrWhiteSpace(changeProductUserTypeResponse))
            {
                changeProductUserTypeResponse = mc.ManageMarketingCenterUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList, rpList.IsAssignedNewPropertyByDefault);
            }
            return changeProductUserTypeResponse;
        }
    }
    #endregion

    #region Accounting
    /// <summary>
    /// A 'Concrete Product Accounting' class
    /// </summary>
    public class OneSiteAccountingProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public OneSiteAccountingProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.FinancialSuite, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public OneSiteAccountingProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.FinancialSuite, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        ///  Create Accounting Center User
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Accounting Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as AccountingRoleAndPropertyList;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductOneSiteAccounting(base.UserClaim);

            if (rpList.IsAssigned)
            {
                return mc.ManageAccountingUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList,
                    rpList.PropertyList, rpList.CompaniesList, rpList.IsAccountingAdmin, rpList.HasAccessToSiteSpendManagementOnly, rpList.HasAccessToAllCurrentFutureProperties);
            }

            // Unassign User
            return mc.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductOneSiteAccounting(base.UserClaim);
            return mc.UpdateAccountingUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Accounting Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as AccountingRoleAndPropertyList;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Accounting user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Accounting user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductOneSiteAccounting(base.UserClaim);

            // Change user type (Update User)
            return mc.ChangeAccountingServiceUserType(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList, rpList.CompaniesList,
                     rpList.IsAccountingAdmin, rpList.HasAccessToSiteSpendManagementOnly, rpList.HasAccessToAllCurrentFutureProperties, batchProcessType);
        }
    }
    #endregion

    #region Ops
    /// <summary>
    /// A 'Concrete Product Ops' class
    /// </summary>
    public class OpsProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public OpsProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.OpsBuyer, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public OpsProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.OpsBuyer, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        ///  Create Ops User
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Ops Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as OpsRoleAndPropertyList;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductOps(base.UserClaim);

            // Assign User
            if (rpList.IsAssigned)
            {
                return mc.ManageOpsUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList);
            }

            // Unassign User
            return mc.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductOps(base.UserClaim);
            return mc.UpdateOPSUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Ops Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            string changeProductUserTypeResponse = string.Empty;

            var rpList = rolePropList as OpsRoleAndPropertyList;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Ops user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Ops user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductOps(base.UserClaim);

            changeProductUserTypeResponse = mc.ManageOpsUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList);
            return changeProductUserTypeResponse;
        }
    }
    #endregion

    #region Vendor Credentialing
    /// <summary>
    /// A 'Concrete implementation for Vendor Credentialing
    /// </summary>
    public class VendorServicesProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public VendorServicesProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.VendorServices, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public VendorServicesProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.VendorServices, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Vendor Credentialing user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Vendor Credentialing Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as UserProductPropertyNotification;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productVendorServices = new ManageProductVendorServices(base.UserClaim);

            // assign user
            if (rpList.IsAssigned)
            {
                return productVendorServices.ManageVendorServicesUser(createUserPersonaId, assignUserPersonaId, rpList);
            }

            // Unassign User
            return productVendorServices.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>		
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productVC = new ManageProductVendorServices(base.UserClaim);

            return productVC.UpdateVendorServicesUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Vendor Credentialing Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as UserProductPropertyNotification;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productVendorServices = new ManageProductVendorServices(base.UserClaim);

            // Change user type (Update User)
            return productVendorServices.ChangeVendorServiceUserType(createUserPersonaId, assignUserPersonaId, rpList, batchProcessType);
        }
    }
    #endregion

    #region Client Portal
    /// <summary>
    /// A 'Concrete implementation for client portal
    /// </summary>
    public class ClientPortalProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public ClientPortalProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.ClientPortal, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public ClientPortalProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.ClientPortal, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Client Portal user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Client Portal Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as ClientPortalPropertyRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productClientPortal = new ManageProductClientPortal(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productClientPortal.ManageClientPortalUser(createUserPersonaId, assignUserPersonaId, roleProp);
            }

            // Unassign User
            return productClientPortal.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Client Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productCP = new ManageProductClientPortal(base.UserClaim);

            return productCP.UpdateClientPortalUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Client Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var roleProp = rolePropList as ClientPortalPropertyRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productCP = new ManageProductClientPortal(base.UserClaim);

            return productCP.ManageClientPortalUser(createUserPersonaId, assignUserPersonaId, roleProp);
        }
    }
    #endregion

    #region Admin Support Portal
    /// <summary>
    /// A 'Concrete implementation for Admin Support Portal
    /// </summary>
    public class AdminSupportPortalProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public AdminSupportPortalProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.AdminSupportPortal, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public AdminSupportPortalProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.AdminSupportPortal, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Admin Support Portal
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Admin Support Portal Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as AdminSupportPortalPropertyRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productAdminSupportPortal = new ManageProductAdminSupportPortal(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productAdminSupportPortal.ManageAdminSupportPortalUser(createUserPersonaId, assignUserPersonaId, roleProp);
            }

            // Unassign User
            return productAdminSupportPortal.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Admin Support Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productASP = new ManageProductAdminSupportPortal(base.UserClaim);

            return productASP.UpdateAdminSupportPortalUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Admin Support Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var roleProp = rolePropList as AdminSupportPortalPropertyRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productASP = new ManageProductAdminSupportPortal(base.UserClaim);

            return productASP.ManageAdminSupportPortalUser(createUserPersonaId, assignUserPersonaId, roleProp);
        }
    }
    #endregion


    #region SalesForce
    /// <summary>
    /// A 'Concrete implementation for SalesForce
    /// </summary>
    public class SalesForceProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public SalesForceProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.SalesForce, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public SalesForceProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.SalesForce, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Client Portal user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">SalesForce Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as ClientPortalPropertyRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productSalesForce = new ManageProductClientPortal(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productSalesForce.ManageSalesForceUser(createUserPersonaId, assignUserPersonaId, roleProp);
            }

            // Unassign User
            return productSalesForce.UnassignSalesForceUser(createUserPersonaId, assignUserPersonaId, roleProp);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">SalseForce Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">SalseForce Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Prospect Contact Center
    /// <summary>
    /// A 'Concrete implementation for Prospect Contact Center 
    /// </summary>
    public class ProspectContactCenterProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ProspectContactCenterProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.ProspectContactCenter, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public ProspectContactCenterProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.ProspectContactCenter, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Prospect Contact Center user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Prospect Contact Center Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as ProspectContactPropertyRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productProspectContactCenter = new ManageProductProspectContact(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productProspectContactCenter.ManageProductProspectContactUser(createUserPersonaId, assignUserPersonaId, roleProp);
            }

            // Unassign User
            return productProspectContactCenter.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productProspectContactCenter = new ManageProductProspectContact(base.UserClaim);
            return productProspectContactCenter.UpdateProspectContactCenterUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Prospect Contact Center Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var roleProp = rolePropList as ProspectContactPropertyRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productProspectContactCenter = new ManageProductProspectContact(base.UserClaim);
            return productProspectContactCenter.ChangeProspectContactUserType(createUserPersonaId, assignUserPersonaId, roleProp, batchProcessType);
        }
    }
    #endregion

    #region Lead2Lease
    /// <summary>
    /// A 'Concrete implementation for Lead2Lease
    /// </summary>
    public class Lead2LeaseProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public Lead2LeaseProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.Lead2Lease, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public Lead2LeaseProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.Lead2Lease, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Lead2Lease user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Lead2Lease Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as RolePropertyList;

            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;

            var productLead2Lease = new ManageProductLead2Lease(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productLead2Lease.ManageLead2LeaseUser(createUserPersonaId, assignUserPersonaId, roleProp.RoleList, roleProp.PropertyList);
            }

            // Unassign User
            return productLead2Lease.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productLead2Lease = new ManageProductLead2Lease(base.UserClaim);
            return productLead2Lease.UpdateLead2LeaseUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Lead2Lease Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            string changeProductUserTypeResponse = string.Empty;
            var roleProp = rolePropList as RolePropertyList;

            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (roleProp.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Lease2Lease user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (roleProp.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Lease2Lease user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productLead2Lease = new ManageProductLead2Lease(base.UserClaim);

            // Unassign User
            changeProductUserTypeResponse = productLead2Lease.UnassignUser(createUserPersonaId, assignUserPersonaId);
            if (string.IsNullOrEmpty(changeProductUserTypeResponse))
            {
                // assign user
                changeProductUserTypeResponse = productLead2Lease.ManageLead2LeaseUser(createUserPersonaId, assignUserPersonaId, roleProp.RoleList, roleProp.PropertyList);
            }

            return changeProductUserTypeResponse;
        }
    }
    #endregion

    #region Resident Portal
    /// <summary>
    /// A 'Concrete implementation for resident portal
    /// </summary>
    public class ResidentPortalProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public ResidentPortalProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.ResidentPortal, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public ResidentPortalProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.ResidentPortal, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Resident Portal user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Resident Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            ObjectOutput<IResidentPortalUser, IErrorData> output = new ObjectOutput<IResidentPortalUser, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            var roleProp = roleProperty as ResidentPortal;
            if (roleProp == null)
            {
                throw new Exception("Input JSON parsing issue; Null object.");
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            ManageProductResidentPortal productResidentPortal = new ManageProductResidentPortal(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                output = productResidentPortal.ManageResidentPortalUser(createUserPersonaId, assignUserPersonaId, roleProp);
                return output.Status.ErrorMsg;
            }

            // Unassign User
            output = productResidentPortal.UnassignResidentPortalUser(createUserPersonaId, assignUserPersonaId);
            return output.Status.ErrorMsg;
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            ObjectOutput<IResidentPortalUser, IErrorData> objectOutput = new ObjectOutput<IResidentPortalUser, IErrorData>();
            string changeProductUserTypeResponse = string.Empty;

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base.UserClaim);

            objectOutput = manageProductResidentPortal.ManageResidentPortalUser(createUserPersonaId, assignUserPersonaId, null, BatchProcessType.ProfileUpdate);
            if (objectOutput.Status.Success == false)
            {
                changeProductUserTypeResponse = objectOutput.Status.ErrorMsg;
            }
            return changeProductUserTypeResponse;
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Resident Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            ObjectOutput<IResidentPortalUser, IErrorData> objectOutput = new ObjectOutput<IResidentPortalUser, IErrorData>();
            string changeProductUserTypeResponse = string.Empty;

            var rpList = rolePropList as ResidentPortal;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Resident Portal user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Resident Portal user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base.UserClaim);

            // Unassign User
            objectOutput = manageProductResidentPortal.UnassignResidentPortalUser(createUserPersonaId, assignUserPersonaId);
            if (objectOutput.Status.Success == true)
            {
                objectOutput = manageProductResidentPortal.ManageResidentPortalUser(createUserPersonaId, assignUserPersonaId, rpList, batchProcessType);
                if (objectOutput.Status.Success == false)
                {
                    changeProductUserTypeResponse = objectOutput.Status.ErrorMsg;
                }
            }
            else
            {
                changeProductUserTypeResponse = objectOutput.Status.ErrorMsg;
            }
            return changeProductUserTypeResponse;
        }
    }
    #endregion

    #region OnSite
    /// <summary>
    /// A 'Concrete implementation for On Site Product  
    /// </summary>
    public class OnSiteProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public OnSiteProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.OnSite, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public OnSiteProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.OnSite, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create On Site user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">OnSite Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as OnSiteUserPropertyRegionRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productOnSite = new ManageProductOnSite(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productOnSite.ManageOnSiteUser(createUserPersonaId, assignUserPersonaId, roleProp);
            }

            // Unassign User
            return productOnSite.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">OnSite Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productOnsite = new ManageProductOnSite(base.UserClaim);
            return productOnsite.UpdateOnSiteUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">OnSite Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var roleProp = rolePropList as OnSiteUserPropertyRegionRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productOnSite = new ManageProductOnSite(base.UserClaim);

            return productOnSite.ManageOnSiteUser(createUserPersonaId, assignUserPersonaId, roleProp, batchProcessType);
        }
    }
    #endregion

    #region Utility Management
    /// <summary>
    /// A 'Concrete implementation for Utility Management Product  
    /// </summary>
    public class UtilityManagementProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public UtilityManagementProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.UtilityManagement, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public UtilityManagementProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.UtilityManagement, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Utility Management user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Utility Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as RumUserPropertyRegionRole;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productRum = new ManageProductRum(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productRum.ManageRumUser(createUserPersonaId, assignUserPersonaId, roleProp);
            }

            // Unassign User
            return productRum.UnassignRumUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Utility Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productRum = new ManageProductRum(base.UserClaim);

            return productRum.UpdateUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">OneSite Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            string changeProductUserTypeResponse = string.Empty;

            var rpList = rolePropList as RumUserPropertyRegionRole;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0 && rpList.PropertyGroupList.Count == 0 && rpList.RegionList.Count == 0))
            {
                return "At least one Property or Group or Region is required in the Input JSON when changing a Utility Management user type from Admin to Regular.";
            }


            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var rum = new ManageProductRum(base.UserClaim);

            return rum.ManageRumUser(createUserPersonaId, assignUserPersonaId, rpList);
        }
    }
    #endregion

    #region Omni Channel
    /// <summary>
    /// A 'Concrete implementation for OmniChannel Product
    /// </summary>
    public class OmniChannelProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public OmniChannelProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.OmniChannel, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public OmniChannelProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.OmniChannel, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create OmniChannel user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Omni Channel Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as Component.SharedObjects.Product.OmniChannel.UserAssignProductPropertyRole;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var productOmniChannel = new ManageProductOmniChannel(base.UserClaim);

            // assign user
            if (rpList.IsAssigned)
            {
                return productOmniChannel.ManageOmniChannelUser(createUserPersonaId, assignUserPersonaId, rpList);
            }

            // Unassign User
            return productOmniChannel.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Omni Channel Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Omni Channel Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Research Application
    /// <summary>
    /// A 'Concrete implementation for ResearchApplication - Blackbook Product
    /// </summary>
    public class ResearchApplicationProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public ResearchApplicationProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.ResearchApplication, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public ResearchApplicationProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.ResearchApplication, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create ResearchApplication user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Research Application Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as ResearchAppRoleAndPropertyList;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productResApp = new ManageResearchApplication(base.UserClaim);

            // assign user
            if (rpList.IsAssigned)
            {
                return productResApp.ManageResearchApplicationUser(createUserPersonaId, assignUserPersonaId, rpList);
            }

            // Unassign User
            return productResApp.UnassignUser(createUserPersonaId, assignUserPersonaId, rpList);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Research Application Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Research Application Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Renters Insurance
    /// <summary>
    /// A 'Concrete implementation for renters insurance
    /// </summary>
    public class RentersInsuranceProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public RentersInsuranceProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.ResidentPortal, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public RentersInsuranceProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.Insurance, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Renters Insurance user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Renters Insurance Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            ObjectOutput<UserAPIResponse, IErrorData> output = new ObjectOutput<UserAPIResponse, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            var roleProp = roleProperty as RentersInsuranceRoleAndPropertyList;
            if (roleProp == null)
            {
                throw new Exception("Input JSON parsing issue; Null object.");
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                output = manageProductRentersInsurance.ManageRentersInsuranceUser(createUserPersonaId, assignUserPersonaId, roleProp);
                return output.Status.ErrorMsg;
            }

            // Unassign User
            output = manageProductRentersInsurance.UnassignRentersInsuranceUser(createUserPersonaId, assignUserPersonaId);
            return output.Status.ErrorMsg;
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            ObjectOutput<UserAPIResponse, IErrorData> objectOutput = new ObjectOutput<UserAPIResponse, IErrorData>();
            string changeProductUserTypeResponse = string.Empty;

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(base.UserClaim);

            objectOutput = manageProductRentersInsurance.ManageRentersInsuranceUser(createUserPersonaId, assignUserPersonaId, null, BatchProcessType.ProfileUpdate);
            if (objectOutput.Status.Success == false)
            {
                changeProductUserTypeResponse = objectOutput.Status.ErrorMsg;
            }
            return changeProductUserTypeResponse;
        }


        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="roleProperty">Accounting Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object roleProperty)
        {

            var roleProp = roleProperty as RentersInsuranceRoleAndPropertyList;

            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (roleProp.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Insurance user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (roleProp.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Insurance user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var mc = new ManageProductRentersInsurance(base.UserClaim);
            ObjectOutput<UserAPIResponse, IErrorData> output = new ObjectOutput<UserAPIResponse, IErrorData>();
            // Change user type (Update User)
            output = mc.ChangeRentersInsuranceUserType(createUserPersonaId, assignUserPersonaId, roleProp, batchProcessType);
            return output.Status.ErrorMsg;
        }
    }

    #endregion

    #region Self Provisioning Portal
    /// <summary>
    /// A 'Concrete implementation for Self Provisioning Portal
    /// </summary>
    public class SelfProvisioningPortalProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public SelfProvisioningPortalProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.SelfProvisioningPortal, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public SelfProvisioningPortalProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.SelfProvisioningPortal, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Self-Provisioning Portal user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Self-Provisioning Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            ObjectOutput<ISelfProvisioningPortal, IErrorData> output = new ObjectOutput<ISelfProvisioningPortal, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            var roleProp = roleProperty as SelfProvisioningPortal;
            if (roleProp == null)
            {
                throw new Exception("Input JSON parsing issue; Null object.");
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            ManageProductSelfProvisioningPortal productSelfProvisioningPortal = new ManageProductSelfProvisioningPortal(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                output = productSelfProvisioningPortal.ManageSelfProvisioningPortalUser(createUserPersonaId, assignUserPersonaId, roleProp);
                return output.Status.ErrorMsg;
            }

            // Unassign User
            output = productSelfProvisioningPortal.UnassignSelfProvisioningPortalUser(createUserPersonaId, assignUserPersonaId, roleProp);
            return output.Status.ErrorMsg;
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Self-Provisioning Portal Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList"> Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Asset Optimizer
    /// <summary>
    /// A Concrete implementation for Asset Optimizer Product  
    /// </summary>
    public class AssetOptimizerProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public AssetOptimizerProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.AssetOptimizer, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public AssetOptimizerProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.AssetOptimizer, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create, Update or Unassign AO User
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">AO Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as AoUserCompanyPropertyRoleDetails;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productAo = new ManageProductAssetOptimization(base.UserClaim);

            // create, update or UNASSIGN user
            return productAo.ManageAssetOptimizationUser(createUserPersonaId, assignUserPersonaId, roleProp.AoUserCompanyPropertyRoleDetailList);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>		
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productAo = new ManageProductAssetOptimization(base.UserClaim);

            return productAo.UpdateUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">AO Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var roleProp = rolePropList as AoUserCompanyPropertyRoleDetails;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productAo = new ManageProductAssetOptimization(base.UserClaim);

            // create, update or UNASSIGN user
            return productAo.ChangeAssetOptimizationProductUserType(createUserPersonaId, assignUserPersonaId, roleProp.AoUserCompanyPropertyRoleDetailList, batchProcessType);
        }
    }

    /// <summary>
	/// A Concrete implementation for Business Intelligence Asset Optimizer Product  
	/// </summary>
	public class AoBusinessIntelligenceProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public AoBusinessIntelligenceProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.AoBusinessIntelligence, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public AoBusinessIntelligenceProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.AoBusinessIntelligence, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create, Update or Unassign BI AO User
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">AO Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            return "AO create user should be used instead";
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>		
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            return "AO update user should be used instead";
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">AO Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            return "AO change user type should be used instead";
        }
    }
    #endregion

    #region Lead Management
    /// <summary>
    /// A 'Concrete implementation for lead management 
    /// </summary>
    public class LeadManagementProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public LeadManagementProduct(DefaultUserClaim userClaim, ProductEnum productType) : base((int)productType, userClaim, null, null)
        {
        }

        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public LeadManagementProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.LeadManagement, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create LM user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Lead Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            List<AdditionalParameters> additionalParameters;
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList, out additionalParameters);
            }

            // Unassign User 
            return productLogic.UnassignUser();
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.UpdateProductUserProfile();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">ILM Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.ChangeProductUserType(rpList, batchProcessType);
        }
    }
    #endregion

    #region RP Document Management
    /// <summary>
    /// A 'Concrete implementation for RPDocumentManagement Product  
    /// </summary>
    public class RPDocumentManagementProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public RPDocumentManagementProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.RPDocumentManagement, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public RPDocumentManagementProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.RPDocumentManagement, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create a Document Management user
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="roleProperty">Document Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object roleProperty)
        {
            var roleProp = roleProperty as RolePropertyList;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;

            var productRPDM = new ManageProductRPDocumentManagement(base.UserClaim);

            // assign user
            if (roleProp.IsAssigned)
            {
                return productRPDM.ManageRPDMUser(createUserPersonaId, assignUserPersonaId, roleProp);
            }

            // Unassign User
            return productRPDM.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {

            base.UserClaim.UserRealPageGuid = createUserRealPageId;

            var productRPDM = new ManageProductRPDocumentManagement(base.UserClaim);


            return productRPDM.UpdateRPDMUserProfile(createUserPersonaId, assignUserPersonaId);
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Document Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var roleProp = rolePropList as RolePropertyList;
            if (roleProp == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (roleProp?.DepartmentList?.Count == 0))
            {
                return "At least one Department is required in the Input JSON when changing a Document Management user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (roleProp?.PropertyList?.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Document Management user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (roleProp.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Document Management user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;

            var productRPDM = new ManageProductRPDocumentManagement(base.UserClaim);

            return productRPDM.ManageRPDMUser(createUserPersonaId, assignUserPersonaId, roleProp);
        }
    }
    #endregion

    #region Portfolio Management
    /// <summary>
    /// A 'Concrete implementation for Portfolio Management 
    /// </summary>
    public class PortfolioManagementProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public PortfolioManagementProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.PortfolioManagement, userClaim, null, null)
        {
        }

        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public PortfolioManagementProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.PortfolioManagement, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Portfolio Management user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Portfolio Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            List<AdditionalParameters> additionalParameters;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList, out additionalParameters);
            }

            // Unassign User 
            return productLogic.UnassignUser();
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Portfolio Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.UpdateProductUserProfile();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Portfolio Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.ChangeProductUserType(rpList, batchProcessType);
        }
    }
    #endregion

    #region Deposit Alternative
    /// <summary>
    /// A 'Concrete implementation for Deposit Alternative
    /// </summary>
    public class DepositAlternativeProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public DepositAlternativeProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.DepositAlternative, userClaim, null, null)
        {
        }

        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public DepositAlternativeProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.DepositAlternative, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Deposit Alternative Product user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Deposit Alternative Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            List<AdditionalParameters> additionalParameters;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList, out additionalParameters);
            }

            // Unassign User 
            return productLogic.UnassignUser();
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Deposit Alternative Product Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.UpdateProductUserProfile();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Deposit Alternative Product Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.ChangeProductUserType(rpList, batchProcessType);
        }
    }
    #endregion

    #region Click Pay

    /// <summary>
    /// A 'Concrete implementation for Click Pay
    /// </summary>
    public class ClickPayProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public ClickPayProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.ClickPay, userClaim, null, null)
        {
        }

        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public ClickPayProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.ClickPay, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Click Pay user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Click Pay Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            List<AdditionalParameters> additionalParameters;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList, out additionalParameters);
            }

            // Unassign User 
            return productLogic.UnassignUser();
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Click Pay Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.UpdateProductUserProfile();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Click Pay Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.ChangeProductUserType(rpList, batchProcessType);
        }
    }
    #endregion

    #region EasyLMS
    /// <summary>
    /// A 'Concrete implementation for EasyLMS
    /// </summary>
    public class EasyLMSProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public EasyLMSProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.EasyLMS, userClaim, null, null)
        {
        }
        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public EasyLMSProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.EasyLMS, userClaim, productInternalSettingRepository, productRepository)
        {
        }
        /// <summary>
        /// Create EasyLMS user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">EasyLMS Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            List<AdditionalParameters> additionalParameters;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);
            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList, out additionalParameters);
            }
            // Unassign User 
            return productLogic.UnassignUser();
        }
        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);
            return productLogic.UpdateProductUserProfile();
        }
        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">EasyLMS Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);
            return productLogic.ChangeProductUserType(rpList, batchProcessType);
        }
    }
    #endregion

    #region Senior Lead Management
    /// <summary>
    /// A 'Concrete implementation for Deposit Alternative
    /// </summary>
    public class SeniorLeadManagementProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public SeniorLeadManagementProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.SeniorLeadManagement, userClaim, null, null)
        {
        }

        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public SeniorLeadManagementProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.SeniorLeadManagement, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create Senior Lead Management Product user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Senior Lead Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            List<AdditionalParameters> additionalParameters;
            //Try to cast as ProductUserRolePropertiesGroups
            var productUserRolePropertiesGroups = rolePropList as ProductUserRolePropertiesGroups;

            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, this.UserClaim);

            if (productUserRolePropertiesGroups == null)
            {
                //Try to cast as dictionary of RolePropertyList
                Dictionary<string, RolePropertyList> rolePropertyList = rolePropList as Dictionary<string, RolePropertyList>;
                RolePropertyList rolePropSLM;

                if (rolePropertyList != null)
                {
                    if (!rolePropertyList.Any())
                    {
                        return "Input JSON parsing issue; Null object.";
                    }
                    else
                    {
                        if (rolePropertyList.Any(p => p.Key == ProductEnum.SeniorLeadManagement.ToString()))
                        {
                            rolePropSLM = rolePropertyList.Where(p => p.Key == ProductEnum.SeniorLeadManagement.ToString()).First().Value;
                        }
                        else
                        {
                            return "Input JSON parsing issue; Null object.";
                        }

                        // Create-update user
                        if (rolePropSLM.IsAssigned)
                        {
                            //Map from RolePropertyList to ProductUserRolePropertiesGroups
                            productUserRolePropertiesGroups = MapPropertiesTorRolePropertiesGroups(rolePropSLM);

                            //Call new method and send new parameters
                            return productLogic.CreateUpdateProductUser(productUserRolePropertiesGroups, out additionalParameters);
                        }
                        else
                        {
                            return productLogic.UnassignUser();
                        }
                    }
                }
                else
                {
                    return "Input JSON parsing issue; Null object.";
                }
            }
            else
            {
                // Create-update user
                if (productUserRolePropertiesGroups.IsAssigned)
                {
                    return productLogic.CreateUpdateProductUser(productUserRolePropertiesGroups, out additionalParameters);
                }
            }

            // Unassign User 
            return productLogic.UnassignUser();
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.UpdateProductUserProfile();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Deposit Alternative Product Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.ChangeProductUserType(rpList, batchProcessType);
        }

        #region "Private Methods"

        private ProductUserRolePropertiesGroups MapPropertiesTorRolePropertiesGroups(RolePropertyList origin)
        {
            ProductUserRolePropertiesGroups result = new ProductUserRolePropertiesGroups();

            result.CanReceiveMonthlyReport = origin.CanReceiveMonthlyReport;
            result.IsAssigned = origin.IsAssigned;
            result.OrganizationRoleList = origin.OrganizationRoleList;
            result.PropertyGroupList = origin.PropertyGroupList;
            result.PropertyList = origin.PropertyList;
            result.PropertyRoleList = origin.PropertyRoleList;
            result.RoleList = origin.RoleList;
            result.RolePropertiesList = origin.RolePropertiesList;
            //result.RoleListString = ?

            return result;
        }

        #endregion

    }

    #endregion

    #region Renovation Manager
    /// <summary>
    /// A 'Concrete implementation for RenovationManagerProduct
    /// </summary>
    public class RenovationManagerProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public RenovationManagerProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.RenovationManager, userClaim, null, null)
        {
        }

        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public RenovationManagerProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.RenovationManager, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create RenovationManager Product user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Portfolio Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            List<AdditionalParameters> additionalParameters;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList, out additionalParameters);
            }

            // Unassign User 
            return productLogic.UnassignUser();
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Portfolio Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.UpdateProductUserProfile();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">Portfolio Management Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            return productLogic.ChangeProductUserType(rpList, batchProcessType);
        }
    }
    #endregion

    #region Intelligent Building
    /// <summary>
    /// A 'Concrete Product Intelligent Building' class
    /// </summary>
    public class IntelligentBuildingProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public IntelligentBuildingProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.UnifiedAmenities, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public IntelligentBuildingProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.UnifiedAmenities, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create UnifiedAmenities user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Unified Amenities Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as IBPropertyRole;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;

            var ib = new ManageIntelligentBuilding(base.UserClaim);

            // assign user
            if (rpList.IsAssigned)
            {
                return ib.ManageIntelligentBuildingUser(createUserPersonaId, assignUserPersonaId, rpList);
            }

            // Unassign User
            return ib.UnassignUser(createUserPersonaId, assignUserPersonaId, rpList);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Unified Amenities Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">>Unified Amenities Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            string changeProductUserTypeResponse = string.Empty;

            var rpList = rolePropList as IBPropertyRole;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Unified Amenities user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Unified Amenities user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var ib = new ManageIntelligentBuilding(base.UserClaim);

            changeProductUserTypeResponse = ib.ManageIntelligentBuildingUser(createUserPersonaId, assignUserPersonaId, rpList);
            return changeProductUserTypeResponse;
        }
    }
    #endregion

    #region UPFM Product Integration
    /// <summary>
    /// A 'Concrete Product Intelligent Building' class
    /// </summary>
    public class UPFMProductIntegration : ProductBase, IUPFMProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public UPFMProductIntegration(int productId, DefaultUserClaim userClaim) : base(productId, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public UPFMProductIntegration(int productId, DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base(productId, userClaim, productInternalSettingRepository, productRepository)
        {
        }

        /// <summary>
        /// Create UnifiedAmenities user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Unified Amenities Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList, out List<AdditionalParameters> additionalParameters)
        {
            additionalParameters = new List<AdditionalParameters>();
            var rpList = rolePropList as UPFMProductPropertyRole;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;

            var ib = new ManageUPFMProductsIntegration(_productId, base.UserClaim);

            // assign user
            if (rpList.IsAssigned)
            {
                return ib.ManageUPFMProductUser(createUserPersonaId, assignUserPersonaId, rpList, out additionalParameters, false);
            }

            // Unassign User
            return ib.UnassignUser(createUserPersonaId, assignUserPersonaId, rpList);
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">Unified Amenities Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">>Unified Amenities Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            string changeProductUserTypeResponse = string.Empty;

            var rpList = rolePropList as UPFMProductPropertyRole;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.PropertyList.Count == 0))
            {
                return "At least one Property is required in the Input JSON when changing a Unified Amenities user type from Admin to Regular.";
            }
            else if ((batchProcessType == BatchProcessType.UserTypeAdminToRegular) && (rpList.RoleList.Count == 0))
            {
                return "At least one Role is required in the Input JSON when changing a Unified Amenities user type from Admin to Regular.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                //Do Nothing
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var ib = new ManageUPFMProductsIntegration(_productId, base.UserClaim);

            changeProductUserTypeResponse = ib.ManageUPFMProductUser(createUserPersonaId, assignUserPersonaId, rpList, out additionalParameters, false);
            return changeProductUserTypeResponse;
        }
    }
    #endregion

    #region RealConnect
    /// <summary>
    /// A 'Concrete implementation for RealConnect
    /// </summary>
    public class RealConnectProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim"></param>
        public RealConnectProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.RealConnect, userClaim, null, null)
        {
        }
        /// <summary>
        /// Used by update process
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public RealConnectProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.RealConnect, userClaim, productInternalSettingRepository, productRepository)
        {
        }
        /// <summary>
        /// Create RealConnect user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">RealConnect Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            base.UserClaim.PersonaId = assignUserPersonaId;
            var rcProduct = new ManageProductRealConnect(base.UserClaim);
            
            // Create-update user
            if (rpList.IsAssigned)
            {
                return rcProduct.CreateUpdateUser(createUserRealPageId, createUserPersonaId, assignUserPersonaId, rolePropList);
            }
            // Unassign User 
            return rcProduct.UnassignUser(createUserPersonaId, assignUserPersonaId);
        }
        /// <summary>
        /// Update Product User Profile
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId)
        {
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            base.UserClaim.PersonaId = assignUserPersonaId;
            var rcProduct = new ManageProductRealConnect(base.UserClaim);
            return rcProduct.UpdateProductUserProfile(createUserPersonaId, assignUserPersonaId);
        }
        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">RealConnect Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as ProductUserRolePropertiesGroups;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            base.UserClaim.PersonaId = assignUserPersonaId;
            var rcProduct = new ManageProductRealConnect(base.UserClaim);
            return rcProduct.CreateUpdateUser(createUserRealPageId, createUserPersonaId, assignUserPersonaId, rolePropList);
        }
    }
    #endregion
}