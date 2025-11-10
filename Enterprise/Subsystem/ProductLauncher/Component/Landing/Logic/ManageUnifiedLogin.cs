using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class ManageUnifiedLogin : ManageProductBase, IManageUnifiedLogin
    {
        private readonly DefaultUserClaim _userClaims;
        private readonly IProductRepository _productRepository;
        private readonly IUserRoleRightRepository _userRoleRightRepository;
        private readonly IManageUnifiedSettings _manageUnifiedSettings;
        private const string PRODUCT_ROLE_CREATE = "{\"action\":\"Created Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLE_DELETE = "{\"action\":\"Deleted Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLE_UPDATE = "{\"action\":\"Updated Role\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLE_USERDEFAULT = "{\"action\":\"Role Set as User Default\",\"value\":\"RoleName\"}";
        private const string RIGHT_ASSIGN = "{\"action\":\"Added Rights\",\"value\":\"RightName\"}";
        private const string RIGHT_UNASSIGN = "{\"action\":\"Removed Rights\",\"value\":\"RightName\"}";
        private const string ROLE_ASSIGN = "{\"action\":\"Added Roles\",\"value\":\"RoleName\"}";
        private const string ROLE_UNASSIGN = "{\"action\":\"Removed Roles\",\"value\":\"RoleName\"}";

        #region Ctor
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="userClaims"></param>
        public ManageUnifiedLogin(DefaultUserClaim userClaims) : base((int) ProductEnum.UnifiedPlatform, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _userClaims = userClaims;
            _productId = (int) ProductEnum.UnifiedPlatform;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaims);
            _productRepository = new ProductRepository(userClaims);
            _userRoleRightRepository = new UserRoleRightRepository();
            _manageUnifiedSettings = new ManageUnifiedSettings(userClaims);
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="userClaims"></param>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        public ManageUnifiedLogin(IRepository repository, DefaultUserClaim userClaims, HttpMessageHandler messageHandler) : base((int)ProductEnum.UnifiedPlatform, userClaims, repository, messageHandler)
        {
            _productId = (int)ProductEnum.UnifiedPlatform;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaims, repository, messageHandler);
            _userClaims = userClaims;
            _productRepository = new ProductRepository(repository, userClaims);
            _userRoleRightRepository = new UserRoleRightRepository(repository, userClaims);
            _manageUnifiedSettings = new ManageUnifiedSettings(repository, userClaims, messageHandler);
        }

        #endregion

        #region Public Methods

        #region Properties and Roles

        /// <summary>
        /// Used to get the list of properties for the company or for the given user for user setup
        /// </summary>
        /// <param name="editorPersonaId">User making the request</param>
        /// <param name="userPersonaId">The user id to merge with the property list, if used. 0 for all properties</param>
        /// <param name="assignedOnly">Only return the properties assigned to the given user persona id</param>
        /// <param name="datafilter">A datafilter used to filter the properties. Not currently used</param>
        /// <returns></returns>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                //long companyId = _editorPersona.Organization.BooksMasterId;
                long companyMasterId = _editorPersona.Organization.BooksCustomerMasterId;

                if (companyMasterId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    return new ListResponse {IsError = true, ErrorReason = CommonMessageConstants.CompanyErrorMessage};
                }

                IList<ProductProperty> customerPropertyList = _blueBook.GetCustomerProperty(companyMasterId, null, null, false);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Completed for user with editorPersona id -{editorPersonaId}." });

                // need to do a filter on the result
                if (userPersonaId != 0)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Calling MergeProductPropertiesWithGreenbook for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    result = MergeProductPropertiesWithGreenbook(customerPropertyList, userPersonaId, assignedOnly);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Completed for user with editorPersona id -{editorPersonaId}." });
                }
                else
                {
                    result = new ListResponse() // create new user
                    {
                        Records = customerPropertyList.Cast<object>().ToList(),
                        TotalRows = customerPropertyList.Count,
                        RowsPerPage = customerPropertyList.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorReason = $"ManageProductProspectContact.GetProperties - There was a problem getting the properties.";
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}." });
            }

            return result;
        }

        public ListResponse GetEnterpriseProperties(long userPersonaId, string include = null)
        {
            ListResponse response = new ListResponse();
            var productInternalSettingList = GetProductSetting((int)ProductEnum.UnifiedPlatform);
            bool usePropertyInstances = (productInternalSettingList?.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase))?.Value) == "1";
            if (!usePropertyInstances)
            {
                response = GetCustomerProperties(userPersonaId, include);
            }
            else
            {
                response = GetUPFMProperties(userPersonaId, include);
            }
            return response;
        }

        /// <summary>
        /// Gets the ProductInternalSetting for a given productId.
        /// </summary>
        /// <param name="productId">The product id.</param>
        /// <returns>The ProductInternalSetting object for the product, or null if not found.</returns>
        public List<ProductInternalSetting> GetProductInternalSettingByProductId(int productId)
        {
            var productInternalSettings = _productInternalSettingRepository.GetProductInternalSettings(productId);
            return productInternalSettings;
        }

        /// <summary>
        /// Get the list of properties for the given user to be used by external systems
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public ListResponse GetCustomerProperties(long userPersonaId, string include = null)
        {
            ListResponse response = new ListResponse();

            List<ProductProperty> productPropertyList = GetAssignedPropertyForPersona(userPersonaId, (int)ProductEnum.UnifiedPlatform);

            if (productPropertyList != null)
            {
                IList<ProductProperty> blueBookPropertyList = _blueBook.GetCustomerProperty(_userClaims.CustomerMasterId);
                if ((productPropertyList.Count == 1) && (productPropertyList[0].ID.Equals("-1")))
                {
                    productPropertyList = blueBookPropertyList.ToList();
                }
                else
                {
                    productPropertyList = blueBookPropertyList.ToList().FindAll(b => productPropertyList.Any(p => p.ID.Equals(b.ID))).ToList();
                }
            }

            if (productPropertyList.Count > 0)
            {
                string includeFields = string.Empty;

                bool bIncludeFields = ((!string.IsNullOrWhiteSpace(include)) && (include.Split(new char[] {','}).Length > 0));

                if (bIncludeFields)
                {
                    DynamicContractResolver dynamicContractResolver = new DynamicContractResolver(include);
                    string productPropertySerializableProperties = JsonConvert.SerializeObject(
                        productPropertyList,
                        new JsonSerializerSettings()
                        {
                            ContractResolver = dynamicContractResolver
                        }
                    );
                    productPropertyList = JsonConvert.DeserializeObject<List<ProductProperty>>(productPropertySerializableProperties);
                }

                productPropertyList.ForEach(p =>
                {
                    p.IsAssigned = null;
                    p.disableSelection = null;
                });

                response.IsError = false;
                response.Records = productPropertyList.Cast<object>().ToList();
                response.TotalRows = productPropertyList.Count;
                response.RowsPerPage = productPropertyList.Count;
                response.TotalPages = 1;
                response.ErrorReason = string.Empty;
            }

            return response;
        }

        /// <summary>
        /// Get a list of UPFM property instances for the give user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="assignedOnly"></param>
        /// <param name="product"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUPFMProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, ProductEnum product, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
            List<ProductProperty> productPropertyList = new List<ProductProperty>();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                var booksPropertyList = _blueBook.GetUPFMPropertyInstances(_userClaims.OrganizationRealPageGuid.ToString());
                var customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"ListUPFMPropertyInstanceIdByInstanceIds completed for user with editorPersona id -{editorPersonaId}." });

                // need to do a filter on the result
                if (userPersonaId != 0)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"Calling MergeUPFMBooksPropertiesWithUPFMProperties for user with editorPersona id -{editorPersonaId} & userPersonaId-{userPersonaId}." });
                    result = MergeUPFMBooksPropertiesWithProductProperty(customerPropertyList, userPersonaId, assignedOnly);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"MergeUPFMBooksPropertiesWithUPFMProperties completed for user with editorPersona id -{editorPersonaId}." });
                }
                else
                {
                    foreach (UPFMPropertyInstance upfmPropertyInstance in customerPropertyList)
                    {
                        var pp = ConvertUPFMPropertyInstanceToProductProperty(upfmPropertyInstance, false);
                        productPropertyList.Add(pp);
                    }

                    result = new ListResponse() // create new user
                    {
                        Records = productPropertyList.Cast<object>().ToList(),
                        TotalRows = productPropertyList.Count,
                        RowsPerPage = productPropertyList.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                result.IsError = true;
                result.ErrorReason = $"ManageProductProspectContact.GetProperties - There was a problem getting the properties.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}." }, exception: ex);
            }

            return result;
        }

        /// <summary>
        /// Get the list of property instances for the given user to be used by external systems
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public ListResponse GetUPFMProperties(long userPersonaId, string include = null)
        {
            ListResponse response = new ListResponse();

            var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, ProductEnum.UnifiedPlatform);
            List<ProductProperty> userPropertyList = new List<ProductProperty>();
            List<UPFMPropertyInstance> customerPropertyList = new List<UPFMPropertyInstance>();

            if (userPropertyIdList != null)
            {
                var booksPropertyList = _blueBook.GetUPFMPropertyInstances(_userClaims.OrganizationRealPageGuid.ToString());
                if (booksPropertyList != null)
                {
                    customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);
                }

                if (userPropertyIdList.Count == 1 && userPropertyIdList[0] == -1 )
                {
                    customerPropertyList.ForEach(cp =>
                    {
                        userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true));
                    });
                }
                else
                {
                    customerPropertyList.ToList().FindAll(b => userPropertyIdList.Any(p => p == b.PropertyInstanceId)).ForEach(cp => { userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true)); });
                }
            }

            if (userPropertyIdList?.Count > 0)
            {
                bool bIncludeFields = (!string.IsNullOrWhiteSpace(include) && include.Split(new char[] {','}).Length > 0);

                if (bIncludeFields)
                {
                    DynamicContractResolver dynamicContractResolver = new DynamicContractResolver(include);
                    string productPropertySerializableProperties = JsonConvert.SerializeObject(
                        userPropertyList,
                        new JsonSerializerSettings()
                        {
                            ContractResolver = dynamicContractResolver
                        }
                    );
                    userPropertyList = JsonConvert.DeserializeObject<List<ProductProperty>>(productPropertySerializableProperties);
                }

                userPropertyList.ForEach(p =>
                {
                    p.IsAssigned = null;
                    p.disableSelection = null;
                });

                response.IsError = false;
                response.Records = userPropertyList.Cast<object>().ToList();
                response.TotalRows = userPropertyList.Count;
                response.RowsPerPage = userPropertyList.Count;
                response.TotalPages = 1;
                response.ErrorReason = string.Empty;
            }

            return response;
        }

        /// <summary>
        /// Used to add/update a role in Greenbook
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>
        /// <param name="partyId"></param>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <param name="inheritRoleId"></param>
        /// <returns></returns>
        public ListResponse AddUpdateRole(long editorPersonaId, long partyId, long roleId, string roleName, string inheritRoleId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError)
            {
                return response;
            }

            try
            {
                roleName = roleName.Trim();
                UnifiedLoginRepository ocr = new UnifiedLoginRepository();
                if (roleId == 0) // New Role
                {

                    int roleTypeId = (int) UserRoleType.SuperUser;
                    List<CategoryType> catList = GetCategoryType();
                    int roleCategoryId = catList.FirstOrDefault(c => c.CategoryName.ToUpper() == "ROLE TYPE" && c.Status.ToUpper() == "CUSTOM").StatusTypeid;


                    var resp = ocr.AddCustomRole(roleName, "", roleTypeId, roleCategoryId, partyId, _userClaims.UserId,_userClaims.OrganizationType);
                    if (resp.ErrorMessage.Trim() != string.Empty)
                    {
                        response.IsError = true;
                        response.ErrorReason = resp.ErrorMessage;
                    }
                    if (!response.IsError)
                    {
                        AddUpdateRoleLogMessage(editorPersonaId, partyId, roleName, "ADD", "Unified Platform", null, 3);
                    }
                    List<object> role = new List<object>();
                    role.Add(resp.Id);
                    response.Records = role;

                }
                else // Existing role to Edit/Update
                {
                    var oldRoleName = GetRoleName(roleId, (int)ProductEnum.UnifiedPlatform);
                    var resp = ocr.UpdateCustomRole(roleId, roleName, "", _userClaims.UserId);
                    if (resp.ErrorMessage.Trim() != string.Empty)
                    {
                        response.IsError = true;
                        response.ErrorReason = resp.ErrorMessage;
                    }
                    if (!response.IsError)
                    {
                        if (oldRoleName != roleName)
                        {
                            AddUpdateRoleLogMessage(editorPersonaId, partyId, roleName, "UPDATE", "Unified Platform", oldRoleName, 3);
                        }
                    }
                    List<object> role = new List<object>();
                    role.Add(resp.Id);
                    response.Records = role;
                }

            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = ex.Message;
            }

            return response;
        }

        public void AddUpdateRoleLogMessage(long editorPersonaId, long partyId, string roleName, string action,string product, string oldRoleName = null, int productId = 0)
        {
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            UserDetails impersonatorUserInfo = impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            var message = "";
            if (action == "ADD")
            {
                message = impersonatorUserInfo != null
                    ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Created {roleName} in {product}."
                    : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Created {roleName} in {product}.";

                additionalParameters.Add(new AdditionalParameters { Key = "Role", Value = PRODUCT_ROLE_CREATE.Replace("RoleName", roleName) });

            }
            else if (action == "UPDATE")
            {
                message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Updated {oldRoleName} to {roleName} in {product}."
                  : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Updated {oldRoleName} to {roleName} in {product}.";

                additionalParameters.Add(new AdditionalParameters { Key = oldRoleName, Value = PRODUCT_ROLE_UPDATE.Replace("RoleName", roleName) });

            }
            PushToQueue(fromUserLogInfo, message, additionalParameters, productId);
        }

        /// <summary>
        /// Used to Delete a role in Greenbook
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="roleId"></param>

        /// <returns></returns>
        public ListResponse DeleteRole(long editorPersonaId, long roleId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteRole", $"Beginning of method for user with editorPersona id - {editorPersonaId}, roleId - {roleId} " });
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError)
            {
                return response;
            }

            var roleName = GetRoleName(roleId, (int)ProductEnum.UnifiedPlatform);
            try
            {
                UnifiedLoginRepository ocr = new UnifiedLoginRepository();

                // check with logged in editors rights
                List<string> editorRights = _userClaims.Rights;
                List<string> rolesToDel = new List<string>();
                rolesToDel.Add(roleId.ToString());
                var partyId = _userClaims.OrganizationPartyId;

                // Get all rights to check with roles
                IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
                var resp = ocr.DeleteRole(roleId);
                if (resp.ErrorMessage.Trim() != string.Empty)
                {
                    response.IsError = true;
                    response.ErrorReason = resp.ErrorMessage;
                }
                List<object> role = new List<object>();
                role.Add(resp.Id);
                response.Records = role;
                if (!response.IsError)
                {
                    DeleteRoleLogMessage(editorPersonaId, roleId, roleName, "Unified Platform", 3);
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = ex.Message;
            }

            return response;
        }

        public void DeleteRoleLogMessage(long editorPersonaId, long roleId, string roleName, string product, int productId)
        {
                var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
                UserDetails impersonatorUserInfo = impersonatorUserDetails(_userClaims.ImpersonatedBy);

                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                var message = impersonatorUserInfo != null
                      ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) deleted {roleName} in {product}."
                    : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} deleted {roleName} in {product}.";

                additionalParameters.Add(new AdditionalParameters { Key ="Role", Value = PRODUCT_ROLE_DELETE.Replace("RoleName", roleName.ToString()) });
                PushToQueue(fromUserLogInfo, message, additionalParameters, productId);
        }

        public UserDetails impersonatorUserDetails(Guid realpageId)
        {
            if (realpageId != null)
                return _userRepository.GetUserDetails(null, realpageId.ToString());
            else
                return null;
        }

        public string GetRoleName(long roleId, int productId)
        {
            var productIds = GetProductIdsByOrg();
            var gbAllRoles = _unifiedLoginRepository.ListRolesForProductsByPartyId(_userClaims.OrganizationPartyId, productId, productIds);
            var roleName = gbAllRoles.Find(r => r.ID == roleId.ToString()).Name;
            return roleName;
        }

        public string GetRightName(string rightId, int productId)
        {
            var productIds = GetProductIdsByOrg();
            UnifiedLoginRepository umr = new UnifiedLoginRepository();
            var gbAllRights = umr.ListAllRightsForProductsByPartyId(_userClaims.OrganizationPartyId, productId, productIds);
            var rightName = gbAllRights.Find(r => r.ID == int.Parse(rightId)).Description.ToString();
            return rightName;
        }
        /// <summary>
        /// Used to Delete a role in Greenbook
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>       
        /// <param name="roleId"></param>

        /// <returns></returns>
        public ListResponse SetDefaultRole(long editorPersonaId, long partyId, long roleId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError)
            {
                return response;
            }

            try
            {
                UnifiedLoginRepository ocr = new UnifiedLoginRepository();

                var resp = ocr.SetDefaultRole(roleId, partyId, _userClaims.UserId);
                if (resp.ErrorMessage.Trim() != string.Empty)
                {
                    response.IsError = true;
                    response.ErrorReason = resp.ErrorMessage;
                }
                List<object> role = new List<object>();
                role.Add(resp.Id);
                response.Records = role;
                if (!response.IsError)
                {
                    SetDefaultRoleLogMessage(editorPersonaId, partyId, roleId);
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = ex.Message;
            }
            return response;
        }

        public void SetDefaultRoleLogMessage(long editorPersonaId, long partyId, long roleId)
        {
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            UserDetails impersonatorUserInfo = impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            var roleName = GetRoleName(roleId, (int)ProductEnum.UnifiedPlatform);
            var message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) made {roleName} in Unified Platform as default."
                : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} made {roleName} in Unified Platform as default.";

            additionalParameters.Add(new AdditionalParameters { Key = "Role", Value = PRODUCT_ROLE_USERDEFAULT.Replace("RoleName", roleName.ToString()) });
            PushToQueue(fromUserLogInfo, message, additionalParameters, 3);
        }

        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>
        /// <param name="roleId">The role being assigned</param>
        /// <param name="rightsToAdd">A list of right ids to add to the role</param>
        /// <param name="rightsToRemove">A list of right ids to remove from the role</param>
        public ListResponse UpdateRightsToRole(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError)
            {
                return response;
            }

            try
            {
                if (rightsToAdd != null && rightsToRemove != null)
                {
                    if (rightsToAdd.Count > 0 || rightsToRemove.Count > 0)
                    {
                        LinkRightsToRole(editorPersonaId, roleId, rightsToAdd, rightsToRemove);
                    }
                }
                if (!response.IsError)
                {
                    if (rightsToAdd.Any() || rightsToRemove.Any())
                    {
                        UpdateRightsToRoleLogMessage(editorPersonaId, roleId, rightsToAdd, rightsToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>
        /// <param name="roleId">The role being assigned</param>
        /// <param name="rightsToAdd">A list of right ids to add to the role</param>
        /// <param name="rightsToRemove">A list of right ids to remove from the role</param>
        public ListResponse CloneRightsToRole(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError)
            {
                return response;
            }

            try
            {
                if (rightsToAdd != null && rightsToRemove != null)
                {
                    if (rightsToAdd.Count > 0 || rightsToRemove.Count > 0)
                    {
                        LinkRightsToRole(editorPersonaId, roleId, rightsToAdd, rightsToRemove);
                    }
                }
                if (!response.IsError)
                {
                    if (rightsToAdd.Any() || rightsToRemove.Any())
                    {
                        UpdateRightsToRoleLogMessage(editorPersonaId, roleId, rightsToAdd, rightsToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = ex.Message;
            }
            return response;
        }

        public void UpdateRightsToRoleLogMessage(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove)
        {
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            UserDetails impersonatorUserInfo = impersonatorUserDetails(_userClaims.ImpersonatedBy);

            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            var roleName = GetRoleName(roleId, (int)ProductEnum.UnifiedPlatform);
            if (rightsToAdd != null)
            {
                foreach (var right in rightsToAdd)
                {
                    var rightName = GetRightName(right, (int)ProductEnum.UnifiedPlatform);
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_ASSIGN.Replace("RightName", rightName) });
                }
            }
            if (rightsToRemove != null)
            {
                foreach (var right in rightsToRemove)
                {
                    var rightName = GetRightName(right, (int)ProductEnum.UnifiedPlatform);
                    additionalParameters.Add(new AdditionalParameters { Key = roleName, Value = RIGHT_UNASSIGN.Replace("RightName", rightName) });
                }
            }
            var message = "";
            message = impersonatorUserInfo != null
              ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed Rights to {roleName} in Unified Platform."
            : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed rights to {roleName} in Unified Platform.";

            PushToQueue(fromUserLogInfo, message, additionalParameters, 3);
        }

        /// <summary>
        /// Returns all Roles For Party(Org) with associated rights (User Access Groups in UnifiedLogin)
        /// </summary>
        public ListResponse GetRoles(long editorPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get roles from DB for UnifiedLogin product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Getting all GB roles from GB DB - pr.ListRolesForProductsByPartyId with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;

                var productIds = GetProductIdsByOrg();

                var gbAllRoles = _unifiedLoginRepository.ListRolesForProductsByPartyId(partyId, productId, productIds);
                gbAllRoles = gbAllRoles.OrderBy(r => r.Name).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = gbAllRoles.Cast<object>().ToList(),
                    TotalRows = gbAllRoles.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };


            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UnifiedLogin - There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all Roles With Count For Party(Org) with associated rights (User Access Groups in UnifiedLogin)
        /// </summary>
        public ListResponse GetRolesWithCount(long editorPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesWithCount", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesWithCount", $"Error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get roles from DB for UnifiedLogin product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesWithCount", $"Getting all GB roles from GB DB - ListRoleWithRights with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;
                UnifiedLoginRepository umr = new UnifiedLoginRepository();

                //Get Product Id's by org

                var productIds = GetProductIdsByOrg();

                var gbAllRoles = umr.ListRoleWithRights(partyId, productId, productIds);
                //Remove Lumina Right if any from each roles when the aichatuseroptions is "All Users"/"Nobody in the company"
                var settings = _manageUnifiedSettings.GetUnifiedSettingsCached("aichat", _userClaims.OrganizationPartyId);
                var aichatUserOptions = settings.FirstOrDefault(x => x.Name == "aichatuseroptions")?.Value;
                if ((aichatUserOptions == "1" || aichatUserOptions == "3") &&
                    (result.Records != null && result.Records.Count > 0 &&
                     (result.Records[0] is Persona p && p.UserTypeId.HasValue && p.UserTypeId.Value == 402)))
                {
                    gbAllRoles.RemoveAll(r => string.Equals(r.RightNickName, "Lumina", StringComparison.OrdinalIgnoreCase));
                }
                var gbRolesWithCount = GetRolesWithRightsCount(gbAllRoles, partyId, productId, productIds);

                gbRolesWithCount = gbRolesWithCount.OrderBy(r => r.Name).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesWithCount", $"Completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = gbRolesWithCount.Cast<object>().ToList(),
                    TotalRows = gbRolesWithCount.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UnifiedLogin - There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesWithCount", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Returns all Rights For Party(Org)  with associated  roles  (User Access Groups in UnifiedLogin)
        /// </summary>
        public ListResponse GetRights(long editorPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRights", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRights", $"Error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get roles from DB for UnifiedLogin product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRights", $"Getting all GB rights from GB DB - ListRightForProductsByPartyId with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;
                var productIds = GetProductIdsByOrg();
                UnifiedLoginRepository umr = new UnifiedLoginRepository();
                var gbAllRights = umr.ListAllRightsForProductsByPartyId(partyId, productId, productIds);

                // gbAllRights = GetRightsWithoutDefault(gbAllRights);

                gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

                //// check with logged in editors rights
                //List<string> editorRights = _claimDetails.Rights;
                //setEnableDisableByEditorRights(editorRights, ref gbAllRights);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRights", $"Completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = gbAllRights.Cast<object>().ToList(),
                    TotalRows = gbAllRights.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };


            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UnifiedLogin - There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRights", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Returns all Rights With Count For Party(Org)  with associated  roles  (User Access Groups in UnifiedLogin)
        /// </summary>
        public ListResponse GetRightsWithCount(long editorPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsWithCount", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsWithCount", $"Error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get roles from DB for UnifiedLogin product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsWithCount", $"Getting all GB roles from GB DB - ListRoleWithRights with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;            
                var productIds = new List<int>();
                if (_userClaims.OrganizationRealPageGuid == DefaultUserClaim.EmployeeCompanyRealPageId)
                {
                    var products = _productRepository.GetAllProducts();
                    productIds = products.Select(p => p.ProductId).ToList();
                }
                else
                {
                    //Get Product Id's by org
                    productIds = GetProductIdsByOrg();
                }

                UnifiedLoginRepository umr = new UnifiedLoginRepository();

                var gbAllRights = umr.ListRightWithRoles(partyId, productId, productIds);
                //Remove Lumina Right if any from each roles when the aichatuseroptions is "All Users"/"Nobody in the company"
                var settings = _manageUnifiedSettings.GetUnifiedSettingsCached("aichat", _userClaims.OrganizationPartyId);
                var aichatUserOptions = settings.FirstOrDefault(x => x.Name == "aichatuseroptions")?.Value;
                if ((aichatUserOptions == "1" || aichatUserOptions == "3") &&
                    (result.Records != null && result.Records.Count > 0 &&
                     (result.Records[0] is Persona p1 && p1.UserTypeId.HasValue && p1.UserTypeId.Value == 402)))
                {
                    gbAllRights.RemoveAll(r => string.Equals(r.RightNickName, "Lumina", StringComparison.OrdinalIgnoreCase));
                }
                var gbRightsWithCount = GetRightsWithRolesCount(gbAllRights);
                gbRightsWithCount = gbRightsWithCount.OrderBy(r => r.Description).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsWithCount", $"Completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = gbRightsWithCount.Cast<object>().ToList(),
                    TotalRows = gbRightsWithCount.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UnifiedLogin - There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsWithCount", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Returns Rights with selected rights for a roleId(User Access Groups in UnifiedLogin)
        /// </summary>
        public ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get rights from DB for UnifiedLogin product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Getting all GB roles from GB DB - ListRolesForProductByParty with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;

                IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
                var gbAllRights = _unifiedLoginRepository.ListRightsByRole(partyId, productIdList, productId, roleId);
                gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = gbAllRights.Cast<object>().ToList(),
                    TotalRows = gbAllRights.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RightErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        public ListResponse GetListRightbyRole(string productCode,int roleId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetListRightbyRole", $"Beginning of method for user with productCode - {productCode}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(_userClaims.PersonaId, _userClaims.PersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetListRightbyRole", $"GetCompanyEditorAndUserDetails error for user with Persona id - {_userClaims.PersonaId} - {result.ErrorReason}" });
                    return result;
                }

                var productList = _productRepository.GetAllProducts();
                int productId = ProductEnumHelper.GetProductIdByProductCode(productCode, productList);
                var productInternalSettings = GetProductInternalSettingByProductId(productId);

                // Check for sharedProductId setting and update productId if present
                string sharedProductSetting = productInternalSettings.FirstOrDefault(a => a.Name.Equals(SettingConstants.SharedProductSettingName, StringComparison.OrdinalIgnoreCase))?.Value;
                if (sharedProductSetting != null && int.TryParse(sharedProductSetting, out int sharedProductId))
                {
                    productId = sharedProductId;
                }

                var gbAllRights = _unifiedLoginRepository.ListRightsByRole(_userClaims.OrganizationPartyId, new List<int> {productId}, productId, roleId);
                gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetListRightbyRole", $"MapProductAccessGroupsToGB completed for user with Persona id - {_userClaims.PersonaId}" });

                response = new ListResponse()
                {
                    Records = gbAllRights.Cast<object>().ToList(),
                    TotalRows = gbAllRights.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RightErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetListRightbyRole", $"Error for user with editorPersona id - {_userClaims.PersonaId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Returns All Rights with selected rights for a roleId(User Access Groups in UnifiedLogin)
        /// </summary>
        public ListResponse GetAllRightsByRole(long editorPersonaId, long partyId, long roleId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRightsByRole", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRightsByRole", $"Error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get rights from DB for UserManagement product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRightsByRole", $"Getting all GB roles from GB DB - ListAllRightsForProductsByPartyId with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;

                var productIds = GetProductIdsByOrg();
                UnifiedLoginRepository umr = new UnifiedLoginRepository();
                var gbAllRights = umr.ListAllRightsForProductsByPartyId(partyId, productId, productIds);
                //Remove Lumina Right if any from each roles when the aichatuseroptions is "All Users"/"Nobody in the company"
                var settings = _manageUnifiedSettings.GetUnifiedSettingsCached("aichat", _userClaims.OrganizationPartyId);
                var aichatUserOptions = settings.FirstOrDefault(x => x.Name == "aichatuseroptions")?.Value;
                if ((aichatUserOptions == "1" || aichatUserOptions == "3") &&
                    (result.Records != null && result.Records.Count > 0 &&
                     (result.Records[0] is Persona p && p.UserTypeId.HasValue && p.UserTypeId.Value == 402)))
                {
                    gbAllRights.RemoveAll(r => string.Equals(r.Alias, "Lumina", StringComparison.OrdinalIgnoreCase));
                }
                gbAllRights = gbAllRights.OrderBy(r => r.Description).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRightsByRole", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                //// check with logged in editors rights
                //List<string> editorRights = _claimDetails.Rights;
                //setEnableDisableByEditorRights(editorRights, ref gbAllRights);

                if (roleId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRightsByRole", $"MergeRightsWithAllRights calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeRightsWithAllRights(gbAllRights, roleId, partyId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRightsByRole", $"MergeRightsWithAllRights completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = gbAllRights.Cast<object>().ToList(),
                        TotalRows = gbAllRights.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UserManagement - There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetAllRightsByRole", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Returns All Roles with selected roles for a rightId(User Access Groups in UserManagement)
        /// </summary>
        public ListResponse GetRolesByRight(long editorPersonaId, long partyId, long rightId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesByRight", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesByRight", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get rights from DB for UserManagement product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesByRight", $"Getting all GB roles from GB DB - ListRolesForProductsByPartyId with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;

                var productIds = GetProductIdsByOrg();
                UnifiedLoginRepository umr = new UnifiedLoginRepository();
                var gbAllRoles = umr.ListRolesForProductsByPartyId(partyId, productId, productIds);

                gbAllRoles = gbAllRoles.OrderBy(r => r.Name).ToList();

                // Get all rights to check with roles
                var gbAllRights = umr.ListRightWithRoles(partyId, productId, productIds);

                //// check with logged in editors rights
                //List<string> editorRights = _claimDetails.Rights;
                //setEnableDisableByEditorRoles(editorRights, ref gbAllRoles, gbAllRights);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesByRight", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                if (rightId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesByRight", $"MergeRightsWithAllRights calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeRolesWithAllRoles(gbAllRoles, rightId, partyId, productId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesByRight", $"MergeRightsWithAllRights completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = gbAllRoles.Cast<object>().ToList(),
                        TotalRows = gbAllRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UserManagement - There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRolesByRight", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }


        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>
        /// <param name="rightId">The right being assigned</param>
        /// <param name="rolesToAdd">A list of roles ids to add to the role</param>
        /// <param name="rolesToRemove">A list of roles ids to remove from the role</param>
        public ListResponse UpdateRolesByRight(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove)
        {
            ListResponse response = new ListResponse();
            List<string> newRolesAdded = new List<string>();

            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            
            if (response.IsError)
            {
                return response;
            }

            try
            {
                var currentRoles = GetRolesByRight(editorPersonaId, _userClaims.OrganizationPartyId, rightId);
                GetRoleAssignmentChanges(rolesToAdd, currentRoles, out newRolesAdded);

                if (rolesToAdd != null && rolesToRemove != null)
                {
                    LinkRolesToRight(editorPersonaId, rightId, rolesToAdd, rolesToRemove);
                }
                if (!response.IsError)
                {
                    if (rolesToAdd.Any() || rolesToRemove.Any())
                    {
                        UpdateRolesByRightLogMessage(editorPersonaId, rightId, newRolesAdded, rolesToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ErrorReason = ex.Message;
            }
            return response;
        }

        private void GetRoleAssignmentChanges(List<string> roles, ListResponse currentRoles, out List<string> rolesToAdd)
        {
            rolesToAdd = new List<string>();

            // Normalize inputs
            var desired = (roles ?? new List<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var assignedNow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (currentRoles?.Records != null && currentRoles.Records.Count > 0)
            {
                foreach (var pr in currentRoles.Records.OfType<ProductRole>())
                {
                    if (pr.IsAssigned && !string.IsNullOrWhiteSpace(pr.ID))
                    {
                        assignedNow.Add(pr.ID.Trim());
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
        }

        public void UpdateRolesByRightLogMessage(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove)
        {
            var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            UserDetails impersonatorUserInfo = impersonatorUserDetails(_userClaims.ImpersonatedBy);
            var rightName = GetRightName(rightId.ToString(), (int)ProductEnum.UnifiedPlatform);
            if (rolesToAdd != null)
            {
                foreach (var role in rolesToAdd)
                {
                    var roleName = GetRoleName(long.Parse(role), (int)ProductEnum.UnifiedPlatform);
                    additionalParameters.Add(new AdditionalParameters { Key = rightName, Value = ROLE_ASSIGN.Replace("RoleName", roleName) });
                }
            }
            if (rolesToRemove != null)
            {
                foreach (var role in rolesToRemove)
                {
                    var roleName = GetRoleName(long.Parse(role), (int)ProductEnum.UnifiedPlatform);
                    additionalParameters.Add(new AdditionalParameters { Key = rightName, Value = ROLE_UNASSIGN.Replace("RoleName", roleName) });
                }
            }
            var message = "";
            message = impersonatorUserInfo != null
              ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName})  Added/Removed roles to {rightName} in Unified Platform."
            : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName}  Added/Removed roles to {rightName} in Unified Platform.";
            
            PushToQueue(fromUserLogInfo, message, additionalParameters, 3);
        }

        public void PushToQueue(UserActivityLogInfo fromUserLogInfo, String message, List<AdditionalParameters> additionalParameters = null, int productId = 0)
        {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.ROLES_RIGHTS,
                    LogCategoryName = LogActivityCategoryType.RolesRights.ToString(),
                    CorrelationId = _userClaims.CorrelationId.ToString(),
                    BooksMasterOrganizationId = fromUserLogInfo.BooksOrganizationMasterId,
                    OrganizationPartyId = fromUserLogInfo.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = fromUserLogInfo.LoginName,
                    FromUserLoginId = fromUserLogInfo.UserId,
                    FromUserFirstName = fromUserLogInfo.FirstName,
                    FromUserLastName = fromUserLogInfo.LastName,
                    FromUserRealpageId = fromUserLogInfo.RealPageId.ToString(),

                    AdditionalInformation = additionalParameters,
                    ContextId = productId > 0 ? productId.ToString() : null
                });
        }

        /// <summary>
        /// Returns Roles for User (User Access Groups in UserManagement)
        /// </summary>
        public ListResponse GetUserRoles(long editorPersonaId, long userPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get roles from DB for UserManagement product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRoles", $"Getting all GB roles from GB DB - ListRolesForProductByParty with party id - {partyId}" });
                var productId = (int) ProductEnum.UnifiedPlatform;

                var productIdList = _productRepository.GetProductIdsByCompany(partyId);
                var gbAllRoles = _productRepository.ListRolesForProductByParty(partyId, productIdList, productId);

                gbAllRoles = gbAllRoles.OrderBy(r => r.Name).ToList();

                //// Get all rights to check with roles
                //UnifiedLoginRepository umr = new UnifiedLoginRepository();
                //var gbAllRights = umr.ListRightWithRoles(partyId, productId, productIds);

                //// check with logged in editors rights
                //List<string> editorRights = _claimDetails.Rights;
                //setEnableDisableByEditorRoles(editorRights, ref gbAllRoles, gbAllRights);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRoles", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                if (userPersonaId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRoles", $"MergeAccessGroupsWithGreenbook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId, partyId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRoles", $"Completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                }
                else // Called during creating a new User
                {
                    // For new user, set a default role - User Role (Name = BASIC END USER)
                    if (gbAllRoles != null)
                    {
                        gbAllRoles.FirstOrDefault(s => s.DefaultRole == "True").IsAssigned = true;
                    }

                    response = new ListResponse()
                    {
                        Records = gbAllRoles.Cast<object>().ToList(),
                        TotalRows = gbAllRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UserManagement - There was a problem getting the roles.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRoles", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Returns Roles for User (User Access Groups in UserManagement)
        /// </summary>
        public ListResponse GetUserRolesWithRights(long editorPersonaId, long userPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRolesWithRights", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRolesWithRights", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get roles from DB for UserManagement product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRolesWithRights", $"Getting all GB roles from GB DB - ListRolesForProductByParty with party id - {partyId}" });
                int productId = (int) ProductEnum.UnifiedPlatform;

                ProductRepository pr = new ProductRepository();
                UserRoleRightRepository urr = new UserRoleRightRepository();
                IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
                var gbAllRoles = urr.GetPlatFormRoleRights(partyId, productIdList, productId);

                gbAllRoles = gbAllRoles.OrderBy(r => r.Role).ToList();

                //Remove Lumina Right if any from each roles when the aichatuseroptions is "All Users"/"Nobody in the company"
                var settings = _manageUnifiedSettings.GetUnifiedSettingsCached("aichat", _userClaims.OrganizationPartyId);
                var aichatUserOptions = settings.FirstOrDefault(x => x.Name == "aichatuseroptions")?.Value;
                if(aichatUserOptions == "1" || aichatUserOptions == "3" || _userClaims.OrganizationType == "AppPartner")
                {
                    //Iterate and remove lumina right if any from each roles
                    foreach (var role in gbAllRoles)
                    {
                        var luminaRight = role.UserRights.FirstOrDefault(f => f.RightNickName == "Lumina");
                        if (luminaRight != null)
                        {
                            role.UserRights.Remove(luminaRight);
                        }
                    }
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRolesWithRights", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                if (userPersonaId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRolesWithRights", $"MergeAccessGroupsWithGreenbook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });

                    response = SetUserSelectedRole(gbAllRoles, userPersonaId, partyId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRolesWithRights", $"MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                }
                else // Called during creating a new User
                {
                    // For new user, set a default role - User Role (Name = BASIC END USER)
                    if (gbAllRoles != null)
                    {
                        gbAllRoles.FirstOrDefault(s => s.DefaultRole == "True").IsAssigned = true;
                    }

                    response = new ListResponse()
                    {
                        Records = gbAllRoles.Cast<object>().ToList(),
                        TotalRows = gbAllRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserRolesWithRights", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all Companies in Green Book
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>
        /// <param name="partyId">Party Id</param>
        public ListResponse GetGBCompanies(long editorPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGBCompanies", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetGBCompanies", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                // get companies from DB for UnifiedLogin product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGBCompanies", $"Getting all GB companies from GB DB - pr.ListCompanies with party id - {partyId}" });

                UnifiedLoginRepository umr = new UnifiedLoginRepository();
                var gbAllComp = umr.ListCompanies();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetGBCompanies", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                response = new ListResponse()
                {
                    Records = gbAllComp.Cast<object>().ToList(),
                    TotalRows = gbAllComp.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = $"UnifiedLogin - There was a problem getting the companies.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetGBCompanies", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if editor has right to update rights
        /// </summary>
        /// <param name="editorRights"> editors rights</param>
        /// <param name="rightToUpdated"></param>

        private bool checkEditorCanUpdateRights(List<string> editorRights, List<string> rightToUpdated)
        {
            if (editorRights.Count == 0)
            {
                return false;
            }

            var partyId = _userClaims.OrganizationPartyId;
            // Get all rights to check with roles
            IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);

            var allRights = _unifiedLoginRepository.ListRightWithRoles(partyId, (int) ProductEnum.UnifiedPlatform, productIdList);

            bool isEditorHasRight = true;
            foreach (var rt in rightToUpdated)
            {
                RightRoleDetail right = allRights.Find(x => x.RightValueTypeId == int.Parse(rt));
                if (!editorRights.Contains(right.RightNickName))
                {
                    return isEditorHasRight = false;
                }
            }

            return isEditorHasRight;
        }

        /// <summary>
        /// Set if editor has the right 
        /// </summary>
        /// <param name="editorRights"> editors rights</param>
        /// <param name="allRights"> all rights for the org</param>                     
        private void setEnableDisableByEditorRights(List<string> editorRights, ref List<ProductRight> allRights)
        {
            foreach (var rt in allRights)
            {
                if (editorRights.Contains(rt.Alias))
                {
                    rt.isEditorHasRight = true;
                }
            }
        }

        /// <summary>
        ///Check if editor has right to update roles
        /// </summary>
        /// <param name="editorRights">editors roles</param>
        /// <param name="allRoles"> all roles for the org</param>
        /// <param name="allRights"></param>        
        public void setEnableDisableByEditorRoles(List<string> editorRights, ref List<ProductRole> allRoles, List<RightRoleDetail> allRights)
        {
            foreach (var role in allRoles)
            {
                List<RightRoleDetail> rightsForRole = allRights.FindAll(x => x.RoleId == int.Parse(role.ID));

                foreach (var rt in rightsForRole)
                {
                    if (!editorRights.Contains(rt.RightNickName))
                    {
                        role.isEditorHasRight = false; // Even if one right dosent match with Editors then set the flag to false
                        break;
                    }

                    role.isEditorHasRight = true;
                }
            }
        }


        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorRights">editors roles</param>
        /// <param name="allRoles"> all roles for the org</param>
        /// <param name="allRights"></param>        
        public bool checkEditorCanUpdateRoles(List<string> editorRights, List<string> allRoles, List<RightRoleDetail> allRights)
        {
            if (editorRights.Count == 0)
            {
                return false;
            }

            bool isEditorHasRight = true;
            foreach (var role in allRoles)
            {
                List<RightRoleDetail> rightsForRole = allRights.FindAll(x => x.RoleId == int.Parse(role));

                foreach (var rt in rightsForRole)
                {
                    if (!editorRights.Contains(rt.RightNickName))
                    {
                        return isEditorHasRight = false; // Even if one right dosent match with Editors then set the flag to false                       
                    }
                }
            }

            return isEditorHasRight;
        }

        /// <summary>
        /// Check if editor has right  Delete Role
        /// </summary>
        /// <param name="editorRights">editors roles</param>
        /// <param name="allRoles"> all roles for the org</param>
        /// <param name="allRights"></param>        
        public bool checkEditorCanDeleteRoles(List<string> editorRights, List<string> allRoles, List<RightRoleDetail> allRights)
        {
            if (editorRights.Count == 0)
            {
                return false;
            }

            bool isEditorHasRight = true;
            foreach (var role in allRoles)
            {
                List<RightRoleDetail> rightsForRole = allRights.FindAll(x => x.RoleId == int.Parse(role));

                foreach (var rt in rightsForRole)
                {
                    if (!editorRights.Contains(rt.RightNickName))
                    {
                        return isEditorHasRight = false; // Even if one right dosent match with Editors then set the flag to false                       
                    }
                }
            }

            return isEditorHasRight;
        }


        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>
        /// <param name="roleId">The right being assigned</param>
        /// <param name="addRights">A list of right ids to add</param>
        /// <param name="delRights">A list of right ids to delete</param>        
        private void LinkRightsToRole(long editorPersonaId, long roleId, List<string> addRights, List<string> delRights)
        {
            UnifiedLoginRepository ocr = new UnifiedLoginRepository();
            List<RightRoleAddRem> rightsAddRem = new List<RightRoleAddRem>();
            foreach (var item in addRights)
            {
                RightRoleAddRem rightAdd = new RightRoleAddRem();
                rightAdd.RoleId = roleId;
                rightAdd.RightValueTypeID = long.Parse(item);
                rightAdd.IsDeleted = 0;
                rightsAddRem.Add(rightAdd);
            }

            foreach (var item in delRights)
            {
                RightRoleAddRem rightDel = new RightRoleAddRem();
                rightDel.RoleId = roleId;
                rightDel.RightValueTypeID = long.Parse(item);
                rightDel.IsDeleted = 1;
                rightsAddRem.Add(rightDel);
            }

            var result = ocr.LinkRightsToRole(rightsAddRem, _userClaims.UserId);
        }


        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the GreenBook user making the change.</param>
        /// <param name="rightId">The right being assigned</param>
        /// <param name="addRoles">A list of role ids to add</param>
        /// <param name="delRoles">A list of role ids to delete</param>        
        private void LinkRolesToRight(long editorPersonaId, long rightId, List<string> addRoles, List<string> delRoles)
        {
            UnifiedLoginRepository ocr = new UnifiedLoginRepository();
            List<RightRoleAddRem> rightsAddRem = new List<RightRoleAddRem>();
            foreach (var item in addRoles)
            {
                RightRoleAddRem roleAdd = new RightRoleAddRem();
                roleAdd.RoleId = long.Parse(item);
                roleAdd.RightValueTypeID = rightId;
                roleAdd.IsDeleted = 0;
                rightsAddRem.Add(roleAdd);
            }

            foreach (var item in delRoles)
            {
                RightRoleAddRem roleDel = new RightRoleAddRem();
                roleDel.RoleId = long.Parse(item);
                roleDel.RightValueTypeID = rightId;
                roleDel.IsDeleted = 1;
                rightsAddRem.Add(roleDel);
            }

            var result = ocr.LinkRightsToRole(rightsAddRem, _userClaims.UserId);
        }


        private List<ProductRight> GetRightsWithoutDefault(List<ProductRight> allRights)
        {

            if (allRights != null &&
                allRights.Count > 0)
            {
                allRights.RemoveAll(p => p.Description.ToUpper().Trim() == "DEFAULT_DASHBOARD_ADMIN");
                allRights.RemoveAll(p => p.Description.ToUpper().Trim() == "DEFAULT_DASHBOARD_USERS");
                allRights.RemoveAll(p => p.Description.ToUpper().Trim() == "DEFAULT_SIDEMENU_USERS");
                allRights.RemoveAll(p => p.Description.ToUpper().Trim() == "DEFAULT_SIDEMENU_ADMIN");
            }

            return allRights;
        }

        private List<ProductRole> GetRolesWithRightsCount(List<RightRoleDetail> allRolesandRights, long partyId, long ulProductId, List<int> productIdList)
        {
            var result = new List<ProductRole>();


            if (allRolesandRights != null &&
                allRolesandRights.Count > 0)
            {
                UnifiedLoginRepository umr = new UnifiedLoginRepository();
                var roles = umr.ListRolesForProductsByPartyId(partyId, ulProductId, productIdList);

                foreach (var role in roles)
                {
                    List<RightRoleDetail> list = new List<RightRoleDetail>();

                    bool roleexists = false;

                    foreach (var item in allRolesandRights)
                    {
                        if (item.RoleId == int.Parse(role.ID))
                        {
                            roleexists = true;
                            if (list.Count == 0)
                            {
                                list.Add(item);
                            }
                            else
                            {
                                if (list.Exists(e => e.RightName == item.RightName) == false)
                                {
                                    list.Add(item);
                                }
                            }
                        }
                    }



                    if (list != null && list.Count > 0)
                    {
                        int rights = list.Count;
                        if (list.Count == 1 && list[0].RightId == 0)
                        {
                            rights = 0;
                        }

                        list[0].RightsAssigned = rights.ToString();
                        ProductRole pr = new ProductRole {ID = list[0].RoleId.ToString(), IsAssigned = list[0].IsAssigned, Roletype = list[0].RoleType, Name = list[0].RoleName, RightsAssigned = list[0].RightsAssigned, DefaultRole = list[0].IsDefaultRole == true ? "User Default" : ""};
                        result.Add(pr);
                    }
                    else
                    {
                        if (roleexists == false)
                        {
                            roleexists = true;
                            //RightRoleDetail roleWithNoRights = new RightRoleDetail();
                            //roleWithNoRights.RoleId = int.Parse(role.ID);
                            //roleWithNoRights.RoleName = role.Name;
                            //roleWithNoRights.RightsAssigned = "0";
                            //roleWithNoRights.RoleType = role.Roletype;
                            //roleWithNoRights.IsAssigned = false;
                            //list.Add(roleWithNoRights);
                            ProductRole pr = new ProductRole {ID = role.ID, IsAssigned = false, Roletype = role.Roletype, Name = role.Name, RightsAssigned = "0", DefaultRole = bool.Parse(role.DefaultRole) == true ? "User Default" : ""};
                            result.Add(pr);
                        }

                    }
                }
            }

            return result;
        }

        private List<ProductRight> GetRightsWithRolesCount(List<RightRoleDetail> allRights)
        {
            var result = new List<ProductRight>();


            if (allRights != null &&
                allRights.Count > 0)
            {

                IEnumerable<int> rightValueTypeIds = allRights.Select(x => x.RightValueTypeId).Distinct();
                foreach (var rightId in rightValueTypeIds)
                {
                    List<RightRoleDetail> list = new List<RightRoleDetail>();

                    //list = allRights.FindAll(a => a.ID == int.Parse(rightId));

                    foreach (var item in allRights)
                    {
                        if (item.RightValueTypeId == rightId)
                        {
                            if (list.Count == 0)
                            {
                                list.Add(item);
                            }
                            else
                            {
                                if (list.Exists(e => e.RoleName == item.RoleName) == false)
                                {
                                    list.Add(item);
                                }
                            }
                        }
                    }

                    if (list != null && list.Count > 0)
                    {
                        int roles = list.Count;
                        list[0].RolesAssigned = roles.ToString();
                        ProductRight pr = new ProductRight {ID = list[0].RightValueTypeId, Assigned = list[0].IsAssigned, Description = list[0].RightName, RolesAssigned = int.Parse(list[0].RolesAssigned), RightDescription = list[0].RightDescription };
                        result.Add(pr);
                    }
                }
            }

            return result;
        }

        private List<Role> GetAssignedRoleForPersona(long userPersonaId, long organizationPartyId)
        {
            int productId = (int) ProductEnum.UnifiedPlatform;
            var cacheKey = $"sp_ListRolesForProductsByPersonaId_{productId}_{userPersonaId}_{organizationPartyId}";
            MemoryCache.Default.Remove(cacheKey);
            List<UL.Role> propRole = _userRoleRightRepository.ListRoleByPersona(productId, userPersonaId, organizationPartyId);
            return propRole;
        }

        private List<CategoryType> GetCategoryType()
        {
            UnifiedLoginRepository ocr = new UnifiedLoginRepository();
            List<CategoryType> categoryList = ocr.GetCategoryType();
            return categoryList;
        }

        private ListResponse MergeSelRolesWithGreenbook(IList<ProductRole> allRoles, long userPersonaId, long partyId)
        {
            // get roles from DB for UnifiedLogin product
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeSelRolesWithGreenbook", $"Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}" });
            var roleList = GetAssignedRoleForPersona(userPersonaId, partyId);

            if (roleList == null)
                return new ListResponse()
                {
                    Records = allRoles.Cast<object>().ToList(),
                    TotalRows = allRoles.Count(),
                    RowsPerPage = 9999,
                    ErrorReason = string.Empty,
                    TotalPages = 1
                };

            // if a user record exists
            foreach (var role in roleList)
            {
                if (allRoles.Any(a => a.ID == role.RoleID.ToString()))
                {
                    ProductRole selrole = (from a in allRoles
                                           where a.ID == role.RoleID.ToString()
                                           select a).FirstOrDefault();
                    if (selrole != null)
                    {
                        selrole.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private ListResponse SetUserSelectedRole(IList<UnifiedLoginRoleRights> allRoles, long userPersonaId, long partyId)
        {
            // get roles from DB for UnifiedLogin product
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "SetUserSelectedRole", $"Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}" });
            List<Role> roleList = GetAssignedRoleForPersona(userPersonaId, partyId);

            // if a user record exists

            foreach (var role in roleList)
            {
                if (allRoles.Any(a => a.RoleId.ToString() == role.RoleID.ToString()))
                {
                    UnifiedLoginRoleRights selrole = (from a in allRoles
                                                      where a.RoleId.ToString() == role.RoleID.ToString()
                                                      select a).FirstOrDefault();
                    if (selrole != null)
                    {
                        selrole.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private ListResponse GetResidentPortalUserRoles(IList<ProductRole> allRoles, long userPersonaId, long editorPersonaId)
        {
            var personaId = userPersonaId == 0 ? editorPersonaId : userPersonaId;
            // get roles from DB for UnifiedLogin product
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetResidentPortalUserRoles", $"Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {personaId}" });
            List<Role> roleList = GetAssignedRoleForPersona(personaId);
            // if a user record exists

            foreach (var role in roleList)
            {
                if (allRoles.Any(a => a.ID == role.RoleID.ToString()))
                {
                    ProductRole selrole = (from a in allRoles
                                           where a.ID == role.RoleID.ToString()
                                           select a).FirstOrDefault();
                    if (selrole != null)
                    {
                        selrole.IsAssigned = true;
                    }
                }
            }

            var itemsToRemove = allRoles.Where(r => (r.Name != "Basic End User" && r.Roletype == "Default") || (!r.IsAssigned && r.Roletype == "Custom")).ToList();

            foreach (var item in itemsToRemove)
            {
                allRoles.Remove(item);
            }

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private ListResponse MergeRightsWithAllRights(List<ProductRight> allRights, long roleId, long partyId)
        {
            // get assigned rights to role from DB for UnifiedLogin product
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRightsWithAllRights", $"Getting assigned user roles from GB DB - GetAssignedRightsForRole with role id - {roleId}" });
            List<ProductRight> rightList = GetAssignedRightsForRole(partyId, roleId);

            // if a user record exists
            foreach (var right in rightList)
            {
                if (allRights.Any(a => a.ID == right.ID))
                {
                    ProductRight selright = (from a in allRights
                                             where a.ID == right.ID
                                             select a).FirstOrDefault();
                    if (selright != null)
                    {
                        selright.Assigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allRights.Cast<object>().ToList(),
                TotalRows = allRights.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private ListResponse MergeRolesWithAllRoles(IList<ProductRole> allRoles, long rightValId, long partyId, int productId)
        {

            // get assigned rights to role from DB for UnifiedLogin product
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRolesWithAllRoles", $"Getting assigned user roles from GB DB - GetAssignedRightsForRole with right id - {rightValId}" });

            var productIds = GetProductIdsByOrg();
            UnifiedLoginRepository umr = new UnifiedLoginRepository();
            var RoleRightDet = umr.ListRoleRightDetForProductsByPartyId(partyId, productId, productIds);

            var roleList = RoleRightDet.FindAll(r => r.RightValueTypeId == rightValId);
            // if a user record exists

            foreach (var role in roleList)
            {
                if (allRoles.Any(a => int.Parse(a.ID) == role.RoleId))
                {
                    ProductRole selrole = (from a in allRoles
                                           where int.Parse(a.ID) == role.RoleId
                                           select a).FirstOrDefault();
                    if (selrole != null)
                    {
                        selrole.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allRoles.Cast<object>().ToList(),
                TotalRows = allRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private List<int> GetProductIdsByOrg()
        {
            return (List<int>) _productRepository.GetProductIdsByCompany(_userClaims.OrganizationRealPageGuid);
        }

        private List<ProductRole> GetAssignedRolesForRight(long partyId, long rightId)
        {
            int productId = (int) ProductEnum.UnifiedPlatform;
            UnifiedLoginRepository umr = new UnifiedLoginRepository();
            List<ProductRole> roles = umr.ListRolesByRight(partyId, productId, rightId);
            return roles;
        }

        private List<ProductRight> GetAssignedRightsForRole(long partyId, long roleId)
        {
            int productId = (int) ProductEnum.UnifiedPlatform;
            var productIds = GetProductIdsByOrg();
            IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
            List<ProductRight> rights = _unifiedLoginRepository.ListRightsByRole(partyId, productIdList, productId, roleId);
            return rights;
        }

        private List<Property> GetAssignedPropertyForPersona(long userPersonaId)
        {
            int productId = (int) ProductEnum.UnifiedPlatform;
            UnifiedLoginRepository ocr = new UnifiedLoginRepository();
            List<Property> prop = ocr.ListPropByPersona(userPersonaId, productId);
            return prop;
        }

        /// <summary>
        /// Used to merge product property data with Unified Login property data for the user
        /// </summary>
        /// <param name="blueBookPropertyList">The list of properties from BlueBook</param>
        /// <param name="userPersonaId">The user id to filter on</param>
        /// <param name="assignedOnly">Only return assigned records</param>
        /// <returns></returns>
        private ListResponse MergeProductPropertiesWithGreenbook(IList<ProductProperty> blueBookPropertyList, long userPersonaId, bool assignedOnly)
        {
            // merge the given user details with the list
            List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, (int)ProductEnum.UnifiedPlatform).ToList();
            var propertyOption = new Dictionary<string, bool>();
            propertyOption.Add("allProperties", false); // Single Property

            foreach (var property in propertyList)
            {
                if (property.ID.ToString() == "-1")
                {
                    // PMC level (all properties)
                    propertyOption["allProperties"] = true;
                }
                else
                {
                    if (blueBookPropertyList.Any(a => a.ID == property.ID.ToString()))
                    {
                        ProductProperty pp = (from a in blueBookPropertyList
                                              where a.ID == property.ID.ToString()
                                              select a).FirstOrDefault();
                        if (pp != null)
                        {
                            pp.IsAssigned = true;
                        }
                    }
                }
            }

            if (assignedOnly)
            {
                blueBookPropertyList = blueBookPropertyList.Where(a => a.IsAssigned == true).ToList();
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

        /// <summary>
        /// Used to get the list of UPFM property instances for the given personaid
        /// </summary>
        /// <param name="blueBookUPFMPropertyList"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="assignedOnly"></param>
        /// <returns>A list of product properties</returns>
        private ListResponse MergeUPFMBooksPropertiesWithProductProperty(IList<UPFMPropertyInstance> blueBookUPFMPropertyList, long userPersonaId, bool assignedOnly)
        {
            // merge the given user details with the list
            var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, ProductEnum.UnifiedPlatform);

            var propertyOption = new Dictionary<string, bool>();

            propertyOption.Add("allProperties", userPropertyIdList.Any(pl => pl == -1)); // Single Property
            List<ProductProperty> productPropertyList = new List<ProductProperty>();

            foreach (UPFMPropertyInstance upfmPropertyInstance in blueBookUPFMPropertyList)
            {
                var pp = ConvertUPFMPropertyInstanceToProductProperty(upfmPropertyInstance, false);

                if (userPropertyIdList.Any(propertyId => propertyId == upfmPropertyInstance.PropertyInstanceId))
                {
                    pp.IsAssigned = true;
                }

                productPropertyList.Add(pp);
            }

            if (assignedOnly)
            {
                productPropertyList = productPropertyList.Where(a => a.IsAssigned == true).ToList();
            }

            return new ListResponse()
            {
                Records = productPropertyList.Cast<object>().ToList(),
                TotalRows = productPropertyList.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = propertyOption
            };
        }

        /// <summary>
        /// Used to convert a UPFM property instance to a Product Property 
        /// </summary>
        /// <param name="upfmPropertyInstance"></param>
        /// <param name="isAssigned"></param>
        /// <returns></returns>
        private static ProductProperty ConvertUPFMPropertyInstanceToProductProperty(UPFMPropertyInstance upfmPropertyInstance, bool isAssigned)
        {
            ProductProperty pp = new ProductProperty()
            {
                ID = upfmPropertyInstance.InstanceId.ToString().ToLower(),
                Name = upfmPropertyInstance.Name,
                Street1 = upfmPropertyInstance.Address,
                City = upfmPropertyInstance.City,
                State = upfmPropertyInstance.State,
                Zip = upfmPropertyInstance.PostalCode,
                IsAssigned = isAssigned,
                InstanceId = upfmPropertyInstance.CustomerPropertyId,
                Latitude = upfmPropertyInstance.Latitude,
                Longitude = upfmPropertyInstance.Longitude,
                Alias = null
            };
            return pp;
        }

        #endregion
    }

    #endregion
}
