using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Caching;
using System.Web.Security;
using IdentityModel.Client;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.VendorServices;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ManageProductBase" />
    /// <seealso cref="IManageProductVendorServices" />
    public class ManageProductVendorServices : ManageProductBase, IManageProductVendorServices
    {
        #region Private members

        private IProductInternalSettingRepository _productInternalSettingRepository;
        private string _clientId;
        private string _apiEndPoint;
        private string _apiSecret;
        private string _accessToken;
        private string _tokenIssueUri;
        private TokenClient _tokenClient;
        private DefaultUserClaim _userClaims;

        #endregion

        #region Ctor

        /// <summary>
		/// Ctor
		/// </summary>
		/// <param name="userClaims"></param>
        public ManageProductVendorServices(DefaultUserClaim userClaims) : base((int)ProductEnum.VendorServices, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductVendorServices", "Ctor - Getting Product settings." });
#endif
            _productId = (int)ProductEnum.VendorServices;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _editorRealPageId = userClaims.UserRealPageGuid;
            _userClaims = userClaims;

            _blueBook = new ManageBlueBook(userClaims);

            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value; //"http://web2012.compliancedepot.com/vcapi"; //
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value; //"AF6977FB-8BCE-43BD-B715-2DDC1E5A6009";
            _clientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value; //"vendorcompliance";
            _tokenIssueUri = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENENDPOINT").Value; //"http://web2012.compliancedepot.com/vcapi"; //
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductVendorServices", "Ctor - Received Product settings; getting token." });
#endif
            _tokenClient = new TokenClient($"{_tokenIssueUri}", _clientId, _apiSecret);

            GetToken();
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaims"></param>
        /// <param name="httpMessageHandler"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="samlRepository"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="repository"></param>
        public ManageProductVendorServices(Guid editorRealPageId, DefaultUserClaim userClaims, HttpMessageHandler httpMessageHandler, IProductInternalSettingRepository productInternalSettingRepository,
            IManagePersona managePersona, ISamlRepository samlRepository, IManageBlueBook manageBlueBook, IProductRepository productRepository, IRepository repository)
            : base((int)ProductEnum.VendorServices, userClaims, repository, httpMessageHandler)
        {
            _editorRealPageId = editorRealPageId;

            _blueBook = manageBlueBook;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _userClaims = userClaims;
            _productRepository = productRepository;
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value; //"http://web2012.compliancedepot.com/vcapi"; //
            _apiSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "APISECRET").Value; //"AF6977FB-8BCE-43BD-B715-2DDC1E5A6009";
            _clientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "CLIENTID").Value; //"vendorcompliance";
            _tokenIssueUri = _productInternalSettingList.First(a => a.Name.ToUpper() == "TOKENENDPOINT").Value;
#if DEBUG
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageProductVendorServices", "Ctor - Received Product settings; getting token." });
#endif
            _tokenClient = new TokenClient($"{_tokenIssueUri}", _clientId, _apiSecret, httpMessageHandler);

            GetToken();
            _client = new HttpClient(httpMessageHandler, false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get Property Groups
        /// </summary> 
        public ListResponse GetPropertyGroups(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            var response = new ListResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Begining of method for user with editorPersona id - {editorPersonaId}" });

            try
            {
                response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);//TODO:need to refactor
                if (response.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {response.ErrorReason}" });
                    return response;
                }

                //int companyInstanceSourceId = 10201; // to get sample groups 
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Getting product groups for user with editorPersona id - {editorPersonaId} and companyInstanceSourceId{companyInstanceSourceId}" });

                var groups =
                    GetDivisions(companyInstanceSourceId)
                        .Concat(GetRegions(companyInstanceSourceId))
                        .Concat(GetOwnershipGroups(companyInstanceSourceId));


                var allPropertyGroups = groups as IList<VendorServicesPropertyGroup> ?? groups.ToList();


                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Received product groups with count {allPropertyGroups.Count}for user with editorPersona id - {editorPersonaId} and companyInstanceSourceId{companyInstanceSourceId}" });

                // Check for edit
                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0))//update existing user
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Calling MergeProductGroupsWithGreenbook for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeProductGroupsWithGreenbook(allPropertyGroups);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"MergeProductGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId}." });
                }
                else
                {
                    response = new ListResponse()
                    {
                        Records = allPropertyGroups.Cast<object>().ToList(),
                        TotalRows = allPropertyGroups.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }
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
                    response.ErrorReason = CommonMessageConstants.PropertyGroupErrorMessage;
                }

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetPropertyGroups", $"Error for user with editorPersona id - {editorPersonaId} - {response.ErrorReason}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Used to get properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            var result = new ListResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Begining of method for user with editorPersona id - {editorPersonaId}" });

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return result;
                }

                int companyInstanceId = GetProductCompanyInstanceId(_udmSourceCode, useTranslate:false).CompanyInstanceId;
                
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetProductCompanyInstanceId - Found blue book company instance id - {companyInstanceId}  for user editorPersona id -{editorPersonaId}" });


                CompanyPropertyRootObject CompanyProperties = _blueBook.GetCompanyPropertyInstance(companyInstanceId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"GetPropertyInstance - Found total {CompanyProperties.data.attributes.getCompanyPropertyInstances.Count} properties with blue book company instance id {companyInstanceId} for user with editorPersona id - {editorPersonaId}." });

                IList<ProductProperty> blueBookPropertyList = CompanyProperties.MapBlueBookToGBProperties() ?? new List<ProductProperty>();
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MapBlueBookToGBProperties completed for user with editorPersona id -{editorPersonaId}." });

                // need to do a filter on the result
                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"Calling MergeProductPropertiesWithGreenbook for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    result = MergeProductPropertiesWithGreenbook(blueBookPropertyList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}." });
                }
                else
                {
                    result = new ListResponse() // Called during creating a new User
                    {
                        Records = blueBookPropertyList.Cast<object>().ToList(),
                        TotalRows = blueBookPropertyList.Count,
                        RowsPerPage = blueBookPropertyList.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}." }, exception: ex);
                result = new ListResponse
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    result.ErrorReason = ex.Message;
                }
                else
                {
                    result.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns Roles (User Access Groups in Vendor Credentialing)
        /// </summary>
        public ListResponse GetRoles(long editorPersonaId, long userPersonaId, AccessType accessType, RequestParameter datafilter)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            var response = new ListResponse();
            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" } );
                    return result;
                }
                // get access groups from Vendor Credentialing product
                var allUserAccessGroups = GetUserAccessGroupsByAcessType(accessType);

                if (allUserAccessGroups == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"No access groups (roles) received from product for user with editorPersona id - {editorPersonaId}." });

                    response.IsError = true;
                    response.ErrorReason = "No User Access groups (roles) received from product.";
                    return response;
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Beginning of method MapProductAccessGroupsToGB for user with editorPersona id - {editorPersonaId}" });

                // Map Product roles to GB Roles
                var gbRoles = MapProductAccessGroupsToGB(allUserAccessGroups)?.OrderBy(x => x.Name).ToList();

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MapProductAccessGroupsToGB completed for user with editorPersona id - {editorPersonaId}" });

                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0)) // Called during updating Existing User
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeAccessGroupsWithGreenbook calling for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                    response = MergeAccessGroupsWithGreenbook(gbRoles);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"MergeAccessGroupsWithGreenbook completed for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}." });
                }
                else // Called during creating a new User
                {
                    response = new ListResponse()
                    {
                        Records = gbRoles.Cast<object>().ToList(),
                        TotalRows = gbRoles.Count(),
                        RowsPerPage = 9999,
                        ErrorReason = string.Empty,
                        TotalPages = 1
                    };
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Exiting method with total rows - {allUserAccessGroups.Count} for user with editorPersona id - {editorPersonaId}." });
            }
            catch (Exception ex)
            {
                response.IsError = true;
                response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetRoles", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
            }

            return response;
        }

        /// <summary>
        /// Get Notification Settings
        /// </summary>
        public Notification GetNotificationSettings(long editorPersonaId, long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetNotificationSettings", $"Beginning of method for user with editorPersonaId id - {editorPersonaId}" });
            var response = new Notification();

            try
            {
                ListResponse result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO:need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetNotificationSettings", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}" });
                    return null;
                }

                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0))
                {
                    VendorServicesUser vendorServicesUser = GetVendorServicesUser();

                    if (vendorServicesUser == null)
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetNotificationSettings", $"Error for user {_productUserId} - User not found." });
                        return null;
                    }

                    response.IsInsuranceExpired = vendorServicesUser.EMailNotifyInsurance;
                    response.IsVendorRecommendationChanges = vendorServicesUser.EMailNotifyRecommendation;
                    response.IsVendorNotLinkedToAnyProperty = vendorServicesUser.EMailNotifyVendorNotLinkedToAnyProperty;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetNotificationSettings", $"Error." }, exception: ex);
                return null;
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
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"userPersonaId:{userPersonaId}" });

            var result = DisableProductUser();
            if (string.IsNullOrEmpty(result))
            {
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
            }

            return result;
        }

        /// <summary>
        /// Update Vendor Compliance User Profile
        /// </summary> 
        public string UpdateVendorServicesUserProfile(long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesUserProfile", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                return listResponse.ErrorReason;
            }

            var persona = _managePersona.GetPersona(userPersonaId);
            var realPageId = persona.RealPageId;
            var person = _managePerson.GetPerson(realPageId);
            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

            // get the email address
            var manageElectronicAddress = new ManageElectronicAddress();
            var addresses = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);

            string userEmailAddress = addresses?.FirstOrDefault()?.AddressString;

            if (string.IsNullOrEmpty(userEmailAddress))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesUserProfile", $"No email address for user with editorPersona id - {editorPersonaId}; assigning bogus email." });

                userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesUserProfile", $"Updating user with persona id {userPersonaId}." });

            // update user profile

            dynamic userObj = new ExpandoObject();
            userObj.Username = _productUsername;
            userObj.FirstName = person.FirstName;
            userObj.LastName = person.LastName;
            userObj.Email = userEmailAddress;

            var apiUrl = $"{_apiEndPoint}/api/Users/";

            using (var client = new HttpClient())
            {

                var content = new ObjectContent<dynamic>(userObj, new JsonMediaTypeFormatter());
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesUserProfile", "JSON input" }, logData: new Dictionary<string, object> { { "JSON response", JsonConvert.SerializeObject(userObj) } });

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl) { Content = content };

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesUserProfile", $"IsSuccessStatusCode return true for user with _productUserId - {_productUserId}." });
                    WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
                    return string.Empty;
                }

                string errorContent = string.Empty;
                try
                {
                    errorContent = response.Content.ReadAsStringAsync().Result;
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesUserProfile", $"ErrorContent= {errorContent}" });
                }
                catch
                {
                    /*Ignored*/
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesUserProfile", "ErrorContent=Ignored" });
                }

                return $"Error in ManageProductVendorServices.UpdateVendorServicesUserProfile; errorContent= {errorContent}";
            }
        }

        /// <summary>
        /// Change user type 
        /// </summary>
        public string ChangeVendorServiceUserType(long createUserPersonaId, long assignUserPersonaId, UserProductPropertyNotification rpList, BatchProcessType batchProcessType)
        {
            return ManageVendorServicesUser(createUserPersonaId, assignUserPersonaId, rpList, batchProcessType);
        }

        /// <summary>
        /// Updated to create/update a user in Vendor Credentialing  
        /// </summary>
        public string ManageVendorServicesUser(long editorPersonaId, long productUserPersonaId, UserProductPropertyNotification userProductPropertyNotification, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Begin create/update user for user with editorPersona id - {editorPersonaId}." });

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, productUserPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}" });
                    return listResponse.ErrorReason;
                }


                var persona = _managePersona.GetPersona(productUserPersonaId);
                var realPageId = persona.RealPageId;
                var person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                // get the email address
                string userEmailAddress = string.Empty;
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

                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"No email address for user with editorPersona id - {editorPersonaId}; assigning bogus email." });

                    userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
                }

                // super user
                if (IsSuperUser(productUserPersonaId))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"New user is Super user with editorPersona id - {editorPersonaId}." });

                    userProductPropertyNotification = new UserProductPropertyNotification
                    {
                        PropertyList = new List<string> { "-1" }, // This will select 'Client; as access level
                        PropertyGroup = null,
                        RoleList = new List<string>()
                    };

                    // get access groups from Vendor Credentialing product
                    var allUserAccessGroups = GetUserAccessGroupsByAcessType(AccessType.Client, true);
                    List<string> list = new List<string>() { "User", "CliVndOnly", "CliVndRO" };

                    if (allUserAccessGroups != null)
                    {
                        foreach (var accGrp in allUserAccessGroups)
                        {
                            if (!list.Contains(accGrp.AccessGroupCode)) 
                            {
                                userProductPropertyNotification.RoleList.Add(accGrp.AccessGroupCode);
                            }
                           
                        }
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"New user is Super user & added {allUserAccessGroups.Count} roles with editorPersona id - {editorPersonaId}." });
                    }
                    else
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"AllUserAccessGroups null for user with editorPersonaId - {editorPersonaId}." });
                    }
                }

                var newproductUsername = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;
                var productLoginName = newproductUsername;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"_productUsername for user is {_productUsername}." });

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Error for user with editorPersona id - {editorPersonaId} Error - Company not found." });
                    return "Company Setup Error: Please Contact Support.";
                }

                // Check for user locations
                List<UserLocation> userLocations = null;
                List<UserAccessGroup> userAccessGroups = null;
                string accessLevel = null;
                int? propertyGroupId = null;
                var vendorServicesUser = new VendorServicesUser();

                if (userProductPropertyNotification != null)
                {
                    // map UserProductPropertyNotification to ProductPropertyNotification
                    var productPropertyNotification = MapGbObjectToProduct(userProductPropertyNotification);

                    if (productPropertyNotification.PropertyList != null &&
                        productPropertyNotification.PropertyList.Count > 0)
                    {
                        userLocations = productPropertyNotification.PropertyList;
                        accessLevel = AccessTypeEnum.Property.ToString();
                    }

                    if (productPropertyNotification.UserAccessGroups != null &&
                        productPropertyNotification.UserAccessGroups.Count > 0)
                    {
                        userAccessGroups = productPropertyNotification.UserAccessGroups;
                    }

                    if (productPropertyNotification.PropertyGroup != null)
                    {
                        propertyGroupId = productPropertyNotification.PropertyGroup.Id;
                        accessLevel = productPropertyNotification.PropertyGroup.Type.ToString();
                    }

                    vendorServicesUser = new VendorServicesUser
                    {
                        Username = userLogin.LoginName,
                        Password = Membership.GeneratePassword(15, 5),
                        CompanyId = company.CompanyInstanceSourceId,
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        Email = userEmailAddress,
                        UserAccessGroups = userAccessGroups,
                        UserCode = GetUserCode(userLogin.LoginName),
                        UserLocations = userLocations,
                        AccessLevel = accessLevel,
                        CompanyDivisionId = propertyGroupId,
                        EMailNotifyInsurance = productPropertyNotification.Notification.IsInsuranceExpired,
                        EMailNotifyRecommendation = productPropertyNotification.Notification.IsVendorRecommendationChanges,
                        EMailNotifyVendorNotLinkedToAnyProperty = productPropertyNotification.Notification.IsVendorNotLinkedToAnyProperty
                    };
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Json to call product API for user with editorPersona id - {editorPersonaId} - {JsonConvert.SerializeObject(vendorServicesUser)}" });

                if (string.IsNullOrEmpty(_productUsername)) // NEW USER
                {
                    // check if user name exists in product
                    if (!string.IsNullOrEmpty(productLoginName))
                    {
                        bool foundNewUserName = false;
                        int incrementor = 0;
                        string updatedproductUsername = $"{person.FirstName.TrimWhiteSpace().Substring(0, 1)}" +
                                        $"{person.LastName.TrimWhiteSpace()}".ToLower();
                        while (!foundNewUserName)
                        {
                            var result = IsUsernameAvailable(productLoginName);
                            if (result == false)
                            {
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"User {productLoginName} already exists in Vendor Credentialing product with editorPersona id -{editorPersonaId}. Getting new one." });
                                incrementor++;
                                if (incrementor == 1)
                                    productLoginName = $"{updatedproductUsername}{productUserPersonaId}";
                                else
                                    productLoginName = $"{updatedproductUsername}{productUserPersonaId}{incrementor}";
                            }
                            else if (result == true)
                            {
                                foundNewUserName = true;
                            }
                            else
                            {
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Unable to validate IsUserNameAvailable method for user {productLoginName}. User creation will not proceed." });
                                return $"Error - Invalid username {productLoginName}";
                            }
                        }
                        // Product username cannot be more than 50 characters
                        if (productLoginName.Length > 50)
                            productLoginName = productLoginName.Substring(1, 50);
                        vendorServicesUser.Username = productLoginName;
                    }

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Trying to CREATE user with editorPersona id - {editorPersonaId}." });
                    string insertResult = InsertVendorServicesProductUser($"{_apiEndPoint}/api/Users", productUserPersonaId, editorPersonaId, productLoginName, vendorServicesUser);

                    return insertResult;
                }
                // UPDATE USER
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Trying to UPDATE user with editorPersona id - {editorPersonaId}." });
                vendorServicesUser.ID = _productUserId;
                vendorServicesUser.Username = _productUsername;

                var updateResult = UpdateVendorServicesProductUser($"{_apiEndPoint}/api/Users", productUserPersonaId, editorPersonaId, vendorServicesUser);

                if (string.IsNullOrEmpty(updateResult))
                {
                    if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                    {
                        WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, batchProcessType);
                    }
                }

                return updateResult;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageVendorServicesUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                return $"Error - {ex.Message}";
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
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError) { return false; }

            try
            {
                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    return false;
                }
                _productUserId = productUserId;
                _productUsername = username;

                //Note : User Active means is not locked in vendor service.
                DisableProductUser(!isActive);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {username} by editorPersonaId = {editorPersonaId}" }, exception: ex);
                return false;
            }

            return true;
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

            try
            {

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    response.ErrorReason = "Company Setup Error: Please Contact Support.";
                    return response;
                }
                var isMigrated = false;
                var startRow = 1;
                var resultPerRow = 1000;
                if (datafilter != null)
                {
                    isMigrated = datafilter.FilterBy.ContainsKey("filter") && datafilter.FilterBy["filter"].ToUpper() == "MIGRATED";
                    if (datafilter.Pages != null)
                    {
                        startRow = datafilter.Pages.StartRow;
                        resultPerRow = datafilter.Pages.ResultsPerPage;
                    }
                }

                var url = $"{_apiEndPoint}/api/users?companyId={companyInstanceSourceId}&isMigrated={isMigrated}&startRow={startRow}&resultsPerPage={resultPerRow}";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Get non migrated users list" }, logData: new Dictionary<string, object> { { "Url", url } });

                var allUsers = GetResultFromApi<IList<VendorServicesUser>>(_accessToken, url);

                if (allUsers == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                    return response;
                }

                var migrationUsers = new List<MigrationUser>();
                foreach (var user in allUsers)
                {
                    var migrationUser = new MigrationUser();
                    migrationUser.CompanyInstanceSourceId = user.CompanyId;
                    migrationUser.FirstName = user.FirstName;
                    migrationUser.LastName = user.LastName;
                    migrationUser.UserId = user.ID;
                    migrationUser.Username = user.Username;
                    migrationUser.Email = user.Email;
                    migrationUser.Phone = user.Phone;
                    migrationUser.LastActivity = user.LastLoginDate.ToString();
                    migrationUser.Status = user.Locked ? "Disabled" : "Active";
                    if (user.UserLocations != null)
                    {
                        foreach (var userLocation in user.UserLocations)
                        {
                            migrationUser.Properties.Add(new MigrationProperty() { PropertyInstanceSourceId = userLocation.PropertyId });
                        }
                    }
                    migrationUsers.Add(migrationUser);
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}." });
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

                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
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

            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError) { migrateResponse.Message = claimResposnse.ErrorReason; return migrateResponse; }

            try
            {

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                    return migrateResponse;
                }

                var vendorServiceMigrateUsers = migrateUsers.Select(migrateUser => new VendorServiceMigrateUser
                {
                    CompanyId = companyInstanceSourceId.ToString(),
                    Id = migrateUser.UserId,
                    UnifiedLoginUserName = migrateUser.UnifiedLoginUserName,
                    UsingUnifiedLogin = migrateUser.UsingUnifiedLogin
                });

                _client.DefaultRequestHeaders.Clear();
                _client.SetBearerToken(_accessToken);

                var url = $"{_apiEndPoint}/api/users/migrateusers";
                var response = _client.PutAsJsonAsync(url, vendorServiceMigrateUsers).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                var logData = new Dictionary<string, object>
                {
                    { "Url", url },
                    { "Response", responseContent },
                    { "EditorPersonaId", editorPersonaId },
                    { "MigratedUser", vendorServiceMigrateUsers }
                };
                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync success" }, logData: logData);
                    migrateResponse.Message = responseContent;
                    migrateResponse.Status = true;
                    return migrateResponse;
                }
                else
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"PostAsJsonAsync Failed" }, logData: logData);
                    migrateResponse.Message = "Cannot update user status to migrated.";
                    return migrateResponse;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                return new MigrateResponse
                { 
                    Status = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region Private Methods

        private IList<VendorServicesPropertyGroup> GetOwnershipGroups(long organizationId)
        {
            IList<VendorServicesPropertyGroup> propGroups = new List<VendorServicesPropertyGroup>();

            string baseUrlAndQuery = $"{_apiEndPoint}/api/OwnershipGroups?companyId={organizationId}";
            var result = GetResultFromApi<IList<dynamic>>(_accessToken, baseUrlAndQuery, false);

            if (result != null)
            {
                foreach (var x in result)
                {
                    propGroups.Add(new VendorServicesPropertyGroup { PropertyGroupId = x.ID, Name = x.OwnershipGroupName, AccessLevel = "Ownergroup" });
                }
            }

            return propGroups;
        }

        private IList<VendorServicesPropertyGroup> GetRegions(long organizationId)
        {
            IList<VendorServicesPropertyGroup> propGroups = new List<VendorServicesPropertyGroup>();

            string baseUrlAndQuery = $"{_apiEndPoint}/api/Regions?companyId={organizationId}";
            var result = GetResultFromApi<IList<dynamic>>(_accessToken, baseUrlAndQuery, false);

            if (result != null)
            {
                foreach (var x in result)
                {
                    propGroups.Add(new VendorServicesPropertyGroup { PropertyGroupId = x.ID, Name = x.RegionName, AccessLevel = "Region" });
                }
            }

            return propGroups;
        }

        private IList<VendorServicesPropertyGroup> GetDivisions(long organizationId)
        {
            IList<VendorServicesPropertyGroup> propGroups = new List<VendorServicesPropertyGroup>();

            string baseUrlAndQuery = $"{_apiEndPoint}/api/Divisions?companyId={organizationId}";
            var result = GetResultFromApi<IList<dynamic>>(_accessToken, baseUrlAndQuery, false);

            if (result != null)
            {
                foreach (var x in result)
                {
                    propGroups.Add(new VendorServicesPropertyGroup { PropertyGroupId = x.ID, Name = x.DivisionName, AccessLevel = "Division" });
                }
            }

            return propGroups;
        }

        private VendorServicesUser GetVendorServicesUser()
        {
            string baseUrlAndQuery = $"{_apiEndPoint}/api/Users/{_productUserId}";
            return GetResultFromApi<VendorServicesUser>(_accessToken, baseUrlAndQuery, false);
        }
        private bool? IsUsernameAvailable(string userName)
        {
            string baseUrlAndQuery = $"{_apiEndPoint}/api/Users/IsUsernameAvailable/{userName}/";
            var result = GetResultFromApi<dynamic>(_accessToken, baseUrlAndQuery, false);
            return result;
        }

        private void GetToken()
        {
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Beginning of the method." });

                ObjectCache tokenCache = MemoryCache.Default;

                // Get token values from cache
                _accessToken = tokenCache["access_token_VC"] as string;

                if (string.IsNullOrEmpty(_accessToken))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Null cache value. Getting new token." });

                    var tokenResponse = _tokenClient.RequestClientCredentialsAsync(_clientId).Result;

                    if (tokenResponse.IsError)
                    {
                        throw new Exception($"ManageProductVendorServices.GetToken - Received null or empty token. {tokenResponse.Error}");
                    }

                    var cachePolicy = new CacheItemPolicy
                    {
                        // Expier cache every after 9 minutes (assuming 10 min is token expiration time)
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(9)
                    };

                    _accessToken = tokenResponse.AccessToken;

                    tokenCache.Set("access_token_VC", _accessToken, cachePolicy);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", "Received & populated cache with token value." });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetToken", $"Error in Getting token- {ex.Message}" });
                throw new Exception($"Error in ManageProductVendorServices.GetToken- {ex.Message}");
            }
        }

        private ProductPropertyNotification MapGbObjectToProduct(UserProductPropertyNotification userProductPropertyNotification)
        {
            var result = new ProductPropertyNotification
            {
                Notification = new Notification
                {
                    IsVendorRecommendationChanges = userProductPropertyNotification.IsVendorRecommendationChanges,
                    IsInsuranceExpired = userProductPropertyNotification.IsInsuranceExpired,
                    IsVendorNotLinkedToAnyProperty = userProductPropertyNotification.IsVendorNotLinkedToAnyProperty
                }
            };

            // TODO : Change to for loop
            if (userProductPropertyNotification.PropertyGroup != null && userProductPropertyNotification.PropertyGroup.Count > 0)
            {
                var propertyGroup = userProductPropertyNotification.PropertyGroup[0];
                result.PropertyGroup = new PropertyGroup
                {
                    Id = propertyGroup.Id,
                    Type = propertyGroup.Type
                };
            }

            if (userProductPropertyNotification.PropertyList != null &&
                userProductPropertyNotification.PropertyList.Count > 0)
            {
                result.PropertyList = new List<UserLocation>();
                foreach (var propId in userProductPropertyNotification.PropertyList)
                {
                    if (propId == "-1")
                    {
                        userProductPropertyNotification.PropertyList = null;
                        result.PropertyGroup = new PropertyGroup
                        {
                            Type = AccessTypeEnum.Client
                        };
                        break;
                    }

                    result.PropertyList.Add(new UserLocation { PropertyId = propId });
                }
            }

            if (userProductPropertyNotification.RoleList != null && userProductPropertyNotification.RoleList.Count > 0)
            {
                result.UserAccessGroups = new List<UserAccessGroup>();
                foreach (var roleId in userProductPropertyNotification.RoleList)
                {
                    result.UserAccessGroups.Add(new UserAccessGroup { AccessGroupCode = roleId });
                }
            }

            return result;
        }

        private T GetResultFromApi<T>(string token, string baseUrlAndQuery, bool throwOnError = true) where T : class
        {
            T results = null;
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = _client.GetAsync(baseUrlAndQuery).Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
            }
            return results;
        }

        private ListResponse MergeAccessGroupsWithGreenbook(IList<ProductRole> allProductRoles)
        {
            VendorServicesUser vendorServicesUser = GetVendorServicesUser();

            if (vendorServicesUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeAccessGroupsWithGreenbook", $"Vendor Credentialing GetRoles error for user {_productUserId} - User not found." });
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            var userAccessGroups = vendorServicesUser.UserAccessGroups;
            foreach (var userAccessGroup in userAccessGroups)
            {
                if (allProductRoles.Any(a => a.ID == userAccessGroup.AccessGroupCode))
                {
                    ProductRole accessGroup = (from a in allProductRoles
                                               where a.ID == userAccessGroup.AccessGroupCode
                                               select a).FirstOrDefault();
                    if (accessGroup != null)
                    {
                        accessGroup.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = allProductRoles.Cast<object>().ToList(),
                TotalRows = allProductRoles.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private ListResponse MergeProductPropertiesWithGreenbook(IList<ProductProperty> blueBookPropertyList)
        {
            // merge the given user details with the list
            VendorServicesUser vendorServicesUser = GetVendorServicesUser();
            if (vendorServicesUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductPropertiesWithGreenbook", $"Error for user {_productUserId} - User not found." });
                return new ListResponse() { IsError = true, ErrorReason = "User not found" };
            }

            // if a user record exists
            var vendorServicesUserLocations = vendorServicesUser.UserLocations;
            foreach (var vendorServicesUserLocation in vendorServicesUserLocations)
            {
                if (blueBookPropertyList.Any(a => a.ID == vendorServicesUserLocation.PropertyId.ToString()))
                {
                    ProductProperty pp = (from a in blueBookPropertyList
                                          where a.ID == vendorServicesUserLocation.PropertyId
                                          select a).FirstOrDefault();
                    if (pp != null)
                    {
                        pp.IsAssigned = true;
                    }
                }
            }

            return new ListResponse()
            {
                Records = blueBookPropertyList.Cast<object>().ToList(),
                TotalRows = blueBookPropertyList.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1
            };
        }

        private ListResponse MergeProductGroupsWithGreenbook(IList<VendorServicesPropertyGroup> allPropertyGroups)
        {
            var accessType = new Dictionary<string, string>();

            VendorServicesUser vendorServicesUser = GetVendorServicesUser();

            if (vendorServicesUser == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductGroupsWithGreenbook", $"Error for user {_productUserId} - User not found." });
                return new ListResponse() { IsError = true, ErrorReason = "User not found." };
            }

            // if a user record exists
            var userAccessLevel = vendorServicesUser.AccessLevel;
            var propGroupId = vendorServicesUser.CompanyDivisionId;

            if (userAccessLevel == "Client")
            {
                accessType.Add("accessType", "allProperties");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductGroupsWithGreenbook", $"AccessType - allProperties" });
            }
            else if (propGroupId != null && propGroupId != 0)
            {
                accessType.Add("accessType", "propertyGroup");
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductGroupsWithGreenbook", $"AccessType - propertyGroup" });

                foreach (var propGroup in allPropertyGroups)
                {
                    if (userAccessLevel == "Division" && propGroupId == propGroup.PropertyGroupId)
                        propGroup.IsAssigned = true;
                    else if (userAccessLevel == "Region" && propGroupId == propGroup.PropertyGroupId)
                        propGroup.IsAssigned = true;
                    else if ((userAccessLevel.ToUpper().Trim() == "OWNERSHIP" || userAccessLevel == "Ownergroup") && propGroupId == propGroup.PropertyGroupId)
                        propGroup.IsAssigned = true;
                }
            }
            else
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "MergeProductGroupsWithGreenbook", $"AccessType - specificProperties" });
                accessType.Add("accessType", "specificProperties");
            }

            return new ListResponse()
            {
                Records = allPropertyGroups.Cast<object>().ToList(),
                TotalRows = allPropertyGroups.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = accessType
            };
        }

        private string CreateProductUserInGreenBook(long userPersonaId, dynamic userResult, string productLoginName)
        {
            string newid = userResult.ID;

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", $"Inserting in GB -productUsername -{productLoginName} and userId {newid}." });
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newid);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CreateProductUserInGreenBook", "Create user Success. Set product status to Success" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

            return newid;
        }

        private IList<ProductRole> MapProductAccessGroupsToGB(IList<UserAccessGroup> allUserAccessGroups)
        {
            var productList = new List<ProductRole>();

            foreach (var userAccessGroup in allUserAccessGroups)
            {
                productList.Add(new ProductRole { ID = userAccessGroup.AccessGroupCode, Description = userAccessGroup.Description, Name = userAccessGroup.AccessGroupName });
            }

            return productList;
        }

        private string GetUserCode(string userLoginName)
        {
            string result = userLoginName;
            if (userLoginName.IndexOf('@') >= 0)
            {
                result = userLoginName.Split('@')[0];
            }

            return result;
        }

        private string InsertVendorServicesProductUser(string productApiUrl, long userPersonaId, long editorPersonaId,
            string productLoginName, VendorServicesUser vendorServicesUser)//TODO : refactor this
        {
            string result = "Error";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertVendorServicesProductUser", $"Calling product API for user with editorPersona id - {editorPersonaId}." });

                var response = client.PostAsJsonAsync(productApiUrl, vendorServicesUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertVendorServicesProductUser", $"IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}." });
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        // for new user insert record
                        string newId = CreateProductUserInGreenBook(userPersonaId, userResult, productLoginName);

                        // Update UL flag in product
                        if (!string.IsNullOrEmpty(newId))
                        {
                            var updateResponse = UpdateUsersMigrationStatus(editorPersonaId, new List<MigrateUser>
                                {
                                    new MigrateUser
                                    {
                                        UserId = newId,
                                        UnifiedLoginUserName = vendorServicesUser.Username, UsingUnifiedLogin = true
                                    }
                                });

                            if (!updateResponse.Status)
                                result = updateResponse.Message;
                        }

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
                    catch {/*Ignored*/ }

                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "InsertVendorServicesProductUser", $"Error for user with editorPersona id- {editorPersonaId} Error - {errorContent}." });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    result = $"There was a problem creating the user with editorPersona id - {editorPersonaId}. Error-{errorContent}";
                }
            }

            return result;
        }

        private string UpdateVendorServicesProductUser(string productApiUrl, long userPersonaId, long editorPersonaId,
              VendorServicesUser vendorServicesUser)//TODO : refactor this
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesProductUser", $"Calling product API for user with editorPersona id - {editorPersonaId}." });

                vendorServicesUser.Password = Guid.NewGuid().ToString();

                var response = client.PutAsJsonAsync(productApiUrl, vendorServicesUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesProductUser", $"IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}." });

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
                    catch {/*Ignored*/ }

                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateVendorServicesProductUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {errorContent}." });
                    //UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    result = $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
                }
            }

            return result;
        }

        private List<UserAccessGroup> GetUserAccessGroupsByAcessType(AccessType accessType, bool isSuperUser = false)
        {
            int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
            string rolesEndpoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "GETROLEENDPOINT").Value;

            // get access groups from Vendor Credentialing product
            var allUserAccessGroups = GetResultFromApi<List<UserAccessGroup>>(_accessToken,
                $"{_apiEndPoint}/{string.Format(rolesEndpoint, companyInstanceSourceId)}", false);

            return allUserAccessGroups;
        }

        private string DisableProductUser(bool isActive = false)
        {
            dynamic userObj = new ExpandoObject();
            userObj.username = _productUsername;
            userObj.locked = isActive;

            var apiUrl = $"{_apiEndPoint}/api/Users/";

            using (var client = new HttpClient())
            {
                var content = new ObjectContent<dynamic>(userObj, new JsonMediaTypeFormatter());
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl) { Content = content };

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DisableProductUser", $"IsSuccessStatusCode return true for user with _productUserId - {_productUserId}." });
                    return string.Empty;
                }

                string errorContent = string.Empty;
                try
                {
                    errorContent = response.Content.ReadAsStringAsync().Result;
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DisableProductUser", $"ErrorContent= {errorContent}" });
                }
                catch
                {
                    /*Ignored*/
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DisableProductUser", "ErrorContent=Ignored" });
                }

                return $"Error in ManageProductVendorServices.DisableProductUser; errorContent= {errorContent}";
            }
        }

        #endregion
    }

    public class VendorServicesPropertyGroup
    {
        public int? PropertyGroupId { get; set; }
        public string Name { get; set; }
        public string AccessLevel { get; set; }
        public bool IsAssigned { get; set; }
    }
}