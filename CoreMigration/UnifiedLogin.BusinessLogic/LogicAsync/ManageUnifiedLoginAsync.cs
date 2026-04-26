using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Base;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.LogicAsync.Product.Integrations;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first replacement for <see cref="ManageUnifiedLogin"/>.
/// <para>Key improvements over the sync version:</para>
/// <list type="bullet">
///   <item>No longer inherits <c>ManageProductBase</c> — all state is resolved per-call from
///         injected services (thread-safe).</item>
///   <item>All <c>new XxxRepository()</c> calls replaced by injected async interfaces.</item>
///   <item>Serilog statics replaced by <see cref="ILogger{T}"/>.</item>
///   <item><c>MemoryCache.Default.Remove</c> replaced by <see cref="IMemoryCache.Remove"/>.</item>
///   <item>Context resolution delegated to <see cref="IProductContextServiceAsync"/>
///         (immutable <see cref="ProductCallContext"/> per call — no shared mutable fields).</item>
///   <item>Internal settings delegated to <see cref="IProductSettingServiceAsync"/>
///         (2-minute cache; removes direct repo dependency from this class).</item>
///   <item>User activity log info delegated to <see cref="IProductAuditServiceAsync"/>
///         (removes duplicate <c>GetUserActivityLogInfo</c> implementation).</item>
///   <item>Lumina-right filtering centralised in a single <c>ShouldFilterLuminaAsync</c>.</item>
///   <item><c>Guid != null</c> (always true) guard fixed to <c>!= Guid.Empty</c>.</item>
/// </list>
/// </summary>
public sealed class ManageUnifiedLoginAsync : IManageUnifiedLoginAsync
{
    // ════════════════════════════════════════════════════════════════════════
    // Fields + Constants
    // ════════════════════════════════════════════════════════════════════════

    #region Fields

    private readonly IProductRepositoryAsync _productRepo;
    private readonly IUserRoleRightRepositoryAsync _userRoleRightRepo;
    private readonly IUserRepositoryAsync _userRepo;
    private readonly IPropertyRepositoryAsync _propertyRepo;
    private readonly IUnifiedLoginRepositoryAsync _unifiedLoginRepo;
    private readonly IManageBlueBookAsync _blueBook;
    private readonly IManageUnifiedSettingsAsync _unifiedSettings;
    private readonly IProductContextServiceAsync _contextService;
    private readonly IProductSettingServiceAsync _settingService;
    private readonly IProductAuditServiceAsync _auditService;
    private readonly IUserClaimsAccessor _userClaimAccessor;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ManageUnifiedLoginAsync> _logger;

    private const string PRODUCT_ROLE_CREATE = "{\"action\":\"Created Role\",\"value\":\"RoleName\"}";
    private const string PRODUCT_ROLE_DELETE = "{\"action\":\"Deleted Role\",\"value\":\"RoleName\"}";
    private const string PRODUCT_ROLE_UPDATE = "{\"action\":\"Updated Role\",\"value\":\"RoleName\"}";
    private const string PRODUCT_ROLE_USERDEFAULT = "{\"action\":\"Role Set as User Default\",\"value\":\"RoleName\"}";
    private const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
    private const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
    private const string ROLE_ASSIGN = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
    private const string ROLE_UNASSIGN = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Constructor
    // ════════════════════════════════════════════════════════════════════════

    #region Constructor

    public ManageUnifiedLoginAsync(
        IProductRepositoryAsync productRepo,
        IUserRoleRightRepositoryAsync userRoleRightRepo,
        IUserRepositoryAsync userRepo,
        IPropertyRepositoryAsync propertyRepo,
        IUnifiedLoginRepositoryAsync unifiedLoginRepo,
        IManageBlueBookAsync blueBook,
        IManageUnifiedSettingsAsync unifiedSettings,
        IProductContextServiceAsync contextService,
        IProductSettingServiceAsync settingService,
        IProductAuditServiceAsync auditService,
        IUserClaimsAccessor userClaimAccessor,
        IMemoryCache cache,
        ILogger<ManageUnifiedLoginAsync> logger)
    {
        ArgumentNullException.ThrowIfNull(productRepo); _productRepo = productRepo;
        ArgumentNullException.ThrowIfNull(userRoleRightRepo); _userRoleRightRepo = userRoleRightRepo;
        ArgumentNullException.ThrowIfNull(userRepo); _userRepo = userRepo;
        ArgumentNullException.ThrowIfNull(propertyRepo); _propertyRepo = propertyRepo;
        ArgumentNullException.ThrowIfNull(unifiedLoginRepo); _unifiedLoginRepo = unifiedLoginRepo;
        ArgumentNullException.ThrowIfNull(blueBook); _blueBook = blueBook;
        ArgumentNullException.ThrowIfNull(unifiedSettings); _unifiedSettings = unifiedSettings;
        ArgumentNullException.ThrowIfNull(contextService); _contextService = contextService;
        ArgumentNullException.ThrowIfNull(settingService); _settingService = settingService;
        ArgumentNullException.ThrowIfNull(auditService); _auditService = auditService;
        ArgumentNullException.ThrowIfNull(userClaimAccessor); _userClaimAccessor = userClaimAccessor;
        ArgumentNullException.ThrowIfNull(cache); _cache = cache;
        ArgumentNullException.ThrowIfNull(logger); _logger = logger;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUnifiedLoginAsync — property reads
    // ════════════════════════════════════════════════════════════════════════

    #region GetPropertiesAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetPropertiesAsync(
        long editorPersonaId, long userPersonaId, bool assignedOnly,
        RequestParameter dataFilter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("GetPropertiesAsync → editorPersonaId={Id}", editorPersonaId);

        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, 0, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        long companyMasterId = ctx.EditorPersona.Organization?.BooksCustomerMasterId ?? 0;
        if (companyMasterId == 0)
        {
            _logger.LogError("GetPropertiesAsync → BooksCustomerMasterId=0 for editorPersonaId={Id}",
                editorPersonaId);
            return ProductManagerHelpers.ErrorResponse(CommonMessageConstants.CompanyErrorMessage);
        }

        IList<ProductProperty> customerProps = await _blueBook.GetCustomerPropertyAsync(
            companyMasterId, null, null, getCached: false, cancellationToken);

        return userPersonaId != 0
            ? await MergeProductPropertiesWithGreenbookAsync(customerProps, userPersonaId, assignedOnly, cancellationToken)
            : ToListResponse(customerProps);
    }

