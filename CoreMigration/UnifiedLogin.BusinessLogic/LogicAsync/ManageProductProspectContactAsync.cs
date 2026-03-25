using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Prospect Contact Center per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductProspectContact"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductProspectContactAsync : IManageProductProspectContactAsync
{
    public Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductProspectContact(userClaim).GetProperties(editorPersonaId, userPersonaId, datafilter));

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, int userId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductProspectContact(userClaim).ChangeUserStatus(editorPersonaId, userId));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductProspectContact(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductProspectContact(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));
}
