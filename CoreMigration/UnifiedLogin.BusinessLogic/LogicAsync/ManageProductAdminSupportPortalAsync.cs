using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True async implementation of Admin Support Portal (Salesforce / Client Portal) user-management operations.
/// Replaces the stepping-stone <c>Task.FromResult(new ManageProductAdminSupportPortal(userClaim).Method(...))</c>
/// pattern with fully async/await calls backed by injected services.
/// </summary>
public sealed class ManageProductAdminSupportPortalAsync : IManageProductAdminSupportPortalAsync
{
    private const int ProductId = (int)ProductEnum.AdminSupportPortal;

    // Token cache keys match the legacy MemoryCache.Default keys so both code paths
    // share the same cached values during any rolling migration window.
    private const string CacheKeyAuthToken       = "access_token_CP";
    private const string CacheKeyInstanceUrl     = "instance_url_CP";
    private const int    SfTokenCacheMinutes     = 9;

    private readonly IProductContextServiceAsync _contextService;
    private readonly IProductSettingServiceAsync _settingService;
    private readonly IManageBlueBookAsync _blueBook;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ManageProductAdminSupportPortalAsync> _logger;

    public ManageProductAdminSupportPortalAsync(
        IProductContextServiceAsync contextService,
        IProductSettingServiceAsync settingService,
        IManageBlueBookAsync blueBook,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<ManageProductAdminSupportPortalAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);    _contextService    = contextService;
        ArgumentNullException.ThrowIfNull(settingService);    _settingService    = settingService;
        ArgumentNullException.ThrowIfNull(blueBook);          _blueBook          = blueBook;
        ArgumentNullException.ThrowIfNull(httpClientFactory); _httpClientFactory = httpClientFactory;
        ArgumentNullException.ThrowIfNull(cache);             _cache             = cache;
        ArgumentNullException.ThrowIfNull(logger);            _logger            = logger;
    }

    // ── GetRolesAsync ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        _logger.LogDebug("GetRolesAsync - beginning for editorPersonaId {EditorPersonaId}", editorPersonaId);

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null)
        {
            _logger.LogError("GetRolesAsync - context error for editorPersonaId {EditorPersonaId}: {Error}",
                editorPersonaId, ctxError.ErrorReason);
            return ctxError;
        }

        var settings        = await _settingService.GetProductSettingAsync(ProductId, ct);
        string apiRoute     = SettingValue(settings, "APIROUTE");
        string ultraLightId = SettingValue(settings, "CLIENTPORTALULTRALIGHTROLEID");

        var response = new ListResponse();
        try
        {
            var allRoles = BuildProductRoles(ultraLightId);

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId))
            {
                // Existing user — look up assigned Salesforce profile and mark it selected
                _logger.LogDebug("GetRolesAsync - merging roles for productUserId {ProductUserId}", ctx.ProductUserId);
                var session  = await GetSalesforceSessionAsync(settings, ct);
                response = await MergeProductRolesWithSalesforceAsync(session, allRoles, ctx.ProductUserId, apiRoute, ct);
            }
            else
            {
                // New user — pre-select defaults matching legacy behaviour
                var emptyIdRole = allRoles.FirstOrDefault(r => r.ID == string.Empty);
                if (emptyIdRole is not null)
                    emptyIdRole.IsAssigned = true;

                var lightRole = allRoles.FirstOrDefault(r =>
                    string.Equals(r.Name?.Trim(), "client portal light", StringComparison.OrdinalIgnoreCase));
                if (lightRole is not null)
                    lightRole.IsAssigned = true;

                response = new ListResponse
                {
                    Records     = allRoles.Cast<object>().ToList(),
                    TotalRows   = allRoles.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages  = 1
                };
            }

            _logger.LogDebug("GetRolesAsync - returning {Count} roles for editorPersonaId {EditorPersonaId}",
                allRoles.Count, editorPersonaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            response.IsError     = true;
            response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
        }

        return response;
    }

    // ── GetPropertiesAsync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        _logger.LogDebug("GetPropertiesAsync - beginning for editorPersonaId {EditorPersonaId}", editorPersonaId);

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null)
        {
            _logger.LogError("GetPropertiesAsync - context error for editorPersonaId {EditorPersonaId}: {Error}",
                editorPersonaId, ctxError.ErrorReason);
            return ctxError;
        }

        var settings    = await _settingService.GetProductSettingAsync(ProductId, ct);
        string apiRoute = SettingValue(settings, "APIROUTE");

        var result = new ListResponse();
        try
        {
            string udmSourceCode = await ResolveUdmSourceCodeAsync(ct);

            var companyMaps = await _blueBook.GetProductCompanyMappingAsync(
                ctx!.EditorPersona.Organization.RealPageId, udmSourceCode, ct);
            int companyInstanceId = companyMaps.FirstOrDefault()?.CompanyInstanceId ?? 0;

            _logger.LogDebug("GetPropertiesAsync - companyInstanceId {CompanyInstanceId} for editorPersonaId {EditorPersonaId}",
                companyInstanceId, editorPersonaId);

            CompanyPropertyRootObject companyProperties =
                await _blueBook.GetCompanyPropertyInstanceAsync(companyInstanceId, ct);

            IList<ProductProperty> blueBookPropertyList =
                companyProperties?.MapBlueBookToGBProperties() ?? new List<ProductProperty>();

            _logger.LogDebug("GetPropertiesAsync - {Count} properties from BlueBook for editorPersonaId {EditorPersonaId}",
                blueBookPropertyList.Count, editorPersonaId);

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
            {
                // Existing user — overlay the property already assigned in Salesforce
                var session = await GetSalesforceSessionAsync(settings, ct);
                result = await MergeProductPropertiesWithSalesforceAsync(
                    session, blueBookPropertyList, ctx.ProductUserId, apiRoute, ct);
            }
            else
            {
                // New user — return the full BlueBook list unfiltered
                result = new ListResponse
                {
                    Records     = blueBookPropertyList.Cast<object>().ToList(),
                    TotalRows   = blueBookPropertyList.Count,
                    RowsPerPage = blueBookPropertyList.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPropertiesAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            result = new ListResponse
            {
                IsError     = true,
                ErrorReason = ex is BlueBookException
                    ? ex.Message
                    : CommonMessageConstants.PropertyErrorMessage
            };
        }

        return result;
    }

    // ── GetMigrationUsersAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null)
        {
            response.ErrorReason = ctxError.ErrorReason;
            return response;
        }

        var settings    = await _settingService.GetProductSettingAsync(ProductId, ct);
        string apiRoute = SettingValue(settings, "APIROUTE");
        string portalId = SettingValue(settings, "PORTALID");
        string orgId    = SettingValue(settings, "ORGANIZATIONID");
        string ultraLightId = SettingValue(settings, "CLIENTPORTALULTRALIGHTROLEID");

        string udmSourceCode = await ResolveUdmSourceCodeAsync(ct);

        var companyMaps = await _blueBook.GetProductCompanyMappingAsync(
            ctx!.EditorPersona.Organization.RealPageId, udmSourceCode, ct);
        string companyInstanceSourceId = companyMaps.FirstOrDefault()?.CompanyInstanceSourceId ?? string.Empty;

        if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
        {
            _logger.LogError(
                "GetMigrationUsersAsync - company not found in BlueBook for editorPersonaId {EditorPersonaId}",
                editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
        }

        bool filter       = false;
        int  startRow     = 0;
        int  resultPerRow = 1000;

        if (datafilter is not null)
        {
            if (datafilter.FilterBy.ContainsKey("filter"))
                filter = datafilter.FilterBy["filter"].Equals("migrated", StringComparison.OrdinalIgnoreCase);

            if (datafilter.Pages is not null)
            {
                startRow     = datafilter.Pages.StartRow;
                resultPerRow = datafilter.Pages.ResultsPerPage;
            }
        }

        string query = ($"SELECT Id,FirstName,LastName,Email,Username,LastLoginDate,IsActive,ProfileId FROM User" +
                        $" WHERE (User.Contact.Account.OMS_ID__c = '{companyInstanceSourceId}'" +
                        $" OR User.Contact.Account.Parent.OMS_ID__c = '{companyInstanceSourceId}')" +
                        $" AND User.Contact.Portal_User_Migrated__c = {filter}" +
                        $" LIMIT {resultPerRow} OFFSET {startRow}").Replace(' ', '+');

        _logger.LogDebug("GetMigrationUsersAsync - querying Salesforce for editorPersonaId {EditorPersonaId}",
            editorPersonaId);

        var session = await GetSalesforceSessionAsync(settings, ct);
        var migrationResponse = await SfGetAsync<ClientPortalMigrationResponse>(
            session, $"{apiRoute}query?q={query}", ct);

        if (migrationResponse is null)
        {
            _logger.LogError(
                "GetMigrationUsersAsync - null response from Salesforce for editorPersonaId {EditorPersonaId}",
                editorPersonaId);
            return response;
        }

        var productRoles   = BuildProductRoles(ultraLightId);
        var migrationUsers = migrationResponse.Records.Select(user =>
        {
            // Salesforce IDs can be 15 or 18 characters; normalise to 15 for profile lookup
            string profileId = user.ProfileId.Length > 15 ? user.ProfileId[..15] : user.ProfileId;
            string roleType  = productRoles.FirstOrDefault(r => r.ID == profileId)?.Roletype;
            return new MigrationUser
            {
                CompanyInstanceSourceId = companyInstanceSourceId,
                FirstName               = user.FirstName,
                LastName                = user.LastName,
                UserId                  = user.UserId,
                Username                = user.Username,
                Email                   = user.Email,
                LastActivity            = user.LastLoginDate.ToString(),
                Extra                   = $"{portalId}|{orgId}|{roleType}",
                Status                  = user.IsActive ? "Active" : "Disabled"
            };
        }).ToList();

        _logger.LogDebug("GetMigrationUsersAsync - {Count} users returned for editorPersonaId {EditorPersonaId}",
            migrationUsers.Count, editorPersonaId);

        response.RowsPerPage = resultPerRow;
        response.ErrorReason = string.Empty;
        response.IsError     = false;
        response.TotalPages  = 1;
        response.Records     = migrationUsers.Cast<object>().ToList();
        response.TotalRows   = migrationResponse.TotalSize;

        return response;
    }

    // ── UpdateUsersMigrationStatusAsync ────────────────────────────────────

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null)
        {
            migrateResponse.Message = ctxError.ErrorReason;
            return migrateResponse;
        }

        var settings    = await _settingService.GetProductSettingAsync(ProductId, ct);
        string apiRoute = SettingValue(settings, "APIROUTE");

        string udmSourceCode = await ResolveUdmSourceCodeAsync(ct);

        var companyMaps = await _blueBook.GetProductCompanyMappingAsync(
            ctx!.EditorPersona.Organization.RealPageId, udmSourceCode, ct);
        string companyInstanceSourceId = companyMaps.FirstOrDefault()?.CompanyInstanceSourceId ?? string.Empty;

        if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
        {
            _logger.LogError(
                "UpdateUsersMigrationStatusAsync - company not found in BlueBook for editorPersonaId {EditorPersonaId}",
                editorPersonaId);
            migrateResponse.Message = "Company Setup Error: Please Contact Support.";
            return migrateResponse;
        }

        var session  = await GetSalesforceSessionAsync(settings, ct);
        bool isError = false;

        foreach (var migrateUser in migrateUsers)
        {
            // Update Contact flags on the linked Contact object
            var contactPatch = new
            {
                Unified_Platform_User__c = migrateUser.UsingUnifiedLogin,
                Portal_User_Migrated__c  = true
            };
            string contactResult = await SfPostAsync(
                session,
                $"{apiRoute}sobjects/User/{migrateUser.UserId}/Contact?_HttpMethod=PATCH",
                contactPatch, ct);
            if (!string.IsNullOrEmpty(contactResult))
            {
                migrateResponse.Message += contactResult;
                isError = true;
            }

            // Update User record flags
            var aspUser = await SfGetAsync<AdminSupportPortalUser>(
                session, $"{apiRoute}sobjects/user/{migrateUser.UserId}", ct);
            if (aspUser is not null)
            {
                aspUser.IsCreatedFromNewPortal__c = true;
                aspUser.ContactId                 = null; // NullValueHandling.Ignore omits field from PATCH body
                string userResult = await SfPostAsync(
                    session,
                    $"{apiRoute}sobjects/User/{migrateUser.UserId}?_HttpMethod=PATCH",
                    aspUser, ct);
                if (!string.IsNullOrEmpty(userResult))
                {
                    migrateResponse.Message += userResult;
                    isError = true;
                }
            }

            _logger.LogDebug(
                "UpdateUsersMigrationStatusAsync - updated userId {UserId} for editorPersonaId {EditorPersonaId}",
                migrateUser.UserId, editorPersonaId);
        }

        migrateResponse.Status  = !isError;
        migrateResponse.Message = isError ? migrateResponse.Message : "success";

        return migrateResponse;
    }

    // ── ChangeUserStatusAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// Always disables the user (IsActive = false). The legacy interface did not expose
    /// the <c>isActive</c> parameter — the same default behaviour is preserved here.
    /// </remarks>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string productUserId,
        CancellationToken ct = default)
    {
        var (_, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null)
        {
            _logger.LogError(
                "ChangeUserStatusAsync - context error for editorPersonaId {EditorPersonaId}: {Error}",
                editorPersonaId, ctxError.ErrorReason);
            return false;
        }

        var settings    = await _settingService.GetProductSettingAsync(ProductId, ct);
        string apiRoute = SettingValue(settings, "APIROUTE");
        var session     = await GetSalesforceSessionAsync(settings, ct);

        var aspUser = await SfGetAsync<AdminSupportPortalUser>(
            session, $"{apiRoute}sobjects/user/{productUserId}", ct);
        if (aspUser is null)
        {
            _logger.LogError(
                "ChangeUserStatusAsync - user not found in Salesforce: productUserId {ProductUserId}", productUserId);
            return false;
        }

        aspUser.IsActive  = false;
        aspUser.ContactId = null;

        string result = await SfPostAsync(
            session, $"{apiRoute}sobjects/User/{productUserId}?_HttpMethod=PATCH", aspUser, ct);

        if (string.IsNullOrEmpty(result))
        {
            _logger.LogDebug("ChangeUserStatusAsync - disabled productUserId {ProductUserId}", productUserId);
            return true;
        }

        _logger.LogError(
            "ChangeUserStatusAsync - error disabling productUserId {ProductUserId}: {Error}", productUserId, result);
        return false;
    }

    // ── Salesforce session (auth + token cache) ────────────────────────────

    /// <summary>
    /// Immutable value holding the Salesforce instance URL and bearer token resolved for
    /// a single logical operation. Obtained from <see cref="GetSalesforceSessionAsync"/>.
    /// </summary>
    private readonly record struct SalesforceSession(string InstanceUrl, string AuthToken);

    /// <summary>
    /// Returns a valid Salesforce bearer token and instance URL, using a 9-minute
    /// <see cref="IMemoryCache"/> entry to avoid re-authenticating on every call.
    /// <para>
    /// Preserves the cache key names (<c>access_token_CP</c>, <c>instance_url_CP</c>) from
    /// the legacy <c>MemoryCache.Default</c> usage so both code paths share the cached values
    /// during any rolling migration period.
    /// </para>
    /// <para>
    /// If essential settings are absent (test/stub environment), returns an empty session
    /// rather than throwing — matching the legacy guard in <c>GetSaleforceTokenInstanceUrl</c>.
    /// </para>
    /// </summary>
    private async Task<SalesforceSession> GetSalesforceSessionAsync(
        IList<ProductInternalSetting> settings, CancellationToken ct)
    {
        // Fast path — return cached values if still valid
        if (_cache.TryGetValue(CacheKeyAuthToken,   out string? cachedToken) &&
            _cache.TryGetValue(CacheKeyInstanceUrl, out string? cachedUrl)   &&
            !string.IsNullOrEmpty(cachedToken) && !string.IsNullOrEmpty(cachedUrl))
        {
            return new SalesforceSession(cachedUrl!, cachedToken!);
        }

        string tokenUrl      = SettingValue(settings, "TOKENURL");
        string apiCode       = SettingValue(settings, "APICODE");
        string apiSecret     = SettingValue(settings, "APISECRET");
        string apiUserName   = SettingValue(settings, "APIUSERNAME");
        string apiPassword   = SettingValue(settings, "APIPASSWORD");
        string securityToken = SettingValue(settings, "SECURITYTOKEN");

        // Guard: missing credentials → skip auth (matches legacy test-environment behaviour)
        if (string.IsNullOrEmpty(tokenUrl) || string.IsNullOrEmpty(apiCode))
        {
            _logger.LogDebug("GetSalesforceSessionAsync - missing API credentials, skipping authentication");
            return new SalesforceSession(string.Empty, string.Empty);
        }

        _logger.LogDebug("GetSalesforceSessionAsync - fetching new token from {TokenUrl}", tokenUrl);

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        request.Headers.Add("X-PrettyPrint", "1");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "password",
            ["client_id"]     = apiCode,
            ["client_secret"] = apiSecret,
            ["username"]      = apiUserName,
            ["password"]      = apiPassword + securityToken,
        });

        using var authClient = _httpClientFactory.CreateClient();
        authClient.Timeout = TimeSpan.FromSeconds(30);
        using var response = await authClient.SendAsync(request, ct);

        string json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Salesforce authentication failed ({(int)response.StatusCode}): {json}");

        var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        if (values is null
            || !values.TryGetValue("access_token", out string? authToken)
            || !values.TryGetValue("instance_url", out string? instanceUrl)
            || string.IsNullOrEmpty(authToken)
            || string.IsNullOrEmpty(instanceUrl))
        {
            throw new InvalidOperationException(
                $"Salesforce token response missing access_token or instance_url. Response: {json}");
        }

        var expiry = TimeSpan.FromMinutes(SfTokenCacheMinutes);
        _cache.Set(CacheKeyAuthToken,   authToken,   expiry);
        _cache.Set(CacheKeyInstanceUrl, instanceUrl, expiry);

        _logger.LogDebug("GetSalesforceSessionAsync - token cached for {Minutes} min", SfTokenCacheMinutes);

        return new SalesforceSession(instanceUrl, authToken);
    }

    // ── Salesforce HTTP helpers ────────────────────────────────────────────

    /// <summary>
    /// Issues an authenticated GET to the Salesforce REST API and deserialises the response
    /// to <typeparamref name="T"/> using Newtonsoft.Json (required because
    /// <c>AdminSupportPortalUser</c> carries <c>[JsonProperty]</c> attributes).
    /// Returns <c>null</c> on any non-success HTTP status rather than throwing.
    /// </summary>
    private async Task<T?> SfGetAsync<T>(
        SalesforceSession session, string relativeUrl, CancellationToken ct) where T : class
    {
        string url = $"{session.InstanceUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", session.AuthToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var client   = _httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        string json = await response.Content.ReadAsStringAsync(ct);
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// Issues an authenticated POST (or PATCH-via-POST with <c>?_HttpMethod=PATCH</c>) to the
    /// Salesforce REST API.
    /// <para>
    /// The body is serialised with Newtonsoft.Json so that <c>[JsonProperty]</c> attributes on
    /// <c>AdminSupportPortalUser</c> (e.g. <c>NullValueHandling.Ignore</c> on <c>ContactId</c>)
    /// are respected.
    /// </para>
    /// Returns the raw response body on error, or an empty string on success
    /// (HTTP 200/204 with no meaningful payload) — callers treat any non-empty return as an
    /// error message, matching legacy <c>PostApi</c> behaviour.
    /// </summary>
    private async Task<string> SfPostAsync(
        SalesforceSession session, string relativeUrl, object body, CancellationToken ct)
    {
        string url     = $"{session.InstanceUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
        string bodyJson = JsonConvert.SerializeObject(body);

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", session.AuthToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");

        using var client   = _httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, ct);

        // HTTP 204 No Content — successful PATCH with no body
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return string.Empty;

        string responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return responseBody; // error payload — return raw for caller to surface

        // Successful 200 response — parse and re-serialise to match legacy PostApi behaviour
        // (dynamic.ToString() on a JObject returns the JSON string)
        dynamic? parsed = JsonConvert.DeserializeObject<dynamic>(responseBody);
        return parsed?.ToString() ?? string.Empty;
    }

    // ── Static / pure helpers ──────────────────────────────────────────────

    private async Task<string> ResolveUdmSourceCodeAsync(CancellationToken ct)
    {
        var detail = await _settingService.GetProductDetailAsync(ProductId, ct);
        return detail?.UDMSourceCode?.Length > 0
            ? detail.UDMSourceCode
            : detail?.BooksProductCode ?? string.Empty;
    }

    private static string SettingValue(IList<ProductInternalSetting> settings, string name)
        => settings.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value
           ?? string.Empty;

    /// <summary>
    /// Returns the hard-coded Salesforce profile list for Admin Support Portal.
    /// <paramref name="ultraLightRoleId"/> is the only product-settings-driven entry;
    /// all other IDs are stable Salesforce metadata IDs shared across environments.
    /// </summary>
    private static IList<ProductRole> BuildProductRoles(string ultraLightRoleId)
        => new List<ProductRole>
        {
            new() { ID = "00e1G000000JItR", Name = "Client Portal Light",                                           Roletype = "Support Portal" },
            new() { ID = ultraLightRoleId,   Name = "Client Portal Ultra Light",                                    Roletype = "Support Portal" },
            new() { ID = "00e37000000MkG1", Name = "Client Portal with Cancellations",                              Roletype = "Admin Portal"   },
            new() { ID = "00e37000000MkFm", Name = "Client Portal with Billing",                                    Roletype = "Admin Portal"   },
            new() { ID = "00e00000006qqxo", Name = "Client Portal with Transaction Limit and BAC Requestor",        Roletype = "Admin Portal"   },
            new() { ID = "00e00000006qqxn", Name = "Client Portal with Transaction Limit and BAC Approver",         Roletype = "Admin Portal"   },
            new() { ID = "00e00000006qqxm", Name = "Client Portal with Billing, Cancellations, and Payments Admin", Roletype = "Admin Portal"   },
            new() { ID = "00e00000006qqxh", Name = "Client Portal Standard User",                                   Roletype = "Support Portal" },
            new() { ID = "00e1G000000ZR97", Name = "Client Portal Support Admin",                                   Roletype = "Admin Portal"   },
            new() { ID = "00e00000006qqxf", Name = "Client Portal Administrator",                                   Roletype = "Admin Portal"   },
            new() { ID = "00e00000006qqxc", Name = "Client Portal with Billing and Cancellations",                  Roletype = "Admin Portal"   },
        };

    private async Task<ListResponse> MergeProductRolesWithSalesforceAsync(
        SalesforceSession session, IList<ProductRole> allRoles, string productUserId, string apiRoute, CancellationToken ct)
    {
        var aspUser = await SfGetAsync<AdminSupportPortalUser>(
            session, $"{apiRoute}sobjects/user/{productUserId}", ct);

        if (aspUser is null)
        {
            _logger.LogError(
                "MergeProductRolesWithSalesforceAsync - user not found: productUserId {ProductUserId}", productUserId);
            return new ListResponse { IsError = true, ErrorReason = $"User {productUserId} not found." };
        }

        // Salesforce returns both 15- and 18-character IDs; normalise to 15 for comparison
        string profileId = aspUser.ProfileId.Length > 15 ? aspUser.ProfileId[..15] : aspUser.ProfileId;
        var matched = allRoles.FirstOrDefault(r => r.ID.Equals(profileId, StringComparison.OrdinalIgnoreCase));
        if (matched is not null)
            matched.IsAssigned = true;

        return new ListResponse
        {
            Records     = allRoles.Cast<object>().ToList(),
            TotalRows   = allRoles.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1
        };
    }

    private async Task<ListResponse> MergeProductPropertiesWithSalesforceAsync(
        SalesforceSession session, IList<ProductProperty> blueBookPropertyList, string productUserId, string apiRoute, CancellationToken ct)
    {
        var account = await SfGetAsync<AdminSupportPortalAccount>(
            session, $"{apiRoute}sobjects/user/{productUserId}/account", ct);

        if (account is null)
        {
            _logger.LogError(
                "MergeProductPropertiesWithSalesforceAsync - account not found for productUserId {ProductUserId}",
                productUserId);
            return new ListResponse
            {
                IsError     = true,
                ErrorReason = $"User not found in client portal with ProductUserId - {productUserId}"
            };
        }

        var propertyOption = new Dictionary<string, bool>();

        if (string.Equals(account.Type, "PROPERTY", StringComparison.OrdinalIgnoreCase))
        {
            // Single-property assignment — mark the matching entry as selected
            var pp = blueBookPropertyList.FirstOrDefault(a => a.ID == account.OMS_ID__c);
            if (pp is not null)
                pp.IsAssigned = true;

            propertyOption["allProperties"] = false;
        }
        else
        {
            // PMC-level — all properties are accessible
            propertyOption["allProperties"] = true;
        }

        return new ListResponse
        {
            Records     = blueBookPropertyList.Cast<object>().ToList(),
            TotalRows   = blueBookPropertyList.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1,
            Additional  = propertyOption
        };
    }
}
