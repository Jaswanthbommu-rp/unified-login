using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Kafka;
using System.Threading.Tasks;
namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging
{
    // <summary>
    /// Interface for Kafka message producer
    /// </summary>
    public interface IKafkaProducerService
    {
        /// <summary>
        /// Publishes a user status change event to Kafka
        /// </summary>
        /// <param name="userStatusEvent">User status change event data</param>
        /// <returns>Task indicating completion</returns>
        Task PublishUserStatusChangeEventAsync(UnifiedLoginUserStatusEvent userStatusEvent);
    }
}
