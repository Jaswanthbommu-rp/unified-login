using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.SharedObjects.Cache;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Product.Ops.Extensions;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True async implementation of Ops (Spend Management) user-management operations.
/// Replaces the stepping-stone <c>Task.FromResult(new ManageProductOps(userClaim).Method(...))</c>
/// pattern with fully async/await calls backed by injected services.
/// </summary>
public sealed class ManageProductOpsAsync : IManageProductOpsAsync
{
    private const int    ProductId               = (int)ProductEnum.OpsBuyer;
    private const int    MaxRetryCount           = 5;
    private const int    SidCacheMinutes         = 90;
    private const int    HttpTimeoutSeconds      = 60;
    private const string CacheKeySidPrefix       = "opsSid_";
    private const string ProductStatusSettingType = "ProductStatus";

    // Activity-log JSON templates (mirrors ManageProductBase constants)
    private const string RoleAssignMsg     = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RoleRemovedMsg    = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropAssignMsg     = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropRemovedMsg    = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";
    private const string RightAssignMsg    = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
    private const string RightUnassignMsg  = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
    private const string RoleDescMsg       = "{\"action\":\"Role Description updated\",\"value\":\"NewValue\"}";
    private const string RoleInvTimeoutMsg = "{\"action\":\"Role InvoiceWorkflowTimeout updated\",\"value\":\"NewValue\"}";
    private const string RoleOrdTimeoutMsg = "{\"action\":\"Role OrderWorkflowTimeout updated\",\"value\":\"NewValue\"}";
    private const string RoleOrdEndorseMsg = "{\"action\":\"Role OrderEndorseEmailReminderFlag updated\",\"value\":\"NewValue\"}";
    private const string RoleInvEndorseMsg = "{\"action\":\"Role InvoiceEndorseEmailReminderFlag updated\",\"value\":\"NewValue\"}";

    private readonly IProductContextServiceAsync      _contextService;
    private readonly IProductSettingServiceAsync      _settingService;
    private readonly IManagePersonaAsync              _managePersona;
    private readonly IManagePersonAsync               _managePerson;
    private readonly IManageContactMechanismAsync     _manageContactMechanism;
    private readonly IManageUserLoginAsync            _manageUserLogin;
    private readonly IManagePartyRelationshipAsync    _managePartyRelationship;
    private readonly ISamlRepositoryAsync             _samlRepository;
    private readonly IUserLoginPersonaRepositoryAsync _userLoginPersonaRepository;
    private readonly IUserRepositoryAsync             _userRepository;
    private readonly IHttpClientFactory               _httpClientFactory;
    private readonly ICacheService                    _cache;
    private readonly ILogger<ManageProductOpsAsync>   _logger;

