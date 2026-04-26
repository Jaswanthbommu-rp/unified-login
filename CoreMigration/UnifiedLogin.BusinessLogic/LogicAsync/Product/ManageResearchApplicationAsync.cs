using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResearchApplication;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// True-async implementation of Research Application (Black Book) user management.
/// <para>
/// Replaces <c>ManageResearchApplication</c> (sync stepping-stone).
/// No <c>DefaultUserClaim</c>, no mutable instance fields, no blocking <c>.Result</c> calls.
/// </para>
/// </summary>
public sealed class ManageResearchApplicationAsync : IManageResearchApplicationAsync
{
    #region Constants

    private const int ProductId              = (int)ProductEnum.ResearchApplication; // 24
    private const int UnifiedPlatformId      = (int)ProductEnum.UnifiedPlatform;     // 3
    private const string HttpClientName      = "ResearchApplication";
    private const string TokenClientName     = "ResearchApplicationToken";
    private const string ConfigCacheKey      = "RA_Config";
    private const string TokenCacheKey       = "RA_AccessToken";
    private const string ResearchAppClientId = "UnifiedLoginResearchApp";            // client_id and scope
    private const string DirectorRoleName    = "BLACK-BOOK DIRECTOR";

    private static readonly TimeSpan ConfigCacheTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan TokenCacheTtl  = TimeSpan.FromMinutes(9);

    #endregion

    #region Dependencies

    private readonly IProductContextServiceAsync  _contextService;
    private readonly IProductRepositoryAsync      _productRepository;
    private readonly IUnifiedLoginRepositoryAsync _unifiedLoginRepository;
    private readonly IManageUserRoleRightAsync    _userRoleRight;
    private readonly IManageUserLoginAsync        _userLogin;
    private readonly IHttpClientFactory           _httpClientFactory;
    private readonly IMemoryCache                 _cache;
    private readonly IConfiguration               _configuration;
    private readonly ILogger<ManageResearchApplicationAsync> _logger;

    #endregion

    #region Constructor

    public ManageResearchApplicationAsync(
        IProductContextServiceAsync  contextService,
        IProductRepositoryAsync      productRepository,
        IUnifiedLoginRepositoryAsync unifiedLoginRepository,
        IManageUserRoleRightAsync    userRoleRight,
        IManageUserLoginAsync        userLogin,
        IHttpClientFactory           httpClientFactory,
        IMemoryCache                 cache,
        IConfiguration               configuration,
        ILogger<ManageResearchApplicationAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(unifiedLoginRepository);
        ArgumentNullException.ThrowIfNull(userRoleRight);
        ArgumentNullException.ThrowIfNull(userLogin);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        _contextService         = contextService;
        _productRepository      = productRepository;
        _unifiedLoginRepository = unifiedLoginRepository;
        _userRoleRight          = userRoleRight;
        _userLogin              = userLogin;
        _httpClientFactory      = httpClientFactory;
        _cache                  = cache;
        _configuration          = configuration;
        _logger                 = logger;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            // GetProductIdsByOrg in the original was dead code — partyId param is authoritative
            var productIdList = await _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken);
            var allRoles      = await _productRepository.ListRolesForProductByPartyAsync(
                partyId, productIdList, ProductId, cancellationToken);

            if (userPersonaId != 0)
                return await MergeSelRolesWithGreenbookAsync(allRoles, userPersonaId, cancellationToken);

