using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async interface for SAML Repository — CancellationToken added to all methods.
/// </summary>
public interface ISamlRepositoryAsync
{
    Task<IList<SamlAttributes>> GetProductSamlDetailsAsync(long personaId, int productId, CancellationToken cancellationToken = default);
    Task<IList<PersonaProductUserDetails>> ListActiveProductsByPersonaIdAsync(long personaId, int productId, string productType, CancellationToken cancellationToken = default);
    Task<IList<PersonaProductUserDetails>> ListAllProductsByPersonaIdAsync(long personaId, int productId, string productType, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> CreateSamlUserAttributeAsync(long personaId, int productId, SamlAttributeEnum samlAttributeId, string value, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdateSamlUserAttributeAsync(SamlAttributes samlAttributes, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> DeleteSamlUserProductInfoAndStatusAsync(long personaId, int productId, CancellationToken cancellationToken = default);
    Task<IList<ProductSamlDetails>> ListPersonaProductsSamlDetailsAsync(long personaId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> DeletePersonaProductErrorAsync(long personaId, CancellationToken cancellationToken = default);
    Task<IList<SamlProductAttributes>> GetSamlProductAttributesAsync(int productId, CancellationToken cancellationToken = default);
    Task<ProductSamlSettings> GetProductSamlSettingsByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> RemoveSamlUserAttributeBySamlAttributeIdAsync(long personaId, int productId, SamlAttributeEnum samlAttributeId, CancellationToken cancellationToken = default);
}