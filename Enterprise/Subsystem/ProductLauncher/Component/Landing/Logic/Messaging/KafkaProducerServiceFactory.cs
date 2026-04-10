using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    /// <summary>
    /// Singleton factory for managing KafkaProducerService instance
    /// Thread-safe implementation using lazy initialization
    /// </summary>
    public sealed class KafkaProducerServiceFactory
    {
        private static readonly Lazy<IKafkaProducerService> _instance = 
            new Lazy<IKafkaProducerService>(() => CreateInstance(), isThreadSafe: true);

        /// <summary>
        /// Gets the singleton instance of KafkaProducerService
        /// </summary>
        public static IKafkaProducerService Instance => _instance.Value;

        /// <summary>
        /// Private constructor to prevent instantiation
        /// </summary>
        private KafkaProducerServiceFactory()
        {
        }
        
        /// <summary>
        /// Creates a new instance of UserStatusKafkaProducer with configuration
        /// </summary>
        private static IKafkaProducerService CreateInstance()
        {
            return new UserStatusKafkaProducer();
        }

        /// <summary>
        /// For testing: allows resetting the singleton instance
        /// WARNING: Only use in test scenarios
        /// </summary>
        internal static void ResetForTesting()
        {
            if (_instance.IsValueCreated && _instance.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
