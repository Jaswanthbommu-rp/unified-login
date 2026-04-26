using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.VendorServices;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native-async implementation of <see cref="IManageProductVendorServicesAsync"/> for
/// Vendor Credentialing (Compliance Depot) user management.
/// <para>
/// Thread-safe: no mutable instance fields.  Bearer token is cached in
/// <see cref="IMemoryCache"/> (9-minute TTL matching VC token lifetime).
/// All three property-group types (Divisions, Regions, Ownership Groups) are
/// fetched in parallel via <c>Task.WhenAll</c>.
/// </para>
/// </summary>
public sealed class ManageProductVendorServicesAsync : IManageProductVendorServicesAsync
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const int    ProductId      = (int)ProductEnum.VendorServices;
    private const string UdmSourceCode  = BlueBookProductConstants.VendorServices; // "CD"
    private const string TokenCacheKey  = "VS_AccessToken";
    private const string ConfigCacheKey = "VS_ProductConfig";

    private static readonly TimeSpan ConfigCacheTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan TokenCacheTtl  = TimeSpan.FromMinutes(9);

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IProductContextServiceAsync           _context;
    private readonly IProductRepositoryAsync               _productRepository;
    private readonly ISamlAttributeServiceAsync            _samlService;
    private readonly IManageBlueBookAsync                  _blueBook;
    private readonly IManagePersonaAsync                   _managePersona;
    private readonly IManagePersonAsync                    _managePerson;
    private readonly IManageUserLoginAsync                 _manageUserLogin;
    private readonly IManageContactMechanismAsync          _manageContactMechanism;
    private readonly IHttpClientFactory                    _httpClientFactory;
    private readonly IMemoryCache                          _cache;
    private readonly ILogger<ManageProductVendorServicesAsync> _logger;

    public ManageProductVendorServicesAsync(
        IProductContextServiceAsync           context,
        IProductRepositoryAsync               productRepository,
        ISamlAttributeServiceAsync            samlService,
        IManageBlueBookAsync                  blueBook,
        IManagePersonaAsync                   managePersona,
        IManagePersonAsync                    managePerson,
        IManageUserLoginAsync                 manageUserLogin,
        IManageContactMechanismAsync          manageContactMechanism,
        IHttpClientFactory                    httpClientFactory,
        IMemoryCache                          cache,
        ILogger<ManageProductVendorServicesAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(samlService);
        ArgumentNullException.ThrowIfNull(blueBook);
        ArgumentNullException.ThrowIfNull(managePersona);
        ArgumentNullException.ThrowIfNull(managePerson);
        ArgumentNullException.ThrowIfNull(manageUserLogin);
        ArgumentNullException.ThrowIfNull(manageContactMechanism);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _context                = context;
        _productRepository      = productRepository;
        _samlService            = samlService;
        _blueBook               = blueBook;
        _managePersona          = managePersona;
        _managePerson           = managePerson;
        _manageUserLogin        = manageUserLogin;
        _manageContactMechanism = manageContactMechanism;
        _httpClientFactory      = httpClientFactory;
        _cache                  = cache;
        _logger                 = logger;
    }

    // ── Public interface ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetPropertyGroups", editorPersonaId);
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var config = await GetProductConfigAsync(cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);
            int companyId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyId == 0)
            {
                _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "GetPropertyGroups", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse("Company Setup Error: Please Contact Support.");
            }

            // Fetch all three property-group types in parallel — replaces 3 serial sync calls
            var divisionsTask  = GetDivisionsAsync(token, config, companyId, cancellationToken);
            var regionsTask    = GetRegionsAsync(token, config, companyId, cancellationToken);
            var ownershipTask  = GetOwnershipGroupsAsync(token, config, companyId, cancellationToken);
            await Task.WhenAll(divisionsTask, regionsTask, ownershipTask);

            IList<VendorServicesPropertyGroup> allGroups =
            [
                .. divisionsTask.Result,
                .. regionsTask.Result,
                .. ownershipTask.Result
            ];

            response = userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId)
                ? await MergeProductGroupsWithGreenbookAsync(token, config, allGroups, ctx.ProductUserId, cancellationToken)
                : BuildListResponse(allGroups);

            _logger.LogDebug("{Action} - TotalRows={Rows}. EditorPersonaId={Id}", "GetPropertyGroups", response.TotalRows, editorPersonaId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetPropertyGroups", editorPersonaId);
            response = new ListResponse
            {
                IsError     = true,
                ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.PropertyGroupErrorMessage
            };
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetProperties", editorPersonaId);
        var result = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            // CompanyInstanceId (translated) is required for property lookup — useTranslate: false
            var org  = ctx!.EditorPersona.Organization;
            var maps = await _blueBook.GetCompanyMapAsync(
                org.RealPageId,
                org.BooksCustomerMasterId,
                UdmSourceCode,
                org.OrganizationDomain?.Name ?? string.Empty,
                useTranslate: false,
                cancellationToken: cancellationToken);

            int companyInstanceId = maps?
                .FirstOrDefault(m => m.Source.Equals(UdmSourceCode, StringComparison.OrdinalIgnoreCase))
                ?.CompanyInstanceId ?? 0;

            if (companyInstanceId == 0)
            {
                _logger.LogError("{Action} - CompanyInstanceId not found. EditorPersonaId={Id}", "GetProperties", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse("Company Setup Error: Please Contact Support.");
            }

            var companyProperties = await _blueBook.GetCompanyPropertyInstanceAsync(companyInstanceId, cancellationToken);
            IList<ProductProperty> propertyList = companyProperties?.MapBlueBookToGBProperties() ?? [];

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
            {
                var config = await GetProductConfigAsync(cancellationToken);
                string token = await GetTokenAsync(config, cancellationToken);
                result = await MergeProductPropertiesWithGreenbookAsync(token, config, propertyList, ctx.ProductUserId, cancellationToken);
            }
            else
            {
                result = new ListResponse
                {
                    Records     = propertyList.Cast<object>().ToList(),
                    TotalRows   = propertyList.Count,
                    RowsPerPage = propertyList.Count,
                    TotalPages  = 1,
                    ErrorReason = string.Empty
                };
            }

            _logger.LogDebug("{Action} - TotalRows={Rows}. EditorPersonaId={Id}", "GetProperties", result.TotalRows, editorPersonaId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetProperties", editorPersonaId);
            result = new ListResponse
            {
                IsError     = true,
                ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.PropertyErrorMessage
            };
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        AccessType accessType, RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetRoles", editorPersonaId);
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var config = await GetProductConfigAsync(cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);
            int companyId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);

            var allAccessGroups = await GetUserAccessGroupsByAccessTypeAsync(token, config, companyId, cancellationToken);
            if (allAccessGroups is null)
            {
                _logger.LogError("{Action} - No access groups from product. EditorPersonaId={Id}", "GetRoles", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse("No User Access groups (roles) received from product.");
            }

            var gbRoles = MapProductAccessGroupsToGB(allAccessGroups)
                .OrderBy(x => x.Name)
                .ToList();

            response = userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId)
                ? await MergeAccessGroupsWithGreenbookAsync(token, config, gbRoles, ctx.ProductUserId, cancellationToken)
                : BuildListResponse(gbRoles);

            _logger.LogDebug("{Action} - TotalRows={Rows}. EditorPersonaId={Id}", "GetRoles", response.TotalRows, editorPersonaId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetRoles", editorPersonaId);
            response = new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.RoleErrorMessage };
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<Notification?> GetNotificationSettingsAsync(
        long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetNotificationSettings", editorPersonaId);
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return null;

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId))
            {
                var config = await GetProductConfigAsync(cancellationToken);
                string token = await GetTokenAsync(config, cancellationToken);
                var vsUser = await GetVendorServicesUserAsync(token, config, ctx.ProductUserId, cancellationToken);
                if (vsUser is null)
                {
                    _logger.LogError("{Action} - User not found. ProductUserId={Id}", "GetNotificationSettings", ctx.ProductUserId);
                    return null;
                }
                return new Notification
                {
                    IsInsuranceExpired                 = vsUser.EMailNotifyInsurance,
                    IsVendorRecommendationChanges      = vsUser.EMailNotifyRecommendation,
                    IsVendorNotLinkedToAnyProperty     = vsUser.EMailNotifyVendorNotLinkedToAnyProperty
                };
            }
            return new Notification();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetNotificationSettings", editorPersonaId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
        if (error is not null)
        {
            _logger.LogError("{Action} - Context error. UserPersonaId={Id}", "UnassignUser", userPersonaId);
            return error.ErrorReason;
        }

        var config = await GetProductConfigAsync(cancellationToken);
        string token = await GetTokenAsync(config, cancellationToken);
        string result = await DisableProductUserAsync(token, config, ctx!.ProductUsername, isLocked: true, editorPersonaId, cancellationToken);

        if (string.IsNullOrEmpty(result))
        {
            _logger.LogDebug("{Action} - UserPersonaId={Id} unassigned.", "UnassignUser", userPersonaId);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Deleted, cancellationToken);
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<string> UpdateVendorServicesUserProfileAsync(
        long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "UpdateVendorServicesUserProfile", editorPersonaId);
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error.ErrorReason;

            var config = await GetProductConfigAsync(cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);

            var persona    = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            var realPageId = persona.RealPageId;

            var personTask  = _managePerson.GetPersonAsync(realPageId, cancellationToken);
            var loginTask   = _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);
            var contactTask = _manageContactMechanism.ListContactMechanismForPersonAsync(realPageId, string.Empty, cancellationToken);
            await Task.WhenAll(personTask, loginTask, contactTask);

            var    person = personTask.Result;
            string email  = ResolveEmail(contactTask.Result, loginTask.Result);

            var payload = new
            {
                Username  = ctx!.ProductUsername,
                FirstName = person.FirstName,
                LastName  = person.LastName,
                Email     = email
            };

            string url = $"{config.ApiEndpoint}/api/Users/";
            _logger.LogDebug("{Action} - PATCH profile at {Url}", "UpdateVendorServicesUserProfile", url);

            using var client  = CreateBearerClient(token);
            var       content = Serialize(payload);
            var       request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
            var response      = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("{Action} - Success. EditorPersonaId={Id}", "UpdateVendorServicesUserProfile", editorPersonaId);
                return string.Empty;
            }

            string errorBody = string.Empty;
            try { errorBody = await response.Content.ReadAsStringAsync(cancellationToken); } catch { /* ignored */ }
            _logger.LogError("{Action} - PATCH failed. Status={Status}. EditorPersonaId={Id}", "UpdateVendorServicesUserProfile", response.StatusCode, editorPersonaId);
            return $"Error in ManageProductVendorServicesAsync.UpdateVendorServicesUserProfileAsync; errorContent= {errorBody}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "UpdateVendorServicesUserProfile", editorPersonaId);
            return $"Error - {ex.Message}";
        }
    }

    /// <inheritdoc/>
    public Task<(string result, List<AdditionalParameters> auditParams)> ChangeVendorServicesUserTypeAsync(
        long createUserPersonaId, long assignUserPersonaId,
        UserProductPropertyNotification rpList, BatchProcessType batchProcessType,
        CancellationToken cancellationToken = default)
        => ManageVendorServicesUserAsync(createUserPersonaId, assignUserPersonaId, rpList, batchProcessType, cancellationToken);

    /// <inheritdoc/>
    public async Task<(string result, List<AdditionalParameters> auditParams)> ManageVendorServicesUserAsync(
        long editorPersonaId, long productUserPersonaId,
        UserProductPropertyNotification userProductPropertyNotification,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "ManageVendorServicesUser", editorPersonaId);
        List<AdditionalParameters> auditParams = [];

        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, productUserPersonaId, ProductId, cancellationToken);
            if (error is not null) return (error.ErrorReason, auditParams);

            var config = await GetProductConfigAsync(cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);

            // ── Parallel identity lookups ──────────────────────────────────────
            var persona    = await _managePersona.GetPersonaAsync(productUserPersonaId, cancellationToken: cancellationToken);
            var realPageId = persona.RealPageId;

            var personTask  = _managePerson.GetPersonAsync(realPageId, cancellationToken);
            var loginTask   = _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);
            var contactTask = _manageContactMechanism.ListContactMechanismForPersonAsync(realPageId, string.Empty, cancellationToken);
            await Task.WhenAll(personTask, loginTask, contactTask);

            var    person    = personTask.Result;
            var    userLogin = loginTask.Result;
            string email     = ResolveEmail(contactTask.Result, userLogin);

            // ── Resolve company ────────────────────────────────────────────────
            int companyId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyId == 0)
            {
                _logger.LogError("{Action} - Company not found. EditorPersonaId={Id}", "ManageVendorServicesUser", editorPersonaId);
                return ("Company Setup Error: Please Contact Support.", auditParams);
            }

            // ── Fetch all access groups (used for super-user role assignment and audit) ──
            var allUserAccessGroups = await GetUserAccessGroupsByAccessTypeAsync(token, config, companyId, cancellationToken, isSuperUser: true);

            // ── Super-user override ────────────────────────────────────────────
            bool isSuperUser = await _context.IsSuperUserAsync(ctx!.UserPersona!, cancellationToken);
            if (isSuperUser)
            {
                _logger.LogDebug("{Action} - SuperUser detected. EditorPersonaId={Id}", "ManageVendorServicesUser", editorPersonaId);
                userProductPropertyNotification = new UserProductPropertyNotification
                {
                    PropertyList  = ["-1"],
                    PropertyGroup = null,
                    RoleList      = []
                };

                if (allUserAccessGroups is not null)
                {
                    HashSet<string> excluded = ["User", "CliVndOnly", "CliVndRO"];
                    foreach (var ag in allUserAccessGroups.Where(ag => !excluded.Contains(ag.AccessGroupCode)))
                        userProductPropertyNotification.RoleList.Add(ag.AccessGroupCode);
                }
            }

            string productLoginName    = string.IsNullOrEmpty(ctx.ProductUsername) ? userLogin.LoginName : ctx.ProductUsername;
            var    productNotification = MapGbObjectToProduct(userProductPropertyNotification);

            // ── Build VendorServicesUser payload ───────────────────────────────
            List<UserLocation>?    userLocations    = null;
            List<UserAccessGroup>? userAccessGroups = null;
            string?                accessLevel      = null;
            int?                   propertyGroupId  = null;

            if (productNotification.PropertyList?.Count > 0)
            {
                userLocations = productNotification.PropertyList;
                accessLevel   = AccessTypeEnum.Property.ToString();
            }
            if (productNotification.UserAccessGroups?.Count > 0)
                userAccessGroups = productNotification.UserAccessGroups;
            if (productNotification.PropertyGroup is not null)
            {
                propertyGroupId = productNotification.PropertyGroup.Id;
                accessLevel     = productNotification.PropertyGroup.Type.ToString();
            }

            var vendorServicesUser = new VendorServicesUser
            {
                Username                                = productLoginName,
                Password                                = GeneratePassword(15, 5),
                CompanyId                               = companyId.ToString(),
                FirstName                               = person.FirstName,
                LastName                                = person.LastName,
                Email                                   = email,
                UserAccessGroups                        = userAccessGroups,
                UserCode                                = GetUserCode(userLogin.LoginName),
                UserLocations                           = userLocations,
                AccessLevel                             = accessLevel,
                CompanyDivisionId                       = propertyGroupId,
                EMailNotifyInsurance                    = productNotification.Notification.IsInsuranceExpired,
                EMailNotifyRecommendation               = productNotification.Notification.IsVendorRecommendationChanges,
                EMailNotifyVendorNotLinkedToAnyProperty = productNotification.Notification.IsVendorNotLinkedToAnyProperty
            };

            // ── Capture "before" snapshot ──────────────────────────────────────
            var userBeforeUpdate = !string.IsNullOrEmpty(ctx.ProductUserId)
                ? await GetVendorServicesUserAsync(token, config, ctx.ProductUserId, cancellationToken)
                : new VendorServicesUser { UserAccessGroups = [], UserLocations = [] };

            // ── Create or update ───────────────────────────────────────────────
            string insUpdResult;
            if (string.IsNullOrEmpty(ctx.ProductUsername)) // NEW USER
            {
                if (!string.IsNullOrEmpty(productLoginName))
                {
                    string baseLoginName = $"{person.FirstName.Trim()[..1]}{person.LastName.Trim()}".ToLower();
                    int    incrementor   = 0;

                    while (true)
                    {
                        bool? available = await IsUsernameAvailableAsync(token, config, productLoginName, cancellationToken);
                        if (available == true) break;
                        if (available is null)
                        {
                            _logger.LogWarning("{Action} - Cannot validate username {Name}. EditorPersonaId={Id}",
                                "ManageVendorServicesUser", productLoginName, editorPersonaId);
                            return ($"Error - Invalid username {productLoginName}", auditParams);
                        }
                        incrementor++;
                        productLoginName = incrementor == 1
                            ? $"{baseLoginName}{productUserPersonaId}"
                            : $"{baseLoginName}{productUserPersonaId}{incrementor}";
                        _logger.LogDebug("{Action} - Username taken; trying {Name}. EditorPersonaId={Id}",
                            "ManageVendorServicesUser", productLoginName, editorPersonaId);
                    }

                    if (productLoginName.Length > 50)
                        productLoginName = productLoginName[..50];
                    vendorServicesUser.Username = productLoginName;
                }

                insUpdResult = await InsertVendorServicesProductUserAsync(
                    token, config, productUserPersonaId, editorPersonaId, productLoginName, vendorServicesUser, cancellationToken);
            }
            else // UPDATE USER
            {
                vendorServicesUser.ID       = ctx.ProductUserId;
                vendorServicesUser.Username = ctx.ProductUsername;
                insUpdResult = await UpdateVendorServicesProductUserAsync(
                    token, config, productUserPersonaId, editorPersonaId, vendorServicesUser, cancellationToken);
            }

            if (string.IsNullOrEmpty(insUpdResult))
            {
                auditParams.AddRange(await BuildActivityDetailsAsync(
                    editorPersonaId, productUserPersonaId,
                    userBeforeUpdate, vendorServicesUser,
                    allUserAccessGroups ?? [],
                    token, config, cancellationToken));
            }

            return (insUpdResult, auditParams);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "ManageVendorServicesUser", editorPersonaId);
            return ($"Error - {ex.Message}", auditParams);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string username, string productUserId,
        bool isActive = false, CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
        if (error is not null)
        {
            _logger.LogError("{Action} - Context error. EditorPersonaId={Id}", "ChangeUserStatus", editorPersonaId);
            return false;
        }

        try
        {
            int companyId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyId == 0)
            {
                _logger.LogError("{Action} - Company not found. EditorPersonaId={Id}", "ChangeUserStatus", editorPersonaId);
                return false;
            }

            var config = await GetProductConfigAsync(cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);
            // In Vendor Credentialing: isActive=true means NOT locked
            await DisableProductUserAsync(token, config, username, isLocked: !isActive, editorPersonaId, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Failed. Username={Name} EditorPersonaId={Id}", "ChangeUserStatus", username, editorPersonaId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
        if (error is not null) { response.ErrorReason = error.ErrorReason; return response; }

        try
        {
            var config    = await GetProductConfigAsync(cancellationToken);
            int companyId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyId == 0)
            {
                _logger.LogError("{Action} - Company not found. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }

            bool isMigrated   = false;
            int  startRow     = 1;
            int  resultPerRow = 1000;
            if (dataFilter is not null)
            {
                if (dataFilter.FilterBy?.ContainsKey("filter") == true)
                    isMigrated = dataFilter.FilterBy["filter"].Equals("MIGRATED", StringComparison.OrdinalIgnoreCase);
                if (dataFilter.Pages is not null)
                {
                    startRow     = dataFilter.Pages.StartRow;
                    resultPerRow = dataFilter.Pages.ResultsPerPage;
                }
            }

            string token   = await GetTokenAsync(config, cancellationToken);
            string url     = $"{config.ApiEndpoint}/api/users?companyId={companyId}&isMigrated={isMigrated}&startRow={startRow}&resultsPerPage={resultPerRow}";
            var    allUsers = await GetFromApiAsync<IList<VendorServicesUser>>(token, url, cancellationToken);

            if (allUsers is null)
            {
                _logger.LogError("{Action} - No users returned. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
                return response;
            }

            var migrationUsers = allUsers.Select(u => new MigrationUser
            {
                CompanyInstanceSourceId = u.CompanyId,
                FirstName               = u.FirstName,
                LastName                = u.LastName,
                UserId                  = u.ID,
                Username                = u.Username,
                Email                   = u.Email,
                Phone                   = u.Phone,
                LastActivity            = u.LastLoginDate.ToString(),
                Status                  = u.Locked ? "Disabled" : "Active",
                Properties              = u.UserLocations?
                    .Select(l => new MigrationProperty { PropertyInstanceSourceId = l.PropertyId })
                    .ToList() ?? []
            }).ToList();

            _logger.LogDebug("{Action} - {Count} migration users. EditorPersonaId={Id}", "GetMigrationUsers", migrationUsers.Count, editorPersonaId);
            response.RowsPerPage = resultPerRow;
            response.ErrorReason = string.Empty;
            response.IsError     = false;
            response.TotalPages  = 1;
            response.Records     = migrationUsers.Cast<object>().ToList();
            response.TotalRows   = migrationUsers.Count;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
            response = new ListResponse { IsError = true, ErrorReason = ex.Message };
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        long editorPersonaId, IList<MigrateUser> migrateUsers, CancellationToken cancellationToken = default)
    {
        var migrateResponse = new MigrateResponse { Status = false, Message = "Could not migrate users." };
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
        if (error is not null) { migrateResponse.Message = error.ErrorReason; return migrateResponse; }

        try
        {
            var config    = await GetProductConfigAsync(cancellationToken);
            int companyId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyId == 0)
            {
                _logger.LogError("{Action} - Company not found. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            var vsUsers = migrateUsers.Select(u => new VendorServiceMigrateUser
            {
                CompanyId            = companyId.ToString(),
                Id                   = u.UserId,
                UnifiedLoginUserName = u.UnifiedLoginUserName,
                UsingUnifiedLogin    = u.UsingUnifiedLogin
            }).ToList();

            string token   = await GetTokenAsync(config, cancellationToken);
            string url     = $"{config.ApiEndpoint}/api/users/migrateusers";
            using var client   = CreateBearerClient(token);
            var       content  = Serialize(vsUsers);
            var       response = await client.PutAsync(url, content, cancellationToken);
            string    body     = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("{Action} - Success. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
                migrateResponse.Message = body;
                migrateResponse.Status  = true;
            }
            else
            {
                _logger.LogError("{Action} - Failed. Status={Status}. EditorPersonaId={Id}",
                    "UpdateUsersMigrationStatus", response.StatusCode, editorPersonaId);
                migrateResponse.Message = "Cannot update user status to migrated.";
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
            return new MigrateResponse { Status = false, Message = ex.Message };
        }
        return migrateResponse;
    }

    // ── Private — config & token ──────────────────────────────────────────────

    private async ValueTask<VsConfig> GetProductConfigAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ConfigCacheKey, out VsConfig? hit)) return hit!;

        var settings = await _productRepository.GetProductInternalSettingsAsync(ProductId, ct);

        string endpoint     = settings.First(s => s.Name.ToUpper() == "APIENDPOINT").Value;
        string apiSecret    = settings.First(s => s.Name.ToUpper() == "APISECRET").Value;
        string clientId     = settings.First(s => s.Name.ToUpper() == "CLIENTID").Value;
        string tokenUri     = settings.First(s => s.Name.ToUpper() == "TOKENENDPOINT").Value;
        string roleEndpoint = settings.First(s => s.Name.ToUpper() == "GETROLEENDPOINT").Value;

        var cfg = new VsConfig(endpoint, apiSecret, clientId, tokenUri, roleEndpoint);
        _cache.Set(ConfigCacheKey, cfg, ConfigCacheTtl);
        return cfg;
    }

    /// <summary>
    /// Fetches a Bearer token for the VC API using OAuth2 client credentials.
    /// Token cached for <see cref="TokenCacheTtl"/> (9 min, matching VC token lifetime).
    /// Replaces <c>GetToken()</c> + <c>System.Runtime.Caching.MemoryCache.Default</c>.
    /// </summary>
    private async Task<string> GetTokenAsync(VsConfig config, CancellationToken ct)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached)) return cached!;

        var form = new Dictionary<string, string>
        {
            ["client_id"]     = config.ClientId,
            ["client_secret"] = config.ApiSecret,
            ["grant_type"]    = "client_credentials",
            ["scope"]         = config.ClientId  // scope == clientId per original VC token contract
        };

        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(config.TokenIssueUri, new FormUrlEncodedContent(form), ct);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"ManageProductVendorServicesAsync.GetTokenAsync - HTTP {(int)response.StatusCode} - {err}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
            ?? throw new InvalidOperationException("ManageProductVendorServicesAsync.GetTokenAsync - Empty token response.");
        if (!dict.TryGetValue("access_token", out var rawToken))
            throw new InvalidOperationException("ManageProductVendorServicesAsync.GetTokenAsync - access_token not present.");

        string token = Convert.ToString(rawToken)!;
        _cache.Set(TokenCacheKey, token, TokenCacheTtl);
        _logger.LogDebug("{Action} - Token acquired and cached", "GetToken");
        return token;
    }

    // ── Private — BlueBook helpers ────────────────────────────────────────────

    private async Task<int> GetCompanyInstanceSourceIdAsync(ProductCallContext ctx, CancellationToken ct)
    {
        var maps = await _blueBook.GetProductCompanyMappingAsync(
            ctx.EditorPersona.Organization.RealPageId, UdmSourceCode, ct);
        return maps?.FirstOrDefault()?.CompanyInstanceSourceId is string s
            && int.TryParse(s, out int id) ? id : 0;
    }

    // ── Private — VC API property-group fetchers (run in parallel) ────────────

    private async Task<IList<VendorServicesPropertyGroup>> GetDivisionsAsync(
        string token, VsConfig config, int companyId, CancellationToken ct)
    {
        string url    = $"{config.ApiEndpoint}/api/Divisions?companyId={companyId}";
        var    result = await GetFromApiAsync<IList<dynamic>>(token, url, ct, throwOnError: false);
        if (result is null) return [];
        return [.. result.Select(x => new VendorServicesPropertyGroup
            { PropertyGroupId = (int?)x.ID, Name = (string)x.DivisionName, AccessLevel = "Division" })];
    }

    private async Task<IList<VendorServicesPropertyGroup>> GetRegionsAsync(
        string token, VsConfig config, int companyId, CancellationToken ct)
    {
        string url    = $"{config.ApiEndpoint}/api/Regions?companyId={companyId}";
        var    result = await GetFromApiAsync<IList<dynamic>>(token, url, ct, throwOnError: false);
        if (result is null) return [];
        return [.. result.Select(x => new VendorServicesPropertyGroup
            { PropertyGroupId = (int?)x.ID, Name = (string)x.RegionName, AccessLevel = "Region" })];
    }

    private async Task<IList<VendorServicesPropertyGroup>> GetOwnershipGroupsAsync(
        string token, VsConfig config, int companyId, CancellationToken ct)
    {
        string url    = $"{config.ApiEndpoint}/api/OwnershipGroups?companyId={companyId}";
        var    result = await GetFromApiAsync<IList<dynamic>>(token, url, ct, throwOnError: false);
        if (result is null) return [];
        return [.. result.Select(x => new VendorServicesPropertyGroup
            { PropertyGroupId = (int?)x.ID, Name = (string)x.OwnershipGroupName, AccessLevel = "Ownergroup" })];
    }

    // ── Private — VC API user helpers ─────────────────────────────────────────

    private async Task<VendorServicesUser?> GetVendorServicesUserAsync(
        string token, VsConfig config, string productUserId, CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/api/Users/{productUserId}";
        return await GetFromApiAsync<VendorServicesUser>(token, url, ct, throwOnError: false);
    }

    private async Task<bool?> IsUsernameAvailableAsync(
        string token, VsConfig config, string userName, CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/api/Users/IsUsernameAvailable/{userName}/";
        using var client   = CreateBearerClient(token);
        using var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return null;
        var body = await response.Content.ReadAsStringAsync(ct);
        return bool.TryParse(body.Trim().Trim('"'), out bool result) ? result : null;
    }

    private async Task<List<UserAccessGroup>?> GetUserAccessGroupsByAccessTypeAsync(
        string token, VsConfig config, int companyId, CancellationToken ct, bool isSuperUser = false)
    {
        string url = $"{config.ApiEndpoint}/{string.Format(config.GetRoleEndpoint, companyId)}";
        return await GetFromApiAsync<List<UserAccessGroup>>(token, url, ct, throwOnError: false);
    }

    private async Task<string> InsertVendorServicesProductUserAsync(
        string token, VsConfig config,
        long userPersonaId, long editorPersonaId,
        string productLoginName, VendorServicesUser vendorServicesUser,
        CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/api/Users";
        _logger.LogDebug("{Action} - POST create user. EditorPersonaId={Id}", "InsertVendorServicesProductUser", editorPersonaId);

        using var client   = CreateBearerClient(token);
        var       content  = Serialize(vendorServicesUser);
        var       response = await client.PostAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            dynamic? userResult = JsonConvert.DeserializeObject<dynamic>(json);
            if (userResult is not null)
            {
                string newId = Convert.ToString((object)userResult.ID)!;
                await _samlService.UpsertAttributesAsync(userPersonaId, ProductId, new Dictionary<SamlAttributeEnum, string>
                {
                    [SamlAttributeEnum.productUsername] = productLoginName,
                    [SamlAttributeEnum.UserId]          = newId
                }, ct);

                await _productRepository.UpdateProductSettingProductStatusAsync(
                    userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Success, ct);

                // Mark user as using UnifiedLogin in the product
                var migrateResult = await UpdateUsersMigrationStatusAsync(editorPersonaId,
                    [new MigrateUser { UserId = newId, UnifiedLoginUserName = vendorServicesUser.Username, UsingUnifiedLogin = true }], ct);
                if (!migrateResult.Status)
                    _logger.LogWarning("{Action} - Migration status update failed after insert. EditorPersonaId={Id}",
                        "InsertVendorServicesProductUser", editorPersonaId);

                _logger.LogDebug("{Action} - User created. NewId={Id} Login={Login}",
                    "InsertVendorServicesProductUser", newId, productLoginName);
                return string.Empty;
            }
        }

        string errorBody = string.Empty;
        try { errorBody = await response.Content.ReadAsStringAsync(ct); } catch { /* ignored */ }
        _logger.LogError("{Action} - Create failed. EditorPersonaId={Id} Error={Err}",
            "InsertVendorServicesProductUser", editorPersonaId, errorBody);
        await _productRepository.UpdateProductSettingProductStatusAsync(
            userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Error, ct);
        return $"There was a problem creating the user with editorPersona id - {editorPersonaId}. Error-{errorBody}";
    }

    private async Task<string> UpdateVendorServicesProductUserAsync(
        string token, VsConfig config,
        long userPersonaId, long editorPersonaId,
        VendorServicesUser vendorServicesUser,
        CancellationToken ct)
    {
        vendorServicesUser.Password = Guid.NewGuid().ToString(); // fresh password on every update
        string url = $"{config.ApiEndpoint}/api/Users";
        _logger.LogDebug("{Action} - PUT update user. EditorPersonaId={Id}", "UpdateVendorServicesProductUser", editorPersonaId);

        using var client   = CreateBearerClient(token);
        var       content  = Serialize(vendorServicesUser);
        var       response = await client.PutAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Success, ct);
            return string.Empty;
        }

        string errorBody = string.Empty;
        try { errorBody = await response.Content.ReadAsStringAsync(ct); } catch { /* ignored */ }
        _logger.LogError("{Action} - Update failed. EditorPersonaId={Id} Error={Err}",
            "UpdateVendorServicesProductUser", editorPersonaId, errorBody);
        return $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorBody}.";
    }

    private async Task<string> DisableProductUserAsync(
        string token, VsConfig config, string username, bool isLocked, long editorPersonaId, CancellationToken ct)
    {
        var payload = new { username, locked = isLocked };
        string url  = $"{config.ApiEndpoint}/api/Users/";

        using var client  = CreateBearerClient(token);
        var       content = Serialize(payload);
        var       request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
        var response      = await client.SendAsync(request, ct);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogDebug("{Action} - Success. EditorPersonaId={Id}", "DisableProductUser", editorPersonaId);
            return string.Empty;
        }

        string errorBody = string.Empty;
        try { errorBody = await response.Content.ReadAsStringAsync(ct); } catch { /* ignored */ }
        _logger.LogError("{Action} - Failed. Status={Status}. EditorPersonaId={Id}",
            "DisableProductUser", response.StatusCode, editorPersonaId);
        return $"Error in ManageProductVendorServicesAsync.DisableProductUserAsync; errorContent= {errorBody}";
    }

    // ── Private — merge helpers ───────────────────────────────────────────────

    private async Task<ListResponse> MergeProductGroupsWithGreenbookAsync(
        string token, VsConfig config,
        IList<VendorServicesPropertyGroup> allGroups, string productUserId, CancellationToken ct)
    {
        var vsUser = await GetVendorServicesUserAsync(token, config, productUserId, ct);
        if (vsUser is null)
        {
            _logger.LogError("{Action} - User not found. ProductUserId={Id}", "MergeProductGroupsWithGreenbook", productUserId);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        var accessType     = new Dictionary<string, string>();
        string? accessLevel = vsUser.AccessLevel;
        int?    propGroupId = vsUser.CompanyDivisionId;

        if (accessLevel == "Client")
        {
            accessType["accessType"] = "allProperties";
        }
        else if (propGroupId is not null && propGroupId != 0)
        {
            accessType["accessType"] = "propertyGroup";
            foreach (var pg in allGroups.Where(pg => pg.PropertyGroupId == propGroupId))
            {
                if (accessLevel == "Division" || accessLevel == "Region" ||
                    string.Equals(accessLevel?.Trim(), "OWNERSHIP", StringComparison.OrdinalIgnoreCase) ||
                    accessLevel == "Ownergroup")
                {
                    pg.IsAssigned = true;
                }
            }
        }
        else
        {
            accessType["accessType"] = "specificProperties";
        }

        return new ListResponse
        {
            Records     = allGroups.Cast<object>().ToList(),
            TotalRows   = allGroups.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1,
            Additional  = accessType
        };
    }

    private async Task<ListResponse> MergeProductPropertiesWithGreenbookAsync(
        string token, VsConfig config,
        IList<ProductProperty> propertyList, string productUserId, CancellationToken ct)
    {
        var vsUser = await GetVendorServicesUserAsync(token, config, productUserId, ct);
        if (vsUser is null)
        {
            _logger.LogError("{Action} - User not found. ProductUserId={Id}", "MergeProductPropertiesWithGreenbook", productUserId);
            return new ListResponse { IsError = true, ErrorReason = "User not found" };
        }

        if (vsUser.UserLocations is not null)
        {
            var assignedIds = vsUser.UserLocations.Select(l => l.PropertyId).ToHashSet();
            foreach (var pp in propertyList.Where(p => assignedIds.Contains(p.ID)))
                pp.IsAssigned = true;
        }

        return new ListResponse
        {
            Records     = propertyList.Cast<object>().ToList(),
            TotalRows   = propertyList.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1
        };
    }

    private async Task<ListResponse> MergeAccessGroupsWithGreenbookAsync(
        string token, VsConfig config,
        IList<ProductRole> allRoles, string productUserId, CancellationToken ct)
    {
        var vsUser = await GetVendorServicesUserAsync(token, config, productUserId, ct);
        if (vsUser is null)
        {
            _logger.LogError("{Action} - User not found. ProductUserId={Id}", "MergeAccessGroupsWithGreenbook", productUserId);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        if (vsUser.UserAccessGroups is not null)
        {
            var assignedCodes = vsUser.UserAccessGroups.Select(ag => ag.AccessGroupCode).ToHashSet();
            foreach (var role in allRoles.Where(r => assignedCodes.Contains(r.ID)))
                role.IsAssigned = true;
        }

        return BuildListResponse(allRoles);
    }

    // ── Private — audit ───────────────────────────────────────────────────────

    private async Task<List<AdditionalParameters>> BuildActivityDetailsAsync(
        long editorPersonaId, long productUserPersonaId,
        VendorServicesUser? userBeforeUpdate, VendorServicesUser vendorServicesUser,
        List<UserAccessGroup> allUserAccessGroups,
        string token, VsConfig config, CancellationToken ct)
    {
        var result = new List<AdditionalParameters>();
        try
        {
            // 1. Access-type diff
            if (userBeforeUpdate?.AccessLevel != vendorServicesUser.AccessLevel)
            {
                result.Add(new AdditionalParameters
                    { Key = "Vendor Credentialing AccessType", Value = $"{GetAccessType(vendorServicesUser)} was assigned" });
                if (userBeforeUpdate is not null && !string.IsNullOrEmpty(userBeforeUpdate.AccessLevel))
                    result.Add(new AdditionalParameters
                        { Key = "Vendor Credentialing AccessType", Value = $"{GetAccessType(userBeforeUpdate)} was removed" });
            }

            // 2. Roles diff — O(1) set operations
            var oldCodes = userBeforeUpdate?.UserAccessGroups?.Select(ag => ag.AccessGroupCode).ToHashSet() ?? [];
            var newCodes = vendorServicesUser.UserAccessGroups?.Select(ag => ag.AccessGroupCode).ToHashSet() ?? [];

            foreach (string r in oldCodes.Except(newCodes))
            {
                string name = allUserAccessGroups.Find(ag => ag.AccessGroupCode == r)?.AccessGroupName ?? r;
                result.Add(new AdditionalParameters { Key = "Vendor Credentialing Roles", Value = $"{name} was removed" });
            }
            foreach (string r in newCodes.Except(oldCodes))
            {
                string name = allUserAccessGroups.Find(ag => ag.AccessGroupCode == r)?.AccessGroupName ?? r;
                result.Add(new AdditionalParameters { Key = "Vendor Credentialing Roles", Value = $"{name} was assigned" });
            }

            // 3. Properties diff — load full property list for name lookup
            var propsResponse = await GetPropertiesAsync(editorPersonaId, productUserPersonaId, new RequestParameter(), ct);
            var properties    = propsResponse.Records?.Cast<ProductProperty>().ToList() ?? [];

            var oldPropIds = userBeforeUpdate?.UserLocations?.Select(l => l.PropertyId).ToHashSet() ?? [];
            var newPropIds = vendorServicesUser.UserLocations?.Select(l => l.PropertyId).ToHashSet() ?? [];

            foreach (string p in oldPropIds.Except(newPropIds))
            {
                var prop = properties.Find(f => f.ID == p);
                if (prop is not null)
                    result.Add(new AdditionalParameters { Key = "Vendor Credentialing Properties", Value = $"{prop.Name} was removed" });
            }
            foreach (string p in newPropIds.Except(oldPropIds))
            {
                var prop = properties.Find(f => f.ID == p);
                if (prop is not null)
                    result.Add(new AdditionalParameters { Key = "Vendor Credentialing Properties", Value = $"{prop.Name} was assigned" });
            }

            // 4. Property-group diff
            if (userBeforeUpdate?.CompanyDivisionId != vendorServicesUser.CompanyDivisionId)
            {
                var groupsResponse = await GetPropertyGroupsAsync(editorPersonaId, productUserPersonaId, new RequestParameter(), ct);
                var groups         = groupsResponse.Records?.Cast<VendorServicesPropertyGroup>().ToList() ?? [];

                var newGrp = groups.Find(g => g.PropertyGroupId == vendorServicesUser.CompanyDivisionId);
                if (newGrp is not null)
                    result.Add(new AdditionalParameters { Key = "Vendor Credentialing PropertyGroups", Value = $"{newGrp.Name} was assigned" });

                var oldGrp = groups.Find(g => g.PropertyGroupId == userBeforeUpdate?.CompanyDivisionId);
                if (oldGrp is not null)
                    result.Add(new AdditionalParameters { Key = "Vendor Credentialing PropertyGroups", Value = $"{oldGrp.Name} was removed" });
            }

            // 5. Notification diffs
            AddNotificationAudit(result, "EMail Notify Insurance",
                userBeforeUpdate?.EMailNotifyInsurance, vendorServicesUser.EMailNotifyInsurance);
            AddNotificationAudit(result, "EMail Notify Recommendation",
                userBeforeUpdate?.EMailNotifyRecommendation, vendorServicesUser.EMailNotifyRecommendation);
            AddNotificationAudit(result, "EMail Notify Vendor Not Linked To Any Property",
                userBeforeUpdate?.EMailNotifyVendorNotLinkedToAnyProperty, vendorServicesUser.EMailNotifyVendorNotLinkedToAnyProperty);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error building audit log. EditorPersonaId={Id}", "BuildActivityDetails", editorPersonaId);
        }
        return result;
    }

    private static void AddNotificationAudit(List<AdditionalParameters> result, string key, bool? oldValue, bool newValue)
    {
        if (oldValue == newValue) return;
        result.Add(new AdditionalParameters { Key = $"Vendor Credentialing {key}", Value = $"{newValue} was assigned" });
        if (oldValue.HasValue)
            result.Add(new AdditionalParameters { Key = $"Vendor Credentialing {key}", Value = $"{oldValue.Value} was removed" });
    }

    // ── Private — HTTP helpers ────────────────────────────────────────────────

    private async Task<T?> GetFromApiAsync<T>(
        string token, string url, CancellationToken ct, bool throwOnError = true)
        where T : class
    {
        _logger.LogDebug("{Action} - GET {Url}", "GetFromApi", url);
        using var client   = CreateBearerClient(token);
        using var response = await client.GetAsync(url, ct);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonConvert.DeserializeObject<T>(json);
        }

        var errBody = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("{Action} - Non-200. Url={Url} Status={Status}", "GetFromApi", url, response.StatusCode);

        if (throwOnError)
            throw new InvalidOperationException($"API error {response.StatusCode}: {errBody}");
        return null;
    }

    private HttpClient CreateBearerClient(string token)
    {
        var client = _httpClientFactory.CreateClient("VendorServices");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ── Private — utilities ───────────────────────────────────────────────────

    private static StringContent Serialize<T>(T value)
        => new(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");

    private static ListResponse BuildListResponse<T>(IList<T> items) where T : class
        => new()
        {
            Records     = items.Cast<object>().ToList(),
            TotalRows   = items.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1
        };

    private static string ResolveEmail(IList<CommonAddress> contacts, UserLoginOnly userLogin)
    {
        string email = contacts
            .FirstOrDefault(a => a.AddressType?.ToUpper() == "EMAIL")
            ?.AddressString ?? string.Empty;
        if (string.IsNullOrEmpty(email))
            email = ProductManagerHelpers.ValidateAndReturnEmailAddress(userLogin.LoginName);
        return email;
    }

    private static string GetAccessType(VendorServicesUser user) => user switch
    {
        { AccessLevel: "Client" }                               => "All Properties",
        { CompanyDivisionId: not null and not 0 }               => "Property Group",
        _                                                       => "Specific Property"
    };

    private static string GetUserCode(string userLoginName)
    {
        int atIdx = userLoginName.IndexOf('@');
        return atIdx >= 0 ? userLoginName[..atIdx] : userLoginName;
    }

    private static IList<ProductRole> MapProductAccessGroupsToGB(IList<UserAccessGroup> groups)
        => [.. groups.Select(ag => new ProductRole
        {
            ID          = ag.AccessGroupCode,
            Description = ag.Description,
            Name        = ag.AccessGroupName
        })];

    private static ProductPropertyNotification MapGbObjectToProduct(UserProductPropertyNotification src)
    {
        var result = new ProductPropertyNotification
        {
            Notification = new Notification
            {
                IsVendorRecommendationChanges      = src.IsVendorRecommendationChanges,
                IsInsuranceExpired                 = src.IsInsuranceExpired,
                IsVendorNotLinkedToAnyProperty     = src.IsVendorNotLinkedToAnyProperty
            }
        };

        if (src.PropertyGroup?.Count > 0)
        {
            result.PropertyGroup = new PropertyGroup
            {
                Id   = src.PropertyGroup[0].Id,
                Type = src.PropertyGroup[0].Type
            };
        }

        if (src.PropertyList?.Count > 0)
        {
            result.PropertyList = [];
            foreach (string propId in src.PropertyList)
            {
                if (propId == "-1")
                {
                    src.PropertyList     = null;
                    result.PropertyGroup = new PropertyGroup { Type = AccessTypeEnum.Client };
                    break;
                }
                result.PropertyList.Add(new UserLocation { PropertyId = propId });
            }
        }

        if (src.RoleList?.Count > 0)
            result.UserAccessGroups = [.. src.RoleList.Select(r => new UserAccessGroup { AccessGroupCode = r })];

        return result;
    }

    /// <summary>Generates a cryptographically-random password with guaranteed non-alphanumeric chars.</summary>
    private static string GeneratePassword(int length, int nonAlphanumericCount)
    {
        if (length < nonAlphanumericCount) throw new ArgumentException("length must be >= nonAlphanumericCount");
        const string alpha    = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const string nonAlpha = "!@$?_-%^&*";
        Span<byte> rndBytes   = stackalloc byte[length];
        RandomNumberGenerator.Fill(rndBytes);
        var sb       = new StringBuilder(length);
        int nonAdded = 0;
        for (int i = 0; i < length; i++)
        {
            if (nonAdded < nonAlphanumericCount && (length - i) <= (nonAlphanumericCount - nonAdded))
            {
                sb.Append(nonAlpha[rndBytes[i] % nonAlpha.Length]);
                nonAdded++;
            }
            else
            {
                sb.Append(alpha[rndBytes[i] % alpha.Length]);
            }
        }
        while (nonAdded < nonAlphanumericCount)
        {
            sb[rndBytes[nonAdded] % sb.Length] = nonAlpha[rndBytes[nonAdded] % nonAlpha.Length];
            nonAdded++;
        }
        return sb.ToString();
    }
}

/// <summary>Immutable Vendor Services product config snapshot — safe to cache in <see cref="IMemoryCache"/>.</summary>
internal sealed record VsConfig(
    string ApiEndpoint,
    string ApiSecret,
    string ClientId,
    string TokenIssueUri,
    string GetRoleEndpoint);