            return new ListResponse
            {
                Records      = allRoles.Cast<object>().ToList(),
                TotalRows    = allRoles.Count,
                RowsPerPage  = 9999,
                ErrorReason  = string.Empty,
                TotalPages   = 1
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{ActionName} - {state}", "GetRolesAsync",
                $"Error. editorPersonaId - {editorPersonaId}");
            return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.RoleErrorMessage };
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId,
        long partyId,
        long roleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (_, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, editorPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var productIdList = await _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken);
            var allRights     = await _unifiedLoginRepository.ListRightsByRoleAsync(
                partyId, productIdList, ProductId, roleId, cancellationToken);

            return new ListResponse
            {
                Records     = allRights.Cast<object>().ToList(),
                TotalRows   = allRights.Count,
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages  = 1
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{ActionName} - {state}", "GetRightsByRoleAsync",
                $"Error. editorPersonaId - {editorPersonaId}, roleId - {roleId}");
            return new ListResponse
            {
                IsError     = true,
                ErrorReason = "ResearchApplication - There was a problem getting the roles."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<string> ManageResearchApplicationUserAsync(
        long editorPersonaId,
        long userPersonaId,
        ResearchAppRoleAndPropertyList userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error.ErrorReason ?? string.Empty;

            // Parallel fetch: user login + editor login (for userId audit param)
            var userLoginTask   = _userLogin.GetUserLoginOnlyAsync(ctx!.UserPersona!.RealPageId, cancellationToken);
            var editorLoginTask = _userLogin.GetUserLoginOnlyAsync(ctx.EditorPersona.RealPageId, cancellationToken);
            await Task.WhenAll(userLoginTask, editorLoginTask);

            var userLogin   = await userLoginTask;
            var editorLogin = await editorLoginTask;
            int editorUserId = (int)(editorLogin?.UserId ?? 0L);

            // Super user override: assign BLACK-BOOK DIRECTOR role automatically
            bool isSuperUser = await _contextService.IsSuperUserAsync(ctx.UserPersona!, cancellationToken);
            if (isSuperUser)
            {
                _logger.LogDebug("{ActionName} - {state}", "ManageResearchApplicationUserAsync",
                    $"New user is Super user. editorPersonaId - {editorPersonaId}");

                long orgPartyId   = ctx.UserPersona!.Organization.PartyId;
                var  productIdList = await _productRepository.GetProductIdsByCompanyAsync(orgPartyId, cancellationToken);
                var  allRoles      = await _productRepository.ListRolesForProductByPartyAsync(
                    orgPartyId, productIdList, ProductId, cancellationToken);

                var directorRole = allRoles.FirstOrDefault(r =>
                    r.Name.Equals(DirectorRoleName, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"{DirectorRoleName} role not found for partyId {orgPartyId}");

                userAssignProductPropertyRole = new ResearchAppRoleAndPropertyList
                {
                    PropertyList = ["-1"],
                    RoleList     = [directorRole.ID]
                };
            }

            // Resolve role from request
            UL.Role role = new();
            if (userAssignProductPropertyRole?.RoleList is { Count: > 0 } roleList)
            {
                var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);
                if (productPropertyRole.RoleList?.Count > 0 &&
                    long.TryParse(productPropertyRole.RoleList[0], out long parsedRoleId))
                {
                    role.RoleID = parsedRoleId;
                }
            }

            var assignedRoles = await GetAssignedRoleForPersonaAsync(userPersonaId, cancellationToken);

            if (assignedRoles is null or { Count: 0 }) // New user — insert only
            {
                _logger.LogDebug("{ActionName} - {state}", "ManageResearchApplicationUserAsync",
                    $"New user — inserting role. userPersonaId - {userPersonaId}, roleId - {role.RoleID}");

                var result = await _userRoleRight.InsertAssignedRoleToUserAsync(
                    userPersonaId, role.RoleID, editorUserId, deleteRole: false, cancellationToken);
                if (result.Id < 0)
                {
                    _logger.LogError("{ActionName} - {state}", "ManageResearchApplicationUserAsync",
                        $"InsertAssignedRoleToUser failed. userPersonaId - {userPersonaId}, roleId - {role.RoleID}");
                }
            }
            else // Existing user — swap roles then notify via event API
            {
                long existingRoleId = assignedRoles[0].RoleID;
                await UpdateBlackBookRoleAsync(
                    userPersonaId, editorUserId, role.RoleID, existingRoleId, cancellationToken);

                _logger.LogDebug("{ActionName} - {state}", "ManageResearchApplicationUserAsync",
                    $"Sending user-updated event. userPersonaId - {userPersonaId}");

                var config = await GetConfigAsync(cancellationToken);
                var token  = await GetAccessTokenAsync(config, cancellationToken);

                using var client = _httpClientFactory.CreateClient(HttpClientName);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var req = new HttpRequestMessage(HttpMethod.Post,
                    $"{config.ApiEndpoint.TrimEnd('/')}/event-api/user-updated/{userPersonaId}")
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };

                var postResponse = await client.SendAsync(req, cancellationToken);
                if (postResponse.IsSuccessStatusCode)
                {
                    var body = await postResponse.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogDebug("{ActionName} - {state}", "ManageResearchApplicationUserAsync",
                        $"user-updated response: {body}. userPersonaId - {userPersonaId}");
                }
                else
                {
                    _logger.LogWarning("{ActionName} - {state}", "ManageResearchApplicationUserAsync",
                        $"user-updated returned {(int)postResponse.StatusCode}. userPersonaId - {userPersonaId}");
                }
            }

            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Success, cancellationToken);

            return string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{ActionName} - {state}", "ManageResearchApplicationUserAsync",
                $"Error. editorPersonaId - {editorPersonaId}, userPersonaId - {userPersonaId}");
            return $"Error - {ex.Message}";
        }
    }

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        ResearchAppRoleAndPropertyList userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error.ErrorReason ?? string.Empty;

            // Parallel fetch: user login + editor login
            var userLoginTask   = _userLogin.GetUserLoginOnlyAsync(ctx!.UserPersona!.RealPageId, cancellationToken);
            var editorLoginTask = _userLogin.GetUserLoginOnlyAsync(ctx.EditorPersona.RealPageId, cancellationToken);
            await Task.WhenAll(userLoginTask, editorLoginTask);

            var userLogin    = await userLoginTask;
            var editorLogin  = await editorLoginTask;
            int editorUserId = (int)(editorLogin?.UserId ?? 0L);

            // Remove existing role from GB DB
            var roleList = await GetAssignedRoleForPersonaAsync(userPersonaId, cancellationToken);
            if (roleList?.Count > 0)
            {
                _logger.LogDebug("{ActionName} - {state}", "UnassignUserAsync",
                    $"Deleting role. editorPersonaId - {editorPersonaId}, userPersonaId - {userPersonaId}, roleId - {roleList[0].RoleID}");

                var result = await _userRoleRight.InsertAssignedRoleToUserAsync(
                    userPersonaId, roleList[0].RoleID, editorUserId, deleteRole: true, cancellationToken);
                if (result.Id < 0)
                {
                    _logger.LogError("{ActionName} - {state}", "UnassignUserAsync",
                        $"DeleteRole failed. userPersonaId - {userPersonaId}, roleId - {roleList[0].RoleID}");
                }
            }

            // Set product status to Deleted
            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Deleted, cancellationToken);

            // Notify Research Application via event API
            var config = await GetConfigAsync(cancellationToken);
            var token  = await GetAccessTokenAsync(config, cancellationToken);

            var deleteBody = new DataObject<ResearchApplicationDeleteUser>
            {
                data = new ResearchApplicationDeleteUser
                {
                    UserId    = userLogin.UserId,
                    PersonaId = userPersonaId
                }
            };

            using var client = _httpClientFactory.CreateClient(HttpClientName);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{config.ApiEndpoint.TrimEnd('/')}/event-api/user-deleted")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(deleteBody), Encoding.UTF8, "application/json")
            };

            var postResponse = await client.SendAsync(req, cancellationToken);
            if (postResponse.IsSuccessStatusCode)
            {
                var body = await postResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("{ActionName} - {state}", "UnassignUserAsync",
                    $"user-deleted response: {body}. userPersonaId - {userPersonaId}");
            }
            else
            {
                _logger.LogWarning("{ActionName} - {state}", "UnassignUserAsync",
                    $"user-deleted returned {(int)postResponse.StatusCode}. userPersonaId - {userPersonaId}");
            }

            return string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{ActionName} - {state}", "UnassignUserAsync",
                $"Error. editorPersonaId - {editorPersonaId}, userPersonaId - {userPersonaId}");
            return $"Error - {ex.Message}";
        }
    }

    #endregion

    #region Private Helpers

    /// <summary>Loads and caches product settings for ResearchApplication and UnifiedPlatform.</summary>
    private async ValueTask<ResearchAppConfig> GetConfigAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ConfigCacheKey, out ResearchAppConfig? cached))
            return cached!;

        // Parallel: ResearchApplication settings + UnifiedPlatform settings
        var raSettingsTask = _productRepository.GetProductInternalSettingsAsync(ProductId, ct);
        var ulSettingsTask = _productRepository.GetProductInternalSettingsAsync(UnifiedPlatformId, ct);
        await Task.WhenAll(raSettingsTask, ulSettingsTask);

        var raSettings = await raSettingsTask;
        var ulSettings = await ulSettingsTask;

        string apiEndpoint = raSettings
            .First(s => s.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
        string productUrl = raSettings
            .First(s => s.Name.Equals("PRODUCTURL", StringComparison.OrdinalIgnoreCase)).Value;

        // Secret is base-64-encoded in the UnifiedPlatform settings
        string rawSecret = ulSettings
            .First(s => s.Name.Equals("UNIFIEDLOGINRESEARCHAPPLICATIONCLIENTSECRET",
                StringComparison.OrdinalIgnoreCase)).Value;
        string clientSecret = Encoding.UTF8.GetString(Convert.FromBase64String(rawSecret));

        string issuerUri = _configuration["UnifiedPlatform:Authority"]
            ?? throw new InvalidOperationException("UnifiedPlatform:Authority is not configured");
        string tokenEndpoint = $"{issuerUri.TrimEnd('/')}/connect/token";

        var config = new ResearchAppConfig(apiEndpoint, productUrl, clientSecret, tokenEndpoint);
        _cache.Set(ConfigCacheKey, config, ConfigCacheTtl);
        return config;
    }

    /// <summary>Obtains a client_credentials token, cached for 9 minutes.</summary>
    private async ValueTask<string> GetAccessTokenAsync(ResearchAppConfig config, CancellationToken ct)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrEmpty(cached))
            return cached!;

        using var tokenClient = _httpClientFactory.CreateClient(TokenClientName);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"]    = "client_credentials",
            ["client_id"]     = ResearchAppClientId,
            ["client_secret"] = config.ClientSecret,
            ["scope"]         = ResearchAppClientId      // scope == client_id, same as original
        });

        var response = await tokenClient.PostAsync(config.TokenEndpoint, form, ct);
        response.EnsureSuccessStatusCode();

        var json    = await response.Content.ReadAsStringAsync(ct);
        var payload = JsonConvert.DeserializeObject<dynamic>(json);
        string token = (string?)payload?.access_token
            ?? throw new InvalidOperationException(
                "ResearchApplication token endpoint returned no access_token");

        _cache.Set(TokenCacheKey, token, TokenCacheTtl);
        return token;
    }

    /// <summary>
    /// Async equivalent of <c>ManageProductBase.GetAssignedRoleForPersona</c>
    /// scoped to <see cref="ProductEnum.ResearchApplication"/>.
    /// </summary>
    private async Task<List<UL.Role>> GetAssignedRoleForPersonaAsync(
        long userPersonaId, CancellationToken ct)
    {
        var roles = await _userRoleRight.GetAssignedRoleForPersonaAsync(
            ProductEnum.ResearchApplication,
            userPersonaId: userPersonaId,
            cancellationToken: ct);
        return roles?.ToList() ?? [];
    }

    /// <summary>
    /// Merges the full role list from GB DB with the user's currently assigned roles,
    /// marking assigned ones. Dictionary lookup replaces O(n²) LINQ search.
    /// </summary>
    private async Task<ListResponse> MergeSelRolesWithGreenbookAsync(
        IList<ProductRole> allRoles, long userPersonaId, CancellationToken ct)
    {
        _logger.LogDebug("{ActionName} - {state}", "MergeSelRolesWithGreenbookAsync",
            $"Getting assigned roles for userPersonaId - {userPersonaId}");

        var assignedRoles = await GetAssignedRoleForPersonaAsync(userPersonaId, ct);

        if (assignedRoles.Count > 0)
        {
            var roleById = allRoles.ToDictionary(r => r.ID);
            foreach (var role in assignedRoles)
            {
                if (roleById.TryGetValue(role.RoleID.ToString(), out var selRole))
                    selRole.IsAssigned = true;
            }
        }

        return new ListResponse
        {
            Records     = allRoles.Cast<object>().ToList(),
            TotalRows   = allRoles.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1
        };
    }

    /// <summary>Swaps a user's Research Application role: deletes old, inserts new.</summary>
    private async Task UpdateBlackBookRoleAsync(
        long userPersonaId, int editorUserId, long newRoleId, long existingRoleId, CancellationToken ct)
    {
        _logger.LogDebug("{ActionName} - {state}", "UpdateBlackBookRoleAsync",
            $"Swapping role: userPersonaId - {userPersonaId}, old - {existingRoleId}, new - {newRoleId}");

        // Delete existing role, then insert new role
        await _userRoleRight.InsertAssignedRoleToUserAsync(
            userPersonaId, existingRoleId, editorUserId, deleteRole: true, ct);

        await _userRoleRight.InsertAssignedRoleToUserAsync(
            userPersonaId, newRoleId, editorUserId, deleteRole: false, ct);
    }

    /// <summary>
    /// Maps <see cref="ResearchAppRoleAndPropertyList"/> to <see cref="UserAssignProductPropertyRole"/>.
    /// Only the <c>RoleList</c> is relevant for Research Application.
    /// </summary>
    private static UserAssignProductPropertyRole MapGbObjectToProduct(
        ResearchAppRoleAndPropertyList source)
    {
        var result = new UserAssignProductPropertyRole();
        if (source.RoleList is { Count: > 0 })
            result.RoleList = [.. source.RoleList];
        return result;
    }

    #endregion
}

/// <summary>
/// Immutable config record for ResearchApplication; cached 1 hour in <see cref="IMemoryCache"/>.
/// </summary>
internal sealed record ResearchAppConfig(
    string ApiEndpoint,
    string ProductUrl,
    string ClientSecret,
    string TokenEndpoint);

/// <summary>
/// Request body for the Research Application user-deleted event API.
/// Replaces the private nested class in the original sync implementation.
/// </summary>
internal sealed record ResearchApplicationDeleteUser
{
    [Newtonsoft.Json.JsonProperty("UserId")]
    public long UserId { get; init; }

    [Newtonsoft.Json.JsonProperty("PersonaId")]
    public long PersonaId { get; init; }
}
