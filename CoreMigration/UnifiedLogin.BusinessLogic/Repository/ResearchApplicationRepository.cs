using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResearchApplication;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// ResearchApplication  Repository
	/// </summary>
	public class ResearchApplicationRepository : BaseRepository
	{
		#region Constructor
		/// <summary>
		/// ResearchApplication base Constructor
		/// </summary>
		public ResearchApplicationRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}
		#endregion

		#region public Roles methods


		/// <summary>
		/// List of Roles by Party ID
		/// </summary>
		/// <param name="partyId">Party ID</param>        
		/// <returns>List of Roles</returns>
		public List<ProductRole> ListRolesByParty(long partyId)
		{
			using (var repository = GetRepository())
			{
				List<ProductRole> rolesList = new List<ProductRole>();
				var procName = StoredProcNameConstants.SP_ListRolesByParty;
				IList<dynamic> result = repository.GetMany<dynamic>(procName, new { partyId }).ToList();
				if (result != null)
				{
					foreach (var item in result)
					{
						rolesList.Add(new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false });
					}
				}
				return rolesList;
			}
		}





		/// <summary>
		/// List of Roles by User Persona ID
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>        
		/// <returns>List of Roles assigned to Persona</returns>
		public List<ProductRole> ListRolesAssignedToPersona(long userPersonaId)
		{
			using (var repository = GetRepository())
			{
				List<ProductRole> rolesList = new List<ProductRole>();
				var procName = StoredProcNameConstants.SP_ListRolesByParty;
				IList<dynamic> result = repository.GetMany<dynamic>(procName, new { userPersonaId }).ToList();
				if (result != null)
				{
					foreach (var item in result)
					{
						rolesList.Add(new ProductRole { ID = item.RoleID.ToString(), Name = item.Value, IsAssigned = false });
					}
				}
				return rolesList;
			}
		}


		/// <summary>
		/// Insert Property/Role to User
		/// </summary>
		/// <param name="userPersonaId">User Persona ID</param>      
		/// <param name="productId">Product ID</param>      
		/// <param name="property">Property</param>      
		/// <param name="role">User Role</param>   
		/// <param name="del">isDeleted</param>   
		/// <returns>List of Roles assigned to Persona</returns>
		public RepositoryResponse InsertDelAssignedPropRoleToUser(long userPersonaId, long productId, UserLocation property, UserAccessGroup role, long del = 0)
		{
			using (var repository = GetRepository())
			{
				RepositoryResponse repositoryResponse = new RepositoryResponse();
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = productId,
					RoleID = int.Parse(role.AccessGroupCode),
					PropertyID = int.Parse(property.PropertyId),
					Deleted = del
				};

				int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
				repositoryResponse.Id = i;

				return repositoryResponse;
			}
		}

		/// <summary>
		/// Insert Property/Role to User
		/// </summary>
		/// <param name="userPersonaId">User Persona ID</param>      
		/// <param name="productId">Product ID</param>      
		/// <param name="propertyId">Property ID</param>      
		/// <param name="roleId">User Role ID</param>   
		/// <param name="del">isDeleted</param>   
		/// <returns>List of Roles assigned to Persona</returns>
		public RepositoryResponse InsertDelAssignedPropRoleToUserNew(long userPersonaId, int productId, long propertyId, long roleId, long del = 0)
		{
			using (var repository = GetRepository())
			{
				RepositoryResponse repositoryResponse = new RepositoryResponse();
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = productId,
					RoleID = roleId,
					PropertyID = propertyId,
					Deleted = del
				};

				int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
				repositoryResponse.Id = i;

				return repositoryResponse;
			}
		}

		#endregion
		#region Private Methods
		private string getRoleRightsSchemaName()
		{
			IProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository();
			var productInternalSettingList = productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
			return productInternalSettingList.FirstOrDefault(s => s.Name.Equals("RolesRightsSchemaName", StringComparison.OrdinalIgnoreCase))?.Value;
		}
		#endregion
	}
}