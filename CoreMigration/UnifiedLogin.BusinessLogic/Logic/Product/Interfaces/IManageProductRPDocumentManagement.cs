using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
	public interface IManageProductRPDocumentManagement
	{
		/// <summary>
		/// Used to get roles for a company or user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="roleId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetRoleClassifierDataset(long editorPersonaId, long userPersonaId, string roleId, RequestParameter datafilter);

		/// <summary>
		/// Updated to create/update a user
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId"></param>
		/// <param name="rolePropertyEntityList">The role, property or department to assign the user</param>
		/// <param name="additionalParameters"></param>
		/// <returns></returns>
		string ManageRPDMUser(long editorPersonaId, long userPersonaId, RolePropertyList rolePropertyEntityList, out List<AdditionalParameters> additionalParameters);

        /// <summary>
        /// Used to unassign a user from the product
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        ///  <param name="productUserId"></param>
        /// <returns></returns>
        string UnassignUser(long editorPersonaId, long userPersonaId , int productUserId =0);

		/// <summary>
		/// Used to get the domain for a user persona
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns>The domain of the company</returns>
		ListResponse GetDomain(long personaId);

		/// <summary>
		/// Used to update the user profile
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		string UpdateRPDMUserProfile(long editorPersonaId, long userPersonaId);

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
	}
}