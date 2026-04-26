using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for RP Document Management (Document Director) product user management.
/// <para>
/// Replaces <c>IManageProductRPDocumentManagement</c> (sync) and the legacy
/// <c>DefaultUserClaim</c>-parameterised stub.  All user context is resolved
/// internally via <c>IProductContextServiceAsync</c> — callers no longer pass
/// <c>DefaultUserClaim</c> per method.
/// </para>
/// <para>
/// The <c>out List&lt;AdditionalParameters&gt;</c> parameter on
/// <c>ManageRPDMUser</c> is replaced with a tuple return —
/// <c>out</c> is incompatible with <c>async</c>.
/// </para>
/// </summary>
public interface IManageProductRPDocumentManagementAsync
{
    // ── Domain ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the Document Director domain for the company associated with
    /// <paramref name="editorPersonaId"/>.  The domain string is returned in
    /// <c>ListResponse.Additional</c>.
    /// </summary>
    Task<ListResponse> GetDomainAsync(
        long editorPersonaId,
        CancellationToken ct = default);

    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the available Document Director roles, with <c>IsAssigned</c> set
    /// for roles currently held by <paramref name="userPersonaId"/> when the user
    /// has a product record.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the available Document Director roles enriched with classifier
    /// property datasets, with <c>IsAssigned</c> flags merged from the user's
    /// current role/property assignments.
    /// </summary>
    Task<ListResponse> GetPropertyRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the classifier dataset (list of properties/entities) for a given
    /// <paramref name="roleId"/>, with <c>IsAssigned</c> flags merged from the
    /// user's current assignments.
    /// </summary>
    Task<ListResponse> GetRoleClassifierDatasetAsync(
        long editorPersonaId,
        long userPersonaId,
        string roleId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    // ── Create / Update ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates the Document Director user for <paramref name="userPersonaId"/>.
    /// <para>
    /// Returns a tuple so audit parameters are available to the caller without
    /// requiring an <c>out</c> parameter (incompatible with <c>async</c>).
    /// An empty <c>result</c> string indicates success; a non-empty string is an
    /// error message.
    /// </para>
    /// </summary>
    Task<(string result, List<AdditionalParameters> auditParams)> ManageRPDMUserAsync(
        long editorPersonaId,
        long userPersonaId,
        RolePropertyList rolePropertyEntityList,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the Document Director user profile (name / email) without
    /// changing roles or properties.
    /// </summary>
    Task<string> UpdateRPDMUserProfileAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── Unassign ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Disables the user in the Document Director API and marks their product
    /// status as <c>Deleted</c> in GreenBook.
    /// When <paramref name="productUserId"/> is non-zero the user is identified
    /// by that ID rather than persona lookup (used by migration portal).
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        int productUserId = 0,
        CancellationToken ct = default);

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the list of Document Director users available for migration,
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
