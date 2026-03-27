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
    private readonly IManageProductOneSiteAccountingAsync _manageProductOneSiteAccounting;
    public ManageProductOneSiteAccountingAsync(IManageProductOneSiteAccountingAsync manageProductOneSiteAccounting)
    {
        _manageProductOneSiteAccounting = manageProductOneSiteAccounting ?? throw new ArgumentNullException(nameof(manageProductOneSiteAccounting));
    }
    public async Task<string> ChangeStatusAccountingUserAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, bool isActive, CancellationToken cancellationToken = default)
        => await _manageProductOneSiteAccounting.ChangeStatusAccountingUserAsync(userClaim, editorPersonaId, userPersonaId, isActive, cancellationToken);

    public async Task<bool> ChangeAccountingUserClaimStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, bool isLinked, CancellationToken cancellationToken = default)
        => await _manageProductOneSiteAccounting.ChangeAccountingUserClaimStatusAsync(userClaim, editorPersonaId, userPersonaId, isLinked, cancellationToken);

    public async Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, CancellationToken cancellationToken = default)
        => await _manageProductOneSiteAccounting.ChangeUserStatusAsync(userClaim, editorPersonaId, userName, cancellationToken);
    public async Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => await _manageProductOneSiteAccounting.GetMigrationUsersAsync(userClaim, editorPersonaId, datafilter, cancellationToken);

    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => await _manageProductOneSiteAccounting.UpdateUsersMigrationStatusAsync(userClaim, editorPersonaId, migrateUsers, cancellationToken);

    public async Task<ListResponse> GetRolesCountAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => await _manageProductOneSiteAccounting.GetRolesCountAsync(userClaim, editorPersonaId, datafilter, cancellationToken);

    public async Task<ListResponse> GetRightsForRoleAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, string roleName, int roleId, CancellationToken cancellationToken = default)
        => await _manageProductOneSiteAccounting.GetRightsForRoleAsync(userClaim, editorPersonaId, datafilter, roleName, roleId, cancellationToken);
}
