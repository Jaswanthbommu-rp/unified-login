using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Resident Portal per-call user-context operations.
/// Encapsulates the legacy <see cref="ManageProductResidentPortal"/> per-call construction
/// pattern behind a mockable async interface.
/// </summary>
public sealed class ManageProductResidentPortalAsync : IManageProductResidentPortalAsync
{
    public Task<ListResponse> ListPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).ListProperties(editorPersonaId, userPersonaId, datafilter));

    public Task<INotifications> GetNotificationSettingsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult<INotifications>(new ManageProductResidentPortal(userClaim).GetNotificationSettings(editorPersonaId, userPersonaId));

    public Task<IResidentPortalUser> GetUserAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult<IResidentPortalUser>(new ManageProductResidentPortal(userClaim).GetUser(editorPersonaId, userPersonaId));

    public Task<IResidentPortalUser> SetLevelAndGroupObjectsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, IResidentPortalUser residentPortalUser, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).SetLevelAndGroupObjects(editorPersonaId, userPersonaId, residentPortalUser));

    public Task<List<ILevel>> ListLevelsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).ListLevels(editorPersonaId, userPersonaId));

    public Task<List<IMessagingGroups>> ListMessageGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).ListMessageGroups(editorPersonaId, userPersonaId));

    public Task<List<ITitle>> ListTitlesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).ListTitles(editorPersonaId, userPersonaId));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));

    public Task<bool> DeleteUserAsync(DefaultUserClaim userClaim, long editorPersonaId, int userId, string userName, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductResidentPortal(userClaim).DeleteUser(editorPersonaId, userId, userName));
}
