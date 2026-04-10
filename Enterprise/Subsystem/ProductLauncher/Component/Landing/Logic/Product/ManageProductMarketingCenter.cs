using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using MC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using Right = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter.Right;

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

        private const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
        private const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
        private const string ROLE_ASSIGN = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
        private const string ROLE_UNASSIGN = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";
		private const string PRODUCT_ROLE_CREATE = "{\"action\":\"Created Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLE_UPDATE = "{\"action\":\"Updated Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLE_DELETE = "{\"action\":\"Deleted Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLENAME_UPDATE = "{\"action\":\"Updated Role Name\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLEDESCRIPTION_UPDATE = "{\"action\":\"Updated Role Description\",\"value\":\"RoleName\"}";
        private const int MAX_USERNAME_ATTEMPTS = 100;
        #region Ctor
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="userClaims">The RealPageId of the editor</param>
        public ManageProductMarketingCenter(DefaultUserClaim userClaims)
           : base((int)ProductEnum.MarketingCenter, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new Logic.ManageBlueBook(userClaims);
            _userClaims = userClaims;

            _productUrl = GetRequiredProductSetting("APIENDPOINT");
            _marketingCenterApiSourceID = GetRequiredProductSetting("MarketingCenterApiSourceID");
            _username = Encoding.UTF8.GetString(Convert.FromBase64String(GetRequiredProductSetting("APIUSERNAME")));
            _password = Encoding.UTF8.GetString(Convert.FromBase64String(GetRequiredProductSetting("APIPASSWORD")));

            _client.BaseAddress = new Uri(_productUrl);
            _client.SetBasicAuthentication(_username, _password);

            var credCache = new CredentialCache();
            credCache.Add(new Uri(_productUrl), "Digest", new NetworkCredential(_username, _password));
            var httpHandler = new HttpClientHandler { Credentials = credCache };

            _httpClient = new HttpClient(httpHandler);
            _httpClient.BaseAddress = new Uri(_productUrl);
            _httpClient.SetBasicAuthentication(_username, _password);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Unit-test constructor – allows injection of a mock HttpMessageHandler.
        /// </summary>
        public ManageProductMarketingCenter(
            Guid editorRealPageId,
            DefaultUserClaim userClaims,
            HttpMessageHandler httpMessageHandler,
            IProductInternalSettingRepository productInternalSettingRepository,
            IManagePersona managePersona,
            ISamlRepository samlRepository,
            IManageBlueBook manageBlueBook,
            IProductRepository productRepository,
            IRepository repository)
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

            _productUrl = GetRequiredProductSetting("APIENDPOINT");
            _marketingCenterApiSourceID = GetRequiredProductSetting("MARKETINGCENTERAPISOURCEID");
            _username = Encoding.UTF8.GetString(Convert.FromBase64String(GetRequiredProductSetting("APIUSERNAME")));
            _password = Encoding.UTF8.GetString(Convert.FromBase64String(GetRequiredProductSetting("APIPASSWORD")));

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
            var result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (result.IsError) return result;
            var logData = new Dictionary<string, object>();

            try
            {
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                string marketingCompanyId = company.CompanyInstanceSourceId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"company source id={marketingCompanyId}" });

                var url = _productUrl + $"/external/company/{marketingCompanyId}/contact/roles";
                logData = new Dictionary<string, object> { { "url", url } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRoles", "GET url" });

                var response = _httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var rolesList = JsonConvert.DeserializeObject<IList<MC.Role>>(json) ?? new List<MC.Role>();

                    IList<ProductRole> list = rolesList.ToGBRoles() ?? new List<ProductRole>();

                    if (userPersonaId != 0)
                    {
                        MC.MarketingCenterUserDetails mUser = GetUserDetails();
                        if (mUser == null)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"User not found. userPersonaId={userPersonaId}" });
                            return new ListResponse { IsError = true, ErrorReason = "User not found" };
                        }

                        var matchingRole = list.FirstOrDefault(a => a.ID == mUser.ContactRoleId.ToString());
                        if (matchingRole != null)
                        {
                            matchingRole.IsAssigned = true;
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Assigned role: {matchingRole.Name}" });
                        }
                    }

                    return new ListResponse
                    {
                        Records = list.Cast<object>().ToList(),
                        TotalRows = list.Count,
                        RowsPerPage = list.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty
                    };
                }

                result.IsError = true;
                result.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"HTTP error: {response.Content.ReadAsStringAsync().Result}" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRoles", ex.Message });
                result = new ListResponse { IsError = true };
                result.ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.RoleErrorMessage;
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
            var result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (result.IsError) return result;

            var logData = new Dictionary<string, object>();
            var allProperties = new Dictionary<string, bool>();

            try
            {
                string marketingCenterCompanyId = string.Empty;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Calling BlueBook.GetCompanyMap" });
                IList<CustomerCompanyMap> companyMap = _blueBook.GetCompanyMap(
                    _editorPersona.Organization.RealPageId,
                    _editorPersona.Organization.BooksCustomerMasterId,
                    source: BlueBookProductConstants.MarketingCenter,
                    domain: _editorPersona.Organization.OrganizationDomain.Name);

                marketingCenterCompanyId = companyMap?
                    .FirstOrDefault(a => a.Source.Equals(BlueBookProductConstants.MarketingCenter, StringComparison.OrdinalIgnoreCase))
                    ?.CompanyInstanceSourceId ?? string.Empty;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"PMC ID={marketingCenterCompanyId}" });

                // Fix: removed accidental whitespace from original URL template
                var url = _productUrl + $"/external/properties?companyId={marketingCenterCompanyId}";
                logData = new Dictionary<string, object> { { "url", url } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetProperties", "GET url" });

                var response = _httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var propertyList = JsonConvert.DeserializeObject<IList<ProductPropertyMap>>(json) ?? new List<ProductPropertyMap>();
                    IList<ProductProperty> list = propertyList.ToGBProperties() ?? new List<ProductProperty>();

                    if (userPersonaId != 0)
                    {
                        MarketingCenterUserDetails mUser = GetUserDetails();
                        if (mUser == null)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"User not found. userPersonaId={userPersonaId}" });
                            return new ListResponse { IsError = true, ErrorReason = "User not found" };
                        }

                        if (mUser.AssignedProperties != null)
                        {
                            int i = 0;
                            logData = new Dictionary<string, object>();
                            foreach (MC.Property p in mUser.AssignedProperties)
                            {
                                ProductProperty existing = list.FirstOrDefault(a => a.ID == p.Id.ToString());
                                if (existing != null)
                                {
                                    existing.IsAssigned = true;
                                }
                                else
                                {
                                    list.Add(new ProductProperty
                                    {
                                        Name = p.Name,
                                        ID = p.Id.ToString(),
                                        IsAssigned = p.Active,
                                        State = p.Address.StateCode,
                                        Street1 = p.Address.Address1,
                                        City = p.Address.CityName,
                                        Zip = p.Address.PostalCode
                                    });
                                    logData[$"ExtraProperty{i++}"] = p.Name;
                                }
                            }
                            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetProperties", "Extra properties added" });
                        }

                        allProperties["IsAssignedNewPropertyByDefault"] = mUser.AssignNewProperty;
                    }

                    return new ListResponse
                    {
                        Records = list.Cast<object>().ToList(),
                        TotalRows = list.Count,
                        RowsPerPage = list.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty,
                        Additional = allProperties
                    };
                }

                result.IsError = true;
                result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"HTTP error: {response.Content.ReadAsStringAsync().Result}" });
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorReason = ex is BlueBookException ? ex.Message : CommonMessageConstants.PropertyErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetProperties", ex.Message });
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

            // FIX: TryParse instead of Convert.ToInt64 to prevent "Input string was not in a
            //      correct format" when _editorProductUserId is null or non-numeric.
            bool editorIdValid = !string.IsNullOrWhiteSpace(_editorProductUserId)
                                 && long.TryParse(_editorProductUserId, out long editorProductUserIdParsed)
                                 && IsUserIdValid(editorProductUserIdParsed);

            if (!editorIdValid)
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
                if (listResponse.IsError) return listResponse.ErrorReason;

                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "Begin" });

                // Check existence in MC before making expensive person/login lookups
                MarketingCenterUserDetails mUser = GetUserDetails();
                if (mUser == null)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"User not found in product. userPersonaId={userPersonaId}" });
                    return "User not found in product";
                }

                Persona userPersona = _managePersona.GetPersona(userPersonaId);
                Guid realPageId = userPersona.RealPageId;
                Person person = _managePerson.GetPerson(realPageId);

                UserLoginOnly userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
                IList<UserOrganization> orgList = _manageUserLogin.GetUserPersonaOrganization(userLogin.LoginName);
                bool isRegularUserNoEmail = IsRegularUserNoEmail(userPersonaId);

                string userEmailAddress = string.Empty;
                string userLeadEmailAddress = string.Empty;

                var addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
                userEmailAddress = addresses?
                    .Where(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.AddressString)
                    .FirstOrDefault() ?? string.Empty;

                if (string.IsNullOrEmpty(userEmailAddress))
                    userEmailAddress = userLogin.LoginName;

                if (isRegularUserNoEmail)
                    userLeadEmailAddress = userEmailAddress;

                userEmailAddress = userPersona.UserTypeId == (int)UserTypeConstants.RegularUserNoEmail
                    ? _productUsername
                    : ValidateAndReturnEmailAddress(userEmailAddress);

                string productLoginName = _productUsername;
                if (orgList.Any(o => o.PrimaryOrganization == true && o.OrganizationPartyId == userPersona.OrganizationPartyId)
                    && !_productUsername.Equals(userEmailAddress, StringComparison.OrdinalIgnoreCase))
                {
                    productLoginName = userEmailAddress;
                }

                var mcUser = new MC.MarketingCenterUser
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
                logData = new Dictionary<string, object> { { "url", url }, { "mcUser", mcUser } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", "PUT profile" });

                var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;
                if (response.IsSuccessStatusCode)
                {
                    UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
                    WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
                    return string.Empty;
                }

                // FIX: Detect duplicate-email 500 from MC and stop the batch instead of returning an error string
                string errorContent = string.Empty;
                try { errorContent = response.Content.ReadAsStringAsync().Result; } catch { /*Ignored*/ }

                logData["errorContent"] = errorContent;
                WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUserProfile", $"PUT failed for userPersonaId={userPersonaId}" });

                if (IsDuplicateEmailError(errorContent))
                {
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                        "An error occurred when {3} {4} attempted to update {2} for {0} {1}. A user already exists with this email address.");
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
                    return ProductBatchStatusType.Stop.ToString();
                }

                return $"There was a problem updating user profile for userPersonaId={userPersonaId}. Error: {errorContent}";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateUserProfile", ex.Message });
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
        public string ManageMarketingCenterUser(long editorPersonaId, long userPersonaId,
            List<int> RoleList, List<string> PropertyList, bool IsAssignedNewPropertyByDefault,
            out List<AdditionalParameters> additionalParameters)
        {
            var logData = new Dictionary<string, object>();
            var mcProperties = new List<int>();
            additionalParameters = new List<AdditionalParameters>();

            ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) return listResponse.ErrorReason;

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Begin" });

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid realPageId = userPersona.RealPageId;
            IC.IPerson person = _managePerson.GetPerson(realPageId);

            IUserLoginOnly userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(realPageId, null);
            userPersona.Organization = organizationList.FirstOrDefault(i => i.PartyId == userPersona.OrganizationPartyId);

            var personaOrganization = userPersona.Organization;
            bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase)
                               && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);

            // Resolve email address
            string userEmailAddress = string.Empty;
            string userLeadEmailAddress = string.Empty;

            var addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
            userEmailAddress = addresses?
                .Where(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.AddressString)
                .FirstOrDefault() ?? string.Empty;

            if (IsRegularUserNoEmail(userPersonaId))
            {
                userLeadEmailAddress = userEmailAddress;
                if (string.IsNullOrEmpty(userLeadEmailAddress))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "No notification email" });
                    return "ManageMarketingCenterUser - Error. No Valid Notification Email Provided";
                }
                userEmailAddress = _productUsername;
                if (string.IsNullOrEmpty(userEmailAddress))
                    userEmailAddress = new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(userLogin.LoginName)
                        ? userLogin.LoginName
                        : userLogin.LoginName + "@NoReply.com";
            }
            else if (string.IsNullOrEmpty(userEmailAddress))
            {
                userEmailAddress = userLogin.LoginName;
            }

            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

            if (!string.IsNullOrEmpty(_productUsername))
                userEmailAddress = _productUsername;

            bool isSuperUser = IsSuperUser(userPersonaId);

            // FIX: Guard empty RoleList/PropertyList for non-super users
            if (!isSuperUser)
            {
                bool missingRoles = RoleList == null || RoleList.Count == 0;
                bool missingProperties = PropertyList == null || PropertyList.Count == 0;

                if (missingRoles)
                {
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                        "An error occurred when {3} {4} attempted to provision {2} for {0} {1}." +
                        "There are no roles active in this company. Please contact the implementation team for this product.");
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
                }

                if (missingProperties)
                {
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                        "An error occurred when {3} {4} attempted to provision {2} for {0} {1}." +
                        "There are no properties active in this company. Please contact the implementation team for this product.");
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
                }

                if (missingRoles || missingProperties)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] {
                        "ManageMarketingCenterUser",
                        $"Stop. RoleList.Count={RoleList?.Count ?? 0}, PropertyList.Count={PropertyList?.Count ?? 0}" });
                    return ProductBatchStatusType.Stop.ToString();
                }
            }

            // FIX: Guard against Records containing Persona objects when GetRoles/GetProperties returns an error
            ListResponse roleListResponse = GetRoles(editorPersonaId, 0, null);
            List<ProductRole> roleList = (roleListResponse != null && !roleListResponse.IsError
                ? roleListResponse.Records ?? new List<object>()
                : new List<object>()).Cast<ProductRole>().ToList();

            ListResponse propertyListResponse = GetProperties(editorPersonaId, 0, null);
            List<ProductProperty> propertyList = (propertyListResponse != null && !propertyListResponse.IsError
                ? propertyListResponse.Records ?? new List<object>()
                : new List<object>()).Cast<ProductProperty>().ToList();

            bool allPropertiesSelected = false;
            var productUserBeforeUpdate = GetUserDetails();

            int roleId = 0;

            if (isSuperUser)
            {
                // Super user gets the Corporate Operations role
                ProductRole corpOpsRole = roleList.FirstOrDefault(a => a.Name.Equals("CORPORATE OPERATIONS", StringComparison.OrdinalIgnoreCase));
                if (corpOpsRole != null && int.TryParse(corpOpsRole.ID, out int parsedRoleId))
                    roleId = parsedRoleId;

                if (roleId == 0)
                {
                    logData = new Dictionary<string, object> { { "roleList", roleList } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "No Corporate Operations role" });
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                        "An error occurred when {3} {4} attempted to provision {2} for {0} {1}." +
                        "There is no Corporate Operations role active in this company." +
                        "Please contact the implementation team for this product.");
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
                    return ProductBatchStatusType.Stop.ToString();
                }

                // FIX: "No Product Properties are found for Enterprise Role" – guard empty propertyList
                if (propertyList == null || propertyList.Count == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "No Product Properties found for super user" });
                    WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                        "An error occurred when {3} {4} attempted to provision {2} for {0} {1}." +
                        "There are no properties active in this company. Please contact the implementation team for this product.");
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
                    return ProductBatchStatusType.Stop.ToString();
                }

                foreach (ProductProperty prop in propertyList)
                {
                    if (int.TryParse(prop.ID, out int pid))
                        mcProperties.Add(pid);
                }
            }
            else
            {
                ProductRole matchedRole = roleList.FirstOrDefault(a => a.ID == RoleList[0].ToString());
                if (matchedRole != null)
                {
                    roleId = RoleList[0];
                }
                else
                {
                    logData = new Dictionary<string, object> { { "roleList", roleList }, { "requestedRoleId", RoleList[0] } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", $"Role {RoleList[0]} not in MC company roles" });
                    return $"Role id {RoleList[0]} not found";
                }

                // FIX: Use int.TryParse to avoid "Input string was not in a correct format"
                foreach (var prop in PropertyList)
                {
                    if (!string.IsNullOrWhiteSpace(prop) && int.TryParse(prop, out int propId))
                        mcProperties.Add(propId);
                    else
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Invalid property value '{prop}' skipped." });
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"_productUsername={_productUsername}" });

            CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
            if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", "Company id missing in BlueBook" });
                return "Company Setup Error: Please Contact Support.";
            }

            if (string.IsNullOrEmpty(_productUsername))
            {
                if (!IsRegularUserNoEmail(userPersonaId))
                    userLeadEmailAddress = userLogin.LoginName;

                userEmailAddress = GetMCUniqueUserName(person.FirstName, person.LastName);
                if (string.IsNullOrEmpty(userEmailAddress))
                    return "An error occurred. Unable to get username.";
            }

            if (!int.TryParse(company.CompanyInstanceSourceId, out int companyId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageMarketingCenterUser", $"Invalid CompanyInstanceSourceId: {company.CompanyInstanceSourceId}" });
                return "Company Setup Error: Please Contact Support.";
            }

            var mcUser = new MC.MarketingCenterUser
            {
                CompanyId = companyId,
                ContactRoleId = roleId,
                ContactRoleName = null,
                FirstName = person.FirstName,
                LastName = person.LastName,
                EmailAddress = userEmailAddress,
                LeadEmailAddress = userLeadEmailAddress,
                WelcomeEmailSent = true,
                AssignUnassignProperties = true,
                AssignPropertyIds = mcProperties,
                AssignNewProperty = IsAssignedNewPropertyByDefault
            };

            if (isSuperUser)
            {
                allPropertiesSelected = true;
                mcUser.AssignNewProperty = true;
            }

            logData = new Dictionary<string, object> { { "mcUser", mcUser } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ManageMarketingCenterUser", "User payload" });

            if (string.IsNullOrEmpty(_productUsername))
            {
                // ---- CREATE ----
                return CreateMarketingCenterUser(editorPersonaId, userPersonaId, mcUser,
                    allPropertiesSelected, userEmailAddress, userLeadEmailAddress, roleList, propertyList,
                    productUserBeforeUpdate, ref additionalParameters);
            }
            else
            {
                // ---- UPDATE ----
                return UpdateMarketingCenterUser(editorPersonaId, userPersonaId, mcUser,
                    allPropertiesSelected, isExternalUser, userEmailAddress, roleList, propertyList,
                    productUserBeforeUpdate, ref additionalParameters);
            }
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

            ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError) return false;

            // FIX: TryParse instead of Convert.ToInt32
            if (!int.TryParse(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId, out int companyInstanceSourceId)
                || companyInstanceSourceId == 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Invalid company id for editorPersonaId={editorPersonaId}" });
                return false;
            }

            try
            {
                return SetMarketingCenterUserStatus(isActive, _productUserId);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeUserStatus", $"Failed for {companyInstanceSourceId}|{username}. {ex.Message}" });
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
                var roleName = (roles.Records ?? new List<object>()).Cast<ProductRole>().FirstOrDefault(r => r.ID == roleId.ToString())?.Name;
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
                var parsedRoleIds = roleList
                    .Where(r => int.TryParse(r, out _))
                    .Select(int.Parse)
                    .ToList();
                var result = _httpClient.PutAsJsonAsync(url, parsedRoleIds).Result;
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
                var existingRoles = (roles.Records ?? new List<object>()).Cast<ProductRole>().ToList();
				var roleName = existingRoles.FirstOrDefault(r => r.ID == mcRole.Id.ToString())?.Name;
				var roleDescription = existingRoles.FirstOrDefault(r => r.ID == mcRole.Id.ToString())?.Description;

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

                    if (mcRight.IsAssigned)
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
				var rightList = (rights.Records ?? new List<object>()).Cast<MCRight>().ToList();
                var roles = GetRoles(editorPersonaId, 0, null);
				if (string.IsNullOrEmpty(roleName))
				{
                    roleName = (roles.Records ?? new List<object>()).Cast<ProductRole>().FirstOrDefault(r => r.ID == roleId.ToString())?.Name;
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
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRightsToRoleLogMessage", $"Error building activity log. editorPersonaId={editorPersonaId}, roleId={roleId}. {ex.Message}" });
            }
        }

        public void UpdateRolesToRightLogMessage(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove)
        {
			try
			{
                ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
                UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

                var roles = GetRoles(editorPersonaId, 0, null);
				var roleList = (roles.Records ?? new List<object>()).Cast<ProductRole>().ToList();
                var rights = GetRightsDetails(editorPersonaId);
				var rightName = (rights.Records ?? new List<object>()).Cast<MCRight>().FirstOrDefault(r => r.RightId.ToString() == rightId.ToString())?.Description;
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
				WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRolesToRightLogMessage", $"Error building activity log. editorPersonaId: {editorPersonaId}, rightId: {rightId}. {ex.Message}" });
            }
        }




        public void AddUpdateRoleLogMessage(long editorPersonaId, string roleName, string product, string oldRoleName, string AttributeName)
        {
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

            var additionalParameters = new List<AdditionalParameters>();
            string message;
            if (AttributeName.Equals("RoleName", StringComparison.OrdinalIgnoreCase))
            {
                message = impersonatorUserInfo != null
                    ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Updated Role Name of {oldRoleName} to {roleName} in {product}."
                    : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Updated Role Name of {oldRoleName} to {roleName} in {product}.";
                additionalParameters.Add(new AdditionalParameters { Key = oldRoleName, Value = PRODUCT_ROLENAME_UPDATE.Replace("RoleName", roleName) });
            }
            else if (AttributeName.Equals("RoleDescription", StringComparison.OrdinalIgnoreCase))
            {
                message = impersonatorUserInfo != null
                    ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Updated Role Description of {oldRoleName} to {roleName} in {product}."
                    : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Updated Role Description of {oldRoleName} to {roleName} in {product}.";
                additionalParameters.Add(new AdditionalParameters { Key = oldRoleName, Value = PRODUCT_ROLEDESCRIPTION_UPDATE.Replace("RoleName", roleName) });
            }
            else
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "AddUpdateRoleLogMessage", $"Unknown AttributeName '{AttributeName}' — activity log skipped." });
                return;
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
                    var res = JsonConvert.DeserializeObject<IList<Right>>(jsonContent) ?? new List<Right>();
                    rightList = res.ToGBRights() ?? new List<MCRight>();
                    logData = new Dictionary<string, object>
                    {
                        { "rightList", rightList }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsForRoleId", "Got rights" });

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
                logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesForRightId", "Getting roles" });

                var apiResponse = _httpClient.GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    roleList = JsonConvert.DeserializeObject<IList<RolesRightsAccessRight>>(jsonContent) ?? new List<RolesRightsAccessRight>();
                    logData = new Dictionary<string, object>
                    {
                        { "rightList", roleList }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesForRightId", "Got roles" });

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
                var url = _productUrl + $"/external/company/{marketingCompanyId}/roles";
                var logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesCountDetails", "Getting roles" });

                var apiResponse = _httpClient.GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rolesList = JsonConvert.DeserializeObject<IList<RolesRightsAccessRight>>(jsonContent) ?? new List<RolesRightsAccessRight>();
                    logData = new Dictionary<string, object>
                    {
                        { "rolesList", rolesList }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRolesCountDetails", "Got roles" });

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
                var logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsDetails", "Getting rights" });

                var apiResponse = _httpClient.GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rights = JsonConvert.DeserializeObject<List<Right>>(jsonContent) ?? new List<Right>();
                    mcRights = rights.ToGBRights() ?? new List<MCRight>();
                    logData = new Dictionary<string, object>
                    {
                        { "rightGroup", rights }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetRightsDetails", "Got rights" });

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
        /// <summary>
        /// Handles the POST (create) path for ManageMarketingCenterUser.
        /// </summary>
        private string CreateMarketingCenterUser(
            long editorPersonaId, long userPersonaId,
            MC.MarketingCenterUser mcUser, bool allPropertiesSelected,
            string userEmailAddress, string userLeadEmailAddress,
            List<ProductRole> roleList, List<ProductProperty> propertyList,
            MC.MarketingCenterUserDetails productUserBeforeUpdate,
            ref List<AdditionalParameters> additionalParameters)
        {
            try
            {
                mcUser.AssignAllProperties = allPropertiesSelected;
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);

                string sourceId = string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId;
                var url = _productUrl + $"/external/contact?sourceid={sourceId}";
                var logData = new Dictionary<string, object> { { "url", url }, { "userJson", JsonConvert.SerializeObject(mcUser) } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "CreateMarketingCenterUser", "POST" });

                var response = _httpClient.PostAsJsonAsync(url, mcUser).Result;
                if (response.IsSuccessStatusCode)
                {
                    var userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                    long newId = userResult.id;

                    _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, userEmailAddress);
                    _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newId.ToString());
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

                    var updateResponse = UpdateUsersMigrationStatus(editorPersonaId, new List<MigrateUser>
                    {
                        new MigrateUser
                        {
                            UnifiedLoginUserName = userEmailAddress,
                            UserId               = newId.ToString(),
                            UsingUnifiedLogin    = true,
                            LeadEmailAddress     = userLeadEmailAddress
                        }
                    });

                    if (!updateResponse.Status)
                        return updateResponse.Message;
                }
                else
                {
                    var errResult = response.Content.ReadAsStringAsync().Result;
                    WriteToErrorLog("{ActionName} - {state}", new Dictionary<string, object> { { "errResult", errResult } },
                        messageProperties: new object[] { "CreateMarketingCenterUser", "POST failed" });
                    return ParseErrorPosting(response, "Create", editorPersonaId, userPersonaId);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CreateMarketingCenterUser", ex.Message });
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                return "There was a problem creating the user";
            }

            AppendActivityLogs(mcUser, productUserBeforeUpdate, roleList, propertyList, editorPersonaId, userPersonaId, ref additionalParameters);
            return string.Empty;
        }

        /// <summary>
        /// Handles the PUT (update) path for ManageMarketingCenterUser.
        /// </summary>
        private string UpdateMarketingCenterUser(
            long editorPersonaId, long userPersonaId,
            MC.MarketingCenterUser mcUser, bool allPropertiesSelected,
            bool isExternalUser, string userEmailAddress,
            List<ProductRole> roleList, List<ProductProperty> propertyList,
            MC.MarketingCenterUserDetails productUserBeforeUpdate,
            ref List<AdditionalParameters> additionalParameters)
        {
            try
            {
                bool isSuperUser = IsSuperUser(userPersonaId);

                if (!isSuperUser)
                {
                    // FIX: Null-safe cast for currentPropertyList
                    ListResponse currentPropResponse = GetProperties(editorPersonaId, userPersonaId, null);
                    List<ProductProperty> currentPropertyList =
                        (currentPropResponse?.Records ?? new List<object>()).Cast<ProductProperty>().ToList();

                    var removePropertyList = new List<int>();
                    foreach (ProductProperty pp in currentPropertyList.Where(p => p.IsAssigned.HasValue && p.IsAssigned.Value))
                    {
                        int ppId = 0;
                        if (!int.TryParse(pp.ID, out ppId)) continue;

                        if (mcUser.AssignPropertyIds != null && mcUser.AssignPropertyIds.Contains(ppId))
                            mcUser.AssignPropertyIds.Remove(ppId);
                        else
                            removePropertyList.Add(ppId);
                    }
                    if (removePropertyList.Any())
                        mcUser.UnassignPropertyIds = removePropertyList;

                    mcUser.AssignAllProperties = allPropertiesSelected;
                }

                if (isExternalUser)
                {
                    mcUser.EmailAddress = _productUsername;
                    mcUser.LeadEmailAddress = userEmailAddress;
                }

                string sourceId = string.IsNullOrEmpty(_editorProductUserId) ? _marketingCenterApiSourceID : _editorProductUserId;
                string url = allPropertiesSelected
                    ? _productUrl + $"/external/contact/{_productUserId}?sourceid={sourceId}&assignAllProperties=true"
                    : _productUrl + $"/external/contact/{_productUserId}?sourceid={sourceId}&unassignAllProperties=false";

                var logData = new Dictionary<string, object> { { "url", url }, { "userJson", JsonConvert.SerializeObject(mcUser) } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateMarketingCenterUser", "PUT" });

                var response = _httpClient.PutAsJsonAsync(url, mcUser).Result;
                if (response.IsSuccessStatusCode)
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                    var userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                    long newId = userResult.id;
                    UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newId.ToString());
                    if (!long.TryParse(_editorProductUserId, out long editorIdParsed) || !IsUserIdValid(editorIdParsed))
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateMarketingCenterUser", $"Invalid admin userId: {_editorProductUserId}" });
                    SetMarketingCenterUserStatus(true, newId.ToString());
                }
                else
                {
                    return ParseErrorPosting(response, "Update", editorPersonaId, userPersonaId);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateMarketingCenterUser", ex.Message });
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                return "There was a problem updating the user";
            }

            AppendActivityLogs(mcUser, productUserBeforeUpdate, roleList, propertyList, editorPersonaId, userPersonaId, ref additionalParameters);
            return string.Empty;
        }

        /// <summary>
        /// Builds activity-log AdditionalParameters for role and property changes.
        /// </summary>
        private void AppendActivityLogs(
            MC.MarketingCenterUser mcUser,
            MC.MarketingCenterUserDetails productUserBeforeUpdate,
            List<ProductRole> roleList,
            List<ProductProperty> propertyList,
            long editorPersonaId, long userPersonaId,
            ref List<AdditionalParameters> additionalParameters)
        {
            try
            {
                if (mcUser.ContactRoleId != productUserBeforeUpdate?.ContactRoleId)
                {
                    if (productUserBeforeUpdate != null && roleList != null)
                    {
                        additionalParameters.AddRange(roleList
                            .Where(f => f?.ID != null && int.TryParse(f.ID, out int fid) && productUserBeforeUpdate.ContactRoleId == fid)
                            .Select(f => new AdditionalParameters
                            {
                                Key = "Marketing Center Roles",
                                Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", f.Name ?? string.Empty)
                            }));
                    }

                    if (roleList != null)
                    {
                        additionalParameters.AddRange(roleList
                            .Where(f => f?.ID != null && int.TryParse(f.ID, out int fid) && mcUser.ContactRoleId == fid)
                            .Select(f => new AdditionalParameters
                            {
                                Key = "Marketing Center Roles",
                                Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", f.Name ?? string.Empty)
                            }));
                    }
                }

                if (mcUser.AssignPropertyIds != null && propertyList != null)
                {
                    additionalParameters.AddRange(propertyList
                        .Where(f => f?.ID != null && int.TryParse(f.ID, out int fid) && mcUser.AssignPropertyIds.Contains(fid))
                        .Select(f => new AdditionalParameters
                        {
                            Key = "Marketing Center Properties",
                            Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.Name ?? string.Empty)
                        }));
                }

                if (mcUser.UnassignPropertyIds != null && propertyList != null)
                {
                    additionalParameters.AddRange(propertyList
                        .Where(f => f?.ID != null && int.TryParse(f.ID, out int fid) && mcUser.UnassignPropertyIds.Contains(fid))
                        .Select(f => new AdditionalParameters
                        {
                            Key = "Marketing Center Properties",
                            Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", f.Name ?? string.Empty)
                        }));
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex,
                    messageProperties: new object[] { "AppendActivityLogs", $"editorPersonaId={editorPersonaId}, userPersonaId={userPersonaId}. {ex.Message}" });
            }
        }

        /// <summary>
        /// Detects the MC duplicate-email 500 response body.
        /// </summary>
        private static bool IsDuplicateEmailError(string errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent)) return false;
            try
            {
                dynamic errorJson = JsonConvert.DeserializeObject<dynamic>(errorContent);
                string msg = errorJson?.fieldErrors?.Error?.message;
                return !string.IsNullOrEmpty(msg)
                    && msg.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0
                    && msg.IndexOf("emailAddress", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }

        private bool CheckIfUserExistInProduct(string productUserId)
        {
            var url = _productUrl + $"/external/contact/details?emailAddress={productUserId}";
            var response = _httpClient.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckIfUserExistInProduct", $"{productUserId} exists." });
                return true;
            }
            return false;
        }

        private string GetMCUniqueUserName(string firstName, string lastName)
        {
            string baseUsername = $"{firstName.TrimWhiteSpace().Substring(0, 1)}{lastName.TrimWhiteSpace().ToLower()}";
            for (int i = 1; i <= MAX_USERNAME_ATTEMPTS; i++)
            {
                string candidate = $"{baseUsername}{i}@noreply.com";
                if (!CheckIfUserExistInProduct(candidate))
                    return candidate;
            }
            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMCUniqueUserName", $"No unique name after {MAX_USERNAME_ATTEMPTS} attempts for {firstName} {lastName}." });
            return string.Empty;
        }

        /// <summary>
        /// Sets Product user status
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="mcUserId"></param>
        /// <returns></returns>
        /// <summary>
        /// Calls the MC status endpoint.
        /// FIX: replaced Convert.ToInt64 with long.TryParse to prevent "Input string was not in a correct format".
        /// FIX: logs the full HTTP response body on failure.
        /// </summary>
        private bool SetMarketingCenterUserStatus(bool isActive, string mcUserId)
        {
            try
            {
                var logData = new Dictionary<string, object>
                {
                    { "mcUserId",            mcUserId ?? "NULL" },
                    { "_editorProductUserId", _editorProductUserId ?? "NULL" },
                    { "isActive",            isActive }
                };

                if (string.IsNullOrWhiteSpace(_editorProductUserId))
                {
                    WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", "Editor Product User Id is null/empty" });
                    return false;
                }

                // FIX: Prevent "Input string was not in a correct format"
                if (!long.TryParse(_editorProductUserId, out long auditUserId))
                {
                    WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", $"_editorProductUserId '{_editorProductUserId}' is not a valid long" });
                    return false;
                }

                if (string.IsNullOrWhiteSpace(mcUserId) || mcUserId == "0")
                {
                    WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", "mcUserId is null/empty/0" });
                    return false;
                }

                string url = _productUrl + $"/external/contact/{mcUserId}/status";
                var mcStatus = new MC.MarketingCenterUserStatus
                {
                    isActive = isActive,
                    isActiveUnifiedUser = isActive,
                    auditUserId = auditUserId
                };

                logData["url"] = url;
                logData["payload"] = JsonConvert.SerializeObject(mcStatus);
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", $"PUT status for userId={mcUserId}" });

                var response = _httpClient.PutAsJsonAsync(url, mcStatus).Result;

                string responseBody = string.Empty;
                try { responseBody = response.Content.ReadAsStringAsync().Result; } catch { /*Ignored*/ }
                logData["responseStatusCode"] = response.StatusCode;
                logData["responseBody"] = responseBody;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", "Success" });
                    return true;
                }

                WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "SetMarketingCenterUserStatus", $"HTTP {(int)response.StatusCode}: {responseBody}" });
                return false;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "SetMarketingCenterUserStatus", ex.Message });
                return false;
            }
        }

        private T GetResultFromApi<T>(string baseUrlAndQuery) where T : class
        {
            var response = _httpClient.GetAsync(baseUrlAndQuery).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject(json, typeof(T)) as T;
            }

            WriteToErrorLog("{ActionName} - {state}",
                new Dictionary<string, object>
                {
                    { "uri",    baseUrlAndQuery },
                    { "error",  response.Content.ReadAsStringAsync().Result },
                    { "status", response.StatusCode }
                },
                messageProperties: new object[] { "GetResultFromApi", "Error" });
            return null;
        }

        private string ParseErrorPosting(HttpResponseMessage response, string action, long editorPersonaId, long userPersonaId)
        {
            dynamic userResult = null;
            bool emailDuplicate = false;

            try
            {
                userResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                string errorText = userResult?.fieldErrors?.Error?.message;
                if (!string.IsNullOrEmpty(errorText)
                    && errorText.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0
                    && errorText.IndexOf("emailAddress", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    emailDuplicate = true;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ParseErrorPosting", "Could not parse error body" });
            }

            var logData = new Dictionary<string, object> { { "responseBody", userResult } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "ParseErrorPosting", $"{action} error. Duplicate={emailDuplicate}" });

            if (emailDuplicate)
            {
                WriteActivityLogWithMessage(editorPersonaId, userPersonaId,
                    "An error occurred when {3} {4} attempted to provision {2} for {0} {1}." +
                    "A user already exists with this email address.Please try using the Migration Tool.");
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Stop);
                return ProductBatchStatusType.Stop.ToString();
            }

            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
            return action.ToUpper() == "CREATE"
                ? "There was a problem creating the user."
                : "There was a problem updating the user.";
        }

        /// <summary>
        /// Used to get info about a user
        /// </summary>
        /// <returns></returns>
        private MC.MarketingCenterUserDetails GetUserDetails()
        {
            if (string.IsNullOrEmpty(_productUserId)) return null;

            try
            {
                var url = _productUrl + $"/external/contact/{_productUserId}/details";
                var response = _httpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                    return JsonConvert.DeserializeObject<MC.MarketingCenterUserDetails>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex,
                    messageProperties: new object[] { "GetUserDetails", $"ProductUserId={_productUserId}. {ex.Message}" });
            }

            return null;
        }

        /// <summary>
        /// Check if the userId exists in the Marketing Center
        /// </summary>
        /// <param name="userId">Product UserID</param>
        /// <returns>boolean</returns>
        private bool IsUserIdValid(long userId)
        {
            string url = _productUrl + $"/external/contact/{userId}/status";
            var response = _httpClient.GetAsync(url).Result;
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Returns UserName for marketing center, we will send this for their auditing purpose. 
        /// </summary>
        /// <returns></returns>
        private string GetLoginName()
        {
            if (string.IsNullOrEmpty(_userClaims.ImpersonatedByName))
                return _userClaims.LoginName;

            UserDetails currentUser = _userRepository.GetUserDetails(null, _userClaims.ImpersonatedBy.ToString());
            return currentUser?.LoginName ?? _userClaims.LoginName;
        }

		/// <summary>
        /// Returns the value of a required product internal setting, throwing a descriptive exception if missing.
        /// Replaces unsafe .First() calls that would throw an uninformative InvalidOperationException.
        /// </summary>
        private string GetRequiredProductSetting(string key)
        {
            return (_productInternalSettingList
                .FirstOrDefault(a => a.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException(
                    $"ManageProductMarketingCenter: Required product internal setting '{key}' is missing."))
                .Value;
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

			if (!int.TryParse(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId, out int companyInstanceSourceId) || companyInstanceSourceId == 0)
			{
				WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Invalid or missing company id in bluebook for editorPersonaId={editorPersonaId}." });
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

			if (!int.TryParse(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId, out int companyInstanceSourceId) || companyInstanceSourceId == 0)
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