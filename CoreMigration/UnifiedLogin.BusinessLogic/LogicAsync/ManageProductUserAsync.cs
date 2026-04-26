using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.CacheHelper;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.Helpers;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Product;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Cache;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Accounting;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.Product.UPFMProduct;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Native async implementation of <see cref="IManageProductUserAsync"/>.
/// Has <b>zero</b> dependency on the sync <c>ManageProductUser</c> class or any factory.
/// </summary>
public sealed class ManageProductUserAsync : IManageProductUserAsync
{
    #region Fields

    private readonly IUserClaimsAccessor                    _userClaimsAccessor;
    private readonly IProductRepositoryAsync                _productRepository;
    private readonly ISamlRepositoryAsync                   _samlRepository;
    private readonly IPersonaRepositoryAsync                _personaRepository;
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepository;
    private readonly IIntegrationTypeFactoryAsync            _integrationTypeFactory;
    private readonly IManagePersonaAsync                    _managePersona;
    private readonly ProductUserActivityLogHelper           _activityLogHelper;
    private readonly ICacheService                          _cache;
    private readonly IHttpClientFactory                     _httpClientFactory;
    private readonly ITokenHelperAsync                      _tokenHelper;
    private readonly IManageProductBatchAsync               _productBatch;
    private readonly ILogger<ManageProductUserAsync>        _logger;

    private static readonly CacheEntryOptions ProductSettingsCacheOptions =
        new() { ExpirationTimeInMinutes = 2 };

    #endregion

    #region Constructor

