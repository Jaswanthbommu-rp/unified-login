using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Maintenance;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Organization = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization;
using PropertySetup = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.PropertySetup;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage Organization repository calls
    /// </summary>
    public class ManageOrganization : IManageOrganization
    {
        #region Private Variables
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly IOrganizationProductRepository _organizationProductRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IManageBlueBook _manageBlueBook;
        private readonly IManageProductPanel _manageProductPanel;
        private readonly IManageUnifiedSettings _manageUnifiedSettings;
        private readonly IConfigurationSettingRepository _configurationSettingRepository;
        private readonly IManageOrganizationProduct _manageOrganizationProduct;
        private readonly IManageProduct _manageProduct;
        private readonly ITokenHelper _tokenHelper;
        private readonly IIntegrationTypeFactory _integrationTypeFactory;
        private IManageUnifiedLogin _manageUnifiedLogin;
        private readonly IManageUser _manageUser;
        private readonly IManagePartyRole _managePartyRole;
        private readonly DefaultUserClaim _defaultUserClaim;
        private readonly IManageProductAssetOptimization _manageProductAssetOptimization;
        private readonly int _maxDOPSetting = 6;
        #endregion

        #region Constructors

        /// <summary>
        /// Unit Test Constructor
        /// </summary>
        public ManageOrganization(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, IManageProductAssetOptimization manageProductAssetOptimization = null)
        {
            _defaultUserClaim = userClaim;
            _organizationRepository = new OrganizationRepository(repository);
            _credentialRepository = new CredentialRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _personaRepository = new PersonaRepository(repository);
            _organizationProductRepository = new OrganizationProductRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _productRepository = new ProductRepository(repository, userClaim);
            _manageBlueBook = new ManageBlueBook(_defaultUserClaim, repository, _productInternalSettingRepository, messageHandler);
            _manageProductPanel = new ManageProductPanel(_defaultUserClaim, repository, _manageBlueBook, messageHandler, null);
            _propertyRepository = new PropertyRepository(repository);
            _configurationSettingRepository = new ConfigurationSettingRepository(repository);
            _manageUnifiedSettings = new ManageUnifiedSettings(repository, userClaim, messageHandler);
            _manageProduct = new ManageProduct(repository, userClaim, messageHandler);
            _manageOrganizationProduct = new ManageOrganizationProduct(userClaim, repository, _manageBlueBook, _manageProduct);
            _tokenHelper = new TokenHelper(repository);
            _manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaim, messageHandler);
            _manageUser = new ManageUser(repository, userClaim, messageHandler);
            _managePartyRole = new ManagePartyRole(repository);
            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, _manageUnifiedLogin, null, _productRepository,
                _productInternalSettingRepository, userClaim);
            _manageProductAssetOptimization = manageProductAssetOptimization;
        }

        /// <summary>
        /// Audit Unit Test Constructor
        /// </summary>
        public ManageOrganization(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, IManageProductOneSite manageProductOneSite)
        {
            _defaultUserClaim = userClaim;
            _organizationRepository = new OrganizationRepository(repository);
            _credentialRepository = new CredentialRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _personaRepository = new PersonaRepository(repository);
            _organizationProductRepository = new OrganizationProductRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _productRepository = new ProductRepository(repository, userClaim);
            _manageBlueBook = new ManageBlueBook(_defaultUserClaim, repository, _productInternalSettingRepository, messageHandler);
            _manageProductPanel = new ManageProductPanel(_defaultUserClaim, repository, _manageBlueBook, messageHandler, manageProductOneSite);
            _propertyRepository = new PropertyRepository(repository);
            _configurationSettingRepository = new ConfigurationSettingRepository(repository);
            _manageUnifiedSettings = new ManageUnifiedSettings(repository, userClaim, messageHandler);
            _manageProduct = new ManageProduct(repository, userClaim, messageHandler);
            _manageOrganizationProduct = new ManageOrganizationProduct(userClaim, repository, _manageBlueBook, _manageProduct);
            _tokenHelper = new TokenHelper(repository);
            _manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaim, messageHandler);
            _manageUser = new ManageUser(repository, userClaim, messageHandler);
            _managePartyRole = new ManagePartyRole(repository);
            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, _manageUnifiedLogin, null, _productRepository,
                _productInternalSettingRepository, userClaim);
        }

        /// <summary>
        /// Create a basic instance of the ManageOrganization Controller class
        /// </summary>
        public ManageOrganization(DefaultUserClaim userClaim)
        {
            _organizationRepository = new OrganizationRepository();
            _credentialRepository = new CredentialRepository();
            _userLoginRepository = new UserLoginRepository();
            _personaRepository = new PersonaRepository(userClaim);
            _organizationProductRepository = new OrganizationProductRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _propertyRepository = new PropertyRepository();
            _configurationSettingRepository = new ConfigurationSettingRepository();
            _manageBlueBook = new ManageBlueBook(userClaim);
            _defaultUserClaim = userClaim;
            _manageUnifiedSettings = new ManageUnifiedSettings(userClaim);
            _manageOrganizationProduct = new ManageOrganizationProduct(userClaim);
            _manageProductPanel = new ManageProductPanel(userClaim);
            _manageProduct = new ManageProduct(userClaim);
            _tokenHelper = new TokenHelper();
            _manageUnifiedLogin = new ManageUnifiedLogin(userClaim);
            _manageUser = new ManageUser(userClaim);
            _managePartyRole = new ManagePartyRole();
            _integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, _manageUnifiedLogin, null, _productRepository,
                _productInternalSettingRepository, userClaim);
            _manageProductAssetOptimization = new ManageProductAssetOptimization(userClaim);
        }

        #endregion

        #region Public Organization methods

        public ObjectOutput<OrganizationCreateResult, IErrorData> CreateOrganization(OrganizationCreate organization, List<int> addProductList, bool processBlueBookMessage = false)
        {
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateOrganization", $"organization name {organization.Name}" });
            var outputResult = new ObjectOutput<OrganizationCreateResult, IErrorData>() { Status = new Status<IErrorData>() { Success = false } };

            if (organization.OrganizationTypeId == 0)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateOrganization", $"organization.OrganizationTypeId and organization name is: {organization.Name}" });
                outputResult.Status.ErrorMsg = $"An invalid Organization Type id was given: {organization.OrganizationTypeId}";
                return outputResult;
            }

            OrganizationAdminUser aUser = organization.AdminUser;
            if (aUser == null)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateOrganization", $"aUser is null and organization name is: {organization.Name}" });
                outputResult.Status.ErrorMsg = "No admin user information provided";
                return outputResult;
            }

            // create the organization
            Organization org = new Organization()
            {
                Name = organization.Name,
                BooksMasterId = (long)organization.BooksCompanyId,
                BooksCustomerMasterId = (long)organization.BooksCustomerMasterId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = organization.OrganizationTypeId
                },
                OrganizationDomain = new OrganizationDomain()
                {
                    OrganizationDomainId = organization.OrganizationDomainId,
                    Name = organization.OrganizationDomain
                },
                IsActive = organization.IsActive
            };
            var repositoryResponse = InsertOrganization(org);

            if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
            {
                outputResult.Status.ErrorMsg = repositoryResponse.ErrorMessage;
                return outputResult;
            }

            Guid organizationRealPageId = repositoryResponse.RealPageId;

            org = GetOrganization(organizationRealPageId);

            //create/update primary property or enterprise role setting
            CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(org.PartyId, "PrimaryProperty", organization.EnablePrimaryProperties);
            CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(org.PartyId, "EnterpriseRole", organization.EnableEnterpriseRoles);

            org.EnablePrimaryProperties = organization.EnablePrimaryProperties;
            org.EnableEnterpriseRoles = organization.EnablePrimaryProperties;

            // add the given products to the new company
            var productResponse = AddProductsToOrganization(addProductList, org.PartyId, organization.OrganizationTypeId, organization.Name);
            if (!string.IsNullOrEmpty(productResponse.ErrorMessage))
            {
                outputResult.Status.ErrorMsg = productResponse.ErrorMessage;
                return outputResult;
            }
            // add the first time super user to the new org
            aUser.Email = $"{org.PartyId}admin@realpage.com".Replace(" ", "");
            CreateInitialOrgSuperUser(org.PartyId, aUser.FirstName, "", aUser.LastName, aUser.Title, aUser.Suffix, aUser.Email, true, null, organizationRealPageId);

            OrganizationCreateResult createOrg = new OrganizationCreateResult()
            {
                Org = org,
                adminLogin = aUser.Email,
                BooksCompanyId = org.BooksMasterId,
                BooksCustomerMasterId = org.BooksCustomerMasterId
            };
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateOrganization", "Before in admin user" });
            //Create an additional admin user for the Company
            if (processBlueBookMessage && organization.CompanyAdminUser != null && !string.IsNullOrWhiteSpace(organization.CompanyAdminUser.Email))
            {
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "CreateOrganization", "In admin user" });
                UserLoginOnly findExistingUser = _userLoginRepository.GetUserLoginOnly(organization.CompanyAdminUser.Email);

                IList<Persona> personaList = new List<Persona>();
                ProfileDetail profileDetail = new ProfileDetail()
                {
                    FirstName = organization.CompanyAdminUser.FirstName,
                    LastName = organization.CompanyAdminUser.LastName,
                    RoleIdList = organization.CompanyAdminUser.RoleIds,

                    MiddleName = string.Empty,
                    NotificationEmail = string.Empty,
                    Password = string.Empty,
                    Persona = new List<Persona>(),
                    CreateUserSourceType = CreateUserSourceType.UnifiedPlatform,
                    TelecommunicationNumber = new List<TelecommunicationNumber>(),
                    CustomFields = new List<CustomFieldValue>(),
                    UserTypeId = (int)UserRoleType.SuperUser,
                    Title = string.Empty,
                    userLogin = new UserLogin()
                    {
                        ThruDate = null,
                        LoginName = organization.CompanyAdminUser.Email,
                        IsActive = true,
                        IsPending = false,
                        IsExpired = false,
                        FromDate = DateTime.UtcNow,
                        Is3rdPartyIDP = false
                    },
                    productBatch = new List<ProductBatch>(),
                    ExternalUserRelationship = new ExternalUserRelationship() { ThirdPartyRelationShipId = 8, ThirdPartyRelationShip = "8" }
                };
                if (findExistingUser != null)
                {
                    ManagePerson managePerson = new ManagePerson();
                    var existingPerson = managePerson.GetPerson(findExistingUser.RealPageId);
                    if (existingPerson != null)
                    {
                        profileDetail.FirstName = existingPerson.FirstName;
                        profileDetail.LastName = existingPerson.LastName;
                        profileDetail.UserTypeId = (int)UserRoleType.ExternalUser;
                        UnifiedLoginRepository umr = new UnifiedLoginRepository();
                        List<int> _productIdList = new List<int>() { (int)ProductEnum.UnifiedPlatform };
                        var gbAllRoles = umr.ListRolesForProductsByPartyId(org.PartyId, (int)ProductEnum.UnifiedPlatform, _productIdList);
                        if (gbAllRoles.Any(p => p.Roletype.Equals("System", StringComparison.OrdinalIgnoreCase) && p.Name.Equals("User Administrator", StringComparison.OrdinalIgnoreCase)))
                        {
                            string roleId = gbAllRoles?.Find(p => p.Roletype.Equals("System", StringComparison.OrdinalIgnoreCase) && p.Name.Equals("User Administrator", StringComparison.OrdinalIgnoreCase)).ID;
                            if (!string.IsNullOrEmpty(roleId))
                            {
                                ProductBatch pb = new ProductBatch
                                {
                                    ProductId = (int)ProductEnum.UnifiedPlatform,
                                    InputJson = new RolePropertyList
                                    {
                                        RoleList = new List<string>() { roleId }
                                    }
                                };
                                profileDetail.productBatch = new List<ProductBatch>() { pb };
                            }
                        }
                    }
                }

                //Default Persona
                IList<PersonaEnvironment> personaEnvironment = _personaRepository.GetPersonaEnvironmentType();
                var personaEnviornment = personaEnvironment.SingleOrDefault<PersonaEnvironment>(p => p.Name.Equals("Production", StringComparison.OrdinalIgnoreCase));
                if (personaEnviornment == null)
                {
                    outputResult.Status.Success = true;
                    outputResult.Status.ErrorMsg = $"MessageHandler.Handle - Persona environment is missing!";
                    return outputResult;
                }
                else
                {
                    Persona persona = new Persona()
                    {
                        Name = profileDetail.UserTypeId == (int)UserRoleType.SuperUser ? "System Administrator" : "Primary",
                        PersonaEnvironmentTypeId = (int)personaEnviornment.PersonaEnvironmentTypeId,
                        FromDate = DateTime.UtcNow,
                        ThruDate = null
                    };
                    personaList.Add(persona);
                    profileDetail.Persona = personaList;
                }

                if (profileDetail.organization.Count == 0)
                {
                    profileDetail.organization.Add(org);
                }
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateOrganization", $"Before creating user {profileDetail.userLogin.LoginName}" });
                CreateUserResponse<IErrorData> errorDataResponse = _manageUser.CreateUser(profileDetail, personaList);
                if (!errorDataResponse.Status.Success)
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "CreateOrganization", $"In error while creating user {profileDetail.userLogin.LoginName} error message is {errorDataResponse.Status.ErrorMsg}" });
                    outputResult.Status.Success = true;
                    outputResult.Status.ErrorMsg = $"{profileDetail.userLogin.LoginName}: " + errorDataResponse.Status.ErrorMsg;
                    return outputResult;
                }

                IPartyRole partyRole = new PartyRole()
                {
                    RoleTypeId = profileDetail.UserTypeId
                };
                _managePartyRole.CreatePartyRoleEnterpriseUserID(profileDetail.userLogin.RealPageId, partyRole);
            }

            outputResult.Status.Success = true;
            outputResult.Status.ErrorMsg = "";
            outputResult.obj = createOrg;
            return outputResult;
        }

        /// <summary>
        /// Used to insert a new Organization
        /// </summary>
        /// <param name="organization">Organization Object</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse InsertOrganization(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization), "Null Organization.");
            }
            //See if the organization.BooksMasterId already exists and UPDATE
            if (organization.RealPageId != Guid.Empty)
            {
                var oldOrganization = GetOrganization(organization.RealPageId);
                var repositoryResponse = _organizationRepository.UpdateOrganization(organization);

                if (string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                {
                    var orgTypes = ListOrganizationType();
                    var auditData = GetUpdatedOrganizationLogActivity(oldOrganization, organization, orgTypes);

                    if (auditData.Count > 0)
                    {
                        var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} updated the {oldOrganization.Name} company";
                        LogAuditActivity(LogActivityTypeConstants.COMPANY_UPDATED, LogActivityCategoryType.CompanySetup, message, auditData);
                    }
                }

                return repositoryResponse;
            }
            //Create a new organization
            else
            {
                var repositoryResponse = _organizationRepository.InsertOrganization(organization);

                if (string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                {
                    var orgTypes = ListOrganizationType();
                    var newOrg = new Organization() { OrganizationDomain = new OrganizationDomain() };
                    var auditData = GetUpdatedOrganizationLogActivity(newOrg, organization, orgTypes);

                    if (auditData.Count > 0)
                    {
                        var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} created a new company {organization.Name}";
                        LogAuditActivity(LogActivityTypeConstants.COMPANY_CREATED, LogActivityCategoryType.CompanySetup, message, auditData);
                    }
                }

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Used to create the initial Super User for the new Organization
        /// </summary>
        /// <param name="organizationId">The partyId of the organization where the user will be added</param>
        /// <param name="firstName">The users first name</param>
        /// <param name="middleName">The users middle name</param>
        /// <param name="lastName">The users last name</param>
        /// <param name="title">The users title</param>
        /// <param name="suffix">The users suffix</param>
        /// <param name="email">The users email address</param>
        /// <param name="defaultIDP">Should the user be assigned to the default IDP</param>
        /// <param name="idpTypeId">The id of the IDP to assign the user to</param>
        /// <param name="organizationRealPageId">Organization Enterprise RealPageId</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse CreateInitialOrgSuperUser(long organizationId, string firstName, string middleName, string lastName, string title, string suffix, string email, bool defaultIDP, int? idpTypeId, Guid organizationRealPageId)
        {
            var cacheKey = $"getProductIdListByCompanyGuid_{organizationRealPageId}";
            RPObjectCache.RemoveFromCache(cacheKey);
            IList<int> productIdList = _productRepository.GetProductIdsByCompany(organizationRealPageId);

            //Exclude following products from RealPage Employee Access admin user
            var _productInternalSettings = _productInternalSettingRepository.GetProductInternalSettings(3);
            var excludeProductList = _productInternalSettings.Find(a => a.Name.Equals("ExcludeProductFromOrgSupportUser", StringComparison.OrdinalIgnoreCase))?.Value;
            if (excludeProductList != null)
            {
                foreach (var productId in excludeProductList.Split(','))
                {
                    productIdList.Remove(productIdList.FirstOrDefault(p => p == Convert.ToInt32(productId)));
                }
            }
            else
            {
                //Unified Platform, Asset Optimization, RealPage Accounting, Client Portal, Product Updates, EasyLMS, Admin & Support Portal
                int[] removeProductIds = new int[] { 3, 4, 8, 14, 28, 36, 89, 98 };
                foreach (var productId in removeProductIds)
                {
                    productIdList.Remove(productIdList.FirstOrDefault(p => p == productId));
                }
            }

            return _organizationRepository.CreateInitialOrgSuperUser(organizationId, firstName, middleName, lastName, title, suffix, email, defaultIDP, idpTypeId, productIdList);
        }

        /// <summary>
        /// Used to update an existing Organization
        /// </summary>
        /// <param name="organization">Organization Object</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateOrganization(Organization organization)
        {
            if (organization == null)
            {
                return new RepositoryResponse() { ErrorMessage = "Organization is Null" };
            }

            if (organization.RealPageId == Guid.Empty)
            {
                return new RepositoryResponse() { ErrorMessage = "Invalid parameter realPageId." };
            }

            var oldOrganization = GetOrganization(organization.RealPageId);
            var repositoryResponse = _organizationRepository.UpdateOrganization(organization);

            if (string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
            {
                var auditData = GetUpdatedOrganizationLogActivity(oldOrganization, organization, ListOrganizationType());
                if (auditData.Count > 0)
                {
                    var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} updated the {organization.Name} company";
                    LogAuditActivity(LogActivityTypeConstants.COMPANY_UPDATED, LogActivityCategoryType.CompanySetup, message, auditData);
                }
            }
            return repositoryResponse;
        }
        /// <summary>
        /// Used to update an Organization ThirdPartyIDP
        /// </summary>
        /// <param name="organization">Organization Object</param>
        /// <returns>RepositoryResponse object</returns>
        public void UpdateOrganizationThirdPartyIDP(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization), "Null Organization.");
            }
            if (organization.RealPageId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(organization), "Invalid parameter realPageId.");
            }
            IList<IdentityProviderType> companyIDPS = _organizationRepository.GetOrganizationIdentityProviderType(organization.RealPageId);
            if (companyIDPS.Count < 2 && organization.ThirdPartyIDP != "None")
            {
                var repositoryResponse = _organizationRepository.UpdateOrganizationThirdPartyIDP(organization);

                if (repositoryResponse != null && string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
                {
                    List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                    additionalParameters.Add(new AdditionalParameters() { Key = "CompanyName", Value = $"{{ \"old\": \"None\", \"new\": \"{organization.ThirdPartyIDP}\" }}" });

                    if (additionalParameters.Count > 0)
                    {
                        var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} updated the {organization.Name} company";
                        LogAuditActivity(LogActivityTypeConstants.COMPANY_UPDATED, LogActivityCategoryType.CompanySetup, message, additionalParameters);
                    }
                }
            }
        }


        public void UpdateOrganizationUsePrimaryPropertySetting(Organization organization)
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization), "Null Organization.");
            }

            if (organization.RealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }

            //create/update use primaryproperty setting
            CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(organization.PartyId, "PrimaryProperty", organization.EnablePrimaryProperties);
            CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(organization.PartyId, "EnterpriseRole", organization.EnableEnterpriseRoles);
        }

        /// <summary>
		/// UpdateUsePrimaryPropertyForOrganizationProduct
		/// </summary>
		/// <param name="organizationPartyId">organizationPartyId</param>
		/// <param name="productId">productId</param>
		/// <param name="usePrimaryProperty">usePrimaryProperty</param>
		/// <returns></returns>
		public RepositoryResponse UpdateUsePrimaryPropertyForOrganizationProduct(long organizationPartyId, int productId, bool usePrimaryProperty)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            if (organizationPartyId == 0)
            {
                repositoryResponse.ErrorMessage = "Invalid parameter organizationPartyId.";
                return repositoryResponse;
            }
            if (productId == 0)
            {
                repositoryResponse.ErrorMessage = "Invalid parameter productId.";
                return repositoryResponse;
            }
            var organizationDetails = _organizationRepository.GetOrganization(null, organizationPartyId);
            if (organizationDetails.EnablePrimaryProperties == 1)
            {
                var productSettingByOrg = _productRepository.GetProductSettings(organizationDetails.RealPageId, productId).ToList();
                string oldSetting = string.Empty;
                if (productSettingByOrg.Exists(p => p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)))
                {
                    oldSetting = productSettingByOrg.Find(p => p.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase)).Value;
                }

                var productSettingTypeId = _productRepository.GetProductSettingType("UsePrimaryProperties");
                repositoryResponse = _organizationProductRepository.CreateOrganizationProductSetting(organizationPartyId, productId, productSettingTypeId, usePrimaryProperty ? "1" : "0");

                var newSetting = (usePrimaryProperty ? "1" : "0");
                if (newSetting != oldSetting)
                {
                    var productList = _productRepository.GetAllProducts();
                    var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} updated primary properties for the {organizationDetails.Name} company.";
                    List<AdditionalParameters> auditData = new List<AdditionalParameters>()
                    {
                        new AdditionalParameters() { Key = $"{productList.First(s => s.ProductId == productId).Name}", Value = $"{{ \"old\": \"{oldSetting}\", \"new\": \"{newSetting}\" }}" }
                    };
                    LogAuditActivity(LogActivityTypeConstants.COMPANY_UPDATED, LogActivityCategoryType.CompanySetup, message, auditData);
                }
            }
            else
            {
                repositoryResponse.ErrorMessage = "Primary properties is not turned on at company level.";
            }
            return repositoryResponse;
        }


        /// <summary>
        /// Get Organization details
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <param name="organizationPartyId">Optional organization PartyId</param>
        /// <returns>Organization object</returns>
        public Organization GetOrganization(Guid realPageId, long? organizationPartyId = null)
        {
            if (realPageId == Guid.Empty && organizationPartyId == null)
            {
                throw new Exception("Invalid parameter: Organization realPageId, partyId is required.");
            }
            Organization organization = _organizationRepository.GetOrganization(realPageId, organizationPartyId);
            IList<IdentityProviderType> companyIDPS = _organizationRepository.GetOrganizationIdentityProviderType(realPageId);
            if(organization != null)
            {
                if (companyIDPS != null && companyIDPS.Count > 1)
                {
                    var idp = companyIDPS.FirstOrDefault(i => i.Name != "IdentityServer");
                    if (idp != null)
                    {
                        organization.ThirdPartyIDP = idp.Name;
                    }
                    else
                    {
                        organization.ThirdPartyIDP = "None";
                    }
                }
                else
                {
                    organization.ThirdPartyIDP = "None";
                }
            }
            return organization;
        }

        /// <summary>
        /// Get Organization setting value
        /// </summary>
        /// <param name="settingName">settingName</param>
        /// <param name="organizationPartyId">Optional organization PartyId</param>
        /// <returns>setting value</returns>
        public string GetOrganizationSettingValue(long organizationPartyId, string settingName)
        {
            if (settingName == string.Empty)
            {
                throw new Exception("Invalid parameter: Setting name is required.");
            }

            if (organizationPartyId == null || organizationPartyId == 0)
            {
                throw new Exception("Invalid parameter: Organization partyId is required.");
            }

            return _organizationRepository.GetOrganizationSettingValue(settingName, organizationPartyId);
        }

        /// <summary>
        /// Used to get the list of organizations
        /// </summary>
        /// <returns>List of Organization</returns>
        public IList<Organization> GetOrganizationList()
        {
            IList<Organization> orgList = _organizationRepository.GetOrganizationList();
            return orgList;
        }

        /// <summary>
        /// List of Unified Login companies
        /// </summary>       
        /// <returns>List of Unified Login companies including admin user info</returns>
        public List<UnifiedLoginCompany> GetUnifiedLoginCompanyList()
        {
            return _organizationRepository.GetUnifiedLoginCompanyList();
        }

        /// <summary>
        /// Used to get the RealPageId of the admin user of the organization
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public Guid GetOrganizationAdminUserRealPageId(Guid organizationRealPageId)
        {
            return _organizationRepository.GetOrganizationAdminUserRealPageId(organizationRealPageId);
        }

        /// <summary>
        /// Used to get the Organization Identity ProviderType by realPageId
        /// </summary>
        /// <param name="realPageId">Organization unique identifier</param>
        /// <returns>Identity Provider Type object</returns>
        public IList<IdentityProviderType> GetOrganizationIdentityProviderType(Guid realPageId)
        {
            return _organizationRepository.GetOrganizationIdentityProviderType(realPageId);
        }

        /// <summary>
        /// Check if organization is the same
        /// </summary>
        /// <param name="organizationMasterId">The master id for the RealPage company</param>
        /// <param name="realPageId">User RealPageId</param>
        /// <param name="organizationId">User Organization RealPageId</param>
        public bool ValidateOrganization(long organizationMasterId, Guid realPageId, Guid organizationId)
        {
            if (organizationMasterId == 0)
            {
                throw new ArgumentNullException(nameof(realPageId), "OrganizationMasterId is required.");
            }

            if (realPageId == null)
            {
                throw new ArgumentNullException(nameof(realPageId), "RealPageId is required.");
            }

            if (organizationId == null)
            {
                throw new ArgumentException(nameof(organizationId), "OrganizationId is required.");
            }

            IList<Organization> listOrg = _credentialRepository.ListOrganizationByRealPageId(realPageId);
            if (listOrg != null)
            {
                if (organizationMasterId == Convert.ToInt64(ConfigReader.OrgMasterId) || listOrg.Any(a => a.RealPageId == organizationId))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Used to parse the list of valid product codes
        /// </summary>
        /// <param name="productCode"></param>
        /// <param name="addProductList"></param>
        /// <returns></returns>
        public List<string> ParseProduct(List<string> productCode, List<int> addProductList)
        {
            var productList = _productRepository.GetAllProducts();

            List<string> invalidProductList = new List<string>();
            if (productCode != null)
            {
                foreach (string product in productCode)
                {
                    bool foundProduct = AddProductToList(productList, product, addProductList);
                    if (!foundProduct)
                    {
                        invalidProductList.Add(product);
                    }
                }
            }

            return invalidProductList;
        }

        /// <summary>
        /// Used to convert the BlueBook product name to the UnifiedUI product id
        /// </summary>
        /// <param name="product"></param>
        /// <param name="addProductList"></param>
        /// <param name="productList"></param>
        /// <returns></returns>
        private bool AddProductToList(IList<GbProductMap> productList, string product, List<int> addProductList)
        {
            bool foundProduct = false;
            var lookupValue = productList.FirstOrDefault(a => a.BooksProductCode?.Equals(product, StringComparison.OrdinalIgnoreCase) == true);

            if (lookupValue != null)
            {
                foundProduct = true;
                addProductList.Add(lookupValue.ProductId);
            }
            return foundProduct;
        }

        /// <summary>
        /// Used to add a product to an organization
        /// </summary>
        /// <param name="addProductList">Product List</param>
        /// <param name="partyId">Organization PartyId</param>
        /// <param name="organizationTypeId">Organization Type</param>
        /// <param name="organizationName">organizationName</param>
        /// <returns>IRepositoryResponse</returns>
        private IRepositoryResponse AddProductsToOrganization(List<int> addProductList, long partyId, int organizationTypeId, string organizationName)
        {
            RepositoryResponse response = new RepositoryResponse();

            List<OrganizationType> organizationTypeList = ListOrganizationType();
            string organizationTypeName = organizationTypeList.Find(o => o.OrganizationTypeId == organizationTypeId).Name;
            //Enable Default products for company
            var productInternalSettingsByType = _productInternalSettingRepository.GetProductSettingByType("AlwaysEnableProductForOrgType");
            foreach (var productSetting in productInternalSettingsByType)
            {
                string[] types = productSetting.Value.Split(',');
                if (types.Contains(organizationTypeName) && !addProductList.Contains(productSetting.ProductId))
                {
                    // add unified login product to every new org
                    addProductList.Add(productSetting.ProductId);
                }
            }
            //Enable Product On Other Products Activation //TODO test one more time
            EnableProductOnOtherProductsActivation(addProductList);
            List<KeyValuePair<int, RepositoryResponse>> responseList = new List<KeyValuePair<int, RepositoryResponse>>();

            foreach (int product in addProductList)
            {
                response = _manageOrganizationProduct.InsertUpdateOrganizationProduct(partyId: partyId, product: product, configurationId: null, fromDate: null, thruDate: null, orgName: organizationName);
                responseList.Add(new KeyValuePair<int, RepositoryResponse>(product, response));
            }

            if (responseList.Count > 0)
            {
                List<string> enabledProducts = new List<string>();
                List<string> failedProducts = new List<string>();
                List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
                var products = _productRepository.GetAllProducts();

                foreach (var p in responseList)
                {
                    if (string.IsNullOrEmpty(p.Value.ErrorMessage))
                    {
                        enabledProducts.Add(products.First(po => po.ProductId == p.Key).Name);
                    }
                    else
                    {
                        failedProducts.Add(products.First(po => po.ProductId == p.Key).Name);
                    }
                }

                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} enabled products for {organizationName}";
                if (enabledProducts.Count > 0)
                {
                    additionalParameters.Add(new AdditionalParameters() { Key = "EnabledProducts", Value = string.Join(", ", enabledProducts) });
                }
                if (failedProducts.Count > 0)
                {
                    additionalParameters.Add(new AdditionalParameters() { Key = "FailedProducts", Value = string.Join(", ", failedProducts) });
                }

                LogAuditActivity(LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message, additionalParameters);
            }

            return response;
        }

        public List<int> EnableProductOnOtherProductsActivation(List<int> addProductList)
        {
            //Enable Product On Other Products Activation
            var productsToActivateOnOtherProductActivation = _productInternalSettingRepository.GetProductSettingByType("EnableProductOnOtherProductsActivation");
            foreach (var productsToActivate in productsToActivateOnOtherProductActivation)
            {
                int[] products = Array.ConvertAll(productsToActivate.Value.Split(','), int.Parse);
                foreach (int productId in products)
                {
                    if (addProductList.Contains(productId) && !addProductList.Contains(productsToActivate.ProductId))
                    {
                        addProductList.Add(productsToActivate.ProductId);
                    }
                }
            }
            return addProductList;
        }

        #region Public Organization Type methods
        /// <summary>
        /// Used to list the Organization Types
        /// </summary>
        /// <returns>list of OrganizationType objects</returns>
        public List<OrganizationType> ListOrganizationType()
        {
            return _organizationRepository.ListOrganizationType();
        }
        #endregion

        #region Public Organization Domain methods
        /// <summary>
        /// Used to list the Organization Domains
        /// </summary>
        /// <returns>list of OrganizationDomain objects</returns>
        public List<OrganizationDomain> ListOrganizationDomain()
        {
            return _organizationRepository.ListOrganizationDomain();
        }

        /// <summary>
        /// Used to add a new organization domain
        /// </summary>
        /// <param name="organizationDomain"></param>
        /// <returns></returns>
        public RepositoryResponse CreateOrganizationDomain(OrganizationDomain organizationDomain)
        {
            return _organizationRepository.CreateOrganizationDomain(organizationDomain);
        }
        #endregion

        #region Organization Delete

        /// <summary>
        /// Used to add a new organization into the queue to be deleted
        /// </summary>
        /// <param name="organizationRemovalQueue"></param>
        /// <returns></returns>
        public OrganizationRemovalQueue InsertOrganizationRemovalQueue(OrganizationRemovalQueue organizationRemovalQueue)
        {
            return _organizationRepository.InsertOrganizationRemovalQueue(organizationRemovalQueue);
        }

        /// <summary>
        /// Delete any organizations available to be deleted
        /// </summary>
        public void DeleteQueuedOrganizations()
        {
            var productInternalSettings = _manageProduct.GetProductInternalSettings(3);
            int batchSize = 5;
            int retryCount = 3;

            if (productInternalSettings.Any(p => p.Name.Equals("OrganizationRemovalBatchSize", StringComparison.OrdinalIgnoreCase)))
            {
                batchSize = Convert.ToInt32(productInternalSettings.First(p => p.Name.Equals("OrganizationRemovalBatchSize", StringComparison.OrdinalIgnoreCase)).Value);
            }
            if (productInternalSettings.Any(p => p.Name.Equals("OrganizationRemovalRetryCount", StringComparison.OrdinalIgnoreCase)))
            {
                retryCount = Convert.ToInt32(productInternalSettings.First(p => p.Name.Equals("OrganizationRemovalRetryCount", StringComparison.OrdinalIgnoreCase)).Value);
            }

            var orgsToDelete = _organizationRepository.GetOrganizationToDelete(batchSize, retryCount, false);

            orgsToDelete.ForEach(p =>
            {
                try
                {
                    var deleteResult = _organizationRepository.DeleteOrganization(p.OrganizationRemovalQueueId, p.OrganizationPartyId, p.OrganizationRealPageId);
                    if (deleteResult == p.OrganizationPartyId)
                    {
                        if (p.OrganizationRemoveUDMData)
                        {
                            // get list of current properties for the company being deleted
                            var propertyToDeleteList = _manageBlueBook.GetPropertyInstanceForCompany(p.OrganizationRealPageId);
                            if (propertyToDeleteList != null)
                            {
                                foreach (var propertyToDelete in propertyToDeleteList)
                                {
                                    var propertyInstanceToDelete = new Guid(propertyToDelete.attributes.propertyInstanceSourceId);
                                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, null, new object[] { "DeleteQueuedOrganizations", $"Deleting property instance {propertyInstanceToDelete} from UPFM Company {p.OrganizationRealPageId}." });
                                    _propertyRepository.DeleteUPFMPropertyInstance(propertyInstanceToDelete);
                                    DeletePropertyForOrganization(propertyInstanceToDelete, p.OrganizationRealPageId);
                                }
                            }

                            // post to UDM to remove 
                            var result = _manageBlueBook.DeleteBooksGreenBookCompanyInstance(new CompanyInstance() { CompanyInstanceSourceId = p.OrganizationRealPageId.ToString(), ModifiedBy = "UPFM Delete company" });
                            _organizationRepository.UpdateOrganizationRemovalQueueStatus(p.OrganizationRemovalQueueId, result ? "UDMData Removed" : "UDMData Removal Failed");
                        }

                        // delete activity log data
                        try
                        {
                            if (productInternalSettings.Exists(setting => setting.Name.Equals("ActivityLogUri", StringComparison.OrdinalIgnoreCase)))
                            {
                                var activityDeleteResult = 1;
                                var activityUri = productInternalSettings.First(setting => setting.Name.Equals("ActivityLogUri", StringComparison.OrdinalIgnoreCase)).Value;
                                if (!string.IsNullOrEmpty(activityUri))
                                {
                                    var ulClientToken = _tokenHelper.GetUnifiedLoginServerToken("activityreader companyfunctions");
                                    using (var httpClient = new HttpClient())
                                    {
                                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ulClientToken);
                                        var deleteUri = new Uri(activityUri + $"api/activity/organization/{p.OrganizationPartyId}");
                                        Dictionary<string, object> logData = new Dictionary<string, object>() { { "deleteUri", deleteUri } };
                                        logData.Add("UlClientToken", ulClientToken);
                                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, null, new object[] { "DeleteQueuedOrganizations", $"Posting to ActivityLog.Delete" });
                                        var response = httpClient.DeleteAsync(deleteUri).Result;

                                        if (!response.IsSuccessStatusCode)
                                        {
                                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, null, new object[] { "DeleteQueuedOrganizations", $"Error while executing ActivityLog.Delete StatusCode:{response.StatusCode}, Company {p.OrganizationRealPageId}, OrganizationRemovalQueueId {p.OrganizationRemovalQueueId}" });
                                            activityDeleteResult = 0;
                                        }
                                    }

                                    _organizationRepository.UpdateOrganizationRemovalQueueStatus(p.OrganizationRemovalQueueId, activityDeleteResult == 1 ? "ActivityLog Removed" : "ActivityLog Removal Failed");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, ex, new object[] { "DeleteQueuedOrganizations", $"Error while executing ActivityLog.Delete Company {p.OrganizationRealPageId}, OrganizationRemovalQueueId {p.OrganizationRemovalQueueId}" });
                        }
                        _organizationRepository.UpdateOrganizationRemovalQueueStatus(p.OrganizationRemovalQueueId, "Complete");
                    }
                }
                catch (Exception ex)
                {
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, ex, new object[] { "DeleteQueuedOrganizations", $"Error while deleting company. Company {p.OrganizationRealPageId}, OrganizationRemovalQueueId {p.OrganizationRemovalQueueId}" });
                }
            });
        }

        #endregion

        #region Company
        #region OrganizationList
        public List<CompanySetup> GetCompanyList(string organizationName, int domain, int? blueId, int organizationId, IDictionary<object, object> globals)
        {
            RequestParameter dataFilter = new RequestParameter();
            if (globals.ContainsKey(BaseType.RequestParameter))
            {
                dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
            }
            var company = _organizationRepository.GetCompanyList(organizationName, domain, blueId, organizationId, dataFilter);
            return GetCompanyAdressFromBooks(company);
        }
        #endregion
        public CompanyMaster SearchCompanyDetailsByCustomerCompanyId(long customerCompanyId)
        {
            var companyMaster = new CompanyMaster();
            companyMaster.CompanyDetail = _manageBlueBook.GetBooksCompanyDetailsByCompanyMasterId(customerCompanyId);
            companyMaster.DomainList = _manageBlueBook.GetListOfDomainsByCompany(customerCompanyId);
            var companyInstances = _manageBlueBook.GetCompanyInstancesByCustomerCompanyId(customerCompanyId);

            foreach (var instance in companyInstances)
            {
                var attributes = instance?.attributes;
                if (attributes != null)
                {
                    companyMaster.CompanyInstance.Add(attributes);
                }
            }

            foreach (var domain in companyMaster.DomainList.ToList())
            {
                if (companyMaster.CompanyInstance.Exists(a => a.Domain.Equals(domain.Domain, StringComparison.OrdinalIgnoreCase)))
                {
                    companyMaster.DomainList.Remove(companyMaster.DomainList.First(a => a.Domain.Equals(domain.Domain, StringComparison.OrdinalIgnoreCase)));
                }
            }

            return companyMaster;
        }
        #endregion

        #region GetPropertiesForCompany
        public List<CompanyPropertySetup> GetPropertiesForCompany(Guid companyInstanceId, string propertyName = null, string domain = null,
                        int? blueId = null, int? status = null, IDictionary<object, object> globals = null, long editorPersonaId = 0,
                        long userPersonaId = 0, bool? isSelectedProperties = null, List<Guid> selectedProperties = null, string operatorCode = null, string operatorValue = null)
        {
            RequestParameter dataFilter = new RequestParameter();

            if (globals.ContainsKey(BaseType.RequestParameter))
            {
                dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
            }
            List<Guid> propertyInstanceIds = new List<Guid>();
            List<PropertySetup> propertyDetails = new List<PropertySetup>();
            List<UPFMPropertyInstance> selectedPropertyInstances = new List<UPFMPropertyInstance>();
            List<int> userProperties = null;
            List<BooksPropertyInstance> booksPropertyInstance = null;

            if (string.IsNullOrEmpty(operatorCode) || string.IsNullOrEmpty(operatorValue))
            {
                booksPropertyInstance = GetPropertyInstanceFromBooks(companyInstanceId);
            }
            else
            {
                booksPropertyInstance = GetPropertyInstanceFromBooks(companyInstanceId);
                UPFMProperty uPFMProperty = new UPFMProperty();
                uPFMProperty.id = booksPropertyInstance.Select(a => a.attributes.propertyInstanceSourceId).ToList();
                var aoProperties = _manageProductAssetOptimization.GetPropertiesWithOperators(_defaultUserClaim.PersonaId, userPersonaId, operatorCode, operatorValue);
                var productResult = _manageBlueBook.TranslateProductPrimaryPropertiesData(uPFMProperty, 4, aoProperties);
                var propertyList = productResult.Records.Cast<ProductProperty>().Where(c => c.InstanceId != null).Select(a => a.InstanceId);
                booksPropertyInstance = booksPropertyInstance?.Where(a => propertyList.Contains(a.attributes.propertyInstanceSourceId)).ToList();
            }

            if (domain != null)
            {
                string[] domainFilter = domain.Split(',');
                propertyInstanceIds = booksPropertyInstance?.Where(p => domainFilter.Contains(p.attributes.domain)).Select(p => p.attributes.propertyInstanceSourceId)?.Select(Guid.Parse).ToList();
            }
            else
            {
                propertyInstanceIds = booksPropertyInstance?.Select(p => p.attributes.propertyInstanceSourceId)?.Select(Guid.Parse).ToList();
            }
            if (userPersonaId > 0 || (selectedProperties != null && selectedProperties.Count > 0))
            {
                status = 1; //Hardcoding status to 1, because primary properties tab should only get active properties
                userProperties = _propertyRepository.ListUPFMPropertyInstanceIdByPersona(userPersonaId, ProductEnum.UnifiedPlatform);
                selectedPropertyInstances = _propertyRepository.ListUPFMPropertyInstanceByPersona(userPersonaId, ProductEnum.UnifiedPlatform);
                List<Guid> selectedPropertyInstanceIds = selectedPropertyInstances?.Select(p => p.InstanceId).ToList();
                if ((selectedProperties != null && selectedProperties.Count > 0))
                {
                    selectedPropertyInstanceIds = selectedProperties;
                }
                if (isSelectedProperties == true)
                {
                    propertyInstanceIds = selectedPropertyInstanceIds;
                }
                else if (isSelectedProperties == false)
                {
                    propertyInstanceIds = propertyInstanceIds.Except(selectedPropertyInstanceIds).ToList<Guid>();
                }
            }
            if ((selectedProperties == null || selectedProperties.Count == 0) && isSelectedProperties == true)
            {
                propertyInstanceIds = new List<Guid>();
            }
            if (propertyInstanceIds != null && propertyInstanceIds.Count > 0)
            {
                propertyDetails = _propertyRepository.GetPropertiesForCompany(propertyInstanceIds, propertyName, blueId, status, dataFilter);
                propertyDetails = AddContractedNameToPropertyList(booksPropertyInstance, propertyDetails, userProperties);
            }
            List<CompanyPropertySetup> companyPropertySetup = new List<CompanyPropertySetup>()
            {
                new CompanyPropertySetup()
                {
                    Domain = booksPropertyInstance?.Where(p=>p.attributes.domain != null).Select(p => p.attributes.domain).Distinct().ToList(),
                    Property = propertyDetails,
                    SelectedPropertyIds = selectedPropertyInstances?.Select(p=>p.InstanceId).ToList<Guid>()
                }
            };
            return companyPropertySetup;
        }
        #endregion

        #region Property
        #region GetPropertyForCompany
        public List<UPFMPropertyInstance> GetPropertyByInstanceId(Guid propertyInstanceId)
        {
            List<Guid> propGuidList = new List<Guid>
            {
                propertyInstanceId
            };
            return _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIds(propGuidList);
        }


        /// <summary>
        /// Process Property List.
        /// </summary>
        /// <param name="propertyInstanceIdList"></param>
        /// <param name="companyInstanceId"></param>
        /// <returns></returns>
        public async Task<IRepositoryResponse> ProcessPropertyList(List<UPFMPropertyInstance> propertyInstanceIdList, Guid companyInstanceId)
        {
            var repositoryResponse = new RepositoryResponse();
            var options = new ParallelOptions() {  MaxDegreeOfParallelism = _maxDOPSetting };
            Parallel.ForEach(propertyInstanceIdList, options, (property, cancelToken) =>
            {              
                var currentProperty = GetPropertyByInstanceId(property.InstanceId);
                if (currentProperty != null)
                {
                    repositoryResponse = UpdateProperty(property, companyInstanceId);
                }
            });
            await Task.WhenAll();
            return repositoryResponse;
        }

        #endregion

        #region UpdateProperty
        /// <summary>
        /// Update existing property
        /// </summary>
        /// <param name="property"></param>
        /// <param name="companyInstanceId"></param>
        /// <returns></returns>
        public RepositoryResponse UpdateProperty(UPFMPropertyInstance property, Guid companyInstanceId)
        {
            if (property.InstanceId == Guid.Empty)
            {
                return new RepositoryResponse() { ErrorMessage = "Invalid parameter propertyInstanceId." };
            }
            if (string.IsNullOrEmpty(property.Name))
            {
                return new RepositoryResponse() { ErrorMessage = "Invalid parameter propertyName." };
            }

            var oldProperty = GetPropertyByInstanceId(property.InstanceId)?.FirstOrDefault();
            var _repositoryResponse = _propertyRepository.UpdateProperty(property);

            if (_repositoryResponse.Id > 0)
            {
                var orgName = GetOrganization(companyInstanceId)?.Name;
                var auditData = GetUpdatedPropertyLogActivity(oldProperty, property);
                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} updated the property name for {orgName} company";
                LogAuditActivity(LogActivityTypeConstants.PROPERTY_UPDATED, LogActivityCategoryType.CompanySetup, message, auditData);

                bool booksResponse = UpdatePropertyInBooks(property);
                bool settingsResponse = false;
                if (booksResponse)
                {
                    settingsResponse = UpdatePropertyInSettings(property.InstanceId, companyInstanceId);
                }
                _repositoryResponse = HandleErrorMessage(booksResponse, settingsResponse, "Error while updating property", _repositoryResponse);
            }
            return _repositoryResponse;
        }
        #endregion

        #region AddProperty
        /// <summary>
        /// Add Property For Organization
        /// </summary>
        /// <param name="property">property</param>
        /// <param name="companyInstanceID">companyInstanceID</param>
        /// <returns></returns>
        public RepositoryResponse AddPropertyForOrganization(UPFMPropertyInstance property, Guid companyInstanceID)
        {
            var response = _propertyRepository.InsertUPFMPropertyInstance(property);
            property.InstanceId = response.RealPageId;
            if (response.ErrorMessage.Length == 0)
            {
                var orgName = GetOrganization(companyInstanceID)?.Name;
                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} created a new property for {orgName} company";

                var auditData = new List<AdditionalParameters>()
                {
                    new AdditionalParameters() { Key = "PropertyId", Value = $"{{ \"old\": \"{""}\", \"new\": \"{property.InstanceId}\" }}" },
                    new AdditionalParameters() { Key = "PropertyName", Value = $"{{ \"old\": \"{""}\", \"new\": \"{property.Name}\" }}" },
                    new AdditionalParameters() { Key = "Domain", Value = $"{{ \"old\": \"{""}\", \"new\": \"{property.Domain}\" }}" }
                };
                
                LogAuditActivity(LogActivityTypeConstants.PROPERTY_CREATED, LogActivityCategoryType.CompanySetup, message, auditData);

                bool booksResponse = AddPropertyToBooks(property, companyInstanceID);
                bool settingsResponse = false;
                if (booksResponse)
                {
                    settingsResponse = AddPropertyToUnifiedSettings(property, companyInstanceID);
                }
                response = HandleErrorMessage(booksResponse, settingsResponse, "Error while adding property", response);
            }
            return response;
        }
        #endregion

        #region Search Property By blue Id
        /// <summary>
        /// Search Property Details By CustomerPropertyId(BlueId)
        /// </summary>
        /// <param name="customerPropertyId">customerPropertyId</param>
        /// <param name="booksCustomerMasterId">booksCustomerMasterId</param>
        /// <returns></returns>
        public PropertyInstanceSearch SearchPropertyDetailsByCustomerPropertyId(string customerPropertyId, string booksCustomerMasterId)
        {
            var booksPropertyInstance = _manageBlueBook.GetCustomerPropertyDetails(customerPropertyId);
            if (booksPropertyInstance == null)
            {
                return new PropertyInstanceSearch();
            }
            else if (booksPropertyInstance != null)
            {
                var instanceExists = booksPropertyInstance?.attributes?.customerCompanyId == booksCustomerMasterId;
                if (!instanceExists)
                {
                    return new PropertyInstanceSearch();
                }
            }
            CustomerProperty propertyDetails = _manageBlueBook.GetCustomerPropertyDetails(customerPropertyId);
            List<BooksPropertyInstance> _booksPropertyInstances = _manageBlueBook.GetUPFMPropertyInstancesByCustomerPropertyId(customerPropertyId);
            List<PropertySetup> _listPropertySetup = new List<PropertySetup>();
            if (_booksPropertyInstances != null)
            {
                foreach (var booksProperty in _booksPropertyInstances)
                {
                    PropertySetup _property = new PropertySetup
                    {
                        Name = booksProperty?.attributes.propertyName,
                        Domain = booksProperty?.attributes.domain,
                        IsActive = booksProperty?.attributes.isActive,
                        InstanceId = string.IsNullOrEmpty(booksProperty?.attributes.propertyInstanceSourceId) ? Guid.Empty : Guid.Parse(booksProperty?.attributes.propertyInstanceSourceId),
                        ContractedName = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.propertyName,
                        Address = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Address,
                        City = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.City,
                        State = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.State,
                        Country = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Country,
                        County = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.County,
                        Latitude = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Latitude,
                        Longitude = booksProperty?.attributes?.customerPropertyMap?.FirstOrDefault()?.customerProperty?.FirstOrDefault()?.address?.Longitude
                    };
                    _listPropertySetup.Add(_property);
                }
            }
            //Prepare domain list
            var booksProductsPropertyInstance = GetAllProductsPropertyInstanceFromBooks(customerPropertyId);
            List<string> distinctDomains = booksProductsPropertyInstance?
                    .Where(p => p.attributes.domain != null).Select(p => p.attributes.domain).Distinct().ToList();

            List<string> existingDomains = _listPropertySetup.Where(p => p.Domain != null).Select(p => p.Domain).Distinct().ToList();
            List<string> domainList = existingDomains != null ? distinctDomains?.Except(existingDomains).ToList() : new List<string>();
            PropertyInstanceSearch propertyInstanceSearch = new PropertyInstanceSearch()
            {
                CustomerProperty = propertyDetails,
                PropertyInstance = _listPropertySetup,
                Domain = domainList
            };
            return propertyInstanceSearch;
        }
        /// <summary>
        /// Insert A UPFM property instance
        /// </summary>
        /// <param name="propertyInstance"></param>
        /// <returns></returns>
        public RepositoryResponse InsertUPFMPropertyInstance(UPFMPropertyInstance propertyInstance)
        {
            var response = _propertyRepository.InsertUPFMPropertyInstance(propertyInstance);

            if (response.ErrorMessage.Length == 0)
            {
                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} created a new property";
                var auditData = new List<AdditionalParameters>()
                {
                    new AdditionalParameters() { Key = "PropertyId", Value = $"{{ \"old\": \"{""}\", \"new\": \"{response.RealPageId.ToString()}\" }}" },
                    new AdditionalParameters() { Key = "PropertyName", Value = $"{{ \"old\": \"{""}\", \"new\": \"{propertyInstance.Name}\" }}" },
                    new AdditionalParameters() { Key = "Domain", Value = $"{{ \"old\": \"{""}\", \"new\": \"{propertyInstance.Domain}\" }}" }
                };
                LogAuditActivity(LogActivityTypeConstants.PROPERTY_CREATED, LogActivityCategoryType.CompanySetup, message, auditData);
            }
            return response;
        }
        #endregion

        #region Delete Property
        /// <summary>
        /// Delete Property instance
        /// </summary>
        /// <param name="propertyInstanceID"></param>
        /// <param name="companyInstanceId"></param>
        /// <returns></returns>
        public RepositoryResponse DeletePropertyForOrganization(Guid propertyInstanceID, Guid companyInstanceId)
        {
            var upfmProperties = new UPFMProperty();
            var instanceIds = new List<string>
            {
                propertyInstanceID.ToString().ToLower()
            };
            upfmProperties.id = instanceIds;
            TranslatePropertyInstance translatedData;
            //Hard coding this as SET, because we need to get translated property for settings and
            //we don't have other way to get product code from company setup property delete.
            translatedData = _manageBlueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, "SET");
            var settingPropertyInstance = translatedData.Data?.Attributes
                    ?.Find(p => p.PropertyInstanceSourceId == propertyInstanceID.ToString())
                    ?.TranslatedPropertyInstances?.FirstOrDefault();

            var response = _propertyRepository.DeleteUPFMPropertyInstance(propertyInstanceID);

            if (response.ErrorMessage.Length == 0)
            {
                var orgName = GetOrganization(companyInstanceId)?.Name;
                var propertyName = GetPropertyByInstanceId(propertyInstanceID)?.FirstOrDefault()?.Name;
                var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} deleted the property {propertyName} for {orgName}";
                LogAuditActivity(LogActivityTypeConstants.PROPERTY_DELETED, LogActivityCategoryType.CompanySetup, message);

                bool booksResponse = DeletePropertyFromBooks(propertyInstanceID);
                bool settingsResponse = settingPropertyInstance != null ? false : true;
                if (booksResponse && settingPropertyInstance != null)
                {
                    settingsResponse = DeletePropertyFromUnifiedSetting(settingPropertyInstance.PropertyInstanceSourceId.ToString().ToLower());
                }
                response = HandleErrorMessage(booksResponse, settingsResponse, "Error while deleting property", response);
            }
            return response;
        }
        #endregion

        #endregion


        #region Audit Property data
        public List<PropertyAudit> AuditCompanyProductPropertiesToUPFM(Guid companyInstanceId, int productId)
        {
            var propertyAuditResult = new List<PropertyAudit>();
            var upfmPropertyDetails = new List<PropertySetup>();
            var productResult = new ListResponse();

            var productInternalSettings = _manageProduct.GetProductInternalSettings(productId);

            // get product type
            var producIntegrationType = productInternalSettings.FirstOrDefault(p => p.Name.Equals("productintegrationtype", StringComparison.OrdinalIgnoreCase)).Value;
            if (producIntegrationType.Equals("UPFM"))
            {
                var integration = _integrationTypeFactory.GetIntegration(productId);
                productResult = integration.GetEnterpriseProperties(_defaultUserClaim.PersonaId, new RequestParameter());
            }
            else
            {
                productResult = _manageProductPanel.GetProductProperties(_defaultUserClaim.PersonaId, 0, productId, null);
            }

            if (productResult.Records != null)
            {
                var upfmProperties = new UPFMProperty();
                var instanceIds = new List<string>();
                var instanceGuids = new List<Guid>();
                var booksPropertyInstance = GetPropertyInstanceFromBooks(companyInstanceId);

                if (booksPropertyInstance != null)
                {
                    foreach (var property in booksPropertyInstance)
                    {
                        instanceIds.Add(property.attributes.propertyInstanceSourceId.ToLower());
                        instanceGuids.Add(new Guid(property.attributes.propertyInstanceSourceId));
                    }
                }

                if (instanceGuids.Count > 0)
                {
                    var upfmList = _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIds(instanceGuids);
                    upfmList.ForEach(pd =>
                    {
                        var booksInstance = booksPropertyInstance?.FirstOrDefault(b => b.attributes.propertyInstanceSourceId.Equals(pd.InstanceId.ToString(), StringComparison.OrdinalIgnoreCase));
                        upfmPropertyDetails.Add(new PropertySetup()
                        {
                            Domain = booksInstance?.attributes.domain,
                            ContractedName = booksInstance?.attributes.propertyName,
                            Name = pd.Name,
                            InstanceId = pd.InstanceId
                        });
                        pd.Domain = booksInstance?.attributes.domain;
                    });
                }

                upfmProperties.id = instanceIds;

                var booksProductDetail = _productRepository.GetBooksMasterProductDetail(productId);

                TranslatePropertyInstance translatedData;

                if (booksProductDetail.ProductId != (int)ProductEnum.UnifiedPlatform)
                {
                    if (string.IsNullOrEmpty(booksProductDetail.UDMSourceCode))
                    {
                        translatedData = _manageBlueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, booksProductDetail.BooksProductCode);
                    }
                    else
                    {
                        translatedData = _manageBlueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, booksProductDetail.UDMSourceCode);
                    }
                }
                else
                {
                    translatedData = new TranslatePropertyInstance() { Data = new TranslatePropertyInstanceData() { Attributes = new List<TranslatePropertyInstanceAttribute>() } };

                    if (booksPropertyInstance != null)
                    {
                        foreach (var instance in booksPropertyInstance)
                        {
                            var tpi = new List<TranslatedPropertyInstanceData>()
                            {
                                new TranslatedPropertyInstanceData() {PropertyInstanceSourceId = instance.attributes.propertyInstanceSourceId, Source = instance.attributes.source}
                            };
                            translatedData.Data.Attributes.Add(new TranslatePropertyInstanceAttribute() { PropertyInstanceSourceId = instance.attributes.propertyInstanceSourceId, Source = booksProductDetail.BooksProductCode, TranslatedPropertyInstances = tpi });
                        }
                    }
                }

                var foundProductPropertyIdList = new List<string>();
                if (productResult.Records.Count > 0)
                {
                    var productPropertyType = productResult.Records[0].GetType();

                    if (productPropertyType == typeof(ProductProperty))
                    {
                        var productList = productResult.Records.Cast<ProductProperty>();
                        foreach (var property in productList)
                        {
                            AuditPropertyCompare(property.ID, property.Name, property.InstanceId, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                            foundProductPropertyIdList.Add(property.ID);
                        }
                    }
                    else if (productPropertyType == typeof(ACProperty))
                    {
                        foreach (var property in productResult.Records.Cast<ACProperty>())
                        {
                            AuditPropertyCompare(property.BookID, property.PropertyName, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                            foundProductPropertyIdList.Add(property.BookID);
                        }
                    }
                    else if (productPropertyType == typeof(AssetGroup))
                    {
                        foreach (var property in productResult.Records.Cast<AssetGroup>())
                        {
                            AuditPropertyCompare(property.AssetID, property.Name, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                            foundProductPropertyIdList.Add(property.AssetID);
                        }
                    }
                    else if (productPropertyType == typeof(OnSiteProperty))
                    {
                        foreach (var property in productResult.Records.Cast<OnSiteProperty>())
                        {
                            AuditPropertyCompare(property.GetPropertyId.ToString(), property.GetName, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                            foundProductPropertyIdList.Add(property.GetPropertyId.ToString());
                        }
                    }
                    else if (productPropertyType == typeof(RumPropertyGroup))
                    {
                        foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
                        {
                            AuditPropertyCompare(property.Id.ToString(), property.Name, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                            foundProductPropertyIdList.Add(property.Id.ToString());
                        }
                    }
                    else if (productPropertyType == typeof(ProductProperties))
                    {
                        foreach (var property in productResult.Records.Cast<ProductProperties>())
                        {
                            AuditPropertyCompare(property.GetPropertyId.ToString(), property.GetName, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                            foundProductPropertyIdList.Add(property.GetPropertyId.ToString());
                        }
                    }
                    else if (productPropertyType == typeof(Portfolio))
                    {
                        foreach (var property in productResult.Records.Cast<Portfolio>())
                        {
                            AuditPropertyCompare(property.ID, property.Name, null, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                            foundProductPropertyIdList.Add(property.ID);
                        }
                    }
                }

                // add properties that were returned from UDM for the product but the product didn't give us, out of sync data?
                translatedData?.Data?.Attributes?.ForEach(udmProperty =>
                {
                    bool foundProperty = false;
                    udmProperty.TranslatedPropertyInstances.ForEach(instances =>
                    {
                        if (foundProductPropertyIdList.Exists(p => p.Equals(instances.PropertyInstanceSourceId, StringComparison.OrdinalIgnoreCase)))
                        {
                            foundProperty = true;
                        }
                    });
                });
                propertyAuditResult = propertyAuditResult.OrderBy(p => p.Name).ThenBy(p => p.ContractedName).ToList();
            }

            return propertyAuditResult;
        }

        private static void AuditPropertyCompare(string propertyId, string propertyName, string instanceId, TranslatePropertyInstance translatedData, List<string> instanceids, List<PropertySetup> upfmPropertyDetails, List<PropertyAudit> propertyAuditResult)
        {
            PropertyAudit pa = new PropertyAudit()
            {
                Name = propertyName,
                ProductInstanceId = propertyId,
            };

            if (instanceId == null)
            {
                var instanceExists = translatedData.Data?.Attributes.Find(p => p.TranslatedPropertyInstances.Exists(o => o.PropertyInstanceSourceId == propertyId));
                if (instanceExists != null)
                {
                    pa.UPFMInstanceId = instanceExists.PropertyInstanceSourceId;
                    pa.Status = instanceids.TrueForAll(p => p != instanceExists.PropertyInstanceSourceId) ? "No ID" : "OK"; // Missing UPFM Instances

                    var upfmProperty = upfmPropertyDetails.Find(p => p.InstanceId == new Guid(instanceExists.PropertyInstanceSourceId));
                    if (upfmProperty != null)
                    {
                        pa.UPFMName = upfmProperty.Name;
                        pa.Domain = upfmProperty.Domain;
                        pa.ContractedName = upfmProperty.ContractedName;
                    }
                }

                if (translatedData.Data == null)
                {
                    pa.Status = "No ID"; // ""No product data translated";
                }
            }
            else
            {
                var propertyGuid = Guid.Empty;
                var instanceIdGuid = Guid.Empty;
                Guid.TryParse(propertyId, out propertyGuid);
                Guid.TryParse(instanceId, out instanceIdGuid);
                var upfmProperty = upfmPropertyDetails.Find(p => p.InstanceId == instanceIdGuid || p.InstanceId == propertyGuid);
                if (upfmProperty != null)
                {
                    pa.Status = "OK";
                    pa.UPFMName = upfmProperty.Name;
                    pa.Domain = upfmProperty.Domain;
                    pa.ContractedName = upfmProperty.ContractedName;
                    pa.UPFMInstanceId = upfmProperty.InstanceId.ToString();
                }
            }

            if (!propertyAuditResult.Exists(p => (pa.ProductInstanceId != null && p.ProductInstanceId != null && p.ProductInstanceId.Equals(pa.ProductInstanceId, StringComparison.OrdinalIgnoreCase)) &&
                                             (pa.ContractedName != null && p.ContractedName != null && p.ContractedName.Equals(pa.ContractedName, StringComparison.OrdinalIgnoreCase)) &&
                                             (pa.Domain != null && p.Domain != null && p.Domain.Equals(pa.Domain, StringComparison.OrdinalIgnoreCase)) &&
                                             (pa.Name != null && p.Name != null && p.Name.Equals(pa.Name, StringComparison.OrdinalIgnoreCase)) &&
                                             (pa.UPFMInstanceId != null && p.UPFMInstanceId != null && p.UPFMInstanceId.Equals(pa.UPFMInstanceId, StringComparison.OrdinalIgnoreCase)) &&
                                             (pa.UPFMName != null && p.UPFMName != null && p.UPFMName.Equals(pa.UPFMName, StringComparison.OrdinalIgnoreCase))))
            {
                propertyAuditResult.Add(pa);
            }
        }

        #endregion

        #region Get Product status details
        /// <summary>
        /// Product status details
        /// </summary>
        /// <param name="propertyInstanceSourceId">productInstanceId</param>
        /// <param name="source">source</param>
        /// <returns></returns>
        public ProductPropertyDetails GetSourceProductDetails(string propertyInstanceSourceId, string source)
        {
            var booksProductSource = _manageBlueBook.GetPropertyDetailsByPropertyInstanceIdAndSource(propertyInstanceSourceId, source);
            if (booksProductSource != null && booksProductSource.attributes != null)
            {
                var customerProp = booksProductSource.attributes?.customerPropertyMap?
                                        .Where(p => p.propertyInstanceSourceId == booksProductSource.attributes.propertyInstanceSourceId)?.FirstOrDefault();
                ProductStatusDetail productStatus = new ProductStatusDetail
                {
                    IsActive = booksProductSource.attributes.isActive,
                    ProductInstanceId = booksProductSource.attributes.propertyInstanceSourceId,
                    Domain = booksProductSource.attributes.domain,
                    CustomerPropertyId = customerProp?.customerPropertyId.ToString(),
                    ContractedName = customerProp?.customerProperty?.FirstOrDefault().propertyName.ToString()
                };
                List<PropertySetup> _listPropertySetup = new List<PropertySetup>();
                if (customerProp != null)
                {
                    List<BooksPropertyInstance> _booksPropertyInstances = _manageBlueBook.GetUPFMPropertyInstancesByCustomerPropertyId(customerProp?.customerPropertyId.ToString());
                    if (_booksPropertyInstances != null)
                    {
                        foreach (var booksProperty in _booksPropertyInstances)
                        {
                            PropertySetup _property = new PropertySetup
                            {
                                Name = booksProperty?.attributes.propertyName,
                                Domain = booksProperty?.attributes.domain,
                                IsActive = booksProperty?.attributes.isActive,
                                InstanceId = string.IsNullOrEmpty(booksProperty?.attributes.propertyInstanceSourceId) ? Guid.Empty : Guid.Parse(booksProperty?.attributes.propertyInstanceSourceId),
                            };
                            _listPropertySetup.Add(_property);
                        }
                    }
                }
                return new ProductPropertyDetails
                {
                    ProductStatusDetail = productStatus,
                    PropertyDetails = _listPropertySetup
                };
            }
            return new ProductPropertyDetails();
        }
        #endregion


        public bool AddUpdateCompanyToUnifiedSettings(string companyInstanceID, string trasactionType, string customerEnvironment = null)
        {
            UnifiedSettingCompanyPropertyPayload payload = new UnifiedSettingCompanyPropertyPayload
            {
                Payload = new UnifiedSettingCompanyProperty
                {
                    Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                    Company = new UnifiedSettingCompanyInstance
                    {
                        CompanyInstanceSourceId = companyInstanceID.ToString().ToLower()
                    },
                    Properties = new List<UnifiedSettingCompanyPropertyInstance>(),
                    CustomerEnvironment = customerEnvironment
                }
            };
            return _manageUnifiedSettings.CreateUpdateCompanyInSetting(payload, trasactionType.ToLower() == "create" ? HttpMethod.Post : HttpMethod.Put);
        }

        #region Private Methods
        private static List<AdditionalParameters> GetUpdatedOrganizationLogActivity(Organization oldOrganization, Organization newOrganiztion, List<OrganizationType> orgTypes)
        {

            // new table
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            //Is company name being updates
            if (string.Compare(oldOrganization.Name, newOrganiztion.Name, StringComparison.OrdinalIgnoreCase) != 0)
            {
                additionalParameters.Add(new AdditionalParameters() { Key = "CompanyName", Value = $"{{ \"old\": \"{oldOrganization.Name}\", \"new\": \"{newOrganiztion.Name}\" }}" });
            }
            //Is company type being updated
            if (oldOrganization.OrganizationTypeId != newOrganiztion.OrganizationTypeId)
            {
                var oldOrgType = orgTypes.Find(t => t.OrganizationTypeId == oldOrganization.OrganizationTypeId).Name;
                var newOrgType = orgTypes.Find(t => t.OrganizationTypeId == newOrganiztion.OrganizationTypeId).Name;
                additionalParameters.Add(new AdditionalParameters() { Key = "OrganizationType", Value = $"{{ \"old\": \"{oldOrgType}\", \"new\": \"{newOrgType}\" }}" });
            }
            //Is company status being updated
            if (oldOrganization.IsActive != newOrganiztion.IsActive)
            {
                string newStatus;
                string prevStatus;

                if (oldOrganization.IsActive == 1)
                {
                    newStatus = "Inactive";
                    prevStatus = "Active";
                }
                else
                {
                    newStatus = "Active";
                    prevStatus = "Inactive";
                }

                additionalParameters.Add(new AdditionalParameters() { Key = "Status", Value = $"{{ \"old\": \"{prevStatus}\", \"new\": \"{newStatus}\" }}" });
            }
            //Created with RealPageId
            if (oldOrganization.RealPageId != newOrganiztion.RealPageId)
            {
                additionalParameters.Add(new AdditionalParameters() { Key = "RealPageId", Value = $"{{ \"old\": \"{oldOrganization.RealPageId}\", \"new\": \"{newOrganiztion.RealPageId}\" }}" });
            }
            //Is Domain changed
            if (oldOrganization.OrganizationDomain.Name != newOrganiztion.OrganizationDomain.Name)
            {
                additionalParameters.Add(new AdditionalParameters() { Key = "Domain", Value = $"{{ \"old\": \"{oldOrganization.OrganizationDomain.Name}\", \"new\": \"{newOrganiztion.OrganizationDomain.Name}\" }}" });
            }
            //Is Primary Properties changed
            if (oldOrganization.EnablePrimaryProperties != newOrganiztion.EnablePrimaryProperties)
            {
                string newStatus;
                string prevStatus;

                if (oldOrganization.EnablePrimaryProperties == 1)
                {
                    newStatus = "Inactive";
                    prevStatus = "Active";
                }
                else
                {
                    newStatus = "Active";
                    prevStatus = "Inactive";
                }

                additionalParameters.Add(new AdditionalParameters() { Key = "PrimaryProperties", Value = $"{{ \"old\": \"{prevStatus}\", \"new\": \"{newStatus}\" }}" });
            }
            //Is Enterprise Roles changed
            if (oldOrganization.EnableEnterpriseRoles != newOrganiztion.EnableEnterpriseRoles)
            {
                string newStatus;
                string prevStatus;

                if (oldOrganization.EnablePrimaryProperties == 1)
                {
                    newStatus = "Inactive";
                    prevStatus = "Active";
                }
                else
                {
                    newStatus = "Active";
                    prevStatus = "Inactive";
                }
                additionalParameters.Add(new AdditionalParameters() { Key = "EnterpriseRoles", Value = $"{{ \"old\": \"{prevStatus}\", \"new\": \"{newStatus}\" }}" });
            }

            return additionalParameters;
        }

        private static List<AdditionalParameters> GetUpdatedPropertyLogActivity(UPFMPropertyInstance oldProperty, UPFMPropertyInstance newProperty)
        {
            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();
            //Is property name being updated
            if (string.Compare(oldProperty.Name, newProperty.Name, StringComparison.OrdinalIgnoreCase) != 0)
            {
                additionalParameters.Add(new AdditionalParameters() { Key = "Name", Value = $"{{ \"old\": \"{oldProperty.Name}\", \"new\": \"{newProperty.Name}\" }}" });
            }
            //Is property address being updated
            var oldAddress = $"{oldProperty.Address}, {oldProperty.City}, {oldProperty.County}, {oldProperty.State}, {oldProperty.Country}, {oldProperty.PostalCode}";
            var newAddress = $"{newProperty.Address}, {newProperty.City}, {newProperty.County}, {newProperty.State}, {newProperty.Country}, {newProperty.PostalCode}";

            if (string.Compare(oldAddress, newAddress, StringComparison.OrdinalIgnoreCase) != 0)
            {
                additionalParameters.Add(new AdditionalParameters() { Key = "Address", Value = $"{{ \"old\": \"{oldAddress}\", \"new\": \"{newAddress}\" }}" });
            }

            //Is property status being updated
            if (oldProperty.IsActive != newProperty.IsActive)
            {
                string newStatus = "";
                string prevStatus = "";

                if (oldProperty.IsActive)
                {
                    newStatus = "Inactive";
                    prevStatus = "Active";
                }
                else
                {
                    newStatus = "Active";
                    prevStatus = "Inactive";
                }
                additionalParameters.Add(new AdditionalParameters() { Key = newProperty.Name , Value = $"{{ \"old\": \"{prevStatus}\", \"new\": \"{newStatus}\" }}" });
            }

            return additionalParameters;
        }

        private List<CompanySetup> GetCompanyAdressFromBooks(List<CompanySetup> companyDetails)
        {
            List<UnifiedLoginCompany> compList = new List<UnifiedLoginCompany>();
            List<string> companyInstanceList = new List<string>();
            foreach (var item in companyDetails)
            {
                companyInstanceList.Add(item.RealPageId.ToString().ToLower());
                compList.Add(new UnifiedLoginCompany
                {
                    CompanyId = long.Parse(item.BooksMasterId.ToString()),
                    BooksCustomerMasterId = long.Parse(item.BooksCustomerMasterId.ToString() == string.Empty ? "0" : item.BooksCustomerMasterId.ToString())
                });
            }
            IList<Company> booksCompanyDetails = _manageBlueBook.GetCompanyListByCompIds(compList);
            IList<CustomerCompanyInstance> booksCompanyInstanceDetails = _manageBlueBook.GetUPFMCompanyDetailsByInstanceIds(companyInstanceList);
            foreach (var items in companyDetails)
            {
                var address = booksCompanyInstanceDetails.Where(add => add.attributes.companyInstanceSourceId == items.RealPageId.ToString()).FirstOrDefault()?.attributes.CompanyInstanceLocation;
                items.ContractedName = booksCompanyDetails.Where(add => add.Id == items.BooksCustomerMasterId).FirstOrDefault()?.CompanyName;
                if (address != null && address.Count > 0)
                {
                    items.CompanyLocation = address[0];
                    items.Address = address[0]?.Address + "," + address[0]?.City + "," + address[0]?.State + "," + address[0]?.PostalCode;
                }
            }
            foreach (var item in companyDetails)
            {

                List<IDPNames> IDPList = _organizationRepository.GetCompanyIDPList(item.OrganizationPartyId);
                IList<IdentityProviderType> companyIDPS = _organizationRepository.GetOrganizationIdentityProviderType(item.RealPageId);
                if (item.ThirdPartyIdps == null)
                {
                    item.ThirdPartyIdps = new List<ThirdPartyIDPs>();
                }
                foreach (var IDP in IDPList)
                {
                    if (IDP.IDPName.Equals("IdentityServer", StringComparison.OrdinalIgnoreCase))
                    {
                        IDP.IDPName = "None";
                    }

                    if (companyIDPS.Any(p => p.ContactMechanismId == IDP.ContactMechanismId))
                    {
                        ThirdPartyIDPs idp = new ThirdPartyIDPs { IDPName = IDP.IDPName, IsAssigned = true };
                        item.ThirdPartyIdps.Add(idp);
                    }
                    else
                    {
                        ThirdPartyIDPs idp = new ThirdPartyIDPs { IDPName = IDP.IDPName, IsAssigned = false };
                        item.ThirdPartyIdps.Add(idp);
                    }
                }
                if(companyIDPS.Count > 1)
                {
                    var idp = item.ThirdPartyIdps.FirstOrDefault(i => i.IDPName.Equals("None", StringComparison.OrdinalIgnoreCase));
                    if (idp != null)
                    {
                        idp.IsAssigned = false;
                    }
                }
            }

            return companyDetails;
        }

        private List<BooksPropertyInstance> GetPropertyInstanceFromBooks(Guid companyInstanceId)
        {
            return _manageBlueBook.GetPropertyInstanceForCompany(companyInstanceId);
        }

        private List<BooksPropertyInstance> GetAllProductsPropertyInstanceFromBooks(string customerPropertyId)
        {
            return _manageBlueBook.GetAllProductsPropertyInstanceFromBooks(customerPropertyId);
        }

        private List<PropertySetup> AddContractedNameToPropertyList(List<BooksPropertyInstance> booksPropertyInstance, List<PropertySetup> propertySetup, List<int> userProperties)
        {
            foreach (var property in propertySetup)
            {
                property.ContractedName = booksPropertyInstance?
                                        .Find(pi => pi.attributes.propertyInstanceSourceId.ToString() == property.InstanceId.ToString())?
                                        .attributes.customerPropertyMap?.FirstOrDefault()?.customerProperty.FirstOrDefault()?.propertyName;
                property.Domain = booksPropertyInstance?
                                        .Find(pi => pi.attributes.propertyInstanceSourceId.ToString() == property.InstanceId.ToString())?
                                        .attributes.domain;

                var propertyAddress = booksPropertyInstance?
                                        .Find(pi => pi.attributes.propertyInstanceSourceId.ToString() == property.InstanceId.ToString())?
                                        .attributes.address;

                property.Address = propertyAddress?.Address;
                property.City = propertyAddress?.City;
                property.State = propertyAddress?.State;
                property.PostalCode = propertyAddress?.PostalCode;
                property.Country = propertyAddress?.Country;
                property.County = propertyAddress?.County;
                property.PropertyAddress = propertyAddress?.Address + "," + propertyAddress?.City + "," + propertyAddress?.State + "," + propertyAddress?.PostalCode;
                if (userProperties != null && userProperties.Count > 0 && userProperties.Contains(property.PropertyInstanceId))
                {
                    property.IsAssigned = true;
                }
            }
            return propertySetup;
        }

        private bool AddPropertyToBooks(UPFMPropertyInstance property, Guid companyInstanceID)
        {
            // insert to books
            PropertyInstance pi = new PropertyInstance()
            {
                PropertyName = property.Name,
                CompanyInstanceSourceId = companyInstanceID.ToString().ToLower(),
                PropertyInstanceSourceId = property.InstanceId.ToString(),
                CustomerEnvironment = property.Domain,
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                IsActive = true,
                Address = new InstanceAddress()
                {
                    Address = property.Address,
                    City = property.City,
                    State = property.State,
                    PostalCode = property.PostalCode,
                    County = property.County,
                    Country = property.Country,
                },
                ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Property Creation"
            };
            if (!string.IsNullOrEmpty(property.PropertyInstancePartner) && !string.IsNullOrEmpty(property.PropertyInstancePartnerSourceId))
            {
                pi.PropertyInstancePartners = new List<PropertyInstancePartner>() { new PropertyInstancePartner() { TargetSource = property.PropertyInstancePartner, TargetPropertyInstanceSourceId = property.PropertyInstancePartnerSourceId } };
            }
            return _manageBlueBook.AddBooksGreenBookPropertyInstanceFromProvisioning(pi);
        }

        private bool UpdatePropertyInBooks(UPFMPropertyInstance property)
        {
            PropertyInstanceAck ack = new PropertyInstanceAck
            {
                PropertyInstanceSourceId = property.InstanceId.ToString(),
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                PropertyName = property.Name,
                IsActive = property.IsActive,
                Address = new PropertyInstanceAddress()
                {
                    Address = property.Address,
                    City = property.City,
                    State = property.State,
                    PostalCode = property.PostalCode,
                    County = property.County,
                    Country = property.Country,
                },
                ModifiedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)
            };
            return _manageBlueBook.AcknowledgePropertyUpdate(ack);
        }

        private bool DeletePropertyFromBooks(Guid propertyInstanceID)
        {
            return _manageBlueBook.DeletePropertyFromBooks(propertyInstanceID);
        }

        private bool AddPropertyToUnifiedSettings(UPFMPropertyInstance property, Guid companyInstanceID)
        {
            UnifiedSettingCompanyPropertyPayload payload = PreparePropertyObjectToUnifiedSetting(property, companyInstanceID);
            return _manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Post);
        }

        private bool UpdatePropertyInSettings(Guid propertyInstanceId, Guid companyInstanceID)
        {
            List<Guid> propGuidList = new List<Guid>
            {
                propertyInstanceId
            };
            UPFMPropertyInstance _propertyInstance = _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIds(propGuidList).FirstOrDefault();
            UnifiedSettingCompanyPropertyPayload payload = PreparePropertyObjectToUnifiedSetting(_propertyInstance, companyInstanceID);
            return _manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Put);
        }
        private bool DeletePropertyFromUnifiedSetting(string settingsPropertyInstanceID)
        {
            return _manageUnifiedSettings.DeletePropertyInSetting(settingsPropertyInstanceID);
        }

        private UnifiedSettingCompanyPropertyPayload PreparePropertyObjectToUnifiedSetting(UPFMPropertyInstance property, Guid companyInstanceID)
        {
            UnifiedSettingCompanyProperty usp = new UnifiedSettingCompanyProperty
            {
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                Company = new UnifiedSettingCompanyInstance
                {
                    CompanyInstanceSourceId = companyInstanceID.ToString().ToLower()
                },
                Properties = new List<UnifiedSettingCompanyPropertyInstance>()
                {
                    new UnifiedSettingCompanyPropertyInstance()
                        {
                            PropertyName = property.Name,
                            PropertyInstanceSourceId = property.InstanceId,
                            CustomerPropertyId = !string.IsNullOrEmpty(property.CustomerPropertyId) ? property.CustomerPropertyId : "0",
                            IsActive = property.IsActive,
                            Address = property.Address,
                            City = property.City,
                            State = property.State,
                            PostalCode = property.PostalCode,
                            County = property.County,
                            Country = property.Country
                    }
                },
                CustomerEnvironment = property.Domain
            };
            return new UnifiedSettingCompanyPropertyPayload
            {
                Payload = usp
            };
        }

        private RepositoryResponse HandleErrorMessage(bool booksReponse, bool settingsResponse, string errorMessage, RepositoryResponse response)
        {
            if (booksReponse && settingsResponse)
            {
                return response;
            }
            else if (!booksReponse && !settingsResponse)
            {
                response.ErrorMessage = errorMessage + " from Books and Settings";
            }
            else
            {
                if (!booksReponse)
                {
                    response.ErrorMessage = errorMessage + " from Books";
                }
                if (!settingsResponse)
                {
                    response.ErrorMessage = errorMessage + " from Settings";
                }
            }
            return response;
        }

        private void CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(long partyId, string mappingName, int value)
        {

            //Add or update primary properties or enterprise role setting
            MasterConfigurationSetting masterConfigurationSetting = new MasterConfigurationSetting
            {
                PartyId = partyId.ToString(),
                MappingName = mappingName,
                Value = value.ToString(),
                CreatedBy = _defaultUserClaim.UserId
            };

            _configurationSettingRepository.CreatePrimaryPropertyEnterpriseRoleMasterConfigurationSetting(masterConfigurationSetting);
        }

        private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType, string message, List<AdditionalParameters> additionalParameters = null)
        {
            try
            {
                LogActivity.WriteActivity(new ActivityDetails
                {
                    LogActivityTypeName = logActivityType,
                    LogCategoryName = logActivityCategoryType.ToString(),
                    CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _defaultUserClaim.OrganizationMasterId,
                    OrganizationPartyId = _defaultUserClaim.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = _defaultUserClaim.LoginName,
                    FromUserLoginId = _defaultUserClaim.UserId,
                    FromUserRealpageId = _defaultUserClaim.UserRealPageGuid.ToString(),
                    FromUserFirstName = _defaultUserClaim.FirstName,
                    FromUserLastName = _defaultUserClaim.LastName,

                    ToUserLoginName = null,
                    ToUserLoginId = null,
                    ToUserFirstName = null,
                    ToUserLastName = null,
                    ToUserRealpageId = null,
                    AdditionalInformation = additionalParameters
                });
            }
            catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, ex, new object[] { "LogAuditActivity", $"Error while adding activity message. BooksMasterOrganizationId {_defaultUserClaim.OrganizationName}, author user login name {_defaultUserClaim.LoginName}" });
            }
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
            try
            {
                string correlationId = "";
                if (_defaultUserClaim != null)
                {
                    correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";
                }
                var logger = Log.Logger;
                if (logData?.Keys != null)
                {
                    logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
                }
                logger = logger.ForContext("ProductModule", this.GetType());
                logger = logger.ForContext("CorrelationId", correlationId);

                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
            }
            catch
            {
                /*ignored*/
            }
        }
        #endregion
    }
}