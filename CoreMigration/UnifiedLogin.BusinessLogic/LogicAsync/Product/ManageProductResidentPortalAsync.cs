using System.Net;
using System.Text;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// True-async implementation of Resident Portal product user management.
/// <para>
/// Replaces <c>ManageProductResidentPortal</c> (sync stepping-stone).
/// No <c>DefaultUserClaim</c>, no mutable instance fields, no blocking <c>.Result</c> calls.
/// </para>
/// <para>
/// HTTP client is obtained per-call via <c>IHttpClientFactory</c> ("ResidentPortal" named client).
/// Headers <c>AB-API-Company-ID</c>, <c>AB-API-Community-ID</c>, <c>X-Forwarded-Proto</c>
/// are set on the per-request <see cref="HttpClient"/> — no shared header mutation between
/// concurrent calls.
/// </para>
/// </summary>
public sealed class ManageProductResidentPortalAsync : IManageProductResidentPortalAsync
{
    #region Constants

    private const int    ProductId          = (int)ProductEnum.ResidentPortal;
    private const int    MaxRetryCount      = 5;
    private const string HttpClientName     = "ResidentPortal";
    private const string ApiEndpointKey     = "APIENDPOINT";
    private const string MtApiEndpointKey   = "MTAPIENDPOINT";
    private const string AppIdKey           = "APPID";
    private const string AppKeyKey          = "APPKEY";
    private const string ForwardedProtocol  = "https";
    private const string ConfigCacheKey     = "RP_ProductConfig";
    private const string UdmSourceCode      = BlueBookProductConstants.ResidentPortal; // "AB"

    // Audit message templates (mirrors ManageProductBase constants)
    private const string RolesAssignMsg = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RolesRemovedMsg = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropsAssignMsg = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropsRemovedMsg = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";

    private static readonly TimeSpan ConfigCacheTtl     = TimeSpan.FromHours(1);
    private static readonly TimeSpan PropertiesCacheTtl = TimeSpan.FromSeconds(300);
    private static readonly TimeSpan BatchDataCacheTtl  = TimeSpan.FromSeconds(600);

    #endregion

    #region Nested types

    /// <summary>Immutable product-config snapshot, cached for 1 hour.</summary>
    internal sealed record RpConfig(
        string ApiEndpoint,
        string MtApiEndpoint,
        string AppId,
        string AppKey);

    #endregion

    #region Dependencies

    private readonly IProductContextServiceAsync               _contextService;
    private readonly IProductRepositoryAsync                   _productRepository;
    private readonly ISamlAttributeServiceAsync                _samlAttributeService;
    private readonly IManageBlueBookAsync                      _blueBook;
    private readonly IManagePersonaAsync                       _managePersona;
    private readonly IManagePersonAsync                        _managePerson;
    private readonly IManageUserLoginAsync                     _userLogin;
    private readonly IManageElectronicAddressAsync             _electronicAddress;
    private readonly IPartyRoleRepositoryAsync                 _partyRoleRepository;
    private readonly ITokenHelperAsync                         _tokenHelper;
    private readonly IMemoryCache                              _cache;
    private readonly IHttpClientFactory                        _httpClientFactory;
    private readonly ILogger<ManageProductResidentPortalAsync> _logger;

    #endregion

    #region Constructor

    public ManageProductResidentPortalAsync(
        IProductContextServiceAsync               contextService,
        IProductRepositoryAsync                   productRepository,
        ISamlAttributeServiceAsync                samlAttributeService,
        IManageBlueBookAsync                      blueBook,
        IManagePersonaAsync                       managePersona,
        IManagePersonAsync                        managePerson,
        IManageUserLoginAsync                     userLogin,
        IManageElectronicAddressAsync             electronicAddress,
        IPartyRoleRepositoryAsync                 partyRoleRepository,
        ITokenHelperAsync                         tokenHelper,
        IMemoryCache                              cache,
        IHttpClientFactory                        httpClientFactory,
        ILogger<ManageProductResidentPortalAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(samlAttributeService);
        ArgumentNullException.ThrowIfNull(blueBook);
        ArgumentNullException.ThrowIfNull(managePersona);
        ArgumentNullException.ThrowIfNull(managePerson);
        ArgumentNullException.ThrowIfNull(userLogin);
        ArgumentNullException.ThrowIfNull(electronicAddress);
        ArgumentNullException.ThrowIfNull(partyRoleRepository);
        ArgumentNullException.ThrowIfNull(tokenHelper);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _contextService       = contextService;
        _productRepository    = productRepository;
        _samlAttributeService = samlAttributeService;
        _blueBook             = blueBook;
        _managePersona        = managePersona;
        _managePerson         = managePerson;
        _userLogin            = userLogin;
        _electronicAddress    = electronicAddress;
        _partyRoleRepository  = partyRoleRepository;
        _tokenHelper          = tokenHelper;
        _cache                = cache;
        _httpClientFactory    = httpClientFactory;
        _logger               = logger;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Properties

    /// <inheritdoc/>
    public async Task<ListResponse> ListPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - Begin. editorPersonaId={EditorId}", nameof(ListPropertiesAsync), editorPersonaId);

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (error is not null) return error;

            var config   = await GetProductConfigAsync(ct);
            var companyMap = await GetCompanyMapInternalAsync(ctx!, config, ct);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");

            var properties = await ListResidentPortalPropertiesAsync(
                config, companySourceId, ctx!.EditorPersona.Organization.PartyId, ct);

            if (properties is null || properties.Count == 0)
            {
                _logger.LogError("{Action} - No active properties. editorPersonaId={EditorId}",
                    nameof(ListPropertiesAsync), editorPersonaId);
                return new ListResponse
                {
                    IsError = true,
                    ErrorReason = $"ManageProductResidentPortalAsync.{nameof(ListPropertiesAsync)} - No active properties found."
                };
            }

            IList<ProductProperty> gbProperties =
                properties.ToGBProperties()?.OrderBy(x => x.Name).ToList()
                ?? [];

            if (userPersonaId != 0 && !string.IsNullOrWhiteSpace(ctx.ProductUserId))
            {
                long firstCommunityId = Convert.ToInt64(properties[0].CommunityId);
                return await MergeProductPropertiesAsync(
                    ctx, config, companySourceId, firstCommunityId,
                    gbProperties, properties, userPersonaId, editorPersonaId, ct);
            }

            return new ListResponse
            {
                Records     = gbProperties.Cast<object>().ToList(),
                TotalRows   = gbProperties.Count,
                RowsPerPage = gbProperties.Count,
                TotalPages  = 1,
                ErrorReason = string.Empty,
                Additional  = new Dictionary<string, bool>
                {
                    ["displayAllProperties"] = true,
                    ["allProperties"]        = false
                }
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}",
                nameof(ListPropertiesAsync), editorPersonaId);
            return new ListResponse
            {
                IsError     = true,
                ErrorReason = ex is SharedObjects.Exceptions.BlueBookException
                    ? ex.Message
                    : CommonMessageConstants.PropertyErrorMessage
            };
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Notifications

    /// <inheritdoc/>
    public async Task<Notifications?> GetNotificationSettingsAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - Begin. editorPersonaId={EditorId}", nameof(GetNotificationSettingsAsync), editorPersonaId);

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (error is not null) return null;

            if (string.IsNullOrWhiteSpace(ctx!.ProductUsername)) return new Notifications();

            var config         = await GetProductConfigAsync(ct);
            var companyMap     = await GetCompanyMapInternalAsync(ctx, config, ct);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
            var properties     = await ListResidentPortalPropertiesAsync(
                config, companySourceId, ctx.EditorPersona.Organization.PartyId, ct);
            long communityId   = properties?.Count > 0 ? Convert.ToInt64(properties[0].CommunityId) : 0;

            var rpUser = await GetUserDetailsAsync(
                config, companySourceId, communityId, ctx.ProductUsername, 0, ct);

