using com.realpage.avro.unity.unifiedlogin;
using Confluent.Kafka;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.SharedObjects.Kafka
{
    /// <summary>
    /// Factory helpers for UnifiedLoginUserStatus messages.
    /// </summary>
    public static class UnifiedLoginUserStatusFactory
    {
            /// <summary>
            /// Pushes ProduceUnifiedLoginUserStatus record to Kafka as a background task.
            /// </summary>
            /// <param name="loginName">The user's login name.</param>
            /// <param name="isActive">Whether the user is active.</param>
            /// <param name="activationOrDeactivationUtc">The activation or deactivation timestamp.</param>
            public static void ProduceUnifiedLoginUserStatusAsync(string loginName, bool isActive, DateTime activationOrDeactivationUtc)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        using (var producer = new UnifiedLoginUserStatusProducer())
                        {
                            var unifiedLoginUserStatus = new UnifiedLoginUserStatus
                            {
                                is_active = isActive,
                                user_activation_deactivation_date = activationOrDeactivationUtc,
                                user_login_name = loginName
                            };

                            var deliveryResult = await producer.ProduceAsync(unifiedLoginUserStatus);

                            //WriteToLog(LogEventLevel.Information, "{ActionName} - {state}",
                            //    new Dictionary<string, object>
                            //    {
                            //        { "KafkaDeliveryStatus", deliveryResult.Status.ToString() },
                            //        { "UserLoginName", loginName }
                            //    },
                            //    null,
                            //    new object[] { "ProduceUnifiedLoginUserStatusAsync", "Kafka message sent for user activation" });
                        }
                    }
                    catch (Exception ex)
                    {
                        //WriteToLog(LogEventLevel.Error, "{ActionName} - {state}",
                        //    new Dictionary<string, object>
                        //    {
                        //        { "UserLoginName", loginName },
                        //        { "IsActive", isActive }
                        //    },
                        //    ex,
                        //    new object[] { "ProduceUnifiedLoginUserStatusAsync", "Failed to send Kafka message for user activation" });
                    }
                });
            }
    }
}