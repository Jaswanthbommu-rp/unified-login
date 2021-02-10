using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using DbConnectionEnum = RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Enum.DbConnectionEnum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Repository
{
    public class UserRoleRepository : BaseRepository, IUserRoleRepository
	{
		IList<ProductRole> _productRoleList;
		IList<ProductRight> _productRightsList;

		#region Constructor

		/// <summary>
		/// UserLoginRepository Constructor
		/// </summary>
		public UserRoleRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
			_productRoleList = new List<ProductRole>();
			_productRightsList = new List<ProductRight>();
		}

		#endregion

		/// <summary>
		/// Get list of Roles by User Persona ID and product
		/// </summary>
		/// <param name="personaId">Persona ID</param>   
		/// <param name="productId">Product ID</param>   
		/// <returns>Role assigned to Persona</returns>
		public IList<ProductRole> GetProductRolesByPersona(long personaId, ProductEnum productId)
		{
			List<dynamic> result;
			var productRepository = new ProductInternalSettingRepository();
			List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>();
			productInternalSettings = productRepository.GetProductSettings((int)ProductEnum.UnifiedPlatform).ToList();
			var procName = StoredProcNameConstants.SP_ListRolesForProductsByPersonaId;

			dynamic param = new
			{
				PersonaID = personaId,
				ProductID = (int)productId
			};
			using (var repository = GetRepository())
			{
				result = repository.GetMany<dynamic>(procName, param);
			}
			if (result.Count > 0)
			{
				foreach (dynamic d in result)
				{
					ProductRole p = new ProductRole()
					{
						ID = Convert.ToString(d.RoleId),
						Name = Convert.ToString(d.Role),
						Roletype = Convert.ToString(d.RoleType),
						Alias = Convert.ToString(d.RoleNickName),
						IsAssigned = true
					};
					_productRoleList.Add(p);
				}
			}
			return _productRoleList;
		}

		/// <summary>
		/// Get list of Rights id by Party, product id and role id
		/// </summary>
		/// <param name="partyId">Company party id</param>
		/// <param name="productIdList">Company product id list</param>
		/// <param name="productId">Product Id</param>
		/// <param name="roleId">Role Id</param>
		/// <returns>The list of rights for the given role</returns>
		public IList<ProductRight> ListRightsByRole(long partyId, IList<int> productIdList, ProductEnum productId, long roleId)
		{
			List<dynamic> result;
			var productRepository = new ProductInternalSettingRepository();
			List<ProductInternalSetting> productInternalSettings = new List<ProductInternalSetting>();
			productInternalSettings = productRepository.GetProductSettings((int)ProductEnum.UnifiedPlatform).ToList();
			var procName = StoredProcNameConstants.SP_ListRolesAssociatedWithRights;

			dynamic param = new
			{
				PartyId = partyId,
				ProductId = (int)productId,
				RoleId = roleId,
				TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
			};

			using (var repository = GetRepository())
			{
				result = repository.GetMany<dynamic>(procName, param);
			}
			
			if (result != null)
			{
				foreach (var item in result)
				{
					_productRightsList.Add(new ProductRight { ID = item.RightValueTypeId, Description = item.Right, Alias = item.RightNickName, Assigned = true }); // RightValueTypeId instead of RightId
				}
			}
			return _productRightsList;
		}
	}
}
