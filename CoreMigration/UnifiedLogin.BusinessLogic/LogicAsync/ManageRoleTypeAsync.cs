using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first implementation of <see cref="IManageRoleTypeAsync"/>.
/// <para>
/// Replaces the stepping-stone that used <c>new ManageRoleType()</c> and
/// <c>new ProfileRepository(userClaim)</c>.
/// </para>
/// <para>
/// <see cref="GetRoleTypeAsync"/> and <see cref="GetRoleTypeDependencyAsync"/> are exact
/// async mirrors of the sync <c>ManageRoleType.GetRoleType</c> /
/// <c>ManageRoleType.GetRoleTypeDependency</c> — each fetches from the repo then applies
/// the shared <see cref="FilterRoleTypeAsync"/> logic.
/// <see cref="ListRoleTypeAsync"/> delegates to those methods and adds the additional
/// controller-level concerns (external-operator filter, RP-Employee removal, relationship enrichment).
/// </para>
/// </summary>
public sealed class ManageRoleTypeAsync : IManageRoleTypeAsync
{
    #region Constants

    private const int ExternalUserRoleTypeId = 405;
    private const int UserNoEmailRoleTypeId  = 404;

    #endregion

    #region Fields
    private readonly IUserClaimsAccessor _userClaim;
    private readonly IRoleTypeRepositoryAsync     _roleTypeRepository;
    private readonly IManageUserLoginAsync        _manageUserLogin;
    private readonly IProfileRepositoryAsync      _profileRepository;
    private readonly IManageRelationshipTypeAsync _manageRelationshipTypeAsync;

    #endregion

    #region Constructor

    public ManageRoleTypeAsync(
        IRoleTypeRepositoryAsync     roleTypeRepository,
        IManageUserLoginAsync        manageUserLogin,
        IProfileRepositoryAsync      profileRepository,
        IManageRelationshipTypeAsync manageRelationshipTypeAsync,
        IUserClaimsAccessor          userClaim)
    {
        _roleTypeRepository          = roleTypeRepository          ?? throw new ArgumentNullException(nameof(roleTypeRepository));
        _manageUserLogin             = manageUserLogin             ?? throw new ArgumentNullException(nameof(manageUserLogin));
        _profileRepository           = profileRepository           ?? throw new ArgumentNullException(nameof(profileRepository));
        _manageRelationshipTypeAsync = manageRelationshipTypeAsync ?? throw new ArgumentNullException(nameof(manageRelationshipTypeAsync));
        _userClaim                   = userClaim                   ?? throw new ArgumentNullException(nameof(userClaim));
    }

    #endregion

    #region IManageRoleTypeAsync — direct equivalents of IManageRoleType

