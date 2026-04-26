using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True-async interface for RealConnect product user management.
/// <para>
/// Replaces <c>ManageProductRealConnect</c>: the single <c>DefaultUserClaim</c>-bound
/// constructor is gone — per-call context is resolved internally via
/// <see cref="IProductContextServiceAsync"/> from the supplied persona IDs.
/// </para>
/// <para>
/// <c>RPObjectCache</c> → <c>IMemoryCache</c>;
/// <c>new HttpClient</c> → <c>IHttpClientFactory</c> (named <c>"RealConnect"</c>);
/// all blocking <c>.Result</c> calls → <c>await</c>.
/// </para>
/// </summary>
public interface IManageProductRealConnectAsync
{
    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full RealConnect role list for the company, merged with the
    /// roles currently assigned to <paramref name="userPersonaId"/>.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    // ── Properties (licenses) ─────────────────────────────────────────────────

    /// <summary>
    /// Returns the company's RealConnect license list (paged via cursor)
    /// split into learner and manager buckets, with <c>IsAssigned</c> flags
    /// set for any licenses already held by <paramref name="userPersonaId"/>.
    /// </summary>
    Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default);

    // ── User management ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates the RealConnect user for <paramref name="assignUserPersonaId"/>,
    /// including dual-role assignment and bulk learning-path assignment.
    /// Returns an empty string on success or a non-empty error message on failure.
    /// </summary>
    Task<string> CreateUpdateUserAsync(
        Guid createUserRealPageId,
        long createUserPersonaId, long assignUserPersonaId,
        object rolePropList,
        CancellationToken ct = default);

    /// <summary>
    /// Sets the RealConnect user status to <paramref name="userStatus"/>
    /// (<c>"disabled"</c> to deactivate, <c>"active"</c> to reactivate).
    /// Returns an empty string on success or a non-empty error message on failure.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        string userStatus = "disabled",
        CancellationToken ct = default);

    /// <summary>
    /// Pushes updated profile data (name, email) to RealConnect and refreshes
    /// the <c>ProductUsername</c> SAML attribute.
    /// Returns an empty string on success or a non-empty error message on failure.
    /// </summary>
    Task<string> UpdateProductUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default);
}
