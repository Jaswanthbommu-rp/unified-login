using System.ServiceModel;
using System.ServiceModel.MsmqIntegration;
using RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Command
{
    [ServiceContract]
    public interface IWriter
    {
        [OperationContract(IsOneWay = true, Action = "*")]
        [ServiceKnownType(typeof(ActivityDetailMessage))]
        void ReadMqActivity(MsmqMessage<ActivityDetailMessage> mqMessage);
    }
}
