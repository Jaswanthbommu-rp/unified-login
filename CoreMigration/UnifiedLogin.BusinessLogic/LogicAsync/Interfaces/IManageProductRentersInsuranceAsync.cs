using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.RentersInsurance;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for Renters Insurance product user management.
/// <para>
/// Replaces <c>IManageProductRentersInsurance</c> (sync) and the legacy
/// <c>DefaultUserClaim</c>-parameterised stub.  All user context is resolved
/// internally via <c>IProductContextServiceAsync</c> — callers no longer pass
/// <c>DefaultUserClaim</c> per method.
/// </para>
/// <para>
/// The <c>out List&lt;AdditionalParameters&gt;</c> parameters on
/// <c>ManageRentersInsuranceUser</c> and <c>ChangeRentersInsuranceUserType</c>
/// are replaced with tuple returns — <c>out</c> is incompatible with <c>async</c>.
/// </para>
/// </summary>
public interface IManageProductRentersInsuranceAsync
{
    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all BluBook properties available to the company, with
    /// <c>IsAssigned</c> flags merged from the insurance API when
    /// <paramref name="userPersonaId"/> is non-zero and the user has a product record.
    /// </summary>
    Task<ListResponse> ListPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all properties known to the insurance product for the company's PMC ID,
    /// cross-referenced with BlueBook property instances so <c>IsActive</c> is set
    /// only for properties registered in the insurance API.
    /// </summary>
    Task<ObjectListOutput<PropertyInstance, IErrorData>> ListPropertiesByPMCIDAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all available insurance roles (with UI display-name overrides applied)
    /// wrapped in a <see cref="ListResponse"/>.  <c>IsAssigned</c> is set for the role
    /// currently held by the user when <paramref name="userPersonaId"/> is non-zero
    /// and the user has a product record.
    /// </summary>
    Task<ListResponse> ListRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── Create / Update ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates the insurance product user for <paramref name="userPersonaId"/>.
    /// <para>
    /// Returns a tuple so audit parameters are available to the caller without requiring
    /// an <c>out</c> parameter (incompatible with <c>async</c>).
    /// </para>
    /// </summary>
    Task<(ObjectOutput<UserAPIResponse, IErrorData> result, List<AdditionalParameters> auditParams)> ManageRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default);

    /// <summary>
    /// Delegates to <see cref="ManageRentersInsuranceUserAsync"/> — kept as a
    /// named alias used by the user-type-change batch flow.
    /// </summary>
    Task<(ObjectOutput<UserAPIResponse, IErrorData> result, List<AdditionalParameters> auditParams)> ChangeRentersInsuranceUserTypeAsync(
        long createUserPersonaId,
        long assignUserPersonaId,
        RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList,
        BatchProcessType batchProcessType,
        CancellationToken ct = default);

    // ── Enable / Disable / Unassign / Unlock ──────────────────────────────────

    /// <summary>Disables the user's account in the insurance API.</summary>
    Task<ObjectOutput<UserAPIResponse, IErrorData>> DisableRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    /// <summary>Re-enables the user's account in the insurance API.</summary>
    Task<ObjectOutput<UserAPIResponse, IErrorData>> EnableRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    /// <summary>
    /// Disables the user in the insurance API and marks their product status as
    /// <c>Deleted</c> in GreenBook.
    /// </summary>
    Task<ObjectOutput<UserAPIResponse, IErrorData>> UnassignRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    /// <summary>Unlocks a locked-out insurance user account.</summary>
    Task<ObjectOutput<UserAPIResponse, IErrorData>> UnlockRentersInsuranceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default);

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the list of insurance users available for migration, filtered and
    /// paged via <paramref name="datafilter"/>.
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

    // ── Status Toggle ─────────────────────────────────────────────────────────

    /// <summary>
    /// Enables or disables an insurance user by their internal product <paramref name="userId"/>
    /// (not persona ID).  Used by the migration portal's bulk status-toggle endpoint.
    /// </summary>
    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId,
        int userId,
        bool isActive = false,
        CancellationToken ct = default);
}
