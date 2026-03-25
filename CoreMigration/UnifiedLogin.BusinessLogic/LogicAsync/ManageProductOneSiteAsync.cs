using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for OneSite per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductOneSite"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductOneSiteAsync : IManageProductOneSiteAsync
{
    public Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOnSite(userClaim).GetRoles(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOnSite(userClaim).GetProperties(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetRegionsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOnSite(userClaim).GetRegions(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOnSite(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOnSite(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductOnSite(userClaim).ChangeUserStatus(editorPersonaId, userId));
}
