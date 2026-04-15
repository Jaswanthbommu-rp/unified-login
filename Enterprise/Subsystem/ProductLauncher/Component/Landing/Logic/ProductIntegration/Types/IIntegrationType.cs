using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types
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

        ListResponse GetProductUserData(long editorPersonaId, int productId, RequestParameter dataFilter);
    }
}