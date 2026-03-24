using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Credential Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// </summary>
public sealed class CredentialRepositoryAsync : ICredentialRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ILogger<CredentialRepositoryAsync> _logger;

    #endregion

    #region Constructor

    public CredentialRepositoryAsync(
        IDbConnection db,
        ILogger<CredentialRepositoryAsync> logger)
    {
        _db     = db     ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region ICredentialRepositoryAsync Implementation

    /// <inheritdoc/>
    public async Task<string> UpdateEnterpriseUserCredentialAsync(
        string enterpriseUserName,
        string newPasswordHash,
        string passwordSalt,
        string correctAnswerToken,
        int activityId,
        long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateEnterpriseUserCredential,
                new { enterpriseUserName, correctAnswerToken, activityTypeId = activityId, newPasswordHash, passwordSalt, partyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<SecurityQuestion>> GetUserSecurityQuestionAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<SecurityQuestion>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserSecurityQuestionAnswers,
                new { enterpriseUserName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<SecurityQuestionAnswer>> GetUserSecurityQuestionAnswerAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<SecurityQuestionAnswer>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserSecurityQuestionAnswer,
                new { enterpriseUserName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<ActivityAttemptDetails> GetActivityAttemptExceedsAsync(
        long organizationPartyId,
        string enterpriseUserName,
        int activityId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<ActivityAttemptDetails>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetActivityAttemptExceeds,
                new { enterpriseUserName, activityTypeId = activityId, partyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<ActivityAttempt> UpdateUserActivityAttemptsAsync(
        string enterpriseUserName,
        ActivityType activityType,
        UserDeviceDetails userDeviceDetails,
        long organizationPartyId,
        string authenticationServiceId = "",
        CancellationToken cancellationToken = default)
    {
        // Replaces: if (userDeviceDetails == null) { userDeviceDetails = new UserDeviceDetails(); }
        userDeviceDetails ??= new UserDeviceDetails();

        var param = new
        {
            enterpriseUserName,
            activityTypeId          = (int)activityType,
            userDeviceDetails.BrowserName,
            userDeviceDetails.BrowserType,
            userDeviceDetails.IpAddress,
            userDeviceDetails.IsMobile,
            userDeviceDetails.Platform,
            userDeviceDetails.Version,
            userDeviceDetails.DeviceType,
            userDeviceDetails.Timezone,
            authenticationServiceId,
            partyId = organizationPartyId
        };

        return await _db.QuerySingleOrDefaultAsync<ActivityAttempt>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateActivityAttempt,
                param,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<TokenDetail> GetActivityTokenAsync(
        string enterpriseUserName,
        string activityToken,
        int activityId,
        long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<TokenDetail>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetActivityToken,
                new { enterpriseUserName, activityToken, activityTypeId = activityId, partyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<string> CreateActivityTokenAsync(
        long organizationPartyId,
        Guid realPageId,
        int activityId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateActivityToken,
                new { partyId = organizationPartyId, realPageId, activityTypeId = activityId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<PasswordDetail>> GetPasswordHistoryAsync(
        string enterpriseUserName,
        int numberOfPasswordsToRemember,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<PasswordDetail>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetPasswordHistory,
                new { enterpriseUserName, numberOfPasswordsToRemember },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> ResetEnterpriseUserCredentialAsync(
        Guid realPageId,
        string newPasswordHash,
        string newPasswordSalt,
        long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ResetEnterpriseUserCredential,
                new { realPageId, newPasswordHash, newPasswordSalt, partyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(
        Guid realPageId,
        long organizationPartyId,
        UserUiStatusType statusType,
        DateTime? fromDate,
        DateTime? thruDate,
        CancellationToken cancellationToken = default)
    {
        return await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                new
                {
                    RealPageId          = realPageId,
                    OrganizationPartyId = organizationPartyId,
                    StatusTypeId        = statusType,
                    FromDate            = fromDate,
                    StatusThruDate      = thruDate
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Multi-step write: UpdateUserLogin → ListActivity → UpdateStatus (conditional).
    /// All steps run in a single transaction — replaces the original single connection
    /// scope where <c>UpdateStatus</c> silently opened its own connection for status updates.
    /// </remarks>
    public async Task<RepositoryResponse> SetEnterpriseUserTemporaryPasswordAsync(
        Guid realPageId,
        long organizationPartyId,
        string newPasswordHash,
        string newPasswordSalt,
        UserLoginOnly user,
        OrganizationStatus organizationStatus,
        CancellationToken cancellationToken = default)
    {
        var response = new RepositoryResponse();

        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        try
        {
            // Step 1 — update login credentials
            var loginResponse = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateUserLogin,
                    new
                    {
                        RealPageId   = realPageId,
                        PasswordHash = newPasswordHash,
                        PasswordSalt = newPasswordSalt,
                        FromDate     = organizationStatus.FromDate,
                        ThruDate     = organizationStatus.ThruDate,
                        PartyId      = organizationStatus.PartyId
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken))
                ?? new RepositoryResponse();

            if (loginResponse.Id == 0)
            {
                tx.Rollback();
                response.ErrorMessage = "Update User Error: Update user login detail failed.";
                return response;
            }

            response.Id = loginResponse.Id;

            // Step 2 — resolve statusThruDate then update status conditionally
            var statusThruDate = DateTime.UtcNow.AddHours(72);

            if (organizationStatus.StatusTypeId == (int)UserUiStatusType.Expired
                || organizationStatus.StatusTypeId == (int)UserUiStatusType.Pending)
            {
                var activities = (await _db.QueryAsync<Activity>(
                    new CommandDefinition(
                        StoredProcNameConstants.SP_ListActivity,
                        new { PartyId = organizationPartyId },
                        transaction: tx,
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken))).ToList();

                var registration = activities
                    .FirstOrDefault(a => a.ActivityTypeId == (int)ActivityType.NewUserRegistration);

                if (registration is not null)
                    statusThruDate = DateTime.UtcNow.AddMinutes(registration.ActivityTokenExpirationMinutes);

                response = await UpdateStatusAsync(
                    realPageId, organizationPartyId,
                    UserUiStatusType.Pending,
                    organizationStatus.FromDate, statusThruDate,
                    response, tx, cancellationToken);
            }
            else if (organizationStatus.StatusTypeId == (int)UserUiStatusType.Active
                     || organizationStatus.StatusTypeId == (int)UserUiStatusType.Locked)
            {
                statusThruDate = DateTime.MaxValue.ToUniversalTime();

                response = await UpdateStatusAsync(
                    realPageId, organizationPartyId,
                    UserUiStatusType.ForceResetPassword,
                    organizationStatus.FromDate, statusThruDate,
                    response, tx, cancellationToken);
            }

            if (response.Id == 0)
            {
                tx.Rollback();
                return response;
            }

            tx.Commit();
            return response;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "{Method} failed for RealPageId={Id}", nameof(SetEnterpriseUserTemporaryPasswordAsync), realPageId);
            response.ErrorMessage = "There was a problem setting the temporary password";
            return response;
        }
    }

    /// <inheritdoc/>
    public async Task<IList<SecurityQuestion>> GetAllSecurityQuestionsAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<SecurityQuestion>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetAllSecurityQuestions,
                new { enterpriseUserName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<long> SaveSecurityQuestionAnswersAsync(
        UserSecurityAnswer userSecurityQuestionsAnswers,
        long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_SaveSecurityQuestionAnswers,
                new
                {
                    userSecurityQuestionsAnswers.EnterpriseUserName,
                    userSecurityQuestionsAnswers.ActivityToken,
                    activityTypeId      = (int)ActivityType.NewUserRegistrationVerification,
                    securityQuestion1Id = userSecurityQuestionsAnswers.SecurityQuestionAnswers[0].SecurityQuestionId,
                    securityAnswer1     = userSecurityQuestionsAnswers.SecurityQuestionAnswers[0].Answer,
                    securityQuestion2Id = userSecurityQuestionsAnswers.SecurityQuestionAnswers[1].SecurityQuestionId,
                    securityAnswer2     = userSecurityQuestionsAnswers.SecurityQuestionAnswers[1].Answer,
                    securityQuestion3Id = userSecurityQuestionsAnswers.SecurityQuestionAnswers[2].SecurityQuestionId,
                    securityAnswer3     = userSecurityQuestionsAnswers.SecurityQuestionAnswers[2].Answer,
                    partyId             = organizationPartyId
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();

        if (result.Id <= 0)
            throw new InvalidOperationException(result.ErrorMessage);

        return result.Id;
    }

    /// <inheritdoc/>
    public async Task<IList<Organization>> ListOrganizationByRealPageIdAsync(
        Guid userRealPageId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<Organization>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListOrganizationByRealPageId,
                new { realPageId = userRealPageId, relationshipTypeName = "User Type" },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<IdentityProviderType> GetIdentityProviderTypeByLoginNameAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        var authType = await _db.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName,
                new { loginName = enterpriseUserName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return new IdentityProviderType { AuthenticationType = authType };
    }

    /// <inheritdoc/>
    public async Task<IList<SecurityQuestion>> GetUserSelectedSecurityQuestionsAsync(
        Guid realpageUserId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<SecurityQuestion>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserSelectedSecurityQuestions,
                new { realpageId = realpageUserId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<long> SaveUserSelectedSecurityQuestionsAsync(
        Guid realpageId,
        IList<SecurityQuestionAnswer> securityQuestionAnswers,
        CancellationToken cancellationToken = default)
    {
        return await _db.ExecuteScalarAsync<long>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserSelectedSecurityQuestions,
                new
                {
                    realpageId,
                    questionId1 = securityQuestionAnswers[0].SecurityQuestionId,
                    answer1     = securityQuestionAnswers[0].Answer,
                    questionId2 = securityQuestionAnswers[1].SecurityQuestionId,
                    answer2     = securityQuestionAnswers[1].Answer,
                    questionId3 = securityQuestionAnswers[2].SecurityQuestionId,
                    answer3     = securityQuestionAnswers[2].Answer
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<IList<Activity>> GetActivitiesAsync(
        long organizationPartyId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<Activity>(
            new CommandDefinition(
                StoredProcNameConstants.SP_ListActivity,
                new { partyId = organizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Replaces: private <c>UpdateStatus</c> — now transaction-aware and fully async.
    /// </summary>
    private async Task<RepositoryResponse> UpdateStatusAsync(
        Guid realPageId,
        long organizationPartyId,
        UserUiStatusType statusType,
        DateTime fromDate,
        DateTime statusThruDate,
        RepositoryResponse current,
        IDbTransaction tx,
        CancellationToken cancellationToken)
    {
        var result = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                new
                {
                    RealPageId          = realPageId,
                    OrganizationPartyId = organizationPartyId,
                    StatusTypeId        = statusType,
                    FromDate            = fromDate,
                    StatusThruDate      = statusThruDate
                },
                transaction: tx,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken))
            ?? new RepositoryResponse();

        if (result.Id == 0)
        {
            current.ErrorMessage = "Update User Error: Update user status failed.";
            current.Id = 0;
        }
        else
        {
            current.Id = result.Id;
        }

        return current;
    }

    /// <summary>
    /// Opens the connection if not already open.
    /// Required before calling <see cref="IDbConnection.BeginTransaction"/>.
    /// </summary>
    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }

    #endregion
}