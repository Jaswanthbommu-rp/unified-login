using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Profile Controller to hold (Person, UserLogin, Contact) management related APIs
	/// </summary>
	public class ProfileController : BaseApiController
    {
        #region Private variables
        IProfileDetail profileDetail = new ProfileDetail();
        IProfile profile = new Profile();
        IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ProfileController() : base() { }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get a user detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>Profile object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get a profile for a Person (User)", Type = typeof(IProfile))]
        [SwaggerResponseExamples(typeof(IProfile), typeof(ProfileExample))]
        [Route("profiles/{realPageId}")]
        [HttpGet]
        public HttpResponseMessage GetProfile(Guid realPageId, string ContactMechanismUsageTypeName = "")
        {
			ObjectOutput<IProfile, IErrorData> output = new ObjectOutput<IProfile, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			IPerson person = new Person();
			IList<TelecommunicationNumber> telecommunicationNumberList = new List<TelecommunicationNumber>();
			PartyRole partyRole = new PartyRole();

			realPageId = (realPageId == Guid.Empty) ? _realpageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
				output.obj = profile;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Profile.GetProfile.1";
				errorStatus.ErrorMsg = "Invalid parameter: realPageId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
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
                    if (_userClaims != null && _userClaims.ImpersonatedBy != Guid.Empty)
                    {
                        profile.IsImpersonated = true;
                    }
                }

                profile.TelecommunicationNumber = telecommunicationNumberList;
                profile.EmailContacts = electronicAddressList;

                //Include the UserLogin details.  IsActive and Is3rdPartyIDP are used by the Edit User
                IManageUserLogin userLoginLogic = new ManageUserLogin();
				var userLogin = userLoginLogic.GetUserLogin(realPageId, _orgPartyId); // keep for now, used by ui, need to investigate how
                userLogin.LoginNameType = EmailFormatValidation.IsValidEmail(userLogin.LoginName) ? "email" : "";

                profile.userLogin = userLogin;

				if (profile != null)
				{
					output.obj = profile;
					output.Status = errorStatus;
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}

			output.obj = profile;
			errorStatus.Success = false;
			errorStatus.ErrorCode = "Profile.GetProfile.1";
			errorStatus.ErrorMsg = "Invalid realPageId";
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.BadRequest, output);
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
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when PartyRelationship object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get a detailed profile for a person", Type = typeof(IProfileDetail))]
        [SwaggerResponseExamples(typeof(IProfileDetail), typeof(ProfileDetailExample))]
        [Route("profiles/{realPageId}/organizations")]
        [HttpGet]
        public HttpResponseMessage GetProfileDetail(Guid realPageId, string roleTypeFrom = null, string roleTypeTo = null, string relationshipType = null, string contactMechanismUsageTypeName = null)
        {
			ObjectOutput<IProfile, IErrorData> output = new ObjectOutput<IProfile, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			IPerson person = new Person();
			IUserLogin userLogin = new UserLogin();
			IList<Organization> organizationList = new List<Organization>();
			IList<CommonAddress> contactMechanismList = new List<CommonAddress>();

			realPageId = (realPageId == Guid.Empty) ? _realpageUserId : realPageId;
            if (realPageId == Guid.Empty)
            {
				output.obj = profile;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Profile.GetProfileDetail.1";
				errorStatus.ErrorMsg = "Invalid parameter: realPageId";
				errorStatus.ErrorData = null;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.BadRequest, output);
			}

			IManagePerson personLogic = new ManagePerson();
            person = personLogic.GetPerson(realPageId);

			if (person != null)
			{
				IManageUserLogin userLoginLogic = new ManageUserLogin(_userClaims);
	            userLogin = userLoginLogic.GetUserLogin(realPageId, _orgPartyId); // keep for now, used by UI

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
					return Request.CreateResponse(HttpStatusCode.OK, output);
				}
			}
			output.obj = profile;
			errorStatus.Success = false;
			errorStatus.ErrorCode = "Prifile.GetProfileDetail.2";
			errorStatus.ErrorMsg = "Invalid realPageId";
			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.BadRequest, output);
        }

        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="profile">profile object of the parameter values</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when profile object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Profile Updated")]
        [Route("profiles/{realPageId}")]
        [AuthorizeRight("editotherprofile", "editownprofile")]
        [HttpPut]
        public HttpResponseMessage UpdateProfile(Guid realPageId, [FromBody]Profile profile)
        {
			ObjectOutput<IProfile, IErrorData> output = new ObjectOutput<IProfile, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.obj = profile;
			realPageId = ((realPageId == Guid.Empty) || (realPageId == null)) ? _realpageUserId : realPageId;
			if ((realPageId == Guid.Empty) || (realPageId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Profile.UpdateProfile.1";
				errorStatus.ErrorMsg = "Update Profile: Invalid parameter realPageId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if (profile == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Profile.UpdateProfile.2";
				errorStatus.ErrorMsg = "Update Profile: Invalid parameter Profile";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (profile.IsFirstNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Profile.UpdateProfile.4";
				errorStatus.ErrorMsg = "First name is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (profile.IsLastNameNullOrWhiteSpace)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Profile.UpdateProfile.5";
				errorStatus.ErrorMsg = "Last name is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageProfile profileLogic = new ManageProfile(_userClaims);
			repositoryResponse = profileLogic.UpdateProfile(realPageId, profile);
			if (repositoryResponse.Id == 0)
			{
				output.obj = profile;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "Profile.UpdateProfile.3";
				errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
        }

        /// <summary>
        /// Get a user Profile detail
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>        
        /// <returns>ProfileDetail object</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when PartyRelationship object have invalid entries)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get a detailed profile for a person", Type = typeof(IProfileDetail))]
        [SwaggerResponseExamples(typeof(IProfileDetail), typeof(ProfileDetailExample))]
        [Route("profiles/details")]
        [HttpGet]
        public HttpResponseMessage GetProfileDetail(Guid? realPageId = null)
        {
            realPageId = (realPageId == Guid.Empty || realPageId == null) ? _realpageUserId : realPageId;

            var personaLogic = new ManagePersona(_userClaims);
            var profileLogic = new ManageProfile(_userClaims);
            var productLogic = new ManageProduct(_userClaims);
            var credentialLogic = new ManageCredential(_userClaims);
            var manageUserLogin = new ManageUserLogin(_userClaims);

            var userLoginOnly = manageUserLogin.GetUserLoginOnly(realPageId.Value);
            profileDetail = profileLogic.GetProfileDetail(realPageId.Value, _orgPartyId);
            var persona = personaLogic.GetFirstAvailablePersonaByCompany(userLoginOnly.RealPageId, _orgPartyId);
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
            return Request.CreateResponse(HttpStatusCode.OK, output);
        }
        #endregion

        #region Get Examples
        /// <summary>
        /// Used to document examples of the Detailed profile for a person (Contract Mechanism, Linked Organization, Role with each Organization,... Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProfileDetailExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Profile example</returns>
            public object GetExamples()
            {
                IUserLogin userLogin = new UserLogin()
                {
                    UserId = 1,
                    PartyId = 1,
                    LoginName = "test@test.com",
                    IsActive = true,
                    PasswordHash = "none",
                    IsLocked = false,
                    IsTainted = false,
                    FromDate = DateTime.UtcNow,
                    ThruDate = DateTime.MaxValue.ToUniversalTime()
                };

                RoleType roleTypeFrom = new RoleType()
                {
                    PartyRoleTypeId = (int)UserRoleType.User,
                    ParentPartyRoleTypeId = 400,
                    Name = "User"
                };

                RoleType roleTypeTo = new RoleType()
                {
                    PartyRoleTypeId = 202,
                    ParentPartyRoleTypeId = 200,
                    Name = "Property Management Company"
                };

                RelationshipType relationshipType = new RelationshipType()
                {
                    RelationshipTypeId = 44,
                    RoleTypeIdValidFrom = (int)UserRoleType.User,
                    RoleTypeIdValidTo = 202,
                    Name = "User Relationship",
                    Description = ""
                };

                PartyRelationship partyRelationship = new PartyRelationship()
                {
                    PartyRelationshipId = 3,
                    PartyIdFrom = 19,
                    RealPageIdFrom = new Guid("8946d26d-8ede-40d1-b6c3-d52bc903f202"),
                    PartyIdTo = 6,
                    RealPageIdTo = new Guid("724DE532-7969-42B5-9E71-2955167179BA"),
                    RoleTypeIdFrom = (int)UserRoleType.User,
                    RoleTypeFrom = roleTypeFrom,
                    RoleTypeIdTo = 202,
                    RoleTypeTo = roleTypeTo,
                    PartyRelationshipTypeId = 44,
                    PartyRelationshipType = relationshipType,
                    FromDate = DateTime.UtcNow,
                    ThruDate = DateTime.MaxValue.ToUniversalTime()
                };

                IList<Organization> organizationList = new List<Organization>();
                Organization organization = new Organization()
                {
                    RealPageId = new Guid("E0F512CE-D7B8-4CC9-998C-6CEFD046BE11"),
                    Name = "Organization A"
                };
                organizationList.Add(organization);

                organization = new Organization()
                {
                    RealPageId = new Guid("724DE532-7969-42B5-9E71-2955167179BA"),
                    Name = "Organization B",
                    partyRelationship = partyRelationship
                };
                organizationList.Add(organization);

                organization = new Organization()
                {
                    RealPageId = new Guid("D6BC2CB8-D62F-4292-8DED-8E1A04A3C816"),
                    Name = "Organization C"
                };
                organizationList.Add(organization);

                ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
                {
                    ContactMechanismUsageTypeId = 201,
                    ParentContactMechanismUsageTypeId = 200,
                    Name = "Primary"
                };

                IList<CommonAddress> contactMechanismList = new List<CommonAddress>();
                contactMechanismList.Add(new CommonAddress()
                {
                    ContactMechanismId = 3,
                    AddressString = "cfdba@live.com",
                    AddressType = "Email",
                    contactMechanismUsageType = contactMechanismUsageType
                });

                contactMechanismUsageType = new ContactMechanismUsageType()
                {
                    ContactMechanismUsageTypeId = 202,
                    ParentContactMechanismUsageTypeId = 200,
                    Name = "Other"
                };

                contactMechanismList.Add(new CommonAddress()
                {
                    ContactMechanismId = 4,
                    AddressString = "none@nowhere.com",
                    AddressType = "Email",
                    contactMechanismUsageType = contactMechanismUsageType
                });

                IProfileDetail example = new ProfileDetail()
                {
                    PartyId = 1,
                    Title = "Property Manager",
                    FirstName = "John",
                    MiddleName = "R",
                    LastName = "Doe",
                    Suffix = "Mr",
                    userLogin = userLogin,
                    contactMechanism = contactMechanismList,
                    organization = organizationList
                };

                ObjectOutput<IProfileDetail, IErrorData> output = new ObjectOutput<IProfileDetail, IErrorData>() { obj = example };

                return output;
            }
        }

		/// <summary>
		/// Used to document examples of the User Profile
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class ProfileExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Profile example</returns>
            public object GetExamples()
            {
                ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType()
                {
                    ContactMechanismUsageTypeId = 201,
                    ParentContactMechanismUsageTypeId = 202,
                    Name = "Phone"
                };

                IList<TelecommunicationNumber> telecomunicationNumberList = new List<TelecommunicationNumber>();
                TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber()
                {
                    PartyContactMechanismId = 10003,
                    ContactMechanismId = 1003,
                    CountryCode = "01",
                    AreaCode = "972",
                    PhoneNumber = "8204000",
                    IsDefault = true,
                    contactMechanismUsageType = contactMechanismUsageType
                };

                telecomunicationNumberList.Add(telecommunicationNumber);

                PartyRole partyRole = new PartyRole()
                {
                    PartyRoleId = 1003,
                    PartyId = 19,
                    RoleTypeId = (int)UserRoleType.SuperUser
				};

                IProfile example = new Profile()
                {
                    PartyId = 1,
                    Title = "Property Manager",
                    FirstName = "John",
                    MiddleName = "R",
                    LastName = "Doe",
                    Suffix = "Mr",
                    PreferredContactMethodId = 1,
                    TelecommunicationNumber = telecomunicationNumberList,
                    PartyRole = partyRole
                };

                ObjectOutput<IProfile, IErrorData> output = new ObjectOutput<IProfile, IErrorData>() { obj = example };

                return output;
            }
        }
        #endregion
    }
}