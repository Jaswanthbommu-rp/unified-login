using com.realpage.avro.unity.unifiedlogin;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Kafka;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    /// <summary>
    /// Kafka producer service for publishing user events with Avro serialization
    /// </summary>
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly string _topicName;
        private readonly string _bootstrapServers;
        private readonly string _schemaRegistryUrl;
        private readonly IProducer<string, UnifiedLoginUserStatusEvent> _producer;

        /// <summary>
        /// Constructor with configuration
        /// </summary>
        /// <param name="bootstrapServers">Kafka broker addresses</param>
        /// <param name="topicName">Kafka topic name</param>
        /// <param name="schemaRegistryUrl">Confluent Schema Registry URL</param>
        public KafkaProducerService(string bootstrapServers, string topicName, string schemaRegistryUrl)
        {
            _bootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
            _topicName = topicName ?? "unified-login-user-status-events";
            _schemaRegistryUrl = schemaRegistryUrl ?? throw new ArgumentNullException(nameof(schemaRegistryUrl));

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                ClientId = "UnifiedLogin-Producer",
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5,
                MessageSendMaxRetries = 3
            };

            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = _schemaRegistryUrl
            };

            var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

            _producer = new ProducerBuilder<string, UnifiedLoginUserStatusEvent>(producerConfig)
                .SetValueSerializer(new AvroSerializer<UnifiedLoginUserStatusEvent>(schemaRegistry))
                .Build();
        }

        /// <summary>
        /// Default constructor for testing
        /// </summary>
        public KafkaProducerService()
        {
            // Default constructor for mocking/testing
        }

        /// <summary>
        /// Publishes a user status change event to Kafka using Avro serialization
        /// </summary>
        /// <param name="userStatusEvent">User status change event data</param>
        /// <returns>Task indicating completion</returns>
        public async Task PublishUserStatusChangeEventAsync(UnifiedLoginUserStatusEvent userStatusEvent)
        {
            if (userStatusEvent == null)
            {
                throw new ArgumentNullException(nameof(userStatusEvent));
            }

            var logger = Log.Logger.ForContext("ProductModule", this.GetType());

            try
            {
                // Convert to Avro schema model
                var avroMessage = new UnifiedLoginUserStatus
                {
                    personaId = userStatusEvent.personaId,
                    user_login_name = userStatusEvent.user_login_name,
                    is_active = userStatusEvent.is_active,
                    user_activation_deactivation_date = userStatusEvent.user_activation_deactivation_date
                };

                var message = new Message<string, UnifiedLoginUserStatus>
                {
                    Key = userStatusEvent.personaId.ToString(),
                    Value = avroMessage
                    //Timestamp = userStatusEvent.user_activation_deactivation_date.HasValue
                    //            ? new Timestamp(new DateTime(1970, 1, 1).AddDays(userStatusEvent.user_activation_deactivation_date.Value))
                    //            : Timestamp.Default
                };

                var logData = new Dictionary<string, object>
                {
                    { "Topic", _topicName },
                    { "PersonaId", avroMessage.personaId },
                    { "Username", avroMessage.user_login_name },
                    { "IsActive", avroMessage.is_active },
                    { "ActivationDate", avroMessage.user_activation_deactivation_date }
                };

                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                logger.Write(LogEventLevel.Information, "{ActionName} - {state}",
                    propertyValue0: "PublishUserStatusChangeEventAsync",
                    propertyValue1: $"Publishing Avro user status change event for user {avroMessage.user_login_name}");

                var deliveryResult = await _producer.ProduceAsync(_topicName, message);

                logger.Write(LogEventLevel.Information, "{ActionName} - {state}",
                    propertyValue0: "PublishUserStatusChangeEventAsync",
                    propertyValue1: $"Successfully published Avro event to partition {deliveryResult.Partition.Value} at offset {deliveryResult.Offset.Value}");
            }
            catch (ProduceException<string, UnifiedLoginUserStatusEvent> ex)
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

        /// <summary>
        /// Converts DateTime to days since Unix epoch (1970-01-01) as required by Avro date logical type
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Days since Unix epoch</returns>
        private int? ConvertToDateInt(DateTime dateTime)
        {
            if (dateTime == default(DateTime))
            {
                return null;
            }

            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcDateTime = dateTime.ToUniversalTime();
            return (int)(utcDateTime.Date - unixEpoch.Date).TotalDays;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
