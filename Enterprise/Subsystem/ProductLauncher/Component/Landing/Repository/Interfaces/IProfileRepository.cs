using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	/// <summary>
	/// Interface for Profile Repository
	/// </summary>	
	public interface IProfileRepository
    {
		/// <summary>
		/// Update Profile
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="profile">profile object of the parameter values</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse UpdateProfile(Guid realPageId, IProfile profile);

		/// <summary>
		/// Returns a list of persons 
		/// </summary>
		/// <param name="organizationActiveProductIdList">List of product ids</param>
		/// <param name="realPageId">Organization realpage uniqueidentifier</param>
		/// <param name="parentPartyRoleTypeId">PartyRole parentId</param>
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <param name="isExport">flag to check userlist export</param>

		/// <returns>List of Person</returns>
		IList<ProfileDetail> ListPersons(IList<int> organizationActiveProductIdList, Guid? realPageId = null, int? parentPartyRoleTypeId = null, RequestParameter dataFilterSort = null, bool isExport = false);

		/// <summary>
		/// Returns a list of persons by ProductId
		/// </summary>
		/// <param name="productId">Single product to search by product id</param>
		/// <param name="realPageId">Optional Organization realpage uniqueidentifier</param>
		/// <param name="personaId">Optional personaId</param>
		/// <returns>List of Person</returns>
		IList<ProductUsers> ListPersonsByProductId(int productId, Guid? realPageId = null, long? personaId = null);
		bool GetOrganizationHasAnyProductAssignmentError(long orgPartyId);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="organizationPartyId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
        ExternalUserRelationship GetExternalUserRelationship(long organizationPartyId, long userId);

		/// <summary>
		/// Info related to activity.
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns></returns>
		UserActivityLogInfo GetUserActivityLogInfo(long personaId);

        /// <summary>
        /// Info related to activity.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="fieldName"></param>
        /// <param name="toUserLogInfo"></param>
        /// <param name="impersonatorUserInfo"></param>
        void AuditActivityLog(String oldValue, string newValue, string fieldName, UserActivityLogInfo toUserLogInfo, UserDetails impersonatorUserInfo);

    }
}