using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProspectContactCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    public class ManageProductProspectContact : ManageProductBase, IManageProductProspectContact
    {

        #region Private Variables

        private static string _apiEndPoint; //= "http://qalb.levelone.com/UserManagementAPI";
        private ProductRepository _productRepository;

        #endregion

        #region CtorS

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="userClaims">The claims of user who is creating new user</param>
        public ManageProductProspectContact(DefaultUserClaim userClaims) : base((int)ProductEnum.ProspectContactCenter, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
#if DEBUG
            WriteToDiagnosticLog("ManageProductProspectContact.Ctor - Getting Product settings.");
#endif
            _productId = (int)ProductEnum.ProspectContactCenter;
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _editorRealPageId = userClaims.UserRealPageGuid;

            _blueBook = new ManageBlueBook(userClaims);
            _productRepository = new ProductRepository(userClaims);

            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
#if DEBUG
            WriteToDiagnosticLog("ManageProductProspectContact.Ctor - Received Product settings.");
#endif
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
        /// <param name="repository"></param>
        public ManageProductProspectContact(Guid editorRealPageId, DefaultUserClaim userClaims, HttpMessageHandler httpMessageHandler, IProductInternalSettingRepository productInternalSettingRepository,
            IManagePersona managePersona, ISamlRepository samlRepository, IManageBlueBook manageBlueBook, IRepository repository)
            : base((int)ProductEnum.ProspectContactCenter, userClaims, repository, httpMessageHandler)
        {
            WriteToDiagnosticLog("ManageProductProspectContact.Ctor - Getting Product settings.");

            _editorRealPageId = editorRealPageId;
            _managePersona = managePersona;
            _samlRepository = samlRepository;
            _blueBook = manageBlueBook;
            _apiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _client = new HttpClient(httpMessageHandler, false);
            _productRepository = new ProductRepository(repository, userClaims);
            WriteToDiagnosticLog("ManageProductProspectContact.Ctor - Received Product settings.");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Used to get properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        public ListResponse GetProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse result = new ListResponse();
            WriteToDiagnosticLog(
                $"ManageProductProspectContact.GetProperties - at begining of method for user with editorPersona id - {editorPersonaId}");

            try
            {
                result = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId); //TODO: need to refactor
                if (result.IsError)
                {
                    WriteToErrorLog(
                        $"ManageProductProspectContact.GetProperties.GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {result.ErrorReason}");
                    return result;
                }

                var companyDetails = GetProductCompanyInstanceId(_udmSourceCode, useTranslate: false);

                // blue book company Id
                int companyInstanceId = companyDetails.CompanyInstanceId;

                // product side company Id
                int companyInstanceSourceId = Convert.ToInt32(companyDetails.CompanyInstanceSourceId);

                WriteToDiagnosticLog(
                    $"ManageProductProspectContact.GetProperties-GetProductCompanyInstanceId - Found blue book company instance id - {companyInstanceId}; companyInstanceSourceId {companyInstanceSourceId}  for user editorPersona id -{editorPersonaId}");

                var productProperties = GetProductProperties(companyInstanceSourceId);

                WriteToDiagnosticLog(
                    $"ManageProductProspectContact.GetProperties-GetPropertyInstance - Found total {productProperties.Count} properties with blue book company instance id {companyInstanceId}, companyInstanceSourceId {companyInstanceSourceId} for user with editorPersona id - {editorPersonaId}.");


                // need to do a filter on the result
                if (userPersonaId != 0 && (_productUserId != null && _productUserId.Length > 0)) //update existing user
                {
                    WriteToDiagnosticLog(
                        $"ManageProductProspectContact.GetProperties- calling MergeProductPropertiesWithGreenbook....for user with editorPersona id -{editorPersonaId} & _productUserId-{_productUserId}.");
                    result = MergeProductPropertiesWithGreenbook(productProperties);
                    WriteToDiagnosticLog(
                        $"ManageProductProspectContact.GetProperties-MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}.");
                }
                else
                {
                    result = new ListResponse() // create new user
                    {
                        Records = productProperties.Cast<object>().ToList(),
                        TotalRows = productProperties.Count,
                        RowsPerPage = productProperties.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty
                    };
                }
            }
            catch (Exception ex)
            {
                result = new ListResponse()
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

                WriteToErrorLog(
                    $"ManageProductProspectContact.GetProperties - There was a problem getting the properties for user with editorPersona id - {editorPersonaId}.",
                    exception: ex);
            }

            return result;
        }

        private IList<ProductProperty> GetProductProperties(int companyInstanceSourceId)
        {
            IList<ProductPropertyMap> propertiesMap = null;

            var uri = new Uri(_apiEndPoint);
            var baseUri = uri.GetLeftPart(System.UriPartial.Authority);

            // not using existing GetResultsFromAPI method as this call requires additional headers
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response =
                    client.GetAsync(
                        $"{baseUri}/reportrestservice/ReportParameter/Property?companyId={companyInstanceSourceId}&mode=All").Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    propertiesMap = JsonConvert.DeserializeObject(jsonContent, typeof(IList<ProductPropertyMap>)) as IList<ProductPropertyMap>;
                }
                else
                {
                    WriteToErrorLog(
                        $"ManageProductProspectContact.GetProductProperties - There was a problem getting the properties for user with companyInstanceSourceId - {companyInstanceSourceId}." +
                        $" URL - {baseUri}/reportrestservice/ReportParameter/Property?companyId={companyInstanceSourceId}");
                }
            }

            return ToGBProperties(propertiesMap);
        }

        /// <summary>
        /// Unassign User
        /// </summary> 
        public string UnassignUser(long editorPersonaId, long userPersonaId)
        {
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog(
                    $"ManageProductProspectContact.UnassignUser - Error for user with userPersonaId:{userPersonaId}. ErrorReason-{listResponse.ErrorReason}");
                return listResponse.ErrorReason;
            }

            string result;
            try
            {
                WriteToDiagnosticLog($"ManageProductProspectContact.UnassignUser userPersonaId:{userPersonaId}");

                var currentProspectContactCenterUser = GetProspectContactCenterUser();
                result = DeactivateCurrentUser(_editorProductUserId);

                if (string.IsNullOrEmpty(result))
                {
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus,
                        (int)ProductBatchStatusType.Deleted);

                }

                var persona = _managePersona.GetPersona(userPersonaId);
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

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog($"ManageProductProspectContact.ManageProductProspectContactUser - Error for user with editorPersona id - {editorPersonaId} Error - Company not found.");
                    return "Company Setup Error: Please Contact Support.";
                }

                if (string.IsNullOrEmpty(userEmailAddress))
                {
                    WriteToDiagnosticLog(
                        $"ManageProductProspectContact.ManageProductProspectContactUser - no email address for user with editorPersona id - {editorPersonaId}; assigning bogus email.");

                    userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
                }

                // Check for user locations
                var prospectContactCenterUser = new ProspectContactCenterUser
                {
                    ModifyingUser = _editorProductUserId,
                    User = new ProspectContactCenterUserProfile
                    {
                        LoginName = userLogin.LoginName,
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        Email = userEmailAddress,
                        UserActive = true
                    },
                };
                prospectContactCenterUser.User.ManagementCompanyID = company.CompanyInstanceSourceId;
                prospectContactCenterUser.User.UserType = "C"; // community level
                prospectContactCenterUser.User.PropertyID = "0";


                string existanceuserType = currentProspectContactCenterUser.UserType.TrimEnd();
                if (existanceuserType != prospectContactCenterUser.User.UserType)
                {
                    ReCreateNewUser(userPersonaId, editorPersonaId, prospectContactCenterUser, true);
                }

            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Change Prospect Contact User Type
        /// </summary>
        public string ChangeProspectContactUserType(long createUserPersonaId, long assignUserPersonaId, ProspectContactPropertyRole roleProp, BatchProcessType batchProcessType)
        {
            WriteToDiagnosticLog($"ManageProductProspectContact.ChangeProspectContactUserType - Begin ChangeProspectContactUserType user for user with editorPersona id - {createUserPersonaId}.");
            return ManageProductProspectContactUser(createUserPersonaId, assignUserPersonaId, roleProp, batchProcessType);
        }

        /// <summary>
        /// Updated to create/update a user in Product Prospect Contact Center
        /// </summary>
        public string ManageProductProspectContactUser(long editorPersonaId, long userPersonaId, ProspectContactPropertyRole userProspectContactPropertyRole, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            WriteToDiagnosticLog($"ManageProductProspectContact.ManageProductProspectContactUser - Begin create/update user for user with editorPersona id - {editorPersonaId}.");

            try
            {
                var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog($"ManageProductProspectContact.ManageProductProspectContactUser Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}");
                    return listResponse.ErrorReason;
                }

                var persona = _managePersona.GetPersona(userPersonaId);
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
                    WriteToDiagnosticLog(
                        $"ManageProductProspectContact.ManageProductProspectContactUser - no email address for user with editorPersona id - {editorPersonaId}; assigning bogus email.");

                    userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
                }

                // super user
                if (IsSuperUser(userPersonaId))
                {
                    WriteToDiagnosticLog($"ManageProductProspectContact.ManageProductProspectContactUser - new user is Super user with editorPersona id - {editorPersonaId}.");

                    // Assign PMC
                    userProspectContactPropertyRole.PropertyList = new List<string> { "ALL" };
                }

                var productLoginName = string.IsNullOrEmpty(_productUsername) ? userLogin.LoginName : _productUsername;

                WriteToDiagnosticLog(
                    $"ManageProductProspectContact.ManageProductProspectContactUser - productUsername for user is {productLoginName}.");

                CustomerCompanyMap company = GetProductCompanyInstanceId(_udmSourceCode);

                if (string.IsNullOrEmpty(company.CompanyInstanceSourceId))
                {
                    WriteToErrorLog($"ManageProductProspectContact.ManageProductProspectContactUser - Error for user with editorPersona id - {editorPersonaId} Error - Company not found.");
                    return "Company Setup Error: Please Contact Support.";
                }

                // Check for user locations
                var prospectContactCenterUser = new ProspectContactCenterUser
                {
                    ModifyingUser = _editorProductUserId,
                    User = new ProspectContactCenterUserProfile
                    {
                        LoginName = userLogin.LoginName,
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        Email = userEmailAddress,
                        UserActive = true
                    },
                };

                //if (PMC) level
                if (userProspectContactPropertyRole.PropertyList[0].Trim().ToUpper() == "ALL") //all properties
                {
                    prospectContactCenterUser.User.ManagementCompanyID = company.CompanyInstanceSourceId;
                    prospectContactCenterUser.User.Properties = new List<string>() { "0" };
                    prospectContactCenterUser.User.PropertyID = "0";
                    prospectContactCenterUser.User.UserType = "M";
                }
                else
                {
                    prospectContactCenterUser.User.ManagementCompanyID = company.CompanyInstanceSourceId;
                    prospectContactCenterUser.User.UserType = "C"; // community level
                    prospectContactCenterUser.User.PropertyID = "0";
                    prospectContactCenterUser.User.Properties = userProspectContactPropertyRole.PropertyList;
                }

                WriteToDiagnosticLog($"ManageProductProspectContact.ManageProductProspectContactUser - Json to call product API for user with editorPersona id " +
                                     $"- {editorPersonaId} - {JsonConvert.SerializeObject(prospectContactCenterUser)}");

                if (string.IsNullOrEmpty(_productUsername)) // NEW USER
                {
                    string newproductUsername = $"{person.FirstName.TrimWhiteSpace().Substring(0, 1)}" + $"{person.LastName.TrimWhiteSpace()}".ToLower();
                    productLoginName = GetUniqueProductLoginName(productLoginName, newproductUsername);
                    prospectContactCenterUser.User.LoginName = productLoginName;

                    WriteToDiagnosticLog($"ManageProductProspectContact.ManageProductProspectContactUser - trying to CREATE user with editorPersona id - {editorPersonaId}.");
                    string newProductUserId = InsertProspectContactCenterUser($"{_apiEndPoint}/User", userPersonaId, editorPersonaId, productLoginName, prospectContactCenterUser);
                    // for new user insert record in green prospectContactCenterUserbook
                    CreateProductUserInGreenBook(userPersonaId, newProductUserId, productLoginName);


                    if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                    {
                        WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, batchProcessType);
                    }

                    return string.Empty;
                }

                // UPDATE USER 
                WriteToDiagnosticLog($"ManageProductProspectContact.ManageProductProspectContactUser - trying to UPDATE user with editorPersona id - {editorPersonaId}.");

                prospectContactCenterUser.User.SystemIdentifier = _productUserId;
                prospectContactCenterUser.User.LoginName = _productUsername;
                var updateResult = UpdateProspectContactCenterPropertyUser(userPersonaId, editorPersonaId, prospectContactCenterUser);

                return updateResult;
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductProspectContact.ManageProductProspectContactUser - Error for user with editorPersona id - {editorPersonaId}", exception: ex);
                return $"Error - {ex.Message}";
            }
        }

        /// <summary>
        /// Update Prospect Contact Center User Profile
        /// </summary> 
        public string UpdateProspectContactCenterUserProfile(long editorPersonaId, long userPersonaId)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            var listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog($"ManageProductProspectContact.UpdateProspectContactCenterUserProfile Error for user with editorPersona id - {editorPersonaId}. Error - {listResponse.ErrorReason}");
                return listResponse.ErrorReason;
            }

            var persona = _managePersona.GetPersona(userPersonaId);
            var realPageId = persona.RealPageId;
            var person = _managePerson.GetPerson(realPageId);
            var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);
            string productLoginName = "";
            // get the email address
            string userEmailAddress = string.Empty;
            var manageElectronicAddress = new ManageElectronicAddress();
            var addresses = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);

            if (addresses != null)
            {
                if (addresses.Any(a => a.AddressType.ToUpper() == "EMAIL"))
                {
                    userEmailAddress = (from a in addresses where a.AddressType.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                }
            }

            if (string.IsNullOrEmpty(userEmailAddress))
            {
                WriteToDiagnosticLog($"ManageProductProspectContact.UpdateProspectContactCenterUserProfile - no email address for user with editorPersona id - {editorPersonaId}; assigning bogus email.");

                userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
            }

            WriteToDiagnosticLog($"ManageProductProspectContact.UpdateUserProfile - Product User Name : {_productUsername}");

            if (string.IsNullOrEmpty(_productUsername))
            {
                productLoginName = userEmailAddress;
            }
            else
            {
                productLoginName = _productUsername;
            }

            IList<UserOrganization> userPersonaOrganizationList = _manageUserLogin.GetUserPersonaOrganization(userLogin.LoginName);
            //If the User's LoginName changed in the PrimaryOrganization then update it in the Product
            if ((userPersonaOrganizationList.ToList().Any(o => o.PrimaryOrganization.Equals(true) && o.OrganizationPartyId.Equals(persona.OrganizationPartyId))) && (!_productUsername.Equals(userLogin.LoginName, StringComparison.OrdinalIgnoreCase)))
            {
                productLoginName = userLogin.LoginName;
            }

            // Check for user locations
            var prospectContactCenterUser = new ProspectContactCenterUser
            {
                ModifyingUser = _editorProductUserId,
                User = new ProspectContactCenterUserProfile
                {
                    SystemIdentifier = _productUserId,
                    LoginName = productLoginName,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = userEmailAddress,
                    UserActive = true
                },
            };

            WriteToDiagnosticLog($"ManageProductProspectContact.UpdateProspectContactCenterUserProfile - updating user with persona id {userPersonaId}.");

            logData = new Dictionary<string, object>
            {
                { "prospectContactCenterUser", prospectContactCenterUser }
            };

            WriteToDiagnosticLog("ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - Update user profile data.", logData);

            // lastly update user
            var result = UpdateProspectContactCenterUser($"{_apiEndPoint}/User", userPersonaId, editorPersonaId, prospectContactCenterUser);

            if (string.IsNullOrEmpty(result))
            {
                WriteToDiagnosticLog($"ManageProductProspectContact.UpdateProspectContactCenterUserProfile - Update in GB -productUsername -{userEmailAddress} and userId {_productUserId}.");
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);

                foreach (var attribute in productAttributes)
                {
                    if (attribute.Name.ToUpper() == "PRODUCTUSERNAME")
                    {
                        attribute.Value = productLoginName;
                        _samlRepository.UpdateSamlUserAttribute(attribute);
                    }
                }

                // add activity log
                WriteUpdateUserTypeActivityLog(editorPersonaId, person, userLogin, BatchProcessType.ProfileUpdate);
            }

            return result;
        }


        #endregion

        /// <summary>
        /// Changes the user status.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, int userId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError)
            {
                return false;
            }

            try
            {
                _productUserId = userId.ToString();
                DeactivateCurrentUser(_editorProductUserId);
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductProspectContact.ChangeUserActiveStatus - Updating user status failed for user {userId} by editorPersonaId = {editorPersonaId}", exception: ex);
                return false;
            }

            return true;
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
            var claimResposnse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResposnse.IsError)
            {
                response.ErrorReason = claimResposnse.ErrorReason;
                return response;
            }

            try
            {

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductProspectContact.GetMigrationUsers.GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    response.ErrorReason = "Company Setup Error: Please Contact Support.";
                    return response;
                }

                var filter = "GreenbookUser";
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

                var url = $"{_apiEndPoint}/users/{companyInstanceSourceId}?filter={filter}&startRow={startRow}&resultsPerPage={resultPerRow}";
                WriteToDiagnosticLog("ManageProductProspectContact.GetMigrationUsers", new Dictionary<string, object> { { "Url", url } });

                var allUsers = GetResultFromApi<List<ProspectContactCenterUserProfile>>(url);

                if (allUsers == null)
                {
                    WriteToErrorLog($"ManageProductProspectContact.GetMigrationUsers-no users received from product for user with editorPersona id - {editorPersonaId}.");
                    return response;
                }

                //This logic will remove the users whom already migrated in to UL,since PCC not implemented filter logic.
                string product = Convert.ToString((int)ProductEnum.ProspectContactCenter);
                IList<SharedObjects.Product.OrganizationProductUser> productUserList = _productRepository.GetProductUsersByCompany(_editorPersona.OrganizationPartyId, product);

                if (productUserList?.Count > 0)
                {
                    allUsers.RemoveAll(o => productUserList.Any(p => p.ProductUserName == o.LoginName));
                }

                var migrationUsers = new List<MigrationUser>();
                foreach (var user in allUsers)
                {
                    var migrationUser = new MigrationUser
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Username = user.LoginName,
                        UserId = user.SystemIdentifier,
                        Status = user.UserActive ? "Active" : "Disabled",
                        LastActivity = user.LastLogin.ToString(),
                        Email = user.Email,
                        CompanyInstanceSourceId = companyInstanceSourceId.ToString()
                    };
                    if (user.Properties?.Count() > 0)
                    {
                        foreach (var item in user.Properties)
                        {
                            migrationUser.Properties.Add(new MigrationProperty() { PropertyInstanceSourceId = item });
                        }
                        //migrationUser.Properties.Add(new MigrationProperty() { PropertyInstanceSourceId = user.PropertyID });
                    }

                    migrationUsers.Add(migrationUser);
                }

                WriteToDiagnosticLog($"ManageProductProspectContact.GetUsers - Received users from product for user with editorPersona id - {editorPersonaId}.");
                response.RowsPerPage = 9999;
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

                WriteToErrorLog($"ManageProductProspectContact.GetMigrationUsers Error for user with editorPersona id - {editorPersonaId} ", exception: ex);
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
            if (claimResposnse.IsError)
            {
                migrateResponse.Message = claimResposnse.ErrorReason;
                return migrateResponse;
            }

            try
            {

                int companyInstanceSourceId = Convert.ToInt32(GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId);
                if (companyInstanceSourceId == 0)
                {
                    WriteToErrorLog(
                        $"ManageProductProspectContact.UpdateUsersMigrationStatus.GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}.");
                    migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                    return migrateResponse;
                }

                var url = $"{_apiEndPoint}/migrate-users/{companyInstanceSourceId}";
                var response = _client.PutAsJsonAsync(url, migrateUsers).Result;
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
                    WriteToDiagnosticLog("ManageProductProspectContact.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
                    migrateResponse.Message = migrationResponse.Message;
                    migrateResponse.Status = migrationResponse.Status;
                    return migrateResponse;
                }
                else
                {
                    WriteToErrorLog($"ManageProductProspectContact.UpdateUsersMigrationStatus.PostAsJsonAsync", logData);
                    migrateResponse.Message = "Cannot update user status to migrated.";
                    return migrateResponse;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog($"ManageProductProspectContact.UpdateUsersMigrationStatus Error for user with editorPersona id - {editorPersonaId} ", exception: ex);

                return new MigrateResponse
                {
                    Status = false,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region Private Methods

        private string GetUniqueProductLoginName(string productLoginName, string newproductUsername)
        {
            bool foundNewUserName = false;
            int incrementor = 0;
            while (!foundNewUserName)
            {
                if (IsUsernameAvailable(productLoginName))
                {
                    incrementor++;
                    productLoginName = $"{newproductUsername}{incrementor}";
                }
                else
                {
                    foundNewUserName = true;
                }
            }

            return productLoginName;
        }

        private ListResponse MergeProductPropertiesWithGreenbook(IList<ProductProperty> propertyList)
        {
            // merge the given user details with the list
            var prospectContactCenterUser = GetProspectContactCenterUser();
            if (prospectContactCenterUser == null)
            {
                WriteToErrorLog($"ManageProductProspectContact.MergeProductPropertiesWithGreenbook - Error - User not found in Prospect Center Contact with ProductUserId - {_productUserId}.");
                return new ListResponse()
                {
                    IsError = true,
                    ErrorReason = $"User not found in Prospect Center Contact with ProductUserId - {_productUserId}"
                };
            }

            Dictionary<string, bool> additionalData = new Dictionary<string, bool>();
            if (prospectContactCenterUser.UserType.Trim().ToUpper() == "M")
            {
                additionalData.Add("allProperties", true);
            }
            else if (prospectContactCenterUser.Properties.Count() > 0)
            {
                foreach (var item in prospectContactCenterUser.Properties)
                {
                    propertyList.FirstOrDefault(x => x.ID == item).IsAssigned = true;
                }

                additionalData.Add("allProperties", false);
            }

            return new ListResponse()
            {
                Records = propertyList.Cast<object>().ToList(),
                TotalRows = propertyList.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = additionalData
            };
        }

        private ProspectContactCenterUserProfile GetProspectContactCenterUser()
        {
            string baseUrlAndQuery = $"{_apiEndPoint}/User/{_productUserId}";
            return GetResultFromApi<ProspectContactCenterUserProfile>(baseUrlAndQuery, false);
        }

        private T GetResultFromApi<T>(string baseUrlAndQuery, bool throwOnError = true) where T : class
        {
            T results = null;
            var response = _client.GetAsync(baseUrlAndQuery).Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                results = JsonConvert.DeserializeObject(jsonContent, typeof(T)) as T;
            }

            return results;
        }

        private bool IsUsernameAvailable(string productLoginName)
        {
            WriteToDiagnosticLog(
                $"ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - checking if {productLoginName} available.");

            using (var client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head,
                    new Uri($"{_apiEndPoint}/User?loginName={productLoginName}"));

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
        }

        private string UpdateProspectContactCenterPropertyUser(long userPersonaId, long editorPersonaId,
            ProspectContactCenterUser prospectContactCenterUser)
        {
            var currentProspectContactCenterUser = GetProspectContactCenterUser();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            if (!currentProspectContactCenterUser.UserActive && prospectContactCenterUser.User.UserActive)
            {
                ReCreateNewUser(userPersonaId, editorPersonaId, prospectContactCenterUser);
            }


            // Check if UserType changed
            if (currentProspectContactCenterUser.UserType.Trim() != prospectContactCenterUser.User.UserType.Trim())
            {

                WriteToDiagnosticLog(
                    $"ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - User Type changed, soft deleteing user before creating new one.");

                // delete/deactivate existing user from product
                DeactivateCurrentUser(prospectContactCenterUser.ModifyingUser);

                WriteToDiagnosticLog(
                    $"ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - User Type changed, updating login name & user email for existing user.");

                var userToUpdate = new ProspectContactCenterUser
                {
                    ModifyingUser = prospectContactCenterUser.ModifyingUser,
                    User = new ProspectContactCenterUserProfile
                    {
                        LoginName = $"{currentProspectContactCenterUser.LoginName}_GB{DateTime.Now.Ticks}",
                        Email = $"{currentProspectContactCenterUser.Email}_GB{DateTime.Now.Ticks}",
                        FirstName = currentProspectContactCenterUser.FirstName,
                        LastName = currentProspectContactCenterUser.LastName,
                        SystemIdentifier = currentProspectContactCenterUser.SystemIdentifier,
                        UserType = currentProspectContactCenterUser.UserType,
                        ManagementCompanyID = currentProspectContactCenterUser.ManagementCompanyID,
                        PropertyID = currentProspectContactCenterUser.PropertyID,
                        //Properties = currentProspectContactCenterUser.Properties,
                        UserActive = true
                    }
                };

                var updateUserResult = UpdateProspectContactCenterUser($"{_apiEndPoint}/User", userPersonaId, editorPersonaId, userToUpdate);

                if (updateUserResult != string.Empty)
                {
                    return updateUserResult;
                }

                WriteToDiagnosticLog(
                    $"ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - Updated GB product status to delete product for user with ersona Id {userPersonaId}.");

                // update GB product status to delete product
                // UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

                // recreate new user
                WriteToDiagnosticLog(
                    $"ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - recreating new user in product for persona id {userPersonaId}.");

                return ReCreateNewUser(userPersonaId, editorPersonaId, prospectContactCenterUser);
            }

            // Check if property Id changed
            //if (currentProspectContactCenterUser.PropertyID != prospectContactCenterUser.User.PropertyID)
            //{
            WriteToDiagnosticLog(
                $"ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - property changed for user with persona id {userPersonaId}.");

            // update property

            var resultUpdateUserProperty = UpdateUserProperty(prospectContactCenterUser.User.Properties,
                prospectContactCenterUser.ModifyingUser, prospectContactCenterUser.User.ManagementCompanyID);
            if (resultUpdateUserProperty != string.Empty)
                return resultUpdateUserProperty;
            //}

            WriteToDiagnosticLog(
                $"ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - updating user with persona id {userPersonaId}.");

            logData = new Dictionary<string, object>();
            logData.Add("prospectContactCenterUser", prospectContactCenterUser);
            WriteToDiagnosticLog("ManageProductProspectContact.UpdateProspectContactCenterPropertyUser - Update user data.", logData);

            // lastly update user
            var result = UpdateProspectContactCenterUser($"{_apiEndPoint}/User", userPersonaId, editorPersonaId, prospectContactCenterUser);

            if (result == string.Empty)
            {
                // Update GB with success status
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
            }
            else
            {
                // Update GB with Error status
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
            }

            return result;
        }

        private string ReCreateNewUser(long userPersonaId, long editorPersonaId, ProspectContactCenterUser prospectContactCenterUser, bool isSamlUpdateRequired = false)
        {
            // change exsting user name
            var newProductLoginName = prospectContactCenterUser.User.LoginName; //IncrementCurrentProductLoginName(prospectContactCenterUser.User.LoginName);
            string newproductUsername = $"{prospectContactCenterUser.User.FirstName.TrimWhiteSpace().Substring(0, 1)}" + $"{prospectContactCenterUser.User.LastName.TrimWhiteSpace()}".ToLower();
            // Now check if user name exists in product
            newProductLoginName = GetUniqueProductLoginName(newProductLoginName, newproductUsername);

            prospectContactCenterUser.User.LoginName = newProductLoginName;
            prospectContactCenterUser.User.SystemIdentifier = null;

            WriteToDiagnosticLog($"ManageProductProspectContact.ManageProductProspectContactUser - trying to CREATE user with editorPersona id - {editorPersonaId}.");
            string newProductUserId = InsertProspectContactCenterUser($"{_apiEndPoint}/User", userPersonaId, editorPersonaId, newProductLoginName, prospectContactCenterUser);

            // Update saml settings in GB
            if (!isSamlUpdateRequired)
            {
                UpdateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, newProductUserId);
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
            }

            return string.Empty;
        }

        private string DeactivateCurrentUser(string modifyingUserId)
        {
            using (var client = new HttpClient())
            {
                string requestUrl = $"{_apiEndPoint}/User?userId={_productUserId}&modifyingUser={modifyingUserId}";
                var response = client.DeleteAsync(requestUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync();
                    return string.Empty;
                }

                string errorContent = string.Empty;
                try
                {
                    errorContent = response.Content.ReadAsStringAsync().Result;
                    WriteToErrorLog($"Error in DeactivateCurrentUser; errorContent= {errorContent}");
                }
                catch
                {
                    /*Ignored*/
                }

                throw new Exception($"Deactivate user Failed. errorContent- {errorContent}");
            }
        }

        private string UpdateUserProperty(IList<string> propertyId, string modifyingUserId, string companyID)
        {

            WriteToDiagnosticLog($"ManageProductProspectContact.UpdateUserProperty - propertyId={propertyId}modifyingUserId={modifyingUserId}");

            dynamic userPropObj = new ExpandoObject();
            userPropObj.PropertyID = 0;
            userPropObj.ModifyingUser = modifyingUserId;
            userPropObj.Properties = propertyId;
            userPropObj.ManagementCompanyID = companyID;

            var apiUrl = $"{_apiEndPoint}/User/{_productUserId}/relationships/property?_HttpMethod=PATCH";

            using (var client = new HttpClient())
            {
                var content = new ObjectContent<dynamic>(userPropObj, new JsonMediaTypeFormatter());
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl) { Content = content };
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog(
                        $"ManageProductProspectContact.UpdateUserProperty - IsSuccessStatusCode return true for user with _productUserId - {_productUserId}.");
                    return string.Empty;
                }

                string errorContent = string.Empty;
                try
                {
                    errorContent = response.Content.ReadAsStringAsync().Result;
                    WriteToErrorLog($"Error in UpdateUserProperty; errorContent= {errorContent}");
                }
                catch
                {
                    /*Ignored*/
                    WriteToErrorLog("Error in UpdateUserProperty; errorContent=Ignored");
                }

                return $"Error in UpdateUserProperty; errorContent= {errorContent}";
            }
        }

        private string UpdateProspectContactCenterUser(string productApiUrl, long userPersonaId, long editorPersonaId, ProspectContactCenterUser prospectContactCenterUser)
        {
            using (var client = new HttpClient())
            {
                WriteToDiagnosticLog($"ManageProductProspectContact.UpdateProspectContactCenterUser - calling product API for user with editorPersona id - {editorPersonaId}.");

                var response = client.PutAsJsonAsync(productApiUrl, prospectContactCenterUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog($"ManageProductProspectContact.UpdateProspectContactCenterUser - IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}.");

                    return string.Empty;
                }

                string errorContent = string.Empty;
                try
                {
                    errorContent = response.Content.ReadAsStringAsync().Result;
                }
                catch
                {
                    /*Ignored*/
                }

                WriteToErrorLog(
                    $"ManageProductProspectContact.UpdateProspectContactCenterUser - Error for user with editorPersona id - {editorPersonaId}. Error - {errorContent}.");

                return $"There was a problem updating the user with editorPersona id - {editorPersonaId} - Error-{errorContent}.";
            }
        }

        private string InsertProspectContactCenterUser(string productApiUrl, long userPersonaId, long editorPersonaId, string productLoginName, ProspectContactCenterUser prospectContactCenterUser)
        {
            string result = string.Empty;
            using (var client = new HttpClient())
            {

                WriteToDiagnosticLog($"ManageProductProspectContact.InsertProspectContactCenterUser - calling product API for user with editorPersona id - {editorPersonaId}.");

                var response = client.PostAsJsonAsync(productApiUrl, prospectContactCenterUser).Result;

                if (response.IsSuccessStatusCode)
                {
                    WriteToDiagnosticLog($"ManageProductProspectContact.InsertProspectContactCenterUser - IsSuccessStatusCode return true for user with editorPersona id - {editorPersonaId}.");
                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                    if (userResult != null)
                    {
                        result = userResult.ToString();
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

                    WriteToErrorLog(
                        $"ManageProductProspectContact.InsertProspectContactCenterUser - Error for user with editorPersona id- {editorPersonaId} Error - {errorContent}.");
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus,
                        (int)ProductBatchStatusType.Error);
                    throw new Exception($"There was a problem creating the user with editorPersona id - {editorPersonaId}. Error-{errorContent}");
                }
            }

            return result;
        }

        private void CreateProductUserInGreenBook(long userPersonaId, string productUserId, string productLoginName)
        {
            WriteToDiagnosticLog($"ManageProductProspectContact.CreateProductUserInGreenBook - Inserting in GB -productUsername -{productLoginName} and userId {productUserId}.");
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, productLoginName);
            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, productUserId);

            WriteToDiagnosticLog("ManageProductProspectContact.CreateProductUserInGreenBook - Create user Success. Set product status to Success. ProductUsername -{productLoginName} and userId {productUserId}.");
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
        }

        /// <summary>
        /// Used to convert a Product property into a GreenBook property
        /// </summary>
        /// <param name="properties">The list of properties to convert</param>
        /// <returns></returns>
        private static IList<ProductProperty> ToGBProperties(IList<ProductPropertyMap> properties)
        {
            if (properties == null) return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (ProductPropertyMap property in properties)
            {
                results.Add(new ProductProperty
                {
                    ID = property.PropertyId,
                    Name = property.PropertyName,
                    State = property.State,
                    Active = property.Active,
                    Status = property.Active == "true" ? "Active" : "Inactive"
                });
            }

            return results;
        }

        #endregion
    }

    public class ProspectContactCenterUser
    {
        public string ModifyingUser { get; set; }

        [JsonProperty(PropertyName = "User")] public ProspectContactCenterUserProfile User { get; set; }
    }

    public class ProspectContactCenterUserProfile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LoginName { get; set; }
        public bool UserActive { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ManagementCompanyID { get; set; }

        public string PropertyID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SystemIdentifier { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime LastLogin { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<string> Properties { get; set; }

    }
}
