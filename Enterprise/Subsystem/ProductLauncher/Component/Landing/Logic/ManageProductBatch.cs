using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageProductBatch : IManageProductBatch
    {
        #region Private Variables		
        public DefaultUserClaim _userClaims;
        //readonly IProductInternalSettingRepository _productInternalSettingRepository;
        readonly IManageUnifiedLogin _manageUnifiedLogin;
        private readonly IManageProductOneSite _manageProductOneSite;
        protected IPropertyRepository _propertyRepository;
        private IManageBlueBook _manageBlueBook;

        private readonly IIntegrationTypeFactory _integrationTypeFactory;

        private readonly IProductRepository _productRepository;
        private readonly IUserRoleRightRepository _userRoleRightRepository;
        private readonly ISharedDataRepository _sharedDataRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly ManageProductPanel _manageProductPanel;
        #endregion

        public ManageProductBatch(DefaultUserClaim userClaims)
        {
            _userClaims = userClaims;
            var manageProduct = new ManageProduct(_userClaims);
            var manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
            var manageProductOneSite = new ManageProductOneSite(_userClaims);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _propertyRepository = new PropertyRepository();
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, _productInternalSettingRepository, _userClaims);
            _userRoleRightRepository = new UserRoleRightRepository();
            _sharedDataRepository = new SharedDataRepository();
            _manageProductPanel = new ManageProductPanel(_userClaims);

        }

        public ManageProductBatch(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims, IOneSiteProductService oneSiteProductService = null)
        {
            _userClaims = userClaims;
            _userRoleRightRepository = new UserRoleRightRepository(repository,  messageHandler, userClaims);
            _sharedDataRepository = new SharedDataRepository(repository, userClaims);
            _productRepository = new ProductRepository(repository, userClaims);
            _propertyRepository = new PropertyRepository(repository);
            var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
            var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            var manageProduct = new ManageProduct(repository, userClaims, messageHandler);
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, _productInternalSettingRepository, userClaims);
            _manageBlueBook = new ManageBlueBook(userClaims, repository, _productInternalSettingRepository, messageHandler);
            _manageProductPanel = new ManageProductPanel(userClaims, repository, _manageBlueBook, messageHandler, manageProductOneSite);
        }

        public ProductBatch GetProductBatchRecord(long editorUserPersonaId, long subjectUserPersonaId, IList<ProductRole> productRoles, ListResponse propertiesResponse, ListResponse rolesResponse, int product, bool usePrimaryProperties)
        {
            ProductBatch batchRecord = new ProductBatch();
            ListResponse propertyGroupResponse = new ListResponse();
            if (product == (int)ProductEnum.FinancialSuite)
            {
                ManageProductOneSiteAccounting accounting = new ManageProductOneSiteAccounting(_userClaims);
                propertyGroupResponse = accounting.GetUserPropertyGroups(editorUserPersonaId, subjectUserPersonaId, null);
                ListResponse companiesResponse = accounting.GetUserCompanies(editorUserPersonaId, subjectUserPersonaId, null);
                return BatchHelper.CreateFinancialSuiteProductBatchRecord(propertiesResponse, rolesResponse, product, companiesResponse, propertyGroupResponse, usePrimaryProperties);
            }
            else if (product == (int)ProductEnum.VendorServices)
            {
                ManageProductVendorServices vs = new ManageProductVendorServices(_userClaims);
                var notifications = vs.GetNotificationSettings(editorUserPersonaId, subjectUserPersonaId);
                propertyGroupResponse = vs.GetPropertyGroups(editorUserPersonaId, subjectUserPersonaId, null);
                return BatchHelper.CreateVendorServiceProductBatchRecord(propertiesResponse, rolesResponse, propertyGroupResponse, notifications, product, usePrimaryProperties);
            }
            else if (product == (int)ProductEnum.ResidentPortal)
            {
                ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaims);

                List<ILevel> LevelList = new List<ILevel>();
                foreach (var rRole in productRoles)
                {
                    LevelList.Add(new Level { Id = rRole.ID, Name = rRole.Name, IsAssigned = rRole.IsAssigned });
                }

                Notifications notifications = manageProductResidentPortal.GetNotificationSettings(editorUserPersonaId, subjectUserPersonaId);
                List<IMessagingGroups> messagingGroups = manageProductResidentPortal.ListMessageGroups(editorUserPersonaId, subjectUserPersonaId);
                return BatchHelper.CreateResidentPortalProductBatchRecord(propertiesResponse, LevelList, notifications, messagingGroups, product, usePrimaryProperties);
            }
            else if (product == (int)ProductEnum.OnSite)
            {
                ManageProductOnSite manageProductOnSite = new ManageProductOnSite(_userClaims);
                var regionResponse = manageProductOnSite.GetRegions(editorUserPersonaId, subjectUserPersonaId, null);
                return BatchHelper.CreateOnSiteBatchRecord(propertiesResponse, rolesResponse, regionResponse, product, usePrimaryProperties);
            }
            //else if (product == (int)ProductEnum.ClickPay)
            //{
            //	//Don't know how it works with enterprise role, since it need more information along with the role

            //	//var productLogic = ManageProductFactory.GetProductLogic(product, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, _userClaim);
            //	//var productUser = productLogic.GetProductUser();
            //	//var organizationRoles = productUser.OrganizationRoles;

            //	//productListToCreate.Add(CreateProductBatchRecordForClickPay(organizationRoles, usePrimaryProperties));
            //}
            else if (product == (int)ProductEnum.DepositAlternative)
            {
                var productLogic = ManageProductFactory.GetProductLogic(product, editorUserPersonaId, subjectUserPersonaId, _userClaims);
                var productUser = productLogic.GetProductUser();
                productUser.RoleList = productRoles.Select(p => p.ID).ToList();
                return BatchHelper.CreateProductBatchRecordForDepositIQ(productUser, usePrimaryProperties);
            }
            else if (product == (int)ProductEnum.IntegrationMarketplace)
            {
                var existingRoleId = Convert.ToInt32(productRoles.Select(p => p.ID).FirstOrDefault());
                return BatchHelper.CreateIntegrationMarketplaceBatchRecord(existingRoleId, product, usePrimaryProperties);
            }
            else if (product == (int)ProductEnum.LeadManagement)
            {
                var productLogic = ManageProductFactory.GetProductLogic(product, editorUserPersonaId, subjectUserPersonaId, _userClaims);
                var productUser = productLogic.GetProductUser();

                return BatchHelper.CreateILMProductBatchRecord(ProductEnum.LeadManagement, productUser.Properties,
                    productRoles.Select(p => p.ID).ToList(), null, usePrimaryProperties);//no groups for LM
            }
            else if (product == (int)ProductEnum.LeadAnalytics)
            {
                var productLogic = ManageProductFactory.GetProductLogic(product, editorUserPersonaId, subjectUserPersonaId, _userClaims);
                var productUser = productLogic.GetProductUser();

                return BatchHelper.CreateILMProductBatchRecord(ProductEnum.LeadAnalytics, productUser.Properties, productRoles.Select(p => p.ID).ToList(), productUser.PropertyGroups, usePrimaryProperties);
            }
            //else if (product == (int)ProductEnum.RPDocumentManagement)
            //{
            //	//Don't know how it works with enterprise role, since it need more information along with the role
            //	break;
            //}
            else if (product == (int)ProductEnum.PortfolioManagement)
            {
                var productLogic = ManageProductFactory.GetProductLogic(product, editorUserPersonaId, subjectUserPersonaId, _userClaims);
                var productUser = productLogic.GetProductUser();
                var propertyRoles = productUser.PropertyRoleList;
                var roles = productRoles.Select(p => p.ID).ToList();

                return BatchHelper.CreateProductBatchRecordForPortfolioManagement(propertyRoles, roles, usePrimaryProperties);
            }
            else if (product == (int)ProductEnum.UtilityManagement)
            {
                ManageProductRum manageProductrum = new ManageProductRum(_userClaims);
                propertyGroupResponse = manageProductrum.GetPropertyGroups(editorUserPersonaId, subjectUserPersonaId, null);
                var regionResponse = manageProductrum.GetRegions(editorUserPersonaId, subjectUserPersonaId, null);

                return BatchHelper.CreateRumProductBatchRecord(propertiesResponse, propertyGroupResponse, regionResponse, rolesResponse, usePrimaryProperties);
            }
            else
            {
                var type = _integrationTypeFactory.GetIntegrationTypeForProductId(product);

                var productBatchRecord = BatchHelper.CreateProductBatchRecord(propertiesResponse, rolesResponse, product, usePrimaryProperties, type);
                return productBatchRecord;
            }

            return batchRecord;
        }

        public ListResponse GetUserPrimaryPropertiesData(long editorPersonaId, long userPersonaId, int productId)
        {
            var productPropertyIdList = new List<string>();
            IManageProductPanel manageProductPanel = new ManageProductPanel(_userClaims);
            ListResponse result = new ListResponse();

            var userProperties = _propertyRepository.ListUPFMPropertyInstanceByPersona(userPersonaId, ProductEnum.UnifiedPlatform);
            result = manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null);
            if (!result.IsError)
            {
                UPFMProperty upfmProperty = new UPFMProperty();
                upfmProperty.id = userProperties?.Select(p => p.InstanceId.ToString()).ToList();

                result = manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, result);
            }
            return result;
        }

        public ListResponse GetEnterpriseRoleUserPrimaryPropertiesData(long editorPersonaId, long userPersonaId, int productId, bool usePrimaryProperties = true)
        {
            var productPropertyIdList = new List<string>();
            ListResponse result = new ListResponse();

            var userProperties = _propertyRepository.ListUPFMPropertyInstanceByPersona(userPersonaId, ProductEnum.UnifiedPlatform);
            result = _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null);
            
            //primaryprop turnoff for user / company dont execute below code
            if (!result.IsError && usePrimaryProperties)
            {
                UPFMProperty upfmProperty = new UPFMProperty();
                upfmProperty.id = userProperties?.Select(p => p.InstanceId.ToString()).ToList();

                result = _manageProductPanel.CompareProductAndPrimaryProperties(upfmProperty, productId, result);
            }
            return result;
        }

        public List<int> GetExistingUserPrimaryPropertiesData(long userPersonaId, int productId)
        {
            var userProperties = _propertyRepository.ListUPFMPropertyInstanceIdByPersona(userPersonaId, productId);

            return userProperties;
        }

        public ListResponse GetProductProperties(long editorPersonaId, long userPersonaId, int productId, DefaultUserClaim userClaim)
        {
            var productResult = _manageProductPanel.GetProductProperties(editorPersonaId, userPersonaId, productId, null);
            return productResult;
        }

        public ListResponse GetProductRoles(long editorPersonaId, long userPersonaId, int productId, long partyId, DefaultUserClaim userClaim)
        {
     
            var productResult = _manageProductPanel.GetProductRoles(editorPersonaId, userPersonaId, partyId, productId, null, null);
            return productResult;
        }

        public bool IsProductEnabledForUsePrimaryProperty(int productId)
        {
            ProductInternalSetting productInternalSetting = new ProductInternalSetting();
        
            IList<ProductInternalSetting> productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings(productId);
            productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));

            if (productInternalSetting != null)
            {
                return productInternalSetting.Value.Trim() == "1" ? true : false;
            }
            return false;
        }

        public List<string> GetPersonaRoleRights(long personaId, long orgPartyId)
        {
            List<string> userRights = new List<string>();
        
            List<SharedObjects.Product.UserManagement.Role> userRoles = _userRoleRightRepository.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, personaId, orgPartyId);

            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"enterpriseRoleProcessgetRolesByParty_{orgPartyId}_{(int)ProductEnum.UnifiedPlatform}";
            IList<UserRoleRights> roleList = rpCache.GetFromCache<IList<UserRoleRights>>(cacheKey, 60, () =>
            {
            
                IList<int> productList = _sharedDataRepository.GetProductIdsByCompany(orgPartyId);
              
                return _userRoleRightRepository.GetAllRoleRights(orgPartyId, productList, (int)ProductEnum.UnifiedPlatform);
            });

            foreach (SharedObjects.Product.UserManagement.Role userRole in userRoles)
            {
                foreach (Right right in roleList.FirstOrDefault(r => r.RoleId == userRole.RoleID).UserRights)
                {
                    if (!string.IsNullOrWhiteSpace(right.RightNickName) && !string.IsNullOrWhiteSpace(right.RightNickName.Trim()) && !userRights.Contains(right.RightNickName))
                    {
                        userRights.Add(right.RightNickName);
                    }
                }
            }

            return userRights;
        }
    }
}
