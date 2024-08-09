using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;

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
        private BatchProductBulkUpdateRepository _productBulkUpdateRepository;
        private IManagePersona _managePersona;
        private ManageProductBatch _manageProductBatch;
        private ManageEnterpriseRolesPrimaryProperties _manageEnterpriseRolesPrimaryProperties;
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
            _productBulkUpdateRepository = new BatchProductBulkUpdateRepository(_userClaim);
            _managePersona = new ManagePersona(_userClaim);
            _manageProductBatch = new ManageProductBatch(_userClaim);
         //   _manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(_userClaim);

        }
		public ManagePrimaryPropertiesBatch(IProductRepository productRepository, IPropertyRepository propertyRepository)
		{
			_productRepository = productRepository;
			_propertyRepository = propertyRepository;
		}

        public string GeneratePrimaryPropertiesUserProductBatch(PrimaryPropertyBatch batch)
        {
            try
            {
                var editorPersona = _managePersona.GetPersona(batch.EditorUserPersonaId);
                var userPersona = _managePersona.GetPersona(batch.SubjectUserPersonaId);
                _userClaim.UserRealPageGuid = editorPersona.RealPageId;
                _userClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
                _userClaim.Rights = _manageProductBatch.GetPersonaRoleRights(batch.EditorUserPersonaId, editorPersona.OrganizationPartyId);
                _userClaim.OrganizationPartyId = editorPersona.OrganizationPartyId;
                _manageEnterpriseRolesPrimaryProperties = new ManageEnterpriseRolesPrimaryProperties(_userClaim);
                string stausMessage = _manageEnterpriseRolesPrimaryProperties.ProcessEnterpriseRolesAndPrimaryPropertiesData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId);
                if (string.IsNullOrEmpty(stausMessage))
                    _productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, (int)ProductBatchStatusType.Success);
                else _productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, (int)ProductBatchStatusType.Error);
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, exception: ex, messageTemplate: "{ActionName} - {state}", propertyValue0: "GeneratePrimaryPropertiesUserProductBatch", propertyValue1: $"Error: {ex.Message}");
                _productBulkUpdateRepository.UpdatePrimaryPropertyProductBatch(batch.PrimaryPropertyBatchProcessId, (int)ProductBatchStatusType.Error);
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
    }
}
