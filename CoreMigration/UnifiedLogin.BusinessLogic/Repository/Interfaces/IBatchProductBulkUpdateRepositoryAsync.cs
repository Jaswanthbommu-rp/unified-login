using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IBatchProductBulkUpdateRepositoryAsync
{
    /// <summary>Queues a product batch for each product in the list.</summary>
    Task<bool> SaveProductBatchAsync(
        long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
        IList<ProductBatch> userProductList, string onesiteWithOtherProductsJson,
        bool isOnesiteMix, int batchProcessType, long impersonatorUserId, string inputAOJson,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a single product batch record, then polls until a SAML attribute
    /// is written or retries are exhausted.
    /// Replaces: blocking <see cref="System.Threading.Thread.Sleep"/> with
    /// <see cref="Task.Delay"/>.
    /// </summary>
    Task<IList<SamlAttributes>> CreateBatchAsync(
        long editorUserPersonaId, long subjectUserPersonaId, Guid editorUserRealPageId,
        int productId, int retryCheckCount, int statusCheckSleepMs, string defaultUserRole,
        long impersonatorUserId, CancellationToken cancellationToken = default);

    Task<IList<UserBatchProductDetail>> GetUserBatchDetailsAsync(
        int batchGroupId, long editorUserPersonId, long subjectUserPersonId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateEnterpriseRoleProductBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default);
    Task<bool> UpdatePrimaryPropertyProductBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBulkUserProductBatchAsync(long productBatchId, int statusTypeId, CancellationToken cancellationToken = default);

    /// <summary>Links or unlinks a role from a persona.</summary>
    Task UpdateUnifiedPlatFormRoleAsync(int roleId, long editorUserId, long userPersonaId, bool deleteRole = false, CancellationToken cancellationToken = default);
}