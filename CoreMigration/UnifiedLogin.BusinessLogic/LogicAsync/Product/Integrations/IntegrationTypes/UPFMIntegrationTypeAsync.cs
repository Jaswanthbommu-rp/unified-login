using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.UPFMProduct;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.IntegrationTypes;

/// <summary>
/// Native-async implementation of <see cref="IIntegrationTypeAsync"/> for products
/// using the UPFM integration pattern.
/// Replaces <c>UPFMIntegrationType</c> (sync).
/// <para>
/// <b>Supported operations</b>: roles, properties (standard + enterprise), rights by role,
/// create user, change user type, unassign user (via UPFM role/property removal).
/// </para>
/// <para>
/// <b>Not applicable</b>: property groups, properties-by-group, organizations, user groups,
/// migration, external profile sync — these throw <see cref="NotSupportedException"/> matching
/// the sync <c>throw new NotImplementedException()</c> stubs.
/// </para>
/// </summary>
public sealed class UPFMIntegrationTypeAsync : IIntegrationTypeAsync
{
    private readonly int _productId;
    private readonly IManageUPFMProductsIntegrationAsync _upfm;
    private readonly IProductRepositoryAsync _productRepository;

    public UPFMIntegrationTypeAsync(
        int                                  productId,
        IManageUPFMProductsIntegrationAsync  upfm,
        IProductRepositoryAsync              productRepository)
    {
        _productId        = productId;
        _upfm             = upfm;
        _productRepository = productRepository;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken ct = default)
        => _upfm.GetUPFMPropertiesAsync(editorPersonaId, userPersonaId, assignedOnly: false, dataFilter, ct);

    /// <inheritdoc/>
    /// <remarks>
    /// Resolves the product's BooksProductCode from the product repository,
    /// then delegates to <see cref="IManageUPFMProductsIntegrationAsync.GetEnterpriseUPFMPropertiesAsync"/>.
    /// </remarks>
    public async Task<ListResponse> GetEnterprisePropertiesAsync(
        long userPersonaId, RequestParameter dataFilter, CancellationToken ct = default)
    {
        var products    = await _productRepository.GetAllProductsAsync(ct);
        var productCode = products.FirstOrDefault(p => p.ProductId == _productId)?.BooksProductCode
                          ?? string.Empty;

        return await _upfm.GetEnterpriseUPFMPropertiesAsync(
            userPersonaId, userPersonaId, _productId, productCode, cancellationToken: ct);
    }

    /// <inheritdoc/>
    public Task<ListResponse> GetPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, string userLoginName = "", CancellationToken ct = default)
        => throw new NotSupportedException("GetPropertyGroups is not applicable for UPFM products.");

    /// <inheritdoc/>
    public Task<ListResponse> GetPropertiesByGroupAsync(
        long editorPersonaId, long userPersonaId,
        string propertyGroupId, RequestParameter dataFilter, CancellationToken ct = default)
        => throw new NotSupportedException("GetPropertiesByGroup is not applicable for UPFM products.");

    // ── Organizations ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<ListResponse> GetOrganizationsAsync(
        long editorPersonaId, long userPersonaId,
        string organizationRoleId, string organizationType, CancellationToken ct = default)
        => throw new NotSupportedException("GetOrganizations is not applicable for UPFM products.");

    // ── Roles & Rights ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        long partyId, AccessType? accessType, RequestParameter dataFilter, CancellationToken ct = default)
        => _upfm.GetRolesAsync(editorPersonaId, userPersonaId, partyId, ct);

    /// <inheritdoc/>
    public Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId, long userPersonaId,
        long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter,
        CancellationToken ct = default)
        => _upfm.GetRightsByRoleAsync(editorPersonaId, partyId, roleId, ct);

    /// <inheritdoc/>
    public Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId, long userPersonaId,
        string roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter,
        CancellationToken ct = default)
        => throw new NotSupportedException("String-based role IDs are not used by UPFM products.");

    /// <inheritdoc/>
    public Task<ListResponse> GetAllRightsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken ct = default)
        => throw new NotSupportedException("GetAllRights is not applicable for UPFM products.");

    // ── User Groups ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<ListResponse> GetUserGroupsAsync(
        long editorPersonaId, long userPersonaId,
        long partyId, RequestParameter dataFilter, CancellationToken ct = default)
        => throw new NotSupportedException("GetUserGroups is not applicable for UPFM products.");

    // ── User Creation & Mutation ──────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Deserialises <c>productUser.InputJson</c> as <see cref="UPFMProductPropertyRole"/>
    /// and delegates to <see cref="IManageUPFMProductsIntegrationAsync.ManageUPFMProductUserAsync"/>.
    /// </remarks>
    public async Task<(string result, List<AdditionalParameters> auditParams)> CreateUserAsync(
        ProductUserProperitiesRoles productUser, CancellationToken ct = default)
    {
        var upfmRoles = TryDeserialize<UPFMProductPropertyRole>(productUser.InputJson);
        if (upfmRoles is null)
            return ("Input JSON parsing issue; Null object.", []);

        var (result, auditParams) = await _upfm.ManageUPFMProductUserAsync(
            productUser.CreateUserPersonaId,
            productUser.AssignUserPersonaId,
            upfmRoles,
            isEmpAccess: productUser.CreateRealPageEmployee,
            ct);

        return (result, auditParams);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// UPFM change-user-type delegates to
    /// <see cref="IManageUPFMProductsIntegrationAsync.ManageUPFMProductUserAsync"/> with
    /// the batch process type recorded in <paramref name="batchRecord"/>.
    /// </remarks>
    public async Task<string> ChangeUserTypeAsync(
        ProductUserProperitiesRoles batchRecord, CancellationToken ct = default)
    {
        var upfmRoles = TryDeserialize<UPFMProductPropertyRole>(batchRecord.InputJson);
        if (upfmRoles is null)
            return "Input JSON parsing issue; Null object.";

        var (result, _) = await _upfm.ManageUPFMProductUserAsync(
            batchRecord.CreateUserPersonaId,
            batchRecord.AssignUserPersonaId,
            upfmRoles,
            isEmpAccess: batchRecord.CreateRealPageEmployee,
            ct);

        return result;
    }

    /// <inheritdoc/>
    public Task<string> UpdateUserProfileAsync(
        ProductUserProperitiesRoles productUser, CancellationToken ct = default)
        => throw new NotSupportedException("UpdateUserProfile is not applicable for UPFM products.");

    /// <inheritdoc/>
    public Task<string> UpdateUserDetailsAsync(
        ProductUserAccountDetails productUserAccountDetails,
        bool internalChange = false, CancellationToken ct = default)
        => throw new NotSupportedException("UpdateUserDetails is not applicable for UPFM products.");

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter dataFilter, CancellationToken ct = default)
        => throw new NotSupportedException("GetMigrationUsers is not applicable for UPFM products.");

    /// <inheritdoc/>
    public Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default)
        => throw new NotSupportedException("UpdateUsersMigrationStatus is not applicable for UPFM products.");

    // ── External Profile Sync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public Task<bool> ExternalUserProfileChangeAsync(
        long editorPersonaId, ProductUserProfile productUserProfile, CancellationToken ct = default)
        => throw new NotSupportedException("ExternalUserProfileChange is not applicable for UPFM products.");

    // ── Private helpers ───────────────────────────────────────────────────────

    private static T? TryDeserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return System.Text.Json.JsonSerializer.Deserialize<T>(json.Trim()); }
        catch { return default; }
    }
}
