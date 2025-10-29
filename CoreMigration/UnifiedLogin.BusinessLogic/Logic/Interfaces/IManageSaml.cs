using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage Saml repository calls
	/// </summary>
	public interface IManageSaml
	{
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
		/// Get the SAML attribute names, types, and values by PersonaId and ProductId
		/// </summary>
		/// <param name="PersonaId">Persona Unique Id</param>
		/// <param name="ProductId">Product Unique Id</param>
		/// <returns>List of Saml Attributes</returns>
		IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId);

		/// <summary>
		/// Used to update the Value of a SAML attribute for the given SamlUserAttributeId
		/// </summary>
		/// <param name="samlAttributes">SamlAttributes object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateSamlUserAttribute(SamlAttributes samlAttributes);
	}
}