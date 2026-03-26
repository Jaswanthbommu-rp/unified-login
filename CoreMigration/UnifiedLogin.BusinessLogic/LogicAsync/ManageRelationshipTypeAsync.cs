using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageRelationshipTypeAsync"/>.
/// Replaces the stepping-stone that used <c>new ManageRelationshipType(userClaim)</c>.
/// Claims context is resolved via <see cref="IUserClaimsAccessor"/> — no <c>DefaultUserClaim</c>
/// is passed through the public API.
/// </summary>
public sealed class ManageRelationshipTypeAsync : IManageRelationshipTypeAsync
{
    #region Constants

    // Mirrors ManageRelationshipType constants
    private const int PartyRoleTypeId_ExternalUserNonRP = 402;
    private const int PartyRoleTypeId_ExternalUserRP    = 403;

    #endregion

    #region Fields

    private readonly IRelationshipTypeRepositoryAsync _relationshipTypeRepository;
    private readonly IManagePersonaAsync              _managePersona;
    private readonly IUserClaimsAccessor              _userClaimsAccessor;

    #endregion

    #region Constructor

    public ManageRelationshipTypeAsync(
        IRelationshipTypeRepositoryAsync relationshipTypeRepository,
        IManagePersonaAsync              managePersona,
        IUserClaimsAccessor              userClaimsAccessor)
    {
        _relationshipTypeRepository = relationshipTypeRepository ?? throw new ArgumentNullException(nameof(relationshipTypeRepository));
        _managePersona              = managePersona              ?? throw new ArgumentNullException(nameof(managePersona));
        _userClaimsAccessor         = userClaimsAccessor         ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
    }

    #endregion

    #region IManageRelationshipTypeAsync

    /// <inheritdoc/>
    public Task<IList<RelationshipType>> GetRelationshipTypeAsync(
        string? relationshipTypeName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relationshipTypeName))
            throw new ArgumentException(
                "Relationship type name cannot be null or empty.", nameof(relationshipTypeName));

        return _relationshipTypeRepository.GetRelationshipTypeAsync(relationshipTypeName, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IList<UserRelationShipType>> GetUserRelationShipTypesAsync(
        CancellationToken cancellationToken = default)
    {
        // ── 1. Resolve persona for the current user ───────────────────────
        var persona = await _managePersona.GetPersonaAsync(
            _userClaimsAccessor.PersonaId, cancellationToken: cancellationToken);

        if (persona is null)
            return [];

        // ── 2. Fetch user relationship types for the current organisation ─
        var userRelationShipTypes = (await _relationshipTypeRepository.GetUserRelationShipTypesAsync(
            _userClaimsAccessor.OrganizationPartyId, cancellationToken)).ToList();

        if (userRelationShipTypes.Count == 0)
            return userRelationShipTypes;

        // ── 3. Apply role-based filters (mirrors ManageRelationshipType) ──
        bool isExternalUser = persona.UserTypeId == (int)UserRoleType.ExternalUser;

        // Rule 1: Non-RP employee external user → hide PartyRoleTypeId 402
        if (!_userClaimsAccessor.IsRPEmployee && isExternalUser)
            userRelationShipTypes.RemoveAll(x => x.PartyRoleTypeId == PartyRoleTypeId_ExternalUserNonRP);

        // Rule 2: RP employee external user → hide PartyRoleTypeId 403
        if (_userClaimsAccessor.IsRPEmployee && isExternalUser)
            userRelationShipTypes.RemoveAll(x => x.PartyRoleTypeId == PartyRoleTypeId_ExternalUserRP);

        return userRelationShipTypes;
    }

    #endregion
}
