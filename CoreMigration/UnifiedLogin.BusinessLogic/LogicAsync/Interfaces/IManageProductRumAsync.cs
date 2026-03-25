using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Resident Utility Management per-call user-context operations.
/// Wraps legacy <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductRum"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// </summary>
public interface IManageProductRumAsync
{
    Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRegionsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertyGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string productUserId, CancellationToken cancellationToken = default);
}
