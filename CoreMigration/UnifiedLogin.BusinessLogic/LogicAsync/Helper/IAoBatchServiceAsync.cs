using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helper;

/// <summary>
/// Async-first interface for building Asset Optimization (AO/BI)
/// <see cref="ProductBatch"/> records during user-clone operations.
/// <para>
/// Replaces <c>BatchHelper.CreateAoBatchRecords(DefaultUserClaim, …)</c> which
/// instantiated <c>ManageProductAssetOptimization(userClaim)</c> and
/// <c>new SamlRepository()</c> inline.
/// </para>
/// <para>
/// <b>Note:</b> the internal call to <c>ManageProductAssetOptimization.CopyRegularUser</c>
/// is still synchronous (annotated <c>// SYNC</c>) pending an async port of that method
/// in <c>IManageProductAssetOptimizationAsync</c>.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public interface IAoBatchServiceAsync
{
    /// <summary>
    /// Builds AO <see cref="ProductBatch"/> records for a user-clone operation.
    /// </summary>
    /// <param name="editorPersonaId">Persona ID of the editor initiating the clone.</param>
    /// <param name="newUserPersonaId">Persona ID of the newly created user.</param>
    /// <param name="externalUser">Whether the new user is an external (SAML/BI) user.</param>
    /// <param name="usePrimaryProperties">Whether to use primary-property inheritance.</param>
    /// <param name="propertiesResponse">Property list for the source user.</param>
    /// <param name="productId">Target AO product ID.</param>
    /// <param name="productRoles">Optional role list to assign.</param>
    /// <param name="productBatchList">Accumulator list — new records are appended.</param>
    /// <param name="isDeleted">Pass <c>true</c> when building a removal batch.</param>
    Task<IList<ProductBatch>> CreateAoBatchRecordsAsync(
        long editorPersonaId,
        long newUserPersonaId,
        bool externalUser,
        bool usePrimaryProperties,
        ListResponse propertiesResponse,
        int productId,
        IList<ProductRole>? productRoles = null,
        IList<ProductBatch>? productBatchList = null,
        bool isDeleted = false,
        CancellationToken cancellationToken = default);
}
