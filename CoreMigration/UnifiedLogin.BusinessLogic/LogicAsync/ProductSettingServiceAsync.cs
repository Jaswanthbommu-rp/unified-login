using Microsoft.Extensions.Caching.Memory;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Default implementation of <see cref="IProductSettingServiceAsync"/>.
/// </summary>
public sealed class ProductSettingServiceAsync : IProductSettingServiceAsync
{
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepo;
    private readonly IProductRepositoryAsync _productRepo;
    private readonly IMemoryCache _cache;

    private const int CacheMinutes = 2;

    public ProductSettingServiceAsync(
        IProductInternalSettingRepositoryAsync internalSettingRepo,
        IProductRepositoryAsync productRepo,
        IMemoryCache cache)
    {
        ArgumentNullException.ThrowIfNull(internalSettingRepo); _internalSettingRepo = internalSettingRepo;
        ArgumentNullException.ThrowIfNull(productRepo);         _productRepo         = productRepo;
        ArgumentNullException.ThrowIfNull(cache);               _cache               = cache;
    }

    // ── 1. Settings read ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<List<ProductInternalSetting>> GetProductSettingAsync(
        int productId, CancellationToken ct = default)
    {
        string key = $"productInternalSetting_{productId}";
        if (_cache.TryGetValue(key, out List<ProductInternalSetting>? hit))
            return hit!;

        var settings = await _internalSettingRepo.GetProductInternalSettingsAsync(productId, ct);
        var list     = settings.ToList();
        _cache.Set(key, list, TimeSpan.FromMinutes(CacheMinutes));
        return list;
    }

    // ── 2. Product detail read ───────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<GbProductMap?> GetProductDetailAsync(
        int productId, CancellationToken ct = default)
    {
        string key = $"productDetails_{productId}";
        if (_cache.TryGetValue(key, out GbProductMap? hit))
            return hit;

        // GetBooksMasterProductDetailAsync does not accept CancellationToken — no ct passed.
        var detail = await _productRepo.GetBooksMasterProductDetailAsync(productId);
        if (detail is not null)
            _cache.Set(key, detail, TimeSpan.FromMinutes(CacheMinutes));
        return detail;
    }

    // ── 3. Product status write ──────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Delegates directly to <see cref="IProductRepositoryAsync.UpdateProductSettingProductStatusAsync{T}"/>,
    /// which already contains the full logic: status escalation (Inactive/Deleted → Deactivated when
    /// the org login is Disabled) + the <c>ListProductSettingType</c> / <c>CreateProductSetting</c>
    /// SP calls. Re-implementing it here would duplicate repository-level business rules.
    /// Note: the underlying SP calls do not accept <see cref="CancellationToken"/>,
    /// so <paramref name="ct"/> is intentionally unused.
    /// </remarks>
    public Task UpdateProductStatusAsync<TValue>(
        long userPersonaId, string settingType, int productId,
        TValue value, CancellationToken ct = default)
        => _productRepo.UpdateProductSettingProductStatusAsync(userPersonaId, productId, settingType, value);
}