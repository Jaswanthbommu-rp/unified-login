using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Enterprise;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.ResponseObject;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Enterprise;

/// <summary>
/// Async-first enterprise user management service.
/// <para>
/// Replaces <c>UserManagement</c> which accepted <c>DefaultUserClaim</c> via constructor and
/// created all repositories with <c>new Xxx(_userClaims)</c> inline.
/// All dependencies are DI-injected; caller identity flows through
/// <see cref="IUserClaimsAccessor"/> so no per-request <c>DefaultUserClaim</c> is needed.
/// </para>
/// <para>
/// <b>DI registration:</b> register as <c>Scoped</c>.
/// </para>
/// </summary>
public sealed class UserManagementAsync : IUserManagementAsync
{
    #region Fields

    private readonly IUserClaimsAccessor                    _userClaims;
    private readonly IEntUserRepositoryAsync                _entUserRepository;
    private readonly IManageUserLoginAsync                  _manageUserLogin;
    private readonly IProductRepositoryAsync                _productRepository;
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepository;
    private readonly ICustomFieldsRepositoryAsync           _customFieldsRepository;
    private readonly IUserRepositoryAsync                   _userRepository;
    private readonly IUserLoginPersonaRepositoryAsync       _userLoginPersonaRepository;
    private readonly IManageOrganizationAsync               _manageOrganization;
    private readonly IManageCredentialAsync                 _manageCredential;
    private readonly IManagePersonaAsync                    _managePersona;
    private readonly IManageProfileAsync                    _manageProfile;
    private readonly IManageProductOpsAsync                 _manageProductOps;
    private readonly IManageUserAsync                       _manageUser;
    private readonly IManageUserRegistrationEmailAsync      _manageUserRegistrationEmail;
    private readonly ILogger<UserManagementAsync>           _logger;

    #endregion

    #region Constructor

