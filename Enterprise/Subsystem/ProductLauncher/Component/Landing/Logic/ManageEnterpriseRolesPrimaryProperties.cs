using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using ProductRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductRole;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageEnterpriseRolesPrimaryProperties
    {
        private readonly DefaultUserClaim _userClaim;
        private IProductRepository _productRepository;
        private readonly IntegrationTypeFactory _integrationTypeFactory;
        private IPropertyRepository _propertyRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IUnifiedSettingsRepository _unifiedSettingsRepository;
        private IManagePersona _managePersona;
        private ManageProductBatch _manageProductBatch;
        private IPersonaRepository _personaRepository;
        private IUserLoginRepository _userLoginRepository;
        private BatchProductBulkUpdateRepository _enterpriseRoleProductRepository;
        private IUserRoleRightRepository _userRoleRightRepository;
        private IManageBlueBook _manageBlueBook;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageEnterpriseRolesPrimaryProperties(DefaultUserClaim userClaim)
        {
            _userClaim = userClaim;
            _manageProductBatch = new ManageProductBatch(_userClaim);
            var manageProduct = new ManageProduct(_userClaim);
            var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
            var manageProductOneSite = new ManageProductOneSite(_userClaim);
            var productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _propertyRepository = new PropertyRepository();
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _unifiedSettingsRepository = new UnifiedSettingsRepository();
            _managePersona = new ManagePersona(_userClaim);
            _personaRepository = new PersonaRepository(_userClaim);
            _personaRepository = new PersonaRepository(_userClaim);
            _userLoginRepository = new UserLoginRepository();
            _enterpriseRoleProductRepository = new BatchProductBulkUpdateRepository(_userClaim);
            _userRoleRightRepository = new UserRoleRightRepository();
        }


        public ManageEnterpriseRolesPrimaryProperties(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims,
            IOneSiteProductService oneSiteProductService = null, IManageBlueBook manageBlueBook = null)
        {
            _userClaim = userClaims;
            _manageProductBatch = new ManageProductBatch(repository,  messageHandler, userClaims, oneSiteProductService);
            var manageProduct = new ManageProduct(repository, userClaims, messageHandler);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
            _manageBlueBook = new ManageBlueBook(userClaims, repository, _productInternalSettingRepository, messageHandler);
            var manageProductOneSite = new ManageProductOneSite(_userClaim, oneSiteProductService, _manageBlueBook, _productInternalSettingRepository, messageHandler, repository);
            _productRepository = new ProductRepository(repository);
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, _productInternalSettingRepository, _userClaim);
            _unifiedSettingsRepository = new UnifiedSettingsRepository(repository);
            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
            _personaRepository = new PersonaRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _enterpriseRoleProductRepository = new BatchProductBulkUpdateRepository(repository, _userClaim);
        }

        public string ProcessEnterpriseRolesAndPrimaryPropertiesData(long editorUserPersonaId, long subjectUserPersonaId, int? enterpriseRoleTemplateId = null, DateTime? createdDateTime = null, int batchProcessTypeId = 0, bool isUnassignAllProducts = false)
        {
            string batchProcessorType = enterpriseRoleTemplateId != null ? "Enterprise Role" : "Primary Properties";
            try
            {
                Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"{batchProcessorType} process started to user - {subjectUserPersonaId} : enterpriseRoleTemplateId - {enterpriseRoleTemplateId} - createddate is {createdDateTime}");
                IList<ProductBatch> productListToCreate = new List<ProductBatch>();
                var editorPersona = _managePersona.GetPersona(editorUserPersonaId);
                var userPersona = _managePersona.GetPersona(subjectUserPersonaId);
                
                List<int> roleTemplateNewProducts = new List<int>();
                List<int> roleTemplateUpdatedProducts = new List<int>();
                List<int> roleTemplateDeletedProducts = new List<int>();
                List<RoleTemplateProductRole> roleTemplateProductRole = new List<RoleTemplateProductRole>();
                IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(userPersona.RealPageId, null);
                var personaOrganization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);
                var personaProductSettings = _personaRepository.GetPersonaProductSettings(subjectUserPersonaId);
                string newproducts = string.Empty;
                string updateproducts = string.Empty;
                string deletedProducts = string.Empty;
                if (batchProcessTypeId == (int)BatchProcessType.BulkAddUpdateEnterpriseRole)
                {
                    if (isUnassignAllProducts)
                    {
                        var personaProducts = _productRepository.ListProductsByPersonaId(userPersona.PersonaId, (Int32)UserUiStatusType.AccountCreationSuccessful).ToList();
                        personaProducts.RemoveAll(m => m.ProductId == (int)ProductEnum.UnifiedPlatform);
                        personaProducts.RemoveAll(m => m.ProductId == (int)ProductEnum.AssetOptimizer);
                        int adminSupportProductId = (int)ProductEnum.AdminSupportPortal;
                        if (personaProducts != null && personaProducts.Any(m => m.ProductId == adminSupportProductId))
                        {
                            var productAttributes = _productRepository.GetProductSamlDetails(subjectUserPersonaId, adminSupportProductId);
                            if (productAttributes != null && productAttributes.Count == 0)
                            {
                                personaProducts.RemoveAll(a => a.ProductId == adminSupportProductId);
                            }
                        }
                        roleTemplateDeletedProducts.AddRange(personaProducts.Select(p => p.ProductId).ToList());
                        if (roleTemplateDeletedProducts != null && roleTemplateDeletedProducts.Count == 0)
                        {
                            return "";
                        }
                    }
                    else
                    {
                        roleTemplateProductRole = _productRepository.GetRoleTemplateProductRoleMapping(enterpriseRoleTemplateId.Value, editorPersona.OrganizationPartyId);
                        roleTemplateNewProducts = roleTemplateProductRole.Select(p => p.ProductId).Distinct().ToList();
                        // Adding UPFM object to roleTemplateUpdatedProducts , It will delete existing UPFM roles and updating to UPFM roles. 
                        roleTemplateUpdatedProducts.Add(roleTemplateNewProducts.FirstOrDefault(m => m == (int)ProductEnum.UnifiedPlatform));
                       
                    }
                }
                else if (enterpriseRoleTemplateId != null)
                {
                    roleTemplateNewProducts = _productRepository.GetEnterpriseRoleNewProductsByRoleTemplateId(enterpriseRoleTemplateId.Value, createdDateTime.Value);
                    roleTemplateUpdatedProducts = _productRepository.GetEnterpriseRoleUpdatedProductsByRoleTemplateId(enterpriseRoleTemplateId.Value, createdDateTime.Value);
                    roleTemplateDeletedProducts = _productRepository.GetEnterpriseRoleDeletedProductsByRoleTemplateId(enterpriseRoleTemplateId.Value, createdDateTime.Value);
                    roleTemplateProductRole = _productRepository.GetRoleTemplateProductRoleMapping(enterpriseRoleTemplateId.Value, editorPersona.OrganizationPartyId);
                    roleTemplateNewProducts.AddRange(roleTemplateUpdatedProducts);

                    // Kept this for only for logs, Will remove this logic once testing is done,
                    // Start
                    updateproducts = roleTemplateUpdatedProducts != null && roleTemplateUpdatedProducts.Count > 0 ? string.Join(",", roleTemplateUpdatedProducts) : "no updated products";
                    Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"update products : {updateproducts}");
                    deletedProducts = roleTemplateDeletedProducts != null && roleTemplateDeletedProducts.Count > 0 ? string.Join(",", roleTemplateDeletedProducts) : "no deleted products";
                    Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"deleted products : {deletedProducts}");
                    // End.
                }
                else
                {
                    var personaProducts = _productRepository.ListProductsByPersonaId(userPersona.PersonaId, (Int32)UserUiStatusType.AccountCreationSuccessful).ToList();
                    //Get User enterprise role for the user, Get list of products for the user
                    //Make distinct of products and assign to personaProducts object
                    var userEnterpriseRole = _productRepository.GetEnterpriseRoleForPersona(subjectUserPersonaId);
                    if (userEnterpriseRole != null)
                    {
                        roleTemplateProductRole = _productRepository.GetRoleTemplateProductRoleMapping(userEnterpriseRole.RoleTemplateId, userPersona.OrganizationPartyId);
                        foreach (var product in roleTemplateProductRole)
                        {
                            if (!personaProducts.Any(p => p.ProductId == product.ProductId) && product.ProductId != (int)ProductEnum.UnifiedPlatform)
                            {
                                personaProducts.Add(new PersonaProductUserDetails()
                                {
                                    ProductId = product.ProductId,
                                    ProductName = product.ProductName
                                });
                            }
                        }
                    }
                    roleTemplateNewProducts = personaProducts.Select(p => p.ProductId).ToList();
                    Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: "In Primary properties block");
                }

                roleTemplateNewProducts = roleTemplateNewProducts.Distinct().ToList();
                var productsWithNoProperties = GetProductsWithNoProperties();
                // Kept this for only for logs, Will remove this logic once testing is done,
                // Start
                newproducts = roleTemplateNewProducts != null && roleTemplateNewProducts.Count > 0 ? string.Join(",", roleTemplateNewProducts) : "no new products";
                Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"New products : {newproducts}");
                // End.

                bool isExternalUser = personaOrganization.RelationshipType != null && personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);
                IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
                if (_userClaim.ImpersonatedBy != Guid.Empty)
                {
                    impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
                }

                Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"{batchProcessorType} started to user - {subjectUserPersonaId}");
                IList<ProductRole> productRoles = null;
                ListResponse propertiesResponse = null;
                ListResponse rolesResponse = null;
                bool personaProductUsePrimaryProperty = false;
                bool usePrimaryProperties = false;
                string inputAOJSON = string.Empty;
                // To get the Primary property setting for Asset Optimization if it is under Asset Optimization product family.
                int productIdForCompanyAndProductSetting = 0;

                foreach (var product in roleTemplateNewProducts)
                {
                    propertiesResponse = new ListResponse();
                    rolesResponse = new ListResponse();
                    personaProductUsePrimaryProperty = false;
                    usePrimaryProperties = false;
                    productIdForCompanyAndProductSetting = ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product) ? (int)(ProductEnum.AssetOptimizer) : product;
                    var ppEnabledForCompanyAndProduct = GetPrimaryPropertySettingsForCompanyAndProduct(productIdForCompanyAndProductSetting);
                    bool productEnabledForPrimaryProperty = _manageProductBatch.IsProductEnabledForUsePrimaryProperty(product);
                    var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == product);

                    if (productSetting != null)
                    {
                        personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ;
                    }
                    usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty && ppEnabledForCompanyAndProduct;
                    
                    usePrimaryProperties = (product == (int)ProductEnum.UnifiedPlatform) ? true : usePrimaryProperties;
                    
                    var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product);

                    if (enterpriseRoleTemplateId != null || (roleTemplateProductRole != null && roleTemplateProductRole.Any(m => m.ProductId == product)))
                    {
                        rolesResponse = _manageProductBatch.GetProductRoles(editorPersona.PersonaId, 0, product, userPersona.OrganizationPartyId, _userClaim);
                        productRoles = GetProductRoleList(roleTemplateProductRole, product);
                        if (productRoles != null && productRoles.Any() && rolesResponse.Records != null && rolesResponse.Records.Any())
                        {
                            var roleType = rolesResponse.Records[0].GetType();
                            if (roleType == typeof(ProductRole))
                            {
                                IList<ProductRole> allproductRolesFromProducts = rolesResponse.Records?.Cast<ProductRole>().ToList();
                                productRoles.ToList().ForEach(m =>
                                {
                                    if (!allproductRolesFromProducts.Any(l => l.ID.ToString() == m.ID))
                                    {
                                        productRoles.Remove(m);
                                    }
                                });
                            }
                        }
                        rolesResponse = new ListResponse()
                        {
                            Records = productRoles.Cast<object>().ToList(),
                            TotalRows = productRoles.Count,
                            RowsPerPage = productRoles.Count,
                            TotalPages = 1,
                            ErrorReason = ""
                        };
                    }
                    else
                    {
                        rolesResponse = _manageProductBatch.GetProductRoles(editorPersona.PersonaId, userPersona.PersonaId, product, userPersona.OrganizationPartyId, _userClaim);
                        if (rolesResponse.Records.Count > 0)
                        {
                            var roleType = rolesResponse.Records[0].GetType();
                            if (roleType == typeof(SharedObjects.Product.ProductRole))
                            {
                                productRoles = rolesResponse.Records?.Cast<ProductRole>().ToList();
                            }
                            else if (roleType == typeof(ProductIntegration.Model.ProductRole))
                            {
                                var rolesToProcess = rolesResponse.Records?.Cast<ProductIntegration.Model.ProductRole>().ToList();
                                if (rolesToProcess.Count > 0)
                                {
                                    productRoles = new List<ProductRole>();
                                    rolesToProcess.ForEach(p =>
                                    {
                                        if (p.IsAssigned)
                                        {
                                            productRoles.Add(new ProductRole() { ID = p.GetRoleId, Name = p.GetName, IsAssigned = p.IsAssigned });
                                        }
                                    });
                                }
                            }

                            if (product == (int)ProductEnum.ResidentPortal)
                            {
                                var levels = rolesResponse.Records?.Cast<Level>().ToList();
                                if (levels.Count > 0)
                                {
                                    productRoles = new List<ProductRole>();

                                    levels.ForEach(p =>
                                    {
                                        if (p.IsAssigned)
                                        {
                                            productRoles.Add(new ProductRole() { ID = p.Id, Name = p.Name, IsAssigned = p.IsAssigned });
                                        }
                                    });
                                }
                            }
                        }
                    }

                    //Get product specific other info and create product batch
                    if (product == (int)ProductEnum.UnifiedPlatform && roleTemplateUpdatedProducts.Contains((int)ProductEnum.UnifiedPlatform))
                    {
                        List<UL.Role> userRolesToDelete = GetAssignedRoleForPersona(product, subjectUserPersonaId, userPersona.OrganizationPartyId);
                        List<long> upfmPlatformRolesToDelete = userRolesToDelete.Select(p => p.RoleID).ToList();
                        foreach (int platformRole in upfmPlatformRolesToDelete)
                        {
                            _enterpriseRoleProductRepository.UpdateUnifiedPlatFormRole(platformRole, editorPersona.UserId, subjectUserPersonaId, true);
                        }
                        List<int> upfmPlatformRolesToInsert = productRoles.Select(p => Convert.ToInt32(p.ID)).ToList();
                        foreach (int platformRole in upfmPlatformRolesToInsert)
                        {
                            _enterpriseRoleProductRepository.UpdateUnifiedPlatFormRole(platformRole, editorPersona.UserId, subjectUserPersonaId);
                        }
                    }
                    else if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
                    {
                        if (productsWithNoProperties.Contains((int)(ProductEnum)product))
                        {
                            GetAOProductWithoutProperies(productListToCreate, productRoles, usePrimaryProperties, product, true);
                        }
                        else
                        {
                            propertiesResponse = _manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(editorUserPersonaId, subjectUserPersonaId, product);
                            propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);
                            BatchHelper.CreateAoBatchRecords(_userClaim, editorUserPersonaId, subjectUserPersonaId, isExternalUser, true, propertiesResponse,
                                product, productRoles, productListToCreate);
                        }
                    }
                    else
                    {
                        ProductBatch productBatchRecord = new ProductBatch();
                        propertiesResponse = _manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(editorUserPersonaId, subjectUserPersonaId, product, usePrimaryProperties);
                        if (propertiesResponse != null && propertiesResponse.Records != null && propertiesResponse.Records.Count > 0)
                        {
                            propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);
                            productBatchRecord = _manageProductBatch.GetProductBatchRecord(editorUserPersonaId, subjectUserPersonaId, productRoles, propertiesResponse, rolesResponse, product, usePrimaryProperties);
                        }
                        else 
                        {
                            productBatchRecord = BatchHelper.CreateProductBatchRecord(propertiesResponse, rolesResponse, product, usePrimaryProperties, integrationType);
                        }
                        if (integrationType == ProductIntegrationTypeEnum.UPFM)
                        {
                            var currentProductPropertiesData = _manageProductBatch.GetExistingUserPrimaryPropertiesData(subjectUserPersonaId, product);
                            var currentUnifiedUIPropertiesData = _manageProductBatch.GetExistingUserPrimaryPropertiesData(subjectUserPersonaId, (int)ProductEnum.UnifiedUI);
                            var propertiesToRemove = currentProductPropertiesData.Except(currentUnifiedUIPropertiesData)
                                .Except(propertiesResponse.Records?.Count > 0 ? productBatchRecord.InputJson.PropertyList.Select(m => Convert.ToInt32(m)) : new List<int>()).ToList();
                            if (propertiesToRemove?.Count > 0)
                            {
                                productBatchRecord.InputJson.RemovedPropertyList = propertiesToRemove.Select(i => i.ToString()).ToList();
                            }
                        }
                        if (propertiesResponse != null && propertiesResponse.Records?.Count == 0)
                        {
                            productBatchRecord.InputJson.IsAssigned = false;
                        }
                        productListToCreate.Add(productBatchRecord);
                    }
                }
                Dictionary<string, RolePropertyList> oneSiteAndOtherProducts = new Dictionary<string, RolePropertyList>();
                bool isOnesiteMix = false;
                if (productListToCreate?.Count > 0)
                {
                    int totalProductCount = productListToCreate.Count;
                    Log.Write(LogEventLevel.Debug, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"{batchProcessorType} product batch update started to user - {subjectUserPersonaId} - product count {totalProductCount}");

                    if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.OneSite)
                           && (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease) || productListToCreate.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement)))
                    {
                        // need to combine the Lead2Lease and OneSite product details so they can run synchronously				
                        isOnesiteMix = true;
                        ProductBatch pbOneSite = (from a in productListToCreate
                                                  where a.ProductId == (int)ProductEnum.OneSite
                                                  select a).FirstOrDefault();

                        ProductBatch pbLead2Lease = null;
                        ProductBatch pbSeniorLead = null;

                        oneSiteAndOtherProducts.Add(ProductEnum.OneSite.ToString(), pbOneSite.InputJson);

                        if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease))
                        {
                            pbLead2Lease = (from a in productListToCreate
                                            where a.ProductId == (int)ProductEnum.Lead2Lease
                                            select a).FirstOrDefault();

                            oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbLead2Lease.InputJson);
                            productListToCreate.Remove(pbLead2Lease);
                        }

                        if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement))
                        {
                            pbSeniorLead = (from a in productListToCreate
                                            where a.ProductId == (int)ProductEnum.SeniorLeadManagement
                                            select a).FirstOrDefault();

                            oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbSeniorLead.InputJson);
                            productListToCreate.Remove(pbSeniorLead);
                        }
                    }
                }
                if (roleTemplateDeletedProducts?.Count > 0)
                {
                    foreach (var product in roleTemplateDeletedProducts)
                    {
                        if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
                        {
                            if (productsWithNoProperties.Contains((int)(ProductEnum)product))
                            {
                                GetAOProductWithoutProperies(productListToCreate, productRoles, usePrimaryProperties, product,false);
                            }
                            else
                            {
                                BatchHelper.CreateAoBatchRecords(_userClaim, editorUserPersonaId, subjectUserPersonaId, isExternalUser, true, null,
                                 product, null, productListToCreate, true);
                            }
                        }
                        else
                        {
                            ProductBatch pb = new ProductBatch()
                            {
                                ProductId = product,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
                            };
                            productListToCreate.Add(pb);
                        }
                    }
                }

                inputAOJSON = BundleAoProducts(productListToCreate);
                if (productListToCreate?.Count > 0)
                {
                    bool isBatchCompleted = _enterpriseRoleProductRepository.SaveProductBatch(editorUserPersonaId, subjectUserPersonaId, editorPersona.RealPageId, productListToCreate,
                        JsonConvert.SerializeObject(oneSiteAndOtherProducts), isOnesiteMix, (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser, impersonatorUserLoginOnly.UserId, inputAOJSON);

                    if (!isBatchCompleted)
                    {
                        Log.Write(LogEventLevel.Error, "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"{batchProcessorType} is failed");

                        return "Error";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, exception: ex, messageTemplate: "{ActionName} - {state}", propertyValue0: "ProcessEnterpriseRolesAndPrimaryPropertiesData", propertyValue1: $"Exception during {batchProcessorType} product batch data insert to user - {subjectUserPersonaId}");
                return "Error";
            }
            return "";
        }

        private void GetAOProductWithoutProperies(IList<ProductBatch> productListToCreate, IList<ProductRole> productRoles, bool usePrimaryProperties, int product, bool isAssigned)
        {
            productListToCreate.Add(
            new ProductBatch()
            {
                ProductId = (int)(ProductEnum)product,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson =
             new RolePropertyList()
             {
                 PropertyList = new List<string>(),
                 RoleList = (productRoles != null && productRoles?.Count > 0) ? (from i in productRoles select i.Name).ToList() : new List<string>(),
                 CompanyId = 0,
                 PropertyGroupList = new List<string>(),
                 UsePrimaryProperties = usePrimaryProperties,
                 IsAssigned = isAssigned
             }
            });
        }

        private List<int> GetProductsWithNoProperties()
        {
            var _productsWithNoProperties = new List<int>();
            var upSettingList = GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
            var productsWithNoProperties = upSettingList?.FirstOrDefault(ps => ps.Name.Equals($"UserAccessDetails_ProductsWithNoProperties", StringComparison.InvariantCultureIgnoreCase))?.Value;
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

        private bool GetPrimaryPropertySettingsForCompanyAndProduct(int productId)
        {
            var productGlobalSettingType = _productInternalSettingRepository.GetProductSettingByType("UsePrimaryProperties");
            var companyProductSettings = _productRepository.GetProductSettings(_userClaim.OrganizationRealPageGuid);

            int organizationUsePrimaryProperties = -1;
            var settings = _unifiedSettingsRepository.GetUnifiedSettings(_userClaim.OrganizationPartyId, "Company");
            if (settings.Any(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase)))
            {
                var settingValue = settings.FirstOrDefault(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase)).Value;
                int.TryParse(settingValue, out organizationUsePrimaryProperties);
            }

            if (organizationUsePrimaryProperties >= 0)
            {
                //Assign PrimaryProperty flag for Product
                string productUsePrimaryPropertiesGlobalStr = productGlobalSettingType?.Where(p => p.Name.ToLower() == "useprimaryproperties" && p.ProductId == productId)?.FirstOrDefault()?.Value?.Trim();
                int.TryParse(companyProductSettings?.Where(p => p.Name.ToLower() == "useprimaryproperties" && p.ProductId == productId)?.FirstOrDefault()?.Value?.Trim(), out int companyProductUsePrimaryPropertySetting);
                if (productUsePrimaryPropertiesGlobalStr != null && (int.TryParse(productUsePrimaryPropertiesGlobalStr, out int productUsePrimaryPropertiesGlobal) && productUsePrimaryPropertiesGlobal >= 0))
                {
                    return productUsePrimaryPropertiesGlobal == 1 && organizationUsePrimaryProperties == 1 && companyProductUsePrimaryPropertySetting == 1;
                }
            }
            return false;
        }

        private IList<ProductRole> GetProductRoleList(List<RoleTemplateProductRole> roleTemplateProductRole, int productId)
        {
            ListResponse rolesResponse = new ListResponse();
            IList<ProductRole> productRoles = new List<ProductRole>();

            var productRoleData = roleTemplateProductRole?.Where(p => p.ProductId == productId);

            var roleTemplateRoles = productRoleData?.Select(p => new
            {
                p.RoleTemplateProductRoleMappingId,
                p.ProductRoleId,
                p.ProductRoleName
            }).Distinct();


            //Roles
            //List<string> productRoles = new List<string>();
            //IList<ProductRole> productRoles = new List<ProductRole>();
            foreach (var role in roleTemplateRoles)
            {
                if (role.RoleTemplateProductRoleMappingId != 0)
                {
                    productRoles.Add(new ProductRole
                    {
                        ID = role.ProductRoleId.ToString(),
                        Name = role.ProductRoleName,
                        IsAssigned = true
                    });
                }
            }

            return productRoles;
        }



        private void GetUserRolesAndProperties(long editorPersonaId, long newUserPersonaId, int? enterpriseRoleTemplateId,
            List<RoleTemplateProductRole> roleTemplateProductRole, ListResponse rolesResponse, ManageProductBatch manageProductBatch,
            ListResponse propertiesResponse, IList<ProductRole> productRoles, IEnumerable<ProductProperty> productList, int productId, long organizationPartyId)
        {

            propertiesResponse = manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(editorPersonaId, newUserPersonaId, productId);
            propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);
            productList = propertiesResponse.Records.Cast<ProductProperty>();
            if (enterpriseRoleTemplateId != null)
            {
                productRoles = GetProductRoleList(roleTemplateProductRole, productId);
                //rolesResponse = new ListResponse()
                //{
                //    Records = productRoles.Cast<object>().ToList(),
                //    TotalRows = productRoles.Count,
                //    RowsPerPage = productRoles.Count,
                //    TotalPages = 1,
                //    ErrorReason = ""
                //};
            }
            else
            {
                rolesResponse = manageProductBatch.GetProductRoles(editorPersonaId, newUserPersonaId, productId, organizationPartyId, _userClaim);
                if (rolesResponse.Records.Count > 0)
                {
                    var roleType = rolesResponse.Records[0].GetType();
                    if (roleType == typeof(SharedObjects.Product.ProductRole))
                    {
                        productRoles = rolesResponse.Records?.Cast<ProductRole>().ToList();
                    }
                    else if (roleType == typeof(ProductIntegration.Model.ProductRole))
                    {
                        var rolesToProcess = rolesResponse.Records?.Cast<ProductIntegration.Model.ProductRole>().ToList();
                        if (rolesToProcess.Count > 0)
                        {
                            productRoles = new List<ProductRole>();
                            rolesToProcess.ForEach(p =>
                            {
                                if (p.IsAssigned)
                                {
                                    productRoles.Add(new ProductRole() { ID = p.GetRoleId, Name = p.GetName, IsAssigned = p.IsAssigned });
                                }
                            });
                        }
                    }
                }
            }

            //int aoCurrentProductId = (int)ProductEnumHelper.GetAoProductEnum(aoUserCompanyPropertyRoleDetail.ProductName);
            //_integrationTypeFactory.GetIntegrationTypeForProductId(aoCurrentProductId);
            //productRoles = new List<ProductRole>();
            //if (enterpriseRoleTemplateId != null)
            //{
            //    productRoles = GetProductRoleList(roleTemplateProductRole, aoCurrentProductId);
            //    rolesResponse = new ListResponse()
            //    {
            //        Records = productRoles.Cast<object>().ToList(),
            //        TotalRows = productRoles.Count,
            //        RowsPerPage = productRoles.Count,
            //        TotalPages = 1,
            //        ErrorReason = ""
            //    };
            //}
            //else
            //{
            //    rolesResponse = manageProductBatch.GetProductRoles(editorPersonaId, newUserPersonaId, aoCurrentProductId, userClaim.OrganizationPartyId, _userClaim);
            //}

            //propertiesResponse = manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(editorPersonaId, newUserPersonaId, aoCurrentProductId);
            //propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);
        }

        public static string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0)
        {
            StringBuilder sb = new StringBuilder();
            // Check if any AO products in product batch and group them
            var aoProductList = productList.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
            if (aoProductList.Any())
            {
                // bundle Ao products under product Id 4 and then remove
                ProductBatch aoProductsBatch = new ProductBatch
                {
                    ProductId = (int)ProductEnum.AssetOptimizer,
                    StatusTypeId = 5,
                    RetryCount = 0,
                    BatchProcessorGroupId = batchProcessorGroupId,
                    InputJson = null
                };

                dynamic expandoList = new ExpandoObject();
                expandoList.IsAssigned = true; // useless for AO
                expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();

                // Collect ALL Json(s) for AO products
                foreach (var aoProduct in aoProductList)
                {
                    dynamic expandoAo = new ExpandoObject();

                    expandoAo.SelectedRoleValues = aoProduct.InputJson.RoleList;
                    expandoAo.SelectedPortfolioValues = aoProduct.InputJson.PropertyList;
                    expandoAo.CompanyId = aoProduct.InputJson.CompanyId;

                    expandoAo.Product = ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId);
                    expandoAo.DivisionName = ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId);
                    ;
                    expandoAo.PropertyGroups = aoProduct.InputJson.PropertyGroupList;

                    expandoAo.IsAssigned = aoProduct.InputJson.IsAssigned;

                    expandoAo.ProductId = aoProduct.ProductId;

                    expandoAo.UsePrimaryProperties = aoProduct.InputJson.UsePrimaryProperties;

                    // add in collection
                    expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);

                    // remove newly created AoProduct from main list
                    productList.Remove(aoProduct);
                }

                sb.Append(JsonConvert.SerializeObject(expandoList));
                productList.Add(aoProductsBatch);
            }
            return sb.ToString();
        }


        /// <summary>
        /// Used to get the list of product roles assigned to the user
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="organizationPartyId">Optional Organization PartyId</param>  
        /// <returns>List of Roles</returns>
        private List<UL.Role> GetAssignedRoleForPersona(int productId, long userPersonaId, long organizationPartyId)
        {
            var cacheKey = $"sp_ListRolesForProductsByPersonaId_{productId}_{userPersonaId}_{organizationPartyId}";
            MemoryCache.Default.Remove(cacheKey);
            List<UL.Role> propRole = _userRoleRightRepository.ListRoleByPersona(productId, userPersonaId, organizationPartyId);
            return propRole;
        }



    }
}
