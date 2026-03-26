using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for relationship-type lookup operations.
/// Wraps legacy <see cref="UnifiedLogin.BusinessLogic.Logic.ManageRelationshipType"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// </summary>
public interface IManageRelationshipTypeAsync
{
    Task<IList<RelationshipType>> GetRelationshipTypeAsync(string relationshipTypeName, CancellationToken cancellationToken = default);

    Task<IList<UserRelationShipType>> GetUserRelationShipTypesAsync(CancellationToken cancellationToken = default);
}
