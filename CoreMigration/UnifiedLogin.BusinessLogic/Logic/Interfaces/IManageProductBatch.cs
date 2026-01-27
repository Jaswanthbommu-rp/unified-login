using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    /// <summary>
    /// Interface for managing product batch operations
    /// </summary>
    public interface IManageProductBatch
    {
        /// <summary>
        /// Gets product roles for a user
        /// </summary>
        ListResponse GetProductRoles(
            long editorPersonaId,
            long userPersonaId,
            int productId,
            long organizationPartyId,
            DefaultUserClaim userClaim);


        /// <summary>
        /// Gets enterprise role user primary properties data asynchronously
        /// </summary>
        Task<ListResponse> GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
            long editorPersonaId,
            long userPersonaId,
            int productId,
            bool usePrimaryProperties = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets existing user primary properties data
        /// </summary>
        List<int> GetExistingUserPrimaryPropertiesData(long userPersonaId, int productId);

        /// <summary>
        /// Gets product batch record asynchronously
        /// </summary>
        Task<ProductBatch> GetProductBatchRecordAsync(
            long editorUserPersonaId,
            long subjectUserPersonaId,
            IList<ProductRole> productRoles,
            ListResponse propertiesResponse,
            ListResponse rolesResponse,
            int product,
            bool usePrimaryProperties,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if product is enabled for use primary property
        /// </summary>
        bool IsProductEnabledForUsePrimaryProperty(int productId);

        /// <summary>
        /// Gets persona role rights
        /// </summary>
        List<string> GetPersonaRoleRights(long editorPersonaId, long organizationPartyId);
    }
}