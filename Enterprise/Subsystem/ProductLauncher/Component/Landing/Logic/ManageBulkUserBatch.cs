using RP.Enterprise.Foundation.DataAccess.Component;
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
    public class ManageBulkUserBatch
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
        private ManageBulkUsers _manageBulkUsers;
        private readonly IRepository _repository;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageBulkUserBatch(DefaultUserClaim userClaim)
		{
        
            _userClaim = userClaim;
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

        }
		public ManageBulkUserBatch(IProductRepository productRepository, IPropertyRepository propertyRepository)
		{
			_productRepository = productRepository;
			_propertyRepository = propertyRepository;
		}

        public string GenerateProductUnAssignProductBatch(BulkUserBatch batch)
        {
            try
            {
                var editorPersona = _managePersona.GetPersona(batch.EditorUserPersonaId);
                _userClaim.UserRealPageGuid = editorPersona.RealPageId;
                _userClaim.OrganizationRealPageGuid = editorPersona.Organization.RealPageId;
                _userClaim.Rights = _manageProductBatch.GetPersonaRoleRights(batch.EditorUserPersonaId, editorPersona.OrganizationPartyId);
                _userClaim.OrganizationPartyId = editorPersona.OrganizationPartyId;
                _manageBulkUsers = new ManageBulkUsers(_repository,_userClaim);
                string stausMessage = _manageBulkUsers.ProcessProductUnAssignBatchData(batch.EditorUserPersonaId, batch.SubjectUserPersonaId, batch.BulkUserBatchProcessId);
                if (string.IsNullOrEmpty(stausMessage))
                    _productBulkUpdateRepository.UpdateBulkUserProductBatch(batch.BulkUserBatchProcessId, (int)ProductBatchStatusType.Success);
                else _productBulkUpdateRepository.UpdateBulkUserProductBatch(batch.BulkUserBatchProcessId, (int)ProductBatchStatusType.Error);
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, exception: ex, messageTemplate: "{ActionName} - {state}", propertyValue0: "GenerateProductUnAssignProductBatch", propertyValue1: $"Error: {ex.Message}");
                _productBulkUpdateRepository.UpdateBulkUserProductBatch(batch.BulkUserBatchProcessId, (int)ProductBatchStatusType.Error);
                return "Error";
            }

            return "";
        }
    }
}
