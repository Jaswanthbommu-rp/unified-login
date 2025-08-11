using System.Collections.Generic;
using System.Data;
using Dapper;
using RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models;
using RP.Enterprise.Foundation.DataAccess.Component.Model;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Command.Repository
{
    internal class ActivityRepository : BaseRepository
    {
        /// <summary>
        /// Insert Activity
        /// </summary> 
        public void InsertActivity(ActivityDetailMessage activityLog)
        {
            var tvp = new TableValueParmInfo
            {
                StoredProcedureName = "[Logging].[InsertActivity]",
                TableParamTypeName = "ADDITIONALINFO",
                TableVariableName = "AdditionalInformationTPV",
                OrderedColumnName = new List<string> { "Key", "Value" }
            };

            var dparm = new DynamicParameters();

            dparm.Add("LogTypeId", activityLog.LogActivityTypeId, DbType.Int32);
            dparm.Add("LogType", activityLog.LogActivityTypeName, DbType.String);
            dparm.Add("LogCategoryType", activityLog.LogCategoryName, DbType.String);
            dparm.Add("Message", activityLog.Message, DbType.String);
            dparm.Add("FromUserLoginName", activityLog.FromUserLoginName, DbType.String);
            dparm.Add("FromUserFirstName", activityLog.FromUserFirstName, DbType.String);
            dparm.Add("FromUserLastName", activityLog.FromUserLastName, DbType.String);
            dparm.Add("FromUserRealpageId", activityLog.FromUserRealpageId, DbType.Guid);
            dparm.Add("OrganizationPartyId", activityLog.OrganizationPartyId, DbType.Int64);
            dparm.Add("Timestamp", activityLog.ApplicationTimestamp, DbType.DateTime);
            dparm.Add("ContextId", activityLog.ContextId, DbType.String);
            dparm.Add("ContextReferenceId", activityLog.ContextReferenceId, DbType.String);
            dparm.Add("IsRealPageEmployee", activityLog.IsRealPageEmployee, DbType.Boolean);
            dparm.Add("ApplicationCorrelationId", activityLog.ApplicationCorrelationId, DbType.String);
            dparm.Add("ActivityId", 0, DbType.Int64, ParameterDirection.Output);

            using (var repository = GetRepository())
            {
                repository.ExecuteStoredProcWithTvp<AdditionalParameters>(tvp, activityLog.AdditionalInformation,
                    dparm);
            }
        }
    }

}