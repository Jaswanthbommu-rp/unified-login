using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for profile orchestration.
/// Mirrors every method on the sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageProfile"/>
/// plus the additional enrichment methods that were previously inline in controller actions.
/// </summary>
public interface IManageProfileAsync
{
    // ── Read ─────────────────────────────────────────────────────────────

    /// <summary>Returns the fully-populated <see cref="IProfile"/>, or <c>null</c> when the person is not found.</summary>
    Task<IProfile> GetProfileAsync(Guid realPageId, string contactMechanismUsageTypeName, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the profile-detail + organisations query chain.
    /// Returns <c>true</c> when the person is found; <c>false</c> when not found.
    /// </summary>
    Task<bool> GetProfileDetailOrganizationsAsync(Guid realPageId, string roleTypeFrom, string roleTypeTo, string relationshipType, string contactMechanismUsageTypeName, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assembles the full <see cref="IProfileDetail"/> including persona-product count,
    /// identity-provider type, and password-expiration detail.
    /// </summary>
    Task<IProfileDetail> GetProfileDetailAsync(Guid realPageId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    /// <summary>Returns a paged list of profile details filtered by the caller's organisation.</summary>
    Task<IList<ProfileDetail>> ListProfileDetailsAsync(IDictionary<object, object> globals, Guid? organizationRealPageId = null, CancellationToken cancellationToken = default);

    /// <summary>Returns profiles that have the given product assigned.</summary>
    Task<IList<ProductUsers>> ListPersonsByProductIdAsync(int productId, Guid? organizationRealPageId = null, long? personaId = null, CancellationToken cancellationToken = default);

    // ── Write ─────────────────────────────────────────────────────────────

    /// <summary>Persists changes to the profile's person record.</summary>
    Task<RepositoryResponse> UpdateProfileAsync(Guid realPageId, IProfile profile, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    // ── Diagnostics ───────────────────────────────────────────────────────

    /// <summary>Returns <c>true</c> when the organisation has at least one product-assignment error.</summary>
    Task<bool> GetOrganizationHasProductAssignmentErrorAsync(long orgPartyId, CancellationToken cancellationToken = default);
}
