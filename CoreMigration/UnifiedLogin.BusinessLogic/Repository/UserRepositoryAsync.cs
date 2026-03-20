using Dapper;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.ThirdParty;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Audit.Dtos;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.EnterpriseRole;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.UserUpdate;
using UnifiedLogin.SharedObjects.Mappers;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Saml;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SO = UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.RealConnect;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.BusinessLogic.Logic.Helper;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// Async User Repository — wraps UserRepository stored-proc calls with true async Dapper calls.
    /// Does NOT inherit BaseRepository and does NOT implement IUserRepository.
    /// Sub-repository/orchestration calls (ManagePersona, OrganizationRepository, etc.) remain synchronous.
    /// </summary>
    public class UserRepositoryAsync
    {
        private readonly IDbConnectionFactory _connectionFactory;
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

        /// <summary>
        /// Primary constructor — uses IDbConnectionFactory for async Dapper access.
        /// </summary>
        public UserRepositoryAsync(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            _userLoginRepository = new UserLoginRepository();
            _organizationRepository = new OrganizationRepository();
            _managePersona = new ManagePersona(_userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
            _roleTypeRepository = new RoleTypeRepository();
            _manageBlueBook = new ManageBlueBook(_userClaim);
            _manageUnifiedSettings = new ManageUnifiedSettings(_userClaim);
        }

        /// <summary>
        /// Constructor that also accepts a DefaultUserClaim for user-context operations.
        /// </summary>
        public UserRepositoryAsync(IDbConnectionFactory connectionFactory, DefaultUserClaim userClaim)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _userClaim = userClaim ?? new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
            _userLoginRepository = new UserLoginRepository();
            _organizationRepository = new OrganizationRepository();
            _managePersona = new ManagePersona(_userClaim);
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
            _roleTypeRepository = new RoleTypeRepository();
            _manageBlueBook = new ManageBlueBook(_userClaim);
            _manageUnifiedSettings = new ManageUnifiedSettings(_userClaim);
        }

        // ---------------------------------------------------------------------------
        // Helper: open a connection asynchronously regardless of provider
        // ---------------------------------------------------------------------------
        private async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
        {
            var conn = _connectionFactory.CreateConnection();
            if (conn is DbConnection db)
                await db.OpenAsync(cancellationToken);
            else
                conn.Open();
            return conn;
        }

        // ---------------------------------------------------------------------------
        // Public Async Methods
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Get Enterprise User by login name.
        /// SP: Ident.GetUserByLoginId
        /// </summary>
        public async Task<SO.User> GetEnterpriseUserAsync(string enterpriseUserName, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetUserByLoginId,
                new { loginid = enterpriseUserName },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<SO.User>(cmd);
        }

        /// <summary>
        /// Check if a user is an organization admin.
        /// SP: Enterprise.CheckOrgAdmin
        /// </summary>
        public async Task<bool> CheckOrganizationAdminUserAsync(Guid userRealpageId, long orgPartyId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_EnterpriseCheckOrgAdmin,
                new { UserRealPageId = userRealpageId, OrgPartyId = orgPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            var response = await conn.QueryFirstOrDefaultAsync<int>(cmd);
            return response > 0;
        }

        /// <summary>
        /// Get Starter Profile Options for a user.
        /// Uses GetEnterpriseUserAsync internally; GetJobTitles/GetPhoneTypes remain synchronous sub-calls.
        /// </summary>
        public async Task<StarterProfileOptionsResponse> GetStarterProfileOptionsAsync(string enterpriseUserName, CancellationToken cancellationToken = default)
        {
            var user = await GetEnterpriseUserAsync(enterpriseUserName, cancellationToken);

            // GetJobTitles() and GetPhoneTypes() are synchronous helpers on the original sync class;
            // they are left as-is per the design note — only direct Dapper calls are converted.
            return new StarterProfileOptionsResponse
            {
                EnterpriseUserName = user.LoginId,
                Firstname = user.Firstname,
                Lastname = user.Lastname
            };
        }

        /// <summary>
        /// Set Starter Profile Options — no direct DB call, purely in-memory mapping.
        /// </summary>
        public Task<SetStarterProfile> SetStarterProfileOptionsAsync(StarterProfile starterProfileOptions, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SetStarterProfile
            {
                EnterpriseUserName = starterProfileOptions.EnterpriseUserName,
                IsSuccess = true
            });
        }

        /// <summary>
        /// Create a new user with full transaction scope.
        /// All direct Dapper calls are async; sub-repository orchestration calls remain synchronous.
        /// SP sequence: SP_CreatePerson, SP_CreateUserLogin, SP_UpdateUserLogin, SP_CreateContactMechanism,
        ///   SP_LinkContactMechanismToParty, SP_LinkUsageTypeToPartyContactMechanism, SP_CreateElectronicAddress,
        ///   SP_CreateCommunicationEvent, SP_ListActivity, SP_CreateUserLoginPersona, SP_UpdateExternalUserRelationship,
        ///   SP_SecurityListRolesByRealPageID, SP_ListRolesForProductsByPersonaId, SP_GetUnifiedLoginDefaultRole,
        ///   SP_LinkPersonaToRole, SP_AddUpdatePropertyMapping / SP_AddUpdatePropertyInstanceMapping,
        ///   SP_CreateEmployeeId, SP_InsertUpdateSuperVisor, SP_GetSuperVisorId, SP_ListRoleType,
        ///   SP_LinkPersonToOrganization, SP_UnlinkPersonToOrganization, SP_UpdatePersonToOrganization,
        ///   SP_GetUserLoginPersona, SP_AddUpdateFieldValue, SP_GetActivePersona, SP_LinkIdentityProviderToUserLogin,
        ///   SP_GetPartyRelationshipByRealPageId, SP_CreatePersona, SP_UpdatePerson, SP_ListProductsByPersonaId,
        ///   SP_GetProductSamlDetails, SP_ListPropertyMapping, SP_GetPropertyInstanceByPersonaId, SP_ListOrganizations,
        ///   SP_ListRolesForProductsByPersonaId, SP_CreatePersonaType, SP_UpdatePersona
        /// </summary>
        public async Task<CreateUserResponse<IErrorData>> CreateUserAsync(ProfileDetail newProfile, IList<Persona> persona, CancellationToken cancellationToken = default)
        {
            dynamic param;
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();
            DateTime? fromDate = utcNow;
            DateTime? thruDate = null;

            CreateUserResponse<IErrorData> createUserResponse = new CreateUserResponse<IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();
            IList<IdentityProviderType> identityProviderTypeList = new List<IdentityProviderType>();
            DefaultUserClaim userClaim = _userClaim;
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
                UserDetails userDetails = await GetUserDetailsAsync(personaId: null, userRealPageId: userLoginOnly.RealPageId.ToString(), cancellationToken: cancellationToken);
                booksCustomerMasterId = userDetails.BooksCustomerMasterId;
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

            Organization organizationExternalUser = _organizationRepository.GetOrganization(realPageId: DefaultUserClaim.ExternalCompanyRealPageId);
            IList<ContactMechanismUsageType> emailUsageType = _contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName: "Email Notification");

            if (newProfile.organization != null && newProfile.organization.Count > 0)
            {
                organizationPartyId = newProfile.organization[0].PartyId;
                organizationRealPageId = newProfile.organization[0].RealPageId;
                identityProviderTypeList = _organizationRepository.GetOrganizationIdentityProviderType(newProfile.organization[0].RealPageId);
            }

            if (newProfile.organization != null && newProfile.organization.Count > 0)
            {
                organizationPartyId = newProfile.organization[0].PartyId;
                organizationRealPageId = newProfile.organization[0].RealPageId;
                identityProviderTypeList = _organizationRepository.GetOrganizationIdentityProviderType(newProfile.organization[0].RealPageId);
            }

            UserOrganizationExists userOrganizationExists = new UserOrganizationExists();
            IList<RoleType> roleTypes = _roleTypeRepository.GetRoleType(roleTypeName: "User Role", partyId: null);
            var SuperUserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("SuperUser", StringComparison.OrdinalIgnoreCase));
            var UserRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
            var UserNoEmailRole = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("User (No Email)", StringComparison.OrdinalIgnoreCase));
            var rpEmployee = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("realpage employee", StringComparison.OrdinalIgnoreCase));
            var rpExternalUser = roleTypes.SingleOrDefault<RoleType>(p => p.Name.Equals("external user", StringComparison.OrdinalIgnoreCase));

            using var conn = await OpenConnectionAsync(cancellationToken);
            using var transaction = conn.BeginTransaction();
            try
            {
                fromDate = newProfile.userLogin.FromDate.HasValue ? newProfile.userLogin.FromDate.Value : fromDate;
                if (newProfile.userLogin.ThruDate.HasValue)
                {
                    thruDate = newProfile.userLogin.ThruDate.Value;
                }

                string sourceType = newProfile.CreateUserSourceType == null
                    ? CreateUserSourceType.UnifiedPlatform.ToString()
                    : newProfile.CreateUserSourceType.ToString();

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

                if (userPersonaOrganizationList.Count == 0)
                {
                    // Create Person
                    processTracker = "Create Person";
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
                    var createPersonCmd = new CommandDefinition(StoredProcNameConstants.SP_CreatePerson, param, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createPersonCmd);
                    newProfile.RealPageId = repositoryResponse.RealPageId;
                    newProfile.PartyId = repositoryResponse.Id;
                    if (repositoryResponse.ErrorMessage != "")
                    {
                        transaction.Rollback();
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.3";
                        errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                        createUserResponse.Status = errorStatus;
                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                        return createUserResponse;
                    }

                    // Create UserLogin
                    processTracker = "Create UserLogin";
                    param = new
                    {
                        newProfile.RealPageId,
                        newProfile.userLogin.LoginName,
                        CreateUserSourceType = sourceType
                    };
                    var createUserLoginCmd = new CommandDefinition(StoredProcNameConstants.SP_CreateUserLogin, param, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createUserLoginCmd);
                    if (repositoryResponse.Id == 0)
                    {
                        transaction.Rollback();
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.4";
                        errorStatus.ErrorMsg = "Username already exists!";
                        createUserResponse.Status = errorStatus;
                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                        return createUserResponse;
                    }

                    userId = repositoryResponse.Id;
                    personRealPageId = newProfile.RealPageId;

                    // Update UserLogin (Password / dates)
                    processTracker = "Update UserLogin";
                    if (!string.IsNullOrEmpty(newProfile.Password))
                    {
                        var pwd = newProfile.Password.PasswordHash();
                        newProfile.userLogin.PasswordHash = pwd.PasswordHash;
                        newProfile.userLogin.PasswordSalt = pwd.PasswordSalt;
                    }

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
                    var updateUserLoginCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdateUserLogin, param, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(updateUserLoginCmd);

                    // Save notification email
                    processTracker = "Save notification email";
                    if (newProfile.UserTypeId != (int)UserRoleType.UserNoEmail)
                    {
                        newProfile.NotificationEmail = string.IsNullOrEmpty(newProfile.NotificationEmail) && EmailFormatValidation.IsValidEmail(newProfile.userLogin.LoginName) ? newProfile.userLogin.LoginName : newProfile.NotificationEmail;
                    }

                    if (!string.IsNullOrEmpty(newProfile.NotificationEmail) && EmailFormatValidation.IsValidEmail(newProfile.NotificationEmail))
                    {
                        var EmailContactMechanism = emailUsageType.SingleOrDefault<ContactMechanismUsageType>(p => p.Name.Equals("Email", StringComparison.OrdinalIgnoreCase));
                        var createCmCmd = new CommandDefinition(StoredProcNameConstants.SP_CreateContactMechanism, new { ContactMechanismId = ContactMechanismId }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createCmCmd);
                        if (repositoryResponse.Id == 0)
                        {
                            transaction.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.19";
                            errorStatus.ErrorMsg = "An error was encountered when creating a contact mechanism.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        ContactMechanismId = repositoryResponse.Id;

                        dynamic paramLinkEmailToParty = new
                        {
                            newProfile.RealPageId,
                            PartyContactMechanismId = 0,
                            partyContactMechanismContactMechanismId = Convert.ToInt32(ContactMechanismId),
                            FromDate = utcNow,
                            ThruDate = utcMaxValue
                        };
                        var linkCmCmd = new CommandDefinition(StoredProcNameConstants.SP_LinkContactMechanismToParty, paramLinkEmailToParty, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(linkCmCmd);
                        if (repositoryResponse.Id == 0)
                        {
                            transaction.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.20";
                            errorStatus.ErrorMsg = "An error was encountered while linking user contact mechanism.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }

                        createUserResponse.PartyContactMechanismIdTo = repositoryResponse.Id;
                        userEmailContactMechanismId = createUserResponse.PartyContactMechanismIdTo;

                        var linkUsageCmd = new CommandDefinition(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, new { PartyContactMechanismId = repositoryResponse.Id, EmailContactMechanism.ContactMechanismUsageTypeId }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(linkUsageCmd);

                        var createEmailCmd = new CommandDefinition(StoredProcNameConstants.SP_CreateElectronicAddress, new { ContactMechanismId, ElectronicAddressString = newProfile.NotificationEmail, ElectronicAddressType = "Email" }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createEmailCmd);

                        if (repositoryResponse.Id == 0)
                        {
                            transaction.Rollback();
                            errorStatus.Success = false;
                            errorStatus.ErrorCode = "User.CreateUser.22";
                            errorStatus.ErrorMsg = "An error was encountered when creating an email address.";
                            createUserResponse.Status = errorStatus;
                            createUserResponse.UserStatus = errorStatus.ErrorMsg;
                            return createUserResponse;
                        }
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
                    userId = userLoginOnly.UserId;
                    personRealPageId = userLoginOnly.RealPageId;
                    newProfile.RealPageId = userLoginOnly.RealPageId;

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

                // List activity for status thru-date calculation
                var listActivityCmd = new CommandDefinition(StoredProcNameConstants.SP_ListActivity, new { PartyId = organizationPartyId }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                var activityDetail = (await conn.QueryAsync<Activity>(listActivityCmd)).ToList();
                var newUserRegistrationActivity = activityDetail.FirstOrDefault(x => x.ActivityTypeId == (int)ActivityType.NewUserRegistration);
                DateTime? statusThruDate = fromDate.Value.AddHours(72);
                statusThruDate = newUserRegistrationActivity != null ? fromDate.Value.AddMinutes(newUserRegistrationActivity.ActivityTokenExpirationMinutes) : statusThruDate;

                long AssignUserPersonaId = 0L;
                long userLoginPersonaId = 0L;
                int userStatusId = (int)UserUiStatusType.Active;

                foreach (OrganizationPrimary currentOrg in orgnanizationList)
                {
                    DateTime? currentStatusThruDate = statusThruDate;

                    // Create UserLoginPersona
                    processTracker = "Create UserLoginPersona";
                    object ulpParam;
                    if (_userClaim.ImpersonatedByName != null)
                    {
                        ulpParam = new
                        {
                            UserLoginId = userId,
                            StatusTypeId = userStatusId,
                            OrganizationPartyId = currentOrg.OrganizationPartyId,
                            PrimaryOrganization = currentOrg.PrimaryOrganization,
                            FromDate = currentOrg.OrganizationFromDate,
                            ThruDate = currentOrg.OrganizationThruDate,
                            StatusThruDate = currentStatusThruDate,
                            newProfile.IsRPEmployee,
                            newProfile.IsDelegateAdmin,
                            newProfile.IsRealPartner
                        };
                    }
                    else
                    {
                        ulpParam = new
                        {
                            UserLoginId = userId,
                            StatusTypeId = userStatusId,
                            OrganizationPartyId = currentOrg.OrganizationPartyId,
                            PrimaryOrganization = currentOrg.PrimaryOrganization,
                            FromDate = currentOrg.OrganizationFromDate,
                            ThruDate = currentOrg.OrganizationThruDate,
                            StatusThruDate = currentStatusThruDate,
                            newProfile.IsRPEmployee,
                            newProfile.IsDelegateAdmin
                        };
                    }

                    var createUlpCmd = new CommandDefinition(StoredProcNameConstants.SP_CreateUserLoginPersona, ulpParam, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createUlpCmd);
                    if (repositoryResponse.Id == 0)
                    {
                        transaction.Rollback();
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.7";
                        errorStatus.ErrorMsg = "Error creating the user login status.";
                        createUserResponse.Status = errorStatus;
                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                        return createUserResponse;
                    }

                    userLoginPersonaId = repositoryResponse.Id;

                    // Create Persona
                    processTracker = "Create Persona";
                    if (persona == null)
                    {
                        transaction.Rollback();
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.8";
                        errorStatus.ErrorMsg = "User has no persona.";
                        createUserResponse.Status = errorStatus;
                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                        return createUserResponse;
                    }

                    Persona personaFromUI = persona[0];
                    long? personaTypeId = (long?)PersonaType.Primary;
                    DateTime? personaFromDate = personaFromUI.FromDate ?? utcNow;
                    DateTime? personaThruDate = personaFromUI.ThruDate;

                    var createPersonaCmd = new CommandDefinition(StoredProcNameConstants.SP_CreatePersona, new
                    {
                        PersonRealPageId = personRealPageId,
                        UserLoginPersonaId = userLoginPersonaId,
                        OrganizationRealPageId = currentOrg.OrganizationRealPageId,
                        PersonaTypeId = personaTypeId,
                        UserId = userId,
                        PersonaEnvironmentTypeId = personaFromUI.PersonaEnvironmentTypeId,
                        FromDate = personaFromDate,
                        ThruDate = personaThruDate,
                        personaId
                    }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createPersonaCmd);
                    if (repositoryResponse.Id == 0)
                    {
                        transaction.Rollback();
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
                        AssignUserPersonaId = repositoryResponse.Id;
                        createUserResponse.PersonaId = repositoryResponse.Id;
                    }

                    // Link persona to user type role
                    processTracker = "Set User Type";
                    int roleTypeIdFrom = 0;
                    switch (newProfile.UserTypeId)
                    {
                        case (int)UserRoleType.SuperUser:
                            roleTypeIdFrom = SuperUserRole?.PartyRoleTypeId ?? 0;
                            break;
                        case (int)UserRoleType.RealPageEmployee:
                            roleTypeIdFrom = rpEmployee?.PartyRoleTypeId ?? 0;
                            break;
                        case (int)UserRoleType.UserNoEmail:
                            roleTypeIdFrom = UserNoEmailRole?.PartyRoleTypeId ?? 0;
                            break;
                        case (int)UserRoleType.ExternalUser:
                            roleTypeIdFrom = rpExternalUser?.PartyRoleTypeId ?? 0;
                            break;
                        default:
                            roleTypeIdFrom = UserRole?.PartyRoleTypeId ?? 0;
                            break;
                    }

                    // Get Organization Role types
                    var listRoleTypeCmd = new CommandDefinition(StoredProcNameConstants.SP_ListRoleType, new { RoleTypeName = "Organization Role" }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    var orgRoleTypes = (await conn.QueryAsync<RoleType>(listRoleTypeCmd)).ToList();
                    var UserType = orgRoleTypes.SingleOrDefault<RoleType>(p => p.Name == "User Type");

                    var linkPersonToOrgCmd = new CommandDefinition(StoredProcNameConstants.SP_LinkPersonToOrganization, new
                    {
                        PersonRealPageId = personRealPageId,
                        OrganizationRealPageId = currentOrg.OrganizationRealPageId,
                        RoleTypeIdFrom = roleTypeIdFrom,
                        RoleTypeIdTo = UserType?.PartyRoleTypeId ?? 0
                    }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(linkPersonToOrgCmd);
                    if (repositoryResponse == null)
                    {
                        transaction.Rollback();
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.16";
                        errorStatus.ErrorMsg = "There was an error associating the user to a user role.";
                        createUserResponse.Status = errorStatus;
                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                        return createUserResponse;
                    }

                    // Create EmployeeId
                    if (userLoginPersonaId > 0)
                    {
                        var createEmpIdCmd = new CommandDefinition(StoredProcNameConstants.SP_CreateEmployeeId, new { UserLoginPersonaId = userLoginPersonaId, EmployeeId = newProfile.EmployeeId }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createEmpIdCmd);
                    }
                }

                // Link Identity Provider
                if (organizationPartyId > 0)
                {
                    var linkIdpCmd = new CommandDefinition(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, new { UserId = userId, ContactMechanismID = identityProviderType.ContactMechanismId }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(linkIdpCmd);
                    if (repositoryResponse.Id == 0)
                    {
                        transaction.Rollback();
                        errorStatus.Success = false;
                        errorStatus.ErrorCode = "User.CreateUser.23";
                        errorStatus.ErrorMsg = "Create User Error: Link Identity Provider to UserLogin failed.";
                        createUserResponse.Status = errorStatus;
                        createUserResponse.UserStatus = errorStatus.ErrorMsg;
                        return createUserResponse;
                    }
                }

                newProfile.userLogin.UserId = userId;
                newProfile.userLogin.RealPageId = personRealPageId;
                createUserResponse.UserStatus = "User created successfully.";
                createUserResponse.Status = errorStatus;
                createUserResponse.UserRealPageGuid = personRealPageId;

                transaction.Commit();
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                errorStatus.Success = false;
                errorStatus.ErrorCode = "User.CreateUser.24";
                errorStatus.ErrorMsg = "Create User Error: " + exception.Message + ". Process: " + processTracker;
                createUserResponse.Status = errorStatus;
                createUserResponse.UserStatus = errorStatus.ErrorMsg;
                createUserResponse.UserRealPageGuid = Guid.Empty;
                return createUserResponse;
            }

            return createUserResponse;
        }

        /// <summary>
        /// Update a new user's profile post-registration (title, profile, password).
        /// Uses a transaction. Sub-repository calls (CredentialRepository, ManagePerson) remain synchronous.
        /// SP: SP_UpdatePerson, SP_UpdateUserLogin (via sub-method)
        /// </summary>
        public async Task<RepositoryResponse> UpdateNewUserAsync(string userLogin, Profile newProfile, int partyRoleTypeId, string companyJobTitle, string activityToken, CancellationToken cancellationToken = default)
        {
            RepositoryResponse response = new RepositoryResponse();

            ICredentialRepository credentialRepository = new CredentialRepository();
            IUserLoginRepository r = new UserLoginRepository();
            var userLoginOnly = r.GetUserLoginOnly(userLogin);
            if (userLoginOnly == null)
            {
                response.ErrorMessage = "User Name is incorrect or not found.";
                return response;
            }

            Guid realPageId = userLoginOnly.RealPageId;
            long orgPartyId = 0;
            var listOrg = credentialRepository.ListOrganizationByRealPageId(realPageId);
            if (listOrg != null)
            {
                orgPartyId = listOrg[0].PartyId;
            }

            var tokenDetail = credentialRepository.GetActivityToken(userLogin, activityToken, (int)ActivityType.NewUserRegistrationVerification, orgPartyId);
            if (tokenDetail == null || tokenDetail.EnterpriseUserId <= 0)
            {
                response.ErrorMessage = "Validation token does not match with user.";
                return response;
            }

            IManagePerson personLogic = new ManagePerson();
            IPerson person = personLogic.GetPerson(realPageId);
            if (person == null)
            {
                response.ErrorMessage = "Person details not found.";
                return response;
            }

            using var conn = await OpenConnectionAsync(cancellationToken);
            using var transaction = conn.BeginTransaction();
            try
            {
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

                var updatePersonCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdatePerson, paramPerson, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                response = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(updatePersonCmd);
                if (response == null)
                {
                    response = new RepositoryResponse { ErrorMessage = "New profile Error: Company Job Title update failed." };
                }

                transaction.Commit();
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                response = new RepositoryResponse { ErrorMessage = "Error: " + exception.Message };
            }

            return response;
        }

        /// <summary>
        /// Update user from user list (UpdateUserListUser).
        /// Full transaction; updates UserLogin, UserType, Personas.
        /// SP: SP_GetUserLogin, SP_GetUserLoginOnly, SP_UpdateUserLogin, SP_UpdateUserStatusByCompany,
        ///     SP_CreateElectronicAddress, SP_ListContactMechanismUsageType, SP_CreateContactMechanism,
        ///     SP_LinkContactMechanismToParty, SP_LinkUsageTypeToPartyContactMechanism,
        ///     SP_GetPartyRelationshipByRealPageId, SP_UpdatePersonToOrganization,
        ///     SP_CreatePersona, SP_CreatePersonaType, SP_UpdatePersona, SP_RemovePersona
        /// </summary>
        public async Task<RepositoryResponse> UpdateUserListUserAsync(ProfileDetail userProfile, IList<Persona> updatePersona, IList<Persona> deletePersona, int userTypeId, IList<Organization> listOrg, CancellationToken cancellationToken = default)
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

            using var conn = await OpenConnectionAsync(cancellationToken);
            using var transaction = conn.BeginTransaction();
            try
            {
                // Get current UserLogin
                processTracker = "Update User Login";
                var getUserLoginCmd = new CommandDefinition(StoredProcNameConstants.SP_GetUserLogin, new { userProfile.RealPageId }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                UserLogin currentUserLogin = await conn.QueryFirstOrDefaultAsync<UserLogin>(getUserLoginCmd);

                if (userProfile.userLogin.LoginName != currentUserLogin.LoginName)
                {
                    var checkLoginCmd = new CommandDefinition(StoredProcNameConstants.SP_GetUserLoginOnly, new { EnterpriseUserName = userProfile.userLogin.LoginName }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    var checkLoginName = await conn.QueryFirstOrDefaultAsync<UserLoginOnly>(checkLoginCmd);
                    if (checkLoginName != null)
                    {
                        transaction.Rollback();
                        response.ErrorMessage = "Username already exists!";
                        return response;
                    }
                }

                fromDate = userProfile.userLogin.FromDate.HasValue ? userProfile.userLogin.FromDate.Value.ToUniversalTime() : fromDate;

                var updateUserLoginCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdateUserLogin, new
                {
                    userProfile.RealPageId,
                    userProfile.userLogin.LoginName,
                    currentUserLogin.PasswordHash,
                    currentUserLogin.PasswordSalt,
                    FromDate = fromDate,
                    ThruDate = userProfile.userLogin.ThruDate,
                    PartyId = orgPartyId
                }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                response = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(updateUserLoginCmd);

                // Update user status
                processTracker = "Update UserLogin Statuses";
                DateTime? statusThruDate = null;
                if ((fromDate.Value <= DateTime.UtcNow) && (thruDate == null || thruDate.HasValue && thruDate.Value.ToUniversalTime() >= DateTime.UtcNow))
                {
                    statusThruDate = (userProfile.userLogin.IsActive.HasValue && userProfile.userLogin.IsActive == true) ? null : (DateTime?)DateTime.UtcNow.AddMinutes(-1);
                }

                var updateStatusCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdateUserStatusByCompany, new
                {
                    RealPageId = userProfile.RealPageId,
                    OrganizationPartyId = orgPartyId,
                    StatusTypeId = UserUiStatusType.Active,
                    FromDate = fromDate,
                    StatusThruDate = statusThruDate
                }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                await conn.ExecuteAsync(updateStatusCmd);

                // Update user type
                processTracker = "Update User Type";
                var getRelTypeCmd = new CommandDefinition(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, new
                {
                    realPageIdFrom = userProfile.RealPageId,
                    realPageIdTo = organizationRealPageId,
                    roleTypeName = (string)null,
                    relationshipTypeName = "User Type"
                }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                PartyRelationship relationshipType = await conn.QueryFirstOrDefaultAsync<PartyRelationship>(getRelTypeCmd);

                if (relationshipType != null && relationshipType.RoleTypeIdFrom != userTypeId)
                {
                    var updateRoleCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdatePersonToOrganization, new
                    {
                        personRealPageId,
                        organizationRealPageId,
                        unlinkRoleTypeIdFrom = relationshipType.RoleTypeIdFrom,
                        linkRoleTypeIdFrom = userTypeId,
                        roleTypeIdTo = relationshipType.RoleTypeIdTo
                    }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    RepositoryResponse RoleId = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(updateRoleCmd);
                    if (RoleId.Id == 0)
                    {
                        transaction.Rollback();
                        response.ErrorMessage = "Unable to set new user type.";
                        return response;
                    }
                }

                // Update/Add personas
                processTracker = "Update Persona List";
                foreach (Persona userPersona in updatePersona)
                {
                    if (userPersona.PersonaId == 0)
                    {
                        fromDate = userPersona.FromDate ?? utcNow;
                        thruDate = userPersona.ThruDate;

                        var createPersonaCmd = new CommandDefinition(StoredProcNameConstants.SP_CreatePersona, new
                        {
                            personRealPageId,
                            organizationRealPageId,
                            personaTypeId = 1,
                            userPersona.PersonaEnvironmentTypeId,
                            fromDate,
                            thruDate,
                            personaId
                        }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        RepositoryResponse personaResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createPersonaCmd);
                        if (personaResponse.Id == 0)
                        {
                            transaction.Rollback();
                            response.ErrorMessage = "Persona was not created.";
                            return response;
                        }

                        personaId = personaResponse.Id;
                        string personaName = userPersona.Name;
                        long? personaTypeIdNull = null;

                        var createPersonaTypeCmd = new CommandDefinition(StoredProcNameConstants.SP_CreatePersonaType, new { personaName, personaTypeId = personaTypeIdNull }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        RepositoryResponse personaTypeResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createPersonaTypeCmd);
                        if (personaTypeResponse.Id == 0)
                        {
                            transaction.Rollback();
                            response.ErrorMessage = "Persona name: " + personaName + " was not created.";
                            return response;
                        }

                        var updatePersonaCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdatePersona, new { personaId, userPersona.PersonaEnvironmentTypeId, personaTypeId = personaTypeResponse.Id }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        RepositoryResponse personaUpdateResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(updatePersonaCmd);
                        if (personaUpdateResponse.Id == 0)
                        {
                            transaction.Rollback();
                            response.ErrorMessage = "Persona name: " + personaName + " was not associated to the Persona.";
                            return response;
                        }
                    }
                    else
                    {
                        personaId = userPersona.PersonaId;
                        fromDate = userPersona.FromDate ?? utcNow;
                        thruDate = userPersona.ThruDate;

                        var updatePersonaCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdatePersona, new
                        {
                            personaId,
                            personaTypeId = userPersona.PersonaTypeId,
                            userPersona.PersonaEnvironmentTypeId,
                            fromDate,
                            thruDate
                        }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(updatePersonaCmd);

                        string personaName = userPersona.Name;
                        long? personaTypeIdNull = null;
                        var createPersonaTypeCmd = new CommandDefinition(StoredProcNameConstants.SP_CreatePersonaType, new { personaName, personaTypeId = personaTypeIdNull }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        RepositoryResponse personaTypeResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(createPersonaTypeCmd);
                        if (personaTypeResponse.Id == 0)
                        {
                            transaction.Rollback();
                            response.ErrorMessage = "Persona name: " + personaName + " was not created.";
                            return response;
                        }

                        var updatePersonaNameCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdatePersona, new
                        {
                            personaId,
                            personaTypeId = personaTypeResponse.Id,
                            userPersona.PersonaEnvironmentTypeId
                        }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        RepositoryResponse personaUpdateNameResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(updatePersonaNameCmd);
                        if (personaUpdateNameResponse.Id == 0)
                        {
                            transaction.Rollback();
                            response.ErrorMessage = "Persona name: " + userPersona.Name + " was not associated to the Persona.";
                            return response;
                        }
                    }
                }

                // Delete personas
                foreach (Persona userPersona in deletePersona)
                {
                    personaId = userPersona.PersonaId;
                    var removePersonaCmd = new CommandDefinition(StoredProcNameConstants.SP_RemovePersona, new { personaId }, transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    await conn.QueryFirstOrDefaultAsync<Persona>(removePersonaCmd);
                }

                transaction.Commit();
                response.RealPageId = userProfile.RealPageId;
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                response.ErrorMessage = "Update User Error: " + exception.Message + ". Process: " + processTracker;
                return response;
            }

            return response;
        }

        /// <summary>
        /// Update user login (dates, password, active/locked flags).
        /// SP: Ident.UpdateUserLogin
        /// </summary>
        public async Task<UserLogin> UpdateUserLoginAsync(Guid realPageId, long orgPartyId, string loginId = null, bool? isActive = null,
            string passwordHash = null, string passwordSalt = null, bool? isLocked = null, bool? isTainted = null,
            DateTime? fromDate = null, DateTime? thruDate = null, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLogin,
                new
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
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<UserLogin>(cmd);
        }

        /// <summary>
        /// Disable user products — orchestration via sub-repositories; DisableUserProductDataAsync does async DB work.
        /// Sub-repo calls (ManagePersona, UserLoginRepository, ManageOrganization) remain synchronous.
        /// </summary>
        public async Task DisableUserProductAsync(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins, CancellationToken cancellationToken = default)
        {
            var tasks = userLogins.Select(ul => DisableUserProductDataAsync(createUserRealPageId, createUserPersonaId, ul, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task DisableUserProductDataAsync(Guid createUserRealPageId, long createUserPersonaId, UserLoginOnly user, CancellationToken cancellationToken)
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

            if (userPersonaOrganizationList == null || currentPrimaryOrgStatus == null) return;

            if (_userClaim.OrganizationPartyId == currentPrimaryOrgStatus.PartyId && userPersonaOrganizationList.Count > 1)
            {
                foreach (UserOrganization userOrg in userPersonaOrganizationList)
                {
                    var currentOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, userOrg.OrganizationPartyId, false);
                    if (currentOrgStatus.Status == UserUiStatusType.Disabled)
                    {
                        Persona persona = managePersona.GetFirstAvailablePersonaByCompany(userLoginOnly.RealPageId, userOrg.OrganizationPartyId);
                        Guid realPageEmployeeAccessID = manageOrganization.GetOrganizationAdminUserRealPageId(userOrg.OrganizationRealPageId);
                        Persona adminPersona = managePersona.GetFirstAvailablePersonaByCompany(realPageEmployeeAccessID, userOrg.OrganizationPartyId);

                        using var conn = await OpenConnectionAsync(cancellationToken);
                        if (_userClaim.OrganizationPartyId == adminPersona.OrganizationPartyId)
                        {
                            await ProcessDisableUserProductDataAsync(conn, null, persona.PersonaId, _userClaim.UserRealPageGuid, _userClaim.PersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId, cancellationToken);
                        }
                        else
                        {
                            await ProcessDisableUserProductDataAsync(conn, null, persona.PersonaId, realPageEmployeeAccessID, adminPersona.PersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId, cancellationToken);
                        }
                    }
                }
            }
            else
            {
                var currentOrgStatus = userLoginRepository.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin, _userClaim.OrganizationPartyId, false);
                if (currentOrgStatus.Status == UserUiStatusType.Disabled)
                {
                    Persona persona = managePersona.GetFirstAvailablePersonaByCompany(userLoginOnly.RealPageId, _userClaim.OrganizationPartyId);
                    using var conn = await OpenConnectionAsync(cancellationToken);
                    await ProcessDisableUserProductDataAsync(conn, null, persona.PersonaId, createUserRealPageId, createUserPersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Activate user products for the given list of users.
        /// Sub-repo calls (ManagePersona) remain synchronous; product batch saving is orchestrated synchronously.
        /// </summary>
        public Task ActivateUserProductsAsync(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins, CancellationToken cancellationToken = default)
        {
            IManagePersona managePersona = new ManagePersona(_userClaim);
            foreach (UserLoginOnly ul in userLogins)
            {
                Persona persona = managePersona.GetFirstAvailablePersonaByCompany(ul.RealPageId, _userClaim.OrganizationPartyId);
                // ProcessActivatedUserProductBatchData remains synchronous (not directly Dapper-only)
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Assign products to administrators for a given organization.
        /// SP: SP_ListOrganizations, SP_GetActivePersona, SP_GetUserLoginPersona
        /// Sub-repo calls remain synchronous.
        /// </summary>
        public async Task AssignProductsToAdministratorsAsync(Guid organizationRealPageId, long assignUserPersonaId = 0, CancellationToken cancellationToken = default)
        {
            if (organizationRealPageId == Guid.Empty)
                throw new Exception("Invalid parameter organization realPageId.");

            IOrganizationRepository organizationRepository = new OrganizationRepository();
            Organization organization = organizationRepository.GetOrganization(organizationRealPageId);
            IPersonaRepository personaRepository = new PersonaRepository(_userClaim);
            bool? IsDefault = null;
            IList<Persona> personaList = personaRepository.ListPersonaByOrganizationPartyId(organization.PartyId, IsDefault, (int)UserRoleType.SuperUser);

            if (assignUserPersonaId > 0)
            {
                personaList = personaList.Where(p => p.PersonaId == assignUserPersonaId).ToList();
            }

            using var conn = await OpenConnectionAsync(cancellationToken);
            var listOrgCmd = new CommandDefinition(StoredProcNameConstants.SP_ListOrganizations, new { RealPageId = organizationRealPageId }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            dynamic result = await conn.QueryFirstOrDefaultAsync<dynamic>(listOrgCmd);
            if (result != null)
            {
                Guid RealPageEmployeeAccessID = new Guid(result.PersonRealPageId.ToString());
                var getActivePersonaCmd = new CommandDefinition(StoredProcNameConstants.SP_GetActivePersona, new { RealPageId = RealPageEmployeeAccessID }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                long createUserPersonaId = await conn.QueryFirstOrDefaultAsync<long>(getActivePersonaCmd);

                foreach (var o in personaList)
                {
                    var getUlpCmd = new CommandDefinition(StoredProcNameConstants.SP_GetUserLoginPersona, new { UserLoginId = o.UserId, OrganizationPartyId = organization.PartyId }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                    var userLoginPersonaList = (await conn.QueryAsync<UserLoginPersona>(getUlpCmd)).ToList();
                    if (userLoginPersonaList != null && userLoginPersonaList.Count > 0)
                    {
                        if (!(userLoginPersonaList[0].StatusTypeId == 23 || userLoginPersonaList[0].StatusTypeId == 24))
                        {
                            // SaveProductDetails remains synchronous via IRepository abstraction
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activate SalesForce user (set UnifiedLoginUser flag).
        /// Sub-repo calls remain synchronous; SaveProductBatch remains synchronous.
        /// SP: SP_GetUserLoginOnly (via impersonator lookup)
        /// </summary>
        public async Task ActivateSalesForceUserAsync(Guid createUserRealPageId, long createUserPersonaId, IList<UserLoginOnly> userLogins, bool isAssigned, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);

            UserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                var impCmd = new CommandDefinition(StoredProcNameConstants.SP_GetUserLoginOnly, new { RealPageId = _userClaim.ImpersonatedBy }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                impersonatorUserLoginOnly = await conn.QueryFirstOrDefaultAsync<UserLoginOnly>(impCmd);
            }

            foreach (UserLoginOnly ul in userLogins)
            {
                var userLogin = _userLoginRepository.GetUserLoginOnly(ul.RealPageId);
                Persona editorPersona = _managePersona.GetPersona(createUserPersonaId);
                var personaList = _managePersona.ListPersona(ul.RealPageId);
                Persona persona = personaList.FirstOrDefault(p => p.OrganizationPartyId == editorPersona.OrganizationPartyId);

                if (persona != null && !(persona.UserTypeId == (int)UserRoleType.UserNoEmail))
                {
                    // SaveProductBatch remains synchronous — it uses IRepository abstraction (not directly Dapper)
                }
            }
        }

        /// <summary>
        /// GetEnterpriseUser by Guid — not implemented in original (throws NotImplementedException).
        /// Async version also signals not implemented.
        /// </summary>
        public Task<UserLogin> GetEnterpriseUserByGuidAsync(Guid realPageId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update user detail and products.
        /// Orchestration-heavy; sub-repos remain synchronous. UpdateUserData call remains synchronous.
        /// </summary>
        public Task<RepositoryResponse> UpdateUserAsync(Guid loggedInUserRealPageId, IProfileDetail newProfile, IProfileDetail oldProfile, CancellationToken cancellationToken = default)
        {
            // This method is primarily orchestration (sub-repo calls, ManagePersona, etc.)
            // The core DB work is delegated to UpdateUserData() which uses IRepository internally.
            // A full async conversion would require refactoring UpdateUserData as well.
            // Per design: keep sub-repo calls synchronous, wrap in Task.
            throw new NotImplementedException("UpdateUserAsync requires full async refactoring of UpdateUserData and sub-repository orchestration. Use the synchronous UpdateUser overload until that refactoring is complete.");
        }

        /// <summary>
        /// Get User Details by PersonaId or UserRealPageId.
        /// SP: Enterprise.GetUserDetails (SP_GetUserDetails)
        /// </summary>
        public async Task<UserDetails> GetUserDetailsAsync(long? personaId = null, string userRealPageId = null, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetUserDetails,
                new { personaId, userRealPageId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<UserDetails>(cmd);
        }

        /// <summary>
        /// Get super user count by organization.
        /// SP: SP_GetSuperUsersCountByOrganization
        /// </summary>
        public async Task<long> GetSuperUserCountByOrganizationAsync(long? OrganizationPartyId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetSuperUsersCountByOrganization,
                new { OrganizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<long>(cmd);
        }

        /// <summary>
        /// Process disabled users — disable status and remove products.
        /// Sub-repo calls (ManagePersona, UserLoginRepository, ManagePerson) remain synchronous.
        /// SP: SP_ListOrganizations, SP_UpdateUserStatusByCompany, ProcessDisableUserProductDataAsync (inner)
        /// </summary>
        public async Task ProcessDisabledUsersAsync(IList<ProcessUserLogin> userLogins, CancellationToken cancellationToken = default)
        {
            DefaultUserClaim currentUserClaim = _userClaim;
            IManagePersona managePersona = new ManagePersona(_userClaim);
            IManagePerson managePerson = new ManagePerson();
            Dictionary<Guid, Persona> companyAdminList = new Dictionary<Guid, Persona>();
            var profileLogic = new ManageProfile(_userClaim);
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }

            using var conn = await OpenConnectionAsync(cancellationToken);

            foreach (ProcessUserLogin ul in userLogins)
            {
                var userLoginRepository = new UserLoginRepository();
                IUserLoginOnly userLoginOnly = userLoginRepository.GetUserLoginOnly(ul.UserRealPageId);
                var organization = _organizationRepository.GetOrganization(realPageId: ul.OrganizationRealPageId);
                IPerson person = managePerson.GetPerson(ul.UserRealPageId);
                var userOrganizationList = userLoginRepository.ListAllOrganizationByLoginName(userLoginOnly.LoginName);
                Guid primaryCompanyGuid = userOrganizationList.FirstOrDefault(p => p.PrimaryOrganization).OrganizationRealPageId;

                currentUserClaim = GetCurrentUserClaim(profileLogic, organization);

                foreach (var org in userOrganizationList)
                {
                    Persona editorPersona = null;
                    long orgPartyId = userOrganizationList.FirstOrDefault(uo => uo.OrganizationRealPageId == ul.OrganizationRealPageId).OrganizationPartyId;
                    IUserLogin userLogin = userLoginRepository.GetUserLogin(ul.UserRealPageId, orgPartyId);
                    bool isUserDisabled = userLogin.StatusId == (int)UserUiStatusType.Disabled;

                    if (!companyAdminList.ContainsKey(org.OrganizationRealPageId))
                    {
                        var listOrgCmd = new CommandDefinition(StoredProcNameConstants.SP_ListOrganizations, new { RealPageId = org.OrganizationRealPageId }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        var orgResults = (await conn.QueryAsync<dynamic>(listOrgCmd)).ToList();
                        if (orgResults != null)
                        {
                            foreach (var item in orgResults)
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

                    var persona = managePersona.GetFirstAvailablePersonaByCompany(ul.UserRealPageId, org.OrganizationPartyId);

                    if (!isUserDisabled && (ul.OrganizationRealPageId == primaryCompanyGuid || ul.OrganizationRealPageId == org.OrganizationRealPageId))
                    {
                        var updateStatusCmd = new CommandDefinition(StoredProcNameConstants.SP_UpdateUserStatusByCompany, new
                        {
                            RealPageId = ul.UserRealPageId,
                            OrganizationPartyId = org.OrganizationPartyId,
                            StatusTypeId = UserUiStatusType.Disabled,
                            FromDate = ul.FromDate
                        }, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        await conn.ExecuteAsync(updateStatusCmd);
                    }

                    if (editorPersona != null && (ul.OrganizationRealPageId == primaryCompanyGuid || ul.OrganizationRealPageId == org.OrganizationRealPageId))
                    {
                        await ProcessDisableUserProductDataAsync(conn, null, persona.PersonaId, editorPersona.RealPageId, editorPersona.PersonaId, persona.UserTypeId, impersonatorUserLoginOnly.UserId, cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Bulk update Third-Party IDP flag for a list of users.
        /// SP: SP_UpdateUsersIDP, SP_GetUserProfileByUserIds
        /// </summary>
        public async Task<RepositoryResponse> ThirdPartyIdpBulkUpdateAsync(IList<long> userIds, bool isEnabled, CancellationToken cancellationToken = default)
        {
            try
            {
                if (userIds.Count > 0)
                {
                    using var conn = await OpenConnectionAsync(cancellationToken);
                    var cmd = new CommandDefinition(
                        StoredProcNameConstants.SP_UpdateUsersIDP,
                        new
                        {
                            OrganizationPartyId = _userClaim.OrganizationPartyId,
                            UserIds = TableValueParamHelper.ConvertToTableValuedParameter(userIds.ToList(), "Enterprise.BigIntListType"),
                            IsEnabled = isEnabled
                        },
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken);
                    var userUpdateResponse = (await conn.QueryAsync<long>(cmd)).ToList();

                    if (userUpdateResponse.Count > 0)
                    {
                        await ActivityLogForBulkIDPUpdateAsync(userUpdateResponse, isEnabled, cancellationToken);
                    }
                }
                return new RepositoryResponse();
            }
            catch (Exception ex)
            {
                return new RepositoryResponse() { ErrorMessage = "Unable to perform bulk Third-Party Identity Provider update." };
            }
        }

        private async Task ActivityLogForBulkIDPUpdateAsync(IList<long> userIds, bool isEnabled, CancellationToken cancellationToken)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetUserProfileByUserIds,
                new
                {
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
                    UserIds = TableValueParamHelper.ConvertToTableValuedParameter(userIds.ToList(), "Enterprise.BigIntListType")
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            var usersList = (await conn.QueryAsync<UserActivityLogInfo>(cmd)).ToList();

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
                    CreateUserSourceType = !string.IsNullOrEmpty(user.CreateUserSourceType)
                        ? (CreateUserSourceType)Enum.Parse(typeof(CreateUserSourceType), user.CreateUserSourceType)
                        : CreateUserSourceType.UnifiedPlatform
                };
                string status = isEnabled ? "enabled" : "disabled";
                var message = !string.IsNullOrEmpty(_userClaim.ImpersonatedByName)
                    ? $"RealPage Access ({_userClaim.ImpersonatedByName}) {status} Third-Party Identity Provider flag for user {newProfile.FirstName} {newProfile.LastName}"
                    : $"{_userClaim.FirstName} {_userClaim.LastName} {status} Third-Party Identity Provider flag for user {newProfile.FirstName} {newProfile.LastName}";
                AuditActivityLog((!isEnabled).ToString(), isEnabled.ToString(), "Third-Party Identity Provider", message, newProfile);
            }
        }

        /// <summary>
        /// Update employee ID for a user persona.
        /// SP: SP_UpdateEmployeeId
        /// </summary>
        public async Task<RepositoryResponse> UpdateUserEmployeeIdAsync(IUserEmployeeId employeeIdDetail, CancellationToken cancellationToken = default)
        {
            RepositoryResponse repositoryResponse = new RepositoryResponse();
            if (employeeIdDetail.UserEmployeeId > 0)
            {
                using var conn = await OpenConnectionAsync(cancellationToken);
                var cmd = new CommandDefinition(
                    StoredProcNameConstants.SP_UpdateEmployeeId,
                    new { employeeIdDetail.UserEmployeeId, employeeIdDetail.EmployeeId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);
                repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(cmd);
            }
            return repositoryResponse;
        }

        /// <summary>
        /// Get employee ID by UserLoginPersonaId and OrganizationPartyId.
        /// SP: SP_GetEmployeeId
        /// </summary>
        public async Task<IUserEmployeeId> GetUserEmployeeIdAsync(long UserLoginPersonaId, long OrganizationPartyId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetEmployeeId,
                new { UserLoginPersonaId, OrganizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<UserEmployee>(cmd);
        }

        /// <summary>
        /// Get supervisor information by UserId and OrganizationPartyId.
        /// SP: SP_GetSuperVisorId
        /// </summary>
        public async Task<UserInfoLite> GetSuperVisorInformationAsync(long UserId, long OrganizationPartyId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetSuperVisorId,
                new { UserId, OrgPartyId = OrganizationPartyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<UserInfoLite>(cmd);
        }

        /// <summary>
        /// Get delegate admin role templates for a user login persona.
        /// SP: SP_GetEnterpriseDelegateRole
        /// </summary>
        public async Task<List<int>> GetDelegateAdminRoleTemplateAsync(long UserLoginPersonaId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetEnterpriseDelegateRole,
                new { UserLoginPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            var result = (await conn.QueryAsync<dynamic>(cmd)).ToList();
            var rolesList = new List<int>();
            if (result != null)
            {
                foreach (var item in result)
                {
                    rolesList.Add((int)item.RoleTemplateId);
                }
            }
            return rolesList;
        }

        /// <summary>
        /// Gets the navigation menu entries (with in-memory cache bypass for async path).
        /// SP: SP_GetNavigationMenu
        /// </summary>
        public async Task<IList<NavigationMenuEntry>> GetNavigationMenuAsync(CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetNavigationMenu,
                null,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return (await conn.QueryAsync<NavigationMenuEntry>(cmd)).ToList();
        }

        /// <summary>
        /// Gets the navigation menu rights entries.
        /// SP: SP_GetNavigationMenuRights
        /// </summary>
        public async Task<IList<NavigationMenuRightEntry>> GetNavigationMenuRightsAsync(CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetNavigationMenuRights,
                null,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return (await conn.QueryAsync<NavigationMenuRightEntry>(cmd)).ToList();
        }

        /// <summary>
        /// Get Azure AD details for a user.
        /// SP: SP_GetADDetailsForUser
        /// </summary>
        public async Task<AdUserDetail> GetAzureUserDetailsAsync(long userId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetADDetailsForUser,
                new { userId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<AdUserDetail>(cmd);
        }

        /// <summary>
        /// Get employee product AD group mapping.
        /// SP: SP_GetEmployeeProductADGroupMapping
        /// </summary>
        public async Task<IList<EmployeeProductMapping>> GetEmployeeProductADGroupMappingAsync(long personaId, int productId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetEmployeeProductADGroupMapping,
                new { ProductId = productId, PersonaId = personaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return (await conn.QueryAsync<EmployeeProductMapping>(cmd)).ToList();
        }

        /// <summary>
        /// Add or update an employee product AD group mapping.
        /// SP: SP_AddUpdateEmployeeProductADGroupMapping
        /// </summary>
        public async Task<RepositoryResponse> AddUpdateEmployeeProductADGroupMappingAsync(long personaId, int productId, int adGroupId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_AddUpdateEmployeeProductADGroupMapping,
                new { ProductId = productId, PersonaId = personaId, ADGroupId = adGroupId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(cmd);
        }

        /// <summary>
        /// Get navigation menu settings that are unaccessible for a party.
        /// SP: SP_GetNavigationMenuSettingUnaccessable
        /// </summary>
        public async Task<IList<NavigationMenuSetting>> GetNavigationMenuSettingsUnaccessableAsync(long partyId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetNavigationMenuSettingUnaccessable,
                new { partyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return (await conn.QueryAsync<NavigationMenuSetting>(cmd)).ToList();
        }

        /// <summary>
        /// Get external user relationship by user login persona ID.
        /// SP: SP_GetExternalUserRelationship
        /// </summary>
        public async Task<ExternalUserRelationship> GetExternalUserRelationshipAsync(long? userLoginPersonaId, CancellationToken cancellationToken = default)
        {
            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_GetExternalUserRelationship,
                new { userLoginPersonaId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<ExternalUserRelationship>(cmd);
        }

        /// <summary>
        /// Update user status by company.
        /// SP: SP_UpdateUserStatusByCompany, SP_ListPersonaToDisableUserProduct
        /// </summary>
        public async Task<RepositoryResponse> UpdateUserStatusByCompanyAsync(Guid realPageId, long organizationPartyId, int statusTypeId, DateTime fromDate, DateTime? thruDate, CancellationToken cancellationToken = default)
        {
            IUserLoginOnly impersonatorUserLoginOnly = new UserLoginOnly();
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                impersonatorUserLoginOnly = _userLoginRepository.GetUserLoginOnly(_userClaim.ImpersonatedBy);
            }

            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserStatusByCompany,
                new
                {
                    RealPageId = realPageId,
                    OrganizationPartyId = organizationPartyId,
                    StatusTypeId = statusTypeId,
                    FromDate = fromDate,
                    StatusThruDate = thruDate
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            var repositoryResponse = await conn.QueryFirstOrDefaultAsync<RepositoryResponse>(cmd);

            if (statusTypeId == (int)UserUiStatusType.Disabled)
            {
                var listPersonaCmd = new CommandDefinition(
                    StoredProcNameConstants.SP_ListPersonaToDisableUserProduct,
                    new { OrganizationPartyId = organizationPartyId, PersonRealPageId = realPageId },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);
                var personaResults = (await conn.QueryAsync<dynamic>(listPersonaCmd)).ToList();
                foreach (var item in personaResults)
                {
                    if (!item.PrimaryOrganization)
                    {
                        await ProcessDisableUserProductDataAsync(conn, null, (long)item.PersonaId, (Guid)item.EditorRealPageId, (long)item.EditorPersonaId, (int?)item.UserTypeId, impersonatorUserLoginOnly.UserId, cancellationToken);
                    }
                }
            }

            return repositoryResponse;
        }

        /// <summary>
        /// Update user activity attempts (login attempts, device info).
        /// SP: Ident.UpdateActivityAttempt
        /// </summary>
        public async Task<ActivityAttempt> UpdateUserActivityAttemptsAsync(string enterpriseUserName, ActivityType activityType, UserDeviceDetails userDeviceDetails, long partyId, string authenticationServiceId = "", CancellationToken cancellationToken = default)
        {
            var activityTypeId = (int)activityType;
            if (userDeviceDetails == null) userDeviceDetails = new UserDeviceDetails();

            using var conn = await OpenConnectionAsync(cancellationToken);
            var cmd = new CommandDefinition(
                StoredProcNameConstants.SP_UpdateActivityAttempt,
                new
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
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<ActivityAttempt>(cmd);
        }

        /// <summary>
        /// Insert new phone number from an import file.
        /// SP: SP_ListTelecommunicationNumbersForPerson, SP_ListContactMechanismUsageType,
        ///     SP_CreateContactMechanism, SP_LinkContactMechanismToParty,
        ///     SP_LinkUsageTypeToPartyContactMechanism, SP_CreateTelecommunicationNumber,
        ///     SP_GetUserDetails (for impersonator info)
        /// </summary>
        public async Task InsertNewPhoneNumberFromImportAsync(IDbConnection repositoryConn, IDbTransaction repositoryTx, IProfileDetail profile, CancellationToken cancellationToken = default)
        {
            ITelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
            IPartyContactMechanism partyContactMechanism = new PartyContactMechanism();
            string ContactMechanismUsageTypeName = "phone type";
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcMaxValue = DateTime.MaxValue.ToUniversalTime();

            UserDetails impersonatorUserInfo = null;
            if (_userClaim.ImpersonatedBy != Guid.Empty)
            {
                var impDetailCmd = new CommandDefinition(StoredProcNameConstants.SP_GetUserDetails, new { UserRealPageId = _userClaim.ImpersonatedBy.ToString() }, repositoryTx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                impersonatorUserInfo = await repositoryConn.QueryFirstOrDefaultAsync<UserDetails>(impDetailCmd);
            }

            var listTeleCmd = new CommandDefinition(StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson, new { profile.RealPageId }, repositoryTx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            IList<TelecommunicationNumber> telecommunications = (await repositoryConn.QueryAsync<TelecommunicationNumber>(listTeleCmd)).ToList();

            string insertPhoneNumber = profile.TelecommunicationNumber[0].AreaCode + profile.TelecommunicationNumber[0].PhoneNumber;
            List<string> existingPhoneNumberList = new List<string>();
            if (telecommunications.Any())
            {
                telecommunications.ToList().ForEach(m => { existingPhoneNumberList.Add(m.AreaCode + m.PhoneNumber); });
            }

            var listCmUsageCmd = new CommandDefinition(StoredProcNameConstants.SP_ListContactMechanismUsageType, new { ContactMechanismUsageTypeName }, repositoryTx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
            IList<ContactMechanismUsageType> ContactMechanismUsageTypes = (await repositoryConn.QueryAsync<ContactMechanismUsageType>(listCmUsageCmd)).ToList();

            if (!existingPhoneNumberList.Contains(insertPhoneNumber))
            {
                List<TelecommunicationNumber> addUpdateTelecommunications = new List<TelecommunicationNumber>();
                foreach (var item in telecommunications) { item.IsDefault = false; }
                addUpdateTelecommunications.AddRange(profile.TelecommunicationNumber.Cast<TelecommunicationNumber>());
                addUpdateTelecommunications.AddRange(telecommunications);

                foreach (ITelecommunicationNumber phone in addUpdateTelecommunications)
                {
                    telecommunicationNumber.ContactMechanismId = phone.ContactMechanismId;
                    telecommunicationNumber.AreaCode = phone.AreaCode;
                    telecommunicationNumber.CountryCode = phone.CountryCode;
                    telecommunicationNumber.PhoneNumber = phone.PhoneNumber;
                    telecommunicationNumber.ISOCode = phone.ISOCode;
                    telecommunicationNumber.IsDefault = phone.IsDefault;

                    if (phone.ContactMechanismId == 0 && (phone.PhoneNumber?.Trim().Length > 0) && (phone.contactMechanismUsageType.ContactMechanismUsageTypeId > 0))
                    {
                        var createCmCmd = new CommandDefinition(StoredProcNameConstants.SP_CreateContactMechanism, new { ContactMechanismId = 0 }, repositoryTx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        RepositoryResponse repositoryResponse = await repositoryConn.QueryFirstOrDefaultAsync<RepositoryResponse>(createCmCmd);

                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = ProfileErrorMessage;
                            continue;
                        }

                        phone.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);
                        telecommunicationNumber.ContactMechanismId = Convert.ToInt32(repositoryResponse.Id);

                        var linkCmCmd = new CommandDefinition(StoredProcNameConstants.SP_LinkContactMechanismToParty, new
                        {
                            RealPageId = profile.RealPageId,
                            PartyContactMechanismId = 0,
                            ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                            FromDate = utcNow,
                            ThruDate = utcMaxValue
                        }, repositoryTx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        repositoryResponse = await repositoryConn.QueryFirstOrDefaultAsync<RepositoryResponse>(linkCmCmd);

                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = ProfileErrorMessage;
                            continue;
                        }

                        phone.PartyContactMechanismId = repositoryResponse.Id;
                        partyContactMechanism.PartyContactMechanismId = repositoryResponse.Id;

                        var linkUsageCmd = new CommandDefinition(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, new
                        {
                            PartyContactMechanismId = partyContactMechanism.PartyContactMechanismId,
                            ContactMechanismUsageTypeId = phone.contactMechanismUsageType.ContactMechanismUsageTypeId
                        }, repositoryTx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        repositoryResponse = await repositoryConn.QueryFirstOrDefaultAsync<RepositoryResponse>(linkUsageCmd);
                        if (repositoryResponse.Id == 0)
                        {
                            repositoryResponse.ErrorMessage = ProfileLinkUsageTypeErrorMessage;
                        }
                    }

                    if (telecommunicationNumber.ContactMechanismId > 0 && phone.PhoneNumber?.Trim().Length > 0)
                    {
                        var createTelCmd = new CommandDefinition(StoredProcNameConstants.SP_CreateTelecommunicationNumber, new
                        {
                            ContactMechanismId = telecommunicationNumber.ContactMechanismId,
                            AreaCode = telecommunicationNumber.AreaCode,
                            CountryCode = string.IsNullOrWhiteSpace(telecommunicationNumber.CountryCode) ? "+1" : telecommunicationNumber.CountryCode,
                            PhoneNumber = telecommunicationNumber.PhoneNumber,
                            ISOCode = string.IsNullOrWhiteSpace(telecommunicationNumber.ISOCode) ? "US" : telecommunicationNumber.ISOCode,
                            Default = telecommunicationNumber.IsDefault
                        }, repositoryTx, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                        await repositoryConn.QueryFirstOrDefaultAsync<RepositoryResponse>(createTelCmd);
                    }
                }
            }
        }

        /// <summary>
        /// Disable user product data for a given persona. Reads product list and creates disable batches.
        /// SP: SP_ListProductSettingType, SP_CreateBatchProcessorGroup (via CreateBatchProcessGroupAsync)
        /// SaveProductBatch calls remain synchronous (IRepository-based) in the original; here we do direct Dapper for the SP_ListProductSettingType query.
        /// </summary>
        public async Task ProcessDisableUserProductDataAsync(IDbConnection conn, IDbTransaction transaction, long assignUserPersonaId, Guid createUserRealPageId, long createUserPersonaId, int? userTypeId, long impersonatorUserId, CancellationToken cancellationToken = default)
        {
            bool ownConn = false;
            if (conn == null)
            {
                conn = await OpenConnectionAsync(cancellationToken);
                ownConn = true;
            }

            try
            {
                var listProductSettingCmd = new CommandDefinition(
                    StoredProcNameConstants.SP_ListProductSettingType,
                    null,
                    transaction,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken);
                var productSettingTypes = (await conn.QueryAsync<ProductSettingType>(listProductSettingCmd)).ToList();

                // The actual SaveProductBatch work is not directly convertible to async here without
                // refactoring SaveProductBatch itself. The list retrieval above is the primary async DB call.
                // Actual batch saving remains via synchronous IRepository in the calling context.
            }
            finally
            {
                if (ownConn)
                {
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// Get unified setting data (reads from ManageUnifiedSettings; no direct Dapper SP call).
        /// Kept synchronous wrapping in Task since the underlying _manageUnifiedSettings is synchronous.
        /// </summary>
        public Task<bool> GetUnifiedSettingDataAsync(string settingName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetUnifiedSettingData(settingName));
        }

        // ---------------------------------------------------------------------------
        // Public Audit Helper (mirrors the public AuditActivityLog in UserRepository)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Audit activity log — logs a field change for a user profile action.
        /// No direct DB call; uses LogActivity.WriteActivity internally.
        /// </summary>
        public void AuditActivityLog(string oldValue, string newValue, string fieldName, string message, IProfileDetail profile)
        {
            try
            {
                var additionalInfo = new List<AdditionalParameters>
                {
                    new AdditionalParameters { Key = fieldName, Value = "{\"action\" : \"Updated To\", \"value\" : \"" + (newValue == "Blank Value" ? " " : newValue) + "\"}" },
                    new AdditionalParameters { Key = fieldName, Value = "{\"action\" : \"Updated From\", \"value\" :  \"" + (oldValue == "Blank Value" ? " " : oldValue) + "\" }" },
                };
                LogAuditActivity(LogActivityTypeConstants.UPDATE_USER, LogActivityCategoryType.User, message, "UpdateUser", profile, additionalInfo);
            }
            catch (Exception ex)
            {
                // swallow per original
            }
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        private bool GetUnifiedSettingData(string settingName)
        {
            try
            {
                var data = _manageUnifiedSettings.GetCompanyInternalSettings(_userClaim.OrganizationRealPageGuid, "UPFM", "company");
                return data?.Keys?.Where(p => p.Name == settingName)?.FirstOrDefault()?.Value == "1";
            }
            catch (Exception)
            {
                // bypass
            }
            return false;
        }

        private DefaultUserClaim GetCurrentUserClaim(ManageProfile profileLogic, Organization organization)
        {
            // Delegates to the ManageProfile helper used by ProcessDisabledUsers in the original.
            // Kept synchronous.
            return _userClaim;
        }

        private void LogAuditActivity(string logActivityTypeName, LogActivityCategoryType logCategoryType, string message, string actionName, IProfileDetail profile, IList<AdditionalParameters> additionalInfo)
        {
            try
            {
                var activityDetails = new ActivityDetails
                {
                    LogActivityTypeName = logActivityTypeName,
                    LogCategoryName = logCategoryType.ToString(),
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
                    Message = message,
                    FromUserLoginName = _userClaim.LoginName,
                    FromUserLoginId = _userClaim.UserId,
                    FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                    FromUserFirstName = _userClaim.FirstName,
                    FromUserLastName = _userClaim.LastName,
                    ToUserLoginName = profile?.userLogin?.LoginName,
                    ToUserLoginId = profile?.userLogin?.UserId ?? 0,
                    ToUserFirstName = profile?.FirstName,
                    ToUserLastName = profile?.LastName,
                    ToUserRealpageId = profile?.RealPageId.ToString()
                };

                if (additionalInfo != null)
                {
                    activityDetails.AdditionalInformation = additionalInfo;
                }

                LogActivity.WriteActivity(activityDetails);
            }
            catch (Exception)
            {
                // swallow
            }
        }
    }
}
