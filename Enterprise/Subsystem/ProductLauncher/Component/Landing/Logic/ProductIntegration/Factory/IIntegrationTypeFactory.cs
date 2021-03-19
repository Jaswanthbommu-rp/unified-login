using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory
{
    public interface IIntegrationTypeFactory
    {
        IIntegrationType GetIntegration(int productId, DefaultUserClaim userClaims);
    }
}