            if (rpUser?.Notifications is not null)
                return rpUser.Notifications;

            var batchData = await GetDeactivatedBatchDataAsync(userPersonaId, ct);
            return batchData?.Notifications ?? new Notifications();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}",
                nameof(GetNotificationSettingsAsync), editorPersonaId);
            return null;
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region User

    /// <inheritdoc/>
    public async Task<ResidentPortalUser?> GetUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - Begin. userPersonaId={UserId}", nameof(GetUserAsync), userPersonaId);

        var (ctx, error) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return null;

        var config         = await GetProductConfigAsync(ct);
        var companyMap     = await GetCompanyMapInternalAsync(ctx!, config, ct);
        long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
        var properties     = await ListResidentPortalPropertiesAsync(
            config, companySourceId, ctx!.EditorPersona.Organization.PartyId, ct);
        long communityId   = properties?.Count > 0 ? Convert.ToInt64(properties[0].CommunityId) : 0;

        return await GetUserDetailsAsync(
            config, companySourceId, communityId, ctx.ProductUsername, 0, ct);
    }

    /// <inheritdoc/>
    public async Task<IResidentPortalUser> SetLevelAndGroupObjectsAsync(
        long editorPersonaId,
        long userPersonaId,
        IResidentPortalUser residentPortalUser,
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - Begin. editorPersonaId={EditorId}", nameof(SetLevelAndGroupObjectsAsync), editorPersonaId);

        try
        {
            if (residentPortalUser.MessageGroups is not null)
            {
                var messageGroupList = await ListMessageGroupsAsync(editorPersonaId, userPersonaId, ct);
                foreach (string group in residentPortalUser.MessageGroups)
                {
                    var match = messageGroupList.Find(item => item.Id == group);
                    if (match is not null) match.IsAssigned = true;
                }
                residentPortalUser.MessageGroups   = null;
                residentPortalUser.MessagingGroups = messageGroupList;
            }

            residentPortalUser.Level  = null;
            residentPortalUser.Levels = await ListLevelsAsync(editorPersonaId, userPersonaId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}",
                nameof(SetLevelAndGroupObjectsAsync), editorPersonaId);
        }

        return residentPortalUser;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Levels

    /// <inheritdoc/>
    public async Task<List<ILevel>> ListLevelsAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - Begin. editorPersonaId={EditorId}", nameof(ListLevelsAsync), editorPersonaId);

        var (ctx, error) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return [];

        var config         = await GetProductConfigAsync(ct);
        var companyMap     = await GetCompanyMapInternalAsync(ctx!, config, ct);
        long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
        var properties     = await ListResidentPortalPropertiesAsync(
            config, companySourceId, ctx!.EditorPersona.Organization.PartyId, ct);
        long communityId   = properties?.Count > 0 ? Convert.ToInt64(properties[0].CommunityId) : 0;

        bool isSuperUser = await _contextService.IsSuperUserAsync(ctx.EditorPersona, ct);

        // Fetch current user details (for assigned role + allowedRoles)
        ResidentPortalUser? rpUser = string.IsNullOrWhiteSpace(ctx.ProductUsername)
            ? null
            : await GetUserDetailsAsync(config, companySourceId, communityId, ctx.ProductUsername, 0, ct);

        List<ILevel> levelList;

        try
        {
            if (isSuperUser)
            {
                // Super-admin: full role list from RP API
                string url = $"{config.ApiEndpoint}/roles";
                var response = await ExecuteWithRetryAsync(
                    "GET", url, config, companySourceId, communityId, null, ct);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    var roleData = JsonConvert.DeserializeObject<ResidentPortalRole>(json);
                    levelList = RolesList(roleData?.Data ?? []);
                }
                else
                {
                    levelList = [];
                }
            }
            else
            {
                // Non-admin: use the editor's canCreateRoles from RP user object
                ResidentPortalUser? editorRpUser = string.IsNullOrWhiteSpace(ctx.EditorProductUsername)
                    ? null
                    : await GetUserDetailsAsync(
                        config, companySourceId, communityId, ctx.EditorProductUsername, 0, ct);

                levelList = RolesList(editorRpUser?.canCreateRoles ?? []);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error fetching roles. editorPersonaId={EditorId}",
                nameof(ListLevelsAsync), editorPersonaId);
            throw;
        }

        // Mark the user's current assigned level
        if (rpUser is not null && !string.IsNullOrEmpty(rpUser.FirstName))
        {
            string sLevel = string.Concat(
                rpUser.EnterpriseUserId > 0 ? "ENTERPRISE" : "STAFF",
                rpUser.Level);

            if (!isSuperUser)
            {
                levelList = RolesList(rpUser.allowedRoles ?? []);
            }

            var assigned = levelList.Find(item => item.Id == sLevel);
            if (assigned is not null) assigned.IsAssigned = true;
        }
        else
        {
            var batchData = await GetDeactivatedBatchDataAsync(userPersonaId, ct);
            if (batchData?.RoleList?.Count > 0)
            {
                var assigned = levelList.Find(item => item.Id == batchData.RoleList[0]);
                if (assigned is not null) assigned.IsAssigned = true;
            }
        }

        // Apply editor-level disable rules for non-super-users
        if (!isSuperUser)
        {
            ResidentPortalUser? editorRpUser = string.IsNullOrWhiteSpace(ctx.EditorProductUsername)
                ? null
                : await GetUserDetailsAsync(
                    config, companySourceId, communityId, ctx.EditorProductUsername, 0, ct);

            string editorLevel = string.Concat(
                editorRpUser?.EnterpriseUserId > 0 ? "ENTERPRISE" : "STAFF",
                editorRpUser?.Level);

            // Disable enterprise-admin for enterprise-standard editors
            if (editorLevel.Equals("ENTERPRISESTANDARD", StringComparison.OrdinalIgnoreCase))
            {
                var item = levelList.Find(l => l.Id.Equals("ENTERPRISEADMIN", StringComparison.OrdinalIgnoreCase));
                if (item is not null) item.IsDisabled = true;
            }

            // Disable enterprise roles for staff editors
            if (editorLevel.StartsWith("STAFF", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string id in (string[])["ENTERPRISEADMIN", "ENTERPRISESTANDARD"])
                {
                    var item = levelList.Find(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                    if (item is not null) item.IsDisabled = true;
                }

                if (editorLevel.Equals("STAFFSTANDARD", StringComparison.OrdinalIgnoreCase))
                {
                    var item = levelList.Find(l => l.Id.Equals("STAFFADMIN", StringComparison.OrdinalIgnoreCase));
                    if (item is not null) item.IsDisabled = true;
                }

                if (editorLevel.Equals("STAFFLIMITED", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string id in (string[])["STAFFADMIN", "STAFFSTANDARD"])
                    {
                        var item = levelList.Find(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                        if (item is not null) item.IsDisabled = true;
                    }
                }
            }
        }

        return levelList;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region MessageGroups

    /// <inheritdoc/>
    public async Task<List<IMessagingGroups>> ListMessageGroupsAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        List<IMessagingGroups> groups =
        [
            new MessagingGroups { Id = "MANAGEMENT",       Name = "Management",        IsAssigned = false },
            new MessagingGroups { Id = "RESIDENT_SERVICES", Name = "Resident Services", IsAssigned = false },
            new MessagingGroups { Id = "FRONT_DESK",        Name = "Front Desk",        IsAssigned = false },
            new MessagingGroups { Id = "MAINTENANCE",       Name = "Maintenance",       IsAssigned = false },
            new MessagingGroups { Id = "LEASING",           Name = "Leasing",           IsAssigned = false }
        ];

        var (ctx, error) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, ProductId, ct);
        if (error is not null) return groups.OrderBy(x => x.Name).ToList();

        if (!string.IsNullOrWhiteSpace(ctx!.ProductUsername))
        {
            var config       = await GetProductConfigAsync(ct);
            var companyMap   = await GetCompanyMapInternalAsync(ctx, config, ct);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
            var properties   = await ListResidentPortalPropertiesAsync(
                config, companySourceId, ctx.EditorPersona.Organization.PartyId, ct);
            long communityId = properties?.Count > 0 ? Convert.ToInt64(properties[0].CommunityId) : 0;

            var rpUser = await GetUserDetailsAsync(
                config, companySourceId, communityId, ctx.ProductUsername, 0, ct);

            if (rpUser is not null && !string.IsNullOrEmpty(rpUser.FirstName) && rpUser.MessageGroups is not null)
            {
                foreach (string group in rpUser.MessageGroups)
                {
                    var match = groups.Find(item => item.Id == group);
                    if (match is not null) match.IsAssigned = true;
                }
                return groups.OrderBy(x => x.Name).ToList();
            }
        }

        // Fall back to deactivated batch data
        var batchData = await GetDeactivatedBatchDataAsync(userPersonaId, ct);
        if (batchData?.MessageGroups is not null)
        {
            foreach (string group in batchData.MessageGroups)
            {
                var match = groups.Find(item => item.Id == group);
                if (match is not null) match.IsAssigned = true;
            }
        }

        return groups.OrderBy(x => x.Name).ToList();
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Titles

    /// <inheritdoc/>
    public Task<List<ITitle>> ListTitlesAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        // C# 13: collection expression in a completed task
        List<ITitle> titles =
        [
            new Title { Id = "MANAGER",           Name = "Manager" },
            new Title { Id = "LEASING_AGENT",     Name = "Leasing Agent" },
            new Title { Id = "BOARD",             Name = "Board" },
            new Title { Id = "FRONTDESK",         Name = "Front Desk" },
            new Title { Id = "ASSISTANT_MANAGER", Name = "Assistant Manager" },
            new Title { Id = "NIGHT_SHIFT",       Name = "Night Shift" },
            new Title { Id = "MAINTENANCE",       Name = "Maintenance" },
            new Title { Id = "CORPORATE",         Name = "Corporate" },
            new Title { Id = "OTHER",             Name = "Other" }
        ];
        return Task.FromResult(titles);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Create / Update

    /// <inheritdoc/>
    public async Task<(ObjectOutput<IResidentPortalUser, IErrorData> result, List<AdditionalParameters> auditParams)>
        ManageResidentPortalUserAsync(
            long editorPersonaId,
            long userPersonaId,
            ResidentPortal residentPortal,
            BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
            CancellationToken ct = default)
    {
        List<AdditionalParameters> auditParams = [];
        var output    = new ObjectOutput<IResidentPortalUser, IErrorData>();
        var errStatus = new Status<IErrorData>();

        _logger.LogDebug("{Action} - Begin. userPersonaId={UserId}", nameof(ManageResidentPortalUserAsync), userPersonaId);

        try
        {
            var (ctx, contextError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (contextError is not null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = contextError.ErrorReason;
                output.Status      = errStatus;
                return (output, auditParams);
            }

            // ── Parallel: config + persona + userLogin ────────────────────────
            var configTask  = GetProductConfigAsync(ct).AsTask();
            var personTask  = _managePerson.GetPersonAsync(ctx!.UserPersona!.RealPageId, ct);
            var loginTask   = _userLogin.GetUserLoginOnlyAsync(ctx.UserPersona.RealPageId, ct);
            await Task.WhenAll(configTask, personTask, loginTask);

            var config   = await configTask;
            var person   = await personTask;
            var login    = await loginTask;

            var companyMap       = await GetCompanyMapInternalAsync(ctx, config, ct);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
            long companyInstanceId = companyMap?.CompanyInstanceId ?? 0;

            if (companySourceId == 0)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Company Setup Error: Please Contact Support.";
                output.Status      = errStatus;
                return (output, auditParams);
            }

            var properties = await ListResidentPortalPropertiesAsync(
                config, companySourceId, ctx.EditorPersona.Organization.PartyId, ct);

            if (properties is null || properties.Count == 0)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = "List properties from Resident Portal - No active properties found.";
                output.Status      = errStatus;
                return (output, auditParams);
            }

            long firstCommunityId = Convert.ToInt64(properties[0].CommunityId);

            // ── Email / username resolution ──────────────────────────────────
            string userEmailAddress;
            bool isNonEmailLogin = !login!.LoginName.Contains('@');

            if (userPersonaId > 0 && isNonEmailLogin)
            {
                var addresses = await _electronicAddress.ListElectronicAddressForPersonAsync(
                    login.RealPageId, string.Empty, ct);
                userEmailAddress = addresses?
                    .FirstOrDefault(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                    ?.AddressString ?? string.Empty;

                userEmailAddress = ProductManagerHelpers.ValidateAndReturnEmailAddress(userEmailAddress);

                string[] emailSubstrings = userEmailAddress.Split('@');
                if (emailSubstrings.Length == 2)
                {
                    userEmailAddress = string.Concat(
                        emailSubstrings[0], "+ul",
                        login.LoginName.Replace("@", ""),
                        "ul@", emailSubstrings[1]);
                }
            }
            else
            {
                userEmailAddress = ProductManagerHelpers.ValidateAndReturnEmailAddress(login.LoginName);
            }

            // ── Snapshot state-before-update for audit ───────────────────────
            List<ILevel>          oldRoles          = [];
            List<ProductProperty> oldProperties     = [];
            List<IMessagingGroups>oldMessageGroups  = [];
            Notifications         oldNotifications  = new();

            if (!string.IsNullOrEmpty(ctx.ProductUsername))
            {
                oldRoles         = await ListLevelsAsync(editorPersonaId, userPersonaId, ct);
                var oldPropList  = await ListPropertiesAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
                if (oldPropList.Records is not null)
                    oldProperties = oldPropList.Records.Cast<ProductProperty>().ToList();
                oldMessageGroups = await ListMessageGroupsAsync(editorPersonaId, userPersonaId, ct);
                oldNotifications = await GetNotificationSettingsAsync(editorPersonaId, userPersonaId, ct) ?? new();
            }

            // ── Determine enterprise vs staff, build communityIds list ───────
            bool isEnterprise;
            string productUsername;
            List<long> communityIds = [];
            List<Community> communityList = [];
            ResidentPortalUser rpUser = new();
            string createOrUpdate     = "create";

            if (string.IsNullOrWhiteSpace(ctx.ProductUsername))
            {
                // New user
                isEnterprise  = residentPortal.RoleList.Count == 0
                    || (residentPortal.RoleList.Count == 1
                        && residentPortal.RoleList[0].StartsWith("ENTERPRISE", StringComparison.OrdinalIgnoreCase));

                productUsername = userEmailAddress;

                // Check for username collision with different company
                var existing = await GetUserDetailsAsync(
                    config, companySourceId, firstCommunityId, productUsername, 0, ct);

                if (existing is not null && companySourceId != existing.CompanyId)
                {
                    string[] parts = userEmailAddress.Split('@');
                    productUsername = parts.Length == 2
                        ? string.Concat(parts[0], "+ul", companySourceId, "ul@", parts[1])
                        : string.Concat(userEmailAddress, "+ul", companySourceId);
                }
            }
            else
            {
                createOrUpdate  = "update";
                productUsername = ctx.ProductUsername;

                rpUser = await GetUserDetailsAsync(
                    config, companySourceId, firstCommunityId, productUsername, 0, ct) ?? new();

                if (rpUser.FirstName is null)
                {
                    errStatus.Success  = false;
                    errStatus.ErrorMsg = "Error: User not found in Resident Portal";
                    output.Status      = errStatus;
                    return (output, auditParams);
                }

                if (rpUser.Communities is not null && rpUser.ManagerId > 0)
                    communityList = rpUser.Communities;

                isEnterprise = rpUser.EnterpriseUserId > 0;

                if (batchProcessType != BatchProcessType.ProfileUpdate)
                {
                    bool enterpriseToStaff = isEnterprise
                        && residentPortal.RoleList.Count > 0
                        && residentPortal.RoleList[0].StartsWith("STAFF", StringComparison.OrdinalIgnoreCase);

                    bool staffToEnterprise = rpUser.ManagerId > 0
                        && residentPortal.RoleList.Count > 0
                        && residentPortal.RoleList[0].StartsWith("ENTERPRISE", StringComparison.OrdinalIgnoreCase);

                    if (enterpriseToStaff || staffToEnterprise)
                    {
                        var unassignResult = await UnassignResidentPortalUserAsync(editorPersonaId, userPersonaId, ct);
                        if (!unassignResult.Status.Success)
                        {
                            unassignResult.Status.ErrorMsg += enterpriseToStaff
                                ? "  Unable to switch from Enterprise to Staff."
                                : "  Unable to switch from Staff to Enterprise.";
                            return (unassignResult, auditParams);
                        }
                    }
                }

                if (batchProcessType == BatchProcessType.ProfileUpdate)
                {
                    residentPortal = BuildProfileUpdatePayload(rpUser, isEnterprise, properties, companySourceId);
                }
            }

            // ── Get access token ──────────────────────────────────────────────
            string token;
            try
            {
                token = await _tokenHelper.GetUnifiedLoginServerTokenAsync("usermanagement", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Action} - Failed to get access token.", nameof(ManageResidentPortalUserAsync));
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Failed to get a Unified Login access token";
                output.Status      = errStatus;
                return (output, auditParams);
            }

            // ── Build RP user payload ─────────────────────────────────────────
            var rpUserPayload = BuildResidentPortalUserPayload(
                residentPortal, rpUser, isEnterprise, batchProcessType,
                userEmailAddress, person, companySourceId,
                properties, communityIds);

            communityIds = rpUserPayload.communityIds;
            var dataObject = new DataObject<ResidentPortalUser> { data = rpUserPayload.user };

            string url = config.ApiEndpoint + (isEnterprise ? "/enterprise-users" : "/managers");

            _logger.LogDebug("{Action} - {Op} user. userPersonaId={UserId}, communities={Count}",
                nameof(ManageResidentPortalUserAsync), createOrUpdate, userPersonaId, communityIds.Count);

            // ── POST per community ────────────────────────────────────────────
            Dictionary<long, string> errorCommunityIds = [];

            foreach (long communityId in communityIds)
            {
                // Remove from the "communities to delete" list if still assigned
                communityList.RemoveAll(c => c.CommunityId == communityId);

                try
                {
                    var content = new StringContent(
                        JsonConvert.SerializeObject(dataObject), Encoding.UTF8, "application/json");

                    var postResponse = await ExecuteWithRetryAsync(
                        "POST", url, config, companySourceId, communityId, content, ct);

                    if (!postResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("{Action} - POST failed for community {CommunityId}",
                            nameof(ManageResidentPortalUserAsync), communityId);
                        errorCommunityIds[communityId] = "Error - assign access to community.";
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    errorCommunityIds[communityId] = "Exception - assign access to community.";
                    _logger.LogError(ex, "{Action} - Error for community {CommunityId}",
                        nameof(ManageResidentPortalUserAsync), communityId);
                }
            }

            // ── Remove communities no longer assigned (staff users only) ──────
            if (!isEnterprise && !string.IsNullOrWhiteSpace(ctx.ProductUsername))
            {
                string deleteUrl = $"{config.ApiEndpoint}/managers/{HttpUtility.UrlEncode(productUsername)}";
                foreach (var community in communityList)
                {
                    try
                    {
                        var deleteResponse = await ExecuteWithRetryAsync(
                            "DELETE", deleteUrl, config, companySourceId, community.CommunityId, null, ct);

                        if (!deleteResponse.IsSuccessStatusCode)
                            errorCommunityIds[community.CommunityId] = "Error - remove access to community.";
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        errorCommunityIds[community.CommunityId] = "Exception - remove access to community.";
                        _logger.LogError(ex, "{Action} - Delete error community {CommunityId}",
                            nameof(ManageResidentPortalUserAsync), community.CommunityId);
                    }
                }
            }

            // ── Verify and persist ────────────────────────────────────────────
            var finalUser = await GetUserDetailsAsync(
                config, companySourceId, firstCommunityId, productUsername, 0, ct);

            if (finalUser is null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Failed to create a resident portal user.";
                output.Status      = errStatus;
                return (output, auditParams);
            }

            string userId = isEnterprise
                ? finalUser.EnterpriseUserId.ToString()
                : finalUser.ManagerId.ToString();

            await _samlAttributeService.UpsertAttributesAsync(
                userPersonaId, ProductId,
                new Dictionary<SharedObjects.Enum.SamlAttributeEnum, string>
                {
                    [SharedObjects.Enum.SamlAttributeEnum.productUsername] = productUsername,
                    [SharedObjects.Enum.SamlAttributeEnum.UserId]          = userId
                }, ct);

            if (errorCommunityIds.Count == 0)
            {
                await _productRepository.UpdateProductSettingProductStatusAsync(
                    userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Success, ct);

                errStatus.Success  = true;
                errStatus.ErrorMsg = string.Empty;
                output.obj         = rpUserPayload.user;
                output.Status      = errStatus;

                try
                {
                    auditParams = await BuildAuditParametersAsync(
                        editorPersonaId, userPersonaId, oldRoles, oldProperties,
                        oldMessageGroups, oldNotifications, communityIds, rpUserPayload.user, ct);
                }
                catch (Exception auditEx)
                {
                    _logger.LogError(auditEx, "{Action} - Audit build error. userPersonaId={UserId}",
                        nameof(ManageResidentPortalUserAsync), userPersonaId);
                }
            }
            else
            {
                _logger.LogWarning("{Action} - Failed for {Count}/{Total} communities. userPersonaId={UserId}",
                    nameof(ManageResidentPortalUserAsync), errorCommunityIds.Count, communityIds.Count, userPersonaId);
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Failed to create a resident portal user.";
                output.Status      = errStatus;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. userPersonaId={UserId}",
                nameof(ManageResidentPortalUserAsync), userPersonaId);
            errStatus.Success  = false;
            errStatus.ErrorMsg = $"Error - {ex.Message}";
            output.Status      = errStatus;
        }

        return (output, auditParams);
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Unassign

    /// <inheritdoc/>
    public async Task<ObjectOutput<IResidentPortalUser, IErrorData>> UnassignResidentPortalUserAsync(
        long editorPersonaId,
        long userPersonaId,
        CancellationToken ct = default)
    {
        var output    = new ObjectOutput<IResidentPortalUser, IErrorData>();
        var errStatus = new Status<IErrorData>();

        _logger.LogDebug("{Action} - Begin. userPersonaId={UserId}", nameof(UnassignResidentPortalUserAsync), userPersonaId);

        try
        {
            var (ctx, contextError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, ct);
            if (contextError is not null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = contextError.ErrorReason;
                output.Status      = errStatus;
                return output;
            }

            if (string.IsNullOrWhiteSpace(ctx!.ProductUsername))
            {
                errStatus.Success  = true;
                errStatus.ErrorMsg = string.Empty;
                output.Status      = errStatus;
                return output;
            }

            var config         = await GetProductConfigAsync(ct);
            var companyMap     = await GetCompanyMapInternalAsync(ctx, config, ct, useTranslate: false);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
            var properties     = await ListResidentPortalPropertiesAsync(
                config, companySourceId, ctx.EditorPersona.Organization.PartyId, ct);

            if (properties is null || properties.Count == 0)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = "List properties from Resident Portal - No active properties found.";
                output.Status      = errStatus;
                return output;
            }

            long firstCommunityId = Convert.ToInt64(properties[0].CommunityId);

            var rpUser = await GetUserDetailsAsync(
                config, companySourceId, firstCommunityId, ctx.ProductUsername, 0, ct);

            if (rpUser is null)
            {
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Error: User not found in Resident Portal";
                output.Status      = errStatus;
                return output;
            }

            string encodedUsername = HttpUtility.UrlEncode(ctx.ProductUsername);
            string url;
            List<long> communityList = [];

            if (rpUser.EnterpriseUserId > 0)
            {
                url = $"{config.ApiEndpoint}/enterprise-users/{encodedUsername}";
                communityList.Add(rpUser.CommunityIds!.First());
            }
            else if (rpUser.ManagerId > 0)
            {
                url = $"{config.ApiEndpoint}/managers/{encodedUsername}";
                rpUser.Communities?.ForEach(c => communityList.Add(c.CommunityId));
            }
            else
            {
                errStatus.Success  = true;
                errStatus.ErrorMsg = string.Empty;
                output.obj         = rpUser;
                output.Status      = errStatus;
                return output;
            }

            Dictionary<long, string> errorCommunityIds = [];

            foreach (long communityId in communityList)
            {
                try
                {
                    var response = await ExecuteWithRetryAsync(
                        "DELETE", url, config, companySourceId, communityId, null, ct);

                    if (!response.IsSuccessStatusCode)
                        errorCommunityIds[communityId] = "Error - remove access to community.";
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    errorCommunityIds[communityId] = "Exception - remove access to community.";
                    _logger.LogError(ex, "{Action} - Delete error community {CommunityId}",
                        nameof(UnassignResidentPortalUserAsync), communityId);
                }
            }

            if (errorCommunityIds.Count == 0)
            {
                await _productRepository.UpdateProductSettingProductStatusAsync(
                    userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Deleted, ct);

                await _samlAttributeService.UpsertAttributesAsync(
                    userPersonaId, ProductId,
                    new Dictionary<SharedObjects.Enum.SamlAttributeEnum, string>
                    {
                        [SharedObjects.Enum.SamlAttributeEnum.productUsername] = string.Empty,
                        [SharedObjects.Enum.SamlAttributeEnum.UserId]          = string.Empty
                    }, ct);

                errStatus.Success  = true;
                errStatus.ErrorMsg = string.Empty;
                output.obj         = rpUser;
                output.Status      = errStatus;
            }
            else
            {
                string noun = communityList.Count > 1 ? "communities" : "community";
                _logger.LogWarning("{Action} - Failed to delete {Error}/{Total} {Noun}. userPersonaId={UserId}",
                    nameof(UnassignResidentPortalUserAsync), errorCommunityIds.Count, communityList.Count, noun, userPersonaId);
                errStatus.Success  = false;
                errStatus.ErrorMsg = "Failed to delete a resident portal user.";
                output.Status      = errStatus;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. userPersonaId={UserId}",
                nameof(UnassignResidentPortalUserAsync), userPersonaId);
            errStatus.Success  = false;
            errStatus.ErrorMsg = $"Error in catch {ex.Message}";
            output.Status      = errStatus;
        }

        return output;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Delete

    /// <inheritdoc/>
    public async Task<bool> DeleteUserAsync(
        long editorPersonaId,
        int productUserId,
        string productUsername,
        CancellationToken ct = default)
    {
        _logger.LogDebug("{Action} - Begin. productUsername={Username}", nameof(DeleteUserAsync), productUsername);

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, 0, ProductId, ct);
            if (error is not null) return false;

            var config         = await GetProductConfigAsync(ct);
            var companyMap     = await GetCompanyMapInternalAsync(ctx!, config, ct, useTranslate: false);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");
            var properties     = await ListResidentPortalPropertiesAsync(
                config, companySourceId, ctx!.EditorPersona.Organization.PartyId, ct);

            if (properties is null || properties.Count == 0)
            {
                _logger.LogError("{Action} - No active properties. editorPersonaId={EditorId}",
                    nameof(DeleteUserAsync), editorPersonaId);
                return false;
            }

            long firstCommunityId = Convert.ToInt64(properties[0].CommunityId);

            var rpUser = await GetUserDetailsAsync(
                config, companySourceId, firstCommunityId, productUsername, productUserId, ct);

            if (rpUser is null) return true; // already gone

            string encodedUsername = HttpUtility.UrlEncode(productUsername);
            string url;
            List<long> communityList = [];

            if (rpUser.EnterpriseUserId > 0)
            {
                url = $"{config.ApiEndpoint}/enterprise-users/{encodedUsername}";
                communityList.Add(rpUser.CommunityIds![0]);
            }
            else if (rpUser.ManagerId > 0)
            {
                url = $"{config.ApiEndpoint}/managers/{encodedUsername}";
                rpUser.Communities?.ForEach(c => communityList.Add(c.CommunityId));
            }
            else
            {
                return true;
            }

            Dictionary<long, string> errorIds = [];

            foreach (long communityId in communityList)
            {
                try
                {
                    var response = await ExecuteWithRetryAsync(
                        "DELETE", url, config, companySourceId, communityId, null, ct);

                    if (!response.IsSuccessStatusCode)
                        errorIds[communityId] = "Error - remove access.";
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    errorIds[communityId] = "Exception - remove access.";
                    _logger.LogError(ex, "{Action} - Delete error for {Username}", nameof(DeleteUserAsync), productUsername);
                }
            }

            return errorIds.Count == 0;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. productUsername={Username}", nameof(DeleteUserAsync), productUsername);
            return false;
        }
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Migration

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, 0, ProductId, ct);
            if (error is not null)
            {
                response.ErrorReason = error.ErrorReason;
                return response;
            }

            var config         = await GetProductConfigAsync(ct);
            var companyMap     = await GetCompanyMapInternalAsync(ctx!, config, ct);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");

            if (companySourceId == 0)
            {
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }

            string filter         = "NonMigrated";
            int    startRow       = 0;
            int    resultsPerPage = 1000;

            if (datafilter is not null)
            {
                if (datafilter.FilterBy.TryGetValue("filter", out string? filterVal))
                    filter = filterVal;
                if (datafilter.Pages is not null)
                {
                    startRow       = datafilter.Pages.StartRow;
                    resultsPerPage = datafilter.Pages.ResultsPerPage;
                }
            }

            string url = $"{config.MtApiEndpoint}/{companySourceId}/users" +
                         $"?filter={filter}&app_id={config.AppId}&app_key={config.AppKey}";

            _logger.LogDebug("{Action} - Fetching migration users. url={Url}",
                nameof(GetMigrationUsersAsync), url);

            var client = _httpClientFactory.CreateClient(HttpClientName);
            var apiResponse = await client.GetAsync(url, ct);

            if (!apiResponse.IsSuccessStatusCode)
            {
                _logger.LogError("{Action} - API error {Status}",
                    nameof(GetMigrationUsersAsync), apiResponse.StatusCode);
                return response;
            }

            var json  = await apiResponse.Content.ReadAsStringAsync(ct);
            var users = JsonConvert.DeserializeObject<IList<ResidentPortalMigrationUser>>(json);

            if (users is null)
            {
                _logger.LogWarning("{Action} - No users received from product.",
                    nameof(GetMigrationUsersAsync));
                return response;
            }

            var allUsers = users.Select(x => new MigrationUser
            {
                CompanyInstanceSourceId = x.CompanyInstanceSourceId,
                Email        = x.Email,
                Extra        = x.Extra,
                FirstName    = x.FirstName,
                LastActivity = x.LastActivity,
                LastName     = x.LastName,
                MiddleName   = x.MiddleName,
                Phone        = x.Phone,
                Status       = x.Status,
                Title        = x.Title,
                UserId       = x.UserId,
                Username     = x.Username,
                Properties   = x.Properties
            }).ToList();

            response.RowsPerPage = resultsPerPage;
            response.ErrorReason = string.Empty;
            response.IsError     = false;
            response.TotalPages  = 1;
            response.Records     = allUsers.Cast<object>().ToList();
            response.TotalRows   = allUsers.Count;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}",
                nameof(GetMigrationUsersAsync), editorPersonaId);
            response = new ListResponse { IsError = true, ErrorReason = ex.Message };
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId,
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, 0, ProductId, ct);
            if (error is not null)
            {
                migrateResponse.Message = error.ErrorReason;
                return migrateResponse;
            }

            var config         = await GetProductConfigAsync(ct);
            var companyMap     = await GetCompanyMapInternalAsync(ctx!, config, ct);
            long companySourceId = Convert.ToInt64(companyMap?.CompanyInstanceSourceId ?? "0");

            if (companySourceId == 0)
            {
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            string url = $"{config.MtApiEndpoint}/{companySourceId}/migrate-users" +
                         $"?app_id={config.AppId}&app_key={config.AppKey}";

            var content = new StringContent(
                JsonConvert.SerializeObject(migrateUsers), Encoding.UTF8, "application/json");

            var client   = _httpClientFactory.CreateClient(HttpClientName);
            var response = await client.PutAsync(url, content, ct);
            var json     = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("{Action} - Success. url={Url}", nameof(UpdateUsersMigrationStatusAsync), url);
                return JsonConvert.DeserializeObject<MigrateResponse>(json)
                    ?? new MigrateResponse { Status = false, Message = "Empty response." };
            }

            _logger.LogWarning("{Action} - Failed. StatusCode={Status}", nameof(UpdateUsersMigrationStatusAsync), response.StatusCode);
            migrateResponse.Message = "Cannot update user status to migrated.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. editorPersonaId={EditorId}",
                nameof(UpdateUsersMigrationStatusAsync), editorPersonaId);
            migrateResponse = new MigrateResponse { Status = false, Message = ex.Message };
        }

        return migrateResponse;
    }

    #endregion

    // ═════════════════════════════════════════════════════════════════════════
    #region Private Methods

    /// <summary>
    /// Loads and caches product config (API endpoint, MT endpoint, AppId, AppKey) for 1 hour.
    /// </summary>
    private ValueTask<RpConfig> GetProductConfigAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ConfigCacheKey, out RpConfig? cached) && cached is not null)
            return ValueTask.FromResult(cached);

        return new ValueTask<RpConfig>(FetchAndCacheConfigAsync(ct));
    }

    private async Task<RpConfig> FetchAndCacheConfigAsync(CancellationToken ct)
    {
        var settings = await _productRepository.GetProductInternalSettingsAsync(ProductId, ct);
        string Get(string key) =>
            settings.FirstOrDefault(s => s.Name.Equals(key, StringComparison.OrdinalIgnoreCase))?.Value
            ?? throw new InvalidOperationException($"Missing RP internal setting: {key}");

        var config = new RpConfig(
            ApiEndpoint  : Get(ApiEndpointKey),
            MtApiEndpoint: Get(MtApiEndpointKey),
            AppId        : Get(AppIdKey),
            AppKey       : Get(AppKeyKey));

        _cache.Set(ConfigCacheKey, config, ConfigCacheTtl);
        return config;
    }

    /// <summary>
    /// Wraps BlueBook company-map lookup; <paramref name="useTranslate"/> defaults <c>true</c>.
    /// Returns <c>null</c> when no matching map entry exists.
    /// </summary>
    private async Task<CustomerCompanyMap?> GetCompanyMapInternalAsync(
        ProductCallContext ctx,
        RpConfig config,
        CancellationToken ct,
        bool useTranslate = true)
    {
        var org  = ctx.EditorPersona.Organization;
        var maps = await _blueBook.GetCompanyMapAsync(
            org.RealPageId,
            org.BooksCustomerMasterId,
            source: UdmSourceCode,
            domain: org.OrganizationDomain.Name,
            includeExtra: string.Empty,
            includeGreenBookCares: true,
            useTranslate: useTranslate,
            cancellationToken: ct);

        return maps?.FirstOrDefault(m =>
            m.Source.Equals(UdmSourceCode, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns Resident Portal user details from the RP API.
    /// Tries enterprise-users first, falls back to managers.
    /// Returns <c>null</c> when neither endpoint has the user.
    /// </summary>
    private async Task<ResidentPortalUser?> GetUserDetailsAsync(
        RpConfig config,
        long companySourceId,
        long communityId,
        string? productUsername,
        long productUserId,
        CancellationToken ct)
    {
        string userKey = !string.IsNullOrWhiteSpace(productUsername)
            ? HttpUtility.UrlEncode(productUsername)
            : productUserId.ToString();

        // Try enterprise-users first
        string enterpriseUrl = $"{config.ApiEndpoint}/enterprise-users/{userKey}?expand=communities";
        var response = await ExecuteWithRetryAsync(
            "GET", enterpriseUrl, config, companySourceId, communityId, null, ct);

        if (!response.IsSuccessStatusCode)
        {
            string managersUrl = $"{config.ApiEndpoint}/managers/{userKey}?expand=communities,notifications,messageGroups";
            response = await ExecuteWithRetryAsync(
                "GET", managersUrl, config, companySourceId, communityId, null, ct);
        }

        if (!response.IsSuccessStatusCode) return null;

        var json      = await response.Content.ReadAsStringAsync(ct);
        var dataObject = JsonConvert.DeserializeObject<DataObject<ResidentPortalUser>>(json);
        var user      = dataObject?.data;

        if (user is not null
            && user.CommunityAccessLevel is not null
            && user.CommunityAccessLevel.Equals("ALL", StringComparison.OrdinalIgnoreCase))
        {
            user.AllProperties = true;
        }

        return user;
    }

    /// <summary>
    /// Fetches and caches the full list of active Resident Portal properties (communities)
    /// for the given organisation, refreshing every 5 minutes.
    /// </summary>
    private async Task<List<ResidentPortalProperty>> ListResidentPortalPropertiesAsync(
        RpConfig config,
        long companySourceId,
        long orgPartyId,
        CancellationToken ct)
    {
        string cacheKey = $"ResidentPortalProperties_{orgPartyId}";

        // GetOrCreateAsync with async factory — no blocking, no lambda-captured .Result
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = PropertiesCacheTtl;

            var all    = new List<ResidentPortalProperty>();
            int offset = 0;

            while (true)
            {
                var page = await FetchPropertyPageAsync(config, companySourceId, 100, offset, ct);
                if (page is null || page.Count == 0) break;

                all.AddRange(page);
                offset += 100;
                if (page.Count < 100) break;
            }

            var active = all.Where(p => p.Active).OrderBy(p => p.Title).ToList();
            return (List<ResidentPortalProperty>)active;
        }) ?? [];
    }

    /// <summary>
    /// Fetches a single page of communities from the RP API (no community-ID header needed).
    /// </summary>
    private async Task<List<ResidentPortalProperty>?> FetchPropertyPageAsync(
        RpConfig config,
        long companySourceId,
        int limit,
        int offset,
        CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/communities" +
                     $"?filters={{\"\"{{\"limit\":{limit},\"offset\":{offset}}}}}&expand=services";

        // Properties endpoint does not require community-ID header
        var response = await ExecuteWithRetryAsync(
            "GET", url, config, companySourceId, communityId: 0, addCommunityHeader: false, content: null, ct: ct);

        if (!response.IsSuccessStatusCode) return null;

        var json     = await response.Content.ReadAsStringAsync(ct);
        var dataRoot = JsonConvert.DeserializeObject<DataList<ResidentPortalProperty>>(json);
        return dataRoot?.data?.Count > 0 ? dataRoot.data.ToList() : null;
    }

    /// <summary>
    /// Executes an HTTP request against the Resident Portal API with OAuth token injection
    /// and automatic retry (up to <see cref="MaxRetryCount"/> times) on 401 Unauthorized.
    /// </summary>
    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        string verb,
        string url,
        RpConfig config,
        long companySourceId,
        long communityId,
        HttpContent? content,
        CancellationToken ct,
        bool addCommunityHeader = true)
    {
        int failedCount = 0;

        while (true)
        {
            string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync("usermanagement", ct);

            var client = _httpClientFactory.CreateClient(HttpClientName);

            // Per-request headers — no shared state between concurrent calls
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("X-Forwarded-Proto", ForwardedProtocol);
            client.DefaultRequestHeaders.Add("AB-API-Company-ID", companySourceId.ToString());

            if (addCommunityHeader && communityId > 0)
                client.DefaultRequestHeaders.Add("AB-API-Community-ID", communityId.ToString());

            HttpResponseMessage response = verb.ToUpperInvariant() switch
            {
                "GET"    => await client.GetAsync(url, ct),
                "DELETE" => await client.DeleteAsync(url, ct),
                "POST"   => await client.PostAsync(url, content, ct),
                "PUT"    => await client.PutAsync(url, content, ct),
                _        => throw new ArgumentOutOfRangeException(nameof(verb), $"Unsupported verb: {verb}")
            };

            if (response.IsSuccessStatusCode)
                return response;

            // Only retry on 401 Unauthorized up to MaxRetryCount
            if (response.StatusCode != HttpStatusCode.Unauthorized || ++failedCount >= MaxRetryCount)
            {
                _logger.LogWarning("{Action} - {Verb} {Url} returned {Status} (attempt {Attempt})",
                    nameof(ExecuteWithRetryAsync), verb, url, response.StatusCode, failedCount);
                return response;
            }

            // Token expired — next iteration will fetch a fresh one via ITokenHelperAsync
            _logger.LogDebug("{Action} - 401 received, retrying (attempt {Attempt})",
                nameof(ExecuteWithRetryAsync), failedCount);
        }
    }

    /// <summary>
    /// Merges RP API user-assignment data into a GreenBook property list,
    /// setting <see cref="ProductProperty.IsAssigned"/> and the Additional flags.
    /// </summary>
    private async Task<ListResponse> MergeProductPropertiesAsync(
        ProductCallContext ctx,
        RpConfig config,
        long companySourceId,
        long communityId,
        IList<ProductProperty> gbProperties,
        List<ResidentPortalProperty> properties,
        long userPersonaId,
        long editorPersonaId,
        CancellationToken ct)
    {
        bool displayAllProperties = true;
        bool allProperties        = false;

        var propertyList = gbProperties.ToList();

        var rpUser = await GetUserDetailsAsync(
            config, companySourceId, communityId, ctx.ProductUsername, 0, ct);

        bool isSuperUser = await _contextService.IsSuperUserAsync(ctx.EditorPersona, ct);

        if (rpUser is not null)
        {
            // Editor-community filter for non-admin editors
            if (!isSuperUser)
            {
                ResidentPortalUser? editorUser = string.IsNullOrWhiteSpace(ctx.EditorProductUsername)
                    ? null
                    : await GetUserDetailsAsync(
                        config, companySourceId, communityId, ctx.EditorProductUsername, 0, ct);

                if (editorUser?.CommunityIds?.Count > 0)
                {
                    // O(1) lookup with HashSet — replaces List.Exists
                    var allowed = editorUser.CommunityIds.Select(id => id.ToString()).ToHashSet();
                    propertyList.RemoveAll(p => !allowed.Contains(p.ID));
                }
                else if (editorUser?.Communities?.Count > 0)
                {
                    var allowed = editorUser.Communities
                        .Select(c => c.CommunityId.ToString()).ToHashSet();
                    propertyList.RemoveAll(p => !allowed.Contains(p.ID));
                }
            }

            // Mark assigned properties
            if (rpUser.Communities is not null && rpUser.ManagerId > 0)
            {
                foreach (var community in rpUser.Communities)
                {
                    var match = propertyList.Find(p => p.ID == community.CommunityId.ToString());
                    if (match is not null) match.IsAssigned = true;
                }
            }
            else if (rpUser.CommunityIds is not null && rpUser.EnterpriseUserId > 0)
            {
                foreach (long community in rpUser.CommunityIds)
                {
                    var match = propertyList.Find(p => p.ID == community.ToString());
                    if (match is not null) match.IsAssigned = true;
                }
            }

            displayAllProperties = rpUser.EnterpriseUserId > 0;
            allProperties        = rpUser.AllProperties;
        }
        else
        {
            var batchData = await GetDeactivatedBatchDataAsync(userPersonaId, ct);

            if (batchData?.PropertyList?.Count > 0)
            {
                if (batchData.PropertyList.Count == 1 && batchData.PropertyList[0] == "all")
                {
                    allProperties = true;
                }
                else
                {
                    foreach (string property in batchData.PropertyList)
                    {
                        var match = propertyList.Find(p => p.ID == property);
                        if (match is not null) match.IsAssigned = true;
                    }
                }
            }
        }

        return new ListResponse
        {
            Records     = propertyList.Cast<object>().ToList(),
            TotalRows   = gbProperties.Count,
            RowsPerPage = 9999,
            TotalPages  = 1,
            ErrorReason = string.Empty,
            Additional  = new Dictionary<string, bool>
            {
                ["displayAllProperties"] = displayAllProperties,
                ["allProperties"]        = allProperties
            }
        };
    }

    /// <summary>
    /// Loads deactivated product batch data per user, cached for 10 minutes.
    /// Replaces <c>ManageProductBase.GetDeactivatedProductBatchData</c>.
    /// </summary>
    private async Task<RolePropertyList?> GetDeactivatedBatchDataAsync(
        long userPersonaId,
        CancellationToken ct)
    {
        string cacheKey = $"RP_DeactivatedBatch_{userPersonaId}";

        if (_cache.TryGetValue(cacheKey, out RolePropertyList? cached))
            return cached;

        var settings = await _productRepository.GetProductSettingsByPersonaAsync(userPersonaId, ct);

        bool isDeactivated = settings?.Any(s =>
            s.ProductId == ProductId
            && s.Value == Convert.ToString((int)UserUiStatusType.Deactivated)) == true;

        RolePropertyList? result = null;

        if (isDeactivated)
            result = await _productRepository.GetUserProductDataFromProductBatchAsync(
                userPersonaId, ProductId, ct);

        _cache.Set(cacheKey, result, BatchDataCacheTtl);
        return result;
    }

    /// <summary>
    /// Builds the <see cref="ResidentPortal"/> payload for a Profile Update batch,
    /// reusing the user's existing role, notifications, and community assignments.
    /// </summary>
    private static ResidentPortal BuildProfileUpdatePayload(
        ResidentPortalUser rpUser,
        bool isEnterprise,
        List<ResidentPortalProperty> properties,
        long companySourceId)
    {
        var rp = new ResidentPortal();

        if (isEnterprise)
        {
            rp.Notifications = rpUser.Notifications;
            if (rpUser.CommunityAccessLevel == "ALL")
            {
                rp.PropertyList = ["all"];
                rp.RoleList     = [$"ENTERPRISE{rpUser.Level}"];
            }
            else
            {
                rp.PropertyList = rpUser.CommunityIds?.Select(id => id.ToString()).ToList() ?? [];
                rp.RoleList     = [$"ENTERPRISE{rpUser.Level}"];
            }
            rp.MessageGroups = rpUser.MessageGroups ?? [];
        }
        else
        {
            rp.Notifications = rpUser.Notifications;
            rp.RoleList      = [$"STAFF{rpUser.Level}"];
            rp.MessageGroups = rpUser.MessageGroups ?? [];
            rp.PropertyList  = rpUser.Communities?
                .Select(c => c.CommunityId.ToString()).ToList() ?? [];
        }

        return rp;
    }

    /// <summary>
    /// Builds the <see cref="ResidentPortalUser"/> API payload and the community ID list
    /// from the incoming <see cref="ResidentPortal"/> request.
    /// </summary>
    private static (ResidentPortalUser user, List<long> communityIds) BuildResidentPortalUserPayload(
        ResidentPortal rp,
        ResidentPortalUser existing,
        bool isEnterprise,
        BatchProcessType batchProcessType,
        string userEmailAddress,
        SharedObjects.IdentityConfig.IPerson person,
        long companySourceId,
        List<ResidentPortalProperty> properties,
        List<long> existingCommunityIds)
    {
        List<long> communityIds = [];

        var user = new ResidentPortalUser
        {
            Email     = userEmailAddress,
            FirstName = person.FirstName,
            LastName  = person.LastName
        };

        if (isEnterprise)
        {
            user.Level    = rp.RoleList?.Count == 1
                ? rp.RoleList[0].ToUpper().Replace("ENTERPRISE", "")
                : "ADMIN";
            user.CompanyId             = companySourceId;
            user.CommunityAccessLevel  = "ALL";

            if (rp.PropertyList is null || rp.PropertyList.Count == 0
                || (rp.PropertyList.Count == 1 && rp.PropertyList[0].Equals("all", StringComparison.OrdinalIgnoreCase)))
            {
                communityIds.Add(Convert.ToInt64(properties[0].CommunityId));
                user.CommunityIds = null;
            }
            else
            {
                user.CommunityAccessLevel = "LIMITED";
                user.CommunityIds         = rp.PropertyList.ConvertAll(long.Parse);
                communityIds              = user.CommunityIds;
            }

            user.Notifications = rp.Notifications;
            user.Groups        = rp.MessageGroups?.Count > 0 ? rp.MessageGroups : [];
        }
        else
        {
            user.Level = rp.RoleList?[0].ToUpper().Replace("STAFF", "") ?? string.Empty;
            user.Notifications = rp.Notifications;
            user.Groups        = rp.MessageGroups?.Count > 0 ? rp.MessageGroups : [];

            if (rp.PropertyList?.Count == 1 && rp.PropertyList[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                communityIds = properties.Select(p => Convert.ToInt64(p.CommunityId)).ToList();
            }
            else
            {
                communityIds = rp.PropertyList?.ConvertAll(long.Parse) ?? [];
            }
        }

        return (user, communityIds);
    }

    /// <summary>
    /// Compares before/after state for roles, properties, messaging groups, and notifications
    /// to produce the audit parameter list.
    /// </summary>
    private async Task<List<AdditionalParameters>> BuildAuditParametersAsync(
        long editorPersonaId,
        long userPersonaId,
        List<ILevel> oldRoles,
        List<ProductProperty> oldProperties,
        List<IMessagingGroups> oldMessageGroups,
        Notifications oldNotifications,
        List<long> communityIds,
        ResidentPortalUser updatedUser,
        CancellationToken ct)
    {
        List<AdditionalParameters> audit = [];

        // Roles
        var newRoles    = await ListLevelsAsync(editorPersonaId, userPersonaId, ct);
        var oldAssigned = oldRoles.Find(f => f.IsAssigned);
        var newAssigned = newRoles.Find(f => f.IsAssigned);

        if (oldAssigned?.Name != newAssigned?.Name)
        {
            if (oldAssigned is not null)
                audit.Add(new AdditionalParameters
                {
                    Key   = "Resident Portals Roles",
                    Value = RolesRemovedMsg.Replace("RoleName", oldAssigned.Name)
                });
            if (newAssigned is not null)
                audit.Add(new AdditionalParameters
                {
                    Key   = "Resident Portals Roles",
                    Value = RolesAssignMsg.Replace("RoleName", newAssigned.Name)
                });
        }

        // Properties
        var newPropResponse = await ListPropertiesAsync(
            editorPersonaId, userPersonaId, new RequestParameter(), ct);
        var newProperties = newPropResponse.Records?.Cast<ProductProperty>().ToList() ?? [];

        var communitySet   = communityIds.Select(id => id.ToString()).ToHashSet();
        var oldAssignedPps = oldProperties.Where(p => p.IsAssigned == true).ToList();
        var newAssignedPps = newProperties.Where(p => communitySet.Contains(p.ID)).ToList();

        foreach (var p in oldAssignedPps.Where(op => !newAssignedPps.Any(np => np.ID == op.ID)))
            audit.Add(new AdditionalParameters
            {
                Key   = "Resident Portals Properties",
                Value = PropsRemovedMsg.Replace("PropertyName", p.Name)
            });

        foreach (var p in newAssignedPps.Where(np => !oldAssignedPps.Any(op => op.ID == np.ID)))
            audit.Add(new AdditionalParameters
            {
                Key   = "Resident Portals Properties",
                Value = PropsAssignMsg.Replace("PropertyName", p.Name)
            });

        // Message groups
        var newGroups    = await ListMessageGroupsAsync(editorPersonaId, userPersonaId, ct);
        var updatedGroupSet = (updatedUser.Groups ?? []).ToHashSet();

        foreach (var g in oldMessageGroups.Where(mg => mg.IsAssigned
            && !newGroups.Any(ng => ng.Id == mg.Id && ng.IsAssigned)))
        {
            audit.Add(new AdditionalParameters
            {
                Key   = "Resident Portals Messaging Groups",
                Value = PropsRemovedMsg.Replace("PropertyName", g.Name)
            });
        }

        foreach (var g in newGroups.Where(ng => ng.IsAssigned
            && !oldMessageGroups.Any(og => og.Id == ng.Id && og.IsAssigned)))
        {
            audit.Add(new AdditionalParameters
            {
                Key   = "Resident Portals Messaging Groups",
                Value = PropsAssignMsg.Replace("PropertyName", g.Name)
            });
        }

        // Notifications — three boolean flags
        AddNotificationAudit(audit, "Front desk instructions",
            oldNotifications?.amenitiesViaEmail, updatedUser.Notifications?.amenitiesViaEmail);
        AddNotificationAudit(audit, "Service request submission & updates",
            oldNotifications?.managerMrViaEmail, updatedUser.Notifications?.managerMrViaEmail);
        AddNotificationAudit(audit, "Front desk instructions",
            oldNotifications?.managerFdiViaEmail, updatedUser.Notifications?.managerFdiViaEmail);

        return audit;
    }

    private static void AddNotificationAudit(
        List<AdditionalParameters> audit,
        string label,
        bool? oldVal,
        bool? newVal)
    {
        if (oldVal == newVal) return;

        if (oldVal is not null)
            audit.Add(new AdditionalParameters
            {
                Key   = $"Resident Portals Notifications {label}",
                Value = RolesRemovedMsg.Replace("RoleName", oldVal == true ? "True" : "False")
            });

        audit.Add(new AdditionalParameters
        {
            Key   = $"Resident Portals Notifications {label}",
            Value = RolesRemovedMsg.Replace("RoleName", newVal == true ? "True" : "False")
        });
    }

    /// <summary>
    /// Converts a <c>Dictionary&lt;string, string&gt;</c> role map from the RP API
    /// into a typed <see cref="ILevel"/> list.  Keys such as "ENTERPRISE_ADMIN" are
    /// normalised to "ENTERPRISEADMIN" (underscore stripped) to match UI expectations.
    /// </summary>
    private static List<ILevel> RolesList(Dictionary<string, string> roleData)
        => roleData.Select(kv => (ILevel)new Level
        {
            Id         = kv.Key.Replace("_", string.Empty),
            Name       = kv.Value,
            IsAssigned = false,
            IsDisabled = false
        }).ToList();

    #endregion
}
