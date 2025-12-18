using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Interface for SAML Repository
    /// </summary>
    public interface ISamlRepository
    {
		/// <summary>
		/// Get the SAML attribute names, types, and values by PersonaId and ProductId
		/// </summary>
		/// <param name="PersonaId">User personaId</param>
		/// <param name="ProductId">ProductId</param>
		/// <returns>List SamlAttributes object</returns>
		IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId);

        /// <summary>
        /// List Active Products by PersonaId
        /// </summary>
        /// <param name="PersonaId">User personaId</param>
        /// <param name="ProductId">ProductId</param>
        /// <param name="ProductType">NULL, ProductWithFavorites, IsResource, IsFavorite</param>
        /// <returns>List of Portfolio Product User Details</returns>
        IList<PersonaProductUserDetails> ListActiveProductsByPersonaId(long PersonaId, int ProductId, string ProductType);

        /// <summary>
        /// List All Products by PersonaId, including deleted and errored
        /// </summary>
        /// <param name="PersonaId">User personaId</param>
        /// <param name="ProductId">ProductId</param>
        /// <param name="ProductType">NULL, ProductWithFavorites, IsResource, IsFavorite</param>
        /// <returns>List of Portfolio Product User Details</returns>
        IList<PersonaProductUserDetails> ListAllProductsByPersonaId(long PersonaId, int ProductId, string ProductType);

		/// <summary>
		/// Used to create a new SAML attribute for the given personaId
		/// </summary>
		/// <param name="PersonaId">The persona of the person to add the saml attribue to</param>
		/// <param name="ProductId">The product id for the saml attribute</param>
		/// <param name="SamlAttributeId">The saml attribute type being created</param>
		/// <param name="Value">The values of the saml attribute being created</param>
		/// <returns>The result of creating the saml attribute</returns>
		RepositoryResponse CreateSamlUserAttribute(long PersonaId, int ProductId, SamlAttributeEnum SamlAttributeId, string Value);

		/// <summary>
		/// Used to update the Value of a SAML attribute for the given SamlUserAttributeId
		/// </summary>
		/// <param name="samlAttributes">SamlAttributes object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateSamlUserAttribute(SamlAttributes samlAttributes);

	    /// <summary>
	    /// Used to delete all SAML product information and status for a user
	    /// </summary>
	    /// <param name="personaId">The persona of the person to delete all of the product SAML information and status for</param>
	    /// <param name="productId">The product id to delete</param>
	    /// <returns>The result of deleting the user info</returns>
	    RepositoryResponse DeleteSamlUserProductInfoAndStatus(long personaId, int productId);

		/// <summary>
		/// List All Persona Products SAML Details
		/// </summary>
		/// <param name="PersonaId">User personaId</param>		
		/// <returns>List of Persona Products SAML Details</returns>
		IList<ProductSamlDetails> ListPersonaProductsSamlDetails(long PersonaId);

		/// <summary>
		/// Used to delete product error for a user
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns></returns>
		RepositoryResponse DeletePersonaProductError(long personaId);


		/// <summary>
		/// Get the SAML product attribute DisplayName, ProductId by ProductId
		/// </summary>
		/// <param name="ProductId">ProductId</param>
		/// <returns>List SamlProductAttributes object</returns>
		IList<SamlProductAttributes> GetSamlProductAttributes(int ProductId);

		ProductSamlSettings GetProductSamlSettingsByProductId(int productId);

		/// <summary>
		/// Remove specific SAML attribute for a user and product
		/// </summary>
		/// <param name="PersonaId"></param>
		/// <param name="ProductId"></param>
		/// <param name="SamlAttributeId"></param>
		/// <returns></returns>
		RepositoryResponse RemoveSamlUserAttributeBySamlAttributeId(long PersonaId, int ProductId, SamlAttributeEnum SamlAttributeId);
    }
}