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
        public static string BootstrapServers => 
            ConfigurationManager.AppSettings["Kafka:BootstrapServers"] 
            ?? "pkc-xxdnk.us-east-2.aws.confluent.cloud:9092";

        /// <summary>
        /// Gets the Kafka topic name for user status events
        /// </summary>
        public static string UserStatusTopicName => 
            ConfigurationManager.AppSettings["Kafka:UserStatusTopic"] 
            ?? "unified-login-user-status-dev";

        /// <summary>
        /// Gets the Schema Registry URL
        /// </summary>
        public static string SchemaRegistryUrl => 
            ConfigurationManager.AppSettings["Kafka:SchemaRegistryUrl"] 
            ?? "https://psrc-vn38j.us-east-2.aws.confluent.cloud";

        public static string SchemaRegistryBasicAuthUserInfo =>
       ConfigurationManager.AppSettings["Kafka:SchemaRegistryBasicAuthUserInfo"]
       ?? "OY5KIFQCOYDITBEN:yvJkGRCDiq3+xnVTzqFh2Tq3yxKvv1ahb2K0I0/0HAludAHyS3Zlj+UVBLJ2gFOP";


        public static string SaslUsername =>
       ConfigurationManager.AppSettings["Kafka:SaslUsername"]
       ?? "JKWMTFLNEA5NCY4J";


       public static string SaslPassword =>
       ConfigurationManager.AppSettings["Kafka:SaslPassword"]
       ?? "+zafbYYkj5B9cceN62TfK7BHWpVAdNHMedKEEmLCTNuCe5RSRyMvYukvja/+QXx+";


    }
}
