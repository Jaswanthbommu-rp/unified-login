using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Models;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UPFMProduct;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// True-async implementation of UPFM product user management.
/// <para>
/// Replaces <c>ManageUPFMProductsIntegration</c> (sync stepping-stone).
/// No <c>DefaultUserClaim</c>, no mutable instance fields, no blocking <c>.Result</c> calls.
/// </para>
/// <para>
/// The <paramref name="productId"/> constructor parameter makes each DI-scoped instance
/// product-specific; the <c>IManageUPFMProductsIntegrationFactory</c> creates the right
/// concrete instance per request.
/// </para>
/// </summary>
public sealed class ManageUPFMProductsIntegrationAsync : IManageUPFMProductsIntegrationAsync
{
    #region Audit message templates

    private const string RolesAssignMsg      = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
    private const string RolesRemovedMsg     = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
    private const string PropsAssignMsg      = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
    private const string PropsRemovedMsg     = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";

    #endregion

    #region Dependencies

    private readonly int _productId;   // immutable — set once at construction
    private readonly IProductContextServiceAsync         _contextService;
    private readonly IProductRepositoryAsync             _productRepository;
    private readonly IProductInternalSettingRepositoryAsync _internalSettingRepository;
    private readonly IManageBlueBookAsync                _blueBook;
    private readonly IPropertyRepositoryAsync            _propertyRepository;
    private readonly IManageUserRoleRightAsync           _userRoleRight;
    private readonly IManageUserLoginAsync               _userLogin;
    private readonly IUnifiedLoginRepositoryAsync        _unifiedLoginRepository;
    private readonly ILogger<ManageUPFMProductsIntegrationAsync> _logger;

    #endregion

    #region Constructor

    public ManageUPFMProductsIntegrationAsync(
        int                                              productId,
        IProductContextServiceAsync                     contextService,
        IProductRepositoryAsync                         productRepository,
        IProductInternalSettingRepositoryAsync          internalSettingRepository,
        IManageBlueBookAsync                            blueBook,
        IPropertyRepositoryAsync                        propertyRepository,
        IManageUserRoleRightAsync                       userRoleRight,
        IManageUserLoginAsync                           userLogin,
        IUnifiedLoginRepositoryAsync                    unifiedLoginRepository,
        ILogger<ManageUPFMProductsIntegrationAsync>     logger)
    {
        ArgumentNullException.ThrowIfNull(contextService);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(internalSettingRepository);
        ArgumentNullException.ThrowIfNull(blueBook);
        ArgumentNullException.ThrowIfNull(propertyRepository);
        ArgumentNullException.ThrowIfNull(userRoleRight);
        ArgumentNullException.ThrowIfNull(userLogin);
        ArgumentNullException.ThrowIfNull(unifiedLoginRepository);
        ArgumentNullException.ThrowIfNull(logger);

        _productId                 = productId;
        _contextService            = contextService;
        _productRepository         = productRepository;
        _internalSettingRepository = internalSettingRepository;
        _blueBook                  = blueBook;
        _propertyRepository        = propertyRepository;
        _userRoleRight             = userRoleRight;
        _userLogin                 = userLogin;
        _unifiedLoginRepository    = unifiedLoginRepository;
        _logger                    = logger;
    }

    #endregion

    #region Roles and Rights

