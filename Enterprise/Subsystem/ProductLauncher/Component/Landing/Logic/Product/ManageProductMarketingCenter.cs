using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using RP.Enterprise.Foundation.DataAccess.Component;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using MC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
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
			// TODO REMOVE WHEN POSTING TO TRUSTED URL
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			// TODO REMOVE WHEN POSTING TO TRUSTED URL
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
			// TODO REMOVE WHEN POSTING TO TRUSTED URL
			ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
			// TODO REMOVE WHEN POSTING TO TRUSTED URL

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
				
				WriteToDiagnosticLog($"GetRoles - Found blue book company source id {marketingCompanyId}");
				var url = _productUrl + $"/v2/company/{marketingCompanyId}/contact/roles";
				logData = new Dictionary<string, object>();
				logData.Add("url", url);
				WriteToDiagnosticLog("GetRoles - Posting to url", logData);
				var response = _httpClient.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					var jsonContent = response.Content.ReadAsStringAsync().Result;

					rolesList = JsonConvert.DeserializeObject<IList<MC.Role>>(jsonContent);
					if (rolesList == null) { rolesList = new List<MC.Role>(); }

					logData = new Dictionary<string, object>();
					logData.Add("rolesList", rolesList);
					WriteToDiagnosticLog("GetRoles - Got response", logData);

					IList<ProductRole> list = rolesList.ToGBRoles();
					if (list == null) { list = new List<ProductRole>(); }

					// need to do a filter on the result
					if (userPersonaId != 0)
					{
						// merge the given user details with the list
						MC.MarketingCenterUserDetails mUser = GetUserDetails();
						if (mUser == null)
						{
							WriteToDiagnosticLog($"GetRoles - Error looking for user. userPersonaId={userPersonaId.ToString()}");
							return new ListResponse() { IsError = true, ErrorReason = "User not found" };
						}
						logData = new Dictionary<string, object>();
						logData.Add("mUser", mUser);
						WriteToDiagnosticLog("GetRoles - Looking for role for user.", logData);

						if (list.Any(a => a.ID == mUser.ContactRoleId.ToString()))
						{
							ProductRole pr = (from a in list where a.ID == mUser.ContactRoleId.ToString() select a).FirstOrDefault();
							if (pr != null)
							{
								pr.IsAssigned = true;
								WriteToDiagnosticLog($"GetRoles - Found role for user. Role: {pr.Name}");
							}
						}
					}
					logData = new Dictionary<string, object>();
					logData.Add("list", list);
					WriteToDiagnosticLog("GetRoles - Returning role list.", logData);

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
					WriteToErrorLog("GetRoles - Error. " + response.Content.ReadAsStringAsync().Result);
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"GetRoles - Error. {ex.Message} ", exception: ex);
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
				WriteToDiagnosticLog("GetMarketingCenterPMCIDFromPersona - Getting info from BlueBook.GetCompanyMap");
				IList<CustomerCompanyMap> companyMap = _blueBook.GetCompanyMap(_editorPersona.Organization.RealPageId, _editorPersona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.MarketingCenter, domain: _editorPersona.Organization.OrganizationDomain.Name);
				WriteToDiagnosticLog("GetMarketingCenterPMCIDFromPersona - Done getting info from BlueBook.GetCompanyMap");
				if (companyMap != null && companyMap.Count > 0 && companyMap.Any(a => a.Source.Equals(BlueBookProductConstants.MarketingCenter, StringComparison.OrdinalIgnoreCase)))
				{
					WriteToDiagnosticLog("GetMarketingCenterPMCIDFromPersona - Getting PMC ID from BlueBook result");
					marketingCenterCompanyId = companyMap.First(a => a.Source.Equals(BlueBookProductConstants.MarketingCenter, StringComparison.OrdinalIgnoreCase)).CompanyInstanceSourceId;
					WriteToDiagnosticLog("GetMarketingCenterPMCIDFromPersona - Found PMC ID from BlueBook result: {marketingCenterCompanyId}");

				}

				//companyInstanceId = 779893; // LeaseStar id 438
				IList<ProductPropertyMap> propertyList = new List<ProductPropertyMap>();
				var url = _productUrl + $"/v2/properties?companyId= { marketingCenterCompanyId} ";
				logData = new Dictionary<string, object>();
				logData.Add("url", url);
				WriteToDiagnosticLog("GetProperties - Posting to url", logData);
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
							WriteToDiagnosticLog($"GetProperties - Error looking for user. userPersonaId={userPersonaId.ToString()}");
							return new ListResponse() { IsError = true, ErrorReason = "User not found" };
						}
						logData = new Dictionary<string, object>();
						logData.Add("mUser", mUser);
						WriteToDiagnosticLog("GetProperties - Looking for properties for user.", logData);
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
									logData.Add("Property" + i.ToString(), p.Name);
									i++;
								}
							}
							WriteToDiagnosticLog($"GetProperties - Adding extra property.", logData);
						}
						//allProperties.Add("allProperties", mUser.AssignNewProperty);
                        allProperties.Add("IsAssignedNewPropertyByDefault", mUser.AssignNewProperty);
                    }

					logData = new Dictionary<string, object>();
					logData.Add("list", list);
					WriteToDiagnosticLog("GetProperties - Returning property list.", logData);

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
					WriteToErrorLog("GetRoles - Error. " + response.Content.ReadAsStringAsync().Result);
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
				WriteToErrorLog($"GetProperties - Error. {ex.Message}", exception: ex);
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
				WriteToDiagnosticLog(response);
			}
			else
			{
				bool status = SetMarketingCenterUserStatus(false, _productUserId);
				if (status)
				{
					WriteToDiagnosticLog($"ManageMarketingCenterUser.UnassignUser - userPersonaId: {userPersonaId}");
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
			ListResponse listResponse = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			try
			{
				listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (listResponse.IsError)
				{
					return listResponse.ErrorReason;
				}

				WriteToDiagnosticLog("ManageMarketingCenterUser.UpdateUserProfile - Begin update user profile");
				string productLoginName = "";

				Persona userPersona = _managePersona.GetPersona(userPersonaId);
				WriteToDiagnosticLog("ManageMarketingCenterUser.UpdateUserProfile - Got persona info");
				Guid realPageId = userPersona.RealPageId;

				Person person = _managePerson.GetPerson(realPageId);
				WriteToDiagnosticLog("ManageMarketingCenterUser.UpdateUserProfile - Got person info");

				UserLoginOnly userLogin = new UserLoginOnly();
				userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

				IList<UserOrganization> userPersonaOrganizationList = _manageUserLogin.GetUserPersonaOrganization(userLogin.LoginName);
				bool isRegularUserNoEmail  = IsRegularUserNoEmail(userPersonaId);

				// get the email address
				WriteToDiagnosticLog("ManageMarketingCenterUser.UpdateUserProfile - Begin get user email address");
				string userEmailAddress = "";
				string userLeadEmailAddress = "";
				ManageElectronicAddress _manageElectronicAddress = new ManageElectronicAddress();
				IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
				WriteToDiagnosticLog("ManageMarketingCenterUser.UpdateUserProfile - Got list of electronic address");
				if (_addresses != null)
				{
					if (_addresses.Any(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)))
					{
						userEmailAddress = (from a in _addresses where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
						WriteToDiagnosticLog($"ManageMarketingCenterUser.UpdateUserProfile - Found email address. {userEmailAddress}");
					}
				}
				if (string.IsNullOrEmpty(userEmailAddress))
				{
					userEmailAddress = userLogin.LoginName;
					WriteToDiagnosticLog("ManageMarketingCenterUser.UpdateUserProfile - Using login name for email address.");
				}

				if (isRegularUserNoEmail)
				{
					userLeadEmailAddress = userEmailAddress;
				}
				// verify email address looks valid, will fail if not
				WriteToDiagnosticLog($"ManageMarketingCenterUser.UpdateUserProfile - User Type : {userPersona.UserTypeId}");
				WriteToDiagnosticLog($"ManageMarketingCenterUser.UpdateUserProfile - Validating email address. Email: {userLogin.LoginName}");
				if (userPersona.UserTypeId == (int)UserTypeConstants.RegularUserNoEmail)
				{
                    userEmailAddress = _productUsername;
                }
				else
				{
					userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
				}

				WriteToDiagnosticLog($"ManageMarketingCenterUser.UpdateUserProfile - Validated email address. Email: {userEmailAddress}");
				WriteToDiagnosticLog($"ManageMarketingCenterUser.UpdateUserProfile - Product User Name : {_productUsername}");

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
					WriteToDiagnosticLog($"ManageMarketingCenterUser.UpdateUserProfile - Error looking for user. userPersonaId={userPersonaId.ToString()}");
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

				var url = _productUrl + $"/v2/contact/{_productUserId}?sourceid={_editorProductUserId}";
				logData = new Dictionary<string, object>();
				logData.Add("url", url);
				logData.Add("mcuser", mcUser);
				WriteToDiagnosticLog("ManageMarketingCenterUser.UpdateUserProfile - Update user profile.", logData);
				var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;

				if (response.IsSuccessStatusCode)
				{
					WriteToDiagnosticLog("ManageMarketingCenterUser - StartUpdate user SAMLAttribute User_email=" + productLoginName);
					UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
					WriteToDiagnosticLog("ManageMarketingCenterUser - Update user SAMLAttribute User_email success. Saved user id");

					WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
					return string.Empty;
				}
				else
				{
					string errorContent = string.Empty;
					try
					{
						errorContent = response.Content.ReadAsStringAsync().Result;
					}
					catch
					{/*Ignored*/ }
					WriteToErrorLog($"ManageMarketingCenterUser.UpdateUserProfile Error for user with editorPersona id - {editorPersonaId}.", logData);
					return $"There was a problem updating user profile for user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageMarketingCenterUser.UpdateUserProfile - Error for user with editorPersona id - {editorPersonaId}", exception: ex);
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
        /// <returns></returns>
        public string ManageMarketingCenterUser(long editorPersonaId, long userPersonaId, List<int> RoleList, List<string> PropertyList, bool IsAssignedNewPropertyByDefault)
		{
			ListResponse listResponse = new ListResponse();
			Dictionary<string, object> logData = new Dictionary<string, object>();
			List<int> mcProperties = new List<int>();
			listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError)
			{
				return listResponse.ErrorReason;
			}

			WriteToDiagnosticLog("ManageMarketingCenterUser - Begin create/update user");
			string productLoginName = "";

			Persona userPersona = _managePersona.GetPersona(userPersonaId);
			WriteToDiagnosticLog("ManageMarketingCenterUser - Got persona info");
			Guid realPageId = userPersona.RealPageId;

			IC.IPerson person = _managePerson.GetPerson(realPageId);
			WriteToDiagnosticLog("ManageMarketingCenterUser - Got person info");

			IUserLoginOnly userLogin = new UserLoginOnly();
			userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
			
            IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);
            userPersona.Organization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);

            var personaOrganization = userPersona.Organization;
			bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

			// get the email address
			WriteToDiagnosticLog("ManageMarketingCenterUser - Begin get user email address");
			string userEmailAddress = "";
			string userLeadEmailAddress = "";
			ManageElectronicAddress _manageElectronicAddress = new ManageElectronicAddress();
			IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
			WriteToDiagnosticLog("ManageMarketingCenterUser - Got list of electronic address");
			if (_addresses != null)
			{
				if (_addresses.Any(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)))
				{
					userEmailAddress = (from a in _addresses where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
					WriteToDiagnosticLog($"ManageMarketingCenterUser - Found email address. {userEmailAddress}");
				}
			}
			if (IsRegularUserNoEmail(userPersonaId))
			{
				userLeadEmailAddress = userEmailAddress;
				if (string.IsNullOrEmpty(userLeadEmailAddress))
				{
					WriteToDiagnosticLog("ManageMarketingCenterUser - Error.No Valid Notification Email Provided.");
					// write an error
					return "ManageMarketingCenterUser - Error.No Valid Notification Email Provided";
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
					WriteToDiagnosticLog("ManageMarketingCenterUser - Using login name for email address.");
				}
			}

			// verify email address looks valid, will fail if not
			WriteToDiagnosticLog($"ManageMarketingCenterUser - Validating email address. Email: {userEmailAddress}");
			userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
			WriteToDiagnosticLog($"ManageMarketingCenterUser - Validated email address. Email: {userEmailAddress}");

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

				WriteToDiagnosticLog($"ManageMarketingCenterUser - Create user error. PropertyList.Count={PropertyList.Count.ToString()} , RoleList.Count={RoleList.Count.ToString()}");
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
					logData = new Dictionary<string, object>();
					logData.Add("roleList", roleList);
					WriteToDiagnosticLog("ManageMarketingCenterUser - Error getting role id for Corporate Operations for super user.", logData);
					WriteActivityLogWithMessage(editorPersonaId, userPersonaId, "An error occurred when {3} {4} attempted to provision {2} for {0} {1}.There is no Corporate Operations role active in this company.Please contact the implementation team for this product.");
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
					// stop batch process
					return ProductBatchStatusType.Stop.ToString();
				}

				// get all properties and assign it to the new super user
				//PropertyList = new List<string>();
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
					logData = new Dictionary<string, object>();
					logData.Add("roleList", roleList);
					WriteToDiagnosticLog($"ManageMarketingCenterUser - Error getting role id {RoleList[0]} from role list.", logData);
					// write an error
					return $"Role id {RoleList[0]} not found";
				}

                foreach (var prop in PropertyList)
                {
                    mcProperties.Add(Convert.ToInt32(prop));
                }
            }

            WriteToDiagnosticLog($"ManageMarketingCenterUser - Using product login name. productUsername: {_productUsername}");
			MC.MarketingCenterUser mcUser = new MC.MarketingCenterUser();
			CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

			if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
			{
				WriteToDiagnosticLog("ManageMarketingCenterUser - Error looking for company id in bluebook.");
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

            logData = new Dictionary<string, object>();
			logData.Add("mcUser", mcUser);
			WriteToDiagnosticLog("ManageMarketingCenterUser - User details.", logData);

			if (string.IsNullOrEmpty(_productUsername))
			{
				// create user
				try
				{
					mcUser.AssignAllProperties = allPropertiesSelected;

					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);

					var url = _productUrl + $"/v2/contact?sourceid={(string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId)}";

					logData = new Dictionary<string, object>();
					logData.Add("url", url);
					WriteToDiagnosticLog("ManageMarketingCenterUser - Create user.", logData);
					WriteToDiagnosticLog($"ManageMarketingCenterUser - JSON input " + JsonConvert.SerializeObject(mcUser));
					var response = _httpClient.PostAsJsonAsync(url, mcUser).Result;

					if (response.IsSuccessStatusCode)
					{
						var userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
						logData = new Dictionary<string, object>();
						logData.Add("userResult", userResult);
						WriteToDiagnosticLog("ManageMarketingCenterUser - Create User. Got result from marketing center.", logData);
						_samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, userEmailAddress);
						// now the id!
						long newid = userResult.id;
						WriteToDiagnosticLog($"ManageMarketingCenterUser - Create user. newid={newid}");
						_samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid.ToString());
						WriteToDiagnosticLog("ManageMarketingCenterUser - Create user success. Saved user id");
						UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
						WriteToDiagnosticLog("ManageMarketingCenterUser - Create user success. Set product status to Success");

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
						WriteToDiagnosticLog("ManageMarketingCenterUser - Create user errored. Set product status to Error");
						return ParseErrorPosting(response, "Create", editorPersonaId, userPersonaId);
					}
				}
				catch (Exception ex)
				{
					// write an error
					WriteToDiagnosticLog($"ManageMarketingCenterUser - Create user errored. {ex.Message}");
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
					WriteToDiagnosticLog("ManageMarketingCenterUser - Create user errored. Set product status to Error");
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
						url = _productUrl + $"/v2/contact/{_productUserId}?sourceid={(string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId)}&assignAllProperties=true";
					}
					else
					{
						url = _productUrl + $"/v2/contact/{_productUserId}?sourceid={(string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId)}&unassignAllProperties=false";
					}
					logData = new Dictionary<string, object>();
					logData.Add("url", url);
					WriteToDiagnosticLog("ManageMarketingCenterUser - Update user.", logData);
					WriteToDiagnosticLog($"ManageMarketingCenterUser - JSON input " + JsonConvert.SerializeObject(mcUser));
					var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;

					if (response.IsSuccessStatusCode)
					{
						UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
						var userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
						logData = new Dictionary<string, object>();
						logData.Add("userResult", userResult);
						WriteToDiagnosticLog("ManageMarketingCenterUser - Update user. Got result from marketing center.", logData);
						long newid = userResult.id;
						WriteToDiagnosticLog("ManageMarketingCenterUser - Update user. newid=" + newid);
						UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid.ToString());
						WriteToDiagnosticLog("ManageMarketingCenterUser - Update user success. Saved user id");

						if (!IsUserIdValid(Convert.ToInt64(_editorProductUserId)))
						{
							string message = $"ManageMarketingCenterUser.ManageMarketingCenterUser - Invalid admin userId: {_editorProductUserId}";
							WriteToDiagnosticLog(message);
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
					WriteToDiagnosticLog($"ManageMarketingCenterUser - Update user errored. {ex.Message}");
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
					WriteToDiagnosticLog("ManageMarketingCenterUser - Update user errored. Set product status to Error");
					return "There was a problem updating the user";
				}
			}
			WriteToDiagnosticLog("ManageMarketingCenterUser - Done create/update user");

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
				WriteToErrorLog($"ManageProductMarketingCente.ChangeUserStatus - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
				return false;
			}
			try
			{
				return SetMarketingCenterUserStatus(isActive, _productUserId);
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageMarketingCenter.ChangeUserActiveStatus - Updating user status failed for user {companyInstanceSourceId}|{username} by editorPersonaId = {editorPersonaId}", exception: ex);
				return false;
			}
		}
		#endregion

		#region Private methods
		private bool CheckIfUserExistInProduct(string _productUserId)
		{
			var url = _productUrl + $"/v2/contact/details?emailAddress={_productUserId}";
			var response = _httpClient.GetAsync(url).Result;
			if (response.IsSuccessStatusCode)
			{
				WriteToDiagnosticLog($"ManageMarketingCenterUser.CheckIfUserExistInProduct - Email address {_productUserId} already exist in Marketing Center.");
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

				if (incrementor == 10)
				{
					// after 10 tries something might be wrong, so bail out.
					WriteToErrorLog($"ManageMarketingCenterUser - Error checking for username in use {userEmailAddress}");
					return "";
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
				Dictionary<string, object> logData = new Dictionary<string, object>();
				if (string.IsNullOrEmpty(_editorProductUserId))
				{
					WriteToDiagnosticLog("ManageMarketingCenterUser.SetMarketingCenterUserStatus - Update user status. Editor Product User Id cannot be null or empty.");
					return false;
				}
				if (mcUserId?.Length > 0 && mcUserId != "0")
				{
					string url = _productUrl + $"/v2/contact/{ mcUserId }/status";
					MC.MarketingCenterUserStatus mcUser = new MC.MarketingCenterUserStatus()
					{
						isActive = isActive,
						isActiveUnifiedUser = isActive,
						auditUserId = Convert.ToInt64(_editorProductUserId)
					};

					string mcUserJson = JsonConvert.SerializeObject(mcUser);

					logData = new Dictionary<string, object>
					{
						{ "url", url }
					};
					WriteToDiagnosticLog($"ManageMarketingCenterUser.SetMarketingCenterUserStatus - Update userId {mcUserId} url and user status request object: {mcUserJson}", logData);

					var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;
					dynamic userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
					logData = new Dictionary<string, object>
					{
						{ "userResult", userResult }
					};
					if (response.IsSuccessStatusCode)
					{
						WriteToDiagnosticLog($"ManageMarketingCenterUser.SetMarketingCenterUserStatus - Update userId {mcUserId} status. Got result from marketing center.", logData);
						return true;
					}
					else
					{
						WriteToDiagnosticLog($"ManageMarketingCenterUser.SetMarketingCenterUserStatus - Update userId {mcUserId} status errored.", logData);
						return false;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				WriteToDiagnosticLog($"ManageMarketingCenterUser.SetMarketingCenterUserStatus - Update user status errored. {ex.Message}");
				return false;
			}
		}

		private T GetResultFromApi<T>(string baseUrlAndQuery) where T : class
		{
			T results = null;
			Dictionary<string, object> logData = new Dictionary<string, object>();
			logData.Add("uri", baseUrlAndQuery);
			var response = _httpClient.GetAsync(baseUrlAndQuery).Result;
			if (response.IsSuccessStatusCode)
			{
				var jsonContent = response.Content.ReadAsStringAsync().Result;
				results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
			}
			else
			{
				logData = new Dictionary<string, object>
				{
					{ "error", response.Content.ReadAsStringAsync().Result },
					{ "status", response.StatusCode }
				};
				WriteToDiagnosticLog("GetAsync - Exiting after error. ", logData);
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
			catch (Exception)
			{
				// couldn't parse the response from the product, so move on
			}
			Dictionary<string, object> logData = new Dictionary<string, object>();
			logData.Add("response.Content.ReadAsStringAsync().Result", userResult);
			WriteToDiagnosticLog($"ManageMarketingCenterUser - {action} errored." + (emailAlreadyExists ? " Email already exists" : ""), logData);
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
					var url = _productUrl + $"/v2/contact/{_productUserId}/details";
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
			string url = _productUrl + $"/v2/contact/{ userId }/status";
			var responsex = _httpClient.GetAsync(url).Result;

			return responsex.IsSuccessStatusCode;
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
				WriteToErrorLog(
					$"ManageProductMarketingCenter.GetMigrationUsers.GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
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

			var url = $"{_productUrl}/v2/api/{companyInstanceSourceId}/users?filter-type={filter}&startRow={startRow}&resultsperpage={resultPerRow}";
			WriteToDiagnosticLog("ManageProductMarketingCenter.GetMigrationUsers", new Dictionary<string, object> { { "Url", url } });

			var migrationResponse = GetResultFromApi<MigrationResponse<IList<MigrationUser>>>(url);

			if (migrationResponse == null)
			{
				WriteToErrorLog($"ManageProductMarketingCenter.GetMigrationUsers-no users received from product for user with editorPersona id - {editorPersonaId}.");
				return response;
			}
			WriteToDiagnosticLog($"ManageProductMarketingCenter.GetUsers - Received users from product for user with editorPersona id - {editorPersonaId}.");
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
				WriteToErrorLog(
					$"ManageProductMarketingCenter.UpdateUsersMigrationStatus.GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
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

			var url = $"{_productUrl}/v2/api/{companyInstanceSourceId}/migrate-users";
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
				WriteToDiagnosticLog("ManageProductMarketingCenter.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
				migrateResponse.Message = migrationResponse.Data.Message;
				migrateResponse.Status = migrationResponse.Data.Status;
				return migrateResponse;
			}
			else
			{
				WriteToErrorLog($"ManageProductMarketingCenter.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
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
	}
}