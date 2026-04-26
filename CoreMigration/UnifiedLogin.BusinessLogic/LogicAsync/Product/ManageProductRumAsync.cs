using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Native-async implementation of <see cref="IManageProductRumAsync"/> for
/// RealPage Utility Management (NWP/RUM).
/// <para>
/// Thread-safe: no mutable instance fields.  Bearer token is cached in
/// <see cref="IMemoryCache"/> (9-minute TTL matching NWP token lifetime).
/// </para>
/// </summary>
public sealed class ManageProductRumAsync : IManageProductRumAsync
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const int    ProductId        = (int)ProductEnum.UtilityManagement;
    private const string UdmSourceCode    = BlueBookProductConstants.UtilityManagement;
    private const string TokenCacheKey    = "RUM_AccessToken";
    private const string ConfigCacheKey   = "RUM_ProductConfig";
    private const string NwpTokenScope    = "greenbooknwpapi";

    private static readonly TimeSpan ConfigCacheTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan TokenCacheTtl  = TimeSpan.FromMinutes(9);

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IProductContextServiceAsync  _context;
    private readonly IProductRepositoryAsync      _productRepository;
    private readonly ISamlAttributeServiceAsync   _samlService;
    private readonly IManageBlueBookAsync         _blueBook;
    private readonly IManagePersonaAsync          _managePersona;
    private readonly IManagePersonAsync           _managePerson;
    private readonly IManageUserLoginAsync        _manageUserLogin;
    private readonly IManageContactMechanismAsync _manageContactMechanism;
    private readonly IHttpClientFactory           _httpClientFactory;
    private readonly IMemoryCache                 _cache;
    private readonly ILogger<ManageProductRumAsync> _logger;

    public ManageProductRumAsync(
        IProductContextServiceAsync  context,
        IProductRepositoryAsync      productRepository,
        ISamlAttributeServiceAsync   samlService,
        IManageBlueBookAsync         blueBook,
        IManagePersonaAsync          managePersona,
        IManagePersonAsync           managePerson,
        IManageUserLoginAsync        manageUserLogin,
        IManageContactMechanismAsync manageContactMechanism,
        IHttpClientFactory           httpClientFactory,
        IMemoryCache                 cache,
        ILogger<ManageProductRumAsync> logger)
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
        RequestParameter datafilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetPropertyGroups", editorPersonaId);
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var config = await GetProductConfigAsync(cancellationToken);
            int companyInstanceSourceId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyInstanceSourceId == 0)
            {
                _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "GetPropertyGroups", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse("Company Setup Error: Please Contact Support.");
            }

            string token = await GetTokenAsync(config, cancellationToken);
            var groups = await GetRumPropertiesDataAsync(token, config, companyInstanceSourceId, "GM", cancellationToken);

            if (groups is null || groups.Count == 0)
            {
                _logger.LogError("{Action} - No property groups from product. EditorPersonaId={Id}", "GetPropertyGroups", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse("No properties received from product.");
            }

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId))
                response = await MergeRumPropertiesWithGreenbookAsync(token, config, groups, ctx.ProductUserId, cancellationToken);
            else
                response = BuildListResponse(groups);

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
        RequestParameter datafilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetProperties", editorPersonaId);
        var result = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var config = await GetProductConfigAsync(cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);

            // Resolve RUM company ID from BlueBook using the UtilityManagement source
            var companyMap = await _blueBook.GetCompanyMapAsync(
                ctx!.EditorPersona.Organization.RealPageId,
                ctx.EditorPersona.Organization.BooksCustomerMasterId,
                source: UdmSourceCode,
                domain: ctx.EditorPersona.Organization.OrganizationDomain?.Name ?? "",
                cancellationToken: cancellationToken);

            string rumCompanyId = companyMap?
                .FirstOrDefault(a => a.Source.ToUpper() == UdmSourceCode)
                ?.CompanyInstanceSourceId ?? "";

            string url = $"{config.ApiEndpoint}/identity/Property?companyId={rumCompanyId}";
            _logger.LogDebug("{Action} - Fetching properties at {Url}", "GetProperties", url);

            var propertyList = await GetFromApiAsync<IList<ProductPropertyMap>>(token, url, cancellationToken);

            if (propertyList is not null && propertyList.Count > 0)
            {
                IList<RumPropertyGroup> rumProperties = propertyList
                    .Select(p => new RumPropertyGroup { Id = p.PropertyId, Name = p.PropertyName, State = p.State, IsAssigned = false })
                    .ToList();

                if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
                    result = await MergeRumPropertiesWithGreenbookAsync(token, config, rumProperties, ctx.ProductUserId, cancellationToken);
                else
                    result = new ListResponse
                    {
                        Records     = rumProperties.Cast<object>().ToList(),
                        TotalRows   = rumProperties.Count,
                        RowsPerPage = rumProperties.Count,
                        TotalPages  = 1,
                        ErrorReason = string.Empty
                    };
            }
            else
            {
                result = ProductManagerHelpers.ErrorResponse(CommonMessageConstants.PropertyErrorMessage);
            }
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
    public async Task<ListResponse> GetRegionsAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetRegions", editorPersonaId);
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var config = await GetProductConfigAsync(cancellationToken);
            int companyInstanceSourceId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyInstanceSourceId == 0)
            {
                _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "GetRegions", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse("Company Setup Error: Please Contact Support.");
            }

            string token  = await GetTokenAsync(config, cancellationToken);
            var allRegions = await GetRumPropertiesDataAsync(token, config, companyInstanceSourceId, "RM", cancellationToken);

            if (allRegions is null)
            {
                _logger.LogError("{Action} - No regions from product. EditorPersonaId={Id}", "GetRegions", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse(CommonMessageConstants.RegionErrorMessage);
            }

            response = userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId)
                ? await MergeRumPropertiesWithGreenbookAsync(token, config, allRegions, ctx.ProductUserId, cancellationToken)
                : BuildListResponse(allRegions);

            _logger.LogDebug("{Action} - TotalRows={Rows}. EditorPersonaId={Id}", "GetRegions", response.TotalRows, editorPersonaId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetRegions", editorPersonaId);
            response = new ListResponse
            {
                IsError     = true,
                ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.RegionErrorMessage
            };
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetRoles", editorPersonaId);
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var config = await GetProductConfigAsync(cancellationToken);
            int companyInstanceSourceId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);

            var allRoles = await GetRumRolesAsync(token, config, companyInstanceSourceId, ctx!, cancellationToken);
            if (allRoles is null)
            {
                _logger.LogError("{Action} - No roles from product. EditorPersonaId={Id}", "GetRoles", editorPersonaId);
                return ProductManagerHelpers.ErrorResponse("No User Access groups (roles) received from product.");
            }

            response = userPersonaId != 0 && !string.IsNullOrEmpty(ctx!.ProductUserId)
                ? await MergeUserRolesWithProductRolesAsync(token, config, allRoles, ctx.ProductUserId, cancellationToken)
                : BuildListResponse(allRoles);

            _logger.LogDebug("{Action} - TotalRows={Rows}. EditorPersonaId={Id}", "GetRoles", response.TotalRows, editorPersonaId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetRoles", editorPersonaId);
            response = new ListResponse
            {
                IsError     = true,
                ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.AdditionalRightErrorMessage
            };
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetUMGlobalRolesAsync(
        long editorPersonaId, long userPersonaId,
        RequestParameter datafilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "GetUMGlobalRoles", editorPersonaId);
        var response = new ListResponse();
        try
        {
            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return error;

            var config = await GetProductConfigAsync(cancellationToken);
            bool isContractCompany = ctx!.EditorPersona.Organization.RealPageId == config.ContractCompanyRealPageId;

            List<ProductRole> globalRoles;
            if (!isContractCompany)
            {
                int companyInstanceSourceId = await GetCompanyInstanceSourceIdAsync(ctx, cancellationToken);
                if (companyInstanceSourceId == 0)
                {
                    _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "GetUMGlobalRoles", editorPersonaId);
                    return ProductManagerHelpers.ErrorResponse("Company Setup Error: Please Contact Support.");
                }

                globalRoles =
                [
                    new ProductRole { ID = "PR", Name = "Select Properties", Description = "Select Properties", IsAssigned = false },
                    new ProductRole { ID = "GM", Name = "Groups",            Description = "Groups",            IsAssigned = false },
                    new ProductRole { ID = "PM", Name = "All Properties",    Description = "All Properties",    IsAssigned = false }
                ];
            }
            else
            {
                globalRoles = [new ProductRole { ID = "SU", Name = "Subcontractor", Description = "Subcontractor", IsAssigned = false }];
            }

            if (userPersonaId != 0 && !string.IsNullOrEmpty(ctx.ProductUserId))
            {
                string token = await GetTokenAsync(config, cancellationToken);
                response = await MergeRumGlobalRolesWithGreenbookAsync(token, config, globalRoles, ctx.ProductUserId, cancellationToken);
            }
            else
            {
                response = BuildListResponse(globalRoles);
            }

            _logger.LogDebug("{Action} - TotalRows={Rows}. EditorPersonaId={Id}", "GetUMGlobalRoles", response.TotalRows, editorPersonaId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "GetUMGlobalRoles", editorPersonaId);
            response = new ListResponse
            {
                IsError     = true,
                ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.RoleErrorMessage
            };
        }
        return response;
    }

    /// <inheritdoc/>
    public async Task<string> UnassignRumUserAsync(
        long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
        if (error is not null)
        {
            _logger.LogError("{Action} - Context error. UserPersonaId={Id} Reason={Reason}",
                "UnassignRumUser", userPersonaId, error.ErrorReason);
            return error.ErrorReason;
        }

        var config = await GetProductConfigAsync(cancellationToken);
        string token = await GetTokenAsync(config, cancellationToken);
        string result = await DeleteRumUserAsync(token, config, ctx!.ProductUserId, editorPersonaId, cancellationToken);

        if (string.IsNullOrEmpty(result))
        {
            _logger.LogDebug("{Action} - UserPersonaId={Id} unassigned successfully", "UnassignRumUser", userPersonaId);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Deleted, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<string> UpdateUserProfileAsync(
        long editorPersonaId, long userPersonaId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "UpdateUserProfile", editorPersonaId);
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

            string email = ResolveEmail(contactTask.Result, loginTask.Result);
            var    person = personTask.Result;

            var rumUser = new RumUser { FirstName = person.FirstName, LastName = person.LastName, Email = email };

            string url = $"{config.ApiEndpoint}/user/putuserinfo?userId={ctx!.ProductUserId}";
            _logger.LogDebug("{Action} - PUT profile at {Url}", "UpdateUserProfile", url);

            using var client  = CreateBearerClient(token);
            var       content = Serialize(rumUser);
            var response      = await client.PutAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("{Action} - Profile updated. EditorPersonaId={Id}", "UpdateUserProfile", editorPersonaId);
                return string.Empty;
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("{Action} - PUT failed. Status={Status} Body={Body}", "UpdateUserProfile", response.StatusCode, errorBody);
            return $"There was a problem updating user profile for user with editorPersona id - {editorPersonaId} - Error-{errorBody}.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "UpdateUserProfile", editorPersonaId);
            return $"Error - {ex.Message}";
        }
    }

    /// <inheritdoc/>
    public async Task<(string result, List<AdditionalParameters> auditParams)> ManageRumUserAsync(
        long editorPersonaId, long userPersonaId,
        RumUserPropertyRegionRole userPropertyRegionRole, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{Action} - EditorPersonaId={Id}", "ManageRumUser", editorPersonaId);
        List<AdditionalParameters> auditParams = [];

        try
        {
            if (userPropertyRegionRole is null)
                throw new ArgumentNullException(nameof(userPropertyRegionRole),
                    "RumUserPropertyRegionRole received null; check JSON in product batch table or parsing issue.");

            var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, userPersonaId, ProductId, cancellationToken);
            if (error is not null) return (error.ErrorReason, auditParams);

            var config = await GetProductConfigAsync(cancellationToken);
            string token = await GetTokenAsync(config, cancellationToken);

            // ── Parallel identity lookups ──────────────────────────────────────
            var persona    = await _managePersona.GetPersonaAsync(userPersonaId, cancellationToken: cancellationToken);
            var realPageId = persona.RealPageId;

            var personTask  = _managePerson.GetPersonAsync(realPageId, cancellationToken);
            var loginTask   = _manageUserLogin.GetUserLoginOnlyAsync(realPageId, cancellationToken);
            var contactTask = _manageContactMechanism.ListContactMechanismForPersonAsync(realPageId, string.Empty, cancellationToken);
            await Task.WhenAll(personTask, loginTask, contactTask);

            var person       = personTask.Result;
            var userLogin    = loginTask.Result;
            string userEmail = ResolveEmail(contactTask.Result, userLogin);

            string productLoginName = string.IsNullOrEmpty(ctx!.ProductUsername) ? userLogin.LoginName : ctx.ProductUsername;
            int    companyId        = 0;
            string userAccessType   = string.Empty;
            List<int> propertiesList = [];

            bool isContractCompany = ctx.EditorPersona.Organization.RealPageId == config.ContractCompanyRealPageId;

            // ── Capture "before" snapshot (parallel queries) ──────────────────
            var oldData = await GetUserAccountableDataAsync(token, config, ctx, editorPersonaId, userPersonaId, cancellationToken);

            // ── Build access type and property list ───────────────────────────
            if (!isContractCompany)
            {
                int companyInstanceSourceId = await GetCompanyInstanceSourceIdAsync(ctx, cancellationToken);
                if (companyInstanceSourceId == 0)
                {
                    _logger.LogError("{Action} - Company not found. EditorPersonaId={Id}", "ManageRumUser", editorPersonaId);
                    return ("Company Setup Error: Please Contact Support.", auditParams);
                }
                companyId = companyInstanceSourceId;

                bool isSuperUser = await _context.IsSuperUserAsync(ctx.UserPersona!, cancellationToken);
                if (isSuperUser)
                {
                    // Preserve existing roles if none explicitly passed
                    if (userPropertyRegionRole.RoleList.Count == 0 && !string.IsNullOrEmpty(ctx.ProductUsername))
                    {
                        var rumUserData = await GetRumUserClaimsAsync(token, config, ctx.ProductUserId, cancellationToken);
                        if (rumUserData is not null)
                        {
                            var existingRoleNames = rumUserData.Claims
                                .Where(c => c.Type == "role")
                                .Select(c => c.Value)
                                .ToHashSet();

                            var allRoles = await GetRumRolesAsync(token, config, companyId, ctx, cancellationToken);
                            if (allRoles is not null)
                            {
                                userPropertyRegionRole.RoleList.AddRange(
                                    allRoles.Where(r => existingRoleNames.Contains(r.Name)).Select(r => r.Name));
                            }
                        }
                    }

                    // Append configured super-user roles
                    if (!string.IsNullOrEmpty(config.SuperUserRoles))
                    {
                        var superRoles = config.SuperUserRoles
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Distinct();
                        userPropertyRegionRole.RoleList.AddRange(superRoles);
                    }

                    propertiesList.Add(companyId);
                    userAccessType = UserType.PortfolioManager.ToString();
                }
                else
                {
                    if (userPropertyRegionRole.PropertyGroupList?.Count > 0)
                    {
                        userAccessType = UserType.GroupManager.ToString();
                        propertiesList.AddRange(userPropertyRegionRole.PropertyGroupList.Select(int.Parse));
                    }

                    if (userPropertyRegionRole.PropertyList?.Count > 0)
                    {
                        if (userPropertyRegionRole.PropertyList[0].ToUpper() != "ALL")
                        {
                            userAccessType = UserType.PropertyManager.ToString();
                            propertiesList.AddRange(userPropertyRegionRole.PropertyList.Select(int.Parse));
                        }
                        else
                        {
                            propertiesList.Add(companyId);
                            userAccessType = UserType.PortfolioManager.ToString();
                        }
                    }
                }
            }
            else
            {
                if (userPropertyRegionRole.PropertyGroupList?.Count > 0)
                    userAccessType = UserType.SubContractor.ToString();
            }

            var rumUser = new RumUser
            {
                FirstName    = person.FirstName,
                LastName     = person.LastName,
                Email        = userEmail,
                Phone        = "",
                RealPageName = "GreenBook",
                UserName     = productLoginName,
                UserTypeCode = userAccessType,
                PortfolioId  = companyId,
                AssetIds     = propertiesList,
                Roles        = userPropertyRegionRole.RoleList
            };

            _logger.LogDebug("{Action} - RumUser built. EditorPersonaId={Id}", "ManageRumUser", editorPersonaId);

            // ── Create or update ───────────────────────────────────────────────
            string insUpdResult;
            if (string.IsNullOrEmpty(ctx.ProductUsername)) // new user
            {
                // Ensure unique username
                if (!string.IsNullOrEmpty(productLoginName))
                {
                    int incrementor = 0;
                    while (await CheckUserExistsInRumAsync(token, config, productLoginName, cancellationToken))
                    {
                        incrementor++;
                        productLoginName = productLoginName + incrementor;
                        _logger.LogDebug("{Action} - Username {Name} taken; trying {New}", "ManageRumUser", productLoginName, productLoginName);
                    }
                    rumUser.UserName = productLoginName;
                }

                insUpdResult = await InsertRumProductUserAsync(
                    token, config, userPersonaId, editorPersonaId, productLoginName, rumUser, companyId, cancellationToken);
            }
            else // update
            {
                insUpdResult = await UpdateRumProductUserAsync(
                    token, config, ctx.ProductUserId, userPersonaId, editorPersonaId, rumUser, cancellationToken);
            }

            // ── "After" snapshot and audit diff ───────────────────────────────
            var newData    = await GetUserAccountableDataAsync(token, config, ctx, editorPersonaId, userPersonaId, cancellationToken);
            auditParams.AddRange(BuildActivityLogs(newData, oldData, rumUser));

            return (insUpdResult, auditParams);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{Action} - Error. EditorPersonaId={Id}", "ManageRumUser", editorPersonaId);
            return ($"Error - {ex.Message}", auditParams);
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetMigrationUsersAsync(
        long editorPersonaId, RequestParameter datafilter, CancellationToken cancellationToken = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
        if (error is not null) { response.ErrorReason = error.ErrorReason; return response; }

        try
        {
            var config = await GetProductConfigAsync(cancellationToken);
            int companyInstanceSourceId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyInstanceSourceId == 0)
            {
                _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }

            string filter        = "NonMigrated";
            int    startRow      = 0;
            int    resultPerRow  = 1000;

            if (datafilter is not null)
            {
                if (datafilter.FilterBy?.ContainsKey("filter") == true)
                    filter = datafilter.FilterBy["filter"];
                if (datafilter.Pages is not null)
                {
                    startRow     = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage;
                }
            }

            string url = $"{config.ApiEndpoint}/migration/{companyInstanceSourceId}/users?filter={filter}&startRow={startRow}&resultsPerPage={resultPerRow}";
            _logger.LogDebug("{Action} - Fetching migration users. Url={Url}", "GetMigrationUsers", url);

            string token  = await GetTokenAsync(config, cancellationToken);
            var allUsers  = await GetFromApiAsync<IList<MigrationUser>>(token, url, cancellationToken);

            if (allUsers is null)
            {
                _logger.LogError("{Action} - No users returned. EditorPersonaId={Id}", "GetMigrationUsers", editorPersonaId);
                return response;
            }

            _logger.LogDebug("{Action} - {Count} migration users. EditorPersonaId={Id}", "GetMigrationUsers", allUsers.Count, editorPersonaId);
            response.RowsPerPage = resultPerRow;
            response.ErrorReason = string.Empty;
            response.IsError     = false;
            response.TotalPages  = 1;
            response.Records     = allUsers.Cast<object>().ToList();
            response.TotalRows   = allUsers.Count;
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
            var config = await GetProductConfigAsync(cancellationToken);
            int companyInstanceSourceId = await GetCompanyInstanceSourceIdAsync(ctx!, cancellationToken);
            if (companyInstanceSourceId == 0)
            {
                _logger.LogError("{Action} - CompanyInstanceSourceId not found. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            string token   = await GetTokenAsync(config, cancellationToken);
            string url     = $"{config.ApiEndpoint}/migration/{companyInstanceSourceId}/migrate-users";
            var    content = Serialize(migrateUsers);

            _logger.LogDebug("{Action} - POST migration status at {Url}", "UpdateUsersMigrationStatus", url);

            using var client   = CreateBearerClient(token);
            var response       = await client.PostAsync(url, content, cancellationToken);
            var responseBody   = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("{Action} - Success. EditorPersonaId={Id}", "UpdateUsersMigrationStatus", editorPersonaId);
                migrateResponse.Message = "Success";
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

    /// <inheritdoc/>
    public async Task<bool> ChangeUserStatusAsync(
        long editorPersonaId, string productUserId, CancellationToken cancellationToken = default)
    {
        var (ctx, error) = await _context.GetUserContextAsync(editorPersonaId, 0, ProductId, cancellationToken);
        if (error is not null)
        {
            _logger.LogError("{Action} - Context error. ProductUserId={Id} Reason={Reason}",
                "ChangeUserStatus", productUserId, error.ErrorReason);
            return false;
        }

        var config = await GetProductConfigAsync(cancellationToken);
        string token  = await GetTokenAsync(config, cancellationToken);
        string result = await DeleteRumUserAsync(token, config, productUserId, editorPersonaId, cancellationToken);

        if (string.IsNullOrEmpty(result))
        {
            _logger.LogDebug("{Action} - Status changed. ProductUserId={Id}", "ChangeUserStatus", productUserId);
            return true;
        }
        return false;
    }

    // ── Private — config & token ──────────────────────────────────────────────

    /// <summary>
    /// Returns <see cref="RumConfig"/> from <see cref="IMemoryCache"/>, refreshing on miss.
    /// Zero Task allocation on cache hit.
    /// </summary>
    private async ValueTask<RumConfig> GetProductConfigAsync(CancellationToken ct)
    {
        if (_cache.TryGetValue(ConfigCacheKey, out RumConfig? hit)) return hit!;

        var settings = await _productRepository.GetProductInternalSettingsAsync(ProductId, ct);

        string endpoint    = settings.First(s => s.Name.ToUpper() == "APIENDPOINT").Value;
        string apiSecret   = settings.First(s => s.Name.ToUpper() == "APISECRET").Value;
        string clientId    = settings.First(s => s.Name.ToUpper() == "CLIENTID").Value;
        string tokenUrl    = settings.First(s => s.Name.ToUpper() == "TOKENURL").Value;
        string superRoles  = settings.FirstOrDefault(s => s.Name.Equals("UtilitySuperUser", StringComparison.OrdinalIgnoreCase))?.Value ?? "";

        // ContractCompanyRealPageId identifies NWP's own subcontractor tenant
        var contractSetting = settings.FirstOrDefault(s => s.Name.Equals("ContractCompanyRealPageId", StringComparison.OrdinalIgnoreCase));
        Guid contractGuid   = contractSetting is not null && Guid.TryParse(contractSetting.Value, out var g) ? g : Guid.Empty;

        var cfg = new RumConfig(endpoint, apiSecret, clientId, tokenUrl, superRoles, contractGuid);
        _cache.Set(ConfigCacheKey, cfg, ConfigCacheTtl);
        return cfg;
    }

    /// <summary>
    /// Fetches a Bearer token for the NWP API using OAuth2 client credentials.
    /// Token is cached for <see cref="TokenCacheTtl"/> (9 min, matching NWP token lifetime).
    /// Replaces <c>GetToken()</c> + <c>System.Runtime.Caching.MemoryCache.Default</c>.
    /// </summary>
    private async Task<string> GetTokenAsync(RumConfig config, CancellationToken ct)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached)) return cached!;

        string tokenUrl = $"{config.TokenUrl}/connect/token";
        var form = new Dictionary<string, string>
        {
            ["client_id"]     = config.ClientId,
            ["client_secret"] = config.ApiSecret,
            ["grant_type"]    = "client_credentials",
            ["scope"]         = NwpTokenScope
        };

        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(form), ct);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"ManageProductRumAsync.GetTokenAsync - HTTP {(int)response.StatusCode} - {err}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
            ?? throw new InvalidOperationException("ManageProductRumAsync.GetTokenAsync - Empty token response.");

        if (!dict.TryGetValue("access_token", out var rawToken))
            throw new InvalidOperationException("ManageProductRumAsync.GetTokenAsync - access_token not present.");

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
        return maps?.FirstOrDefault()?.CompanyInstanceSourceId is string s && int.TryParse(s, out int id) ? id : 0;
    }

    // ── Private — RUM API helpers ─────────────────────────────────────────────

    private async Task<IList<RumPropertyGroup>> GetRumPropertiesDataAsync(
        string token, RumConfig config, long companyInstanceSourceId, string type, CancellationToken ct)
    {
        string url    = $"{config.ApiEndpoint}/identity/AccessItems?portfolioId={companyInstanceSourceId}&accessTypeCd={type}";
        var    result = await GetFromApiAsync<IList<dynamic>>(token, url, ct, throwOnError: false);
        _logger.LogDebug("{Action} - type={Type} Url={Url}", "GetRumPropertiesData", type, url);

        if (result is null) return [];

        return result
            .Select(x => new RumPropertyGroup { Id = (string)x.AccessId, Name = (string)x.AccessName, IsAssigned = false })
            .ToList();
    }

    private async Task<IList<Role>?> GetRumRolesAsync(
        string token, RumConfig config, long companyInstanceSourceId, ProductCallContext ctx, CancellationToken ct)
    {
        bool isContractCompany = ctx.EditorPersona.Organization.RealPageId == config.ContractCompanyRealPageId;
        string url = isContractCompany
            ? $"{config.ApiEndpoint}/roleoptions/GetRolesForType?userType=su"
            : $"{config.ApiEndpoint}/roleoptions/get?companyId={companyInstanceSourceId}";

        _logger.LogDebug("{Action} - Url={Url}", "GetRumRoles", url);
        var result = await GetFromApiAsync<IList<dynamic>>(token, url, ct, throwOnError: false);
        if (result is null) return null;

        var roles = new List<Role>(result.Count);
        int idx   = 0;
        foreach (var x in result)
        {
            bool internalOnly = (bool)x.InternalOnly;
            roles.Add(internalOnly
                ? new Role { Id = (int)x.RoleId, Description = (string)x.RoleDescription, Name = (string)x.RoleName, IsAssigned = false }
                : new Role { Id = idx + 101,      Description = (string)x.RoleDescription, Name = (string)x.RoleName, IsAssigned = false });
            idx++;
        }
        return roles;
    }

    private async Task<RumUserClaims?> GetRumUserClaimsAsync(
        string token, RumConfig config, string productUserId, CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/user/getuser?userId={productUserId}";
        return await GetFromApiAsync<RumUserClaims>(token, url, ct, throwOnError: false);
    }

    private async Task<bool> CheckUserExistsInRumAsync(
        string token, RumConfig config, string loginName, CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/user/userexists?userName={loginName}";
        using var client   = CreateBearerClient(token);
        using var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode) return false;
        var body = await response.Content.ReadAsStringAsync(ct);
        return bool.TryParse(body.Trim().Trim('"'), out bool exists) && exists;
    }

    private async Task<string> DeleteRumUserAsync(
        string token, RumConfig config, string productUserId, long editorPersonaId, CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/user/deleteuser?userId={productUserId}";
        _logger.LogDebug("{Action} - DELETE at {Url}", "DeleteRumUser", url);

        using var client   = CreateBearerClient(token);
        var response       = await client.DeleteAsync(url, ct);

        if (response.IsSuccessStatusCode) return string.Empty;

        var errMsg = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("{Action} - Delete failed. Status={Status} Msg={Msg}", "DeleteRumUser", response.StatusCode, errMsg);
        return $"There was a problem deleting Rum user with editorPersona id - {editorPersonaId} - Error-{errMsg}.";
    }

    private async Task ReActivateRumUserAsync(
        string token, RumConfig config, string productUserId, long editorPersonaId, CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/user/reactivateuser?userId={productUserId}";
        _logger.LogDebug("{Action} - POST reactivate at {Url}", "ReActivateRumUser", url);

        using var client   = CreateBearerClient(token);
        var content        = Serialize(new { });
        var response       = await client.PostAsync(url, content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errMsg = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("{Action} - Reactivate failed. EditorPersonaId={Id} Msg={Msg}", "ReActivateRumUser", editorPersonaId, errMsg);
        }
    }

    private async Task UpdateInactiveUserAsync(
        string token, RumConfig config, string productUserId, long editorPersonaId, CancellationToken ct)
    {
        var rumUser = await GetRumUserClaimsAsync(token, config, productUserId, ct);
        if (rumUser is null) return;

        var crmStatus = rumUser.Claims
            .Where(c => c.Type == "crmstatus")
            .Select(c => c.Value)
            .FirstOrDefault();

        _logger.LogDebug("{Action} - CrmStatus={Status}. EditorPersonaId={Id}", "UpdateInactiveUser", crmStatus, editorPersonaId);

        if (crmStatus == "Inactive")
            await ReActivateRumUserAsync(token, config, productUserId, editorPersonaId, ct);
    }

    private async Task<string> InsertRumProductUserAsync(
        string token, RumConfig config, long userPersonaId, long editorPersonaId,
        string productLoginName, RumUser rumUser, int companyId, CancellationToken ct)
    {
        string url = $"{config.ApiEndpoint}/user/postuser";
        _logger.LogDebug("{Action} - POST create user at {Url}. EditorPersonaId={Id}", "InsertRumProductUser", url, editorPersonaId);

        using var client   = CreateBearerClient(token);
        var content        = Serialize(rumUser);
        var response       = await client.PostAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            var json    = await response.Content.ReadAsStringAsync(ct);
            dynamic? id = JsonConvert.DeserializeObject<dynamic>(json);
            if (id is not null)
            {
                string newId = Convert.ToString((object)id)!;
                await _samlService.UpsertAttributesAsync(userPersonaId, ProductId, new Dictionary<SamlAttributeEnum, string>
                {
                    [SamlAttributeEnum.productUsername] = productLoginName,
                    [SamlAttributeEnum.UserId]          = newId,
                    [SamlAttributeEnum.NWPUserType]     = rumUser.UserTypeCode
                }, ct);

                _logger.LogDebug("{Action} - User created. NewId={NewId} Login={Login}", "InsertRumProductUser", newId, productLoginName);
                await _productRepository.UpdateProductSettingProductStatusAsync(
                    userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Success, ct);
            }
            return string.Empty;
        }

        string errorBody = string.Empty;
        try { errorBody = await response.Content.ReadAsStringAsync(ct); } catch { /* ignored */ }

        _logger.LogError("{Action} - Create failed. EditorPersonaId={Id} Error={Err}", "InsertRumProductUser", editorPersonaId, errorBody);
        await _productRepository.UpdateProductSettingProductStatusAsync(
            userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Error, ct);
        return $"There was a problem creating the user with editorPersona id - {editorPersonaId}. Error-{errorBody}";
    }

    private async Task<string> UpdateRumProductUserAsync(
        string token, RumConfig config, string productUserId,
        long userPersonaId, long editorPersonaId, RumUser rumUser, CancellationToken ct)
    {
        // Re-activate if the user is currently inactive
        await UpdateInactiveUserAsync(token, config, productUserId, editorPersonaId, ct);

        string url = $"{config.ApiEndpoint}/user/putuser?userId={productUserId}";
        _logger.LogDebug("{Action} - PUT update user at {Url}. EditorPersonaId={Id}", "UpdateRumProductUser", url, editorPersonaId);

        using var client   = CreateBearerClient(token);
        var content        = Serialize(rumUser);
        var response       = await client.PutAsync(url, content, ct);

        if (response.IsSuccessStatusCode)
        {
            await _samlService.UpsertAttributeAsync(userPersonaId, ProductId, SamlAttributeEnum.NWPUserType, rumUser.UserTypeCode, ct);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, ProductId, "ProductStatus", (int)ProductBatchStatusType.Success, ct);
            return string.Empty;
        }

        string errorBody = string.Empty;
        try { errorBody = await response.Content.ReadAsStringAsync(ct); } catch { /* ignored */ }

        _logger.LogError("{Action} - Update failed. EditorPersonaId={Id} Error={Err}", "UpdateRumProductUser", editorPersonaId, errorBody);
        return $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorBody}.";
    }

    // ── Private — merge helpers ───────────────────────────────────────────────

    private async Task<ListResponse> MergeRumPropertiesWithGreenbookAsync(
        string token, RumConfig config, IList<RumPropertyGroup> allPropertyGroups,
        string productUserId, CancellationToken ct)
    {
        var rumUser = await GetRumUserClaimsAsync(token, config, productUserId, ct);
        if (rumUser is null)
        {
            _logger.LogError("{Action} - User not found. ProductUserId={Id}", "MergeRumPropertiesWithGreenbook", productUserId);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        var claims        = rumUser.Claims;
        string accessLevel = claims.Where(c => c.Type == "nwpusertype").Select(c => c.Value).FirstOrDefault() ?? "";

        string claimType = accessLevel switch
        {
            "RM" => "regionid",
            "GM" => "groupid",
            "PR" or "PM" => "propid",
            _ => string.Empty
        };

        var accessTypeDict = accessLevel switch
        {
            "RM" => new Dictionary<string, string> { ["accessType"] = "regionalGroup" },
            "GM" => new Dictionary<string, string> { ["accessType"] = "propertyGroup" },
            "PR" => new Dictionary<string, string> { ["accessType"] = "specificProperties" },
            "PM" => new Dictionary<string, string> { ["accessType"] = "allProperties" },
            _    => new Dictionary<string, string>()
        };

        if (!string.IsNullOrEmpty(claimType))
        {
            var assignedIds = claims
                .Where(c => c.Type == claimType)
                .Select(c => c.Value)
                .ToHashSet();

            foreach (var rpg in allPropertyGroups.Where(p => assignedIds.Contains(p.Id)))
                rpg.IsAssigned = true;
        }

        return new ListResponse
        {
            Records     = allPropertyGroups.Cast<object>().ToList(),
            TotalRows   = allPropertyGroups.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1,
            Additional  = accessTypeDict
        };
    }

    private async Task<ListResponse> MergeRumGlobalRolesWithGreenbookAsync(
        string token, RumConfig config, IList<ProductRole> allRoles,
        string productUserId, CancellationToken ct)
    {
        var rumUser = await GetRumUserClaimsAsync(token, config, productUserId, ct);
        if (rumUser is null)
        {
            _logger.LogError("{Action} - User not found. ProductUserId={Id}", "MergeRumGlobalRolesWithGreenbook", productUserId);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        string accessLevel = rumUser.Claims
            .Where(c => c.Type == "nwpusertype")
            .Select(c => c.Value)
            .FirstOrDefault() ?? "";

        var matched = allRoles.FirstOrDefault(r => r.ID == accessLevel);
        if (matched is not null) matched.IsAssigned = true;

        return new ListResponse
        {
            Records     = allRoles.Cast<object>().ToList(),
            TotalRows   = allRoles.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1,
            Additional  = new Dictionary<string, string>()
        };
    }

    private async Task<ListResponse> MergeUserRolesWithProductRolesAsync(
        string token, RumConfig config, IList<Role> allRoles,
        string productUserId, CancellationToken ct)
    {
        var rumUser = await GetRumUserClaimsAsync(token, config, productUserId, ct);
        if (rumUser is null)
        {
            _logger.LogError("{Action} - User not found. ProductUserId={Id}", "MergeUserRolesWithProductRoles", productUserId);
            return new ListResponse { IsError = true, ErrorReason = "User not found." };
        }

        var assignedRoles = rumUser.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToHashSet();

        foreach (var role in allRoles.Where(r => assignedRoles.Contains(r.Name)))
            role.IsAssigned = true;

        return new ListResponse
        {
            Records     = allRoles.Cast<object>().ToList(),
            TotalRows   = allRoles.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1,
            Additional  = string.Empty
        };
    }

    // ── Private — audit helpers ───────────────────────────────────────────────

    /// <summary>
    /// Captures roles, properties, property groups and access type in parallel.
    /// Replaces serial <c>GetUserAccountableData</c> in the sync implementation.
    /// </summary>
    private async Task<Dictionary<string, List<object>>> GetUserAccountableDataAsync(
        string token, RumConfig config, ProductCallContext ctx,
        long editorPersonaId, long userPersonaId, CancellationToken ct)
    {
        var rolesTask   = GetRolesAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
        var propsTask   = GetPropertiesAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
        var groupsTask  = GetPropertyGroupsAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);
        var accessTask  = GetUMGlobalRolesAsync(editorPersonaId, userPersonaId, new RequestParameter(), ct);

        await Task.WhenAll(rolesTask, propsTask, groupsTask, accessTask);

        var data = new Dictionary<string, List<object>>();

        // "old" or "new" prefix is baked into the key by the caller via reflection of the state
        // Use a neutral key pattern — BuildActivityLogs will match by "Roles", "Properties", etc.
        if (rolesTask.Result.Records is not null)
            data["Roles"] = rolesTask.Result.Records.ToList();
        if (propsTask.Result.Records is not null)
            data["Properties"] = propsTask.Result.Records.ToList();
        if (groupsTask.Result.Records is not null)
            data["PropertyGroups"] = groupsTask.Result.Records.ToList();
        if (accessTask.Result.Records is not null)
            data["AccessType"] = accessTask.Result.Records.ToList();

        return data;
    }

    /// <summary>
    /// Builds audit parameters by diffing the "before" and "after" snapshots.
    /// Pure static — no I/O.
    /// </summary>
    private static List<AdditionalParameters> BuildActivityLogs(
        Dictionary<string, List<object>> newData,
        Dictionary<string, List<object>> oldData,
        RumUser rumUser)
    {
        var result = new List<AdditionalParameters>();

        // ── Roles diff ────────────────────────────────────────────────────────
        var currentRoles = newData.GetValueOrDefault("Roles")?
            .Cast<Role>().Where(r => rumUser.Roles.Contains(r.Name)).ToList() ?? [];
        var oldRoles = oldData.GetValueOrDefault("Roles")?
            .Cast<Role>().Where(r => r.IsAssigned).ToList() ?? [];

        foreach (var r in oldRoles.Where(o => !currentRoles.Exists(c => c.Name == o.Name)))
            result.Add(new AdditionalParameters { Key = "Utility Management Roles", Value = $"{r.Name} was removed" });
        foreach (var r in currentRoles.Where(c => !oldRoles.Exists(o => o.Name == c.Name)))
            result.Add(new AdditionalParameters { Key = "Utility Management Roles", Value = $"{r.Name} was assigned" });

        // ── Access type diff ──────────────────────────────────────────────────
        var currentAccess = newData.GetValueOrDefault("AccessType")?
            .Cast<ProductRole>().Where(r => rumUser.UserTypeCode == r.ID).ToList() ?? [];
        var oldAccess = oldData.GetValueOrDefault("AccessType")?
            .Cast<ProductRole>().Where(r => r.IsAssigned).ToList() ?? [];

        foreach (var r in oldAccess.Where(o => !currentAccess.Exists(c => c.Name == o.Name)))
            result.Add(new AdditionalParameters { Key = "Utility Management Access Type", Value = $"{r.Name} was removed" });
        foreach (var r in currentAccess.Where(c => !oldAccess.Exists(o => o.Name == c.Name)))
            result.Add(new AdditionalParameters { Key = "Utility Management Access Type", Value = $"{r.Name} was assigned" });

        // ── Properties diff ───────────────────────────────────────────────────
        var assetSet = rumUser.AssetIds?.Select(id => id.ToString()).ToHashSet() ?? [];

        var currentProps = newData.GetValueOrDefault("Properties")?
            .Cast<RumPropertyGroup>().Where(p => assetSet.Contains(p.Id)).ToList() ?? [];
        var oldProps = oldData.GetValueOrDefault("Properties")?
            .Cast<RumPropertyGroup>().Where(p => p.IsAssigned).ToList() ?? [];

        foreach (var p in oldProps)
            result.Add(new AdditionalParameters { Key = "Utility Management Properties", Value = $"{p.Name} was removed" });
        foreach (var p in currentProps)
            result.Add(new AdditionalParameters { Key = "Utility Management Properties", Value = $"{p.Name} was assigned" });

        // ── Property groups diff ──────────────────────────────────────────────
        var currentGroups = newData.GetValueOrDefault("PropertyGroups")?
            .Cast<RumPropertyGroup>().Where(p => assetSet.Contains(p.Id)).ToList() ?? [];
        var oldGroups = oldData.GetValueOrDefault("PropertyGroups")?
            .Cast<RumPropertyGroup>().Where(p => p.IsAssigned).ToList() ?? [];

        foreach (var g in oldGroups)
            result.Add(new AdditionalParameters { Key = "Utility Management Property Group", Value = $"{g.Name} was removed" });
        foreach (var g in currentGroups)
            result.Add(new AdditionalParameters { Key = "Utility Management Property Group", Value = $"{g.Name} was assigned" });

        return result;
    }

    // ── Private — HTTP helpers ────────────────────────────────────────────────

    /// <summary>Central async GET helper replacing blocking <c>GetResultFromApi&lt;T&gt;</c>.</summary>
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

    /// <summary>Creates a per-request <see cref="HttpClient"/> with Bearer token header.</summary>
    private HttpClient CreateBearerClient(string token)
    {
        var client = _httpClientFactory.CreateClient("RUM");
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
}

/// <summary>Immutable RUM product config snapshot — safe to cache in <see cref="IMemoryCache"/>.</summary>
internal sealed record RumConfig(
    string ApiEndpoint,
    string ApiSecret,
    string ClientId,
    string TokenUrl,
    string SuperUserRoles,
    Guid   ContractCompanyRealPageId);
