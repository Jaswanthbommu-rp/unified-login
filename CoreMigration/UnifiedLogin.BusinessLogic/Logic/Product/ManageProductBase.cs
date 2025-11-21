using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.Caching;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Saml;
using Serilog;
using Serilog.Events;
using IC = UnifiedLogin.SharedObjects.IdentityConfig;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.BusinessLogic.Logic.Helper;

namespace UnifiedLogin.BusinessLogic.Logic.Product
{
    /// <summary>
    /// Base for Products
    /// </summary>
    public class ManageProductBase : IDisposable
    {
        const int MAXRETRYCOUNT = 5;

        /// <summary>
        /// Used to store the product id
        /// </summary>
        protected int _productId;

        /// <summary>
        /// product Url
        /// </summary>
        protected string _productUrl;

        /// <summary>
        /// Logged in enterprise User Guid
        /// </summary>
        protected Guid _editorRealPageId;

        /// <summary>
        /// Logged in enterprise User Persona
        /// </summary>
        protected Persona _editorPersona;

        /// <summary>
        ///  Logged in enterprise User product username
        /// </summary>
        protected string _editorProductUsername = "";

        /// <summary>
        /// Logged in enterprise User product userId
        /// </summary>
        protected string _editorProductUserId = "";

        /// <summary>
        /// The Persona of the user being requested
        /// </summary>
        protected Persona _userPersona;

        /// <summary>
        /// Product username
        /// </summary>
        protected string _productUsername = "";

        /// <summary>
        /// Product user Id
        /// </summary>
        protected string _productUserId = "";

        /// <summary>
        /// Product learner Id
        /// </summary>
        protected string _productLearnerId = "";

        /// <summary>
        /// Product manager Id
        /// </summary>
        protected string _productManagerId = "";

        /// <summary>
        /// Productudm source code
        /// </summary>
        protected string _udmSourceCode = "";

        // Managers
        /// <summary>
        /// Manage Persoan Business logic
        /// </summary>
        protected IManagePersona _managePersona;// = new ManagePersona();

        /// <summary>
        /// Manage BlueBook Business logic
        /// </summary>
        protected IManageBlueBook _blueBook;

        /// <summary>
        /// Manage Person Business logic
        /// </summary>
        protected IManagePerson _managePerson;// = new ManagePerson();

        /// <summary>
        /// Manage UserLogin Business logic
        /// </summary>
        protected IManageUserLogin _manageUserLogin;// = new ManageUserLogin();

        /// <summary>
        /// Manage Organization Business logic
        /// </summary>
        //protected IManageOrganization _manageOrganization = new ManageOrganization();

        /// <summary>
        /// Manage Electronic Address Business logic
        /// </summary>
        protected IManageElectronicAddress _manageElectronicAddress;// = new ManageElectronicAddress();

        /// <summary>
        /// Manage Party Relationship Business logic
        /// </summary>
        protected IManagePartyRelationship _managePartyRelationship;// = new ManagePartyRelationship();

        // Repositories
        /// <summary>
        /// Product InternalSetting Repository
        /// </summary>
        protected IProductInternalSettingRepository _productInternalSettingRepository;// = new ProductInternalSettingRepository();

        /// <summary>
        /// Saml Repository
        /// </summary>
        protected ISamlRepository _samlRepository;// = new SamlRepository();

        /// <summary>
        /// Product Repository
        /// </summary>
        protected IProductRepository _productRepository;// = new ProductRepository();

        /// <summary>
        /// Persona Repository
        /// </summary>
        protected IPersonaRepository _personaRepository;// = new PersonaRepository();

        /// <summary>
        /// Product Repository
        /// </summary>
        protected IPropertyRepository _propertyRepository;// = new PropertyRepository();

        /// <summary>
        /// User login repository
        /// </summary>
        protected IUserLoginRepository _userLoginRepository;// = new UserLoginRepository();

        /// <summary>
        /// User login persona repository
        /// </summary>
        protected IUserLoginPersonaRepository _userLoginPersonaRepository;// = new UserLoginPersonaRepository();

        /// <summary>
        /// User repository
        /// </summary>
        protected IUserRepository _userRepository;// = new UserRepository();

        /// <summary>
        /// List of Product InternalSetting
        /// </summary>
        protected IList<IC.ProductInternalSetting> _productInternalSettingList = new List<IC.ProductInternalSetting>();

        /// <summary>
        /// UserRole Repository
        /// </summary>
        protected IUserRoleRightRepository _userRoleRightRepository;// = new UserRoleRightRepository();

        // Services
        /// <summary>
        /// HttpClient
        /// </summary>
        protected HttpClient _client;

        /// <summary>
        /// Http Message Handler
        /// </summary>
        protected HttpMessageHandler _messageHandler;// = new HttpClientHandler();

        // Constants
        /// <summary>
        /// Product Status Constant
        /// </summary>
        public const string _productSettingType_ProductStatus = "ProductStatus";

        /// <summary>
        /// All Properties Constant
        /// </summary>
        protected const string _productSettingType_AllProperties = "AllProperties";

        /// <summary>
        /// Used for activity logging
        /// </summary>
        private string _correlationId;

        /// <summary>
        /// User claim
        /// </summary>
        private DefaultUserClaim _userClaim;

        /// <summary>
        /// Unified Login Repository
        /// </summary>
        protected IUnifiedLoginRepository _unifiedLoginRepository;// = new UnifiedLoginRepository();

        /// <summary>
        /// Contact Mechanism Manager
        /// </summary>
        protected IManageContactMechanism _manageContactMechanism;// = new ManageContactMechanism();

        protected GbProductMap _productDetails = new GbProductMap();

        public static readonly Guid _contractCompanyRealPageId = new Guid("10F5A427-4636-4F47-840E-6212BD842BC0");
        public static readonly Guid _employeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");

        public const string PRODUCT_ROLES_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"RoleName\"}";
        public const string PRODUCT_ROLES_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"RoleName\"}";
        public const string PRODUCT_PROPERTIES_ASSIGN_MESSAGE = "{\"action\":\"Assigned\",\"value\":\"PropertyName\"}";
        public const string PRODUCT_PROPERTIES_REMOVED_MESSAGE = "{\"action\":\"Removed\",\"value\":\"PropertyName\"}";

        private IRepository _repository;

