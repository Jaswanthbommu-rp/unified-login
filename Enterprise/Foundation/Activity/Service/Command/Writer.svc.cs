using System;
using System.ServiceModel.MsmqIntegration;
using RP.Enterprise.Foundation.Activity.Service.Logging.Command.Repository;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.Audit.Core.Component.Enums;
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
                    Log.Write(LogType.Error, new LogDetails
                    {
                        Message = $"Activity Message with no organization. Message -{activity?.Message}",// accept message even orgId 0  
                        ProductModule = this.GetType().ToString(),
                    });
                }

                var repo = new ActivityRepository();
                repo.InsertActivity(activity);
            }
            catch (Exception ex)
            {
                // Log in elastic search
                Log.Write(LogType.Error, new LogDetails
                {
                    Message = $"Error in Activity Command - {ex.Message}",
                    ProductModule = this.GetType().ToString(),
                    UserId = activity?.FromUserLoginId.ToString(),
                    PmcId = activity?.BooksMasterOrganizationId.ToString(),
                    Exception = ex,
                });
            }
        }
    }
}
