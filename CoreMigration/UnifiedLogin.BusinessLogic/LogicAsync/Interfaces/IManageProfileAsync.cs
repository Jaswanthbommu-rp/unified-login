using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for profile read/write operations.
/// Stepping-stone wrapper that encapsulates the multi-service orchestration
/// previously living inside <c>ProfileController</c> actions.
/// </summary>
public interface IManageProfileAsync
{
    /// <summary>Returns the populated profile, or null when the person is not found.</summary>
    Task<IProfile> GetProfileAsync(Guid realPageId, string contactMechanismUsageTypeName, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the profile-detail-organisations query chain.
    /// Returns true when the person is found; false when not found.
    /// Note: the original endpoint always returns an empty <c>Profile</c> object on success
    /// (the profileDetail is populated but never surfaced — bug preserved per no-contract-breaking rule).
    /// </summary>
    Task<bool> GetProfileDetailOrganizationsAsync(Guid realPageId, string roleTypeFrom, string roleTypeTo, string relationshipType, string contactMechanismUsageTypeName, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    Task<RepositoryResponse> UpdateProfileAsync(Guid realPageId, IProfile profile, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);

    Task<IProfileDetail> GetProfileDetailAsync(Guid realPageId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);
}
