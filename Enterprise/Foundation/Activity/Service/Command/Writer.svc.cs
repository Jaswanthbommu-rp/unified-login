using System;
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
                activity = (ActivityDetailMessage)mqMessage.Body;

                if (activity.BooksMasterOrganizationId == 0)
                {
                    Log.Error( $"Activity Message with no organization. Message -{activity?.Message}");
                }

                var repo = new ActivityRepository();
                repo.InsertActivity(activity);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in Activity Command - {ex.Message}. User: {activity?.FromUserLoginId.ToString()}. PmcId: {activity?.BooksMasterOrganizationId.ToString()}");
            }
        }
    }
}
