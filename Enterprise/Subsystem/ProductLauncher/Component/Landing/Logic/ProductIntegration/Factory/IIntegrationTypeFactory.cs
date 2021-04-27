using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory
{
    public interface IIntegrationTypeFactory
    {
        IIntegrationType GetIntegration(int productId);

        ProductIntegrationTypeEnum GetIntegrationTypeForProductId(int productId);
    }
}