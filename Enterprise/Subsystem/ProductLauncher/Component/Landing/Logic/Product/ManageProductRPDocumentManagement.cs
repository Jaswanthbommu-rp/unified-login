using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
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
using System.Text.RegularExpressions;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Used to for RPDM user management
    /// </summary>
    public class ManageProductRPDocumentManagement : ManageProductBase, IManageProductRPDocumentManagement
	{
        private readonly DefaultUserClaim _userClaims;
        private readonly List<ProductInternalSetting> _unifiedLoginSettings;
		#region Ctor

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="userClaims">user claim related information</param>
		public ManageProductRPDocumentManagement(DefaultUserClaim userClaims) : base((int) ProductEnum.RPDocumentManagement, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
			_userClaims = userClaims;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_blueBook = new ManageBlueBook(userClaims);
			_productUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _unifiedLoginSettings = GetProductSetting((int) ProductEnum.UnifiedPlatform);
            string _apiUser = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value));
			string _apiPassword = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value));
			
			_client.SetBasicAuthentication(_apiUser, _apiPassword);
		}

        /// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="userClaims"></param>
		/// <param name="httpMessageHandler"></param>
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
		/// <param name="repository"></param>
        public ManageProductRPDocumentManagement(DefaultUserClaim userClaims, HttpMessageHandler httpMessageHandler, HttpClient client, IProductInternalSettingRepository productInternalSettingRepository,
			IManagePersona managePersona, ISamlRepository samlRepository, IManageBlueBook blueBook, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManageContactMechanism manageContactMechanism, IManagePartyRelationship managePartyRelationship, IProductRepository productRepository, IUserLoginRepository userLoginRepository, IRepository repository)
			: base((int) ProductEnum.RPDocumentManagement, userClaims, repository, httpMessageHandler)
		{
			_userClaims = userClaims;
			_editorRealPageId = userClaims.UserRealPageGuid;
			_client = client;
            _messageHandler = httpMessageHandler;
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

			return GetRoles(userPersonaId);
		}

		/// <summary>
		/// Used to get roles along with properties for a company or user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		public ListResponse GetPropertyRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
		{
			ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (response.IsError)
			{
				return response;
			}
			Persona editorPersona = response.Records[0] as Persona;
			
			return GetPropertyRoles(userPersonaId, editorPersona.Organization.PartyId);
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

			return GetRoleClassifierDataset(userPersonaId, roleId);
		}

		/// <summary>
		/// Updated to create/update a user
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId"></param>
		/// <param name="rolePropertyEntityList">The role, property or department to assign the user</param>
		/// <param name="additionalParameters"></param>
		/// <returns></returns>
		public string ManageRPDMUser(long editorPersonaId, long userPersonaId, RolePropertyList rolePropertyEntityList, out List<AdditionalParameters> additionalParameters)
		{
			List<PAMRolePropertyList> lstRoleProperties = new List<PAMRolePropertyList>();
			var response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			additionalParameters = new List<AdditionalParameters>();
			if (response.IsError)
			{
				return response.ErrorReason;
			}

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
			Guid realPageId = userPersona.RealPageId;
			Dictionary<string, object> logData = new Dictionary<string, object>();

			Person person = _managePerson.GetPerson(realPageId);

			//Removing Special Characters for First Name and Last Name
			person.FirstName = Regex.Replace(person.FirstName, @"[^A-Za-z0-9]+", "");
			person.LastName = Regex.Replace(person.LastName, @"[^A-Za-z0-9]+", "");

			var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

			IList<CommonAddress> contactMechansimList = _manageContactMechanism.ListContactMechanismForPerson(realPageId, null);

			var userBeforeUpdate = !string.IsNullOrEmpty(_productUserId) ? GetUserDetails(_productUserId) : new RPDMUser() { Roles = new List<RPDMUserRoles>() { new RPDMUserRoles() { Role = new RPDMScope(), Entity = new RPDMScope() } }, Groups = new List<RPDMScope>() };

            // get the email address
            var userEmailAddress = GetEmailAddress(contactMechansimList , userLogin);			
			bool isSuperUser = IsSuperUser(userPersona.PersonaId);			
			string insUpdResult = string.Empty;
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

			// setting roles and properties values if it is not a dynamic panel
			if (rolePropertyEntityList.RolePropertiesList== null && (rolePropertyEntityList?.RoleList?.Count > 0 || rolePropertyEntityList?.PropertyList?.Count > 0 || rolePropertyEntityList?.DepartmentList?.Count > 0))
			{
				if (rolePropertyEntityList.DepartmentList?.Count > 0 && rolePropertyEntityList.PropertyList?.Count > 0)
				{
					rolePropertyEntityList.PropertyList.AddRange(rolePropertyEntityList.DepartmentList);
				}
				else if(rolePropertyEntityList.DepartmentList?.Count > 0)
				{
					rolePropertyEntityList.PropertyList = rolePropertyEntityList.DepartmentList;
				}
				
				foreach (string roleId in rolePropertyEntityList.RoleList)
				{
					PAMRolePropertyList objRole = new PAMRolePropertyList();
					objRole.RoleId = roleId;
					objRole.PropertyIds = rolePropertyEntityList.PropertyList;
					lstRoleProperties.Add(objRole);
				}
				rolePropertyEntityList.RolePropertiesList = lstRoleProperties;
			}

			if (!isSuperUser && (rolePropertyEntityList == null || rolePropertyEntityList.RolePropertiesList == null || rolePropertyEntityList.RolePropertiesList.Count == 0))
			{
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", $"Create user error. RoleList.Count={rolePropertyEntityList?.RolePropertiesList?.Count.ToString()}" });
				return "There was a problem creating the user. Missing required information.";
			}

			// get the roles and find the ones passed
			RPDMResult<RPDMRole> rpdmResult = GetResultFromApi<RPDMResult<RPDMRole>>("/roles?isApi=true", "name");
			if (rpdmResult == null)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", "Error getting roles. rpdmResult == null" });
				return "There was a problem creating the user. Missing role details";
			}

			RPDMUser manageUser = NewRPDMUser(userEmailAddress , person);

            //Validate Domain
            if (manageUser.Domain.Contains("There was a problem creating the user"))
            {
                return manageUser.Domain;
            }


            manageUser.Roles = new List<RPDMUserRoles>();
            // fix up the users roles/property/department info
            try
            {
				if (rolePropertyEntityList?.RolePropertiesList?.Count > 0)
				{
					foreach (PAMRolePropertyList role in rolePropertyEntityList.RolePropertiesList)
					{
						if (rpdmResult.Page.Exists(p => p.ID == role.RoleId))
						{
							RPDMRole roleDetail = (from a in rpdmResult.Page where a.ID == role.RoleId select a).FirstOrDefault();
							RPDMRoleDetail rpdmRoleDetail = GetResultFromApi<RPDMRoleDetail>("/roles/" + role.RoleId);
							if (rpdmRoleDetail.Scope != null)
							{
								if (rolePropertyEntityList?.RolePropertiesList?.Count > 0)
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
												InsertRoleDetails(role.PropertyIds, rpdmDataSetResults, roleDetail, rpdmRoleDetail, manageUser);
											}
										}
									}
								}
								else
								{
									RPDMUserRoles ur = new RPDMUserRoles
									{
										Role = new RPDMScope() { HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name }
									};
									manageUser.Roles.Add(ur);
								}
							}
							else
							{
								RPDMUserRoles ur = new RPDMUserRoles
								{
									Role = new RPDMScope() { HRef = roleDetail.HRef, Id = roleDetail.ID, Name = roleDetail.Name }
								};
								manageUser.Roles.Add(ur);
							}
						}
					}
				}
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", $"Create user errored. {ex.Message}" }, exception: ex);
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", "Create user errored. Set product status to Error" });
                // write an error
                return "There was a problem creating the user. " + ex.Message;
            }
			if (isSuperUser && !(manageUser.Roles.Any(p => p.Role.Name.ToUpper() == "DOMAIN ADMIN")))
			{
				if (rpdmResult.Page.Exists(p => p.Name.ToUpper() == "DOMAIN ADMIN"))
				{
					RPDMRole domainAdmin = (from a in rpdmResult.Page where a.Name.ToUpper() == "DOMAIN ADMIN" select a).FirstOrDefault();
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
					var url = _productUrl.Replace("{{domain}}", manageUser.Domain) + $"/api/{manageUser.Domain}" + "/users/newuser";
					logData = new Dictionary<string, object>() {{"url", url}, {"manageUser", JsonConvert.SerializeObject(manageUser)}};
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", "Create user" }, logData: logData);

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
							WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", $"Create user. newid={newid}, login={newUser.Name}" });
							UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
							WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", "Create user success. Set product status to Success" });
						}
					}
					else
					{
						logData = new Dictionary<string, object>() {{"postResponse.Content", postResponse.Content.ReadAsStringAsync().Result}};
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", "Create user errored." }, logData: logData);
						UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductBatchStatusType.Error);
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", "Create user errored. Set product status to Error" });
						// write an error
						return "There was a problem creating the user. " + postResponse.Content.ReadAsStringAsync().Result;
					}
				}
				catch (Exception ex)
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", $"Create user errored. {ex.Message}" }, exception: ex);
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int) ProductBatchStatusType.Error);
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRPDMUser", "Create user errored. Set product status to Error" });
					// write an error
					return "There was a problem creating the user. " + ex.Message;
				}
			}
			else
			{
                // update user
                insUpdResult =  UpdateRPDMUser(manageUser, userPersonaId);
			}

			//Activity logs
			if (string.IsNullOrEmpty(insUpdResult))
			{
				//Roles
				var oldAccessCodes = userBeforeUpdate.Roles.Where(a => a.Role.Id != null).Select(s => s.Role.Id);
                var newAccessCodes = manageUser.Roles.Select(s => s.Role.Id);

				var mergedRoles = userBeforeUpdate.Roles.Concat(manageUser.Roles);

                var removedRoles = oldAccessCodes.Except(newAccessCodes).ToList();
                var addedRoles = newAccessCodes.Except(oldAccessCodes).ToList();

                if (removedRoles.Any())
                {
                    foreach (string r in removedRoles)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Document Director Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", mergedRoles.FirstOrDefault(f => f.Role.Id == r).Role.Name) });
                    }
                }
                if (addedRoles.Any())
                {
                    foreach (string r in addedRoles)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Document Director Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", mergedRoles.FirstOrDefault(f => f.Role.Id == r).Role.Name) });
                    }
                }

				//Groups
				var oldGroups = userBeforeUpdate.Roles.Exists(s => s.Entity != null) ? userBeforeUpdate.Roles
																					   .Where(s => s.Entity?.Id != null)
																					   .Select(x => x.Entity.HRef).ToList()
																	 : new List<string>();
				var newGroups = manageUser.Roles.Exists(s => s.Entity != null) ? manageUser.Roles.Where(s => s.Entity?.Id != null)
																				 .Select(x => x.Entity.HRef).ToList()
																	   : new List<string>();

				var mergedGroups = userBeforeUpdate.Roles.Exists(s => s.Entity != null) ? userBeforeUpdate.Roles.Select(s => s.Entity)
																								.Concat(manageUser.Roles.Select(s => s.Entity)) 
																					: manageUser.Roles.Select(s => s.Entity);

                var removedGroups = oldGroups.Except(newGroups).ToList();
                var addedGroups = newGroups.Except(oldGroups).ToList();

                if (removedGroups.Any())
                {
                    foreach (string r in removedGroups)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Document Director Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", mergedGroups.FirstOrDefault(f => f.HRef == r).Name) });
                    }
                }
                if (addedGroups.Any())
                {
                    foreach (string r in addedGroups)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Document Director Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", mergedGroups.FirstOrDefault(f => f.HRef == r).Name) });
                    }
                }
            }

			return insUpdResult;
		}

		/// <summary>
		/// Used to update the user profile
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		public string UpdateRPDMUserProfile(long editorPersonaId, long userPersonaId)
		{
			Dictionary<string, object> logData = new Dictionary<string, object>();

			try
			{
				var response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
				if (response.IsError)
				{
					return response.ErrorReason;
				}

				Persona userPersona = _managePersona.GetPersona(userPersonaId);
				Guid realPageId = userPersona.RealPageId;
				var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
				Person person = _managePerson.GetPerson(realPageId);

				IList<CommonAddress> contactMechansimList = _manageContactMechanism.ListContactMechanismForPerson(realPageId, null);

				// get the email address
				var userEmailAddress = GetEmailAddress(contactMechansimList, userLogin);

				RPDMUser manageUser = NewRPDMUser(userEmailAddress, person);

				return UpdateRPDMUser(manageUser, userPersonaId, true);
			}
			catch (Exception ex)
			{

				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUserProfile", $"Update user profile. {ex.Message}" }, exception: ex);
				UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUserProfile", "Update user profile errored. Set product status to Error" });
				// write an error
				return "There was a problem updating the user profile. " + ex.Message;
			}

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
			try
			{
                CustomerCompanyMap companyMap = GetRPDocumentManagementCompanyInstanceId();
                var useUPFMInstance = _unifiedLoginSettings?.FirstOrDefault(a => a.Name.Equals("BooksUseUPFMId", StringComparison.OrdinalIgnoreCase))?.Value == "1";
                if (!useUPFMInstance)
                {
                    IList<InstanceAttribute> listInstanceAttribute = companyMap.CompanyInstance[0].Attributes;
                    if (listInstanceAttribute.ToList().Any(a => a.AttributeName.ToUpper() == "DOMAIN ID"))
                    {
                        InstanceAttribute instanceAttribute = listInstanceAttribute.ToList().Find(a => a.AttributeName.ToUpper() == "DOMAIN ID");
                        domain = instanceAttribute.AttributeValue;
                    }
                }
                else
                {
                    domain = companyMap.CompanyInstanceSourceId;
                }
            }
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetDomain", "There was a problem getting the DocManagement attribute in BlueBook" }, exception: ex);

				return CommonMessageConstants.CompanyErrorMessage;
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
        /// <param name="productUserId"></param>
        /// <returns></returns>
        public string UnassignUser(long editorPersonaId, long userPersonaId, int productUserId = 0)
		{
			if (productUserId != 0)
			{
				_productUserId = productUserId.ToString();

            }
			ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
			if (listResponse.IsError) { return listResponse.ErrorReason; }
                       
            
            string domain = GetDomain();

			var url = _productUrl.Replace("{{domain}}", domain) + $"/api/{domain}/users/{_productUserId}/disable";
			Dictionary<string, object> logData = new Dictionary<string, object>() {{"url", url}};
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "Disable user" }, logData: logData);

			var postResponse = _client.PostAsJsonAsync(url, "").Result;
			if (!postResponse.IsSuccessStatusCode)
			{
				if (productUserId != 0 && userPersonaId == 0)
				{
					logData = new Dictionary<string, object>();
					var erroMessage = postResponse.Content.ReadAsStringAsync().Result.ToString();
					logData.Add("error", erroMessage);
					logData.Add("status", postResponse.StatusCode);
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "Disable user failed." }, logData: logData);
					return $"There was a problem Delete Document Directory User the user with editorPersona id - {editorPersonaId} - Error-{erroMessage}.";

				}
				else
				{
                    // write an error
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "UnassignUser user errored. Set product status to Error" });
                    return "Error";
                }				
			}

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Successfully Disabled user userPersonaId:{userPersonaId}" });
            if (userPersonaId != 0 && productUserId == 0)
			{
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
            }
			return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <returns></returns>
		private ListResponse GetRoles(long userPersonaId)
		{
			ListResponse response = new ListResponse();

			try
			{
				RPDMResult<RPDMRole> rpdmResult = GetResultFromApi<RPDMResult<RPDMRole>>("/roles?isApi=true", "name");
				if (rpdmResult == null)
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Error. rpdmResult == null" });
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
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Error. list == null" });
					response.IsError = true;
					response.ErrorReason = "There was a problem getting the role details";
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error. {ex.Message} " }, exception: ex);
				response.IsError = true;
				response.ErrorReason = "There was a problem getting the role details";
			}

			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="organizationPartyId"></param>
		/// <returns></returns>
		private ListResponse GetPropertyRoles(long userPersonaId, long organizationPartyId)
		{
			ListResponse response = new ListResponse();
			IList<ProductRole> rpdmRolelist = new List<ProductRole>();
			IList<ProductRole> list;
			ListResponse propertyResponse = new ListResponse();
			try
			{
					response = GetRoles(userPersonaId);
					if (response.TotalRows > 0)
					{
						list = response.Records.Cast<ProductRole>().ToArray();
						ProductRole pRole = new ProductRole();
						ListResponse propertyListResponse = new ListResponse();
						foreach (ProductRole item in list)
						{
							pRole = item;
							if (!string.IsNullOrEmpty(item.Roletype))
							{
								if (item.Name.Contains("(" + item.Roletype + ")"))
								{
									pRole.Name = item.Name.Replace("(" + item.Roletype + ")", "").Trim();
								}
								if (item.Roletype.ToLower().Contains("site"))
								{
									pRole.Roletype = "Property";
								}

								propertyResponse = GetRoleClassifierDataset(userPersonaId, item.ID, organizationPartyId);
								if (propertyResponse.Records.Count > 0)
								{
									pRole.propertiesList = propertyResponse.Records as List<object>;
								}
							}
							rpdmRolelist.Add(pRole);
						}
					}

					if (rpdmRolelist != null)
					{
						rpdmRolelist = rpdmRolelist.OrderBy(p => p.Name).ToList();

						response = new ListResponse()
						{
							Records = rpdmRolelist.Cast<object>().ToList(),
							TotalRows = rpdmRolelist.Count,
							RowsPerPage = rpdmRolelist.Count,
							TotalPages = 1,
							ErrorReason = ""
						};
						
					}
					else
					{
						WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyRoles", "Error. list == null" });
						response.IsError = true;
						response.ErrorReason = "There was a problem getting the role details";
					}
					return response;
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyRoles", $"Error. {ex.Message}" }, exception: ex);
				response.IsError = true;
				response.ErrorReason = "There was a problem getting the role details";
				return response;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		private ListResponse GetRoleClassifierDataset(long userPersonaId, string roleId)
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
												if (pp.ID == ur.Entity?.Id?.ToUpper() && roleId == ur.Role?.Id?.ToUpper())
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
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoleClassifierDataset", $"Error. {ex.Message} " }, exception: ex);
				response.IsError = true;
				response.ErrorReason = "There was a problem getting the classifier";
			}

			return response;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userPersonaId"></param>
		/// <param name="roleId"></param>
		/// <param name="organizationPartyId"></param>
		/// <returns></returns>
		private ListResponse GetRoleClassifierDataset(long userPersonaId, string roleId, long organizationPartyId)
		{
			ListResponse response = new ListResponse();
			RPObjectCache rpCache = new RPObjectCache();
			string cacheKey = $"DocumentDirector_Roles_{organizationPartyId}_{roleId}";
			
			try
			{
				IList<ProductProperty> list = rpCache.GetFromCache(cacheKey, 300, () =>
				{
					RPDMRoleDetail rpdmRoleDetail = GetResultFromApi<RPDMRoleDetail>("/roles/" + roleId);
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
									return rpdmResult.Page.ToGBProperties();
								}
							}
						}
					}
					return null;
				});

				if (list !=null)
				{
					// flag any values that are assigned to the given user
					if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
					{
						RPDMUser user = GetUserDetails(_productUserId);
						if (user.Roles?.Count > 0)
						{
							List<RPDMUserRoles> userRoles = user.Roles;
							foreach (ProductProperty pp in list)
							{
								bool isAssigned = false;
								foreach (RPDMUserRoles ur in userRoles)
								{
									if (pp.ID == ur.Entity?.Id?.ToUpper() && roleId == ur.Role?.Id?.ToUpper())
									{
										isAssigned = true;
										break;
									}
								}
								pp.IsAssigned = isAssigned;
							}
						}
					}
					else
					{
						foreach (ProductProperty pp in list)
						{
							pp.IsAssigned = false;
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
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoleClassifierDataset", $"Error. {ex.Message}" }, exception: ex);
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
			return GetProductCompanyInstanceId(_udmSourceCode, "companyInstance.attributes");
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
		/// Validates that the resolved product URL/domain can produce a parseable URI.
		/// Guards against BlueBook placeholder values (e.g. "This product is not yet implemented...")
		/// being interpolated into _productUrl, which would result in an unparseable URI.
		/// </summary>
		/// <param name="productUrl">The resolved product URL template after domain substitution</param>
		/// <param name="domain">The resolved domain value</param>
		/// <returns>true if the URL is safe to call; false otherwise</returns>
		internal static bool IsProductUrlValid(string productUrl, string domain)
		{
			if (string.IsNullOrWhiteSpace(productUrl) || string.IsNullOrWhiteSpace(domain))
			{
				return false;
			}

			// Reject known BlueBook placeholder text
			if (domain.IndexOf("not yet implemented", StringComparison.OrdinalIgnoreCase) >= 0
				|| productUrl.IndexOf("not yet implemented", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return false;
			}

			// Domain must not contain spaces or characters that produce an unparseable URI
			if (domain.Contains(" ") || domain.Contains("\t") || domain.Contains("\r") || domain.Contains("\n"))
			{
				return false;
			}

			// Final sanity check via Uri.TryCreate
			if (!Uri.TryCreate(productUrl, UriKind.Absolute, out Uri parsedUri))
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(parsedUri.Host) || parsedUri.Host.Contains(" "))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Used to get data from RPDM
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="additionalQuery"></param>
		/// <param name="sortBy">Used to sort the result data</param>
		/// <returns></returns>

		private T GetResultFromApi<T>(string additionalQuery, string sortBy = null, bool migrationUser = false) where T : class
		{
			T results = null;
			string sortByAdditional = "";

			string domain = GetDomain();
			if (domain.Contains("There was a problem creating the user") || string.IsNullOrEmpty(domain))
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", $"Error - No CompanyInstanceSourceId found, additionalQuery {additionalQuery}" });
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
			string url;
			if (migrationUser)
			{
				 url = _productUrl.Replace("{{domain}}", domain) + additionalQuery;
			}
			else
			{
				 url = _productUrl.Replace("{{domain}}", domain) + $"/api/{domain}" + additionalQuery + sortByAdditional;
			}

			// Guard against invalid/placeholder URLs sourced from BlueBook product configuration.
			// If the resolved URL is not a parseable absolute URI, skip the HTTP call rather than
			// surface an "Invalid URI: The hostname could not be parsed." exception.
			if (!IsProductUrlValid(url, domain))
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", $"Error - Invalid product URL detected. Skipping HTTP call. url={url}, domain={domain}, additionalQuery={additionalQuery}" });
				return null;
			}

			Dictionary<string, object> logData = new Dictionary<string, object>() {{"url", url}};
			WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", "Posting to url." }, logData: logData);
			try
			{
				var response = _client.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					results = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result, typeof(T)) as T;
				}
				else
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", $"Error - Response is not 200. url {url}, StatusCode - {response.StatusCode}" });
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetResultFromApi", $"Error {ex.Message}, url {url}" }, exception: ex);
			}

			return results;
		}

		/// <summary>
		/// Get the user email address
		/// </summary>
		/// <param name="contactMechansimList"></param>
		/// <param name="userLogin"></param>
		/// <returns></returns>
		private string GetEmailAddress(IList<CommonAddress> contactMechansimList, UserLoginOnly userLogin)
		{
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

			return userEmailAddress;
		}

		/// <summary>
		/// New RPDMUser
		/// </summary>
		/// <param name="userEmailAddress"></param>
		/// <param name="person"></param>
		/// <returns></returns>
		private RPDMUser NewRPDMUser(string userEmailAddress , Person person)
		{
			return new RPDMUser()
			{
				Domain = GetDomain(),
				FirstName = person.FirstName,
				LastName = person.LastName,
				Name = _productUsername,
				Email = userEmailAddress,
				TimeZone = "US/Central",
				Locale = "en",
				Enabled = true,
			};
		}

		/// <summary>
		/// Update RPDMUser 
		/// </summary>
		/// <param name="manageUser"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="isUserProfile"></param>
		/// <returns></returns>
		private string UpdateRPDMUser(RPDMUser manageUser, long userPersonaId , bool isUserProfile = false)
		{
			Dictionary<string, object> logData = new Dictionary<string, object>();

			try
			{
				RPDMUser currentUser = GetUserDetails(_productUserId);

				manageUser.Id = _productUserId;
				manageUser.TimeZone = currentUser.TimeZone;
				manageUser.Locale = currentUser.Locale;
				manageUser.Photo = currentUser.Photo;
				manageUser.Groups = currentUser.Groups;

				if (isUserProfile)
				{
					manageUser.Roles = currentUser.Roles;
				}

				var url = "";

				if (!currentUser.Enabled && !isUserProfile)
				{
					// reactivate the user
					url = _productUrl.Replace("{{domain}}", manageUser.Domain) + $"/api/{manageUser.Domain}/users/{_productUserId}/enable";
					logData = new Dictionary<string, object>() { { "url", url } };
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUser", $"Update user {_productUserId}, enable disabled user" }, logData: logData);
					var postEnableResponse = _client.PostAsJsonAsync(url, manageUser).Result;
					if (postEnableResponse.IsSuccessStatusCode || postEnableResponse.StatusCode == System.Net.HttpStatusCode.NotModified)
					{
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUser", $"Update user {_productUserId}, enable disabled user success" }, logData: logData);
					}
					else
					{
						logData = new Dictionary<string, object>() { { "postEnableResponse.Content", postEnableResponse.Content.ReadAsStringAsync().Result } };
						WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUser", $"Update user {_productUserId} errored." }, logData: logData);
						// write an error
						return "There was a problem updating the user. " + postEnableResponse.Content.ReadAsStringAsync().Result;
					}
				}

				url = _productUrl.Replace("{{domain}}", manageUser.Domain) + $"/api/{manageUser.Domain}" + $"/users/{_productUserId}";
				logData = new Dictionary<string, object>() { { "url", url }, { "manageUser", JsonConvert.SerializeObject(manageUser) } };
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUser", "Update user" }, logData: logData);

				var postUpdateResponse = _client.PostAsJsonAsync(url, manageUser).Result;
				if (postUpdateResponse.IsSuccessStatusCode || postUpdateResponse.StatusCode == System.Net.HttpStatusCode.NotModified)
				{
					UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
					logData = new Dictionary<string, object>() { { "postResponse.Content", postUpdateResponse.Content.ReadAsStringAsync().Result } };
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUser", "Update user success. Set product status to Success" }, logData: logData);
					return string.Empty;

				}
				else
				{
					logData = new Dictionary<string, object>() { { "postResponse.Content", postUpdateResponse.Content.ReadAsStringAsync().Result } };
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUser", "Update user errored." }, logData: logData);
					// write an error
					return "There was a problem updating the user. " + postUpdateResponse.Content.ReadAsStringAsync().Result;
				}
			}
			catch (Exception ex)
			{
				//UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRPDMUser", $"Create user errored. {ex.Message} " });
				return "There was a problem updating the user";
			}

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

			try
			{

				string companyInstanceSourceId = GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId;
				if (companyInstanceSourceId == null)
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
					response.ErrorReason = "Company Setup Error: Please Contact Support.";
					return response;
				}

				var filter = "NonMigrated";
				var resultsperpage = 1000;
				int pageNumber =1;
				if (datafilter != null)
				{
					if (datafilter.FilterBy.ContainsKey("filter"))
					{
						filter = datafilter.FilterBy["filter"];
					}
					if (datafilter.Pages != null)
					{
						pageNumber = datafilter.Pages.StartRow;
						resultsperpage = datafilter.Pages.ResultsPerPage;
					}
				}

				var url = $"/api/unity/{companyInstanceSourceId}/users?filter={filter}&pageNumber={pageNumber}&resultsperpage={resultsperpage}";
				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Beginning Get migration users" }, logData: new Dictionary<string, object> { { "Url", url } });

				var rPDMigrationList = GetResultFromApi<IList<RPDMigrationUser>>(url,null,true);

				if (rPDMigrationList == null)
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
					return response;
				}
				var allUsers = rPDMigrationList.Select(x => new MigrationUser()
				{
					CompanyInstanceSourceId = x.companyId,
					Email = x.Email,
					Extra = x.Extra,
					FirstName = x.FirstName,
					LastActivity = x.LastActivity,
					LastName = x.LastName,
					MiddleName = x.MiddleName,
					Phone = x.Phone,
					Status = x.isActive ? "true" : "false",
					Title = x.Title,
					UserId = x.UserId,
					Username = x.Username,
					Properties = x.Properties,
					isRealPageEmployee = x.isRealPageEmployee,
					isAdminUser = x.isAdminUser,
					isReadOnly = x.isReadOnly,
					isMigratedUser = x.isMigratedUser
				}).ToList();

				WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}." });
				response.RowsPerPage = resultsperpage;
				response.ErrorReason = string.Empty;
				response.IsError = false;
				response.TotalPages = 1;
				response.Records = allUsers.Cast<object>().ToList();
				response.TotalRows = allUsers.Count();
			}
			catch (Exception ex)
			{
				response = new ListResponse
				{
					IsError = true,
					ErrorReason = ex.Message
				};

				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
			}
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

            string domain = GetDomain();
            if (domain.Contains("There was a problem creating the user") || string.IsNullOrEmpty(domain))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "Error - No CompanyInstanceSourceId found." });
                return null;
            }
            try
			{
				string companyInstanceSourceId = GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId;
				if (companyInstanceSourceId == null)
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
					migrateResponse.Message = "Company Setup Error: Please Contact Support.";
					return migrateResponse;
				}
                var logDatapayload = new Dictionary<string, object>
                {
                    { "MigratedUser", migrateUsers }
                };
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "PatchAsJsonAsync" }, logData: logDatapayload);
                var url = _productUrl.Replace("{{domain}}", domain) + $"/api/users/{companyInstanceSourceId}/migrate";
                var integration = new ApiIntegration(_client, url);
                var response = integration.PatchEntity<MigrateResponse>(migrateUsers);

                var logData = new Dictionary<string, object> { { "result", response } };
                if (response.IsSuccessStatusCode)
				{
                    var migrationResponse = JsonConvert.DeserializeObject<MigrateResponse>(JsonConvert.SerializeObject(response.Content));
					WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "PatchAsJsonAsync Success" }, logData: logData);
					migrateResponse.Message = migrationResponse.Message;
					migrateResponse.Status = migrationResponse.Status;
					return migrateResponse;
				}
				else
				{
					WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"PostAsJsonAsync Error" }, logData: logData);
					migrateResponse.Message = "Cannot update user status to migrated.";
					return migrateResponse;
				}
			}
			catch (Exception ex)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);

				return new MigrateResponse
				{
					Status = false,
					Message = ex.Message
				};
			}
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
