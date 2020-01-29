using Newtonsoft.Json;
using RP.Enterprise.Foundation.Audit.Core.Component;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
        }

        public UserRepository(IRepository repository) : base(repository)
        {
            _userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            _userLoginRepository = new UserLoginRepository(repository);
            _managePersona = new ManagePersona(repository);
            _organizationRepository = new OrganizationRepository(repository);
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
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Get User Product Details By UserId
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <returns>UserProduct object</returns>
        public UserProduct GetUserProductDetailsByUserId(int userId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<UserProduct>("Auth.GetUserProductDetailsByUserId", new { userId });
            }
        }

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

        /// <summary>
        /// Get enterprise user by user id
        /// </summary>
        /// <param name="enterpriseUserId">enterprise User Id</param>
        /// <returns>UserLogin object</returns>
        public SO.User GetEnterpriseUser(int enterpriseUserId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<SO.User>(StoredProcNameConstants.SP_GetUserById, new { userid = enterpriseUserId });
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

            IUserLoginRepository userLoginRepository = new UserLoginRepository();
            IUserLoginOnly userLoginOnly = userLoginRepository.GetUserLoginOnly(newProfile.userLogin.LoginName);
            if (userLoginOnly != null)
            {
                //Get User Details before save
                UserDetails userDetails = GetUserDetails(personaId: null, userRealPageId: userLoginOnly.RealPageId.ToString());
                //Check if ONLY user profile changed without any product changes
                profileChanged = IsUserProfileChanged(newProfile, userDetails);

                userPersonaOrganizationList = userLoginRepository.ListOrganizationByLoginName(newProfile.userLogin.LoginName);
                if (userPersonaOrganizationList.ToList().Any(i => i.OrganizationPartyId.Equals(newProfile.organization[0].PartyId)))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "User.CreateUser.1";
                    errorStatus.ErrorMsg = "Username already exists in this company.";
                    createUserResponse.Status = errorStatus;
                    createUserResponse.UserStatus = errorStatus.ErrorMsg;
                    return createUserResponse;
                }
                currentPrimaryOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);
            }

            IOrganizationRepository organizationRepository = new OrganizationRepository();
            //BlueBook MasterId for External Users
            IOrganization organizationExternalUser = organizationRepository.GetOrganization(blueBookId: DefaultUserClaim.ExternalCompanyMasterId);

            IContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
            IList<ContactMechanismUsageType> emailUsageType = contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName: "Email Notification");

            if (newProfile.organization != null && newProfile.organization.Count > 0)
            {
                //Get the Organization IDP
                organizationPartyId = newProfile.organization[0].PartyId;
                organizationRealPageId = newProfile.organization[0].RealPageId;
                identityProviderTypeList = organizationRepository.GetOrganizationIdentityProviderType(newProfile.organization[0].RealPageId);
            }

            //NOTE TO DEVELOPERS
            //Any new products are added down the line,we need to update the logic in "getProductBatchForUserClone" to get new products to clone.
            if (newProfile.ClonedUser)
            {
                cloneUserPersonaId = newProfile.Persona[0].PersonaId;
                using (var pbRepository = GetRepository())
                {
                    //TODO: FIX PRODUCTS SO WE DONT CLONE PRODUCTS THIS USER DOESN'T HAVE
                    List<PersonaProductUserDetails> userProducts = pbRepository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = newProfile.Persona[0].PersonaId, ProductStatusValue = ((Int32)ProductBatchStatusType.Success).ToString() }).ToList();
                    if (userProducts.Count > 0)
                    {
                        long createUserPersonaId = 0L;
                        ManageCloneProductBatch manageProductBatch = new ManageCloneProductBatch();
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

                        //Then Get Product Batch Data
                        IList<ProductBatch> pbData = manageProductBatch.GetUserProductBatchData(cloneUserPersonaId, userClaim, userProducts, createUserPersonaId);

                        foreach (ProductBatch pb in pbData)
                        {
                            newProfile.productBatch.Add(pb);
                        }

                        ////Then Remove Products Which are Deselected in UI
                        var profileProductBatch = newProfile.productBatch.Where(x => x.InputJson.IsAssigned == false);
                        foreach (var pb in profileProductBatch.ToList())
                        {
                            newProfile.productBatch.Remove(pb);
                        }
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

            IRoleTypeRepository roleTypeRepository = new RoleTypeRepository();
            IList<RoleType> roleTypes = roleTypeRepository.GetRoleType(roleTypeName: "User Role", partyId: null);
            var SuperUserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
            var UserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
            var UserNoEmailRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
            var rpEmployee = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("realpage employee", StringComparison.OrdinalIgnoreCase));
            var rpExternalUser = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("external user", StringComparison.OrdinalIgnoreCase));

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
                        newProfile.userLogin.ThruDate = Convert.ToDateTime("12/31/9999");
                    }

                    identityProviderType = (from a in identityProviderTypeList where a.IsLocal == (newProfile.userLogin.Is3rdPartyIDP ? false : true) select a).FirstOrDefault();
                    if (identityProviderType == null)
                    {
                        identityProviderType = identityProviderTypeList[0];
                    }

                    #endregion

                    if ((newProfile.UserTypeId != (int)UserRoleType.ExternalUser) && (userPersonaOrganizationList.ToList().Any(x => x.PartyRoleTypeId != (int)UserRoleType.ExternalUser)))
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

                        if ((newProfile.TelecommunicationNumber.Count > 0) && (newProfile.PreferredContactMethodId > 0))
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
                                PrimaryOrganization = true,
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
                                    PrimaryOrganization = true,
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

                    long AssignUserPersonaId = 0L;

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
                                currentStatusThruDate = currentPrimaryOrgStatus.StatusThruDate;
                            }
                            else
                            {
                                // the current company the user has been added to 
                                if (userPersonaOrganizationList.Count == 1 && !currentPrimaryOrgStatus.IsActive.Value)
                                {
                                    if (fromDate > DateTime.UtcNow)
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
                            if (fromDate > DateTime.UtcNow)
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
                                        userStatusId = (int)UserUiStatusType.Pending;
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

                        param = new
                        {
                            UserLoginId = userId,
                            StatusTypeId = userStatusId,
                            OrganizationPartyId = currentOrg.OrganizationPartyId,
                            PrimaryOrganization = currentOrg.PrimaryOrganization,
                            FromDate = currentOrg.OrganizationFromDate,
                            ThruDate = currentOrg.OrganizationThruDate,
                            StatusThruDate = currentStatusThruDate
                        };

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

                        long userLoginPersonaId = repositoryResponse.Id;

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

                        // Linking Persona to a Role based on user type
                        param = new
                        {
                            realPageId = currentOrg.OrganizationRealPageId
                        };
                        IList<EnterpriseRole> enterpriseRoles = repository.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_ListRolesByRealPageID, param);

                        int greenBookRole = 0;
                        if (currentOrg.OrganizationPartyId.Equals(organizationExternalUser.PartyId))
                        {
                            greenBookRole = enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase)).RoleId;
                        }
                        else
                        {
                            if (SuperUserRole.PartyRoleTypeId == newProfile.UserTypeId)
                            {
                                greenBookRole = enterpriseRoles.FirstOrDefault(r => r.Role.Equals("User Administrator", StringComparison.OrdinalIgnoreCase)).RoleId;
                            }
                            else
                            {
                                ProductBatch gbProductBatch = newProfile.productBatch?.FirstOrDefault<ProductBatch>((Func<ProductBatch, bool>)(p => p.ProductId == (int)ProductEnum.UnifiedLogin));
                                if (gbProductBatch != null)
                                {
                                    greenBookRole = GetGreenBookRole(gbProductBatch);
                                }
                                else
                                {
                                    if (newProfile.ClonedUser)
                                    {
                                        // get the users existing UnifiedLogin role
                                        param = new
                                        {
                                            PersonaID = cloneUserPersonaId,
                                            ProductID = (int)ProductEnum.UnifiedLogin
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

                        param = new
                        {
                            personaID = personaId,
                            roleID = greenBookRole,
                            personaPrivilgeID = 0
                        };
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
                        if (repositoryResponse.Id == 0)
                        {
                            repository.UnitOfWork.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.10";
                            errorStatus.ErrorMsg = "There was an error associating the persona to a user role.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        //End Create Persona

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

                    int productCount = SaveProductDetails(repository, newProfile.productBatch, createUserResponse, CreateUserPersonaId, AssignUserPersonaId, userClaim.UserRealPageGuid, organizationRealPageId, errorStatus, newProfile.UserTypeId, true, aoProductsAvailableForUser, newProfile.MigratedUser, true);

                    #endregion

                    // used to pass back user id for logging
                    newProfile.userLogin.UserId = userId;
                    newProfile.userLogin.RealPageId = personRealPageId;

                    createUserResponse.UserStatus = "User created successfully.";
                    createUserResponse.Status = errorStatus;

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
            IManageOrganization manageOrganization = new ManageOrganization();

            var userLoginOnly = userLoginRepository.GetUserLoginOnly(user.RealPageId);
            var userPersonaOrganizationList = userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);
            var currentPrimaryOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, 0, true);

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
                            ProcessDisableUserProductData(repository, persona.PersonaId, realPageEmployeeAccessID, adminPersona.PersonaId, persona.UserTypeId);
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
                        ProcessDisableUserProductData(repository, persona.PersonaId, createUserRealPageId, createUserPersonaId, persona.UserTypeId);
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

            int productCount = 0;
            long createUserPersonaId = 0;
            bool? IsDefault = null;
            IList<ProductBatch> productList = new List<ProductBatch>();
            IList<string> aoProductsAvailableForUser = null;
            Guid RealPageEmployeeAccessID = Guid.Empty;

            // if company has AO product assigned then get products available to assign based on editor User
            aoProductsAvailableForUser = GetEditorUserAoProduct(_userClaim.UserRealPageGuid, _userClaim.PersonaId, organizationRealPageId);

            IOrganizationRepository organizationRepository = new OrganizationRepository();

            IOrganization organization = organizationRepository.GetOrganization(organizationRealPageId);

            IPersonaRepository personaRepository = new PersonaRepository();

            IList<Persona> personaList = personaRepository.ListPersonaByOrganizationPartyId(organization.PartyId, IsDefault, (int)UserRoleType.SuperUser);

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
                    RealPageEmployeeAccessID = new Guid(result.PersonRealPageId);
                    //Use RealPage Employee Access PersonaId when creating the Product Patches.
                    createUserPersonaId = repository.GetOne<long>(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = RealPageEmployeeAccessID });

                    personaList.ToList().ForEach(o =>
                        productCount = SaveProductDetails(repository, productList, null, createUserPersonaId, o.PersonaId, RealPageEmployeeAccessID, organizationRealPageId, null, (int)UserRoleType.SuperUser, true, aoProductsAvailableForUser, false, false)
                    );
                    if ((personaList.Count > 0) && (productCount > 0))
                    {
                        string logMessage = $"{{0}} {{1}} performed Refresh Admin Users for {personaList.Count} ";
                        logMessage += (personaList.Count > 1) ? "administrators" : "administrator";
                        logMessage += $"; {productCount} new product user";
                        logMessage += (productCount > 1) ? "s were created" : " was created.";
                        LogActivity.WriteActivity(new ActivityDetails
                        {
                            LogActivityTypeName = LogActivityTypeConstants.PRODUCT_ACCESS,
                            LogCategoryName = LogActivityCategoryType.ProductAccess.ToString(),
                            CorrelationId = _userClaim.CorrelationId.ToString(),
                            BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
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
                                    JsonConvert.SerializeObject(product.InputJson));
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
        /// <param name="profile">Edited User detail and Products</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdateUser(Guid loggedInUserRealPageId, IProfileDetail profile)
        {
            dynamic param;
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            long createUserPersonaId = 0L;
            int greenBookRole = 0;

            string saveProductBatchError = "Save Product(s) Error: ";
            long? contactMechanismId = null;
            bool userIsActive = false;
            bool isFeatureUser = false;
            bool isCurrentOrgThePrimaryOrg = false;

            RepositoryResponse repositoryResponse = new RepositoryResponse();
            IUserRoleRightRepository userRoleRightRepository = new UserRoleRightRepository();
            IUserLoginRepository userLoginRepository = new UserLoginRepository();
            IPersonaRepository personaRespository = new PersonaRepository();
            IOrganizationRepository organizationRepository = new OrganizationRepository();
            IContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();

            IManagePersona managePersona = new ManagePersona(_userClaim);
            IList<IdentityProviderType> identityProviderTypeList = new List<IdentityProviderType>();
            IList<ProductBatch> productBatchData = new List<ProductBatch>();
            IList<ContactMechanismUsageType> emailUsageType = new List<ContactMechanismUsageType>();

            //BlueBook MasterId for External Users
            IOrganization organizationExternalUser = organizationRepository.GetOrganization(blueBookId: DefaultUserClaim.ExternalCompanyMasterId);

            IUserLoginOnly userLoginOnly = userLoginRepository.GetUserLoginOnly(profile.RealPageId);

            IList<UserOrganization> userPersonaOrganizationList = new List<UserOrganization>();
            userPersonaOrganizationList = userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);

            //Get the edited user Current Persona Id
            var persona = managePersona.GetFirstAvailablePersonaByCompany(profile.RealPageId, profile.organization[0].PartyId);
            long currentOrgPartyId = persona.OrganizationPartyId;

            bool userIsExternalEverywhere = userPersonaOrganizationList.ToList().All(x => x.PartyRoleTypeId.Equals((int)UserRoleType.ExternalUser));

            long assignUserPersonaId = persona.PersonaId;

            if ((persona != null) && (persona.Organization != null))
            {
                //Get the Organization IDP
                identityProviderTypeList = organizationRepository.GetOrganizationIdentityProviderType(persona.Organization.RealPageId);
            }

            // get the users existing UnifiedLogin role
            long existingRoleId = userRoleRightRepository.GetRoleIdByPersona(assignUserPersonaId, (int)ProductEnum.UnifiedLogin);
            if (profile.userLogin.IsActive.HasValue && profile.userLogin.IsActive == true)
            {
                userIsActive = true;
            }

            OrganizationStatus currentPrimaryOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(profile.userLogin.UserId, userLoginOnly.LastLogin, 0, true);
            OrganizationStatus currentOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(profile.userLogin.UserId, userLoginOnly.LastLogin, currentOrgPartyId, false);

            //Get the logged-in user Current Persona Id
            createUserPersonaId = personaRespository.GetActivePersonaId(realPageId: loggedInUserRealPageId);

            IList<Persona> personaList = personaRespository.ListActivePersona(profile.RealPageId, true);
            if (!currentOrgStatus.PrimaryOrganization)
            {
                personaList = personaList.Where(p => p.OrganizationPartyId == currentOrgStatus.PartyId).ToList();
            }

            IList<string> aoProductsAvailableForUser = null;
            // Get AO roles before transaction scope begins
            if (profile.UserTypeId == (int)UserRoleType.SuperUser)
            {
                // if company has AO product assigned then get products available to assign based on editor User
                aoProductsAvailableForUser = GetEditorUserAoProduct(loggedInUserRealPageId, createUserPersonaId, persona.Organization.RealPageId);
            }

            emailUsageType = contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName: "Email Notification");

            //Get User Details before save
            var userDetails = GetUserDetails(persona.PersonaId);
            //Check if ONLY user profile changed without any product changes
            bool profileChanged = IsUserProfileChanged(profile, userDetails);
            bool loginNamechanged = isUserLoginNameChanged(profile, userDetails);
            //Check if user type changed
            int batchProcessUserType = 0;
            bool userTypeChanged = false;
            string userTypeName = "";
            bool isUserTypeChangedFromNoEmailToRegular = false;
            bool isUserTypeChangedFromNoEmailToExternal = false;
            string userTypeChangedToFromExternal = string.Empty;

            if (userDetails.UserRoleTypeId != profile.UserTypeId)
            {
                #region From Regular User

                //To Regular (No Email) - NOT allowed

                // To SuperUser
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.User) && (profile.UserTypeId == (int)UserRoleType.SuperUser))
                {
                    batchProcessUserType = (int)BatchProcessType.UserTypeRegularToAdmin;
                    userTypeChanged = true;
                    userTypeName = BatchProcessType.UserTypeRegularToAdmin.ToString();
                }

                //To External
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.User) && (profile.UserTypeId == (int)UserRoleType.ExternalUser))
                {
                    userTypeChangedToFromExternal = "ToExternal";
                }

                #endregion

                #region From SuperUser

                //To Regular
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.SuperUser) && (profile.UserTypeId == (int)UserRoleType.User))
                {
                    batchProcessUserType = (int)BatchProcessType.UserTypeAdminToRegular;
                    userTypeChanged = true;
                    userTypeName = BatchProcessType.UserTypeAdminToRegular.ToString();
                }

                //To External
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.SuperUser) && (profile.UserTypeId == (int)UserRoleType.ExternalUser))
                {
                    userTypeChangedToFromExternal = "ToExternal";
                    batchProcessUserType = (int)BatchProcessType.UserTypeAdminToExternal;
                    userTypeChanged = true;
                    userTypeName = BatchProcessType.UserTypeAdminToExternal.ToString();
                }

                #endregion

                #region From Regular (No Email)

                //To Regular
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.UserNoEmail) && (profile.UserTypeId == (int)UserRoleType.User))
                {
                    isUserTypeChangedFromNoEmailToRegular = true;
                }

                //To External
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.UserNoEmail) && (profile.UserTypeId == (int)UserRoleType.ExternalUser))
                {
                    isUserTypeChangedFromNoEmailToExternal = true;
                    userTypeChangedToFromExternal = "ToExternal";
                }

                #endregion

                #region From External

                //To Regular (No Email) -- NOT allowed

                //To SuperUser (If User is External EveryWhere)
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.ExternalUser) && (profile.UserTypeId == (int)UserRoleType.SuperUser) && (userIsExternalEverywhere))
                {
                    userTypeChangedToFromExternal = "FromExternal";
                    batchProcessUserType = (int)BatchProcessType.UserTypeExternalToAdmin;
                    userTypeChanged = true;
                    userTypeName = BatchProcessType.UserTypeExternalToAdmin.ToString();
                }

                //To Regular
                if ((userDetails.UserRoleTypeId == (int)UserRoleType.ExternalUser) && (profile.UserTypeId == (int)UserRoleType.User) && (userIsExternalEverywhere))
                {
                    userTypeChangedToFromExternal = "FromExternal";
                }

                #endregion
            }

            //If user is activated from deactivate status ,get all products data which are previously assigned to user
            if (profile.userLogin.Status == UserUiStatusType.Disabled)
            {
                productBatchData = GetActivatedUserProductBatchData(persona.PersonaId, profile.productBatch);
            }
            else
            {
                productBatchData = profile.productBatch;
            }

            //Get Current user state before any update
            var primaryOrg = organizationRepository.GetOrganization(realPageId: currentPrimaryOrgStatus.RealPageId);
            var currentOrg = organizationRepository.GetOrganization(organizationPartyId: currentOrgPartyId);

            isCurrentOrgThePrimaryOrg = primaryOrg.PartyId.Equals(_userClaim.OrganizationPartyId);

            //Update profile (First, Last names, ...) required data per Organization
            IList<EditorAssignedPersona> editorAssignedPersonaList = new List<EditorAssignedPersona>();
            IList<UserOrganization> userOrganizationList = userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);
            userOrganizationList.ToList().ForEach(o =>
            {
                //editor persona
                Persona editorPersona = new Persona();
                //Is the company RealPage Employee? OR Persona Company
                if ((o.BooksCustomerMasterId.Equals(-1) && o.BooksMasterId.Equals(-1)) || (o.OrganizationPartyId.Equals(currentOrgPartyId)))
                 {
                    editorPersona = managePersona.GetFirstAvailablePersonaByCompany(loggedInUserRealPageId, o.OrganizationPartyId);
                }
                else
                {
                    Guid realPageEmployeeAccessId = _organizationRepository.GetOrganizationAdminUserRealPageId(o.OrganizationRealPageId);
                    editorPersona = managePersona.GetFirstAvailablePersonaByCompany(realPageEmployeeAccessId, o.OrganizationPartyId);
                }

                //asigned persona
                Persona assignedPersona = managePersona.GetFirstAvailablePersonaByCompany(profile.RealPageId, o.OrganizationPartyId);

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
            });

            using (var repository = GetRepository())
            {
                //Begin the transaction
                repository.UnitOfWork.BeginTransaction();
                try
                {
                    #region Update Person
                    repositoryResponse.Id = userDetails.PersonPartyId;
                    if ((profileChanged) && ((isCurrentOrgThePrimaryOrg) || (userTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))))
                    {
                        //Setup the parameter values to update the person's info
                        param = new
                        {
                            RealPageId = profile.RealPageId,
                            FirstName = profile.FirstName,
                            MiddleName = profile.MiddleName,
                            LastName = profile.LastName
                        };
                        //Update the person's info
                        repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, param);
                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = "Update User Error: Update person failed.";
                            throw new Exception(repositoryResponse.ErrorMessage);
                        }
                    }
                    #endregion

                    if (repositoryResponse.Id != 0)
                    {
                        IIdentityProviderType idpt = (from a in identityProviderTypeList where a.IsLocal == (profile.userLogin.Is3rdPartyIDP ? false : true) select a).FirstOrDefault();
                        if (idpt == null)
                        {
                            idpt = identityProviderTypeList[0];
                        }

                        if (isCurrentOrgThePrimaryOrg || userTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))
                        {
                            //Link Identity Provider (ContactMechanismId for the Identity Provider value) to new user by UserLoginId & ActivePersonaId
                            param = new
                            {
                                UserId = profile.userLogin.UserId,
                                ContactMechanismID = idpt.ContactMechanismId
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update User Error: Link Identity Provider to UserLogin failed.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }
                        }

                        #region Update UserLogin

                        if (profile.userLogin != null)
                        {
                            //check to see if user from date changed to feature date
                            isFeatureUser = profile.userLogin.FromDate.Value.Date > DateTime.Now.Date ? true : false;

                            if (profile.userLogin.ThruDate == null)
                            {
                                profile.userLogin.ThruDate = Convert.ToDateTime("12/31/9999");
                            }

                            param = new
                            {
                                RealPageId = profile.RealPageId,
                                LoginName = ((isCurrentOrgThePrimaryOrg) || (userTypeChangedToFromExternal.Equals("FromExternal", StringComparison.OrdinalIgnoreCase))) ? profile.userLogin.LoginName : userLoginOnly.LoginName,
                                FromDate = profile.userLogin.FromDate.Value,
                                ThruDate = profile.userLogin.ThruDate,
                                PartyId = persona.OrganizationPartyId
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, param);
                            if (repositoryResponse.Id == 0)
                            {
                                repositoryResponse.ErrorMessage = "Update User Error: Update user login detail failed.";
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }

                            //UserType Changed To External OR From External
                            string changeUserTypeExternal = ChangeUserTypeExternal(repository, organizationExternalUser, currentPrimaryOrgStatus, profile, persona, userPersonaOrganizationList, emailUsageType, userLoginOnly, idpt, userTypeChangedToFromExternal);
                            if (changeUserTypeExternal != string.Empty)
                            {
                                repositoryResponse.ErrorMessage = changeUserTypeExternal;
                                throw new Exception(repositoryResponse.ErrorMessage);
                            }

                            //update User custom fields
                            if (profile.CustomFields?.Count > 0)
                            {
                                if (profile.CustomFields.ToList().Any(c => c.UserLoginPersonaId.Equals(0)))
                                {
                                    param = new
                                    {
                                        UserLoginId = profile.userLogin.UserId,
                                        OrganizationPartyId = currentOrgPartyId
                                    };
                                    IList<UserLoginPersona> userLoginPersonaList = repository.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona, param);

                                    profile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId);
                                }

                                string customFieldsValuesJson = JsonConvert.SerializeObject(profile.CustomFields);
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

                            if (profile.userLogin.Is3rdPartyIDP != userLoginOnly.Is3rdPartyIDP)
                            {
                                isUserAccessLevelChanged = true;
                            }

                            if (profile.userLogin.FromDate.Value > DateTime.UtcNow)
                            {
                                isUserEffectiveDateChanged = true;
                            }

                            //Check to see if there is any status change or 3rdpartyidp changed or user effective date changed to future date on 
                            //user update then process for new status
                            if ((profile.userLogin.IsActive != currentOrgStatus.IsActive) || isUserAccessLevelChanged || isUserEffectiveDateChanged)
                            {
                                DateTime? statusThruDate = null;
                                UserUiStatusType statusTypeId = UserUiStatusType.UnDefined;

                                //current state user login is disabled and user activated through update process and
                                //user never logedin before then set user status to pending
                                if (currentOrgStatus.StatusTypeId == (int)UserUiStatusType.Disabled && profile.userLogin.IsActive == true && userLoginOnly.LastLogin == null && idpt.IsLocal)
                                {
                                    statusTypeId = UserUiStatusType.Pending;
                                }
                                else
                                {
                                    statusThruDate = null;
                                    statusTypeId = UserUiStatusType.Active;
                                }

                                if (profile.userLogin.FromDate.Value <= DateTime.UtcNow &&
                                    profile.userLogin.ThruDate.HasValue && profile.userLogin.ThruDate.Value.ToUniversalTime() < DateTime.UtcNow)
                                {
                                    statusTypeId = UserUiStatusType.Disabled;
                                    statusThruDate = null;
                                }

                                if (profile.userLogin.IsActive == false && currentOrgStatus.IsActive == true)
                                {
                                    statusTypeId = UserUiStatusType.Disabled;
                                    statusThruDate = null;
                                }

                                //Disabled or Pending state and Effective from date changed to today date,then user need to be in pending state to complete profile
                                // or if the user was 3rd party idp and never logged in and now the user is changing to local login instead, create a new pending activity
                                if (idpt.IsLocal && ((profile.userLogin.Status == UserUiStatusType.Disabled) || (profile.userLogin.Status == UserUiStatusType.Pending) || (userLoginOnly.Is3rdPartyIDP == true && userLoginOnly.LastLogin == null)))
                                {
                                    if (userLoginOnly != null && currentOrgStatus.FromDate > DateTime.UtcNow && profile.userLogin.FromDate.Value <= DateTime.UtcNow)
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
                                    statusThruDate = profile.userLogin.FromDate.Value.Date.AddHours(72); //default
                                    statusThruDate = newUserRegistrationActivity != null ? profile.userLogin.FromDate.Value.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : statusThruDate;
                                }

                                if (statusTypeId != UserUiStatusType.UnDefined)
                                {
                                    param = new
                                    {
                                        RealPageId = profile.RealPageId,
                                        OrganizationPartyId = persona.OrganizationPartyId,
                                        StatusTypeId = statusTypeId,
                                        FromDate = profile.userLogin.FromDate,
                                        StatusThruDate = statusThruDate
                                    };
                                    repositoryResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany, param);
                                    if (repositoryResponse.Id == 0)
                                    {
                                        repositoryResponse.ErrorMessage = "Update User Error: Update user status failed.";
                                        throw new Exception(repositoryResponse.ErrorMessage);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Update Persona

                        //if user type changes then update persona type
                        if (userTypeChanged)
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

                            param = new
                            {
                                PersonaId = assignUserPersonaId,
                                PersonaTypeId = personaTypeId,
                                PersonaEnvironmentTypeId = persona.PersonaEnvironmentTypeId
                            };
                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersona, param);
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
                            RealPageIdFrom = profile.RealPageId,
                            RealPageIdTo = persona.Organization.RealPageId,
                            RoleTypeName = roleTypeName,
                            RelationshipTypeName = relationshipTypeName
                        };
                        PartyRelationship relationshipType = repository.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, paramRelType);

                        //Update the User Type  
                        if ((relationshipType != null) && (relationshipType.RoleTypeIdFrom != profile.UserTypeId))
                        {
                            param = new
                            {
                                PersonRealPageId = profile.RealPageId,
                                OrganizationRealPageId = persona.Organization.RealPageId,
                                UnlinkRoleTypeIdFrom = relationshipType.RoleTypeIdFrom,
                                LinkRoleTypeIdFrom = profile.UserTypeId,
                                RoleTypeIdTo = relationshipType.RoleTypeIdTo
                            };

                            repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePersonToOrganization, param);
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

                        IList<CommonAddress> result = repository.GetMany<CommonAddress>(StoredProcNameConstants.SP_ListContactMechanismsForPerson, new { profile.RealPageId }).ToList();
                        if (result != null)
                        {
                            foreach (var contactMechanismFind in result)
                            {
                                var usageType = emailUsageType.FirstOrDefault(i => i.ContactMechanismUsageTypeId == contactMechanismFind.ContactMechanismUsageTypeId);
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

                        //update email contact mechanisim if user login name changed
                        if (userContactMechanismId != 0)
                        {
                            // the user already had the contact mechanism so update id
                            if ((profile.UserTypeId != (int)UserRoleType.UserNoEmail) && loginNamechanged)
                            {
                                param = new
                                {
                                    ContactMechanismId = userContactMechanismId,
                                    ElectronicAddressString = profile.userLogin.LoginName
                                };

                                repositoryResponse = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, param);
                                if (repositoryResponse.Id == 0)
                                {
                                    repositoryResponse.ErrorMessage = "An error was encountered when updating an user login email address.";
                                    throw new Exception(repositoryResponse.ErrorMessage);
                                }
                            }
                        }
                        else if (profile.UserTypeId != (int)UserRoleType.UserNoEmail)
                        {
                            //add the missing email
                            profile.NotificationEmail = profile.userLogin.LoginName;
                        }

                        // end the existing notification email if one exists
                        if (!string.IsNullOrEmpty(priorNotificationEmail) && string.IsNullOrEmpty(profile.NotificationEmail))
                        {
                            // the user had a notification email but no longer, so it needs to be ended
                            endExistingNotificationEmail = true;
                        }

                        if (!string.IsNullOrEmpty(priorNotificationEmail) && !string.IsNullOrEmpty(profile.NotificationEmail) && (priorNotificationEmail.ToLower() != profile.NotificationEmail.ToLower()))
                        {
                            // the email has changed so end the existing record before creating a new one
                            endExistingNotificationEmail = true;
                        }

                        if (endExistingNotificationEmail)
                        {
                            param = new
                            {
                                RealPageId = profile.RealPageId,
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
                        if (profile.NotificationEmail != null && (isFeatureUser || (priorNotificationEmail.ToLower() != profile.NotificationEmail.ToLower())) && !string.IsNullOrEmpty(profile.NotificationEmail))
                        {
                            if (EmailFormatValidation.IsValidEmail(profile.NotificationEmail))
                            {
                                partyContactMechanismId = 0;

                                var EmailContactMechanism = emailUsageType.SingleOrDefault<ContactMechanismUsageType>(p => p.Name == "Email");

                                //Build the parameters for email
                                LinkElectronicAddress electronicAddress = new LinkElectronicAddress();
                                electronicAddress.PartyContactMechanism.FromDate = utcNow;
                                electronicAddress.PartyContactMechanism.ThruDate = utcMaxValue;
                                electronicAddress.ElectronicAddress.AddressString = profile.NotificationEmail;
                                electronicAddress.ElectronicAddress.AddressType = EmailContactMechanism.Name;
                                electronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId = (int)EmailContactMechanism.ContactMechanismUsageTypeId;
                                addressType = EmailContactMechanism.Name;

                                contactMechanismId = null;
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
                                    profile.RealPageId,
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
                                    ElectronicAddressString = profile.NotificationEmail,
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
                                    IList<CommonAddress> contactMechanismList = ListContactMechanismForPerson(repository, currentOrg.RealPageId, emailUsageType);
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

                        if (userIsActive && !userTypeChanged)
                        {
                            int productCount = SaveProductDetails(repository, productBatchData, null, createUserPersonaId, assignUserPersonaId, loggedInUserRealPageId, persona.Organization.RealPageId, null, profile.UserTypeId, userIsActive, aoProductsAvailableForUser);

                            if (userDetails.UserRoleTypeId != profile.UserTypeId)
                            {
                                var bpType = string.Empty;
                                if (profile.UserTypeId == (int)UserRoleType.User && userDetails.UserRoleTypeId == (int)UserRoleType.ExternalUser)
                                {
                                    bpType = "External User to Regular User";
                                }

                                if (profile.UserTypeId == (int)UserRoleType.ExternalUser && userDetails.UserRoleTypeId == (int)UserRoleType.User)
                                {
                                    bpType = "Regular User to External User";
                                }

                                if (isUserTypeChangedFromNoEmailToExternal)
                                {
                                    bpType = "Regular User (No Email) to External User";
                                }

                                var auditMessage = $"The User Type of {{0}} {{1}} was changed from {bpType} by {{2}} {{3}}";

                                LogAuditActivity(LogActivityTypeConstants.PRODUCT_ACCESS, LogActivityCategoryType.ProductAccess, auditMessage, userTypeName, profile);
                            }
                        }

                        if (!userIsActive)
                        {
                            DisableAllCompanyProducts(loggedInUserRealPageId, profile, currentOrg, repository, assignUserPersonaId, createUserPersonaId, personaList);
                        }

                        if ((profile.userLogin.Status != UserUiStatusType.Disabled) && (profileChanged || loginNamechanged || (!priorNotificationEmail.Equals(profile.NotificationEmail, StringComparison.OrdinalIgnoreCase))))
                        {
                            editorAssignedPersonaList.ToList().ForEach(p =>
                            {
                                SaveUserProductBatchData(repository, null, p.EditorPersonaId, p.AssignedPersonaId, p.EditorPersonaRealPageId, p.OrganizationRealPageId, null, (Int32)BatchProcessType.ProfileUpdate, productBatchData, null, p.AssignedUserTypeId);
                            });
                        }

                        if (userIsActive && userTypeChanged)
                        {
                            SaveUserProductBatchData(repository, null, createUserPersonaId, assignUserPersonaId, loggedInUserRealPageId, persona.Organization.RealPageId, null, batchProcessUserType, productBatchData, aoProductsAvailableForUser, profile.UserTypeId);

                            var bpType = string.Empty; //batchProcessUserType == 5 ? "Regular User to RealPage System Administrator" : "RealPage System Administrator to Regular User";
                            switch (batchProcessUserType)
                            {
                                case 5:
                                    bpType = "Regular User to RealPage System Administrator";
                                    break;
                                case 6:
                                    bpType = "RealPage System Administrator to Regular User";
                                    break;
                                case 8:
                                    bpType = "External User to RealPage System Administrator";
                                    break;
                                case 9:
                                    bpType = "RealPage System Administrator to External User";
                                    break;
                                default:
                                    break;
                            }

                            var auditMessage = $"The User Type of {{0}} {{1}} was changed from {bpType} by {{2}} {{3}}";
                            if (bpType != string.Empty)
                            {
                                LogAuditActivity(LogActivityTypeConstants.PRODUCT_ACCESS, LogActivityCategoryType.ProductAccess, auditMessage, userTypeName, profile);
                            }
                        }

                        // GreenBook - UnifiedLogin call updating GB Role
                        greenBookRole = 0;
                        var gbProdBatch = profile.productBatch.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedLogin);
                        if (gbProdBatch != null)
                        {
                            greenBookRole = GetGreenBookRole(gbProdBatch);
                        }
                        else
                        {
                            //This will exceuted when user type changes and no productbatch record for greenbook role is coming from UI
                            if (userTypeChanged)
                            {
                                IList<RoleType> roleTypes;
                                dynamic paramUserRole = new
                                {
                                    RoleTypeName = "User Role"
                                };
                                roleTypes = repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, paramUserRole);
                                var SuperUserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name == "SuperUser");

                                var enterpriseRoles = repository.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_ListRolesByRealPageID, new { realPageId = persona.Organization.RealPageId });

                                if (SuperUserRole.PartyRoleTypeId == profile.UserTypeId)
                                {
                                    greenBookRole = enterpriseRoles.FirstOrDefault(ur => ur.Role == "User Administrator").RoleId;
                                }
                                else
                                {
                                    var paramDefaultRole = new
                                    {
                                        RealPageID = persona.Organization.RealPageId
                                    };
                                    var defaultRole = repository.GetOne<dynamic>(StoredProcNameConstants.SP_GetUnifiedLoginDefaultRole, paramDefaultRole);

                                    greenBookRole = defaultRole != null ? defaultRole.RoleId : enterpriseRoles.FirstOrDefault(rl => rl.Role == "Basic End User").RoleId;
                                }
                            }
                        }

                        UpdateGreenBookRole(repository, greenBookRole, assignUserPersonaId, loggedInUserRealPageId, persona.Organization.RealPageId, profile.UserTypeId, existingRoleId);

                        if (saveProductBatchError != "Save Product(s) Error: ")
                        {
                            repositoryResponse.Id = 0;
                            repositoryResponse.ErrorMessage = saveProductBatchError;
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
                        repositoryResponse.RealPageId = profile.RealPageId;

                        //Commit and end transaction.
                        repository.UnitOfWork.Commit();
                    }
                    else
                    {
                        //Rollback transaction and dispose it.
                        repositoryResponse.Id = 0;
                        repository.UnitOfWork.Rollback();
                    }
                }

                // Activity logging
                if (repositoryResponse.Id > 0)
                {
                    if (isUserTypeChangedFromNoEmailToRegular)
                    {
                        //Log Activity
                        LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, "{0} {1} user type changed from regular (No Email) to regular user by {2} {3}.", "UpdateUser", profile);
                    }
                    else
                    {
                        //Log Activity
                        LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, "User {0} {1} successfully updated by user {2} {3}.", "UpdateUser", profile);
                    }
                }

                return repositoryResponse;
            }
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
        /// Used to disable products for the given list of users which is called from windows service
        /// </summary>		
        /// <param name="userLogins"></param>
        public void ProcessDisabledUsers(IList<ProcessUserLogin> userLogins)
        {
            //IManageUserLogin userLoginLogic = new ManageUserLogin();
            IManagePersona managePersona = new ManagePersona();
            Dictionary<Guid, Persona> companyAdminList = new Dictionary<Guid, Persona>();

            using (var repository = GetRepository())
            {
                foreach (ProcessUserLogin ul in userLogins)
                {
                    var userLoginRepository = new UserLoginRepository();
                    IUserLoginOnly userLoginOnly = userLoginRepository.GetUserLoginOnly(ul.UserRealPageId);
                    var userOrganizationList = userLoginRepository.ListOrganizationByLoginName(userLoginOnly.LoginName);
                    Guid primaryCompanyGuid = userOrganizationList.FirstOrDefault(p => p.PrimaryOrganization).OrganizationRealPageId;
                    List<Guid> organizationsToProcess = new List<Guid>();

                    foreach (var org in userOrganizationList)
                    {
                        Persona editorPersona = null;
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
                                    Guid realPageEmployeeAccessId = new Guid(item.PersonRealPageId);
                                    long orgPartyId = item.PartyId;
                                    editorPersona = managePersona.GetFirstAvailablePersonaByCompany(realPageEmployeeAccessId, orgPartyId);
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

                        //update user status to disabled
                        if (org.PrimaryOrganization)
                        {
                            var updateUserStatusResponse = repository.Execute<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserStatusByCompany, new
                            {
                                RealPageId = ul.UserRealPageId,
                                OrganizationPartyId = org.OrganizationPartyId,
                                StatusTypeId = UserUiStatusType.Disabled,
                                FromDate = ul.FromDate
                            });
                        }
                        //remove products
                        if (editorPersona != null && (ul.OrganizationRealPageId == primaryCompanyGuid || ul.OrganizationRealPageId == org.OrganizationRealPageId))
                        {
                            ProcessDisableUserProductData(repository, persona.PersonaId, editorPersona.RealPageId, editorPersona.PersonaId, persona.UserTypeId);
                        }
                    }
                }
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
                                    PhoneNumber = telecommunicationNumber.PhoneNumber
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
                                    telecommunicationNumber.PhoneNumber
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
        /// Used to get list of valid products assigned to organization 
        /// </summary>
        /// <param name="repository">Dapper Repository</param>		
        /// <param name="realPageId">enterprise User Id</param>
        /// <param name="organizationRealPageId">enterprise Organization Id</param>		
        /// <param name="aoProducts">Applicable if PMC has AO products</param>
        private List<ProductUI> GetOrganizationProductList(IRepository repository, Guid realPageId, Guid organizationRealPageId, IList<string> aoProducts = null)
        {
            List<int> productIDUserCreateList;
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getListProductsByOrganization_{organizationRealPageId}";

            List<ProductUI> productsAssignedToCompany = rpCache.GetFromCache<List<ProductUI>>(cacheKey, 180, () =>
            {
                return repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    new { OrganizationRealPageId = organizationRealPageId }).ToList();
            });

            // Add AO products if any
            if (aoProducts != null)
            {
                foreach (var aoProduct in aoProducts)
                {
                    productsAssignedToCompany.Add(new PersonaProductUserDetails { ProductId = (int)ProductEnumHelper.GetAoProductEnum(aoProduct) });
                }
            }

            IList<ProductBatch> productListToCreate = new List<ProductBatch>();

            //Get the organization type
            dynamic param = null;
            cacheKey = $"getListOrganizationType";
            IList<OrganizationType> organizationTypeList = rpCache.GetFromCache<IList<OrganizationType>>(cacheKey, 180, () => { return repository.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, param); });

            cacheKey = $"getGetOrganization_{organizationRealPageId}";
            Organization organization = rpCache.GetFromCache<Organization>(cacheKey, 180, () =>
            {
                param = new
                {
                    RealPageId = organizationRealPageId
                };
                return repository.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, param);
            });

            if (organization != null)
            {
                organization.organizationType = organizationTypeList.ToList().FirstOrDefault(o => o.OrganizationTypeId == organization.OrganizationTypeId);
            }

            if (organization.organizationType.Name.Equals("Vendor", StringComparison.OrdinalIgnoreCase))
            {
                //Company type is Vendor
                productIDUserCreateList = new List<int>()
                {
                    (int) ProductEnum.VendorMarketplace
                };
            }
            else
            {
                // the list of products that are currently available to add users for (Company type is NOT Vendor)
                productIDUserCreateList = new List<int>()
                {
                    (int) ProductEnum.OneSite,
                    (int) ProductEnum.FinancialSuite,
                    (int) ProductEnum.MarketingCenter,
                    (int) ProductEnum.OpsBuyer,
                    (int) ProductEnum.ClientPortal,
                    (int) ProductEnum.VendorServices,
                    (int) ProductEnum.Lead2Lease,
                    (int) ProductEnum.ResidentPortal,
                    (int) ProductEnum.ProspectContactCenter,
                    (int) ProductEnum.Insurance,
                    (int) ProductEnum.OnSite,
                    (int) ProductEnum.UtilityManagement,
                    (int) ProductEnum.SelfProvisioningPortal,
                    (int) ProductEnum.UnifiedAmenities,
                    (int) ProductEnum.ResearchApplication,
                    (int) ProductEnum.RPDocumentManagement,
                    (int) ProductEnum.LeadManagement,
                    (int) ProductEnum.LeadAnalytics,
                    (int) ProductEnum.PortfolioManagement,
                    (int) ProductEnum.IntegrationMarketplace,
                    (int) ProductEnum.DepositAlternative,
                    (int) ProductEnum.ClickPay
                };
            }

            // Add AO products Supported by GB
            productIDUserCreateList.AddRange(ProductEnumHelper.GetAoProductList().Cast<int>().ToList());
            foreach (ProductUI prod in productsAssignedToCompany.ToList())
            {
                if (!productIDUserCreateList.Contains(prod.ProductId))
                {
                    productsAssignedToCompany.Remove(prod);
                }
            }

            return productsAssignedToCompany;
        }

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
        /// <param name="aoProducts">Applicable if PMC has AO products</param>
        /// <returns>Number of Products</returns>
        private int SaveProductDetails(IRepository repository, IList<ProductBatch> productList, CreateUserResponse<IErrorData> createUserResponse, long CreateUserPersonaId, long AssignUserPersonaId, Guid realPageId, Guid organizationRealPageId, Status<IErrorData> errorStatus, int userTypeId, bool userIsActive, IList<string> aoProducts = null, bool migratedUser = false, bool isCreateUser = false)
        {
            int productCount = 0;
            string saveProductBatchError = "Save Product(s) Error: ";

            if (errorStatus == null)
            {
                errorStatus = new Status<IErrorData>();
            }

            // if superuser do all products that currently support user creation;
            if (userIsActive && userTypeId == (int)UserRoleType.SuperUser && !migratedUser)
            {
                IList<ProductBatch> productListToCreate = new List<ProductBatch>();
                IList<PersonaProductUserDetails> userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = AssignUserPersonaId }).ToList();
                List<ProductUI> productsAssignedToCompany = GetOrganizationProductList(repository, realPageId, organizationRealPageId, aoProducts);

                foreach (ProductUI prod in productsAssignedToCompany)
                {
                    // see if the user already has the product, or if they do if it is Deleted or Deactivated, and if so add it or turn it back on
                    if (userProducts == null
                        || (!userProducts.Any(a => a.ProductId == prod.ProductId))
                        || (userProducts.Any(a => a.ProductId == prod.ProductId && a.ProductStatus == (int)ProductBatchStatusType.Deleted))
                        || (userProducts.Any(a => a.ProductId == prod.ProductId && a.ProductStatus == (int)ProductBatchStatusType.Deactivated))
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
                                InputJson = new RolePropertyList() { PropertyRoleList = new List<PropertyRoleList>(), PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true }
                            };
                            productListToCreate.Add(pb);
                        }
                    }
                }

                productList = productListToCreate;
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
                            InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = (isCreateUser || userIsActive) }
                        };
                        productList.Add(pb);
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
                        InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
                    };
                    productList.Add(pb);
                }
            }

            //Save selected products
            if ((productList != null) && (productList.Count > 0))
            {
                //Do we have the Create & Assign PersonaIds
                if ((CreateUserPersonaId > 0) && (AssignUserPersonaId > 0))
                {
                    // if the user isn't a superuser, check to see if both Lead2Lease and OneSite are in the products to be saved. If they are, then they need to be combined into a single product call
                    if (!(userTypeId == (int)UserRoleType.SuperUser) && productList.Any(a => a.ProductId == (int)ProductEnum.OneSite) && productList.Any(a => a.ProductId == (int)ProductEnum.Lead2Lease))
                    {
                        // need to combine the Lead2Lease and OneSite product details so they can run synchronously
                        SO.Product.Lead2Lease.Lead2LeaseOneSiteProduct l2lOneSite = new SO.Product.Lead2Lease.Lead2LeaseOneSiteProduct();

                        ProductBatch pbLead2Lease = (from a in productList
                                                     where a.ProductId == (int)ProductEnum.Lead2Lease
                                                     select a).FirstOrDefault();
                        l2lOneSite.Lead2Lease = pbLead2Lease.InputJson;

                        ProductBatch pbOneSite = (from a in productList
                                                  where a.ProductId == (int)ProductEnum.OneSite
                                                  select a).FirstOrDefault();
                        l2lOneSite.OneSite = pbOneSite.InputJson;

                        SaveProductBatch(repository, pbLead2Lease, createUserResponse, saveProductBatchError, CreateUserPersonaId, AssignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(l2lOneSite));

                        if (errorStatus.Success == false)
                        {
                            errorStatus.ErrorMsg = saveProductBatchError;
                        }
                        else
                        {
                            // remove OneSite and L2L from the product batch
                            productList.Remove(pbLead2Lease);
                            productList.Remove(pbOneSite);
                        }
                    }

                    // Handle Ao Products if any
                    var aoInputJsonString = BundleAoProducts(productList);

                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productList)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedLogin)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products
                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, CreateUserPersonaId, AssignUserPersonaId, realPageId, errorStatus, aoInputJsonString);
                        }
                        else
                        {
                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, CreateUserPersonaId, AssignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson));
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

            IList<PersonaProductUserDetails> userProducts = repository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = assignUserPersonaId, ProductStatusValue = ((Int32)UserUiStatusType.AccountCreationSuccessful).ToString() }).ToList();
            IList<ProductBatch> productListToCreate = new List<ProductBatch>();
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
                        if (!ProductEnumHelper.GetAoProductList().Contains((ProductEnum)prod.ProductId) &&
                            (ProductEnum)prod.ProductId != ProductEnum.AssetOptimizer)
                        {
                            // remove products which are completely unassigned
                            if (productBatchData.All(p => p.ProductId != prod.ProductId))
                            {
                                ProductBatch pb = new ProductBatch()
                                {
                                    ProductId = prod.ProductId,
                                    StatusTypeId = 5,
                                    RetryCount = 0,
                                    InputJson = new RolePropertyList()
                                    {
                                        PropertyList = new List<string>(),
                                        RoleList = new List<string>(),
                                        IsAssigned = false
                                    }
                                };

                                productListToRemove.Add(pb);
                            }
                            else if (productBatchData.Any(p => p.ProductId == prod.ProductId))
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
                                        InputJson = new RolePropertyList()
                                        {
                                            PropertyList = batchRecord.InputJson.PropertyList,
                                            RoleList = batchRecord.InputJson.RoleList,
                                            PropertyRoleList = batchRecord.InputJson.PropertyRoleList,
                                            PropertyGroup = batchRecord.InputJson.PropertyGroup,
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
                                            OrganizationRoleList = batchRecord.InputJson.OrganizationRoleList
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
                            InputJson = null
                        };

                        StringBuilder sb = new StringBuilder();
                        dynamic expandoList = new ExpandoObject();
                        expandoList.IsAssigned = true;
                        expandoList.AoUserCompanyPropertyRoleDetailList = new List<ExpandoObject>();

                        // Collect ALL Json(s) for AO products based on assigned or un-assigned
                        foreach (var aoProduct in aoUserProductList)
                        {
                            dynamic expandoAo = new ExpandoObject();

                            if (productBatchData.All(p => p.ProductId != aoProduct.ProductId))
                            {
                                // user has removed specific product
                                expandoAo.SelectedRoleValues = null;
                                expandoAo.SelectedPortfolioValues = null;
                                expandoAo.CompanyId = 0;

                                expandoAo.Product = ProductEnumHelper.GetAoProductId((ProductEnum)aoProduct.ProductId);
                                expandoAo.DivisionName =
                                    ProductEnumHelper.GetAoDivisionName((ProductEnum)aoProduct.ProductId);
                                expandoAo.PropertyGroups = null;

                                expandoAo.IsAssigned = false;
                            }
                            else
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

                            // add in collection
                            expandoList.AoUserCompanyPropertyRoleDetailList.Add(expandoAo);
                        }

                        // add record to remove AO products
                        sb.Append(JsonConvert.SerializeObject(expandoList));

                        // save AO specific records in batch
                        SaveProductBatch(repository, aoProductsBatch, createUserResponse,
                            saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus,
                            sb.ToString(), (int)BatchProcessType.CreateUpdateProductUser);
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
                                InputJson = new RolePropertyList()
                                { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = false }
                            };
                            productListToRemove.Add(pbs);
                        }
                    }

                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productListToRemove)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedLogin)
                        {
                            continue;
                        }

                        SaveProductBatch(repository, product, createUserResponse, saveProductBatchError,
                            createUserPersonaId, assignUserPersonaId, realPageId, errorStatus,
                            JsonConvert.SerializeObject(product.InputJson),
                            (int)BatchProcessType.CreateUpdateProductUser);
                    }

                    if (errorStatus.Success == false)
                    {
                        errorStatus.ErrorMsg = saveProductBatchError;
                    }
                }
                else //UserTypeRegularToAdmin || UserTypeExternalToAdmin
                {
                    // Get products assigned to company including AO products
                    List<ProductUI> productsAssignedToCompany = GetOrganizationProductList(repository, realPageId, organizationRealPageId, aoProductsAvailableForUser);

                    //Regular to Admin
                    foreach (ProductUI prod in productsAssignedToCompany)
                    {
                        ProductBatch pb = new ProductBatch()
                        {
                            ProductId = prod.ProductId,
                            StatusTypeId = 5,
                            RetryCount = 0,
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

                    // AO product handling - removes AO products from list & returns JSON string
                    aoInputJsonString = BundleAoProducts(productListToCreate);
                }
            }
            else if (batchProcessTypeId == (int)BatchProcessType.ProfileUpdate)
            {
                if (userProducts?.Count > 0)
                {
                    foreach (var product in userProducts)
                    {
                        ProductBatch pb = new ProductBatch()
                        {
                            ProductId = product.ProductId,
                            StatusTypeId = 5,
                            RetryCount = 0,
                            InputJson = new RolePropertyList() { PropertyList = new List<string>(), RoleList = new List<string>(), IsAssigned = true }
                        };
                        productListToCreate.Add(pb);
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
                if ((createUserPersonaId > 0) && (assignUserPersonaId > 0))
                {
                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productListToCreate)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedLogin)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products
                            SaveProductBatch(repository, product, createUserResponse,
                                saveProductBatchError, createUserPersonaId, assignUserPersonaId,
                                realPageId, errorStatus, aoInputJsonString, batchProcessTypeId);
                        }
                        else
                        {
                            if (userProducts.Any(pr => pr.ProductId == product.ProductId))
                            {
                                SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson), batchProcessTypeId);
                            }
                            else
                            {
                                SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, realPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson));
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

        private string BundleAoProducts(IList<ProductBatch> productList)
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
        /// <param name="batchProcessTypeId">Batch Process Type</param>
        private void SaveProductBatch(IRepository repository, IProductBatch product, CreateUserResponse<IErrorData> createUserResponse, string saveProductBatchError, long CreateUserPersonaId, long AssignUserPersonaId, Guid realPageId, Status<IErrorData> errorStatus, string inputJson, int batchProcessTypeId = 1)
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
                    StatusTypeId = product.StatusTypeId,
                    RetryCount = product.RetryCount,
                    InputJson = inputJson,
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BatchProcessTypeId = batchProcessTypeId
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
        /// Used to Update GB(Unified Login) product Role information for a user
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="newRoleId"></param>
        /// <param name="assignUserPersonaId"></param>        
        /// <param name="loggedInUserRealPageId"></param>
        /// <param name="realPageId"></param>
        /// <param name="userTypeId"></param>
        /// <param name="existingRoleId"></param>
        private void UpdateGreenBookRole(IRepository repository, int newRoleId, long assignUserPersonaId, Guid loggedInUserRealPageId, Guid realPageId, int userTypeId, long existingRoleId)
        {
            if (newRoleId > 0)
            {
                IUserRoleRightRepository userRoleRightRepository = new UserRoleRightRepository(repository);
                if (existingRoleId == 0 || newRoleId != existingRoleId)
                {
                    if (existingRoleId != 0)
                    {
                        // Delete existing roleId
                        userRoleRightRepository.InsertAssignedRoleToUser(assignUserPersonaId, existingRoleId, true);
                    }

                    //Insert new roleId to GB
                    userRoleRightRepository.InsertAssignedRoleToUser(assignUserPersonaId, newRoleId);
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
        public void ProcessDisableUserProductData(IRepository repository, long assignUserPersonaId, Guid createUserRealPageId, long createUserPersonaId, int? userTypeId)
        {
            CreateUserResponse<IErrorData> createUserResponse = new CreateUserResponse<IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IList<ProductSettingType> productSettingTypes = new List<ProductSettingType>();
            RPObjectCache rpcache = new RPObjectCache();
            string saveProductBatchError = "Save Product(s) Error: ";
            string _productStatus = "ProductStatus";
            //Save latest previous product batch to process again when user is re activated.

            IList<ProductBatch> productListToDisable = GetListOfProductsToRemoveByPersonaId(repository, assignUserPersonaId);

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

                    productSettingTypes = rpcache.GetFromCache<List<ProductSettingType>>(cacheKey, 600, () =>
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
                        aoInputJsonString = BundleAoProducts(productListToDisable);
                    }

                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productListToDisable)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedLogin)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products by passing json string
                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, createUserRealPageId, errorStatus, aoInputJsonString);
                        }
                        else
                        {
                            SaveProductBatch(repository, product, createUserResponse, saveProductBatchError, createUserPersonaId, assignUserPersonaId, createUserRealPageId, errorStatus, JsonConvert.SerializeObject(product.InputJson));
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
        /// <param name="userDetails"></param>
        /// <returns></returns>
        private bool IsUserProfileChanged(IProfileDetail profile, UserDetails userDetails)
        {
            bool isChanged = (
                (!profile.FirstName.Equals(userDetails.FirstName))
                ||
                (!string.IsNullOrEmpty(profile.MiddleName) && !profile.MiddleName.Equals(userDetails.MiddleName))
                ||
                (!profile.LastName.Equals(userDetails.LastName))
            );
            return isChanged;
        }

        /// <summary>
        /// isUserLoginNameChanged
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="userDetails"></param>
        /// <returns></returns>
        private bool isUserLoginNameChanged(IProfileDetail profile, UserDetails userDetails)
        {
            bool isChanged = !profile.userLogin.LoginName.Equals(userDetails.LoginName);

            return isChanged;
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
        private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType, string message, string stepName, IProfileDetail profile)
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName = logActivityType,
                LogCategoryName = logActivityCategoryType.ToString(),
                CorrelationId = _userClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                Message = string.Format(message, profile.FirstName, profile.LastName, _userClaim.FirstName, _userClaim.LastName, profile.CreateUserSourceType.ToString()),

                FromUserLoginName = _userClaim.LoginName,
                FromUserLoginId = _userClaim.UserId,
                FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                FromUserFirstName = _userClaim.FirstName,
                FromUserLastName = _userClaim.LastName,

                ToUserLoginName = profile.userLogin.LoginName,
                ToUserLoginId = profile.userLogin.UserId,
                ToUserFirstName = profile.FirstName,
                ToUserLastName = profile.LastName,
                ToUserRealpageId = profile.userLogin.RealPageId.ToString()
            });
        }

        private string ChangeUserTypeExternal(IRepository repository, IOrganization organizationExternalUser, OrganizationStatus currentPrimaryOrgStatus, IProfileDetail profile, IPersona persona, IList<UserOrganization> userPersonaOrganizationList, IList<ContactMechanismUsageType> emailUsageType, IUserLoginOnly userLoginOnly, IIdentityProviderType identityProviderType, string userTypeChangedToFromExternal)
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
                IList<EnterpriseRole> enterpriseRoles = repository.GetMany<EnterpriseRole>(StoredProcNameConstants.SP_ListRolesByRealPageID, param);

                greenBookRole = enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase)).RoleId;
                param = new
                {
                    personaID = personaId,
                    roleID = greenBookRole,
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

                ProcessDisableUserProductData(repository, companyPersona.PersonaId, editorRealPageId, editorPersonaId, profile.UserTypeId);
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

            //Any new products are added down the line,we need to update the logic in "getProductBatchForUserClone" to get new products to clone.
            using (var pbRepository = GetRepository())
            {
                List<PersonaProductUserDetails> userProducts = pbRepository.GetMany<PersonaProductUserDetails>(StoredProcNameConstants.SP_ListProductsByPersonaId, new { PersonaId = personaId, ProductStatusValue = ((Int32)UserUiStatusType.Deactivated).ToString() }).ToList();
                if (userProducts.Count > 0)
                {
                    //Then Get Product Batch Data
                    productBatch = GetActivatedUserProductBatchData(personaId, productBatch);

                    // If any AO product
                    string aoInputJsonString = string.Empty;
                    var aoProductList = userProducts.Where(y => ProductEnumHelper.GetAoProductList().Contains((ProductEnum)y.ProductId)).ToList();
                    if (aoProductList.Any())
                    {
                        // Bundle AO products
                        aoInputJsonString = BundleAoProducts(productBatch);
                    }

                    //Loop through the rest of the products list and create the Batch records
                    foreach (IProductBatch product in productBatch)
                    {
                        if (product.ProductId == (int)ProductEnum.UnifiedLogin)
                        {
                            continue;
                        }

                        if (product.ProductId == (int)ProductEnum.AssetOptimizer)
                        {
                            // special treatment for bundled AO products by passing json string
                            SaveProductBatch(pbRepository, product, createUserResponse, saveProductBatchError,
                                createUserPersonaId, personaId, createUserRealPageId, errorStatus,
                                aoInputJsonString);
                        }
                        else
                        {
                            SaveProductBatch(pbRepository, product, createUserResponse, saveProductBatchError,
                                createUserPersonaId, personaId, createUserRealPageId, errorStatus,
                                JsonConvert.SerializeObject(product.InputJson));
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

        #region Edit Profile - Product Batch

        public class EditorAssignedPersona
        {
            public long AssignedPersonaId { get; set; }

            public int AssignedUserTypeId { get; set; }

            public long EditorPersonaId { get; set; }

            public Guid EditorPersonaRealPageId { get; set; }

            public Guid OrganizationRealPageId { get; set; }
        }

        #endregion
    }
}
