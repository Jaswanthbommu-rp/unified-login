using System.Collections.Frozen;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;

/// <summary>
/// Async-first replacement for the sync <c>ProductFactory</c>.
/// <para>
/// Eliminates <c>Activator.CreateInstance</c> and the <c>Dictionary&lt;ProductEnum, Type&gt;</c>
/// approach in favour of a <see cref="FrozenDictionary{TKey,TValue}"/> populated from a caller-
/// supplied registration map, making all product handlers DI-resolved at startup.
/// </para>
/// <para>
/// <b>DI registration (example):</b>
/// <code>
/// // Implement IProductAsync on the appropriate async product class, e.g.:
/// //   LeadManagementProfileUpdaterAsync : IProductAsync
/// services.AddScoped&lt;IProductAsync, LeadManagementProfileUpdaterAsync&gt;(
///     _ => new LeadManagementProfileUpdaterAsync(...));
///
/// services.AddScoped&lt;ProductFactoryAsync&gt;(sp =>
///     new ProductFactoryAsync(new Dictionary&lt;ProductEnum, IProductAsync&gt;
///     {
///         [ProductEnum.LeadAnalytics] = sp.GetRequiredService&lt;LeadManagementProfileUpdaterAsync&gt;()
///     }));
/// </code>
/// </para>
/// </summary>
public sealed class ProductFactoryAsync
{
    private readonly FrozenDictionary<ProductEnum, IProductAsync> _registry;

    /// <param name="registrations">
    /// Map of <see cref="ProductEnum"/> → <see cref="IProductAsync"/> handlers.
    /// Pass an empty dictionary if no product-specific handlers are needed yet.
    /// </param>
    public ProductFactoryAsync(IReadOnlyDictionary<ProductEnum, IProductAsync> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        _registry = registrations.ToFrozenDictionary();
    }

    /// <summary>
    /// Returns the <see cref="IProductAsync"/> handler for <paramref name="productEnum"/>.
    /// Throws <see cref="InvalidOperationException"/> if no handler is registered.
    /// </summary>
    public IProductAsync GetProductLogic(ProductEnum productEnum)
        => _registry.TryGetValue(productEnum, out var handler)
            ? handler
            : throw new InvalidOperationException(
                $"No async product handler registered for product '{productEnum}'.");

    /// <summary>
    /// Returns <see langword="true"/> if a handler is registered for
    /// <paramref name="productEnum"/> and sets <paramref name="handler"/>.
    /// </summary>
    public bool TryGetProductLogic(ProductEnum productEnum, out IProductAsync? handler)
        => _registry.TryGetValue(productEnum, out handler);
}
