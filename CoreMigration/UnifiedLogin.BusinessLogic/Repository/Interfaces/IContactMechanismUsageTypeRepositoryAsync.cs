using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async repository interface for contact mechanism usage type lookups.
/// </summary>
public interface IContactMechanismUsageTypeRepositoryAsync
{
    /// <summary>
    /// Returns contact mechanism usage types, optionally filtered by name.
    /// Returns an empty list (never <c>null</c>) when no records match or on error.
    /// </summary>
    /// <param name="contactMechanismUsageTypeName">
    /// Optional filter; pass <c>null</c> or empty to return all types.
    /// </param>
    Task<IList<ContactMechanismUsageType>> ListContactMechanismUsageTypeAsync(
        string?           contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default);
}
