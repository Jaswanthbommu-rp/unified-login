using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.RentersInsurance;

namespace UnifiedLogin.BusinessLogic.Logic.Product.Interfaces
{
	/// <summary>
	/// Interface for ManageProductRentersInsurance
	/// </summary>
	public interface IManageProductRentersInsurance
	{
		/// <summary>
		/// Disable User in Renters Insurance
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>ObjectOutput object</returns>
		ObjectOutput<UserAPIResponse, IErrorData> DisableRentersInsuranceUser(long editorPersonaId, long userPersonaId);

        /// <summary>
        /// Enable User in Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Error object</returns>
        ObjectOutput<UserAPIResponse, IErrorData> EnableRentersInsuranceUser(long editorPersonaId, long userPersonaId);

        /// <summary>
        /// Used to list properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        /// <returns>ListResponse object</returns>
        ListResponse ListProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter);

		/// <summary>
		/// List Level
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Levels list</returns>
		IList<ProductRole> ListRoles(long editorPersonaId, long userPersonaId);

        ObjectOutput<UserAPIResponse, IErrorData> ChangeRentersInsuranceUserType(long createUserPersonaId, long assignUserPersonaId, RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList, BatchProcessType batchProcessType);

        /// <summary>
        /// Add or update a Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="rentersInsuranceRoleAndPropertyList">Used to grant a user level, set the Messaging groups, and Is the Product assigned or removed for the user.</param>
		/// <param name="batchProcessType"></param>
		/// <param name="additionalParameters"></param>
        /// <returns>ObjectOuput and Error</returns>
        ObjectOutput<UserAPIResponse, IErrorData> ManageRentersInsuranceUser(long editorPersonaId, long userPersonaId, RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);

        /// <summary>
        /// Unassign User in GreenBook and disable in Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>ObjectOutput object</returns>
        ObjectOutput<UserAPIResponse, IErrorData> UnassignRentersInsuranceUser(long editorPersonaId, long userPersonaId);

		/// <summary>
		/// Disable User in Renters Insurance
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>ObjectOutput object</returns>
		ObjectOutput<UserAPIResponse, IErrorData> UnlockRentersInsuranceUser(long editorPersonaId, long userPersonaId);

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
        /// Disable Remters Insurance product user.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        bool ChangeUserStatus(long editorPersonaId, int userId, bool isActive = false);
    }
}