using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageBulkEnterpriseRoles : IManageBulkEnterpriseRoles
    {
        public ManageBulkEnterpriseRoles()
        {
        }
        private readonly DefaultUserClaim _userClaim;
        private IProductRepository _productRepository;
        private readonly IntegrationTypeFactory _integrationTypeFactory;
        private IPropertyRepository _propertyRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IUnifiedSettingsRepository _unifiedSettingsRepository;
        private BatchProductBulkUpdateRepository _productBulkUpdateRepository;
        private IManagePersona _managePersona;
        private ManageProductBatch _manageProductBatch;
        private IBulkEnterpriseRoleLogic _bulkEnterpriseRoleLogic;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ManageBulkEnterpriseRoles(DefaultUserClaim userClaim)
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
            _bulkEnterpriseRoleLogic = new BulkEnterpriseRoleLogic(_userClaim);

        }
        public ManageBulkEnterpriseRoles(IProductRepository productRepository, IPropertyRepository propertyRepository)
        {
            _productRepository = productRepository;
            _propertyRepository = propertyRepository;
        }

        public string UpdateEnterpriseToUsers(long editorUserPersonaId, List<long> userPersonaIds, RoleTemplateProductRoleMapping roleTemplateProductRole)
        {
            try
            {
                string stausMessage = _bulkEnterpriseRoleLogic.UpdateEnterpriseToUsers(editorUserPersonaId, userPersonaIds, roleTemplateProductRole);
                if (string.IsNullOrEmpty(stausMessage))
                    _ = _productBulkUpdateRepository.InsertBatchPersonaIdToEnterpriseRoleBatchProcessAsync(editorUserPersonaId, userPersonaIds);
                else Log.Write(LogEventLevel.Debug, $"Editor personaId is : {editorUserPersonaId}, roleTemplateName is {roleTemplateProductRole.RoleTemplateName} ");
            }
            catch (Exception ex)
            {
                Log.Write(LogEventLevel.Error, ex.Message);
                Log.Write(LogEventLevel.Debug, $"Editor personaId is : {editorUserPersonaId}, roleTemplateName is {roleTemplateProductRole.RoleTemplateName} ");
                return "Error";
            }
            return "";
        }
  
    }
}
