using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Used to get property information from UnifiedLogin for the given persona and product. 
	/// </summary>
	public class PropertyRepository : BaseRepository, IPropertyRepository
	{
		#region Constructor
		/// <summary>
		/// Omnichannel base Constructor
		/// </summary>
		public PropertyRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}
		#endregion

		/// <summary>
		/// List of Roles by User Persona ID
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>List of Properties/Role assigned to Persona</returns>
		public List<ProductProperty> ListPropertiesByPersona(long userPersonaId, ProductEnum productId)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = (int)productId
				};

				List<ProductProperty> propList = new List<ProductProperty>();
				var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPropertyMapping, param);
				if (result != null)
				{
					foreach (var item in result)
					{
						propList.Add(new ProductProperty { ID = item.PropertyID.ToString() });
					}
				}
				return propList;
			}
		}

		/// <summary>
		/// Insert or Remove a Property for the given User
		/// </summary>
		/// <param name="userPersonaId">User Persona ID</param>      
		/// <param name="productId">Product ID</param>      
		/// <param name="propertyId">Property ID</param>      
		/// <param name="remove">isDeleted</param>   
		/// <returns>List of Roles assigned to Persona</returns>
		public RepositoryResponse InsertRemoveAssignedPropertyToUser(long userPersonaId, ProductEnum productId, long propertyId, int remove = 0)
		{
			using (var repository = GetRepository())
			{
				RepositoryResponse repositoryResponse = new RepositoryResponse();
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = (int)productId,
					PropertyID = propertyId,
					Deleted = remove
				};

				int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
				repositoryResponse.Id = i;

				return repositoryResponse;
			}
		}

		public RepositoryResponse AddUpdatePropertyMapping(long personaId, ProductEnum productId, string propertyJSON)
		{
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			repositoryResponse.Id = 0;

			using (var repository = GetRepository())
			{
				repository.UnitOfWork.BeginTransaction();
				try
				{
					repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdatePropertyMapping, new { personaId, productId, propertyJSON });
					if ((repositoryResponse.Id == 0) && (!string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage)))
					{
						repositoryResponse.ErrorMessage = $"Update Property Mapping Error: {repositoryResponse.ErrorMessage}.";
					}
				}
				catch (Exception exception)
				{
					repositoryResponse.Id = 0;
					repositoryResponse.ErrorMessage = "Update Property MApping Exception: " + exception.Message;
				}
				finally
				{
					if (repositoryResponse.ErrorMessage.Length == 0)
					{
						//Commit and end transaction.
						repository.UnitOfWork.Commit();
					}
					else
					{
						//Rollback transaction and dispose it.
						repository.UnitOfWork.Rollback();
					}
				}
				return repositoryResponse;
			}
		}
	}
}