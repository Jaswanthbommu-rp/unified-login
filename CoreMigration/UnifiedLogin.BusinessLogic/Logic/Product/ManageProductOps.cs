using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Product.Ops.Extensions;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    /// Used to update Ops (Spend Management) user information
    /// </summary>
    public class ManageProductOps : ManageProductBase, IManageProductOps
    {
        private string _opsBuyerUrl;
        private string _currentSessionId;

        private int _moduleAssetGroups = 0;
        ObjectCache _manageOpsCache = MemoryCache.Default;

        private const int MAXRETRYCOUNT = 5;
        private const int SIDREFRESHTIMEMINUTES = 90;
        private const int HTTP_TIMEOUT_SECONDS = 60;
        private DefaultUserClaim _userClaims;
        private const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
        private const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
        private const string PRODUCT_ROLE_DEC_UPDATE = "{\"action\":\"Role Description updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_InvoiceWorkflowTimeout_UPDATE = "{\"action\":\"Role InvoiceWorkflowTimeout updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_OrderWorkflowTimeout_UPDATE = "{\"action\":\"Role OrderWorkflowTimeout updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_OrderEndorseEmailReminderFlag_UPDATE = "{\"action\":\"Role OrderEndorseEmailReminderFlag updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_InvoiceEndorseEmailReminderFlag_UPDATE = "{\"action\":\"Role InvoiceEndorseEmailReminderFlag updated\",\"value\":\"NewValue\"}";

        #region Ctor
        public ManageProductOps(DefaultUserClaim userClaims) : base((int)ProductEnum.OpsBuyer, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _editorRealPageId = userClaims.UserRealPageGuid;
            _userClaims = userClaims;
            _blueBook = new ManageBlueBook(userClaims);

            _opsBuyerUrl = _productInternalSettingList.First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
            _client.BaseAddress = new Uri(_opsBuyerUrl);
            _client.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);
        }

        public ManageProductOps(Guid editorRealPageId, DefaultUserClaim userClaim, HttpMessageHandler httpMessageHandler, HttpClient client, IProductInternalSettingRepository productInternalSettingRepository,
                                    IManagePersona managePersona, ISamlRepository samlRepository, IManageBlueBook blueBook, IProductRepository productRepository, IRepository repository)
             : base((int)ProductEnum.OpsBuyer, userClaim, repository, httpMessageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _userClaims = userClaim;
            _client = client;
            _productInternalSettingRepository = productInternalSettingRepository;
            _blueBook = blueBook;
            _managePersona = managePersona;
            _samlRepository = samlRepository;
            _productRepository = productRepository;
            _opsBuyerUrl = _productInternalSettingList.First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
            _client.BaseAddress = new Uri(_opsBuyerUrl);
            _client.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS);
        }
        #endregion

        #region Public Methods
        public ListResponse GetOpsAssetGroups(long editorPersonaId, long userPersonaId, int assetGroupId = 0)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            return GetOpsAssetGroupsDetails(editorPersonaId, userPersonaId, assetGroupId);
        }

        public ListResponse GetOpsAssets(long editorPersonaId, long userPersonaId, string status)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            return GetOpsAssetDetails(editorPersonaId, userPersonaId, status);
        }

        public ListResponse CreateOpsAssetGroup(long editorPersonaId, long userPersonaId, AssetGroupCreate assetGroup)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            return CreateAssetGroup(editorPersonaId, userPersonaId, assetGroup);
        }

        public ListResponse UpdateOpsAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupCreate assetGroup)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            return UpdateAssetGroup(editorPersonaId, userPersonaId, assetGroupId, assetGroup);
        }

        public ListResponse PatchOpsAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupPatch assetGroup)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }
            return PatchAssetGroup(editorPersonaId, userPersonaId, assetGroupId, assetGroup);
        }

        public ListResponse GetCompanyAssets(long editorPersonaId, long userPersonaId, bool includeDisabled, RequestParameter datafilter)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                return response;
            }
            return GetCompanyAssetDetails(editorPersonaId, userPersonaId, includeDisabled, updateAssetNames: true);
        }

        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, string assetCode, RequestParameter datafilter)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                return response;
            }
            return GetRoles(editorPersonaId, userPersonaId, assetCode);
        }

        public ListResponse GetRolesCount(long editorPersonaId, string assetCode)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            return GetRolesCountDetails(editorPersonaId, assetCode);
        }

        public ListResponse GetRolesForRight(long editorPersonaId, int rightId)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                ListResponse allRoles = GetRoles(editorPersonaId, editorPersonaId, string.Empty);
                logData.Add("allRoles", allRoles);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesForRight", "Result from api" }, logData: logData);
                return allRoles;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesForRight", "Error" }, exception: ex);
                return new ListResponse() { IsError = true, ErrorReason = ex.Message };
            }
        }

        public ListResponse GetRightsByRole(long editorPersonaId, long roleId)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            return GetRightsDetailsForRole(editorPersonaId, roleId);
        }

        public ListResponse GetRights(long editorPersonaId)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            return GetRightsDetails(editorPersonaId);
        }

        public string EnableUser(long editorPersonaId, long userPersonaId, bool isActive, bool deleteUser)
        {
            ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            OpsUser opsUser = GetUserDetailsById(Convert.ToInt32(_productUserId));
            if (opsUser == null) { return "There was an error getting the user details"; }

            opsUser.Email = null;
            opsUser.Phone = null;

            if (opsUser.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && isActive) { return ""; }
            if (opsUser.Status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) && !isActive) { return ""; }

            opsUser.Status = isActive ? "active" : "inactive";
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/users/" + _productUserId;
                var logData = new Dictionary<string, object> { { "url", url }, { "opsUser", opsUser } };
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "EnableUser", $"Updating user status. productuserId = {_productUserId}, isActive = {isActive}" }, logData: logData);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                var putResponse = SendWithSidAsync(HttpMethod.Put, url, opsUser, cts.Token).GetAwaiter().GetResult();

                if (putResponse.IsSuccessStatusCode)
                {
                    if (isActive)
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                    else if (deleteUser)
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
                    else
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Inactive);
                }
                else
                {
                    var errorContent = putResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "EnableUser", "Update user errored." }, logData: new Dictionary<string, object> { { "response", errorContent } });
                    return "There was a problem updating the user status";
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "EnableUser", $"Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive}" }, exception: ex);
            }
            return "";
        }

        public ListResponse CreateRole(long editorPersonaId, OpsInput rightInput, long roleId)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            Dictionary<string, object> logData;
            ListResponse roleListResponse = new ListResponse();
            var rightsToAdd = new List<string>();
            var rightsToRemove = new List<string>();
            List<object> rightsInput = new List<object>();
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);

            foreach (var item in rightInput.rightsList)
            {
                var resp = new Dictionary<string, string> { { "name", item.Name }, { "value", item.Value } };
                rightsInput.Add(resp);
                if (item.Value == "1") rightsToAdd.Add(item.Name);
                else rightsToRemove.Add(item.Name);
            }

            dynamic newRole = new
            {
                name = rightInput.RoleName,
                description = rightInput.RoleDesc,
                invoice_endorse_email_reminder_flag = rightInput.InvoiceEndorseEmailReminderFlag == "true" ? "1" : "0",
                order_workflow_timeout = rightInput.OrderWorkflowTimeout == "" ? "0" : rightInput.OrderWorkflowTimeout,
                invoice_workflow_timeout = rightInput.InvoiceWorkflowTimeout == "" ? "0" : rightInput.InvoiceWorkflowTimeout,
                order_endorse_email_reminder_flag = rightInput.OrderEndorseEmailReminderFlag == "true" ? "1" : "0",
                responsibility_list = rightsInput
            };

            if (roleId == 0)
            {
                try
                {
                    var url = _opsBuyerUrl + "/api/v1.0/roles";
                    logData = new Dictionary<string, object> { { "url", url }, { "newRole", JsonConvert.SerializeObject(newRole) } };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Creating role" }, logData: logData);

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var postResponse = SendWithSidAsync(HttpMethod.Post, url, (object)newRole, cts.Token).GetAwaiter().GetResult();

                    if (postResponse.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<Role>(postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", $"Create Role - result = {result}" });

                        IList<Role> rolesList = new List<Role> { result };
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = rolesList.Count, RowsPerPage = rolesList.Count, TotalPages = 1, ErrorReason = "", IsError = false
                        };
                        unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, rightInput.RoleName, "ADD", "Spend Management", null, 13);
                        if (rightsToAdd.Any() || rightsToRemove.Any())
                            UpdateRightsToRoleLogMessage(editorPersonaId, rightInput.RoleName, rightsToAdd, rightsToRemove);
                    }
                    else
                    {
                        string error = postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        logData = new Dictionary<string, object> { { "postResponse.Content", error } };
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Create role errored." }, logData: logData);
                        IList<Role> rolesList = new List<Role>();
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true
                        };
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", $"Create role errored. {ex.Message}" }, exception: ex);
                    IList<Role> rolesList = new List<Role>();
                    roleListResponse = new ListResponse()
                    {
                        Records = rolesList.Cast<object>().ToList(),
                        TotalRows = 0, RowsPerPage = 0, TotalPages = 1,
                        ErrorReason = "There was a problem creating the role. " + ex.Message, IsError = true
                    };
                }
            }
            else
            {
                try
                {
                    var rolesListResponse = GetRolesCountDetails(editorPersonaId, null);
                    var roleList = rolesListResponse.Records.Cast<Role>().ToList();
                    var oldRole = roleList.FirstOrDefault(r => r.Id == roleId.ToString());
                    var url = _opsBuyerUrl + "/api/v1.0/roles/" + roleId;
                    logData = new Dictionary<string, object> { { "url", url }, { "CreateRole - Update", newRole } };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Update user" }, logData: logData);

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var putResponse = SendWithSidAsync(HttpMethod.Put, url, (object)newRole, cts.Token).GetAwaiter().GetResult();

                    if (putResponse.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<Role>(putResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        IList<Role> rolesList = new List<Role> { result };
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = rolesList.Count, RowsPerPage = rolesList.Count, TotalPages = 1, ErrorReason = "", IsError = false
                        };
                        if (oldRole.Name != newRole.name)
                            unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, newRole.name, "UPDATE", "Spend Management", oldRole.Name, 13);
                        if (oldRole.Description != newRole.description)
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.Description, newRole.description, "Description");
                        if (oldRole.InvoiceWorkflowTimeout != newRole.invoice_workflow_timeout)
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.InvoiceWorkflowTimeout, newRole.invoice_workflow_timeout, "InvoiceWorkflowTimeout");
                        if (oldRole.OrderWorkflowTimeout != newRole.order_workflow_timeout)
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.OrderWorkflowTimeout, newRole.order_workflow_timeout, "OrderWorkflowTimeout");
                        if (oldRole.OrderEndorseEmailReminderFlag != newRole.order_endorse_email_reminder_flag)
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.OrderEndorseEmailReminderFlag, newRole.order_endorse_email_reminder_flag, "OrderEndorseEmailReminderFlag");
                        if (oldRole.InvoiceEndorseEmailReminderFlag != newRole.invoice_endorse_email_reminder_flag)
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.InvoiceEndorseEmailReminderFlag, newRole.invoice_endorse_email_reminder_flag, "InvoiceEndorseEmailReminderFlag");
                        if (rightsToAdd.Any() || rightsToRemove.Any())
                            UpdateRightsToRoleLogMessage(editorPersonaId, rightInput.RoleName, rightsToAdd, rightsToRemove);
                    }
                    else
                    {
                        string error = putResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        logData = new Dictionary<string, object> { { "response", error } };
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Update role errored in else" }, logData: logData);
                        IList<Role> rolesList = new List<Role>();
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true
                        };
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", $"Update role errored in exception. {ex.Message}" }, exception: ex);
                    IList<Role> rolesList = new List<Role>();
                    roleListResponse = new ListResponse()
                    {
                        Records = rolesList.Cast<object>().ToList(),
                        TotalRows = 0, RowsPerPage = 0, TotalPages = 1,
                        ErrorReason = "There was a problem creating the role. " + ex.Message, IsError = true
                    };
                }
            }
            return roleListResponse;
        }

        public void UpdateRightsToRoleLogMessage(long editorPersonaId, string roleName, List<string> rightsToAdd, List<string> rightsToRemove)
        {
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            if (rightsToAdd != null)
                foreach (var right in rightsToAdd)
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_ASSIGN.Replace("RightName", right) });
            if (rightsToRemove != null)
                foreach (var right in rightsToRemove)
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_UNASSIGN.Replace("RightName", right) });

            var message = impersonatorUserInfo != null
                ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed Rights to {roleName} in Spend Management."
                : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed rights to {roleName} in Spend Management.";

            unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, 13);
        }

        public void updateRoleLogMessage(long editorPersonaId, string roleName, string oldValue, string newValue, string fieldName)
        {
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            string message;
            string templateKey = $"{roleName}-{oldValue}";

            switch (fieldName)
            {
                case "Description":
                    message = impersonatorUserInfo != null
                        ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated Description of {roleName} in Spend Management from {oldValue} to {newValue}."
                        : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated Description of {roleName} in Spend Management from {oldValue} to {newValue}.";
                    additionalParameters.Add(new AdditionalParameters { Key = templateKey, Value = PRODUCT_ROLE_DEC_UPDATE.Replace("NewValue", newValue) });
                    break;
                case "InvoiceWorkflowTimeout":
                    message = impersonatorUserInfo != null
                        ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated InvoiceWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}."
                        : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated InvoiceWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}.";
                    additionalParameters.Add(new AdditionalParameters { Key = templateKey, Value = PRODUCT_ROLE_InvoiceWorkflowTimeout_UPDATE.Replace("NewValue", newValue) });
                    break;
                case "OrderWorkflowTimeout":
                    message = impersonatorUserInfo != null
                        ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated OrderWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}."
                        : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated OrderWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}.";
                    additionalParameters.Add(new AdditionalParameters { Key = templateKey, Value = PRODUCT_ROLE_OrderWorkflowTimeout_UPDATE.Replace("NewValue", newValue) });
                    break;
                case "OrderEndorseEmailReminderFlag":
                    message = impersonatorUserInfo != null
                        ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated OrderEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}."
                        : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated OrderEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}.";
                    additionalParameters.Add(new AdditionalParameters { Key = templateKey, Value = PRODUCT_ROLE_OrderEndorseEmailReminderFlag_UPDATE.Replace("NewValue", newValue) });
                    break;
                case "InvoiceEndorseEmailReminderFlag":
                    message = impersonatorUserInfo != null
                        ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated InvoiceEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}."
                        : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated InvoiceEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}.";
                    additionalParameters.Add(new AdditionalParameters { Key = templateKey, Value = PRODUCT_ROLE_InvoiceEndorseEmailReminderFlag_UPDATE.Replace("NewValue", newValue) });
                    break;
                default:
                    return;
            }
            unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, 13);
        }

        public string ManageOpsUser(long editorPersonaId, long userPersonaId, List<int> RoleList, List<int> PropertyList, out List<AdditionalParameters> additionalParameters)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            additionalParameters = new List<AdditionalParameters>();
            if (response.IsError) { return response.ErrorReason; }

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid realPageId = userPersona.RealPageId;
            Dictionary<string, object> logData;

            Person person = _managePerson.GetPerson(realPageId);
            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
            IList<UserLoginPersona> userLoginPersonaList = _userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userPersona.UserId, organizationPartyId: userPersona.Organization.PartyId);
            var employeeId = _userRepository.GetUserEmployeeId(userLoginPersonaList[0].UserLoginPersonaId, userPersona.OrganizationPartyId);
            person.EmployeeId = (employeeId != null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : null;

            IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
            IList<CommonAddress> contactMechansimList = contactMechanismLogic.ListContactMechanismForPerson(realPageId, null);

            string userEmailAddress = "";
            if (userPersona.UserTypeId == (int)UserRoleType.UserNoEmail && contactMechansimList.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Get the email address if usertype is User(No Email)" });
                userEmailAddress = (from a in contactMechansimList where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
            }
            else if (contactMechansimList.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Get the email address" });
                userEmailAddress = (from a in contactMechansimList where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
            }
            else
            {
                userEmailAddress = userLogin.LoginName;
            }
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

            bool isSuperUser = IsSuperUser(userPersona.PersonaId);

            string userPhoneNumber = "555-555-5555";
            if (contactMechansimList.Any(a => a.AddressType?.Equals("PHONE", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true))
                userPhoneNumber = (from a in contactMechansimList where a.AddressType.Equals("PHONE", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();

            if (string.IsNullOrEmpty(_productUserId))
            {
                bool foundNewUserName = false;
                int incrementor = 0;
                string lastNameNoWhiteSpace = person.LastName.TrimWhiteSpace();
                string newproductUsername = (person.FirstName.TrimWhiteSpace().Substring(0, 1) + lastNameNoWhiteSpace.Substring(0, (lastNameNoWhiteSpace.Length >= 19 ? 19 : lastNameNoWhiteSpace.Length))).ToLower();
                _productUsername = newproductUsername;
                try
                {
                    while (!foundNewUserName)
                    {
                        if (CheckIfUserLoginIsUsed(_editorPersona.PersonaId, _productUsername))
                        {
                            incrementor++;
                            _productUsername = newproductUsername + incrementor;
                        }
                        else
                        {
                            foundNewUserName = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Get user. Problem checking the user information. Message {ex.Message}" });
                    return "There was a problem getting the user information.";
                }
            }

            if (!isSuperUser && (PropertyList.Count == 0 || RoleList.Count == 0))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user error. PropertyList.Count={PropertyList.Count}, RoleList.Count={RoleList.Count}" });
                return "There was a problem creating the user. Missing required information.";
            }

            ListResponse assetListResponse = GetCompanyAssetDetails(editorPersonaId, userPersonaId, includeDisabled: true, updateAssetNames: false, buildhierarchy: false);
            string assetType = (assetListResponse.Additional as string).ToUpper();
            string roleName = "";
            string assetCode = null;
            string assetName = null;

            try
            {
                switch (assetType)
                {
                    case "PORTFOLIO":
                        List<Portfolio> portfolioList = assetListResponse.Records.Cast<Portfolio>().ToList();
                        if (portfolioList.Any(a => (isSuperUser && string.IsNullOrEmpty(a.ParentAssetId)) || (!isSuperUser && a.ID == PropertyList[0].ToString())))
                        {
                            Portfolio p = portfolioList.FirstOrDefault(a => (isSuperUser && string.IsNullOrEmpty(a.ParentAssetId)) || (!isSuperUser && a.ID == PropertyList[0].ToString()));
                            if (p != null) { assetCode = p.Code; assetName = p.Name; }
                        }
                        break;
                    case "ASSETGROUPS":
                        List<AssetGroup> assetGroupList = assetListResponse.Records.Cast<AssetGroup>().ToList();
                        if (assetGroupList.Any(a => (isSuperUser && a.GroupType.Equals("COMPANY", StringComparison.OrdinalIgnoreCase)) || (!isSuperUser && a.ID == PropertyList[0].ToString())))
                        {
                            AssetGroup ag = assetGroupList.FirstOrDefault(a => (isSuperUser && a.GroupType.Equals("COMPANY", StringComparison.OrdinalIgnoreCase)) || (!isSuperUser && a.ID == PropertyList[0].ToString()));
                            if (ag != null) { assetCode = ag.Code; assetName = ag.Name; }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user. Problem assigning asset group. Message {ex.Message}" });
                return "There was a problem creating the user. Invalid asset group.";
            }

            if (string.IsNullOrEmpty(assetCode) && string.IsNullOrEmpty(assetName))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user. Invalid asset group" });
                return "There was a problem creating the user. Invalid asset group.";
            }

            ListResponse roleListResponse = isSuperUser
                ? GetRoles(editorPersonaId, userPersonaId, assetCode: null)
                : GetRoles(editorPersonaId, userPersonaId, assetCode: (assetType == "PORTFOLIO" ? assetCode : null));

            List<ProductRole> roleList = roleListResponse.Records.Cast<ProductRole>().ToList();
            ProductRole pr = roleList.FirstOrDefault(a => (isSuperUser && a.Roletype == "1") || (!isSuperUser && a.ID == RoleList[0].ToString()));
            if (pr != null) roleName = pr.Name;

            if (string.IsNullOrEmpty(roleName))
                return "There was a problem creating the user. Invalid role.";

            OpsUser userDetailsBeforeUpdate = !string.IsNullOrEmpty(_productUserId) ? GetUserDetailsById(Convert.ToInt32(_productUserId)) : null;

            OpsUser manageUser = new OpsUser()
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                EmployeeId = person.EmployeeId,
                Loginname = _productUsername,
                Password = PasswordGenerator.GeneratePassword(15, 5),
                RoleName = roleName,
                AssetCode = assetCode ?? "",
                AssetName = assetName,
                UserTypeId = null,
                AssetID = null,
                Email = userEmailAddress,
                Phone = userPhoneNumber,
                Status = "active"
            };

            if (string.IsNullOrEmpty(_productUserId))
            {
                try
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);
                    var url = _opsBuyerUrl + "/api/v1.0/users";
                    logData = new Dictionary<string, object> { { "url", url }, { "manageUser", JsonConvert.SerializeObject(manageUser) } };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user" }, logData: logData);

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var postResponse = SendWithSidAsync(HttpMethod.Post, url, manageUser, cts.Token).GetAwaiter().GetResult();

                    if (postResponse.IsSuccessStatusCode)
                    {
                        var userResult = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, userResult.Value<string>("login_name"));
                        string newid = userResult.id;
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user. newid={newid}" });
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user success. Set product status to Success" });
                        UpdateUsersMigrationStatus(editorPersonaId, new List<MigrateUser> { new MigrateUser { UserId = newid, UnifiedLoginUserName = userEmailAddress, UsingUnifiedLogin = true } });
                    }
                    else
                    {
                        string errorContent = postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        logData = new Dictionary<string, object> { { "postResponse.Content", errorContent } };
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user errored." }, logData: logData);
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                        return "There was a problem creating the user. " + errorContent;
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user errored. {ex.Message}" }, exception: ex);
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    return "There was a problem creating the user. " + ex.Message;
                }
            }
            else
            {
                try
                {
                    manageUser.ID = _productUserId;
                    manageUser.Password = null;
                    var url = _opsBuyerUrl + "/api/v1.0/users/" + _productUserId;
                    logData = new Dictionary<string, object> { { "url", url }, { "manageUser", manageUser } };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Update user" }, logData: logData);

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var putResponse = SendWithSidAsync(HttpMethod.Put, url, manageUser, cts.Token).GetAwaiter().GetResult();

                    if (putResponse.IsSuccessStatusCode)
                    {
                        var userResult = JsonConvert.DeserializeObject<dynamic>(putResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        string newid = userResult.id;
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Update user. newid={newid}" });
                        UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                    }
                    else
                    {
                        string errorContent = putResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Update user errored." }, logData: new Dictionary<string, object> { { "response", errorContent } });
                        return "There was a problem updating the user";
                    }
                }
                catch (Exception ex)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Update user errored. {ex.Message}" });
                    return "There was a problem updating the user";
                }
            }

            try
            {
                string oldRoleNameForActivity = string.Empty;
                if (!string.IsNullOrEmpty(userDetailsBeforeUpdate?.UserTypeId))
                    oldRoleNameForActivity = roleList.Find(f => f.ID == userDetailsBeforeUpdate.UserTypeId)?.Name;
                if (oldRoleNameForActivity != manageUser.RoleName)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", manageUser.RoleName) });
                    if (!string.IsNullOrEmpty(oldRoleNameForActivity))
                        additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", oldRoleNameForActivity) });
                }
                string grpNameForActivity = string.Empty;
                if (!string.IsNullOrEmpty(userDetailsBeforeUpdate?.AssetID))
                {
                    if (assetType == "PORTFOLIO")
                        grpNameForActivity = assetListResponse.Records.Cast<Portfolio>().ToList().Find(f => f.ID == userDetailsBeforeUpdate.AssetID)?.Name;
                    else if (assetType == "ASSETGROUPS")
                        grpNameForActivity = assetListResponse.Records.Cast<AssetGroup>().ToList().Find(f => f.AssetID == userDetailsBeforeUpdate.AssetID)?.Name;
                }
                if (grpNameForActivity != manageUser.AssetName)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Property Group", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", manageUser.AssetName) });
                    if (!string.IsNullOrEmpty(grpNameForActivity))
                        additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Property Group", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", grpNameForActivity) });
                }
            }
            catch (Exception e)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Error while building activity details for ops. {e.Message}" }, exception: e);
            }
            return "";
        }

        public string UpdateOPSUserProfile(long editorPersonaId, long userPersonaId)
        {
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOPSUserProfile", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
                IList<UserLoginPersona> userLoginPersonaList = _userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: persona.UserId, organizationPartyId: persona.Organization.PartyId);
                var employeeId = _userRepository.GetUserEmployeeId(userLoginPersonaList[0].UserLoginPersonaId, persona.OrganizationPartyId);
                person.EmployeeId = (employeeId != null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : null;

                IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
                IList<CommonAddress> contactMechansimList = contactMechanismLogic.ListContactMechanismForPerson(realPageId, null);
                string userEmailAddress = "";
                if (persona.UserTypeId == (int)UserRoleType.UserNoEmail && contactMechansimList.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOPSUserProfile", "Get the email address if usertype is User(No Email)" });
                    userEmailAddress = (from a in contactMechansimList where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
                }
                else if (contactMechansimList.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true))
                    userEmailAddress = (from a in contactMechansimList where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
                else
                    userEmailAddress = userLogin.LoginName;

                userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

                var userRepository = new UserRepository(_userClaims);
                var userDetails = userRepository.GetUserDetails(personaId: persona.PersonaId);

                OpsUserPatch manageUser = new OpsUserPatch()
                {
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    EmployeeId = person.EmployeeId,
                    Loginname = _productUsername,
                    Email = userEmailAddress,
                    Status = (userDetails.IsActive == true) ? "active" : "inactive"
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOPSUserProfile", "Update user profile" }, logData: new Dictionary<string, object> { { "manageUser", manageUser } });
                listResponse = PatchUserInfo(_productUserId, manageUser);
                if (listResponse.IsError) { return listResponse.ErrorReason; }

                WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
                return "";
            }
            catch (Exception ex)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOPSUserProfile", $"Update user profile {ex.Message}" });
                return "There was a problem updating the user";
            }
        }

        public override ListResponse DoAdditional(ListResponse response)
        {
            bool gotOpsGuid = GetOpsSessionGuid();
            if (!gotOpsGuid)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DoAdditional", "Unable to get Ops guid" });
                response = new ListResponse() { IsError = true, ErrorReason = "Unable to get Ops guid" };
            }
            return response;
        }

        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"userPersonaId:{userPersonaId}" });
            OpsUserPatch patchDetails = new OpsUserPatch() { Status = "inactive" };
            listResponse = PatchUserInfo(_productUserId, patchDetails);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
            return "";
        }

        public ListResponse GetUsers(long editorPersonaId, RequestParameter datafilter)
        {
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { return claimResposnse; }

            var startRow = 1;
            var resultPerRow = 100;
            var filters = "";
            if (datafilter != null)
            {
                filters = string.Join("&", datafilter.FilterBy.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                if (filters.Length > 0) filters = "&" + filters;
                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage <= 100 ? datafilter.Pages.ResultsPerPage : 100;
                }
            }

            var response = new ListResponse() { CurrentPage = startRow, RowsPerPage = resultPerRow, ErrorReason = "" };
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var url = _opsBuyerUrl + $"/api/v1.0/users?unify_login_status=all&page_number={startRow}&page_size={resultPerRow}{filters}";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", "Get user." }, logData: new Dictionary<string, object> { { "url", url } });
                var getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (getResponse.IsSuccessStatusCode)
                {
                    var json = getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var users = JsonConvert.DeserializeObject<OpsUsers>(json);
                    response.Records = users.UserList.Cast<object>().ToList();
                    response.TotalRows = users.Pagination.TotalRecords;
                    response.RowsPerPage = users.Pagination.PageSize;
                    response.CurrentPage = users.Pagination.PageNumber;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Got user result. usercount={users.UserList.Count()}" });
                }
            }
            catch (Exception ex)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Error {ex.Message}" });
                response.ErrorReason = ex.Message;
            }
            sw.Stop();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Elapsed execution time {sw.ElapsedMilliseconds}" });
            return response;
        }

        public bool ChangeUserStatus(long editorPersonaId, string userName, string productUserId, bool isActive = false)
        {
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { return false; }

            _productUserId = productUserId;
            OpsUser opsUser = GetUserDetailsById(Convert.ToInt32(_productUserId));
            if (opsUser == null) { return false; }

            opsUser.Email = null;
            opsUser.Phone = null;

            if (opsUser.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && isActive) { return true; }
            if (opsUser.Status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) && !isActive) { return true; }

            opsUser.Status = isActive ? "active" : "inactive";
            try
            {
                var result = UpdateOpsUserStatus(opsUser);
                if (result.Item1.IsSuccessStatusCode) { return true; }

                result.Item2["response"] = result.Item1.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {userName} by editorPersonaId = {editorPersonaId} from response" }, logData: result.Item2);
                return false;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {userName} by editorPersonaId = {editorPersonaId} from exception" }, exception: ex);
                return false;
            }
        }

        #region Migration
        public ListResponse GetMigrationUsers(long editorPersonaId, RequestParameter datafilter)
        {
            var response = new ListResponse() { IsError = true, ErrorReason = "No Users." };
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { response.ErrorReason = claimResposnse.ErrorReason; return response; }

            var startRow = 0;
            var resultPerRow = 1000;
            var filters = "inactive";
            var extras = "";
            if (datafilter != null)
            {
                if (datafilter.FilterBy.ContainsKey("filter"))
                {
                    filters = datafilter.FilterBy["filter"];
                    if (!datafilter.FilterBy["filter"].ToLower().Equals("all"))
                        filters = datafilter.FilterBy["filter"].ToLower().Equals("migrated") ? "active" : "inactive";
                    datafilter.FilterBy.Remove("filter");
                }
                extras = string.Join("&", datafilter.FilterBy.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                if (extras.Length > 0) extras = "&" + extras;
                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage;
                }
            }

            try
            {
                var url = _opsBuyerUrl + $"/api/v1.0/users?page_number={startRow}&page_size={resultPerRow}&unify_login_status={filters}{extras}";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Get user." }, logData: new Dictionary<string, object> { { "Url", url } });
                var getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (getResponse.IsSuccessStatusCode)
                {
                    var json = getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var users = JsonConvert.DeserializeObject<OpsUsers>(json);

                    if (users == null)
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                        return response;
                    }

                    var migrationUsers = new List<MigrationUser>();
                    foreach (var user in users.UserList)
                    {
                        var migrationUser = new MigrationUser
                        {
                            UserId = user.ID,
                            FirstName = user.FirstName,
                            MiddleName = user.MiddleName,
                            LastName = user.LastName,
                            Email = user.Email,
                            Username = user.Loginname,
                            Status = user.Status?.ToLower() == "active" ? "Active" : "Disabled",
                            Phone = user.Phone,
                            EmployeeId = user.EmployeeId
                        };
                        if (!string.IsNullOrWhiteSpace(user.AssetGroup?.ID))
                            migrationUser.Properties.Add(new MigrationProperty() { PropertyInstanceSourceId = user.AssetGroup.ID });
                        migrationUsers.Add(migrationUser);
                    }

                    response.Records = migrationUsers.Cast<object>().ToList();
                    response.ErrorReason = string.Empty;
                    response.IsError = false;
                    response.TotalRows = users.Pagination.TotalRecords;
                    response.RowsPerPage = users.Pagination.PageSize;
                    response.CurrentPage = users.Pagination.PageNumber;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Got user result." });
                }
            }
            catch (Exception ex)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error {ex.Message}" });
                response.ErrorReason = ex.Message;
            }
            return response;
        }

        public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
        {
            var migrateResponse = new MigrateResponse() { Status = false };
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { migrateResponse.Message = claimResposnse.ErrorReason; return migrateResponse; }

            var url = $"{_opsBuyerUrl}/api/v1.0/users";
            var opsMigrateusers = migrateUsers.Select(x => new OpsMigrateUser()
            {
                UserId = x.UserId,
                UnifiedLoginUserName = x.UnifiedLoginUserName,
                UsingUnifiedLogin = x.UsingUnifiedLogin ? 1 : 0
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(url),
                Content = new StringContent(JsonConvert.SerializeObject(opsMigrateusers))
            };
            request.Headers.TryAddWithoutValidation("sid", _currentSessionId);

            var httpResponse = _client.SendAsync(request, cts.Token).GetAwaiter().GetResult();
            var responseContent = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            var logData = new Dictionary<string, object>
            {
                { "Url", url }, { "Response", responseContent },
                { "EditorPersonaId", editorPersonaId }, { "MigratedUser", migrateUsers }
            };
            if (httpResponse.IsSuccessStatusCode)
            {
                var migrationResponse = JsonConvert.DeserializeObject<MigrateResponse>(responseContent);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "SendAsAsync success" }, logData: logData);
                migrateResponse.Message = migrationResponse.Message;
                migrateResponse.Status = migrationResponse.Status;
            }
            else
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "SendAsAsync Error" }, logData: logData);
                migrateResponse.Message = "Cannot update user status to migrated.";
            }
            return migrateResponse;
        }
        #endregion
        #endregion

        #region Privates
        private bool CheckIfUserLoginIsUsed(long editorPersonaId, string userLogin)
        {
            OpsUser user = GetUserDetailsByLoginName(userLogin);
            return user.Loginname?.ToUpper() == userLogin.ToUpper();
        }

        private OpsUser GetUserDetailsById(int userId) => GetUserDetails(null, userId);
        private OpsUser GetUserDetailsByLoginName(string userLogin) => GetUserDetails(userLogin, 0);

        private OpsUser GetUserDetails(string userLogin, int userId)
        {
            try
            {
                string url = _opsBuyerUrl + "/api/v1.0/users";
                if (!string.IsNullOrEmpty(userLogin)) url += "/0/?login_name=" + userLogin;
                if (userId != 0) url += "/" + userId;

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", "Get user." }, logData: new Dictionary<string, object> { { "url", url } });
                var getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (!getResponse.IsSuccessStatusCode) { return null; }

                var userResult = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                var user = new OpsUser()
                {
                    ID = userResult.id,
                    FirstName = userResult.first_name,
                    MiddleName = userResult.middle_name,
                    LastName = userResult.last_name,
                    Loginname = userResult.login_name,
                    AssetID = userResult.asset.id,
                    UserTypeId = userResult.user_type.id,
                    Status = userResult.status
                };
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", "Got user result." }, logData: new Dictionary<string, object> { { "user", user } });
                return user;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", $"Error {ex.Message}" }, exception: ex);
                throw new Exception("Unable to get ops user details.");
            }
        }

        private ListResponse GetRoles(long editorPersonaId, long userPersonaId, string assetCode)
        {
            ListResponse response = new ListResponse();
            string assetCodeUrl = "";

            if (!string.IsNullOrEmpty(assetCode))
            {
                ListResponse assetGroupResponse = GetCompanyAssetDetails(editorPersonaId, 0, includeDisabled: true, updateAssetNames: true, buildhierarchy: false);
                if (assetGroupResponse.IsError) { return assetGroupResponse; }

                switch ((assetGroupResponse.Additional as string).ToUpper())
                {
                    case "PORTFOLIO":
                        var portfolioList = assetGroupResponse.Records.Cast<Portfolio>().ToList();
                        if (portfolioList != null && portfolioList.Any(m => m.Code?.ToUpper() == assetCode.ToUpper()))
                            assetCodeUrl = "?asset_code=" + assetCode;
                        break;
                    case "ASSETGROUPS":
                        var assetGroupList = assetGroupResponse.Records.Cast<AssetGroup>().ToList();
                        if (assetGroupList != null && assetGroupList.Any(m => m.Code?.ToUpper() == assetCode.ToUpper()))
                            assetCodeUrl = "?asset_code=" + assetCode;
                        break;
                }
            }

            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/roles" + assetCodeUrl;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Getting roles." }, logData: new Dictionary<string, object> { { "url", url } });

                var apiResponse = GetAsync(url).GetAwaiter().GetResult();
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    IList<Role> rolesList = JsonConvert.DeserializeObject<IList<Role>>(jsonContent) ?? new List<Role>();
                    IList<ProductRole> list = rolesList.ToGBRoles() ?? new List<ProductRole>();

                    if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                    {
                        OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                        if (!string.IsNullOrEmpty(user.UserTypeId))
                        {
                            var pr = list.FirstOrDefault(a => a.ID == user.UserTypeId);
                            if (pr != null) pr.IsAssigned = true;
                        }
                    }
                    response = new ListResponse()
                    {
                        Records = list.Cast<object>().ToList(),
                        TotalRows = list.Count, RowsPerPage = list.Count, TotalPages = 1, ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error. {ex.Message}" }, exception: ex);
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the roles";
            }
            return response;
        }

        private ListResponse GetCompanyAssetDetails(long editorPersonaId, long userPersonaId, bool includeDisabled, bool updateAssetNames, bool buildhierarchy = true)
        {
            ListResponse result = new ListResponse();
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyAssetDetails", "Get company settings." }, logData: new Dictionary<string, object> { { "url", url } });
                var response = GetAsync(url).GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                    return new ListResponse() { IsError = true, ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage };

                var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                if (_moduleAssetGroups == 1)
                {
                    url = _opsBuyerUrl + "/api/v1.0/assets/groups";
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyAssetDetails", "Get asset groups." }, logData: new Dictionary<string, object> { { "url", url } });
                    response = GetAsync(url).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                        return new ListResponse() { IsError = true, ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage };

                    List<AssetGroup> assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyAssetDetails", "Got asset groups." });

                    if (!includeDisabled)
                        assetGroups = assetGroups.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                    if (updateAssetNames)
                        UpdateAssetGroupNames(assetGroups);

                    if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                    {
                        OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                        if (!string.IsNullOrEmpty(user.AssetID))
                        {
                            var ag = assetGroups.FirstOrDefault(a => a.AssetID == user.AssetID);
                            if (ag != null) ag.IsAssigned = true;
                        }
                    }
                    result = new ListResponse()
                    {
                        Records = assetGroups.Cast<object>().ToList(),
                        TotalRows = assetGroups.Count, RowsPerPage = assetGroups.Count, TotalPages = 1, ErrorReason = "", Additional = "AssetGroups"
                    };
                }
                else
                {
                    url = _opsBuyerUrl + "/api/v1.0/assets/portfolio";
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var portfolioResponse = SendWithSidAsync(HttpMethod.Get, url, null, cts.Token).GetAwaiter().GetResult();
                    List<Portfolio> portfolioList = JsonConvert.DeserializeObject<List<Portfolio>>(portfolioResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    portfolioList = portfolioList.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                    int totalRows = portfolioList.Count;

                    if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                    {
                        OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                        if (!string.IsNullOrEmpty(user.AssetID))
                        {
                            var p = portfolioList.FirstOrDefault(a => a.ID == user.AssetID);
                            if (p != null) p.IsAssigned = true;
                        }
                    }
                    if (buildhierarchy)
                    {
                        var portfolioListParents = portfolioList.Where(m => string.IsNullOrEmpty(m.ParentAssetId)).ToList();
                        foreach (Portfolio p in portfolioListParents) p.BuildTree(portfolioList);
                        result = new ListResponse()
                        {
                            Records = portfolioListParents.Cast<object>().ToList(),
                            TotalRows = totalRows, RowsPerPage = totalRows, TotalPages = 1, ErrorReason = "", Additional = "Portfolio"
                        };
                    }
                    else
                    {
                        result = new ListResponse()
                        {
                            Records = portfolioList.Cast<object>().ToList(),
                            TotalRows = totalRows, RowsPerPage = totalRows, TotalPages = 1, ErrorReason = "", Additional = "Portfolio"
                        };
                    }
                }
            }
            catch (Exception)
            {
                return new ListResponse() { IsError = true, ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage };
            }
            return result;
        }

        private void UpdateAssetGroupNames(List<AssetGroup> assetGroups)
        {
            assetGroups = assetGroups.OrderBy(e => e.Name).ToList();
            foreach (AssetGroup ag in assetGroups)
                ag.Name = ag.GroupType.ToUpper() == "PROPERTY" ? "[A] " + ag.Name : "[G] " + ag.Name;
        }

        private ListResponse PatchUserInfo(string userId, OpsUserPatch userPatch)
        {
            var response = new ListResponse() { ErrorReason = "" };
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/users/" + userId;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchUserInfo", "Patching User Info" }, logData: new Dictionary<string, object> { { "url", url }, { "manageUser", JsonConvert.SerializeObject(userPatch) } });

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                var req = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                req.Headers.TryAddWithoutValidation("sid", _currentSessionId);
                req.Content = new StringContent(JsonConvert.SerializeObject(userPatch), Encoding.Default, "application/json");

                var postResponse = _client.SendAsync(req, cts.Token).GetAwaiter().GetResult();
                if (postResponse.IsSuccessStatusCode)
                {
                    var json = postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchUserInfo", "Got patch result." }, logData: new Dictionary<string, object> { { "json", json } });
                }
            }
            catch (Exception ex)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchUserInfo", $"Error {ex.Message}" });
                response.ErrorReason = ex.Message;
            }
            return response;
        }

        private bool GetOpsSessionGuid()
        {
            if (string.IsNullOrEmpty(_editorProductUsername)) { return false; }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", "Getting session guid." });
            SessionRequest sr = new SessionRequest() { Login_name = _editorProductUsername };

            if (_manageOpsCache["opsSid_" + sr.Login_name] != null)
            {
                _currentSessionId = _manageOpsCache["opsSid_" + sr.Login_name] as string;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", "Got session guid from cache." });
                return true;
            }

            using MD5 md5 = MD5.Create();
            string key = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.Equals("APIKEY", StringComparison.OrdinalIgnoreCase)).Value));
            byte[] inputBytes = Encoding.Default.GetBytes(sr.Login_name + key);
            byte[] result = md5.ComputeHash(inputBytes);
            sr.Trust_key = BitConverter.ToString(result).ToLower().Replace("-", "");

            bool doneProcessing = false;
            int failedCount = 0;

            while (!doneProcessing)
            {
                try
                {
                    var url = _opsBuyerUrl + "/api/v1.0/sessions";
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var req = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(sr), Encoding.Default, "application/json")
                    };
                    var response = _client.SendAsync(req, cts.Token).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        var sessionResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        _currentSessionId = sessionResult.session.sid;
                        CacheItemPolicy policy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(SIDREFRESHTIMEMINUTES) };
                        _manageOpsCache.Set("opsSid_" + sr.Login_name, _currentSessionId, policy);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", $"Got session guid. failedCount {failedCount}" });
                        return true;
                    }
                    else
                    {
                        var errorBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", $"Failed to get session guid. failedCount {failedCount} ResponseCode: {response.StatusCode}" }, logData: new Dictionary<string, object> { { "responseBody", errorBody } });
                        failedCount += 1;
                    }

                    if (failedCount >= MAXRETRYCOUNT) { doneProcessing = true; }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", "Failed to get session guid." }, exception: ex);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Sends an HTTP request with the sid header applied per-request (thread-safe).
        /// </summary>
        private async Task<HttpResponseMessage> SendWithSidAsync(HttpMethod method, string url, object body, CancellationToken cancellationToken)
        {
            using var req = new HttpRequestMessage(method, url);
            req.Headers.TryAddWithoutValidation("sid", _currentSessionId);
            if (body != null)
                req.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.Default, "application/json");
            return await _client.SendAsync(req, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Used to get data async with retry logic and per-request sid injection.
        /// </summary>
        private async Task<HttpResponseMessage> GetAsync(string uri)
        {
            bool doneProcessing = false;
            int failedCount = 0;
            HttpResponseMessage response = new HttpResponseMessage();
            Dictionary<string, object> logData = new Dictionary<string, object> { { "uri", uri } };

            if (!GetOpsSessionGuid())
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));

            while (!doneProcessing)
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                req.Headers.TryAddWithoutValidation("sid", _currentSessionId);

                response = await _client.SendAsync(req, cts.Token).ConfigureAwait(false);
                doneProcessing = response.IsSuccessStatusCode;

                if (!doneProcessing)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        logData = new Dictionary<string, object>
                        {
                            { "error", await response.Content.ReadAsStringAsync().ConfigureAwait(false) },
                            { "status", response.StatusCode }
                        };
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAsync", "User not found." }, logData: logData);
                        doneProcessing = true;
                    }
                    else if (response.StatusCode != HttpStatusCode.Unauthorized)
                    {
                        logData = new Dictionary<string, object>
                        {
                            { "error", await response.Content.ReadAsStringAsync().ConfigureAwait(false) },
                            { "status", response.StatusCode }
                        };
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAsync", "Exiting after error." }, logData: logData);
                        doneProcessing = true;
                        throw new Exception($"Failed to get user information with URL {uri}");
                    }
                    else
                    {
                        _currentSessionId = "";
                        GetOpsSessionGuid();
                        failedCount += 1;
                    }

                    if (failedCount >= MAXRETRYCOUNT)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAsync", "Exiting after too many tries." }, logData: logData);
                        doneProcessing = true;
                        throw new Exception($"Failed to get user information after too many attempts with URL {uri}");
                    }
                }
            }
            return response;
        }

        private Tuple<HttpResponseMessage, Dictionary<string, object>> UpdateOpsUserStatus(OpsUser opsUser, bool isActive = false)
        {
            var url = _opsBuyerUrl + "/api/v1.0/users/" + _productUserId;
            var logData = new Dictionary<string, object> { { "url", url }, { "opsUser", opsUser } };
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOpsUserStatus", $"EnableUser productuserId = {_productUserId}, isActive = {isActive}" }, logData: logData);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
            var putResponse = SendWithSidAsync(HttpMethod.Put, url, opsUser, cts.Token).GetAwaiter().GetResult();
            return Tuple.Create(putResponse, logData);
        }

        private ListResponse GetRolesCountDetails(long editorPersonaId, string assetCode)
        {
            ListResponse response = new ListResponse();
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/roles";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", "Getting roles." }, logData: new Dictionary<string, object> { { "url", url } });

                var apiResponse = GetAsync(url).GetAwaiter().GetResult();
                if (apiResponse.IsSuccessStatusCode)
                {
                    IList<Role> rolesList = JsonConvert.DeserializeObject<IList<Role>>(apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()) ?? new List<Role>();
                    response = new ListResponse()
                    {
                        Records = rolesList.Cast<object>().ToList(),
                        TotalRows = rolesList.Count, RowsPerPage = rolesList.Count, TotalPages = 1, ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", $"Error {ex.Message}" }, exception: ex);
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the roles";
            }
            return response;
        }

        private ListResponse GetRightsDetails(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/rights";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetails", "Getting rights." }, logData: new Dictionary<string, object> { { "url", url } });

                var apiResponse = GetAsync(url).GetAwaiter().GetResult();
                if (apiResponse.IsSuccessStatusCode)
                {
                    RightGroup rightGroup = JsonConvert.DeserializeObject<RightGroup>(apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()) ?? new RightGroup();
                    List<MainGroup> list = EnableComplianceRights(rightGroup.ToRightsFormatForClient());
                    response = new ListResponse()
                    {
                        Records = list.Cast<object>().ToList(),
                        TotalRows = list.Count, RowsPerPage = list.Count, TotalPages = 1, ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetails", $"Error {ex.Message}" }, exception: ex);
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the rights";
            }
            return response;
        }

        private List<MainGroup> EnableComplianceRights(List<MainGroup> list)
        {
            foreach (var item in list)
            {
                if (item.mainName == "Compliance Setup")
                    foreach (var sub in item.subGroupList)
                        foreach (var right in sub.rightsList) { right.isAssigned = true; right.value = "1"; }
            }
            return list;
        }

        private ListResponse GetRightsDetailsForRole(long editorPersonaId, long roleId)
        {
            ListResponse response = new ListResponse();
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/roles/" + roleId;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetailsForRole", "Getting rights." }, logData: new Dictionary<string, object> { { "url", url } });

                var apiResponse = GetAsync(url).GetAwaiter().GetResult();
                if (apiResponse.IsSuccessStatusCode)
                {
                    RightGroupRole rightGroup = JsonConvert.DeserializeObject<RightGroupRole>(apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()) ?? new RightGroupRole();
                    List<MainGroup> list = rightGroup.rights.ToRightsFormatForClient();
                    response = new ListResponse()
                    {
                        Records = list.Cast<object>().ToList(),
                        TotalRows = list.Count, RowsPerPage = list.Count, TotalPages = 1, ErrorReason = ""
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetailsForRole", $"Error {ex.Message}" }, exception: ex);
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the rights";
            }
            return response;
        }

        private ListResponse GetOpsAssetGroupsDetails(long editorPersonaId, long userPersonaId, int assetGroupId = 0)
        {
            ListResponse result = new ListResponse();
            List<AssetGroup> assetGroups = new List<AssetGroup>();
            try
            {
                ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsAssetGroupsDetails", "Get company settings." }, logData: new Dictionary<string, object> { { "url", url } });
                HttpResponseMessage getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (!getResponse.IsSuccessStatusCode)
                    return new ListResponse() { IsError = true, ErrorReason = "There was a problem getting the asset group" };

                var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                if (_moduleAssetGroups == 1)
                {
                    url = _opsBuyerUrl + "/api/v1.0/assetgroups" + (assetGroupId > 0 ? $"/{assetGroupId}" : "");
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsAssetGroupsDetails", "Get asset groups." }, logData: new Dictionary<string, object> { { "url", url } });

                    getResponse = GetAsync(url).GetAwaiter().GetResult();
                    if (!getResponse.IsSuccessStatusCode)
                        return new ListResponse() { IsError = true, ErrorReason = "There was a problem getting the asset group." };

                    var responseBody = getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (assetGroupId == 0)
                    {
                        assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(responseBody);
                    }
                    else
                    {
                        AssetGroup assetGroup = JsonConvert.DeserializeObject<AssetGroup>(responseBody);
                        assetGroup.property_list.ToList().ForEach(p => p.Properties = null);
                        assetGroups.Add(assetGroup);
                    }

                    if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                    {
                        OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                        if (!string.IsNullOrEmpty(user.AssetID))
                        {
                            var ag = assetGroups.FirstOrDefault(a => a.AssetID == user.AssetID);
                            if (ag != null) ag.IsAssigned = true;
                        }
                    }
                    result = new ListResponse()
                    {
                        Records = assetGroups.Cast<object>().ToList(),
                        TotalRows = assetGroups.Count, RowsPerPage = assetGroups.Count, TotalPages = 1, ErrorReason = "", Additional = "AssetGroups"
                    };
                }
            }
            catch (Exception)
            {
                return new ListResponse() { IsError = true, ErrorReason = "There was a problem getting the asset group" };
            }
            return result;
        }

        private ListResponse GetOpsAssetDetails(long editorPersonaId, long userPersonaId, string status)
        {
            ListResponse result = new ListResponse();
            try
            {
                ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsAssetDetails", "Get company settings." }, logData: new Dictionary<string, object> { { "url", url } });
                HttpResponseMessage getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (!getResponse.IsSuccessStatusCode)
                    return new ListResponse() { IsError = true, ErrorReason = "There was a problem getting the asset group" };

                url = _opsBuyerUrl + "/api/v1.0/properties?status=" + (string.IsNullOrWhiteSpace(status) ? "all" : status);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                getResponse = SendWithSidAsync(HttpMethod.Get, url, null, cts.Token).GetAwaiter().GetResult();
                List<Portfolio> portfolioList = JsonConvert.DeserializeObject<List<Portfolio>>(getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                portfolioList.ForEach(p => p.Properties = null);
                portfolioList = portfolioList.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                int totalRows = portfolioList.Count;

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                {
                    OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                    if (!string.IsNullOrEmpty(user.AssetID))
                    {
                        var p = portfolioList.FirstOrDefault(a => a.ID == user.AssetID);
                        if (p != null) p.IsAssigned = true;
                    }
                }
                result = new ListResponse()
                {
                    Records = portfolioList.Cast<object>().ToList(),
                    TotalRows = totalRows, RowsPerPage = totalRows, TotalPages = 1, ErrorReason = "", Additional = "Portfolio"
                };
            }
            catch (Exception)
            {
                return new ListResponse() { IsError = true, ErrorReason = "There was a problem getting the asset group" };
            }
            return result;
        }

        private ListResponse CreateAssetGroup(long editorPersonaId, long userPersonaId, AssetGroupCreate assetGroup)
        {
            ListResponse listResponse = new ListResponse();
            IList<AssetGroup> assetGroupList = new List<AssetGroup>();
            try
            {
                ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", "Get company settings." }, logData: new Dictionary<string, object> { { "url", url } });
                HttpResponseMessage getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (!getResponse.IsSuccessStatusCode) { return listResponse; }

                var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                if (_moduleAssetGroups == 1)
                {
                    url = _opsBuyerUrl + "/api/v1.0/assetgroups";
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", "Create asset groups." }, logData: new Dictionary<string, object> { { "url", url } });

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var postResponse = SendWithSidAsync(HttpMethod.Post, url, assetGroup, cts.Token).GetAwaiter().GetResult();

                    if (postResponse.IsSuccessStatusCode)
                    {
                        AssetGroup postResult = JsonConvert.DeserializeObject<AssetGroup>(postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        postResult.Name = assetGroup.Name;
                        assetGroupList.Add(postResult);
                        listResponse = new ListResponse() { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };
                    }
                    else
                    {
                        string error = postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", "Error" }, logData: new Dictionary<string, object> { { "postResponse.Content", error } });
                        listResponse = new ListResponse() { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true };
                    }
                }
            }
            catch (Exception exception)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", $"Error {exception.Message}" }, exception: exception);
                listResponse = new ListResponse() { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "There was a problem creating the AssetGroup. " + exception.Message, IsError = true };
            }
            return listResponse;
        }

        private ListResponse UpdateAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupCreate assetGroup)
        {
            ListResponse listResponse = new ListResponse();
            IList<AssetGroup> assetGroupList = new List<AssetGroup>();
            try
            {
                ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", "Get company settings." }, logData: new Dictionary<string, object> { { "url", url } });
                HttpResponseMessage getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (!getResponse.IsSuccessStatusCode) { return listResponse; }

                var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                if (_moduleAssetGroups == 1)
                {
                    url = _opsBuyerUrl + $"/api/v1.0/assetgroups/{assetGroupId}";
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", "Update asset groups." }, logData: new Dictionary<string, object> { { "url", url } });

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var postResponse = SendWithSidAsync(HttpMethod.Put, url, assetGroup, cts.Token).GetAwaiter().GetResult();

                    if (postResponse.IsSuccessStatusCode)
                    {
                        AssetGroup postResult = JsonConvert.DeserializeObject<AssetGroup>(postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        postResult.Name = assetGroup.Name;
                        assetGroupList.Add(postResult);
                        listResponse = new ListResponse() { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };
                    }
                    else
                    {
                        string error = postResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", "Error" }, logData: new Dictionary<string, object> { { "postResponse.Content", error } });
                        listResponse = new ListResponse() { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = error, IsError = true };
                    }
                }
            }
            catch (Exception exception)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", $"Error. {exception.Message}" }, exception: exception);
                listResponse = new ListResponse() { Records = assetGroupList.Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "There was a problem updating the AssetGroup. " + exception.Message, IsError = true };
            }
            return listResponse;
        }

        private ListResponse PatchAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupPatch assetGroup)
        {
            ListResponse listResponse = new ListResponse();
            IList<AssetGroupPatch> assetGroupCreateList = new List<AssetGroupPatch> { assetGroup };
            try
            {
                ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", "Get company settings." }, logData: new Dictionary<string, object> { { "url", url } });
                HttpResponseMessage getResponse = GetAsync(url).GetAwaiter().GetResult();

                if (!getResponse.IsSuccessStatusCode) { return listResponse; }

                var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                if (_moduleAssetGroups == 1)
                {
                    url = _opsBuyerUrl + $"/api/v1.0/assetgroups/{assetGroupId}";
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", "Patch asset group." }, logData: new Dictionary<string, object> { { "url", url } });

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(HTTP_TIMEOUT_SECONDS));
                    var req = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    req.Headers.TryAddWithoutValidation("sid", _currentSessionId);
                    req.Content = new StringContent(JsonConvert.SerializeObject(assetGroup), Encoding.Default, "application/json");
                    var patchResponse = _client.SendAsync(req, cts.Token).GetAwaiter().GetResult();

                    if (patchResponse.IsSuccessStatusCode)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", $"Success result: {patchResponse.IsSuccessStatusCode}" });
                        listResponse = new ListResponse() { Records = assetGroupCreateList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = "", IsError = false };
                    }
                    else
                    {
                        string error = patchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", "Error" }, logData: new Dictionary<string, object> { { "postResponse.Content", error } });
                        listResponse = new ListResponse() { Records = assetGroupCreateList.Cast<object>().ToList(), TotalRows = 1, RowsPerPage = 1, TotalPages = 1, ErrorReason = error, IsError = true };
                    }
                }
            }
            catch (Exception exception)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", $"Error {exception.Message}" }, exception: exception);
                listResponse = new ListResponse() { Records = new List<AssetGroup>().Cast<object>().ToList(), TotalRows = 0, RowsPerPage = 0, TotalPages = 1, ErrorReason = "There was a problem patching the AssetGroup. " + exception.Message, IsError = true };
            }
            return listResponse;
        }
        #endregion
    }
}