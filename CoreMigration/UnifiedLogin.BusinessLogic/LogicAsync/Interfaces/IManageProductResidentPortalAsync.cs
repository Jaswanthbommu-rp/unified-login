using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Resident Portal per-call user-context operations.
/// Wraps legacy <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductResidentPortal"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// </summary>
public interface IManageProductResidentPortalAsync
{
    Task<ListResponse> ListPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<INotifications> GetNotificationSettingsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<IResidentPortalUser> GetUserAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<IResidentPortalUser> SetLevelAndGroupObjectsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, IResidentPortalUser residentPortalUser, CancellationToken cancellationToken = default);

    Task<List<ILevel>> ListLevelsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<List<IMessagingGroups>> ListMessageGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<List<ITitle>> ListTitlesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);

    Task<bool> DeleteUserAsync(DefaultUserClaim userClaim, long editorPersonaId, int userId, string userName, CancellationToken cancellationToken = default);
}