    /// <inheritdoc/>
    public async Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {state}", "GetRolesAsync",
            $"Beginning. editorPersonaId - {editorPersonaId}, productId - {_productId}");

        var response = new ListResponse();
        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, _productId, cancellationToken);
            if (error is not null) return error;

            // Parallel: product settings + product IDs per company
            var settingsTask     = _internalSettingRepository.GetProductInternalSettingsAsync(_productId, cancellationToken);
            var productIdsTask   = _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken);
            await Task.WhenAll(settingsTask, productIdsTask);

            var productSettings = await settingsTask;
            var productIdList   = (await productIdsTask).ToList();

            int effectiveProductId = await GetSharedProductIdAsync(productIdList, cancellationToken);

            // Check whether to hit BlueBook for company map
            bool getUDMDetails = true;
            var udmSetting = productSettings.FirstOrDefault(s => s.Name.Equals("UpdateProductInUDM", StringComparison.OrdinalIgnoreCase));
            if (udmSetting is not null)
                getUDMDetails = udmSetting.Value == "1";

            if (getUDMDetails)
            {
                string udmSourceCode = await ResolveUdmSourceCodeAsync(cancellationToken);
                await _blueBook.GetCompanyMapAsync(
                    ctx!.EditorPersona.Organization.RealPageId,
                    ctx.EditorPersona.Organization.BooksCustomerMasterId,
                    source: udmSourceCode.ToUpper(),
                    domain: ctx.EditorPersona.Organization.OrganizationDomain?.Name ?? string.Empty,
                    cancellationToken: cancellationToken);
            }

            var allRoles = (await _productRepository.ListRolesForProductByPartyAsync(
                partyId, productIdList, effectiveProductId, cancellationToken))
                ?? [];
            allRoles = [.. allRoles.OrderBy(r => r.Name)];

            if (userPersonaId != 0)
            {
                response = await MergeSelRolesWithGreenbookAsync(allRoles, userPersonaId, cancellationToken);
            }
            else
            {
                // New user — mark the default role
                var defaultRole = allRoles.FirstOrDefault(
                    s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase));
                if (defaultRole is not null)
                    defaultRole.IsAssigned = true;

                response = new ListResponse
                {
                    Records     = allRoles.Cast<object>().ToList(),
                    TotalRows   = allRoles.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages  = 1
                };
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            response.IsError = true;
            response.ErrorReason = ex is BlueBookException
                ? ex.Message
                : CommonMessageConstants.RoleErrorMessage;
            _logger.LogError(ex, "{ActionName} - {state}", "GetRolesAsync",
                $"Error. editorPersonaId - {editorPersonaId}");
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetRightsByRoleAsync(
        long editorPersonaId,
        long partyId,
        long roleId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {state}", "GetRightsByRoleAsync",
            $"Beginning. editorPersonaId - {editorPersonaId}");

        var response = new ListResponse();
        try
        {
            var (_, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, editorPersonaId, _productId, cancellationToken);
            if (error is not null) return error;

            var productIdList = (await _productRepository.GetProductIdsByCompanyAsync(partyId, cancellationToken)).ToList();
            int effectiveProductId = await GetSharedProductIdAsync(productIdList, cancellationToken);

            var allRights = (await _unifiedLoginRepository.ListRightsByRoleAsync(
                partyId, productIdList, effectiveProductId, roleId, cancellationToken))
                ?? [];
            allRights = [.. allRights.OrderBy(r => r.Description)];

            response = new ListResponse
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
            response.IsError = true;
            response.ErrorReason = CommonMessageConstants.RightErrorMessage;
            _logger.LogError(ex, "{ActionName} - {state}", "GetRightsByRoleAsync",
                $"Error. editorPersonaId - {editorPersonaId}");
        }

        return response;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public async Task<ListResponse> GetEnterpriseUPFMPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        int product,
        string productCode,
        string? include = null,
        bool isMultiCompany = false,
        string? multiCompanyRealPageId = null,
        CancellationToken cancellationToken = default)
    {
        var response = new ListResponse();

        // CIMPL / UnifiedSettings: redirect to UnifiedPlatform
        int effectiveUpfmProductId = product;
        string udmSourceCode = !string.IsNullOrEmpty(productCode)
            ? productCode
            : await ResolveUdmSourceCodeAsync(cancellationToken);

        if (product == (int)ProductEnum.CIMPL || product == (int)ProductEnum.UnifiedSettings)
        {
            effectiveUpfmProductId = (int)ProductEnum.UnifiedPlatform;
        }

        var userPropertyIdList = await _propertyRepository.ListUPFMPropertyInstanceIdByPersonaAsync(
            userPersonaId, effectiveUpfmProductId, cancellationToken);

        List<ProductProperty>      userPropertyList         = [];
        List<ProductProperty>      translatedUserPropertyList = [];
        List<UPFMPropertyInstance> customerPropertyList     = [];

        if (userPropertyIdList is { Count: > 0 })
        {
            // Resolve editor org real page ID for BlueBook calls
            var (ctx, _) = await _contextService.GetUserContextAsync(
                editorPersonaId, editorPersonaId, _productId, cancellationToken);

            string orgRealPageIdStr = isMultiCompany && !string.IsNullOrEmpty(multiCompanyRealPageId)
                ? multiCompanyRealPageId!
                : ctx?.EditorPersona.Organization.RealPageId.ToString() ?? string.Empty;

            if (userPropertyIdList.Count == 1 && userPropertyIdList[0] == -1)
            {
                customerPropertyList = await GetProductPropertyInstancesAsync(
                    orgRealPageIdStr, udmSourceCode, effectiveUpfmProductId, cancellationToken);
                foreach (var cp in customerPropertyList)
                    userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true));
            }
            else
            {
                var booksPropertyGuids = await _blueBook.GetUPFMPropertyInstancesAsync(orgRealPageIdStr, cancellationToken);
                if (booksPropertyGuids?.Count > 0)
                    customerPropertyList = await _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIdsAsync(
                        booksPropertyGuids, cancellationToken);

                var userPropertySet = new HashSet<int>(userPropertyIdList);
                foreach (var cp in customerPropertyList)
                {
                    if (userPropertySet.Contains(cp.PropertyInstanceId))
                        userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true));
                }
            }
        }

        if (userPropertyIdList?.Count > 0)
        {
            // Build instance-ID list and get product details in parallel
            List<string> instanceIds = [.. userPropertyList.Select(p => p.InstanceId.ToLower())];
            var booksProductDetailTask = _productRepository.GetBooksMasterProductDetailAsync(product, cancellationToken);

            var upfmProperties = new UPFMProperty { id = instanceIds };
            var translatedDataTask = _blueBook.GetTranslatePropertiesFromUPFMToProductv3Async(
                upfmProperties, udmSourceCode, cancellationToken);

            await Task.WhenAll(booksProductDetailTask, translatedDataTask);

            var booksProductDetail = await booksProductDetailTask;
            var translatedData     = await translatedDataTask;

            if (translatedData?.Data is not null)
            {
                string booksProductCode = string.IsNullOrEmpty(booksProductDetail?.UDMSourceCode)
                    ? booksProductDetail?.BooksProductCode ?? string.Empty
                    : booksProductDetail.UDMSourceCode;

                var propertyByInstanceId = userPropertyList.ToDictionary(
                    p => p.InstanceId, p => p, StringComparer.OrdinalIgnoreCase);

                foreach (var attribute in translatedData.Data.Attributes)
                {
                    foreach (var translated in attribute.TranslatedPropertyInstances)
                    {
                        if (translated.Source == booksProductCode &&
                            propertyByInstanceId.TryGetValue(attribute.PropertyInstanceSourceId, out var prop))
                        {
                            prop.ID                  = translated.PropertyInstanceSourceId;
                            prop.Alias               = null;
                            prop.CustomerPropertyId  = translated.CustomerPropertyId;
                            translatedUserPropertyList.Add(prop);
                        }
                    }
                }
            }

            // Apply optional field filter (sync JSON round-trip — intentional)
            if (!string.IsNullOrWhiteSpace(include) && include.Split(',').Length > 0)
            {
                var resolver  = new DynamicContractResolver(include);
                var json      = JsonConvert.SerializeObject(translatedUserPropertyList,
                    new JsonSerializerSettings { ContractResolver = resolver });
                translatedUserPropertyList = JsonConvert.DeserializeObject<List<ProductProperty>>(json) ?? [];
            }

            foreach (var p in translatedUserPropertyList)
            {
                p.IsAssigned        = null;
                p.disableSelection  = null;
            }

            response.IsError    = false;
            response.Records    = translatedUserPropertyList.Cast<object>().ToList();
            response.TotalRows  = translatedUserPropertyList.Count;
            response.RowsPerPage = translatedUserPropertyList.Count;
            response.TotalPages = 1;
            response.ErrorReason = string.Empty;
        }

        return response;
    }

    /// <inheritdoc/>
    public async Task<ListResponse> GetUPFMPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        bool assignedOnly,
        RequestParameter datafilter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("{ActionName} - {state}", "GetUPFMPropertiesAsync",
            $"Beginning. editorPersonaId - {editorPersonaId}, productId - {_productId}");

        var result = new ListResponse();
        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, 0, _productId, cancellationToken);
            if (error is not null) return error;

            string orgRealPageId = ctx!.EditorPersona.Organization.RealPageId.ToString();
            string udmSourceCode = await ResolveUdmSourceCodeAsync(cancellationToken);

            var customerPropertyList = await GetProductPropertyInstancesAsync(
                orgRealPageId, udmSourceCode, _productId, cancellationToken);

            result = await MergeUPFMBooksPropertiesWithProductPropertyAsync(
                customerPropertyList, userPersonaId, _productId, assignedOnly, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            result.IsError = true;
            result.ErrorReason = ex is BlueBookException
                ? CommonMessageConstants.CompanyErrorMessage
                : "ManageUPFMProductUser.GetProperties - There was a problem getting the properties.";
            _logger.LogError(ex, "{ActionName} - {state}", "GetUPFMPropertiesAsync",
                $"Error. editorPersonaId - {editorPersonaId}");
        }

        return result;
    }

    #endregion

    #region Manage / Unassign

    /// <inheritdoc/>
    public async Task<(string result, List<AdditionalParameters> auditParams)> ManageUPFMProductUserAsync(
        long editorPersonaId,
        long userPersonaId,
        UPFMProductPropertyRole userAssignProductPropertyRole,
        bool isEmpAccess = false,
        CancellationToken cancellationToken = default)
    {
        List<AdditionalParameters> auditParams = [];
        var sw = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogDebug("{ActionName} - {state}", "ManageUPFMProductUserAsync",
            $"Begin create/update. userPersonaId - {userPersonaId}");

        try
        {
            var (ctx, contextError) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, _productId, cancellationToken);
            if (contextError is not null)
                return (contextError.ErrorReason ?? string.Empty, auditParams);

            // Parallel: user login + product IDs + product settings + existing roles + property IDs
            var userLoginTask       = _userLogin.GetUserLoginOnlyAsync(ctx!.UserPersona!.RealPageId, cancellationToken);
            var editorLoginTask     = _userLogin.GetUserLoginOnlyAsync(ctx.EditorPersona.RealPageId, cancellationToken);
            var productIdsTask      = _productRepository.GetProductIdsByCompanyAsync(
                ctx.EditorPersona.Organization.PartyId, cancellationToken);
            var productSettingsTask = _internalSettingRepository.GetProductInternalSettingsAsync(_productId, cancellationToken);
            var upPlatSettingsTask  = _internalSettingRepository.GetProductInternalSettingsAsync(
                (int)ProductEnum.UnifiedPlatform, cancellationToken);
            var existingRolesTask   = _userRoleRight.GetAssignedRoleForPersonaAsync(
                (ProductEnum)_productId, userPersonaId, cancellationToken: cancellationToken);
            var userPropIdsTask     = _propertyRepository.ListUPFMPropertyInstanceIdByPersonaAsync(
                userPersonaId, _productId, cancellationToken);

            await Task.WhenAll(userLoginTask, editorLoginTask, productIdsTask,
                               productSettingsTask, upPlatSettingsTask,
                               existingRolesTask, userPropIdsTask);

            var userLogin       = await userLoginTask;
            int editorUserId    = (int)((await editorLoginTask)?.UserId ?? 0L);
            var productIdList   = (await productIdsTask).ToList();
            var productSettings = await productSettingsTask;
            var userPropertyIdList = (await userPropIdsTask) ?? [];
            var roleList        = (await existingRolesTask)?.ToList() ?? [];

            int effectiveProductId = await GetSharedProductIdAsync(productIdList, cancellationToken);

            // ── Super-user override ────────────────────────────────────────────
            var orgTypeName = string.Empty;
            if (await _contextService.IsSuperUserAsync(ctx.UserPersona!, cancellationToken))
            {
                _logger.LogDebug("{ActionName} - {state}", "ManageUPFMProductUserAsync",
                    $"Super user detected. userPersonaId - {userPersonaId}");

                orgTypeName = ctx.UserPersona!.Organization.organizationType?.Name?.ToLower() ?? string.Empty;

                List<string> superUserRoleIds = productSettings
                    .FirstOrDefault(s => s.Name.Equals("SuperUserRoleId", StringComparison.OrdinalIgnoreCase))
                    ?.Value?.Split(',').ToList() ?? [];

                // Vendor-specific role override for VendorMarketplace
                string vmpVendorOrgType = string.Empty;
                if (_productId == (int)ProductEnum.VendorMarketplace &&
                    productSettings.Any(s => s.Name.Equals("VPMForVendorsOrgType", StringComparison.OrdinalIgnoreCase)))
                {
                    vmpVendorOrgType = productSettings
                        .First(s => s.Name.Equals("VPMForVendorsOrgType", StringComparison.OrdinalIgnoreCase))
                        .Value.ToLower();

                    if (orgTypeName == vmpVendorOrgType &&
                        productSettings.Any(s => s.Name.Equals("VendorSuperUserRoleId", StringComparison.OrdinalIgnoreCase)))
                    {
                        superUserRoleIds = productSettings
                            .First(s => s.Name.Equals("VendorSuperUserRoleId", StringComparison.OrdinalIgnoreCase))
                            .Value.Split(',').ToList();
                    }
                }

                // Build property removal list (exclude the all-properties sentinel -1)
                List<string> propertiesToRemove = [..
                    userPropertyIdList
                        .Where(p => p != -1)
                        .Select(p => p.ToString())];

                List<string> userRoleIdList;
                if (userAssignProductPropertyRole.IsVendorRoleIdOverride &&
                    userAssignProductPropertyRole.RoleList?.Count > 0)
                {
                    userRoleIdList = userAssignProductPropertyRole.RoleList;
                }
                else if (orgTypeName == vmpVendorOrgType && roleList.Count > 0 &&
                         _productId == (int)ProductEnum.VendorMarketplace)
                {
                    userRoleIdList = [.. roleList.Select(r => r.RoleID.ToString())];
                }
                else
                {
                    userRoleIdList = superUserRoleIds;
                }

                userAssignProductPropertyRole = new UPFMProductPropertyRole
                {
                    PropertyList        = ["-1"],
                    RemovedPropertyList = propertiesToRemove,
                    RoleList            = userRoleIdList
                };
            }

            var productLoginName = string.IsNullOrEmpty(ctx.ProductUsername)
                ? userLogin?.LoginName ?? string.Empty
                : ctx.ProductUsername;

            if (userAssignProductPropertyRole is not null)
            {
                var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);

                List<long> existingRoleIds  = [.. roleList.Select(r => r.RoleID)];
                List<long> assignedRoleIds  = [];

                // ── Roles: remove old, add new ─────────────────────────────────
                if (productPropertyRole.RoleList?.Count > 0)
                {
                    foreach (var item in productPropertyRole.RoleList)
                        assignedRoleIds.Add(long.Parse(item));

                    foreach (long roleId in existingRoleIds)
                    {
                        var result = await _userRoleRight.InsertAssignedRoleToUserAsync(
                            userPersonaId, roleId, editorUserId, deleteRole: true, cancellationToken);
                        if (result.Id < 0)
                        {
                            _logger.LogError("{ActionName} - {state}", "ManageUPFMProductUserAsync",
                                $"Unable to delete role. userPersonaId - {userPersonaId}, roleId - {roleId}");
                            return (result.ErrorMessage ?? string.Empty, auditParams);
                        }
                    }

                    foreach (long roleId in assignedRoleIds)
                    {
                        _logger.LogDebug("{ActionName} - {state}", "ManageUPFMProductUserAsync",
                            $"Adding role. userPersonaId - {userPersonaId}, roleId - {roleId}");
                        var result = await _userRoleRight.InsertAssignedRoleToUserAsync(
                            userPersonaId, roleId, editorUserId, deleteRole: false, cancellationToken);
                        if (result.Id < 0)
                        {
                            _logger.LogError("{ActionName} - {state}", "ManageUPFMProductUserAsync",
                                $"Unable to add role. userPersonaId - {userPersonaId}, roleId - {roleId}");
                            return (result.ErrorMessage ?? string.Empty, auditParams);
                        }
                    }
                }

                // ── Properties ────────────────────────────────────────────────
                if (userAssignProductPropertyRole.PropertyList?.Count > 0)
                {
                    // "ALL" sentinel → check if role grants access-all-properties
                    if (userAssignProductPropertyRole.PropertyList[0].Equals("ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        var orgProductIdList = (await _productRepository.GetProductIdsByCompanyAsync(
                            ctx.UserPersona!.Organization.PartyId, cancellationToken)).ToList();
                        var allRoles = await _productRepository.ListRolesForProductByPartyAsync(
                            ctx.UserPersona.Organization.PartyId, orgProductIdList, effectiveProductId, cancellationToken)
                            ?? [];

                        if (allRoles.Any(r =>
                            assignedRoleIds.Contains(long.Parse(r.ID)) && r.accessAllProperties))
                        {
                            userAssignProductPropertyRole.PropertyList = ["-1"];
                        }
                    }

                    List<string> assignedPropertyList  = userAssignProductPropertyRole.PropertyList ?? [];
                    List<string> unAssignedPropertyList = userAssignProductPropertyRole.RemovedPropertyList ?? [];

                    // Expand all-properties sentinel: remove individually assigned props
                    if (assignedPropertyList.Contains("-1"))
                    {
                        var toRemove = userPropertyIdList
                            .Where(p => p != -1)
                            .Select(p => p.ToString());
                        unAssignedPropertyList = [.. unAssignedPropertyList, .. toRemove];
                    }

                    // Validate: non-super users must have at least one property
                    if (!await _contextService.IsSuperUserAsync(ctx.UserPersona!, cancellationToken) &&
                        userAssignProductPropertyRole.IsAssigned &&
                        assignedPropertyList.Count == 0)
                    {
                        var doesNotUseProperties = productSettings
                            .FirstOrDefault(s => s.Name.Equals("DoesNotUseProperties", StringComparison.OrdinalIgnoreCase))?.Value;
                        if (doesNotUseProperties is null or not "1")
                        {
                            _logger.LogError("{ActionName} - {state}", "ManageUPFMProductUserAsync",
                                $"No properties found. userPersonaId - {userPersonaId}");
                            return ("No Properties are found to assign/unassign", auditParams);
                        }
                    }

                    var userPropertySet = new HashSet<int>(userPropertyIdList);

                    List<string> toAssign = [];
                    foreach (var propId in assignedPropertyList)
                    {
                        if (!userPropertySet.Contains(Convert.ToInt32(propId)) || isEmpAccess)
                            toAssign.Add(propId);
                    }

                    // Ensure -1 (all-props) is cleared when specific props are being added
                    if (unAssignedPropertyList.Count == 0 && toAssign.Count > 0 &&
                        userPropertyIdList.Contains(-1))
                    {
                        unAssignedPropertyList = ["-1"];
                    }

                    var propertyResult = await ProcessPropertyOperationsBatchedAsync(
                        userPersonaId, effectiveProductId, [.. unAssignedPropertyList], toAssign, cancellationToken);

                    if (!string.IsNullOrEmpty(propertyResult))
                    {
                        _logger.LogError("{ActionName} - {state}", "ManageUPFMProductUserAsync",
                            $"Property operations failed: {propertyResult}");
                        await _productRepository.UpdateProductSettingProductStatusAsync(
                            userPersonaId, _productId, "ProductStatus", (int)ProductBatchStatusType.Error, cancellationToken);
                        return (propertyResult, auditParams);
                    }
                }
                else if (!await _contextService.IsSuperUserAsync(ctx.UserPersona!, cancellationToken) &&
                         userAssignProductPropertyRole.IsAssigned &&
                         userAssignProductPropertyRole.PropertyList?.Count == 0)
                {
                    var doesNotUseProperties = productSettings
                        .FirstOrDefault(s => s.Name.Equals("DoesNotUseProperties", StringComparison.OrdinalIgnoreCase))?.Value;
                    if (doesNotUseProperties is null or not "1")
                    {
                        _logger.LogError("{ActionName} - {state}", "ManageUPFMProductUserAsync",
                            $"No properties found. userPersonaId - {userPersonaId}");
                        return ("No Properties are found to assign/unassign", auditParams);
                    }
                }

                // ── Audit parameters ─────────────────────────────────────────
                List<string> existingPropertyList = userAssignProductPropertyRole.PropertyList ?? [];

                var addedRoleList     = existingRoleIds.Count == 0
                    ? assignedRoleIds
                    : [.. assignedRoleIds.Except(existingRoleIds)];
                var removedRoleList   = existingRoleIds.Except(assignedRoleIds).ToList();

                var addedPropertyList = userPropertyIdList.Count == 0
                    ? existingPropertyList
                    : [.. existingPropertyList.Except(userPropertyIdList.Select(p => p.ToString()))];
                var removedPropertyList = userPropertyIdList
                    .Select(p => p.ToString())
                    .Except(existingPropertyList)
                    .ToList();

                var allProductsList = await _productRepository.GetAllProductsAsync(cancellationToken);
                string productName  = allProductsList
                    .FirstOrDefault(p => p.ProductId == _productId)?.Name ?? string.Empty;

                auditParams = await BuildAuditParametersAsync(
                    addedRoleList, removedRoleList,
                    addedPropertyList, removedPropertyList,
                    productName,
                    ctx.EditorPersona.Organization.PartyId,
                    userPersonaId,
                    cancellationToken);
            }

            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, _productId, "ProductStatus", (int)ProductBatchStatusType.Success, cancellationToken);

            sw.Stop();
            _logger.LogInformation("ManageUPFMProductUserAsync completed for {UserPersonaId} in {ElapsedMs}ms",
                userPersonaId, sw.ElapsedMilliseconds);

            return (string.Empty, auditParams);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sw.Stop();
            _logger.LogError(ex, "ManageUPFMProductUserAsync failed for {UserPersonaId} after {ElapsedMs}ms",
                userPersonaId, sw.ElapsedMilliseconds);
            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, _productId, "ProductStatus", (int)ProductBatchStatusType.Error, cancellationToken);
            return ($"Error - {ex.Message}", auditParams);
        }
    }

    /// <inheritdoc/>
    public async Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        UPFMProductPropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (ctx, error) = await _contextService.GetUserContextAsync(
                editorPersonaId, userPersonaId, _productId, cancellationToken);
            if (error is not null) return error.ErrorReason ?? string.Empty;

            var editorLogin  = await _userLogin.GetUserLoginOnlyAsync(ctx!.EditorPersona.RealPageId, cancellationToken);
            int editorUserId = (int)(editorLogin?.UserId ?? 0L);

            var roleList = await _userRoleRight.GetAssignedRoleForPersonaAsync(
                (ProductEnum)_productId, userPersonaId, cancellationToken: cancellationToken);

            if (roleList?.Count > 0)
            {
                long roleId = roleList[0].RoleID;
                var roleResult = await _userRoleRight.InsertAssignedRoleToUserAsync(
                    userPersonaId, roleId, editorUserId, deleteRole: true, cancellationToken);
                if (roleResult.Id < 0)
                {
                    _logger.LogError("{ActionName} - {state}", "UnassignUserAsync",
                        $"DeleteRole failed. userPersonaId - {userPersonaId}, roleId - {roleId}");
                    return roleResult.ErrorMessage ?? string.Empty;
                }

                // Get assigned property IDs and batch-remove all
                var propertyIds = await _propertyRepository.ListUPFMPropertyInstanceIdByPersonaAsync(
                    userPersonaId, _productId, cancellationToken);

                if (propertyIds?.Count > 0)
                {
                    var unassignList = propertyIds.Select(p => p.ToString()).ToList();
                    _logger.LogDebug("{ActionName} - {state}", "UnassignUserAsync",
                        $"Unassigning {unassignList.Count} properties for userPersonaId - {userPersonaId}");

                    var propResult = await ProcessPropertyOperationsBatchedAsync(
                        userPersonaId, _productId, unassignList, [], cancellationToken);

                    if (!string.IsNullOrEmpty(propResult))
                        _logger.LogWarning("{ActionName} - {state}", "UnassignUserAsync",
                            $"Property unassignment partial failure: {propResult}. Continuing.");
                }
            }

            _logger.LogInformation("{ActionName} - {state}", "UnassignUserAsync",
                $"userPersonaId - {userPersonaId} unassigned.");

            await _productRepository.UpdateProductSettingProductStatusAsync(
                userPersonaId, _productId, "ProductStatus", (int)ProductBatchStatusType.Deleted, cancellationToken);

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

    #region Company Instance / Multi-Company

    /// <inheritdoc/>
    public async Task<string> GetProductCompanyInstanceIdAsync(
        Guid organizationRealPageId,
        long booksCustmerMasterId,
        string blueBookProductName,
        string domain,
        string includeExtra = "",
        bool useTranslate = true,
        CancellationToken cancellationToken = default)
    {
        var companyList = await _blueBook.GetCompanyMapAsync(
            organizationRealPageId,
            booksCustmerMasterId,
            source: blueBookProductName.ToUpper(),
            domain: domain,
            includeExtra: includeExtra,
            useTranslate: useTranslate,
            cancellationToken: cancellationToken) ?? [];

        return companyList
            .FirstOrDefault(c => c.Source.Equals(blueBookProductName, StringComparison.OrdinalIgnoreCase))
            ?.CompanyInstanceSourceId ?? string.Empty;
    }

    /// <inheritdoc/>
    public async Task<List<UserCompaniesProperties>?> GetUPFMMultiCompanyPropertiesAsync(
        long editorPersonaId,
        string productCode,
        CancellationToken cancellationToken = default)
    {
        var (ctx, _) = await _contextService.GetUserContextAsync(
            editorPersonaId, editorPersonaId, _productId, cancellationToken);

        var editorLogin = await _userLogin.GetUserLoginOnlyAsync(
            ctx!.EditorPersona.RealPageId, cancellationToken);

        var companyResponse = await _userLogin.GetUserPersonaOrganizationAsync(
            editorLogin?.LoginName ?? string.Empty, cancellationToken: cancellationToken);

        if (companyResponse is null or { Count: 0 }) return null;

        bool isMultiCompany = companyResponse.Count > 1;
        List<UserCompaniesProperties> userCompaniesProperties = [];

        foreach (var company in companyResponse)
        {
            bool hasProduct = await _productRepository.IsProductAssignedAsync(
                company.PersonaId, (int)UserUiStatusType.AccountCreationSuccessful, _productId, cancellationToken);

            if (!hasProduct) continue;

            string companyInstanceSourceId = await GetProductCompanyInstanceIdAsync(
                company.OrganizationRealPageId,
                company.BooksCustomerMasterId,
                productCode,
                "Primary",
                cancellationToken: cancellationToken);

            string? multiCompanyRealPageId = isMultiCompany
                ? company.OrganizationRealPageId.ToString()
                : null;

            var propertyResponse = await GetEnterpriseUPFMPropertiesAsync(
                editorPersonaId,
                company.PersonaId,
                _productId,
                productCode,
                include: null,
                isMultiCompany: isMultiCompany,
                multiCompanyRealPageId: multiCompanyRealPageId,
                cancellationToken: cancellationToken);

            string errorReason = propertyResponse.Records is null or { Count: 0 }
                ? "Properties are not loaded from Blue Book"
                : string.Empty;

            var userCompanyProperties = new UserCompaniesProperties
            {
                Id               = companyInstanceSourceId,
                OrganizationName = company.OrganizationName,
                InstanceId       = company.OrganizationRealPageId,
                ErrorReason      = errorReason,
                Properties       = []
            };

            if (propertyResponse.Records is not null)
            {
                foreach (var record in propertyResponse.Records)
                {
                    var pp = (ProductProperty)record;
                    userCompanyProperties.Properties.Add(new Properties
                    {
                        Id           = pp.ID,
                        InstanceId   = pp.InstanceId,
                        PropertyName = pp.Name
                    });
                }
            }

            userCompaniesProperties.Add(userCompanyProperties);
        }

        return userCompaniesProperties.Count == 0 ? null : userCompaniesProperties;
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Async equivalent of <c>GetSharedProductDetails</c>.
    /// Returns the effective product ID (may differ from <see cref="_productId"/> when the product is
    /// configured as a shared alias). Also adds the resolved ID to <paramref name="productIdList"/> when missing.
    /// </summary>
    private async Task<int> GetSharedProductIdAsync(List<int> productIdList, CancellationToken ct)
    {
        var sharedProducts = await _internalSettingRepository.GetProductSettingByTypeAsync(
            SettingConstants.SharedProductSettingName, ct);

        if (sharedProducts?.FirstOrDefault(m => m.ProductId == _productId) is { } match &&
            int.TryParse(match.Value, out int resolvedId))
        {
            if (!productIdList.Contains(resolvedId))
                productIdList.Add(resolvedId);
            return resolvedId;
        }

        return _productId;
    }

    /// <summary>
    /// Resolves the UDM source code for <see cref="_productId"/> from the product master.
    /// Equivalent of <c>ManageProductBase._udmSourceCode</c>.
    /// </summary>
    private async Task<string> ResolveUdmSourceCodeAsync(CancellationToken ct)
    {
        var detail = await _productRepository.GetBooksMasterProductDetailAsync(_productId, ct);
        return detail?.UDMSourceCode?.Length > 0
            ? detail.UDMSourceCode
            : detail?.BooksProductCode ?? string.Empty;
    }

    /// <summary>
    /// Async equivalent of <c>GetProductPropertyInstancesBasedOnUPFMProperties</c>.
    /// Branches on the <c>DirectUDMTranslateProperty</c> product setting.
    /// </summary>
    private async Task<List<UPFMPropertyInstance>> GetProductPropertyInstancesAsync(
        string orgRealPageId, string udmSourceCode, int effectiveProductId, CancellationToken ct)
    {
        var productSettings = await _internalSettingRepository.GetProductInternalSettingsAsync(_productId, ct);

        bool directUDMTranslate = false;
        var translateSetting = productSettings.FirstOrDefault(
            s => s.Name.Equals("DirectUDMTranslateProperty", StringComparison.OrdinalIgnoreCase));
        if (translateSetting is not null)
            directUDMTranslate = translateSetting.Value == "1";

        if (directUDMTranslate)
        {
            var booksGuids = await _blueBook.GetUPFMPropertyInstancesAsync(orgRealPageId, ct);
            if (booksGuids is null or { Count: 0 }) return [];

            var properties = new UPFMProperty { id = [.. booksGuids.Select(g => g.ToString())] };
            var translated  = await _blueBook.GetTranslatePropertiesFromUPFMToProductv3Async(
                properties, udmSourceCode.ToUpper(), ct);

            if (translated?.Data?.Attributes is not { Count: > 0 }) return [];

            var translatedGuids = translated.Data.Attributes
                .Select(a => new Guid(a.PropertyInstanceSourceId))
                .ToList();

            var customerPropertyList = await _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIdsAsync(
                translatedGuids, ct);

            // Attach the translated CustomerPropertyId to each instance
            var translatedDict = translated.Data.Attributes.ToDictionary(
                a => a.PropertyInstanceSourceId,
                a => a,
                StringComparer.OrdinalIgnoreCase);

            foreach (var cp in customerPropertyList)
            {
                if (translatedDict.TryGetValue(cp.InstanceId.ToString(), out var attr))
                    cp.CustomerPropertyId = attr.TranslatedPropertyInstances[0].PropertyInstanceSourceId;
            }

            return customerPropertyList;
        }
        else
        {
            var booksGuids = await _blueBook.GetPropertiesPerProductCenterAsync(orgRealPageId, effectiveProductId, ct);
            return booksGuids is null or { Count: 0 }
                ? []
                : await _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIdsAsync(booksGuids, ct);
        }
    }

    /// <summary>
    /// Merges the BlueBook UPFM property list with the user's assigned properties.
    /// Uses <see cref="HashSet{T}"/> for O(1) membership checks.
    /// </summary>
    private async Task<ListResponse> MergeUPFMBooksPropertiesWithProductPropertyAsync(
        IList<UPFMPropertyInstance> blueBookPropertyList,
        long userPersonaId,
        int effectiveProductId,
        bool assignedOnly,
        CancellationToken ct)
    {
        var userPropertyIdList = await _propertyRepository.ListUPFMPropertyInstanceIdByPersonaAsync(
            userPersonaId, effectiveProductId, ct);

        bool allProperties = userPropertyIdList.Any(p => p == -1);
        var  propertyOption = new Dictionary<string, bool> { ["allProperties"] = allProperties };

        var userPropertySet = new HashSet<int>(userPropertyIdList);
        var productPropertyList = new List<ProductProperty>(blueBookPropertyList.Count);

        foreach (var upfm in blueBookPropertyList)
        {
            var pp = ConvertUPFMPropertyInstanceToProductProperty(upfm, false);
            if (allProperties || userPropertySet.Contains(upfm.PropertyInstanceId))
                pp.IsAssigned = true;

            if (!assignedOnly || pp.IsAssigned == true)
                productPropertyList.Add(pp);
        }

        return new ListResponse
        {
            Records     = productPropertyList.Cast<object>().ToList(),
            TotalRows   = productPropertyList.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1,
            Additional  = propertyOption
        };
    }

    /// <summary>
    /// Async equivalent of <c>MergeSelRolesWithGreenbook</c>.
    /// Marks assigned roles; applies default role when none assigned.
    /// </summary>
    private async Task<ListResponse> MergeSelRolesWithGreenbookAsync(
        IList<ProductRole> allRoles, long userPersonaId, CancellationToken ct)
    {
        var assignedRoles = await _userRoleRight.GetAssignedRoleForPersonaAsync(
            (ProductEnum)_productId, userPersonaId, cancellationToken: ct);

        if (assignedRoles?.Count > 0)
        {
            var assignedIds = new HashSet<string>(assignedRoles.Select(r => r.RoleID.ToString()));
            foreach (var role in allRoles)
                if (assignedIds.Contains(role.ID))
                    role.IsAssigned = true;
        }

        // Apply default role if nothing is assigned
        if (allRoles?.Any(r => r.IsAssigned) == false)
        {
            var defaultRole = allRoles.FirstOrDefault(
                r => r.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase));
            if (defaultRole is not null) defaultRole.IsAssigned = true;
        }

        return new ListResponse
        {
            Records     = allRoles!.Cast<object>().ToList(),
            TotalRows   = allRoles.Count,
            RowsPerPage = 9999,
            ErrorReason = string.Empty,
            TotalPages  = 1
        };
    }

    /// <summary>
    /// Async equivalent of <c>ProcessPropertyOperationsBatched</c>.
    /// <para>
    /// The <c>-1</c> all-properties sentinel is routed through
    /// <see cref="IPropertyRepositoryAsync.InsertRemoveAssignedPropertyInstanceToUserAsync"/> because
    /// the TVP stored procedure does not accept <c>-1</c> as a valid <c>PropertyInstanceID</c>.
    /// </para>
    /// <para>
    /// Real property IDs are deduplicated via a <see cref="Dictionary{TKey,TValue}"/> before being
    /// sent to the TVP, preventing PRIMARY KEY violations (SQL error 3602) that can occur when the
    /// same ID appears in both the unassign and assign lists due to upstream list aliasing.
    /// Assign takes priority over unassign for the same ID.
    /// </para>
    /// </summary>
    private async Task<string> ProcessPropertyOperationsBatchedAsync(
        long userPersonaId,
        int effectiveProductId,
        List<string> unassignedProperties,
        List<string> assignedProperties,
        CancellationToken ct)
    {
        int total = unassignedProperties.Count + assignedProperties.Count;
        if (total == 0) return string.Empty;

        _logger.LogDebug("{ActionName} - {state}", "ProcessPropertyOperationsBatchedAsync",
            $"Processing {total} properties (Unassign: {unassignedProperties.Count}, Assign: {assignedProperties.Count})");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // ── Sentinel -1 (all-properties access) ───────────────────────────────
            // The TVP stored procedure cannot accept -1 as a PropertyInstanceID.
            // Route sentinel rows through the individual insert/remove method instead.
            foreach (var prop in unassignedProperties.Where(p => p == "-1"))
            {
                var sentinelResult = await _propertyRepository.InsertRemoveAssignedPropertyInstanceToUserAsync(
                    userPersonaId, effectiveProductId, -1, remove: 1, ct);
                if (sentinelResult.Id < 0)
                {
                    _logger.LogError("{ActionName} - {state}", "ProcessPropertyOperationsBatchedAsync",
                        $"Failed to remove sentinel -1 for userPersonaId {userPersonaId}: {sentinelResult.ErrorMessage}");
                    return sentinelResult.ErrorMessage ?? "Failed to remove all-properties sentinel";
                }
            }
            foreach (var prop in assignedProperties.Where(p => p == "-1"))
            {
                var sentinelResult = await _propertyRepository.InsertRemoveAssignedPropertyInstanceToUserAsync(
                    userPersonaId, effectiveProductId, -1, remove: 0, ct);
                if (sentinelResult.Id < 0)
                {
                    _logger.LogError("{ActionName} - {state}", "ProcessPropertyOperationsBatchedAsync",
                        $"Failed to assign sentinel -1 for userPersonaId {userPersonaId}: {sentinelResult.ErrorMessage}");
                    return sentinelResult.ErrorMessage ?? "Failed to assign all-properties sentinel";
                }
            }

            // ── Real property IDs — deduplicated TVP batch ─────────────────────────
            // Use a Dictionary keyed on PropertyInstanceID to collapse duplicates that can
            // arise when the RemovedPropertyList and PropertyList share the same ID. Assign
            // takes priority over unassign (assigned list processed last = last-write wins).
            var seen = new Dictionary<long, UPFMPropertyInstanceMapping>();

            foreach (var prop in unassignedProperties)
                if (long.TryParse(prop, out long id) && id > 0)
                    seen[id] = new UPFMPropertyInstanceMapping { PropertyInstanceID = id, IsDeleted = true };

            foreach (var prop in assignedProperties)
                if (long.TryParse(prop, out long id) && id > 0)
                    seen[id] = new UPFMPropertyInstanceMapping { PropertyInstanceID = id, IsDeleted = false };

            var mappings = seen.Values.ToList();
            if (mappings.Count == 0)
            {
                sw.Stop();
                return string.Empty;
            }

            var result = await _propertyRepository.BulkInsertRemovePropertyInstanceMappingsAsync(
                userPersonaId, effectiveProductId, mappings, ct);

            sw.Stop();

            if (result.Id < 0 || !string.IsNullOrEmpty(result.ErrorMessage))
            {
                _logger.LogError("{ActionName} - {state}", "ProcessPropertyOperationsBatchedAsync",
                    $"Bulk failed after {sw.ElapsedMilliseconds}ms. Error: {result.ErrorMessage}");
                return $"Property bulk operation failed: {result.ErrorMessage}";
            }

            _logger.LogInformation(
                "Bulk property operations for {UserPersonaId}: {Count}/{Total} in {ElapsedMs}ms",
                userPersonaId, result.Id, total, sw.ElapsedMilliseconds);

            return string.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sw.Stop();
            _logger.LogError(ex, "{ActionName} - {state}", "ProcessPropertyOperationsBatchedAsync",
                $"Exception after {sw.ElapsedMilliseconds}ms for {total} properties");
            return $"Critical error: {ex.Message}";
        }
    }

    /// <summary>
    /// Async equivalent of <c>AssignedRoleandPropertyNameList</c>.
    /// Uses <see cref="HashSet{T}"/> for O(1) role/property lookups in audit generation.
    /// </summary>
    private async Task<List<AdditionalParameters>> BuildAuditParametersAsync(
        List<long>   addedRoleList,
        List<long>   removedRoleList,
        List<string> addedPropertyList,
        List<string> removedPropertyList,
        string       productName,
        long         partyId,
        long         userPersonaId,
        CancellationToken ct)
    {
        try
        {
            var productIdList = (await _productRepository.GetProductIdsByCompanyAsync(partyId, ct)).ToList();
            int effectiveProductId = await GetSharedProductIdAsync(productIdList, ct);

            // Parallel: roles + property instances
            var rolesTask  = _productRepository.ListRolesForProductByPartyAsync(
                partyId, productIdList, effectiveProductId, ct);
            var udmTask    = ResolveUdmSourceCodeAsync(ct);

            // GetProductPropertyInstancesAsync needs the org real page ID — use editor context
            var (ctx, _) = await _contextService.GetUserContextAsync(
                userPersonaId, 0, _productId, ct);   // userPersonaId used as editor for this audit query
            string orgRealPageId = ctx?.EditorPersona.Organization.RealPageId.ToString() ?? string.Empty;
            string udmSourceCode = await udmTask;

            var propsTask = GetProductPropertyInstancesAsync(orgRealPageId, udmSourceCode, effectiveProductId, ct);
            await Task.WhenAll(rolesTask, propsTask);

            var gbAllRoles            = await rolesTask  ?? [];
            var customerPropertyList  = await propsTask;

            var addedRoleSet    = new HashSet<long>(addedRoleList);
            var removedRoleSet  = new HashSet<long>(removedRoleList);
            var addedPropSet    = new HashSet<string>(addedPropertyList, StringComparer.OrdinalIgnoreCase);
            var removedPropSet  = new HashSet<string>(removedPropertyList, StringComparer.OrdinalIgnoreCase);

            List<AdditionalParameters> result = [];

            foreach (var role in gbAllRoles)
            {
                if (!long.TryParse(role.ID, out long rid)) continue;
                if (addedRoleSet.Contains(rid))
                    result.Add(new AdditionalParameters
                    {
                        Key   = $"{productName} Roles",
                        Value = RolesAssignMsg.Replace("RoleName", role.Name)
                    });
                if (removedRoleSet.Contains(rid))
                    result.Add(new AdditionalParameters
                    {
                        Key   = $"{productName} Roles",
                        Value = RolesRemovedMsg.Replace("RoleName", role.Name)
                    });
            }

            foreach (var prop in customerPropertyList)
            {
                string propId = prop.PropertyInstanceId.ToString();
                if (addedPropSet.Contains(propId))
                    result.Add(new AdditionalParameters
                    {
                        Key   = $"{productName} Properties",
                        Value = PropsAssignMsg.Replace("PropertyName", prop.Name)
                    });
                if (removedPropSet.Contains(propId))
                    result.Add(new AdditionalParameters
                    {
                        Key   = $"{productName} Properties",
                        Value = PropsRemovedMsg.Replace("PropertyName", prop.Name)
                    });
            }

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "{ActionName} - {state}", "BuildAuditParametersAsync",
                $"Unable to get role/property names. userPersonaId - {userPersonaId}");
            return [];
        }
    }

    /// <summary>Maps <see cref="UPFMProductPropertyRole"/> — copies only the role list.</summary>
    private static UPFMProductPropertyRole MapGbObjectToProduct(UPFMProductPropertyRole source)
    {
        var result = new UPFMProductPropertyRole();
        if (source.RoleList?.Count > 0)
            result.RoleList = [.. source.RoleList];
        return result;
    }

    /// <summary>Converts a <see cref="UPFMPropertyInstance"/> to a <see cref="ProductProperty"/>.</summary>
    private static ProductProperty ConvertUPFMPropertyInstanceToProductProperty(
        UPFMPropertyInstance upfm, bool isAssigned) => new()
    {
        ID                = upfm.CustomerPropertyId.ToString(),
        Name              = upfm.Name,
        Street1           = upfm.Address,
        City              = upfm.City,
        State             = upfm.State,
        Zip               = upfm.PostalCode,
        IsAssigned        = isAssigned,
        InstanceId        = upfm.InstanceId.ToString(),
        Latitude          = upfm.Latitude,
        Longitude         = upfm.Longitude,
        Alias             = upfm.PropertyInstanceId.ToString()
    };

    #endregion
}
