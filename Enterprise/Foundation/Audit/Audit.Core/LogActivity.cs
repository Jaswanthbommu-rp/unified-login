using System;
using System.Messaging;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;

namespace RP.Enterprise.Foundation.Audit.Core.Component
{
    /// <summary>
    /// Log activity
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

                if (string.IsNullOrEmpty(ConfigReader.ActivityMQName))
                {
                    throw new Exception($"ActivityMQName is missing check config file.");
                }

                using (var queue = new MessageQueue(ConfigReader.ActivityMQName))
                {
                    var logMessage = new Message(derivedActivityDetails);
                    queue.Send(logMessage);
                }
            }
            catch (Exception ex)
            {
                // log exceptin in elastic
                Log.Write(LogType.Error, new LogDetails { Exception = ex, Message = "Exception in Activity Logging" });
            }
        }
    }


}
