using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.IntegrationsMarketplace;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
	/// <summary>
	/// Integration Marketplace Logic
	/// </summary>
	public class ManageProductIntegrationMarketplace : ManageProductBase, IManageProductIntegrationMarketplace
	{
		#region Private members

		private DefaultUserClaim _userClaims;
		private readonly string _apiEndPoint;
		private readonly string _adminDefaultRole;

		#endregion

		#region Ctor

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="userClaims"></param>        
		public ManageProductIntegrationMarketplace(DefaultUserClaim userClaims) : base((int)ProductEnum.IntegrationMarketplace, userClaims, null, null)
		{
			WriteToDiagnosticLog("IntegrationMarketplace - ManageProductIntegrationMarketplace.Ctor - Getting Product settings.");
			_userClaims = userClaims;
			_productId = (int)ProductEnum.IntegrationMarketplace;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_blueBook = new ManageBlueBook(userClaims);
			_apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
			_adminDefaultRole = _productInternalSettingList.First(a => a.Name.Equals("SystemAdminUserDefaultRole", StringComparison.OrdinalIgnoreCase)).Value;
			WriteToDiagnosticLog("IntegrationMarketplace.Ctor - Received Product settings.");
		}

		#endregion

		#region Properties and Roles

		/// <summary>
		/// Returns Roles (User Access Groups in IntegrationMarketplace)
		/// </summary>
		public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId)
		{
			WriteToDiagnosticLog(
				$"IntegrationMarketplace - ManageProductIntegrationMarketplace.GetRoles at beginning of method for user with editorPersona id - {editorPersonaId}");

			var response = new ListResponse();
			try
			{
				ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor
				if (result.IsError)
				{
					WriteToErrorLog(
						$"IntegrationMarketplace - ManageProductIntegrationMarketplace.GetRoles.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
					return result;
				}

				var gbAllRoles = GetIntegrationMarketplaceRoles();

				if (gbAllRoles == null)
				{
					WriteToErrorLog($"ManageProductIntegrationMarketplace.GetRoles-no roles received from product for user with editorPersona id - {editorPersonaId}.");

					response.IsError = true;
					response.ErrorReason = "No roles received from product.";
					return response;
				}

				WriteToDiagnosticLog(
					$"IntegrationMarketplace - ManageProductIntegrationMarketplace.GetRoles.MapProductAccessGroupsToGB() completed for user with editorPersona id - {editorPersonaId}");

				if (userPersonaId != 0) // Called during updating Existing User
				{
					WriteToDiagnosticLog(
							$"IntegrationMarketplace - ManageProductIntegrationMarketplace.GetRoles-MergeAccessGroupsWithGreenbook calling....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
					response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId);
					WriteToDiagnosticLog(
						   $"IntegrationMarketplace - ManageProductIntegrationMarketplace.GetRoles-MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
				}
				else // Called during creating a new User
				{
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
				response.ErrorReason = $"IntegrationMarketplace - There was a problem getting the roles.";
				WriteToErrorLog($"IntegrationMarketplace - ManageProductIntegrationMarketplace.GetRoles Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
			}

			return response;
		}

		/// <summary>
		/// Change user type 
		/// </summary>
		public string ChangeIntegrationMarketplaceUserType(long createUserPersonaId, long assignUserPersonaId, IntegrationMarketplacePropertyRole rpList, BatchProcessType batchProcessType)
		{
			return ManageIntegrationMarketplaceUser(createUserPersonaId, assignUserPersonaId, rpList, batchProcessType);
		}

		/// <summary>
		/// Used to create/update a user in IntegrationMarketplace
		/// </summary>
		public string ManageIntegrationMarketplaceUser(long editorPersonaId, long userPersonaId, IntegrationMarketplacePropertyRole userAssignProductPropertyRole, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
		{
			WriteToDiagnosticLog($"ManageIntegrationMarketplaceUser - Begin create/update user for user with userPersonaId id - {userPersonaId}.");

			try
			{
				var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					WriteToErrorLog($"ManageIntegrationMarketplaceUser Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}");
					return listResponse.ErrorReason;
				}

				var userPersona = _managePersona.GetPersona(userPersonaId);
				var realPageId = userPersona.RealPageId;
				var person = _managePerson.GetPerson(realPageId);
				var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

				var roleCodeToAssign = string.Empty;

				// super user
				if (IsSuperUser(userPersonaId))
				{
					WriteToDiagnosticLog($"ManageIntegrationMarketplaceUser - new user is Super user with userPersonaId id - {userPersonaId}.");
					roleCodeToAssign = _adminDefaultRole;
				}
				else
				{
					if (userAssignProductPropertyRole != null && !userAssignProductPropertyRole.RoleList.Any())
					{
						WriteToErrorLog(
							$"ManageIntegrationMarketplaceUser - No roles received for user with userPersonaId id - {userPersonaId}");
						UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus,
							(int)ProductBatchStatusType.Error);
						return $"Error - No roles received for user with userPersonaId id - {userPersonaId}";
					}

					var roleIdToAssign = Convert.ToInt32(userAssignProductPropertyRole.RoleList.FirstOrDefault());

					// Get IM Roles
					var allImRoles = GetIntegrationMarketplaceRoles();

					if (!allImRoles.Any())
					{
						WriteToErrorLog(
							"ManageIntegrationMarketplaceUser - GetIntegrationMarketplaceRoles - failed to get roles.");
						return "Not able to IM get roles.";
					}

					var roleObjectToAssign = allImRoles.FirstOrDefault(x => x.Id == roleIdToAssign);

					if (roleObjectToAssign == null)
					{
						WriteToErrorLog(
							"ManageIntegrationMarketplaceUser - roleObjectToAssign - unable to convert object.");
						return "Not able to parse roles.";
					}

					roleCodeToAssign = roleObjectToAssign.ShortName;
				}

				var existingRoleCode = string.Empty;
				IList<SamlAttributes> productAttributes = GetAssignedRoleForpersonaFromSamlAttribute(userPersonaId);

				if (productAttributes.Any(a =>
					a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant()))
				{
					existingRoleCode =
						(from a in productAttributes
						 where a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant()
						 select a.Value).FirstOrDefault();
					WriteToDiagnosticLog($"ManageIntegrationMarketplaceUser - existingRoleId={existingRoleCode}");
				}

				// Check Create User(no role) vs Update User (existing role)
				if (!string.IsNullOrEmpty(roleCodeToAssign) && !roleCodeToAssign.Equals(existingRoleCode, StringComparison.OrdinalIgnoreCase))
				{
					if (!string.IsNullOrEmpty(existingRoleCode)) // user already exist
					{
						WriteToDiagnosticLog($"ManageIntegrationMarketplaceUser - Updating role for user userPersonaId id - {userPersonaId}, existingRoleCode - {existingRoleCode}.");

						int samlUserAttributeId = 0;
						if (productAttributes.Any(a => a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant()))
						{
							samlUserAttributeId = (from a in productAttributes where a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant() select a.SamlUserAttributeId).FirstOrDefault();
							WriteToDiagnosticLog($"ManageIntegrationMarketplaceUser - samlUserAttributeId={samlUserAttributeId}");
						}

						SamlAttributes samlAttributes = new SamlAttributes
						{
							SamlAttributeId = (int)SamlAttributeEnum.RoleCode,
							Value = roleCodeToAssign,
							SamlUserAttributeId = samlUserAttributeId
						};

						var result = _samlRepository.UpdateSamlUserAttribute(samlAttributes);

						if (result.Id < 0)
						{
							WriteToErrorLog($"ManageIntegrationMarketplaceUser - Unable to Updating role for user with userPersonaId - {userPersonaId}, existingRoleCode - {existingRoleCode}");
							return result.ErrorMessage;
						}
					}
					else
					{
						// new user - add the role
						WriteToDiagnosticLog($"ManageIntegrationMarketplaceUser - adding role for userPersonaId id - {userPersonaId}, roleCodeToAssign - {roleCodeToAssign}.");
						var result = _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.RoleCode, roleCodeToAssign);
						if (result.Id < 0)
						{
							WriteToErrorLog($"ManageIntegrationMarketplaceUser - Unable to add role for user with userPersonaId - {userPersonaId}, roleCodeToAssign - {roleCodeToAssign}");
							return result.ErrorMessage;
						}

                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

						return string.Empty;
                    }
				}
				else
				{
					/*** shouldn't come to this unless UI sending wrong info ***/

					// no update as role matches
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

					// add activity log
					WriteUserActivityLogWithMessage(editorPersonaId, person, userLogin,
						"No change in role for user {0} {1} in product {2} by user {3} {4}. old role is -" + existingRoleCode + "new role code" + roleCodeToAssign);

					return string.Empty;
				}

				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

				if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
				{
					WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, batchProcessType);
				}


				return string.Empty;
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageIntegrationMarketplaceUser - Error for user with userPersonaId id - {userPersonaId}", exception: ex);
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
				return $"Error - {ex.Message}";
			}
		}

		/// <summary>
		/// Un-assign User
		/// </summary> 
		public string UnassignUser(long editorPersonaId, long userPersonaId, IntegrationMarketplacePropertyRole userAssignProductPropertyRole)
		{
			var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError)
			{
				WriteToErrorLog(
					$"UnassignUser - Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
				return listResponse.ErrorReason;
			}

			string roleCode = string.Empty;
			IList<SamlAttributes> productAttributes = GetAssignedRoleForpersonaFromSamlAttribute(userPersonaId);

			if (productAttributes.Any(a => a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant()))
			{
				roleCode = (from a in productAttributes where a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant() select a.Value).FirstOrDefault();
				WriteToDiagnosticLog($"UnassignUser -  roleCode={roleCode}");
			}

			if (!string.IsNullOrEmpty(roleCode))
			{
				// Delete existing roleCode
				RepositoryResponse result = _samlRepository.DeleteSamlUserProductInfoAndStatus(userPersonaId, (int)ProductEnum.IntegrationMarketplace);

				if (result.Id < 0)
				{
					WriteToErrorLog($"UnassignUser - Unable to delete record for user with userPersonaId - {userPersonaId}, roleCode - {roleCode}");
					return result.ErrorMessage;
				}
			}

			WriteToInformationLog($"UnassignUser success userPersonaId:{userPersonaId}");
			UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

			return string.Empty;
		}

		/// <summary>
		/// Get All IM Roles
		/// </summary>
		public List<IntegrationMarketplaceRole> GetIntegrationMarketplaceRoles()
		{
			// get access groups from Integration Marketplace  
			var gbAllRolesResponse = GetResultFromApi<IntegrationMarketplaceRoleResponse>($"{_apiEndPoint}/roles");

			// Get Roles from API
			return gbAllRolesResponse?.Data;
		}

		#endregion

		#region Private Methods  

		private ListResponse MergeSelRolesWithGreenbook(IList<IntegrationMarketplaceRole> allRoles, long userPersonaId)
		{
			// get roles from DB for IntegrationMarketplace product
			WriteToDiagnosticLog(
				   $"IntegrationMarketplace - Getting assigned user roles from GB SAMLAttributes - GetAssignedRoleForPersona with persona id - {userPersonaId}");

			string roleCode = string.Empty;
			IList<SamlAttributes> productAttributes = GetAssignedRoleForpersonaFromSamlAttribute(userPersonaId);

			if (productAttributes.Any(a => a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant()))
			{
				roleCode = (from a in productAttributes where a.Name.ToUpper() == SamlAttributeEnum.RoleCode.ToString().ToUpperInvariant() select a.Value).FirstOrDefault();
				WriteToDiagnosticLog($"IntegrationMarketplace - MergeSelRolesWithGreenbook - roleCode={roleCode}");
			}

			if (allRoles.Any(a => a.ShortName.Equals(roleCode, StringComparison.OrdinalIgnoreCase)))
			{
				IntegrationMarketplaceRole selectedRole = (from a in allRoles
														   where a.ShortName.Equals(roleCode, StringComparison.OrdinalIgnoreCase)
														   select a).FirstOrDefault();
				if (selectedRole != null)
				{
					selectedRole.IsAssigned = true;
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

		private IList<SamlAttributes> GetAssignedRoleForpersonaFromSamlAttribute(long userPersonaId)
		{

			WriteToDiagnosticLog($"GetAssignedRoleForpersonaFromSAMLAttribute - userPersonaId={userPersonaId}");
			IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);

			return productAttributes;
		}

		private T GetResultFromApi<T>(string baseUrlAndQuery) where T : class
		{
			T results = null;
			_client.DefaultRequestHeaders.Clear();
			var response = _client.GetAsync(baseUrlAndQuery).Result;

			if (response.IsSuccessStatusCode)
			{
				var jsonContent = response.Content.ReadAsStringAsync().Result;
				results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
			}
			return results;
		}

		#endregion

	}

	/// <summary>
	/// IntegrationMarketplaceRoleResponse
	/// </summary>
	public class IntegrationMarketplaceRoleResponse
	{
		/// <summary>
		/// Data from API
		/// </summary>
		public List<IntegrationMarketplaceRole> Data { get; set; }

	}

	/// <summary>
	/// IntegrationMarketplaceRole
	/// </summary>
	public class IntegrationMarketplaceRole
	{
		private string _name = string.Empty;

		/// <summary>
		/// Role Id
		/// </summary>
		[JsonProperty(PropertyName = "id")]
		public int Id { get; set; }

		/// <summary>
		/// Role Short Name
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Get Role Display Name
		/// </summary>
		[JsonProperty(PropertyName = "name")]
		public string GetName
		{
			get { return this._name; }
		}

		/// <summary>
		/// Role Name
		/// </summary>
		[JsonProperty(PropertyName = "DisplayName")]
		public string SetName
		{
			set
			{
				this._name = value;
			}
		}

		/// <summary>
		/// Role Description
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Is Role Assigned
		/// </summary>
		public bool IsAssigned { get; set; }
	}
}
