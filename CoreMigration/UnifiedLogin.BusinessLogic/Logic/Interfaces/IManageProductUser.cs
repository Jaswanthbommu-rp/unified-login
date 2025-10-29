using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manages Product User
	/// </summary>
	public interface IManageProductUser
    {
		/// <summary>
		/// Change Product User Type from admin to regular or vice versa
		/// </summary>
		/// <param name="batchRecord">Product batch details for a user</param>
		/// <returns>String.empty if success else error</returns>
		string ChangeUserType(ProductUserProperitiesRoles batchRecord);

		/// <summary>
		/// Creates Product User
		/// </summary> 
		/// <param name="productUser">Product details for a user</param>
		/// <returns>String.empty if success else error</returns>
		string CreateProductUser(ProductUserProperitiesRoles productUser);

        /// <summary>
		/// Creates an employee product user
		/// </summary>
		/// <param name="productUser"></param>
		/// <returns></returns>
        string CreateEmployeeProductUser(ProductUserProperitiesRoles productUser);

		/// <summary>
		/// Used to delete all SAML product information and status for a user
		/// </summary>
		/// <param name="productUserAccountDetails">product User Account Details</param>
		/// <param name="internalChange">Tells if it is called internally.</param>
		/// <returns>String.empty if success else error</returns>
		string DeleteSamlUserProductInfoAndStatus(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false);

		/// <summary>
		/// Returns List of Product Batch Statuses
		/// </summary>
		/// <param name="realPageId">User Enterprise Id</param>
		/// <param name="assignUserId">Assigned User PersonaId</param>
		/// <returns>List of ProductBatchStatus</returns>
		IList<ProductBatchStatus> GetProductStatuses(Guid realPageId, long assignUserId);

		/// <summary>
		/// Update product details for a user
		/// </summary> 
		/// <param name="productUserAccountDetails">Product User Account Details</param>
		/// <param name="internalChange">Tells if it has been called internally</param>
		/// <returns>String.empty if success else error</returns>
		string UpdateProductUserAccountDetails(ProductUserAccountDetails productUserAccountDetails, bool internalChange = false);

		/// <summary>
		/// Update Product User Profile
		/// </summary>
		/// <param name="productUser">Product details for a user</param>
		/// <returns>String.empty if success else error</returns>
		string UpdateProductUserProfile(ProductUserProperitiesRoles productUser);

		/// <summary>
		/// Creates Enterprise Role Product User
		/// </summary> 
		/// <param name="productUser">Product details for a user</param>
		/// <returns>String.empty if success else error</returns>
		string CreateEnterpriseRoleProductUser(ProductUserProperitiesRoles productUser);
	}
}