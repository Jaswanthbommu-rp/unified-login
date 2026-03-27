using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Ops per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductOps"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductOpsAsync : IManageProductOpsAsync
{
    private readonly IManageProductOpsAsync _manageProductOps;
    public ManageProductOpsAsync(IManageProductOpsAsync manageProductOps)
    {
        _manageProductOps = manageProductOps ?? throw new ArgumentNullException(nameof(manageProductOps));
    }
    public async Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => await _manageProductOps.GetMigrationUsersAsync(userClaim, editorPersonaId, datafilter, cancellationToken);
}
