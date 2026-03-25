using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Marketing Center product management operations.
/// Each method takes <see cref="DefaultUserClaim"/> because the underlying legacy class
/// requires per-call user context at construction time.
/// </summary>
public interface IManageProductMarketingCenterAsync
{
    Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<(string result, List<AdditionalParameters> additionalParameters)> ManageMarketingCenterUserAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, List<int> roleList, List<string> propertyList, bool isAssignedNewPropertyByDefault, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, string productUserId, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesCountAsync(DefaultUserClaim userClaim, long editorPersonaId, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsAsync(DefaultUserClaim userClaim, long editorPersonaId, CancellationToken cancellationToken = default);

    Task<ListResponse> DeleteRoleAsync(DefaultUserClaim userClaim, long editorPersonaId, int roleId, CancellationToken cancellationToken = default);

    Task<ListResponse> UpdateRoleStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, int roleId, bool isActive, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesForRightIdAsync(DefaultUserClaim userClaim, long editorPersonaId, int rightId, CancellationToken cancellationToken = default);

    Task<ListResponse> UpdateRolesForRightAsync(DefaultUserClaim userClaim, long editorPersonaId, int rightId, List<string> roleList, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRightsForRoleIdAsync(DefaultUserClaim userClaim, long editorPersonaId, int roleId, CancellationToken cancellationToken = default);

    Task<ListResponse> CreateNewMCRoleWithRightsAsync(DefaultUserClaim userClaim, long editorPersonaId, MCRole mcRole, CancellationToken cancellationToken = default);

    Task<ListResponse> UpdateMCRoleWithRightsAsync(DefaultUserClaim userClaim, long editorPersonaId, MCRole mcRole, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);
}
