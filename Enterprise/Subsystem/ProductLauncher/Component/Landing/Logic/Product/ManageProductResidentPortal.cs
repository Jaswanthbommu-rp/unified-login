using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Used to update Resident Portal user information
    /// </summary>
    public class ManageProductResidentPortal : ManageProductBase, IManageProductResidentPortal
    {
        #region Private Constants
        private const int MAXRETRYCOUNT = 5;
        #endregion

        #region Private Variables
        private readonly string _residentPortalApiEndPoint;
        private readonly string _mtApiEndPoint;
        private readonly string _appId;
        private readonly string _appKey;
        private string _accessToken;
        private readonly string _forwardedProtocol = "https";
        private long _companyInstanceSourceId;
        private long _companyInstanceId;
        private long _communityId;
        private ITokenHelper _tokenHelper;
        private readonly RPObjectCache _manageResidentPortalCache = new RPObjectCache();
        private ListResponse _listResponse = new ListResponse();
        private List<ILevel> _levelList = new List<ILevel>();
        private Notifications _notifications = new Notifications();
        private ResidentPortalUser _residentPortalUser = new ResidentPortalUser();
        private ResidentPortalUser _residentPortalEditorUser = new ResidentPortalUser();
        private readonly DefaultUserClaim _userClaims;
        #endregion

        #region Constructor
        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="userClaims">User Claim</param>
        public ManageProductResidentPortal(DefaultUserClaim userClaims) : base((int)ProductEnum.ResidentPortal, userClaims, productInternalSettingRepository: null, productRepository: null)
        {
            _productId = (int)ProductEnum.ResidentPortal;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaims);

            _residentPortalApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            _client.BaseAddress = new Uri(_residentPortalApiEndPoint);
            _appId = _productInternalSettingList.First(a => a.Name.ToUpper() == "APPID").Value;
            _appKey = _productInternalSettingList.First(a => a.Name.ToUpper() == "APPKEY").Value;
            _userClaims = userClaims;
        }

        /// <summary>
        /// Unit test constructor to test Levels, Notifications, and MessageGroups
        /// </summary>
        /// <param name="editorRealPageId">The RealPageId of the editor</param>
        /// <param name="residentPortalEditorUser">Resident Portal editor User object</param>
        /// <param name="residentPortalUser">Resident Portal User object</param>
        /// <param name="samlRepository">SAML Repository</param>
        /// <param name="managePersona">Persona business logic</param>
        /// <param name="manageBlueBook">BlueBook business logic</param>
        /// <param name="productRepository">Product Repository</param>
        /// <param name="productInternalSettingRepository">Product Internal Setting Repository</param>
        /// <param name="managePerson">Person business logic</param>
        /// <param name="manageUserLogin">UserLogin business logic</param>
        /// <param name="managePartyRelationship">Party Relationship business logic</param>
        /// <param name="manageElectronicAddress">Electronic Address business logic</param>
        /// <param name="userClaim">Used the hold user claim related info</param>
        /// <param name="messageHandler">A base type for HTTP message handlers</param>
        /// <param name="repository">Repository used for moq'ing sql data</param>
        public ManageProductResidentPortal(Guid editorRealPageId, ResidentPortalUser residentPortalEditorUser, ResidentPortalUser residentPortalUser,
            ISamlRepository samlRepository, IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository, IManagePerson managePerson, IManageUserLogin manageUserLogin,
            IManagePartyRelationship managePartyRelationship, IManageElectronicAddress manageElectronicAddress, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, IRepository repository)
            : base((int)ProductEnum.ResidentPortal, userClaim, repository, messageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _residentPortalEditorUser = residentPortalEditorUser;
            _residentPortalUser = residentPortalUser;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _managePerson = managePerson;
            _manageUserLogin = manageUserLogin;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
            _manageElectronicAddress = manageElectronicAddress;
            _userClaims = userClaim;
            _messageHandler = messageHandler;
            _client = new HttpClient(messageHandler, false);

            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            _appId = _productInternalSettingList.First(a => a.Name.ToUpper() == "APPID").Value;
            _appKey = _productInternalSettingList.First(a => a.Name.ToUpper() == "APPKEY").Value;
        }

        /// <summary>
        /// Unit test constructor to test list properties
        /// </summary>
        /// <param name="editorRealPageId">The RealPageId of the editor</param>
        /// <param name="companyInstanceId">Company Id</param>
        /// <param name="samlRepository">SAML Repository</param>
        /// <param name="managePersona">Persona business logic</param>
        /// <param name="manageBlueBook">BlueBook business logic</param>
        /// <param name="productRepository">Product Repository</param>
        /// <param name="productInternalSettingRepository">Product Internal Setting Repository</param>
        /// <param name="managePerson">Person business logic</param>
        /// <param name="manageUserLogin">UserLogin business logic</param>
        /// <param name="managePartyRelationship">Party Relationship business logic</param>
        /// <param name="userClaim">Used the hold user claim related info</param>
        /// <param name="messageHandler"></param>
        /// <param name="repository"></param>
        public ManageProductResidentPortal(Guid editorRealPageId, long companyInstanceId, ISamlRepository samlRepository, IManagePersona managePersona,
                IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository,
                IManagePerson managePerson, IManageUserLogin manageUserLogin, IManagePartyRelationship managePartyRelationship, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, IRepository repository)
            : base((int)ProductEnum.ResidentPortal, userClaim, repository, messageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _companyInstanceId = companyInstanceId;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _managePerson = managePerson;
            _manageUserLogin = manageUserLogin;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
            _userClaims = userClaim;
            _productRepository = productRepository;
            _client = new HttpClient(messageHandler, false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get Notification Settings
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Notifications object</returns>
        public Notifications GetNotificationSettings(long editorPersonaId, long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetNotificationSettings", $"Beginning of method for user with editorPersonaId id - {editorPersonaId}" });

            try
            {
                _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if ((!_listResponse.IsError) && (_residentPortalUser.Notifications == null) && !string.IsNullOrWhiteSpace(_productUsername))
                {
                    _residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
                    if ((_residentPortalUser != null) && (_residentPortalUser.Notifications != null))
                    {
                        _notifications = _residentPortalUser.Notifications;
                    }
                    else
                    {
                        RolePropertyList roleproperty = GetDeactivatedProductBatchData(userPersonaId);

                        if (roleproperty != null && roleproperty.Notifications != null)
                        {
                            _notifications = roleproperty.Notifications;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetNotificationSettings", $"Error - {ex.Message}" }, exception: ex);
                return null;
            }

            return _notifications;
        }

        /// <summary>
        /// Initialize and Set the MessageGroups and Level objects for UI
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">User PersonaId</param>
        /// <param name="residentPortalUser">residentPortalUser object</param>
        /// <returns>residentPortalUser object</returns>
        public IResidentPortalUser SetLevelAndGroupObjects(long editorPersonaId, long userPersonaId, IResidentPortalUser residentPortalUser)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "SetLevelAndGroupObjects", $"Beginning of method for user with editorPersonaId id - {editorPersonaId}" });

            try
            {
                if (residentPortalUser.MessageGroups != null)
                {
                    List<IMessagingGroups> messageGroupeList = ListMessageGroups(editorPersonaId, userPersonaId);
                    foreach (string messageGroup in residentPortalUser.MessageGroups)
                    {
                        messageGroupeList.Find(item => item.Id == messageGroup).IsAssigned = true;
                    }
                    residentPortalUser.MessageGroups = null;
                    residentPortalUser.MessagingGroups = messageGroupeList;
                }
                List<ILevel> levelList = ListLevels(editorPersonaId, userPersonaId);
                residentPortalUser.Level = null;
                residentPortalUser.Levels = levelList;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "SetLevelAndGroupObjects", $"Error - {ex.Message}" }, exception: ex);
            }
            return residentPortalUser;
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
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"Beginning of method for user with editorPersona id - {editorPersonaId}" });

            try
            {
                _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (_listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"GetCompanyEditorAndUserDetails error for user with editorPersona id - {editorPersonaId} - {_listResponse.ErrorReason}" });
                    return _listResponse;
                }

                CustomerCompanyMap companyMap = GetProductCompanyInstanceId(_udmSourceCode);
                _companyInstanceSourceId = Convert.ToInt32(companyMap.CompanyInstanceSourceId);

                IList<ResidentPortalProperty> propertyProductList = ListResidentPortalProperties();

                if ((propertyProductList == null) || (propertyProductList.Count == 0))
                {
                    _listResponse.IsError = true;
                    _listResponse.ErrorReason = $"ManageProductResidentPortal.ManageResidentPortalUser Error for user userPersonaId - {userPersonaId}. Error - No active properties found {propertyProductList}";
                    return _listResponse;
                }

                //Used to convert a BlueBook property into a GreenBook property (ID, Name, State)
                IList<ProductProperty> residentPortalPropertyList = propertyProductList.ToGBProperties()?.OrderBy(x => x.Name).ToList();
                if (residentPortalPropertyList == null)
                {
                    residentPortalPropertyList = new List<ProductProperty>();
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"ToGBProperties completed for user with editorPersona id - {editorPersonaId}." });

                //called during updating Existing User to flag the properties the user has access to.
                if ((userPersonaId != 0) && (!string.IsNullOrWhiteSpace(_productUsername)))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"Calling MergeProductPropertiesWithGreenbook for user with editorPersona id -{editorPersonaId} & _productUsername-{_productUsername}." });
                    _listResponse = MergeProductPropertiesWithGreenbook(editorPersonaId, userPersonaId, residentPortalPropertyList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"MergeProductPropertiesWithGreenbook completed for user with editorPersona id -{editorPersonaId}." });
                }
                else
                {
                    Dictionary<string, bool> additionalDictionary = new Dictionary<string, bool>
                    {
                        { "displayAllProperties", true },
                        { "allProperties", false }
                    };

                    _listResponse = new ListResponse()
                    {
                        Records = residentPortalPropertyList.Cast<object>().ToList(),
                        TotalRows = residentPortalPropertyList.Count,
                        RowsPerPage = residentPortalPropertyList.Count,
                        TotalPages = 1,
                        ErrorReason = string.Empty,
                        Additional = additionalDictionary
                    };
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ListProperties", $"There was a problem getting the properties for user with editorPersona id - {editorPersonaId}." }, exception: ex);

                _listResponse = new ListResponse()
                {
                    IsError = true
                };

                if (ex is BlueBookException)
                {
                    _listResponse.ErrorReason = ex.Message;
                }
                else
                {
                    _listResponse.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }
            }
            return _listResponse;
        }

        /// <summary>
        /// Get Resident Portal Enterprise User Details
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>ResidentPortal object</returns>
        public ResidentPortalUser GetUser(long editorPersonaId, long userPersonaId)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUser", $"Begin get enterprise user details - {userPersonaId}." });

            _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (_listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {_listResponse.ErrorReason}" });
                return _residentPortalUser;
            }
            if (IsSuperUser(userPersonaId))
            {
                _residentPortalUser.Title = null;
                _residentPortalUser.OfficePhone = null;
                _residentPortalUser.MobilePhone = null;
                _residentPortalUser.DisplayInCorner = null;
                _residentPortalUser.DateCreated = null;
                _residentPortalUser.DateUpdated = null;
                _residentPortalUser.MessageGroups = null;
            }

            return GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
        }

        /// <summary>
        /// Add or update a Resident Portal
        /// 1. SuperUser (enterprise) who has access to all communities for a PMC 
        /// OR
        /// 2. Manager/Staff
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="residentPortal">Used to grant a user level, set the Messaging groups, and Is the Product assigned or removed for the user.</param>
        /// <param name="batchProcessType">batchProcess Type</param>
        /// <param name="additionalParameters"></param>
        /// <returns>ObjectOuput and Error</returns>
        public ObjectOutput<IResidentPortalUser, IErrorData> ManageResidentPortalUser(long editorPersonaId, long userPersonaId, ResidentPortal residentPortal, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser)
        {
            additionalParameters = new List<AdditionalParameters>();
            //CommunityIds to Assign
            List<long> communityIds = new List<long>();
            //List of Communities Staff user currently has access to
            List<Community> communityList = new List<Community>();
            //CommunityIds list did not get assigned to the user
            Dictionary<long, string> errorCommunityIds = new Dictionary<long, string>();
            Dictionary<string, object> logData = new Dictionary<string, object>();
            IPartyRoleRepository partyRoleRepository = new PartyRoleRepository();
            ObjectOutput<IResidentPortalUser, IErrorData> output = new ObjectOutput<IResidentPortalUser, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            string createUpdateUser = "create";
            bool IsEnterprise = false;
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Begin user provisioning with userPersonaId - {userPersonaId}." });

            try
            {
                ListResponse listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (listResponse.IsError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error for user userPersonaId - {userPersonaId}. Error - {listResponse.ErrorReason}" });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = listResponse.ErrorReason;
                    output.Status = errorStatus;
                    return output;
                }

                Persona userPersona = _managePersona.GetPersona(userPersonaId);
                Guid realPageId = userPersona.RealPageId;
                IPerson person = _managePerson.GetPerson(realPageId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(realPageId);

                CustomerCompanyMap companyMap = GetProductCompanyInstanceId(_udmSourceCode);
                if ((companyMap != null) && (Convert.ToInt32(companyMap.CompanyInstanceSourceId) > 0))
                {
                    _companyInstanceSourceId = Convert.ToInt32(companyMap.CompanyInstanceSourceId);
                }
                else
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error for user userPersonaId - {userPersonaId}. Error - Get CompanyMap - greenBookCares not enabled {companyMap}" });

                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "Company Setup Error: Please Contact Support.";
                    output.Status = errorStatus;

                    return output;
                }

                IList<ResidentPortalProperty> propertyProductList = ListResidentPortalProperties();
                if ((propertyProductList == null) || (propertyProductList.Count == 0))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error for user userPersonaId - {userPersonaId}. Error - No active properties found." });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "List properties from Resident Portal - No active properties found.";
                    output.Status = errorStatus;
                    return output;
                }

                // get the email address
                string userEmailAddress = string.Empty;
                List<ILevel> oldRoles = new List<ILevel>();
                List<ProductProperty> oldProperties = new List<ProductProperty>();
                List<IMessagingGroups> oldMessageGroups = new List<IMessagingGroups>();
                Notifications oldNotifications = new Notifications();
                if (!string.IsNullOrEmpty(_productUsername))
                {
                    oldRoles = ListLevels(editorPersonaId, userPersonaId);
                    var olsPropList = ListProperties(editorPersonaId, userPersonaId, new RequestParameter());
                    if (olsPropList.Records != null)
                    {
                        oldProperties = olsPropList.Records.Cast<ProductProperty>().ToList();
                    }
                    oldMessageGroups = ListMessageGroups(editorPersonaId, userPersonaId);
                    oldNotifications = GetNotificationSettings(editorPersonaId, userPersonaId);
                }

                if ((userPersonaId > 0) && (IsRegularUserNoEmail(userPersonaId)))
                {
                    IManageElectronicAddress manageElectronicAddress = new ManageElectronicAddress();
                    IList<ElectronicAddress> electronicAddressList = manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, string.Empty);
                    if (electronicAddressList != null && electronicAddressList.Any(a => a.AddressType.ToUpper() == "EMAIL"))
                    {
                        userEmailAddress = (from a in electronicAddressList where a.AddressType.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                    }
                    userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
                    String[] emailSubstrings = userEmailAddress.Split('@');
                    if (emailSubstrings.Length == 2)
                    {
                        userEmailAddress = string.Concat(emailSubstrings[0], "+ul", userLogin.LoginName.Replace("@", ""), "ul@", emailSubstrings[1]);
                    }
                }
                else
                {
                    //user GreenBook UserLogin.LoginName
                    userEmailAddress = ValidateAndReturnEmailAddress(userLogin.LoginName);
                }

                // create user
                if (string.IsNullOrWhiteSpace(_productUsername))
                {
                    IsEnterprise = ((residentPortal.RoleList.Count == 0) || ((residentPortal.RoleList.Count == 1) && (residentPortal.RoleList[0].ToUpper().StartsWith("ENTERPRISE"))));
                    _productUsername = userEmailAddress;
                    _residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
                    if (_residentPortalUser != null && _companyInstanceSourceId != _residentPortalUser.CompanyId)
                    {
                        string[] loginNameSubStrings = userEmailAddress.Split('@');
                        userEmailAddress = loginNameSubStrings.Length == 2 ? string.Concat(loginNameSubStrings[0], "+ul", _companyInstanceSourceId.ToString(), "ul@", loginNameSubStrings[1]) :
                                                                             string.Concat(userEmailAddress, "+ul", _companyInstanceSourceId.ToString());

                        _productUsername = userEmailAddress;
                    }
                    _residentPortalUser = new ResidentPortalUser();
                }
                else
                {
                    createUpdateUser = "update";
                    userEmailAddress = _productUsername;

                    _residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);

                    if (_residentPortalUser == null)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorMsg = "Error: User not found in Resident Portal";
                        output.Status = errorStatus;
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", errorStatus.ErrorMsg });
                        return output;
                    }

                    //List of Communities Staff user currently has access to
                    if ((_residentPortalUser.Communities != null) && (_residentPortalUser?.ManagerId > 0))
                    {
                        communityList = _residentPortalUser.Communities;
                    }
                    IsEnterprise = (_residentPortalUser?.EnterpriseUserId > 0);

                    //Not applicable to Profile Update
                    if (batchProcessType != BatchProcessType.ProfileUpdate)
                    {
                        //Update user Role from Enterprise to Staff
                        bool enterpriseToStaff = ((_residentPortalUser?.EnterpriseUserId > 0) && (residentPortal.RoleList[0].ToUpper().StartsWith("STAFF")));
                        //Update user Role from Staff to Enterprise
                        bool staffToEnterprise = ((_residentPortalUser?.ManagerId > 0) && (residentPortal.RoleList[0].ToUpper().StartsWith("ENTERPRISE")));
                        if (enterpriseToStaff || staffToEnterprise)
                        {
                            output = UnassignResidentPortalUser(editorPersonaId, userPersonaId);
                            if (!output.Status.Success)
                            {
                                output.Status.ErrorMsg += "  Unable to switch the user role ";
                                if (enterpriseToStaff)
                                {
                                    output.Status.ErrorMsg += "from Enterprise to Staff.";
                                }
                                else
                                {
                                    output.Status.ErrorMsg += "from Staff to Enterprise.";
                                }
                                return output;
                            }
                        }
                    }
                }

                bool gotUnifiedLoginToken = GetUnifiedLoginAccessToken();
                if (!gotUnifiedLoginToken)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "Failed to get a Unified Login access token";
                    output.Status = errorStatus;
                    return output;
                }

                //Create the Enterprise User data
                ResidentPortalUser residentPortalUser = new ResidentPortalUser()
                {
                    Email = userEmailAddress,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                };

                //Profile Update First and/or Last names
                if (batchProcessType == BatchProcessType.ProfileUpdate)
                {
                    residentPortalUser.Level = _residentPortalUser.Level;
                    residentPortalUser.Notifications = _residentPortalUser.Notifications;
                    if (IsEnterprise)
                    {
                        residentPortalUser.CompanyId = _residentPortalUser.CompanyId;
                        residentPortalUser.CommunityAccessLevel = _residentPortalUser.CommunityAccessLevel;
                        residentPortalUser.Groups = new List<string>();
                        if (residentPortalUser.CommunityAccessLevel == "ALL")
                        {
                            //Enterprise Level: ADMIN
                            communityIds.Add(Convert.ToInt64(propertyProductList[0].CommunityId));
                            residentPortalUser.CommunityIds = null;
                        }
                        else
                        {
                            //Enterprise Level: STANDARD
                            residentPortalUser.CommunityIds = _residentPortalUser.CommunityIds;
                            communityIds = residentPortalUser.CommunityIds;
                        }
                    }
                    else
                    {
                        //Staff Level: ADMIN, STANDARD, or LIMITED
                        residentPortalUser.Groups = _residentPortalUser.MessageGroups;
                        residentPortalUser.Title = string.IsNullOrWhiteSpace(_residentPortalUser.Title) ? "Property Staff" : _residentPortalUser.Title;
                        PartyRole partRole = partyRoleRepository.GetPartyRoleByEnterpriseUserID(realPageId);

                        residentPortalUser.Title = !string.IsNullOrEmpty(person.Title) ? person.Title
                                                   : partRole != null && !partRole.Name.Equals(residentPortalUser.Title, StringComparison.OrdinalIgnoreCase) ? partRole.Name : residentPortalUser.Title;

                        foreach (Community community in _residentPortalUser.Communities)
                        {
                            communityIds.Add(Convert.ToInt64(community.CommunityId));
                        }
                    }
                }
                else
                {
                    if (IsEnterprise)
                    {
                        //Enterprise Level: ADMIN, or STANDARD
                        if ((residentPortal.RoleList != null) && (residentPortal.RoleList.Count == 1))
                        {
                            //{"RoleList":["ENTERPRISEADMIN"]} OR {"RoleList":["ENTERPRISESTANDARD"]}
                            residentPortalUser.Level = residentPortal.RoleList[0].ToUpper().Replace("ENTERPRISE", "");
                        }
                        else
                        {
                            //{"RoleList":[]}
                            residentPortalUser.Level = "ADMIN";
                        }
                        residentPortalUser.CompanyId = _companyInstanceSourceId;
                        residentPortalUser.CommunityAccessLevel = "ALL";
                        if ((residentPortal.PropertyList != null) && ((residentPortal.PropertyList.Count == 0) || ((residentPortal.PropertyList.Count == 1) && (residentPortal.PropertyList[0].ToUpper() == "ALL"))))
                        {
                            //{"PropertyList":[]} OR {"PropertyList":["all"]}
                            communityIds.Add(Convert.ToInt64(propertyProductList[0].CommunityId));
                            residentPortalUser.CommunityIds = null;
                        }
                        else
                        {
                            //{"PropertyList":["4288","2865"],"RoleList":["ENTERPRISESTANDARD"]}
                            residentPortalUser.CommunityAccessLevel = "LIMITED";
                            residentPortalUser.CommunityIds = residentPortal.PropertyList.ConvertAll(long.Parse);
                            communityIds = residentPortalUser.CommunityIds;
                        }

                        if (residentPortal.Notifications != null)
                        {
                            residentPortalUser.Notifications = residentPortal.Notifications;
                        }
                        //Groups (Messaging groups): MANAGEMENT, RESIDENT_SERVICES, FRONT_DESK, MAINTENANCE, LEASING
                        if ((residentPortal.MessageGroups != null) && (residentPortal.MessageGroups.Count > 0))
                        {
                            residentPortalUser.Groups = residentPortal.MessageGroups;
                        }
                        else
                        {
                            residentPortalUser.Groups = new List<string>();
                        }
                    }
                    else
                    {
                        //Staff Level: ADMIN, STANDARD, or LIMITED
                        residentPortalUser.Level = residentPortal.RoleList[0].ToUpper().Replace("STAFF", "");
                        if (residentPortal.Notifications != null)
                        {
                            residentPortalUser.Notifications = residentPortal.Notifications;
                        }
                        //Groups (Messaging groups): MANAGEMENT, RESIDENT_SERVICES, FRONT_DESK, MAINTENANCE, LEASING
                        if ((residentPortal.MessageGroups != null) && (residentPortal.MessageGroups.Count > 0))
                        {
                            residentPortalUser.Groups = residentPortal.MessageGroups;
                        }
                        else
                        {
                            residentPortalUser.Groups = new List<string>();
                        }
                        //Title (Manager custom title): MANAGER, LEASING_AGENT, BOARD, FRONTDESK, ASSISTANT_MANAGER, NIGHT_SHIFT, MAINTENANCE, CORPORATE, OTHER
                        //Updated from the Edit Profile

                        PartyRole partRole = partyRoleRepository.GetPartyRoleByEnterpriseUserID(realPageId);
                        residentPortalUser.Title = !string.IsNullOrEmpty(person.Title) ? person.Title : partRole != null && !string.IsNullOrEmpty(partRole.Name) ? partRole.Name : "Property Staff";

                        //If All Curent and Future properties toggle switch
                        if ((residentPortal.PropertyList != null) && (residentPortal.PropertyList.Count == 1) && (residentPortal.PropertyList[0].ToUpper() == "ALL"))
                        {
                            //Add all Active communities from BlueBook
                            foreach (ResidentPortalProperty community in propertyProductList)
                            {
                                communityIds.Add(Convert.ToInt64(community.CommunityId));
                            }
                        }
                        else
                        {
                            communityIds = residentPortal.PropertyList.ConvertAll(long.Parse);
                        }
                    }
                }

                IDataObject<ResidentPortalUser> dataObject = new DataObject<ResidentPortalUser>()
                {
                    data = residentPortalUser
                };
                AddCompanyIDToClient();

                string url = _residentPortalApiEndPoint + ((IsEnterprise) ? "/enterprise-users" : "/managers");
                logData.Add("url", url);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Begin {createUpdateUser} user userPersonaId - {userPersonaId} and loop through total communities - {communityIds.Count}" }, logData: logData);
                string userId = string.Empty;

                foreach (long community in communityIds)
                {
                    logData = new Dictionary<string, object>();
                    try
                    {
                        _communityId = community;
                        //Updating an existing user
                        //communityList = List of Communities Staff user currently has access to
                        if ((communityList != null) && (communityList.Count > 0))
                        {
                            //Remove the community from the list if the Staff user still has access to
                            //Later in method, we'll loop through the list to remove access to communities
                            Community communityToRemove = communityList.Find(a => a.CommunityId == _communityId);
                            communityList.Remove(communityToRemove);
                        }

                        AddCommunityIDToClient();
                        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url)
                        {
                            Content = new StringContent(JsonConvert.SerializeObject(dataObject), System.Text.Encoding.Default, "application/json")
                        };

                        logData.Add("dataObject", dataObject);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Posting - {userPersonaId} community - {community}" }, logData: logData);
                        HttpResponseMessage postResponse = _client.SendAsync(req).Result;
                        if (postResponse.IsSuccessStatusCode)
                        {
                            dynamic resultObject = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
                            logData = new Dictionary<string, object>
                            {
                                { "resultObject", resultObject }
                            };
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Post result - {userPersonaId} community - {community}" }, logData: logData);
                        }
                        else
                        {
                            dynamic errorResultObject = JsonConvert.DeserializeObject<dynamic>(postResponse.Content.ReadAsStringAsync().Result);
                            logData = new Dictionary<string, object>
                            {
                                { "errorResultObject", errorResultObject }
                            };
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error result." }, logData: logData);
                            errorCommunityIds.Add(_communityId, "Error - assign access to community.");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Do not throw any Exceptions and loop through all communities
                        errorCommunityIds.Add(_communityId, "Exception - assign access to community.");
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                    }
                }

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"End create/update user userPersonaId - {userPersonaId} and loop through total communities - {communityIds.Count}" });

                //Updating a Staff User and removing access to communities
                if ((!IsEnterprise) && (!string.IsNullOrWhiteSpace(_productUsername)))
                {
                    //IList<string> communitiesRemoved = new List<string>();
                    bool isCommunityRemoved = false;

                    url = _residentPortalApiEndPoint + "/managers/" + HttpUtility.UrlEncode(_productUsername);
                    logData.Add("url - managers", url);
                    foreach (ICommunity community in communityList)
                    {
                        try
                        {
                            _communityId = community.CommunityId;
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Community remove access - {userPersonaId} community - {community}" }, logData: logData);
                            var getResponse = RequestActionAsync("Delete", url, false, true).Result;

                            if (getResponse.IsSuccessStatusCode)
                            {
                                dynamic resultObject = JsonConvert.DeserializeObject<DataObject<dynamic>>(getResponse.Content.ReadAsStringAsync().Result);
                                logData.Add("ResidentPortalUser", resultObject);
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"result." }, logData: logData);
                            }
                            else
                            {
                                dynamic errorResultObject = JsonConvert.DeserializeObject<dynamic>(getResponse.Content.ReadAsStringAsync().Result);
                                logData.Add("ErrorResidentPortalUser", errorResultObject);
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error result." }, logData: logData);
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Delete errored - {userPersonaId} community - {_communityId}" });
                                errorCommunityIds.Add(_communityId, "Error - remove access to community.");
                            }
                        }
                        catch (Exception ex)
                        {
                            //Do not throw any Exceptions and loop through all communities
                            errorCommunityIds.Add(_communityId, "Exception - remove access to community.");
                            WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                        }
                    }
                }

                _residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
                if (_residentPortalUser != null)
                {
                    userId = (IsEnterprise) ? _residentPortalUser.EnterpriseUserId.ToString() : _residentPortalUser.ManagerId.ToString();
                    //Create OR Update a new Product UserName SAML attribute for the given personaId
                    //Create OR Update a new Product UserId SAML attribute for the given personaId
                    Dictionary<SamlAttributeEnum, string> userSetting = new Dictionary<SamlAttributeEnum, string>()
                    {
                        {SamlAttributeEnum.productUsername, _productUsername},
                        {SamlAttributeEnum.UserId, userId }
                    };
                    UpdateSamlUserAttributes(userPersonaId, userSetting);
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"End {createUpdateUser} user for user with editorPersona id - {editorPersonaId}." });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "Failed to create a resident portal user.";
                    output.Status = errorStatus;
                    return output;
                }

                if (errorCommunityIds.Count == 0)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Setting product result to success" });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"End {createUpdateUser} user for user with editorPersona id - {editorPersonaId}." });
                    errorStatus.Success = true;
                    errorStatus.ErrorMsg = "";
                    output.obj = residentPortalUser;
                    output.Status = errorStatus;

                    //Additional parameters logs
                    //Roles
                    var oldRoleOnly = oldRoles.Find(f => f.IsAssigned);
                    var newRoleListOnly = ListLevels(editorPersonaId, userPersonaId);
                    var newRoleOnly = newRoleListOnly.Find(f => f.IsAssigned);
                    if (oldRoleOnly?.Name != newRoleOnly.Name)
                    {
                        if (oldRoleOnly != null)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", oldRoleOnly.Name) });
                        }
                        if (newRoleOnly != null)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", newRoleOnly.Name) });
                        }
                    }

                    //Properties
                    var oldPropertiesOnly = oldProperties.Where(f => f.IsAssigned == true);
                    var newPropertiesOnly = oldProperties.Where(f => residentPortalUser.CommunityIds != null && residentPortalUser.CommunityIds.Contains(Convert.ToInt64(f.ID)));
                    if (oldPropertiesOnly.Any())
                    {
                        foreach (var p in oldPropertiesOnly.Where(p => newPropertiesOnly == null || !newPropertiesOnly.Any(c => c.ID == p.ID)))
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", p.Name) });
                        }
                    }
                    if (newPropertiesOnly.Any())
                    {
                        foreach (var p in newPropertiesOnly.Where(p => oldPropertiesOnly == null || !oldPropertiesOnly.Any(c => c.ID == p.ID)))
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", p.Name) });
                        }
                    }

                    //Message Groups
                    var oldMessagesOnly = oldMessageGroups.Where(f => f.IsAssigned);
                    var newMessagesOnly = oldMessageGroups.Where(f => residentPortalUser.Groups != null && residentPortalUser.Groups.Contains(f.Id.ToString()));
                    if (oldMessagesOnly.Any())
                    {
                        foreach (var p in oldMessagesOnly.Where(p => newMessagesOnly == null || !newMessagesOnly.Any(c => c.Id == p.Id)))
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Messaging Groups", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", p.Name) });
                        }
                    }
                    if (newMessagesOnly.Any())
                    {
                        foreach (var p in newMessagesOnly.Where(p => oldMessagesOnly == null || !oldMessagesOnly.Any(c => c.Id == p.Id)))
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Messaging Groups", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", p.Name) });
                        }
                    }

                    //Notifications
                    if (oldNotifications?.amenitiesViaEmail != residentPortalUser.Notifications.amenitiesViaEmail)
                    {
                        if (oldNotifications != null)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Notifications", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", oldNotifications?.amenitiesViaEmail == true ? "True" : "False") });
                        }
                        additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Notifications", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", residentPortalUser.Notifications.amenitiesViaEmail ? "True" : "False") });
                    }
                    if (oldNotifications?.managerMrViaEmail != residentPortalUser.Notifications.managerMrViaEmail)
                    {
                        if (oldNotifications != null)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Notifications", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", oldNotifications?.managerMrViaEmail == true ? "True" : "False") });
                        }
                        additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Notifications", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", residentPortalUser.Notifications.managerMrViaEmail ? "True" : "False") });
                    }
                    if (oldNotifications?.managerFdiViaEmail != residentPortalUser.Notifications.managerFdiViaEmail)
                    {
                        if (oldNotifications != null)
                        {
                            additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Notifications", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", oldNotifications?.managerFdiViaEmail == true ? "True" : "False") });
                        }
                        additionalParameters.Add(new AdditionalParameters { Key = "Resident Portals Notifications", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", residentPortalUser.Notifications.managerFdiViaEmail ? "True" : "False") });
                    }
                }
                else
                {
                    string sCommunity = (communityIds.Count > 1) ? "communities" : "community";
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Failed to {createUpdateUser} a resident portal user OR assign access to {errorCommunityIds.Count} out of {communityIds.Count} {sCommunity} for userPersonaId - {userPersonaId}." });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "Failed to create a resident portal user.";
                    output.Status = errorStatus;
                }
                return output;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageResidentPortalUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                errorStatus.Success = false;
                errorStatus.ErrorMsg = $"Error - {ex.Message}";
                output.Status = errorStatus;
                return output;
            }
        }

        /// <summary>
        /// List Resident Portal Enterprise Users Details
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="listSuperUsers">List true = Superusers, false = managers</param>
        /// <returns>List of ResidentPortalEnterpriseUser object</returns>
        public IList<ResidentPortalUser> ListUser(long editorPersonaId, bool listSuperUsers = true)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListUser", $"Begin list enterprise OR manager users details - {editorPersonaId}." });
            IList<ResidentPortalUser> listResidentPortalUser = new List<ResidentPortalUser>();

            _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (_listResponse.IsError)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ListUser", $"Error for user with editorPersona id - {editorPersonaId}. Error - {_listResponse.ErrorReason}" });
                return listResidentPortalUser;
            }
            return ListUserDetails(listSuperUsers);
        }

        /// <summary>
        /// GreenBook - Unassign User (enterprise or manager)
        /// Resident Portal - Deletes the user if they only have access to one community, otherwise it just removes access to the given community
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Error object</returns>
        public ObjectOutput<IResidentPortalUser, IErrorData> UnassignResidentPortalUser(long editorPersonaId, long userPersonaId)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            ObjectOutput<IResidentPortalUser, IErrorData> output = new ObjectOutput<IResidentPortalUser, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            //List of Communities Staff user currently has access to
            List<long> communityList = new List<long>();
            Dictionary<long, string> ErrorCommunityIds = new Dictionary<long, string>();

            string url = _residentPortalApiEndPoint;
            string productUsername = string.Empty;

            _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (_listResponse.IsError)
            {
                errorStatus.Success = false;
                errorStatus.ErrorMsg = _listResponse.ErrorReason;
                output.Status = errorStatus;
                return output;
            }

            //Deletes the user if they only have access to one community, otherwise it just removes access to the given community
            try
            {
                string userRole = string.Empty;

                if (_companyInstanceId == 0)
                {
                    _companyInstanceId = GetProductCompanyInstanceId(_udmSourceCode, useTranslate: false).CompanyInstanceId;
                    if (_companyInstanceId == 0)
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    }
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"GetProductCompanyInstanceId - Found blue book company instance id - {_companyInstanceId}  for user editorPersona id -{editorPersonaId}" });

                //lists Properties
                IList<ResidentPortalProperty> propertyProductList = ListResidentPortalProperties();
                if ((propertyProductList == null) || (propertyProductList.Count == 0))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"Error for user userPersonaId - {userPersonaId}. Error - No active properties found {propertyProductList}" });
                    errorStatus.Success = false;
                    errorStatus.ErrorMsg = "List properties from Resident Portal - No active properties found.";
                    output.Status = errorStatus;
                    return output;
                }

                if (_productUsername?.Length > 0)
                {
                    _residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);

                    if (_residentPortalUser != null)
                    {
                        productUsername = HttpUtility.UrlEncode(_productUsername);
                        if (_residentPortalUser?.EnterpriseUserId > 0)
                        {
                            userRole = "enterprise";
                            url += "/enterprise-users/";
                            //Remove access to 1 community to delete a Resident Portal Enterprise user
                            _communityId = _residentPortalUser.CommunityIds.First();
                            communityList.Add(_communityId);
                        }
                        else if (_residentPortalUser?.ManagerId > 0)
                        {
                            userRole = "manager";
                            url += "/managers/";
                            _residentPortalUser.Communities.ForEach(c =>
                            {
                                communityList.Add(c.CommunityId);
                            });
                        }

                        logData.Add($"url - {userRole}-users", url);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"Begin Delete {userRole} user." }, logData: logData);
                        url += productUsername;

                        if ((communityList != null) && (communityList.Count > 0))
                        {
                            foreach (long community in communityList)
                            {
                                try
                                {
                                    _communityId = community;
                                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"Community remove access - {userPersonaId} community - {community}" }, logData: logData);
                                    var getResponse = RequestActionAsync("Delete", url, false, true).Result;
                                    if (getResponse.IsSuccessStatusCode)
                                    {
                                        dynamic result = JsonConvert.DeserializeObject<DataObject<dynamic>>(getResponse.Content.ReadAsStringAsync().Result);
                                        logData = new Dictionary<string, object>
                                        {
                                            { "ResidentPortalUser", result }
                                        };
                                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"result." }, logData: logData);
                                    }
                                    else
                                    {
                                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"Delete errored - {userPersonaId} community - {_communityId}" });
                                        ErrorCommunityIds.Add(_communityId, "Error - remove access to community.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //Do not throw any Exceptions and loop through all communities
                                    ErrorCommunityIds.Add(_communityId, "Exception - remove access to community.");
                                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                                }
                            }
                            if (ErrorCommunityIds.Count == 0)
                            {
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"userPersonaId:{userPersonaId}" });
                                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"Delete SamlUserProductInfo And Status userPersonaId:{userPersonaId}" });
                                //clear the Resident Portal SAML attribute values instead of deleting because the delete removes the PersonaConfiguration record needed when Activating the user.
                                Dictionary<SamlAttributeEnum, string> userSetting = new Dictionary<SamlAttributeEnum, string>()
                            {
                                {SamlAttributeEnum.productUsername, string.Empty},
                                {SamlAttributeEnum.UserId, string.Empty }
                            };
                                UpdateSamlUserAttributes(userPersonaId, userSetting);

                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"End delete user for user with editorPersona id - {editorPersonaId}." });
                                errorStatus.Success = true;
                                errorStatus.ErrorMsg = "";
                                output.obj = _residentPortalUser;
                                output.Status = errorStatus;
                            }
                            else
                            {
                                string sCommunity = (communityList.Count > 1) ? "communities" : "community";
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", $"Failed to delete a resident portal user OR remove access to {ErrorCommunityIds.Count} out of {communityList.Count} {sCommunity} for userPersonaId - {userPersonaId}." });
                                errorStatus.Success = false;
                                errorStatus.ErrorMsg = "Failed to delete a resident portal user.";
                                output.Status = errorStatus;
                            }
                        }
                    }
                    else
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorMsg = "Error: User not found in Resident Portal";
                        output.Status = errorStatus;
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", errorStatus.ErrorMsg });
                        return output;
                    }
                }
                else
                {
                    errorStatus.Success = true;
                    errorStatus.ErrorMsg = "";
                    output.obj = _residentPortalUser;
                    output.Status = errorStatus;
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                errorStatus.Success = false;
                errorStatus.ErrorMsg = "Error in catch " + ex.Message;
                output.Status = errorStatus;
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignResidentPortalUser", errorStatus.ErrorMsg });
                return output;
            }

            return output;
        }

        /// <summary>
		/// ValidateUserAccess
		/// </summary>
		/// <param name="editorRealpageId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>true or false</returns>
        public bool ValidateUserAccess(Guid editorRealpageId, long userPersonaId)
        {
            bool valid = true;
            bool isRightExists = CheckUserRight.CheckUserHasAccess(_userClaims.Rights, "AddEditResidentPortalUser");
            IManagePersona managePersona = new ManagePersona(_userClaims);
            //Active Persona is linked to one organization
            Persona persona = managePersona.GetFirstAvailablePersonaByCompany(editorRealpageId, _userClaims.OrganizationPartyId);
            long editorPersonaId = persona.PersonaId;

            if (!IsSuperUser(editorPersonaId) && isRightExists)
            {
                ResidentPortalUser residentPortalUser = new ResidentPortalUser();
                ResidentPortalUser residentPortalEditorUser = new ResidentPortalUser();

                _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);

                string sUserLevel = "";
                string sEditorLevel = "";
                if (!string.IsNullOrWhiteSpace(_productUsername))
                {
                    residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
                    sUserLevel = string.Concat(residentPortalUser?.EnterpriseUserId > 0 ? "ENTERPRISE" : "STAFF", residentPortalUser?.Level);
                }
                //editor user list of roles can be assigned to user being created/edited
                if (!string.IsNullOrWhiteSpace(_editorProductUsername))
                {
                    residentPortalEditorUser = GetUserDetails(editorPersonaId, userPersonaId, _editorProductUsername, 0);
                    if (residentPortalEditorUser != null) // residentPortalEditorUser is NULL as User has the right in claims BUT user does not have product assigned
                    {
                        sEditorLevel = string.Concat(residentPortalEditorUser.EnterpriseUserId > 0 ? "ENTERPRISE" : "STAFF", residentPortalEditorUser.Level);
                    }
                }

                if ((residentPortalUser != null) && (residentPortalEditorUser != null))
                {
                    if (sEditorLevel.Equals("ENTERPRISEADMIN"))
                    {
                        valid = true;
                    }
                    else if (sUserLevel == sEditorLevel)
                    {
                        valid = true;
                    }
                    else if (sEditorLevel.Equals("STAFFADMIN") && sUserLevel.Length == 0)
                    {
                        valid = true;
                    }
                    else if (sEditorLevel.Equals("STAFFADMIN") && sUserLevel.Contains("STAFF"))
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = false;
                    }
                }
            }
            return valid;
        }

        /// <summary>
		/// ValidateUserAccess
		/// </summary>
		/// <param name="persona">Logged-in user Persona</param>		
		/// <returns>true or false</returns>
        public bool ValidateCreateUserAccess(Persona persona)
        {
            bool valid = true;
            bool isRightExists = CheckUserRight.CheckUserHasAccess(_userClaims.Rights, "AddEditResidentPortalUser");
            //Active Persona is linked to one organization
            long editorPersonaId = persona.PersonaId;

            if (!IsSuperUser(editorPersonaId) && isRightExists)
            {
                ResidentPortalUser residentPortalEditorUser = new ResidentPortalUser();
                _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);

                string sEditorLevel = "";
                //editor user list of roles can be assighed to user being created/edited
                if (!string.IsNullOrWhiteSpace(_editorProductUsername))
                {
                    residentPortalEditorUser = GetUserDetails(editorPersonaId, 0, _editorProductUsername, 0);
                    if (residentPortalEditorUser != null) // residentPortalEditorUser is NULL as User has the right in claims BUT user does not have product assigned
                    {
                        sEditorLevel = string.Concat(residentPortalEditorUser.EnterpriseUserId > 0 ? "ENTERPRISE" : "STAFF", residentPortalEditorUser.Level);
                    }
                }

                if (residentPortalEditorUser != null)
                {
                    if (sEditorLevel.Equals("ENTERPRISEADMIN") || sEditorLevel.Equals("STAFFADMIN"))
                    {
                        valid = true;
                    }
                    else
                    {
                        valid = false;
                    }
                }
            }
            return valid;
        }
        /// <summary>
        /// List Level
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Levels list</returns>
        public List<ILevel> ListLevels(long editorPersonaId, long userPersonaId)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            bool isAddEditResidentPortalUserRightExists = CheckUserRight.CheckUserHasAccess(_userClaims.Rights, "AddEditResidentPortalUser");
            bool isCreateUserRightExists = CheckUserRight.CheckUserHasAccess(_userClaims.Rights, "CreateUser");
            bool isSuperUser = false;
            string sLevel = string.Empty;

            if (editorPersonaId > 0)
            {
                isSuperUser = base.IsSuperUser(editorPersonaId);
            }

            _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if ((!_listResponse.IsError) && (_residentPortalUser.Levels == null) && !string.IsNullOrWhiteSpace(_productUsername))
            {
                _residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
            }

            try
            {
                //Get Resident Portal roles if the logged-in user is
                //A Unified Login Administrator
                //OR
                //Has the Unified Login system User Administrator role (includes "Ability to create users" Right)
                //OR
                //Has a Custom Role that includes either the Right "Ability to create and manage users for Resident Portal" OR "Ability to create users"			
                if ((editorPersonaId > 0) && ((isSuperUser || isAddEditResidentPortalUserRightExists || isCreateUserRightExists)))
                {
                    string url = _residentPortalApiEndPoint + "/roles";
                    logData = new Dictionary<string, object>
                    {
                        { "url", url }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListLevels", "List roles." }, logData: logData);
                    var getResponse = RequestActionAsync("Get", url, true, true).Result;
                    if (getResponse.IsSuccessStatusCode)
                    {
                        ResidentPortalRole resultObject = JsonConvert.DeserializeObject<ResidentPortalRole>(getResponse.Content.ReadAsStringAsync().Result);
                        _levelList = RolesList(resultObject.Data);
                        logData = new Dictionary<string, object>
                        {
                            { "Level", _levelList }
                        };
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListLevels", "Got roles result." }, logData: logData);
                    }
                }
                else
                {
                    if (_residentPortalEditorUser.Levels == null)
                    {
                        if (!string.IsNullOrWhiteSpace(_editorProductUsername))
                        {
                            _residentPortalEditorUser = GetUserDetails(editorPersonaId, userPersonaId, _editorProductUsername, 0);
                        }
                    }

                    //editor user list of roles can be assighed to user being created/edited
                    _levelList = RolesList(_residentPortalEditorUser.canCreateRoles);
                }
            }
            catch (Exception ex)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListLevels", $"Error {ex.Message}" });
                throw ex;
            }

            if ((_residentPortalUser != null) && (!string.IsNullOrEmpty(_residentPortalUser.FirstName)))
            {
                sLevel = string.Concat(_residentPortalUser?.EnterpriseUserId > 0 ? "ENTERPRISE" : "STAFF", _residentPortalUser.Level);
                //What are the allowed roles for the edited user
                if (!isAddEditResidentPortalUserRightExists && (!IsSuperUser(editorPersonaId)))
                {
                    _levelList = RolesList(_residentPortalUser.allowedRoles);
                }
                _levelList.Find(item => item.Id == sLevel).IsAssigned = true;
            }
            else
            {
                RolePropertyList roleproperty = GetDeactivatedProductBatchData(userPersonaId);

                if (roleproperty != null && (roleproperty.RoleList?.Count > 0))
                {
                    string role = roleproperty.RoleList[0];
                    _levelList.Find(item => item.Id == role).IsAssigned = true;
                }
            }
            //if logged-in user is NOT a Unified Login Administrator
            if (!isSuperUser)
            {
                if (_residentPortalEditorUser.Levels == null)
                {
                    if (!string.IsNullOrWhiteSpace(_editorProductUsername))
                    {
                        _residentPortalEditorUser = GetUserDetails(editorPersonaId, userPersonaId, _editorProductUsername, 0);
                    }
                }
                sLevel = string.Concat(_residentPortalEditorUser?.EnterpriseUserId > 0 ? "ENTERPRISE" : "STAFF", _residentPortalEditorUser.Level);
                //Disable the Resident Portal Enterprise Admin role if the user has the Enterprise Standard role
                if (sLevel.Equals("ENTERPRISESTANDARD"))
                {
                    _levelList.Find(item => item.Id.Equals("ENTERPRISEADMIN")).IsDisabled = true;
                }

                //Disable the Resident Portal Enterprise Admin and Standard roles if the user has any of the Staff roles
                if (sLevel.Contains("STAFF"))
                {
                    ILevel level = _levelList.Find(item => item.Id.Equals("ENTERPRISEADMIN"));
                    if (level != null)
                    {
                        level.IsDisabled = true;
                    }
                    level = _levelList.Find(item => item.Id.Equals("ENTERPRISESTANDARD"));
                    if (level != null)
                    {
                        level.IsDisabled = true;
                    }

                    //Disable the Resident Portal Staff Admin role if the user has the Staff Standard role
                    if (sLevel.Equals("STAFFSTANDARD"))
                    {
                        _levelList.Find(item => item.Id.Equals("STAFFADMIN")).IsDisabled = true;
                    }

                    //Disable the Resident Portal Staff Admin and Standard roles if the user has the Staff Limited role
                    if (sLevel.Equals("STAFFLIMITED"))
                    {
                        _levelList.Find(item => item.Id.Equals("STAFFADMIN")).IsDisabled = true;
                        _levelList.Find(item => item.Id.Equals("STAFFSTANDARD")).IsDisabled = true;
                    }
                }
            }

            return _levelList;
        }

        /// <summary>
        /// Calls List levels internally but returns ListResponse type object
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        public ListResponse ListLevelsResponse(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse;
            try
            {

                List<ILevel> listLevels = ListLevels(editorPersonaId, userPersonaId)?.OrderBy(x => x.Name).ToList();
                listResponse = new ListResponse()
                {
                    Records = listLevels.Cast<object>().ToList(),
                    TotalRows = listLevels?.Count ?? 0,
                    RowsPerPage = 9999,
                    TotalPages = 1,
                    ErrorReason = ""
                };
            }
            catch (Exception ex)
            {
                listResponse = new ListResponse()
                {
                    IsError = true,
                    ErrorReason = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }

            return listResponse;
        }

        /// <summary>
        /// List Messaging Groups
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Messaging Groups list</returns>
        public List<IMessagingGroups> ListMessageGroups(long editorPersonaId, long userPersonaId)
        {
            _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if ((!_listResponse.IsError) && (_residentPortalUser.MessagingGroups == null))
            {
                if (!string.IsNullOrWhiteSpace(_productUsername))
                {
                    _residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
                }
            }

            List<IMessagingGroups> messageGroupeList = new List<IMessagingGroups>()
            {
                new MessagingGroups()
                {
                    Id = "MANAGEMENT",
                    Name = "Management",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "RESIDENT_SERVICES",
                    Name = "Resident Services",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "FRONT_DESK",
                    Name = "Front Desk",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "MAINTENANCE",
                    Name = "Maintenance",
                    IsAssigned = false
                },
                new MessagingGroups()
                {
                    Id = "LEASING",
                    Name = "Leasing",
                    IsAssigned = false
                }
            };

            if ((_residentPortalUser != null) && (!string.IsNullOrEmpty(_residentPortalUser.FirstName)) && (_residentPortalUser.MessageGroups != null))
            {
                foreach (string messageGroup in _residentPortalUser.MessageGroups)
                {
                    messageGroupeList.Find(item => item.Id == messageGroup).IsAssigned = true;
                }
            }
            else
            {
                RolePropertyList roleproperty = GetDeactivatedProductBatchData(userPersonaId);

                if (roleproperty != null && roleproperty.MessageGroups != null)
                {
                    foreach (string messageGroup in roleproperty.MessageGroups)
                    {
                        messageGroupeList.Find(item => item.Id == messageGroup).IsAssigned = true;
                    }
                }
            }
            return messageGroupeList.OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// List Titles
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>Titles list</returns>
        public List<ITitle> ListTitles(long editorPersonaId, long userPersonaId)
        {
            List<ITitle> titleList = new List<ITitle>()
            {
                new Title()
                {
                    Id = "MANAGER",
                    Name = "Manager"
                },
                new Title()
                {
                    Id = "LEASING_AGENT",
                    Name = "Leasing Agent"
                },
                new Title()
                {
                    Id = "BOARD",
                    Name = "Board"
                },
                new Title()
                {
                    Id = "FRONTDESK",
                    Name = "Front Desk"
                },
                new Title()
                {
                    Id = "ASSISTANT_MANAGER",
                    Name = "Assistant Manager"
                },
                new Title()
                {
                    Id = "NIGHT_SHIFT",
                    Name = "Night Shift"
                },
                new Title()
                {
                    Id = "MAINTENANCE",
                    Name = "Maintenance"
                },
                new Title()
                {
                    Id = "CORPORATE",
                    Name = "Corporate"
                },
                new Title()
                {
                    Id = "OTHER",
                    Name = "Other"
                }
            };
            return titleList;
        }
        #endregion

        #region User

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="productUserId">The product user identifier.</param>
        /// <param name="productUsername">The product username.</param>
        /// <returns></returns>
        public bool DeleteUser(long editorPersonaId, long productUserId, string productUsername)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            //List of Communities Staff user currently has access to
            List<long> communityList = new List<long>();
            Dictionary<long, string> ErrorCommunityIds = new Dictionary<long, string>();

            string url = _residentPortalApiEndPoint;

            _listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (_listResponse.IsError)
            {
                return false;
            }

            //Deletes the user if they only have access to one community, otherwise it just removes access to the given community
            try
            {
                string userRole = string.Empty;

                if (_companyInstanceId == 0)
                {
                    _companyInstanceId = GetProductCompanyInstanceId(_udmSourceCode, useTranslate: false).CompanyInstanceId;
                    if (_companyInstanceId == 0)
                    {
                        WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"GetProductCompanyInstanceId - Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}." });
                    }
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"GetProductCompanyInstanceId - Found blue book company instance id - {_companyInstanceId}  for user editorPersona id - {editorPersonaId}" });

                //lists Properties
                IList<ResidentPortalProperty> propertyProductList = ListResidentPortalProperties();
                if ((propertyProductList == null) || (propertyProductList.Count == 0))
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Failed to delete a resident portal user - {productUsername}. Error - No active properties found" });
                    return false;
                }

                _residentPortalUser = GetUserDetails(editorPersonaId, 0, productUsername, productUserId);

                if (_residentPortalUser != null)
                {
                    productUsername = HttpUtility.UrlEncode(productUsername);
                    if (_residentPortalUser.EnterpriseUserId > 0)
                    {
                        userRole = "enterprise";
                        url += "/enterprise-users/";
                        //Remove access to 1 community to delete a Resident Portal Enterprise user
                        _communityId = _residentPortalUser.CommunityIds[0];
                        communityList.Add(_communityId);
                    }
                    else if (_residentPortalUser.ManagerId > 0)
                    {
                        userRole = "manager";
                        url += "/managers/";
                        _residentPortalUser.Communities.ForEach(c =>
                        {
                            communityList.Add(c.CommunityId);
                        });
                    }

                    logData.Add($"url - {userRole}-users", url);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Begin Delete {userRole} user." }, logData: logData);
                    url += productUsername;

                    if ((communityList != null) && (communityList.Count > 0))
                    {
                        foreach (long community in communityList)
                        {
                            try
                            {
                                _communityId = community;
                                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Community remove access - {productUsername} community - {community}" }, logData: logData);
                                var getResponse = RequestActionAsync("Delete", url, false, true).Result;
                                if (getResponse.IsSuccessStatusCode)
                                {
                                    dynamic result = JsonConvert.DeserializeObject<DataObject<dynamic>>(getResponse.Content.ReadAsStringAsync().Result);
                                    logData.Add("ResidentPortalUser", result);
                                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"result." }, logData: logData);
                                }
                                else
                                {
                                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Delete errored - {productUsername} community - {_communityId}" });
                                    ErrorCommunityIds.Add(_communityId, "Error - remove access to community.");
                                }
                            }
                            catch (Exception ex)
                            {
                                //Do not throw any Exceptions and loop through all communities
                                ErrorCommunityIds.Add(_communityId, "Exception - remove access to community.");
                                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Error for user with editorPersona id - {editorPersonaId}" }, exception: ex);
                            }
                        }
                        if (ErrorCommunityIds.Count == 0)
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"End delete user for user with editorPersona id - {editorPersonaId}." });
                        }
                        else
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Failed to delete a resident portal user OR remove access to {ErrorCommunityIds.Count} out of {communityList.Count} Community(s) for productUsername - {productUsername}." });
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteUser", $"Error {ex.Message}" });
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

                var url = $"{_mtApiEndPoint}/{companyInstanceSourceId}/users?filter={filter}&app_id={_appId}&app_key={_appKey}";
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", "Beginning Get Migration Users List" }, logData: new Dictionary<string, object> { { "Url", url } });

                var residentPortalUsers = GetResultFromApi<IList<ResidentPortalMigrationUser>>(url);

                if (residentPortalUsers == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}." });
                    return response;
                }

                var allUsers = residentPortalUsers.Select(x => new MigrationUser()
                {
                    CompanyInstanceSourceId = x.CompanyInstanceSourceId,
                    Email = x.Email,
                    Extra = x.Extra,
                    FirstName = x.FirstName,
                    LastActivity = x.LastActivity,
                    LastName = x.LastName,
                    MiddleName = x.MiddleName,
                    Phone = x.Phone,
                    Status = x.Status,
                    Title = x.Title,
                    UserId = x.UserId,
                    Username = x.Username,
                    Properties = x.Properties
                }).ToList();
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}." });
                response.RowsPerPage = resultPerRow;
                response.ErrorReason = string.Empty;
                response.IsError = false;
                response.TotalPages = 1;
                response.Records = allUsers.Cast<object>().ToList();
                response.TotalRows = allUsers.Count;
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

                var url = $"{_mtApiEndPoint}/{companyInstanceSourceId}/migrate-users?app_id={_appId}&app_key={_appKey}";
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
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"PutAsJsonAsync Success" }, logData: logData);
                    return JsonConvert.DeserializeObject<MigrateResponse>(responseContent);
                }
                else
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"PutAsJsonAsync Fail" }, logData: logData);
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
        /// <summary>
        /// Get the access token needed to make the call to Resident Portal
        /// </summary>
        /// <returns>true if getting an access token was successful; otherwise false</returns>
        private bool GetUnifiedLoginAccessToken()
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUnifiedLoginAccessToken", $"Getting Access Token." });

            try
            {
                AddHeaderValuesToClient();

                if (_tokenHelper == null)
                {
                    _tokenHelper = new TokenHelper();
                }
                _accessToken = _tokenHelper.GetUnifiedLoginServerToken("usermanagement");
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                if (_accessToken == null)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUnifiedLoginAccessToken", $"Failed to get Access Token." });
                    return false;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetUnifiedLoginAccessToken", $"Failed to get Access Token." }, exception: ex);
                return false;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUnifiedLoginAccessToken", $"Got Access Token." });

            return true;
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
            bool displayAllProperties = true;
            bool allProperties = false;
            Dictionary<string, bool> additionalDictionary = new Dictionary<string, bool>();

            ProductProperty productProperty;
            List<ProductProperty> propertyList = blueBookPropertyList.ToList();
            // merge the given user details with the list
            ResidentPortalUser residentPortalUser = GetUserDetails(editorPersonaId, userPersonaId, _productUsername, 0);
            bool isSuperUser = IsSuperUser(editorPersonaId);
            bool isRightExists = CheckUserRight.CheckUserHasAccess(_userClaims.Rights, "AddEditResidentPortalUser");
            // if a user record exists
            if (residentPortalUser != null)
            {
                //remove properties are not assigned for user with AddEditResidentPortalUser right Access
                if (isRightExists && !isSuperUser)
                {
                    ResidentPortalUser editorResidentPortalUser = GetUserDetails(editorPersonaId, editorPersonaId, _editorProductUsername, 0);

                    if (editorResidentPortalUser?.CommunityIds?.Count > 0)
                    {
                        var removeCommunityIds = editorResidentPortalUser.CommunityIds.ToList();
                        propertyList.RemoveAll(p => !removeCommunityIds.Exists(r => p.ID == r.ToString()));
                    }
                    if (editorResidentPortalUser?.Communities?.Count > 0)
                    {
                        var removeCommunity = editorResidentPortalUser.Communities.ToList();
                        propertyList.RemoveAll(p => !removeCommunity.Exists(r => p.ID == r.CommunityId.ToString()));
                    }
                }

                //Staff user access to communities
                if ((residentPortalUser.Communities != null) && (residentPortalUser.ManagerId > 0))
                {
                    foreach (ICommunity community in residentPortalUser.Communities)
                    {
                        productProperty = propertyList.Find(item => item.ID == community.CommunityId.ToString());
                        if (productProperty != null)
                        {
                            productProperty.IsAssigned = true;
                        }
                    }
                }
                //Enterprise user with limited community access level
                else if ((residentPortalUser.CommunityIds != null) && (residentPortalUser.EnterpriseUserId > 0))
                {
                    foreach (long community in residentPortalUser.CommunityIds)
                    {
                        productProperty = propertyList.Find(item => item.ID == community.ToString());
                        if (productProperty != null)
                        {
                            productProperty.IsAssigned = true;
                        }
                    }
                }
                displayAllProperties = (residentPortalUser.EnterpriseUserId > 0);
                allProperties = residentPortalUser.AllProperties;
            }
            else
            {
                RolePropertyList roleproperty = GetDeactivatedProductBatchData(userPersonaId);

                if (roleproperty != null && (roleproperty.PropertyList != null && roleproperty.PropertyList.Count > 0))
                {
                    if (roleproperty.PropertyList.Count == 1 && roleproperty.PropertyList[0] == "all")
                    {
                        allProperties = true;
                    }
                    else
                    {
                        foreach (string property in roleproperty.PropertyList)
                        {
                            productProperty = propertyList.Find(item => item.ID == property);
                            if (productProperty != null)
                            {
                                productProperty.IsAssigned = true;
                            }
                        }
                    }
                }
            }

            //Display the "Allow access to all current and future properties" toggle switch?
            additionalDictionary.Add("displayAllProperties", displayAllProperties);
            //"Allow access to all current and future properties" On or Off?
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
        /// Used to add CommunityID to http client
        /// 1. Set AB-API-Community-ID to one of the community Ids when adding an enterprise user that has access to ALL or a limited list
        /// 2. Set AB-API-Community-ID to each community a manager has access to (Add a record for each community to ProductBatch 
        /// </summary>
        private void AddCommunityIDToClient()
        {
            if (_client.DefaultRequestHeaders.Contains("AB-API-Community-ID"))
            {
                _client.DefaultRequestHeaders.Remove("AB-API-Community-ID");
            }
            _client.DefaultRequestHeaders.Add("AB-API-Community-ID", _communityId.ToString());
        }

        /// <summary>
        /// Used to add CompanyID to http client
        /// </summary>
        private void AddCompanyIDToClient()
        {
            if (_client.DefaultRequestHeaders.Contains("AB-API-Company-ID"))
            {
                _client.DefaultRequestHeaders.Remove("AB-API-Company-ID");
            }
            _client.DefaultRequestHeaders.Add("AB-API-Company-ID", _companyInstanceSourceId.ToString());
        }

        /// <summary>
        /// Used to add required header values to http client
        /// </summary>
        private void AddHeaderValuesToClient()
        {
            if (_client.DefaultRequestHeaders.Contains("X-Forwarded-Proto"))
            {
                _client.DefaultRequestHeaders.Remove("X-Forwarded-Proto");
            }
            _client.DefaultRequestHeaders.Add("X-Forwarded-Proto", _forwardedProtocol);
        }

        /// <summary>
        /// Used to get details about an Resident Portal user
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">User PersonaId</param>
        /// <param name="userLogin">username (email)</param>
        /// <param name="userId">Resident Portal userId</param>
        /// <returns>Resident Portal object</returns>
        private ResidentPortalUser GetUserDetails(long editorPersonaId, long userPersonaId, string userLogin, long userId)
        {
            IDataObject<ResidentPortalUser> dataRootObject = new DataObject<ResidentPortalUser>();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            try
            {
                string url = _residentPortalApiEndPoint + "/enterprise-users/";
                string userIdOrName = (!string.IsNullOrWhiteSpace(userLogin)) ? HttpUtility.UrlEncode(userLogin) : userId.ToString();

                logData.Add("url - enterprise-users", url + userIdOrName);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", $"Get user." }, logData: logData);
                var getResponse = RequestActionAsync("Get", url + userIdOrName + "?expand=communities", true, true).Result;
                if (!getResponse.IsSuccessStatusCode)
                {
                    url = _residentPortalApiEndPoint + "/managers/" + userIdOrName + "?expand=communities,notifications,messageGroups";
                    logData.Add("url - managers", url);
                    getResponse = RequestActionAsync("Get", url, true, true).Result;
                }

                if (getResponse.IsSuccessStatusCode)
                {
                    dataRootObject = JsonConvert.DeserializeObject<DataObject<ResidentPortalUser>>(getResponse.Content.ReadAsStringAsync().Result);
                    if ((dataRootObject.data.CommunityAccessLevel != null) && (dataRootObject.data.CommunityAccessLevel.ToUpper() == "ALL"))
                    {
                        dataRootObject.data.AllProperties = true;
                    }
                    logData.Add("ResidentPortalUser", dataRootObject.data);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", $"Got user result." }, logData: logData);
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetUserDetails", $"Error {ex.Message}" });
            }

            return dataRootObject.data;
        }

        /// <summary>
        /// Used to list enterprise users
        /// </summary>
        /// <param name="listSuperUsers">List true = Superusers, false = managers</param>
        /// <returns>Resident Portal object</returns>
        private IList<ResidentPortalUser> ListUserDetails(bool listSuperUsers = true)
        {
            IDataList<ResidentPortalUser> dataList = new DataList<ResidentPortalUser>();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            try
            {
                string url = _residentPortalApiEndPoint + ((listSuperUsers) ? "/enterprise-users" : "managers");
                logData = new Dictionary<string, object>
                {
                    { "url", url }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListUserDetails", "List user." }, logData: logData);
                var getResponse = RequestActionAsync("Get", url, true, true).Result;

                if (getResponse.IsSuccessStatusCode)
                {
                    dataList = JsonConvert.DeserializeObject<DataList<ResidentPortalUser>>(getResponse.Content.ReadAsStringAsync().Result);
                    logData = new Dictionary<string, object>
                    {
                        { "ResidentPortalUser", dataList.data }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListUserDetails", "Got users result." }, logData: logData);
                }
            }
            catch (Exception ex)
            {
                // return the user exists
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListUserDetails", $"Error {ex.Message}" });
            }

            return dataList.data;
        }

        /// <summary>
        /// Used to get data async with retry
        /// </summary>
        /// <param name="verb">HTTP verb</param>
        /// <param name="uri">The Uri the request is sent to</param>
        /// <param name="getCommunityId">Call Resident Portal for a list of Communities to add a community to client</param>
        /// <param name="addCommunityIdToClient">Add the community to client</param>
        /// <returns>The task object representing the asynchronous operation</returns>
        private async Task<HttpResponseMessage> RequestActionAsync(string verb, string uri, bool getCommunityId = true, bool addCommunityIdToClient = true)
        {
            bool doneProcessing = false;
            int failedCount = 0;
            HttpResponseMessage response = new HttpResponseMessage();
            Dictionary<string, object> logData = new Dictionary<string, object>
            {
                { "uri", uri }
            };

            if (!GetUnifiedLoginAccessToken())
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if ((_companyInstanceSourceId == 0) || (_communityId == 0))
            {
                CustomerCompanyMap companyMap = GetProductCompanyInstanceId(_udmSourceCode);
                if (companyMap != null)
                {
                    _companyInstanceSourceId = Convert.ToInt32(companyMap.CompanyInstanceSourceId);
                    if (addCommunityIdToClient)
                    {
                        IList<ResidentPortalProperty> propertyProductList = ListResidentPortalProperties();
                        if ((propertyProductList != null) && (propertyProductList.Count > 0))
                        {
                            _communityId = Convert.ToInt64(propertyProductList[0].CommunityId);
                        }
                    }
                }
            }

            AddCompanyIDToClient();
            if (addCommunityIdToClient)
            {
                AddCommunityIDToClient();
            }

            while (!doneProcessing)
            {
                logData = new Dictionary<string, object>
                {
                    { "uri", uri }
                };
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "RequestActionAsync", $"{verb}Async - Posting attempt {failedCount}" }, logData: logData);
                switch (verb.ToUpper())
                {
                    case "DELETE":
                        response = _client.DeleteAsync(uri).Result;
                        break;
                    case "GET":
                        response = _client.GetAsync(uri).Result;
                        break;
                }
                doneProcessing = response.IsSuccessStatusCode;
                if (!doneProcessing)
                {
                    if (response.StatusCode != HttpStatusCode.Unauthorized)
                    {
                        logData.Add("response.StatusCode", response.StatusCode);
                        logData.Add("response.Content", response.Content.ReadAsStringAsync().Result);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "RequestActionAsync", $"{verb}Async - Exiting after error." }, logData: logData);
                        doneProcessing = true;
                    }
                    else
                    {
                        // reset the token so it gets a new one if we got an unauthorized error
                        _accessToken = "";
                        GetUnifiedLoginAccessToken();
                        failedCount += 1;
                    }
                    if (failedCount >= MAXRETRYCOUNT)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "RequestActionAsync", $"{verb}Async - Exiting after too many tries." }, logData: logData);
                        doneProcessing = true;
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// List of resident portal roles a user can assign when creating/updating users
        /// </summary>
        /// <param name="roleData">Roles from Resident Portal</param>
        /// <returns>List of resident portal roles a user can assign when creating/updating users</returns>
        private List<ILevel> RolesList(Dictionary<string, string> roleData)
        {
            List<ILevel> levelList = new List<ILevel>();
            ILevel level;

            foreach (var role in roleData)
            {
                level = new Level()
                {
                    Id = role.Key.Replace("_", ""),
                    Name = role.Value,
                    IsAssigned = false,
                    IsDisabled = false
                };
                levelList.Add(level);
            }
            return levelList;
        }

        /// <summary>
        /// Used to get information from the product
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseUrlAndQuery"></param>
        /// <returns></returns>
        private T GetResultFromApi<T>(string baseUrlAndQuery) where T : class
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

        /// <summary>
        /// Return a list of properties from Resident Portal
        /// </summary>
        /// <returns>list of ProductPropertyMap</returns>
        private IList<ResidentPortalProperty> ListResidentPortalProperties()
        {
            List<ResidentPortalProperty> propertyProductList = new List<ResidentPortalProperty>();
            string cacheKey = "ResidentPortalProperties" + _userClaims.OrganizationPartyId;
            propertyProductList = _manageResidentPortalCache.GetFromCache<List<ResidentPortalProperty>>(cacheKey, 300, () =>
            {
                int limit = 100;
                int offset = 0;

                for (int index = 0; index <= 9999; index++)
                {
                    IList<ResidentPortalProperty> newPropertyProductList = ListResidentPortalPropertiesWithPaging(limit.ToString(), offset.ToString());
                    if (newPropertyProductList != null && newPropertyProductList.Count > 0)
                    {
                        propertyProductList.AddRange(newPropertyProductList);
                        offset = offset + 100;
                        if (newPropertyProductList.Count < 100)
                            break;
                    }
                    else
                        break;
                }

                propertyProductList = propertyProductList.Where(p => p.Active).ToList();
                if (propertyProductList.Count > 0)
                {
                    propertyProductList = propertyProductList.OrderBy(p => p.Title).ToList();
                }
                return propertyProductList.ToList();
            });
            return propertyProductList.ToList();
        }

        /// <summary>
        /// Return a list of properties from Resident Portal with Paging
        /// </summary>
        /// <returns>list of ProductPropertyMap</returns>
        private IList<ResidentPortalProperty> ListResidentPortalPropertiesWithPaging(string limit, string offset)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            IList<ResidentPortalProperty> communityList = new List<ResidentPortalProperty>();
            string url = _residentPortalApiEndPoint + "/communities?filters={\"\":{\"limit\":" + limit + ",\"offset\":" + offset + "}}&expand=services";
            logData = new Dictionary<string, object>
            {
                { "url", url }
            };
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListResidentPortalPropertiesWithPaging", "List properties." }, logData: logData);
            var getResponse = RequestActionAsync("Get", url, false, false).Result;
            if (getResponse.IsSuccessStatusCode)
            {
                IDataList<ResidentPortalProperty> dataRoot = JsonConvert.DeserializeObject<DataList<ResidentPortalProperty>>(getResponse.Content.ReadAsStringAsync().Result);

                if ((dataRoot != null) && (dataRoot.data.Count > 0))
                {
                    communityList = dataRoot.data;
                }

                logData.Add("Properties", communityList);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListResidentPortalPropertiesWithPaging", "Got properties result." }, logData: logData);
            }
            else
            {
                logData.Add("Response", getResponse);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListResidentPortalPropertiesWithPaging", $"Errored company instance source id {_companyInstanceSourceId}" }, logData: logData);
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ListResidentPortalPropertiesWithPaging", $"Communities - Found total {communityList.Count} properties with Resident Portal company instance source id {_companyInstanceSourceId}." });
            return communityList;
        }

        #endregion
    }

    /// <summary>
    /// Roles data from Resident Portal
    /// </summary>
    public class ResidentPortalRole
    {
        /// <summary>
        /// Roles data from Resident Portal
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public Dictionary<string, string> Data { get; set; }
    }

    /// <summary>
    /// Resident Portal Property
    /// </summary>
    public class ResidentPortalProperty
    {
        /// <summary>
        /// communityId
        /// </summary>
        public string CommunityId { get; set; }

        /// <summary>
        /// Community Name
        /// </summary>		
        public string Title { get; set; }

        /// <summary>
        /// Is community active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public List<Services> Services { get; set; }
    }

    /// <summary>
    /// Services
    /// </summary>
    public class Services
    {
        /// <summary>
        /// Address
        /// </summary>		
        public Location Location { get; set; }
    }

    /// <summary>
    /// Address Location
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Address
        /// </summary>		
        public Address Address { get; set; }
    }

    /// <summary>
    /// Property Address from Resident Portal
    /// </summary>
    public class Address
    {
        /// <summary>
        /// City
        /// </summary>		
        public string City { get; set; }

        /// <summary>
        /// Country
        /// </summary>		
        public string Country { get; set; }

        /// <summary>
        /// State
        /// </summary>		
        public string State { get; set; }

        /// <summary>
        /// Street
        /// </summary>		
        public string Street { get; set; }

        /// <summary>
        /// ZipCode
        /// </summary>		
        public string ZipCode { get; set; }
    }

    /// <summary>
    /// Used to help convert product classes to GreenBook classes
    /// </summary>
    public static class ManageProductResidentPortalHelpers
    {
        /// <summary>
        /// Used to convert a Product property into a GreenBook property
        /// </summary>
        /// <param name="listProductPropertyMap">The list of properties to convert</param>
        /// <returns>list of ProductProperty</returns>
        public static IList<ProductProperty> ToGBProperties(this IList<ResidentPortalProperty> listProductPropertyMap)
        {
            string state = string.Empty;
            if (listProductPropertyMap == null)
            {
                return null;
            }
            IList<ProductProperty> listProductProperty = new List<ProductProperty>();
            foreach (ResidentPortalProperty property in listProductPropertyMap)
            {
                state = string.Empty;
                if ((property?.Services[0]?.Location?.Address?.State != null) && (property.Services[0]?.Location?.Address?.State.Length > 0))
                {
                    state = property.Services[0].Location.Address.State;
                }
                listProductProperty.Add(new ProductProperty
                {
                    ID = property?.CommunityId,
                    Name = property?.Title,
                    State = state
                });
            }
            return listProductProperty;
        }
    }
}