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
            dparm.Add("ActivityId", 0, DbType.Int64, ParameterDirection.Output);
            dparm.Add("LogType", activityLog.LogActivityTypeName, DbType.String);
            dparm.Add("LogCategoryType", activityLog.LogCategoryName, DbType.String);
            dparm.Add("CorrelationId", activityLog.CorrelationId, DbType.String);

            dparm.Add("FromUserLoginName", activityLog.FromUserLoginName, DbType.String);
            dparm.Add("FromUserFirstName", activityLog.FromUserFirstName, DbType.String);
            dparm.Add("FromUserLastName", activityLog.FromUserLastName, DbType.String);
            dparm.Add("FromUserLoginId", activityLog.FromUserLoginId, DbType.Int64);
            dparm.Add("FromUserRealpageId", activityLog.FromUserRealpageId, DbType.Guid);

            dparm.Add("ToUserLoginName", activityLog.ToUserLoginName, DbType.String);
            dparm.Add("ToUserLoginId", activityLog.ToUserLoginId, DbType.Int64);
            dparm.Add("ToUserFirstName", activityLog.ToUserFirstName, DbType.String);
            dparm.Add("ToUserLastName", activityLog.ToUserLastName, DbType.String);
            dparm.Add("ToUserRealpageId", activityLog.ToUserRealpageId, DbType.Guid);
            
            dparm.Add("OrganizationPartyId", activityLog.OrganizationPartyId, DbType.Int64);
            dparm.Add("BooksMasterOrganizationId", activityLog.BooksMasterOrganizationId, DbType.Int64);
            dparm.Add("BooksProductCode", activityLog.BooksProductCode, DbType.String);

            dparm.Add("Message", activityLog.Message, DbType.String);
             
            dparm.Add("ServerName", activityLog.ServerName, DbType.String);
            dparm.Add("Timestamp", activityLog.ApplicationTimestamp, DbType.DateTime);

            dparm.Add("BooksMasterPropertyId", activityLog.BooksMasterPropertyId, DbType.Int32);
            dparm.Add("IsSystemAdminActivity", activityLog.IsSystemAdminActivity, DbType.Boolean);

            //Used for Settings
            dparm.Add("SourceId", activityLog.SourceId, DbType.String);
            dparm.Add("MappingKey", activityLog.MappingKey, DbType.String);
            dparm.Add("ContextId", activityLog.ContextId, DbType.Int32);
            dparm.Add("InstanceId", activityLog.InstanceId, DbType.String);

            using (var repository = GetRepository())
            {
                repository.ExecuteStoredProcWithTvp<AdditionalParameters>(tvp, activityLog.AdditionalInformation,
                    dparm);
            }
        }
    }

}