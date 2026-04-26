using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;

/// <summary>
/// Native-async abstract base class for standard v1 product integrations.
/// Replaces <c>StandardV1ProductIntegration</c>.
/// <para>
/// <b>Async initialization pattern</b>: the constructor assigns dependencies only.
/// Call <see cref="InitAsync"/> once before using any public methods; the caller-side
/// factory (Phase 6) is responsible for this.
/// </para>
/// <para>
/// <b>Breaking changes vs sync base class:</b>
/// <list type="bullet">
///   <item><c>DefaultUserClaim</c> replaced by <see cref="IUserClaimsAccessor"/> — all
///     <c>_userClaims.*</c> reads route through the accessor.</item>
///   <item>Stored <c>HttpClient _httpClient</c> eliminated — per-call <see cref="ApiIntegrationAsync"/>
///     instances created via <see cref="IHttpClientFactory"/>.</item>
///   <item>5 × <c>.Result</c> blocking calls in <c>ApplyApiSecurity</c> → <c>await</c>.</item>
///   <item><c>RPObjectCache</c> in <c>CheckForOverrideCompanyIdForProduct</c> → <see cref="ICacheService"/>.</item>
///   <item><c>out List&lt;AdditionalParameters&gt;</c> in <c>CreateUser</c> / <c>UpdateUser</c>
///     → named-tuple returns (incompatible with <c>async</c>).</item>
///   <item>Serilog <c>Log.Logger.ForContext(...).Write(...)</c> → <see cref="ILogger{T}"/>
///     structured logging.</item>
/// </list>
/// </para>
/// </summary>
public abstract class StandardV1ProductIntegrationAsync : IManageProductIntegrationAsync
{
    // ── Audit-log message templates ───────────────────────────────────────────
    private const string ProductSettingTypeStatus          = "ProductStatus";
    private const string MsgRolesAssigned                  = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string MsgRolesRemoved                   = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string MsgPropertiesAssigned             = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string MsgPropertiesRemoved              = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";
    private const string MsgUserGroupsAssigned             = "{\"action\":\"Assigned\",\"value\":\"UserGroupName\"}";
    private const string MsgUserGroupsRemoved              = "{\"action\":\"Removed\",\"value\":\"UserGroupName\"}";
    private const string MsgPropertyGroupsAssigned         = "{\"action\":\"Assigned\",\"value\":\"PropertyGroupName\"}";
    private const string MsgPropertyGroupsRemoved          = "{\"action\":\"Removed\",\"value\":\"PropertyGroupName\"}";
    private const int    OverridePmcCacheSeconds            = 300;

    // ── Per-operation context (set in InitAsync) ──────────────────────────────
    protected int                       ProductId                           { get; }
    protected Guid                      CorrelationId                       { get; private set; }
    protected UserDetails               EditorUserDetails                   { get; private set; } = null!;
    protected UserDetails?              SubjectUserDetails                  { get; private set; }
    protected GbProductMap              BlueBookGbProductMap                { get; private set; } = null!;
    protected string                    CompanyInstanceSourceId             { get; private set; } = string.Empty;
    protected string                    ProductApiBaseUrl                   { get; private set; } = string.Empty;
    protected IList<ProductInternalSetting> ProductInternalSettingList      { get; private set; } = [];
    protected bool                      ProductAcceptsUniqueProductUserName { get; private set; }
    protected bool                      CreateUpdateMultiCompanyUserRequiresPMC { get; private set; }
    protected bool                      ProductNotAvailableForRegularUserNoEmail { get; private set; }

    // ── Per-instance auth (built during InitAsync / ResolveApiSecurityAsync) ──
    private AuthenticationHeaderValue?             _authHeader;
    private IReadOnlyDictionary<string, string>    _apiAdditionalHeaders = new Dictionary<string, string>();

    // ── Dependencies ──────────────────────────────────────────────────────────
    private   readonly long                                   _editorPersonaId;
    private   readonly long                                   _subjectPersonaId;
    protected readonly IDataCollectorAsync                    _dataCollector;
    protected readonly IProductRepositoryAsync                _productRepository;
    protected readonly IManagePersonaAsync                    _managePersona;
    protected readonly IManageUserLoginAsync                  _manageUserLogin;
    private   readonly IUserClaimsAccessor                    _userClaimsAccessor;
    private   readonly IHttpClientFactory                     _httpClientFactory;
    protected readonly ITokenHelperAsync                      _tokenHelper;
    private   readonly ICacheService                          _cacheService;
    private   readonly ILogger<StandardV1ProductIntegrationAsync> _logger;
    private   readonly ILogger<ApiIntegrationAsync>           _apiLogger;

    // ── Constructor (protected — callers use InitAsync before first use) ──────

    protected StandardV1ProductIntegrationAsync(
        int                                         productId,
        long                                        editorPersonaId,
        long                                        subjectPersonaId,
        IDataCollectorAsync                         dataCollector,
        IProductRepositoryAsync                     productRepository,
        IManagePersonaAsync                         managePersona,
        IManageUserLoginAsync                       manageUserLogin,
        IUserClaimsAccessor                         userClaimsAccessor,
        IHttpClientFactory                          httpClientFactory,
        ITokenHelperAsync                           tokenHelper,
        ICacheService                               cacheService,
        ILoggerFactory                              loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(dataCollector);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(managePersona);
        ArgumentNullException.ThrowIfNull(manageUserLogin);
        ArgumentNullException.ThrowIfNull(userClaimsAccessor);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(tokenHelper);
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        ProductId          = productId;
        _editorPersonaId   = editorPersonaId;
        _subjectPersonaId  = subjectPersonaId;
        _dataCollector     = dataCollector;
        _productRepository = productRepository;
        _managePersona     = managePersona;
        _manageUserLogin   = manageUserLogin;
        _userClaimsAccessor = userClaimsAccessor;
        _httpClientFactory = httpClientFactory;
        _tokenHelper       = tokenHelper;
        _cacheService      = cacheService;
        _logger            = loggerFactory.CreateLogger<StandardV1ProductIntegrationAsync>();
        _apiLogger         = loggerFactory.CreateLogger<ApiIntegrationAsync>();
    }

    // ── Async initialization ──────────────────────────────────────────────────

    /// <summary>
    /// Performs the async equivalent of the sync constructor <c>Init()</c> call.
    /// Must be awaited once before calling any public methods.
    /// The Phase-6 factory is responsible for invoking this.
    /// </summary>
    public async Task InitAsync(CancellationToken ct = default)
    {
        CorrelationId = _userClaimsAccessor.CorrelationId;
        await ValidateAndLoadUserDetailsAsync(_editorPersonaId, _subjectPersonaId, ct);
        await LoadProductEndpointDetailsAsync(ct);
        await LoadBlueBookAndCompanyDetailsAsync(_subjectPersonaId, ct);
        (_authHeader, _apiAdditionalHeaders) = await ResolveApiSecurityAsync(ct);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // IManageProductIntegrationAsync — public methods
    // ══════════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetProductRolesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);

            if (baseUrlAndQuery.Contains("{0}"))
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

            var roleList = await GetResultFromApiAsync<IList<ProductRole>>(baseUrlAndQuery, ct: ct);
            if (roleList is null) throw new InvalidOperationException("Null role list from product API.");

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                var user = await GetProductUserAsync(ct: ct);
                if (user is not null)
                    MergeUserRoles(roleList, user.Roles);
            }

