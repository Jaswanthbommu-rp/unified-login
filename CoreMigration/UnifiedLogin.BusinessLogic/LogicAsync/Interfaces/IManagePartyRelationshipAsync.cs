using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for party relationship operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManagePartyRelationship"/>.
/// </summary>
public interface IManagePartyRelationshipAsync
{
    Task<PartyRelationship> GetPartyRelationshipAsync(Guid realPageIdFrom, Guid realPageIdTo, string roleTypeNameFrom, string roleTypeNameTo, string relationshipTypeName, CancellationToken cancellationToken = default);
}
