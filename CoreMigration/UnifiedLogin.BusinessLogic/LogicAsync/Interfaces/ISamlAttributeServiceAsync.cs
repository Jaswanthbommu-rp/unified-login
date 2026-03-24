using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Focused service for SAML attribute management.
/// Extracted from <c>ManageProductBase</c> — only product managers that use SAML inject this.
/// <para>
/// Centralises the five SAML operations that were previously scattered as protected helpers
/// on <c>ManageProductBase</c>:
/// </para>
/// <list type="number">
///   <item><see cref="GetProductSamlDetailsAsync"/> — read all attributes for a persona + product.</item>
///   <item><see cref="UpsertAttributeAsync"/> — create or update a single attribute.</item>
///   <item><see cref="UpsertAttributesAsync"/> — batch create or update (single round-trip read).</item>
///   <item><see cref="DeleteProductInfoAndStatusAsync"/> — delete all product info and clear persona error.</item>
///   <item><see cref="RemoveAttributeAsync"/> — remove one specific attribute by type.</item>
/// </list>
/// </summary>
public interface ISamlAttributeServiceAsync
{
    /// <summary>
    /// Returns all SAML attributes for the given <paramref name="personaId"/> and
    /// <paramref name="productId"/>.
    /// Replaces: direct <c>ISamlRepository.GetProductSamlDetails</c> calls in product managers.
    /// </summary>
    Task<IList<SamlAttributes>> GetProductSamlDetailsAsync(
        long personaId, int productId, CancellationToken ct = default);

    /// <summary>
    /// Creates the attribute if absent; updates its value if already present.
    /// Replaces: <c>ManageProductBase.UpdateSamlUserAttribute</c> /
    /// <c>CreateSamlUserAttribute</c> (private overloads).
    /// </summary>
    Task UpsertAttributeAsync(
        long personaId, int productId,
        SamlAttributeEnum attribute, string value, CancellationToken ct = default);

    /// <summary>
    /// Batch upsert — reads existing attributes once, then creates or updates each entry.
    /// Replaces: <c>ManageProductBase.UpdateSamlUserAttributes</c> (both overloads).
    /// </summary>
    Task UpsertAttributesAsync(
        long personaId, int productId,
        Dictionary<SamlAttributeEnum, string> attributes, CancellationToken ct = default);

    /// <summary>
    /// Deletes all SAML product info and status for the persona, then clears any
    /// lingering persona product error record.
    /// Replaces: <c>ManageProductBase.DeleteSamlUserProductInfoAndStatus</c>.
    /// </summary>
    Task DeleteProductInfoAndStatusAsync(
        long personaId, int productId, CancellationToken ct = default);

    /// <summary>
    /// Removes one specific SAML attribute identified by <paramref name="attribute"/>.
    /// Replaces: direct <c>ISamlRepository.RemoveSamlUserAttributeBySamlAttributeId</c> calls.
    /// </summary>
    Task RemoveAttributeAsync(
        long personaId, int productId,
        SamlAttributeEnum attribute, CancellationToken ct = default);
}