using UnifiedLogin.SharedObjects.Enterprise;

namespace UnifiedLogin.BusinessLogic.Repository.Enterprise;

/// <summary>
/// Async data-access interface for enterprise user operations.
/// Replaces the sync <see cref="IEntUserRepository"/> whose implementations depended on
/// <c>DefaultUserClaim</c> constructor injection and <c>BaseRepository</c> connection management.
/// <para>
/// <b>Design note on <c>productCodeToIdMap</c>:</b>
/// The sync <c>EntUserRepository.SaveProductBatch</c> called <c>new ProductRepository()</c> internally
/// to resolve product codes to IDs — a cross-repository coupling smell.
/// In the async design this lookup is the caller's responsibility: the logic layer
/// pre-resolves codes and passes a read-only map, keeping the repository a pure data-access layer.
/// </para>
/// </summary>
public interface IEntUserRepositoryAsync
{
    /// <summary>
    /// Creates an enterprise user record and queues the initial product-batch assignments
    /// within a single database transaction.
    /// </summary>
    /// <param name="userProductDetails">Full user profile and product list.</param>
    /// <param name="productCodeToIdMap">
    /// Pre-resolved map of UPPER-CASE books product code → internal product ID.
    /// Built by the caller so the repository stays free of product-lookup dependencies.
    /// </param>
    /// <param name="editorPersonaId">Persona ID of the logged-in editor, from <c>IUserClaimsAccessor.PersonaId</c>.</param>
    /// <param name="impersonatedBy">
    /// RealPageId of the impersonating user when the call is made under impersonation;
    /// <see cref="Guid.Empty"/> otherwise. From <c>IUserClaimsAccessor.ImpersonatedBy</c>.
    /// </param>
    /// <returns>The new user's RealPageId string.</returns>
    Task<string> CreateEnterpriseUserAsync(
        UserProductDetails userProductDetails,
        IReadOnlyDictionary<string, int> productCodeToIdMap,
        long editorPersonaId,
        Guid impersonatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a paged/filtered list of users for an organisation.</summary>
    Task<IList<UsersData>> ListUsersAsync(
        long organizationPartyId,
        IList<int> productIdList,
        int statusTypeId,
        Guid? realPageId = null,
        string? name = null,
        int rowsPerPage = 0,
        int pageNumber = 1,
        CancellationToken cancellationToken = default);

    /// <summary>Returns product-login detail attributes for a persona.</summary>
    Task<IList<UserProductDetailAttribute>> ListUserProductDetailsLoginByPersonaIdAsync(
        long personaId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns product-login detail attributes for a login name across all companies.</summary>
    Task<IList<UserProductDetailAttribute>> ListUserProductDetailsLoginByLoginNameAsync(
        string loginName,
        CancellationToken cancellationToken = default);
}
