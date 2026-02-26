using Confluent.Kafka;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using Serilog;
using System;
using System.Messaging;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions
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
                Log.Information("Activity details 22 before sending to queue: {ActivityDetails}", JsonConvert.SerializeObject(activityDetails));
                var derivedActivityDetails = new ActivityDetailMessage(activityDetails)
                {
                    ServerName = Environment.MachineName,
                    ApplicationTimestamp = DateTime.UtcNow,
                };

                if (string.IsNullOrEmpty(ConfigReader.GetActivityMQName))
                {
                    throw new Exception($"ActivityMQName is missing check config file.");
                }
                Log.Information("Activity details 33 before sending to queue: {ActivityDetails}", JsonConvert.SerializeObject(activityDetails));
                Log.Information("Activity details 34 before sending to queue: {ActivityDetails}", JsonConvert.SerializeObject(derivedActivityDetails));
                using (var queue = new MessageQueue(ConfigReader.GetActivityMQName))
                {
                    var logMessage = new Message(derivedActivityDetails);
                    Log.Information("Activity details 38 before sending to queue: {ActivityDetails}", JsonConvert.SerializeObject(activityDetails));
                    queue.Send(logMessage);
                    Log.Information("Activity details 41 after sendinf before sending to queue: {ActivityDetails}", JsonConvert.SerializeObject(activityDetails));
                }
            }
            catch (Exception ex)
            {
                Log.Information("Activity details exception  46 before sending to queue: {ActivityDetails}", JsonConvert.SerializeObject(ex));
                // log exceptin in elastic
                Log.Error(ex, "Exception in Activity Logging");
            }
        }
    }
}
