using com.realpage.avro.unity.unifiedlogin;
using Confluent.Kafka;
using Confluent.SchemaRegistry.Serdes;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Kafka;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    /// <summary>
    /// Kafka producer for publishing user status change events with Avro serialization
    /// </summary>
    public class UserStatusKafkaProducer : KafkaProducerBase, IKafkaProducerService
    {
        //private IProducer<string, UnifiedLoginUserStatus> _producer;

        /// <summary>
        /// Initializes the user status Kafka producer
        /// </summary>
        public UserStatusKafkaProducer() : base()
        {
        }

         /// <summary>
        /// Flushes and disposes the typed producer
        /// </summary>
        protected override void DisposeProducer()
        {
            if (_producer != null)
            {
                _producer.Flush(TimeSpan.FromSeconds(30));
                _producer.Dispose();
            }
        }

        /// <summary>
        /// Publishes a user status change event to Kafka using Avro serialization
        /// </summary>
        /// <param name="userStatusEvent">User status change event data</param>
        /// <returns>Task indicating completion</returns>
        public async Task PublishUserStatusChangeEventAsync(UnifiedLoginUserStatusEvent userStatusEvent)
        {
            var topicName = KafkaConfiguration.UserStatusTopicName;

            if (userStatusEvent == null)
            {
                throw new ArgumentNullException(nameof(userStatusEvent));
            }

            if (_producer == null)
            {
                throw new InvalidOperationException("Kafka producer is not initialized. Use the constructor with configuration parameters.");
            }

            var logger = Log.Logger.ForContext("ProductModule", this.GetType());

            try
            {
                // Convert DTO to Avro schema model              
                var avroMessage = new UnifiedLoginUserStatus
                {
                    persona_id = userStatusEvent.persona_id,
                    user_login_name = userStatusEvent.user_login_name,
                    is_active = userStatusEvent.is_active,
                    user_activation_deactivation_date = userStatusEvent.user_activation_deactivation_date
                };

                var message = new Message<string, UnifiedLoginUserStatus>
                {
                    Key = userStatusEvent.persona_id?.ToString() ?? Guid.NewGuid().ToString(),
                    Value = avroMessage
                };

                // On-prem: attach the Avro schema inline as a message header
                // so consumers can deserialize without Schema Registry
                if (_isOnPrem)
                {
                    message.Headers = new Headers
                    {
                        { "avro.schema", AvroSchemaBytes }
                    };
                }

                var logData = new Dictionary<string, object>
                {
                    { "Topic", topicName },
                    { "Username", avroMessage.user_login_name },
                    { "IsActive", avroMessage.is_active },
                    { "PersonaId", avroMessage.persona_id },
                    { "ActivationDate", avroMessage.user_activation_deactivation_date }
                };

                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                logger.Write(LogEventLevel.Information, "{ActionName} - {state}",
                    propertyValue0: "PublishUserStatusChangeEventAsync",
                    propertyValue1: $"Publishing Avro user status change event for user {avroMessage.user_login_name}");

                // Use DeliveryHandler for better async performance (fire-and-forget with callback)
                var deliveryResult = await _producer.ProduceAsync(topicName, message).ConfigureAwait(false);

                logger.Write(LogEventLevel.Information, "{ActionName} - {state}",
                    propertyValue0: "PublishUserStatusChangeEventAsync",
                    propertyValue1: $"Successfully published Avro event to partition {deliveryResult.Partition.Value} at offset {deliveryResult.Offset.Value}");
            }
            catch (ProduceException<string, UnifiedLoginUserStatus> ex)
            {
                logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}",
                    propertyValue0: "PublishUserStatusChangeEventAsync",
                    propertyValue1: $"Failed to publish user status change event for user {userStatusEvent.user_login_name}. Error: {ex.Error.Reason}");

                // Log but don't throw - we don't want to fail the user status change operation if Kafka is unavailable
            }
            catch (Exception ex)
            {
                logger.Write(LogEventLevel.Error, ex, "{ActionName} - {state}",
                    propertyValue0: "PublishUserStatusChangeEventAsync",
                    propertyValue1: $"Unexpected error publishing user status change event for user {userStatusEvent.user_login_name}");
            }
        }
    }
}
