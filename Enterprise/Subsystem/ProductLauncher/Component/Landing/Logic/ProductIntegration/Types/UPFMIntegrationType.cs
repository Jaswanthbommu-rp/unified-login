using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public class UPFMIntegrationType : IIntegrationType
    {
        private readonly int _productId;

        private readonly DefaultUserClaim _userClaims;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private IProductRepository _productRepository;

        private ManageUPFMProductsIntegration _manageUPFMProductIntegration => new ManageUPFMProductsIntegration(_productId, _userClaims);

        private IUPFMProduct _upfmProductIntegration => new UPFMProductIntegration(_productId, _userClaims);

        public UPFMIntegrationType(int productId, DefaultUserClaim userClaims, IProductInternalSettingRepository productInternalSettingRepository)
        {
            _productId = productId;
            _userClaims = userClaims;
            
            _productRepository = new ProductRepository(_userClaims);
            _productInternalSettingRepository = productInternalSettingRepository;
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetRoles(editorPersonaId, userPersonaId, _userClaims.OrganizationPartyId);

        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetUPFMProperties(editorPersonaId, userPersonaId, false, dataFilter);

        public ListResponse GetEnterpriseProperties(long userPersonaId, RequestParameter dataFilter)
        {
            var products = _productRepository.GetAllProducts();
            var productCode = products.FirstOrDefault(a => a.ProductId == _productId)?.BooksProductCode;
            return _manageUPFMProductIntegration.GetEnterpriseUPFMProperties(userPersonaId, _productId, productCode);
        }

        public ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, string roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter)
        {
            ListResponse result = new ListResponse();
            return result;
        }

        public ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetRightsByRole(editorPersonaId, partyId, roleId);

        public string CreateUser(ProductUserProperitiesRoles productUser,out List<AdditionalParameters> additionalParameters)
        {
            additionalParameters = null;
            var productPropertiesRoles = DeserializeJSON<UPFMProductPropertyRole>(productUser.InputJson);
            return _upfmProductIntegration.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
               productUser.AssignUserPersonaId, productPropertiesRoles, out additionalParameters);
        }

        public string ChangeUserType(ProductUserProperitiesRoles batchRecord)
        {
            var productPropertiesRoles = DeserializeJSON<UPFMProductPropertyRole>(batchRecord.InputJson);
            return _upfmProductIntegration.ChangeProductUserType(batchRecord.RealPageId, batchRecord.CreateUserPersonaId,
               batchRecord.AssignUserPersonaId, batchRecord.BatchProcessType, productPropertiesRoles);
        }

        public ListResponse GetAllRights(long editorPersonaId, long userPersonaId, RequestParameter dataFilter)
        {
            throw new NotImplementedException();
        }

        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter, string userLoginName = "")
        {
            throw new NotImplementedException();
        }

        private T DeserializeJSON<T>(string productUserInputJson)
        {
            if (string.IsNullOrEmpty(productUserInputJson))
            {
                return default(T);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(productUserInputJson.Trim());
            }
            catch
            {
                return default(T);
            }
        }

        public ListResponse GetPropertiesByGroup(long editorPersonaId, long userPersonaId, string propertyGroupId, RequestParameter dataFilter)
        {
            throw new NotImplementedException();
        }

        public ListResponse GetOrganizations(long editorPersonaId, long userPersonaId, string organizationRoleId, string organizationType)
        {
            throw new NotImplementedException();
        }

        public ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter dataFilter)
        {
            throw new NotImplementedException();
        }

        public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
        {
            throw new NotImplementedException();
        }

        public bool ExternalUserProfileChange(long editorPersonaId, ProductUserProfile productUserProfile)
        {
            throw new NotImplementedException();
        }

        public string UpdateUserProfile(ProductUserProperitiesRoles productUser)
        {
            // UPFM products are fully DB-managed; profile data (name/email) is stored in the
            // Unified Login database and requires no external API sync.
            return string.Empty;
        }

        public string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false)
        {
            var product = new ProductBase(_productId, _userClaims, _productInternalSettingRepository, _productRepository);
            return product.UpdateUserDetails(productUserAccountDetails, internalChange);
        }
        public ListResponse GetUserGroups(long editorPersonaId, long userPersonaId, long partyId, RequestParameter dataFilter)
        {
            throw new NotImplementedException();
        }
    }
}