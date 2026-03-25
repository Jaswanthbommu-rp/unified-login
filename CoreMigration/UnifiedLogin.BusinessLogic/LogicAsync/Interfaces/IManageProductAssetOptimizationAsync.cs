using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async interface for Asset Optimization product operations.
/// DefaultUserClaim is passed per-call because ManageProductAssetOptimization requires it at construction time.
/// </summary>
public interface IManageProductAssetOptimizationAsync
{
    Task<ListResponse> GetCompaniesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertyGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, IList<string> selectedCompanies, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesInGroupAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, int propertyGroupId, CancellationToken cancellationToken = default);

    Task<ListResponse> GetCompaniesWithRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default);

    Task<ListResponse> GetCompaniesWithPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default);

    Task<ListResponse> GetOperatorsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesWithOperatorsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string operatorCode, string operatorValue, CancellationToken cancellationToken = default);

    Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, string firstName, string lastName, CancellationToken cancellationToken = default);

    Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default);
}
