using UnifiedLogin.SharedObjects.Enterprise;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Repository.Enterprise
{
	/// <summary>
	/// Interface for Enterprise User Repository
	/// </summary>
	 public interface IEntUserRepository
	{
		/// <summary>
		/// Create Enterprise User
		/// </summary>
		/// <param name="userProductDetails">User Product Details</param>
		/// <returns></returns>
		string CreateEnterpriseUser(UserProductDetails userProductDetails);

		/// <summary>
		/// Get/List Users
		/// </summary>
		/// <param name="organizationPartyId">Company PartyId</param>
		/// <param name="productIdList">List of product ids</param>
		/// <param name="statusTypeId">Status Type Id</param>
		/// <param name="realPageId">Optional User EnterpriseId</param>
		/// <param name="name">Optional filter by FirstName, LastName, or UserName</param>
		/// <param name="rowsPerPage">Optional Rows Per page to return</param>
		/// <param name="pageNumber">Optional PageNumber</param>
		/// <returns>List of Users (List of 1 if getting a user)</returns>
		IList<UsersData> ListUsers(long organizationPartyId, IList<int> productIdList, int statusTypeId, Guid? realPageId = null, string name = null, int rowsPerPage = 0, int pageNumber = 1);
	}
}