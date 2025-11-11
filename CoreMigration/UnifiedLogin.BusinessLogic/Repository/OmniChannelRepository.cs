using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.OmniChannel;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Omnichannel Repository
	/// </summary>
	public class OmniChannelRepository : BaseRepository
	{
		#region Constructor
		/// <summary>
		/// Omnichannel base Constructor
		/// </summary>
		public OmniChannelRepository() : base(DbConnectionEnum.IdpConfigurationDb)
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
				IList<dynamic> result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesByParty, new { partyId }).ToList();
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
				IList<dynamic> result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListRolesByParty, new { userPersonaId }).ToList();
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
		/// <param name="productId">Product ID</param>   
		/// <returns>List of Properties/Role assigned to Persona</returns>
		public List<PropertyRole> ListPropertyMappingByPersona(long userPersonaId, long productId)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = productId
				};

				List<PropertyRole> propRoleList = new List<PropertyRole>();
				var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPropertyMapping, param);
				if (result != null)
				{
					foreach (var item in result)
					{
						propRoleList.Add(new PropertyRole { RoleID = item.RoleID, PropID = item.PropertyID });
					}
				}
				return propRoleList;
			}
		}

		/// <summary>
		/// List of Roles by User Persona ID
		/// </summary>
		/// <param name="userPersonaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>List of Properties/Role assigned to Persona</returns>
		public List<Property> ListPropByPersona(long userPersonaId, long productId)
		{
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					PersonaID = userPersonaId,
					ProductID = productId
				};

				List<Property> propList = new List<Property>();
				var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPropertyMapping, param);
				if (result != null)
				{
					foreach (var item in result)
					{
						propList.Add(new Property { PropID = item.PropertyID });
					}
				}
				return propList;
			}
		}

		///// <summary>
		///// Insert Property/Role to User
		///// </summary>
		///// <param name="userPersonaId">User Persona ID</param>      
		///// <param name="productId">Product ID</param>      
		///// <param name="property">Property</param>      
		///// <param name="role">User Role</param>   
		///// <param name="del">isDeleted</param>   
		///// <returns>List of Roles assigned to Persona</returns>
		//public RepositoryResponse InsertDelAssignedPropRoleToUser(long userPersonaId, long productId, UserLocation property, UserAccessGroup role, long del = 0)
		//{
		//	using (var repository = GetRepository())
		//	{
		//		RepositoryResponse repositoryResponse = new RepositoryResponse();
		//		dynamic param = new
		//		{
		//			PersonaID = userPersonaId,
		//			ProductID = productId,
		//			RoleID = int.Parse(role.AccessGroupCode),
		//			PropertyID = int.Parse(property.PropertyId),
		//			Deleted = del
		//		};

		//		int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
		//		repositoryResponse.Id = i;

		//		return repositoryResponse;
		//	}
		//}

		/// <summary>
		/// Insert Property/Role to User
		/// </summary>
		/// <param name="userPersonaId">User Persona ID</param>             
		/// <param name="role">User Role</param>   
		/// <param name="del">isDeleted</param>   
		/// <returns>List of Roles assigned to Persona</returns>
		public RepositoryResponse InsertAssignedRoleToUser(long userPersonaId, UserAccessGroup role, long del = 0)
		{
			using (var repository = GetRepository())
			{
				RepositoryResponse repositoryResponse = new RepositoryResponse();
				dynamic param = new
				{
					PersonaID = userPersonaId,
					RoleID = int.Parse(role.AccessGroupCode)
				};

				int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_LinkPersonaToRole, param);
				repositoryResponse.Id = i;

				return repositoryResponse;
			}
		}

		///// <summary>
		///// Insert Property/Role to User
		///// </summary>
		///// <param name="userPersonaId">User Persona ID</param>      
		///// <param name="productId">Product ID</param>      
		///// <param name="propertyId">Property ID</param>      
		///// <param name="roleId">User Role ID</param>   
		///// <param name="del">isDeleted</param>   
		///// <returns>List of Roles assigned to Persona</returns>
		//public RepositoryResponse InsertDelAssignedPropRoleToUserNew(long userPersonaId, long productId, long propertyId, long roleId, long del = 0)
		//{
		//	using (var repository = GetRepository())
		//	{
		//		RepositoryResponse repositoryResponse = new RepositoryResponse();
		//		dynamic param = new
		//		{
		//			PersonaID = userPersonaId,
		//			ProductID = productId,
		//			RoleID = roleId,
		//			PropertyID = propertyId,
		//			Deleted = del
		//		};

		//		int i = repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreatePropertyMapping, param);
		//		repositoryResponse.Id = i;

		//		return repositoryResponse;
		//	}
		//}



		#endregion
	}
}