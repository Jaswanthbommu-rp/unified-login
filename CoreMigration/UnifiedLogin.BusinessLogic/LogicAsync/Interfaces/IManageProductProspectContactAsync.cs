using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ProspectContactCenter;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True-async interface for Prospect Contact Center product user management.
/// <para>
/// Replaces <c>IManageProductProspectContact</c> which accepted a
/// <c>DefaultUserClaim</c>-bound constructor. Context is now resolved
/// per-call via injected async services.
/// </para>
/// <para>
/// The <c>out List&lt;AdditionalParameters&gt;</c> parameters on
/// <c>ChangeProspectContactUserType</c> and <c>ManageProductProspectContactUser</c>
/// are incompatible with async; both methods return a named tuple instead.
/// </para>
/// </summary>
public interface IManageProductProspectContactAsync
{
    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the product-specific property list for <paramref name="userPersonaId"/>,
    /// merged with the user's current PCC assignments when the user already exists
    /// in the product.
    /// </summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    // ── User management ───────────────────────────────────────────────────────

    /// <summary>
    /// Deactivates the user in PCC and marks product status as <c>Deleted</c>
    /// in GreenBook. Returns an empty string on success or an error message on failure.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Changes the user type for an existing PCC user by delegating to
    /// <see cref="ManageProductProspectContactUserAsync"/>.
    /// Replaces the <c>out List&lt;AdditionalParameters&gt;</c> overload.
    /// </summary>
    Task<(string error, List<AdditionalParameters> additionalParameters)> ChangeProspectContactUserTypeAsync(
        long createUserPersonaId, long assignUserPersonaId,
        ProspectContactPropertyRole roleProp,
        BatchProcessType batchProcessType,
        CancellationToken ct = default);

    /// <summary>
    /// Creates or updates a user in Prospect Contact Center.
    /// Returns <c>(error, additionalParameters)</c>; <c>error</c> is empty on success.
    /// Replaces the <c>out List&lt;AdditionalParameters&gt;</c> overload.
    /// </summary>
    Task<(string error, List<AdditionalParameters> additionalParameters)> ManageProductProspectContactUserAsync(
        long editorPersonaId, long userPersonaId,
        ProspectContactPropertyRole userProspectContactPropertyRole,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default);

    /// <summary>
    /// Updates the user's profile (name, email, login name) in PCC and refreshes
    /// the SAML <c>ProductUsername</c> attribute in GreenBook.
    /// Returns an empty string on success or an error message on failure.
    /// </summary>
    Task<string> UpdateProspectContactCenterUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Deactivates the PCC user identified by the numeric <paramref name="userId"/>.
    /// Returns <c>true</c> on success.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, int userId,
        CancellationToken ct = default);

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all PCC users for the company, excluding those already migrated into UL.
    /// </summary>
    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Sends a batch migration-status update to the PCC API and returns the result.
    /// </summary>
    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken ct = default);
}
