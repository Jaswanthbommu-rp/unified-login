using com.realpage.avro.unity.unifiedlogin;
using Confluent.Kafka;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.SharedObjects.Kafka
{
    /// <summary>
    /// Factory helpers for UnifiedLoginUserStatus messages.
    /// </summary>
    public class UnifiedLoginUserStatusFactory
    {
            /// <summary>
            /// Pushes ProduceUnifiedLoginUserStatus record to Kafka as a background task.
            /// </summary>
            /// <param name="loginName">The user's login name.</param>
            /// <param name="isActive">Whether the user is active.</param>
            /// <param name="activationOrDeactivationUtc">The activation or deactivation timestamp.</param>
            public static void ProduceUnifiedLoginUserStatusAsync(string loginName, bool isActive, DateTime? activationOrDeactivationUtc)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var producer = UnifiedLoginUserStatusProducer.Instance;
                        var unifiedLoginUserStatus = new UnifiedLoginUserStatus
                        {
                            is_active = isActive,
                            user_activation_deactivation_date = activationOrDeactivationUtc,
                            user_login_name = loginName
                        };

                        var deliveryResult = await producer.ProduceAsync(unifiedLoginUserStatus);

                        WriteToLog(LogEventLevel.Information, "{ActionName} - {state}",
                            new Dictionary<string, object>
                            {
                                { "KafkaDeliveryStatus", deliveryResult.Status.ToString() },
                                { "UserLoginName", loginName }
                            },
                            null,
                            new object[] { "ProduceUnifiedLoginUserStatusAsync", "Kafka message sent for user activation" });
                    }
                    catch (Exception ex)
                    {
                        WriteToLog(LogEventLevel.Error, "{ActionName} - {state}",
                            new Dictionary<string, object>
                            {
                                { "UserLoginName", loginName },
                                { "IsActive", isActive },
                                { "Exception", ex.Message }
                            },
                            ex,
                            new object[] { "ProduceUnifiedLoginUserStatusAsync", "Failed to send Kafka message for user activation" });
                    }
                });
            }



        /// <summary>
        /// ProduceUnifiedLoginUserStatusAsync
        /// </summary>
        /// <param name="userLoginRepository"></param>
        /// <param name="realPageIdList"></param>
        /// <param name="isActive"></param>
        public static void ProduceUnifiedLoginUserStatusAsync(IUserLoginLookup userLoginRepository, List<Guid> realPageIdList, bool isActive)
        {
            Task.Run(async () =>
            {
                try
                {
                    foreach (var realPageId in realPageIdList)
                    {
                        var userLoginOnly = userLoginRepository.GetUserLoginOnly(realPageId);
                        if (userLoginOnly.PrimaryOrganization && userLoginOnly.UserRoleTypeId != UserTypeConstants.RegularUserNoEmail && !userLoginOnly.IsRPEmployee)
                        {
                            var producer = UnifiedLoginUserStatusProducer.Instance;
                            var unifiedLoginUserStatus = new UnifiedLoginUserStatus
                            {
                                is_active = isActive,
                                user_activation_deactivation_date = userLoginOnly.UserDeactivationDate.HasValue ? userLoginOnly.UserDeactivationDate.Value : (DateTime?)null,
                                user_login_name = userLoginOnly.LoginName
                            };

                            var deliveryResult = await producer.ProduceAsync(unifiedLoginUserStatus);

                            WriteToLog(LogEventLevel.Information, "{ActionName} - {state}",
                                new Dictionary<string, object>
                                {
                                    { "KafkaDeliveryStatus", deliveryResult.Status.ToString() },
                                    { "UserLoginName", userLoginOnly.LoginName }
                                },
                                null,
                                new object[] { "ProduceUnifiedLoginUserStatusAsync", "Kafka message sent for user activation" });
                        }
                    }

                }
                catch (Exception ex)
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}",
                        new Dictionary<string, object>
                        {
                            { "Exception", ex.Message }
                        },
                        ex,
                        new object[] { "ProduceUnifiedLoginUserStatusAsync", "Failed to send Kafka message for user activation" });
                }
            });
        }

        private static void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            try
            {
                var logger = Log.Logger;
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }
                logger = logger.ForContext("ProductModule", typeof(UnifiedLoginUserStatusFactory));       

                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
            catch
            {
               
            }
        }
    }
}