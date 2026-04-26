using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.BatchProcessAsync.Factory;

/// <summary>
/// Async contract for product-specific user profile updates invoked from the batch pipeline.
/// Replaces the sync <c>IProduct</c> interface whose <c>UpdateProductUserProfile</c> blocked
/// the thread via <c>Activator.CreateInstance</c> inside <c>ProductFactory</c>.
/// </summary>
public interface IProductAsync
{
    /// <summary>
    /// Applies a product-specific user profile update for the batch record.
    /// Returns <see cref="string.Empty"/> on success; otherwise an error message.
    /// </summary>
    Task<string> UpdateProductUserProfileAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default);
}
