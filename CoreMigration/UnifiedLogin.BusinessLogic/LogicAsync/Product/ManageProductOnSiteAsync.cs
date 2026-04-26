using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Onsite;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// True async implementation of On-Site (1S) REST API user-management operations.
/// Replaces the stepping-stone <c>ManageProductOnSite</c> wrapper that required a
/// <see cref="DefaultUserClaim"/> at construction time. Context is resolved internally via
/// <see cref="IProductContextServiceAsync"/> from the supplied persona IDs.
/// </summary>
public sealed class ManageProductOnSiteAsync : IManageProductOnSiteAsync
{
    private const int    ProductId               = (int)ProductEnum.OnSite;
    private const string ProductStatusSettingType = "ProductStatus";
    private const string TokenCacheKey           = "onsite_access_token";
    private const int    TokenCacheMinutes       = 9;

    // Audit-trail message templates (mirrors ManageProductBase constants)
    private const string RoleAssignMessage   = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RoleRemovedMessage  = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropAssignMessage   = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropRemovedMessage  = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";

    // STJ options: snake_case naming policy handles most field names automatically;
    // [JsonPropertyName] overrides are used for the few that diverge.
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy       = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition     = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IProductContextServiceAsync                _contextService;
    private readonly IProductSettingServiceAsync                _settingService;
    private readonly IManageBlueBookAsync                       _blueBook;
    private readonly IHttpClientFactory                         _httpClientFactory;
    private readonly IMemoryCache                               _cache;
    private readonly ILogger<ManageProductOnSiteAsync>          _logger;
    private readonly ISamlRepositoryAsync                       _samlRepository;
    private readonly IManagePersonaAsync                        _managePersona;
    private readonly IManagePersonAsync                         _managePerson;
    private readonly IManageUserLoginAsync                      _manageUserLogin;
    private readonly IManageElectronicAddressAsync              _manageElectronicAddress;

    public ManageProductOnSiteAsync(
        IProductContextServiceAsync       contextService,
        IProductSettingServiceAsync       settingService,
        IManageBlueBookAsync              blueBook,
        IHttpClientFactory                httpClientFactory,
        IMemoryCache                      cache,
        ILogger<ManageProductOnSiteAsync> logger,
        ISamlRepositoryAsync              samlRepository,
        IManagePersonaAsync               managePersona,
        IManagePersonAsync                managePerson,
        IManageUserLoginAsync             manageUserLogin,
        IManageElectronicAddressAsync     manageElectronicAddress)
    {
        ArgumentNullException.ThrowIfNull(contextService);       _contextService       = contextService;
        ArgumentNullException.ThrowIfNull(settingService);       _settingService       = settingService;
        ArgumentNullException.ThrowIfNull(blueBook);             _blueBook             = blueBook;
        ArgumentNullException.ThrowIfNull(httpClientFactory);    _httpClientFactory    = httpClientFactory;
        ArgumentNullException.ThrowIfNull(cache);                _cache                = cache;
        ArgumentNullException.ThrowIfNull(logger);               _logger               = logger;
        ArgumentNullException.ThrowIfNull(samlRepository);       _samlRepository       = samlRepository;
        ArgumentNullException.ThrowIfNull(managePersona);        _managePersona        = managePersona;
        ArgumentNullException.ThrowIfNull(managePerson);         _managePerson         = managePerson;
        ArgumentNullException.ThrowIfNull(manageUserLogin);      _manageUserLogin      = manageUserLogin;
        ArgumentNullException.ThrowIfNull(manageElectronicAddress); _manageElectronicAddress = manageElectronicAddress;
    }

    // ── Private per-call context ───────────────────────────────────────────────

    /// <summary>
    /// Immutable per-call context resolved once by <see cref="GetOnSiteContextAsync"/>.
    /// Replaces the five mutable fields <c>_accessToken</c>, <c>_apiEndPoint</c>,
    /// <c>_productUserId</c>, <c>_productUsername</c>, and <c>_companyInstanceSourceId</c>
    /// that <c>ManageProductOnSite</c> set at construction / <c>GetCompanyEditorAndUserDetails</c>.
    /// </summary>
    private sealed record OnSiteCtx(
        Persona EditorPersona,
        string  ProductUserId,
        string  ProductUsername,
        string  AccessToken,
        string  ApiEndPoint,
        int     CompanyInstanceSourceId);

    /// <summary>Loaded-once API credential bundle; cached per-call from product internal settings.</summary>
    private sealed record OnSiteApiConfig(
        string ApiEndPoint,
        string ApiSecret,
        string ClientId,
        string TokenEndPoint);

