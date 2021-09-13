using System;
using System.Collections.Generic;
using System.Messaging;
using RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models;

namespace ActivityTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var addi = new List<AdditionalParameters>
            {
                new AdditionalParameters {Key = "Client",Value= "Important"}
            };

            SendObject(new ActivityDetailMessage
            {
                FromUserLoginName = "abc@test.com",
                FromUserLoginId = 2222,
                Message = "Test Msg dumped to MQ.",
                ApplicationTimestamp = DateTime.Now.Date,
                LogActivityTypeName = "Security",
                AdditionalInformation = addi,
            }, @".\private$\greenbook_activity");
        }

        static void SendObject(ActivityDetailMessage activityDetails, string queueName)
        {
            using (var queue = new MessageQueue(queueName))
            {
                var logMessage = new Message(activityDetails);
                queue.Send(logMessage);
            }
        }
    }
}
