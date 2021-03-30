using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory
{
    public interface IIntegrationTypeFactory
    {
        IIntegrationType GetIntegration(int productId);
    }
}