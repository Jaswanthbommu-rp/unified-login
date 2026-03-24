using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Focused service for product configuration reads and status writes.
/// <para>
/// Extracts three groups of helpers from <c>ManageProductBase</c>:
/// </para>
/// <list type="number">
///   <item><see cref="GetProductSettingAsync"/> — cached product internal settings.</item>
///   <item><see cref="GetProductDetailAsync"/> — cached BooksProductMap (name, code, etc.).</item>
///   <item><see cref="UpdateProductStatusAsync"/> — write product-batch status for a persona.</item>
/// </list>
/// </summary>
public interface IProductSettingServiceAsync
{
    /// <summary>
    /// Returns the internal settings for <paramref name="productId"/>,
    /// using a 2-minute cache on hit.
    /// Replaces: <c>ManageProductBase.GetProductSetting(productId)</c>.
    /// </summary>
    Task<List<ProductInternalSetting>> GetProductSettingAsync(
        int productId, CancellationToken ct = default);

    /// <summary>
    /// Returns the BlueBook product map for <paramref name="productId"/>,
    /// using a 2-minute cache on hit.
    /// Replaces: private <c>ManageProductBase.GetBooksMasterProductDetail(productId, noCache)</c>.
    /// </summary>
    Task<GbProductMap?> GetProductDetailAsync(
        int productId, CancellationToken ct = default);

    /// <summary>
    /// Writes a product-batch status setting for the given persona, upgrading the status
    /// to <c>Deactivated</c> when the organisation login is <c>Disabled</c>.
    /// Replaces: both overloads of <c>ManageProductBase.UpdateProductSettingProductStatus&lt;T&gt;</c>.
    /// </summary>
    Task UpdateProductStatusAsync<TValue>(
        long userPersonaId, string settingType, int productId,
        TValue value, CancellationToken ct = default);
}