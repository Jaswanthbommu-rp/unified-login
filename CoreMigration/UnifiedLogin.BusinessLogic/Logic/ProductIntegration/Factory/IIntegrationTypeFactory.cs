using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Types;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory
{
    public interface IIntegrationTypeFactory
    {
        IIntegrationType GetIntegration(int productId);

        IIntegrationType GetIntegrationStandardV1(int productId);

        ProductIntegrationTypeEnum GetIntegrationTypeForProductId(int productId);
    }
}