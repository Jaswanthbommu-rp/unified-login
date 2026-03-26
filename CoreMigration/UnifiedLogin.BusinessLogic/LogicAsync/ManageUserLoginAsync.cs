using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True-async implementation of <see cref="IManageUserLoginAsync"/>.
/// All database I/O is awaited via async repository interfaces.
/// Sync service dependencies (<see cref="IManageUserRegistrationEmail"/>, <see cref="IManageProfile"/>)
/// are called synchronously until their own async counterparts are created.
/// </summary>
public sealed class ManageUserLoginAsync : IManageUserLoginAsync
{
    #region Fields

    private readonly IUserLoginRepositoryAsync _userLoginRepository;
    private readonly IUserRepositoryAsync _userRepository;
    private readonly IOrganizationRepositoryAsync _organizationRepository;
    private readonly IRoleTypeRepositoryAsync _roleTypeRepository;
    private readonly IPersonRepositoryAsync _personRepository;
    private readonly ICredentialRepositoryAsync _credentialRepository;
    private readonly IManageUserRegistrationEmail _manageUserRegistrationEmail;
    private readonly IManageProfileAsync _manageProfile;
    private readonly IManagePersonaAsync _managePersona;
    private readonly IUserClaimsAccessor _userClaimAccessor;
    private readonly ILogger<ManageUserLoginAsync> _logger;

    private static readonly Guid EmployeeCompanyRealPageId = new("0D018E46-C20E-477D-ADED-4E5A35FB8F99");

    #endregion

    #region Constructor

