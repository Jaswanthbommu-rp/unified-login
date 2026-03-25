using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for OneSite Accounting per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductOneSiteAccounting"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductOneSiteAccountingAsync : IManageProductOneSiteAccountingAsync
{
    public Task<string> ChangeStatusAccountingUserAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, bool isActive, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOneSiteAccounting(userClaim).ChangeStatusAccountingUser(editorPersonaId, userPersonaId, isActive));

    public Task<bool> ChangeAccountingUserClaimStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, bool isLinked, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOneSiteAccounting(userClaim).ChangeAccountingUserClaimStatus(editorPersonaId, userPersonaId, isLinked));

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOneSiteAccounting(userClaim).ChangeUserStatus(editorPersonaId, userName));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOneSiteAccounting(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOneSiteAccounting(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));

    public Task<ListResponse> GetRolesCountAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOneSiteAccounting(userClaim).GetRolesCount(editorPersonaId, datafilter));

    public Task<ListResponse> GetRightsForRoleAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, string roleName, int roleId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOneSiteAccounting(userClaim).GetRightsForRole(editorPersonaId, datafilter, roleName, roleId));
}