    public ManageProductUserAsync(
        IUserClaimsAccessor                    userClaimsAccessor,
        IProductRepositoryAsync                productRepository,
        ISamlRepositoryAsync                   samlRepository,
        IPersonaRepositoryAsync                personaRepository,
        IProductInternalSettingRepositoryAsync productInternalSettingRepository,
        IIntegrationTypeFactoryAsync           integrationTypeFactory,
        IManagePersonaAsync                    managePersona,
        ProductUserActivityLogHelper           activityLogHelper,
        ICacheService                          cache,
        IHttpClientFactory                     httpClientFactory,
        ITokenHelperAsync                      tokenHelper,
        IManageProductBatchAsync               productBatch,
        ILogger<ManageProductUserAsync>        logger)
    {
        _userClaimsAccessor               = userClaimsAccessor               ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
        _productRepository                = productRepository                ?? throw new ArgumentNullException(nameof(productRepository));
        _samlRepository                   = samlRepository                   ?? throw new ArgumentNullException(nameof(samlRepository));
        _personaRepository                = personaRepository                ?? throw new ArgumentNullException(nameof(personaRepository));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _integrationTypeFactory           = integrationTypeFactory           ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
        _managePersona                    = managePersona                    ?? throw new ArgumentNullException(nameof(managePersona));
        _activityLogHelper                = activityLogHelper                ?? throw new ArgumentNullException(nameof(activityLogHelper));
        _cache                            = cache                            ?? throw new ArgumentNullException(nameof(cache));
        _httpClientFactory                = httpClientFactory                ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _tokenHelper                      = tokenHelper                      ?? throw new ArgumentNullException(nameof(tokenHelper));
        _productBatch                     = productBatch                     ?? throw new ArgumentNullException(nameof(productBatch));
        _logger                           = logger                           ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<string> CreateProductUserAsync(
        ProductUserProperitiesRoles productUser,
        CancellationToken cancellationToken = default)
    {
        int    productId               = productUser.ProductId;
        bool   isUpdateUser            = false;
        bool   isCreateUserWithNoProps = true;
        string result                  = string.Empty;
        string prodUserInputJson       = string.Empty;

        var rolePropDictionary       = new Dictionary<int, RolePropertyList>();
        var rolePrimaryPropDictionary = new Dictionary<int, RolePropertyList>();
        var usePrimaryPropertyFlags  = new Dictionary<int, bool>();
        var additionalParameters     = new List<AdditionalParameters>();

        // ── 1. Parse input JSON (pure logic — no I/O) ─────────────────────
        if (ValidateDictionaryMapping(productUser.InputJson))
        {
            prodUserInputJson = productUser.InputJson;
            foreach (var (key, val) in JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(productUser.InputJson.Trim())!)
                rolePropDictionary.Add((int)Enum.Parse(typeof(ProductEnum), key), val);
        }
        else
        {
            rolePropDictionary.Add(productId,
                JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson)!);
        }

        try
        {
            // ── 2. Is this an update or a new assignment? ──────────────────
            var productAttributes = await _samlRepository.GetProductSamlDetailsAsync(
                productUser.AssignUserPersonaId, productId, cancellationToken);
            isUpdateUser = productAttributes.Any();

            // ── 3. Current UsePrimaryProperties flag ───────────────────────
            if (productUser.AssignUserPersonaId > 0)
            {
                var personaSettings = await _personaRepository.GetPersonaProductSettingsAsync(
                    productUser.AssignUserPersonaId, cancellationToken);
                if (personaSettings.FirstOrDefault(s =>
                        s.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                        && s.ProductId == productId) is { } setting)
                {
                    _ = bool.TryParse(setting.Value.Trim() == "1" ? "true" : "false", out var usePP);
                    _ = usePP; // captured below per rolePropertyList iteration
                }
            }

            // ── 4. No-property products list ───────────────────────────────
            var productsWithNoProperties = await GetProductsWithNoPropertiesAsync(cancellationToken);

            // ── 5. Per-product role/property processing ────────────────────
            foreach (var (rpKey, rpVal) in rolePropDictionary)
            {
                usePrimaryPropertyFlags[rpKey] = rpVal.UsePrimaryProperties;

                var foundPrimaryProps = await AssignPrimaryPropertiesToProductBatchAsync(
                    productUser, rpVal, productsWithNoProperties, cancellationToken).ConfigureAwait(false);
                if (foundPrimaryProps is not null)
                    rolePrimaryPropDictionary[rpKey] = foundPrimaryProps;

                if (rpVal.UsePrimaryProperties
                    && !productsWithNoProperties.Contains(productId)
                    && rpVal.IsAssigned == true
                    && rpVal.PropertyList?.Count == 0)
                {
                    var activeProducts = await _samlRepository.ListActiveProductsByPersonaIdAsync(
                        productUser.AssignUserPersonaId, 0, string.Empty, cancellationToken);
                    if (!activeProducts.Any(a => a.ProductId == productId))
                        isCreateUserWithNoProps = false;

                    UnassignProductInJson(productUser, ref prodUserInputJson);
                }
                else if (productId == (int)ProductEnum.KnockCRM)
                {
                    await ApplyKnockCrmRoleAsync(productUser, cancellationToken);
                }
            }

            if (!string.IsNullOrEmpty(prodUserInputJson))
                productUser.InputJson = prodUserInputJson;

            // ── 6. Call integration ────────────────────────────────────────────────────────
            if (isCreateUserWithNoProps)
            {
                var integration = _integrationTypeFactory.GetIntegration(productId);
                await _productRepository.UpdateBatchProcessorLogAsync(
                    productUser.ProductBatchId, DateTime.UtcNow, null, cancellationToken);

                (result, var addParams) = await integration.CreateUserAsync(productUser, cancellationToken).ConfigureAwait(false);
                additionalParameters.AddRange(addParams);
            }
        }
        catch (Exception ex)
        {
            var inner = ex;
            while (inner.InnerException is not null) inner = inner.InnerException;
            result = inner.Message;
            _logger.LogError(ex,
                "CreateProductUser failed. ProductId={ProductId} PersonaId={PersonaId} Error={Error}",
                productId, productUser.AssignUserPersonaId, inner.Message);
        }
        finally
        {
            await _productRepository.UpdateBatchProcessorLogAsync(
                productUser.ProductBatchId, null, DateTime.UtcNow, cancellationToken);
        }

        // ── 7. Batch completion ────────────────────────────────────────────
        bool isBatchCompleted = false;
        try
        {
            if (string.IsNullOrEmpty(result))
            {
                foreach (var (ppKey, ppVal) in rolePrimaryPropDictionary)
                {
                    bool usePP = usePrimaryPropertyFlags.GetValueOrDefault(ppKey);
                    await SavePersonaProductPrimaryPropertiesAsync(
                        usePP, productUser.AssignUserPersonaId, ppKey, ppVal,
                        productUser.InputJson, cancellationToken);
                }

                isBatchCompleted = await _productRepository.UpdateProductBatchAsync(
                    productUser.ProductBatchId, (int)ProductBatchStatusType.Success,
                    productUser.InputJson, null, cancellationToken);

                if (additionalParameters.Count > 0)
                    await _productRepository.UpdateProductActivityLogAsync(
                        productUser.BatchProcessorGroupId, productId,
                        JsonConvert.SerializeObject(additionalParameters), cancellationToken);

                // ── Sync properties via true-async HTTP ────────────────────
                var roleProp = JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson);
                var settings = await GetProductInternalSettingsAsync(productId, cancellationToken);
                string? doesNotUseProperties = settings
                    .FirstOrDefault(s => s.Name.Equals("DoesNotUseProperties", StringComparison.OrdinalIgnoreCase))?.Value;

                if ((doesNotUseProperties is null || doesNotUseProperties != "1") && roleProp!.IsAssigned)
                {
                    var syncTargets = rolePropDictionary.Count > 1
                        ? rolePropDictionary.Keys.ToList()
                        : [productId];

                    foreach (int targetProductId in syncTargets)
                        await SyncUserProductPropertiesAsync(
                            targetProductId, productUser.AssignUserPersonaId,
                            productUser.CreateUserPersonaId, cancellationToken);
                }
            }
            else
            {
                if (result.Equals(ProductBatchStatusType.Stop.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    isBatchCompleted = await _productRepository.UpdateProductBatchAsync(
                        productUser.ProductBatchId, (int)ProductBatchStatusType.Stop, null,
                        "Batch Process stopped due to internal error for this product.", cancellationToken);
                }
                else
                {
                    isBatchCompleted = await _productRepository.UpdateProductBatchAsync(
                        productUser.ProductBatchId, (int)ProductBatchStatusType.Error, null,
                        result, cancellationToken);

                    if (!isUpdateUser && isCreateUserWithNoProps)
                        await _productRepository.UpdateProductSettingProductStatusAsync(
                            productUser.AssignUserPersonaId, productId,
                            "ProductStatus", (int)ProductBatchStatusType.Error, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch status update failed. BatchGroupId={BatchGroupId}",
                productUser.BatchProcessorGroupId);
        }

        // ── 8. Activity log ────────────────────────────────────────────────
        if (isBatchCompleted)
        {
            try
            {
                var productActivityLog = (await _productRepository.GetProductActivityLogAsync(
                    productUser.BatchProcessorGroupId, cancellationToken)).ToList();
                await _productRepository.ClearPersonaErrorAsync(
                    productUser.AssignUserPersonaId, productId, cancellationToken);
                await WriteActivityLogAsync(
                    productUser.CreateUserPersonaId, productUser.AssignUserPersonaId,
                    productUser.BatchProcessorGroupId, productUser.ImpersonatorUserId,
                    productActivityLog, cancellationToken);
                await _productRepository.DeleteProductActivityLogAsync(
                    productUser.BatchProcessorGroupId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Activity log write failed. BatchGroupId={BatchGroupId}",
                    productUser.BatchProcessorGroupId);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<string> UpdateProductUserAccountDetailsAsync(
        ProductUserAccountDetails productUser,
        CancellationToken cancellationToken = default)
    {
        var integration = _integrationTypeFactory.GetIntegration(productUser.ProductId);
        return await integration.UpdateUserDetailsAsync(productUser, false, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<string> DeleteSamlUserProductInfoAndStatusAsync(
        ProductUserAccountDetails productUser,
        CancellationToken cancellationToken = default)
    {
        await _samlRepository.DeleteSamlUserProductInfoAndStatusAsync(
            productUser.PersonaId, productUser.ProductId, cancellationToken);

        var fromUserInfo = await _activityLogHelper.GetUserActivityLogInfo(_userClaimsAccessor.PersonaId);
        var toUserInfo   = await _activityLogHelper.GetUserActivityLogInfo(productUser.PersonaId);
        var products     = await _productRepository.ListProductsAsync(
            productUser.ProductId, null, null, null, cancellationToken);
        var product      = products.First();

        string userName = string.IsNullOrEmpty(_userClaimsAccessor.ImpersonatedByName)
            ? $"{_userClaimsAccessor.FirstName} {_userClaimsAccessor.LastName}"
            : $" RealPage Access ({_userClaimsAccessor.ImpersonatedByName}) ";

        if (_userClaimsAccessor.OrganizationRealPageGuid != DefaultUserClaim.EmployeeCompanyRealPageId
            && productUser.ProductId == (int)ProductEnum.EasyLMS)
        {
            _activityLogHelper.PushToQueue(fromUserInfo, toUserInfo,
                $"{toUserInfo.FirstName} {toUserInfo.LastName}'s {product.Name} data is removed by {userName}",
                "PRODUCT_ACCESS");
        }

        return string.Empty;
    }

    /// <inheritdoc/>
    public async Task<IList<ProductBatchStatus>> GetProductStatusesAsync(
        long assignUserPersonaId,
        CancellationToken cancellationToken = default)
    {
        if (_userClaimsAccessor.UserRealPageGuid == Guid.Empty)
            throw new InvalidOperationException("User claim contains an empty RealPage Id.");
        if (assignUserPersonaId == 0)
            throw new ArgumentException("assignUserId not supplied.", nameof(assignUserPersonaId));

        return await _productRepository.ListProductBatchStatusesAsync(
            _userClaimsAccessor.UserRealPageGuid, assignUserPersonaId, cancellationToken);
    }

    // ── Batch-pipeline methods ─────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<string> ChangeUserTypeAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default)
    {
        string result = string.Empty;
        try
        {
            var integration = _integrationTypeFactory.GetIntegration(batchRecord.ProductId);
            result = await integration.ChangeUserTypeAsync(batchRecord, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var inner = ex;
            while (inner.InnerException is not null) inner = inner.InnerException;
            result = inner.Message;
            _logger.LogError(ex,
                "ChangeUserType failed. ProductId={ProductId} PersonaId={PersonaId}",
                batchRecord.ProductId, batchRecord.AssignUserPersonaId);
        }

        bool completed;
        if (string.IsNullOrEmpty(result))
        {
            completed = await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Success,
                null, null, cancellationToken).ConfigureAwait(false);
        }
        else if (result.Equals(ProductBatchStatusType.Stop.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            completed = await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Stop,
                null, "Batch Process stopped due to internal error for this product.",
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            completed = await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Error,
                null, result, cancellationToken).ConfigureAwait(false);
        }

        if (completed)
        {
            try
            {
                var log = (await _productRepository.GetProductActivityLogAsync(
                    batchRecord.BatchProcessorGroupId, cancellationToken)).ToList();
                await WriteActivityLogAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    batchRecord.BatchProcessorGroupId, batchRecord.ImpersonatorUserId,
                    log, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Activity log write failed. BatchGroupId={BatchGroupId}",
                    batchRecord.BatchProcessorGroupId);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<string> UpdateProductUserProfileAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default)
    {
        string result = string.Empty;
        try
        {
            var integration = _integrationTypeFactory.GetIntegration(batchRecord.ProductId);
            result = await integration.UpdateUserProfileAsync(batchRecord, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var inner = ex;
            while (inner.InnerException is not null) inner = inner.InnerException;
            result = inner.Message;
            _logger.LogError(ex,
                "UpdateProductUserProfile failed. ProductId={ProductId} PersonaId={PersonaId}",
                batchRecord.ProductId, batchRecord.AssignUserPersonaId);
        }

        if (string.IsNullOrEmpty(result))
        {
            await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Success,
                null, null, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Error,
                null, result, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<string> CreateEnterpriseRoleProductUserAsync(
        ProductUserProperitiesRoles batchRecord,
        CancellationToken cancellationToken = default)
    {
        int    productId = batchRecord.ProductId;
        string result    = string.Empty;

        try
        {
            await _productRepository.UpdateBatchProcessorLogAsync(
                batchRecord.ProductBatchId, DateTime.UtcNow, null, cancellationToken)
                .ConfigureAwait(false);

            var integration = _integrationTypeFactory.GetIntegration(productId);
            (result, _) = await integration.CreateUserAsync(batchRecord, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var inner = ex;
            while (inner.InnerException is not null) inner = inner.InnerException;
            result = inner.Message;
            _logger.LogError(ex,
                "CreateEnterpriseRoleProductUser failed. ProductId={ProductId} PersonaId={PersonaId}",
                productId, batchRecord.AssignUserPersonaId);
        }
        finally
        {
            await _productRepository.UpdateBatchProcessorLogAsync(
                batchRecord.ProductBatchId, null, DateTime.UtcNow, cancellationToken)
                .ConfigureAwait(false);
        }

        bool completed;
        if (string.IsNullOrEmpty(result))
        {
            completed = await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Success,
                batchRecord.InputJson, null, cancellationToken).ConfigureAwait(false);
        }
        else if (result.Equals(ProductBatchStatusType.Stop.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            completed = await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Stop,
                null, "Batch Process stopped due to internal error for this product.",
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            completed = await _productRepository.UpdateProductBatchAsync(
                batchRecord.ProductBatchId, (int)ProductBatchStatusType.Error,
                null, result, cancellationToken).ConfigureAwait(false);
        }

        if (completed)
        {
            try
            {
                var log = (await _productRepository.GetProductActivityLogAsync(
                    batchRecord.BatchProcessorGroupId, cancellationToken)).ToList();
                await _productRepository.ClearPersonaErrorAsync(
                    batchRecord.AssignUserPersonaId, productId, cancellationToken)
                    .ConfigureAwait(false);
                await WriteActivityLogAsync(
                    batchRecord.CreateUserPersonaId, batchRecord.AssignUserPersonaId,
                    batchRecord.BatchProcessorGroupId, batchRecord.ImpersonatorUserId,
                    log, cancellationToken).ConfigureAwait(false);
                await _productRepository.DeleteProductActivityLogAsync(
                    batchRecord.BatchProcessorGroupId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Activity log write failed. BatchGroupId={BatchGroupId}",
                    batchRecord.BatchProcessorGroupId);
            }
        }

        return result;
    }

    #endregion

    #region Private — Async helpers

    private async ValueTask<List<ProductInternalSetting>> GetProductInternalSettingsAsync(
        int productId, CancellationToken cancellationToken)
    {
        var result = await _cache.GetOrSetAsync(
            $"productInternalSetting_{productId}",
            async _ => (await _productInternalSettingRepository
                .GetProductInternalSettingsAsync(productId, cancellationToken))?.ToList(),
            ProductSettingsCacheOptions);

        return result ?? [];
    }

    private async Task<List<int>> GetProductsWithNoPropertiesAsync(CancellationToken cancellationToken)
    {
        var settings = await GetProductInternalSettingsAsync(
            productId: (int)ProductEnum.UnifiedPlatform, cancellationToken);

        var value = settings
            .FirstOrDefault(s => s.Name.Equals(
                "UserAccessDetails_ProductsWithNoProperties",
                StringComparison.OrdinalIgnoreCase))?.Value;

        if (string.IsNullOrEmpty(value)) return [];

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .Distinct()
            .ToList();
    }

    private async Task SavePersonaProductPrimaryPropertiesAsync(
        bool usePrimaryProperties, long assignUserPersonaId, int productId,
        RolePropertyList roleProp, string inputJson, CancellationToken cancellationToken)
    {
        if (productId != (int)ProductEnum.AssetOptimizer)
        {
            if (usePrimaryProperties && roleProp.ProductPrimaryProperties?.Count > 0)
                await _productRepository.SavePersonaProductPropertiesAsync(
                    assignUserPersonaId, productId,
                    JsonConvert.SerializeObject(roleProp.ProductPrimaryProperties), cancellationToken);

            await _productRepository.UpdateProductSettingProductStatusAsync(
                assignUserPersonaId, productId, "UsePrimaryProperties",
                usePrimaryProperties ? 1 : 0, cancellationToken);
        }
        else
        {
            var aoData = JsonConvert.DeserializeObject<AoUserCompanyPropertyRoleDetails>(inputJson);
            if (aoData?.AoUserCompanyPropertyRoleDetailList is null) return;

            foreach (var item in aoData.AoUserCompanyPropertyRoleDetailList)
            {
                if (item.ProductId == 0) continue;

                if (item.UsePrimaryProperties && item.ProductPrimaryProperties is not null)
                {
                    if (item.ProductPrimaryProperties.Count == 0) item.IsAssigned = false;
                    await _productRepository.SavePersonaProductPropertiesAsync(
                        assignUserPersonaId, item.ProductId,
                        JsonConvert.SerializeObject(item.ProductPrimaryProperties), cancellationToken);
                }

                await _productRepository.UpdateProductSettingProductStatusAsync(
                    assignUserPersonaId, item.ProductId, "UsePrimaryProperties",
                    item.UsePrimaryProperties ? 1 : 0, cancellationToken);
            }
        }
    }

    private async Task SyncUserProductPropertiesAsync(
        int productId, long personaId, long editorPersonaId,
        CancellationToken cancellationToken)
    {
        try
        {
            var settings = await GetProductInternalSettingsAsync(productId: 3, cancellationToken);

            string baseUri = settings.First(s => s.Name.Equals(
                "UnifiedLoginApiBaseUri", StringComparison.OrdinalIgnoreCase)).Value;
            string scopes  = settings.First(s => s.Name.Equals(
                "ULInternalClientTokenScopes", StringComparison.OrdinalIgnoreCase)).Value;

            var products     = await _productRepository.GetAllProductsAsync(cancellationToken);
            string code      = ProductEnumHelper.GetBooksSourceCodeByProductId(productId, products);
            string endpoint  = $"/apicore/v2/UserSync?syncJobType=2&forceCreate=false&editorPersonaId={editorPersonaId}";

            var payload = new[]
            {
                new UserSyncRequest { PersonaId = personaId, Sources = [code], ForceCreate = false }
            };

            string token = await _tokenHelper.GetUnifiedLoginServerTokenAsync(scopes, cancellationToken);

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var request = new HttpRequestMessage(HttpMethod.Post, baseUri + endpoint)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "SyncUserProductProperties failed. ProductId={ProductId} PersonaId={PersonaId} Status={Status} Body={Body}",
                    productId, personaId, (int)response.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SyncUserProductProperties threw. PersonaId={PersonaId} ProductId={ProductId}",
                personaId, productId);
        }
    }

    private async Task WriteActivityLogAsync(
        long fromPersonaId, long toPersonaId, int batchGroupId, long impersonatorUserId,
        List<AdditionalParameters> additionalParameters, CancellationToken cancellationToken)
    {
        var fromUserInfo = await _activityLogHelper.GetUserActivityLogInfo(fromPersonaId);
        var toUserInfo   = await _activityLogHelper.GetUserActivityLogInfo(toPersonaId);

        // Resolve impersonator display name — reuse GetUserActivityLogInfo since it works for any persona
        string? impersonatorName = null;
        if (impersonatorUserId > 0)
        {
            var imp = await _activityLogHelper.GetUserActivityLogInfo(impersonatorUserId);
            impersonatorName = $"{imp.FirstName} {imp.LastName}";
        }

        // Primary-org-name lookup (requires IManageOrganizationAsync — deferred for now)
        string primaryOrgName = string.Empty;

        var data = await _productRepository.GetUserBatchDetailsAsync(
            batchGroupId, fromPersonaId, toPersonaId, cancellationToken);

        if (data is null || data.Count == 0) return;

        foreach (var item in data.Where(d => !string.IsNullOrEmpty(d.InputJSON)))
        {
            var role = JsonConvert.DeserializeObject<UPFMProductPropertyRole>(item.InputJSON.Trim());
            item.IsAssigned = role?.IsAssigned ?? false;
        }

        if (data[0].BatchProcessorGroupActivityLogged) return;

        var success = data.Where(x => x.StatusTypeId == 8).ToList();
        if (success.Count > 0)
            GenerateQueueMessage(fromUserInfo, toUserInfo, success, isSuccess: true,
                impersonatorName, primaryOrgName, fromPersonaId, additionalParameters);

        var failed = data.Where(x => x.StatusTypeId == 7).ToList();
        if (failed.Count > 0)
            GenerateQueueMessage(fromUserInfo, toUserInfo, failed, isSuccess: false,
                impersonatorName, primaryOrgName, fromPersonaId);

        await _productRepository.UpdateBatchGroupStatusAsync(batchGroupId, isLogged: true, cancellationToken);
    }

    private void GenerateQueueMessage(
        UserActivityLogInfo fromUserInfo, UserActivityLogInfo toUserInfo,
        List<UserBatchProductDetail> items, bool isSuccess,
        string? impersonatorName, string primaryOrgName,
        long fromPersonaId, List<AdditionalParameters>? additionalParameters = null)
    {
        string actor = impersonatorName is not null
            ? $"RealPage Access ({impersonatorName})"
            : $"{fromUserInfo.FirstName} {fromUserInfo.LastName}";

        if (isSuccess)
        {
            var assigned   = new List<string>();
            var unassigned = new List<string>();

            foreach (var item in items)
            {
                if (item.IsAssigned)
                {
                    if (item.ProductId == (int)ProductEnum.AssetOptimizer)
                    {
                        assigned.AddRange(GetAOProductsForActivity(item, isAssigned: true, statusTypeId: 8));
                        unassigned.AddRange(GetAOProductsForActivity(item, isAssigned: false, statusTypeId: 8));
                    }
                    else assigned.Add(item.Name);
                }
                else unassigned.Add(item.Name);
            }

            if (assigned.Count > 0)
                _activityLogHelper.PushToQueue(fromUserInfo, toUserInfo,
                    $"{actor} updated access for {toUserInfo.FirstName} {toUserInfo.LastName}: Access was granted to {string.Join(", ", assigned)}.",
                    "PRODUCT_ACCESS", additionalParameters);

            if (unassigned.Count > 0)
            {
                string prefix = !string.IsNullOrEmpty(primaryOrgName)
                    ? $"Owner Company ({primaryOrgName}) Deactivated user and updated access for {toUserInfo.FirstName} {toUserInfo.LastName}:"
                    : $"{actor} updated access for {toUserInfo.FirstName} {toUserInfo.LastName}:";

                var unassignParams = unassigned.Select(name => new AdditionalParameters
                {
                    Key = "Product Access",
                    Value = $"{{\"action\":\"Unassigned\",\"value\":\"{name}\"}}"
                }).ToList();

                _activityLogHelper.PushToQueue(fromUserInfo, toUserInfo,
                    $"{prefix} Access was unassigned from {string.Join(", ", unassigned)}.",
                    "PRODUCT_ACCESS", unassignParams);
            }
        }
        else
        {
            var failed = items
                .SelectMany(item => item.ProductId == (int)ProductEnum.AssetOptimizer
                    ? GetAOProductsForActivity(item, isAssigned: true, statusTypeId: 7)
                    : [(item.Name)])
                .ToList();

            string msg =
                $"An exception occurred when {actor} attempted to update product access for " +
                $"{toUserInfo.FirstName} {toUserInfo.LastName} in {string.Join(", ", failed)}.";

            _activityLogHelper.PushToQueue(fromUserInfo, toUserInfo, msg, "PRODUCT_ACCESS");
            SendNotificationFireAndForget(msg + " Please contact RealPage Support for assistance.", fromPersonaId);
        }
    }

    private void SendNotificationFireAndForget(string message, long notificationTo)
        => _ = SendNotificationAsync(message, notificationTo);

    private async Task SendNotificationAsync(string message, long notificationTo)
    {
        try
        {
            var settings = await GetProductInternalSettingsAsync(
                (int)ProductEnum.UnifiedPlatform, CancellationToken.None);

            var notification = new RealPage.UnifiedNotifications.Notification(
                settings.First(s => s.Name.Equals("UnifiedLoginServerClientName",  StringComparison.OrdinalIgnoreCase)).Value,
                Encoding.UTF8.GetString(Convert.FromBase64String(
                    settings.First(s => s.Name.Equals("UnifiedLoginServerClientSecret", StringComparison.OrdinalIgnoreCase)).Value)),
                settings.First(s => s.Name.Equals("TokenEndPoint",               StringComparison.OrdinalIgnoreCase)).Value,
                settings.First(s => s.Name.Equals("NotificationsApiEndPoint",    StringComparison.OrdinalIgnoreCase)).Value + "/v1/notifications",
                settings.First(s => s.Name.Equals("NotificationsApiEndPoint",    StringComparison.OrdinalIgnoreCase)).Value + "/" +
                settings.First(s => s.Name.Equals("NotificationsEventsEndPoint", StringComparison.OrdinalIgnoreCase)).Value);

            await notification.SendNotification(
                "User Update Exception", message,
                [notificationTo.ToString()],
                settings.First(s => s.Name.Equals("NotificationCategoryCode", StringComparison.OrdinalIgnoreCase)).Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendNotification failed for persona {PersonaId}", notificationTo);
        }
    }

    #endregion

    #region Private — Async helpers (primary properties)

    private async Task<RolePropertyList?> AssignPrimaryPropertiesToProductBatchAsync(
        ProductUserProperitiesRoles productUser, RolePropertyList roleProp,
        List<int> productsWithNoProperties, CancellationToken cancellationToken)
    {
        string? productType = null;

        if (productUser.ProductId != (int)ProductEnum.AssetOptimizer && roleProp.UsePrimaryProperties)
        {
            var propertyList = await _productBatch.GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
                productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, productUser.ProductId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (propertyList.Records?.Count > 0)
            {
                roleProp.ProductPrimaryProperties = GetSelectedProperties(propertyList, productType);
                roleProp.PropertyList = roleProp.ProductPrimaryProperties?.Select(p => p.ProductPropertyId).ToList() ?? [];
                productUser.InputJson = JsonConvert.SerializeObject(roleProp);
            }
        }
        else if (productUser.ProductId == (int)ProductEnum.AssetOptimizer)
        {
            var aoData = JsonConvert.DeserializeObject<AoUserCompanyPropertyRoleDetails>(productUser.InputJson);
            if (aoData?.AoUserCompanyPropertyRoleDetailList is not null)
            {
                foreach (var item in aoData.AoUserCompanyPropertyRoleDetailList)
                {
                    if (item.UsePrimaryProperties && !productsWithNoProperties.Contains(item.ProductId))
                    {
                        var props = await _productBatch.GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
                            productUser.CreateUserPersonaId, productUser.AssignUserPersonaId, item.ProductId,
                            cancellationToken: cancellationToken).ConfigureAwait(false);

                        if (props.Records.Count > 0)
                        {
                            item.ProductPrimaryProperties = GetSelectedProperties(props, productType);
                            item.SelectedPortfolioValues = item.ProductPrimaryProperties?
                                .Select(p => int.Parse(p.ProductPropertyId)).ToList() ?? [];

                            if (item.ProductPrimaryProperties?.Count == 0) item.IsAssigned = false;
                        }
                    }
                }
            }
            productUser.InputJson = JsonConvert.SerializeObject(aoData);
        }

        return roleProp;
    }

    private async Task ApplyKnockCrmRoleAsync(
        ProductUserProperitiesRoles productUser, CancellationToken cancellationToken)
    {
        var editorPersona = await _managePersona.GetPersonaAsync(
            productUser.CreateUserPersonaId, cancellationToken: cancellationToken).ConfigureAwait(false);
        var integration = _integrationTypeFactory.GetIntegration(productUser.ProductId);
        var knockRoles  = await integration.GetRolesAsync(
            productUser.CreateUserPersonaId, 0, editorPersona!.OrganizationPartyId,
            AccessType.Property, null!, cancellationToken).ConfigureAwait(false);
        var roleProp    = JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson)!;

        if (knockRoles.Records?.Count > 0
            && knockRoles.Records[0] is Logic.ProductIntegration.Model.ProductRole)
        {
            var roles = knockRoles.Records.Cast<Logic.ProductIntegration.Model.ProductRole>().ToList();
            var master = roles.FirstOrDefault(r => r.GetName.Equals("master", StringComparison.OrdinalIgnoreCase));
            if (master is not null && roleProp.RoleList?.Count > 0 && master.GetRoleId == roleProp.RoleList[0])
                roleProp.IsAssigned = true;
        }

        productUser.InputJson = JsonConvert.SerializeObject(roleProp);
    }

    private static void UnassignProductInJson(
        ProductUserProperitiesRoles productUser, ref string prodUserInputJson)
    {
        if (ValidateDictionaryMapping(productUser.InputJson))
        {
            prodUserInputJson = productUser.InputJson;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(productUser.InputJson.Trim())!;
            foreach (var item in dict.Values) item.IsAssigned = false;
            productUser.InputJson = JsonConvert.SerializeObject(dict);
        }
        else
        {
            var rpl = JsonConvert.DeserializeObject<RolePropertyList>(productUser.InputJson)!;
            rpl.IsAssigned = false;
            productUser.InputJson = JsonConvert.SerializeObject(rpl);
        }
    }

    private static bool ValidateDictionaryMapping(string json)
    {
        try { JsonConvert.DeserializeObject<Dictionary<string, RolePropertyList>>(json.Trim()); return true; }
        catch { return false; }
    }



    private static List<string> GetAOProductsForActivity(
        UserBatchProductDetail item, bool isAssigned, int statusTypeId)
    {
        var aoData = JsonConvert.DeserializeObject<AoUserCompanyPropertyRoleDetails>(item.InputJSON.Trim());
        if (aoData?.AoUserCompanyPropertyRoleDetailList is null) return [];

        var filteredList = aoData.AoUserCompanyPropertyRoleDetailList
            .Where(p => p.ProductId != (int)ProductEnum.AoBenchmarking)
            .ToList();

        var targets = statusTypeId == 8
            ? filteredList.Where(p => p.IsAssigned == isAssigned).ToList()
            : (IEnumerable<AoUserCompanyPropertyRoleDetail>)filteredList;

        return targets.Select(p => ProductEnumHelper.GetAoProductDescription((ProductEnum)p.ProductId)).ToList();
    }

    // Note: GetSelectedProperties kept as in ManageProductUser — pure type-switch logic, no I/O
    private static List<ProductPrimaryProperties> GetSelectedProperties(ListResponse result, string? integrationType)
        => result.Records switch
        {
            _ when result.Records[0] is ProductProperty => result.Records.Cast<ProductProperty>()
                .Where(p => p.IsAssigned == true)
                .Select(p => new ProductPrimaryProperties
                {
                    PropertyInstanceId = p.InstanceId,
                    ProductPropertyId  = integrationType?.Equals("UPFM", StringComparison.OrdinalIgnoreCase) == true ? p.Alias : p.ID
                }).ToList(),
            _ when result.Records[0] is ACProperty => result.Records.Cast<ACProperty>()
                .Where(p => p.IsAssigned == true)
                .Select(p => new ProductPrimaryProperties { ProductPropertyId = p.Id, PropertyInstanceId = p.InstanceId }).ToList(),
            _ when result.Records[0] is UPFMPropertyInstance => result.Records.Cast<UPFMPropertyInstance>()
                .Where(p => p.IsAssigned == true)
                .Select(p => new ProductPrimaryProperties
                {
                    ProductPropertyId  = p.PropertyInstanceId.ToString().ToLower(),
                    PropertyInstanceId = p.InstanceId.ToString().ToLower()
                }).ToList(),
            _ => []
        };

    #endregion
}
