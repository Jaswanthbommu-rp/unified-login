using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Factory that resolves the correct <see cref="IIntegrationTypeAsync"/> implementation
/// for a given product ID.
/// Replaces <see cref="UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory.IIntegrationTypeFactory"/>.
/// <para>
/// <list type="bullet">
///   <item>Returns <see cref="IIntegrationTypeAsync"/> instead of the sync
///     <c>IIntegrationType</c>.</item>
///   <item>Factory resolution itself is synchronous — no I/O is performed during
///     lookup, so these methods do not return <c>Task</c>.</item>
///   <item><c>DefaultUserClaim</c> removed — the resolved implementation receives
///     caller context via the injected <c>IUserClaimsAccessor</c>.</item>
/// </list>
/// </para>
/// </summary>
public interface IIntegrationTypeFactoryAsync
{
    /// <summary>
    /// Returns the <see cref="IIntegrationTypeAsync"/> implementation registered for
    /// <paramref name="productId"/>. Throws <see cref="InvalidOperationException"/>
    /// when no integration type is registered for the product.
    /// </summary>
    IIntegrationTypeAsync GetIntegration(int productId);

    /// <summary>
    /// Returns the StandardV1 <see cref="IIntegrationTypeAsync"/> implementation for
    /// <paramref name="productId"/>, bypassing any product-specific override.
    /// </summary>
    IIntegrationTypeAsync GetIntegrationStandardV1(int productId);

    /// <summary>
    /// Returns the <see cref="ProductIntegrationTypeEnum"/> that maps to
    /// <paramref name="productId"/>. Used by callers that need to branch on
    /// integration type without resolving the full implementation.
    /// </summary>
    ProductIntegrationTypeEnum GetIntegrationTypeForProductId(int productId);
}