    /// <inheritdoc/>
    public async Task<IList<RoleType>> GetRoleTypeAsync(
        string roleTypeName,
        long? partyId,
        long? orgMasterId = null,       // accepted for API parity; not yet used in filtering
        string? loginName = null,
        CancellationToken cancellationToken = default)
    {
        var roleTypeList = (await _roleTypeRepository.GetRoleTypeAsync(
            roleTypeName, partyId, cancellationToken)).ToList();

        return await FilterRoleTypeAsync(roleTypeList, loginName, partyId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IList<RoleType>> GetRoleTypeDependencyAsync(
        long? roleTypeId,
        long? partyId,
        long? orgMasterId = null,       // accepted for API parity; not yet used in filtering
        string? loginName = null,
        CancellationToken cancellationToken = default)
    {
        var roleTypeList = (await _roleTypeRepository.GetRoleTypeDependencyAsync(
            roleTypeId, partyId, cancellationToken)).ToList();

        return await FilterRoleTypeAsync(roleTypeList, loginName, partyId, cancellationToken);
    }

    #endregion

    #region IManageRoleTypeAsync — controller orchestration

    /// <inheritdoc/>
    public async Task<IList<RoleType>> ListRoleTypeAsync(
        string roleTypeName,
        string? loginName,
        bool includeRelationShips,
        Persona? persona,       
        CancellationToken cancellationToken = default)
    {
        // ── 1. Fetch and base-filter via the direct equivalents above ─────
        List<RoleType> roleTypeList;

        if (persona is not null)
        {
            roleTypeList = (await GetRoleTypeDependencyAsync(
                persona.UserTypeId,
                partyId:     _userClaim.OrganizationPartyId,
                orgMasterId: persona.Organization?.BooksCustomerMasterId,
                loginName:   loginName,
                cancellationToken)).ToList();

            // ── 2. External-user operator filter (controller-level concern) ─
            if (!_userClaim.IsRPEmployee && persona.UserTypeId == (int)UserRoleType.ExternalUser)
            {
                roleTypeList.RemoveAll(x =>
                    x.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));

                var externalRelationship = await _profileRepository.GetExternalUserRelationshipAsync(
                    _userClaim.OrganizationPartyId, _userClaim.UserId, cancellationToken);

                if (!string.IsNullOrEmpty(externalRelationship?.OperatorCode)
                    && !string.IsNullOrEmpty(externalRelationship?.OperatorValue))
                {
                    roleTypeList.RemoveAll(x => x.PartyRoleTypeId != ExternalUserRoleTypeId);
                }
            }
        }
        else
        {
            roleTypeList = (await GetRoleTypeAsync(
                roleTypeName,
                partyId:     null,
                orgMasterId: null,
                loginName:   loginName,
                cancellationToken)).ToList();
        }

        // ── 3. Remove RealPage Employee for unauthenticated requests ──────
        if (_userClaim.OrganizationPartyId == 0)
            roleTypeList.RemoveAll(x =>
                x.Name.Equals("RealPage Employee", StringComparison.OrdinalIgnoreCase));

        if (roleTypeList is null) return null;

        // ── 4. Optional relationship-type enrichment ──────────────────────
        if (includeRelationShips)
        {
            var userRelationshipTypes = await _manageRelationshipTypeAsync
                .GetUserRelationShipTypesAsync(cancellationToken);

            foreach (var r in roleTypeList)
                r.UserRelationShipTypes = userRelationshipTypes
                    .Where(c => c.PartyRoleTypeId == r.PartyRoleTypeId)
                    .ToList();
        }

        return roleTypeList;
    }

    #endregion

    #region Private — FilterRoleTypeAsync (mirrors ManageRoleType.FilterRoleType)

    /// <summary>
    /// Async equivalent of <c>ManageRoleType.FilterRoleType</c>.
    /// Applies the two org-level role-type restrictions when a <paramref name="loginName"/> is supplied.
    /// </summary>
    private async Task<IList<RoleType>> FilterRoleTypeAsync(
        IList<RoleType> roleTypeList,
        string? loginName,
        long? partyId,
        CancellationToken cancellationToken)
    {
        // Guard: mirrors ManageRoleType.ShouldApplyFiltering
        if (roleTypeList is null || roleTypeList.Count == 0 || string.IsNullOrWhiteSpace(loginName))
            return roleTypeList ?? [];

        // Guard: mirrors ManageRoleType.HasUserPersonaOrganizations
        var userPersonaOrgs = await _manageUserLogin
            .GetUserPersonaOrganizationAsync(loginName, cancellationToken: cancellationToken);

        if (userPersonaOrgs is null || userPersonaOrgs.Count == 0)
            return roleTypeList;

        // Rule 1: user exists as a non-external member of a different org
        //         → restrict to External User roles only
        bool hasNonExternalInOtherOrg = userPersonaOrgs.Any(i =>
            !i.OrganizationPartyId.Equals(partyId) &&
            !i.PartyRoleTypeId.Equals(ExternalUserRoleTypeId));

        if (hasNonExternalInOtherOrg)
            roleTypeList = roleTypeList
                .Where(r => r.PartyRoleTypeId == ExternalUserRoleTypeId)
                .ToList();

        // Rule 2: user has any external-user persona → hide UserNoEmail
        bool hasExternalUser = userPersonaOrgs.Any(i =>
            i.PartyRoleTypeId.Equals(ExternalUserRoleTypeId));

        if (hasExternalUser)
            roleTypeList = roleTypeList
                .Where(r => r.PartyRoleTypeId != UserNoEmailRoleTypeId)
                .ToList();

        return roleTypeList;
    }

    #endregion
}
