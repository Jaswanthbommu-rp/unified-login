using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
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
        /// <param name="bookMasterId">Book MasterId</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        public IList<Setting> GetSecuritySettings(long bookMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId)
        {
            IList<Setting> securitySettingList = new List<Setting>();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Get SecuritySettings", $"Organization Book MasterId: {bookMasterId}, Book Master TypeId: {bookMasterTypeId}" }
            };
            WriteToLog(LogType.Diagnostic, "GetSecuritySettings: Begin", correlationId, logData, null);

            if (bookMasterId == 0)
            {
                throw new Exception("Missing Book Master Id.");
            }

            try
            {
                securitySettingList = _securitySettingsRepository.GetSecuritySettings(bookMasterId, bookMasterTypeId);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Get SecuritySettings: Data", "Exception" }
                };
                WriteToLog(LogType.Error, "GetSecuritySettings: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Get SecuritySettings: Data", securitySettingList }
            };
            WriteToLog(LogType.Diagnostic, "GetSecuritySettings: End", correlationId, logData, null);

            return securitySettingList;
        }

        /// <summary>
        /// Update an existing Security Settings (Password and Activity Configuration Security Settings)
        /// </summary>
        /// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
        /// <param name="bookMasterId">BlackBookId MasterBook Id</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateSecuritySettings(IList<Setting> settings, long bookMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Update SecuritySettings", $"Organization Book MasterId: {bookMasterId}, dataImportApplicationId: {bookMasterTypeId}, securitySettings: {settings}" }
            };
            WriteToLog(LogType.Diagnostic, "UpdateSecuritySettings: Begin", correlationId, logData, null);
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Null Security Settings (Password and Activity Configuration Security Settings).");
            }

            try
            {
                repositoryResponse = _securitySettingsRepository.UpdateSecuritySettings(settings, bookMasterId, bookMasterTypeId);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Update SecuritySettings", "Exception" }
                };
                WriteToLog(LogType.Diagnostic, "UpdateSecuritySettings: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Update SecuritySettings", settings }
            };
            WriteToLog(LogType.Diagnostic, "UpdateSecuritySettings: End", correlationId, logData, null);

            return repositoryResponse;
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
        private void WriteToLog(LogEventLevel logType, string message, Guid correlationId, Dictionary<string, object> logData = null, Exception exception = null)
        {
            LogDetails logDetails = new LogDetails
            {
                Message = message,
                AdditionalInfo = logData,
                ProductModule = this.GetType().ToString(),
                CorrelationId = correlationId.ToString(),
                Exception = exception
            };

            Log.Write(logType, exception, message, logDetails);
        }
        #endregion
    }
}