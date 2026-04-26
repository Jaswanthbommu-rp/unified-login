using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageProductBatchAsync
{
    /// <summary>
    /// Builds a <see cref="ProductBatch"/> record for the given product, gathering
    /// product-specific properties, roles, groups, and notifications as needed.
    /// </summary>
    Task<ProductBatch> GetProductBatchRecordAsync(
        long editorPersonaId,
        long subjectPersonaId,
        IList<ProductRole> productRoles,
        ListResponse propertiesResponse,
        ListResponse rolesResponse,
        int product,
        bool usePrimaryProperties,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the filtered primary-property list for a user on a given product,
    /// compared against the user's UPFM property assignments.
    /// </summary>
    Task<ListResponse> GetUserPrimaryPropertiesDataAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the enterprise-role-scoped primary-property list, with optional
    /// primary-property filtering. Handles the KnockCRM special case.
    /// </summary>
    Task<ListResponse> GetEnterpriseRoleUserPrimaryPropertiesDataAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        bool usePrimaryProperties = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the list of UPFM property-instance IDs assigned to the user for a product.
    /// </summary>
    Task<List<int>> GetExistingUserPrimaryPropertiesDataAsync(
        long userPersonaId,
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns product properties for the given user via the panel async layer.
    /// </summary>
    Task<ListResponse> GetProductPropertiesAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns product roles for the given user and company via the panel async layer.
    /// </summary>
    Task<ListResponse> GetProductRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        int productId,
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> when the product's internal settings have
    /// <c>UsePrimaryProperties = 1</c>.
    /// </summary>
    Task<bool> IsProductEnabledForUsePrimaryPropertyAsync(
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the flat list of right nick-names held by all roles assigned to
    /// <paramref name="personaId"/> for the Unified Platform product.
    /// Role-rights are cached per <paramref name="orgPartyId"/> for 60 minutes.
    /// </summary>
    Task<List<string>> GetPersonaRoleRightsAsync(
        long personaId,
        long orgPartyId,
        CancellationToken cancellationToken = default);
}
