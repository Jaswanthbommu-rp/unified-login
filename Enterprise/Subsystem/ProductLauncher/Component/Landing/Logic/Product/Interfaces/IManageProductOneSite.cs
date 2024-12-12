using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces
{
    /// <summary>
    /// Used to test ManageProductOneSite
    /// </summary>
    public interface IManageProductOneSite
    {
        ListResponse AddUpdateRole(long editorPersonaId, int roleId, string roleName, string inheritRoleId);

		/// <summary>
		/// Used to delete a OneSite user
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>String.empty if success else error</returns>
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
        string ManageOneSiteUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, bool isUserProfileChanged = false);
        string UpdatePropertiesForUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign);
        string UpdateRolesForUser(long editorPersonaId, long userPersonaId, List<string> rolesToAssign);
        string UpdateRightToRoles(long editorPersonaId, int rightId, List<string> roles, bool assignRight);
        string UpdateRoleToRights(long editorPersonaId, int roleId, List<string> rightsToAdd, List<string> rightsToRemove);

        /// <summary>
        /// Unassign User
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="deleteSamlUserProductInfoAndStatus">Optional paramter: Delete all SAML product information and status for the OneSite user when changing the usertype from Admin to Regular user</param>
        /// <returns>String.empty if success else error</returns>
        string UnassignUser(long editorPersonaId, long userPersonaId, bool deleteSamlUserProductInfoAndStatus = false);

		bool UserInLeasingAgentList(long editorPersonaId, long userPersonaId, int siteId);
        PMCInfo GetPMCURL(long userPersonaId);
        OneSiteUser GetOneSiteUserInfo(string systemIdentifier);
	    MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);

		/// <summary>
		/// Get List of One Site Users for Migration 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Changes the user status.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="username">The username.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string username, bool isActive = false);
        void UpdateRolesByRightLogMessage(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove);
        void UpdateRightsToRoleLogMessage(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove);

    }
}