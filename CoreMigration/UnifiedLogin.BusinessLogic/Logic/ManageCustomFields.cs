using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.BusinessLogic.Logic
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
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "AddUpdateFieldValue", "Begin" });

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
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", _userClaim.CorrelationId, logData, exception, messageProperties: new object[] { "AddUpdateFieldValue", "Exception" });
            }

            logData = new Dictionary<string, object>
            {
                { "ManageCustomFields.AddUpdateFieldValue", customFieldsValuesJson }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "AddUpdateFieldValue", "End" });

            return repositoryResponse;
        }
                
        /// <summary>
        /// Get Custom Fields
        /// </summary>
        /// <param name="globals">Parameter for filter and sort</param>
        /// <param name="booksCustomerMasterId">Books Customer MasterId</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>List of Custom Fields objects</returns>
        public IList<CustomField> GetCustomField(IDictionary<object, object> globals, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
        {
            RequestParameter dataFilter = new RequestParameter();
            IList<CustomField> customFieldList = new List<CustomField>();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "ManageCustomFields.GetCustomField", $"Organization Book MasterId: {booksCustomerMasterId}, Book Master TypeId: {bookMasterTypeId}" }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "GetCustomField", "End" });

            if (booksCustomerMasterId == 0)
            {
                throw new Exception("Missing Book Master Id.");
            }

            try
            {
                if (globals.ContainsKey(BaseType.RequestParameter))
                {
                    dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
                }

                // customFieldList = _customFieldsRepository.GetCustomField(booksCustomerMasterId, bookMasterTypeId, dataFilter);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "ManageCustomFields.GetCustomField", "Exception" }
                };
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", _userClaim.CorrelationId, logData, exception, messageProperties: new object[] { "GetCustomField", "Exception" });
            }

            logData = new Dictionary<string, object>
            {
                { "ManageCustomFields.GetCustomField: Data", customFieldList }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "GetCustomField", "End" });

            return customFieldList;
        }
        /// <summary>
        /// Get Custom Fields
        /// </summary>
        /// <param name="globals">Parameter for filter and sort</param>
        /// <param name="partyId">org partyId</param>        
        /// <returns>List of Custom Fields objects</returns>
        public IList<CustomField> GetCustomField(IDictionary<object, object> globals, long partyId)
        {
            RequestParameter dataFilter = new RequestParameter();
            IList<CustomField> customFieldList = new List<CustomField>();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "ManageCustomFields.GetCustomField", $"Organization partyId: {partyId}" }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "GetCustomField", "Begin" });

            if (partyId == 0)
            {
                throw new Exception("Missing Organization PartyId.");
            }

            try
            {
                if (globals.ContainsKey(BaseType.RequestParameter))
                {
                    dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
                }

                customFieldList = _customFieldsRepository.GetCustomField(partyId, dataFilter);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "ManageCustomFields.GetCustomField", "Exception" }
                };
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", _userClaim.CorrelationId, logData, exception, messageProperties: new object[] { "GetCustomField", "Exception" });
            }

            logData = new Dictionary<string, object>
            {
                { "ManageCustomFields.GetCustomField: Data", customFieldList }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "GetCustomField", "End" });

            return customFieldList;
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
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "GetCustomFieldsValues", "Begin" });

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
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", _userClaim.CorrelationId, logData, exception, messageProperties: new object[] { "GetCustomFieldsValues", "Exception" });
            }

            logData = new Dictionary<string, object>
            {
                { "ManageCustomFields.GetCustomFieldsValues", customFieldList }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", _userClaim.CorrelationId, logData, null, messageProperties: new object[] { "GetCustomFieldsValues", "End" });

            return customFieldList;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        /// <param name="correlationId">Correlation Id</param>
        private void WriteToLog(LogEventLevel logType, string message, Guid correlationId, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
			logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId.ToString());

            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }

        #endregion
    }
}
