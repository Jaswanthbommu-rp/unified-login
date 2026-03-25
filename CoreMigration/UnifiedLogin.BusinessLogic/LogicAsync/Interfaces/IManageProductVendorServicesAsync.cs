using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.VendorServices;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Vendor Services per-call user-context operations.
/// Wraps legacy <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductVendorServices"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// </summary>
public interface IManageProductVendorServicesAsync
{
    Task<ListResponse> GetPropertyGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, AccessType accessType, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<Notification> GetNotificationSettingsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string username, string productUserId, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);
}