    // ── Context resolution ─────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the full On-Site call context for a single method invocation.
    /// Replaces <c>ManageProductOnSite.GetCompanyEditorAndUserDetails</c> +
    /// constructor-time credential loading.
    /// </summary>
    private async Task<(OnSiteCtx? ctx, ListResponse? error)> GetOnSiteContextAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct)
    {
        var (callCtx, ctxError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return (null, ctxError);

        OnSiteApiConfig config;
        try   { config = await GetApiConfigAsync(ct); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOnSiteContextAsync – failed to load API config");
            return (null, new ListResponse { IsError = true, ErrorReason = "Product configuration error." });
        }

        string token;
        try   { token = await GetAccessTokenAsync(config, ct); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOnSiteContextAsync – failed to obtain access token");
            return (null, new ListResponse { IsError = true, ErrorReason = "Authentication failed." });
        }

        int companyId = await GetCompanyInstanceIdAsync(callCtx!.EditorPersona, ct);
        if (companyId == 0)
            return (null, new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." });

        return (new OnSiteCtx(
            callCtx.EditorPersona,
            callCtx.ProductUserId,
            callCtx.ProductUsername,
            token,
            config.ApiEndPoint,
            companyId), null);
    }

    private async Task<OnSiteApiConfig> GetApiConfigAsync(CancellationToken ct)
    {
        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);

        string Setting(string name) =>
            settings.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value
            ?? throw new InvalidOperationException($"Missing On-Site product setting: {name}");

        return new OnSiteApiConfig(
            Setting("APIENDPOINT"),
            Setting("APISECRET"),
            Setting("CLIENTID"),
            Setting("TOKENURL"));
    }

    private async Task<string> GetAccessTokenAsync(OnSiteApiConfig config, CancellationToken ct)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
            return cached;

        _logger.LogDebug("GetAccessTokenAsync – requesting new token from {Endpoint}", config.TokenEndPoint);

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var form = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type",    "client_credentials"),
            new KeyValuePair<string, string>("client_id",     config.ClientId),
            new KeyValuePair<string, string>("client_secret", config.ApiSecret)
        ]);

        var response = await client.PostAsync(config.TokenEndPoint, form, ct);
        var json     = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to get On-Site access token: {json}");

        using var doc = JsonDocument.Parse(json);
        string token  = doc.RootElement.GetProperty("access_token").GetString()
                        ?? throw new InvalidOperationException("Null access_token in On-Site token response.");

        _cache.Set(TokenCacheKey, token, TimeSpan.FromMinutes(TokenCacheMinutes));
        _logger.LogDebug("GetAccessTokenAsync – token cached for {Minutes} min", TokenCacheMinutes);
        return token;
    }

    private async Task<int> GetCompanyInstanceIdAsync(Persona editorPersona, CancellationToken ct)
    {
        try
        {
            var map = await _blueBook.GetCompanyMapAsync(
                editorPersona.Organization.RealPageId,
                editorPersona.Organization.BooksCustomerMasterId,
                source: BlueBookProductConstants.OnSite,
                domain: editorPersona.Organization.OrganizationDomain.Name,
                cancellationToken: ct);

            string? sourceId = map?
                .FirstOrDefault(c => c.Source.Equals(BlueBookProductConstants.OnSite, StringComparison.OrdinalIgnoreCase))
                ?.CompanyInstanceSourceId;

            return int.TryParse(sourceId, out int id) ? id : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCompanyInstanceIdAsync – BlueBook lookup failed for personaId={Id}",
                editorPersona.PersonaId);
            return 0;
        }
    }

    // ── GetPropertiesAsync ─────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, userPersonaId, ct);
        if (error is not null) return error;

        try
        {
            _logger.LogDebug("GetPropertiesAsync – editorPersonaId={EditorId}", editorPersonaId);

            var props = await GetResultFromApiAsync<List<OnSiteProperty>>(
                ctx!.AccessToken,
                $"{ctx.ApiEndPoint}/properties?company_id={ctx.CompanyInstanceSourceId}", ct);

            if (props is null)
            {
                _logger.LogError("GetPropertiesAsync – no properties received editorPersonaId={EditorId}", editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = "No properties received from product." };
            }

            var activeProps = props.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUsername))
                return await MergePropertiesWithAssignedAsync(ctx, activeProps, ct);

            return new ListResponse
            {
                Records     = activeProps.Cast<object>().ToList(),
                TotalRows   = activeProps.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetPropertiesAsync – error editorPersonaId={EditorId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.PropertyErrorMessage };
        }
    }

    // ── GetRegionsAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRegionsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, userPersonaId, ct);
        if (error is not null) return error;

        try
        {
            _logger.LogDebug("GetRegionsAsync – editorPersonaId={EditorId}", editorPersonaId);

            var regions = await GetResultFromApiAsync<List<OnSiteRegion>>(
                ctx!.AccessToken,
                $"{ctx.ApiEndPoint}/regions?company_id={ctx.CompanyInstanceSourceId}", ct);

            if (regions is null)
            {
                _logger.LogError("GetRegionsAsync – no regions received editorPersonaId={EditorId}", editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage };
            }

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUsername))
                return await MergeRegionsWithAssignedAsync(ctx, regions, ct);

            return new ListResponse
            {
                Records     = regions.OrderBy(r => r.Name).Cast<object>().ToList(),
                TotalRows   = regions.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty,
                Additional  = new Dictionary<string, bool> { { "allRegions", false } }
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetRegionsAsync – error editorPersonaId={EditorId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage };
        }
    }

    // ── GetRolesAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, userPersonaId, ct);
        if (error is not null) return error;

        try
        {
            _logger.LogDebug("GetRolesAsync – editorPersonaId={EditorId}", editorPersonaId);

            var roles = await GetResultFromApiAsync<List<OnSiteRole>>(
                ctx!.AccessToken,
                $"{ctx.ApiEndPoint}/roles?company_id={ctx.CompanyInstanceSourceId}", ct);

            if (roles is null)
            {
                _logger.LogError("GetRolesAsync – no roles received editorPersonaId={EditorId}", editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = "No User Access groups (roles) received from product." };
            }

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUsername))
                return await MergeRolesWithAssignedAsync(ctx, roles, ct);

            return new ListResponse
            {
                Records     = roles.Cast<object>().ToList(),
                TotalRows   = roles.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetRolesAsync – error editorPersonaId={EditorId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.RoleErrorMessage };
        }
    }

    // ── UnassignUserAsync ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, userPersonaId, ct);
        if (error is not null) return error.ErrorReason;

        _logger.LogDebug("UnassignUserAsync – deactivating userPersonaId={UserId}", userPersonaId);

        string result = await ActivateDeactivateUserAsync(ctx!, deactivate: true, ct);

        if (string.IsNullOrEmpty(result))
        {
            _logger.LogDebug("UnassignUserAsync – marking Deleted userPersonaId={UserId}", userPersonaId);
            await _settingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Deleted, ct);
        }

        return result;
    }

    // ── ManageOnSiteUserAsync ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> auditParams)> ManageOnSiteUserAsync(
        long editorPersonaId, long userPersonaId,
        List<int> propertyList, List<int> regionList, List<int> roleList,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default)
    {
        List<AdditionalParameters> auditParams = [];

        try
        {
            var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, userPersonaId, ct);
            if (error is not null) return (error.ErrorReason, auditParams);

            // Resolve persona → person → login
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, false, ct);
            if (userPersona is null) return ($"User persona {userPersonaId} not found.", auditParams);

            var person    = await _managePerson.GetPersonAsync(userPersona.RealPageId, ct);
            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(userPersona.RealPageId, ct);

            string userEmail;

            if (await _contextService.IsSuperUserAsync(userPersona, ct))
            {
                _logger.LogDebug("ManageOnSiteUserAsync – super user userPersonaId={UserId}", userPersonaId);
                // Super users get all-company access and the fixed "super user" role level
                propertyList = [-1];
                regionList   = [];
                roleList     = [1000];
                userEmail    = userLogin.LoginName;
            }
            else if (await _contextService.IsRegularUserNoEmailAsync(userPersona, ct))
            {
                userEmail = await ResolveEmailAddressAsync(userPersona, userLogin, ct);
            }
            else
            {
                userEmail = userLogin.LoginName;
            }

            // Derive product login name: use existing On-Site username for updates, or strip domain for new users
            string productLoginName = string.IsNullOrEmpty(ctx!.ProductUsername)
                ? GetUserCodeFromLogin(userLogin.LoginName)
                : ctx.ProductUsername;

            _logger.LogDebug("ManageOnSiteUserAsync – productUsername={Name} userPersonaId={UserId}",
                ctx.ProductUsername, userPersonaId);

            // Snapshot state BEFORE the update so we can diff for the audit trail
            var userBefore      = !string.IsNullOrEmpty(ctx.ProductUsername)
                                  ? await GetOnSiteUserAsync(ctx, ctx.ProductUsername, ct)
                                  : null;
            var beforeRoles     = userBefore?.User?.Roles?.Select(r => r.Level).ToList()            ?? [];
            var beforeProps     = userBefore?.User?.Properties?.PropertyIdList                      ?? [];
            var beforeRegions   = userBefore?.User?.Properties?.RegionIdList                        ?? [];

            // Build the create/update payload
            var onSiteUser = new OnSiteInsertUpdate
            {
                FirstName   = person.FirstName,
                LastName    = person.LastName,
                Email       = userEmail,
                UserName    = productLoginName,
                PhoneNumber = string.Empty,
                IsActive    = null,
                Roles       = MapUserRoles(roleList, ctx.CompanyInstanceSourceId),
                Properties  = MapUserPropertyAccess(propertyList, regionList, ctx.CompanyInstanceSourceId)
            };

            string insUpdResult;

            if (string.IsNullOrEmpty(ctx.ProductUsername))
            {
                // ── NEW user ────────────────────────────────────────────────────────────
                // Guarantee login name uniqueness in the On-Site product
                if (!string.IsNullOrEmpty(productLoginName))
                {
                    string baseLogin  = productLoginName;
                    int    suffix     = 0;
                    while (await GetOnSiteUserAsync(ctx, productLoginName, ct) is not null)
                    {
                        productLoginName  = $"{baseLogin}{++suffix}";
                        _logger.LogDebug("ManageOnSiteUserAsync – username taken, trying {Name}", productLoginName);
                    }
                    onSiteUser.UserName = productLoginName;
                }

                _logger.LogDebug("ManageOnSiteUserAsync – CREATE userPersonaId={UserId}", userPersonaId);
                insUpdResult = await InsertOnSiteUserAsync(ctx, userPersonaId, productLoginName, onSiteUser, ct);
            }
            else
            {
                // ── EXISTING user ───────────────────────────────────────────────────────
                // Must reactivate before updating (each user can belong to multiple companies;
                // IsActive on the user object has a different meaning from per-company status)
                _logger.LogDebug("ManageOnSiteUserAsync – UPDATE userPersonaId={UserId}", userPersonaId);
                onSiteUser.UserId   = ctx.ProductUserId;
                onSiteUser.UserName = null;  // username cannot be changed on update

                string activateResult = await ActivateDeactivateUserAsync(ctx, deactivate: false, ct);
                if (!string.IsNullOrEmpty(activateResult))
                    throw new InvalidOperationException(
                        $"ManageOnSiteUserAsync: error reactivating user before update — {activateResult}");

                insUpdResult = await UpdateOnSiteUserAsync(ctx, userPersonaId, onSiteUser, ct);

                if (string.IsNullOrEmpty(insUpdResult) &&
                    batchProcessType is BatchProcessType.UserTypeRegularToAdmin
                                     or BatchProcessType.UserTypeAdminToRegular
                                     or BatchProcessType.UserTypeAdminToExternal
                                     or BatchProcessType.UserTypeExternalToAdmin)
                {
                    _logger.LogDebug("ManageOnSiteUserAsync – user-type change batchProcessType={Type}", batchProcessType);
                    // Caller receives audit detail in the returned auditParams
                }
            }

            if (!string.IsNullOrEmpty(insUpdResult))
                return (insUpdResult, auditParams);

            // ── Wait for On-Site to propagate the create/update before reading back ──
            // The On-Site REST API propagates asynchronously; 30 s is the observed delay.
            await Task.Delay(TimeSpan.FromSeconds(30), ct);

            // ── Audit: roles diff ─────────────────────────────────────────────────────
            var afterRoleLevels = roleList;
            var rolesResponse   = await GetRolesAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
            var allRoles        = rolesResponse.Records?.Cast<OnSiteRole>().ToList() ?? [];

            foreach (int r in beforeRoles.Except(afterRoleLevels))
            {
                string title = allRoles.FirstOrDefault(x => x.Level == r)?.Title ?? r.ToString();
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "On-Site Roles",
                    Value = RoleRemovedMessage.Replace("RoleName", title)
                });
            }
            foreach (int r in afterRoleLevels.Except(beforeRoles))
            {
                string title = allRoles.FirstOrDefault(x => x.Level == r)?.Title ?? r.ToString();
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "On-Site Roles",
                    Value = RoleAssignMessage.Replace("RoleName", title)
                });
            }

            // ── Audit: properties diff ────────────────────────────────────────────────
            var afterPropIds    = onSiteUser.Properties?.PropertyIdList ?? [];
            var propsResponse   = await GetPropertiesAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
            var allProps        = propsResponse.Records?.Cast<OnSiteProperty>().ToList() ?? [];

            foreach (int p in beforeProps.Except(afterPropIds))
            {
                string name = allProps.FirstOrDefault(x => x.Id == p)?.Name ?? p.ToString();
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "On-Site Properties",
                    Value = PropRemovedMessage.Replace("PropertyName", name)
                });
            }
            foreach (int p in afterPropIds.Except(beforeProps))
            {
                string name = allProps.FirstOrDefault(x => x.Id == p)?.Name ?? p.ToString();
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "On-Site Properties",
                    Value = PropAssignMessage.Replace("PropertyName", name)
                });
            }

            // ── Audit: regions diff ───────────────────────────────────────────────────
            var afterRegionIds    = onSiteUser.Properties?.RegionIdList ?? [];
            var regionsResponse   = await GetRegionsAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
            var allRegions        = regionsResponse.Records?.Cast<OnSiteRegion>().ToList() ?? [];

            foreach (int r in beforeRegions.Except(afterRegionIds))
            {
                string name = allRegions.FirstOrDefault(x => x.Id == r)?.Name ?? r.ToString();
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "On-Site Property Group",
                    Value = PropRemovedMessage.Replace("PropertyName", name)
                });
            }
            foreach (int r in afterRegionIds.Except(beforeRegions))
            {
                string name = allRegions.FirstOrDefault(x => x.Id == r)?.Name ?? r.ToString();
                auditParams.Add(new AdditionalParameters
                {
                    Key   = "On-Site Property Group",
                    Value = PropAssignMessage.Replace("PropertyName", name)
                });
            }

            return (string.Empty, auditParams);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "ManageOnSiteUserAsync – error editorPersonaId={EditorId}", editorPersonaId);
            return ($"Error - {ex.Message}", auditParams);
        }
    }

    // ── ChangeOnSiteServiceUserTypeAsync ──────────────────────────────────────

    /// <inheritdoc/>
    public Task<(string error, List<AdditionalParameters> auditParams)> ChangeOnSiteServiceUserTypeAsync(
        long editorPersonaId, long userPersonaId,
        List<int> propertyList, List<int> regionList, List<int> roleList,
        BatchProcessType batchProcessType, CancellationToken ct = default)
        => ManageOnSiteUserAsync(editorPersonaId, userPersonaId, propertyList, regionList, roleList, batchProcessType, ct);

    // ── UpdateOnSiteUserProfileAsync ───────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> UpdateOnSiteUserProfileAsync(
        long editorPersonaId, long userPersonaId, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, userPersonaId, ct);
        if (error is not null) return error.ErrorReason;

        try
        {
            var userPersona = await _managePersona.GetPersonaAsync(userPersonaId, false, ct);
            if (userPersona is null) return $"User persona {userPersonaId} not found.";

            var person    = await _managePerson.GetPersonAsync(userPersona.RealPageId, ct);
            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(userPersona.RealPageId, ct);

            string userEmail = await _contextService.IsRegularUserNoEmailAsync(userPersona, ct)
                ? await ResolveEmailAddressAsync(userPersona, userLogin, ct)
                : userLogin.LoginName;

            // Must reactivate before updating profile
            string activateResult = await ActivateDeactivateUserAsync(ctx!, deactivate: false, ct);
            if (!string.IsNullOrEmpty(activateResult))
                throw new InvalidOperationException(
                    $"UpdateOnSiteUserProfileAsync: error reactivating user before profile update — {activateResult}");

            var profileUpdate = new OnSiteProfileUpdate
            {
                UserId      = ctx!.ProductUserId,
                FirstName   = person.FirstName,
                LastName    = person.LastName,
                Email       = userEmail,
                UserName    = null,
                PhoneNumber = string.Empty,
                IsActive    = null
            };

            _logger.LogDebug("UpdateOnSiteUserProfileAsync – UPDATE profile userPersonaId={UserId}", userPersonaId);
            return await UpdateOnSiteUserProfileApiAsync(ctx, userPersonaId, profileUpdate, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "UpdateOnSiteUserProfileAsync – error editorPersonaId={EditorId}", editorPersonaId);
            return $"Error - {ex.Message}";
        }
    }

    // ── GetUsersAsync ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUsersAsync(
        long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, 0, ct);
        if (error is not null) return error;

        try
        {
            string url   = $"{ctx!.ApiEndPoint}/users?company_id={ctx.CompanyInstanceSourceId}";
            _logger.LogDebug("GetUsersAsync – url={Url}", url);

            var users = await GetResultFromApiAsync<List<OnSiteApiUserWrapper>>(ctx.AccessToken, url, ct);
            if (users is null)
            {
                _logger.LogError("GetUsersAsync – no users received editorPersonaId={EditorId}", editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = "No Users." };
            }

            return new ListResponse
            {
                Records     = users.Cast<object>().ToList(),
                TotalRows   = users.Count,
                RowsPerPage = 9999,
                TotalPages  = 1,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetUsersAsync – error editorPersonaId={EditorId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── ChangeUserStatusAsync ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string productUserId,
        bool isDeactivate = true, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, 0, ct);
        if (error is not null)
        {
            _logger.LogError("ChangeUserStatusAsync – context error: {Reason}", error.ErrorReason);
            return false;
        }

        // The product user ID is supplied explicitly (not from SAML attributes in the context)
        var ctxWithUser = ctx! with { ProductUserId = productUserId };
        string result   = await ActivateDeactivateUserAsync(ctxWithUser, isDeactivate, ct);

        if (!string.IsNullOrEmpty(result))
        {
            _logger.LogError("ChangeUserStatusAsync – failed productUserId={UserId}: {Result}", productUserId, result);
            return false;
        }

        _logger.LogDebug("ChangeUserStatusAsync – success productUserId={UserId}", productUserId);
        return true;
    }

    // ── GetMigrationUsersAsync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter, CancellationToken ct = default)
    {
        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, 0, ct);
        if (error is not null) return new ListResponse { IsError = true, ErrorReason = error.ErrorReason };

        try
        {
            string filter       = "UnMigrated";
            int    startRow     = 0;
            int    resultPerRow = 1000;

            if (datafilter is not null)
            {
                if (datafilter.FilterBy.TryGetValue("filter", out string? f)) filter = f;
                if (datafilter.Pages is not null)
                {
                    startRow     = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage;
                }
            }

            string url   = $"{ctx!.ApiEndPoint}/users?company_id={ctx.CompanyInstanceSourceId}"
                         + $"&filter={filter}&page={startRow}&per_page={resultPerRow}";

            _logger.LogDebug("GetMigrationUsersAsync – url={Url}", url);

            var users = await GetResultFromApiAsync<List<OnSiteApiUserWrapper>>(ctx.AccessToken, url, ct);
            if (users is null)
            {
                _logger.LogError("GetMigrationUsersAsync – no users received editorPersonaId={EditorId}", editorPersonaId);
                return new ListResponse { IsError = true, ErrorReason = "No Users." };
            }

            var migrationUsers = users.Select(u => new MigrationUser
            {
                CompanyInstanceSourceId = ctx.CompanyInstanceSourceId.ToString(),
                UserId    = u.User?.UserId,
                FirstName = u.User?.FirstName,
                LastName  = u.User?.LastName,
                Email     = u.User?.Email,
                Username  = u.User?.UserName,
                Status    = u.User?.IsActive == true ? "Active" : "Disabled",
                Phone     = u.User?.PhoneNumber,
                Properties = u.User?.Properties?.PropertyIdList?
                    .Select(p => new MigrationProperty { PropertyInstanceSourceId = p.ToString() })
                    .ToList()
            }).ToList();

            return new ListResponse
            {
                IsError     = false,
                RowsPerPage = resultPerRow,
                TotalPages  = 1,
                Records     = migrationUsers.Cast<object>().ToList(),
                TotalRows   = migrationUsers.Count,
                ErrorReason = string.Empty
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetMigrationUsersAsync – error editorPersonaId={EditorId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── UpdateUsersMigrationStatusAsync ───────────────────────────────────────

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken ct = default)
    {
        var result = new MigrateResponse { Status = true };

        var (ctx, error) = await GetOnSiteContextAsync(editorPersonaId, 0, ct);
        if (error is not null) { result.Message = error.ErrorReason; return result; }

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ctx!.AccessToken);

            var toMigrate   = new OnSiteMigrateUsersRequest
            {
                Users = migrateUsers.Where(x => x.UsingUnifiedLogin)
                    .Select(x => new OnSiteMigrateUserItem { UserId = x.UserId }).ToList()
            };
            var toUnmigrate = new OnSiteMigrateUsersRequest
            {
                Users = migrateUsers.Where(x => !x.UsingUnifiedLogin)
                    .Select(x => new OnSiteMigrateUserItem { UserId = x.UserId }).ToList()
            };

            if (toMigrate.Users.Count > 0)
            {
                string migrateUrl = $"{ctx.ApiEndPoint}/users/migrate_users";
                using var content = JsonContent.Create(toMigrate, options: _jsonOptions);
                var response      = await client.PostAsync(migrateUrl, content, ct);
                string json       = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(json);
                    int count     = doc.RootElement.GetProperty("count").GetInt32();
                    result.Message = count.ToString();
                    result.Status  = count != 0;
                    _logger.LogDebug("UpdateUsersMigrationStatusAsync – migrate count={Count}", count);
                }
                else
                {
                    _logger.LogError("UpdateUsersMigrationStatusAsync – migrate failed: {Body}", json);
                    result.Message = "Cannot update user status to migrated.";
                    result.Status  = false;
                }
            }

            if (toUnmigrate.Users.Count > 0)
            {
                string unmigrateUrl = $"{ctx.ApiEndPoint}/users/unmigrate_users";
                using var content   = JsonContent.Create(toUnmigrate, options: _jsonOptions);
                var response        = await client.PostAsync(unmigrateUrl, content, ct);
                string json         = await response.Content.ReadAsStringAsync(ct);

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(json);
                    int count     = doc.RootElement.GetProperty("count").GetInt32();
                    result.Message = string.IsNullOrEmpty(result.Message)
                        ? count.ToString()
                        : $"{result.Message} {count}";
                    result.Status  = result.Status && count != 0;
                    _logger.LogDebug("UpdateUsersMigrationStatusAsync – unmigrate count={Count}", count);
                }
                else
                {
                    _logger.LogError("UpdateUsersMigrationStatusAsync – unmigrate failed: {Body}", json);
                    result.Message = "Cannot update user status to unmigrated.";
                    result.Status  = false;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "UpdateUsersMigrationStatusAsync – error editorPersonaId={EditorId}", editorPersonaId);
            result = new MigrateResponse { Status = false, Message = ex.Message };
        }

        return result;
    }

    // ── Private HTTP helpers ───────────────────────────────────────────────────

    private async Task<T?> GetResultFromApiAsync<T>(string token, string url, CancellationToken ct)
        where T : class
    {
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return null;

        string json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    private async Task<OnSiteApiUserWrapper?> GetOnSiteUserAsync(
        OnSiteCtx ctx, string userName, CancellationToken ct)
    {
        string url = $"{ctx.ApiEndPoint}/users/exists?username={Uri.EscapeDataString(userName)}";
        return await GetResultFromApiAsync<OnSiteApiUserWrapper>(ctx.AccessToken, url, ct);
    }

    private async Task<string> ActivateDeactivateUserAsync(
        OnSiteCtx ctx, bool deactivate, CancellationToken ct)
    {
        string action = deactivate ? "deactivate" : "reactivate";
        string url    = $"{ctx.ApiEndPoint}/users/{ctx.ProductUserId}/{action}"
                      + $"?company_id={ctx.CompanyInstanceSourceId}";

        _logger.LogDebug("ActivateDeactivateUserAsync – productUserId={UserId} action={Action}",
            ctx.ProductUserId, action);

        try
        {
            using var client  = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

            // Body must be present (even empty) for POST to work with some proxy layers
            using var content  = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
            var response       = await client.PostAsync(url, content, ct);

            if (response.IsSuccessStatusCode) return string.Empty;

            string errorBody = await TryReadContentAsync(response);
            _logger.LogError("ActivateDeactivateUserAsync – failed productUserId={UserId}: {Error}",
                ctx.ProductUserId, errorBody);
            return $"There was a problem updating the user with productUserId - {ctx.ProductUserId}. Error-{errorBody}.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "ActivateDeactivateUserAsync – exception productUserId={UserId}", ctx.ProductUserId);
            return ex.Message;
        }
    }

    private async Task<string> InsertOnSiteUserAsync(
        OnSiteCtx ctx, long userPersonaId, string productLoginName,
        OnSiteInsertUpdate user, CancellationToken ct)
    {
        string url = $"{ctx.ApiEndPoint}/users";
        _logger.LogDebug("InsertOnSiteUserAsync – creating user loginName={Login} editorPersonaId={EditorId}",
            productLoginName, ctx.EditorPersona.PersonaId);

        using var client  = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

        using var content  = JsonContent.Create(user, options: _jsonOptions);
        var response       = await client.PostAsync(url, content, ct);
        string json        = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(json);
            string userId = doc.RootElement
                .GetProperty("user").GetProperty("user_id").GetString() ?? string.Empty;

            await CreateProductUserInGreenbookAsync(userPersonaId, userId, productLoginName,
                ctx.CompanyInstanceSourceId, ct);

            _logger.LogDebug("InsertOnSiteUserAsync – success userId={Id} loginName={Login}", userId, productLoginName);
            return string.Empty;
        }

        _logger.LogError("InsertOnSiteUserAsync – failed: {Error}", json);
        await _settingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Error, ct);
        return $"There was a problem creating the user. Error-{json}";
    }

    private async Task<string> UpdateOnSiteUserAsync(
        OnSiteCtx ctx, long userPersonaId, OnSiteInsertUpdate user, CancellationToken ct)
    {
        string url = $"{ctx.ApiEndPoint}/users/{user.UserId}/update";
        _logger.LogDebug("UpdateOnSiteUserAsync – userPersonaId={UserId}", userPersonaId);

        using var client  = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

        using var content  = JsonContent.Create(user, options: _jsonOptions);
        var response       = await client.PostAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            await _settingService.UpdateProductStatusAsync(
                userPersonaId, ProductStatusSettingType, ProductId,
                (int)ProductBatchStatusType.Success, ct);
            return string.Empty;
        }

        string errorBody = await TryReadContentAsync(response);
        _logger.LogError("UpdateOnSiteUserAsync – failed for userPersonaId={UserId}: {Error}", userPersonaId, errorBody);
        return $"There was a problem updating the user. Error-{errorBody}";
    }

    private async Task<string> UpdateOnSiteUserProfileApiAsync(
        OnSiteCtx ctx, long userPersonaId, OnSiteProfileUpdate user, CancellationToken ct)
    {
        string url = $"{ctx.ApiEndPoint}/users/{user.UserId}/update";
        _logger.LogDebug("UpdateOnSiteUserProfileApiAsync – userPersonaId={UserId}", userPersonaId);

        using var client  = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

        using var content  = JsonContent.Create(user, options: _jsonOptions);
        var response       = await client.PostAsync(url, content, ct);

        if (response.IsSuccessStatusCode) return string.Empty;

        string errorBody = await TryReadContentAsync(response);
        _logger.LogError("UpdateOnSiteUserProfileApiAsync – failed for userPersonaId={UserId}: {Error}",
            userPersonaId, errorBody);
        return $"There was a problem updating the user profile. Error-{errorBody}";
    }

    private static async Task<string> TryReadContentAsync(HttpResponseMessage response)
    {
        try   { return await response.Content.ReadAsStringAsync(); }
        catch { return string.Empty; }
    }

    // ── Private merge helpers ──────────────────────────────────────────────────

    private async Task<ListResponse> MergePropertiesWithAssignedAsync(
        OnSiteCtx ctx, List<OnSiteProperty> allProperties, CancellationToken ct)
    {
        var userInfo = await GetOnSiteUserAsync(ctx, ctx.ProductUsername, ct);
        if (userInfo?.User is null)
        {
            _logger.LogError("MergePropertiesWithAssignedAsync – user not found username={Name}", ctx.ProductUsername);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        var userProps   = userInfo.User.Properties?.PropertyIdList ?? [];
        var userRegions = userInfo.User.Properties?.RegionIdList   ?? [];
        bool allFlag    = userProps.Count == 0 && userRegions.Count == 0;

        if (!allFlag)
        {
            // Mark directly assigned properties
            foreach (var p in allProperties.Where(p => userProps.Contains(p.Id)))
                p.IsAssigned = true;

            // Mark properties that belong to assigned regions
            var regionSet = userRegions.ToHashSet();
            foreach (var p in allProperties.Where(p =>
                !string.IsNullOrEmpty(p.RegionId) &&
                int.TryParse(p.RegionId, out int rid) &&
                regionSet.Contains(rid)))
                p.IsAssigned = true;
        }

        return new ListResponse
        {
            Records     = allProperties.Cast<object>().ToList(),
            TotalRows   = allProperties.Count,
            RowsPerPage = 9999,
            TotalPages  = 1,
            ErrorReason = string.Empty,
            Additional  = new Dictionary<string, bool> { { "allProperties", allFlag } }
        };
    }

    private async Task<ListResponse> MergeRegionsWithAssignedAsync(
        OnSiteCtx ctx, List<OnSiteRegion> allRegions, CancellationToken ct)
    {
        var userInfo = await GetOnSiteUserAsync(ctx, ctx.ProductUsername, ct);
        if (userInfo?.User is null)
        {
            _logger.LogError("MergeRegionsWithAssignedAsync – user not found username={Name}", ctx.ProductUsername);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        var userRegionIds = userInfo.User.Properties?.RegionIdList?.ToHashSet() ?? [];
        bool allFlag      = userRegionIds.Count == 0;

        if (!allFlag)
        {
            foreach (var region in allRegions.Where(r => userRegionIds.Contains(r.Id)))
                region.IsAssigned = true;
        }

        return new ListResponse
        {
            Records     = allRegions.OrderBy(r => r.Name).Cast<object>().ToList(),
            TotalRows   = allRegions.Count,
            RowsPerPage = 9999,
            TotalPages  = 1,
            ErrorReason = string.Empty,
            Additional  = new Dictionary<string, bool> { { "allRegions", allFlag } }
        };
    }

    private async Task<ListResponse> MergeRolesWithAssignedAsync(
        OnSiteCtx ctx, List<OnSiteRole> allRoles, CancellationToken ct)
    {
        var userInfo = await GetOnSiteUserAsync(ctx, ctx.ProductUsername, ct);
        if (userInfo?.User is null)
        {
            _logger.LogError("MergeRolesWithAssignedAsync – user not found username={Name}", ctx.ProductUsername);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        var userRoleLevels = userInfo.User.Roles?.Select(r => r.Level).ToHashSet() ?? [];
        foreach (var role in allRoles.Where(r => userRoleLevels.Contains(r.Level)))
            role.IsAssigned = true;

        return new ListResponse
        {
            Records     = allRoles.Cast<object>().ToList(),
            TotalRows   = allRoles.Count,
            RowsPerPage = 9999,
            TotalPages  = 1,
            ErrorReason = string.Empty
        };
    }

    // ── Private domain helpers ─────────────────────────────────────────────────

    private async Task CreateProductUserInGreenbookAsync(
        long userPersonaId, string userId, string loginName, int companyId, CancellationToken ct)
    {
        _logger.LogDebug("CreateProductUserInGreenbookAsync – persisting SAML attrs userPersonaId={UserId}", userPersonaId);
        await _samlRepository.CreateSamlUserAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.productUsername, loginName,  ct);
        await _samlRepository.CreateSamlUserAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.UserId,          userId,     ct);
        await _samlRepository.CreateSamlUserAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.PMCID,           companyId.ToString(), ct);

        await _settingService.UpdateProductStatusAsync(
            userPersonaId, ProductStatusSettingType, ProductId,
            (int)ProductBatchStatusType.Success, ct);
    }

    private async Task<string> ResolveEmailAddressAsync(
        Persona userPersona, UserLoginOnly userLogin, CancellationToken ct)
    {
        var addresses = await _manageElectronicAddress.ListElectronicAddressForPersonAsync(
            userLogin.RealPageId, null, ct);

        return addresses?
                   .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                   ?.AddressString
               ?? userLogin.LoginName;
    }

    private static string GetUserCodeFromLogin(string loginName)
        => loginName.Contains('@') ? loginName.Split('@')[0] : loginName;

    private static List<OnSiteRole> MapUserRoles(List<int> roleList, int companyId)
        => roleList.Select(r => new OnSiteRole { Level = r, CompanyId = companyId }).ToList();

    private static OnSiteApiPropertyAccess MapUserPropertyAccess(
        List<int> propertyList, List<int> regionList, int companyId)
    {
        // Super-user sentinel: PropertyList = [-1] means "all properties in company"
        if (propertyList is [{ } first] && first == -1)
        {
            return new OnSiteApiPropertyAccess
            {
                CompanyIdList  = [companyId],
                PropertyIdList = [],
                RegionIdList   = []
            };
        }

        return new OnSiteApiPropertyAccess
        {
            PropertyIdList = propertyList,
            RegionIdList   = regionList,
            CompanyIdList  = null
        };
    }
}

// ── On-Site REST API models ────────────────────────────────────────────────────
// System.Text.Json with SnakeCaseLower naming policy handles most field-name
// conversions automatically. [JsonPropertyName] overrides are only added where
// the JSON key diverges from the snake_case equivalent of the C# property name.

/// <summary>
/// Outer envelope returned by <c>/users/exists?username=…</c>.
/// The On-Site API wraps the user profile under a "user" key.
/// </summary>
internal sealed class OnSiteApiUserWrapper
{
    [JsonPropertyName("user")]
    public OnSiteApiUserProfile? User { get; set; }
}


/// <summary>Batch request body sent to <c>/users/migrate_users</c> or <c>/users/unmigrate_users</c>.</summary>
internal sealed class OnSiteMigrateUsersRequest
{
    public List<OnSiteMigrateUserItem> Users { get; set; } = [];
}

/// <summary>Single user entry within a migration batch request.</summary>
internal sealed class OnSiteMigrateUserItem
{
    public string? UserId { get; set; }
}
