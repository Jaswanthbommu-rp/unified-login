using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageProductMarketingCenter
    {
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        string ManageMarketingCenterUser(long editorPersonaId, long userPersonaId, List<int> RoleList, List<string> PropertyList, bool IsAssignedNewPropertyByDefault, out List<AdditionalParameters> additionalParameters);
        string UnassignUser(long createUserPersonaId, long assignUserPersonaId);
        ListResponse GetRolesCount(long editorPersonaId);
        ListResponse GetRights(long editorPersonaId);
        ListResponse GetRightsForRoleId(long editorPersonaId, int roleId);

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Update the users migration status
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="migrateUsers"></param>
        /// <returns></returns>
        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="username"></param>
        /// <param name="productUserId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string username, string productUserId, bool isActive);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>

        /// <param name="roleId"></param>
        /// <returns></returns>
        ListResponse DeleteRole(long editorPersonaId, int roleId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId"></param>
        /// <param name="Isactive"></param>
        /// <returns></returns>
        ListResponse UpdateRoleStatus(long editorPersonaId, int roleId, bool Isactive);
    }
}