using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Rum;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// True-async interface for Resident Utility Management (NWP) user operations.
/// Replaces the stepping-stone wrapper that required <see cref="DefaultUserClaim"/> at call time.
/// Context resolution is handled internally via <see cref="IProductContextServiceAsync"/>.
/// </summary>
public interface IManageProductRumAsync
{
    Task<ListResponse> GetPropertyGroupsAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRegionsAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetRolesAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<ListResponse> GetUMGlobalRolesAsync(long editorPersonaId, long userPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<string> UnassignRumUserAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<string> UpdateUserProfileAsync(long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<(string result, List<AdditionalParameters> auditParams)> ManageRumUserAsync(long editorPersonaId, long userPersonaId, RumUserPropertyRegionRole userPropertyRegionRole, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(long editorPersonaId, string productUserId, CancellationToken cancellationToken = default);
}
