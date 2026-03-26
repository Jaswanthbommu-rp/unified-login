using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// True-async implementation of <see cref="IManageProfileAsync"/>.
/// Every DB-bound call is awaited via an async repository or logic interface.
/// Sync dependencies (<see cref="IManageTelecommunicationNumber"/>, <see cref="IManageElectronicAddress"/>,<see cref="IManagePartyRole"/>, <see cref="IProfileRepository"/>) are called synchronously
/// until their own async counterparts are available.
/// </summary>
public sealed class ManageProfileAsync : IManageProfileAsync
{
    #region Fields

    private readonly IManagePersonAsync              _personLogic;
    private readonly IManageUserLoginAsync           _userLoginLogic;
    private readonly IManagePartyRelationshipAsync   _partyRelationshipLogic;
    private readonly IManageContactMechanismAsync    _contactMechanismLogic;
    private readonly IManageConfigurationSettingAsync _configSettingLogic;
    private readonly IProfileRepositoryAsync         _profileRepository;
    private readonly IProductRepositoryAsync         _productRepository;
    private readonly IManagePersonaAsync             _personaLogic;
    private readonly IManageCredentialAsync          _credentialLogic;
    // Sync — no async interface available yet; TODO: port to async interfaces
    private readonly IManageTelecommunicationNumberAsync  _telecommLogic;
    private readonly IManageElectronicAddressAsync        _electronicAddressLogic;
    private readonly IManagePartyRoleAsync                _partyRoleLogic;
    private readonly IProfileRepositoryAsync              _profileRepositorySync;
    private readonly IUserClaimsAccessor                _userClaimAccessor;
    private readonly ILogger<ManageProfileAsync>     _logger;
    private readonly IProfileService                 _profileService;
    private static readonly int? ParentPartyRoleTypeId = (int)ParentUserRoleType.UserRole;

    #endregion

    #region Constructor

    public ManageProfileAsync(
        IManagePersonAsync personLogic,
        IManageUserLoginAsync userLoginLogic,
        IManagePartyRelationshipAsync partyRelationshipLogic,
        IManageContactMechanismAsync contactMechanismLogic,
        IManageConfigurationSettingAsync configSettingLogic,
        IProfileRepositoryAsync profileRepository,
        IProductRepositoryAsync productRepository,
        IManagePersonaAsync personaLogic,
        IManageCredentialAsync credentialLogic,
        IManageTelecommunicationNumberAsync telecommLogic,
        IManageElectronicAddressAsync electronicAddressLogic,
        IManagePartyRoleAsync partyRoleLogic,
        IProfileRepositoryAsync profileRepositorySync,
        IUserClaimsAccessor userClaimAccessor,
        ILogger<ManageProfileAsync> logger,
        IProfileService profileService)
    {
        _personLogic   = personLogic   ?? throw new ArgumentNullException(nameof(personLogic));
        _userLoginLogic = userLoginLogic ?? throw new ArgumentNullException(nameof(userLoginLogic));
        _partyRelationshipLogic = partyRelationshipLogic ?? throw new ArgumentNullException(nameof(partyRelationshipLogic));
        _contactMechanismLogic = contactMechanismLogic ?? throw new ArgumentNullException(nameof(contactMechanismLogic));
        _configSettingLogic = configSettingLogic ?? throw new ArgumentNullException(nameof(configSettingLogic));
        _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _personaLogic = personaLogic ?? throw new ArgumentNullException(nameof(personaLogic));
        _credentialLogic = credentialLogic ?? throw new ArgumentNullException(nameof(credentialLogic));
        _telecommLogic = telecommLogic ?? throw new ArgumentNullException(nameof(telecommLogic));
        _electronicAddressLogic = electronicAddressLogic ?? throw new ArgumentNullException(nameof(electronicAddressLogic));
        _partyRoleLogic = partyRoleLogic ?? throw new ArgumentNullException(nameof(partyRoleLogic));
        _profileRepositorySync = profileRepositorySync ?? throw new ArgumentNullException(nameof(profileRepositorySync));
        _userClaimAccessor = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
    }

    #endregion

    #region GetProfileAsync

