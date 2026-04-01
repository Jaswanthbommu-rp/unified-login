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
using System.Configuration;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    /// <summary>
    /// Kafka producer service for publishing user events with Avro serialization
    /// </summary>
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private string _topicName;
        private readonly string _bootstrapServers;
        private readonly string _schemaRegistryUrl;
        private readonly IProducer<string, UnifiedLoginUserStatus> _producer;
        private readonly object _lock = new object();
        private bool _disposed = false;


        public KafkaProducerService()
        {
           
            _schemaRegistryUrl = KafkaConfiguration.SchemaRegistryUrl;

            // Initialize producer immediately for singleton use
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = KafkaConfiguration.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MaxInFlight = 5,
                MessageSendMaxRetries = 3,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = KafkaConfiguration.SaslUsername,
                SaslPassword = KafkaConfiguration.SaslPassword,
                EnableSslCertificateVerification = true
                //SaslUsername = ConfigurationManager.AppSettings["Kafka:SaslUsername"] ?? "JKWMTFLNEA5NCY4J",
                //SaslPassword = ConfigurationManager.AppSettings["Kafka:SaslPassword"] ?? "+zafbYYkj5B9cceN62TfK7BHWpVAdNHMedKEEmLCTNuCe5RSRyMvYukvja/+QXx+",
                //EnableSslCertificateVerification = true,
                //// Performance tuning for high throughput
                //LingerMs = 10, // Batch messages for up to 10ms
                //BatchSize = 16384, // 16KB batch size
                //CompressionType = CompressionType.Snappy, // Fast compression
                //QueueBufferingMaxMessages = 100000,
                //QueueBufferingMaxKbytes = 1048576 // 1GB buffer
            };

            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = _schemaRegistryUrl,
                BasicAuthUserInfo = KafkaConfiguration.SchemaRegistryBasicAuthUserInfo
            };
            if (KafkaConfiguration.OnPrem.HasValue && KafkaConfiguration.OnPrem.Value)
            {
                producerConfig.SecurityProtocol = SecurityProtocol.Ssl;
                producerConfig.SaslUsername = null;
                producerConfig.SaslPassword = null;
                schemaRegistryConfig.BasicAuthUserInfo = null;
                producerConfig.SslCaCertificateStores = KafkaConfiguration.SslCaCertificateStores;
                schemaRegistryConfig.EnableSslCertificateVerification = false;

                // Configure SSL for Schema Registry client (uses HttpClient, not librdkafka)
                //if (!string.IsNullOrEmpty(KafkaConfiguration.SslCaLocation))
                //{
                //    schemaRegistryConfig.SslCaLocation = KafkaConfiguration.SslCaLocation;
                //}
                //else
                //{
                //    // On-prem Schema Registry uses internal CA; disable verification when no CA cert file is provided
                //    schemaRegistryConfig.EnableSslCertificateVerification = false;
                //}
            }

            var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

            _producer = new ProducerBuilder<string, UnifiedLoginUserStatus>(producerConfig)
                .SetValueSerializer(new AvroSerializer<UnifiedLoginUserStatus>(schemaRegistry))
                .SetErrorHandler((_, e) => 
                {
                    Log.Logger.Error("Kafka producer error: {Reason}", e.Reason);
                })
                .Build();
        }


        /// <summary>
        /// Publishes a user status change event to Kafka using Avro serialization
        /// </summary>
        /// <param name="userStatusEvent">User status change event data</param>
        /// <returns>Task indicating completion</returns>
        public async Task PublishUserStatusChangeEventAsync(UnifiedLoginUserStatusEvent userStatusEvent)
        {
            _topicName = KafkaConfiguration.UserStatusTopicName;
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

                var logData = new Dictionary<string, object>
                {
                    { "Topic", _topicName },
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
                var deliveryResult = await _producer.ProduceAsync(_topicName, message).ConfigureAwait(false);

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

        /// <summary>
        /// Converts DateTime to days since Unix epoch (1970-01-01) as required by Avro date logical type
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Days since Unix epoch</returns>
        private int? ConvertToDateInt(DateTime? dateTime)
        {
            if (!dateTime.HasValue || dateTime.Value == default(DateTime))
            {
                return null;
            }

            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utcDateTime = dateTime.Value.Kind == DateTimeKind.Utc 
                ? dateTime.Value 
                : dateTime.Value.ToUniversalTime();
            return (int)(utcDateTime.Date - unixEpoch.Date).TotalDays;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose pattern
        /// </summary>
        /// <param name="disposing">True if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                lock (_lock)
                {
                    if (_producer != null)
                    {
                        // Flush with timeout to ensure all messages are sent
                        _producer.Flush(TimeSpan.FromSeconds(30));
                        _producer.Dispose();
                    }
                }
            }

            _disposed = true;
        }
    }
}