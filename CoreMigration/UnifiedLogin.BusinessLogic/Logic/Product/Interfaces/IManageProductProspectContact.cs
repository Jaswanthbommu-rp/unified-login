using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ProspectContactCenter;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageProductProspectContact
    {
        /// <summary>
        /// Used to get properties  
        /// </summary> 
        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

        /// <summary>
        /// Unassign User
        /// </summary>
        string UnassignUser(long createUserPersonaId, long assignUserPersonaId);

        /// <summary>
        /// Change user type
        /// </summary>
        string ChangeProspectContactUserType(long createUserPersonaId, long assignUserPersonaId, ProspectContactPropertyRole roleProp, BatchProcessType batchProcessType, out List<AdditionalParameters> additionalParameters);

        /// <summary>
        /// Create/update a user in Product Prospect Contact Center
        /// </summary>
        string ManageProductProspectContactUser(long editorPersonaId, long userPersonaId, ProspectContactPropertyRole userProductPropertyNotification, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);

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
        /// Update Prospect Contact Center User Profile
        /// </summary>
        string UpdateProspectContactCenterUserProfile(long userPersonaId, long editorPersonaId);

        /// <summary>
        /// Changes the user status.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, int userId);
    }
}