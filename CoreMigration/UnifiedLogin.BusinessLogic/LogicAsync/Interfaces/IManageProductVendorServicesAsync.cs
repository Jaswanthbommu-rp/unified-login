using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.VendorServices;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True-async interface for Vendor Credentialing / Vendor Services user operations.
/// Replaces the stepping-stone wrapper that required <see cref="DefaultUserClaim"/> at call time.
/// Context resolution is handled internally via <see cref="IProductContextServiceAsync"/>.
/// </summary>
public interface IManageProductVendorServicesAsync
{
    Task<ListResponse> GetPropertyGroupsAsync(long editorPersonaId, long userPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(long editorPersonaId, long userPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesAsync(long editorPersonaId, long userPersonaId, AccessType accessType, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<Notification?> GetNotificationSettingsAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<string> UnassignUserAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<string> UpdateVendorServicesUserProfileAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<(string result, List<AdditionalParameters> auditParams)> ChangeVendorServicesUserTypeAsync(long createUserPersonaId, long assignUserPersonaId, UserProductPropertyNotification rpList, BatchProcessType batchProcessType, CancellationToken cancellationToken = default);

    Task<(string result, List<AdditionalParameters> auditParams)> ManageVendorServicesUserAsync(long editorPersonaId, long productUserPersonaId, UserProductPropertyNotification userProductPropertyNotification, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(long editorPersonaId, string username, string productUserId, bool isActive = false, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(long editorPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);
}