    /// <inheritdoc/>
    public async Task<IProfile> GetProfileAsync(
        Guid realPageId,
        string contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        var person = await _personLogic.GetPersonAsync(realPageId, cancellationToken);
        if (person is null) return null;

        // Sync — no async telecom/email interfaces yet
        var telecomList = await _telecommLogic.ListTelecommunicationNumberForPersonAsync(realPageId, contactMechanismUsageTypeName, cancellationToken);
        if (telecomList.Count == 0)
            telecomList.Add(new TelecommunicationNumber { contactMechanismUsageType = new ContactMechanismUsageType() });

        var emailList     = await _electronicAddressLogic.ListElectronicAddressForPersonAsync(realPageId, string.Empty, cancellationToken);
        bool hasSecondary = false;

        if (emailList.Count == 0)
        {
            emailList.Add(CreateDefaultSecondaryEmail());
        }
        else
        {
            foreach (var item in emailList)
            {
                if (item.ContactMechanismUsageTypeId == 302)
                    hasSecondary = true;
            }
            if (!hasSecondary)
                emailList.Add(CreateDefaultSecondaryEmail());
        }

        // Sync — no async party role interface yet
        var partyRole = await _partyRoleLogic.GetPartyRoleAsync(realPageId, cancellationToken);

        var userLogin = await _userLoginLogic.GetUserLoginAsync(realPageId, _userClaimAccessor.Current.OrganizationPartyId, cancellationToken);
        userLogin.LoginNameType = EmailFormatValidation.IsValidEmail(userLogin.LoginName) ? "email" : string.Empty;

        return new Profile
        {
            PartyId                  = person.PartyId,
            RealPageId               = person.RealPageId,
            Title                    = person.Title,
            FirstName                = person.FirstName,
            MiddleName               = person.MiddleName,
            LastName                 = person.LastName,
            Suffix                   = person.Suffix,
            PreferredContactMethodId = person.PreferredContactMethodId,
            PartyRole                = partyRole,
            IsImpersonated           = _userClaimAccessor.Current.ImpersonatedBy != Guid.Empty,
            TelecommunicationNumber  = telecomList,
            EmailContacts            = emailList,
            userLogin                = userLogin
        };
    }

    #endregion

    #region GetProfileDetailOrganizationsAsync

    /// <inheritdoc/>
    public async Task<bool> GetProfileDetailOrganizationsAsync(
        Guid realPageId,
        string roleTypeFrom, string roleTypeTo, string relationshipType, string contactMechanismUsageTypeName,
        CancellationToken cancellationToken = default)
    {
        var person = await _personLogic.GetPersonAsync(realPageId, cancellationToken);
        if (person is null) return false;

        var userLogin     = await _userLoginLogic.GetUserLoginAsync(realPageId, _userClaimAccessor.Current.OrganizationPartyId, cancellationToken);
        var orgList       = await _userLoginLogic.ListOrganizationByEnterpriseUserIdAsync(realPageId, relationshipType, cancellationToken);
        var contactMechs  = await _contactMechanismLogic.ListContactMechanismForPersonAsync(realPageId, contactMechanismUsageTypeName, cancellationToken);

        foreach (var org in orgList)
        {
            var rel = await _partyRelationshipLogic.GetPartyRelationshipAsync(
                realPageId, org.RealPageId, roleTypeFrom, roleTypeTo, org.RelationshipType, cancellationToken);
            if (rel is not null) org.partyRelationship = rel;
        }

        // profileDetail assembled below — the original controller intentionally returns
        // an empty Profile on success rather than surfacing profileDetail (behaviour preserved).
        _ = new ProfileDetail
        {
            PartyId                  = person.PartyId,
            RealPageId               = person.RealPageId,
            Title                    = person.Title,
            FirstName                = person.FirstName,
            MiddleName               = person.MiddleName,
            LastName                 = person.LastName,
            Suffix                   = person.Suffix,
            PreferredContactMethodId = person.PreferredContactMethodId,
            userLogin                = userLogin,
            organization             = orgList,
            contactMechanism         = contactMechs
        };

        return true;
    }

    #endregion

    #region GetProfileDetailAsync

