using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for Asset Optimization product operations.
/// Encapsulates the legacy <see cref="ManageProductAssetOptimization"/> per-call construction
/// pattern (the class requires <see cref="DefaultUserClaim"/> at construction time) behind a
/// mockable async interface.
/// </summary>
public sealed class ManageProductAssetOptimizationAsync : IManageProductAssetOptimizationAsync
{
    public Task<ListResponse> GetCompaniesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetCompanies(editorPersonaId, userPersonaId, productName, datafilter, userLoginName));

    public Task<ListResponse> GetPropertyGroupsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, IList<string> selectedCompanies, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetPropertyGroups(editorPersonaId, userPersonaId, productName, selectedCompanies));

    public Task<ListResponse> GetPropertiesInGroupAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, int propertyGroupId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetPropertiesInGroup(editorPersonaId, userPersonaId, propertyGroupId));

    public Task<ListResponse> GetCompaniesWithRolesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetCompaniesWithRoles(editorPersonaId, userPersonaId, productName, datafilter, userLoginName));

    public Task<ListResponse> GetCompaniesWithPropertiesAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "", CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetCompaniesWithProperties(editorPersonaId, userPersonaId, productName, datafilter, userLoginName));

    public Task<ListResponse> GetOperatorsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetOperators(editorPersonaId, userPersonaId));

    public Task<ListResponse> GetPropertiesWithOperatorsAsync(DefaultUserClaim userClaim, long editorPersonaId, long userPersonaId, string operatorCode, string operatorValue, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetPropertiesWithOperators(editorPersonaId, userPersonaId, operatorCode, operatorValue));

    public Task<bool> ChangeUserStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, string userName, string firstName, string lastName, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).ChangeUserStatus(editorPersonaId, userName, firstName, lastName));

    public Task<ListResponse> GetMigrationUsersAsync(DefaultUserClaim userClaim, long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).GetMigrationUsers(editorPersonaId, datafilter));

    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(DefaultUserClaim userClaim, long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
        => Task.FromResult(new ManageProductAssetOptimization(userClaim).UpdateUsersMigrationStatus(editorPersonaId, migrateUsers));
}
