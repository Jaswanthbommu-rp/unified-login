using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper encapsulating the multi-service orchestration
/// previously living inside <c>ProfileController</c> actions.
/// </summary>
public class ManageProfileAsync : IManageProfileAsync
{
    /// <summary>
    /// Assembles a full <see cref="IProfile"/> from Person, TelecommunicationNumber,
    /// ElectronicAddress, PartyRole and UserLogin services.
    /// Returns null when <paramref name="realPageId"/> is not found.
    /// </summary>
    public Task<IProfile> GetProfileAsync(Guid realPageId, string contactMechanismUsageTypeName, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var person = new ManagePerson().GetPerson(realPageId);
        if (person == null)
            return Task.FromResult<IProfile>(null);

        var telecommunicationNumberList = new ManageTelecommunicationNumber()
            .ListTelecommunicationNumberForPerson(realPageId, contactMechanismUsageTypeName);
        if (telecommunicationNumberList.Count == 0)
        {
            telecommunicationNumberList.Add(new TelecommunicationNumber
            {
                contactMechanismUsageType = new ContactMechanismUsageType()
            });
        }

        var electronicAddressList = new ManageElectronicAddress().ListElectronicAddressForPerson(realPageId, string.Empty);
        bool isSecondaryEmail = false;
        if (electronicAddressList.Count == 0)
        {
            electronicAddressList.Add(CreateDefaultSecondaryEmail());
        }
        else
        {
            foreach (var item in electronicAddressList)
            {
                if (item.ContactMechanismUsageTypeId == 302)
                    isSecondaryEmail = true;
            }
        }

        if (!isSecondaryEmail)
            electronicAddressList.Add(CreateDefaultSecondaryEmail());

        var partyRole = new ManagePartyRole().GetPartyRole(realPageId);

        IProfile profile = new Profile
        {
            PartyId = person.PartyId,
            RealPageId = person.RealPageId,
            Title = person.Title,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            Suffix = person.Suffix,
            PreferredContactMethodId = person.PreferredContactMethodId,
            PartyRole = partyRole,
            IsImpersonated = userClaim != null && userClaim.ImpersonatedBy != Guid.Empty,
            TelecommunicationNumber = telecommunicationNumberList,
            EmailContacts = electronicAddressList
        };

        var userLogin = new ManageUserLogin().GetUserLogin(realPageId, userClaim.OrganizationPartyId);
        userLogin.LoginNameType = EmailFormatValidation.IsValidEmail(userLogin.LoginName) ? "email" : "";
        profile.userLogin = userLogin;

        return Task.FromResult(profile);
    }

    /// <summary>
    /// Executes the profile-detail-organisations query chain.
    /// Returns true when the person is found; false when not found.
    /// Note: the original endpoint always returns an empty <c>Profile</c> object on success
    /// (the profileDetail is populated but never surfaced — bug preserved per no-contract-breaking rule).
    /// </summary>
    public Task<bool> GetProfileDetailOrganizationsAsync(Guid realPageId, string roleTypeFrom, string roleTypeTo, string relationshipType, string contactMechanismUsageTypeName, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var person = new ManagePerson().GetPerson(realPageId);
        if (person == null)
            return Task.FromResult(false);

        var userLoginLogic = new ManageUserLogin(userClaim);
        var userLogin = userLoginLogic.GetUserLogin(realPageId, userClaim.OrganizationPartyId);
        var organizationList = userLoginLogic.ListOrganizationByEnterpriseUserId(realPageId, relationshipType);

        var partyRelationshipLogic = new ManagePartyRelationship();
        foreach (var organization in organizationList)
        {
            var partyRelationship = partyRelationshipLogic.GetPartyRelationship(realPageId, organization.RealPageId, roleTypeFrom, roleTypeTo, relationshipType);
            if (partyRelationship != null)
                organization.partyRelationship = partyRelationship;
        }

        var contactMechanismList = new ManageContactMechanism().ListContactMechanismForPerson(realPageId, contactMechanismUsageTypeName);

        // profileDetail is assembled but intentionally not returned — bug preserved per no-contract-breaking rule.
        // The controller returns empty Profile on success, not profileDetail.
        _ = new ProfileDetail
        {
            PartyId = person.PartyId,
            RealPageId = person.RealPageId,
            Title = person.Title,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            Suffix = person.Suffix,
            PreferredContactMethodId = person.PreferredContactMethodId,
            userLogin = userLogin,
            organization = organizationList,
            contactMechanism = contactMechanismList
        };

        return Task.FromResult(true);
    }

