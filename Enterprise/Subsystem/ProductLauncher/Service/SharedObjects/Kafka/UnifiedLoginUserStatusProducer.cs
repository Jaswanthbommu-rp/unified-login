using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using com.realpage.avro.unity.unifiedlogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.SharedObjects.Kafka
{
    /// <summary>
    /// Kafka producer for UnifiedLogin user activation/deactivation events.
    /// </summary>
    public sealed class UnifiedLoginUserStatusProducer : IDisposable
    {
        private static readonly object _lock = new object();
        private static UnifiedLoginUserStatusProducer _instance;

        private readonly IProducer<string, UnifiedLoginUserStatus> _producer;
        private readonly string _topic;

        /// <summary>
        /// Gets the singleton instance of the Kafka producer.
        /// Must be initialized via <see cref="Initialize"/> during application startup.
        /// </summary>
        public static UnifiedLoginUserStatusProducer Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("UnifiedLoginUserStatusProducer has not been initialized. Call Initialize() in Startup.");
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the singleton Kafka producer using centralized configuration from ConfigReader.
        /// Should be called once during application startup.
        /// </summary>
        public static void Initialize()
        {
            if (_instance != null) return;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new UnifiedLoginUserStatusProducer();
                }
            }
        }

        /// <summary>
        /// Shuts down the singleton Kafka producer instance.
        /// Should be called during application shutdown.
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    _instance.Dispose();
                    _instance = null;
                }
            }
        }

        /// <summary>
        /// Creates a new UnifiedLoginUserStatusProducer using centralized ConfigReader settings.
        /// </summary>
        private UnifiedLoginUserStatusProducer()
        {
            _topic = ConfigReader.KafkaTopic;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = ConfigReader.KafkaBootstrapServers,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = ConfigReader.KafkaSaslUsername,
                SaslPassword = ConfigReader.KafkaSaslPassword,
                EnableSslCertificateVerification = true,
                ClientId = ConfigReader.KafkaClientId,
                Acks = Acks.All,
                EnableIdempotence = true
            };

            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = ConfigReader.KafkaSchemaRegistryUrl,
                BasicAuthUserInfo = ConfigReader.KafkaSchemaRegistryBasicAuthUserInfo
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