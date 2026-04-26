using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.ClickPay;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True async implementation of product-panel orchestration.
/// <para>
/// Delegates product-specific operations to <see cref="IIntegrationTypeFactoryAsync"/>,
/// calling the native-async <see cref="IIntegrationTypeAsync"/> methods directly — no
/// <c>Task.Run</c> wrappers are required.
/// </para>
/// <para>
/// Repository and BlueBook calls use true async interfaces backed by
/// <c>IDbConnectionFactory</c>. <c>DefaultUserClaim</c> has been removed from all
/// method signatures — context is resolved internally via <see cref="IManagePersonaAsync"/>
/// and <see cref="IProductRepositoryAsync"/>.
/// </para>
/// </summary>
public sealed class ManageProductPanelAsync : IManageProductPanelAsync
{
    private readonly IIntegrationTypeFactoryAsync             _integrationTypeFactory;
    private readonly IProductRepositoryAsync                  _productRepository;
    private readonly IPersonaRepositoryAsync                  _personaRepository;
    private readonly IManagePersonaAsync                      _managePersona;
    private readonly IManageBlueBookAsync                     _manageBlueBook;
    private readonly IManageProductOneSiteAccountingAsync     _oneSiteAccounting;
    private readonly IManageProductRumAsync                   _rum;
    private readonly ILogger<ManageProductPanelAsync>         _logger;

    public ManageProductPanelAsync(
        IIntegrationTypeFactoryAsync             integrationTypeFactory,
        IProductRepositoryAsync                  productRepository,
        IPersonaRepositoryAsync                  personaRepository,
        IManagePersonaAsync                      managePersona,
        IManageBlueBookAsync                     manageBlueBook,
        IManageProductOneSiteAccountingAsync     oneSiteAccounting,
        IManageProductRumAsync                   rum,
        ILogger<ManageProductPanelAsync>         logger)
    {
        ArgumentNullException.ThrowIfNull(integrationTypeFactory); _integrationTypeFactory = integrationTypeFactory;
        ArgumentNullException.ThrowIfNull(productRepository);      _productRepository      = productRepository;
        ArgumentNullException.ThrowIfNull(personaRepository);      _personaRepository      = personaRepository;
        ArgumentNullException.ThrowIfNull(managePersona);          _managePersona          = managePersona;
        ArgumentNullException.ThrowIfNull(manageBlueBook);         _manageBlueBook         = manageBlueBook;
        ArgumentNullException.ThrowIfNull(oneSiteAccounting);      _oneSiteAccounting      = oneSiteAccounting;
        ArgumentNullException.ThrowIfNull(rum);                    _rum                    = rum;
        ArgumentNullException.ThrowIfNull(logger);                 _logger                 = logger;
    }

    // ── GetProductPropertiesAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductPropertiesAsync(
        long editorPersonaId, long userPersonaId, int productId,
        RequestParameter datafilter,
        CancellationToken ct = default)
    {
        try
        {
            var integration = _integrationTypeFactory.GetIntegration(productId);
            var result      = await integration.GetPropertiesAsync(editorPersonaId, userPersonaId, datafilter, ct);

            if (result.IsError)
                throw new Exception(result.ErrorReason);

            bool usePrimaryProperty = false;

            if (userPersonaId > 0)
            {
                // Resolve persona product settings and org-level product settings in parallel
                var personaSettingsTask = _personaRepository.GetPersonaProductSettingsAsync(userPersonaId, ct);
                var editorPersonaTask   = _managePersona.GetPersonaAsync(editorPersonaId, withRights: false, ct);
                await Task.WhenAll(personaSettingsTask, editorPersonaTask);

                var personaProductSettings = await personaSettingsTask;
                var orgRealPageGuid        = (await editorPersonaTask).Organization.RealPageId;

                var productSetting = personaProductSettings.FirstOrDefault(item =>
                    item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                    && item.ProductId == productId);

                IList<ProductSettingList> productSettingByOrg;
                ProductSettingList?       usePrimaryPropertiesOrgFlag;

                if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId))
                {
                    productSettingByOrg         = await _productRepository.GetProductSettingsAsync(orgRealPageGuid, (int)ProductEnum.AssetOptimizer, ct);
                    usePrimaryPropertiesOrgFlag = productSettingByOrg.FirstOrDefault(item =>
                        item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                        && item.ProductId == (int)ProductEnum.AssetOptimizer);
                }
                else
                {
                    productSettingByOrg         = await _productRepository.GetProductSettingsAsync(orgRealPageGuid, productId, ct);
                    usePrimaryPropertiesOrgFlag = productSettingByOrg.FirstOrDefault(item =>
                        item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                        && item.ProductId == productId);
                }

                if (productSetting is not null)
                    usePrimaryProperty = productSetting.Value.Trim() == "1";

                // Org flag overrides persona flag: "0" → false, anything else → keep persona value
                usePrimaryProperty = usePrimaryPropertiesOrgFlag is null
                    ? false
                    : usePrimaryPropertiesOrgFlag.Value.Trim() == "0" ? false : usePrimaryProperty;
            }

            // Merge Additional dictionary
            Dictionary<string, bool> additionalInfo = [];

            if (productId == 18 && result.Additional is Dictionary<string, string> strDict)
            {
                foreach (var pair in strDict)
                {
                    if (pair.Key.Equals("accessType", StringComparison.OrdinalIgnoreCase))
                        additionalInfo[pair.Value] = true;
                }
            }

            additionalInfo["usePrimaryProperties"] = usePrimaryProperty;

            if (result.Additional is Dictionary<string, bool> boolDict)
            {
                foreach (var pair in boolDict)
                {
                    if (!pair.Key.Equals("usePrimaryProperties", StringComparison.OrdinalIgnoreCase))
                        additionalInfo[pair.Key] = pair.Value;
                }
            }

            result.Additional = additionalInfo;
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetProductPropertiesAsync - error for productId {ProductId}", productId);
            return new ListResponse { IsError = true, ErrorReason = ResolvePropertyError(ex) };
        }
    }

    // ── GetProductGroupPropertiesAsync ────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductGroupPropertiesAsync(
        long editorPersonaId, long userPersonaId, int productId,
        string propertyGroupId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        var integration = _integrationTypeFactory.GetIntegration(productId);
        return await integration.GetPropertiesByGroupAsync(editorPersonaId, userPersonaId, propertyGroupId, datafilter, ct);
    }

    // ── GetProductPropertyGroupsAsync ─────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductPropertyGroupsAsync(
        long editorPersonaId, long userPersonaId, int productId,
        RequestParameter datafilter,
        bool assignedOnly = false, string userLoginName = "",
        CancellationToken ct = default)
    {
        try
        {
            var integration = _integrationTypeFactory.GetIntegration(productId);
            var result      = await integration.GetPropertyGroupsAsync(editorPersonaId, userPersonaId, datafilter, userLoginName, ct);

            if (result.IsError)
                throw new Exception(result.ErrorReason);

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetProductPropertyGroupsAsync - error for productId {ProductId}", productId);
            var errorResult = new ListResponse { IsError = true };

            if (ex is BlueBookException)
                errorResult.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
            else if (ex.Message == CommonMessageConstants.RegionErrorMessage
                  || ex.Message == CommonMessageConstants.CompanyTabErrorMessage)
                errorResult.ErrorReason = ex.Message;
            else if (ex.Message == CommonMessageConstants.CompanyErrorMessage
                  || ex.InnerException?.Message == CommonMessageConstants.CompanyErrorMessage)
                errorResult.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
            else
                errorResult.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;

            return errorResult;
        }
    }

    // ── GetProductLocationGroupsAsync ─────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductLocationGroupsAsync(
        long editorPersonaId, long userPersonaId, int productId,
        RequestParameter datafilter,
        bool assignedOnly = false, string userLoginName = "",
        CancellationToken ct = default)
    {
        ListResponse result = productId switch
        {
            (int)ProductEnum.FinancialSuite =>
                await _oneSiteAccounting.GetUserPropertyGroupsAsync(editorPersonaId, userPersonaId, datafilter, ct),

            (int)ProductEnum.UtilityManagement =>
                await _rum.GetUMGlobalRolesAsync(editorPersonaId, userPersonaId, datafilter, ct),

            _ => new ListResponse()
        };

        if (result.IsError)
        {
            _logger.LogError("GetProductLocationGroupsAsync - error for productId {ProductId}: {Error}", productId, result.ErrorReason);
            throw new Exception(result.ErrorReason);
        }

        return result;
    }

    // ── GetProductRolesAsync ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductRolesAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        int productId, RequestParameter datafilter, AccessType? accessType,
        CancellationToken ct = default)
    {
        try
        {
            var integration = _integrationTypeFactory.GetIntegration(productId);
            var result      = await integration.GetRolesAsync(editorPersonaId, userPersonaId, partyId, accessType, datafilter, ct);

            if (result.IsError)
                throw new Exception(result.ErrorReason);

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetProductRolesAsync - error for productId {ProductId}", productId);
            return ResolveRoleError(ex);
        }
    }

    // ── GetUserProductRolesAsync ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<RoleTemplateProductRoleMapping> GetUserProductRolesAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        CancellationToken ct = default)
    {
        List<RoleTemplateProduct> userProducts = [];
        List<string>              productError = [];
        var                       datafilter   = new RequestParameter();

        try
        {
            // Unified Platform roles
            var upIntegration = _integrationTypeFactory.GetIntegration((int)ProductEnum.UnifiedPlatform);
            var upResult      = await upIntegration.GetRolesAsync(editorPersonaId, userPersonaId, partyId, null, datafilter, ct);

            if (!upResult.IsError)
            {
                var assignedUpRoles = upResult.Records
                    .Cast<UnifiedLoginRoleRights>()
                    .Where(p => p.IsAssigned)
                    .ToList();

                if (assignedUpRoles.Count > 0)
                {
                    userProducts.Add(new RoleTemplateProduct
                    {
                        ProductId             = (int)ProductEnum.UnifiedPlatform,
                        RoleTemplateProductId = 0,
                        Roles                 = assignedUpRoles.Select(role => new RoleTemplateRoles
                        {
                            RoleId                           = role.RoleId.ToString(),
                            RoleName                         = role.Role,
                            RoleTemplateProductRoleMappingID = 0
                        }).ToList()
                    });
                }
            }

            // Persona-assigned product roles
            var personaProducts = await _productRepository.ListProductsByPersonaIdAsync(
                userPersonaId, (int)UserUiStatusType.AccountCreationSuccessful, ct);

            foreach (var product in personaProducts)
            {
                try
                {
                    var integration = _integrationTypeFactory.GetIntegration(product.ProductId);
                    var result      = await integration.GetRolesAsync(editorPersonaId, userPersonaId, partyId, AccessType.Property, datafilter, ct);

                    if (!result.IsError && result.Records is { Count: > 0 })
                    {
                        var assignedRoles = ExtractAssignedRoles(result.Records.ToList());
                        if (assignedRoles.Count > 0)
                        {
                            userProducts.Add(new RoleTemplateProduct
                            {
                                ProductId             = product.ProductId,
                                RoleTemplateProductId = 0,
                                Roles                 = assignedRoles
                            });
                        }
                    }
                    else
                    {
                        productError.Add(product.ProductName);
                    }
                }
                catch
                {
                    productError.Add(product.ProductName);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetUserProductRolesAsync - error for userPersonaId {UserPersonaId}", userPersonaId);
            throw;
        }

        return new RoleTemplateProductRoleMapping
        {
            PartyId        = partyId,
            RoleTemplateId = 0,
            Products       = userProducts,
            ProductsError  = productError
        };
    }

    // ── GetProductUserGroupsAsync ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductUserGroupsAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        int productId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        try
        {
            var integration = _integrationTypeFactory.GetIntegration(productId);
            var result      = await integration.GetUserGroupsAsync(editorPersonaId, userPersonaId, partyId, datafilter, ct);

            if (result.IsError)
                throw new Exception(result.ErrorReason);

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "GetProductUserGroupsAsync - error for productId {ProductId}", productId);
            var errorResult = new ListResponse { IsError = true };

            if (ex is BlueBookException || ex.InnerException is BlueBookException)
                errorResult.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
            else if (ex.Message.Equals(CommonMessageConstants.UserGroupsErrorMessage, StringComparison.OrdinalIgnoreCase))
                errorResult.ErrorReason = ex.Message;
            else if (ex.Message.Equals(CommonMessageConstants.CompanyErrorMessage, StringComparison.OrdinalIgnoreCase)
                  || ex.InnerException?.Message.Equals(CommonMessageConstants.CompanyErrorMessage, StringComparison.OrdinalIgnoreCase) == true)
                errorResult.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
            else
                errorResult.ErrorReason = CommonMessageConstants.UserGroupsErrorMessage;

            return errorResult;
        }
    }

    // ── GetProductRightsForRoleAsync (int) ────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductRightsForRoleAsync(
        long editorPersonaId, int roleId, long partyId,
        int productId, RequestParameter datafilter,
        bool assignedToRoleOnly = false,
        CancellationToken ct = default)
    {
        var integration = _integrationTypeFactory.GetIntegration(productId);
        return await integration.GetRightsForRoleAsync(editorPersonaId, 0L, roleId, partyId, assignedToRoleOnly, datafilter, ct);
    }

    // ── GetProductRightsForRoleAsync (string) ─────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductRightsForRoleAsync(
        long editorPersonaId, string roleId, long partyId,
        int productId, RequestParameter datafilter,
        bool assignedToRoleOnly = false,
        CancellationToken ct = default)
    {
        var integration = _integrationTypeFactory.GetIntegration(productId);

        // StandardV1 products expose roles as strings natively; all other integration
        // types (Legacy / UPFM) identify roles by numeric ID — parse before delegating.
        return _integrationTypeFactory.GetIntegrationTypeForProductId(productId) == ProductIntegrationTypeEnum.StandardV1
            ? await integration.GetRightsForRoleAsync(editorPersonaId, 0L, roleId, partyId, assignedToRoleOnly, datafilter, ct)
            : await integration.GetRightsForRoleAsync(editorPersonaId, 0L, long.Parse(roleId), partyId, assignedToRoleOnly, datafilter, ct);
    }

    // ── GetProductRightsAsync ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductRightsAsync(
        long editorPersonaId, long userPersonaId, long partyId,
        int productId, RequestParameter datafilter,
        CancellationToken ct = default)
    {
        // UtilityManagement exposes its global roles (UM service areas) as "rights"
        // in the product panel.  All other products delegate through GetAllRightsAsync.
        if (productId == (int)ProductEnum.UtilityManagement)
            return await _rum.GetRolesAsync(editorPersonaId, userPersonaId, datafilter, ct);

        var integration = _integrationTypeFactory.GetIntegration(productId);
        return await integration.GetAllRightsAsync(editorPersonaId, userPersonaId, datafilter, ct);
    }

    // ── GetProductOrganizationsAsync ──────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ListResponse> GetProductOrganizationsAsync(
        long editorPersonaId, long userPersonaId, int productId,
        string organizationRoleId, string organizationType,
        CancellationToken ct = default)
    {
        var integration = _integrationTypeFactory.GetIntegration(productId);
        return await integration.GetOrganizationsAsync(editorPersonaId, userPersonaId, organizationRoleId, organizationType, ct);
    }

    // ── CompareProductAndPrimaryPropertiesAsync ───────────────────────────────

    /// <inheritdoc/>
    public Task<ListResponse> CompareProductAndPrimaryPropertiesAsync(
        UPFMProperty upfmProperty, int productId, ListResponse productResult,
        CancellationToken ct = default)
    {
        if (productResult?.Records is not { Count: > 0 })
            return Task.FromResult(productResult!);

        return _manageBlueBook.TranslateProductPrimaryPropertiesDataAsync(upfmProperty, productId, productResult, ct);
    }

    // ── TranslateProductPropertiesAsync ──────────────────────────────────────

    /// <inheritdoc/>
    public async Task<UPFMProperty> TranslateProductPropertiesAsync(
        UPFMProperty upfmProperty, int productId,
        CancellationToken ct = default)
    {
        var primaryPropertyIds = new UPFMProperty
        {
            id = upfmProperty.id.ConvertAll(d => d.ToLower())
        };

        var productList   = await _productRepository.GetAllProductsAsync(ct);
        var udmSourceCode = ProductEnumHelper.GetUDMSourceCodeByProductId(productId, productList);
        var productCode   = ProductEnumHelper.GetProductCodeByProductId(productId, productList);

        if (!string.IsNullOrEmpty(udmSourceCode))
            productCode = udmSourceCode;

        var translatedData = await _manageBlueBook.GetTranslatePropertiesFromProductToUPFMAsync(
            primaryPropertyIds, productCode, ct);

        List<string> translatedInstances = [];
        if (translatedData?.Data?.Attributes is not null)
        {
            foreach (var attr in translatedData.Data.Attributes)
                foreach (var prop in attr.TranslatedPropertyInstances)
                    translatedInstances.Add(prop.PropertyInstanceSourceId);
        }

        return new UPFMProperty { id = translatedInstances };
    }

    // ── GetPersonaProductPrimaryPropertiesAsync ───────────────────────────────

    /// <inheritdoc/>
    public Task<List<PersonaProductProperty>> GetPersonaProductPrimaryPropertiesAsync(
        long userPersonaId,
        CancellationToken ct = default)
        => _productRepository.GetPersonaProductPrimaryPropertiesAsync(userPersonaId, ct);

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Maps a property-fetch exception to the appropriate
    /// <see cref="CommonMessageConstants"/> error reason, preserving the
    /// pass-through convention for PropertyGroup and Entity sentinel errors.
    /// </summary>
    private static string ResolvePropertyError(Exception ex)
    {
        if (ex is BlueBookException)
            return CommonMessageConstants.CompanyErrorMessage;

        if (ex.Message == CommonMessageConstants.PropertyGroupErrorMessage
         || ex.Message == CommonMessageConstants.EntityErrorMessage)
            return ex.Message;

        if (ex.Message == CommonMessageConstants.CompanyErrorMessage
         || ex.InnerException?.Message == CommonMessageConstants.CompanyErrorMessage)
            return CommonMessageConstants.CompanyErrorMessage;

        return CommonMessageConstants.PropertyErrorMessage;
    }

    /// <summary>
    /// Maps a role-fetch exception to the appropriate
    /// <see cref="CommonMessageConstants"/> error reason, preserving the
    /// pass-through convention for the Right sentinel error.
    /// </summary>
    private static ListResponse ResolveRoleError(Exception ex)
    {
        var result = new ListResponse { IsError = true };

        if (ex is BlueBookException || ex.InnerException is BlueBookException)
            result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
        else if (ex.Message.Equals(CommonMessageConstants.RightErrorMessage, StringComparison.OrdinalIgnoreCase))
            result.ErrorReason = ex.Message;
        else if (ex.Message.Equals(CommonMessageConstants.CompanyErrorMessage, StringComparison.OrdinalIgnoreCase)
              || ex.InnerException?.Message.Equals(CommonMessageConstants.CompanyErrorMessage, StringComparison.OrdinalIgnoreCase) == true)
            result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
        else
            result.ErrorReason = CommonMessageConstants.RoleErrorMessage;

        return result;
    }

    /// <summary>
    /// Extracts assigned roles from a <see cref="ListResponse.Records"/> collection,
    /// handling all known product-role types (<see cref="SharedObjects.Product.ProductRole"/>,
    /// <see cref="ClickPayRole"/>, <c>ProductIntegration.Model.ProductRole</c>,
    /// <see cref="Level"/>, and <see cref="SharedObjects.Product.Rum.Role"/>).
    /// </summary>
    private static List<RoleTemplateRoles> ExtractAssignedRoles(List<object> records)
    {
        if (records is not { Count: > 0 }) return [];

        var type = records[0].GetType();

        if (type == typeof(SharedObjects.Product.ProductRole))
            return records.Cast<SharedObjects.Product.ProductRole>()
                .Where(p => p.IsAssigned)
                .Select(r => new RoleTemplateRoles { RoleId = r.ID, RoleName = r.Name, RoleTemplateProductRoleMappingID = 0 })
                .ToList();

        if (type == typeof(ClickPayRole))
            return records.Cast<ClickPayRole>()
                .Where(p => p.IsAssigned)
                .Select(r => new RoleTemplateRoles { RoleId = r.Id, RoleName = r.Name, RoleTemplateProductRoleMappingID = 0 })
                .ToList();

        if (type == typeof(Logic.ProductIntegration.Model.ProductRole))
            return records.Cast<Logic.ProductIntegration.Model.ProductRole>()
                .Where(p => p.IsAssigned)
                .Select(r => new RoleTemplateRoles { RoleId = r.GetRoleId, RoleName = r.GetName, RoleTemplateProductRoleMappingID = 0 })
                .ToList();

        if (type == typeof(Level))
            return records.Cast<ILevel>()
                .Where(p => p.IsAssigned)
                .Select(r => new RoleTemplateRoles { RoleId = r.Id, RoleName = r.Name, RoleTemplateProductRoleMappingID = 0 })
                .ToList();

        if (type == typeof(SharedObjects.Product.Rum.Role))
            return records.Cast<SharedObjects.Product.Rum.Role>()
                .Where(p => p.IsAssigned)
                .Select(r => new RoleTemplateRoles { RoleId = r.Id.ToString(), RoleName = r.Name, RoleTemplateProductRoleMappingID = 0 })
                .ToList();

        return [];
    }
}
