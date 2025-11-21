using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using UnifiedLogin.BusinessLogic.Logic.Helper;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// ManageOrganizationProduct class
    /// </summary>
    public class ManageOrganizationProduct : IManageOrganizationProduct
    {
        #region Private Variables
        IOrganizationProductRepository _organizationProductRepository;
        IManageBlueBook _manageBlueBook;
        IManageProduct _manageProduct;
        IProductRepository _productRepository; 
        private IProductInternalSettingRepository _productInternalSettingRepository ;

        private DefaultUserClaim _defaultUserClaim;
        #endregion

        #region Constructors
        /// <summary>
        /// Manage Organization Product Constructor (Default)
        /// </summary>
        /// <param name="userClaim"></param>
        public ManageOrganizationProduct(DefaultUserClaim userClaim)
        {
            _organizationProductRepository = new OrganizationProductRepository();
            _defaultUserClaim = userClaim;
            _manageBlueBook = new ManageBlueBook(userClaim);
            _manageProduct = new ManageProduct(userClaim);
            _productRepository = new ProductRepository(userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }

        /// <summary>
        /// Create a basic instance of the ManageOrganizationProduct class
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="repository"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="manageProduct"></param>

        public ManageOrganizationProduct(DefaultUserClaim userClaim, IRepository repository, IManageBlueBook manageBlueBook, IManageProduct manageProduct)
        {
            _organizationProductRepository = new OrganizationProductRepository(repository);
            _manageBlueBook = manageBlueBook;
            _manageProduct = manageProduct;
            _productRepository = new ProductRepository(repository, userClaim);
            _defaultUserClaim = userClaim;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Used to add a list of products to the given company
        /// </summary>
        /// <param name="org"></param>
        /// <param name="productList"></param>
        /// <returns></returns>
        public IRepositoryResponse InsertUpdateOrganizationProduct(Organization org, List<int> productList)
        {
            RepositoryResponse response = new RepositoryResponse();
            List<KeyValuePair<int, RepositoryResponse>> responseList = new List<KeyValuePair<int, RepositoryResponse>>();
            foreach (int product in productList.Distinct())
            {
                var productInternalSettings = _manageProduct.GetProductInternalSettings((int)product);
                var updateinUDM = productInternalSettings.Where(x => x.Name.ToUpper() == "UPDATEPRODUCTINUDM").FirstOrDefault();

                if (updateinUDM != null && updateinUDM.Value == "1")
                {
                    var systemProductCenter = new SystemProductCenter()
                    {
                        Id = 0,
                        CompanyInstanceSourceId = org.RealPageId.ToString().ToLower(),
                        CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                        ProductCenterSourceId = product.ToString(),
                        PropertyInstanceSourceId = null,
                        Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
                    };
                    var isUpdated = _manageBlueBook.ProductCenterEnable(systemProductCenter);

                    if (!isUpdated)
                    {
                        response.ErrorMessage = "Unable to update product in UDM";
                        return response;
                    }
                }

                response = InsertUpdateOrganizationProduct(partyId: org.PartyId, product: product, configurationId: null, fromDate: null, thruDate: null, orgName: org.Name);
                responseList.Add(new KeyValuePair<int, RepositoryResponse>(product, response));
            }

            if (responseList.Count > 0)
            {
                List<string> enabledProducts = new List<string>();
                List<string> failedProducts = new List<string>();
                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                var products = _productRepository.GetAllProducts();

                foreach (var p in responseList)
                {
                    if (string.IsNullOrEmpty(p.Value.ErrorMessage))
                    {
                        enabledProducts.Add(products.First(po => po.ProductId == p.Key).Name);
                    }
                    else
                    {
                        failedProducts.Add(products.First(po => po.ProductId == p.Key).Name);
                    }
                }

                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} changed products for {org.Name}";
                if (enabledProducts.Count > 0)
                {
                    additionalParameters.Add(new AdditionalParameters() { Key = "EnabledProducts", Value = $"{{ \"old\": \"{""}\", \"new\": \"{string.Join(", ", enabledProducts)}\" }}" });
                }
                if (failedProducts.Count > 0)
                {
                    additionalParameters.Add(new AdditionalParameters() { Key = "FailedProducts", Value = $"{{ \"old\": \"{""}\", \"new\": \"{string.Join(", ", failedProducts)}\" }}" });
                }

                LogAuditActivity(LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message, additionalParameters);
            }

            return response;
        }


        public IRepositoryResponse CheckSharedProductsEnabled(IList<ProductUI> orgEnabledproductList, List<int> addProductList , List<int> removeProductList)
        {           
            RepositoryResponse response = new RepositoryResponse();
            List<string> errorProductList = new List<string>();
            var sharedProductList = _productInternalSettingRepository.GetProductSettingByType("PreventEnablingThisProductID").ToList();
            string errorProduct = string.Empty;

            foreach (var productId in addProductList)
            {
                var productIdToCheck = sharedProductList.FirstOrDefault(m => m.ProductId == productId);
                if (productIdToCheck != null && orgEnabledproductList.Any( m => m.ProductId == Convert.ToInt32(productIdToCheck.Value))
                    && !removeProductList.Any(m => m == Convert.ToInt32(productIdToCheck.Value)))
                {
                    errorProduct = sharedProductList.FirstOrDefault( m => m.ProductId == Convert.ToInt32(productIdToCheck.Value)).ProductName;
                    errorProductList.Add(errorProduct);
                }
            }
            if (errorProductList.Any())
            {
                response.ErrorMessage = "Unable to enable products : " + string.Join(",", errorProductList);
            }

            return response;
        }

        /// <summary>
        /// Used to insert a new product to an Organization
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="product"></param>
        /// <param name="configurationId"></param>
        /// <param name="fromDate"></param>
        /// <param name="thruDate"></param>
        /// <param name="orgName"></param>
        /// <returns></returns>
        public RepositoryResponse InsertUpdateOrganizationProduct(long partyId, int product, int? configurationId, DateTime? fromDate, DateTime? thruDate, string orgName)
        {
            var response = _organizationProductRepository.InsertUpdateOrganizationProduct(partyId, product, configurationId, fromDate, thruDate);
            return response;
        }

        /// <summary>
        /// Used to insert a new product to an Organization from provisioning
        /// </summary>
        /// <param name="product"></param>
        /// <param name="configurationId"></param>
        /// <param name="fromDate"></param>
        /// <param name="thruDate"></param>
        /// <param name="org"></param>
        /// <returns></returns>
        public IRepositoryResponse InsertUpdateOrganizationProductFromProvisioning(int product, int? configurationId, DateTime? fromDate, DateTime? thruDate, Organization org)
        {
            var response = _organizationProductRepository.InsertUpdateOrganizationProduct(org.PartyId, product, configurationId, fromDate, thruDate);

            if (response.ErrorMessage.Length == 0)
            {
                var products = _productRepository.GetAllProducts();
                var productName = products.FirstOrDefault(p => p.ProductId == (int)product)?.Name;
                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} enabled {productName} for {org.Name}";
                LogAuditActivity(LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message);
            }

            return response;
        }

        /// <summary>
        /// Used to delete a product from an Organization
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="product"></param>
        /// <param name="org"></param>
        /// <returns></returns>
        public RepositoryResponse DeleteOrganizationProduct(long partyId, int product, Organization org, bool logActivity = true)
        {
            var response = _organizationProductRepository.DeleteOrganizationProduct(partyId, product);

            if (response.ErrorMessage.Length == 0 && logActivity)
            {
                var products = _productRepository.GetAllProducts();
                var productName = products.FirstOrDefault(p => p.ProductId == (int)product)?.Name;
                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} disabled {productName} for {org.Name}";
                LogAuditActivity(LogActivityTypeConstants.PRODUCT_DISABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message);
            }

            return response;
        }

        /// <summary>
        /// Used to delete products from an organization
        /// </summary>
        /// <param name="unassignProductList"></param>
        /// <param name="org"></param>
        public RepositoryResponse DeleteProductsFromOrganization(List<int> unassignProductList, Organization org)
        {
            var response = new RepositoryResponse();
            List<KeyValuePair<int, RepositoryResponse>> responseList = new List<KeyValuePair<int, RepositoryResponse>>();
            foreach (var product in unassignProductList)
            {
                var productInternalSettings = _manageProduct.GetProductInternalSettings(product);
                var updateinUDM = productInternalSettings.Find(x => x.Name.ToUpper() == "UPDATEPRODUCTINUDM");

                if (updateinUDM != null && updateinUDM.Value == "1")
                {
                    var systemProductCenter = new SystemProductCenter()
                    {
                        Id = org.PartyId,
                        CompanyInstanceSourceId = org.RealPageId.ToString().ToLower(),
                        CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
                        ProductCenterSourceId = product.ToString(),
                        PropertyInstanceSourceId = null,
                        Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
                    };
                    var isUpdated = _manageBlueBook.ProductCenterDisable(systemProductCenter);
                    if (!isUpdated)
                    {
                        response.ErrorMessage = "Unable to delete product in UDM";
                        return response;
                    }
                }

                response = DeleteOrganizationProduct(partyId: org.PartyId, product: product, org: org, logActivity: false);
                responseList.Add(new KeyValuePair<int, RepositoryResponse>(product, response));
            }
            if (responseList.Count > 0)
            {
                List<string> deletedProducts = new List<string>();
                List<string> failedProducts = new List<string>();
                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                var products = _manageProduct.ListProducts();

                foreach (var p in responseList)
                {
                    if (string.IsNullOrEmpty(p.Value.ErrorMessage))
                    {
                        deletedProducts.Add(products.First(po => po.ProductId == p.Key).Name);
                    }
                    else
                    {
                        failedProducts.Add(products.First(po => po.ProductId == p.Key).Name);
                    }
                }

                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} Deleted products for {org.Name}";
                if (deletedProducts.Count > 0)
                {
                    additionalParameters.Add(new AdditionalParameters() { Key = "DeletedProducts", Value = $"{{ \"old\": \"{""}\", \"new\": \"{string.Join(", ", deletedProducts)}\" }}" });
                }
                if (failedProducts.Count > 0)
                {
                    additionalParameters.Add(new AdditionalParameters() { Key = "FailedProducts", Value = $"{{ \"old\": \"{""}\", \"new\": \"{string.Join(", ", failedProducts)}\" }}" });
                }

                LogAuditActivity(LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message, additionalParameters);
            }

            return response;
        }

        /// <summary>
        /// Used to delete users for product for an Organization
        /// </summary>
        /// <param name="partyId">The organization id for the product</param>
        /// <param name="product">The product Id</param>
        /// <returns></returns>
        public IRepositoryResponse DisableUsersForProduct(long partyId, ProductEnum product)
        {
            return _organizationProductRepository.DisableUsersForProduct(partyId, product);
        }

        private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType, string message, List<AdditionalParameters> additionalParameters = null)
        {
            try
            {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = logActivityType,
                    LogCategoryName = logActivityCategoryType.ToString(),
                    CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _defaultUserClaim.OrganizationMasterId,
                    OrganizationPartyId = _defaultUserClaim.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = _defaultUserClaim.LoginName,
                    FromUserLoginId = _defaultUserClaim.UserId,
                    FromUserRealpageId = _defaultUserClaim.UserRealPageGuid.ToString(),
                    FromUserFirstName = _defaultUserClaim.FirstName,
                    FromUserLastName = _defaultUserClaim.LastName,

                    ToUserLoginName = null,
                    ToUserLoginId = null,
                    ToUserFirstName = null,
                    ToUserLastName = null,
                    ToUserRealpageId = null,
                    AdditionalInformation = additionalParameters
                });
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", exception: ex, messageProperties: new object[] { "LogAuditActivity", $"Error while adding activity message. BooksMasterOrganizationId{_defaultUserClaim.OrganizationName}, author user login name {_defaultUserClaim.LoginName}" });
            }
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            try
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
            catch
            {
                /*ignored*/
            }
        }
        #endregion
    }
}