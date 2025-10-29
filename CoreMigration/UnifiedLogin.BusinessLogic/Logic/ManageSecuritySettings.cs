using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage SecuritySettings repository calls
    /// </summary>
    public class ManageSecuritySettings : IManageSecuritySettings
    {
        #region Private Variables
        ISecuritySettingsRepository _securitySettingsRepository;
        private DefaultUserClaim _userClaim;
        #endregion

        #region Constructors
        /// <summary>
        /// ManageSecuritySettings Constructor
        /// </summary>
        /// <param name="securitySettingsRepository">SecuritySettings Repository</param>
        /// <param name="userClaim">Information about the user</param>
        public ManageSecuritySettings(ISecuritySettingsRepository securitySettingsRepository, DefaultUserClaim userClaim)
        {
            _securitySettingsRepository = securitySettingsRepository;
            _userClaim = userClaim;
        }

        /// <summary>
        /// Create a basic instance of the ManageSecuritySettings class
        /// </summary>
        /// <param name="userClaim">Information about the user</param>
        public ManageSecuritySettings(DefaultUserClaim userClaim)
        {
            _securitySettingsRepository = new SecuritySettingsRepository();
            _userClaim = userClaim;
        }
        #endregion

        #region Public ManageSecuritySettings methods
        /// <summary>
        /// Get Security Settings (PasswordPolicy and ActivityConfiguration)
        /// </summary>
        /// <param name="booksCustomerMasterId">Books Customer MasterId</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        public IList<Setting> GetSecuritySettings(long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
        {
            IList<Setting> securitySettingList = new List<Setting>();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Get SecuritySettings", $"Organization Books Customer MasterId: {booksCustomerMasterId}, Book Master TypeId: {bookMasterTypeId}" }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "GetSecuritySettings", "Begin" });

            if (booksCustomerMasterId == 0)
            {
                throw new Exception("Missing Books Customer Master Id.");
            }

            try
            {
                securitySettingList = _securitySettingsRepository.GetSecuritySettings(booksCustomerMasterId, bookMasterTypeId);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Get SecuritySettings: Data", "Exception" }
                };
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, logData, exception, messageProperties: new object[] { "GetSecuritySettings", "Error" });
            }

            logData = new Dictionary<string, object>
            {
                { "Get SecuritySettings: Data", securitySettingList }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "GetSecuritySettings", "End" });

            return securitySettingList;
        }

        /// <summary>
        /// Update an existing Security Settings (Password and Activity Configuration Security Settings)
        /// </summary>
        /// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
        /// <param name="booksCustomerMasterId">Books Customer MasterId</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateSecuritySettings(IList<Setting> settings, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Update SecuritySettings", $"Organization Book MasterId: {booksCustomerMasterId}, dataImportApplicationId: {bookMasterTypeId}, securitySettings: {settings}" }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "UpdateSecuritySettings", "Begin" });
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Null Security Settings (Password and Activity Configuration Security Settings).");
            }

            try
            {
                repositoryResponse = _securitySettingsRepository.UpdateSecuritySettings(settings, booksCustomerMasterId, bookMasterTypeId);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Update SecuritySettings", "Exception" }
                };
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, logData, exception, messageProperties: new object[] { "UpdateSecuritySettings", "Error" });
            }

            logData = new Dictionary<string, object>
            {
                { "Update SecuritySettings", settings }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "UpdateSecuritySettings", "End" });

            return repositoryResponse;
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