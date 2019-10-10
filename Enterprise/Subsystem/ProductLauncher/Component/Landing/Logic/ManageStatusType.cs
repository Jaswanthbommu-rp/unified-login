using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
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
			WriteToLog(LogType.Diagnostic, "GetStatusType: Begin", correlationId, logData, null);

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
				WriteToLog(LogType.Diagnostic, "GetStatusType: Exception", correlationId, logData, exception);
			}
			logData = new Dictionary<string, object>
			{
				{ "Get StatusType", statusTypeList }
			};
			WriteToLog(LogType.Diagnostic, "GetStatusType: End", correlationId, logData, null);

            statusTypeList.ToList().Find(s => s.Name.Equals("Disabled", StringComparison.OrdinalIgnoreCase)).Name = "Deactivated";

			return statusTypeList;
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
