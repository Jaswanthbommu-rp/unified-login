using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for party relationship operations.
/// Delegates to the existing sync <see cref="IManagePartyRelationship"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManagePartyRelationshipAsync : IManagePartyRelationshipAsync
{
    private readonly IManagePartyRelationship _managePartyRelationship;

    public ManagePartyRelationshipAsync(IManagePartyRelationship managePartyRelationship)
    {
        _managePartyRelationship = managePartyRelationship ?? throw new ArgumentNullException(nameof(managePartyRelationship));
    }

    public Task<PartyRelationship> GetPartyRelationshipAsync(Guid realPageIdFrom, Guid realPageIdTo, string roleTypeNameFrom, string roleTypeNameTo, string relationshipTypeName, CancellationToken cancellationToken = default)
        => Task.FromResult(_managePartyRelationship.GetPartyRelationship(realPageIdFrom, realPageIdTo, roleTypeNameFrom, roleTypeNameTo, relationshipTypeName));
}
