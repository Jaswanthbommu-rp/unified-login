using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using ProductRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductRole;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{

    /// <summary>
    /// Manage Enterprise Role User Product Batch 
    /// </summary>
    public class ManageEnterpriseRoleProductBatch
    {
        private readonly DefaultUserClaim _userClaim;
        private IProductRepository _productRepository;
        private readonly IntegrationTypeFactory _integrationTypeFactory;
        private IManagePersona _managePersona;
        private ManageProductBatch _manageProductBatch;
        private BatchProductBulkUpdateRepository _enterpriseRoleProductRepository;
        private ManageEnterpriseRolesPrimaryProperties _manageEnterpriseRolesPrimaryProperties;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageEnterpriseRoleProductBatch(DefaultUserClaim userClaim)
        {
            _userClaim = userClaim;

            var manageProduct = new ManageProduct(_userClaim);
            var manageUnifiedLogin = new ManageUnifiedLogin(_userClaim);
            var manageProductOneSite = new ManageProductOneSite(_userClaim);
            var productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin,
                manageProductOneSite, _productRepository, productInternalSettingRepository, _userClaim);
            _manageProductBatch = new ManageProductBatch(_userClaim);
            _managePersona = new ManagePersona(_userClaim);
            _enterpriseRoleProductRepository = new BatchProductBulkUpdateRepository(_userClaim);
          //  _manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(_userClaim);
        }

        public string GenerateEnterpriseRoleUserProductBatch(EnterpriseRoleBatch batch)
        {
            string stausMessage = string.Empty;
            try
            {
                var editorPersona = _managePersona.GetPersona(batch.EditorUserPersonaId);
                var userPersona = _managePersona.GetPersona(batch.SubjectUserPersonaId);
                _userClaim.UserRealPageGuid = editorPersona.RealPageId;
                _userClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
                _userClaim.Rights = _manageProductBatch.GetPersonaRoleRights(batch.EditorUserPersonaId, editorPersona.OrganizationPartyId);
                _userClaim.OrganizationPartyId = editorPersona.OrganizationPartyId;
                _manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(_userClaim);

                if (batch.BatchProcessTypeId == (int)BatchProcessType.BulkAddUpdateEnterpriseRole)
                {
                    stausMessage = _manageEnterpriseRolesPrimaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, batch.EnterpriseRoleTemplateId, batch.CreatedDateTime, batch.BatchProcessTypeId,true);
                    if (string.IsNullOrEmpty(stausMessage))
                    {
                        stausMessage = _manageEnterpriseRolesPrimaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, batch.EnterpriseRoleTemplateId, batch.CreatedDateTime, batch.BatchProcessTypeId);
                    }
                }
                else
                {
                    stausMessage = _manageEnterpriseRolesPrimaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, batch.EnterpriseRoleTemplateId, batch.CreatedDateTime);
                }
                if (string.IsNullOrEmpty(stausMessage))
                { 
                    _enterpriseRoleProductRepository.UpdateEnterpriseRoleProductBatch(batch.EnterpriseRoleBatchProcessId, (int)ProductBatchStatusType.Success);
                }
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, exception: ex, messageTemplate: "{ActionName} - {state}", propertyValue0: "GenerateEnterpriseRoleUserProductBatch", propertyValue1: "Error");
                _enterpriseRoleProductRepository.UpdateEnterpriseRoleProductBatch(batch.EnterpriseRoleBatchProcessId, (int)ProductBatchStatusType.Error);
                return "Error";
            }
            return "";
        }

        private List<string> GetSelectedProperties(ListResponse productResult)
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
                        selectedProperties.Add(property.AssetID);
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
    }
}
