using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.UnifiedAmenities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UserAssignProductPropertyRole = UnifiedLogin.SharedObjects.Product.UnifiedAmenities.UserAssignProductPropertyRole;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Logic.Product
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
		public ManageUnifiedAmenities(DefaultUserClaim userClaims) : base((int)ProductEnum.UnifiedAmenities, userClaims, (IProductInternalSettingRepository)null, (IProductRepository)null)
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
		/// <param name="userLoginRepository"></param>
		public ManageUnifiedAmenities(DefaultUserClaim defaultUserClaim, IManagePersona managePersona, IManagePerson managePerson, IManageBlueBook manageBlueBook, IProductRepository productRepository, ISamlRepository samlRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, IUserRoleRightRepository userRoleRightRepository, IManageUserLogin manageUserLogin, IUnifiedLoginRepository unifiedLoginRepository, IPropertyRepository propertyRepository, IUserLoginRepository userLoginRepository) : base((int)ProductEnum.UnifiedAmenities, defaultUserClaim, productInternalSettingRepository, productRepository)
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


                    // ✅ OPTIMIZED: Calculate property differences efficiently
                    List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, (int)ProductEnum.UnifiedAmenities);
                    List<string> assignedPropertyList = userAssignProductPropertyRole.PropertyList;

                    // Use HashSet for O(n) complexity instead of O(n²)
                    var currentPropertySet = new HashSet<string>(propertyList.Select(p => p.ID), StringComparer.OrdinalIgnoreCase);
                    var requestedPropertySet = new HashSet<string>(assignedPropertyList, StringComparer.OrdinalIgnoreCase);

                    var unassignedProperties = currentPropertySet.Except(requestedPropertySet).ToList();
                    var newAssignedProperties = requestedPropertySet.Except(currentPropertySet).ToList();


                    WriteToDiagnosticLog($"ManageUnifiedAmenitiesUser - Properties to unassign: {unassignedProperties.Count}, Properties to assign: {newAssignedProperties.Count}");

                    // ✅ FIXED: Process properties with proper error handling and batching
                    if (!usePropertyInstances)
                    {
                        var operationResult = ProcessPropertyOperations(
                            userPersonaId,
                            unassignedProperties,
                            newAssignedProperties,
                            usePropertyInstances: false);

                        if (!string.IsNullOrEmpty(operationResult))
                        {
                            WriteToErrorLog($"ManageUnifiedAmenitiesUser - Property operations failed: {operationResult}");
                            // Don't return error - log and continue
                        }
                    }
                    else
                    {
                        var operationResult = ProcessPropertyOperations(
                            userPersonaId,
                            unassignedProperties,
                            newAssignedProperties,
                            usePropertyInstances: true);

                        if (!string.IsNullOrEmpty(operationResult))
                        {
                            WriteToErrorLog($"ManageUnifiedAmenitiesUser - Property instance operations failed: {operationResult}");
                            // Don't return error - log and continue
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
        /// Process property assignments/unassignments with batching and error handling
        /// </summary>
        private string ProcessPropertyOperations(
            long userPersonaId,
            List<string> unassignedProperties,
            List<string> assignedProperties,
            bool usePropertyInstances)
        {
            var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
            var successCount = 0;
            var lockObj = new object();

            // Configure parallelism based on property count
            int maxDegreeOfParallelism = unassignedProperties.Count + assignedProperties.Count > 500 ? 3 : 5;
            int batchSize = 50;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Process unassignments in batches
                if (unassignedProperties.Count > 0)
                {
                    WriteToDiagnosticLog($"ProcessPropertyOperations - Unassigning {unassignedProperties.Count} properties");

                    for (int i = 0; i < unassignedProperties.Count; i += batchSize)
                    {
                        var batch = unassignedProperties.Skip(i).Take(batchSize).ToList();

                        Parallel.ForEach(batch,
                            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                            property =>
                            {
                                try
                                {
                                    RepositoryResponse result;

                                    if (usePropertyInstances)
                                    {
                                        result = DeleteAssignedPropertyInstanceData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property));
                                    }
                                    else
                                    {
                                        result = DeleteAssignedPropertyData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property));
                                    }

                                    if (result.Id < 0)
                                    {
                                        errors.Add($"Failed to unassign property {property}: {result.ErrorMessage}");
                                    }
                                    else
                                    {
                                        lock (lockObj) { successCount++; }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"Exception unassigning property {property}: {ex.Message}");
                                    WriteToErrorLog($"ProcessPropertyOperations - Error unassigning property {property}", exception: ex);
                                }
                            });

                        // Log batch progress
                        WriteToDiagnosticLog($"ProcessPropertyOperations - Unassigned batch {i / batchSize + 1} ({Math.Min(i + batchSize, unassignedProperties.Count)}/{unassignedProperties.Count})");
                    }
                }

                // Process assignments in batches
                if (assignedProperties.Count > 0)
                {
                    WriteToDiagnosticLog($"ProcessPropertyOperations - Assigning {assignedProperties.Count} properties");

                    for (int i = 0; i < assignedProperties.Count; i += batchSize)
                    {
                        var batch = assignedProperties.Skip(i).Take(batchSize).ToList();

                        Parallel.ForEach(batch,
                            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                            property =>
                            {
                                try
                                {
                                    RepositoryResponse result;

                                    if (usePropertyInstances)
                                    {
                                        result = InsertAssignedPropertyInstanceData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property));
                                    }
                                    else
                                    {
                                        result = InsertAssignedPropertyData(userPersonaId, ProductEnum.UnifiedAmenities, Convert.ToInt64(property));
                                    }

                                    if (result.Id < 0)
                                    {
                                        errors.Add($"Failed to assign property {property}: {result.ErrorMessage}");
                                    }
                                    else
                                    {
                                        lock (lockObj) { successCount++; }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"Exception assigning property {property}: {ex.Message}");
                                    WriteToErrorLog($"ProcessPropertyOperations - Error assigning property {property}", exception: ex);
                                }
                            });

                        // Log batch progress
                        WriteToDiagnosticLog($"ProcessPropertyOperations - Assigned batch {i / batchSize + 1} ({Math.Min(i + batchSize, assignedProperties.Count)}/{assignedProperties.Count})");
                    }
                }

                stopwatch.Stop();
                WriteToDiagnosticLog($"ProcessPropertyOperations - Completed in {stopwatch.ElapsedMilliseconds}ms. Success: {successCount}, Errors: {errors.Count}");

                if (errors.Count > 0)
                {
                    var errorSummary = $"Property operations completed with {errors.Count} errors. First 5: {string.Join("; ", errors.Take(5))}";
                    WriteToErrorLog(errorSummary);
                    return errorSummary;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ProcessPropertyOperations - Critical error", exception: ex);
                return $"Critical error in property operations: {ex.Message}";
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
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter)
		{
			WriteToDiagnosticLog($"GetProperties at beginning of method for user with editorPersona id - {editorPersonaId}");

			var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (result.IsError)
				{
					WriteToErrorLog($"GetProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				var userPersona = _managePersona.GetPersona(userPersonaId);
				if (userPersona == null)
				{
					WriteToErrorLog($"GetProperties - Unable to find persona for userPersonaId - {userPersonaId}");
					return new ListResponse
					{
						IsError = true,
						ErrorReason = "Invalid user persona"
					};
				}

				// Get properties from BlueBook
				IList<CustomerCompanyPropertyMap> propertyMapList = _blueBook.GetVCompanyPropertyMap(userPersona.Organization.BooksCustomerMasterId, "");
				IList<ProductProperty> gbPropertyList = new List<ProductProperty>();

				if (propertyMapList != null && propertyMapList.Any())
				{
					gbPropertyList = propertyMapList.Select(p => new ProductProperty
					{
						ID = p.CustomerPropertyId.ToString(),
						Name = p.PropertyName,
						City = p.PropertyCity,
						State = p.PropertyState,
						Street1 = p.PropertyAddress,
						Zip = string.Empty, // PostalCode not available in CustomerCompanyPropertyMap
						IsAssigned = false,
						Active = p.IsActive ? "1" : "0"
					}).ToList();
				}

				if (userPersonaId != 0 && assignedOnly)
				{
					// Get assigned properties for the user
					List<ProductProperty> assignedProperties = GetAssignedPropertyForPersona(userPersonaId, (int)ProductEnum.UnifiedAmenities);
					
					if (assignedProperties != null && assignedProperties.Any())
					{
						foreach (var assignedProp in assignedProperties)
						{
							var prop = gbPropertyList.FirstOrDefault(p => p.ID == assignedProp.ID);
							if (prop != null)
							{
								prop.IsAssigned = true;
							}
						}
					}
				}

				WriteToDiagnosticLog($"GetProperties completed for user with editorPersona id - {editorPersonaId}");

				response = new ListResponse()
				{
					Records = gbPropertyList.Cast<object>().ToList(),
					TotalRows = gbPropertyList.Count(),
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				response.IsError = true;
				response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
				WriteToErrorLog($"GetProperties Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
			}

			return response;
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