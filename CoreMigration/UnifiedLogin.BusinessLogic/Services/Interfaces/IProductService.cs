using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.BusinessLogic.Services.Interfaces;

/// <summary>
/// Orchestration service for the three methods that were business logic
/// masquerading as data-access inside ProductRepository.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Returns the enriched product tile list for a user's dashboard.
    /// Replaces: ProductRepository.GetAssignedProductsByPersona (300 lines).
    /// </summary>
    Task<IList<PersonaProductUserDetails>> GetAssignedProductsByPersonaAsync(
        Persona persona,
        ProductSelectType? productSelectType = null,
        RouteSecurity? security = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the product family/solution tree for the user-edit screen.
    /// Replaces: ProductRepository.GetProductFamilies (400 lines).
    /// </summary>
    Task<IList<ProductFamily>> GetProductFamiliesAsync(
        Guid organizationRealPageId,
        Guid editorRealPageId,
        Guid? personRealPageId = null,
        string? accessFilter = null,
        string? loginName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience wrapper — looks up the setting type then calls CreateProductSetting.
    /// Replaces: ProductRepository.UpdateProductSettingProductStatus&lt;T&gt;.
    /// </summary>
    Task UpdateProductSettingProductStatusAsync<T>(
        long personaId,
        int productId,
        string settingType,
        T value,
        CancellationToken cancellationToken = default);
}