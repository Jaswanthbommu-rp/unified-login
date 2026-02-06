using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Exceptions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    /// On-Site integration
    /// </summary>
    public class ManageProductOnSite : ManageProductBase, IManageProductOnSite
    {
        #region Private members

        private IProductInternalSettingRepository _productInternalSettingRepository;
        private string _clientId;
        private string _apiEndPoint;
        private string _apiSecret;
        private string _accessToken;
        private string _tokenEndPoint;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public ManageProductOnSite(DefaultUserClaim userClaims) : base((int)ProductEnum.OnSite, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductOnSite", "Getting Product settings" });
#endif
            _productId = (int)ProductEnum.OnSite;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _editorRealPageId = userClaims.UserRealPageGuid;

            _blueBook = new ManageBlueBook(userClaims);

            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value; //"https://staging9.on-site.com/api/greenbook"; //
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value; //"f3865f8b7c1a2177b0147f2ab1bb3ccfee25f716f883eb341d700986a61d4048";
            _clientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value; //"3431c19ab693ead1bfe2a138e2a220f3d96c6e24d3c4547236c9f7b52cb0d4e5";
            _tokenEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENURL").Value; //*/"https://staging9.on-site.com/oauth/token";
            //_apiEndPoint = "https://staging2.on-site.com/api/greenbook"; //
            //_apiSecret = "166d471cb0ce32d6a6a46a5564f56cfd5806a957e51305f6f97e371446730fd6";
            //_clientId = "57bd3e1874f18787d720eb2712217710f9cc788d9ee121d152f16ce92e983d11";
            //_tokenEndPoint = "https://staging2.on-site.com/oauth/token";
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductOnSite", "Received Product settings; getting token" });
#endif
            GetToken();
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaims"></param>
        /// <param name="messageHandler"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="samlRepository"></param>
        /// <param name="blueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="repository"></param>
        public ManageProductOnSite(Guid editorRealPageId, DefaultUserClaim userClaims, HttpMessageHandler messageHandler, IProductInternalSettingRepository productInternalSettingRepository,
            IManagePersona managePersona, ISamlRepository samlRepository, IManageBlueBook blueBook, IProductRepository productRepository, IRepository repository)
            : base((int)ProductEnum.OnSite, userClaims, repository, messageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _messageHandler = messageHandler;
            _productInternalSettingRepository = productInternalSettingRepository;
            _blueBook = blueBook;
            _managePersona = managePersona;
            _samlRepository = samlRepository;
            _productRepository = productRepository;
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value; //"https://staging9.on-site.com/api/greenbook"; //
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value; //"f3865f8b7c1a2177b0147f2ab1bb3ccfee25f716f883eb341d700986a61d4048";
            _clientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value; //"3431c19ab693ead1bfe2a138e2a220f3d96c6e24d3c4547236c9f7b52cb0d4e5";
            _tokenEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENURL").Value; //*/"https://staging9.on-site.com/oauth/token";

            _client = new HttpClient(messageHandler, false);
            GetToken();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get Properties
        /// </summary>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Beginning editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetCompanyEditorAndUserDetails error. editorPersona id - {editorPersonaId} - Reason: {result.ErrorReason}" });
                    return result;
                }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetProductCompanyInstanceId - Found blue book company instance source id - {companyInstanceSourceId} editorPersona id -{editorPersonaId}" });


                // get access groups from on-site product
                var allProperties = GetResultFromApi<IList<OnSiteProperty>>(_accessToken,
                    $"{_apiEndPoint}/properties?company_id={companyInstanceSourceId}"); //TODO: What Company ID?

                if (allProperties == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"No properties received from product. editorPersona id - {editorPersonaId}" });

                    response.IsError = true;
                    response.ErrorReason = "No properties received from product.";
                    return response;
                }

                //Sort the properties by name in ascending order
                allProperties = allProperties.Where(a => a.IsActive).OrderBy(p => p.GetName).ToList();

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MergePropertiesWithGreenbook calling. editorPersona id - {editorPersonaId} & _productUserId-{_productUserId}" });
                    response = MergePropertiesWithGreenbook(allProperties);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MergePropertiesWithGreenbook completed. editorPersona id - {editorPersonaId} & _productUserId-{_productUserId}" });
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = allProperties.Cast<object>().ToList(),
                        TotalRows = allProperties.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Complete. total rows - {response.TotalRows} editorPersona id - {editorPersonaId}" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetProperties", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });

                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
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
        /// Get Regions
        /// </summary>
        public ListResponse GetRegions(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"Beginning editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"Error. editorPersona id - {editorPersonaId} Reason: {result.ErrorReason}" });
                    return result;
                }

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                // get access groups from on-site product
                var allRegions = GetResultFromApi<IList<OnSiteRegion>>(_accessToken,
                    $"{_apiEndPoint}/regions?company_id={companyInstanceSourceId}");

                if (allRegions == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"No properties received from product. editorPersona id - {editorPersonaId}" });

                    response.IsError = true;
                    //UI calls getregions but diplays the results in the propertygroup tab
                    response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"MergeRegionsWithGreenbook calling. editorPersona id - {editorPersonaId} & _productUserId-{_productUserId}" });
                    response = MergeRegionsWithGreenbook(allRegions);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"MergeRegionsWithGreenbook completed. editorPersona id - {editorPersonaId} & _productUserId-{_productUserId}" });
                }
                else // Called during creating a new User
                {
                    Dictionary<string, bool> additionalData = new Dictionary<string, bool> { { "allRegions", false } };
                    response = new ListResponse()
                    {
                        Records = allRegions.OrderBy(p => p.GetRegionName).Cast<object>().ToList(),
                        TotalRows = allRegions.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1,
                        Additional = additionalData
                    };
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRegions", $"Complete. total rows - {response.TotalRows} editorPersona id - {editorPersonaId}" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRegions", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });

                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException blueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    //UI calls getregions but diplays the results in the propertygroup tab
                    response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                }
            }

            return response;
        }

        /// <summary>
        ///Get roles
        /// </summary>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor

                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error. editorPersona id - {editorPersonaId} Reason: {result.ErrorReason}" });
                    return result;
                }

                //int companyInstanceSourceId = 279; // to get sample groups 
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                // get access groups from on-site product
                var allRoles = GetResultFromApi<IList<OnSiteRole>>(_accessToken, $"{_apiEndPoint}/roles?company_id={companyInstanceSourceId}");

                if (allRoles == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"No access groups (roles) received from product. editorPersona id - {editorPersonaId}" });

                    response.IsError = true;
                    response.ErrorReason = "No User Access groups (roles) received from product.";
                    return response;
                }

                if (userPersonaId != 0 && !string.IsNullOrEmpty(_productUserId)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeAccessGroupsWithGreenbook calling. editorPersona id - {editorPersonaId} & _productUserId-{_productUserId}" });
                    response = MergeAccessGroupsWithGreenbook(allRoles);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeAccessGroupsWithGreenbook completed. editorPersona id - {editorPersonaId} & _productUserId-{_productUserId}" });
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = allRoles.Cast<object>().ToList(),
                        TotalRows = allRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Complete. total rows - {response.TotalRows} editorPersona id - {editorPersonaId}" });
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                }

                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetRoles", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
            }

            return response;
        }

        /// <summary>
        /// Unassign User
        /// </summary> 
        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error. userPersonaId:{userPersonaId}. Reason: {listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            // Get Company
            CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

            if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error. editorPersona id - {editorPersonaId} Error - Company not found" });
                return "Company Setup Error: Please Contact Support.";
            }

            // deactivate user
            var result = ActivateDeactivateOnSiteProductUser(company.CompanyInstanceSourceId, true);

            if (string.IsNullOrEmpty(result))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Deleted. userPersonaId: {userPersonaId}" });

                // remove product association in Unified Login
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus,
                    (int)ProductBatchStatusType.Deleted);

            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createUserPersonaId"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="rpList"></param>
        /// <param name="batchProcessType"></param>
        /// <returns></returns>
        public string ChangeOnSiteServiceUserType(long createUserPersonaId, long assignUserPersonaId, OnSiteUserPropertyRegionRole rpList, BatchProcessType batchProcessType)
        {
            return ManageOnSiteUser(createUserPersonaId, assignUserPersonaId, rpList, out var additionalParameters, batchProcessType);
        }

        /// <summary>
        /// Updated to create/update a user in On Site 
        /// </summary>
        public string ManageOnSiteUser(long editorPersonaId, long userPersonaId, OnSiteUserPropertyRegionRole userPropertyRegionRole, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"Begin. editorPersona id - {editorPersonaId}" });
            additionalParameters = new List<AdditionalParameters>();
            try
            {
                if (userPropertyRegionRole == null)
                {
                    throw new Exception("OnSiteUserPropertyRegionRole received null; check JSON in product batch table or parsing issue.");
                }

                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"Error. editorPersona id - {editorPersonaId}. Reason: {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                string userEmailAddress = string.Empty;

                // super user
                if (IsSuperUser(userPersonaId))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"New user is Super user. editorPersona id - {editorPersonaId}" });

                    userPropertyRegionRole = new OnSiteUserPropertyRegionRole
                    {
                        PropertyList = new List<int> { -1 },
                        RegionList = new List<int>(),
                        RoleList = new List<int> { 1000 }
                    };

                    // use login name as email for super user
                    userEmailAddress = userLogin.LoginName;
                }
                else if (IsRegularUserNoEmail(userPersonaId))
                {
                    // get the email address

                    var manageElectronicAddress = new ManageElectronicAddress();
                    var addresses = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);

                    if (addresses != null && addresses.Any(a => a.AddressType.ToUpper() == "EMAIL"))
                    {
                        userEmailAddress = (from a in addresses
                                            where a.AddressType.ToUpper() == "EMAIL"
                                            select a.AddressString).FirstOrDefault();
                    }
                }
                else
                {
                    userEmailAddress = userLogin.LoginName;
                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;

                // get user name from email
                productLoginName = GetUserCode(productLoginName);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"_productUsername for user is {_productUsername}" });

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"Error Company not found. editorPersona id - {editorPersonaId}" });
                    return "Company Setup Error: Please Contact Support.";
                }

                // Check for user locations
                string insUpdResult = string.Empty;
                int companyId = Convert.ToInt32(company.CompanyInstanceSourceId);

                OnSiteUser userBeforeUpdate = !string.IsNullOrEmpty(_productUsername) ? GetOnSiteUser(_productUsername) : new OnSiteUser() { OnSiteUserProfile = new OnSiteUserProfile() { Roles = new List<OnSiteRole>(), Properties = new PropertyAcsess() { PropertyIdList = new List<int>(), RegionIdList = new List<int>(), CompanyIdList = new List<int>() } } };

                OnSiteUserInsertUpdate onSiteUser = new OnSiteUserInsertUpdate
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = userEmailAddress,
                    UserName = productLoginName,
                    PhoneNumber = "",
                    IsActive = null,
                    Properties = MapUserPropertyAccess(userPropertyRegionRole, companyId),
                    Roles = MapUserRoles(userPropertyRegionRole, companyId)
                };

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "onSiteUser", JsonConvert.SerializeObject(onSiteUser) } }, messageProperties: new object[] { "ManageOnSiteUser", $"Json to call product API for user. editorPersona id - {editorPersonaId}" });

                if (string.IsNullOrEmpty(_productUsername)) // NEW USER
                {
                    // check if user name exists in product
                    if (!string.IsNullOrEmpty(productLoginName))
                    {
                        bool foundNewUserName = false;
                        int incrementor = 0;
                        while (!foundNewUserName)
                        {
                            var result = GetOnSiteUser(productLoginName);
                            if (result != null)
                            {
                                incrementor++;
                                productLoginName = productLoginName + incrementor.ToString();
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"User {productLoginName} already exists in On Site product, getting new one. editorPersona id - {editorPersonaId}" });
                            }
                            else
                            {
                                foundNewUserName = true;
                            }
                        }

                        // reassign in case user name change
                        onSiteUser.UserName = productLoginName;
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"Trying to CREATE user. editorPersona id - {editorPersonaId}" });

                    insUpdResult = InsertOnSiteProductUser(userPersonaId, editorPersonaId, productLoginName, onSiteUser, companyId);
                }
                else
                {
                    // UPDATE USER
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"Trying to UPDATE user. editorPersona id - {editorPersonaId}" });
                    onSiteUser.UserId = _productUserId;
                    onSiteUser.UserName = null;

                    // activate user - every time we have to call activate user before updating user
                    // this is because each user can have multiple companies & IsActive in User object has different meaning 
                    var activateResult = ActivateDeactivateOnSiteProductUser(company.CompanyInstanceSourceId);

                    if (string.IsNullOrEmpty(activateResult))
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOnSiteUser", $"Called ActivateDeactivateOnSiteProductUser userPersonaId: {userPersonaId}" });
                    }
                    else
                    {
                        throw new Exception($"ManageProductOnSite.ManageOnSiteUser; error while activating user before calling update/ Error contents - {activateResult}");
                    }

                    insUpdResult = UpdateOnSiteProductUser(userPersonaId, editorPersonaId, onSiteUser);
                    if (string.IsNullOrEmpty(insUpdResult) && (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin))
                    {
                        WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, batchProcessType);
                    }
                }

                //Activity Details
                //Roles
                Thread.Sleep(30000);// wait for the user to be created/updated in the product

                var rolesResponse = GetRoles(editorPersonaId, userPersonaId, new RequestParameter());
                List<OnSiteRole> roles = new List<OnSiteRole>();
                if (rolesResponse.Records != null)
                {
                    roles = rolesResponse.Records.Cast<OnSiteRole>().ToList();
                }

                var oldAccessCodes = userBeforeUpdate.OnSiteUserProfile.Roles.Select(s => s.Level);
                var newAccessCodes = onSiteUser.Roles.Select(s => s.Level);

                var removedRoles = oldAccessCodes.Except(newAccessCodes).ToList();
                var addedRoles = newAccessCodes.Except(oldAccessCodes).ToList();

                if (removedRoles.Any())
                {
                    foreach (int r in removedRoles)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "On-Site Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", roles.Find(f => f.Level == r).Title) });
                    }
                }
                if (addedRoles.Any())
                {
                    foreach (int r in addedRoles)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "On-Site Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", roles.Find(f => f.Level == r).Title) });
                    }
                }

                //Properties
                var propertiesListResponse = GetProperties(editorPersonaId, userPersonaId, null);
                List<OnSiteProperty> properties = new List<OnSiteProperty>();
                if (propertiesListResponse.Records != null)
                {
                    properties = propertiesListResponse.Records.Cast<OnSiteProperty>().ToList();
                }

                var removedProperties = userBeforeUpdate.OnSiteUserProfile.Properties.PropertyIdList.Except(onSiteUser.Properties.PropertyIdList).ToList();
                var addedProperties = onSiteUser.Properties.PropertyIdList.Except(userBeforeUpdate.OnSiteUserProfile.Properties.PropertyIdList).ToList();

                if (removedProperties.Any())
                {
                    foreach (var p in removedProperties)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "On-Site Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", properties.FirstOrDefault(f => f.GetPropertyId == p)?.GetName) });
                    }
                }
                if (addedProperties.Any())
                {
                    foreach (var p in addedProperties)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "On-Site Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", properties.FirstOrDefault(f => f.GetPropertyId == p)?.GetName) });
                    }
                }

                //PropertyGroups / Regions
                var regionsListResponse = GetRegions(editorPersonaId, userPersonaId, null);
                List<OnSiteRegion> regions = new List<OnSiteRegion>();
                if(regionsListResponse.Records != null)
                {
                    regions = regionsListResponse.Records.Cast<OnSiteRegion>().ToList();
                }

                var removedRegions = userBeforeUpdate.OnSiteUserProfile.Properties.RegionIdList.Except(onSiteUser.Properties.RegionIdList).ToList();
                var addedRegions = onSiteUser.Properties.RegionIdList.Except(userBeforeUpdate.OnSiteUserProfile.Properties.RegionIdList).ToList();

                if (removedRegions.Any())
                {
                    foreach (var p in removedRegions)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "On-Site Property Group", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", regions.Find(f => f.GetRegionId == p).GetRegionName) });
                    }
                }
                if (addedRegions.Any())
                {
                    foreach (var p in addedRegions)
                    {
                        additionalParameters.Add(new AdditionalParameters { Key = "On-Site Property Group", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", regions.Find(f => f.GetRegionId == p).GetRegionName) });
                    }
                }

                return insUpdResult;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageOnSiteUser", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
                return $"Error - {ex.Message}";
            }
        }

        /// <summary>
        /// Updates user profile  
        /// </summary>
        public string UpdateOnSiteUserProfile(long editorPersonaId, long userPersonaId)
        {
            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOnSiteUserProfile", $"Error. editorPersona id - {editorPersonaId}. Reason: {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }

                var persona = _managePersona.GetPersona(userPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                string userEmailAddress = string.Empty;
                if (IsRegularUserNoEmail(userPersonaId))
                {
                    // get the email address
                    var manageElectronicAddress = new ManageElectronicAddress();
                    var addresses = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);

                    if (addresses != null)
                    {
                        if (addresses.Any(
                                a =>
                                    a.AddressType.ToUpper() == "EMAIL"))
                        {
                            userEmailAddress = (from a in addresses
                                                where
                                                    a.AddressType.ToUpper() == "EMAIL"
                                                select a.AddressString).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    userEmailAddress = userLogin.LoginName;
                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;
                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);
                int companyId = Convert.ToInt32(company.CompanyInstanceSourceId);
                OnSiteUserProfileUpdate onSiteUser = new OnSiteUserProfileUpdate
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = userEmailAddress,
                    UserName = null,
                    PhoneNumber = "",
                    IsActive = null,
                    UserId = _productUserId
                };

                // UPDATE USER
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOnSiteUserProfile", $"Trying to UPDATE user Profile. editorPersona id - {editorPersonaId} userPersonaId: {userPersonaId}" });

                // activate user - everytime we have to call activate user before updating user
                // this is because each user can have multiple companies & IsActive in User object has diffrent meaning 
                var activateResult = ActivateDeactivateOnSiteProductUser(company.CompanyInstanceSourceId);

                if (string.IsNullOrEmpty(activateResult))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOnSiteUserProfile", $"Success. userPersonaId: {userPersonaId}" });
                }
                else
                {
                    throw new Exception($"UpdateOnSiteUserProfile.ManageOnSiteUser; error while activating user profile before calling update/ Error contents - {activateResult}");
                }

                var updateResult = UpdateOnSiteProductUserProfile(userPersonaId, editorPersonaId, onSiteUser);
                if (string.IsNullOrEmpty(updateResult))
                {
                    // add activity log
                    WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
                }

                return updateResult;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateOnSiteUserProfile", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
                return $"Error - {ex.Message}";
            }
        }

        /// <summary>
        /// List all users
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUsers(long editorPersonaId, RequestParameter datafilter)
        {
            var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError)
            {
                return claimResponse;
            }

            var response = new ListResponse();
            try
            {

                //int companyInstanceSourceId = 279; // to get sample groups 
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "Url", $"{_apiEndPoint}/users?company_id={companyInstanceSourceId}" } }, messageProperties: new object[] { "GetUsers", "Begin" });

                var allUsers = GetResultFromApi<IList<OnSiteUser>>(_accessToken, $"{_apiEndPoint}/users?company_id={companyInstanceSourceId}");

                if (allUsers == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Error. No users received from product. editorPersona id - {editorPersonaId}" });
                    response.IsError = true;
                    response.ErrorReason = "No Users.";
                    return response;
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUsers", $"Received users from product. editorPersona id - {editorPersonaId}" });
                response.RowsPerPage = 9999;
                response.ErrorReason = string.Empty;
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

                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetUsers", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
            }

            return response;

        }

        #endregion

        #region User-Status

        /// <summary>
        /// Disables the On-Site product users.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="productUserId">The product user Id.</param>
        /// <param name="isDeactivate">if set to <c>false</c> [is active].</param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, string productUserId, bool isDeactivate = true)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error productUserId:{productUserId}. Reason: {listResponse.ErrorReason}" });
                return false;
            }

            // Get Company
            CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

            if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error editorPersona id - {editorPersonaId} Error: Company not found." });
                return false;
            }

            _productUserId = productUserId;
            // activate or deactivate user
            var result = ActivateDeactivateOnSiteProductUser(company.CompanyInstanceSourceId, isDeactivate);

            if (!string.IsNullOrEmpty(result))
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error productUserId:{productUserId}. Reason: {result}" });
                return false;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Success productUserId:{productUserId}" });
            return true;
        }

        #endregion

        #region Migration Tool

        /// <summary>
        /// Get List of On-Site Users for Migration 
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
            var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError)
            {
                response.ErrorReason = claimResponse.ErrorReason;
                return response;
            }

            try
            {

                //int companyInstanceSourceId = 279; // to get sample groups 
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                var filter = "UnMigrated";
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

                var url = $"{_apiEndPoint}/users?company_id={companyInstanceSourceId}&filter={filter}&page={startRow}&per_page={resultPerRow}";
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "Url", url } }, messageProperties: new object[] { "GetMigrationUsers", "Posting to api" });

                var allUsers = GetResultFromApi<IList<OnSiteUser>>(_accessToken, url);

                if (allUsers == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product. editorPersona id - {editorPersonaId}" });
                    return response;
                }

                var migrationUsers = new List<MigrationUser>();
                foreach (var user in allUsers)
                {
                    var migrationUser = new MigrationUser
                    {
                        CompanyInstanceSourceId = companyInstanceSourceId.ToString(),
                        UserId = user.OnSiteUserProfile?.UserId,
                        FirstName = user.OnSiteUserProfile?.FirstName,
                        LastName = user.OnSiteUserProfile?.LastName,
                        Email = user.OnSiteUserProfile?.Email,
                        Username = user.OnSiteUserProfile?.UserName,
                        Status = user.OnSiteUserProfile?.IsActive == true ? "Active" : "Disabled",
                        Phone = user.OnSiteUserProfile?.PhoneNumber,
                        Properties = user.OnSiteUserProfile?.Properties?.PropertyIdList?.Select(p => new MigrationProperty() { PropertyInstanceSourceId = p.ToString() }).ToList()
                    };
                    migrationUsers.Add(migrationUser);
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product. editorPersona id - {editorPersonaId}" });
                response.RowsPerPage = resultPerRow;
                response.ErrorReason = string.Empty;
                response.IsError = false;
                response.TotalPages = 1;
                response.Records = migrationUsers.Cast<object>().ToList();
                response.TotalRows = migrationUsers.Count();
            }
            catch (Exception ex)
            {
                response = new ListResponse
                {
                    IsError = true,
                    ErrorReason = ex.Message
                };

                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetMigrationUsers", $"Error editorPersona id - {editorPersonaId}. Reason: {ex.Message}" });

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
                Status = true
            };

            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError)
            {
                migrateResponse.Message = claimResposnse.ErrorReason;
                return migrateResponse;
            }

            try
            {
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                var onSitemigrateUsers = new OnSiteMigrateUsers();
                var onSiteUnmigrateUsers = new OnSiteMigrateUsers();

                onSitemigrateUsers.Users = migrateUsers.Where(x => x.UsingUnifiedLogin).Select(x => new OnSiteMigrateUser() { UserId = x.UserId }).ToList();
                onSiteUnmigrateUsers.Users = migrateUsers.Where(x => !x.UsingUnifiedLogin).Select(x => new OnSiteMigrateUser() { UserId = x.UserId }).ToList();

                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                if (onSitemigrateUsers.Users.Any())
                {
                    var url = $"{_apiEndPoint}/users/migrate_users";
                    var response = _client.PostAsJsonAsync(url, onSitemigrateUsers).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var logData = new Dictionary<string, object>
                    {
                        { "Url", url },
                        { "Response", responseContent },
                        { "EditorPersonaId", editorPersonaId },
                        { "MigratedUser", onSitemigrateUsers }
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        var migrationResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "Migrate success" });
                        migrateResponse.Message = migrationResponse.count;
                        migrateResponse.Status = migrationResponse.count != 0;
                    }
                    else
                    {
                        WriteToErrorLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "Migrate error" });
                        migrateResponse.Message = "Cannot update user status to migrated.";
                        migrateResponse.Status = false;
                    }
                }

                if (onSiteUnmigrateUsers.Users.Any())
                {
                    var url = $"{_apiEndPoint}/users/unmigrate_users";
                    var response = _client.PostAsJsonAsync(url, onSiteUnmigrateUsers).Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var logData = new Dictionary<string, object>
                    {
                        { "Url", url },
                        { "Response", responseContent },
                        { "EditorPersonaId", editorPersonaId },
                        { "MigratedUser", onSiteUnmigrateUsers }
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        var migrationResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "Unmigrate success" });
                        migrateResponse.Message = $"{migrateResponse.Message} {migrationResponse.count}";
                        migrateResponse.Status = migrateResponse.Status && migrationResponse.count != 0;
                    }
                    else
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "Unmigrate failed" });
                        migrateResponse.Message = "Cannot update user status to unmigrated.";
                        migrateResponse.Status = false;
                    }
                }
            }
            catch (Exception ex)
            {
                migrateResponse = new MigrateResponse
                {
                    Status = false,
                    Message = ex.Message
                };

                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
            }

            return migrateResponse;
        }

        #endregion

        #region Private Methods

        private string GetUserCode(string userLoginName)
        {
            string result = userLoginName;
            if (userLoginName.IndexOf('@') >= 0)
            {
                result = userLoginName.Split('@')[0];
            }

            return result;
        }

        private void GetToken()
        {
            string result = string.Empty;
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Begin" });

                ObjectCache tokenCache = MemoryCache.Default;

                // Get token values from cache
                _accessToken = tokenCache["access_token_1S"] as string;

                if (!string.IsNullOrEmpty(_accessToken)) return;

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "tokenEndpoint", _tokenEndPoint } }, messageProperties: new object[] { "GetToken", "Getting token" });

                dynamic expando = new ExpandoObject();
                expando.grant_type = "client_credentials";
                expando.client_id = _clientId;
                expando.client_secret = _apiSecret;

                using (var client = new HttpClient(_messageHandler, false))
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync(_tokenEndPoint, (object)expando).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                        if (userResult != null)
                        {
                            result = userResult.ToString();
                        }
                    }
                    else
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        dynamic errorResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                        if (errorResult != null)
                        {
                            result = errorResult.ToString();
                        }

                        throw new Exception($"Exception while getting token. {result}");
                    }
                }

                // parse acess token
                if (!string.IsNullOrEmpty(result))
                {
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(result);
                    _accessToken = userResult.access_token.ToString();
                }

                // make sure access token exists
                if (string.IsNullOrEmpty(_accessToken))
                {
                    throw new Exception("Null or empty access token");
                }

                // add token in cache
                var cachePolicy = new CacheItemPolicy
                {
                    // Expire cache every after 9 minutes (assuming 10 min is token expiration time)
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
                };

                tokenCache.Set("access_token_1S", _accessToken, cachePolicy);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Received & populated cache with token value" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", $"Error Reason: {ex.Message}" });
                throw new Exception($"Error in ManageProductOnSite.GetToken- {ex.Message}");
            }
        }

        private T GetResultFromApi<T>(string token, string baseUrlAndQuery, bool throwOnError = true) where T : class
        {
            T results = null;
            using (var client = new HttpClient(_messageHandler, false))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync(baseUrlAndQuery).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
                }
            }

            return results;
        }

        private ListResponse MergeAccessGroupsWithGreenbook(IList<OnSiteRole> allRoles)
        {
            var onSiteUserUser = GetOnSiteUser(_productUsername);

            if (onSiteUserUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeAccessGroupsWithGreenbook", $"Error productUsername {_productUsername} - User not found" });
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            var userOnSiteRoles = onSiteUserUser.OnSiteUserProfile.Roles;
            foreach (var userOnSiteRole in userOnSiteRoles)
            {
                if (allRoles.Any(a => a.Level == userOnSiteRole.Level))
                {
                    OnSiteRole accessGroup = (from a in allRoles
                                              where a.Level == userOnSiteRole.Level
                                              select a).FirstOrDefault();

                    if (accessGroup != null)
                    {
                        accessGroup.IsAssigned = true;
                    }
                    else
                    {
                        accessGroup.IsAssigned = false;
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

        private ListResponse MergePropertiesWithGreenbook(IList<OnSiteProperty> allProperties)
        {
            var onSiteUserUser = GetOnSiteUser(_productUsername);

            if (onSiteUserUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergePropertiesWithGreenbook", $"Error productUsername {_productUsername} - User not found" });
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            var userOnSiteProperties = onSiteUserUser.OnSiteUserProfile.Properties.PropertyIdList;
            var userOnSiteRegions = onSiteUserUser.OnSiteUserProfile.Properties.RegionIdList;

            Dictionary<string, bool> additionalData = new Dictionary<string, bool>();
            if ((userOnSiteProperties == null || userOnSiteProperties.Count == 0) && (userOnSiteRegions == null || userOnSiteRegions.Count == 0))
            {
                additionalData.Add("allProperties", true);
            }
            else
            {
                foreach (var userOnSiteProperty in userOnSiteProperties)
                {
                    if (allProperties.Any(a => a.GetPropertyId == userOnSiteProperty))
                    {
                        OnSiteProperty property = (from a in allProperties
                                                   where a.GetPropertyId == userOnSiteProperty
                                                   select a).FirstOrDefault();

                        if (property != null)
                        {
                            property.IsAssigned = true;
                        }
                    }
                }

                additionalData.Add("allProperties", false);
            }

            if (userOnSiteRegions != null && userOnSiteRegions.Count > 0)
            {
                foreach (var region in userOnSiteRegions)
                {
                    var property = (from a in allProperties
                                    where a.RegionId == region.ToString()
                                    select a);
                    foreach (var prop in property)
                    {
                        prop.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allProperties.Cast<object>().ToList(),
                TotalRows = allProperties.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = additionalData
            };
        }

        private ListResponse MergeRegionsWithGreenbook(IList<OnSiteRegion> allRegions)
        {
            var onSiteUserUser = GetOnSiteUser(_productUsername);

            if (onSiteUserUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeRegionsWithGreenbook", $"Error productUsername {_productUsername} - User not found" });
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            var userOnSiteRegions = onSiteUserUser.OnSiteUserProfile.Properties.RegionIdList;

            Dictionary<string, bool> additionalData = new Dictionary<string, bool>();
            if (userOnSiteRegions == null || userOnSiteRegions.Count == 0)
            {
                additionalData.Add("allRegions", true);
            }
            else
            {
                foreach (var userOnSiteRegion in userOnSiteRegions)
                {
                    if (allRegions.Any(a => a.GetRegionId == userOnSiteRegion))
                    {
                        OnSiteRegion region = (from a in allRegions
                                               where a.GetRegionId == userOnSiteRegion
                                               select a).FirstOrDefault();

                        if (region != null)
                        {
                            region.IsAssigned = true;
                        }
                    }
                }

                additionalData.Add("allRegions", false);
            }

            return new ListResponse()
            {
                Records = allRegions.OrderBy(p => p.GetRegionName).Cast<object>().ToList(),
                TotalRows = allRegions.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = additionalData
            };
        }

        private OnSiteUser GetOnSiteUser(string userName)
        {
            string baseUrlAndQuery = $"{_apiEndPoint}/users/exists?username={userName}";
            return GetResultFromApi<OnSiteUser>(_accessToken, baseUrlAndQuery, false);
        }

        private void CreateProductUserInGreenBook(long userPersonaId, dynamic userResult, string productLoginName, int companyId)
        {
            string newid = userResult.user.user_id;

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUsername", productLoginName }, { "UserId", newid }, { "companyId", companyId } }, messageProperties: new object[] { "CreateProductUserInGreenBook", "Inserting in UnifiedLogi SAML user info" });
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.PMCID, companyId.ToString());

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", "Setting status to Success" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
        }

        private string UpdateOnSiteProductUser(long userPersonaId, long editorPersonaId, OnSiteUserInsertUpdate onSiteUser)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userdata", JsonConvert.SerializeObject(onSiteUser) } }, messageProperties: new object[] { "UpdateOnSiteProductUser", $"Calling product API editorPersona id - {editorPersonaId}" });

                var response = client.PostAsJsonAsync($"{_apiEndPoint}/users/{onSiteUser.UserId}/update", onSiteUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOnSiteProductUser", $"Success editorPersona id - {editorPersonaId}" });

                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        result = string.Empty;
                    }

                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                }
                else
                {
                    string errorContent = string.Empty;
                    try
                    {
                        errorContent = response.Content.ReadAsStringAsync().Result;
                    }
                    catch
                    {
                        /*Ignored*/
                    }

                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOnSiteProductUser", $"Error editorPersona id - {editorPersonaId}. Reason: {errorContent}" });
                    //UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    result = $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
                }
            }

            return result;
        }

        private string UpdateOnSiteProductUserProfile(long userPersonaId, long editorPersonaId, OnSiteUserProfileUpdate onSiteUser)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userdata", JsonConvert.SerializeObject(onSiteUser) } }, messageProperties: new object[] { "UpdateOnSiteProductUserProfile", $"Calling product API editorPersona id - {editorPersonaId}" });

                var response = client.PostAsJsonAsync($"{_apiEndPoint}/users/{onSiteUser.UserId}/update", onSiteUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOnSiteProductUserProfile", $"Success editorPersona id - {editorPersonaId}" });
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        result = string.Empty;
                    }
                    //UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                }
                else
                {
                    string errorContent = string.Empty;
                    try
                    {
                        errorContent = response.Content.ReadAsStringAsync().Result;
                    }
                    catch
                    {
                        /*Ignored*/
                    }

                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateOnSiteProductUserProfile", $"Error editorPersona id - {editorPersonaId}. Reason: {errorContent}" });
                    //UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    result = $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
                }
            }

            return result;
        }

        private string ActivateDeactivateOnSiteProductUser(string companyId, bool isDeactivate = false)
        {
            string result = string.Empty;
            using (var client = new HttpClient(_messageHandler, false))
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUserId", _productUserId }, { "isDeactivate", isDeactivate } }, messageProperties: new object[] { "ActivateDeactivateOnSiteProductUser", "Calling product API" });
                HttpResponseMessage response;

                if (isDeactivate)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUserId", _productUserId }, { "url", $"{_apiEndPoint}/users/{_productUserId}/deactivate?company_id={companyId}" } }, messageProperties: new object[] { "ActivateDeactivateOnSiteProductUser", "Deactivating user" });
                    // deactivate user
                    response = client.PostAsJsonAsync($"{_apiEndPoint}/users/{_productUserId}/deactivate?company_id={companyId}", string.Empty).Result;
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUserId", _productUserId }, { "url", $"{_apiEndPoint}/users/{_productUserId}/reactivate?company_id={companyId}" } }, messageProperties: new object[] { "ActivateDeactivateOnSiteProductUser", "Reactivating user" });
                    // reactivate user
                    response = client.PostAsJsonAsync($"{_apiEndPoint}/users/{_productUserId}/reactivate?company_id={companyId}", string.Empty).Result;
                }

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUserId", _productUserId } }, messageProperties: new object[] { "ActivateDeactivateOnSiteProductUser", "Success" });
                    result = string.Empty;
                }
                else
                {
                    string errorContent = string.Empty;
                    try
                    {
                        errorContent = response.Content.ReadAsStringAsync().Result;
                    }
                    catch
                    {
                        /*Ignored*/
                    }

                    WriteToErrorLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUserId", _productUserId } }, messageProperties: new object[] { "ActivateDeactivateOnSiteProductUser", $"Error. Reason: {errorContent}" });
                    result = $"There was a problem updating the user with productUserId - {_productUserId}. Error-{errorContent}.";
                }
            }

            return result;
        }

        private string InsertOnSiteProductUser(long userPersonaId, long editorPersonaId, string productLoginName, OnSiteUserInsertUpdate onSiteUser, int companyId)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userdata", JsonConvert.SerializeObject(onSiteUser) } }, messageProperties: new object[] { "InsertOnSiteProductUser", $"Calling product API. editorPersona id - {editorPersonaId}" });

                var response = client.PostAsJsonAsync($"{_apiEndPoint}/users", onSiteUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productLoginName", productLoginName } }, messageProperties: new object[] { "InsertOnSiteProductUser", "Success" });
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        // for new user insert record
                        CreateProductUserInGreenBook(userPersonaId, userResult, productLoginName, companyId);
                        result = string.Empty;
                    }
                }
                else
                {
                    string errorContent = string.Empty;
                    try
                    {
                        errorContent = response.Content.ReadAsStringAsync().Result;
                    }
                    catch
                    {
                        /*Ignored*/
                    }

                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "InsertOnSiteProductUser", $"Error editorPersona id - {editorPersonaId} Reason: {errorContent}" });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    result = $"There was a problem creating the user with editorPersona id - {editorPersonaId}. Error-{errorContent}";
                }
            }

            return result;
        }

        private IList<OnSiteRole> MapUserRoles(OnSiteUserPropertyRegionRole userPropertyRegionRole, int companyId)
        {
            IList<OnSiteRole> os = new List<OnSiteRole>();

            foreach (var x in userPropertyRegionRole.RoleList)
            {
                os.Add(new OnSiteRole { Level = x, IsAssigned = null, CompanyId = companyId });
            }

            return os;
        }

        private PropertyAcsess MapUserPropertyAccess(OnSiteUserPropertyRegionRole userPropertyRegionRole, int companyId)
        {
            var ps = new PropertyAcsess
            {
                PropertyIdList = userPropertyRegionRole.PropertyList,
                RegionIdList = userPropertyRegionRole.RegionList
            };

            ps.CompanyIdList = null;

           if (ps.PropertyIdList != null && ps.PropertyIdList.Count > 0 && ps.PropertyIdList[0] == -1)
            {
                ps.CompanyIdList = new List<int> { companyId };
                ps.PropertyIdList = new List<int>();
                ps.RegionIdList = new List<int>();
            }

            return ps;
        }

        #endregion
    }

    public class OnSiteRegion
    {
        private string _name = string.Empty;
        private int _regionId;

        [JsonProperty(PropertyName = "id")] public int GetRegionId => _regionId;

        [JsonProperty(PropertyName = "region_id")]
        public int SetRegionId
        {
            set { this._regionId = value; }
        }

        [JsonProperty(PropertyName = "name")] public string GetRegionName => _name;

        [JsonProperty(PropertyName = "region_name")]
        public string SetRegionName
        {
            set { _name = value; }
        }

        [JsonProperty(PropertyName = "company_id")]
        public int CompanyId { get; set; }

        public bool IsAssigned { get; set; }

    }

    public class OnSiteUserProfile
    {
        [JsonProperty(PropertyName = "user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "user_name", NullValueHandling = NullValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email_address")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "is_active", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

        [JsonProperty(PropertyName = "property_access")]
        public PropertyAcsess Properties { get; set; }

        [JsonProperty(PropertyName = "roles")] public List<OnSiteRole> Roles { get; set; }
    }

    public class OnSiteUser
    {
        [JsonProperty(PropertyName = "user")] public OnSiteUserProfile OnSiteUserProfile { get; set; }
    }

    public class OnSiteUserInsertUpdate
    {
        [JsonProperty(PropertyName = "user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "user_name", NullValueHandling = NullValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email_address")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "is_active", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

        [JsonProperty(PropertyName = "property_access")]
        public PropertyAcsess Properties { get; set; }

        [JsonProperty(PropertyName = "roles")] public IList<OnSiteRole> Roles { get; set; }
    }

    public class OnSiteUserProfileUpdate
    {
        [JsonProperty(PropertyName = "user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "user_name", NullValueHandling = NullValueHandling.Ignore)]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email_address")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "is_active", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

    }

    public class PropertyAcsess
    {
        [JsonProperty(PropertyName = "all_in_company_ids")]
        public List<int> CompanyIdList { get; set; }

        [JsonProperty(PropertyName = "all_in_region_ids")]
        public List<int> RegionIdList { get; set; }

        [JsonProperty(PropertyName = "property_ids")]
        public List<int> PropertyIdList { get; set; }
    }

    public class OnSiteRole
    {
        [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "level", NullValueHandling = NullValueHandling.Ignore)]
        public int Level { get; set; }

        [JsonProperty(PropertyName = "company_id", NullValueHandling = NullValueHandling.Ignore)]
        public int CompanyId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAssigned { get; set; } = false;
    }

    public class OnSiteProperty
    {
        private string _name = string.Empty;
        private int _propertyId;

        [JsonProperty(PropertyName = "id")]
        public int GetPropertyId
        {
            get { return _propertyId; }
        }

        [JsonProperty(PropertyName = "property_id")]
        public int SetPropertyId
        {
            set { this._propertyId = value; }
        }

        [JsonProperty(PropertyName = "name")] public string GetName => _name;

        [JsonProperty(PropertyName = "property_name")]
        public string SetName
        {
            set { this._name = value; }
        }

        [JsonProperty(PropertyName = "state")] public string State { get; set; }

        [JsonProperty(PropertyName = "city")] public string City { get; set; }


        [JsonProperty(PropertyName = "region_id")]
        public string RegionId { get; set; }

        //[JsonIgnore] //Commented for ticket TFS-123011/PME-148190
        [JsonProperty(PropertyName = "active")]
        public bool IsActive { get; set; }

        public bool IsAssigned { get; set; }

        /// <summary>
        /// The UPFM property instance id
        /// </summary>
        public string InstanceId { get; set; }
    }

    public class OnSiteUserPropertyRegionRole
    {
        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> PropertyList { get; set; }

        /// <summary>
        /// A list of roles to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> RoleList { get; set; }

        /// <summary>
        /// A list of regions to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> RegionList { get; set; }

        public bool IsAssigned { get; set; }

    }
}