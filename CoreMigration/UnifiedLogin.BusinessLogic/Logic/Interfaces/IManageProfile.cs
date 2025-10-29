using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage Profile repository calls
	/// </summary>
	public interface IManageProfile
    {
        /// <summary>
        /// Get Profile Detail for a person
        /// </summary>
        /// <param name="realPageId"></param>
        /// <param name="orgPartyId"></param>
        /// <param name="roleTypeFrom"></param>
        /// <param name="roleTypeTo"></param>
        /// <param name="relationshipType"></param>
        /// <param name="contactMechanismUsageTypeName"></param>
        /// <returns></returns>
        IProfileDetail GetProfileDetail(Guid realPageId, long orgPartyId, string roleTypeFrom = null, string roleTypeTo = null, string relationshipType = null, string contactMechanismUsageTypeName = null);

        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">Profile object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        RepositoryResponse UpdateProfile(Guid realPageId, IProfile profile);

        /// <summary>
        /// Get a list of persons 
        /// </summary>
        /// <param name="globals">Parameter for filter and sort</param>
        /// <param name="organizationRealPageId">Organization's realPageId</param>
        /// <returns>List of Persons</returns>
        IList<ProfileDetail> ListProfileDetails(IDictionary<object, object> globals, Guid? organizationRealPageId = null);

        /// <summary>
        /// Returns a list of persons by ProductId
        /// </summary>
        /// <param name="productId">Single product to search by product id</param>
        /// <param name="organizationRealPageId">Optional Organization realpage uniqueidentifier</param>
        /// <param name="personaId">Optional personaId</param>
        /// <returns>List of Person</returns>
        IList<ProductUsers> ListPersonsByProductId(int productId, Guid? organizationRealPageId = null, long? personaId = null);
        bool GetOrganizationHasProductAssignmentError(long orgPartyId);
	}
}