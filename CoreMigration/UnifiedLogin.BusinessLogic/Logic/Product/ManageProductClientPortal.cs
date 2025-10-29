using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.ClientPortal;
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
    public class ManageProductClientPortal : ManageProductBase, IManageProductClientPortal
    {
        #region Private members

        private IProductInternalSettingRepository _productInternalSettingRepository;

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
        public ManageProductClientPortal(DefaultUserClaim userClaims) : base((int)ProductEnum.ClientPortal, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductClientPortal", "Getting Product settings" });
#endif
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _productId = (int)ProductEnum.ClientPortal;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
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
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductClientPortal", "Received Product settings; getting token values" });
#endif
            GetSalesforceTokenInstanceUrl();
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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"At beginning of method for user with editorPersona id - {editorPersonaId}" });
            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                int companyInstanceId = GetProductCompanyInstanceId(_udmSourceCode, useTranslate: false).CompanyInstanceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Found blue book company instance id - {companyInstanceId}  for user editorPersona id -{editorPersonaId}" });

                CompanyPropertyRootObject companyProperties = _blueBook.GetCompanyPropertyInstance(companyInstanceId);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Found total {companyProperties.data.attributes.getCompanyPropertyInstances.Count} properties with blue book company instance id {companyInstanceId} for user with editorPersona id - {editorPersonaId}" });

                IList<ProductProperty> blueBookPropertyList = companyProperties.MapBlueBookToGBProperties() ?? new List<ProductProperty>();
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MapBlueBookToGBProperties() completed for user with editorPersona id -{editorPersonaId}" });

                // need to do a filter on the result
                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0)) //update existing user
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Calling MergeProductPropertiesWithGreenbook....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}" });
                    result = MergeProductPropertiesWithGreenbook(blueBookPropertyList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Completed for user with editorPersona id -{editorPersonaId}" });
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
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}" });
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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"At beginning of method for user with editorPersona id - {editorPersonaId}" });
            var response = new ListResponse();
            try
            {
                var result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                var clientPortalAllRoles = GetProductRoles();

                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0))  // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeProductRolesWithGreenBook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}" });
                    response = MergeProductRolesWithGreenBook(clientPortalAllRoles);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeProductRolesWithGreenBook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}" });
                }
                else // Called during creating a new User
                {
                    ProductRole accessGroup = (from a in clientPortalAllRoles
                                               where a.ID == ""
                                               select a).FirstOrDefault();
                    if (accessGroup != null)
                    {
                        accessGroup.IsAssigned = true;
                    }

                    clientPortalAllRoles.FirstOrDefault(s => s.Name.ToLower().Trim() == "client portal standard user").IsAssigned = true;

                    response = new ListResponse()
                    {
                        Records = clientPortalAllRoles.Cast<object>().ToList(),
                        TotalRows = clientPortalAllRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Exiting, total rows - {clientPortalAllRoles.Count} for user with editorPersona id - {editorPersonaId}" });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRoles", $"Error for user with editorPersona id - {editorPersonaId}" });
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ManageClientPortalUser(long editorPersonaId, long userPersonaId,
            ClientPortalPropertyRole clientPortalPropertyRole)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Begin create/update user for user with editorPersona id - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                ClientPortalContactResult  clientPortalContactResult = new ClientPortalContactResult();
                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);
                persona.Organization = organizationList.FirstOrDefault(i => i.PartyId == persona.OrganizationPartyId);

                var personaOrganization = persona.Organization;
                bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

                string productLoginName;
                bool isUserUpdate = false;
                if (string.IsNullOrEmpty(_productUsername))
                {
                    if (!IsUserWithEmail(userPersonaId) || !RegexUtilities.IsValidEmail(userLogin.LoginName))
                    {
                        // throw exception
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"No valid email address for user with editorPersona id - {editorPersonaId}" });
                        return $"Error-ManageProductClientPortal.ManageClientPortalUser - no valid email address for user with editorPersona id - {editorPersonaId}.";
                    }

                    productLoginName = userLogin.LoginName;
                }
                else
                {
                    productLoginName = _productUsername;
                    isUserUpdate = true;
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"_productUsername for user is {_productUsername} and editorPersonaId  - {userPersonaId}" });
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Error for user with editorPersona id - {editorPersonaId} Error - Company not found" });
                    return $"ManageProductClientPortal.ManageClientPortalUser - Company not found for user with editorPersona id - {editorPersonaId}.";
                }


                // super user
                if (IsSuperUser(userPersonaId))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"New user is Super user with editorPersona id - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                    clientPortalPropertyRole = new ClientPortalPropertyRole
                    {
                        PropertyList = new List<string> { "-1" },
                        RoleList = new List<string> { "00e00000006qqxf" }
                    };
                }

                // check Contact by Property or PMC to get OMS Id
                string searchOmsId = string.Empty;
                string parentOmsId = company.CompanyInstanceSourceId;
                if (clientPortalPropertyRole.PropertyList != null && clientPortalPropertyRole.PropertyList.Count > 0 && clientPortalPropertyRole.PropertyList[0] != null &&
                    clientPortalPropertyRole.PropertyList[0].Length > 3)
                {
                    searchOmsId = clientPortalPropertyRole.PropertyList[0];
                }
                else
                {
                    searchOmsId = company.CompanyInstanceSourceId;
                }


                // For multiple contacts result
                var clientPortalContactAcrossCompanies = CheckClientPortalContactsExists(userLogin.LoginName);
                List<ClientPortalContactResult> clientPortalContactResults = new List<ClientPortalContactResult>();
                // 'Contact.Account.Parent.OMS_ID__c'
                if (clientPortalContactAcrossCompanies != null && clientPortalContactAcrossCompanies.Count > 0) 
                {
                    foreach (var item in clientPortalContactAcrossCompanies)
                    {
                        if (item.ParentOMS_ID__c == parentOmsId) 
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"ParentId equalsto parentOMSId - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                            clientPortalContactResults.Add(item);
                        }
                        else if (item.OMS_ID__c != null && item.OMS_ID__c.StartsWith("C") && string.IsNullOrEmpty(item.ParentOMS_ID__c) && item.OMS_ID__c == parentOmsId)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"parentId equalsto ParentOMS_ID__c is empty - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                            clientPortalContactResults.Add(item);
                        }
                        else if (item.OMS_ID__c != null && item.OMS_ID__c.StartsWith("C") && !string.IsNullOrEmpty(item.ParentOMS_ID__c) && item.OMS_ID__c == parentOmsId && item.ParentOMS_ID__c.StartsWith("C"))
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"ParentOMS_ID__c is not empty - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                            clientPortalContactResults.Add(item);
                        }
                        else if ((item.OMS_ID__c != null && item.OMS_ID__c.StartsWith("C") && !string.IsNullOrEmpty(item.ParentOMS_ID__c) && parentOmsId.Equals(item.ParentOMS_ID__c) && item.ParentOMS_ID__c.StartsWith("C")))
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Company is having a Parent company - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                            clientPortalContactResults.Add(item);
                        }
                    }
                }
               
                var contactId = string.Empty;
                string accountId = string.Empty;
                isMultiCompanyUser = (isExternalUser || string.IsNullOrEmpty(_productUsername) || clientPortalContactResults.Count > 0) && !isUserUpdate;
                var uniqueProductLoginName = !isUserUpdate ? IterateUserNameIfExists(productLoginName) : productLoginName;

                // If no contact then create new contact in salesforce
                if (clientPortalContactResults == null || clientPortalContactResults.Count == 0 )
                {
                    // Find Account Id in salesforce for oms Id
                    accountId = GetClientPortalContactAccountId(searchOmsId);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Account id {accountId} received for user with _productUsername {_productUsername}" });
                    // create a new contact in salesforce
                    contactId =
                      CreateClientPortalContact(new ClientPortalContact
                      {
                          AccountId = accountId,
                          Email = productLoginName,
                          FirstName = person.FirstName,
                          LastName = person.LastName,
                          Unified_Platform_User__c = true,
                          Portal_User_Migrated__c = true
                      });
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"New contact created with contact id {contactId} and account id {accountId} received for user with _productUsername {_productUsername}" });
                }
                else
                {
                    foreach (var contact in clientPortalContactResults)
                    {
                        //contact exists; check update on property if yes then update contact
                        contactId = contact.Id;

                        // PMC to PMC OMS change is not allowed for same user login
                        if (!string.IsNullOrEmpty(contact.OMS_ID__c) && !string.IsNullOrEmpty(searchOmsId) && contact.OMS_ID__c != searchOmsId)
                        {
                            if (searchOmsId[0] == 'C' && contact.OMS_ID__c[0] == 'C')
                            {
                                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Error for user with editorPersona id - {editorPersonaId} - PMC to PMC OMS change is not allowed." });
                                return "Error - PMC to PMC OMS change is not allowed.";
                            }
                        }

                        if (!string.IsNullOrEmpty(contact.OMS_ID__c) && !string.IsNullOrEmpty(searchOmsId) && contact.OMS_ID__c != searchOmsId)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Update contact - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                            // update salesforce contact with new OMS ID
                            UpdateContact(contactId, searchOmsId, false, true);
                        }
                        else if(!string.IsNullOrEmpty(contact.OMS_ID__c) && !string.IsNullOrEmpty(contactId) && contact.OMS_ID__c == searchOmsId)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Update PortalUser MigratedFlag - {editorPersonaId} and editorPersonaId  - {userPersonaId}" });
                            UpdatePortalUserMigratedFlag(contactId);
                        }
                    }
                }

                var clientPortaluserDetails = CheckClientPortalUserExists(userLogin.LoginName);


                var clientPortalUser = new ClientPortalUser
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
                    ProfileId = clientPortalPropertyRole.RoleList[0],
                    IsActive = true
                };

                if (isUserUpdate)
                {
                    clientPortalContactResult = clientPortaluserDetails.FirstOrDefault(m => m.Id == _productUserId);
                }
                if (clientPortaluserDetails != null && clientPortaluserDetails.Count > 0  && (clientPortaluserDetails.Any(m=> m.Id == _productUserId)) && clientPortalContactResult != null && clientPortalContactResult.IsPortalEnabled)
                {
                    // Update User & return result
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Trying to UPDATE user with editorPersona id - {editorPersonaId}" });
                    
                    // set fields to null which we can't update
                    clientPortalUser.ContactId = null;

                    var updateResult = UpdateClientPortalUser(clientPortalUser, _productUserId, userPersonaId);

                    return updateResult;
                }
                else
                {
                    // Create New User & return result
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageClientPortalUser", $"Trying to CREATE user with editorPersona id - {editorPersonaId}" });
                    string insertResult = CreateClientPortalUser(userPersonaId, clientPortalUser, clientPortalContactResult.IsPortalEnabled);

                    return insertResult;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageClientPortalUser", $"Error for user with editorPersona id - {editorPersonaId}" });

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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IterateUserNameIfExists", $"Generated iterated clientPortalLoginName = {clientPortalLoginName}" });
            return productLoginName;
        }

        /// <summary>
        /// Updates user profile  
        /// </summary>
        public string UpdateClientPortalUserProfile (long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateClientPortalUserProfile", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
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
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateClientPortalUserProfile", $"No valid email address for user with editorPersona id - {editorPersonaId}" });
                    return $"Error-ManageProductClientPortal.UpdateClientPortalUserProfile - no valid email address for user with editorPersona id - {editorPersonaId}.";
                }

                productLoginName = userLogin.LoginName;
            }
            else
            {
                productLoginName = _productUsername;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateClientPortalUserProfile", $"_productUsername for user is {_productUsername}" });

            List<ClientPortalContactResult> clientPortalList = CheckClientPortalUserExists(productLoginName);
            List<ClientPortalContactResult> salesForceContactResults = null;
            ClientPortalUser clientPortalUser = GetClientPortalUser();
            if (clientPortalUser == null)
            {
                return "Error getting user info from ClientPortal";
            }
            clientPortalUser.Email = userLogin.LoginName;
            clientPortalUser.FirstName = person.FirstName;
            clientPortalUser.LastName = person.LastName;
            clientPortalUser.Username = productLoginName;
            clientPortalUser.IsActive = true;
            clientPortalUser.ContactId = null;

            var result =  PostApi($"{_apiRoute}sobjects/User/{_productUserId}?_HttpMethod=PATCH", clientPortalUser);

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


                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateClientPortalUserProfile", $"Update in GB -productUsername -{productLoginName} and userId {_productUserId}" });
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

            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateClientPortalUserProfile", $"Error for user with userPersonaId:{userPersonaId} and productUserId: {_productUserId}. ErrorReason-{result}" });

            return result;
        }

        public string ManageSalesForceUser(long editorPersonaId, long userPersonaId, ClientPortalPropertyRole clientPortalPropertyRole, bool isUnassigned = false)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageSalesForceUser", $"Begin create/update user for user with editorPersona id - {editorPersonaId}" });
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
                WriteToErrorLog(exception: ex, message: "{ActionName} - {state}", messageProperties: new object[] { "ManageSalesForceUser", $"Error for user with editorPersona id - {editorPersonaId}" });
                return $"Error - {ex.Message}";
            }
        }

        private void UpdateContact(string contactId, string searchOmsId, bool formerInactive, bool unifiedLoginUser)
        {
            // Find Account Id for new OMS Id
            var accountId = GetClientPortalContactAccountId(searchOmsId);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContact", $"Account id {accountId} received for user with _productUsername {_productUsername}.; OMS ID- {searchOmsId}" });
            dynamic accountObj = new ExpandoObject();
            accountObj.AccountId = accountId;
            accountObj.Unified_Platform_User__c = unifiedLoginUser;
            accountObj.Former_Inactive__c = formerInactive;
            accountObj.Portal_User_Migrated__c = true;
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", accountObj);
            if (string.IsNullOrEmpty(result)) return;

            var logData = new Dictionary<string, object> { { "result", result } };
            WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateContact", $"Error for user with contactId - {contactId}" });
            throw new Exception($"Error while updating user - {result}");
        }


        private void UpdatePortalUserMigratedFlag(string contactId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePortalUserMigratedFlag", $"ContactId id {contactId} received for user with _productUsername {_productUsername}. - Portal_User_Migrated__c setting to true" });

            dynamic accountObj = new ExpandoObject(); 
            accountObj.Portal_User_Migrated__c = true;
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", accountObj);
            if (string.IsNullOrEmpty(result)) return;

            var logData = new Dictionary<string, object> { { "result", result } };
            WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdatePortalUserMigratedFlag", $"Error for user with contactId - {contactId}" });
            throw new Exception($"Error while UpdatePortalUserMigratedFlag updating user - {result}");
        }


        private void UpdateContactProfile(string contactId, string firstName, string LastName, string email)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContactProfile", $"ContactId id {contactId} received for user with _productUsername {_productUsername}. - Portal_User_Migrated__c setting to true" });
            
            dynamic accountObj = new ExpandoObject();
            accountObj.Email = email;
            accountObj.FirstName = firstName;
            accountObj.LastName = LastName;
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", accountObj);
            if (string.IsNullOrEmpty(result)) return;

            var logData = new Dictionary<string, object> { { "result", result } };
            WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateContactProfile", $"Error for user with contactId - {contactId}" });
            throw new Exception($"Error while updating UpdateContactInfo for user user - {result}");
        }


        private void UpdateContactSalesForce(string contactId, string searchOmsId, bool isUnassigned)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateContactSalesForce", $"Account id {contactId} received for user with _productUsername {_productUsername}.; OMS ID- {searchOmsId}" });
            dynamic contactObj = new ExpandoObject();
            contactObj.Unified_Platform_User__c = (isUnassigned == false);
            contactObj.Former_Inactive__c = isUnassigned;     
            var result = PostApi($"{_apiRoute}sobjects/Contact/{contactId}?_HttpMethod=PATCH", contactObj);
            if (string.IsNullOrEmpty(result)) return;

            var logData = new Dictionary<string, object> { { "result", result } };
            WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateContactSalesForce", $"Error for user with contactId - {contactId}" });
            throw new Exception($"Error while updating user - {result}");
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
        public string UnassignSalesForceUser(long editorPersonaId, long userPersonaId, ClientPortalPropertyRole clientPortalPropertyRole)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            var persona = _managePersona.GetPersona(userPersonaId);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"userPersonaId:{userPersonaId} - start Deactivated user successfully in Client Portal" });
            var result = ManageSalesForceUser(editorPersonaId, userPersonaId, clientPortalPropertyRole, true);
            if (!string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"Error for user with userPersonaId:{userPersonaId} and productUserId: {_productUserId}. ErrorReason-{result}" });

                return result;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignSalesForceUser", $"userPersonaId:{userPersonaId} - end Deactivated contacts successfully in Client Portal" });
            return "";
        }

        /// <summary>
        /// Disable User
        /// </summary>
        public string EnableDisableUser(long editorPersonaId, long userPersonaId, bool isActive = false)
        {
            ClientPortalUser clientPortalUser = GetClientPortalUser();
            if (clientPortalUser == null)
            {
                return "Error getting user info from ClientPortal";
            }
            clientPortalUser.IsActive = isActive;
            clientPortalUser.ContactId = null;

            return PostApi($"{_apiRoute}sobjects/User/{_productUserId}?_HttpMethod=PATCH", clientPortalUser);
        }


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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
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

            string query = ($"SELECT Id,FirstName,LastName,Email,Username,LastLoginDate,IsActive FROM User" +
                                      $" WHERE (User.Contact.Account.OMS_ID__c = '{companyInstanceSourceId}' OR User.Contact.Account.Parent.OMS_ID__c = '{companyInstanceSourceId}')" +
                                      $" AND User.Contact.Portal_User_Migrated__c = {filter}" +
                                      $" LIMIT {resultPerRow} OFFSET {startRow}").Replace(' ', '+');

            var partialurl = $"{_apiRoute}query?q={query}";

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "Url", _instanceUrl + partialurl } }, messageProperties: new object[] { "GetMigrationUsers", "GetMigrationUsers" });

            var migrationResponse = GetResultFromApi<ClientPortalMigrationResponse>(partialurl);
            if (migrationResponse == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"no users received from product for user with editorPersona id - {editorPersonaId}" });
                return response;
            }

            var migrationUsers = new List<MigrationUser>();
            foreach (var user in migrationResponse.Records)
            {
                var migrationUser = new MigrationUser
                {
                    CompanyInstanceSourceId = companyInstanceSourceId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    LastActivity = user.LastLoginDate.ToString(),
                    Extra = $"{_portalId}|{_organizationId}",
                    Status = user.IsActive ? "Active" : "Disabled"
                };
                migrationUsers.Add(migrationUser);
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}" });

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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }

            dynamic contact = new ExpandoObject();
            var isError = false;
            foreach (var migrateUser in migrateUsers)
            {
                contact.Unified_Platform_User__c = migrateUser.UsingUnifiedLogin;
                contact.Portal_User_Migrated__c = true;
                var result = PostApi($"{_apiRoute}sobjects/User/{migrateUser.UserId}/Contact?_HttpMethod=PATCH", contact);
                if (!string.IsNullOrEmpty(result))
                {
                    migrateResponse.Message += result;
                    isError = true;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Updating unified login status. editorPersonaId-{editorPersonaId}" });
            }

            migrateResponse.Status = !isError;
            if (!isError)
                migrateResponse.Message = "success";

            return migrateResponse;
        }

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
#endregion

