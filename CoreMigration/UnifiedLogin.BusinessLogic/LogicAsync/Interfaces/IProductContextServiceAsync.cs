using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Resolves the per-call editor + user context needed by every product manager method.
/// <para>
/// Replaces <c>ManageProductBase.GetCompanyEditorAndUserDetails</c> and its mutable
/// side-effects (<c>_editorPersona</c>, <c>_productUserId</c>, etc.).
/// Returns an immutable <see cref="ProductCallContext"/> — thread-safe, no shared state.
/// </para>
/// </summary>
public interface IProductContextServiceAsync
{
    /// <summary>
    /// Resolves editor persona + SAML attributes and, when <paramref name="userPersonaId"/> is
    /// non-zero, the target user persona + SAML attributes for <paramref name="productId"/>.
    /// Returns <c>(null, error)</c> on any guard failure so callers can short-circuit cleanly.
    /// Replaces: <c>GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId)</c>.
    /// </summary>
    Task<(ProductCallContext? ctx, ListResponse? error)> GetUserContextAsync(
        long editorPersonaId, long userPersonaId, int productId,
        CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> when the persona holds the <c>SuperUser</c> role type
    /// in the <c>User Type</c> party relationship with its organisation.
    /// Replaces the private <c>IsSuperUserAsync</c> helpers duplicated across product managers.
    /// </summary>
    Task<bool> IsSuperUserAsync(Persona userPersona, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> when the persona holds the <c>USER (NO EMAIL)</c> role type
    /// in the <c>User Type</c> party relationship with its organisation.
    /// Replaces the private <c>IsRegularUserNoEmailAsync</c> helpers duplicated across product managers.
    /// </summary>
    Task<bool> IsRegularUserNoEmailAsync(Persona userPersona, CancellationToken ct = default);
}