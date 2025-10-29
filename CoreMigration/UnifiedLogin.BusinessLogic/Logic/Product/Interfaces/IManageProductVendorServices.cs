using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.VendorServices;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageProductVendorServices
    {
        /// <summary>
        /// Get Property Groups
        /// </summary> 
        ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter dataFilter);

        /// <summary>
        /// Used to get properties  
        /// </summary> 
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter dataFilter);

        /// <summary>
        /// Returns Roles (User Access Groups in Vendor Credentialing)
        /// </summary>
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, AccessType accessType, RequestParameter dataFilter);

        /// <summary>
        /// Change user type 
        /// </summary>
        string ChangeVendorServiceUserType(long createUserPersonaId, long assignUserPersonaId, UserProductPropertyNotification rpList, BatchProcessType batchProcessType);

        /// <summary>
        /// Updated to create/update a user in Vendor Credentialing 
        /// </summary>
        string ManageVendorServicesUser(long editorPersonaId, long userPersonaId, UserProductPropertyNotification userProductPropertyNotificationBatchProcessType, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);

        /// <summary>
        /// Unassign User
        /// </summary>
        string UnassignUser(long createUserPersonaId, long assignUserPersonaId);

        /// <summary>
        /// Get Notification Settings 
        /// </summary>
        Notification GetNotificationSettings(long editorPersonaId, long userPersonaId);


        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter dataFilter);

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
    }
}