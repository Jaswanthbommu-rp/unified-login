using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageCustomFields
	/// </summary>
	public interface IManageCustomFields
	{
		/// <summary>
		/// Add/Update Custom Fields
		/// </summary>
		/// <param name="settings">A list of one Setting object where the Value is a JSON of the Custom Fields to Add/Update</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse AddUpdateCustomFields(IList<Setting> settings, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId);

		/// <summary>
		/// Add/Update Custom Fields
		/// </summary>
		/// <param name="settings">A list of one Setting object where the Value is a JSON of the Custom Fields to Add/Update</param>
		/// <param name="partyId">Books Customer MasterId</param>
		/// <param name="operation">add/update/delete</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse AddUpdateDeleteCustomFields(IList<Setting> settings, long partyId, string operation = "update");

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
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>Custom Fields (KeyValue pairs)</returns>
		IList<Setting> GetCustomFields(IDictionary<object, object> globals, long partyId);

		/// <summary>
		/// Get Custom Fields
		/// </summary>
		/// <param name="globals">Parameter for filter and sort</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>List of Custom Fields objects</returns>
		IList<CustomField> GetCustomField(IDictionary<object, object> globals, long booksCustomerMasterId);

		/// <summary>
		/// Get Custom Fields Values for a User
		/// </summary>
		/// <param name="organizationPartyId">Unique Organization PartyId</param>
		/// <param name="userLoginPersonaId">UserLoginPersonaId</param>
		/// <param name="enabled">Enabled</param>
		/// <returns>Custom Fields Values for a User</returns>
		IList<CustomFieldValue> GetCustomFieldsValues(long organizationPartyId, long? userLoginPersonaId = null, bool? enabled = null);

		/// <summary>
		/// Get CustomField Type
		/// </summary>
		/// <param name="fieldTypeId">Optional FieldTypeId</param>
		/// <returns>List of CustomField types</returns>
		IList<CustomFieldType> GetCustomFieldType(byte? fieldTypeId = null);
	}
}