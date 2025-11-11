using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.OneSite;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageProductOneSite
    {
        ListResponse AddUpdateRole(long editorPersonaId, int roleId, string roleName, string inheritRoleId);
        string DeleteOneSiteUser(long editorPersonaId, long userPersonaId);
        string DeleteRole(long editorPersonaId, int roleId);
        string ResetVerificationCode(long editorPersonaId, long userPersonaId);
        string EnableOneSiteUser(long editorPersonaId, long userPersonaId, bool active);
        ListResponse GetOneSitePropertyList(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter);
        ListResponse GetOneSitePropertyListAll(Persona persona, RequestParameter datafilter);
        PropertyList GetOneSitePropertyListMain(Dictionary<string, string> args, RequestParameter datafilter, string uniqueIdentifier);
        ListResponse GetOneSiteRights(long editorPersonaId, RequestParameter datafilter, long roleId = 0, bool assignedToRoleOnly = false);
        ListResponse GetOneSiteRightsCenters(long editorPersonaId);
        RightList GetOneSiteRightsMain(Dictionary<string, string> args, RequestParameter datafilter, string uniqueIdentifier);
        ListResponse GetOneSiteRoleList(long editorPersonaId, long userPersonaId, bool AssignedOnly, RequestParameter datafilter);
        ListResponse GetOneSiteRoleListAll(long editorPersonaId, RequestParameter datafilter);
        RoleList GetOneSiteRoleListMain(Dictionary<string, string> args, RequestParameter datafilter, string uniqueIdentifier);
        ListResponse GetRolesForRight(long editorPersonaId, int rightId, bool assignedOnly, RequestParameter datafilter);
        ListResponse GetUsersForProperty(long editorPersonaId, int propertyId, bool assignedOnly, RequestParameter datafilter);
        ListResponse GetUsersForRole(long editorPersonaId, int roleId, bool assignedOnly, RequestParameter datafilter);
        string ManageOneSiteUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, out List<AdditionalParameters> additionalParameters, bool isUserProfileChanged = false);
        string UpdatePropertiesForUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, out List<AdditionalParameters> additionalParameters);
        string UpdateRolesForUser(long editorPersonaId, long userPersonaId, List<string> rolesToAssign, out List<AdditionalParameters> additionalParameters);
        string UpdateRightToRoles(long editorPersonaId, int rightId, List<string> roles, bool assignRight);
        string UpdateRoleToRights(long editorPersonaId, int roleId, List<string> rightsToAdd, List<string> rightsToRemove);
        string UnassignUser(long editorPersonaId, long userPersonaId, bool deleteSamlUserProductInfoAndStatus = false);
        bool UserInLeasingAgentList(long editorPersonaId, long userPersonaId, int siteId);
        PMCInfo GetPMCURL(long userPersonaId);
        OneSiteUser GetOneSiteUserInfo(string systemIdentifier);
        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);
        bool ChangeUserStatus(long editorPersonaId, string username, bool isActive = false);
        void UpdateRolesByRightLogMessage(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove);
        void UpdateRightsToRoleLogMessage(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove);
    }
}