        /// <summary>
        /// Default constructor with correlationId
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="userClaim"></param>
        /// <param name="productInternalSettingRepository"></param>
        /// <param name="productRepository"></param>
        public ManageProductBase(int productId, DefaultUserClaim userClaim, IProductInternalSettingRepository productInternalSettingRepository, IProductRepository productRepository)
        {
            _productId = productId;
            _userClaim = userClaim;
            _correlationId = _userClaim.CorrelationId.ToString();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            if (productInternalSettingRepository != null) { _productInternalSettingRepository = productInternalSettingRepository; }
            if (productRepository != null) { _productRepository = productRepository; }
            _productInternalSettingList = GetProductSetting(_productId);
            _productDetails = GetBooksMasterProductDetail(_productId, false);
            if (_productDetails != null)
            {
                _udmSourceCode = _productDetails.UDMSourceCode?.Length > 0 ? _productDetails.UDMSourceCode : _productDetails.BooksProductCode;
            }

            _blueBook = new ManageBlueBook(userClaim);
            _managePersona = new ManagePersona(userClaim);
            _managePerson = new ManagePerson();
            _manageUserLogin = new ManageUserLogin(userClaim);
            _manageElectronicAddress = new ManageElectronicAddress();
            _managePartyRelationship = new ManagePartyRelationship();
            _personaRepository = new PersonaRepository();
            _propertyRepository = new PropertyRepository();
            _userLoginRepository = new UserLoginRepository();
            _userLoginPersonaRepository = new UserLoginPersonaRepository();
            _userRepository = new UserRepository(userClaim);
            _userRoleRightRepository = new UserRoleRightRepository();
            _client = new HttpClient();
            _messageHandler = new HttpClientHandler();
            _unifiedLoginRepository = new UnifiedLoginRepository();
            _manageContactMechanism = new ManageContactMechanism();
            _samlRepository = new SamlRepository();
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="userClaim"></param>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        protected ManageProductBase(int productId, DefaultUserClaim userClaim, IRepository repository, HttpMessageHandler messageHandler)
        {
            _repository = repository;
            _productId = productId;
            _userClaim = userClaim;
            _correlationId = _userClaim.CorrelationId.ToString();
            new RPObjectCache().BustCache();
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _productRepository = new ProductRepository(repository, userClaim);
            _productInternalSettingList = GetProductSetting(_productId, true);
            _productDetails = GetBooksMasterProductDetail(_productId, true);
            _udmSourceCode = _productDetails.UDMSourceCode?.Length > 0 ? _productDetails.UDMSourceCode : _productDetails.BooksProductCode;

            _blueBook = new ManageBlueBook(userClaim, repository, messageHandler);
            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _managePerson = new ManagePerson(repository);
            _manageUserLogin = new ManageUserLogin(repository, userClaim, messageHandler);
            _manageElectronicAddress = new ManageElectronicAddress();
            _managePartyRelationship = new ManagePartyRelationship(repository);
            _personaRepository = new PersonaRepository(repository, userClaim);
            _propertyRepository = new PropertyRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _userLoginPersonaRepository = new UserLoginPersonaRepository(repository);
            _userRepository = new UserRepository(repository, userClaim, messageHandler);
            _userRoleRightRepository = new UserRoleRightRepository(repository, userClaim);
            _client = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };
            _messageHandler = messageHandler;
            _unifiedLoginRepository = new UnifiedLoginRepository(repository);
            _manageContactMechanism = new ManageContactMechanism(repository);
            _samlRepository = new SamlRepository(repository);
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="userClaim"></param>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="httpClient"></param>
        protected ManageProductBase(int productId, DefaultUserClaim userClaim, IRepository repository, HttpMessageHandler messageHandler, HttpClient httpClient)
        {
            _repository = repository;
            _productId = productId;
            _userClaim = userClaim;
            _correlationId = _userClaim.CorrelationId.ToString();
            new RPObjectCache().BustCache();
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _productRepository = new ProductRepository(repository, userClaim);
            _productInternalSettingList = GetProductSetting(_productId, true);
            _productDetails = GetBooksMasterProductDetail(_productId, true);
            _udmSourceCode = _productDetails.UDMSourceCode?.Length > 0 ? _productDetails.UDMSourceCode : _productDetails.BooksProductCode;

            _blueBook = new ManageBlueBook(userClaim, repository, messageHandler);
            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _managePerson = new ManagePerson(repository);
            _manageUserLogin = new ManageUserLogin(repository, userClaim, messageHandler);
            _manageElectronicAddress = new ManageElectronicAddress();
            _managePartyRelationship = new ManagePartyRelationship(repository);
            _personaRepository = new PersonaRepository(repository, userClaim);
            _propertyRepository = new PropertyRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _userLoginPersonaRepository = new UserLoginPersonaRepository(repository);
            _userRepository = new UserRepository(repository, userClaim, messageHandler);
            _userRoleRightRepository = new UserRoleRightRepository(repository, userClaim);
            _client = httpClient;
            _messageHandler = messageHandler;
            _unifiedLoginRepository = new UnifiedLoginRepository(repository);
            _manageContactMechanism = new ManageContactMechanism(repository);
            _samlRepository = new SamlRepository(repository);
        }

        /// <summary>
        /// Get Product Setting
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <param name="noCache">Do not cache result. for unit tests.</param>
        /// <returns>List of Product Internal Settings</returns>
        public List<IC.ProductInternalSetting> GetProductSetting(int productId, bool noCache = false)
        {
            if (_repository != null || noCache)
            {
                // unit test
                return _productInternalSettingRepository.GetProductInternalSettings(productId);
            }

            var rpcache = new RPObjectCache();
            var cacheKey = $"productInternalSetting_{productId}";
            return rpcache.GetFromCache(cacheKey, 120, () => _productInternalSettingRepository.GetProductInternalSettings(productId));
        }

        /// <summary>
        /// Get User deactivated Product batch data
        /// </summary>
        /// <param name="userPersonaId">User PersonaId</param>
        /// <returns> User deactivated Product batch data</returns>
        public RolePropertyList GetDeactivatedProductBatchData(long userPersonaId)
        {
            IList<ProductSettingList> _userProductSettings = new List<ProductSettingList>();
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = "deactivatedproductsettingsdata_" + userPersonaId.ToString();

            RolePropertyList roleproperty = new RolePropertyList();

            _userProductSettings = rpcache.GetFromCache<IList<ProductSettingList>>(cacheKey, 600, () =>
            {
                // get the current user product settings from GreenBook
                return _productRepository.GetProductSettingsByPersona(userPersonaId);

            });

            bool isDeactivated = _userProductSettings.Any(s => s.ProductId == _productId && s.Value == Convert.ToString((int)UserUiStatusType.Deactivated));
            if (isDeactivated)
            {
                roleproperty = _productRepository.GetUserProductDataFromProductBatch(userPersonaId, _productId);
            }

            return roleproperty;
        }
        /// <summary>
        /// Used to get information about the calling user and user being modified
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        protected ListResponse GetCompanyEditorAndUserDetails(long editorPersonaId, long userPersonaId)
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
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(editorPersonaId, _productId);
                // the  user making the change, get the Company from the user
                if (productAttributes.Any(a => a.Name.ToUpper() == "PRODUCTUSERNAME"))
                {
                    _editorProductUsername = (from a in productAttributes where a.Name.ToUpper() == "PRODUCTUSERNAME" select a.Value).FirstOrDefault();
                }
                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    _editorProductUserId = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
                }
                //_editorPersona.Organization.BooksMasterId = _manageOrganization.GetBooksMasterId(_editorPersona.Organization.RealPageId);
            }

