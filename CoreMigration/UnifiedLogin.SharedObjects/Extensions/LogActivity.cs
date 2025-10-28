using Serilog;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Helper;

namespace UnifiedLogin.SharedObjects.Extensions
{
    /// <summary>
    /// Log activity class moved from Audit.Core
    /// </summary>
    public static class LogActivity
    {
        /// <summary>
        /// Write activity in activity database
        /// </summary>
        public static void WriteActivity(ActivityDetails activityDetails)
        {
            try
            {
                var derivedActivityDetails = new ActivityDetailMessage(activityDetails)
                {
                    ServerName = Environment.MachineName,
                    ApplicationTimestamp = DateTime.UtcNow,
                };

                if (string.IsNullOrEmpty(ConfigReader.GetActivityMQName))
                {
                    throw new Exception($"ActivityMQName is missing check config file.");
                }

                //TODO-Migration-Core: Call activitylog write api / new activity log write api / write to kafka topic when migrated to new
                //using (var queue = new MessageQueue(ConfigReader.GetActivityMQName))
                //{
                //    var logMessage = new Message(derivedActivityDetails);
                //    queue.Send(logMessage);
                //}
            }
            catch (Exception ex)
            {
                // log exceptin in elastic
                Log.Error(ex,"Exception in Activity Logging" );
            }
        }
    }
}
