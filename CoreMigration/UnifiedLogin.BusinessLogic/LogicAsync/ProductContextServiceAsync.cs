using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Default implementation of <see cref="IProductContextServiceAsync"/>.
/// Replaces <c>ManageProductBase.GetCompanyEditorAndUserDetails</c> +
/// <c>verifyPersona</c> with a stateless, thread-safe service.
/// </summary>
public sealed class ProductContextServiceAsync : IProductContextServiceAsync
{
    private readonly IPersonaRepositoryAsync       _personaRepo;
    private readonly ISamlAttributeServiceAsync    _samlService;
    private readonly IManagePartyRelationshipAsync _partyRelationship;

    public ProductContextServiceAsync(
        IPersonaRepositoryAsync       personaRepo,
        ISamlAttributeServiceAsync    samlService,
        IManagePartyRelationshipAsync partyRelationship)
    {
        ArgumentNullException.ThrowIfNull(personaRepo);        _personaRepo        = personaRepo;
        ArgumentNullException.ThrowIfNull(samlService);        _samlService        = samlService;
        ArgumentNullException.ThrowIfNull(partyRelationship);  _partyRelationship  = partyRelationship;
    }

    /// <inheritdoc/>
    public async Task<(ProductCallContext? ctx, ListResponse? error)> GetUserContextAsync(
        long editorPersonaId, long userPersonaId, int productId,
        CancellationToken ct = default)
    {
        if (editorPersonaId == 0)
            return (null, Error("Invalid editor persona"));

        try
        {
            var editorPersona = await _personaRepo.GetPersonaAsync(editorPersonaId, false, ct);
            if (editorPersona is null)
                return (null, Error($"Editor persona {editorPersonaId} not found."));

            // Resolve SAML attributes for the editor
            var editorSaml    = await _samlService.GetProductSamlDetailsAsync(editorPersonaId, productId, ct);
            string editorUser = SamlValue(editorSaml, "PRODUCTUSERNAME");
            string editorId   = SamlValue(editorSaml, "USERID");

            Persona? userPersona = null;
            string productUser = "", productUserId = "", learnerId = "", managerId = "";

            if (userPersonaId != 0 && userPersonaId != editorPersonaId)
            {
                userPersona = await _personaRepo.GetPersonaAsync(userPersonaId, false, ct);
                if (userPersona?.Organization?.PartyId != editorPersona.Organization?.PartyId)
                    return (null, Error("Invalid user persona"));

                var userSaml  = await _samlService.GetProductSamlDetailsAsync(userPersonaId, productId, ct);
                productUser   = SamlValue(userSaml, "PRODUCTUSERNAME");
                productUserId = SamlValue(userSaml, "USERID");
                learnerId     = SamlValue(userSaml, "LEARNERID");
                managerId     = SamlValue(userSaml, "MANAGERID");
            }
            else if (userPersonaId == editorPersonaId)
            {
                userPersona = editorPersona;
            }

            return (new ProductCallContext(
                editorPersona, userPersona,
                editorUser, editorId,
                productUser, productUserId,
                learnerId, managerId), null);
        }
        catch (Exception ex)
        {
            return (null, Error(ex.Message));
        }
    }

    // ── User-type helpers ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> IsSuperUserAsync(Persona userPersona, CancellationToken ct = default)
    {
        var rel = await _partyRelationship.GetPartyRelationshipAsync(
            userPersona.RealPageId, userPersona.Organization.RealPageId,
            roleTypeNameFrom: null, roleTypeNameTo: null,
            relationshipTypeName: "User Type", cancellationToken: ct);
        return rel?.RoleTypeFrom?.Name?.Equals("SuperUser", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <inheritdoc/>
    public async Task<bool> IsRegularUserNoEmailAsync(Persona userPersona, CancellationToken ct = default)
    {
        var rel = await _partyRelationship.GetPartyRelationshipAsync(
            userPersona.RealPageId, userPersona.Organization.RealPageId,
            roleTypeNameFrom: null, roleTypeNameTo: null,
            relationshipTypeName: "User Type", cancellationToken: ct);
        return rel?.RoleTypeFrom?.Name?.Equals("USER (NO EMAIL)", StringComparison.OrdinalIgnoreCase) == true;
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private static string SamlValue(IList<SamlAttributes> attrs, string name)
        => attrs.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value
           ?? string.Empty;

    private static ListResponse Error(string reason)
        => new() { IsError = true, ErrorReason = reason };
}