    public Task<RepositoryResponse> UpdateProfileAsync(Guid realPageId, IProfile profile, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var result = new ManageProfile(userClaim).UpdateProfile(realPageId, profile);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Assembles and returns the full <see cref="IProfileDetail"/> used by the /profiles/details endpoint.
    /// </summary>
    public Task<IProfileDetail> GetProfileDetailAsync(Guid realPageId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
    {
        var manageUserLogin = new ManageUserLogin(userClaim);
        var profileLogic = new ManageProfile(userClaim);
        var productLogic = new ManageProduct(userClaim);
        var credentialLogic = new ManageCredential(userClaim);
        var personaLogic = new ManagePersona(userClaim);

        var userLoginOnly = manageUserLogin.GetUserLoginOnly(realPageId);
        var profileDetail = profileLogic.GetProfileDetail(realPageId, userClaim.OrganizationPartyId);
        var persona = personaLogic.GetFirstAvailablePersonaByCompany(userLoginOnly.RealPageId, userClaim.OrganizationPartyId);
        var personaProducts = productLogic.GetUserAssignedProductsByPersona(persona);

        profileDetail.SummaryCount = new SummaryCounts
        {
            TotalAssignedProducts = personaProducts.Count
        };

        var identityProviderType = credentialLogic.GetIdentityProviderTypeByLoginName(profileDetail.userLogin.LoginName);
        profileDetail.AuthenticationType = identityProviderType.AuthenticationType;

        var userTypes = new List<UserRoleType>
        {
            UserRoleType.SuperUser,
            UserRoleType.User,
            UserRoleType.UserNoEmail,
            UserRoleType.ExternalUser,
            UserRoleType.SDE,
            UserRoleType.RealPageEmployee
        };

        if (identityProviderType.IsLocal && !profileDetail.userLogin.PasswordModifiedDate.HasValue && profileDetail.organization.HasAnyUserRole(userTypes))
        {
            profileDetail.VerificationActivityToken = credentialLogic.GetNewUserRegistrationVerificationToken(userLoginOnly.UserId, userLoginOnly.RealPageId);
            profileDetail.userLogin.IsPending = true;
        }

        if (!profileDetail.organization.Any(p => p.PrimaryOrganization))
        {
            var primaryOrgStatus = manageUserLogin.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin.Value, 0, true);
            profileDetail.userLogin.IsForceReSetPassword = primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.ForceResetPassword;
        }

        if (!identityProviderType.IsLocal)
        {
            profileDetail.userLogin.IsExpired = false;
        }

        var checkPasswordExpiration = credentialLogic.CheckPasswordExpiration(userLoginOnly.UserId, userLoginOnly.RealPageId);
        if (checkPasswordExpiration != null)
        {
            profileDetail.PasswordExpirationDetail = checkPasswordExpiration;
        }

        return Task.FromResult(profileDetail);
    }

    private static ElectronicAddress CreateDefaultSecondaryEmail() =>
        new ElectronicAddress
        {
            AddressType = "Email",
            AddressString = "",
            contactMechanismUsageType = new ContactMechanismUsageType
            {
                ParentContactMechanismUsageTypeId = 300,
                ContactMechanismUsageTypeId = 302,
                Name = "Email"
            }
        };
}
