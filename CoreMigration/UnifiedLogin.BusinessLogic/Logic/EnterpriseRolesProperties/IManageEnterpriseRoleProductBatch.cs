using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Batch;

namespace UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties
{
    /// <summary>
    /// Interface for managing enterprise role product batch operations
    /// </summary>
    public interface IManageEnterpriseRoleProductBatch
    {
        /// <summary>
        /// Generates and processes an enterprise role user product batch asynchronously
        /// </summary>
        Task<BatchProcessResult> GenerateEnterpriseRoleUserProductBatchAsync(
            EnterpriseRoleBatch batch,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Synchronous version for backward compatibility
        /// </summary>
        [System.Obsolete("Use GenerateEnterpriseRoleUserProductBatchAsync instead")]
        string GenerateEnterpriseRoleUserProductBatch(EnterpriseRoleBatch batch);
    }
}