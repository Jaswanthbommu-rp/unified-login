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
using System;
using System.Collections.Generic;
using System.Linq;
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

            List<int> productIds = new List<int>();
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
                result.RoleList = new List<string>();
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
            /*
                Updating product code to ProductEnum.UnifiedPlatform for CIMPL and Settings 
                because these two products properties saved as productid 3 in UP database
            */
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

                    customerPropertyList.ToList().FindAll(b => userPropertyIdList.Any(p => p == b.PropertyInstanceId)).ForEach(cp => { userPropertyList.Add(ConvertUPFMPropertyInstanceToProductProperty(cp, true)); });
                }
            }


            if (userPropertyIdList?.Count > 0)
            {
                // call translate with upfm properties to get ib property id and merges propertyinstanceid with translated id
                //note save upfmid into alias field before updating with translated id
                UPFMProperty upfmProperties = new UPFMProperty();
                List<string> instanceids = new List<string>();
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
                    foreach (var attributes in translatedData.Data.Attributes)
                    {
                        foreach (var propertyData in attributes.TranslatedPropertyInstances)
                        {
                            if (propertyData.Source == booksProductCode)
                            {
                                var translatedProductProperty = userPropertyList.FirstOrDefault(u => u.InstanceId == attributes.PropertyInstanceSourceId);
                                if (translatedProductProperty != null)
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
                booksPropertyList = new List<Guid>();

                UPFMProperty properties = new UPFMProperty { id = new List<string>() };

                booksUPFMPropertyList.ForEach(c => properties.id.Add(c.ToString()));

                var translatedProperties = _blueBook.GetTranslatePropertiesFromUPFMToProductv3(properties, _udmSourceCode.ToUpper());
                if (translatedProperties?.Data?.Attributes.Count > 0)
                {
                    foreach (var t in translatedProperties.Data.Attributes)
                    {
                        booksPropertyList.Add(new Guid(t.PropertyInstanceSourceId));
                    }

                    customerPropertyList = ListUPFMPropertyInstanceIdByInstanceIds(booksPropertyList);
                    customerPropertyList.ForEach(cp =>
                    {
                        if (translatedProperties.Data.Attributes.Any(tp => tp.PropertyInstanceSourceId.Equals(cp.InstanceId.ToString(), StringComparison.OrdinalIgnoreCase)))
                        {
                            cp.CustomerPropertyId = translatedProperties?.Data?.Attributes?.FirstOrDefault(p => p.PropertyInstanceSourceId.Equals(cp.InstanceId.ToString(), StringComparison.OrdinalIgnoreCase))?.TranslatedPropertyInstances[0].PropertyInstanceSourceId;
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



        /// <summary>
        /// Used to unassign a property instance to the given user
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="propertyInstanceId"></param>
        /// <returns></returns>
        private RepositoryResponse DeleteAssignedPropertyInstanceData(long userPersonaId, int product, long propertyInstanceId)
        {
            return DeleteAssignedUserPropertyInstanceData(userPersonaId, product, propertyInstanceId);
        }

        /// <summary>
        /// Used to assign a property instance to the given user
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <param name="propertyInstanceId"></param>
        /// <returns></returns>
        private RepositoryResponse InsertAssignedPropertyInstanceData(long userPersonaId, int product, long propertyInstanceId)
        {
            return InsertAssignedUserPropertyInstanceData(userPersonaId, product, propertyInstanceId);
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
            var userPropertyIdList = GetAssignedUPFMPropertyIdsForPersona(userPersonaId, _upfmProductId);

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

        #endregion

        /// <summary>
        /// Used to create/update a user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="userAssignProductPropertyRole"></param>
        /// <param name="isEmpAccess"></param>
        /// <returns></returns>
        public string ManageUPFMProductUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole, out List<AdditionalParameters> additionalParameters, bool isEmpAccess = false)
        {
            additionalParameters = new List<AdditionalParameters>();
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
                // Existing user Roles
                List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);

                // super user
                // TODO what to do here?
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
                    if (userAssignProductPropertyRole.IsVendorRoleIdOverride && userAssignProductPropertyRole.RoleList.Count > 0 )
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

                    // map userAssignProductPropertyRole to ProductPropertyRole
                    var productPropertyRole = MapGbObjectToProduct(userAssignProductPropertyRole);
                    List<long> existinguserRoleIds = new List<long>();
                    List<long> userassignedRoles = new List<long>();

                    if (productPropertyRole.RoleList?.Count > 0)
                    {
                        //role.RoleID = long.Parse(productPropertyRole.RoleList[0]);
                        foreach (var item in productPropertyRole.RoleList)
                        {
                            userassignedRoles.Add(long.Parse(item));
                        }
                        // Existing user Roles
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
                                //add the role
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
                    if (userAssignProductPropertyRole.PropertyList != null && userAssignProductPropertyRole.PropertyList.Count > 0)
                    {
                        if (userAssignProductPropertyRole.PropertyList[0].ToUpper() == "ALL")
                        {
                            IList<int> productIdList = _productRepository.GetProductIdsByCompany(userPersona.OrganizationPartyId);
                            var gbAllRoles = _productRepository.ListRolesForProductByParty(userPersona.OrganizationPartyId, productIdList, _productId) ?? new List<ProductRole>();
                            if (gbAllRoles != null)
                            {
                                // role.RoleID = long.Parse(productPropertyRole.RoleList[0]);

                                //if (gbAllRoles.Any(r => long.Parse(r.ID) == role.RoleID && (r.accessAllProperties)))
                                if (gbAllRoles.Any(r => userassignedRoles.Contains(long.Parse(r.ID)) && (r.accessAllProperties)))
                                {
                                    userAssignProductPropertyRole.PropertyList = new List<string> { "-1" };
                                }
                            }

                        }

                        List<string> assignedPropertyList = (userAssignProductPropertyRole.PropertyList == null) ? new List<string>() : userAssignProductPropertyRole.PropertyList;
                        List<string> unAssignedPropertyList = (userAssignProductPropertyRole?.RemovedPropertyList == null) ? new List<string>() : userAssignProductPropertyRole.RemovedPropertyList;
                        /*
                         *Unassign all the individual properties if property list has -1(all properties selection is true)
                         */
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

                        if (assignedPropertyList != null)
                        {
                            foreach (string propertyId in assignedPropertyList)
                            {
                                if (userPropertyIdList.All(p => p != Convert.ToInt32(propertyId)) || isEmpAccess)
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

                        if (unassignedProperties.Count > 0)
                        {
                            Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyInstanceData(userPersonaId, _productId, Convert.ToInt64(property)); });
                        }

                        if (assignedProperties.Count > 0)
                        {
                            Parallel.ForEach(assignedProperties, property => { result = InsertAssignedPropertyInstanceData(userPersonaId, _productId, Convert.ToInt64(property)); });
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

                return string.Empty;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageUPFMProductUser", $"Error for user with userPersonaId id - {userPersonaId}" }, exception: ex);
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                return $"Error - {ex.Message}";
            }
        }

        private List<AdditionalParameters> AssignedRoleandPropertyNameList(List<long> addedRoleList, List<long> removedRoleList, List<string> addedPropertyList, List<string> removedPropertyList, string productName, long partyId, long userPersonaId)
        {

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "AssignedRoleandPropertyNameList", $"Getting Roles and Property names for user : {userPersonaId}, with Product name : {productName}." });

                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                IList<int> productIdList = _productRepository.GetProductIdsByCompany(partyId);
                var gbAllRoles = _productRepository.ListRolesForProductByParty(partyId, productIdList, _upfmProductId) ?? new List<ProductRole>();
                var customerPropertyList = GetProductPropertyInstancesBasedOnUPFMProperties();
                foreach (var role in gbAllRoles)
                {
                    if (addedRoleList.Contains(long.Parse(role.ID)))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", role.Name) });
                    }
                    if (removedRoleList.Contains(long.Parse(role.ID)))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", role.Name) });
                    }
                }
                foreach (var property in customerPropertyList)
                {
                    if (addedPropertyList.Contains(property.PropertyInstanceId.ToString()))
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = productName + " Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", property.Name) });
                    }
                    if (removedPropertyList.Contains(property.PropertyInstanceId.ToString()))
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
        /// Unassign User
        /// </summary> 
        public string UnassignUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            //var product = ProductEnumHelper.GetUPFMProductEnum(_upfmProductId);

            List<UL.Role> roleList = GetAssignedRoleForPersona(userPersonaId);
            if (roleList?.Count > 0)
            {
                long roleId = roleList[0].RoleID;
                // Delete existing roleId
                RepositoryResponse result = _userRoleRightRepository.InsertAssignedRoleToUser(userPersonaId: userPersonaId, roleId: roleId, userId: _userClaims.UserId, deleteRole: true);
                if (result.Id < 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Unable to delete record for user with userPersonaId - {userPersonaId}, RoleId - {roleId}" });
                    return result.ErrorMessage;
                }

                List<ProductProperty> propertyList = GetAssignedPropertyForPersona(userPersonaId, _productId);
                List<string> unassignedProperties = new List<string>();

                foreach (var property in propertyList)
                {
                    unassignedProperties.Add(property.ID.ToString());
                }

                if (unassignedProperties.Count > 0)
                {
                    Parallel.ForEach(unassignedProperties, property => { result = DeleteAssignedPropertyInstanceData(userPersonaId, _productId, Convert.ToInt64(property)); });
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
    }
}
