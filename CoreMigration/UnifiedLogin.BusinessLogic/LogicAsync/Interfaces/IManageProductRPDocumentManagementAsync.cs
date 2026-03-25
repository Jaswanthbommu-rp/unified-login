using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for RPDM per-call user-context operations.
/// Only the migration listing requires per-call <see cref="DefaultUserClaim"/> construction;
/// all other RPDM operations remain on
/// <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageProductRPDocumentManagement"/>.
/// </summary>
public interface IManageProductRPDocumentManagementAsync
{
    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);
}
