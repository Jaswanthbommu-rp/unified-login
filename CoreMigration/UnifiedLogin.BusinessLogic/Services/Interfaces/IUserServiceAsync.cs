using System.Data;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using SO = UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.Interfaces;

/// <summary>
/// Async-first orchestration service for all user lifecycle operations.
///
/// Replaces the sync <see cref="UnifiedLogin.BusinessLogic.Repository.Interfaces.IUserRepository"/>
/// for every method that coordinates more than one stored-procedure call,
/// calls an external service (ManagePersona, ManageBlueBook, ManageUnifiedSettings),
/// or contains business rules.
///
/// Boundary rules:
/// <list type="bullet">
///   <item>Single SP read  → <see cref="Repository.Interfaces.IUserRepositoryAsync"/></item>
///   <item>Multi-SP / external-service / transactional → THIS interface</item>
///   <item>Audit logging   → <see cref="Audit.IUserAuditService"/> (called internally)</item>
/// </list>
///
/// Mapping from legacy sync methods:
/// <code>
/// Legacy sync (UserRepository)                  → Async replacement here
/// ─────────────────────────────────────────────────────────────────────────
/// CreateUser(ProfileDetail, IList{Persona})      → CreateUserAsync
/// UpdateNewUser(string, Profile, ...)            → UpdateNewUserAsync
/// UpdateUser(Guid, IProfileDetail, IProfileDetail)  → UpdateUserAsync
/// UpdateUserListUser(ProfileDetail, ...)         → UpdateUserListUserAsync
/// DisableUserProduct(Guid, long, IList{...})     → DisableUserProductAsync
/// ActivateUserProducts(Guid, long, IList{...})   → ActivateUserProductsAsync
/// AssignProductsToAdministrators(Guid, long)     → AssignProductsToAdministratorsAsync
/// ActivateSalesForceUser(Guid, long, IList{...}) → ActivateSalesForceUserAsync
/// ProcessDisabledUsers(IList{ProcessUserLogin})  → ProcessDisabledUsersAsync
/// ProcessDisableUserProductData(IRepository,...) → ProcessDisableUserProductDataAsync
///   ↑ IRepository param removed — service owns the connection via IConnectionFactory
/// InsertNewPhoneNumberFromImport(IRepository,...) → InsertNewPhoneNumberFromImportAsync
///   ↑ IRepository param replaced by IDbTransaction — caller controls transaction scope
/// ThirdPartyIdpBulkUpdate(IList{long}, bool)     → ThirdPartyIdpBulkUpdateAsync
/// GetUnifiedSettingData(string)                  → GetUnifiedSettingDataAsync
///   ↑ ManageUnifiedSettings is a service call, not a repository call
/// UpdateUserStatusByCompany(...)                 → UpdateUserStatusByCompanyAsync
///   ↑ The orchestration loop (disable products when status = Disabled) lives here
/// </code>
/// </summary>
public interface IUserServiceAsync
{
    // ── User creation ─────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new user with the full lifecycle:
    /// Person → UserLogin → Persona → Products → RoleType → BlueBook → Audit.
    /// Runs inside a single database transaction; rolls back on any step failure.
    /// </summary>
    /// <param name="newProfile">
    /// Complete profile including org, user-login, persona and product-batch data.
    /// </param>
    /// <param name="persona">
    /// Ordered list of personas to create for the new user. May be empty for
    /// cloned users where persona is derived from the clone source.
    /// </param>
    /// <returns>
    /// <see cref="CreateUserResponse{T}"/> with <c>Status.Success = true</c> and
    /// populated <c>UserId</c>/<c>PersonaId</c> on success, or <c>Status.Success = false</c>
    /// with <c>ErrorCode</c> / <c>ErrorMsg</c> on any business-rule or DB failure.
    /// </returns>
    Task<CreateUserResponse<IErrorData>> CreateUserAsync(
        ProfileDetail newProfile,
        IList<Persona> persona,
        CancellationToken cancellationToken = default);

    // ── Profile updates ───────────────────────────────────────────────────────

