using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.VendorServices;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for <see cref="ManageProductVendorServices"/>.
/// Encapsulates the per-call <c>new ManageProductVendorServices(userClaim)</c> anti-pattern.
/// </summary>
public class ManageProductVendorServicesAsync : IManageProductVendorServicesAsync
{
    public Task<ListResponse> GetPropertyGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        var svc = new ManageProductVendorServices(userClaim);
        return Task.FromResult(svc.GetPropertyGroups(editorPersonaId, userPersonaId, dataFilter));
    }

    public Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, AccessType accessType, RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        var svc = new ManageProductVendorServices(userClaim);
        return Task.FromResult(svc.GetRoles(editorPersonaId, userPersonaId, accessType, dataFilter));
    }

    public Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        var svc = new ManageProductVendorServices(userClaim);
        return Task.FromResult(svc.GetProperties(editorPersonaId, userPersonaId, dataFilter));
    }

    public Task<Notification> GetNotificationSettingsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
    {
        var svc = new ManageProductVendorServices(userClaim);
        return Task.FromResult(svc.GetNotificationSettings(editorPersonaId, userPersonaId));
    }

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string username, string productUserId, CancellationToken cancellationToken = default)
    {
        var svc = new ManageProductVendorServices(userClaim);
        return Task.FromResult(svc.ChangeUserStatus(editorPersonaId, username, productUserId));
    }

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        var svc = new ManageProductVendorServices(userClaim);
        return Task.FromResult(svc.GetMigrationUsers(editorPersonaId, dataFilter));
    }

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
    {
        var svc = new ManageProductVendorServices(userClaim);
        return Task.FromResult(svc.UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));
    }
}