    public ManageProductOpsAsync(
        IProductContextServiceAsync      contextService,
        IProductSettingServiceAsync      settingService,
        IManagePersonaAsync              managePersona,
        IManagePersonAsync               managePerson,
        IManageContactMechanismAsync     manageContactMechanism,
        IManageUserLoginAsync            manageUserLogin,
        IManagePartyRelationshipAsync    managePartyRelationship,
        ISamlRepositoryAsync             samlRepository,
        IUserLoginPersonaRepositoryAsync userLoginPersonaRepository,
        IUserRepositoryAsync             userRepository,
        IHttpClientFactory               httpClientFactory,
        ICacheService                    cache,
        ILogger<ManageProductOpsAsync>   logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);            _contextService            = contextService;
        ArgumentNullException.ThrowIfNull(settingService);            _settingService            = settingService;
        ArgumentNullException.ThrowIfNull(managePersona);             _managePersona             = managePersona;
        ArgumentNullException.ThrowIfNull(managePerson);              _managePerson              = managePerson;
        ArgumentNullException.ThrowIfNull(manageContactMechanism);    _manageContactMechanism    = manageContactMechanism;
        ArgumentNullException.ThrowIfNull(manageUserLogin);           _manageUserLogin           = manageUserLogin;
        ArgumentNullException.ThrowIfNull(managePartyRelationship);   _managePartyRelationship   = managePartyRelationship;
        ArgumentNullException.ThrowIfNull(samlRepository);            _samlRepository            = samlRepository;
        ArgumentNullException.ThrowIfNull(userLoginPersonaRepository); _userLoginPersonaRepository = userLoginPersonaRepository;
        ArgumentNullException.ThrowIfNull(userRepository);            _userRepository            = userRepository;
        ArgumentNullException.ThrowIfNull(httpClientFactory);         _httpClientFactory         = httpClientFactory;
        ArgumentNullException.ThrowIfNull(cache);                     _cache                     = cache;
        ArgumentNullException.ThrowIfNull(logger);                    _logger                    = logger;
    }

    // ── GetOpsAssetGroupsAsync ───────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetOpsAssetGroupsAsync(
        long editorPersonaId, long userPersonaId, int assetGroupId = 0,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        if (string.IsNullOrEmpty(session.Sid))
            return new ListResponse { IsError = true, ErrorReason = "Unable to get Ops session." };

        var result = new ListResponse();
        try
        {
            // Verify asset-groups mode is enabled for this company
            int moduleAssetGroups = await GetModuleAssetGroupsConfigAsync(session, ct);
            if (moduleAssetGroups != 1)
                return result; // company is not configured for asset groups

            string url = $"{session.BaseUrl}/api/v1.0/assetgroups{(assetGroupId > 0 ? $"/{assetGroupId}" : "")}";
            _logger.LogDebug("GetOpsAssetGroupsAsync - GET {Url}", url);

            using var response = await OpsGetWithRetryAsync(settings, session, url, ct);
            if (!response.IsSuccessStatusCode)
                return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the asset group." };

            string body = await response.Content.ReadAsStringAsync(ct);
            List<AssetGroup> assetGroups;

            if (assetGroupId == 0)
            {
                assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(body) ?? new List<AssetGroup>();
            }
            else
            {
                var single = JsonConvert.DeserializeObject<AssetGroup>(body)!;
                single.property_list.ToList().ForEach(p => p.Properties = null);
                assetGroups = new List<AssetGroup> { single };
            }

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var opsUser = await OpsGetUserByIdAsync(session, ctx.ProductUserId, ct);
                if (opsUser is not null && !string.IsNullOrEmpty(opsUser.AssetID))
                {
                    var ag = assetGroups.FirstOrDefault(a => a.AssetID == opsUser.AssetID);
                    if (ag is not null) ag.IsAssigned = true;
                }
            }

            result = new ListResponse
            {
                Records     = assetGroups.Cast<object>().ToList(),
                TotalRows   = assetGroups.Count,
                RowsPerPage = assetGroups.Count,
                TotalPages  = 1,
                ErrorReason = "",
                Additional  = "AssetGroups"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOpsAssetGroupsAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            result = new ListResponse { IsError = true, ErrorReason = "There was a problem getting the asset group" };
        }
        return result;
    }

    // ── GetOpsAssetsAsync ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetOpsAssetsAsync(
        long editorPersonaId, long userPersonaId, string status,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        if (string.IsNullOrEmpty(session.Sid))
            return new ListResponse { IsError = true, ErrorReason = "Unable to get Ops session." };

        try
        {
            // Verify company config — result is not used beyond the guard check
            _ = await GetModuleAssetGroupsConfigAsync(session, ct);

            string url = $"{session.BaseUrl}/api/v1.0/properties?status={(string.IsNullOrWhiteSpace(status) ? "all" : status)}";
            _logger.LogDebug("GetOpsAssetsAsync - GET {Url}", url);

            using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
            using var client   = CreateClient();
            using var response = await client.SendAsync(req, ct);

            var portfolioList = JsonConvert.DeserializeObject<List<Portfolio>>(
                await response.Content.ReadAsStringAsync(ct)) ?? new List<Portfolio>();

            portfolioList.ForEach(p => p.Properties = null);
            portfolioList = portfolioList
                .Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var opsUser = await OpsGetUserByIdAsync(session, ctx.ProductUserId, ct);
                if (opsUser is not null && !string.IsNullOrEmpty(opsUser.AssetID))
                {
                    var p = portfolioList.FirstOrDefault(a => a.ID == opsUser.AssetID);
                    if (p is not null) p.IsAssigned = true;
                }
            }

            return new ListResponse
            {
                Records     = portfolioList.Cast<object>().ToList(),
                TotalRows   = portfolioList.Count,
                RowsPerPage = portfolioList.Count,
                TotalPages  = 1,
                ErrorReason = "",
                Additional  = "Portfolio"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOpsAssetsAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = "There was a problem getting the asset group" };
        }
    }

    // ── CreateOpsAssetGroupAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> CreateOpsAssetGroupAsync(
        long editorPersonaId, long userPersonaId, AssetGroupCreate assetGroup,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        IList<AssetGroup> assetGroupList = new List<AssetGroup>();
        try
        {
            int moduleAssetGroups = await GetModuleAssetGroupsConfigAsync(session, ct);
            if (moduleAssetGroups != 1)
                return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "", IsError = false };

            string url = $"{session.BaseUrl}/api/v1.0/assetgroups";
            _logger.LogDebug("CreateOpsAssetGroupAsync - POST {Url}", url);

            using var req      = OpsRequest(HttpMethod.Post, url, session.Sid, assetGroup);
            using var client   = CreateClient();
            using var response = await client.SendAsync(req, ct);

            if (response.IsSuccessStatusCode)
            {
                var postResult = JsonConvert.DeserializeObject<AssetGroup>(await response.Content.ReadAsStringAsync(ct))!;
                postResult.Name = assetGroup.Name;
                assetGroupList.Add(postResult);
                return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };
            }

            string error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("CreateOpsAssetGroupAsync - error: {Error}", error);
            return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateOpsAssetGroupAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "There was a problem creating the AssetGroup. " + ex.Message, IsError = true };
        }
    }

    // ── UpdateOpsAssetGroupAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateOpsAssetGroupAsync(
        long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupCreate assetGroup,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        IList<AssetGroup> assetGroupList = new List<AssetGroup>();
        try
        {
            int moduleAssetGroups = await GetModuleAssetGroupsConfigAsync(session, ct);
            if (moduleAssetGroups != 1)
                return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "", IsError = false };

            string url = $"{session.BaseUrl}/api/v1.0/assetgroups/{assetGroupId}";
            _logger.LogDebug("UpdateOpsAssetGroupAsync - PUT {Url}", url);

            using var req      = OpsRequest(HttpMethod.Put, url, session.Sid, assetGroup);
            using var client   = CreateClient();
            using var response = await client.SendAsync(req, ct);

            if (response.IsSuccessStatusCode)
            {
                var postResult = JsonConvert.DeserializeObject<AssetGroup>(await response.Content.ReadAsStringAsync(ct))!;
                postResult.Name = assetGroup.Name;
                assetGroupList.Add(postResult);
                return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };
            }

            string error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("UpdateOpsAssetGroupAsync - error: {Error}", error);
            return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateOpsAssetGroupAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "There was a problem updating the AssetGroup. " + ex.Message, IsError = true };
        }
    }

    // ── PatchOpsAssetGroupAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> PatchOpsAssetGroupAsync(
        long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupPatch assetGroup,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        IList<AssetGroupPatch> patchList = new List<AssetGroupPatch> { assetGroup };
        try
        {
            int moduleAssetGroups = await GetModuleAssetGroupsConfigAsync(session, ct);
            if (moduleAssetGroups != 1)
                return new ListResponse { Records = patchList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "", IsError = false };

            string url = $"{session.BaseUrl}/api/v1.0/assetgroups/{assetGroupId}";
            _logger.LogDebug("PatchOpsAssetGroupAsync - PATCH {Url}", url);

            using var req      = OpsRequest(new HttpMethod("PATCH"), url, session.Sid, assetGroup);
            using var client   = CreateClient();
            using var response = await client.SendAsync(req, ct);

            if (response.IsSuccessStatusCode)
                return new ListResponse { Records = patchList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };

            string error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("PatchOpsAssetGroupAsync - error: {Error}", error);
            return new ListResponse { Records = patchList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = error, IsError = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PatchOpsAssetGroupAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = "There was a problem patching the AssetGroup. " + ex.Message };
        }
    }

    // ── GetCompanyAssetsAsync ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetCompanyAssetsAsync(
        long editorPersonaId, long userPersonaId, bool includeDisabled, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null)
        {
            ctxError.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
            return ctxError;
        }

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        return await GetCompanyAssetDetailsAsync(session, ctx, userPersonaId, includeDisabled, updateAssetNames: true, buildHierarchy: true, ct);
    }

    // ── GetRolesAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId, string assetCode, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null)
        {
            ctxError.ErrorReason = CommonMessageConstants.RoleErrorMessage;
            return ctxError;
        }

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        return await GetRolesInternalAsync(session, ctx, userPersonaId, assetCode, ct);
    }

    // ── GetRolesCountAsync ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesCountAsync(
        long editorPersonaId, string assetCode,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        return await GetAllRolesAsync(session, ct);
    }

    // ── GetRolesForRightAsync ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesForRightAsync(
        long editorPersonaId, int rightId,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        try
        {
            return await GetRolesInternalAsync(session, ctx, editorPersonaId, string.Empty, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesForRightAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
    }

    // ── GetRightsAsync ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsAsync(
        long editorPersonaId,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        var response = new ListResponse();
        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/rights";
            using var httpResponse = await OpsGetWithRetryAsync(settings, session, url, ct);

            if (httpResponse.IsSuccessStatusCode)
            {
                var rightGroup = JsonConvert.DeserializeObject<RightGroup>(await httpResponse.Content.ReadAsStringAsync(ct)) ?? new RightGroup();
                List<MainGroup> list = EnableComplianceRights(rightGroup.ToRightsFormatForClient());
                response = new ListResponse { Records = list.Cast<object>().ToList(), TotalRows = list.Count, RowsPerPage = list.Count, TotalPages = 1, ErrorReason = "" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRightsAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            response.IsError     = true;
            response.ErrorReason = "There was a problem getting the rights";
        }
        return response;
    }

    // ── GetRightsByRoleAsync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId, long roleId,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        var response = new ListResponse();
        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/roles/{roleId}";
            using var httpResponse = await OpsGetWithRetryAsync(settings, session, url, ct);

            if (httpResponse.IsSuccessStatusCode)
            {
                var rightGroup = JsonConvert.DeserializeObject<RightGroupRole>(await httpResponse.Content.ReadAsStringAsync(ct)) ?? new RightGroupRole();
                List<MainGroup> list = rightGroup.rights.ToRightsFormatForClient();
                response = new ListResponse { Records = list.Cast<object>().ToList(), TotalRows = list.Count, RowsPerPage = list.Count, TotalPages = 1, ErrorReason = "" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRightsByRoleAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            response.IsError     = true;
            response.ErrorReason = "There was a problem getting the rights";
        }
        return response;
    }

    // ── CreateRoleAsync ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> CreateRoleAsync(
        long editorPersonaId, OpsInput rightInput, long roleId,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, editorPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        var rightsToAdd    = new List<string>();
        var rightsToRemove = new List<string>();
        var rightsInput    = new List<object>();

        foreach (var item in rightInput.rightsList)
        {
            rightsInput.Add(new Dictionary<string, string> { { "name", item.Name }, { "value", item.Value } });
            if (item.Value == "1") rightsToAdd.Add(item.Name);
            else                   rightsToRemove.Add(item.Name);
        }

        dynamic newRole = new
        {
            name = rightInput.RoleName,
            description = rightInput.RoleDesc,
            invoice_endorse_email_reminder_flag = rightInput.InvoiceEndorseEmailReminderFlag == "true" ? "1" : "0",
            order_workflow_timeout = string.IsNullOrEmpty(rightInput.OrderWorkflowTimeout) ? "0" : rightInput.OrderWorkflowTimeout,
            invoice_workflow_timeout = string.IsNullOrEmpty(rightInput.InvoiceWorkflowTimeout) ? "0" : rightInput.InvoiceWorkflowTimeout,
            order_endorse_email_reminder_flag = rightInput.OrderEndorseEmailReminderFlag == "true" ? "1" : "0",
            responsibility_list = rightsInput
        };

        var roleListResponse = new ListResponse();
        IList<Role> emptyRoles = new List<Role>();

        if (roleId == 0)
        {
            // Create new role
            try
            {
                string url = $"{session.BaseUrl}/api/v1.0/roles";
                _logger.LogDebug("CreateRoleAsync - POST {Url}", url);

                using var req      = OpsRequest(HttpMethod.Post, url, session.Sid, (object)newRole);
                using var client   = CreateClient();
                using var response = await client.SendAsync(req, ct);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Role>(await response.Content.ReadAsStringAsync(ct))!;
                    roleListResponse = new ListResponse { Records = new List<Role> { result }.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };
                    // TODO: emit activity log (AddUpdateRoleLogMessage, UpdateRightsToRoleLogMessage)
                    // requires IManageUnifiedLoginAsync.PushToQueueAsync (not yet available on interface)
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("CreateRoleAsync - create failed: {Error}", error);
                    roleListResponse = new ListResponse { Records = emptyRoles.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateRoleAsync - create error for editorPersonaId {EditorPersonaId}", editorPersonaId);
                roleListResponse = new ListResponse { Records = emptyRoles.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "There was a problem creating the role. " + ex.Message, IsError = true };
            }
        }
        else
        {
            // Update existing role
            try
            {
                var existingRolesResponse = await GetAllRolesAsync(session, ct);
                var roleList = existingRolesResponse.Records.Cast<Role>().ToList();
                var oldRole  = roleList.FirstOrDefault(r => r.Id == roleId.ToString());

                string url = $"{session.BaseUrl}/api/v1.0/roles/{roleId}";
                _logger.LogDebug("CreateRoleAsync (update) - PUT {Url}", url);

                using var req      = OpsRequest(HttpMethod.Put, url, session.Sid, (object)newRole);
                using var client   = CreateClient();
                using var response = await client.SendAsync(req, ct);

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Role>(await response.Content.ReadAsStringAsync(ct))!;
                    roleListResponse = new ListResponse { Records = new List<Role> { result }.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };
                    // TODO: emit activity log for field changes (description, timeouts, flags)
                    // requires IManageUnifiedLoginAsync.PushToQueueAsync (not yet available on interface)
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("CreateRoleAsync (update) - failed: {Error}", error);
                    roleListResponse = new ListResponse { Records = emptyRoles.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateRoleAsync (update) - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
                roleListResponse = new ListResponse { Records = emptyRoles.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "There was a problem creating the role. " + ex.Message, IsError = true };
            }
        }

        return roleListResponse;
    }

    // ── ManageOpsUserAsync ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(string error, List<AdditionalParameters> additionalParameters)> ManageOpsUserAsync(
        long editorPersonaId, long userPersonaId, List<int> roleList, List<int> propertyList,
        CancellationToken ct = default)
    {
        var additionalParameters = new List<AdditionalParameters>();

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return (ctxError.ErrorReason, additionalParameters);

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        // Resolve the user persona and person details
        var userPersona     = await _managePersona.GetPersonaAsync(userPersonaId, withRights: false, ct);
        var person          = await _managePerson.GetPersonAsync(userPersona.RealPageId, ct);
        var userLogin       = await _manageUserLogin.GetUserLoginOnlyAsync(userPersona.RealPageId, ct);
        var userLoginPersonas = await _userLoginPersonaRepository.ListUserLoginPersonaAsync(
            userLoginPersonaId: null, userLoginId: userPersona.UserId, organizationPartyId: userPersona.Organization.PartyId, ct);
        var employeeId      = await _userRepository.GetUserEmployeeIdAsync(
            userLoginPersonas[0].UserLoginPersonaId, userPersona.OrganizationPartyId, ct);
        person.EmployeeId   = (employeeId is not null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : null;

        // Resolve email address
        var contactMechanisms = await _manageContactMechanism.ListContactMechanismForPersonAsync(userPersona.RealPageId, null, ct);
        string userEmailAddress;

        if (userPersona.UserTypeId == (int)UserRoleType.UserNoEmail &&
            contactMechanisms.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true
                                    && a.contactMechanismUsageType?.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true))
        {
            userEmailAddress = contactMechanisms
                .Where(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)
                          && a.contactMechanismUsageType.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.AddressString).FirstOrDefault() ?? string.Empty;
        }
        else if (contactMechanisms.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true
                                          && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true))
        {
            userEmailAddress = contactMechanisms
                .Where(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)
                          && a.contactMechanismUsageType.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.AddressString).FirstOrDefault() ?? string.Empty;
        }
        else
        {
            userEmailAddress = userLogin.LoginName;
        }
        userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

        string userPhoneNumber = "555-555-5555";
        var primaryPhone = contactMechanisms.FirstOrDefault(a =>
            a.AddressType?.Equals("PHONE", StringComparison.OrdinalIgnoreCase) == true
         && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true);
        if (primaryPhone is not null) userPhoneNumber = primaryPhone.AddressString;

        // Determine if this is a super user
        bool isSuperUser = await IsSuperUserAsync(userPersona, ct);

        // Resolve the product username — for new users, generate and check uniqueness
        string productUsername = ctx.ProductUserId.Length > 0 ? ctx.ProductUsername : string.Empty;
        if (string.IsNullOrEmpty(ctx.ProductUserId))
        {
            string lastNameNoWhiteSpace = person.LastName.TrimWhiteSpace();
            string baseUsername = (person.FirstName.TrimWhiteSpace().Substring(0, 1)
                + lastNameNoWhiteSpace.Substring(0, Math.Min(lastNameNoWhiteSpace.Length, 19))).ToLower();

            productUsername = baseUsername;
            try
            {
                int incrementor = 0;
                while (await OpsLoginNameInUseAsync(session, productUsername, ct))
                {
                    incrementor++;
                    productUsername = baseUsername + incrementor;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ManageOpsUserAsync - username check failed");
                return ("There was a problem getting the user information.", additionalParameters);
            }
        }

        if (!isSuperUser && (propertyList.Count == 0 || roleList.Count == 0))
            return ("There was a problem creating the user. Missing required information.", additionalParameters);

        // Resolve company asset / role
        var assetListResponse = await GetCompanyAssetDetailsAsync(session, ctx, userPersonaId, includeDisabled: true, updateAssetNames: false, buildHierarchy: false, ct);
        string assetType = (assetListResponse.Additional as string ?? string.Empty).ToUpper();
        string? assetCode = null, assetName = null;

        try
        {
            switch (assetType)
            {
                case "PORTFOLIO":
                    var portfolioList = assetListResponse.Records.Cast<Portfolio>().ToList();
                    var p = portfolioList.FirstOrDefault(a => (isSuperUser && string.IsNullOrEmpty(a.ParentAssetId)) || (!isSuperUser && a.ID == propertyList[0].ToString()));
                    if (p is not null) { assetCode = p.Code; assetName = p.Name; }
                    break;
                case "ASSETGROUPS":
                    var assetGroupList = assetListResponse.Records.Cast<AssetGroup>().ToList();
                    var ag = assetGroupList.FirstOrDefault(a => (isSuperUser && a.GroupType.Equals("COMPANY", StringComparison.OrdinalIgnoreCase)) || (!isSuperUser && a.ID == propertyList[0].ToString()));
                    if (ag is not null) { assetCode = ag.Code; assetName = ag.Name; }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ManageOpsUserAsync - asset group resolution failed");
            return ("There was a problem creating the user. Invalid asset group.", additionalParameters);
        }

        if (string.IsNullOrEmpty(assetCode) && string.IsNullOrEmpty(assetName))
            return ("There was a problem creating the user. Invalid asset group.", additionalParameters);

        var rolesResponse = await GetRolesInternalAsync(session, ctx, userPersonaId, assetCode: isSuperUser ? null : (assetType == "PORTFOLIO" ? assetCode : null), ct);
        var productRoles  = rolesResponse.Records.Cast<ProductRole>().ToList();
        string roleName   = productRoles.FirstOrDefault(a => (isSuperUser && a.Roletype == "1") || (!isSuperUser && a.ID == roleList[0].ToString()))?.Name ?? string.Empty;

        if (string.IsNullOrEmpty(roleName))
            return ("There was a problem creating the user. Invalid role.", additionalParameters);

        OpsUser? userDetailsBeforeUpdate = !string.IsNullOrEmpty(ctx.ProductUserId)
            ? await OpsGetUserByIdAsync(session, ctx.ProductUserId, ct)
            : null;

        var manageUser = new OpsUser
        {
            FirstName  = person.FirstName,
            MiddleName = person.MiddleName,
            LastName   = person.LastName,
            EmployeeId = person.EmployeeId,
            Loginname  = productUsername,
            Password   = PasswordGenerator.GeneratePassword(15, 5),
            RoleName   = roleName,
            AssetCode  = assetCode ?? string.Empty,
            AssetName  = assetName,
            UserTypeId = null,
            AssetID    = null,
            Email      = userEmailAddress,
            Phone      = userPhoneNumber,
            Status     = "active"
        };

        if (string.IsNullOrEmpty(ctx.ProductUserId))
        {
            // Create new Ops user
            try
            {
                await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Running, ct);

                string url = $"{session.BaseUrl}/api/v1.0/users";
                _logger.LogDebug("ManageOpsUserAsync - POST create user {Url}", url);

                using var req      = OpsRequest(HttpMethod.Post, url, session.Sid, manageUser);
                using var client   = CreateClient();
                using var response = await client.SendAsync(req, ct);

                if (response.IsSuccessStatusCode)
                {
                    dynamic? userResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync(ct));
                    string newid       = userResult?.id ?? string.Empty;
                    string loginName   = userResult?.Value<string>("login_name") ?? string.Empty;

                    await _samlRepository.CreateSamlUserAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.productUsername, loginName, ct);
                    await _samlRepository.CreateSamlUserAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.UserId, newid, ct);
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Success, ct);

                    _logger.LogDebug("ManageOpsUserAsync - created user newid={NewId}", newid);
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("ManageOpsUserAsync - create failed: {Error}", errorContent);
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Error, ct);
                    return ("There was a problem creating the user. " + errorContent, additionalParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ManageOpsUserAsync - create error for editorPersonaId {EditorPersonaId}", editorPersonaId);
                await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Error, ct);
                return ("There was a problem creating the user. " + ex.Message, additionalParameters);
            }
        }
        else
        {
            // Update existing Ops user
            try
            {
                manageUser.ID       = ctx.ProductUserId;
                manageUser.Password = null;

                string url = $"{session.BaseUrl}/api/v1.0/users/{ctx.ProductUserId}";
                _logger.LogDebug("ManageOpsUserAsync - PUT update user {Url}", url);

                using var req      = OpsRequest(HttpMethod.Put, url, session.Sid, manageUser);
                using var client   = CreateClient();
                using var response = await client.SendAsync(req, ct);

                if (response.IsSuccessStatusCode)
                {
                    dynamic? userResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync(ct));
                    string newid        = userResult?.id ?? string.Empty;
                    var samlAttr = new SamlAttributes { SamlAttributeId = (int)SamlAttributeEnum.UserId, Value = newid };//PersonaId = userPersonaId,
                    await _samlRepository.UpdateSamlUserAttributeAsync(samlAttr, ct);
                    await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Success, ct);
                    _logger.LogDebug("ManageOpsUserAsync - updated user newid={NewId}", newid);
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("ManageOpsUserAsync - update failed: {Error}", errorContent);
                    return ("There was a problem updating the user", additionalParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ManageOpsUserAsync - update error for editorPersonaId {EditorPersonaId}", editorPersonaId);
                return ("There was a problem updating the user", additionalParameters);
            }
        }

        // Build activity log additional parameters
        try
        {
            string oldRoleName = userDetailsBeforeUpdate?.UserTypeId is { Length: > 0 }
                ? productRoles.Find(f => f.ID == userDetailsBeforeUpdate.UserTypeId)?.Name ?? string.Empty
                : string.Empty;

            if (oldRoleName != manageUser.RoleName)
            {
                additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Roles", Value = RoleAssignMsg.Replace("RoleName", manageUser.RoleName) });
                if (!string.IsNullOrEmpty(oldRoleName))
                    additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Roles", Value = RoleRemovedMsg.Replace("RoleName", oldRoleName) });
            }

            string oldAssetName = string.Empty;
            if (!string.IsNullOrEmpty(userDetailsBeforeUpdate?.AssetID))
            {
                oldAssetName = assetType == "PORTFOLIO"
                    ? assetListResponse.Records.Cast<Portfolio>().ToList().Find(f => f.ID == userDetailsBeforeUpdate.AssetID)?.Name ?? string.Empty
                    : assetListResponse.Records.Cast<AssetGroup>().ToList().Find(f => f.AssetID == userDetailsBeforeUpdate.AssetID)?.Name ?? string.Empty;
            }
            if (oldAssetName != manageUser.AssetName)
            {
                additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Property Group", Value = PropAssignMsg.Replace("PropertyName", manageUser.AssetName) });
                if (!string.IsNullOrEmpty(oldAssetName))
                    additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Property Group", Value = PropRemovedMsg.Replace("PropertyName", oldAssetName) });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ManageOpsUserAsync - activity parameter build error for editorPersonaId {EditorPersonaId}", editorPersonaId);
        }

        return (string.Empty, additionalParameters);
    }

    // ── UpdateOPSUserProfileAsync ────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> UpdateOPSUserProfileAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError.ErrorReason;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        try
        {
            var persona         = await _managePersona.GetPersonaAsync(userPersonaId, withRights: false, ct);
            var person          = await _managePerson.GetPersonAsync(persona.RealPageId, ct);
            var userLogin       = await _manageUserLogin.GetUserLoginOnlyAsync(persona.RealPageId, ct);
            var userLoginPersonas = await _userLoginPersonaRepository.ListUserLoginPersonaAsync(
                null, persona.UserId, persona.Organization.PartyId, ct);
            var employeeId      = await _userRepository.GetUserEmployeeIdAsync(
                userLoginPersonas[0].UserLoginPersonaId, persona.OrganizationPartyId, ct);
            person.EmployeeId   = (employeeId is not null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : null;

            var contactMechanisms = await _manageContactMechanism.ListContactMechanismForPersonAsync(persona.RealPageId, null, ct);
            string userEmailAddress;

            if (persona.UserTypeId == (int)UserRoleType.UserNoEmail &&
                contactMechanisms.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true
                                        && a.contactMechanismUsageType?.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true))
            {
                userEmailAddress = contactMechanisms
                    .Where(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)
                              && a.contactMechanismUsageType.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.AddressString).FirstOrDefault() ?? string.Empty;
            }
            else if (contactMechanisms.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true
                                              && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true))
            {
                userEmailAddress = contactMechanisms
                    .Where(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)
                              && a.contactMechanismUsageType.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.AddressString).FirstOrDefault() ?? string.Empty;
            }
            else
            {
                userEmailAddress = userLogin.LoginName;
            }
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

            var userDetails = await _userRepository.GetUserDetailsAsync(personaId: persona.PersonaId, cancellationToken: ct);

            var patchUser = new OpsUserPatch
            {
                FirstName  = person.FirstName,
                MiddleName = person.MiddleName,
                LastName   = person.LastName,
                EmployeeId = person.EmployeeId,
                Loginname  = ctx.ProductUsername,
                Email      = userEmailAddress,
                Status     = userDetails.IsActive == true ? "active" : "inactive"
            };

            _logger.LogDebug("UpdateOPSUserProfileAsync - PATCH user {ProductUserId}", ctx.ProductUserId);
            string patchError = await PatchUserInfoAsync(session, ctx.ProductUserId, patchUser, ct);
            if (!string.IsNullOrEmpty(patchError)) return patchError;

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateOPSUserProfileAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return "There was a problem updating the user";
        }
    }

    // ── EnableUserAsync ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> EnableUserAsync(
        long editorPersonaId, long userPersonaId, bool isActive, bool deleteUser,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError.ErrorReason;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        var opsUser = await OpsGetUserByIdAsync(session, ctx.ProductUserId, ct);
        if (opsUser is null) return "There was an error getting the user details";

        opsUser.Email = null;
        opsUser.Phone = null;

        if (opsUser.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && isActive)   return string.Empty;
        if (opsUser.Status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) && !isActive) return string.Empty;

        opsUser.Status = isActive ? "active" : "inactive";

        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/users/{ctx.ProductUserId}";
            _logger.LogDebug("EnableUserAsync - PUT {Url}", url);

            using var req      = OpsRequest(HttpMethod.Put, url, session.Sid, opsUser);
            using var client   = CreateClient();
            using var response = await client.SendAsync(req, ct);

            if (response.IsSuccessStatusCode)
            {
                var statusType = isActive
                    ? (int)ProductBatchStatusType.Success
                    : (deleteUser ? (int)ProductBatchStatusType.Deleted : (int)ProductBatchStatusType.Inactive);

                await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, statusType, ct);
                return string.Empty;
            }

            string errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("EnableUserAsync - failed: {Error}", errorContent);
            return "There was a problem updating the user status";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EnableUserAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            return string.Empty; // matches legacy: catch swallows, returns ""
        }
    }

    // ── UnassignUserAsync ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, ct);
        if (ctxError is not null) return ctxError.ErrorReason;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        _logger.LogDebug("UnassignUserAsync - setting inactive for userPersonaId {UserPersonaId}", userPersonaId);

        string patchError = await PatchUserInfoAsync(session, ctx.ProductUserId, new OpsUserPatch { Status = "inactive" }, ct);
        if (!string.IsNullOrEmpty(patchError)) return patchError;

        await _settingService.UpdateProductStatusAsync(userPersonaId, ProductStatusSettingType, ProductId, (int)ProductBatchStatusType.Deleted, ct);
        return string.Empty;
    }

    // ── GetUsersAsync ────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) return ctxError;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        int    startRow     = 1;
        int    resultPerRow = 100;
        string filters      = string.Empty;

        if (datafilter is not null)
        {
            filters = string.Join("&", datafilter.FilterBy.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            if (filters.Length > 0) filters = "&" + filters;
            if (datafilter.Pages is not null)
            {
                startRow     = datafilter.Pages.StartRow;
                resultPerRow = Math.Min(datafilter.Pages.ResultsPerPage, 100);
            }
        }

        var response = new ListResponse { CurrentPage = startRow, RowsPerPage = resultPerRow, ErrorReason = "" };
        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/users?unify_login_status=all&page_number={startRow}&page_size={resultPerRow}{filters}";
            _logger.LogDebug("GetUsersAsync - GET {Url}", url);

            using var httpResponse = await OpsGetWithRetryAsync(settings, session, url, ct);
            if (httpResponse.IsSuccessStatusCode)
            {
                var users = JsonConvert.DeserializeObject<OpsUsers>(await httpResponse.Content.ReadAsStringAsync(ct))!;
                response.Records     = users.UserList.Cast<object>().ToList();
                response.TotalRows   = users.Pagination.TotalRecords;
                response.RowsPerPage = users.Pagination.PageSize;
                response.CurrentPage = users.Pagination.PageNumber;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsersAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            response.ErrorReason = ex.Message;
        }
        return response;
    }

    // ── ChangeUserStatusAsync ────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string userName, string productUserId, bool isActive = false,
        CancellationToken ct = default)
    {
        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) return false;

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        var opsUser = await OpsGetUserByIdAsync(session, productUserId, ct);
        if (opsUser is null) return false;

        opsUser.Email = null;
        opsUser.Phone = null;

        if (opsUser.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && isActive)   return true;
        if (opsUser.Status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) && !isActive) return true;

        opsUser.Status = isActive ? "active" : "inactive";

        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/users/{productUserId}";
            using var req      = OpsRequest(HttpMethod.Put, url, session.Sid, opsUser);
            using var client   = CreateClient();
            using var response = await client.SendAsync(req, ct);

            if (response.IsSuccessStatusCode) return true;

            string errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("ChangeUserStatusAsync - failed for user {UserName}: {Error}", userName, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangeUserStatusAsync - error for user {UserName}", userName);
            return false;
        }
    }

    // ── GetMigrationUsersAsync ───────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) { response.ErrorReason = ctxError.ErrorReason; return response; }

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        int    startRow     = 0;
        int    resultPerRow = 1000;
        string filter       = "inactive";
        string extras       = string.Empty;

        if (datafilter is not null)
        {
            if (datafilter.FilterBy.ContainsKey("filter"))
            {
                string filterValue = datafilter.FilterBy["filter"];
                if (filterValue.Equals("all", StringComparison.OrdinalIgnoreCase))
                    filter = "all";
                else
                    filter = filterValue.Equals("migrated", StringComparison.OrdinalIgnoreCase) ? "active" : "inactive";

                datafilter.FilterBy.Remove("filter");
            }
            extras = string.Join("&", datafilter.FilterBy.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            if (extras.Length > 0) extras = "&" + extras;
            if (datafilter.Pages is not null)
            {
                startRow     = datafilter.Pages.StartRow;
                resultPerRow = datafilter.Pages.ResultsPerPage;
            }
        }

        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/users?page_number={startRow}&page_size={resultPerRow}&unify_login_status={filter}{extras}";
            _logger.LogDebug("GetMigrationUsersAsync - GET {Url}", url);

            using var httpResponse = await OpsGetWithRetryAsync(settings, session, url, ct);
            if (!httpResponse.IsSuccessStatusCode) return response;

            var users = JsonConvert.DeserializeObject<OpsUsers>(await httpResponse.Content.ReadAsStringAsync(ct));
            if (users is null)
            {
                _logger.LogError("GetMigrationUsersAsync - null users response for editorPersonaId {EditorPersonaId}", editorPersonaId);
                return response;
            }

            var migrationUsers = users.UserList.Select(user =>
            {
                var mu = new MigrationUser
                {
                    UserId     = user.ID,
                    FirstName  = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName   = user.LastName,
                    Email      = user.Email,
                    Username   = user.Loginname,
                    Status     = user.Status?.ToLower() == "active" ? "Active" : "Disabled",
                    Phone      = user.Phone,
                    EmployeeId = user.EmployeeId
                };
                if (!string.IsNullOrWhiteSpace(user.AssetGroup?.ID))
                    mu.Properties.Add(new MigrationProperty { PropertyInstanceSourceId = user.AssetGroup.ID });
                return mu;
            }).ToList();

            response.Records     = migrationUsers.Cast<object>().ToList();
            response.ErrorReason = string.Empty;
            response.IsError     = false;
            response.TotalRows   = users.Pagination.TotalRecords;
            response.RowsPerPage = users.Pagination.PageSize;
            response.CurrentPage = users.Pagination.PageNumber;

            _logger.LogDebug("GetMigrationUsersAsync - {Count} users returned for editorPersonaId {EditorPersonaId}",
                migrationUsers.Count, editorPersonaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMigrationUsersAsync - error for editorPersonaId {EditorPersonaId}", editorPersonaId);
            response.ErrorReason = ex.Message;
        }
        return response;
    }

    // ── UpdateUsersMigrationStatusAsync ──────────────────────────────────────

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers,
        CancellationToken ct = default)
    {
        var migrateResponse = new MigrateResponse { Status = false };

        var (ctx, ctxError) = await _contextService.GetUserContextAsync(editorPersonaId, 0, ProductId, ct);
        if (ctxError is not null) { migrateResponse.Message = ctxError.ErrorReason; return migrateResponse; }

        var settings = await _settingService.GetProductSettingAsync(ProductId, ct);
        var session  = await GetOpsSessionAsync(settings, ctx!.EditorProductUsername, ct);

        var opsMigrateUsers = migrateUsers.Select(x => new OpsMigrateUser
        {
            UserId               = x.UserId,
            UnifiedLoginUserName = x.UnifiedLoginUserName,
            UsingUnifiedLogin    = x.UsingUnifiedLogin ? 1 : 0
        });

        string url = $"{session.BaseUrl}/api/v1.0/users";
        _logger.LogDebug("UpdateUsersMigrationStatusAsync - PATCH {Url}", url);

        using var req = new HttpRequestMessage(new HttpMethod("PATCH"), url);
        req.Headers.TryAddWithoutValidation("sid", session.Sid);
        req.Content = new StringContent(JsonConvert.SerializeObject(opsMigrateUsers), Encoding.Default, "application/json");

        using var client   = CreateClient();
        using var response = await client.SendAsync(req, ct);
        string responseContent = await response.Content.ReadAsStringAsync(ct);

        if (response.IsSuccessStatusCode)
        {
            var parsed = JsonConvert.DeserializeObject<MigrateResponse>(responseContent);
            migrateResponse.Message = parsed?.Message ?? string.Empty;
            migrateResponse.Status  = parsed?.Status ?? false;
            _logger.LogDebug("UpdateUsersMigrationStatusAsync - success for editorPersonaId {EditorPersonaId}", editorPersonaId);
        }
        else
        {
            _logger.LogError("UpdateUsersMigrationStatusAsync - failed for editorPersonaId {EditorPersonaId}: {Error}", editorPersonaId, responseContent);
            migrateResponse.Message = "Cannot update user status to migrated.";
        }
        return migrateResponse;
    }

    // ── OPS Session Management ────────────────────────────────────────────────

    /// <summary>
    /// Immutable value holding the Ops API base URL, session ID (sid), and the login name
    /// used to acquire it — the login name is needed to invalidate the cache on 401.
    /// </summary>
    private readonly record struct OpsSession(string BaseUrl, string Sid, string LoginName);

    /// <summary>
    /// Returns a valid Ops session SID, using a <see cref="SidCacheMinutes"/>-minute
    /// <see cref="ICacheService"/> entry to avoid re-authenticating on every call.
    /// <para>
    /// Cache key <c>opsSid_{loginName}</c> matches the legacy <c>MemoryCache.Default</c>
    /// key so both code paths share the cached SID during any rolling migration window.
    /// </para>
    /// <para>
    /// If the login name is empty (test/stub environment) or APIKEY is missing,
    /// returns an empty-SID session rather than throwing.
    /// </para>
    /// </summary>
    private async Task<OpsSession> GetOpsSessionAsync(
        IList<ProductInternalSetting> settings, string loginName, CancellationToken ct)
    {
        string baseUrl = SettingValue(settings, "APIENDPOINT");

        if (string.IsNullOrEmpty(loginName))
        {
            _logger.LogDebug("GetOpsSessionAsync - loginName is empty, skipping auth");
            return new OpsSession(baseUrl, string.Empty, loginName);
        }

        string apiKey = SettingValue(settings, "APIKEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("GetOpsSessionAsync - APIKEY is not configured");
            return new OpsSession(baseUrl, string.Empty, loginName);
        }

        string cacheKey = $"{CacheKeySidPrefix}{loginName}";

        string? sid = await _cache.GetOrSetAsync<string>(
            cacheKey,
            token => AcquireOpsSessionSidAsync(settings, loginName, apiKey, baseUrl, token),
            new CacheEntryOptions { ExpirationTimeInMinutes = SidCacheMinutes, SkipDistributedCache = true },
            ct);

        return new OpsSession(baseUrl, sid ?? string.Empty, loginName);
    }

    /// <summary>
    /// Performs the actual HTTP POST to <c>/api/v1.0/sessions</c> and returns the SID,
    /// or <c>null</c> when all attempts fail. Called exclusively by <see cref="GetOpsSessionAsync"/>
    /// as the <c>ICacheService.GetOrSetAsync</c> factory.
    /// </summary>
    private async Task<string?> AcquireOpsSessionSidAsync(
        IList<ProductInternalSetting> settings, string loginName, string apiKey,
        string baseUrl, CancellationToken ct)
    {
        string key = Encoding.UTF8.GetString(Convert.FromBase64String(apiKey));
        string trustKey;
        using (var md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(loginName + key));
            trustKey = BitConverter.ToString(hash).ToLower().Replace("-", "");
        }

        var sessionRequest = new SessionRequest { Login_name = loginName, Trust_key = trustKey };
        string sessionUrl  = $"{baseUrl.TrimEnd('/')}/api/v1.0/sessions";

        _logger.LogDebug("AcquireOpsSessionSidAsync - requesting new session for {LoginName}", loginName);

        for (int attempt = 0; attempt < MaxRetryCount; attempt++)
        {
            try
            {
                using var client = CreateClient();
                using var req    = new HttpRequestMessage(HttpMethod.Post, sessionUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(sessionRequest), Encoding.Default, "application/json")
                };
                using var response = await client.SendAsync(req, ct);

                if (response.IsSuccessStatusCode)
                {
                    dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync(ct));
                    string? sid     = (string?)result?.session?.sid;

                    if (string.IsNullOrEmpty(sid))
                    {
                        _logger.LogError("AcquireOpsSessionSidAsync - response missing sid for {LoginName}", loginName);
                        return null;
                    }

                    _logger.LogDebug("AcquireOpsSessionSidAsync - session acquired for {LoginName}", loginName);
                    return sid;
                }

                string errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("AcquireOpsSessionSidAsync - attempt {Attempt} failed ({Status}): {Error}",
                    attempt + 1, response.StatusCode, errorBody);
            }
            catch (Exception ex) when (attempt < MaxRetryCount - 1)
            {
                _logger.LogWarning(ex, "AcquireOpsSessionSidAsync - attempt {Attempt} exception, retrying", attempt + 1);
            }
        }

        _logger.LogError("AcquireOpsSessionSidAsync - could not obtain session for {LoginName}", loginName);
        return null;
    }

    // ── HTTP Helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Issues an authenticated GET to the Ops API with automatic retry on HTTP 401
    /// (session expiry): clears the SID cache and re-acquires up to
    /// <see cref="MaxRetryCount"/> times, matching the legacy <c>GetAsync</c> behaviour.
    /// </summary>
    private async Task<HttpResponseMessage> OpsGetWithRetryAsync(
        IList<ProductInternalSetting> settings, OpsSession session, string url, CancellationToken ct)
    {
        int attempt = 0;
        while (true)
        {
            using var req    = OpsRequest(HttpMethod.Get, url, session.Sid, body: null);
            using var client = CreateClient();
            var response     = await client.SendAsync(req, ct);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                return response;

            if (response.StatusCode == HttpStatusCode.Unauthorized && attempt < MaxRetryCount)
            {
                // Refresh session and retry
                await _cache.RemoveAsync($"{CacheKeySidPrefix}{session.LoginName}", ct);
                session = await GetOpsSessionAsync(settings, session.LoginName, ct);
                attempt++;
                continue;
            }

            // Non-recoverable error (or retry limit reached)
            string errorBody = await response.Content.ReadAsStringAsync(ct);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                throw new InvalidOperationException($"GET {url} failed ({(int)response.StatusCode}): {errorBody}");

            // MaxRetryCount reached on 401
            throw new InvalidOperationException($"GET {url} failed after {MaxRetryCount} session-refresh attempts.");
        }
    }

    /// <summary>
    /// Builds a new <see cref="HttpRequestMessage"/> with the Ops SID header
    /// and optional JSON body.
    /// </summary>
    private static HttpRequestMessage OpsRequest(HttpMethod method, string url, string sid, object? body)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.TryAddWithoutValidation("sid", sid);
        if (body is not null)
            req.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.Default, "application/json");
        return req;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);
        return client;
    }

    // ── Private shared helpers ────────────────────────────────────────────────

    /// <summary>
    /// Fetches <c>MODULE_ASSET_GROUPS</c> from the Ops company config endpoint.
    /// Returns 0 when the company is not configured for asset groups.
    /// </summary>
    private async Task<int> GetModuleAssetGroupsConfigAsync(OpsSession session, CancellationToken ct)
    {
        string url = $"{session.BaseUrl}/api/v1.0/company/configs?config_name=module_asset_groups";
        using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
        using var client   = CreateClient();
        using var response = await client.SendAsync(req, ct);
        if (!response.IsSuccessStatusCode) return 0;

        dynamic? config = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync(ct));
        return Convert.ToInt16(config?.MODULE_ASSET_GROUPS ?? 0);
    }

    private async Task<OpsUser?> OpsGetUserByIdAsync(OpsSession session, string productUserId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(productUserId)) return null;

        string url = $"{session.BaseUrl}/api/v1.0/users/{productUserId}";
        using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
        using var client   = CreateClient();
        using var response = await client.SendAsync(req, ct);
        if (!response.IsSuccessStatusCode) return null;

        dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync(ct));
        if (result is null) return null;

        return new OpsUser
        {
            ID         = result.id,
            FirstName  = result.first_name,
            MiddleName = result.middle_name,
            LastName   = result.last_name,
            Loginname  = result.login_name,
            AssetID    = result.asset?.id,
            UserTypeId = result.user_type?.id,
            Status     = result.status
        };
    }

    private async Task<bool> OpsLoginNameInUseAsync(OpsSession session, string loginName, CancellationToken ct)
    {
        string url = $"{session.BaseUrl}/api/v1.0/users/0/?login_name={loginName}";
        using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
        using var client   = CreateClient();
        using var response = await client.SendAsync(req, ct);
        if (!response.IsSuccessStatusCode) return false;

        dynamic? user = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync(ct));
        string? foundLogin = user?.login_name;
        return string.Equals(foundLogin, loginName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Sends a PATCH to <c>/api/v1.0/users/{userId}</c>.
    /// Returns an empty string on success, or the error message on failure.
    /// </summary>
    private async Task<string> PatchUserInfoAsync(OpsSession session, string userId, OpsUserPatch userPatch, CancellationToken ct)
    {
        string url = $"{session.BaseUrl}/api/v1.0/users/{userId}";
        using var req      = OpsRequest(new HttpMethod("PATCH"), url, session.Sid, userPatch);
        using var client   = CreateClient();
        using var response = await client.SendAsync(req, ct);

        if (response.IsSuccessStatusCode) return string.Empty;

        string error = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("PatchUserInfoAsync - failed for userId {UserId}: {Error}", userId, error);
        return error;
    }

    private async Task<ListResponse> GetAllRolesAsync(OpsSession session, CancellationToken ct)
    {
        var response = new ListResponse();
        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/roles";
            using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
            using var client   = CreateClient();
            using var httpResp = await client.SendAsync(req, ct);

            if (httpResp.IsSuccessStatusCode)
            {
                IList<Role> rolesList = JsonConvert.DeserializeObject<IList<Role>>(await httpResp.Content.ReadAsStringAsync(ct)) ?? new List<Role>();
                response = new ListResponse { Records = rolesList.Cast<object>().ToList(), TotalRows = rolesList.Count, RowsPerPage = rolesList.Count, TotalPages = 1, ErrorReason = "" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllRolesAsync - error");
            response.IsError     = true;
            response.ErrorReason = "There was a problem getting the roles";
        }
        return response;
    }

    /// <summary>
    /// Resolves the Ops roles for the company, optionally scoped to an asset code,
    /// and marks the assigned role when an existing product user is provided.
    /// </summary>
    private async Task<ListResponse> GetRolesInternalAsync(
        OpsSession session, ProductCallContext ctx,
        long userPersonaId, string? assetCode, CancellationToken ct)
    {
        string assetCodeParam = string.Empty;

        if (!string.IsNullOrEmpty(assetCode))
        {
            // Only append ?asset_code= when the code exists in the company's asset list
            var assetListResp = await GetCompanyAssetDetailsAsync(session, ctx, 0, includeDisabled: true, updateAssetNames: true, buildHierarchy: false, ct);
            if (!assetListResp.IsError)
            {
                string assetType = (assetListResp.Additional as string ?? string.Empty).ToUpper();
                bool codeExists = assetType == "PORTFOLIO"
                    ? assetListResp.Records.Cast<Portfolio>().Any(m => m.Code?.ToUpper() == assetCode.ToUpper())
                    : assetListResp.Records.Cast<AssetGroup>().Any(m => m.Code?.ToUpper() == assetCode.ToUpper());

                if (codeExists) assetCodeParam = $"?asset_code={assetCode}";
            }
        }

        var response = new ListResponse();
        try
        {
            string url = $"{session.BaseUrl}/api/v1.0/roles{assetCodeParam}";
            using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
            using var client   = CreateClient();
            using var httpResp = await client.SendAsync(req, ct);

            if (httpResp.IsSuccessStatusCode)
            {
                IList<Role> rolesList = JsonConvert.DeserializeObject<IList<Role>>(await httpResp.Content.ReadAsStringAsync(ct)) ?? new List<Role>();
                IList<ProductRole> productRoles = rolesList.ToGBRoles() ?? new List<ProductRole>();

                if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
                {
                    var opsUser = await OpsGetUserByIdAsync(session, ctx.ProductUserId, ct);
                    if (opsUser is not null && !string.IsNullOrEmpty(opsUser.UserTypeId))
                    {
                        var pr = productRoles.FirstOrDefault(a => a.ID == opsUser.UserTypeId);
                        if (pr is not null) pr.IsAssigned = true;
                    }
                }

                response = new ListResponse { Records = productRoles.Cast<object>().ToList(), TotalRows = productRoles.Count, RowsPerPage = productRoles.Count, TotalPages = 1, ErrorReason = "" };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesInternalAsync - error");
            response.IsError     = true;
            response.ErrorReason = "There was a problem getting the roles";
        }
        return response;
    }

    /// <summary>
    /// Returns the company's portfolio or asset-group list, replicating the
    /// legacy <c>GetCompanyAssetDetails</c> private method.
    /// </summary>
    private async Task<ListResponse> GetCompanyAssetDetailsAsync(
        OpsSession session, ProductCallContext ctx,
        long userPersonaId, bool includeDisabled, bool updateAssetNames, bool buildHierarchy,
        CancellationToken ct)
    {
        try
        {
            int moduleAssetGroups = await GetModuleAssetGroupsConfigAsync(session, ct);

            if (moduleAssetGroups == 1)
            {
                // Asset-group mode
                string url = $"{session.BaseUrl}/api/v1.0/assets/groups";
                using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
                using var client   = CreateClient();
                using var response = await client.SendAsync(req, ct);
                if (!response.IsSuccessStatusCode)
                    return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage };

                var assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(await response.Content.ReadAsStringAsync(ct)) ?? new List<AssetGroup>();

                if (!includeDisabled)
                    assetGroups = assetGroups.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                if (updateAssetNames)
                    assetGroups.ForEach(ag => ag.Name = ag.GroupType.ToUpper() == "PROPERTY" ? "[A] " + ag.Name : "[G] " + ag.Name);

                if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
                {
                    var opsUser = await OpsGetUserByIdAsync(session, ctx.ProductUserId, ct);
                    if (opsUser is not null && !string.IsNullOrEmpty(opsUser.AssetID))
                    {
                        var ag = assetGroups.FirstOrDefault(a => a.AssetID == opsUser.AssetID);
                        if (ag is not null) ag.IsAssigned = true;
                    }
                }

                return new ListResponse { Records = assetGroups.Cast<object>().ToList(), TotalRows = assetGroups.Count, RowsPerPage = assetGroups.Count, TotalPages = 1, ErrorReason = "", Additional = "AssetGroups" };
            }
            else
            {
                // Portfolio mode
                string url = $"{session.BaseUrl}/api/v1.0/assets/portfolio";
                using var req      = OpsRequest(HttpMethod.Get, url, session.Sid, null);
                using var client   = CreateClient();
                using var response = await client.SendAsync(req, ct);

                var portfolioList = JsonConvert.DeserializeObject<List<Portfolio>>(await response.Content.ReadAsStringAsync(ct)) ?? new List<Portfolio>();
                portfolioList = portfolioList.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                int totalRows = portfolioList.Count;

                if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
                {
                    var opsUser = await OpsGetUserByIdAsync(session, ctx.ProductUserId, ct);
                    if (opsUser is not null && !string.IsNullOrEmpty(opsUser.AssetID))
                    {
                        var p = portfolioList.FirstOrDefault(a => a.ID == opsUser.AssetID);
                        if (p is not null) p.IsAssigned = true;
                    }
                }

                if (buildHierarchy)
                {
                    var parents = portfolioList.Where(m => string.IsNullOrEmpty(m.ParentAssetId)).ToList();
                    foreach (var p in parents) p.BuildTree(portfolioList);
                    return new ListResponse { Records = parents.Cast<object>().ToList(), TotalRows = totalRows, RowsPerPage = totalRows, TotalPages = 1, ErrorReason = "", Additional = "Portfolio" };
                }

                return new ListResponse { Records = portfolioList.Cast<object>().ToList(), TotalRows = totalRows, RowsPerPage = totalRows, TotalPages = 1, ErrorReason = "", Additional = "Portfolio" };
            }
        }
        catch
        {
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage };
        }
    }

    private async Task<bool> IsSuperUserAsync(Persona userPersona, CancellationToken ct)
    {
        var relationship = await _managePartyRelationship.GetPartyRelationshipAsync(
            userPersona.RealPageId, userPersona.Organization.RealPageId,
            roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type", ct);

        return relationship is not null
            && relationship.RoleTypeFrom.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase);
    }

    private static List<MainGroup> EnableComplianceRights(List<MainGroup> list)
    {
        foreach (var item in list)
        {
            if (item.mainName == "Compliance Setup")
                foreach (var sub in item.subGroupList)
                    foreach (var right in sub.rightsList)
                    { right.isAssigned = true; right.value = "1"; }
        }
        return list;
    }

    /// <summary>
    /// Validates an email address string and returns it if valid, or the original
    /// string unchanged when it cannot be parsed as a <see cref="System.Net.Mail.MailAddress"/>.
    /// Mirrors the legacy <c>ManageProductBase.ValidateAndReturnEmailAddress</c> guard.
    /// </summary>
    private static string ValidateAndReturnEmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return email;
        try
        {
            _ = new System.Net.Mail.MailAddress(email);
            return email;
        }
        catch
        {
            return email;
        }
    }

    private static string SettingValue(IList<ProductInternalSetting> settings, string name)
        => settings.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value
           ?? string.Empty;
}
