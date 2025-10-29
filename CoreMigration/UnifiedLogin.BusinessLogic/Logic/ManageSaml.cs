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
		ISamlRepository _samlRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageSaml Constructor
		/// </summary>
		/// <param name="samlRepository">Saml Repository</param>
		public ManageSaml(ISamlRepository samlRepository)
		{
			_samlRepository = samlRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageSaml Controller class
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
		/// <param name="PersonaId">The persona of the person to add the saml attribue to</param>
		/// <param name="ProductId">The product id for the saml attribute</param>
		/// <param name="SamlAttributeId">The saml attribute type being created</param>
		/// <param name="Value">The values of the saml attribute being created</param>
		/// <returns>The result of creating the saml attribute</returns>
		public RepositoryResponse CreateSamlUserAttribute(long PersonaId, int ProductId, SamlAttributeEnum SamlAttributeId, string Value)
		{
			if (PersonaId == 0)
			{
				throw new Exception("Invalid parameter PersonaId.");
			}

			if (ProductId == 0)
			{
				throw new Exception("Invalid parameter ProductId.");
			}

			if (Value.Trim().Length == 0)
			{
				throw new Exception("Invalid parameter Value.");
			}

			if (SamlAttributeId == 0)
			{
				throw new Exception("Invalid parameter SamlAttributeId.");
			}

			return _samlRepository.CreateSamlUserAttribute(PersonaId, ProductId, SamlAttributeId, Value);
		}

		/// <summary>
		/// Get the SAML attribute names, types, and values by PersonaId and ProductId
		/// </summary>
		/// <param name="PersonaId">Persona Unique Id</param>
		/// <param name="ProductId">Product Unique Id</param>
		/// <returns>List of Saml Attributes</returns>
		public IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId)
		{
			return _samlRepository.GetProductSamlDetails(PersonaId, ProductId);
		}

		/// <summary>
		/// Used to update the Value of a SAML attribute for the given SamlUserAttributeId
		/// </summary>
		/// <param name="samlAttributes">SamlAttributes object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		public RepositoryResponse UpdateSamlUserAttribute(SamlAttributes samlAttributes)
		{
			if (samlAttributes == null)
			{
				throw new ArgumentNullException(nameof(samlAttributes), "Null SamlAttributes.");
			}

			return _samlRepository.UpdateSamlUserAttribute(samlAttributes);
		}
		#endregion
	}
}