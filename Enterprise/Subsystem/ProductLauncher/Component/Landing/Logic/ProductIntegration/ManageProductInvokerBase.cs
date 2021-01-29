using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration
{
    /// <summary>
    /// Manage Product Invoker Abstract Base Class
    /// Purpose of this class is ONLY product integration
    /// Do not add any other methods which do not make product API calls
    /// </summary>
    public abstract class ManageProductInvokerBase
    {
        #region Private Variables

        protected static HttpClient _httpClient;
        protected DefaultUserClaim _userClaims;

        private const string PRODUCT_SETTINGTYPE_STATUS = "ProductStatus";

        #endregion

        #region Properties

        protected ProductEnum ProductType;
        protected int ProductId;
        protected IDataCollector _dataCollector;
        private IManagePersona _managePersona;
        private IProductInternalSettingRepository _productInternalSettingRepository;

        protected UserDetails EditorUserDetails { get; set; }
        protected UserDetails SubjectUserDetails { get; set; }
        protected string ProductApiBaseUrl { get; set; }
        protected string CompanyInstanceSourceId { get; set; }
        protected IList<ProductInternalSetting> ProductInternalSettingList { get; set; }

        /// <summary>
        /// Correlation Id used for logging
        /// </summary>
        protected Guid CorrelationId { get; set; }

        /// <summary>
        /// GB # Blue Book Product Map (Used for logging etc)
        /// </summary>
        protected GbProductMap BlueBookGbProductMap { get; set; }

        /// <summary>
        /// Productudm source code
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
        protected ManageProductInvokerBase(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims)
        {
            _dataCollector = new DataCollector();
            Init(productType, editorPersonaId, subjectPersonaId, userClaims);
        }

        /// <summary>
        ///  Used for unit testing
        /// </summary>
        protected ManageProductInvokerBase(ProductEnum productType, long editorPersonaId, long subjectPersonaId,
            DefaultUserClaim userClaims, IDataCollector injectedDataCollector, IManagePersona injectedManagePersona,
            IProductInternalSettingRepository injectedProductInternalSettingRepository)
        {
            _managePersona = injectedManagePersona;
            _productInternalSettingRepository = injectedProductInternalSettingRepository;
            _dataCollector = injectedDataCollector;

            Init(productType, editorPersonaId, subjectPersonaId, userClaims);
        }

        private void Init(ProductEnum productType, long editorPersonaId, long subjectPersonaId, DefaultUserClaim userClaims)
        {
            ProductType = productType;
            ProductId = (int) ProductType;
            _userClaims = userClaims;

            // Get editor & subject user details & Verify editor user is the logged-in user
            GetValidateEditorSubjectUserDetails(editorPersonaId, subjectPersonaId);

            // Get product & end point details
            GetProductEndPointDetails();

            // Apply API security to HTTP Client
            ApplyApiSecurity();

            // Get Blue book product Code & company instance Id
            GetBlueBookProductMapAndCompanyDetails(subjectPersonaId);

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
                    $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRoleEndpoint);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                bool isCompanyIdRequiredToQuery = baseUrlAndQuery.Contains("{0}");
                if (isCompanyIdRequiredToQuery)
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId, "false");

                var roleList = GetResultFromApi<IList<ProductRole>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {roleList?.Count}");

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");
                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

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
                WriteToErrorLog($"ManageProductInvokerBase.GetProductRoles - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
        public virtual ListResponse GetProductRightsForRole(RequestParameter dataFilter, long roleId, string baseUrlAndQuery = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRightsForRole - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRightEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, roleId);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRightsForRole - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");


                var rightList = GetResultFromApi<IList<ProductRight>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductRightsForRole - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {rightList?.Count}");

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductInvokerBase.GetProductRightsForRole - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");
                    var user = GetProductUser();

                    // map user roles
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductInvokerBase.GetProductRightsForRole - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

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
                WriteToErrorLog($"ManageProductInvokerBase.GetProductRightsForRole - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
                    $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                //Get all rights by company
                baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetRightEndpoint);
                baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                var allRights = GetResultFromApi<IList<ProductRight>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received roleList with count = {allRights?.Count}");

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
                WriteToErrorLog($"ManageProductInvokerBase.GetAllRights - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
                    $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                }

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                Dictionary<string, bool> additionalData = new Dictionary<string, bool>();
                var propertyList = GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}");

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");

                    var user = GetProductUser();

                    // map user properties
                    if (user != null && user.Properties != null)
                    {
                        if (user.Properties.Contains("all"))
                        {
                            additionalData.Add("allProperties", true);
                        }
                        WriteToDiagnosticLog(
                            $"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

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
                WriteToErrorLog($"ManageProductInvokerBase.GetProductProperties - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
                    $"ManageProductInvokerBase.GetProductPropertiesByGroup - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");
                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyByGroupEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, groupId);
                }

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductPropertiesByGroup - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                var propertyList = GetResultFromApi<IList<ProductProperties>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductPropertiesByGroup - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received propertyList with count = {propertyList?.Count}");

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
                WriteToErrorLog($"ManageProductInvokerBase.GetProductPropertiesByGroup - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
        public virtual ListResponse GetProductPropertyGroups(RequestParameter dataFilter, string baseUrlAndQuery = null , string tabName = null)
        {
            try
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductPropertyGroups - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

                if (string.IsNullOrEmpty(baseUrlAndQuery))
                {
                    baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetPropertyGroupsEndpoint);
                    baseUrlAndQuery = string.Format(baseUrlAndQuery, CompanyInstanceSourceId);
                }

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductPropertyGroups - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At API calling - {baseUrlAndQuery}");

                var groupList = GetResultFromApi<IList<ProductPropertyGroups>>(baseUrlAndQuery);

                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.GetProductPropertyGroups - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received regionList with count = {groupList?.Count}");

                if (!string.IsNullOrEmpty(SubjectUserDetails?.ProductUserName))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductInvokerBase.GetProductPropertyGroups - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling GetUser for subject persona Id -{SubjectUserDetails.PersonaId}");
                    var user = GetProductUser();

                    // map user regions
                    if (user != null)
                    {
                        WriteToDiagnosticLog(
                            $"ManageProductInvokerBase.GetProductPropertyGroups - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling Merge for subject persona Id -{SubjectUserDetails.PersonaId}");

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
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductInvokerBase.GetProductPropertyGroups - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Error - {ex.Message}", null, ex);
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
                $"ManageProductInvokerBase.UnassignUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method, calling DeleteUser().");

            var productUserProfile = new ProductUserProfile
            {
                UserId = SubjectUserDetails.ProductUserId,
                IsActive = false,
                CompanyId = CompanyInstanceSourceId,
                LoginName = SubjectUserDetails.ProductUserName,
                Email = SubjectUserDetails.Email,
                FirstName = SubjectUserDetails.FirstName,
                LastName = SubjectUserDetails.LastName
            };

            // Delete / deactivate uer in the product
            var result = DeleteUser(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.UnassignUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns success, updating Greenboook status.");

                IManageUserLogin manageUserLogin = new ManageUserLogin();
                IUserLoginRepository userLoginRepository = new UserLoginRepository();

                var userLogin = manageUserLogin.GetUserLoginOnly(SubjectUserDetails.UserRealPageId);
                Persona persona = _managePersona.GetPersona(SubjectUserDetails.PersonaId);

                OrganizationStatus orgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, false);
                //var organizationList = userLoginRepository.ListOrganizationWithoutStatusByUserId(userLogin.UserId);
                //OrganizationStatus orgStatus = organizationList.FirstOrDefault(p => p.PartyId == persona.OrganizationPartyId);

                int statusValue = (int) UserUiStatusType.AccountHidden;

                //if user is disabled then set status to deactivated instead hidden
                if (orgStatus.Status.ToString().Equals(UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    statusValue = (int) UserUiStatusType.Deactivated;
                }

                // Update product status in green book
                _dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, statusValue);

                // Activity Logging
                ProductActivityLogger.WriteUnassignUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode, CorrelationId);

                return string.Empty;
            }

            Dictionary<string, object> logData = new Dictionary<string, object> {{"result", result}};
            WriteToErrorLog($"ManageProductInvokerBase.UnassignUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. DeleteUser() returns fail", logData);

            return result.Content;
        }

        /// <summary>
        /// Change product user type
        /// </summary> 
        public string ChangeProductUserType(ProductUserRolePropertiesGroups userRolePropertiesRegion, BatchProcessType batchProcessType)
        {
            return CreateUpdateProductUser(userRolePropertiesRegion, batchProcessType);
        }

        /// <summary>
        /// Create or update product user
        /// Gets called from Product-Batch
        /// </summary> 
        public virtual string CreateUpdateProductUser(ProductUserRolePropertiesGroups userRolePropertiesRegion, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            string result;

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method.");

            // Get product user object 
            var newProductUser = GenerateProductUserObject(userRolePropertiesRegion);

            if (string.IsNullOrEmpty(SubjectUserDetails.ProductUserName))
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling CreateUser.");

                // Get User & check if already exist 
                bool isUserExistInProduct = CheckUserExistInProduct(newProductUser.LoginName);
                if (isUserExistInProduct)
                {
                    WriteToErrorLog(
                        $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Product User {newProductUser.LoginName} already exist.");

                    return $"{newProductUser.LoginName} already exist in the product {ProductType}.";
                }

                // Create User
                result = CreateUser(newProductUser);

            }
            else
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUpdateProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling UpdateUser.");
                // Update user with Id/Login from product
                newProductUser.UserId = SubjectUserDetails.ProductUserId;
                newProductUser.LoginName = SubjectUserDetails.ProductUserName;

                result = UpdateUser(newProductUser, batchProcessType);
            }

            return result;
        }

        /// <summary>
        /// Get Product User API call
        /// </summary> 
        public virtual IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
        {
            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

            // Get partial api query based on end point
            if (string.IsNullOrEmpty(baseUrlAndQuery))
                baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint), SubjectUserDetails.ProductUserName);

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.GetProductUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

            return GetResultFromApi<IntegrationProductUser>(baseUrlAndQuery, isThrowOnError);
        }

        /// <summary>
        /// Returns companies for a product
        /// </summary>
        public virtual ListResponse GetProductOrganizations(string roleOrganizationId, string organizationType, RequestParameter dataFilter, string baseUrlAndQuery = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete User - patch with isActive = false
        /// </summary> 
        protected virtual ApiResponse DeleteUser(ProductUserProfile profile = null)
        {
            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.DeleteUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

            // patch to se isActive flag to false
            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PatchProfileEndpoint);

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.DeleteUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, profile);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            return integration.PatchEntity<string>(profile);
        }

        /// <summary>
        /// Create a user in the product
        /// </summary>
        protected virtual string CreateUser(IntegrationProductUser productUser)
        {
            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PostUserEndpoint);

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

            // dump api info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.PostEntity<IntegrationProductUser>(productUser);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Greenbook mapping.");

                // map product user in green book
                _dataCollector.CreateProductUserInGreenBook(SubjectUserDetails.PersonaId, result.Content, ProductId, productUser.LoginName);

                // OPTIONAL - If product needs more attributes than userid or loginName then override in the product (e.g. PAM uses)
                CreateAdditionalSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser);

                // activity logging
                ProductActivityLogger.WriteCreateUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode,
                    CorrelationId);

                return string.Empty;
            }
            Dictionary<string, object> logData = new Dictionary<string, object> {{"result", result}};
            WriteToErrorLog(
                $"ManageProductInvokerBase.CreateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}.", logData);

            return result.Content;
        }

        /// <summary>
        /// Update a user in the product
        /// </summary>
        protected virtual string UpdateUser(IntegrationProductUser productUser, BatchProcessType batchProcessType)
        {
            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.UpdateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of the method.");

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PutUserEndpoint);

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.UpdateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Calling API - {baseUrlAndQuery}.");

            // dump API call info
            DumpApiCallInfoToDiagnosticLog(baseUrlAndQuery, productUser);

            var integration = new ApiIntegration(_httpClient, baseUrlAndQuery);
            var result = integration.PutEntity<IntegrationProductUser>(productUser);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.UpdateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. Received success. Updating Greenbook mapping.");

                if (batchProcessType == BatchProcessType.CreateUpdateProductUser)
                {
                    // activity logging
                    ProductActivityLogger.WriteUpdateUserActivityLog(EditorUserDetails, SubjectUserDetails,
                        BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode, CorrelationId);
                }
                else if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType ==
                    BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                {
                    // activity logging
                    ProductActivityLogger.WriteUpdateUserTypeActivityLog(EditorUserDetails, SubjectUserDetails,
                        BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode, CorrelationId, batchProcessType);
                }

                _dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, (int) ProductBatchStatusType.Success);

                UpdateSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUser.UserId, productUser.LoginName, productUser.Email);

                return string.Empty;
            }
            Dictionary<string, object> logData = new Dictionary<string, object> {{"result", result}};
            WriteToErrorLog(
                $"ManageProductInvokerBase.UpdateUser - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}.", logData);

            return result.Content;
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
        /// Returns true if user exists in the product
        /// </summary> 
        protected virtual bool CheckUserExistInProduct(string loginNameToCheck, string baseUrlAndQuery = null)
        {
            if (baseUrlAndQuery == null)
                baseUrlAndQuery = string.Format(GetOperationEndPoint(ProductEntityEndpointKeyEnum.GetUserEndpoint), loginNameToCheck);

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
                $"ManageProductInvokerBase.UpdateProductUserProfile - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}. At beginning of method.");

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
                $"ManageProductInvokerBase.UpdateUserProfile - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}, subjectPersonaId - {SubjectUserDetails.PersonaId}. At beginning of the method.");

            // Call shared method to update profile from green-book or from external source (migration tool)
            var result = ProductUserProfileChange(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                WriteToDiagnosticLog(
                    $"ManageProductInvokerBase.UpdateUserProfile - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId} subjectPersonaId - {SubjectUserDetails.PersonaId}. Received success. Updating Greenbook mapping.");

                // activity logging
                ProductActivityLogger.WriteUpdateUserActivityLog(EditorUserDetails, SubjectUserDetails, BlueBookGbProductMap.Name, BlueBookGbProductMap.BooksProductCode,
                    CorrelationId);

                _dataCollector.UpdateProductSettingProductStatus(SubjectUserDetails.PersonaId, PRODUCT_SETTINGTYPE_STATUS, ProductId, (int) ProductBatchStatusType.Success);

                UpdateSamlUserAttribute(SubjectUserDetails.PersonaId, ProductId, productUserProfile.UserId, productUserProfile.LoginName, productUserProfile.Email);

                return string.Empty;
            }

            Dictionary<string, object> logData = new Dictionary<string, object> {{"result", result}};
            WriteToErrorLog(
                $"ManageProductInvokerBase.UpdateUserProfile - Product {ProductType} " +
                $"editorPersona id - {EditorUserDetails.PersonaId} productUserProfile.UserId - {productUserProfile.UserId}.");

            return result.Content;
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
                $"ManageProductInvokerBase.ProductUserProfileChange - Product {ProductType} editorPersona id - " +
                $"{EditorUserDetails.PersonaId}, productUserProfile.UserId - {productUserProfile.UserId}. At beginning of the method.");

            var baseUrlAndQuery = GetOperationEndPoint(ProductEntityEndpointKeyEnum.PatchProfileEndpoint);

            WriteToDiagnosticLog(
                $"ManageProductInvokerBase.ProductUserProfileChange - Product {ProductType} editorPersona id - " +
                $"{EditorUserDetails.PersonaId}  productUserProfile.UserId - {productUserProfile.UserId}. Calling API - {baseUrlAndQuery}.");

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
                WriteToErrorLog($"ManageProductInvokerBase.GetMigrationUsers - no users received from product.");
                return response;
            }

            WriteToDiagnosticLog($"ManageProductInvokerBase.GetMigrationUsers - Received users from product.");
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
            var logData = new Dictionary<string, object> {{"result", result}};
            if (result.IsSuccessStatusCode)
            {
                var migrationResponse = JsonConvert.DeserializeObject<MigrateResponse>(JsonConvert.SerializeObject(result.Content));
                WriteToDiagnosticLog($"ManageProductMarketingCenter.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
                return migrationResponse;
            }

            WriteToErrorLog($"ManageProductMarketingCenter.UpdateUsersMigrationStatus.PostAsJsonAsync {result}", logData);
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
                $"ManageProductInvokerBase.ExternalProductUserProfileChange - Product {ProductType} " +
                $"editorPersona id - {EditorUserDetails.PersonaId}, productUserProfile.UserId - {productUserProfile.UserId}. At beginning of the method.");

            // used from external source (migration tool) so no activity logging required
            var result = ProductUserProfileChange(productUserProfile);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }

            // log exception details from result
            var logData = new Dictionary<string, object> {{"result", result}};
            WriteToErrorLog(
                $"ManageProductInvokerBase.ExternalProductUserProfileChange - Product {ProductType} " +
                $"editorPersona id - {EditorUserDetails.PersonaId} productUserProfile.UserId - {productUserProfile.UserId}.", logData);

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
                IsActive = true,
                PropertyGroups = userRolePropertiesRegion.PropertyGroupList,
                Properties = userRolePropertiesRegion.PropertyList,
                Roles = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString()),
                PropertyRoles = userRolePropertiesRegion.PropertyRoleList,
                OrganizationRoles = userRolePropertiesRegion.OrganizationRoleList,
                CanReceiveMonthlyReport = userRolePropertiesRegion.CanReceiveMonthlyReport,
                PropertyRoleList = userRolePropertiesRegion.RolePropertiesList,
                RoleList = userRolePropertiesRegion.RoleList?.ConvertAll<string>(x => x.ToString())
            };

            if (SubjectUserDetails.UserRoleTypeId == (int) UserRoleType.SuperUser)
            {
                ApplySuperUserData(productUser);
            }

            return productUser;
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
            Dictionary<string, object> logData = new Dictionary<string, object>();
            logData.Add("baseUrlAndQuery", baseUrlAndQuery);

            if (apiPayLoad != null)
                logData.Add("apiPayLoad", JsonConvert.SerializeObject(apiPayLoad));

            WriteToDiagnosticLog($"API Call for product {ProductType} is getting called.", logData);
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
            var partialApiQueryUrl = ProductInternalSettingList.First(a => a.Name.ToUpper() == entityType.ToString().ToUpper()).Value;
            if (string.IsNullOrEmpty(partialApiQueryUrl))
            {
                throw new Exception();
            }

            return ProductApiBaseUrl + partialApiQueryUrl;
        }

        protected string GetProductInternalSettingValue(string settingName)
        {
            // Get product setting value
            var settingValue = ProductInternalSettingList.First(a => a.Name.ToUpper() == settingName.ToUpper()).Value;
            return settingValue;
        }

        #endregion

        #region Logging Methods

        protected void WriteToInformationLog(string message, Dictionary<string, object> logData = null)
        {
            WriteToLog(LogEventLevel.Information, message, logData);
        }

        protected void WriteToErrorLog(string message, Dictionary<string, object> logData = null, Exception exception = null)
        {
            WriteToLog(LogEventLevel.Error, message, logData, exception);
        }

        protected void WriteToDiagnosticLog(string message, Dictionary<string, object> logData = null)
        {
            WriteToLog(LogEventLevel.Debug, message, logData);
        }

        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null)
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
            logger.Write(logType, exception, message );
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
                WriteToErrorLog(
                    $"ManageProductInvokerBase.GetValidateEditorSubjectUserDetails - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}.", null, ex);

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
                ProductApiBaseUrl = ProductInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            }
            catch (Exception ex)
            {
                WriteToErrorLog(
                    $"ManageProductInvokerBase.GetValidateEditorSubjectUserDetails - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}.", null, ex);

                throw;
            }
        }

        private void ApplyApiSecurity()
        {
            try
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();

                var apiSecurity = new ProductApiSecurity(ProductType, ProductInternalSettingList);
                apiSecurity.ApplySecurityToHttpClient(_httpClient);
            }
            catch (Exception ex)
            {
                WriteToErrorLog(
                    $"ManageProductInvokerBase.ApplyApiSecurity - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}.", null, ex);

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
                    userBooksMasterId = SubjectUserDetails.BooksCustomerMasterId;
                CompanyInstanceSourceId = _dataCollector.GetProductCompanyMap(blueBookProductCode, userBooksMasterId, _userClaims, EditorUserDetails.OrganizationDomain).CompanyInstanceSourceId;

                if (string.IsNullOrEmpty(CompanyInstanceSourceId))
                {
                    throw new BlueBookException("Company Setup Error: Please Contact Support.");
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog(
                    $"ManageProductInvokerBase.GetBlueBookProductCodeAndCompanyDetails - Product {ProductType} editorPersona id - {EditorUserDetails.PersonaId}.", null, ex);

                throw ex;
            }
        }

        #endregion
    }
}