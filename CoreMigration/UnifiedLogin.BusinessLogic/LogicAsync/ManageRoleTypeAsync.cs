using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for <see cref="ManageRoleType"/> role-type lookups.
/// Encapsulates per-call construction of ManageRoleType, ProfileRepository, and
/// ManageRelationshipType that previously lived inside the controller action.
/// </summary>
public class ManageRoleTypeAsync : IManageRoleTypeAsync
{
    private readonly IManageRelationshipTypeAsync _manageRelationshipTypeAsync;

    public ManageRoleTypeAsync(IManageRelationshipTypeAsync manageRelationshipTypeAsync)
    {
        _manageRelationshipTypeAsync = manageRelationshipTypeAsync;
    }

    public async Task<IList<RoleType>> ListRoleTypeAsync(
        string roleTypeName,
        string loginName,
        bool includeRelationShips,
        Persona persona,
        DefaultUserClaim userClaim,
        CancellationToken cancellationToken = default)
    {
        var manageRoleType = new ManageRoleType();
        List<RoleType> roleTypeList;

        if (persona != null)
        {
            // "User Role" branch — persona pre-resolved by caller
            roleTypeList = (List<RoleType>)manageRoleType.GetRoleTypeDependency(
                roleTypeId: persona.UserTypeId,
                partyId: userClaim.OrganizationPartyId,
                orgMasterId: persona.Organization.BooksCustomerMasterId,
                loginName: loginName);

            if (!userClaim.IsRPEmployee && persona.UserTypeId == (int)UserRoleType.ExternalUser)
            {
                roleTypeList.RemoveAll(x => x.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
                var profileRepository = new ProfileRepository(userClaim);
                var externalUserRelationship = profileRepository.GetExternalUserRelationship(userClaim.OrganizationPartyId, userClaim.UserId);
                if (!string.IsNullOrEmpty(externalUserRelationship.OperatorCode) && !string.IsNullOrEmpty(externalUserRelationship.OperatorValue))
                {
                    roleTypeList.RemoveAll(x => x.PartyRoleTypeId != 405);
                }
            }
        }
        else
        {
            roleTypeList = (List<RoleType>)manageRoleType.GetRoleType(
                roleTypeName: roleTypeName,
                partyId: null,
                orgMasterId: null,
                loginName: loginName);
        }

        // Remove RealPage Employee role for unauthenticated requests
        if (userClaim.OrganizationPartyId == 0)
        {
            roleTypeList.RemoveAll(x => x.Name.Equals("RealPage Employee", StringComparison.OrdinalIgnoreCase));
        }

        if (roleTypeList == null) return null;

        if (includeRelationShips)
        {
            var userRelationshipTypes = await _manageRelationshipTypeAsync.GetUserRelationShipTypesAsync(userClaim, cancellationToken);
            foreach (var r in roleTypeList)
            {
                r.UserRelationShipTypes = userRelationshipTypes.Where(c => c.PartyRoleTypeId == r.PartyRoleTypeId).ToList();
            }
        }

        return roleTypeList;
    }
}
