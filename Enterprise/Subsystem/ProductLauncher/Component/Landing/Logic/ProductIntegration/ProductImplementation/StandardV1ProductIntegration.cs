using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation
{
    /// <summary>
    /// Manage Product Invoker Abstract Base Class
    /// Purpose of this class is ONLY product integration
    /// Do not add any other methods which do not make product API calls
    /// </summary>
    public class StandardV1ProductIntegration : IManageProductIntegration
    {
        #region Private Variables

        protected HttpClient _httpClient;
        protected DefaultUserClaim _userClaims;

        private const string PRODUCT_SETTINGTYPE_STATUS = "ProductStatus";
        private const string PRODUCT_ROLES_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLES_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
        private const string PRODUCT_PROPERTIES_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
        private const string PRODUCT_PROPERTIES_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";
        private const string PRODUCT_USERGROUPS_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"UserGroupName\"}";
        private const string PRODUCT_USERGROUPS_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"UserGroupName\"}";
        private const string PRODUCT_PROPERTYGROUPS_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"PropertyGroupName\"}";
        private const string PRODUCT_PROPERTYGROUPS_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"PropertyGroupName\"}";
        private const string TokenGrantTypePassword = "password";

        /// <summary>
        /// Fallback HTTP timeout in seconds, used when no 'ApiTimeoutSeconds' product setting is present.
        /// 30 s is intentionally conservative — product APIs should respond well within this window.
        /// Keeping it explicit prevents the silent 100-second default from holding batch threads
        /// hostage when a downstream API hangs.
        /// </summary>
        private const int DefaultApiTimeoutSeconds = 30;
        #endregion

        #region Properties

        protected int ProductId;
        protected IDataCollector _dataCollector;
        private IManagePersona _managePersona;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private ITokenHelper _tokenHelper;

        protected UserDetails EditorUserDetails { get; set; }
        protected UserDetails SubjectUserDetails { get; set; }
        protected string ProductApiBaseUrl { get; set; }
        protected bool CreateUpdateMultiCompanyUserRequiresPMC { get; private set; }
        protected string CompanyInstanceSourceId { get; set; }
        protected List<ProductInternalSetting> ProductInternalSettingList { get; set; }

        /// <summary>
        /// Correlation Id used for logging
        /// </summary>
        protected Guid CorrelationId { get; set; }

        /// <summary>
        /// GB # Blue Book Product Map (Used for logging etc)
        /// </summary>
        protected GbProductMap BlueBookGbProductMap { get; set; }
        public bool ProductNotAvailableForRegularUserNoEmail { get; set; }

        public bool ProductAcceptsUniqueProductUserName { get; set; }

        /// <summary>
        /// Product udm source code
        /// </summary>
        protected string _udmSourceCode = "";

        /// <summary>
        /// Product Details
        /// </summary>
        protected GbProductMap _productDetails = new GbProductMap();

        /// <summary>
        /// Product Repository
        /// </summary>
        protected IProductRepository _productRepository = new ProductRepository();
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor for normal execution
        /// </summary>
        public StandardV1ProductIntegration(int productId, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims)
        {
            _dataCollector = new DataCollector();
            Init(productId, editorPersonaId, subjectPersonaId, userClaims);
        }

        /// <summary>
        ///  Used for unit testing
        /// </summary>
        public StandardV1ProductIntegration(int productId, long editorPersonaId, long subjectPersonaId,
            DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona,
            IProductInternalSettingRepository injectedProductInternalSettingRepository, IProductRepository injectedProductRepository = null)
        {
            _managePersona = injectedManagePersona;
            _productInternalSettingRepository = injectedProductInternalSettingRepository;
            _dataCollector = injectedDataCollector;
            _productRepository = injectedProductRepository ?? new ProductRepository();

            Init(productId, editorPersonaId, subjectPersonaId, userClaims);
        }

        private void Init(int productId, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims)
        {
            ProductId = productId;
            _userClaims = userClaims;
            _productDetails = _productRepository.GetBooksMasterProductDetail(ProductId);
            _udmSourceCode = _productDetails.UDMSourceCode?.Length > 0 ? _productDetails.UDMSourceCode : _productDetails.BooksProductCode;

            // Get editor & subject user details & Verify editor user is the logged-in user
            GetValidateEditorSubjectUserDetails(editorPersonaId, subjectPersonaId);

            // Get product & end point details
            GetProductEndPointDetails();

            // Get Blue book product Code & company instance Id
            GetBlueBookProductMapAndCompanyDetails(subjectPersonaId);

            // Apply API security to HTTP Client
            ApplyApiSecurity();

            // Used for logging etc
            CorrelationId = userClaims.CorrelationId;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns Product Roles
        /// </summary>
        public virtual ListResponse GetProductRoles(RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method." });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

                bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
                if (isCompanyIdRequiredToQuery)
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

                var roleList = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {roleList?.Count}" });

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });
                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            "{ActionName} - {state}", messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

                        var userRoles = user.Roles;
                        MergeUserRoles(roleList, userRoles);
                    }
                }

                if (roleList == null)
                    throw new Exception("Null Role List.");

                return new ListResponse
                {
                    Records = roleList.Cast<object>().ToList(),
                    TotalRows = roleList.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetProductRoles", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" });
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }


        /// <summary>
        /// Returns Product Roles
        /// </summary>
        public virtual ListResponse GetProductUserGroups(RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductUserGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserGroupEndpoint);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: new Dictionary<string, object>() {{ "baseUrlAndQuery", baseUrlAndQuery }}, messageProperties: new object[] { "GetProductUserGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

                bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
                if (isCompanyIdRequiredToQuery)
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

                var userGroupList = GetResultFromApi<IList<ProductUserGroup>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductUserGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received user group list with count = {userGroupList?.Count}" });

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "GetProductUserGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });

                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            "{ActionName} - {state}", messageProperties: new object[] { "GetProductUserGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

                        var userGroups = user.UserGroups;
                        MergeUserGroup(userGroupList, userGroups);
                    }
                }

                if (userGroupList == null)
                    throw new Exception("Null Product User Groups.");

                return new ListResponse
                {
                    Records = userGroupList.Cast<object>().ToList(),
                    TotalRows = userGroupList.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetProductUserGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" });
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }



        /// <summary>
        /// Returns Product Rights for a Role
        /// </summary>
        public virtual ListResponse GetProductRightsForRole(RequestParameter dataFilter, string roleId, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductRightsForRole", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRightEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, roleId);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "GetProductRightsForRole", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

                var rightList = GetResultFromApi<IList<ProductRight>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductRightsForRole", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {rightList?.Count}" });

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "GetProductRightsForRole", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });

                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            "{ActionName} - {state}", messageProperties: new object[] { "GetProductRightsForRole", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

                        var userRoles = user.Roles;
                        MergeUserRights(rightList, userRoles);
                    }
                }

                if (rightList == null)
                    throw new Exception("Null Right List.");

                return new ListResponse
                {
                    Records = rightList.Cast<object>().ToList(),
                    TotalRows = rightList.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetProductRightsForRole", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" });
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }

        /// <summary>
        /// Returns Product Rights for a Company 
        /// </summary>
        /// <param name="dataFilter">Request parameters</param>
        /// <param name="baseUrlAndQuery">Base url</param>
        /// <returns>A response list</returns>
        public virtual ListResponse GetAllRights(RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

                //Get all rights by company
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRightEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

                var allRights = GetResultFromApi<IList<ProductRight>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {allRights?.Count}" });

                if (allRights == null)
                    throw new Exception("Null Right List.");

                return new ListResponse
                {
                    Records = allRights.Cast<object>().ToList(),
                    TotalRows = allRights.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetAllRights", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" });
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }

        /// <summary>
        /// Returns Product Properties
        /// </summary> 
        public virtual ListResponse GetProductProperties(RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                }

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

                Dictionary<string, bool> additionalData = new Dictionary<string, bool>();
                var propertyList = GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}" });

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });

                    var user = GetProductUser();

                    // map user properties
                    if (user != null && user.Properties != null)
                    {
                        if (user.Properties.Contains("all"))
                        {
                            additionalData.Add("allProperties", true);
                        }
                        WriteToDiagnosticLog(
                            "{ActionName} - {state}", messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

                        if (user != null && user.Properties != null)
                        {
                            var userProperties = user.Properties.ConvertAll(p => p.ToUpper());
                            MergeUserProperties(propertyList, userProperties);
                        }
                    }
                }

                if (propertyList == null)
                    throw new Exception("Null Property List.");

                return new ListResponse
                {
                    Records = propertyList.Cast<object>().ToList(),
                    TotalRows = propertyList.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1,
                    Additional = additionalData
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetProductProperties", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" });
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }

        /// <summary>
        /// Returns Product Properties by region or group Id
        /// </summary> 
        public ListResponse GetProductPropertiesByGroup(string groupId, RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertiesByGroup", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyByGroupEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, groupId);
                }

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "GetProductPropertiesByGroup", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

                var propertyList = GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertiesByGroup", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}" });

                if (propertyList == null)
                    throw new Exception("Null Property List.");

                return new ListResponse
                {
                    Records = propertyList.Cast<object>().ToList(),
                    TotalRows = propertyList.Count,
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetProductPropertiesByGroup", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" });
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }

        /// <summary>
        /// Returns Product Property Groups / Regions
        /// </summary>
        public virtual ListResponse GetProductPropertyGroups(RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupsEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                }

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "GetProductPropertyGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

                Dictionary<string, bool> additionalData = new Dictionary<string, bool>();
                var groupList = GetResultFromApi<IList<ProductPropertyGroups>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received regionList with count = {groupList?.Count}" });

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}" });

                    var user = GetProductUser();

                    // map user regions
                    if (user != null)
                    {
                        if (user.PropertyGroups != null && user.PropertyGroups.Contains("all"))
                        {
                            additionalData.Add("allProperties", true);
                        }

                        WriteToDiagnosticLog(
                            "{ActionName} - {state}", messageProperties: new object[] { "GetProductPropertyGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}" });

                        MergeUserPropertyGroups(groupList, user);
                    }
                }

                if (groupList == null)
                    throw new Exception("Null Property Group List.");

                return new ListResponse()
                {
                    Records = groupList.Cast<object>().ToList(),
                    TotalRows = groupList.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    Additional = additionalData,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetProductPropertyGroups", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}" });
                return new ListResponse()
                {
                    ErrorReason = ex.Message,
                    IsError = true
                };
            }
        }

        /// <summary>
        /// Unassign User
        /// </summary> 
        public virtual string UnassignUser()
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

            // Check for bogus phone number if PhoneNumbers is empty
            var phoneNumbers = SubjectUserDetails.PhoneNumbers;
            if (phoneNumbers == null || phoneNumbers.Count == 0)
            {
                var bogusPhoneNumber = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("BogusPhoneNumber", StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrEmpty(bogusPhoneNumber))
                {
                    phoneNumbers = new List<string> { bogusPhoneNumber };
                }
            }
            var productUserProfile = new ProductUserProfile
            {
                UserId = SubjectUserDetails.ProductUserId,
                IsActive = false,
                CompanyId = CompanyInstanceSourceId,
                LoginName = SubjectUserDetails.ProductUserName,
                Email = SubjectUserDetails.Email,
                FirstName = SubjectUserDetails.FirstName,
                LastName = SubjectUserDetails.LastName,
                Phone = SubjectUserDetails.PhoneNumber,
                PhoneNumbers = phoneNumbers
            };


            // Delete / deactivate uer in the product
            var result = DeleteUser(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns success, Updating Unified Login status" });

                try
                {
                    IManageUserLogin manageUserLogin = new ManageUserLogin();
                    IUserLoginRepository userLoginRepository = new UserLoginRepository();

                    var userLogin = manageUserLogin.GetUserLoginOnly(SubjectUserDetails.UserRealPageId);
                    Persona persona = _managePersona.GetPersona(SubjectUserDetails.PersonaId);

                    OrganizationStatus orgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, false);
                    //var organizationList = userLoginRepository.ListOrganizationWithoutStatusByUserId(userLogin.UserId);
                    //OrganizationStatus orgStatus = organizationList.FirstOrDefault(p => p.PartyId == persona.OrganizationPartyId);

                    int statusValue = (int)UserUiStatusType.AccountHidden;

                    //if user is disabled then set status to deactivated instead hidden
                    if (orgStatus.Status.ToString().Equals(UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        statusValue = (int)UserUiStatusType.Deactivated;
                    }

                    // Update product status in green book
                    _dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, statusValue);

                    return string.Empty;
                }
                catch (Exception localWriteEx)
                {
                    // 4.c — SPLIT STATE: the user was successfully deactivated in the external
                    // product but the subsequent local GreenBook status write failed. The local
                    // system still shows the user as active, so the next batch run may attempt
                    // to reassign a product account that is already deactivated on the product side.
                    // Log all identifiers needed for manual reconciliation and return an error so
                    // the batch marks this record as failed rather than silently treating it as success.
                    WriteToErrorLog(
                        "{ActionName} - {state}",
                        logData: new Dictionary<string, object>
                        {
                            { "productLoginName", SubjectUserDetails.ProductUserName },
                            { "productUserId", SubjectUserDetails.ProductUserId },
                            { "subjectPersonaId", SubjectUserDetails.PersonaId }
                        },
                        exception: localWriteEx,
                        messageProperties: new object[] { "UnassignUser", $"SPLIT STATE — Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. User '{SubjectUserDetails.ProductUserName}' was deactivated in the product but local GreenBook status update failed. Manual reconciliation required." });

                    return $"User was deactivated in product {ProductId} but local status update failed. Manual reconciliation required: {localWriteEx.Message}";
                }
            }

            WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "result", result }, { "productUserProfile", productUserProfile } }, messageProperties: new object[] { "UnassignUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns fail" });

            // 6.c — Return a sanitized message instead of the raw API response body.
            // Full error details are already captured in the error log above. Returning
            // result.Content directly risks leaking product API internals (stack traces,
            // internal IDs) to callers and potentially to the UI.
            return $"Failed to unassign user from product {ProductId}. Status: {result.StatusCode}.";
        }

        /// <summary>
        /// Change product user type
        /// </summary> 
        public string ChangeProductUserType(ProductUserRolePropertiesGroups userRolePropertiesRegion, BatchProcessType batchProcessType)
        {
            List<AdditionalParameters> additionalParameters;
            return CreateUpdateProductUser(userRolePropertiesRegion, out additionalParameters, batchProcessType);
        }

        /// <summary>
        /// </summary>

        private string GetUniqueProductLoginName(UserDetails SubjectUserDetails)
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "GetUniqueProductLoginName", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method" });

            var baseLoginName = (SubjectUserDetails.FirstName.TrimWhiteSpace().Substring(0, 1) + SubjectUserDetails.LastName.TrimWhiteSpace()).ToLower();
            var newLoginName = baseLoginName;

            // A hard cap on attempts prevents an infinite loop if the product API is
            // misbehaving or if the base-name space is saturated. 50 iterations combined
            // with a 4-char random suffix (1,679,616 combinations) makes exhaustion
            // effectively impossible under normal batch volumes.
            const int maxAttempts = 50;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (!CheckUserExistInProduct(newLoginName))
                {
                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "GetUniqueProductLoginName", $"Generated LoginName = {newLoginName}" });
                    return newLoginName;
                }

                // Use a globally unique 4-character random suffix instead of an incrementing
                // number. Numeric sequences (jsmith1, jsmith2 …) are predictable and can be
                // claimed simultaneously by concurrent batch threads racing on the same integer.
                // UniqueIdentifierGenerator guarantees no two calls return the same suffix
                // within this process lifetime, regardless of concurrency.
                var suffix = UniqueIdentifierGenerator.GenerateSuffix();
                newLoginName = baseLoginName + suffix;
            }

            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "GetUniqueProductLoginName", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Unable to generate unique LoginName after {maxAttempts} attempts for base '{baseLoginName}'" });

            return string.Empty;
        }

        /// <summary>
        /// Create or update product user
        /// Gets called from Product-Batch
        /// </summary> 
        public virtual string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            string result;
            additionalParameters = new List<AdditionalParameters>();
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method" });

            var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

            if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.UserNoEmail && !ProductNotAvailableForRegularUserNoEmail && string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
            {
                    newProductUser.LoginName = newProductUser.Email;
                    var newLoginName = GetUniqueProductLoginName(SubjectUserDetails);
                    if (string.IsNullOrEmpty(newLoginName))
                    {
                        return "An error occurred. Unable to get username.";
                    }
                    newProductUser.LoginName = newLoginName;
            }
         
            bool isProductUser = false;
            string loginName = !string.IsNullOrEmpty(SubjectUserDetails.ProductUserName) ? SubjectUserDetails.ProductUserName : newProductUser.LoginName;
            var productUser = GetBaseUserDataFromProduct(loginName);
            isProductUser = productUser != null && !string.IsNullOrEmpty(productUser.LoginName);

            //Removing Special Characters for First Name and Last Name
            newProductUser.FirstName = Regex.Replace(newProductUser.FirstName, @"[^A-Za-z0-9]+", "");
            newProductUser.LastName = Regex.Replace(newProductUser.LastName, @"[^A-Za-z0-9]+", "");

            if (isProductUser)
            {
                newProductUser.UserId = productUser.UserId;
                newProductUser.LoginName = productUser.LoginName;
            
                string iterateUserNameRequiredForUserCreation = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("IterateUserNameRequiredForUserCreation", StringComparison.OrdinalIgnoreCase))?.Value;
                if (iterateUserNameRequiredForUserCreation == "1" && string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
                {
                    isProductUser = false;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Product User {newProductUser.LoginName} ,before iteration username {newProductUser.LoginName}" });
                    newProductUser.LoginName = IterateUserNameIfExists(newProductUser.LoginName);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Product User {newProductUser.LoginName} ,after iteration username {newProductUser.LoginName}" });
                }

            }

            if (SubjectUserDetails.UserRoleTypeId == (int)UserRoleType.SuperUser)
            {
                var IsSuperUserProperties = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("SuperUserPropertiesId", StringComparison.OrdinalIgnoreCase));
                if (IsSuperUserProperties != null)
                {
                    List<string> PropertiesList = new List<string>();
                    PropertiesList.Add(IsSuperUserProperties.Value.ToString());
                    newProductUser.Properties = PropertiesList;
                }

                var defaultRoleToSuperUser = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("SuperUserRoleId", StringComparison.OrdinalIgnoreCase));
                if (defaultRoleToSuperUser != null)
                {
                    List<string> rolesList = new List<string>();
                    rolesList.Add(defaultRoleToSuperUser.Value.ToString());
                    newProductUser.Roles = rolesList;
                }

                var superUserRoleType = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("SuperUserRoleType", StringComparison.OrdinalIgnoreCase));
                if (superUserRoleType != null)
                {
                    newProductUser.RoleType = superUserRoleType.Value;
                }

                var defaultUsergroupsToSuperUser = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("UserGroupsId", StringComparison.OrdinalIgnoreCase));
                if (defaultUsergroupsToSuperUser != null)
                {
                    var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserGroupEndpoint);

                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API for SuperUser" });

                    bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
                    if (isCompanyIdRequiredToQuery)
                        baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

                    var userGroupList = GetResultFromApi<IList<ProductUserGroup>>(baseUrlAndQuery);

                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received user group list with count = {userGroupList?.Count} for SuperUser" });

                    List<string> userGroups = new List<string>();
                    if (userGroupList != null)
                    {
                        foreach (var groups in userGroupList)
                        {
                            userGroups.Add(groups.GetGroupId.ToString());
                        }
                    }
                    newProductUser.UserGroups = userGroups;
                }

            }                      

            if (!isProductUser && string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser" });

                // 5.a — The pre-flight existence check that previously appeared here was removed.
                // GetBaseUserDataFromProduct (above) already established that no product user exists
                // for this login name. A second CheckUserExistInProduct call here does not close the
                // race window: under concurrent batch execution two threads can both pass either check
                // simultaneously before either has reached CreateUser. The correct conflict authority
                // is the product API itself — CreateUser handles a "user already exists" response via
                // the CallUpdateWhenCreateReturnsUserExists setting, which falls back to UpdateUser
                // atomically from the product's perspective. Removing the redundant check eliminates
                // an unnecessary network round-trip and the false sense of safety it provided.
                result = CreateUser(newProductUser, out additionalParameters, batchProcessType);

            }
            else if (isProductUser && CreateUpdateMultiCompanyUserRequiresPMC)
            {
                result = CreateMultiCompanyUser(newProductUser, out additionalParameters);
            }
            else
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "CreateUpdateProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser" });

                newProductUser.UserId = SubjectUserDetails.ProductUserId;
                newProductUser.LoginName = SubjectUserDetails.ProductUserName;
                result = UpdateUser(newProductUser, batchProcessType, out additionalParameters);
            }

            // Get product user object 
            return result;
        }


        /// <summary>
        /// Iterate user name if it already exists in product.
        /// </summary>
        /// <param name="productLoginName"></param>
        /// <returns></returns>

        private string IterateUserNameIfExists(string productLoginName)
        {
            // A hard cap on attempts prevents an infinite loop if the product API is
            // unresponsive or the login namespace for this base name is saturated.
            const int maxAttempts = 50;

            // Safely decompose email-format login names. Splitting unconditionally and
            // indexing [1] would throw IndexOutOfRangeException if the name has no '@'.
            bool isEmailFormat = productLoginName.Contains('@');
            string localPart = isEmailFormat ? productLoginName.Split('@')[0] : productLoginName;
            string domain    = isEmailFormat ? "@" + productLoginName.Split('@')[1] : string.Empty;

            string iteratedLoginName = productLoginName;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (!CheckUserExistInProduct(iteratedLoginName))
                {
                    WriteToDiagnosticLog(
                        "{ActionName} - {state}", messageProperties: new object[] { "IterateUserNameIfExists", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId} - generated iterated LoginName = {iteratedLoginName}" });
                    return iteratedLoginName;
                }

                // Use a globally unique 4-character random suffix instead of an incrementing
                // integer. Numeric suffixes (user1@co.com, user2@co.com …) are predictable and
                // can be simultaneously claimed by concurrent batch threads observing the same
                // "next" integer. UniqueIdentifierGenerator guarantees each suffix is issued
                // at most once per process lifetime, making concurrent collisions impossible.
                var suffix = UniqueIdentifierGenerator.GenerateSuffix();
                iteratedLoginName = localPart + suffix + domain;
            }

            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "IterateUserNameIfExists", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId} - unable to generate unique iterated LoginName after {maxAttempts} attempts for base '{productLoginName}'" });

            return iteratedLoginName;
        }

        #region private
        public virtual IntegrationProductUser GetBaseUserDataFromProduct(string loginNameToCheck, string baseUrlAndQuery = null)
        {
            if (string.IsNullOrEmpty(baseUrlAndQuery))
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

            var encodedLoginNameToCheck = Uri.EscapeDataString(loginNameToCheck ?? string.Empty);
            if (baseUrlAndQuery.Contains("{1}"))
            {
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, encodedLoginNameToCheck);
            }
            else
            {
                baseUrlAndQuery = string.Format(baseUrlAndQuery, encodedLoginNameToCheck);
            }
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "GetBaseUserDataFromProduct", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

            var productUser = GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, false);

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "IntegrationProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

            return productUser;
        }
        #endregion


        /// <summary>
        /// Create a user in the product
        /// </summary>
        protected virtual string CreateMultiCompanyUser(IntegrationProductUser productUser, out List<AdditionalParameters> additionalParameters)
        {
            additionalParameters = new List<AdditionalParameters>();
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

            // dump api info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);
            var user = GetProductUser();

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.PutEntity<IntegrationProductUser>(productUser);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Unified Login SAML product mapping" });

                try
                {
                    // map product user in unified login
                    _dataCollector.CreateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser);

                    // OPTIONAL - If product needs more attributes than userid or loginName then override in the product (e.g. PAM uses)
                    CreateAdditionalSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser);

                    CreateAdditionalSamlUserAttributeForStandardIntegration(SubjectUserDetails.PersonaId, ProductId, productUser);

                    var productList = _productRepository.GetAllProducts();
                    string productName = productList.FirstOrDefault(a => a.ProductId == ProductId).Name;

                    additionalParameters = AssignedRoleandPropertyNameList(user, productUser, productName);

                    if (productUser.EmployeeAdditional != null)
                    {
                        _dataCollector.AddUpdateEmployeeProductADGroupMapping(SubjectUserDetails.PersonaId, ProductId, productUser.EmployeeAdditional.AzureADGroupId);
                    }

                    return string.Empty;
                }
                catch (Exception localWriteEx)
                {
                    // 4.a — SPLIT STATE: the user was successfully created in the external product
                    // (multi-company path) but the subsequent local GreenBook write failed.
                    // Log all identifiers needed for manual reconciliation. Return an error so
                    // the batch marks this record as failed rather than silently succeeding.
                    WriteToErrorLog(
                        "{ActionName} - {state}",
                        logData: new Dictionary<string, object>
                        {
                            { "productLoginName", productUser.LoginName },
                            { "productUserId", productUser.UserId },
                            { "subjectPersonaId", SubjectUserDetails.PersonaId },
                            { "productApiResponseContent", result.Content?.ToString() }
                        },
                        exception: localWriteEx,
                        messageProperties: new object[] { "CreateMultiCompanyUser", $"SPLIT STATE — Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. User created in product but local GreenBook write failed. Manual reconciliation required for login '{productUser.LoginName}'." });

                    return $"User was created in product {ProductId} but local status update failed. Manual reconciliation required: {localWriteEx.Message}";
                }
            }

            WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "result", result } }, messageProperties: new object[] { "CreateMultiCompanyUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Error" });

            // 6.c — Return a sanitized message. Full details are in the error log above.
            return $"Failed to create user in product {ProductId}. Status: {result.StatusCode}.";
        }

        private List<AdditionalParameters> AssignedRoleandPropertyNameList(IntegrationProductUser user,IntegrationProductUser productUser, string productName)
        {

            try
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "AssignedRoleandPropertyNameList", $"Assigned roles and properties name list for product {productName}" });
                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                if (productUser.RoleList != null)
                {
                    var addedRoleList = user?.Roles == null ? productUser.RoleList.ToList() : productUser.RoleList.Except(user?.Roles).ToList();
                    var removedRoleList = user?.Roles?.Except(productUser.RoleList).ToList() ?? new List<string>();

                    if (addedRoleList.Any())
                    {
                        var assignedRoleNameList = GetRoleNameList(addedRoleList, PRODUCT_ROLES_ASSIGN_MESSAGE, productName);
                        if (assignedRoleNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(assignedRoleNameList).ToList();
                        }
                    }
                    if (removedRoleList.Any())
                    {
                        var removedRoleNameList = GetRoleNameList(removedRoleList, PRODUCT_ROLES_REMOVED_MESSAGE, productName);
                        if (removedRoleNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(removedRoleNameList).ToList();
                        }
                    }
                }
                if (productUser.Properties != null)
                {
                    var addedPropertyList = user?.Properties == null ? productUser.Properties.ToList() : productUser.Properties.Except(user?.Properties).ToList();
                    var removedPropertyList = user?.Properties?.Except(productUser.Properties).ToList() ?? new List<string>();

                    if (addedPropertyList.Any())
                    {
                        var assignedPropertyNameList = GetPropertyNameList(addedPropertyList, PRODUCT_PROPERTIES_ASSIGN_MESSAGE, productName);
                        if (assignedPropertyNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(assignedPropertyNameList).ToList();
                        }
                    }

                    if (removedPropertyList.Any())
                    {
                        var removedPropertyNameList = GetPropertyNameList(removedPropertyList, PRODUCT_PROPERTIES_REMOVED_MESSAGE, productName);
                        if (removedPropertyNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(removedPropertyNameList).ToList();
                        }
                    }
                }
                if (productUser.UserGroups != null)
                {
                    var addedUserGroupsList = user?.UserGroups == null ? productUser.UserGroups.ToList() : productUser.UserGroups.Except(user?.UserGroups).ToList();
                    var removedUserGroupsList = user?.UserGroups?.Except(productUser.UserGroups).ToList() ?? new List<string>();

                    if (addedUserGroupsList.Any())
                    {
                        var assignedUserGroupsNameList = GetUserGroupNameList(addedUserGroupsList, PRODUCT_USERGROUPS_ASSIGN_MESSAGE, productName);
                        if (assignedUserGroupsNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(assignedUserGroupsNameList).ToList();
                        }
                    }

                    if (removedUserGroupsList.Any())
                    {
                        var removedUserGroupsNameList = GetUserGroupNameList(removedUserGroupsList, PRODUCT_USERGROUPS_REMOVED_MESSAGE, productName);
                        if (removedUserGroupsNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(removedUserGroupsNameList).ToList();
                        }
                    }
                }
                if (productUser.PropertyGroups != null)
                {
                    var addedPropertyGroupsList = user?.PropertyGroups == null ? productUser.PropertyGroups.ToList() : productUser.PropertyGroups.Except(user?.PropertyGroups).ToList();
                    var removedPropertyGroupsList = user?.PropertyGroups?.Except(productUser.PropertyGroups).ToList() ?? new List<string>();

                    if (addedPropertyGroupsList.Any())
                    {
                        var assignedPropertyGroupsNameList = GetPropertyGroupNameList(addedPropertyGroupsList, PRODUCT_PROPERTYGROUPS_ASSIGN_MESSAGE, productName);
                        if (assignedPropertyGroupsNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(assignedPropertyGroupsNameList).ToList();
                        }
                    }

                    if (removedPropertyGroupsList.Any())
                    {
                        var removedPropertyGroupNameList = GetPropertyGroupNameList(removedPropertyGroupsList, PRODUCT_PROPERTYGROUPS_REMOVED_MESSAGE, productName);
                        if (removedPropertyGroupNameList != null)
                        {
                            additionalParameters = additionalParameters.Concat(removedPropertyGroupNameList).ToList();
                        }
                    }
                }
                return additionalParameters;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "result", ex.Message } }, messageProperties: new object[] { "AssignedRoleandPropertyNameList", $"Unable to get the role and property names list for product {productName} " });
                return new List<AdditionalParameters>();
            }
        }

        private List<AdditionalParameters> GetRoleNameList(List<string> userRoleList, string jsonString, string productName)
        {
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            if (userRoleList != null && userRoleList.Count > 0)
            {
                string baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);

                bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
                if (isCompanyIdRequiredToQuery)
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

                var roleList = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);
                if (roleList == null) return additionalParameters;
                foreach (var role in roleList)
                {
                    if (userRoleList.Contains(role.GetRoleId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Roles", Value = jsonString.Replace("RoleName", role.GetName) });
                    }
                }
            }
            return additionalParameters;
        }

        private List<AdditionalParameters> GetPropertyNameList(List<string> userPropertiesList, string jsonString, string productName)
        {
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            if (userPropertiesList != null && userPropertiesList.Count > 0)
            {
                string baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

                var propertyList = GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);
                if (propertyList == null) return additionalParameters;
                foreach (var proeprty in propertyList)
                {
                    if (userPropertiesList.Contains(proeprty.GetPropertyId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Properties", Value = jsonString.Replace("PropertyName", proeprty.GetName) });
                    }
                }
                // If the user has access to all properties, directly add it without querying the property list
                if (userPropertiesList.Any(p => p.Equals("all", StringComparison.OrdinalIgnoreCase) || p == "-1"))
                {
                    additionalParameters.Add(new AdditionalParameters { Key = productName + " Properties", Value = jsonString.Replace("PropertyName", "All Properties") });
                }
            }
            return additionalParameters;
        }

        private List<AdditionalParameters> GetUserGroupNameList(List<string> userGroupList, string jsonString, string productName)
        {
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            if (userGroupList != null && userGroupList.Count > 0)
            {
                string baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserGroupEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

                var userGroupsList = GetResultFromApi<IList<ProductUserGroup>>(baseUrlAndQuery);
                if (userGroupsList == null) return additionalParameters;
                foreach (var group in userGroupsList)
                {
                    if (userGroupList.Contains(group.GetGroupId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " UserGroups", Value = jsonString.Replace("UserGroupName", group.UserGroupName) });
                    }
                }
            }
            return additionalParameters;
        }

        private List<AdditionalParameters> GetPropertyGroupNameList(List<string> propertyGroupList, string jsonString, string productName)
        {
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            if (propertyGroupList != null && propertyGroupList.Count > 0)
            {
                string baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupsEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

                var propertyGroupsList = GetResultFromApi<IList<ProductPropertyGroups>>(baseUrlAndQuery);
                if (propertyGroupsList == null) return additionalParameters;
                foreach (var group in propertyGroupsList)
                {
                    if (propertyGroupList.Contains(group.GetGroupId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " PropertyGroups", Value = jsonString.Replace("PropertyGroupName", group.GetGroupName) });
                    }
                }
            }
            return additionalParameters;
        }
        /// <summary>
        /// Get Product User API call
        /// </summary> 
        public virtual IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "GetProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

            // Get partial api query based on end point
            if (string.IsNullOrEmpty(baseUrlAndQuery))
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "GetProductUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

            //If product failed to assign to subject the saml is null here, use login name to get user info from product
            var rawLoginName = !string.IsNullOrEmpty(SubjectUserDetails.ProductUserName)
                ? SubjectUserDetails.ProductUserName
                : SubjectUserDetails.LoginName;
            var encodedLoginName = Uri.EscapeDataString(rawLoginName ?? string.Empty);

            if (baseUrlAndQuery.Contains("{1}"))
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, encodedLoginName);
            else
                baseUrlAndQuery = string.Format(baseUrlAndQuery, encodedLoginName);

            return GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError);
        }

        /// <summary>
        /// Returns companies for a product
        /// </summary>
        public virtual ListResponse GetProductOrganizations(string roleOrganizationId, string organizationType, string baseUrlAndQuery = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete User - patch with isActive = false
        /// </summary> 
        protected virtual ApiResponse DeleteUser(ProductUserProfile profile = null)
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

            // patch to se isActive flag to false
            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PatchProfileEndpoint);

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "DeleteUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, profile);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            return integration.PatchEntity<string>(profile);
        }

        /// <summary>
        /// Create a user in the product
        /// </summary>
        protected virtual string CreateUser(IntegrationProductUser productUser, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = 0)
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "CreateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "CreateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

            // dump api info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.PostEntity<IntegrationProductUser>(productUser);
            additionalParameters = new List<AdditionalParameters>();
            string response = string.Empty;
            string callUpdateWhenCreateReturnsUserExists = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("CallUpdateWhenCreateReturnsUserExists", StringComparison.OrdinalIgnoreCase))?.Value;
            if (!result.IsSuccessStatusCode && callUpdateWhenCreateReturnsUserExists == "1" && result.Content != null)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. User already exists. Proceeding to update." });

                // 3.d — Guard against non-JSON error bodies (HTML error pages, plain-text
                // gateway responses, empty strings). A JsonReaderException here would propagate
                // unhandled out of the success-fallback path, making the actual API error
                // unrecoverable and unactionable from the log.
                Newtonsoft.Json.Linq.JObject userResult = null;
                try
                {
                    userResult = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ToString());
                }
                catch (JsonException parseEx)
                {
                    WriteToErrorLog("{ActionName} - {state}", null, parseEx, messageProperties: new object[] { "CreateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Failed to parse error response as JSON. Raw content: {result.Content}" });
                }

                if (result.StatusCode == (int)HttpStatusCode.BadRequest && userResult != null)
                {
                    string statusValue = userResult.GetValue("Status", StringComparison.OrdinalIgnoreCase)?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(statusValue) && statusValue.ToLower().Contains("user already exists"))
                    {
                        // Proceed to update user
                        string userIdValue = userResult.GetValue("UserId", StringComparison.OrdinalIgnoreCase)?.ToString();
                        if (!string.IsNullOrEmpty(userIdValue))
                        {
                            productUser.UserId = userIdValue;
                            return UpdateUser(productUser, batchProcessType, out additionalParameters);
                        }
                    }
                }
            }

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "CreateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Unified Login SAML product mapping" });

                try
                {
                    // map product user in green book
                    _dataCollector.CreateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser);

                    // OPTIONAL - If product needs more attributes than userid or loginName then override in the product (e.g. PAM uses)
                    CreateAdditionalSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser);

                    CreateAdditionalSamlUserAttributeForStandardIntegration(SubjectUserDetails.PersonaId, ProductId, productUser);

                    if (productUser.EmployeeAdditional != null)
                    {
                        _dataCollector.AddUpdateEmployeeProductADGroupMapping(SubjectUserDetails.PersonaId, ProductId, productUser.EmployeeAdditional.AzureADGroupId);
                    }

                    var productList = _productRepository.GetAllProducts();
                    string productName = productList.FirstOrDefault(a => a.ProductId == ProductId).Name;

                    var isActivityCheckNotRequired = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("IsActivityCheckNotRequired", StringComparison.OrdinalIgnoreCase))?.Value;
                    //Getting the assigned role names
                    if ((productUser.RoleList != null || productUser.Properties != null) && isActivityCheckNotRequired != "1")
                    {
                        var userRoles = productUser.RoleList;
                        var userProperties = productUser.Properties;
                        var userGroups = productUser.UserGroups;
                        var propertyGroups = productUser.PropertyGroups;

                        if (userRoles != null)
                        {
                            var roleNameList = GetRoleNameList(userRoles, PRODUCT_ROLES_ASSIGN_MESSAGE, productName);
                            additionalParameters = additionalParameters.Concat(roleNameList).ToList();
                        }
                        if (userProperties != null)
                        {
                            var propertyNameList = GetPropertyNameList(userProperties, PRODUCT_PROPERTIES_ASSIGN_MESSAGE, productName);
                            additionalParameters = additionalParameters.Concat(propertyNameList).ToList();
                        }
                        if (userGroups != null)
                        {
                            var userGroupList = GetUserGroupNameList(userGroups, PRODUCT_USERGROUPS_ASSIGN_MESSAGE, productName);
                            additionalParameters = additionalParameters.Concat(userGroupList).ToList();
                        }
                        if (propertyGroups != null)
                        {
                            var propertyGroupList = GetPropertyGroupNameList(propertyGroups, PRODUCT_PROPERTYGROUPS_ASSIGN_MESSAGE, productName);
                            additionalParameters = additionalParameters.Concat(propertyGroupList).ToList();
                        }
                    }
                }
                catch (Exception localWriteEx)
                {
                    // 4.a — SPLIT STATE: the user was successfully created in the external product
                    // but the subsequent local GreenBook write failed. The two systems are now
                    // inconsistent. Log all identifiers needed for manual reconciliation or an
                    // automated reconciliation job. Return an error so the batch marks this record
                    // as failed — preventing the caller from treating it as a success and
                    // leaving the orphaned account undiscovered.
                    WriteToErrorLog(
                        "{ActionName} - {state}",
                        logData: new Dictionary<string, object>
                        {
                            { "productLoginName", productUser.LoginName },
                            { "productUserId", productUser.UserId },
                            { "subjectPersonaId", SubjectUserDetails.PersonaId },
                            { "productApiResponseContent", result.Content?.ToString() }
                        },
                        exception: localWriteEx,
                        messageProperties: new object[] { "CreateUser", $"SPLIT STATE — Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. User created in product but local GreenBook write failed. Manual reconciliation required for login '{productUser.LoginName}'." });

                    response = $"User was created in product {ProductId} but local status update failed. Manual reconciliation required: {localWriteEx.Message}";
                }
            }
            else
            {
                WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "result", result } }, messageProperties: new object[] { "CreateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}" });

                response = result.Content as string;

                if(string.IsNullOrWhiteSpace(response))
                {
                    response = "Unknown error";
                }
            }
            return response;
        }

        /// <summary>
        /// Update a user in the product
        /// </summary>
        protected virtual string UpdateUser(IntegrationProductUser productUser, BatchProcessType batchProcessType, out List<AdditionalParameters> additionalParameters)
        {
            additionalParameters = new List<AdditionalParameters>();
            IntegrationProductUser user = null;

            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "UpdateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method" });

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "UpdateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Calling API" });

            // dump API call info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);
            //Get existing user information before update
            
            
            var isActivateUserBeforeUpdate = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("IsActivateUserBeforeUpdate", StringComparison.OrdinalIgnoreCase))?.Value;
            var isActivityCheckNotRequired = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("IsActivityCheckNotRequired", StringComparison.OrdinalIgnoreCase))?.Value;

            if (isActivityCheckNotRequired != "1" && !string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
            {
                user = GetProductUser();
            }
            // 4.b — Track whether a reactivation PATCH was issued before the update PUT.
            // If the PUT later fails the user is left active in the product without the
            // intended configuration change applied — a split state that needs to surface
            // clearly in logs so operators can investigate.
            bool wasReactivated = false;
            if (isActivateUserBeforeUpdate == "1")
            {
                //If knock product is unassigned and trying to assigned back knock to user we need to make Patch call to reactivate a user first and then make update call
                //IsActivateUserBeforeUpdate flag is enabled for the knock product only
                var userStatus = GetBaseUserDataFromProduct(productUser.LoginName);
                productUser.IsActive = userStatus.IsActive;
                if (!userStatus.IsActive)
                {
                    UpdateProductUserProfile();
                    wasReactivated = true;
                }
            }

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.PutEntity<IntegrationProductUser>(productUser);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "UpdateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Unified Login SAML product mapping" });

                if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType ==
                    BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                {
                    // activity logging
                    ProductActivityLogger.WriteUpdateUserTypeActivityLog(EditorUserDetails, SubjectUserDetails,
                        BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode, CorrelationId, batchProcessType);
                }
                var assignSamlAttributeBySetting = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("AssignSamlAttributeBySetting", StringComparison.OrdinalIgnoreCase))?.Value;

                if (assignSamlAttributeBySetting != null && assignSamlAttributeBySetting.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    _dataCollector.UpdateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser);
                }

                var isRequireSamlCreateOnUpdateUser = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("IsRequireSamlCreateOnUpdateUser", StringComparison.OrdinalIgnoreCase))?.Value;
                if (isRequireSamlCreateOnUpdateUser != null && isRequireSamlCreateOnUpdateUser.Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    var existingUserDetails = _dataCollector.GetUserDetailsByPersona(SubjectUserDetails.PersonaId, ProductId);
                    if (existingUserDetails == null || (string.IsNullOrEmpty(existingUserDetails.ProductUserName) && string.IsNullOrEmpty(existingUserDetails.ProductUserId)))
                    {
                        _dataCollector.CreateSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, SamlAttributeEnum.productUsername, productUser.LoginName);
                        _dataCollector.CreateSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, SamlAttributeEnum.UserId, productUser.UserId);
                    }
                }

                _dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, (int) ProductBatchStatusType.Success);

                if (!ProductAcceptsUniqueProductUserName)
                    UpdateSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser.UserId, productUser.LoginName, productUser.Email);

                if (productUser.EmployeeAdditional != null)
                {
                    _dataCollector.AddUpdateEmployeeProductADGroupMapping(SubjectUserDetails.PersonaId, ProductId, productUser.EmployeeAdditional.AzureADGroupId);
                }

                if (isActivityCheckNotRequired != "1")
                {
                    var productList = _productRepository.GetAllProducts();
                    string productName = productList.FirstOrDefault(a => a.ProductId == ProductId)?.Name;

                    additionalParameters = AssignedRoleandPropertyNameList(user, productUser, productName);
                }

                return string.Empty;
            }
            // 4.b — SPLIT STATE: if reactivation succeeded but the configuration update failed,
            // the user is now active in the product with stale role/property assignments.
            // Log a distinct warning so this partial state is immediately visible in monitoring
            // rather than appearing as a generic update failure.
            if (wasReactivated)
            {
                WriteToErrorLog(
                    "{ActionName} - {state}",
                    logData: new Dictionary<string, object>
                    {
                        { "productLoginName", productUser.LoginName },
                        { "productUserId", productUser.UserId },
                        { "subjectPersonaId", SubjectUserDetails.PersonaId },
                        { "result", result }
                    },
                    messageProperties: new object[] { "UpdateUser", $"SPLIT STATE — Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. User '{productUser.LoginName}' was reactivated in the product but the subsequent configuration update failed. User is active with potentially stale assignments. Manual review required." });
            }
            else
            {
                WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "result", result } }, messageProperties: new object[] { "UpdateUser", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}" });
            }

            // 6.c — Return a sanitized message. Full details are in the error log above.
            return $"Failed to update user in product {ProductId}. Status: {result.StatusCode}.";
        }

        /// <summary>
        /// Override this in product implementation if any product requires to update user saml settings after update
        /// e.g. used in ILM
        /// </summary>
        protected virtual void UpdateSamlUserAttribute(long personaId, int productId, string productUserId, string productUserLoginName, string productUserEmail)
        {
            //Blank method used for override
        }

        /// <summary>
        /// Override this in product implementation if any product requires to create additional saml settings
        /// e.g. used in PAM
        /// </summary>
        protected virtual void CreateAdditionalSamlUserAttribute(long personaId, int productId, IntegrationProductUser productUser)
        {
            //Blank method used for override
        }

        /// <summary>
        /// Used to add additional product information for the given user and product.
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="productId"></param>
        /// <param name="productUser"></param>
        private void CreateAdditionalSamlUserAttributeForStandardIntegration(long personaId, int productId, IntegrationProductUser productUser)
        {
            string additionalSamlAttributesForStandardIntegration = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("SI_AdditionalSAMLUserAttributes", StringComparison.OrdinalIgnoreCase))?.Value;
            if (additionalSamlAttributesForStandardIntegration != null)
            {
                var samlAttributeList = additionalSamlAttributesForStandardIntegration.Split(',');
                foreach (var attribute in samlAttributeList)
                {
                    switch (attribute.ToUpperInvariant())
                    {
                        case "PMCID":
                            _dataCollector.CreateSamlUserAttribute(personaId, productId, SamlAttributeEnum.PMCID, productUser.CompanyId);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if user exists in the product
        /// </summary> 
        protected virtual bool CheckUserExistInProduct(string loginNameToCheck, string baseUrlAndQuery = null)
        {
            if (baseUrlAndQuery == null)
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint);

            var encodedCheckName = Uri.EscapeDataString(loginNameToCheck ?? string.Empty);
            if (baseUrlAndQuery.Contains("{1}"))
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, encodedCheckName);
            else
                baseUrlAndQuery = string.Format(baseUrlAndQuery, encodedCheckName);

            var productUser = GetProductUser(baseUrlAndQuery, false);

            if (productUser != null && !string.IsNullOrEmpty(productUser.UserId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update Product User Profile information
        /// </summary>
        /// <returns>Empty string if success</returns>
        public virtual string UpdateProductUserProfile()
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "UpdateProductUserProfile", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method" });

            // Get product user object 
            var productUserProfile = new ProductUserProfile()
            {
                LoginName = SubjectUserDetails.ProductUserName,
                FirstName = SubjectUserDetails.FirstName,
                MiddleName = SubjectUserDetails.MiddleName,
                LastName = SubjectUserDetails.LastName,
                Email = SubjectUserDetails.Email,
                PhoneNumbers = SubjectUserDetails.PhoneNumbers,
                Phone = SubjectUserDetails.PhoneNumber,
                IsActive = Convert.ToBoolean(SubjectUserDetails.IsActive),
                //Title = SubjectUserDetails.Title,
                UserId = SubjectUserDetails.ProductUserId,
                CompanyId = CompanyInstanceSourceId
            };

            return UpdateUserProfile(productUserProfile);
        }

        /// <summary>
        /// Update User Profile.
        /// Called from green-book 
        /// </summary>
        protected virtual string UpdateUserProfile(ProductUserProfile productUserProfile)
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}, subjectPersonaId - {SubjectUserDetails.PersonaId}. At beginning of the method" });

            // Call shared method to update profile from green-book or from external source (migration tool)
            var result = ProductUserProfileChange(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", messageProperties: new object[] { "UpdateUserProfile", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId} subjectPersonaId - {SubjectUserDetails.PersonaId}. Received success. Updating Unified Login SAML product mapping" });

                _dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, (int) ProductBatchStatusType.Success);

                if (!ProductAcceptsUniqueProductUserName)
                    UpdateSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUserProfile.UserId, productUserProfile.LoginName, productUserProfile.Email);

                return string.Empty;
            }

            WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "result", result } }, messageProperties: new object[] { "UpdateUserProfile", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId} productUserProfile.UserId - {productUserProfile.UserId}" });

            // 6.c — Return a sanitized message. Full details are in the error log above.
            return $"Failed to update user profile in product {ProductId}. Status: {result.StatusCode}.";
        }

        /// <summary>
        /// Used to update profile in product either through unified login or migration tool
        /// If used from migration tool then it is direct call to update user profile without
        /// considering if user is in unified or not.
        /// </summary>
        /// <param name="productUserProfile">Product user information</param>
        /// <returns>Api Response.</returns>
        protected virtual ApiResponse ProductUserProfileChange(ProductUserProfile productUserProfile)
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "ProductUserProfileChange", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}, productUserProfile.UserId - {productUserProfile.UserId}. At beginning of the method" });

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PatchProfileEndpoint);

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery } }, messageProperties: new object[] { "ProductUserProfileChange", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}  productUserProfile.UserId - {productUserProfile.UserId}. Calling API" });

            // dump API call info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUserProfile);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            return integration.PatchEntity<ProductUserProfile>(productUserProfile);
        }

        protected void MergeUserRoles(IList<ProductRole> roleList, List<string> userRoles)
        {
            foreach (var role in roleList)
            {
                if (userRoles != null && userRoles.Contains(role.GetRoleId))
                {
                    role.IsAssigned = true;
                }
            }
        }

        protected void MergeUserGroup(IList<ProductUserGroup> userGroupList, List<string> userGroups)
        {
            foreach (var userGroup in userGroupList)
            {
                if (userGroups != null && userGroups.Contains(userGroup.GetGroupId))
                {
                    userGroup.IsAssigned = true;
                }
            }
        }

        #endregion

        #region Migration

        /// <summary>
        /// Gets the migration users.
        /// </summary>
        /// <param name="datafilter">The datafilter.</param>
        /// <returns></returns>
        public virtual ListResponse GetMigrationUsers(RequestParameter datafilter)
        {
            var response = new ListResponse()
            {
                IsError = true,
                ErrorReason = "No Users."
            };

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

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetListUsersEndpoint);

            baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, filter, startRow, resultPerRow);

            // dump API call info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.GetEntityFromApi<IList<IntegrationProductUser>>();

            if (result == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "No users received from product" });
                return response;
            }

            WriteToDiagnosticLog(
                "{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Received users from product" });

            response.RowsPerPage = resultPerRow;
            response.ErrorReason = string.Empty;
            response.IsError = false;
            response.TotalPages = 1;
            response.Records = result.Cast<object>().ToList();
            response.TotalRows = result.Count();
            return response;
        }

        /// <summary>
        /// Updates the users migration status.
        /// </summary>
        /// <param name="migrateUsers">The migrate users.</param>
        /// <returns></returns>
        public virtual MigrateResponse UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            var migrateResponse = new MigrateResponse()
            {
                Status = false
            };

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PatchMigrateUsersEndpoint);
            baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);

            // dump API call info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, migrateUsers);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.PatchEntity<MigrateResponse>(migrateUsers);
            var logData = new Dictionary<string, object> { { "result", result }, { "baseUrlAndQuery", baseUrlAndQuery } };
            if (result.IsSuccessStatusCode)
            {
                var migrationResponse = JsonConvert.DeserializeObject<MigrateResponse>(JsonConvert.SerializeObject(result.Content));
                WriteToDiagnosticLog(
                    "{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "Success" });

                return migrationResponse;
            }

            WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "Error" });
            migrateResponse.Message = "Cannot update user status to migrated.";
            return migrateResponse;
        }

        /// <summary>
        /// Direct call to product to change profile including isActive (mainly used to activate-deactivate from Migration tool)
        /// </summary>
        /// <param name="productUserProfile">Product user information</param>
        /// <returns>string.Empty if success else response contents.</returns>
        public virtual bool ExternalProductUserProfileChange(ProductUserProfile productUserProfile)
        {
            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUserProfile", productUserProfile } }, messageProperties: new object[] { "ExternalProductUserProfileChange", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}, productUserProfile.UserId - {productUserProfile.UserId}. At beginning of the method" });

            // used from external source (migration tool) so no activity logging required
            var result = ProductUserProfileChange(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }

            // log exception details from result
            WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "result", result } }, messageProperties: new object[] { "ExternalProductUserProfileChange", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId} productUserProfile.UserId - {productUserProfile.UserId}" });

            return false;
        }

        protected string GetUniqueProductLogin(string unityUserName)
        {
            return unityUserName;
        }

        protected virtual IntegrationProductUser GenerateProductUserObject(ProductUserRolePropertiesGroups userRolePropertiesRegion)
        {
            // Map user info
            var productUser = new IntegrationProductUser
            {
                LoginName = string.IsNullOrEmpty(SubjectUserDetails.LoginName) ? SubjectUserDetails.LoginName : GetUniqueProductLogin(SubjectUserDetails.LoginName),
                CompanyId = CompanyInstanceSourceId,
                FirstName = SubjectUserDetails.FirstName,
                LastName = SubjectUserDetails.LastName,
                Email = SubjectUserDetails.Email,
                Phone = SubjectUserDetails.PhoneNumber,
                PhoneNumbers = SubjectUserDetails.PhoneNumbers,
                IsActive = true,
                PropertyGroups = userRolePropertiesRegion.PropertyGroupList,
                Properties = userRolePropertiesRegion.PropertyList,
                Roles = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
                PropertyRoles = userRolePropertiesRegion.PropertyRoleList,
                OrganizationRoles = userRolePropertiesRegion.OrganizationRoleList,
                CanReceiveMonthlyReport = userRolePropertiesRegion.CanReceiveMonthlyReport,
                PropertyRoleList = userRolePropertiesRegion.RolePropertiesList,
                RoleList = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
                IsRealPageEmployee = SubjectUserDetails.IsRPEmployee,
                UserGroups = userRolePropertiesRegion.UserGroups,
                IsMigratedUser = true,
                UnifiedLoginUserID = SubjectUserDetails.UserId,
                UnifiedLoginPersonaID = SubjectUserDetails.PersonaId,
                RoleType = userRolePropertiesRegion.RoleType
            };

            if (SubjectUserDetails.UserRoleTypeId == (int) UserRoleType.SuperUser)
            {
                ApplySuperUserData(productUser);
            }

            var supportsEmployeeAccess = GetProductInternalSettingValue("SI_SupportsEmployeeCreation");
            if (SubjectUserDetails.IsRPEmployee && supportsEmployeeAccess == "1")
            {
                ApplyEmployeeData(productUser);
            }

            return productUser;
        }

        private void ApplyEmployeeData(IntegrationProductUser productUser)
        {
            var personaList = _managePersona.ListPersona(SubjectUserDetails.UserRealPageId);
            var employeePersona = personaList.FirstOrDefault(p => p.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);
            if (employeePersona == null)
            {
                return;
            }

            productUser.EmployeeAdditional = new EmployeeAdditional() { AzureADGroup = "" };
            // gather AD info
            var adUserInfo = _dataCollector.GetAzureUserDetails(SubjectUserDetails.UserId);
            productUser.EmployeeAdditional.SAMAccountName = adUserInfo?.SamAccountName;
            var existingProductAdGroupInfo = _dataCollector.GetEmployeeProductADGroupMapping(SubjectUserDetails.PersonaId, ProductId).FirstOrDefault();

            var productAdGroups = _productRepository.GetAdGroupsForProduct(ProductId);
            if (productAdGroups.Count > 0)
            {
                var companyPersonaList = personaList.Where(p => p.OrganizationPartyId == SubjectUserDetails.OrganizationPartyId).ToList();
                var orderedAdGroup = productAdGroups.OrderBy(p => p.AssignmentOrder);
                var userAdGroups = _productRepository.GetAdGroupsForUser(employeePersona.PersonaId);
                var usedGroups = new List<AdGroup>();

                // if the user more than 1 persona in the company, record which groups have already been assigned
                if (companyPersonaList.Count > 1)
                {
                    // the user has multiple persona, so we need to figure out if any of the others already have product logins assigned and what adgroup they are using
                    companyPersonaList.ForEach(p =>
                    {
                        if (p.PersonaId != SubjectUserDetails.PersonaId)
                        {
                            var prodAdgroupInfo = _dataCollector.GetEmployeeProductADGroupMapping(p.PersonaId, ProductId)?.FirstOrDefault();
                            if (prodAdgroupInfo != null)
                            {
                                var isProductAssigned = _productRepository.isProductAssigned(p.PersonaId, 8, ProductId);
                                if (isProductAssigned)
                                {
                                    usedGroups.Add(new AdGroup() { ADGroupId = prodAdgroupInfo.ADGroupId });
                                }
                            }
                        }
                    });
                }

                foreach (var adGroupProduct in orderedAdGroup)
                {
                    if (userAdGroups.All(adg => adg.ADGroupId != adGroupProduct.ADGroupId)) continue;
                    if (usedGroups.Any(adg => adg.ADGroupId == adGroupProduct.ADGroupId)) continue;
                    productUser.EmployeeAdditional.AzureADGroup = adGroupProduct.ActiveDirectoryId.ToString();
                    productUser.EmployeeAdditional.AzureADGroupId = adGroupProduct.ADGroupId;
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(productUser.EmployeeAdditional.AzureADGroup))
            {
                throw new Exception("No ADGroups available to assign to create new product user.");
            }
        }

        protected virtual void ApplySuperUserData(IntegrationProductUser productUser)
        {
            // super user related assignments; if anything different then override in product implementation
            productUser.IsAdminUser = true;
        }

        protected T GetResultFromApi<T>(string baseUrlAndQuery, bool isThrowOnError = true) where T : class
        {
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery);
            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            return integration.GetEntityFromApi<T>(isThrowOnError);
        }

        protected void DumpApiCallInfoToDiagnosticLog(string baseUrlAndQuery, object apiPayLoad = null)
        {
            var logData = new Dictionary<string, object> { { "baseUrlAndQuery", baseUrlAndQuery } };

            if (apiPayLoad != null)
                logData.Add("apiPayLoad", JsonConvert.SerializeObject(apiPayLoad));

            WriteToDiagnosticLog(
                "{ActionName} - {state}", logData: logData, messageProperties: new object[] { "DumpApiCallInfoToDiagnosticLog", $"Product {ProductId} . Calling API" });

        }

        protected virtual void MergeUserPropertyGroups(IList<ProductPropertyGroups> groupList, IntegrationProductUser user)
        {
            List<string> userPropertyGroups = user.PropertyGroups;
            foreach (var region in groupList)
            {
                if (userPropertyGroups.Contains(region.GetGroupId.ToUpper()))
                {
                    region.IsAssigned = true;
                }
            }
        }

        protected string GetOperationEndPoint(ProductEntityEndpointKeyEnum entityType)
        {
            // Get partial api query based on end point                         
            var partialApiQueryUrl = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals(entityType.ToString(), StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrEmpty(partialApiQueryUrl))
            {
                throw new Exception($"Unable to find setting for {entityType}");
            }

            return ProductApiBaseUrl + partialApiQueryUrl;
        }

        protected string GetProductInternalSettingValue(string settingName)
        {
            // Get product setting value
            var settingValue = ProductInternalSettingList.FirstOrDefault(a => a.Name.ToUpper() == settingName.ToUpper())?.Value;
            return settingValue;
        }

        #endregion

        #region Logging Methods

        protected void WriteToInformationLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
            WriteToLog(logType: LogEventLevel.Information, message: message, logData: logData, messageProperties: messageProperties);
        }

        protected void WriteToErrorLog(string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            WriteToLog(logType: LogEventLevel.Error, message: message, logData: logData, exception: exception, messageProperties: messageProperties);
        }

        protected void WriteToDiagnosticLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
            WriteToLog(logType: LogEventLevel.Debug, message: message, logData: logData, messageProperties: messageProperties);
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            if (logData == null)
            {
                logData = new Dictionary<string, object>();
            }

            var editorUserDictionary = EditorUserDetails?.ToDictionary();
            logData.AddRange(editorUserDictionary);

            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", CorrelationId.ToString());

            // 6.a — Use the params object[] overload so every element in messageProperties is
            // forwarded to Serilog. The previous two-argument overload silently discarded any
            // property beyond the second, causing misleading log entries with missing fields.
            logger.Write(logType, exception, message, messageProperties ?? Array.Empty<object>());
        }

        #endregion

        #region Private methods

        private void MergeUserRights(IList<ProductRight> rightList, List<string> userRights)
        {
            foreach (var right in rightList)
            {
                if (userRights.Contains(right.GetRightId))
                {
                    right.IsAssigned = true;
                }
            }
        }

        internal void MergeUserProperties(IList<ProductProperties> propertyList, List<string> userProperties)
        {
            if (propertyList != null && userProperties != null)
            {
                foreach (var property in propertyList)
                {
                    if (userProperties.Contains(property.GetPropertyId.ToUpper()))
                    {
                        property.IsAssigned = true;
                    }
                }
            }
        }

        private void GetValidateEditorSubjectUserDetails(long editorPersonaId, long subjectPersonaId)
        {
            try
            {
                EditorUserDetails = _dataCollector.GetUserDetailsByPersona(editorPersonaId, ProductId);
                if (subjectPersonaId > 0)
                {
                    SubjectUserDetails = _dataCollector.GetUserDetailsByPersona(subjectPersonaId, ProductId);
                }

                // Verify editor user is the logged-in user
                if (!ValidateEditorUser(EditorUserDetails))
                {
                    throw new Exception("Invalid Persona");
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetValidateEditorSubjectUserDetails", $"Product {ProductId} editorPersona id - {EditorUserDetails?.PersonaId}" });
                throw;
            }
        }

        private void GetProductEndPointDetails()
        {
            try
            {
                // Get product & end point details
                if (_productInternalSettingRepository == null)
                    _productInternalSettingRepository = new ProductInternalSettingRepository();

                ProductInternalSettingList = 
                    _productInternalSettingRepository.GetProductInternalSettings(ProductId);

                ProductApiBaseUrl = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("ApiEndPoint", StringComparison.OrdinalIgnoreCase))?.Value;
                if (string.IsNullOrEmpty(ProductApiBaseUrl))
                {
                    throw new Exception($"Product configuration error: 'ApiEndPoint' setting is missing for product {ProductId}.");
                }
                var alternateApiEndPoint = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("AlternateApiEndPoint", StringComparison.OrdinalIgnoreCase))?.Value;
                if (alternateApiEndPoint != null)
                {
                    ProductApiBaseUrl = alternateApiEndPoint;
                }

                var productInternalSetting = ProductInternalSettingList.FirstOrDefault(item => item.Name.Equals("CreateUpdateMultiCompanyUserRequiresPMC", StringComparison.OrdinalIgnoreCase));
                CreateUpdateMultiCompanyUserRequiresPMC = (productInternalSetting != null) && productInternalSetting.Value.Trim() == "1";

                var productInternalSettingProductNotAvailable = ProductInternalSettingList.FirstOrDefault(item => item.Name.Equals("ProductNotAvailableForRegularUserNoEmail", StringComparison.OrdinalIgnoreCase));
                ProductNotAvailableForRegularUserNoEmail = (productInternalSettingProductNotAvailable != null) && productInternalSettingProductNotAvailable.Value.Trim() == "1";

                var productInternalSettingAcceptsUniqueProductUserName = ProductInternalSettingList.FirstOrDefault(item => item.Name.Equals("ProductAcceptsUniqueProductUserName", StringComparison.OrdinalIgnoreCase));
                ProductAcceptsUniqueProductUserName = (productInternalSettingAcceptsUniqueProductUserName != null) && productInternalSettingAcceptsUniqueProductUserName.Value.Trim() == "1";
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetProductEndPointDetails", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}" });
                throw;
            }
        }

        protected virtual void ApplyApiSecurity()
        {
            try
            {
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // 3.a — Explicit timeout. Without this the HttpClient default is 100 seconds,
                // which can tie up batch threads for nearly two minutes per hanging API call.
                // Read from the per-product 'ApiTimeoutSeconds' setting so individual products
                // can tune the value; fall back to the class-level constant otherwise.
                string apiTimeoutSetting = ProductInternalSettingList
                    .FirstOrDefault(a => a.Name.Equals("ApiTimeoutSeconds", StringComparison.OrdinalIgnoreCase))?.Value;
                int timeoutSeconds = int.TryParse(apiTimeoutSetting, out int parsedTimeout) && parsedTimeout > 0
                    ? parsedTimeout
                    : DefaultApiTimeoutSeconds;
                _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                string tokenScopes = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("TokenAuthScopes", StringComparison.OrdinalIgnoreCase))?.Value;
                string apiUser = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("ApiUserName", StringComparison.OrdinalIgnoreCase))?.Value;
                string apiPassword = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("ApiPassword", StringComparison.OrdinalIgnoreCase))?.Value;
                string apiSecret = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("ApiSecret", StringComparison.OrdinalIgnoreCase))?.Value;
                string tokenURL = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("TokenURL", StringComparison.OrdinalIgnoreCase))?.Value;
                string clientId = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("ApiCode", StringComparison.OrdinalIgnoreCase))?.Value;

                if (tokenScopes != null)
                {
                    if (_tokenHelper == null)
                    {
                        _tokenHelper = new TokenHelper();
                    }

                    var ulToken = _tokenHelper.GetUnifiedLoginServerToken(tokenScopes);
                    _httpClient.SetBearerToken(ulToken);
                }
                else if (!string.IsNullOrEmpty(apiSecret) && !string.IsNullOrEmpty(clientId))
                {
                    string tokenGrantType = TokenGrantTypePassword;
                    if (_tokenHelper == null)
                        _tokenHelper = new TokenHelper();
                    if (!string.IsNullOrEmpty(tokenGrantType) && tokenGrantType.Equals(TokenGrantTypePassword, StringComparison.OrdinalIgnoreCase))
                    {
                        string jsonResponse;
                        using (var client = new HttpClient())
                        {
                            var request = new FormUrlEncodedContent(new Dictionary<string, string>
                            {
                                {"grant_type", TokenGrantTypePassword},
                                {"client_id", clientId?.Trim()},
                                {"client_secret", apiSecret?.Trim()},
                                {"username", apiUser?.Trim()},
                                {TokenGrantTypePassword, apiPassword?.Trim()},
                            }
                            );
                            request.Headers.Add("X-PrettyPrint", "1");

                            // 3.b — Avoid deadlock by running async work on a thread-pool thread
                            // that has no SynchronizationContext. In ASP.NET classic, calling
                            // .Result directly on a Task captures the current sync context and
                            // waits for a continuation that can never run on the same captured
                            // thread — a classic deadlock. Task.Run offloads to a context-free
                            // thread-pool thread so continuations complete without contention.
                            var response = Task.Run(() => client.PostAsync(tokenURL, request)).GetAwaiter().GetResult();
                            jsonResponse = Task.Run(() => response.Content.ReadAsStringAsync()).GetAwaiter().GetResult();
                            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
                            _httpClient.SetBearerToken(values["access_token"]);
                            return;
                        }
                    }
                }

                string ignoreBasicAuthHeader = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("SI_IgnoreApiBasicAuthHeader", StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrWhiteSpace(apiUser) && !string.IsNullOrWhiteSpace(apiPassword) && (string.IsNullOrWhiteSpace(ignoreBasicAuthHeader) || ignoreBasicAuthHeader == "0"))
                {
                    _httpClient.SetBasicAuthentication(apiUser, apiPassword);
                }

                string apiKey = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("ApiKey", StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
                }

                var includeCompanyIdHeader = ProductInternalSettingList.FirstOrDefault(a => a.Name.Equals("Kong-IncludeCompanyIdHeader", StringComparison.OrdinalIgnoreCase))?.Value;
                if (includeCompanyIdHeader == "1")
                {
                    _httpClient.DefaultRequestHeaders.Add("company-id", CompanyInstanceSourceId);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ApplyApiSecurity", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}" });
                throw;
            }
        }

        private bool ValidateEditorUser(UserDetails editorUserDetails)
        {
            if (editorUserDetails.PersonaId == 0)
            {
                return false;
            }

            // verify the persona belongs to the current user
            if (_managePersona == null)
                _managePersona = new ManagePersona();

            var editor = _managePersona.GetPersona(editorUserDetails.PersonaId);
            if (editor == null || editor.RealPageId != editorUserDetails.UserRealPageId)
            {
                return false;
            }

            return true;
        }

        private void GetBlueBookProductMapAndCompanyDetails(long subjectPersonaId)
        {
            try
            {
                // Get Blue book product Code & company instance Id
                BlueBookGbProductMap = _dataCollector.GetBlueBookProductMap(ProductId);

                string blueBookProductCode = BlueBookGbProductMap.UDMSourceCode?.Length > 0 ? BlueBookGbProductMap.UDMSourceCode : BlueBookGbProductMap.BooksProductCode;
                // Get Company Books Instance Source Id
                var userBooksMasterId = EditorUserDetails.BooksCustomerMasterId;
                if (subjectPersonaId != 0)
                {
                    userBooksMasterId = SubjectUserDetails.BooksCustomerMasterId;
                }
                
                var overrideCompanyInstanceSourceId = CheckForOverrideCompanyIdForProduct();
                if (string.IsNullOrEmpty(overrideCompanyInstanceSourceId))
                {
                    CompanyInstanceSourceId = _dataCollector.GetProductCompanyMap(blueBookProductCode, userBooksMasterId, _userClaims, EditorUserDetails.OrganizationDomain).CompanyInstanceSourceId;
                }
                else
                {
                    CompanyInstanceSourceId = overrideCompanyInstanceSourceId;
                }

                if (string.IsNullOrEmpty(CompanyInstanceSourceId))
                {
                    throw new BlueBookException("Company Setup Error: Please Contact Support.");
                }

                
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetBlueBookProductMapAndCompanyDetails", $"Product {ProductId} editorPersona id - {EditorUserDetails.PersonaId}" });
                // 6.d — Use bare `throw` to rethrow with the original stack trace intact.
                // `throw ex` resets the stack trace to this catch block, hiding the actual
                // call site where the exception originated and making root-cause analysis harder.
                throw;
            }
        }

        private string CheckForOverrideCompanyIdForProduct()
        {
            var overridePMCId = "";
            var rpcache = new RPObjectCache();
            var cacheKey = $"orgProductSettings_{_userClaims.OrganizationRealPageGuid}_{ProductId}";
            IList<ProductSettingList> orgProductSettingList = rpcache.GetFromCache(cacheKey, 300, () => _productRepository.GetProductSettings(_userClaims.OrganizationRealPageGuid, ProductId));
            if (orgProductSettingList.Any(p => p.Name.Equals("OverridePMCID", StringComparison.OrdinalIgnoreCase)))
            {
                var overridePMC = orgProductSettingList.FirstOrDefault(p => p.Name.Equals("OverridePMCID", StringComparison.OrdinalIgnoreCase))?.Value;
                if (overridePMC == null) return overridePMCId;

                overridePMCId = overridePMC;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckForOverrideCompanyIdForProduct", $"Found OverridePMCID {overridePMC}" });
            }

            return overridePMCId;
        }

        #endregion
    }
}