            if (userPersonaId != 0)
            {
                // verify the persona being changed belongs to the same company as the user making the changes
                Persona user = _managePersona.GetPersona(userPersonaId);
                if (user == null || user.Organization.PartyId != _editorPersona.Organization.PartyId)
                {
                    response.IsError = true;
                    response.ErrorReason = "Invalid user persona";
                    return response;
                }
                IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(userPersonaId, _productId);
                // the Accounting user making the change to the role, get the Company from the user
                if (productAttributes.Any(a => a.Name.ToUpper() == "PRODUCTUSERNAME"))
                {
                    _productUsername = (from a in productAttributes where a.Name.ToUpper() == "PRODUCTUSERNAME" select a.Value).FirstOrDefault();
                }
                if (productAttributes.Any(a => a.Name.ToUpper() == "USERID"))
                {
                    _productUserId = (from a in productAttributes where a.Name.ToUpper() == "USERID" select a.Value).FirstOrDefault();
                }
                if (productAttributes.Any(a => a.Name.ToUpper() == "LEARNERID"))
                {
                    _productLearnerId = (from a in productAttributes where a.Name.ToUpper() == "LEARNERID" select a.Value).FirstOrDefault();
                }
                if (productAttributes.Any(a => a.Name.ToUpper() == "MANAGERID"))
                {
                    _productManagerId = (from a in productAttributes where a.Name.ToUpper() == "MANAGERID" select a.Value).FirstOrDefault();
                }
            }
            response = DoAdditional(response);
            return response;
        }

        /// <summary>
        /// Used to delete all SAML product information and status for a user
        /// </summary>
        /// <param name="personaId">The persona of the person to delete all of the product SAML information and status for</param>
        /// <param name="productId">The product id to delete</param>
        public void DeleteSamlUserProductInfoAndStatus(long personaId, int productId)
        {
            var result =_samlRepository.DeleteSamlUserProductInfoAndStatus(personaId, productId);
            if(result.Id > 0)
            {
                _samlRepository.DeletePersonaProductError(personaId);
            }
        }

        /// <summary>
        /// Used to create a new saml user attribute
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="attributeType"></param>
        /// <param name="newValue"></param>
        private void CreateSamlUserAttribute(long personaId, SamlAttributeEnum attributeType, string newValue)
        {
            CreateSamlUserAttribute(personaId, attributeType, newValue, _productId);
        }

        /// <summary>
        /// Used to create a new saml user attribute (Used for AO)
        /// </summary>
        private void CreateSamlUserAttribute(long personaId, SamlAttributeEnum attributeType, string newValue, int productId)
        {
            _samlRepository.CreateSamlUserAttribute(personaId, productId, attributeType, newValue);
        }

        /// <summary>
        /// Used to update an existing saml user attribute or add one if it does not exist
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="productId"></param>
        /// <param name="attributeType"></param>
        /// <param name="newValue"></param>
        public void UpdateSamlUserAttribute(long personaId, int productId, SamlAttributeEnum attributeType, string newValue)
        {
            Dictionary<SamlAttributeEnum, string> settingList = new Dictionary<SamlAttributeEnum, string>();
            settingList.Add(attributeType, newValue);

            UpdateSamlUserAttributes(personaId, settingList);
        }

        /// <summary>
        /// Used to add/update a list of product settings for the given product and persona
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="settingList"></param>
        public void UpdateSamlUserAttributes(long personaId, Dictionary<SamlAttributeEnum, string> settingList)
        {
            UpdateSamlUserAttributes(personaId, settingList, _productId);
        }
        /// <summary>
        /// Update user Employee Id
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="employeeId"></param>
        public void UpdateUserEmployeeId(long personaId, string employeeId)
        {
            Persona userPersona = _managePersona.GetPersona(personaId);
            var userLogin = _manageUserLogin.GetUserLoginOnly(userPersona.RealPageId);
            IList<IC.UserLoginPersona> userLoginPersonaList = _userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userPersona.UserId, organizationPartyId: userPersona.Organization.PartyId);
            var employeeIdDetails = _userRepository.GetUserEmployeeId(userLoginPersonaList[0].UserLoginPersonaId, userPersona.OrganizationPartyId);

            //Update User Employee Id
            if (string.IsNullOrEmpty(employeeIdDetails.EmployeeId))
            {
                employeeIdDetails.EmployeeId = employeeId;
                _userRepository.UpdateUserEmployeeId(employeeIdDetails);
            }
        }

        public string GetSupervisorUserDetails(long personaId)
        {
            string supervisorId = string.Empty;
            Persona subjectuserPersona = _managePersona.GetPersona(personaId);
            var supervisorInfo = _userRepository.GetSuperVisorInformation(subjectuserPersona.UserId, subjectuserPersona.OrganizationPartyId);
            if (supervisorInfo != null && supervisorInfo.SuperVisorUserId > 0)
            {
                var userLoginInfo = _userLoginRepository.GetUserLoginOnly(supervisorInfo.SuperVisorUserId);

                if (userLoginInfo != null)
                {
                    IList<Persona> personaList = _managePersona.ListActivePersona(userLoginInfo.RealPageId, false);
                    if (personaList.Count > 0)
                    {
                        long supervisorPersonaId = personaList.Where(a => a.Organization.PartyId == subjectuserPersona.OrganizationPartyId).Select(a => a.PersonaId).FirstOrDefault();
                        IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(supervisorPersonaId, _productId);
                        if (productAttributes != null && productAttributes.Count > 0)
                        {
                            supervisorId = productAttributes.Where(a => a.SamlAttributeId == (int)SamlAttributeEnum.productUsername).Select(a => a.Value).FirstOrDefault();
                        }
                    }
                }
            }
            return supervisorId;
        }

        /// <summary>
        /// Used to add/update a list of product settings for the given product and persona
        /// </summary> 
        public void UpdateSamlUserAttributes(long personaId, Dictionary<SamlAttributeEnum, string> settingList, int productId)
        {
            IList<SamlAttributes> productAttributes = _samlRepository.GetProductSamlDetails(personaId, productId);
            if (settingList.Count > 0)
            {
                foreach (KeyValuePair<SamlAttributeEnum, string> setting in settingList)
                {
                    if (productAttributes.Any(a => a.SamlAttributeId == (int)setting.Key))
                    {
                        SamlAttributes attributeToReplace = (from a in productAttributes where a.SamlAttributeId == (int)setting.Key select a).FirstOrDefault();
                        if (attributeToReplace != null)
                        {
                            attributeToReplace.Value = setting.Value;
                            _samlRepository.UpdateSamlUserAttribute(attributeToReplace);
                        }
                    }
                    else
                    {
                        CreateSamlUserAttribute(personaId, setting.Key, setting.Value, productId);
                    }
                }
            }
        }

        /// <summary>
        /// Used to get the list of product roles assigned to the user
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="organizationPartyId">Optional Organization PartyId</param>  
        /// <returns>List of Roles</returns>
        public List<UL.Role> GetAssignedRoleForPersona(long userPersonaId, long? organizationPartyId = null)
        {
            var cacheKey = $"sp_ListRolesForProductsByPersonaId_{_productId}_{userPersonaId}_{organizationPartyId}";
            MemoryCache.Default.Remove(cacheKey);
            List<UL.Role> propRole = _userRoleRightRepository.ListRoleByPersona(_productId, userPersonaId, organizationPartyId);
            return propRole;
        }

        /// <summary>
        /// Used to determine if a user is a GB SuperUser
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        protected bool IsSuperUser(long userPersonaId)
        {
            Persona userPersona = _managePersona.GetPersona(userPersonaId);

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsSuperUser", $"IsSuperUser - Getting superuser status, userPersonaId={userPersonaId}" });
            IC.PartyRelationship partyRelationship = _managePartyRelationship.GetPartyRelationship(userPersona.RealPageId, userPersona.Organization.RealPageId, roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type");
            if (partyRelationship != null && partyRelationship.RoleTypeFrom.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase))
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsSuperUser", $"userPersonaId={userPersonaId} : true" });
                return true;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsSuperUser", $"userPersonaId={userPersonaId} : false" });
            return false;
        }

        /// <summary>
        /// Used to determine if a user is a GB Regular User (No Email)
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        protected bool IsRegularUserNoEmail(long userPersonaId)
        {
            Persona userPersona = _managePersona.GetPersona(userPersonaId);
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsRegularUserNoEmail", $"Getting superuser status, userPersonaId={userPersonaId}" });
            IC.PartyRelationship partyRelationship = _managePartyRelationship.GetPartyRelationship(userPersona.RealPageId, userPersona.Organization.RealPageId, roleTypeNameFrom: null, roleTypeNameTo: null, relationshipTypeName: "User Type");
            if (partyRelationship?.RoleTypeFrom.Name.ToUpper() == "USER (NO EMAIL)")
            {
                WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsRegularUserNoEmail", $"userPersonaId={userPersonaId} : true" });
                return true;
            }
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "IsRegularUserNoEmail", $"userPersonaId={userPersonaId} : false" });
            return false;
        }

        /// <summary>
        /// Get GB User Login by Persona Id
        /// </summary>
        protected Tuple<UserLoginOnly, Persona> GetUserLoginByPersonaId(long personaId)
        {
            var persona = _managePersona.GetPersona(personaId);
            return new Tuple<UserLoginOnly, Persona>(_manageUserLogin.GetUserLoginOnly(persona.RealPageId), persona);
        }

        /// <summary>
        /// Used to get the list of properties for the given user
        /// </summary>
        /// <param name="userPersonaId">The persona id of the user to get the list of properties for</param>
        /// <param name="productId">The product id</param>
        /// <returns></returns>
        protected List<ProductProperty> GetAssignedPropertyForPersona(long userPersonaId, int productId)
        {
            List<ProductProperty> propertyList = _propertyRepository.ListPropertiesByPersona(userPersonaId, productId);
            return propertyList;
        }

        /// <summary>
        /// Used to get the list of 
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productEnum"></param>
        /// <returns></returns>
        protected List<int> GetAssignedUPFMPropertyIdsForPersona(long userPersonaId, ProductEnum productEnum)
        {
            return _propertyRepository.ListUPFMPropertyInstanceIdByPersona(userPersonaId, productEnum);
        }

        /// <summary>
        /// Used to get the list of 
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        protected List<int> GetAssignedUPFMPropertyIdsForPersona(long userPersonaId, int productId)
        {
            return _propertyRepository.ListUPFMPropertyInstanceIdByPersona(userPersonaId, productId);
        }

        protected List<UPFMPropertyInstance> ListUPFMPropertyInstanceIdByInstanceIds(List<Guid> propertyInstanceIds)
        {
            return _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIds(propertyInstanceIds);
        }

        /// <summary>
        /// Used to add the given property id to the given user
        /// </summary>
        /// <param name="userPersonaId">The persona id of the user where the property will be added</param>
        /// <param name="productId">The product enum</param>
        /// <param name="propertyId">The property id to add</param>
        /// <returns></returns>
        protected RepositoryResponse InsertAssignedUserPropertyData(long userPersonaId, ProductEnum productId, long propertyId)
        {
            RepositoryResponse result = new RepositoryResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserPropertyData", $"Begin - calling DB to insert Property assigned to user userPersonaId - {userPersonaId}, PropertyId - {propertyId}." });
            result = _propertyRepository.InsertRemoveAssignedPropertyToUser(userPersonaId: userPersonaId, productId: productId, propertyId: propertyId, remove: 0);
            if (result.Id < 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserPropertyData", $"Unable to Insert record for user with userPersonaId - {userPersonaId}, PropertyId - {propertyId}" });
                return result;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserPropertyData", $"End - calling DB to insert Property assigned to user userPersonaId - {userPersonaId}, PropertyId - {propertyId}, result - {result.Id}." });
            return result;
        }

        /// <summary>
        /// Used to add the given property instance id to the given user
        /// </summary>
        /// <param name="userPersonaId">The persona id of the user where the property will be added</param>
        /// <param name="productId">The product enum</param>
        /// <param name="propertyInstanceId">The property instance id to add</param>
        /// <returns></returns>
        protected RepositoryResponse InsertAssignedUserPropertyInstanceData(long userPersonaId, int productId, long propertyInstanceId)
        {
            RepositoryResponse result = new RepositoryResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserPropertyInstanceData", $"START - calling DB to insert Property instance assigned to user userPersonaId - {userPersonaId}, PropertyId - {propertyInstanceId}." });
            result = _propertyRepository.InsertRemoveAssignedPropertyInstanceToUser(userPersonaId: userPersonaId, productId: productId, propertyInstanceId: propertyInstanceId, remove: 0);
            if (result.Id < 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserPropertyInstanceData", $"Unable to Insert record for user with userPersonaId - {userPersonaId}, PropertyInstanceId - {propertyInstanceId}" });
                return result;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "InsertAssignedUserPropertyInstanceData", $"END - calling DB to insert Property instance assigned to user userPersonaId - {userPersonaId}, PropertyInstanceId - {propertyInstanceId}, result - {result.Id}." });
            return result;
        }

        /// <summary>
        /// Used to remove the given property id from the given user
        /// </summary>
        /// <param name="userPersonaId">The persona id of the user where the property will be removed</param>
        /// <param name="productId">The product enum</param>
        /// <param name="propertyId">The property id to remove</param>
        /// <returns></returns>
        protected RepositoryResponse DeleteAssignedUserPropertyData(long userPersonaId, ProductEnum productId, long propertyId)
        {
            RepositoryResponse result = new RepositoryResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserPropertyData", $"START - calling DB to delete Property assigned to user userPersonaId - {userPersonaId}, PropertyId - {propertyId}." });

            result = _propertyRepository.InsertRemoveAssignedPropertyToUser(userPersonaId: userPersonaId, productId: productId, propertyId: propertyId, remove: 1);
            if (result.Id < 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserPropertyData", $"Unable to delete record for user with userPersonaId - {userPersonaId}, PropertyId - {propertyId}" });
                return result;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserPropertyData", $"END - calling DB to delete Property assigned to user userPersonaId - {userPersonaId}, PropertyId - {propertyId}." });
            return result;
        }

        /// <summary>
        /// Used to remove the given property instance id from the given user
        /// </summary>
        /// <param name="userPersonaId">The persona id of the user where the property will be removed</param>
        /// <param name="productId">The product enum</param>
        /// <param name="propertyInstanceId">The property instance id to remove</param>
        /// <returns></returns>
        protected RepositoryResponse DeleteAssignedUserPropertyInstanceData(long userPersonaId, int productId, long propertyInstanceId)
        {
            RepositoryResponse result = new RepositoryResponse();
            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserPropertyInstanceData", $"START - calling DB to delete Property instance assigned to user userPersonaId - {userPersonaId}, PropertyInstanceId - {propertyInstanceId}." });

            result = _propertyRepository.InsertRemoveAssignedPropertyInstanceToUser(userPersonaId: userPersonaId, productId: productId, propertyInstanceId: propertyInstanceId, remove: 1);
            if (result.Id < 0)
            {
                WriteToErrorLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserPropertyInstanceData", $"Unable to delete record for user with userPersonaId - {userPersonaId}, PropertyInstanceId - {propertyInstanceId}" });
                return result;
            }

            WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "DeleteAssignedUserPropertyInstanceData", $"END - calling DB to delete Property assigned to user userPersonaId - {userPersonaId}, PropertyInstanceId - {propertyInstanceId}." });
            return result;
        }

        /// <summary>
		/// Check User product right
		/// </summary>		
		/// <param name="productRightEnum">The product right id</param>
		/// <returns></returns>
		protected bool CheckUserProductRight(ProductRightEnum productRightEnum)
        {
            return _userClaim.Rights.Contains(productRightEnum.ToString());
        }

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            var ulProductInternalSettingList = GetProductSetting(3);
            string logSettings = null;
            if (ulProductInternalSettingList != null)
            {
                logSettings = ulProductInternalSettingList.FirstOrDefault(p => p.Name.Equals("Elk_LogManageProductBase", StringComparison.OrdinalIgnoreCase))?.Value;
            }

            if (logSettings != "1" && exception == null) return;

            var logger = Log.Logger;
            if (logData?.Keys != null)
            {
                //var json = JsonConvert.SerializeObject(logData, Formatting.Indented);
                logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
            }
            logger = logger.ForContext("ProductModule", this.GetType());
            logger = logger.ForContext("CorrelationId", _correlationId);
            
            logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
        }

        /// <summary>
        /// Used to write data to the information log for diagnostic
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        protected void WriteToInformationLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
            WriteToLog(LogEventLevel.Information, message, logData, messageProperties: messageProperties);
        }

        /// <summary>
        /// Used to write data to the error log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        /// <param name="exception"></param>
        protected void WriteToErrorLog(string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
            WriteToLog(LogEventLevel.Error, message, logData, exception, messageProperties: messageProperties);
        }

        /// <summary>
        /// Used to write data to the diagnostic log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logData"></param>
        /// <param name="messageProperties">Message properties</param>
        public void WriteToDiagnosticLog(string message, Dictionary<string, object> logData = null, object[] messageProperties = null)
        {
            WriteToLog(LogEventLevel.Debug, message, logData, messageProperties: messageProperties);
        }

        /// <summary>
        /// Do more stuff in the product manager if needed to set up the product
        /// </summary>
        public virtual ListResponse DoAdditional(ListResponse response)
        {
            return response;
        }

        /// <summary>
        /// Verify if the persona passed matches the current user context
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns></returns>
        protected ListResponse verifyPersona(long personaId)
        {
            ListResponse response = new ListResponse() { IsError = false };
            Persona editor = new Persona();
            Dictionary<string, object> logData = new Dictionary<string, object>();

            if (personaId == 0)
            {
                response = new ListResponse()
                {
                    TotalRows = 0,
                    RowsPerPage = 0,
                    TotalPages = 1,
                    ErrorReason = "Invalid persona",
                    IsError = true
                };
            }
            else
            {
                // verify the persona belongs to the current user
                editor = _managePersona.GetPersona(personaId);
                if (editor == null || editor.RealPageId != _editorRealPageId)
                {
                    WriteToDiagnosticLog("{ActionName} - {state}", messageProperties: new object[] { "VerifyPersona", $"Error getting persona. personaId={personaId}" });
                    // the passed persona doesn't belong to the caller, so fail
                    response = new ListResponse()
                    {
                        TotalRows = 0,
                        RowsPerPage = 0,
                        TotalPages = 1,
                        ErrorReason = "Invalid persona",
                        IsError = true
                    };
                }
                else
                {
                    IList<Persona> personaList = new List<Persona>() { editor };
                    // removing to see if this is the reason logging is stopping in early mornings
                    //logData = new Dictionary<string, object>();
                    //logData.Add("personaList", personaList);
                    //WriteToDiagnosticLog($"verifyPersona - Found persona. personaId={personaId}", logData);
                    response = new ListResponse()
                    {
                        Records = personaList.Cast<object>().ToList(),
                        TotalRows = 0,
                        RowsPerPage = 0,
                        TotalPages = 1,
                        ErrorReason = "",
                        IsError = false
                    };
                }
            }
            return response;
        }

        /// <summary>
        /// Used to update the persona product setting type for the given user and setting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userPersonaId"></param>
        /// <param name="settingType"></param>
        /// <param name="value"></param>
        public void UpdateProductSettingProductStatus<T>(long userPersonaId, string settingType, T value)
        {
            // add the new status flag to the product before we start
            IList<ProductSettingType> productSettingTypes = _productRepository.ListProductSettingType();
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            Dictionary<string, object> logData = new Dictionary<string, object>();

            string statusValue = value.ToString();
            logData.Add("Status Value", statusValue);
            if (Int32.Parse(statusValue) == (int)ProductBatchStatusType.Deleted || Int32.Parse(statusValue) == (int)ProductBatchStatusType.Inactive)
            {
                var persona = _managePersona.GetPersona(userPersonaId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(persona.RealPageId);
                OrganizationStatus orgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, false);

                int deactivatedStatus = (int)UserUiStatusType.Deactivated;
                logData.Add("User Current login", userLogin);
                logData.Add("orgStatus", orgStatus);
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateProductSettingProductStatus", $"User Current Status personaId={userPersonaId}" });

                if (string.Equals(orgStatus.Status.ToString(), UserUiStatusType.Disabled.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    statusValue = deactivatedStatus.ToString();
                }
            }

            // get the id for ProductStatus type
            if (productSettingTypes.Any(a => a.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase)))
            {
                int productStatusTypeId = (from a in productSettingTypes where a.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase) select a.ProductSettingTypeId).FirstOrDefault();
                repositoryResponse = _productRepository.CreateProductSetting(userPersonaId, _productId, productStatusTypeId, statusValue.ToString());
            }
        }

        /// <summary>
        /// Used to update the persona product setting type for the given user, product and setting (Used in AO)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userPersonaId"></param>
        /// <param name="settingType"></param>
        /// <param name="productId"></param>
        /// <param name="value"></param>
        public void UpdateProductSettingProductStatus<T>(long userPersonaId, string settingType, int productId, T value)
        {
            // add the new status flag to the product before we start
            IList<ProductSettingType> productSettingTypes = _productRepository.ListProductSettingType();
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            Dictionary<string, object> logData = new Dictionary<string, object>();

            string statusValue = value.ToString();
            logData.Add("Status Value", statusValue);
            if (Int32.Parse(statusValue) == (int)ProductBatchStatusType.Deleted || Int32.Parse(statusValue) == (int)ProductBatchStatusType.Inactive)
            {
                var persona = _managePersona.GetPersona(userPersonaId);
                var userLogin = _manageUserLogin.GetUserLoginOnly(persona.RealPageId);
                OrganizationStatus orgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLogin.UserId, userLogin.LastLogin, persona.OrganizationPartyId, false);

                int deactivatedStatus = (int)UserUiStatusType.Deactivated;
                logData.Add("User Current login", userLogin);
                WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "UpdateProductSettingProductStatus", $"User Current Status personaId={userPersonaId}" });
                if (orgStatus.Status.ToString().ToUpper() == UserUiStatusType.Disabled.ToString().ToUpper())
                {
                    statusValue = deactivatedStatus.ToString();
                }
            }

            // get the id for ProductStatus type
            if (productSettingTypes.Any(a => a.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase)))
            {
                int productStatusTypeId = (from a in productSettingTypes where a.Name.Equals(settingType, StringComparison.OrdinalIgnoreCase) select a.ProductSettingTypeId).FirstOrDefault();
                repositoryResponse = _productRepository.CreateProductSetting(userPersonaId, productId, productStatusTypeId, statusValue.ToString());
            }
        }
        /// <summary>
        /// Used to get product company information from BlueBook for the given product
        /// </summary>
        /// <param name="blueBookProductName">The BlueBook product name to request the company information for</param>
        /// <param name="includeExtra">Additional information to filter bluebook data returned</param>
        /// <param name="useTranslate">Only get the product instance id by translating UPFM if available</param>
        /// <returns>The blue book company instance information</returns>
        protected CustomerCompanyMap GetProductCompanyInstanceId(string blueBookProductName, string includeExtra = "", bool useTranslate = true)
        {
            IList<CustomerCompanyMap> companyProductList = _blueBook.GetCompanyMap(_editorPersona.Organization.RealPageId, _editorPersona.Organization.BooksCustomerMasterId, source: blueBookProductName.ToUpper(), domain: _editorPersona.Organization.OrganizationDomain.Name, includeExtra: includeExtra, useTranslate: useTranslate);
            if (companyProductList == null) { companyProductList = new List<CustomerCompanyMap>(); }
            CustomerCompanyMap company = new CustomerCompanyMap();
            if (companyProductList.Any(a => a.Source.Equals(blueBookProductName, StringComparison.OrdinalIgnoreCase)))
            {
                company = (from a in companyProductList where a.Source.Equals(blueBookProductName, StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
            }
            return company;
        }

        /// <summary>
        /// Used to validate or construct what looks like a valid email address to be used by products
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        protected string ValidateAndReturnEmailAddress(string emailAddress)
        {
            if (!new EmailAddressAttribute().IsValid(emailAddress))
            {
                try
                {
                    MailAddress ma = new MailAddress(emailAddress);
                    if (!ma.Host.Contains("."))
                    {
                        emailAddress = ValidateAndReturnEmailAddress(emailAddress + ".com");
                    }
                }
                catch (Exception ex)
                {
                    if (!emailAddress.Contains("@"))
                    {
                        // add @test.com to the passed email so it looks legit
                        emailAddress = ValidateAndReturnEmailAddress(emailAddress + "@bogusemail.com");
                    }
                }
            }
            else
            {
                return emailAddress;
            }

            return emailAddress;
        }

        /// <summary>
        /// Dumps API base URL and body
        /// </summary>
        protected void DumpApiCallInfoToDiagnosticLog(string baseUrlAndQuery, object apiPayLoad = null)
        {
            Dictionary<string, object> logData = new Dictionary<string, object>();
            logData.Add("baseUrlAndQuery", baseUrlAndQuery);

            if (apiPayLoad != null)
                logData.Add("apiPayLoad", JsonConvert.SerializeObject(apiPayLoad));

            WriteToDiagnosticLog("{ActionName} - {state}", logData, messageProperties: new object[] { "DumpApiCallInfoToDiagnosticLog", $"API Call for product {_productId} is getting called." });
        }

        #region Activity Logging

        /// <summary>
        /// Write unassign activity log for user
        /// </summary> 
        protected void WriteUnassignActivityLog(long fromPersonaId, long toPersonaId)
        {
            // log product user updated activity
            var fromUserLogDetail = GetUserActivityLogInfo(fromPersonaId);
            var toUserLogDetails = GetUserActivityLogInfo(toPersonaId);
            //var booksProductDetail = _productRepository.GetBooksMasterProductDetail(_productId);

            WriteActivityLog(fromUserLogDetail, toUserLogDetails,
               _productDetails.BooksProductCode,
                $"{toUserLogDetails.FirstName} {toUserLogDetails.LastName} is unassigned in product {_productDetails.Name} by user {fromUserLogDetail.FirstName} {fromUserLogDetail.LastName}.");
        }

        /// <summary>
        /// Write deactivated activity log for user
        /// </summary> 
        protected void WriteDeActivatedActivityLog(long fromPersonaId, long toPersonaId)
        {
            // log product user updated activity
            var fromUserLogDetail = GetUserActivityLogInfo(fromPersonaId);
            var toUserLogDetails = GetUserActivityLogInfo(toPersonaId);
            //var booksProductDetail = _productRepository.GetBooksMasterProductDetail(_productId);

            WriteActivityLog(fromUserLogDetail, toUserLogDetails,
               _productDetails.BooksProductCode,
                $"{toUserLogDetails.FirstName} {toUserLogDetails.LastName} is unassigned in product {_productDetails.Name} by user {fromUserLogDetail.FirstName} {fromUserLogDetail.LastName}.");
        }

        /// <summary>
        /// Write Reactivated activity log for user
        /// </summary> 
        protected void WriteReActivatedActivityLog(long fromPersonaId, long toPersonaId)
        {
            // log product user updated activity
            var fromUserLogDetail = GetUserActivityLogInfo(fromPersonaId);
            var toUserLogDetails = GetUserActivityLogInfo(toPersonaId);
            //var booksProductDetail = _productRepository.GetBooksMasterProductDetail(_productId);

            WriteActivityLog(fromUserLogDetail, toUserLogDetails,
               _productDetails.BooksProductCode,
                $"{toUserLogDetails.FirstName} {toUserLogDetails.LastName} is re-activated in product {_productDetails.Name} by user {fromUserLogDetail.FirstName} {fromUserLogDetail.LastName}.");
        }

        /// <summary>
        /// Write Create User activity log
        /// </summary> 
        protected void WriteCreateUserActivityLog(long fromPersonaId, IC.Person toPerson, UserLoginOnly toUserGbLogin)
        {
            WriteActivityLog(fromPersonaId, toPerson, toUserGbLogin,
                "{0} {1} created in product {2} by user {3} {4}.");
        }

        /// <summary>
        /// Write Update User activity log
        /// </summary> 
        protected void WriteResetVerificationCodeActivityLog(long fromPersonaId, IC.Person toPerson, UserLoginOnly toUserGbLogin)
        {
            WriteActivityLog(fromPersonaId, toPerson, toUserGbLogin,
                 "{3} {4} reset the OneSite verification code for {0} {1}.");
        }

        /// <summary>
        /// Write Update User-Type Activity Log
        /// </summary>
        protected void WriteUpdateUserTypeActivityLog(long fromPersonaId, IC.Person toPerson, UserLoginOnly toUserGbLogin, BatchProcessType batchProcessType)
        {
            string message = string.Empty;
            if (batchProcessType == BatchProcessType.UserTypeRegularToAdmin)
            {
                message = "{0} {1} user type changed from Regular User to admin in product {2} by user {3} {4}.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeAdminToRegular)
            {
                message = "{0} {1} user type changed from admin to Regular User in product {2} by user {3} {4}.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeAdminToExternal)
            {
                message = "{0} {1} user type changed from admin to External User in product {2} by user {3} {4}.";
            }
            else if (batchProcessType == BatchProcessType.UserTypeExternalToAdmin)
            {
                message = "{0} {1} user type changed from External User to admin in product {2} by user {3} {4}.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                WriteActivityLog(fromPersonaId, toPerson, toUserGbLogin, message);
            }
        }

        protected void WriteUserActivityLogWithMessage(long fromPersonaId, IC.Person toPerson, UserLoginOnly toUserGbLogin, string message)
        {
            WriteActivityLog(fromPersonaId, toPerson, toUserGbLogin, message);
        }

        protected void WriteActivityLogWithMessage(long fromPersonaId, long toPersonaId, string message)
        {
            // log product user updated activity
            var fromUserLogDetail = GetUserActivityLogInfo(fromPersonaId);
            var toUserLogDetail = GetUserActivityLogInfo(toPersonaId);
            //var booksProductDetail = _productRepository.GetBooksMasterProductDetail(_productId);

            var logMessage = string.Format(message, toUserLogDetail.FirstName, toUserLogDetail.LastName,
                _productDetails.Name, fromUserLogDetail.FirstName, fromUserLogDetail.LastName);

            WriteActivityLog(fromUserLogDetail, toUserLogDetail,
               _productDetails.BooksProductCode, logMessage);
        }

        protected void WriteActivityLogWithMessageByProduct(long fromPersonaId, long toPersonaId, int productId, string message)
        {
            // log product user updated activity
            var fromUserLogDetail = GetUserActivityLogInfo(fromPersonaId);
            var toUserLogDetail = GetUserActivityLogInfo(toPersonaId);
            var booksProductDetail = _productRepository.GetBooksMasterProductDetail(productId);

            var logMessage = string.Format(message, toUserLogDetail.FirstName, toUserLogDetail.LastName,
                booksProductDetail.Name, fromUserLogDetail.FirstName, fromUserLogDetail.LastName);

            WriteActivityLog(fromUserLogDetail, toUserLogDetail,
               booksProductDetail.BooksProductCode, logMessage);
        }
        
        private void WriteActivityLog(long fromPersonaId, IC.Person toPerson, UserLoginOnly toUserGbLogin, string message)
        {
            // log product user created activity
            var fromUserLogDetail = GetUserActivityLogInfo(fromPersonaId);
           // var booksProductDetail = _productRepository.GetBooksMasterProductDetail(_productId);

            var messageToLog = string.Format(message, toPerson.FirstName, toPerson.LastName,
                _productDetails.Name,
                fromUserLogDetail.FirstName, fromUserLogDetail.LastName);

            WriteActivityLog(fromUserLogDetail,
                 new UserActivityLogInfo
                 {
                     FirstName = toPerson.FirstName,
                     LastName = toPerson.LastName,
                     BooksOrganizationMasterId = fromUserLogDetail.BooksOrganizationMasterId,
                     OrganizationPartyId = fromUserLogDetail.OrganizationPartyId,
                     LoginName = toUserGbLogin.LoginName,
                     UserId = toUserGbLogin.UserId,
                     RealPageId = toUserGbLogin.RealPageId
                 },
            _productDetails.BooksProductCode, messageToLog);
        }

        /// <summary>
        /// Get User info for activity logging
        /// </summary>
        public UserActivityLogInfo GetUserActivityLogInfo(long personaId)
        {
            var persona = _managePersona.GetPersona(personaId);
            var userLogin = _manageUserLogin.GetUserLoginOnly(persona.RealPageId);
            var person = _managePerson.GetPerson(persona.RealPageId);

            return new UserActivityLogInfo
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                RealPageId = userLogin.RealPageId,
                LoginName = userLogin.LoginName,
                BooksOrganizationMasterId = persona.Organization.BooksMasterId,
                OrganizationPartyId = persona.OrganizationPartyId,
                UserId = userLogin.UserId
            };
        }

        private void WriteActivityLog(UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo, string booksProductCode, string message)
        {
            // log product user updated activity
            try
            {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.PRODUCT_ACCESS,
                    LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                    CorrelationId = _correlationId,
                    BooksMasterOrganizationId = toUserLogInfo.BooksOrganizationMasterId,
                    OrganizationPartyId = toUserLogInfo.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = fromUserLogInfo.LoginName,
                    FromUserLoginId = fromUserLogInfo.UserId,
                    FromUserFirstName = fromUserLogInfo.FirstName,
                    FromUserLastName = fromUserLogInfo.LastName,
                    FromUserRealpageId = fromUserLogInfo.RealPageId.ToString(),

                    ToUserLoginId = toUserLogInfo.UserId,
                    ToUserLoginName = toUserLogInfo.LoginName,
                    ToUserFirstName = toUserLogInfo.FirstName,
                    ToUserLastName = toUserLogInfo.LastName,
                    ToUserRealpageId = toUserLogInfo.RealPageId.ToString(),

                    BooksProductCode = booksProductCode
                });
            }
            catch (Exception ex)
            {
            }
        }

        private GbProductMap GetBooksMasterProductDetail(int productID, bool noCacheResult)
        {
            var productMap = new GbProductMap();
            var rpcache = new RPObjectCache();
            var cacheKey = "productDetails_" + productID.ToString();
            if (!noCacheResult)
            {
                return rpcache.GetFromCache<GbProductMap>(cacheKey, 120, () => _productRepository.GetBooksMasterProductDetail(productID));
            }

            return _productRepository.GetBooksMasterProductDetail(productID);
        }
        #endregion

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    /// <summary>
    /// Used internally for activity logging
    /// </summary>
    public class UserActivityLogInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid RealPageId { get; set; }
        public string LoginName { get; set; }
        public long BooksOrganizationMasterId { get; set; }
        public long OrganizationPartyId { get; set; }
        public string OrganizationName { get; set; }
        public long UserId { get; set; }
        public string ClientCode { get; set; } = null;
        public Guid OrganizationRealpageId { get; set; }
    }

    /// <summary>
    /// Used to help convert product classes to GreenBook classes
    /// </summary>
    public static class BlueBookHelpers
    {
        /// <summary>
        /// Used to convert a BlueBook property into a GreenBook property
        /// </summary>
        /// <param name="properties">The list of roles to convert</param>
        /// <returns></returns>
        public static IList<ProductProperty> FromBlueBookToGBProperties(this IList<PropertyInstance> properties)
        {
            if (properties == null) return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (PropertyInstance property in properties)
            {
                if (property.IsActive)
                {
                    results.Add(new ProductProperty
                    {
                        ID = property.PropertyInstanceSourceId,
                        Name = property.PropertyName,
                        State = property.Address.State
                    });
                }
            }
            return results;
        }

        /// <summary>
        /// Used to convert a BlueBook property into a GreenBook property
        /// </summary>
        /// <param name="properties">The list of roles to convert</param>
        /// <returns></returns>
        public static IList<ProductProperty> MapBlueBookToGBProperties(this CompanyPropertyRootObject companyProperties)
        {
            if (companyProperties == null) return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (var property in companyProperties.data.attributes.getCompanyPropertyInstances)
            {
                if (property.isActive)
                {
                    results.Add(new ProductProperty
                    {
                        ID = property.propertyInstanceSourceId,
                        Name = property.propertyName,
                        State = property.state
                    });
                }
            }
            return results;
        }

        /// <summary>
        /// Used to convert a BlueBook master property into a GreenBook property
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IList<ProductProperty> FromBlueBookMasterPropertyToGBProperties(this IList<CustomerCompanyPropertyMap> properties)
        {
            if (properties == null) return null;
            IList<ProductProperty> results = new List<ProductProperty>();
            foreach (CustomerCompanyPropertyMap property in properties)
            {
                if (property.IsActive)
                {
                    results.Add(new ProductProperty
                    {
                        ID = property.CustomerPropertyId.ToString(),
                        Name = property.PropertyName,
                        Street1 = property.PropertyAddress,
                        State = property.PropertyState
                    });
                }
            }
            return results;
        }
    }
}