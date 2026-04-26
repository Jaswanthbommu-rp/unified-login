using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations.ProductImplementation;

/// <summary>
/// Native-async concrete implementation for the Portfolio Management product integration.
/// Replaces <c>PortfolioManagement</c> (sync).
/// <para>
/// <b>Key differences from the standard V1 base class:</b>
/// <list type="bullet">
///   <item><see cref="ResolveApiSecurityAsync"/> — overrides the base to obtain a
///     client-credentials bearer token directly from the product's own token endpoint
///     (configured via <c>ApiEndpoint</c>, <c>TokenClientId</c>, <c>TokenClientSecret</c>
///     internal settings). Uses <see cref="ITokenHelperAsync.GetExternalClientCredentialServerTokenAsync"/>.</item>
///   <item><see cref="GetProductRolesAsync"/> — requests <em>global</em> roles by passing
///     <c>"true"</c> as the <c>isGlobalRoles</c> query parameter.</item>
///   <item><see cref="GetProductUserAsync"/> — formats the URL with both
///     <c>CompanyInstanceSourceId</c> and <c>ProductUserName</c>.</item>
///   <item><see cref="GetProductPropertiesAsync"/> — returns a composite
///     <see cref="PortfolioRoleProperty"/> list combining non-global roles,
///     properties, and property groups. Existing user assignments are merged.</item>
///   <item><see cref="CheckUserExistInProductAsync"/> — URL uses both company and login name.</item>
///   <item><see cref="CreateAdditionalSamlUserAttributeAsync"/> — creates a
///     <c>PMCID</c> SAML attribute from the user's <c>CompanyId</c>.</item>
/// </list>
/// </para>
/// <para>
/// <b>Protocol note</b>: the sync version used HTTP Basic authentication for the token
/// request (credentials in the <c>Authorization</c> header). The async version uses
/// <c>ITokenHelperAsync.GetExternalClientCredentialServerTokenAsync</c> which sends
/// credentials in the request body per the OAuth 2.0 specification.
/// If the Portfolio Management token endpoint requires header-based credentials this
/// override may need revisiting (see <c>ProductIntegrationPhase4-Refactor.md</c>).
/// </para>
/// </summary>
public sealed class PortfolioManagementAsync : StandardV1ProductIntegrationAsync
{
    private readonly ILogger<PortfolioManagementAsync> _portfolioLogger;

    /// <summary>
    /// Initialises a new instance. Call <see cref="StandardV1ProductIntegrationAsync.InitAsync"/>
    /// before using any public methods.
    /// </summary>
    public PortfolioManagementAsync(
        int                     productId,
        long                    editorPersonaId,
        long                    subjectPersonaId,
        IDataCollectorAsync     dataCollector,
        IProductRepositoryAsync productRepository,
        IManagePersonaAsync     managePersona,
        IManageUserLoginAsync   manageUserLogin,
        IUserClaimsAccessor     userClaimsAccessor,
        IHttpClientFactory      httpClientFactory,
        ITokenHelperAsync       tokenHelper,
        ICacheService           cacheService,
        ILoggerFactory          loggerFactory)
        : base(productId, editorPersonaId, subjectPersonaId,
               dataCollector, productRepository, managePersona, manageUserLogin,
               userClaimsAccessor, httpClientFactory, tokenHelper, cacheService, loggerFactory)
    {
        _portfolioLogger = loggerFactory.CreateLogger<PortfolioManagementAsync>();
    }

