using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RentersInsurance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Security;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Used to update Renters Insurance user information
    /// </summary>
    public class ManageProductRentersInsurance : ManageProductBase, IManageProductRentersInsurance
    {
        #region Private Variables
        private string _username;
        private string _password;
        private string _rentersInsuranceUrl;
        private string _rentersInsuranceApiEndPoint;
        private int _requestedBy;
        private long _companyInstanceSourceId;
        private long _companyInstanceId;

        private IList<ProductSettingList> _userProductSettings = new List<ProductSettingList>();
        private UserAPIResponse _userAPIResponse = new UserAPIResponse();
        private ListOfUserRolesResponse _listOfUserRolesResponse = new ListOfUserRolesResponse();
        private ListPropertyByPMCIDResponse _listPropertyByPMCIDResponse = new ListPropertyByPMCIDResponse();

        /// Services
        private IInsuranceService _insuranceService = new InsuranceService();
        #endregion

        #region Constructor
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="userClaims">User Claim</param>
        public ManageProductRentersInsurance(DefaultUserClaim userClaims) : base((int)ProductEnum.Insurance,userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _productId = (int)ProductEnum.Insurance;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaims);

            _rentersInsuranceUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "PRODUCTURL").Value;
            _rentersInsuranceApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _username = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value;
            _password = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value;
            _requestedBy = Convert.ToInt32(_productInternalSettingList.First(a => a.Name.ToUpper() == "REQUESTEDBY").Value);

            _insuranceService.Url = _rentersInsuranceApiEndPoint;
        }

        /// <summary>
        /// Unit test constructor to test list roles
        /// </summary>
        /// <param name="editorRealPageId">The RealPageId of the editor</param>
        /// <param name="userClaim">The claim of the user executing the call</param>
        /// <param name="messageHandler">An http handler used for moq'ing requests</param>
        /// <param name="rentersInsuraceService">Renters Insurace Service</param>
        /// <param name="samlRepository">SAML Repository</param>
        /// <param name="managePersona">Persona business logic</param>
        /// <param name="manageBlueBook">BlueBook business logic</param>
        /// <param name="productRepository">Product Repository</param>
        /// <param name="productInternalSettingRepository">Product Internal Setting Repository</param>
        /// <param name="managePerson">Person business logic</param>
        /// <param name="manageUserLogin">UserLogin business logic</param>
        /// <param name="managePartyRelationship">Party Relationship business logic</param>
        /// <param name="repository">The sql repository used for moq'ing sql calls</param>
        public ManageProductRentersInsurance(Guid editorRealPageId, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, 
            IInsuranceService rentersInsuraceService, ISamlRepository samlRepository,
            IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin,
            IManagePartyRelationship managePartyRelationship, IRepository repository)
            : base((int)ProductEnum.Insurance, userClaim, repository, messageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _insuranceService = rentersInsuraceService;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _managePerson = managePerson;
            _manageUserLogin = manageUserLogin;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
            _productRepository = productRepository;
            
            _insuranceService.Url = "http://localhost";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editorRealPageId">The RealPageId of the editor</param>
        /// <param name="userClaim">The claim of the user executing the call</param>
        /// <param name="messageHandler">An http handler used for moq'ing requests</param>
        /// <param name="companyInstanceId">Company Id</param>
        /// <param name="rentersInsuraceService">Renters Insurace Service</param>
        /// <param name="listPropertyByPMCIDResponse">list of Properties By PMCID Response</param>
        /// <param name="samlRepository">SAML Repository</param>
        /// <param name="managePersona">Persona business logic</param>
        /// <param name="manageBlueBook">BlueBook business logic</param>
        /// <param name="productRepository">Product Repository</param>
        /// <param name="productInternalSettingRepository">Product Internal Setting Repository</param>
        /// <param name="managePerson">Person business logic</param>
        /// <param name="manageUserLogin">UserLogin business logic</param>
        /// <param name="managePartyRelationship">Party Relationship business logic</param>
        /// <param name="repository">The sql repository used for moq'ing sql calls</param>
        public ManageProductRentersInsurance(Guid editorRealPageId, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, long companyInstanceId, IInsuranceService rentersInsuraceService, ListPropertyByPMCIDResponse listPropertyByPMCIDResponse, ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship, IRepository repository)
            : base((int)ProductEnum.Insurance, userClaim, repository, messageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _companyInstanceId = companyInstanceId;
            _insuranceService = rentersInsuraceService;
            _listPropertyByPMCIDResponse = listPropertyByPMCIDResponse;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _managePerson = managePerson;
            _manageUserLogin = manageUserLogin;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Disable User in Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>ObjectOutput object</returns>
        public ObjectOutput<UserAPIResponse, IErrorData> DisableRentersInsuranceUser(long editorPersonaId, long userPersonaId)
        {
            ObjectOutput<UserAPIResponse, IErrorData> output = new ObjectOutput<UserAPIResponse, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                errorStatus.Success = false;
                errorStatus.ErrorMsg = listResponse.ErrorReason;
                output.Status = errorStatus;
                return output;
            }

            //Call Renters Insurance Disable User API
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUserId", _productUserId } }, messageProperties: new object[] { "DisableRentersInsuranceUser", "Deleting user" });
                UserActionRequest userActionRequest = new UserActionRequest()
                {
                    Login = _username,
                    Password = _password,
                    RequestedBy = _requestedBy,
                    UserId = Convert.ToInt32(_productUserId)
                };
                _userAPIResponse = _insuranceService.DisableUser(userActionRequest);
                if (_userAPIResponse.IsSuccess && !string.IsNullOrWhiteSpace(_userAPIResponse.UserId.ToString()))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", JsonConvert.SerializeObject(_userAPIResponse) } }, messageProperties: new object[] { "DisableRentersInsuranceUser", "Deleting user result" });
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                errorStatus.Success = false;
                errorStatus.ErrorMsg = "ManageProductRentersInsurance.DisableRentersInsuranceUser - Error " + ex.Message;
                output.Status = errorStatus;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "DisableRentersInsuranceUser", $"Error. Reason: {ex.Message}" });
                return output;
            }

            errorStatus.Success = true;
            errorStatus.ErrorMsg = "";
            output.Status = errorStatus;
            output.obj = _userAPIResponse;
            return output;
        }

        /// <summary>
        /// Enable User in Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Error object</returns>
        public ObjectOutput<UserAPIResponse, IErrorData> EnableRentersInsuranceUser(long editorPersonaId, long userPersonaId)
        {
            ObjectOutput<UserAPIResponse, IErrorData> output = new ObjectOutput<UserAPIResponse, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                errorStatus.Success = false;
                errorStatus.ErrorMsg = listResponse.ErrorReason;
                output.Status = errorStatus;
                return output;
            }

            //Call Renters Insurance Enable User API
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() {{"userId", _productUserId}}, messageProperties: new object[] { "EnableRentersInsuranceUser", "Enable user" });
                UserActionRequest userActionRequest = new UserActionRequest()
                {
                    Login = _username,
                    Password = _password,
                    RequestedBy = _requestedBy,
                    UserId = Convert.ToInt32(_productUserId)
                };
                _userAPIResponse = _insuranceService.EnableUser(userActionRequest);
                if (_userAPIResponse.IsSuccess && !string.IsNullOrWhiteSpace(_userAPIResponse.UserId.ToString()))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", JsonConvert.SerializeObject(_userAPIResponse) } }, messageProperties: new object[] { "EnableRentersInsuranceUser", "Api response" });
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                errorStatus.Success = false;
                errorStatus.ErrorMsg = "ManageProductRentersInsurance.EnableRentersInsuranceUser - Error " + ex.Message;
                output.Status = errorStatus;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "EnableRentersInsuranceUser", $"Error. Reason: {ex.Message}" });
                return output;
            }

            errorStatus.Success = true;
            errorStatus.ErrorMsg = "";
            output.Status = errorStatus;
            output.obj = _userAPIResponse;
            return output;
        }

        /// <summary>
        /// Used to list properties  
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <param name="datafilter"></param>
        /// <returns>ListResponse object</returns>
        public ListResponse ListProperties(long editorPersonaId, long userPersonaId, RequestParameter datafilter)
        {
            ListResponse listResponse = new ListResponse();
            IList<ProductProperty> blueBookPropertyList = new List<ProductProperty>();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"Beginning. editorPersona id - {editorPersonaId}" });

            try
            {
                listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"Error. editorPersona id - {editorPersonaId} - Error: {listResponse.ErrorReason}" });
                    return listResponse;
                }

                if (_companyInstanceId == 0)
                {
                    _companyInstanceId = GetProductCompanyInstanceId(_udmSourceCode, useTranslate:false).CompanyInstanceId;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"Found blue book company instance id - {_companyInstanceId} editorPersona id - {editorPersonaId}" });
                
                CompanyPropertyRootObject CompanyProperties = _blueBook.GetCompanyPropertyInstance(_companyInstanceId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"Found total {CompanyProperties.data.attributes.getCompanyPropertyInstances.Count} properties with blue book company instance id {_companyInstanceId} for user with editorPersona id - {editorPersonaId}" });

                blueBookPropertyList = CompanyProperties.MapBlueBookToGBProperties() ?? new List<ProductProperty>();
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"MapBlueBookToGBProperties() completed for user with editorPersona id - {editorPersonaId}" });

                //called during updating Existing User to flag the properties the user has access to.
                if (userPersonaId != 0 && (_productUserId?.Length > 0))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"Calling MergeProductPropertiesWithGreenbook. editorPersona id - {editorPersonaId} & _productUserId-{_productUserId}" });
                    listResponse = MergeProductPropertiesWithGreenbook(editorPersonaId, userPersonaId, blueBookPropertyList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"MergeProductPropertiesWithGreenbook completed for user with editorPersona id - {editorPersonaId}" });
                }
                else
                {
                    Dictionary<string, bool> additionalDictionary = new Dictionary<string, bool>
                    {
                        { "allProperties", false }
                    };

                    listResponse = new ListResponse()
                    {
                        Records = blueBookPropertyList.Cast<object>().ToList(),
                        TotalRows = blueBookPropertyList.Count,
                        RowsPerPage = blueBookPropertyList.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty,
                        Additional = additionalDictionary
                    };
                }
            }
            catch (Exception ex)
            {
                listResponse = new ListResponse()
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    listResponse.ErrorReason = ex.Message;
                }
                else
                {
                    listResponse.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }

                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ListProperties", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
            }
            return listResponse;
        }

        /// <summary>
        /// Used to list Renters Insurance properties by PMCId
        /// </summary>
        /// <param name="editorPersonaId">The persona id of the user making the request</param>
        /// <param name="userPersonaId">The persona id of the user being changed</param>
        /// <returns>ListResponse object</returns>
        public ObjectListOutput<PropertyInstance, IErrorData> ListPropertiesByPMCID(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse = new ListResponse();
            ListPropertyByPMCIDResponse listPropertyByPMCIDResponse = new ListPropertyByPMCIDResponse();
            ObjectListOutput<PropertyInstance, IErrorData> outputList = new ObjectListOutput<PropertyInstance, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            outputList.Status = errorStatus;
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListPropertiesByPMCID", $"Beginning. editorPersona id - {editorPersonaId}" });

            try
            {
                listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ListPropertiesByPMCID", $"Error. editorPersona id - {editorPersonaId} Reason: {listResponse.ErrorReason}" });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "ManageProductRentersInsurance.ListPropertiesByPMCID.GetCompanyEditorAndUserDetails - Error " + listResponse.ErrorReason;
                    outputList.Status = errorStatus;
                    return outputList;
                }

                int companyInstanceId = GetProductCompanyInstanceId(_udmSourceCode, useTranslate:false).CompanyInstanceId;
                if (companyInstanceId == 0)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ListPropertiesByPMCID", $"Error looking for company id in bluebook. editorPersona id - {editorPersonaId}" });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "Company Setup Error: Please Contact Support.";
                    outputList.Status = errorStatus;
                    return outputList;
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListPropertiesByPMCID", $"Found bluebook company instance id - {companyInstanceId} editorPersona id - {editorPersonaId}" });

                IList<PropertyInstance> propertyList = _blueBook.GetPropertyInstance(companyInstanceId);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListPropertiesByPMCID", $"Found total {propertyList.Count} properties with blue book company instance id {companyInstanceId} editorPersona id - {editorPersonaId}" });

                listPropertyByPMCIDResponse = _insuranceService.GetListPropertyByPMCID(companyInstanceId);

                if (listPropertyByPMCIDResponse?.PropertyList != null)
                {
                    propertyList.ToList().ForEach(blueBook => blueBook.IsActive = listPropertyByPMCIDResponse.PropertyList.Any(rentersInsurance => rentersInsurance.PropertyID.ToString() == blueBook.PropertyInstanceSourceId));
                }
                errorStatus.Success = true;
                errorStatus.ErrorMsg = "";
                outputList.Status = errorStatus;
                outputList.list = propertyList;
                return outputList;
            }
            catch (Exception ex)
            {
                errorStatus.Success = false;

                if (ex is BlueBookException)
                {
                    errorStatus.ErrorMsg = ex.Message;
                }
                else
                {
                    errorStatus.ErrorMsg = "ManageProductRentersInsurance.ListPropertiesByPMCID - Error " + ex.Message;
                }

                outputList.Status = errorStatus;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ListPropertiesByPMCID", $"Error. editorPersona id - {editorPersonaId} Reason: {ex.Message}" });
                return outputList;
            }
        }

        /// <summary>
        /// List Roles
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Levels list</returns>
        public IList<ProductRole> ListRoles(long editorPersonaId, long userPersonaId)
        {
            GetUserByIDResponse getUserByIDResponse = new GetUserByIDResponse();
            IList<ProductRole> productRoleList = new List<ProductRole>();
            ListResponse listResponse = new ListResponse();

            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if ((!listResponse.IsError) && (!string.IsNullOrWhiteSpace(_productUserId)))
            {
                UserActionRequest userActionRequest = new UserActionRequest()
                {
                    Login = _username,
                    Password = _password,
                    RequestedBy = _requestedBy,
                    UserId = Convert.ToInt32(_productUserId)
                };
                getUserByIDResponse = _insuranceService.GetUserByID(userActionRequest);
            }

            _listOfUserRolesResponse = _insuranceService.GetListOfUserRoles();
            productRoleList = _listOfUserRolesResponse.ToGBRoles();

            //if a user record exists
            if (getUserByIDResponse?.UserInfo != null)
            {
                productRoleList.ToList().Find(item => item.Name == getUserByIDResponse.UserInfo.Role).IsAssigned = true;
            }
            //relabel Renters Insurance Roles names for the UI
            productRoleList.ToList().ForEach(item =>
            {
                switch (Convert.ToInt32(item.ID))
                {
                    case 2:
                        //ID = 2 and Name = PMC => Corporate User
                        item.Name = "Corporate User";
                        break;
                    case 21:
                        //ID = 21 and Name = RPXPMC => Corporate User with RPX
                        item.Name = "Corporate User with RPX";
                        break;
                    case 22:
                        //ID = 22 and Name = RPXProperty Manager => Property Manager with RPX
                        item.Name = "Property Manager with RPX";
                        break;
                    default:
                        //ID = 14 and Name = Property Manager => Property Manager
                        break;
                }
            });
            //return productRoleList.OrderBy(item => item.Name).ToList();
            return productRoleList;
        }
        /// <summary>
        /// List Roles Response
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Levels list</returns>
        public ListResponse ListRolesResponse(long editorPersonaId, long userPersonaId)
        {
            IList<ProductRole> productRoleList = ListRoles(editorPersonaId, userPersonaId);
            ListResponse result = new ListResponse()
            {
                Records = productRoleList.Cast<object>().ToList(),
                TotalRows = productRoleList.Count,
                RowsPerPage = productRoleList.Count,
                TotalPages = 1,
                ErrorReason = ""
            };
            return result;
        }

        /// <summary>
        /// Change user type
        /// </summary>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">new user PersonaId</param>
        /// <param name="rentersInsuranceRoleAndPropertyList">Used to grant a user Role, Properties, and and Is the Product assigned or removed for the user.</param>
        /// <param name="batchProcessType">batchProcess Type</param>
        /// <returns></returns>
        public ObjectOutput<UserAPIResponse, IErrorData> ChangeRentersInsuranceUserType(long createUserPersonaId, long assignUserPersonaId, RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList, BatchProcessType batchProcessType)
        {
            return ManageRentersInsuranceUser(createUserPersonaId, assignUserPersonaId, rentersInsuranceRoleAndPropertyList, out var additionalParameters, batchProcessType);
        }

        /// <summary>
        /// Add or update a Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="rentersInsuranceRoleAndPropertyList">Used to grant a user Role, Properties, and and Is the Product assigned or removed for the user.</param>
        /// <param name="batchProcessType">batchProcess Type</param>
        /// <param name="additionalParameters"></param>
        /// <returns>ObjectOuput and Error</returns>
        public ObjectOutput<UserAPIResponse, IErrorData> ManageRentersInsuranceUser(long editorPersonaId, long userPersonaId, RentersInsuranceRoleAndPropertyList rentersInsuranceRoleAndPropertyList, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            UserProperty userProperty = new UserProperty();
            IList<UserProperty> userPropertyList = new List<UserProperty>();
            GetUserByIDResponse getUserByIDResponse = new GetUserByIDResponse();
            ObjectOutput<UserAPIResponse, IErrorData> output = new ObjectOutput<UserAPIResponse, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", $"Begin create/update user. userPersonaId - {userPersonaId}" });
            additionalParameters = new List<AdditionalParameters>();
            try
            {
                ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", $"Error. userPersonaId - {userPersonaId}. Reason: {listResponse.ErrorReason}" });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = listResponse.ErrorReason;
                    output.Status = errorStatus;
                    return output;
                }
                Persona userPersona = _managePersona.GetPersona(userPersonaId);
                Guid realPageId = userPersona.RealPageId;

                IPerson person = _managePerson.GetPerson(realPageId);

                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                CustomerCompanyMap companyMap = GetRentersInsuranceCompanyInstanceId();
                _companyInstanceSourceId = Convert.ToInt32(companyMap.CompanyInstanceSourceId);

                CompanyPropertyRootObject CompanyProperties = _blueBook.GetCompanyPropertyInstance(companyMap.CompanyInstanceId);
                IList<ProductProperty> blueBookPropertyList = CompanyProperties.MapBlueBookToGBProperties() ?? new List<ProductProperty>();

                string userEmail = string.Empty;
                string userEmailAddress = string.Empty;
                string productUserName = string.Empty;
                if ((userPersonaId > 0) && (IsRegularUserNoEmail(userPersonaId)))
                {
                    productUserName = userLogin.LoginName;
                    // get the email address
                    IList<ElectronicAddress> electronicAddressList = new List<ElectronicAddress>();
                    IManageElectronicAddress manageElectronicAddress = new ManageElectronicAddress();
                    electronicAddressList = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);
                    if (electronicAddressList != null && electronicAddressList.Any(a => a.AddressType.ToUpper() == "EMAIL"))
                    {
                        userEmailAddress = (from a in electronicAddressList where a.AddressType.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                    }
                }
                else
                {
                    userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
                    productUserName = userEmailAddress;
                }

                userEmail = string.IsNullOrWhiteSpace(userEmailAddress) ? userEmailAddress : userEmailAddress.Substring(0, Math.Min(userEmailAddress.Length, 155));

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", $"Begin create/update user userPersonaId - {userPersonaId}" });
                // create user
                if (string.IsNullOrWhiteSpace(_productUserId))
                {
                    // get a login name that isn't in use for the new user
                    bool foundUserName = false;
                    int incrementor = 0;
                    string newproductUsername = productUserName;
                    CheckUserLogin checkUserLogin = new CheckUserLogin()
                    {
                        Login = _username,
                        Password = _password,
                        RequestedBy = _requestedBy,
                        UserLogin = newproductUsername
                    };
                    // give up after 10 tries
                    while (!foundUserName)
                    {
                        if (CheckIfUserLoginIsUsed(checkUserLogin))
                        {
                            incrementor++;
                            string[] loginNameSubStrings = newproductUsername.Split('@');
                            newproductUsername = loginNameSubStrings.Length == 2 ?
                                                 string.Concat(loginNameSubStrings[0], incrementor.ToString(), "@", loginNameSubStrings[1]) :
                                                 string.Concat(newproductUsername, incrementor.ToString());
                            checkUserLogin.UserLogin = newproductUsername;
                        }
                        else
                        {
                            foundUserName = true;
                        }
                    }
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", $"Generated RentersInsuranceLoginName = {newproductUsername}" });
                    productUserName = newproductUsername;
                }
                else
                {
                    productUserName = _productUsername;
                    if (batchProcessType == BatchProcessType.ProfileUpdate)
                    {
                        rentersInsuranceRoleAndPropertyList = new RentersInsuranceRoleAndPropertyList();
                        getUserByIDResponse = GetUserDetail(editorPersonaId, userPersonaId);
                        // if a user record exists
                        if (getUserByIDResponse?.UserInfo != null)
                        {
                            rentersInsuranceRoleAndPropertyList.RoleList = new List<string>()
                            {
                                getUserByIDResponse.UserInfo.RoleID.ToString()
                            };
                        }
                    }
                }

                var userBeforeUpdate = GetUserDetail(editorPersonaId, userPersonaId);

                //User details
                UserInfo userInfo = new UserInfo()
                {
                    CompanyId = Convert.ToInt32(_companyInstanceSourceId),
                    DateLastLogin = null,
                    Email = userEmail,
                    FailedCounter = 0,
                    //GreenBook nvarchar 100 -- Renters Insurance 50
                    FirstName = string.IsNullOrWhiteSpace(person.FirstName) ? person.FirstName : person.FirstName.Substring(0, Math.Min(person.FirstName.Length, 50)),
                    IsActive = true,
                    //GreenBook nvarchar 100 -- Renters Insurance 50
                    LastName = string.IsNullOrWhiteSpace(person.LastName) ? person.LastName : person.LastName.Substring(0, Math.Min(person.LastName.Length, 50)),
                    //If we are adding an Admin and the "RoleList":[] then set the RoleId to 2 = Corporate User
                    RoleID = (rentersInsuranceRoleAndPropertyList.RoleList.Count > 0) ? Convert.ToInt32(rentersInsuranceRoleAndPropertyList.RoleList[0]) : 2,
                    Role = null,
                    UserId = (!string.IsNullOrWhiteSpace(_productUserId)) ? Convert.ToInt32(_productUserId) : 0,
                    User = productUserName
                };

                if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                {
                    if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                    {
                        if ((rentersInsuranceRoleAndPropertyList.PropertyList != null) && ((rentersInsuranceRoleAndPropertyList.PropertyList.Count == 0) || ((rentersInsuranceRoleAndPropertyList.PropertyList.Count == 1) && (rentersInsuranceRoleAndPropertyList.PropertyList[0].ToUpper() == "ALL"))))
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", "BatchProcessType.UserTypeRegularToAdmin Properties - START" });
                            foreach (var property in blueBookPropertyList)
                            {
                                userProperty = new UserProperty()
                                {
                                    PropertyID = Convert.ToInt32(property.ID),
                                    PropertyName = property.Name
                                };
                                userPropertyList.Add(userProperty);
                            }

                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userPropertyList", userPropertyList } }, messageProperties: new object[] { "ManageRentersInsuranceUser", "BatchProcessType.UserTypeRegularToAdmin Properties - END" });
                        }
                    }
                    else if (batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", "BatchProcessType.UserTypeAdminToRegular Properties - START" });
                        foreach (string property in rentersInsuranceRoleAndPropertyList.PropertyList)
                        {
                            var propertyData = blueBookPropertyList.ToList().Find(item => item.ID == property);
                            userProperty = new UserProperty()
                            {
                                PropertyID = Convert.ToInt32(propertyData.ID),
                                PropertyName = propertyData.Name
                            };
                            userPropertyList.Add(userProperty);
                        }
                        WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userPropertyList", userPropertyList } }, messageProperties: new object[] { "ManageRentersInsuranceUser", "BatchProcessType.UserTypeAdminToRegular Properties - END" });
                    }
                }
                else if ((batchProcessType == BatchProcessType.ProfileUpdate) && (getUserByIDResponse?.UserInfo?.PropertyList != null))
                {
                    userPropertyList = getUserByIDResponse.UserInfo.PropertyList;
                }
                else
                {
                    //UI will set {"PropertyList":["all"]} in ProductBatch if Role = Corporate User OR Corporate User with RPX
                    //give user acces to all properties if the User's Role is Corporate User OR Corporate User with RPX
                    if ((rentersInsuranceRoleAndPropertyList.PropertyList != null) && ((rentersInsuranceRoleAndPropertyList.PropertyList.Count == 0) || ((rentersInsuranceRoleAndPropertyList.PropertyList.Count == 1) && (rentersInsuranceRoleAndPropertyList.PropertyList[0].ToUpper() == "ALL"))))
                    {
                        foreach (var property in blueBookPropertyList)
                        {
                            userProperty = new UserProperty()
                            {
                                PropertyID = Convert.ToInt32(property.ID),
                                PropertyName = property.Name
                            };
                            userPropertyList.Add(userProperty);
                        }
                    }
                    else
                    {
                        //give the user access to a list of selected properties if User's Role is Property Manager OR Property Manager with RPX
                        //Property list the user has access to
                        foreach (string property in rentersInsuranceRoleAndPropertyList.PropertyList)
                        {
                            var propertyData = blueBookPropertyList.ToList().Find(item => item.ID == property);
                            userProperty = new UserProperty()
                            {
                                PropertyID = Convert.ToInt32(propertyData.ID),
                                PropertyName = propertyData.Name
                            };
                            userPropertyList.Add(userProperty);
                        }
                    }
                }

                userInfo.PropertyList = userPropertyList.ToArray();

                //API required details
                AddUpdateUserRequest addUpdateUserRequest = new AddUpdateUserRequest()
                {
                    Login = _username,
                    Password = _password,
                    RequestedBy = _requestedBy,
                    User = userInfo
                };

                if (string.IsNullOrWhiteSpace(_productUserId))
                {
                    //Generate a random password when adding a new user.
                    userInfo.Password = Membership.GeneratePassword(20, 5);
                    //Add User
                    _userAPIResponse = _insuranceService.AddUser(addUpdateUserRequest);
                }
                else
                {
                    //Do not update the user Password in Renters Insurance when Updating the user detail
                    userInfo.Password = null;
                    //Update User
                    _userAPIResponse = _insuranceService.UpdateUser(addUpdateUserRequest);
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", $"End create/update user userPersonaId - {userPersonaId}" });

                if ((_userAPIResponse.IsSuccess) && (!string.IsNullOrWhiteSpace(_userAPIResponse.UserId.ToString())))
                {
                    //Create OR Update a new Product UserName SAML attribute for the given personaId
                    //Create OR Update a new Product UserId SAML attribute for the given personaId
                    Dictionary<SamlAttributeEnum, string> userSetting = new Dictionary<SamlAttributeEnum, string>()
                    {
                        {
                            SamlAttributeEnum.productUsername,
                            productUserName
                        },
                        {
                            SamlAttributeEnum.UserId,
                            _userAPIResponse.UserId.ToString()
                        }
                    };
                    UpdateSamlUserAttributes(userPersonaId, userSetting);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", "Setting product result to success" });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageRentersInsuranceUser", $"End create/update user for user with editorPersona id - {editorPersonaId}" });
                    errorStatus.Success = true;
                    errorStatus.ErrorMsg = "";
                    output.obj = _userAPIResponse;
                    output.Status = errorStatus;

                    if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin || batchProcessType == BatchProcessType.UserTypeAdminToRegular || batchProcessType == BatchProcessType.UserTypeAdminToExternal || batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
                    {
                        WriteUpdateUserTypeActivityLog(editorPersonaId, (Person)person, userLogin, batchProcessType);
                    }

                    //Activity Logs
                    //Roles
                    if(userBeforeUpdate.UserInfo.RoleID != userInfo.RoleID)
                    {
                        var roles = ListRoles(editorPersonaId, userPersonaId);
                        additionalParameters.Add(new AdditionalParameters { Key = "Renters Insurance Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", roles.FirstOrDefault(e => e.ID == userInfo.RoleID.ToString()).Name) });
                        if (userBeforeUpdate?.UserInfo?.RoleID != null)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Renters Insurance Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", roles.FirstOrDefault(e => e.ID == userBeforeUpdate?.UserInfo?.RoleID.ToString()).Name) });
                        }
                    }

                    //Properties
                    var oldProperties = userBeforeUpdate.UserInfo.PropertyList != null ? userBeforeUpdate.UserInfo.PropertyList.Select(s => s.PropertyID) : new List<int>();
                    var newProperties = userInfo.PropertyList.Select(s => s.PropertyID);

                    var removedProperties = oldProperties.Except(newProperties).ToList();
                    var addedProperties = newProperties.Except(oldProperties).ToList();

                    if (removedProperties.Any())
                    {
                        foreach (int p in removedProperties)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Renters Insurance Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", blueBookPropertyList.FirstOrDefault(f => f.ID == p.ToString()).Name) });
                        }
                    }
                    if (addedProperties.Any())
                    {
                        foreach (int p in addedProperties)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Renters Insurance Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", blueBookPropertyList.FirstOrDefault(f => f.ID == p.ToString()).Name) });
                        }
                    }
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", JsonConvert.SerializeObject(_userAPIResponse) } }, messageProperties: new object[] { "ManageRentersInsuranceUser", $"Failed to create/update a renters insurance user for userPersonaId - {userPersonaId}" });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "Failed to create a renters insurance user.";
                    output.Status = errorStatus;
                }
                return output;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageRentersInsuranceUser", $"Error for user with editorPersona id - {editorPersonaId} Error: {ex.Message}" });
                errorStatus.Success = false;
                errorStatus.ErrorMsg = $"Error - {ex.Message}";
                output.Status = errorStatus;
                return output;
            }
        }

        /// <summary>
        /// Unassign User in GreenBook and disable in Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>ObjectOutput object</returns>
        public ObjectOutput<UserAPIResponse, IErrorData> UnassignRentersInsuranceUser(long editorPersonaId, long userPersonaId)
        {
            ObjectOutput<UserAPIResponse, IErrorData> output = new ObjectOutput<UserAPIResponse, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                errorStatus.Success = false;
                errorStatus.ErrorMsg = listResponse.ErrorReason;
                output.Status = errorStatus;
                return output;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignRentersInsuranceUser", $"Begin. userPersonaId: {userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

            //Call Renters Insurance Disable User API
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userId", _productUserId } }, messageProperties: new object[] { "UnassignRentersInsuranceUser", "Delete user" });
                UserActionRequest userActionRequest = new UserActionRequest()
                {
                    Login = _username,
                    Password = _password,
                    RequestedBy = _requestedBy,
                    UserId = Convert.ToInt32(_productUserId)
                };

                _userAPIResponse = _insuranceService.DisableUser(userActionRequest);
                if ((_userAPIResponse.IsSuccess) && (!string.IsNullOrWhiteSpace(_userAPIResponse.UserId.ToString())))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", JsonConvert.SerializeObject(_userAPIResponse) } }, messageProperties: new object[] { "UnassignRentersInsuranceUser", "Response" });
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                string errorMsg = "ManageProductRentersInsurance.UnassignRentersInsuranceUser - Error " + ex.Message;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UnassignRentersInsuranceUser", $"Error for user with editorPersona id - {editorPersonaId} userPersonaId - {userPersonaId} Error: {ex.Message}" });
                errorStatus.Success = false;
                errorStatus.ErrorMsg = errorMsg;
                output.Status = errorStatus;
                return output;
            }


            errorStatus.Success = true;
            errorStatus.ErrorMsg = "";
            output.Status = errorStatus;
            output.obj = _userAPIResponse;
            return output;
        }

        /// <summary>
        /// Disable User in Renters Insurance
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>ObjectOutput object</returns>
        public ObjectOutput<UserAPIResponse, IErrorData> UnlockRentersInsuranceUser(long editorPersonaId, long userPersonaId)
        {
            ObjectOutput<UserAPIResponse, IErrorData> output = new ObjectOutput<UserAPIResponse, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                errorStatus.Success = false;
                errorStatus.ErrorMsg = listResponse.ErrorReason;
                output.Status = errorStatus;
                return output;
            }

            //Call Renters Insurance Unlock User API
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userId", _productUserId } }, messageProperties: new object[] { "UnlockRentersInsuranceUser", "Delete user" });
                UserActionRequest userActionRequest = new UserActionRequest()
                {
                    Login = _username,
                    Password = _password,
                    RequestedBy = _requestedBy,
                    UserId = Convert.ToInt32(_productUserId)
                };

                _userAPIResponse = _insuranceService.UnlockUser(userActionRequest);
                if ((_userAPIResponse.IsSuccess) && (!string.IsNullOrWhiteSpace(_userAPIResponse.UserId.ToString())))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", JsonConvert.SerializeObject(_userAPIResponse) } }, messageProperties: new object[] { "UnlockRentersInsuranceUser", "Response" });
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                errorStatus.Success = false;
                errorStatus.ErrorMsg = "ManageProductRentersInsurance.UnlockRentersInsuranceUser - Error " + ex.Message;
                output.Status = errorStatus;
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UnlockRentersInsuranceUser", $"Error for user with editorPersona id - {editorPersonaId} userPersonaId - {userPersonaId} Error: {ex.Message}" });
                return output;
            }

            errorStatus.Success = true;
            errorStatus.ErrorMsg = "";
            output.Status = errorStatus;
            output.obj = _userAPIResponse;
            return output;
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
            var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError) { response.ErrorReason = claimResponse.ErrorReason; return response; }

            try
            {

                var companyInstanceSourceId = GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId;
                if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                    response.ErrorReason = "Company Setup Error: Please Contact Support.";
                    return response;
                }
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

                var userActionByPMCIDRequest = new UserActionByPMCIDRequest()
                {
                    CompanyId = companyInstanceSourceId,
                    Login = _username,
                    Password = _password,
                    RequestedBy = _requestedBy,
                    FilterType = filter,
                    StartRow = startRow,
                    Resultsperpage = resultPerRow
                };
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object> { { "request", $"{companyInstanceSourceId}, {filter}, {startRow}, {resultPerRow}" } }, messageProperties: new object[] { "GetMigrationUsers", "GetMigrationUsers" });

                var allUsers = _insuranceService.GetUsersByPMC(userActionByPMCIDRequest);
                if (allUsers == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}" });
                    return response;
                }

                var migrationUsers = new List<MigrationUser>();
                foreach (var user in allUsers.UserList)
                {
                    var migrationUser = new MigrationUser
                    {
                        CompanyInstanceSourceId = companyInstanceSourceId,
                        UserId = user.UserId.ToString(),
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Username = user.User,
                        Email = user.Email,
                        LastActivity = user.DateLastLogin,
                        Status = user.IsActive ? "Active" : "Disabled"
                    };
                    if (user.PropertyList != null && user.PropertyList.Length > 0)
                    {
                        foreach (var property in user.PropertyList)
                        {
                            migrationUser.Properties.Add(new MigrationProperty() { PropertyInstanceSourceId = property.PropertyID.ToString() });
                        }
                    }
                    migrationUsers.Add(migrationUser);
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"GetUsers - Received users from product. editorPersona id - {editorPersonaId}" });
                
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

                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetMigrationUsers", $"Error for user with editorPersona id - {editorPersonaId} Error: {ex.Message}" });
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

            var claimResponse = base.GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (claimResponse.IsError) { migrateResponse.Message = claimResponse.ErrorReason; return migrateResponse; }

            try
            {
                string companyInstanceSourceId = GetProductCompanyInstanceId(_udmSourceCode).CompanyInstanceSourceId;
                if (string.IsNullOrWhiteSpace(companyInstanceSourceId))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                    migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                    return migrateResponse;
                }

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "migrateUsers", JsonConvert.SerializeObject(migrateUsers) } }, messageProperties: new object[] { "UpdateUsersMigrationStatus", "Begin" });

                var migrateUserRequests = migrateUsers.Select(mu =>
                {
                    return new MigrateUserrequest()
                    {
                        unifiedLoginUserName = mu.UnifiedLoginUserName,
                        userid = mu.UserId,
                        usingUnifiedLogin = mu.UsingUnifiedLogin.ToString()
                    };
                });
                var migratedArray = migrateUserRequests.ToArray();

                migrateResponse.Message = _insuranceService.MigrateUser(migratedArray);
                migrateResponse.Status = true;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Result: {migrateResponse.Message}" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error for user with editorPersona id - {editorPersonaId} Error: {ex.Message}" });
                migrateResponse.Message = ex.Message;
                migrateResponse.Status = false;
            }

            return migrateResponse;
        }

        #region User-Status

        /// <summary>
        /// Changes the user status.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="userId">The user Id.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, int userId, bool isActive = false)
        {
            ListResponse listResponse = new ListResponse();
            UserAPIResponse userAPIResponse = null;
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError)
            {
                return false;
            }

            UserActionRequest userActionRequest = new UserActionRequest()
            {
                Login = _username,
                Password = _password,
                RequestedBy = _requestedBy,
                UserId = userId
            };

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Updating user status for user = {userId}, isActive = {isActive}" });

                if (isActive)
                    userAPIResponse = _insuranceService.EnableUser(userActionRequest);
                else
                    userAPIResponse = _insuranceService.DisableUser(userActionRequest);

                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "apiResponse", JsonConvert.SerializeObject(userAPIResponse) } }, messageProperties: new object[] { "ChangeUserStatus", "Response" });

                if (userAPIResponse == null || !userAPIResponse.IsSuccess || string.IsNullOrWhiteSpace(userAPIResponse.UserId.ToString()))
                    return false;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeUserStatus", $"Error. user {userId} editorPersonaId - {editorPersonaId} Reason: {ex.Message}" });
                return false;
            }

            return true;
        }

        #endregion

        #endregion

        #region Private Methods
        /// <summary>
        /// Get Renters Insurance Company InstanceId from BlueBook
        /// </summary>
        /// <returns>CompanyMap object</returns>
        private CustomerCompanyMap GetRentersInsuranceCompanyInstanceId()
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetRentersInsuranceCompanyInstanceId", "Begin" });
            //IList<CompanyMap> companyProductList = _blueBook.GetCompanyMap(_editorPersona.Organization.BooksMasterId, BlueBookProductConstants.Insurance);
            IList<CustomerCompanyMap> companyProductList = _blueBook.GetCompanyMap(_editorPersona.Organization.RealPageId, _editorPersona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.Insurance, domain: _editorPersona.Organization.OrganizationDomain.Name, useTranslate: false);
            CustomerCompanyMap company = new CustomerCompanyMap();
            if (companyProductList.Any(a => a.Source.ToUpper() == BlueBookProductConstants.Insurance))
            {
                company = (from a in companyProductList where a.Source.ToUpper() == BlueBookProductConstants.Insurance select a).FirstOrDefault();
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "companyProductList", JsonConvert.SerializeObject(companyProductList) } }, messageProperties: new object[] { "GetRentersInsuranceCompanyInstanceId", "Got info" });
            return company;
        }

        /// <summary>
        /// Used to get details about a Renters Insurance user
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">User PersonaId</param>
        /// <returns>GetUserByIDResponse object</returns>
        private GetUserByIDResponse GetUserDetail(long editorPersonaId, long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetail", $"Begin. userPersonaId - {userPersonaId}" });
            GetUserByIDResponse GetUserByIDResponse = new GetUserByIDResponse();
            ListResponse listResponse = new ListResponse();

            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetail", $"Error. editorPersona id - {editorPersonaId}. Reason: {listResponse.ErrorReason}" });
                return GetUserByIDResponse;
            }

            UserActionRequest userActionRequest = new UserActionRequest()
            {
                Login = _username,
                Password = _password,
                RequestedBy = _requestedBy,
                UserId = Convert.ToInt32(_productUserId)
            };
            return _insuranceService.GetUserByID(userActionRequest);
        }

        /// <summary>
        /// merge the given user details with the list
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="blueBookPropertyList">blueBook Property List</param>
        /// <returns>ListResponse object</returns>
        private ListResponse MergeProductPropertiesWithGreenbook(long editorPersonaId, long userPersonaId, IList<ProductProperty> blueBookPropertyList)
        {
            bool allProperties = false;
            Dictionary<string, bool> additionalDictionary = new Dictionary<string, bool>();

            List<ProductProperty> propertyList = new List<ProductProperty>();
            propertyList = blueBookPropertyList.ToList();
            // merge the given user details with the list
            GetUserByIDResponse getUserByIDResponse = GetUserDetail(editorPersonaId, userPersonaId);

            // if a user record exists
            if (getUserByIDResponse?.UserInfo?.PropertyList != null)
            {
                propertyList.ToList().ForEach(blueBook => blueBook.IsAssigned = getUserByIDResponse.UserInfo.PropertyList.Any(rentersInsurance => rentersInsurance.PropertyID.ToString() == blueBook.ID));
            }

            //"Allow access to all current and future propeties" On or Off?
            additionalDictionary.Add("allProperties", allProperties);

            return new ListResponse()
            {
                Records = propertyList.Cast<object>().ToList(),
                TotalRows = blueBookPropertyList.Count(),
                RowsPerPage = 9999,
                ErrorReason = string.Empty,
                TotalPages = 1,
                Additional = additionalDictionary
            };
        }

        /// <summary>
        /// Used to see if a new user login being added already exists or not
        /// </summary>
        /// <param name="checkUserLogin"></param>
        /// <returns></returns>
        private bool CheckIfUserLoginIsUsed(CheckUserLogin checkUserLogin)
        {
            bool userExists = false;
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckIfUserLoginIsUsed", $"Login - {checkUserLogin}" });
                var result = _insuranceService.CheckUserLogin(checkUserLogin);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "CheckIfUserLoginIsUsed", $"result={result}" });
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "result", JsonConvert.SerializeObject(result) } }, messageProperties: new object[] { "CheckIfUserLoginIsUsed", "Response" });
                if (result != null && result.ErrorCode == "-1")
                {
                    userExists = true;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "CheckIfUserLoginIsUsed", $"Error. Result: {ex.Message}" });
            }
            return userExists;
        }
        #endregion
    }

    /// <summary>
    /// Used to build the XML required to call the Renters Insurance web services
    /// </summary>
    public static class ManageProductRentersInsuranceHelpers
    {
        /// <summary>
        /// Used to convert a OneSite property into a GreenBook role to be used by the UI
        /// </summary>
        /// <param name="properties">The list of properties to convert</param>
        /// <returns></returns>
        public static IList<ProductProperty> ToGBProperties(this List<UserProperty> properties)
        {
            if (properties == null)
            {
                return null;
            }
            IList<ProductProperty> results = new List<ProductProperty>();
            //Loop through the Renters Insurance user properties
            foreach (UserProperty userProperty in properties)
            {
                results.Add(new ProductProperty
                {
                    Name = userProperty.PropertyName,
                    ID = userProperty.PropertyID.ToString(),
                    IsAssigned = true
                });
            }
            return results;
        }

        /// <summary>
        /// Used to convert a Renters Insurance role into a GreenBook role to be used by the UI
        /// </summary>
        /// <param name="roles">The list of roles to convert</param>
        /// <returns></returns>
        public static IList<ProductRole> ToGBRoles(this ListOfUserRolesResponse roles)
        {
            if (roles?.UserRoleList == null)
            {
                return null;
            }
            IList<ProductRole> results = new List<ProductRole>();
            foreach (UserRole role in roles.UserRoleList)
            {
                results.Add(new ProductRole
                {
                    Name = role.RoleName,
                    ID = role.RoleID.ToString(),
                    IsAssigned = false
                });
            }
            return results;
        }
    }
}