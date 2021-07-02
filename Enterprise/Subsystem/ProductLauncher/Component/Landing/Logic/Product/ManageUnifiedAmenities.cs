using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedAmenities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using UserAssignProductPropertyRole = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedAmenities.UserAssignProductPropertyRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
	/// <summary>
	/// Used to manage Unified Amenity users
	/// </summary>
	public class ManageUnifiedAmenities : ManageProductBase, IManageUnifiedAmenities
	{
		private DefaultUserClaim _userClaims;

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public ManageUnifiedAmenities(DefaultUserClaim userClaims) : base((int)ProductEnum.UnifiedAmenities, userClaims, null, null)
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
		public ManageUnifiedAmenities(DefaultUserClaim defaultUserClaim, IManagePersona managePersona, IManagePerson managePerson, IManageBlueBook manageBlueBook, IProductRepository productRepository, ISamlRepository samlRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, IUserRoleRightRepository userRoleRightRepository, IManageUserLogin manageUserLogin, IUnifiedLoginRepository unifiedLoginRepository, IPropertyRepository propertyRepository, IUserLoginRepository userLoginRepository) : base((int)ProductEnum.UnifiedAmenities, productInternalSettingRepository, productRepository)
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
		public string ManageUnifiedAmenitiesUser(long editorPersonaId, long userPersonaId, UnifiedAmenitiesPropertyRole userAssignProductPropertyRole)
		{
			WriteToDiagnosticLog($"ManageUnifiedAmenitiesUser - Begin create/update user for user with userPersonaId id - {userPersonaId}.");
			try
			{
				var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					WriteToErrorLog($"ManageUnifiedAmenitiesUser Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}");
					return listResponse.ErrorReason;
				}

                var productInternalSettingList = GetProductSetting((int)ProductEnum.UnifiedPlatform);
                bool usePropertyInstances = (productInternalSettingList?.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedAmenities", StringComparison.OrdinalIgnoreCase))?.Value) == "1";

                var userPersona = _managePersona.GetPersona(userPersonaId);
				var realPageId = userPersona.RealPageId;
				var person = _managePerson.GetPerson(realPageId);
				var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

				// super user
				// TODO what to do here?
				if (IsSuperUser(userPersonaId))
				{
					WriteToDiagnosticLog($"ManageUnifiedAmenitiesUser - new user is Super user with userPersonaId id - {userPersonaId}.");
					IList<int> productIdList = _productRepository.GetProductIdsByCompany(userPersona.OrganizationPartyId);
					var gbAllRoles = _productRepository.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId) ?? new List<ProductRole>();
					string superUserRoleId = gbAllRoles.Find(p => p.Name.ToUpper() == "MANAGE AMENITY WITH PRICING").ID;

					userAssignProductPropertyRole = new UnifiedAmenitiesPropertyRole
					{
						PropertyList = new List<string> { "-1" },
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
								WriteToDiagnosticLog($"ManageUnifiedAmenitiesUser - removing role for user userPersonaId id - {userPersonaId}, RoleId - {existingRoleId}.");
								result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: existingRoleId, userId: _userClaims.UserId, deleteRole: true);
								if (result.Id < 0)
								{
									WriteToErrorLog($"ManageUnifiedAmenitiesUser - Unable to delete role for user with userPersonaId - {userPersonaId}, RoleId - {existingRoleId}");
									return result.ErrorMessage;
								}
							}

							if (role.RoleID != 0)
							{
								// add the role
								WriteToDiagnosticLog($"ManageUnifiedAmenitiesUser - adding role for userPersonaId id - {userPersonaId}, RoleId - {role.RoleID}.");
								result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: role.RoleID, userId: _userClaims.UserId, deleteRole: false);
								if (result.Id < 0)
								{
									WriteToErrorLog($"ManageUnifiedAmenitiesUser - Unable to add role for user with userPersonaId - {userPersonaId}, RoleId - {role.RoleID}");
									return result.ErrorMessage;
								}
							}
						}
					}

                    if (userAssignProductPropertyRole.PropertyList != null && userAssignProductPropertyRole.PropertyList.Count > 0 && userAssignProductPropertyRole.PropertyList[0].ToUpper() == "ALL")
                    {
                        IList<int> productIdList = _productRepository.GetProductIdsByCompany(userPersona.OrganizationPartyId);
                        var gbAllRoles = _productRepository.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId) ?? new List<ProductRole>();
                        if(gbAllRoles != null )
                        {
                            role.RoleID = long.Parse(productPropertyRole.RoleList[0]);                           
                            if (gbAllRoles.Any(r => long.Parse(r.ID) == role.RoleID && (r.accessAllProperties)))
                            {
                                userAssignProductPropertyRole.PropertyList = new List<string> { "-1" };
                            }
                        }
                        
                    }

					List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, (int)ProductEnum.UnifiedAmenities);
					List<string> assignedPropertyList = userAssignProductPropertyRole.PropertyList;

					List<string> unassignedProperties = new List<string>();
					List<string> assignedProperties = new List<string>();

					foreach (string propertyId in assignedPropertyList)
					{
						if (propertyList.All(p => p.ID != propertyId))
						{
							// new property to be added
							assignedProperties.Add(propertyId);
						}
					}

					foreach (ProductProperty prop in propertyList)
					{
						if (assignedPropertyList.All(p => p != prop.ID.ToString()))
						{
							unassignedProperties.Add(prop.ID.ToString());
						}
					}

                    if (!usePropertyInstances)
                    {
                        if (unassignedProperties.Count > 0)
                        {
                            Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property)); });
                        }

                        if (assignedProperties.Count > 0)
                        {
                            Parallel.ForEach(assignedProperties, property => { result = InsertAssignedPropertyData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property)); });
                        }
                    }
                    else
                    {
                        if (unassignedProperties.Count > 0)
                        {
                            Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyInstanceData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property)); });
                        }

                        if (assignedProperties.Count > 0)
                        {
                            Parallel.ForEach(assignedProperties, property => { result = InsertAssignedPropertyInstanceData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property)); });
                        }

                    }
                }

				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

				return string.Empty;
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageUnifiedAmenitiesUser - Error for user with userPersonaId id - {userPersonaId}", exception: ex);
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
				return $"Error - {ex.Message}";
			}
		}

		/// <summary>
		/// Unassign User
		/// </summary> 
		public string UnassignUser(long editorPersonaId, long userPersonaId, UnifiedAmenitiesPropertyRole userAssignProductPropertyRole)
		{
			var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError)
			{
				WriteToErrorLog(
					$"UnassignUser - Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
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
					WriteToErrorLog($"UnassignUser - Unable to delete record for user with userPersonaId - {userPersonaId}, RoleId - {roleId}");
					return result.ErrorMessage;
				}
			}

			WriteToInformationLog($"UnassignUser userPersonaId:{userPersonaId}");
			UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);


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
			WriteToDiagnosticLog($"GetRoles at beginning of method for user with editorPersona id - {editorPersonaId}");

			var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (result.IsError)
				{
					WriteToErrorLog($"GetRoles.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				// get roles from DB for UnifiedAmenities product
				WriteToDiagnosticLog($"GetRoles  Getting all GB roles from GB DB - ocr.ListRolesByParty with party id - {partyId}");
				IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
				var gbAllRoles = _productRepository.ListRolesForProductByParty(partyId, productIdList, _productId) ?? new List<ProductRole>();
				gbAllRoles = gbAllRoles?.OrderBy(r => r.Name).ToList();

                

                WriteToDiagnosticLog($"GetRoles.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

				if (userPersonaId != 0) // Called during updating Existing User
				{
					WriteToDiagnosticLog($"GetRoles-MergeAccessGroupsWithGreenbook calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
					response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId);
					WriteToDiagnosticLog($"GetRoles-MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
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
				WriteToErrorLog($"GetRoles Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
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
			WriteToDiagnosticLog($"GetRightsByRole at beginning of method for user with editorPersona id - {editorPersonaId}");

			var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
				if (result.IsError)
				{
					WriteToErrorLog($"GetRightsByRole.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				WriteToDiagnosticLog($"GetRightsByRole Getting all GB roles from GB DB - pr.ListRolesForProductByParty with party id - {partyId}");
				ProductRepository pr = new ProductRepository();
				IList<int> productIdList = pr.GetProductIdsByCompany(partyId);
				var gbAllRights = _unifiedLoginRepository.ListRightsByRole(partyId, productIdList, _productId, roleId) ?? new List<ProductRight>();

				gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

				WriteToDiagnosticLog($"GetRightsByRole.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

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
				WriteToErrorLog($"ManageUnifiedAmenities.GetRightsByRole Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
			}

			return response;
		}

        /// <summary>
        /// Used to unassign a role assigned to the user
        /// </summary>
        /// <param name="userProductPropertyRole"></param>
        /// <returns></returns>
        private UserAssignProductPropertyRole MapGbObjectToProduct(UnifiedAmenitiesPropertyRole userProductPropertyRole)
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

			WriteToDiagnosticLog($"MergeSelRolesWithGreenbook  Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}");
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
			if(allRoles != null){
				if (!allRoles.Any(s => s.IsAssigned == true) && allRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase))){
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
		/// Used to get the list of properties for the company or for the given user
		/// </summary>
		/// <param name="editorPersonaId">User making the request</param>
		/// <param name="userPersonaId">The user id to merge with the property list, if used. 0 for all properties</param>
		/// <param name="assignedOnly">Only return the properties assigned to the given user persona id</param>
		/// <param name="datafilter">A datafilter used to filter the properties. Not currently used</param>
		/// <returns></returns>
		public ListResponse GetProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter)
		{
			ListResponse result = new ListResponse();
			WriteToDiagnosticLog($"GetProperties - at beginning of method for user with editorPersona id - {editorPersonaId}");

			try
			{
				result = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
				if (result.IsError)
				{
					WriteToErrorLog($"GetProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				CustomerCompany masterCompany = _blueBook.GetCompanyCustomerInfo(_editorPersona.Organization.RealPageId, _editorPersona.Organization.OrganizationDomain.Name, _editorPersona.Organization.BooksCustomerMasterId);
                if (masterCompany == null || masterCompany?.CustomerCompanyId == 0 || (!masterCompany.IsActive == true && masterCompany.CustomerCompanyId != 0))
                {
					WriteToErrorLog($"GetProperties-GetCompanyCustomerInfo - Error looking for company id {_editorPersona.Organization.RealPageId} customermasterid {_editorPersona.Organization.BooksCustomerMasterId} in bluebook for user with editorPersona id - {editorPersonaId}.");
					return new ListResponse { IsError = true, ErrorReason = CommonMessageConstants.CompanyErrorMessage };
				}

				WriteToDiagnosticLog($"ManageProductProspectContact.GetProperties-GetProductCompanyInstanceId - Found blue book company id {_editorPersona.Organization.RealPageId} - customermasterid {_editorPersona.Organization.BooksCustomerMasterId} for user editorPersona id -{editorPersonaId}");

                //IList<CustomerCompanyPropertyMap> propertyList = _blueBook.GetVCompanyPropertyMap(masterCompany.CustomerCompany.FirstOrDefault(p => p.IsActive == true && p.CustomerCompanyId != 0).CustomerCompanyId, "");
                IList<CustomerCompanyPropertyMap> propertyList = _blueBook.GetVCompanyPropertyMap( (masterCompany.IsActive == true && masterCompany.CustomerCompanyId != 0) ? masterCompany.CustomerCompanyId : 0, "");
                WriteToDiagnosticLog($"ManageProductProspectContact.GetProperties-GetPropertyInstance - Found total {propertyList?.Count} properties with blue book company id {_editorPersona.Organization.RealPageId} customermasterid {_editorPersona.Organization.BooksCustomerMasterId} for user with editorPersona id - {editorPersonaId}.");

				IList<ProductProperty> blueBookPropertyList = propertyList.FromBlueBookMasterPropertyToGBProperties() ?? new List<ProductProperty>();
				WriteToDiagnosticLog($"ManageProductProspectContact.GetProperties-FromBlueBookToGBProperties() completed for user with editorPersona id -{editorPersonaId}.");

				// need to do a filter on the result
				if (userPersonaId != 0)
				{
					WriteToDiagnosticLog($"GetProperties- calling MergeProductPropertiesWithGreenbook....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
					result = MergeProductPropertiesWithGreenbook(blueBookPropertyList, userPersonaId, assignedOnly);
					WriteToDiagnosticLog($"GetProperties-MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}.");
				}
				else
				{
					result = new ListResponse() // create new user
					{
						Records = blueBookPropertyList.Cast<object>().ToList(),
						TotalRows = blueBookPropertyList.Count,
						RowsPerPage = blueBookPropertyList.Count,
						TotalPages = 1,
						ErrorReason = string.Empty
					};
				}
			}
			catch (Exception ex)
			{
				result.IsError = true;
				result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
				WriteToErrorLog($"ManageProductProspectContact.GetProperties - There was a problem getting the properties for user with editorPersona id - {editorPersonaId}.",
					exception: ex);
			}

			return result;
		}

		/// <summary>
		/// Used to merge product property data with Unifed Login property data for the user
		/// </summary>
		/// <param name="blueBookPropertyList">The list of properties from BlueBook</param>
		/// <param name="userPersonaId">The user id to filter on</param>
		/// <param name="assignedOnly">Only return assigned records</param>
		/// <returns></returns>
		private ListResponse MergeProductPropertiesWithGreenbook(IList<ProductProperty> blueBookPropertyList, long userPersonaId, bool assignedOnly)
		{
			// merge the given user details with the list
			List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, (int)ProductEnum.UnifiedAmenities);

			foreach (var property in propertyList)
			{
				if (blueBookPropertyList.Any(a => a.ID == property.ID.ToString()))
				{
					ProductProperty pp = (from a in blueBookPropertyList
										  where a.ID == property.ID.ToString()
										  select a).FirstOrDefault();
					if (pp != null)
					{
						pp.IsAssigned = true;
					}
				}
			}

			if (assignedOnly)
			{
				blueBookPropertyList = blueBookPropertyList.Where(a => a.IsAssigned == true).ToList();
			}

			return new ListResponse()
			{
				Records = blueBookPropertyList.Cast<object>().ToList(),
				TotalRows = blueBookPropertyList.Count(),
				RowsPerPage = 9999,
				ErrorReason = string.Empty,
				TotalPages = 1,
				Additional = null
			};
		}

		/// <summary>
		/// Used to assign a property to the given user
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		private RepositoryResponse InsertAssignedPropertyData(long userPersonaId, ProductEnum productId, long propertyId)
		{
			return InsertAssignedUserPropertyData(userPersonaId, ProductEnum.UnifiedAmenities, propertyId);
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
            return InsertAssignedUserPropertyInstanceData(userPersonaId, (int)ProductEnum.UnifiedAmenities, propertyInstanceId);
        }

		/// <summary>
		/// Used to unassign a property to the given user
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="productId"></param>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		private RepositoryResponse DeleteAssignedPropertyData(long userPersonaId, ProductEnum productId, long propertyId)
		{
			return DeleteAssignedUserPropertyData(userPersonaId, ProductEnum.UnifiedAmenities, propertyId);
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
            return DeleteAssignedUserPropertyInstanceData(userPersonaId, (int)ProductEnum.UnifiedAmenities, propertyInstanceId);
        }

        #endregion

	}
}