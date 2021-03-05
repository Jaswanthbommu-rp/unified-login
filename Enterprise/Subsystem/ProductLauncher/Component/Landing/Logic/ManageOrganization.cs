using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using PropertySetup = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.PropertySetup;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Helpers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    /// <summary>
    /// Manage Organization repository calls
    /// </summary>
    public class ManageOrganization : IManageOrganization
    {
        #region Private Variables
        private IOrganizationRepository _organizationRepository;
        private ICredentialRepository _credentialRepository;
        private IUserLoginRepository _userLoginRepository;
        private IPersonaRepository _personaRepository;
        private IOrganizationProductRepository _organizationProductRepository;
        private IProductInternalSettingRepository _productInternalSettingRepository;
        private IProductRepository _productRepository;
        private IPropertyRepository _propertyRepository;
        private IManageBlueBook _manageBlueBook;
        private IManageProductPanel _manageProductPanel;
        private IManageUnifiedSettings _manageUnifiedSettings;       

        private DefaultUserClaim _defaultUserClaim;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Unit Test Constructor
        /// </summary>
        public ManageOrganization(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _credentialRepository = new CredentialRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _personaRepository = new PersonaRepository(repository);
            _organizationProductRepository = new OrganizationProductRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _productRepository = new ProductRepository(repository);
            _defaultUserClaim = userClaim;
            _manageBlueBook = new ManageBlueBook(_defaultUserClaim, _productInternalSettingRepository, messageHandler);
            _manageProductPanel = new ManageProductPanel(_defaultUserClaim, repository, _manageBlueBook, messageHandler, null);
            _propertyRepository = new PropertyRepository(repository);
            _manageUnifiedSettings = new ManageUnifiedSettings(repository,userClaim, messageHandler);
        }

        /// <summary>
        /// Audit Unit Test Constructor
        /// </summary>
        public ManageOrganization(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler, IManageProductOneSite manageProductOneSite)
        {
            _organizationRepository = new OrganizationRepository(repository);
            _credentialRepository = new CredentialRepository(repository);
            _userLoginRepository = new UserLoginRepository(repository);
            _personaRepository = new PersonaRepository(repository);
            _organizationProductRepository = new OrganizationProductRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _productRepository = new ProductRepository(repository);
            _defaultUserClaim = userClaim;
            _manageBlueBook = new ManageBlueBook(_defaultUserClaim, _productInternalSettingRepository, messageHandler);
            _manageProductPanel = new ManageProductPanel(_defaultUserClaim, repository, _manageBlueBook,messageHandler, manageProductOneSite);
            _propertyRepository = new PropertyRepository(repository);
            _manageUnifiedSettings = new ManageUnifiedSettings(repository, userClaim, messageHandler);
        }

        /// <summary>
        /// Create a basic instance of the ManageOrganization Controller class
        /// </summary>
        public ManageOrganization(DefaultUserClaim userClaim)
        {
            _organizationRepository = new OrganizationRepository();
            _credentialRepository = new CredentialRepository();
            _userLoginRepository = new UserLoginRepository();
            _personaRepository = new PersonaRepository();
            _organizationProductRepository = new OrganizationProductRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _productRepository = new ProductRepository();
            _propertyRepository = new PropertyRepository();
            _manageBlueBook = new ManageBlueBook(userClaim);
            _manageProductPanel = new ManageProductPanel(userClaim);
            _defaultUserClaim = userClaim;
            _manageUnifiedSettings = new ManageUnifiedSettings(userClaim);
        }

        #endregion

        #region Public Organization methods

        public ObjectOutput<OrganizationCreateResult, IErrorData> CreateOrganization(OrganizationCreate organization, bool processBlueBookMessage = false)
        {
            var repositoryResponse = new RepositoryResponse();
            var outputResult = new ObjectOutput<OrganizationCreateResult, IErrorData>() {Status = new Status<IErrorData>() {Success = false}};

            if (organization.BooksCompanyId == organization.BooksCustomerMasterId)
            {
                outputResult.Status.ErrorMsg = "Duplicate master ids";
                return outputResult;
			}

			if (organization.OrganizationTypeId == 0)
			{
                outputResult.Status.ErrorMsg = $"An invalid Organization Type id was given: {organization.OrganizationTypeId}";
                return outputResult;
            }

			List<ProductEnum> addProductList = new List<ProductEnum>();
            // verify the products, if any, exist and can be added to the customer
            List<string> invalidProductList = ParseProduct(organization.Products, addProductList);

            if (invalidProductList.Count > 0)
            {
                outputResult.Status.ErrorMsg = "An invalid product was given : " + String.Join(",", invalidProductList);
                return outputResult;
            }

            OrganizationAdminUser aUser = organization.AdminUser;
            if (aUser == null)
            {
                outputResult.Status.ErrorMsg = "No admin user information provided";
                return outputResult;
            }

            if (processBlueBookMessage)
            {
                IList<Organization> organizationList = GetOrganizationList();
                if (organizationList.Any(c => c.Name.Equals(organization.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    outputResult.Status.ErrorMsg = $"MessageHandler.Handle - Company: {organization.Name} with BlueBookId: {organization.BooksCustomerMasterId} already exists!";
                    return outputResult;
                }

                // TODO update for domain?
                if (organizationList.Any(c => c.BooksCustomerMasterId == organization.BooksCustomerMasterId))
                {
                    outputResult.Status.ErrorMsg = $"MessageHandler.Handle - Bluebook customer master id {organization.BooksCustomerMasterId} already in use!";
                    return outputResult;
                }
            }

            // see if the given email already exists and reject it if it is found
            UserLoginOnly findExistingUser = _userLoginRepository.GetUserLoginOnly(aUser.Email);
            if (findExistingUser != null)
            {
                outputResult.Status.ErrorMsg = "Admin email already exists";
                return outputResult;
            }

            // create the organization
            Organization org = new Organization()
            {
                Name = organization.Name,
                BooksMasterId = organization.BooksCompanyId,
                BooksCustomerMasterId = organization.BooksCustomerMasterId,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = organization.OrganizationTypeId
                },
				OrganizationDomain = new OrganizationDomain()
                {
					OrganizationDomainId = organization.OrganizationDomainId
                }
            };
            repositoryResponse = InsertOrganization(org);

            if (!string.IsNullOrEmpty(repositoryResponse.ErrorMessage))
            {
                outputResult.Status.ErrorMsg = repositoryResponse.ErrorMessage;
                return outputResult;
            }

            Guid organizationRealPageId = repositoryResponse.RealPageId;

            org = GetOrganization(organizationRealPageId);

            //check the guid


            // add the given products to the new company
            var productResponse = AddProductsToOrganization(addProductList, org.PartyId, organization.OrganizationTypeId);
            if (!string.IsNullOrEmpty(productResponse.ErrorMessage))
            {
                outputResult.Status.ErrorMsg = productResponse.ErrorMessage;
                return outputResult;
            }
            // add the first time super user to the new org

            try
            {
                CreateInitialOrgSuperUser(org.PartyId, aUser.FirstName, "", aUser.LastName, aUser.Title, aUser.Suffix, aUser.Email, true, null, organizationRealPageId);
            }
            catch (Exception ex)
            {
            }

            var userLogin = _userLoginRepository.GetUserLoginOnly(aUser.Email);
            long personaId = _personaRepository.GetActivePersonaId(userLogin.RealPageId);

            OrganizationCreateResult createOrg = new OrganizationCreateResult()
            {
                Org = org,
                adminLogin = aUser.Email,
                //adminPersonaId = personaId,
                BooksCompanyId = org.BooksMasterId,
                BooksCustomerMasterId = org.BooksCustomerMasterId
            };

            //Create an additional admin user for the Company
            if ((processBlueBookMessage) && (organization.CompanyAdminUser != null) && (!string.IsNullOrWhiteSpace(organization.CompanyAdminUser.Email)))
            {
                findExistingUser = _userLoginRepository.GetUserLoginOnly(organization.CompanyAdminUser.Email);

                ManageUser manageUser = new ManageUser(_defaultUserClaim);
                IList<Persona> personaList = new List<Persona>();
                ProfileDetail profileDetail = new ProfileDetail()
                {
                    FirstName = organization.CompanyAdminUser.FirstName,
                    LastName = organization.CompanyAdminUser.LastName,
                    MiddleName = string.Empty,
                    NotificationEmail = string.Empty,
                    Password = string.Empty,
                    Persona = new List<Persona>(),
                    CreateUserSourceType = CreateUserSourceType.UnifiedPlatform,
                    TelecommunicationNumber = new List<TelecommunicationNumber>(),
                    CustomFields = new List<CustomFieldValue>(),
                    UserTypeId = (int) UserRoleType.SuperUser,
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
                    productBatch = null
                };
                if (findExistingUser != null)
                {
                    ManagePerson managePerson = new ManagePerson();
                    var existingPerson = managePerson.GetPerson(findExistingUser.RealPageId);
                    if (existingPerson != null)
                    {
                        profileDetail.FirstName = existingPerson.FirstName;
                        profileDetail.LastName = existingPerson.LastName;
                        profileDetail.UserTypeId = (int) UserRoleType.ExternalUser;
                        UnifiedLoginRepository umr = new UnifiedLoginRepository();
                        List<int> _productIdList = new List<int>() {(int) ProductEnum.UnifiedPlatform};
                        var gbAllRoles = umr.ListRolesForProductsByPartyId(org.PartyId, (int) ProductEnum.UnifiedPlatform, _productIdList);
                        if (gbAllRoles.Any(p => p.Roletype.Equals("System", StringComparison.OrdinalIgnoreCase) && p.Name.Equals("User Administrator", StringComparison.OrdinalIgnoreCase)))
                        {
                            string roleId = gbAllRoles?.FirstOrDefault(p => p.Roletype.Equals("System", StringComparison.OrdinalIgnoreCase) && p.Name.Equals("User Administrator", StringComparison.OrdinalIgnoreCase)).ID;
                            if (!string.IsNullOrEmpty(roleId))
                            {
                                ProductBatch pb = new ProductBatch
                                {
                                    ProductId = (int) ProductEnum.UnifiedPlatform,
                                    InputJson = new RolePropertyList
                                    {
                                        RoleList = new List<string>() {roleId}
                                    }
                                };
                                profileDetail.productBatch = new List<ProductBatch>() {pb};
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
                        Name = profileDetail.UserTypeId == (int) UserRoleType.SuperUser ? "System Administrator" : "Primary",
                        PersonaEnvironmentTypeId = (int) personaEnviornment.PersonaEnvironmentTypeId,
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

                CreateUserResponse<IErrorData> errorDataResponse = manageUser.CreateUser(profileDetail, personaList);
                if (!errorDataResponse.Status.Success)
                {
                    outputResult.Status.Success = true;
                    outputResult.Status.ErrorMsg = $"{profileDetail.userLogin.LoginName}: " + errorDataResponse.Status.ErrorMsg;
                    return outputResult;
                }

                IManagePartyRole managePartyRole = new ManagePartyRole();
                IPartyRole partyRole = new PartyRole()
                {
                    RoleTypeId = profileDetail.UserTypeId
                };
                RepositoryResponse repositoryResponse2 = managePartyRole.CreatePartyRoleEnterpriseUserID(profileDetail.userLogin.RealPageId, partyRole);
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
            // see if the organization.BooksMasterId already exists
            if (organization.RealPageId != Guid.Empty)
            {
                return _organizationRepository.UpdateOrganization(organization);
            }
            else
            {
                return _organizationRepository.InsertOrganization(organization);
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
            IList<int> productIdList = _productRepository.GetProductIdsByCompany(organizationRealPageId);

            //Exclude following products from RealPage Employee Access admin user
            //Unified Platform, Asset Optimization, RealPage Accounting, Client Portal, Product Updates, EasyLMS
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.UnifiedPlatform));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.AssetOptimizer));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.FinancialSuite));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.ClientPortal));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.ProductUpdates));
            productIdList.Remove(productIdList.FirstOrDefault(p => p == (int)ProductEnum.EasyLMS));

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
                throw new ArgumentNullException(nameof(organization), "Null Organization.");
            }

            if (organization.RealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }
            return _organizationRepository.UpdateOrganization(organization);
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
            return organization;
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
        /// Used to parse the list of product codes and convert them into ProductEnum
        /// </summary>
        /// <param name="productCode"></param>
        /// <param name="addProductList"></param>
        /// <returns></returns>
        public static List<string> ParseProduct(List<string> productCode, List<ProductEnum> addProductList)
        {
            List<string> invalidProductList = new List<string>();
            if (productCode != null)
            {
                foreach (string product in productCode)
                {
                    bool foundProduct = AddProductToList(product, addProductList);
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
        /// <returns></returns>
        private static bool AddProductToList(string product, List<ProductEnum> addProductList)
        {
            bool foundProduct = false;
            try
            {
                ProductEnum productEnum = ProductEnumHelper.GetProductEnumByProductCode(product);

                foundProduct = true;
                addProductList.Add(productEnum);
            }
            catch (Exception ex)
            {
            }

            return foundProduct;
        }

        /// <summary>
		/// Used to add a product to an organization
		/// </summary>
		/// <param name="addProductList">Product List</param>
		/// <param name="partyId">Organization PartyId</param>
		/// <param name="organizationTypeId">Organization Type</param>
		/// <returns>IRepositoryResponse</returns>
		private IRepositoryResponse AddProductsToOrganization(List<ProductEnum> addProductList, long partyId, int organizationTypeId)
		{
            IRepositoryResponse response = new RepositoryResponse();
            ManageOrganizationProduct manageOrganizationProduct = new ManageOrganizationProduct(_organizationProductRepository);

			IList<OrganizationType> organizationTypeList = ListOrganizationType();
			string organizationTypeName = organizationTypeList.ToList().FirstOrDefault(o => o.OrganizationTypeId == organizationTypeId).Name;

			if (!addProductList.Contains(ProductEnum.UnifiedPlatform))
			{
				// add unified login product to every new org
				addProductList.Add(ProductEnum.UnifiedPlatform);
			}
			if (!addProductList.Contains(ProductEnum.ProductUpdates))
			{
				// add product updates to every new org
				addProductList.Add(ProductEnum.ProductUpdates);
			}
			if (!addProductList.Contains(ProductEnum.ClientPortal))
			{
				// add client portal product to every new org
				addProductList.Add(ProductEnum.ClientPortal);
			}
			if (!addProductList.Contains(ProductEnum.MigrationTool))
			{
				// add migration tool product to every new org
				addProductList.Add(ProductEnum.MigrationTool);
			}

			//Do not add products ClientPortal and MigrationTool to Company if the company type is Vendor.
			if (organizationTypeName.Equals("Vendor", StringComparison.OrdinalIgnoreCase))
			{
				addProductList.Remove(ProductEnum.ClientPortal);
				addProductList.Remove(ProductEnum.MigrationTool);
				if (!addProductList.Contains(ProductEnum.VendorMarketplace))
				{
					// add VendorMarketplace product to every new org of type Vendor
					addProductList.Add(ProductEnum.VendorMarketplace);
				}
			}

			foreach (ProductEnum product in addProductList)
			{
				response = manageOrganizationProduct.InsertUpdateOrganizationProduct(partyId: partyId, product: product, configurationId: null, fromDate: null, thruDate: null);
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					return response;
				}
			}
			return response;
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

            foreach(var instance in companyInstances)
            {
                var attributes = instance?.attributes;
                if (attributes != null)
                {
                    companyMaster.CompanyInstance.Add(attributes);
                }                
            }

            foreach(var domain in companyMaster.DomainList.ToList())
            {
                if (companyMaster.CompanyInstance.Any(a => a.Domain.Equals(domain.Domain, StringComparison.OrdinalIgnoreCase))){
                    companyMaster.DomainList.Remove(companyMaster.DomainList.First(a => a.Domain.Equals(domain.Domain, StringComparison.OrdinalIgnoreCase)));
                }
            }

            return companyMaster;
        }
        #endregion

        #region GetPropertiesForCompany
        public List<CompanyPropertySetup> GetPropertiesForCompany(Guid companyInstanceId, string propertyName = null, string domain = null, int? blueId = null, IDictionary<object, object> globals=null, long editorPersonaId=0, long userPersonaId = 0)
        {
            RequestParameter dataFilter = new RequestParameter();
           
            if (globals.ContainsKey(BaseType.RequestParameter))
            {
                dataFilter = globals[BaseType.RequestParameter] as RequestParameter;
            }            
            List<Guid> propertyInstanceIds;
            List<PropertySetup> propertyDetails = new List<PropertySetup>();
            List<int> userProperties = null;
            List<BooksPropertyInstance> booksPropertyInstance = GetPropertyInstanceFromBooks(companyInstanceId);            
            if (userPersonaId > 0)
            {
                userProperties = new List<int>();
                userProperties = _propertyRepository.ListUPFMPropertyInstanceIdByPersona(userPersonaId, ProductEnum.UnifiedUI);
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
            if(propertyInstanceIds != null)
            {
                propertyDetails = _propertyRepository.GetPropertiesForCompany(propertyInstanceIds, propertyName, blueId, dataFilter);
                propertyDetails = AddContractedNameToPropertyList(booksPropertyInstance, propertyDetails, userProperties);
            }
            List<CompanyPropertySetup> companyPropertySetup = new List<CompanyPropertySetup>()
            {
                new CompanyPropertySetup()
				{
                    Domain = booksPropertyInstance?.Where(p=>p.attributes.domain != null).Select(p => p.attributes.domain).Distinct().ToList(),
                    Property = propertyDetails
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
        #endregion

        #region UpdateProperty
        /// <summary>
        /// Update existing Property
        /// </summary>
        /// <param name="companyInstanceId">companyInstanceId</param>
        /// <param name="propertyInstanceId">property Instance Id</param>
        /// <param name="propertyName">propertyName</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateProperty(Guid companyInstanceId, Guid propertyInstanceId, string propertyName)
        {
            if (propertyInstanceId == Guid.Empty)
            {
                throw new Exception("Invalid parameter propertyInstanceId.");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new Exception("Invalid parameter propertyName.");
            }
            var _repositoryResponse = _propertyRepository.UpdateProperty(propertyInstanceId, propertyName);           
            if (_repositoryResponse.Id > 0)
            {
               bool booksResponse =  UpdatePropertyInBooks(propertyInstanceId, propertyName);
                bool settingsResponse = false;
                if (booksResponse)
                {
                    settingsResponse = UpdatePropertyInSettings(propertyInstanceId, companyInstanceId);
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
        /// <param name="companyInstanceId">companyInstanceId</param>
        /// <returns></returns>
        public PropertyInstanceSearch SearchPropertyDetailsByCustomerPropertyId(string customerPropertyId, Guid companyInstanceId)
		{
            List<BooksPropertyInstance> booksPropertyInstance = GetPropertyInstanceFromBooks(companyInstanceId);
            if(booksPropertyInstance != null &&  booksPropertyInstance.Count > 0)
			{
               var instanceExists = booksPropertyInstance.FirstOrDefault(pi =>pi.attributes.customerPropertyMap.Any(p => p.customerPropertyId == Convert.ToInt64(customerPropertyId)));
                if (instanceExists == null)
				{
                    return new PropertyInstanceSearch();
				}
            }
            CustomerProperty propertyDetails = _manageBlueBook.GetCustomerPropertyDetails(customerPropertyId);
            List<BooksPropertyInstance> _booksPropertyInstances =  _manageBlueBook.GetPropertyInstanceByCustomerPropertyId(customerPropertyId);
            List<PropertySetup> _listPropertySetup = new List<PropertySetup>();
            if(_booksPropertyInstances != null)
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

            PropertyInstanceSearch propertyInstanceSearch = new PropertyInstanceSearch()
            {
                CustomerProperty = propertyDetails,
                PropertyInstance = _listPropertySetup
            };
            return propertyInstanceSearch;
        }
        #endregion

        #region Delete Property
        /// <summary>
        /// Delete Property instance
        /// </summary>
        /// <param name="propertyInstanceID">propertyInstanceID</param>
        /// <returns></returns>
        public RepositoryResponse DeletePropertyForOrganization(Guid propertyInstanceID)
        {
            var response = _propertyRepository.DeleteUPFMPropertyInstance(propertyInstanceID);
            if (response.ErrorMessage.Length == 0)
            {
                bool booksResponse = DeletePropertyFromBooks(propertyInstanceID);
                bool settingsResponse = false;
                if (booksResponse)
                {
                    settingsResponse = DeletePropertyFromUnifiedSetting(propertyInstanceID);
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
            var productResult = _manageProductPanel.GetProductProperties(_defaultUserClaim.PersonaId, 0, productId, null);
            
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

                if (booksProductDetail.ProductId != (int) ProductEnum.UnifiedPlatform)
                {
                    translatedData = _manageBlueBook.GetTranslatePropertiesFromUPFMToProductv3(upfmProperties, booksProductDetail.BooksProductCode);
                }
                else
                {
                    translatedData = new TranslatePropertyInstance() {Data = new TranslatePropertyInstanceData() { Attributes = new List<TranslatePropertyInstanceAttribute>()}};

                    if (booksPropertyInstance != null)
                    {
                        var tpi = new List<TranslatedPropertyInstanceData>();
                        foreach (var instance in booksPropertyInstance)
                        {
                            tpi.Add(new TranslatedPropertyInstanceData() {PropertyInstanceSourceId = instance.attributes.propertyInstanceSourceId, Source = instance.attributes.source});
                            translatedData.Data.Attributes.Add(new TranslatePropertyInstanceAttribute() {PropertyInstanceSourceId = instance.attributes.propertyInstanceSourceId, Source = booksProductDetail.BooksProductCode, TranslatedPropertyInstances = tpi});
                        }
                    }
                }

                var productPropertyType = productResult.Records[0].GetType();
                var foundProductPropertyIdList = new List<string>();

                if (productPropertyType == typeof(ProductProperty))
                {
                    var productList = productResult.Records.Cast<ProductProperty>();
                    foreach (var property in productList)
                    {
                        AuditPropertyCompare(property.ID, property.Name, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                        foundProductPropertyIdList.Add(property.ID);
                    }
                }
                else if (productPropertyType == typeof(ACProperty))
                {
                    foreach (var property in productResult.Records.Cast<ACProperty>())
                    {
                        AuditPropertyCompare(property.Id, property.PropertyName, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                        foundProductPropertyIdList.Add(property.Id);
                    }
                }
                else if (productPropertyType == typeof(AssetGroup))
                {
                    foreach (var property in productResult.Records.Cast<AssetGroup>())
                    {
                        AuditPropertyCompare(property.ID, property.Name, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                        foundProductPropertyIdList.Add(property.ID);
                    }
                }
                else if (productPropertyType == typeof(OnSiteProperty))
                {
                    foreach (var property in productResult.Records.Cast<OnSiteProperty>())
                    {
                        AuditPropertyCompare(property.GetPropertyId.ToString(), property.GetName, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                        foundProductPropertyIdList.Add(property.GetPropertyId.ToString());
                    }
                }
                else if (productPropertyType == typeof(RumPropertyGroup))
                {
                    foreach (var property in productResult.Records.Cast<RumPropertyGroup>())
                    {
                        AuditPropertyCompare(property.Id.ToString(), property.Name, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                        foundProductPropertyIdList.Add(property.Id.ToString());
                    }
                }
                else if (productPropertyType == typeof(ProductProperties))
                {
                    foreach (var property in productResult.Records.Cast<ProductProperties>())
                    {
                        AuditPropertyCompare(property.GetPropertyId.ToString(), property.GetName, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                        foundProductPropertyIdList.Add(property.GetPropertyId.ToString());
                    }
                }
                else if (productPropertyType == typeof(Portfolio))
                {
                    foreach (var property in productResult.Records.Cast<Portfolio>())
                    {
                        AuditPropertyCompare(property.ID, property.Name, translatedData, instanceIds, upfmPropertyDetails, propertyAuditResult);
                        foundProductPropertyIdList.Add(property.ID);
                    }
                }
                
                // add properties that were returned from UDM for the product but the product didn't give us, out of sync data?
                translatedData?.Data?.Attributes?.ForEach(udmProperty =>
                {
                    bool foundProperty = false;
                    udmProperty.TranslatedPropertyInstances.ForEach(instances =>
                    {
                        if (foundProductPropertyIdList.Any(p => p.Equals(instances.PropertyInstanceSourceId, StringComparison.OrdinalIgnoreCase)))
                        {
                            foundProperty = true;
                        }
                    });
                    if (!foundProperty && upfmPropertyDetails != null)
                    {
                        var missingProductProperty = upfmPropertyDetails.FirstOrDefault(o => o.InstanceId.ToString().Equals(udmProperty.PropertyInstanceSourceId, StringComparison.OrdinalIgnoreCase));
                        if (missingProductProperty != null)
                        {
                            propertyAuditResult.Add(new PropertyAudit()
                            {
                                ProductInstanceId = udmProperty.TranslatedPropertyInstances[0].PropertyInstanceSourceId,
                                UPFMName = missingProductProperty.Name,
                                UPFMInstanceId = udmProperty.PropertyInstanceSourceId,
                                Status = "No Product"
                            });
                        }
                    }
                });
                propertyAuditResult = propertyAuditResult.OrderBy(p => p.Name).ThenBy(p => p.ContractedName).ToList();
            }

            return propertyAuditResult;
        }

        private static void AuditPropertyCompare(string propertyId, string propertyName, TranslatePropertyInstance translatedData, List<string> instanceids, List<PropertySetup> upfmPropertyDetails, List<PropertyAudit> propertyAuditResult)
        {
            PropertyAudit pa = new PropertyAudit()
            {
                Name = propertyName,
                ProductInstanceId = propertyId,
            };

            var instanceExists = translatedData.Data?.Attributes.FirstOrDefault(p => p.TranslatedPropertyInstances.Any(o => o.PropertyInstanceSourceId == propertyId));
            if (instanceExists != null)
            {
                pa.UPFMInstanceId = instanceExists.PropertyInstanceSourceId;
                pa.Status = instanceids.All(p => p != instanceExists.PropertyInstanceSourceId) ? "No ID" : "OK"; // Missing UPFM Instances

                var upfmProperty = upfmPropertyDetails.FirstOrDefault(p => p.InstanceId == new Guid(instanceExists.PropertyInstanceSourceId));
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
            propertyAuditResult.Add(pa);
        }

        #endregion


        #region Private Methods


        private List<CompanySetup> GetCompanyAdressFromBooks(List<CompanySetup> companyDetails)
        {
            List<UnifiedLoginCompany> compList = new List<UnifiedLoginCompany>();
            // ManageBlueBook _blueBook = new ManageBlueBook(_userClaim);
            foreach (var item in companyDetails)
            {
                compList.Add(new UnifiedLoginCompany
                {
                    CompanyId = long.Parse(item.BooksMasterId.ToString()),
                    BooksCustomerMasterId = long.Parse(item.BooksCustomerMasterId.ToString() == string.Empty ? "0" : item.BooksCustomerMasterId.ToString())
                });
            }
            IList<Company> booksCompanyDetails = _manageBlueBook.GetCompanyListByCompIds(compList);
            foreach (var items in companyDetails)
            {
                var address = booksCompanyDetails.Where(add => add.Id == items.BooksCustomerMasterId).FirstOrDefault()?.CustomerCompanyLocation;
                if (address != null && address.Length > 0)
                {
                    items.ContractedName = booksCompanyDetails.Where(add => add.Id == items.BooksCustomerMasterId).FirstOrDefault()?.CompanyName;
                    items.CompanyLocation = address[0];
                    items.Address = address[0]?.Address + "," + address[0]?.City + "," + address[0]?.State + "," + address[0]?.PostalCode;
                }
            }
            return companyDetails;
        }

        private List<BooksPropertyInstance> GetPropertyInstanceFromBooks(Guid companyInstanceId)
        {                    
            return _manageBlueBook.GetPropertyInstanceForCompany(companyInstanceId);
        }

        private List<PropertySetup> AddContractedNameToPropertyList(List<BooksPropertyInstance> booksPropertyInstance, List<PropertySetup> propertySetup, List<int> userProperties)
        {
            foreach (var property in propertySetup)
            {
                property.ContractedName = booksPropertyInstance?
										.Where(pi => pi.attributes.propertyInstanceSourceId.ToString() == property.InstanceId.ToString())
										.FirstOrDefault()?.attributes.customerPropertyMap?.FirstOrDefault()?.customerProperty.FirstOrDefault()?.propertyName;
                property.Domain = booksPropertyInstance?
                                        .Where(pi => pi.attributes.propertyInstanceSourceId.ToString() == property.InstanceId.ToString())
                                        .FirstOrDefault()?.attributes.domain;				
				property.PropertyAddress = property?.Address + "," + property?.City + "," + property?.State + "," + property?.PostalCode;
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
               // CustomerPropertyId = Convert.ToInt32(!string.IsNullOrEmpty(property.CustomerPropertyId) ? property.CustomerPropertyId : "0"),
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
            return _manageBlueBook.AddBooksGreenBookPropertyInstanceFromProvisioning(pi);
        }

        private bool UpdatePropertyInBooks(Guid propertyInstanceId, string propertyName)
        {
            PropertyInstanceAck ack = new PropertyInstanceAck
            {
                PropertyInstanceSourceId = propertyInstanceId.ToString(),
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                PropertyName = propertyName,
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
            UnifiedSettingPropertyPayload payload = PreparePropertyObjectToUnifiedSetting(property, companyInstanceID);
            return _manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Post);
        }

        private bool UpdatePropertyInSettings(Guid propertyInstanceId, Guid companyInstanceID)
        {
            List<Guid> propGuidList = new List<Guid>
            {
                propertyInstanceId
            };
            UPFMPropertyInstance _propertyInstance = _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIds(propGuidList).FirstOrDefault();
            UnifiedSettingPropertyPayload payload = PreparePropertyObjectToUnifiedSetting(_propertyInstance, companyInstanceID);
            return _manageUnifiedSettings.CreateUpdatePropertyInSetting(payload, HttpMethod.Put);
        }
        private bool DeletePropertyFromUnifiedSetting(Guid propertyInstanceID)
        {
            return _manageUnifiedSettings.DeletePropertyInSetting(propertyInstanceID);
        }

        private UnifiedSettingPropertyPayload PreparePropertyObjectToUnifiedSetting(UPFMPropertyInstance property, Guid companyInstanceID)
        {
            UnifiedSettingProperty usp = new UnifiedSettingProperty
            {
                Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform),
                Company = new UnifiedSettingCompanyInstance
                {
                    CompanyInstanceSourceId = companyInstanceID.ToString().ToLower()
                },
                Properties = new List<UnifiedSettingPropertyInstance>()
                {
                    new UnifiedSettingPropertyInstance()
                        {
                            PropertyName = property.Name,
                            PropertyInstanceSourceId = property.InstanceId,
                            CustomerPropertyId = !string.IsNullOrEmpty(property.CustomerPropertyId) ? property.CustomerPropertyId : "0",
                            IsActive = true,
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
            return new UnifiedSettingPropertyPayload
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
        #endregion
    }
}