using System;
using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Saml repository calls
	/// </summary>
	public class ManageSaml : IManageSaml
	{
		#region Private Variables
		private readonly ISamlRepository _samlRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageSaml Constructor with dependency injection (recommended)
		/// </summary>
		/// <param name="samlRepository">Saml Repository</param>
		public ManageSaml(ISamlRepository samlRepository)
		{
			_samlRepository = samlRepository ?? throw new ArgumentNullException(nameof(samlRepository));
		}

		/// <summary>
		/// Create a basic instance of the ManageSaml class (legacy support)
		/// </summary>
		public ManageSaml()
		{
			_samlRepository = new SamlRepository();
		}
		#endregion

		#region Public ManageSaml methods
		/// <summary>
		/// Used to create a new SAML attribute for the given personaId
		/// </summary>
		/// <param name="PersonaId">The persona of the person to add the saml attribute to</param>
		/// <param name="ProductId">The product id for the saml attribute</param>
		/// <param name="SamlAttributeId">The saml attribute type being created</param>
		/// <param name="Value">The values of the saml attribute being created</param>
		/// <returns>The result of creating the saml attribute</returns>
		/// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
		/// <exception cref="ArgumentNullException">Thrown when Value is null</exception>
		public RepositoryResponse CreateSamlUserAttribute(long PersonaId, int ProductId, SamlAttributeEnum SamlAttributeId, string Value)
		{
			ValidateCreateParameters(PersonaId, ProductId, SamlAttributeId, Value);

			return _samlRepository.CreateSamlUserAttribute(PersonaId, ProductId, SamlAttributeId, Value);
		}

		/// <summary>
		/// Get the SAML attribute names, types, and values by PersonaId and ProductId
		/// </summary>
		/// <param name="PersonaId">Persona Unique Id</param>
		/// <param name="ProductId">Product Unique Id</param>
		/// <returns>List of Saml Attributes</returns>
		/// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
		public IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId)
		{
			ValidateGetParameters(PersonaId, ProductId);

			return _samlRepository.GetProductSamlDetails(PersonaId, ProductId);
		}

		/// <summary>
		/// Used to update the Value of a SAML attribute for the given SamlUserAttributeId
		/// </summary>
		/// <param name="samlAttributes">SamlAttributes object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		/// <exception cref="ArgumentNullException">Thrown when samlAttributes is null</exception>
		public RepositoryResponse UpdateSamlUserAttribute(SamlAttributes samlAttributes)
		{
			if (samlAttributes == null)
			{
				throw new ArgumentNullException(nameof(samlAttributes), "Null SamlAttributes.");
			}

			return _samlRepository.UpdateSamlUserAttribute(samlAttributes);
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Validate parameters for CreateSamlUserAttribute
		/// </summary>
		/// <param name="personaId">Persona ID</param>
		/// <param name="productId">Product ID</param>
		/// <param name="samlAttributeId">SAML Attribute ID</param>
		/// <param name="value">Attribute value</param>
		private void ValidateCreateParameters(long personaId, int productId, SamlAttributeEnum samlAttributeId, string value)
		{
			if (personaId == 0)
			{
				throw new ArgumentException("Invalid parameter PersonaId.", nameof(personaId));
			}

			if (productId == 0)
			{
				throw new ArgumentException("Invalid parameter ProductId.", nameof(productId));
			}

			if (samlAttributeId == 0)
			{
				throw new ArgumentException("Invalid parameter SamlAttributeId.", nameof(samlAttributeId));
			}

			if (value == null)
			{
				throw new ArgumentNullException(nameof(value), "Value cannot be null.");
			}

			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException("Invalid parameter Value.", nameof(value));
			}
		}

		/// <summary>
		/// Validate parameters for GetProductSamlDetails
		/// </summary>
		/// <param name="personaId">Persona ID</param>
		/// <param name="productId">Product ID</param>
		private void ValidateGetParameters(long personaId, int productId)
		{
			if (personaId == 0)
			{
				throw new ArgumentException("Invalid parameter PersonaId.", nameof(personaId));
			}

			if (productId == 0)
			{
				throw new ArgumentException("Invalid parameter ProductId.", nameof(productId));
			}
		}
		#endregion
	}
}