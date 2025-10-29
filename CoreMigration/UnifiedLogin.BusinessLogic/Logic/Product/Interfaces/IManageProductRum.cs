using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Rum;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageProductRum
    {
        /// <summary>
        /// Unassign User
        /// </summary> 
        string UnassignRumUser(long editorPersonaId, long userPersonaId);

        /// <summary>
        /// Updated to create/update a user in On Site 
        /// </summary>
        string ManageRumUser(long editorPersonaId, long userPersonaId, RumUserPropertyRegionRole userPropertyRegionRole, out List<AdditionalParameters> additionalParameters);

        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetRegions(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Update the user migration status
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="migrateUsers"></param>
        /// <returns></returns>
        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);

        /// <summary>
        /// Disables RUM product user.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="productUserId">The product user identifier.</param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string productUserId);
    }
}