#endregion

        #region Private Methods

        private ClientPortalUser GetClientPortalUser()
        {
            ClientPortalUser clientPortalUser =
                GetResultFromApi<ClientPortalUser>($"{_apiRoute}sobjects/user/{_productUserId}");

            if (clientPortalUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientPortalUser", $"Error for user {_productUserId} - User not found" });
            }
            return clientPortalUser;

        }

        private ListResponse MergeProductRolesWithGreenBook(IList<ProductRole> allProductRoles)
        {
            ClientPortalUser clientPortalUser =
                GetResultFromApi<ClientPortalUser>($"{_apiRoute}sobjects/user/{_productUserId}");

            if (clientPortalUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductRolesWithGreenBook", $"Error for user {_productUserId} - User not found" });
                return new ListResponse() { IsError = true, ErrorReason = $"User {_productUserId} not found." };
            }

            // if a user record exists
            var profileId = clientPortalUser.ProfileId;

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

        private void GetSalesforceTokenInstanceUrl()
        {
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetSalesforceTokenInstanceUrl", "Beginning of the method" });

                ObjectCache tokenCache = MemoryCache.Default;

                // Get token values from cache
                _authToken = tokenCache["access_token_CP"] as string;
                _instanceUrl = tokenCache["instance_url_CP"] as string;

                // If no values from cache then get new one
                if (!string.IsNullOrEmpty(_authToken) && !string.IsNullOrEmpty(_instanceUrl)) return;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetSalesforceTokenInstanceUrl", "Null cache values. Getting new one" });
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
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetSalesforceTokenInstanceUrl", "Received null or empty values from Salesforce" });
                    throw new Exception("ManageProductClientPortal.GetSaleforceTokenInstanceUrl - Received null or empty values from Salesforce.");
                }

                var cachePolicy = new CacheItemPolicy
                {
                    // Expier cache every after 9 minutes (assuming 10 min is token expiration time)
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
                };

                tokenCache.Set("access_token_CP", _authToken, cachePolicy);
                tokenCache.Set("instance_url_CP", _instanceUrl, cachePolicy);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetSalesforceTokenInstanceUrl", "Received & populated cache with token values" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetSalesforceTokenInstanceUrl", $"Error: {ex.Message}" });
            }
        }

        private ListResponse MergeProductPropertiesWithGreenbook(IList<ProductProperty> blueBookPropertyList)
        {
            // merge the given user details with the list
            var clientPortalAccount = GetClientPortalAccount();
            if (clientPortalAccount == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductPropertiesWithGreenbook", $"Error - User not found in client portal with ProductUserId - {_productUserId}" });
                return new ListResponse()
                {
                    IsError = true,
                    ErrorReason = $"User not found in client portal with ProductUserId - {_productUserId}"
                };
            }

            var omsId = clientPortalAccount.OMS_ID__c;
            var omsType = clientPortalAccount.Type;

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductPropertiesWithGreenbook", $"received - omsId{omsId},omsType{omsType}" });

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

        private ClientPortalAccount GetClientPortalAccount()
        {
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "Url", $"{_apiRoute}sobjects/user/{_productUserId}/account" } }, messageProperties: new object[] { "GetClientPortalAccount", $"Calling API for _productUserId - {_productUserId}" });
            return GetResultFromApi<ClientPortalAccount>($"{_apiRoute}sobjects/user/{_productUserId}/account");
        }
        
        private List<ClientPortalContactResult> CheckClientPortalContactsExists(string loginName)
        {
            List<ClientPortalContactResult> clientPortalContacts = new List<ClientPortalContactResult>();

            var jsonQueryString =
                JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"Contact\", \"fields\":[\"Id\", \"Email\", \"Account.OMS_ID__c\",\"Account.Parent.OMS_ID__c\"]}]}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExists", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and query string - {jsonQueryString} for user with _productUsername {_productUsername}" });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExists", $"Contact exists for user with _productUsername {_productUsername}" });

                foreach (var cpContact in data.searchRecords)
                {
                    ClientPortalContactResult clientPortalContactResult = new ClientPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c,
                        ParentOMS_ID__c = (cpContact.Account == null || cpContact.Account.Parent ==null ) ? "" : cpContact.Account.Parent.OMS_ID__c,
                    };
                    clientPortalContacts.Add(clientPortalContactResult);
                }

            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExists", $"No existing contact exists for user with _productUsername {_productUsername}" });
            }

            return clientPortalContacts;
        }

        private List<ClientPortalContactResult> CheckClientPortalContactsExistsByAccount(string loginName, string accountOmsId )
        {
            List<ClientPortalContactResult> clientPortalContacts = new List<ClientPortalContactResult>();

            var jsonQueryString =
                JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"Contact\", \"fields\":[\"Id\", \"Email\", \"Account.OMS_ID__c\"], \"where\" : \"Account.OMS_ID__c = \'" + accountOmsId + "\'\"}]}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExistsByAccount", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and quert string - {jsonQueryString} for user with _productUsername {_productUsername}" });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExistsByAccount", $"Contact exists for user with _productUsername {_productUsername}" });

                foreach (var cpContact in data.searchRecords)
                {
                    ClientPortalContactResult clientPortalContactResult = new ClientPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c
                    };
                    clientPortalContacts.Add(clientPortalContactResult);
                }

            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalContactsExistsByAccount", $"No existing contact exists for user with _productUsername {_productUsername}" });
            }

            return clientPortalContacts;
        }


        private List<ClientPortalContactResult> CheckClientPortalUserExists(string loginName)
        {
            List<ClientPortalContactResult> clientPortalContacts = new List<ClientPortalContactResult>();

            var jsonQueryString =
                JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"User\", \"fields\":[\"Id\", \"Email\",\"IsPortalEnabled\", \"Account.OMS_ID__c\"]}]}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and quert string - {jsonQueryString} for user with _productUsername {_productUsername}" });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"User exists for user with _productUsername {_productUsername}" });

                foreach (var cpContact in data.searchRecords)
                {
                    ClientPortalContactResult clientPortalContactResult = new ClientPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c,
                        IsPortalEnabled = cpContact.IsPortalEnabled
                    };
                    clientPortalContacts.Add(clientPortalContactResult);
                }

            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"No existing user exists for user with _productUsername {_productUsername}" });
            }

            return clientPortalContacts;
        }

        private List<ClientPortalContactResult> CheckClientPortalUserExists(string loginName, string accountOmsId)
        {
            List<ClientPortalContactResult> clientPortalContacts = new List<ClientPortalContactResult>();

            var jsonQueryString =
             JObject.Parse("{ \"q\":\"" + loginName + "\",\"sobjects\":[{\"name\": \"User\", \"fields\":[\"Id\", \"Email\", \"Account.OMS_ID__c\"], \"where\" : \"Account.OMS_ID__c = \'" + accountOmsId + "\'\"}]}");
            
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"Calling API with - URL '{_apiRoute}parameterizedSearch' and quert string - {jsonQueryString} for user with _productUsername {_productUsername}" });
            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"Contact exists for user with _productUsername {_productUsername}" });

                foreach (var cpContact in data.searchRecords)
                {
                    ClientPortalContactResult clientPortalContactResult = new ClientPortalContactResult
                    {
                        Id = cpContact.Id,
                        Email = cpContact.Email,
                        OMS_ID__c = cpContact.Account == null ? "" : cpContact.Account.OMS_ID__c
                    };
                    clientPortalContacts.Add(clientPortalContactResult);
                }

            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckClientPortalUserExists", $"No existing contact exists for user with _productUsername {_productUsername}" });
            }

            return clientPortalContacts;
        }

        private string GetClientPortalContactAccountId(string accountOmsId)
        {
            string result = string.Empty;

            var jsonQueryString =
                JObject.Parse(
                    $"{{\"q\":\"{accountOmsId}\", \"sobjects\":[{{\"name\": \"Account\", \"fields\":[\"Id\", \"OMS_ID__c\"], \"where\" : \"OMS_ID__c = \'{accountOmsId}\'\"}}]}}");

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientPortalContactAccountId", $"Getting Account for Oms Id - {accountOmsId}" });

            var response = PostApi($"{_apiRoute}parameterizedSearch", jsonQueryString);

            dynamic data = JObject.Parse(response);

            if (data != null && data.searchRecords != null && data.searchRecords.Count >= 1)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientPortalContactAccountId", $"Received Account for Oms Id - {accountOmsId}" });

                result = data.searchRecords[0].Id;
            }

            return result;
        }

        private string CreateClientPortalContact(ClientPortalContact clientPortalContact)
        {
            var logData = new Dictionary<string, object> { { "clientPortalContact", clientPortalContact } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "CreateClientPortalContact", "Beginning" });

            var result = PostApi($"{_apiRoute}sobjects/Contact", clientPortalContact);

            if (string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateClientPortalContact", $"Error for user - {clientPortalContact.Email}. result - {result}" });
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
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateClientPortalContact", $"Error for user - {clientPortalContact.Email}" });
                throw new Exception($"Error while creating contact.{userResult?.ToString()} Error - {ex.Message}");
            }

            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateClientPortalContact", $"Error while creating contact - {clientPortalContact.Email}" });
            throw new Exception("Error while creating contact.");
        }

        private string CreateClientPortalUser(long userPersonaId, ClientPortalUser clientPortalUser, bool isPortalEnabled)
        {
            var logData = new Dictionary<string, object> { { "clientPortalUser", clientPortalUser } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "CreateClientPortalUser", "Beginning" });

            var result = PostApi($"{_apiRoute}sobjects/User", clientPortalUser);
            if (string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "result", result } }, messageProperties: new object[] { "CreateClientPortalUser", $"Error while creating user with userPersonaId {userPersonaId}" });
                throw new Exception($"ManageProductClientPortal.CreateClientPortalUser - Error while creating user with userPersonaId {userPersonaId}.");
            }

            dynamic userResult = JsonConvert.DeserializeObject<dynamic>(result);

            if (result.Contains("errorCode") && userResult != null)
            {
                WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userResult", userResult } }, messageProperties: new object[] { "CreateClientPortalUser", $"Error while creating user with userPersonaId {userPersonaId}" });
                throw new Exception($"ManageProductClientPortal.CreateClientPortalUser - Error while creating user.{userResult[0].errorCode} - {userResult[0].message}");
            }

            if (userResult != null)
            {
                var newId = userResult.id.ToString();
                CreateProductUserInGreenBook(userPersonaId, newId, clientPortalUser.Username);
                if (isPortalEnabled) return "";
                WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "CreateClientPortalUser", "Beginning" });
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
                return "";
            }
            WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userResult", "null" } }, messageProperties: new object[] { "CreateClientPortalUser", $"Error while creating user with userPersonaId {userPersonaId}" });
            throw new Exception($"Error while creating user with userPersonaId {userPersonaId}");
        }

        private string UpdateClientPortalUser(ClientPortalUser clientPortalUser, string productUserId, long userPersonaId)
        {
            var logData = new Dictionary<string, object> { { "clientPortalUser", clientPortalUser } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateClientPortalUser", "Beginning" });

            var result = PostApi($"{_apiRoute}sobjects/User/{productUserId}?_HttpMethod=PATCH", clientPortalUser);
            if (!string.IsNullOrEmpty(result))
            {
                throw new Exception($"Error while updating user - {result}");
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateClientPortalUser", $"Setting product status to Success. productUserId-{productUserId}, userPersonaId-{userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus,
                (int)ProductBatchStatusType.Success);

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
                new ProductRole {ID = "00e00000006qqxc", Name = "Client Portal with Billing and Cancellations"},
                new ProductRole {ID = "00e00000006qqxf", Name = "Client Portal Administrator"},
                new ProductRole {ID = "00e00000006qqxh", Name = "Client Portal Standard User"},
                new ProductRole
                {
                    ID = "00e00000006qqxm",
                    Name = "Client Portal with Billing, Cancellations, and Payments Admin"
                },
                new ProductRole {ID = "00e00000006qqxn", Name = "Client Portal with Transaction Limit and BAC Approver"},
                new ProductRole
                {
                    ID = "00e00000006qqxo",
                    Name = "Client Portal with Transaction Limit and BAC Requestor"
                },
                new ProductRole {ID = "00e37000000MkFm", Name = "Client Portal with Billing"},
                new ProductRole {ID = "00e37000000MkG1", Name = "Client Portal with Cancellations  "},
                //"00e00000006ojYqAAI", "System Administrator"
            };

            return productRoles;
        }

        private void CreateProductUserInGreenBook(long userPersonaId, string newid, string productLoginName)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Inserting in GB -productUsername -{productLoginName} and userId {newid}" });

            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.portal_id, _portalId);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.organization_id, _organizationId);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Create user success. Setting product status to Success. productLoginName-{productLoginName}, userPersonaId-{userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
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
            var userPersona = _managePersona.GetPersona(userPersonaId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsUserWithEmail", $"Getting status, userPersonaId={userPersonaId}" });
            var partyRelationship = _managePartyRelationship.GetPartyRelationship(userPersona.RealPageId, userPersona.Organization.RealPageId,
                roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type");

            if (partyRelationship?.RoleTypeIdFrom == (int)UserRoleType.UserNoEmail ||
                partyRelationship?.RoleTypeIdFrom == (int)UserRoleType.RealPageEmployee)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsUserWithEmail", $"IsUserWithEmail - {partyRelationship?.RoleTypeIdFrom} userPersonaId={userPersonaId} : false" });
                return false;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsUserWithEmail", $"IsUserWithEmail -{partyRelationship?.RoleTypeIdFrom} userPersonaId={userPersonaId} : true" });
            return true;
        }
        #endregion
    }

    #region Refactor this classes after beta

    internal class ClientPortalUser
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

        public bool IsActive { get; set; } //true/false     
    }

    public class ClientPortalContact
    {
        //public string ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string AccountId { get; set; }
        public bool Unified_Platform_User__c { get; set; }

        public bool Portal_User_Migrated__c { get; set; }

    }

    internal class ClientPortalAccount
    {
        public string OMS_Account_ID__c { get; set; }
        public string OMS_ID__c { get; set; }
        public string Type { get; set; }
    }

    internal class ClientPortalContactResult
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string OMS_ID__c { get; set; }

        public string ParentOMS_ID__c { get; set; }

        public bool IsPortalEnabled { get; set; } = true;
    }

    #endregion
}