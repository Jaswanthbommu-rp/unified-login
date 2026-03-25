using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for <see cref="ManageRelationshipType"/>.
/// Encapsulates the per-call <c>new ManageRelationshipType(userClaim)</c> anti-pattern.
/// </summary>
public class ManageRelationshipTypeAsync : IManageRelationshipTypeAsync
{
    public Task<IList<RelationshipType>> GetRelationshipTypeAsync(DefaultUserClaim userClaim, string relationshipTypeName, CancellationToken cancellationToken = default)
    {
        var svc = new ManageRelationshipType(userClaim);
        return Task.FromResult(svc.GetRelationshipType(relationshipTypeName));
    }

    public Task<IList<UserRelationShipType>> GetUserRelationShipTypesAsync(DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var svc = new ManageRelationshipType(userClaim);
        return Task.FromResult(svc.GetUserRelationShipTypes());
    }
}
