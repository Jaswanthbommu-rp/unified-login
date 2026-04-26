using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for building clone-user product batch records.
/// <para>
/// Replaces <c>ManageCloneProductBatch</c> which accepted <c>DefaultUserClaim</c>
/// via its constructor and instantiated all product services inline with
/// <c>new ManageProductXxx(_userClaim)</c>.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public interface IManageCloneProductBatchAsync
{
    /// <summary>
    /// Builds the product batch list for a clone-user operation.
    /// For each product assigned to <paramref name="baseOrgAdminPersonaId"/> the method
    /// fetches the source user's roles / properties / settings and packages them as a
    /// <see cref="ProductBatch"/> ready for the batch processor.
    /// </summary>
    /// <param name="personaId">The new (target) user's persona ID.</param>
    /// <param name="userProducts">Products to clone, sourced from the base persona.</param>
    /// <param name="baseOrgAdminPersonaId">The source persona being cloned from.</param>
    /// <param name="upfmProperty">UPFM property used for primary-property translation.</param>
    /// <param name="productSettingList">Per-product <c>UsePrimaryProperties</c> settings.</param>
    /// <param name="externalUser">Whether the target user is an external user (affects AO BI path).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IList<ProductBatch>> GetUserProductBatchDataAsync(
        long personaId,
        List<PersonaProductUserDetails> userProducts,
        long baseOrgAdminPersonaId,
        UPFMProperty upfmProperty,
        List<ProductSettingList> productSettingList,
        bool externalUser = false,
        CancellationToken cancellationToken = default);
}
