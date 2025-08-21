using Microsoft.Extensions.Http;
using Newtonsoft.Json;
using Polly;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.CacheHelper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RealConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ProductRoleModel = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    public class ManageProductRealConnect : ManageProductBase
    {
        private readonly DefaultUserClaim _userClaims;
        private static string _apiEndPoint;
        private string _clientId;
        private static List<string> ref1Data = new List<string>() { "custom", "location", "position", "property" };
        private readonly IManageUnifiedSettings _manageUnifiedSettings;
        private readonly HttpClient lpClient;
        private readonly RPObjectCache _cache;
        private readonly IRedisCacheService _distributedCacheService;
        private readonly int _learningPathRedisChacheInMinutes;
        private readonly int _licenseDetailsRedisChacheInMinutes;
        private readonly string _isLearningPathAPICallsEnabled;
        private readonly string _panoramaApiKey;
        private LearningPathsContent _learningPathsForPanorama;
        private List<string> _selectedLPSlugs = new List<string>();

        public ManageProductRealConnect(DefaultUserClaim userClaims) : base(94, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _userClaims = userClaims;
            _cache = new RPObjectCache();
            _distributedCacheService = new RedisCacheService();
            _manageUnifiedSettings = new ManageUnifiedSettings(_userClaims);
            _editorRealPageId = _userClaims.UserRealPageGuid;
            var userPersonaInfo = GetUserLoginByPersonaId(_userClaims.PersonaId);
            _userClaims.OrganizationRealPageGuid = userPersonaInfo.Item2.Organization.RealPageId;
            var policyHandler = new PolicyHttpMessageHandler(GetRateLimitPolicy()) { InnerHandler = new HttpClientHandler() };
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            var _apiKey = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIKEY").Value;//TODO encrypt and save in db, decrypt here
            _learningPathRedisChacheInMinutes = _productInternalSettingList.FirstOrDefault(a => a.Name.ToUpper() == "LEARNINGPATHREDISCACHEINMINUTES")?.Value == null ? 120 : Convert.ToInt32(_productInternalSettingList.First(a => a.Name.ToUpper() == "LEARNINGPATHREDISCACHEINMINUTES")?.Value);
            _licenseDetailsRedisChacheInMinutes = _productInternalSettingList.FirstOrDefault(a => a.Name.ToUpper() == "LICENSEDETAILSREDISCACHEINMINUTES")?.Value == null ? 120 : Convert.ToInt32(_productInternalSettingList.First(a => a.Name.ToUpper() == "LICENSEDETAILSREDISCACHEINMINUTES")?.Value);
            _isLearningPathAPICallsEnabled = _productInternalSettingList.FirstOrDefault(a => a.Name.ToUpper() == "ISLEARNINGPATHAPICALLSENABLED")?.Value;
            _clientId = GetClientIdFromUDM();
            if (string.IsNullOrEmpty(_clientId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductRealConnect", $"Ctor UDM mapping not found for company {_userClaims.OrganizationRealPageGuid}" });
                throw new Exception($"UDM mapping not found for company {_userClaims.OrganizationRealPageGuid}");
            }

            _client = new HttpClient(policyHandler);
            _client.SetBearerToken(_apiKey);
            if (_isLearningPathAPICallsEnabled != null && _isLearningPathAPICallsEnabled == "1")
            {
                _panoramaApiKey = GetApiKeyForPanoramaFromSettings();
                if (string.IsNullOrEmpty(_panoramaApiKey))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductRealConnect", $"Ctor Panorama key not found in settings for company {_userClaims.OrganizationRealPageGuid}" });
                    throw new Exception($"Panorama key not found in settings for company {_userClaims.OrganizationRealPageGuid}");
                }
                else
                {
                    lpClient = new HttpClient(policyHandler);
                    lpClient.SetBearerToken(_panoramaApiKey);
                }
            }
        }

        /// <summary>
        /// Get roles from UL database
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">Not in use</param>
        /// <returns></returns>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            var logData = new Dictionary<string, object>();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Getting roles for product {_productId} editorPersonaId {editorPersonaId}" });
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    logData.Add("GetCompanyEditorAndUserDetails", listResponse.ErrorReason);
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "GetCompanyEditorAndUserDetails Error creating the user" }, logData: logData);
                    return listResponse;
                }

                var roles = _productRepository.ListRolesForProductByParty(_userClaims.OrganizationPartyId, new List<int>() { _productId }, _productId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Got roles for product {_productId} editorPersonaId {editorPersonaId}" });
                response = MergeRolesWithUser(roles, userPersonaId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Got roles for product {_productId} editorPersonaId {editorPersonaId} completed" });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error Getting roles for product {_productId} editorPersonaId {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get license details from RealConnect
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter">Not in use</param>
        /// <returns></returns>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            var logData = new Dictionary<string, object>();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Getting Licenses for product {_productId} editorPersonaId {editorPersonaId}" });
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);

                if (listResponse.IsError)
                {
                    logData.Add("GetCompanyEditorAndUserDetails", listResponse.ErrorReason);
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "GetCompanyEditorAndUserDetails Error creating the user" }, logData: logData);
                    return listResponse;
                }

                if (string.IsNullOrEmpty(_clientId))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"ClientId not found for company {_userClaims.OrganizationRealPageGuid}" });
                    response.IsError = true;
                    response.ErrorReason = "ClientId not found or company doesnt have product assigned";
                    return response;
                }
                var clientLicenseDetails = GetClientLicenseDetailsForPanoramaCached(_userClaims.OrganizationPartyId).Result;           
                var licenseJson = JsonConvert.SerializeObject(clientLicenseDetails);
                CompanyLicenses companyLicenses = new CompanyLicenses();
                companyLicenses.ManagerLicenses = JsonConvert.DeserializeObject<ClientLicenseDetails>(licenseJson);
                companyLicenses.LearnerLicenses = JsonConvert.DeserializeObject<ClientLicenseDetails>(licenseJson);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Got Licenses for product {_productId} editorPersonaId {editorPersonaId}" });

                if (!string.IsNullOrEmpty(_productLearnerId))
                {
                    var realConnectUser = GetUser(_productLearnerId).Result;

                    foreach (var license in realConnectUser.AllocatedLicenses)
                    {
                        if (companyLicenses.LearnerLicenses.Licenses.Exists(l => l.Id == license.LicenseId))
                        {
                            companyLicenses.LearnerLicenses.Licenses.Find(l => l.Id == license.LicenseId).IsAssigned = true;
                        }
                    }
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Got User details and updated license isassigned flag for product {_productId} editorPersonaId {editorPersonaId}" });
                }

                if (!string.IsNullOrEmpty(_productManagerId))
                {

                    var realConnectUser = GetUser(_productManagerId).Result;
                    foreach (var license in realConnectUser.AllocatedLicenses)
                    {
                        if (companyLicenses.ManagerLicenses.Licenses.Exists(l => l.Id == license.LicenseId))
                        {
                            companyLicenses.ManagerLicenses.Licenses.Find(l => l.Id == license.LicenseId).IsAssigned = true;
                        }
                    }
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Got User details and updated license isassigned flag for product {_productId} editorPersonaId {editorPersonaId}" });
                }

                foreach (var l in companyLicenses.ManagerLicenses.Licenses)
                {
                    if (l.Ref1 == "property") { l.SortId = 1; }
                    else if (l.Ref1 == "position") { l.SortId = 2; }
                    else if (l.Ref1 == "location") { l.SortId = 3; }
                    else { l.SortId = 4; }
                }

                companyLicenses.ManagerLicenses.Licenses = companyLicenses.ManagerLicenses.Licenses.OrderBy(o => o.SortId).ThenBy(c => c.Name).ToList();
                companyLicenses.LearnerLicenses.Licenses = companyLicenses.LearnerLicenses.Licenses.OrderBy(o => o.Name).ToList();
                var lstCompanyLicenses = new List<CompanyLicenses> { companyLicenses };

                response.Records = lstCompanyLicenses.Cast<object>().ToList();
                response.TotalRows = clientLicenseDetails.Licenses.Count;
                response.RowsPerPage = 9999;
                response.ErrorReason = string.Empty;
                response.TotalPages = 1;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Returning listresponse Licenses for product {_productId} editorPersonaId {editorPersonaId}" });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Error Getting properties for product {_productId} editorPersonaId {editorPersonaId}" }, exception: ex);
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createUserRealPageId"></param>
        /// <param name="createUserPersonaId"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="rolePropList"></param>
        /// <returns></returns>
        public string CreateUpdateUser(Guid createUserRealPageId, long createUserPersonaId, long assignUserPersonaId, object rolePropList)
        {
            if (string.IsNullOrEmpty(_clientId))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", $"ClientId not found for company {_userClaims.OrganizationRealPageGuid}" });
                return "ClientId not found or company doesnt have product assigned";
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Begin creating the user" });
            string result = string.Empty;
            string userEmailAddress = string.Empty;
            ProductUserRolePropertiesGroups userProp = rolePropList as ProductUserRolePropertiesGroups;
            if (userProp.RCLicenseDetails == null)
            {
                userProp.RCLicenseDetails = new RCProductBatch();
                userProp.RCLicenseDetails.LearnerLicenseId = new List<string>();
                userProp.RCLicenseDetails.ManagerLicenseId = new List<string>();
            }
            var logData = new Dictionary<string, object>();
            var listResponse = GetCompanyEditorAndUserDetails(createUserPersonaId, assignUserPersonaId);

            if (listResponse.IsError)
            {
                logData.Add("GetCompanyEditorAndUserDetails", listResponse.ErrorReason);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "GetCompanyEditorAndUserDetails Error creating the user" }, logData: logData);
                return listResponse.ErrorReason;
            }

            var roles = _productRepository.ListRolesForProductByParty(_userClaims.OrganizationPartyId, new List<int>() { _productId }, _productId);
            var selectedRoles = roles.Where(x => userProp.RoleList.Contains(x.ID)).Select(c => c.Alias).ToList();

            Persona userPersona = _managePersona.GetPersona(assignUserPersonaId);
            Guid realPageId = userPersona.RealPageId;

            Person person = _managePerson.GetPerson(realPageId);
            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "CreateUpdateUser", $"Got person info {realPageId}" });
            UserLoginOnly userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            if (selectedRoles.Count > 2)
            {
                logData.Add("CreateUpdateUser", $"More than 2 roles are selected for user {realPageId} product {_productId}");
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", $"More than 2 roles are selected for user {realPageId} product {_productId}" }, logData: logData);
            }
   
            var clientLicenses = GetClientLicenseDetailsForPanoramaCached(_userClaims.OrganizationPartyId).Result;
            var selectedLicenses = clientLicenses.Licenses.Where(x => userProp.RCLicenseDetails.LearnerLicenseId.Contains(x.Id)).ToList();

            if (!(selectedLicenses.Any(a => a.Ref1 == "position") && selectedLicenses.Any(a => a.Ref1 == "property") && selectedLicenses.Any(a => a.Ref1 == "location")))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "GetCompanyEditorAndUserDetails Error creating the user" });
                return "No license and manager information.";
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", $"Generating email for loginName {userLogin.LoginName}" });
            userEmailAddress = FormattedEmail(userLogin.LoginName, assignUserPersonaId, userPersona.RealPageId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", $"Generated email for loginName {userLogin.LoginName} is {userEmailAddress}" });
            if (_isLearningPathAPICallsEnabled != null && _isLearningPathAPICallsEnabled == "1")
            {
                //Holding LearningPaths content for 3 min to reduce calls to TI
                _learningPathsForPanorama = GetLearningPathsForPanoramaCached(_userClaims.OrganizationPartyId);
                //_cache.GetFromCache<LearningPathsContent>($"LearningPaths_Panorama_{_userClaims.OrganizationPartyId}", 3600, () => { return GetLearningPathsForPanorama(); });

                var selectedLP = selectedLicenses.SelectMany(x => x.LearningPathIds).Distinct().ToList();
                _selectedLPSlugs = _learningPathsForPanorama.ContentItems.Where(c => selectedLP.Contains(c.Id)).Select(c => c.Slug).ToList();
            }
            CreateRCUser user = new CreateRCUser()
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = userEmailAddress,
                ClientId = _clientId,
                CourseIds = selectedLicenses.SelectMany(y => y.CourseIds).Distinct().ToList(),
                StudentLicenseIds = selectedLicenses.Select(l => l.Id).Distinct().ToList(),
                ExternalCustomerId = userLogin.UserId.ToString(),
                Role = "student", //Set student role first
                LearningPathSlugs = _selectedLPSlugs
            };

            logData.Add("RCUserPayload", user);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Prepared user object to createuser" }, logData: logData);

            if (_productLearnerId == Guid.Empty.ToString() || string.IsNullOrEmpty(_productLearnerId))
            {
                //Create User
                string url = $"{_apiEndPoint}/users";
                logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                try
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Creating new user in the product" }, logData: logData);
                    var response = _client.PostAsJsonAsync<CreateRCUser>(url, user).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        if (jsonContent.Contains("errors"))
                        {
                            logData.Add("error", jsonContent);
                            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Error creating the user" }, logData: logData);
                            UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                            return $"Error creating user {jsonContent}";
                        }
                        var userResponse = JsonConvert.DeserializeObject<RealConnectUser>(jsonContent);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "User created successfully" }, logData: logData);
                        //save user saml attributes
                        UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Success);
                        _samlRepository.CreateSamlUserAttribute(assignUserPersonaId, _productId, SamlAttributeEnum.LearnerId, userResponse.Id.ToString());
                        _samlRepository.CreateSamlUserAttribute(assignUserPersonaId, _productId, SamlAttributeEnum.productUsername, user.Email);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Saml details created for user successfully" }, logData: logData);

                        //add second role if dual roles are selected in UI
                        if (selectedRoles.Count > 1)
                        {
                            logData.Add("Dual Role user", userResponse.Id);
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Adding dual role for user" }, logData: logData);
                            result = AddDualRoleToUser(userResponse.Id.ToString(), selectedRoles, assignUserPersonaId, clientLicenses, person, userLogin, userEmailAddress, userProp);
                        }
                        //result += BulkContentAssignment(userResponse.Id.ToString(), clientLicenses, selectedLicenses);

                        return result;
                    }
                    logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Error Creating user in product" }, logData: logData);
                    UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                    return $"Error creating user {response.Content.ReadAsStringAsync().Result}";
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Error creating user in exception" }, logData: logData, exception: ex);
                    return $"Error creating user {ex.Message}";
                }
            }
            else
            {
                var userInformation = GetUser(_productLearnerId).Result;
                if (userInformation != null && userInformation.Disabled)
                {
                    //Activate user if disabled before update
                    var userStatusResponse = UnassignUser(createUserPersonaId, assignUserPersonaId, "active");
                }

                //Update User
                string url = $"{_apiEndPoint}/users/{_productLearnerId}";
                logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                try
                {
                    user.ReplaceLicenseAccess = true; //Set to true to replace the license access
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Updating user in the product" }, logData: logData);
                    var response = _client.PutAsJsonAsync(url, user).Result;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Updated user in the product" }, logData: logData);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        if (jsonContent.Contains("errors"))
                        {
                            logData.Add("error", jsonContent);
                            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Error updating the user" }, logData: logData);
                            UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                            return $"Error creating user {jsonContent}";
                        }
                        var userResponse = JsonConvert.DeserializeObject<RealConnectUser>(jsonContent);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "User updated successfully" }, logData: logData);
                        UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Success);
                        UpdateSamlUserAttribute(assignUserPersonaId, (int)ProductEnum.RealConnect, SamlAttributeEnum.productUsername, user.Email);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Saml details updated for user successfully" }, logData: logData);

                        //add second role if dual roles are selected in UI
                        if (selectedRoles.Count > 1)
                        {
                            logData.Add("Dual Role user", userResponse.Id);
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Updating dual role for user" }, logData: logData);
                            result = AddDualRoleToUser(userResponse.Id.ToString(), selectedRoles, assignUserPersonaId, clientLicenses, person, userLogin, userEmailAddress, userProp);
                        }
                        else if (!string.IsNullOrEmpty(_productManagerId))
                        {
                            //remove dual role if only one role is selected in UI
                            result += RemoveDualRoleToUser(assignUserPersonaId);
                        }
                        //result += BulkContentAssignment(_productLearnerId, clientLicenses, selectedLicenses);

                        return result;
                    }
                    logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Error Creating user in product" }, logData: logData);
                    UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                    return $"Error creating user {response.Content.ReadAsStringAsync().Result}";
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", "Error updating user in exception" }, logData: logData, exception: ex);
                    return $"Error creating user {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createUserPersonaId"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="userStatus"></param>
        /// <returns></returns>
        public string UnassignUser(long createUserPersonaId, long assignUserPersonaId, string userStatus = "disabled")
        {
            var logData = new Dictionary<string, object>();
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(createUserPersonaId, assignUserPersonaId);
                if (listResponse.IsError)
                {
                    logData.Add("GetCompanyEditorAndUserDetails", listResponse.ErrorReason);
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "GetCompanyEditorAndUserDetails Error creating the user" }, logData: logData);
                    return listResponse.ErrorReason;
                }

                if (string.IsNullOrEmpty(_productLearnerId))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"ProductUserd is empty for product {_productId}" });
                    return $"Error unassigning userPersona {assignUserPersonaId}";
                }

                string url = $"{_apiEndPoint}/users/{_productLearnerId}/updateStatus";
                logData.Add("url", url);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "Begin disable user" }, logData: logData);
                RCUserStatus status = new RCUserStatus() { Status = userStatus };
                var response = _client.PutAsJsonAsync(url, status).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    if (jsonContent.Contains("errors"))
                    {
                        logData.Add("error", jsonContent);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "Error Unassigning the product to user" }, logData: logData);
                        UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                        return $"Error creating user {jsonContent}";
                    }
                    var user = JsonConvert.DeserializeObject<RCUserStatus>(jsonContent);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "Disable user successful" }, logData: logData);
                    UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, userStatus == "disabled" ? (int)ProductBatchStatusType.Deactivated : (int)ProductBatchStatusType.Success);
                    return string.Empty;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "Error disabling user" }, logData: logData);
                return $"Error creating user {response.Content.ReadAsStringAsync().Result}";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "Error disabling user in exception" }, logData: logData, exception: ex);
                return $"Error creating user {ex.Message}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createUserPersonaId"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <returns></returns>
        public string UpdateProductUserProfile(long createUserPersonaId, long assignUserPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", "Beginning profile update" });
            var logData = new Dictionary<string, object>();
            var listResponse = GetCompanyEditorAndUserDetails(createUserPersonaId, assignUserPersonaId);
            string userEmailAddress = string.Empty;

            if (listResponse.IsError)
            {
                logData.Add("GetCompanyEditorAndUserDetails", listResponse.ErrorReason);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", "GetCompanyEditorAndUserDetails Error updating user profile" }, logData: logData);
                return listResponse.ErrorReason;
            }

            if (string.IsNullOrEmpty(_clientId))
            {
                _userClaims.OrganizationRealPageGuid = _editorPersona.Organization.RealPageId;
                _clientId = GetClientIdFromUDM();
            }

            if (string.IsNullOrEmpty(_clientId))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", $"ClientId not found for company {_userClaims.OrganizationRealPageGuid}" });
                return "ClientId not found or company doesnt have product assigned";
            }

            Persona userPersona = _managePersona.GetPersona(assignUserPersonaId);
            Guid realPageId = userPersona.RealPageId;

            Person person = _managePerson.GetPerson(realPageId);
            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateProductUserProfile", $"Got person info {realPageId}" });
            UserLoginOnly userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
           
            var clientLicenses = GetClientLicenseDetailsForPanoramaCached(_userClaims.OrganizationPartyId).Result;
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", $"Generating email for loginName {userLogin.LoginName}" });
            userEmailAddress = FormattedEmail(userLogin.LoginName, assignUserPersonaId, userPersona.RealPageId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateUser", $"Generated email for loginName {userLogin.LoginName} is {userEmailAddress}" });

            UpdateUserProfile userProfile = new UpdateUserProfile()
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = userEmailAddress,
                ClientId = _clientId,
                Upsert = !string.IsNullOrEmpty(_productManagerId)
            };

            string url = $"{_apiEndPoint}/users/{(!string.IsNullOrEmpty(_productManagerId) ? _productManagerId : _productLearnerId)}";
            logData = new Dictionary<string, object>
            {
                { "url", url }
            };

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", "Calling user profile update" }, logData: logData);
                var response = _client.PutAsJsonAsync(url, userProfile).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    if (jsonContent.Contains("errors"))
                    {
                        logData.Add("error", jsonContent);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", "Error updating profile to the user" }, logData: logData);
                        UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                        return $"Error creating user {jsonContent}";
                    }
                    var user = JsonConvert.DeserializeObject<RealConnectUser>(jsonContent);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", "Profile update successful" }, logData: logData);
                    return string.Empty;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", "Error updating profile" }, logData: logData);
                return $"Error creating user {response.Content.ReadAsStringAsync().Result}";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", "Error updating profile in exception" }, logData: logData, exception: ex);
                return $"Error creating user {ex.Message}";
            }
        }

        #region Private
        /// <summary>
        /// 
        /// </summary>
        /// <param name="learnerUserId"></param>
        /// <param name="roles"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="clientLicenses"></param>
        /// <param name="person"></param>
        /// <param name="userLogin"></param>
        /// <param name="emailAddress"></param>
        /// <param name="userProp"></param>
        /// <returns></returns>
        private string AddDualRoleToUser(string learnerUserId, List<string> roles, long assignUserPersonaId, ClientLicenseDetails clientLicenses, Person person, UserLoginOnly userLogin, string emailAddress, ProductUserRolePropertiesGroups userProp)
        {
            if (string.IsNullOrEmpty(learnerUserId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "AddDualRoleToUser", "Cannot update dual role, userid is empty" });
                return "Cannot update dual role, userid is empty";
            }
            if (string.IsNullOrEmpty(_productManagerId))
            {
                //If ManagerId is empty then user need to be tagged for dual role first time
                TagDualRoleToUser(learnerUserId, roles, assignUserPersonaId);
            }

            string url = $"{_apiEndPoint}/users/{_productManagerId}";
            var logData = new Dictionary<string, object>
            {
                { "url", url },
                { "managerId", _productManagerId }
            };

            try
            {
                string dualRoleName = roles.Find(x => x != "student");
                var selectedLicenses = clientLicenses.Licenses.Where(x => userProp.RCLicenseDetails.ManagerLicenseId.Contains(x.Id));
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "AddDualRoleToUser", "Begin adding dual role for user" }, logData: logData);

                CreateRCUser managerUser = new CreateRCUser()
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = emailAddress,
                    ClientId = _clientId,
                    ManagerLicenseIds = selectedLicenses.Select(l => l.Id).Distinct().ToList(),
                    ExternalCustomerId = userLogin.UserId.ToString(),
                    Role = dualRoleName,
                    DualRole = true,
                    Upsert = !string.IsNullOrEmpty(_productManagerId)
                };
                var response = _client.PutAsJsonAsync(url, managerUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    if (jsonContent.Contains("errors"))
                    {
                        logData.Add("error", jsonContent);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "AddDualRoleToUser", "Error adding dual role to user" }, logData: logData);
                        UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                        return $"Error creating user {jsonContent}";
                    }
                    var userResponse = JsonConvert.DeserializeObject<RealConnectUser>(jsonContent);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "AddDualRoleToUser", "Dual role added to user successfully" }, logData: logData);
                    return string.Empty;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "AddDualRoleToUser", "Error adding dual role" }, logData: logData);
                return $"Error creating user {response.Content.ReadAsStringAsync().Result}";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "AddDualRoleToUser", "Error adding dual role in exception" }, logData: logData, exception: ex);
                return $"Error creating user {ex.Message}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assignUserPersonaId"></param>
        /// <returns></returns>
        private string RemoveDualRoleToUser(long assignUserPersonaId)
        {
            string url = $"{_apiEndPoint}/users/bulkRemoveDualRoleManager";
            var logData = new Dictionary<string, object>
            {
                { "url", url },
                { "managerId", _productManagerId }
            };

            try
            {
                var removeDualRole = new BulkRemoveDualRoleManager()
                {
                    UserIds = new List<string> { _productManagerId }
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "RemoveDualRoleToUser", "Begin removing dual role for user" }, logData: logData);

                var response = _client.PutAsJsonAsync(url, removeDualRole).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    if (jsonContent.Contains("errors"))
                    {
                        logData.Add("error", jsonContent);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "RemoveDualRoleToUser", "Error removing dual role to user" }, logData: logData);
                        UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                        return $"Error creating user {jsonContent}";
                    }
                    var userResponse = JsonConvert.DeserializeObject<BulkRemoveDualRoleManagerResponse>(jsonContent);
                    if (userResponse.InvalidUserIds.Count > 0)
                    {
                        logData.Add("InvalidUserIds", userResponse.InvalidUserIds);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "RemoveDualRoleToUser", "Error removing dual role to user" }, logData: logData);
                        return $"Error removing dual role to user {jsonContent}";
                    }
                    _samlRepository.RemoveSamlUserAttributeBySamlAttributeId(assignUserPersonaId, _productId, SamlAttributeEnum.ManagerId);
                    _samlRepository.RemoveSamlUserAttributeBySamlAttributeId(assignUserPersonaId, _productId, SamlAttributeEnum.DualRole);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "RemoveDualRoleToUser", "Dual role removed to user successfully" }, logData: logData);
                    return string.Empty;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "RemoveDualRoleToUser", "Error removing dual role" }, logData: logData);
                return $"Error creating user {response.Content.ReadAsStringAsync().Result}";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "RemoveDualRoleToUser", "Error removing dual role in exception" }, logData: logData, exception: ex);
                return $"Error creating user {ex.Message}";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="learnerUserId"></param>
        /// <param name="roles"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <returns></returns>
        private string TagDualRoleToUser(string learnerUserId, List<string> roles, long assignUserPersonaId)
        {
            string url = $"{_apiEndPoint}/users/{learnerUserId}/makeDualRole";
            var logData = new Dictionary<string, object>
            {
                { "url", url },
                { "userId", learnerUserId }
            };

            try
            {
                string dualRoleName = roles.Find(x => x != "student");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "TagDualRoleToUser", "Begin tagging dual role for user" }, logData: logData);
                RCRole status = new RCRole() { Role = dualRoleName };
                var response = _client.PutAsJsonAsync(url, status).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    if (jsonContent.Contains("errors"))
                    {
                        logData.Add("error", jsonContent);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "TagDualRoleToUser", "Error tagging dual role to user" }, logData: logData);
                        UpdateProductSettingProductStatus(assignUserPersonaId, _productSettingType_ProductStatus, _productId, (int)ProductBatchStatusType.Error);
                        return $"Error creating user {jsonContent}";
                    }
                    var userResponse = JsonConvert.DeserializeObject<RCRoleResponse>(jsonContent);
                    _productManagerId = userResponse.ManagerId.ToString();
                    _samlRepository.CreateSamlUserAttribute(assignUserPersonaId, _productId, SamlAttributeEnum.ManagerId, _productManagerId);
                    _samlRepository.CreateSamlUserAttribute(assignUserPersonaId, _productId, SamlAttributeEnum.DualRole, "true");

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "TagDualRoleToUser", "Dual role tagged to user successfully" }, logData: logData);
                    return string.Empty;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "TagDualRoleToUser", "Error tagging dual role" }, logData: logData);
                return $"Error creating user {response.Content.ReadAsStringAsync().Result}";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "TagDualRoleToUser", "Error tagging dual role in exception" }, logData: logData, exception: ex);
                return $"Error creating user {ex.Message}";
            }
        }

        public async Task<ClientLicenseDetails> GetClientLicenseDetailsCaching(string cursor = "")
        {
            var clientLicensesForPanorama = _cache.GetFromCache<ClientLicenseDetails>
                                            ($"ClientLicenseDetails_Panorama_{_userClaims.OrganizationPartyId}",
                                            1800,
                                            () => { return GetClientLicenseDetails("").Result; }
                                            );
            return clientLicensesForPanorama;
        }

        public Task<ClientLicenseDetails> GetClientLicenseDetailsForPanoramaCached(long orgPartyId)
        {
            string cacheKey = $"ClientLicenseDetails_Panorama_{orgPartyId}";
            var licenseDetails = _distributedCacheService.GetCacheValue<ClientLicenseDetails>(cacheKey);

            if (licenseDetails == null)
            {                
                // Simulate fetching product details from a database
                licenseDetails = GetClientLicenseDetails("").Result;

                // Cache the product details for 10 minutes
                _distributedCacheService.SetCacheValue(cacheKey, licenseDetails, TimeSpan.FromMinutes(_licenseDetailsRedisChacheInMinutes));
            }

            return Task.FromResult(licenseDetails);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        private Task<ClientLicenseDetails> GetClientLicenseDetails(string cursor = "")
        {
            ClientLicenseDetails clientLicenseDetails = GetClientLicenseDetailsPaging(cursor).Result;
            if (clientLicenseDetails.PageInfo.HasMore)
            {
                var clientLicenseDetailsPaging = GetClientLicenseDetails(clientLicenseDetails.PageInfo.Cursor).Result;
                clientLicenseDetails.Licenses.AddRange(clientLicenseDetailsPaging.Licenses);
            }
            return Task.FromResult(clientLicenseDetails);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        private async Task<ClientLicenseDetails> GetClientLicenseDetailsPaging(string cursor = "")
        {
            string url = $"{_apiEndPoint}/clients/{_clientId}/licenses" + (!string.IsNullOrEmpty(cursor) ? "?cursor=" + cursor : "");
            var logData = new Dictionary<string, object>
            {
                { "url", url }
            };

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientLicenseDetails", "Getting client license details" }, logData: logData);
                var response = _client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    if (jsonContent.Contains("errors"))
                    {
                        logData.Add("error", jsonContent);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientLicenseDetails", "Error getting panorama information" }, logData: logData);
                        return null;
                    }
                    var clientLicenseDetails = JsonConvert.DeserializeObject<ClientLicenseDetails>(jsonContent);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientLicenseDetails", "Got client license details" }, logData: logData);
                    //Remove licenses if Ref1 is null

                    clientLicenseDetails.Licenses = clientLicenseDetails.Licenses.Where(x => ref1Data.Contains(x.Ref1)).ToList();
                    return clientLicenseDetails;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientLicenseDetails", "Error getting client license details" }, logData: logData);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientLicenseDetails", "Error getting client license details in exception" }, logData: logData, exception: ex);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<RCClientDetails> GetClientDetails()
        {
            string url = $"{_apiEndPoint}/clients/{_clientId}";
            var logData = new Dictionary<string, object>
            {
                { "url", url }
            };

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientDetails", "Getting client details" }, logData: logData);
                var response = _client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    var clientLicenseDetails = JsonConvert.DeserializeObject<RCClientDetails>(jsonContent);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientDetails", "Got client details" }, logData: logData);
                    return clientLicenseDetails;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientDetails", "Error getting client details" }, logData: logData);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientDetails", "Error getting client details in exception" }, logData: logData, exception: ex);
            }
            return null;
        }

        /// <summary>
        /// Get User Information
        /// </summary>
        /// <param name="userIdentity"></param>
        /// <returns></returns>
        public async Task<RealConnectUser> GetUser(string userIdentity)
        {
            string url = $"{_apiEndPoint}/users/{userIdentity}";
            var logData = new Dictionary<string, object>
            {
                { "url", url }
            };

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUser", "Getting user details" }, logData: logData);
                var response = _client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    var user = JsonConvert.DeserializeObject<RealConnectUser>(jsonContent);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUser", "Got user details" }, logData: logData);
                    return user;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUser", "Error getting user details" }, logData: logData);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUser", "Error getting user details in exception" }, logData: logData, exception: ex);
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="allRoles"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        private ListResponse MergeRolesWithUser(IList<ProductRoleModel.ProductRole> allRoles, long userPersonaId)
        {
            if (allRoles == null)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRolesWithUser", $"Cannot find roles for the product {_productId}, return back" });
                return new ListResponse();
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRolesWithUser", $"Merging roles for the userPersonaId {userPersonaId}" });
            List<string> userRoles = new List<string>();
            var rcUser = GetUser(_productLearnerId).Result;
            userRoles.Add(rcUser.RoleKey);
            if (rcUser.ManagerUserId != null)
            {
                var rcManageUser = GetUser(rcUser.ManagerUserId.ToString()).Result;
                userRoles.Add(rcManageUser.RoleKey);
            }

            foreach (var role in userRoles)
            {
                if (allRoles.Any(a => a.Alias == role))
                {
                    allRoles.FirstOrDefault(a => a.Alias == role).IsAssigned = true;
                }
            }


            foreach (var role in allRoles)
            {
                if (role.Alias == "student") { role.SortId = 1; }
                else if (role.Alias == "sublicense-manager") { role.SortId = 2; }
                else if (role.Alias == "customer-reporting-only") { role.SortId = 3; }
                else if (role.Alias == "customer-admin") { role.SortId = 4; }
                else { role.SortId = 5; }
            }
            allRoles = allRoles.OrderBy(o => o.SortId).ToList();

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetClientIdFromUDM()
        {
            var greenbookCaresCheckRequired = _productInternalSettingList.FirstOrDefault(s => s.Name.Equals("IsGreenbookCaresCheckRequired", StringComparison.OrdinalIgnoreCase))?.Value;
            bool isGreenbookCaresCheckRequired = greenbookCaresCheckRequired != null && greenbookCaresCheckRequired != "0";

            if (isGreenbookCaresCheckRequired)
            {
                IList<GbProductMap> allProducts = _productRepository.ListProducts(null, null, null, null).ToList();
                var productDetails = allProducts.FirstOrDefault(x => x.ProductId == _productId);
                //string udmSource = productDetails.UDMSourceCode?.Length > 0 ? productDetails.UDMSourceCode : productDetails.BooksProductCode;
                var booksCompanyInstance = _blueBook.GetCompanyInstanceByUPFMCompanyId(_userClaims.OrganizationRealPageGuid.ToString().ToLower());
                int customerCompanyId = booksCompanyInstance?.Attributes?.CustomerCompanyMap.FirstOrDefault()?.CustomerCompanyId ?? 0;
                string domain = booksCompanyInstance?.Attributes?.Domain;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientIdFromUDM", $"Got BooksProductCode : {productDetails.BooksProductCode} and customerCompanyId - {customerCompanyId}" });

                if (!string.IsNullOrEmpty(domain) && customerCompanyId != 0)
                {
                    var booksCustomerCompanyMap = _blueBook.GetCustomerCompanyMapByCustomerCompanyId(customerCompanyId, domain);
                    var rcCompanyInstance = booksCustomerCompanyMap?.Find(p => p.Source == productDetails.UDMSourceCode);

                    if (rcCompanyInstance != null)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientIdFromUDM", $"Found Company instance for RC : {productDetails.BooksProductCode} and ClientId - {rcCompanyInstance.CompanyInstanceSourceId}" });
                        return rcCompanyInstance.CompanyInstanceSourceId;
                    }
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetClientIdFromUDM", $"Company instance not found for RC : {productDetails.BooksProductCode} and ClientId - {_userClaims.OrganizationRealPageGuid}" });
                    return string.Empty;

                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Used to assign courses and learningPaths to user in bulk
        /// </summary>
        /// <returns></returns>
        private string BulkContentAssignment(string learnerId, ClientLicenseDetails clientDetails, List<License> selectedLicenses)
        {
            if (clientDetails is null && !clientDetails.LearningPathIds.Any())
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"No Learning path for the client {_client}" });
                return $"No Learning path for the client {_client}";
            }
            if (selectedLicenses is null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"No Licenses to assign for the user {learnerId}" });
                return $"No Licenses to assign for the user {learnerId}";
            }

            var selectedLearningPaths = clientDetails.Licenses.Where(c => selectedLicenses.Select(s => s.Id).Contains(c.Id)).SelectMany(d => d.LearningPathIds).Distinct().ToList();

            var logData = new Dictionary<string, object>();
            string url = $"{_apiEndPoint}/users/bulkContentAssignment";
            logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
            BulkContentAssignment bulkContent = new BulkContentAssignment()
            {
                Id = learnerId,
                LearningPathIds = selectedLearningPaths
            };

            var bulkContentObject = new BulkAssignContent();
            bulkContentObject.Users.Add(bulkContent);

            logData.Add("bulkContentObject", bulkContentObject);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"Calling bulk update for learning path assignments for email: {learnerId}," }, logData: logData);

            try
            {
                var response = _client.PostAsJsonAsync(url, bulkContentObject).Result;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"Called bulk update api for email {learnerId}" }, logData: logData);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    var bulkResponse = JsonConvert.DeserializeObject<BulkContentAssignmentResponse>(jsonContent);
                    if (bulkResponse.Errors.Count > 0)
                    {
                        logData.Add("BulkError", bulkResponse);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"Unable to assign bulk content for user {learnerId}" }, logData: logData);
                        return "Unable to assign bulk content";
                    }
                    else
                    {
                        if (selectedLearningPaths.Count == 0)
                        {
                            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"No LearningPaths, unassigning learning paths for user {learnerId}" });
                            return "";
                        }
                        else
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"Assigned learning paths successfully for user {learnerId}" }, logData: logData);
                            return "";
                        }
                    }
                }
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"Unable to assign bulk content for user {learnerId} as response is not success" }, logData: logData);
                return "Unable to assign bulk content - status is error";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "BulkContentAssignment", $"Error assigning bulk content for user {learnerId}" }, logData: logData, exception: ex);
                return "Error assigning Bulk content";
            }
        }

        private string FormattedEmail(string email, long personaId, Guid realPageId)
        {
            bool isValidEmail = new EmailAddressAttribute().IsValid(email);
            string emailResult;
            bool isUserNoemail = IsRegularUserNoEmail(personaId);

            if (isUserNoemail)
            {
                IList<CommonAddress> contactMechansimList = _manageContactMechanism.ListContactMechanismForPerson(realPageId, null);
                if (contactMechansimList.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true
                    && a.contactMechanismUsageType?.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true))
                {
                    string notificationEmail = contactMechansimList.FirstOrDefault(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true
                    && a.contactMechanismUsageType?.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true).AddressString;

                    if (!string.IsNullOrEmpty(notificationEmail) && new EmailAddressAttribute().IsValid(notificationEmail))
                    {
                        //User is no email type with valid notification email
                        var splitResult = notificationEmail.Split('@');
                        emailResult = $"{splitResult[0]}+{personaId}@{splitResult[1]}";
                    }
                    else
                    {
                        //User is no email type with no notification email
                        emailResult = $"{email}+{personaId}@bogusemail.com";
                    }
                }
                else
                {
                    if (isValidEmail)
                    {
                        var splitResult = email.Split('@');
                        emailResult = $"{splitResult[0]}+{personaId}@{splitResult[1]}";
                    }
                    else
                    {
                        emailResult = $"{email}+{personaId}@bogusemail.com";
                    }
                }
            }
            else
            {
                var splitResult = email.Split('@');
                emailResult = $"{splitResult[0]}+{personaId}@{splitResult[1]}";
            }

            return emailResult.ToLower();
        }

        private string GetApiKeyForPanoramaFromSettings()
        {
            //Get the API Key from the settings and cache for 3 min
            var settings = _cache.GetFromCache<InternalSettingResponse>($"PanoramaKey_{_userClaims.OrganizationRealPageGuid}", 180, () =>
            {
                return _manageUnifiedSettings.GetCompanyInternalSettings(_userClaims.OrganizationRealPageGuid, "UPFM", "LMSAPIKey");
            });

            return settings.Keys.FirstOrDefault()?.Value;
        }

        private LearningPathsContent GetLearningPathsForPanoramaCached(long orgPartyId)
        {
            string cacheKey = $"LearningPaths_{orgPartyId}";
            var lp = _distributedCacheService.GetCacheValue<LearningPathsContent>(cacheKey);

            if (lp == null)
            {
                // Simulate fetching product details from a database
                lp = GetLearningPathsForPanorama("");

                // Cache the product details for 10 minutes
                _distributedCacheService.SetCacheValue(cacheKey, lp, TimeSpan.FromMinutes(_learningPathRedisChacheInMinutes));
            }

            return lp;
        }

        private LearningPathsContent GetLearningPathsForPanorama(string cursor = "")
        {
            //based on the key get the learning paths
            LearningPathsContent lpContent = GetLearningPathsForPanoramaByPaging(cursor);
            if (lpContent.PageInfo.HasMore)
            {
                var lpContentPaging = GetLearningPathsForPanorama(lpContent.PageInfo.Cursor);
                lpContent.ContentItems.AddRange(lpContentPaging.ContentItems);
            }
            return lpContent;
        }

        private LearningPathsContent GetLearningPathsForPanoramaByPaging(string cursor = "")
        {
            //based on the key get the learning paths
            string url = $"{_apiEndPoint}/content" + (!string.IsNullOrEmpty(cursor) ? "?cursor=" + cursor : "");

            var logData = new Dictionary<string, object>
            {
                { "url", url }
            };

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetLearningPathsForPanoramaByPaging", "Getting learning path details" }, logData: logData);

                var response = lpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    if (jsonContent.Contains("errors"))
                    {
                        logData.Add("error", jsonContent);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetLearningPathsForPanoramaByPaging", "Error getting learning path information" }, logData: logData);
                        return null;
                    }
                    var lpDetails = JsonConvert.DeserializeObject<LearningPathsContent>(jsonContent);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetLearningPathsForPanoramaByPaging", "Got learning path details" }, logData: logData);

                    return lpDetails;
                }
                logData.Add("Error", response.Content.ReadAsStringAsync().Result);
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetLearningPathsForPanoramaByPaging", "Error getting learning path details" }, logData: logData);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetLearningPathsForPanoramaByPaging", "Error getting learning path details in exception" }, logData: logData, exception: ex);
            }
            return null;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRateLimitPolicy()
        {
            //return Policy
            //    .RateLimitAsync<HttpResponseMessage>(1, TimeSpan.FromSeconds(1)); // Allow 1 request per second

            return Policy
               .HandleResult<HttpResponseMessage>(msg => msg.StatusCode == (System.Net.HttpStatusCode)429)
               .WaitAndRetryAsync(
                   retryCount: 3,
                   sleepDurationProvider: (retryAttempt, response, context) =>
                   {
                       if (response.Result.Headers.TryGetValues("X-RateLimit-Reset", out var values))
                       {
                           var retryAfter = values.First();
                           if (int.TryParse(retryAfter, out int seconds))
                           {
                               return TimeSpan.FromSeconds(seconds);
                           }
                       }
                       return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                   },
                   onRetryAsync: async (outcome, timespan, retryAttempt, context) =>
                   {
                       Console.WriteLine($"Retrying in {timespan.TotalSeconds} seconds... (Attempt {retryAttempt})");
                       await Task.CompletedTask;
                   });
        }
        #endregion
    }
}
