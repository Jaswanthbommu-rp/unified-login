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
    public class ManageUnifiedSettings : IManageUnifiedSettings
    {
        #region Private Variables
        IUnifiedSettingsRepository _unifiedSettingsRepository;
        private DefaultUserClaim _userClaim;
        #endregion

        #region Constructors
        /// <summary>
        /// ManageSecuritySettings Constructor
        /// </summary>
        /// <param name="securitySettingsRepository">SecuritySettings Repository</param>
        /// <param name="userClaim">Information about the user</param>
        public ManageUnifiedSettings(IUnifiedSettingsRepository unifiedSettingsRepository, DefaultUserClaim userClaim)
        {
            _unifiedSettingsRepository = unifiedSettingsRepository;
            _userClaim = userClaim;
        }

        /// <summary>
        /// Create a basic instance of the ManageSecuritySettings class
        /// </summary>
        /// <param name="userClaim">Information about the user</param>
        public ManageUnifiedSettings(DefaultUserClaim userClaim)
        {
            _unifiedSettingsRepository = new UnifiedSettingsRepository();
            _userClaim = userClaim;
        }
        #endregion

        #region Public ManageSecuritySettings methods
        /// <summary>
        /// Get Company Settings
        /// </summary>
        /// <param name="category">Setting Category type</param>
        /// <param name="partyId">Company Id</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        public IList<Setting> GetUnifiedSettings(string category, long partyId)
        {
            IList<Setting> unfiedSettingList = new List<Setting>();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Get UnifiedSettings", $"Organization Id: {partyId}, Category: {category}" }
            };
            WriteToLog(LogEventLevel.Debug, "GetUnifiedSettings: Begin", correlationId, logData, null);

            if (partyId == 0)
            {
                throw new Exception("Missing Organization Id.");
            }

            try
            {
                unfiedSettingList = _unifiedSettingsRepository.GetUnifiedSettings(partyId, category);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Get Unified Settings: Data", "Exception" }
                };
                WriteToLog(LogEventLevel.Error, "unfiedSettingList: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Get UnifiedSettings: Data", unfiedSettingList }
            };
            WriteToLog(LogEventLevel.Debug, "unfiedSettingList: End", correlationId, logData, null);

            return unfiedSettingList;
        }

        /// <summary>
        /// Update an existing unified Settings 
        /// </summary>
        /// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
        /// <param name="booksCustomerMasterId">Books Customer MasterId</param>
        /// <param name="partyId">Company Id</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateUnifiedSettings(IList<Setting> settings, string category, long partyId, string[] includes)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            Guid correlationId = Guid.NewGuid();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "Update SecuritySettings", $"Organization Id: {partyId}, category: {category}, Settings: {settings}" }
            };
            WriteToLog(LogEventLevel.Debug, "UpdateUnifiedSettings: Begin", correlationId, logData, null);
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Null  Settings ");
            }

            try
            {
                repositoryResponse = _unifiedSettingsRepository.UpdateUnifiedSettings(settings, partyId, category, _userClaim.UserId);
            }
            catch (Exception exception)
            {
                logData = new Dictionary<string, object>
                {
                    { "Update UnifiedSettings", "Exception" }
                };
                WriteToLog(LogEventLevel.Debug, "UpdateUnifiedSettings: Exception", correlationId, logData, exception);
            }

            logData = new Dictionary<string, object>
            {
                { "Update UnifiedSettings", settings }
            };
            WriteToLog(LogEventLevel.Debug, "UpdateUnifiedSettings: End", correlationId, logData, null);

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
            logger.Write(logType, exception, message);
        }
        #endregion
    }
}
