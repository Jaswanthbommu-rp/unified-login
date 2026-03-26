using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.IntegrationsMarketplace;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for Integration Marketplace user management.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Product.Interfaces.IManageProductIntegrationMarketplace"/>.
/// </summary>
public interface IManageProductIntegrationMarketplaceAsync
{
    /// <summary>
    /// Returns the available roles (User Access Groups) for a given party,
    /// merged with the user's currently assigned role.
    /// </summary>
    Task<ListResponse> GetRolesAsync(
        long editorPersonaId,
        long userPersonaId,
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the user type for a user in Integration Marketplace.
    /// Delegates to <see cref="ManageIntegrationMarketplaceUserAsync"/>.
    /// </summary>
    Task<string> ChangeIntegrationMarketplaceUserTypeAsync(
        long createUserPersonaId,
        long assignUserPersonaId,
        IntegrationMarketplacePropertyRole rpList,
        BatchProcessType batchProcessType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a user in Integration Marketplace.
    /// </summary>
    Task<string> ManageIntegrationMarketplaceUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IntegrationMarketplacePropertyRole userAssignProductPropertyRole,
        BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unassigns a user from Integration Marketplace, removing their SAML product info.
    /// </summary>
    Task<string> UnassignUserAsync(
        long editorPersonaId,
        long userPersonaId,
        IntegrationMarketplacePropertyRole userAssignProductPropertyRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all available roles from the Integration Marketplace API.
    /// </summary>
    Task<List<IntegrationMarketplaceRole>> GetIntegrationMarketplaceRolesAsync(
        CancellationToken cancellationToken = default);
}
