using System.Text.Json;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.IntegrationTypes;

/// <summary>
/// Native-async implementation of <see cref="IIntegrationTypeAsync"/> for products
/// using the StandardV1 HTTP integration pattern.
/// Replaces <c>StandardV1IntegrationType</c> (sync).
/// <para>
/// A fresh <see cref="StandardV1ProductIntegrationAsync"/> instance is created and
/// initialised (<see cref="StandardV1ProductIntegrationAsync.InitAsync"/>) per method
/// call, exactly as the sync version called <c>new StandardV1ProductIntegration(…)</c>
/// per method.
/// </para>
/// </summary>
public sealed class StandardV1IntegrationTypeAsync : IIntegrationTypeAsync
{
    private readonly int _productId;
    private readonly IDataCollectorAsync _dataCollector;
    private readonly IProductRepositoryAsync _productRepository;
    private readonly IManagePersonaAsync _managePersona;
    private readonly IManageUserLoginAsync _manageUserLogin;
    private readonly IUserClaimsAccessor _userClaimsAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenHelperAsync _tokenHelper;
    private readonly ICacheService _cacheService;
    private readonly ILoggerFactory _loggerFactory;

    public StandardV1IntegrationTypeAsync(
        int                     productId,
        IDataCollectorAsync     dataCollector,
        IProductRepositoryAsync productRepository,
        IManagePersonaAsync     managePersona,
        IManageUserLoginAsync   manageUserLogin,
        IUserClaimsAccessor     userClaimsAccessor,
        IHttpClientFactory      httpClientFactory,
        ITokenHelperAsync       tokenHelper,
        ICacheService           cacheService,
        ILoggerFactory          loggerFactory)
    {
        _productId        = productId;
        _dataCollector    = dataCollector;
        _productRepository = productRepository;
        _managePersona    = managePersona;
        _manageUserLogin  = manageUserLogin;
        _userClaimsAccessor = userClaimsAccessor;
        _httpClientFactory = httpClientFactory;
        _tokenHelper      = tokenHelper;
        _cacheService     = cacheService;
        _loggerFactory    = loggerFactory;
    }

    // ── Factory helper ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="StandardV1ProductIntegrationAsync"/> scoped to the given
    /// personas and calls <see cref="StandardV1ProductIntegrationAsync.InitAsync"/> before
    /// returning it. Mirrors the sync pattern of <c>new StandardV1ProductIntegration(…)</c>
    /// per-method.
    /// </summary>
    private async Task<StandardV1ProductIntegrationAsync> CreateAndInitAsync(
        long editorPersonaId, long subjectPersonaId, CancellationToken ct)
    {
        var integration = new SelfGuidedTourAsync(
            _productId, editorPersonaId, subjectPersonaId,
            _dataCollector, _productRepository, _managePersona, _manageUserLogin,
            _userClaimsAccessor, _httpClientFactory, _tokenHelper, _cacheService, _loggerFactory);

        await integration.InitAsync(ct);
        return integration;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetProductPropertiesAsync(dataFilter, ct: ct);
    }

    /// <inheritdoc/>
    public Task<ListResponse> GetEnterprisePropertiesAsync(
        long userPersonaId, RequestParameter dataFilter, CancellationToken ct = default)
        => GetPropertiesAsync(userPersonaId, userPersonaId, dataFilter, ct);

    /// <inheritdoc/>
    /// <remarks>
    /// StandardV1 products that use <c>GetPropertyGroupsAsync</c> for the KnockCRM-style
    /// property groups tab delegate to <see cref="IManageProductIntegrationAsync.GetProductPropertyGroupsAsync"/>;
    /// all other StandardV1 products fall back to <see cref="IManageProductIntegrationAsync.GetAllRightsAsync"/>
    /// because the base class has no concept of property groups.
    /// </remarks>
    public async Task<ListResponse> GetPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, string userLoginName = "",
        CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetProductPropertyGroupsAsync(dataFilter, ct: ct);
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesByGroupAsync(
        long editorPersonaId, long userPersonaId,
        string propertyGroupId, RequestParameter dataFilter, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetProductPropertiesByGroupAsync(propertyGroupId, dataFilter, ct: ct);
    }

