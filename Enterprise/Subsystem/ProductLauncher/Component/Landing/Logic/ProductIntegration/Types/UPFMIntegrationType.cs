using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public class UPFMIntegrationType : IIntegrationType
    {
        private readonly int _productId;

        private readonly DefaultUserClaim _userClaims;

        private ManageUPFMProductsIntegration _manageUPFMProductIntegration => new ManageUPFMProductsIntegration(_productId, _userClaims);

        private IUPFMProduct _upfmProductIntegration => new UPFMProductIntegration(_productId, _userClaims);

        public UPFMIntegrationType(int productId, DefaultUserClaim userClaims)
        {
            _productId = productId;
            _userClaims = userClaims;
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetRoles(editorPersonaId, userPersonaId, _userClaims.OrganizationPartyId);

        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetUPFMProperties(editorPersonaId, userPersonaId, false, dataFilter);

        public ListResponse GetEnterpriseProperties(long userPersonaId, string include) =>
            _manageUPFMProductIntegration.GetEnterpriseUPFMProperties(_userClaims.PersonaId, _productId, include);

        public ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter) =>
            _manageUPFMProductIntegration.GetRightsByRole(editorPersonaId, partyId, roleId);

        public string CreateUser(ProductUserProperitiesRoles productUser)
        {
            var productPropertiesRoles = DeserializeJSON<UPFMProductPropertyRole>(productUser.InputJson);
            return _upfmProductIntegration.CreateUser(productUser.RealPageId, productUser.CreateUserPersonaId,
               productUser.AssignUserPersonaId, productPropertiesRoles);
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
            throw new NotImplementedException();
        }
    }
}