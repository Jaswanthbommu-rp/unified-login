using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Lead2Lease product operations.
/// Encapsulates the legacy <see cref="ManageProductLead2Lease"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductLead2LeaseAsync : IManageProductLead2LeaseAsync
{
    public Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductLead2Lease(userClaim).GetRoles(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductLead2Lease(userClaim).GetProperties(editorPersonaId, userPersonaId, datafilter));

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, string productUserId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductLead2Lease(userClaim).ChangeUserStatus(editorPersonaId, userName, productUserId));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductLead2Lease(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductLead2Lease(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));
}