    // ── Organizations ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetOrganizationsAsync(
        long editorPersonaId, long userPersonaId,
        string organizationRoleId, string organizationType, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetProductOrganizationsAsync(organizationRoleId, organizationType, ct: ct);
    }

    // ── Roles & Rights ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        long partyId, AccessType? accessType, RequestParameter dataFilter, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetProductRolesAsync(dataFilter, ct: ct);
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId, long userPersonaId,
        long roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter,
        CancellationToken ct = default)
    {
        // StandardV1 numeric role IDs are not natively supported — return empty
        return new ListResponse();
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsForRoleAsync(
        long editorPersonaId, long userPersonaId,
        string roleId, long partyId, bool assignedToRoleOnly, RequestParameter dataFilter,
        CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetProductRightsForRoleAsync(dataFilter, roleId, ct: ct);
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetAllRightsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetAllRightsAsync(dataFilter, ct: ct);
    }

    // ── User Groups ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserGroupsAsync(
        long editorPersonaId, long userPersonaId,
        long partyId, RequestParameter dataFilter, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, userPersonaId, ct);
        return await i.GetProductUserGroupsAsync(dataFilter, ct: ct);
    }

    // ── User Creation & Mutation ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string result, List<AdditionalParameters> auditParams)> CreateUserAsync(
        ProductUserProperitiesRoles productUser, CancellationToken ct = default)
    {
        var rpList = TryDeserialize<ProductUserRolePropertiesGroups>(productUser.InputJson);
        if (rpList is null)
            return ("Input JSON parsing issue; Null object.", []);

        var i = await CreateAndInitAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct);

        if (rpList.IsAssigned)
            return await i.CreateUpdateProductUserAsync(rpList, productUser.BatchProcessType, ct);

        var unassignResult = await i.UnassignUserAsync(ct);
        return (unassignResult, []);
    }

    /// <inheritdoc/>
    public async Task<string> ChangeUserTypeAsync(
        ProductUserProperitiesRoles batchRecord, CancellationToken ct = default)
    {
        var rpList = TryDeserialize<ProductUserRolePropertiesGroups>(batchRecord.InputJson);
        if (rpList is null)
            return "Input JSON parsing issue; Null object.";

        var i = await CreateAndInitAsync(batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId, ct);
        return await i.ChangeProductUserTypeAsync(rpList, batchRecord.BatchProcessType, ct);
    }

    /// <inheritdoc/>
    public async Task<string> UpdateUserProfileAsync(
        ProductUserProperitiesRoles productUser, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, ct);
        return await i.UpdateProductUserProfileAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<string> UpdateUserDetailsAsync(
        ProductUserAccountDetails productUserAccountDetails,
        bool internalChange = false, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(
            productUserAccountDetails.PersonaId,
            productUserAccountDetails.PersonaId, ct);
        return await i.UpdateProductUserProfileAsync(ct);
    }

    // ── Migration ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter dataFilter, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, editorPersonaId, ct);
        return await i.GetMigrationUsersAsync(dataFilter, ct);
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, editorPersonaId, ct);
        return await i.UpdateUsersMigrationStatusAsync(migrateUsers, ct);
    }

    // ── External Profile Sync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ExternalUserProfileChangeAsync(
        long editorPersonaId, ProductUserProfile productUserProfile, CancellationToken ct = default)
    {
        var i = await CreateAndInitAsync(editorPersonaId, editorPersonaId, ct);
        return await i.ExternalProductUserProfileChangeAsync(productUserProfile, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static T? TryDeserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return System.Text.Json.JsonSerializer.Deserialize<T>(json.Trim()); }
        catch { return default; }
    }
}
