using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for contact mechanism usage type operations.
/// Replaces <see cref="Logic.Interfaces.IManageContactMechanismUsageType"/>.
/// </summary>
public interface IManageContactMechanismUsageTypeAsync
{
    /// <summary>
    /// Returns all contact mechanism usage types, optionally filtered by name.
    /// Returns an empty list (never <c>null</c>) when no records are found or on error.
    /// </summary>
    /// <param name="contactMechanismUsageTypeName">
    /// Optional filter; pass <c>null</c> or empty to return all types.
    /// </param>
    Task<IList<ContactMechanismUsageType>> ListContactMechanismUsageTypeAsync(
        string?           contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default);
}
