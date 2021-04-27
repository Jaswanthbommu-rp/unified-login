using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
{
    public class StandardV1IntegrationType : IIntegrationType
    {
        private readonly int _productId;

        private readonly DefaultUserClaim _userClaims;

        public StandardV1IntegrationType(int productId, DefaultUserClaim userClaims)
        {
            _productId = productId;
            _userClaims = userClaims;
        }

        public string ChangeUserType(ProductUserProperitiesRoles batchRecord)
        {
            var rpList = DeserializeJSON<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var productIntegration = new StandardV1ProductIntegration(_productId, batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, _userClaims);

            return productIntegration.ChangeProductUserType(rpList, batchRecord.BatchProcessType);
        }

        public string CreateUser(ProductUserProperitiesRoles productUser)
        {
            var rpList = DeserializeJSON<ProductUserRolePropertiesGroups>(productUser.InputJson);
            if (rpList == null)
            {
                return "Input JSON parsing issue; Null object.";
            }

            var productIntegration = new StandardV1ProductIntegration(_productId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, _userClaims);

            if (rpList.IsAssigned)
            {
                return productIntegration.CreateUpdateProductUser(rpList);
            }

            return productIntegration.UnassignUser();
        }

        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productIntegration.GetProductProperties(dataFilter);
        }

        public ListResponse GetEnterpriseProperties(long userPersonaId, string include)
        {
            throw new NotImplementedException();
        }

        public ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productIntegration.GetProductRightsForRole(dataFilter, roleId);
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productIntegration.GetProductRoles(dataFilter);
        }

        public ListResponse GetAllRights(long editorPersonaId, long userPersonaId, RequestParameter dataFilter)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productIntegration.GetAllRights(dataFilter);
        }

        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter dataFilter, string userLoginName = "")
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productIntegration.GetAllRights(dataFilter);
        }

        public ListResponse GetPropertiesByGroup(long editorPersonaId, long userPersonaId, string propertyGroupId, RequestParameter dataFilter)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productIntegration.GetProductPropertiesByGroup(propertyGroupId, dataFilter);
        }

        public ListResponse GetOrganizations(long editorPersonaId, long userPersonaId, string organizationRoleId, string organizationType)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, userPersonaId, _userClaims);
            return productIntegration.GetProductOrganizations(organizationRoleId, organizationType);
        }

        public ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter dataFilter)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, editorPersonaId, _userClaims);
            return productIntegration.GetMigrationUsers(dataFilter);
        }

        public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, editorPersonaId, _userClaims);
            return productIntegration.UpdateUsersMigrationStatus(migrateUsers);
        }

        public bool ExternalUserProfileChange(long editorPersonaId, ProductUserProfile productUserProfile)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, editorPersonaId, editorPersonaId, _userClaims);
            return productIntegration.ExternalProductUserProfileChange(productUserProfile);
        }

        public string UpdateUserProfile(ProductUserProperitiesRoles productUser)
        {
            var productIntegration = new StandardV1ProductIntegration(_productId, productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, _userClaims);
            return productIntegration.UpdateProductUserProfile();
        }

        private T DeserializeJSON<T>(string productUserInputJson)
        {
            if (string.IsNullOrEmpty(productUserInputJson))
                return default(T);

            try
            {
                return JsonConvert.DeserializeObject<T>(productUserInputJson.Trim());
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}