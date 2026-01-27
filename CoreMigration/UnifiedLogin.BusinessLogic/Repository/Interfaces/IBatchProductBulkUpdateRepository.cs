using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for batch product bulk update operations
    /// </summary>
    public interface IBatchProductBulkUpdateRepository
    {
        /// <summary>
        /// Saves product batch records for processing
        /// </summary>
        /// <param name="editorUserPersonaId">Editor user persona ID</param>
        /// <param name="subjectUserPersonaId">Subject user persona ID</param>
        /// <param name="editorUserRealPageId">Editor user RealPage ID</param>
        /// <param name="userProductList">List of product batches to save</param>
        /// <param name="onesiteWithOherProductsJson">JSON for OneSite mixed products</param>
        /// <param name="isOnesiteMix">Whether OneSite products are mixed</param>
        /// <param name="batchProcessType">Batch process type</param>
        /// <param name="impersonatorUserId">Impersonator user ID</param>
        /// <param name="inputAOJSON">Asset Optimization JSON input</param>
        /// <returns>True if successful, false otherwise</returns>
        bool SaveProductBatch(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            Guid editorUserRealPageId,
            IList<ProductBatch> userProductList,
            string onesiteWithOherProductsJson,
            bool isOnesiteMix,
            int batchProcessType,
            long impersonatorUserId,
            string inputAOJSON);

        /// <summary>
        /// Creates a batch for admin portal
        /// </summary>
        IList<SamlAttributes> CreateBatch(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            Guid editorUserRealPageId,
            int productId,
            int retryCheckCount,
            int statusCheckSleep,
            string defaultUserRole,
            long impersonatorUserId);

        /// <summary>
        /// Gets user batch details
        /// </summary>
        IList<UserBatchProductDetail> GetUserBatchDetails(
            int batchGroupId,
            long editorUserPersonId,
            long subjectUserPersonId);

        /// <summary>
        /// Updates an enterprise role product batch status
        /// </summary>
        bool UpdateEnterpriseRoleProductBatch(long productBatchId, int statusTypeId);

        /// <summary>
        /// Updates a primary property product batch status
        /// </summary>
        bool UpdatePrimaryPropertyProductBatch(long productBatchId, int statusTypeId);

        /// <summary>
        /// Updates a bulk user product batch status
        /// </summary>
        bool UpdateBulkUserProductBatch(long productBatchId, int statusTypeId);

        /// <summary>
        /// Updates unified platform role
        /// </summary>
        void UpdateUnifiedPlatFormRole(int roleId, long editorUserId, long userPersonaId, bool deleteRole = false);
    }
}
