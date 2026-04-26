using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for Resident Portal product user management.
/// <para>
/// Replaces <c>IManageProductResidentPortal</c> (sync) and the legacy
/// <c>DefaultUserClaim</c>-parameterised stub.  All user context is resolved
/// internally via <c>IProductContextServiceAsync</c> — callers no longer pass
/// <c>DefaultUserClaim</c> per method.
/// </para>
/// <para>
/// The <c>out List&lt;AdditionalParameters&gt;</c> parameter on
/// <c>ManageResidentPortalUser</c> is replaced with a tuple return —
/// <c>out</c> is incompatible with <c>async</c>.
/// </para>
/// </summary>
public interface IManageProductResidentPortalAsync
{
    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all Resident Portal properties available to the company,
    /// with <c>IsAssigned</c> flags merged from the RP API when
    /// <paramref name="userPersonaId"/> is non-zero and the user has a product record.
    /// </summary>
    Task<ListResponse> ListPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    // ── Notifications ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the notification settings for the user's Resident Portal account,
    /// or deactivated batch data when the user has no active product record.
    /// </summary>
    Task<Notifications?> GetNotificationSettingsAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── User ──────────────────────────────────────────────────────────────────

    /// <summary>Returns the Resident Portal user object for the given persona.</summary>
    Task<ResidentPortalUser?> GetUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── Level / Group helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Populates <paramref name="residentPortalUser"/>'s <c>MessagingGroups</c> and
    /// <c>Levels</c> collections from the RP API, then returns the updated object.
    /// </summary>
    Task<IResidentPortalUser> SetLevelAndGroupObjectsAsync(
        long editorPersonaId,
        long userPersonaId,
        IResidentPortalUser residentPortalUser,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the available level/role list with <c>IsAssigned</c> set for the user's
    /// current role, and <c>IsDisabled</c> flags applied based on the editor's role.
    /// </summary>
    Task<List<ILevel>> ListLevelsAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the five standard messaging groups with <c>IsAssigned</c> flags
    /// merged from the user's current RP settings.
    /// </summary>
    Task<List<IMessagingGroups>> ListMessageGroupsAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    /// <summary>Returns the static list of Resident Portal staff titles.</summary>
    Task<List<ITitle>> ListTitlesAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── Create / Update ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates the Resident Portal user for <paramref name="userPersonaId"/>.
    /// <para>
    /// Returns a tuple so audit parameters are available to the caller without requiring
    /// an <c>out</c> parameter (incompatible with <c>async</c>).
    /// </para>
    /// </summary>
    Task<(ObjectOutput<IResidentPortalUser, IErrorData> result, List<AdditionalParameters> auditParams)> ManageResidentPortalUserAsync(
        long editorPersonaId,
        long userPersonaId,
        ResidentPortal residentPortal,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default);

    // ── Unassign ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes the user from all Resident Portal communities and clears their
    /// SAML product attributes; marks product status as <c>Deleted</c> in GreenBook.
    /// </summary>
    Task<ObjectOutput<IResidentPortalUser, IErrorData>> UnassignResidentPortalUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── Delete ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Deletes a user directly by product user ID / username (used by migration portal).
    /// </summary>
    Task<bool> DeleteUserAsync(
        long editorPersonaId,
        int productUserId,
        string productUsername,
        CancellationToken ct = default);

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the list of Resident Portal users available for migration,
    /// filtered and paged via <paramref name="datafilter"/>.
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>Updates the unified-login migration flag for the supplied user list.</summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);
}
