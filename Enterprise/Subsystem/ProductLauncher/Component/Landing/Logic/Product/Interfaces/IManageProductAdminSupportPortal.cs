using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.AdminSupportPortal;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces
{
    public interface IManageProductAdminSupportPortal
    {
        /// <summary>
        /// Returns properties 
        /// </summary> 
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Returns Roles  
        /// </summary>
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Updated to create/update a user  
        /// </summary>
        string ManageAdminSupportPortalUser(long editorPersonaId, long userPersonaId, AdminSupportPortalPropertyRole adminSupportPortalPropertyRole, out List<AdditionalParameters> additionalParameters);
        /// <summary>
        /// Returns list of users 
        /// </summary>
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter);
        /// <summary>
        /// Update list of users 
        /// </summary>
        MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers);
        /// <summary>
        /// Update User Profile change 
        /// </summary>
        string UpdateAdminSupportPortalUserProfile(long editorPersonaId, long userPersonaId);

        /// <summary>
        /// Disable or Enable Client Portal product user
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="productUserId">The product user identifier.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string productUserId, bool isActive = false);
    }
}