    // ── Overrides ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Overrides the base security resolution to use the Portfolio Management product's
    /// own OAuth 2.0 token endpoint (client-credentials grant).
    /// Token URI, client ID and client secret are read from <c>ProductInternalSettingList</c>
    /// (<c>ApiEndpoint</c>, <c>TokenClientId</c>, <c>TokenClientSecret</c>).
    /// </remarks>
    protected override async Task<(AuthenticationHeaderValue?, IReadOnlyDictionary<string, string>)>
        ResolveApiSecurityAsync(CancellationToken ct)
    {
        try
        {
            string tokenIssueUri = ProductInternalSettingList
                .First(s => s.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
            string tokenClientId = ProductInternalSettingList
                .First(s => s.Name.Equals("TOKENCLIENTID", StringComparison.OrdinalIgnoreCase)).Value;
            string tokenClientSecret = ProductInternalSettingList
                .First(s => s.Name.Equals("TOKENCLIENTSECRET", StringComparison.OrdinalIgnoreCase)).Value;

            // GetExternalClientCredentialServerTokenAsync sends credentials in the request body
            // (OAuth 2.0 standard). The sync version used Basic auth header — see changelog note.
            string accessToken = await _tokenHelper.GetExternalClientCredentialServerTokenAsync(
                tokenIssueUri + "/token",
                tokenClientId,
                tokenClientSecret,
                scopes: string.Empty,
                ct);

            return (new AuthenticationHeaderValue("Bearer", accessToken),
                    new Dictionary<string, string>());
        }
        catch (Exception ex)
        {
            _portfolioLogger.LogError(ex, "[{Method}] Product={ProductId} Failed to obtain Portfolio token",
                nameof(ResolveApiSecurityAsync), ProductId);
            throw new InvalidOperationException($"Error obtaining Portfolio Management access token: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Requests <em>global</em> roles by formatting the endpoint with <c>"true"</c>
    /// for the <c>isGlobalRoles</c> parameter.
    /// Uses the standard <see cref="ProductRole"/> list (not a ClickPay-style wrapper).
    /// </remarks>
    public override async Task<ListResponse> GetProductRolesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            // isGlobalRoles = "true" for Portfolio
            baseUrlAndQuery = string.Format(
                GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint),
                CompanyInstanceSourceId, "true");

            var roleList = await GetResultFromApiAsync<IList<ProductRole>>(baseUrlAndQuery, ct: ct)
                           ?? throw new InvalidOperationException("Null Role List from product API.");

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                var user = await GetProductUserAsync(ct: ct);
                if (user is not null)
                    MergeUserRoles(roleList, user.RoleList);
            }

            return ToListResponse(roleList);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductRolesAsync));
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Portfolio user URLs use two parameters: <c>CompanyInstanceSourceId</c> and
    /// <c>ProductUserName</c>.
    /// </remarks>
    public override async Task<IntegrationProductUser?> GetProductUserAsync(
        string? baseUrlAndQuery = null,
        bool isThrowOnError = true,
        CancellationToken ct = default)
    {
        baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);
        baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails?.ProductUserName);

        return await GetResultFromApiAsync<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError, ct);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Builds a composite <see cref="PortfolioRoleProperty"/> list by combining:
    /// <list type="number">
    ///   <item>All properties (via <c>GetPropertyEndpoint</c>).</item>
    ///   <item>All property groups (via <c>GetPropertyGroupsEndpoint</c>).</item>
    ///   <item>All non-global / property-specific roles (via <c>GetRoleEndpoint</c> with <c>isGlobalRoles=false</c>).</item>
    /// </list>
    /// Each role becomes a <see cref="PortfolioRoleProperty"/> node that contains the
    /// full property and group lists. If the subject user already exists, their
    /// <c>PropertyRoleList</c> is merged to mark assigned entries.
    /// </remarks>
    public override async Task<ListResponse> GetProductPropertiesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        // Fetch all three building blocks in parallel for performance
        var propertiesTask      = GetPortfolioPropertiesAsync(ct);
        var propertyGroupsTask  = GetPortfolioPropertyGroupsAsync(ct);
        var propertyRolesTask   = GetPortfolioPropertySpecificRolesAsync(ct);

        await Task.WhenAll(propertiesTask, propertyGroupsTask, propertyRolesTask);

        // Awaiting already-completed tasks returns synchronously — safe after WhenAll
        var allProperties     = propertiesTask.Result.ToList();
        var allPropertyGroups = propertyGroupsTask.Result.ToList();
        var allPropertyRoles  = propertyRolesTask.Result.ToList();

        // Build role–property composite
        IList<PortfolioRoleProperty> propertiesList = allPropertyRoles
            .Select(role => new PortfolioRoleProperty(role, allProperties, allPropertyGroups))
            .ToList();

        // Merge existing user assignments
        if (SubjectUserDetails is not null && !string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
        {
            var userUrl = string.Format(
                GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint),
                CompanyInstanceSourceId, SubjectUserDetails.ProductUserName);

            var user = await GetResultFromApiAsync<IntegrationProductUser>(userUrl, ct: ct);
            if (user is not null)
                MergePropertyRoles(propertiesList, user.PropertyRoleList);
        }

        return ToListResponse(propertiesList);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// URL uses both <c>CompanyInstanceSourceId</c> and the candidate login name.
    /// </remarks>
    protected override async Task<bool> CheckUserExistInProductAsync(
        string loginNameToCheck, string? baseUrlAndQuery = null, CancellationToken ct = default)
    {
        baseUrlAndQuery = string.Format(
            GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint),
            CompanyInstanceSourceId, loginNameToCheck);

        var user = await GetResultFromApiAsync<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError: false, ct);
        return user is { UserId: { Length: > 0 } };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Creates a <c>PMCID</c> SAML attribute from the user's <c>CompanyId</c>.
    /// This corresponds to the Portfolio Management SAML requirement for PMC identification.
    /// </remarks>
    protected override async Task CreateAdditionalSamlUserAttributeAsync(
        long personaId, int productId, IntegrationProductUser productUser, CancellationToken ct)
    {
        _portfolioLogger.LogDebug("[{Method}] Product={ProductId} loginName={LoginName} PMC={PMC}",
            nameof(CreateAdditionalSamlUserAttributeAsync), ProductId,
            productUser.LoginName, productUser.CompanyId);

        await _dataCollector.CreateSamlUserAttributeAsync(
            personaId, productId, SamlAttributeEnum.PMCID, productUser.CompanyId ?? string.Empty, ct);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<IList<ProductProperties>> GetPortfolioPropertiesAsync(CancellationToken ct)
    {
        var url = string.Format(
            GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint),
            CompanyInstanceSourceId);
        return await GetResultFromApiAsync<IList<ProductProperties>>(url, ct: ct) ?? [];
    }

    private async Task<IList<ProductPropertyGroups>> GetPortfolioPropertyGroupsAsync(CancellationToken ct)
    {
        var url = string.Format(
            GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupsEndpoint),
            CompanyInstanceSourceId);
        return await GetResultFromApiAsync<IList<ProductPropertyGroups>>(url, ct: ct) ?? [];
    }

    private async Task<IList<ProductRole>> GetPortfolioPropertySpecificRolesAsync(CancellationToken ct)
    {
        // isGlobalRoles = "false" for property-specific roles
        var url = string.Format(
            GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint),
            CompanyInstanceSourceId, "false");
        return await GetResultFromApiAsync<IList<ProductRole>>(url, ct: ct) ?? [];
    }

    /// <summary>
    /// Merges the user's existing <see cref="PAMRolePropertyList"/> assignments back into
    /// the composite <see cref="PortfolioRoleProperty"/> list.
    /// </summary>
    private static void MergePropertyRoles(
        IList<PortfolioRoleProperty> portfolioPropertyRoles,
        List<PAMRolePropertyList>?   userPropertyRoles)
    {
        if (userPropertyRoles is null) return;

        foreach (var role in userPropertyRoles)
        {
            foreach (var propRoleEntry in portfolioPropertyRoles.Where(x => x.GetRoleId == role.RoleId))
            {
                if (propRoleEntry.PropertiesList.Any(y => role.PropertyIds.Contains(y.GetPropertyId)))
                {
                    propRoleEntry.IsAssigned = true;
                    foreach (var prop in propRoleEntry.PropertiesList
                                 .Where(z => role.PropertyIds.Contains(z.GetPropertyId)))
                        prop.IsAssigned = true;
                }

                if (propRoleEntry.GroupList.Any(y => role.PropertyGroupList.Contains(y.GetGroupId)))
                {
                    propRoleEntry.IsAssigned = true;
                    foreach (var group in propRoleEntry.GroupList
                                 .Where(z => role.PropertyGroupList.Contains(z.GetGroupId)))
                        group.IsAssigned = true;
                }
            }
        }
    }
}

// ── Nested DTO ────────────────────────────────────────────────────────────────

/// <summary>
/// Combines a <see cref="ProductRole"/> with the full property and group lists
/// for Portfolio Management's role-property model.
/// </summary>
public sealed class PortfolioRoleProperty : ProductRole
{
    public PortfolioRoleProperty(
        ProductRole                role,
        List<ProductProperties>    properties,
        List<ProductPropertyGroups> groups)
    {
        SetName   = role.GetName;
        SetRoleId = role.GetRoleId;

        PropertiesList = properties
            .Select(a => new ProductProperties
            {
                SetPropertyId = a.GetPropertyId,
                SetName       = a.GetName,
                PropertyType  = a.PropertyType
            })
            .ToList();

        GroupList = groups
            .Select(a => new ProductPropertyGroups
            {
                SetGroupId   = a.GetGroupId,
                SetGroupName = a.GetGroupName
            })
            .ToList();
    }

    /// <summary>All properties available for this role (assigned flag merged from user data).</summary>
    public List<ProductProperties> PropertiesList { get; set; }

    /// <summary>All property groups available for this role (assigned flag merged from user data).</summary>
    public List<ProductPropertyGroups> GroupList { get; set; }
}
