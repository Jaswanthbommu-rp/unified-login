using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageCustomFields
	/// </summary>
	public interface IManageCustomFields
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
		/// <param name="globals">Parameter for filter and sort</param>
		/// <param name="partyId">partyId</param>
		/// <returns>List of Custom Fields objects</returns>
		IList<CustomField> GetCustomField(IDictionary<object, object> globals, long partyId);

		/// <summary>
		/// Get Custom Fields Values for a User
		/// </summary>
		/// <param name="organizationPartyId">Unique Organization PartyId</param>
		/// <param name="userLoginPersonaId">UserLoginPersonaId</param>
		/// <param name="enabled">Enabled</param>
		/// <returns>Custom Fields Values for a User</returns>
		IList<CustomFieldValue> GetCustomFieldsValues(long organizationPartyId, long? userLoginPersonaId = null, bool? enabled = null);
		
	}
}