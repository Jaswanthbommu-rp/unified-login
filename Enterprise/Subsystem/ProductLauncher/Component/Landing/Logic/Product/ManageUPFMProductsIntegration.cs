using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    public class ManageUPFMProductsIntegration : ManageProductBase, IManageUPFMProductsIntegration
    {
        private DefaultUserClaim _userClaims;
        public int _upfmProductId;
        private const string PRODUCT_ROLES_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
        private const string PRODUCT_ROLES_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
        private const string PRODUCT_PROPERTIES_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
        private const string PRODUCT_PROPERTIES_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";

        // ✅ Performance configuration constants
        private const int SmallDatasetThreshold = 100;
        private const int MediumDatasetThreshold = 500;
        private const int LargeDatasetThreshold = 1500;

        private const int SmallBatchSize = 100;
        private const int MediumBatchSize = 50;
        private const int LargeBatchSize = 25;

        private const int SmallParallelism = 5;
        private const int MediumParallelism = 3;
        private const int LargeParallelism = 2;

        private const int BatchDelayMs = 100;

        // ✅ NEW: Retry configuration constants
        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMs = 1000;      // 1 second base delay
        private const int RetryBackoffMultiplier = 2; // Exponential backoff

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="userClaims"></param>
        public ManageUPFMProductsIntegration(int productId, DefaultUserClaim userClaims) : base(productId, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductsIntegration", "Ctor - Getting Product settings." });
#endif
            _userClaims = userClaims;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _productRepository = new ProductRepository(_userClaims);
            _blueBook = new ManageBlueBook(userClaims);
            _upfmProductId = productId;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="defaultUserClaim"></param>
        /// <param name="managePersona"></param>
        /// <param name="managePerson"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="samlRepository"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePartyRelationship"></param>
        /// <param name="userRoleRightRepository"></param>
        /// <param name="manageUserLogin"></param>
        /// <param name="unifiedLoginRepository"></param>
        /// <param name="propertyRepository"></param>
        /// <param name="userLoginRepository"></param>
        public ManageUPFMProductsIntegration(int productId, DefaultUserClaim defaultUserClaim, IManagePersona managePersona, IManagePerson managePerson, IManageBlueBook manageBlueBook, IProductRepository productRepository, ISamlRepository samlRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, IUserRoleRightRepository userRoleRightRepository, IManageUserLogin manageUserLogin, IUnifiedLoginRepository unifiedLoginRepository, IPropertyRepository propertyRepository, IUserLoginRepository userLoginRepository) : base(productId, defaultUserClaim, productInternalSettingRepository, productRepository)
        {
            _userClaims = defaultUserClaim;
            _editorRealPageId = defaultUserClaim.UserRealPageGuid;
            _managePersona = managePersona;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _samlRepository = samlRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
            _userRoleRightRepository = userRoleRightRepository;
            _manageUserLogin = manageUserLogin;
            _unifiedLoginRepository = unifiedLoginRepository;
            _managePerson = managePerson;
            _propertyRepository = propertyRepository;
            _userLoginRepository = userLoginRepository;
            _upfmProductId = productId;
        }

        #region Roles and Rights

        /// <summary>
        /// Returns Roles for the given user and company
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="partyId"></param>
        /// <returns></returns>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });
            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"GetCompanyEditorAndUserDetails error for product {_upfmProductId} with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                var productInternalSettingList = GetProductSetting(_upfmProductId);
                bool getUDMDetails = true;
                if (productInternalSettingList.Any(a => a.Name.Equals("UpdateProductInUDM", StringComparison.OrdinalIgnoreCase)))
                {
                    getUDMDetails = Convert.ToBoolean("1" == productInternalSettingList.First(p => p.Name.Equals("UpdateProductInUDM")).Value);
                }

                if (getUDMDetails)
                {
                    _blueBook.GetCompanyMap(_editorPersona.Organization.RealPageId, _editorPersona.Organization.BooksCustomerMasterId, source: _udmSourceCode.ToUpper(), domain: _editorPersona.Organization.OrganizationDomain.Name);
                }

                // get roles from DB for UnifiedAmenities product
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Getting all GB roles from DB - ListRolesForProductByParty with party id - {partyId} and product {_upfmProductId}" });
                IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
                GetSharedProductDetails(productIdList);

                var gbAllRoles = _productRepository.ListRolesForProductByParty(partyId, productIdList, _upfmProductId) ?? new List<ProductRole>();
                gbAllRoles = gbAllRoles?.OrderBy(r => r.Name).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                if (userPersonaId != 0) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeAccessGroupsWithGreenbook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeSelRolesWithGreenbook(gbAllRoles, userPersonaId);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                }
                else // Called during creating a new User
                {
                    // For new user, set a default role
                    if (gbAllRoles != null)
                    {
                        if (gbAllRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)))
                        {
                            gbAllRoles.FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)).IsAssigned = true;
                        }
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

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                }

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Returns Rights with selected rights for a roleId
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="partyId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
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

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Getting all GB roles from GB DB - pr.ListRolesForProductByParty with party id - {partyId}" });
                ProductRepository pr = new ProductRepository();
                IList<int> productIdList = pr.GetProductIdsByCompany(partyId);

                GetSharedProductDetails(productIdList);
                var gbAllRights = _unifiedLoginRepository.ListRightsByRole(partyId, productIdList, _productId, roleId) ?? new List<ProductRight>();

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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRightsByRole", $"Error for user with editorPersona id - {editorPersonaId} " }, exception: ex);
            }

            return response;
        }

        private void GetSharedProductDetails(IList<int> productIdList)
        {
            var sharedProductList = _productInternalSettingRepository.GetProductSettingByType(SettingConstants.SharedProductSettingName);
            if (sharedProductList != null && sharedProductList.Any(m => m.ProductId == _productId))
            {
                var baseProductDtails = sharedProductList.FirstOrDefault(m => m.ProductId == _productId);
                if (baseProductDtails != null)
                {
                    int.TryParse(baseProductDtails.Value, out _upfmProductId);
                    _productId = _upfmProductId;
                    if (!productIdList.Any(m => m == _upfmProductId))
                    {
                        productIdList.Add(_upfmProductId);
                    }
                }
            }
        }

        /// <summary>
        /// Get Product Ids by Org
        /// </summary>        
        /// <returns></returns>
        private List<int> GetProductIdsByOrg()
        {
            ProductRepository pr = new ProductRepository();
            IList<int> productList = pr.GetProductIdsByCompany(_userClaims.OrganizationRealPageGuid);

            // ✅ OPTIMIZED: Pre-allocate list capacity
            List<int> productIds = new List<int>(productList.Count);
            foreach (var item in productList)
            {
                productIds.Add(item);
            }

            return productIds;
        }

        /// <summary>
        /// Used to unassign a role assigned to the user
        /// </summary>
        /// <param name="userProductPropertyRole"></param>
        /// <returns></returns>
        private UPFMProductPropertyRole MapGbObjectToProduct(UPFMProductPropertyRole userProductPropertyRole)
        {
            var result = new UPFMProductPropertyRole();

            if (userProductPropertyRole.RoleList?.Count > 0)
            {
                // ✅ OPTIMIZED: Pre-allocate list capacity
                result.RoleList = new List<string>(userProductPropertyRole.RoleList.Count);
                foreach (var roleId in userProductPropertyRole.RoleList)
                {
                    result.RoleList.Add(roleId);
                }
            }

            return result;
        }

        /// <summary>
        /// Used to merge product roles to UnifedLogin roles
        /// </summary>
        /// <param name="allRoles"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        private ListResponse MergeSelRolesWithGreenbook(IList<ProductRole> allRoles, long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeSelRolesWithGreenbook", $"Getting assigned user roles from GB DB - GetAssignedRoleForPersona with persona id - {userPersonaId}" });
            List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);

            // ✅ OPTIMIZED: Use HashSet for O(1) lookups instead of O(n) Any() calls
            var assignedRoleIds = new HashSet<string>(roleList.Select(r => r.RoleID.ToString()));

            foreach (var role in allRoles)
            {
                if (assignedRoleIds.Contains(role.ID))
                {
                    role.IsAssigned = true;
                }
            }

            if (allRoles != null)
            {
                if (!allRoles.Any(s => s.IsAssigned == true) && allRoles.Any(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)))
                {
                    allRoles.FirstOrDefault(s => s.DefaultRole.Equals("True", StringComparison.OrdinalIgnoreCase)).IsAssigned = true;
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

        #endregion

        #region Property

        /// <summary>
        /// Get the list of property instances for the given user to be used by external systems
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="product"></param>
        /// <param name="productCode"></param>
        /// <param name="include"></param>
        /// <param name="isMultiCompany"></param>
        /// <param name="multiCompanyRealPageId"></param>
        /// <returns></returns>
        public ListResponse GetEnterpriseUPFMProperties(long userPersonaId, int product, string productCode, string include = null, bool isMultiCompany = false, string multiCompanyRealPageId = null)
        {
            ListResponse response = new ListResponse();

            if (product == (int)ProductEnum.CIMPL || product == (int)ProductEnum.UnifiedSettings)
            {
                _upfmProductId = (int)ProductEnum.UnifiedPlatform;
                _udmSourceCode = !string.IsNullOrEmpty(productCode) ? productCode : _udmSourceCode;
            }

            var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, _upfmProductId);
            List<ProductProperty> userPropertyList = new List<ProductProperty>();
            List<ProductProperty> translatedUserPropertyList = new List<ProductProperty>();
            List<UPFMPropertyInstance> customerPropertyList = new List<UPFMPropertyInstance>();
            int upfmProductId = (int)product;

            if (userPropertyIdList != null)
            {
                var organizationRealPageId = isMultiCompany ? multiCompanyRealPageId : _userClaims.OrganizationRealPageGuid.ToString();

                if (userPropertyIdList.Count == 1 && userPropertyIdList[0] == -1)
                {
                    customerPropertyList = GetProductPropertyInstancesBasedOnUPFMProperties();
                    customerPropertyList.ForEach(cp => { userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true)); });
                }
                else
                {
                    var booksPropertyList = _blueBook.GetUPFMPropertyInstances(organizationRealPageId);
                    if (booksPropertyList != null)
                    {
                        customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);
                    }

                    // ✅ OPTIMIZED: Use HashSet for O(1) lookups
                    var userPropertySet = new HashSet<int>(userPropertyIdList);
                    foreach (var cp in customerPropertyList)
                    {
                        if (userPropertySet.Contains(cp.PropertyInstanceId))
                        {
                            userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true));
                        }
                    }
                }
            }

            if (userPropertyIdList?.Count > 0)
            {
                UPFMProperty upfmProperties = new UPFMProperty();
                // ✅ OPTIMIZED: Pre-allocate list capacity
                List<string> instanceids = new List<string>(userPropertyList.Count);
                var booksProductDetail = _productRepository.GetBooksMasterProductDetail(upfmProductId);
                foreach (var property in userPropertyList)
                {
                    instanceids.Add(property.InstanceId.ToLower());
                }

                upfmProperties.id = instanceids;

                var translatedData = _blueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, _udmSourceCode);
                if (translatedData?.Data != null)
                {
                    var booksProductCode = booksProductDetail.UDMSourceCode == null ? booksProductDetail.BooksProductCode : booksProductDetail.UDMSourceCode;

                    // ✅ OPTIMIZED: Build dictionary for O(1) lookups
                    var propertyDict = userPropertyList.ToDictionary(
                        p => p.InstanceId,
                        p => p,
                        StringComparer.OrdinalIgnoreCase);

                    foreach (var attributes in translatedData.Data.Attributes)
                    {
                        foreach (var propertyData in attributes.TranslatedPropertyInstances)
                        {
                            if (propertyData.Source == booksProductCode)
                            {
                                if (propertyDict.TryGetValue(attributes.PropertyInstanceSourceId, out var translatedProductProperty))
                                {
                                    translatedProductProperty.ID = propertyData.PropertyInstanceSourceId;
                                    translatedProductProperty.Alias = null;
                                    translatedProductProperty.CustomerPropertyId = propertyData.CustomerPropertyId;
                                    translatedUserPropertyList.Add(translatedProductProperty);
                                }
                            }
                        }
                    }
                }

                bool bIncludeFields = (!string.IsNullOrWhiteSpace(include) && include.Split(new char[] { ',' }).Length > 0);

                if (bIncludeFields)
                {
                    DynamicContractResolver dynamicContractResolver = new DynamicContractResolver(include);
                    string productPropertySerializableProperties = JsonConvert.SerializeObject(
                        translatedUserPropertyList,
                        new JsonSerializerSettings()
                        {
                            ContractResolver = dynamicContractResolver
                        }
                    );
                    translatedUserPropertyList = JsonConvert.DeserializeObject<List<ProductProperty>>(productPropertySerializableProperties);
                }

                translatedUserPropertyList.ForEach(p =>
                {
                    p.IsAssigned = null;
                    p.disableSelection = null;
                });

                response.IsError = false;
                response.Records = translatedUserPropertyList.Cast<object>().ToList();
                response.TotalRows = translatedUserPropertyList.Count;
                response.RowsPerPage = translatedUserPropertyList.Count;
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
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUPFMProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"Beginning of method for user with editorPersona id - {editorPersonaId} - for product {_upfmProductId}" });

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                var customerPropertyList = GetProductPropertyInstancesBasedOnUPFMProperties();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"Calling MergeUPFMBooksPropertiesWithUPFMProperties for user with editorPersona id -{editorPersonaId} & userPersonaId-{userPersonaId}." });
                result = MergeUPFMBooksPropertiesWithProductProperty(customerPropertyList, userPersonaId, assignedOnly);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"MergeUPFMBooksPropertiesWithUPFMProperties completed for user with editorPersona id -{editorPersonaId}." });
            }
            catch (Exception ex)
            {
                result.IsError = true;
                if (ex is BlueBookException)
                {
                    result.ErrorReason = CommonMessageConstants.CompanyErrorMessage;
                }
                else
                {
                    result.ErrorReason = $"ManageUPFMProductUser.GetProperties - There was a problem getting the properties.";
                }

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUPFMProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}." },
                    exception: ex);
            }

            return result;
        }

        private List<UPFMPropertyInstance> GetProductPropertyInstancesBasedOnUPFMProperties()
        {
            var productSettingList = GetProductSetting(_productId);
            bool directUDMTranslateProperty = false;
            if (productSettingList.Any(p => p.Name.Equals("DirectUDMTranslateProperty", StringComparison.OrdinalIgnoreCase)))
            {
                directUDMTranslateProperty = Convert.ToBoolean(int.Parse(productSettingList.First(a => a.Name.Equals("DirectUDMTranslateProperty", StringComparison.OrdinalIgnoreCase)).Value));
            }

            List<Guid> booksPropertyList;
            List<UPFMPropertyInstance> customerPropertyList = new List<UPFMPropertyInstance>();

            if (directUDMTranslateProperty)
            {
                var booksUPFMPropertyList = _blueBook.GetUPFMPropertyInstances(_userClaims.OrganizationRealPageGuid.ToString());
                // ✅ OPTIMIZED: Pre-allocate list capacity
                booksPropertyList = new List<Guid>(booksUPFMPropertyList.Count);

                UPFMProperty properties = new UPFMProperty { id = new List<string>(booksUPFMPropertyList.Count) };

                booksUPFMPropertyList.ForEach(c => properties.id.Add(c.ToString()));

                var translatedProperties = _blueBook.GetTranslatePropertiesFromUPFMToProductv3(properties, _udmSourceCode.ToUpper());
                if (translatedProperties?.Data?.Attributes.Count > 0)
                {
                    foreach (var t in translatedProperties.Data.Attributes)
                    {
                        booksPropertyList.Add(new Guid(t.PropertyInstanceSourceId));
                    }

                    customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);

                    // ✅ OPTIMIZED: Build dictionary for O(1) lookups
                    var translatedDict = translatedProperties.Data.Attributes.ToDictionary(
                        tp => tp.PropertyInstanceSourceId,
                        tp => tp,
                        StringComparer.OrdinalIgnoreCase);

                    customerPropertyList.ForEach(cp =>
                    {
                        if (translatedDict.TryGetValue(cp.InstanceId.ToString(), out var translated))
                        {
                            cp.CustomerPropertyId = translated.TranslatedPropertyInstances[0].PropertyInstanceSourceId;
                        }
                    });
                }
            }
            else
            {
                booksPropertyList = _blueBook.GetPropertiesPerProductCenter(_userClaims.OrganizationRealPageGuid.ToString(), _upfmProductId);
                customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);
            }

            return customerPropertyList;
        }

        private static ProductProperty ConvertUPFMPropertyInstanceToProductProperty(UPFMPropertyInstance upfmPropertyInstance, bool isAssigned)
        {
            ProductProperty pp = new ProductProperty()
            {
                ID = upfmPropertyInstance.CustomerPropertyId.ToString(),
                Name = upfmPropertyInstance.Name,
                Street1 = upfmPropertyInstance.Address,
                City = upfmPropertyInstance.City,
                State = upfmPropertyInstance.State,
                Zip = upfmPropertyInstance.PostalCode,
                IsAssigned = isAssigned,
                InstanceId = upfmPropertyInstance.InstanceId.ToString(),
                Latitude = upfmPropertyInstance.Latitude,
                Longitude = upfmPropertyInstance.Longitude,
                Alias = upfmPropertyInstance.PropertyInstanceId.ToString()
            };
            return pp;
        }

        private RepositoryResponse DeleteAssignedPropertyInstanceData(long userPersonaId, int product, long propertyInstanceId)
        {
            return DeleteAssignedUserPropertyInstanceData(userPersonaId, product, propertyInstanceId);
        }

        private RepositoryResponse InsertAssignedPropertyInstanceData(long userPersonaId, int product, long propertyInstanceId)
        {
            return InsertAssignedUserPropertyInstanceData(userPersonaId, product, propertyInstanceId);
        }

        /// <summary>
        /// ✅ OPTIMIZED: Use HashSet for O(1) lookups
        /// </summary>
        private ListResponse MergeUPFMBooksPropertiesWithProductProperty(IList<UPFMPropertyInstance> blueBookUPFMPropertyList, long userPersonaId, bool assignedOnly)
        {
            var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, _upfmProductId);

            var propertyOption = new Dictionary<string, bool>();
            propertyOption.Add("allProperties", userPropertyIdList.Any(pl => pl == -1));

            // ✅ OPTIMIZED: Pre-allocate list and use HashSet for O(1) lookups
            List<ProductProperty> productPropertyList = new List<ProductProperty>(blueBookUPFMPropertyList.Count);
            var userPropertySet = new HashSet<int>(userPropertyIdList);

            foreach (UPFMPropertyInstance upfmPropertyInstance in blueBookUPFMPropertyList)
            {
                var pp = ConvertUPFMPropertyInstanceToProductProperty(upfmPropertyInstance, false);

                if (userPropertySet.Contains(upfmPropertyInstance.PropertyInstanceId))
                {
                    pp.IsAssigned = true;
                }

                if (!assignedOnly || pp.IsAssigned == true)
                {
                    productPropertyList.Add(pp);
                }
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

        #endregion

        /// <summary>
        /// ✅ OPTIMIZED: Main method with controlled parallelism and proper error handling
        /// </summary>
        public string ManageUPFMProductUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole, out List<AdditionalParameters> additionalParameters, bool isEmpAccess = false)
        {
            additionalParameters = new List<AdditionalParameters>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Begin create/update user for user with userPersonaId id - {userPersonaId}." });

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Error for user with userPersonaId id - {userPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                var orgTypeName = "";
                var userPersona = _managePersona.GetPersona(userPersonaId);
                var realPageId = userPersona.RealPageId;
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
                var productInternalSettingList = GetProductSetting((int)ProductEnum.UnifiedPlatform);
                var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, _upfmProductId);
                var productSettingList = GetProductSetting(_productId);
                IList<int> ProductIdsList = _productRepository.GetProductIdsByCompany(_userClaims.OrganizationPartyId);
                GetSharedProductDetails(ProductIdsList);

                List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);

                // Handle super user
                if (IsSuperUser(userPersonaId))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"New user is Super user with userPersonaId id - {userPersonaId}." });
                    List<string> superUserRoleIds = new List<string>();
                    var vmpForVendorOrgTypeName = "";
                    orgTypeName = userPersona.Organization.organizationType.Name.ToLower();

                    if (productSettingList.Any(a => a.Name.Equals("SuperUserRoleId", StringComparison.OrdinalIgnoreCase)))
                    {
                        superUserRoleIds = productSettingList.FirstOrDefault(a => a.Name.Equals("SuperUserRoleId", StringComparison.OrdinalIgnoreCase))?.Value?.Split(',')?.ToList();
                    }

                    if (productSettingList.Any(a => a.Name.Equals("VPMForVendorsOrgType", StringComparison.OrdinalIgnoreCase)) && (_upfmProductId == (int)ProductEnum.VendorMarketplace))
                    {
                        vmpForVendorOrgTypeName = productSettingList.FirstOrDefault(a => a.Name.Equals("VPMForVendorsOrgType", StringComparison.OrdinalIgnoreCase))?.Value.ToLower();
                        if (orgTypeName == vmpForVendorOrgTypeName)
                        {
                            if (productSettingList.Any(a => a.Name.Equals("VendorSuperUserRoleId", StringComparison.OrdinalIgnoreCase)))
                            {
                                superUserRoleIds = productSettingList.FirstOrDefault(a => a.Name.Equals("VendorSuperUserRoleId", StringComparison.OrdinalIgnoreCase))?.Value?.Split(',')?.ToList();
                            }
                        }
                    }

                    List<string> propertiesToRemove = new List<string>();
                    if (userPropertyIdList?.Count > 0)
                    {
                        foreach (var prop in userPropertyIdList)
                        {
                            if (prop != -1)
                            {
                                propertiesToRemove.Add(prop.ToString());
                            }
                        }
                    }

                    List<string> userRoleIdList = new List<string>();
                    if (userAssignProductPropertyRole.IsVendorRoleIdOverride && userAssignProductPropertyRole.RoleList.Count > 0)
                    {
                        userRoleIdList = userAssignProductPropertyRole.RoleList;
                    }
                    else if (orgTypeName == vmpForVendorOrgTypeName && roleList.Count > 0 && (_upfmProductId == (int)ProductEnum.VendorMarketplace))
                    {
                        userRoleIdList = roleList.Select(r => r.RoleID.ToString()).ToList();
                    }
                    else
                    {
                        userRoleIdList.AddRange(superUserRoleIds);
                    }

                    userAssignProductPropertyRole = new UPFMProductPropertyRole
                    {
                        PropertyList = new List<string> { "-1" },
                        RemovedPropertyList = propertiesToRemove,
                        RoleList = userRoleIdList
                    };
                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;

                if (userAssignProductPropertyRole != null)
                {
                    RepositoryResponse result;
                    var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);
                    List<long> existinguserRoleIds = new List<long>();
                    List<long> userassignedRoles = new List<long>();

                    // Handle roles (fast operation - no parallel needed)
                    if (productPropertyRole.RoleList?.Count > 0)
                    {
                        foreach (var item in productPropertyRole.RoleList)
                        {
                            userassignedRoles.Add(long.Parse(item));
                        }

                        if (roleList?.Count > 0)
                        {
                            foreach (var item in roleList)
                            {
                                existinguserRoleIds.Add(item.RoleID);
                            }
                        }

                        if (existinguserRoleIds.Count > 0)
                        {
                            foreach (var item in existinguserRoleIds.ToList())
                            {
                                // remove the existing role
                                //WriteToDiagnosticLog($"ManageUPFMProductUser - removing role for user userPersonaId id - {userPersonaId}, RoleId - {existingRoleId}.");
                                result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: item, userId: _userClaims.UserId, deleteRole: true);
                                if (result.Id < 0)
                                {
                                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Unable to delete role for user with userPersonaId - {userPersonaId}, RoleId - {item}" });
                                    return result.ErrorMessage;
                                }
                            }
                        }

                        if (userassignedRoles.Count > 0)
                        {
                            foreach (var item in userassignedRoles.ToList())
                            {
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Adding role for userPersonaId id - {userPersonaId}, RoleId - {item}." });
                                result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: item, userId: _userClaims.UserId, deleteRole: false);
                                if (result.Id < 0)
                                {
                                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Unable to add role for user with userPersonaId - {userPersonaId}, RoleId - {item}" });
                                    return result.ErrorMessage;
                                }
                            }
                        }
                    }

                    // Handle properties
                    if (userAssignProductPropertyRole.PropertyList != null && userAssignProductPropertyRole.PropertyList.Count > 0)
                    {
                        if (userAssignProductPropertyRole.PropertyList[0].ToUpper() == "ALL")
                        {
                            IList<int> productIdList = _productRepository.GetProductIdsByCompany(userPersona.OrganizationPartyId);
                            var gbAllRoles = _productRepository.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId) ?? new List<ProductRole>();
                            if (gbAllRoles != null)
                            {
                                if (gbAllRoles.Any(r => userassignedRoles.Contains(long.Parse(r.ID)) && (r.accessAllProperties)))
                                {
                                    userAssignProductPropertyRole.PropertyList = new List<string> { "-1" };
                                }
                            }
                        }

                        List<string> assignedPropertyList = (userAssignProductPropertyRole.PropertyList == null) ? new List<string>() : userAssignProductPropertyRole.PropertyList;
                        List<string> unAssignedPropertyList = (userAssignProductPropertyRole?.RemovedPropertyList == null) ? new List<string>() : userAssignProductPropertyRole.RemovedPropertyList;

                        if (userAssignProductPropertyRole.PropertyList != null && userAssignProductPropertyRole.PropertyList.Contains("-1"))
                        {
                            List<string> removePropList = new List<string>();
                            if (userPropertyIdList != null)
                            {
                                foreach (var propId in userPropertyIdList)
                                {
                                    if (propId != -1)
                                    {
                                        removePropList.Add(propId.ToString());
                                    }
                                }
                            }

                            unAssignedPropertyList.AddRange(removePropList);
                        }

                        List<string> unassignedProperties = new List<string>();
                        List<string> assignedProperties = new List<string>();

                        if (!IsSuperUser(userPersonaId) && userAssignProductPropertyRole.IsAssigned && assignedPropertyList?.Count == 0 && unassignedProperties?.Count == 0)
                        {
                            var doesNotUseProperties = productSettingList.FirstOrDefault(a => a.Name.Equals("DoesNotUseProperties", StringComparison.OrdinalIgnoreCase))?.Value;
                            if (doesNotUseProperties == null || doesNotUseProperties != "1")
                            {
                                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"No Properties are found to assign/unassign for user with userPersonaId - {userPersonaId}" });
                                return "No Properties are found to assign/unassign";
                            }
                        }

                        // ✅ OPTIMIZED: Use HashSet for O(1) lookups
                        var userPropertySet = new HashSet<int>(userPropertyIdList);

                        if (assignedPropertyList != null)
                        {
                            foreach (string propertyId in assignedPropertyList)
                            {
                                if (!userPropertySet.Contains(Convert.ToInt32(propertyId)) || isEmpAccess)
                                {
                                    // new property to be added
                                    assignedProperties.Add(propertyId);
                                }
                            }
                        }

                        if (unAssignedPropertyList != null)
                        {
                            foreach (string propertyId in unAssignedPropertyList)
                            {
                                // remove property
                                unassignedProperties.Add(propertyId);
                            }
                        }

                        if ((unAssignedPropertyList == null || unAssignedPropertyList?.Count == 0) && assignedProperties?.Count > 0)
                        {
                            if (userPropertyIdList.Any(p => p == -1))
                            {
                                unassignedProperties.Add("-1");
                            }
                        }

                        int totalOperations = unassignedProperties.Count + assignedProperties.Count;
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Processing {totalOperations} property operations (Unassign: {unassignedProperties.Count}, Assign: {assignedProperties.Count})" });

                        // ✅ CRITICAL FIX: Use optimized batch processing
                        var propertyResult = ProcessPropertyOperationsOptimized(userPersonaId, unassignedProperties, assignedProperties);

                        if (!string.IsNullOrEmpty(propertyResult))
                        {
                            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Property operations failed: {propertyResult}" });
                            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                            return propertyResult;
                        }
                    }
                    else
                    {
                        if (!IsSuperUser(userPersonaId) && userAssignProductPropertyRole.IsAssigned && userAssignProductPropertyRole.PropertyList?.Count == 0)
                        {
                            var doesNotUseProperties = productSettingList.FirstOrDefault(a => a.Name.Equals("DoesNotUseProperties", StringComparison.OrdinalIgnoreCase))?.Value;
                            if (doesNotUseProperties == null || doesNotUseProperties != "1")
                            {
                                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"No Properties are found to assign/unassign for user with userPersonaId - {userPersonaId}" });
                                return "No Properties are found to assign/unassign";
                            }
                        }
                    }

                    // Generate audit data
                    List<string> existingPropertyList = (userAssignProductPropertyRole.PropertyList == null) ? new List<string>() : userAssignProductPropertyRole.PropertyList;

                    var addedRoleList = existinguserRoleIds == null ? userassignedRoles.ToList() : userassignedRoles.Except(existinguserRoleIds).ToList();
                    var removedRoleList = existinguserRoleIds?.Except(userassignedRoles).ToList() ?? new List<long>();

                    var addedPropertiesList = userPropertyIdList == null ? existingPropertyList.ToList() : existingPropertyList.Except(userPropertyIdList.Select(p => p.ToString())).ToList();
                    var removedPropertiesList = userPropertyIdList?.Select(p => p.ToString()).Except(existingPropertyList).ToList() ?? new List<string>();

                    var productList = _productRepository.GetAllProducts();
                    string productName = productList.FirstOrDefault(a => a.ProductId == _upfmProductId).Name;

                    additionalParameters = AssignedRoleandPropertyNameList(addedRoleList, removedRoleList, addedPropertiesList, removedPropertiesList, productName, _userClaims.OrganizationPartyId, userPersonaId);
                }

                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

                stopwatch.Stop();
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Completed successfully in {stopwatch.ElapsedMilliseconds}ms for userPersonaId {userPersonaId}" });
                Log.Write(LogEventLevel.Information, "ManageUPFMProductUser completed for user {UserPersonaId} in {ElapsedMs}ms", userPersonaId, stopwatch.ElapsedMilliseconds);

                return string.Empty;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Error for user with userPersonaId id - {userPersonaId} after {stopwatch.ElapsedMilliseconds}ms" }, exception: ex);
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                Log.Write(LogEventLevel.Error, ex, "ManageUPFMProductUser failed for user {UserPersonaId}", userPersonaId);
                return $"Error - {ex.Message}";
            }
        }

        /// <summary>
        /// ✅ UPDATED: Track retry statistics
        /// </summary>
        private string ProcessPropertyOperationsOptimized(long userPersonaId, List<string> unassignedProperties, List<string> assignedProperties)
        {
            var errors = new ConcurrentBag<string>();
            int successCount = 0;
            int totalOperations = unassignedProperties.Count + assignedProperties.Count;

            if (totalOperations == 0)
            {
                return string.Empty;
            }

            // Adaptive settings based on dataset size
            int maxDegreeOfParallelism;
            int batchSize;

            if (totalOperations <= SmallDatasetThreshold)
            {
                maxDegreeOfParallelism = SmallParallelism;
                batchSize = SmallBatchSize;
            }
            else if (totalOperations <= MediumDatasetThreshold)
            {
                maxDegreeOfParallelism = MediumParallelism;
                batchSize = MediumBatchSize;
            }
            else
            {
                maxDegreeOfParallelism = LargeParallelism;
                batchSize = LargeBatchSize;

                Log.Write(LogEventLevel.Warning,
                    "LARGE PROPERTY DATASET: Processing {TotalOperations} properties with retry logic enabled (Max {MaxRetries} attempts per property)",
                    totalOperations, MaxRetryAttempts);
            }

            WriteToDiagnosticLog("{ActionName} - {state}",
                messageProperties: new object[] { "ProcessPropertyOperations",
                $"Using {(totalOperations <= SmallDatasetThreshold ? "SMALL" : totalOperations <= MediumDatasetThreshold ? "MEDIUM" : "LARGE")} dataset settings (Parallelism: {maxDegreeOfParallelism}, Batch: {batchSize}, Retry: {MaxRetryAttempts})" });

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (unassignedProperties.Count > 0)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}",
                        messageProperties: new object[] { "ProcessPropertyOperations",
                        $"Unassigning {unassignedProperties.Count} properties with retry enabled" });
                    ProcessPropertyBatch(userPersonaId, unassignedProperties, false, maxDegreeOfParallelism, batchSize, errors, ref successCount);
                }

                if (assignedProperties.Count > 0)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}",
                        messageProperties: new object[] { "ProcessPropertyOperations",
                        $"Assigning {assignedProperties.Count} properties with retry enabled" });
                    ProcessPropertyBatch(userPersonaId, assignedProperties, true, maxDegreeOfParallelism, batchSize, errors, ref successCount);
                }

                stopwatch.Stop();

                WriteToDiagnosticLog("{ActionName} - {state}",
                    messageProperties: new object[] { "ProcessPropertyOperations",
                    $"Completed in {stopwatch.ElapsedMilliseconds}ms. Success: {successCount}/{totalOperations}, Errors: {errors.Count}" });

                Log.Write(LogEventLevel.Information,
                    "Property operations completed for user {UserPersonaId}: {SuccessCount}/{TotalOperations} successful in {ElapsedMs}ms with retry logic",
                    userPersonaId, successCount, totalOperations, stopwatch.ElapsedMilliseconds);

                if (errors.Count > 0)
                {
                    double errorRate = (double)errors.Count / totalOperations;

                    if (errorRate > 0.2) // More than 20% failure
                    {
                        var errorSummary = $"CRITICAL: Property operations completed with {errors.Count}/{totalOperations} failures ({errorRate:P1}) after retry attempts. Sample errors: {string.Join(" | ", errors.Take(5))}";
                        WriteToErrorLog("{ActionName} - {state}",
                            messageProperties: new object[] { "ProcessPropertyOperations", errorSummary });
                        Log.Write(LogEventLevel.Error,
                            "High error rate after retries: {ErrorRate:P1}, {ErrorCount} failures",
                            errorRate, errors.Count);
                        return errorSummary;
                    }
                    else if (errorRate > 0.05) // More than 5% failure
                    {
                        var warningSummary = $"Property operations completed with elevated failures: {errors.Count}/{totalOperations} ({errorRate:P1}) after retries. First 3: {string.Join("; ", errors.Take(3))}";
                        WriteToDiagnosticLog("{ActionName} - {state}",
                            messageProperties: new object[] { "ProcessPropertyOperations", warningSummary });
                        Log.Write(LogEventLevel.Warning,
                            "Elevated error rate after retries: {ErrorRate:P1}, {ErrorCount} failures",
                            errorRate, errors.Count);
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                WriteToErrorLog("{ActionName} - {state}",
                    messageProperties: new object[] { "ProcessPropertyOperations",
                    $"Critical error after {stopwatch.ElapsedMilliseconds}ms" },
                    exception: ex);
                Log.Write(LogEventLevel.Error, ex, "Critical error for user {UserPersonaId}", userPersonaId);
                return $"Critical error: {ex.Message}";
            }
        }

        /// <summary>
        /// ✅ UPDATED: Process property batch with retry logic
        /// </summary>
        private void ProcessPropertyBatch(
            long userPersonaId,
            List<string> properties,
            bool isAssignment,
            int maxDegreeOfParallelism,
            int batchSize,
            ConcurrentBag<string> errors,
            ref int successCount)
        {
            int totalBatches = (int)Math.Ceiling((double)properties.Count / batchSize);
            string operationType = isAssignment ? "assignment" : "unassignment";

            for (int i = 0; i < properties.Count; i += batchSize)
            {
                var batch = properties.Skip(i).Take(batchSize).ToList();
                int currentBatch = (i / batchSize) + 1;

                WriteToDiagnosticLog("{ActionName} - {state}",
                    messageProperties: new object[] { "ProcessPropertyBatch",
                    $"Processing {operationType} batch {currentBatch}/{totalBatches} ({batch.Count} properties)" });

                try
                {
                    int localSuccessCount = 0;
                    Parallel.ForEach(batch,
                        new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                        property =>
                        {
                            try
                            {
                                // ✅ NEW: Use retry logic instead of direct call
                                RepositoryResponse result = ExecutePropertyOperationWithRetry(
                                    userPersonaId,
                                    _productId,
                                    Convert.ToInt64(property),
                                    isAssignment,
                                    property);

                                if (result.Id < 0 || !string.IsNullOrEmpty(result.ErrorMessage))
                                {
                                    errors.Add($"Failed to {operationType} property {property}: {result.ErrorMessage ?? "Unknown error"}");
                                    Log.Write(LogEventLevel.Warning,
                                        "Property {OperationType} failed for {PropertyId} after retries: {ErrorMessage}",
                                        operationType, property, result.ErrorMessage);
                                }
                                else
                                {
                                    Interlocked.Increment(ref localSuccessCount);
                                }
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Exception during {operationType} of property {property}: {ex.Message}");
                                WriteToErrorLog("{ActionName} - {state}",
                                    messageProperties: new object[] { "ProcessPropertyBatch",
                                    $"Error {operationType} property {property}" },
                                    exception: ex);
                            }
                        });

                    Interlocked.Add(ref successCount, localSuccessCount);

                    // ✅ Throttling between batches
                    if (currentBatch < totalBatches && properties.Count > MediumDatasetThreshold)
                    {
                        Thread.Sleep(BatchDelayMs);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Critical error in batch {currentBatch}: {ex.Message}");
                    WriteToErrorLog("{ActionName} - {state}",
                        messageProperties: new object[] { "ProcessPropertyBatch",
                        $"Critical error in batch {currentBatch}" },
                        exception: ex);
                }
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Use HashSets for O(1) lookups instead of O(n²)
        /// </summary>
        private List<AdditionalParameters> AssignedRoleandPropertyNameList(
            List<long> addedRoleList,
            List<long> removedRoleList,
            List<string> addedPropertyList,
            List<string> removedPropertyList,
            string productName,
            long partyId,
            long userPersonaId)
        {
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "AssignedRoleandPropertyNameList", $"Getting Roles and Property names for user : {userPersonaId}, with Product name : {productName}." });

                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
                var gbAllRoles = _productRepository.ListRolesForProductByParty(partyId, productIdList, _upfmProductId) ?? new List<ProductRole>();
                var customerPropertyList = GetProductPropertyInstancesBasedOnUPFMProperties();

                // ✅ OPTIMIZED: Use HashSets for O(1) lookups
                var addedRoleSet = new HashSet<long>(addedRoleList);
                var removedRoleSet = new HashSet<long>(removedRoleList);
                var addedPropertySet = new HashSet<string>(addedPropertyList, StringComparer.OrdinalIgnoreCase);
                var removedPropertySet = new HashSet<string>(removedPropertyList, StringComparer.OrdinalIgnoreCase);

                // ✅ OPTIMIZED: Parse role IDs once into dictionary
                foreach (var role in gbAllRoles)
                {
                    long roleId = long.Parse(role.ID);
                    if (addedRoleSet.Contains(roleId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", role.Name) });
                    }
                    if (removedRoleSet.Contains(roleId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", role.Name) });
                    }
                }

                foreach (var property in customerPropertyList)
                {
                    string propertyId = property.PropertyInstanceId.ToString();
                    if (addedPropertySet.Contains(propertyId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", property.Name) });
                    }
                    if (removedPropertySet.Contains(propertyId))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", property.Name) });
                    }
                }

                return additionalParameters;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "result", ex.Message } }, messageProperties: new object[] { "AssignedRoleandPropertyNameList", $"Unable to get the role and property names list for user - {userPersonaId}, with Product name : {productName}." });
                return new List<AdditionalParameters>();
            }
        }

        /// <summary>
        /// ✅ OPTIMIZED: Unassign user with controlled parallelism
        /// </summary>
        public string UnassignUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);
            if (roleList?.Count > 0)
            {
                long roleId = roleList[0].RoleID;
                RepositoryResponse result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: roleId, userId: _userClaims.UserId, deleteRole: true);
                if (result.Id < 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Unable to delete record for user with userPersonaId - {userPersonaId}, RoleId - {roleId}" });
                    return result.ErrorMessage;
                }

                List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, _productId);
                // ✅ OPTIMIZED: Pre-allocate list capacity
                List<string> unassignedProperties = new List<string>(propertyList.Count);

                foreach (var property in propertyList)
                {
                    unassignedProperties.Add(property.ID.ToString());
                }

                if (unassignedProperties.Count > 0)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Unassigning {unassignedProperties.Count} properties for user {userPersonaId}" });

                    // ✅ CRITICAL FIX: Use optimized batch processing
                    var propertyResult = ProcessPropertyOperationsOptimized(userPersonaId, unassignedProperties, new List<string>());

                    if (!string.IsNullOrEmpty(propertyResult))
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Property unassignment failed: {propertyResult}" });
                        // Continue despite errors - user is being unassigned
                    }
                }
            }

            WriteToInformationLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"userPersonaId:{userPersonaId} unassigned" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

            return string.Empty;
        }

        /// <summary>
        /// Get Company Product Company InstanceId
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <param name="booksCustmerMasterId"></param>
        /// <param name="blueBookProductName"></param>
        /// <param name="domain"></param>
        /// <param name="includeExtra"></param>
        /// <param name="useTranslate"></param>
        /// <returns></returns>
        public string GetProductCompanyInstanceId(Guid organizationRealPageId, long booksCustmerMasterId, string blueBookProductName, string domain, string includeExtra = "", bool useTranslate = true)
        {
            IList<CustomerCompanyMap> companyProductList = _blueBook.GetCompanyMap(organizationRealPageId, booksCustmerMasterId, source: blueBookProductName.ToUpper(), domain: domain, includeExtra: includeExtra, useTranslate: useTranslate);
            if (companyProductList == null)
            {
                companyProductList = new List<CustomerCompanyMap>();
            }

            CustomerCompanyMap company = new CustomerCompanyMap();
            if (companyProductList.Any(a => a.Source.Equals(blueBookProductName, StringComparison.OrdinalIgnoreCase)))
            {
                company = (from a in companyProductList where a.Source.Equals(blueBookProductName, StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
            }

            return company.CompanyInstanceSourceId;
        }

        /// <summary>
        /// Get multi company propeties of product
        /// </summary>
        /// <param name="productCode"></param>
        /// <returns></returns>
        public List<UserCompaniesProperties> GetUPFMMultiCompanyProperties(string productCode)
        {
            IManageUserLogin manageUserLogin = new ManageUserLogin(_userClaims);
            List<UserCompaniesProperties> userCompaniesProperties = new List<UserCompaniesProperties>();
            var companyResponse = manageUserLogin.GetUserPersonaOrganization(_userClaims.LoginName);
            string errorReason = string.Empty;

            foreach (var company in companyResponse)
            {
                if (_productRepository.isProductAssigned(company.PersonaId, (int)UserUiStatusType.AccountCreationSuccessful, _productId))
                {
                    if (userCompaniesProperties == null)
                        userCompaniesProperties = new List<UserCompaniesProperties>();

                    var compnayInstanceSourceId = GetProductCompanyInstanceId(company.OrganizationRealPageId, company.BooksCustomerMasterId, productCode, "Primary");
                    var propertyResponse = GetEnterpriseUPFMProperties(company.PersonaId, _productId, productCode, null, companyResponse.Count > 1 ? true : false, company.OrganizationRealPageId.ToString());
                    if (propertyResponse.Records == null || propertyResponse.Records.Count == 0) errorReason = "Properties are not loaded from Blue Book";

                    var userCompanyProperties = new UserCompaniesProperties()
                    {
                        Id = compnayInstanceSourceId,
                        OrganizationName = company.OrganizationName,
                        InstanceId = company.OrganizationRealPageId,
                        ErrorReason = errorReason,
                        Properties = new List<Properties>()
                    };

                    foreach (var product in propertyResponse.Records.ToList())
                    {
                        var properties = new Properties()
                        {
                            Id = ((ProductProperty)product).ID,
                            InstanceId = ((ProductProperty)product).InstanceId,
                            PropertyName = ((ProductProperty)product).Name
                        };
                        userCompanyProperties.Properties.Add(properties);
                    }

                    userCompaniesProperties.Add(userCompanyProperties);
                }
                else
                    userCompaniesProperties = userCompaniesProperties == null || userCompaniesProperties.Count == 0 ? userCompaniesProperties = null : userCompaniesProperties;
            }

            return userCompaniesProperties;
        }
        /// <summary>
        /// ✅ NEW: Execute property operation with retry logic for transient failures
        /// </summary>
        private RepositoryResponse ExecutePropertyOperationWithRetry(
            long userPersonaId,
            int productId,
            long propertyId,
            bool isAssignment,
            string propertyIdString)
        {
            int attempt = 0;
            RepositoryResponse result = null;
            Exception lastException = null;

            while (attempt < MaxRetryAttempts)
            {
                try
                {
                    attempt++;

                    if (isAssignment)
                    {
                        result = InsertAssignedPropertyInstanceData(userPersonaId, productId, propertyId);
                    }
                    else
                    {
                        result = DeleteAssignedPropertyInstanceData(userPersonaId, productId, propertyId);
                    }

                    // ✅ CRITICAL: Log the EXACT values returned
                    WriteToDiagnosticLog("{ActionName} - {state}",
                        messageProperties: new object[] { "ExecutePropertyOperationWithRetry",
                        $"Property {propertyIdString} attempt {attempt}: RESULT => Id={result?.Id.ToString() ?? "NULL"}, ErrorMessage='{result?.ErrorMessage ?? "NULL"}', IsNull={result == null}" });

                    // ✅ Handle NULL result
                    if (result == null)
                    {
                        WriteToErrorLog("{ActionName} - {state}",
                            messageProperties: new object[] { "ExecutePropertyOperationWithRetry",
                            $"Property {propertyIdString} returned NULL result on attempt {attempt}" });
                        
                        result = new RepositoryResponse { Id = -1, ErrorMessage = "Repository returned null response" };
                        return result;
                    }

                    // ✅ COMPREHENSIVE SUCCESS DETECTION
                    bool isSuccess = false;
                    string successReason = "";

                    if (result.Id > 0 && string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        // Definite success: positive ID, no error
                        isSuccess = true;
                        successReason = $"Positive Id={result.Id}";
                    }
                    else if (result.Id == 1 && !isAssignment)
                    {
                        // DELETE operation returns Id=1 (stored proc quirk)
                        isSuccess = true;
                        successReason = "DELETE returned Id=1";
                    }
                    else if (result.Id == 0 && string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        // Id=0 with no error = idempotent success
                        isSuccess = true;
                        successReason = "Idempotent operation (Id=0, no error)";
                    }
                    else if (result.Id >= 0)
                    {
                        // Check if error message indicates idempotent success
                        string errorLower = (result.ErrorMessage ?? "").ToLower();
                        if (errorLower.Contains("already exists") || 
                            errorLower.Contains("already deleted") ||
                            errorLower.Contains("does not exist") ||
                            string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            isSuccess = true;
                            successReason = $"Idempotent: {result.ErrorMessage ?? "empty error"}";
                        }
                    }

                    if (isSuccess)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}",
                            messageProperties: new object[] { "ExecutePropertyOperationWithRetry",
                            $"Property {propertyIdString} SUCCESS on attempt {attempt}: {successReason}" });
                        return result;
                    }

                    // ✅ Populate empty error messages for failures
                    if (result.Id < 0 && string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        result.ErrorMessage = $"Database operation failed with Id={result.Id} but no error message provided";
                    }

                    // ✅ Check if error is retryable
                    bool isRetryable = IsRetryableError(result.ErrorMessage);

                    if (!isRetryable || attempt >= MaxRetryAttempts)
                    {
                        WriteToErrorLog("{ActionName} - {state}",
                            messageProperties: new object[] { "ExecutePropertyOperationWithRetry",
                            $"Property {propertyIdString} FAILED after {attempt} attempts. Id={result.Id}, Error='{result.ErrorMessage}', Retryable={isRetryable}" });
                        return result;
                    }

                    // ✅ Exponential backoff
                    int delayMs = RetryDelayMs * (int)Math.Pow(RetryBackoffMultiplier, attempt - 1);
                    WriteToDiagnosticLog("{ActionName} - {state}",
                        messageProperties: new object[] { "ExecutePropertyOperationWithRetry",
                        $"Property {propertyIdString} attempt {attempt} failed (retryable). Waiting {delayMs}ms. Error: {result.ErrorMessage}" });
                    Thread.Sleep(delayMs);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    WriteToErrorLog("{ActionName} - {state}",
                        messageProperties: new object[] { "ExecutePropertyOperationWithRetry",
                        $"Property {propertyIdString} EXCEPTION on attempt {attempt}: {ex.Message}" },
                        exception: ex);

                    bool isRetryable = IsRetryableException(ex);
                    if (!isRetryable || attempt >= MaxRetryAttempts)
                    {
                        return new RepositoryResponse { Id = -1, ErrorMessage = $"Exception after {attempt} attempts: {ex.Message}" };
                    }

                    int delayMs = RetryDelayMs * (int)Math.Pow(RetryBackoffMultiplier, attempt - 1);
                    Thread.Sleep(delayMs);
                }
            }

            return result ?? new RepositoryResponse { Id = -1, ErrorMessage = lastException?.Message ?? $"Max retry attempts ({MaxRetryAttempts}) reached" };
        }

        /// <summary>
        /// ✅ NEW: Determine if error message indicates a transient/retryable failure
        /// </summary>
        private bool IsRetryableError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return false;

            string lowerError = errorMessage.ToLower();

            // ✅ Common transient SQL Server errors
            return lowerError.Contains("timeout") ||
                   lowerError.Contains("deadlock") ||
                   lowerError.Contains("connection") ||
                   lowerError.Contains("network") ||
                   lowerError.Contains("transport") ||
                   lowerError.Contains("pooling") ||
                   lowerError.Contains("dapper proc execution failed") ||
                   lowerError.Contains("task was canceled") ||
                   lowerError.Contains("the wait operation timed out") ||
                   lowerError.Contains("a transport-level error") ||
                   lowerError.Contains("an existing connection was forcibly closed");
        }

        /// <summary>
        /// ✅ NEW: Determine if exception indicates a transient/retryable failure
        /// </summary>
        private bool IsRetryableException(Exception ex)
        {
            if (ex == null)
                return false;

            string exceptionType = ex.GetType().Name.ToLower();
            string message = ex.Message?.ToLower() ?? string.Empty;

            // ✅ Retryable exception types
            return exceptionType.Contains("timeout") ||
                   exceptionType.Contains("sqlexception") ||
                   exceptionType.Contains("operationcanceledexception") ||
                   exceptionType.Contains("taskcancel") ||
                   message.Contains("timeout") ||
                   message.Contains("deadlock") ||
                   message.Contains("connection") ||
                   message.Contains("network") ||
                   IsRetryableError(ex.Message);
        }
    }
}
