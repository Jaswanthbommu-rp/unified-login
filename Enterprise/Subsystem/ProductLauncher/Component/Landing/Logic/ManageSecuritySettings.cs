using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
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
        /// <param name="booksCustomerMasterId">Books Customer MasterId</param>
        /// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        public IList<Setting> GetSecuritySettings(long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId)
        {
            IList<Setting> securitySettingList = new List<Setting>();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Get SecuritySettings", $"Organization Books Customer MasterId: {booksCustomerMasterId}, Book Master TypeId: {bookMasterTypeId}" }
            };
            WriteToLog(LogEventLevel.Debug, "GetSecuritySettings: Begin", correlationId, logData, null);

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
                WriteToLog(LogEventLevel.Error, "GetSecuritySettings: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Get SecuritySettings: Data", securitySettingList }
            };
            WriteToLog(LogEventLevel.Debug, "GetSecuritySettings: End", correlationId, logData, null);

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
            WriteToLog(LogEventLevel.Debug, "UpdateSecuritySettings: Begin", correlationId, logData, null);
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
                WriteToLog(LogEventLevel.Debug, "UpdateSecuritySettings: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Update SecuritySettings", settings }
            };
            WriteToLog(LogEventLevel.Debug, "UpdateSecuritySettings: End", correlationId, logData, null);

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
            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
			logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", correlationId.ToString());
            logger.Write(logType, exception, message );
        }
        #endregion
    }
}