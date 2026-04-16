using Avro.IO;
using Avro.Specific;
using com.realpage.avro.unity.unifiedlogin;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Serilog;
using System;
using System.IO;
using System.Text;

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
        protected readonly bool _isOnPrem;

        /// <summary>
        /// Cached Avro schema JSON bytes attached as a message header for on-prem consumers
        /// </summary>
        protected static readonly byte[] AvroSchemaBytes =
            Encoding.UTF8.GetBytes(UnifiedLoginUserStatus._SCHEMA.ToString());

        /// <summary>
        /// Initializes shared Kafka infrastructure and delegates typed producer creation to subclass
        /// </summary>
        protected KafkaProducerBase()
        {
            _isOnPrem = KafkaConfiguration.OnPrem.HasValue && KafkaConfiguration.OnPrem.Value;
            var producerConfig = CreateProducerConfig();

            if (_isOnPrem)
            {
                producerConfig.SecurityProtocol = SecurityProtocol.Ssl;
                producerConfig.SaslMechanism = null;
                producerConfig.SaslUsername = null;
                producerConfig.SaslPassword = null;
                producerConfig.SslCaCertificateStores = KafkaConfiguration.SslCaCertificateStores;

                // On-prem: inline Avro serializer — no Schema Registry dependency
                _producer = new ProducerBuilder<string, UnifiedLoginUserStatus>(producerConfig)
                    .SetKeySerializer(Serializers.Utf8)
                    .SetValueSerializer(new InlineAvroSerializer())
                    .SetErrorHandler((_, e) =>
                    {
                        Log.Logger.Error("Kafka producer error: {Reason}", e.Reason);
                    })
                    .Build();
            }
            else
            {
                // Cloud: use Schema Registry for schema management
                var schemaRegistryConfig = CreateSchemaRegistryConfig();
                _schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
                BuildProducer(producerConfig);
            }
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

        /// <summary>
        /// Inline Avro serializer using the schema from the generated
        /// <see cref="UnifiedLoginUserStatus"/> class. Produces raw Avro binary
        /// without the Confluent wire-format (magic byte + schema ID), removing
        /// the Schema Registry dependency for on-prem environments.
        /// The writer schema is delivered to consumers via the "avro.schema"
        /// message header.
        /// </summary>
        protected sealed class InlineAvroSerializer : ISerializer<UnifiedLoginUserStatus>
        {
            public byte[] Serialize(UnifiedLoginUserStatus data, SerializationContext context)
            {
                if (data == null) return null;

                using (var stream = new MemoryStream())
                {
                    var writer = new SpecificDatumWriter<UnifiedLoginUserStatus>(UnifiedLoginUserStatus._SCHEMA);
                    var encoder = new BinaryEncoder(stream);
                    writer.Write(data, encoder);
                    encoder.Flush();
                    return stream.ToArray();
                }
            }
        }
    }
}

