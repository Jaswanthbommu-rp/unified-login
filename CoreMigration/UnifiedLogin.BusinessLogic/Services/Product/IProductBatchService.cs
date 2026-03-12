using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Services.Product;

/// <summary>
/// Service for managing product batch operations with async support
/// </summary>
public interface IProductBatchService
{
    /// <summary>
    /// Save product details for a user (Async)
    /// </summary>
    Task<int> SaveProductDetailsAsync(
        IList<ProductBatch> productList,
        long createUserPersonaId,
        long assignUserPersonaId,
        Guid organizationRealPageId,
        int userTypeId,
        bool userIsActive,
        CancellationToken cancellationToken = default,
        IList<string> aoProducts = null);

    /// <summary>
    /// Bundle AO products for batch processing
    /// </summary>
    string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0);

    /// <summary>
    /// Create batch processor group (Async)
    /// </summary>
    Task<BatchProcessorGroup> CreateBatchProcessGroupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Save individual product batch (Async)
    /// </summary>
    Task SaveProductBatchAsync(
        IProductBatch product,
        long createUserPersonaId,
        long assignUserPersonaId,
        Guid realPageId,
        string inputJson,
        int batchProcessTypeId = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable user products (Async)
    /// </summary>
    Task DisableUserProductsAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate user products (Async)
    /// </summary>
    Task ActivateUserProductsAsync(
        Guid createUserRealPageId,
        long createUserPersonaId,
        IList<UserLoginOnly> userLogins,
        CancellationToken cancellationToken = default);
}