    /// <inheritdoc/>
    public async Task<IProfileDetail> GetProfileDetailAsync(
        Guid realPageId, 
        long orgPartyId, 
        string roleTypeFrom = null, 
        string roleTypeTo = null, 
        string relationshipType = null, 
        string contactMechanismUsageTypeName = null,
        CancellationToken cancellationToken = default)
    {
       // long orgPartyId = _userClaimAccessor.Current.OrganizationPartyId;

        // ── 1. Base data (all truly async) ───────────────────────────────
        var person         = await _personLogic.GetPersonAsync(realPageId, cancellationToken);
        var userLogin      = await _userLoginLogic.GetUserLoginAsync(realPageId, orgPartyId, cancellationToken);
        var userLoginOnly  = await _userLoginLogic.GetUserLoginOnlyAsync(realPageId, cancellationToken);

        var rawOrgList     = await _userLoginLogic.ListOrganizationByEnterpriseUserIdAsync(realPageId, relationshipType, cancellationToken);
        var orgList        = rawOrgList.Where(p => p.PartyId == orgPartyId).ToList();

        var orgSettingList = await _configSettingLogic.ListOrganizationConfigurationSettingAsync(orgPartyId, null, cancellationToken);
        var contactMechs   = await _contactMechanismLogic.ListContactMechanismForPersonAsync(realPageId, contactMechanismUsageTypeName, cancellationToken);

        // ── 2. Party relationships (depends on orgList) ───────────────────
        foreach (var org in orgList)
        {
            var rel = await _partyRelationshipLogic.GetPartyRelationshipAsync(
                realPageId, org.RealPageId, roleTypeFrom, roleTypeTo, relationshipType, cancellationToken);
            if (rel is not null) org.partyRelationship = rel;
        }

        // ── 3. PartyRole (async) ─────────────────────────────────────────
        var partyRole = await _partyRoleLogic.GetPartyRoleAsync(realPageId, cancellationToken);

        // ── 4. Build org settings list ────────────────────────────────────
        var organizationSettings = orgSettingList?
            .Select(s => new OrganizationSetting { Value = s.Value, Name = s.SettingName })
            .ToList() ?? [];

        // ── 5. Assemble base profile detail ───────────────────────────────
        IProfileDetail profileDetail = new ProfileDetail
        {
            PartyId                  = person.PartyId,
            RealPageId               = person.RealPageId,
            Title                    = person.Title,
            FirstName                = person.FirstName,
            MiddleName               = person.MiddleName,
            LastName                 = person.LastName,
            Suffix                   = person.Suffix,
            PreferredContactMethodId = person.PreferredContactMethodId,
            userLogin                = userLogin,
            organization             = orgList,
            contactMechanism         = contactMechs,
            OrganizationSettings     = organizationSettings,
            UserTypeId               = (int)(userLogin?.UserRoleType ?? UserRoleType.User),
            PartyRole                = partyRole
        };

        // Notification email (ContactMechanismUsageTypeId 301 = notification)
        var notificationEmails = contactMechs
            .Where(p => p.contactMechanismUsageType?.ContactMechanismUsageTypeId == 301).ToList();
        if (notificationEmails.Count > 0)
        {
            var noEmailRoles = new List<UserRoleType> { UserRoleType.UserNoEmail };
            if (profileDetail.organization.HasAnyUserRole(noEmailRoles))
                profileDetail.NotificationEmail = notificationEmails[0].AddressString;
        }

        // ── 6. Persona products count ─────────────────────────────────────
        var persona = await _personaLogic.GetFirstAvailablePersonaByCompanyAsync(
            userLoginOnly.RealPageId, orgPartyId, cancellationToken);

        if (persona is not null)
        {
            var personaProducts = await _productRepository.ListProductsByPersonaIdAsync(
                persona.PersonaId, (int)UserUiStatusType.AccountCreationSuccessful, cancellationToken);
            profileDetail.SummaryCount = new SummaryCounts
            {
                TotalAssignedProducts = personaProducts.Count
            };
        }

        // ── 7. Identity provider & verification token ─────────────────────
        var idp = await _credentialLogic.GetIdentityProviderTypeByLoginNameAsync(
            profileDetail.userLogin.LoginName, cancellationToken);
        profileDetail.AuthenticationType = idp.AuthenticationType;

        var userTypes = new List<UserRoleType>
        {
            UserRoleType.SuperUser,
            UserRoleType.User,
            UserRoleType.UserNoEmail,
            UserRoleType.ExternalUser,
            UserRoleType.SDE,
            UserRoleType.RealPageEmployee
        };

        if (idp.IsLocal
            && !profileDetail.userLogin.PasswordModifiedDate.HasValue
            && profileDetail.organization.HasAnyUserRole(userTypes))
        {
            profileDetail.VerificationActivityToken = await _credentialLogic
                .GetNewUserRegistrationVerificationTokenAsync(userLoginOnly.UserId, userLoginOnly.RealPageId, cancellationToken);
            profileDetail.userLogin.IsPending = true;
        }

        // ── 8. Force-reset-password flag ──────────────────────────────────
        if (!profileDetail.organization.Any(p => p.PrimaryOrganization))
        {
            var primaryOrgStatus = await _userLoginLogic.GetUserOrganizationWithStatusAsync(
                userLoginOnly.UserId, userLoginOnly.LastLogin ?? DateTime.MinValue, 0, true, cancellationToken);
            profileDetail.userLogin.IsForceReSetPassword =
                primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.ForceResetPassword;
        }

        if (!idp.IsLocal)
            profileDetail.userLogin.IsExpired = false;

        // ── 9. Password expiration ────────────────────────────────────────
        var passwordExpiration = await _credentialLogic
            .CheckPasswordExpirationAsync(userLoginOnly.UserId, userLoginOnly.RealPageId, cancellationToken);
        if (passwordExpiration is not null)
            profileDetail.PasswordExpirationDetail = passwordExpiration;

        return profileDetail;
    }

