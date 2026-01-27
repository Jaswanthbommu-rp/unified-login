using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties
{
    /// <summary>
    /// Interface for managing enterprise roles and primary properties operations
    /// </summary>
    public interface IManageEnterpriseRolesPrimaryProperties
    {
        /// <summary>
        /// Processes enterprise roles and primary properties data for a user asynchronously
        /// </summary>
        /// <param name="editorUserPersonaId">Editor user persona ID</param>
        /// <param name="subjectUserPersonaId">Subject user persona ID</param>
        /// <param name="enterpriseRoleTemplateId">Optional enterprise role template ID</param>
        /// <param name="createdDateTime">Optional created date time</param>
        /// <param name="batchProcessTypeId">Batch process type ID (default: 0)</param>
        /// <param name="isUnassignAllProducts">Whether to unassign all products (default: false)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Empty string on success, error message on failure</returns>
        Task<string> ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            int? enterpriseRoleTemplateId = null,
            DateTime? createdDateTime = null,
            int batchProcessTypeId = 0,
            bool isUnassignAllProducts = false,
            CancellationToken cancellationToken = default);
    }
}
