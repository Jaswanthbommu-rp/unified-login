using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.Interfaces;

/// <summary>
/// Orchestration service — all methods that coordinate multiple SPs,
/// external services (ManagePersona, ManageBlueBook, ManageUnifiedSettings)
/// or contain business rules that were previously inside UserRepository.
///
/// Migration from UserRepository to this service is required for:
///
///   CreateUser            — 400-line transaction (person/login/persona/products/role)
///   UpdateNewUser         — token validation + profile write
///   UpdateUser            — delegates to UpdateUserData (~600 lines)
///   UpdateUserListUser    — persona add/update/delete + user-type update
///   DisableUserProduct    — Parallel.ForEach + product-batch disable per org
///   ActivateUserProducts  — requeue product batches per persona
///   AssignProductsToAdministrators — bulk admin product refresh
///   ActivateSalesForceUser — product batch for SF re-activation
///   ProcessDisabledUsers  — windows-service scheduled user disabling
///   ProcessDisableUserProductData — product batch disable + SP list
///   InsertNewPhoneNumberFromImport — phone import within a caller's transaction
///   ThirdPartyIdpBulkUpdateAsync — DB update + audit activity log
///   GetUnifiedSettingData  — ManageUnifiedSettings call (not a repository call)
/// </summary>
public interface IUserService
{
    Task<CreateUserResponse<IErrorData>> CreateUserAsync(
        ProfileDetail newProfile, IList<Persona> persona,
        CancellationToken cancellationToken = default);

    Task<RepositoryResponse> UpdateNewUserAsync(
        string userLogin, Profile newProfile, int partyRoleTypeId,
        string companyJobTitle, string activityToken,
        CancellationToken cancellationToken = default);

    Task<RepositoryResponse> UpdateUserAsync(
        Guid loggedInUserRealPageId, IProfileDetail newProfile, IProfileDetail oldProfile,
        CancellationToken cancellationToken = default);

    Task<RepositoryResponse> UpdateUserListUserAsync(
        ProfileDetail userProfile, IList<Persona> updatePersona,
        IList<Persona> deletePersona, int userTypeId, IList<Organization> listOrg,
        CancellationToken cancellationToken = default);

    Task DisableUserProductAsync(
        Guid createUserRealPageId, long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default);

    Task ActivateUserProductsAsync(
        Guid createUserRealPageId, long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default);

    Task AssignProductsToAdministratorsAsync(
        Guid organizationRealPageId, long assignUserPersonaId = 0,
        CancellationToken cancellationToken = default);

    Task ActivateSalesForceUserAsync(
        Guid createUserRealPageId, long createUserPersonaId,
        IList<UserLoginOnly> userLogins, bool isAssigned,
        CancellationToken cancellationToken = default);

    Task ProcessDisabledUsersAsync(
        IList<ProcessUserLogin> userLogins,
        CancellationToken cancellationToken = default);

    Task ProcessDisableUserProductDataAsync(
        long assignUserPersonaId, Guid createUserRealPageId,
        long createUserPersonaId, int? userTypeId, long impersonatorUserId,
        CancellationToken cancellationToken = default);

    Task InsertNewPhoneNumberFromImportAsync(
        IProfileDetail profile, System.Data.IDbTransaction tx,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <see cref="IUserRepositoryAsync.ThirdPartyIdpBulkUpdateAsync"/>
    /// then audits each updated user via LogActivity.
    /// </summary>
    Task<RepositoryResponse> ThirdPartyIdpBulkUpdateAsync(
        IList<long> userIds, bool isEnabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces: <c>UserRepository.GetUnifiedSettingData(string)</c> — calls
    /// ManageUnifiedSettings which is a service, not a repository.
    /// </summary>
    Task<bool> GetUnifiedSettingDataAsync(string settingName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Full implementation of UpdateUserStatusByCompany including the
    /// "if disabled → SP_ListPersonaToDisableUserProduct → ProcessDisableUserProductData"
    /// orchestration loop that the repository version omits.
    /// </summary>
    Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(
        Guid realPageId, long organizationPartyId,
        int statusTypeId, DateTime fromDate, DateTime? thruDate,
        CancellationToken cancellationToken = default);
}