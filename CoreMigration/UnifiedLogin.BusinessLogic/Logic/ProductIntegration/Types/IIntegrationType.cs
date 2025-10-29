using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Types
{
    public interface IIntegrationType
    {
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter);

        ListResponse GetEnterpriseProperties(long userPersonaId, RequestParameter dataFilter);

        ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter, string userLoginName = "");

        ListResponse GetPropertiesByGroup(long editorPersonaId, long userPersonaId, string propertyGroupId, RequestParameter dataFilter);

        ListResponse GetOrganizations(long editorPersonaId, long userPersonaId, string organizationRoleId, string organizationType);

        ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, AccessType? accessType, RequestParameter dataFilter);

        ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter);
        ListResponse GetRightsForRole(long editorPersonaId, long userPersonaId, string roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter);

        ListResponse GetAllRights(long editorPersonaId, long userPersonaId, RequestParameter dataFilter);

        string CreateUser(ProductUserProperitiesRoles productUser, out List<AdditionalParameters> additionalParameters);

        string ChangeUserType(ProductUserProperitiesRoles batchRecord);

        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter dataFilter);

        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);

        bool ExternalUserProfileChange(long editorPersonaId, ProductUserProfile productUserProfile);

        string UpdateUserProfile(ProductUserProperitiesRoles productUser);
        
        string UpdateUserDetails(ProductUserAccountDetails productUserAccountDetails, bool internalchange = false);

        ListResponse GetUserGroups(long editorPersonaId, long userPersonaId, long partyId, RequestParameter dataFilter);
    }
}