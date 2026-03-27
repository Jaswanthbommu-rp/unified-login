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
    private readonly IManageProductOneSiteAsync _manageProductOneSite;
    public ManageProductOneSiteAsync(IManageProductOneSiteAsync manageProductOneSite)
    {
        _manageProductOneSite = manageProductOneSite ?? throw new ArgumentNullException(nameof(manageProductOneSite));
    }
    public async Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => await _manageProductOneSite.GetRolesAsync(userClaim, editorPersonaId, userPersonaId, datafilter, cancellationToken);

    public async Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => await _manageProductOneSite.GetPropertiesAsync(userClaim, editorPersonaId, userPersonaId, datafilter, cancellationToken);

    public async Task<ListResponse> GetRegionsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => await _manageProductOneSite.GetRegionsAsync(userClaim, editorPersonaId, userPersonaId, datafilter, cancellationToken);
    public async Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => await _manageProductOneSite.GetMigrationUsersAsync(userClaim, editorPersonaId, datafilter, cancellationToken);

    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => await _manageProductOneSite.UpdateUsersMigrationStatusAsync(userClaim, editorPersonaId, migrateUsers, cancellationToken);

    public async Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userId, CancellationToken cancellationToken = default)
        => await _manageProductOneSite.ChangeUserStatusAsync(userClaim, editorPersonaId, userId, cancellationToken);
}
