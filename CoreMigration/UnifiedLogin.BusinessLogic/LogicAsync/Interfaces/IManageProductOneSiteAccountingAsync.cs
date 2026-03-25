using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for OneSite Accounting per-call user-context operations.
/// Methods here wrap legacy <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductOneSiteAccounting"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// Operations that do not require per-call user context remain on the existing
/// <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageProductOneSiteAccounting"/> interface.
/// </summary>
public interface IManageProductOneSiteAccountingAsync
{
    Task<string> ChangeStatusAccountingUserAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, bool isActive, CancellationToken cancellationToken = default);

    Task<bool> ChangeAccountingUserClaimStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, bool isLinked, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesCountAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsForRoleAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, string roleName, int roleId, CancellationToken cancellationToken = default);
}
