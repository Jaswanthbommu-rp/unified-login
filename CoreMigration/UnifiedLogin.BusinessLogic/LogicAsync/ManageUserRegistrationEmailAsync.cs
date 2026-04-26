using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first user registration and password-reset email service.
/// Replaces: sync <c>ManageUserRegistrationEmail</c> + any blocking <c>Task.Run</c> wrappers.
/// </summary>
public sealed class ManageUserRegistrationEmailAsync : IManageUserRegistrationEmailAsync
{
    #region Fields

    private readonly IManageEmailAsync _emailLogic;
    private readonly IManageContactMechanismAsync _contactMechanism;
    private readonly IManageCommunicationEventsAsync _communicationEvents;
    private readonly IUserTokenRepositoryAsync _userTokenRepository;
    private readonly IManagePersonAsync _managePerson;
    private readonly IUserLoginRepositoryAsync _userLoginRepository;
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepository;
    private readonly IUserClaimsAccessor _userClaims;
    private readonly ILogger<ManageUserRegistrationEmailAsync> _logger;

    private const string EmailNotificationUsage = "Email Notification";
    private const string IsSendGridEnabledKey = "IsSendGridEnabled";
    private const string IsUnifiedEmailEnabledKey = "IsUnifiedEmailEnabled";
    private const string RegistrationEmailCategory = "RegistrationEmail";

    #endregion

    #region Constructor

