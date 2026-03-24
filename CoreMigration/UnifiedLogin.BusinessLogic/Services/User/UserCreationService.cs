using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Audit;
using UnifiedLogin.BusinessLogic.Services.Product;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Batch;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.User;

/// <summary>
/// Handles user creation operations with async patterns.
/// Extracted from 500+ lines in UserRepository.CreateUser.
/// <para>
/// Refactoring changes vs original:
/// <list type="bullet">
///   <item><c>IRepositoryAsync</c> + <c>UnitOfWork</c> replaced by <see cref="IDbConnection"/> + <c>IDbTransaction</c> (Dapper).</item>
///   <item>Sync <c>IUserLoginRepository</c> replaced by <see cref="IUserLoginRepositoryAsync"/>.</item>
///   <item>Sync <c>IOrganizationRepository</c> replaced by <see cref="IOrganizationRepositoryAsync"/>.</item>
///   <item><c>DefaultUserClaim</c> field replaced by <see cref="IUserClaimsAccessor"/> (no stale-claim risk).</item>
///   <item>Unused <c>IManagePersona</c> dependency removed.</item>
///   <item><c>DetermineUserStatus</c> and <c>GetIdentityProviderTypeAsync</c> are now truly async.</item>
///   <item><c>CalculateStatusThruDate</c> unused <c>IRepositoryAsync</c> parameter removed.</item>
/// </list>
/// </para>
/// </summary>
public class UserCreationService : IUserCreationService
{
    #region Fields

    private readonly IDbConnection _db;
    private readonly IUserLoginRepositoryAsync _userLoginRepo;
    private readonly IOrganizationRepositoryAsync _organizationRepo;
    private readonly IProductBatchService _productBatchService;
    private readonly IUserAuditService _auditService;
    private readonly IUserClaimsAccessor _userClaimsAccessor;
    private readonly ILogger<UserCreationService> _logger;

    #endregion

    #region Constructor

