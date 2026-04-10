using com.realpage.avro.unity.unifiedlogin;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Serilog;
using System;
using static Confluent.Kafka.ConfigPropertyNames;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    /// <summary>
    /// Abstract base class for Kafka producers providing shared infrastructure:
    /// ProducerConfig, SchemaRegistryConfig, on-prem handling, and Dispose pattern.
    /// </summary>
    public abstract class KafkaProducerBase : IDisposable
    {
        protected readonly CachedSchemaRegistryClient _schemaRegistry;
        protected readonly object _lock = new object();
        private bool _disposed = false;
        public IProducer<string, UnifiedLoginUserStatus> _producer;

        /// <summary>
        /// Initializes shared Kafka infrastructure and delegates typed producer creation to subclass
        /// </summary>
        protected KafkaProducerBase()
        {
            var producerConfig = CreateProducerConfig();
            var schemaRegistryConfig = CreateSchemaRegistryConfig();
            var logger = Log.Logger.ForContext("ProductModule", this.GetType());

            if (KafkaConfiguration.OnPrem.HasValue && KafkaConfiguration.OnPrem.Value)
            {
                producerConfig.SecurityProtocol = SecurityProtocol.Ssl;
                producerConfig.SaslUsername = null;
                producerConfig.SaslPassword = null;
                schemaRegistryConfig.BasicAuthUserInfo = null;
                if (!string.IsNullOrEmpty(KafkaConfiguration.SaslUsername) && !string.IsNullOrEmpty(KafkaConfiguration.SaslPassword))
                {
                    producerConfig.SaslUsername = KafkaConfiguration.SaslUsername;
                    producerConfig.SaslPassword = KafkaConfiguration.SaslPassword;
                }
                if (!string.IsNullOrEmpty(KafkaConfiguration.SchemaRegistryBasicAuthUserInfo))
                {
                    schemaRegistryConfig.BasicAuthUserInfo = KafkaConfiguration.SchemaRegistryBasicAuthUserInfo;
                }
                producerConfig.SslCaCertificateStores = KafkaConfiguration.SslCaCertificateStores;
            }

            _schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
            BuildProducer(producerConfig);
        }

        protected void BuildProducer(ProducerConfig config)
        {
            _producer = new ProducerBuilder<string, UnifiedLoginUserStatus>(config)
                .SetValueSerializer(new AvroSerializer<UnifiedLoginUserStatus>(_schemaRegistry))
                .SetErrorHandler((_, e) =>
                {
                    Log.Logger.Error("Kafka producer error: {Reason}", e.Reason);
                })
                .Build();
        }

        /// <summary>
        /// Creates a standard ProducerConfig from KafkaConfiguration
        /// </summary>
        protected static ProducerConfig CreateProducerConfig()
        {
            return new ProducerConfig
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
                //EnableSslCertificateVerification = true,
                //// Performance tuning for high throughput
                //LingerMs = 10, // Batch messages for up to 10ms
                //BatchSize = 16384, // 16KB batch size
                //CompressionType = CompressionType.Snappy, // Fast compression
                //QueueBufferingMaxMessages = 100000,
                //QueueBufferingMaxKbytes = 1048576 // 1GB buffer
            };
        }

        /// <summary>
        /// Creates a standard SchemaRegistryConfig from KafkaConfiguration
        /// </summary>
        protected static SchemaRegistryConfig CreateSchemaRegistryConfig()
        {
            return new SchemaRegistryConfig
            {
                Url = KafkaConfiguration.SchemaRegistryUrl,
                BasicAuthUserInfo = KafkaConfiguration.SchemaRegistryBasicAuthUserInfo
            };
        }

        /// <summary>
        /// Builds the typed Kafka producer. Implemented by each concrete producer.
        /// </summary>
        /// <param name="config">The shared ProducerConfig</param>
      //  protected abstract void BuildProducer(ProducerConfig config);

        /// <summary>
        /// Flushes and disposes the typed Kafka producer. Implemented by each concrete producer.
        /// </summary>
        protected abstract void DisposeProducer();

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
                    try
                    {
                        DisposeProducer();
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Error disposing Kafka producer");
                    }
                }
            }

            _disposed = true;
        }
    }
}
