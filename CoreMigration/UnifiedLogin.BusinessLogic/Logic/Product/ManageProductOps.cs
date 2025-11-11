using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Helpers;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Product.Ops;
using UnifiedLogin.SharedObjects.Landing.Product.Ops.Extensions;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.Migration;
using UnifiedLogin.SharedObjects.Product.Ops;
//using System.Web.Security;

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
        private DefaultUserClaim _userClaims;
        private const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
        private const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
        private const string PRODUCT_ROLE_DEC_UPDATE = "{\"action\":\"Role Description updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_InvoiceWorkflowTimeout_UPDATE = "{\"action\":\"Role InvoiceWorkflowTimeout updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_OrderWorkflowTimeout_UPDATE = "{\"action\":\"Role OrderWorkflowTimeout updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_OrderEndorseEmailReminderFlag_UPDATE = "{\"action\":\"Role OrderEndorseEmailReminderFlag updated\",\"value\":\"NewValue\"}";
        private const string PRODUCT_ROLE_InvoiceEndorseEmailReminderFlag_UPDATE = "{\"action\":\"Role InvoiceEndorseEmailReminderFlag updated\",\"value\":\"NewValue\"}";

        #region Ctor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ManageProductOps(DefaultUserClaim userClaims) : base((int)ProductEnum.OpsBuyer, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _editorRealPageId = userClaims.UserRealPageGuid;
            _userClaims = userClaims;
            _blueBook = new ManageBlueBook(userClaims);

            _opsBuyerUrl = _productInternalSettingList.First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value;
            _client.BaseAddress = new Uri(_opsBuyerUrl);
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaim"></param>
        /// <param name="httpMessageHandler"></param>
        /// <param name="client"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="samlRepository"></param>
        /// <param name="blueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="repository"></param>
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
            _opsBuyerUrl = _productInternalSettingList.First(a => a.Name.Equals("APIENDPOINT", StringComparison.OrdinalIgnoreCase)).Value; //"https://staging9.on-site.com/api/greenbook"; //
            _client.BaseAddress = new Uri(_opsBuyerUrl);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get Ops AssetGroups
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">Optional assetGroupId</param>
        /// <returns>ListResponse</returns>
        public ListResponse GetOpsAssetGroups(long editorPersonaId, long userPersonaId, int assetGroupId = 0)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                return response;
            }

            response = GetOpsAssetGroupsDetails(editorPersonaId, userPersonaId, assetGroupId);

            return response;
        }

        /// <summary>
        /// Used to get the list of assets
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="status">Used to remove the disabled assets from the result</param>
        /// <returns>ListResponse</returns>
        public ListResponse GetOpsAssets(long editorPersonaId, long userPersonaId, string status)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError) { return response; }

            response = GetOpsAssetDetails(editorPersonaId, userPersonaId, status);

            return response;
        }

        /// <summary>
        /// Create an Ops AssetGroup
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        public ListResponse CreateOpsAssetGroup(long editorPersonaId, long userPersonaId, AssetGroupCreate assetGroup)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                return response;
            }

            response = CreateAssetGroup(editorPersonaId, userPersonaId, assetGroup);

            return response;
        }

        /// <summary>
        /// Edit/Update an Ops AssetGroup
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        public ListResponse UpdateOpsAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupCreate assetGroup)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                return response;
            }

            response = UpdateAssetGroup(editorPersonaId, userPersonaId, assetGroupId, assetGroup);

            return response;
        }

        /// <summary>
        /// Update Asset Group Name/Status
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        public ListResponse PatchOpsAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupPatch assetGroup)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                return response;
            }

            response = PatchAssetGroup(editorPersonaId, userPersonaId, assetGroupId, assetGroup);

            return response;
        }

        /// <summary>
        /// Used to get the list of assets for the company
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="includeDisabled">Used to remove the disabled assets from the result</param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetCompanyAssets(long editorPersonaId, long userPersonaId, bool includeDisabled, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                return response;
            }

            response = GetCompanyAssetDetails(editorPersonaId, userPersonaId, includeDisabled, updateAssetNames: true);

            return response;
        }

        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="assetCode"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, string assetCode, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (response.IsError)
            {
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                return response;
            }

            response = GetRoles(editorPersonaId, userPersonaId, assetCode);

            return response;
        }

        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <returns></returns>
        public ListResponse GetRolesCount(long editorPersonaId, string assetCode)
        {
            ListResponse response = new ListResponse();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            response = GetRolesCountDetails(editorPersonaId, assetCode);

            return response;
        }

        /// <summary>
        /// Used to get a list of roles associated to the given right 
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="rightId"></param>
        /// <returns></returns>
        public ListResponse GetRolesForRight(long editorPersonaId, int rightId)
        {
            //RoleList roleListResult = new RoleList();
            ListResponse response = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            IList<ProductRole> list;

            try
            {
                ListResponse allRoles = GetRoles(editorPersonaId, editorPersonaId, string.Empty);
                logData.Add("allRoles", allRoles);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesForRight", $"Result from api" }, logData: logData);
                response = allRoles;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesForRight", $"Error" }, exception: ex);
                response = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };
            }

            return response;
        }

        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <returns></returns>
        public ListResponse GetRightsByRole(long editorPersonaId, long roleId)
        {
            ListResponse response = new ListResponse();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            response = GetRightsDetailsForRole(editorPersonaId, roleId);

            return response;
        }

        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>        
        /// <returns></returns>
        public ListResponse GetRights(long editorPersonaId)
        {
            ListResponse response = new ListResponse();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            response = GetRightsDetails(editorPersonaId);

            return response;
        }

        /// <summary>
        /// Used to enable/disable a user
        /// </summary>
        /// <param name="editorPersonaId">The user altering the user</param>
        /// <param name="userPersonaId">The user being altered</param>
        /// <param name="isActive">Status of the user</param>
        /// <param name="deleteUser">Is the user being deleted</param>
        /// <returns></returns>
        public string EnableUser(long editorPersonaId, long userPersonaId, bool isActive, bool deleteUser)
        {
            ListResponse listResponse = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            OpsUser opsUser = GetUserDetailsById(Convert.ToInt32(_productUserId));
            if (opsUser != null)
            {
                // set the email and phone to null so they don't get removed in Ops
                opsUser.Email = null;
                opsUser.Phone = null;

                if (opsUser.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && isActive)
                {
                    // don't need to update the user because they are already active
                    return "";
                }
                if (opsUser.Status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) && !isActive)
                {
                    // don't need to update the user because they are already inactive
                    return "";
                }

                opsUser.Status = (isActive ? "active" : "inactive");
                try
                {
                    var url = _opsBuyerUrl + "/api/v1.0/users/" + _productUserId;
                    logData = new Dictionary<string, object>();
                    logData.Add("url", url);
                    logData.Add("opsUser", opsUser);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "EnableUser", $"Updating user status. productuserId = {_productUserId}, isActive = {isActive.ToString()}" }, logData: logData);

                    var putResponse = _client.PutAsJsonAsync(url, opsUser).Result;

                    if (putResponse.IsSuccessStatusCode)
                    {
                        if (isActive)
                        {
                            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                        }
                        else
                        {
                            if (deleteUser)
                            {
                                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
                            }
                            else
                            {
                                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Inactive);
                            }
                        }
                    }
                    else
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("response", putResponse.Content.ReadAsStringAsync().Result);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "EnableUser", "Update user errored." }, logData: logData);
                        return "There was a problem updating the user status";
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "EnableUser", $"Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive.ToString()}" }, exception: ex);
                }
            }
            else
            {
                return "There was an error getting the user details";
            }
            return "";
        }

        /// <summary>
        /// Updated to create/update a user in Accounting
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="rightInput"></param>        
        /// <param name="roleId">The asset to assign to the user</param>
        /// <returns></returns>
        public ListResponse CreateRole(long editorPersonaId, OpsInput rightInput, long roleId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            Dictionary<string, object> logData = new Dictionary<string, object>();

            ListResponse roleListResponse = new ListResponse();
            var rightsToAdd = new List<string>();
            var rightsToRemove = new List<string>();

            List<object> rightsInput = new List<object>();
            ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
            foreach (var item in rightInput.rightsList)
            {
                Dictionary<string, string> resp = new Dictionary<string, string>();
                resp.Add("name", item.Name);
                resp.Add("value", item.Value);
                rightsInput.Add(resp);
                if (item.Value == "1")
                {
                    rightsToAdd.Add(item.Name);
                }
                else
                {
                    rightsToRemove.Add(item.Name);
                }
            }

            dynamic newRole = new
            {
                name = rightInput.RoleName,
                description = rightInput.RoleDesc,
                invoice_endorse_email_reminder_flag = rightInput.InvoiceEndorseEmailReminderFlag == "true" ? "1" : "0",
                //IsMarketPlaceAdmin = rightInput.IsMarketPlaceAdmin,
                order_workflow_timeout = rightInput.OrderWorkflowTimeout == "" ? "0" : rightInput.OrderWorkflowTimeout,
                invoice_workflow_timeout = rightInput.InvoiceWorkflowTimeout == "" ? "0" : rightInput.InvoiceWorkflowTimeout,
                // SupplierWorkflowTimeout = rightInput.SupplierWorkflowTimeout,
                order_endorse_email_reminder_flag = rightInput.OrderEndorseEmailReminderFlag == "true" ? "1" : "0",
                responsibility_list = rightsInput

            };

            if (roleId == 0)
            {
                // create role
                try
                {
                    var url = _opsBuyerUrl + "/api/v1.0/roles";
                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
                    req.Content = new StringContent(JsonConvert.SerializeObject(newRole), System.Text.Encoding.Default, "application/json");

                    logData = new Dictionary<string, object>();
                    logData.Add("url", url);
                    logData.Add("newRole", JsonConvert.SerializeObject(newRole));
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Creating role" }, logData: logData);

                    //var postResponse = _client.PostAsJsonAsync(url, manageUser).Result;
                    var postResponse = _client.SendAsync(req).Result;

                    if (postResponse.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<Role>(postResponse.Content.ReadAsStringAsync().Result);


                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", $"Create Role - result = {result}" });

                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Create role success. Set product status to Success" });

                        logData = new Dictionary<string, object>();
                        logData.Add("result", result);

                        IList<Role> rolesList = new List<Role>();
                        rolesList.Add(result);
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = rolesList.Count,
                            RowsPerPage = rolesList.Count,
                            TotalPages = 1,
                            ErrorReason = "",
                            IsError = false
                        };
                        unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, rightInput.RoleName, "ADD", "Spend Management", null, 13);
                        if(rightsToAdd.Any() || rightsToRemove.Any())
                        {
                            UpdateRightsToRoleLogMessage(editorPersonaId, rightInput.RoleName, rightsToAdd, rightsToRemove);
                        }
                    }
                    else
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("postResponse.Content", postResponse.Content.ReadAsStringAsync().Result);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Create role errored." }, logData: logData);
                        string error = postResponse.Content.ReadAsStringAsync().Result.ToString();
                        IList<Role> rolesList = new List<Role>();
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = rolesList.Count,
                            RowsPerPage = rolesList.Count,
                            TotalPages = 1,
                            ErrorReason = error,
                            IsError = true
                        };
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", $"Create role errored. {ex.Message}" }, exception: ex);

                    // write an error                    
                    string error = "There was a problem creating the role. " + ex.Message;
                    IList<Role> rolesList = new List<Role>();
                    roleListResponse = new ListResponse()
                    {
                        Records = rolesList.Cast<object>().ToList(),
                        TotalRows = rolesList.Count,
                        RowsPerPage = rolesList.Count,
                        TotalPages = 1,
                        ErrorReason = error,
                        IsError = true
                    };
                }
            }
            else
            {
                // update role
                try
                {
                    var rolesListResponse = GetRolesCountDetails(editorPersonaId, null);
                    var roleList = rolesListResponse.Records.Cast<Role>().ToList();
                    var oldRole = roleList.FirstOrDefault(r => r.Id == roleId.ToString());
                    var url = _opsBuyerUrl + "/api/v1.0/roles/" + roleId.ToString();
                    logData = new Dictionary<string, object>();
                    logData.Add("url", url);
                    logData.Add("CreateRole - Update", newRole);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Update user" }, logData: logData);

                    var putResponse = _client.PutAsJsonAsync(url, (Object)newRole).Result;

                    if (putResponse.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<Role>(putResponse.Content.ReadAsStringAsync().Result);

                        IList<Role> rolesList = new List<Role>();
                        rolesList.Add(result);
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = rolesList.Count,
                            RowsPerPage = rolesList.Count,
                            TotalPages = 1,
                            ErrorReason = "",
                            IsError = false
                        };
                        if (oldRole.Name != newRole.name)
                        {
                            unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, newRole.name, "UPDATE", "Spend Management", oldRole.Name, 13);
                        }
                        if(oldRole.Description != newRole.description)
                        {
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.Description, newRole.description, "Description");
                        }
                        if (oldRole.InvoiceWorkflowTimeout != newRole.invoice_workflow_timeout)
                        {
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.InvoiceWorkflowTimeout, newRole.invoice_workflow_timeout, "InvoiceWorkflowTimeout");
                        }
                        if (oldRole.OrderWorkflowTimeout != newRole.order_workflow_timeout)
                        {
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.OrderWorkflowTimeout, newRole.order_workflow_timeout, "OrderWorkflowTimeout");
                        }
                        if (oldRole.OrderEndorseEmailReminderFlag != newRole.order_endorse_email_reminder_flag)
                        {
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.OrderEndorseEmailReminderFlag, newRole.order_endorse_email_reminder_flag, "OrderEndorseEmailReminderFlag");
                        }
                        if (oldRole.InvoiceEndorseEmailReminderFlag != newRole.invoice_endorse_email_reminder_flag)
                        {
                            updateRoleLogMessage(editorPersonaId, rightInput.RoleName, oldRole.InvoiceEndorseEmailReminderFlag, newRole.invoice_endorse_email_reminder_flag, "InvoiceEndorseEmailReminderFlag");
                        }
                        if (rightsToAdd.Any() || rightsToRemove.Any())
                        {
                            UpdateRightsToRoleLogMessage(editorPersonaId, rightInput.RoleName, rightsToAdd, rightsToRemove);
                        }
                    }
                    else
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("response", putResponse.Content.ReadAsStringAsync().Result);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", "Update role errored in else" }, logData: logData);
                        string error = putResponse.Content.ReadAsStringAsync().Result.ToString();
                        IList<Role> rolesList = new List<Role>();
                        roleListResponse = new ListResponse()
                        {
                            Records = rolesList.Cast<object>().ToList(),
                            TotalRows = rolesList.Count,
                            RowsPerPage = rolesList.Count,
                            TotalPages = 1,
                            ErrorReason = error,
                            IsError = true
                        };
                    }
                }
                catch (Exception ex)
                {

                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateRole", $"Update role errored in exception. {ex.Message}" }, exception: ex);
                    string error = "There was a problem creating the role. " + ex.Message;
                    IList<Role> rolesList = new List<Role>();
                    roleListResponse = new ListResponse()
                    {
                        Records = rolesList.Cast<object>().ToList(),
                        TotalRows = rolesList.Count,
                        RowsPerPage = rolesList.Count,
                        TotalPages = 1,
                        ErrorReason = error,
                        IsError = true
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
            {
                foreach (var right in rightsToAdd)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_ASSIGN.Replace("RightName", right) });
                }
            }
            if (rightsToRemove != null)
            {
                foreach (var right in rightsToRemove)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_UNASSIGN.Replace("RightName", right) });
                }
            }
            var message = "";
            message = impersonatorUserInfo != null
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
            var message = "";
            if (fieldName == "Description")
            {
                message = impersonatorUserInfo != null
             ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated Description of {roleName} in Spend Management from {oldValue} to {newValue}."
           : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated Description of {roleName} in Spend Management from {oldValue} to {newValue}.";

                additionalParameters.Add(new AdditionalParameters { Key = roleName +"-" + oldValue, Value = PRODUCT_ROLE_DEC_UPDATE.Replace("NewValue", newValue) });
            }
            else if (fieldName == "InvoiceWorkflowTimeout")
            {
                message = impersonatorUserInfo != null
             ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated InvoiceWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}."
           : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated InvoiceWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}.";

                additionalParameters.Add(new AdditionalParameters { Key = roleName + "-" + oldValue, Value = PRODUCT_ROLE_InvoiceWorkflowTimeout_UPDATE.Replace("NewValue", newValue) });
            }
            else if (fieldName == "OrderWorkflowTimeout")
            {
                message = impersonatorUserInfo != null
             ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated OrderWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}."
           : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated OrderWorkflowTimeout of {roleName} in Spend Management from {oldValue} to {newValue}.";

                additionalParameters.Add(new AdditionalParameters { Key = roleName + "-" + oldValue, Value = PRODUCT_ROLE_OrderWorkflowTimeout_UPDATE.Replace("NewValue", newValue) });
            }
            else if (fieldName == "OrderEndorseEmailReminderFlag")
            {
                message = impersonatorUserInfo != null
             ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated OrderEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}."
           : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated OrderEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}.";

                additionalParameters.Add(new AdditionalParameters { Key = roleName + "-" + oldValue, Value = PRODUCT_ROLE_OrderEndorseEmailReminderFlag_UPDATE.Replace("NewValue", newValue) });
            }
            else if (fieldName == "InvoiceEndorseEmailReminderFlag")
            {
                message = impersonatorUserInfo != null
             ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated InvoiceEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}."
           : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} updated InvoiceEndorseEmailReminderFlag of {roleName} in Spend Management from {oldValue} to {newValue}.";

                additionalParameters.Add(new AdditionalParameters { Key = roleName + "-" + oldValue, Value = PRODUCT_ROLE_InvoiceEndorseEmailReminderFlag_UPDATE.Replace("NewValue", newValue) });
            }
            unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters, 13);
        }

        /// <summary>
        /// Updated to create/update a user in Accounting
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId"></param>
        /// <param name="RoleList">The role name to assign the user</param>
        /// <param name="PropertyList">The asset to assign to the user</param>
        /// <param name="additionalParameters"></param>
        /// <returns></returns>
        public string ManageOpsUser(long editorPersonaId, long userPersonaId, List<int> RoleList, List<int> PropertyList, out List<AdditionalParameters> additionalParameters)
        {
            ListResponse response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            additionalParameters = new List<AdditionalParameters>();
            if (response.IsError) { return response.ErrorReason; }

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid realPageId = userPersona.RealPageId;
            Dictionary<string, object> logData = new Dictionary<string, object>();

            Person person = _managePerson.GetPerson(realPageId);

            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            IList<UserLoginPersona> userLoginPersonaList = _userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userPersona.UserId, organizationPartyId: userPersona.Organization.PartyId);

            var employeeId = _userRepository.GetUserEmployeeId(userLoginPersonaList[0].UserLoginPersonaId, userPersona.OrganizationPartyId);
            person.EmployeeId = (employeeId != null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : null;

            IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
            IList<CommonAddress> contactMechansimList = contactMechanismLogic.ListContactMechanismForPerson(realPageId, null);

            // get the email address
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
                // this should probably look like a real email, need to test if it isn't
                userEmailAddress = userLogin.LoginName;
            }
            // verify email address looks valid, will fail if not
            userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);

            bool isSuperUser = IsSuperUser(userPersona.PersonaId);

            // get the user phone
            string userPhoneNumber = "555-555-5555";
            if (contactMechansimList.Any(a => a.AddressType?.Equals("PHONE", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true))
            {
                userPhoneNumber = (from a in contactMechansimList where a.AddressType.Equals("PHONE", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
            }

            if (string.IsNullOrEmpty(_productUserId))
            {
                // get a login name that isn't in use for the new user
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
                            _productUsername = newproductUsername + incrementor.ToString();
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
            string roleName = "";
            string assetCode = null;
            string assetName = null;

            if (!isSuperUser && (PropertyList.Count == 0 || RoleList.Count == 0))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user error. PropertyList.Count={PropertyList.Count}, RoleList.Count={RoleList.Count}" });
                return "There was a problem creating the user. Missing required information.";
            }

            // get the asset list and find the one passed
            ListResponse assetListResponse = GetCompanyAssetDetails(editorPersonaId, userPersonaId, includeDisabled: true, updateAssetNames: false, buildhierarchy: false);
            string assetType = (assetListResponse.Additional as string).ToUpper();
            try
            {
                switch (assetType)
                {
                    case "PORTFOLIO":
                        List<Portfolio> portfolioList = assetListResponse.Records.Cast<Portfolio>().ToList();
                        // if superuser, get the highest level portfolio, i.e. the one without a parent
                        // if not superuser, take what was passed to the call and assign it if it is found
                        if (portfolioList.Any(a => (
                                (isSuperUser && string.IsNullOrEmpty(a.ParentAssetId)) ||
                                (!isSuperUser && a.ID == PropertyList[0].ToString())))
                           )
                        {
                            Portfolio p = (from a in portfolioList
                                           where
                                           ((isSuperUser && string.IsNullOrEmpty(a.ParentAssetId)) ||
                                           (!isSuperUser && a.ID == PropertyList[0].ToString()))
                                           select a).FirstOrDefault();
                            if (p != null)
                            {
                                assetCode = p.Code;
                                assetName = p.Name;
                            }
                        }

                        break;
                    case "ASSETGROUPS":
                        List<AssetGroup> assetGroupList = assetListResponse.Records.Cast<AssetGroup>().ToList();
                        // if superuser, get the highest level asset, i.e. the one defined as company
                        // if not superuser, take what was passed to the call and assign it if it is found
                        if (assetGroupList.Any(a => (
                                 (isSuperUser && a.GroupType.Equals("COMPANY", StringComparison.OrdinalIgnoreCase)) ||
                                 (!isSuperUser && a.ID == PropertyList[0].ToString())))
                           )
                        {
                            AssetGroup ag = (from a in assetGroupList
                                             where
                                                ((isSuperUser && a.GroupType.Equals("COMPANY", StringComparison.OrdinalIgnoreCase)) ||
                                                (!isSuperUser && a.ID == PropertyList[0].ToString()))
                                             select a).FirstOrDefault();
                            if (ag != null)
                            {
                                assetCode = ag.Code;
                                assetName = ag.Name;
                            }
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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user. Invalid asset group" });
                return "There was a problem creating the user. Invalid asset group.";
            }

            ListResponse roleListResponse = new ListResponse();
            if (isSuperUser)
            {
                // set the superuser to be a marketplace admin
                roleListResponse = GetRoles(editorPersonaId, userPersonaId, assetCode: null);
            }
            else
            {
                // otherwise set the role to be what was passed in the UI
                roleListResponse = GetRoles(editorPersonaId, userPersonaId, assetCode: (assetType == "PORTFOLIO" ? assetCode : null));
            }
            List<ProductRole> roleList = roleListResponse.Records.Cast<ProductRole>().ToList();
            if (roleList.Any(a => (
                        (isSuperUser && a.Roletype == "1")) ||
                        (!isSuperUser && a.ID == RoleList[0].ToString())
                    )
                )
            {
                ProductRole pr = (from a in roleList
                                  where
                                    ((isSuperUser && a.Roletype == "1") ||
                                    (!isSuperUser && a.ID == RoleList[0].ToString()))
                                  select a).FirstOrDefault();
                if (pr != null)
                {
                    roleName = pr.Name;
                }
            }

            if (string.IsNullOrEmpty(roleName))
            {
                return "There was a problem creating the user. Invalid role.";
            }

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
                AssetCode = (string.IsNullOrEmpty(assetCode) ? "" : assetCode),
                AssetName = assetName,
                UserTypeId = null,
                AssetID = null,
                Email = userEmailAddress,
                Phone = userPhoneNumber,
                Status = "active"
            };

            if (string.IsNullOrEmpty(_productUserId))
            {
                // create user
                try
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);
                    var url = _opsBuyerUrl + "/api/v1.0/users";
                    HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);
                    req.Content = new StringContent(JsonConvert.SerializeObject(manageUser), System.Text.Encoding.Default, "application/json");

                    logData = new Dictionary<string, object>();
                    logData.Add("url", url);
                    logData.Add("manageUser", JsonConvert.SerializeObject(manageUser));
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user" }, logData: logData);

                    //var postResponse = _client.PostAsJsonAsync(url, manageUser).Result;
                    var postResponse = _client.SendAsync(req).Result;

                    if (postResponse.IsSuccessStatusCode)
                    {
                        var userResult = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, userResult.Value<string>("login_name"));
                        // now the id!
                        string newid = userResult.id;
                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user. newid={newid}" });
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user success. Set product status to Success" });

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
                        migrateResponse = UpdateUsersMigrationStatus(editorPersonaId, migrateUsers);
                    }
                    else
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("postResponse.Content", postResponse.Content.ReadAsStringAsync().Result);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user errored." }, logData: logData);
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user errored. Set product status to Error" });
                        // write an error
                        return "There was a problem creating the user. " + postResponse.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Create user errored. {ex.Message}" }, exception: ex);
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Create user errored. Set product status to Error" });
                    // write an error
                    return "There was a problem creating the user. " + ex.Message;
                }
            }
            else
            {
                // update user
                try
                {
                    manageUser.ID = _productUserId;
                    manageUser.Password = null;
                    var url = _opsBuyerUrl + "/api/v1.0/users/" + _productUserId;
                    logData = new Dictionary<string, object>();
                    logData.Add("url", url);
                    logData.Add("manageUser", manageUser);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Update user" },logData: logData);

                    var putResponse = _client.PutAsJsonAsync(url, manageUser).Result;

                    if (putResponse.IsSuccessStatusCode)
                    {
                        var userResult = JsonConvert.DeserializeObject<dynamic>(putResponse.Content.ReadAsStringAsync().Result);
                        string newid = userResult.id;
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Update user. newid={newid}" });
                        UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Update user success. Set product status to Success" });
                    }
                    else
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("response", putResponse.Content.ReadAsStringAsync().Result);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", "Update user errored." }, logData: logData);
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
                //build activity details
                if (!string.IsNullOrEmpty(userDetailsBeforeUpdate?.UserTypeId))
                {
                    oldRoleNameForActivity = roleList.Find(f => f.ID == userDetailsBeforeUpdate.UserTypeId)?.Name;
                }
                if (oldRoleNameForActivity != manageUser.RoleName)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", manageUser.RoleName) });
                    if (!string.IsNullOrEmpty(oldRoleNameForActivity))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", oldRoleNameForActivity) });
                    }
                }

                string grpNameForActivity = string.Empty;
                if (!string.IsNullOrEmpty(userDetailsBeforeUpdate?.AssetID))
                {
                    if (assetType == "PORTFOLIO")
                    {
                        grpNameForActivity = assetListResponse.Records.Cast<Portfolio>().ToList().Find(f => f.ID == userDetailsBeforeUpdate.AssetID)?.Name;
                    }
                    else if (assetType == "ASSETGROUPS")
                    {
                        grpNameForActivity = assetListResponse.Records.Cast<AssetGroup>().ToList().Find(f => f.AssetID == userDetailsBeforeUpdate.AssetID)?.Name;
                    }
                }
                if (grpNameForActivity != manageUser.AssetName)
                {
                    additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Property Group", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", manageUser.AssetName) });
                    if (!string.IsNullOrEmpty(grpNameForActivity))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "Spend Management Property Group", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", grpNameForActivity) });
                    }
                }
            }
            catch(Exception e)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOpsUser", $"Error while building activity details for ops. {e.Message}" }, exception: e);
            }

            return "";
        }

        /// <summary>
        /// Updates user profile  
        /// </summary>
        public string UpdateOPSUserProfile(long editorPersonaId, long userPersonaId)
        {
            try
            {
                Dictionary<string, object> logData = new Dictionary<string, object>();
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
                // get the email address
                string userEmailAddress = "";
                if (persona.UserTypeId == (int)UserRoleType.UserNoEmail && contactMechansimList.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOPSUserProfile", "Get the email address if usertype is User(No Email)" });
                    userEmailAddress = (from a in contactMechansimList where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
                }
                else if (contactMechansimList.Any(a => a.AddressType?.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) == true && a.contactMechanismUsageType?.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) == true))
                {
                    userEmailAddress = (from a in contactMechansimList where a.AddressType.Equals("EMAIL", StringComparison.OrdinalIgnoreCase) && a.contactMechanismUsageType.Name.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase) select a.AddressString).FirstOrDefault();
                }
                else
                {
                    // this should probably look like a real email, need to test if it isn't
                    userEmailAddress = userLogin.LoginName;
                }
                // verify email address looks valid, will fail if not
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

                //manageUser.ID = _productUserId;
                //manageUser.Password = null;
                logData = new Dictionary<string, object>();
                logData.Add("manageUser", manageUser);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOPSUserProfile", "Update user profile" }, logData: logData);

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

        /// <summary>
        /// Do additional setup for Ops
        /// </summary>
        /// <param name="response"></param>
        public override ListResponse DoAdditional(ListResponse response)
        {
            bool gotOpsGuid = GetOpsSessionGuid();
            if (!gotOpsGuid)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DoAdditional", "Unable to get Ops guid" });
                response = new ListResponse();
                response.IsError = true;
                response.ErrorReason = "Unable to get Ops guid";
            }
            return response;
        }

        /// <summary>
        /// Unassign User
        /// </summary> 
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>string</returns>
        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }


            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"userPersonaId:{userPersonaId}" });
            OpsUserPatch patchDetails = new OpsUserPatch() { Status = "inactive" };
            listResponse = PatchUserInfo(_productUserId, patchDetails);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);


            return "";
        }

        /// <summary>
        /// List all users
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="datafilter">datafilter</param>
		/// <returns>ListResponse</returns>
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
                if (filters.Length > 0)
                {
                    filters = "&" + filters;
                }
                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerRow = datafilter.Pages.ResultsPerPage <= 100 ? datafilter.Pages.ResultsPerPage : 100;
                }
            }

            var response = new ListResponse()
            {
                CurrentPage = startRow,
                RowsPerPage = resultPerRow,
                ErrorReason = ""
            };
            Dictionary<string, object> logData = new Dictionary<string, object>();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var url = _opsBuyerUrl + $"/api/v1.0/users?unify_login_status=all&page_number={startRow}&page_size={resultPerRow}{filters}";
                logData = new Dictionary<string, object>() { { "url", url } };
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", "Get user." }, logData: logData);
                var getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    var json = getResponse.Content.ReadAsStringAsync().Result;
                    var users = JsonConvert.DeserializeObject<OpsUsers>(json);
                    response.Records = users.UserList.Cast<object>().ToList();
                    response.TotalRows = users.Pagination.TotalRecords;
                    response.RowsPerPage = users.Pagination.PageSize;
                    response.CurrentPage = users.Pagination.PageNumber;
                    logData = new Dictionary<string, object>() { { "user", users } };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Got user result. usercount{users.UserList.Count()}" }, logData: logData);
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Error {ex.Message}" });
                response.ErrorReason = ex.Message;
            }
            sw.Stop();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Elapsed execution time {sw.ElapsedMilliseconds}" });

            return response;
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
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { return false; }

            _productUserId = productUserId;
            OpsUser opsUser = GetUserDetailsById(Convert.ToInt32(_productUserId));
            if (opsUser != null)
            {
                // set the email and phone to null so they don't get removed in Ops
                opsUser.Email = null;
                opsUser.Phone = null;

                if (opsUser.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) && isActive)
                {
                    // don't need to update the user because they are already active
                    return true;
                }
                if (opsUser.Status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) && !isActive)
                {
                    // don't need to update the user because they are already inactive
                    return true;
                }

                opsUser.Status = (isActive ? "active" : "inactive");
                try
                {
                    var response = UpdateOpsUserStatus(opsUser);
                    if (response.Item1.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        response.Item2.Add("response", response.Item1.Content.ReadAsStringAsync().Result);
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {userName} by editorPersonaId = {editorPersonaId} from response" }, logData: response.Item2);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {userName} by editorPersonaId = {editorPersonaId} from exception" }, exception: ex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #region Migration        
        /// <summary>
        /// List all users
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="datafilter">datafilter</param>
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
                    {
                        filters = datafilter.FilterBy["filter"].ToLower().Equals("migrated") ? "active" : "inactive";
                    }
                    datafilter.FilterBy.Remove("filter");
                }
                extras = string.Join("&", datafilter.FilterBy.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                if (extras.Length > 0)
                {
                    extras = "&" + extras;
                }
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

                var getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    var json = getResponse.Content.ReadAsStringAsync().Result;
                    var users = JsonConvert.DeserializeObject<OpsUsers>(json);

                    if (users == null)
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                        return response;
                    }

                    var migrationUsers = new List<MigrationUser>();
                    foreach (var user in users.UserList)
                    {
                        var migrationUser = new MigrationUser();
                        migrationUser.UserId = user.ID;
                        migrationUser.FirstName = user.FirstName;
                        migrationUser.MiddleName = user.MiddleName;
                        migrationUser.LastName = user.LastName;
                        migrationUser.Email = user.Email;
                        migrationUser.Username = user.Loginname;
                        migrationUser.Status = user.Status?.ToLower() == "active" ? "Active" : "Disabled";
                        migrationUser.Phone = user.Phone;
                        migrationUser.EmployeeId = user.EmployeeId;
                        if (!string.IsNullOrWhiteSpace(user.AssetGroup?.ID))
                        {
                            migrationUser.Properties.Add(new MigrationProperty() { PropertyInstanceSourceId = user.AssetGroup.ID });
                        }
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
                // return the user exists
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error {ex.Message}" });
                response.ErrorReason = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Update the users migration status
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="migrateUsers">List of Migrated user(s)</param>
		/// <returns>MigrateResponse</returns>
        public MigrateResponse UpdateUsersMigrationStatus(long editorPersonaId, IList<MigrateUser> migrateUsers)
        {
            var migrateResponse = new MigrateResponse()
            {
                Status = false
            };

            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { migrateResponse.Message = claimResposnse.ErrorReason; return migrateResponse; }

            var url = $"{_opsBuyerUrl}/api/v1.0/users";
            var opsMigrateusers = migrateUsers.Select(x => new OpsMigrateUser()
            {
                UserId = x.UserId,
                UnifiedLoginUserName = x.UnifiedLoginUserName,
                UsingUnifiedLogin = x.UsingUnifiedLogin ? 1 : 0
            });
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                RequestUri = new Uri(url),
                Content = new StringContent(JsonConvert.SerializeObject(opsMigrateusers))
            };
            var response = _client.SendAsync(request).Result;
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
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "SendAsAsync success" }, logData: logData);
                migrateResponse.Message = migrationResponse.Message;
                migrateResponse.Status = migrationResponse.Status;
                return migrateResponse;
            }
            else
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "SendAsAsync Error" }, logData: logData);
                migrateResponse.Message = "Cannot update user status to migrated.";
                return migrateResponse;
            }
        }
        #endregion
        #endregion

        #region Privates
        /// <summary>
        /// Used to see if a new user login being added already exists or not
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        private bool CheckIfUserLoginIsUsed(long editorPersonaId, string userLogin)
        {
            //ListResponse response = new ListResponse();
            //response = GetCompanyEditorAndUserDetails(editorPersonaId, 0);

            bool userExists = false;
            OpsUser user = GetUserDetailsByLoginName(userLogin);

            if (user.Loginname?.ToUpper() == userLogin.ToUpper())
            {
                userExists = true;
            }
            return userExists;
        }

        private OpsUser GetUserDetailsById(int userId)
        {
            return GetUserDetails(null, userId);
        }

        private OpsUser GetUserDetailsByLoginName(string userLogin)
        {
            return GetUserDetails(userLogin, 0);
        }

        /// <summary>
        /// Used to get details about an Ops user
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private OpsUser GetUserDetails(string userLogin, int userId)
        {
            OpsUser user = new OpsUser();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            try
            {
                string url = _opsBuyerUrl + "/api/v1.0/users";
                if (!string.IsNullOrEmpty(userLogin))
                {
                    url += "/0/?login_name=" + userLogin;
                }
                if (userId != 0)
                {
                    url += "/" + userId;
                }
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", "Get user." }, logData: logData);
                var getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    var userResult = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().Result);
                    user = new OpsUser()
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
                    logData = new Dictionary<string, object>();
                    logData.Add("user", user);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", "Got user result." }, logData: logData);
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", $"Error {ex.Message}" }, exception: ex);
                throw new Exception("Unable to get ops user details.");
            }

            return user;
        }

        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="assetCode"></param>
        /// <returns></returns>
        private ListResponse GetRoles(long editorPersonaId, long userPersonaId, string assetCode)
        {
            ListResponse response = new ListResponse();
            string assetCodeUrl = "";
            Dictionary<string, object> logData = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(assetCode))
            {
                // need to verify the assetGroup passed matches the company
                ListResponse assetGroupResponse = GetCompanyAssetDetails(editorPersonaId, 0, includeDisabled: true, updateAssetNames: true, buildhierarchy: false);
                if (assetGroupResponse.IsError)
                {
                    return assetGroupResponse;
                }

                switch ((assetGroupResponse.Additional as string).ToUpper())
                {
                    case "PORTFOLIO":
                        List<Portfolio> portfolioList = assetGroupResponse.Records.Cast<Portfolio>().ToList();
                        if (portfolioList != null && portfolioList.Any(m => m.Code?.ToUpper() == assetCode.ToUpper()))
                        {
                            assetCodeUrl = "?asset_code=" + assetCode;
                        }

                        break;
                    case "ASSETGROUPS":
                        List<AssetGroup> assetGroupList = assetGroupResponse.Records.Cast<AssetGroup>().ToList();
                        if (assetGroupList != null && assetGroupList.Any(m => m.Code?.ToUpper() == assetCode.ToUpper()))
                        {
                            assetCodeUrl = "?asset_code=" + assetCode;
                        }
                        break;
                }
            }

            IList<Role> rolesList = new List<Role>();

            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/roles" + assetCodeUrl;
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Getting roles." }, logData: logData);

                //var apiResponse = _client.GetAsync(url).Result;
                var apiResponse = GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rolesList = JsonConvert.DeserializeObject<IList<Role>>(jsonContent);
                    logData = new Dictionary<string, object>();
                    logData.Add("rolesList", rolesList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Got roles. " }, logData: logData);

                    if (rolesList == null) { rolesList = new List<Role>(); }
                    IList<ProductRole> list = rolesList.ToGBRoles();
                    if (list == null) { list = new List<ProductRole>(); }

                    // flag any roles that are assigned to the given user
                    if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                    {
                        // flag the role assigned to the given user
                        OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                        if (!string.IsNullOrEmpty(user.UserTypeId))
                        {
                            if (list.Any(a => a.ID == user.UserTypeId))
                            {
                                ProductRole pr = (from a in list
                                                  where a.ID == user.UserTypeId
                                                  select a).FirstOrDefault();
                                if (pr != null)
                                {
                                    pr.IsAssigned = true;
                                }
                            }
                        }
                    }

                    logData = new Dictionary<string, object>();
                    logData.Add("list", list);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", "Returning roles." }, logData: logData);
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error. {ex.Message}" }, exception: ex);
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the roles";
            }
            return response;
        }

        /// <summary>
        /// Used to get the list of assets for the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="includeDisabled">Include disabled assets in the result</param>
        /// <param name="updateAssetNames">Update the asset names to include the types</param>
        /// <param name="buildhierarchy">Used to return assets in hierarchical form when needed</param>
        private ListResponse GetCompanyAssetDetails(long editorPersonaId, long userPersonaId, bool includeDisabled, bool updateAssetNames, bool buildhierarchy = true)
        {
            ListResponse result = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyAssetDetails", "Get company settings." }, logData: logData);
                var response = GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                    _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                    if (_moduleAssetGroups == 1)
                    {
                        // get the asset groups for the company
                        url = _opsBuyerUrl + "/api/v1.0/assets/groups";
                        //req = new HttpRequestMessage(HttpMethod.Get, url);

                        logData = new Dictionary<string, object>();
                        logData.Add("url", url);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyAssetDetails", "Get asset groups. " }, logData: logData);

                        response = GetAsync(url).Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorMessage = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                            // write an error
                            result = new ListResponse()
                            {
                                IsError = true,
                                ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage
                            };
                            return result;
                        }
                        List<AssetGroup> assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(response.Content.ReadAsStringAsync().Result);
                        logData = new Dictionary<string, object>();
                        logData.Add("assetGroups", assetGroups);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetCompanyAssetDetails", "Got asset groups." }, logData: logData);

                        if (!includeDisabled)
                        {
                            assetGroups = assetGroups.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                        }
                        if (updateAssetNames)
                        {
                            UpdateAssetGroupNames(assetGroups);
                        }

                        if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                        {
                            // flag the role assigned to the given user
                            OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                            if (!string.IsNullOrEmpty(user.AssetID))
                            {
                                if (assetGroups.Any(a => a.AssetID == user.AssetID))
                                {
                                    AssetGroup ag = (from a in assetGroups
                                                     where a.AssetID == user.AssetID
                                                     select a).FirstOrDefault();
                                    if (ag != null)
                                    {
                                        ag.IsAssigned = true;
                                    }
                                }
                            }
                        }

                        result = new ListResponse()
                        {
                            Records = assetGroups.Cast<object>().ToList(),
                            TotalRows = assetGroups.Count,
                            RowsPerPage = assetGroups.Count,
                            TotalPages = 1,
                            ErrorReason = "",
                            Additional = "AssetGroups"
                        };
                    }
                    else
                    {
                        // Keep this block of code in case we ever want to use it later

                        // get the portfolios
                        // get the asset groups for the company
                        url = _opsBuyerUrl + "/api/v1.0/assets/portfolio";
                        response = _client.GetAsync(url).Result;
                        List<Portfolio> portfolioList = JsonConvert.DeserializeObject<List<Portfolio>>(response.Content.ReadAsStringAsync().Result);
                        // only keep active portfolios
                        portfolioList = portfolioList.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                        int totalRows = portfolioList.Count();

                        if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                        {
                            // flag the role assigned to the given user
                            OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                            if (!string.IsNullOrEmpty(user.AssetID))
                            {
                                if (portfolioList.Any(a => a.ID == user.AssetID))
                                {
                                    Portfolio p = (from a in portfolioList
                                                   where a.ID == user.AssetID
                                                   select a).FirstOrDefault();
                                    if (p != null)
                                    {
                                        p.IsAssigned = true;
                                    }
                                }
                            }
                        }

                        if (buildhierarchy)
                        {
                            List<Portfolio> portfolioListParents = new List<Portfolio>();
                            if (portfolioList.Any(m => string.IsNullOrEmpty(m.ParentAssetId)))
                            {
                                portfolioListParents = portfolioList.Where(m => string.IsNullOrEmpty(m.ParentAssetId)).ToList();
                            }
                            foreach (Portfolio p in portfolioListParents)
                            {
                                p.BuildTree(portfolioList);
                            }

                            result = new ListResponse()
                            {
                                Records = portfolioListParents.Cast<object>().ToList(),
                                TotalRows = totalRows,
                                RowsPerPage = totalRows,
                                TotalPages = 1,
                                ErrorReason = "",
                                Additional = "Portfolio"
                            };
                        }
                        else
                        {
                            result = new ListResponse()
                            {
                                Records = portfolioList.Cast<object>().ToList(),
                                TotalRows = totalRows,
                                RowsPerPage = totalRows,
                                TotalPages = 1,
                                ErrorReason = "",
                                Additional = "Portfolio"
                            };

                        }
                    }
                }
                else
                {
                    // write an error
                    result = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                // write an error
                result = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage
                };
                return result;
            }
            return result;
        }

        private void UpdateAssetGroupNames(List<AssetGroup> assetGroups)
        {
            // first, sort the list by the original name
            assetGroups = assetGroups.OrderBy(e => e.Name).ToList();
            // now add the [A] for asset or [G] for group to each entry
            foreach (AssetGroup ag in assetGroups)
            {
                switch (ag.GroupType.ToUpper())
                {
                    case "PROPERTY":
                        ag.Name = "[A] " + ag.Name;
                        break;
                    default:
                        ag.Name = "[G] " + ag.Name;
                        break;
                }
            }
        }

        /// <summary>
        /// Used to update portions of a users Ops information. 
        /// </summary>
        /// <param name="userId">The product user id to be patched</param>
        /// <param name="userPatch">Attributes to be patched for the given user</param>
        /// <returns>ListResponse</returns>
        private ListResponse PatchUserInfo(string userId, OpsUserPatch userPatch)
        {
            var response = new ListResponse() { ErrorReason = "" };
            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/users/" + userId;
                var method = new HttpMethod("PATCH");

                HttpRequestMessage req = new HttpRequestMessage(method, url);
                req.Content = new StringContent(JsonConvert.SerializeObject(userPatch), System.Text.Encoding.Default, "application/json");

                Dictionary<string, object> logData = new Dictionary<string, object>() { { "url", url }, { "manageUser", JsonConvert.SerializeObject(userPatch) } };
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchUserInfo", "Patching User Info" }, logData: logData);

                var postResponse = _client.SendAsync(req).Result;

                if (postResponse.IsSuccessStatusCode)
                {
                    var json = postResponse.Content.ReadAsStringAsync().Result;
                    logData = new Dictionary<string, object>() { { "json", json } };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchUserInfo", "Got patch result." }, logData: logData);
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchUserInfo", $"Error {ex.Message}" });
                response.ErrorReason = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Get the session token needed to make the call to Ops
        /// </summary>
        /// <returns>boolean</returns>
        private bool GetOpsSessionGuid()
        {
            if (string.IsNullOrEmpty(_editorProductUsername))
            {
                return false;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", "Getting session guid." });
            SessionRequest sr = new SessionRequest() { Login_name = _editorProductUsername };

            if (!(_manageOpsCache["opsSid_" + sr.Login_name] == null))
            {
                _currentSessionId = _manageOpsCache["opsSid_" + sr.Login_name] as string;
                AddSidToClient();
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", "Got session guid from cache." });
                return true;
            }

            MD5 md5 = new MD5CryptoServiceProvider();
            string key = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.Equals("APIKEY", StringComparison.OrdinalIgnoreCase)).Value));

            byte[] inputBytes = Encoding.Default.GetBytes(sr.Login_name + key);
            byte[] result = md5.ComputeHash(inputBytes);
            sr.Trust_key = System.BitConverter.ToString(result).ToLower().Replace("-", "");
            bool doneProcessing = false;
            int failedCount = 0;

            while (!doneProcessing)
            {
                try
                {
                    var url = _opsBuyerUrl + "/api/v1.0/sessions";
                    _client.DefaultRequestHeaders.Clear();
                    var response = _client.PostAsJsonAsync(url, sr).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var sessionResult = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                        _currentSessionId = sessionResult.session.sid;
                        CacheItemPolicy policy = new CacheItemPolicy();
                        policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(SIDREFRESHTIMEMINUTES);
                        _manageOpsCache.Set("opsSid_" + sr.Login_name, _currentSessionId, policy);
                        AddSidToClient();
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", $"Got session guid. failedCount {failedCount}" });
                        return true;
                    }
                    else
                    {
                        Dictionary<string, object> logData = new Dictionary<string, object> { { "response", response } };
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsSessionGuid", $"Failed to get session guid. failedCount {failedCount} ResponseCode: {response.StatusCode}" });
                        //return false;
                        failedCount += 1;
                    }

                    if (failedCount >= MAXRETRYCOUNT)
                    {
                        doneProcessing = true;
                    }
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
        /// Used to add the current sid to http client
        /// </summary>
        private void AddSidToClient()
        {
            if (_client.DefaultRequestHeaders.Contains("sid"))
            {
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("sid", _currentSessionId);
            }
            else
            {
                _client.DefaultRequestHeaders.Add("sid", _currentSessionId);
            }
        }

        /// <summary>
		/// Get Ops CompanyInstanceId
        /// </summary>
		/// <returns>CompanyMap</returns>
        private CustomerCompanyMap GetMarketingCenterCompanyInstanceId()
        {
            return GetProductCompanyInstanceId(BlueBookProductConstants.MarketingCenter, "companyInstance.attributes");
        }

        /// <summary>
        /// Used to get data async with retry
        /// </summary>
		/// <param name="uri">The Uri the request is sent to</param>
		/// <returns>The task object representing the asynchronous operation</returns>
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

            while (!doneProcessing)
            {
                response = _client.GetAsync(uri).Result;
                doneProcessing = response.IsSuccessStatusCode;
                if (!doneProcessing)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("error", response.Content.ReadAsStringAsync().Result);
                        logData.Add("status", response.StatusCode);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAsync", "User not found." }, logData: logData);
                        doneProcessing = true;
                    }
                    else if (!(response.StatusCode == HttpStatusCode.Unauthorized))
                    {
                        logData = new Dictionary<string, object>();
                        logData.Add("error", response.Content.ReadAsStringAsync().Result);
                        logData.Add("status", response.StatusCode);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAsync", "Exiting after error." }, logData: logData);
                        doneProcessing = true;
                        throw new Exception($"Failed to get user information with URL {uri}");
                    }
                    else
                    {
                        // reset the token so it gets a new one if we got an unauthorized error
                        //_cache.Remove("sid");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opsUser"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        private Tuple<HttpResponseMessage, Dictionary<string, object>> UpdateOpsUserStatus(OpsUser opsUser, bool isActive = false)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            var url = _opsBuyerUrl + "/api/v1.0/users/" + _productUserId;
            logData = new Dictionary<string, object>();
            logData.Add("url", url);
            logData.Add("opsUser", opsUser);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOpsUserStatus", $"EnableUser productuserId = {_productUserId}, isActive = {isActive.ToString()}" }, logData: logData);

            var putResponse = _client.PutAsJsonAsync(url, opsUser).Result;
            var result = Tuple.Create(putResponse, logData);
            return result;

        }

        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// /// <param name="assetCode"></param>
        /// <returns></returns>
        private ListResponse GetRolesCountDetails(long editorPersonaId, string assetCode)
        {
            ListResponse response = new ListResponse();
            string assetCodeUrl = "";
            Dictionary<string, object> logData = new Dictionary<string, object>();

            IList<Role> rolesList = new List<Role>();

            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/roles" + assetCodeUrl;
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", "Getting roles." }, logData: logData);

                //var apiResponse = _client.GetAsync(url).Result;
                var apiResponse = GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rolesList = JsonConvert.DeserializeObject<IList<Role>>(jsonContent);
                    logData = new Dictionary<string, object>();
                    logData.Add("rolesList", rolesList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", "Got roles." }, logData: logData);

                    if (rolesList == null) { rolesList = new List<Role>(); }

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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", $"Error {ex.Message}" }, exception: ex);
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the roles";

            }
            return response;
        }

        private ListResponse GetRightsDetails(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            string assetCodeUrl = "";
            Dictionary<string, object> logData = new Dictionary<string, object>();

            RightGroup rightGroup = new RightGroup();

            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/rights" + assetCodeUrl;
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", "Getting rights." }, logData: logData);

                //var apiResponse = _client.GetAsync(url).Result;
                var apiResponse = GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rightGroup = JsonConvert.DeserializeObject<RightGroup>(jsonContent);

                    logData = new Dictionary<string, object>();
                    logData.Add("rightGroup", rightGroup);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", "Got rights." }, logData: logData);

                    if (rightGroup == null) { rightGroup = new RightGroup(); }
                    List<MainGroup> list = new List<MainGroup>();
                    list = rightGroup.ToRightsFormatForClient();
                    list = EnableComplianceRights(list);

                    logData = new Dictionary<string, object>();
                    logData.Add("list", list);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", "Returning rights." }, logData: logData);
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesCountDetails", $"Error {ex.Message}" }, exception: ex);
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
                {
                    foreach (var sub in item.subGroupList)
                    {
                        foreach (var right in sub.rightsList)
                        {
                            right.isAssigned = true;
                            right.value = "1";
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Used to get roles for Marketing Center
        /// </summary>
        /// <param name="editorPersonaId"></param>                
        /// <returns></returns>
        private ListResponse GetRightsDetailsForRole(long editorPersonaId, long roleId)
        {
            ListResponse response = new ListResponse();

            Dictionary<string, object> logData = new Dictionary<string, object>();

            RightGroupRole rightGroup = new RightGroupRole();

            try
            {
                var url = _opsBuyerUrl + "/api/v1.0/roles/" + roleId;
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetailsForRole", "Getting rights." }, logData: logData);

                //var apiResponse = _client.GetAsync(url).Result;
                var apiResponse = GetAsync(url).Result;
                if (apiResponse.IsSuccessStatusCode)
                {
                    var jsonContent = apiResponse.Content.ReadAsStringAsync().Result;
                    rightGroup = JsonConvert.DeserializeObject<RightGroupRole>(jsonContent);


                    logData = new Dictionary<string, object>();
                    logData.Add("rightGroup", rightGroup);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetailsForRole", "Got rights." }, logData: logData);

                    if (rightGroup == null) { rightGroup = new RightGroupRole(); }
                    List<MainGroup> list = new List<MainGroup>();
                    list = rightGroup.rights.ToRightsFormatForClient();


                    logData = new Dictionary<string, object>();
                    logData.Add("list", list);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetailsForRole", "Returning rights." }, logData: logData);
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsDetailsForRole", $"Error {ex.Message}" }, exception: ex);
                response.IsError = true;
                response.ErrorReason = "There was a problem getting the rights";
            }
            return response;
        }

        /// <summary>
        /// Used to get the list of asset groups or one with list of properties and asset type.  Send a GET request to the Ops.
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">Optional assetGroupId</param>
        /// <returns>ListResponse object</returns>
        private ListResponse GetOpsAssetGroupsDetails(long editorPersonaId, long userPersonaId, int assetGroupId = 0)
        {
            ListResponse result = new ListResponse();
            List<AssetGroup> assetGroups = new List<AssetGroup>();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                ListResponse response = new ListResponse();
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsAssetGroupsDetails", "Get company settings." }, logData: logData);
                HttpResponseMessage getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().Result);
                    _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                    if (_moduleAssetGroups == 1)
                    {
                        // get the asset groups for the company
                        url = _opsBuyerUrl + "/api/v1.0/assetgroups" + ((assetGroupId > 0) ? $"/{assetGroupId}" : "");

                        logData = new Dictionary<string, object>();
                        logData.Add("url", url);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsAssetGroupsDetails", "Get asset groups." }, logData: logData);

                        getResponse = GetAsync(url).Result;
                        if (!getResponse.IsSuccessStatusCode)
                        {
                            var errorMessage = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().Result);
                            //write an error
                            result = new ListResponse()
                            {
                                IsError = true,
                                ErrorReason = "There was a problem getting the asset group."
                            };
                            return result;
                        }
                        if (assetGroupId == 0)
                        {
                            assetGroups = JsonConvert.DeserializeObject<List<AssetGroup>>(getResponse.Content.ReadAsStringAsync().Result);
                        }
                        else
                        {
                            AssetGroup assetGroup = JsonConvert.DeserializeObject<AssetGroup>(getResponse.Content.ReadAsStringAsync().Result);
                            assetGroup.property_list.ToList().ForEach(p => p.Properties = null);
                            assetGroups.Add(assetGroup);
                        }
                        logData = new Dictionary<string, object>();
                        logData.Add("assetGroups", assetGroups);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsAssetGroupsDetails", "Got asset groups." }, logData: logData);

                        if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                        {
                            // flag the role assigned to the given user
                            OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                            if (!string.IsNullOrEmpty(user.AssetID))
                            {
                                if (assetGroups.Any(a => a.AssetID == user.AssetID))
                                {
                                    AssetGroup ag = (from a in assetGroups where a.AssetID == user.AssetID select a).FirstOrDefault();
                                    if (ag != null)
                                    {
                                        ag.IsAssigned = true;
                                    }
                                }
                            }
                        }

                        result = new ListResponse()
                        {
                            Records = assetGroups.Cast<object>().ToList(),
                            TotalRows = assetGroups.Count,
                            RowsPerPage = assetGroups.Count,
                            TotalPages = 1,
                            ErrorReason = "",
                            Additional = "AssetGroups"
                        };
                    }
                }
                else
                {
                    //write an error
                    result = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = "There was a problem getting the asset group"
                    };
                    return result;
                }
            }
            catch (Exception exception)
            {
                // write an error
                result = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = "There was a problem getting the asset group"
                };
                return result;
            }
            return result;
        }

        /// <summary>
        /// Used to get the list of asset groups.  Send a GET request to the Ops.
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="status">Used to remove the disabled assets from the result</param>
        /// <returns>ListResponse object</returns>
        private ListResponse GetOpsAssetDetails(long editorPersonaId, long userPersonaId, string status)
        {
            ListResponse result = new ListResponse();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                ListResponse response = new ListResponse();
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOpsAssetDetails", "Get company settings." }, logData: logData);
                HttpResponseMessage getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    // Keep this block of code in case we ever want to use it later

                    // get the portfolios
                    // get the asset groups for the company
                    url = _opsBuyerUrl + "/api/v1.0/properties?status=" + (string.IsNullOrWhiteSpace(status) ? "all" : status);
                    getResponse = _client.GetAsync(url).Result;
                    List<Portfolio> portfolioList = JsonConvert.DeserializeObject<List<Portfolio>>(getResponse.Content.ReadAsStringAsync().Result);
                    portfolioList.ForEach(p => p.Properties = null);

                    // only keep active portfolios
                    portfolioList = portfolioList.Where(m => m.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();
                    int totalRows = portfolioList.Count();

                    if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId))
                    {
                        // flag the role assigned to the given user
                        OpsUser user = GetUserDetailsById(Convert.ToInt32(_productUserId));
                        if (!string.IsNullOrEmpty(user.AssetID))
                        {
                            if (portfolioList.Any(a => a.ID == user.AssetID))
                            {
                                Portfolio p = (from a in portfolioList
                                               where a.ID == user.AssetID
                                               select a).FirstOrDefault();
                                if (p != null)
                                {
                                    p.IsAssigned = true;
                                }
                            }
                        }
                    }

                    result = new ListResponse()
                    {
                        Records = portfolioList.Cast<object>().ToList(),
                        TotalRows = totalRows,
                        RowsPerPage = totalRows,
                        TotalPages = 1,
                        ErrorReason = "",
                        Additional = "Portfolio"
                    };
                }
                else
                {
                    // write an error
                    result = new ListResponse()
                    {
                        IsError = true,
                        ErrorReason = "There was a problem getting the asset group"
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                // write an error
                result = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = "There was a problem getting the asset group"
                };
                return result;
            }
            return result;
        }

        /// <summary>
        /// Create an Asset Group - Send a POST request to the Ops.
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        private ListResponse CreateAssetGroup(long editorPersonaId, long userPersonaId, AssetGroupCreate assetGroup)
        {
            ListResponse listResponse = new ListResponse();
            IList<AssetGroup> assetGroupList = new List<AssetGroup>();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                ListResponse response = new ListResponse();
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", "Get company settings." }, logData: logData);
                HttpResponseMessage getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().Result);
                    _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                    if (_moduleAssetGroups == 1)
                    {
                        // create the asset groups for the company
                        url = _opsBuyerUrl + "/api/v1.0/assetgroups";

                        logData = new Dictionary<string, object>();
                        logData.Add("url", url);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", "Create asset groups." }, logData: logData);

                        req = new HttpRequestMessage(HttpMethod.Post, url);
                        req.Content = new StringContent(JsonConvert.SerializeObject(assetGroup), Encoding.Default, "application/json");
                        var postResponse = _client.SendAsync(req).Result;
                        if (postResponse.IsSuccessStatusCode)
                        {
                            AssetGroup postResult = JsonConvert.DeserializeObject<AssetGroup>(postResponse.Content.ReadAsStringAsync().Result);
                            postResult.Name = assetGroup.Name;

                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", $"Success: result {postResult}" });

                            logData = new Dictionary<string, object>();
                            logData.Add("result", postResult);

                            assetGroupList.Add(postResult);
                            listResponse = new ListResponse()
                            {
                                Records = assetGroupList.Cast<object>().ToList(),
                                TotalRows = assetGroupList.Count,
                                RowsPerPage = assetGroupList.Count,
                                TotalPages = 1,
                                ErrorReason = "",
                                IsError = false
                            };
                        }
                        else
                        {
                            logData = new Dictionary<string, object>();
                            logData.Add("postResponse.Content", postResponse.Content.ReadAsStringAsync().Result);
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", "Error" }, logData: logData);
                            string error = postResponse.Content.ReadAsStringAsync().Result.ToString();
                            listResponse = new ListResponse()
                            {
                                Records = assetGroupList.Cast<object>().ToList(),
                                TotalRows = assetGroupList.Count,
                                RowsPerPage = assetGroupList.Count,
                                TotalPages = 1,
                                ErrorReason = error,
                                IsError = true
                            };
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "CreateAssetGroup", $"Error {exception.Message}" }, exception: exception);

                // write an error                    
                string error = "There was a problem creating the AssetGroup. " + exception.Message;
                listResponse = new ListResponse()
                {
                    Records = assetGroupList.Cast<object>().ToList(),
                    TotalRows = assetGroupList.Count,
                    RowsPerPage = assetGroupList.Count,
                    TotalPages = 1,
                    ErrorReason = error,
                    IsError = true
                };
            }
            return listResponse;
        }

        /// <summary>
        /// Edit/Update an Asset Group - Send a PUT request to the Ops.
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        private ListResponse UpdateAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupCreate assetGroup)
        {
            ListResponse listResponse = new ListResponse();
            IList<AssetGroup> assetGroupList = new List<AssetGroup>();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                ListResponse response = new ListResponse();
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", "Get company settings." }, logData: logData);
                HttpResponseMessage getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().Result);
                    _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                    if (_moduleAssetGroups == 1)
                    {
                        // create the asset groups for the company
                        url = _opsBuyerUrl + $"/api/v1.0/assetgroups/{assetGroupId}";

                        logData = new Dictionary<string, object>();
                        logData.Add("url", url);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", "Update asset groups." }, logData: logData);

                        req = new HttpRequestMessage(HttpMethod.Put, url);
                        req.Content = new StringContent(JsonConvert.SerializeObject(assetGroup), System.Text.Encoding.Default, "application/json");
                        var postResponse = _client.SendAsync(req).Result;
                        if (postResponse.IsSuccessStatusCode)
                        {
                            AssetGroup postResult = JsonConvert.DeserializeObject<AssetGroup>(postResponse.Content.ReadAsStringAsync().Result);
                            postResult.Name = assetGroup.Name;

                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", $"Success result: {postResult}" });

                            logData = new Dictionary<string, object>();
                            logData.Add("result", postResult);

                            assetGroupList.Add(postResult);
                            listResponse = new ListResponse()
                            {
                                Records = assetGroupList.Cast<object>().ToList(),
                                TotalRows = assetGroupList.Count,
                                RowsPerPage = assetGroupList.Count,
                                TotalPages = 1,
                                ErrorReason = "",
                                IsError = false
                            };
                        }
                        else
                        {
                            logData = new Dictionary<string, object>();
                            logData.Add("postResponse.Content", postResponse.Content.ReadAsStringAsync().Result);
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", "Error" }, logData: logData);
                            string error = postResponse.Content.ReadAsStringAsync().Result.ToString();
                            listResponse = new ListResponse()
                            {
                                Records = assetGroupList.Cast<object>().ToList(),
                                TotalRows = assetGroupList.Count,
                                RowsPerPage = assetGroupList.Count,
                                TotalPages = 1,
                                ErrorReason = error,
                                IsError = true
                            };
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateAssetGroup", $"Error. {exception.Message}" }, exception: exception);

                // write an error                    
                string error = "There was a problem updating the AssetGroup. " + exception.Message;
                listResponse = new ListResponse()
                {
                    Records = assetGroupList.Cast<object>().ToList(),
                    TotalRows = assetGroupList.Count,
                    RowsPerPage = assetGroupList.Count,
                    TotalPages = 1,
                    ErrorReason = error,
                    IsError = true
                };
            }
            return listResponse;
        }

        /// <summary>
        /// Update Asset Group Name/Status - Send a PATCH request to the Ops.
        /// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="assetGroupId">assetGroupId being updated</param>
        /// <param name="assetGroup">AssetGroup object (Name, Description, and List of Properties {Ids, OR Codes}</param>
        /// <returns>ListResponse object</returns>
        private ListResponse PatchAssetGroup(long editorPersonaId, long userPersonaId, int assetGroupId, AssetGroupPatch assetGroup)
        {
            ListResponse listResponse = new ListResponse();
            IList<AssetGroup> assetGroupList = new List<AssetGroup>();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            try
            {
                ListResponse response = new ListResponse();
                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (response.IsError) { return response; }

                var url = _opsBuyerUrl + "/api/v1.0/company/configs?config_name=module_asset_groups";
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
                logData = new Dictionary<string, object>();
                logData.Add("url", url);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", "Get company settings." }, logData: logData);
                HttpResponseMessage getResponse = GetAsync(url).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    var assetGroupSetting = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().Result);
                    _moduleAssetGroups = Convert.ToInt16(assetGroupSetting.MODULE_ASSET_GROUPS);

                    if (_moduleAssetGroups == 1)
                    {
                        // create the asset groups for the company
                        url = _opsBuyerUrl + $"/api/v1.0/assetgroups/{assetGroupId}";

                        logData = new Dictionary<string, object>();
                        logData.Add("url", url);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", "Create asset groups." }, logData: logData);

                        HttpMethod httpMethod = new HttpMethod("PATCH");
                        req = new HttpRequestMessage(httpMethod, url);
                        req.Content = new StringContent(JsonConvert.SerializeObject(assetGroup), System.Text.Encoding.Default, "application/json");
                        var patchResponse = _client.SendAsync(req).Result;

                        IList<AssetGroupPatch> assetGroupCreateList = new List<AssetGroupPatch>()
                        {
                            assetGroup
                        };

                        if (patchResponse.IsSuccessStatusCode)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", $"Success result: {patchResponse.IsSuccessStatusCode}" });

                            logData = new Dictionary<string, object>();
                            logData.Add("result", assetGroup);

                            listResponse = new ListResponse()
                            {
                                Records = assetGroupCreateList.Cast<object>().ToList(),
                                TotalRows = assetGroupCreateList.Count,
                                RowsPerPage = assetGroupCreateList.Count,
                                TotalPages = 1,
                                ErrorReason = "",
                                IsError = false
                            };
                        }
                        else
                        {
                            logData = new Dictionary<string, object>();
                            logData.Add("postResponse.Content", patchResponse.Content.ReadAsStringAsync().Result);
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", "Error" }, logData: logData);
                            string error = patchResponse.Content.ReadAsStringAsync().Result.ToString();
                            listResponse = new ListResponse()
                            {
                                Records = assetGroupCreateList.Cast<object>().ToList(),
                                TotalRows = assetGroupCreateList.Count,
                                RowsPerPage = assetGroupCreateList.Count,
                                TotalPages = 1,
                                ErrorReason = error,
                                IsError = true
                            };
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "PatchAssetGroup", $"Error {exception.Message}" }, exception: exception);

                // write an error                    
                string error = "There was a problem patching the AssetGroup. " + exception.Message;
                listResponse = new ListResponse()
                {
                    Records = assetGroupList.Cast<object>().ToList(),
                    TotalRows = assetGroupList.Count,
                    RowsPerPage = assetGroupList.Count,
                    TotalPages = 1,
                    ErrorReason = error,
                    IsError = true
                };
            }
            return listResponse;
        }
        #endregion
    }
}