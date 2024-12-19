using Elasticsearch.Net;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RoleType = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite.RoleType;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product
{
    /// <summary>
    /// Used to update OneSite user information
    /// </summary>
    public class ManageProductOneSite : ManageProductBase, IManageProductOneSite
    {
        // get the list of onesite properties for the given BlackBook OneSite id
        // http://booksapi-stg.realpage.com/companypropertyinstancemap?filter[companyInstanceId]=650532&include=propertyInstance&fields[propertyInstance]=propertyInstanceSourceId,propertyInstanceId,source,propertyName,isActive

        // get all the products for a given company
        // http://booksapi-stg.realpage.com/companymap?filter[companyId]=2326
        // http://booksapi-stg.realpage.com/companymap?filter[companyId]=2326&include=companyInstance

        //private const int _productId = (int)ProductEnum.OneSite;

        private string _username;
        private string _password;
        private string _onesiteUrl;

        private string _mtApiEndPoint;
        private string _mtClientSecret;
        private string _mtClientId;
        private string _mtTokenEndPoint;

        private PropertyList _propertyList = new PropertyList();
        private RoleList _roleList = new RoleList();
        private RightList _rightList = new RightList();
        private UserList _userList = new UserList();
        private IList<ProductSettingList> _userProductSettings = new List<ProductSettingList>();

        private DefaultUserClaim _userClaims;
        private const string RIGHT_ASSIGN = "{\"action\":\"Added\",\"value\":\"RightName\"}";
        private const string RIGHT_UNASSIGN = "{\"action\":\"Removed\",\"value\":\"RightName\"}";
        private const string ROLE_ASSIGN = "{\"action\":\"Added\",\"value\":\"RoleName\"}";
        private const string ROLE_UNASSIGN = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";

        /// <summary>
        /// The PMCID for the request
        /// </summary>
        private string _pmcID;
        /// <summary>
        /// The OneSite PMCID|LoginName for the user being used
        /// </summary>
        private string _systemIdentifier;

        // Services
        private IOneSiteProductService _service = new OneSiteProductService();

        private IManageUnifiedLogin _unifiedLogin ;


        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="userClaims"></param>
        public ManageProductOneSite(DefaultUserClaim userClaims) 
            : base((int)ProductEnum.OneSite, userClaims,productInternalSettingRepository: null, productRepository: null)
        {
            _unifiedLogin = new ManageUnifiedLogin(userClaims);
            _productId = (int)ProductEnum.OneSite;
            _userClaims = userClaims;
            _editorRealPageId = userClaims.UserRealPageGuid;
            _blueBook = new ManageBlueBook(userClaims);

            _onesiteUrl = _productInternalSettingList.First(a => a.Name.ToUpper() == "APIENDPOINT").Value;
            _username = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIUSERNAME").Value));
            _password = Encoding.UTF8.GetString(Convert.FromBase64String(_productInternalSettingList.First(a => a.Name.ToUpper() == "APIPASSWORD").Value));

            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            _mtTokenEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTTOKENENDPOINT").Value;
            _mtClientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTID").Value;
            _mtClientSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTSECRET").Value;

            _service.Url = _onesiteUrl;
            _service.PreAuthenticate = true;
            _service.Credentials = new System.Net.NetworkCredential(_username, _password);
            //ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            //Guid realGuid;
            //if (Guid.TryParse((from nvp in currentClaimPrincipal.Claims where nvp.Type == "realPageId" select nvp.Value).FirstOrDefault(), out realGuid))
            //    _editorRealPageId = realGuid;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaim"></param>
        /// <param name="service"></param>
        /// <param name="samlRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="repository"></param>
        public ManageProductOneSite(Guid editorRealPageId, DefaultUserClaim userClaim, IOneSiteProductService service, ISamlRepository samlRepository,
            IManagePersona managePersona, IManageBlueBook manageBlueBook, IProductRepository productRepository,
            IProductInternalSettingRepository productInternalSettingRepository, HttpMessageHandler messageHandler, IRepository repository)
            : base((int)ProductEnum.OneSite, userClaim, repository, messageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _service = service;
            _userClaims = userClaim;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _messageHandler = messageHandler;

            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            _mtTokenEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTTOKENENDPOINT").Value;
            _mtClientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTID").Value;
            _mtClientSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTSECRET").Value;
        }

        public ManageProductOneSite(DefaultUserClaim userClaim, IOneSiteProductService service,
          IManageBlueBook manageBlueBook,  IProductInternalSettingRepository productInternalSettingRepository, HttpMessageHandler messageHandler, IRepository repository)
         : base((int)ProductEnum.OneSite, userClaim, repository, messageHandler)
        {
            _service = service;
            _blueBook = manageBlueBook;
            _productInternalSettingRepository = productInternalSettingRepository;
            _messageHandler = messageHandler;
            _productRepository = new ProductRepository(repository, userClaim);
            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            _mtTokenEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTTOKENENDPOINT").Value;
            _mtClientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTID").Value;
            _mtClientSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTSECRET").Value;
        }


        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="editorRealPageId"></param>
        /// <param name="userClaim"></param>
        /// <param name="messageHandler"></param>
        /// <param name="service"></param>
        /// <param name="userList"></param>
        /// <param name="roleList"></param>
        /// <param name="rightList"></param>
        /// <param name="propertyList"></param>
        /// <param name="samlRepository"></param>
        /// <param name="managePersona"></param>
        /// <param name="personaRepository"></param>
        /// <param name="managePerson"></param>
        /// <param name="userLoginRepository"></param>
        /// <param name="manageUserLogin"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="productRepository"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="managePartyRelationship"></param>
        /// <param name="manageElectronicAddress"></param>
        /// <param name="userLoginPersonaRepository"></param>
        /// <param name="userRepository"></param>
        /// <param name="repository"></param>
        /// <param name="systemIdentifier"></param>
        /// <param name="UnifiedLogin"></param>
        public ManageProductOneSite(Guid editorRealPageId, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, IOneSiteProductService service, UserList userList, RoleList roleList, RightList rightList, PropertyList propertyList, ISamlRepository samlRepository, IManagePersona managePersona, IPersonaRepository personaRepository, IManagePerson managePerson, IUserLoginRepository userLoginRepository, IManageUserLogin manageUserLogin, IManageBlueBook manageBlueBook, IProductRepository productRepository, IProductInternalSettingRepository productInternalSettingRepository, IManagePartyRelationship managePartyRelationship, IManageElectronicAddress manageElectronicAddress, IUserLoginPersonaRepository userLoginPersonaRepository, IUserRepository userRepository, IRepository repository, string systemIdentifier, IManageUnifiedLogin UnifiedLogin = null) : base((int)ProductEnum.OneSite, userClaim, repository, messageHandler)
        {
            _editorRealPageId = editorRealPageId;
            _service = service;
            _userList = userList;
            _rightList = rightList;
            _roleList = roleList;
            _propertyList = propertyList;
            _samlRepository = samlRepository;
            _managePersona = managePersona;
            _managePerson = managePerson;
            _personaRepository = personaRepository;
            _manageUserLogin = manageUserLogin;
            _blueBook = manageBlueBook;
            _productRepository = productRepository;
            _productInternalSettingRepository = productInternalSettingRepository;
            _managePartyRelationship = managePartyRelationship;
            _manageElectronicAddress = manageElectronicAddress;
            _userRepository = userRepository;
            _userLoginPersonaRepository = userLoginPersonaRepository;
            _userClaims = userClaim;
            _unifiedLogin = UnifiedLogin;
            _systemIdentifier = systemIdentifier;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaims"></param>
        /// <param name="messageHandler"></param>
        /// <param name="oneSiteProductService"></param>
        public ManageProductOneSite(IRepository repository, DefaultUserClaim userClaims, HttpMessageHandler messageHandler, IOneSiteProductService oneSiteProductService)
            : base((int)ProductEnum.OneSite, userClaims, repository, messageHandler)
        {
            _editorRealPageId = userClaims.UserRealPageGuid;
            _service = oneSiteProductService;
            _samlRepository = new SamlRepository(repository);
            _managePersona = new ManagePersona(repository, userClaims, messageHandler);
            _blueBook = new ManageBlueBook(userClaims, repository, messageHandler);
            _productRepository = new ProductRepository(repository, userClaims);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _messageHandler = messageHandler;
            _mtApiEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTAPIENDPOINT").Value;
            _mtTokenEndPoint = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTTOKENENDPOINT").Value;
            _mtClientId = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTID").Value;
            _mtClientSecret = _productInternalSettingList.First(a => a.Name.ToUpper() == "MTCLIENTSECRET").Value;
        }

        #region Property
        /// <summary>
        /// Used to get a list of OneSite properties for the given user
        /// </summary>
        /// <param name="editorPersonaId">Used to uniquely identify the OneSite user. PMCID|loginname</param>
        /// <param name="userPersonaId"></param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter">A sorting or filtering object for the property</param>
        /// <returns></returns>
        public ListResponse GetOneSitePropertyList(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();

            try
            {
                response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
                if (response.IsError) { return response; }
                if (userPersonaId != 0)
                {
                    _pmcID = GetOneSitePMCIDFromPersona(_userPersona);
                }
                else
                {
                    _pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
                }
                FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "SiteName", 0, 9999);
                PropertyList propertyList = new PropertyList();
                Dictionary<string, object> logData = new Dictionary<string, object>();
                OneSiteUser onesiteuser = new OneSiteUser();
                
                if (!string.IsNullOrEmpty(_systemIdentifier))
                {
                    onesiteuser = GetOneSiteUserInfo(_systemIdentifier);
                    logData = new Dictionary<string, object>
                    {
                        { "wsParams", wsParams },
                        { "_systemIdentifier", _systemIdentifier }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetOneSitePropertyList", "Getting property list user" });
                    if (string.IsNullOrEmpty(onesiteuser.SystemIdentifier))
                    {
                        throw new Exception("Unable to locate user info");
                    }
                    propertyList = _service.GetUserProperties(_systemIdentifier, assignedOnly, wsParams);
                }
                else
                {
                    Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", _pmcID } };
                    NameValuePair[] uiArgs = (from a in args.ToList() select new NameValuePair { Name = a.Key, Value = a.Value }).ToArray();
                    logData = new Dictionary<string, object>
                    {
                        { "wsParams", wsParams },
                        { "uiArgs", uiArgs }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetOneSitePropertyList", "Getting property list all" });
                    propertyList = _service.GetAllProperties(uiArgs, _systemIdentifier, wsParams);
                }
                logData = new Dictionary<string, object> { { "propertyList", propertyList } };
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetOneSitePropertyList", "Got property list" });

                Dictionary<string, bool> allProperties = new Dictionary<string, bool> { { "allProperties", onesiteuser.AllProperties } };

                if (propertyList == null) { propertyList = new PropertyList(); }
                IList<ProductProperty> list = propertyList.ToGBProperties();
                if (list == null) { list = new List<ProductProperty>(); }
                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = propertyList.TotalProperties,
                    RowsPerPage = datafilter == null ? list.Count() : datafilter.Pages.ResultsPerPage,
                    TotalPages = 1,
                    ErrorReason = "",
                    Additional = allProperties
                };
                return response;
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetOneSitePropertyList", $"Error. {ex.Message}" });
                response = new ListResponse();
                response.IsError = true;

                if (ex is BlueBookException blueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.PropertyErrorMessage;
                }

                return response;
            }

        }

        /// <summary>
        /// Used to get a list of all OneSite properties for the company
        /// </summary>
        /// <param name="persona">Used to get the list property list for the current logged in user</param>
        /// <param name="datafilter">A sorting or filtering object for the property</param>
        /// <returns></returns>
        public ListResponse GetOneSitePropertyListAll(Persona persona, RequestParameter datafilter)
        {

            string uniqueIdentifier = "";// GetOneSiteUniqueIdByPersonaId(persona.PersonaId);
            string pmcID = GetOneSitePMCIDFromPersona(persona);

            Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", pmcID } };
            _propertyList = GetOneSitePropertyListMain(args, datafilter, uniqueIdentifier);
            IList<ProductProperty> list = _propertyList.ToGBProperties();
            if (list == null) { list = new List<ProductProperty>(); }
            ListResponse response = new ListResponse()
            {
                Records = list.Cast<object>().ToList(),
                TotalRows = _propertyList.TotalProperties,
                RowsPerPage = 9999,
                TotalPages = 1,
                ErrorReason = ""
            };
            return response;
        }

        /// <summary>
        /// Used to get a list of all OneSite properties for the company
        /// </summary>
        /// <param name="args">Used to filter the property list to a specific PMC</param>
        /// <param name="datafilter">A sorting or filtering object for the property</param>
        /// <param name="uniqueIdentifier">Used to identify in the result of a specific OneSite user has been given access. PMCID|loginname</param>
        /// <returns></returns>
        public PropertyList GetOneSitePropertyListMain(Dictionary<string, string> args, RequestParameter datafilter, string uniqueIdentifier)
        {
            PropertyList propertyListResult = new PropertyList();
            try
            {
                FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "SiteName", 0, 9999);
                NameValuePair[] uiArgs = (from a in args.ToList() select new NameValuePair { Name = a.Key, Value = a.Value }).ToArray();
                propertyListResult = _service.GetAllProperties(uiArgs, uniqueIdentifier, wsParams);
            }
            catch (Exception ex) { }
            return propertyListResult;
        }


        /// <summary>
        /// Used to get a list of users associated to the given property id
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="propertyId"></param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUsersForProperty(long editorPersonaId, int propertyId, bool assignedOnly, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            UserList userListResult = new UserList();
            try
            {
                FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "UserName", 0, 9999);
                _userList = _service.GetUsersForProperty(_systemIdentifier, propertyId, assignedOnly, wsParams);
            }
            catch (Exception ex) { }
            IList<Component.SharedObjects.Product.ProductUser> list = _userList.ToGBUsers();
            if (list == null) { list = new List<Component.SharedObjects.Product.ProductUser>(); }
            response = new ListResponse()
            {
                Records = list.Cast<object>().ToList(),
                TotalRows = _userList.TotalUsers,
                RowsPerPage = 9999,
                ErrorReason = "",
                TotalPages = 1
            };
            return response;
        }

        /// <summary>
        /// Used to update/remove properties for the given OneSite user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="propertiesToAssign"></param>
        /// <param name="additionalParameters"></param>
        /// <returns>Count of records added or deleted, or All if all properties given to user</returns>
        public string UpdatePropertiesForUser(long editorPersonaId, long userPersonaId, List<string> propertiesToAssign, out List<AdditionalParameters> additionalParameters)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Beginning update to properties" });
            string propertyIDAddList = "";
            string propertyIDRemoveList = "";
            List<string> propertiesToRemove = new List<string>();
            string resultCount = "";

            additionalParameters = new List<AdditionalParameters>();
            PropertyList userCurrentPropertyList;
            // check to see if the user is a superuser and if so don't update the roles
            bool superUser = IsSuperUser(userPersonaId);
            if (superUser)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "User is superuser, no change" });
                return resultCount;
            }

            if (propertiesToAssign[0].ToUpper() != "ALL")
            {
                string PMCID = _systemIdentifier.Split('|')[0];
                Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", PMCID } };
                RequestParameter datafilter = new RequestParameter() { Pages = new PageRequest() { ResultsPerPage = 9999 } };
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Getting current user properties" });
                userCurrentPropertyList = GetOneSitePropertyListMain(args, datafilter, _systemIdentifier);
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Parsing properties to determine what to add/delete" });
                // compare the current property list to what was passed to determine what is new and what was removed.

                //Fix for bug GB-7138
                var onesiteUserInfo = GetOneSiteUserInfo(_systemIdentifier);
                if (onesiteUserInfo != null && onesiteUserInfo.AllProperties && userCurrentPropertyList.Property.Count() == propertiesToAssign.Count)
                {
                    if (propertiesToAssign.Count > 0)
                    {
                        var assignedProp = userCurrentPropertyList.Property
                                .Where(f => propertiesToAssign.Contains(f.PropertyID))
                                .Select(f => new AdditionalParameters { Key = "OneSite Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.PropertyName) })
                                .ToList();

                        additionalParameters.AddRange(assignedProp);
                        propertyIDAddList = string.Join("|", propertiesToAssign);
                    }
                }
                else
                {
                    foreach (PropertyType prop in userCurrentPropertyList.Property)
                    {
                        if (!(propertiesToAssign.Contains(prop.PropertyID)) && prop.IsAssignedToUser)
                        {
                            // property doesn't exist, so add it to the list
                            propertiesToRemove.Add(prop.PropertyID);
                        }
                        if (propertiesToAssign.Contains(prop.PropertyID) && prop.IsAssignedToUser)
                        {
                            propertiesToAssign.Remove(prop.PropertyID);
                        }
                    }

                    if (propertiesToAssign.Count > 0)
                    {
                        var assignedProp = userCurrentPropertyList.Property
                                .Where(f => propertiesToAssign.Contains(f.PropertyID))
                                .Select(f => new AdditionalParameters { Key = "OneSite Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", f.PropertyName) })
                                .ToList();

                        additionalParameters.AddRange(assignedProp);
                        propertyIDAddList = string.Join("|", propertiesToAssign);
                    }

                    if (propertiesToRemove.Count > 0)
                    {
                        var unAssignedProp = userCurrentPropertyList.Property
                                .Where(f => propertiesToRemove.Contains(f.PropertyID))
                                .Select(f => new AdditionalParameters { Key = "OneSite Properties", Value = PRODUCT_PROPERTIES_REMOVED_MESSAGE.Replace("PropertyName", f.PropertyName) })
                                .ToList();

                        additionalParameters.AddRange(unAssignedProp);
                        propertyIDRemoveList = string.Join("|", propertiesToRemove);
                    }
                }

                resultCount = (propertiesToAssign.Count + propertiesToRemove.Count).ToString();
            }
            else
            {
                // the user is begin given all properties, so call the service to give all properties
                propertyIDAddList = "ALL";
                resultCount = "All";
                additionalParameters.Add(new AdditionalParameters { Key = "OneSite Properties", Value = PRODUCT_PROPERTIES_ASSIGN_MESSAGE.Replace("PropertyName", propertyIDAddList) });
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Build add/remove property list" });
            try
            {
                if (!string.IsNullOrWhiteSpace(propertyIDRemoveList))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Posting to remove properties from user" });
                    AssignStatus removeStatus = _service.RemovePropertiesFromUser(_systemIdentifier, propertyIDRemoveList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Done posting to remove properties from user" });
                }

                if (!string.IsNullOrWhiteSpace(propertyIDAddList))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Posting to add properties to user" });
                    AssignStatus addStatus = _service.AssignPropertiesToUser(_systemIdentifier, propertyIDAddList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdatePropertiesForUser", "Done posting to add properties to user" });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdatePropertiesForUser", $"Encountered error adding properties to user. Error {ex.Message}" });
            }

            return resultCount;
        }

        #endregion

        #region Roles
        /// <summary>
        /// Used to get a list of OneSite roles for the given user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">Used to filter the list to a specific OneSite user. PMCID|loginname</param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter">A sorting or filtering object for the role</param>
        /// <returns></returns>
        public ListResponse GetOneSiteRoleList(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);

            FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "RoleName", 0, 9999, false);
            RoleList roleList = null;
            OneSiteUser onesiteuser = new OneSiteUser();
            try
            {
                if (!string.IsNullOrEmpty(_systemIdentifier))
                {
                    onesiteuser = GetOneSiteUserInfo(_systemIdentifier);
                    var logData = new Dictionary<string, object>
                    {
                        { "wsParams", wsParams },
                        { "_systemIdentifier", _systemIdentifier }
                    };
                    WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "GetOneSiteRoleList", "Getting role list user" });
                    if (string.IsNullOrEmpty(onesiteuser.SystemIdentifier))
                    {
                        throw new Exception("Unable to locate user info");
                    }
                    roleList = _service.GetUserRoles(_systemIdentifier, assignedOnly, wsParams);
                }
                else
                {
                    // the user passed doesn't have a login yet so get the full list
                    return GetOneSiteRoleListAll(editorPersonaId, datafilter);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetOneSiteRoleList", $"Error. {ex.Message}" });
                response = new ListResponse();
                response.IsError = true;

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                }
            }

            if (roleList == null) { roleList = new RoleList(); }
            IList<ProductRole> list = roleList.ToGBRoles();
            if (list == null) { list = new List<ProductRole>(); }
            response = new ListResponse()
            {
                Records = list.Cast<object>().ToList(),
                TotalRows = list.Count,
                RowsPerPage = 9999,
                ErrorReason = "",
                TotalPages = 1
            };
            return response;
        }

        /// <summary>
        /// Used to get a list of OneSite roles for the company
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter">A sorting or filtering object for the role</param>
        /// <returns></returns>
        public ListResponse GetOneSiteRoleListAll(long editorPersonaId, RequestParameter datafilter)
        {
            ListResponse response;
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSiteRoleListAll", "Begin" });

                response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
                _pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
                if (response.IsError) 
                {
                    return response; 
                }

                if (string.IsNullOrWhiteSpace(_pmcID))
                {
                    throw new BlueBookException(CommonMessageConstants.CompanyErrorMessage);
                }

                Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", _pmcID } };
                _roleList = GetOneSiteRoleListMain(args, datafilter, _systemIdentifier);
                IList<ProductRole> list = _roleList.ToGBRoles();
                if (list == null) { list = new List<ProductRole>(); }
                // set all the isactive to false because OneSite may return the roles that the editor has assigned
                foreach (ProductRole pr in list)
                {
                    pr.IsAssigned = false;
                }

                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    ErrorReason = "",
                    TotalPages = 1
                };

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSiteRoleListAll", "End" });
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetOneSiteRoleListAll", $"Error. {ex.Message}" });
                response = new ListResponse();
                response.IsError = true;

                if (ex is BlueBookException)
                {
                    response.ErrorReason = ex.Message;
                }
                else
                {
                    response.ErrorReason = CommonMessageConstants.RoleErrorMessage;
                }
            }

            return response;
        }

        /// <summary>
        /// Used to update the roles for a given OneSite user
        /// </summary>
        /// <param name="editorPersonaId">The id of the user making the changes</param>
        /// <param name="userPersonaId">The id of the user being updated</param>
        /// <param name="rolesToAssign">A list of roles to assign to the user</param>
        /// <param name="additionalParameters">Detailed log of Audit data</param>
        /// <returns>A count of the number of changes made</returns>
        public string UpdateRolesForUser(long editorPersonaId, long userPersonaId, List<string> rolesToAssign, out List<AdditionalParameters> additionalParameters)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Beginning update to roles" });
            var response = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);

            string roleIDAddList = "";
            string roleIDRemoveList = "";
            List<string> rolesToRemove = new List<string>();
            string resultCount = "";
            
            bool superUser = IsSuperUser(userPersonaId);
            additionalParameters = new List<AdditionalParameters>();

            string PMCID = _systemIdentifier.Split('|')[0];
            Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", PMCID } };
            RequestParameter datafilter = new RequestParameter() { Pages = new PageRequest() { ResultsPerPage = 9999 } };
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Getting current user roles" });
            RoleList userCurrentRoleList = GetOneSiteRoleListMain(args, datafilter, _systemIdentifier);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Parsing roles to determine what to add/delete" });
            // compare the current property list to what was passed to determine what is new and what was removed.
            foreach (RoleType role in userCurrentRoleList.Role)
            {
                if (role.IsInternal && (!OneSiteHelpers.IsValidRoleForCustomer(role, false)))
                {
                    // if trying to add an internal role and the user already has it, don't do anything
                    if (rolesToAssign.Contains(role.RoleID) && role.IsAssigned)
                    {
                        rolesToAssign.Remove(role.RoleID);
                    }
                    // leave the role alone in the user
                    continue;
                }
                if (!(rolesToAssign.Contains(role.RoleID)))
                {
                    // check to see if the user is a superuser if so then add e-doc signer role
                    if (superUser && role.RoleName.ToUpper().Equals("E-DOC SIGNER"))
                    {
                        if (!role.IsAssigned)
                        {
                            rolesToAssign.Add(role.RoleID);
                        }
                    }
                    else if (role.IsAssigned)
                    {
                        // role doesn't exist, so add it to the list
                        rolesToRemove.Add(role.RoleID);
                    }
                }
                if (rolesToAssign.Contains(role.RoleID) && role.IsAssigned)
                {
                    rolesToAssign.Remove(role.RoleID);
                }
            }

            if (rolesToAssign.Count > 0)
            {
                var assignedRoles = userCurrentRoleList.Role
                    .Where(f => rolesToAssign.Contains(f.RoleID))
                    .Select(f => new AdditionalParameters { Key = "OneSite Roles", Value = PRODUCT_ROLES_ASSIGN_MESSAGE.Replace("RoleName", f.RoleName) })
                    .ToList();

                additionalParameters.AddRange(assignedRoles);
                roleIDAddList = string.Join("|", rolesToAssign);
            }

            if (rolesToRemove.Count > 0)
            {
                var removedRoles = userCurrentRoleList.Role
                    .Where(f => rolesToRemove.Contains(f.RoleID))
                    .Select(f => new AdditionalParameters { Key = "OneSite Roles", Value = PRODUCT_ROLES_REMOVED_MESSAGE.Replace("RoleName", f.RoleName) })
                    .ToList();

                additionalParameters.AddRange(removedRoles);
                roleIDRemoveList = string.Join("|", rolesToRemove);
            }
            resultCount = (rolesToAssign.Count + rolesToRemove.Count).ToString();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Build add/remove role list" });
            try
            {
                if (!string.IsNullOrWhiteSpace(roleIDRemoveList))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Posting to remove roles from user" });
                    AssignStatus removeStatus = _service.RemoveRolesFromUser(_systemIdentifier, roleIDRemoveList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Done posting to remove roles from user" });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRolesForUser", $"Encountered error removing roles from user. Error :{ex.Message}" });
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(roleIDAddList))
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Posting to add roles to user" });
                    AssignStatus addStatus = _service.AssignRolesToUser(_systemIdentifier, roleIDAddList);
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateRolesForUser", "Done posting to add roles to user" });
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UpdateRolesForUser", $"Encountered error adding roles to user. Error :{ex.Message}" });
            }

            return resultCount;
        }

        /// <summary>
        /// Used to get a list of all OneSite roles for the company
        /// </summary>
        /// <param name="args">Used to filter the role list to a specific PMC</param>
        /// <param name="datafilter">A sorting or filtering object for the role</param>
        /// <param name="uniqueIdentifier">Used to identify in the result of a specific OneSite user has been given access. PMCID|loginname</param>
        /// <returns></returns>
        public RoleList GetOneSiteRoleListMain(Dictionary<string, string> args, RequestParameter datafilter, string uniqueIdentifier)
        {
            RoleList roleListResult = new RoleList();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSiteRoleListMain", "Begin get role list" });
            try
            {
                FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "RoleName", 0, 9999);
                NameValuePair[] uiArgs = (from a in args.ToList() select new NameValuePair { Name = a.Key, Value = a.Value }).ToArray();
                WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "uiArgs", uiArgs }, { "wsParams", wsParams } }, messageProperties: new object[] { "GetOneSiteRoleListMain", "Getting role list" });
                roleListResult = _service.GetAllRoles(uiArgs, uniqueIdentifier, wsParams);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetOneSiteRoleListMain", $"Error getting role list. Error: {ex.Message}" });
            }
            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() {{ "roleListResult", roleListResult } }, messageProperties: new object[] { "GetOneSiteRoleListMain", "Finished getting role list" });

            return roleListResult;
        }

        /// <summary>
        /// Used to get the list of centers available for the PMC rights.
        /// </summary>
        /// <param name="editorPersonaId">The user making the call to get the rights</param>
        /// <returns></returns>
        public ListResponse GetOneSiteRightsCenters(long editorPersonaId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            _pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
            if (response.IsError) { return response; }

            Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", _pmcID } };
            RightCenter rightCenter = new RightCenter();
            try
            {
                NameValuePair[] uiArgs = (from a in args.ToList() select new NameValuePair { Name = a.Key, Value = a.Value }).ToArray();
                rightCenter = _service.GetRightsCentersList(uiArgs, _systemIdentifier);
            }
            catch (Exception ex) { }
            if (rightCenter == null) { rightCenter = new RightCenter(); }
            response = new ListResponse()
            {
                Records = rightCenter.RightCenters.Cast<object>().ToList(),
                TotalRows = rightCenter.RightCenters.Length,
                RowsPerPage = 9999,
                ErrorReason = "",
                TotalPages = 1
            };
            return response;
        }

        /// <summary>
        /// Used to get a list of users associated to the given role id
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId"></param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetUsersForRole(long editorPersonaId, int roleId, bool assignedOnly, RequestParameter datafilter)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            UserList userListResult = new UserList();
            try
            {
                FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "UserName", 0, 9999);
                _userList = _service.GetUsersForRole(_systemIdentifier, roleId, assignedOnly, wsParams);
            }
            catch (Exception ex) { }
            IList<Component.SharedObjects.Product.ProductUser> list = _userList.ToGBUsers();
            if (list == null) { list = new List<Component.SharedObjects.Product.ProductUser>(); }
            response = new ListResponse()
            {
                Records = list.Cast<object>().ToList(),
                TotalRows = list.Count,
                RowsPerPage = 9999,
                ErrorReason = "",
                TotalPages = 1
            };
            return response;
        }

        /// <summary>
        /// Used to get the list of rights.
        /// </summary>
        /// <param name="editorPersonaId">The user making the call to get the rights</param>
        /// <param name="datafilter"></param>
        /// <param name="roleId">Default to null. If passed, returns which rights are assigned to the given role id.</param>
        /// <param name="assignedToRoleOnly">Only return rights assigned to the requested role</param>
        /// <returns></returns>
        public ListResponse GetOneSiteRights(long editorPersonaId, RequestParameter datafilter, long roleId = 0, bool assignedToRoleOnly = false)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            try
            {
                _pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "PMCID", _pmcID },
                    { "RoleID", roleId.ToString() },
                    { "AssignedToRoleOnly", (assignedToRoleOnly ? "1" : "0") }
                };

                _rightList = GetOneSiteRightsMain(args, datafilter, _systemIdentifier);

                if (_rightList == null) { _rightList = new RightList(); }
                IList<ProductRight> list = _rightList.ToGBRights();
                if (list == null) { list = new List<ProductRight>(); }
                response = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = list.Count,
                    RowsPerPage = 9999,
                    ErrorReason = "",
                    TotalPages = 1
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetOneSiteRights", $"Error for user with editorPersona id - {editorPersonaId}. Error: {ex.Message}" });

                response = new ListResponse
                {
                    IsError = true,
                    ErrorReason = CommonMessageConstants.RightErrorMessage
                };
            }
            return response;
        }

        /// <summary>
        /// Used to get the list of rights from OneSite
        /// </summary>
        /// <param name="args"></param>
        /// <param name="datafilter"></param>
        /// <param name="uniqueIdentifier"></param>
        public RightList GetOneSiteRightsMain(Dictionary<string, string> args, RequestParameter datafilter, string uniqueIdentifier)
        {
            RightList rightListResult = new RightList();
            try
            {
                FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "RightDescription", 0, 9999);
                NameValuePair[] uiArgs = (from a in args.ToList() select new NameValuePair { Name = a.Key, Value = a.Value }).ToArray();
                rightListResult = _service.GetRightsList(uiArgs, uniqueIdentifier, wsParams);
            }
            catch (Exception ex) { }
            return rightListResult;
        }

        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
        /// <param name="rightId">The right being assigned</param>
        /// <param name="roles">A list of role ids to update</param>
        /// <param name="assignRight">Is the right being added or removed</param>
        public string UpdateRightToRoles(long editorPersonaId, int rightId, List<string> roles, bool assignRight)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response.ErrorReason; }
            string roleIdList = string.Join("|", roles);
            // the OneSite user making the change to the role
            AssignStatus status = new AssignStatus();
            try
            {
                if (!string.IsNullOrWhiteSpace(roleIdList))
                {
                    status = _service.ModifyRightToRoles(_systemIdentifier, rightId, roleIdList, assignRight);
                    if (string.IsNullOrEmpty(status.ErrorMessage))
                    {
                        if (roles.Any())
                        {
                            if (assignRight == true)
                                UpdateRolesByRightLogMessage(editorPersonaId, rightId, roles, null);
                            else
                                UpdateRolesByRightLogMessage(editorPersonaId, rightId, null, roles);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ParseSoapErrorMessage(ex.Message);
            }
            return status.ErrorMessage;
        }

        /// <summary>
        /// Used to assign or unassign a right to a list of roles
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
        /// <param name="roleId">The role being assigned</param>
        /// <param name="rightsToAdd">A list of right ids to add to the role</param>
        /// <param name="rightsToRemove">A list of right ids to remove from the role</param>
        public string UpdateRoleToRights(long editorPersonaId, int roleId, List<string> rightsToAdd, List<string> rightsToRemove)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response.ErrorReason; }

            string rightsToAddList = "";
            if (rightsToAdd != null && rightsToAdd.Count > 0)
            {
                rightsToAddList = string.Join("|", rightsToAdd);
            }
            string rightsToRemoveList = "";
            if (rightsToRemove != null && rightsToRemove.Count > 0)
            {
                rightsToRemoveList = string.Join("|", rightsToRemove);
            }

            AssignStatus status = new AssignStatus();
            try
            {
                if (!string.IsNullOrWhiteSpace(rightsToAddList) || !string.IsNullOrWhiteSpace(rightsToRemoveList))
                {
                    status = _service.ModifyRoleToRights(_systemIdentifier, roleId, rightsToAddList, rightsToRemoveList);
                    if (string.IsNullOrEmpty(status.ErrorMessage))
                    {
                        if (rightsToAdd.Any() || rightsToRemove.Any())
                        {
                            UpdateRightsToRoleLogMessage(editorPersonaId, roleId, rightsToAdd, rightsToRemove);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ParseSoapErrorMessage(ex.Message);
            }

            return status.ErrorMessage;
        }

        public void UpdateRightsToRoleLogMessage(long editorPersonaId, long roleId, List<string> rightsToAdd, List<string> rightsToRemove)
        {
            try
            {
                ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
                UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);

                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                var pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
                Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", pmcID } };
                var roles = GetOneSiteRoleListMain(args, null, _systemIdentifier);
                var rolesList = roles.ToGBRoles();
                var roleName = "";
                if (rolesList != null)
                { roleName = rolesList.FirstOrDefault(r => r.ID == roleId.ToString())?.Name; }
                Dictionary<string, string> args1 = new Dictionary<string, string>
                {
                    { "PMCID", pmcID },
                    { "RoleID", "0" },
                    { "AssignedToRoleOnly", "0" }
                };

                var rightList = GetOneSiteRightsMain(args1, null, _systemIdentifier);

                if (rightList == null) { rightList = new RightList(); }
                var list = rightList.ToGBRights();
                if (rightsToAdd != null)
                {
                    foreach (var right in rightsToAdd)
                    {
                        var rightName = list.FirstOrDefault(r => r.ID.ToString() == right)?.Description;
                        additionalParameters.Add(new AdditionalParameters { Key = "OneSite " + roleName, Value = RIGHT_ASSIGN.Replace("RightName", rightName) });
                    }
                }
                if (rightsToRemove != null)
                {
                    foreach (var right in rightsToRemove)
                    {
                        var rightName = list.FirstOrDefault(r => r.ID.ToString() == right)?.Description;
                        additionalParameters.Add(new AdditionalParameters { Key = "OneSite " + roleName, Value = RIGHT_UNASSIGN.Replace("RightName", rightName) });
                    }
                }
                var message = "";
                message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed rights to {roleName} in OneSite."
                : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed rights to {roleName} in OneSite.";
                
                unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters);
            }
            catch { return; }
        }

        public void UpdateRolesByRightLogMessage(long editorPersonaId, long rightId, List<string> rolesToAdd, List<string> rolesToRemove)
        {
            try
            {
                ManageUnifiedLogin unifiedLogin = new ManageUnifiedLogin(_userClaims);
                var fromUserLogInfo = GetUserActivityLogInfo(editorPersonaId);
                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                UserDetails impersonatorUserInfo = unifiedLogin.impersonatorUserDetails(_userClaims.ImpersonatedBy);
                var pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
                Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", pmcID } };
                var roles = GetOneSiteRoleListMain(args, null, _systemIdentifier);
                var rolesList = roles.ToGBRoles();
                var assignedRights = new List<string>();
                Dictionary<string, string> args1 = new Dictionary<string, string>
                {
                    { "PMCID", pmcID },
                    { "RoleID", "0" },
                    { "AssignedToRoleOnly", "0" }
                };
                var rightList = GetOneSiteRightsMain(args1, null, _systemIdentifier);

                if (rightList == null) { rightList = new RightList(); }
                IList<ProductRight> list = rightList.ToGBRights();
                var rightName = list.FirstOrDefault(r => r.ID == rightId)?.Description;
                if (rolesToAdd != null)
                {
                    foreach (var role in rolesToAdd)
                    {
                        var roleName = rolesList.FirstOrDefault(r => r.ID == role.ToString())?.Name;
                        additionalParameters.Add(new AdditionalParameters { Key = "OneSite " + rightName, Value = ROLE_ASSIGN.Replace("RoleName", roleName) });
                    }
                }
                if (rolesToRemove != null)
                {
                    foreach (var role in rolesToRemove)
                    {
                        var roleName = rolesList.FirstOrDefault(r => r.ID == role.ToString())?.Name;
                        additionalParameters.Add(new AdditionalParameters { Key = "OneSite " + rightName, Value = ROLE_UNASSIGN.Replace("RoleName", roleName) });
                    }
                }
                var message = "";
                message = impersonatorUserInfo != null
                  ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added/Removed roles to {rightName} in OneSite."
                : $"{fromUserLogInfo.FirstName} {fromUserLogInfo.LastName} Added/Removed roles to {rightName} in OneSite.";
               
                unifiedLogin.PushToQueue(fromUserLogInfo, message, additionalParameters);
            }
            catch { return; }
        }
        /// <summary>
        /// Used to add/update a role in OneSite
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <param name="inheritRoleId"></param>
        /// <returns></returns>
        public ListResponse AddUpdateRole(long editorPersonaId, int roleId, string roleName, string inheritRoleId)
        {
            RoleList roleListResult = new RoleList();
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }

            AssignStatus status = new AssignStatus();
            RoleList roleList = new RoleList();
            string roleToAlter = "";
            var oldRoleName = "";
            try
            {
                if (roleId == 0)
                {
                    roleToAlter = "";
                }
                else
                {
                    roleToAlter = roleId.ToString();
                    var pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
                    Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", pmcID } };
                    var roles = GetOneSiteRoleListMain(args, null, _systemIdentifier);
                    IList<ProductRole> rolesList = roles.ToGBRoles();
                    if (rolesList != null)
                        oldRoleName = rolesList.FirstOrDefault(r => r.ID == roleId.ToString())?.Name;
                }
                roleList = _service.AddUpdateRole(_systemIdentifier, roleToAlter, roleName, inheritRoleId);
                status.ErrorMessage = string.Empty;

                if (roleToAlter == "")
                {
                    _unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, roleName, "ADD", "OneSite");
                }
                else
                {
                    if (oldRoleName != roleName)
                    {
                        _unifiedLogin.AddUpdateRoleLogMessage(editorPersonaId, _userClaims.OrganizationPartyId, roleName, "UPDATE", "OneSite", oldRoleName);
                    }
                }
            
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ParseSoapErrorMessage(ex.Message);
            }

            IList<ProductRole> list = new List<ProductRole>();
            if (roleList.TotalRoles != 0)
            {
                list = roleList.ToGBRoles();
            }
            response = new ListResponse()
            {
                Records = list.Cast<object>().ToList(),
                TotalRows = list.Count,
                RowsPerPage = 9999,
                ErrorReason = status.ErrorMessage,
                TotalPages = 1
            };
            return response;
        }

        /// <summary>
        /// Used to delete a role
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user making the change. Used to log the OneSite user making the change.</param>
        /// <param name="roleId">The role being deleted</param>
        public string DeleteRole(long editorPersonaId, int roleId)
        {
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response.ErrorReason; }
            AssignStatus status = new AssignStatus() { ErrorMessage = "" };
            status.ErrorMessage = "";
            var roleName = "";
            try
            {
                var pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
                Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", pmcID } };
                var roleList = GetOneSiteRoleListMain(args, null, _systemIdentifier);
                IList<ProductRole> list = roleList.ToGBRoles();
                if(list != null)
                    roleName = list.FirstOrDefault(r => r.ID == roleId.ToString())?.Name;
                _service.DeleteRole(_systemIdentifier, roleId);
                _unifiedLogin.DeleteRoleLogMessage(editorPersonaId, roleId, roleName, "OneSite");
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ParseSoapErrorMessage(ex.Message);
            }
            return status.ErrorMessage;
        }

        /// <summary>
        /// Used to get a list of roles associated to the given right id
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="rightId"></param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        public ListResponse GetRolesForRight(long editorPersonaId, int rightId, bool assignedOnly, RequestParameter datafilter)
        {
            RoleList roleListResult = new RoleList();
            ListResponse response = new ListResponse();
            response = GetCompanyEditorAndUserDetails(editorPersonaId, editorPersonaId);
            if (response.IsError) { return response; }
            _pmcID = GetOneSitePMCIDFromPersona(_editorPersona);
            Dictionary<string, string> args = new Dictionary<string, string> { { "PMCID", _pmcID } };

            try
            {
                FilterSortParameters wsParams = OneSiteHelpers.GenerateSearchAndPaging(datafilter, "RightDescription", 0, 9999);
                NameValuePair[] uiArgs = (from a in args.ToList() select new NameValuePair { Name = a.Key, Value = a.Value }).ToArray();
                _roleList = _service.GetRolesForRight(uiArgs, rightId, assignedOnly, wsParams);
            }
            catch (Exception ex) { }
            IList<ProductRole> list = _roleList.ToGBRoles();

            response = new ListResponse()
            {
                Records = list.Cast<object>().ToList(),
                TotalRows = list.Count,
                RowsPerPage = 9999,
                ErrorReason = "",
                TotalPages = 1
            };
            return response;
        }
       
        #endregion

        #region User

        /// <summary>
        /// Unassign User
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <param name="deleteSamlUserProductInfoAndStatus">Optional: Delete all SAML product information and status for the OneSite user when changing the usertype from Admin to Regular user</param>
        /// <returns>String.empty if success else error</returns>
        public string UnassignUser(long editorPersonaId, long userPersonaId, bool deleteSamlUserProductInfoAndStatus = false)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            string disableOneSite = EnableOneSiteUser(editorPersonaId, userPersonaId, false);

            if (!string.IsNullOrEmpty((disableOneSite)))
            {
                return disableOneSite;
            }
            else
            {
                if (deleteSamlUserProductInfoAndStatus)
                {
                    //Delete all SAML product information and status for the OneSite user when changing the usertype from Admin to Regular user
                    RepositoryResponse repositoryResponse = _samlRepository.DeleteSamlUserProductInfoAndStatus(userPersonaId, _productId);
                }
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UnassignUser", $"userPersonaId:{userPersonaId}" });
            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);

            return "";
        }

        /// <summary>
        /// Used to get information about a OneSite user
        /// </summary>
        /// <param name="systemIdentifier">PMCID|UserLoginName</param>
        /// <returns></returns>
        public OneSiteUser GetOneSiteUserInfo(string systemIdentifier)
        {
            OneSiteUser osu = new OneSiteUser();
            Dictionary<string, object> resultData = new Dictionary<string, object>();

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "url", _service.Url } }, messageProperties: new object[] { "GetOneSiteUserInfo", $"Begin systemIdentifier={systemIdentifier}" });

            if (string.IsNullOrEmpty(systemIdentifier))
            {
                return null;
            }
            NameValuePair[] response;
            string PMCID = systemIdentifier.Split('|')[0];
            string LogonName = systemIdentifier.Split('|')[1];
            string userId = "";
            int maxTryCount = 5;
            int tryCount = 0;
            List<NameValuePair> userArray = new List<NameValuePair> {
                        new NameValuePair() { Name = "PMCID", Value = PMCID },
                        new NameValuePair() { Name = "LogonName", Value = LogonName }
                    };
            try
            {
                while (tryCount < maxTryCount)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userArray", userArray }, { "url", _service.Url } }, messageProperties: new object[] { "GetOneSiteUserInfo", $"Posting to service systemIdentifier={systemIdentifier}" });
                    response = _service.GetUser(userArray.ToArray());
                    WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", response }, { "url", _service.Url } }, messageProperties: new object[] { "GetOneSiteUserInfo", $"Response from service systemIdentifier={systemIdentifier}" });

                    if (response.Length > 0)
                    {
                        if (response.Any(a => a.Name.ToUpper() == "USERID"))
                        {
                            userId = (from a in response where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
                            if (userId.ToUpper() != "UNKNOWN")
                            {
                                osu = ParseOneSiteGetUser(response);
                                break;
                            }
                        }
                    }
                    tryCount++;
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", null, ex, messageProperties: new object[] { "GetOneSiteUserInfo", $"Response from service systemIdentifier={systemIdentifier}. Error: {ex.Message}" });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "url", _service.Url } }, messageProperties: new object[] { "GetOneSiteUserInfo", $"End systemIdentifier={systemIdentifier}" });
            return osu;
        }

        /// <summary>
        /// Used to parse the name/value result from the OneSite Get user service call
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private OneSiteUser ParseOneSiteGetUser(NameValuePair[] response)
        {
            OneSiteUser osu = new OneSiteUser();
            if (response.Any(a => a.Name.Equals("USERID", StringComparison.OrdinalIgnoreCase)))
            {
                osu.UserId = Convert.ToInt32((from a in response where a.Name.Equals("USERID", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault());
            }
            if (response.Any(a => a.Name.Equals("SYSTEMIDENTIFIER", StringComparison.OrdinalIgnoreCase)))
            {
                osu.SystemIdentifier = (from a in response where a.Name.Equals("SYSTEMIDENTIFIER", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
            }
            if (response.Any(a => a.Name.Equals("FIRSTNAME", StringComparison.OrdinalIgnoreCase)))
            {
                osu.FirstName = (from a in response where a.Name.Equals("FIRSTNAME", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
            }
            if (response.Any(a => a.Name.Equals("LASTNAME", StringComparison.OrdinalIgnoreCase)))
            {
                osu.LastName = (from a in response where a.Name.Equals("LASTNAME", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
            }
            if (response.Any(a => a.Name.Equals("USERPIN", StringComparison.OrdinalIgnoreCase)))
            {
                string userPin = (from a in response where a.Name.Equals("USERPIN", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
                if (!string.IsNullOrEmpty(userPin))
                {
                    osu.UserPin = Convert.ToInt32(userPin);
                }
            }
            if (response.Any(a => a.Name.Equals("USERALLPROPERTY", StringComparison.OrdinalIgnoreCase)))
            {
                string allProperties = (from a in response where a.Name.Equals("USERALLPROPERTY", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
                osu.AllProperties = (allProperties == "1" ? true : false);
            }
            if (response.Any(a => a.Name.Equals("USERTHIRDPARTYREFERENCE", StringComparison.OrdinalIgnoreCase)))
            {
                osu.UserThirdPartyReference = (from a in response where a.Name.Equals("USERTHIRDPARTYREFERENCE", StringComparison.OrdinalIgnoreCase) select a.Value).FirstOrDefault();
            }
            return osu;
        }

        /// <summary>
        /// Used to Add/Update a OneSite user record
        /// </summary>
        /// <param name="editorPersonaId">The persona of the user creating/updating the OneSite user</param>
        /// <param name="userPersonaId">The GreenBook user being altered</param>
        /// <param name="RoleList"></param>
        /// <param name="PropertyList"></param>
		/// <param name="isUserProfileChanged"></param>
        /// <param name="additionalParameters">Detailed log of Audit data</param>
        /// <returns></returns>
        public string ManageOneSiteUser(long editorPersonaId, long userPersonaId, List<string> RoleList, List<string> PropertyList, out List<AdditionalParameters> additionalParameters, bool isUserProfileChanged = false)
        {
            // Default to XXXX to tell OneSite to use existing pin
            string onesitePin = "XXXX";
            bool existingUser = false;
            string userThirdPartyReference = "";
            additionalParameters = new List<AdditionalParameters>();

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "Beginning" });

            // use the persona to get the user id, organization id and system identifier for the user
            //string pmcid = GetOneSitePMCIDFromPersona(AdminPersona);
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }
            _pmcID = GetOneSitePMCIDFromPersona(_editorPersona);

            string onesiteLoginName = "";

            //if (personaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid userRealPageId = userPersona.RealPageId;

            IC.Person person = _managePerson.GetPerson(userRealPageId);

            var userLogin = _manageUserLogin.GetUserLoginOnly(userRealPageId);

            IList<IC.UserLoginPersona> userLoginPersonaList = _userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userPersona.UserId, organizationPartyId: userPersona.Organization.PartyId);
            var employeeId = _userRepository.GetUserEmployeeId(userLoginPersonaList[0].UserLoginPersonaId, userPersona.OrganizationPartyId);
            person.EmployeeId = (employeeId != null && !string.IsNullOrEmpty(employeeId.EmployeeId)) ? employeeId.EmployeeId : null;


            if (string.IsNullOrEmpty(_systemIdentifier))
            {
                // generate a new random pin for the new user
                Random generator = new Random();
                onesitePin = generator.Next(1, 10000).ToString("D4");
            }
            else
            {
                onesiteLoginName = _systemIdentifier.Split('|')[1];
                existingUser = true;
            }

            // get the email address
            string userEmailAddress = "";
            IList<IC.ElectronicAddress> _addresses = _manageElectronicAddress.ListElectronicAddressForPerson(userLogin.RealPageId, "");
            if (_addresses.Any(a => a.AddressType?.ToUpper() == "EMAIL"))
            {
                userEmailAddress = (from a in _addresses where a.AddressType.ToUpper() == "EMAIL" select a.AddressString).FirstOrDefault();
                if (string.IsNullOrEmpty(userEmailAddress) && !IsRegularUserNoEmail(userPersonaId))
                {
                    // this must look like a real email address or Intact will fail to create the user
                    userEmailAddress = userLogin.LoginName;
                }
            }
            else if (!IsRegularUserNoEmail(userPersonaId))
            {
                // this must look like a real email address or Intact will fail to create the user
                userEmailAddress = userLogin.LoginName;
            }
            // verify email address looks valid, will fail if not
            if (!string.IsNullOrWhiteSpace(userEmailAddress))
            {
                userEmailAddress = ValidateAndReturnEmailAddress(userEmailAddress);
            }
            bool isSuperUser = IsSuperUser(userPersona.PersonaId);

            List<NameValuePair> userArray = new List<NameValuePair>();
            //Dictionary<string, object> logData = new Dictionary<string, object>();

            if (userLogin.PartyId > 0)
            {
                NameValuePair[] response;
                string errorMessage = "";
                userThirdPartyReference = person.EmployeeId;

                if (!string.IsNullOrWhiteSpace(_systemIdentifier))
                {
                    var onesiteuser = GetOneSiteUserInfo(_systemIdentifier);
                    if (person.EmployeeId != onesiteuser.UserThirdPartyReference)
                    {
                        userThirdPartyReference = person.EmployeeId;
                    }
                    else
                    {
                        userThirdPartyReference = onesiteuser.UserThirdPartyReference;
                    }
                }

                if (!isSuperUser)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "isSuperUser = false" });

                    // build the call to OneSite to create the user
                    userArray = new List<NameValuePair> {
                        new NameValuePair() { Name = "FirstName", Value = new string(person.FirstName.Where(Char.IsLetter).ToArray())},
                        new NameValuePair() { Name = "LastName", Value = new string(person.LastName.Where(Char.IsLetter).ToArray())},
                        new NameValuePair() { Name = "Pin", Value = onesitePin },
                        new NameValuePair() { Name = "ReferenceNumber", Value = userThirdPartyReference },
                        new NameValuePair() { Name = "PMCID", Value = _pmcID },
                        new NameValuePair() { Name = "IsSuperuser", Value = "0" },
                        new NameValuePair() { Name = "LogonName", Value = onesiteLoginName }, // leave empty login name so OneSite will create one
						new NameValuePair() { Name = "IsULLinked", Value = "1" }, // Set the user is using UnifiedLogin
						new NameValuePair() { Name = "EmailAddress", Value = userEmailAddress.Contains("@bogusemail.com") ? string.Empty : userEmailAddress }
                    };

                    try
                    {
                        if (string.IsNullOrEmpty(_systemIdentifier))
                        {
                            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Running);
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userArray", userArray } }, messageProperties: new object[] { "ManageOneSiteUser", "Posting to create new user" });
                            response = _service.CreateUser(userArray.ToArray());
                            // add to product to the personaconfiguration
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", response } }, messageProperties: new object[] { "ManageOneSiteUser", "Got response from create new user" });
                            // add the pmcid to the saml attribute
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "Saving PMC id to new user" });
                            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.PMCID, _pmcID);
                            for (int i = 0; i < response.Length; i++)
                            {
                                // pull out the needed info
                                string key = response[i].Name.ToUpper();
                                switch (key) // SystemIdentifier
                                {
                                    case "SYSTEMIDENTIFIER":
                                        string pmcuserlogin = response[i].Value;
                                        WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userId", pmcuserlogin } }, messageProperties: new object[] { "ManageOneSiteUser", "Saving UserId id to new user" });
                                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, pmcuserlogin);
                                        WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUsername", pmcuserlogin.Split('|')[1] } }, messageProperties: new object[] { "ManageOneSiteUser", "Saving productUsername to new user" });
                                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, pmcuserlogin.Split('|')[1]);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "Posting to update regular user" });
                            response = _service.UpdateUser(_systemIdentifier, userArray.ToArray());
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", response } }, messageProperties: new object[] { "ManageOneSiteUser", "Got response from update regular user" });
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageOneSiteUser", $"Error encountered. Error :{ex.Message}" });
                        if (string.IsNullOrEmpty(_systemIdentifier))
                        {
                            UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                        }

                        return "Error : " + ex.Message;
                    }
                }
                else
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "isSuperUser = true" });
                    // build the call to OneSite to create the user
                    userArray = new List<NameValuePair>
                    {
                        new NameValuePair() { Name = "FirstName", Value = new string(person.FirstName.Where(Char.IsLetter).ToArray())},
                        new NameValuePair() { Name = "LastName", Value = new string(person.LastName.Where(Char.IsLetter).ToArray())},
                        new NameValuePair() { Name = "Pin", Value = onesitePin },
                        new NameValuePair() { Name = "ReferenceNumber", Value = userThirdPartyReference },
                        new NameValuePair() { Name = "PMCID", Value = _pmcID },
                        new NameValuePair() { Name = "IsSuperuser", Value = "1" },
                        new NameValuePair() { Name = "LogonName", Value = onesiteLoginName }, // leave empty login name so OneSite will create one
                        new NameValuePair() { Name = "IsULLinked", Value = "1" }, // Set the user is using UnifiedLogin
                        new NameValuePair() { Name = "EmailAddress", Value = userEmailAddress }, // is the login name an email?
                        new NameValuePair() { Name = "Title", Value = "SuperUser" },
                    };
                    try
                    {
                        if (string.IsNullOrEmpty(_systemIdentifier))
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userArray", userArray } }, messageProperties: new object[] { "ManageOneSiteUser", "Posting to create new super user" });
                            response = _service.CreateSuperuser(userArray.ToArray());
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", response } }, messageProperties: new object[] { "ManageOneSiteUser", "Got response from create new super user" });
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "PMCID", _pmcID } }, messageProperties: new object[] { "ManageOneSiteUser", "Saving PMC id to new user" });
                            _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.PMCID, _pmcID);
                            for (int i = 0; i < response.Length; i++)
                            {
                                // pull out the needed info
                                string key = response[i].Name.ToUpper();
                                switch (key) // SystemIdentifier
                                {
                                    case "SYSTEMIDENTIFIER":
                                        string pmcuserlogin = response[i].Value;
                                        WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "UserId", pmcuserlogin } }, messageProperties: new object[] { "ManageOneSiteUser", "Saving UserId id to new user" });
                                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.UserId, pmcuserlogin);
                                        WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "productUsername", pmcuserlogin.Split('|')[1] } }, messageProperties: new object[] { "ManageOneSiteUser", "Saving productUsername to new user" });
                                        _samlRepository.CreateSamlUserAttribute(userPersonaId, _productId, SamlAttributeEnum.productUsername, pmcuserlogin.Split('|')[1]);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "userArray", userArray } }, messageProperties: new object[] { "ManageOneSiteUser", "Posting to update super user" });
                            response = _service.UpdateSuperuser(_systemIdentifier, userArray.ToArray());
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "response", response } }, messageProperties: new object[] { "ManageOneSiteUser", "Got response from update super user" });
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ManageOneSiteUser", $"Error encountered. Error: {ex.Message}" });
                        UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                        return "Error : " + ex.Message;
                    }
                }

                // now add the new OneSite account and info to the GreenBook user product list
                // TODO
                // save unique id to the persona for OneSite
                bool hasError = false;
                for (int i = 0; i < response.Length; i++)
                {
                    string key = response[i].Name.ToUpper();
                    switch (key)
                    {
                        case "ISSUCCESSFUL":
                            if (response[i].Value == "0")
                            {
                                hasError = true;
                            }
                            break;
                        case "SYSTEMIDENTIFIER":
                            _systemIdentifier = response[i].Value;
                            break;
                        case "ERRORMESSAGE":
                            errorMessage = response[i].Value;
                            break;
                    }
                }
                if (hasError)
                {
                    WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", $"Error encountered : {errorMessage}" });
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                    return errorMessage;
                }

                if (existingUser)
                {
                    // if the user already exists, make sure it is now active
                    EnableOneSiteUser(editorPersonaId, userPersonaId, true);
                }

                if (RoleList == null)
                {
                    RoleList = new List<string>();
                }
                if (PropertyList == null)
                {
                    PropertyList = new List<string>();
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "Setting product result to success" });
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);

                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "Beginning update to roles and properties" });
                if ((RoleList.Count > 0 || isSuperUser) && !isUserProfileChanged)
                {
                    UpdateRolesForUser(editorPersonaId, userPersonaId, RoleList, out var additionalParametersRoles);
                    additionalParameters.AddRange(additionalParametersRoles);
                }
                if (PropertyList.Count > 0 && !isUserProfileChanged)
                {
                    UpdatePropertiesForUser(editorPersonaId, userPersonaId, PropertyList, out var additionalParametersProperties);
                    additionalParameters.AddRange(additionalParametersProperties);
                }
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", "Finished update to roles and properties" });
            }
            else
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ManageOneSiteUser", $"Error : Missing party id for userPersonaId {userPersonaId}" });
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Error);
                return $"Error : Missing party id for userPersonaId {userPersonaId}";
            }
            return "";
        }

        /// <summary>
        /// Used to enable/disable a OneSite user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public string EnableOneSiteUser(long editorPersonaId, long userPersonaId, bool isActive)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "EnableOneSiteUser", $"Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive}" });
                if (isActive)
                {
                    _service.EnableUser(_systemIdentifier);
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Success);
                }
                else
                {
                    _service.DisableUser(_systemIdentifier);
                    UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Inactive);
                }
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "EnableOneSiteUser", $"Updating user status. userPersonaId = {userPersonaId}, isActive = {isActive}" });
            }
            return "";
        }

        /// <summary>
        /// Used to ResetVerificationCode of a OneSite user
        /// </summary>
        /// <returns></returns>
        public string ResetVerificationCode(long editorPersonaId, long userPersonaId)
        {
            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            Guid userRealPageId = userPersona.RealPageId;

            IC.Person person = _managePerson.GetPerson(userRealPageId);

            var userLogin = _manageUserLogin.GetUserLoginOnly(userRealPageId);

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ResetVerificationCode", "Resetting User Verification Code" });

                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);
                // the Accounting user making the change to the role, get the Company from the user
                _systemIdentifier = string.Empty;
                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    _systemIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
                }
                _service.ResetVerificationCode(_systemIdentifier);

                //Activity Log
                WriteResetVerificationCodeActivityLog(editorPersonaId, person, userLogin);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ResetVerificationCode", $"Resetting User Verification Code = {_systemIdentifier}" });
                return "There was a problem resetting verification code";
            }

            return "";
        }

        /// <summary>
        /// Used to delete a OneSite user
        /// </summary>
        /// <param name="editorPersonaId">Logged-in user PersonaId</param>
        /// <param name="userPersonaId">new user PersonaId</param>
        /// <returns>String.empty if success else error</returns>
        public string DeleteOneSiteUser(long editorPersonaId, long userPersonaId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return listResponse.ErrorReason; }

            IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(editorPersonaId, _productId);
            // the OneSite user deleting the user
            string uniqueIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
            //productAttributes = _samlRepository.GetProductSamlDetails(deletedPersona, _productId);
            //string deleteUserUniqueIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
            try
            {
                _service.DeleteUser(uniqueIdentifier, _systemIdentifier);
                // now set the status of the persona record to deleted, what do we do with it later?
                UpdateProductSettingProductStatus(userPersonaId, _productSettingType_ProductStatus, (int)ProductBatchStatusType.Deleted);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "DeleteOneSiteUser", $"Error deleting user. userPersonaId = {userPersonaId}. Error: {ex.Message}" });
                return "There was a problem deleting the user";
            }
            return "";
        }

        /// <summary>
        /// Check to see if the given user login is a leasing agent in OneSite
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="siteId">The site id to verify if the user is a leasing agent</param>
        /// <returns></returns>
        public bool UserInLeasingAgentList(long editorPersonaId, long userPersonaId, int siteId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, userPersonaId);
            if (listResponse.IsError) { return false; }
            bool userIsLeasingAgent = false;

            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "UserInLeasingAgentList", $"Getting if user is leasing agent in OneSite. userPersonaId = {userPersonaId}" });
                userIsLeasingAgent = _service.GetUserInLeasingAgentList(_systemIdentifier, siteId);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "UserInLeasingAgentList", $"Get leasing agent status failed. userPersonaId = {userPersonaId}. Error: {ex.Message}" });
            }
            return userIsLeasingAgent;
        }

        /// <summary>
        /// Used to get the Onesite PMC URL for the given OneSite user login
        /// </summary>
        /// <param name="userPersonaId">The persona id of the currently logged in user</param>
        /// <returns></returns>
        public PMCInfo GetPMCURL(long userPersonaId)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(userPersonaId, userPersonaId);
            if (listResponse.IsError) { return null; }

            int PMCID = 0;
            if (!string.IsNullOrEmpty(_systemIdentifier))
            {
                PMCID = Convert.ToInt32(_systemIdentifier.Split('|')[0]);
            }
            else
            {
                // otherwise, the user doesn't have a login for OneSite, so try to get the pmc from BlueBook
                Persona persona = new Persona() { PersonaId = _userPersona.PersonaId };
                PMCID = Convert.ToInt32(GetOneSitePMCIDFromPersona(persona));
            }
            return GetPMCInfo(PMCID);
        }

        /// <summary>
        /// Changes the user status.
        /// </summary>
        /// <param name="editorPersonaId">The editor persona identifier.</param>
        /// <param name="username">The username.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        /// <returns></returns>
        public bool ChangeUserStatus(long editorPersonaId, string username, bool isActive = false)
        {
            ListResponse listResponse = new ListResponse();
            listResponse = GetCompanyEditorAndUserDetails(editorPersonaId, 0);
            if (listResponse.IsError) { return false; }

            int companyInstanceSourceId = Convert.ToInt32(GetOneSitePMCIDFromPersona(_editorPersona));
            if (companyInstanceSourceId == 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                return false;
            }

            var systemIdentifier = $"{companyInstanceSourceId}|{username}";
            try
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "ChangeUserStatus", $"Updating user status for user = {systemIdentifier}, isActive = {isActive}" });

                if (isActive)
                    _service.EnableUser(systemIdentifier);
                else
                    _service.DisableUser(systemIdentifier);
            }
            catch (Exception ex)
            {
                WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "ChangeUserStatus", $"Updating user status failed for user {systemIdentifier} by editorPersonaId = {editorPersonaId}. Error: {ex.Message}" });
                return false;
            }

            return true;
        }

        #endregion

        #region Privates

        /// <summary>
        /// Get PMC Info by PMC Id
        /// </summary>
        /// <param name="pmcId"></param>
        /// <returns></returns>
        private PMCInfo GetPMCInfo(int pmcId)
        {
            var rpcache = new RPObjectCache();
            var pmcInfoCache = rpcache.GetFromCache($"onesitePMCInfo_{pmcId}", 600, () =>
            {
                try
                {
                    return _service.GetPMCUrl(pmcId);
                }
                catch
                {
                    return null;
                }
            });

            PMCInfo pmcInfo = null;
            if (pmcInfoCache == null) return null;
            pmcInfo = new PMCInfo();
            pmcInfo.ID = pmcInfoCache.ID;
            pmcInfo.PMCURL = pmcInfoCache.PMCURL.ToString();
            return pmcInfo;
        }

        /// <summary>
        /// Used to get information about the calling user and user being modified
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        private new ListResponse GetCompanyEditorAndUserDetails(long editorPersonaId, long userPersonaId)
        {
            ListResponse response = new ListResponse();

            response = verifyPersona(editorPersonaId);
            if (response.IsError)
            {
                return response;
            }
            else
            {
                // get the editors persona from the result
                _editorPersona = response.Records[0] as Persona;
            }

            if (userPersonaId != 0)
            {
                // verify the persona being changed belongs to the same company as the user making the changes
                _userPersona = _managePersona.GetPersona(userPersonaId);
                if (_userPersona == null || _userPersona.Organization.PartyId != _editorPersona.Organization.PartyId)
                {
                    response.IsError = true;
                    response.ErrorReason = "Invalid user persona";
                    return response;
                }
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);
                // the Accounting user making the change to the role, get the Company from the user
                _systemIdentifier = string.Empty;
                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    _systemIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
                }
                // get the current user product settings from GreenBook
                _userProductSettings = _productRepository.GetProductSettingsByPersona(userPersonaId);
            }
            return response;
        }

        /// <summary>
        /// Get the OneSite PMCID for the current user
        /// </summary>
        /// <param name="persona"></param>
        /// <returns></returns>
        private string GetOneSitePMCIDFromPersona(Persona persona)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", "Begin" });
            string pmcID = "";
            IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(persona.PersonaId, _productId);
            // the OneSite user making the change to the role, get the PMCID from the user
            string uniqueIdentifier = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
            if (uniqueIdentifier == null)
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", "Couldn't find unique identifier for user" });
                // see if the PMC has an override id
                RPObjectCache rpcache = new RPObjectCache();
                var cacheKey = "orgProductSettings_" + persona.Organization.RealPageId.ToString() + "_" + _productId.ToString();
                IList<ProductSettingList> orgProductSettingList = rpcache.GetFromCache<IList<ProductSettingList>>(cacheKey, 300, () =>
                {
                    // load from database
                    return _productRepository.GetProductSettings(persona.Organization.RealPageId, _productId);
                });
                if (orgProductSettingList.Any(p => p.Name.ToUpper() == "OVERRIDEPMCID"))
                {
                    ProductSettingList overridePMC = orgProductSettingList.First(p => p.Name.ToUpper() == "OVERRIDEPMCID");
                    pmcID = overridePMC.Value;
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", $"Found PMC ID override {pmcID}" });
                }
                else
                {
                    try
                    {
                        // get the PMCID from BlueBook because the user doesn't have the PMCID for OneSite yet
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", "Getting info from BlueBook.GetCompanyMapResource" });
                        //IList<CompanyMap> companyMapResource = _blueBook.GetCompanyMap(persona.Organization.BooksMasterId, BlueBookProductConstants.OneSite);
                        IList<CustomerCompanyMap> companyMapResource = _blueBook.GetCompanyMap(persona.Organization.RealPageId, persona.Organization.BooksCustomerMasterId, source: BlueBookProductConstants.OneSite, domain: persona.Organization.OrganizationDomain.Name);
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", "Done getting info from BlueBook.GetCompanyMapResource" });
                        if (companyMapResource != null && companyMapResource.Count > 0 && companyMapResource.Any(a => a.Source.ToUpper() == BlueBookProductConstants.OneSite))
                        {
                            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", "Getting PMC ID from BlueBook result" });
                            pmcID = companyMapResource.First(a => a.Source.ToUpper() == BlueBookProductConstants.OneSite).CompanyInstanceSourceId;
                            WriteToDiagnosticLog("{ActionName} - {state}", logData: new Dictionary<string, object>() { { "pmcID", pmcID } }, messageProperties: new object[] { "GetOneSitePMCIDFromPersona", "Found PMC ID from BlueBook result" });
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteToErrorLog("{ActionName} - {state}", exception: ex, messageProperties: new object[] { "GetOneSitePMCIDFromPersona", $"Error for user with persona id - {persona.PersonaId}. Error: {ex.Message}" });
                        return string.Empty;
                    }
                }
            }
            else
            {
                pmcID = uniqueIdentifier.Split('|')[0];
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", $"Use PMCID from user login {pmcID}" });
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetOneSitePMCIDFromPersona", $"Done. pmcID: {pmcID}" });
            return pmcID;
        }

        /// <summary>
        /// Used to parse the error message from a SOAP web service exception
        /// </summary>
        /// <param name="soapErrorException"></param>
        /// <returns></returns>
        private string ParseSoapErrorMessage(string soapErrorException)
        {
            return soapErrorException.Replace("Server was unable to process request. ---> ", "");
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
                else
                {
                    //if (!(response.StatusCode == System.Net.HttpStatusCode.Unauthorized))
                    WriteToErrorLog("{ActionName} - {state}", new Dictionary<string, object>() { { "baseUrlAndQuery", baseUrlAndQuery }, { "error", response.Content.ReadAsStringAsync().Result }, { "status", response.StatusCode } }, messageProperties: new object[] { "GetResultFromApi", "Exiting after error" });
                }
            }

            return results;
        }

        private string GetTokenByPMC(PMCInfo pmcInfo)
        {
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetTokenByPMC", "Beginning" });

            var rpcache = new RPObjectCache();

            // Get token values from cache
            return rpcache.GetFromCache($"mt_access_token_{pmcInfo.ID}", 600, () =>
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetTokenByPMC", $"Getting token from Issue URI https://{pmcInfo.PMCURL}/{_mtTokenEndPoint}" });

                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _mtClientId),
                    new KeyValuePair<string, string>("client_secret", _mtClientSecret)
                };
                string result = null;

                using (var client = new HttpClient(_messageHandler, false))
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new FormUrlEncodedContent(postData);
                    var response = client.PostAsync($"https://{pmcInfo.PMCURL}/{_mtTokenEndPoint}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetTokenByPMC", $"Got token from Issue URI https://{pmcInfo.PMCURL}/{_mtTokenEndPoint}" });
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        dynamic userResult = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                        if (userResult != null)
                        {
                            result = userResult.access_token.ToString();
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

                return result;
            });
        }
        #endregion

        #region Migration
        /// <summary>
        /// Get List of One Site Users for Migration 
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

            int companyInstanceSourceId = Convert.ToInt32(GetOneSitePMCIDFromPersona(_editorPersona));
            if (companyInstanceSourceId == 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                response.ErrorReason = "Company Setup Error: Please Contact Support.";
                return response;
            }
            var filter = "NonMigrated";
            var startRow = 0;
            var resultPerPage = 1000;
            if (datafilter != null)
            {
                if (datafilter.FilterBy.ContainsKey("filter"))
                {
                    filter = datafilter.FilterBy["filter"];
                }
                if (datafilter.Pages != null)
                {
                    startRow = datafilter.Pages.StartRow;
                    resultPerPage = datafilter.Pages.ResultsPerPage;
                }
            }

            var pmcInfo = GetPMCInfo(companyInstanceSourceId);
            if (pmcInfo == null || pmcInfo.ID != companyInstanceSourceId)
            {
                response.ErrorReason = $"Could not get PMC Info for company Instance Source id - {companyInstanceSourceId}.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"GetPMCInfo Error: {response.ErrorReason}" });
                return response;
            }

            var url = $"https://{pmcInfo.PMCURL}/{_mtApiEndPoint}/{companyInstanceSourceId}/users?filter={filter}&startRow={startRow}&resultsPerPage={resultPerPage}";
            var _mtAccessToken = GetTokenByPMC(pmcInfo);
            if (string.IsNullOrWhiteSpace(_mtAccessToken))
            {
                response.ErrorReason = $"Could not get access token from PMC for company Instance Source id - {companyInstanceSourceId}.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Error: {response.ErrorReason}" });
                return response;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", new Dictionary<string, object> { { "Url", url } }, messageProperties: new object[] { "GetMigrationUsers", "Get users" });

            var allUsers = GetResultFromApi<IList<OneSiteMigrateUser>>(_mtAccessToken, url);

            if (allUsers == null)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"No users received from product for user with editorPersona id - {editorPersonaId}" });
                return response;
            }
            foreach (var user in allUsers)
            {
                user.CompanyInstanceSourceId = companyInstanceSourceId.ToString();
                user.EmployeeId = user.ReferenceNumber;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "GetMigrationUsers", $"Received users from product for user with editorPersona id - {editorPersonaId}" });
            response.RowsPerPage = resultPerPage;
            response.ErrorReason = string.Empty;
            response.IsError = false;
            response.TotalPages = 1;
            response.Records = allUsers.Cast<object>().ToList();
            response.TotalRows = allUsers.Count();
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

            int companyInstanceSourceId = Convert.ToInt32(GetOneSitePMCIDFromPersona(_editorPersona));
            if (companyInstanceSourceId == 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error looking for company id in bluebook for user with editorPersona id - {editorPersonaId}" });
                migrateResponse.Message = "Company Setup Error: Please Contact Support.";
                return migrateResponse;
            }
            var pmcInfo = GetPMCInfo(companyInstanceSourceId);
            if (pmcInfo == null || pmcInfo.ID != companyInstanceSourceId)
            {
                migrateResponse.Message = $"Could not get PMC Info for company Instance Source id - {companyInstanceSourceId}.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error: {migrateResponse.Message}" });
                return migrateResponse;
            }
            var url = $"https://{pmcInfo.PMCURL}/{_mtApiEndPoint}/{companyInstanceSourceId}/migrate-users";
            var _mtAccessToken = GetTokenByPMC(pmcInfo);
            if (string.IsNullOrWhiteSpace(_mtAccessToken))
            {
                migrateResponse.Message = $"Could not get access token from PMC for company Instance Source id - {companyInstanceSourceId}.";
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "UpdateUsersMigrationStatus", $"Error: {migrateResponse.Message}" });
                return migrateResponse;
            }
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _mtAccessToken);
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
                WriteToDiagnosticLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync success" });
                migrateResponse.Message = "Success";
                migrateResponse.Status = true;
                return migrateResponse;
            }

            WriteToErrorLog("{ActionName} - {state}", logData: logData, messageProperties: new object[] { "UpdateUsersMigrationStatus", "PostAsJsonAsync error" });
            migrateResponse.Message = "Cannot update user status to migrated.";
            return migrateResponse;
        }
        #endregion

    }

    /// <summary>
    /// Used to build the XML required to call the OneSite web services
    /// </summary>
    public static class OneSiteHelpers
    {
        /// <summary>
        /// Used to convert a OneSite property into a GreenBook role to be used by the UI
        /// </summary>
        /// <param name="properties">The list of properties to convert</param>
        /// <returns></returns>
        public static IList<ProductProperty> ToGBProperties(this PropertyList properties)
        {
            if (properties == null || properties.Property == null)
                return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (PropertyType loc in properties.Property)
            {
                results.Add(new ProductProperty
                {
                    Name = loc.PropertyName,
                    ID = loc.PropertyID,
                    Street1 = loc.SiteAddress,
                    Street2 = "",
                    City = loc.SiteCityName,
                    State = loc.SiteState,
                    Zip = loc.SiteZip,
                    IsAssigned = loc.IsAssignedToUser
                });
            }
            return results;
        }

        /// <summary>
        /// Used to convert a OneSite role into a GreenBook role to be used by the UI
        /// </summary>
        /// <param name="roles">The list of roles to convert</param>
        /// <returns></returns>
        public static IList<ProductRole> ToGBRoles(this RoleList roles)
        {
            if (roles == null || roles.Role == null)
                return null;
            IList<ProductRole> results = new List<ProductRole>();
            foreach (RoleType role in roles.Role)
            {
                if (IsValidRoleForCustomer(role))
                {
                    results.Add(new ProductRole
                    {
                        Name = role.RoleName,
                        ID = role.RoleID,
                        RightsAssigned = role.RightsAssigned,
                        IsAssigned = role.IsAssigned,
                        Roletype = role.Roletype

                    });
                }
            }
            return results;
        }

        /// <summary>
        /// Used to convert a OneSite right into a GreenBook right to be used by the UI
        /// </summary>
        /// <param name="rights">The list of rights to convert</param>
        /// <returns></returns>
        public static IList<ProductRight> ToGBRights(this RightList rights)
        {
            if (rights == null || rights.Right == null)
                return null;
            IList<ProductRight> results = new List<ProductRight>();
            foreach (RightType right in rights.Right)
            {
                results.Add(new ProductRight
                {
                    ID = Convert.ToInt32(right.RightID),
                    Description = right.RightDescription,
                    Assigned = right.Assigned,
                    CenterName = right.CenterName,
                    RolesAssigned = right.RolesAssigned,
                    UsageDescription = right.UsageDescription
                });
            }
            return results;
        }

        /// <summary>
        /// Used to convert a OneSite user into a GreenBook user to be used by the UI
        /// </summary>
        /// <param name="users">The list of users to convert</param>
        /// <returns></returns>
        public static IList<Component.SharedObjects.Product.ProductUser> ToGBUsers(this UserList users)
        {
            if (users == null || users.User == null)
                return null;
            IList<Component.SharedObjects.Product.ProductUser> results = new List<Component.SharedObjects.Product.ProductUser>();
            foreach (UserType user in users.User)
            {
                results.Add(new Component.SharedObjects.Product.ProductUser
                {
                    UserId = user.UserId,
                    UserLogin = user.UserLogin,
                    UserName = user.UserName,
                    IsAssigned = user.Assigned
                });
            }
            return results;
        }

        /// <summary>
        /// Used to filter out roles that should not be granted from the UI
        /// </summary>
        /// <param name="role">A role to verify if it is usable by customers</param>
        /// <param name="internalUser">Is user being updated a RealPage user</param>
        /// <returns></returns>
        public static bool IsValidRoleForCustomer(RoleType role, bool internalUser = false)
        {
            if (internalUser) return true;

            if (role.Roletype.ToUpper() == "DEFAULT")
            {
                // RoleID 14
                if (role.RoleName.ToUpper() == "DEVELOPER")
                {
                    return false;
                }
                // RoleID 15
                if (role.RoleName.ToUpper() == "INTERNAL ADMINISTRATOR")
                {
                    return false;
                }

                if (role.RoleName.ToUpper() == "INTERNAL USER")
                {
                    return false;
                }
                if (role.RoleName.ToUpper() == "CLOSE CRIMINAL DISPUTE")
                {
                    return false;
                }
            }

            if (role.RoleName.ToUpper() == "SCREENING INTERFACE ACCESS")
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Used to build the filter/sorting object to use when posting to OneSite
        /// </summary>
        /// <param name="datafilter"></param>
        /// <param name="defaultFieldToSort"></param>
        /// <param name="start"></param>
        /// <param name="pageLength"></param>
        /// <param name="excludeAssigned"></param>
        /// <returns></returns>
        public static FilterSortParameters GenerateSearchAndPaging(RequestParameter datafilter, string defaultFieldToSort, int start, int pageLength, bool excludeAssigned = false)
        {
            //handle pagination
            FilterSortParameters wsParams = new FilterSortParameters { StartPosition = start, PageLength = pageLength };

            // nothing to filter
            if (datafilter == null) { return wsParams; }

            //handle sorting
            if (datafilter.SortBy != null && datafilter.SortBy.Count > 0)
            {
                wsParams.SortCondition = new SortCondition();

                foreach (KeyValuePair<string, string> kp in datafilter.SortBy)
                {
                    wsParams.SortCondition.ColumnName = kp.Key;
                    wsParams.SortCondition.SortDirection = kp.Value;
                }
            }

            List<FilterCondition> filterObjs = new List<FilterCondition>();
            if (excludeAssigned)
            {
                filterObjs.Add(new FilterCondition
                {
                    PropertyName = "excludeassigned",
                    Operator = "equalto",
                    SearchValue = "1"
                });
            }
            //handle search
            if (datafilter.FilterBy != null && datafilter.FilterBy.Count > 0)
            {
                foreach (KeyValuePair<string, string> kp in datafilter.FilterBy)
                {
                    filterObjs.Add(new FilterCondition { PropertyName = kp.Key, SearchValue = kp.Value, Operator = "startswith", QuerySuffix = " AND " });
                }
            }

            // handle paging and row results
            if (datafilter.Pages.ResultsPerPage > 0)
            {
                wsParams.PageLength = datafilter.Pages.ResultsPerPage;
            }
            if (datafilter.Pages.StartRow >= 0)
            {
                wsParams.StartPosition = datafilter.Pages.StartRow;
            }

            wsParams.FilterConditionList = filterObjs.Count == 0 ? null : filterObjs.ToArray();

            return wsParams;
        }
    }
}