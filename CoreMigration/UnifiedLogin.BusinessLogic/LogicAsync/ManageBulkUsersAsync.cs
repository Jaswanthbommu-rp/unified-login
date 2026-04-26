using System.Data;
using System.Text;
using System.Dynamic;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.LogicAsync.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first bulk user product un-assignment service.
/// <para>
/// Replaces <c>ManageBulkUsers</c> which extended <c>BaseRepository</c>
/// and constructed all dependencies inline with <c>new Xxx(_userClaim)</c>.
/// </para>
/// <para>
/// Key improvements:
/// <list type="bullet">
///   <item>All repository and service calls are genuinely async.</item>
///   <item><c>IDbConnectionFactory</c> replaces <c>BaseRepository.GetRepository()</c>
///         for the multi-map <c>GetUserBatchRecordAsync</c> query.</item>
///   <item><c>RPObjectCache</c> replaced by a direct async settings read
///         for <c>GetProductsWithNoPropertiesAsync</c>.</item>
///   <item><c>BatchHelper.CreateAoBatchRecords</c> replaced by
///         <see cref="IAoBatchServiceAsync"/> (injectable, no <c>DefaultUserClaim</c>).</item>
///   <item>Impersonation context flows through the <c>impersonatorUserId</c> parameter
///         (stored on <see cref="BulkUserBatch.ImpersonatorUserId"/> — DB column
///         <c>ImpersonatorUserId bigint null</c> — at batch-creation time) rather than
///         through <c>IUserClaimsAccessor</c>, which is unavailable in the
///         batch-processor service context.</item>
/// </list>
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class ManageBulkUsersAsync : IManageBulkUsersAsync
{
    #region Fields

    private readonly IManagePersonaAsync                    _managePersona;
    private readonly IProductRepositoryAsync                _productRepository;
    private readonly IUserLoginRepositoryAsync              _userLoginRepository;
    private readonly IManagePartyRelationshipAsync          _managePartyRelationship;
    private readonly IBatchProductBulkUpdateRepositoryAsync _batchRepository;
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepository;
    private readonly IAoBatchServiceAsync                   _aoBatchService;
    private readonly IDbConnectionFactory                   _dbFactory;
    private readonly ILogger<ManageBulkUsersAsync>          _logger;

    #endregion

    #region Constructor

    public ManageBulkUsersAsync(
        IManagePersonaAsync                    managePersona,
        IProductRepositoryAsync                productRepository,
        IUserLoginRepositoryAsync              userLoginRepository,
        IManagePartyRelationshipAsync          managePartyRelationship,
        IBatchProductBulkUpdateRepositoryAsync batchRepository,
        IProductInternalSettingRepositoryAsync productInternalSettingRepository,
        IAoBatchServiceAsync                   aoBatchService,
        IDbConnectionFactory                   dbFactory,
        ILogger<ManageBulkUsersAsync>          logger)
    {
        _managePersona                    = managePersona                    ?? throw new ArgumentNullException(nameof(managePersona));
        _productRepository                = productRepository                ?? throw new ArgumentNullException(nameof(productRepository));
        _userLoginRepository              = userLoginRepository              ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _managePartyRelationship          = managePartyRelationship          ?? throw new ArgumentNullException(nameof(managePartyRelationship));
        _batchRepository                  = batchRepository                  ?? throw new ArgumentNullException(nameof(batchRepository));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _aoBatchService                   = aoBatchService                   ?? throw new ArgumentNullException(nameof(aoBatchService));
        _dbFactory                        = dbFactory                        ?? throw new ArgumentNullException(nameof(dbFactory));
        _logger                           = logger                           ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<string> ProcessProductUnAssignBatchDataAsync(
        long editorUserPersonaId,
        long subjectUserPersonaId,
        int bulkUserBatchProcessId,
        long? impersonatorUserId,
        CancellationToken cancellationToken = default)
    {
        const string batchProcessorType = "Un Assign products for Bulk Users";
        try
        {
            // ── 1. Resolve editor and subject personas ────────────────────────
            var editorPersona = await _managePersona.GetPersonaAsync(
                editorUserPersonaId, withRights: false, cancellationToken).ConfigureAwait(false);

            var userPersona = await _managePersona.GetPersonaAsync(
                subjectUserPersonaId, withRights: false, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug(
                "ProcessProductUnAssignBatchDataAsync: {BatchProcessorType} started for subject {SubjectPersonaId}",
                batchProcessorType, subjectUserPersonaId);

            // ── 2. Resolve subject user's organisation list ───────────────────
            var organizationList = await _userLoginRepository.ListOrganizationByEnterpriseUserIdAsync(
                userPersona.RealPageId, relationshipType: null!).ConfigureAwait(false);

            var personaOrganization = organizationList
                .FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);

            // ── 3. Skip SuperUsers ────────────────────────────────────────────
            var partyRelationship = await _managePartyRelationship.GetPartyRelationshipAsync(
                userPersona.RealPageId,
                userPersona.Organization.RealPageId,
                roleTypeNameFrom: null,
                roleTypeNameTo: null,
                relationshipTypeName: "User Type",
                cancellationToken).ConfigureAwait(false);

            if (partyRelationship?.RoleTypeFrom?.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase) == true)
                return string.Empty;

            // ── 4. Resolve active products for the subject persona ────────────
            var personaProducts = (await _productRepository.ListProductsByPersonaIdAsync(
                userPersona.PersonaId, (int)UserUiStatusType.AccountCreationSuccessful, cancellationToken)
                .ConfigureAwait(false)).ToList();

            // UnifiedPlatform is never un-assigned via this path
            personaProducts.RemoveAll(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

            // ── 5. Load requested un-assignment list ──────────────────────────
            var userRecords    = await GetUserBatchRecordAsync(bulkUserBatchProcessId, cancellationToken).ConfigureAwait(false);
            var bulkProductIds = userRecords.SelectMany(r => r.BulkUserProducts).Select(p => p.ProductId);

            var roleTemplateDeletedProducts = bulkProductIds
                .Intersect(personaProducts.Select(p => p.ProductId))
                .ToList();

            // ── 6. AdminSupportPortal guard ───────────────────────────────────
            int adminSupportProductId = (int)ProductEnum.AdminSupportPortal;
            if (personaProducts.Any(p => p.ProductId == adminSupportProductId))
            {
                var productAttributes = await _productRepository.GetProductSamlDetailsAsync(
                    subjectUserPersonaId, adminSupportProductId, cancellationToken).ConfigureAwait(false);

                if (productAttributes is { Count: 0 })
                    personaProducts.RemoveAll(p => p.ProductId == adminSupportProductId);
            }

            if (roleTemplateDeletedProducts.Count == 0)
                return string.Empty;

            // ── 7. Resolve products that carry no property list ───────────────
            var productsWithNoProperties = await GetProductsWithNoPropertiesAsync(cancellationToken)
                .ConfigureAwait(false);

            bool isExternalUser = personaOrganization?.RelationshipType is not null
                && personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase)
                && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

            // ── 8. Impersonator userId ────────────────────────────────────────
            // DB column: ImpersonatorUserId bigint null.
            // App-wide convention (see UserServiceAsync, EntUserRepositoryAsync): pass 0 for "no impersonation".
            // The value is stored on BulkUserBatch at creation time (HTTP context),
            // so no secondary lookup is needed here.

            // ── 9. Build product batch list ───────────────────────────────────
            List<ProductBatch> productListToCreate = [];

            foreach (int productId in roleTemplateDeletedProducts.Where(id => id > 0))
            {
                if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)productId))
                {
                    if (productsWithNoProperties.Contains(productId))
                    {
                        AddAoProductWithoutProperties(productListToCreate, productId);
                    }
                    else
                    {
                        await _aoBatchService.CreateAoBatchRecordsAsync(
                            editorUserPersonaId, subjectUserPersonaId,
                            isExternalUser, usePrimaryProperties: true,
                            propertiesResponse: new ListResponse(),
                            productId,
                            productRoles: null,
                            productBatchList: productListToCreate,
                            isDeleted: true,
                            cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    productListToCreate.Add(new ProductBatch
                    {
                        ProductId    = productId,
                        StatusTypeId = 5,
                        RetryCount   = 0,
                        InputJson    = new RolePropertyList
                        {
                            PropertyList = [],
                            RoleList     = [],
                            IsAssigned   = false
                        }
                    });
                }
            }

            // ── 10. Bundle AO products + persist ─────────────────────────────
            string inputAoJson = BundleAoProducts(productListToCreate);

            if (productListToCreate.Count > 0)
            {
                bool isBatchCompleted = await _batchRepository.SaveProductBatchAsync(
                    editorUserPersonaId, subjectUserPersonaId,
                    editorPersona.RealPageId, productListToCreate,
                    string.Empty, isOnesiteMix: false,
                    (int)BatchProcessType.CreateUpdateProductUser,
                    impersonatorUserId ?? 0L, inputAoJson,
                    cancellationToken).ConfigureAwait(false);

                if (!isBatchCompleted)
                {
                    _logger.LogError(
                        "ProcessProductUnAssignBatchDataAsync: SaveProductBatch failed for subject {SubjectPersonaId}",
                        subjectUserPersonaId);
                    return "Error";
                }
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ProcessProductUnAssignBatchDataAsync: exception during {BatchProcessorType} for subject {SubjectPersonaId}",
                batchProcessorType, subjectUserPersonaId);
            return "Error";
        }
    }

    #endregion

    #region Private — Data helpers

    /// <summary>
    /// Fetches bulk user batch records using a Dapper multi-map query.
    /// Replaces <c>ManageBulkUsers.GetUserBatchRecord</c> which used
    /// <c>BaseRepository.GetRepository().GetManyWithSpliOn</c>.
    /// </summary>
    private async Task<IList<BulkUserBatch>> GetUserBatchRecordAsync(
        int bulkUserBatchProcessId,
        CancellationToken cancellationToken)
    {
        var bulkUsers = new Dictionary<int, BulkUserBatch>();

        await using var connection = _dbFactory.CreateConnection();

        // Dapper multi-map: CancellationToken passed via CommandDefinition overload
        await connection.QueryAsync<BulkUserBatch, BulkUserProduct, BulkUserBatch>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetBulkUserBatchRecords,
                new { BulkUserBatchProcessId = bulkUserBatchProcessId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken),
            (bulkUser, product) =>
            {
                bulkUsers.TryAdd(product.BulkUserBatchProcessId, bulkUser);
                bulkUsers[product.BulkUserBatchProcessId].BulkUserProducts.Add(product);
                return bulkUsers[product.BulkUserBatchProcessId];
            },
            splitOn: "BulkUserBatchProcessId,ProductId").ConfigureAwait(false);

        return [.. bulkUsers.Values];
    }

    /// <summary>
    /// Returns the list of AO product IDs that carry no property list.
    /// Replaces the <c>RPObjectCache</c>-based sync <c>GetProductsWithNoProperties</c>.
    /// </summary>
    private async Task<List<int>> GetProductsWithNoPropertiesAsync(CancellationToken cancellationToken)
    {
        var settings = await _productInternalSettingRepository
            .GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken)
            .ConfigureAwait(false);

        var value = settings?.FirstOrDefault(s =>
            s.Name.Equals("UserAccessDetails_ProductsWithNoProperties",
                           StringComparison.InvariantCultureIgnoreCase))?.Value;

        if (string.IsNullOrEmpty(value))
            return [];

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .Distinct()
                    .ToList();
    }

    #endregion

    #region Private — Batch-building helpers

    private static void AddAoProductWithoutProperties(
        IList<ProductBatch> productList,
        int productId)
    {
        productList.Add(new ProductBatch
        {
            ProductId    = productId,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = new RolePropertyList
            {
                PropertyList         = [],
                RoleList             = [],
                CompanyId            = 0,
                PropertyGroupList    = [],
                UsePrimaryProperties = false,
                IsAssigned           = false
            }
        });
    }

    /// <summary>
    /// Groups all AO sub-product batches under a single <c>AssetOptimizer</c> envelope batch
    /// and serialises the composite into the <c>inputAOJSON</c> string.
    /// Replaces <c>ManageBulkUsers.BundleAoProducts</c> (static) — moved here as a private static
    /// to keep the static / IO boundary clear.
    /// </summary>
    private static string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0)
    {
        var aoProductList = productList
            .Where(p => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)p.ProductId))
            .ToList();

        if (aoProductList.Count == 0)
            return string.Empty;

        // Bundle AO sub-products under a single AssetOptimizer envelope
        var aoEnvelope = new ProductBatch
        {
            ProductId             = (int)ProductEnum.AssetOptimizer,
            StatusTypeId          = 5,
            RetryCount            = 0,
            BatchProcessorGroupId = batchProcessorGroupId,
            InputJson             = null
        };

        dynamic expandoList = new ExpandoObject();
        expandoList.IsAssigned = true;
        expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();

        foreach (var aoProd in aoProductList)
        {
            dynamic expandoAo = new ExpandoObject();
            expandoAo.SelectedRoleValues      = aoProd.InputJson?.RoleList;
            expandoAo.SelectedPortfolioValues = aoProd.InputJson?.PropertyList;
            expandoAo.CompanyId               = aoProd.InputJson?.CompanyId;
            expandoAo.Product                 = ProductEnumHelper.GetAoProductId((ProductEnum)aoProd.ProductId);
            expandoAo.DivisionName            = ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProd.ProductId);
            expandoAo.PropertyGroups          = aoProd.InputJson?.PropertyGroupList;
            expandoAo.IsAssigned              = aoProd.InputJson?.IsAssigned;
            expandoAo.ProductId               = aoProd.ProductId;
            expandoAo.UsePrimaryProperties    = aoProd.InputJson?.UsePrimaryProperties;
            expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);

            productList.Remove(aoProd);
        }

        productList.Add(aoEnvelope);
        return JsonConvert.SerializeObject(expandoList);
    }

    #endregion
}
