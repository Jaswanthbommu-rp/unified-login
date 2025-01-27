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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Lead2Lease;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Used to manage Lead2Lease functions
    /// </summary>
    public class ManageProductLead2Lease : ManageProductBase, IManageProductLead2Lease
    {
        private string _apiEndPoint;
        private string _mtApiEndPoint;

        ObjectCache _manageL2LCache = MemoryCache.Default;
        private static HttpMessageHandler _messageHandler;
        private static HttpClient _httpClient;
        private IManageProductOneSite _mpOneSite;
        private IList<ProductSettingList> _userProductSettings = new List<ProductSettingList>();
        private const int MAXRETRYCOUNT = 5;       
        private DefaultUserClaim _userClaims;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ManageProductLead2Lease(DefaultUserClaim userClaims) : base((int)ProductEnum.Lead2Lease, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _userClaims = userClaims;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new Logic.ManageBlueBook(userClaims);
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;           
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaim"></param>
        /// <param name="messageHandler"></param>
        /// <param name="samlRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePerson"></param>
        /// <param name="manageUserLogin"></param>
        /// <param name="managePartyRelationship"></param>
        /// <param name="manageElectronicAddress"></param>
        /// <param name="manageProductOneSite"></param>
        /// <param name="userLoginRepository"></param>
        /// <param name="repository"></param>
        public ManageProductLead2Lease(Guid editorRealPageId, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship, IManageElectronicAddress manageElectronicAddress, IManageProductOneSite manageProductOneSite, IUserLoginRepository userLoginRepository, IRepository repository)
            : base((int)ProductEnum.Lead2Lease, userClaim, repository, messageHandler)
        {
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            _editorRealPageId = editorRealPageId;
            _messageHandler = messageHandler;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _managePerson = managePerson;
            _manageUserLogin = manageUserLogin;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
            _manageElectronicAddress = manageElectronicAddress;
            _mpOneSite = manageProductOneSite;
            _userLoginRepository = userLoginRepository;
            _httpClient = new HttpClient(messageHandler);
        }

        #region Roles

        /// <summary>
        /// Used to get roles for Lead2Lease
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }

            IList<ProductRole> list;

            try
            {
                RoleInfo result = GetRolesMain();

                list = result.Roles.ToGBRoles();
                if (list == null) { list = new List<ProductRole>(); }

                if (!string.IsNullOrEmpty(_productUserId))
                {
                    Lead2LeaseUser user = GetUser(_productUserId);
                    if (user == null)
                    {
                        response = new ListResponse()
                        {
                            IsError = true,
                            ErrorReason = "User info is missing"
                        };
                        return response;
                    }
                    if (user.Permissions.Count > 0)
                    {
                        foreach (Permission p in user.Permissions)
                        {
                            if (list.Any(a => a.ID == p.UserRoleId.ToString()))
                            {
                                ProductRole pr = (from a in list
                                                  where a.ID == p.UserRoleId.ToString()
                                                  select a).FirstOrDefault();
                                if (pr != null)
                                {
                                    pr.IsAssigned = true;
                                }
                            }
                        }
                    }
                }

                list = list.OrderBy(a => a.Name).ToList();
                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
                if (result.Presets.Count > 0)
                {
                    Dictionary<string, object> presets = new Dictionary<string, object>();
                    presets.Add("Presets", result.Presets);
                    response.Additional = presets;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRoles", $"Error. {ex.Message}" });
                response = new ListResponse();
                response.IsError = true;

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    //UI is displaying the message in Right tab, so that's why the message should be right in this case
                    response.ErrorReason = CommonMessageConstants.RightErrorMessage;
                }
            }

            return response;
        }

        /// <summary>
        /// Used to get the list of properties and which properties are assigned to the given user if provided
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }

            IList<ProductProperty> list;

            try
            {
                IList<Property> result = GetPropertyMain();
                if (result == null)
                {
                    response = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = "Company Setup Error: Please Contact Support."
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", "Error retrieving property info" });
                    return response;
                }

                list = result.ToGBProperty();
                if (list == null) { list = new List<ProductProperty>(); }

                if (!string.IsNullOrEmpty(_productUserId))
                {
                    Lead2LeaseUser user = GetUser(_productUserId);
                    if (user == null)
                    {
                        response = new ListResponse()
                        {
                            IsError = true,
                            ErrorReason = "User info is missing"
                        };
                        return response;
                    }
                    List<Property> Properties = new List<Property>();
                    Properties = user.Properties;
                    if (Properties.Count == 0)
                    {
                        RolePropertyList roleproperty = new RolePropertyList();
                        roleproperty = GetDeactivatedProductBatchData(userPersonaId);

                        if (roleproperty != null && roleproperty.PropertyList != null)
                        {
                            foreach (string property in roleproperty.PropertyList)
                            {
                                Property pdata = new Property
                                {
                                    PropertyId = Convert.ToInt32(property)
                                };
                                Properties.Add(pdata);
                            }
                        }
                    }

                    if (Properties.Count > 0)
                    {
                        foreach (Property p in Properties)
                        {
                            if (list.Any(a => a.ID == p.PropertyId.ToString()))
                            {
                                ProductProperty pp = (from a in list
                                                      where a.ID == p.PropertyId.ToString()
                                                      select a).FirstOrDefault();
                                if (pp != null)
                                {
                                    pp.IsAssigned = true;
                                }
                            }
                        }
                    }
                }
                list = list.OrderBy(a => a.Name).ToList();

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };

            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetProperties", $"Error. {ex.Message}" });
                response = new ListResponse();
                response.IsError = true;

                if (ex is BlueBookException blueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }
            }
            return response;
        }

        /// <summary>
        /// Used to create/update the Lead2Lease user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="RoleList"></param>
        /// <param name="PropertyList"></param>
        /// <param name="additionalParameters"></param>
        /// <returns></returns>
        public string ManageLead2LeaseUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, out List<AdditionalParameters> additionalParameters)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", "Beginning" });
            additionalParameters = new List<AdditionalParameters>();
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response.ErrorReason; }

            Dictionary<string, object> logData = new Dictionary<string, object>();

            Lead2LeaseUser l2LUser = new Lead2LeaseUser();
            Lead2LeaseUser userBeforeUpdate = new Lead2LeaseUser();
            List<Property> propertyListToSave = new List<Property>();
            List<Permission> permissionListToSave = new List<Permission>();

            //List<Property> assignedProperties = new List<Property>();

            Component.SharedObjects.Product.OneSite.OneSiteUser OSUser = new Component.SharedObjects.Product.OneSite.OneSiteUser();

            string oneSiteSystemIdentifier = "";

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid realPageId = userPersona.RealPageId;

            var person = _managePerson.GetPerson(realPageId);

            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            bool isSuperUser = IsSuperUser(userPersona.PersonaId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"isSuperUser = {isSuperUser}" });

            // get the email address
            string userEmailAddress = "";
            IList<ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
            if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL" && a.contactMechanismUsageType?.Name.ToUpper() == "PRIMARY"))
            {
                userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" && a.contactMechanismUsageType.Name.ToUpper() == "PRIMARY" select a.AddressString).FirstOrDefault();
            }
            else
            {
                // this must look like a real email address or Intact will fail to create the user
                userEmailAddress = userLogin.LoginName;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"Before email fix userEmailAddress = {userEmailAddress}" });
            // verify email address looks valid, will fail if not
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"After email fix userEmailAddress = {userEmailAddress}" });
            if (string.IsNullOrEmpty(_productUserId))
            {
                l2LUser.Password = Guid.NewGuid().ToString().Replace("-", "");
                if (!isSuperUser)
                {
                    l2LUser.UserType = "user";
                }
                else
                {
                    l2LUser.UserType = "power user";
                }
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"Used _productUsername = {_productUsername}" });
                // get current user info
                l2LUser = GetUser(_productUserId);
                if (l2LUser == null)
                {
                    response = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = "User info missing"
                    };
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", "Error getting user info" });
                    return response.ErrorReason;
                }
                else 
                {
                    userBeforeUpdate = l2LUser.Clone() as Lead2LeaseUser;
                }
            }

            // update L2L user object
            l2LUser.FirstName = person.FirstName;
            l2LUser.LastName = person.LastName;
            l2LUser.Email = userEmailAddress;

            // Get the list of active properties to see if one of them may be linked with OneSite
            IList<Property> propertyList = GetPropertyMain();
            RoleInfo result = GetRolesMain();
            if (propertyList == null)
            {
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = "Company Setup Error: Please Contact Support."
                };
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", "Error getting property list" });
                return response.ErrorReason;
            }

            bool checkOneSiteUserInfo = false;

            if (isSuperUser)
            {
                l2LUser.UserType = "power user";
                // set the properties to save for the super user to everything
                PropertyList = new List<string>();
                foreach (Property p in propertyList)
                {
                    PropertyList.Add(p.PropertyId.ToString());
                }
            }

            // walk the list of properties sent to be saved to the user 
            foreach (string prptyId in PropertyList)
            {
                if (isSuperUser)
                {
                    propertyListToSave.Add(new Property() { PropertyId = Convert.ToInt32(prptyId) });
                    continue;
                }
                // find the property being added in the main list and see if it has a OneSite ID associated to it
                if (propertyList.Any(a => a.PropertyId.ToString() == prptyId))
                {
                    Property p = (from a in propertyList
                                  where a.PropertyId.ToString() == prptyId
                                  select a).FirstOrDefault();
                    if (p != null)
                    {
                        Property toAdd = new Property() { PropertyId = p.PropertyId, PMSystemID = p.PMSystemID };
                        if (!string.IsNullOrEmpty(p.PMSystemID))
                        {
                            checkOneSiteUserInfo = true;
                        }
                        propertyListToSave.Add(toAdd);
                    }
                }
            }
            IList<ProductProperty> osPropertyList;

            // OneSite super users aren't assigned the Leasing Consultant right so no need to check for the right for a GB Super User
            if (checkOneSiteUserInfo && !isSuperUser)
            {
                // See if the L2L user is also a OneSite user
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, (int)ProductEnum.OneSite);

                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    oneSiteSystemIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
                    if (_mpOneSite == null) { _mpOneSite = new ManageProductOneSite(_userClaims); }
                    OSUser = _mpOneSite.GetOneSiteUserInfo(oneSiteSystemIdentifier);

                    response = _mpOneSite.GetOneSitePropertyList(editorPersonaId, userPersonaId, true, null);
                    osPropertyList = response.Records.Cast<ProductProperty>().ToList();
                    bool isLeasingAgentInOneSite = false;
                    bool didLeasingAgentOneSiteCheck = false;

                    foreach (Property p in propertyListToSave)
                    {
                        if (!string.IsNullOrEmpty(p.PMSystemID))
                        {
                            if (osPropertyList.Any(a => a.ID == p.PMSystemID))
                            {
                                // the L2L system id appears to be a OneSite site id, so see if this user has the Leasing Consultant right
                                if (!didLeasingAgentOneSiteCheck)
                                {
                                    isLeasingAgentInOneSite = _mpOneSite.UserInLeasingAgentList(editorPersonaId, userPersonaId, Convert.ToInt32(p.PMSystemID));
                                    didLeasingAgentOneSiteCheck = true;
                                }
                                if (isLeasingAgentInOneSite)
                                {
                                    p.PMUserId = OSUser.UserId.ToString();
                                    p.PMUserName = OSUser.SystemIdentifier.Split('|')[1];
                                    p.FirstName = person.FirstName;
                                    p.LastName = person.LastName;
                                }
                                else
                                {
                                    p.PMSystemID = null;
                                }
                            }
                        }
                    }
                }
            }

            // remove any PMCSystem id's if they don't belong
            foreach (Property p in propertyListToSave)
            {
                if (!string.IsNullOrEmpty(p.PMSystemID) && p.PMUserId == null)
                {
                    p.PMSystemID = null;
                }
            }

            if (!isSuperUser)
            {
                //permissionListToSave
                foreach (Property p in propertyListToSave)
                {
                    foreach (string roldId in RoleList)
                    {
                        permissionListToSave.Add(new Permission() { PropertyId = p.PropertyId, UserRoleId = Convert.ToInt32(roldId) });
                    }
                }
            }
            else
            {
                try
                {
                    result = GetRolesMain();
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageLead2LeaseUser", $"Error. {ex.Message}" });
                    response = new ListResponse();
                    response.IsError = true;

                    if (ex is BlueBookException)
                    {
                        response.ErrorReason = ex.Message;
                    }
                    else
                    {
                        response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                    }

                    return response.ErrorReason;
                }

                List<string> adminRights = new List<string> {
                "ALLOW USER TO CHANGE PASSWORDS MANUALLY",
                "ATTACH FILE FROM ATTACHMENT MANAGER",
                "ATTACH FILES ON DEMAND",
                "CAN SCHEDULE REPORTS",
                "ENABLE PUSH NOTIFICATIONS",
                "EXPORT LEADS - MULTIPLE PROPERTIES",
                "FULL ACCESS",
                "MANAGE FILES IN ATTACHMENT MANAGER",
                "RUN MULTI PROPERTY REPORTS",
                "SCORE CALLS",
                "SEND EMAILS FROM LEAD2LEASE",
                "SET AUTORESPONSE POLICIES",
                "SET EMAIL PREFERENCES",
                "SET PROPERTY SETTINGS",
                "SUPER USER"
                };

                if (result.Roles != null)
                {
                    foreach (Property p in propertyListToSave)
                    {
                        foreach (Role role in result.Roles)
                        {
                            if (adminRights.Contains(role.UserRoleName.ToUpper()))
                            {
                                permissionListToSave.Add(new Permission() { PropertyId = p.PropertyId, UserRoleId = role.UserRoleId });
                            }
                        }
                    }
                }

            }

            l2LUser.Properties = propertyListToSave;
            l2LUser.Permissions = permissionListToSave;
            try
            {
                if (string.IsNullOrEmpty(_productUserId))
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);

                    char[] arr = userPersona.Organization.Name.ToCharArray();
                    arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c))));
                    string orgname = new string(arr);
                    string baseUrlAndQuery = $"{_apiEndPoint}/Users/{orgname}";
                    logData = new Dictionary<string, object>
                    {
                        { "user", RemovePrivateData(l2LUser) }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"Creating user. userPersonaId = {userPersonaId} baseUrlAndQuery = {baseUrlAndQuery}" });
                    var userResponse = _httpClient.PostAsJsonAsync(baseUrlAndQuery, l2LUser).Result;

                    logData = new Dictionary<string, object>
                    {
                        { "userResult.Result", userResponse.Content.ReadAsStringAsync().Result }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"Creating user response. userPersonaId = {userPersonaId} baseUrlAndQuery = {baseUrlAndQuery}" });
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var userResult = JsonConvert.DeserializeObject<Lead2LeaseUser>(userResponse.Content.ReadAsStringAsync().Result);
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, userResult.UserName);
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, userResult.UserId.ToString());
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"Created user. UserLogin:{userResult.UserName} UserId:{userResult.UserId}" });
                        //update user migration status
                        // Update UL flag in product

                        additionalParameters.AddRange(ExtractActivityDetailLogs(userBeforeUpdate, l2LUser, result, propertyList));

                        var updateResponse = UpdateUsersMigrationStatus(editorPersonaId, new List<MigrateUser>
                        {
                            new MigrateUser
                            {
                                UnifiedLoginUserName = userLogin.LoginName,
                                UserId = userResult.UserId.ToString(),
                                UsingUnifiedLogin = true
                            }
                        });

                        if (!updateResponse.Status)
                            return updateResponse.Message;
                    }
                    else
                    {
                        // write an error
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", "Create user errored. Set product status to Error" });
                        return "Error";
                    }
                }
                else
                {
                    string baseUrlAndQuery = $"{_apiEndPoint}/Users/edit";
                    logData = new Dictionary<string, object> { { "user", RemovePrivateData(l2LUser) } };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "ManageLead2LeaseUser", $"Updating user. userPersonaId = {userPersonaId} baseUrlAndQuery = {baseUrlAndQuery}" });
                    var userResponse = _httpClient.PutAsJsonAsync(baseUrlAndQuery, l2LUser).Result;
                    logData = new Dictionary<string, object>
                    {
                        { "userResult.Result", userResponse.Content.ReadAsStringAsync().Result }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "ManageLead2LeaseUser", $"Updating user response. userPersonaId = {userPersonaId} baseUrlAndQuery = {baseUrlAndQuery}" });
                    if (!userResponse.IsSuccessStatusCode)
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageLead2LeaseUser", $"Updating user errored. userPersonaId = {userPersonaId} baseUrlAndQuery = {baseUrlAndQuery}" });
                        return "Error";
                    }
                    else
                    {
                        additionalParameters.AddRange(ExtractActivityDetailLogs(userBeforeUpdate, l2LUser, result, propertyList));
                    }
                }

                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageLead2LeaseUser", $"Error. userPersonaId = {userPersonaId} Message: {ex.Message}" });
                return "Error";
            }
            return "";
        }

        /// <summary>
        /// Unassign User
        /// </summary>
        /// <returns></returns>    
        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            var result = EnableDisableUser(editorPersonaId, userPersonaId);

            if (!string.IsNullOrEmpty(result))
            {
                // write an error
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", "UnassignUser user errored. Set product status to Error" });
                return "Error";
            }
            
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Successfully deactivated user userPersonaId:{userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

            return "";
        }

        public string UpdateLead2LeaseUserProfile(long editorPersonaId, long userPersonaId)
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Begin update user profile" });
                var productLoginName = "";

                var userPersona = _managePersona.GetPersona(userPersonaId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Got persona info" });
                var realPageId = userPersona.RealPageId;

                var person = _managePerson.GetPerson(realPageId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Got person info" });

                var userLogin = new UserLoginOnly();
                userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                IList<UserOrganization> userPersonaOrganizationList = _manageUserLogin.GetUserPersonaOrganization(userLogin.LoginName);
                bool isRegularUserNoEmail = IsRegularUserNoEmail(userPersonaId);

                // get the email address
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Begin get user email address" });
                string userEmailAddress = "";
                string userLeadEmailAddress = "";
                ManageElectronicAddress _manageElectronicAddress = new ManageElectronicAddress();
                IList<ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Got list of electronic address" });
                if (_addresses != null)
                {
                    if (_addresses.Any(a => a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase)))
                    {
                        userEmailAddress = (from a in _addresses where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"Found email address. {userEmailAddress}" });
                    }
                }
                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    userEmailAddress = userLogin.LoginName;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Using login name for email address" });
                }

                if (isRegularUserNoEmail)
                {
                    userLeadEmailAddress = userEmailAddress;
                }
                // verify email address looks valid, will fail if not
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"User Type : {userPersona.UserTypeId}" });
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"Validating email address. Email: {userLogin.LoginName}" });
                
                if (userPersona.UserTypeId == (int)UserTypeConstants.RegularUserNoEmail)
                {
                    userEmailAddress = _productUsername;
                }
                else
                {
                    userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"Validated email address. Email: {userEmailAddress}" });
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"Product User Name : {_productUsername}" });

                productLoginName = _productUsername;

                //If the User's LoginName changed in the PrimaryOrganization then update it in the Product
                if ((userPersonaOrganizationList.ToList().Any(o => o.PrimaryOrganization.Equals(true)
                    && o.OrganizationPartyId.Equals(userPersona.OrganizationPartyId)))
                    && (!_productUsername.Equals(userEmailAddress, StringComparison.OrdinalIgnoreCase)))
                {
                    productLoginName = userEmailAddress;
                }
                var user = GetUser(_productUserId);

                if (user == null)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"Error looking for user. userPersonaId={userPersonaId}" });
                    return "User not found in product";
                }

                var L2LUser = new Lead2LeaseUser()
                {
                    UserId = user.UserId,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = userEmailAddress,
                    UserName = productLoginName
                };

                var url = _apiEndPoint + $"/Users/profile";
                logData = new Dictionary<string, object>
                {
                    { "url", url },
                    { "L2LUser", L2LUser }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Update user profile" });
                var response = _httpClient.PutAsJsonAsync(url, L2LUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"StartUpdate user SAMLAttribute User_email={productLoginName}" });
                    UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateLead2LeaseUserProfile", "Update user SAMLAttribute User_email success. Saved user id" });

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
                    WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"Error for user with editorPersona id - {editorPersonaId}" });
                    return $"There was a problem updating user profile for user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
                }

            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateLead2LeaseUserProfile", $"Error for user with editorPersona id - {editorPersonaId}" });
                return $"Error - {ex.Message}";
            }
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

            try
            {
                var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (claimResponse.IsError) { response.ErrorReason = claimResponse.ErrorReason; return response; }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
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

                var url = $"{_mtApiEndPoint}/{companyInstanceSourceId}/users?filter={filter}&startRow={startRow}&resultsperpage={resultPerRow}";
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "Url", url } }, messageProperties: new object[] { "GetMigrationUsers", "GetUsers" });
                var allUsers = GetResultFromApi<IList<MigrationUser>>(url);

                if (allUsers == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}" });
                    return response;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}" });
                response.RowsPerPage = resultPerRow;
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

                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetMigrationUsers", $"Error for user with editorPersona id - {editorPersonaId}" });
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

            try
            {

                var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (claimResponse.IsError) { migrateResponse.Message = claimResponse.ErrorReason; return migrateResponse; }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                    migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                    return migrateResponse;
                }

                var url = $"{_mtApiEndPoint}/{companyInstanceSourceId}/migrate-users";
                var response = _httpClient.PutAsJsonAsync(url, migrateUsers).Result;
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
                    var migrationResponse = JsonConvert.DeserializeObject<MigrateResponse>(responseContent);
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync" });
                    migrateResponse.Message = migrationResponse.Message;
                    migrateResponse.Status = migrationResponse.Status;
                    return migrateResponse;
                }

                WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync - Error" });
                migrateResponse.Message = "Cannot update user status to migrated.";
                return migrateResponse;
            }
            catch (Exception ex)
            {
                migrateResponse = new MigrateResponse
                { 
                    Status = false,
                    Message = ex.Message
                };
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error for user with editorPersona id - {editorPersonaId}" });
                
                return migrateResponse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userName"></param>
        /// <param name="productUserId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, string userName, string productUserId, bool isActive = false)
        {
            var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError) { return false; }
            _productUserId = productUserId;
            try
            {
                var response = UpdateL2LUserstatus(isActive);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var logData = new Dictionary<string, object>();
                var errorMessage = response.Content.ReadAsStringAsync().Result.ToString();
                logData.Add("error", errorMessage);
                logData.Add("status", response.StatusCode);
                WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "ChangeUserStatus", $"Error for user with editorPersona id - {editorPersonaId}. Message: {errorMessage}" });
                return false;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {userName} by editorPersonaId = {editorPersonaId}" });
                return false;
            }
        }

        #endregion

        #region Private functions

        private List<AdditionalParameters> ExtractActivityDetailLogs(Lead2LeaseUser userBeforeUpdate, Lead2LeaseUser l2LUser, RoleInfo result, IList<Property> propertyList)
        {
            //Activity log details
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            //Rights
            if (userBeforeUpdate?.Permissions == null)
            {
                userBeforeUpdate.Permissions = new List<Permission>();
            }
            var oldAccessCodes = userBeforeUpdate.Permissions.Select(s => s.UserRoleId);
            var newAccessCodes = l2LUser.Permissions.Select(s => s.UserRoleId);

            var removedRoles = oldAccessCodes.Except(newAccessCodes).ToList();
            var addedRoles = newAccessCodes.Except(oldAccessCodes).ToList();

            if (newAccessCodes.Any())
            {
                foreach (var r in addedRoles)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Lead2Lease Rights", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", result.Roles.FirstOrDefault(f => f.UserRoleId == r)?.UserRoleName) });
                }
            }
            if (removedRoles.Any())
            {
                foreach (var r in removedRoles)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Lead2Lease Rights", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", result.Roles.FirstOrDefault(f => f.UserRoleId == r)?.UserRoleName) });
                }
            }

            //Properties
            if (userBeforeUpdate?.Properties == null)
            {
                userBeforeUpdate.Properties = new List<Property>();
            }
            var oldProperties = userBeforeUpdate.Properties.Select(s => s.PropertyId);
            var newProperties = l2LUser.Properties.Select(s => s.PropertyId);

            var removedProperties = oldProperties.Except(newProperties).ToList();
            var addedProperties = newProperties.Except(oldProperties).ToList();

            if (addedProperties.Any())
            {
                foreach (var p in addedProperties)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Lead2Lease Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", propertyList.FirstOrDefault(f => f.PropertyId == p)?.ComplexName) });
                }
            }
            if (removedProperties.Any())
            {
                foreach (var p in removedProperties)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Lead2Lease Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", propertyList.FirstOrDefault(f => f.PropertyId == p)?.ComplexName) });
                }
            }
            return additionalParameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        private string EnableDisableUser(long editorPersonaId, long userPersonaId)
        {
            string result = string.Empty;
            Dictionary<string, object> logData = new Dictionary<string, object>();
            var response = UpdateL2LUserstatus();
            if (response.IsSuccessStatusCode)
            {
                return result;
            }

            var errorMessage = response.Content.ReadAsStringAsync().Result.ToString();
            logData.Add("error", errorMessage);
            logData.Add("status", response.StatusCode);
            WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "EnableDisableUser", $"Error for user with editorPersona id - {editorPersonaId}. Message: {errorMessage}" });
            return $"There was a problem Disabling the user with editorPersona id - {editorPersonaId} - Error-{errorMessage}.";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private HttpResponseMessage UpdateL2LUserstatus(bool isActive = false)
        {
            Lead2LeaseUser l2LUser = new Lead2LeaseUser();
            l2LUser = GetUser(_productUserId);

            int[] propIds = new int[0];
            if (l2LUser != null && l2LUser.Properties != null)
            {
                propIds = new int[l2LUser.Properties == null ? 0 : l2LUser.Properties.Count];
                int i = 0;
                foreach (var item in l2LUser.Properties)
                {
                    propIds[i] = item.PropertyId;
                    i++;
                }
            }
            string baseUrlAndQuery = isActive ? $"{_apiEndPoint}/Users/Enable/{_productUserId}" : $"{_apiEndPoint}/Users/Disable/{_productUserId}";
            var logData = new Dictionary<string, object> { { "baseUrlAndQuery", baseUrlAndQuery } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateL2LUserstatus", "Enable or disable L2L user" });
            var response = _httpClient.PutAsJsonAsync(baseUrlAndQuery, propIds).Result;
            return response;
        }

        /// <summary>
        /// Used to get the list of roles from Lead2Lease
        /// </summary>
        /// <returns></returns>
        private RoleInfo GetRolesMain()
        {
            RoleInfo result;

            var logData = new Dictionary<string, object>();
            string baseUrlAndQuery = $"{_apiEndPoint}/Users/ActiveRoles";
            logData.Add("baseUrlAndQuery", baseUrlAndQuery);
            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "GetRolesMain", "Getting info" });
            result = GetResultFromApi<RoleInfo>(baseUrlAndQuery, false);

            if (result == null)
            {
                throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);
            }

            if (result.Presets?.Any() != true)
            {
                throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Used to get the list of properties from Lead2Lease
        /// </summary>
        /// <returns></returns>
        private IList<Property> GetPropertyMain()
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
            //string lead2LeaseCompanyId = (lead2leasecompanyoverride == "" ? company.CompanyInstanceSourceId : lead2leasecompanyoverride);
            // switch to BlueBook when it has valid data and can link L2L Properties with OneSite properties
            //IList<PropertyInstance> bbPropertyList = _blueBook.GetPropertyInstance(company.CompanyInstanceId);
            // switch to BlueBook when it has valid data and can link L2L Properties with OneSite properties
            string lead2LeaseCompanyId = company?.CompanyInstanceSourceId;
            IList<Property> propertyListResult = new List<Property>();

            if (string.IsNullOrEmpty(lead2LeaseCompanyId) || lead2LeaseCompanyId == "0")
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyMain", "Error looking for company id in bluebook" });
                return null;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyMain", $"Found blue book company source id {lead2LeaseCompanyId}" });
            
            string baseUrlAndQuery = $"{_apiEndPoint}/Users/ActiveProperties/{lead2LeaseCompanyId}";
            logData.Add("baseUrlAndQuery", baseUrlAndQuery);
            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "GetPropertyMain", "Getting info" });
            try
            {
                propertyListResult = GetResultFromApi<IList<Property>>(baseUrlAndQuery, false);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, logData: logData, messageProperties: new object[] { "GetPropertyMain", "Failed" });
                propertyListResult = null;
            }
            return propertyListResult;
        }

        /// <summary>
        /// Used to get information for a user
        /// </summary>
        /// <returns></returns>
        private Lead2LeaseUser GetUser(string userId)
        {
            string baseUrlAndQuery = $"{_apiEndPoint}/Users/{userId}";
            var logData = new Dictionary<string, object> { { "baseUrlAndQuery", baseUrlAndQuery } };
            WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "GetUser", "Getting info" });
            Lead2LeaseUser user = new Lead2LeaseUser();
            try
            {
                user = GetResultFromApi<Lead2LeaseUser>(baseUrlAndQuery, false);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, logData: logData, messageProperties: new object[] { "GetPropertyMain", "Failed" });
                user = null;
            }
            return user;
        }

        /// <summary>
        /// Used to remove private info
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private Lead2LeaseUser RemovePrivateData(Lead2LeaseUser user)
        {
            Lead2LeaseUser l2luser = user.Clone() as Lead2LeaseUser;

            l2luser.Password = "XXXX";
            return l2luser;
        }

        /// <summary>
        /// Used to get information from the product
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseUrlAndQuery"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        private T GetResultFromApi<T>(string baseUrlAndQuery, bool throwOnError = true) where T : class
        {
            T results = null;

            //if (_messageHandler != null)
            //{
            //    _httpClient = new HttpClient(_messageHandler);
            //}
            //else
            //{
            //    _httpClient = new HttpClient();
            //}
            var response = _httpClient.GetAsync(baseUrlAndQuery).Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
            }

            return results;
        }

        private class RoleInfo
        {
            public IList<Preset> Presets { get; set; }
            public IList<Role> Roles { get; set; }
        }

        #endregion
    }
    #endregion
    /// <summary>
    /// Used to build the XML required to call the OneSite web services
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Used to convert product roles to GreenBook roles
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public static IList<ProductRole> ToGBRoles(this IList<Role> roles)
        {
            if (roles == null || roles.Count == 0)
                return null;
            IList<ProductRole> results = new List<ProductRole>();
            foreach (Role role in roles)
            {
                results.Add(new ProductRole
                {
                    ID = role.UserRoleId.ToString(),
                    Name = role.UserRoleName,
                    IsAssigned = false,
                    Roletype = role.RoleTypeId.ToString()
                });

            }
            return results;
        }

        /// <summary>
        /// Used to convert product properties to GreenBook properties
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IList<ProductProperty> ToGBProperty(this IList<Property> properties)
        {
            if (properties == null || properties.Count == 0)
                return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (Property property in properties)
            {
                results.Add(new ProductProperty
                {
                    ID = property.PropertyId.ToString(),
                    Name = property.ComplexName,
                    City = property.City,
                    State = property.State,
                    Street1 = property.Address,
                    Zip = property.Zip,
                    IsAssigned = false,
                });

            }
            return results;
        }

    }
}


