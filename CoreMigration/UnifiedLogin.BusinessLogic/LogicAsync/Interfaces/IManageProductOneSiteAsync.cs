using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for OneSite per-call user-context operations.
/// Methods here wrap legacy <see cref="UnifiedLogin.BusinessLogic.Logic.Product.ManageProductOneSite"/>
/// calls that require <see cref="DefaultUserClaim"/> at construction time.
/// Operations that do not require per-call user context remain on the existing
/// <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageProductOneSite"/> interface.
/// </summary>
public interface IManageProductOneSiteAsync
{
    Task<ListResponse> GetRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRegionsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userId, CancellationToken cancellationToken = default);
}
