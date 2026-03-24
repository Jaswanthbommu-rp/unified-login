using System;
using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Profile Repository
	/// </summary>	
	public interface IProfileRepositoryAsync
    {
		/// <summary>
		/// Update Profile
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="profile">profile object of the parameter values</param>
		/// <returns>Repository response object</returns>
		//Task<RepositoryResponse> UpdateProfileAsync(Guid realPageId, IProfile profile);

		/// <summary>
		/// Returns a list of persons 
		/// </summary>
		/// <param name="organizationActiveProductIdList">List of product ids</param>
		/// <param name="realPageId">Organization realpage uniqueidentifier</param>
		/// <param name="parentPartyRoleTypeId">PartyRole parentId</param>
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <param name="isExport">flag to check userlist export</param>

		/// <returns>List of Person</returns>
	//	Task<IList<ProfileDetail>> ListPersonsAsync(IList<int> organizationActiveProductIdList, Guid? realPageId = null, int? parentPartyRoleTypeId = null, RequestParameter dataFilterSort = null, bool isExport = false);

		/// <summary>
		/// Returns a list of persons by ProductId
		/// </summary>
		/// <param name="productId">Single product to search by product id</param>
		/// <param name="realPageId">Optional Organization realpage uniqueidentifier</param>
		/// <param name="personaId">Optional personaId</param>
		/// <returns>List of Person</returns>
		Task<IList<ProductUsers>> ListPersonsByProductIdAsync(int productId, Guid? realPageId = null, long? personaId = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Info related to activity.
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns></returns>
	//	Task<UserActivityLogInfo> GetUserActivityLogInfoAsync(long personaId);

		/// <summary>
		/// Info related to activity.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <param name="fieldName"></param>
		/// <param name="toUserLogInfo"></param>
		/// <param name="impersonatorUserInfo"></param>
		//Task AuditActivityLogAsync(String oldValue, string newValue, string fieldName, UserActivityLogInfo toUserLogInfo, UserDetails impersonatorUserInfo);

        /// <summary>Returns the external operator relationship for a user in an org.</summary>
        Task<ExternalUserRelationship> GetExternalUserRelationshipAsync(long organizationPartyId, long userId, CancellationToken cancellationToken = default);
		Task<bool> GetOrganizationHasAnyProductAssignmentErrorAsync(long orgPartyId, CancellationToken cancellationToken = default);
    }
}