using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntelligentBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using UserAssignProductPropertyRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntelligentBuilding.UserAssignProductPropertyRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
	public class ManageUPFMProductsIntegration : ManageProductBase, IManageUPFMProductsIntegration
	{
		private DefaultUserClaim _userClaims;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public ManageUPFMProductsIntegration(DefaultUserClaim userClaims) : base(, userClaims, null)
		{
			WriteToDiagnosticLog("ManageUPFMProductsIntegration Ctor - Getting Product settings.");
			_userClaims = userClaims;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_productRepository = new ProductRepository(_userClaims);
			_blueBook = new ManageBlueBook(userClaims);
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="defaultUserClaim"></param>
		/// <param name="managePersona"></param>
		/// <param name="managePerson"></param>
		/// <param name="manageBlueBook"></param>
		/// <param name="productRepository"></param>
		/// <param name="samlRepository"></param>
		/// <param name="productInternalSettingRepository"></param>
		/// <param name="managePartyRelationship"></param>
		/// <param name="userRoleRightRepository"></param>
		/// <param name="manageUserLogin"></param>
		/// <param name="unifiedLoginRepository"></param>
		/// <param name="propertyRepository"></param>
		public ManageUPFMProductsIntegration(DefaultUserClaim defaultUserClaim, IManagePersona managePersona, IManagePerson managePerson, IManageBlueBook manageBlueBook, IProductRepository productRepository, ISamlRepository samlRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, IUserRoleRightRepository userRoleRightRepository, IManageUserLogin manageUserLogin, IUnifiedLoginRepository unifiedLoginRepository, IPropertyRepository propertyRepository, IUserLoginRepository userLoginRepository) : base((int)ProductEnum.IntelligentBuilding, defaultUserClaim, productInternalSettingRepository)
		{
			_userClaims = defaultUserClaim;
			_editorRealPageId = defaultUserClaim.UserRealPageGuid;
			_managePersona = managePersona;
			_blueBook = manageBlueBook;
			_productRepository = productRepository;
			_samlRepository = samlRepository;
			_productInternalSettingRepository = productInternalSettingRepository;
			_managePartyRelationship = managePartyRelationship;
			_userRoleRightRepository = userRoleRightRepository;
			_manageUserLogin = manageUserLogin;
			_unifiedLoginRepository = unifiedLoginRepository;
			_managePerson = managePerson;
			_propertyRepository = propertyRepository;
			_userLoginRepository = userLoginRepository;
		}

		#region Roles and Rights

		/// <summary>
		/// Returns Roles for the given user and company
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="partyId"></param>
		/// <returns></returns>
		public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId)
		{
			WriteToDiagnosticLog($"ManageIntelligentBuildingUser-GetRoles at beginning of method for user with editorPersona id - {editorPersonaId}");

			var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (result.IsError)
				{
					WriteToErrorLog($"ManageIntelligentBuildingUser-GetRoles.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				// get roles from DB for UnifiedAmenities product
				WriteToDiagnosticLog($"ManageIntelligentBuildingUser-GetRoles  Getting all GB roles from GB DB - ocr.ListRolesByParty with party id - {partyId}");
				IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
				var gbAllRoles = _productRepository.ListRolesForProductByParty(partyId, productIdList, _productId) ?? new List<ProductRole>();
				gbAllRoles = gbAllRoles?.OrderBy(r => r.Name).ToList();



				WriteToDiagnosticLog($"ManageIntelligentBuildingUser-GetRoles.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

				if (userPersonaId != 0) // Called during updating Existing User
				{
					WriteToDiagnosticLog($"ManageIntelligentBuildingUser-GetRoles-MergeAccessGroupsWithGreenbook calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
					response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId);
					WriteToDiagnosticLog($"ManageIntelligentBuildingUser-GetRoles-MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
				}
				else // Called during creating a new User
				{
					// For new user, set a default role
					if (gbAllRoles != null)
					{
						if (gbAllRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)))
						{
							gbAllRoles.FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)).IsAssigned = true;
						}
					}
					response = new ListResponse()
					{
						Records = gbAllRoles.Cast<object>().ToList(),
						TotalRows = gbAllRoles.Count(),
						RowsPerPage = 9999,
						ErrorReason = string.Empty,
						TotalPages = 1
					};
				}
			}
			catch (Exception ex)
			{
				response.IsError = true;
				response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
				WriteToErrorLog($"ManageIntelligentBuildingUser-GetRoles Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
			}

			return response;
		}

		/// <summary>
		/// Returns Rights with selected rights for a roleId
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="partyId"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		public ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId)
		{
			WriteToDiagnosticLog($"ManageIntelligentBuildingUser.GetRightsByRole at beginning of method for user with editorPersona id - {editorPersonaId}");

			var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
				if (result.IsError)
				{
					WriteToErrorLog($"ManageIntelligentBuildingUser.GetRightsByRole.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				WriteToDiagnosticLog($"ManageIntelligentBuildingUser.GetRightsByRole Getting all GB roles from GB DB - pr.ListRolesForProductByParty with party id - {partyId}");
				ProductRepository pr = new ProductRepository();
				IList<int> productIdList = pr.GetProductIdsByCompany(partyId);
				var gbAllRights = _unifiedLoginRepository.ListRightsByRole(partyId, productIdList, _productId, roleId) ?? new List<ProductRight>();

				gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

				WriteToDiagnosticLog($"ManageIntelligentBuildingUser.GetRightsByRole.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

				response = new ListResponse()
				{
					Records = gbAllRights.Cast<object>().ToList(),
					TotalRows = gbAllRights.Count(),
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				response.IsError = true;
				response.ErrorReason = CommonMessageConstants.RightErrorMessage;
				WriteToErrorLog($"ManageIntelligentBuildingUser.GetRightsByRole Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
			}

			return response;
		}

		/// <summary>
		/// Get Product Ids by Org
		/// </summary>        
		/// <returns></returns>
		private List<int> GetProductIdsByOrg()
		{
			ProductRepository pr = new ProductRepository();
			IList<int> productList = pr.GetProductIdsByCompany(_userClaims.OrganizationRealPageGuid);

			List<int> productIds = new List<int>();
			foreach (var item in productList)
			{
				productIds.Add(item);
			}
			return productIds;
		}

		/// <summary>
		/// Used to unassign a role assigned to the user
		/// </summary>
		/// <param name="userProductPropertyRole"></param>
		/// <returns></returns>
		private UserAssignProductPropertyRole MapGbObjectToProduct(IBPropertyRole userProductPropertyRole)
		{
			var result = new UserAssignProductPropertyRole();

			if (userProductPropertyRole.RoleList?.Count > 0)
			{
				result.RoleList = new List<string>();
				foreach (var roleId in userProductPropertyRole.RoleList)
				{
					result.RoleList.Add(roleId);
				}
			}

			return result;
		}

		/// <summary>
		/// Used to merge product roles to UnifedLogin roles
		/// </summary>
		/// <param name="allRoles"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		private ListResponse MergeSelRolesWithGreenbook(IList<ProductRole> allRoles, long userPersonaId)
		{

			WriteToDiagnosticLog($"ManageIntelligentBuildingUser.MergeSelRolesWithGreenbook  Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}");
			List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);

			// if a user record exists

			foreach (var role in roleList)
			{
				if (allRoles.Any(a => a.ID == role.RoleID.ToString()))
				{
					ProductRole selrole = (from a in allRoles
										   where a.ID == role.RoleID.ToString()
										   select a).FirstOrDefault();
					if (selrole != null)
					{
						selrole.IsAssigned = true;
					}
				}
			}
			if (allRoles != null)
			{
				if (!allRoles.Any(s => s.IsAssigned == true) && allRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)))
				{
					allRoles.FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)).IsAssigned = true;
				}
			}

			return new ListResponse()
			{
				Records = allRoles.Cast<object>().ToList(),
				TotalRows = allRoles.Count(),
				RowsPerPage = 9999,
				ErrorReason = string.Empty,
				TotalPages = 1
			};
		}

		#endregion
	}
}
