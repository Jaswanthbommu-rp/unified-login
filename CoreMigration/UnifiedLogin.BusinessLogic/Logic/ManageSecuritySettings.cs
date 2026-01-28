using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage SecuritySettings repository calls
    /// </summary>
    public class ManageSecuritySettings : IManageSecuritySettings
    {
        #region Private Variables
        private readonly ISecuritySettingsRepository _securitySettingsRepository;
        private readonly DefaultUserClaim _userClaim;
        private readonly ILogger _logger;
        #endregion

        #region Constructors
        /// <summary>
        /// ManageSecuritySettings Constructor with dependency injection (recommended)
        /// </summary>
        /// <param name="securitySettingsRepository">SecuritySettings Repository</param>
        /// <param name="userClaim">Information about the user</param>
        /// <param name="logger">Logger instance</param>
        public ManageSecuritySettings(
            ISecuritySettingsRepository securitySettingsRepository, 
            DefaultUserClaim userClaim,
            ILogger logger = null)
        {
            _securitySettingsRepository = securitySettingsRepository ?? throw new ArgumentNullException(nameof(securitySettingsRepository));
            _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
            _logger = logger ?? Log.Logger;
        }

        /// <summary>
        /// Create a basic instance of the ManageSecuritySettings class (legacy support)
        /// </summary>
        /// <param name="userClaim">Information about the user</param>
        public ManageSecuritySettings(DefaultUserClaim userClaim)
        {
            if (userClaim == null) throw new ArgumentNullException(nameof(userClaim));
            
            _securitySettingsRepository = new SecuritySettingsRepository();
            _userClaim = userClaim;
            _logger = Log.Logger;
        }
        #endregion

        #region Public ManageSecuritySettings methods
        /// <summary>
        /// Get Security Settings (PasswordPolicy and ActivityConfiguration)
        /// </summary>
        /// <param name="booksCustomerMasterId">Books Customer MasterId</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        /// <exception cref="ArgumentException">Thrown when booksCustomerMasterId is 0</exception>
        public IList<Setting> GetSecuritySettings(long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
        {
            ValidateGetParameters(booksCustomerMasterId);

            IList<Setting> securitySettingList = new List<Setting>();
            Guid correlationId = Guid.NewGuid();
            
            LogBeginOperation("GetSecuritySettings", correlationId, booksCustomerMasterId, bookMasterTypeId);

            try
            {
                securitySettingList = _securitySettingsRepository.GetSecuritySettings(booksCustomerMasterId, bookMasterTypeId);
            }
            catch (Exception exception)
            {
                LogException("GetSecuritySettings", correlationId, exception);
                throw; // Re-throw to preserve stack trace
            }

            LogEndOperation("GetSecuritySettings", correlationId, securitySettingList);

            return securitySettingList;
        }

        /// <summary>
        /// Update an existing Security Settings (Password and Activity Configuration Security Settings)
        /// </summary>
        /// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
        /// <param name="booksCustomerMasterId">Books Customer MasterId</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>RepositoryResponse object</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings is null</exception>
        /// <exception cref="ArgumentException">Thrown when booksCustomerMasterId is 0</exception>
        public RepositoryResponse UpdateSecuritySettings(IList<Setting> settings, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId)
        {
            ValidateUpdateParameters(settings, booksCustomerMasterId);

            RepositoryResponse repositoryResponse = new RepositoryResponse();
            Guid correlationId = Guid.NewGuid();
            
            LogBeginUpdateOperation("UpdateSecuritySettings", correlationId, booksCustomerMasterId, bookMasterTypeId, settings);

            try
            {
                repositoryResponse = _securitySettingsRepository.UpdateSecuritySettings(settings, booksCustomerMasterId, bookMasterTypeId);
            }
            catch (Exception exception)
            {
                LogException("UpdateSecuritySettings", correlationId, exception);
                throw; // Re-throw to preserve stack trace
            }

            LogEndUpdateOperation("UpdateSecuritySettings", correlationId, settings);

            return repositoryResponse;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate parameters for GetSecuritySettings
        /// </summary>
        /// <param name="booksCustomerMasterId">Books Customer Master ID</param>
        private void ValidateGetParameters(long booksCustomerMasterId)
        {
            if (booksCustomerMasterId == 0)
            {
                throw new ArgumentException("Missing Books Customer Master Id.", nameof(booksCustomerMasterId));
            }
        }

        /// <summary>
        /// Validate parameters for UpdateSecuritySettings
        /// </summary>
        /// <param name="settings">Security settings</param>
        /// <param name="booksCustomerMasterId">Books Customer Master ID</param>
        private void ValidateUpdateParameters(IList<Setting> settings, long booksCustomerMasterId)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Null Security Settings (Password and Activity Configuration Security Settings).");
            }

            if (booksCustomerMasterId == 0)
            {
                throw new ArgumentException("Missing Books Customer Master Id.", nameof(booksCustomerMasterId));
            }
        }

        /// <summary>
        /// Log begin operation for GetSecuritySettings
        /// </summary>
        private void LogBeginOperation(string actionName, Guid correlationId, long booksCustomerMasterId, int bookMasterTypeId)
        {
            var logData = new Dictionary<string, object>
            {
                { $"Get {actionName}", $"Organization Books Customer MasterId: {booksCustomerMasterId}, Book Master TypeId: {bookMasterTypeId}" }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, new object[] { actionName, "Begin" });
        }

        /// <summary>
        /// Log end operation for GetSecuritySettings
        /// </summary>
        private void LogEndOperation(string actionName, Guid correlationId, object data)
        {
            var logData = new Dictionary<string, object>
            {
                { $"Get {actionName}: Data", data }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, new object[] { actionName, "End" });
        }

        /// <summary>
        /// Log begin operation for UpdateSecuritySettings
        /// </summary>
        private void LogBeginUpdateOperation(string actionName, Guid correlationId, long booksCustomerMasterId, int bookMasterTypeId, object settings)
        {
            var logData = new Dictionary<string, object>
            {
                { $"Update {actionName}", $"Organization Book MasterId: {booksCustomerMasterId}, dataImportApplicationId: {bookMasterTypeId}, securitySettings: {settings}" }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, new object[] { actionName, "Begin" });
        }

        /// <summary>
        /// Log end operation for UpdateSecuritySettings
        /// </summary>
        private void LogEndUpdateOperation(string actionName, Guid correlationId, object settings)
        {
            var logData = new Dictionary<string, object>
            {
                { $"Update {actionName}", settings }
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, new object[] { actionName, "End" });
        }

        /// <summary>
        /// Log exception
        /// </summary>
        private void LogException(string actionName, Guid correlationId, Exception exception)
        {
            var logData = new Dictionary<string, object>
            {
                { $"{actionName}: Data", "Exception" }
            };
            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, logData, exception, new object[] { actionName, "Error" });
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="correlationId">Correlation Id</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Guid correlationId, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            var logger = _logger;
            
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