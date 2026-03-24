using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.Interfaces;

/// <summary>
/// Orchestration service for profile read and write operations.
/// Replaces the mixed-concern UpdateProfile / ListPersons methods
/// that previously lived inside ProfileRepository.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Updates a user's personal details, job title, phone numbers,
    /// email contacts and queues any required product batch jobs.
    /// Replaces: ProfileRepository.UpdateProfile (350 lines).
    /// </summary>
    Task<RepositoryResponse> UpdateProfileAsync(
        Guid realPageId,
        IProfile profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paged, filtered, sorted list of ProfileDetail records
    /// for the current organisation, enriched with login status, supervisor
    /// and default phone.
    /// Replaces: ProfileRepository.ListPersons (200 lines).
    /// </summary>
    Task<IList<ProfileDetail>> ListPersonsAsync(
        IList<int> organizationActiveProductIdList,
        Guid? realPageId = null,
        int? parentPartyRoleTypeId = null,
        RequestParameter dataFilterSort = null,
        bool isExport = false,
        CancellationToken cancellationToken = default);
}