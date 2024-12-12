using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	public interface IManageUnifiedLogin
    {
        /// <summary>
        /// Returns Roles (Roles in GB)
        /// </summary>
        ListResponse GetRoles(long editorPersonaId,  long partyId);

        /// <summary>
        /// Returns Roles With Count (Roles in GB)
        /// </summary>
        ListResponse GetRolesWithCount(long editorPersonaId, long partyId);

        /// <summary>
        /// Returns Rights (Rights in GB)
        /// </summary>
        ListResponse GetRights(long editorPersonaId, long partyId);

        /// <summary>
        /// Returns Rights With Count  (Rights in GB)
        /// </summary>
        ListResponse GetRightsWithCount(long editorPersonaId, long partyId);

        /// <summary>
        /// Returns Roles for User (Roles in GB)
        /// </summary>
        ListResponse GetUserRoles(long editorPersonaId, long userPersonaId, long partyId);

        /// <summary>
        /// Returns Rights by role (Rights in GB)
        /// </summary>
        ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId);

        /// <summary>
        /// Returns ALL Rights by role (Rights in GB)
        /// </summary>
        ListResponse GetAllRightsByRole(long editorPersonaId, long partyId, long roleId);

        /// <summary>
        /// Returns Roles by right (Roles in GB)
        /// </summary>
        ListResponse GetRolesByRight(long editorPersonaId, long partyId, long rightId);

        /// <summary>
        /// Add/Update Role(Custom Role in GB)
        /// </summary>
        ListResponse AddUpdateRole(long editorPersonaId, long partyId, long roleId, string roleName, string inheritRoleId);

        /// <summary>
        /// Delete Role(Custom Role in GB)
        /// </summary>
        ListResponse DeleteRole(long editorPersonaId, long roleId);

        /// <summary>
        /// Set Default Role(Custom/System Role in GB)
        /// </summary>
        ListResponse SetDefaultRole(long editorPersonaId, long partyId, long roleId);
        
        /// <summary>
        /// Add/Delete Rights
        /// </summary>
        ListResponse UpdateRightsToRole(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove);

        /// <summary>
        /// Add/Delete Roles
        /// </summary>
        ListResponse UpdateRolesByRight(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove);
        
        /// <summary>
        /// Clone Rights
        /// </summary>
        ListResponse CloneRightsToRole(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove);

        /// <summary>
        /// Returns Roles with assigned rights for User (User Access Groups in UserManagement)
        /// </summary>
        ListResponse GetUserRolesWithRights(long editorPersonaId, long userPersonaId, long partyId);

        /// <summary>
        /// Used to get the list of properties for the company or for the given user
        /// </summary>
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter);

        ListResponse GetEnterpriseProperties(long userPersonaId, string include = null);

        ListResponse GetUPFMProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, ProductEnum product, RequestParameter datafilter);

        ListResponse GetListRightbyRole(string productCode, int roleId);
        void DeleteRoleLogMessage(long editorPersonaId, long roleId, string roleName, string product);
        void AddUpdateRoleLogMessage(long editorPersonaId, long partyId, string roleName, string action, string product, string oldRoleName = null);
    }
}