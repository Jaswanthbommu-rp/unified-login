using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text.Json; // Added for manual token response parsing
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.ProductImplementation
{
    public class RenovationManager : StandardV1ProductIntegration, IManageProductIntegration
	{
		private IManagePersona _managePersona = new ManagePersona();
		private const string PRODUCT_SETTINGTYPE_STATUS = "ProductStatus";

		#region Ctor
		//private readonly List<ProductInternalSetting> _productIntegrationDetails;
		public RenovationManager(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base((int)productType, editorPersonaId, subjectPersonaId, userClaims)
		{ }

		public RenovationManager(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base((int)productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

		#endregion

		#region Methods

		protected override void ApplyApiSecurity()
        {
			var apiSecret = ProductInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value;
			var clientId = ProductInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value;
			var tokenIssueUri = ProductInternalSettingList.First(a => a.Name.ToUpper() == "TOKENENDPOINT").Value;

			var token = GetAccessToken(tokenIssueUri, clientId, apiSecret);
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		private string GetAccessToken(string tokenIssueUri, string clientId, string apiSecret)
		{
			try
			{
				string scope = ProductInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTSCOPE").Value;
				ObjectCache tokenCache = MemoryCache.Default;

				// Get token values from cache
				string accessToken = tokenCache[clientId] as string;

				if (string.IsNullOrEmpty(accessToken))
				{
					using var client = new HttpClient();
					var form = new Dictionary<string, string>
					{
						{"client_id", clientId},
						{"client_secret", apiSecret},
						{"grant_type", "client_credentials"},
						{"scope", scope}
					};
					var response = client.PostAsync(tokenIssueUri, new FormUrlEncodedContent(form)).GetAwaiter().GetResult();
					if (!response.IsSuccessStatusCode)
					{
						throw new Exception($"Received non-success status code {(int)response.StatusCode} when requesting token.");
					}
					var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (string.IsNullOrWhiteSpace(json))
					{
						throw new Exception("Received empty token response body.");
					}
					using var doc = JsonDocument.Parse(json);
					if (!doc.RootElement.TryGetProperty("access_token", out var tokenElement))
					{
						throw new Exception("'access_token' field not found in token response.");
					}
					accessToken = tokenElement.GetString();
					if (string.IsNullOrWhiteSpace(accessToken))
					{
						throw new Exception("Token value is null or empty after parsing response.");
					}
					var cachePolicy = new CacheItemPolicy
					{
						// Expire cache after 9 minutes (assuming 10 min token expiration)
						AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
					};
					tokenCache.Set(clientId, accessToken, cachePolicy);
				}
				return accessToken;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetToken- {ex.Message}");
			}
		}

		protected override void ApplySuperUserData(IntegrationProductUser productUser)
		{
			// super user related assignments
			List<string> roleNames = new List<string> { "SuperUserRoleId1", "SuperUserRoleId2" };
			productUser.Roles = GetProductInternalSettingValue(roleNames);
			productUser.Properties = new List<string>();
			productUser.PropertyGroups = new List<string>();
		}
		protected override bool CheckUserExistInProduct(string loginNameToCheck, string baseUrlAndQuery = null)
		{
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

			bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
			if (isCompanyIdRequiredToQuery)
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, loginNameToCheck);
			}
			else
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, loginNameToCheck);
			}
						
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckUserExistInProduct", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

			var productUser =  GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, false);

			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckUserExistInProduct", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}." });

			if (productUser != null && !string.IsNullOrEmpty(productUser.UserId))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get Product User API call
		/// </summary> 
		public override IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
		{
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

			bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
			if (isCompanyIdRequiredToQuery)
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails.ProductUserName);
			}
			else
			{
				baseUrlAndQuery = string.Format(baseUrlAndQuery, SubjectUserDetails.ProductUserName);
			}			

			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}." });

			return GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError);
		}

		/// <summary>
		/// Override this in product implementation if any product requires to create additional saml settings
		/// e.g. used in PAM
		/// </summary>
		protected override void CreateAdditionalSamlUserAttribute(long personaId, int productId, IntegrationProductUser productUser)
		{
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAdditionalSamlUserAttribute", $"Product {ProductId} userLoginName - {productUser.LoginName} ; PMC {productUser.CompanyId} . At beginning of the method." });

			_dataCollector.CreateSamlUserAttribute(personaId, productId, SamlAttributeEnum.PMCID, productUser.CompanyId);
		}

		/// <summary>
		/// Create or update product user
		/// Gets called from Product-Batch
		/// </summary> 
		public override string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
		{
			string result;

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method." });

			// Get product user object 
			var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

			if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.UserNoEmail)
			{
				newProductUser.LoginName = newProductUser.Email;
			}

			var productUser = GetBaseUserDataFromProduct(newProductUser.LoginName);
			//For Multi company user creation first check user data from product,if user data exists then do put else post

			if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName) && productUser == null)
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser." });
				// Create User
				result = CreateUser(newProductUser, out additionalParameters);
			}
			//Create Multi company with put
			else if(string.IsNullOrEmpty(SubjectUserDetails.ProductUserName) && productUser != null)
			{
				result = CreateMultiCompanyUser(newProductUser, out additionalParameters);
			}
			else
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"RenovationManager.CreateUpdateProductUser - Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser." });
				// Update user with Id/Login from product
				if (productUser != null)
				{
					newProductUser.UserId = productUser.UserId;
					newProductUser.LoginName = productUser.LoginName;
				}
				else 
				{
					newProductUser.UserId = SubjectUserDetails.ProductUserId;
					newProductUser.LoginName = SubjectUserDetails.ProductUserName;
				}
				
				result = UpdateUser(newProductUser, batchProcessType, out additionalParameters);
			}
			return result;
		}

		/// <summary>
		/// Unassign user form the product 
		/// </summary>
		/// <returns></returns>
		public override string UnassignUser()
		{
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method, calling DeleteUser()." });
			var productUserProfile = new IntegrationProductUser
			{
				UserId = SubjectUserDetails.ProductUserId,
				IsActive = false,
				CompanyId = CompanyInstanceSourceId,
				LoginName = SubjectUserDetails.ProductUserName,
				Email = SubjectUserDetails.Email,
				FirstName = SubjectUserDetails.FirstName,
				LastName = SubjectUserDetails.LastName,
				Properties = new List<string>(),
				Roles = new List<string>(),
				IsMigratedUser = false,
				IsAdminUser = false
			};
			// Delete / deactivate uer in the product
			var result = DeleteUser(productUserProfile);
			if (result.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns success, updating Greenboook status." });
				IManageUserLogin manageUserLogin = new ManageUserLogin();
				IUserLoginRepository userLoginRepository = new UserLoginRepository();
				var userLogin = manageUserLogin.GetUserLoginOnly(SubjectUserDetails.UserRealPageId);
				Persona persona = _managePersona.GetPersona(SubjectUserDetails.PersonaId);
				OrganizationStatus orgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, false);
				//var organizationList = userLoginRepository.ListOrganizationWithoutStatusByUserId(userLogin.UserId);
				//OrganizationStatus orgStatus = organizationList.FirstOrDefault(p => p.PartyId == persona.OrganizationPartyId);
				int statusValue = (int)UserUiStatusType.AccountHidden;
				//if user is disabled then set status to deactivated instead hidden
				if (orgStatus.Status.ToString().Equals(UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					statusValue = (int)UserUiStatusType.Deactivated;
				}
				// Update product status in green book
				_dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, statusValue);
				
				return string.Empty;
			}
			WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns fail - error - {result}" });
			return result.Content;
		}

		/// <summary>
		/// Get Product Roles
		/// </summary>
		/// <param name="dataFilter"></param>
		/// <param name="baseUrlAndQuery"></param>
		/// <returns></returns>
		public override ListResponse GetProductRoles(RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			try
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });
				if (string.IsNullOrEmpty(baseUrlAndQuery))
					baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}" });
				bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
				if (isCompanyIdRequiredToQuery)
					baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");
				var roleList = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {roleList?.Count}" });
				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });
					var user = GetProductUser(null, false);
					// map user roles
					if (user != null)
					{
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });
						var userRoles = user.Roles;
						MergeUserRoles(roleList, userRoles);
					}
				}
				if (roleList == null)
					throw new Exception("Null Role List.");
				return new ListResponse
				{
					Records = roleList.Cast<object>().ToList(),
					TotalRows = roleList.Count,
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" }, exception: ex);
				return new ListResponse()
				{
					ErrorReason = ex.Message,
					IsError = true
				};
			}
		}
		/// <summary>
		/// Returns Product Properties
		/// </summary> 
		public override ListResponse GetProductProperties(RequestParameter dataFilter, string baseUrlAndQuery = null)
		{
			try
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });
				if (string.IsNullOrEmpty(baseUrlAndQuery))
				{
					baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
					baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
				}
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}" });
				var propertyList = GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}" });
				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });
					var user = GetProductUser(null, false);
					// map user properties
					if (user != null && user.Properties != null)
					{
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });
						if (user != null && user.Properties != null)
						{
							var userProperties = user.Properties.ConvertAll(p => p.ToUpper());
							MergeUserProperties(propertyList, userProperties);
						}
					}
				}
				if (propertyList == null)
					throw new Exception("Null Property List.");
				return new ListResponse
				{
					Records = propertyList.Cast<object>().ToList(),
					TotalRows = propertyList.Count,
					RowsPerPage = 9999,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" }, exception: ex);
				return new ListResponse()
				{
					ErrorReason = ex.Message,
					IsError = true
				};
			}
		}

		/// <summary>
		/// Delete User - patch with isActive = false
		/// </summary> 
		protected virtual ApiResponse DeleteUser(IntegrationProductUser profile = null)
		{
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });
			// patch to se isActive flag to false
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}." });
			DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, profile);
			var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
			return integration.PutEntity<string>(profile);
		}

		protected override IntegrationProductUser GenerateProductUserObject(ProductUserRolePropertiesGroups userRolePropertiesRegion)
		{
			// Map user info
			var productUser = new IntegrationProductUser
			{
				LoginName = string.IsNullOrEmpty(SubjectUserDetails.LoginName) ? SubjectUserDetails.LoginName : GetUniqueProductLogin(SubjectUserDetails.LoginName),
				CompanyId = CompanyInstanceSourceId,
				FirstName = SubjectUserDetails.FirstName,
				LastName = SubjectUserDetails.LastName,
				Email = SubjectUserDetails.Email,
				Phone = SubjectUserDetails.PhoneNumber,
				IsActive = true,
				PropertyGroups = userRolePropertiesRegion.PropertyGroupList,
				Properties = userRolePropertiesRegion.PropertyList,
				Roles = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
				PropertyRoles = userRolePropertiesRegion.PropertyRoleList,
				OrganizationRoles = userRolePropertiesRegion.OrganizationRoleList,
				CanReceiveMonthlyReport = userRolePropertiesRegion.CanReceiveMonthlyReport,
				PropertyRoleList = userRolePropertiesRegion.RolePropertiesList,
				RoleList = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
				IsMigratedUser = true
			};

			if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.SuperUser)
			{
				ApplySuperUserData(productUser);
			}

			return productUser;
		}

		protected List<string> GetProductInternalSettingValue(List<string> settingName)
		{
			// Get product setting value
			var settingValue = ProductInternalSettingList.Where(a => settingName.Contains(a.Name)).Select(b => b.Value).ToList(); 
			return settingValue;
		}

		/// <summary>
		/// Create a user in the product
		/// </summary>
		protected override string CreateMultiCompanyUser(IntegrationProductUser productUser, out List<AdditionalParameters> additionalParameters)
		{
			additionalParameters = new List<AdditionalParameters>();

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}." });

			// dump api info
			DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);

			var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
			var result = integration.PutEntity<IntegrationProductUser>(productUser);

			if (result.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Geenbook mapping." });

                // map product user in green book
                _dataCollector.CreateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser);

                // OPTIONAL - If product needs more attributes than userid or loginName then override in the product (e.g. PAM uses)
                CreateAdditionalSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser);

				return string.Empty;
			}

			WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. result received - {result}." });

			return result.Content;
		}
		#endregion
		#region private
		public  override IntegrationProductUser GetBaseUserDataFromProduct(string loginNameToCheck, string baseUrlAndQuery = null)
		{
			if (string.IsNullOrEmpty(baseUrlAndQuery))
				baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserExistEndpoint);

			baseUrlAndQuery = string.Format(baseUrlAndQuery, loginNameToCheck);			

			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetBaseUserDataFromProduct", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

			var productUser = GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, false);

			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetBaseUserDataFromProduct", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}." });

			return productUser;
		}
		#endregion
	}
}