    #endregion

    #region GetEnterprisePropertiesAsync / GetCustomerPropertiesAsync / GetUPFMPropertiesAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetEnterprisePropertiesAsync(
        long userPersonaId, string? include = null, CancellationToken cancellationToken = default)
    {
        var settings = await _settingService.GetProductSettingAsync(
            (int)ProductEnum.UnifiedPlatform, cancellationToken);

        bool usePropertyInstances = settings
            .FirstOrDefault(s => s.Name.Equals(
                "UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase))
            ?.Value == "1";

        return usePropertyInstances
            ? await GetUPFMPropertiesAsync(userPersonaId, include, cancellationToken)
            : await GetCustomerPropertiesAsync(userPersonaId, include, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetCustomerPropertiesAsync(
        long userPersonaId, string? include = null, CancellationToken cancellationToken = default)
    {
        var userClaims = _userClaimAccessor.Current;

        List<ProductProperty> userPropList = await _propertyRepo.ListPropertiesByPersonaAsync(
            userPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);

        if (userPropList.Count == 0) return new ListResponse();

        var blueBookList = (await _blueBook.GetCustomerPropertyAsync(
            userClaims.CustomerMasterId, cancellationToken: cancellationToken)).ToList();

        List<ProductProperty> resultList = userPropList is [{ ID: "-1" }]
            ? blueBookList
            : blueBookList.FindAll(b => userPropList.Any(p => p.ID == b.ID));

        if (resultList.Count == 0) return new ListResponse();

        resultList = ApplyContractResolverIfNeeded(resultList, include);
        resultList.ForEach(p => { p.IsAssigned = null; p.disableSelection = null; });

        return ToListResponse(resultList);
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetUPFMPropertiesAsync(
        long editorPersonaId, long userPersonaId, bool assignedOnly,
        ProductEnum product, RequestParameter dataFilter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("GetUPFMPropertiesAsync → editorPersonaId={Id}", editorPersonaId);

        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, 0, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        var userClaims = _userClaimAccessor.Current;
        var booksGuids = await _blueBook.GetUPFMPropertyInstancesAsync(
            userClaims.OrganizationRealPageGuid.ToString(), cancellationToken);
        var customerProps = await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(
            booksGuids, cancellationToken);

        return userPersonaId != 0
            ? await MergeUPFMBooksPropertiesWithProductPropertyAsync(
                customerProps, userPersonaId, assignedOnly, cancellationToken)
            : ToListResponse(customerProps.Select(p =>
                ConvertUPFMPropertyInstanceToProductProperty(p, false)));
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetUPFMPropertiesAsync(
        long userPersonaId, string? include = null, CancellationToken cancellationToken = default)
    {
        var userClaims = _userClaimAccessor.Current;

        List<int> userPropertyIds = await _propertyRepo.ListUPFMPropertyInstanceIdByPersonaAsync(
            userPersonaId, ProductEnum.UnifiedPlatform, cancellationToken);

        if (userPropertyIds.Count == 0) return new ListResponse();

        var booksGuids = await _blueBook.GetUPFMPropertyInstancesAsync(
            userClaims.OrganizationRealPageGuid.ToString(), cancellationToken);

        List<UPFMPropertyInstance> customerList = booksGuids is { Count: > 0 }
            ? await _propertyRepo.ListUPFMPropertyInstanceIdByInstanceIdsAsync(booksGuids, cancellationToken)
            : [];

        List<ProductProperty> userPropList = userPropertyIds is [var id] && id == -1
            ? customerList.Select(cp => ConvertUPFMPropertyInstanceToProductProperty(cp, true)).ToList()
            : customerList
                .FindAll(b => userPropertyIds.Any(p => p == b.PropertyInstanceId))
                .Select(cp => ConvertUPFMPropertyInstanceToProductProperty(cp, true)).ToList();

        userPropList = ApplyContractResolverIfNeeded(userPropList, include);
        userPropList.ForEach(p => { p.IsAssigned = null; p.disableSelection = null; });

        return ToListResponse(userPropList);
    }

    #endregion

    #region GetProductInternalSettingByProductIdAsync

    /// <inheritdoc/>
    public Task<List<ProductInternalSetting>> GetProductInternalSettingByProductIdAsync(
        int productId, CancellationToken cancellationToken = default)
        => _settingService.GetProductSettingAsync(productId, cancellationToken);

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUnifiedLoginAsync — role management
    // ════════════════════════════════════════════════════════════════════════

    #region AddUpdateRoleAsync

    /// <inheritdoc/>
    public async Task<ListResponse> AddUpdateRoleAsync(
        long editorPersonaId, long partyId, long roleId, string roleName, string inheritRoleId,
        CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        var userClaims = _userClaimAccessor.Current;
        roleName = roleName.Trim();
        var response = new ListResponse();

        try
        {
            if (roleId == 0)
            {
                int roleTypeId = (int)UserRoleType.SuperUser;
                var catList = await _unifiedLoginRepo.GetCategoryTypeAsync(cancellationToken);
                int roleCategoryId = catList.First(c =>
                    c.CategoryName.ToUpper() == "ROLE TYPE" &&
                    c.Status.ToUpper() == "CUSTOM").StatusTypeid;

                var resp = await _unifiedLoginRepo.AddCustomRoleAsync(
                    roleName, string.Empty, roleTypeId, roleCategoryId,
                    partyId, userClaims.UserId, userClaims.OrganizationType, cancellationToken);

                if (!string.IsNullOrWhiteSpace(resp.ErrorMessage))
                { response.IsError = true; response.ErrorReason = resp.ErrorMessage; }

                if (!response.IsError)
                    await AddUpdateRoleLogMessageAsync(editorPersonaId, partyId, roleName,
                        "ADD", "Unified Platform", null, 3, cancellationToken);

                response.Records = [resp.Id];
            }
            else
            {
                string oldRoleName = await GetRoleNameAsync(
                    roleId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
                var resp = await _unifiedLoginRepo.UpdateCustomRoleAsync(
                    roleId, roleName, string.Empty, userClaims.UserId, cancellationToken);

                if (!string.IsNullOrWhiteSpace(resp.ErrorMessage))
                { response.IsError = true; response.ErrorReason = resp.ErrorMessage; }

                if (!response.IsError && oldRoleName != roleName)
                    await AddUpdateRoleLogMessageAsync(editorPersonaId, partyId, roleName,
                        "UPDATE", "Unified Platform", oldRoleName, 3, cancellationToken);

                response.Records = [resp.Id];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddUpdateRoleAsync failed editorPersonaId={Id}", editorPersonaId);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    #endregion

    #region DeleteRoleAsync

    /// <inheritdoc/>
    public async Task<ListResponse> DeleteRoleAsync(
        long editorPersonaId, long roleId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("DeleteRoleAsync → editorPersonaId={Id} roleId={RId}", editorPersonaId, roleId);

        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        string roleName = await GetRoleNameAsync(roleId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        var response = new ListResponse();

        try
        {
            var resp = await _unifiedLoginRepo.DeleteRoleAsync(roleId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(resp.ErrorMessage))
            { response.IsError = true; response.ErrorReason = resp.ErrorMessage; }

            response.Records = [resp.Id];
            if (!response.IsError)
                await DeleteRoleLogMessageAsync(editorPersonaId, roleId, roleName,
                    "Unified Platform", 3, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteRoleAsync failed for roleId={Id}", roleId);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    #endregion

    #region SetDefaultRoleAsync

    /// <inheritdoc/>
    public async Task<ListResponse> SetDefaultRoleAsync(
        long editorPersonaId, long partyId, long roleId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        var response = new ListResponse();
        try
        {
            var resp = await _unifiedLoginRepo.SetDefaultRoleAsync(
                roleId, partyId, _userClaimAccessor.Current.UserId, cancellationToken);

            if (!string.IsNullOrWhiteSpace(resp.ErrorMessage))
            { response.IsError = true; response.ErrorReason = resp.ErrorMessage; }

            response.Records = [resp.Id];
            if (!response.IsError)
                await SetDefaultRoleLogMessageAsync(editorPersonaId, partyId, roleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SetDefaultRoleAsync failed for roleId={Id}", roleId);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    #endregion

    #region GetRolesAsync / GetRolesWithCountAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId, long partyId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIds = await GetProductIdsByOrgAsync(cancellationToken);
            var roles = await _unifiedLoginRepo.ListRolesForProductsByPartyIdAsync(
                partyId, (int)ProductEnum.UnifiedPlatform, productIds, cancellationToken);
            return ToListResponse(roles.OrderBy(r => r.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UnifiedLogin - There was a problem getting the roles.");
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesWithCountAsync(
        long editorPersonaId, long partyId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, 0, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIds = await GetProductIdsByOrgAsync(cancellationToken);
            var allRoleRights = await _unifiedLoginRepo.ListRoleWithRightsAsync(
                partyId, (int)ProductEnum.UnifiedPlatform, productIds, cancellationToken);

            if (await ShouldFilterLuminaAsync(ctx.EditorPersona, false, cancellationToken))
                allRoleRights.RemoveAll(r =>
                    string.Equals(r.RightNickName, "Lumina", StringComparison.OrdinalIgnoreCase));

            var rolesWithCount = await GetRolesWithRightsCountAsync(
                allRoleRights, partyId, (int)ProductEnum.UnifiedPlatform, productIds, cancellationToken);

            return ToListResponse(rolesWithCount.OrderBy(r => r.Name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesWithCountAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UnifiedLogin - There was a problem getting the roles.");
        }
    }

    #endregion

    #region GetUserRolesAsync / GetUserRolesWithRightsAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserRolesAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIdList = await _productRepo.GetProductIdsByCompanyAsync(partyId);
            var allRoles = await _productRepo.ListRolesForProductByPartyAsync(
                partyId, productIdList, (int)ProductEnum.UnifiedPlatform);
            var sortedRoles = allRoles.OrderBy(r => r.Name).ToList();

            if (userPersonaId != 0)
                return await MergeSelRolesWithGreenbookAsync(sortedRoles, userPersonaId, partyId, cancellationToken);

            sortedRoles.FirstOrDefault(s => s.DefaultRole == "True")!.IsAssigned = true;
            return ToListResponse(sortedRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserRolesAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UserManagement - There was a problem getting the roles.");
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetUserRolesWithRightsAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, userPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIdList = await _productRepo.GetProductIdsByCompanyAsync(partyId);
            var allRoles = await _userRoleRightRepo.GetPlatformRoleRightsAsync(
                partyId, productIdList, (int)ProductEnum.UnifiedPlatform);
            allRoles = allRoles.OrderBy(r => r.Role).ToList();

            if (await ShouldFilterLuminaAsync(ctx.EditorPersona, includeAppPartner: true, cancellationToken))
                foreach (var role in allRoles)
                {
                    var lumina = role.UserRights.FirstOrDefault(f => f.RightNickName == "Lumina");
                    if (lumina is not null) role.UserRights.Remove(lumina);
                }

            if (userPersonaId != 0)
                return await SetUserSelectedRoleAsync(allRoles, userPersonaId, partyId, cancellationToken);

            allRoles.FirstOrDefault(s => s.DefaultRole == "True")!.IsAssigned = true;
            return ToListResponse(allRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserRolesWithRightsAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse(CommonMessageConstants.RoleErrorMessage);
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUnifiedLoginAsync — right management
    // ════════════════════════════════════════════════════════════════════════

    #region UpdateRightsToRoleAsync / CloneRightsToRoleAsync

    /// <inheritdoc/>
    public Task<ListResponse> UpdateRightsToRoleAsync(
        long editorPersonaId, long roleId,
        List<string> rightsToAdd, List<string> rightsToRemove,
        CancellationToken cancellationToken = default)
        => ApplyRightChangesAsync(editorPersonaId, roleId, rightsToAdd, rightsToRemove, cancellationToken);

    /// <inheritdoc/>
    public Task<ListResponse> CloneRightsToRoleAsync(
        long editorPersonaId, long roleId,
        List<string> rightsToAdd, List<string> rightsToRemove,
        CancellationToken cancellationToken = default)
        => ApplyRightChangesAsync(editorPersonaId, roleId, rightsToAdd, rightsToRemove, cancellationToken);

    private async Task<ListResponse> ApplyRightChangesAsync(
        long editorPersonaId, long roleId,
        List<string> rightsToAdd, List<string> rightsToRemove, CancellationToken ct)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, ct);
        if (ctx is null) return guardError!;

        var response = new ListResponse();
        try
        {
            if ((rightsToAdd?.Count ?? 0) > 0 || (rightsToRemove?.Count ?? 0) > 0)
            {
                await LinkRightsToRoleAsync(editorPersonaId, roleId,
                    rightsToAdd ?? [], rightsToRemove ?? [], ct);
                await UpdateRightsToRoleLogMessageAsync(editorPersonaId, roleId, rightsToAdd, rightsToRemove, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApplyRightChangesAsync failed roleId={Id}", roleId);
            response.IsError = true; response.ErrorReason = ex.Message;
        }
        return response;
    }

    #endregion

    #region GetRightsAsync / GetRightsWithCountAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsAsync(
        long editorPersonaId, long partyId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIds = await GetProductIdsByOrgAsync(cancellationToken);
            var rights = await _unifiedLoginRepo.ListAllRightsForProductsByPartyIdAsync(
                partyId, (int)ProductEnum.UnifiedPlatform, productIds, cancellationToken);
            return ToListResponse(rights.OrderBy(r => r.Description));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRightsAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UnifiedLogin - There was a problem getting the rights.");
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsWithCountAsync(
        long editorPersonaId, long partyId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        var userClaims = _userClaimAccessor.Current;
        try
        {
            List<int> productIds;
            if (userClaims.OrganizationRealPageGuid == DefaultUserClaim.EmployeeCompanyRealPageId)
            {
                var all = await _productRepo.GetAllProductsAsync();
                productIds = all.Select(p => p.ProductId).ToList();
            }
            else
            {
                productIds = await GetProductIdsByOrgAsync(cancellationToken);
            }

            var allRights = await _unifiedLoginRepo.ListRightWithRolesAsync(
                partyId, (int)ProductEnum.UnifiedPlatform, productIds, cancellationToken);

            if (await ShouldFilterLuminaAsync(ctx.EditorPersona, false, cancellationToken))
                allRights.RemoveAll(r =>
                    string.Equals(r.RightNickName, "Lumina", StringComparison.OrdinalIgnoreCase));

            return ToListResponse(GetRightsWithRolesCount(allRights).OrderBy(r => r.Description));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRightsWithCountAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UnifiedLogin - There was a problem getting the rights.");
        }
    }

    #endregion

    #region GetRightsByRoleAsync / GetListRightByRoleAsync / GetAllRightsByRoleAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId, long partyId, long roleId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIdList = await _productRepo.GetProductIdsByCompanyAsync(partyId);
            var rights = await _unifiedLoginRepo.ListRightsByRoleAsync(
                partyId, productIdList, (int)ProductEnum.UnifiedPlatform, roleId, cancellationToken);
            return ToListResponse(rights.OrderBy(r => r.Description));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRightsByRoleAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse(CommonMessageConstants.RightErrorMessage);
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetListRightByRoleAsync(
        string productCode, int roleId, CancellationToken cancellationToken = default)
    {
        var userClaims = _userClaimAccessor.Current;

        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            userClaims.PersonaId, userClaims.PersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productList = await _productRepo.GetAllProductsAsync();
            int productId = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);

            var settings = await _settingService.GetProductSettingAsync(productId, cancellationToken);
            string? shared = settings.FirstOrDefault(a =>
                a.Name.Equals(SettingConstants.SharedProductSettingName,
                    StringComparison.OrdinalIgnoreCase))?.Value;

            if (shared is not null && int.TryParse(shared, out int sharedProductId))
                productId = sharedProductId;

            var rights = await _unifiedLoginRepo.ListRightsByRoleAsync(
                userClaims.OrganizationPartyId, [productId], productId, roleId, cancellationToken);

            return ToListResponse(rights.OrderBy(r => r.Description));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetListRightByRoleAsync failed productCode={Code}", productCode);
            return ProductManagerHelpers.ErrorResponse(CommonMessageConstants.RightErrorMessage);
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetAllRightsByRoleAsync(
        long editorPersonaId, long partyId, long roleId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIds = await GetProductIdsByOrgAsync(cancellationToken);
            var allRights = await _unifiedLoginRepo.ListAllRightsForProductsByPartyIdAsync(
                partyId, (int)ProductEnum.UnifiedPlatform, productIds, cancellationToken);

            if (await ShouldFilterLuminaAsync(ctx.EditorPersona, false, cancellationToken))
                allRights.RemoveAll(r =>
                    string.Equals(r.Alias, "Lumina", StringComparison.OrdinalIgnoreCase));

            allRights = allRights.OrderBy(r => r.Description).ToList();

            return roleId != 0
                ? await MergeRightsWithAllRightsAsync(allRights, roleId, partyId, cancellationToken)
                : ToListResponse(allRights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllRightsByRoleAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UserManagement - There was a problem getting the rights.");
        }
    }

    #endregion

    #region GetRolesByRightAsync / UpdateRolesByRightAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesByRightAsync(
        long editorPersonaId, long partyId, long rightId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var productIds = await GetProductIdsByOrgAsync(cancellationToken);
            var allRoles = (await _unifiedLoginRepo.ListRolesForProductsByPartyIdAsync(
                partyId, (int)ProductEnum.UnifiedPlatform, productIds, cancellationToken))
                .OrderBy(r => r.Name).ToList();

            return rightId != 0
                ? await MergeRolesWithAllRolesAsync(allRoles, rightId, partyId,
                    (int)ProductEnum.UnifiedPlatform, cancellationToken)
                : ToListResponse(allRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRolesByRightAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UserManagement - There was a problem getting the roles.");
        }
    }

    /// <inheritdoc/>
    public async Task<ListResponse> UpdateRolesByRightAsync(
        long editorPersonaId, long rightId,
        List<string> rolesToAdd, List<string> rolesToRemove,
        CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        var response = new ListResponse();
        try
        {
            var currentRoles = await GetRolesByRightAsync(
                editorPersonaId, _userClaimAccessor.Current.OrganizationPartyId, rightId, cancellationToken);
            GetRoleAssignmentChanges(rolesToAdd, currentRoles, out var newRolesAdded);

            if (rolesToAdd is not null && rolesToRemove is not null)
                await LinkRolesToRightAsync(editorPersonaId, rightId, rolesToAdd, rolesToRemove, cancellationToken);

            if ((rolesToAdd?.Any() == true) || (rolesToRemove?.Any() == true))
                await UpdateRolesByRightLogMessageAsync(
                    editorPersonaId, rightId, newRolesAdded, rolesToRemove ?? [], cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateRolesByRightAsync failed editorPersonaId={Id}", editorPersonaId);
            response.ErrorReason = ex.Message;
        }
        return response;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // IManageUnifiedLoginAsync — company
    // ════════════════════════════════════════════════════════════════════════

    #region GetGBCompaniesAsync

    /// <inheritdoc/>
    public async Task<ListResponse> GetGBCompaniesAsync(
        long editorPersonaId, long partyId, CancellationToken cancellationToken = default)
    {
        var (ctx, guardError) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, (int)ProductEnum.UnifiedPlatform, cancellationToken);
        if (ctx is null) return guardError!;

        try
        {
            var companies = await _unifiedLoginRepo.ListCompaniesAsync("", null, cancellationToken);
            return ToListResponse(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetGBCompaniesAsync failed editorPersonaId={Id}", editorPersonaId);
            return ProductManagerHelpers.ErrorResponse("UnifiedLogin - There was a problem getting the companies.");
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — audit log helpers
    // ════════════════════════════════════════════════════════════════════════

    #region Audit log helpers

    private async Task AddUpdateRoleLogMessageAsync(
        long editorPersonaId, long partyId, string roleName, string action,
        string product, string? oldRoleName, int productId, CancellationToken ct)
    {
        var fromUser = await _auditService.GetUserActivityLogInfoAsync(editorPersonaId, ct);
        var impersonator = await GetImpersonatorDetailsAsync(_userClaimAccessor.Current.ImpersonatedBy, ct);
        string actor = BuildActorName(fromUser, impersonator);
        var additional = new List<AdditionalParameters>();
        string message;

        if (action == "ADD")
        {
            message = $"{actor} Created {roleName} in {product}.";
            additional.Add(new AdditionalParameters
            { Key = "Role", Value = PRODUCT_ROLE_CREATE.Replace("RoleName", roleName) });
        }
        else
        {
            message = $"{actor} Updated {oldRoleName} to {roleName} in {product}.";
            additional.Add(new AdditionalParameters
            { Key = oldRoleName!, Value = PRODUCT_ROLE_UPDATE.Replace("RoleName", roleName) });
        }
        PushToQueue(fromUser, message, additional, productId);
    }

    private async Task DeleteRoleLogMessageAsync(
        long editorPersonaId, long roleId, string roleName,
        string product, int productId, CancellationToken ct)
    {
        var fromUser = await _auditService.GetUserActivityLogInfoAsync(editorPersonaId, ct);
        var impersonator = await GetImpersonatorDetailsAsync(_userClaimAccessor.Current.ImpersonatedBy, ct);
        string actor = BuildActorName(fromUser, impersonator);
        PushToQueue(fromUser, $"{actor} deleted {roleName} in {product}.",
            [new AdditionalParameters { Key = "Role", Value = PRODUCT_ROLE_DELETE.Replace("RoleName", roleName) }],
            productId);
    }

    private async Task SetDefaultRoleLogMessageAsync(
        long editorPersonaId, long partyId, long roleId, CancellationToken ct)
    {
        var fromUser = await _auditService.GetUserActivityLogInfoAsync(editorPersonaId, ct);
        var impersonator = await GetImpersonatorDetailsAsync(_userClaimAccessor.Current.ImpersonatedBy, ct);
        string actor = BuildActorName(fromUser, impersonator);
        string roleName = await GetRoleNameAsync(roleId, (int)ProductEnum.UnifiedPlatform, ct);
        PushToQueue(fromUser, $"{actor} made {roleName} in Unified Platform as default.",
            [new AdditionalParameters { Key = "Role", Value = PRODUCT_ROLE_USERDEFAULT.Replace("RoleName", roleName) }],
            productId: 3);
    }

    private async Task UpdateRightsToRoleLogMessageAsync(
        long editorPersonaId, long roleId,
        List<string>? rightsToAdd, List<string>? rightsToRemove, CancellationToken ct)
    {
        var fromUser = await _auditService.GetUserActivityLogInfoAsync(editorPersonaId, ct);
        var impersonator = await GetImpersonatorDetailsAsync(_userClaimAccessor.Current.ImpersonatedBy, ct);
        string actor = BuildActorName(fromUser, impersonator);
        string roleName = await GetRoleNameAsync(roleId, (int)ProductEnum.UnifiedPlatform, ct);
        var additional = new List<AdditionalParameters>();

        if (rightsToAdd is not null)
            foreach (var r in rightsToAdd)
            {
                string name = await GetRightNameAsync(r, (int)ProductEnum.UnifiedPlatform, ct);
                additional.Add(new AdditionalParameters
                { Key = roleName, Value = RIGHT_ASSIGN.Replace("RightName", name) });
            }

        if (rightsToRemove is not null)
            foreach (var r in rightsToRemove)
            {
                string name = await GetRightNameAsync(r, (int)ProductEnum.UnifiedPlatform, ct);
                additional.Add(new AdditionalParameters
                { Key = roleName, Value = RIGHT_UNASSIGN.Replace("RightName", name) });
            }

        PushToQueue(fromUser, $"{actor} Added/Removed Rights to {roleName} in Unified Platform.",
            additional, productId: 3);
    }

    private async Task UpdateRolesByRightLogMessageAsync(
        long editorPersonaId, long rightId,
        List<string> rolesToAdd, List<string> rolesToRemove, CancellationToken ct)
    {
        var fromUser = await _auditService.GetUserActivityLogInfoAsync(editorPersonaId, ct);
        var impersonator = await GetImpersonatorDetailsAsync(_userClaimAccessor.Current.ImpersonatedBy, ct);
        string actor = BuildActorName(fromUser, impersonator);
        string rightName = await GetRightNameAsync(rightId.ToString(), (int)ProductEnum.UnifiedPlatform, ct);
        var additional = new List<AdditionalParameters>();

        foreach (var role in rolesToAdd)
        {
            string name = await GetRoleNameAsync(long.Parse(role), (int)ProductEnum.UnifiedPlatform, ct);
            additional.Add(new AdditionalParameters
            { Key = rightName, Value = ROLE_ASSIGN.Replace("RoleName", name) });
        }
        foreach (var role in rolesToRemove)
        {
            string name = await GetRoleNameAsync(long.Parse(role), (int)ProductEnum.UnifiedPlatform, ct);
            additional.Add(new AdditionalParameters
            { Key = rightName, Value = ROLE_UNASSIGN.Replace("RoleName", name) });
        }

        PushToQueue(fromUser, $"{actor} Added/Removed roles to {rightName} in Unified Platform.",
            additional, productId: 3);
    }

    /// <summary>
    /// Uses <c>ROLES_RIGHTS</c> log type — distinct from <c>IProductAuditServiceAsync</c>
    /// which uses <c>PRODUCT_ACCESS</c>.
    /// </summary>
    private void PushToQueue(
        UserActivityLogInfoAsync fromUser, string message,
        List<AdditionalParameters>? additional = null, int productId = 0)
    {
        var userClaims = _userClaimAccessor.Current;
        LogActivity.WriteActivity(new ActivityDetails
        {
            LogActivityTypeName = LogActivityTypeConstants.ROLES_RIGHTS,
            LogCategoryName = LogActivityCategoryType.RolesRights.ToString(),
            CorrelationId = userClaims.CorrelationId.ToString(),
            BooksMasterOrganizationId = fromUser.BooksOrganizationMasterId,
            OrganizationPartyId = fromUser.OrganizationPartyId,
            Message = message,
            FromUserLoginName = fromUser.LoginName,
            FromUserLoginId = fromUser.UserId,
            FromUserFirstName = fromUser.FirstName,
            FromUserLastName = fromUser.LastName,
            FromUserRealpageId = fromUser.RealPageId.ToString(),
            AdditionalInformation = additional,
            ContextId = productId > 0 ? productId.ToString() : null
        });
    }

    private static string BuildActorName(UserActivityLogInfoAsync fromUser, UserDetails? impersonator)
        => impersonator is not null
            ? $"RealPage Access ({impersonator.FirstName} {impersonator.LastName})"
            : $"{fromUser.FirstName} {fromUser.LastName}";

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — impersonator lookup
    // ════════════════════════════════════════════════════════════════════════

    #region GetImpersonatorDetailsAsync

    /// <summary>
    /// FIX: original used <c>!= null</c> on a <see cref="Guid"/> (always true) —
    /// corrected to <c>!= Guid.Empty</c>.
    /// </summary>
    private async Task<UserDetails?> GetImpersonatorDetailsAsync(Guid realPageId, CancellationToken ct)
    {
        if (realPageId == Guid.Empty) return null;
        return await _userRepo.GetUserDetailsAsync(null, realPageId.ToString(), ct);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — merge helpers
    // ════════════════════════════════════════════════════════════════════════

    #region Merge helpers

    private async Task<ListResponse> MergeProductPropertiesWithGreenbookAsync(
        IList<ProductProperty> blueBookList, long userPersonaId, bool assignedOnly, CancellationToken ct)
    {
        var propList = await _propertyRepo.ListPropertiesByPersonaAsync(
            userPersonaId, (int)ProductEnum.UnifiedPlatform, ct);

        var option = new Dictionary<string, bool> { ["allProperties"] = false };

        foreach (var prop in propList)
        {
            if (prop.ID == "-1") { option["allProperties"] = true; continue; }
            var match = blueBookList.FirstOrDefault(a => a.ID == prop.ID);
            if (match is not null) match.IsAssigned = true;
        }

        var result = (assignedOnly
            ? blueBookList.Where(a => a.IsAssigned == true)
            : blueBookList).ToList();

        return new ListResponse
        {
            Records = result.Cast<object>().ToList(),
            TotalRows = result.Count,
            RowsPerPage = 9999,
            TotalPages = 1,
            ErrorReason = string.Empty,
            Additional = option
        };
    }

    private async Task<ListResponse> MergeUPFMBooksPropertiesWithProductPropertyAsync(
        IList<UPFMPropertyInstance> blueBookList, long userPersonaId, bool assignedOnly, CancellationToken ct)
    {
        var userIds = await _propertyRepo.ListUPFMPropertyInstanceIdByPersonaAsync(
            userPersonaId, ProductEnum.UnifiedPlatform, ct);

        var option = new Dictionary<string, bool>
        { ["allProperties"] = userIds.Any(p => p == -1) };

        var productList = blueBookList.Select(p =>
        {
            var pp = ConvertUPFMPropertyInstanceToProductProperty(p, false);
            if (userIds.Any(id => id == p.PropertyInstanceId)) pp.IsAssigned = true;
            return pp;
        }).ToList();

        if (assignedOnly) productList = productList.Where(p => p.IsAssigned == true).ToList();

        return new ListResponse
        {
            Records = productList.Cast<object>().ToList(),
            TotalRows = productList.Count,
            RowsPerPage = 9999,
            TotalPages = 1,
            ErrorReason = string.Empty,
            Additional = option
        };
    }

    private async Task<ListResponse> MergeSelRolesWithGreenbookAsync(
        IList<ProductRole> allRoles, long userPersonaId, long partyId, CancellationToken ct)
    {
        var roleList = await _userRoleRightRepo.ListRoleByPersonaAsync(
            (int)ProductEnum.UnifiedPlatform, userPersonaId, partyId);

        if (roleList is null) return ToListResponse(allRoles);

        foreach (var role in roleList)
        {
            var match = allRoles.FirstOrDefault(a => a.ID == role.RoleID.ToString());
            if (match is not null) match.IsAssigned = true;
        }
        return ToListResponse(allRoles);
    }

    private async Task<ListResponse> SetUserSelectedRoleAsync(
        IList<UnifiedLoginRoleRights> allRoles, long userPersonaId, long partyId, CancellationToken ct)
    {
        var roleList = await _userRoleRightRepo.ListRoleByPersonaAsync(
            (int)ProductEnum.UnifiedPlatform, userPersonaId, partyId);

        if (roleList is null) return ToListResponse(allRoles);

        foreach (var role in roleList)
        {
            var match = allRoles.FirstOrDefault(a => a.RoleId.ToString() == role.RoleID.ToString());
            if (match is not null) match.IsAssigned = true;
        }
        return ToListResponse(allRoles);
    }

    private async Task<ListResponse> MergeRightsWithAllRightsAsync(
        List<ProductRight> allRights, long roleId, long partyId, CancellationToken ct)
    {
        var productIds = await GetProductIdsByOrgAsync(ct);
        var productIdList = await _productRepo.GetProductIdsByCompanyAsync(partyId);
        var rightList = await _unifiedLoginRepo.ListRightsByRoleAsync(
            partyId, productIdList, (int)ProductEnum.UnifiedPlatform, roleId, ct);

        foreach (var right in rightList)
        {
            var match = allRights.FirstOrDefault(a => a.ID == right.ID);
            if (match is not null) match.Assigned = true;
        }
        return ToListResponse(allRights);
    }

    private async Task<ListResponse> MergeRolesWithAllRolesAsync(
        IList<ProductRole> allRoles, long rightValId, long partyId, int productId, CancellationToken ct)
    {
        var productIds = await GetProductIdsByOrgAsync(ct);
        var roleRightDet = await _unifiedLoginRepo.ListRoleRightDetForProductsByPartyIdAsync(
            partyId, productId, productIds, ct);

        foreach (var role in roleRightDet.FindAll(r => r.RightValueTypeId == rightValId))
        {
            var match = allRoles.FirstOrDefault(a => int.Parse(a.ID) == role.RoleId);
            if (match is not null) match.IsAssigned = true;
        }
        return ToListResponse(allRoles);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — link helpers
    // ════════════════════════════════════════════════════════════════════════

    #region LinkRightsToRoleAsync / LinkRolesToRightAsync

    private async Task LinkRightsToRoleAsync(
        long editorPersonaId, long roleId,
        List<string> addRights, List<string> delRights, CancellationToken ct)
    {
        var payload = addRights
            .Select(r => new RightRoleAddRem
            { RoleId = roleId, RightValueTypeID = long.Parse(r), IsDeleted = 0 })
            .Concat(delRights
                .Select(r => new RightRoleAddRem
                { RoleId = roleId, RightValueTypeID = long.Parse(r), IsDeleted = 1 }))
            .ToList();

        await _unifiedLoginRepo.LinkRightsToRoleAsync(payload, _userClaimAccessor.Current.UserId, ct);
    }

    private async Task LinkRolesToRightAsync(
        long editorPersonaId, long rightId,
        List<string> addRoles, List<string> delRoles, CancellationToken ct)
    {
        var payload = addRoles
            .Select(r => new RightRoleAddRem
            { RoleId = long.Parse(r), RightValueTypeID = rightId, IsDeleted = 0 })
            .Concat(delRoles
                .Select(r => new RightRoleAddRem
                { RoleId = long.Parse(r), RightValueTypeID = rightId, IsDeleted = 1 }))
            .ToList();

        await _unifiedLoginRepo.LinkRightsToRoleAsync(payload, _userClaimAccessor.Current.UserId, ct);
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — name lookup helpers
    // ════════════════════════════════════════════════════════════════════════

    #region GetRoleNameAsync / GetRightNameAsync

    private async Task<string> GetRoleNameAsync(long roleId, int productId, CancellationToken ct)
    {
        var productIds = await GetProductIdsByOrgAsync(ct);
        var roles = await _unifiedLoginRepo.ListRolesForProductsByPartyIdAsync(
            _userClaimAccessor.Current.OrganizationPartyId, productId, productIds, ct);
        return roles.Find(r => r.ID == roleId.ToString())?.Name ?? string.Empty;
    }

    private async Task<string> GetRightNameAsync(string rightId, int productId, CancellationToken ct)
    {
        var productIds = await GetProductIdsByOrgAsync(ct);
        var rights = await _unifiedLoginRepo.ListAllRightsForProductsByPartyIdAsync(
            _userClaimAccessor.Current.OrganizationPartyId, productId, productIds, ct);
        return rights.Find(r => r.ID == int.Parse(rightId))?.Description ?? string.Empty;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — org product ID + Lumina helpers
    // ════════════════════════════════════════════════════════════════════════

    #region GetProductIdsByOrgAsync / ShouldFilterLuminaAsync

    private async Task<List<int>> GetProductIdsByOrgAsync(CancellationToken ct)
    {
        var result = await _productRepo.GetProductIdsByCompanyAsync(
            _userClaimAccessor.Current.OrganizationRealPageGuid);
        return result as List<int> ?? result.ToList();
    }

    /// <summary>
    /// Centralises the Lumina-right filter condition that was duplicated across four methods.
    /// <paramref name="includeAppPartner"/> adds the <c>OrganizationType == "AppPartner"</c>
    /// check used only in <c>GetUserRolesWithRightsAsync</c>.
    /// </summary>
    private async Task<bool> ShouldFilterLuminaAsync(
        Persona? editorPersona, bool includeAppPartner, CancellationToken ct)
    {
        var userClaims = _userClaimAccessor.Current;
        if (includeAppPartner && userClaims.OrganizationType == "AppPartner") return true;

        var settings = await _unifiedSettings.GetUnifiedSettingsCachedAsync(
            userClaims.OrganizationPartyId, "aichat", ct);

        string? opt = settings.FirstOrDefault(x => x.Name == "aichatuseroptions")?.Value;
        return opt is "1" or "3" && editorPersona?.UserTypeId == 402;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — count aggregation (pure computation)
    // ════════════════════════════════════════════════════════════════════════

    #region GetRolesWithRightsCountAsync / GetRightsWithRolesCount

    private async Task<List<ProductRole>> GetRolesWithRightsCountAsync(
        List<RightRoleDetail> allRolesAndRights, long partyId,
        long ulProductId, List<int> productIdList, CancellationToken ct)
    {
        var result = new List<ProductRole>();
        if (allRolesAndRights is null || allRolesAndRights.Count == 0) return result;

        var roles = await _unifiedLoginRepo.ListRolesForProductsByPartyIdAsync(
            partyId, ulProductId, productIdList, ct);

        foreach (var role in roles)
        {
            var list = new List<RightRoleDetail>();
            bool roleFound = false;

            foreach (var item in allRolesAndRights)
            {
                if (item.RoleId != int.Parse(role.ID)) continue;
                roleFound = true;
                if (list.Count == 0 || !list.Exists(e => e.RightName == item.RightName))
                    list.Add(item);
            }

            if (list.Count > 0)
            {
                int rights = list.Count == 1 && list[0].RightId == 0 ? 0 : list.Count;
                list[0].RightsAssigned = rights.ToString();
                result.Add(new ProductRole
                {
                    ID = list[0].RoleId.ToString(),
                    IsAssigned = list[0].IsAssigned,
                    Roletype = list[0].RoleType,
                    Name = list[0].RoleName,
                    RightsAssigned = list[0].RightsAssigned,
                    DefaultRole = list[0].IsDefaultRole == true ? "User Default" : string.Empty
                });
            }
            else if (!roleFound)
            {
                result.Add(new ProductRole
                {
                    ID = role.ID,
                    IsAssigned = false,
                    Roletype = role.Roletype,
                    Name = role.Name,
                    RightsAssigned = "0",
                    DefaultRole = bool.Parse(role.DefaultRole) ? "User Default" : string.Empty
                });
            }
        }

        return result;
    }

    private static List<ProductRight> GetRightsWithRolesCount(List<RightRoleDetail> allRights)
    {
        var result = new List<ProductRight>();
        if (allRights is null || allRights.Count == 0) return result;

        foreach (var rightId in allRights.Select(x => x.RightValueTypeId).Distinct())
        {
            var list = new List<RightRoleDetail>();
            foreach (var item in allRights)
            {
                if (item.RightValueTypeId != rightId) continue;
                if (list.Count == 0 || !list.Exists(e => e.RoleName == item.RoleName))
                    list.Add(item);
            }

            if (list.Count == 0) continue;

            list[0].RolesAssigned = list.Count.ToString();
            result.Add(new ProductRight
            {
                ID = list[0].RightValueTypeId,
                Assigned = list[0].IsAssigned,
                Description = list[0].RightName,
                RolesAssigned = int.Parse(list[0].RolesAssigned),
                RightDescription = list[0].RightDescription
            });
        }

        return result;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Private — pure static utilities
    // ════════════════════════════════════════════════════════════════════════

    #region Static utilities

    private static void GetRoleAssignmentChanges(
        List<string> roles, ListResponse currentRoles, out List<string> rolesToAdd)
    {
        rolesToAdd = [];

        var desired = (roles ?? [])
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var assignedNow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (currentRoles?.Records?.Count > 0)
            foreach (var pr in currentRoles.Records.OfType<ProductRole>())
                if (pr.IsAssigned && !string.IsNullOrWhiteSpace(pr.ID))
                    assignedNow.Add(pr.ID.Trim());

        foreach (var roleId in desired)
            if (!assignedNow.Contains(roleId))
                rolesToAdd.Add(roleId);
    }

    private static List<ProductProperty> ApplyContractResolverIfNeeded(
        List<ProductProperty> list, string? include)
    {
        if (string.IsNullOrWhiteSpace(include) || include.Split(',').Length == 0) return list;
        var resolver = new DynamicContractResolver(include);
        string json = JsonConvert.SerializeObject(list,
            new JsonSerializerSettings { ContractResolver = resolver });
        return JsonConvert.DeserializeObject<List<ProductProperty>>(json) ?? list;
    }

    private static ProductProperty ConvertUPFMPropertyInstanceToProductProperty(
        UPFMPropertyInstance p, bool isAssigned) => new()
        {
            ID = p.InstanceId.ToString().ToLower(),
            Name = p.Name,
            Street1 = p.Address,
            City = p.City,
            State = p.State,
            Zip = p.PostalCode,
            IsAssigned = isAssigned,
            InstanceId = p.CustomerPropertyId,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            Alias = null
        };

    private static ListResponse ToListResponse<T>(IEnumerable<T> items, int rowsPerPage = 9999)
    {
        var list = items.ToList();
        return new ListResponse
        {
            Records = list.Cast<object>().ToList(),
            TotalRows = list.Count,
            RowsPerPage = rowsPerPage,
            TotalPages = 1,
            ErrorReason = string.Empty
        };
    }

    #endregion
}