    public UserManagementAsync(
        IUserClaimsAccessor                    userClaims,
        IEntUserRepositoryAsync                entUserRepository,
        IManageUserLoginAsync                  manageUserLogin,
        IProductRepositoryAsync                productRepository,
        IProductInternalSettingRepositoryAsync productInternalSettingRepository,
        ICustomFieldsRepositoryAsync           customFieldsRepository,
        IUserRepositoryAsync                   userRepository,
        IUserLoginPersonaRepositoryAsync       userLoginPersonaRepository,
        IManageOrganizationAsync               manageOrganization,
        IManageCredentialAsync                 manageCredential,
        IManagePersonaAsync                    managePersona,
        IManageProfileAsync                    manageProfile,
        IManageProductOpsAsync                 manageProductOps,
        IManageUserAsync                       manageUser,
        IManageUserRegistrationEmailAsync      manageUserRegistrationEmail,
        ILogger<UserManagementAsync>           logger)
    {
        _userClaims                      = userClaims                      ?? throw new ArgumentNullException(nameof(userClaims));
        _entUserRepository               = entUserRepository               ?? throw new ArgumentNullException(nameof(entUserRepository));
        _manageUserLogin                 = manageUserLogin                 ?? throw new ArgumentNullException(nameof(manageUserLogin));
        _productRepository               = productRepository               ?? throw new ArgumentNullException(nameof(productRepository));
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
        _customFieldsRepository          = customFieldsRepository          ?? throw new ArgumentNullException(nameof(customFieldsRepository));
        _userRepository                  = userRepository                  ?? throw new ArgumentNullException(nameof(userRepository));
        _userLoginPersonaRepository      = userLoginPersonaRepository      ?? throw new ArgumentNullException(nameof(userLoginPersonaRepository));
        _manageOrganization              = manageOrganization              ?? throw new ArgumentNullException(nameof(manageOrganization));
        _manageCredential                = manageCredential                ?? throw new ArgumentNullException(nameof(manageCredential));
        _managePersona                   = managePersona                   ?? throw new ArgumentNullException(nameof(managePersona));
        _manageProfile                   = manageProfile                   ?? throw new ArgumentNullException(nameof(manageProfile));
        _manageProductOps                = manageProductOps                ?? throw new ArgumentNullException(nameof(manageProductOps));
        _manageUser                      = manageUser                      ?? throw new ArgumentNullException(nameof(manageUser));
        _manageUserRegistrationEmail     = manageUserRegistrationEmail     ?? throw new ArgumentNullException(nameof(manageUserRegistrationEmail));
        _logger                          = logger                          ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public async Task<ObjectResponse> CreateEnterpriseUnityUserAsync(
        UserProductDetails userProductDetails,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userProductDetails);
        _logger.LogDebug("CreateEnterpriseUnityUserAsync starting for login {LoginName}",
            userProductDetails.UserProfileDetails.LoginName);

        var response = new ObjectResponse();

        // ── 1. Common field validations ────────────────────────────────────
        var validationError = await ValidateUserProductDetailsAsync(userProductDetails, cancellationToken)
            .ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationError))
            return response.WithError(validationError);

        // ── 2. Login-name duplicate check ──────────────────────────────────
        var userOrganizationExists = await _manageUserLogin.IsLoginNameExistsAsync(
            userProductDetails.UserProfileDetails.LoginName,
            userProductDetails.UserProfileDetails.OrganizationRealPageId,
            Guid.Empty,
            userProductDetails.UserProfileDetails.FirstName,
            userProductDetails.UserProfileDetails.LastName,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (userOrganizationExists.UserExists)
            return response.WithError("User Login Name already exists.");

        if (userOrganizationExists.IsValidDomainUsername)
            return response.WithError("Email domain is not allowed.");

        // ── 3. Product role/property validation ────────────────────────────
        var productErrors = await ValidateProductDataAsync(userProductDetails.ProductList, cancellationToken)
            .ConfigureAwait(false);
        if (productErrors.Count > 0)
            return response.WithError(string.Join(",", productErrors));

        // ── 4. Password hash (pure computation — no I/O) ──────────────────
        if (!userProductDetails.UserProfileDetails.IsExternalIdp
            && !(userProductDetails.UserProfileDetails.SendInvitationEmail ?? false))
        {
            var pwd = userProductDetails.UserProfileDetails.Password.PasswordHash();
            userProductDetails.UserProfileDetails.PasswordHash = pwd.PasswordHash;
            userProductDetails.UserProfileDetails.PasswordSalt = pwd.PasswordSalt;
        }

        // ── 5. Custom fields validation ────────────────────────────────────
        var (customFields, customFieldError) = await ValidateAndAssignCustomFieldValuesAsync(
            null,
            userProductDetails.UserProfileDetails.CustomFields,
            cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(customFieldError))
            return response.WithError(customFieldError);

        // ── 6. Trim name fields ────────────────────────────────────────────
        userProductDetails.UserProfileDetails.FirstName  = userProductDetails.UserProfileDetails.FirstName.TrimWhiteSpace();
        userProductDetails.UserProfileDetails.MiddleName = userProductDetails.UserProfileDetails.MiddleName?.TrimWhiteSpace() ?? string.Empty;
        userProductDetails.UserProfileDetails.LastName   = userProductDetails.UserProfileDetails.LastName.TrimWhiteSpace();

        // ── 7. Resolve product codes to IDs for the repository layer ───────
        var productCodeToIdMap = await BuildProductCodeToIdMapAsync(
            userProductDetails.ProductList, cancellationToken).ConfigureAwait(false);

        // ── 8. Create the user in the DB (transactional) ───────────────────
        var userRealPageId = await _entUserRepository.CreateEnterpriseUserAsync(
            userProductDetails, productCodeToIdMap,
            _userClaims.PersonaId, _userClaims.ImpersonatedBy,
            cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("CreateEnterpriseUnityUserAsync: new user RealPageId={UserRealPageId}", userRealPageId);

        // ── 9. Optional invitation email ───────────────────────────────────
        bool isMailNotified = false;
        if (userProductDetails.UserProfileDetails.SendInvitationEmail ?? false)
            isMailNotified = await SendInvitationEmailAsync(new Guid(userRealPageId), cancellationToken)
                .ConfigureAwait(false);

        // ── 10. Resolve new user's details for audit / custom fields ───────
        var newUserDetails = await _userRepository.GetUserDetailsAsync(
            userRealPageId: userRealPageId, cancellationToken: cancellationToken).ConfigureAwait(false);

        var userLoginPersonaList = await _userLoginPersonaRepository.ListUserLoginPersonaAsync(
            null, newUserDetails.UserId,
            userProductDetails.UserProfileDetails.OrganizationPartyId,
            cancellationToken).ConfigureAwait(false);

        // ── 11. Persist custom field values ────────────────────────────────
        if (customFields is { Count: > 0 } && userLoginPersonaList.Count > 0)
        {
            customFields.ToList().ForEach(c =>
                c.UserLoginPersonaId = userLoginPersonaList[0].UserLoginPersonaId);

            string customFieldJson = JsonConvert.SerializeObject(customFields);
            var cfResponse = await _customFieldsRepository.AddUpdateFieldValueAsync(
                customFieldJson, _userClaims.UserId, cancellationToken).ConfigureAwait(false);

            if (cfResponse.Id == 0)
                _logger.LogWarning(
                    "AddUpdateFieldValue returned Id=0 for persona {PersonaId}. Error: {Error}",
                    userLoginPersonaList[0].UserLoginPersonaId, cfResponse.ErrorMessage);
        }

        // ── 12. Audit activity log ─────────────────────────────────────────
        LogAuditActivity(
            LogActivityTypeConstants.CREATE_USER, LogActivityCategoryType.User,
            "New User {0} {1} successfully created by RealPage user {2} using enterprise API.",
            "CreateUser", newUserDetails);

        if ((userProductDetails.UserProfileDetails.SendInvitationEmail ?? false) && isMailNotified)
            LogAuditActivity(
                LogActivityTypeConstants.EMAIL_SENT, LogActivityCategoryType.Email,
                "Welcome Email sent to user {0} {1} by RealPage user {2}.",
                "CreateUser", newUserDetails);

        response.Data = userRealPageId;
        return response;
    }

    /// <inheritdoc/>
    public async Task<ObjectResponse> UpdateEnterpriseUnityUserAsync(
        UserProductDetails userProductDetails,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userProductDetails);
        _logger.LogDebug("UpdateEnterpriseUnityUserAsync starting for login {LoginName}",
            userProductDetails.UserProfileDetails.LoginName);

        var response = new ObjectResponse();

        // ── 1. Validate login name matches the RealPage ID ─────────────────
        bool isValidUsername = await _manageUserLogin.ValidateUsernameAsync(
            userProductDetails.UserProfileDetails.UserRealPageId,
            userProductDetails.UserProfileDetails.LoginName,
            cancellationToken).ConfigureAwait(false);

        if (!isValidUsername)
            return response.WithError("User login name doesn't match with RealPage Id.");

        // ── 2. Common validations ──────────────────────────────────────────
        var validationError = await ValidateUserProductDetailsAsync(userProductDetails, cancellationToken)
            .ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationError))
            return response.WithError(validationError);

        // ── 3. Product validation ──────────────────────────────────────────
        var productErrors = await ValidateProductDataAsync(userProductDetails.ProductList, cancellationToken)
            .ConfigureAwait(false);
        if (productErrors.Count > 0)
            return response.WithError(string.Join(",", productErrors));

        // ── 4. Password hash ───────────────────────────────────────────────
        if (!userProductDetails.UserProfileDetails.IsExternalIdp)
        {
            var pwd = userProductDetails.UserProfileDetails.Password.PasswordHash();
            userProductDetails.UserProfileDetails.PasswordHash = pwd.PasswordHash;
            userProductDetails.UserProfileDetails.PasswordSalt = pwd.PasswordSalt;
        }

        // ── 5. Fetch current user record ───────────────────────────────────
        var updateUserDetails = await _userRepository.GetUserDetailsAsync(
            userRealPageId: userProductDetails.UserProfileDetails.UserRealPageId.ToString(),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var userLoginPersonaList = await _userLoginPersonaRepository.ListUserLoginPersonaAsync(
            null, updateUserDetails.UserId,
            userProductDetails.UserProfileDetails.OrganizationPartyId,
            cancellationToken).ConfigureAwait(false);

        // ── 6. Custom fields ───────────────────────────────────────────────
        long? userLoginPersonaId = userLoginPersonaList.Count > 0
            ? userLoginPersonaList[0].UserLoginPersonaId
            : null;

        var (customFields, customFieldError) = await ValidateAndAssignCustomFieldValuesAsync(
            userLoginPersonaId,
            userProductDetails.UserProfileDetails.CustomFields,
            cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(customFieldError))
            return response.WithError(customFieldError);

        // ── 7. Build profile-detail update object ──────────────────────────
        IProfileDetail updateObject = new ProfileDetail
        {
            CreateUserSourceType = CreateUserSourceType.RPX,
            CustomFields         = customFields,
        };

        updateObject.userLogin.FromDate   = userProductDetails.UserProfileDetails.UserEffectiveDate  ?? updateUserDetails.FromDate;
        updateObject.userLogin.ThruDate   = userProductDetails.UserProfileDetails.UserExpirationDate ?? updateUserDetails.ThruDate;
        updateObject.userLogin.PartyId    = updateUserDetails.PersonPartyId;
        updateObject.userLogin.RealPageId = userProductDetails.UserProfileDetails.UserRealPageId;
        updateObject.userLogin.IsActive   = true;
        updateObject.userLogin.Status     = UserUiStatusType.Active;
        updateObject.userLogin.Password   = userProductDetails.UserProfileDetails.Password;
        updateObject.userLogin.UserRoleType = UserRoleType.User;
        updateObject.userLogin.Is3rdPartyIDP = userProductDetails.UserProfileDetails.IsExternalIdp;
        updateObject.userLogin.UserId     = updateUserDetails.UserId;
        updateObject.userLogin.LoginName  = userProductDetails.UserProfileDetails.LoginName;

        updateObject.organization.Add(new Organization
        {
            RealPageId     = _userClaims.OrganizationRealPageGuid,
            PartyId        = _userClaims.OrganizationPartyId,
            BooksMasterId  = _userClaims.OrganizationMasterId,
            Name           = _userClaims.OrganizationName
        });

        updateObject.Persona =
        [
            new Persona
            {
                Organization = new Organization
                {
                    RealPageId    = _userClaims.OrganizationRealPageGuid,
                    PartyId       = _userClaims.OrganizationPartyId,
                    BooksMasterId = _userClaims.OrganizationMasterId,
                    Name          = _userClaims.OrganizationName
                },
                PersonaTypeId                      = 3,
                PersonaEnvironmentTypeId           = 1,
                hasManageSpendManagementProductAccess = true,
                hasViewOnlySupportToolAccess       = true,
                hasViewOnlySettingsAccess          = true,
                hasImportUsersAccess               = true,
                UserTypeId                         = 401,
                PersonaId                          = updateUserDetails.PersonaId,
                PersonPartyId                      = updateUserDetails.PersonPartyId,
                RealPageId                         = updateUserDetails.UserRealPageId,
                OrganizationPartyId                = _userClaims.OrganizationPartyId,
                Name                               = "Primary",
                UserId                             = updateUserDetails.UserId,
                Role                               = []
            }
        ];

        updateObject.Password          = userProductDetails.UserProfileDetails.Password;
        updateObject.NotificationEmail = userProductDetails.UserProfileDetails.Email;
        updateObject.UserTypeId        = 401;
        updateObject.PartyId           = updateUserDetails.PersonPartyId;
        updateObject.RealPageId        = updateUserDetails.UserRealPageId;
        updateObject.FirstName         = userProductDetails.UserProfileDetails.FirstName.TrimWhiteSpace();
        updateObject.MiddleName        = userProductDetails.UserProfileDetails.MiddleName?.TrimWhiteSpace() ?? string.Empty;
        updateObject.LastName          = userProductDetails.UserProfileDetails.LastName.TrimWhiteSpace();
        updateObject.Title             = userProductDetails.UserProfileDetails.Title;
        updateObject.Suffix            = userProductDetails.UserProfileDetails.Suffix;
        updateObject.EmployeeId        = userProductDetails.UserProfileDetails.EmployeeId;

        updateObject.productBatch = await GetProductBatchDataAsync(userProductDetails.ProductList, cancellationToken)
            .ConfigureAwait(false);

        // ── 8. Persist the update ──────────────────────────────────────────
        var result = await _manageUser.UpdateUserAsync(
            userProductDetails.EditorRealPageId, updateObject, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            _logger.LogWarning("UpdateEnterpriseUnityUserAsync: UpdateUser returned error {Error}",
                result.ErrorMessage);
        }

        response.Data = result.RealPageId.ToString();
        _logger.LogDebug("UpdateEnterpriseUnityUserAsync complete. RealPageId={RealPageId}",
            result.RealPageId);
        return response;
    }

    /// <inheritdoc/>
    public async Task<ObjectResponse> ActivateDeactivateUserAsync(
        Guid unityRealPageUserId,
        UserUiStatusType statusTypeName,
        CancellationToken cancellationToken = default)
    {
        var response = new ObjectResponse();
        _logger.LogDebug("ActivateDeactivateUserAsync: userId={UserId} status={Status}",
            unityRealPageUserId, statusTypeName);

        // ── 1. Verify user exists ──────────────────────────────────────────
        var currentUserLogin = await _manageUserLogin.GetUserLoginOnlyAsync(
            unityRealPageUserId, cancellationToken).ConfigureAwait(false);

        if (currentUserLogin is null || string.IsNullOrEmpty(currentUserLogin.LoginName))
            return response.WithError("Users RealPageUserId is incorrect");

        // ── 2. Update login status ─────────────────────────────────────────
        bool succeeded = await _manageUserLogin.CreateUpdateUserStatusAsync(
            unityRealPageUserId, statusTypeName, cancellationToken).ConfigureAwait(false);

        if (!succeeded)
            return response.WithError("Error while changing user status.");

        // ── 3. Cascade product status update ──────────────────────────────
        await UpdateUserProductStatusAsync(
            [new UserLoginOnly { RealPageId = unityRealPageUserId }],
            statusTypeName,
            cancellationToken).ConfigureAwait(false);

        response.Data = "Success";
        return response;
    }

    /// <inheritdoc/>
    public async Task<IList<UsersData>> ListUsersAsync(
        long organizationPartyId,
        int statusTypeId,
        Guid? realPageId = null,
        string? name = null,
        int rowsPerPage = 0,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        // Filter to relevant products for enterprise API (OPS + UnifiedPlatform)
        var productIdList = await _productRepository.GetProductIdsByCompanyAsync(
            organizationPartyId, cancellationToken).ConfigureAwait(false);

        var filterProductIdList = new List<int>();
        if (productIdList.Any(p => p == (int)ProductEnum.OpsBuyer))
            filterProductIdList.Add((int)ProductEnum.OpsBuyer);
        filterProductIdList.Add((int)ProductEnum.UnifiedPlatform);

        return await _entUserRepository.ListUsersAsync(
            organizationPartyId, filterProductIdList, statusTypeId,
            realPageId, name, rowsPerPage, pageNumber, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IList<UserProductDetailLogin>> ListUserProductDetailsLoginByPersonaIdAsync(
        long personaId,
        CancellationToken cancellationToken = default)
    {
        var attributes = await _entUserRepository.ListUserProductDetailsLoginByPersonaIdAsync(
            personaId, cancellationToken).ConfigureAwait(false);

        return attributes.Select(u => new UserProductDetailLogin
        {
            ProductId   = u.ProductId,
            ProductCode = u.ProductCode,
            Details     = JsonConvert.DeserializeObject<IList<Dictionary<string, string>>>(u.UserAttribute)
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<UserProductDetailLogin>> ListUserProductDetailsLoginByLoginNameAsync(
        string loginName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ListUserProductDetailsLoginByLoginNameAsync: {LoginName}", loginName);

        var attributes = await _entUserRepository.ListUserProductDetailsLoginByLoginNameAsync(
            loginName, cancellationToken).ConfigureAwait(false);

        return attributes.Select(u => new UserProductDetailLogin
        {
            ProductId   = u.ProductId,
            ProductCode = u.ProductCode,
            Company     = u.Company,
            RealPageId  = u.RealPageId,
            UserType    = u.UserType switch
            {
                "401" => UserRoleType.User.ToEnumDescription(),
                "402" => UserRoleType.SuperUser.ToEnumDescription(),
                _     => UserRoleType.ExternalUser.ToEnumDescription()
            },
            Details = JsonConvert.DeserializeObject<IList<Dictionary<string, string>>>(u.UserAttribute)
        }).ToList();
    }

    #endregion

    #region Private — Validation helpers

    /// <summary>
    /// Validates common user profile data: password presence, company existence,
    /// external IDP availability, product availability, and password-policy compliance.
    /// Returns an error string or <c>null</c> on success.
    /// </summary>
    private async Task<string?> ValidateUserProductDetailsAsync(
        UserProductDetails userProductDetails,
        CancellationToken cancellationToken)
    {
        var details = userProductDetails.UserProfileDetails;

        // Password required for local IDP without invitation
        if (!details.IsExternalIdp
            && !(details.SendInvitationEmail ?? false)
            && string.IsNullOrWhiteSpace(details.Password))
        {
            return "Password is required.";
        }

        // Company must exist
        var org = await _manageOrganization.GetOrganizationAsync(
            details.OrganizationRealPageId, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (org is null) return "Request has incorrect CompanyId.";

        details.CompanyName = org.Name;
        long organizationPartyId = org.PartyId;

        // External IDP must be configured for the company
        if (details.IsExternalIdp)
        {
            var providers = await _manageOrganization.GetOrganizationIdentityProviderTypeAsync(
                details.OrganizationRealPageId, cancellationToken).ConfigureAwait(false);

            if (!providers.Any(i => !i.AuthenticationType.Equals("LOCAL", StringComparison.OrdinalIgnoreCase)))
                return "Company has no external Identity Provider configuration. Send IsExternalIdp as false.";
        }

        // Each product code must exist and belong to the company
        if (userProductDetails.ProductList?.Count > 0)
        {
            var orgProducts = await _productRepository.GetProductIdsByCompanyAsync(
                details.OrganizationRealPageId, cancellationToken).ConfigureAwait(false);

            if (orgProducts.Count == 0)
                return "Organization has no products assigned.";

            foreach (var product in userProductDetails.ProductList)
            {
                var productMap = (await _productRepository.ListProductsAsync(
                    null, null, null, product.ProductCode.ToUpper(), cancellationToken)
                    .ConfigureAwait(false)).FirstOrDefault();

                if (productMap is null)
                    return $"Product with code {product.ProductCode} is incorrect.";

                if (!orgProducts.Contains(productMap.ProductId))
                    return $"Product with code {product.ProductCode} is not available for the company.";

                if (productMap.ProductId == (int)ProductEnum.OpsBuyer)
                {
                    if (product.PropertiesAssigned is not { Count: 1 })
                        return $"Product with code {product.ProductCode} requires one property.";

                    if (product.RolesAssigned is not { Count: 1 })
                        return $"Product with code {product.ProductCode} requires one role.";

                    if (!int.TryParse(product.PropertiesAssigned[0], out _))
                        return $"Product with code {product.ProductCode} requires integer property Id.";

                    if (!int.TryParse(product.RolesAssigned[0], out _))
                        return $"Product with code {product.ProductCode} requires integer role Id.";
                }
            }
        }

        // Password policy compliance
        if (!(details.SendInvitationEmail ?? false) && !details.IsExternalIdp)
        {
            var pwdResult = await _manageCredential.ValidatePasswordAsync(new ValidatePassword
            {
                PartyId              = organizationPartyId,
                PasswordToValidate   = details.Password,
                EnterpriseUserName   = details.LoginName
            }, cancellationToken).ConfigureAwait(false);

            if (pwdResult.IsError)
                return pwdResult.ErrorReason;
        }

        return null;
    }

    /// <summary>
    /// Validates product roles/properties and returns any error messages.
    /// Uses <see cref="IManageProductOpsAsync"/> for OPS-specific property/role checks.
    /// </summary>
    private async Task<List<string>> ValidateProductDataAsync(
        IList<ProductDetail>? productList,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (productList is null or { Count: 0 }) return errors;

        // Resolve shared product mappings (e.g., LeadAnalytics → ClickPay)
        productList = await GetProductSharedWithOtherProductAsync(productList, cancellationToken)
            .ConfigureAwait(false);

        foreach (var product in productList)
        {
            var productMap = (await _productRepository.ListProductsAsync(
                null, null, null, product.ProductCode.ToUpper(), cancellationToken)
                .ConfigureAwait(false)).FirstOrDefault();

            if (productMap is null)
            {
                errors.Add($"Product with code {product.ProductCode} is incorrect.");
                continue;
            }

            if (productMap.ProductId != (int)ProductEnum.OpsBuyer) continue;

            string? propertyId = product.PropertiesAssigned?.FirstOrDefault();
            string? roleId     = product.RolesAssigned?.FirstOrDefault();

            // Validate property exists
            var assetsResponse = await _manageProductOps.GetCompanyAssetsAsync(
                _userClaims.PersonaId, 0, false, null!, cancellationToken).ConfigureAwait(false);

            if (!assetsResponse.IsError)
            {
                var assets = assetsResponse.Records.Cast<AssetGroup>().ToList();
                if (assets.All(o => o.ID != propertyId))
                    errors.Add($"Product with code {product.ProductCode} has invalid property Id - {propertyId}");
            }
            else
            {
                errors.Add($"Product with code {product.ProductCode} has invalid property Id - {propertyId}");
            }

            // Validate role exists
            var rolesResponse = await _manageProductOps.GetRolesAsync(
                _userClaims.PersonaId, 0, string.Empty, null!, cancellationToken).ConfigureAwait(false);

            if (!rolesResponse.IsError)
            {
                var roles = rolesResponse.Records.Cast<SharedObjects.Product.ProductRole>().ToList();
                if (roles.All(r => r.ID != roleId))
                    errors.Add($"Product with code {product.ProductCode} has invalid role Id - {roleId}");
            }
            else
            {
                errors.Add($"Product with code {product.ProductCode} has invalid role Id - {roleId}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Resolves shared-product code mappings (e.g., "LeadAnalytics" → "ClickPay")
    /// using the <c>SharedProductId</c> product internal setting.
    /// </summary>
    private async Task<IList<ProductDetail>> GetProductSharedWithOtherProductAsync(
        IList<ProductDetail> productList,
        CancellationToken cancellationToken)
    {
        // Fetch the shared-product setting for all products
        var sharedSettings = await _productInternalSettingRepository.GetProductSettingByTypeAsync(
            SettingConstants.SharedProductSettingName, cancellationToken).ConfigureAwait(false);

        // Get all products for name resolution
        var allProducts = (await _productRepository.GetAllProductsAsync(cancellationToken)
            .ConfigureAwait(false)).ToList();

        // Get only organisation products for source-code matching
        var orgProducts = (await _productRepository.GetProductIdsByCompanyAsync(
            _userClaims.OrganizationPartyId, cancellationToken).ConfigureAwait(false)).ToList();

        var orgProductList = allProducts.Where(p => orgProducts.Contains(p.ProductId)).ToList();

        // Build source→target pairs
        var sharedPairs = sharedSettings
            .Where(s => !string.IsNullOrEmpty(s.Value) && int.TryParse(s.Value, out _))
            .Select(s => (sourceProductId: s.ProductId, targetProductId: int.Parse(s.Value!)))
            .ToList();

        foreach (var (sourceProductId, targetProductId) in sharedPairs)
        {
            var sourceProduct = orgProductList.FirstOrDefault(p => p.ProductId == sourceProductId);
            var targetProduct = allProducts.FirstOrDefault(p => p.ProductId == targetProductId);

            if (sourceProduct is null || targetProduct is null) continue;

            foreach (var pd in productList.Where(p =>
                string.Equals(p.ProductCode, sourceProduct.BooksProductCode, StringComparison.OrdinalIgnoreCase)))
            {
                pd.ProductCode = targetProduct.BooksProductCode;
            }
        }

        return productList;
    }

    /// <summary>
    /// Validates caller-supplied custom fields against the company's configured fields
    /// and returns (resolvedFields, errorMessage). <c>resolvedFields</c> is <c>null</c>
    /// when the company has no custom fields enabled.
    /// </summary>
    private async Task<(IList<CustomFieldValue>? fields, string error)> ValidateAndAssignCustomFieldValuesAsync(
        long? userLoginPersonaId,
        Dictionary<string, string>? customFields,
        CancellationToken cancellationToken)
    {
        try
        {
            var customFieldValues = await _customFieldsRepository.GetCustomFieldsValuesAsync(
                _userClaims.OrganizationPartyId, userLoginPersonaId, enabled: true, cancellationToken)
                .ConfigureAwait(false);

            if (customFieldValues is not { Count: > 0 })
                return (null, string.Empty);

            foreach (var field in customFieldValues)
            {
                if (field.Required == true
                    && (customFields is null || !customFields.ContainsKey(field.Name)))
                {
                    return (null, $"{field.Name} is required custom field & not provided.");
                }

                KeyValuePair<string, string> kv = default;
                if (customFields is not null)
                    kv = customFields.FirstOrDefault(c =>
                        c.Key.Equals(field.Name, StringComparison.OrdinalIgnoreCase));

                if (field.MaxCharLength > 0 && kv.Value is not null && kv.Key is not null)
                {
                    if (kv.Value.Length > field.MaxCharLength)
                        return (null, $"{field.Name} is required max characters up to {field.MaxCharLength}.");

                    if (kv.Value.Length < field.MinCharLength)
                        return (null, $"{field.Name} is required minimum characters up to {field.MinCharLength}.");
                }

                field.Value = kv.Value;
            }

            return (customFieldValues, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateAndAssignCustomFieldValuesAsync failed");
            return (null, string.Empty);
        }
    }

    #endregion

    #region Private — Product helpers

    /// <summary>
    /// Pre-resolves product codes (upper-case) to product IDs using the repository.
    /// The resulting map is passed to <see cref="IEntUserRepositoryAsync.CreateEnterpriseUserAsync"/>
    /// so the repository stays free of product-lookup dependencies.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, int>> BuildProductCodeToIdMapAsync(
        IList<ProductDetail>? productList,
        CancellationToken cancellationToken)
    {
        if (productList is null or { Count: 0 })
            return new Dictionary<string, int>();

        var map = new Dictionary<string, int>(productList.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var product in productList)
        {
            string code = product.ProductCode.ToUpperInvariant();
            if (map.ContainsKey(code)) continue;

            var match = (await _productRepository.ListProductsAsync(
                null, null, null, code, cancellationToken).ConfigureAwait(false))
                .FirstOrDefault();

            map[code] = match?.ProductId ?? 0;
        }

        return map;
    }

    /// <summary>
    /// Builds a <see cref="IList{ProductBatch}"/> from the caller-supplied product list,
    /// resolving each product code to its internal ID via the async product repository.
    /// </summary>
    private async Task<IList<ProductBatch>> GetProductBatchDataAsync(
        IList<ProductDetail>? productList,
        CancellationToken cancellationToken)
    {
        var batch = new List<ProductBatch>();
        if (productList is null) return batch;

        foreach (var product in productList)
        {
            var productMap = (await _productRepository.ListProductsAsync(
                null, null, null, product.ProductCode.ToUpperInvariant(), cancellationToken)
                .ConfigureAwait(false)).FirstOrDefault();

            if (productMap is null) continue;

            batch.Add(new ProductBatch
            {
                ProductId  = productMap.ProductId,
                StatusTypeId = 5,
                InputJson  = new RolePropertyList
                {
                    PropertyList = product.PropertiesAssigned,
                    RoleList     = product.RolesAssigned,
                    RegionList   = product.RegionsAssigned,
                    IsAssigned   = product.IsAssigned
                }
            });
        }

        return batch;
    }

    #endregion

    #region Private — Side-effect helpers

    /// <summary>Sends a new-user invitation email using the async registration email service.</summary>
    private async Task<bool> SendInvitationEmailAsync(Guid userRealPageId, CancellationToken cancellationToken)
    {
        try
        {
            var profileDetail = await _manageProfile.GetProfileDetailAsync(
                userRealPageId, _userClaims.OrganizationPartyId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return await _manageUserRegistrationEmail.SendNewUserRegistrationEmailAsync(
                profileDetail, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendInvitationEmailAsync failed for {UserRealPageId}", userRealPageId);
            return false;
        }
    }

    /// <summary>
    /// Cascades a status change across all active product assignments for the given users.
    /// </summary>
    private async Task UpdateUserProductStatusAsync(
        IList<UserLoginOnly> userLogins,
        UserUiStatusType userLoginStatusType,
        CancellationToken cancellationToken)
    {
        var persona = await _managePersona.GetFirstAvailablePersonaByCompanyAsync(
            _userClaims.UserRealPageGuid, _userClaims.OrganizationPartyId, cancellationToken)
            .ConfigureAwait(false);

        if (persona is null)
        {
            _logger.LogWarning("UpdateUserProductStatusAsync: no persona found for {UserRealPageGuid}",
                _userClaims.UserRealPageGuid);
            return;
        }

        await _manageUser.UpdateUserStatusAsync(
            _userClaims.UserRealPageGuid, persona.PersonaId, userLogins, userLoginStatusType,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Writes a structured audit activity entry via the static <c>LogActivity</c> helper.</summary>
    private void LogAuditActivity(
        string logActivityType,
        LogActivityCategoryType logActivityCategoryType,
        string message,
        string stepName,
        UserDetails userDetails)
    {
        string actor = string.IsNullOrEmpty(_userClaims.ImpersonatedByName)
            ? $"{_userClaims.FirstName} {_userClaims.LastName}"
            : $" RealPage Access ({_userClaims.ImpersonatedByName}) ";
        try
        {
            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName      = logActivityType,
                LogCategoryName          = logActivityCategoryType.ToString(),
                CorrelationId            = _userClaims.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaims.OrganizationMasterId,
                OrganizationPartyId      = _userClaims.OrganizationPartyId,
                Message                  = string.Format(message, userDetails.FirstName, userDetails.LastName, actor),
                FromUserLoginName        = _userClaims.LoginName,
                FromUserLoginId          = _userClaims.UserId,
                FromUserRealpageId       = _userClaims.UserRealPageGuid.ToString(),
                FromUserFirstName        = _userClaims.FirstName,
                FromUserLastName         = _userClaims.LastName,
                ToUserLoginName          = userDetails.LoginName,
                ToUserLoginId            = userDetails.UserId,
                ToUserFirstName          = userDetails.FirstName,
                ToUserLastName           = userDetails.LastName,
                ToUserRealpageId         = userDetails.UserRealPageId.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "LogAuditActivity failed for type={ActivityType} org={Org}",
                logActivityType, _userClaims.OrganizationName);
        }
    }

    #endregion
}

/// <summary>
/// Fluent helper on <see cref="ObjectResponse"/> for concise early-return error paths.
/// </summary>
file static class ObjectResponseExtensions
{
    internal static ObjectResponse WithError(this ObjectResponse response, string reason)
    {
        response.IsError     = true;
        response.ErrorReason = reason;
        return response;
    }
}
