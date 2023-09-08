using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using ProductRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductRole;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManagePrimaryPropertiesBatch
    {
        private readonly DefaultUserClaim _userClaim;
        private IProductRepository _productRepository;
        private readonly IntegrationTypeFactory _integrationTypeFactory;
        private IPropertyRepository _propertyRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IUnifiedSettingsRepository _unifiedSettingsRepository;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManagePrimaryPropertiesBatch(DefaultUserClaim userClaim)
        {
            _userClaim = userClaim;
            //var manageProductBatch = new ManageProductBatch(_userClaim);
            var manageProduct = new ManageProduct(_userClaim);
            var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
            var manageProductOneSite = new ManageProductOneSite(_userClaim);
            var productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _propertyRepository = new PropertyRepository();
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _unifiedSettingsRepository = new UnifiedSettingsRepository();
        
        }
        public ManagePrimaryPropertiesBatch(IProductRepository productRepository, IPropertyRepository propertyRepository)
        {
            _productRepository = productRepository;
            _propertyRepository = propertyRepository;
        }

        public string GeneratePrimaryPropertiesUserProductBatch(PrimaryPropertyBatch batch)
        {
            IList<ProductBatch> productListToCreate = new List<ProductBatch>();
            IManagePersona _managePersona = new ManagePersona();
            ManageProductBatch manageProductBatch = new ManageProductBatch(_userClaim);
            var editorPersona = _managePersona.GetPersona(batch.EditorUserPersonaId);
            var userPersona = _managePersona.GetPersona(batch.SubjectUserPersonaId);
            _userClaim.UserRealPageGuid = editorPersona.RealPageId;
            _userClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
            _userClaim.Rights = manageProductBatch.GetPersonaRoleRights(batch.EditorUserPersonaId, editorPersona.OrganizationPartyId);
            _userClaim.OrganizationPartyId = editorPersona.OrganizationPartyId;

            IPersonaRepository personaRepository = new PersonaRepository(_userClaim);
            IUserLoginRepository userLoginRepository = new UserLoginRepository();
            BatchProductBulkUpdateRepository productBulkUpdateRepository = new BatchProductBulkUpdateRepository(_userClaim);
            var batchGroup = productBulkUpdateRepository.CreateBatchProcessGroup();
            IList<Organization> organizationList = userLoginRepository.ListOrganizationByEnterpriseUserId(userPersona.RealPageId, null);
            var personaOrganization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);
            var personaProductSettings = personaRepository.GetPersonaProductSettings(batch.SubjectUserPersonaId);
            var personaProducts = _productRepository.ListProductsByPersonaId(userPersona.PersonaId, (Int32)UserUiStatusType.AccountCreationSuccessful, true).ToList();
            bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }

            string message = $"Primary Properties product update started to user - {batch.SubjectUserPersonaId}";
            Log.Write(LogEventLevel.Debug, message);
            bool personaProductUsePrimaryProperty = false;
            bool usePrimaryProperties = false;

            foreach (var product in personaProducts)
            {
                ListResponse propertiesResponse = new ListResponse();
                ListResponse rolesResponse = new ListResponse();
                personaProductUsePrimaryProperty = false;
                usePrimaryProperties = false;

                string prodmessage = $"Primary Properties product update batch record generation started for product - {product.ProductName}";
                Log.Write(LogEventLevel.Debug, prodmessage);

                var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product.ProductId);
                bool productEnabledForPrimaryProperty = manageProductBatch.IsProductEnabledForUsePrimaryProperty(product.ProductId);
                var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == product.ProductId);
                var ppEnabledForCompanyAndProduct = GetPrimaryPropertySettingsForCompanyAndProduct(product.ProductId);

                if (productSetting != null)
                {
                    personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
                }

                usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty && ppEnabledForCompanyAndProduct;

                if (usePrimaryProperties)
                {
                    rolesResponse = manageProductBatch.GetProductRoles(editorPersona.PersonaId, userPersona.PersonaId, product.ProductId, userPersona.OrganizationPartyId, _userClaim);

                    propertiesResponse = manageProductBatch.GetEnterpriseRoleUserPrimaryPropertiesData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, product.ProductId);
                    if (propertiesResponse.Records?.Count > 0)
                    {
                        propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);
                    }

                    if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product.ProductId))
                    {
                        var batchRecords = BatchHelper.CreateAoBatchRecords(_userClaim, batch.EditorUserPersonaId, batch.SubjectUserPersonaId, isExternalUser, true, propertiesResponse);
                        string inputAOJson = BatchHelper.BundleAoProducts(batchRecords, batchGroup.BatchProcessorGroupId);
                        // bundle Ao products under product Id 4 and then remove
                        ProductBatch aoProductsBatch = new ProductBatch
                        {
                            ProductId = (int)ProductEnum.AssetOptimizer,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                            InputJson = null
                        };
                        IList<ProductBatch> aOproductListToCreate = new List<ProductBatch>() { aoProductsBatch };
                        productBulkUpdateRepository.SaveAOProductBatch(batch, aOproductListToCreate, editorPersona, productBulkUpdateRepository, impersonatorUserLoginOnly, batchGroup.BatchProcessorGroupId, inputAOJson);
                    }
                    else
                    {
                        List<ProductRole> productRoles = null;
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

                            if (product.ProductId == (int)ProductEnum.ResidentPortal)
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

                        var productBatchRecord = manageProductBatch.GetProductBatchRecord(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, productRoles, propertiesResponse, rolesResponse, product.ProductId, usePrimaryProperties);
                        if (integrationType == ProductIntegrationTypeEnum.UPFM)
                        {
                            var currentProductPropertiesData = manageProductBatch.GetExistingUserPrimaryPropertiesData(batch.SubjectUserPersonaId, product.ProductId);
                            var currentUnifiedUIPropertiesData = manageProductBatch.GetExistingUserPrimaryPropertiesData(batch.SubjectUserPersonaId, (int)ProductEnum.UnifiedUI);
                            var propertiesToRemove = currentProductPropertiesData.Except(currentUnifiedUIPropertiesData).ToList();
                            if (propertiesToRemove?.Count > 0)
                            {
                                productBatchRecord.InputJson.RemovedPropertyList = propertiesToRemove.Select(i => i.ToString()).ToList();
                            }
                        }
                        if (propertiesResponse == null || propertiesResponse.Records == null || (propertiesResponse != null && propertiesResponse.Records?.Count == 0))
                        {
                            productBatchRecord.InputJson.IsAssigned = false;
                        }
                        productListToCreate.Add(productBatchRecord);
                    }
                }
            }
            try
            {
                Dictionary<string, RolePropertyList> oneSiteAndOtherProducts = new Dictionary<string, RolePropertyList>();
                bool isOnesiteMix = false;
                if (productListToCreate?.Count > 0)
                {
                    string btmessage = $"Primary Properties product batch update started to user - {batch.SubjectUserPersonaId} - product count {productListToCreate.Count}";
                    Log.Write(LogEventLevel.Debug, btmessage);

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
                if (productListToCreate?.Count > 0)
                {
                    int statusTypeId = (int)ProductBatchStatusType.Success;
                    bool isBatchCompleted = productBulkUpdateRepository.SaveProductBatch(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, editorPersona.RealPageId, productListToCreate, JsonConvert.SerializeObject(oneSiteAndOtherProducts), isOnesiteMix, (int)BatchProcessType.PrimaryPropertiesUpdateProductUser, impersonatorUserLoginOnly.UserId, batchGroup.BatchProcessorGroupId);
                    if (!isBatchCompleted)
                    {
                        statusTypeId = (int)ProductBatchStatusType.Error;
                    }
                    bool status = productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, statusTypeId);
                }
                else
                {
                    productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, (int)ProductBatchStatusType.Success);
                }
            }
            catch (Exception ex)
            {
                string exmessage = $"Exception during product primary properties updates to user - {batch.SubjectUserPersonaId} ";
                Log.Write(LogEventLevel.Error, ex, exmessage);
                productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, (int)ProductBatchStatusType.Error);
                return "Error";
            }
            return "";
        }

        private bool GetPrimaryPropertySettingsForCompanyAndProduct(int productId)
        {
            var productGlobalSettingType = _productInternalSettingRepository.GetProductSettingByType("UsePrimaryProperties");
            var companyProductSettings = _productRepository.GetProductSettings(_userClaim.OrganizationRealPageGuid);

            int organizationUsePrimaryProperties = -1;
            var settings = _unifiedSettingsRepository.GetUnifiedSettings(_userClaim.OrganizationPartyId, "Company");
            if (settings.Any(a => a.Name.Equals("PrimaryPropertyEnterpriseRole", StringComparison.OrdinalIgnoreCase)))
            {
                var settingValue = settings.FirstOrDefault(a => a.Name.Equals("PrimaryPropertyEnterpriseRole", StringComparison.OrdinalIgnoreCase)).Value;
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
    }
}
