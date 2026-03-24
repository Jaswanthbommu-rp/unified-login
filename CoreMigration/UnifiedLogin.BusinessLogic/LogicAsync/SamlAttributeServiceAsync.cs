using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Default implementation of <see cref="ISamlAttributeServiceAsync"/>.
/// Orchestrates <see cref="ISamlRepositoryAsync"/> — the five SAML operations previously
/// duplicated as protected helpers on <c>ManageProductBase</c> now live here.
/// </summary>
public sealed class SamlAttributeServiceAsync : ISamlAttributeServiceAsync
{
    private readonly ISamlRepositoryAsync _samlRepo;

    public SamlAttributeServiceAsync(ISamlRepositoryAsync samlRepo)
    {
        ArgumentNullException.ThrowIfNull(samlRepo);
        _samlRepo = samlRepo;
    }

    // ── 1. Read ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<IList<SamlAttributes>> GetProductSamlDetailsAsync(
        long personaId, int productId, CancellationToken ct = default)
        => _samlRepo.GetProductSamlDetailsAsync(personaId, productId, ct);

    // ── 2. Single upsert ─────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Fetches the current attribute list, then creates or updates the matching entry.
    /// Use <see cref="UpsertAttributesAsync"/> when updating multiple attributes to avoid
    /// repeated round-trips.
    /// </remarks>
    public async Task UpsertAttributeAsync(
        long personaId, int productId,
        SamlAttributeEnum attribute, string value, CancellationToken ct = default)
    {
        var attrs    = await _samlRepo.GetProductSamlDetailsAsync(personaId, productId, ct);
        var existing = attrs.FirstOrDefault(a => a.SamlAttributeId == (int)attribute);
        if (existing is not null)
        {
            existing.Value = value;
            await _samlRepo.UpdateSamlUserAttributeAsync(existing, ct);
        }
        else
        {
            await _samlRepo.CreateSamlUserAttributeAsync(personaId, productId, attribute, value, ct);
        }
    }

    // ── 3. Batch upsert ──────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Reads existing attributes <b>once</b>, then processes each entry in-memory —
    /// avoids N+1 repository calls.
    /// </remarks>
    public async Task UpsertAttributesAsync(
        long personaId, int productId,
        Dictionary<SamlAttributeEnum, string> attributes, CancellationToken ct = default)
    {
        if (attributes.Count == 0) return;

        var attrs = await _samlRepo.GetProductSamlDetailsAsync(personaId, productId, ct);

        foreach (var (attr, value) in attributes)
        {
            var existing = attrs.FirstOrDefault(a => a.SamlAttributeId == (int)attr);
            if (existing is not null)
            {
                existing.Value = value;
                await _samlRepo.UpdateSamlUserAttributeAsync(existing, ct);
            }
            else
            {
                await _samlRepo.CreateSamlUserAttributeAsync(personaId, productId, attr, value, ct);
            }
        }
    }

    // ── 4. Delete product info + cleanup ─────────────────────────────────

    /// <inheritdoc/>
    public async Task DeleteProductInfoAndStatusAsync(
        long personaId, int productId, CancellationToken ct = default)
    {
        var result = await _samlRepo.DeleteSamlUserProductInfoAndStatusAsync(personaId, productId, ct);
        if (result.Id > 0)
            await _samlRepo.DeletePersonaProductErrorAsync(personaId, ct);
    }

    // ── 5. Remove single attribute ────────────────────────────────────────

    /// <inheritdoc/>
    public Task RemoveAttributeAsync(
        long personaId, int productId,
        SamlAttributeEnum attribute, CancellationToken ct = default)
        => _samlRepo.RemoveSamlUserAttributeBySamlAttributeIdAsync(personaId, productId, attribute, ct);
}