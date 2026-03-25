using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Resident Utility Management per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductRum"/> per-call construction
/// pattern behind a mockable async interface.
/// </summary>
public sealed class ManageProductRumAsync : IManageProductRumAsync
{
    public Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRum(userClaim).GetRoles(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRum(userClaim).GetProperties(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetRegionsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRum(userClaim).GetRegions(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetPropertyGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRum(userClaim).GetPropertyGroups(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRum(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRum(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string productUserId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRum(userClaim).ChangeUserStatus(editorPersonaId, productUserId));
}
