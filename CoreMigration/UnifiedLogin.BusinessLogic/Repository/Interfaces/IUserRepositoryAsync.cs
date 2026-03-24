using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using SO = UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async data-access interface for user records.
/// <para>
/// Orchestration methods (CreateUser, UpdateUser, DisableUserProduct,
/// ProcessDisabledUsers, etc.) were deliberately excluded — they coordinate
/// multiple SPs, external services, and contain business rules.
/// They belong in <see cref="Services.Interfaces.IUserService"/>.
/// </para>
/// </summary>
public interface IUserRepositoryAsync
{
    // ── Identity / login lookups ──────────────────────────────────────────
    Task<SO.User> GetEnterpriseUserAsync(string enterpriseUserName, CancellationToken cancellationToken = default);

    /// <summary>Replaces: <c>GetEnterpriseUser(Guid)</c> which threw <see cref="NotImplementedException"/>.</summary>
    Task<UserLogin> GetEnterpriseUserByRealPageIdAsync(Guid realPageId, CancellationToken cancellationToken = default);

    Task<bool> CheckOrganizationAdminUserAsync(Guid userRealPageId, long orgPartyId, CancellationToken cancellationToken = default);

    Task<UserLogin> UpdateUserLoginAsync(
        Guid realPageId, long organizationPartyId,
        string loginId = null, bool? isActive = null,
        string passwordHash = null, string passwordSalt = null,
        bool? isLocked = null, bool? isTainted = null,
        DateTime? fromDate = null, DateTime? thruDate = null,
        CancellationToken cancellationToken = default);

    // ── Profile ───────────────────────────────────────────────────────────
    Task<StarterProfileOptionsResponse> GetStarterProfileOptionsAsync(string enterpriseUserName, CancellationToken cancellationToken = default);

    /// <summary>Pure constructor — no DB call; kept synchronous.</summary>
    SetStarterProfile SetStarterProfileOptions(StarterProfile starterProfileOptions);

    Task<UserDetails> GetUserDetailsAsync(long? personaId = null, string userRealPageId = null, CancellationToken cancellationToken = default);

    Task<long> GetSuperUserCountByOrganizationAsync(long? organizationPartyId, CancellationToken cancellationToken = default);

    // ── Employee / supervisor ─────────────────────────────────────────────
    Task<IUserEmployeeId> GetUserEmployeeIdAsync(long userLoginPersonaId, long organizationPartyId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateUserEmployeeIdAsync(IUserEmployeeId employeeIdDetail, CancellationToken cancellationToken = default);
    Task<UserInfoLite> GetSuperVisorInformationAsync(long userId, long organizationPartyId, CancellationToken cancellationToken = default);

    // ── Delegate admin ────────────────────────────────────────────────────
    Task<List<int>> GetDelegateAdminRoleTemplateAsync(long userLoginPersonaId, CancellationToken cancellationToken = default);

    // ── Navigation menu (cached) ──────────────────────────────────────────
    Task<IList<NavigationMenuEntry>> GetNavigationMenuAsync(CancellationToken cancellationToken = default);
    Task<IList<NavigationMenuRightEntry>> GetNavigationMenuRightsAsync(CancellationToken cancellationToken = default);
    Task<IList<NavigationMenuSetting>> GetNavigationMenuSettingsUnaccessableAsync(long partyId, CancellationToken cancellationToken = default);

    // ── AD / Azure ────────────────────────────────────────────────────────
    Task<AdUserDetail> GetAzureUserDetailsAsync(long userId, CancellationToken cancellationToken = default);
    Task<IList<EmployeeProductMapping>> GetEmployeeProductADGroupMappingAsync(long personaId, int productId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> AddUpdateEmployeeProductADGroupMappingAsync(long personaId, int productId, int adGroupId, CancellationToken cancellationToken = default);

    // ── External user relationship ────────────────────────────────────────
    Task<ExternalUserRelationship> GetExternalUserRelationshipAsync(long? userLoginPersonaId, CancellationToken cancellationToken = default);

    // ── Status ────────────────────────────────────────────────────────────
    /// <summary>
    /// Single SP call only.
    /// The "if disabled → remove products" orchestration belongs in
    /// <see cref="Services.Interfaces.IUserService.UpdateUserStatusByCompanyAsync"/>.
    /// </summary>
    Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(
        Guid realPageId, long organizationPartyId,
        int statusTypeId, DateTime fromDate, DateTime? thruDate,
        CancellationToken cancellationToken = default);

    // ── Activity / audit support ──────────────────────────────────────────
    Task<ActivityAttempt> UpdateUserActivityAttemptsAsync(
        string enterpriseUserName, ActivityType activityType,
        UserDeviceDetails userDeviceDetails, long partyId,
        string authenticationServiceId = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <c>SP_UpdateUsersIDP</c> and returns the updated user IDs.
    /// Audit logging (calling <c>SP_GetUserProfileByUserIds</c> + LogActivity) is
    /// the caller's responsibility — see
    /// <see cref="Services.Interfaces.IUserService.ThirdPartyIdpBulkUpdateAsync"/>.
    /// </summary>
    Task<IList<long>> ThirdPartyIdpBulkUpdateAsync(
        IList<long> userIds, bool isEnabled, long organizationPartyId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns user profile rows needed by the bulk-IDP audit log.</summary>
    Task<IList<UserActivityLogInfo>> GetUserProfilesByUserIdsAsync(
        long organizationPartyId, IList<long> userIds,
        CancellationToken cancellationToken = default);
}