using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Native-async interface for Asset Optimization product operations.
/// All per-call <c>DefaultUserClaim</c> parameters removed — the service resolves the
/// current user context via <see cref="IUserClaimsAccessor"/> injected at construction time.
/// </summary>
public interface IManageProductAssetOptimizationAsync
{
    // ── Company / Role / Property reads ─────────────────────────────────────

    Task<ListResponse> GetCompaniesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetCompaniesWithRolesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetCompaniesWithPropertiesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetProductRolesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetProductPropertiesAsync(
        long editorPersonaId, long userPersonaId, string productName,
        RequestParameter datafilter, string userLoginName = "",
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetOperatorsAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesWithOperatorsAsync(
        long editorPersonaId, long userPersonaId,
        string operatorCode, string operatorValue,
        CancellationToken cancellationToken = default);

    // ── Property groups ──────────────────────────────────────────────────────

    Task<ListResponse> GetPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId, string productName,
        IList<string> selectedCompanies,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetProductPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId, string productName,
        string userLoginName,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetPropertiesInGroupAsync(
        long editorPersonaId, long userPersonaId, int propertyGroupId,
        CancellationToken cancellationToken = default);

    Task<ListResponse> GetGroupPropertiesAsync(
        long editorPersonaId, long userPersonaId, int propertyGroupId, int productId,
        CancellationToken cancellationToken = default);

    // ── Migrations ────────────────────────────────────────────────────────────

    Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken cancellationToken = default);

    Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken cancellationToken = default);

    // ── User status / profile ────────────────────────────────────────────────

    Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string firstName, string lastName,
        CancellationToken cancellationToken = default);

    Task<string> UpdateUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken cancellationToken = default);

    // ── Product assignment ────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates a user in the AO/BI product.
    /// Replaces the sync <c>ManageAssetOptimizationUser</c> (which used an <c>out</c> parameter
    /// for activity-log details — now returned as part of the result tuple).
    /// </summary>
    Task<(string Result, List<AdditionalParameters> ActivityLog)> ManageAssetOptimizationUserAsync(
        long editorPersonaId, long productUserPersonaId,
        IList<Logic.Product.AoUserCompanyPropertyRoleDetail> aoDetails,
        SharedObjects.Enum.BatchProcessType batchProcessType = SharedObjects.Enum.BatchProcessType.CreateUpdateProductUser,
        CancellationToken cancellationToken = default);

    // ── Editor product discovery ──────────────────────────────────────────────

    Task<IList<string>> GetGbSupportedAoEditorUserProductsToAssignAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default);

    Task<IList<string>> GetGbSupportedAoProductsWithUserAdminRoleAsync(
        long editorPersonaId,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetAOProductsForNewMultiCompanyUserAsync(
        long editorPersonaId, string loginName,
        CancellationToken cancellationToken = default);

    // ── Clone helpers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the AO product assignments of <paramref name="subjectPersonaId"/> that are
    /// visible to <paramref name="editorPersonaId"/>.
    /// When <paramref name="productUserName"/> is provided it overrides the SAML product
    /// login-name lookup for the subject (used for the AO/BI external-user clone path).
    /// </summary>
    Task<IList<AoUserCompanyPropertyRoleDetail>> CopyRegularUserAsync(
        long editorPersonaId,
        long subjectPersonaId,
        string? productUserName = null,
        CancellationToken cancellationToken = default);
}
