using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.RealConnect;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Saml;
using System.Dynamic;
using System.Text;
using ProductRole = UnifiedLogin.SharedObjects.Product.ProductRole;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UnifiedLogin.SharedObjects.BlackBook;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManageEnterpriseRolesPrimaryPropertiesAsync : IManageEnterpriseRolesPrimaryPropertiesAsync
{
    private readonly IManagePersonaAsync                            _managePersona;
    private readonly IProductRepositoryAsync                        _productRepo;
    private readonly IPropertyRepositoryAsync                       _propertyRepo;
    private readonly IUserLoginRepositoryAsync                      _userLoginRepo;
    private readonly IPersonaRepositoryAsync                        _personaRepo;
    private readonly IBatchProductBulkUpdateRepositoryAsync         _batchRepo;
    private readonly IUserRoleRightRepositoryAsync                  _userRoleRightRepo;
    private readonly IProductInternalSettingRepositoryAsync         _productInternalSettingRepo;
    private readonly IUnifiedSettingsRepositoryAsync                _unifiedSettingsRepo;
    private readonly IManageProductAdminSupportPortalAsync          _adminSupportPortal;
    private readonly IIntegrationTypeFactoryAsync                   _integrationTypeFactory;
    private readonly IManageProductPanelAsync                       _productPanel;
    private readonly IUserClaimsAccessor                            _userClaims;
    private readonly IMemoryCache                                   _cache;
    private readonly ILogger<ManageEnterpriseRolesPrimaryPropertiesAsync> _logger;

    public ManageEnterpriseRolesPrimaryPropertiesAsync(
        IManagePersonaAsync                                 managePersona,
        IProductRepositoryAsync                             productRepo,
        IPropertyRepositoryAsync                            propertyRepo,
        IUserLoginRepositoryAsync                           userLoginRepo,
        IPersonaRepositoryAsync                             personaRepo,
        IBatchProductBulkUpdateRepositoryAsync              batchRepo,
        IUserRoleRightRepositoryAsync                       userRoleRightRepo,
        IProductInternalSettingRepositoryAsync              productInternalSettingRepo,
        IUnifiedSettingsRepositoryAsync                     unifiedSettingsRepo,
        IManageProductAdminSupportPortalAsync               adminSupportPortal,
        IIntegrationTypeFactoryAsync                        integrationTypeFactory,
        IManageProductPanelAsync                            productPanel,
        IUserClaimsAccessor                                 userClaims,
        IMemoryCache                                        cache,
        ILogger<ManageEnterpriseRolesPrimaryPropertiesAsync> logger)
    {
        _managePersona              = managePersona              ?? throw new ArgumentNullException(nameof(managePersona));
        _productRepo                = productRepo                ?? throw new ArgumentNullException(nameof(productRepo));
        _propertyRepo               = propertyRepo               ?? throw new ArgumentNullException(nameof(propertyRepo));
        _userLoginRepo              = userLoginRepo              ?? throw new ArgumentNullException(nameof(userLoginRepo));
        _personaRepo                = personaRepo                ?? throw new ArgumentNullException(nameof(personaRepo));
        _batchRepo                  = batchRepo                  ?? throw new ArgumentNullException(nameof(batchRepo));
        _userRoleRightRepo          = userRoleRightRepo          ?? throw new ArgumentNullException(nameof(userRoleRightRepo));
        _productInternalSettingRepo = productInternalSettingRepo ?? throw new ArgumentNullException(nameof(productInternalSettingRepo));
        _unifiedSettingsRepo        = unifiedSettingsRepo        ?? throw new ArgumentNullException(nameof(unifiedSettingsRepo));
        _adminSupportPortal         = adminSupportPortal         ?? throw new ArgumentNullException(nameof(adminSupportPortal));
        _integrationTypeFactory     = integrationTypeFactory     ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
        _productPanel               = productPanel               ?? throw new ArgumentNullException(nameof(productPanel));
        _userClaims                 = userClaims                 ?? throw new ArgumentNullException(nameof(userClaims));
        _cache                      = cache                      ?? throw new ArgumentNullException(nameof(cache));
        _logger                     = logger                     ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
        long editorUserPersonaId,
        long subjectUserPersonaId,
        int? enterpriseRoleTemplateId = null,
        DateTime? createdDateTime = null,
        int batchProcessTypeId = 0,
        bool isUnassignAllProducts = false,
        CancellationToken cancellationToken = default)
    {
        string batchProcessorType = enterpriseRoleTemplateId is not null ? "Enterprise Role" : "Primary Properties";
        try
        {
            _logger.LogDebug(
                "{CorrelationId} {Type} process started SubjectPersonaId={SubjectPersonaId} TemplateId={TemplateId}",
                _userClaims.CorrelationId, batchProcessorType, subjectUserPersonaId, enterpriseRoleTemplateId);

            // ── Phase 1: Load personas and persona product settings in parallel ──────────────
            var editorPersonaTask         = _managePersona.GetPersonaAsync(editorUserPersonaId, cancellationToken: cancellationToken);
            var userPersonaTask           = _managePersona.GetPersonaAsync(subjectUserPersonaId, cancellationToken: cancellationToken);
            var personaProductSettingsTask = _personaRepo.GetPersonaProductSettingsAsync(subjectUserPersonaId, cancellationToken);

            await Task.WhenAll(editorPersonaTask, userPersonaTask, personaProductSettingsTask).ConfigureAwait(false);

            var editorPersona          = await editorPersonaTask;
            var userPersona            = await userPersonaTask;
            var personaProductSettings = await personaProductSettingsTask;

            long orgPartyId       = editorPersona.OrganizationPartyId;
            Guid orgRealPageGuid  = editorPersona.Organization.RealPageId;

            // ── Phase 2: Organization list and impersonator user id ───────────────────────
            var organizations = await _userLoginRepo
                .ListOrganizationByEnterpriseUserIdAsync(userPersona.RealPageId, null)
                .ConfigureAwait(false);
            var personaOrganization = organizations.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);

            long impersonatorUserId = 0;
            if (_userClaims.ImpersonatedBy != Guid.Empty)
            {
                var impersonatorLogin = await _userLoginRepo
                    .GetUserLoginOnlyAsync(_userClaims.ImpersonatedBy)
                    .ConfigureAwait(false);
                impersonatorUserId = (long)(impersonatorLogin?.UserId ?? 0);
            }

            bool isExternalUser = personaOrganization?.RelationshipType is not null
                && personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase)
                && personaOrganization.RoleNameFrom?.Equals("External User", StringComparison.OrdinalIgnoreCase) == true;

            // ── Phase 3: Determine which products to process ──────────────────────────────
            List<int> roleTemplateNewProducts     = [];
            List<int> roleTemplateUpdatedProducts = [];
            List<int> roleTemplateDeletedProducts = [];
            List<RoleTemplateProductRole> roleTemplateProductRole = [];

            if (batchProcessTypeId == (int)BatchProcessType.BulkAddUpdateEnterpriseRole)
            {
                if (isUnassignAllProducts)
                {
                    var personaProducts = (await _productRepo
                        .ListProductsByPersonaIdAsync(userPersona.PersonaId, (int)UserUiStatusType.AccountCreationSuccessful, cancellationToken)
                        .ConfigureAwait(false)).ToList();

                    personaProducts.RemoveAll(m => m.ProductId == (int)ProductEnum.UnifiedPlatform);
                    personaProducts.RemoveAll(m => m.ProductId == (int)ProductEnum.AssetOptimizer);

                    int adminSupportProductId = (int)ProductEnum.AdminSupportPortal;
                    if (personaProducts.Any(m => m.ProductId == adminSupportProductId))
                    {
                        var samlDetails = await _productRepo
                            .GetProductSamlDetailsAsync(subjectUserPersonaId, adminSupportProductId, cancellationToken)
                            .ConfigureAwait(false);
                        if (samlDetails?.Count == 0)
                            personaProducts.RemoveAll(a => a.ProductId == adminSupportProductId);
                    }

                    roleTemplateDeletedProducts.AddRange(personaProducts.Select(p => p.ProductId));
                    if (roleTemplateDeletedProducts.Count == 0)
                        return "";
                }
                else
                {
                    roleTemplateProductRole = await _productRepo
                        .GetRoleTemplateProductRoleMappingAsync(enterpriseRoleTemplateId!.Value, orgPartyId, cancellationToken)
                        .ConfigureAwait(false);
                    roleTemplateNewProducts = roleTemplateProductRole.Select(p => p.ProductId).Distinct().ToList();
                    roleTemplateUpdatedProducts.Add(roleTemplateNewProducts.FirstOrDefault(m => m == (int)ProductEnum.UnifiedPlatform));
                }
            }
            else if (enterpriseRoleTemplateId is not null)
            {
                // Parallel fetch of template product lists
                var newTask     = _productRepo.GetEnterpriseRoleNewProductsByRoleTemplateIdAsync(enterpriseRoleTemplateId.Value, createdDateTime!.Value, cancellationToken);
                var updatedTask = _productRepo.GetEnterpriseRoleUpdatedProductsByRoleTemplateIdAsync(enterpriseRoleTemplateId.Value, createdDateTime.Value, cancellationToken);
                var deletedTask = _productRepo.GetEnterpriseRoleDeletedProductsByRoleTemplateIdAsync(enterpriseRoleTemplateId.Value, createdDateTime.Value, cancellationToken);
                var mappingTask = _productRepo.GetRoleTemplateProductRoleMappingAsync(enterpriseRoleTemplateId.Value, orgPartyId, cancellationToken);

                await Task.WhenAll(newTask, updatedTask, deletedTask, mappingTask).ConfigureAwait(false);

                roleTemplateNewProducts     = await newTask;
                roleTemplateUpdatedProducts = await updatedTask;
                roleTemplateDeletedProducts = await deletedTask;
                roleTemplateProductRole     = await mappingTask;
                roleTemplateNewProducts.AddRange(roleTemplateUpdatedProducts);

                _logger.LogDebug("{CorrelationId} UpdatedProducts={Updated} DeletedProducts={Deleted}",
                    _userClaims.CorrelationId,
                    string.Join(",", roleTemplateUpdatedProducts),
                    string.Join(",", roleTemplateDeletedProducts));
            }
            else
            {
                var personaProducts = (await _productRepo
                    .ListProductsByPersonaIdAsync(userPersona.PersonaId, (int)UserUiStatusType.AccountCreationSuccessful, cancellationToken)
                    .ConfigureAwait(false)).ToList();

                var userEnterpriseRole = await _productRepo
                    .GetEnterpriseRoleForPersonaAsync(subjectUserPersonaId, cancellationToken)
                    .ConfigureAwait(false);

                if (userEnterpriseRole is not null)
                {
                    roleTemplateProductRole = await _productRepo
                        .GetRoleTemplateProductRoleMappingAsync(userEnterpriseRole.RoleTemplateId, userPersona.OrganizationPartyId, cancellationToken)
                        .ConfigureAwait(false);

                    foreach (var templateProduct in roleTemplateProductRole)
                    {
                        if (!personaProducts.Any(p => p.ProductId == templateProduct.ProductId)
                            && templateProduct.ProductId != (int)ProductEnum.UnifiedPlatform)
                        {
                            personaProducts.Add(new PersonaProductUserDetails
                            {
                                ProductId   = templateProduct.ProductId,
                                ProductName = templateProduct.ProductName
                            });
                        }
                    }
                }

                roleTemplateNewProducts = personaProducts.Select(p => p.ProductId).ToList();
                _logger.LogDebug("{CorrelationId} In Primary properties block", _userClaims.CorrelationId);
            }

            roleTemplateNewProducts = [.. roleTemplateNewProducts.Distinct()];

            _logger.LogDebug("{CorrelationId} {Type} NewProducts={Products}",
                _userClaims.CorrelationId, batchProcessorType, string.Join(",", roleTemplateNewProducts));

            var productsWithNoProperties = await GetProductsWithNoPropertiesAsync(cancellationToken).ConfigureAwait(false);

            // ── Phase 4: Pre-fetch company-level primary-property settings (shared across loop) ──
            var globalSettingsTask    = _productInternalSettingRepo.GetProductSettingByTypeAsync("UsePrimaryProperties", cancellationToken);
            var companySettingsTask   = _productRepo.GetProductSettingsAsync(orgRealPageGuid, cancellationToken);
            var unifiedSettingsTask   = _unifiedSettingsRepo.GetUnifiedSettingsAsync(orgPartyId, "Company", cancellationToken);

            await Task.WhenAll(globalSettingsTask, companySettingsTask, unifiedSettingsTask).ConfigureAwait(false);

            var productGlobalSettings  = await globalSettingsTask;
            var companyProductSettings = await companySettingsTask;
            var unifiedSettings        = await unifiedSettingsTask;

            int orgUsePrimaryProperties = -1;
            var ppSetting = unifiedSettings.FirstOrDefault(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase));
            if (ppSetting is not null)
                int.TryParse(ppSetting.Value, out orgUsePrimaryProperties);

            _logger.LogDebug("{CorrelationId} {Type} started SubjectPersonaId={SubjectPersonaId}",
                _userClaims.CorrelationId, batchProcessorType, subjectUserPersonaId);

            // ── Phase 5: Process each product ─────────────────────────────────────────────
            IList<ProductBatch> productListToCreate = [];
            IList<ProductRole>  productRoles        = null;
            bool isAllPropertiesForAdminPortal      = false;

            foreach (var product in roleTemplateNewProducts)
            {
                if (product == (int)ProductEnum.AssetOptimizer) continue;

                var propertiesResponse = new ListResponse();
                var rolesResponse      = new ListResponse();

                int productIdForPPSetting = ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product)
                    ? (int)ProductEnum.AssetOptimizer
                    : product;

                bool ppEnabledForCompanyAndProduct = ComputePrimaryPropertyEnabled(
                    productIdForPPSetting, productGlobalSettings, companyProductSettings, orgUsePrimaryProperties);

                bool productEnabledForPrimaryProperty = await IsProductEnabledForUsePrimaryPropertyAsync(product, cancellationToken).ConfigureAwait(false);

                bool personaProductUsePrimaryProperty = false;
                var productSetting = personaProductSettings.FirstOrDefault(item =>
                    item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)
                    && item.ProductId == product);

                if (productSetting is not null)
                {
                    personaProductUsePrimaryProperty = productSetting.Value?.Trim() == "1";
                }
                else
                {
                    var upfmProps = await _propertyRepo
                        .ListUPFMPropertyInstanceByPersonaAsync(subjectUserPersonaId, ProductEnum.UnifiedPlatform, cancellationToken)
                        .ConfigureAwait(false);
                    personaProductUsePrimaryProperty = upfmProps.Count > 0;
                }

                bool usePrimaryProperties = productEnabledForPrimaryProperty
                    && personaProductUsePrimaryProperty
                    && ppEnabledForCompanyAndProduct;
                if (product == (int)ProductEnum.UnifiedPlatform) usePrimaryProperties = true;

                var integrationType = _integrationTypeFactory.GetIntegrationTypeForProductId(product);

                bool hasTemplateRole = enterpriseRoleTemplateId is not null
                    || roleTemplateProductRole.Any(m => m.ProductId == product);

                if (hasTemplateRole)
                {
                    rolesResponse = await _productPanel
                        .GetProductRolesAsync(editorPersona.PersonaId, 0, userPersona.OrganizationPartyId, product, null!, null, cancellationToken)
                        .ConfigureAwait(false);

                    productRoles = GetProductRoleList(roleTemplateProductRole, product);

                    if (product == (int)ProductEnum.AdminSupportPortalStandard)
                        EnrichRolesWithTypes(productRoles, rolesResponse);

                    if (productRoles?.Any() == true && rolesResponse.Records?.Any() == true)
                    {
                        if (rolesResponse.Records[0] is ProductRole)
                        {
                            var allRoles = rolesResponse.Records.Cast<ProductRole>().ToList();
                            productRoles = [.. productRoles.Where(m => allRoles.Any(l => l.ID?.ToString() == m.ID))];
                        }
                    }

                    rolesResponse = new ListResponse
                    {
                        Records      = productRoles?.Cast<object>().ToList() ?? [],
                        TotalRows    = productRoles?.Count ?? 0,
                        RowsPerPage  = productRoles?.Count ?? 0,
                        TotalPages   = 1,
                        ErrorReason  = ""
                    };
                }
                else
                {
                    rolesResponse = await _productPanel
                        .GetProductRolesAsync(editorPersona.PersonaId, userPersona.PersonaId, userPersona.OrganizationPartyId, product, null!, null, cancellationToken)
                        .ConfigureAwait(false);

                    productRoles = ExtractAssignedRoles(rolesResponse, product);
                }

                // ── Per-product batch record construction ──────────────────────────────────
                if (product == (int)ProductEnum.UnifiedPlatform
                    && roleTemplateUpdatedProducts.Contains((int)ProductEnum.UnifiedPlatform))
                {
                    var rolesToDelete = await GetAssignedRoleForPersonaAsync(
                        product, subjectUserPersonaId, userPersona.OrganizationPartyId, cancellationToken).ConfigureAwait(false);

                    foreach (var r in rolesToDelete)
                        await _batchRepo.UpdateUnifiedPlatFormRoleAsync(
                            (int)r.RoleID, editorPersona.UserId, subjectUserPersonaId, true, cancellationToken).ConfigureAwait(false);

                    foreach (var r in productRoles ?? [])
                        await _batchRepo.UpdateUnifiedPlatFormRoleAsync(
                            Convert.ToInt32(r.ID), editorPersona.UserId, subjectUserPersonaId, false, cancellationToken).ConfigureAwait(false);
                }
                else if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
                {
                    if (productsWithNoProperties.Contains(product))
                    {
                        AddAOProductWithoutProperties(productListToCreate, productRoles, usePrimaryProperties, product, isAssigned: true);
                    }
                    else
                    {
                        propertiesResponse = await GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
                            editorUserPersonaId, subjectUserPersonaId, product, usePrimaryProperties, cancellationToken).ConfigureAwait(false);
                        propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);
                        BatchHelper.CreateAoBatchRecords(
                            _userClaims.UserClaim, editorUserPersonaId, subjectUserPersonaId,
                            isExternalUser, usePrimaryProperties, propertiesResponse,
                            product, productRoles, productListToCreate);
                    }
                }
                else
                {
                    var productBatchRecord = new ProductBatch();

                    propertiesResponse = await GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
                        editorUserPersonaId, subjectUserPersonaId, product, usePrimaryProperties, cancellationToken).ConfigureAwait(false);

                    if (propertiesResponse?.Records?.Count > 0)
                    {
                        if (product == (int)ProductEnum.AdminSupportPortal && !usePrimaryProperties)
                        {
                            var userProperties = await _adminSupportPortal
                                .GetPropertiesAsync(editorUserPersonaId, subjectUserPersonaId, null!, cancellationToken)
                                .ConfigureAwait(false);
                            var productAttributes = await _productRepo
                                .ListPersonaProductsSamlDetailsAsync(subjectUserPersonaId, cancellationToken)
                                .ConfigureAwait(false);

                            if (productAttributes is not null)
                            {
                                var adminProduct = productAttributes.FirstOrDefault(p => p.ProductId == (int)ProductEnum.AdminSupportPortal);
                                if (adminProduct is not null && userProperties?.Records is not null)
                                {
                                    var propertyList = userProperties.Records
                                        .OfType<ProductProperty>()
                                        .Where(c => c.IsAssigned == true)
                                        .ToList();

                                    if (adminProduct.ProductStatus?.ToLower() == "success" && propertyList.Count > 0)
                                    {
                                        userProperties.Records      = propertyList.Cast<object>().ToList();
                                        isAllPropertiesForAdminPortal = CheckForAllProperties(userProperties.Additional);
                                        propertiesResponse          = userProperties;
                                        productBatchRecord          = await GetProductBatchRecordAsync(
                                            editorUserPersonaId, subjectUserPersonaId,
                                            productRoles, propertiesResponse, rolesResponse,
                                            product, usePrimaryProperties, cancellationToken).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                        else
                        {
                            propertiesResponse = BatchHelper.GetUserAssignedPropertiesData(propertiesResponse);
                            productBatchRecord = await GetProductBatchRecordAsync(
                                editorUserPersonaId, subjectUserPersonaId,
                                productRoles, propertiesResponse, rolesResponse,
                                product, usePrimaryProperties, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else if (product == (int)ProductEnum.RealConnect && !usePrimaryProperties)
                    {
                        // NOTE: ManageProductRealConnect.GetUser / GetClientLicenseDetailsCaching are not yet on
                        // IManageProductRealConnectAsync. LearnerLicenseId will be populated once those methods
                        // are added to the async interface. Until then the RCProductBatch is created with an
                        // empty LearnerLicenseId, preserving the batch-record structure.
                        var productAttributes = await _productRepo
                            .ListPersonaProductsSamlDetailsAsync(subjectUserPersonaId, cancellationToken)
                            .ConfigureAwait(false);

                        List<string> positionsToAdd = [];
                        if (productAttributes is not null)
                        {
                            // TODO: When GetUserAsync / GetClientLicensesAsync are available on
                            // IManageProductRealConnectAsync, populate positionsToAdd from learner
                            // allocated licenses merged with enterprise template position attributes.
                            _ = productAttributes.FirstOrDefault(p => p.ProductId == (int)ProductEnum.RealConnect);
                        }

                        productBatchRecord = BatchHelper.CreateProductBatchRecord(
                            propertiesResponse, rolesResponse, product, usePrimaryProperties, integrationType);
                        productBatchRecord.InputJson.RCLicenseDetails = new RCProductBatch
                        {
                            LearnerLicenseId = positionsToAdd,
                            ManagerLicenseId = []
                        };
                    }
                    else
                    {
                        productBatchRecord = BatchHelper.CreateProductBatchRecord(
                            propertiesResponse, rolesResponse, product, usePrimaryProperties, integrationType);

                        if (product == (int)ProductEnum.KnockCRM)
                        {
                            var roleProp = productBatchRecord.InputJson;
                            roleProp.PropertyGroupList ??= [];
                            var knockIntegration = _integrationTypeFactory.GetIntegration(product);
                            var propertyGroupResponse = await knockIntegration
                                .GetPropertyGroupsAsync(editorUserPersonaId, subjectUserPersonaId, null!,null!, cancellationToken)
                                .ConfigureAwait(false);
                            if (propertyGroupResponse.Records is not null)
                            {
                                foreach (var item in propertyGroupResponse.Records.OfType<ProductPropertyGroups>())
                                {
                                    if (item.IsAssigned)
                                        roleProp.PropertyGroupList.Add(item.GetGroupId);
                                }
                            }
                        }
                    }

                    // UPFM: compute properties to remove
                    if (integrationType == ProductIntegrationTypeEnum.UPFM)
                    {
                        var currentProductProps = await _propertyRepo
                            .ListUPFMPropertyInstanceIdByPersonaAsync(subjectUserPersonaId, product, cancellationToken)
                            .ConfigureAwait(false);
                        var currentUnifiedUIProps = await _propertyRepo
                            .ListUPFMPropertyInstanceIdByPersonaAsync(subjectUserPersonaId, (int)ProductEnum.UnifiedUI, cancellationToken)
                            .ConfigureAwait(false);

                        var propertiesToRemove = currentProductProps
                            .Except(currentUnifiedUIProps)
                            .Except(propertiesResponse?.Records?.Count > 0
                                ? productBatchRecord.InputJson.PropertyList.Select(m => Convert.ToInt32(m))
                                : Enumerable.Empty<int>())
                            .ToList();

                        if (propertiesToRemove.Count > 0)
                            productBatchRecord.InputJson.RemovedPropertyList = propertiesToRemove.Select(i => i.ToString()).ToList();
                    }

                    // OneSite Accounting (productId == 8) additional boolean attributes
                    if (product == 8)
                    {
                        var additionalRoles = roleTemplateProductRole?
                            .Where(p => p.ProductId == product)
                            .Select(p => new { p.RoleTemplateProductRoleMappingId, p.AttributeName, p.AttributeValue })
                            .Distinct();

                        var siteUser = additionalRoles?.FirstOrDefault(p => p.AttributeName == "hasAccessToSiteSpendManagementOnly");
                        if (siteUser is not null)
                            productBatchRecord.InputJson.HasAccessToSiteSpendManagementOnly = bool.Parse(siteUser.AttributeValue);

                        var accountingAdmin = additionalRoles?.FirstOrDefault(p => p.AttributeName == "isAccountingAdmin");
                        if (accountingAdmin is not null)
                            productBatchRecord.InputJson.IsAccountingAdmin = bool.Parse(accountingAdmin.AttributeValue);

                        var allCurrentFuture = additionalRoles?.FirstOrDefault(p => p.AttributeName == "hasAccessToAllCurrentFutureProperties");
                        if (allCurrentFuture is not null)
                            productBatchRecord.InputJson.HasAccessToAllCurrentFutureProperties = bool.Parse(allCurrentFuture.AttributeValue);
                    }

                    if (propertiesResponse?.Records?.Count == 0 && !isAllPropertiesForAdminPortal
                        && product != (int)ProductEnum.DataHub)
                    {
                        productBatchRecord.InputJson.IsAssigned = false;
                    }

                    productListToCreate.Add(productBatchRecord);
                }
            }

            // ── Phase 6: OneSite + Lead2Lease mix handling ────────────────────────────────
            Dictionary<string, RolePropertyList> oneSiteAndOtherProducts = [];
            bool isOnesiteMix = false;

            if (productListToCreate.Count > 0)
            {
                _logger.LogDebug(
                    "{CorrelationId} {Type} product batch update started SubjectPersonaId={SubjectPersonaId} Count={Count}",
                    _userClaims.CorrelationId, batchProcessorType, subjectUserPersonaId, productListToCreate.Count);

                if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.OneSite)
                    && productListToCreate.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease))
                {
                    isOnesiteMix = true;
                    var pbOneSite = productListToCreate.First(a => a.ProductId == (int)ProductEnum.OneSite);
                    oneSiteAndOtherProducts.Add(ProductEnum.OneSite.ToString(), pbOneSite.InputJson);

                    var pbLead2Lease = productListToCreate.FirstOrDefault(a => a.ProductId == (int)ProductEnum.Lead2Lease);
                    if (pbLead2Lease is not null)
                    {
                        oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbLead2Lease.InputJson);
                        productListToCreate.Remove(pbLead2Lease);
                    }
                }
            }

            // ── Phase 7: Append deleted-product batch records ─────────────────────────────
            if (roleTemplateDeletedProducts?.Count > 0)
            {
                foreach (var product in roleTemplateDeletedProducts)
                {
                    if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product))
                    {
                        if (productsWithNoProperties.Contains(product))
                            AddAOProductWithoutProperties(productListToCreate, productRoles, usePrimaryProperties: false, product, isAssigned: false);
                        else
                            BatchHelper.CreateAoBatchRecords(
                                _userClaims.UserClaim, editorUserPersonaId, subjectUserPersonaId,
                                isExternalUser, usePrimaryProperties: true, propertiesResponse: null,
                                product, productRoles: null, productListToCreate,  true);
                    }
                    else
                    {
                        productListToCreate.Add(new ProductBatch
                        {
                            ProductId    = product,
                            StatusTypeId = 5,
                            RetryCount   = 0,
                            InputJson    = new RolePropertyList { PropertyList = [], RoleList = [], IsAssigned = false }
                        });
                    }
                }
            }

            // ── Phase 8: Bundle AO products and save batch ────────────────────────────────
            string inputAOJSON = ManageEnterpriseRolesPrimaryProperties.BundleAoProducts(productListToCreate);

            if (productListToCreate.Count > 0)
            {
                bool isBatchCompleted = await _batchRepo.SaveProductBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId, editorPersona.RealPageId,
                    productListToCreate,
                    JsonConvert.SerializeObject(oneSiteAndOtherProducts),
                    isOnesiteMix,
                    (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser,
                    impersonatorUserId,
                    inputAOJSON,
                    cancellationToken).ConfigureAwait(false);

                if (!isBatchCompleted)
                {
                    _logger.LogError("{CorrelationId} {Type} batch failed SubjectPersonaId={SubjectPersonaId}",
                        _userClaims.CorrelationId, batchProcessorType, subjectUserPersonaId);
                    return "Error";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{CorrelationId} Exception during {Type} batch insert SubjectPersonaId={SubjectPersonaId}",
                _userClaims.CorrelationId, batchProcessorType, subjectUserPersonaId);
            return "Error";
        }

        return "";
    }

    // ── Private async helpers ──────────────────────────────────────────────────────────

    private async Task<ListResponse> GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
        long editorPersonaId, long userPersonaId, int productId, bool usePrimaryProperties,
        CancellationToken ct)
    {
        var userProperties = await _propertyRepo
            .ListUPFMPropertyInstanceByPersonaAsync(userPersonaId, ProductEnum.UnifiedPlatform, ct)
            .ConfigureAwait(false);

        var result = new ListResponse();
        if (productId != (int)ProductEnum.KnockCRM)
        {
            result = await _productPanel
                .GetProductPropertiesAsync(editorPersonaId, userPersonaId, productId, null!, ct)
                .ConfigureAwait(false);
        }

        if (!result.IsError && usePrimaryProperties)
        {
            var upfmProperty = new UPFMProperty
            {
                id = userProperties?.Select(p => p.InstanceId.ToString()).ToList()
            };

            if (productId == (int)ProductEnum.KnockCRM)
            {
                result = new ListResponse
                {
                    Records   = userProperties.Cast<object>().ToList(),
                    TotalRows = userProperties.Count
                };
            }

            result = await _productPanel
                .CompareProductAndPrimaryPropertiesAsync(upfmProperty, productId, result, ct)
                .ConfigureAwait(false);
        }

        return result;
    }

    private async Task<bool> IsProductEnabledForUsePrimaryPropertyAsync(int productId, CancellationToken ct)
    {
        string cacheKey = $"productInternalSetting_usePrimaryProp_{productId}";
        if (_cache.TryGetValue(cacheKey, out bool cached))
            return cached;

        var settings = await _productInternalSettingRepo
            .GetProductInternalSettingsAsync(productId, ct)
            .ConfigureAwait(false);

        bool result = settings
            .FirstOrDefault(s => s.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase))
            ?.Value?.Trim() == "1";

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
        return result;
    }

    private async Task<List<int>> GetProductsWithNoPropertiesAsync(CancellationToken ct)
    {
        string cacheKey = $"productInternalSetting_{(int)ProductEnum.UnifiedPlatform}";
        if (!_cache.TryGetValue(cacheKey, out List<ProductInternalSetting> upSettingList))
        {
            upSettingList = (await _productInternalSettingRepo
                .GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, ct)
                .ConfigureAwait(false)).ToList();

            _cache.Set(cacheKey, upSettingList, TimeSpan.FromMinutes(2));
        }

        var productsStr = upSettingList?
            .FirstOrDefault(ps => ps.Name.Equals(
                "UserAccessDetails_ProductsWithNoProperties", StringComparison.InvariantCultureIgnoreCase))
            ?.Value;

        var result = new List<int>();
        if (string.IsNullOrEmpty(productsStr)) return result;

        foreach (var idStr in productsStr.Split(','))
        {
            if (int.TryParse(idStr.Trim(), out int id) && !result.Contains(id))
                result.Add(id);
        }

        return result;
    }

    private async Task<List<UL.Role>> GetAssignedRoleForPersonaAsync(
        int productId, long userPersonaId, long organizationPartyId, CancellationToken ct)
    {
        // Force-invalidate any cached role list so we operate on fresh data
        _cache.Remove($"sp_ListRolesForProductsByPersonaId_{productId}_{userPersonaId}_{organizationPartyId}");
        return await _userRoleRightRepo
            .ListRoleByPersonaAsync(productId, userPersonaId, organizationPartyId)
            .ConfigureAwait(false);
    }

    private Task<ProductBatch> GetProductBatchRecordAsync(
        long editorUserPersonaId, long subjectUserPersonaId,
        IList<ProductRole> productRoles, ListResponse propertiesResponse,
        ListResponse rolesResponse, int product, bool usePrimaryProperties,
        CancellationToken ct)
    {
        // Async-over-sync adapter. ManageProductBatch still uses sync repositories internally.
        // This offloads the sync I/O to the thread pool to avoid blocking the request thread.
        // Replace with a true-async implementation once IManageProductBatchAsync is available.
        return Task.Run(() =>
        {
            var batch = new ManageProductBatch(_userClaims.UserClaim);
            return batch.GetProductBatchRecord(
                editorUserPersonaId, subjectUserPersonaId,
                productRoles, propertiesResponse, rolesResponse,
                product, usePrimaryProperties);
        }, ct);
    }

    // ── Private static helpers ─────────────────────────────────────────────────────────

    private static bool ComputePrimaryPropertyEnabled(
        int productId,
        IList<ProductInternalSettingByType> globalSettings,
        IList<ProductSettingList> companySettings,
        int orgUsePrimaryProperties)
    {
        if (orgUsePrimaryProperties < 0) return false;

        string globalStr = globalSettings?
            .FirstOrDefault(p => p.Name.ToLower() == "useprimaryproperties" && p.ProductId == productId)
            ?.Value?.Trim();

        if (globalStr is null) return false;
        if (!int.TryParse(globalStr, out int globalVal) || globalVal < 0) return false;

        int.TryParse(companySettings?
            .FirstOrDefault(p => p.Name.ToLower() == "useprimaryproperties" && p.ProductId == productId)
            ?.Value?.Trim(), out int companySetting);

        return globalVal == 1 && orgUsePrimaryProperties == 1 && companySetting == 1;
    }

    private static IList<ProductRole> GetProductRoleList(
        List<RoleTemplateProductRole> roleTemplateProductRole, int productId)
    {
        IList<ProductRole> productRoles = [];

        var roleTemplateRoles = roleTemplateProductRole?
            .Where(p => p.ProductId == productId)
            .Select(p => new { p.RoleTemplateProductRoleMappingId, p.ProductRoleId, p.ProductRoleName })
            .Distinct();

        if (roleTemplateRoles is null) return productRoles;

        foreach (var role in roleTemplateRoles)
        {
            if (role.RoleTemplateProductRoleMappingId != 0)
            {
                productRoles.Add(new ProductRole
                {
                    ID         = role.ProductRoleId.ToString(),
                    Name       = role.ProductRoleName,
                    IsAssigned = true
                });
            }
        }

        return productRoles;
    }

    private static void EnrichRolesWithTypes(IList<ProductRole> productRoles, ListResponse rolesResponse)
    {
        if (productRoles is null || !productRoles.Any() || rolesResponse?.Records is null || !rolesResponse.Records.Any())
            return;

        if (rolesResponse.Records[0] is ProductRole)
        {
            var all = rolesResponse.Records.Cast<ProductRole>().ToList();
            foreach (var role in productRoles)
            {
                var match = all.FirstOrDefault(r => r.ID?.ToString() == role.ID);
                if (match is not null) role.Roletype = match.Roletype;
            }
        }
        else if (rolesResponse.Records[0] is Product.Integrations.Model.ProductRole)
        {
            var all = rolesResponse.Records.Cast<Product.Integrations.Model.ProductRole>().ToList();
            foreach (var role in productRoles)
            {
                var match = all.FirstOrDefault(r => r.GetRoleId == role.ID);
                if (match is not null) role.Roletype = match.RoleType;
            }
        }
    }

    private static IList<ProductRole> ExtractAssignedRoles(ListResponse rolesResponse, int product)
    {
        IList<ProductRole> result = [];
        if (rolesResponse?.Records?.Count == 0) return result;

        if (product == (int)ProductEnum.ResidentPortal)
        {
            var levels = rolesResponse.Records?.OfType<Level>().ToList();
            if (levels?.Count > 0)
            {
                return levels
                    .Where(p => p.IsAssigned)
                    .Select(p => new ProductRole { ID = p.Id, Name = p.Name, IsAssigned = p.IsAssigned })
                    .ToList();
            }
        }

        if (rolesResponse?.Records?[0] is ProductRole)
        {
            return rolesResponse.Records.Cast<ProductRole>().Where(p => p.IsAssigned).ToList();
        }

        if (rolesResponse?.Records?[0] is Product.Integrations.Model.ProductRole)
        {
            return rolesResponse.Records
                .Cast<Product.Integrations.Model.ProductRole>()
                .Where(p => p.IsAssigned)
                .Select(p => new ProductRole { ID = p.GetRoleId, Name = p.GetName, IsAssigned = p.IsAssigned })
                .ToList();
        }

        return result;
    }

    private static void AddAOProductWithoutProperties(
        IList<ProductBatch> productListToCreate,
        IList<ProductRole> productRoles,
        bool usePrimaryProperties,
        int product,
        bool isAssigned)
    {
        productListToCreate.Add(new ProductBatch
        {
            ProductId    = product,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList      = [],
                RoleList          = productRoles?.Count > 0 ? productRoles.Select(i => i.Name).ToList() : [],
                CompanyId         = 0,
                PropertyGroupList = [],
                UsePrimaryProperties = usePrimaryProperties,
                IsAssigned        = isAssigned
            }
        });
    }

    private static bool CheckForAllProperties(object additionalInfo)
    {
        if (additionalInfo is Dictionary<string, bool> dict
            && dict.TryGetValue("allProperties", out bool value))
            return value;
        return false;
    }
}
