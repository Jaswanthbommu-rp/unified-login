using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.MarketingCenter;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.ResponseObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;
using MC = UnifiedLogin.SharedObjects.Product.MarketingCenter;
using Right = UnifiedLogin.SharedObjects.Product.MarketingCenter.Right;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    /// Marketing Center
    /// </summary>
    public class ManageProductMarketingCenter : ManageProductBase, IManageProductMarketingCenter
	{
		private string _username;
		private string _password;
		private string _marketingCenterApiSourceID;		
		private DefaultUserClaim _userClaims;
        private HttpClient _httpClient;

        private const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
        private const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
        private const string ROLE_ASSIGN = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
        private const string ROLE_UNASSIGN = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";
		private const string PRODUCT_ROLE_CREATE = "{\"action\":\"Created Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLE_UPDATE = "{\"action\":\"Updated Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLE_DELETE = "{\"action\":\"Deleted Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLENAME_UPDATE = "{\"action\":\"Updated Role Name\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLEDESCRIPTION_UPDATE = "{\"action\":\"Updated Role Description\",\"value\":\"RoleName\"}";
        #region Ctor
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="userClaims">The RealPageId of the editor</param>
        public ManageProductMarketingCenter(DefaultUserClaim userClaims) : base((int)ProductEnum.MarketingCenter, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
			_editorRealPageId = userClaims.UserRealPageGuid;
			_blueBook = new Logic.ManageBlueBook(userClaims);
			_userClaims = userClaims;
			
			_productUrl = _productInternalSettingList.First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
			_marketingCenterApiSourceID = _productInternalSettingList.First(a => a.Name.Equals("MarketingCenterApiSourceID", StringComparison.OrdinalIgnoreCase)).Value;
			_username = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.Equals("APIUSERNAME", StringComparison.OrdinalIgnoreCase)).Value));
			_password = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.Equals("APIPASSWORD", StringComparison.OrdinalIgnoreCase)).Value));
			_client.BaseAddress = new Uri(_productUrl);
			_client.SetBasicAuthentication(_username, _password);
			var credCache = new CredentialCache();
			credCache.Add(new Uri(_productUrl), "Digest", new NetworkCredential(_username, _password));
			var HttpHandler = new HttpClientHandler();
			HttpHandler.Credentials = credCache;
			_httpClient = new HttpClient(HttpHandler);
			_httpClient.BaseAddress = new Uri(_productUrl);
			_httpClient.SetBasicAuthentication(_username, _password);
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaims"></param>
        /// <param name="httpMessageHandler"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="samlRepository"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="repository"></param>
        public ManageProductMarketingCenter(Guid editorRealPageId, DefaultUserClaim userClaims, HttpMessageHandler httpMessageHandler, IProductInternalSettingRepository productInternalSettingRepository,
			IManagePersona managePersona, ISamlRepository samlRepository, IManageBlueBook manageBlueBook, IProductRepository productRepository, IRepository repository)
			: base((int)ProductEnum.MarketingCenter, userClaims, repository, httpMessageHandler)
		{
			_editorRealPageId = editorRealPageId;
			_messageHandler = httpMessageHandler;
			_productInternalSettingRepository = productInternalSettingRepository;
			_managePersona = managePersona;
			_samlRepository = samlRepository;
			_blueBook = manageBlueBook;
			_userClaims = userClaims;
			_productRepository = productRepository;
			_productUrl = _productInternalSettingList.First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
			_marketingCenterApiSourceID = _productInternalSettingList.First(a => a.Name.Equals("MARKETINGCENTERAPISOURCEID", StringComparison.OrdinalIgnoreCase)).Value;
			_username = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.Equals("APIUSERNAME", StringComparison.OrdinalIgnoreCase)).Value));
			_password = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.Equals("APIPASSWORD", StringComparison.OrdinalIgnoreCase)).Value));

			_httpClient = new HttpClient(httpMessageHandler);
        }
		#endregion

		#region Public methods
		/// <summary>
		/// Used to get roles for Marketing Center
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			ListResponse result = new ListResponse();
			IList<MC.Role> rolesList = new List<MC.Role>();
			Dictionary<string, object> logData = new Dictionary<string, object>();

			result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);

			try
			{
				CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
				string marketingCompanyId = company.CompanyInstanceSourceId;
				
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Found blue book company source id {marketingCompanyId}" });
				var url = _productUrl + $"/external/company/{marketingCompanyId}/contact/roles";
				logData = new Dictionary<string, object> { { "url", url } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRoles", "Posting to url" });
                var response = _httpClient.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;

					rolesList = JsonConvert.DeserializeObject<IList<MC.Role>>(jsonContent);
					if (rolesList == null) { rolesList = new List<MC.Role>(); }

					logData = new Dictionary<string, object> { { "rolesList", rolesList } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRoles", "Got response" });

                    IList<ProductRole> list = rolesList.ToGBRoles();
					if (list == null) { list = new List<ProductRole>(); }

					// need to do a filter on the result
					if (userPersonaId != 0)
					{
						// merge the given user details with the list
						MC.MarketingCenterUserDetails mUser = GetUserDetails();
						if (mUser == null)
						{
							WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error looking for user. userPersonaId={userPersonaId}" });
                            return new ListResponse() { IsError = true, ErrorReason = "User not found" };
						}
						logData = new Dictionary<string, object>();
						logData.Add("mUser", mUser);
						WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRoles", "Looking for role for user" });

                        if (list.Any(a => a.ID == mUser.ContactRoleId.ToString()))
						{
							ProductRole pr = (from a in list where a.ID == mUser.ContactRoleId.ToString() select a).FirstOrDefault();
							if (pr != null)
							{
								pr.IsAssigned = true;
								WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Found role for user. Role: {pr.Name}" });
                            }
						}
					}
					logData = new Dictionary<string, object>();
					logData.Add("list", list);
					WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRoles", "Returning role list" });

                    result = new ListResponse()
					{
						Records = list.Cast<object>().ToList(),
						TotalRows = list.Count,
						RowsPerPage = list.Count,
						TotalPages = 1,
						ErrorReason = ""
					};
				}
				else
				{
					result.IsError = true;
					result.ErrorReason =  CommonMessageConstants.RoleErrorMessage;
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Error " + response.Content.ReadAsStringAsync().Result });
                }
			}
			catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRoles", $"Error. {ex.Message}" });
                result = new ListResponse();
				result.IsError = true;

				if (ex is BlueBookException)
				{
					result.ErrorReason = ex.Message;
				}
				else
				{
					result.ErrorReason = CommonMessageConstants.RoleErrorMessage;
				}
			}
			return result;
		}

		/// <summary>
		/// Used to get properties for Marketing Center
		/// </summary>
		/// <param name="editorPersonaId">The persona id of the user making the request</param>
		/// <param name="userPersonaId">The persona id of the user being changed</param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			ListResponse result = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			Dictionary<string, bool> allProperties = new Dictionary<string, bool>();
			result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (result.IsError) { return result; }

			try
			{
				string marketingCenterCompanyId = "";

				// get the PMCID from BlueBook because the user doesn't have the PMCID for Marketing Center yet
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Getting info from BlueBook.GetCompanyMap" });
                IList<CustomerCompanyMap> companyMap = _blueBook.GetCompanyMap(_editorPersona.Organization.RealPageId, _editorPersona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.MarketingCenter, domain: _editorPersona.Organization.OrganizationDomain.Name);
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Done getting info from BlueBook.GetCompanyMap" });
                if (companyMap != null && companyMap.Count > 0 && companyMap.Any(a => a.Source.Equals(BlueBookProductConstants.MarketingCenter, StringComparison.OrdinalIgnoreCase)))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Getting PMC ID from BlueBook result" });
                    marketingCenterCompanyId = companyMap.First(a => a.Source.Equals(BlueBookProductConstants.MarketingCenter, StringComparison.OrdinalIgnoreCase)).CompanyInstanceSourceId;
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Found PMC ID from BlueBook result: {marketingCenterCompanyId}" });

                }

				//companyInstanceId = 779893; // LeaseStar id 438
				IList<ProductPropertyMap> propertyList = new List<ProductPropertyMap>();
				var url = _productUrl + $"/external/properties?companyId= { marketingCenterCompanyId} ";
				logData = new Dictionary<string, object> { { "url", url } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetProperties", "Posting to url" });
                var response = _httpClient.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;
					propertyList = JsonConvert.DeserializeObject<IList<ProductPropertyMap>>(jsonContent);

					IList<ProductProperty> list = propertyList.ToGBProperties();

					if (list == null) { list = new List<ProductProperty>(); }

					// need to do a filter on the result
					if (userPersonaId != 0)
					{
						// merge the given user details with the list
						MarketingCenterUserDetails mUser = GetUserDetails();
						if (mUser == null)
						{
							WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Error looking for user. userPersonaId={userPersonaId}" });
                            return new ListResponse() { IsError = true, ErrorReason = "User not found" };
						}
						logData = new Dictionary<string, object>();
						logData.Add("mUser", mUser);
						WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetProperties", "Looking for properties for user" });
                        // if a user record exists
                        if (mUser.AssignedProperties != null)
						{
							logData = new Dictionary<string, object>();
							List<MC.Property> prop = mUser.AssignedProperties;
							int i = 0;
							foreach (MC.Property p in prop)
							{
								if (list.Any(a => a.ID == p.Id.ToString()))
								{
									ProductProperty pp = (from a in list where a.ID == p.Id.ToString() select a).FirstOrDefault();
									if (pp != null)
									{
										pp.IsAssigned = true;
									}
								}
								else
								{
									// if the property wasn't found, we need to add it to the list.
									list.Add(new ProductProperty() { Name = p.Name, ID = p.Id.ToString(), IsAssigned = p.Active, State = p.Address.StateCode, Street1 = p.Address.Address1, City = p.Address.CityName, Zip = p.Address.PostalCode });
									logData.Add("Property" + i, p.Name);
									i++;
								}
							}
							WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetProperties", "Adding extra property" });
                        }
						//allProperties.Add("allProperties", mUser.AssignNewProperty);
                        allProperties.Add("IsAssignedNewPropertyByDefault", mUser.AssignNewProperty);
                    }

					logData = new Dictionary<string, object> { { "list", list } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetProperties", "Returning property list" });

                    result = new ListResponse()
					{
						Records = list.Cast<object>().ToList(),
						TotalRows = list.Count,
						RowsPerPage = list.Count,
						TotalPages = 1,
						ErrorReason = "",
						Additional = allProperties
					};
				}
				else
				{
					result.IsError = true;
					result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Error. {response.Content.ReadAsStringAsync().Result}" });
                }
			}
			catch (Exception ex)
			{
				result.IsError = true;

				if (ex is BlueBookException)
				{
					result.ErrorReason = ex.Message;
				}
				else
				{
					result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
				}
				WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetProperties", $"Error. {ex.Message}" });
            }
			return result;
		}

		/// <summary>
		/// Unassign User
		/// </summary>
		public string UnassignUser(long editorPersonaId, long userPersonaId)
		{
			string response = string.Empty;
			ListResponse listResponse = new ListResponse();
			listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError)
			{
				return listResponse.ErrorReason;
			}

			if (!IsUserIdValid(Convert.ToInt64(_editorProductUserId)))
			{
				response = $"ManageMarketingCenterUser.UnassignUser - Invalid admin userId: {_editorProductUserId}";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Invalid admin userId: {_editorProductUserId}" });
            }
			else
			{
				bool status = SetMarketingCenterUserStatus(false, _productUserId);
				if (status)
				{
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"userPersonaId: {userPersonaId}" });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
				}
				else
				{
					response = $"ManageMarketingCenterUser.UnassignUser errored- userPersonaId: {userPersonaId}";
				}
			}

			return response;
		}
                
		/// <summary>
		/// Update User Profile
		/// </summary>
		public string UpdateUserProfile(long editorPersonaId, long userPersonaId)
		{
			try
			{
                var logData = new Dictionary<string, object>();
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					return listResponse.ErrorReason;
				}

                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Begin update user profile" });
                string productLoginName = "";

				Persona userPersona = _managePersona.GetPersona(userPersonaId);
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Got persona info" });
                Guid realPageId = userPersona.RealPageId;

				Person person = _managePerson.GetPerson(realPageId);
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Got person info" });

                UserLoginOnly userLogin = new UserLoginOnly();
				userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

				IList<UserOrganization> userPersonaOrganizationList = _manageUserLogin.GetUserPersonaOrganization(userLogin.LoginName);
				bool isRegularUserNoEmail  = IsRegularUserNoEmail(userPersonaId);

				// get the email address
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Begin get user email address" });
                string userEmailAddress = "";
				string userLeadEmailAddress = "";
				ManageElectronicAddress _manageElectronicAddress = new ManageElectronicAddress();
				IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Got list of electronic address" });
                if (_addresses != null)
				{
					if (_addresses.Any(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)))
					{
						userEmailAddress = (from a in _addresses where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
                        WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"Found email address. {userEmailAddress}" });
                    }
				}
				if (string.IsNullOrEmpty(userEmailAddress))
				{
					userEmailAddress = userLogin.LoginName;
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Using login name for email address" });
                }

				if (isRegularUserNoEmail)
				{
					userLeadEmailAddress = userEmailAddress;
				}
				// verify email address looks valid, will fail if not
				WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"User Type : {userPersona.UserTypeId}" });
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"Validating email address. Email: {userLogin.LoginName}" });
                if (userPersona.UserTypeId == (int)UserTypeConstants.RegularUserNoEmail)
				{
                    userEmailAddress = _productUsername;
                }
				else
				{
					userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
				}

                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"Validated email address. Email: {userEmailAddress}" });
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"Product User Name : {_productUsername}" });

                productLoginName = _productUsername;
				//If the User's LoginName changed in the PrimaryOrganization then update it in the Product
				if ((userPersonaOrganizationList.ToList().Any(o => o.PrimaryOrganization.Equals(true)
					&& o.OrganizationPartyId.Equals(userPersona.OrganizationPartyId))) 
					&& (!_productUsername.Equals(userEmailAddress, StringComparison.OrdinalIgnoreCase)))
				{
					productLoginName = userEmailAddress;
				}		
				
				MarketingCenterUserDetails mUser = GetUserDetails();
				if (mUser == null)
				{
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"Error looking for user. userPersonaId={userPersonaId}" });
                    return "User not found in product";
				}

                var mcUser = new MC.MarketingCenterUser()
                {
                    CompanyId = mUser.CompanyId,
                    ContactRoleId = mUser.ContactRoleId,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    EmailAddress = productLoginName,
                    LeadEmailAddress = userLeadEmailAddress,
                    WelcomeEmailSent = true,
                    AssignNewProperty = mUser.AssignNewProperty
				};

				var url = _productUrl + $"/external/contact/{_productUserId}?sourceid={_editorProductUserId}";
				logData = new Dictionary<string, object>
                {
                    { "url", url },
                    { "mcuser", mcUser }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Update user profile" });
                var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;

				if (response.IsSuccessStatusCode)
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"StartUpdate user SAMLAttribute User_email={productLoginName}" });
                    UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", "Update user SAMLAttribute User_email success. Saved user id" });

                    WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
					return string.Empty;
				}

                var errorContent = string.Empty;
                try
                {
                    errorContent = response.Content.ReadAsStringAsync().Result;
                    logData.Add("errorContent", errorContent);
                }
                catch
                {/*Ignored*/ }
                WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"Error for user with editorPersona id - {editorPersonaId}" });
                return $"There was a problem updating user profile for user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
            }
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateUserProfile", $"Error. {ex.Message}, for user with editorPersona id - {editorPersonaId}" });
                return $"Error - {ex.Message}";
			}
		}

        /// <summary>
        /// Updated to create/update a user in Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="RoleList">The role id to assign the user</param>
        /// <param name="PropertyList">The list of property id's to assign to the user</param>
        /// <param name="IsAssignedNewPropertyByDefault">For UI toggle Assign new property by default selected</param>
		/// <param name="additionalParameters"></param>
        /// <returns></returns>
        public string ManageMarketingCenterUser(long editorPersonaId, long userPersonaId, List<int> RoleList, List<string> PropertyList, bool IsAssignedNewPropertyByDefault, out List<AdditionalParameters> additionalParameters)
		{
			Dictionary<string, object> logData = new Dictionary<string, object>();
			List<int> mcProperties = new List<int>();
            additionalParameters = new List<AdditionalParameters>();
            ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError)
			{
				return listResponse.ErrorReason;
			}

			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Begin create/update user" });
            string productLoginName = "";
			
			Persona userPersona = _managePersona.GetPersona(userPersonaId);
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Got persona info" });
            Guid realPageId = userPersona.RealPageId;

			IC.IPerson person = _managePerson.GetPerson(realPageId);
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Got person info" });

            IUserLoginOnly userLogin = new UserLoginOnly();
			userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
			
            IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);
            userPersona.Organization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);

            var personaOrganization = userPersona.Organization;
			bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

			// get the email address
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Begin get user email address" });
            string userEmailAddress = "";
			string userLeadEmailAddress = "";
			ManageElectronicAddress _manageElectronicAddress = new ManageElectronicAddress();
			IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Got list of electronic address" });
            if (_addresses != null)
			{
				if (_addresses.Any(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)))
				{
					userEmailAddress = (from a in _addresses where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Found email address. {userEmailAddress}" });
                }
			}
			if (IsRegularUserNoEmail(userPersonaId))
			{
				userLeadEmailAddress = userEmailAddress;
				if (string.IsNullOrEmpty(userLeadEmailAddress))
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "No Valid Notification Email Provided" });
                    // write an error
                    return "ManageMarketingCenterUser - Error. No Valid Notification Email Provided";
				}

                userEmailAddress = _productUsername;
                if(string.IsNullOrEmpty(userEmailAddress))
                    userEmailAddress = !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(userLogin.LoginName) ? string.Concat(userLogin.LoginName, "@NoReply.com") : userLogin.LoginName;

            }
			else
			{
				if (string.IsNullOrEmpty(userEmailAddress))
				{
					userEmailAddress = userLogin.LoginName;
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Using login name for email address" });
                }
			}

			// verify email address looks valid, will fail if not
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Validating email address. Email: {userEmailAddress}" });
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Validated email address. Email: {userEmailAddress}" });

            if (string.IsNullOrEmpty(_productUsername))
			{
				productLoginName = userEmailAddress;
			}
			else
			{
				productLoginName = _productUsername;
			}

			bool isSuperUser = IsSuperUser(userPersonaId);

			if (!isSuperUser && (RoleList.Count == 0 || PropertyList.Count == 0))
			{
				if (RoleList.Count == 0)
				{
					WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.There are no roles active in this company. Please contact the implementation team for this product.");
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
				}

				if (PropertyList.Count == 0)
				{
					WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.There are no properties active in this company. Please contact the implementation team for this product.");
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
				}

				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Create user error. PropertyList.Count={PropertyList.Count} , RoleList.Count={RoleList.Count}" });
                // stop batch process
                return ProductBatchStatusType.Stop.ToString();
			}

			// get the current roles for the company
			ListResponse roleListResponse = GetRoles(editorPersonaId, 0, null);
			List<ProductRole> roleList = roleListResponse.Records.Cast<ProductRole>().ToList();

			// get the current properties for the company
			ListResponse propertyListResponse = GetProperties(editorPersonaId, 0, null);
			List<ProductProperty> propertyList = propertyListResponse.Records.Cast<ProductProperty>().ToList();
			bool allPropertiesSelected = false;

			//Used for Activity logs details
            var productUserBeforeUpdate = GetUserDetails();

            int roleId = 0;
			if (isSuperUser)
			{
				// get the role id for Corporate Operations and assign it to the new super user
				if (roleList.Any(a => a.Name.Equals("CORPORATE OPERATIONS", StringComparison.OrdinalIgnoreCase)))
				{
					ProductRole p = (from a in roleList where a.Name.Equals("CORPORATE OPERATIONS", StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
					if (p != null)
					{
						roleId = Convert.ToInt32(p.ID);
					}
				}
				if (roleId == 0)
				{
					logData = new Dictionary<string, object> { { "roleList", roleList } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "Error getting role id for Corporate Operations for super user" });
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.There is no Corporate Operations role active in this company.Please contact the implementation team for this product.");
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
					// stop batch process
					return ProductBatchStatusType.Stop.ToString();
				}

				// get all properties and assign it to the new super user
				
				mcProperties.AddRange((from a in propertyList select Convert.ToInt32(a.ID)).ToArray());
			}
			else
			{
				if (roleList.Any(a => a.ID == RoleList[0].ToString()))
				{
					roleId = RoleList[0];
				}
				else
				{
					logData = new Dictionary<string, object> { { "roleList", roleList } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", $"Error getting role id {RoleList[0]} from role list" });
                    // write an error
                    return $"Role id {RoleList[0]} not found";
				}

                foreach (var prop in PropertyList)
                {
                    mcProperties.Add(Convert.ToInt32(prop));
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Using product login name. productUsername: {_productUsername}" });
            MC.MarketingCenterUser mcUser = new MC.MarketingCenterUser();
			CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

			if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Error looking for company id in bluebook" });
                // write an error
                return "Company Setup Error: Please Contact Support.";
			}

			// get a login name that isn't in use for the new user
			if (string.IsNullOrEmpty(_productUsername) && (isExternalUser || IsRegularUserNoEmail(userPersonaId)))
			{
                if(!IsRegularUserNoEmail(userPersonaId))
                    userLeadEmailAddress = userLogin.LoginName;

				userEmailAddress = GetMCUniqueUserName(person.FirstName, person.LastName);
				if (string.IsNullOrEmpty(userEmailAddress) )
				{
					return "An error occurred. Unable to get username.";
				}
			}

			mcUser = new MC.MarketingCenterUser()
			{
				CompanyId = Convert.ToInt32(company.CompanyInstanceSourceId),
				ContactRoleId = roleId,
				ContactRoleName = null,
				FirstName = person.FirstName,
				LastName = person.LastName,
				EmailAddress = userEmailAddress,
				LeadEmailAddress = userLeadEmailAddress,
				WelcomeEmailSent = true, // send true so it doesn't send an email
				AssignUnassignProperties = true
			};

			mcUser.AssignPropertyIds = mcProperties;
            mcUser.AssignNewProperty = IsAssignedNewPropertyByDefault;

            if (isSuperUser)
            {
                allPropertiesSelected = true;
                mcUser.AssignNewProperty = true;
            }

            logData = new Dictionary<string, object> { { "mcUser", mcUser } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "User details" });

            if (string.IsNullOrEmpty(_productUsername))
			{
				// create user
				try
				{
					mcUser.AssignAllProperties = allPropertiesSelected;

					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);

					var url = _productUrl + $"/external/contact?sourceid={(string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId)}";

					logData = new Dictionary<string, object> { { "url", url }, {"userJson", JsonConvert.SerializeObject(mcUser) } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "Create user" });
                    var response = _httpClient.PostAsJsonAsync(url, mcUser).Result;

					if (response.IsSuccessStatusCode)
					{
						var userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
						logData = new Dictionary<string, object> { { "userResult", userResult } };
                        WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "Create user. Got result from marketing center" });
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, userEmailAddress);
						// now the id!
						long newid = userResult.id;
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Create user. newid={newid}" });
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid.ToString());
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Create user. Saved user id" });
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Create user. Set product status to Success" });

                        //update user migration status
                        // Update UL flag in product
                        var updateResponse = UpdateUsersMigrationStatus(editorPersonaId, new List<MigrateUser>
						{
							new MigrateUser
							{
								UnifiedLoginUserName = userEmailAddress,
								UserId = newid.ToString(),
								UsingUnifiedLogin = true,
								LeadEmailAddress = userLeadEmailAddress
							}
						});

						if (!updateResponse.Status)
							return updateResponse.Message;
					}
					else
					{
                        // write an error
                        var userResult = response.Content.ReadAsStringAsync().Result;
                        logData = new Dictionary<string, object> { { "userResult", userResult } };
                        WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "ManageMarketingCenterUser", "Create user. Set product status to Error" });
                        return ParseErrorPosting(response, "Create", editorPersonaId, userPersonaId);
					}
				}
				catch (Exception ex)
				{
					// write an error
					WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageMarketingCenterUser", $"Create user errored. {ex.Message}" });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageMarketingCenterUser", "Create user errored. Set product status to Error" });
                    return "There was a problem creating the user";
				}
			}
			else
			{
				// update user
				try
				{
					if (!isSuperUser)
					{
						//if (!allPropertiesSelected)
						//{
							// get the users current property list and get everything that is currently assigned so it can be removed before adding again
							ListResponse currentPropertyListResponse = GetProperties(editorPersonaId, userPersonaId, null);
							List<ProductProperty> currentPropertyList = currentPropertyListResponse.Records.Cast<ProductProperty>().ToList();
							List<int> removePropertyList = new List<int>();

							if (currentPropertyList.Any(a => a.IsAssigned.Value))
							{
								foreach (ProductProperty pp in currentPropertyList)
								{
									if (pp.IsAssigned.Value)
									{
										if (mcUser.AssignPropertyIds.Contains(Convert.ToInt32(pp.ID)))
										{
											mcUser.AssignPropertyIds.Remove(Convert.ToInt32(pp.ID));
										}
										else
										{
											removePropertyList.Add(Convert.ToInt32(pp.ID));
										}
									}
								}
								mcUser.UnassignPropertyIds = removePropertyList;
							}
						//}
						mcUser.AssignAllProperties = allPropertiesSelected;
					}
                    if(isExternalUser)
                    {
                        mcUser.EmailAddress = _productUsername;
                        mcUser.LeadEmailAddress = userEmailAddress;
                    }
					var url = "";
					if (allPropertiesSelected)
					{
						url = _productUrl + $"/external/contact/{_productUserId}?sourceid={(string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId)}&assignAllProperties=true";
					}
					else
					{
						url = _productUrl + $"/external/contact/{_productUserId}?sourceid={(string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId)}&unassignAllProperties=false";
					}

                    logData = new Dictionary<string, object> { { "url", url }, { "userJson", JsonConvert.SerializeObject(mcUser) } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "Update user" });

					var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;

					if (response.IsSuccessStatusCode)
					{
						UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
						var userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
						logData = new Dictionary<string, object> { { "userResult", userResult } };
                        WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "Update user. Got result from marketing center." });
                        long newid = userResult.id;
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Update user. newid={newid}" });
                        UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid.ToString());
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Update user success. Saved user id" });

                        if (string.IsNullOrEmpty(_editorProductUserId) || !IsUserIdValid(Convert.ToInt64(_editorProductUserId)))
						{
							string message = $"ManageMarketingCenterUser.ManageMarketingCenterUser - Invalid admin userId: {_editorProductUserId}";
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Invalid admin userId: {_editorProductUserId}" });
                        }
						bool status = SetMarketingCenterUserStatus(true, newid.ToString());
					}
					else
					{
						return ParseErrorPosting(response, "Update", editorPersonaId, userPersonaId);
					}
				}
				catch (Exception ex)
				{
					// write an error
                    WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageMarketingCenterUser", $"Update user errored. {ex.Message}" });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageMarketingCenterUser", "Update user errored. Set product status to Error." });
                    return "There was a problem updating the user";
				}
			}
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Done create/update user" });

			try
			{
				//Activity Log for Roles
				if (mcUser.ContactRoleId != productUserBeforeUpdate?.ContactRoleId)
				{
					//Create user case
					if (productUserBeforeUpdate != null)
					{
						var removedRoles = roleList
								.Where(f => productUserBeforeUpdate.ContactRoleId == Convert.ToInt32(f.ID))
								.Select(f => new AdditionalParameters { Key = "Marketing Center Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", f.Name) })
								.ToList();

						additionalParameters.AddRange(removedRoles);
					}

					var assignedRoles = roleList
							.Where(f => mcUser.ContactRoleId == Convert.ToInt32(f.ID))
							.Select(f => new AdditionalParameters { Key = "Marketing Center Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", f.Name) })
							.ToList();

					additionalParameters.AddRange(assignedRoles);
				}

				//Activity Log for Properties
				if (mcUser.AssignPropertyIds != null)
				{
					var assignedProp = propertyList
							.Where(f => mcUser.AssignPropertyIds.Contains(Convert.ToInt32(f.ID)))
							.Select(f => new AdditionalParameters { Key = "Marketing Center Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.Name) })
							.ToList();

					additionalParameters.AddRange(assignedProp);
				}
				if (mcUser.UnassignPropertyIds != null)
				{
					var assignedProp = propertyList
							.Where(f => mcUser.UnassignPropertyIds.Contains(Convert.ToInt32(f.ID)))
							.Select(f => new AdditionalParameters { Key = "Marketing Center Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", f.Name) })
							.ToList();

					additionalParameters.AddRange(assignedProp);
				}
			}
			catch(Exception e)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ExtractActivityDetailLogs", $"Error building Activity logs for MarketingCenter. editorPersonaId: {editorPersonaId}, productUserPersonaId: {userPersonaId}" }, exception: e);
            }

            return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="username"></param>
		/// <param name="productUserId"></param>
		/// <param name="isActive"></param>
		/// <returns></returns>
		public bool ChangeUserStatus(long editorPersonaId, string username, string productUserId, bool isActive = false)
		{
			_productUserId = productUserId;
			ListResponse listResponse = new ListResponse();
			listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
			if (listResponse.IsError) { return false; }

			int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
			if (companyInstanceSourceId == 0)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                return false;
			}
			try
			{
				return SetMarketingCenterUserStatus(isActive, _productUserId);
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {companyInstanceSourceId}|{username} by editorPersonaId = {editorPersonaId}. {ex.Message}" });
                return false;
			}
		}
        #endregion

        #region Role Right Setup
        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <returns></returns>
        public ListResponse GetRolesCount(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            try
            {
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                response = GetRolesCountDetails(editorPersonaId);
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = ex.Message;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRolesCount", $"Error {ex.Message}" });
            }
            return response;
        }

        /// <summary>
        /// Used to get rights for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <returns></returns>
        public ListResponse GetRights(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            try
            {
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                response = GetRightsDetails(editorPersonaId);
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = ex.Message;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRights", $"Error {ex.Message}" });
            }
            return response;
        }

        /// <summary>
        /// Used to Delete a role in MarketingCenter
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="roleId"></param>
        /// <returns></returns>
        public ListResponse DeleteRole(long editorPersonaId, int roleId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }           
            try
			{
                var logData = new Dictionary<string, object>();
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string marketingCompanyId = company.CompanyInstanceSourceId;
                var roles = GetRoles(editorPersonaId, 0, null);
                var roleName = roles.Records.Cast<ProductRole>().FirstOrDefault(r => r.ID == roleId.ToString())?.Name;
                var url = _productUrl + $"/external/company/{marketingCompanyId}/roles/{roleId}?username={GetLoginName()}";
                var result = _httpClient.DeleteAsync(url).Result;
                if (result.IsSuccessStatusCode)
                {
                    DeleteRoleLogMessage(editorPersonaId, roleId, roleName, "Marketing Center", _productId);
                    dynamic jsonResult = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
                    logData = new Dictionary<string, object>
                    {
                        { "roleResult", jsonResult }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "DeleteRole", $"Delete role {roleId}. Got result from marketing center." });
                }
                else
                {
                    WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "DeleteRole", $"Delete role {roleId} status errored." });
                    response = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = "ManageMarketingCenterUser.DeleteRole - Unable to delete role"
                    };
                    return response;
                }
            }

			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "DeleteRole", $"Error. {ex.Message}" });
                response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
            return response;
        }

        public void DeleteRoleLogMessage(long editorPersonaId, long roleId, string roleName, string product, int productId)
        {
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            var message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) deleted {roleName} in {product}."
                : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} deleted {roleName} in {product}.";

            additionalParameters.Add(new AdditionalParameters { Key = "Role", Value = PRODUCT_ROLE_DELETE.Replace("RoleName", roleName.ToString()) });
            unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, productId);
        }

        /// <summary>
        /// Used to update a role status in MarketingCenter
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="roleId"></param>
        /// <param name="IsActive"></param>
        /// <returns></returns>
        public ListResponse UpdateRoleStatus(long editorPersonaId, int roleId, bool IsActive)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            try
			{
                var logData = new Dictionary<string, object>();
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
				string marketingCompanyId = company.CompanyInstanceSourceId;
				var url = _productUrl + $"/external/company/{marketingCompanyId}/roles/{roleId}?active={IsActive}&username={GetLoginName()}";
				var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
				var result = _httpClient.SendAsync(request).Result;
				if (result.IsSuccessStatusCode)
				{
                    dynamic jsonResult = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
                    logData = new Dictionary<string, object>
                    {
                        { "roleResult", jsonResult }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateRoleStatus", $"Update roleId {roleId} status. Got result from marketing center." });
                }
				else
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRoleStatus", $"Update userId {roleId} status errored." });
                    response = new ListResponse()
					{
						IsError = true,
						ErrorReason = "ManageMarketingCenterUser.UpdateRoleStatus - Unable to update role status"
					};
					return response;
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRoleStatus", $"Error. {ex.Message}" });
                response = new ListResponse()
				{
					IsError = true,
					ErrorReason = ex.Message
				};
			}
            return response;
        }

        /// <summary>
        /// Used to update a role status in MarketingCenter
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="rightId"></param>
        /// <param name="roleList"></param>
        /// <returns></returns>
        public ListResponse UpdateRolesForRight(long editorPersonaId, int rightId, List<string> roleList)
        {
            ListResponse response = new ListResponse();
            
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            List<string> rolesToAdd = new List<string>();
            List<string> rolesToRemove = new List<string>();
            try
			{
                var currentRoles = GetRolesForRightId(editorPersonaId, rightId);

                if (!currentRoles.IsError && currentRoles.Records != null && currentRoles.Records.Count > 0)
                {
                    currentRoles.Records = currentRoles.Records.OfType<RolesRightsAccessRight>().Where(r => r.IsAssigned).Cast<object>().ToList();
                }

                GetRoleAssignmentChanges(roleList, currentRoles, out rolesToAdd, out rolesToRemove);
            }
			catch (Exception ex)
			{
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRolesForRight", $"Error. {ex.Message}" });

            }
            try
            {
                var logData = new Dictionary<string, object>();
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string marketingCompanyId = company.CompanyInstanceSourceId;
				var url = _productUrl + $"/external/company/{marketingCompanyId}/rights/{rightId}/roles?username={GetLoginName()}";
                var result = _httpClient.PutAsJsonAsync(url, roleList.Select(int.Parse).ToList()).Result;
                if (result.IsSuccessStatusCode)
                {
                    dynamic jsonResult = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
					UpdateRolesToRightLogMessage(editorPersonaId, rightId, rolesToAdd, rolesToRemove);
					response.Records = null;
                    logData = new Dictionary<string, object>
                    {
                        { "roleResult", jsonResult }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateRolesForRight", $"Update rightId {rightId} status. Got result from marketing center." });
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForRight", $"Update rightId {rightId} status errored." });
                    response = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = "ManageMarketingCenterUser.UpdateRolesForRight - Unable to update role status"
                    };
                    return response;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRolesForRight", $"Error. {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        private void GetRoleAssignmentChanges(List<string> roles, ListResponse currentRoles, out List<string> rolesToAdd, out List<string> rolesToRemove)
        {
            rolesToAdd = new List<string>();
            rolesToRemove = new List<string>();

            // Normalize inputs
            var desired = (roles ?? new List<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var assignedNow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (currentRoles?.Records != null && currentRoles.Records.Count > 0)
            {
                foreach (var pr in currentRoles.Records.OfType<RolesRightsAccessRight>())
                {
                    if (pr.IsAssigned)
                    {
                        assignedNow.Add(pr.Id.ToString().Trim());
                    }
                }
            }

            // Roles to add: desired minus currently assigned
            foreach (var roleId in desired)
            {
                if (!assignedNow.Contains(roleId))
                {
                    rolesToAdd.Add(roleId);
                }
            }

            // Roles to remove: currently assigned minus desired
            foreach (var roleId in assignedNow)
            {
                if (!desired.Contains(roleId))
                {
                    rolesToRemove.Add(roleId);
                }
            }
        }
        /// <summary>
        /// Create new role
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="mcRole"></param>
        /// <returns></returns>
        public ListResponse CreateNewMCRoleWithRights(long editorPersonaId, MCRole mcRole)
        {
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            try
            {
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
				string marketingCompanyId = company.CompanyInstanceSourceId;
                var url = _productUrl + $"/external/company/{marketingCompanyId}/roles?active={mcRole.Active}&username={GetLoginName()}";
                var result = _httpClient.PostAsJsonAsync(url, mcRole).Result;
                if (result.IsSuccessStatusCode)
                {
                    dynamic jsonResult = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);

					//Activity Log for Create Role
                    ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                    UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);
                    var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
                    List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                    var message = "";
                    message = impersonatorUserInfo != null
							 ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Created {mcRole.Name} in Marketing Center."
							 : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Created {mcRole.Name} in Marketing Center.";

                    additionalParameters.Add(new AdditionalParameters { Key = "Role", Value = PRODUCT_ROLE_CREATE.Replace("RoleName", mcRole.Name) });

                    unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, _productId);


                    // All submitted rights are treated as newly added on create. None removed.
                    var addedRights = mcRole.Rights != null? mcRole.Rights.Select(r => r.ToString()).ToList(): new List<string>();
                    var removedRights = new List<string>();

                    UpdateRightsToRoleLogMessage(editorPersonaId, mcRole.Id, mcRole.Name, addedRights, removedRights);

                    response.Records = null;
                    logData = new Dictionary<string, object>
                    {
                        { "roleResult", jsonResult }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "CreateNewMCRoleWithRights", "Got result from marketing center" });
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateNewMCRoleWithRights", "Got error result from marketing center" });
                    RoleErrors roleErrors = JsonConvert.DeserializeObject<RoleErrors>(result.Content.ReadAsStringAsync().Result);
					response = new ListResponse()
					{
						IsError = true,
						Additional = "RoleError",
						ErrorReason = !string.IsNullOrEmpty(roleErrors?.FieldErrors?.Error?.Message) ? roleErrors.FieldErrors.Error.Message : "Unable to create role"
					};
                    return response;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateNewMCRoleWithRights", $"Error. {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Update role
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="mcRole"></param>
        /// <returns></returns>
        public ListResponse UpdateNewMCRoleWithRights(long editorPersonaId, MCRole mcRole)
        {
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            try
            {
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                //Find Existing Role Rights
                var currentRights = GetRightsForRoleId(editorPersonaId, mcRole.Id);

                // Filter: keep only currently assigned rights (IsAssigned == true)
                if (!currentRights.IsError && currentRights.Records != null)
                {
                    currentRights.Records = currentRights.Records.OfType<MCRight>().Where(r => r.IsAssigned).Cast<object>().ToList();
                }

                //Identify role name & description before update
                var roles = GetRoles(editorPersonaId, 0, null);
				var roleName = roles.Records.Cast<ProductRole>().FirstOrDefault(r => r.ID == mcRole.Id.ToString())?.Name;
				var roleDescription = roles.Records.Cast<ProductRole>().FirstOrDefault(r => r.ID == mcRole.Id.ToString())?.Description;

                // Determine added / removed rights
                List<string> addedRights;
                List<string> removedRights;
                GetRightAssignmentChanges(currentRights, mcRole.Rights, out addedRights, out removedRights);
                
				//End
                string marketingCompanyId = company.CompanyInstanceSourceId;
                var url = _productUrl + $"/external/company/{marketingCompanyId}/roles/{mcRole.Id}?username={GetLoginName()}";
                var result = _httpClient.PutAsJsonAsync(url, mcRole).Result;
                if (result.IsSuccessStatusCode)
                {
                    dynamic jsonResult = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
					if (!roleName.Equals(mcRole.Name))
					{
                        AddUpdateRoleLogMessage(editorPersonaId, mcRole.Name, "Marketing Center" , roleName, "RoleName");
                    }
                    if (!roleDescription.Equals(mcRole.Description))
                    {
                        AddUpdateRoleLogMessage(editorPersonaId, mcRole.Description, "Marketing Center", roleDescription, "RoleDescription");
                    }

					if (addedRights.Count > 0 || removedRights.Count > 0)
					{
                        UpdateRightsToRoleLogMessage(editorPersonaId, mcRole.Id, string.Empty, addedRights, removedRights);
                    }
                    response.Records = null;
                    logData = new Dictionary<string, object>
                    {
                        { "roleResult", jsonResult }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateNewMCRoleWithRights", "Got result from marketing center" });
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateNewMCRoleWithRights", "Got error result from marketing center" });
                    RoleErrors roleErrors = JsonConvert.DeserializeObject<RoleErrors>(result.Content.ReadAsStringAsync().Result);
                    response = new ListResponse()
                    {
                        IsError = true,
                        Additional = "RoleError",
                        ErrorReason = !string.IsNullOrEmpty(roleErrors?.FieldErrors?.Error?.Message) ? roleErrors.FieldErrors.Error.Message : "Unable to update role"
                    };
                    return response;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateNewMCRoleWithRights", $"Error. {ex.Message}" });
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }
            return response;
        }

        /// <summary>
        /// Compares current assigned rights for a role against desired rights list and returns added / removed sets.
        /// </summary>
        /// <param name="currentRights">Current rights ListResponse (records should be MCRight)</param>
        /// <param name="desiredRights">Desired list of right ids (int). If null/empty treated as none selected.</param>
        /// <param name="addedRights">Out: rights to add (string RightIds)</param>
        /// <param name="removedRights">Out: rights to remove (string RightIds)</param>
        private void GetRightAssignmentChanges(ListResponse currentRights, IList<int> desiredRights, out List<string> addedRights, out List<string> removedRights)
        {
            addedRights = new List<string>();
            removedRights = new List<string>();

            // Normalize desired rights
            var desired = new HashSet<int>((desiredRights ?? new List<int>()).Distinct());

            // Extract currently assigned rights (records are expected to be MCRight)
            var currentlyAssigned = new HashSet<int>();
            if (currentRights?.Records != null)
            {
                foreach (var r in currentRights.Records)
                {
                    var mcRight = r as MCRight;
                    if (mcRight == null) continue;

                    // If endpoint returns only assigned rights we just add them.
                    // If it returns all rights with a flag, only include those marked assigned.
                    if (mcRight.IsAssigned || currentRights.Records.Count > 0) // fallback keeps behavior even if IsAssigned not populated
                    {
                        currentlyAssigned.Add(mcRight.RightId);
                    }
                }
            }

            // Added Rights
            foreach (var rightId in desired)
            {
                if (!currentlyAssigned.Contains(rightId))
                {
                    addedRights.Add(rightId.ToString());
                }
            }

            // Removed Rights
            foreach (var rightId in currentlyAssigned)
            {
                if (!desired.Contains(rightId))
                {
                    removedRights.Add(rightId.ToString());
                }
            }
        }


		public void UpdateRightsToRoleLogMessage(long editorPersonaId, long roleId, string roleName, List<string> rightsToAdd, List<string> rightsToRemove)
		{
			try
			{
                ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
                UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

                var rights = GetRightsDetails(editorPersonaId);
				var rightList = rights.Records.Cast<MCRight>().ToList();
                var roles = GetRoles(editorPersonaId, 0, null);
				if (string.IsNullOrEmpty(roleName))
				{
                    roleName = roles.Records.Cast<ProductRole>().FirstOrDefault(r => r.ID == roleId.ToString())?.Name;
                }
                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                if (rightsToAdd != null)
                {
                    foreach (var right in rightsToAdd)
                    {
                        var rightName = rightList.FirstOrDefault(r => r.RightId.ToString() == right)?.Description;
                        additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_ASSIGN.Replace("RightName", rightName) });
                    }
                }
                if (rightsToRemove != null)
                {
                    foreach (var right in rightsToRemove)
                    {
                        var rightName = rightList.FirstOrDefault(r => r.RightId.ToString() == right)?.Description;
                        additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_UNASSIGN.Replace("RightName", rightName) });
                    }
                }
                var message = "";
                message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed rights to {roleName} in Marketing Center."
                : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed rights to {roleName} in Marketing Center.";

                unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, _productId);
            }
            catch { return; }
        }

        public void UpdateRolesToRightLogMessage(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove)
        {
			try
			{
                ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
                UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

                var roles = GetRoles(editorPersonaId, 0, null);
				var roleList = roles.Records.Cast<ProductRole>().ToList();
                var rights = GetRightsDetails(editorPersonaId);
				var rightName = rights.Records.Cast<MCRight>().FirstOrDefault(r => r.RightId.ToString() == rightId.ToString())?.Description;
				List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
				if (rolesToAdd != null)
				{
					foreach (var role in rolesToAdd)
					{
						var roleName = roleList.FirstOrDefault(r => r.ID == role)?.Name;
						additionalParameters.Add(new AdditionalParameters { Key = rightName, Value = ROLE_ASSIGN.Replace("RoleName", roleName) });
					}
				}
				if (rolesToRemove != null)
				{
					foreach (var role in rolesToRemove)
					{
						var roleName = roleList.FirstOrDefault(r => r.ID == role)?.Name;
						additionalParameters.Add(new AdditionalParameters { Key = rightName, Value = ROLE_UNASSIGN.Replace("RoleName", roleName) });
					}
                }
                var message = "";
                message = impersonatorUserInfo != null
				? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed roles to {rightName} in Marketing Center."
				: $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed roles to {rightName} in Marketing Center.";

				unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, _productId);

            }
			catch (Exception ex)
            {
            }
        }




        public void AddUpdateRoleLogMessage(long editorPersonaId, string roleName, string product, string oldRoleName, string AttributeName)
        {
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            var message = "";
			if (AttributeName.Equals("RoleName"))
			{
                message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Updated Role Name of {oldRoleName} to {roleName} in {product}."
                  : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Updated Role Name of {oldRoleName} to {roleName} in {product}.";

                additionalParameters.Add(new AdditionalParameters { Key = oldRoleName, Value = PRODUCT_ROLENAME_UPDATE.Replace("RoleName", roleName) });
            }
            if (AttributeName.Equals("RoleDescription"))
            {
                message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Updated Role Description of {oldRoleName} to {roleName} in {product}."
                  : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Updated Role Description of {oldRoleName} to {roleName} in {product}.";

                additionalParameters.Add(new AdditionalParameters { Key = oldRoleName, Value = PRODUCT_ROLEDESCRIPTION_UPDATE.Replace("RoleName", roleName) });
            }
            

            unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, _productId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public ListResponse GetRightsForRoleId(long editorPersonaId, int roleId)
		{
            ListResponse response = new ListResponse();
            try
            {
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                IList<MCRight> rightList = new List<MCRight>();
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string marketingCompanyId = company.CompanyInstanceSourceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsForRoleId", $"Found blue book company source id {marketingCompanyId}" });
                var url = roleId == 0 ? _productUrl + $"/external/company/{marketingCompanyId}/rights" : _productUrl + $"/external/company/{marketingCompanyId}/roles/{roleId}/rights";
                var logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsForRoleId", "Getting rights" });

                var apiResponse = _httpClient.GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    var res = JsonConvert.DeserializeObject<IList<Right>>(jsonContent);
                    logData = new Dictionary<string, object>
                    {
                        { "rightList", rightList }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsForRoleId", "Got rights" });

                    if (rightList == null) { rightList = new List<MCRight>(); }
					else { rightList = res.ToGBRights(); }

                    response = new ListResponse()
                    {
                        Records = rightList.Cast<object>().ToList(),
                        TotalRows = rightList.Count,
                        RowsPerPage = rightList.Count,
                        TotalPages = 1,
                        ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRightsForRoleId", $"Error. {ex.Message}" });
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the roles";

            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="rightId"></param>
        /// <returns></returns>
        public ListResponse GetRolesForRightId(long editorPersonaId, int rightId)
        {
            ListResponse response = new ListResponse();
            try
            {
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var logData = new Dictionary<string, object>();
                IList<RolesRightsAccessRight> roleList = new List<RolesRightsAccessRight>();
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string marketingCompanyId = company.CompanyInstanceSourceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesForRightId", $"Found blue book company source id {marketingCompanyId}" });
                var url = _productUrl + $"/external/company/{marketingCompanyId}/rights/{rightId}/roles";
                //var url = "https://api.pv3.myleasestar.com/external/company/30032/roles";
                logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesForRightId", "Getting roles" });

                var apiResponse = _httpClient.GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    roleList = JsonConvert.DeserializeObject<IList<RolesRightsAccessRight>>(jsonContent);
                    logData = new Dictionary<string, object>
                    {
                        { "rightList", roleList }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesForRightId", "Got roles" });

                    if (roleList == null) { roleList = new List<RolesRightsAccessRight>(); }

                    response = new ListResponse()
                    {
                        Records = roleList.Cast<object>().ToList(),
                        TotalRows = roleList.Count,
                        RowsPerPage = roleList.Count,
                        TotalPages = 1,
                        ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRolesForRightId", $"Error. {ex.Message}" });
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the roles";

            }
            return response;
        }

        /// <summary>
        /// Used to get roles for Marketing Center in Roles and Rights Access page.
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <returns></returns>
        private ListResponse GetRolesCountDetails(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            try
            {
                IList<RolesRightsAccessRight> rolesList = new List<RolesRightsAccessRight>();
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string marketingCompanyId = company.CompanyInstanceSourceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", $"Found blue book company source id {marketingCompanyId}" });
                var url = _productUrl + $"/external/company/{marketingCompanyId}/roles" ;
				//var url = "https://api.pv3.myleasestar.com/external/company/30032/roles";
                var logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesCountDetails", "Getting roles" });

                var apiResponse = _httpClient.GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rolesList = JsonConvert.DeserializeObject<IList<RolesRightsAccessRight>>(jsonContent);
                    logData = new Dictionary<string, object>
                    {
                        { "rolesList", rolesList }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesCountDetails", "Got roles" });

                    if (rolesList == null) { rolesList = new List<RolesRightsAccessRight>(); }

                    response = new ListResponse()
                    {
                        Records = rolesList.Cast<object>().ToList(),
                        TotalRows = rolesList.Count,
                        RowsPerPage = rolesList.Count,
                        TotalPages = 1,
                        ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRolesCountDetails", $"Error. {ex.Message}" });
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the roles";

            }
            return response;
        }

        /// <summary>
        /// Used to get rights for Marketing Center in Roles and Rights Access page.
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <returns></returns>
        private ListResponse GetRightsDetails(long editorPersonaId)
        {
            ListResponse response = new ListResponse();

            IList<Right> rights = new List<Right>();
            IList<MCRight> mcRights = new List<MCRight>();
            try
            {
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string marketingCompanyId = company.CompanyInstanceSourceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetails", $"Found blue book company source id {marketingCompanyId}" });
                var url = _productUrl + $"/external/company/{marketingCompanyId}/rights";
                //var url = "https://api.pv3.myleasestar.com/external/company/30032/rights";
                var logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsDetails", "Getting rights" });

                var apiResponse = _httpClient.GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rights = JsonConvert.DeserializeObject<List<Right>>(jsonContent);
                    logData = new Dictionary<string, object>
                    {
                        { "rightGroup", rights }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsDetails", "Got rights" });

                    if (rights == null) { rights = new List<Right>(); }
					else { mcRights = rights.ToGBRights(); }

                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsDetails", "Returning rights" });
                    response = new ListResponse()
                    {
                        Records = mcRights.Cast<object>().ToList(),
                        TotalRows = mcRights.Count,
                        RowsPerPage = mcRights.Count,
                        TotalPages = 1,
                        ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRightsDetails", $"Error. {ex.Message}" });
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the rights";
            }
            return response;
        }

        #endregion

        #region Private methods
        private bool CheckIfUserExistInProduct(string _productUserId)
		{
			var url = _productUrl + $"/external/contact/details?emailAddress={_productUserId}";
			var response = _httpClient.GetAsync(url).Result;
			if (response.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckIfUserExistInProduct", $"Email address {_productUserId} already exist in Marketing Center." });
                return true;
			}
			return false;
		}

		private string GetMCUniqueUserName(string firstName, string lastName)
		{
			// get a login name that isn't in use for the new user
			bool foundUserName = false;
			string userEmailAddress = "";
			int incrementor = 1;
			string newproductUsername = $"{firstName.TrimWhiteSpace().Substring(0, 1)}" + $"{lastName.TrimWhiteSpace()}".ToLower();
			userEmailAddress = $"{newproductUsername}{incrementor.ToString()}@noreply.com";

			while (!foundUserName)
			{
				if (CheckIfUserExistInProduct(userEmailAddress))
				{
					incrementor++;
					userEmailAddress = $"{newproductUsername}{incrementor.ToString()}@noreply.com";
				}
				else
				{
					foundUserName = true;
				}

			}
			return userEmailAddress;
		}

		/// <summary>
		/// Sets Product user status
		/// </summary>
		/// <param name="isActive"></param>
		/// <param name="mcUserId"></param>
		/// <returns></returns>
		private bool SetMarketingCenterUserStatus(bool isActive, string mcUserId)
		{
			try
			{
				var logData = new Dictionary<string, object>();
				if (string.IsNullOrEmpty(_editorProductUserId))
				{
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "SetMarketingCenterUserStatus", "Update user status. Editor Product User Id cannot be null or empty." });
                    return false;
				}
				if (mcUserId?.Length > 0 && mcUserId != "0")
				{
					string url = _productUrl + $"/external/contact/{ mcUserId }/status";
					MC.MarketingCenterUserStatus mcUser = new MC.MarketingCenterUserStatus()
					{
						isActive = isActive,
						isActiveUnifiedUser = isActive,
						auditUserId = Convert.ToInt64(_editorProductUserId)
					};

					string mcUserJson = JsonConvert.SerializeObject(mcUser);

					logData = new Dictionary<string, object>
					{
						{ "url", url },
                        {"mcUserJson", mcUserJson}
					};
					WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", $"Update userId {mcUserId} url and user status request object" });

                    var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;
					dynamic userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
					logData = new Dictionary<string, object>
					{
						{ "userResult", userResult }
					};
					if (response.IsSuccessStatusCode)
					{
						WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", $"Update userId {mcUserId} status. Got result from marketing center." });
                        return true;
					}
					else
					{
						WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", $"Update userId {mcUserId} status errored." });
                        return false;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "SetMarketingCenterUserStatus", $"Update user status errored. {ex.Message}" });
                return false;
			}
		}

		private T GetResultFromApi<T>(string baseUrlAndQuery) where T : class
		{
			T results = null;
			
			var response = _httpClient.GetAsync(baseUrlAndQuery).Result;
			if (response.IsSuccessStatusCode)
			{
				var jsonContent = response.Content.ReadAsStringAsync().Result;
				results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
			}
			else
			{
				var logData = new Dictionary<string, object>
				{
                    { "uri", baseUrlAndQuery },
                    { "error", response.Content.ReadAsStringAsync().Result },
					{ "status", response.StatusCode }
				};
				WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetResultFromApi", "Exiting after error" });
            }
			return results;
		}

		private string ParseErrorPosting(HttpResponseMessage response, string action, long editorPersonaId, long userPersonaId)
		{
			var userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
			bool emailAlreadyExists = false;
			try
			{
				// see if the error is from the user email already existing
				string errorText = userResult.fieldErrors.Error.message;
				if (errorText.Contains("duplicate") && errorText.Contains("emailAddress"))
				{
					emailAlreadyExists = true;
				}
			}
			catch (Exception ex)
			{
				// couldn't parse the response from the product, so move on
				WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageMarketingCenterUser.ParseErrorPosting", "Error" });
            }
			var logData = new Dictionary<string, object> { { "response.Content.ReadAsStringAsync().Result", userResult } };

            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser.ParseErrorPosting", $"{action} errored." + (emailAlreadyExists ? " Email already exists" : "") });
            string result = "";
			switch (action.ToUpper())
			{
				case "CREATE":
					result = "There was a problem creating the user." + (emailAlreadyExists ? " Email already exists" : "");
					break;
				case "UPDATE":
					result = "There was a problem updating the user." + (emailAlreadyExists ? " Email already exists" : "");
					break;
			}

			if (emailAlreadyExists)
			{
				WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.A user already exists with this email address.Please try using the Migration Tool.");
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
				// stop batch process
				result = ProductBatchStatusType.Stop.ToString();
			}
			else
			{
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
			}
			return result;
		}

		/// <summary>
		/// Used to get info about a user
		/// </summary>
		/// <returns></returns>
		private MC.MarketingCenterUserDetails GetUserDetails()
		{
			MC.MarketingCenterUserDetails mDetails = new MC.MarketingCenterUserDetails();
			if (!string.IsNullOrEmpty(_productUserId))
			{
				try
				{
					var url = _productUrl + $"/external/contact/{_productUserId}/details";
					var response = _httpClient.GetAsync(url).Result;

					if (response.IsSuccessStatusCode)
					{
						mDetails = JsonConvert.DeserializeObject<MC.MarketingCenterUserDetails>(response.Content.ReadAsStringAsync().Result);
					}
				}
				catch (Exception ex)
				{
				}
			}
			return mDetails;
		}

		/// <summary>
		/// Check if the userId exists in the Marketing Center
		/// </summary>
		/// <param name="userId">Product UserID</param>
		/// <returns>boolean</returns>
		private bool IsUserIdValid(long userId)
		{
			string url = _productUrl + $"/external/contact/{ userId }/status";
			var responsex = _httpClient.GetAsync(url).Result;

			return responsex.IsSuccessStatusCode;
		}

		/// <summary>
		/// Returns UserName for marketing center, we will send this for their auditing purpose. 
		/// </summary>
		/// <returns></returns>
		private string GetLoginName()
		{
            string loginName = string.Empty;    
            if (string.IsNullOrEmpty(_userClaims.ImpersonatedByName))
            {
                loginName = _userClaims.LoginName;
            }
            else
            {
                UserDetails currentUser = _userRepository.GetUserDetails(null, _userClaims.ImpersonatedBy.ToString());
                loginName = currentUser.LoginName;
            }
            return loginName;
		}

		#endregion

		#region Migration
		/// <summary>
		/// List all users
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter)
		{
			var response = new ListResponse()
			{
				IsError = true,
				ErrorReason = "No Users."
			};
			var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
			if (claimResposnse.IsError) { response.ErrorReason = claimResposnse.ErrorReason; return response; }

			int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
			if (companyInstanceSourceId == 0)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
				return response;
			}
			var filter = "NonMigrated";
			var startRow = 0;
			var resultPerRow = 1000;
			if (datafilter != null)
			{
				if (datafilter.FilterBy.ContainsKey("filter"))
				{
					filter = datafilter.FilterBy["filter"];
				}
				if (datafilter.Pages != null)
				{
					startRow = datafilter.Pages.StartRow;
					resultPerRow = datafilter.Pages.ResultsPerPage;
				}
			}

			var url = $"{_productUrl}/external/api/{companyInstanceSourceId}/users?filter-type={filter}&startRow={startRow}&resultsperpage={resultPerRow}";
			WriteToDiagnosticLog("{ActionName} - {state}", new Dictionary<string, object> { { "Url", url } }, messageProperties: new object[] { "GetMigrationUsers", "Getting users" });

            var migrationResponse = GetResultFromApi<MigrationResponse<IList<MigrationUser>>>(url);

			if (migrationResponse == null)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                return response;
			}
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}." });
            response.RowsPerPage = resultPerRow;
			response.ErrorReason = string.Empty;
			response.IsError = false;
			response.TotalPages = 1;
			response.Records = migrationResponse.Data.Cast<object>().ToList();
			response.TotalRows = migrationResponse.Data.Count();
			return response;
		}

		/// <summary>
		/// Update the users migration status
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="migrateUsers"></param>
		/// <returns></returns>
		public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
		{
			var migrateResponse = new MigrateResponse()
			{
				Status = false
			};

			var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
			if (claimResposnse.IsError) { migrateResponse.Message = claimResposnse.ErrorReason; return migrateResponse; }

			int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
			if (companyInstanceSourceId == 0)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
				return migrateResponse;
			}

			//Below logic is needed when product user migrated to existing UL user,we need to send notification email address
			//to product to update email
			foreach (var user in migrateUsers)
			{
				if (string.IsNullOrEmpty(user.LeadEmailAddress))
				{
					// get the email address
					ManageElectronicAddress _manageElectronicAddress = new ManageElectronicAddress();
					IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(user.UnifiedLoginUserName, _editorPersona.OrganizationPartyId, "");

					if (_addresses != null)
					{
						if (_addresses.Any(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)))
						{
							user.LeadEmailAddress = (from a in _addresses where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
						}
					}
				}
			}

			var url = $"{_productUrl}/external/api/{companyInstanceSourceId}/migrate-users";
			var response = _httpClient.PostAsJsonAsync(url, migrateUsers).Result;
			var responseContent = response.Content.ReadAsStringAsync().Result;

			var logData = new Dictionary<string, object>
			{
				{ "Url", url },
				{ "Response", responseContent },
				{ "EditorPersonaId", editorPersonaId },
				{ "MigratedUser", migrateUsers }
			};
			if (response.IsSuccessStatusCode)
			{
				var migrationResponse = JsonConvert.DeserializeObject<MigrationResponse<MigrateResponse>>(responseContent);
				WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync migrate-users" });
                migrateResponse.Message = migrationResponse.Data.Message;
				migrateResponse.Status = migrationResponse.Data.Status;
				return migrateResponse;
			}
			else
			{
				WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync migrate-users error" });
                migrateResponse.Message = "Cannot update user status to migrated.";
				return migrateResponse;
			}
		}
		#endregion
	}

	/// <summary>
	/// Used to help convert product classes to GreenBook classes
	/// </summary>
	public static class ManageProductMarketingCenterHelpers
	{
		/// <summary>
		/// Used to convert a product role into a GreenBook role to be used by the UI
		/// </summary>
		/// <param name="roles">The list of roles to convert</param>
		/// <returns></returns>
		public static IList<ProductRole> ToGBRoles(this IList<MC.Role> roles)
		{
			if (roles == null) return null;
			IList<ProductRole> results = new List<ProductRole>();
			foreach (MC.Role role in roles)
			{
				if (role.IsActive)
				{
					results.Add(new ProductRole
					{
						ID = role.RoleId.ToString(),
						Name = role.RoleName,
						Description = role.Description
					});
				}
			}
			return (from role in results orderby role.Name select role).ToList();
		}

		/// <summary>
		/// Used to convert a Product property into a GreenBook property
		/// </summary>
		/// <param name="properties">The list of properties to convert</param>
		/// <returns></returns>
		public static IList<ProductProperty> ToGBProperties(this IList<ProductPropertyMap> properties)
		{
			if (properties == null) return null;
			IList<ProductProperty> results = new List<ProductProperty>();
			foreach (ProductPropertyMap property in properties)
			{
				results.Add(new ProductProperty
				{
					ID = property.PropertyId,
					Name = property.PropertyName,
					State = property.State
				});
			}
			return results;
		}

		public static IList<MCRight> ToGBRights(this IList<Right> rights)
		{
			if (rights == null) return null;
			IList<MCRight> res = new List<MCRight>();
			foreach (Right right in rights)
			{
				res.Add(new MCRight
                {
					RightId = right.RightId,
					Description = right.Description,
					GroupName = right.GroupName,
					GroupId = right.GroupId,
					SubGroupId = right.SubGroupId,
					SubGroupName = right.SubGroupName,
					DisplaySequence	= right.DisplaySequence,
					RightName = right.RightName,
					Action = right.Action,
                    RolesAssigned = right.RoleCount,
					IsAssigned = right.IsAssigned
				});
			}
			return res;
		}
	}
}