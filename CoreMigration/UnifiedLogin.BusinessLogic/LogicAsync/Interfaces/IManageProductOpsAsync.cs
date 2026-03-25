using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Ops per-call user-context operations.
/// Methods here wrap legacy <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductOps"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// Operations that do not require per-call user context remain on the existing
/// <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageProductOps"/> interface.
/// </summary>
public interface IManageProductOpsAsync
{
    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);
}
