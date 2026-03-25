using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for product management operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageProduct"/> calls.
/// </summary>
public interface IManageProductAsync
{
    Task<IList<ProductUI>> GetProductsAsync(Guid realPageId, long personaId, bool allProducts, bool replaceProductCodeWithUDMIfExists = true, CancellationToken cancellationToken = default);
    Task<List<ProductInternalSetting>> GetProductInternalSettingsAsync(int productId, CancellationToken cancellationToken = default);
    Task<IList<ProductUI>> AddProductSourceAndGreenBookCareFlagToProductsAsync(Guid realPageId, long partyId, IList<ProductUI> productList, CancellationToken cancellationToken = default);

    Task<IList<ProductUsers>> GetProductUsersAsync(int productId, long companyInstanceId, long personaId = 0, CancellationToken cancellationToken = default);
    Task<IList<ProductFamily>> GetProductFamiliesAsync(Guid organizationRealPageId, Guid realpageUserId, Guid? personRealPageId, string accessFilter = null, string loginName = null, CancellationToken cancellationToken = default);
    Task<IList<ProductInternalSettingByType>> GetProductSettingByTypeAsync(string productSettingType, string orgType = null, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> CreateProductSettingAndLinkToConfigurationAsync(int productId, ProductInternalSetting productInternalSetting, CancellationToken cancellationToken = default);
    Task<IList<ProductSettingType>> ListProductSettingTypeAsync(CancellationToken cancellationToken = default);
    Task<IList<ProductType>> GetProductTypesAsync(CancellationToken cancellationToken = default);
    Task<IList<GbProductMap>> ListProductsAsync(CancellationToken cancellationToken = default);
    Task<List<AdGroupProduct>> GetAdGroupsForProductAsync(int productId, CancellationToken cancellationToken = default);
    Task<List<AdGroup>> GetAdGroupsForUserAsync(long personaId, CancellationToken cancellationToken = default);
}
