using System.Configuration;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    /// <summary>
    /// Configuration helper for Kafka settings
    /// </summary>
    public static class KafkaConfiguration
    {
        /// <summary>
        /// Gets the Kafka bootstrap servers from configuration
        /// </summary>
        public static string BootstrapServers = ConfigurationManager.AppSettings["Kafka:BootstrapServers"];
   

        /// <summary>
        /// Gets the Kafka topic name for user status events
        /// </summary>
        public static string UserStatusTopicName = ConfigurationManager.AppSettings["Kafka:UserStatusTopicName"];


        /// <summary>
        /// Gets the Schema Registry URL
        /// </summary>
        public static string SchemaRegistryUrl = ConfigurationManager.AppSettings["Kafka:SchemaRegistryUrl"];

        /// <summary>
        /// Gets the SchemaRegistryBasicAuthUserInfo
        /// </summary>
        public static string SchemaRegistryBasicAuthUserInfo = ConfigurationManager.AppSettings["Kafka:SchemaRegistryBasicAuthUserInfo"];


        /// <summary>
        /// Gets the SASL username for Kafka SaslUsername
        /// </summary>
        public static string SaslUsername = ConfigurationManager.AppSettings["Kafka:SaslUsername"];


        /// <summary>
        /// Gets the SASL password for Kafka SaslPassword
        /// </summary>
        public static string SaslPassword = ConfigurationManager.AppSettings["Kafka:SaslPassword"];


        /// <summary>
        /// Gets the Kafka OnPrem to identify 
        /// </summary>
        public static bool? OnPrem = bool.TryParse(ConfigurationManager.AppSettings["Kafka:OnPrem"], out var onPrem) ? onPrem : (bool?)null;



    }
}
