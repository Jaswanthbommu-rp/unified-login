using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageProductLead2Lease
    {
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        string ManageLead2LeaseUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, out List<AdditionalParameters> additionalParameters);
        string UnassignUser(long editorPersonaId, long userPersonaId);

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
        bool ChangeUserStatus(long editorPersonaId, string username, string productUserId, bool isActive = false);

        /// <summary>
        /// Updates user profile
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        string UpdateLead2LeaseUserProfile(long editorPersonaId, long userPersonaId);
    }
}