    public ManageUserRegistrationEmailAsync(
        IManageEmailAsync emailLogic,
        IManageContactMechanismAsync contactMechanism,
        IManageCommunicationEventsAsync communicationEvents,
        IUserTokenRepositoryAsync userTokenRepository,
        IManagePersonAsync managePerson,
        IUserLoginRepositoryAsync userLoginRepository,
        IProductInternalSettingRepositoryAsync productInternalSettingRepository,
        IUserClaimsAccessor userClaims,
        ILogger<ManageUserRegistrationEmailAsync> logger)
    {
        _emailLogic = emailLogic ?? throw new ArgumentNullException(nameof(emailLogic));
        _contactMechanism = contactMechanism ?? throw new ArgumentNullException(nameof(contactMechanism));
        _communicationEvents = communicationEvents ?? throw new ArgumentNullException(nameof(communicationEvents));
        _userTokenRepository = userTokenRepository ?? throw new ArgumentNullException(nameof(userTokenRepository));
        _managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _userClaims = userClaims ?? throw new ArgumentNullException(nameof(userClaims));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<bool> SendNewUserRegistrationEmailAsync(
        IProfileDetail profile,
        CancellationToken cancellationToken = default)
    {
        var userLoginOnly = new UserLoginOnly
        {
            Is3rdPartyIDP = profile.userLogin.Is3rdPartyIDP,
            LoginName = profile.userLogin.LoginName,
            RealPageId = profile.RealPageId,
            UserId = profile.userLogin.UserId,
            LastLogin = profile.userLogin.LastLogin
        };

        return await SendNewUserRegistrationEmailAsync(
            userLoginOnly,
            profile.organization[0].Name,
            profile.UserTypeId,
            profile.organization[0].PartyId,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> SendNewUserRegistrationEmailAsync(
        UserLoginOnly userLoginOnly,
        string companyName,
        int userTypeId,
        long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        var emailAddress = userLoginOnly.LoginName;

        if (!IsEligibleForRegistrationEmail(emailAddress, userTypeId, userLoginOnly.Is3rdPartyIDP))
            return true;

        try
        {
            // Phase 1 — independent fetches in parallel
            var personTask = _managePerson.GetPersonAsync(userLoginOnly.RealPageId, cancellationToken);
            var orgListTask = _userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(userLoginOnly.RealPageId, null);
            await Task.WhenAll(personTask, orgListTask);

            var firstName = personTask.Result.FirstName;
            var organizationList = orgListTask.Result;

            // Derive initial audience type from user role
            int audienceTypeId = (UserRoleType)userTypeId switch
            {
                UserRoleType.RealPageEmployee or UserRoleType.SuperUser => (int)CommunicationEventAudienceType.SuperUser,
                UserRoleType.User   => (int)CommunicationEventAudienceType.RegularUser,
                UserRoleType.ExternalUser => (int)CommunicationEventAudienceType.ExternalUser,
                _ => 0
            };

            int activityId = (int)ActivityType.NewUserRegistration;
            string userToken = string.Empty;

            var orgEntry = organizationList.FirstOrDefault(p => p is not null && p.PartyId == organizationPartyId);

            if (orgEntry?.PrimaryOrganization is true)
            {
                var primaryOrgStatus = await _userLoginRepository.GetUserOrganizationWithStatusAsync(
                    userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);

                if (primaryOrgStatus.IsPending is true
                    || primaryOrgStatus.IsExpired is true
                    || primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.Disabled)
                {
                    userToken = await _userTokenRepository.GetUserActivityTokenAsync(
                        userLoginOnly.RealPageId, activityId, organizationPartyId, cancellationToken);
                }
                else
                {
                    audienceTypeId = (int)CommunicationEventAudienceType.RegularUser;
                }
            }
            else
            {
                audienceTypeId = (int)CommunicationEventAudienceType.MultiCompanyUser;
            }

            var orgRealPageId = orgEntry?.RealPageId ?? Guid.Empty;
            var purposeTypeId = (int)CommunicationEventPurposeType.NewUserSetup;

            // Phase 2 — template + both contact-mechanism lists in parallel
            var templateTask = _emailLogic.GetEmailTemplateAsync(audienceTypeId, purposeTypeId, cancellationToken);
            var fromContactTask = _contactMechanism.ListContactMechanismForPersonAsync(orgRealPageId, EmailNotificationUsage, cancellationToken);
            var toContactTask = _contactMechanism.ListContactMechanismForPersonAsync(userLoginOnly.RealPageId, EmailNotificationUsage, cancellationToken);
            await Task.WhenAll(templateTask, fromContactTask, toContactTask);

            var emailTemplate = templateTask.Result;
            var fromContacts = fromContactTask.Result;
            var toContacts = toContactTask.Result;

            _logger.LogInformation("{ActionName} - email template generated - {RealPageId}",
                nameof(SendNewUserRegistrationEmailAsync), userLoginOnly.RealPageId);

            var senderEmailAddress = fromContacts[0].AddressString;
            var partyContactMechanismIdFrom = fromContacts[0].PartyContactMechanismId;
            var partyContactMechanismIdTo = toContacts[0].PartyContactMechanismId;

            var cesEmail = await _emailLogic.CreateWelcomeEmailAsync(
                userLoginOnly.LoginName, firstName, companyName, organizationPartyId,
                emailTemplate, userToken, senderEmailAddress, emailAddress, cancellationToken);

            if (cesEmail.EmailBody is not null)
            {
                _logger.LogInformation("{ActionName} - email body generated - {RealPageId} - {AdditionalInfo}",
                    nameof(SendNewUserRegistrationEmailAsync), userLoginOnly.RealPageId,
                    JsonConvert.SerializeObject(new { userToken, cesEmail, audienceTypeId }, Formatting.Indented));
            }

            var utcStarted = DateTime.UtcNow;
            string emailStatus;
            bool isSendGridEnabled;
#if DEBUG
            emailStatus = "success";
            isSendGridEnabled = false;
#else
            (emailStatus, isSendGridEnabled) = await GetEmailStatusAsync(userLoginOnly, cesEmail, firstName, cancellationToken);
#endif
            var utcEnded = DateTime.UtcNow;

            return await SaveCommunicationEventChainAsync(
                emailStatus, isSendGridEnabled,
                partyContactMechanismIdFrom, partyContactMechanismIdTo,
                utcStarted, utcEnded,
                emailTemplate.CommunicationEmailTemplateId,
                cesEmail.ClientUniqueID.ToString().ToUpper(),
                userLoginOnly.RealPageId,
                nameof(SendNewUserRegistrationEmailAsync),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - email generation failed - {RealPageId}",
                nameof(SendNewUserRegistrationEmailAsync), userLoginOnly.RealPageId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SendPasswordResetEmailAsync(
        ProfileDetail profileDetail,
        CancellationToken cancellationToken = default)
    {
        // Phase 1 — person + user-login in parallel (no mutual dependency)
        var personTask = _managePerson.GetPersonAsync(profileDetail.RealPageId, cancellationToken);
        var userLoginTask = _userLoginRepository.GetUserLoginOnlyAsync(profileDetail.RealPageId);
        await Task.WhenAll(personTask, userLoginTask);

        var firstName = personTask.Result.FirstName;
        var userLoginOnly = userLoginTask.Result;
        var emailAddress = userLoginOnly.LoginName;

        if (profileDetail.UserTypeId == UserTypeConstants.RegularUserNoEmail
            || userLoginOnly.Is3rdPartyIDP
            || !EmailFormatValidation.IsValidEmail(emailAddress))
        {
            return true;
        }

        try
        {
            int activityId = (int)ActivityType.NewUserRegistration;

            // Phase 2 — org-status + activity-token in parallel (both depend only on userLoginOnly)
            var orgStatusTask = _userLoginRepository.GetUserOrganizationWithStatusAsync(
                userLoginOnly.UserId, profileDetail.userLogin.LastLogin, 0, true);
            var userTokenTask = _userTokenRepository.GetUserActivityTokenAsync(
                userLoginOnly.RealPageId, activityId, _userClaims.OrganizationPartyId, cancellationToken);
            await Task.WhenAll(orgStatusTask, userTokenTask);

            var userToken = userTokenTask.Result;
            var audienceTypeId = (int)CommunicationEventAudienceType.RegularUser;
            var purposeTypeId = (int)CommunicationEventPurposeType.PasswordReset;

            // Phase 3 — template + both contact-mechanism lists in parallel
            var templateTask = _emailLogic.GetEmailTemplateAsync(audienceTypeId, purposeTypeId, cancellationToken);
            var fromContactTask = _contactMechanism.ListContactMechanismForPersonAsync(
                _userClaims.OrganizationRealPageGuid, EmailNotificationUsage, cancellationToken);
            var toContactTask = _contactMechanism.ListContactMechanismForPersonAsync(
                userLoginOnly.RealPageId, EmailNotificationUsage, cancellationToken);
            await Task.WhenAll(templateTask, fromContactTask, toContactTask);

            var emailTemplate = templateTask.Result;
            var fromContacts = fromContactTask.Result;
            var toContacts = toContactTask.Result;

            _logger.LogInformation("{ActionName} - email template generated - {RealPageId}",
                nameof(SendPasswordResetEmailAsync), userLoginOnly.RealPageId);

            var senderEmailAddress = fromContacts[0].AddressString;
            var partyContactMechanismIdFrom = fromContacts[0].PartyContactMechanismId;
            var partyContactMechanismIdTo = toContacts[0].PartyContactMechanismId;

            var cesEmail = await _emailLogic.CreateWelcomeEmailAsync(
                userLoginOnly.LoginName, firstName,
                _userClaims.OrganizationName, _userClaims.OrganizationPartyId,
                emailTemplate, userToken, senderEmailAddress, emailAddress, cancellationToken);

            if (cesEmail.EmailBody is not null)
            {
                _logger.LogInformation("{ActionName} - email body generated - {RealPageId} - {AdditionalInfo}",
                    nameof(SendPasswordResetEmailAsync), userLoginOnly.RealPageId,
                    JsonConvert.SerializeObject(new { userToken, cesEmail, audienceTypeId }, Formatting.Indented));
            }

            var utcStarted = DateTime.UtcNow;
            string emailStatus;
            bool isSendGridEnabled;
#if DEBUG
            emailStatus = "success";
            isSendGridEnabled = false;
#else
            (emailStatus, isSendGridEnabled) = await GetEmailStatusAsync(userLoginOnly, cesEmail, firstName, cancellationToken);
#endif
            var utcEnded = DateTime.UtcNow;

            return await SaveCommunicationEventChainAsync(
                emailStatus, isSendGridEnabled,
                partyContactMechanismIdFrom, partyContactMechanismIdTo,
                utcStarted, utcEnded,
                emailTemplate.CommunicationEmailTemplateId,
                cesEmail.ClientUniqueID.ToString().ToUpper(),
                userLoginOnly.RealPageId,
                nameof(SendPasswordResetEmailAsync),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ActionName} - email generation failed - {RealPageId} - {ErrorMessage}",
                nameof(SendPasswordResetEmailAsync), profileDetail.RealPageId, ex.Message);
            return false;
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Guards whether a registration email should be sent at all.
    /// Mirrors the outer if-condition from the sync implementation.
    /// </summary>
    private static bool IsEligibleForRegistrationEmail(string emailAddress, int userTypeId, bool is3rdPartyIdp)
        => EmailFormatValidation.IsValidEmail(emailAddress)
           && userTypeId != UserTypeConstants.RegularUserNoEmail
           && (((userTypeId == UserTypeConstants.SuperUser || userTypeId == UserTypeConstants.RegularUser) && !is3rdPartyIdp)
               || userTypeId == UserTypeConstants.ExternalUser);

    /// <summary>
    /// Resolves IsSendGridEnabled / IsUnifiedEmailEnabled from product settings, then dispatches
    /// the email via the correct channel. Returns the status string and the sendgrid flag
    /// (callers need the flag to decide whether to record the CES event).
    /// </summary>
    private async Task<(string status, bool isSendGridEnabled)> GetEmailStatusAsync(
        UserLoginOnly userLoginOnly,
        Email cesEmail,
        string firstName,
        CancellationToken cancellationToken)
    {
        var settings = await _productInternalSettingRepository.GetProductInternalSettingsAsync(
            (int)ProductEnum.UnifiedPlatform, cancellationToken);

        bool isSendGridEnabled = settings
            .FirstOrDefault(s => s.Name.Equals(IsSendGridEnabledKey, StringComparison.OrdinalIgnoreCase))
            ?.Value?.Equals("1") ?? false;

        var unifiedEmailEntry = settings
            .FirstOrDefault(s => s.Name.Equals(IsUnifiedEmailEnabledKey, StringComparison.OrdinalIgnoreCase));

        // Default to enabled when setting is absent (preserves original behaviour)
        bool isUnifiedEmailEnabled = unifiedEmailEntry is null || unifiedEmailEntry.Value.Trim() == "1";

        string status;

        if (isUnifiedEmailEnabled)
        {
            var emailModel = new EmailModel
            {
                Subject = cesEmail.EmailSubject,
                To = [new UserEmail { Email = cesEmail.EmailTo, Name = firstName }],
                Body = cesEmail.EmailBody,
                Bcc = []
            };
            status = await _emailLogic.SendEmailModelAsync(emailModel, cancellationToken) ? "success" : string.Empty;
        }
        else if (isSendGridEnabled)
        {
            ISendGridEmail sendGridEmail = new SendGridEmail
            {
                emailSubject = cesEmail.EmailSubject,
                fromAddress = new EmailAddress { email = cesEmail.EmailFrom, name = cesEmail.EmailFrom },
                toAddress = [new EmailAddress { email = cesEmail.EmailTo, name = cesEmail.EmailTo }],
                message = cesEmail.EmailBody,
                transId = userLoginOnly.UserId.ToString(),
                category = RegistrationEmailCategory
            };
            status = await _emailLogic.SendGridEmailAsync(sendGridEmail, cancellationToken);
        }
        else
        {
            status = await _emailLogic.SendEmailAsync(cesEmail, cancellationToken);
        }

        return (status, isSendGridEnabled);
    }

    /// <summary>
    /// Persists the communication-event record chain (event → template link → CES link).
    /// Extracted to avoid duplicating the identical 4-step sequence across both public methods.
    /// </summary>
    private async Task<bool> SaveCommunicationEventChainAsync(
        string emailStatus,
        bool isSendGridEnabled,
        long partyContactMechanismIdFrom,
        long partyContactMechanismIdTo,
        DateTime utcStarted,
        DateTime utcEnded,
        int communicationEmailTemplateId,
        string cesClientUniqueId,
        Guid realPageId,
        string actionName,
        CancellationToken cancellationToken)
    {
        bool success = emailStatus.Contains("success", StringComparison.OrdinalIgnoreCase);

        var communicationEventResponse = await _communicationEvents.CreateCommunicationEventAsync(
            (int)(success ? EmailStatusType.EmailSuccess : EmailStatusType.EmailError),
            partyContactMechanismIdFrom, partyContactMechanismIdTo,
            utcStarted, utcEnded, emailStatus, cancellationToken);

        if (!success)
        {
            _logger.LogInformation("{ActionName} - email generation failed - {RealPageId}", actionName, realPageId);
            return false;
        }

        _logger.LogInformation("{ActionName} - email sent - {RealPageId}", actionName, realPageId);

        if (communicationEventResponse.Id != 0)
        {
            communicationEventResponse = await _communicationEvents.CreateCommunicationEventEmailAsync(
                communicationEmailTemplateId, communicationEventResponse.Id, cancellationToken);
        }

        // CES link is skipped when SendGrid delivered the message (it tracks its own events)
        if (communicationEventResponse.Id != 0 && !isSendGridEnabled)
        {
            await _communicationEvents.CreateCESCommunicationEventEmailAsync(
                cesClientUniqueId, communicationEventResponse.Id, cancellationToken);
        }

        return true;
    }

    #endregion
}
