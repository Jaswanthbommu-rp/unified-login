using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Services.Interfaces;

/// <summary>
/// Orchestration service for product-related operations.
/// Async-first replacement for the methods previously spread across
/// <see cref="UnifiedLogin.BusinessLogic.Logic.ManageProduct"/> and
/// <c>ProductRepository</c>.
/// </summary>
public interface IProductService
{
    // ── Tile / dashboard ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the enriched product tile list for a user's dashboard.
    /// Replaces: <c>ManageProduct.GetUserAssignedProductsByPersona</c>.
    /// </summary>
    Task<IList<PersonaProductUserDetails>> GetAssignedProductsByPersonaAsync(
        Persona persona,
        ProductSelectType? productSelectType = null,
        RouteSecurity? security = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the products an organisation has licensed, optionally merged with
    /// persona access flags (HasAccess, IsFavorite, AO sub-products).
    /// Replaces: <c>ManageProduct.GetProducts</c>.
    /// </summary>
    Task<IList<ProductUI>> GetProductsAsync(
        Guid organizationRealPageId,
        long personaId = 0,
        bool allProducts = false,
        bool replaceProductCodeWithUDMIfExists = true,
        CancellationToken cancellationToken = default);

    // ── Product families / solutions ────────────────────────────────────────

    /// <summary>
    /// Returns the product family/solution tree for the user-edit screen.
    /// Replaces: <c>ManageProduct.GetProductFamilies</c>.
    /// </summary>
    Task<IList<ProductFamily>> GetProductFamiliesAsync(
        Guid organizationRealPageId,
        Guid editorRealPageId,
        Guid? personRealPageId = null,
        string? accessFilter = null,
        string? loginName = null,
        CancellationToken cancellationToken = default);

    // ── Product types / GB map ───────────────────────────────────────────────

    /// <summary>
    /// Returns all product types.
    /// Replaces: <c>ManageProduct.GetProductTypes</c>.
    /// </summary>
    Task<IList<ProductType>> GetProductTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all GB products, optionally filtered.
    /// Replaces: <c>ManageProduct.ListProducts</c>.
    /// </summary>
    Task<IList<GbProductMap>> ListProductsAsync(
        int? productId = null,
        Guid? productGuid = null,
        string? name = null,
        string? booksProductCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all products assigned to a persona, filtered by status.
    /// Replaces: <c>ManageProduct.GetAllProductsByPersona</c>.
    /// </summary>
    Task<IList<PersonaProduct>> GetAllProductsByPersonaAsync(
        long personaId,
        ProductBatchStatusType statusType,
        CancellationToken cancellationToken = default);

    // ── Internal settings ───────────────────────────────────────────────────

    /// <summary>
    /// Returns cached internal settings for a product.
    /// Replaces: <c>ManageProduct.GetProductInternalSettings</c>.
    /// </summary>
    Task<IList<ProductInternalSetting>> GetProductInternalSettingsAsync(
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all internal settings for a given setting type.
    /// When <paramref name="productSettingType"/> is <c>"ShowInNewCompanySetup"</c>
    /// the list is filtered by <paramref name="orgType"/> using the
    /// <c>AvailableOnlyForThisOrgType</c> rule.
    /// Replaces: <c>ManageProduct.GetProductSettingByType</c>.
    /// </summary>
    Task<IList<ProductInternalSettingByType>> GetProductSettingByTypeAsync(
        string productSettingType,
        string? orgType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a product internal setting, links it to the configuration and
    /// invalidates the cached settings for that product.
    /// Replaces: <c>ManageProduct.CreateProductSettingAndLinkToConfiguration</c>.
    /// </summary>
    Task<RepositoryResponse> CreateProductSettingAndLinkToConfigurationAsync(
        int productId,
        ProductInternalSetting productInternalSetting,
        CancellationToken cancellationToken = default);

    // ── Product setting types ───────────────────────────────────────────────

    /// <summary>
    /// Returns all product setting types.
    /// Replaces: <c>ManageProduct.ListProductSettingType</c>.
    /// </summary>
    Task<IList<ProductSettingType>> ListProductSettingTypeAsync(
        CancellationToken cancellationToken = default);

    // ── Persona product settings ────────────────────────────────────────────

    /// <summary>
    /// Looks up the setting type by name then creates the product setting for the persona.
    /// Replaces: <c>ManageProduct.UpdateProductSetting</c>.
    /// </summary>
    Task<RepositoryResponse> UpdateProductSettingAsync(
        ProductSetting productSetting,
        long personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience wrapper — looks up the setting type then calls CreateProductSetting.
    /// Replaces: <c>ProductRepository.UpdateProductSettingProductStatus&lt;T&gt;</c>.
    /// </summary>
    Task UpdateProductSettingProductStatusAsync<T>(
        long personaId,
        int productId,
        string settingType,
        T value,
        CancellationToken cancellationToken = default);

    // ── AD Groups ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the AD groups assigned to a user persona.
    /// Replaces: <c>ManageProduct.GetAdGroupsForUser</c>.
    /// </summary>
    Task<List<AdGroup>> GetAdGroupsForUserAsync(
        long personaId,
        CancellationToken cancellationToken = default);
}