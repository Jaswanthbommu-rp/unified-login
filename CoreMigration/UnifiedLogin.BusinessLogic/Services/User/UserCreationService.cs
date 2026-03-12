using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Audit;
using UnifiedLogin.BusinessLogic.Services.Product;
using UnifiedLogin.DataAccess;
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
/// Handles user creation operations with async patterns
/// Extracted from 500+ lines in UserRepository.CreateUser
/// </summary>
public class UserCreationService : IUserCreationService
{
    private readonly IRepositoryAsync _repositoryAsync;
    private readonly IUserLoginRepository _userLoginRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IProductBatchService _productBatchService;
    private readonly IUserAuditService _auditService;
    private readonly IManagePersona _managePersona;
    private readonly DefaultUserClaim _userClaim;
    private readonly ILogger<UserCreationService> _logger;

    public UserCreationService(
        IRepositoryAsync repositoryAsync,
        IUserLoginRepository userLoginRepository,
        IOrganizationRepository organizationRepository,
        IProductBatchService productBatchService,
        IUserAuditService auditService,
        IManagePersona managePersona,
        DefaultUserClaim userClaim,
        ILogger<UserCreationService> logger)
    {
        _repositoryAsync = repositoryAsync ?? throw new ArgumentNullException(nameof(repositoryAsync));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        _productBatchService = productBatchService ?? throw new ArgumentNullException(nameof(productBatchService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        _userClaim = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new user with personas and products (Async)
    /// Refactored from UserRepository.CreateUser (500+ lines)
    /// </summary>
    public async Task<CreateUserResponse<IErrorData>> CreateUserAsync(
        ProfileDetail newProfile,
        IList<Persona> persona,
        CancellationToken cancellationToken = default)
    {
        var response = new CreateUserResponse<IErrorData>();
        var errorStatus = new Status<IErrorData>();

        try
        {
            _logger.LogInformation("Creating user {LoginName} for organization {OrgRealPageId}",
                newProfile.userLogin.LoginName,
                newProfile.organization[0].RealPageId);

            // Step 1: Validation
            var validationResult = await ValidateUserCreationAsync(
                newProfile,
                newProfile.organization[0].RealPageId,
                cancellationToken);

            if (!validationResult.IsValid)
            {
                return CreateErrorResponse("User.Validation.1", validationResult.ErrorMessage, response);
            }

            await using var repo = _repositoryAsync;
            
            // Begin transaction
            repo.UnitOfWork.BeginTransaction();

            try
            {
                // Step 2: Determine if user exists
                var userLoginOnly = _userLoginRepository.GetUserLoginOnly(newProfile.userLogin.LoginName);
                var personRealPageId = Guid.Empty;
                long userId = 0;

                if (userLoginOnly == null)
                {
                    // New user - create Person and UserLogin
                    var personResult = await CreatePersonAsync(repo, newProfile, cancellationToken);
                    if (!personResult.IsSuccess)
                    {
                        throw new InvalidOperationException(personResult.ErrorMessage);
                    }

                    personRealPageId = personResult.PersonRealPageId;
                    newProfile.RealPageId = personRealPageId;
                    newProfile.PartyId = personResult.PersonPartyId;

                    var userLoginResult = await CreateUserLoginAsync(
                        repo,
                        newProfile,
                        personRealPageId,
                        cancellationToken);

                    if (!userLoginResult.IsSuccess)
                    {
                        throw new InvalidOperationException(userLoginResult.ErrorMessage);
                    }

                    userId = userLoginResult.UserId;

                    // Update password if provided
                    if (!string.IsNullOrEmpty(newProfile.Password))
                    {
                        await UpdateUserLoginPasswordAsync(repo, newProfile, personRealPageId, cancellationToken);
                    }

                    // Create notification email if needed
                    await CreateNotificationEmailAsync(repo, newProfile, personRealPageId, cancellationToken);
                }
                else
                {
                    // Existing user - use existing details
                    userId = userLoginOnly.UserId;
                    personRealPageId = userLoginOnly.RealPageId;
                    newProfile.RealPageId = personRealPageId;
                }

                // Step 3: Create UserLoginPersona
                var userLoginPersonaId = await CreateUserLoginPersonaAsync(
                    repo,
                    newProfile,
                    userId,
                    cancellationToken);

                // Step 4: Create Persona
                var personaResult = await CreatePersonaAsync(
                    repo,
                    newProfile,
                    persona,
                    userId,
                    userLoginPersonaId,
                    cancellationToken);

                if (!personaResult.IsSuccess)
                {
                    throw new InvalidOperationException(personaResult.ErrorMessage);
                }

                response.PersonaId = personaResult.PersonaId;

                // Step 5: Link Enterprise Role if provided
                await LinkEnterpriseRoleAsync(repo, newProfile, personaResult.PersonaId, cancellationToken);

                // Step 6: Link Persona to Role (GreenBook role)
                await LinkPersonaToRoleAsync(
                    repo,
                    newProfile,
                    personaResult.PersonaId,
                    cancellationToken);

                // Step 7: Create Employee ID and Supervisor if provided
                if (!string.IsNullOrEmpty(newProfile.EmployeeId))
                {
                    await CreateEmployeeIdAsync(repo, userLoginPersonaId, newProfile.EmployeeId, cancellationToken);
                }

                if (newProfile.SuperVisorUserId > 0)
                {
                    await CreateSupervisorAsync(repo, userId, newProfile.SuperVisorUserId, cancellationToken);
                }

                // Step 8: Link Person to Organization with User Type
                await LinkPersonToOrganizationAsync(
                    repo,
                    newProfile,
                    personRealPageId,
                    cancellationToken);

                // Step 9: Create Custom Fields if provided
                if (newProfile.CustomFields?.Count > 0)
                {
                    await CreateCustomFieldsAsync(repo, userId, newProfile, userLoginPersonaId, cancellationToken);
                }

                // Step 10: Save Product Batch Data
                await _productBatchService.SaveProductDetailsAsync(
                    newProfile.productBatch,
                    _userClaim.PersonaId,
                    personaResult.PersonaId,
                    newProfile.organization[0].RealPageId,
                    newProfile.UserTypeId,
                    true,
                    cancellationToken);

                // Commit transaction
                repo.UnitOfWork.Commit();

                // Step 11: Audit (after successful commit)
                await _auditService.LogActivityAsync(
                    LogActivityTypeConstants.CREATE_USER,
                    LogActivityCategoryType.User,
                    "User {0} {1} created successfully by {2}.",
                    newProfile,
                    cancellationToken);

                response.Status = new Status<IErrorData> { Success = true };
                response.UserStatus = "User created successfully.";
                response.UserRealPageGuid = personRealPageId;

                _logger.LogInformation("User {LoginName} created successfully with PersonaId {PersonaId}",
                    newProfile.userLogin.LoginName, personaResult.PersonaId);

                return response;
            }
            catch (Exception ex)
            {
                repo.UnitOfWork.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {LoginName}", newProfile.userLogin.LoginName);
            
            return CreateErrorResponse(
                "User.CreateUser.24",
                $"Create User Error: {ex.Message}",
                response);
        }
    }

    /// <summary>
    /// Validates user creation prerequisites
    /// </summary>
    public async Task<ValidationResult> ValidateUserCreationAsync(
        ProfileDetail profile,
        Guid organizationRealPageId,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        try
        {
            // Check required fields
            if (string.IsNullOrWhiteSpace(profile.FirstName))
                errors.Add("First name is required.");

            if (string.IsNullOrWhiteSpace(profile.LastName))
                errors.Add("Last name is required.");

            if (profile.organization == null || !profile.organization.Any())
                errors.Add("Organization is required.");

            if (string.IsNullOrWhiteSpace(profile.userLogin?.LoginName))
                errors.Add("Login name is required.");

            // Check if username already exists in this organization
            var existingUser = _userLoginRepository.GetUserLoginOnly(profile.userLogin.LoginName);
            
            if (existingUser != null)
            {
                var userOrgs = _userLoginRepository.ListOrganizationByLoginName(profile.userLogin.LoginName);
                
                if (userOrgs.Any(x => x.OrganizationPartyId == profile.organization[0].PartyId))
                {
                    errors.Add("Username already exists in this company.");
                }
            }

            // Validate email format
            if (!string.IsNullOrEmpty(profile.NotificationEmail) &&
                !EmailFormatValidation.IsValidEmail(profile.NotificationEmail))
            {
                errors.Add("Invalid notification email format.");
            }

            // Validate user type
            if (profile.UserTypeId <= 0)
                errors.Add("Valid user type is required.");

            return new ValidationResult
            {
                IsValid = !errors.Any(),
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user creation validation");
            errors.Add($"Validation error: {ex.Message}");
            
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Get starter profile options (Async)
    /// </summary>
    public async Task<StarterProfileOptionsResponse> GetStarterProfileOptionsAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var repo = _repositoryAsync;
            
            var user = await repo.GetOneAsync<SharedObjects.Landing.User>(
                StoredProcNameConstants.SP_GetUserByLoginId,
                new { loginid = enterpriseUserName },
                cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"User {enterpriseUserName} not found");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting starter profile options for {UserName}", enterpriseUserName);
            throw;
        }
    }

    #region Private Helper Methods

    private async Task<PersonCreationResult> CreatePersonAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating person: {FirstName} {LastName}", profile.FirstName, profile.LastName);

        var param = new
        {
            Title = profile.Title,
            FirstName = profile.FirstName,
            MiddleName = profile.MiddleName,
            LastName = profile.LastName,
            Suffix = profile.Suffix,
            PreferredContactMethodId = 0,
            RealPageId = Guid.Empty
        };

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_CreatePerson,
            param,
            cancellationToken);

        return new PersonCreationResult
        {
            IsSuccess = response.Id > 0 && string.IsNullOrEmpty(response.ErrorMessage),
            PersonRealPageId = response.RealPageId,
            PersonPartyId = response.Id,
            ErrorMessage = response.ErrorMessage ?? "Failed to create person"
        };
    }

    private async Task<UserLoginCreationResult> CreateUserLoginAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        Guid personRealPageId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating user login: {LoginName}", profile.userLogin.LoginName);

        var sourceType = profile.MigratedUser
            ? CreateUserSourceType.MigrationTool.ToString()
            : (profile.CreateUserSourceType?.ToString() ?? CreateUserSourceType.UnifiedPlatform.ToString());

        var param = new
        {
            RealPageId = personRealPageId,
            LoginName = profile.userLogin.LoginName,
            CreateUserSourceType = sourceType
        };

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_CreateUserLogin,
            param,
            cancellationToken);

        return new UserLoginCreationResult
        {
            IsSuccess = response.Id > 0,
            UserId = response.Id,
            ErrorMessage = response.Id == 0 ? "Username already exists!" : response.ErrorMessage
        };
    }

    private async Task UpdateUserLoginPasswordAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        Guid personRealPageId,
        CancellationToken cancellationToken)
    {
        var pwd = profile.Password.PasswordHash();

        var param = new
        {
            RealPageId = personRealPageId,
            LoginName = profile.userLogin.LoginName,
            PasswordHash = pwd.PasswordHash,
            PasswordSalt = pwd.PasswordSalt,
            FromDate = profile.userLogin.FromDate ?? DateTime.UtcNow,
            ThruDate = profile.userLogin.ThruDate ?? new DateTime(9999, 12, 31),
            PartyId = profile.organization[0].PartyId
        };

        await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_UpdateUserLogin,
            param,
            cancellationToken);
    }

    private async Task<long> CreateUserLoginPersonaAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        long userId,
        CancellationToken cancellationToken)
    {
        var fromDate = profile.userLogin.FromDate ?? DateTime.UtcNow;
        var thruDate = profile.userLogin.ThruDate;

        // Determine status
        var statusTypeId = DetermineUserStatus(profile, fromDate);
        var statusThruDate = CalculateStatusThruDate(statusTypeId, fromDate, repo);

        var param = new
        {
            UserLoginId = userId,
            StatusTypeId = statusTypeId,
            OrganizationPartyId = profile.organization[0].PartyId,
            PrimaryOrganization = true,
            FromDate = fromDate,
            ThruDate = thruDate,
            StatusThruDate = statusThruDate,
            IsRPEmployee = profile.IsRPEmployee,
            IsDelegateAdmin = profile.IsDelegateAdmin,
            IsRealPartner = profile.IsRealPartner
        };

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_CreateUserLoginPersona,
            param,
            cancellationToken);

        if (response.Id == 0)
        {
            throw new InvalidOperationException("Error creating the user login status.");
        }

        return response.Id;
    }

    private async Task<PersonaCreationResult> CreatePersonaAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        IList<Persona> persona,
        long userId,
        long userLoginPersonaId,
        CancellationToken cancellationToken)
    {
        if (persona == null || !persona.Any())
        {
            throw new ArgumentException("User must have at least one persona", nameof(persona));
        }

        var personaFromUI = persona[0];
        var personaTypeId = DeterminePersonaTypeId(personaFromUI.Name);

        var param = new
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
        };

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_CreatePersona,
            param,
            cancellationToken);

        return new PersonaCreationResult
        {
            IsSuccess = response.Id > 0,
            PersonaId = response.Id,
            ErrorMessage = response.Id == 0 ? "Persona was not created." : string.Empty
        };
    }

    private async Task LinkEnterpriseRoleAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        long personaId,
        CancellationToken cancellationToken)
    {
        var enterpriseRole = profile.productBatch?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedUI);
        
        if (enterpriseRole?.InputJson?.RoleList != null && enterpriseRole.InputJson.RoleList.Any())
        {
            var roleTemplateId = Convert.ToInt32(enterpriseRole.InputJson.RoleList.First());

            var param = new
            {
                RoleTemplateId = roleTemplateId,
                PersonaId = personaId
            };

            var response = await repo.GetOneAsync<RepositoryResponse>(
                StoredProcNameConstants.SP_InsertUpdateRoleTemplateUserMapping,
                param,
                cancellationToken);

            if (response.Id == 0)
            {
                throw new InvalidOperationException("User not assigned to Enterprise Role.");
            }
        }
    }

    private async Task LinkPersonaToRoleAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        long personaId,
        CancellationToken cancellationToken)
    {
        // Get enterprise roles for organization
        var enterpriseRoles = await repo.GetManyAsync<EnterpriseRole>(
            StoredProcNameConstants.SP_SecurityListRolesByRealPageID,
            new { realPageId = profile.organization[0].RealPageId },
            cancellationToken);

        var roleList = enterpriseRoles?.ToList() ?? new List<EnterpriseRole>();
        var greenBookRole = DetermineGreenBookRole(profile, roleList);

        if (greenBookRole > 0)
        {
            var param = new
            {
                personaID = personaId,
                roleID = greenBookRole,
                CreatedBy = _userClaim.UserId,
                personaPrivilgeID = 0
            };

            var response = await repo.GetOneAsync<RepositoryResponse>(
                StoredProcNameConstants.SP_LinkPersonaToRole,
                param,
                cancellationToken);

            if (response.Id == 0)
            {
                throw new InvalidOperationException("Error associating persona to user role.");
            }
        }
    }

    private async Task CreateEmployeeIdAsync(
        IRepositoryAsync repo,
        long userLoginPersonaId,
        string employeeId,
        CancellationToken cancellationToken)
    {
        var param = new
        {
            UserLoginPersonaId = userLoginPersonaId,
            EmployeeId = employeeId
        };

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_CreateEmployeeId,
            param,
            cancellationToken);

        if (response.Id == 0)
        {
            throw new InvalidOperationException($"Error creating EmployeeId for persona {userLoginPersonaId}");
        }
    }

    private async Task CreateSupervisorAsync(
        IRepositoryAsync repo,
        long userId,
        long supervisorUserId,
        CancellationToken cancellationToken)
    {
        var param = new
        {
            UserId = userId,
            SuperVisorUserId = supervisorUserId
        };

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_InsertUpdateSuperVisor,
            param,
            cancellationToken);

        if (response.Id == 0)
        {
            throw new InvalidOperationException($"Error creating Supervisor for user {userId}");
        }
    }

    private async Task LinkPersonToOrganizationAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        Guid personRealPageId,
        CancellationToken cancellationToken)
    {
        var roleTypeIdFrom = DetermineRoleTypeIdFrom(profile.UserTypeId);

        var param = new
        {
            PersonRealPageId = personRealPageId,
            OrganizationRealPageId = profile.organization[0].RealPageId,
            RoleTypeIdFrom = roleTypeIdFrom,
            RoleTypeIdTo = (int)UserRoleType.UserType // User Type
        };

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_LinkPersonToOrganization,
            param,
            cancellationToken);

        if (response == null || response.Id == 0)
        {
            throw new InvalidOperationException("Error associating user to user role.");
        }
    }

    private async Task CreateNotificationEmailAsync(
        IRepositoryAsync repo,
        ProfileDetail profile,
        Guid personRealPageId,
        CancellationToken cancellationToken)
    {
        if (profile.UserTypeId == (int)UserRoleType.UserNoEmail)
            return;

        var notificationEmail = string.IsNullOrEmpty(profile.NotificationEmail) &&
                                EmailFormatValidation.IsValidEmail(profile.userLogin.LoginName)
            ? profile.userLogin.LoginName
            : profile.NotificationEmail;

        if (string.IsNullOrEmpty(notificationEmail) || !EmailFormatValidation.IsValidEmail(notificationEmail))
            return;

        // Create Contact Mechanism
        var contactMechResponse = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_CreateContactMechanism,
            new { ContactMechanismId = (long?)null },
            cancellationToken);

        if (contactMechResponse.Id == 0)
            throw new InvalidOperationException("Error creating contact mechanism for email");

        var contactMechanismId = contactMechResponse.Id;

        // Link to Party
        var linkResponse = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_LinkContactMechanismToParty,
            new
            {
                RealPageId = personRealPageId,
                PartyContactMechanismId = 0,
                ContactMechanismId = contactMechanismId,
                FromDate = DateTime.UtcNow,
                ThruDate = DateTime.MaxValue.ToUniversalTime()
            },
            cancellationToken);

        if (linkResponse.Id == 0)
            throw new InvalidOperationException("Error linking email to party");

        // Link usage type (Email Notification = 301)
        await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
            new
            {
                PartyContactMechanismId = linkResponse.Id,
                ContactMechanismUsageTypeId = 301 // Email Notification
            },
            cancellationToken);

        // Create electronic address
        await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_CreateElectronicAddress,
            new
            {
                ContactMechanismId = contactMechanismId,
                ElectronicAddressString = notificationEmail,
                ElectronicAddressType = "Email"
            },
            cancellationToken);
    }

    private async Task CreateCustomFieldsAsync(
        IRepositoryAsync repo,
        long userId,
        ProfileDetail profile,
        long userLoginPersonaId,
        CancellationToken cancellationToken)
    {
        profile.CustomFields.ToList().ForEach(c => c.UserLoginPersonaId = userLoginPersonaId);
        
        var customFieldsJson = JsonConvert.SerializeObject(profile.CustomFields);

        if (!ValidateJson.IsValidJson<IList<CustomFieldValue>>(customFieldsJson))
        {
            _logger.LogWarning("Invalid custom fields JSON for user {UserId}", userId);
            return;
        }

        var response = await repo.GetOneAsync<RepositoryResponse>(
            StoredProcNameConstants.SP_AddUpdateFieldValue,
            new
            {
                JSON = customFieldsJson,
                CreatedBy = _userClaim.UserId
            },
            cancellationToken);

        if (response.Id == 0 && !string.IsNullOrWhiteSpace(response.ErrorMessage))
        {
            throw new InvalidOperationException("User Custom Fields were not created.");
        }
    }

    private int DetermineUserStatus(ProfileDetail profile, DateTime fromDate)
    {
        // Determine if user should be Pending or Active
        if (fromDate > DateTime.UtcNow)
            return (int)UserUiStatusType.Disabled;

        var identityProviderType = GetIdentityProviderType(profile);

        if (identityProviderType.IsLocal)
        {
            return profile.userLogin.doNotForceChangePassword
                ? (int)UserUiStatusType.Active
                : (int)UserUiStatusType.Pending;
        }

        return (int)UserUiStatusType.Active;
    }

    private DateTime? CalculateStatusThruDate(int statusTypeId, DateTime fromDate, IRepositoryAsync repo)
    {
        if (statusTypeId != (int)UserUiStatusType.Pending)
            return null;

        // Default to 72 hours
        return fromDate.AddHours(72);
    }

    private int DeterminePersonaTypeId(string personaName)
    {
        return personaName?.ToLowerInvariant() switch
        {
            "primary" => (int)PersonaType.Primary,
            "system administrator" => (int)PersonaType.SuperUser,
            _ => (int)PersonaType.Primary
        };
    }

    private int DetermineRoleTypeIdFrom(int userTypeId)
    {
        return userTypeId switch
        {
            (int)UserRoleType.User => (int)UserRoleType.User,
            (int)UserRoleType.SuperUser => (int)UserRoleType.SuperUser,
            (int)UserRoleType.RealPageEmployee => (int)UserRoleType.RealPageEmployee,
            (int)UserRoleType.UserNoEmail => (int)UserRoleType.UserNoEmail,
            (int)UserRoleType.ExternalUser => (int)UserRoleType.ExternalUser,
            _ => (int)UserRoleType.User
        };
    }

    private int DetermineGreenBookRole(ProfileDetail profile, List<EnterpriseRole> enterpriseRoles)
    {
        var gbProductBatch = profile.productBatch?.FirstOrDefault(p => p.ProductId == (int)ProductEnum.UnifiedPlatform);

        if (profile.UserTypeId == (int)UserRoleType.SuperUser)
        {
            return enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Platform Admin", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
        }

        if (gbProductBatch?.InputJson?.RoleList != null && gbProductBatch.InputJson.RoleList.Any())
        {
            return int.Parse(gbProductBatch.InputJson.RoleList.First());
        }

        // Default role
        return enterpriseRoles.FirstOrDefault(r => r.Role.Equals("Basic End User", StringComparison.OrdinalIgnoreCase))?.RoleId ?? 0;
    }

    private IdentityProviderType GetIdentityProviderType(ProfileDetail profile)
    {
        var identityProviderTypeList = _organizationRepository.GetOrganizationIdentityProviderType(
            profile.organization[0].RealPageId);

        return identityProviderTypeList.FirstOrDefault(a => a.IsLocal == !profile.userLogin.Is3rdPartyIDP) 
               ?? identityProviderTypeList.FirstOrDefault() 
               ?? new IdentityProviderType { IsLocal = true };
    }

    private IList<Phone> GetPhoneTypes()
    {
        var phones = new List<Phone>();
        foreach (var en in Enum.GetValues(typeof(PhoneType)))
        {
            phones.Add(new Phone
            {
                PhoneTypeId = (int)en,
                PhoneType = ((Enum)en).ToEnumDescription()
            });
        }
        return phones;
    }

    private IList<JobTitle> GetJobTitles()
    {
        var jobTitles = new List<JobTitle>();
        foreach (var en in Enum.GetValues(typeof(JobTitleType)))
        {
            jobTitles.Add(new JobTitle
            {
                JobTitleId = (int)en,
                Name = ((Enum)en).ToEnumDescription()
            });
        }
        return jobTitles;
    }

    private CreateUserResponse<IErrorData> CreateErrorResponse(
        string errorCode,
        string errorMessage,
        CreateUserResponse<IErrorData> response)
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

    #endregion
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