    public UserCreationService(
        IDbConnection db,
        IUserLoginRepositoryAsync userLoginRepo,
        IOrganizationRepositoryAsync organizationRepo,
        IProductBatchService productBatchService,
        IUserAuditService auditService,
        IUserClaimsAccessor userClaimsAccessor,
        ILogger<UserCreationService> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _userLoginRepo = userLoginRepo ?? throw new ArgumentNullException(nameof(userLoginRepo));
        _organizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
        _productBatchService = productBatchService ?? throw new ArgumentNullException(nameof(productBatchService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    // Public — CreateUserAsync
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new user with personas and products.
    /// Refactored from UserRepository.CreateUser (500+ lines).
    /// </summary>
    public async Task<CreateUserResponse<IErrorData>> CreateUserAsync(
        ProfileDetail newProfile,
        IList<Persona> persona,
        CancellationToken cancellationToken = default)
    {
        var response = new CreateUserResponse<IErrorData>();

        try
        {
            _logger.LogInformation("Creating user {LoginName} for organisation {OrgRealPageId}",
                newProfile.userLogin.LoginName, newProfile.organization[0].RealPageId);

            // Step 1: Validation
            var validation = await ValidateUserCreationAsync(
                newProfile, newProfile.organization[0].RealPageId, cancellationToken);

            if (!validation.IsValid)
                return CreateErrorResponse("User.Validation.1", validation.ErrorMessage, response);

            // ── Open connection once and wrap everything in a transaction ────
            if (_db.State != ConnectionState.Open) _db.Open();
            using var tx = _db.BeginTransaction();

            try
            {
                // Step 2: Determine if user already exists
                var userLoginOnly = await _userLoginRepo.GetUserLoginOnlyAsync(
                    newProfile.userLogin.LoginName);

                Guid personRealPageId;
                long userId;

                if (userLoginOnly is null)
                {
                    // New user — create Person + UserLogin
                    var personResult = await CreatePersonAsync(tx, newProfile, cancellationToken);
                    if (!personResult.IsSuccess)
                        throw new InvalidOperationException(personResult.ErrorMessage);

                    personRealPageId = personResult.PersonRealPageId;
                    newProfile.RealPageId = personRealPageId;
                    newProfile.PartyId = personResult.PersonPartyId;

                    var loginResult = await CreateUserLoginAsync(
                        tx, newProfile, personRealPageId, cancellationToken);

                    if (!loginResult.IsSuccess)
                        throw new InvalidOperationException(loginResult.ErrorMessage);

                    userId = loginResult.UserId;

                    if (!string.IsNullOrEmpty(newProfile.Password))
                        await UpdateUserLoginPasswordAsync(tx, newProfile, personRealPageId, cancellationToken);

                    await CreateNotificationEmailAsync(tx, newProfile, personRealPageId, cancellationToken);
                }
                else
                {
                    // Existing user — reuse their IDs
                    userId = userLoginOnly.UserId;
                    personRealPageId = userLoginOnly.RealPageId;
                    newProfile.RealPageId = personRealPageId;
                }

                // Step 3: Create UserLoginPersona
                var userLoginPersonaId = await CreateUserLoginPersonaAsync(
                    tx, newProfile, userId, cancellationToken);

                // Step 4: Create Persona
                var personaResult = await CreatePersonaAsync(
                    tx, newProfile, persona, userId, userLoginPersonaId, cancellationToken);

                if (!personaResult.IsSuccess)
                    throw new InvalidOperationException(personaResult.ErrorMessage);

                response.PersonaId = personaResult.PersonaId;

                // Step 5: Link Enterprise Role
                await LinkEnterpriseRoleAsync(tx, newProfile, personaResult.PersonaId, cancellationToken);

                // Step 6: Link Persona to GreenBook Role
                await LinkPersonaToRoleAsync(tx, newProfile, personaResult.PersonaId, cancellationToken);

                // Step 7: Employee ID / Supervisor
                if (!string.IsNullOrEmpty(newProfile.EmployeeId))
                    await CreateEmployeeIdAsync(tx, userLoginPersonaId, newProfile.EmployeeId, cancellationToken);

                if (newProfile.SuperVisorUserId > 0)
                    await CreateSupervisorAsync(tx, userId, newProfile.SuperVisorUserId, cancellationToken);

                // Step 8: Link Person to Organisation
              //TODO  await LinkPersonToOrganizationAsync(tx, newProfile, personRealPageId, cancellationToken);

                // Step 9: Custom Fields
                if (newProfile.CustomFields?.Count > 0)
                    await CreateCustomFieldsAsync(tx, userId, newProfile, userLoginPersonaId, cancellationToken);

                // Step 10: Product Batch (outside transaction — external service calls)
                tx.Commit();

                await _productBatchService.SaveProductDetailsAsync(
                    newProfile.productBatch,
                    _userClaimsAccessor.PersonaId,
                    personaResult.PersonaId,
                    newProfile.organization[0].RealPageId,
                    newProfile.UserTypeId,
                    true,
                    cancellationToken);

                // Step 11: Audit
                await _auditService.LogActivityAsync(
                    LogActivityTypeConstants.CREATE_USER,
                    LogActivityCategoryType.User,
                    "User {0} {1} created successfully by {2}.",
                    newProfile,
                    cancellationToken);

                response.Status = new Status<IErrorData> { Success = true };
                response.UserStatus = "User created successfully.";
                response.UserRealPageGuid = personRealPageId;

                _logger.LogInformation("User {LoginName} created with PersonaId {PersonaId}",
                    newProfile.userLogin.LoginName, personaResult.PersonaId);

                return response;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {LoginName}", newProfile.userLogin.LoginName);
            return CreateErrorResponse("User.CreateUser.24", $"Create User Error: {ex.Message}", response);
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Public — ValidateUserCreationAsync
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Validates user-creation prerequisites.</summary>
    public async Task<ValidationResult> ValidateUserCreationAsync(
        ProfileDetail profile,
        Guid organizationRealPageId,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(profile.FirstName)) errors.Add("First name is required.");
            if (string.IsNullOrWhiteSpace(profile.LastName)) errors.Add("Last name is required.");
            if (profile.organization is null || !profile.organization.Any()) errors.Add("Organisation is required.");
            if (string.IsNullOrWhiteSpace(profile.userLogin?.LoginName)) errors.Add("Login name is required.");
            if (profile.UserTypeId <= 0) errors.Add("Valid user type is required.");

            if (!string.IsNullOrEmpty(profile.NotificationEmail) &&
                !EmailFormatValidation.IsValidEmail(profile.NotificationEmail))
                errors.Add("Invalid notification email format.");

            // FIX: was sync — now uses async overload
            var existingUser = await _userLoginRepo.GetUserLoginOnlyAsync(profile.userLogin.LoginName);

            if (existingUser is not null)
            {
                // FIX: was sync ListOrganizationByLoginName — now async
                var userOrgs = await _userLoginRepo.ListOrganizationByLoginNameAsync(profile.userLogin.LoginName);

                if (userOrgs.Any(x => x.OrganizationPartyId == profile.organization[0].PartyId))
                    errors.Add("Username already exists in this company.");
            }

            return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user creation validation");
            errors.Add($"Validation error: {ex.Message}");
            return new ValidationResult { IsValid = false, Errors = errors };
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Public — GetStarterProfileOptionsAsync
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Get starter profile options.</summary>
    public async Task<StarterProfileOptionsResponse> GetStarterProfileOptionsAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // FIX: was IRepositoryAsync.GetOneAsync — now Dapper directly
            var user = await _db.QuerySingleOrDefaultAsync<SharedObjects.Landing.User>(
                new CommandDefinition(
                    StoredProcNameConstants.SP_GetUserByLoginId,
                    new { loginid = enterpriseUserName },
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            if (user is null)
                throw new InvalidOperationException($"User {enterpriseUserName} not found.");

            return new StarterProfileOptionsResponse
            {
                EnterpriseUserName = user.LoginId,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                StandardJobTitles = GetJobTitles(),
                PhoneTypes = GetPhoneTypes()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting starter profile options for {UserName}", enterpriseUserName);
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Private — transactional write helpers  (all accept IDbTransaction)
    // ════════════════════════════════════════════════════════════════════════

    private async Task<PersonCreationResult> CreatePersonAsync(
        IDbTransaction tx, ProfileDetail profile, CancellationToken ct)
    {
        _logger.LogDebug("Creating person: {FirstName} {LastName}", profile.FirstName, profile.LastName);

        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePerson,
                new
                {
                    Title = profile.Title,
                    FirstName = profile.FirstName,
                    MiddleName = profile.MiddleName,
                    LastName = profile.LastName,
                    Suffix = profile.Suffix,
                    PreferredContactMethodId = 0,
                    RealPageId = Guid.Empty
                },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        return new PersonCreationResult
        {
            IsSuccess = response.Id > 0 && string.IsNullOrEmpty(response.ErrorMessage),
            PersonRealPageId = response.RealPageId,
            PersonPartyId = response.Id,
            ErrorMessage = response.ErrorMessage ?? "Failed to create person."
        };
    }

    private async Task<UserLoginCreationResult> CreateUserLoginAsync(
        IDbTransaction tx, ProfileDetail profile, Guid personRealPageId, CancellationToken ct)
    {
        _logger.LogDebug("Creating user login: {LoginName}", profile.userLogin.LoginName);

        var sourceType = profile.MigratedUser
            ? CreateUserSourceType.MigrationTool.ToString()
            : (profile.CreateUserSourceType?.ToString() ?? CreateUserSourceType.UnifiedPlatform.ToString());

        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserLogin,
                new { RealPageId = personRealPageId, LoginName = profile.userLogin.LoginName, CreateUserSourceType = sourceType },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        return new UserLoginCreationResult
        {
            IsSuccess = response.Id > 0,
            UserId = response.Id,
            ErrorMessage = response.Id == 0 ? "Username already exists!" : (response.ErrorMessage ?? string.Empty)
        };
    }

    private async Task UpdateUserLoginPasswordAsync(
        IDbTransaction tx, ProfileDetail profile, Guid personRealPageId, CancellationToken ct)
    {
        var pwd = profile.Password.PasswordHash();

        await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_UpdateUserLogin,
                new
                {
                    RealPageId = personRealPageId,
                    LoginName = profile.userLogin.LoginName,
                    PasswordHash = pwd.PasswordHash,
                    PasswordSalt = pwd.PasswordSalt,
                    FromDate = profile.userLogin.FromDate ?? DateTime.UtcNow,
                    ThruDate = profile.userLogin.ThruDate ?? new DateTime(9999, 12, 31),
                    PartyId = profile.organization[0].PartyId
                },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    private async Task<long> CreateUserLoginPersonaAsync(
        IDbTransaction tx, ProfileDetail profile, long userId, CancellationToken ct)
    {
        var fromDate = profile.userLogin.FromDate ?? DateTime.UtcNow;

        // FIX: DetermineUserStatus is now async — awaited properly
        var statusTypeId = await DetermineUserStatusAsync(profile, fromDate, ct);
        var statusThruDate = CalculateStatusThruDate(statusTypeId, fromDate);

        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateUserLoginPersona,
                new
                {
                    UserLoginId = userId,
                    StatusTypeId = statusTypeId,
                    OrganizationPartyId = profile.organization[0].PartyId,
                    PrimaryOrganization = true,
                    FromDate = fromDate,
                    ThruDate = profile.userLogin.ThruDate,
                    StatusThruDate = statusThruDate,
                    IsRPEmployee = profile.IsRPEmployee,
                    IsDelegateAdmin = profile.IsDelegateAdmin,
                    IsRealPartner = profile.IsRealPartner
                },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (response.Id == 0)
            throw new InvalidOperationException("Error creating the user login status.");

        return response.Id;
    }

    private async Task<PersonaCreationResult> CreatePersonaAsync(
        IDbTransaction tx, ProfileDetail profile, IList<Persona> persona,
        long userId, long userLoginPersonaId, CancellationToken ct)
    {
        if (persona is null || !persona.Any())
            throw new ArgumentException("User must have at least one persona.", nameof(persona));

        var personaFromUI = persona[0];
        var personaTypeId = DeterminePersonaTypeId(personaFromUI.Name);

        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreatePersona,
                new
                {
                    PersonRealPageId = profile.RealPageId,
                    UserLoginPersonaId = userLoginPersonaId,
                    OrganizationRealPageId = profile.organization[0].RealPageId,
                    PersonaTypeId = personaTypeId,
                    UserId = userId,
                    PersonaEnvironmentTypeId = personaFromUI.PersonaEnvironmentTypeId,
                    FromDate = personaFromUI.FromDate ?? DateTime.UtcNow,
                    ThruDate = personaFromUI.ThruDate,
                    PersonaId = (long?)null
                },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        return new PersonaCreationResult
        {
            IsSuccess = response.Id > 0,
            PersonaId = response.Id,
            ErrorMessage = response.Id == 0 ? "Persona was not created." : string.Empty
        };
    }

    private async Task LinkEnterpriseRoleAsync(
        IDbTransaction tx, ProfileDetail profile, long personaId, CancellationToken ct)
    {
        var enterpriseRole = profile.productBatch?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
        if (enterpriseRole?.InputJson?.RoleList is null || !enterpriseRole.InputJson.RoleList.Any()) return;

        var roleTemplateId = Convert.ToInt32(enterpriseRole.InputJson.RoleList.First());

        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_InsertUpdateRoleTemplateUserMapping,
                new { RoleTemplateId = roleTemplateId, PersonaId = personaId },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (response.Id == 0)
            throw new InvalidOperationException("User not assigned to Enterprise Role.");
    }

    private async Task LinkPersonaToRoleAsync(
        IDbTransaction tx, ProfileDetail profile, long personaId, CancellationToken ct)
    {
        var enterpriseRoles = (await _db.QueryAsync<EnterpriseRole>(
            new CommandDefinition(
                StoredProcNameConstants.SP_SecurityListRolesByRealPageID,
                new { realPageId = profile.organization[0].RealPageId },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))).ToList();

        var greenBookRole = DetermineGreenBookRole(profile, enterpriseRoles);
        if (greenBookRole == 0) return;

        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkPersonaToRole,
                new
                {
                    personaID = personaId,
                    roleID = greenBookRole,
                    CreatedBy = _userClaimsAccessor.UserId,
                    personaPrivilgeID = 0
                },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (response.Id == 0)
            throw new InvalidOperationException("Error associating persona to user role.");
    }

    private async Task CreateEmployeeIdAsync(
        IDbTransaction tx, long userLoginPersonaId, string employeeId, CancellationToken ct)
    {
        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateEmployeeId,
                new { UserLoginPersonaId = userLoginPersonaId, EmployeeId = employeeId },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (response.Id == 0)
            throw new InvalidOperationException($"Error creating EmployeeId for persona {userLoginPersonaId}.");
    }

    private async Task CreateSupervisorAsync(
        IDbTransaction tx, long userId, long supervisorUserId, CancellationToken ct)
    {
        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_InsertUpdateSuperVisor,
                new { UserId = userId, SuperVisorUserId = supervisorUserId },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (response.Id == 0)
            throw new InvalidOperationException($"Error creating Supervisor for user {userId}.");
    }

    //private async Task LinkPersonToOrganizationAsync(
    //    IDbTransaction tx, ProfileDetail profile, Guid personRealPageId, CancellationToken ct)
    //{
    //    var roleTypeIdFrom = DetermineRoleTypeIdFrom(profile.UserTypeId);

    //    var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
    //        new CommandDefinition(
    //            StoredProcNameConstants.SP_LinkPersonToOrganization,
    //            new
    //            {
    //                PersonRealPageId = personRealPageId,
    //                OrganizationRealPageId = profile.organization[0].RealPageId,
    //                RoleTypeIdFrom = roleTypeIdFrom,
    //                RoleTypeIdTo = (int)UserType.PartyRoleTypeId
    //            },
    //            tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
    //        ?? new RepositoryResponse();

    //    if (response.Id == 0)
    //        throw new InvalidOperationException("Error associating user to user role.");
    //}

    private async Task CreateNotificationEmailAsync(
        IDbTransaction tx, ProfileDetail profile, Guid personRealPageId, CancellationToken ct)
    {
        if (profile.UserTypeId == (int)UserRoleType.UserNoEmail) return;

        var notificationEmail = string.IsNullOrEmpty(profile.NotificationEmail) &&
                                EmailFormatValidation.IsValidEmail(profile.userLogin.LoginName)
            ? profile.userLogin.LoginName
            : profile.NotificationEmail;

        if (string.IsNullOrEmpty(notificationEmail) || !EmailFormatValidation.IsValidEmail(notificationEmail))
            return;

        // Contact Mechanism
        var contactMechResponse = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateContactMechanism,
                new { ContactMechanismId = (long?)null },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (contactMechResponse.Id == 0)
            throw new InvalidOperationException("Error creating contact mechanism for email.");

        var contactMechanismId = contactMechResponse.Id;

        // Link to Party
        var linkResponse = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkContactMechanismToParty,
                new
                {
                    RealPageId = personRealPageId,
                    PartyContactMechanismId = 0,
                    ContactMechanismId = contactMechanismId,
                    FromDate = DateTime.UtcNow,
                    ThruDate = DateTime.MaxValue.ToUniversalTime()
                },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (linkResponse.Id == 0)
            throw new InvalidOperationException("Error linking email to party.");

        // Usage type (301 = Email Notification)
        await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
                new { PartyContactMechanismId = linkResponse.Id, ContactMechanismUsageTypeId = 301 },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));

        // Electronic address
        await _db.ExecuteAsync(
            new CommandDefinition(
                StoredProcNameConstants.SP_CreateElectronicAddress,
                new
                {
                    ContactMechanismId = contactMechanismId,
                    ElectronicAddressString = notificationEmail,
                    ElectronicAddressType = "Email"
                },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct));
    }

    private async Task CreateCustomFieldsAsync(
        IDbTransaction tx, long userId, ProfileDetail profile,
        long userLoginPersonaId, CancellationToken ct)
    {
        profile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = userLoginPersonaId);

        var customFieldsJson = JsonConvert.SerializeObject(profile.CustomFields);

        if (!ValidateJson.IsValidJson<IList<CustomFieldValue>>(customFieldsJson))
        {
            _logger.LogWarning("Invalid custom fields JSON for user {UserId}", userId);
            return;
        }

        var response = await _db.QuerySingleOrDefaultAsync<RepositoryResponse>(
            new CommandDefinition(
                StoredProcNameConstants.SP_AddUpdateFieldValue,
                new { JSON = customFieldsJson, CreatedBy = _userClaimsAccessor.UserId },
                tx, commandType: CommandType.StoredProcedure, cancellationToken: ct))
            ?? new RepositoryResponse();

        if (response.Id == 0 && !string.IsNullOrWhiteSpace(response.ErrorMessage))
            throw new InvalidOperationException("User Custom Fields were not created.");
    }

    // ════════════════════════════════════════════════════════════════════════
    // Private — pure-logic helpers
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// FIX: was sync. Calls <see cref="GetIdentityProviderTypeAsync"/> which hits the DB.
    /// </summary>
    private async Task<int> DetermineUserStatusAsync(
        ProfileDetail profile, DateTime fromDate, CancellationToken ct)
    {
        if (fromDate > DateTime.UtcNow)
            return (int)UserUiStatusType.Disabled;

        var idpType = await GetIdentityProviderTypeAsync(profile, ct);

        if (idpType.IsLocal)
            return profile.userLogin.doNotForceChangePassword
                ? (int)UserUiStatusType.Active
                : (int)UserUiStatusType.Pending;

        return (int)UserUiStatusType.Active;
    }

    /// <summary>
    /// FIX: was sync. Removed unused <c>IRepositoryAsync</c> parameter.
    /// </summary>
    private static DateTime? CalculateStatusThruDate(int statusTypeId, DateTime fromDate)
        => statusTypeId == (int)UserUiStatusType.Pending
            ? fromDate.AddHours(72)   // 72-hour pending window
            : null;

    /// <summary>
    /// FIX: was sync — now async so it doesn't block the thread pool.
    /// Replaces: <c>_organizationRepository.GetOrganizationIdentityProviderType(...)</c>
    /// </summary>
    private async Task<IdentityProviderType> GetIdentityProviderTypeAsync(
     ProfileDetail profile, CancellationToken ct)
    {
        var list = await _organizationRepo.GetOrganizationIdentityProviderTypeAsync(
            profile.organization[0].RealPageId);

        var result = list.FirstOrDefault(a => a.IsLocal == !profile.userLogin.Is3rdPartyIDP)
               ?? list.FirstOrDefault();

        if (result == null)
        {
            throw new InvalidOperationException(
                $"No identity provider type found for organization {profile.organization[0].RealPageId}");
        }

        return result;
    }

    private static int DeterminePersonaTypeId(string personaName)
        => personaName?.ToLowerInvariant() switch
        {
            "primary" => (int)PersonaType.Primary,
            "system administrator" => (int)PersonaType.SuperUser,
            _ => (int)PersonaType.Primary
        };

    private static int DetermineRoleTypeIdFrom(int userTypeId)
        => userTypeId switch
        {
            (int)UserRoleType.User => (int)UserRoleType.User,
            (int)UserRoleType.SuperUser => (int)UserRoleType.SuperUser,
            (int)UserRoleType.RealPageEmployee => (int)UserRoleType.RealPageEmployee,
            (int)UserRoleType.UserNoEmail => (int)UserRoleType.UserNoEmail,
            (int)UserRoleType.ExternalUser => (int)UserRoleType.ExternalUser,
            _ => (int)UserRoleType.User
        };

    private static int DetermineGreenBookRole(ProfileDetail profile, List<EnterpriseRole> enterpriseRoles)
    {
        if (profile.UserTypeId == (int)UserRoleType.SuperUser)
            return enterpriseRoles.FirstOrDefault(r =>
                r.Role.Equals("Platform Admin", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;

        var gbBatch = profile.productBatch?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);
        if (gbBatch?.InputJson?.RoleList?.Any() == true)
            return int.Parse(gbBatch.InputJson.RoleList.First());

        return enterpriseRoles.FirstOrDefault(r =>
            r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
    }

    private static IList<Phone> GetPhoneTypes()
    {
        var phones = new List<Phone>();
        foreach (var en in Enum.GetValues(typeof(PhoneType)))
            phones.Add(new Phone { PhoneTypeId = (int)en, PhoneType = ((Enum)en).ToEnumDescription() });
        return phones;
    }

    private static IList<JobTitle> GetJobTitles()
    {
        var jobTitles = new List<JobTitle>();
        foreach (var en in Enum.GetValues(typeof(JobTitleType)))
            jobTitles.Add(new JobTitle { JobTitleId = (int)en, Name = ((Enum)en).ToEnumDescription() });
        return jobTitles;
    }

    private static CreateUserResponse<IErrorData> CreateErrorResponse(
        string errorCode, string errorMessage, CreateUserResponse<IErrorData> response)
    {
        response.Status = new Status<IErrorData>
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMsg = errorMessage
        };
        response.UserStatus = errorMessage;
        response.UserRealPageGuid = Guid.Empty;
        return response;
    }
}

#region Helper Records

internal record PersonCreationResult
{
    public required bool IsSuccess { get; init; }
    public required Guid PersonRealPageId { get; init; }
    public required long PersonPartyId { get; init; }
    public required string ErrorMessage { get; init; }
}

internal record UserLoginCreationResult
{
    public required bool IsSuccess { get; init; }
    public required long UserId { get; init; }
    public required string ErrorMessage { get; init; }
}

internal record PersonaCreationResult
{
    public required bool IsSuccess { get; init; }
    public required long PersonaId { get; init; }
    public required string ErrorMessage { get; init; }
}

#endregion