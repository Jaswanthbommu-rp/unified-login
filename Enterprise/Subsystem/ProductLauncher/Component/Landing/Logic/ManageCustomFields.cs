using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	/// <summary>
	/// Manage CustomFields repository calls
	/// </summary>
	public class ManageCustomFields : IManageCustomFields
	{
		#region Private Variables
		ICustomFieldsRepository _customFieldsRepository;
		private DefaultUserClaim _userClaim;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageCustomFields Constructor
		/// </summary>
		/// <param name="customFieldsRepository">CustomFields Repository</param>
		/// <param name="userClaim">Information about the user</param>
		public ManageCustomFields(ICustomFieldsRepository customFieldsRepository, DefaultUserClaim userClaim)
		{
			_customFieldsRepository = customFieldsRepository;
			_userClaim = userClaim;
		}

		/// <summary>
		/// Create a basic instance of the ManageCustomFields class
		/// </summary>
		/// <param name="userClaim">Information about the user</param>
		public ManageCustomFields(DefaultUserClaim userClaim)
		{
			_customFieldsRepository = new CustomFieldsRepository();
			_userClaim = userClaim;
		}
		#endregion

		#region Public ManageCustomFields methods
		/// <summary>
		/// Add/Update Custom Fields
		/// </summary>
		/// <param name="settings">A list of one Setting object where the Value is a JSON of the Custom Fields to Add/Update</param>
		/// <param name="bookMasterId">BlackBookId MasterBook Id</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>RepositoryResponse object</returns>
		public RepositoryResponse AddUpdateCustomFields(IList<Setting> settings, long bookMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId)
		{
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.AddUpdateCustomFields", $"Organization Book MasterId: {bookMasterId}, dataImportApplicationId: {bookMasterTypeId}, customFields: {settings}" }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.AddUpdateCustomFields: Begin", _userClaim.CorrelationId, logData, null);
			if ((settings == null) || (settings.Count == 0))
			{
				throw new ArgumentNullException(nameof(settings), "Null Custom Fields.");
			}

			if (bookMasterId == 0)
			{
				throw new Exception("Missing Book Master Id.");
			}

			try
			{
				repositoryResponse = _customFieldsRepository.AddUpdateCustomFields(settings, _userClaim.UserId, bookMasterId, bookMasterTypeId);
			}
			catch (Exception exception)
			{
				logData = new Dictionary<string, object>
				{
					{ "ManageCustomFields.AddUpdateCustomFields", "Exception" }
				};
				WriteToLog(LogType.Diagnostic, "ManageCustomFields.AddUpdateCustomFields: Exception", _userClaim.CorrelationId, logData, exception);
			}

			logData = new Dictionary<string, object>
			{
				{ "Update CustomFields", settings }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.AddUpdateCustomFields: End", _userClaim.CorrelationId, logData, null);

			return repositoryResponse;
		}

		/// <summary>
		/// Add/Update Custom Fields values for a user
		/// </summary>
		/// <param name="customFieldsValuesJson">Custom Fields values</param>
		/// <param name="createdBy">Created/Modified by UserId</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse AddUpdateFieldValue(string customFieldsValuesJson, long createdBy)
		{
			if (createdBy == 0)
			{
				throw new Exception("Missing CreatedBy UserId.");
			}

			bool IsValidJson = ValidateJson.IsValidJson<IList<CustomFieldValue>>(customFieldsValuesJson);
			if ((string.IsNullOrWhiteSpace(customFieldsValuesJson)) || (!IsValidJson))
			{
				throw new ArgumentNullException(nameof(customFieldsValuesJson), "Invalid user Custom Fields Json.");
			}

			RepositoryResponse repositoryResponse = new RepositoryResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.AddUpdateFieldValue", $"customFieldsValuesJson: {customFieldsValuesJson}, createdBy: {createdBy}" }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.AddUpdateFieldValue: Begin", _userClaim.CorrelationId, logData, null);

			try
			{
				repositoryResponse = _customFieldsRepository.AddUpdateFieldValue(customFieldsValuesJson, createdBy);
			}
			catch (Exception exception)
			{
				logData = new Dictionary<string, object>
				{
					{ "ManageCustomFields.AddUpdateFieldValue ", "Exception" }
				};
				WriteToLog(LogType.Diagnostic, "ManageCustomFields.AddUpdateFieldValue: Exception", _userClaim.CorrelationId, logData, exception);
			}

			logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.AddUpdateFieldValue", customFieldsValuesJson }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.AddUpdateFieldValue: End", _userClaim.CorrelationId, logData, null);

			return repositoryResponse;
		}

		/// <summary>
		/// Get Custom Fields
		/// </summary>
		/// <param name="globals">Parameter for filter and sort</param>
		/// <param name="bookMasterId">Book MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>Custom Fields (KeyValue pairs)</returns>
		public IList<Setting> GetCustomFields(IDictionary<object, object> globals, long bookMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId)
		{
			RequestParameter dataFilter = new RequestParameter();
			IList<Setting> settingList = new List<Setting>();
			Dictionary<string, object> logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.GetCustomFields", $"Organization Book MasterId: {bookMasterId}, Book Master TypeId: {bookMasterTypeId}" }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.GetCustomFields: Begin", _userClaim.CorrelationId, logData, null);

			if (bookMasterId == 0)
			{
				throw new Exception("Missing Book Master Id.");
			}

			try
			{
				if (globals.ContainsKey(BaseType.RequestParameter))
				{
					dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
				}

				settingList = _customFieldsRepository.GetCustomFields(bookMasterId, bookMasterTypeId, dataFilter);
			}
			catch (Exception exception)
			{
				logData = new Dictionary<string, object>
				{
					{ "ManageCustomFields.GetCustomFields: Data", "Exception" }
				};
				WriteToLog(LogType.Error, "GetCustomFields: Exception", _userClaim.CorrelationId, logData, exception);
			}

			logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.GetCustomFields", settingList }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.GetCustomFields: End", _userClaim.CorrelationId, logData, null);

			return settingList;
		}

		/// <summary>
		/// Get Custom Fields
		/// </summary>
		/// <param name="globals">Parameter for filter and sort</param>
		/// <param name="bookMasterId">Book MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>List of Custom Fields objects</returns>
		public IList<CustomField> GetCustomField(IDictionary<object, object> globals, long bookMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId)
		{
			RequestParameter dataFilter = new RequestParameter();
			IList<CustomField> customFieldList = new List<CustomField>();
			Dictionary<string, object> logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.GetCustomField", $"Organization Book MasterId: {bookMasterId}, Book Master TypeId: {bookMasterTypeId}" }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.GetCustomField: Begin", _userClaim.CorrelationId, logData, null);

			if (bookMasterId == 0)
			{
				throw new Exception("Missing Book Master Id.");
			}

			try
			{
				if (globals.ContainsKey(BaseType.RequestParameter))
				{
					dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
				}

				customFieldList = _customFieldsRepository.GetCustomField(bookMasterId, bookMasterTypeId, dataFilter);
			}
			catch (Exception exception)
			{
				logData = new Dictionary<string, object>
				{
					{ "ManageCustomFields.GetCustomField", "Exception" }
				};
				WriteToLog(LogType.Error, "ManageCustomFields.GetCustomField: Exception", _userClaim.CorrelationId, logData, exception);
			}

			logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.GetCustomField: Data", customFieldList }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.GetCustomField: End", _userClaim.CorrelationId, logData, null);

			return customFieldList;
		}

		/// <summary>
		/// Get CustomField Type
		/// </summary>
		/// <param name="fieldTypeId">Optional FieldTypeId</param>
		/// <returns>List of CustomField types</returns>
		public IList<CustomFieldType> GetCustomFieldType(byte? fieldTypeId = null)
		{
			Dictionary<string, object> logData = new Dictionary<string, object>();
			IList<CustomFieldType> customFieldTypeList = new List<CustomFieldType>();

			if (fieldTypeId <= 0)
			{
				throw new Exception("Missing fieldType Id.");
			}

			try
			{
				customFieldTypeList = _customFieldsRepository.GetCustomFieldType(fieldTypeId);
			}
			catch (Exception exception)
			{
				logData = new Dictionary<string, object>
				{
					{ "ManageCustomFields.GetCustomFieldType", "Exception" }
				};
				WriteToLog(LogType.Error, "ManageCustomFields.GetCustomField: Exception", _userClaim.CorrelationId, logData, exception);
			}

			return customFieldTypeList;
		}

		/// <summary>
		/// Get Custom Fields Values for a User
		/// </summary>
		/// <param name="organizationPartyId">Unique Organization PartyId</param>
		/// <param name="userLoginPersonaId">UserLoginPersonaId</param>
		/// <param name="enabled">Enabled</param>
		/// <returns>Custom Fields Values for a User</returns>
		public IList<CustomFieldValue> GetCustomFieldsValues(long organizationPartyId, long? userLoginPersonaId = null, bool? enabled = null)
		{
			IList<CustomFieldValue> customFieldList = new List<CustomFieldValue>();
			Dictionary<string, object> logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.GetCustomFieldsValues", $"UserLoginPersonaId: {userLoginPersonaId}, Enabled: {enabled}" }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.GetCustomFieldsValues: Begin", _userClaim.CorrelationId, logData, null);

			if (organizationPartyId == 0)
			{
				throw new Exception("Missing organization partyId.");
			}

			userLoginPersonaId = (userLoginPersonaId <= 0) ? null : userLoginPersonaId;

			try
			{
				customFieldList = _customFieldsRepository.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, enabled);
			}
			catch (Exception exception)
			{
				logData = new Dictionary<string, object>
				{
					{ "ManageCustomFields.GetCustomFieldsValues", "Exception" }
				};
				WriteToLog(LogType.Error, "ManageCustomFields.GetCustomFieldsValues: Exception", _userClaim.CorrelationId, logData, exception);
			}

			logData = new Dictionary<string, object>
			{
				{ "ManageCustomFields.GetCustomFieldsValues", customFieldList }
			};
			WriteToLog(LogType.Diagnostic, "ManageCustomFields.GetCustomFieldsValues: End", _userClaim.CorrelationId, logData, null);

			return customFieldList;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Used to write to the log
		/// </summary>
		/// <param name="logType">logType</param>
		/// <param name="message">message</param>
		/// <param name="logData">logData</param>
		/// <param name="exception">exception</param>
		/// <param name="correlationId">correlationId</param>
		private void WriteToLog(LogType logType, string message, Guid correlationId, Dictionary<string, object> logData = null, Exception exception = null)
		{
			Log.Write(logType, new LogDetails
			{
				Message = message,
				AdditionalInfo = logData,
				ProductModule = this.GetType().ToString(),
				CorrelationId = correlationId.ToString(),
				Exception = exception
			});
		}
		#endregion
	}
}
