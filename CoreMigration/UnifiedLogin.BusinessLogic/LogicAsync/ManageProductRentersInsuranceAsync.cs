using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Renters Insurance per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductRentersInsurance"/> per-call construction
/// pattern behind a mockable async interface.
/// </summary>
public sealed class ManageProductRentersInsuranceAsync : IManageProductRentersInsuranceAsync
{
    public Task<ListResponse> ListPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRentersInsurance(userClaim).ListProperties(editorPersonaId, userPersonaId, datafilter));

    public Task<IList<ProductRole>> ListRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRentersInsurance(userClaim).ListRoles(editorPersonaId, userPersonaId));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRentersInsurance(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRentersInsurance(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, int userId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductRentersInsurance(userClaim).ChangeUserStatus(editorPersonaId, userId));
}
