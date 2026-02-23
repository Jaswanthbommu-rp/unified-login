using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.AdminSupportPortal;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    public class ManageProductAdminSupportPortal : ManageProductBase, IManageProductAdminSupportPortal
    {
        #region Private members

        // product settings
        private static string _apiCode;
        private static string _apiSecret;
        private static string _tokenUrl;
        private static string _apiRoute;
        private static string _securityToken;
        private static string _apiPassword;
        private static string _apiUserName;
        private static string _portalId;
        private static string _organizationId;
        private static string _clientportalUltraLightRoleId;
        // token & url params after authentication with SForce
        private static string _authToken;
        private static string _instanceUrl;
        private DefaultUserClaim _userClaims;
        public bool isMultiCompanyUser = false;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userClaims">Real page Id of user who is creating new user</param>
        public ManageProductAdminSupportPortal(DefaultUserClaim userClaims) : base((int)ProductEnum.AdminSupportPortal, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductAdminSupportPortal", "Ctor - Getting Product settings." });
#endif
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _productId = (int)ProductEnum.AdminSupportPortal;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _userClaims = userClaims;

            _blueBook = new ManageBlueBook(userClaims);

            // get product settings
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value;
            _apiCode = _productInternalSettingList.First(a => a.Name.ToUpper() == "APICODE").Value;
            _tokenUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENURL").Value;
            _apiRoute = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIROUTE").Value;
            _securityToken = _productInternalSettingList.First(a => a.Name.ToUpper() == "SECURITYTOKEN").Value;
            _apiPassword = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value;
            _apiUserName = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value;
            _portalId = _productInternalSettingList.First(a => a.Name.ToUpper() == "PORTALID").Value;
            _organizationId = _productInternalSettingList.First(a => a.Name.ToUpper() == "ORGANIZATIONID").Value;
            _clientportalUltraLightRoleId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTPORTALULTRALIGHTROLEID").Value;
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductAdminSupportPortal", "Ctor - Received Product settings; getting token values." });
#endif
            GetSaleforceTokenInstanceUrl();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Used to get properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                int companyInstanceId = GetProductCompanyInstanceId(_udmSourceCode, useTranslate: false).CompanyInstanceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetProductCompanyInstanceId - Found blue book company instance id - {companyInstanceId}  for user editorPersona id -{editorPersonaId}" });


                CompanyPropertyRootObject companyProperties = _blueBook.GetCompanyPropertyInstance(companyInstanceId);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetPropertyInstance - Found total {companyProperties.data.attributes.getCompanyPropertyInstances.Count} properties with blue book company instance id {companyInstanceId} for user with editorPersona id - {editorPersonaId}." });

                IList<ProductProperty> blueBookPropertyList = companyProperties.MapBlueBookToGBProperties() ?? new List<ProductProperty>();
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MapBlueBookToGBProperties completed for user with editorPersona id -{editorPersonaId}." });

                // need to do a filter on the result
                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0)) //update existing user
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Calling MergeProductPropertiesWithGreenbook for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    result = MergeProductPropertiesWithGreenbook(blueBookPropertyList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}." });
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}." }, exception: ex);

                result = new ListResponse()
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    result.ErrorReason = ex.Message;
                }
                else
                {
                    result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns Roles  
        /// </summary>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });
            var response = new ListResponse();
            try
            {
                var result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                var adminSupportPortalAllRoles = GetProductRoles();

                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0))  // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeProductRolesWithGreenBook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeProductRolesWithGreenBook(adminSupportPortalAllRoles);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeProductRolesWithGreenBook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                }
                else // Called during creating a new User
                {
                    ProductRole accessGroup = (from a in adminSupportPortalAllRoles
                                               where a.ID == ""
                                               select a).FirstOrDefault();
                    if (accessGroup != null)
                    {
                        accessGroup.IsAssigned = true;
                    }

                    adminSupportPortalAllRoles.FirstOrDefault(s => s.Name.ToLower().Trim() == "client portal light").IsAssigned = true;

                    response = new ListResponse()
                    {
                        Records = adminSupportPortalAllRoles.Cast<object>().ToList(),
                        TotalRows = adminSupportPortalAllRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Exiting method with total rows - {adminSupportPortalAllRoles.Count} for user with editorPersona id - {editorPersonaId}." });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ManageAdminSupportPortalUser(long editorPersonaId, long userPersonaId, AdminSupportPortalPropertyRole adminSupportPortalPropertyRole, out List<AdditionalParameters> additionalParameters)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Begin create/update user for user with editorPersona id - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
            additionalParameters = new List<AdditionalParameters>();
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                AdminSupportPortalContactResult adminSupportPortalContactResult = new AdminSupportPortalContactResult() ;
                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);
                persona.Organization = organizationList.FirstOrDefault(i => i.PartyId == persona.OrganizationPartyId);

                var personaOrganization = persona.Organization;
                bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

                ListResponse userPropertiesBeforeUpdate = new ListResponse();
                AdminSupportPortalUser userRolesBeforeUpdate = new AdminSupportPortalUser();
                if (!string.IsNullOrEmpty(_productUserId))
                {
                    userPropertiesBeforeUpdate = GetProperties(editorPersonaId, userPersonaId, new RequestParameter());
                    userRolesBeforeUpdate = GetAdminSupportPortalUser();
                }

                string productLoginName;
                bool isUserUpdate = false;
                if (string.IsNullOrEmpty(_productUsername))
                {
                    if (!IsUserWithEmail(userPersonaId) || !RegexUtilities.IsValidEmail(userLogin.LoginName))
                    {
                        // throw exception
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"No valid email address for user with editorPersona id - {editorPersonaId}." });
                        return $"Error-ManageProductAdminSupportPortal.ManageAdminSupportPortalUser - no valid email address for user with editorPersona id - {editorPersonaId}.";
                    }

                    productLoginName = userLogin.LoginName;
                }
                else
                {
                    productLoginName = _productUsername;
                    isUserUpdate = true;
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"_productUsername for user is {_productUsername} and editorPersonaId  - {userPersonaId}" });

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Error for user with editorPersona id - {editorPersonaId} Error - Company not found." });
                    return $"ManageProductAdminSupportPortal.ManageAdminSupportPortalUser - Company not found for user with editorPersona id - {editorPersonaId}.";
                }


                // super user
                if (IsSuperUser(userPersonaId) && (!isUserUpdate || (adminSupportPortalPropertyRole.RoleList != null && adminSupportPortalPropertyRole.RoleList.Count == 0)))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"New user is Super user with editorPersona id - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                    adminSupportPortalPropertyRole = new AdminSupportPortalPropertyRole
                    {
                        PropertyList = new List<string> { "-1" },
                        RoleList = new List<string> { "00e00000006qqxf" }
                    };
                }

                // check Contact by Property or PMC to get OMS Id
                string searchOmsId = string.Empty;
                string parentOmsId = company.CompanyInstanceSourceId;
                if (adminSupportPortalPropertyRole.PropertyList != null && adminSupportPortalPropertyRole.PropertyList.Count > 0 && adminSupportPortalPropertyRole.PropertyList[0] != null &&
                    adminSupportPortalPropertyRole.PropertyList[0].Length > 3)
                {
                    searchOmsId = adminSupportPortalPropertyRole.PropertyList[0];
                }
                else
                {
                    searchOmsId = company.CompanyInstanceSourceId;
                }

                // For multiple contacts result
                var clientPortalContactAcrossCompanies = CheckClientPortalContactsExists(userLogin.LoginName);
                List<AdminSupportPortalContactResult> adminSupportPortalContactResults = new List<AdminSupportPortalContactResult>();
                // 'Contact.Account.Parent.OMS_ID__c'
                if (clientPortalContactAcrossCompanies != null && clientPortalContactAcrossCompanies.Count > 0)
                {
                    foreach (var item in clientPortalContactAcrossCompanies)
                    {
                        if (item.ParentOMS_ID__c == parentOmsId)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"parentId equalsto parentOMSId - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });

                            adminSupportPortalContactResults.Add(item);
                        }
                        else if (item.OMS_ID__c != null && item.OMS_ID__c.StartsWith("C") && string.IsNullOrEmpty(item.ParentOMS_ID__c) && item.OMS_ID__c == parentOmsId)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"parentId equalsto ParentOMS_ID__c is empty - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });

                            adminSupportPortalContactResults.Add(item);
                        }
                        else if (item.OMS_ID__c != null && item.OMS_ID__c.StartsWith("C") && !string.IsNullOrEmpty(item.ParentOMS_ID__c) && item.OMS_ID__c == parentOmsId && item.ParentOMS_ID__c.StartsWith("C"))
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"ParentOMS_ID__c is not empty - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });

                            adminSupportPortalContactResults.Add(item);
                        }
                        else if ((item.OMS_ID__c != null && item.OMS_ID__c.StartsWith("C") && !string.IsNullOrEmpty(item.ParentOMS_ID__c) && parentOmsId.Equals(item.ParentOMS_ID__c) && item.ParentOMS_ID__c.StartsWith("C")))
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Company is having a Parent company - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });

                            adminSupportPortalContactResults.Add(item);
                        }
                    }
                }

                var contactId = string.Empty;
                string accountId = string.Empty;
                string result = string.Empty;
                isMultiCompanyUser = (isExternalUser || string.IsNullOrEmpty(_productUsername) || adminSupportPortalContactResults.Count > 0) && !isUserUpdate;
                var uniqueProductLoginName = !isUserUpdate ? IterateUserNameIfExists(productLoginName) : productLoginName;

                // If no contact then create new contact in salesforce
                if (adminSupportPortalContactResults == null || adminSupportPortalContactResults.Count == 0)
                {
                    // Find Account Id in salesforce for oms Id
                    accountId = GetClientPortalContactAccountId(searchOmsId);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"account id {accountId} received for user with _productUsername {_productUsername}." });

                    // create a new contact in salesforce
                    contactId =
                      CreateAdminSupportPortalContact(new AdminSupportPortalContact
                      {
                          AccountId = accountId,
                          Email = productLoginName,
                          FirstName = person.FirstName,
                          LastName = person.LastName,
                          Unified_Platform_User__c = true,
                          Portal_User_Migrated__c = true
                      });

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"New contact created with contact id {contactId} and account id {accountId} received for user with _productUsername {_productUsername}." });
                }
                else
                {
                    foreach (var contact in adminSupportPortalContactResults)
                    {
                        //contact exists; check update on property if yes then update contact
                        contactId = contact.Id;

                        // PMC to PMC OMS change is not allowed for same user login
                        if (!string.IsNullOrEmpty(contact.OMS_ID__c) && !string.IsNullOrEmpty(searchOmsId) && contact.OMS_ID__c != searchOmsId)
                        {
                            if (searchOmsId[0] == 'C' && contact.OMS_ID__c[0] == 'C')
                            {
                                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Error for user with editorPersona id - {editorPersonaId} - PMC to PMC OMS change is not allowed." });
                                return "Error - PMC to PMC OMS change is not allowed.";
                            }
                        }

                        if (!string.IsNullOrEmpty(contact.OMS_ID__c) && !string.IsNullOrEmpty(searchOmsId) && contact.OMS_ID__c != searchOmsId)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Update contact - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });

                            // update salesforce contact with new OMS ID
                            UpdateContact(contactId, searchOmsId, false, true);
                        }
                        else if (!string.IsNullOrEmpty(contact.OMS_ID__c) && !string.IsNullOrEmpty(contactId) && contact.OMS_ID__c == searchOmsId)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Update PortalUser MigratedFlag - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });

                            UpdatePortalUserMigratedFlag(contactId);
                        }
                    }
                }

                var clientPortaluserDetails = CheckClientPortalUserExists(userLogin.LoginName);
                var productRoles = GetProductRoles();
                var roleType = productRoles.Where(a => a.ID == adminSupportPortalPropertyRole.RoleList[0]).Select(a => a.Roletype).FirstOrDefault();
                
                var adminSupportPortalUser = new AdminSupportPortalUser
                {
                    Email = userLogin.LoginName,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    ContactId = contactId,
                    EmailEncodingKey = "UTF-8",
                    LanguageLocaleKey = "en_US",
                    TimeZoneSidKey = "America/Chicago",
                    Username = uniqueProductLoginName,
                    LocaleSidKey = "en_US",
                    CommunityNickname = uniqueProductLoginName.Substring(0, uniqueProductLoginName.Length >= 40 ? 40 : uniqueProductLoginName.Length),
                    Alias = GetAliasFromLogin(uniqueProductLoginName),
                    ProfileId = adminSupportPortalPropertyRole.RoleList[0],
                    IsActive = true,
                    IsCreatedFromNewPortal__c = true
                };

                if (isUserUpdate)
                {
                    adminSupportPortalContactResult = clientPortaluserDetails.Find(m => m.Id == _productUserId);
                }

                if (clientPortaluserDetails != null && clientPortaluserDetails.Count > 0 && clientPortaluserDetails.Exists(m => m.Id == _productUserId) && adminSupportPortalContactResult != null && adminSupportPortalContactResult.IsPortalEnabled)
                {
                    // Update User & return result
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Trying to UPDATE user with editorPersona id - {editorPersonaId}." });

                    // set fields to null which we can't update
                    adminSupportPortalUser.ContactId = null;
                    result = UpdateAdminSupportPortalUser(adminSupportPortalUser, _productUserId, userPersonaId, roleType);
                }
                else
                {
                    // Create New User & return result
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Trying to CREATE user with editorPersona id - {editorPersonaId}." });
                    result = CreateAdminSupportPortalUser(userPersonaId, adminSupportPortalUser, roleType, adminSupportPortalContactResult.IsPortalEnabled);
                }

                //Activity Log Roles
                string profileId = string.Empty;
                if (!string.IsNullOrEmpty(userRolesBeforeUpdate.ProfileId) && userRolesBeforeUpdate.ProfileId.Length > 15)
                    profileId = userRolesBeforeUpdate?.ProfileId.Substring(0, 15);

                if (profileId != adminSupportPortalUser.ProfileId)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Admin & Support Portal Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", productRoles.FirstOrDefault(f => f.ID == adminSupportPortalUser.ProfileId).Name) });
                    if (!string.IsNullOrEmpty(profileId) && productRoles.Any(f => f.ID == profileId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Admin & Support Portal Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", productRoles.FirstOrDefault(f => f.ID == profileId).Name) });
                    }
                }

                //Activity Log Properties
                var oldProps = userPropertiesBeforeUpdate.Records != null ? userPropertiesBeforeUpdate.Records.Cast<ProductProperty>().ToList() : new List<ProductProperty>();
                var assignedOldProp = oldProps.Find(f => f.IsAssigned == true);
                var newPropsList = GetProperties(editorPersonaId, userPersonaId, new RequestParameter());
                var newProps = newPropsList.Records.Cast<ProductProperty>().ToList();
                var assignedNewProp = newProps.Find(f => f.IsAssigned == true);

                if (assignedOldProp?.ID != assignedNewProp?.ID)
                {
                    if (!string.IsNullOrEmpty(assignedOldProp?.Name))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Admin & Support Portal Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", assignedOldProp.Name) });
                    }

                    if (!string.IsNullOrEmpty(assignedNewProp?.Name))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Admin & Support Portal Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", assignedNewProp.Name) });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageAdminSupportPortalUser", $"Error for user with editorPersona id - {editorPersonaId}" },
                    exception: ex);
                return $"Error - {ex.Message}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productLoginName"></param>
        private string IterateUserNameIfExists(string productLoginName)
        {
            bool foundUserName = false;
            int incrementor = 0;
            string clientPortalLoginName = productLoginName;

            while (!foundUserName)
            {
                if (CheckClientPortalUserExists(clientPortalLoginName).Count > 0)
                {
                    incrementor++;
                    clientPortalLoginName = productLoginName.Split('@')[0] + incrementor.ToString() + "@" + productLoginName.Split('@')[1];
                }
                else
                {
                    foundUserName = true;
                    productLoginName = clientPortalLoginName;
                }
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IterateUserNameIfExists", $"Generated iterated clinetPortalLoginName = {clientPortalLoginName}" });
            return productLoginName;
        }

        /// <summary>
        /// Updates user profile  
        /// </summary>
        public string UpdateAdminSupportPortalUserProfile(long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAdminSupportPortalUserProfile", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            var persona = _managePersona.GetPersona(userPersonaId);
            var realPageId = persona.RealPageId;
            var person = _managePerson.GetPerson(realPageId);
            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            string productLoginName;
            if (string.IsNullOrEmpty(_productUsername))
            {
                if (!IsUserWithEmail(userPersonaId) || !RegexUtilities.IsValidEmail(userLogin.LoginName))
                {
                    // throw exception
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAdminSupportPortalUserProfile", $"No valid email address for user with editorPersona id - {editorPersonaId}." });
                    return $"Error-ManageProductAdminSupportPortal.UpdateAdminSupportPortalUserProfile - no valid email address for user with editorPersona id - {editorPersonaId}.";
                }

                productLoginName = userLogin.LoginName;
            }
            else
            {
                productLoginName = _productUsername;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAdminSupportPortalUserProfile", $"_productUsername for user is {_productUsername}." });

            List<AdminSupportPortalContactResult> clientPortalList = CheckClientPortalUserExists(productLoginName);
            List<AdminSupportPortalContactResult> salesForceContactResults = null;
            AdminSupportPortalUser adminSupportPortalUser = GetAdminSupportPortalUser();
            if (adminSupportPortalUser == null)
            {
                return "Error getting user info from AdminSupportPortal";
            }
            adminSupportPortalUser.Email = userLogin.LoginName;
            adminSupportPortalUser.FirstName = person.FirstName;
            adminSupportPortalUser.LastName = person.LastName;
            adminSupportPortalUser.Username = productLoginName;
            adminSupportPortalUser.IsActive = true;
            adminSupportPortalUser.ContactId = null;

            var result = PostApi($"{_apiRoute}sobjects/User/{_productUserId}?_HttpMethod=PATCH", adminSupportPortalUser);

            if (string.IsNullOrEmpty(result))
            {
                // For multiple contacts result
                if (clientPortalList != null && clientPortalList.Count > 0)
                {
                    salesForceContactResults = CheckClientPortalContactsExists(clientPortalList[0].Email);
                }
                var contactId = string.Empty;

                // If contact EXIST then update contact in salesforce with => Unified_Platform_User__c
                if (salesForceContactResults != null && salesForceContactResults.Count > 0)
                {
                    foreach (var contact in salesForceContactResults)
                    {
                        //contact exists; check update on property if yes then update contact
                        contactId = contact.Id;
                        UpdateContactProfile(contactId, person.FirstName, person.LastName, userLogin.LoginName);
                    }
                }


                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAdminSupportPortalUserProfile", $"Update in GB -productUsername -{productLoginName} and userId {_productUserId}." });
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);

                foreach (var attribute in productAttributes)
                {
                    if (attribute.Name.ToUpper() == "PRODUCTUSERNAME")
                    {
                        attribute.Value = productLoginName;
                        _samlRepository.UpdateSamlUserAttribute(attribute);
                    }
                }

                // add activity log
                WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
                return "";
            }

            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAdminSupportPortalUserProfile", $"Error for user with userPersonaId:{userPersonaId} and productUserId: {_productUserId}. ErrorReason-{result}" });

            return result;
        }

        public string ManageSalesForceUser(long editorPersonaId, long userPersonaId, AdminSupportPortalPropertyRole adminSupportPortalPropertyRole, bool isUnassigned = false)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageSalesForceUser", $"Begin create/update user for user with editorPersona id - {editorPersonaId}." });

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageSalesForceUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                string productLoginName;
                if (string.IsNullOrEmpty(_productUsername))
                {
                    if (!IsUserWithEmail(userPersonaId) || !RegexUtilities.IsValidEmail(userLogin.LoginName))
                    {
                        // nothing to do, the user doesn't have an email to verify
                        return "";
                    }
                    productLoginName = userLogin.LoginName;
                }
                else
                {
                    productLoginName = _productUsername;
                }

                // For multiple contacts result
                var salesForceContactResults = CheckClientPortalContactsExists(productLoginName);

                var contactId = string.Empty;

                // If contact EXIST then update contact in salesforce with => Unified_Platform_User__c
                if (salesForceContactResults != null && salesForceContactResults.Count > 0)
                {
                    foreach (var contact in salesForceContactResults)
                    {
                        //contact exists; check update on property if yes then update contact
                        contactId = contact.Id;
                        UpdateContactSalesForce(contactId, contact.OMS_ID__c, isUnassigned);
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageSalesForceUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                return $"Error - {ex.Message}";
            }
        }

        private void UpdateContact(string contactId, string searchOmsId, bool formerInactive, bool unifiedLoginUser)
        {
            // Find Account Id for new OMS Id
            var accountId = GetClientPortalContactAccountId(searchOmsId);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContact", $"account id {accountId} received for user with _productUsername {_productUsername}. OMS ID- {searchOmsId}" });

            dynamic accountObj = new ExpandoObject();
            accountObj.AccountId = accountId;
            accountObj.Unified_Platform_User__c = unifiedLoginUser;
            accountObj.Former_Inactive__c = formerInactive;
            accountObj.Portal_User_Migrated__c = true;
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", accountObj);
            if (!string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContact", $"Error for user with contactId - {contactId}" }, logData: result);
                throw new Exception($"Error while updating user - {result}");
            }
        }

        private void UpdatePortalUserMigratedFlag(string contactId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePortalUserMigratedFlag", $"ContactId id {contactId} received for user with _productUsername {_productUsername}. - Portal_User_Migrated__c setting to true." });

            dynamic accountObj = new ExpandoObject();
            accountObj.Portal_User_Migrated__c = true;
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", accountObj);
            if (!string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePortalUserMigratedFlag", $"Error for user with contactId - {contactId}" }, logData: result);
                throw new Exception($"Error while UpdatePortalUserMigratedFlag updating user - {result}");
            }
        }

        private void UpdateContactProfile(string contactId, string firstName, string LastName, string email)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContactProfile", $"ContactId id {contactId} received for user with _productUsername {_productUsername}. - Portal_User_Migrated__c setting to true." });

            dynamic accountObj = new ExpandoObject();
            accountObj.Email = email;
            accountObj.FirstName = firstName;
            accountObj.LastName = LastName;
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", accountObj);
            if (!string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContactProfile", $"Error for user with contactId - {contactId}" }, logData: result);
                throw new Exception($"Error while updating UpdateContactInfo for user user - {result}");
            }
        }

        private void UpdateContactSalesForce(string contactId, string searchOmsId, bool isUnassigned)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContactSalesForce", $"Account id {contactId} received for user with _productUsername {_productUsername}. OMS ID- {searchOmsId}" });

            dynamic contactObj = new ExpandoObject();
            contactObj.Unified_Platform_User__c = (isUnassigned == false);
            contactObj.Former_Inactive__c = isUnassigned;
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", contactObj);
            if (!string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContactSalesForce", $"Error for user with contactId - {contactId}" }, logData: result);
                throw new Exception($"Error while updating user - {result}");
            }
        }

        /// <summary>
        /// Unassign User
        /// </summary>
        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            //Call API to disable user
            var result = EnableDisableUser(editorPersonaId, userPersonaId, false);

            if (string.IsNullOrEmpty(result))
            {
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
                return "";
            }

            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error for user with userPersonaId:{userPersonaId} and productUserId: {_productUserId}. ErrorReason-{result}" });
            return result;
        }

        /// <summary>
        /// Unassign Salesforce User
        /// </summary>
        public string UnassignSalesForceUser(long editorPersonaId, long userPersonaId, AdminSupportPortalPropertyRole adminSupportPortalPropertyRole)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            var persona = _managePersona.GetPersona(userPersonaId);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"UserPersonaId:{userPersonaId} - start Deactivated user successfully in Client Portal." });
            var result = ManageSalesForceUser(editorPersonaId, userPersonaId, adminSupportPortalPropertyRole, true);
            if (!string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"Error for user with userPersonaId:{userPersonaId} and productUserId: {_productUserId}. ErrorReason-{result}" });

                return result;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"UserPersonaId:{userPersonaId} - end Deactivated contacts successfully in Client Portal." });

            return "";
        }

        /// <summary>
        /// Disable User
        /// </summary>
        public string EnableDisableUser(long editorPersonaId, long userPersonaId, bool isActive = false)
        {
            AdminSupportPortalUser adminSupportPortalUser = GetAdminSupportPortalUser();
            if (adminSupportPortalUser == null)
            {
                return "Error getting user info from AdminSupportPortal";
            }
            adminSupportPortalUser.IsActive = isActive;
            adminSupportPortalUser.ContactId = null;

            return PostApi($"{_apiRoute}sobjects/User/{_productUserId}?_HttpMethod=PATCH", adminSupportPortalUser);
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
            var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError) { response.ErrorReason = claimResponse.ErrorReason; return response; }

            string companyInstanceSourceId = GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId;
            if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                return new ListResponse { IsError = true, ErrorReason = "Company Setup Error: Please Contact Support." };
            }

            var filter = false;
            var startRow = 0;
            var resultPerRow = 1000;

            if (datafilter != null)
            {
                if (datafilter.FilterBy.ContainsKey("filter"))
                {
                    filter = datafilter.FilterBy["filter"].ToLower() == "migrated" ? true : false;
                }
                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage;
                }
            }

            string query = ($"SELECT Id,FirstName,LastName,Email,Username,LastLoginDate,IsActive,ProfileId FROM User" +
                                      $" WHERE (User.Contact.Account.OMS_ID__c = '{companyInstanceSourceId}' OR User.Contact.Account.Parent.OMS_ID__c = '{companyInstanceSourceId}')" +
                                      $" AND User.Contact.Portal_User_Migrated__c = {filter}" +
                                      $" LIMIT {resultPerRow} OFFSET {startRow}").Replace(' ', '+');

            var partialurl = $"{_apiRoute}query?q={query}";

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Beginning GetMigrationUsers" }, logData: new Dictionary<string, object> { { "Url", _instanceUrl + partialurl } });

            var migrationResponse = GetResultFromApi<ClientPortalMigrationResponse>(partialurl);
            if (migrationResponse == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                return response;
            }

            var productRoles = GetProductRoles();
            var migrationUsers = migrationResponse.Records.Select(user =>
            {
                string profileId = user.ProfileId.Remove(user.ProfileId.Length - 3);
                string roleType = productRoles.FirstOrDefault(c => c.ID == profileId)?.Roletype;
                return new MigrationUser
                {
                    CompanyInstanceSourceId = companyInstanceSourceId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    LastActivity = user.LastLoginDate.ToString(),
                    Extra = $"{_portalId}|{_organizationId}|{roleType}",
                    Status = user.IsActive ? "Active" : "Disabled"
                };
            }).ToList();

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}." });
            response.RowsPerPage = resultPerRow;
            response.ErrorReason = string.Empty;
            response.IsError = false;
            response.TotalPages = 1;
            response.Records = migrationUsers.Cast<object>().ToList();
            response.TotalRows = migrationResponse.TotalSize;

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

            var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError) { migrateResponse.Message = claimResponse.ErrorReason; return migrateResponse; }

            string companyInstanceSourceId = GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId;
            if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            dynamic contact = new ExpandoObject();
            var isError = false;
            foreach (var migrateUser in migrateUsers)
            {
                //Update Contact flags
                contact.Unified_Platform_User__c = migrateUser.UsingUnifiedLogin;
                contact.Portal_User_Migrated__c = true;
                var result = PostApi($"{_apiRoute}sobjects/User/{migrateUser.UserId}/Contact?_HttpMethod=PATCH", contact);
                if (!string.IsNullOrEmpty(result))
                {
                    migrateResponse.Message += result;
                    isError = true;
                }

                //Update User flags
                AdminSupportPortalUser adminSupportPortalUser = GetAdminSupportPortalUser(migrateUser.UserId);
                adminSupportPortalUser.IsCreatedFromNewPortal__c = true;
                adminSupportPortalUser.ContactId = null;
                var userResult = PostApi($"{_apiRoute}sobjects/User/{migrateUser.UserId}?_HttpMethod=PATCH", adminSupportPortalUser);
                if (!string.IsNullOrEmpty(userResult))
                {
                    migrateResponse.Message += userResult;
                    isError = true;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Updating unified login status. editorPersonaId-{editorPersonaId}" });
            }

            migrateResponse.Status = !isError;
            if (!isError)
                migrateResponse.Message = "success";

            return migrateResponse;
        }

        #endregion

        #region User-Status
        /// <summary>
        /// Disable Client Portal product user
        /// </summary>
        /// <param name="editorPersonaId"> editor persona identifier</param>
        /// <param name="productUserId">product user identifier</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, string productUserId, bool isActive = false)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error for user with productUserId:{productUserId} and editorPersonaId: {editorPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return false;
            }

            _productUserId = productUserId;
            //Call API to disable user
            var result = EnableDisableUser(editorPersonaId, 0, isActive);

            if (string.IsNullOrEmpty(result))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"productUserId:{productUserId} and editorPersonaId: {editorPersonaId}" });
                return true;
            }

            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error for user with productUserId:{productUserId} and editorPersonaId: {editorPersonaId}. ErrorReason-{result}" });

            return false;
        }
        #endregion

        #region Private Methods

        private AdminSupportPortalUser GetAdminSupportPortalUser(string userId = "")
        {
            AdminSupportPortalUser adminSupportPortalUser = GetResultFromApi<AdminSupportPortalUser>($"{_apiRoute}sobjects/user/{(string.IsNullOrEmpty(userId) ? _productUserId : userId)}");
            if (adminSupportPortalUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetAdminSupportPortalUser", $"Error for user {_productUserId} - User not found." });
            }
            return adminSupportPortalUser;
        }

        private ListResponse MergeProductRolesWithGreenBook(IList<ProductRole> allProductRoles)
        {
            AdminSupportPortalUser adminSupportPortalUser =
                GetResultFromApi<AdminSupportPortalUser>($"{_apiRoute}sobjects/user/{_productUserId}");

            if (adminSupportPortalUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductRolesWithGreenBook", $"Error for user {_productUserId} - User not found." });
                return new ListResponse() { IsError = true, ErrorReason = $"User {_productUserId} not found." };
            }

            // if a user record exists
            var profileId = adminSupportPortalUser.ProfileId;

            // Salesforce has both 15 and 18 digit versions of ID’s; so get first 15
            if (profileId.Length > 15)
                profileId = profileId.Substring(0, 15);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductRolesWithGreenBook", $"_productUserId-{_productUserId} - received profileId {profileId}" });

            if (allProductRoles.Any(a => a.ID.ToUpper() == profileId.ToUpper()))
            {
                ProductRole accessGroup = (from a in allProductRoles
                                           where a.ID == profileId
                                           select a).FirstOrDefault();
                if (accessGroup != null)
                {
                    accessGroup.IsAssigned = true;
                }
            }

            return new ListResponse()
            {
                Records = allProductRoles.Cast<object>().ToList(),
                TotalRows = allProductRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private void GetSaleforceTokenInstanceUrl()
        {
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetSaleforceTokenInstanceUrl", "Begining of the method." });

                ObjectCache tokenCache = MemoryCache.Default;

                // Get token values from cache
                _authToken = tokenCache["access_token_CP"] as string;
                _instanceUrl = tokenCache["instance_url_CP"] as string;

                // If no values from cache then get new one
                if (string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_instanceUrl))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetSaleforceTokenInstanceUrl", "Null cache values. Getting new one" });
                    string jsonResponse;
                    using (var client = new HttpClient())
                    {
                        var request = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                {"grant_type", "password"},
                                {"client_id", _apiCode},
                                {"client_secret", _apiSecret},
                                {"username", _apiUserName},
                                {"password", _apiPassword + _securityToken},
                            }
                        );
                        request.Headers.Add("X-PrettyPrint", "1");

                        var response = client.PostAsync(_tokenUrl, request).Result;
                        jsonResponse = response.Content.ReadAsStringAsync().Result;
                    }
                    var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                    _authToken = values["access_token"];
                    _instanceUrl = values["instance_url"];

                    if (string.IsNullOrEmpty(_authToken) || string.IsNullOrEmpty(_instanceUrl))
                    {
                        throw new Exception("ManageProductAdminSupportPortal.GetSaleforceTokenInstanceUrl - Received null or empty values from Salesforce.");
                    }

                    var cachePolicy = new CacheItemPolicy
                    {
                        // Expier cache every after 9 minutes (assuming 10 min is token expiration time)
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
                    };

                    tokenCache.Set("access_token_CP", _authToken, cachePolicy);
                    tokenCache.Set("instance_url_CP", _instanceUrl, cachePolicy);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetSaleforceTokenInstanceUrl", "Received & populated cache with token values." });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetSaleforceTokenInstanceUrl", $"Error in - {ex.Message}" }, exception: ex);
            }
        }

        private ListResponse MergeProductPropertiesWithGreenbook(IList<ProductProperty> blueBookPropertyList)
        {
            // merge the given user details with the list
            var adminSupportPortalAccount = GetAdminSupportPortalAccount();
            if (adminSupportPortalAccount == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductPropertiesWithGreenbook", $"Error - User not found in client portal with ProductUserId - {_productUserId}." });
                return new ListResponse()
                {
                    IsError = true,
                    ErrorReason = $"User not found in client portal with ProductUserId - {_productUserId}"
                };
            }

            var omsId = adminSupportPortalAccount.OMS_ID__c;
            var omsType = adminSupportPortalAccount.Type;

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductPropertiesWithGreenbook", $"Received - omsId{omsId},omsType{omsType}." });

            var propertyOption = new Dictionary<string, bool>();
            // Merge only for property & not PMC
            if (omsType.ToUpper() == "PROPERTY")
            {
                if (blueBookPropertyList.Any(a => a.ID == omsId))
                {
                    ProductProperty pp = (from a in blueBookPropertyList
                                          where a.ID == omsId
                                          select a).FirstOrDefault();
                    if (pp != null)
                    {
                        pp.IsAssigned = true;
                    }
                }
                propertyOption.Add("allProperties", false);// Single Property
            }
            else
            {
                propertyOption.Add("allProperties", true);// PMC level (all properties)
            }

            return new ListResponse()
            {
                Records = blueBookPropertyList.Cast<object>().ToList(),
                TotalRows = blueBookPropertyList.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = propertyOption
            };
        }

        private AdminSupportPortalAccount GetAdminSupportPortalAccount()
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAdminSupportPortalAccount", $"Calling API for _productUserId - {_productUserId}." });
            return GetResultFromApi<AdminSupportPortalAccount>($"{_apiRoute}sobjects/user/{_productUserId}/account");
        }

        private List<AdminSupportPortalContactResult> CheckClientPortalContactsExists(string loginName)
        {
            List<AdminSupportPortalContactResult> adminSupportPortalContacts = new List<AdminSupportPortalContactResult>();

            var jsonQueryString =
                JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"Contact\", \"fields\":[\"Id\", \"Email\", \"Account.OMS_ID__c\",\"Account.Parent.OMS_ID__c\"]}]}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExists", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and quert string - {jsonQueryString} for user with _productUsername {_productUsername}." });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExists", $"Contact exists for user with _productUsername {_productUsername}." });

                foreach (var cpContact in data.searchRecords)
                {
                    AdminSupportPortalContactResult adminSupportPortalContactResult = new AdminSupportPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c,
                        ParentOMS_ID__c = (cpContact.Account == null || cpContact.Account.Parent == null) ? "" : cpContact.Account.Parent.OMS_ID__c,
                    };
                    adminSupportPortalContacts.Add(adminSupportPortalContactResult);
                }
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExists", $"No existing contact exists for user with _productUsername {_productUsername}." });
            }

            return adminSupportPortalContacts;
        }

        private List<AdminSupportPortalContactResult> CheckClientPortalContactsExistsByAccount(string loginName, string accountOmsId)
        {
            List<AdminSupportPortalContactResult> adminSupportPortalContacts = new List<AdminSupportPortalContactResult>();

            var jsonQueryString =
                JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"Contact\", \"fields\":[\"Id\", \"Email\", \"Account.OMS_ID__c\"], \"where\" : \"Account.OMS_ID__c = \'" + accountOmsId + "\'\"}]}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExistsByAccount", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and quert string - {jsonQueryString} for user with _productUsername {_productUsername}." });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExistsByAccount", $"Contact exists for user with _productUsername {_productUsername}." });

                foreach (var cpContact in data.searchRecords)
                {
                    AdminSupportPortalContactResult adminSupportPortalContactResult = new AdminSupportPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c
                    };
                    adminSupportPortalContacts.Add(adminSupportPortalContactResult);
                }
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExistsByAccount", $"No existing contact exists for user with _productUsername {_productUsername}." });
            }

            return adminSupportPortalContacts;
        }

        private List<AdminSupportPortalContactResult> CheckClientPortalUserExists(string loginName)
        {
            List<AdminSupportPortalContactResult> adminSupportPortalContacts = new List<AdminSupportPortalContactResult>();

            var jsonQueryString =
                JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"User\", \"fields\":[\"Id\", \"Email\",\"IsPortalEnabled\", \"Account.OMS_ID__c\"]}]}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and query string - {jsonQueryString} for user with _productUsername {_productUsername}." });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"User exists for user with _productUsername {_productUsername}." });

                foreach (var cpContact in data.searchRecords)
                {
                    AdminSupportPortalContactResult adminSupportPortalContactResult = new AdminSupportPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c,
                        IsPortalEnabled = cpContact.IsPortalEnabled
                    };
                    adminSupportPortalContacts.Add(adminSupportPortalContactResult);
                }
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"No existing user exists for user with _productUsername {_productUsername}." });
            }

            return adminSupportPortalContacts;
        }

        private List<AdminSupportPortalContactResult> CheckClientPortalUserExists(string loginName, string accountOmsId)
        {
            List<AdminSupportPortalContactResult> adminSupportPortalContacts = new List<AdminSupportPortalContactResult>();

            var jsonQueryString =
             JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"User\", \"fields\":[\"Id\", \"Email\", \"Account.OMS_ID__c\"], \"where\" : \"Account.OMS_ID__c = \'" + accountOmsId + "\'\"}]}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and quert string - {jsonQueryString} for user with _productUsername {_productUsername} - {accountOmsId}." });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"Contact exists for user with _productUsername {_productUsername} - {accountOmsId}." });

                foreach (var cpContact in data.searchRecords)
                {
                    AdminSupportPortalContactResult adminSupportPortalContactResult = new AdminSupportPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c
                    };
                    adminSupportPortalContacts.Add(adminSupportPortalContactResult);
                }
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"No existing contact exists for user with _productUsername {_productUsername} - {accountOmsId}." });
            }

            return adminSupportPortalContacts;
        }

        private string GetClientPortalContactAccountId(string accountOmsId)
        {
            string result = string.Empty;

            var jsonQueryString =
                JObject.Parse(
                    $"{{\"q\":\"{accountOmsId}\", \"sobjects\":[{{\"name\": \"Account\", \"fields\":[\"Id\", \"OMS_ID__c\"], \"where\" : \"OMS_ID__c = \'{accountOmsId}\'\"}}]}}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientPortalContactAccountId", $"Getting Account for Oms Id - {accountOmsId}." });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientPortalContactAccountId", $"Received Account for Oms Id - {accountOmsId}." });

                result = data.searchRecords[0].Id;
            }

            return result;
        }

        private string CreateAdminSupportPortalContact(AdminSupportPortalContact adminSupportPortalContact)
        {
            var logData = new Dictionary<string, object> { { "clientPortalContact", adminSupportPortalContact } };

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAdminSupportPortalContact", $"Beginning" }, logData: logData);

            var result = PostApi($"{_apiRoute}sobjects/Contact", adminSupportPortalContact);

            if (string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAdminSupportPortalContact", $"Error for user - {adminSupportPortalContact.Email}. result - {result}" });
                throw new Exception("Error while creating contact.");
            }
            dynamic userResult = JsonConvert.DeserializeObject<dynamic>(result);

            try
            {
                if (userResult != null)
                    return userResult.id;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAdminSupportPortalContact", $"Error for user - {adminSupportPortalContact.Email}" }, exception: ex);
                throw new Exception($"Error while creating contact.{userResult?.ToString()} Error - {ex.Message}");
            }

            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAdminSupportPortalContact", $"Error while creating contact - {adminSupportPortalContact.Email}." });
            throw new Exception("Error while creating contact.");
        }

        private string CreateAdminSupportPortalUser(long userPersonaId, AdminSupportPortalUser adminSupportPortalUser, string roleType, bool isPortalEnabled)
        {
            var logData = new Dictionary<string, object> { { "adminSupportPortalUser", adminSupportPortalUser } };
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAdminSupportPortalUser", $"Beginning" }, logData: logData);

            var result = PostApi($"{_apiRoute}sobjects/User", adminSupportPortalUser);
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception($"ManageProductAdminSupportPortal.CreateadminSupportPortalUser - Error while creating user with userPersonaId {userPersonaId}.");
            }

            dynamic userResult = JsonConvert.DeserializeObject<dynamic>(result);

            if (result.Contains("errorCode") && userResult != null)
                throw new Exception($"ManageProductAdminSupportPortal.CreateadminSupportPortalUser - Error while creating user.{userResult[0].errorCode} - {userResult[0].message}");

            if (userResult != null)
            {
                var newId = userResult.id.ToString();
                CreateProductUserInGreenBook(userPersonaId, newId, adminSupportPortalUser.Username, roleType);
                if (!isPortalEnabled)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAdminSupportPortalUser", $"Beginning - isPortalEnabled {isPortalEnabled}" }, logData: logData);
                    SamlAttributes samlAttributes = new SamlAttributes();
                    IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);
                    if (productAttributes != null && productAttributes.Count > 0)
                    {
                        SamlAttributes samlAttributeToUpdate = productAttributes.FirstOrDefault(m => m.SamlAttributeId == (int)SamlAttributeEnum.UserId);
                        if (samlAttributeToUpdate != null)
                        {
                            samlAttributes.SamlUserAttributeId = samlAttributeToUpdate.SamlUserAttributeId;
                            samlAttributes.Value = newId;
                            _samlRepository.UpdateSamlUserAttribute(samlAttributes);
                        }
                    }
                }
                return "";
            }

            throw new Exception($"Error while creating user with userPersonaId {userPersonaId}");
        }

        private string UpdateAdminSupportPortalUser(AdminSupportPortalUser adminSupportPortalUser, string productUserId, long userPersonaId, string roleType)
        {
            var logData = new Dictionary<string, object> { { "adminSupportPortalUser", adminSupportPortalUser } };

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAdminSupportPortalUser", $"Beginning" }, logData: logData);

            var result = PostApi($"{_apiRoute}sobjects/User/{productUserId}?_HttpMethod=PATCH", adminSupportPortalUser);
            if (!string.IsNullOrEmpty(result))
            {
                throw new Exception($"Error while updating user - {result}");
            }
            else
            {
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);

                foreach (var attribute in productAttributes)
                {
                    if (attribute.Name.ToUpper() == "ROLECODE")
                    {
                        attribute.Value = roleType;
                        _samlRepository.UpdateSamlUserAttribute(attribute);
                    }
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAdminSupportPortalUser", $"Setting product status to Success. productUserId-{productUserId}, userPersonaId-{userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

            return "";
        }

        private static string PostApi(string partialUrl, object json)
        {
            string result = string.Empty;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync(_instanceUrl + partialUrl, json).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        result = userResult.ToString();
                    }
                }
                else
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic errorResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (errorResult != null)
                    {
                        result = errorResult.ToString();
                    }
                }
            }

            return result;
        }

        private IList<ProductRole> GetProductRoles()
        {
            IList<ProductRole> productRoles = new List<ProductRole>
            {
                new ProductRole {ID = "00e1G000000JItR", Name = "Client Portal Light", Roletype = "Support Portal"},
                new ProductRole {ID = _clientportalUltraLightRoleId, Name = "Client Portal Ultra Light", Roletype = "Support Portal"},
                new ProductRole {ID = "00e37000000MkG1", Name = "Client Portal with Cancellations", Roletype = "Admin Portal"},
                new ProductRole {ID = "00e37000000MkFm", Name = "Client Portal with Billing", Roletype = "Admin Portal"},
                new ProductRole
                {
                    ID = "00e00000006qqxo",
                    Name = "Client Portal with Transaction Limit and BAC Requestor",
                    Roletype = "Admin Portal"
                },
                new ProductRole {ID = "00e00000006qqxn", Name = "Client Portal with Transaction Limit and BAC Approver", Roletype = "Admin Portal"},
                new ProductRole
                {
                    ID = "00e00000006qqxm",
                    Name = "Client Portal with Billing, Cancellations, and Payments Admin",
                    Roletype = "Admin Portal"
                },
                new ProductRole {ID = "00e00000006qqxh", Name = "Client Portal Standard User", Roletype = "Support Portal"},
                new ProductRole {ID = "00e1G000000ZR97", Name = "Client Portal Support Admin", Roletype = "Admin Portal"},
                new ProductRole {ID = "00e00000006qqxf", Name = "Client Portal Administrator", Roletype = "Admin Portal"},
                new ProductRole {ID = "00e00000006qqxc", Name = "Client Portal with Billing and Cancellations", Roletype = "Admin Portal"},
                //"00e00000006ojYqAAI", "System Administrator"
            };

            return productRoles;
        }

        private void CreateProductUserInGreenBook(long userPersonaId, string newid, string productLoginName, string roleType)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Inserting in GB -productUsername -{productLoginName} and userId {newid}." });
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername,
                productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.RoleCode,
                roleType);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);

            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.portal_id, _portalId);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.organization_id, _organizationId);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Create user success. Setting product status to Success. productLoginName-{productLoginName}, userPersonaId-{userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus,
                (int)ProductBatchStatusType.Success);
        }

        private string GetAliasFromLogin(string userLoginName)
        {
            string result = userLoginName;
            if (userLoginName.IndexOf('@') >= 0)
            {
                result = userLoginName.Split('@')[0];
            }

            if (result.Length > 8)
                return result.Substring(0, 8);

            return result;
        }

        private static T GetResultFromApi<T>(string partialUrl) where T : class
        {
            T result = null;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.GetAsync(_instanceUrl + partialUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    T userResult = JsonConvert.DeserializeObject<T>(jsonContent);

                    if (userResult != null)
                    {
                        result = userResult;
                    }
                }
                else
                {
                    try
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        dynamic errorResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                        if (errorResult != null)
                        {
                            // LOG errorResult.ToString();
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if a user type is with proper email (shouldn't be RealPageEmployee = 403,UserNoEmail = 404,External = 405)
        /// </summary> 
        private bool IsUserWithEmail(long userPersonaId)
        {
            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsUserWithEmail", $"Getting status, userPersonaId={userPersonaId}" });
            SharedObjects.IdentityConfig.PartyRelationship partyRelationship = _managePartyRelationship.GetPartyRelationship(userPersona.RealPageId, userPersona.Organization.RealPageId,
                roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type");

            if (partyRelationship?.RoleTypeIdFrom == (int)UserRoleType.UserNoEmail ||
                partyRelationship?.RoleTypeIdFrom == (int)UserRoleType.RealPageEmployee)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsUserWithEmail", $"{partyRelationship?.RoleTypeIdFrom} userPersonaId={userPersonaId} : false" });
                return false;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsUserWithEmail", $"{partyRelationship?.RoleTypeIdFrom} userPersonaId={userPersonaId} : true" });
            return true;
        }
        #endregion
    }

    #region Refactor this classes after beta

    internal class AdminSupportPortalUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ProfileId { get; set; } //" : "00e00000006qqxf",
        public string Username { get; set; } //":"Resttest @mailinator.com",
        public string Alias { get; set; } //":"Rtest", 
        public string CommunityNickname { get; set; } //":"Resttest @mailinator.com", 
        public string TimeZoneSidKey { get; set; } //":"America/Chicago", 
        public string LocaleSidKey { get; set; } //":"en_US", 
        public string EmailEncodingKey { get; set; } //":"UTF-8", 
        public string LanguageLocaleKey { get; set; } //":"en_US",

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ContactId { get; set; } //":"0033C0000055jYbQAI"

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RoleType { get; set; } //":"Admin Portal"

        public bool IsActive { get; set; } //true/false     

        public bool IsCreatedFromNewPortal__c { get; set; }
    }

    public class AdminSupportPortalContact
    {
        //public string ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AccountId { get; set; }
        public bool Unified_Platform_User__c { get; set; }
        public bool Portal_User_Migrated__c { get; set; }
    }

    internal class AdminSupportPortalAccount
    {
        public string OMS_Account_ID__c { get; set; }
        public string OMS_ID__c { get; set; }
        public string Type { get; set; }
    }

    internal class AdminSupportPortalContactResult
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string OMS_ID__c { get; set; }

        public string ParentOMS_ID__c { get; set; }

        public bool IsPortalEnabled { get; set; } = true;
    }

    #endregion
}
