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
	public class ManageIntelligentBuilding : ManageProductBase, IManageIntelligentBuilding
	{
		private DefaultUserClaim _userClaims;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public ManageIntelligentBuilding(DefaultUserClaim userClaims) : base((int)ProductEnum.IntelligentBuildingTrash, userClaims, null, null)
		{
			WriteToDiagnosticLog("Ctor - Getting Product settings.");
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
		public ManageIntelligentBuilding(DefaultUserClaim defaultUserClaim, IManagePersona managePersona, IManagePerson managePerson, IManageBlueBook manageBlueBook, IProductRepository productRepository, ISamlRepository samlRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, IUserRoleRightRepository userRoleRightRepository, IManageUserLogin manageUserLogin, IUnifiedLoginRepository unifiedLoginRepository, IPropertyRepository propertyRepository, IUserLoginRepository userLoginRepository) : base((int)ProductEnum.IntelligentBuildingTrash, defaultUserClaim, productInternalSettingRepository, productRepository)
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


		/// <summary>
		/// Used to create/update a user in Unifed Amenities
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		public string ManageIntelligentBuildingUser(long editorPersonaId, long userPersonaId, IBPropertyRole userAssignProductPropertyRole)
		{
			WriteToDiagnosticLog($"ManageIntelligentBuildingUser - Begin create/update user for user with userPersonaId id - {userPersonaId}.");
			try
			{
				var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					WriteToErrorLog($"ManageIntelligentBuildingUser Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}");
					return listResponse.ErrorReason;
				}


				var userPersona = _managePersona.GetPersona(userPersonaId);
				var realPageId = userPersona.RealPageId;
				var person = _managePerson.GetPerson(realPageId);
				var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
				var productInternalSettingList = GetProductSetting((int)ProductEnum.UnifiedPlatform);
				var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, ProductEnum.IntelligentBuildingTrash);

				// super user
				// TODO what to do here?
				if (IsSuperUser(userPersonaId))
				{
					WriteToDiagnosticLog($"ManageIntelligentBuildingUser - new user is Super user with userPersonaId id - {userPersonaId}.");
					IList<int> productIdList = _productRepository.GetProductIdsByCompany(userPersona.OrganizationPartyId);
					var gbAllRoles = _productRepository.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId) ?? new List<ProductRole>();
					string superUserRoleId = gbAllRoles.First(a => a.Name.Equals("Portfolio Manager", StringComparison.OrdinalIgnoreCase)).ID;
					List<string> propertiesToRemove = new List<string>();
					if (userPropertyIdList?.Count > 0)
					{
						foreach (var prop in userPropertyIdList)
						{
							if (prop != -1)
							{
								propertiesToRemove.Add(prop.ToString());
							}
						}
					}

					userAssignProductPropertyRole = new IBPropertyRole
					{
						PropertyList = new List<string> { "-1" },
						RemovedPropertyList = propertiesToRemove,
						RoleList = new List<string>() { superUserRoleId }
					};
				}

				var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;

				// Check for user role
				UL.Role role = new UL.Role();

				if (userAssignProductPropertyRole != null)
				{
					RepositoryResponse result;

					// map userAssignProductPropertyRole to ProductPropertyRole
					var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);
					long existingRoleId = 0;

					if (productPropertyRole.RoleList?.Count > 0)
					{
						role.RoleID = long.Parse(productPropertyRole.RoleList[0]);

						List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);

						if (roleList?.Count > 0) // Existing user
						{
							existingRoleId = roleList[0].RoleID;
						}

						if (role.RoleID != existingRoleId)
						{
							if (existingRoleId != 0)
							{
								// remove the existing role
								WriteToDiagnosticLog($"ManageIntelligentBuildingUser - removing role for user userPersonaId id - {userPersonaId}, RoleId - {existingRoleId}.");
								result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: existingRoleId, userId: _userClaims.UserId, deleteRole: true);
								if (result.Id < 0)
								{
									WriteToErrorLog($"ManageIntelligentBuildingUser - Unable to delete role for user with userPersonaId - {userPersonaId}, RoleId - {existingRoleId}");
									return result.ErrorMessage;
								}
							}

							if (role.RoleID != 0)
							{
								// add the role
								WriteToDiagnosticLog($"ManageIntelligentBuildingUser - adding role for userPersonaId id - {userPersonaId}, RoleId - {role.RoleID}.");
								result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: role.RoleID, userId: _userClaims.UserId, deleteRole: false);
								if (result.Id < 0)
								{
									WriteToErrorLog($"ManageIntelligentBuildingUser - Unable to add role for user with userPersonaId - {userPersonaId}, RoleId - {role.RoleID}");
									return result.ErrorMessage;
								}
							}
						}
					}

					if (userAssignProductPropertyRole.PropertyList != null && userAssignProductPropertyRole.PropertyList.Count > 0 && userAssignProductPropertyRole.PropertyList[0].ToUpper() == "ALL")
					{
						IList<int> productIdList = _productRepository.GetProductIdsByCompany(userPersona.OrganizationPartyId);
						var gbAllRoles = _productRepository.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId) ?? new List<ProductRole>();
						if (gbAllRoles != null)
						{
							role.RoleID = long.Parse(productPropertyRole.RoleList[0]);
							if (gbAllRoles.Any(r => long.Parse(r.ID) == role.RoleID && (r.accessAllProperties)))
							{
								userAssignProductPropertyRole.PropertyList = new List<string> { "-1" };
							}
						}

					}

					List<string> assignedPropertyList = userAssignProductPropertyRole.PropertyList;
					List<string> unAssignedPropertyList = userAssignProductPropertyRole.RemovedPropertyList;

					List<string> unassignedProperties = new List<string>();
					List<string> assignedProperties = new List<string>();

					if (assignedPropertyList != null)
					{
						foreach (string propertyId in assignedPropertyList)
						{
							if (userPropertyIdList.All(p => p != Convert.ToInt32(propertyId)))
							{
								// new property to be added
								assignedProperties.Add(propertyId);
							}
						}
					}

					if (unAssignedPropertyList != null)
					{
						foreach (string propertyId in unAssignedPropertyList)
						{
							// remove property
							unassignedProperties.Add(propertyId);
						}
					}

					if ((unAssignedPropertyList == null || unAssignedPropertyList?.Count == 0) && assignedProperties?.Count > 0)
					{
						if (userPropertyIdList.Any(p => p == -1))
						{
							unassignedProperties.Add("-1");
						}
					}

					if (unassignedProperties.Count > 0)
					{
						Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyInstanceData(userPersonaId, ProductEnum.IntelligentBuildingTrash, Convert.ToInt64(property)); });
					}

					if (assignedProperties.Count > 0)
					{
						Parallel.ForEach(assignedProperties, property => { result = InsertAssignedPropertyInstanceData(userPersonaId, ProductEnum.IntelligentBuildingTrash, Convert.ToInt64(property)); });
					}
				}

				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

				// add activity log
				WriteUserActivityLogWithMessage(editorPersonaId, person, userLogin, "Access is granted for user {0} {1} in product {2} by user {3} {4}.");

				return string.Empty;
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageIntelligentBuildingUser - Error for user with userPersonaId id - {userPersonaId}", exception: ex);
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
				return $"Error - {ex.Message}";
			}
		}

		/// <summary>
		/// Unassign User
		/// </summary> 
		public string UnassignUser(long editorPersonaId, long userPersonaId, IBPropertyRole userAssignProductPropertyRole)
		{
			var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError)
			{
				WriteToErrorLog(
					$"ManageIntelligentBuilding-UnassignUser - Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
				return listResponse.ErrorReason;
			}


			List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);
			if (roleList?.Count > 0)
			{
				long roleId = roleList[0].RoleID;
				// Delete existing roleId
				RepositoryResponse result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: roleId, userId: _userClaims.UserId, deleteRole: true);
				if (result.Id < 0)
				{
					WriteToErrorLog($"ManageIntelligentBuildingUser-UnassignUser - Unable to delete record for user with userPersonaId - {userPersonaId}, RoleId - {roleId}");
					return result.ErrorMessage;
				}

				List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, ProductEnum.IntelligentBuildingTrash);
				List<string> unassignedProperties = new List<string>();

				foreach (var property in propertyList)
				{
					unassignedProperties.Add(property.ID.ToString());
				}

				if (unassignedProperties.Count > 0)
				{
					Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyInstanceData(userPersonaId, ProductEnum.IntelligentBuildingTrash, Convert.ToInt64(property)); });
				}
			}

			WriteToInformationLog($"ManageIntelligentBuildingUser-UnassignUser userPersonaId:{userPersonaId}");
			UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

			// Activity Logging
			WriteUnassignActivityLog(editorPersonaId, userPersonaId);

			return string.Empty;
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

		#region Property
		/// <summary>
		/// Get the list of property instances for the given user to be used by external systems
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="include"></param>
		/// <returns></returns>
		public ListResponse GetUPFMProperties(long userPersonaId, string include = null)
		{
			ListResponse response = new ListResponse();
			var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, ProductEnum.IntelligentBuildingTrash);
			List<ProductProperty> userPropertyList = new List<ProductProperty>();
			List<ProductProperty> translatedUserPropertyList = new List<ProductProperty>();
			List<UPFMPropertyInstance> customerPropertyList = new List<UPFMPropertyInstance>();

			if (userPropertyIdList != null)
			{
				var booksPropertyList = _blueBook.GetUPFMPropertyInstances(_userClaims.OrganizationRealPageGuid.ToString());
				if (booksPropertyList != null)
				{
					customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);
				}

				if (userPropertyIdList.Count == 1 && userPropertyIdList[0] == -1)
				{
					customerPropertyList.ForEach(cp =>
					{
						userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true));
					});
				}
				else
				{
					customerPropertyList.ToList().FindAll(b => userPropertyIdList.Any(p => p == b.PropertyInstanceId)).ForEach(cp => { userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true)); });
				}
			}


			if (userPropertyIdList?.Count > 0)
			{
				// call translate with upfm properties to get ib properety id and merges propertyinstanceid with translated id
				//note save upfmid into alias field before updating with translated id
				UPFMProperty upfmProperties = new UPFMProperty();
				List<string> instanceids = new List<string>();
				foreach (var property in userPropertyList)
				{
					instanceids.Add(property.InstanceId.ToLower());
				}
				upfmProperties.id = instanceids;

				var translatedData = _blueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, ProductEnum.IntelligentBuildingTrash.ToEnumDescription());
				if (translatedData != null)
				{
					foreach (var attributs in translatedData.Data.Attributes)
					{
						foreach (var propertyData in attributs.TranslatedPropertyInstances)
						{
							if (propertyData.Source == ProductEnum.IntelligentBuildingTrash.ToEnumDescription())
							{
								var translatedProductProperty = userPropertyList.FirstOrDefault(u => u.InstanceId == attributs.PropertyInstanceSourceId);
								if (translatedProductProperty != null)
								{
									translatedProductProperty.ID = propertyData.PropertyInstanceSourceId;
									translatedProductProperty.Alias = null;
									translatedUserPropertyList.Add(translatedProductProperty);
								}
							}
						}
					}
				}

				bool bIncludeFields = (!string.IsNullOrWhiteSpace(include) && include.Split(new char[] { ',' }).Length > 0);

				if (bIncludeFields)
				{
					DynamicContractResolver dynamicContractResolver = new DynamicContractResolver(include);
					string productPropertySerializableProperties = JsonConvert.SerializeObject(
						translatedUserPropertyList,
						new JsonSerializerSettings()
						{
							ContractResolver = dynamicContractResolver
						}
					);
					translatedUserPropertyList = JsonConvert.DeserializeObject<List<ProductProperty>>(productPropertySerializableProperties);
				}

				translatedUserPropertyList.ForEach(p =>
				{
					p.IsAssigned = null;
					p.disableSelection = null;
				});

				response.IsError = false;
				response.Records = translatedUserPropertyList.Cast<object>().ToList();
				response.TotalRows = translatedUserPropertyList.Count;
				response.RowsPerPage = translatedUserPropertyList.Count;
				response.TotalPages = 1;
				response.ErrorReason = string.Empty;
			}

			return response;
		}

		/// <summary>
		/// Get a list of UPFM property instances for the give user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="product"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetUPFMProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, ProductEnum product, RequestParameter datafilter)
		{
			ListResponse result = new ListResponse();
			WriteToDiagnosticLog($"ManageUnifiedLogin.GetUPFMProperties - at beginning of method for user with editorPersona id - {editorPersonaId}");

			try
			{
				result = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
				if (result.IsError)
				{
					WriteToErrorLog($"ManageUnifiedLogin.GetUPFMProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				//var booksPropertyList = _blueBook.GetUPFMPropertyInstances(_userClaims.OrganizationRealPageGuid.ToString());

				var booksPropertyList = _blueBook.GetPropertiesPerProductCenter(_userClaims.OrganizationRealPageGuid.ToString(), product);
				var customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);

				WriteToDiagnosticLog($"ManageUnifiedLogin.ListUPFMPropertyInstanceIdByInstanceIds() completed for user with editorPersona id -{editorPersonaId}.");
				WriteToDiagnosticLog($"GetProperties- calling MergeUPFMBooksPropertiesWithUPFMProperties....for user with editorPersona id -{editorPersonaId} & userPersonaId-{userPersonaId}.");
				result = MergeUPFMBooksPropertiesWithProductProperty(customerPropertyList, userPersonaId, assignedOnly);
				WriteToDiagnosticLog($"GetProperties-MergeUPFMBooksPropertiesWithUPFMProperties completed for user with editorPersona id -{editorPersonaId}.");

			}
			catch (Exception ex)
			{
				result.IsError = true;
				result.ErrorReason = $"ManageProductProspectContact.GetProperties - There was a problem getting the properties.";
				WriteToErrorLog($"ManageProductProspectContact.GetProperties - There was a problem getting the properties for user with editorPersona id - {editorPersonaId}.",
					exception: ex);
			}

			return result;
		}


		/// <summary>
		/// Used to convert a UPFM property instance to a Product Property 
		/// </summary>
		/// <param name="upfmPropertyInstance"></param>
		/// <param name="isAssigned"></param>
		/// <returns></returns>
		private static ProductProperty ConvertUPFMPropertyInstanceToProductProperty(UPFMPropertyInstance upfmPropertyInstance, bool isAssigned)
		{
			ProductProperty pp = new ProductProperty()
			{
				ID = upfmPropertyInstance.CustomerPropertyId.ToString(),
				Name = upfmPropertyInstance.Name,
				Street1 = upfmPropertyInstance.Address,
				City = upfmPropertyInstance.City,
				State = upfmPropertyInstance.State,
				Zip = upfmPropertyInstance.PostalCode,
				IsAssigned = isAssigned,
				InstanceId = upfmPropertyInstance.InstanceId.ToString(),
				Latitude = upfmPropertyInstance.Latitude,
				Longitude = upfmPropertyInstance.Longitude,
				Alias = upfmPropertyInstance.PropertyInstanceId.ToString(),
				CustomerPropertyId = upfmPropertyInstance.CustomerPropertyId
			};
			return pp;
		}



		/// <summary>
		/// Used to unassign a property instance to the given user
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <param name="propertyInstanceId"></param>
		/// <returns></returns>
		private RepositoryResponse DeleteAssignedPropertyInstanceData(long userPersonaId, ProductEnum productId, long propertyInstanceId)
		{
			return DeleteAssignedUserPropertyInstanceData(userPersonaId, ProductEnum.IntelligentBuildingTrash, propertyInstanceId);
		}
		/// <summary>
		/// Used to assign a property instance to the given user
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <param name="propertyInstanceId"></param>
		/// <returns></returns>
		private RepositoryResponse InsertAssignedPropertyInstanceData(long userPersonaId, ProductEnum productId, long propertyInstanceId)
		{
			return InsertAssignedUserPropertyInstanceData(userPersonaId, ProductEnum.IntelligentBuildingTrash, propertyInstanceId);
		}

		/// <summary>
		/// Used to get the list of UPFM property instances for the given personaid
		/// </summary>
		/// <param name="blueBookUPFMPropertyList"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <returns>A list of product properties</returns>
		private ListResponse MergeUPFMBooksPropertiesWithProductProperty(IList<UPFMPropertyInstance> blueBookUPFMPropertyList, long userPersonaId, bool assignedOnly)
		{
			// merge the given user details with the list
			var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, ProductEnum.IntelligentBuildingTrash);

			var propertyOption = new Dictionary<string, bool>();

			propertyOption.Add("allProperties", userPropertyIdList.Any(pl => pl == -1)); // Single Property
			List<ProductProperty> productPropertyList = new List<ProductProperty>();

			foreach (UPFMPropertyInstance upfmPropertyInstance in blueBookUPFMPropertyList)
			{
				var pp = ConvertUPFMPropertyInstanceToProductProperty(upfmPropertyInstance, false);

				if (userPropertyIdList.Any(propertyId => propertyId == upfmPropertyInstance.PropertyInstanceId))
				{
					pp.IsAssigned = true;
				}

				productPropertyList.Add(pp);
			}

			if (assignedOnly)
			{
				productPropertyList = productPropertyList.Where(a => a.IsAssigned == true).ToList();
			}

			return new ListResponse()
			{
				Records = productPropertyList.Cast<object>().ToList(),
				TotalRows = productPropertyList.Count(),
				RowsPerPage = 9999,
				ErrorReason = string.Empty,
				TotalPages = 1,
				Additional = propertyOption
			};
		}
		#endregion
	}
}
