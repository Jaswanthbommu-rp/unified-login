using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.ProductImplementation
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PortfolioManagement : StandardV1ProductIntegration, IManageProductIntegration
	{
		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		/// <param name="productType"></param>
		/// <param name="editorPersonaId"></param>
		/// <param name="subjectPersonaId"></param>
		/// <param name="userClaims"></param>
		public PortfolioManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims) : base((int)productType, editorPersonaId, subjectPersonaId, userClaims)
		{
		}

		public PortfolioManagement(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona, IProductInternalSettingRepository productInternalSettingRepository) :
			base((int)productType, editorPersonaId, subjectPersonaId, userClaims, injectedDataCollector, injectedManagePersona, productInternalSettingRepository)
		{ }

        #endregion

        #region Methods

        protected override void ApplyApiSecurity()
        {
			string tokenClientId = ProductInternalSettingList.First(a => a.Name.ToUpper() == "TOKENCLIENTID").Value;
			string tokenClientSecret = ProductInternalSettingList.First(a => a.Name.ToUpper() == "TOKENCLIENTSECRET").Value;
			string tokenIssueUri = ProductInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;

			string accessToken = GetPortfolioManagementAccessToken(tokenIssueUri, tokenClientId, tokenClientSecret);
			_httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
		}

		/// <summary>
		/// Get Portfolio Management AccessToken
		/// </summary>
		/// <param name="tokenIssueUri">Token Url</param>
		/// <param name="tokenClientId">Username</param>
		/// <param name="tokenClientSecret">Password</param>
		/// <returns>Access Token</returns>
		private string GetPortfolioManagementAccessToken(string tokenIssueUri, string tokenClientId, string tokenClientSecret)
		{
			string accessToken = string.Empty;
			try
			{
				HttpClient client = new HttpClient();
				client.SetBasicAuthentication(tokenClientId, tokenClientSecret);
				Dictionary<string, string> dictionary = new Dictionary<string, string>()
				{
					{
						"grant_type",
						"client_credentials"
					},
					{   "scope",
						""
					}
				};

				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenIssueUri + "/token")
				{
					Content = new FormUrlEncodedContent(dictionary)
				};
				HttpResponseMessage postResponse = client.SendAsync(request).Result;
				if (postResponse.IsSuccessStatusCode)
				{
					dynamic resultObject = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
					accessToken = resultObject.access_token;
				}
				return accessToken;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error in GetToken- {ex.Message}");
			}
		}

		/// <summary>
		/// Get GLOBAL product roles
		/// </summary> 
		public override ListResponse GetProductRoles(RequestParameter dataFilter, string apiQuery = null)
		{
			try
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

				// Get end point for global role
				var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
				baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "true");

				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}" });

				var roleList = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);

				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {roleList?.Count}" });

				if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });
					var user = GetProductUser();

					// map user roles
					if (user != null)
					{
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

						var userRoles = user.RoleList;
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
		/// Override used in Get Roles
		/// </summary> 
		public override IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
		{
			baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails.ProductUserName);

			return base.GetProductUser(baseUrlAndQuery);
		}

		/// <summary>
		/// Get Portfolio Properties+Roles
		/// </summary> 
		public override ListResponse GetProductProperties(RequestParameter dataFilter, string apiQuery = null)
		{
			IList<PortfolioRoleProperty> propertiesList = new List<PortfolioRoleProperty>();

			// get all properties
			var allProperties = GetPortfolioProperties().ToList();

			// get all property groups
			var allPropertyGroups = GetPortfolioPropertyGroups().ToList();

			// get all non-global roles
			var allPropertiesRoles = GetPortfolioPropertySpecificRoles().ToList();

			//build roles-entities object
			foreach (var role in allPropertiesRoles)
			{
				propertiesList.Add(new PortfolioRoleProperty(role, allProperties, allPropertyGroups));
			}

			// if user already exists in product
			if (SubjectUserDetails != null && !string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
			{
				var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);
				baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, SubjectUserDetails.ProductUserName);
				var user = GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery);

				MergePropertyRoles(propertiesList, user.PropertyRoleList);
			}

			return new ListResponse
			{
				Records = propertiesList.Cast<object>().ToList(),
				TotalRows = propertiesList.Count,
				RowsPerPage = 9999,
				ErrorReason = string.Empty,
				TotalPages = 1
			};
		}

		/// <summary>
		/// Get Portfolio Properties by GroupId
		/// </summary> 
		public ListResponse GetProductPropertiesByGroup(string groupId, RequestParameter dataFilter, string apiQuery = null)
		{
			// get properties by GroupId
			var propertiesList = GetPortfolioPropertiesByGroup(groupId).ToList();
			return new ListResponse
			{
				Records = propertiesList.Cast<object>().ToList(),
				TotalRows = propertiesList.Count,
				RowsPerPage = 9999,
				ErrorReason = string.Empty,
				TotalPages = 1
			};
		}

		/// <summary>
		/// Override this in product implementation if any product requires to create additional saml settings
		/// e.g. used in PAM
		/// </summary>
		protected override void CreateAdditionalSamlUserAttribute(long personaId, int productId, IntegrationProductUser productUser)
		{
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} userLoginName - {productUser.LoginName} ; PMC {productUser.CompanyId} . At beginning of the method." });

			_dataCollector.CreateSamlUserAttribute(personaId, productId, SamlAttributeEnum.PMCID, productUser.CompanyId);
		}

		/// <summary>
		/// Returns true if user exists in the product
		/// </summary> 
		protected override bool CheckUserExistInProduct(string newUserLoginName, string baseUrlAndQuery = null)
		{
			baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, newUserLoginName);

			var productUser = base.GetProductUser(baseUrlAndQuery, false);

			if (productUser != null && !string.IsNullOrEmpty(productUser.UserId))
			{
				return true;
			}

			return false;
		}

		#endregion

		#region Private Methods

		private void MergeUserRoles(IList<ProductRole> roleList, List<string> userRoles)
		{
			foreach (var role in roleList)
			{
				if (userRoles != null && userRoles.Contains(role.GetRoleId))
				{
					role.IsAssigned = true;
				}
			}
		}

		private IList<ProductProperties> GetPortfolioPropertiesByGroup(string propertyGroupId)
		{
			// Get end point for properties
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyByGroupEndpoint);
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, propertyGroupId);

			// call API
			return GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);
		}
		private IList<ProductPropertyGroups> GetPortfolioPropertyGroups()
		{
			// Get end point for properties
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupsEndpoint);
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
			// call API
			return GetResultFromApi<IList<ProductPropertyGroups>>(baseUrlAndQuery);
		}

		private IList<ProductRole> GetPortfolioPropertySpecificRoles()
		{
			// Get end point for property-role
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);  //http://wmu-books.asseteye.net/api/gandk/Roles?isGlobalRoles=true
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

			// call API
			return GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);
		}

		private IList<ProductProperties> GetPortfolioProperties()
		{
			// Get end point for properties
			var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
			baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

			// call API
			return GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);
		}

		private void MergePropertyRoles(IList<PortfolioRoleProperty> portfolioPropertyRoles, List<PAMRolePropertyList> userPropertyRoles)
		{
			if (userPropertyRoles != null) 
			{
				foreach (var role in userPropertyRoles)
				{
					foreach (var propRolesList in portfolioPropertyRoles.Where(x => x.GetRoleId == role.RoleId))
					{
						if (propRolesList.PropertiesList.Any(y => role.PropertyIds.Contains(y.GetPropertyId)))
						{
							propRolesList.IsAssigned = true;
							foreach(var property in propRolesList.PropertiesList.Where(z => role.PropertyIds.Contains(z.GetPropertyId)))
							{
								property.IsAssigned = true;
							}
						}
						if (propRolesList.GroupList.Any(y => role.PropertyGroupList.Contains(y.GetGroupId)))
						{
							propRolesList.IsAssigned = true;
							foreach (var group in propRolesList.GroupList.Where(z => role.PropertyGroupList.Contains(z.GetGroupId)))
							{
								group.IsAssigned = true;
							}
						}
					}
				}
			}
		}

		#endregion
	}

	public class PortfolioRoleProperty : ProductRole
	{
		public PortfolioRoleProperty(ProductRole role, List<ProductProperties> properties, List<ProductPropertyGroups> Groups)
		{
			SetName = role.GetName;
			SetRoleId = role.GetRoleId;
			PropertiesList = new List<ProductProperties>();
			GroupList = new List<ProductPropertyGroups>();
			PropertiesList.AddRange(properties.Select(a => new ProductProperties
			{
				SetPropertyId = a.GetPropertyId,
				SetName = a.GetName,
				PropertyType = a.PropertyType
			}));
			GroupList.AddRange(Groups.Select(a => new ProductPropertyGroups
			{
				SetGroupId = a.GetGroupId,
				SetGroupName = a.GetGroupName
			}));
		}
		public List<ProductProperties> PropertiesList { get; set; }
		public List<ProductPropertyGroups> GroupList { get; set; }
	}
}