using System;
using System.Collections.Generic;
using System.ServiceModel.MsmqIntegration;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.Activity.Service.Logging.Command.Repository;
using Serilog;
using Serilog.Events;
using ActivityDetailMessage = RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models.ActivityDetailMessage;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Command
{
    public class Writer : IWriter
    {
        public void ReadMqActivity(MsmqMessage<ActivityDetailMessage> mqMessage)
        {
            ActivityDetailMessage activity = null;

            try
            {
                //Log.Information($"Starting Activity Insertion");

                activity = (ActivityDetailMessage)mqMessage.Body;

                if (activity.OrganizationPartyId == 0)
                {
                    var logger = Log.Logger;
                    var  logData = new Dictionary<string, object>() { { "ActivityDetailMessage", mqMessage.Body } };
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                    logger.Write(LogEventLevel.Error, messageTemplate: "{ActionName} - {state}", propertyValue0: "ReadMqActivity", propertyValue1: $"Activity Message with no organization party id. Message -{activity?.Message}");
                }

                var repo = new ActivityRepository();

                //To support unified login and needs to be removed once unified login replaces ToUserRealpageId with contextreferenceId
                if (string.IsNullOrEmpty(activity.ContextReferenceId) && activity.ToUserRealpageId != null
                                                                      && activity.ToUserRealpageId != Guid.Empty)
                {
                    activity.ContextReferenceId = activity.ToUserRealpageId.ToString();
                }
                repo.InsertActivity(activity);
            }
            catch (Exception ex)
            {
                Log.Error(ex, messageTemplate: "{ActionName} - {state}", propertyValue0: "ReadMqActivity", propertyValue1: $"Error. User: {activity?.FromUserLoginId.ToString()}. PmcId: {activity?.OrganizationPartyId.ToString()} Reason: {ex.Message}");
            }
        }
    }
}
