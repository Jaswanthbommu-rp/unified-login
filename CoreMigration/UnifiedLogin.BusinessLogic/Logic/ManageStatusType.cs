using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Landing;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage StatusType repository calls
    /// </summary>
    public class ManageStatusType : IManageStatusType
	{
		#region Private Variables
		IStatusTypeRepository _statusTypeRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageSecuritySettings Constructor
		/// </summary>
		/// <param name="statusTypeRepository">StatusType Repository</param>
		public ManageStatusType(IStatusTypeRepository statusTypeRepository)
		{
			_statusTypeRepository = statusTypeRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageUser Controller class
		/// </summary>
		public ManageStatusType()
		{
			_statusTypeRepository = new StatusTypeRepository();
		}
		#endregion

		#region Public ManageStatusType methods
		/// <summary>
		/// List StatusTypes
		/// </summary>
		/// <param name="CategoryTypeName">Category TypeName (e.g. Status)</param>
		/// <param name="CategoryName">Category Name (e.g. User Status)</param>
		/// <returns>List of StatusType objects</returns>
		public IList<StatusType> GetStatusType(string CategoryTypeName, string CategoryName)
		{
			IList<StatusType> statusTypeList = new List<StatusType>();
			Guid correlationId = Guid.NewGuid();
			Dictionary<string, object> logData = new Dictionary<string, object>
			{
				{ "Get StatusType", $"Category TypeName: {CategoryTypeName}, Category name: {CategoryTypeName}"}
			};
			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "GetStatusType", "Begin" });

            if (string.IsNullOrWhiteSpace(CategoryTypeName))
			{
				throw new Exception("Invalid Category TypeName.");
			}

			if (string.IsNullOrWhiteSpace(CategoryName))
			{
				throw new Exception("Invalid Category Name.");
			}

			try
			{
				statusTypeList = _statusTypeRepository.GetStatusType(CategoryTypeName, CategoryName);
			}
			catch (Exception exception)
			{
				logData = new Dictionary<string, object>
				{
					{ "Get StatusType", "Exception" }
				};
				WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", correlationId, logData, exception, messageProperties: new object[] { "GetStatusType", "Exception" });
            }
			logData = new Dictionary<string, object>
			{
				{ "Get StatusType", statusTypeList }
			};
			WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", correlationId, logData, null, messageProperties: new object[] { "GetStatusType", "End" });

            statusTypeList.ToList().Find(s => s.Name.Equals("Disabled", StringComparison.OrdinalIgnoreCase)).Name = "Deactivated";

			return statusTypeList;
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
