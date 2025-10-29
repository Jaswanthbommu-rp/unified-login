using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Interface for CustomFieldsRepository
	/// </summary>
	public interface ICustomFieldsRepository
	{
		/// <summary>
		/// Add/Update Custom Fields values for a user
		/// </summary>
		/// <param name="customFieldsValuesJson">Custom Fields values</param>
		/// <param name="createdBy">Created/Modified by UserId</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse AddUpdateFieldValue(string customFieldsValuesJson, long createdBy);

		/// <summary>
		/// Get Custom Fields
		/// </summary>
		/// <param name="partyId">org party Id</param>		
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <returns>Custom Fields (KeyValue pairs)</returns>
		IList<Setting> GetCustomFields(long partyId, RequestParameter dataFilterSort = null);

		/// <summary>
		/// Get Custom Fields
		/// </summary>
		/// <param name="partyId">org party id</param>		
		/// <param name="dataFilterSort">Data Filtering and Sorting</param>
		/// <returns>List of Custom Fields objects</returns>
		IList<CustomField> GetCustomField(long partyId, RequestParameter dataFilterSort = null);

		/// <summary>
		/// Get Custom Fields Values for a User
		/// </summary>
		/// <param name="organizationPartyId">Unique Organization PartyId</param>
		/// <param name="userLoginPersonaId">userLoginPersonaId</param>
		/// <param name="enabled">Enabled</param>
		/// <returns>Custom Fields Values for a User</returns>
		IList<CustomFieldValue> GetCustomFieldsValues(long organizationPartyId, long? userLoginPersonaId = null, bool? enabled = null);
	}
}