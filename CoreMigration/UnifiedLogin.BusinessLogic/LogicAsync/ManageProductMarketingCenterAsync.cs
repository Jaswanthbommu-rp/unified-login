using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Marketing Center product operations.
/// Encapsulates the legacy <see cref="ManageProductMarketingCenter"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductMarketingCenterAsync : IManageProductMarketingCenterAsync
{
    public Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).GetRoles(editorPersonaId, userPersonaId, datafilter));

    public Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).GetProperties(editorPersonaId, userPersonaId, datafilter));

    public Task<(string result, List<AdditionalParameters> additionalParameters)> ManageMarketingCenterUserAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, List<int> roleList, List<string> propertyList, bool isAssignedNewPropertyByDefault, CancellationToken cancellationToken = default)
    {
        List<AdditionalParameters> additionalParameters;
        string result = new ManageProductMarketingCenter(userClaim).ManageMarketingCenterUser(editorPersonaId, userPersonaId, roleList, propertyList, isAssignedNewPropertyByDefault, out additionalParameters);
        return Task.FromResult((result, additionalParameters));
    }

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, string productUserId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).ChangeUserStatus(editorPersonaId, userName, productUserId));

    public Task<ListResponse> GetRolesCountAsync(DefaultUserClaim userClaim, long editorPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).GetRolesCount(editorPersonaId));

    public Task<ListResponse> GetRightsAsync(DefaultUserClaim userClaim, long editorPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).GetRights(editorPersonaId));

    public Task<ListResponse> DeleteRoleAsync(DefaultUserClaim userClaim, long editorPersonaId, int roleId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).DeleteRole(editorPersonaId, roleId));

    public Task<ListResponse> UpdateRoleStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, int roleId, bool isActive, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).UpdateRoleStatus(editorPersonaId, roleId, isActive));

    public Task<ListResponse> GetRolesForRightIdAsync(DefaultUserClaim userClaim, long editorPersonaId, int rightId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).GetRolesForRightId(editorPersonaId, rightId));

    public Task<ListResponse> UpdateRolesForRightAsync(DefaultUserClaim userClaim, long editorPersonaId, int rightId, List<string> roleList, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).UpdateRolesForRight(editorPersonaId, rightId, roleList));

    public Task<ListResponse> GetRightsForRoleIdAsync(DefaultUserClaim userClaim, long editorPersonaId, int roleId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).GetRightsForRoleId(editorPersonaId, roleId));

    public Task<ListResponse> CreateNewMCRoleWithRightsAsync(DefaultUserClaim userClaim, long editorPersonaId, MCRole mcRole, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).CreateNewMCRoleWithRights(editorPersonaId, mcRole));

    public Task<ListResponse> UpdateMCRoleWithRightsAsync(DefaultUserClaim userClaim, long editorPersonaId, MCRole mcRole, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).UpdateNewMCRoleWithRights(editorPersonaId, mcRole));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductMarketingCenter(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));
}
