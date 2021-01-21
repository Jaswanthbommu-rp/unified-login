using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
//using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntelligentBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
//using UserAssignProductPropertyRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.IntelligentBuilding.UserAssignProductPropertyRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
	public class ManageUPFMProductsIntegration : ManageProductBase, IManageUPFMProductsIntegration
	{
		private DefaultUserClaim _userClaims;
		public int _upfmProductId;

		//private int _upfmProductId;
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public ManageUPFMProductsIntegration(int productId, DefaultUserClaim userClaims) : base(productId, userClaims, null, null)
		{
			WriteToDiagnosticLog("ManageUPFMProductsIntegration Ctor - Getting Product settings.");
			_userClaims = userClaims;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_productRepository = new ProductRepository(_userClaims);
			_blueBook = new ManageBlueBook(userClaims);
			_upfmProductId = productId;
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
		public ManageUPFMProductsIntegration(int productId, DefaultUserClaim defaultUserClaim, IManagePersona managePersona, IManagePerson managePerson, IManageBlueBook manageBlueBook, IProductRepository productRepository, ISamlRepository samlRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, IUserRoleRightRepository userRoleRightRepository, IManageUserLogin manageUserLogin, IUnifiedLoginRepository unifiedLoginRepository, IPropertyRepository propertyRepository, IUserLoginRepository userLoginRepository) : base(productId, defaultUserClaim, productInternalSettingRepository, productRepository)
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
			_upfmProductId = productId;
		}

		#region Roles and Rights

		/// <summary>
		/// Returns Roles for the given user and company
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="partyId"></param>
		/// <returns></returns>
		public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId, ProductEnum product)
		{
			WriteToDiagnosticLog($"ManageUPFMProductUser - GetRoles at beginning of method for user with editorPersona id - {editorPersonaId}");
			int upfmProductId = (int) product;
		   var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (result.IsError)
				{
					WriteToErrorLog($"ManageUPFMProductUser-GetRoles.GetCompanyEditorAndUserDetails error for product {upfmProductId}  with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				// get roles from DB for UnifiedAmenities product
				WriteToDiagnosticLog($"ManageUPFMProductUser -GetRoles  Getting all GB roles from GB DB - ocr.ListRolesByParty with party id - {partyId} and product {upfmProductId}");
				IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
				var gbAllRoles = _productRepository.ListRolesForProductByParty(partyId, productIdList, upfmProductId) ?? new List<ProductRole>();
				gbAllRoles = gbAllRoles?.OrderBy(r => r.Name).ToList();



				WriteToDiagnosticLog($"ManageUPFMProductUser-GetRoles.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

				if (userPersonaId != 0) // Called during updating Existing User
				{
					WriteToDiagnosticLog($"ManageUPFMProductUser-GetRoles-MergeAccessGroupsWithGreenbook calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
					response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId);
					WriteToDiagnosticLog($"ManageUPFMProductUser-GetRoles-MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
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
				WriteToErrorLog($"ManageUPFMProductUser -GetRoles Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
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
		public ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId, ProductEnum product)
		{
			WriteToDiagnosticLog($"ManageUPFMProductUser.GetRightsByRole at beginning of method for user with editorPersona id - {editorPersonaId}");
			int upfmProductId = (int)product;
			var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
				if (result.IsError)
				{
					WriteToErrorLog($"ManageUPFMProductUser.GetRightsByRole.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				WriteToDiagnosticLog($"ManageUPFMProductUser.GetRightsByRole Getting all GB roles from GB DB - pr.ListRolesForProductByParty with party id - {partyId}");
				ProductRepository pr = new ProductRepository();
				IList<int> productIdList = pr.GetProductIdsByCompany(partyId);
				var gbAllRights = _unifiedLoginRepository.ListRightsByRole(partyId, productIdList, upfmProductId, roleId) ?? new List<ProductRight>();

				gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

				WriteToDiagnosticLog($"ManageUPFMProductUser.GetRightsByRole.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

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
				WriteToErrorLog($"ManageUPFMProductUser.GetRightsByRole Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
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
		private UPFMProductPropertyRole MapGbObjectToProduct(UPFMProductPropertyRole userProductPropertyRole)
		{
			var result = new UPFMProductPropertyRole();

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

			WriteToDiagnosticLog($"ManageUPFMProductUser.MergeSelRolesWithGreenbook  Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}");
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
		/// <param name="product"></param>
		/// <param name="include"></param>
		/// <param name="isMultiCompany"></param>
		/// <param name="multiCompanyRealPageId"></param>
		/// <returns></returns>
		public ListResponse GetUPFMProperties(long userPersonaId, ProductEnum product, string include = null, bool isMultiCompany = false, string multiCompanyRealPageId = null)
		{
			ListResponse response = new ListResponse();
			var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, _upfmProductId);
			List<ProductProperty> userPropertyList = new List<ProductProperty>();
			List<ProductProperty> translatedUserPropertyList = new List<ProductProperty>();
			List<UPFMPropertyInstance> customerPropertyList = new List<UPFMPropertyInstance>();
			int upfmProductId = (int)product;

			if (userPropertyIdList != null)
			{
				var organizationRealPageId = isMultiCompany ? multiCompanyRealPageId : _userClaims.OrganizationRealPageGuid.ToString();
				var booksPropertyList = _blueBook.GetUPFMPropertyInstances(organizationRealPageId);				
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
				var booksProductDetail = _productRepository.GetBooksMasterProductDetail(upfmProductId);
				foreach (var property in userPropertyList)
				{
					instanceids.Add(property.InstanceId.ToLower());
				}
				upfmProperties.id = instanceids;

				var translatedData = _blueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, _udmSourceCode);
				if (translatedData?.Data != null)
				{
					var productCode = booksProductDetail.UDMSourceCode == null ? booksProductDetail.BooksProductCode : booksProductDetail.UDMSourceCode;
					foreach (var attributs in translatedData.Data.Attributes)
					{
						foreach (var propertyData in attributs.TranslatedPropertyInstances)
						{
							if (propertyData.Source == productCode)
							{
								var translatedProductProperty = userPropertyList.FirstOrDefault(u => u.InstanceId == attributs.PropertyInstanceSourceId);
								if (translatedProductProperty != null)
								{
									translatedProductProperty.ID = propertyData.PropertyInstanceSourceId;
									translatedProductProperty.Alias = null;
									translatedProductProperty.CustomerPropertyId = propertyData.CustomerPropertyId;									
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
			WriteToDiagnosticLog($"ManageUPFMProductUser.GetUPFMProperties - at beginning of method for user with editorPersona id - {editorPersonaId} - for product {product}");

			try
			{
				result = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
				if (result.IsError)
				{
					WriteToErrorLog($"ManageUPFMProductUser.GetUPFMProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				//var booksPropertyList = _blueBook.GetUPFMPropertyInstances(_userClaims.OrganizationRealPageGuid.ToString());

				var booksPropertyList = _blueBook.GetPropertiesPerProductCenter(_userClaims.OrganizationRealPageGuid.ToString(), product);
				var customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);

				WriteToDiagnosticLog($"ManageUPFMProductUser.ListUPFMPropertyInstanceIdByInstanceIds() completed for user with editorPersona id -{editorPersonaId}.");
				WriteToDiagnosticLog($"ManageUPFMProductUser.GetProperties- calling MergeUPFMBooksPropertiesWithUPFMProperties....for user with editorPersona id -{editorPersonaId} & userPersonaId-{userPersonaId}.");
				result = MergeUPFMBooksPropertiesWithProductProperty(customerPropertyList, userPersonaId, assignedOnly);
				WriteToDiagnosticLog($"ManageUPFMProductUser.GetProperties-MergeUPFMBooksPropertiesWithUPFMProperties completed for user with editorPersona id -{editorPersonaId}.");

			}
			catch (Exception ex)
			{
				result.IsError = true;
				result.ErrorReason = $"ManageUPFMProductUser.GetProperties - There was a problem getting the properties.";
				WriteToErrorLog($"ManageUPFMProductUser.GetProperties - There was a problem getting the properties for user with editorPersona id - {editorPersonaId}.",
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
				Alias = upfmPropertyInstance.PropertyInstanceId.ToString()
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
		private RepositoryResponse DeleteAssignedPropertyInstanceData(long userPersonaId, ProductEnum product, long propertyInstanceId)
		{
			return DeleteAssignedUserPropertyInstanceData(userPersonaId, product, propertyInstanceId);
		}
		/// <summary>
		/// Used to assign a property instance to the given user
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <param name="propertyInstanceId"></param>
		/// <returns></returns>
		private RepositoryResponse InsertAssignedPropertyInstanceData(long userPersonaId, ProductEnum product, long propertyInstanceId)
		{
			return InsertAssignedUserPropertyInstanceData(userPersonaId, product, propertyInstanceId);
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
			var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, _upfmProductId);

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
		/// <summary>
		/// Used to create/update a user in Unifed Amenities
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		public string ManageUPFMProductUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole, ProductEnum product)
		{
			WriteToDiagnosticLog($"ManageUPFMProductUser - Begin create/update user for user with userPersonaId id - {userPersonaId}.");
			try
			{
				var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					WriteToErrorLog($"ManageUPFMProductUser Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}");
					return listResponse.ErrorReason;
				}


				var userPersona = _managePersona.GetPersona(userPersonaId);
				var realPageId = userPersona.RealPageId;
				var person = _managePerson.GetPerson(realPageId);
				var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
				var productInternalSettingList = GetProductSetting((int)ProductEnum.UnifiedPlatform);
				var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, _upfmProductId);

				// super user
				// TODO what to do here?
				if (IsSuperUser(userPersonaId))
				{
					WriteToDiagnosticLog($"ManageUPFMProductUser - new user is Super user with userPersonaId id - {userPersonaId}.");
					IList<int> productIdList = _productRepository.GetProductIdsByCompany(userPersona.OrganizationPartyId);
					var gbAllRoles = _productRepository.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId) ?? new List<ProductRole>();
					string superUserRoleId;
					if (_productId == (int)ProductEnum.HospitalityService)
					{
						superUserRoleId = gbAllRoles.First(a => a.Name.Equals("Property Admin", StringComparison.OrdinalIgnoreCase)).ID;
					}
					else if(_productId == (int)ProductEnum.HOTS)
                    {
						superUserRoleId = gbAllRoles.First(a => a.Name.Equals("Creator", StringComparison.OrdinalIgnoreCase)).ID;
					}
					else
					{
						superUserRoleId = gbAllRoles.First(a => a.Name.Equals("Portfolio Manager", StringComparison.OrdinalIgnoreCase)).ID;
					}
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

					userAssignProductPropertyRole = new UPFMProductPropertyRole
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
								WriteToDiagnosticLog($"ManageUPFMProductUser - removing role for user userPersonaId id - {userPersonaId}, RoleId - {existingRoleId}.");
								result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: existingRoleId, userId: _userClaims.UserId, deleteRole: true);
								if (result.Id < 0)
								{
									WriteToErrorLog($"ManageUPFMProductUser - Unable to delete role for user with userPersonaId - {userPersonaId}, RoleId - {existingRoleId}");
									return result.ErrorMessage;
								}
							}

							if (role.RoleID != 0)
							{
								// add the role
								WriteToDiagnosticLog($"ManageUPFMProductUser - adding role for userPersonaId id - {userPersonaId}, RoleId - {role.RoleID}.");
								result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: role.RoleID, userId: _userClaims.UserId, deleteRole: false);
								if (result.Id < 0)
								{
									WriteToErrorLog($"ManageUPFMProductUser - Unable to add role for user with userPersonaId - {userPersonaId}, RoleId - {role.RoleID}");
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

					//var product = ProductEnumHelper.GetUPFMProductEnum(_upfmProductId);

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
						Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyInstanceData(userPersonaId, product, Convert.ToInt64(property)); });
					}

					if (assignedProperties.Count > 0)
					{
						Parallel.ForEach(assignedProperties, property => { result = InsertAssignedPropertyInstanceData(userPersonaId, product, Convert.ToInt64(property)); });
					}
				}

				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

				// add activity log
				WriteUserActivityLogWithMessage(editorPersonaId, person, userLogin, "Access is granted for user {0} {1} in product {2} by user {3} {4}.");

				return string.Empty;
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageUPFMProductUser - Error for user with userPersonaId id - {userPersonaId}", exception: ex);
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
				return $"Error - {ex.Message}";
			}
		}

		/// <summary>
		/// Unassign User
		/// </summary> 
		public string UnassignUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole, ProductEnum product)
		{
			var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError)
			{
				WriteToErrorLog(
					$"ManageUPFMProductUser-UnassignUser - Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
				return listResponse.ErrorReason;
			}

			//var product = ProductEnumHelper.GetUPFMProductEnum(_upfmProductId);

			List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);
			if (roleList?.Count > 0)
			{
				long roleId = roleList[0].RoleID;
				// Delete existing roleId
				RepositoryResponse result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: roleId, userId: _userClaims.UserId, deleteRole: true);
				if (result.Id < 0)
				{
					WriteToErrorLog($"ManageUPFMProductUser-UnassignUser - Unable to delete record for user with userPersonaId - {userPersonaId}, RoleId - {roleId}");
					return result.ErrorMessage;
				}

				List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, product);
				List<string> unassignedProperties = new List<string>();

				foreach (var property in propertyList)
				{
					unassignedProperties.Add(property.ID.ToString());
				}

				if (unassignedProperties.Count > 0)
				{
					Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyInstanceData(userPersonaId, product, Convert.ToInt64(property)); });
				}
			}

			WriteToInformationLog($"ManageUPFMProductUser-UnassignUser userPersonaId:{userPersonaId}");
			UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

			// Activity Logging
			WriteUnassignActivityLog(editorPersonaId, userPersonaId);

			return string.Empty;
		}

		/// <summary>
		/// Get Company Product Company InstanceId
		/// </summary>
		/// <param name="organizationRealPageId"></param>
		/// <param name="booksCustmerMasterId"></param>
		/// <param name="blueBookProductName"></param>
		/// <param name="domain"></param>
		/// <param name="includeExtra"></param>
		/// <param name="useTranslate"></param>
		/// <returns></returns>
		public string GetProductCompanyInstanceId(Guid organizationRealPageId, long booksCustmerMasterId, string blueBookProductName, string domain, string includeExtra = "", bool useTranslate = true)
		{			
			IList<CustomerCompanyMap> companyProductList = _blueBook.GetCompanyMap(organizationRealPageId, booksCustmerMasterId, source: blueBookProductName.ToUpper(), domain: domain, includeExtra: includeExtra, useTranslate: useTranslate);			
			if (companyProductList == null) { companyProductList = new List<CustomerCompanyMap>(); }
			CustomerCompanyMap company = new CustomerCompanyMap();
			if (companyProductList.Any(a => a.Source.Equals(blueBookProductName, StringComparison.OrdinalIgnoreCase)))
			{
				company = (from a in companyProductList where a.Source.Equals(blueBookProductName, StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
			}
			return company.CompanyInstanceSourceId;
		}
		/// <summary>
		/// Get multi company propeties of product
		/// </summary>
		/// <param name="productCode"></param>
		/// <returns></returns>
		public List<UserCompaniesProperties> GetUPFMMultiCompanyProperties(string productCode)
		{			
			IManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);
			int productId = (int)ProductEnumHelper.GetProductEnumByProductCode(productCode);
			List<UserCompaniesProperties> userCompaniesProperties = new List<UserCompaniesProperties>();
			var companyResponse = manageUserLogin.GetUserPersonaOrganization(_userClaims.LoginName);
			var upfmProduct = ProductEnumHelper.GetUPFMProductEnum(productId);
			string errorReason = string.Empty;
			foreach (var company in companyResponse)
			{
				var compnayInstanceSourceId = GetProductCompanyInstanceId(company.OrganizationRealPageId, company.BooksCustomerMasterId, productCode, "Primary");
				var propertyResponse = GetUPFMProperties(company.PersonaId, upfmProduct, null, companyResponse.Count > 1 ? true : false, company.OrganizationRealPageId.ToString());
				if (propertyResponse.Records == null || propertyResponse.Records.Count == 0) errorReason = "Properties are not loaded from Blue Book";

				var userCompanyProperties = new UserCompaniesProperties()
				{
					Id = compnayInstanceSourceId,
					OrganizationName = company.OrganizationName,
					InstanceId = company.OrganizationRealPageId,
					ErrorReason = errorReason,
					Properties = new List<Properties>()
				};
				foreach (var product in propertyResponse.Records.ToList())
				{
					var properties = new Properties()
					{
						Id = ((ProductProperty)product).ID,
						InstanceId = ((ProductProperty)product).InstanceId,
						PropertyName = ((ProductProperty)product).Name
					};
					userCompanyProperties.Properties.Add(properties);
				}
				userCompaniesProperties.Add(userCompanyProperties);
			}
			return userCompaniesProperties;
		}
	}
}
