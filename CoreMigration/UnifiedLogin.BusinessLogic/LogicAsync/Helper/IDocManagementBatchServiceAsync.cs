using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helper;

/// <summary>
/// Async-first interface for building <see cref="ProductBatch"/> records for
/// RP Document Management (Document Director).
/// <para>
/// Replaces the static <c>BatchHelper.CreateDocManagementBatchRecords(DefaultUserClaim, …)</c>
/// which instantiated <c>ManageProductRPDocumentManagement(userClaim)</c> inline.
/// Identity context is now resolved by <c>IManageProductRPDocumentManagementAsync</c> internally
/// via <c>IUserClaimsAccessor</c>.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public interface IDocManagementBatchServiceAsync
{
    /// <summary>
    /// Builds a <see cref="ProductBatch"/> for the RP Document Management product by
    /// fetching the editor's role/property assignments and constructing the batch payload.
    /// </summary>
    /// <param name="editorPersonaId">Persona ID of the user performing the operation.</param>
    /// <param name="subjectPersonaId">Persona ID of the user being created/cloned.</param>
    /// <param name="usePrimaryProperties">Whether to use primary-property inheritance.</param>
    Task<ProductBatch> CreateDocManagementBatchRecordAsync(
        long editorPersonaId,
        long subjectPersonaId,
        bool usePrimaryProperties,
        CancellationToken cancellationToken = default);
}
