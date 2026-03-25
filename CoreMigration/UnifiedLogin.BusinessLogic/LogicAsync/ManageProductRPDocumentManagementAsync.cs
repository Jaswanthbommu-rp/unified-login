using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for RPDM per-call user-context operations.
/// Only the migration listing requires per-call <see cref="DefaultUserClaim"/> construction;
/// all other RPDM operations remain on the injected sync
/// <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageProductRPDocumentManagement"/>.
/// </summary>
public sealed class ManageProductRPDocumentManagementAsync : IManageProductRPDocumentManagementAsync
{
    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRPDocumentManagement(userClaim).GetMigrationUsers(editorPersonaId, datafilter));
}