    #endregion

    #region ListProfileDetailsAsync

    /// <inheritdoc/>
    public async Task<IList<ProfileDetail>> ListProfileDetailsAsync(
        IDictionary<object, object> globals,
        Guid? organizationRealPageId = null,
        CancellationToken cancellationToken = default)
    {
        if (organizationRealPageId is null || !_userClaimAccessor.Current.RealPageEmployee)
            organizationRealPageId = _userClaimAccessor.Current.OrganizationRealPageGuid;

        RequestParameter dataFilter = new();
        bool isExport = false;

        if (globals.TryGetValue(BaseType.RequestParameter, out var filterObj))
            dataFilter = filterObj as RequestParameter;
        if (globals.ContainsKey("isExport"))
            isExport = true;

        // ── Product ID list (truly async) ─────────────────────────────────
        var orgProductIds = (await _productRepository.GetProductIdsByCompanyAsync(
            _userClaimAccessor.Current.OrganizationRealPageGuid, cancellationToken)).ToList();

        if (orgProductIds.Contains((int)ProductEnum.AssetOptimizer))
        {
            var allProducts = await _productRepository.GetAllProductsAsync(cancellationToken);
            orgProductIds.Remove((int)ProductEnum.AssetOptimizer);
            foreach (var product in allProducts)
            {
                if (ProductEnumHelper.CheckAoProductSupportedByGreenBook(product.BooksProductCode)
                    && !orgProductIds.Contains(product.ProductId))
                    orgProductIds.Add(product.ProductId);
            }
        }

        // ── ListPersons
       return await _profileService.ListPersonsAsync(orgProductIds, organizationRealPageId, ParentPartyRoleTypeId, dataFilter, isExport, cancellationToken);
        
    }

    #endregion

    #region ListPersonsByProductIdAsync

    /// <inheritdoc/>
    public Task<IList<ProductUsers>> ListPersonsByProductIdAsync(
        int productId, Guid? organizationRealPageId = null, long? personaId = null,
        CancellationToken cancellationToken = default)
        => _profileRepository.ListPersonsByProductIdAsync(productId, organizationRealPageId, personaId, cancellationToken);

    #endregion

    #region UpdateProfileAsync

    /// <inheritdoc/>
    public async Task<RepositoryResponse> UpdateProfileAsync(
        Guid realPageId, IProfile profile,  CancellationToken cancellationToken = default)
    {
        if (realPageId == Guid.Empty) throw new ArgumentException("Invalid parameter realPageId.", nameof(realPageId));
        ArgumentNullException.ThrowIfNull(profile);

       return await _profileService.UpdateProfileAsync(realPageId, profile);
    }

    #endregion

    #region GetOrganizationHasProductAssignmentErrorAsync

    /// <inheritdoc/>
    public Task<bool> GetOrganizationHasProductAssignmentErrorAsync(
        long orgPartyId, CancellationToken cancellationToken = default)
        => _profileRepository.GetOrganizationHasAnyProductAssignmentErrorAsync(orgPartyId, cancellationToken);

    #endregion

    #region Private Helpers

    private static ElectronicAddress CreateDefaultSecondaryEmail() => new()
    {
        AddressType   = "Email",
        AddressString = "",
        contactMechanismUsageType = new ContactMechanismUsageType
        {
            ContactMechanismUsageTypeId = 302,
            Name                        = "Secondary Email"
        }
    };

    #endregion
}