            return ToListResponse(roleList);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductRolesAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetProductRightsForRoleAsync(
        RequestParameter dataFilter,
        string roleId,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleRightsEndpoint);
            baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, roleId);

            var rights = await GetResultFromApiAsync<IList<ProductRight>>(baseUrlAndQuery, ct: ct)
                         ?? [];
            return ToListResponse(rights);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductRightsForRoleAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetAllRightsAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRightsEndpoint);

            if (baseUrlAndQuery.Contains("{0}"))
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

            var rights = await GetResultFromApiAsync<IList<ProductRight>>(baseUrlAndQuery, ct: ct)
                         ?? [];
            return ToListResponse(rights);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetAllRightsAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetProductPropertiesAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
            baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

            var properties = await GetResultFromApiAsync<IList<ProductProperties>>(baseUrlAndQuery, ct: ct)
                             ?? [];

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                var user = await GetProductUserAsync(ct: ct);
                if (user is not null)
                    MergeUserProperties(properties, user.Properties);
            }

            return ToListResponse(properties);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductPropertiesAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetProductPropertyGroupsAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupEndpoint);

            if (baseUrlAndQuery.Contains("{0}"))
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

            var groups = await GetResultFromApiAsync<IList<ProductPropertyGroups>>(baseUrlAndQuery, ct: ct)
                         ?? [];

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                var user = await GetProductUserAsync(ct: ct);
                if (user is not null)
                    MergeUserPropertyGroups(groups, user);
            }

            return ToListResponse(groups);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductPropertyGroupsAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetProductPropertiesByGroupAsync(
        string groupId,
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertiesByGroupEndpoint);
            baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, groupId);

            var properties = await GetResultFromApiAsync<IList<ProductProperties>>(baseUrlAndQuery, ct: ct)
                             ?? [];
            return ToListResponse(properties);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductPropertiesByGroupAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual Task<ListResponse> GetProductOrganizationsAsync(
        string organizationRoleId,
        string organizationType,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
        => throw new NotImplementedException(
               $"{GetType().Name} does not implement {nameof(GetProductOrganizationsAsync)}.");

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetProductUserGroupsAsync(
        RequestParameter dataFilter,
        string? baseUrlAndQuery = null,
        CancellationToken ct = default)
    {
        try
        {
            baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserGroupEndpoint);

            if (baseUrlAndQuery.Contains("{0}"))
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

            var groups = await GetResultFromApiAsync<IList<ProductUserGroup>>(baseUrlAndQuery, ct: ct)
                         ?? [];

            if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                var user = await GetProductUserAsync(ct: ct);
                if (user is not null)
                    MergeUserGroup(groups, user.UserGroups);
            }

            return ToListResponse(groups);
        }
        catch (Exception ex)
        {
            LogError(ex, nameof(GetProductUserGroupsAsync));
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IntegrationProductUser?> GetProductUserAsync(
        string? baseUrlAndQuery = null,
        bool isThrowOnError = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(baseUrlAndQuery))
        {
            baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

            if (baseUrlAndQuery.Contains("{1}"))
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails?.ProductUserName);
            else
                baseUrlAndQuery = string.Format(baseUrlAndQuery, SubjectUserDetails?.ProductUserName);
        }

        return await GetResultFromApiAsync<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError, ct);
    }

    /// <inheritdoc/>
    public virtual async Task<(string result, List<AdditionalParameters> auditParams)> CreateUpdateProductUserAsync(
        ProductUserRolePropertiesGroups userRolePropertiesRegion,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken ct = default)
    {
        LogDebug(nameof(CreateUpdateProductUserAsync), "Beginning of method");

        var newProductUser = await GenerateProductUserObjectAsync(userRolePropertiesRegion, ct);

        // Derive login name for no-email users
        if (SubjectUserDetails?.UserRoleTypeId == (int)UserRoleType.UserNoEmail
            && !ProductNotAvailableForRegularUserNoEmail
            && string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
        {
            newProductUser.LoginName = newProductUser.Email;
            var newLoginName = await GetUniqueProductLoginNameAsync(SubjectUserDetails, ct);
            if (string.IsNullOrEmpty(newLoginName))
                return ("An error occurred. Unable to get username.", []);
            newProductUser.LoginName = newLoginName;
        }

        // Check whether user already exists in product
        string loginName = !string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName)
            ? SubjectUserDetails.ProductUserName
            : newProductUser.LoginName;
        var existingUser = await GetBaseUserDataFromProductAsync(loginName, ct: ct);
        bool isProductUser = existingUser is { LoginName: { Length: > 0 } };

        // Strip special characters from name fields (product API constraint)
        newProductUser.FirstName = Regex.Replace(newProductUser.FirstName ?? string.Empty, @"[^A-Za-z0-9]+", string.Empty);
        newProductUser.LastName  = Regex.Replace(newProductUser.LastName  ?? string.Empty, @"[^A-Za-z0-9]+", string.Empty);

        if (isProductUser)
        {
            newProductUser.UserId    = existingUser!.UserId;
            newProductUser.LoginName = existingUser.LoginName;

            string? iterateFlag = ProductInternalSettingList
                .FirstOrDefault(s => s.Name.Equals("IterateUserNameRequiredForUserCreation", StringComparison.OrdinalIgnoreCase))?.Value;

            if (iterateFlag == "1" && string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            {
                isProductUser = false;
                newProductUser.LoginName = await IterateUserNameIfExistsAsync(newProductUser.LoginName, ct);
            }
        }

        // Apply super-user overrides
        if (SubjectUserDetails?.UserRoleTypeId == (int)UserRoleType.SuperUser)
            await ApplySuperUserSettingsAsync(newProductUser, ct);

        // Route to create / multi-company-create / update
        if (!isProductUser && string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
        {
            bool alreadyExists = await CheckUserExistInProductAsync(newProductUser.LoginName, ct: ct);
            if (alreadyExists)
            {
                LogError(null, nameof(CreateUpdateProductUserAsync),
                    $"User {newProductUser.LoginName} already exists in product {ProductId}.");
                return ($"{newProductUser.LoginName} already exist in the product {ProductId}.", []);
            }

            return await CreateUserAsync(newProductUser, batchProcessType, ct);
        }

        if (isProductUser && CreateUpdateMultiCompanyUserRequiresPMC)
            return await CreateMultiCompanyUserAsync(newProductUser, ct);

        newProductUser.UserId    = SubjectUserDetails?.ProductUserId;
        newProductUser.LoginName = SubjectUserDetails?.ProductUserName;
        return await UpdateUserAsync(newProductUser, batchProcessType, ct);
    }

    /// <inheritdoc/>
    public virtual async Task<string> ChangeProductUserTypeAsync(
        ProductUserRolePropertiesGroups rolePropertiesGroups,
        BatchProcessType batchProcessType,
        CancellationToken ct = default)
    {
        var (result, _) = await CreateUpdateProductUserAsync(rolePropertiesGroups, batchProcessType, ct);
        return result;
    }

    /// <inheritdoc/>
    public virtual async Task<string> UpdateProductUserProfileAsync(CancellationToken ct = default)
    {
        LogDebug(nameof(UpdateProductUserProfileAsync), "Beginning of method");

        var profile = new ProductUserProfile
        {
            LoginName    = SubjectUserDetails?.ProductUserName,
            FirstName    = SubjectUserDetails?.FirstName,
            MiddleName   = SubjectUserDetails?.MiddleName,
            LastName     = SubjectUserDetails?.LastName,
            Email        = SubjectUserDetails?.Email,
            PhoneNumbers = SubjectUserDetails?.PhoneNumbers,
            Phone        = SubjectUserDetails?.PhoneNumber,
            IsActive     = Convert.ToBoolean(SubjectUserDetails?.IsActive),
            UserId       = SubjectUserDetails?.ProductUserId,
            CompanyId    = CompanyInstanceSourceId
        };

        return await UpdateUserProfileAsync(profile, ct);
    }

    /// <inheritdoc/>
    public virtual async Task<string> UnassignUserAsync(CancellationToken ct = default)
    {
        LogDebug(nameof(UnassignUserAsync), "Beginning of method");

        var profile = new ProductUserProfile
        {
            UserId       = SubjectUserDetails?.ProductUserId,
            IsActive     = false,
            CompanyId    = CompanyInstanceSourceId,
            LoginName    = SubjectUserDetails?.ProductUserName,
            Email        = SubjectUserDetails?.Email,
            FirstName    = SubjectUserDetails?.FirstName,
            LastName     = SubjectUserDetails?.LastName,
            Phone        = SubjectUserDetails?.PhoneNumber,
            PhoneNumbers = SubjectUserDetails?.PhoneNumbers
        };

        var result = await DeleteUserAsync(profile, ct);

        if (result.IsSuccessStatusCode)
        {
            LogDebug(nameof(UnassignUserAsync), "DeleteUser succeeded — updating Unified Login status");

            var userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(SubjectUserDetails!.UserRealPageId, ct);
            var persona   = await _managePersona.GetPersonaAsync(SubjectUserDetails.PersonaId, withRights: false, ct);

            var orgStatus = await _manageUserLogin.GetUserOrganizationWithStatusAsync(
                userLogin.UserId, userLogin.LastLogin ?? DateTime.UtcNow, persona.OrganizationPartyId, getPrimaryOrg: false, ct);

            int statusValue = orgStatus.Status.ToString()
                .Equals(UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase)
                ? (int)UserUiStatusType.Deactivated
                : (int)UserUiStatusType.AccountHidden;

            await _dataCollector.UpdateProductSettingProductStatusAsync(
                SubjectUserDetails.PersonaId, ProductSettingTypeStatus, ProductId, statusValue, ct);

            return string.Empty;
        }

        LogError(null, nameof(UnassignUserAsync), $"DeleteUser failed for persona {SubjectUserDetails?.PersonaId}");
        return result.Content as string ?? "Unknown error";
    }

    /// <inheritdoc/>
    public virtual async Task<ListResponse> GetMigrationUsersAsync(
        RequestParameter dataFilter,
        CancellationToken ct = default)
    {
        var response = new ListResponse { IsError = true, ErrorReason = "No Users." };

        string filter       = dataFilter?.FilterBy?.GetValueOrDefault("filter") ?? "NonMigrated";
        int    startRow     = dataFilter?.Pages?.StartRow ?? 0;
        int    resultPerRow = dataFilter?.Pages?.ResultsPerPage ?? 1000;

        var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetListUsersEndpoint);
        baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, filter, startRow, resultPerRow);

        var result = await GetResultFromApiAsync<IList<IntegrationProductUser>>(baseUrlAndQuery, isThrowOnError: false, ct);

        if (result is null)
        {
            LogError(null, nameof(GetMigrationUsersAsync), "No users received from product");
            return response;
        }

        response.RowsPerPage = resultPerRow;
        response.ErrorReason = string.Empty;
        response.IsError     = false;
        response.TotalPages  = 1;
        response.Records     = result.Cast<object>().ToList();
        response.TotalRows   = result.Count;
        return response;
    }

    /// <inheritdoc/>
    public virtual async Task<MigrateResponse> UpdateUsersMigrationStatusAsync(
        IList<MigrateUser> migrateUsers,
        CancellationToken ct = default)
    {
        var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PatchMigrateUsersEndpoint);
        baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

        var api    = CreateApi(baseUrlAndQuery);
        var result = await api.PatchEntityAsync<MigrateResponse>(migrateUsers, ct: ct);

        if (result.IsSuccessStatusCode)
        {
            var migrationResponse = JsonConvert.DeserializeObject<MigrateResponse>(
                JsonConvert.SerializeObject(result.Content));
            LogDebug(nameof(UpdateUsersMigrationStatusAsync), "Success");
            return migrationResponse!;
        }

        LogError(null, nameof(UpdateUsersMigrationStatusAsync), "Error updating migration status");
        return new MigrateResponse { Status = false, Message = "Cannot update user status to migrated." };
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExternalProductUserProfileChangeAsync(
        ProductUserProfile productUserProfile,
        CancellationToken ct = default)
    {
        LogDebug(nameof(ExternalProductUserProfileChangeAsync), $"UserId={productUserProfile.UserId}");

        var result = await ProductUserProfileChangeAsync(productUserProfile, ct);
        if (result.IsSuccessStatusCode) return true;

        LogError(null, nameof(ExternalProductUserProfileChangeAsync),
            $"Profile change failed for UserId={productUserProfile.UserId}");
        return false;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Protected virtual — override in concrete product implementations
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates the user in the product API.
    /// Override when a product requires non-standard creation (e.g. PAM).
    /// </summary>
    protected virtual async Task<(string result, List<AdditionalParameters> auditParams)> CreateUserAsync(
        IntegrationProductUser productUser,
        BatchProcessType batchProcessType,
        CancellationToken ct)
    {
        var auditParams  = new List<AdditionalParameters>();
        var baseUrl      = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

        LogDebug(nameof(CreateUserAsync), baseUrl);

        var api    = CreateApi(baseUrl);
        var result = await api.PostEntityAsync<IntegrationProductUser>(productUser, ct: ct);

        // 400 "user already exists" → fall through to update
        if (!result.IsSuccessStatusCode && result.StatusCode == 400)
        {
            dynamic? parsed = JsonConvert.DeserializeObject(result.Content?.ToString() ?? string.Empty);
            if (parsed is not null)
            {
                string status = parsed["Status"]?.ToString() ?? string.Empty;
                if (status.Contains("user already exists", StringComparison.OrdinalIgnoreCase))
                {
                    string? userId = parsed["UserId"]?.ToString();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        productUser.UserId = userId;
                        return await UpdateUserAsync(productUser, batchProcessType, ct);
                    }
                }
            }
        }

        if (result.IsSuccessStatusCode)
        {
            LogDebug(nameof(CreateUserAsync), "Success — updating SAML mappings");

            await _dataCollector.CreateProductUserInGreenBookAsync(
                SubjectUserDetails!.PersonaId, result.Content!, ProductId, productUser, ct);

            await CreateAdditionalSamlUserAttributeAsync(SubjectUserDetails.PersonaId, ProductId, productUser, ct);
            await CreateAdditionalSamlUserAttributeForStandardIntegrationAsync(SubjectUserDetails.PersonaId, ProductId, productUser, ct);

            if (productUser.EmployeeAdditional is not null)
                await _dataCollector.AddUpdateEmployeeProductADGroupMappingAsync(
                    SubjectUserDetails.PersonaId, ProductId, productUser.EmployeeAdditional.AzureADGroupId, ct);

            string? isActivityCheckNotRequired = GetProductInternalSettingValue("IsActivityCheckNotRequired");
            if (isActivityCheckNotRequired != "1")
            {
                var products = await _productRepository.GetAllProductsAsync(ct);
                string? name = products.FirstOrDefault(p => p.ProductId == ProductId)?.Name;
                auditParams  = BuildAuditList(productUser.RoleList, productUser.Properties,
                                              productUser.UserGroups, productUser.PropertyGroups, name);
            }

            return (string.Empty, auditParams);
        }

        LogError(null, nameof(CreateUserAsync), $"Failed — StatusCode={result.StatusCode}");
        string? errorMsg = result.Content as string;
        return (string.IsNullOrWhiteSpace(errorMsg) ? "Unknown error" : errorMsg, auditParams);
    }

    /// <summary>
    /// Updates the user in the product API.
    /// Override when a product requires non-standard update behaviour (e.g. Knock).
    /// </summary>
    protected virtual async Task<(string result, List<AdditionalParameters> auditParams)> UpdateUserAsync(
        IntegrationProductUser productUser,
        BatchProcessType batchProcessType,
        CancellationToken ct)
    {
        var auditParams = new List<AdditionalParameters>();
        var baseUrl     = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);

        LogDebug(nameof(UpdateUserAsync), baseUrl);

        IntegrationProductUser? existingUser = null;
        string? isActivityCheckNotRequired   = GetProductInternalSettingValue("IsActivityCheckNotRequired");
        string? isActivateUserBeforeUpdate   = GetProductInternalSettingValue("IsActivateUserBeforeUpdate");

        if (isActivityCheckNotRequired != "1" && !string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
            existingUser = await GetProductUserAsync(ct: ct);

        if (isActivateUserBeforeUpdate == "1")
        {
            var status = await GetBaseUserDataFromProductAsync(productUser.LoginName!, ct: ct);
            if (status is not null)
            {
                productUser.IsActive = status.IsActive;
                if (!status.IsActive)
                    await UpdateProductUserProfileAsync(ct);
            }
        }

        var api    = CreateApi(baseUrl);
        var result = await api.PutEntityAsync<IntegrationProductUser>(productUser, ct: ct);

        if (result.IsSuccessStatusCode)
        {
            LogDebug(nameof(UpdateUserAsync), "Success — updating SAML mappings");

            if (batchProcessType is BatchProcessType.UserTypeAdminToRegular
                                 or BatchProcessType.UserTypeRegularToAdmin
                                 or BatchProcessType.UserTypeAdminToExternal
                                 or BatchProcessType.UserTypeExternalToAdmin)
            {
                ProductActivityLogger.WriteUpdateUserTypeActivityLog(
                    EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name,
                    BlueBookGbProductMap.BooksProductCode, CorrelationId, batchProcessType);
            }

            string? assignBySetting = GetProductInternalSettingValue("AssignSamlAttributeBySetting");
            if (assignBySetting?.Equals("1", StringComparison.OrdinalIgnoreCase) == true)
                await _dataCollector.UpdateProductUserInGreenBookAsync(
                    SubjectUserDetails!.PersonaId, result.Content!, ProductId, productUser, ct);

            await _dataCollector.UpdateProductSettingProductStatusAsync(
                SubjectUserDetails!.PersonaId, ProductSettingTypeStatus, ProductId,
                (int)ProductBatchStatusType.Success, ct);

            if (!ProductAcceptsUniqueProductUserName)
                await UpdateSamlUserAttributeAsync(
                    SubjectUserDetails.PersonaId, ProductId,
                    productUser.UserId, productUser.LoginName, productUser.Email, ct);

            if (productUser.EmployeeAdditional is not null)
                await _dataCollector.AddUpdateEmployeeProductADGroupMappingAsync(
                    SubjectUserDetails.PersonaId, ProductId, productUser.EmployeeAdditional.AzureADGroupId, ct);

            if (isActivityCheckNotRequired != "1")
            {
                var products = await _productRepository.GetAllProductsAsync(ct);
                string? name = products.FirstOrDefault(p => p.ProductId == ProductId)?.Name;
                auditParams  = AssignedRoleAndPropertyNameList(existingUser, productUser, name);
            }

            return (string.Empty, auditParams);
        }

        LogError(null, nameof(UpdateUserAsync), $"Failed — StatusCode={result.StatusCode}");
        return (result.Content?.ToString() ?? "Unknown error", auditParams);
    }

    /// <summary>
    /// Override to create additional SAML attributes after user creation (e.g. PAM uses this).
    /// </summary>
    protected virtual Task CreateAdditionalSamlUserAttributeAsync(
        long personaId, int productId, IntegrationProductUser productUser, CancellationToken ct)
        => Task.CompletedTask;

    /// <summary>
    /// Override to update SAML attributes after user update (e.g. ILM uses this).
    /// </summary>
    protected virtual Task UpdateSamlUserAttributeAsync(
        long personaId, int productId,
        string? productUserId, string? productUserLoginName, string? productUserEmail,
        CancellationToken ct)
        => Task.CompletedTask;

    /// <summary>
    /// Override to apply super-user-specific data (e.g. admin flag).
    /// Base implementation sets <c>IsAdminUser = true</c>.
    /// </summary>
    protected virtual Task ApplySuperUserDataAsync(IntegrationProductUser productUser, CancellationToken ct)
    {
        productUser.IsAdminUser = true;
        return Task.CompletedTask;
    }

    /// <summary>Returns true if the login name is already registered in the product.</summary>
    protected virtual async Task<bool> CheckUserExistInProductAsync(
        string loginNameToCheck, string? baseUrlAndQuery = null, CancellationToken ct = default)
    {
        baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

        baseUrlAndQuery = baseUrlAndQuery.Contains("{1}")
            ? string.Format(baseUrlAndQuery, CompanyInstanceSourceId, loginNameToCheck)
            : string.Format(baseUrlAndQuery, loginNameToCheck);

        var user = await GetProductUserAsync(baseUrlAndQuery, isThrowOnError: false, ct);
        return user is { UserId: { Length: > 0 } };
    }

    /// <summary>
    /// Builds the <see cref="IntegrationProductUser"/> from subject details + role/property data.
    /// Override in product implementations that require custom field mapping.
    /// </summary>
    protected virtual async Task<IntegrationProductUser> GenerateProductUserObjectAsync(
        ProductUserRolePropertiesGroups userRolePropertiesRegion,
        CancellationToken ct)
    {
        var productUser = new IntegrationProductUser
        {
            LoginName                 = GetUniqueProductLogin(SubjectUserDetails?.LoginName ?? string.Empty),
            CompanyId                 = CompanyInstanceSourceId,
            FirstName                 = SubjectUserDetails?.FirstName,
            LastName                  = SubjectUserDetails?.LastName,
            Email                     = SubjectUserDetails?.Email,
            Phone                     = SubjectUserDetails?.PhoneNumber,
            PhoneNumbers              = SubjectUserDetails?.PhoneNumbers,
            IsActive                  = true,
            PropertyGroups            = userRolePropertiesRegion.PropertyGroupList,
            Properties                = userRolePropertiesRegion.PropertyList,
            Roles                     = userRolePropertiesRegion.RoleList?.ConvertAll(x => x.ToString()),
            PropertyRoles             = userRolePropertiesRegion.PropertyRoleList,
            OrganizationRoles         = userRolePropertiesRegion.OrganizationRoleList,
            CanReceiveMonthlyReport   = userRolePropertiesRegion.CanReceiveMonthlyReport,
            PropertyRoleList          = userRolePropertiesRegion.RolePropertiesList,
            RoleList                  = userRolePropertiesRegion.RoleList?.ConvertAll(x => x.ToString()),
            IsRealPageEmployee        = SubjectUserDetails?.IsRPEmployee ?? false,
            UserGroups                = userRolePropertiesRegion.UserGroups,
            IsMigratedUser            = true,
            UnifiedLoginUserID        = SubjectUserDetails?.UserId ?? 0,
            UnifiedLoginPersonaID     = SubjectUserDetails?.PersonaId ?? 0,
            RoleType                  = userRolePropertiesRegion.RoleType
        };

        if (SubjectUserDetails?.UserRoleTypeId == (int)UserRoleType.SuperUser)
            await ApplySuperUserDataAsync(productUser, ct);

        string? supportsEmployee = GetProductInternalSettingValue("SI_SupportsEmployeeCreation");
        if ((SubjectUserDetails?.IsRPEmployee == true) && supportsEmployee == "1")
            await ApplyEmployeeDataAsync(productUser, ct);

        return productUser;
    }

    // ── Protected helpers ─────────────────────────────────────────────────────

    protected string GetOperationEndPoint(ProductEntityEndpointKeyEnum entityType)
    {
        string? partial = ProductInternalSettingList
            .FirstOrDefault(s => s.Name.Equals(entityType.ToString(), StringComparison.OrdinalIgnoreCase))?.Value;

        if (string.IsNullOrEmpty(partial))
            throw new InvalidOperationException($"Unable to find product setting for endpoint key '{entityType}'.");

        return ProductApiBaseUrl + partial;
    }

    protected string? GetProductInternalSettingValue(string settingName)
        => ProductInternalSettingList
            .FirstOrDefault(s => s.Name.Equals(settingName, StringComparison.OrdinalIgnoreCase))?.Value;

    protected async Task<T?> GetResultFromApiAsync<T>(
        string url, bool isThrowOnError = true, CancellationToken ct = default) where T : class
    {
        LogDebug(nameof(GetResultFromApiAsync), url);
        return await CreateApi(url).GetEntityFromApiAsync<T>(isThrowOnError, ct);
    }

    protected void DumpApiCallInfoToDiagnosticLog(string url, object? payload = null)
    {
        if (payload is not null)
            _logger.LogDebug("[{Method}] Product={ProductId} URL={Url} Payload={Payload}",
                "API", ProductId, url, JsonConvert.SerializeObject(payload));
        else
            _logger.LogDebug("[{Method}] Product={ProductId} URL={Url}", "API", ProductId, url);
    }

    protected static void MergeUserRoles(IList<ProductRole> roleList, List<string>? userRoles)
    {
        if (userRoles is null) return;
        foreach (var role in roleList)
            if (userRoles.Contains(role.GetRoleId))
                role.IsAssigned = true;
    }

    protected static void MergeUserGroup(IList<ProductUserGroup> groupList, List<string>? userGroups)
    {
        if (userGroups is null) return;
        foreach (var g in groupList)
            if (userGroups.Contains(g.GetGroupId))
                g.IsAssigned = true;
    }

    protected virtual void MergeUserPropertyGroups(IList<ProductPropertyGroups> groupList, IntegrationProductUser user)
    {
        var userGroups = user.PropertyGroups;
        if (userGroups is null) return;
        foreach (var group in groupList)
            if (userGroups.Contains(group.GetGroupId.ToUpper()))
                group.IsAssigned = true;
    }

    protected static void MergeUserProperties(IList<ProductProperties> propertyList, List<string>? userProperties)
    {
        if (propertyList is null || userProperties is null) return;
        foreach (var prop in propertyList)
            if (userProperties.Contains(prop.GetPropertyId.ToUpper()))
                prop.IsAssigned = true;
    }

    protected static void MergeUserRights(IList<ProductRight> rightList, List<string>? userRights)
    {
        if (userRights is null) return;
        foreach (var right in rightList)
            if (userRights.Contains(right.GetRightId))
                right.IsAssigned = true;
    }

    protected static string GetUniqueProductLogin(string userName) => userName;

    // ── Private helpers ───────────────────────────────────────────────────────

    protected ApiIntegrationAsync CreateApi(string url)
        => new(_httpClientFactory, url, _apiLogger, _authHeader, _apiAdditionalHeaders);

    private async Task<IntegrationProductUser?> GetBaseUserDataFromProductAsync(
        string loginName, string? baseUrlAndQuery = null, CancellationToken ct = default)
    {
        baseUrlAndQuery ??= GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

        baseUrlAndQuery = baseUrlAndQuery.Contains("{1}")
            ? string.Format(baseUrlAndQuery, CompanyInstanceSourceId, loginName)
            : string.Format(baseUrlAndQuery, loginName);

        return await GetResultFromApiAsync<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError: false, ct);
    }

    private async Task<(string result, List<AdditionalParameters> auditParams)> CreateMultiCompanyUserAsync(
        IntegrationProductUser productUser, CancellationToken ct)
    {
        var baseUrl = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);
        LogDebug(nameof(CreateMultiCompanyUserAsync), baseUrl);

        var existingUser = await GetProductUserAsync(ct: ct);
        var api    = CreateApi(baseUrl);
        var result = await api.PutEntityAsync<IntegrationProductUser>(productUser, ct: ct);

        if (result.IsSuccessStatusCode)
        {
            await _dataCollector.CreateProductUserInGreenBookAsync(
                SubjectUserDetails!.PersonaId, result.Content!, ProductId, productUser, ct);

            await CreateAdditionalSamlUserAttributeAsync(SubjectUserDetails.PersonaId, ProductId, productUser, ct);
            await CreateAdditionalSamlUserAttributeForStandardIntegrationAsync(SubjectUserDetails.PersonaId, ProductId, productUser, ct);

            if (productUser.EmployeeAdditional is not null)
                await _dataCollector.AddUpdateEmployeeProductADGroupMappingAsync(
                    SubjectUserDetails.PersonaId, ProductId, productUser.EmployeeAdditional.AzureADGroupId, ct);

            var products  = await _productRepository.GetAllProductsAsync(ct);
            string? name  = products.FirstOrDefault(p => p.ProductId == ProductId)?.Name;
            var auditList = AssignedRoleAndPropertyNameList(existingUser, productUser, name);
            return (string.Empty, auditList);
        }

        LogError(null, nameof(CreateMultiCompanyUserAsync), $"Failed — StatusCode={result.StatusCode}");
        return (result.Content?.ToString() ?? "Unknown error", []);
    }

    private async Task<ApiResponse> DeleteUserAsync(ProductUserProfile profile, CancellationToken ct)
    {
        var baseUrl = GetOperationEndPoint(ProductEntityEndpointKeyEnum.DeleteUserEndpoint);

        if (baseUrl.Contains("{1}"))
            baseUrl = string.Format(baseUrl, CompanyInstanceSourceId, profile.UserId);
        else
            baseUrl = string.Format(baseUrl, profile.UserId);

        LogDebug(nameof(DeleteUserAsync), baseUrl);
        return await CreateApi(baseUrl).DeleteEntityAsync<ProductUserProfile>(ct: ct);
    }

    private async Task<string> UpdateUserProfileAsync(ProductUserProfile profile, CancellationToken ct)
    {
        var result = await ProductUserProfileChangeAsync(profile, ct);

        if (result.IsSuccessStatusCode)
        {
            await _dataCollector.UpdateProductSettingProductStatusAsync(
                SubjectUserDetails!.PersonaId, ProductSettingTypeStatus, ProductId,
                (int)ProductBatchStatusType.Success, ct);

            if (!ProductAcceptsUniqueProductUserName)
                await UpdateSamlUserAttributeAsync(
                    SubjectUserDetails.PersonaId, ProductId,
                    profile.UserId, profile.LoginName, profile.Email, ct);

            return string.Empty;
        }

        LogError(null, nameof(UpdateUserProfileAsync), $"Failed for UserId={profile.UserId}");
        return result.Content?.ToString() ?? "Unknown error";
    }

    /// <summary>
    /// Issues the product-API call that updates a user profile.
    /// Default: PATCH to <c>PatchProfileEndpoint</c>.
    /// Override in product implementations that use a different verb / URL (e.g. ClickPay uses PUT).
    /// </summary>
    protected virtual async Task<ApiResponse> ProductUserProfileChangeAsync(ProductUserProfile profile, CancellationToken ct)
    {
        var baseUrl = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PatchProfileEndpoint);
        LogDebug(nameof(ProductUserProfileChangeAsync), baseUrl);
        return await CreateApi(baseUrl).PatchEntityAsync<ProductUserProfile>(profile, ct: ct);
    }

    private async Task<string> GetUniqueProductLoginNameAsync(
        UserDetails subjectUser, CancellationToken ct)
    {
        bool foundUserName    = false;
        int  incrementor      = 0;
        string updatedLogin   = (subjectUser.FirstName.TrimWhiteSpace()[..1] + subjectUser.LastName.TrimWhiteSpace()).ToLower();
        string? newLoginName  = null;

        while (!foundUserName)
        {
            string candidate = incrementor == 0
                ? updatedLogin
                : updatedLogin + incrementor;

            bool exists = await CheckUserExistInProductAsync(candidate, ct: ct);
            if (!exists)
            {
                newLoginName   = candidate;
                foundUserName  = true;
            }
            else
            {
                incrementor++;
                if (incrementor > 9999)
                {
                    _logger.LogError("[{Method}] Product={Id} Unable to find a unique login name after 10000 attempts",
                        nameof(GetUniqueProductLoginNameAsync), ProductId);
                    return string.Empty;
                }
            }
        }

        return newLoginName ?? string.Empty;
    }

    private async Task<string> IterateUserNameIfExistsAsync(string loginName, CancellationToken ct)
    {
        bool   found       = false;
        int    incrementor = 0;
        string result      = loginName;

        while (!found)
        {
            if (await CheckUserExistInProductAsync(result, ct: ct))
            {
                incrementor++;
                string[] parts = loginName.Split('@');
                result = parts.Length == 2
                    ? $"{parts[0]}{incrementor}@{parts[1]}"
                    : loginName + incrementor;
            }
            else
            {
                found = true;
            }
        }

        return result;
    }

    private async Task ApplySuperUserSettingsAsync(IntegrationProductUser productUser, CancellationToken ct)
    {
        string? propsId      = GetProductInternalSettingValue("SuperUserPropertiesId");
        string? roleId       = GetProductInternalSettingValue("SuperUserRoleId");
        string? roleType     = GetProductInternalSettingValue("SuperUserRoleType");
        string? userGroupsId = GetProductInternalSettingValue("UserGroupsId");

        if (propsId is not null)     productUser.Properties = [propsId];
        if (roleId is not null)      productUser.Roles      = [roleId];
        if (roleType is not null)    productUser.RoleType   = roleType;

        if (userGroupsId is not null)
        {
            var baseUrl = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserGroupEndpoint);
            if (baseUrl.Contains("{0}"))
                baseUrl = string.Format(baseUrl, CompanyInstanceSourceId, "false");

            var groups = await GetResultFromApiAsync<IList<ProductUserGroup>>(baseUrl, ct: ct);
            productUser.UserGroups = groups?.Select(g => g.GetGroupId.ToString()).ToList() ?? [];
        }

        await ApplySuperUserDataAsync(productUser, ct);
    }

    private async Task ApplyEmployeeDataAsync(IntegrationProductUser productUser, CancellationToken ct)
    {
        var personaList      = await _managePersona.ListPersonaAsync(SubjectUserDetails!.UserRealPageId, ct);
        var employeePersona  = personaList.FirstOrDefault(
            p => p.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);

        if (employeePersona is null) return;

        productUser.EmployeeAdditional = new EmployeeAdditional { AzureADGroup = string.Empty };
        var adUserInfo = await _dataCollector.GetAzureUserDetailsAsync(SubjectUserDetails.UserId, ct);
        productUser.EmployeeAdditional.SAMAccountName = adUserInfo?.SamAccountName;

        var productAdGroups = await _productRepository.GetAdGroupsForProductAsync(ProductId, ct);
        if (productAdGroups.Count == 0) return;

        var companyPersonas = personaList
            .Where(p => p.OrganizationPartyId == SubjectUserDetails.OrganizationPartyId)
            .ToList();

        var orderedGroups  = productAdGroups.OrderBy(g => g.AssignmentOrder);
        var userAdGroups   = await _productRepository.GetAdGroupsForUserAsync(employeePersona.PersonaId, ct);
        var usedGroupIds   = new HashSet<int>();

        if (companyPersonas.Count > 1)
        {
            foreach (var persona in companyPersonas.Where(p => p.PersonaId != SubjectUserDetails.PersonaId))
            {
                var mapping = (await _dataCollector.GetEmployeeProductADGroupMappingAsync(persona.PersonaId, ProductId, ct))
                              .FirstOrDefault();
                if (mapping is not null)
                {
                    bool assigned = await _productRepository.IsProductAssignedAsync(persona.PersonaId, 8, ProductId, ct);
                    if (assigned) usedGroupIds.Add(mapping.ADGroupId);
                }
            }
        }

        foreach (var adGroupProduct in orderedGroups)
        {
            if (userAdGroups.All(g => g.ADGroupId != adGroupProduct.ADGroupId)) continue;
            if (usedGroupIds.Contains(adGroupProduct.ADGroupId)) continue;

            productUser.EmployeeAdditional.AzureADGroup   = adGroupProduct.ActiveDirectoryId.ToString();
            productUser.EmployeeAdditional.AzureADGroupId = adGroupProduct.ADGroupId;
            return;
        }

        throw new InvalidOperationException("No ADGroups available to assign to new product user.");
    }

    private async Task CreateAdditionalSamlUserAttributeForStandardIntegrationAsync(
        long personaId, int productId, IntegrationProductUser productUser, CancellationToken ct)
    {
        string? additionalAttrs = GetProductInternalSettingValue("SI_AdditionalSAMLUserAttributes");
        if (additionalAttrs is null) return;

        foreach (var attribute in additionalAttrs.Split(','))
        {
            if (attribute.Trim().ToUpperInvariant() == "PMCID")
                await _dataCollector.CreateSamlUserAttributeAsync(
                    personaId, productId, SamlAttributeEnum.PMCID, productUser.CompanyId ?? string.Empty, ct);
        }
    }

    // ── Init private helpers ──────────────────────────────────────────────────

    private async Task ValidateAndLoadUserDetailsAsync(
        long editorPersonaId, long subjectPersonaId, CancellationToken ct)
    {
        try
        {
            EditorUserDetails = await _dataCollector.GetUserDetailsByPersonaAsync(editorPersonaId, ProductId, ct)
                ?? throw new InvalidOperationException($"Editor persona {editorPersonaId} not found.");

            if (subjectPersonaId > 0)
                SubjectUserDetails = await _dataCollector.GetUserDetailsByPersonaAsync(subjectPersonaId, ProductId, ct);

            if (!await ValidateEditorUserAsync(EditorUserDetails, ct))
                throw new InvalidOperationException("Invalid Persona — editor user validation failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Method}] Product={Id} EditorPersona={EId}",
                nameof(ValidateAndLoadUserDetailsAsync), ProductId, editorPersonaId);
            throw;
        }
    }

    private async Task<bool> ValidateEditorUserAsync(UserDetails editorUserDetails, CancellationToken ct)
    {
        if (editorUserDetails.PersonaId == 0) return false;
        var editor = await _managePersona.GetPersonaAsync(editorUserDetails.PersonaId, withRights: false, ct);
        return editor is not null && editor.RealPageId == editorUserDetails.UserRealPageId;
    }

    private async Task LoadProductEndpointDetailsAsync(CancellationToken ct)
    {
        try
        {
            ProductInternalSettingList = await _productRepository.GetProductInternalSettingsAsync(ProductId, ct);

            string? apiEndPoint = ProductInternalSettingList
                .FirstOrDefault(s => s.Name.Equals("ApiEndPoint", StringComparison.OrdinalIgnoreCase))?.Value
                ?? throw new InvalidOperationException($"ApiEndPoint setting missing for product {ProductId}.");

            string? alternateEndPoint = ProductInternalSettingList
                .FirstOrDefault(s => s.Name.Equals("AlternateApiEndPoint", StringComparison.OrdinalIgnoreCase))?.Value;

            ProductApiBaseUrl = alternateEndPoint ?? apiEndPoint;

            CreateUpdateMultiCompanyUserRequiresPMC = ProductInternalSettingList
                .FirstOrDefault(s => s.Name.Equals("CreateUpdateMultiCompanyUserRequiresPMC", StringComparison.OrdinalIgnoreCase))
                ?.Value?.Trim() == "1";

            ProductNotAvailableForRegularUserNoEmail = ProductInternalSettingList
                .FirstOrDefault(s => s.Name.Equals("ProductNotAvailableForRegularUserNoEmail", StringComparison.OrdinalIgnoreCase))
                ?.Value?.Trim() == "1";

            ProductAcceptsUniqueProductUserName = ProductInternalSettingList
                .FirstOrDefault(s => s.Name.Equals("ProductAcceptsUniqueProductUserName", StringComparison.OrdinalIgnoreCase))
                ?.Value?.Trim() == "1";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Method}] Product={Id}", nameof(LoadProductEndpointDetailsAsync), ProductId);
            throw;
        }
    }

    private async Task LoadBlueBookAndCompanyDetailsAsync(long subjectPersonaId, CancellationToken ct)
    {
        try
        {
            BlueBookGbProductMap = await _dataCollector.GetBlueBookProductMapAsync(ProductId, ct)
                ?? throw new InvalidOperationException($"BlueBook product map not found for product {ProductId}.");

            string blueBookProductCode = BlueBookGbProductMap.UDMSourceCode?.Length > 0
                ? BlueBookGbProductMap.UDMSourceCode
                : BlueBookGbProductMap.BooksProductCode;

            int booksMasterId = subjectPersonaId != 0 && SubjectUserDetails is not null
                ? SubjectUserDetails.BooksCustomerMasterId
                : EditorUserDetails.BooksCustomerMasterId;

            string? overrideId = await CheckForOverrideCompanyIdForProductAsync(ct);
            if (!string.IsNullOrEmpty(overrideId))
            {
                CompanyInstanceSourceId = overrideId;
            }
            else
            {
                var companyMap = await _dataCollector.GetProductCompanyMapAsync(
                    blueBookProductCode, booksMasterId, EditorUserDetails.OrganizationDomain, ct);

                CompanyInstanceSourceId = companyMap?.CompanyInstanceSourceId
                    ?? throw new BlueBookException("Company Setup Error: Please Contact Support.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Method}] Product={Id}", nameof(LoadBlueBookAndCompanyDetailsAsync), ProductId);
            throw;
        }
    }

    private async Task<string?> CheckForOverrideCompanyIdForProductAsync(CancellationToken ct)
    {
        string cacheKey = $"orgProductSettings_{_userClaimsAccessor.OrganizationRealPageGuid}_{ProductId}";

        var cacheOptions = new CacheEntryOptions
        {
            ExpirationTimeInMinutes = OverridePmcCacheSeconds / 60,
            SkipDistributedCache    = true,
            SkipMemoryCache         = false
        };

        var orgSettings = await _cacheService.GetOrSetAsync<IList<ProductSettingList>>(
            cacheKey,
            async _ => await _productRepository.GetProductSettingsAsync(
                _userClaimsAccessor.OrganizationRealPageGuid, ProductId, ct),
            cacheOptions, ct) ?? [];

        return orgSettings
            .FirstOrDefault(s => s.Name.Equals("OverridePMCID", StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    /// <summary>
    /// Resolves the <see cref="AuthenticationHeaderValue"/> and any additional product headers
    /// from the loaded <see cref="ProductInternalSettingList"/>.
    /// Eliminates all <c>.Result</c> blocking calls from the sync <c>ApplyApiSecurity</c>.
    /// </summary>
    protected virtual async Task<(AuthenticationHeaderValue? authHeader, IReadOnlyDictionary<string, string> additionalHeaders)>
        ResolveApiSecurityAsync(CancellationToken ct)
    {
        AuthenticationHeaderValue? authHeader = null;
        var additional = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        string? tokenScopes  = GetProductInternalSettingValue("TokenAuthScopes");
        string? apiUser      = GetProductInternalSettingValue("ApiUserName");
        string? apiPassword  = GetProductInternalSettingValue("ApiPassword");
        string? apiSecret    = GetProductInternalSettingValue("ApiSecret");
        string? tokenUrl     = GetProductInternalSettingValue("TokenURL");
        string? clientId     = GetProductInternalSettingValue("ApiCode");
        string? apiKey       = GetProductInternalSettingValue("ApiKey");
        string? includeCoId  = GetProductInternalSettingValue("Kong-IncludeCompanyIdHeader");
        string? ignoreBasic  = GetProductInternalSettingValue("SI_IgnoreApiBasicAuthHeader");

        // ── Priority 1: UL server Bearer token ────────────────────────────────
        if (tokenScopes is not null)
        {
            string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync(tokenScopes, ct);
            authHeader = new AuthenticationHeaderValue("Bearer", token);
        }
        // ── Priority 2: External password-grant Bearer token ──────────────────
        else if (!string.IsNullOrEmpty(apiSecret) && !string.IsNullOrEmpty(clientId)
                 && !string.IsNullOrEmpty(tokenUrl))
        {
            // Password grant (resource-owner flow) — send via IHttpClientFactory
            using var client = _httpClientFactory.CreateClient(ApiIntegrationAsync.ClientName);
            using var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"]    = "password",
                    ["client_id"]     = clientId.Trim(),
                    ["client_secret"] = apiSecret.Trim(),
                    ["username"]      = apiUser?.Trim() ?? string.Empty,
                    ["password"]      = apiPassword?.Trim() ?? string.Empty
                })
            };
            req.Headers.Add("X-PrettyPrint", "1");

            var resp   = await client.SendAsync(req, ct);
            var json   = await resp.Content.ReadAsStringAsync(ct);
            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                         ?? throw new InvalidOperationException("Empty token response from product IdP.");
            authHeader = new AuthenticationHeaderValue("Bearer", values["access_token"]);
        }
        // ── Priority 3: HTTP Basic auth ───────────────────────────────────────
        else if (!string.IsNullOrWhiteSpace(apiUser) && !string.IsNullOrWhiteSpace(apiPassword)
                 && (string.IsNullOrWhiteSpace(ignoreBasic) || ignoreBasic == "0"))
        {
            authHeader = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiUser}:{apiPassword}")));
        }

        // ── Additional headers ────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(apiKey))
            additional["apikey"] = apiKey!;

        if (includeCoId == "1")
            additional["company-id"] = CompanyInstanceSourceId;

        return (authHeader, additional);
    }

    // ── Audit helpers ─────────────────────────────────────────────────────────

    private static List<AdditionalParameters> BuildAuditList(
        List<string>? addedRoles, List<string>? addedProperties,
        List<string>? addedGroups, List<string>? addedPropertyGroups,
        string? productName)
    {
        // For CreateUser the user is new — all assignments are "added"
        var audit = new List<AdditionalParameters>();
        if (addedRoles          is { Count: > 0 }) audit.AddRange(BuildList(addedRoles,          MsgRolesAssigned,          productName, "Roles"));
        if (addedProperties     is { Count: > 0 }) audit.AddRange(BuildList(addedProperties,     MsgPropertiesAssigned,     productName, "Properties"));
        if (addedGroups         is { Count: > 0 }) audit.AddRange(BuildList(addedGroups,          MsgUserGroupsAssigned,     productName, "UserGroups"));
        if (addedPropertyGroups is { Count: > 0 }) audit.AddRange(BuildList(addedPropertyGroups, MsgPropertyGroupsAssigned, productName, "PropertyGroups"));
        return audit;
    }

    private List<AdditionalParameters> AssignedRoleAndPropertyNameList(
        IntegrationProductUser? before, IntegrationProductUser after, string? productName)
    {
        try
        {
            var audit = new List<AdditionalParameters>();
            void Diff(List<string>? prev, List<string>? next, string assignMsg, string removeMsg, string key)
            {
                if (next is null) return;
                var added   = prev is null ? next : next.Except(prev).ToList();
                var removed = prev?.Except(next).ToList() ?? [];
                if (added.Count   > 0) audit.AddRange(BuildList(added,   assignMsg, productName, key));
                if (removed.Count > 0) audit.AddRange(BuildList(removed, removeMsg, productName, key));
            }

            Diff(before?.RoleList,       after.RoleList,       MsgRolesAssigned,          MsgRolesRemoved,          "Roles");
            Diff(before?.Properties,     after.Properties,     MsgPropertiesAssigned,     MsgPropertiesRemoved,     "Properties");
            Diff(before?.UserGroups,     after.UserGroups,     MsgUserGroupsAssigned,     MsgUserGroupsRemoved,     "UserGroups");
            Diff(before?.PropertyGroups, after.PropertyGroups, MsgPropertyGroupsAssigned, MsgPropertyGroupsRemoved, "PropertyGroups");
            return audit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Method}] Unable to build audit list for product {Name}",
                nameof(AssignedRoleAndPropertyNameList), productName);
            return [];
        }
    }

    private static List<AdditionalParameters> BuildList(
        List<string> ids, string jsonTemplate, string? productName, string key)
        => ids.Select(id => new AdditionalParameters
               { Key = $"{productName} {key}", Value = jsonTemplate.Replace("RoleName", id)
                                                                    .Replace("PropertyName", id)
                                                                    .Replace("UserGroupName", id)
                                                                    .Replace("PropertyGroupName", id) })
              .ToList();

    protected static ListResponse ToListResponse<T>(IList<T> records)
        => new()
        {
            Records     = records.Cast<object>().ToList(),
            TotalRows   = records.Count,
            RowsPerPage = 9999,
            TotalPages  = 1,
            ErrorReason = string.Empty
        };

    // ── Structured logging shorthands ─────────────────────────────────────────

    protected void LogDebug(string method, string state)
        => _logger.LogDebug("[{Method}] Product={ProductId} Editor={EditorId} {State}",
               method, ProductId, EditorUserDetails?.PersonaId, state);

    protected void LogError(Exception? ex, string method, string? state = null)
        => _logger.LogError(ex, "[{Method}] Product={ProductId} Editor={EditorId} {State}",
               method, ProductId, EditorUserDetails?.PersonaId, state ?? string.Empty);
}
