using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Saml;
using ProductRole = UnifiedLogin.SharedObjects.Product.ProductRole;
using UnifiedLogin.SharedObjects.Product.Accounting;
using Serilog;
using Serilog.Events;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage User Product Batch for Cloning
    /// </summary>
    public class ManageCloneProductBatch
    {
        private readonly DefaultUserClaim _userClaim;

        private readonly IntegrationTypeFactory _integrationTypeFactory;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageCloneProductBatch(DefaultUserClaim userClaim)
        {
            _userClaim = userClaim;

            var manageProduct = new ManageProduct(_userClaim);
            var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
            var manageProductOneSite = new ManageProductOneSite(_userClaim);
            var productRepository = new ProductRepository(_userClaim);
            var productInternalSettingRepository = new ProductInternalSettingRepository();

            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin,
                manageProductOneSite, productRepository, productInternalSettingRepository, _userClaim);
        }

        /// <summary>
        /// Gets Product Batch
        /// </summary> 
        public IList<ProductBatch> GetUserProductBatchData(long personaId, List<PersonaProductUserDetails> userProducts, long baseOrgAdminPersonaId, UPFMProperty upfmProperty, List<ProductSettingList> productSettingList, bool externalUser = false)
        {
            IList<ProductBatch> productListToCreate = new List<ProductBatch>();
            IManageBlueBook manageBlueBook = new ManageBlueBook(_userClaim);
            ListResponse propertiesResponse = new ListResponse();
            ListResponse propertyGroupResponse = new ListResponse();
            ListResponse rolesResponse = new ListResponse();
            foreach (PersonaProductUserDetails product in userProducts)
            {
                try
                {
                    bool translateProperties = false;
                    bool personaProductUsePrimaryProperty = false;
                    bool usePrimaryProperties = false;
                    bool productEnabledForPrimaryProperty = IsProductEnabledForUsePrimaryProperty(product.ProductId);

                    var productSetting = productSettingList.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == product.ProductId);

                    if (productSetting != null)
                    {
                        personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
                    }

                    usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty;
                    translateProperties = (productEnabledForPrimaryProperty && personaProductUsePrimaryProperty && upfmProperty.id != null);

                    if (product.ProductId == (int)ProductEnum.OneSite)
                    {
                        ManageProductOneSite mg = new ManageProductOneSite(_userClaim);
                        propertiesResponse = mg.GetOneSitePropertyList(baseOrgAdminPersonaId, personaId, true, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = mg.GetOneSiteRoleList(baseOrgAdminPersonaId, personaId, true, null);
                        productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.FinancialSuite)
                    {
                        ManageProductOneSiteAccounting accounting = new ManageProductOneSiteAccounting(_userClaim);
                        propertiesResponse = accounting.GetUserProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        propertyGroupResponse = accounting.GetUserPropertyGroups(baseOrgAdminPersonaId, personaId, null);
                        rolesResponse = accounting.GetUserRoles(baseOrgAdminPersonaId, personaId, null);
                        ListResponse companiesResponse = accounting.GetUserCompanies(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateFinancialSuiteProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, companiesResponse, propertyGroupResponse, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.MarketingCenter)
                    {
                        ManageProductMarketingCenter marketing = new ManageProductMarketingCenter(_userClaim);
                        propertiesResponse = marketing.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = marketing.GetRoles(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateMarketingCenterProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.OpsBuyer)
                    {
                        ManageProductOps opsbuyer = new ManageProductOps(_userClaim);
                        propertiesResponse = opsbuyer.GetCompanyAssets(baseOrgAdminPersonaId, personaId, false, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = opsbuyer.GetRoles(baseOrgAdminPersonaId, personaId, "", null);
                        productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.VendorServices)
                    {
                        ManageProductVendorServices vs = new ManageProductVendorServices(_userClaim);
                        propertiesResponse = vs.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = vs.GetRoles(baseOrgAdminPersonaId, personaId, AccessType.Property, null);
                        var notifications = vs.GetNotificationSettings(baseOrgAdminPersonaId, personaId);
                        propertyGroupResponse = vs.GetPropertyGroups(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateVendorServiceProductBatchRecord(propertiesResponse, rolesResponse, propertyGroupResponse, notifications, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.ClientPortal)
                    {
                        ManageProductClientPortal cp = new ManageProductClientPortal(_userClaim);
                        propertiesResponse = cp.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = cp.GetRoles(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.AdminSupportPortal)
                    {
                        ManageProductAdminSupportPortal asp = new ManageProductAdminSupportPortal(_userClaim);
                        propertiesResponse = asp.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = asp.GetRoles(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.ProspectContactCenter)
                    {
                        ManageProductProspectContact prospContact = new ManageProductProspectContact(_userClaim);
                        propertiesResponse = prospContact.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.Lead2Lease)
                    {
                        ManageProductLead2Lease l2l = new ManageProductLead2Lease(_userClaim);
                        propertiesResponse = l2l.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = l2l.GetRoles(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.ResidentPortal)
                    {
                        ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(_userClaim);
                        propertiesResponse = manageProductResidentPortal.ListProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        List<ILevel> LevelList = manageProductResidentPortal.ListLevels(baseOrgAdminPersonaId, personaId);
                        Notifications notifications = manageProductResidentPortal.GetNotificationSettings(baseOrgAdminPersonaId, personaId);
                        List<IMessagingGroups> messagingGroups = manageProductResidentPortal.ListMessageGroups(baseOrgAdminPersonaId, personaId);
                        productListToCreate.Add(CreateResidentPortalProductBatchRecord(propertiesResponse, LevelList, notifications, messagingGroups, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.Insurance)
                    {
                        ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(_userClaim);
                        propertiesResponse = manageProductRentersInsurance.ListProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        IList<ProductRole> productRoleList = manageProductRentersInsurance.ListRoles(baseOrgAdminPersonaId, personaId);
                        productListToCreate.Add(CreateRentersInsuranceProductBatchRecord(propertiesResponse, productRoleList, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.OnSite)
                    {
                        ManageProductOnSite manageProductOnSite = new ManageProductOnSite(_userClaim);
                        propertiesResponse = manageProductOnSite.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        rolesResponse = manageProductOnSite.GetRoles(baseOrgAdminPersonaId, personaId, null);
                        var regionResponse = manageProductOnSite.GetRegions(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateOnSiteBatchRecord(propertiesResponse, rolesResponse, regionResponse, product.ProductId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.UtilityManagement)
                    {
                        ManageProductRum manageProductrum = new ManageProductRum(_userClaim);
                        propertiesResponse = manageProductrum.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        propertyGroupResponse = manageProductrum.GetPropertyGroups(baseOrgAdminPersonaId, personaId, null);
                        var regionResponse = manageProductrum.GetRegions(baseOrgAdminPersonaId, personaId, null);
                        var accesstypeResponse = manageProductrum.GetUMGlobalRoles(baseOrgAdminPersonaId, personaId, null);                   
                        rolesResponse = manageProductrum.GetRoles(baseOrgAdminPersonaId, personaId, null);
                        productListToCreate.Add(CreateRumProductBatchRecord(propertiesResponse, propertyGroupResponse, regionResponse, rolesResponse, new ListResponse(), usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.SelfProvisioningPortal)
                    {
                        ManageProductSelfProvisioningPortal manageProductSelfProvisioningPortal = new ManageProductSelfProvisioningPortal(_userClaim);
                        productListToCreate.Add(CreateSelfProvisioningPortalProductBatchRecord(product.ProductId));
                    }
                    else if (product.ProductId == (int)ProductEnum.LeadManagement)
                    {
                        var productLogic = ManageProductFactory.GetProductLogic(product.ProductId, baseOrgAdminPersonaId, personaId, _userClaim);
                        var productUser = productLogic.GetProductUser();
                        var integrationType = _integrationTypeFactory.GetIntegration(product.ProductId);
                        propertiesResponse = integrationType.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        productListToCreate.Add(CreateILMProductBatchRecord(ProductEnum.LeadManagement, propertiesResponse,
                            productUser.Roles.ConvertAll<string>(i => i.ToString()), null, usePrimaryProperties));//no groups for LM
                    }
                    else if (product.ProductId == (int)ProductEnum.LeadAnalytics)
                    {
                        var productLogic = ManageProductFactory.GetProductLogic(product.ProductId, baseOrgAdminPersonaId, personaId, _userClaim);
                        var productUser = productLogic.GetProductUser();
                        var integrationType = _integrationTypeFactory.GetIntegration(product.ProductId);
                        propertiesResponse = integrationType.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        if (translateProperties)
                        {
                            propertiesResponse = manageBlueBook.TranslateProductPrimaryPropertiesData(upfmProperty, product.ProductId, propertiesResponse);
                        }
                        productListToCreate.Add(CreateILMProductBatchRecord(ProductEnum.LeadAnalytics, propertiesResponse,
                            productUser.Roles.ConvertAll<string>(i => i.ToString()), productUser.PropertyGroups, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.RPDocumentManagement)
                    {
                        productListToCreate.Add(CreateDocManagementBatchRecords(_userClaim, baseOrgAdminPersonaId, personaId, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.PortfolioManagement)
                    {
                        var productLogic = ManageProductFactory.GetProductLogic(product.ProductId, baseOrgAdminPersonaId, personaId, _userClaim);
                        var productUser = productLogic.GetProductUser();
                        var propertyRoles = productUser.PropertyRoleList;
                        var roles = productUser.RoleList.ConvertAll<string>(i => i.ToString());

                        productListToCreate.Add(CreateProductBatchRecordForPortfolioManagement(propertyRoles, roles, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.DepositAlternative)
                    {
                        var productLogic = ManageProductFactory.GetProductLogic(product.ProductId, baseOrgAdminPersonaId, personaId, _userClaim);
                        var productUser = productLogic.GetProductUser();

                        productListToCreate.Add(CreateProductBatchRecordForDepositIQ(productUser, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.ClickPay)
                    {
                        var productLogic = ManageProductFactory.GetProductLogic(product.ProductId, baseOrgAdminPersonaId, personaId, _userClaim);
                        var productUser = productLogic.GetProductUser();
                        var organizationRoles = productUser.OrganizationRoles;

                        productListToCreate.Add(CreateProductBatchRecordForClickPay(organizationRoles, usePrimaryProperties));
                    }
                    else if (product.ProductId == (int)ProductEnum.KnockCRM)
                    {
                        var integrationType = _integrationTypeFactory.GetIntegration(product.ProductId);
                        propertyGroupResponse = integrationType.GetPropertyGroups(baseOrgAdminPersonaId, personaId, null);
                        rolesResponse = integrationType.GetRoles(baseOrgAdminPersonaId, personaId, _userClaim.OrganizationPartyId, null, null);
                        var productBatchRecord = CreateKnockCRMBatchRecord(propertyGroupResponse, rolesResponse, product.ProductId, usePrimaryProperties);
                        productListToCreate.Add(productBatchRecord);
                    }
                    else if (!ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product.ProductId))
                    {
                        var integrationType = _integrationTypeFactory.GetIntegration(product.ProductId);
                        propertiesResponse = integrationType.GetProperties(baseOrgAdminPersonaId, personaId, null);
                        rolesResponse = integrationType.GetRoles(baseOrgAdminPersonaId, personaId, _userClaim.OrganizationPartyId, null, null);

                        var productBatchRecord = CreateProductBatchRecord(propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties);
                        productListToCreate.Add(productBatchRecord);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(LogEventLevel.Error, ex, "{ActionName} - {state}", new object[] { "GetUserProductBatchData", $"Exception during clone user for product - {product?.ProductName}" });
                }
            }

            // Check if any AO products exists & then add them in batch
            var aoProductList = userProducts.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
            if (aoProductList.Any())
            {
                var batches = CreateAoBatchRecords(_userClaim, baseOrgAdminPersonaId, personaId, productSettingList, externalUser);
                foreach (var productBatch in batches)
                {
                    // add only if userProducts has productId else product is modified after clone
                    if (userProducts.Any(x => x.ProductId == productBatch.ProductId))
                    {
                        productListToCreate.Add(productBatch);
                    }
                }
            }

            return productListToCreate;
        }

        private ProductBatch CreateProductBatchRecordForClickPay(List<OrganizationRole> userOrganizationRole, bool usePrimaryProperties)
        {
            var pb = new ProductBatch()
            {
                ProductId = (int)ProductEnum.ClickPay,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList()
                {
                    OrganizationRoleList = userOrganizationRole,
                    UsePrimaryProperties = usePrimaryProperties
                }
            };

            return pb;
        }

        private ProductBatch CreateProductBatchRecordForDepositIQ(IntegrationProductUser productUser, bool usePrimaryProperties)
        {
            ProductBatch productBatch = new ProductBatch()
            {
                ProductId = (int)ProductEnum.DepositAlternative,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList()
                {
                    RoleList = productUser.Roles,
                    CanReceiveMonthlyReport = productUser.CanReceiveMonthlyReport,
                    PropertyGroupList = productUser.PropertyGroups,
                    PropertyList = productUser.Properties,
                    UsePrimaryProperties = usePrimaryProperties
                }
            };

            return productBatch;
        }

      
        private ProductBatch CreateIntegrationMarketplaceBatchRecord(int existingRoleId, int productProductId, bool usePrimaryProperties)
        {
            var roleList = new List<string> { existingRoleId.ToString() };
            ProductBatch productBatch = new ProductBatch()
            {
                ProductId = productProductId,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList() { RoleList = roleList, UsePrimaryProperties = usePrimaryProperties }
            };

            return productBatch;
        }

        private ProductBatch CreateILMProductBatchRecord(ProductEnum ilmProduct, ListResponse productUserProperties,
            List<string> productUserRoles, List<string> productUserGroups, bool usePrimaryProperties)
        {
            List<string> propertyList = new List<string>();
            IEnumerable<object> propertiesCollection = (IEnumerable<object>)productUserProperties.Records;
            if (productUserProperties.Records != null)
            {
                foreach (object item in propertiesCollection)
                {
                    if (((ProductProperties)item).IsAssigned)
                    {
                        propertyList.Add(((ProductProperties)item).GetPropertyId.ToString());
                    }
                }
            }

            var pb = new ProductBatch()
            {
                ProductId = (int)ilmProduct,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList()
                {
                    PropertyList = propertyList,
                    RoleList = productUserRoles,
                    PropertyGroupList = productUserGroups,
                    UsePrimaryProperties = usePrimaryProperties
                }
            };

            return pb;
        }

        private ProductBatch CreateProductBatchRecordForPortfolioManagement(List<PAMRolePropertyList> rolePropertyList, List<string> roleList, bool usePrimaryProperties)
        {
            var pb = new ProductBatch()
            {
                ProductId = (int)ProductEnum.PortfolioManagement,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList() { RolePropertiesList = rolePropertyList, RoleList = roleList, UsePrimaryProperties = usePrimaryProperties }
            };

            return pb;
        }

        private ProductBatch CreateOnSiteBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, ListResponse regionResponse, int productId, bool usePrimaryProperties)
        {
            List<string> propertyList = new List<string>();
            List<string> roleList = new List<string>();
            List<string> regionList = new List<string>();

            bool allProperties = false;
            bool allRegions = false;

            IEnumerable<object> propertiesCollection = propertiesResponse.Records;
            if (propertiesResponse.Additional != null)
            {
                allProperties = CheckForAllProperties(propertiesResponse.Additional);
            }

            if (allProperties)
            {
                propertyList.Add("-1");
            }
            else
            {
                foreach (object item in propertiesCollection)
                {
                    if (((OnSiteProperty)item).IsAssigned)
                    {
                        propertyList.Add(((OnSiteProperty)item).GetPropertyId.ToString());
                    }
                }
            }

            RolePropertyList inputJson = new RolePropertyList { PropertyList = propertyList };

            /**/
            IEnumerable<object> regionCollection = regionResponse.Records;
            if (regionResponse.Additional != null)
            {
                allRegions = CheckForAllRegions(regionResponse.Additional);
            }

            if (allRegions)
            {
                regionList.Add("-1");
            }
            else
            {
                foreach (object item in regionCollection)
                {
                    if (((OnSiteRegion)item).IsAssigned)
                    {
                        regionList.Add(((OnSiteRegion)item).GetRegionId.ToString());
                    }
                }
            }

            inputJson.RegionList = regionList;

            /**/

            foreach (object item in rolesResponse.Records)
            {
                var isAssigned = ((OnSiteRole)item).IsAssigned;
                bool result = isAssigned != null && isAssigned.Value;

                if (result)
                {
                    roleList.Add(((OnSiteRole)item).Level.ToString());
                }
            }

            inputJson.RoleList = roleList;
            inputJson.UsePrimaryProperties = usePrimaryProperties;
            ProductBatch productBatch = new ProductBatch()
            {
                ProductId = productId,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = inputJson
            };

            return productBatch;
        }

        private bool CheckForAllRegions(object additionalInfo)
        {
            bool allProperties = false;
            if (additionalInfo.GetType().Name.ToUpper() != "STRING")
            {
                Dictionary<string, bool> additionalDataCollection = (Dictionary<string, bool>)additionalInfo;
                foreach (KeyValuePair<string, bool> pair in additionalDataCollection)
                {
                    if (pair.Key == "allProperties")
                    {
                        allProperties = pair.Value;
                    }
                }
            }

            return allProperties;
        }

        private ProductBatch CreateProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID, bool usePrimaryProperties)
        {
            List<string> PropertyList = new List<string>();
            List<string> RoleList = new List<string>();
            bool allProperties = false;
            IEnumerable<object> propertiesCollection;
            if (propertiesResponse.Records != null)
            {
                propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
            }
            else
            {
                propertiesCollection = new List<object>();
            }

            if (propertiesResponse.Additional != null)
            {
                allProperties = CheckForAllProperties(propertiesResponse.Additional);
            }
            if (_integrationTypeFactory.GetIntegrationTypeForProductId(productID) == ProductIntegrationTypeEnum.StandardV1)
            {
                if (rolesResponse.Records != null)
                {
                    IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
                    foreach (object item in roleCollection)
                    {
                        if (((ProductIntegration.Model.ProductRole)item).IsAssigned)
                        {
                            RoleList.Add(((ProductIntegration.Model.ProductRole)item).GetRoleId);
                        }
                    }
                }
            }
            else if (productID != (int)ProductEnum.ProspectContactCenter)
            {
                if (rolesResponse.Records != null)
                {
                    IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
                    foreach (object item in roleCollection)
                    {
                        if (((ProductRole)item).IsAssigned)
                        {
                            RoleList.Add(((ProductRole)item).ID);
                        }
                    }
                }
            }

            if (allProperties)
            {
                if (productID == (int)ProductEnum.ClientPortal ||
                    _integrationTypeFactory.GetIntegrationTypeForProductId(productID) == ProductIntegrationTypeEnum.UPFM)
                {
                    PropertyList.Add("-1");
                }
                else if (productID == (int)ProductEnum.OneSite ||
                         productID == (int)ProductEnum.FinancialSuite ||
                         productID == (int)ProductEnum.ProspectContactCenter ||
                         productID == (int)ProductEnum.MarketingCenter ||
                         _integrationTypeFactory.GetIntegrationTypeForProductId(productID) == ProductIntegrationTypeEnum.StandardV1)
                {
                    PropertyList.Add("ALL");
                }
            }
            else if (_integrationTypeFactory.GetIntegrationTypeForProductId(productID) == ProductIntegrationTypeEnum.StandardV1)
            {
                foreach (object item in propertiesCollection)
                {
                    if (((ProductIntegration.Model.ProductProperties)item).IsAssigned)
                    {
                        PropertyList.Add(((ProductIntegration.Model.ProductProperties)item).GetPropertyId);
                    }
                }
            }
            else
            {
                foreach (object item in propertiesCollection)
                {
                    if (productID == (int)ProductEnum.OpsBuyer)
                    {
                        if (((UnifiedLogin.SharedObjects.Product.Ops.AssetGroup)item).IsAssigned)
                        {
                            PropertyList.Add(((UnifiedLogin.SharedObjects.Product.Ops.AssetGroup)item).ID);
                        }
                    }
                    else if (((ProductProperty)item).IsAssigned.Value)
                    {
                        if (_integrationTypeFactory.GetIntegrationTypeForProductId(productID) == ProductIntegrationTypeEnum.UPFM)
                        {
                            PropertyList.Add(((ProductProperty)item).Alias);
                        }
                        else
                        {
                            PropertyList.Add(((ProductProperty)item).ID);
                        }
                    }
                }

            }

            ProductBatch pb = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList() { PropertyList = PropertyList, RoleList = RoleList, UsePrimaryProperties = usePrimaryProperties }
            };

            return pb;
        }


        private ProductBatch CreateKnockCRMBatchRecord(ListResponse propertyGroupResponse, ListResponse rolesResponse, int productID, bool usePrimaryProperties)
        {
            List<string> PropertyList = new List<string>();
            List<string> PropertyGroupList = new List<string>();
            List<string> RoleList = new List<string>();
            bool allProperties = false;
            IEnumerable<object> propertyGroupsCollection;

            if (_integrationTypeFactory.GetIntegrationTypeForProductId(productID) == ProductIntegrationTypeEnum.StandardV1)
            {
                if (rolesResponse.Records != null)
                {
                    IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
                    foreach (object item in roleCollection)
                    {
                        if (((ProductIntegration.Model.ProductRole)item).IsAssigned)
                        {
                            RoleList.Add(((ProductIntegration.Model.ProductRole)item).GetRoleId);
                        }
                    }
                }
            }

            if (propertyGroupResponse.Records != null)
            {
                propertyGroupsCollection = (IEnumerable<object>)propertyGroupResponse.Records;
            }
            else
            {
                propertyGroupsCollection = new List<object>();
            }


            foreach (object item in propertyGroupsCollection)
            {
                if (((ProductPropertyGroups)item).IsAssigned)
                {
                    PropertyGroupList.Add(((ProductPropertyGroups)item).GetGroupId);
                }
            }

            ProductBatch pb = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList() { PropertyList = PropertyList, PropertyGroupList = PropertyGroupList, RoleList = RoleList, UsePrimaryProperties = usePrimaryProperties }
            };

            return pb;
        }

        private ProductBatch CreateMarketingCenterProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID, bool usePrimaryProperties)
        {
            List<string> PropertyList = new List<string>();
            List<string> RoleList = new List<string>();
            bool isAssignNewPropertyByDefault = false;
            IEnumerable<object> propertiesCollection;
            if (propertiesResponse.Records != null)
            {
                propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
            }
            else
            {
                propertiesCollection = new List<object>();
            }

            if (propertiesResponse.Additional != null)
            {
                isAssignNewPropertyByDefault = CheckForIsAssignedNewPropertyFlag(propertiesResponse.Additional);
            }

            if (productID != (int)ProductEnum.ProspectContactCenter)
            {
                if (rolesResponse.Records != null)
                {
                    IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
                    foreach (object item in roleCollection)
                    {
                        if (((ProductRole)item).IsAssigned)
                        {
                            RoleList.Add(((ProductRole)item).ID);
                        }
                    }
                }
            }

            foreach (object item in propertiesCollection)
            {
                if (productID == (int)ProductEnum.OpsBuyer)
                {
                    if (((UnifiedLogin.SharedObjects.Product.Ops.AssetGroup)item).IsAssigned)
                    {
                        PropertyList.Add(((UnifiedLogin.SharedObjects.Product.Ops.AssetGroup)item).ID);
                    }
                }
                else if (((ProductProperty)item).IsAssigned.Value)
                {
                    PropertyList.Add(((ProductProperty)item).ID);
                }
            }

            ProductBatch pb = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList() { PropertyList = PropertyList, RoleList = RoleList, IsAssignedNewPropertyByDefault = isAssignNewPropertyByDefault, UsePrimaryProperties = usePrimaryProperties }
            };

            return pb;
        }

        private ProductBatch CreateFinancialSuiteProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, int productID, ListResponse companiesResponse, ListResponse propertyGroupResponse, bool usePrimaryProperties)
        {
            List<string> PropertyList = new List<string>();
            List<string> PropertyGroupList = new List<string>();
            List<string> RoleList = new List<string>();
            List<string> companiesList = new List<string>();
            bool hasAccessToSiteSpendManagementOnly = false;
            bool isAccountingAdmin = false;
            bool hasAccessToAllCurrentFutureProperties = false;
            IEnumerable<object> propertiesCollection;
            IEnumerable<object> propertyGroupsCollection;

            if (propertiesResponse.Records != null)
            {
                propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
            }
            else
            {
                propertiesCollection = new List<object>();
            }

            if (propertyGroupResponse.Records != null)
            {
                propertyGroupsCollection = (IEnumerable<object>)propertyGroupResponse.Records;
            }
            else
            {
                propertyGroupsCollection = new List<object>();
            }

            if (companiesResponse.Additional != null)
            {
                AccountingUser accountingUser = (AccountingUser)companiesResponse.Additional;
                hasAccessToSiteSpendManagementOnly = accountingUser.HasAccessToSiteSpendManagementOnly;
                isAccountingAdmin = accountingUser.IsAccountingAdmin;
                hasAccessToAllCurrentFutureProperties = accountingUser.HasAccessToAllCurrentFutureProperties;
            }

            if (companiesResponse?.Records != null)
            {
                IEnumerable<object> companiesCollection = (IEnumerable<object>)companiesResponse.Records;
                foreach (object item in companiesCollection)
                {
                    if (!string.IsNullOrEmpty(((ACCompany)item).Id))
                    {
                        companiesList.Add(((ACCompany)item).Id);
                    }
                }
            }

            if (rolesResponse.Records != null)
            {
                IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
                foreach (object item in roleCollection)
                {
                    if (((ProductRole)item).IsAssigned)
                    {
                        RoleList.Add(((ProductRole)item).ID);
                    }
                }
            }

            foreach (object item in propertiesCollection)
            {
                if (((ProductProperty)item).IsAssigned.Value)
                {
                    PropertyList.Add(((ProductProperty)item).ID);
                }
            }

            foreach (object item in propertyGroupsCollection)
            {
                if (((ProductPropertyGroup)item).IsAssigned.Value)
                {
                    PropertyList.Add(((ProductPropertyGroup)item).ID);
                }
            }

            ProductBatch pb = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList()
                {
                    PropertyList = PropertyList,
                    RoleList = RoleList,
                    HasAccessToSiteSpendManagementOnly = hasAccessToSiteSpendManagementOnly,
                    IsAccountingAdmin = isAccountingAdmin,
                    HasAccessToAllCurrentFutureProperties = hasAccessToAllCurrentFutureProperties,
                    CompaniesList = companiesList,
                    UsePrimaryProperties = usePrimaryProperties
                }
            };

            return pb;
        }

        private bool CheckForIsAssignedNewPropertyFlag(object additionalInfo)
        {
            bool isAssignNewPropertyByDefault = false;
            if (additionalInfo.GetType().Name.ToUpper() != "STRING")
            {
                Dictionary<string, bool> additionalDataCollection = (Dictionary<string, bool>)additionalInfo;
                foreach (KeyValuePair<string, bool> pair in additionalDataCollection)
                {
                    if (pair.Key.Equals("IsAssignedNewPropertyByDefault", StringComparison.OrdinalIgnoreCase))
                    {
                        isAssignNewPropertyByDefault = pair.Value;
                    }
                }
            }
            return isAssignNewPropertyByDefault;
        }

        private ProductBatch CreateVendorServiceProductBatchRecord(ListResponse propertiesResponse, ListResponse rolesResponse, ListResponse propertyGroup, UnifiedLogin.SharedObjects.Product.VendorServices.Notification notification, int productID, bool usePrimaryProperties)
        {
            List<string> PropertyList = new List<string>();
            List<string> RoleList = new List<string>();
            List<UnifiedLogin.SharedObjects.Product.VendorServices.PropertyGroup> propertyGroupList = new List<UnifiedLogin.SharedObjects.Product.VendorServices.PropertyGroup>();
            bool allProperties = false;

            IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
            IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;

            if (propertiesResponse.Additional != null)
            {
                allProperties = CheckForAllProperties(propertiesResponse.Additional);
            }

            // Below logic is applied when a user is being cloned from a user that has access to all properties. 
            if (propertiesResponse != null)
            {
                var unselectedPropertiesCount = propertiesCollection.Where(p => ((ProductProperty)p).IsAssigned == false).Count();
                if (unselectedPropertiesCount == propertiesCollection.Count())
                    allProperties = true;
            }


            foreach (object item in roleCollection)
            {
                if (((ProductRole)item).IsAssigned)
                {
                    RoleList.Add(((ProductRole)item).ID);
                }
            }

            if (propertyGroup.TotalRows > 0)
            {
                foreach (object item in propertyGroup.Records)
                {
                    if (((VendorServicesPropertyGroup)item).IsAssigned)
                    {
                        int? value = ((VendorServicesPropertyGroup)item).PropertyGroupId;
                        var propertyGroupData = new UnifiedLogin.SharedObjects.Product.VendorServices.PropertyGroup
                        {
                            Id = value,
                            IsAssigned = true,
                            Type = (UnifiedLogin.SharedObjects.Product.VendorServices.AccessTypeEnum)Enum.Parse(typeof(UnifiedLogin.SharedObjects.Product.VendorServices.AccessTypeEnum), ((VendorServicesPropertyGroup)item).AccessLevel)
                        };

                        propertyGroupList.Add(propertyGroupData);
                    }
                }
            }

            if (allProperties)
            {
                PropertyList.Add("-1");
            }
            else
            {
                foreach (object item in propertiesCollection)
                {
                    if (productID == (int)ProductEnum.OpsBuyer)
                    {
                        if (((UnifiedLogin.SharedObjects.Product.Ops.AssetGroup)item).IsAssigned)
                        {
                            PropertyList.Add(((UnifiedLogin.SharedObjects.Product.Ops.AssetGroup)item).ID);
                        }
                    }
                    else if (((ProductProperty)item).IsAssigned.Value)
                    {
                        PropertyList.Add(((ProductProperty)item).ID);
                    }
                }
            }

            var inputJson = new RolePropertyList();
            inputJson.PropertyList = PropertyList;
            inputJson.RoleList = RoleList;
            if (propertyGroupList.Count > 0)
            {
                inputJson.PropertyGroup = propertyGroupList;
            }

            inputJson.IsInsuranceExpired = notification.IsInsuranceExpired;
            inputJson.IsVendorRecommendationChanges = notification.IsVendorRecommendationChanges;
            inputJson.IsVendorNotLinkedToAnyProperty = notification.IsVendorNotLinkedToAnyProperty;
            inputJson.UsePrimaryProperties = usePrimaryProperties;

            ProductBatch pb = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = inputJson
            };

            return pb;
        }

        private IList<ProductBatch> CreateAoBatchRecords(DefaultUserClaim userClaim, long editorPersonaId, long newUserPersonaId, List<ProductSettingList> productSettingList, bool externalUser = false)
        {
            var productBatchList = new List<ProductBatch>();
            IList<AoUserCompanyPropertyRoleDetail> aoBIUserCompanyPropertyRoleDetails = new List<AoUserCompanyPropertyRoleDetail>();
            var manageProductAssetOptimization = new ManageProductAssetOptimization(userClaim);
            //below code block will add external user bi product to clone user batch.
            if (externalUser)
            {
                ISamlRepository samlRepository = new SamlRepository();
                string aoBIUserName = string.Empty;
                IList<SamlAttributes> productAttributes = samlRepository.GetProductSamlDetails(newUserPersonaId, (int)ProductEnum.AoBusinessIntelligence);
                if (productAttributes.Any(a => a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase)))
                {
                    aoBIUserName = (from a in productAttributes where a.Name.Equals("ProductUserName", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
                }
                if (aoBIUserName != null)
                {
                    aoBIUserCompanyPropertyRoleDetails = manageProductAssetOptimization.CopyRegularUser(editorPersonaId, newUserPersonaId, aoBIUserName);
                }
            }

            var aoUserCompanyPropertyRoleDetails = manageProductAssetOptimization.CopyRegularUser(editorPersonaId, newUserPersonaId);

            foreach (var aoBIUserCompanyPropertyRoleDetail in aoBIUserCompanyPropertyRoleDetails)
            {
                aoUserCompanyPropertyRoleDetails.Add(aoBIUserCompanyPropertyRoleDetail);
            }

            foreach (var aoUserCompanyPropertyRoleDetail in aoUserCompanyPropertyRoleDetails)
            {
                if (aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues == null)
                {
                    aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues = new List<int>();
                }

                if (aoUserCompanyPropertyRoleDetail.PropertyGroups == null)
                {
                    aoUserCompanyPropertyRoleDetail.PropertyGroups = new List<int>();
                }

                bool productEnabledForPrimaryProperty = IsProductEnabledForUsePrimaryProperty((int)ProductEnum.AssetOptimizer);

                var productSetting = productSettingList.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                                            && item.ProductId == (int)ProductEnumHelper.GetAoProductEnum(aoUserCompanyPropertyRoleDetail.ProductName));
                bool personaProductUsePrimaryProperty = false;
                bool usePrimaryProperties = false;
                if (productSetting != null)
                {
                    personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
                }

                usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty;

                var productBatch = new ProductBatch()
                {
                    ProductId = (int)ProductEnumHelper.GetAoProductEnum(aoUserCompanyPropertyRoleDetail.ProductName),
                    StatusTypeId = 5,
                    RetryCount = 0,
                    InputJson =
                        new RolePropertyList()
                        {
                            PropertyList = (from i in aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues select i.ToString()).ToList(),
                            RoleList = (from i in aoUserCompanyPropertyRoleDetail.SelectedRoleValues select i).ToList(),
                            CompanyId = aoUserCompanyPropertyRoleDetail.CompanyId,
                            PropertyGroupList = (from i in aoUserCompanyPropertyRoleDetail.PropertyGroups select i.ToString()).ToList(),
                            UsePrimaryProperties = usePrimaryProperties
                        }
                };

                productBatchList.Add(productBatch);
            }

            return productBatchList;
        }

        private ProductBatch CreateRumProductBatchRecord(ListResponse propertiesResponse, ListResponse groupResponse, ListResponse regionResponse, ListResponse rolesResponse, ListResponse accesstypeResponse, bool usePrimaryProperties)
        {
            List<string> propertyList = new List<string>();
            List<string> propertyGroupList = new List<string>();
            List<string> regionsList = new List<string>();
            List<string> roleList = new List<string>();

            IEnumerable<object> roleCollection = (IEnumerable<object>)rolesResponse.Records;
            foreach (object item in roleCollection)
            {
                if (((UnifiedLogin.SharedObjects.Product.Rum.Role)item).IsAssigned)
                {
                    roleList.Add(((UnifiedLogin.SharedObjects.Product.Rum.Role)item).Name);
                }
            }

            IEnumerable<object> regionCollection = (IEnumerable<object>)regionResponse.Records;
            if (regionResponse.Records != null)
            {
                foreach (object item in regionCollection)
                {
                    if (((RumPropertyGroup)item).IsAssigned)
                    {
                        regionsList.Add(((RumPropertyGroup)item).Id.ToString());
                    }
                }
            }


            IEnumerable<object> groupCollection = (IEnumerable<object>)groupResponse.Records;
            if (groupResponse.Records != null)
            {
                foreach (object item in groupCollection)
                {
                    if (((RumPropertyGroup)item).IsAssigned)
                    {
                        propertyGroupList.Add(((RumPropertyGroup)item).Id.ToString());
                    }
                }
            }

            IEnumerable<object> accessTypes = (IEnumerable<object>)accesstypeResponse.Records;
            if (accesstypeResponse.Records != null && propertyGroupList.Count == 0)
            {
                foreach (object item in accessTypes)
                {
                    if (((ProductRole)item).IsAssigned)
                    {
                        propertyGroupList.Add(((ProductRole)item).ID.ToString());
                    }
                }
            }

            IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
            if (propertiesResponse.Records != null)
            {
                foreach (object item in propertiesCollection)
                {
                    if (((RumPropertyGroup)item).IsAssigned)
                    {
                        propertyList.Add(((RumPropertyGroup)item).Id.ToString());
                    }
                }
            }
            // Below logic is applied when a user is being cloned from a user that has access to all properties. 
            if (propertiesCollection != null && propertyGroupList.Count == 0)
            {
                var unselectedPropertiesCount = propertiesCollection.Where(p => ((RumPropertyGroup)p).IsAssigned == false).Count();
                if (unselectedPropertiesCount == propertiesCollection.Count())
                    propertyList.Add("All");
            }

            ProductBatch pb = new ProductBatch()
            {
                ProductId = (int)ProductEnum.UtilityManagement,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = new RolePropertyList() { PropertyList = propertyList, PropertyGroupList = propertyGroupList, RegionList = regionsList, RoleList = roleList, UsePrimaryProperties = usePrimaryProperties }
            };

            return pb;
        }

        /// <summary>
        /// Create ResidentPortal ProductBatch Record
        /// </summary>
        /// <param name="propertiesResponse">list of Communities</param>
        /// <param name="rolesResponse"> list of Roles (Level of Access)</param>
        /// <param name="notifications">Notification Settings</param>
        /// <param name="messagingGroups">Message Groups</param>
        /// <param name="productID">Product Id</param>
        /// <returns>ProductBatch object</returns>
        private ProductBatch CreateResidentPortalProductBatchRecord(ListResponse propertiesResponse, List<ILevel> rolesResponse, Notifications notifications, List<IMessagingGroups> messagingGroups, int productID, bool usePrimaryProperties)
        {
            List<string> PropertyList = new List<string>();
            List<string> RoleList = new List<string>();
            List<string> MessageGroups = new List<string>();
            bool allProperties = false;

            IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
            if (propertiesResponse.Additional != null)
            {
                allProperties = CheckForAllProperties(propertiesResponse.Additional);
            }

            if (allProperties)
            {
                if (productID == (int)ProductEnum.ResidentPortal)
                {
                    PropertyList.Add("ALL");
                }
            }
            else
            {
                foreach (object item in propertiesCollection)
                {
                    if (((ProductProperty)item).IsAssigned.Value)
                    {
                        PropertyList.Add(((ProductProperty)item).ID);
                    }
                }
            }

            RolePropertyList inputJson = new RolePropertyList();
            inputJson.PropertyList = PropertyList;
            //RoleList - Level of Access
            string accessLevel = rolesResponse.Find(item => item.IsAssigned == true).Id.ToUpper();
            RoleList.Add(accessLevel);

            inputJson.RoleList = RoleList;
            //Notification Settings
            inputJson.Notifications = notifications;
            //Message Group
            foreach (MessagingGroups messageGroup in messagingGroups)
            {
                if (messageGroup.IsAssigned)
                {
                    MessageGroups.Add(messageGroup.Id);
                }
            }

            inputJson.MessageGroups = MessageGroups;
            inputJson.UsePrimaryProperties = usePrimaryProperties;

            ProductBatch productBatch = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = inputJson
            };

            return productBatch;
        }


        /// <summary>
        /// Create Renters Insurance ProductBatch Record
        /// </summary>
        /// <param name="propertiesResponse">list of Properties</param>
        /// <param name="rolesResponse"> list of Roles</param>
        /// <param name="productID">Product Id</param>
        /// <returns>ProductBatch object</returns>
        private ProductBatch CreateRentersInsuranceProductBatchRecord(ListResponse propertiesResponse, IList<ProductRole> rolesResponse, int productID, bool usePrimaryProperties)
        {
            List<string> PropertyList = new List<string>();
            List<string> RoleList = new List<string>();
            bool allProperties = false;

            IEnumerable<object> propertiesCollection = (IEnumerable<object>)propertiesResponse.Records;
            if (propertiesResponse.Additional != null)
            {
                allProperties = CheckForAllProperties(propertiesResponse.Additional);
            }

            if (allProperties)
            {
                if (productID == (int)ProductEnum.Insurance)
                {
                    PropertyList.Add("ALL");
                }
            }
            else
            {
                foreach (object item in propertiesCollection)
                {
                    if (((ProductProperty)item).IsAssigned.Value)
                    {
                        PropertyList.Add(((ProductProperty)item).ID);
                    }
                }
            }

            RolePropertyList inputJson = new RolePropertyList();
            inputJson.PropertyList = PropertyList;

            //RoleList
            string roleId = rolesResponse.ToList().Find(item => item.IsAssigned == true).ID;
            RoleList.Add(roleId);

            inputJson.RoleList = RoleList;
            inputJson.UsePrimaryProperties = usePrimaryProperties;
            ProductBatch productBatch = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = inputJson
            };

            return productBatch;
        }

        /// <summary>
        /// Create SelfProvisioningPortal ProductBatch Record
        /// </summary>
        /// <param name="productID">Product Id</param>
        /// <returns>ProductBatch object</returns>
        private ProductBatch CreateSelfProvisioningPortalProductBatchRecord(int productID)
        {
            RolePropertyList inputJson = new RolePropertyList();

            ProductBatch productBatch = new ProductBatch()
            {
                ProductId = productID,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = inputJson
            };

            return productBatch;
        }

        /// <summary>
        /// Used to copy DocManagement Users information to another user
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="createUserPersonaId"></param>
        /// <param name="personaId"></param>
        /// <returns></returns>
        private ProductBatch CreateDocManagementBatchRecords(DefaultUserClaim userClaim, long createUserPersonaId, long personaId, bool usePrimaryProperties)
        {
            ManageProductRPDocumentManagement manageProductRpDocumentManagement = new ManageProductRPDocumentManagement(userClaim);

            List<string> propertyList = new List<string>();
            List<string> departmentList = new List<string>();
            List<PAMRolePropertyList> lstRoleProperties = new List<PAMRolePropertyList>();

            //List<string> roleList = new List<string>();
            RolePropertyList inputJson = new RolePropertyList() { IsAssigned = true };

            ListResponse result = manageProductRpDocumentManagement.GetPropertyRoles(createUserPersonaId, personaId, null);
            if (result != null && result.Records.Count > 0)
            {
                IList<ProductRole> roleList = result.Records.Cast<ProductRole>().ToList().FindAll(p => p.IsAssigned);
                foreach (ProductRole role in roleList)
                {
                    PAMRolePropertyList objRole = new PAMRolePropertyList();
                    objRole.RoleId = role.ID;
                    if (role.Roletype != null)
                    {
                        // get the additional role info that is assigned to the user
                        result = manageProductRpDocumentManagement.GetRoleClassifierDataset(createUserPersonaId, personaId, role.ID, null);
                        if (result != null && result.Records.Count > 0)
                        {
                            IList<ProductProperty> assignedList = result.Records.Cast<ProductProperty>().ToList().FindAll(p => p.IsAssigned.Value);
                            List<string> propertyIds = new List<string>();
                            foreach (ProductProperty pp in assignedList)
                            {
                                propertyIds.Add(pp.ID);
                            }
                            objRole.PropertyIds = propertyIds;
                        }
                    }
                    lstRoleProperties.Add(objRole);
                }
                inputJson.RolePropertiesList = lstRoleProperties;
            }
            inputJson.UsePrimaryProperties = usePrimaryProperties;

            ProductBatch productBatch = new ProductBatch()
            {
                ProductId = (int)ProductEnum.RPDocumentManagement,
                StatusTypeId = 5,
                RetryCount = 0,
                InputJson = inputJson
            };

            return productBatch;
        }

        /// <summary>
        /// Check For All Properties as one of the Keys
        /// </summary>
        /// <param name="additionalInfo">additional Info to process the data</param>
        /// <returns>All Properties = true</returns>
        private bool CheckForAllProperties(object additionalInfo)
        {
            bool allProperties = false;
            if (additionalInfo.GetType().Name.ToUpper() != "STRING")
            {
                Dictionary<string, bool> additionalDataCollection = (Dictionary<string, bool>)additionalInfo;
                foreach (KeyValuePair<string, bool> pair in additionalDataCollection)
                {
                    if (pair.Key == "allProperties")
                    {
                        allProperties = pair.Value;
                    }
                }
            }

            return allProperties;
        }

        private bool IsProductEnabledForUsePrimaryProperty(int productId)
        {
            var productInternalSettingRepository = new ProductInternalSettingRepository();
            var productInternalSettingList = productInternalSettingRepository.GetProductInternalSettings(productId);
            var productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));

            if (productInternalSetting != null)
            {
                return productInternalSetting.Value.Trim() == "1" ? true : false;
            }
            return false;
        }

    }
}