    /// <summary>
    /// Completes first-time profile setup after a new-user registration e-mail
    /// link is clicked. Validates the one-time activity token, then writes the
    /// starter profile fields (name, phone, job title) to the database.
    /// </summary>
    /// <param name="userLogin">Login name (e-mail) of the registering user.</param>
    /// <param name="newProfile">Starter profile fields submitted by the user.</param>
    /// <param name="partyRoleTypeId">Role type to assign on completion.</param>
    /// <param name="companyJobTitle">Free-text job title chosen by the user.</param>
    /// <param name="activityToken">
    /// One-time token from the registration e-mail.
    /// Returns an error response when invalid or expired.
    /// </param>
    Task<RepositoryResponse> UpdateNewUserAsync(
        string userLogin,
        Profile newProfile,
        int partyRoleTypeId,
        string companyJobTitle,
        string activityToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Full user-profile update: contact mechanism, user-type, persona list,
    /// product-batch, BlueBook, and audit-log — all inside one transaction.
    /// Delegates internally to the private <c>UpdateUserData</c> pipeline.
    /// </summary>
    /// <param name="loggedInUserRealPageId">
    /// RealPageId of the editor (logged-in admin). Used to resolve editor-persona
    /// for product batch operations and audit attribution.
    /// </param>
    /// <param name="newProfile">Updated profile as submitted by the UI.</param>
    /// <param name="oldProfile">
    /// Snapshot of the profile retrieved from the DB before the edit started.
    /// Used to diff field changes for the audit log.
    /// </param>
    Task<RepositoryResponse> UpdateUserAsync(
        Guid loggedInUserRealPageId,
        IProfileDetail newProfile,
        IProfileDetail oldProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user-type and persona list from the user-list grid (bulk edit path).
    /// Supports adding new personas, updating existing ones, deleting removed ones,
    /// and changing the user-type relationship — all in one transaction.
    /// </summary>
    /// <param name="userProfile">Profile detail for the user being edited.</param>
    /// <param name="updatePersona">Personas to create or update.</param>
    /// <param name="deletePersona">Personas to remove.</param>
    /// <param name="userTypeId">New <see cref="UserRoleType"/> to assign.</param>
    /// <param name="listOrg">Organisations the user belongs to (first entry used as primary).</param>
    Task<RepositoryResponse> UpdateUserListUserAsync(
        ProfileDetail userProfile,
        IList<Persona> updatePersona,
        IList<Persona> deletePersona,
        int userTypeId,
        IList<Organization> listOrg,
        CancellationToken cancellationToken = default);

    // ── Product enable / disable ──────────────────────────────────────────────

    /// <summary>
    /// Disables all product-batch entries for each user in <paramref name="userLogins"/>.
    /// Runs the per-user disable pipeline concurrently (bounded parallelism).
    /// </summary>
    /// <param name="createUserRealPageId">
    /// RealPageId of the admin performing the operation (editor identity).
    /// </param>
    /// <param name="createUserPersonaId">PersonaId of the admin (editor persona).</param>
    /// <param name="userLogins">Users whose products should be disabled.</param>
    Task DisableUserProductAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-activates product-batch entries for each user in <paramref name="userLogins"/>,
    /// re-queuing the batches that were saved when the products were previously disabled.
    /// </summary>
    /// <param name="createUserRealPageId">RealPageId of the admin performing the operation.</param>
    /// <param name="createUserPersonaId">PersonaId of the admin.</param>
    /// <param name="userLogins">Users whose products should be re-activated.</param>
    Task ActivateUserProductsAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns any products the company has that an admin user is missing.
    /// Iterates over all admin personas for <paramref name="organizationRealPageId"/>
    /// and runs the product-batch assign pipeline for each gap found.
    /// Called after a new product is provisioned for a company.
    /// </summary>
    /// <param name="organizationRealPageId">The company to refresh admin products for.</param>
    /// <param name="assignUserPersonaId">
    /// PersonaId to use as the editor/assigner identity.
    /// Defaults to 0, which resolves the company's first RP-employee admin.
    /// </param>
    Task AssignProductsToAdministratorsAsync(
        Guid organizationRealPageId,
        long assignUserPersonaId = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the Salesforce-specific product-batch pipeline for each user in
    /// <paramref name="userLogins"/>, activating or deactivating the SF product
    /// based on <paramref name="isAssigned"/>.
    /// </summary>
    /// <param name="createUserRealPageId">RealPageId of the admin/editor.</param>
    /// <param name="createUserPersonaId">PersonaId of the admin/editor.</param>
    /// <param name="userLogins">Users whose SF product access should change.</param>
    /// <param name="isAssigned">
    /// <c>true</c> to activate the Salesforce product;
    /// <c>false</c> to deactivate.
    /// </param>
    Task ActivateSalesForceUserAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        bool isAssigned,
        CancellationToken cancellationToken = default);

    // ── Scheduled / batch operations ─────────────────────────────────────────

    /// <summary>
    /// Processes a batch of users whose accounts have passed their
    /// <c>ThruDate</c> and should be deactivated. Called by the scheduled
    /// Windows Service on a timed interval.
    /// For each user:
    /// <list type="number">
    ///   <item>Updates status to Disabled via SP_UpdateUserStatusByCompany.</item>
    ///   <item>Calls <see cref="ProcessDisableUserProductDataAsync"/> to remove products.</item>
    ///   <item>Writes an activity-log entry describing the auto-deactivation.</item>
    /// </list>
    /// </summary>
    /// <param name="userLogins">
    /// List of users to process, pre-filtered to those past their ThruDate.
    /// </param>
    Task ProcessDisabledUsersAsync(
        IList<ProcessUserLogin> userLogins,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all active product-batch entries for a given persona and writes
    /// a snapshot of the removed batches so they can be restored on re-activation.
    /// Creates a <c>BatchProcessorGroup</c> record to correlate the disable event.
    /// </summary>
    /// <remarks>
    /// The legacy signature accepted an <c>IRepository repository</c> parameter so
    /// the caller could share an open transaction. The async version owns its own
    /// connection via <see cref="DataAccess.IConnectionFactory"/>; callers that need
    /// this to participate in a larger transaction should call
    /// <see cref="ProcessDisableUserProductDataInTransactionAsync"/> instead.
    /// </remarks>
    /// <param name="assignUserPersonaId">PersonaId of the user whose products are being disabled.</param>
    /// <param name="createUserRealPageId">RealPageId of the editor/admin driving the operation.</param>
    /// <param name="createUserPersonaId">PersonaId of the editor/admin.</param>
    /// <param name="userTypeId">User-role type of the user being disabled (affects which products are targeted).</param>
    /// <param name="impersonatorUserId">
    /// UserId of the impersonator if the action is performed via impersonation; 0 otherwise.
    /// </param>
    Task ProcessDisableUserProductDataAsync(
        long assignUserPersonaId,
        Guid createUserRealPageId,
        long createUserPersonaId,
        int? userTypeId,
        long impersonatorUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transaction-aware overload of <see cref="ProcessDisableUserProductDataAsync"/>.
    /// Accepts an open <see cref="IDbTransaction"/> so this step can participate in
    /// the caller's unit of work (e.g. inside <see cref="ProcessDisabledUsersAsync"/>
    /// or <see cref="UpdateUserStatusByCompanyAsync"/>).
    /// </summary>
    Task ProcessDisableUserProductDataInTransactionAsync(
        long assignUserPersonaId,
        Guid createUserRealPageId,
        long createUserPersonaId,
        int? userTypeId,
        long impersonatorUserId,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default);

    // ── Import helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts or updates phone numbers for a user as part of a bulk import.
    /// Participates in the <b>caller's</b> transaction — the import job opens the
    /// transaction, calls this method for each user, then commits or rolls back.
    /// </summary>
    /// <remarks>
    /// The legacy signature accepted <c>IRepository repository</c>.
    /// The async version accepts <see cref="IDbTransaction"/> directly so callers
    /// do not need to hold a reference to the repository abstraction.
    /// </remarks>
    /// <param name="profile">Profile containing the phone numbers to insert.</param>
    /// <param name="transaction">Open transaction from the caller's unit of work.</param>
    Task InsertNewPhoneNumberFromImportAsync(
        IProfileDetail profile,
        IDbTransaction transaction,
        CancellationToken cancellationToken = default);

    // ── Bulk identity-provider update ─────────────────────────────────────────

    /// <summary>
    /// Bulk-updates the IDP flag for a list of users via <c>SP_UpdateUsersIDP</c>,
    /// then records an activity-log entry for each updated user.
    /// </summary>
    /// <remarks>
    /// The legacy <c>ThirdPartyIdpBulkUpdate</c> returned a bare
    /// <see cref="RepositoryResponse"/>.  The updated version returns the count of
    /// successfully updated records inside <c>RepositoryResponse.Id</c> to let
    /// callers detect partial failures.
    /// </remarks>
    /// <param name="userIds">Internal user IDs to update.</param>
    /// <param name="isEnabled"><c>true</c> to enable third-party IDP; <c>false</c> to disable.</param>
    Task<RepositoryResponse> ThirdPartyIdpBulkUpdateAsync(
        IList<long> userIds,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    // ── Status ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates a user's status for a specific company, then runs the
    /// product-disable pipeline when <paramref name="statusTypeId"/> equals
    /// <see cref="Enum.UserUiStatusType.Disabled"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Repository.Interfaces.IUserRepositoryAsync.UpdateUserStatusByCompanyAsync"/>
    /// executes only the single SP call.  This method adds the orchestration:
    /// list affected personas → call
    /// <see cref="ProcessDisableUserProductDataAsync"/> for each → audit-log.
    /// </remarks>
    /// <param name="realPageId">User's enterprise identifier.</param>
    /// <param name="organizationPartyId">Company party ID.</param>
    /// <param name="statusTypeId">New status value (<see cref="Enum.UserUiStatusType"/>).</param>
    /// <param name="fromDate">Effective start date of the new status.</param>
    /// <param name="thruDate">Effective end date; <c>null</c> means indefinite.</param>
    Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(
        Guid realPageId,
        long organizationPartyId,
        int statusTypeId,
        DateTime fromDate,
        DateTime? thruDate,
        CancellationToken cancellationToken = default);

    // ── Configuration / settings ─────────────────────────────────────────────

    /// <summary>
    /// Returns a boolean company-level unified setting.
    /// Delegates to <c>IManageUnifiedSettingsAsync</c> — this is a service call,
    /// not a repository call, which is why it belongs here rather than in
    /// <see cref="Repository.Interfaces.IUserRepositoryAsync"/>.
    /// Returns <c>false</c> when the setting does not exist or cannot be read.
    /// </summary>
    /// <param name="settingName">
    /// Setting key name, e.g. <c>"delegateadministrators"</c>.
    /// </param>
    Task<bool> GetUnifiedSettingDataAsync(
        string settingName,
        CancellationToken cancellationToken = default);
}
