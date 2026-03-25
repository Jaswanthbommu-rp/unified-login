using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for product management operations.
/// Delegates to the existing sync <see cref="IManageProduct"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageProductAsync : IManageProductAsync
{
    private readonly IManageProduct _manageProduct;

    public ManageProductAsync(IManageProduct manageProduct)
    {
        _manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
    }

    public Task<IList<ProductUI>> GetProductsAsync(Guid realPageId, long personaId, bool allProducts, bool replaceProductCodeWithUDMIfExists = true, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetProducts(realPageId, personaId, allProducts, replaceProductCodeWithUDMIfExists));

    public Task<List<ProductInternalSetting>> GetProductInternalSettingsAsync(int productId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetProductInternalSettings(productId));

    public Task<IList<ProductUI>> AddProductSourceAndGreenBookCareFlagToProductsAsync(Guid realPageId, long partyId, IList<ProductUI> productList, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.AddProductSourceAndGreenBookCareFlagToProducts(realPageId, partyId, productList));

    public Task<IList<ProductUsers>> GetProductUsersAsync(int productId, long companyInstanceId, long personaId = 0, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetProductUsers(productId, companyInstanceId, personaId));

    public Task<IList<ProductFamily>> GetProductFamiliesAsync(Guid organizationRealPageId, Guid realpageUserId, Guid? personRealPageId, string accessFilter = null, string loginName = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetProductFamilies(organizationRealPageId, realpageUserId, personRealPageId, accessFilter, loginName));

    public Task<IList<ProductInternalSettingByType>> GetProductSettingByTypeAsync(string productSettingType, string orgType = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetProductSettingByType(productSettingType, orgType));

    public Task<RepositoryResponse> CreateProductSettingAndLinkToConfigurationAsync(int productId, ProductInternalSetting productInternalSetting, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.CreateProductSettingAndLinkToConfiguration(productId, productInternalSetting));

    public Task<IList<ProductSettingType>> ListProductSettingTypeAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.ListProductSettingType());

    public Task<IList<ProductType>> GetProductTypesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetProductTypes());

    public Task<IList<GbProductMap>> ListProductsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.ListProducts());

    public Task<List<AdGroupProduct>> GetAdGroupsForProductAsync(int productId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetAdGroupsForProduct(productId));

    public Task<List<AdGroup>> GetAdGroupsForUserAsync(long personaId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageProduct.GetAdGroupsForUser(personaId));
}
