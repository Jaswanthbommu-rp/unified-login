using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for user login operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageUserLogin"/> + blocking <c>Task.Run</c> calls.
/// </summary>
public interface IManageUserLoginAsync
{
    // ── Read ─────────────────────────────────────────────────────────────

    Task<UserLoginOnly> GetUserLoginOnlyAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task<UserLoginOnly> GetUserLoginOnlyAsync(string enterpriseUserName, CancellationToken cancellationToken = default);
    Task<UserLoginOnly> GetUserLoginOnlyAsync(long userId, CancellationToken cancellationToken = default);

    Task<UserLogin> GetUserLoginAsync(Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default);
    Task<UserLogin> GetUserLoginAsync(UserLogin userLogin, long orgPartyId, CancellationToken cancellationToken = default);

    Task<IList<Organization>> ListOrganizationByEnterpriseUserIdAsync(Guid realPageId, string? relationshipType = null, CancellationToken cancellationToken = default);
    Task<IList<UserOrganization>> GetUserPersonaOrganizationAsync(string loginName, Guid? organizationRealPageId = null, CancellationToken cancellationToken = default);
    Task<OrganizationStatus> GetUserOrganizationWithStatusAsync(long userId, DateTime lastLogin, long orgPartyId, bool getPrimaryOrg, CancellationToken cancellationToken = default);
    Task<LogOutIntervalResponse> GetLogOutIntervalAsync(Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default);
    Task<DefaultUserClaim> GetUserClaimsFromNonUserAsync(Guid userRealPageId, CancellationToken cancellationToken = default);
    Task<UserOrganizationExists> IsLoginNameExistsAsync(string loginName, Guid organizationRealPageId, Guid userRealPageId, string? firstName = null, string? lastName = null, int userType = 0, bool isFromExport = false, CancellationToken cancellationToken = default);
    Task<bool> ValidateUsernameAsync(Guid realPageId, string enterpriseUsername, CancellationToken cancellationToken = default);
    Task<bool> IsUserEmailDomainValidAsync(string loginName, string? firstName = null, string? lastName = null, Guid? userRealPageId = null, CancellationToken cancellationToken = default);

    // ── Write ─────────────────────────────────────────────────────────────

    Task<RepositoryResponse> CreateUserLoginAsync(Guid realPageId, IUserLogin userLogin, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateUserLoginAsync(Guid realPageId, IUserLogin userLogin, CancellationToken cancellationToken = default);
    Task<IList<RepositoryResponse>> UpdateUserLoginsAsync(IList<UserLogin> userLogins, UserUiStatusType userLoginStatusType, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateBulkUserLoginsAsync(IList<UserLoginOnly> userLogins, UserUiStatusType userLoginStatusType, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateLastLoginAsync(string username, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> LinkIdentityProviderToUserLoginAsync(long personaId, long userId, int contactMechanismId, CancellationToken cancellationToken = default);

    // ── Status ────────────────────────────────────────────────────────────

    Task<bool> CreateUpdateUserStatusAsync(Guid realPageId, UserUiStatusType uiStatusType, CancellationToken cancellationToken = default);
    Task<bool> UpdateActiveUserStatusAsync(Guid realPageId, UserUiStatusType uiStatusType, CancellationToken cancellationToken = default);
    Task<bool> ResendInvitationAsync(IList<UserLogin> userLogins, bool isCalledFromService = false, CancellationToken cancellationToken = default);
    Task<bool> ClearPasswordAndQuestionsAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task LogUserRequestedEmailLinkResentAsync(Guid userRealPageId, CancellationToken cancellationToken = default);
}
