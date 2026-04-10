using Dapper;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Foundation.DataAccess.Component.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Messaging;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.ThirdParty;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Dtos;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Batch;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.EnterpriseRole;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Kafka;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.UserUpdate;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Mappers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SO = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    /// <summary>
    /// User Repository
    /// </summary>
    public class UserRepository : BaseRepository, IUserRepository
    {
        private DefaultUserClaim _userClaim;
        private IUserLoginRepository _userLoginRepository;
        private IManagePersona _managePersona;
        private IOrganizationRepository _organizationRepository;
        private PropertyRepository _propertyRepository;
        private ContactMechanismUsageTypeRepository _contactMechanismUsageTypeRepository;
        private RoleTypeRepository _roleTypeRepository;
        IProductInternalSettingRepository _productInternalSettingRepository;
        private ManageBlueBook _manageBlueBook;
        private ManageUnifiedSettings _manageUnifiedSettings;
        private const string ProfileErrorMessage = "Update profile Error: Create Contact Mechanism failed.";
        private const string ProfileLinkUsageTypeErrorMessage = "Update profile Error: Link UsageType to Party Contact Mechanism failed.";


        #region Ctor

        /// <summary>
        /// User base Constructor
        /// </summary>
        public UserRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            //_userLoginLogic = new ManageUserLogin();
            _userLoginRepository = new UserLoginRepository();
            _organizationRepository = new OrganizationRepository();
            _managePersona = new ManagePersona();
            _userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
            _roleTypeRepository = new RoleTypeRepository();
            _manageBlueBook = new ManageBlueBook();
            _manageUnifiedSettings = new ManageUnifiedSettings(_userClaim);
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaim"></param>
        /// <param name="messageHandler"></param>
        /// <param name="blah"></param>
        public UserRepository(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler) : base(repository)
        {
            _userClaim = userClaim; //new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            _userLoginRepository = new UserLoginRepository(repository);
            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _organizationRepository = new OrganizationRepository(repository);
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _propertyRepository = new PropertyRepository(repository);
            _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository(repository);
            _roleTypeRepository = new RoleTypeRepository(repository);
            _manageBlueBook = new ManageBlueBook(userClaim, repository, messageHandler);
            _manageUnifiedSettings = new ManageUnifiedSettings(repository, userClaim, messageHandler);
        }

        /// <summary>
        /// Used when the user is known
        /// </summary>
        /// <param name="userClaim"></param>
        public UserRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
        {
            if (userClaim == null)
            {
                userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            }

            _userClaim = userClaim;
            _userLoginRepository = new UserLoginRepository();
            _managePersona = new ManagePersona(_userClaim);
            _organizationRepository = new OrganizationRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _propertyRepository = new PropertyRepository();
            _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
            _roleTypeRepository = new RoleTypeRepository();
            _manageBlueBook = new ManageBlueBook(userClaim);
            _manageUnifiedSettings = new ManageUnifiedSettings(userClaim);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Get Enterprise User
        /// </summary>
        /// <param name="enterpriseUserName">Enterprise UserName</param>
        /// <returns>User object</returns>
        public SO.User GetEnterpriseUser(string enterpriseUserName)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<SO.User>(StoredProcNameConstants.SP_GetUserByLoginId, new { loginid = enterpriseUserName });
            }
        }

        public bool CheckOrganizationAdminUser(Guid userRealpageId, long orgPartyId)
        {
            using (var repo = GetRepository())
            {
                var response = repo.GetOne<int>(StoredProcNameConstants.SP_EnterpriseCheckOrgAdmin, new { UserRealPageId = userRealpageId, OrgPartyId = orgPartyId });
                return response > 0;
            }
        }

        /// <summary>
        /// Get Starter Profile Options
        /// </summary>
        /// <param name="enterpriseUserName">enterprise UserName</param>
        /// <returns>StarterProfileOptionsResponse object</returns>
        public StarterProfileOptionsResponse GetStarterProfileOptions(string enterpriseUserName)
        {
            SO.User user;

            using (var repository = GetRepository())
            {
                user = repository.GetOne<SO.User>(StoredProcNameConstants.SP_GetUserByLoginId, new { loginid = enterpriseUserName });
            }

            return new StarterProfileOptionsResponse
            {
                EnterpriseUserName = user.LoginId,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                StandardJobTitles = GetJobTitles(),
                PhoneTypes = GetPhoneTypes()
            };
        }

        /// <summary>
        /// Set Starter Profile Options
        /// </summary>
        /// <param name="starterProfileOptions">starterProfile Options object</param>
        /// <returns>SetStarterProfile object</returns>
        public SetStarterProfile SetStarterProfileOptions(StarterProfile starterProfileOptions)
        {
            return new SetStarterProfile
            {
                EnterpriseUserName = starterProfileOptions.EnterpriseUserName,
                IsSuccess = true
            };
        }

        private BatchProcessorGroup CreateBatchProcessGroup(IRepository repo)
        {
            {
                DynamicParameters param = new DynamicParameters();
                int groupID = 0;
                param.Add("@BatchProcessorGroupID", groupID, dbType: DbType.Int32, direction: ParameterDirection.Output);

                try
                {
                    var a = repo.Execute(StoredProcNameConstants.SP_CreateBatchProcessorGroup, param);
                    groupID = param.Get<int>("@BatchProcessorGroupID");
                }
                catch (Exception ex)
                {
                }

                return new BatchProcessorGroup()
                {
                    BatchProcessorGroupId = groupID,
                    BatchProcessorGroupActivityLogged = false
                };
            }
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="newProfile">New Profile object</param>
        /// <param name="persona">Persona list</param>
        /// <returns>CreateUserResponse object with a Error Status object</returns>
        public CreateUserResponse<IErrorData> CreateUser(ProfileDetail newProfile, IList<Persona> persona)
        {
            dynamic param;
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            DateTime? fromDate = utcNow;
            DateTime? thruDate = null;

            CreateUserResponse<IErrorData> createUserResponse = new CreateUserResponse<IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IList<IdentityProviderType> identityProviderTypeList = new List<IdentityProviderType>();
            DefaultUserClaim userClaim = new DefaultUserClaim(ClaimsPrincipal.Current);
            IIdentityProviderType identityProviderType = new IdentityProviderType();
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            IList<OrganizationPrimary> orgnanizationList = new List<OrganizationPrimary>();
            IList<UserOrganization> userPersonaOrganizationList = new List<UserOrganization>();
            OrganizationStatus currentPrimaryOrgStatus = null;
            ProductBatch gbProductBatch = new ProductBatch();
            ProductBatch primaryPropertiesBatch = new ProductBatch();
            bool isDelegateAdminEnabled = GetUnifiedSettingData("delegateadministrators");

            long organizationPartyId = 0;
            long userId = 0;
            long? personaId = null;
            long cloneUserPersonaId = 0;
            string processTracker = "";
            long? ContactMechanismId = null;
            Guid organizationRealPageId = new Guid();
            Guid personRealPageId = Guid.Empty;
            long userEmailContactMechanismId = 0;
            bool profileChanged = false;
            long booksCustomerMasterId = 0;
            int greenBookRole = 0;
            List<int> greenBookRoles = new List<int>();
            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
            var platformAdminRole = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("PlatformAdminRole", StringComparison.OrdinalIgnoreCase))?.Value;
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }
            IUserLoginOnly userLoginOnly = _userLoginRepository.GetUserLoginOnly(newProfile.userLogin.LoginName);
            if (newProfile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId)
            {
                newProfile.IsRPEmployee = newProfile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId;
            }
            if (userLoginOnly != null)
            {
                //Get User Details before save
                UserDetails userDetails = GetUserDetails(personaId: null, userRealPageId: userLoginOnly.RealPageId.ToString());
                booksCustomerMasterId = userDetails.BooksCustomerMasterId;
                //Check if ONLY user profile changed without any product changes
                profileChanged = IsUserProfileChanged(newProfile, userDetails);

                userPersonaOrganizationList = _userLoginRepository.ListOrganizationByLoginName(newProfile.userLogin.LoginName);
                if (userPersonaOrganizationList.ToList().Any(i => i.OrganizationPartyId.Equals(newProfile.organization[0].PartyId)))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.1";
                    errorStatus.ErrorMsg = "Username already exists in this company.";
                    createUserResponse.Status = errorStatus;
                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                    return createUserResponse;
                }

                currentPrimaryOrgStatus = _userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);
            }

            bool organizationUsePrimaryProperties = false;
            organizationUsePrimaryProperties = GetUnifiedSettingData("PrimaryProperties");

            //IOrganizationRepository organizationRepository = new OrganizationRepository();
            Organization organizationExternalUser = _organizationRepository.GetOrganization(realPageId: DefaultUserClaim.ExternalCompanyRealPageId);

            IList<ContactMechanismUsageType> emailUsageType = _contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName: "Email Notification");

            if (newProfile.organization != null && newProfile.organization.Count > 0)
            {
                //Get the Organization IDP
                organizationPartyId = newProfile.organization[0].PartyId;
                organizationRealPageId = newProfile.organization[0].RealPageId;
                identityProviderTypeList = _organizationRepository.GetOrganizationIdentityProviderType(newProfile.organization[0].RealPageId);
            }

            bool usePropertyInstanceUnifiedLogin = getPropertyInstanceUnifiedLogin();
            primaryPropertiesBatch = newProfile.productBatch?.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.UnifiedPlatform));

            //NOTE TO DEVELOPERS
            //Any new products are added down the line,we need to update the logic in "getProductBatchForUserClone" to get new products to clone.
            if (newProfile.ClonedUser)
            {
                cloneUserPersonaId = newProfile.Persona[0].PersonaId;
                var cloneUserPersona = _managePersona.GetPersona(cloneUserPersonaId);
                IList<Organization> organizationList = _userLoginRepository.ListOrganizationByEnterpriseUserId(cloneUserPersona.RealPageId, null);
                cloneUserPersona.Organization = organizationList.FirstOrDefault(i => i.PartyId == cloneUserPersona.OrganizationPartyId);

                var personaOrganization = cloneUserPersona.Organization;
                bool isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);
                using (var pbRepository = GetRepository())
                {
                    //TODO: FIX PRODUCTS SO WE DONT CLONE PRODUCTS THIS USER DOESN'T HAVE
                    List<PersonaProductUserDetails> userProducts = pbRepository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = newProfile.Persona[0].PersonaId, ProductStatusValue = ((Int32)ProductBatchStatusType.Success).ToString() }).ToList();
                    if (userProducts != null && userProducts.Any(m => m.ProductId == 89 || m.ProductId == 104))
                    {
                        int adminSupportProductId = userProducts.First(m => m.ProductId == 89 || m.ProductId == 104).ProductId;
                        var productAttributes = pbRepository.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails, new { newProfile.Persona[0].PersonaId, ProductId = adminSupportProductId }).ToList();
                        if (productAttributes != null && productAttributes.Count == 0)
                        {
                            userProducts.RemoveAll(a => a.ProductId == adminSupportProductId);
                        }
                    }
                    if (userProducts.Count > 0)
                    {
                        long createUserPersonaId = 0L;
                        ManageCloneProductBatch manageProductBatch = new ManageCloneProductBatch(userClaim);
                        //Get the logged in user Current Persona Id
                        if (userClaim.UserRealPageGuid != Guid.Empty)
                        {
                            createUserPersonaId = pbRepository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = userClaim.UserRealPageGuid });
                        }

                        //Next Remove products which are exists in product batch
                        foreach (var product in newProfile.productBatch)
                        {
                            userProducts.RemoveAll(a => a.ProductId == product.ProductId);
                        }

                        UPFMProperty upfmProperty = new UPFMProperty();
                        if (primaryPropertiesBatch != null)
                        {
                            upfmProperty.id = primaryPropertiesBatch.InputJson.PropertyList.ToList();
                        }

                        IPersonaRepository personaRepository = new PersonaRepository(userClaim);
                        var personaProductSettings = personaRepository.GetPersonaProductSettings(cloneUserPersonaId);

                        //Then Get Product Batch Data
                        IList<ProductBatch> pbData = manageProductBatch.GetUserProductBatchData(cloneUserPersonaId, userProducts, createUserPersonaId, upfmProperty, personaProductSettings, isExternalUser);

                        foreach (ProductBatch pb in pbData)
                        {
                            newProfile.productBatch.Add(pb);
                        }

                        //Then Remove Products Which are Deselected in UI
                        var profileProductBatch = newProfile.productBatch.Where(x => x.InputJson.IsAssigned == false);
                        foreach (var pb in profileProductBatch.ToList())
                        {
                            newProfile.productBatch.Remove(pb);
                        }
                    }

                    //Get the Clone User list of UnifiedLogin Top level properties and Role
                    var procName = StoredProcNameConstants.SP_ListRolesForProductsByPersonaId;
                    var ulRole = pbRepository.GetMany<dynamic>(procName, new { ProductId = (int)ProductEnum.UnifiedPlatform, PersonaId = cloneUserPersonaId });
                    IEnumerable<dynamic> ulProperties = null;
                    IEnumerable<dynamic> ulPropertyInstances = null;

                    if (!usePropertyInstanceUnifiedLogin)
                    {
                        ulProperties = pbRepository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPropertyMapping, new { PersonaId = cloneUserPersonaId, ProductId = (int)ProductEnum.UnifiedPlatform });
                    }
                    else
                    {
                        ulPropertyInstances = pbRepository.GetMany<dynamic>(StoredProcNameConstants.SP_GetPropertyInstanceByPersonaId, new { PersonaId = cloneUserPersonaId, ProductId = (int)ProductEnum.UnifiedPlatform });
                    }

                    if ((ulProperties != null || ulPropertyInstances != null) && ulRole != null)
                    {
                        List<string> roleList = new List<string>();
                        foreach (var role in ulRole)
                        {
                            roleList.Add(Convert.ToString(role.RoleId));
                        }

                        List<string> propertyList = new List<string>();
                        if (ulProperties != null)
                        {
                            foreach (var property in ulProperties)
                            {
                                if (!usePropertyInstanceUnifiedLogin)
                                {
                                    propertyList.Add(Convert.ToString(property.PropertyID));
                                }
                                else
                                {
                                    propertyList.Add(Convert.ToString(property.PropertyInstanceID));
                                }
                            }
                        }

                        ProductBatch unifiedPlatformProductBatch = new ProductBatch()
                        {
                            ProductId = (int)ProductEnum.UnifiedPlatform,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            InputJson = new RolePropertyList()
                            {
                                PropertyList = propertyList,
                                RoleList = roleList
                            }
                        };

                        newProfile.productBatch.Add(unifiedPlatformProductBatch);
                    }
                }
            }

            IList<string> aoProductsAvailableForUser = null;
            // Get AO roles before transaction scope begins
            if (newProfile.UserTypeId == (int)UserRoleType.SuperUser && !newProfile.ClonedUser)
            {
                // if company has AO product assigned then get products available to assign based on editor User
                aoProductsAvailableForUser = GetEditorUserAoProduct(userClaim.UserRealPageGuid, userClaim.PersonaId, organizationRealPageId);
            }

            UserOrganizationExists userOrganizationExists = new UserOrganizationExists();
            IList<RoleType> roleTypes = _roleTypeRepository.GetRoleType(roleTypeName: "User Role", partyId: null);
            var SuperUserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
            var UserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
            var UserNoEmailRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
            var rpEmployee = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("realpage employee", StringComparison.OrdinalIgnoreCase));
            var rpExternalUser = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("external user", StringComparison.OrdinalIgnoreCase));

            userOrganizationExists = IsLoginNameExistsAsAdminInOtherDomain(newProfile.userLogin.LoginName, newProfile.organization[0].RealPageId, booksCustomerMasterId);
            bool primaryOrganization = true;
            //if org has mutil domain and user already exists in other domain and not in in this org
            if ((userOrganizationExists.UserExistsAsAdminInOtherDomain || userOrganizationExists.UserExistsAsRegularUserInOtherDomain) && !userOrganizationExists.UserExistsInThisOrganization)
            {
                if (!newProfile.organization[0].OrganizationDomain.Name.Equals("Primary"))
                {
                    primaryOrganization = false;
                }

            }

            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    #region Status

                    fromDate = newProfile.userLogin.FromDate.HasValue ? newProfile.userLogin.FromDate.Value : fromDate;
                    if (newProfile.userLogin.ThruDate.HasValue)
                    {
                        thruDate = newProfile.userLogin.ThruDate.Value;
                    }

                    string sourceType = "";
                    if (newProfile.CreateUserSourceType == null)
                    {
                        sourceType = CreateUserSourceType.UnifiedPlatform.ToString();
                    }
                    else
                    {
                        sourceType = newProfile.CreateUserSourceType.ToString();
                    }

                    if (newProfile.MigratedUser)
                    {
                        sourceType = CreateUserSourceType.MigrationTool.ToString();
                    }

                    if (newProfile.userLogin.ThruDate == null)
                    {
                        newProfile.userLogin.ThruDate = new DateTime(9999, 12, 31);
                    }

                    identityProviderType = (from a in identityProviderTypeList where a.IsLocal == (newProfile.userLogin.Is3rdPartyIDP ? false : true) select a).FirstOrDefault();
                    if (identityProviderType == null)
                    {
                        identityProviderType = identityProviderTypeList[0];
                    }

                    #endregion

                    if ((newProfile.UserTypeId != (int)UserRoleType.ExternalUser) &&
                        (userPersonaOrganizationList.ToList().Any(x => x.PartyRoleTypeId != (int)UserRoleType.ExternalUser)) &&
                        (newProfile.UserTypeId != (int)UserRoleType.RealPageEmployee) &&
                        (userPersonaOrganizationList.ToList().Any(x => x.PartyRoleTypeId != (int)UserRoleType.RealPageEmployee)) &&
                        ((!userOrganizationExists.UserExistsAsAdminInOtherDomain || !userOrganizationExists.UserExistsAsRegularUserInOtherDomain) && userOrganizationExists.UserExistsInThisOrganization))
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.2";
                        errorStatus.ErrorMsg = "This user type already exists for this username.";
                        createUserResponse.Status = errorStatus;
                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                        return createUserResponse;
                    }

                    if (userPersonaOrganizationList.Count == 0)
                    {
                        //User does not exist

                        #region Create Person

                        processTracker = "Create Person";
                        //if User does not exists
                        IPerson person = new Person();
                        param = new
                        {
                            Title = newProfile.Title,
                            FirstName = newProfile.FirstName,
                            MiddleName = newProfile.MiddleName,
                            LastName = newProfile.LastName,
                            Suffix = newProfile.Suffix,
                            PreferredContactMethodId = 0,
                            RealPageId = Guid.Empty
                        };
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePerson, param);
                        newProfile.RealPageId = repositoryResponse.RealPageId;
                        newProfile.PartyId = repositoryResponse.Id;
                        if (repositoryResponse.ErrorMessage != "")
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.3";
                            errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        #endregion

                        #region Create UserLogin

                        processTracker = "Create UserLogin";
                        param = new
                        {
                            newProfile.RealPageId,
                            newProfile.userLogin.LoginName,
                            CreateUserSourceType = sourceType
                        };

                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLogin, param);
                        if (repositoryResponse.Id == 0)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.4";
                            errorStatus.ErrorMsg = "Username already exists!";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        if (repositoryResponse.ErrorMessage != "")
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.5";
                            errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        userId = repositoryResponse.Id;
                        personRealPageId = newProfile.RealPageId;

                        #endregion

                        #region Update UserLogin (Password)

                        processTracker = "Update UserLogin";

                        //Set Password
                        if (!string.IsNullOrEmpty(newProfile.Password))
                        {
                            var pwd = newProfile.Password.PasswordHash();
                            newProfile.userLogin.PasswordHash = pwd.PasswordHash;
                            newProfile.userLogin.PasswordSalt = pwd.PasswordSalt;
                        }

                        //Update UserLogin
                        param = new
                        {
                            newProfile.RealPageId,
                            newProfile.userLogin.LoginName,
                            newProfile.userLogin.PasswordHash,
                            newProfile.userLogin.PasswordSalt,
                            FromDate = fromDate,
                            newProfile.userLogin.ThruDate,
                            PartyId = organizationPartyId
                        };
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, param);

                        #endregion

                        #region Preferred Contact Method and Tele-Communication

                        if ((newProfile.TelecommunicationNumber.Count > 0))
                        {
                            var response = UpdateProfile(repository, newProfile.RealPageId, newProfile);

                            if (response.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "User.CreateUser.17";
                                errorStatus.ErrorMsg = "There was an error while new user profile update.";
                                createUserResponse.Status = errorStatus;
                                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                return createUserResponse;
                            }
                        }

                        #endregion

                        #region Save notification email

                        processTracker = "Save notification email";
                        //"Regular User (No Email)" Notification Email requirement varies by Product and is handled by the UI.  Do not overwrite it by the user LoginName if it's not provided
                        if (newProfile.UserTypeId != (int)UserRoleType.UserNoEmail)
                        {
                            newProfile.NotificationEmail = string.IsNullOrEmpty(newProfile.NotificationEmail) && EmailFormatValidation.IsValidEmail(newProfile.userLogin.LoginName) ? newProfile.userLogin.LoginName : newProfile.NotificationEmail;
                        }

                        //Save the notification email if it exists
                        if (!string.IsNullOrEmpty(newProfile.NotificationEmail))
                        {
                            if (EmailFormatValidation.IsValidEmail(newProfile.NotificationEmail))
                            {
                                var EmailContactMechanism = emailUsageType.SingleOrDefault<ContactMechanismUsageType>(p => p.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));

                                //Build the parameters for email
                                LinkElectronicAddress electronicAddress = new LinkElectronicAddress()
                                {
                                    PartyContactMechanism = new PartyContactMechanism()
                                    {
                                        FromDate = utcNow,
                                        ThruDate = utcMaxValue
                                    },
                                    ElectronicAddress = new ElectronicAddress()
                                    {
                                        AddressString = newProfile.NotificationEmail,
                                        AddressType = EmailContactMechanism.Name
                                    },
                                    ContactMechanismUsageType = new ContactMechanismUsageType()
                                    {
                                        ContactMechanismUsageTypeId = (int)EmailContactMechanism.ContactMechanismUsageTypeId
                                    }
                                };

                                dynamic paramContactMechanism = new
                                {
                                    ContactMechanismId = ContactMechanismId
                                };

                                //Create Contact Mechanism
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, paramContactMechanism);
                                if (repositoryResponse.Id == 0)
                                {
                                    repository.UnitOfWork.Rollback();
                                    errorStatus.Success = false;
                                    errorStatus.ErrorCode = "User.CreateUser.19";
                                    errorStatus.ErrorMsg = "An error was encountered when creating a contact mechanism.";
                                    createUserResponse.Status = errorStatus;
                                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                    return createUserResponse;
                                }

                                ContactMechanismId = repositoryResponse.Id;

                                //Associate the Contact Mechanism to a Party
                                IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
                                partyContactMechanism = electronicAddress.PartyContactMechanism;
                                partyContactMechanism.ContactMechanismId = Convert.ToInt32(ContactMechanismId);

                                dynamic paramLinkEmailToParty = new
                                {
                                    newProfile.RealPageId,
                                    partyContactMechanism.PartyContactMechanismId,
                                    partyContactMechanism.ContactMechanismId,
                                    partyContactMechanism.FromDate,
                                    partyContactMechanism.ThruDate
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, paramLinkEmailToParty);
                                if (repositoryResponse.Id == 0)
                                {
                                    repository.UnitOfWork.Rollback();
                                    errorStatus.Success = false;
                                    errorStatus.ErrorCode = "User.CreateUser.20";
                                    errorStatus.ErrorMsg = "An error was encountered while linking user contact mechanism.";
                                    createUserResponse.Status = errorStatus;
                                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                    return createUserResponse;
                                }

                                //Assign a usage type to the Contact Mechanism
                                createUserResponse.PartyContactMechanismIdTo = repositoryResponse.Id;
                                partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                                dynamic paramLinkUsageTypeToParty = new
                                {
                                    partyContactMechanism.PartyContactMechanismId,
                                    electronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, paramLinkUsageTypeToParty);
                                if (repositoryResponse.Id == 0)
                                {
                                    repository.UnitOfWork.Rollback();
                                    errorStatus.Success = false;
                                    errorStatus.ErrorCode = "User.CreateUser.21";
                                    errorStatus.ErrorMsg = "An error was encountered when assigning a usage type to the contact mechanism.";
                                    createUserResponse.Status = errorStatus;
                                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                    return createUserResponse;
                                }

                                dynamic paramPersonEmail = new
                                {
                                    ContactMechanismId,
                                    ElectronicAddressString = electronicAddress.ElectronicAddress.AddressString,
                                    ElectronicAddressType = electronicAddress.ElectronicAddress.AddressType
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, paramPersonEmail);
                                if (repositoryResponse.Id == 0)
                                {
                                    repository.UnitOfWork.Rollback();
                                    errorStatus.Success = false;
                                    errorStatus.ErrorCode = "User.CreateUser.22";
                                    errorStatus.ErrorMsg = "An error was encountered when creating an email address.";
                                    createUserResponse.Status = errorStatus;
                                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                    return createUserResponse;
                                }

                                userEmailContactMechanismId = createUserResponse.PartyContactMechanismIdTo;
                            }
                        }

                        #endregion

                        //add to this Organization as Primary (regardless of user type)
                        orgnanizationList = new List<OrganizationPrimary>()
                        {
                            new OrganizationPrimary()
                            {
                                OrganizationRealPageId = organizationRealPageId,
                                OrganizationPartyId = organizationPartyId,
                                PrimaryOrganization = primaryOrganization,
                                OrganizationFromDate = fromDate.Value,
                                OrganizationThruDate = (thruDate ?? null)
                            }
                        };
                    }
                    else
                    {
                        //else User exists
                        userId = userLoginOnly.UserId;
                        personRealPageId = userLoginOnly.RealPageId;
                        newProfile.RealPageId = userLoginOnly.RealPageId;

                        //if user type is External, then add to this organization as External
                        if (newProfile.UserTypeId == (int)UserRoleType.ExternalUser)
                        {
                            orgnanizationList = new List<OrganizationPrimary>()
                            {
                                new OrganizationPrimary()
                                {
                                    OrganizationRealPageId = organizationRealPageId,
                                    OrganizationPartyId = organizationPartyId,
                                    PrimaryOrganization = false,
                                    OrganizationFromDate = fromDate.Value,
                                    OrganizationThruDate = (thruDate ?? null)
                                }
                            };

                            //The user is in one company as external.  Create UserLoginPersona - External Users : Primary
                            if ((userPersonaOrganizationList.Count == 1) && (userPersonaOrganizationList.ToList().Any(i => i.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser))))
                            {
                                if (!userPersonaOrganizationList.ToList().Any(m => m.OrganizationPartyId == organizationExternalUser.PartyId))
                                {
                                    DateTime newFromDate = fromDate.Value;
                                    if (currentPrimaryOrgStatus.FromDate < fromDate.Value)
                                    {
                                        newFromDate = currentPrimaryOrgStatus.FromDate;
                                    }

                                    OrganizationPrimary organizationPrimary = new OrganizationPrimary()
                                    {
                                        OrganizationRealPageId = organizationExternalUser.RealPageId,
                                        OrganizationPartyId = organizationExternalUser.PartyId,
                                        PrimaryOrganization = true,
                                        OrganizationFromDate = newFromDate,
                                        OrganizationThruDate = null

                                    };

                                    orgnanizationList.Add(organizationPrimary);
                                }
                            }
                        }
                        //Adding the User as Non-External and External Users is the Primary Company
                        //else, Delete from External Users and Add user to this organizatiom as Primary (Switch Primary Organization from External Users to this organization)
                        else if (newProfile.UserTypeId != (int)UserRoleType.ExternalUser && userPersonaOrganizationList.ToList().Any(x => ((x.PrimaryOrganization.Equals(true) && x.OrganizationPartyId.Equals(organizationExternalUser.PartyId)))))
                        {
                            //Unlink the user from External Users (Enterprise.PartyRelationship)
                            param = new
                            {
                                PersonRealPageId = userLoginOnly.RealPageId,
                                OrganizationRealPageId = organizationExternalUser.RealPageId
                            };

                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UnlinkPersonToOrganization, param);
                            if (repositoryResponse == null)
                            {
                                repository.UnitOfWork.Rollback();
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "User.CreateUser.6";
                                errorStatus.ErrorMsg = "There was an error unassociating the user to a user role.";
                                createUserResponse.Status = errorStatus;
                                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                return createUserResponse;
                            }

                            orgnanizationList = new List<OrganizationPrimary>()
                            {
                                new OrganizationPrimary()
                                {
                                    OrganizationRealPageId = organizationRealPageId,
                                    OrganizationPartyId = organizationPartyId,
                                    PrimaryOrganization = true,
                                    OrganizationFromDate = fromDate.Value,
                                    OrganizationThruDate = (thruDate ?? null)
                                }
                            };
                        }
                        else
                        {
                            orgnanizationList = new List<OrganizationPrimary>()
                            {
                                new OrganizationPrimary()
                                {
                                    OrganizationRealPageId = organizationRealPageId,
                                    OrganizationPartyId = organizationPartyId,
                                    PrimaryOrganization = primaryOrganization,
                                    OrganizationFromDate = fromDate.Value,
                                    OrganizationThruDate = (thruDate ?? null)
                                }
                            };
                        }

                        if ((profileChanged) && (newProfile.UserTypeId != (int)UserRoleType.ExternalUser))
                        {
                            param = new
                            {
                                RealPageId = newProfile.RealPageId,
                                FirstName = newProfile.FirstName,
                                MiddleName = newProfile.MiddleName,
                                LastName = newProfile.LastName
                            };
                            //Update the person's info
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "User.CreateUser.26";
                                errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                                createUserResponse.Status = errorStatus;
                                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                return createUserResponse;
                            }
                        }
                    }

                    processTracker = "Pending email notification";
                    if (identityProviderType.IsLocal)
                    {
                        IList<CommonAddress> orgMechanismList = ListContactMechanismForPerson(repository, organizationRealPageId, emailUsageType);
                        if (userEmailContactMechanismId == 0)
                        {
                            IList<CommonAddress> userMechanismList = ListContactMechanismForPerson(repository, personRealPageId, emailUsageType);
                            if (userMechanismList.Count > 0)
                            {
                                userEmailContactMechanismId = userMechanismList[0].PartyContactMechanismId;
                            }
                        }

                        var orgContactMechanismId = orgMechanismList[0].PartyContactMechanismId;
                        if (orgContactMechanismId > 0 && userEmailContactMechanismId > 0)
                        {
                            long? communicationEventId = null;
                            int statusTypeId = (int)EmailStatusType.EmailPending;
                            string note = "pending";
                            DateTime started = DateTime.UtcNow;
                            DateTime ended = DateTime.UtcNow;
                            dynamic paramCommunicationEvent = new
                            {
                                StatusTypeID = statusTypeId,
                                FromPartyContactMechanismId = orgContactMechanismId,
                                ToPartyContactMechanismId = userEmailContactMechanismId,
                                Started = started,
                                Ended = ended,
                                Note = note,
                                CommunicationEventID = communicationEventId
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEvent, paramCommunicationEvent);
                        }
                    }

                    long primaryOrgId = 0;
                    primaryOrgId = orgnanizationList.Any(x => x.PrimaryOrganization) ? orgnanizationList.FirstOrDefault(x => x.PrimaryOrganization).OrganizationPartyId : currentPrimaryOrgStatus.PartyId;

                    // get NewUserRegistration activity exp time                    
                    var activityDetail = repository.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, new { PartyId = primaryOrgId }).ToList();
                    var newUserRegistrationActivity = activityDetail.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);

                    DateTime? statusThruDate = fromDate.Value.AddHours(72); //default
                    statusThruDate = newUserRegistrationActivity != null ? fromDate.Value.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : statusThruDate;

                    int userStatusId = (int)UserUiStatusType.Active;
                    Dictionary<string, object> userRegistrationActivity = new Dictionary<string, object>() { { "UserRegistrationActivity", newUserRegistrationActivity } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", userRegistrationActivity, messageProperties: new object[] { "UserRepository.CreateUser", $"StatusThruDate : {statusThruDate}, PartyId : {primaryOrgId}, userId : {userId}" });

                    long AssignUserPersonaId = 0L;
                    long userLoginPersonaId = 0L;

                    foreach (OrganizationPrimary currentOrg in orgnanizationList)
                    {
                        DateTime? currentStatusThruDate = statusThruDate;

                        //TO DO: Update UserLoginPersona StatusTypeId if user is not Active.

                        #region Create UserLoginPersona

                        //add to External Users company
                        if (currentOrg.OrganizationPartyId.Equals(organizationExternalUser.PartyId))
                        {
                            if (currentPrimaryOrgStatus.IsPending.Value)
                            {
                                userStatusId = currentPrimaryOrgStatus.StatusTypeId;
                                currentStatusThruDate = null;
                            }
                            else
                            {
                                // the current company the user has been added to 
                                if (userPersonaOrganizationList.Count == 1 && !currentPrimaryOrgStatus.IsActive.Value)
                                {
                                    if (fromDate.Value.Subtract(DateTime.UtcNow).TotalMinutes >= 15)
                                    {
                                        userStatusId = (int)UserUiStatusType.Disabled;
                                    }
                                    else
                                    {
                                        //Insert Pending status after persona created
                                        if (identityProviderType.IsLocal)
                                        {
                                            userStatusId = (int)UserUiStatusType.Pending;
                                        }
                                        else
                                        {
                                            userStatusId = (int)UserUiStatusType.Active;
                                            currentStatusThruDate = null;
                                        }
                                    }
                                }
                                else
                                {
                                    userStatusId = (int)UserUiStatusType.Active;
                                    currentStatusThruDate = null;
                                }
                            }
                        }
                        //add to Logged-in (Current) Company
                        else
                        {
                            if (fromDate.Value.Subtract(DateTime.UtcNow).TotalMinutes >= 15)
                            {
                                userStatusId = (int)UserUiStatusType.Disabled;
                            }
                            else
                            {
                                //Insert Pending status after persona created
                                if (identityProviderType.IsLocal)
                                {
                                    if (currentPrimaryOrgStatus == null) // && newProfile.UserTypeId != (int) UserRoleType.ExternalUser)
                                    {
                                        if (newProfile.userLogin.doNotForceChangePassword == true)
                                        {
                                            userStatusId = (int)UserUiStatusType.Active;
                                            currentStatusThruDate = null;
                                        }
                                        else
                                        {
                                            userStatusId = (int)UserUiStatusType.Pending;
                                        }
                                    }
                                    else
                                    {
                                        userStatusId = (int)UserUiStatusType.Active;
                                        currentStatusThruDate = null;

                                        if (newProfile.UserTypeId != (int)UserRoleType.ExternalUser && currentPrimaryOrgStatus.IsPending.Value)
                                        {
                                            userStatusId = currentPrimaryOrgStatus.StatusTypeId;
                                            currentStatusThruDate = currentPrimaryOrgStatus.StatusThruDate;
                                        }
                                    }
                                }
                                else
                                {
                                    userStatusId = (int)UserUiStatusType.Active;
                                    currentStatusThruDate = null;
                                }
                            }
                        }

                        #endregion

                        #region Create Persona

                        //Create Persona
                        processTracker = "Create Persona";
                        long? personaTypeId = null;
                        long? personaEnvironmentTypeId = null;
                        DateTime? personaFromDate = utcNow;
                        DateTime? personaThruDate = null;

                        //Persona
                        if (persona == null)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.8";
                            errorStatus.ErrorMsg = "User has no persona.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        Persona personaFromUI = persona[0];

                        if (currentPrimaryOrgStatus != null && organizationExternalUser.PartyId == currentOrg.OrganizationPartyId)
                        {
                            // set the external persona from date to be the same as the current primary company
                            personaFromDate = currentPrimaryOrgStatus.FromDate;
                            personaThruDate = null;
                        }
                        else
                        {
                            personaFromDate = personaFromUI.FromDate;
                            if (personaFromUI.FromDate == null)
                            {
                                personaFromDate = utcNow;
                            }

                            personaThruDate = personaFromUI.ThruDate;
                        }
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", null, messageProperties: new object[] { "UserRepository.CreateUser CurrentStatusThruDate", $"CurrentStatusThruDate : {currentStatusThruDate}, PartyId : {currentOrg.OrganizationPartyId}, userId : {userId}" });

                        if (_userClaim.ImpersonatedByName != null)
                        {
                            param = new
                            {
                                UserLoginId = userId,
                                StatusTypeId = userStatusId,
                                OrganizationPartyId = currentOrg.OrganizationPartyId,
                                PrimaryOrganization = currentOrg.PrimaryOrganization,
                                FromDate = currentOrg.OrganizationFromDate,
                                ThruDate = currentOrg.OrganizationThruDate,
                                StatusThruDate = currentStatusThruDate,
                                IsRPEmployee = newProfile.IsRPEmployee,
                                IsDelegateAdmin = newProfile.IsDelegateAdmin,
                                IsRealPartner = newProfile.IsRealPartner
                            };
                        }
                        else
                        {
                            param = new
                            {
                                UserLoginId = userId,
                                StatusTypeId = userStatusId,
                                OrganizationPartyId = currentOrg.OrganizationPartyId,
                                PrimaryOrganization = currentOrg.PrimaryOrganization,
                                FromDate = currentOrg.OrganizationFromDate,
                                ThruDate = currentOrg.OrganizationThruDate,
                                StatusThruDate = currentStatusThruDate,
                                IsRPEmployee = newProfile.IsRPEmployee,
                                IsDelegateAdmin = newProfile.IsDelegateAdmin
                            };

                        }

                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLoginPersona, param);
                        if (repositoryResponse.Id == 0)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.7";
                            errorStatus.ErrorMsg = "Error creating the user login status.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        userLoginPersonaId = repositoryResponse.Id;

                        switch (personaFromUI.Name.ToLowerInvariant())
                        {
                            case "primary":
                                personaTypeId = (int)PersonaType.Primary;
                                break;
                            case "system administrator":
                                personaTypeId = (int)PersonaType.SuperUser;
                                break;
                            default:
                                personaTypeId = (int)PersonaType.Primary;
                                break;
                        }

                        if (currentOrg.OrganizationPartyId.Equals(organizationExternalUser.PartyId))
                        {
                            personaTypeId = (int)PersonaType.Primary;
                        }

                        personaEnvironmentTypeId = personaFromUI.PersonaEnvironmentTypeId;

                        personaId = null;

                        param = new
                        {
                            PersonRealPageId = personRealPageId,
                            UserLoginPersonaId = userLoginPersonaId,
                            OrganizationRealPageId = currentOrg.OrganizationRealPageId,
                            PersonaTypeId = personaTypeId,
                            UserId = userId,
                            PersonaEnvironmentTypeId = personaEnvironmentTypeId,
                            FromDate = personaFromDate,
                            ThruDate = personaThruDate,
                            personaId
                        };

                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, param);
                        if (repositoryResponse.Id == 0)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.9";
                            errorStatus.ErrorMsg = "Persona was not created.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        personaId = repositoryResponse.Id;
                        if (organizationPartyId == currentOrg.OrganizationPartyId)
                        {
                            // get the new persona for the company the user is being added to so it can be used later in the product batch calls
                            AssignUserPersonaId = repositoryResponse.Id;
                            createUserResponse.PersonaId = repositoryResponse.Id;
                        }

                        #region create user company association

                        if (FeatureFlag.GetUserCompanyAssociationFeatureFlag())
                        {
                            if (newProfile.ExternalUserRelationship != null && newProfile.ExternalUserRelationship.ThirdPartyRelationShipId > 0)
                            {
                                param = new
                                {
                                    UserLoginPersonaId = userLoginPersonaId,
                                    ThirdPartyRelationshipId = newProfile.ExternalUserRelationship.ThirdPartyRelationShipId,
                                    CompanyName = newProfile.ExternalUserRelationship.ThirdPartyCompanyName,
                                    ThirdPartyCompanyRealPageId = newProfile.ExternalUserRelationship.ThirdPartyCompanyRealPageId,
                                    OperatorCode = newProfile.ExternalUserRelationship.OperatorCode,
                                    OperatorValue = newProfile.ExternalUserRelationship.OperatorValue
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateExternalUserRelationship, param);

                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Create ExternalUser Relationship: Create External User Relationship failed.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }
                            }
                        }

                        #endregion


                        //Link persona to enterprise Role ID
                        var enterpriseRole = newProfile.productBatch?.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.UnifiedUI));
                        if (enterpriseRole?.InputJson?.RoleList != null && enterpriseRole?.InputJson?.RoleList.Count > 0)
                        {
                            int roleTemplateId = Convert.ToInt32(enterpriseRole.InputJson.RoleList.FirstOrDefault());
                            repositoryResponse = InsertUpdateEnterpriseRoleToUser(repository, roleTemplateId, personaId);
                            if (repositoryResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "User.CreateUser.9";
                                errorStatus.ErrorMsg = "User not assigned to Enterprise Role.";
                                createUserResponse.Status = errorStatus;
                                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                return createUserResponse;
                            }
                        }

                        // Linking Persona to a Role based on user type
                        param = new
                        {
                            realPageId = currentOrg.OrganizationRealPageId
                        };
                        IList<EnterpriseRole> enterpriseRoles = repository.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_SecurityListRolesByRealPageID, param);

                        gbProductBatch = newProfile.productBatch?.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.UnifiedPlatform));

                        if (currentOrg.OrganizationPartyId.Equals(organizationExternalUser.PartyId))
                        {
                            greenBookRole = enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase)).RoleId;
                        }
                        else
                        {
                            if (SuperUserRole.PartyRoleTypeId == newProfile.UserTypeId)
                            {
                                greenBookRole = enterpriseRoles.FirstOrDefault(r => r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase)).RoleId;
                            }
                            else
                            {
                                if (gbProductBatch != null && gbProductBatch.InputJson.RoleList?.Count > 0)
                                {
                                    greenBookRoles = GetGreenBookRoles(gbProductBatch);
                                }
                                else
                                {
                                    if (newProfile.ClonedUser)
                                    {
                                        // get the users existing UnifiedLogin role                                       
                                        param = new
                                        {
                                            PersonaID = cloneUserPersonaId,
                                            ProductID = (int)ProductEnum.UnifiedPlatform
                                        };
                                        var userRole = repository.GetOne<dynamic>(StoredProcNameConstants.SP_ListRolesForProductsByPersonaId, param);
                                        greenBookRole = userRole != null ? userRole.RoleId : 0;
                                    }
                                    else
                                    {
                                        param = new
                                        {
                                            RealPageID = currentOrg.OrganizationRealPageId
                                        };
                                        var defaultRole = repository.GetOne<dynamic>(StoredProcNameConstants.SP_GetUnifiedLoginDefaultRole, param);

                                        greenBookRole = defaultRole != null ? defaultRole.RoleId : enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase)).RoleId;
                                    }
                                }
                            }
                        }

                        bool roleisLinked = false;

                        if (greenBookRole > 0)
                        {
                            param = new
                            {
                                personaID = personaId,
                                roleID = greenBookRole,
                                CreatedBy = _userClaim.UserId,
                                personaPrivilgeID = 0
                            };

                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
                            if (repositoryResponse.Id == 0)
                                roleisLinked = false;
                            else
                                roleisLinked = true;
                        }
                        else
                        {
                            foreach (var role in greenBookRoles)
                            {
                                param = new
                                {
                                    personaID = personaId,
                                    roleID = role,
                                    CreatedBy = _userClaim.UserId,
                                    personaPrivilgeID = 0
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    roleisLinked = false;
                                    break;
                                }
                                else
                                    roleisLinked = true;
                            }
                        }

                        if (!roleisLinked)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.10";
                            errorStatus.ErrorMsg = "There was an error associating the persona to a user role.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }
                        else
                        {
                            if ((SuperUserRole.PartyRoleTypeId == newProfile.UserTypeId) && (enterpriseRoles.FirstOrDefault(r => r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase)).RoleId > 0))
                            {
                                gbProductBatch = new ProductBatch()
                                {
                                    InputJson = new RolePropertyList()
                                    {
                                        PropertyList = new List<string>()
                                            { "-1" }
                                    }
                                };
                            }

                            if (gbProductBatch != null && ((gbProductBatch.InputJson?.PropertyList?.Count > 0) || (gbProductBatch.InputJson?.RemovedPropertyList?.Count > 0)))
                            {
                                string propertyJSON = JsonConvert.SerializeObject(gbProductBatch);
                                if (!usePropertyInstanceUnifiedLogin)
                                {
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdatePropertyMapping, new { PersonaId = personaId, ProductId = (int)ProductEnum.UnifiedPlatform, PropertyJSON = propertyJSON });
                                }
                                else
                                {
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdatePropertyInstanceMapping, new { PersonaId = personaId, ProductId = (int)ProductEnum.UnifiedPlatform, PropertyInstanceJSON = propertyJSON });
                                }

                                if (repositoryResponse.Id == 0)
                                {
                                    repository.UnitOfWork.Rollback();
                                    errorStatus.Success = false;
                                    errorStatus.ErrorCode = "User.CreateUser.27";
                                    errorStatus.ErrorMsg = "There was an error assigning top level properties to persona: {personaId}.";
                                    createUserResponse.Status = errorStatus;
                                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                    return createUserResponse;
                                }
                            }

                        }

                        //End Create Persona

                        #endregion

                        #region Create UserEmployeeId

                        if (userLoginPersonaId > 0)
                        {
                            param = new
                            {
                                UserLoginPersonaId = userLoginPersonaId,
                                EmployeeId = newProfile.EmployeeId
                            };

                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateEmployeeId, param);

                            if (repositoryResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "User.CreateUser.28";
                                errorStatus.ErrorMsg = "Error creating EmployeeId to the user login perosna: {userLoginPersonaId}";
                                createUserResponse.Status = errorStatus;
                                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                return createUserResponse;
                            }
                        }

                        #endregion

                        #region Create Supervisor

                        if (newProfile.SuperVisorUserId > 0)
                        {
                            param = new
                            {
                                UserId = userId,
                                SuperVisorUserId = newProfile.SuperVisorUserId
                            };

                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertUpdateSuperVisor, param);

                            if (repositoryResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "User.CreateUser.28";
                                errorStatus.ErrorMsg = $"Error creating Supervisor to the user login persona: {userLoginPersonaId}";
                                createUserResponse.Status = errorStatus;
                                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                return createUserResponse;
                            }
                            else
                            {
                                string userName = string.IsNullOrEmpty(_userClaim.ImpersonatedByName) ? _userClaim.FirstName + " " + _userClaim.LastName : " RealPage Access (" + _userClaim.ImpersonatedByName + ") ";
                                param = new
                                {
                                    UserId = userId,
                                    OrgPartyId = _userClaim.OrganizationPartyId
                                };
                                var supervisorinfo = repository.GetOne<UserInfoLite>(StoredProcNameConstants.SP_GetSuperVisorId, param);
                                if (supervisorinfo != null)
                                {
                                    string superVisorMessage = $"{userName} updated supervisor for {newProfile.FirstName} {newProfile.LastName}. Set to {supervisorinfo.FirstName} {supervisorinfo.LastName}({supervisorinfo.LoginName}).";
                                    AuditActivityLog(" ", supervisorinfo.LoginName, "Supervisor", superVisorMessage, newProfile);
                                }
                            }
                        }

                        #endregion

                        #region Set Default Employment Role

                        processTracker = "Set Default Employment Role";
                        //Set Person Role (Employer, User Type) to Organization                    
                        param = new
                        {
                            RoleTypeName = "Organization Role"
                        };
                        roleTypes = repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, param);
                        var Employer = roleTypes.SingleOrDefault<RoleType>(p => p.Name == "Employer");
                        var UserType = roleTypes.SingleOrDefault<RoleType>(p => p.Name == "User Type");
                        if (Employer == null)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.13";
                            errorStatus.ErrorMsg = "Employer role is missing.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        if (UserType == null)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.14";
                            errorStatus.ErrorMsg = "User Type role is missing.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        #endregion

                        #region Set User Type

                        processTracker = "Set User Type";
                        int roleTypeIdFrom = 0;
                        int roleTypeIdTo = (int)Employer.PartyRoleTypeId; //Employer
                        if (SuperUserRole == null || UserRole == null || UserNoEmailRole == null || rpEmployee == null)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.15";
                            errorStatus.ErrorMsg = "User role(s) missing.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        switch (newProfile.UserTypeId)
                        {
                            case (int)UserRoleType.User:
                                roleTypeIdFrom = UserRole.PartyRoleTypeId;
                                break;
                            case (int)UserRoleType.SuperUser:
                                roleTypeIdFrom = SuperUserRole.PartyRoleTypeId;
                                break;
                            case (int)UserRoleType.RealPageEmployee:
                                roleTypeIdFrom = rpEmployee.PartyRoleTypeId;
                                break;
                            case (int)UserRoleType.UserNoEmail:
                                roleTypeIdFrom = UserNoEmailRole.PartyRoleTypeId;
                                break;
                            case (int)UserRoleType.ExternalUser:
                                roleTypeIdFrom = rpExternalUser.PartyRoleTypeId;
                                break;
                            default:
                                roleTypeIdFrom = UserRole.PartyRoleTypeId;
                                break;
                        }

                        roleTypeIdTo = (int)UserType.PartyRoleTypeId; //User Type

                        param = new
                        {
                            PersonRealPageId = personRealPageId,
                            OrganizationRealPageId = currentOrg.OrganizationRealPageId,
                            RoleTypeIdFrom = roleTypeIdFrom,
                            RoleTypeIdTo = roleTypeIdTo
                        };

                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonToOrganization, param);
                        if (repositoryResponse == null)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.16";
                            errorStatus.ErrorMsg = "There was an error associating the user to a user role.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        #endregion
                    }

                    if (!userPersonaOrganizationList.ToList().Any(x => ((x.PartyRoleTypeId.Equals((int)UserRoleType.RealPageEmployee)))) && newProfile.UserTypeId == (int)UserRoleType.RealPageEmployee)
                    {
                        foreach (var userPreviousOrg in userPersonaOrganizationList)
                        {
                            #region Update User Type if Realpage Employee

                            processTracker = "Update User Type";
                            //Get the Current User Type
                            Guid realPageIdFrom = personRealPageId;
                            Guid realPageIdTo = userPreviousOrg.OrganizationRealPageId;
                            string roleTypeName = null;
                            string relationshipTypeName = "User Type";

                            dynamic paramRelType = new
                            {
                                realPageIdFrom,
                                realPageIdTo,
                                roleTypeName,
                                relationshipTypeName
                            };
                            PartyRelationship relationshipType = repository.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, paramRelType);
                            if (relationshipType != null)
                            {
                                int unlinkRoleTypeIdFrom = relationshipType.RoleTypeIdFrom;
                                int linkRoleTypeIdFrom = (int)UserRoleType.ExternalUser;
                                int roleTypeIdTo = relationshipType.RoleTypeIdTo;

                                //Update the User Type  
                                if (unlinkRoleTypeIdFrom != linkRoleTypeIdFrom && relationshipType.RoleTypeIdFrom != (int)UserRoleType.ExternalUser)
                                {
                                    dynamic paramRole = new
                                    {
                                        personRealPageId,
                                        userPreviousOrg.OrganizationRealPageId,
                                        unlinkRoleTypeIdFrom,
                                        linkRoleTypeIdFrom,
                                        roleTypeIdTo
                                    };

                                    RepositoryResponse RoleId = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, paramRole);
                                    if (RoleId.Id == 0)
                                    {
                                        repository.UnitOfWork.Rollback();
                                        errorStatus.Success = false;
                                        errorStatus.ErrorCode = "User.CreateUser.29";
                                        errorStatus.ErrorMsg = "Unable to set new user type.";
                                        createUserResponse.Status = errorStatus;
                                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                        return createUserResponse;
                                    }
                                }
                            }

                            #endregion
                        }
                    }

                    #region Create User custom fields

                    processTracker = "Create User custom fields";
                    if (newProfile.CustomFields?.Count > 0)
                    {
                        param = new
                        {
                            UserLoginId = userId,
                            OrganizationPartyId = organizationPartyId
                        };
                        IList<UserLoginPersona> userLoginPersonaList = repository.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona, param);

                        newProfile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId);
                        string customFieldsValuesJson = JsonConvert.SerializeObject(newProfile.CustomFields);

                        bool IsValidJson = ValidateJson.IsValidJson<IList<CustomFieldValue>>(customFieldsValuesJson);
                        if (IsValidJson)
                        {
                            param = new
                            {
                                JSON = customFieldsValuesJson,
                                CreatedBy = _userClaim.UserId
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateFieldValue, param);
                            if ((repositoryResponse.Id == 0) && (!string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage)))
                            {
                                repository.UnitOfWork.Rollback();
                                errorStatus.Success = false;
                                errorStatus.ErrorCode = "User.CreateUser.18";
                                errorStatus.ErrorMsg = "User Custom Fields was not created.";
                                createUserResponse.Status = errorStatus;
                                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                                return createUserResponse;
                            }
                        }
                    }

                    #endregion

                    #region SaveProductDetails

                    long CreateUserPersonaId = 0L;

                    try
                    {
                        //Get the logged in user Current Persona Id
                        if (userClaim.UserRealPageGuid != Guid.Empty)
                        {
                            CreateUserPersonaId = userClaim.PersonaId; //repository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = userClaim.UserRealPageGuid });
                        }
                        else if (_userClaim.UserRealPageGuid != Guid.Empty)
                        {
                            CreateUserPersonaId = _userClaim.PersonaId;
                            userClaim = _userClaim;
                        }
                    }
                    catch
                    {
                    }

                    if (primaryOrgId.Equals(organizationPartyId))
                    {
                        //Link Identity Provider (ContactMechanismId for the Identity Provider value) to new user by UserLoginId & ActivePersonaId
                        param = new
                        {
                            UserId = userId,
                            ContactMechanismID = identityProviderType.ContactMechanismId
                        };
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, param);
                        if (repositoryResponse.Id == 0)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.23";
                            errorStatus.ErrorMsg = "Create User Error: Link Identity Provider to UserLogin failed.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }
                    }

                    processTracker = "SaveProductDetails";
                    int productCount = SaveProductDetails(repository, newProfile.productBatch, createUserResponse, CreateUserPersonaId, AssignUserPersonaId, userClaim.UserRealPageGuid, organizationRealPageId, errorStatus, newProfile.UserTypeId, true, impersonatorUserLoginOnly.UserId, aoProductsAvailableForUser, newProfile.MigratedUser, true, greenBookRole, "add", false, newProfile.RoleIdList, organizationUsePrimaryProperties);

                    #endregion

                    #region Enterprise roles Delegate User
                    if (isDelegateAdminEnabled && newProfile.IsDelegateAdmin && newProfile.DelegateRoleTemplate.RoleTemplateId.Count() > 0)
                    {
                        List<int> templateRoleLists = newProfile.DelegateRoleTemplate?.RoleTemplateId?.ToList();
                        repositoryResponse = InsertUpdateDelegateAdminRole(repository, userLoginPersonaId, templateRoleLists);
                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = "Create Delegate Admin Role  failed.";
                            throw new Exception(repositoryResponse.ErrorMessage);
                        }
                    }
                    #endregion

                    // used to pass back user id for logging
                    newProfile.userLogin.UserId = userId;
                    newProfile.userLogin.RealPageId = personRealPageId;

                    createUserResponse.UserStatus = "User created successfully.";
                    createUserResponse.Status = errorStatus;
                    createUserResponse.UserRealPageGuid = personRealPageId;

                    //COMMIT THE CHANGE
                    repository.UnitOfWork.Commit();
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.24";
                    errorStatus.ErrorMsg = "Create User Error: " + exception.Message + ". Process: " + processTracker;
                    createUserResponse.Status = errorStatus;
                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                    createUserResponse.UserRealPageGuid = Guid.Empty;
                    WriteToLog(LogEventLevel.Error, "{ActionName} - {state}", null, exception, messageProperties: new object[] { "CreateUser", $"Error : {exception.Message}" });

                    return createUserResponse;
                }

                return createUserResponse;
            }
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <param name="userLogin">userLogin</param>
        /// <param name="newProfile">newProfile object</param>
        /// <param name="partyRoleTypeId">party RoleType Id</param>
        /// <param name="companyJobTitle">company Job Title</param>
        /// <param name="activityToken">activity Token</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateNewUser(string userLogin, Profile newProfile, int partyRoleTypeId, string companyJobTitle, string activityToken)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            RepositoryResponse response = new RepositoryResponse();

            //Get the realPageId from the userLogin
            ICredentialRepository _credentialRepository = new CredentialRepository();
            IUserLoginRepository r = new UserLoginRepository();
            var userLoginOnly = r.GetUserLoginOnly(userLogin);
            if (userLoginOnly == null)
            {
                response.ErrorMessage = "User Name is incorrect or not found.";
                return response;
            }

            Guid realPageId = userLoginOnly.RealPageId;
            long orgPartyId = 0;
            // Get user's OrgId
            var listOrg = _credentialRepository.ListOrganizationByRealPageId(realPageId);
            if (listOrg != null)
            {
                Guid organizationRealPageId = listOrg[0].RealPageId;
                orgPartyId = listOrg[0].PartyId;
            }

            var tokenDetail = _credentialRepository.GetActivityToken(userLogin, activityToken, (int)ActivityType.NewUserRegistrationVerification, orgPartyId);

            if (tokenDetail == null || tokenDetail.EnterpriseUserId <= 0)
            {
                response.ErrorMessage = "Validation token does not match with user.";
                return response;
            }

            //Get the Person from the realPageId
            IPerson person = new Person();
            IManagePerson personLogic = new ManagePerson();
            person = personLogic.GetPerson(realPageId);

            if (person == null)
            {
                response.ErrorMessage = "Person details not found.";
                return response;
            }

            using (var repository = GetRepository())
            {
                try
                {
                    repository.UnitOfWork.BeginTransaction();

                    //Update the title
                    person.Title = companyJobTitle;
                    dynamic paramPerson = new
                    {
                        realPageId,
                        person.FirstName,
                        person.MiddleName,
                        person.LastName,
                        person.Title,
                        person.Suffix,
                        person.PreferredContactMethodId
                    };

                    response = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, paramPerson);
                    if (response == null)
                    {
                        response.ErrorMessage = "New profile Error: Company Job Title update failed.";
                    }
                    else
                    {
                        response = UpdateUserProfile(repository, person.RealPageId, newProfile);

                        if (response.Id == 0)
                        {
                            repository.UnitOfWork.Rollback();
                            response.ErrorMessage = "Error: " + response.ErrorMessage;
                        }
                    }

                    repository.UnitOfWork.Commit();
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    response.ErrorMessage = "Error: " + exception.Message;
                }
            }

            return response;
        }

        /// <summary>
        /// Update User from User List
        /// </summary>
        /// <param name="userProfile">userProfile object</param>
        /// <param name="updatePersona">List of Persona to update</param>
        /// <param name="deletePersona">List of Persona to delete</param>
        /// <param name="userTypeId">User Type</param>
        /// <param name="listOrg">List of Organization</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateUserListUser(ProfileDetail userProfile, IList<Persona> updatePersona, IList<Persona> deletePersona, int userTypeId, IList<Organization> listOrg)
        {
            RepositoryResponse response = new RepositoryResponse();
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            DateTime? fromDate = utcNow;
            DateTime? thruDate = utcMaxValue;

            Guid personRealPageId = userProfile.RealPageId;
            Guid organizationRealPageId = Guid.Empty;
            long? orgPartyId = null;

            if (listOrg != null)
            {
                organizationRealPageId = listOrg[0].RealPageId;
                orgPartyId = listOrg[0].PartyId;
            }

            long? personaId = null;

            string processTracker = "";
            using (var repository = GetRepository())
            {
                repository.UnitOfWork.BeginTransaction();

                try
                {
                    #region Update User Login

                    processTracker = "Update User Login";

                    dynamic paramNewUserLogin = new
                    {
                        userProfile.RealPageId
                    };
                    UserLogin currentUserLogin = repository.GetOne<UserLogin>(StoredProcNameConstants.SP_GetUserLogin, paramNewUserLogin);

                    if (userProfile.userLogin.LoginName != currentUserLogin.LoginName)
                    {
                        dynamic paramCheckLogin = new
                        {
                            EnterpriseUserName = userProfile.userLogin.LoginName
                        };
                        var checkLoginName = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, paramCheckLogin);
                        if (checkLoginName != null)
                        {
                            repository.UnitOfWork.Rollback();
                            response.ErrorMessage = "Username already exists!";
                            return response;
                        }
                    }

                    fromDate = userProfile.userLogin.FromDate.HasValue ? userProfile.userLogin.FromDate.Value.ToUniversalTime() : fromDate;

                    if (userProfile.userLogin.ThruDate.HasValue && userProfile.userLogin.ThruDate.Value.Date == utcMaxValue.Date)
                    {
                        userProfile.userLogin.ThruDate = userProfile.userLogin.ThruDate.Value.AddMilliseconds(-999);
                    }

                    dynamic paramUpdateUserLogin = new
                    {
                        userProfile.RealPageId,
                        userProfile.userLogin.LoginName,
                        currentUserLogin.PasswordHash,
                        currentUserLogin.PasswordSalt,
                        FromDate = fromDate,
                        ThruDate = userProfile.userLogin.ThruDate,
                        PartyId = orgPartyId
                    };

                    response = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, paramUpdateUserLogin);

                    processTracker = "Update UserLogin Statuses";

                    DateTime? statusThruDate = null;

                    var userFromDate = fromDate.Value;
                    //set active or inactive only when eff date is current/past and exp date is in future or not set
                    if ((fromDate.Value <= DateTime.UtcNow)
                        && (thruDate == null || thruDate.HasValue && thruDate.Value.ToUniversalTime() >= DateTime.UtcNow))
                    {
                        if (userProfile.userLogin.IsActive.HasValue && userProfile.userLogin.IsActive == true)
                        {
                            statusThruDate = null;
                        }
                        else
                        {
                            statusThruDate = DateTime.UtcNow.AddMinutes(-1);
                        }
                    }

                    RepositoryResponse updateUserStatusResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                        new
                        {
                            RealPageId = userProfile.RealPageId,
                            OrganizationPartyId = orgPartyId,
                            StatusTypeId = UserUiStatusType.Active,
                            FromDate = fromDate,
                            StatusThruDate = statusThruDate
                        });

                    //update notification email
                    if (!string.IsNullOrEmpty(userProfile.NotificationEmail))
                    {
                        if (EmailFormatValidation.IsValidEmail(userProfile.NotificationEmail))
                        {
                            CommonAddress emailAddress = new CommonAddress();
                            if (userProfile.contactMechanism.Count > 0)
                            {
                                emailAddress = userProfile.contactMechanism.FirstOrDefault(p => p.contactMechanismUsageType.ContactMechanismUsageTypeId == 301);

                                dynamic paramEmail = new
                                {
                                    emailAddress.ContactMechanismId,
                                    ElectronicAddressString = userProfile.NotificationEmail
                                };
                                RepositoryResponse emailUpdate = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, paramEmail);
                                if (emailUpdate.Id == 0)
                                {
                                    repository.UnitOfWork.Rollback();
                                    response.ErrorMessage = "Notification email was not created.";
                                    return response;
                                }
                            }
                            //Email does not yet exist
                            else
                            {
                                if (EmailFormatValidation.IsValidEmail(userProfile.NotificationEmail))
                                {
                                    string ContactMechanismUsageTypeName = "Email Notification";
                                    dynamic paramEmail = new
                                    {
                                        ContactMechanismUsageTypeName
                                    };
                                    IList<ContactMechanismUsageType> emailUsageType = repository.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, paramEmail);
                                    var EmailContactMechanism = emailUsageType.SingleOrDefault<ContactMechanismUsageType>(p => p.Name == "Email");

                                    //Build the parameters for email
                                    LinkElectronicAddress electronicAddress = new LinkElectronicAddress();
                                    electronicAddress.PartyContactMechanism.FromDate = utcNow;
                                    electronicAddress.PartyContactMechanism.ThruDate = utcMaxValue;
                                    electronicAddress.ElectronicAddress.AddressString = userProfile.NotificationEmail;
                                    electronicAddress.ElectronicAddress.AddressType = EmailContactMechanism.Name;
                                    electronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId = (int)EmailContactMechanism.ContactMechanismUsageTypeId;

                                    long? ContactMechanismId = null;
                                    dynamic paramContactMechanism = new
                                    {
                                        ContactMechanismId = ContactMechanismId
                                    };

                                    //Create Contact Mechanism
                                    RepositoryResponse contactMechanism = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, paramContactMechanism);
                                    if (contactMechanism.Id == 0)
                                    {
                                        repository.UnitOfWork.Rollback();
                                        response.ErrorMessage = "An error was encountered when creating a contact mechanism.";
                                        return response;
                                    }

                                    ContactMechanismId = contactMechanism.Id;

                                    //Associate the Contact Mechanism to a Party
                                    IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
                                    partyContactMechanism = electronicAddress.PartyContactMechanism;
                                    partyContactMechanism.ContactMechanismId = Convert.ToInt32(ContactMechanismId);

                                    dynamic paramLinkEmailToParty = new
                                    {
                                        userProfile.RealPageId,
                                        partyContactMechanism.PartyContactMechanismId,
                                        partyContactMechanism.ContactMechanismId,
                                        partyContactMechanism.FromDate,
                                        partyContactMechanism.ThruDate
                                    };

                                    RepositoryResponse LinkEmailToParty = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, paramLinkEmailToParty);
                                    if (LinkEmailToParty.Id == 0)
                                    {
                                        repository.UnitOfWork.Rollback();
                                        response.ErrorMessage = "An error was encountered when creating a contact mechanism.";
                                        return response;
                                    }

                                    //Assign a usage type to the Contact Mechanism
                                    partyContactMechanism.PartyContactMechanismId = LinkEmailToParty.Id;
                                    dynamic paramLinkUsageTypeToParty = new
                                    {
                                        partyContactMechanism.PartyContactMechanismId,
                                        electronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId
                                    };
                                    RepositoryResponse LinkUsageTypeToParty = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, paramLinkUsageTypeToParty);
                                    if (LinkUsageTypeToParty.Id == 0)
                                    {
                                        repository.UnitOfWork.Rollback();
                                        response.ErrorMessage = "An error was encountered when assigning a usage type to the contact mechanism.";
                                        return response;
                                    }

                                    dynamic paramPersonEmail = new
                                    {
                                        ContactMechanismId,
                                        ElectronicAddressString = electronicAddress.ElectronicAddress.AddressString,
                                        ElectronicAddressType = electronicAddress.ElectronicAddress.AddressType
                                    };

                                    RepositoryResponse PersonEmail = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, paramPersonEmail);
                                    if (PersonEmail.Id == 0)
                                    {
                                        repository.UnitOfWork.Rollback();
                                        response.ErrorMessage = "An error was encountered when creating an email address.";
                                        return response;
                                    }
                                }
                            }
                        }
                    }

                    #endregion UpdateUserLogin

                    #region Update User Type

                    processTracker = "Update User Type";
                    //Get the Current User Type
                    Guid realPageIdFrom = userProfile.RealPageId;
                    Guid realPageIdTo = organizationRealPageId;
                    string roleTypeName = null;
                    string relationshipTypeName = "User Type";

                    dynamic paramRelType = new
                    {
                        realPageIdFrom,
                        realPageIdTo,
                        roleTypeName,
                        relationshipTypeName
                    };
                    PartyRelationship relationshipType = repository.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, paramRelType);

                    int unlinkRoleTypeIdFrom = relationshipType.RoleTypeIdFrom;
                    int linkRoleTypeIdFrom = userTypeId;
                    int roleTypeIdTo = relationshipType.RoleTypeIdTo;

                    //Update the User Type  
                    if (unlinkRoleTypeIdFrom != linkRoleTypeIdFrom)
                    {
                        dynamic paramRole = new
                        {
                            personRealPageId,
                            organizationRealPageId,
                            unlinkRoleTypeIdFrom,
                            linkRoleTypeIdFrom,
                            roleTypeIdTo
                        };

                        RepositoryResponse RoleId = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, paramRole);
                        if (RoleId.Id == 0)
                        {
                            repository.UnitOfWork.Rollback();
                            response.ErrorMessage = "Unable to set new user type.";
                            return response;
                        }
                    }

                    #endregion

                    #region Persona Updates

                    processTracker = "Update Persona List";
                    long? personaTypeId = null;
                    string personaName = "";
                    //Update Persona
                    foreach (Persona userPersona in updatePersona)
                    {
                        //ADD
                        if (userPersona.PersonaId == 0)
                        {
                            personaId = null;
                            personaTypeId = 1;
                            if (userPersona.FromDate == null)
                            {
                                fromDate = utcNow;
                            }
                            else
                            {
                                fromDate = userPersona.FromDate;
                            }

                            thruDate = userPersona.ThruDate;

                            dynamic paramPersona = new
                            {
                                personRealPageId,
                                organizationRealPageId,
                                personaTypeId,
                                userPersona.PersonaEnvironmentTypeId,
                                fromDate,
                                thruDate,
                                personaId
                            };
                            RepositoryResponse personaResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, paramPersona);
                            if (personaResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                response.ErrorMessage = "Persona was not created.";
                                return response;
                            }

                            personaId = personaResponse.Id;
                            personaName = userPersona.Name;
                            personaTypeId = null;
                            dynamic paramPersonaType = new
                            {
                                personaName,
                                personaTypeId
                            };
                            RepositoryResponse personaTypeResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersonaType, paramPersonaType);
                            if (personaTypeResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                response.ErrorMessage = "Persona name: " + personaName + " was not created.";
                                return response;
                            }

                            personaTypeId = personaTypeResponse.Id;
                            dynamic paramAssocPersonaName = new
                            {
                                personaId,
                                userPersona.PersonaEnvironmentTypeId,
                                personaTypeId
                            };
                            RepositoryResponse personaUpdateResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, paramAssocPersonaName);
                            if (personaUpdateResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                response.ErrorMessage = "Persona name: " + personaName + " was not associated to the Persona.";
                                return response;
                            }
                        }
                        //UPDATE
                        else
                        {
                            personaId = userPersona.PersonaId;
                            personaTypeId = userPersona.PersonaTypeId;
                            if (userPersona.FromDate == null)
                            {
                                fromDate = utcNow;
                            }
                            else
                            {
                                fromDate = userPersona.FromDate;
                            }

                            thruDate = userPersona.ThruDate;

                            dynamic paramUpdatePersona = new
                            {
                                personaId,
                                personaTypeId,
                                userPersona.PersonaEnvironmentTypeId,
                                fromDate,
                                thruDate
                            };
                            RepositoryResponse personaUpdateResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, paramUpdatePersona);
                            personaName = userPersona.Name;
                            personaTypeId = null;
                            dynamic paramPersonaType = new
                            {
                                personaName,
                                personaTypeId
                            };
                            RepositoryResponse personaTypeResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersonaType, paramPersonaType);
                            if (personaTypeResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                response.ErrorMessage = "Persona name: " + personaName + " was not created.";
                                return response;
                            }

                            personaTypeId = personaTypeResponse.Id;
                            dynamic paramAssocPersonaName = new
                            {
                                personaId,
                                personaTypeId,
                                userPersona.PersonaEnvironmentTypeId //pass this again because it is being set as default
                            };
                            RepositoryResponse personaUpdateNameResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, paramAssocPersonaName);
                            if (personaUpdateNameResponse.Id == 0)
                            {
                                repository.UnitOfWork.Rollback();
                                response.ErrorMessage = "Persona name: " + personaName + " was not associated to the Persona.";
                                return response;
                            }
                        }
                    }

                    //DELETE
                    Persona inactivePersona = new Persona();
                    foreach (Persona userPersona in deletePersona)
                    {
                        personaId = userPersona.PersonaId;
                        dynamic paramPersona = new
                        {
                            personaId
                        };
                        inactivePersona = repository.GetOne<Persona>(StoredProcNameConstants.SP_RemovePersona, paramPersona);
                    }

                    #endregion

                    repository.UnitOfWork.Commit();
                    response.RealPageId = userProfile.RealPageId;
                }
                catch (Exception exception)
                {
                    repository.UnitOfWork.Rollback();
                    response.ErrorMessage = "Update User Error: " + exception.Message + ". Process: " + processTracker;
                    return response;
                }

                return response;
            }
        }

        /// <summary>
        /// Update user login
        /// </summary>
        /// <param name="realPageId">enterprise User Id</param>
        /// <param name="orgPartyId">Org Party ID</param>
        /// <param name="loginId">loginId</param>
        /// <param name="isActive">isActive</param>
        /// <param name="passwordHash">passwordHash</param>
        /// <param name="passwordSalt">passwordSalt</param>
        /// <param name="isLocked">isLocked</param>
        /// <param name="isTainted">isTainted</param>
        /// <param name="fromDate">fromDate</param>
        /// <param name="thruDate">thruDate</param>

        /// <returns>UserLogin object</returns>
        public UserLogin UpdateUserLogin(Guid realPageId, long orgPartyId, string loginId = null, bool? isActive = null,
            string passwordHash = null, string passwordSalt = null, bool? isLocked = null, bool? isTainted = null, DateTime? fromDate = null, DateTime? thruDate = null)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<UserLogin>(StoredProcNameConstants.SP_UpdateUserLogin, new
                {
                    RealPageId = realPageId,
                    LoginId = loginId,
                    IsActive = isActive,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    IsLocked = isLocked,
                    IsTainted = isTainted,
                    FromDate = fromDate,
                    ThruDate = thruDate,
                    PartyId = orgPartyId
                });
            }
        }

        /// <summary>
        /// Used to disable products for the given list of users
        /// </summary>
        /// <param name="createUserRealPageId"></param>
        /// <param name="createUserPersonaId"></param>
        /// <param name="userLogins"></param>
        public void DisableUserProduct(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins)
        {
            Parallel.ForEach(userLogins, new ParallelOptions { MaxDegreeOfParallelism = 5 }, userLoginOnly => { DisableUserProductData(createUserRealPageId, createUserPersonaId, userLoginOnly); });
        }

        private void DisableUserProductData(Guid createUserRealPageId, long createUserPersonaId, UserLoginOnly user)
        {
            IUserLoginRepository userLoginRepository = new UserLoginRepository();
            IManagePersona managePersona = new ManagePersona(_userClaim);
            IManageOrganization manageOrganization = new ManageOrganization(_userClaim);

            var userLoginOnly = userLoginRepository.GetUserLoginOnly(user.RealPageId);
            var userPersonaOrganizationList = userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);
            var currentPrimaryOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }

            Persona persona = null;
            Persona adminPersona = null;
            if (userPersonaOrganizationList == null || currentPrimaryOrgStatus == null)
            {
                return;
            }

            if (_userClaim.OrganizationPartyId == currentPrimaryOrgStatus.PartyId && userPersonaOrganizationList.Count > 1)
            {
                foreach (UserOrganization userOrg in userPersonaOrganizationList)
                {
                    var currentOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, userOrg.OrganizationPartyId, false);
                    if (currentOrgStatus.Status == UserUiStatusType.Disabled)
                    {
                        persona = managePersona.GetFirstAvailablePersonaByCompany(userLoginOnly.RealPageId, userOrg.OrganizationPartyId);
                        Guid realPageEmployeeAccessID = manageOrganization.GetOrganizationAdminUserRealPageId(userOrg.OrganizationRealPageId);
                        adminPersona = managePersona.GetFirstAvailablePersonaByCompany(realPageEmployeeAccessID, userOrg.OrganizationPartyId);

                        using (var repository = GetRepository())
                        {
                            if (_userClaim.OrganizationPartyId == adminPersona.OrganizationPartyId)
                            {
                                ProcessDisableUserProductData(repository, persona.PersonaId, _userClaim.UserRealPageGuid, _userClaim.PersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId);
                            }
                            else
                            {
                                ProcessDisableUserProductData(repository, persona.PersonaId, realPageEmployeeAccessID, adminPersona.PersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId);
                            }
                        }
                    }
                }
            }
            else
            {
                var currentOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, _userClaim.OrganizationPartyId, false);
                if (currentOrgStatus.Status == UserUiStatusType.Disabled)
                {
                    persona = managePersona.GetFirstAvailablePersonaByCompany(userLoginOnly.RealPageId, _userClaim.OrganizationPartyId);
                    using (var repository = GetRepository())
                    {
                        ProcessDisableUserProductData(repository, persona.PersonaId, createUserRealPageId, createUserPersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId);
                    }
                }
            }
        }

        /// <summary>
        /// Used to activate products for the given list of users whom previously disabled
        /// </summary>
        /// <param name="createUserRealPageId"></param>
        /// <param name="createUserPersonaId"></param>
        /// <param name="userLogins"></param>
        public void ActivateUserProducts(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins)
        {
            //IManageUserLogin userLoginLogic = new ManageUserLogin();
            IManagePersona managePersona = new ManagePersona(_userClaim);

            using (var repository = GetRepository())
            {
                foreach (UserLoginOnly ul in userLogins)
                {
                    //Persona persona = managePersona.GetActivePersona(ul.RealPageId);
                    Persona persona = managePersona.GetFirstAvailablePersonaByCompany(ul.RealPageId, _userClaim.OrganizationPartyId);

                    ProcessActivatedUserProductBatchData(persona.PersonaId, createUserRealPageId, createUserPersonaId);
                }
            }
        }

        /// <summary>
        /// Give administrators access to missing products based on a customer company
        /// </summary>
        /// <param name="organizationRealPageId">Organization enterprise Id</param>
        /// <param name="assignUserPersonaId">Assigned to user PersonaId</param>
        public void AssignProductsToAdministrators(Guid organizationRealPageId, long assignUserPersonaId = 0)
        {
            if (organizationRealPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter organization realPageId.");
            }

            int totalProductCount = 0;
            long createUserPersonaId = 0;
            bool? IsDefault = null;
            int adminsUpdated = 0;

            IList<string> aoProductsAvailableForUser = null;
            Guid RealPageEmployeeAccessID = Guid.Empty;

            // if company has AO product assigned then get products available to assign based on editor User
            aoProductsAvailableForUser = GetEditorUserAoProduct(_userClaim.UserRealPageGuid, _userClaim.PersonaId, organizationRealPageId);
            IOrganizationRepository organizationRepository = new OrganizationRepository();
            Organization organization = organizationRepository.GetOrganization(organizationRealPageId);
            IPersonaRepository personaRepository = new PersonaRepository(_userClaim);
            IList<Persona> personaList = personaRepository.ListPersonaByOrganizationPartyId(organization.PartyId, IsDefault, (int)UserRoleType.SuperUser);
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }
            if (assignUserPersonaId > 0)
            {
                personaList = personaList.Where(p => p.PersonaId == assignUserPersonaId).ToList();
            }

            using (IRepository repository = GetRepository())
            {
                dynamic result = repository.GetOne<dynamic>(StoredProcNameConstants.SP_ListOrganizations, new { RealPageId = organizationRealPageId });
                if (result != null)
                {
                    //Found the RealPage Employee Access for the Organization.
                    RealPageEmployeeAccessID = new Guid(result.PersonRealPageId.ToString());
                    //Use RealPage Employee Access PersonaId when creating the Product Patches.
                    createUserPersonaId = repository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = RealPageEmployeeAccessID });

                    personaList.ToList().ForEach(o =>
                    {
                        IList<ProductBatch> productList = new List<ProductBatch>();
                        dynamic param = new
                        {
                            UserLoginId = o.UserId,
                            OrganizationPartyId = organization.PartyId
                        };
                        IList<UserLoginPersona> userLoginPersonaList = repository.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona, param);
                        if (userLoginPersonaList != null)
                        {
                            if (!(userLoginPersonaList[0].StatusTypeId == 23 || userLoginPersonaList[0].StatusTypeId == 24))
                            {
                                // don't update disabled or expired admins
                                var productCount = SaveProductDetails(repository, productList, null, createUserPersonaId, o.PersonaId, RealPageEmployeeAccessID, organizationRealPageId, null, (int)UserRoleType.SuperUser, true, impersonatorUserLoginOnly.UserId, aoProductsAvailableForUser, false, false);
                                adminsUpdated++;
                                totalProductCount = productCount > totalProductCount ? productCount : totalProductCount;
                            }
                        }
                    }
                    );
                    if ((adminsUpdated > 0) && (totalProductCount > 0))
                    {
                        string logMessage = $"{{0}} {{1}} performed Refresh Admin Users in {result.Name} for {adminsUpdated} ";
                        logMessage += (adminsUpdated > 1) ? "administrators" : "administrator";
                        logMessage += $"; at most {totalProductCount} new product user";
                        logMessage += (totalProductCount > 1) ? "s were created" : " was created.";
                        LogActivity.WriteActivity(new ActivityDetails
                        {
                            LogActivityTypeName = LogActivityTypeConstants.PRODUCT_ACCESS,
                            LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                            CorrelationId = _userClaim.CorrelationId.ToString(),
                            BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                            OrganizationPartyId = _userClaim.OrganizationPartyId,
                            Message = string.Format(logMessage, _userClaim.FirstName, _userClaim.LastName),
                            FromUserLoginName = _userClaim.LoginName,
                            FromUserLoginId = _userClaim.UserId,
                            FromUserFirstName = _userClaim.FirstName,
                            FromUserLastName = _userClaim.LastName,
                            FromUserRealpageId = _userClaim.UserRealPageGuid.ToString()
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Used to activate salesforce users who where disabled prev
        /// </summary>
        /// <param name="createUserRealPageId"></param>
        /// <param name="createUserPersonaId"></param>
        /// <param name="userLogins"></param>
        /// <param name="isAssigned"></param>
        public void ActivateSalesForceUser(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins, bool isAssigned)
        {
            string saveProductBatchError = "Save Product(s) Error: ";

            CreateUserResponse<IErrorData> createUserResponse = new CreateUserResponse<IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            using (var repository = GetRepository())
            {
                UserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
                if (_userClaim.ImpersonatedBy != Guid.Empty)
                {
                    impersonatorUserLoginOnly = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = _userClaim.ImpersonatedBy });
                }

                foreach (UserLoginOnly ul in userLogins)
                {
                    var userLogin = _userLoginRepository.GetUserLoginOnly(ul.RealPageId);
                    Persona editorPersona = _managePersona.GetPersona(createUserPersonaId);

                    //Persona persona = managePersona.GetActivePersona(ul.RealPageId);
                    var personaList = _managePersona.ListPersona(ul.RealPageId);
                    Persona persona = personaList.FirstOrDefault(p => p.OrganizationPartyId == editorPersona.OrganizationPartyId);

                    IList<ProductBatch> productListToActivate = new List<ProductBatch>();

                    if (productListToActivate != null)
                    {
                        //if (!productListToActivate.Any(p => p.ProductId == (int)ProductEnum.ClientPortal))
                        {
                            if (!(persona.UserTypeId == (int)UserRoleType.UserNoEmail))
                            {
                                // check salesforce contact for  all disabled users and set UnifiedLoginUser to false
                                ProductBatch pb = new ProductBatch()
                                {
                                    ProductId = (int)ProductEnum.SalesForce,
                                    StatusTypeId = 5,
                                    RetryCount = 0,
                                    InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = isAssigned }
                                };
                                productListToActivate.Add(pb);
                            }
                        }
                    }

                    if ((productListToActivate.Count > 0))
                    {
                        //Do we have the Create & Assign PersonaIds
                        if ((createUserPersonaId > 0) && (persona.PersonaId > 0))
                        {
                            //Loop through the rest of the products list and create the Batch records
                            foreach (IProductBatch product in productListToActivate)
                            {
                                SaveProductBatch(repository, product, createUserResponse, saveProductBatchError,
                                    createUserPersonaId, persona.PersonaId, createUserRealPageId, errorStatus,
                                    JsonConvert.SerializeObject(product.InputJson), impersonatorUserLoginOnly.UserId);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get enterprise user
        /// </summary>
        /// <param name="realPageId">enterprise User Id</param>
        /// <returns>UserLogin object</returns>
        public UserLogin GetEnterpriseUser(Guid realPageId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update User Detail and Products
        /// </summary>
        /// <param name="loggedInUserRealPageId">Logged-In User unique identifier</param>
        /// <param name="newProfile">Edited User detail and Products</param>
        /// <param name="oldProfile">Old detail profile from database</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateUser(Guid loggedInUserRealPageId, IProfileDetail newProfile, IProfileDetail oldProfile)
        {
            long createUserPersonaId = 0L;

            string saveProductBatchError = "Save Product(s) Error: ";
            bool isCurrentOrgThePrimaryOrg = false;

            IUserRoleRightRepository userRoleRightRepository = new UserRoleRightRepository();
            IUserLoginRepository userLoginRepository = new UserLoginRepository();
            IPersonaRepository personaRespository = new PersonaRepository(_userClaim);
            IOrganizationRepository organizationRepository = new OrganizationRepository();
            IContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();

            IManagePersona managePersona = new ManagePersona(_userClaim);
            IList<IdentityProviderType> identityProviderTypeList = new List<IdentityProviderType>();
            IList<ProductBatch> productBatchData = new List<ProductBatch>();
            IList<ContactMechanismUsageType> emailUsageType = new List<ContactMechanismUsageType>();

            //Notification Email
            IContactMechanismRepository contactMechanismRepository = new ContactMechanismRepository();
            //BlueBook MasterId for External Users
            Organization organizationExternalUser = organizationRepository.GetOrganization(realPageId: DefaultUserClaim.ExternalCompanyRealPageId);

            IUserLoginOnly userLoginOnly = userLoginRepository.GetUserLoginOnly(newProfile.RealPageId);

            IList<UserOrganization> userPersonaOrganizationList = new List<UserOrganization>();
            userPersonaOrganizationList = userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);

            //Get the edited user Current Persona Id

            bool userIsExternalEverywhere = userPersonaOrganizationList.ToList().All(x => x.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser));

            if ((oldProfile.Persona[0] != null) && (oldProfile.Persona[0].Organization != null))
            {
                //Get the Organization IDP
                identityProviderTypeList = organizationRepository.GetOrganizationIdentityProviderType(oldProfile.Persona[0].Organization.RealPageId);
            }

            // get the user's existing List of UnifiedLogin roles
            var existingRoleIds = userRoleRightRepository.GetRoleIdsByPersona(oldProfile.Persona[0].PersonaId, (int)ProductEnum.UnifiedPlatform);

            OrganizationStatus currentPrimaryOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(newProfile.userLogin.UserId, userLoginOnly.LastLogin, 0, true);
            OrganizationStatus currentOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(newProfile.userLogin.UserId, userLoginOnly.LastLogin, oldProfile.Persona[0].OrganizationPartyId, false);


            //Get the logged-in user Current Persona Id
            createUserPersonaId = personaRespository.GetActivePersonaId(realPageId: loggedInUserRealPageId);

            IList<Persona> personaList = personaRespository.ListActivePersona(newProfile.RealPageId, true);
            if (!currentOrgStatus.PrimaryOrganization)
            {
                personaList = personaList.Where(p => p.OrganizationPartyId == currentOrgStatus.PartyId).ToList();
            }

            IList<string> aoProductsAvailableForUser = null;
            // Get AO roles before transaction scope begins
            if (newProfile.UserTypeId == (int)UserRoleType.SuperUser)
            {
                // if company has AO product assigned then get products available to assign based on editor User
                aoProductsAvailableForUser = GetEditorUserAoProduct(loggedInUserRealPageId, createUserPersonaId, oldProfile.Persona[0].Organization.RealPageId);
            }

            emailUsageType = contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName: "Email Notification");

            var enterpriseRole = newProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
            int enterpriseRoleId = newProfile.RoleTemplateId;
            if (enterpriseRole?.InputJson?.RoleList != null && enterpriseRole?.InputJson?.RoleList.Count > 0)
            {
                enterpriseRoleId = Convert.ToInt32(enterpriseRole.InputJson.RoleList.FirstOrDefault());
            }

            //TODO:CG.WE CAN DELETEDE DE USERDETAILS BECAUSE WE ARE SENDING THE OLDPROFILE
            //Get User Details before save
            var userDetails = GetUserDetails(oldProfile.Persona[0].PersonaId);

            //If user is activated from deactivate status ,get all products data which are previously assigned to user
            if (newProfile.userLogin.Status == UserUiStatusType.Disabled && enterpriseRoleId == 0)
            {
                productBatchData = GetActivatedUserProductBatchData(oldProfile.Persona[0].PersonaId, newProfile.productBatch);
            }
            else
            {
                productBatchData = newProfile.productBatch;
            }

            //Get Current user state before any update
            var primaryOrg = organizationRepository.GetOrganization(realPageId: currentPrimaryOrgStatus.RealPageId);
            var currentOrg = organizationRepository.GetOrganization(organizationPartyId: oldProfile.Persona[0].OrganizationPartyId);

            isCurrentOrgThePrimaryOrg = primaryOrg.PartyId.Equals(_userClaim.OrganizationPartyId);

            ProductBatch primaryPropertyBatch = null;
            if (currentOrg.EnablePrimaryProperties == 1)
            {
                primaryPropertyBatch = newProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);
            }

            //Update profile (First, Last names, ...) required data per Organization
            IList<EditorAssignedPersona> editorAssignedPersonaList = new List<EditorAssignedPersona>();
            IList<UserOrganization> userOrganizationList = userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);
            userOrganizationList.ToList().ForEach(o =>
            {
                if (!o.BooksCustomerMasterId.Equals(-1) && !o.BooksMasterId.Equals(-1))
                {
                    Guid realPageEmployeeAccessId = _organizationRepository.GetOrganizationAdminUserRealPageId(o.OrganizationRealPageId);
                    Persona editorPersona = managePersona.GetFirstAvailablePersonaByCompany(realPageEmployeeAccessId, o.OrganizationPartyId);

                    //asigned persona
                    Persona assignedPersona = managePersona.GetFirstAvailablePersonaByCompany(newProfile.RealPageId, o.OrganizationPartyId);

                    editorAssignedPersonaList.Add(
                        new EditorAssignedPersona()
                        {
                            AssignedPersonaId = assignedPersona.PersonaId,
                            AssignedUserTypeId = assignedPersona.UserTypeId.Value,
                            EditorPersonaId = editorPersona.PersonaId,
                            EditorPersonaRealPageId = editorPersona.RealPageId,
                            OrganizationRealPageId = o.OrganizationRealPageId
                        }
                    );
                }
            });


            // primary properties selected
            if (primaryPropertyBatch != null && enterpriseRoleId == 0)
            {
                IPropertyRepository propertyRepository = new PropertyRepository();
                // get persona primary properties
                List<string> currentprimaryProperties = new List<string>();

                List<UPFMPropertyInstance> ulPropertyInstances = new List<UPFMPropertyInstance>();
                ulPropertyInstances = propertyRepository.ListUPFMPropertyInstanceByPersona(oldProfile.Persona[0].PersonaId, ProductEnum.UnifiedPlatform);

                foreach (var property in ulPropertyInstances)
                {
                    currentprimaryProperties.Add(Convert.ToString(property.InstanceId));
                }


                if (currentprimaryProperties?.Count > 0 && primaryPropertyBatch.InputJson.PropertyList?.Count > 0)
                {
                    productBatchData = UpdateProductBatchDataWithPrimaryProperties(createUserPersonaId, oldProfile.Persona[0].PersonaId, primaryPropertyBatch, productBatchData.ToList(), currentprimaryProperties);
                }

            }

            UpdateUserProfileEntity updateUserProfileEntity = new UpdateUserProfileEntity
            {
                LoggedInUserRealPageId = loggedInUserRealPageId,
                NewProfile = newProfile,
                OldProfile = oldProfile,
                CreateUserPersonaId = createUserPersonaId,
                SaveProductBatchError = saveProductBatchError,
                IsCurrentOrgThePrimaryOrg = isCurrentOrgThePrimaryOrg,
                IdentityProviderTypeList = identityProviderTypeList,
                ProductBatchData = productBatchData,
                EmailUsageType = emailUsageType,
                OrganizationExternalUser = organizationExternalUser,
                UserLoginOnly = userLoginOnly,
                UserPersonaOrganizationList = userPersonaOrganizationList,
                ExistingRoleIds = existingRoleIds,
                CurrentPrimaryOrgStatus = currentPrimaryOrgStatus,
                CurrentOrgStatus = currentOrgStatus,
                PersonaList = personaList,
                AoProductsAvailableForUser = aoProductsAvailableForUser,
                UserIsExternalEverywhere = userIsExternalEverywhere,
                CurrentOrg = currentOrg,
                EditorAssignedPersonaList = editorAssignedPersonaList
            };

            return UpdateUserData(updateUserProfileEntity);
        }

        /// <summary>
        /// Get User Details by Persona Id
        /// </summary>
        public UserDetails GetUserDetails(long? personaId = null, string userRealPageId = null)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<UserDetails>(StoredProcNameConstants.SP_GetUserDetails,
                    new { personaId, userRealPageId });
            }
        }

        /// <summary>
        /// Get User Details by Persona Id
        /// </summary>
        public long GetSuperUserCountByOrganizationAsync(long? OrganizationPartyId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<long>(StoredProcNameConstants.SP_GetSuperUsersCountByOrganization,
                    new { OrganizationPartyId });
            }
        }

        /// <summary>
        /// Used to disable products for the given list of users which is called from windows service
        /// </summary>		
        /// <param name="userLogins"></param>
        public void ProcessDisabledUsers(IList<ProcessUserLogin> userLogins)
        {
            //IManageUserLogin userLoginLogic = new ManageUserLogin();
            DefaultUserClaim currentUserClaim = new DefaultUserClaim(ClaimsPrincipal.Current);
            var logData = new Dictionary<string, object>();
            IManagePersona managePersona = new ManagePersona();
            IManagePerson managePerson = new ManagePerson();
            Dictionary<Guid, Persona> companyAdminList = new Dictionary<Guid, Persona>();
            RepositoryResponse updateUserStatusResponse = new RepositoryResponse();
            logData = new Dictionary<string, object> { { "userLogins", userLogins } };
            var profileLogic = new ManageProfile(_userClaim);
            DateTime? thruDateCST = null;
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "ProcessDisabledUsers", "At beginning of method for user with json" });
            using (var repository = GetRepository())
            {
                foreach (ProcessUserLogin ul in userLogins)
                {
                    var userLoginRepository = new UserLoginRepository();
                    IUserLoginOnly userLoginOnly = userLoginRepository.GetUserLoginOnly(ul.UserRealPageId);
                    var organization = _organizationRepository.GetOrganization(realPageId: ul.OrganizationRealPageId);
                    IPerson person = managePerson.GetPerson(ul.UserRealPageId);
                    var userOrganizationList = userLoginRepository.ListAllOrganizationByLoginName(userLoginOnly.LoginName);
                    Guid primaryCompanyGuid = userOrganizationList.FirstOrDefault(p => p.PrimaryOrganization).OrganizationRealPageId;
                    List<Guid> organizationsToProcess = new List<Guid>();

                    //since windows service doesn't have editor persona,Get RealPageEmployeeAccessID to use in to get editor persona
                    currentUserClaim = GetCurrentUserClaim(profileLogic, organization);

                    logData = new Dictionary<string, object> { { "userLogins", userLoginOnly }, { "userOrganizationList", userOrganizationList } };
                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "ProcessDisabledUsers", $"Getting info for process to disable User with login name {userLoginOnly.LoginName} and user realpageId {ul.UserRealPageId}" });

                    foreach (var org in userOrganizationList)
                    {
                        Persona editorPersona = null;
                        updateUserStatusResponse = new RepositoryResponse();
                        long orgPartyId = userOrganizationList.FirstOrDefault(uo => uo.OrganizationRealPageId == ul.OrganizationRealPageId).OrganizationPartyId;
                        IUserLogin userLogin = userLoginRepository.GetUserLogin(ul.UserRealPageId, orgPartyId);
                        bool isUserdisabled = userLogin.StatusId == (int)UserUiStatusType.Disabled;

                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "ProcessDisabledUsers", $"Verifying user status is {isUserdisabled} for OrganizationPartyId {orgPartyId}" });

                        if (!companyAdminList.ContainsKey(org.OrganizationRealPageId))
                        {
                            //since windows service doesn't have editor persona,Get RealPageEmployeeAccessID to use in to get editor persona and save it for later calls
                            dynamic param = new
                            {
                                RealPageId = org.OrganizationRealPageId
                            };
                            var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, param);

                            if (result != null)
                            {
                                foreach (var item in result)
                                {
                                    Guid realPageEmployeeAccessId = new Guid(Convert.ToString(item.PersonRealPageId));
                                    editorPersona = managePersona.GetFirstAvailablePersonaByCompany(realPageEmployeeAccessId, item.PartyId);
                                }

                                companyAdminList.Add(org.OrganizationRealPageId, editorPersona);
                            }
                        }
                        else
                        {
                            editorPersona = companyAdminList[org.OrganizationRealPageId];
                        }

                        // eventually this will be a list of personas when we start doing multi persona
                        var persona = managePersona.GetFirstAvailablePersonaByCompany(ul.UserRealPageId, org.OrganizationPartyId);

                        //Check if user primary company and current company in loop are same
                        if (!isUserdisabled && ul.OrganizationRealPageId == primaryCompanyGuid)
                        {
                            updateUserStatusResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany, new
                            {
                                RealPageId = ul.UserRealPageId,
                                OrganizationPartyId = org.OrganizationPartyId,
                                StatusTypeId = UserUiStatusType.Disabled,
                                FromDate = ul.FromDate
                            });
                        }
                        else if (!isUserdisabled && ul.OrganizationRealPageId == org.OrganizationRealPageId)
                        {
                            updateUserStatusResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany, new
                            {
                                RealPageId = ul.UserRealPageId,
                                OrganizationPartyId = orgPartyId,
                                StatusTypeId = UserUiStatusType.Disabled,
                                FromDate = ul.FromDate
                            });
                        }

                        logData = new Dictionary<string, object> { { "updateUserStatusResponse", updateUserStatusResponse } };
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "ProcessDisabledUsers", $"Verifying update user status response {updateUserStatusResponse.Id}" });

                        if (updateUserStatusResponse.Id > 0)
                        {
                            thruDateCST = userLogin.ThruDate != null ? TimeZoneInfo.ConvertTime(Convert.ToDateTime(userLogin.ThruDate), TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")) : userLogin.ThruDate;
                            string message = $"{person.FirstName} {person.LastName} was deactivated by the system due to the scheduled User Expires date. | {(thruDateCST != null ? thruDateCST.Value.ToShortDateString() + "/ " + thruDateCST.Value.ToShortTimeString() : string.Empty)} CST";
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "ProcessDisabledUsers", $"Calling AddActivityLog method for activity - {message}" });
                            AddActivityLog(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, message, person, userLoginOnly, org, currentUserClaim);
                            // send message to kafka topic.
                            UserDetails userDetailsInfo = new UserDetails();
                            userDetailsInfo = GetUserDetails(userRealPageId: userLogin.RealPageId.ToString());
                            IUserLoginPersonaRepository userLoginPersonaRepository = new UserLoginPersonaRepository();
                            IList<UserLoginPersona> userLoginPersonaList = userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId: null, userLoginId: userDetailsInfo.UserId, organizationPartyId: userDetailsInfo.OrganizationPartyId);
                            var primaryOrgPersona = userLoginPersonaList.Where(x => x.PrimaryOrganization == true).FirstOrDefault();
                            if (primaryOrgPersona != null && userDetailsInfo != null
                                && userDetailsInfo.UserRoleTypeId != UserTypeConstants.RegularUserNoEmail
                                && !userDetailsInfo.IsRPEmployee
                                && !userDetailsInfo.LoginName.Equals($"{userDetailsInfo.BooksMasterId}admin@realpage.com", StringComparison.OrdinalIgnoreCase))

                            {
                                var kafkaProducer = KafkaProducerServiceFactory.Instance;
                                kafkaProducer.PublishUserStatusChangeEventAsync(new UnifiedLoginUserStatusEvent
                                {
                                    persona_id = userDetailsInfo.PersonaId,
                                    user_login_name = userDetailsInfo.LoginName,
                                    is_active = false,
                                    user_activation_deactivation_date = DateTime.UtcNow
                                }).ConfigureAwait(false);
                            }
                        }

                        //remove products
                        if (editorPersona != null && (ul.OrganizationRealPageId == primaryCompanyGuid || ul.OrganizationRealPageId == org.OrganizationRealPageId))
                        {
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "ProcessDisabledUsers", $"Calling ProcessDisableUserProductData" });
                            ProcessDisableUserProductData(repository, persona.PersonaId, editorPersona.RealPageId, editorPersona.PersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId);
                        }
                    }
                }
            }
        }


		/// <summary>
		/// Used to bulk update Third-Party IDP flag for the given list of users
		/// </summary>  
		/// <param name="userIds">List of user IDs to update</param>
		/// <param name="isEnabled">Enable or disable Third-Party IDP</param>
		public RepositoryResponse ThirdPartyIdpBulkUpdate(IList<long> userIds, bool isEnabled)
		{
			try
			{
				List<long> userUpdateResponse = new List<long>();
				if (userIds.Count > 0)
				{
					using (var repository = GetRepository())
					{
						dynamic param = new
						{
							OrganizationPartyId = _userClaim.OrganizationPartyId,
							UserIds = TableValueParamHelper.ConvertToTableValuedParameter(userIds.ToList(), "Enterprise.BigIntListType"),
							IsEnabled = isEnabled
						};
						userUpdateResponse = repository.GetMany<long>(StoredProcNameConstants.SP_UpdateUsersIDP, param);
					}
					if (userUpdateResponse.Count > 0)
					{
						ActivityLogForBulkIDPUpdate(userUpdateResponse, isEnabled);
					}
				}
				return new RepositoryResponse();
			}
			catch (Exception ex)
			{
				WriteToLog(LogEventLevel.Error, message: "{ActionName} - {state}", logData: null,exception: ex, messageProperties: new object[] { "ThirdPartyIdpBulkUpdate", $"Unable to perform bulk Third-Party Identity Provider update: {string.Join(",", userIds)}" });
				return new RepositoryResponse() { ErrorMessage = "Unable to perform bulk Third-Party Identity Provider update." };
			}
		}

		/// <summary>
		/// Log activities for bulk IDP update
		/// </summary>
		/// <param name="userIds"></param>
		/// <param name="isEnabled"></param>
		private void ActivityLogForBulkIDPUpdate(IList<long> userIds, bool isEnabled)
		{
			List<UserActivityLogInfo> usersList = new List<UserActivityLogInfo>();
			//get the users
			using (var repository = GetRepository())
			{
				dynamic param = new
				{
					OrganizationPartyId = _userClaim.OrganizationPartyId,
					UserIds = TableValueParamHelper.ConvertToTableValuedParameter(userIds.ToList(), "Enterprise.BigIntListType")
				};

				usersList = repository.GetMany<UserActivityLogInfo>(StoredProcNameConstants.SP_GetUserProfileByUserIds, param);

			}
			foreach (var user in usersList)
			{
				IProfileDetail newProfile = new ProfileDetail
				{
					RealPageId = user.RealPageId,
					FirstName = user.FirstName,
					LastName = user.LastName,
					userLogin = new UserLogin
					{
						UserId = user.UserId,
						LoginName = user.LoginName,
						RealPageId = user.RealPageId

					},
					// Fix: user.CreateUserSourceType is a string, so do not cast to int
					CreateUserSourceType = !string.IsNullOrEmpty(user.CreateUserSourceType) ? (CreateUserSourceType)Enum.Parse(typeof(CreateUserSourceType), user.CreateUserSourceType) : CreateUserSourceType.UnifiedPlatform
				};
				string status = isEnabled ? "enabled" : "disabled";
				var message = !string.IsNullOrEmpty(_userClaim.ImpersonatedByName) ? $"RealPage Access ({_userClaim.ImpersonatedByName}) {status} Third-Party Identity Provider flag for user {newProfile.FirstName} {newProfile.LastName}" : $"{_userClaim.FirstName} {_userClaim.LastName} {status} Third-Party Identity Provider flag for user {newProfile.FirstName} {newProfile.LastName}";
				AuditActivityLog((!isEnabled).ToString(), isEnabled.ToString(), "Third-Party Identity Provider", message, newProfile);
			}
		}

		/// <summary>
		/// Update user Employee Id
		/// </summary>
		/// <param name="employeeIdDetail"></param>
		/// <returns></returns>
		public RepositoryResponse UpdateUserEmployeeId(IUserEmployeeId employeeIdDetail)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            if (employeeIdDetail.UserEmployeeId > 0)
            {
                using (var repository = GetRepository())
                {
                    dynamic param = new
                    {
                        employeeIdDetail.UserEmployeeId,
                        employeeIdDetail.EmployeeId
                    };

                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateEmployeeId, param);

                }
            }

            return repositoryResponse;
        }

        /// <summary>
        /// Get the an UserEmployee by UserLoginPersonaId and OrganizationPartyId
        /// </summary>
        /// <param name="UserLoginPersonaId"></param>
        /// <param name="OrganizationPartyId"></param>
        /// <returns>IUserEmployeeId</returns>
        public IUserEmployeeId GetUserEmployeeId(long UserLoginPersonaId, long OrganizationPartyId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    UserLoginPersonaId,
                    OrganizationPartyId
                };

                return repository.GetOne<UserEmployee>(StoredProcNameConstants.SP_GetEmployeeId, param);
            }
        }

        /// <summary>
        /// Get SuperVisor Information by UserId and OrganizationPartyId
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="OrganizationPartyId"></param>
        /// <returns>IUserEmployeeId</returns>
        public UserInfoLite GetSuperVisorInformation(long UserId, long OrganizationPartyId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    UserId = UserId,
                    OrgPartyId = OrganizationPartyId
                };

                return repository.GetOne<UserInfoLite>(StoredProcNameConstants.SP_GetSuperVisorId, param);
            }
        }

        public List<int> GetDelegateAdminRoleTemplate(long UserLoginPersonaId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    UserLoginPersonaId
                };
                var roleslists = new List<int>();
                var procName = StoredProcNameConstants.SP_GetEnterpriseDelegateRole;
                var result = repository.GetMany<dynamic>(procName, param);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        roleslists.Add(item.RoleTemplateId);
                    }
                }
                return roleslists;
            }
        }


        /// <summary>
        /// Gets the list of possible navigation menu entries
        /// </summary>
        /// <returns>A list of navigation menu entries</returns>
        public IList<NavigationMenuEntry> GetNavigationMenu()
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"navigationMenuEntries";

            return rpCache.GetFromCache(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    return repository.GetMany<NavigationMenuEntry>(StoredProcNameConstants.SP_GetNavigationMenu, null).ToList();
                }
            });
        }

        /// <summary>
        /// Gets the list of rights for specific navigation menu entries
        /// </summary>
        /// <returns>A list of rights/menu ids</returns>
        public IList<NavigationMenuRightEntry> GetNavigationMenuRights()
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"navigationMenuRightEntries";

            return rpCache.GetFromCache(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    return repository.GetMany<NavigationMenuRightEntry>(StoredProcNameConstants.SP_GetNavigationMenuRights, null).ToList();
                }
            });
        }

        public AdUserDetail GetAzureUserDetails(long userId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<AdUserDetail>(StoredProcNameConstants.SP_GetADDetailsForUser, new { userId });
            }
        }

        /// <summary>
        /// Used to get the list of products and the adgroup assigned to it for the given persona
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IList<EmployeeProductMapping> GetEmployeeProductADGroupMapping(long personaId, int productId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    ProductId = productId,
                    PersonaId = personaId
                };
                return repository.GetMany<EmployeeProductMapping>(StoredProcNameConstants.SP_GetEmployeeProductADGroupMapping, param);
            }
        }

        /// <summary>
        /// Used to add or update an employees product to adgroup user mapping
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="productId"></param>
        /// <param name="adGroupId"></param>
        /// <returns></returns>
        public RepositoryResponse AddUpdateEmployeeProductADGroupMapping(long personaId, int productId, int adGroupId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    ProductId = productId,
                    PersonaId = personaId,
                    ADGroupId = adGroupId
                };
                return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateEmployeeProductADGroupMapping, param);
            }
        }

        public IList<NavigationMenuSetting> GetNavigationMenuSettingsUnaccessable(long partyId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetMany<NavigationMenuSetting>(StoredProcNameConstants.SP_GetNavigationMenuSettingUnaccessable, new { partyId }).ToList();
            }
        }

        public ExternalUserRelationship GetExternalUserRelationship(long userLoginPersonaId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship, new { userLoginPersonaId });
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// List ContactMechanism For Person
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="realPageId"></param>
        /// <param name="contactMechanismUsageTypeList"></param>
        /// <returns>list of CommonAddress</returns>
        private IList<CommonAddress> ListContactMechanismForPerson(IRepository repository, Guid realPageId, IList<ContactMechanismUsageType> contactMechanismUsageTypeList)
        {
            IList<CommonAddress> result = repository.GetMany<CommonAddress>(StoredProcNameConstants.SP_ListContactMechanismsForPerson, new { realPageId }).ToList();
            if (result != null)
            {
                foreach (var contactMechanism in result)
                {
                    var usageType = contactMechanismUsageTypeList.FirstOrDefault(i => i.ContactMechanismUsageTypeId == contactMechanism.ContactMechanismUsageTypeId);
                    if (usageType != null)
                    {
                        contactMechanism.contactMechanismUsageType = usageType;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Update Profile
        /// </summary>
        /// <param name="repository">repositoryr</param>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">profile object of the parameter values</param>
        /// <returns>Repository response object</returns>
        private RepositoryResponse UpdateProfile(IRepository repository, Guid realPageId, IProfileDetail profile)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            try
            {
                //Setup the parameter values to update the person's info
                dynamic param = new
                {
                    RealPageId = realPageId,
                    Title = profile.Title,
                    FirstName = profile.FirstName,
                    MiddleName = profile.MiddleName,
                    LastName = profile.LastName,
                    Suffix = profile.Suffix,
                    PreferredContactMethodId = profile.PreferredContactMethodId
                };
                //Update the person's info
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, param);
                if (repositoryResponse.Id == 0)
                {
                    repositoryResponse.ErrorMessage = "Update profile Error: Update person failed.";
                }
                else
                {
                    //Job Title
                    IPartyRoleRepository partyRoleRepository = new PartyRoleRepository();
                    IPartyRole partyRole = new PartyRole();
                    if ((profile.PartyRole != null) && (profile.PartyRole.RoleTypeId > 0))
                    {
                        //Add Job title parameter values
                        param = new
                        {
                            RealPageId = realPageId,
                            RoleTypeId = profile.PartyRole.RoleTypeId
                        };
                        //Assign Job tile to person
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePartyRoleByRealPageId, param);
                    }

                    if (repositoryResponse.Id == 0)
                    {
                        repositoryResponse.ErrorMessage = "Update profile Error: Job Title failed.";
                    }
                    else
                    {
                        ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
                        IContactMechanismRepository contactMechanismRepository = new ContactMechanismRepository();
                        IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
                        //Loop through all the Telecommunication numbers
                        foreach (ITelecommunicationNumber phone in profile.TelecommunicationNumber)
                        {
                            telecommunicationNumber.ContactMechanismId = phone.ContactMechanismId;
                            telecommunicationNumber.AreaCode = phone.AreaCode;
                            telecommunicationNumber.CountryCode = phone.CountryCode;
                            telecommunicationNumber.PhoneNumber = phone.PhoneNumber;
                            telecommunicationNumber.ISOCode = phone.ISOCode;
                            telecommunicationNumber.IsDefault = phone.IsDefault;
                            //New Telecommunication number
                            if (phone.ContactMechanismId == 0)
                            {
                                //Is the Phone and Type valid
                                if ((phone.PhoneNumber.Trim().Length > 0) && (phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0))
                                {
                                    //Add a new phone
                                    //Create the Contact Mechanism
                                    param = new
                                    {
                                        ContactMechanismId = 0
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update profile Error: Create Contact Mechanism failed.";
                                    }
                                    else
                                    {
                                        telecommunicationNumber.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);
                                        //Associate the Contact Mechanism to a Party
                                        param = new
                                        {
                                            RealPageId = realPageId,
                                            PartyContactMechanismId = 0,
                                            ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                                            FromDate = utcNow,
                                            ThruDate = utcMaxValue
                                        };
                                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                        if (repositoryResponse.Id == 0)
                                        {
                                            repositoryResponse.ErrorMessage = "Update profile Error: Create Contact Mechanism failed.";
                                        }
                                        else
                                        {
                                            //Assign a usage type to the Contact Mechanism
                                            partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                                            param = new
                                            {
                                                PartyContactMechanismId = partyContactMechanism.PartyContactMechanismId,
                                                ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                                            };
                                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
                                            if (repositoryResponse.Id == 0)
                                            {
                                                repositoryResponse.ErrorMessage = "Update profile Error: Link UsageType to Party Contact Mechanism failed.";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0) && (phone.PhoneNumber.Trim().Length > 0))
                                {
                                    //Set the PhoneType for the added/updated Telecommunication number
                                    param = new
                                    {
                                        PartyContactMechanismID = phone.PartyContactMechanismId,
                                        ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                                    };

                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateContactMechanismUsageForParty, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update profile Error: Update Contact Mechanism Usage For Party failed.";
                                    }
                                }
                                else
                                {
                                    //Expire the Telecommunication number is the phone number or type is cleared
                                    param = new
                                    {
                                        RealPageId = realPageId,
                                        PartyContactMechanismId = phone.PartyContactMechanismId,
                                        ContactMechanismId = phone.ContactMechanismId,
                                        FromDate = utcNow,
                                        ThruDate = utcNow
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                }

                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update profile Error: Link Contact Mechanism To a Party failed.";
                                }
                            }

                            if ((telecommunicationNumber.ContactMechanismId > 0) && (phone.PhoneNumber.Trim().Length > 0))
                            {
                                param = new
                                {
                                    ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                                    AreaCode = telecommunicationNumber.AreaCode,
                                    CountryCode = telecommunicationNumber.CountryCode,
                                    PhoneNumber = telecommunicationNumber.PhoneNumber,
                                    ISOCode = telecommunicationNumber.ISOCode,
                                    Default = telecommunicationNumber.IsDefault
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateTelecommunicationNumber, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update profile Error: Link a telecommunication number details for a person failed.";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                repositoryResponse.Id = 0;
                repositoryResponse.ErrorMessage = "Update profile Error: " + exception.Message;
            }

            return repositoryResponse;
        }

        /// <summary>
        /// Update UserProfile
        /// </summary>
        /// <param name="repository">repositoryr</param>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">profile object of the parameter values</param>
        /// <returns>Repository response object</returns>
        private RepositoryResponse UpdateUserProfile(IRepository repository, Guid realPageId, Profile profile)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            RepositoryResponse repositoryResponse = new RepositoryResponse();

            try
            {
                //Setup the parameter values to update the person's info
                dynamic param = new
                {
                    RealPageId = realPageId,
                    Title = profile.Title,
                    FirstName = profile.FirstName,
                    MiddleName = profile.MiddleName,
                    LastName = profile.LastName,
                    Suffix = profile.Suffix,
                    PreferredContactMethodId = profile.PreferredContactMethodId
                };
                //Update the person's info
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, param);
                if (repositoryResponse.Id == 0)
                {
                    repositoryResponse.ErrorMessage = "Update user profile Error: Update person failed.";
                }
                else
                {
                    //Job Title
                    IPartyRoleRepository partyRoleRepository = new PartyRoleRepository();
                    IPartyRole partyRole = new PartyRole();
                    if ((profile.PartyRole != null) && (profile.PartyRole.PartyRoleId > 0))
                    {
                        //Update Job title parameter values
                        param = new
                        {
                            PartyRoleId = profile.PartyRole.PartyRoleId,
                            RoleTypeID = profile.PartyRole.RoleTypeId
                        };
                        //Update the person's Job title
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePartyRoleByRealPageId, param);
                    }
                    else
                    {
                        //Add Job title parameter values
                        param = new
                        {
                            realPageId = realPageId,
                            RoleTypeId = profile.PartyRole.RoleTypeId
                        };
                        //Assign Job tile to person
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePartyRoleByRealPageId, param);
                    }

                    if (repositoryResponse != null && repositoryResponse.Id == 0)
                    {
                        repositoryResponse.ErrorMessage = "Update  user profile Error: Job Title failed.";
                    }
                    else
                    {
                        ITelecommunicationNumberRepository telecommunicationNumberRepository = new TelecommunicationNumberRepository();
                        ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
                        IContactMechanismRepository contactMechanismRepository = new ContactMechanismRepository();
                        IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
                        //Loop through all the Telecommunication numbers
                        foreach (ITelecommunicationNumber phone in profile.TelecommunicationNumber)
                        {
                            telecommunicationNumber.ContactMechanismId = phone.ContactMechanismId;
                            telecommunicationNumber.AreaCode = phone.AreaCode;
                            telecommunicationNumber.CountryCode = phone.CountryCode;
                            telecommunicationNumber.PhoneNumber = phone.PhoneNumber;
                            telecommunicationNumber.ISOCode = phone.ISOCode;
                            telecommunicationNumber.IsDefault = phone.IsDefault;
                            //New Telecommunication number
                            if (phone.ContactMechanismId == 0)
                            {
                                //Is the Phone and Type valid
                                if ((phone.PhoneNumber.Trim().Length > 0) && (phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0))
                                {
                                    //Add a new phone
                                    //Create the Contact Mechanism
                                    param = new
                                    {
                                        ContactMechanismId = 0
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update user profile Error: Create Contact Mechanism failed.";
                                    }
                                    else
                                    {
                                        telecommunicationNumber.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);
                                        //Associate the Contact Mechanism to a Party
                                        param = new
                                        {
                                            RealPageId = realPageId,
                                            PartyContactMechanismId = 0,
                                            ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                                            FromDate = utcNow,
                                            ThruDate = utcMaxValue
                                        };
                                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                        if (repositoryResponse.Id == 0)
                                        {
                                            repositoryResponse.ErrorMessage = "Update user profile Error: Create Contact Mechanism failed.";
                                        }
                                        else
                                        {
                                            //Assign a usage type to the Contact Mechanism
                                            partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                                            param = new
                                            {
                                                PartyContactMechanismId = partyContactMechanism.PartyContactMechanismId,
                                                ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                                            };
                                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
                                            if (repositoryResponse.Id == 0)
                                            {
                                                repositoryResponse.ErrorMessage = "Update user profile Error: Link UsageType to Party Contact Mechanism failed.";
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if ((phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0) && (phone.PhoneNumber.Trim().Length > 0))
                                {
                                    //Set the PhoneType for the added/updated Telecommunication number
                                    param = new
                                    {
                                        PartyContactMechanismID = phone.PartyContactMechanismId,
                                        ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                                    };

                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateContactMechanismUsageForParty, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update user profile Error: Update Contact Mechanism Usage For Party failed.";
                                    }
                                }
                                else
                                {
                                    //Expire the Telecommunication number is the phone number or type is cleared
                                    param = new
                                    {
                                        RealPageId = realPageId,
                                        PartyContactMechanismId = phone.PartyContactMechanismId,
                                        ContactMechanismId = phone.ContactMechanismId,
                                        FromDate = utcNow,
                                        ThruDate = utcNow
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                }

                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update user profile Error: Link Contact Mechanism To a Party failed.";
                                }
                            }

                            if ((telecommunicationNumber.ContactMechanismId > 0) && (phone.PhoneNumber.Trim().Length > 0))
                            {
                                param = new
                                {
                                    ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                                    AreaCode = telecommunicationNumber.AreaCode,
                                    CountryCode = telecommunicationNumber.CountryCode,
                                    telecommunicationNumber.PhoneNumber,
                                    telecommunicationNumber.ISOCode,
                                    telecommunicationNumber.IsDefault
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateTelecommunicationNumber, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update user profile Error: Link a telecommunication number details for a person failed.";
                                }
                            }
                        }

                        IManageElectronicAddress electronicAddressLogic = new ManageElectronicAddress();
                        IElectronicAddress electronicAddress = new ElectronicAddress();
                        electronicAddress = profile.EmailContacts[0];

                        if (electronicAddress.ContactMechanismId == 0 && profile.EmailContacts.Count > 0 && profile.EmailContacts[0].AddressString != null && profile.EmailContacts[0].AddressString.Trim().Length > 0)
                        {
                            param = new
                            {
                                ContactMechanismId = 0
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);

                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update user user profile Error: Create Contact Mechanism failed for Electronic Email Address.";
                            }
                            else
                            {
                                electronicAddress.ContactMechanismId = (int)repositoryResponse.Id;
                                //Associate the Contact Mechanism to a Party
                                param = new
                                {
                                    RealPageId = realPageId,
                                    PartyContactMechanismId = 0,
                                    ContactMechanismId = repositoryResponse.Id,
                                    FromDate = utcNow,
                                    ThruDate = utcMaxValue
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update user user profile Error: Create Contact Mechanism failed for Electronic Email Address.";
                                }
                                else
                                {
                                    electronicAddress.PartyContactMechanismId = (int)repositoryResponse.Id;
                                    param = new
                                    {
                                        PartyContactMechanismId = repositoryResponse.Id,
                                        ContactMechanismUsageTypeId = profile.EmailContacts[0].contactMechanismUsageType.ContactMechanismUsageTypeId
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update user profile Error: Link UsageType to Party Contact Mechanism failed for Electronic Email Address.";
                                    }
                                    else
                                    {
                                        electronicAddress.ContactMechanismUsageTypeId = (int)repositoryResponse.Id;
                                    }
                                }
                            }
                        }

                        if (profile.EmailContacts.Count > 0 && profile.EmailContacts[0].AddressString != null && profile.EmailContacts[0].AddressString.Trim().Length > 0)
                        {
                            param = new
                            {
                                ContactMechanismId = profile.EmailContacts[0].ContactMechanismId,
                                ElectronicAddressString = profile.EmailContacts[0].AddressString,
                                ElectronicAddressType = profile.EmailContacts[0].AddressType
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update user profile Error: Link a Electronic Email Address details for a person failed.";
                            }
                        }
                        else
                        {
                            if (electronicAddress.PartyContactMechanismId != 0)
                            {
                                param = new
                                {
                                    PartyContactMechanismID = electronicAddress.PartyContactMechanismId,
                                    RealPageId = profile.RealPageId

                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_ExpirePartyContactMechanism, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update user profile Error: Link Contact Mechanism Expire for a Party failed for Electronic Email Address.";
                                }
                                else
                                {
                                    electronicAddress.ContactMechanismId = 0;
                                    electronicAddress.ContactMechanismUsageTypeId = 0;
                                    electronicAddress.PartyContactMechanismId = 0;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                repositoryResponse.Id = 0;
                repositoryResponse.ErrorMessage = "Update user profile Error: " + exception.Message;
            }

            return repositoryResponse;
        }

        /// <summary>
        /// Used to get list of valid products assigned to organization for admin user
        /// </summary>
        /// <param name="repository">Dapper Repository</param>		
        /// <param name="realPageId">enterprise User Id</param>
        /// <param name="organizationRealPageId">enterprise Organization Id</param>		
        /// <param name="aoProducts">Applicable if PMC has AO products</param>
        private List<ProductUI> GetOrganizationProductListForAdminUser(IRepository repository, Guid realPageId, Guid organizationRealPageId, IList<string> aoProducts = null)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getListProductsByOrganizationForAdminUser_{organizationRealPageId}";

            List<ProductUI> productsAssignedToCompany = rpCache.GetFromCache<List<ProductUI>>(cacheKey, 180, () =>
            {
                return repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganizationForAdminUser,
                    new { OrganizationRealPageId = organizationRealPageId }).ToList();
            });

            // Check For AO products
            if (aoProducts != null)
            {
                //check with admin user ao products, if product is not assigned to editor admin then remove that product
                foreach (ProductUI prod in productsAssignedToCompany.Where(a => a.UDMSourceCode == "AO").ToList())
                {
                    if (!aoProducts.Contains(prod.ProductCode))
                    {
                        productsAssignedToCompany.Remove(prod);
                    }
                }
            }
            else
            {
                foreach (ProductUI prod in productsAssignedToCompany.Where(a => a.UDMSourceCode == "AO").ToList())
                {
                    productsAssignedToCompany.Remove(prod);
                }
            }

            return productsAssignedToCompany;
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
            string correlationId = "";
            if (_userClaim != null)
            {
                correlationId = (_userClaim.CorrelationId != Guid.Empty) ? _userClaim.CorrelationId.ToString() : "";
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

        #endregion

        #region Private methods

        /// <summary>
        /// Used to Add/Update product information for a user
        /// </summary>
        /// <param name="repository">Dapper Repository</param>
        /// <param name="productList">list of Product Batch object</param>
        /// <param name="createUserResponse">Response when creating a new user</param>
        /// <param name="CreateUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="AssignUserPersonaId">Assigned to user PersonaId</param>
        /// <param name="realPageId">enterprise User Id</param>
        /// <param name="organizationRealPageId">enterprise Organization Id</param>
        /// <param name="errorStatus">Error Status</param>
        /// <param name="userTypeId">User TypeId</param>
        /// <param name="userIsActive">Is the user active</param>
        /// <param name="impersonatorUserId"></param>
        /// <param name="isCreateUser"></param>
        /// <param name="isRealpageAccessUser"></param>
        /// <param name="roleIdList"></param>
        /// <param name="organizationUsePrimaryProperties"></param>
        /// <param name="migratedUser"></param>
        /// <param name="operationType"></param>
        /// <param name="unifiedPlatformRole"></param>
        /// <param name="aoProducts">Applicable if PMC has AO products</param>
        /// <returns>Number of Products</returns>
        private int SaveProductDetails(IRepository repository, IList<ProductBatch> productList, CreateUserResponse<IErrorData> createUserResponse, long CreateUserPersonaId, long AssignUserPersonaId, Guid realPageId, Guid organizationRealPageId, Status<IErrorData> errorStatus, int userTypeId, bool userIsActive, long impersonatorUserId, IList<string> aoProducts = null, bool migratedUser = false, bool isCreateUser = false, int unifiedPlatformRole = 0, string operationType = "update", bool isRealpageAccessUser = false, IList<string> roleIdList = null, bool organizationUsePrimaryProperties = false)
        {
            int productCount = 0;
            int enterpriseRoleId = 0;
            int batchProcessTypeId = (int)BatchProcessType.CreateUpdateProductUser;
            string saveProductBatchError = "Save Product(s) Error: ";
            List<RoleTemplateProductRole> roleTemplateProductRole = new List<RoleTemplateProductRole>();
            List<string> vendorRoleIdList = new List<string>();
            if (errorStatus == null)
            {
                errorStatus = new Status<IErrorData>();
            }

            var batchGroup = CreateBatchProcessGroup(repository);

            ProductBatch primaryPropertyBatch = productList?.ToList().FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
            if (primaryPropertyBatch?.InputJson?.RoleList != null && primaryPropertyBatch?.InputJson?.RoleList.Count > 0)
            {
                enterpriseRoleId = Convert.ToInt32(primaryPropertyBatch.InputJson.RoleList.FirstOrDefault());
                productList.Remove(primaryPropertyBatch);
            }

            if (enterpriseRoleId > 0)
            {
                object param = new
                {
                    RoleTemplateId = enterpriseRoleId,
                    OrganizationRealPageId = organizationRealPageId
                };

                roleTemplateProductRole = repository.GetMany<RoleTemplateProductRole>(StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings, param).ToList();
            }

            // if superuser do all products that currently support user creation;
            if (userIsActive && userTypeId == (int)UserRoleType.SuperUser)
            {
                IList<ProductBatch> productListToCreate = new List<ProductBatch>();
                IList<PersonaProductUserDetails> userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = AssignUserPersonaId }).ToList();
                List<ProductUI> productsAssignedToCompany = GetOrganizationProductListForAdminUser(repository, realPageId, organizationRealPageId, aoProducts);

                if (roleIdList != null)
                {
                    vendorRoleIdList = (List<string>)roleIdList;
                }

                foreach (ProductUI prod in productsAssignedToCompany)
                {
                    // see if the user already has the product, or if they do if it is Deleted or Deactivated, and if so add it or turn it back on
                    if (userProducts == null
                        || (!userProducts.Any(a => a.ProductId == prod.ProductId))
                        || (userProducts.Any(a => a.ProductId == prod.ProductId && a.ProductStatus == (int)ProductBatchStatusType.Deleted))
                        || (userProducts.Any(a => a.ProductId == prod.ProductId && a.ProductStatus == (int)ProductBatchStatusType.Deactivated))
                        || isRealpageAccessUser
                       )
                    {

                        // don't add the product if it is already in the list
                        if (productListToCreate.All(a => a.ProductId != prod.ProductId))
                        {
                            ProductBatch pb = new ProductBatch()
                            {
                                ProductId = prod.ProductId,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                InputJson = new RolePropertyList() { PropertyRoleList = new List<PropertyRoleList>(), PropertyList = new List<string>(), RoleList = prod.ProductId == (int)ProductEnum.VendorMarketplace ? vendorRoleIdList : new List<string>(), IsVendorRoleIdOverride = prod.ProductId == (int)ProductEnum.VendorMarketplace && vendorRoleIdList?.Count > 0, IsAssigned = true }
                            };

                            productListToCreate.Add(pb);
                        }
                    }
                }

                // add edited products for admin with other list
                if (productList != null)
                {
                    foreach (var product in productList)
                    {
                        if (!productListToCreate.Any(p => p.ProductId == product.ProductId))
                        {
                            productListToCreate.Add(product);
                        }
                    }
                }
                // For System Admin if Products that are not Configured are not processed
                IList<PersonaProductUserDetails> creatorUserProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = CreateUserPersonaId }).ToList();
                IList<GbProductMap> allProducts = repository.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, null).ToList();
                foreach (var productmap in productListToCreate)
                {
                    bool isGreenBookCaresEnabled = false;
                    dynamic param = new { ProductId = productmap.ProductId };
                    List<ProductInternalSetting> productInternalSettingList;

                    var rpcache = new RPObjectCache();
                    var cacheKey = $"productInternalSetting_{productmap.ProductId}";
                    productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () => { return repository.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, param); });
                    var editUserRequiresProduct = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("IsEditUserRequiresProduct", StringComparison.OrdinalIgnoreCase))?.Value;
                    var greenbookCaresCheckRequired = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("IsGreenbookCaresCheckRequired", StringComparison.OrdinalIgnoreCase))?.Value;

                    bool isEditUserRequiresProduct = editUserRequiresProduct != null && editUserRequiresProduct != "0";
                    bool isGreenbookCaresCheckRequired = greenbookCaresCheckRequired != null && greenbookCaresCheckRequired != "0";
                    if (isGreenbookCaresCheckRequired)
                    {
                        var productDetails = allProducts.FirstOrDefault(x => x.ProductId == productmap.ProductId);
                        string udmSource = productDetails.UDMSourceCode?.Length > 0 ? productDetails.UDMSourceCode : productDetails.BooksProductCode;
                        IList<CustomerCompanyMap> companyMapping = _manageBlueBook.GetProductCompanyMapping(organizationRealPageId, udmSource);
                        var booksCompanyInstance = _manageBlueBook.GetCompanyInstanceByUPFMCompanyId(organizationRealPageId.ToString().ToLower());
                        int customerCompanyId = booksCompanyInstance?.Attributes?.CustomerCompanyMap.FirstOrDefault()?.CustomerCompanyId ?? 0;
                        string domain = booksCompanyInstance?.Attributes?.Domain;
                        WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "SaveProductDetails", $"Got - AssignUserPersonaId : {AssignUserPersonaId} - BooksProductCode : {productDetails.BooksProductCode} and customerCompanyId - {customerCompanyId}" });

                        if (!string.IsNullOrEmpty(domain) && customerCompanyId != 0)
                        {
                            var booksCustomerCompanyMap = _manageBlueBook.GetCustomerCompanyMapByCustomerCompanyId(customerCompanyId, domain);
                            var findBooksProductCode = booksCustomerCompanyMap?.Where(p => p.Source == (!string.IsNullOrEmpty(productDetails.UDMSourceCode) ? productDetails.UDMSourceCode : productDetails.BooksProductCode));
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "SaveProductDetails", $"Found booksproductcode - AssignUserPersonaId : {AssignUserPersonaId} - BooksProductCode : {productDetails.BooksProductCode} and Count - {findBooksProductCode.Count()}" });

                            if (findBooksProductCode != null && findBooksProductCode.Count() == 1)
                            {
                                isGreenBookCaresEnabled = findBooksProductCode.FirstOrDefault().CompanyInstance.FirstOrDefault().GreenBookCares;
                                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "SaveProductDetails", $"Got AssignUserPersonaId: {AssignUserPersonaId} - isGreenBookCaresEnabled : {isGreenBookCaresEnabled}" });

                            }
                        }

                        if (companyMapping != null && isGreenBookCaresEnabled)
                        {
                            if (isEditUserRequiresProduct)
                            {
                                if (creatorUserProducts.Any(x => x.ProductId == productmap.ProductId && x.ProductStatus == 8))
                                {
                                    if (!productList.Any(p => p.ProductId == productmap.ProductId))
                                    {
                                        productList.Add(productmap);
                                    }
                                }
                            }
                            else
                            {
                                if (!productList.Any(p => p.ProductId == productmap.ProductId))
                                {
                                    productList.Add(productmap);
                                }
                            }
                        }
                    }
                    else if (isEditUserRequiresProduct)
                    {
                        if (creatorUserProducts.Any(x => x.ProductId == productmap.ProductId && x.ProductStatus == 8))
                        {
                            if (!productList.Any(p => p.ProductId == productmap.ProductId))
                            {
                                productList.Add(productmap);
                            }
                        }
                    }
                    else
                    {
                        if (!productList.Any(p => p.ProductId == productmap.ProductId))
                        {
                            productList.Add(productmap);
                        }
                    }
                }
            }

            if (userIsActive && userTypeId != (int)UserRoleType.SuperUser && !migratedUser && enterpriseRoleId > 0 && operationType == "add")
            {
                object param = new
                {
                    RoleTemplateId = enterpriseRoleId,
                    OrganizationRealPageId = organizationRealPageId
                };
                var roleTemplateProducts = repository.GetMany<int>(StoredProcNameConstants.SP_GetEnterpriseRoleProductsByOrganization, param).ToList();

                foreach (var product in roleTemplateProducts)
                {
                    batchProcessTypeId = (int)BatchProcessType.CreateUpdateProductUser;
                    var productRoleData = roleTemplateProductRole?.Where(p => p.ProductId == product);
                    var roleTemplateRoles = productRoleData?.Select(p => new
                    {
                        p.RoleTemplateProductRoleMappingId,
                        p.ProductRoleId,
                        p.ProductRoleName
                    }).Distinct();

                    //Roles
                    List<string> productRoles = new List<string>();
                    foreach (var roles in roleTemplateRoles)
                    {
                        if (roles.RoleTemplateProductRoleMappingId != 0)
                        {
                            productRoles.Add(roles.ProductRoleId);
                        }
                    }

                    if (product == (int)ProductEnum.UnifiedPlatform)
                    {
                        int enterprisePlatformRole = Convert.ToInt32(productRoles.FirstOrDefault());
                        if (enterprisePlatformRole > 0 && unifiedPlatformRole > 0)
                        {
                            UpdateGreenBookRole(repository, new List<int>() { enterprisePlatformRole }, AssignUserPersonaId, new List<long>() { unifiedPlatformRole }, _userClaim.UserId);
                        }
                    }

                    //First check for any batchdata coming from ui, if no batch data then process for enterprise role batch

                    var productInternalSettingList = repository.GetMany<ProductInternalSetting>(
                        StoredProcNameConstants.SP_ListGlobalSettingsForProduct,
                        new { ProductId = product }
                    ).ToList();
                    
                    int productPrimaryPropertySetting = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase))?.Value != "0" ? 1 : 0; 
                    bool productUsePrimaryProperties = productPrimaryPropertySetting == 1 && organizationUsePrimaryProperties;

                    var productBatch = productList.FirstOrDefault(p => p.ProductId == product);
                    if (productBatch == null && product != (int)ProductEnum.UnifiedPlatform)
                    {
                        batchProcessTypeId = product == (int)ProductEnum.KnockCRM ? (int)BatchProcessType.CreateUpdateProductUser : (int)BatchProcessType.EnterpriseRoleCreateUpdateProductUser;
                        ProductBatch pb = null;

                        if (product == 94 && enterpriseRoleId > 0 && roleTemplateProductRole.Any(e => e.ProductId == 94))
                        {
                            var licenses = roleTemplateProductRole.Where(a => a.ProductId == 94)?.Select(r => r.AttributeValue)?.Distinct()?.ToList();
                            if (licenses == null)
                                licenses = new List<string>();

                            pb = new ProductBatch()
                            {
                                ProductId = product,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                InputJson = new RolePropertyList()
                                {
                                    PropertyRoleList = new List<PropertyRoleList>(),
                                    PropertyList = new List<string>(),
                                    RoleList = productRoles,
                                    IsAssigned = true,
                                    IsAssignedNewPropertyByDefault = false,
                                    UsePrimaryProperties = productUsePrimaryProperties && (productBatch.InputJson != null ? productBatch.InputJson.UsePrimaryProperties : false),
                                    RCLicenseDetails = new SO.Product.RealConnect.RCProductBatch() { LearnerLicenseId = licenses, ManagerLicenseId = new List<string>() { } }
                                }
                            };
                        }
                        else
                        {
                            pb = new ProductBatch()
                            {
                                ProductId = product,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                InputJson = new RolePropertyList()
                                {
                                    PropertyRoleList = new List<PropertyRoleList>(),
                                    PropertyList = new List<string>(),
                                    RoleList = productRoles,
                                    IsAssigned = true,
                                    IsAssignedNewPropertyByDefault = false,
                                    UsePrimaryProperties = productUsePrimaryProperties && (productBatch.InputJson != null ? productBatch.InputJson.UsePrimaryProperties : false)
                                }
                            };
                        }
                        if (!productList.Any(p => p.ProductId == product))
                        {
                            productList.Add(pb);
                        }
                    }
                }
            }

            if (productList != null)
            {
                //Product EasyLMS is assigned and the tile is display for all users if it's assigned to the Organization 
                //No need to add an EasyLMS ProductPatch eventhough it's included in the ProductBatch product list from the UI
                ProductBatch easyLMSProductBatch = productList.ToList().FirstOrDefault(p => p.ProductId == (int)ProductEnum.EasyLMS);
                if (easyLMSProductBatch != null)
                {
                    productList.Remove(easyLMSProductBatch);
                }

                // Do not add client portal product for realpage access users.
                if (isRealpageAccessUser)
                {
                    ProductBatch clientPortalProductBatch = productList.ToList().FirstOrDefault(p => p.ProductId == (int)ProductEnum.ClientPortal);
                    if (clientPortalProductBatch != null)
                    {
                        productList.Remove(clientPortalProductBatch);
                    }
                }

                // check salesforce contact for all users
                //if (!productList.Any(p => p.ProductId == (int) ProductEnum.ClientPortal))
                {
                    if (userTypeId != (int)UserRoleType.UserNoEmail)
                    {
                        ProductBatch pb = new ProductBatch()
                        {
                            ProductId = (int)ProductEnum.SalesForce,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                            InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = (isCreateUser || userIsActive) }
                        };
                        if (!productList.Any(p => p.ProductId == (int)ProductEnum.SalesForce))
                        {
                            productList.Add(pb);
                        }
                    }
                }

                /***********************
                * THis is work around for bug where ui is not sending BM in product list if performance analytic removed (un-assign all products)
                 * Issue - GB-4367
                ***********************/
                var aoPerformanceAnalyticProduct = productList.FirstOrDefault(x => x.ProductId == (int)ProductEnum.AoPerformanceAnalytics);
                if (aoPerformanceAnalyticProduct != null && !aoPerformanceAnalyticProduct.InputJson.IsAssigned)
                {
                    // forcefully set BM un-assigned whenever Performance Analytic unassigned
                    ProductBatch pb = new ProductBatch()
                    {
                        ProductId = (int)ProductEnum.AoBenchmarking,
                        StatusTypeId = 5,
                        RetryCount = 0,
                        BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                        InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
                    };
                    productList.Add(pb);
                }
            }

            //Save selected products
            if ((productList != null) && (productList.Count > 0))
            {
                //Do we have the Create & Assign PersonaIds
                if (productList.Any(a => a.ProductId == (int)ProductEnum.VendorMarketplace) && vendorRoleIdList?.Count > 0)
                {
                    CreateUserPersonaId = AssignUserPersonaId;
                    var userPersona = repository.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, new { personaId = AssignUserPersonaId });
                    realPageId = userPersona.RealPageId;
                }
                if ((CreateUserPersonaId > 0) && (AssignUserPersonaId > 0))
                {
                    // if the user isn't a superuser, check to see if both Lead2Lease and OneSite are in the products to be saved. If they are, then they need to be combined into a single product call
                    // if the user isn't a superuser, check to see if both SeniorLead and OneSite are in the products to be saved. If they are, then they need to be combined into a single product call

                    //Lead2Lease and OneSite, SeniorLead and OneSite, SeniorLead and OneSite and Lead2Lease
                    if (!(userTypeId == (int)UserRoleType.SuperUser)
                        && productList.Any(a => a.ProductId == (int)ProductEnum.OneSite)
                        && (productList.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease) || productList.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement)))
                    {
                        // need to combine the Lead2Lease and OneSite product details so they can run synchronously
                        Dictionary<string, RolePropertyList> oneSiteAndOtherProducts = new Dictionary<string, RolePropertyList>();

                        ProductBatch pbOneSite = (from a in productList
                                                  where a.ProductId == (int)ProductEnum.OneSite
                                                  select a).FirstOrDefault();

                        ProductBatch pbLead2Lease = null;
                        ProductBatch pbSeniorLead = null;

                        oneSiteAndOtherProducts.Add(ProductEnum.OneSite.ToString(), pbOneSite.InputJson);

                        if (productList.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease))
                        {
                            pbLead2Lease = (from a in productList
                                            where a.ProductId == (int)ProductEnum.Lead2Lease
                                            select a).FirstOrDefault();

                            oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbLead2Lease.InputJson);
                        }

                        if (productList.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement))
                        {
                            pbSeniorLead = (from a in productList
                                            where a.ProductId == (int)ProductEnum.SeniorLeadManagement
                                            select a).FirstOrDefault();

                            oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbSeniorLead.InputJson);
                        }

                        pbOneSite.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
                        SaveProductBatch(repository, pbOneSite, createUserResponse, saveProductBatchError, CreateUserPersonaId, AssignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(oneSiteAndOtherProducts), impersonatorUserId, batchProcessTypeId);

                        if (errorStatus.Success == false)
                        {
                            errorStatus.ErrorMsg = saveProductBatchError;
                        }
                        else
                        {
                            // remove OneSite and any other products with it from the product batch
                            productList.Remove(pbOneSite);
                            if (pbLead2Lease != null)
                            {
                                productList.Remove(pbLead2Lease);
                            }

                            if (pbSeniorLead != null)
                            {
                                productList.Remove(pbSeniorLead);
                            }
                        }
                    }

                    // Handle Ao Products if any
                    var aoInputJsonString = BundleAoProducts(productList, batchGroup.BatchProcessorGroupId);

                    if (isRealpageAccessUser && CreateUserPersonaId == AssignUserPersonaId)
                    {
                        var aOInputJson = new RolePropertyList()
                        {
                            PropertyList = new List<string>(),
                            RoleList = new List<string>(),
                            IsAssigned = true,
                            CompanyId = 0
                        };
                        aoInputJsonString = JsonConvert.SerializeObject(aOInputJson).ToString();
                    }
                    //Loop through the rest of the products list and create the Batch records
                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productList)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedPlatform || product.ProductId == (int)ProductEnum.UnifiedUI)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products
                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, CreateUserPersonaId, AssignUserPersonaId, realPageId, errorStatus, aoInputJsonString, impersonatorUserId, batchProcessTypeId);
                        }
                        else
                        {
                            product.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "SaveProductDetails", $"Got group id AssignUserPersonaId - {AssignUserPersonaId} - productId : {product.ProductId} - batchProcessorGroupId : {product.BatchProcessorGroupId}" });

                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, CreateUserPersonaId, AssignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson), impersonatorUserId, batchProcessTypeId);
                        }
                    }

                    if (errorStatus.Success == false)
                    {
                        errorStatus.ErrorMsg = saveProductBatchError;
                    }
                }

                productCount = productList.Count;
            }

            return productCount;
        }

        /// <summary>
        /// Used to process any information (such as first name or last name or uer email)for a user changed or user type changed
        /// </summary>
        /// <param name="repository">Dapper Repository</param>
        /// <param name="createUserResponse">Response when creating a new user</param>
        /// <param name="createUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="assignUserPersonaId">Assigned to user PersonaId</param>
        /// <param name="realPageId">enterprise User Id</param>
        /// <param name="organizationRealPageId">enterprise Organization Id</param>
        /// <param name="errorStatus">Error Status</param>
        /// <param name="batchProcessTypeId">Batch Process Type</param>
        /// <param name="productBatchData">Product Batch Data</param>
        private void SaveUserProductBatchData(IRepository repository,
            CreateUserResponse<IErrorData> createUserResponse,
            long createUserPersonaId,
            long assignUserPersonaId,
            Guid realPageId,
            Guid organizationRealPageId,
            Status<IErrorData> errorStatus,
            int batchProcessTypeId,
            IList<ProductBatch> productBatchData,
            IList<string> aoProductsAvailableForUser,
            int userTypeId)
        {
            string saveProductBatchError = "Save Product User Profile/Type Error: ";
            string aoInputJsonString = string.Empty;
            IList<ProductBatch> productListToRemove = new List<ProductBatch>();

            if (errorStatus == null)
            {
                errorStatus = new Status<IErrorData>();
            }

            var batchGroup = CreateBatchProcessGroup(repository);
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = _userClaim.ImpersonatedBy });
            }

            IList<PersonaProductUserDetails> userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = assignUserPersonaId, ProductStatusValue = ((Int32)UserUiStatusType.AccountCreationSuccessful).ToString() }).ToList();
            IList<ProductBatch> productListToCreate = new List<ProductBatch>();
            IList<ProductBatch> productListMapping = new List<ProductBatch>();
            /*             
            var primaryPropertyBatch = productBatchData.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);

            if (primaryPropertyBatch != null)
            {
                productBatchData.Remove(primaryPropertyBatch);
            }
            */
            //Remove products to process when product batch data updated in ui while processing user type changed batch process
            if (batchProcessTypeId == (int)BatchProcessType.UserTypeAdminToRegular || batchProcessTypeId == (int)BatchProcessType.UserTypeRegularToAdmin || batchProcessTypeId == (int)BatchProcessType.UserTypeAdminToExternal || batchProcessTypeId == (int)BatchProcessType.UserTypeExternalToAdmin)
            {
                if (productBatchData != null && (batchProcessTypeId == (int)BatchProcessType.UserTypeAdminToRegular || batchProcessTypeId == (int)BatchProcessType.UserTypeAdminToExternal))
                {
                    //Admin to Regular
                    //First unassign (remove) all products access which *user* has previously has a admin user type
                    foreach (var prod in userProducts)
                    {
                        // skip AO products
                        if (!ProductEnumHelper.GetAoProductList().Contains((ProductEnum)prod.ProductId) && (ProductEnum)prod.ProductId != ProductEnum.AssetOptimizer)
                        {
                            // remove products which are completely unassigned
                            if (productBatchData.All(p => p.ProductId != prod.ProductId) || (productBatchData.Any(p => p.ProductId == prod.ProductId && !p.InputJson.IsAssigned)))
                            {
                                ProductBatch pb = new ProductBatch()
                                {
                                    ProductId = prod.ProductId,
                                    StatusTypeId = 5,
                                    RetryCount = 0,
                                    BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                    InputJson = new RolePropertyList()
                                    {
                                        PropertyList = new List<string>(),
                                        RoleList = new List<string>(),
                                        IsAssigned = false
                                    }
                                };

                                productListToRemove.Add(pb);
                            }
                            else if (productBatchData.Any(p => p.ProductId == prod.ProductId && p.InputJson.IsAssigned))
                            {
                                var batchRecord =
                                    productBatchData.FirstOrDefault(p => p.ProductId == prod.ProductId);

                                if (batchRecord != null)
                                {
                                    // add product to productListToCreate
                                    ProductBatch pb = new ProductBatch()
                                    {
                                        ProductId = prod.ProductId,
                                        StatusTypeId = 5,
                                        RetryCount = 0,
                                        BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                        InputJson = new RolePropertyList()
                                        {
                                            PropertyList = batchRecord.InputJson.PropertyList,
                                            RoleList = batchRecord.InputJson.RoleList,
                                            PropertyRoleList = batchRecord.InputJson.PropertyRoleList,
                                            PropertyGroup = batchRecord.InputJson.PropertyGroup,
                                            RolePropertiesList = batchRecord.InputJson.RolePropertiesList,
                                            Notifications = batchRecord.InputJson.Notifications,
                                            RegionList = batchRecord.InputJson.RegionList,
                                            PropertyGroupList = batchRecord.InputJson.PropertyGroupList,
                                            DepartmentList = batchRecord.InputJson.DepartmentList,
                                            IsInsuranceExpired = batchRecord.InputJson.IsInsuranceExpired,
                                            IsVendorRecommendationChanges =
                                                batchRecord.InputJson.IsVendorRecommendationChanges,
                                            IsVendorNotLinkedToAnyProperty =
                                                batchRecord.InputJson.IsVendorNotLinkedToAnyProperty,
                                            MessageGroups = batchRecord.InputJson.MessageGroups,
                                            IsAssigned = true,
                                            CompaniesList = batchRecord.InputJson.CompaniesList,
                                            HasAccessToAllCurrentFutureProperties = batchRecord.InputJson.HasAccessToAllCurrentFutureProperties,
                                            HasAccessToSiteSpendManagementOnly = batchRecord.InputJson.HasAccessToSiteSpendManagementOnly,
                                            IsAccountingAdmin = batchRecord.InputJson.IsAccountingAdmin,
                                            OrganizationRoleList = batchRecord.InputJson.OrganizationRoleList,
                                            IsAssignedNewPropertyByDefault = batchRecord.InputJson.IsAssignedNewPropertyByDefault
                                        }
                                    };

                                    productListToCreate.Add(pb);
                                }
                            }
                        }
                    }

                    // Get AO products for user
                    var aoUserProductList = userProducts.Where(y =>
                        ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();

                    // if user has AO products then add or remove
                    if (aoUserProductList.Any())
                    {
                        IProductBatch aoProductsBatch = new ProductBatch
                        {
                            ProductId = (int)ProductEnum.AssetOptimizer,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                            InputJson = null
                        };

                        StringBuilder sb = new StringBuilder();
                        dynamic expandoList = new ExpandoObject();
                        expandoList.IsAssigned = true;
                        expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();

                        // Collect ALL Json(s) for AO products based on assigned or removed

                        if (aoUserProductList.Any(aoProduct => productBatchData.Any(p => (p.ProductId == aoProduct.ProductId))))
                        {
                            sb = new StringBuilder();
                            expandoList = new ExpandoObject();
                            expandoList.IsAssigned = true;
                            expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();
                            foreach (var aoProduct in aoUserProductList)
                            {
                                dynamic expandoAo = new ExpandoObject();

                                if (productBatchData.Any(p => p.ProductId == aoProduct.ProductId && p.InputJson.IsAssigned))
                                {
                                    // user has added specific product
                                    // Get product details from one added in batch
                                    var batchRecord =
                                        productBatchData.FirstOrDefault(p => p.ProductId == aoProduct.ProductId);

                                    if (batchRecord != null)
                                    {
                                        expandoAo.SelectedRoleValues = batchRecord.InputJson.RoleList;
                                        expandoAo.SelectedPortfolioValues = batchRecord.InputJson.PropertyList;
                                        expandoAo.CompanyId = batchRecord.InputJson.CompanyId;
                                        expandoAo.PropertyGroups = batchRecord.InputJson.PropertyGroupList;
                                    }

                                    expandoAo.Product =
                                        ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId);
                                    expandoAo.DivisionName =
                                        ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId);

                                    expandoAo.IsAssigned = true;
                                }
                                else
                                {
                                    //dynamic expandoAo = new ExpandoObject();
                                    // user has removed specific product
                                    expandoAo.SelectedRoleValues = null;
                                    expandoAo.SelectedPortfolioValues = null;
                                    expandoAo.CompanyId = 0;
                                    expandoAo.Product = ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId);
                                    expandoAo.DivisionName =
                                        ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId);
                                    expandoAo.PropertyGroups = null;
                                    expandoAo.IsAssigned = false;
                                    //expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);
                                }

                                // add in collection
                                expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);

                            }

                            // add record to Add AO products
                            sb.Append(JsonConvert.SerializeObject(expandoList));

                            // save AO specific records in batch
                            SaveProductBatch(repository, aoProductsBatch, createUserResponse,
                                saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus,
                                sb.ToString(), impersonatorUserLoginOnly.UserId, (int)BatchProcessType.CreateUpdateProductUser);
                        }
                    }

                    if (!productBatchData.Any(p => p.ProductId == (int)ProductEnum.ClientPortal))
                    {
                        if (!(userTypeId == (int)UserRoleType.UserNoEmail))
                        {
                            // add salesforce to the batch data
                            ProductBatch pbs = new ProductBatch()
                            {
                                ProductId = (int)ProductEnum.SalesForce,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                InputJson = new RolePropertyList()
                                { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
                            };
                            productListToRemove.Add(pbs);
                        }
                    }

                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productListToRemove)
                    {
                        int finalBatchProcessorTypeId = (int)BatchProcessType.CreateUpdateProductUser;
                        if (product.ProductId == (int)ProductEnum.UnifiedPlatform)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.OneSite && !product.InputJson.IsAssigned && (batchProcessTypeId == (int)BatchProcessType.UserTypeAdminToRegular || batchProcessTypeId == (int)BatchProcessType.UserTypeAdminToExternal))
                        {
                            finalBatchProcessorTypeId = batchProcessTypeId;
                        }

                        SaveProductBatch(repository, product, createUserResponse, saveProductBatchError,
                            createUserPersonaId, assignUserPersonaId, realPageId, errorStatus,
                            JsonConvert.SerializeObject(product.InputJson), impersonatorUserLoginOnly.UserId, finalBatchProcessorTypeId);
                    }

                    if (errorStatus.Success == false)
                    {
                        errorStatus.ErrorMsg = saveProductBatchError;
                    }
                }
                else //UserTypeRegularToAdmin || UserTypeExternalToAdmin
                {
                    // Get products assigned to company including AO products
                    List<ProductUI> productsAssignedToCompany = GetOrganizationProductListForAdminUser(repository, realPageId, organizationRealPageId, aoProductsAvailableForUser);

                    //Regular to Admin
                    foreach (ProductUI prod in productsAssignedToCompany)
                    {
                        if (prod.ProductId == (int)ProductEnum.VendorMarketplace)
                        {
                            ProductBatch pb = new ProductBatch()
                            {
                                ProductId = prod.ProductId,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                InputJson = new RolePropertyList()
                                {
                                    PropertyRoleList = new List<PropertyRoleList>(),
                                    PropertyList = new List<string>(),
                                    RoleList = GetVMPVendorAdminRoles(repository, organizationRealPageId),
                                    IsAssigned = true
                                }
                            };
                            productListToCreate.Add(pb);
                        }
                        else
                        {
                            ProductBatch pb = new ProductBatch()
                            {
                                ProductId = prod.ProductId,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                InputJson = new RolePropertyList()
                                {
                                    PropertyRoleList = new List<PropertyRoleList>(),
                                    PropertyList = new List<string>(),
                                    RoleList = new List<string>(),
                                    IsAssigned = true
                                }
                            };
                            productListToCreate.Add(pb);
                        }
                    }

                    // AO product handling - removes AO products from list & returns JSON string
                    aoInputJsonString = BundleAoProducts(productListToCreate, batchGroup.BatchProcessorGroupId);

                    // For System Admin if Products that are not Configured are not processed
                    IList<GbProductMap> allProducts = repository.GetMany<GbProductMap>(StoredProcNameConstants.SP_ListProduct, null).ToList();
                    IList<PersonaProductUserDetails> creatorUserProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = createUserPersonaId }).ToList();

                    foreach (var productmap in productListToCreate)
                    {
                        bool isGreenBookCaresEnabled = false;
                        dynamic param = new { ProductId = productmap.ProductId };
                        List<ProductInternalSetting> productInternalSettingList;

                        var rpcache = new RPObjectCache();
                        var cacheKey = $"productInternalSetting_{productmap.ProductId}";
                        productInternalSettingList = rpcache.GetFromCache(cacheKey, 120, () => { return repository.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, param); });
                        var editUserRequiresProduct = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("IsEditUserRequiresProduct", StringComparison.OrdinalIgnoreCase))?.Value;
                        var greenbookCaresCheckRequired = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("IsGreenbookCaresCheckRequired", StringComparison.OrdinalIgnoreCase))?.Value;

                        bool isEditUserRequiresProduct = editUserRequiresProduct != null && editUserRequiresProduct != "0";
                        bool isGreenbookCaresCheckRequired = greenbookCaresCheckRequired != null && greenbookCaresCheckRequired != "0";
                        if (isGreenbookCaresCheckRequired)
                        {
                            var productDetails = allProducts.FirstOrDefault(x => x.ProductId == productmap.ProductId);
                            string udmSource = productDetails.UDMSourceCode?.Length > 0 ? productDetails.UDMSourceCode : productDetails.BooksProductCode;
                            IList<CustomerCompanyMap> companyMapping = _manageBlueBook.GetProductCompanyMapping(organizationRealPageId, udmSource);
                            var booksCompanyInstance = _manageBlueBook.GetCompanyInstanceByUPFMCompanyId(organizationRealPageId.ToString().ToLower());
                            int customerCompanyId = booksCompanyInstance?.Attributes?.CustomerCompanyMap.FirstOrDefault()?.CustomerCompanyId ?? 0;
                            string domain = booksCompanyInstance?.Attributes?.Domain;
                            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "SaveProductDetails", $"Got - AssignUserPersonaId : {assignUserPersonaId} - BooksProductCode : {productDetails.BooksProductCode} and customerCompanyId - {customerCompanyId}" });

                            if (!string.IsNullOrEmpty(domain) && customerCompanyId != 0)
                            {
                                var booksCustomerCompanyMap = _manageBlueBook.GetCustomerCompanyMapByCustomerCompanyId(customerCompanyId, domain);
                                var findBooksProductCode = booksCustomerCompanyMap?.Where(p => p.Source == (!string.IsNullOrEmpty(productDetails.UDMSourceCode) ? productDetails.UDMSourceCode : productDetails.BooksProductCode));
                                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "SaveProductDetails", $"Found booksproductcode - AssignUserPersonaId : {assignUserPersonaId} - BooksProductCode : {productDetails.BooksProductCode} and Count - {findBooksProductCode.Count()}" });

                                if (findBooksProductCode != null && findBooksProductCode.Count() == 1)
                                {
                                    isGreenBookCaresEnabled = findBooksProductCode.FirstOrDefault().CompanyInstance.FirstOrDefault().GreenBookCares;
                                    WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "SaveProductDetails", $"Got AssignUserPersonaId: {assignUserPersonaId} - isGreenBookCaresEnabled : {isGreenBookCaresEnabled}" });

                                }
                            }

                            if (companyMapping != null && isGreenBookCaresEnabled)
                            {
                                if (isEditUserRequiresProduct)
                                {
                                    if (creatorUserProducts.Any(x => x.ProductId == productmap.ProductId && x.ProductStatus == 8))
                                    {
                                        if (!productListMapping.Any(p => p.ProductId == productmap.ProductId))
                                        {
                                            productListMapping.Add(productmap);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!productListMapping.Any(p => p.ProductId == productmap.ProductId))
                                    {
                                        productListMapping.Add(productmap);
                                    }
                                }
                            }
                        }
                        else if (isEditUserRequiresProduct)
                        {
                            if (creatorUserProducts.Any(x => x.ProductId == productmap.ProductId && x.ProductStatus == 8))
                            {
                                if (!productListMapping.Any(p => p.ProductId == productmap.ProductId))
                                {
                                    productListMapping.Add(productmap);
                                }
                            }
                        }
                        else
                        {
                            if (!productListMapping.Any(p => p.ProductId == productmap.ProductId))
                            {
                                productListMapping.Add(productmap);
                            }
                        }
                    }

                    if (productListMapping != null)
                    {
                        productListToCreate = productListMapping;
                    }

                }
            }
            else if (batchProcessTypeId == (int)BatchProcessType.ProfileUpdate)
            {
                if (userProducts?.Count > 0)
                {
                    foreach (var product in userProducts)
                    {
                        if (!ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product.ProductId))
                        {
                            ProductBatch pb = new ProductBatch()
                            {
                                ProductId = product.ProductId,
                                StatusTypeId = 5,
                                RetryCount = 0,
                                BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                                InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true }
                            };
                            productListToCreate.Add(pb);
                        }
                    }
                }
            }

            if (productListToCreate != null)
            {
                if (!productBatchData.Any(p => p.ProductId == (int)ProductEnum.ClientPortal))
                {
                    if (!(userTypeId == (int)UserRoleType.UserNoEmail))
                    {
                        // check salesforce contact for  all new users
                        ProductBatch pb = new ProductBatch()
                        {
                            ProductId = (int)ProductEnum.SalesForce,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                            InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true }
                        };
                        productListToCreate.Add(pb);
                    }
                }
            }

            //Save selected products
            if ((productListToCreate != null) && (productListToCreate.Count > 0))
            {
                //Do we have the Create & Assign PersonaIds
                if (createUserPersonaId > 0 && assignUserPersonaId > 0)
                {
                    if (!(userTypeId == (int)UserRoleType.SuperUser)
                        && productListToCreate.Any(a => a.ProductId == (int)ProductEnum.OneSite)
                        && (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease) || productListToCreate.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement)))
                    {
                        // need to combine the Lead2Lease and OneSite product details so they can run synchronously
                        Dictionary<string, RolePropertyList> oneSiteAndOtherProducts = new Dictionary<string, RolePropertyList>();

                        ProductBatch pbOneSite = (from a in productListToCreate
                                                  where a.ProductId == (int)ProductEnum.OneSite
                                                  select a).FirstOrDefault();

                        ProductBatch pbLead2Lease = null;
                        ProductBatch pbSeniorLead = null;

                        oneSiteAndOtherProducts.Add(ProductEnum.OneSite.ToString(), pbOneSite.InputJson);

                        if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease))
                        {
                            pbLead2Lease = (from a in productListToCreate
                                            where a.ProductId == (int)ProductEnum.Lead2Lease
                                            select a).FirstOrDefault();

                            oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbLead2Lease.InputJson);
                        }

                        if (productListToCreate.Any(a => a.ProductId == (int)ProductEnum.SeniorLeadManagement))
                        {
                            pbSeniorLead = (from a in productListToCreate
                                            where a.ProductId == (int)ProductEnum.SeniorLeadManagement
                                            select a).FirstOrDefault();

                            oneSiteAndOtherProducts.Add(ProductEnum.Lead2Lease.ToString(), pbSeniorLead.InputJson);
                        }

                        pbOneSite.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
                        if (userProducts.Any(pr => pr.ProductId == (int)ProductEnum.OneSite))
                        {
                            SaveProductBatch(repository, pbOneSite, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(oneSiteAndOtherProducts), impersonatorUserLoginOnly.UserId, batchProcessTypeId);
                        }
                        else
                        {
                            SaveProductBatch(repository, pbOneSite, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(oneSiteAndOtherProducts), impersonatorUserLoginOnly.UserId);
                        }

                        if (errorStatus.Success == false)
                        {
                            errorStatus.ErrorMsg = saveProductBatchError;
                        }
                        else
                        {
                            // remove OneSite and any other products with it from the product batch
                            productListToCreate.Remove(pbOneSite);
                            if (pbLead2Lease != null)
                            {
                                productListToCreate.Remove(pbLead2Lease);
                            }

                            if (pbSeniorLead != null)
                            {
                                productListToCreate.Remove(pbSeniorLead);
                            }
                        }
                    }


                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productListToCreate)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedPlatform)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products
                            SaveProductBatch(repository, product, createUserResponse,
                                saveProductBatchError, createUserPersonaId, assignUserPersonaId,
                                realPageId, errorStatus, aoInputJsonString, impersonatorUserLoginOnly.UserId, batchProcessTypeId);
                        }
                        else
                        {
                            if (userProducts.Any(pr => pr.ProductId == product.ProductId))
                            {
                                SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson), impersonatorUserLoginOnly.UserId, batchProcessTypeId);
                            }
                            else
                            {
                                SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson), impersonatorUserLoginOnly.UserId);
                            }
                        }
                    }

                    if (errorStatus.Success == false)
                    {
                        errorStatus.ErrorMsg = saveProductBatchError;
                    }
                }
            }
        }

        public List<string> GetVMPVendorAdminRoles(IRepository repository, Guid? organizationRealPageId)
        {
            List<string> superUserRoleIds = new List<string>();
            var vmpForVendorOrgTypeName = "";
            var orgTypeName = "";
            Organization organization = repository.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, new { RealPageId = (organizationRealPageId == Guid.Empty) ? null : organizationRealPageId });
            if (organization != null)
            {
                var orgType = repository.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null).FirstOrDefault(o => o.OrganizationTypeId == organization.OrganizationTypeId);
                organization.organizationType = orgType != null ? new OrganizationType { Name = orgType.Name, OrganizationTypeId = orgType.OrganizationTypeId, CreateDate = orgType.CreateDate } : new OrganizationType();
            }
            orgTypeName = organization.organizationType.Name.ToLower();
            var productSettingList = repository.GetMany<ProductInternalSetting>(StoredProcNameConstants.SP_ListGlobalSettingsForProduct, new { ProductId = (int)ProductEnum.VendorMarketplace });
            if (productSettingList.Any(a => a.Name.Equals("SuperUserRoleId", StringComparison.OrdinalIgnoreCase)))
            {
                superUserRoleIds = productSettingList.FirstOrDefault(a => a.Name.Equals("SuperUserRoleId", StringComparison.OrdinalIgnoreCase))?.Value?.Split(',')?.ToList();
            }
            if (productSettingList.Any(a => a.Name.Equals("VPMForVendorsOrgType", StringComparison.OrdinalIgnoreCase)))
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
            return superUserRoleIds;
        }

        private string BundleAoProducts(IList<ProductBatch> productList, int batchProcessorGroupId = 0)
        {
            StringBuilder sb = new StringBuilder();

            // Check if any AO products in product batch and group them
            var aoProductList = productList.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
            if (aoProductList.Any())
            {
                // bundle Ao products under product Id 4 and then remove
                ProductBatch aoProductsBatch = new ProductBatch
                {
                    ProductId = (int)ProductEnum.AssetOptimizer,
                    StatusTypeId = 5,
                    RetryCount = 0,
                    BatchProcessorGroupId = batchProcessorGroupId,
                    InputJson = null
                };

                dynamic expandoList = new ExpandoObject();
                expandoList.IsAssigned = true; // useless for AO
                expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();

                // Collect ALL Json(s) for AO products
                foreach (var aoProduct in aoProductList)
                {
                    dynamic expandoAo = new ExpandoObject();

                    expandoAo.SelectedRoleValues = aoProduct.InputJson.RoleList;
                    expandoAo.SelectedPortfolioValues = aoProduct.InputJson.PropertyList;
                    expandoAo.CompanyId = aoProduct.InputJson.CompanyId;

                    expandoAo.Product = ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId);
                    expandoAo.DivisionName = ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId);
                    ;
                    expandoAo.PropertyGroups = aoProduct.InputJson.PropertyGroupList;

                    expandoAo.IsAssigned = aoProduct.InputJson.IsAssigned;

                    expandoAo.ProductId = aoProduct.ProductId;

                    expandoAo.UsePrimaryProperties = aoProduct.InputJson.UsePrimaryProperties;

                    // add in collection
                    expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);

                    // remove newly created AoProduct from main list
                    productList.Remove(aoProduct);
                }

                sb.Append(JsonConvert.SerializeObject(expandoList));
                productList.Add(aoProductsBatch);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Save Product Batch
        /// </summary>
        /// <param name="repository">Dapper Repository</param>
        /// <param name="product">Product Batch object</param>
        /// <param name="createUserResponse">Response when creating a new user</param>
        /// <param name="saveProductBatchError"></param>
        /// <param name="CreateUserPersonaId">Logged-in user PersonaId</param>
        /// <param name="AssignUserPersonaId">Assigned to user PersonaId</param>
        /// <param name="realPageId">The enterprise User Id of the person editing the product user</param>
        /// <param name="errorStatus">Error Status</param>
        /// <param name="inputJson">Product Batch Input JSON</param>
        /// <param name="impersonatorUserId">Impersonator UserID</param>
        /// <param name="batchProcessTypeId">Batch Process Type</param>
        private void SaveProductBatch(IRepository repository, IProductBatch product, CreateUserResponse<IErrorData> createUserResponse, string saveProductBatchError, long CreateUserPersonaId, long AssignUserPersonaId, Guid realPageId, Status<IErrorData> errorStatus, string inputJson, long impersonatorUserId, int batchProcessTypeId = 1)
        {
            try
            {
                //Set the Logged-in and New User PeronaIds
                product.CreateUserPersonaId = CreateUserPersonaId;
                product.AssignUserPersonaId = AssignUserPersonaId;
                //Assign the 
                //product.InputJson.IsAssigned = true;
                //Create the Product Batch data to be processed by the Queuing
                dynamic param = new
                {
                    PersonRealPageId = realPageId,
                    CreateUserPersonaId = product.CreateUserPersonaId,
                    AssignUserPersonaId = product.AssignUserPersonaId,
                    ProductId = product.ProductId,
                    BatchProcessorGroupId = product.BatchProcessorGroupId,
                    StatusTypeId = product.StatusTypeId,
                    RetryCount = product.RetryCount,
                    InputJson = inputJson,
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BatchProcessTypeId = batchProcessTypeId,
                    ImpersonatorUserId = impersonatorUserId
                };

                RepositoryResponse repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductBatch, param);
                //In-case of an error creating a product batch record, append the ProductId to the ErrorReason
                if (repositoryResponse == null || repositoryResponse.Id == 0)
                {
                    if (errorStatus != null)
                    {
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.SaveProductBatch.1";
                    }

                    if (createUserResponse != null)
                    {
                        createUserResponse.Status = errorStatus;
                    }

                    if (saveProductBatchError != "Save Product(s) Error: ")
                    {
                        saveProductBatchError += ", ";
                    }

                    saveProductBatchError += product.ProductId.ToString();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Get list of phone types
        /// </summary>
        /// <returns>List of Phone types</returns>
        private IList<Phone> GetPhoneTypes()
        {
            var phones = new List<Phone>();
            foreach (var en in Enum.GetValues(typeof(PhoneType)))
            {
                phones.Add(new Phone { PhoneTypeId = (int)en, PhoneType = ((Enum)en).ToEnumDescription() });
            }

            return phones;
        }

        /// <summary>
        /// List Job titles
        /// </summary>
        /// <returns>List of Job titles</returns>
        private IList<JobTitle> GetJobTitles()
        {
            var jobTitles = new List<JobTitle>();
            foreach (var en in Enum.GetValues(typeof(JobTitleType)))
            {
                jobTitles.Add(new JobTitle { JobTitleId = (int)en, Name = ((Enum)en).ToEnumDescription() });
            }

            return jobTitles;
        }

        /// <summary>
        /// Used to get the list of products to remove when disabling a user
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <returns></returns>
        private IList<ProductBatch> GetListOfProductsToRemoveByPersonaId(IRepository repository, long assignUserPersonaId)
        {
            // get a list of all the products assigned to the user and disable them
            IList<PersonaProductUserDetails> userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = assignUserPersonaId, ProductStatusValue = ((Int32)UserUiStatusType.AccountCreationSuccessful).ToString() }).ToList();
            IList<ProductBatch> productsToRemove = new List<ProductBatch>();

            // remove batch record with product Id 4  
            var aop = userProducts.FirstOrDefault(x => x.ProductId == (int)ProductEnum.AssetOptimizer);
            if (aop != null)
                userProducts.Remove(aop);

            if (userProducts != null && userProducts.Any(m => m.ProductId == 89 || m.ProductId == 104))
            {
                //Remove Admin and Support Portal from list if user doesnt have it
                int adminSupportProductId = userProducts.First(m => m.ProductId == 89 || m.ProductId == 104).ProductId;
                var productAttributes = repository.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails, new { PersonaId = assignUserPersonaId, ProductId = adminSupportProductId }).ToList();
                if (productAttributes != null && productAttributes.Count == 0)
                {
                    var adminSupportProduct = userProducts.FirstOrDefault(c => c.ProductId == adminSupportProductId);
                    userProducts.Remove(adminSupportProduct);
                }
            }
            foreach (PersonaProductUserDetails prod in userProducts)
            {
                if (prod.ProductStatus == (int)ProductBatchStatusType.Success)
                {
                    ProductBatch pb = new ProductBatch()
                    {
                        ProductId = prod.ProductId,
                        StatusTypeId = 5,
                        RetryCount = 0,
                        InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
                    };

                    productsToRemove.Add(pb);
                }
            }

            return productsToRemove;
        }

        /// <summary>
        /// get GB(Unified Login) product Role information 
        /// </summary>       
        /// <param name="gbProductBatch"></param>                
        private int GetGreenBookRole(ProductBatch gbProductBatch)
        {
            int roleId = 0;

            var role = gbProductBatch.InputJson.RoleList[0];

            if (role != null && role.Length > 0)
            {
                roleId = int.Parse(gbProductBatch.InputJson.RoleList[0]);
            }

            return roleId;
        }

        /// <summary>
        /// get list of GB(Unified Login) product Roles 
        /// </summary>       
        /// <param name="productBatch"></param>                
        private List<int> GetGreenBookRoles(ProductBatch productBatch)
        {
            List<int> roleIds = new List<int>();

            foreach (var item in productBatch.InputJson.RoleList)
            {
                roleIds.Add(int.Parse(item));
            }

            return roleIds;
        }

        /// <summary>
        /// Used to Update GB(Unified Login) product Role information for a user
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="newRoleIds"></param>
        /// <param name="assignUserPersonaId"></param>        
        /// <param name="existingRoleIds"></param>
        /// <param name="userId"></param>
        private void UpdateGreenBookRole(IRepository repository, List<int> newRoleIds, long assignUserPersonaId, List<long> existingRoleIds, long userId)
        {
            if (newRoleIds.Count > 0)
            {
                //convert int to long
                List<long> newIds = newRoleIds.ConvertAll(x => (long)x);

                //common between new and existing
                List<long> commonIds = newIds.Intersect(existingRoleIds).ToList();

                //remove the common from new id 
                newIds.RemoveAll(x => commonIds.Contains(x));

                //remove the common from old id
                existingRoleIds.RemoveAll(x => commonIds.Contains(x));


                foreach (var id in existingRoleIds)
                {
                    IUserRoleRightRepository userRoleRightRepository = new UserRoleRightRepository(repository);
                    // Delete existing roleId
                    userRoleRightRepository.InsertAssignedRoleToUser(assignUserPersonaId, id, Convert.ToInt32(userId), true);
                }

                foreach (var id in newIds)
                {
                    IUserRoleRightRepository userRoleRightRepository = new UserRoleRightRepository(repository);
                    //Insert new roleId to GB
                    userRoleRightRepository.InsertAssignedRoleToUser(assignUserPersonaId, id, Convert.ToInt32(userId));
                }
            }
        }

        /// <summary>
        /// GetEditorUserAoProduct
        /// </summary>
        /// <param name="userRealPageGuid"></param>
        /// <param name="personaId"></param>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        private IList<string> GetEditorUserAoProduct(Guid userRealPageGuid, long personaId, Guid organizationRealPageId)
        {
            IList<string> aoProductsAvailableForUser = null;
            List<ProductUI> productsAssignedToCompany = null;

            using (var repository = GetRepository())
            {
                productsAssignedToCompany =
                    repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                        new { OrganizationRealPageId = organizationRealPageId }).ToList();
            }

            // if company has AO product assigned then get products available to assign based on editor User
            if (productsAssignedToCompany.Any(w => w.ProductId == (int)ProductEnum.AssetOptimizer))
            {
                var ao = new ManageProductAssetOptimization(_userClaim);
                IList<string> aoProductsAvailableForUserListResponse =
                    ao.GetGbSupportedAoEditorUserProductsToAssign(personaId);

                aoProductsAvailableForUser = aoProductsAvailableForUserListResponse.ToList();
                if (aoProductsAvailableForUser != null && aoProductsAvailableForUser.Count > 0)
                {
                    foreach (var aoProduct in aoProductsAvailableForUser)
                    {
                        productsAssignedToCompany.Add(new PersonaProductUserDetails
                        {
                            ProductId = (int)ProductEnumHelper.GetAoProductEnum(aoProduct)
                        });
                    }
                }
            }

            return aoProductsAvailableForUser;
        }

        /// <summary>
        /// ProcessDisableUserProductData
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="assignUserPersonaId"></param>
        /// <param name="createUserRealPageId"></param>
        /// <param name="createUserPersonaId"></param>
        /// <param name="impersonatorUserId"></param>
        /// <param name="userTypeId"></param>
        public void ProcessDisableUserProductData(IRepository repository, long assignUserPersonaId, Guid createUserRealPageId, long createUserPersonaId, int? userTypeId, long impersonatorUserId)
        {
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "ProcessDisableUserProductData", $"At beginning of method. assignUserPersonaId - {assignUserPersonaId} and createUserPersonaId - {createUserPersonaId}" });
            CreateUserResponse<IErrorData> createUserResponse = new CreateUserResponse<IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IList<ProductSettingType> productSettingTypes = new List<ProductSettingType>();
            RPObjectCache rpcache = new RPObjectCache();
            string saveProductBatchError = "Save Product(s) Error: ";
            string _productStatus = "ProductStatus";
            //Save latest previous product batch to process again when user is re activated.

            IList<ProductBatch> productListToDisable = GetListOfProductsToRemoveByPersonaId(repository, assignUserPersonaId);
            var batchGroup = CreateBatchProcessGroup(repository);

            if (productListToDisable != null)
            {
                if (userTypeId != (int)UserRoleType.UserNoEmail)
                {
                    // check salesforce contact for  all disabled users and set UnifiedLoginUser to false
                    ProductBatch pb = new ProductBatch()
                    {
                        ProductId = (int)ProductEnum.SalesForce,
                        StatusTypeId = 5,
                        RetryCount = 0,
                        BatchProcessorGroupId = batchGroup.BatchProcessorGroupId,
                        InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
                    };
                    productListToDisable.Add(pb);
                }
            }

            if ((productListToDisable?.Count > 0))
            {
                //Do we have the Create & Assign PersonaIds
                if ((createUserPersonaId > 0) && (assignUserPersonaId > 0))
                {
                    var cacheKey = "listProductSettingType";

                    productSettingTypes = rpcache.GetFromCache(cacheKey, 120, () =>
                    {
                        // load from database
                        return repository.GetMany<ProductSettingType>(StoredProcNameConstants.SP_ListProductSettingType, null).ToList();
                    });

                    RepositoryResponse repositoryResponse = new RepositoryResponse();
                    int productStatusTypeId = (from a in productSettingTypes where a.Name.ToUpper() == _productStatus.ToUpper() select a.ProductSettingTypeId).FirstOrDefault();

                    // If any AO product
                    string aoInputJsonString = string.Empty;
                    var aoProductList = productListToDisable.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
                    if (aoProductList.Any())
                    {
                        // Bundle AO products
                        aoInputJsonString = BundleAoProducts(productListToDisable, batchGroup.BatchProcessorGroupId);
                    }

                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productListToDisable)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedPlatform)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products by passing json string
                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, createUserRealPageId, errorStatus, aoInputJsonString, impersonatorUserId);
                        }
                        else
                        {
                            product.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, createUserRealPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson), impersonatorUserId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// GetActivatedUserProductBatchData
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="productBatch"></param>
        /// <returns></returns>
        private IList<ProductBatch> GetActivatedUserProductBatchData(long personaId, IList<ProductBatch> productBatch)
        {
            DefaultUserClaim userClaim = new DefaultUserClaim(ClaimsPrincipal.Current);

            //Any new products are added down the line,we need to update the logic in "getProductBatchForUserClone" to get new products to clone.
            using (var pbRepository = GetRepository())
            {
                IList<ProductBatch> aoProductListToCreate = new List<ProductBatch>();
                List<PersonaProductUserDetails> userProducts = pbRepository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = personaId, ProductStatusValue = ((Int32)UserUiStatusType.Deactivated).ToString() }).ToList();
                if (userProducts.Count > 0)
                {
                    long createUserPersonaId = 0L;
                    //Get the logged in user Current Persona Id
                    if (userClaim.UserRealPageGuid != Guid.Empty)
                    {
                        createUserPersonaId = pbRepository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = userClaim.UserRealPageGuid });
                    }

                    //Next Remove products which are exists in product batch
                    if (productBatch.Count > 0)
                    {
                        foreach (var product in productBatch)
                        {
                            userProducts.RemoveAll(a => a.ProductId == product.ProductId);
                        }
                    }

                    // Check if any AO products exists & then add them in batch
                    var aoProductList = userProducts.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
                    if (aoProductList.Any())
                    {
                        userProducts.RemoveAll(a => a.ProductId == (int)ProductEnum.AssetOptimizer);
                        var batches = GetAoBatchRecords(userClaim.UserRealPageGuid, createUserPersonaId, personaId);
                        foreach (var aoProductBatch in batches)
                        {
                            // add only if userProducts has productId else product is modified after clone
                            if (userProducts.Any(x => x.ProductId == aoProductBatch.ProductId))
                            {
                                productBatch.Add(aoProductBatch);
                            }
                        }
                    }

                    //Then get latest product json data from product batch table which was ran successfully before user disabled
                    foreach (var product in userProducts)
                    {
                        if (!ProductEnumHelper.GetAoProductList().Contains((ProductEnum)product.ProductId))
                        {
                            var batchData = GetUserProductBatchData(personaId, product.ProductId);
                            if (batchData.ProductId > 0)
                                productBatch.Add(batchData);
                        }
                    }

                    //Then remove products which have valid json product data
                    foreach (var product in productBatch)
                    {
                        userProducts.RemoveAll(a => a.ProductId == product.ProductId);
                    }

                    ////Then Remove Products Which are Deselected in UI
                    var profileProductBatch = productBatch.Where(x => x.InputJson.IsAssigned == false);
                    foreach (var pb in profileProductBatch.ToList())
                    {
                        productBatch.Remove(pb);
                    }
                }
            }

            return productBatch;
        }

        /// <summary>
        /// GetUserProductBatchData
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        private ProductBatch GetUserProductBatchData(long personaId, int productId)
        {
            ProductBatch pb = new ProductBatch();
            using (var repository = GetRepository())
            {
                string productUserInputJson = repository.GetOne<string>(StoredProcNameConstants.SP_GetUserProductBatchJsonData, new { ProductId = productId, PersonaId = personaId });
                if (!string.IsNullOrEmpty(productUserInputJson))
                {
                    pb.InputJson = JsonConvert.DeserializeObject<RolePropertyList>(productUserInputJson.Trim());
                    pb.ProductId = productId;
                    pb.StatusTypeId = 5;
                    pb.RetryCount = 0;
                }
            }

            return pb;
        }

        /// <summary>
        /// IsUserProfileChanged
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="oldProfile"></param>
        /// <returns></returns>
        private bool IsUserProfileChanged(IProfileDetail profile, IProfileDetail oldProfile)
        {
            bool isChanged = (
                (!string.Equals(profile.FirstName, oldProfile.FirstName))
                ||
                (!string.Equals(profile.MiddleName, oldProfile.MiddleName))
                ||
                (!string.Equals(profile.LastName, oldProfile.LastName))
            );
            return isChanged;
        }

        /// <summary>
        /// IsUserProfileChanged
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="userDetail"></param>
        /// <returns></returns>
        private bool IsUserProfileChanged(IProfileDetail profile, UserDetails userDetail)
        {
            bool isChanged = (
                (!string.Equals(profile.FirstName, userDetail.FirstName, StringComparison.OrdinalIgnoreCase))
                ||
                (!string.Equals(profile.MiddleName, userDetail.MiddleName, StringComparison.OrdinalIgnoreCase))
                ||
                (!string.Equals(profile.LastName, userDetail.LastName, StringComparison.OrdinalIgnoreCase))
            );
            return isChanged;
        }

        /// <summary>
        /// isUserLoginNameChanged
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="oldProfile"></param>
        /// <returns></returns>
        private bool isUserLoginNameChanged(IProfileDetail profile, IProfileDetail oldProfile)
        {
            return !profile.userLogin.LoginName.Equals(oldProfile.userLogin.LoginName);
        }

        /// <summary>
        /// isEmployeeIdChanged
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="oldProfile"></param>
        /// <returns></returns>
        private bool isEmployeeIdChanged(IProfileDetail profile, IProfileDetail oldProfile)
        {
            var oldEmployeeId = string.IsNullOrEmpty(oldProfile.EmployeeId) ? "" : oldProfile.EmployeeId;
            var newEmployeeId = string.IsNullOrEmpty(profile.EmployeeId) ? "" : profile.EmployeeId;
            return !newEmployeeId.Equals(oldEmployeeId);
        }

        /// <summary>
        /// isSupervisorIdChanged
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="oldProfile"></param>
        /// <returns></returns>
        private bool isSupervisorIdChanged(IProfileDetail profile, IProfileDetail oldProfile)
        {
            return profile.SuperVisorUserId != oldProfile.SuperVisorUserId;
        }

        /// <summary>
        /// isNotificationEmailChanged
        /// </summary>
        /// <param name="priorNotificationEmail"></param>
        /// <param name="newNotificationEmail"></param>
        /// <returns></returns>
        private bool isNotificationEmailChanged(string priorNotificationEmail, string newNotificationEmail)
        {
            return !string.Equals(priorNotificationEmail ?? string.Empty, newNotificationEmail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// GetAoBatchRecords
        /// </summary>
        /// <param name="editorRealPageGuid"></param>
        /// <param name="editorPersonaId"></param>
        /// <param name="newUserPersonaId"></param>
        /// <returns></returns>
        private IList<ProductBatch> GetAoBatchRecords(Guid editorRealPageGuid, long editorPersonaId, long newUserPersonaId)
        {
            var productBatchList = new List<ProductBatch>();

            var manageProductAssetOptimization = new ManageProductAssetOptimization(_userClaim);
            var aoUserCompanyPropertyRoleDetails = manageProductAssetOptimization.CopyRegularUser(editorPersonaId, newUserPersonaId);

            foreach (var aoUserCompanyPropertyRoleDetail in aoUserCompanyPropertyRoleDetails)
            {
                if (aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues == null)
                {
                    aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues = new List<int>();
                }

                if (aoUserCompanyPropertyRoleDetail.PropertyGroups == null)
                {
                    aoUserCompanyPropertyRoleDetail.PropertyGroups = new List<int>();
                }

                var productBatch = new ProductBatch()
                {
                    ProductId = (int)ProductEnumHelper.GetAoProductEnum(aoUserCompanyPropertyRoleDetail.ProductName),
                    StatusTypeId = 5,
                    RetryCount = 0,
                    InputJson =
                        new RolePropertyList()
                        {
                            PropertyList = (from i in aoUserCompanyPropertyRoleDetail.SelectedPortfolioValues select i.ToString()).ToList(),
                            RoleList = (from i in aoUserCompanyPropertyRoleDetail.SelectedRoleValues select i).ToList(),
                            CompanyId = aoUserCompanyPropertyRoleDetail.CompanyId,
                            PropertyGroupList = (from i in aoUserCompanyPropertyRoleDetail.PropertyGroups select i.ToString()).ToList()
                        }
                };

                productBatchList.Add(productBatch);
            }

            return productBatchList;
        }

        /// <summary>
        /// LogAuditActivity
        /// </summary>
        /// <param name="logActivityType"></param>
        /// <param name="logActivityCategoryType"></param>
        /// <param name="message"></param>
        /// <param name="stepName"></param>
        /// <param name="profile"></param>
        /// <param name="additionalInformation"></param>
        private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType, string message, string stepName, IProfileDetail profile, List<AdditionalParameters> additionalInformation = null)
        {
			string userName = string.IsNullOrEmpty(_userClaim.ImpersonatedByName) ? _userClaim.FirstName + " " + _userClaim.LastName : " RealPage Access (" + _userClaim.ImpersonatedByName + ") ";

			var activityDetails = new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = logActivityCategoryType.ToString(),
                CorrelationId = _userClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                OrganizationPartyId = _userClaim.OrganizationPartyId,
                Message = string.Format(message, profile.FirstName, profile.LastName, userName, profile.CreateUserSourceType.ToString()),

                FromUserLoginName = _userClaim.LoginName,
                FromUserLoginId = _userClaim.UserId,
                FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                FromUserFirstName = _userClaim.FirstName,
                FromUserLastName = _userClaim.LastName,

                ToUserLoginName = profile.userLogin.LoginName,
                ToUserLoginId = profile.userLogin.UserId,
                ToUserFirstName = profile.FirstName,
                ToUserLastName = profile.LastName,
                ToUserRealpageId = profile.userLogin.RealPageId.ToString(),
            };

            if (additionalInformation != null)
            {
                activityDetails.AdditionalInformation = additionalInformation;
            }

            LogActivity.WriteActivity(activityDetails);
        }

        private void AddActivityLog(string logActivityType, LogActivityCategoryType logActivityCategoryType, string message, IPerson person, IUserLoginOnly userLogin = null, IUserOrganization userOrg = null, DefaultUserClaim defaultUserClaim = null)
        {
            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "AddActivityLog", $"At beginning of method for for activity - {message} and correlationId is {_userClaim.CorrelationId}" });
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = logActivityCategoryType.ToString(),
                CorrelationId = defaultUserClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = defaultUserClaim.OrganizationMasterId,
                OrganizationPartyId = defaultUserClaim.OrganizationPartyId,
                Message = message,

                FromUserLoginName = defaultUserClaim.LoginName,
                FromUserLoginId = defaultUserClaim.UserId,
                FromUserFirstName = defaultUserClaim.FirstName,
                FromUserLastName = defaultUserClaim.LastName,
                FromUserRealpageId = defaultUserClaim.UserRealPageGuid.ToString(),

                ToUserLoginId = userLogin.UserId,
                ToUserLoginName = userLogin.LoginName,
                ToUserFirstName = person.FirstName,
                ToUserLastName = person.LastName,
                ToUserRealpageId = userLogin.RealPageId.ToString(),
            });

            WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", messageProperties: new object[] { "AddActivityLog", $"At ending of method for for activity - {message} and correlationId is {_userClaim.CorrelationId}" });
        }

        private DefaultUserClaim GetCurrentUserClaim(ManageProfile profileLogic, Organization org)
        {
            DefaultUserClaim currentUserClaim;
            Guid realPageEmployeeAccessID = _organizationRepository.GetOrganizationAdminUserRealPageId(org.RealPageId);

            if (realPageEmployeeAccessID != Guid.Empty)
            {
                var adminUserLogin = _userLoginRepository.GetUserLoginOnly(realPageEmployeeAccessID);
                var adminProfileDetail = profileLogic.GetProfileDetail(realPageEmployeeAccessID, org.PartyId);

                currentUserClaim = new DefaultUserClaim()
                {
                    OrganizationMasterId = org.BooksMasterId,
                    OrganizationPartyId = org.PartyId,
                    FirstName = adminProfileDetail.FirstName,
                    LastName = adminProfileDetail.LastName,
                    UserId = Convert.ToInt32(adminUserLogin.UserId),
                    LoginName = adminUserLogin.LoginName,
                    UserRealPageGuid = adminUserLogin.RealPageId,
                    CorrelationId = _userClaim.CorrelationId
                };
            }
            else
            {
                currentUserClaim = new DefaultUserClaim()
                {
                    OrganizationMasterId = org.BooksMasterId,
                    OrganizationPartyId = org.PartyId,
                    FirstName = "Automated",
                    LastName = "System",
                    UserId = 1,
                    LoginName = "automatedsystem",
                    UserRealPageGuid = Guid.Empty,
                    CorrelationId = _userClaim.CorrelationId
                };
            }

            return currentUserClaim;
        }

        private string ChangeUserTypeExternal(IRepository repository, Organization organizationExternalUser, OrganizationStatus currentPrimaryOrgStatus, IProfileDetail profile, IPersona persona, IList<UserOrganization> userPersonaOrganizationList, IList<ContactMechanismUsageType> emailUsageType, IUserLoginOnly userLoginOnly, IIdentityProviderType identityProviderType, string userTypeChangedToFromExternal)
        {
            dynamic param;
            long? personaId = null;
            int greenBookRole = 0;
            long userEmailContactMechanismId = 0;
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();

            RepositoryResponse repositoryResponse = new RepositoryResponse();

            bool userIsExternalEverywhere = userPersonaOrganizationList.ToList().All(x => x.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser));

            #region UserType Changed To External OR From External

            if ((userTypeChangedToFromExternal.Equals("ToExternal", StringComparison.OrdinalIgnoreCase)) && (userPersonaOrganizationList.Count > 1) && (userPersonaOrganizationList.ToList().Any(x => x.OrganizationPartyId.Equals(persona.OrganizationPartyId) && x.PrimaryOrganization == true)))
            {
                //Add to External Users as Primary
                param = new
                {
                    UserLoginId = profile.userLogin.UserId,
                    StatusTypeId = profile.userLogin.Status,
                    OrganizationPartyId = organizationExternalUser.PartyId,
                    PrimaryOrganization = true,
                    FromDate = currentPrimaryOrgStatus.FromDate,
                    ThruDate = currentPrimaryOrgStatus.ThruDate,
                    StatusThruDate = currentPrimaryOrgStatus.StatusThruDate
                };
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLoginPersona, param);
                if (repositoryResponse.Id == 0)
                {
                    return "Update User Error: Add to External Users as primary failed.";
                }

                //Create Persona for External Users
                long userLoginPersonaId = repositoryResponse.Id;
                personaId = null;

                param = new
                {
                    PersonRealPageId = profile.RealPageId,
                    UserLoginPersonaId = userLoginPersonaId,
                    OrganizationRealPageId = organizationExternalUser.RealPageId,
                    PersonaTypeId = (int)PersonaType.Primary,
                    UserId = profile.userLogin.UserId,
                    PersonaEnvironmentTypeId = persona.PersonaEnvironmentTypeId,
                    FromDate = currentPrimaryOrgStatus.FromDate,
                    ThruDate = persona.ThruDate,
                    personaId
                };
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePersona, param);
                personaId = repositoryResponse.Id;

                // Linking Persona to a Role based on user type for External Users                
                param = new
                {
                    realPageId = organizationExternalUser.RealPageId
                };
                IList<EnterpriseRole> enterpriseRoles = repository.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_SecurityListRolesByRealPageID, param);

                greenBookRole = enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase)).RoleId;
                param = new
                {
                    personaID = personaId,
                    roleID = greenBookRole,
                    CreatedBy = _userClaim.UserId,
                    personaPrivilgeID = 0
                };

                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
                if (repositoryResponse.Id == 0)
                {
                    return "Update User Error: Linking Persona to a Role based on user type for External Users failed.";
                }

                //Set Default Employment Role within External Users

                #region Set Default Employment Role

                //Set Person Role (Employer, User Type) to Organization
                param = new
                {
                    RoleTypeName = "Organization Role"
                };
                IList<RoleType> roleTypes = repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, param);
                RoleType Employer = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("Employer", StringComparison.OrdinalIgnoreCase));
                RoleType UserType = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User Type", StringComparison.OrdinalIgnoreCase));

                param = new
                {
                    RoleTypeName = "User Role"
                };
                roleTypes = repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, param);
                RoleType SuperUserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
                RoleType UserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
                RoleType UserNoEmailRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
                RoleType rpEmployee = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("realpage employee", StringComparison.OrdinalIgnoreCase));
                RoleType rpExternalUser = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("external user", StringComparison.OrdinalIgnoreCase));

                #endregion

                //Set User Type for External Users

                #region Set User Type

                int roleTypeIdFrom = 0;
                int roleTypeIdTo = (int)Employer.PartyRoleTypeId; //Employer

                switch (profile.UserTypeId)
                {
                    case (int)UserRoleType.User:
                        roleTypeIdFrom = UserRole.PartyRoleTypeId;
                        break;
                    case (int)UserRoleType.SuperUser:
                        roleTypeIdFrom = SuperUserRole.PartyRoleTypeId;
                        break;
                    case (int)UserRoleType.RealPageEmployee:
                        roleTypeIdFrom = rpEmployee.PartyRoleTypeId;
                        break;
                    case (int)UserRoleType.UserNoEmail:
                        roleTypeIdFrom = UserNoEmailRole.PartyRoleTypeId;
                        break;
                    case (int)UserRoleType.ExternalUser:
                        roleTypeIdFrom = rpExternalUser.PartyRoleTypeId;
                        break;
                    default:
                        roleTypeIdFrom = UserRole.PartyRoleTypeId;
                        break;
                }

                roleTypeIdTo = (int)UserType.PartyRoleTypeId; //User Type

                param = new
                {
                    PersonRealPageId = profile.RealPageId,
                    OrganizationRealPageId = organizationExternalUser.RealPageId,
                    RoleTypeIdFrom = roleTypeIdFrom,
                    RoleTypeIdTo = roleTypeIdTo
                };

                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonToOrganization, param);
                if (repositoryResponse == null)
                {
                    return "Update User Error: Link person to External Users failed.";
                }

                #endregion

                #region Preferred Contact Method and Tele-Communication

                if ((profile.TelecommunicationNumber.Count > 0) && (profile.PreferredContactMethodId > 0))
                {
                    var response = UpdateProfile(repository, profile.RealPageId, profile);
                    if (response.Id == 0)
                    {
                        return "Update User Error: Update Preferred ContactMethod and Phone for External Users failed.";
                    }
                }

                #endregion

                #region Email Communication Event

                if (identityProviderType.IsLocal)
                {
                    IList<CommonAddress> orgMechanismList = ListContactMechanismForPerson(repository, organizationExternalUser.RealPageId, emailUsageType);
                    if (userEmailContactMechanismId == 0)
                    {
                        IList<CommonAddress> userMechanismList = ListContactMechanismForPerson(repository, profile.RealPageId, emailUsageType);
                        userEmailContactMechanismId = userMechanismList[0].PartyContactMechanismId;
                    }

                    var orgContactMechanismId = orgMechanismList[0].PartyContactMechanismId;
                    if (orgContactMechanismId > 0 && userEmailContactMechanismId > 0)
                    {
                        long? communicationEventId = null;
                        int statusTypeId = (int)EmailStatusType.EmailPending;
                        string note = "pending";
                        DateTime started = DateTime.UtcNow;
                        DateTime ended = DateTime.UtcNow;
                        dynamic paramCommunicationEvent = new
                        {
                            StatusTypeID = statusTypeId,
                            FromPartyContactMechanismId = orgContactMechanismId,
                            ToPartyContactMechanismId = userEmailContactMechanismId,
                            Started = started,
                            Ended = ended,
                            Note = note,
                            CommunicationEventID = communicationEventId
                        };
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEvent, paramCommunicationEvent);
                        if (repositoryResponse.Id == 0)
                        {
                            return "Update User Error:  Create communication event for External Users failed.";
                        }
                    }
                }

                #endregion
            }

            if ((userTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase)) && (userIsExternalEverywhere))
            {
                //Unlink the user from External Users (Enterprise.PartyRelationship)
                param = new
                {
                    PersonRealPageId = profile.RealPageId,
                    OrganizationRealPageId = organizationExternalUser.RealPageId
                };
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UnlinkPersonToOrganization, param);
                if (repositoryResponse.Id == 0)
                {
                    return "Update User Error: Unlink the user from External Users failed.";
                }

                //Set this Organization as Primary
                param = new
                {
                    UserLoginId = profile.userLogin.UserId,
                    StatusTypeId = currentPrimaryOrgStatus.StatusTypeId,
                    OrganizationPartyId = persona.OrganizationPartyId,
                    Primaryorganization = true,
                    StatusThruDate = currentPrimaryOrgStatus.StatusThruDate
                };
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLoginPersona, param);
                if (repositoryResponse.Id == 0)
                {
                    return "Update User Error: Update User login persona External Users failed.";
                }
            }

            #endregion

            return string.Empty;
        }

        private void DisableAllCompanyProducts(Guid loggedInUserRealPageId, IProfileDetail profile, Organization currentOrg, IRepository repository, long assignUserPersonaId, long createUserPersonaId, IList<Persona> personaList)
        {
            long editorPersonaId = 0;
            var editorRealPageId = Guid.Empty;
            UserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = _userClaim.ImpersonatedBy });
            }

            foreach (var companyPersona in personaList)
            {
                if (companyPersona.OrganizationPartyId != currentOrg.PartyId)
                {
                    dynamic paramOrg = new
                    {
                        RealPageId = currentOrg.RealPageId
                    };

                    var resultDynamic = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListOrganizations, paramOrg);
                    long orgPartyId = 0;
                    if (resultDynamic != null)
                    {
                        foreach (var item in resultDynamic)
                        {
                            editorRealPageId = new Guid(item.PersonRealPageId);
                            orgPartyId = item.PartyId;
                            //var editorPersona = managePersona.GetFirstAvailablePersonaByCompany(editorRealPageId, orgPartyId);
                        }
                    }
                }
                else
                {
                    editorPersonaId = createUserPersonaId;
                    editorRealPageId = loggedInUserRealPageId;
                }

                ProcessDisableUserProductData(repository, companyPersona.PersonaId, editorRealPageId, editorPersonaId, profile.UserTypeId, impersonatorUserLoginOnly.UserId);
            }
        }

        private void FindAndAddExistingProductPersona(IRepository repository, IList<ProductBatch> productList, long userPersonaId, Guid userRealPageId)
        {
            var productsToAdd = new List<int>();
            bool foundExistingAOUser = false;
            var samlAttributes = new List<SamlAttributes>();

            foreach (ProductBatch pb in productList)
            {
                if (ProductEnumHelper.GetAoProductList().Contains((ProductEnum)pb.ProductId))
                {
                    productsToAdd.Add(pb.ProductId);
                    foundExistingAOUser = true;
                }
            }

            if (productsToAdd.Count == 0)
            {
                return;
            }

            dynamic param = new
            {
                RealPageId = userRealPageId
            };
            IList<Persona> personaList = repository.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, param);

            if (foundExistingAOUser)
            {
                productsToAdd.Add((int)ProductEnum.AssetOptimizer);
                // get any existing AO user details and add it to the new products being added so it will append to the existing AO user
                foreach (int productId in ProductEnumHelper.GetAoProductList())
                {
                    if (samlAttributes.Count > 0)
                    {
                        break;
                    }

                    foreach (var persona in personaList)
                    {
                        var productAttributes = repository.GetMany<SamlAttributes>(StoredProcNameConstants.SP_GetProductSamlDetails, new { persona.PersonaId, productId }).ToList();
                        if (productAttributes.Count > 0)
                        {
                            foreach (var attribute in productAttributes)
                            {
                                if (!samlAttributes.Any(p => p.SamlAttributeId == attribute.SamlAttributeId && p.Value == attribute.Value))
                                {
                                    samlAttributes.Add(attribute);
                                }
                            }
                        }

                        if (samlAttributes.Count > 0)
                        {
                            break;
                        }
                    }
                }

                foreach (var productId in productsToAdd)
                {
                    foreach (var samlAttribute in samlAttributes)
                    {
                        param = new
                        {
                            PersonaId = userPersonaId,
                            ProductId = productId,
                            SamlAttributeId = samlAttribute.SamlAttributeId,
                            Value = samlAttribute.Value
                        };
                        var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateSamlUserAttribute, param);
                    }
                }
            }
        }

        private void ProcessActivatedUserProductBatchData(long personaId, Guid createUserRealPageId, long createUserPersonaId)
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            DateTime? fromDate = utcNow;
            DateTime? thruDate = utcMaxValue;
            CreateUserResponse<IErrorData> createUserResponse = new CreateUserResponse<IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IList<IdentityProviderType> identityProviderTypeList = new List<IdentityProviderType>();
            IList<ProductBatch> productBatch = new List<ProductBatch>();
            DefaultUserClaim userClaim = new DefaultUserClaim(ClaimsPrincipal.Current);
            string saveProductBatchError = "Save Product(s) Error: ";
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }

            //Any new products are added down the line,we need to update the logic in "getProductBatchForUserClone" to get new products to clone.
            using (var pbRepository = GetRepository())
            {
                List<PersonaProductUserDetails> userProducts = pbRepository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = personaId, ProductStatusValue = ((Int32)UserUiStatusType.Deactivated).ToString() }).ToList();
                if (userProducts.Count > 0)
                {
                    //Then Get Product Batch Data
                    productBatch = GetActivatedUserProductBatchData(personaId, productBatch);
                    var batchGroup = CreateBatchProcessGroup(pbRepository);

                    // If any AO product
                    string aoInputJsonString = string.Empty;
                    var aoProductList = userProducts.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
                    if (aoProductList.Any())
                    {
                        // Bundle AO products
                        aoInputJsonString = BundleAoProducts(productBatch, batchGroup.BatchProcessorGroupId);
                    }

                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productBatch)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedPlatform)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products by passing json string
                            SaveProductBatch(pbRepository, product, createUserResponse, saveProductBatchError,
                                createUserPersonaId, personaId, createUserRealPageId, errorStatus,
                                aoInputJsonString, impersonatorUserLoginOnly.UserId);
                        }
                        else
                        {
                            product.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
                            SaveProductBatch(pbRepository, product, createUserResponse, saveProductBatchError,
                                createUserPersonaId, personaId, createUserRealPageId, errorStatus,
                                JsonConvert.SerializeObject(product.InputJson), impersonatorUserLoginOnly.UserId);
                        }
                    }
                }
            }
        }

        #endregion

        #region Create UserLoginPersona and Persona

        public class OrganizationPrimary
        {
            public Guid OrganizationRealPageId { get; set; }

            public long OrganizationPartyId { get; set; }

            public bool PrimaryOrganization { get; set; }

            public DateTime OrganizationFromDate { get; set; }

            public DateTime? OrganizationThruDate { get; set; }
        }

        #endregion

        #region Audit

        private void AuditUserUpdate(IProfileDetail oldProfile, IProfileDetail newProfile)
        {
            UserAuditDto oldUser = oldProfile.IProfileDetailToUserAuditDto<UserAuditDto>();
            UserAuditDto newUser = newProfile.IProfileDetailToUserAuditDto<UserAuditDto>();

            newUser.UserType = ((UserRoleType)newProfile.UserTypeId).ToEnumDescription();
            oldUser.UserType = ((UserRoleType)oldProfile.UserTypeId).ToEnumDescription();

            var auditResult = ExtensionMethods.GenerateUpdateAudit(oldUser, newUser, "user profile", oldProfile.Persona[0].Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId);

            auditResult.ForEach(x =>
            {
                AuditActivityLog(x.OldValue?.ToString() ?? "", x.NewValue?.ToString() ?? "", x.ColumnName.ToString(), x.AuditMessage, newProfile);
            });

            var auditCustomFieldsResult = ExtensionMethods.GetCustomFieldsAudit(oldProfile.CustomFields, newProfile.CustomFields);

            auditCustomFieldsResult.ForEach(x =>
            {
                AuditActivityLog(x.OldValue?.ToString() ?? "", x.NewValue?.ToString() ?? "", x.ColumnName.ToString(), x.AuditMessage, newProfile);
            });

            UserDetails impersonatorUserInfo = _userClaim.ImpersonatedBy == Guid.Empty ? null : GetUserDetails(null, _userClaim.ImpersonatedBy.ToString());
            if (oldProfile.userLogin.Is3rdPartyIDP != newProfile.userLogin.Is3rdPartyIDP)
            {
                var message = impersonatorUserInfo != null
                 ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName})  updated Third party identity provider flag from {oldProfile.userLogin.Is3rdPartyIDP} to {newProfile.userLogin.Is3rdPartyIDP}."
            : $"{_userClaim.FirstName} {_userClaim.LastName}  updated Third party identity provider flag from {oldProfile.userLogin.Is3rdPartyIDP} to {newProfile.userLogin.Is3rdPartyIDP}.";
                AuditActivityLog(oldProfile.userLogin.Is3rdPartyIDP.ToString(), newProfile.userLogin.Is3rdPartyIDP.ToString(), "Third party identity provider", message, newProfile);
            }
        }


        #endregion

        /// <summary>
        /// Insert or update User Status by company
        /// </summary>
        /// <param name="realPageId">User enterprise Id</param>
        /// <param name="organizationPartyId">User enterprise Id</param>
        /// <param name="statusTypeId">statusType Id</param>
        /// <param name="fromDate">FromDate</param>
        /// <param name="thruDate">ThruDate</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdateUserStatusByCompany(Guid realPageId, long organizationPartyId, int statusTypeId, DateTime fromDate, DateTime? thruDate)
        {
            var repositoryResponse = new RepositoryResponse();

            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }

            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    RealPageId = realPageId,
                    OrganizationPartyId = organizationPartyId,
                    StatusTypeId = statusTypeId,
                    FromDate = fromDate,
                    StatusThruDate = thruDate
                };
                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany, param);

                //remove products
                //If Primary Organization - List all Persona(s) across all user companies
                //Else List Persona(s) for a company
                if (statusTypeId == (int)UserUiStatusType.Disabled)
                {
                    param = new
                    {
                        OrganizationPartyId = organizationPartyId,
                        PersonRealPageId = realPageId
                    };
                    var result = repository.GetMany<dynamic>(StoredProcNameConstants.SP_ListPersonaToDisableUserProduct, param);
                    foreach (var item in result)
                    {
                        if (!item.PrimaryOrganization)
                        {
                            ProcessDisableUserProductData(repository, item.PersonaId, item.EditorRealPageId, item.EditorPersonaId, item.UserTypeId, impersonatorUserLoginOnly.UserId);
                        }
                    }
                }

                return repositoryResponse;
            }
        }

        /// <summary>
        /// Update User Activity Attempts
        /// </summary>
        public ActivityAttempt UpdateUserActivityAttempts(string enterpriseUserName,
            ActivityType activityType,
            UserDeviceDetails userDeviceDetails,
            long partyId,
            string authenticationServiceId = "")
        {
            var activityTypeId = (int)activityType;

            if (userDeviceDetails == null)
            {
                userDeviceDetails = new UserDeviceDetails();
            }

            dynamic param = new
            {
                enterpriseUserName,
                activityTypeId,
                userDeviceDetails.BrowserName,
                userDeviceDetails.BrowserType,
                userDeviceDetails.IpAddress,
                userDeviceDetails.IsMobile,
                userDeviceDetails.Platform,
                userDeviceDetails.Version,
                userDeviceDetails.DeviceType,
                userDeviceDetails.Timezone,
                authenticationServiceId,
                partyId
            };

            using (var repository = GetRepository())
            {
                return repository.GetOne<ActivityAttempt>(StoredProcNameConstants.SP_UpdateActivityAttempt, param);
            }
        }

        #region UpdateUser Private Methods

        /// <summary>
        /// Validate if the old user UserType has changed and set the UserBatchEntity values
        /// </summary>
        /// <param name="newProfile">New User</param>
        /// <param name="oldProfile">Old User</param>
        /// <param name="userIsExternalEverywhere"></param>
        /// <returns>An UserBatchEntity object</returns>
        private UserBatchEntity GetUserBatch(IProfileDetail newProfile, IProfileDetail oldProfile, bool userIsExternalEverywhere)
        {
            if (newProfile.UserTypeId != oldProfile.UserTypeId)
            {
                // From Regular User
                // To SuperUser
                if ((oldProfile.UserTypeId == (int)UserRoleType.User) && (newProfile.UserTypeId == (int)UserRoleType.SuperUser))
                {
                    return new UserBatchEntity
                    {
                        BatchProcessUserType = (int)BatchProcessType.UserTypeRegularToAdmin,
                        UserTypeChanged = true,
                        UserTypeName = BatchProcessType.UserTypeRegularToAdmin.ToString(),
                    };

                }

                //To External
                if ((oldProfile.UserTypeId == (int)UserRoleType.User) && (newProfile.UserTypeId == (int)UserRoleType.ExternalUser))
                {
                    return new UserBatchEntity
                    {
                        UserTypeChangedToFromExternal = "ToExternal"
                    };

                }

                // From SuperUser
                // To Regular
                if ((oldProfile.UserTypeId == (int)UserRoleType.SuperUser) && (newProfile.UserTypeId == (int)UserRoleType.User))
                {
                    return new UserBatchEntity
                    {
                        BatchProcessUserType = (int)BatchProcessType.UserTypeAdminToRegular,
                        UserTypeChanged = true,
                        UserTypeName = BatchProcessType.UserTypeAdminToRegular.ToString()
                    };

                }

                //To External
                if ((oldProfile.UserTypeId == (int)UserRoleType.SuperUser) && (newProfile.UserTypeId == (int)UserRoleType.ExternalUser))
                {
                    return new UserBatchEntity
                    {
                        UserTypeChangedToFromExternal = "ToExternal",
                        BatchProcessUserType = (int)BatchProcessType.UserTypeAdminToExternal,
                        UserTypeChanged = true,
                        UserTypeName = BatchProcessType.UserTypeAdminToExternal.ToString()
                    };
                }

                //From Regular (No Email)
                //To Regular
                if ((oldProfile.UserTypeId == (int)UserRoleType.UserNoEmail) && (newProfile.UserTypeId == (int)UserRoleType.User))
                {
                    return new UserBatchEntity
                    {
                        IsUserTypeChangedFromNoEmailToRegular = true
                    };
                }

                //To External
                if ((oldProfile.UserTypeId == (int)UserRoleType.UserNoEmail) && (newProfile.UserTypeId == (int)UserRoleType.ExternalUser))
                {
                    return new UserBatchEntity
                    {
                        IsUserTypeChangedFromNoEmailToExternal = true,
                        UserTypeChangedToFromExternal = "ToExternal"
                    };
                }

                //From External
                //To Regular (No Email) -- NOT allowed
                //To SuperUser (If User is External EveryWhere)
                if ((oldProfile.UserTypeId == (int)UserRoleType.ExternalUser) && (newProfile.UserTypeId == (int)UserRoleType.SuperUser) && (userIsExternalEverywhere))
                {
                    return new UserBatchEntity
                    {
                        UserTypeChangedToFromExternal = "FromExternal",
                        BatchProcessUserType = (int)BatchProcessType.UserTypeExternalToAdmin,
                        UserTypeChanged = true,
                        UserTypeName = BatchProcessType.UserTypeExternalToAdmin.ToString()
                    };
                }

                //To Regular
                if ((oldProfile.UserTypeId == (int)UserRoleType.ExternalUser) && (newProfile.UserTypeId == (int)UserRoleType.User) && (userIsExternalEverywhere))
                {
                    return new UserBatchEntity
                    {
                        UserTypeChangedToFromExternal = "FromExternal"
                    };

                }
            }

            return new UserBatchEntity { };
        }

        /// <summary>
        /// Uptate the person's info
        /// </summary>
        /// <param name="newProfile">New Profile</param>
        /// <param name="repository">IRepository Object</param>
        /// <returns>RepositoryResponse object</returns>
        private RepositoryResponse UpdatePerson(IProfileDetail newProfile, IRepository repository)
        {
            //Setup the parameter values to update the person's info
            var param = new
            {
                RealPageId = newProfile.RealPageId,
                FirstName = newProfile.FirstName,
                MiddleName = newProfile.MiddleName,
                LastName = newProfile.LastName
            };

            //Update the person's info
            return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, param);

        }

        /// <summary>
        /// Update the Persona's info
        /// </summary>
        /// <param name="oldProfile">Old Profile</param>
        /// <param name="repository">IRepository Object</param>
        /// <param name="batchProcessUserType">BatchProcessUserType</param>
        /// <returns>RepositoryResponse Object</returns>
        private RepositoryResponse UpdatePersona(IProfileDetail oldProfile, IRepository repository, int batchProcessUserType)
        {
            int personaTypeId = 0;
            if (batchProcessUserType == (int)BatchProcessType.UserTypeAdminToRegular || batchProcessUserType == (int)BatchProcessType.UserTypeAdminToExternal)
            {
                personaTypeId = (int)PersonaType.Primary;
            }
            else if (batchProcessUserType == (int)BatchProcessType.UserTypeRegularToAdmin || batchProcessUserType == (int)BatchProcessType.UserTypeExternalToAdmin)
            {
                personaTypeId = (int)PersonaType.SuperUser;
            }

            var param = new
            {
                PersonaId = oldProfile.Persona[0].PersonaId,
                PersonaTypeId = personaTypeId,
                PersonaEnvironmentTypeId = oldProfile.Persona[0].PersonaEnvironmentTypeId
            };
            return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, param);
        }

        /// <summary>
        /// Update User Type
        /// </summary>
        /// <param name="newProfile">New Profile</param>
        /// <param name="oldProfile">Old Profile</param>
        /// <param name="repository">IRepository Object</param>
        /// <param name="relationshipType">PartyRelationship Object</param>
        /// <returns>RepositoryResponse Object</returns>
        private RepositoryResponse UpdateUserType(IProfileDetail newProfile, IProfileDetail oldProfile, IRepository repository, PartyRelationship relationshipType)
        {
            var param = new
            {
                PersonRealPageId = newProfile.RealPageId,
                OrganizationRealPageId = oldProfile.Persona[0].Organization.RealPageId,
                UnlinkRoleTypeIdFrom = relationshipType.RoleTypeIdFrom,
                LinkRoleTypeIdFrom = newProfile.UserTypeId,
                RoleTypeIdTo = relationshipType.RoleTypeIdTo
            };

            return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, param);
        }

        /// <summary>
        /// Final method to update the user in the database
        /// </summary>
        /// <param name="updateUserProfileEntity">Update user profile entity</param>
        /// <returns>A repository response</returns>
        private RepositoryResponse UpdateUserData(UpdateUserProfileEntity updateUserProfileEntity)
        {
            dynamic param;
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            UserBatchEntity userBatchEntity;
            bool isFeatureUser = false;
            bool usePropertyInstances = getPropertyInstanceUnifiedLogin();
            bool isDelegateAdmin = GetUnifiedSettingData("delegateadministrators");

            //We can get this with the oldProfile
            bool deleteOldPropertyInstanceMapping = false;
            bool externalUserRelationUpdated = IsExternaUserlRelationUpdated(updateUserProfileEntity, out deleteOldPropertyInstanceMapping);
            RequestParameter dataFilter = new RequestParameter();
            List<CompanySetup> companyList = _organizationRepository.GetCompanyList(null, 0, null, (int)_userClaim.OrganizationPartyId, dataFilter);
            bool isRealpageAccessUser = companyList.Where(a => a.RealPageAccessUser == _userClaim.LoginName).Distinct().Count() > 0;
            var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
            var platformAdminRole = productInternalSettingList.FirstOrDefault(s => s.Name.Equals("PlatformAdminRole", StringComparison.OrdinalIgnoreCase))?.Value;
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            
            int organizationUsePrimaryProperties = 0;
            UnifiedSettingsRepository unifiedSettingsRepository = new UnifiedSettingsRepository();
            var companyProductSettings = unifiedSettingsRepository.GetUnifiedSettings(_userClaim.OrganizationPartyId, "company");

            if (companyProductSettings.Any(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase)))
            {
                var settingValue = companyProductSettings.FirstOrDefault(a => a.Name.Equals("PrimaryProperty", StringComparison.OrdinalIgnoreCase)).Value;
                int.TryParse(settingValue, out organizationUsePrimaryProperties);
            }
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }
            using (var repository = GetRepository())
            {
                //Begin the transaction
                repository.UnitOfWork.BeginTransaction();

                param = new
                {
                    UserLoginId = updateUserProfileEntity.NewProfile.userLogin.UserId,
                    OrganizationPartyId = updateUserProfileEntity.OldProfile.Persona[0].OrganizationPartyId
                };
                IList<UserLoginPersona> userLoginPersonaList = repository.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona, param);

                List<int> greenBookRoles = new List<int>();

                userBatchEntity = GetUserBatch(updateUserProfileEntity.NewProfile, updateUserProfileEntity.OldProfile, updateUserProfileEntity.UserIsExternalEverywhere);

                bool profileChanged = IsUserProfileChanged(updateUserProfileEntity.NewProfile, updateUserProfileEntity.OldProfile);
                bool loginNamechanged = isUserLoginNameChanged(updateUserProfileEntity.NewProfile, updateUserProfileEntity.OldProfile);
                bool employeeIdChanged = isEmployeeIdChanged(updateUserProfileEntity.NewProfile, updateUserProfileEntity.OldProfile);
                bool supervisorIdChanged = isSupervisorIdChanged(updateUserProfileEntity.NewProfile, updateUserProfileEntity.OldProfile);

                bool isPrimaryPropertiesUpdated = false;
                bool isEnterpriseRolesUpdated = false;
                string enterpriseUserRoleUpdated = string.Empty;
                bool isEnterpriseRoleUnassigned = false;
                string enterpriseRoleUnassigned = string.Empty;
                var enterpriseRoles = repository.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_SecurityListRolesByRealPageID, new { realPageId = updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId });

                var gbProdBatch = new ProductBatch();

                try
                {
                    repositoryResponse.Id = updateUserProfileEntity.OldProfile.Persona[0].PersonPartyId;

                    #region Update Person

                    if ((profileChanged) && ((updateUserProfileEntity.IsCurrentOrgThePrimaryOrg) || (userBatchEntity.UserTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))))
                    {
                        repositoryResponse = UpdatePerson(updateUserProfileEntity.NewProfile, repository);

                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = "Update User Error: Update person failed.";
                            throw new Exception(repositoryResponse.ErrorMessage);
                        }
                    }

                    #endregion

                    if (repositoryResponse.Id != 0)
                    {
                        IIdentityProviderType idpt = (from a in updateUserProfileEntity.IdentityProviderTypeList where a.IsLocal == (updateUserProfileEntity.NewProfile.userLogin.Is3rdPartyIDP ? false : true) select a).FirstOrDefault();
                        if (idpt == null)
                        {
                            idpt = updateUserProfileEntity.IdentityProviderTypeList[0];
                        }

                        if (updateUserProfileEntity.IsCurrentOrgThePrimaryOrg || userBatchEntity.UserTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))
                        {
                            //Link Identity Provider (ContactMechanismId for the Identity Provider value) to new user by UserLoginId & ActivePersonaId
                            param = new
                            {
                                UserId = updateUserProfileEntity.NewProfile.userLogin.UserId,
                                ContactMechanismID = idpt.ContactMechanismId
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update User Error: Link Identity Provider to UserLogin failed.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }
                        }
                        #region Update RealPartner 
                        if (updateUserProfileEntity.OldProfile.IsRealPartner != updateUserProfileEntity.NewProfile.IsRealPartner && _userClaim.ImpersonatedByName != null)
                        {
                            param = new
                            {
                                UserLoginId = updateUserProfileEntity.NewProfile.userLogin.UserId,
                                StatusTypeId = updateUserProfileEntity.CurrentPrimaryOrgStatus.StatusTypeId,
                                OrganizationPartyId = updateUserProfileEntity.OldProfile.Persona[0].OrganizationPartyId,
                                Primaryorganization = true,
                                StatusThruDate = updateUserProfileEntity.CurrentPrimaryOrgStatus.StatusThruDate,
                                IsRealPartner = updateUserProfileEntity.NewProfile.IsRealPartner
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLoginPersona, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update User Error: Update Realpartner failed.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }


                        }
                        #endregion

                        #region Update UserLogin

                        if (updateUserProfileEntity.NewProfile.userLogin != null)
                        {
                            //check to see if user from date changed to feature date
                            isFeatureUser = updateUserProfileEntity.NewProfile.userLogin.FromDate.Value.Date > DateTime.Now.Date ? true : false;

                            if (updateUserProfileEntity.NewProfile.userLogin.ThruDate == null)
                            {
                                updateUserProfileEntity.NewProfile.userLogin.ThruDate = new DateTime(9999, 12, 31);
                            }

                            param = new
                            {
                                RealPageId = updateUserProfileEntity.NewProfile.RealPageId,
                                LoginName = ((updateUserProfileEntity.IsCurrentOrgThePrimaryOrg) || (userBatchEntity.UserTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))) ? updateUserProfileEntity.NewProfile.userLogin.LoginName : updateUserProfileEntity.UserLoginOnly.LoginName,
                                FromDate = updateUserProfileEntity.NewProfile.userLogin.FromDate.Value,
                                ThruDate = updateUserProfileEntity.NewProfile.userLogin.ThruDate,
                                PartyId = updateUserProfileEntity.OldProfile.Persona[0].OrganizationPartyId
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update User Error: Update user login detail failed.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }

                            //UserType Changed To External OR From External
                            string changeUserTypeExternal = ChangeUserTypeExternal(repository, updateUserProfileEntity.OrganizationExternalUser, updateUserProfileEntity.CurrentPrimaryOrgStatus, updateUserProfileEntity.NewProfile, updateUserProfileEntity.OldProfile.Persona[0], updateUserProfileEntity.UserPersonaOrganizationList, updateUserProfileEntity.EmailUsageType, updateUserProfileEntity.UserLoginOnly, idpt, userBatchEntity.UserTypeChangedToFromExternal);
                            if (changeUserTypeExternal != string.Empty)
                            {
                                repositoryResponse.ErrorMessage = changeUserTypeExternal;
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }

                            //update User custom fields
                            if (updateUserProfileEntity.NewProfile.CustomFields?.Count > 0)
                            {
                                if (updateUserProfileEntity.NewProfile.CustomFields.ToList().Any(c => c.UserLoginPersonaId.Equals(0)))
                                {
                                    updateUserProfileEntity.NewProfile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId);
                                }

                                string customFieldsValuesJson = JsonConvert.SerializeObject(updateUserProfileEntity.NewProfile.CustomFields);
                                bool IsValidJson = ValidateJson.IsValidJson<IList<CustomFieldValue>>(customFieldsValuesJson);
                                if (IsValidJson)
                                {
                                    dynamic paramUserCustomFieldValues = new
                                    {
                                        JSON = customFieldsValuesJson,
                                        CreatedBy = _userClaim.UserId
                                    };
                                    repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdateFieldValue, paramUserCustomFieldValues);
                                    if ((repositoryResponse.Id == 0) && (!string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage)))
                                    {
                                        repositoryResponse.ErrorMessage = $"Update User Error: : Update custom fields values {customFieldsValuesJson} Error: {repositoryResponse.ErrorMessage}.";
                                        throw new Exception(repositoryResponse.ErrorMessage);
                                    }
                                }
                            }

                            bool isUserAccessLevelChanged = false;
                            bool isUserEffectiveDateChanged = false;

                            if (updateUserProfileEntity.NewProfile.userLogin.Is3rdPartyIDP != updateUserProfileEntity.UserLoginOnly.Is3rdPartyIDP)
                            {
                                isUserAccessLevelChanged = true;
                            }

                            if (updateUserProfileEntity.NewProfile.userLogin.FromDate.Value > DateTime.UtcNow)
                            {
                                isUserEffectiveDateChanged = true;
                            }

                            //Check to see if there is any status change or 3rdpartyidp changed or user effective date changed to future date on 
                            //user update then process for new status
                            if ((updateUserProfileEntity.NewProfile.userLogin.IsActive != updateUserProfileEntity.CurrentOrgStatus.IsActive) || isUserAccessLevelChanged || isUserEffectiveDateChanged)
                            {


                                DateTime? statusThruDate = null;
                                UserUiStatusType statusTypeId = UserUiStatusType.UnDefined;

                                //current state user login is disabled and user activated through update process and
                                //user never logedin before then set user status to pending
                                if (updateUserProfileEntity.CurrentOrgStatus.StatusTypeId == (int)UserUiStatusType.Disabled && updateUserProfileEntity.NewProfile.userLogin.IsActive == true && updateUserProfileEntity.UserLoginOnly.LastLogin == null && idpt.IsLocal)
                                {
                                    statusTypeId = UserUiStatusType.Pending;
                                }
                                else
                                {
                                    statusThruDate = null;
                                    statusTypeId = UserUiStatusType.Active;
                                }

                                if (updateUserProfileEntity.NewProfile.userLogin.FromDate.Value <= DateTime.UtcNow &&
                                    updateUserProfileEntity.NewProfile.userLogin.ThruDate.HasValue && updateUserProfileEntity.NewProfile.userLogin.ThruDate.Value.ToUniversalTime() < DateTime.UtcNow)
                                {
                                    statusTypeId = UserUiStatusType.Disabled;
                                    statusThruDate = null;
                                }

                                if (updateUserProfileEntity.NewProfile.userLogin.IsActive == false && updateUserProfileEntity.CurrentOrgStatus.IsActive == true)
                                {
                                    statusTypeId = UserUiStatusType.Disabled;
                                    statusThruDate = null;
                                }

                                //Disabled or Pending state and Effective from date changed to today date,then user need to be in pending state to complete profile
                                // or if the user was 3rd party idp and never logged in and now the user is changing to local login instead, create a new pending activity
                                if (idpt.IsLocal && ((updateUserProfileEntity.NewProfile.userLogin.Status == UserUiStatusType.Disabled) || (updateUserProfileEntity.NewProfile.userLogin.Status == UserUiStatusType.Pending) || (updateUserProfileEntity.UserLoginOnly.Is3rdPartyIDP == true && updateUserProfileEntity.UserLoginOnly.LastLogin == null)))
                                {
                                    if (updateUserProfileEntity.UserLoginOnly != null && updateUserProfileEntity.CurrentOrgStatus.FromDate > DateTime.UtcNow && updateUserProfileEntity.NewProfile.userLogin.FromDate.Value <= DateTime.UtcNow)
                                    {
                                        statusTypeId = UserUiStatusType.Pending;
                                    }
                                }

                                //user effective date changed to future date then set to pending
                                if (isUserEffectiveDateChanged)
                                {
                                    statusTypeId = UserUiStatusType.Disabled;
                                    statusThruDate = null;
                                }

                                if (statusTypeId == UserUiStatusType.Pending)
                                {
                                    // get NewUserRegistration activity exp time                    
                                    var activityDetail = repository.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, new { PartyId = _userClaim.OrganizationPartyId }).ToList();
                                    var newUserRegistrationActivity = activityDetail.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
                                    statusThruDate = updateUserProfileEntity.NewProfile.userLogin.FromDate.Value.Date.AddHours(72); //default
                                    statusThruDate = newUserRegistrationActivity != null ? updateUserProfileEntity.NewProfile.userLogin.FromDate.Value.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : statusThruDate;
                                }

                                if (statusTypeId != UserUiStatusType.UnDefined)
                                {
                                    param = new
                                    {
                                        RealPageId = updateUserProfileEntity.NewProfile.RealPageId,
                                        OrganizationPartyId = updateUserProfileEntity.OldProfile.Persona[0].OrganizationPartyId,
                                        StatusTypeId = statusTypeId,
                                        FromDate = updateUserProfileEntity.NewProfile.userLogin.FromDate,
                                        StatusThruDate = statusThruDate
                                    };
                                    repositoryResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update User Error: Update user status failed.";
                                        throw new Exception(repositoryResponse.ErrorMessage);
                                    }
                                    else
                                    {
                                        if (updateUserProfileEntity.NewProfile.userLogin.IsActive != updateUserProfileEntity.OldProfile.userLogin.IsActive )
                                        {
                                            if (updateUserProfileEntity.IsCurrentOrgThePrimaryOrg
                                               && updateUserProfileEntity.NewProfile.UserTypeId != UserTypeConstants.RegularUserNoEmail
                                               && !(updateUserProfileEntity.NewProfile.organization[0].RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId)
                                               && !updateUserProfileEntity.NewProfile.userLogin.LoginName.Equals($"{updateUserProfileEntity.NewProfile.organization[0].BooksMasterId}admin@realpage.com", StringComparison.OrdinalIgnoreCase))

                                            {
                                                var kafkaProducer = KafkaProducerServiceFactory.Instance;
                                                kafkaProducer.PublishUserStatusChangeEventAsync(new UnifiedLoginUserStatusEvent
                                                {
                                                    persona_id = updateUserProfileEntity.NewProfile.Persona[0].PersonaId,
                                                    user_login_name = updateUserProfileEntity.NewProfile.userLogin.LoginName,
                                                    is_active = updateUserProfileEntity.NewProfile.userLogin.IsActive == true,
                                                    user_activation_deactivation_date = DateTime.UtcNow
                                                }).ConfigureAwait(false);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Update Persona

                        //if user type changes then update persona type
                        if (userBatchEntity.UserTypeChanged)
                        {
                            repositoryResponse = UpdatePersona(updateUserProfileEntity.OldProfile, repository, userBatchEntity.BatchProcessUserType);

                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Persona name was not associated to the Persona.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }
                        }

                        #endregion

                        #region Update User Type

                        //Get the Current User Type
                        string roleTypeName = null;
                        string relationshipTypeName = "User Type";

                        dynamic paramRelType = new
                        {
                            RealPageIdFrom = updateUserProfileEntity.NewProfile.RealPageId,
                            RealPageIdTo = updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId,
                            RoleTypeName = roleTypeName,
                            RelationshipTypeName = relationshipTypeName
                        };
                        PartyRelationship relationshipType = repository.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, paramRelType);

                        //Update the User Type  
                        if ((relationshipType != null) && (relationshipType.RoleTypeIdFrom != updateUserProfileEntity.NewProfile.UserTypeId))
                        {
                            repositoryResponse = UpdateUserType(updateUserProfileEntity.NewProfile, updateUserProfileEntity.OldProfile, repository, relationshipType);

                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update User Error: Unable to set new user type.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }
                        }

                        #endregion

                        #region Update notification email

                        string addressType = "";
                        long partyContactMechanismId = 0;
                        long userContactMechanismId = 0;
                        bool endExistingNotificationEmail = false;

                        #region Existing email check

                        // see if an existing notification email already exists
                        string priorNotificationEmail = "";

                        IList<CommonAddress> result = repository.GetMany<CommonAddress>(StoredProcNameConstants.SP_ListContactMechanismsForPerson, new { updateUserProfileEntity.NewProfile.RealPageId }).ToList();
                        if (result != null)
                        {
                            foreach (var contactMechanismFind in result)
                            {
                                var usageType = updateUserProfileEntity.EmailUsageType.FirstOrDefault(i => i.ContactMechanismUsageTypeId == contactMechanismFind.ContactMechanismUsageTypeId);
                                if (usageType != null)
                                {
                                    contactMechanismFind.contactMechanismUsageType = usageType;
                                    priorNotificationEmail = contactMechanismFind.AddressString;
                                    partyContactMechanismId = contactMechanismFind.PartyContactMechanismId;
                                    userContactMechanismId = contactMechanismFind.ContactMechanismId;
                                    break;
                                }
                            }
                        }

                        #endregion

                        #region Insert new Phone Number from update import file.

                        if (updateUserProfileEntity.NewProfile.IsFromImport && updateUserProfileEntity.NewProfile?.TelecommunicationNumber?.Count > 0)
                        {
                            InsertNewPhoneNumberFromImport(repository, updateUserProfileEntity.NewProfile);
                        }

                        #endregion
                        //update email contact mechanisim if user login name changed
                        bool isUserContactMechanismUpdated = false;
                        if (userContactMechanismId != 0)
                        {
                            // the user already had the contact mechanism so update id
                            if ((updateUserProfileEntity.NewProfile.UserTypeId != (int)UserRoleType.UserNoEmail) && (loginNamechanged || userBatchEntity.IsUserTypeChangedFromNoEmailToRegular || userBatchEntity.IsUserTypeChangedFromNoEmailToExternal))
                            {
                                param = new
                                {
                                    ContactMechanismId = userContactMechanismId,
                                    ElectronicAddressString = updateUserProfileEntity.NewProfile.userLogin.LoginName
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when updating an user login email address.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }

                                isUserContactMechanismUpdated = true;
                            }
                        }
                        else if (updateUserProfileEntity.NewProfile.UserTypeId != (int)UserRoleType.UserNoEmail)
                        {
                            //add the missing email
                            updateUserProfileEntity.NewProfile.NotificationEmail = updateUserProfileEntity.NewProfile.userLogin.LoginName;
                        }

                        // end the existing notification email if one exists
                        if (!string.IsNullOrEmpty(priorNotificationEmail) && string.IsNullOrEmpty(updateUserProfileEntity.NewProfile.NotificationEmail))
                        {
                            // the user had a notification email but no longer, so it needs to be ended
                            endExistingNotificationEmail = true;
                        }

                        if (!string.IsNullOrEmpty(priorNotificationEmail) && !string.IsNullOrEmpty(updateUserProfileEntity.NewProfile.NotificationEmail) && (priorNotificationEmail.ToLower() != updateUserProfileEntity.NewProfile.NotificationEmail.ToLower()))
                        {
                            // the email has changed so end the existing record before creating a new one
                            endExistingNotificationEmail = true;
                        }

                        if (endExistingNotificationEmail)
                        {
                            param = new
                            {
                                RealPageId = updateUserProfileEntity.NewProfile.RealPageId,
                                PartyContactMechanismId = partyContactMechanismId
                            };

                            //end prior notification email
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_ExpirePartyContactMechanism, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "An error was encountered when ending a contact mechanism.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }
                        }

                        //Save the notification email if it exists
                        if (!isUserContactMechanismUpdated && updateUserProfileEntity.NewProfile.NotificationEmail != null && (isFeatureUser || (priorNotificationEmail.ToLower() != updateUserProfileEntity.NewProfile.NotificationEmail.ToLower())) && !string.IsNullOrEmpty(updateUserProfileEntity.NewProfile.NotificationEmail))
                        {
                            if (EmailFormatValidation.IsValidEmail(updateUserProfileEntity.NewProfile.NotificationEmail))
                            {
                                partyContactMechanismId = 0;

                                var EmailContactMechanism = updateUserProfileEntity.EmailUsageType.SingleOrDefault<ContactMechanismUsageType>(p => p.Name == "Email");

                                //Build the parameters for email
                                LinkElectronicAddress electronicAddress = new LinkElectronicAddress();
                                electronicAddress.PartyContactMechanism.FromDate = utcNow;
                                electronicAddress.PartyContactMechanism.ThruDate = utcMaxValue;
                                electronicAddress.ElectronicAddress.AddressString = updateUserProfileEntity.NewProfile.NotificationEmail;
                                electronicAddress.ElectronicAddress.AddressType = EmailContactMechanism.Name;
                                electronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId = (int)EmailContactMechanism.ContactMechanismUsageTypeId;
                                addressType = EmailContactMechanism.Name;

                                long? contactMechanismId = null;
                                param = new
                                {
                                    ContactMechanismId = contactMechanismId
                                };

                                //Create Contact Mechanism
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when creating a contact mechanism.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }

                                contactMechanismId = repositoryResponse.Id;

                                //Associate the Contact Mechanism to a Party
                                IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
                                partyContactMechanism = electronicAddress.PartyContactMechanism;
                                partyContactMechanism.ContactMechanismId = Convert.ToInt32(contactMechanismId);
                                partyContactMechanism.PartyContactMechanismId = partyContactMechanismId;

                                param = new
                                {
                                    updateUserProfileEntity.NewProfile.RealPageId,
                                    partyContactMechanism.PartyContactMechanismId,
                                    partyContactMechanism.ContactMechanismId,
                                    partyContactMechanism.FromDate,
                                    partyContactMechanism.ThruDate
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when creating a contact mechanism.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }

                                //Assign a usage type to the Contact Mechanism
                                partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                                param = new
                                {
                                    partyContactMechanism.PartyContactMechanismId,
                                    electronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when assigning a usage type to the contact mechanism.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }

                                param = new
                                {
                                    ContactMechanismId = contactMechanismId,
                                    ElectronicAddressString = updateUserProfileEntity.NewProfile.NotificationEmail,
                                    ElectronicAddressType = addressType
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when creating an email address.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }

                                // "Pending email notification" For Feature user
                                if (idpt.IsLocal && isFeatureUser)
                                {
                                    IList<CommonAddress> contactMechanismList = ListContactMechanismForPerson(repository, updateUserProfileEntity.CurrentOrg.RealPageId, updateUserProfileEntity.EmailUsageType);
                                    var fromPartyContactMechanismId = contactMechanismList[0].PartyContactMechanismId;
                                    var toPartyContactMechanismId = partyContactMechanism.PartyContactMechanismId;
                                    if (fromPartyContactMechanismId > 0 && toPartyContactMechanismId > 0)
                                    {
                                        long? communicationEventId = null;
                                        int statusTypeId = (int)EmailStatusType.EmailPending;
                                        string note = "pending";
                                        DateTime started = DateTime.UtcNow;
                                        DateTime ended = DateTime.UtcNow;
                                        param = new
                                        {
                                            statusTypeId,
                                            fromPartyContactMechanismId,
                                            toPartyContactMechanismId,
                                            started,
                                            ended,
                                            note,
                                            communicationEventId
                                        };
                                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateCommunicationEvent, param);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Update UserEmployeeId

                        if (updateUserProfileEntity.NewProfile.EmployeeId != updateUserProfileEntity.OldProfile.EmployeeId)
                        {
                            //If the old user has EmployeeId so update if not create the EmployeeId
                            if (updateUserProfileEntity.OldProfile.UserEmployeeId > 0)
                            {
                                dynamic update = new
                                {
                                    updateUserProfileEntity.OldProfile.UserEmployeeId,
                                    updateUserProfileEntity.NewProfile.EmployeeId
                                };

                                RepositoryResponse employeeResult = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateEmployeeId, update);

                                if (employeeResult.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when updating an user employee.";
                                    throw new Exception(employeeResult.ErrorMessage);
                                }

                            }
                            else
                            {
                                dynamic create = new
                                {
                                    userLoginPersonaList[0].UserLoginPersonaId,
                                    updateUserProfileEntity.NewProfile.EmployeeId,
                                };

                                RepositoryResponse employeeResult = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateEmployeeId, create);

                                if (employeeResult.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when updating an user employee.";
                                    throw new Exception(employeeResult.ErrorMessage);
                                }
                            }
                        }

                        #endregion

                        #region Update SuperVisor

                        if (updateUserProfileEntity.NewProfile.SuperVisorUserId != updateUserProfileEntity.OldProfile.SuperVisorUserId)
                        {
                            dynamic update = new
                            {
                                UserId = updateUserProfileEntity.NewProfile.userLogin.UserId,
                                SuperVisorUserId = updateUserProfileEntity.NewProfile.SuperVisorUserId
                            };

                            RepositoryResponse supervisorResult = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertUpdateSuperVisor, update);

                            if (supervisorResult.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "An error was encountered when updating supervisor for an user employee.";
                                throw new Exception(supervisorResult.ErrorMessage);
                            }
                            else
                            {
                                string userName = string.IsNullOrEmpty(_userClaim.ImpersonatedByName) ? _userClaim.FirstName + " " + _userClaim.LastName : "RealPage Access (" + _userClaim.ImpersonatedByName + ") ";
                                param = new
                                {
                                    UserId = updateUserProfileEntity.NewProfile.userLogin.UserId,
                                    OrgPartyId = _userClaim.OrganizationPartyId
                                };
                                var supervisorinfo = repository.GetOne<UserInfoLite>(StoredProcNameConstants.SP_GetSuperVisorId, param);
                                if (supervisorinfo != null)
                                {
                                    string superVisorMessage = $"{userName} updated supervisor for {updateUserProfileEntity.NewProfile.FirstName} {updateUserProfileEntity.NewProfile.LastName}. Set to {supervisorinfo.FirstName} {supervisorinfo.LastName}({supervisorinfo.LoginName}).";
                                    AuditActivityLog(updateUserProfileEntity.OldProfile.SuperVisorUser.LoginName, supervisorinfo.LoginName, "Supervisor", superVisorMessage, updateUserProfileEntity.NewProfile);
                                }
                                else if (updateUserProfileEntity.NewProfile.SuperVisorUserId == 0 && updateUserProfileEntity.NewProfile.SuperVisorUser?.SuperVisorUserId > 0)
                                {
                                    string superVisorMessage = $"{userName} Deleted supervisor for {updateUserProfileEntity.NewProfile.FirstName} {updateUserProfileEntity.NewProfile.LastName}.";
                                    AuditActivityLog(updateUserProfileEntity.OldProfile.SuperVisorUser.LoginName, " ", "Supervisor", superVisorMessage, updateUserProfileEntity.NewProfile);
                                }
                            }
                        }

                        #endregion

                        #region update delegate roles
                        bool oldProfileDelegate = updateUserProfileEntity.OldProfile.IsDelegateAdmin;
                        bool newProfileDelegate = updateUserProfileEntity.NewProfile.IsDelegateAdmin;
                        if (isDelegateAdmin && (newProfileDelegate || oldProfileDelegate))
                        {
                            if ((updateUserProfileEntity.NewProfile.IsDelegateAdmin != updateUserProfileEntity.OldProfile.IsDelegateAdmin))
                            {
                                UpdateDelegateAdminStatus(repository, userLoginPersonaList[0].UserLoginPersonaId, updateUserProfileEntity.NewProfile.IsDelegateAdmin);
                            }
                            // make db call here...
                            repositoryResponse = InsertUpdateDelegateAdminRole(repository, userLoginPersonaList[0].UserLoginPersonaId,
                                                        updateUserProfileEntity.NewProfile.DelegateRoleTemplate.RoleTemplateId.ToList());
                            if (repositoryResponse.ErrorMessage.Length != 0)
                            {
                                repositoryResponse.ErrorMessage = "Unable to Create  Delegate Template role to the User.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }
                        }
                        #endregion

                        #region update external user company association

                        if (FeatureFlag.GetUserCompanyAssociationFeatureFlag())
                        {
                            if (updateUserProfileEntity.NewProfile != null
                                && updateUserProfileEntity.NewProfile.ExternalUserRelationship != null
                                && updateUserProfileEntity.NewProfile.ExternalUserRelationship.ThirdPartyRelationShipId > 0
                                && (externalUserRelationUpdated
                                    || (updateUserProfileEntity.NewProfile.ExternalUserRelationship.OperatorCode != updateUserProfileEntity.OldProfile.ExternalUserRelationship.OperatorCode)
                                    || (updateUserProfileEntity.NewProfile.ExternalUserRelationship.OperatorValue != updateUserProfileEntity.OldProfile.ExternalUserRelationship.OperatorValue)))
                            {
                                param = new
                                {
                                    UserLoginPersonaId = updateUserProfileEntity.NewProfile.ExternalUserRelationship.UserLoginPersonaId,
                                    ThirdPartyRelationshipId = updateUserProfileEntity.NewProfile.ExternalUserRelationship.ThirdPartyRelationShipId,
                                    CompanyName = updateUserProfileEntity.NewProfile.ExternalUserRelationship.ThirdPartyCompanyName,
                                    ThirdPartyCompanyRealPageId = updateUserProfileEntity.NewProfile.ExternalUserRelationship.ThirdPartyCompanyRealPageId,
                                    OperatorCode = updateUserProfileEntity.NewProfile.ExternalUserRelationship.OperatorCode,
                                    OperatorValue = updateUserProfileEntity.NewProfile.ExternalUserRelationship.OperatorValue
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateExternalUserRelationship, param);

                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Update ExternalUser Relationship: Update External User Relationship failed.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }
                            }
                        }
                        #endregion

                        var enterpriseRole = updateUserProfileEntity.NewProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
                        //Update Enterprise role template to persona                        
                        if (enterpriseRole?.InputJson?.RoleList != null && enterpriseRole?.InputJson?.RoleList.Count > 0)
                        {
                            int roleTemplateId = Convert.ToInt32(enterpriseRole.InputJson.RoleList.FirstOrDefault());
                            if (roleTemplateId != 0)
                            {
                                isEnterpriseRolesUpdated = true;
                                object paramObject = new
                                {
                                    RoleTemplateId = roleTemplateId,
                                    OrganizationRealPageId = _userClaim.OrganizationRealPageGuid
                                };
                                var roleTemplateProductRole = repository.GetMany<RoleTemplateProductRole>(StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings, paramObject).ToList();
                                enterpriseUserRoleUpdated = roleTemplateProductRole.Select(x => x.RoleTemplateName).FirstOrDefault();

                                repositoryResponse = InsertUpdateEnterpriseRoleToUser(repository, roleTemplateId, updateUserProfileEntity.OldProfile.Persona[0].PersonaId);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "Unable to update enterprise role to the Persona.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }
                            }
                            else if (roleTemplateId == 0)
                            {
                                //unassign enterprise role from user
                                UnassignEnterpriseRoleFromUser(repository, updateUserProfileEntity.OldProfile.Persona[0].PersonaId);
                            }
                        }
                        else if (updateUserProfileEntity.OldProfile.RoleTemplateId > 0 && updateUserProfileEntity.NewProfile.RoleTemplateId == 0)
                        {
                            isEnterpriseRoleUnassigned = true;
                            object paramObject = new
                            {
                                RoleTemplateId = updateUserProfileEntity.OldProfile.RoleTemplateId,
                                OrganizationRealPageId = _userClaim.OrganizationRealPageGuid
                            };
                            var roleTemplateProductRole = repository.GetMany<RoleTemplateProductRole>(StoredProcNameConstants.SP_GetRoleTemplateProductRoleMappings, paramObject).ToList();
                            enterpriseRoleUnassigned = roleTemplateProductRole.Select(x => x.RoleTemplateName).FirstOrDefault();

                            //unassign enterprise role from user
                            UnassignEnterpriseRoleFromUser(repository, updateUserProfileEntity.OldProfile.Persona[0].PersonaId);
                        }

                        bool notificationEmailChanged = isNotificationEmailChanged(priorNotificationEmail, updateUserProfileEntity.NewProfile.NotificationEmail);

                        if (updateUserProfileEntity.ProductBatchData != null)
                        {
                            var batchGroup = CreateBatchProcessGroup(repository);
                            foreach (var item in updateUserProfileEntity.ProductBatchData)
                            {
                                item.BatchProcessorGroupId = batchGroup.BatchProcessorGroupId;
                            }
                        }

                        if ((updateUserProfileEntity.NewProfile.userLogin.Status != UserUiStatusType.Disabled) && (profileChanged || loginNamechanged || notificationEmailChanged || employeeIdChanged || supervisorIdChanged))
                        {
                            updateUserProfileEntity.EditorAssignedPersonaList.ToList().ForEach(p => { SaveUserProductBatchData(repository, null, p.EditorPersonaId, p.AssignedPersonaId, p.EditorPersonaRealPageId, p.OrganizationRealPageId, null, (Int32)BatchProcessType.ProfileUpdate, updateUserProfileEntity.ProductBatchData, null, p.AssignedUserTypeId); });
                        }

                        if (updateUserProfileEntity.NewProfile.userLogin.IsActive.GetBooleanValue() && !userBatchEntity.UserTypeChanged)
                        {
                            int productCount = SaveProductDetails(repository, updateUserProfileEntity.ProductBatchData, null, updateUserProfileEntity.CreateUserPersonaId, updateUserProfileEntity.OldProfile.Persona[0].PersonaId, updateUserProfileEntity.LoggedInUserRealPageId, updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId, null, updateUserProfileEntity.NewProfile.UserTypeId, updateUserProfileEntity.NewProfile.userLogin.IsActive.GetBooleanValue(), impersonatorUserLoginOnly.UserId, updateUserProfileEntity.AoProductsAvailableForUser, false, false, 0, "update", isRealpageAccessUser);
                        }

  

                        if (!updateUserProfileEntity.NewProfile.userLogin.IsActive.GetBooleanValue())
                        {
                            DisableAllCompanyProducts(updateUserProfileEntity.LoggedInUserRealPageId, updateUserProfileEntity.NewProfile, updateUserProfileEntity.CurrentOrg, repository, updateUserProfileEntity.OldProfile.Persona[0].PersonaId, updateUserProfileEntity.CreateUserPersonaId, updateUserProfileEntity.PersonaList);
                        }

                        if (updateUserProfileEntity.NewProfile.userLogin.IsActive.GetBooleanValue() && userBatchEntity.UserTypeChanged)
                        {
                            SaveUserProductBatchData(repository, null, updateUserProfileEntity.CreateUserPersonaId, updateUserProfileEntity.OldProfile.Persona[0].PersonaId, updateUserProfileEntity.LoggedInUserRealPageId, updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId, null, userBatchEntity.BatchProcessUserType, updateUserProfileEntity.ProductBatchData, updateUserProfileEntity.AoProductsAvailableForUser, updateUserProfileEntity.NewProfile.UserTypeId);
                        }

                        // GreenBook - UnifiedLogin call updating GB Role
                        gbProdBatch = updateUserProfileEntity.NewProfile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

                        if (gbProdBatch != null)
                        {
                            greenBookRoles = GetGreenBookRoles(gbProdBatch);

                            if (greenBookRoles.Count == 0 || userBatchEntity.UserTypeChanged)
                            {
                                greenBookRoles.Clear();
                                greenBookRoles.Add(GetUnifiedPlatformDefaultRole(repository, updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId, enterpriseRoles));
                            }
                        }
                        else
                        {
                            //This will be exceuted when user type changes and no productbatch record for greenbook role is coming from UI
                            if (userBatchEntity.UserTypeChanged)
                            {
                                IList<RoleType> roleTypes;
                                dynamic paramUserRole = new
                                {
                                    RoleTypeName = "User Role"
                                };
                                roleTypes = repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, paramUserRole);
                                var SuperUserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name == "SuperUser");

                                if (SuperUserRole.PartyRoleTypeId == updateUserProfileEntity.NewProfile.UserTypeId)
                                {
                                    greenBookRoles.Add(enterpriseRoles.FirstOrDefault(ur => ur.Role == platformAdminRole).RoleId);
                                }
                                else
                                {
                                    greenBookRoles.Add(GetUnifiedPlatformDefaultRole(repository, updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId, enterpriseRoles));
                                }

                                if ((SuperUserRole.PartyRoleTypeId == updateUserProfileEntity.NewProfile.UserTypeId) && (enterpriseRoles.FirstOrDefault(r => r.Role.Equals(platformAdminRole, StringComparison.OrdinalIgnoreCase)).RoleId > 0))
                                {
                                    gbProdBatch = new ProductBatch()
                                    {
                                        ProductId = (int)ProductEnum.UnifiedPlatform,
                                        InputJson = new RolePropertyList()
                                        {
                                            PropertyList = new List<string>()
                                                { "-1" }
                                        }
                                    };
                                }
                            }
                        }

                        var personaId = updateUserProfileEntity.OldProfile.Persona[0].PersonaId;

                        var duplicateGreenBookroles = new List<int>(greenBookRoles);
                        var duplicateExistingroles = new List<long>(updateUserProfileEntity.ExistingRoleIds);

                        UpdateGreenBookRole(repository, duplicateGreenBookroles, updateUserProfileEntity.OldProfile.Persona[0].PersonaId, duplicateExistingroles, updateUserProfileEntity.UserLoginOnly.UserId);

                        if (updateUserProfileEntity.SaveProductBatchError != "Save Product(s) Error: ")
                        {
                            repositoryResponse.Id = 0;
                            repositoryResponse.ErrorMessage = updateUserProfileEntity.SaveProductBatchError;
                        }
                        else
                        {
                            if ((gbProdBatch != null) && ((gbProdBatch.InputJson?.PropertyList?.Count > 0) || (gbProdBatch.InputJson?.RemovedPropertyList?.Count > 0)))
                            {
                                string propertyJSON = JsonConvert.SerializeObject(gbProdBatch);

                                isPrimaryPropertiesUpdated = true;

                                if (FeatureFlag.GetUserCompanyAssociationFeatureFlag() && deleteOldPropertyInstanceMapping)
                                {
                                    repository.Execute(StoredProcNameConstants.SP_DeletePropertyInstanceMapping, new { PersonaId = updateUserProfileEntity.OldProfile.Persona[0].PersonaId, ProductId = (int)ProductEnum.UnifiedPlatform });
                                }

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_AddUpdatePropertyInstanceMapping, new { PersonaId = updateUserProfileEntity.OldProfile.Persona[0].PersonaId, ProductId = (int)ProductEnum.UnifiedPlatform, PropertyInstanceJSON = propertyJSON });

                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    repositoryResponse.Id = 0;
                    if (repositoryResponse.ErrorMessage.Length == 0)
                    {
                        repositoryResponse.ErrorMessage = "There was a problem updating the user";
                    }
                }
                finally
                {
                    if (repositoryResponse.ErrorMessage.Length == 0)
                    {
                        // if all success then send user's RealPage id back along with id.
                        repositoryResponse.RealPageId = updateUserProfileEntity.NewProfile.RealPageId;

                        //Commit and end transaction.
                        repository.UnitOfWork.Commit();

                        AuditUserUpdate(updateUserProfileEntity.OldProfile, updateUserProfileEntity.NewProfile);
                        
                        //add activity log for Primary property
                        if (isPrimaryPropertiesUpdated && organizationUsePrimaryProperties == 1)
                        {
                            string message = "{2} updated Primary Properties for {0} {1}.";
                            List<AdditionalParameters> additionalParameters = new List<AdditionalParameters>();

                            if (gbProdBatch.InputJson?.PropertyList?.Count > 0)
                            {
                                List<Guid> addedGuid = new List<Guid>();
                                bool allProperties = false;
                                foreach (var item in gbProdBatch.InputJson?.PropertyList)
                                {
                                    if (!item.Equals("-1"))
                                    {
                                        addedGuid.Add(new Guid(item));
                                    }
                                    else
                                    {
                                        allProperties = true;
                                        break;
                                    }
                                }

                                if (!allProperties)
                                {
                                    var properties = _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIds(addedGuid);

                                    foreach (var item in properties)
                                    {
                                        additionalParameters.Add(new AdditionalParameters()
                                        {
                                            Key = "Primary Properties",
                                            Value = "{\"action\" : \"Assigned\", \"value\" : \"" + item.Name + "\"}"
                                        });
                                    }
                                }
                                else
                                {
                                    additionalParameters.Add(new AdditionalParameters()
                                    {
                                        Key = "Primary Properties",
                                        Value = "{\"action\" : \"Assigned\", \"value\" : \"All Properties\"}"
                                    });
                                }
                            }

                            if (gbProdBatch.InputJson?.RemovedPropertyList?.Count > 0)
                            {
                                List<Guid> removedGuid = new List<Guid>();

                                foreach (var item in gbProdBatch.InputJson?.RemovedPropertyList)
                                {
                                    if (Guid.TryParse(item, out Guid propertyGuid))
                                    {
                                        removedGuid.Add(propertyGuid);
                                    }
                                }

                                var properties = _propertyRepository.ListUPFMPropertyInstanceIdByInstanceIds(removedGuid);

                                foreach (var item in properties)
                                {
                                    additionalParameters.Add(new AdditionalParameters()
                                    {
                                        Key = "Primary Properties",
                                        Value = "{\"action\" : \"Removed\", \"value\" : \"" + item.Name + "\"}"
                                    });
                                }
                            }

                            LogAuditActivity(LogActivityTypeConstants.PRIMARY_PROPERTIES, LogActivityCategoryType.User, message, "Update primary property", updateUserProfileEntity.NewProfile, additionalParameters);

                        }
                        //add activity log for Enterprise Roles
                        if (isEnterpriseRolesUpdated || isEnterpriseRoleUnassigned)
                        {
                            string userName = string.IsNullOrEmpty(_userClaim.ImpersonatedByName) ? _userClaim.FirstName + " " + _userClaim.LastName : " RealPage Access (" + _userClaim.ImpersonatedByName + ") ";
                            string enterpriseRolesMessage = $"{userName} updated access for {updateUserProfileEntity.NewProfile.FirstName} {updateUserProfileEntity.NewProfile.LastName} : Enterprise Role: {(isEnterpriseRolesUpdated ? enterpriseUserRoleUpdated + " was granted." : enterpriseRoleUnassigned + " was unassigned.")} ";

                            AddActivityLog(LogActivityTypeConstants.ENTERPRISE_ROLES, LogActivityCategoryType.User, enterpriseRolesMessage, updateUserProfileEntity.NewProfile, updateUserProfileEntity.UserLoginOnly, null, _userClaim);
                        }
                    }
                    else
                    {
                        //Rollback transaction and dispose it.
                        repositoryResponse.Id = 0;
                        repository.UnitOfWork.Rollback();
                    }
                }

                // Activity logging
                if (repositoryResponse.Id > 0 || string.IsNullOrWhiteSpace(repositoryResponse.ErrorMessage))
                {
                    var newRoles = enterpriseRoles?.Where(x => greenBookRoles.Contains(x.RoleId)).ToList().Select(e => e.Role).ToList();
                    if (enterpriseRoles != null && newRoles.Count > 0)
                    {
                        var oldRoles = enterpriseRoles.Where(x => updateUserProfileEntity.ExistingRoleIds.Contains(x.RoleId)).ToList().Select(e => e.Role).ToList();
                        if (newRoles.Except(oldRoles).ToList().Count > 0)
                        {
                            string joinedOldRoles = string.Join(", ", oldRoles);
                            string joinedNewRoles = string.Join(", ", newRoles);
                            var auditMessage = $"{(updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId ? "RealPage user " : String.Empty)}{{2}} changed the Unified Platform role for {{0}} {{1}}. Previous role(s): {joinedOldRoles}. New role(s) : {joinedNewRoles}.";
                            LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, auditMessage, "UpdateUser", updateUserProfileEntity.NewProfile);
                        }
                    }

                    //Log Activity
                    if (userBatchEntity.IsUserTypeChangedFromNoEmailToRegular)
                    {
                        LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, "{0} {1} user type changed from  User (No Email) to Regular User by {2}.", "UpdateUser", updateUserProfileEntity.NewProfile);
                    }
                    else if (updateUserProfileEntity.OldProfile.Persona[0].Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId) //Compare with RealPage employee GUID
                    {
                        LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, "User {0} {1} successfully updated by RealPage user {2}.", "UpdateUser", updateUserProfileEntity.NewProfile);
                    }
                    else
                    {
                        LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, "User {0} {1} successfully updated by user {2}.", "UpdateUser", updateUserProfileEntity.NewProfile);
                    }

                    if (externalUserRelationUpdated)
                    {
                        CreateExternalUpdatelogParams(updateUserProfileEntity);
                    }

                    bool oldProfileDelegate = updateUserProfileEntity.OldProfile.IsDelegateAdmin;
                    bool newProfileDelegate = updateUserProfileEntity.NewProfile.IsDelegateAdmin;
                    if (isDelegateAdmin)
                    {
                        //Get all enterprise role Names by orgpartyid
                        ProductRepository productRepository = new ProductRepository(_userClaim);
                        List<RoleTemplate> roleTemplates = productRepository.GetRoleTemplateList(_userClaim.OrganizationPartyId);

                        if (newProfileDelegate != oldProfileDelegate)
                        {
                            string delegateMessage = "User admin{2}has " + (updateUserProfileEntity.NewProfile.IsDelegateAdmin ? "added" : "removed") + " {0} {1} as Delegate admin";
                            LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, delegateMessage, "UpdateUser", updateUserProfileEntity.NewProfile);
                        }

                        var oldDelegateRoles = updateUserProfileEntity.OldProfile.DelegateRoleTemplate?.RoleTemplateId != null ? updateUserProfileEntity.OldProfile.DelegateRoleTemplate.RoleTemplateId : new List<int>();
                        var newDelegateRoles = updateUserProfileEntity.NewProfile.DelegateRoleTemplate.RoleTemplateId != null ? updateUserProfileEntity.NewProfile.DelegateRoleTemplate.RoleTemplateId : new List<int>();
                        var rolesAdded = newDelegateRoles.Except(oldDelegateRoles).ToList();
                        var rolesRemoved = oldDelegateRoles.Except(newDelegateRoles).ToList();

                        if (rolesRemoved.Count > 0 || !newProfileDelegate)
                        {
                            var userEnterpriseRoles = roleTemplates.Where(r => rolesRemoved.Contains(r.RoleTemplateId));
                            string delegateRolesMessage = "User admin{2}has removed " + string.Join(", ", userEnterpriseRoles.Select(s => s.RoleTemplateName)) + " enterprise roles for Delegate admin {0} {1}";
                            LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, delegateRolesMessage, "UpdateUser", updateUserProfileEntity.NewProfile);
                        }
                        if (rolesAdded.Count > 0)
                        {
                            var userEnterpriseRoles = roleTemplates.Where(r => rolesAdded.Contains(r.RoleTemplateId));
                            string delegateRolesMessage = "User admin{2}has added " + string.Join(", ", userEnterpriseRoles.Select(s => s.RoleTemplateName)) + " enterprise roles to Delegate admin {0} {1}";
                            LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, delegateRolesMessage, "UpdateUser", updateUserProfileEntity.NewProfile);
                        }
                    }
                }

                return repositoryResponse;
            }
        }
        private UserActivityLogInfo GetUserActivityLogInfo(IRepository repository, long personaId, Guid userRealPageId)
        {
            var persona = repository.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, new { PersonaId = personaId });
            UserLoginOnly userLogin = repository.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = userRealPageId });
            var person = repository.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, new { RealPageId = userRealPageId });
            return new UserActivityLogInfo
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                RealPageId = userLogin.RealPageId,
                LoginName = userLogin.LoginName,
                BooksOrganizationMasterId = persona.Organization != null ? persona.Organization.BooksMasterId : 0,
                OrganizationPartyId = persona.OrganizationPartyId,
                UserId = userLogin.UserId
            };
        }
        /// <summary>
        /// inserting new phone number from updat einport file and setting deafualt phone number 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="profile"></param>
        public void InsertNewPhoneNumberFromImport(IRepository repository, IProfileDetail profile)
        {
            ITelecommunicationNumberRepository telecommunicationNumberRepository = new TelecommunicationNumberRepository();
            ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
            IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
            string ContactMechanismUsageTypeName = "phone type";
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            long personaId = profile.Persona[0].PersonaId;
            Persona persona = profile.Persona[0];
            var toUserLogInfo = GetUserActivityLogInfo(repository, personaId, profile.RealPageId);
            UserDetails impersonatorUserInfo = _userClaim.ImpersonatedBy == Guid.Empty ? null :
            repository.GetOne<UserDetails>(StoredProcNameConstants.SP_GetUserDetails, new { UserRealPageId = _userClaim.ImpersonatedBy.ToString() });

            List<TelecommunicationNumber> addUpdateTelecommunications = new List<TelecommunicationNumber>();
            RepositoryResponse repositoryResponse = null;
            IList<TelecommunicationNumber> telecommunications = repository.GetMany<TelecommunicationNumber>(StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson, new { profile.RealPageId }).ToList();
            string insertPhoneNumber = profile.TelecommunicationNumber[0].AreaCode + profile.TelecommunicationNumber[0].PhoneNumber;
            List<string> existingPhoneNumberList = new List<string>();
            TelecommunicationNumber oldDefault = null;
            if (telecommunications.Any())
            {
                telecommunications.ToList().ForEach(m => { existingPhoneNumberList.Add(m.AreaCode + m.PhoneNumber); });
                oldDefault = telecommunications.FirstOrDefault(t => t.IsDefault);
                oldDefault = new TelecommunicationNumber()
                {
                    PhoneNumber = oldDefault.PhoneNumber,
                    ContactMechanismId = oldDefault.ContactMechanismId,
                    ISOCode = oldDefault.ISOCode,
                    CountryCode = oldDefault.CountryCode,
                    ContactMechanismUsageTypeId = oldDefault.ContactMechanismUsageTypeId,
                    IsDefault = oldDefault.IsDefault,
                    AreaCode = oldDefault.AreaCode,
                    contactMechanismUsageType = oldDefault.contactMechanismUsageType
                };
            }
            foreach (var item in telecommunications)
            {
                item.IsDefault = false;
            }
            addUpdateTelecommunications.AddRange(profile.TelecommunicationNumber);
            addUpdateTelecommunications.AddRange(telecommunications);
            dynamic paramEmail = new
            {
                ContactMechanismUsageTypeName
            };
            IList<ContactMechanismUsageType> ContactMechanismUsageTypes = repository.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, paramEmail);

            if (!existingPhoneNumberList.Contains(insertPhoneNumber))
            {
                //Loop through all the Telecommunication numbers
                foreach (ITelecommunicationNumber phone in addUpdateTelecommunications)
                {
                    telecommunicationNumber.ContactMechanismId = phone.ContactMechanismId;
                    telecommunicationNumber.AreaCode = phone.AreaCode;
                    telecommunicationNumber.CountryCode = phone.CountryCode;
                    telecommunicationNumber.PhoneNumber = phone.PhoneNumber;
                    telecommunicationNumber.ISOCode = phone.ISOCode;
                    telecommunicationNumber.IsDefault = phone.IsDefault;
                    if (phone.ContactMechanismId == 0 && !string.IsNullOrEmpty(phone.PhoneNumber))
                    {
                        var phoneType = profile.TelecommunicationNumber.FirstOrDefault(t => t.ContactMechanismId == phone.ContactMechanismId);
                        string PhoneNumberType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == phoneType?.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                        AuditActivityLog($"{phone.ISOCode}({phone.CountryCode}) {phone.PhoneNumber},{PhoneNumberType}", " ", "Added Phone Number", toUserLogInfo, impersonatorUserInfo);
                    }
                    if (phone.ContactMechanismId == 0 && (phone.PhoneNumber.Trim().Length > 0) && (phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0))
                    {
                        dynamic param = new
                        {
                            ContactMechanismId = 0
                        };
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);

                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = ProfileErrorMessage;
                        }
                        else
                        {
                            phone.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);
                            telecommunicationNumber.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);
                            //Associate the Contact Mechanism to a Party
                            param = new
                            {
                                RealPageId = profile.RealPageId,
                                PartyContactMechanismId = 0,
                                ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                                FromDate = utcNow,
                                ThruDate = utcMaxValue
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = ProfileErrorMessage;
                            }
                            else
                            {
                                phone.PartyContactMechanismId = repositoryResponse.Id;
                                //Assign a usage type to the Contact Mechanism
                                partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;
                                param = new
                                {
                                    PartyContactMechanismId = partyContactMechanism.PartyContactMechanismId,
                                    ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                                };
                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = ProfileLinkUsageTypeErrorMessage;
                                }
                            }
                        }
                    }



                    if (telecommunicationNumber.ContactMechanismId > 0 && phone.PhoneNumber.Trim().Length > 0)
                    {
                        dynamic param = new
                        {
                            ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                            AreaCode = telecommunicationNumber.AreaCode,
                            CountryCode = string.IsNullOrWhiteSpace(telecommunicationNumber.CountryCode) ? "+1" : telecommunicationNumber.CountryCode,
                            PhoneNumber = telecommunicationNumber.PhoneNumber,
                            ISOCode = string.IsNullOrWhiteSpace(telecommunicationNumber.ISOCode) ? "US" : telecommunicationNumber.ISOCode,
                            Default = telecommunicationNumber.IsDefault
                        };
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateTelecommunicationNumber, param);
                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = "Update profile Error: Link a telecommunication number details for a person failed.";
                        }
                        else if (telecommunications.Any() && profile.TelecommunicationNumber.Any() && oldDefault.ContactMechanismId == phone.ContactMechanismId)
                        {
                            var newDefault = profile.TelecommunicationNumber.FirstOrDefault(t => t.IsDefault);
                            if (oldDefault != null && newDefault != null)
                            {
                                string oldPhoneType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == oldDefault.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                string newPhoneType = ContactMechanismUsageTypes.Where(r => r.ContactMechanismUsageTypeId == newDefault.contactMechanismUsageType.ContactMechanismUsageTypeId).Select(r => r.Name).FirstOrDefault();
                                if (oldDefault.PhoneNumber != newDefault.PhoneNumber)
                                {
                                    AuditActivityLog($"{oldDefault.ISOCode}({oldDefault.CountryCode})  {oldDefault.PhoneNumber},{oldPhoneType}", $"{newDefault.ISOCode}({newDefault.CountryCode})  {newDefault.PhoneNumber},{newPhoneType}", "Default Phone Number", toUserLogInfo, impersonatorUserInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// logging info related to phone number update in activity log
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="fieldName"></param>
        /// <param name="toUserLogInfo"></param>
        /// <param name="impersonatorUserInfo"></param>
        private void AuditActivityLog(String oldValue, string newValue, string fieldName, UserActivityLogInfo toUserLogInfo, UserDetails impersonatorUserInfo)
        {
            try
            {
                string message = "";
                List<AdditionalParameters> additionalInfo = new List<AdditionalParameters>();
                if (fieldName != "Deleted Phone Number" && fieldName != "Added Phone Number")
                {
                    if (string.IsNullOrEmpty(oldValue))
                    {
                        oldValue = "Blank Value";
                    }
                    if (string.IsNullOrEmpty(newValue))
                    {
                        newValue = "Blank Value";
                    }
                    message = impersonatorUserInfo != null
                    ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated {fieldName} from {oldValue} to {newValue}."
                    : $"{_userClaim.FirstName} {_userClaim.LastName} updated {fieldName} from {oldValue} to {newValue}.";

                    additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = "{\"action\" : \"Updated To\", \"value\" : \"" + (newValue == "Blank Value" ? " " : newValue) + "\"}" });
                    additionalInfo.Add(new AdditionalParameters { Key = fieldName, Value = "{\"action\" : \"Updated From\", \"value\" :  \"" + (oldValue == "Blank Value" ? " " : oldValue) + "\" }" });
                }
                else if (fieldName == "Deleted Phone Number")
                {
                    message = impersonatorUserInfo != null
                                       ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Deleted Phone Number {oldValue}."
                                       : $"{_userClaim.FirstName} {_userClaim.LastName} Deleted Phone Number {oldValue}.";
                    additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = "{\"action\" : \"Deleted\", \"value\" : \"" + oldValue + "\"}" });
                }
                else if (fieldName == "Added Phone Number")
                {
                    message = impersonatorUserInfo != null
                                       ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) Added Phone Number {oldValue}."
                                       : $"{_userClaim.FirstName} {_userClaim.LastName} Added Phone Number {oldValue}.";
                    additionalInfo.Add(new AdditionalParameters { Key = "Phone Number", Value = "{\"action\" : \"Added\", \"value\" : \"" + oldValue + "\"}" });
                }

                var activityDetails = new ActivityDetails
                {
                    LogActivityTypeName = LogActivityTypeConstants.UPDATE_USER,
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
                    Message = message,

                    FromUserLoginName = _userClaim.LoginName,
                    FromUserLoginId = _userClaim.UserId,
                    FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                    FromUserFirstName = _userClaim.FirstName,
                    FromUserLastName = _userClaim.LastName,

                    ToUserLoginName = toUserLogInfo.LoginName,
                    ToUserLoginId = toUserLogInfo.UserId,
                    ToUserFirstName = toUserLogInfo.FirstName,
                    ToUserLastName = toUserLogInfo.LastName,
                    ToUserRealpageId = toUserLogInfo.RealPageId.ToString(),
                };

                if (additionalInfo != null)
                {
                    activityDetails.AdditionalInformation = additionalInfo;
                }

                LogActivity.WriteActivity(activityDetails);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while logging activity for phone number update: ", ex);
            }
        }


        private bool CompareList(List<long> first, List<long> second)
        {
            bool isequal = false;

            isequal = Enumerable.SequenceEqual(first.OrderBy(e => e), second.OrderBy(e => e));

            return isequal;
        }

        private bool getPropertyInstanceUnifiedLogin()
        {
            RPObjectCache rpcache = new RPObjectCache();

            var cacheKey = "getPropertyInstanceUnifiedLogin_" + (int)ProductEnum.UnifiedPlatform;
            string usePropertyInstanceUnifiedLogin = rpcache.GetFromCache<string>(cacheKey, 60, () =>
            {
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                return (productInternalSettingList.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedLogin", StringComparison.OrdinalIgnoreCase))?.Value);
            });

            return usePropertyInstanceUnifiedLogin == "1";
        }

        private bool getPropertyInstanceUnifiedAmenities()
        {
            RPObjectCache rpcache = new RPObjectCache();

            var cacheKey = "getPropertyInstanceUnifiedAmenities_" + (int)ProductEnum.UnifiedPlatform;
            string usePropertyInstanceUnifiedAmenities = rpcache.GetFromCache<string>(cacheKey, 60, () =>
            {
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                return (productInternalSettingList.FirstOrDefault(s => s.Name.Equals("UsePropertyInstanceUnifiedAmenities", StringComparison.OrdinalIgnoreCase))?.Value);
            });

            return usePropertyInstanceUnifiedAmenities == "1";
        }

        private List<ProductBatch> UpdateProductBatchDataWithPrimaryProperties(long editorPersonaId, long userPersonaId, ProductBatch primaryPropertiesBatch, List<ProductBatch> productBatch, List<string> currentprimaryProperties)
        {
            List<ProductBatch> finalProductBatch = new List<ProductBatch>();
            List<PersonaProductUserDetails> userProducts = new List<PersonaProductUserDetails>();
            using (var pbRepository = GetRepository())
            {
                userProducts = pbRepository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = userPersonaId, ProductStatusValue = ((Int32)ProductBatchStatusType.Success).ToString() }).ToList();
            }
            using (var pbRepository = GetRepository())
            {
                var editorUserProducts = pbRepository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = editorPersonaId, ProductStatusValue = ((Int32)ProductBatchStatusType.Success).ToString() }).ToList();
                var aoProductListforEditorUser = editorUserProducts.Any(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId));
                if (!aoProductListforEditorUser)
                {
                    foreach (var item in userProducts.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList())
                    {
                        userProducts.Remove(item);
                    }
                }
            }

            if (userProducts.Count > 0)
            {
                DefaultUserClaim userClaim = new DefaultUserClaim(ClaimsPrincipal.Current);
                ManageCloneProductBatch manageProductBatch = new ManageCloneProductBatch(userClaim);

                UPFMProperty upfmProperty = new UPFMProperty();
                var userPersona = _managePersona.GetPersona(userPersonaId);
                List<string> filteredList = null;
                List<string> updatedPrimaryProperties = primaryPropertiesBatch.InputJson.PropertyList.ToList();
                List<string> removedPrimaryProperties = (primaryPropertiesBatch.InputJson.RemovedPropertyList == null) ? new List<string>() : primaryPropertiesBatch.InputJson.RemovedPropertyList.ToList();

                if (removedPrimaryProperties?.Count > 0)
                {
                    currentprimaryProperties = currentprimaryProperties.Except(removedPrimaryProperties, StringComparer.OrdinalIgnoreCase).ToList();
                }

                if (updatedPrimaryProperties?.Count > 0)
                {
                    currentprimaryProperties.AddRange(updatedPrimaryProperties);
                    filteredList = currentprimaryProperties;
                }
                else
                {
                    filteredList = currentprimaryProperties;
                }

                var logData = new Dictionary<string, object> { { "updatedprimarypropertieslist", filteredList } };
                WriteToLog(LogEventLevel.Debug, "{ActionName} - {state}", logData, messageProperties: new object[] { "InsertAssignedUserPropertyData", $"Generating data for persona {userPersonaId}" });

                if (filteredList?.Count > 0)
                {
                    upfmProperty.id = filteredList;

                    var personaOrganization = userPersona.Organization;
                    bool isExternalUser = false;
                    if (personaOrganization.RelationshipType != null && personaOrganization.RoleNameFrom != null)
                    {
                        isExternalUser = personaOrganization.RelationshipType.Equals("User Type", StringComparison.OrdinalIgnoreCase) && personaOrganization.RoleNameFrom.Equals("External User", StringComparison.OrdinalIgnoreCase);
                    }

                    IPersonaRepository personaRepository = new PersonaRepository(userClaim);
                    var personaProductSettings = personaRepository.GetPersonaProductSettings(userPersonaId);

                    bool personaProductUsePrimaryProperty = false;
                    bool usePrimaryProperties = false;

                    //Next Remove products which are exists in product batch
                    foreach (var product in productBatch)
                    {
                        if (product.ProductId != (int)ProductEnum.UnifiedUI)
                        {
                            finalProductBatch.Add(product);
                        }

                        userProducts.RemoveAll(a => a.ProductId == product.ProductId);
                    }

                    foreach (var product in userProducts.ToList())
                    {
                        bool productEnabledForPrimaryProperty = IsProductEnabledForUsePrimaryProperty(product.ProductId);
                        var productSetting = personaProductSettings.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase) && item.ProductId == product.ProductId);

                        if (productSetting != null)
                        {
                            personaProductUsePrimaryProperty = productSetting.Value.Trim() == "1" ? true : false;
                        }

                        usePrimaryProperties = productEnabledForPrimaryProperty && personaProductUsePrimaryProperty;
                        if (!usePrimaryProperties)
                        {
                            userProducts.Remove(product);
                        }
                    }

                    //Then Get Product Batch Data
                    IList<ProductBatch> pbData = manageProductBatch.GetUserProductBatchData(userPersonaId, userProducts, editorPersonaId, upfmProperty, personaProductSettings, isExternalUser);

                    foreach (ProductBatch pb in pbData)
                    {
                        finalProductBatch.Add(pb);
                    }

                    return finalProductBatch;
                }

            }

            return productBatch;
        }

        private bool IsProductEnabledForUsePrimaryProperty(int productId)
        {
            ProductInternalSetting productInternalSetting = new ProductInternalSetting();
            IProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository();
            var productInternalSettingList = productInternalSettingRepository.GetProductInternalSettings(productId);
            productInternalSetting = productInternalSettingList.FirstOrDefault(item => item.Name.Equals("UsePrimaryProperties", StringComparison.OrdinalIgnoreCase));

            if (productInternalSetting != null)
            {
                return productInternalSetting.Value.Trim() == "1" ? true : false;
            }
            return false;
        }

        /// <summary>
        /// Map enterprise role to Persona
        /// </summary>
        /// <param name="repository">IRepository Object</param>
        /// <param name="roleTemplateId">RoleTemplateId</param>
        /// <param name="personaId">PersonaId</param>
        /// <returns>RepositoryResponse Object</returns>
        private RepositoryResponse InsertUpdateEnterpriseRoleToUser(IRepository repository, int roleTemplateId, long? personaId)
        {
            var param = new
            {
                RoleTemplateId = roleTemplateId,
                PersonaId = personaId
            };
            return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertUpdateRoleTemplateUserMapping, param);
        }

        private RepositoryResponse InsertUpdateDelegateAdminRole(IRepository repository, long userLoginPersonaId, List<int> templateRoleLists)
        {
            dynamic param = new
            {
                UserLoginPersonaId = userLoginPersonaId,
                TargetRoleTemplateLists = TableValueParamHelper.ConvertToTableValuedParameter(templateRoleLists, "enterprise.intlisttype")
            };
            return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_InsertUpdateDelegateAdminTemplate, param);
        }
        private RepositoryResponse UpdateDelegateAdminStatus(IRepository repository, long userLoginPersonaId, bool isDelegateAdmin)
        {
            var param = new
            {
                IsDelegateAdmin = isDelegateAdmin,
                UserLoginPersonaId = userLoginPersonaId,

            };
            var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateDelegateAdminStatus, param);
            return result;
        }
        /// <summary>
        /// Unassign EnterpriseRole From User
        /// </summary>
        /// <param name="repository">IRepository Object</param>
        /// <param name="personaId">PersonaId</param>
        /// <returns>RepositoryResponse Object</returns>
        private RepositoryResponse UnassignEnterpriseRoleFromUser(IRepository repository, long personaId)
        {
            var param = new
            {
                PersonaId = personaId
            };
            return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UnassignEnterpriseRoleFromUser, param);
        }

        private bool IsExternaUserlRelationUpdated(UpdateUserProfileEntity updateUserProfileEntity, out bool delOldProperyInstanceMapping)
        {
            bool isUpdated = false;
            delOldProperyInstanceMapping = false;

            var newData = updateUserProfileEntity.NewProfile.ExternalUserRelationship;
            var oldData = updateUserProfileEntity.OldProfile.ExternalUserRelationship;

            //if id == 1 ui will not send the comapny name but company guid
            if (newData != null)
            {
                if (newData.ThirdPartyRelationShipId == 1 || newData.ThirdPartyRelationShipId == 10)
                {
                    if (GetUnifiedSettingData("owneroperatorrelationship"))
                    {

                        var org = _organizationRepository.GetOrganization(newData.ThirdPartyCompanyRealPageId);

                        if (newData.OperatorCode != oldData.OperatorCode
                            || newData.OperatorValue != oldData.OperatorValue)
                        {
                            isUpdated = true;
                        }
                    }
                    else
                    {
                        var externalRelationship = GetExternalUserRelationship(updateUserProfileEntity.NewProfile.ExternalUserRelationship.UserLoginPersonaId);
                        if (newData.ThirdPartyRelationShipId != oldData.ThirdPartyRelationShipId ||
                            externalRelationship.ThirdPartyCompanyName != newData.ThirdPartyCompanyName)
                        {
                            isUpdated = true;
                        }
                    }

                }
                else
                {
                    isUpdated = (newData.ThirdPartyCompanyName != oldData.ThirdPartyCompanyName ||
                                 newData.ThirdPartyRelationShipId != oldData.ThirdPartyRelationShipId);
                }
            }
            return isUpdated;
        }

        private void CreateExternalUpdatelogParams(UpdateUserProfileEntity updateUserProfileEntity)
        {
            List<AdditionalParameters> additionalParams = new List<AdditionalParameters>();

            var oldData = updateUserProfileEntity.OldProfile.ExternalUserRelationship;
            var newData = updateUserProfileEntity.NewProfile.ExternalUserRelationship;

            RelationshipTypeRepository _relationshipTypeRepository = new RelationshipTypeRepository();
            List<UserRelationShipType> userRelationShipTypes = (List<UserRelationShipType>)_relationshipTypeRepository.GetUserRelationShipTypes(partyId: updateUserProfileEntity.NewProfile.Persona[0].OrganizationPartyId);
            var newUserRelationshipType = userRelationShipTypes.Where(u => u.ThirdPartyRelationshipId == updateUserProfileEntity.NewProfile.ExternalUserRelationship.ThirdPartyRelationShipId).Select(p => p.UserRelationshipName).FirstOrDefault();
            newData.ThirdPartyRelationShip = newUserRelationshipType;
            if (newData.ThirdPartyRelationShipId != 0)
            {
                if (!GetUnifiedSettingData("owneroperatorrelationship")) //Operator Setting is NOT ENABLED for the company
                {
                    if (oldData.ThirdPartyRelationShipId != newData.ThirdPartyRelationShipId)
                    {
                        additionalParams.Add(
                            new AdditionalParameters { Key = "User Relationship", Value = "{\"action\" : \"Updated To\", \"value\" : \"" + newData.ThirdPartyRelationShip + "\"}" }
                        );
                        additionalParams.Add(
                            new AdditionalParameters { Key = "User Relationship", Value = "{\"action\" : \"Updated From\", \"value\" : \"" + oldData.ThirdPartyRelationShip + "\"}" }
                        );
                    }

                    if (oldData.ThirdPartyCompanyName != newData.ThirdPartyCompanyName)
                    {
                        additionalParams.Add(
                            new AdditionalParameters { Key = "Company Name", Value = "{\"action\" : \"Updated To\", \"value\" : \"" + newData.ThirdPartyCompanyName + "\"}" }
                        );
                        additionalParams.Add(
                            new AdditionalParameters { Key = "Company Name", Value = "{\"action\" : \"Updated From\", \"value\" : \"" + oldData.ThirdPartyCompanyName + "\"}" }
                        );
                    }
                }

                else //Operator Settig is ENABLED for the company
                {
                    if (oldData.ThirdPartyRelationShipId == newData.ThirdPartyRelationShipId)
                    {
                        if (oldData.ThirdPartyRelationShipId == 1)
                        {
                            if (oldData.ThirdPartyCompanyRealPageId != newData.ThirdPartyCompanyRealPageId)
                            {

                                if (oldData.ThirdPartyCompanyName != newData.ThirdPartyCompanyName)
                                {
                                    additionalParams.Add(
                                        new AdditionalParameters { Key = "Operator", Value = "{\"action\" : \"Updated To\", \"value\" : \"" + newData.OperatorValue + "\"}" }
                                    );
                                    additionalParams.Add(
                                        new AdditionalParameters { Key = "Operator", Value = "{\"action\" : \"Updated From\", \"value\" : \"" + oldData.OperatorValue + "\"}" }
                                    );
                                }
                            }
                        }
                        else
                        {
                            if (oldData.ThirdPartyCompanyName != newData.ThirdPartyCompanyName)
                            {
                                if (oldData.ThirdPartyCompanyName != newData.ThirdPartyCompanyName)
                                {
                                    additionalParams.Add(
                                        new AdditionalParameters { Key = "Company Name", Value = "{\"action\" : \"Updated To\", \"value\" : \"" + newData.ThirdPartyCompanyName + "\"}" }
                                    );
                                    additionalParams.Add(
                                        new AdditionalParameters { Key = "Company Name", Value = "{\"action\" : \"Updated From\", \"value\" : \"" + oldData.ThirdPartyCompanyName + "\"}" }
                                    );
                                }
                            }
                        }
                    }
                    else
                    {
                        additionalParams.Add(
                            new AdditionalParameters { Key = "User Relationship", Value = "{\"action\" : \"Updated To\", \"value\" : \"" + newData.ThirdPartyRelationShip + "\"}" }
                        );
                        additionalParams.Add(
                            new AdditionalParameters { Key = "User Relationship", Value = "{\"action\" : \"Updated From\", \"value\" : \"" + oldData.ThirdPartyRelationShip + "\"}" }
                        );

                        //if changed to 1
                        if (newData.ThirdPartyRelationShipId == 1)
                        {
                            if (oldData.ThirdPartyCompanyName != newData.ThirdPartyCompanyName)
                            {
                                additionalParams.Add(
                                    new AdditionalParameters { Key = "Operator", Value = "{\"action\" : \"Updated To\", \"value\" : \"" + newData.OperatorValue + "\"}" }
                                );
                                additionalParams.Add(
                                    new AdditionalParameters { Key = "Operator", Value = "{\"action\" : \"Updated From\", \"value\" : \"" + oldData.OperatorValue + "\"}" }
                                );
                            }
                        }
                        //if changed away from 1
                        else
                        {
                            additionalParams.Add(
                                        new AdditionalParameters { Key = "Company Name", Value = "{\"action\" : \"Updated To\", \"value\" : \"" + newData.ThirdPartyCompanyName + "\"}" }
                                    );
                            additionalParams.Add(
                                new AdditionalParameters { Key = "Company Name", Value = "{\"action\" : \"Updated From\", \"value\" : \"" + oldData.ThirdPartyCompanyName + "\"}" }
                            );
                        }
                    }
                }
                UserDetails impersonatorUserInfo = _userClaim.ImpersonatedBy == Guid.Empty ? null : GetUserDetails(null, _userClaim.ImpersonatedBy.ToString());

                string mainMessage = impersonatorUserInfo != null
                 ? $"RealPage Access ({impersonatorUserInfo.FirstName} {impersonatorUserInfo.LastName}) updated User Relationship from {oldData.ThirdPartyRelationShip} to {newData.ThirdPartyRelationShip}."
                 : $"{_userClaim.FirstName} {_userClaim.LastName} updated User Relationship from {oldData.ThirdPartyRelationShip} to {newData.ThirdPartyRelationShip}.";
                LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, mainMessage, "UpdateUser", updateUserProfileEntity.NewProfile, additionalParams);
            }
        }

        public bool GetUnifiedSettingData(string settingName)
        {
            try
            {
                var data = _manageUnifiedSettings.GetCompanyInternalSettings(_userClaim.OrganizationRealPageGuid, "UPFM", "company");
                return data?.Keys?.Where(p => p.Name == settingName)?.FirstOrDefault()?.Value == "1";
            }
            catch (Exception exp)
            {
                //by pass
            }
            return false;
        }

        #endregion

        #region Multi Domain User Check

        private UserOrganizationExists IsLoginNameExistsAsAdminInOtherDomain(string loginName, Guid organizationRealPageId, long booksMasterId)
        {
            UserOrganizationExists userOrganizationExists = new UserOrganizationExists();
            //Organization orgDetails = _organizationRepository.GetOrganization(realPageId: organizationRealPageId);
            IList<UserOrganization> userPersonaOrganizationList = _userLoginRepository.ListOrganizationByLoginName(loginName);
            bool isAdminUser = false;
            bool isRegularUser = false;
            userOrganizationExists.UserExistsAsAdminInOtherDomain = false;
            // userOrganizationExists.OrgIsRealpageEmployee = (orgDetails.Name.ToLower().Replace(" ", "") == UserRoleType.RealPageEmployee.ToString().ToLower());

            userOrganizationExists.UserExists = (userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0);
            userOrganizationExists.UserExistsInThisOrganization = (userPersonaOrganizationList != null && userPersonaOrganizationList.Count >= 0 && userPersonaOrganizationList.ToList().Any(a => a.OrganizationRealPageId == organizationRealPageId));
            userOrganizationExists.UserExistsAsNoEmail = userPersonaOrganizationList != null && userPersonaOrganizationList.Count > 0 && userPersonaOrganizationList.Any(p => (p.PartyRoleTypeId == (int)UserRoleType.UserNoEmail));

            if (userOrganizationExists.UserExists && !userOrganizationExists.UserExistsInThisOrganization)
            {
                UserOrganization userOrganization = userPersonaOrganizationList.ToList().FirstOrDefault(m => m.PrimaryOrganization.Equals(true));

                isAdminUser = userOrganization != null && userOrganization.PartyRoleTypeId == (int)UserRoleType.SuperUser;
                isRegularUser = userOrganization != null && userOrganization.PartyRoleTypeId == (int)UserRoleType.User;

                if (userOrganization != null && (isAdminUser || isRegularUser) && userOrganization.BooksCustomerMasterId == booksMasterId)
                {
                    var orgDomains = _organizationRepository.GetOrganizationListByBooksCustomerMasterId(booksMasterId);

                    if (orgDomains.Count > 1)
                    {
                        userOrganizationExists.UserExists = false;
                        userOrganizationExists.UserExistsAsAdminInOtherDomain = isAdminUser;
                        userOrganizationExists.UserExistsAsRegularUserInOtherDomain = isRegularUser;
                    }
                }
            }

            return userOrganizationExists;
        }

        private int GetUnifiedPlatformDefaultRole(IRepository repository, Guid realPageId, IEnumerable<EnterpriseRole> enterpriseRoles)
        {
            var paramDefaultRole = new
            {
                RealPageID = realPageId
            };
            var defaultRole = repository.GetOne<dynamic>(StoredProcNameConstants.SP_GetUnifiedLoginDefaultRole, paramDefaultRole);
            if (defaultRole != null)
            {
                return defaultRole.RoleId;
            }
            else
            {
                return enterpriseRoles.FirstOrDefault(rl => rl.Role == "Basic End User").RoleId;
            }
        }
        public void AuditActivityLog(String oldValue, string newValue, string fieldName, string message, IProfileDetail profile)
        {
            try
            {
                var additionalInfo = new List<AdditionalParameters>
                        {
                             new AdditionalParameters { Key = fieldName, Value  = "{\"action\" : \"Updated To\", \"value\" : \"" + (newValue == "Blank Value" ? " " : newValue) + "\"}" },
                             new AdditionalParameters {  Key = fieldName, Value  = "{\"action\" : \"Updated From\", \"value\" :  \"" + (oldValue == "Blank Value" ? " " : oldValue) + "\" }" },
                        };
                LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, message, "UpdateUser", profile, additionalInfo);
            }
            catch (Exception ex)
            { }
        }
        #endregion
    }
}