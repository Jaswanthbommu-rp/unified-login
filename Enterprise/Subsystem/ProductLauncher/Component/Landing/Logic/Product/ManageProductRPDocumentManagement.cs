using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RPDocumentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Used to for RPDM user management
    /// </summary>
    public class ManageProductRPDocumentManagement : ManageProductBase, IManageProductRPDocumentManagement
	{
		private DefaultUserClaim _userClaims;

		#region Ctor

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims">user claim related information</param>
		public ManageProductRPDocumentManagement(DefaultUserClaim userClaims) : base((int) ProductEnum.RPDocumentManagement, userClaims, null)
		{
			_userClaims = userClaims;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_blueBook = new ManageBlueBook(userClaims);
			_productUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
			string _apiUser = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value));
			string _apiPassword = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value));
			
			_client.SetBasicAuthentication(_apiUser, _apiPassword);
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="userClaims">user claim related information</param>
		/// <param name="client"></param>
		/// <param name="productInternalSettingRepository"></param>
		/// <param name="managePersona"></param>
		/// <param name="samlRepository"></param>
		/// <param name="blueBook"></param>
		/// <param name="managePerson"></param>
		/// <param name="manageUserLogin"></param>
		/// <param name="manageContactMechanism"></param>
		/// <param name="managePartyRelationship"></param>
		/// <param name="productRepository"></param>
		/// <param name="userLoginRepository"></param>
		public ManageProductRPDocumentManagement(DefaultUserClaim userClaims, HttpClient client, IProductInternalSettingRepository productInternalSettingRepository,
			IManagePersona managePersona, ISamlRepository samlRepository, IManageBlueBook blueBook, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManageContactMechanism manageContactMechanism, IManagePartyRelationship managePartyRelationship, IProductRepository productRepository, IUserLoginRepository userLoginRepository)
			: base((int) ProductEnum.RPDocumentManagement, userClaims, productInternalSettingRepository)
		{
			_userClaims = userClaims;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_client = client;
			_productInternalSettingRepository = productInternalSettingRepository;
			_blueBook = blueBook;
			_managePersona = managePersona;
			_samlRepository = samlRepository;
			_managePerson = managePerson;
			_manageUserLogin = manageUserLogin;
			_manageContactMechanism = manageContactMechanism;
			_managePartyRelationship = managePartyRelationship;
			_productRepository = productRepository;
			_productUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _userLoginRepository = userLoginRepository;

        }

		#endregion

		#region Public Methods
		/// <summary>
		/// Used to get the domain for a user persona
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns>The domain of the company</returns>
		public ListResponse GetDomain(long personaId)
		{
			ListResponse response = GetCompanyEditorAndUserDetails(personaId, 0);
			if (response.IsError)
			{
				return response;
			}

			string domain = GetDomain();

			response = new ListResponse(){ Additional = domain };
			return response;
		}

		/// <summary>
		/// Used to get roles for a company or user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError)
			{
				return response;
			}

			return GetRoles(editorPersonaId, userPersonaId);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="roleId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetRoleClassifierDataset(long editorPersonaId, long userPersonaId, string roleId, RequestParameter datafilter)
		{
			ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError)
			{
				return response;
			}

			return GetRoleClassifierDataset(editorPersonaId, userPersonaId, roleId);
		}

		/// <summary>
		/// Updated to create/update a user
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId"></param>
		/// <param name="rolePropertyEntityList">The role, property or department to assign the user</param>
		/// <returns></returns>
		public string ManageRPDMUser(long editorPersonaId, long userPersonaId, RolePropertyList rolePropertyEntityList)
		{
			ListResponse response = new ListResponse();
			response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError)
			{
				return response.ErrorReason;
			}

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
			Guid realPageId = userPersona.RealPageId;
			Dictionary<string, object> logData = new Dictionary<string, object>();

			Person person = _managePerson.GetPerson(realPageId);

			var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

			IList<CommonAddress> contactMechansimList = _manageContactMechanism.ListContactMechanismForPerson(realPageId, null);

			// get the email address
			string userEmailAddress = "";
			if (contactMechansimList.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY"))
			{
				userEmailAddress = (from a in contactMechansimList where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "PRIMARY" select a.AddressString).FirstOrDefault();
			}
			else
			{
				// this should probably look like a real email, need to test if it isn't
				userEmailAddress = userLogin.LoginName;
			}

			// verify email address looks valid, will fail if not
			userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

			bool isSuperUser = IsSuperUser(userPersona.PersonaId);

			// get the user phone
			string userPhoneNumber = "555-555-5555";
			if (contactMechansimList.Any(a => a.AddressType?.ToUpper() == "PHONE" && a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY"))
			{
				userPhoneNumber = (from a in contactMechansimList where a.AddressType.ToUpper() == "PHONE" && a.contactMechanismUsageType.Name.ToUpper() == "PRIMARY" select a.AddressString).FirstOrDefault();
			}

			if (string.IsNullOrEmpty(_productUserId))
			{
				// get a login name that isn't in use for the new user
				bool foundNewUserName = false;
				int incrementor = 0;
				string newproductUsername = (person.FirstName.Substring(0, 1) + person.LastName.Substring(0, (person.LastName.Length >= 19 ? 19 : person.LastName.Length))).ToLower();
				_productUsername = newproductUsername.Replace(" ", "");
				while (!foundNewUserName)
				{
					if (CheckIfUserLoginIsUsed(_productUsername))
					{
						incrementor++;
						_productUsername = newproductUsername + incrementor.ToString();
					}
					else
					{
						foundNewUserName = true;
					}
				}
			}

			if (!isSuperUser && (rolePropertyEntityList == null || rolePropertyEntityList.RoleList == null || rolePropertyEntityList.RoleList.Count == 0))
			{
				WriteToDiagnosticLog("ManageRPDMUser - Create user error. RoleList.Count=" + rolePropertyEntityList?.RoleList?.Count.ToString() + " , PropertyList.Count=" + rolePropertyEntityList?.PropertyList?.Count.ToString() + " , DepartmentList.Count=" + rolePropertyEntityList?.DepartmentList?.Count.ToString());
				return "There was a problem creating the user. Missing required information.";
			}

			// get the roles and find the ones passed
			RPDMResult<RPDMRole> rpdmResult = GetResultFromApi<RPDMResult<RPDMRole>>("/roles?isApi=true", "name");
			if (rpdmResult == null)
			{
				WriteToErrorLog("ManageRPDMUser - Error getting roles. rpdmResult == null");
				return "There was a problem creating the user. Missing role details";
			}

			string domain = GetDomain();
			if (domain.Contains("There was a problem creating the user"))
			{
				return domain;
			}

			RPDMUser manageUser = new RPDMUser()
			{
				Domain = domain,
				FirstName = person.FirstName,
				LastName = person.LastName,
				Name = _productUsername,
				Email = userEmailAddress,
				TimeZone = "US/Central",
				Locale = "en",
				Enabled = true,
			};

			if (!isSuperUser)
			{
				manageUser.Roles = new List<RPDMUserRoles>();
				// fix up the users roles/property/department info
				try
				{
					foreach (string roleId in rolePropertyEntityList.RoleList)
					{
						if (rpdmResult.Page.Any(p => p.ID == roleId))
						{
							RPDMRole roleDetail = (from a in rpdmResult.Page where a.ID == roleId select a).FirstOrDefault();
							RPDMRoleDetail rpdmRoleDetail = GetResultFromApi<RPDMRoleDetail>("/roles/" + roleId);
							IList<ProductProperty> list = new List<ProductProperty>();
							if (rpdmRoleDetail.Scope != null)
							{
								if (rolePropertyEntityList?.PropertyList?.Count > 0 || rolePropertyEntityList?.DepartmentList?.Count > 0)
								{
									// get additional information for the role details
									if (!string.IsNullOrEmpty(rpdmRoleDetail.Scope.HRef))
									{
										RPDMClassifier classifier = GetResultFromApi<RPDMClassifier>(rpdmRoleDetail.Scope.HRef);
										if (classifier != null && classifier.DataSet.HRef != null)
										{
											RPDMResult<RPDMDataset> rpdmDataSetResults = GetResultFromApi<RPDMResult<RPDMDataset>>(classifier.DataSet.HRef + "/values", "name");
											if (rpdmDataSetResults.Page.Count > 0)
											{
												InsertRoleDetails(rolePropertyEntityList.PropertyList, rpdmDataSetResults, roleDetail, rpdmRoleDetail, manageUser);
												InsertRoleDetails(rolePropertyEntityList.DepartmentList, rpdmDataSetResults, roleDetail, rpdmRoleDetail, manageUser);
											}
										}
									}
								}
								else
								{
									RPDMUserRoles ur = new RPDMUserRoles
									{
										Role = new RPDMScope() {HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name}
									};
									manageUser.Roles.Add(ur);
								}
							}
							else
							{
								RPDMUserRoles ur = new RPDMUserRoles
								{
									Role = new RPDMScope() {HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name}
								};
								manageUser.Roles.Add(ur);
							}
						}
					}
				}
				catch (Exception ex)
				{
					WriteToErrorLog("ManageRPDMUser - Create user errored. " + ex.Message, null, ex);
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductBatchStatusType.Error);
					WriteToDiagnosticLog("ManageRPDMUser - Create user errored. Set product status to Error");
					// write an error
					return "There was a problem creating the user. " + ex.Message;
				}
			}
			else
			{
				if (rpdmResult.Page.Any(p => p.Name.ToUpper() == "DOMAIN ADMIN"))
				{
					RPDMRole domainAdmin = (from a in rpdmResult.Page where a.Name.ToUpper() == "DOMAIN ADMIN" select a).FirstOrDefault();
					manageUser.Roles = new List<RPDMUserRoles>();
					RPDMUserRoles ur = new RPDMUserRoles
					{
						Role = new RPDMScope() { HRef = domainAdmin.HRef, Id = domainAdmin.ID, Name = domainAdmin.Name }
					};
					manageUser.Roles.Add(ur);
				}
			}

			if (string.IsNullOrEmpty(_productUserId))
			{
				// create user
				try
				{
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductBatchStatusType.Running);
					var url = _productUrl.Replace("{{domain}}", domain) + $"/api/{domain}" + "/users/newuser";
					logData = new Dictionary<string, object>() {{"url", url}, {"manageUser", JsonConvert.SerializeObject(manageUser)}};
					WriteToDiagnosticLog("ManageRPDMUser - Create user", logData);

					var postResponse = _client.PostAsJsonAsync(url, manageUser).Result;
					string newid = "";
					if (postResponse.IsSuccessStatusCode)
					{
						var userResult = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
						// parse the result
						newid = userResult.target.href;
						newid = newid.Split('/')[newid.Split('/').Count() - 1].ToString(); // parse the id from the href result
						RPDMUser newUser = GetUserDetails(newid);
						if (newUser != null)
						{
							_samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
							_samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, newUser.Name);
							WriteToDiagnosticLog($"ManageRPDMUser - Create user. newid={newid}, login={newUser.Name}");
							UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
							WriteToDiagnosticLog("ManageRPDMUser - Create user success. Set product status to Success");
							WriteCreateUserActivityLog(editorPersonaId, person, userLogin);

							//Update the user in Spend Management as a migrated user
							MigrateResponse migrateResponse = new MigrateResponse();
							IList<MigrateUser> migrateUsers = new List<MigrateUser>()
							{
								new MigrateUser()
								{
									UserId = newid,
									UnifiedLoginUserName = userEmailAddress,
									UsingUnifiedLogin = true
								}
							};
						}

						//migrateResponse = UpdateUsersMigrationStatus(editorPersonaId, migrateUsers);
					}
					else
					{
						logData = new Dictionary<string, object>() {{"postResponse.Content", postResponse.Content.ReadAsStringAsync().Result}};
						WriteToDiagnosticLog("ManageRPDMUser - Create user errored.", logData);
						UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductBatchStatusType.Error);
						WriteToDiagnosticLog("ManageRPDMUser - Create user errored. Set product status to Error");
						// write an error
						return "There was a problem creating the user. " + postResponse.Content.ReadAsStringAsync().Result;
					}
				}
				catch (Exception ex)
				{
					WriteToErrorLog("ManageRPDMUser - Create user errored. " + ex.Message, null, ex);
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductBatchStatusType.Error);
					WriteToDiagnosticLog("ManageRPDMUser - Create user errored. Set product status to Error");
					// write an error
					return "There was a problem creating the user. " + ex.Message;
				}
			}
			else
			{
				// update user
				try
				{
					RPDMUser currentUser = GetUserDetails(_productUserId);

					manageUser.Id = _productUserId;
					manageUser.TimeZone = currentUser.TimeZone;
					manageUser.Locale = currentUser.Locale;
					manageUser.Photo = currentUser.Photo;
					manageUser.Groups = currentUser.Groups;
					var url = "";

					if (currentUser.Enabled == false)
					{
						// reactivate the user
						url = _productUrl.Replace("{{domain}}", domain) + $"/api/{domain}/users/{_productUserId}/enable";
						logData = new Dictionary<string, object>() { { "url", url } };
						WriteToDiagnosticLog($"ManageRPDMUser - Update user {_productUserId}, enable disabled user", logData);
						var postEnableResponse = _client.PostAsJsonAsync(url, manageUser).Result;
						if (postEnableResponse.IsSuccessStatusCode || postEnableResponse.StatusCode == System.Net.HttpStatusCode.NotModified)
						{
							WriteToDiagnosticLog($"ManageRPDMUser - Update user {_productUserId}, enable disabled user success", logData);
							WriteUpdateUserActivityLog(editorPersonaId, person, userLogin);
						}
						else
						{
							logData = new Dictionary<string, object>() { { "postEnableResponse.Content", postEnableResponse.Content.ReadAsStringAsync().Result } };
							WriteToDiagnosticLog($"ManageRPDMUser - Update user {_productUserId} errored.", logData);
							// write an error
							return "There was a problem updating the user. " + postEnableResponse.Content.ReadAsStringAsync().Result;
						}
					}

					url = _productUrl.Replace("{{domain}}", domain) + $"/api/{domain}" + $"/users/{_productUserId}";
					logData = new Dictionary<string, object>() {{"url", url}, {"manageUser", JsonConvert.SerializeObject(manageUser)}};
					WriteToDiagnosticLog("ManageRPDMUser - Update user", logData);

					var postUpdateResponse = _client.PostAsJsonAsync(url, manageUser).Result;

					if (postUpdateResponse.IsSuccessStatusCode || postUpdateResponse.StatusCode == System.Net.HttpStatusCode.NotModified)
					{
						UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductBatchStatusType.Success);
						logData = new Dictionary<string, object>() {{"postResponse.Content", postUpdateResponse.Content.ReadAsStringAsync().Result}};
						WriteToDiagnosticLog("ManageRPDMUser - Update user success. Set product status to Success", logData);
						WriteUpdateUserActivityLog(editorPersonaId, person, userLogin);
					}
					else
					{
						logData = new Dictionary<string, object>() {{"postResponse.Content", postUpdateResponse.Content.ReadAsStringAsync().Result}};
						WriteToDiagnosticLog("ManageRPDMUser - Update user errored.", logData);
						// write an error
						return "There was a problem updating the user. " + postUpdateResponse.Content.ReadAsStringAsync().Result;
					}
				}
				catch (Exception ex)
				{
					//UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
					WriteToDiagnosticLog("ManageOpsUser - Create user errored. " + ex.Message);
					return "There was a problem updating the user";
				}
			}

			return "";
		}
		#endregion

		#region Privates
		/// <summary>
		/// Used to add roles to a user
		/// </summary>
		/// <param name="list"></param>
		/// <param name="rpdmDataSetResults"></param>
		/// <param name="roleDetail"></param>
		/// <param name="rpdmRoleDetail"></param>
		/// <param name="manageUser"></param>
		private static void InsertRoleDetails(List<string> list, RPDMResult<RPDMDataset> rpdmDataSetResults, RPDMRole roleDetail, RPDMRoleDetail rpdmRoleDetail, RPDMUser manageUser)
		{
			if (list == null)
			{
				return;
			}

			foreach (string id in list)
			{
				if (rpdmDataSetResults.Page.Any(p => p.Id == id))
				{
					RPDMDataset dataset = (from a in rpdmDataSetResults.Page where a.Id == id select a).FirstOrDefault();
					RPDMUserRoles ur = new RPDMUserRoles
					{
						Role = new RPDMScope() {HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name}
					};
					ur.Entity = new RPDMScope()
					{
						Id = dataset.Id,
						HRef = dataset.HRef,
						Name = dataset.Name,
						Rel = rpdmRoleDetail.Type
					};
					manageUser.Roles.Add(ur);
				}
			}
		}

		/// <summary>
		/// Used to get the DocManagement domain for the give company
		/// </summary>
		/// <returns></returns>
		private string GetDomain()
		{
			string domain = "";
			CustomerCompanyMap companyMap = GetRPDocumentManagementCompanyInstanceId();
			try
			{
				IList<InstanceAttribute> listInstanceAttribute = companyMap.CompanyInstance[0].Attributes;
				if (listInstanceAttribute.ToList().Any(a => a.AttributeName.ToUpper() == "DOMAIN ID"))
				{
					InstanceAttribute instanceAttribute = listInstanceAttribute.ToList().Find(a => a.AttributeName.ToUpper() == "DOMAIN ID");
					domain = instanceAttribute.AttributeValue;
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog("ManageRPDMUser - There was a problem getting the DocManagement attribute in BlueBook", exception: ex);
				return "There was a problem getting the company details from BlueBook.";
			}

			return domain;
		}

		/// <summary>
		/// Used to see if a new user login being added already exists or not
		/// </summary>
		/// <param name="userLogin"></param>
		/// <returns></returns>
		private bool CheckIfUserLoginIsUsed(string userLogin)
		{
			//ListResponse response = new ListResponse();
			//response = GetCompanyEditorAndUserDetails(editorPersonaId, 0);

			bool userExists = false;
			RPDMResult<RPDMDataset> user = GetUserDetailsByLoginName(userLogin);

			if (user.Page.Count > 0)
			{
				userExists = true;
			}

			return userExists;
		}

		/// <summary>
		/// Used to unassign a user from the product
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		public string UnassignUser(long editorPersonaId, long userPersonaId)
		{
			ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError) { return listResponse.ErrorReason; }
                       
            
            string domain = GetDomain();

			var url = _productUrl.Replace("{{domain}}", domain) + $"/api/{domain}/users/{_productUserId}/disable";
			Dictionary<string, object> logData = new Dictionary<string, object>() {{"url", url}};
			WriteToDiagnosticLog("ManageRPDMUser - UnassignUser - Disable user", logData);

			var postResponse = _client.PostAsJsonAsync(url, "").Result;
			if (!postResponse.IsSuccessStatusCode)
			{
				// write an error
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
				WriteToDiagnosticLog("ManageRPDMUser - UnassignUser user errored. Set product status to Error");
				return "Error";
			}
			
			// Activity Logging
			WriteUnassignActivityLog(editorPersonaId, userPersonaId);
			
			WriteToDiagnosticLog($"ManageRPDMUser - UnassignUser - Successfully Disabled user userPersonaId:{userPersonaId}");
			UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

			return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		private ListResponse GetRoles(long editorPersonaId, long userPersonaId)
		{
			ListResponse response = new ListResponse();

			try
			{
				RPDMResult<RPDMRole> rpdmResult = GetResultFromApi<RPDMResult<RPDMRole>>("/roles?isApi=true", "name");
				if (rpdmResult == null)
				{
					WriteToErrorLog("GetRoles - Error. rpdmResult == null");
					response.IsError = true;
					response.ErrorReason = "There was a problem getting the role details";
					return response;
				}

				IList<ProductRole> list = GetAdditionalRoleDetails(rpdmResult);

				// flag any roles that are assigned to the given user
				if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
				{
					RPDMUser user = GetUserDetails(_productUserId);
					if (user?.Roles?.Count > 0)
					{
						List<RPDMUserRoles> userRoles = user.Roles;
						foreach (ProductRole pr in list)
						{
							foreach (RPDMUserRoles ur in userRoles)
							{
								if (pr.ID == ur.Role?.Id?.ToUpper())
								{
									pr.IsAssigned = true;
									break;
								}
							}
						}
					}
				}

				if (list != null)
				{
					list = list.OrderBy(p => p.Name).ToList();

					response = new ListResponse()
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
					WriteToErrorLog("GetRoles - Error. list == null");
					response.IsError = true;
					response.ErrorReason = "There was a problem getting the role details";
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog("GetRoles - Error. " + ex.Message, exception: ex);
				response.IsError = true;
				response.ErrorReason = "There was a problem getting the role details";
			}

			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		private ListResponse GetRoleClassifierDataset(long editorPersonaId, long userPersonaId, string roleId)
		{
			ListResponse response = new ListResponse();

			try
			{
				RPDMRoleDetail rpdmRoleDetail = GetResultFromApi<RPDMRoleDetail>("/roles/" + roleId);
				IList<ProductProperty> list = new List<ProductProperty>();
				if (rpdmRoleDetail.Scope != null)
				{
					// get additional information for the role details
					if (!string.IsNullOrEmpty(rpdmRoleDetail.Scope.HRef))
					{
						RPDMClassifier classifier = GetResultFromApi<RPDMClassifier>(rpdmRoleDetail.Scope.HRef);
						if (classifier != null && classifier.DataSet.HRef != null)
						{
							RPDMResult<RPDMDataset> rpdmResult = GetResultFromApi<RPDMResult<RPDMDataset>>(classifier.DataSet.HRef + "/values", "name");
							if (rpdmResult?.Page.Count > 0)
							{
								list = rpdmResult.Page.ToGBProperties();
								// flag any values that are assigned to the given user
								if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
								{
									RPDMUser user = GetUserDetails(_productUserId);
									if (user.Roles?.Count > 0)
									{
										List<RPDMUserRoles> userRoles = user.Roles;
										foreach (ProductProperty pp in list)
										{
											foreach (RPDMUserRoles ur in userRoles)
											{
												if (pp.ID == ur.Entity?.Id?.ToUpper())
												{
													pp.IsAssigned = true;
													break;
												}
											}
										}
									}
								}
							}
						}
					}
				}

				if (list != null)
				{
					list = list.OrderBy(p => p.Name).ToList();
					response = new ListResponse()
					{
						Records = list.Cast<object>().ToList(),
						TotalRows = list.Count,
						RowsPerPage = list.Count,
						TotalPages = 1,
						ErrorReason = ""
					};
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog("GetRoles - Error. " + ex.Message, exception: ex);
				response.IsError = true;
				response.ErrorReason = "There was a problem getting the classifier";
			}

			return response;
		}

		/// <summary>
		/// Get CompanyInstanceId
		/// </summary>
		/// <returns>CompanyMap</returns>
		private CustomerCompanyMap GetRPDocumentManagementCompanyInstanceId()
		{
			return GetProductCompanyInstanceId(BlueBookProductConstants.RPDocumentManagement, "companyInstance.attributes");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rpdmRoleResult"></param>
		/// <returns></returns>
		private List<ProductRole> GetAdditionalRoleDetails(RPDMResult<RPDMRole> rpdmRoleResult)
		{
			List<ProductRole> roleList = new List<ProductRole>();

			if (rpdmRoleResult.Page.Count > 0)
			{
				foreach (RPDMRole role in rpdmRoleResult.Page)
				{
					RPDMRoleDetail rpdmRoleDetail = GetResultFromApi<RPDMRoleDetail>("/roles/" + role.ID);
					if (rpdmRoleDetail == null)
					{
						return null;
					}

					ProductRole pr = new ProductRole()
					{
						ID = role.ID,
						Name = role.Name,
						Alias = role.HRef
					};
					if (rpdmRoleDetail.Scope != null && !string.IsNullOrEmpty(rpdmRoleDetail.Scope.Name))
					{
						pr.Roletype = rpdmRoleDetail.Scope.Name;
					}

					roleList.Add(pr);
				}
			}

			return roleList;
		}

		private RPDMUser GetUserDetails(string userId)
		{
			return GetResultFromApi<RPDMUser>("/users/" + userId);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="loginName"></param>
		/// <returns></returns>
		private RPDMResult<RPDMDataset> GetUserDetailsByLoginName(string loginName)
		{
			return GetResultFromApi<RPDMResult<RPDMDataset>>($"/users?s=username({loginName})");
		}

		/// <summary>
		/// Used to get data from RPDM
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="additionalQuery"></param>
		/// <param name="sortBy">Used to sort the result data</param>
		/// <returns></returns>

		private T GetResultFromApi<T>(string additionalQuery, string sortBy = null) where T : class
		{
			T results = null;
			string sortByAdditional = "";

			string domain = GetDomain();
			if (domain.Contains("There was a problem creating the user") || string.IsNullOrEmpty(domain))
			{
				WriteToErrorLog($"Error - No CompanyInstanceSourceId found. ManageProductRPDM.GetResultFromApi, additionalQuery {additionalQuery}");
				return null;
			}

			if (!string.IsNullOrEmpty(sortBy))
			{
				sortByAdditional = "&sort=" + sortBy;
			}

			if (additionalQuery.Contains("?"))
			{
				additionalQuery += "&pageSize=9999";
			}
			else
			{
				additionalQuery += "?pageSize=9999";
			}

			var url = _productUrl.Replace("{{domain}}", domain) + $"/api/{domain}" + additionalQuery + sortByAdditional;
			Dictionary<string, object> logData = new Dictionary<string, object>() {{"url", url}};
			WriteToDiagnosticLog("GetResultFromApi - Posting to url. ", logData);
			try
			{
				var response = _client.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					results = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(T)) as T;
				}
				else
				{
					WriteToErrorLog($"Error - Response is not 200. ManageProductRPDM.GetResultFromApi, url {url}, StatusCode - {response.StatusCode}");
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog($"ManageProductRPDM.GetResultFromApi - Error. url {url} " + ex.Message, exception: ex);
			}

			return results;
		}
		#endregion
	}

	/// <summary>
	/// A list of data from the RPDM service
	/// </summary>
	public class RPDMResult<T>
	{
		/// <summary>
		/// The number of records in the role list
		/// </summary>
		public int Size { get; set; }

		/// <summary>
		/// The list roles for the given company
		/// </summary>
		public List<T> Page { get; set; }
	}

	public static class RPDMHelpers
	{
		/// <summary>
		/// Used to convert a dataset into a GreenBook property
		/// </summary>
		/// <param name="listDataset">The list of data to convert</param>
		/// <returns>list of ProductProperty</returns>
		public static IList<ProductProperty> ToGBProperties(this IList<RPDMDataset> listDataset)
		{
			string state = string.Empty;
			if (listDataset == null)
			{
				return null;
			}

			IList<ProductProperty> listProductProperty = new List<ProductProperty>();
			foreach (RPDMDataset dm in listDataset)
			{
				listProductProperty.Add(new ProductProperty
				{
					ID = dm.Id,
					Name = dm.Name,
					disableSelection = null,
					State = null,
					Alias = dm.HRef
				});
			}

			return listProductProperty;
		}
	}
}