using UnifiedLogin.LandingAPI.Attributes;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Extensions;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Profile Controller to hold (Person, UserLogin, Contact) management related APIs
    /// </summary>
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        #region Private variables
        private readonly IUserClaimsAccessor _userClaimsAccessor;
        IProfileDetail profileDetail = new ProfileDetail();
        IProfile profile = new Profile();
        IRepositoryResponse repositoryResponse = new RepositoryResponse();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="userClaimsAccessor">User claims accessor</param>
        public ProfileController(IUserClaimsAccessor userClaimsAccessor)
        {
            _userClaimsAccessor = userClaimsAccessor;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get a user detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>Profile object</returns>
        [HttpGet("profiles/{realPageId}")]
        [ProducesResponseType(typeof(ObjectOutput<IProfile, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile(Guid realPageId, [FromQuery] string ContactMechanismUsageTypeName = "")
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectOutput<IProfile, IErrorData> output = new ObjectOutput<IProfile, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                IPerson person = new Person();
                IList<TelecommunicationNumber> telecommunicationNumberList = new List<TelecommunicationNumber>();
                PartyRole partyRole = new PartyRole();

                realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
                if (realPageId == Guid.Empty)
                {
                    output.obj = profile;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Profile.GetProfile.1";
                    errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                    output.Status = errorStatus;
                    return BadRequest(output);
                }

                IManagePerson personLogic = new ManagePerson();
                person = personLogic.GetPerson(realPageId);

                if (person != null)
                {
                    IManageTelecommunicationNumber telecommunicationNumberLogic = new ManageTelecommunicationNumber();
                    telecommunicationNumberList = telecommunicationNumberLogic.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName);
                    if (telecommunicationNumberList.Count == 0)
                    {
                        ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType();
                        TelecommunicationNumber teleCommunicationNumber = new TelecommunicationNumber();
                        teleCommunicationNumber.contactMechanismUsageType = contactMechanismUsageType;
                        telecommunicationNumberList.Add(teleCommunicationNumber);
                    }

                    IList<ElectronicAddress> electronicAddressList = new List<ElectronicAddress>();
                    bool isSecondaryEmail = false;
                    var manageElectronicAddress = new ManageElectronicAddress();
                    electronicAddressList = manageElectronicAddress.ListElectronicAddressForPerson(realPageId, string.Empty);
                    if (electronicAddressList.Count == 0)
                    {
                        ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType();
                        contactMechanismUsageType.ParentContactMechanismUsageTypeId = 300;
                        contactMechanismUsageType.ContactMechanismUsageTypeId = 302;
                        contactMechanismUsageType.Name = "Email";
                        ElectronicAddress electronicAdd = new ElectronicAddress();
                        electronicAdd.AddressType = "Email";
                        electronicAdd.AddressString = "";
                        electronicAdd.contactMechanismUsageType = contactMechanismUsageType;
                        electronicAddressList.Add(electronicAdd);
                    }
                    else
                    {

                        foreach (var item in electronicAddressList)
                        {
                            if (item.ContactMechanismUsageTypeId == 302)
                            {
                                isSecondaryEmail = true;
                            }
                        }
                    }

                    if(isSecondaryEmail == false)
                    {
                        ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType();
                        contactMechanismUsageType.ParentContactMechanismUsageTypeId = 300;
                        contactMechanismUsageType.ContactMechanismUsageTypeId = 302;
                        contactMechanismUsageType.Name = "Email";
                        ElectronicAddress electronicAdd = new ElectronicAddress();
                        electronicAdd.AddressType = "Email";
                        electronicAdd.AddressString = "";
                        electronicAdd.contactMechanismUsageType = contactMechanismUsageType;
                        electronicAddressList.Add(electronicAdd);
                    }

                    IManagePartyRole partyRoleLogic = new ManagePartyRole();
                    partyRole = partyRoleLogic.GetPartyRole(realPageId);

                    profile.PartyId = person.PartyId;
                    profile.RealPageId = person.RealPageId;
                    profile.Title = person.Title;
                    profile.FirstName = person.FirstName;
                    profile.MiddleName = person.MiddleName;
                    profile.LastName = person.LastName;
                    profile.Suffix = person.Suffix;
                    profile.PreferredContactMethodId = person.PreferredContactMethodId;
                    profile.PartyRole = partyRole;
                    if (profile != null)
                    {
                        profile.IsImpersonated = false;
                        if (userClaim != null && userClaim.ImpersonatedBy != Guid.Empty)
                        {
                            profile.IsImpersonated = true;
                        }
                    }

                    profile.TelecommunicationNumber = telecommunicationNumberList;
                    profile.EmailContacts = electronicAddressList;

                    //Include the UserLogin details.  IsActive and Is3rdPartyIDP are used by the Edit User
                    IManageUserLogin userLoginLogic = new ManageUserLogin();
                    var userLogin = userLoginLogic.GetUserLogin(realPageId, userClaim.OrganizationPartyId); // keep for now, used by ui, need to investigate how
                    userLogin.LoginNameType = EmailFormatValidation.IsValidEmail(userLogin.LoginName) ? "email" : "";

                    profile.userLogin = userLogin;

                    if (profile != null)
                    {
                        output.obj = profile;
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }

                output.obj = profile;
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Profile.GetProfile.1";
                errorStatus.ErrorMsg = "Invalid realPageId";
                output.Status = errorStatus;
                return BadRequest(output);
            });
        }

        /// <summary>
        /// Get a user Profile detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="roleTypeFrom">Person Role Type name in the Relationship (Optional)</param>
        /// <param name="roleTypeTo">Organization Role Type name in the Relationship (Optional)</param>
        /// <param name="relationshipType">Parties Relationship type name (Optional)</param>
        /// <param name="contactMechanismUsageTypeName">Contact Mechanism UsageType Name (Optional)</param>
        /// <returns>ProfileDetail object</returns>
        [HttpGet("profiles/{realPageId}/organizations")]
        [ProducesResponseType(typeof(ObjectOutput<IProfile, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfileDetail(Guid realPageId, [FromQuery] string roleTypeFrom = null, [FromQuery] string roleTypeTo = null, [FromQuery] string relationshipType = null, [FromQuery] string contactMechanismUsageTypeName = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectOutput<IProfile, IErrorData> output = new ObjectOutput<IProfile, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                IPerson person = new Person();
                IUserLogin userLogin = new UserLogin();
                IList<Organization> organizationList = new List<Organization>();
                IList<CommonAddress> contactMechanismList = new List<CommonAddress>();

                realPageId = (realPageId == Guid.Empty) ? userClaim.UserRealPageGuid : realPageId;
                if (realPageId == Guid.Empty)
                {
                    output.obj = profile;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Profile.GetProfileDetail.1";
                    errorStatus.ErrorMsg = "Invalid parameter: realPageId";
                    errorStatus.ErrorData = null;
                    output.Status = errorStatus;
                    return BadRequest(output);
                }

                IManagePerson personLogic = new ManagePerson();
                person = personLogic.GetPerson(realPageId);

                if (person != null)
                {
                    IManageUserLogin userLoginLogic = new ManageUserLogin(userClaim);
                    userLogin = userLoginLogic.GetUserLogin(realPageId, userClaim.OrganizationPartyId); // keep for now, used by UI

                    //IManageOrganization organizationLogic = new ManageOrganization();
                    //organizationList = organizationLogic.ListOrganizationByEnterpriseUserId(realPageId, relationshipType);
                    organizationList = userLoginLogic.ListOrganizationByEnterpriseUserId(realPageId, relationshipType);

                    PartyRelationship partyRelationship = new PartyRelationship();
                    IManagePartyRelationship partyRelationshipLogic = new ManagePartyRelationship();
                    foreach (var organization in organizationList)
                    {
                        partyRelationship = partyRelationshipLogic.GetPartyRelationship(realPageId, organization.RealPageId, roleTypeFrom, roleTypeTo, relationshipType);
                        if (partyRelationship != null)
                        {
                            organization.partyRelationship = partyRelationship;
                        }
                    }

                    IManageContactMechanism contactMechanismLogic = new ManageContactMechanism();
                    contactMechanismList = contactMechanismLogic.ListContactMechanismForPerson(realPageId, contactMechanismUsageTypeName);

                    profileDetail.PartyId = person.PartyId;
                    profileDetail.RealPageId = person.RealPageId;
                    profileDetail.Title = person.Title;
                    profileDetail.FirstName = person.FirstName;
                    profileDetail.MiddleName = person.MiddleName;
                    profileDetail.LastName = person.LastName;
                    profileDetail.Suffix = person.Suffix;
                    profileDetail.PreferredContactMethodId = person.PreferredContactMethodId;
                    profileDetail.userLogin = userLogin;
                    profileDetail.organization = organizationList;
                    profileDetail.contactMechanism = contactMechanismList;

                    if (profileDetail != null)
                    {
                        output.obj = profile;
                        output.Status = errorStatus;
                        return Ok(output);
                    }
                }
                output.obj = profile;
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Prifile.GetProfileDetail.2";
                errorStatus.ErrorMsg = "Invalid realPageId";
                output.Status = errorStatus;
                return BadRequest(output);
            });
        }

        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">profile object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("profiles/{realPageId}")]
        [AuthorizeRight("editotherprofile", "editownprofile")]
        [ProducesResponseType(typeof(ObjectOutput<IProfile, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile(Guid realPageId, [FromBody] Profile profile)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                ObjectOutput<IProfile, IErrorData> output = new ObjectOutput<IProfile, IErrorData>();
                Status<IErrorData> errorStatus = new Status<IErrorData>();
                output.obj = profile;
                realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? userClaim.UserRealPageGuid : realPageId;
                if ((realPageId == Guid.Empty) || (realPageId == null))
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Profile.UpdateProfile.1";
                    errorStatus.ErrorMsg = "Update Profile: Invalid parameter realPageId";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                if (profile == null)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Profile.UpdateProfile.2";
                    errorStatus.ErrorMsg = "Update Profile: Invalid parameter Profile";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (profile.IsFirstNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Profile.UpdateProfile.4";
                    errorStatus.ErrorMsg = "First name is required.";
                    output.Status = errorStatus;
                    return Ok(output);
                }
                else if (profile.IsLastNameNullOrWhiteSpace)
                {
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Profile.UpdateProfile.5";
                    errorStatus.ErrorMsg = "Last name is required.";
                    output.Status = errorStatus;
                    return Ok(output);
                }

                IManageProfile profileLogic = new ManageProfile(userClaim);
                repositoryResponse = profileLogic.UpdateProfile(realPageId, profile);
                if (repositoryResponse.Id == 0)
                {
                    output.obj = profile;
                    errorStatus.Success = false;
                    errorStatus.ErrorCode = "Profile.UpdateProfile.3";
                    errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                    output.Status = errorStatus;
                    return Ok(output);
                }

                output.Status = errorStatus;
                return Ok(output);
            });
        }

        /// <summary>
        /// Get a user Profile detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>ProfileDetail object</returns>
        [HttpGet("profiles/details")]
        [ProducesResponseType(typeof(ObjectOutput<IProfileDetail, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfileDetail([FromQuery] Guid? realPageId = null)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var userClaim = _userClaimsAccessor.GetUserClaim();
                realPageId = (realPageId == Guid.Empty || realPageId == null) ? userClaim.UserRealPageGuid : realPageId;

                var personaLogic = new ManagePersona(userClaim);
                var profileLogic = new ManageProfile(userClaim);
                var productLogic = new ManageProduct(userClaim);
                var credentialLogic = new ManageCredential(userClaim);
                var manageUserLogin = new ManageUserLogin(userClaim);

                var userLoginOnly = manageUserLogin.GetUserLoginOnly(realPageId.Value);
                profileDetail = profileLogic.GetProfileDetail(realPageId.Value, userClaim.OrganizationPartyId);
                var persona = personaLogic.GetFirstAvailablePersonaByCompany(userLoginOnly.RealPageId, userClaim.OrganizationPartyId);
                var personaProducts = productLogic.GetUserAssignedProductsByPersona(persona);

                profileDetail.SummaryCount = new SummaryCounts()
                {
                    TotalAssignedProducts = personaProducts.Count
                };

                var identityProviderType = credentialLogic.GetIdentityProviderTypeByLoginName(profileDetail.userLogin.LoginName);
                profileDetail.AuthenticationType = identityProviderType.AuthenticationType;

                var userTypes = new List<UserRoleType>() {
                    UserRoleType.SuperUser,
                    UserRoleType.User,
                    UserRoleType.UserNoEmail,
                    UserRoleType.ExternalUser,
                    UserRoleType.SDE,
                    UserRoleType.RealPageEmployee
                };

                // TODO revisit this call, skip if it isn't the primary company?
                if (identityProviderType.IsLocal && !profileDetail.userLogin.PasswordModifiedDate.HasValue && profileDetail.organization.HasAnyUserRole(userTypes)) {
                    profileDetail.VerificationActivityToken = credentialLogic.GetNewUserRegistrationVerificationToken(userLoginOnly.UserId, userLoginOnly.RealPageId);
                    profileDetail.userLogin.IsPending = true;
                }

                // see if the primary company has the user flagged to reset password if this company isn't the users primary login company
                if (!profileDetail.organization.Any(p => p.PrimaryOrganization))
                {
                    var primaryOrgStatus = manageUserLogin.GetUserOrganizationWithStatus(userLoginOnly.UserId, userLoginOnly.LastLogin.Value, 0, true);
                    profileDetail.userLogin.IsForceReSetPassword = primaryOrgStatus.StatusTypeId == (int)UserUiStatusType.ForceResetPassword;
                }

                //3rd Party IDP user should not expired.
                if (!identityProviderType.IsLocal)
                {
                    profileDetail.userLogin.IsExpired = false;
                }

                CheckPasswordExpirationResponse checkPasswordExpirationResponse = credentialLogic.CheckPasswordExpiration(userLoginOnly.UserId, userLoginOnly.RealPageId);
                if (checkPasswordExpirationResponse != null)
                {
                    profileDetail.PasswordExpirationDetail = checkPasswordExpirationResponse;
                }

                var output = new ObjectOutput<IProfileDetail, IErrorData>() { obj = profileDetail };
                return Ok(output);
            });
        }
        #endregion
    }
}
