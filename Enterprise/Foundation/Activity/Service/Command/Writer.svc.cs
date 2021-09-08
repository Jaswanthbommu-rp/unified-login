using System;
using System.Collections.Generic;
using System.ServiceModel.MsmqIntegration;
using RP.Enterprise.Foundation.Activity.Service.Logging.Command.Repository;
using Serilog;
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
                    var  logData = new Dictionary<string, object>() { { "ActivityDetailMessage", mqMessage.Body } };
                    Log.Error( $"Activity Message with no organization party id. Message -{activity?.Message}", logData);
                }

                var repo = new ActivityRepository();
                if (string.IsNullOrEmpty(activity.ContextReferenceId) && activity.ToUserRealpageId != null
                                                                      && activity.ToUserRealpageId != Guid.Empty)
                {
                    activity.ContextReferenceId = activity.ToUserRealpageId.ToString();
                }
                repo.InsertActivity(activity);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in Activity Command - {ex.Message}. User: {activity?.FromUserLoginId.ToString()}. PmcId: {activity?.OrganizationPartyId.ToString()}");
            }
        }
    }
}
