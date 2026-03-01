using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using com.realpage.avro.unity.unifiedlogin;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.SharedObjects.Kafka
{
    /// <summary>
    /// Kafka producer for UnifiedLogin user activation/deactivation events.
    /// </summary>
    public sealed class UnifiedLoginUserStatusProducer : IDisposable
    {
        private readonly IProducer<string, UnifiedLoginUserStatus> _producer;
        private readonly string _topic;

        /// <summary>
        /// Creates a new UnifiedLoginUserStatusProducer.
        /// </summary>
        public UnifiedLoginUserStatusProducer()
        {
            _topic = ConfigurationManager.AppSettings["Kafka:Topic"] ?? "unified-login-user-status-dev";

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = ConfigurationManager.AppSettings["Kafka:BootstrapServers"],
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = ConfigurationManager.AppSettings["Kafka:SaslUsername"],
                SaslPassword = ConfigurationManager.AppSettings["Kafka:SaslPassword"],
                EnableSslCertificateVerification = true,
                ClientId = ConfigurationManager.AppSettings["Kafka:ClientId"] ?? "unifiedlogin-userstatus-producer",
                Acks = Acks.All,
                EnableIdempotence = true
            };

            var basicAuthUserInfo = ConfigurationManager.AppSettings["Kafka:SchemaRegistryBasicAuthUserInfo"];
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = ConfigurationManager.AppSettings["Kafka:SchemaRegistryUrl"],
                BasicAuthUserInfo = basicAuthUserInfo
            };

            var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

            var avroSerializerConfig = new AvroSerializerConfig
            {
                SubjectNameStrategy = SubjectNameStrategy.TopicRecord
            };

            var valueSerializer = new AvroSerializer<UnifiedLoginUserStatus>(
                schemaRegistryClient,
                avroSerializerConfig
            );

            _producer = new ProducerBuilder<string, UnifiedLoginUserStatus>(producerConfig)
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(valueSerializer)
                .Build();
        }

        /// <summary>
        /// Produces a UnifiedLoginUserStatus message to Kafka.
        /// </summary>
        /// <param name="status">User status payload.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<DeliveryResult<string, UnifiedLoginUserStatus>> ProduceAsync(UnifiedLoginUserStatus status, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (status == null) throw new ArgumentNullException(nameof(status));

            var key = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            var message = new Message<string, UnifiedLoginUserStatus>
            {
                Key = key,
                Value = status
            };

            return await _producer.ProduceAsync(_topic, message, cancellationToken);
        }

        /// <summary>
        /// Flushes outstanding messages and disposes the underlying producer.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _producer.Flush(TimeSpan.FromSeconds(10));
            }
            catch
            {
                // ignore flush exceptions during dispose
            }

            _producer.Dispose();
        }
    }
}