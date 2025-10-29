using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// SAML Repository
	/// </summary>
	public class SamlRepository : BaseRepository, ISamlRepository
    {
		#region Ctor
		/// <summary>
		/// SAML base Constructor
		/// </summary>
		public SamlRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public SamlRepository(IRepository repository) : base(repository)
        {
        }
		#endregion

		/// <summary>
		/// List Active Products by PersonaId
		/// </summary>
		/// <param name="PersonaId">User personaId</param>
		/// <param name="ProductId">ProductId</param>
		/// <param name="ProductType">NULL, ProductWithFavorites, IsResource, IsFavorite</param>
		/// <returns>List of Portfolio Product User Details</returns>
		public IList<PersonaProductUserDetails> ListActiveProductsByPersonaId(long PersonaId, int ProductId, string ProductType)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = PersonaId, ProductStatusValue = ((Int32)UserUiStatusType.AccountCreationSuccessful).ToString() }).ToList();
            }
        }

        /// <summary>
        /// List All Products by PersonaId, including deleted and errored
        /// </summary>
        /// <param name="PersonaId">User personaId</param>
        /// <param name="ProductId">ProductId</param>
        /// <param name="ProductType">NULL, ProductWithFavorites, IsResource, IsFavorite</param>
        /// <returns>List of Portfolio Product User Details</returns>
        public IList<PersonaProductUserDetails> ListAllProductsByPersonaId(long PersonaId, int ProductId, string ProductType)
        {
	        using (var repo = GetRepository())
	        {
		        return repo.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = PersonaId }).ToList();
	        }
        }

		/// <summary>
		/// List All Persona Products SAML Details
		/// </summary>
		/// <param name="PersonaId">User personaId</param>		
		/// <returns>List of Persona Products SAML Details</returns>
		public IList<ProductSamlDetails> ListPersonaProductsSamlDetails(long PersonaId)
		{
			using (var repo = GetRepository())
			{
				return repo.GetMany<ProductSamlDetails>(StoredProcNameConstants.SP_ListPersonaProductsSamlDetails, new { PersonaId = PersonaId }).ToList();
			}
		}

		/// <summary>
		/// Get the SAML attribute names, types, and values by PersonaId and ProductId
		/// </summary>
		/// <param name="PersonaId">User personaId</param>
		/// <param name="ProductId">ProductId</param>
		/// <returns>list SamlAttributes object</returns>
		public IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails, new { PersonaId, ProductId }).ToList();
            }
        }

		/// <summary>
		/// Get the SAML product attribute DisplayName, ProductId by ProductId
		/// </summary>
		/// <param name="ProductId">ProductId</param>
		/// <returns>list SamlProductAttributes object</returns>
		public IList<SamlProductAttributes> GetSamlProductAttributes(int ProductId)
		{
			using (var repo = GetRepository())
			{
				return repo.GetMany<SamlProductAttributes>(StoredProcNameConstants.SP_GetSamlProductAttributes, new { ProductId }).ToList();
			}
		}

		/// <summary>
		/// Get a product Saml Settings
		/// </summary>
		/// <param name="productId"></param>
		/// <returns>ProductSamlSettings object</returns>
		public ProductSamlSettings GetProductSamlSettingsByProductId(int productId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<ProductSamlSettings>(StoredProcNameConstants.SP_GetProductSamlSettings, new { productId });
            }
        }

		/// <summary>
		/// Used to delete all SAML product information and status for a user
		/// </summary>
		/// <param name="personaId">The persona of the person to delete all of the product SAML information and status for</param>
		/// <param name="productId">The product id to delete</param>
		/// <returns>The result of deleting the user info</returns>
		public RepositoryResponse DeleteSamlUserProductInfoAndStatus(long personaId, int productId)
	    {
		    using (var repo = GetRepository())
		    {
			    dynamic param = new
			    {
				    PersonaId = personaId,
				    ProductId = productId
			    };
			    var result = repo.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_DeleteSamlUserProductInfoAndStatus, param);
			    return result;

		    }
	    }
		/// <summary>
		/// Used to delete product error for a user
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns></returns>
		public RepositoryResponse DeletePersonaProductError(long personaId)
		{
			using (var repo = GetRepository())
			{
				dynamic param = new
				{
					PersonaId = personaId
				};
				var result = repo.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_ManagePersonaProductError, param);
				return result;

			}
		}

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
            using (var repo = GetRepository())
            {
                dynamic param = new
                {
                    PersonaId = PersonaId,
                    ProductId = ProductId,
                    SamlAttributeId = (int)SamlAttributeId,
                    Value = Value
                };
                var result = repo.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateSamlUserAttribute, param);
                return result;
                
            }
        }

		/// <summary>
		/// Used to update the Value of a SAML attribute for the given SamlUserAttributeId
		/// </summary>
		/// <param name="samlAttributes">SamlAttributes object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		public RepositoryResponse UpdateSamlUserAttribute(SamlAttributes samlAttributes)
		{
            dynamic param = new
            {
                SamlUserAttributeId = (int)samlAttributes.SamlUserAttributeId,
                Value = samlAttributes.Value
            };

			using (var repo = GetRepository())
			{
				var result = repo.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateSamlUserAttribute, param);
				return result;

			}
		}

		public RepositoryResponse RemoveSamlUserAttributeBySamlAttributeId(long PersonaId, int ProductId, SamlAttributeEnum SamlAttributeId)
        {
            using (var repo = GetRepository())
            {
                dynamic param = new
                {
                    PersonaId = PersonaId,
                    ProductId = ProductId,
                    SamlAttributeId = (int)SamlAttributeId
                };
                var result = repo.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_RemoveSamlUserAttributeBySamlAttributeId, param);
                return result;

            }
        }
    }
}