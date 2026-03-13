using IdentityModel.Client;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ResearchApplication;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UserAssignProductPropertyRole = UnifiedLogin.SharedObjects.Product.ResearchApplication.UserAssignProductPropertyRole;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    /// Used to update Research Application user information
    /// </summary>
    public class ManageResearchApplication : ManageProductBase, IManageResearchApplication
    {
		#region Private members
		// used for activity logging
		private string _researchApplicationUrl = string.Empty;
		private string _researchApplicationApiEndPoint = string.Empty;
		private string _accessToken = string.Empty;
		private string _unifiedLoginResearchAppTokenUri = string.Empty;
		private string _UnifiedLoginResearchApplicationClientSecret = string.Empty;
        private DefaultUserClaim _userClaims;
		#endregion

		#region Ctor
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public ManageResearchApplication(DefaultUserClaim userClaims) : base((int)ProductEnum.ResearchApplication, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResearchApplication", "Getting Product settings" });
#endif
            _editorRealPageId = userClaims.UserRealPageGuid;
			_userClaims = userClaims;
            _blueBook = new ManageBlueBook(userClaims);

			_researchApplicationUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "PRODUCTURL").Value;
			_researchApplicationApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
			_client.BaseAddress = new Uri(_researchApplicationApiEndPoint);

			var listUnifiedLoginProductSetting = GetProductSetting((int)ProductEnum.UnifiedPlatform);
			_UnifiedLoginResearchApplicationClientSecret = Encoding.UTF8.GetString(Convert.FromBase64String(listUnifiedLoginProductSetting.First(a => a.Name.ToUpper() == "UNIFIEDLOGINRESEARCHAPPLICATIONCLIENTSECRET").Value));

			GetToken();
		}
		#endregion

		#region Public Methods
		#region Properties and Roles
		/// <summary>
		/// Returns Roles (User Access Groups in ResearchApplication)
		/// </summary>
		/// <param name="editorPersonaId">The persona id of the user making the request</param>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="partyId"></param>
		/// <returns>ListResponse object</returns>
		public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning. editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - Error: {result.ErrorReason}" });
                    return result;
                }

                // get roles from DB for ResearchApplication product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Getting all GB roles from GB DB - ocr.ListRolesByParty with party id - {partyId}" });

                var productIds = GetProductIdsByOrg();
                ProductRepository pr = new ProductRepository();
	            IList<int> productIdList = pr.GetProductIdsByCompany(partyId);
				var gbAllRoles = pr.ListRolesForProductByParty(partyId, productIdList, _productId);

                //var gbAllRoles = pr.ListRolesForProductByParty(partyId, _productId, productIds);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Complete. editorPersona id - {editorPersonaId}" });

                if (userPersonaId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Calling MergeSelRolesWithGreenbook. editorPersona id - {editorPersonaId} _productUserId - {_productUserId}" });
                    response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Completed. editorPersona id - {editorPersonaId} & _productUserId - {_productUserId}" });
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
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRoles", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
            }

            return response;
        }

		/// <summary>
		/// Returns Rights with selected rights for a roleId(User Access Groups in UserManagement)
		/// </summary>
		/// <param name="editorPersonaId">The persona id of the user making the request</param>
		/// <param name="partyId">Party Id</param>
		/// <param name="roleId">Role Id</param>
		/// <returns>ListResponse object</returns>
		public ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Beginning editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"GetCompanyEditorAndUserDetails Error editorPersona id - {editorPersonaId} Reason: {result.ErrorReason}" });
                    return result;
                }

                // get rights from DB for UserManagement product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Calling ListRightsByRole partyId - {partyId}" });

                var productIds = GetProductIdsByOrg();

                UnifiedLoginRepository umr = new UnifiedLoginRepository();
	            ProductRepository pr = new ProductRepository();
	            IList<int> productIdList = pr.GetProductIdsByCompany(partyId);
				var gbAllRights = umr.ListRightsByRole(partyId, productIdList, _productId, roleId);

                //gbAllRights = GetRightsWithoutDefault(gbAllRights);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Completed ListRightsByRole editorPersona id - {editorPersonaId}" });

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
                response.ErrorReason = "ResearchApplication -  There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRightsByRole", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
            }

            return response;
        }

		/// <summary>
		/// Updated to create/update a user in ResearchApplication
		/// </summary>
		/// <param name="editorPersonaId">The persona id of the user making the request</param>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="userAssignProductPropertyRole">userAssignProductPropertyRole object</param>
		/// <returns>Error Message</returns>
		public string ManageResearchApplicationUser(long editorPersonaId, long userPersonaId, ResearchAppRoleAndPropertyList userAssignProductPropertyRole)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResearchApplicationUser", $"Beginning editorPersona id - {editorPersonaId}" });
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResearchApplicationUser", $"Error editorPersona id - {editorPersonaId}. Reason: {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                var userPersona = _managePersona.GetPersona(userPersonaId);
                var realPageId = userPersona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                // super user
                if (IsSuperUser(userPersonaId))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResearchApplicationUser", $"New user is Super user. editorPersona id - {editorPersonaId}" });
                    var productIds = GetProductIdsByOrg();
                    ProductRepository pr = new ProductRepository();
	                IList<int> productIdList = pr.GetProductIdsByCompany(userPersona.OrganizationPartyId);
					var gbAllRoles = pr.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId);

                    var directorRole = gbAllRoles.First(p => p.Name.ToUpper() == "BLACK-BOOK DIRECTOR");

                    userAssignProductPropertyRole = new ResearchAppRoleAndPropertyList
                    {
                        PropertyList = new List<string>() { "-1" },
                        RoleList = new List<string>() { directorRole.ID }
                    };
                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResearchApplicationUser", $"_productUsername for user is {_productUsername}" });

                // Check for user role
                UL.Role role = new UL.Role();

                if (userAssignProductPropertyRole != null)
                {
                    // map userAssignProductPropertyRole to ProductPropertyRole
                    var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);

                    if (productPropertyRole.RoleList != null &&
                        productPropertyRole.RoleList.Count > 0)
                    {
                        role.RoleID = int.Parse(productPropertyRole.RoleList[0]);
                    }
                }

                List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);

                if (roleList == null || roleList.Count == 0) // New User
                {
                    // Insert into GB DB
                    InsertResearchApplicationProductUserDB(userPersonaId, editorPersonaId, role.RoleID);
                }
                else
                {
                    long existingRoleId = roleList[0].RoleID;
                    UpdateBlackBookRole(userPersonaId, editorPersonaId, role.RoleID, existingRoleId);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResearchApplicationUser", $"Trying to Add user. editorPersona id - {editorPersonaId}" });
                    //Post to Research Application when a persona (that has access to the Research Tool) roles are updated.
                    string url = string.Concat(_researchApplicationApiEndPoint, "/event-api/user-updated/", userPersonaId.ToString());
					HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url)
					{
						Content = new StringContent(JsonConvert.SerializeObject(new {}), System.Text.Encoding.Default, "application/json")
					};
					_client.DefaultRequestHeaders.Clear();
					_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
					HttpResponseMessage postResponse = _client.SendAsync(req).Result;
					if (postResponse.IsSuccessStatusCode)
					{
						dynamic resultObject = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
						WriteToDiagnosticLog("{ActionName} - {state}", logData : new Dictionary<string, object>(){{ "resultObject", resultObject } }, messageProperties: new object[] { "ManageResearchApplicationUser", $"Post result userPersonaId - {userPersonaId}" });
					}
                }

                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

                return "";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageResearchApplicationUser", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
                return $"Error - {ex.Message}";
            }
        }

		/// <summary>
		/// Unassign User
		/// </summary>
		/// <param name="editorPersonaId">The persona id of the user making the request</param>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="userAssignProductPropertyRole">userAssignProductPropertyRole object</param>
		/// <returns></returns>
		public string UnassignUser(long editorPersonaId, long userPersonaId, ResearchAppRoleAndPropertyList userAssignProductPropertyRole)
        {
			IPersona userPersona = _managePersona.GetPersona(userPersonaId);
			var userLogin = _manageUserLogin.GetUserLoginOnly(userPersona.RealPageId);

			var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error. userPersonaId - {userPersonaId}. Reason: {listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);
            if (roleList?.Count > 0)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Trying to DELETE existing role with editorPersona id - {editorPersonaId} userPersonaId - {userPersonaId}" });
                // Delete existing roleId
                InsertResearchApplicationProductUserDB(userPersonaId, editorPersonaId, roleList[0].RoleID, true);
            }

            WriteToInformationLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Setting product status to DELETED. editorPersona id - {editorPersonaId} userPersonaId - {userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

			//Post to Research Application when the persona access to research tool is revoked
			string url = string.Concat(_researchApplicationApiEndPoint, "/event-api/user-deleted");
			//Build the body of thr Post
			ResearchApplicationDeleteUser researchApplicationDeleteUser = new ResearchApplicationDeleteUser()
			{
				UserId = userLogin.UserId,
				PersonaId = userPersonaId
			};
			IDataObject<ResearchApplicationDeleteUser> dataObject = new DataObject<ResearchApplicationDeleteUser>()
			{
				data = researchApplicationDeleteUser
			};
			HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url)
			{
				Content = new StringContent(JsonConvert.SerializeObject(dataObject), System.Text.Encoding.Default, "application/json")
			};
			_client.DefaultRequestHeaders.Clear();
			_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
			HttpResponseMessage postResponse = _client.SendAsync(req).Result;
			if (postResponse.IsSuccessStatusCode)
			{
				dynamic resultObject = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "resultObject", resultObject } }, messageProperties: new object[] { "UnassignUser", $"Post result - {userPersonaId}" });
            }
            
			return "";
        }
		#endregion
		#endregion

		#region Private Methods  
		/// <summary>
		/// Get Client Token
		/// </summary>
		private void GetToken()
		{
			try
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Beginning" });
                string unifiedLoginResearchAppScope = "UnifiedLoginResearchApp";
				ObjectCache tokenCache = MemoryCache.Default;

				// Get token values from cache
				_accessToken = tokenCache["access_token_UnifiedLoginResearchApp"] as string;
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "accessToken", _accessToken } }, messageProperties: new object[] { "GetToken", "Beginning" });

                if (string.IsNullOrEmpty(_accessToken))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Null cache value. Getting new token" });
					string tokenUri = ConfigReader.GetIssuerUri;

					WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "tokenUri", $"{tokenUri}/connect/token" } }, messageProperties: new object[] { "GetToken", "Beginning" });

					using var tokenHttpClient = new HttpClient();
					var tokenResponse = tokenHttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
					{
						Address = $"{tokenUri}/connect/token",
						ClientId = "UnifiedLoginResearchApp",
						ClientSecret = _UnifiedLoginResearchApplicationClientSecret,
						Scope = unifiedLoginResearchAppScope
					}).Result;

					if (tokenResponse.IsError)
					{
						throw new Exception($"ResearchApplication - ManageResearchApplication.GetToken - Received null or empty token. {tokenResponse.Error}");
					}

					var cachePolicy = new CacheItemPolicy
					{
						// Expier cache every after 9 minutes (assuming 10 min is token expiration time)
						AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
					};

					_accessToken = tokenResponse.AccessToken;

					tokenCache.Set("access_token_UnifiedLoginResearchApp", _accessToken, cachePolicy);
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "accessToken", _accessToken } }, messageProperties: new object[] { "GetToken", "Got token, received & populated cache with token value" });
                }
			}
			catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", $"Error. Reason: {ex.Message}" });
                throw new Exception($"Error in ResearchApplication - ManageResearchApplication.GetToken- {ex.Message}");
			}
		}

        /// <summary>
        /// Get Product Ids by Org
        /// </summary>        
        /// <returns></returns>
        private List<int> GetProductIdsByOrg()
        {
            ProductRepository pr = new ProductRepository();
            return (List<int>)pr.GetProductIdsByCompany(_userClaims.OrganizationRealPageGuid);
        }

        /// <summary>
        /// Used to Update  Research Application - (Blackbook) resource Role information for a user
        /// </summary>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="newRoleId">new RoleId</param>
        /// <param name="existingRoleId">Current RoleId</param>
        private void UpdateBlackBookRole(long userPersonaId, long editorPersonaId, long newRoleId, long existingRoleId)
		{
			// Delete existing roleId
			InsertResearchApplicationProductUserDB(userPersonaId, editorPersonaId, existingRoleId, true);

			//Insert new roleId to BlackBook
			InsertResearchApplicationProductUserDB(userPersonaId, editorPersonaId, newRoleId, false);
		}

		/// <summary>
		/// User Assign Product Property Role
		/// </summary>
		/// <param name="userProductPropertyRole">userProductPropertyRole object</param>
		/// <returns>UserAssignProductPropertyRole</returns>
		private UserAssignProductPropertyRole MapGbObjectToProduct(ResearchAppRoleAndPropertyList userProductPropertyRole)
        {
            var result = new UserAssignProductPropertyRole();

            if (userProductPropertyRole.RoleList != null && userProductPropertyRole.RoleList.Count > 0)
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
		/// Insert Research Application ProductUserDB
		/// </summary>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="editorPersonaId">The persona id of the user making the request</param>
		/// <param name="roleId">RoleId</param>
		/// <param name="deleteRole">Delete role?</param>
		private void InsertResearchApplicationProductUserDB(long userPersonaId, long editorPersonaId, long roleId, bool deleteRole = false)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertResearchApplicationProductUserDB", $"Calling DB to insert Property/Role assigned to user userPersonaId - {userPersonaId},  RoleId - {roleId} ,  isDelete - {deleteRole}" });

            //Inserting/ Deleting Role
            try
            {
                RepositoryResponse result = new RepositoryResponse();
                result = InsertAssignedUserRoleData(userPersonaId, roleId, deleteRole);

            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "InsertResearchApplicationProductUserDB", $"Error. userPersonaId - {userPersonaId}, RoleId - {roleId} ,  isDelete - {deleteRole} Reason: {ex.Message}" });
            }
        }

		/// <summary>
		/// Insert Assigned User RoleData
		/// </summary>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="roleID">RoleId</param>
		/// <param name="deleteRole">Delete Role?</param>
		/// <returns>RepositoryResponse object</returns>
		private RepositoryResponse InsertAssignedUserRoleData(long userPersonaId, long roleID, bool deleteRole = false)
        {
            RepositoryResponse result = new RepositoryResponse();
            IUserRoleRightRepository userRoleRightRepository = new UserRoleRightRepository();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserRoleData", $"Beginning userPersonaId - {userPersonaId}, RoleId - {roleID}" });

            result = userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId, roleID, _userClaims.UserId , deleteRole);
            if (result.Id < 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserRoleData", $"Unable to Insert record for user with userPersonaId - {userPersonaId}, RoleId - {roleID}" });
                return result;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserRoleData", $"Complete userPersonaId - {userPersonaId}, RoleId - {roleID}, result - {result.Id}" });
            return result;
        }

		/// <summary>
		/// Delete Assigned UserData
		/// </summary>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="productId">ProductId</param>
		/// <param name="propID">PropertyId</param>
		/// <param name="roleID">RoleId</param>
		/// <returns>RepositoryResponse object</returns>
		private RepositoryResponse DeleteAssignedUserData(long userPersonaId, int productId, long propID, long roleID)
        {
            ResearchApplicationRepository ocr = new ResearchApplicationRepository();
            RepositoryResponse result = new RepositoryResponse();
            int del = 1; // setting for Delete in DB
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserData", $"Beginning userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}" });

            result = ocr.InsertDelAssignedPropRoleToUserNew(userPersonaId, productId, propID, roleID, del);
            if (result.Id < 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserData", $"Unable to Delete record for user with userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}" });
                return result;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserData", $"Complete userPersonaId - {userPersonaId}, PropertyId - {propID}, RoleId - {roleID}, result - {result.Id}" });
            return result;
        }

		/// <summary>
		/// Merge Selected Roles With Greenbook
		/// </summary>
		/// <param name="allRoles">All roles</param>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <returns></returns>
		private ListResponse MergeSelRolesWithGreenbook(IList<ProductRole> allRoles, long userPersonaId)
        {
            // get roles from DB for ResearchApplication product
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeSelRolesWithGreenbook", $"Getting assigned user roles - personaId - {userPersonaId}" });
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

		#region ResearchApplicationDeleteUser
		/// <summary>
		/// Used when Posting to Research Application when the persona access to research tool is revoked
		/// </summary>
		private class ResearchApplicationDeleteUser
		{
			/// <summary>
			/// UserId
			/// </summary>
			[JsonProperty(PropertyName = "UserId")]
			public long UserId { get; set; }

			/// <summary>
			/// PersonaId
			/// </summary>
			[JsonProperty(PropertyName = "PersonaId")]
			public long PersonaId { get; set; }
		}
		#endregion
	}
}