    public ManageUserLoginAsync(
        IUserLoginRepositoryAsync userLoginRepository,
        IUserRepositoryAsync userRepository,
        IOrganizationRepositoryAsync organizationRepository,
        IRoleTypeRepositoryAsync roleTypeRepository,
        IPersonRepositoryAsync personRepository,
        ICredentialRepositoryAsync credentialRepository,
        IManageUserRegistrationEmail manageUserRegistrationEmail,
        IManageProfileAsync manageProfile,
        IManagePersonaAsync managePersona,
        IUserClaimsAccessor userClaimAccessor,
        ILogger<ManageUserLoginAsync> logger)
    {
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        _roleTypeRepository = roleTypeRepository ?? throw new ArgumentNullException(nameof(roleTypeRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _manageUserRegistrationEmail = manageUserRegistrationEmail ?? throw new ArgumentNullException(nameof(manageUserRegistrationEmail));
        _manageProfile = manageProfile ?? throw new ArgumentNullException(nameof(manageProfile));
        _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        _userClaimAccessor = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Read — GetUserLogin

    /// <inheritdoc/>
    public async Task<UserLoginOnly> GetUserLoginOnlyAsync(Guid realPageId, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty) throw new ArgumentException("Missing user realpage id.", nameof(realPageId));
        return await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
    }

    /// <inheritdoc/>
    public async Task<UserLoginOnly> GetUserLoginOnlyAsync(string enterpriseUserName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(enterpriseUserName)) throw new ArgumentException("Missing login name.", nameof(enterpriseUserName));
        return await _userLoginRepository.GetUserLoginOnlyAsync(enterpriseUserName);
    }

    /// <inheritdoc/>
    public async Task<UserLoginOnly> GetUserLoginOnlyAsync(long userId, CancellationToken cancellationToken = default)
    {
        if (userId == 0) throw new ArgumentException("Missing user Id.", nameof(userId));
        return await _userLoginRepository.GetUserLoginOnlyAsync(userId);
    }

    /// <inheritdoc/>
    public async Task<UserLogin> GetUserLoginAsync(Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty) throw new ArgumentException("Missing RealPage Id.", nameof(realPageId));
        return await GetUserLoginCoreAsync(realPageId, orgPartyId, null, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<UserLogin> GetUserLoginAsync(UserLogin userLogin, long orgPartyId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userLogin);
        return await GetUserLoginCoreAsync(Guid.Empty, orgPartyId, userLogin, cancellationToken);
    }

    #endregion

    #region Read — Org / Status

    /// <inheritdoc/>
    public async Task<IList<Organization>> ListOrganizationByEnterpriseUserIdAsync(
        Guid realPageId, string? relationshipType = null, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty) throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));
        return await _userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(realPageId, relationshipType);
    }

    /// <inheritdoc/>
    public Task<IList<UserOrganization>> GetUserPersonaOrganizationAsync(
        string loginName, Guid? organizationRealPageId = null, CancellationToken cancellationToken = default)
        => _userLoginRepository.ListOrganizationByLoginNameAsync(loginName, organizationRealPageId);

    /// <inheritdoc/>
    public Task<OrganizationStatus> GetUserOrganizationWithStatusAsync(
        long userId, DateTime lastLogin, long orgPartyId, bool getPrimaryOrg, CancellationToken cancellationToken = default)
        => _userLoginRepository.GetUserOrganizationWithStatusAsync(userId, lastLogin, orgPartyId, getPrimaryOrg);

    /// <inheritdoc/>
    public async Task<LogOutIntervalResponse> GetLogOutIntervalAsync(
        Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default)
    {
        var response = new LogOutIntervalResponse();

        if (realPageId == Guid.Empty)
            throw new ArgumentException("Null realPageId.", nameof(realPageId));

        var userLogin = await GetUserLoginOnlyAsync(realPageId, cancellationToken);
        if (userLogin is null)
        {
            response.IsError = true;
            response.ErrorReason = "User does not exist.";
            return response;
        }

        var orgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, orgPartyId, false);

        if (orgStatus.ThruDate is null)
        {
            response.LogOutSetInterval = -1;
            return response;
        }

        var clientLocal = ClientTimezone.GetClientDatetime(null);
        var expirationLocal = orgStatus.ThruDate.Value.ToLocalTime();
        var daysLeft = (int)Math.Floor(expirationLocal.Date.Subtract(clientLocal.Date).TotalDays);

        if (daysLeft < 0)
        {
            response.IsError = true;
            response.ErrorReason = "User account has expired.";
            response.SeverityLevel = SeverityLevelType.Critical;
            return response;
        }

        response.DaysToExpire = daysLeft;
        response.SeverityLevel = daysLeft switch
        {
            <= 10 => SeverityLevelType.Critical,
            <= 20 => SeverityLevelType.Warning,
            <= 30 => SeverityLevelType.Information,
            _ => SeverityLevelType.None
        };

        if (daysLeft <= 24)
        {
            var tsTms = new TimeSpan(23, 59, 0).TotalMilliseconds;
            var expireInMilliSec = daysLeft > 0
                ? daysLeft * 24 * 60 * 60 * 1000 + (int)tsTms
                : (int)tsTms - (int)clientLocal.TimeOfDay.TotalMilliseconds;
            response.LogOutSetInterval = expireInMilliSec;
            response.Remaining = new TimeSpan(0, 0, 0, 0, response.LogOutSetInterval).ToString(@"dd\.hh\:mm\:ss");
        }
        else
        {
            response.LogOutSetInterval = -1;
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<DefaultUserClaim> GetUserClaimsFromNonUserAsync(
        Guid userRealPageId, CancellationToken cancellationToken = default)
    {
        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(userRealPageId);
        if (userLogin is null) return null;

        var orgWithoutStatus = await _userLoginRepository.GetPrimaryOrgWithoutStatusByUserIdAsync(userLogin.UserId);
        if (orgWithoutStatus is null) return null;

        var org = await _organizationRepository.GetOrganizationAsync(realPageId: orgWithoutStatus.RealPageId);
        if (org is null) return null;

        var orgWithStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, orgWithoutStatus.PartyId, false);
        if (orgWithStatus is null) return null;

        return await GetCurrentUserClaimAsync(org, cancellationToken);
    }

    #endregion

    #region Read — Validation

    /// <inheritdoc/>
    public async Task<bool> ValidateUsernameAsync(
        Guid realPageId, string enterpriseUsername, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty) throw new ArgumentException("RealPageId is required.", nameof(realPageId));
        if (string.IsNullOrWhiteSpace(enterpriseUsername)) throw new ArgumentException("Username is required.", nameof(enterpriseUsername));

        var current = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
        if (current.LoginName.Equals(enterpriseUsername, StringComparison.OrdinalIgnoreCase))
            return true;

        var check = await _userLoginRepository.GetUserLoginOnlyAsync(enterpriseUsername);
        return check is null;
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserEmailDomainValidAsync(
        string loginName, string? firstName = null, string? lastName = null,
        Guid? userRealPageId = null, CancellationToken cancellationToken = default)
    {
        var blacklisted = await _userLoginRepository.GetBlacklistedDomainsAsync();
        var domain = loginName.Split('@').LastOrDefault();
        bool isValid = !blacklisted.Contains(domain);

        if (!isValid)
        {
            try
            {
                string? userRealpageStr = (userRealPageId.HasValue && userRealPageId.Value != Guid.Empty)
                    ? userRealPageId.Value.ToString() : null;

                var userDetails = await _userRepository.GetUserDetailsAsync(userRealPageId: userRealpageStr, cancellationToken: cancellationToken);

                UserDetails? impersonatorInfo = _userClaimAccessor.Current.ImpersonatedBy == Guid.Empty
                    ? null
                    : await _userRepository.GetUserDetailsAsync(userRealPageId: _userClaimAccessor.Current.ImpersonatedBy.ToString(), cancellationToken: cancellationToken);

                string message = (string.IsNullOrEmpty(_userClaimAccessor.Current.ImpersonatedByName) || impersonatorInfo is null)
                    ? $"{_userClaimAccessor.Current.FirstName} {_userClaimAccessor.Current.LastName} ({_userClaimAccessor.Current.LoginName}) acknowledged an Unauthorized Access warning when attempting to create a user for {firstName} {lastName} ({loginName})."
                    : $"RealPage Access ({impersonatorInfo.FirstName} {impersonatorInfo.LastName} ({impersonatorInfo.LoginName})) acknowledged an Unauthorized Access warning when attempting to create a user for {firstName} {lastName} ({loginName}).";

                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.UPDATE_USER,
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = _userClaimAccessor.Current.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaimAccessor.Current.OrganizationMasterId,
                    OrganizationPartyId = _userClaimAccessor.Current.OrganizationPartyId,
                    Message = message,
                    FromUserLoginName = _userClaimAccessor.Current.LoginName,
                    FromUserLoginId = _userClaimAccessor.Current.UserId,
                    FromUserRealpageId = _userClaimAccessor.Current.UserRealPageGuid.ToString(),
                    FromUserFirstName = _userClaimAccessor.Current.FirstName,
                    FromUserLastName = _userClaimAccessor.Current.LastName,
                    ToUserLoginName = loginName,
                    ToUserLoginId = userDetails?.UserId ?? 0,
                    ToUserFirstName = firstName,
                    ToUserLastName = lastName,
                    ToUserRealpageId = userRealpageStr
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Activity log failed in {Method}", nameof(IsUserEmailDomainValidAsync));
            }
        }

        return isValid;
    }

    /// <inheritdoc/>
    public async Task<UserOrganizationExists> IsLoginNameExistsAsync(
        string loginName, Guid organizationRealPageId, Guid userRealPageId,
        string? firstName = null, string? lastName = null,
        int userType = 0, bool isFromExport = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(loginName))
            throw new ArgumentException("Invalid parameter loginName.", nameof(loginName));
        if (organizationRealPageId == Guid.Empty)
            throw new ArgumentException("Null organizationRealPageId.", nameof(organizationRealPageId));

        loginName = loginName.Trim();

        var orgDetails = await _organizationRepository.GetOrganizationAsync(realPageId: organizationRealPageId);
        var result = new UserOrganizationExists();

        var userPersonaOrgList = await _userLoginRepository.ListOrganizationByLoginNameAsync(loginName, null);

        if (isFromExport && userPersonaOrgList.Any()
            && !userType.Equals((int)UserRoleType.UserNoEmail))
        {
            var existingRoleTypes = userPersonaOrgList.Select(m => m.PartyRoleTypeId);
            if (!existingRoleTypes.Any(m => m == (int)UserRoleType.UserNoEmail))
            {
                var withOrgList = await _userLoginRepository.ListOrganizationByLoginNameAsync(loginName, organizationRealPageId);
                if (withOrgList.Count == 0)
                {
                    bool isExternalEverywhere = userPersonaOrgList.All(x => x.PartyRoleTypeId == (int)UserRoleType.ExternalUser);
                    if (userType == (int)UserRoleType.ExternalUser
                        || (isExternalEverywhere && userType == (int)UserRoleType.User))
                        userPersonaOrgList = withOrgList;
                }
            }
        }

        result.IsValidDomainUsername = await IsUserEmailDomainValidAsync(loginName, firstName, lastName, userRealPageId, cancellationToken);
        result.UserExistsAsAdminInOtherDomain = false;
        result.OrgIsRealpageEmployee = orgDetails.RealPageId == EmployeeCompanyRealPageId;
        result.UserExists = userPersonaOrgList is { Count: > 0 };
        result.UserExistsInThisOrganization = userPersonaOrgList?.Any(a => a.OrganizationRealPageId == organizationRealPageId) ?? false;
        result.UserExistsAsNoEmail = userPersonaOrgList?.Any(p => p.PartyRoleTypeId == (int)UserRoleType.UserNoEmail) ?? false;

        if (userPersonaOrgList?.Count > 0)
        {
            result.UserIsExternalEverywhere = userPersonaOrgList.All(x => x.PartyRoleTypeId == (int)UserRoleType.ExternalUser);

            var thisOrgStatus = userPersonaOrgList.FirstOrDefault(p => p.OrganizationRealPageId == organizationRealPageId);
            if (thisOrgStatus is not null && !thisOrgStatus.PrimaryOrganization)
            {
                result.Restricted = new Dictionary<string, List<string>>
                {
                    ["Fields"] = ["FirstName", "MiddleName", "LastName", "LoginName"],
                    ["Tabs"] = ["resetPassword", "securityQuestions"]
                };
            }
        }

        if (result.UserExists)
        {
            var ulo = await _userLoginRepository.GetUserLoginOnlyAsync(loginName);
            result.Person = new Person { RealPageId = ulo.RealPageId };

            var primaryOrg = userPersonaOrgList!.FirstOrDefault(c => c.PrimaryOrganization);
            var supervisorInfo = await _userRepository.GetSuperVisorInformationAsync(ulo.UserId, _userClaimAccessor.Current.OrganizationPartyId, cancellationToken);
            result.SuperVisor = supervisorInfo ?? new UserInfoLite();
            result.SuperVisor.UserId = ulo.UserId;
            result.SuperVisor.IsReadOnly = primaryOrg is not null && _userClaimAccessor.Current.OrganizationPartyId != primaryOrg.OrganizationPartyId;

            var primaryUserOrg = userPersonaOrgList.FirstOrDefault(m => m.PrimaryOrganization);
            if (primaryUserOrg is not null)
            {
                if (primaryUserOrg.OrganizationRealPageId != DefaultUserClaim.EmployeeCompanyRealPageId
                    && primaryUserOrg.OrganizationRealPageId != DefaultUserClaim.ExternalCompanyRealPageId
                    && primaryUserOrg.OrganizationRealPageId != DefaultUserClaim.ContractCompanyRealPageId)
                    result.PrimaryCompanyName = primaryUserOrg.OrganizationName;

                var userLogin = await GetUserLoginCoreAsync(ulo.RealPageId, primaryUserOrg.OrganizationPartyId, null, cancellationToken);
                if (userLogin is not null)
                    result.UserIsDisabledInPrimaryCompany = userLogin.StatusId == (int)UserUiStatusType.Disabled;
            }

            if (result.OrgIsRealpageEmployee && !result.UserExistsInThisOrganization)
                result.Person = await _personRepository.GetPersonAsync(ulo.RealPageId, cancellationToken);

            var userRoles = await _roleTypeRepository.GetRoleTypeAsync("User Role", _userClaimAccessor.Current.OrganizationPartyId, cancellationToken);
            if (userRoles.All(c => c.PartyRoleTypeId != (int)UserRoleType.ExternalUser))
            {
                result.UserExistsNotAvailable = true;
                return result;
            }

            var person = await _personRepository.GetPersonAsync(ulo.RealPageId, cancellationToken);
            if (person is not null) result.Person = person;
        }

        if (result.UserExists && !result.UserExistsInThisOrganization)
        {
            var primaryUserOrg = userPersonaOrgList!.FirstOrDefault(m => m.PrimaryOrganization);
            bool isAdmin = primaryUserOrg?.PartyRoleTypeId == (int)UserRoleType.SuperUser;
            bool isRegular = primaryUserOrg?.PartyRoleTypeId == (int)UserRoleType.User;

            if (primaryUserOrg is not null && (isAdmin || isRegular)
                && primaryUserOrg.BooksCustomerMasterId == orgDetails.BooksCustomerMasterId)
            {
                var orgDomains = await _organizationRepository.GetOrganizationListByBooksCustomerMasterIdAsync(orgDetails.BooksCustomerMasterId);
                if (orgDomains.Count > 1)
                {
                    result.UserExists = true;
                    result.UserExistsAsAdminInOtherDomain = isAdmin;
                    result.UserExistsAsRegularUserInOtherDomain = isRegular;
                }
            }
        }

        return result;
    }

    #endregion

    #region Write — UserLogin

    /// <inheritdoc/>
    public async Task<RepositoryResponse> CreateUserLoginAsync(
        Guid realPageId, IUserLogin userLogin, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userLogin);
        if (realPageId == Guid.Empty) throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));

        if (!string.IsNullOrEmpty(userLogin.Password))
        {
            var pwd = userLogin.Password.PasswordHash();
            userLogin.PasswordHash = pwd.PasswordHash;
            userLogin.PasswordSalt = pwd.PasswordSalt;
        }

        return await _userLoginRepository.CreateUserLoginAsync(realPageId, userLogin);
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserLoginAsync(
        Guid realPageId, IUserLogin userLogin, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userLogin);
        if (realPageId == Guid.Empty) throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));

        if (!string.IsNullOrEmpty(userLogin.Password))
        {
            var pwd = userLogin.Password.PasswordHash();
            userLogin.PasswordHash = pwd.PasswordHash;
            userLogin.PasswordSalt = pwd.PasswordSalt;
        }

        var statusType = userLogin switch
        {
            { IsActive: true } => UserUiStatusType.Active,
            { IsActive: false } => UserUiStatusType.Disabled,
            { IsLocked: true } => UserUiStatusType.Locked,
            { IsLocked: false } => UserUiStatusType.Unlocked,
            _ => UserUiStatusType.Active
        };

        if (userLogin.RealPageId != Guid.Empty)
            await CreateUpdateUserStatusAsync(userLogin.RealPageId, statusType, cancellationToken);

        return await _userLoginRepository.UpdateUserLoginAsync(realPageId, userLogin, _userClaimAccessor.Current.OrganizationPartyId);
    }

    /// <inheritdoc/>
    public async Task<IList<RepositoryResponse>> UpdateUserLoginsAsync(
        IList<UserLogin> userLogins, UserUiStatusType userLoginStatusType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userLogins);

        var responses = new List<RepositoryResponse>(userLogins.Count);
        foreach (var userLogin in userLogins)
        {
            bool ok = await CreateUpdateUserStatusAsync(userLogin.RealPageId, userLoginStatusType, cancellationToken);
            if (ok) responses.Add(new RepositoryResponse { Id = userLogin.UserId });
        }
        return responses;
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateBulkUserLoginsAsync(
        IList<UserLoginOnly> userLogins, UserUiStatusType userLoginStatusType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userLogins);

        var response = new RepositoryResponse();
        if (userLogins.Count == 0) return response;

        DateTime fromUtcDateTime = DateTime.UtcNow;
        DateTime? thruUtcDateTime = null;
        int statusTypeId = 0;

        switch (userLoginStatusType)
        {
            case UserUiStatusType.Locked:
                thruUtcDateTime = DateTime.MaxValue;
                statusTypeId = (int)UserUiStatusType.Locked;
                break;
            case UserUiStatusType.Unlocked:
            case UserUiStatusType.Active:
                thruUtcDateTime = null;
                statusTypeId = (int)UserUiStatusType.Active;
                break;
            case UserUiStatusType.Disabled:
                thruUtcDateTime = null;
                statusTypeId = (int)UserUiStatusType.Disabled;
                break;
        }

        if (statusTypeId == 0) return response;

        var userRealPageIds = userLogins.Select(u => u.RealPageId).ToList();
        int result = await _userLoginRepository.UpdateBulkUserStatusAsync(
            userRealPageIds, statusTypeId, fromUtcDateTime, thruUtcDateTime, _userClaimAccessor.Current.OrganizationPartyId);

        if (result == -1)
        {
            response.ErrorMessage = "Error while updating bulk statuses.";
            return response;
        }

        if (userLoginStatusType == UserUiStatusType.Active)
        {
            // TODO: Add ActivateSalesForceUserAsync to IUserRepositoryAsync then call:
            // await _userRepository.ActivateSalesForceUserAsync(_defaultUserClaim.UserRealPageGuid, _defaultUserClaim.PersonaId, ul, true, cancellationToken);

            foreach (var userLogin in userLogins)
            {
                var userLoginOnly = await _userLoginRepository.GetUserLoginOnlyAsync(userLogin.RealPageId);
                var userLoginFull = await GetUserLoginCoreAsync(userLogin.RealPageId, _userClaimAccessor.Current.OrganizationPartyId, null, cancellationToken);
                var orgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
                    userLoginOnly.UserId, userLoginOnly.LastLogin, _userClaimAccessor.Current.OrganizationPartyId, false);

                bool isUserExpired = (orgStatus.ThruDate is not null && DateTime.UtcNow > orgStatus.ThruDate)
                                          || (orgStatus.StatusThruDate is not null && DateTime.UtcNow > orgStatus.StatusThruDate);
                bool newUserwithActive = userLoginOnly.LastLogin is null && userLoginOnly.PasswordModifiedDate is not null && !isUserExpired;
                bool newUserWithFeatureDate = orgStatus.FromDate > DateTime.UtcNow;

                bool shouldSendEmail = orgStatus.PrimaryOrganization
                    && (newUserWithFeatureDate || (userLoginOnly.LastLogin is null && !userLoginOnly.Is3rdPartyIDP && orgStatus.Status != UserUiStatusType.Locked))
                    && !newUserwithActive;

                if (shouldSendEmail)
                {
                    bool? isNotified = _manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                        userLoginOnly, orgStatus.Name, (int)(userLoginFull?.UserRoleType ?? UserRoleType.User), orgStatus.PartyId);

                    var userDetailsInfo = await _userRepository.GetUserDetailsAsync(
                        userRealPageId: userLogin.RealPageId.ToString(), cancellationToken: cancellationToken);
                    IProfileDetail profile = new ProfileDetail();
                    profile.FirstName = userDetailsInfo.FirstName;
                    profile.LastName = userDetailsInfo.LastName;
                    profile.userLogin.LoginName = userDetailsInfo.LoginName;
                    profile.userLogin.UserId = userDetailsInfo.UserId;
                    profile.userLogin.RealPageId = userDetailsInfo.UserRealPageId;

                    string activityType = isNotified is true ? LogActivityTypeConstants.EMAIL_SENT : LogActivityTypeConstants.EMAIL_RESENT;
                    string message = isNotified is true
                        ? "Welcome Email sent to user {0} {1} by user {2}."
                        : "Unable to Resend Welcome Email to user {0} {1} by user {2}.";

                    LogAuditActivity(activityType, LogActivityCategoryType.Email, message, "UpdateBulkUserLogins", profile);
                }
            }
        }

        foreach (var userLogin in userLogins)
            await AddActivityLogAsync(userLogin, userLoginStatusType.ToString(), ProductEnum.UnifiedPlatform.ToEnumDescription(), _userClaimAccessor.Current, cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> UpdateLastLoginAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(username)) throw new ArgumentException("Invalid parameter username.", nameof(username));
        return _userLoginRepository.UpdateLastLoginAsync(username);
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> LinkIdentityProviderToUserLoginAsync(
        long personaId, long userId, int contactMechanismId, CancellationToken cancellationToken = default)
    {
        if (personaId <= 0) throw new ArgumentException("Missing Persona Id.", nameof(personaId));
        if (userId <= 0) throw new ArgumentException("Missing UserLogin Id.", nameof(userId));
        if (contactMechanismId <= 0) throw new ArgumentException("Missing Contact Mechanism Id.", nameof(contactMechanismId));

        return _userLoginRepository.LinkIdentityProviderToUserLoginAsync(personaId, userId, contactMechanismId);
    }

    #endregion

    #region Status management

    /// <inheritdoc/>
    public async Task<bool> CreateUpdateUserStatusAsync(
        Guid realPageId, UserUiStatusType uiStatusType, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty) throw new ArgumentException("realPageId cannot be empty.", nameof(realPageId));

        DateTime fromUtcDateTime = DateTime.UtcNow;
        DateTime? thruUtcDateTime = null;
        int statusTypeId = (int)MapUiStatusToDb(uiStatusType);
        bool? isNotified = null;
        bool newUserWithFeatureDate = false;
        bool isUserExpired = false;
        bool newUserwithActiveStatus = false;
        UserLoginOnly? userLoginOnly = null;
        OrganizationStatus orgStatus = new();

        // Sync: TODO replace with ICredentialRepositoryAsync.GetActivitiesAsync once available
        var activity = GetNewUserRegistrationActivity(_userClaimAccessor.Current.OrganizationPartyId);
        if (activity is not null)
            thruUtcDateTime = fromUtcDateTime.AddMinutes(activity.ActivityTokenExpirationMinutes);

        switch (uiStatusType)
        {
            case UserUiStatusType.Locked:
                thruUtcDateTime = DateTime.MaxValue;
                break;
            case UserUiStatusType.Unlocked:
                thruUtcDateTime = null;
                statusTypeId = (int)UserUiStatusType.Active;
                break;
            case UserUiStatusType.Expired:
                thruUtcDateTime = DateTime.UtcNow.AddMinutes(-1);
                statusTypeId = (int)UserUiStatusType.Expired;
                break;
            case UserUiStatusType.Disabled:
                thruUtcDateTime = null;
                statusTypeId = (int)UserUiStatusType.Disabled;
                break;
        }

        if (uiStatusType == UserUiStatusType.Active)
        {
            userLoginOnly = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
            var userLogin = await GetUserLoginCoreAsync(realPageId, _userClaimAccessor.Current.OrganizationPartyId, null, cancellationToken);

            if (userLoginOnly is not null)
            {
                orgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
                    userLoginOnly.UserId, userLoginOnly.LastLogin, _userClaimAccessor.Current.OrganizationPartyId, false);

                if (orgStatus.ThruDate is not null && DateTime.UtcNow > orgStatus.ThruDate) isUserExpired = true;
                if (orgStatus.StatusThruDate is not null && DateTime.UtcNow > orgStatus.StatusThruDate) isUserExpired = true;

                if (userLoginOnly.LastLogin is null && userLoginOnly.PasswordModifiedDate is not null && !isUserExpired)
                    newUserwithActiveStatus = true;

                fromUtcDateTime = orgStatus.FromDate;
                orgStatus.ThruDate = new DateTime(9999, 12, 31);

                if (orgStatus.FromDate > DateTime.UtcNow)
                {
                    orgStatus.FromDate = DateTime.UtcNow;
                    newUserWithFeatureDate = true;
                }

                var ul = new UserLogin
                {
                    LoginName = userLoginOnly.LoginName,
                    PasswordHash = userLoginOnly.PasswordHash,
                    PasswordSalt = userLoginOnly.PasswordSalt,
                    FromDate = orgStatus.FromDate,
                    ThruDate = orgStatus.ThruDate
                };
                await _userLoginRepository.UpdateUserLoginAsync(realPageId, ul, _userClaimAccessor.Current.OrganizationPartyId);

                bool shouldSendEmail = orgStatus.PrimaryOrganization
                    && (newUserWithFeatureDate || (userLoginOnly.LastLogin is null && !userLoginOnly.Is3rdPartyIDP && orgStatus.Status != UserUiStatusType.Locked))
                    && !newUserwithActiveStatus;

                if (shouldSendEmail)
                {
                    isNotified = _manageUserRegistrationEmail.SendNewUserRegistrationEmail(
                        userLoginOnly, orgStatus.Name, (int)(userLogin?.UserRoleType ?? UserRoleType.User), orgStatus.PartyId);
                    statusTypeId = (int)UserUiStatusType.Pending;
                }
                else
                {
                    statusTypeId = (int)UserUiStatusType.Active;
                    thruUtcDateTime = null;
                }
                // TODO: await _userRepository.ActivateSalesForceUserAsync(...) once added to IUserRepositoryAsync
            }
        }

        if (uiStatusType == UserUiStatusType.Unlocked)
        {
            var ul = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
            await _userRepository.UpdateUserActivityAttemptsAsync(
                ul.LoginName, ActivityType.UnlockUser, null, _userClaimAccessor.Current.OrganizationPartyId,
                cancellationToken: cancellationToken);
        }

        await _userRepository.UpdateUserStatusByCompanyAsync(
            realPageId, _userClaimAccessor.Current.OrganizationPartyId, statusTypeId, fromUtcDateTime, thruUtcDateTime, cancellationToken);

        if (uiStatusType is UserUiStatusType.Active or UserUiStatusType.Disabled or UserUiStatusType.Locked or UserUiStatusType.Unlocked)
        {
            var ul = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
            await AddActivityLogAsync(ul, uiStatusType.ToString(), ProductEnum.UnifiedPlatform.ToEnumDescription(), _userClaimAccessor.Current, cancellationToken);

            bool shouldLogEmail = orgStatus.PrimaryOrganization
                && (newUserWithFeatureDate || (userLoginOnly is not null && userLoginOnly.LastLogin is null && !userLoginOnly.Is3rdPartyIDP && orgStatus.Status != UserUiStatusType.Locked))
                && !newUserwithActiveStatus;

            if (shouldLogEmail)
            {
                var userDetailsInfo = await _userRepository.GetUserDetailsAsync(
                    userRealPageId: realPageId.ToString(), cancellationToken: cancellationToken);
                IProfileDetail profile = new ProfileDetail();
                profile.FirstName = userDetailsInfo.FirstName;
                profile.LastName = userDetailsInfo.LastName;
                profile.userLogin.LoginName = userDetailsInfo.LoginName;
                profile.userLogin.UserId = userDetailsInfo.UserId;
                profile.userLogin.RealPageId = userDetailsInfo.UserRealPageId;

                string actType = isNotified is true ? LogActivityTypeConstants.EMAIL_SENT : LogActivityTypeConstants.EMAIL_RESENT;
                string msg = isNotified is true
                    ? "Welcome Email sent to user {0} {1} by user {2}."
                    : "Unable to Resend Welcome Email to user {0} {1} by user {2}.";
                LogAuditActivity(actType, LogActivityCategoryType.Email, msg, nameof(CreateUpdateUserStatusAsync), profile);
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateActiveUserStatusAsync(
        Guid realPageId, UserUiStatusType uiStatusType, CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty) throw new ArgumentException("Null realPageId.", nameof(realPageId));

        if (uiStatusType != UserUiStatusType.Active) return true;

        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
        if (userLogin is null) return true;

        var orgList = await _userLoginRepository.ListOrganizationWithoutStatusByUserIdAsync(userLogin.UserId);
        var currentOrg = orgList.FirstOrDefault(p => p.RealPageId == _userClaimAccessor.Current.OrganizationRealPageGuid)
                      ?? orgList.FirstOrDefault();

        if (currentOrg is null) return true;

        if (_userClaimAccessor.Current.PersonaId == 0)
        {
            var persona = await _managePersona.GetFirstAvailablePersonaByCompanyAsync(realPageId, currentOrg.PartyId, cancellationToken);
            if (persona is not null)
                _userClaimAccessor.Current.PersonaId = persona.PersonaId;
        }

        await _userRepository.UpdateUserStatusByCompanyAsync(
            userLogin.RealPageId, currentOrg.PartyId, (int)UserUiStatusType.Active, currentOrg.FromDate, null, cancellationToken);

        // TODO: await _userRepository.ActivateSalesForceUserAsync(...) once added to IUserRepositoryAsync

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ResendInvitationAsync(
        IList<UserLogin> userLogins, bool isCalledFromService = false, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var user in userLogins)
            {
                var profileDetail = await _manageProfile.GetProfileDetailAsync(user.RealPageId, _userClaimAccessor.Current.OrganizationPartyId);
                var userLogin = profileDetail.userLogin;

                if (userLogin is null
                    || userLogin.UserRoleType == UserRoleType.UserNoEmail
                    || (!userLogin.IsExpired!.Value && !userLogin.IsPending!.Value && !isCalledFromService))
                    continue;

                int statusTypeId = (int)MapUiStatusToDb(UserUiStatusType.Pending);
                var userFromDate = userLogin.FromDate!.Value;

                DateTime thruUtcDateTime = userFromDate.AddHours(72);
                var registrationActivity = GetNewUserRegistrationActivity(_userClaimAccessor.Current.OrganizationPartyId);
                if (registrationActivity is not null)
                    thruUtcDateTime = DateTime.UtcNow.Date.AddMinutes(registrationActivity.ActivityTokenExpirationMinutes);

                await _userLoginRepository.UpdateUserLoginAsync(
                    userLogin.RealPageId, userLogin, _userClaimAccessor.Current.OrganizationPartyId);

                bool isNotified = _manageUserRegistrationEmail.SendNewUserRegistrationEmail(profileDetail);

                string userName = _userClaimAccessor.Current.LoginName?.Length == 0
                    ? "notification service"
                    : !string.IsNullOrEmpty(_userClaimAccessor.Current.ImpersonatedByName)
                        ? $"RealPage Access ({_userClaimAccessor.Current.ImpersonatedByName})"
                        : $"{_userClaimAccessor.Current.FirstName} {_userClaimAccessor.Current.LastName}";
                string message = isCalledFromService
                    ? (isNotified
                        ? $"Welcome Email send to user {userLogin.LoginName} by user {userName}."
                        : $"Unable to send Welcome Email to user {userLogin.LoginName} by user {userName}.")
                    : (isNotified
                        ? $"Resent Welcome Email to user {userLogin.LoginName} by user {userName}."
                        : $"Unable to send Welcome Email to user {userLogin.LoginName} by user {userName}.");

                try
                {
                    LogActivity.WriteActivity(new ActivityDetails
                    {
                        LogActivityTypeName = isCalledFromService ? LogActivityTypeConstants.EMAIL_SENT : LogActivityTypeConstants.EMAIL_RESENT,
                        LogCategoryName = LogActivityCategoryType.Email.ToString(),
                        CorrelationId = _userClaimAccessor.Current.CorrelationId.ToString(),
                        BooksMasterOrganizationId = _userClaimAccessor.Current.OrganizationMasterId,
                        OrganizationPartyId = _userClaimAccessor.Current.OrganizationPartyId,
                        Message = message,
                        FromUserLoginName = userName,
                        FromUserLoginId = _userClaimAccessor.Current.UserId,
                        FromUserRealpageId = _userClaimAccessor.Current.UserRealPageGuid.ToString(),
                        FromUserFirstName = _userClaimAccessor.Current.FirstName,
                        FromUserLastName = _userClaimAccessor.Current.LastName,
                        ToUserLoginName = profileDetail.userLogin.LoginName,
                        ToUserLoginId = profileDetail.userLogin.UserId,
                        ToUserFirstName = profileDetail.FirstName,
                        ToUserLastName = profileDetail.LastName,
                        ToUserRealpageId = profileDetail.userLogin.RealPageId.ToString()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Activity log failed in {Method}", nameof(ResendInvitationAsync));
                }

                await _userRepository.UpdateUserStatusByCompanyAsync(
                    userLogin.RealPageId, _userClaimAccessor.Current.OrganizationPartyId,
                    statusTypeId, userFromDate, thruUtcDateTime, cancellationToken);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Method} failed", nameof(ResendInvitationAsync));
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ClearPasswordAndQuestionsAsync(Guid realPageId, CancellationToken cancellationToken = default)
    {
        var profileDetail = await _manageProfile.GetProfileDetailAsync(realPageId, _userClaimAccessor.Current.OrganizationPartyId);
        var userLogin = profileDetail.userLogin;

        if (userLogin is null || userLogin.UserRoleType == UserRoleType.UserNoEmail)
            return false;

        var spResponse = await _credentialRepository.ResetEnterpriseUserCredentialAsync(
            realPageId, null, null, _userClaimAccessor.Current.OrganizationPartyId);
        if (spResponse.Id == 0)
        {
            LogResetPasswordActivity(false, (ProfileDetail) profileDetail);
            return false;
        }

        DateTime thruUtcDateTime = DateTime.UtcNow.Date.AddHours(72);
        var registrationActivity = GetNewUserRegistrationActivity(_userClaimAccessor.Current.OrganizationPartyId);
        if (registrationActivity is not null)
            thruUtcDateTime = DateTime.UtcNow.Date.AddMinutes(registrationActivity.ActivityTokenExpirationMinutes);

        userLogin.PasswordHash = null;
        userLogin.PasswordSalt = null;

        var updateResponse = await _userLoginRepository.UpdateUserLoginAsync(
            realPageId, userLogin, _userClaimAccessor.Current.OrganizationPartyId);
        if (!string.IsNullOrEmpty(updateResponse.ErrorMessage))
        {
            LogResetPasswordActivity(false, (ProfileDetail)profileDetail);
            return false;
        }

        bool emailSent = _manageUserRegistrationEmail.SendPasswordResetEmail((ProfileDetail) profileDetail);
        if (emailSent)
        {
            int statusTypeId = (int)MapUiStatusToDb(UserUiStatusType.Pending);
            await _userRepository.UpdateUserStatusByCompanyAsync(
                userLogin.RealPageId, _userClaimAccessor.Current.OrganizationPartyId,
                statusTypeId, userLogin.FromDate!.Value, thruUtcDateTime, cancellationToken);
        }

        LogResetPasswordActivity(emailSent, (ProfileDetail)profileDetail);
        return emailSent;
    }

    /// <inheritdoc/>
    public async Task LogUserRequestedEmailLinkResentAsync(Guid userRealPageId, CancellationToken cancellationToken = default)
    {
        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(userRealPageId);
        var orgWithoutStatus = await _userLoginRepository.GetPrimaryOrgWithoutStatusByUserIdAsync(userLogin.UserId);
        var orgWithStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
            userLogin.UserId, userLogin.LastLogin, orgWithoutStatus.PartyId, false);
        var profileDetail = await _manageProfile.GetProfileDetailAsync(userRealPageId, orgWithStatus.PartyId);

        string message = $"User {profileDetail.FirstName} {profileDetail.LastName} requested a new activation link";
        LogAuditActivity(LogActivityTypeConstants.USER_REQUESTED_NEW_ACTIVATION_LINK,
            LogActivityCategoryType.Email, message, null, profileDetail);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Core implementation of GetUserLogin — shared by the two public overloads.
    /// </summary>
    private async Task<UserLogin?> GetUserLoginCoreAsync(
        Guid realPageId, long orgPartyId, UserLogin? userLogin, CancellationToken ct)
    {
        if (realPageId != Guid.Empty)
            userLogin = await _userLoginRepository.GetUserLoginAsync(realPageId, orgPartyId);

        if (userLogin is not null)
        {
            if (realPageId != Guid.Empty && userLogin.UserRoleType is null && userLogin.RealPageId != Guid.Empty)
            {
                var orgList = await _userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(userLogin.RealPageId, "User Type");
                var orgUserType = orgList.FirstOrDefault(o => o.PartyId == orgPartyId
                    && o.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase));

                if (orgUserType is not null)
                {
                    string userType = Regex.Replace(orgUserType.RoleNameFrom, @"[^A-Za-z0-9]+", "").ToUpper();
                    if (Enum.TryParse<UserRoleType>(userType, true, out var roleType))
                        userLogin.UserRoleType = roleType;
                }
                else
                {
                    _logger.LogError("{Method} - No user type for UserId={UserId}", nameof(GetUserLoginCoreAsync), userLogin.UserId);
                }
            }

            userLogin.IsSuperUser = userLogin.UserRoleType == UserRoleType.SuperUser;

            if (realPageId == Guid.Empty)
                userLogin = UserLoginStatus.SetUserLoginStatus(userLogin);
        }

        return userLogin;
    }

    /// <summary>
    /// Builds a <see cref="DefaultUserClaim"/> from the org's admin user for automated processes.
    /// </summary>
    private async Task<DefaultUserClaim> GetCurrentUserClaimAsync(Organization org, CancellationToken ct)
    {
        var adminRealPageId = await _organizationRepository.GetOrganizationAdminUserRealPageIdAsync(org.RealPageId);

        if (adminRealPageId != Guid.Empty)
        {
            var adminLogin = await _userLoginRepository.GetUserLoginOnlyAsync(adminRealPageId);
            var adminProfile = await _manageProfile.GetProfileDetailAsync(adminRealPageId, org.PartyId);

            return new DefaultUserClaim
            {
                OrganizationMasterId = org.BooksMasterId,
                OrganizationPartyId = org.PartyId,
                FirstName = adminProfile.FirstName,
                LastName = adminProfile.LastName,
                UserId = Convert.ToInt32(adminLogin.UserId),
                LoginName = adminLogin.LoginName,
                UserRealPageGuid = adminLogin.RealPageId,
                CorrelationId = _userClaimAccessor.Current.CorrelationId
            };
        }

        return new DefaultUserClaim
        {
            OrganizationMasterId = org.BooksMasterId,
            OrganizationPartyId = org.PartyId,
            FirstName = "Automated",
            LastName = "System",
            UserId = 1,
            LoginName = "automatedsystem",
            UserRealPageGuid = Guid.Empty,
            CorrelationId = _userClaimAccessor.Current.CorrelationId
        };
    }

    /// <summary>
    /// Fetches the NewUserRegistration activity from the sync credential repository.
    /// TODO: Move to <c>ICredentialRepositoryAsync.GetActivitiesAsync</c> once available.
    /// </summary>
    private Activity? GetNewUserRegistrationActivity(long organizationPartyId)
    {
        try
        {
            var activities = _credentialRepository.GetActivitiesAsync(organizationPartyId).Result;
            return activities?.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GetActivities failed for org {OrgId} — thruDate will use default", organizationPartyId);
            return null;
        }
    }

    private async Task AddActivityLogAsync(
        UserLoginOnly userLogin, string activityTypeName, string booksProductCode,
        DefaultUserClaim claim, CancellationToken ct, string activityMessage = "")
    {
        var person = await _personRepository.GetPersonAsync(userLogin.RealPageId, ct);
        var userLoginTo = await _userLoginRepository.GetUserLoginOnlyAsync(userLogin.RealPageId);

        (string activity, string[] logTypes) = activityTypeName.ToLowerInvariant() switch
        {
            "active" => ("Activated", new[] { LogActivityTypeConstants.LOGIN_ENABLED }),
            "disabled" => ("Deactivated", new[] { LogActivityTypeConstants.LOGIN_DISABLED }),
            "locked" => ("Locked", new[] { LogActivityTypeConstants.USER_LOCKED }),
            "unlocked" => ("Unlocked", new[] { LogActivityTypeConstants.USER_UNLOCKED }),
            "expired" => ("Expired", new[] { LogActivityTypeConstants.USER_EXPIRED }),
            "pending" => ("Pending", new[] { LogActivityTypeConstants.LOGIN_ENABLED }),
            _ => (string.Empty, Array.Empty<string>())
        };

        if (string.IsNullOrEmpty(activity)) return;

        string message = string.IsNullOrEmpty(activityMessage)
            ? claim.ImpersonatedBy == Guid.Empty
                ? $"{claim.FirstName} {claim.LastName} {activity} user {person.FirstName} {person.LastName}."
                : $"RealPage Access ({claim.ImpersonatedByName}) {activity} user {person.FirstName} {person.LastName}."
            : string.Format(activityMessage, person.FirstName, person.LastName);

        foreach (string logType in logTypes)
        {
            try
            {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = logType,
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = claim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = claim.OrganizationMasterId,
                    OrganizationPartyId = claim.OrganizationPartyId,
                    Message = message,
                    FromUserLoginName = claim.LoginName,
                    FromUserLoginId = claim.UserId,
                    FromUserFirstName = claim.FirstName,
                    FromUserLastName = claim.LastName,
                    FromUserRealpageId = claim.UserRealPageGuid.ToString(),
                    ToUserLoginId = userLoginTo.UserId,
                    ToUserLoginName = userLoginTo.LoginName,
                    ToUserFirstName = person.FirstName,
                    ToUserLastName = person.LastName,
                    ToUserRealpageId = userLoginTo.RealPageId.ToString(),
                    BooksProductCode = booksProductCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Activity log failed for type {LogType}", logType);
            }
        }
    }

    private void LogAuditActivity(
        string logActivityType, LogActivityCategoryType categoryType,
        string message, string? stepName, IProfileDetail profile)
    {
        try
        {
            string userName = string.IsNullOrEmpty(_userClaimAccessor.Current.ImpersonatedByName)
                ? $"{_userClaimAccessor.Current.FirstName} {_userClaimAccessor.Current.LastName}"
                : $"RealPage Access ({_userClaimAccessor.Current.ImpersonatedByName})";

            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = categoryType.ToString(),
                CorrelationId = _userClaimAccessor.Current.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaimAccessor.Current.OrganizationMasterId,
                OrganizationPartyId = _userClaimAccessor.Current.OrganizationPartyId,
                Message = string.Format(message, profile.FirstName, profile.LastName, userName, profile.CreateUserSourceType.ToString()),
                FromUserLoginName = _userClaimAccessor.Current.LoginName,
                FromUserLoginId = _userClaimAccessor.Current.UserId,
                FromUserRealpageId = _userClaimAccessor.Current.UserRealPageGuid.ToString(),
                FromUserFirstName = _userClaimAccessor.Current.FirstName,
                FromUserLastName = _userClaimAccessor.Current.LastName,
                ToUserLoginName = profile.userLogin.LoginName,
                ToUserLoginId = profile.userLogin.UserId,
                ToUserFirstName = profile.FirstName,
                ToUserLastName = profile.LastName,
                ToUserRealpageId = profile.userLogin.RealPageId.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit log failed in step {Step}", stepName);
        }
    }

    private void LogResetPasswordActivity(bool isSuccess, ProfileDetail profileDetail)
    {
        string editorName = _userClaimAccessor.Current.ImpersonatedBy == Guid.Empty
            ? $"{_userClaimAccessor.Current.FirstName} {_userClaimAccessor.Current.LastName}"
            : $"RealPage Access ({_userClaimAccessor.Current.ImpersonatedByName})";

        string message = isSuccess
            ? $"{editorName} successfully initiated password reset for {profileDetail.FirstName} {profileDetail.LastName}."
            : $"An exception occurred when {editorName} was updating the password for {profileDetail.FirstName} {profileDetail.LastName}.";

        try
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = LogActivityTypeConstants.EMAIL_RESETPASSWORDSENT,
                LogCategoryName = LogActivityCategoryType.Email.ToString(),
                CorrelationId = _userClaimAccessor.Current.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaimAccessor.Current.OrganizationMasterId,
                OrganizationPartyId = _userClaimAccessor.Current.OrganizationPartyId,
                Message = message,
                BooksProductCode = "UPFM",
                FromUserLoginName = _userClaimAccessor.Current.LoginName,
                FromUserLoginId = _userClaimAccessor.Current.UserId,
                FromUserRealpageId = _userClaimAccessor.Current.UserRealPageGuid.ToString(),
                FromUserFirstName = _userClaimAccessor.Current.FirstName,
                FromUserLastName = _userClaimAccessor.Current.LastName,
                ToUserLoginName = profileDetail.userLogin.LoginName,
                ToUserLoginId = profileDetail.userLogin.UserId,
                ToUserFirstName = profileDetail.FirstName,
                ToUserLastName = profileDetail.LastName,
                ToUserRealpageId = profileDetail.userLogin.RealPageId.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Activity log failed in {Method}", nameof(LogResetPasswordActivity));
        }
    }

    private static UserDbStatusType MapUiStatusToDb(UserUiStatusType uiStatus) => uiStatus switch
    {
        UserUiStatusType.Pending or
        UserUiStatusType.Expired or
        UserUiStatusType.AccountCreationSuccessful => UserDbStatusType.Pending,

        UserUiStatusType.Active or
        UserUiStatusType.Disabled => UserDbStatusType.Active,

        UserUiStatusType.Locked or
        UserUiStatusType.Unlocked => UserDbStatusType.Locked,

        UserUiStatusType.ForceResetPassword => UserDbStatusType.ForceResetPassword,
        _ => default
    };

    #endregion
}