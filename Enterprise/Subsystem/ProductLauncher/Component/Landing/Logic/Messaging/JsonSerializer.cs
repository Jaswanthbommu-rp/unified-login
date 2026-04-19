using com.realpage.avro.unity.unifiedlogin;
using Confluent.Kafka;
using Confluent.SchemaRegistry;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    internal class JsonSerializer<T> : ISerializer<UnifiedLoginUserStatus>
    {
        private CachedSchemaRegistryClient schemaRegistry;

        public JsonSerializer(CachedSchemaRegistryClient schemaRegistry)
        {
            this.schemaRegistry = schemaRegistry;
        }

        public byte[] Serialize(UnifiedLoginUserStatus data, SerializationContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}