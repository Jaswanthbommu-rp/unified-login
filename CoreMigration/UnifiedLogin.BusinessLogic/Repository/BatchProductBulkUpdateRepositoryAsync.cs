using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Async-first Batch Product Bulk Update Repository.
/// Uses injected <see cref="IDbConnection"/> (Dapper) directly —
/// no <see cref="BaseRepository"/> inheritance, no <c>new</c> keyword.
/// <para>
/// Key improvements over the sync version:
/// <list type="bullet">
///   <item><c>Thread.Sleep</c> → <c>await Task.Delay</c> — polling no longer blocks the thread pool.</item>
///   <item>Duplicate <c>CreateBatchProcessGroup</c> overloads unified into one private async helper.</item>
///   <item>Unreachable <c>return false</c> after <c>return true</c> in <c>SaveProductBatch</c> removed.</item>
///   <item><c>new SamlRepository()</c> replaced by injected <see cref="ISamlRepositoryAsync"/>.</item>
///   <item>Silent <c>catch (Exception ex) {}</c> in batch-group helper → <see cref="ILogger"/> warning.</item>
/// </list>
/// </para>
/// </summary>
public sealed class BatchProductBulkUpdateRepositoryAsync : IBatchProductBulkUpdateRepositoryAsync
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly ISamlRepositoryAsync _samlRepo;
    private readonly ILogger<BatchProductBulkUpdateRepositoryAsync> _logger;

    #endregion

    #region Constructor

    public BatchProductBulkUpdateRepositoryAsync(
        IDbConnection db,
        ISamlRepositoryAsync samlRepo,
        ILogger<BatchProductBulkUpdateRepositoryAsync> logger)
    {
        _db       = db       ?? throw new ArgumentNullException(nameof(db));
        _samlRepo = samlRepo ?? throw new ArgumentNullException(nameof(samlRepo));
        _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region IBatchProductBulkUpdateRepositoryAsync

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: <c>SaveProductBatch</c> sync + unreachable <c>return false</c> —
    /// the outer <c>using</c> already returned <c>true</c>; exception path now
    /// bubbles to the caller instead of silently returning <c>false</c>.
    /// </remarks>
    public async Task<bool> SaveProductBatchAsync(
        long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
        IList<ProductBatch> userProductList, string onesiteWithOtherProductsJson,
        bool isOnesiteMix, int batchProcessType, long impersonatorUserId, string inputAOJson,
        CancellationToken cancellationToken = default)
    {
        // Batch group is created outside the per-product loop (same as original)
        var batchGroup = await CreateBatchProcessGroupAsync(cancellationToken);

        foreach (var prod in userProductList)
        {
            // Replaces: inline statusType ternary — same logic, named clearly
            int statusType = DetermineStatusType(prod, batchProcessType);
            string inputJson = BuildInputJson(prod, onesiteWithOtherProductsJson, isOnesiteMix, inputAOJson);

            var response = await _db.QuerySingleOrDefaultAsync<dynamic>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateProductBatch,
                    new
                    {
                        PersonRealPageId      = editorUserRealPageId,
                        CreateUserPersonaId   = editorUserPersonaId,
                        AssignUserPersonaId   = subjectUserPersonaId,
                        ProductId             = prod.ProductId,
                        StatusTypeId          = 5,
                        RetryCount            = 0,
                        InputJson             = inputJson,
                        CorrelationId         = Guid.NewGuid().ToString(),
                        BatchProcessTypeId    = statusType,
                        BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                        ImpersonatorUserId    = impersonatorUserId,
                        UseAPIV2              = true
                    },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            if (response?.Id == 0)
                throw new InvalidOperationException(
                    $"Exception while inserting product with code {prod.ProductId} in the product batch.");
        }

        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Replaces: <c>System.Threading.Thread.Sleep</c> blocking call with
    /// <c>await Task.Delay</c> so the thread pool is not held during polling.
    /// The transactional write (batch group + batch record) is committed before
    /// the polling loop begins — identical semantics to the original.
    /// </remarks>
    public async Task<IList<SamlAttributes>> CreateBatchAsync(
        long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
        int productId, int retryCheckCount, int statusCheckSleepMs, string defaultUserRole,
        long impersonatorUserId, CancellationToken cancellationToken = default)
    {
        IList<SamlAttributes> samlAttributesDetails = [];

        // ── Transactional write ────────────────────────────────────────────
        OpenIfClosed();
        using var tx = _db.BeginTransaction();
        int batchProcessorGroupId;

        try
        {
            var batchGroup        = await CreateBatchProcessGroupAsync(tx, cancellationToken);
            batchProcessorGroupId = batchGroup.BatchProcessorGroupId;

            var propertyList = productId == (int)ProductEnum.AdminSupportPortalStandard
                ? new List<string> { "all" }
                : new List<string> { "-1" };

            var response = await _db.QuerySingleOrDefaultAsync<dynamic>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateProductBatch,
                    new
                    {
                        PersonRealPageId      = editorUserRealPageId,
                        CreateUserPersonaId   = editorUserPersonaId,
                        AssignUserPersonaId   = subjectUserPersonaId,
                        ProductId             = productId,
                        StatusTypeId          = 5,
                        RetryCount            = 0,
                        BatchProcessorGroupId = batchProcessorGroupId,
                        ImpersonatorUserId    = impersonatorUserId,
                        InputJson             = JsonConvert.SerializeObject(new RolePropertyList
                        {
                            PropertyList = propertyList,
                            RoleList     = new List<string> { defaultUserRole },
                            IsAssigned   = true,
                            RoleType     = "Support Portal"
                        }),
                        UseAPIV2 = true
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            if (response?.Id == 0)
                throw new InvalidOperationException(
                    $"Exception while inserting product {productId} in the product batch.");

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        // ── Polling loop (outside transaction) ────────────────────────────
        // Replaces: System.Threading.Thread.Sleep → await Task.Delay
        while (retryCheckCount >= 0)
        {
            await Task.Delay(statusCheckSleepMs, cancellationToken);

            var batchDetails = await GetUserBatchDetailsAsync(
                batchProcessorGroupId, editorUserPersonaId, subjectUserPersonaId, cancellationToken);

            if (batchDetails.Any(a => a.StatusTypeId == 8))
            {
                // Status 8 = success — check SAML attributes
                samlAttributesDetails = await _samlRepo.GetProductSamlDetailsAsync(
                    subjectUserPersonaId, productId, cancellationToken);

                if (samlAttributesDetails.Count == 0)
                {
                    retryCheckCount--;
                }
                else
                {
                    // Give the system a final moment then return
                    await Task.Delay(statusCheckSleepMs, cancellationToken);
                    return samlAttributesDetails;
                }
            }
            else if (batchDetails.Any(a => a.StatusTypeId == 7))
            {
                // Status 7 = error — return empty list
                return samlAttributesDetails;
            }
            else
            {
                retryCheckCount--;
            }
        }

        return samlAttributesDetails;
    }

    /// <inheritdoc/>
    public async Task<IList<UserBatchProductDetail>> GetUserBatchDetailsAsync(
        int batchGroupId, long editorUserPersonId, long subjectUserPersonId,
        CancellationToken cancellationToken = default)
    {
        var result = await _db.QueryAsync<UserBatchProductDetail>(
            new CommandDefinition(
                StoredProcNameConstants.SP_GetUserBatchRecords,
                new { batchProcessorGroupId = batchGroupId, editorUserPersonId, subjectUserPersonId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateEnterpriseRoleProductBatchAsync(
        long productBatchId, int statusTypeId, CancellationToken cancellationToken = default)
    {
        return await _db.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateEnterpriseRoleProductBatch,
                new { productBatchId, statusTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<bool> UpdatePrimaryPropertyProductBatchAsync(
        long productBatchId, int statusTypeId, CancellationToken cancellationToken = default)
    {
        return await _db.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdatePrimaryPropertyProductBatch,
                new { productBatchId, statusTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateBulkUserProductBatchAsync(
        long productBatchId, int statusTypeId, CancellationToken cancellationToken = default)
    {
        return await _db.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateBulkUserProductBatch,
                new { productBatchId, statusTypeId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public async Task UpdateUnifiedPlatFormRoleAsync(
        int roleId, long editorUserId, long userPersonaId,
        bool deleteRole = false, CancellationToken cancellationToken = default)
    {
        await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonaToRole,
                new
                {
                    personaID          = userPersonaId,
                    roleID             = roleId,
                    IsDeleted          = deleteRole,
                    CreatedBy          = editorUserId,
                    personaPrivilgeID  = 0
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
    }

    #endregion

    #region Private helpers

    /// <summary>
    /// Creates a batch process group without a transaction — used by
    /// <see cref="SaveProductBatchAsync"/> which manages its own loop.
    /// Replaces: the no-argument <c>CreateBatchProcessGroup()</c> overload.
    /// </summary>
    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(
        CancellationToken cancellationToken)
    {
        var param = new DynamicParameters();
        param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

        try
        {
            await _db.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateBatchProcessorGroup,
                    param,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            // Replaces: silent catch{} — failure is now observable
            _logger.LogWarning(ex, "{Method} failed to create batch process group", nameof(CreateBatchProcessGroupAsync));
        }

        return new BatchProcessorGroup
        {
            BatchProcessorGroupId             = param.Get<int>("@BatchProcessorGroupID"),
            BatchProcessorGroupActivityLogged = false
        };
    }

    /// <summary>
    /// Creates a batch process group within an active transaction — used by
    /// <see cref="CreateBatchAsync"/> which needs atomic write + commit.
    /// Replaces: the <c>CreateBatchProcessGroup(IRepository repo)</c> overload.
    /// </summary>
    private async Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(
        IDbTransaction tx, CancellationToken cancellationToken)
    {
        var param = new DynamicParameters();
        param.Add("@BatchProcessorGroupID", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

        try
        {
            await _db.ExecuteAsync(
                new CommandDefinition(
                    StoredProcNameConstants.SP_CreateBatchProcessorGroup,
                    param,
                    transaction: tx,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Method} failed to create batch process group (in transaction)", nameof(CreateBatchProcessGroupAsync));
            throw; // re-throw so the outer tx is rolled back
        }

        return new BatchProcessorGroup
        {
            BatchProcessorGroupId             = param.Get<int>("@BatchProcessorGroupID"),
            BatchProcessorGroupActivityLogged = false
        };
    }

    /// <summary>
    /// Determines the batch processor status type for the given product.
    /// Replaces: inline ternary chain inside the <c>foreach</c>.
    /// </summary>
    private static int DetermineStatusType(ProductBatch prod, int batchProcessType)
    {
        // KnockCRM always gets CreateUpdateProductUser
        if (prod.ProductId == (int)ProductEnum.KnockCRM)
            return (int)BatchProcessType.CreateUpdateProductUser;

        // AO always gets CreateUpdateProductUser regardless of IsAssigned
        if (prod.ProductId == (int)ProductEnum.AssetOptimizer)
            return (int)BatchProcessType.CreateUpdateProductUser;

        // All others use caller-supplied type when the product is being assigned
        return prod.InputJson.IsAssigned ? batchProcessType : (int)BatchProcessType.CreateUpdateProductUser;
    }

    /// <summary>
    /// Builds the InputJson string for the product batch row.
    /// Replaces: three sequential <c>if</c> blocks mutating <c>inputJson</c>.
    /// </summary>
    private static string BuildInputJson(
        ProductBatch prod, string onesiteWithOtherProductsJson,
        bool isOnesiteMix, string inputAOJson)
    {
        if (prod.ProductId == (int)ProductEnum.AssetOptimizer)
            return inputAOJson;

        if (prod.ProductId == (int)ProductEnum.OneSite && isOnesiteMix)
            return onesiteWithOtherProductsJson;

        return JsonConvert.SerializeObject(prod.InputJson);
    }

    private void OpenIfClosed()
    {
        if (_db.State != ConnectionState.Open)
            _db.Open();
    }

    #endregion
}