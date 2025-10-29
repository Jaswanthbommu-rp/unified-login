using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
    public interface IManageProductOnSite
    {
     /// <summary>
        /// Unassign User
        /// </summary> 
        string UnassignUser(long editorPersonaId, long userPersonaId);

	    /// <summary>
	    /// Change user type 
	    /// </summary>
	    string ChangeOnSiteServiceUserType(long createUserPersonaId, long assignUserPersonaId, OnSiteUserPropertyRegionRole rpList, BatchProcessType batchProcessType);
		
		/// <summary>
		/// Updated to create/update a user in On Site 
		/// </summary>
		string ManageOnSiteUser(long editorPersonaId, long userPersonaId, OnSiteUserPropertyRegionRole userPropertyRegionRole,out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);

		/// <summary>
		/// Do more stuff in the product manager if needed to set up the product
		/// </summary>
		ListResponse DoAdditional(ListResponse response);

        ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetRegions(long editorPersonaId, long userPersonaId, RequestParameter datafilter);
        ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        ListResponse GetUsers(long editorPersonaId, RequestParameter datafilter);
        
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
		/// Update User Profile change 
		/// </summary>
		string UpdateOnSiteUserProfile(long editorPersonaId, long userPersonaId);

        /// <summary>
        /// Disables the On-site product user.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="productUserId">The product user identifier.</param>
        /// <param name="isDeactivate">if set to <c>false</c> [is active].</param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, string productUserId, bool isDeactivate = true);
    }
}