using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using Serilog.Events;
using Serilog;
using System.Dynamic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using ProductRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductRole;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class BulkEnterpriseRoleLogic : IBulkEnterpriseRoleLogic
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
        public BulkEnterpriseRoleLogic(DefaultUserClaim userClaim)
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
            _userLoginRepository = new UserLoginRepository();
            _enterpriseRoleProductRepository = new BatchProductBulkUpdateRepository(_userClaim);
            _userRoleRightRepository = new UserRoleRightRepository();
        }


        public BulkEnterpriseRoleLogic(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims,
            IOneSiteProductService oneSiteProductService = null, IManageBlueBook manageBlueBook = null)
        {
            _userClaim = userClaims;
            _manageProductBatch = new ManageProductBatch(repository, messageHandler, userClaims, oneSiteProductService);
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

        public string UpdateEnterpriseToUsers(long editorUserPersonaId, List<long> userPersonaIds, RoleTemplateProductRoleMapping roleTemplateProductRoleMapping)
        {
            try
            {
                IList<ProductBatch> productListToCreate = new List<ProductBatch>();
                var editorPersona = _managePersona.GetPersona(editorUserPersonaId);
                string newproducts = string.Empty;
                string updateproducts = string.Empty;
                string deletedProducts = string.Empty;
                string inputAOJSON = string.Empty;
                foreach (var subjectUserPersonaId in userPersonaIds)
                {
                    var userPersona = _managePersona.GetPersona(subjectUserPersonaId);
                    _userClaim.UserRealPageGuid = editorPersona.RealPageId;
                    _userClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
                    _userClaim.Rights = _manageProductBatch.GetPersonaRoleRights(editorUserPersonaId, editorPersona.OrganizationPartyId);
                    _userClaim.OrganizationPartyId = editorPersona.OrganizationPartyId;
                    List<int> roleTemplateProducts = new List<int>();
                    List<int> roleTemplateUpdatedProducts = new List<int>();
                    List<int> roleTemplateDeletedProducts = new List<int>();
                    List<RoleTemplateProductRole> roleTemplateProductRole = new List<RoleTemplateProductRole>();
                    IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(userPersona.RealPageId, null);
                    var personaOrganization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);    
                    var personaProducts = _productRepository.ListProductsByPersonaId(userPersona.PersonaId, (Int32)UserUiStatusType.AccountCreationSuccessful).ToList();
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
                    roleTemplateProducts = personaProducts.Select(p => p.ProductId).ToList();                   
                    roleTemplateProducts = roleTemplateProducts.Distinct().ToList();
                    if (roleTemplateProducts?.Count > 0)
                    {
                        Log.Write(LogEventLevel.Debug, $"roleTemplateProducts count : {roleTemplateProducts.Count}, subjectUserPersonaId is {subjectUserPersonaId} ");
                        var productsWithNoProperties = GetProductsWithNoProperties();
                        bool isExternalUser = personaOrganization.RelationshipType != null && personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);
                        IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
                        if (_userClaim.ImpersonatedBy != Guid.Empty)
                        {
                            impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
                        }
                        foreach (var product in roleTemplateProducts)
                        {
                            if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
                            {
                                if (productsWithNoProperties.Contains((int)(ProductEnum)product))
                                {
                                    GetAOProductWithoutProperies(productListToCreate, null, true, product, false);
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
                        inputAOJSON = BundleAoProducts(productListToCreate);
                        if (productListToCreate?.Count > 0)
                        {
                            bool isBatchCompleted = _enterpriseRoleProductRepository.SaveProductBatch(editorUserPersonaId, subjectUserPersonaId, editorPersona.RealPageId, productListToCreate,
                                null, false, (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser, impersonatorUserLoginOnly.UserId, inputAOJSON);

                            if (!isBatchCompleted)
                            {
                                Log.Write(LogEventLevel.Error, $"insert batch process is failed , for subject personaId : {subjectUserPersonaId} ");
                                return "Error";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string exmessage = $"UpdateEnterpriseToUsers : Exception during editor personaId : {editorUserPersonaId} product.";
                Log.Write(LogEventLevel.Error, ex, exmessage);
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

        private static string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0)
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
    }
}
