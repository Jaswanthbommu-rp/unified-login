using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface INoCacheRepository
{
    List<ProductSetting> GetProductInternalSettings(int productId);
}
