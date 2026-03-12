using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Services.User;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Audit.Dtos;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.Audit;

/// <summary>
/// Centralized user audit logging service with async patterns
/// Extracted from UserRepository audit methods
/// </summary>
public class UserAuditService : IUserAuditService
{
    private readonly DefaultUserClaim _userClaim;
    private readonly ILogger<UserAuditService> _logger;
    private readonly IUserQueryService _userQueryService;

    public UserAuditService(
        DefaultUserClaim userClaim,
        ILogger<UserAuditService> logger,
        IUserQueryService userQueryService)
    {
        _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userQueryService = userQueryService ?? throw new ArgumentNullException(nameof(userQueryService));
    }

    /// <summary>
    /// Logs general user activity (Async)
    /// </summary>
    public async Task LogActivityAsync(
        string logActivityType,
        LogActivityCategoryType category,
        string message,
        IProfileDetail profile,
        CancellationToken cancellationToken = default,
        List<AdditionalParameters> additionalInfo = null)
    {
        try
        {
            var userName = GetUserDisplayName();
            var sourceType = profile.CreateUserSourceType?.ToString() ?? CreateUserSourceType.UnifiedPlatform.ToString();

            var activityDetails = new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = category.ToString(),
                CorrelationId = _userClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                OrganizationPartyId = _userClaim.OrganizationPartyId,
                Message = string.Format(message,
                    profile.FirstName,
                    profile.LastName,
                    userName,
                    sourceType),

                FromUserLoginName = _userClaim.LoginName,
                FromUserLoginId = _userClaim.UserId,
                FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                FromUserFirstName = _userClaim.FirstName,
                FromUserLastName = _userClaim.LastName,

                ToUserLoginName = profile.userLogin?.LoginName ?? string.Empty,
                ToUserLoginId = profile.userLogin?.UserId ?? 0,
                ToUserFirstName = profile.FirstName,
                ToUserLastName = profile.LastName,
                ToUserRealpageId = profile.userLogin?.RealPageId.ToString() ?? string.Empty,

                AdditionalInformation = additionalInfo
            };

            await Task.Run(() => LogActivity.WriteActivity(activityDetails), cancellationToken);

            _logger.LogDebug("Logged activity: {ActivityType} for user {LoginName}",
                logActivityType, profile.userLogin?.LoginName);
        }
        catch (Exception ex)
        {
            // Don't throw - audit failures shouldn't break business operations
            _logger.LogWarning(ex, "Audit logging failed for {ActivityType}", logActivityType);
        }
    }

    /// <summary>
    /// Logs user profile update audit (Async)
    /// </summary>
    public async Task LogUserUpdateAsync(
        IProfileDetail oldProfile,
        IProfileDetail newProfile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var oldUser = oldProfile.IProfileDetailToUserAuditDto<UserAuditDto>();
            var newUser = newProfile.IProfileDetailToUserAuditDto<UserAuditDto>();

            newUser.UserType = ((UserRoleType)newProfile.UserTypeId).ToEnumDescription();
            oldUser.UserType = ((UserRoleType)oldProfile.UserTypeId).ToEnumDescription();

            var isRealPageEmployee = oldProfile.Persona[0].Organization.RealPageId == 
                DefaultUserClaim.EmployeeCompanyRealPageId;

            var auditResults = ExtensionMethods.GenerateUpdateAudit(
                oldUser,
                newUser,
                "user profile",
                isRealPageEmployee);

            // Log each audit result
            foreach (var audit in auditResults)
            {
                await LogFieldChangeAsync(
                    audit.OldValue?.ToString() ?? string.Empty,
                    audit.NewValue?.ToString() ?? string.Empty,
                    audit.ColumnName.ToString(),
                    audit.AuditMessage,
                    newProfile,
                    cancellationToken);
            }

            // Audit custom fields changes
            var customFieldAudits = ExtensionMethods.GetCustomFieldsAudit(
                oldProfile.CustomFields,
                newProfile.CustomFields);

            foreach (var audit in customFieldAudits)
            {
                await LogFieldChangeAsync(
                    audit.OldValue?.ToString() ?? string.Empty,
                    audit.NewValue?.ToString() ?? string.Empty,
                    audit.ColumnName.ToString(),
                    audit.AuditMessage,
                    newProfile,
                    cancellationToken);
            }

            // Audit 3rd Party IDP change
            if (oldProfile.userLogin.Is3rdPartyIDP != newProfile.userLogin.Is3rdPartyIDP)
            {
                var impersonatorInfo = _userClaim.ImpersonatedBy == Guid.Empty
                    ? null
                    : await _userQueryService.GetUserDetailsAsync(null, _userClaim.ImpersonatedBy.ToString(), cancellationToken);

                var message = impersonatorInfo != null
                    ? $"RealPage Access ({impersonatorInfo.FirstName} {impersonatorInfo.LastName}) updated Third party identity provider flag from {oldProfile.userLogin.Is3rdPartyIDP} to {newProfile.userLogin.Is3rdPartyIDP}."
                    : $"{_userClaim.FirstName} {_userClaim.LastName} updated Third party identity provider flag from {oldProfile.userLogin.Is3rdPartyIDP} to {newProfile.userLogin.Is3rdPartyIDP}.";

                await LogFieldChangeAsync(
                    oldProfile.userLogin.Is3rdPartyIDP.ToString(),
                    newProfile.userLogin.Is3rdPartyIDP.ToString(),
                    "Third party identity provider",
                    message,
                    newProfile,
                    cancellationToken);
            }

            _logger.LogInformation("User update audit logged for {LoginName}", newProfile.userLogin.LoginName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error logging user update audit");
        }
    }

    /// <summary>
    /// Logs bulk IDP update activity (Async)
    /// </summary>
    public async Task LogBulkIdpUpdateAsync(
        IList<long> userIds,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        if (userIds == null || !userIds.Any())
            return;

        try
        {
            var status = isEnabled ? "enabled" : "disabled";

            // Process each user's audit log
            var tasks = userIds.Select(async userId =>
            {
                try
                {
                    var userDetails = await _userQueryService.GetUserDetailsAsync(
                        null,
                        userId.ToString(),
                        cancellationToken);

                    if (userDetails == null)
                    {
                        _logger.LogWarning("User details not found for UserId {UserId} during IDP audit", userId);
                        return;
                    }

                    var profile = ConvertToProfileDetail(userDetails);
                    var message = GetIdpUpdateMessage(status, userDetails);

                    await LogFieldChangeAsync(
                        (!isEnabled).ToString(),
                        isEnabled.ToString(),
                        "Third-Party Identity Provider",
                        message,
                        profile,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to log IDP update for UserId {UserId}", userId);
                }
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation("Bulk IDP update audit logged for {Count} users", userIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in bulk IDP update audit");
        }
    }

    /// <summary>
    /// Logs individual field change (Async)
    /// </summary>
    public async Task LogFieldChangeAsync(
        string oldValue,
        string newValue,
        string fieldName,
        string message,
        IProfileDetail profile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var additionalInfo = new List<AdditionalParameters>
            {
                new() { Key = fieldName, Value = $"{{\"action\" : \"Updated To\", \"value\" : \"{newValue}\"}}" },
                new() { Key = fieldName, Value = $"{{\"action\" : \"Updated From\", \"value\" : \"{oldValue}\"}}" }
            };

            await LogActivityAsync(
                LogActivityTypeConstants.UPDATE_USER,
                LogActivityCategoryType.User,
                message,
                profile,
                cancellationToken,
                additionalInfo);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error logging field change for {FieldName}", fieldName);
        }
    }

    #region Private Helper Methods

    private string GetUserDisplayName()
    {
        return string.IsNullOrEmpty(_userClaim.ImpersonatedByName)
            ? $"{_userClaim.FirstName} {_userClaim.LastName}"
            : $"RealPage Access ({_userClaim.ImpersonatedByName})";
    }

    private string GetIdpUpdateMessage(string status, UserDetails userDetails)
    {
        var userName = GetUserDisplayName();
        return $"{userName} {status} Third-Party Identity Provider flag for user {userDetails.FirstName} {userDetails.LastName}";
    }

    private ProfileDetail ConvertToProfileDetail(UserDetails userDetails)
    {
        return new ProfileDetail
        {
            RealPageId = userDetails.RealPageId,
            FirstName = userDetails.FirstName,
            LastName = userDetails.LastName,
            userLogin = new UserLogin
            {
                UserId = userDetails.UserId,
                LoginName = userDetails.LoginName,
                RealPageId = userDetails.RealPageId
            },
            CreateUserSourceType = !string.IsNullOrEmpty(userDetails.CreateUserSourceType)
                ? Enum.Parse<CreateUserSourceType>(userDetails.CreateUserSourceType)
                : CreateUserSourceType.UnifiedPlatform
        };
    }

    #endregion
}