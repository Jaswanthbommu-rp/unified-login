using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ClientPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntegrationMarketplace;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntelligentBuilding;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProspectContactCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RentersInsurance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResearchApplication;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.SelfProvisioningPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedAmenities;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.VendorServices;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;


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

        private readonly IIntegrationTypeFactory _integrationTypeFactory;

        #endregion

        #region Constructors
        /// <summary>
        /// Manages Product User constructor
        /// </summary>
        public ManageProductUser(IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository, ISamlRepository samlRepository)
        {
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _samlRepository = samlRepository;
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

            var manageProduct = new ManageProduct(_defaultUserClaim);
            var manageUnifiedLogin = new ManageUnifiedLogin(_defaultUserClaim);
            var manageProductOneSite = new ManageProductOneSite(_defaultUserClaim);
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, _defaultUserClaim);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Used to delete all SAML product information and status for a user
        /// </summary>
        /// <param name="productUserAccountDetails">product User Account Details</param>
        /// <returns>String.empty if success else error</returns>
        public string DeleteSamlUserProductInfoAndStatus(ProductUserAccountDetails productUserAccountDetails)
        {
            long assignUserPersonaId = productUserAccountDetails.PersonaId;

            var manageProductBase = new ManageProductBase((int)productUserAccountDetails.ProductName, _productInternalSettingRepository, _productRepository);

            manageProductBase.DeleteSamlUserProductInfoAndStatus(assignUserPersonaId, (int)productUserAccountDetails.ProductName);

            return string.Empty;
        }

        private string UpdateProductPrimaryPropertyProductStatus(long userPersonaId, int productId, int settingvalue)
        {
            var manageProductBase = new ManageProductBase(productId, _productInternalSettingRepository, _productRepository);
            manageProductBase.UpdateProductSettingProductStatus(userPersonaId, "UsePrimaryProperties", productId, settingvalue);
            return string.Empty;
		}

        /// <summary>
        /// Creates Product User
        /// </summary> 
        /// <param name="productUser">Product details for a user</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateProductUser(ProductUserProperitiesRoles productUser)
        {
            string result = string.Empty;
            int productId = 0;

            bool isUpdateUser = false;
            bool usePrimaryProperties = false;
            try
            {
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(productUser.AssignUserPersonaId, (int)productUser.ProductName);
                if (productAttributes.Any())
                {
                    isUpdateUser = true;
                }

                var roleProp = GetProductPropertiesRoles<RolePropertyList>(productUser.InputJson) as RolePropertyList;
                usePrimaryProperties = roleProp.UsePrimaryProperties;

                var integration = _integrationTypeFactory.GetIntegration(productUser.ProductName);
                result = integration.CreateUser(productUser);
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
                UpdateProductPrimaryPropertyProductStatus(productUser.AssignUserPersonaId, (int)productUser.ProductName, usePrimaryProperties == true ? 1 : 0);
            }
            else
            {
                if (result.ToUpper() == ProductBatchStatusType.Stop.ToString().ToUpper())
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Stop, null, "Batch Process stoped due to internal error for this product.");
                }
                else
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);

                    if (!isUpdateUser)
                    {
                        _productRepository.UpdateProductSettingProductStatus(productUser.AssignUserPersonaId, productId, "ProductStatus", (int)ProductBatchStatusType.Error);
                    }
                    else
                    {
                        //Activity log
                        result = "An error occurred during the update process";
                        WriteActivityLogWithMessage(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, result, productId);
                    }
                }
            }

            if (isBatchCompleted)
            {
                WriteActivityLog(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, productUser.BatchProcessorGroupId);
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
            string result = string.Empty;
            int productId = 0;

            bool isUpdateUser = false;
            bool usePrimaryProperties = false;
            bool isRolesExists = false;
            RolePropertyList roleProp = new RolePropertyList();
            try
            {
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(productUser.AssignUserPersonaId, (int)productUser.ProductName);
                if (productAttributes.Any())
                {
                    isUpdateUser = true;
                }

                //First get enterprise role id for user persona
                int userRoleTemplateId = _productRepository.GetUserEnterpriseRoleTemplateID(productUser.AssignUserPersonaId);

                if (userRoleTemplateId > 0)
                {
                    IManagePersona _managePersona = new ManagePersona();
                    var persona = _managePersona.GetPersona(productUser.AssignUserPersonaId);

                    var properties = getEnterpriseRoleUserPrimaryPropertiesData(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,(int)productUser.ProductName);
                 
                    if (properties?.Count > 0)
                    {
                        if (ValidateDictionaryMapping(productUser.InputJson) && (int)productUser.ProductName == (int)ProductEnum.OneSite)
                        {
                            object productPropertiesRoles = JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(productUser.InputJson.Trim());
                            var combinedRoleProp = new Dictionary<string, RolePropertyList>();
                            combinedRoleProp = productPropertiesRoles as Dictionary<string, RolePropertyList>;
                            if (combinedRoleProp.Any(p => p.Key == ProductEnum.OneSite.ToString()))
                            {
                                var osproperties = combinedRoleProp.Where(p => p.Key == ProductEnum.OneSite.ToString()).First().Value;
                                osproperties.PropertyList = properties;
                                isRolesExists = roleProp.RoleList?.Count > 0;
                            }
                            //Lead2Lease
                            if (combinedRoleProp.Any(p => p.Key == ProductEnum.Lead2Lease.ToString()))
                            {
                                // RolePropertyList lead2Lease
                                var l2lproperties = getEnterpriseRoleUserPrimaryPropertiesData(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, (int)ProductEnum.Lead2Lease);
                                var l2lroleProp = combinedRoleProp.Where(p => p.Key == ProductEnum.Lead2Lease.ToString()).First().Value;
                                l2lroleProp.PropertyList = l2lproperties;
                                isRolesExists = l2lroleProp.RoleList?.Count > 0;
                            }

                            //SeniorLeadManagement
                            if (combinedRoleProp.Any(p => p.Key == ProductEnum.SeniorLeadManagement.ToString()))
                            {
                                // RolePropertyList slm
                                var slmproperties = getEnterpriseRoleUserPrimaryPropertiesData(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, (int)ProductEnum.SeniorLeadManagement);
                                var slmroleProp = combinedRoleProp.Where(p => p.Key == ProductEnum.SeniorLeadManagement.ToString()).First().Value;
                                slmroleProp.PropertyList = slmproperties;
                                isRolesExists = slmroleProp.RoleList?.Count > 0;
                            }
                            //roleProp = combinedRoleProp;
                            productUser.InputJson = JsonConvert.SerializeObject(combinedRoleProp);
                        }
                        else
                        {
                            roleProp = JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson);
                            roleProp.PropertyList = properties;
                            isRolesExists = roleProp.RoleList?.Count > 0;
                            productUser.InputJson = JsonConvert.SerializeObject(roleProp);
                        }
                    }                    

                    usePrimaryProperties = true;
                    if (!isRolesExists)
                    {
                        result = "No Product Roles are found for Enterprise Role";
					}
                    else if (properties?.Count == 0 && !(productId == 63 || productId == 39))
                    {
                        result = "No Product Properties are found for Enterprise Role";
                    }
                    else
                    {
                        var integration = _integrationTypeFactory.GetIntegration(productUser.ProductName);
                        result = integration.CreateUser(productUser);
                    }
                    
                }
                else
                {
                    result = $"No Enterprise Role found for persona - {productUser.AssignUserPersonaId}";
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
                UpdateProductPrimaryPropertyProductStatus(productUser.AssignUserPersonaId, (int)productUser.ProductName, usePrimaryProperties == true ? 1 : 0);
            }
            else
            {
                if (result.ToUpper() == ProductBatchStatusType.Stop.ToString().ToUpper())
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Stop, null, "Batch Process stoped due to internal error for this product.");
                }
                else
                {
                    isBatchCompleted = _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);

                    if (!isUpdateUser)
                    {
                        _productRepository.UpdateProductSettingProductStatus(productUser.AssignUserPersonaId, productId, "ProductStatus", (int)ProductBatchStatusType.Error);
                    }
                    else
                    {
                        //Activity log
                        result = "An error occurred during the update process";
                        WriteActivityLogWithMessage(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, result, productId);
                    }
                }
            }

            if (isBatchCompleted)
            {
                WriteActivityLog(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, productUser.BatchProcessorGroupId);
            }

            return result;
        }

        /// <summary>
        /// Update product details for a user
        /// </summary> 
        /// <param name="productUserAccountDetails">Product User Account Details</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserAccountDetails(ProductUserAccountDetails productUserAccountDetails)
        {
            string result = string.Empty;
            IProduct product;

            switch (productUserAccountDetails.ProductName)
            {
                case ProductEnum.OneSite:
                    product = new OneSiteProduct(_defaultUserClaim, _productInternalSettingRepository,_productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.MarketingCenter:
                    product = new MarketingCenterProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.FinancialSuite:
                    product = new OneSiteAccountingProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.OpsBuyer:
                    product = new OpsProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.VendorServices:
                    product = new VendorServicesProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.ClientPortal:
                    product = new ClientPortalProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.SalesForce:
                    product = new ClientPortalProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.ProspectContactCenter:
                    product = new ProspectContactCenterProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.Lead2Lease:
                    product = new Lead2LeaseProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.ResidentPortal:
                    product = new ResidentPortalProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.OnSite:
                    product = new OnSiteProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.Insurance:
                    product = new RentersInsuranceProduct(_productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.ResearchApplication:
                    product = new ResearchApplicationProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.UnifiedAmenities:
                    product = new UnifiedAmenitiesProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.SelfProvisioningPortal:
                    product = new SelfProvisioningPortalProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.UtilityManagement:
                    product = new UtilityManagementProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.AssetOptimizer:
                    product = new AssetOptimizerProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.AoBusinessIntelligence:
                    product = new AoBusinessIntelligenceProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.LeadManagement:
                    product = new LeadManagementProduct(ProductEnum.LeadManagement);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.LeadAnalytics:
                    product = new LeadManagementProduct(ProductEnum.LeadAnalytics);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.RPDocumentManagement:
                    product = new RPDocumentManagementProduct(_defaultUserClaim, _productInternalSettingRepository, _productRepository);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.PortfolioManagement:
                    product = new PortfolioManagementProduct(ProductEnum.PortfolioManagement);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.IntegrationMarketplace:
                    product = new IntegrationMarketplaceProduct(_defaultUserClaim);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.DepositAlternative:
                    product = new DepositAlternativeProduct(ProductEnum.DepositAlternative);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.ClickPay:
                    product = new ClickPayProduct(ProductEnum.ClickPay);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.SeniorLeadManagement:
                    product = new SeniorLeadManagementProduct(_defaultUserClaim, ProductEnum.SeniorLeadManagement);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.RenovationManager:
                    product = new RenovationManagerProduct(ProductEnum.RenovationManager);
                    result = product.UpdateUserDetails(productUserAccountDetails);
                    break;
                case ProductEnum.IntelligentBuildingEnergy:
                case ProductEnum.IntelligentBuildingTrash:
                case ProductEnum.IntelligentBuildingWater:
                case ProductEnum.HandsOnTrainingSystem:
                case ProductEnum.LeaseLabs:
                case ProductEnum.HospitalityService:
                case ProductEnum.LeadScoring:
                    result = "User details Change not implemented for this Product.";
                    break;
                default:
                    result = "Product code does not exist.";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Update Product User Profile
        /// </summary>
        /// <param name="productUser">Product details for a user</param>
        /// <returns>String.empty if success else error</returns>
        public string UpdateProductUserProfile(ProductUserProperitiesRoles productUser)
        {
            string result = string.Empty;

            try
            {
                var integrationType = _integrationTypeFactory.GetIntegration(productUser.ProductName);
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
                _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Success);
            }
            else
            {
                _productRepository.UpdateProductBatch(productUser.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);
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

            try
            {
                var integration = _integrationTypeFactory.GetIntegration(batchRecord.ProductName);
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
                _productRepository.UpdateProductBatch(batchRecord.ProductBatchId, (int)ProductBatchStatusType.Success);               
            }
            else
            {
                if (result.ToUpper() == ProductBatchStatusType.Stop.ToString().ToUpper())
                {
                    _productRepository.UpdateProductBatch(batchRecord.ProductBatchId, (int)ProductBatchStatusType.Stop, null, "Batch Process stoped due to internal error for this product.");
                }
                else
                {
                    _productRepository.UpdateProductBatch(batchRecord.ProductBatchId, (int)ProductBatchStatusType.Error, null, result);
                    //Activity log
                    result = "An error occurred during the change user type process";
                    WriteActivityLogWithMessage(batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, result, batchRecord.ProductName);

                }
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

        private void WriteActivityLogWithMessage(long fromPersonaId, long toPersonaId, string message, int productId)
        {
            // log product user updated activity
            var fromUserLogDetail = GetUserActivityLogInfo(fromPersonaId);
            var toUserLogDetail = GetUserActivityLogInfo(toPersonaId);
            var booksProductDetail = _productRepository.GetBooksMasterProductDetail(productId);

            var logMessage = string.Format(message, toUserLogDetail.FirstName, toUserLogDetail.LastName,
                booksProductDetail.Name, fromUserLogDetail.FirstName, fromUserLogDetail.LastName);

            WriteActivityLog(fromUserLogDetail, toUserLogDetail,
               booksProductDetail.BooksProductCode, logMessage);
        }

        /// <summary>
        /// Get User info for activity logging
        /// </summary>
        private UserActivityLogInfo GetUserActivityLogInfo(long personaId)
        {
            IManagePersona _managePersona = new ManagePersona();
            IManagePerson _managePerson = new ManagePerson();
            IManageUserLogin _manageUserLogin = new ManageUserLogin();

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
                UserId = userLogin.UserId
            };
        }

        private void WriteActivityLog(UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo, string booksProductCode, string message)
        {
            // log product user updated activity
            try
            {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.PRODUCT_ACCESS,
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    BooksMasterOrganizationId = toUserLogInfo.BooksOrganizationMasterId,
                    OrganizationPartyId = toUserLogInfo.OrganizationPartyId,
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

                    BooksProductCode = booksProductCode
                });
            }
            catch (Exception ex)
            {
            }
        }

        private void WriteActivityLog(long fromPersonaId, long toPersonaId, int batchGroupId)
        {
            var fromUserLogInfo = GetUserActivityLogInfo(fromPersonaId);
            var toUserLogInfo = GetUserActivityLogInfo(toPersonaId);

            var data = _productRepository.GetUserBatchDetails(batchGroupId, fromPersonaId, toPersonaId, BatchProcessType.CreateUpdateProductUser);

            if (data != null & data.Count > 0) 
            {
                foreach (var item in data)
                {
                    var role = JsonConvert.DeserializeObject<UPFMProductPropertyRole>(item.InputJSON.Trim());
                    item.IsAssigned = role.IsAssigned;
                }

                bool activityLogged = data[0].BatchProcessorGroupActivityLogged;
                if (!activityLogged) 
                {
                    var successRecords = data.Where(x => x.StatusTypeId == 8).ToList();
                    if (successRecords != null)
                    {
                        var message = GenerateQueueMessage(fromUserLogInfo, toUserLogInfo, successRecords, true);
                        PushToQueue(fromUserLogInfo, toUserLogInfo, message);
                    }

                    var failedRecords = data.Where(x => x.StatusTypeId == 7).ToList();
                    if (failedRecords != null)
                    {
                        var message = GenerateQueueMessage(fromUserLogInfo, toUserLogInfo, failedRecords, false);
                        PushToQueue(fromUserLogInfo, toUserLogInfo, message);
                    }

                    //update status
                    _productRepository.UpdateBatchGroupStatus(batchGroupId, true);
                }
            }
        }

        private void PushToQueue(UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo, String message) 
        {
            try
            {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.PRODUCT_ACCESS,
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = Guid.NewGuid().ToString(),
                    BooksMasterOrganizationId = toUserLogInfo.BooksOrganizationMasterId,
                    OrganizationPartyId = toUserLogInfo.OrganizationPartyId,
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
                });
            }
            catch (Exception ex)
            {
            }
        }

        private string GenerateQueueMessage(UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo, List<UserBatchProductDetail> userBatchProductDetails, bool IsSuccess) 
        {
            string message = "";

            List<string> assinedProducts = new List<string>();
            List<string> unassignedProducts= new List<string>();

            string assignedMessage = "";
            string unassignedMessage = "";

            if (IsSuccess) 
            {
                message = $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated product access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName}:";
                foreach (var item in userBatchProductDetails)
                {
                    if (item.IsAssigned)
                        assinedProducts.Add(item.Name);

                    if (!item.IsAssigned)
                        unassignedProducts.Add(item.Name);
                }

                if (assinedProducts.Count > 0)
                    assignedMessage = " Access was granted to " + string.Join(", ", assinedProducts) + ".";


                if (unassignedProducts.Count > 0)
                    unassignedMessage = " Access was unassigned from " + string.Join(", ", unassignedProducts) + ".";

                message += assignedMessage;
                message += unassignedMessage;
            }

            else
            {
                message = $"An exception occured when {fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} attempted to update product access for {toUserLogInfo.FirstName} {toUserLogInfo.LastName} in ";
                string[] products = new string[userBatchProductDetails.Count];

                for (int i = 0; i < userBatchProductDetails.Count; i++)
                {
                    products[i] = userBatchProductDetails[i].Name;
                }

                var commaString = string.Join(", ", products);
                var lastComma = commaString.LastIndexOf(',');

                if (lastComma != -1)
                    commaString = commaString.Remove(lastComma, 1).Insert(lastComma, " and");

                message += commaString + ".";
            }

           

            return message;
        }

        #endregion
        /// <summary>
        /// Used to write to the log
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        /// <param name="exception"></param>
        public void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
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
            logger.Write(logType, exception, message );
        }

        private List<string> getEnterpriseRoleUserPrimaryPropertiesData(long editorPersonaId, long userPersonaId,int productId)
        {
            var productPropertyIdList = new List<string>();
            IManageProductPanel manageProductPanel = new ManageProductPanel(_defaultUserClaim);
            ListResponse result = new ListResponse();

            var userProperties = _propertyRepository.ListUPFMPropertyInstanceByPersona(userPersonaId, ProductEnum.UnifiedUI);
            result = manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null);
            if (!result.IsError)
            {
                UPFMProperty upfmProperty = new UPFMProperty();
                upfmProperty.id = userProperties?.Select(p => p.InstanceId.ToString()).ToList();

                result = manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, result);
                if (result.Records.Count > 0)
                {
                    productPropertyIdList = getSelectedProperties(result);
                }
            }
            return productPropertyIdList;
        }

        private List<string> getSelectedProperties(ListResponse productResult)
        {
            List<string> selectedProperties = new List<string>();
            var productPropertyType = productResult.Records[0].GetType();

            if (productPropertyType == typeof(ProductProperty))
            {
                var productList = productResult.Records.Cast<ProductProperty>();
                foreach (var property in productList)
                {
                    if (property.IsAssigned == true)
                    {
                        selectedProperties.Add(property.ID);
                    }
                }
            }
            else if (productPropertyType == typeof(ACProperty))
            {
                foreach (var property in productResult.Records.Cast<ACProperty>())
                {
                    if (property.IsAssigned == true)
                    {
                        selectedProperties.Add(property.Id);
                    }
                }
            }
            else if (productPropertyType == typeof(AssetGroup))
            {
                foreach (var property in productResult.Records.Cast<AssetGroup>())
                {
                    if (property.IsAssigned == true)
                    {
                        selectedProperties.Add(property.ID);
                    }
                }
            }
            else if (productPropertyType == typeof(OnSiteProperty))
            {
                foreach (var property in productResult.Records.Cast<OnSiteProperty>())
                {
                    if (property.IsAssigned == true)
                    {
                        selectedProperties.Add(property.GetPropertyId.ToString());
                    }
                }
            }
            else if (productPropertyType == typeof(RumPropertyGroup))
            {
                foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
                {
                    if (property.IsAssigned == true)
                    {
                        selectedProperties.Add(property.Id.ToString());
                    }
                }
            }
            else if (productPropertyType == typeof(ProductProperties))
            {
                foreach (var property in productResult.Records.Cast<ProductProperties>())
                {
                    if (property.IsAssigned == true)
                    {
                        selectedProperties.Add(property.GetPropertyId.ToString());
                    }
                }
            }
            else if (productPropertyType == typeof(Portfolio))
            {
                foreach (var property in productResult.Records.Cast<Portfolio>())
                {
                    if (property.IsAssigned == true)
                    {
                        selectedProperties.Add(property.ID);
                    }
                }
            }
            return selectedProperties;
        }
    }


    #region Interfaces
    /// <summary>
    /// The 'Product' abstract class
    /// </summary>
    interface IProduct
    {
        string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolepropList); 
        
        string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails);

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
        string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolepropList);
      
        string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails);

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

        /// <summary>
        /// ProductBase
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productInternalSettingRepository"></param>
        public ProductBase(int productId, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository)
        {
            _productId = productId;
            _productInternalSettingRepository = productInternalSettingRepository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// ProductBase
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        public ProductBase(int productId, DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository)
        {
            _productId = productId;
            _userClaim = userClaim;
            _productInternalSettingRepository = productInternalSettingRepository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Update product identifiers for a given user
        /// </summary>
        public string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails) //long assignUserPersonaId, ProductBatchStatusType productStatus, Dictionary<SamlAttributeEnum, string> settingList)
        {
            // Handle all other products than AO
            long assignUserPersonaId = productUserAccountDetails.PersonaId;
            _userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var manageProductBase = new ManageProductBase(_productId, _userClaim, _productInternalSettingRepository, _productRepository);
            
            // Update user Employee Id
            if (!string.IsNullOrEmpty(productUserAccountDetails.EmployeeId))
            {
                manageProductBase.UpdateUserEmployeeId(assignUserPersonaId, productUserAccountDetails.EmployeeId);
            }

            // Handle AO user products separately 
            if (_productId == (int)ProductEnum.AssetOptimizer)
            {
                return UpdateAoUserDetails(productUserAccountDetails);
            }

            manageProductBase.UpdateSamlUserAttributes(assignUserPersonaId, productUserAccountDetails.ProductSettings);
            manageProductBase.UpdateProductSettingProductStatus(assignUserPersonaId,
                ManageProductBase._productSettingType_ProductStatus, (int)productUserAccountDetails.ProductStatus);

            return string.Empty;
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
                var productSeniorLeadManagement = new SeniorLeadManagementProduct(base.UserClaim, ProductEnum.SeniorLeadManagement);

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
            Dictionary<string, object> logData = new Dictionary<string, object>();
            logData.Add("rolePropList", rolePropList);

            os.WriteToDiagnosticLog("OneSite.ChangeProductUserType", logData);
            // Unassign User
            bool deleteSamlUserProductInfoAndStatus = true;
            changeProductUserTypeResponse = os.UnassignUser(createUserPersonaId, assignUserPersonaId, deleteSamlUserProductInfoAndStatus);
            if (string.IsNullOrWhiteSpace(changeProductUserTypeResponse))
            {
                changeProductUserTypeResponse = os.ManageOneSiteUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList, false);
            }

            var lead2leaseresult = "";
            if (combinedRoleProp.Any(p => p.Key == ProductEnum.Lead2Lease.ToString()))
            {
                os.WriteToDiagnosticLog("OneSite.ChangeProductUserType.Adding Lead2Lease");
                rpList = combinedRoleProp.Where(p => p.Key == ProductEnum.Lead2Lease.ToString()).First().Value;
                var productLead2Lease = new ManageProductLead2Lease(base.UserClaim);
                productLead2Lease.WriteToDiagnosticLog("OneSite.ChangeProductUserType.UnassignUser");
                // Unassign User
                lead2leaseresult = productLead2Lease.UnassignUser(createUserPersonaId, assignUserPersonaId);
                if (string.IsNullOrEmpty(lead2leaseresult))
                {
                    productLead2Lease.WriteToDiagnosticLog("OneSite.ChangeProductUserType.ReassignUser");
                    // assign user
                    lead2leaseresult = productLead2Lease.ManageLead2LeaseUser(createUserPersonaId, assignUserPersonaId, rpList.RoleList, rpList.PropertyList);
                }

                if (!string.IsNullOrEmpty(lead2leaseresult))
                {
                    changeProductUserTypeResponse += lead2leaseresult;
                }
                productLead2Lease.WriteToDiagnosticLog("OneSite.ChangeProductUserType.Lead2Lease result:" + lead2leaseresult);
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
        public RentersInsuranceProduct(IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.Insurance, productInternalSettingRepository, productRepository)
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

    #region Unified Amenities
    /// <summary>
    /// A 'Concrete Product UnifiedAmenities' class
    /// </summary>
    public class UnifiedAmenitiesProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public UnifiedAmenitiesProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.UnifiedAmenities, userClaim, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="userClaim">User claim related information</param>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public UnifiedAmenitiesProduct(DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.UnifiedAmenities, userClaim, productInternalSettingRepository, productRepository)
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
            var rpList = rolePropList as UnifiedAmenitiesPropertyRole;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }
            base.UserClaim.UserRealPageGuid = createUserRealPageId;

            var ua = new ManageUnifiedAmenities(base.UserClaim);

            // assign user
            if (rpList.IsAssigned)
            {
                return ua.ManageUnifiedAmenitiesUser(createUserPersonaId, assignUserPersonaId, rpList);
            }

            // Unassign User
            return ua.UnassignUser(createUserPersonaId, assignUserPersonaId, rpList);
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

            var rpList = rolePropList as UnifiedAmenitiesPropertyRole;

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
            var ua = new ManageUnifiedAmenities(base.UserClaim);

            changeProductUserTypeResponse = ua.ManageUnifiedAmenitiesUser(createUserPersonaId, assignUserPersonaId, rpList);
            return changeProductUserTypeResponse;
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
        public LeadManagementProduct(ProductEnum productType) : base((int)productType, null, null)
        {
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        /// <param name="productInternalSettingRepository">Internal settings for a product</param>
        public LeadManagementProduct(IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository) : base((int)ProductEnum.LeadManagement, productInternalSettingRepository, productRepository)
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
                return productLogic.CreateUpdateProductUser(rpList);
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
        /// <param name="productType">Identify products by id</param>
        public PortfolioManagementProduct(ProductEnum productType) : base((int)productType, null, null)
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
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList);
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

    #region Integration Marketplace

    /// <summary>
    /// A 'Concrete IntegrationMarketplaceProduct' class
    /// </summary>
    public class IntegrationMarketplaceProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        public IntegrationMarketplaceProduct(DefaultUserClaim userClaim) : base((int)ProductEnum.IntegrationMarketplace, userClaim, null, null)
        {
        }

        /// <summary>
        /// Create IntegrationMarketplaceProduct user
        /// </summary> 
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rolePropList">IntegrationMarketplaceProduct Role</param>
        /// <returns>String.empty if success else error</returns>
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            var rpList = rolePropList as IntegrationMarketplacePropertyRole;

            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var integrationMarketplaceLogic = new ManageProductIntegrationMarketplace(base.UserClaim);

            // assign user
            if (rpList.IsAssigned)
            {
                return integrationMarketplaceLogic.ManageIntegrationMarketplaceUser(createUserPersonaId, assignUserPersonaId, rpList);
            }

            // Unassign User
            return integrationMarketplaceLogic.UnassignUser(createUserPersonaId, assignUserPersonaId, rpList);
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
            // never comes here
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Product User Type from Admin to Regular or Regular to Admin
        /// </summary>
        /// <param name="createUserRealPageId">Logged-in user Enterprise UserId</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="batchProcessType">Batch Process Type</param>
        /// <param name="rolePropList">>Integration Marketplace Role And Property List</param>
        /// <returns>String.empty if success else error</returns>
        public string ChangeProductUserType(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, BatchProcessType batchProcessType, object rolePropList)
        {
            var rpList = rolePropList as IntegrationMarketplacePropertyRole;
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            base.UserClaim.UserRealPageGuid = createUserRealPageId;
            var productIntegrationMarketplaceLogic = new ManageProductIntegrationMarketplace(base.UserClaim);

            // Change user type (Update User)
            return productIntegrationMarketplaceLogic.ChangeIntegrationMarketplaceUserType(createUserPersonaId, assignUserPersonaId, rpList, batchProcessType);
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
        /// <param name="productType">Identify products by id</param>
        public DepositAlternativeProduct(ProductEnum productType) : base((int)productType, null, null)
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
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList);
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
        /// <param name="productType">Identify products by id</param>
        public ClickPayProduct(ProductEnum productType) : base((int)productType, null, null)
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
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList);
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

    #region Senior Lead Management
    /// <summary>
    /// A 'Concrete implementation for Deposit Alternative
    /// </summary>
    public class SeniorLeadManagementProduct : ProductBase, IProduct
    {
        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="userClaim">Use to hold user claim related information</param>
        /// <param name="productType">Product Type</param>
        public SeniorLeadManagementProduct(DefaultUserClaim userClaim, ProductEnum productType) : base((int)productType, userClaim, null, null)
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
                            return productLogic.CreateUpdateProductUser(productUserRolePropertiesGroups);
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
                    return productLogic.CreateUpdateProductUser(productUserRolePropertiesGroups);
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
        /// <param name="productType">Identify products by id</param>
        public RenovationManagerProduct(ProductEnum productType) : base((int)productType, null, null)
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
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var userClaims = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            var productLogic = ManageProductFactory.GetProductLogic(_productId, createUserPersonaId, assignUserPersonaId, userClaims);

            // Create-update user
            if (rpList.IsAssigned)
            {
                return productLogic.CreateUpdateProductUser(rpList);
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
        public string CreateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
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
                return ib.ManageUPFMProductUser(createUserPersonaId, assignUserPersonaId, rpList);
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

            changeProductUserTypeResponse = ib.ManageUPFMProductUser(createUserPersonaId, assignUserPersonaId, rpList);
            return changeProductUserTypeResponse;
        }
    }
    #endregion
}