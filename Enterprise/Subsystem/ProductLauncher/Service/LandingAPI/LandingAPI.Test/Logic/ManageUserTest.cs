using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	/// <summary>
	/// Person Unit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManageUserTest
	{
		Mock<IUserRepository> _mockUserRepository = new Mock<IUserRepository>();
		Mock<ICredentialRepository> _mockCredentialRepository = new Mock<ICredentialRepository>();
		Mock<IManageEmail> _mockEmailLogic = new Mock<IManageEmail>();
		Mock<IContactMechanismRepository> _mockContactMechanismRepository = new Mock<IContactMechanismRepository>();
		Mock<IManageUserLogin> _mockUserLoginLogic = new Mock<IManageUserLogin>();
		Mock<IUserLoginRepository> _mockUserLoginRepository = new Mock<IUserLoginRepository>();
		Mock<IManageOrganization> _mockOrganizationLogic = new Mock<IManageOrganization>();
		Mock<IManageCommunicationEvents> _mockCommunicationEventsLogic = new Mock<IManageCommunicationEvents>();
		Mock<IManagePerson> _mockManagePerson = new Mock<IManagePerson>();
		Mock<IUserTokenRepository> _mockUserTokenRepository = new Mock<IUserTokenRepository>();
		Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();

		DefaultUserClaim userClaims = new DefaultUserClaim()
		{
			LoginName = "MocTest",
			CorrelationId = Guid.NewGuid(),
			OrganizationName = "MocTest",
			OrganizationPartyId = 1,
			OrganizationRealPageGuid = Guid.NewGuid(),
			OrganizationMasterId = 1,
			UserRealPageGuid = Guid.NewGuid(),
		};

		CommunicationEmail expectedEmailTemplateResponse = new CommunicationEmail()
		{
			CommunicationEmailTemplateId = 1,
			Subject = "Welcome to RealPage!",
			Body = "Email Content"
		};

		Email expectedCESEmailResponse = new Email()
		{
			ClientUniqueID = Guid.NewGuid(),
			EmailFrom = "no-reply@realpage.com",
			EmailSubject = "Welcome to RealPage!",
			EmailBody = "Email Content with values",
			EntityID = "1",
			SiteID = "1"
		};

		string expectedSendEmailResponse = "Email sent successfully.";

		[Fact]
		public void AssignProductsToAdministrators_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			Guid emptyRealPageId = Guid.Empty;
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);


			//Act
			Exception exception = Record.Exception(() => manageUser.AssignProductsToAdministrators(emptyRealPageId, 1));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void AssignProductsToAdministrators_InvalidAssignUserPersonaId_ExceptionThrown()
		{
			//Arrange
			Guid emptyRealPageId = Guid.Empty;
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);

			//Act
			Exception exception = Record.Exception(() => manageUser.AssignProductsToAdministrators(emptyRealPageId, -1));

			//Assert
			Assert.IsType<Exception>(exception);
		}

        [Fact(Skip = "Integration test")]
        public void CreateNewUser_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			Type responseType = typeof(CreateUserResponse<IErrorData>);
			IUserLogin newUserLogin = new UserLogin()
			{
				LoginName = "johndoe@test.com",
				FromDate = DateTime.UtcNow
			};
			Persona newUserPersona = new Persona()
			{
				Name = "Primary",
				PersonaEnvironmentTypeId = 1,
				FromDate = DateTime.UtcNow
			};

			List<Persona> personaList = new List<Persona>();
			personaList.Add(newUserPersona);

			Guid userRealPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");

			UserLoginOnly userLoginOnly = new UserLoginOnly()
			{
				UserId = 1234,
				LoginName = newUserLogin.LoginName,
				RealPageId = userRealPageId,
				Is3rdPartyIDP = false
			};

			ProfileDetail newUserProfile = new ProfileDetail()
			{
				RealPageId = userRealPageId,
				userLogin = newUserLogin,
				FirstName = "John",
				LastName = "Doe",
				Persona = personaList,
				Password = "P@ssw0rd",
				NotificationEmail = "johndoe@test.com",
				CreateUserSourceType = Component.SharedObjects.Enum.CreateUserSourceType.UnifiedPlatform,
				UserTypeId = UserTypeConstants.RegularUser
			};
			string senderEmail = "no-reply@realpage.com";
			var localIDP = new IdentityProviderType()
			{
				AuthenticationType = "Local"
			};
			Guid orgRealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C");
			IList<Organization> listOrg = new List<Organization>();
			listOrg.Add(new Organization() { RealPageId = orgRealPageId, PartyId = 1234, Name = "Test Company", PrimaryOrganization = true });
			newUserProfile.organization = listOrg;

			int audienceType = (int)CommunicationEventAudienceType.RegularUser;
			int purposeType = 1;

			string userToken = Guid.NewGuid().ToString();
			string usageType = "Email Notification";

			errorStatus.Success = true;
			errorStatus.ErrorCode = "";
			errorStatus.ErrorMsg = "";

			CreateUserResponse<IErrorData> expectedNewUserResponse = new CreateUserResponse<IErrorData>()
			{
				EmailTemplate = "",
				EmailStatus = "",
				UserStatus = "User created successfully.",
				UserToken = userToken,
				PersonaId = 1,
				Status = errorStatus,
				UserRealPageGuid = userRealPageId
			};

			List<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>()
			{
				new ProductInternalSetting()
				{
					Name = "IsSendGridEnabled",
					Value = "1"
				}
			};

		   IdentityProviderType expectedIdentityProviderType = new IdentityProviderType()
			{
				AuthenticationType = "local"
			};
			CommonAddress orgCommonAddress = new CommonAddress()
			{
				PartyContactMechanismId = 1,
				ContactMechanismId = 1,
				AddressString = senderEmail,
				AddressType = "Email",
				ContactMechanismUsageTypeId = 301
			};
			IList<CommonAddress> orgCommonAddressList = new List<CommonAddress>();
			orgCommonAddressList.Add(orgCommonAddress);

			CommonAddress userCommonAddress = new CommonAddress()
			{
				PartyContactMechanismId = 2,
				ContactMechanismId = 1,
				AddressString = newUserLogin.LoginName,
				AddressType = "Email",
				ContactMechanismUsageTypeId = 301
			};
			IList<CommonAddress> userCommonAddressList = new List<CommonAddress>();
			userCommonAddressList.Add(userCommonAddress);

			Person person = new Person() { FirstName = newUserProfile.FirstName, LastName = newUserProfile.LastName };

			RepositoryResponse cesRepositoryResponse = new RepositoryResponse() { Id = 1234, ErrorMessage = "Success" };

			_mockUserRepository
				.Setup(m => m.CreateUser(newUserProfile, personaList))
				.Returns(expectedNewUserResponse);

			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(userRealPageId))
				.Returns(listOrg);

			_mockCredentialRepository
				.Setup(m => m.GetIdentityProviderTypeByLoginName(newUserProfile.userLogin.LoginName))
				.Returns(expectedIdentityProviderType);

			_mockEmailLogic
				.Setup(m => m.GetEmailTemplate(audienceType, purposeType))
				.Returns(expectedEmailTemplateResponse);

			_mockEmailLogic
				.Setup(m => m.CreateWelcomeEmail(newUserProfile.userLogin.LoginName, newUserProfile.FirstName, newUserProfile.organization[0].Name, newUserProfile.organization[0].PartyId, expectedEmailTemplateResponse, userToken, senderEmail, newUserProfile.NotificationEmail))
				.Returns(expectedCESEmailResponse);

			_mockEmailLogic
				.Setup(m => m.SendEmail(expectedCESEmailResponse))
				.Returns(expectedSendEmailResponse);

			_mockContactMechanismRepository
				.Setup(m => m.ListContactMechanismForPerson(orgRealPageId, usageType))
				.Returns(orgCommonAddressList);

			_mockContactMechanismRepository
				.Setup(m => m.ListContactMechanismForPerson(userRealPageId, usageType))
				.Returns(userCommonAddressList);

			_mockManagePerson
				.Setup(m => m.GetPerson(userRealPageId))
				.Returns(person);

			_mockUserLoginRepository
				.Setup(m => m.GetUserLoginOnly(newUserLogin.LoginName))
				.Returns(userLoginOnly);

			_mockUserLoginRepository
				.Setup(m => m.ListOrganizationByEnterpriseUserId(userRealPageId, null))
				.Returns(listOrg);

			_mockUserTokenRepository
				.Setup(m => m.GetUserActivityToken(userRealPageId, (int)ActivityType.NewUserRegistration, listOrg[0].PartyId))
				.Returns(userToken);

			_mockCommunicationEventsLogic
				.Setup(m => m.CreateCommunicationEvent(It.IsAny<int>(), orgCommonAddressList[0].PartyContactMechanismId, userCommonAddressList[0].PartyContactMechanismId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
				.Returns(cesRepositoryResponse);

			_mockCommunicationEventsLogic
				.Setup(m => m.CreateCommunicationEventEmail(It.IsAny<int>(), It.IsAny<long>()))
				.Returns(cesRepositoryResponse);

			_mockCommunicationEventsLogic
				.Setup(m => m.CreateCESCommunicationEventEmail(It.IsAny<string>(), It.IsAny<long>()))
				.Returns(cesRepositoryResponse);

			_mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
				.Returns(productInternalSettingList);

			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, _mockCommunicationEventsLogic.Object, _mockUserTokenRepository.Object, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, userRegistrationEmail, userClaims);

			//Act
			int numberOfProperties = responseType.GetProperties().Length;
			ICreateUserResponse<IErrorData> actualNewUserResponse = manageUser.CreateUser(newUserProfile, personaList);

			//Assert
			Assert.True(actualNewUserResponse.Status.Success == true
						&& actualNewUserResponse.Status.ErrorMsg == ""
						&& actualNewUserResponse.UserStatus == "User created successfully."
						&& actualNewUserResponse.UserToken == userToken
						&& actualNewUserResponse.PersonaId > 0
						&& numberOfProperties == 8);
		}

		[Fact]
		public void UpdateNewUser_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Type responseType = typeof(RepositoryResponse);
			string userLogin = "test1@test.com";
			PartyRole partyRole = new PartyRole()
			{
				PartyId = 33,
				PartyRoleId = 328,
				RoleTypeId = 203
			};
			TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber()
			{
				PhoneNumber = "0123456789"
			};
			IList<TelecommunicationNumber> telecommunicationNumberList = new List<TelecommunicationNumber>()
			{
				telecommunicationNumber
			};
			Profile newProfile = new Profile()
			{
				FirstName = "James",
				MiddleName = "N",
				LastName = "Reames",
				Suffix = "Mr.",
				Title = "Solutions Architect IV",
				PartyRole = partyRole,
				TelecommunicationNumber = telecommunicationNumberList
			};
			IUserLogin newUserLogin = new UserLogin()
			{
				LoginName = "johndoe",
				FromDate = DateTime.UtcNow
			};
			Persona newUserPersona = new Persona()
			{
				Name = "Primary",
				PersonaEnvironmentTypeId = 1,
				FromDate = DateTime.UtcNow
			};
			List<Persona> personaList = new List<Persona>();
			personaList.Add(newUserPersona);

			Guid realPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");

			ProfileDetail newUserProfile = new ProfileDetail()
			{
				userLogin = newUserLogin,
				FirstName = "James",
				LastName = "Reames",
				Persona = personaList,
				Password = "P@ssw0rd",
				NotificationEmail = "james@test.com",
				CreateUserSourceType = Component.SharedObjects.Enum.CreateUserSourceType.UnifiedPlatform
			};
			int partyRoleTypeId = 328;
			string companyJobTitle = "Leasing Agent I";
			string activityToken = Guid.NewGuid().ToString();

			Guid orgRealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C");
			IList<Organization> listOrg = new List<Organization>();
			listOrg.Add(new Organization() { RealPageId = orgRealPageId, PartyId = 1234 });
			newUserProfile.organization = listOrg;

			int audienceType = 1;
			int purposeType = 1;
			string userToken = Guid.NewGuid().ToString();
			string usageType = "Email";

			RepositoryResponse expectedRepositoryResponse = new RepositoryResponse()
			{
				Id = 1,
				ErrorMessage = "",
				RealPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F")
			};
			CommonAddress expectedCommonAddress = new CommonAddress()
			{
				PartyContactMechanismId = 1,
				ContactMechanismId = 1,
				AddressString = "no-reply@realpage.com",
				AddressType = "Email",
				ContactMechanismUsageTypeId = 301
			};
			IList<CommonAddress> expectedCommonAddressList = new List<CommonAddress>();
			expectedCommonAddressList.Add(expectedCommonAddress);
			string senderEmail = "no-reply@realpage.com";

			Person person = new Person() { FirstName = newUserProfile.FirstName };

			List<ProductInternalSetting> productInternalSettingList = new List<ProductInternalSetting>()
			{
				new ProductInternalSetting()
				{
					Name = "IsSendGridEnabled",
					Value = "1"
				}
			};
			_mockCredentialRepository
				.Setup(m => m.ListOrganizationByRealPageId(realPageId))
				.Returns(listOrg);
			_mockUserRepository
				.Setup(m => m.UpdateNewUser(userLogin, newProfile, partyRoleTypeId, companyJobTitle, activityToken))
				.Returns(expectedRepositoryResponse);
			_mockEmailLogic
				.Setup(m => m.GetEmailTemplate(audienceType, purposeType))
				.Returns(expectedEmailTemplateResponse);
			_mockEmailLogic
				.Setup(m => m.CreateWelcomeEmail(newUserProfile.userLogin.LoginName, newUserProfile.FirstName, newUserProfile.organization[0].Name, newUserProfile.organization[0].PartyId, expectedEmailTemplateResponse, userToken, senderEmail, newUserProfile.NotificationEmail))
				.Returns(expectedCESEmailResponse);
			_mockEmailLogic
				.Setup(m => m.SendEmail(expectedCESEmailResponse))
				.Returns(expectedSendEmailResponse);
			_mockContactMechanismRepository
				.Setup(m => m.ListContactMechanismForPerson(orgRealPageId, usageType))
				.Returns(expectedCommonAddressList);
			_mockManagePerson
				.Setup(m => m.GetPerson(realPageId))
				.Returns(person);

			_mockProductInternalSettingRepository
				.Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
				.Returns(productInternalSettingList);

			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, null, null, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, userRegistrationEmail, userClaims);

			//Act
			int numberOfProperties = responseType.GetProperties().Length;
			IRepositoryResponse repositoryResponse = manageUser.UpdateNewUser(userLogin, newProfile, partyRoleTypeId, companyJobTitle, activityToken);

			//Assert
			Assert.True(repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == expectedRepositoryResponse.RealPageId
				&& numberOfProperties == 3);
		}

        
		#region Validate User
		[Fact]
		public void ValidateUser_Error_NoUserName()
		{
			//Arrange
			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, _mockCommunicationEventsLogic.Object, _mockUserTokenRepository.Object, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);

			//Act
			var response = manageUser.ValidateUser(null, null);

			//Assert
			Assert.True(response.IsError);
			Assert.Equal("No Username specified.", response.ErrorReason);
		}

		[Fact]
		public void ValidateUser_Error_NoTokenSupplied()
		{
			//Arrange
			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, _mockCommunicationEventsLogic.Object, _mockUserTokenRepository.Object, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);

			//Act
			var response = manageUser.ValidateUser("someUserName", null);

			//Assert
			Assert.True(response.IsError);
			Assert.Equal("No validation token specified.", response.ErrorReason);
		}

		[Fact]
		public void ValidateUser_Error_NoTokenMatch()
		{
			//Arrange
			string enterpriseUserdName = "someUerName";
			var _mockUserRepository = new Mock<IUserRepository>();
			var _mockCredentialRepository = new Mock<ICredentialRepository>();
			long organizationPartyId = 1;
			_mockCredentialRepository.Setup(m => m.GetActivityToken(enterpriseUserdName, "t", (int)ActivityType.NewUserRegistration, organizationPartyId)).Returns(() => null);

			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, _mockCommunicationEventsLogic.Object, _mockUserTokenRepository.Object, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);

			//Act
			ValidateUserResponse response = manageUser.ValidateUser(enterpriseUserdName, "t");

			//Assert
			Assert.True(
				response.ErrorReason == "User login information is missing." && response.IsError);
		}

		[Fact]
		public void ValidateUser_Error_UnableToGetToken()
		{
			//Arrange
			string enterpriseUserdName = "someUerName";
			int enterpriseUserId = 101;
			long organizationPartyId = 1;
			Guid realPageId = new Guid();
			UserLoginOnly userlogin = new UserLoginOnly()
			{
				UserId = enterpriseUserId,
				LoginName = "test",
				PartyId = 30,
				RealPageId = realPageId,
				LastLogin = DateTime.UtcNow
			};

			OrganizationStatus organizationStatus = new OrganizationStatus()
			{
				PartyId = organizationPartyId,
				IsPending = true,
				IsActive = true,
				IsExpired = false,
				StatusTypeId = (int)UserUiStatusType.Active,
				Status = UserUiStatusType.Active
			};

			IList<Organization> orgList = new List<Organization>() { new Organization() { PartyId = organizationPartyId, Name = "Primary Org", PrimaryOrganization = true } };

			var _mockUserRepository = new Mock<IUserRepository>();
			var _mockCredentialRepository = new Mock<ICredentialRepository>();
			var _mockUserLoginLogic = new Mock<IManageUserLogin>();

			_mockUserLoginRepository
			   .Setup(m => m.GetUserLoginOnly(
				   enterpriseUserdName
			   ))
			   .Returns(userlogin);

			_mockUserLoginLogic
				.Setup(m => m.GetUserLoginOnly(
					userlogin.RealPageId
				))
				.Returns(userlogin);

			_mockUserLoginRepository
				.Setup(m => m.GetPrimaryOrgWithoutStatusByUserId(
					userlogin.UserId
				))
				.Returns(organizationStatus);

			_mockUserLoginRepository
				.Setup(m => m.ListOrganizationByEnterpriseUserId(userlogin.RealPageId, null))
				.Returns(orgList);

			_mockUserLoginRepository
				.Setup(m => m.GetUserOrganizationWithStatus(userlogin.UserId, It.IsAny<DateTime>(), It.IsAny<long>(), true))
				.Returns(organizationStatus);

			_mockCredentialRepository.Setup(m => m.GetActivityToken(enterpriseUserdName, "t", (int)ActivityType.NewUserRegistration, organizationPartyId)).Returns(() => new TokenDetail { EnterpriseUserId = enterpriseUserId, Token = "t" });
			_mockCredentialRepository.Setup(m => m.CreateActivityToken(organizationPartyId, realPageId, (int)ActivityType.NewUserRegistrationVerification)).Returns(() => null);

			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, _mockCommunicationEventsLogic.Object, _mockUserTokenRepository.Object, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);

			//Act

			//Assert
			Assert.Throws<Exception>(() => manageUser.ValidateUser(enterpriseUserdName, "t"));

			organizationStatus.IsActive = false;
			organizationStatus.IsPending = true;

			manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, userRegistrationEmail, userClaims);

			var response = manageUser.ValidateUser(enterpriseUserdName, "t");
			Assert.Equal("Account is inactive.", response.ErrorReason, true, true, true);

			organizationStatus.IsActive = true;
			organizationStatus.IsPending = false;

			manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, userRegistrationEmail, userClaims);

			response = manageUser.ValidateUser(enterpriseUserdName, "t");
			Assert.Equal("Profile already completed.", response.ErrorReason, true, true, true);

			_mockCredentialRepository.Setup(m => m.GetActivityToken(enterpriseUserdName, "t", (int)ActivityType.NewUserRegistration, organizationPartyId)).Returns(() => null);

			manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, userRegistrationEmail, userClaims);

			//Act

			//Assert
			response = manageUser.ValidateUser(enterpriseUserdName, "t");
			Assert.Equal("This link has expired. Please contact your System Administrator.", response.ErrorReason, true, true, true);
		}
		#endregion

		#region Set Profile / Get Profile
		[Fact]
		public void SetStarterProfile_Error_ArgumentNullException()
		{
			//Arrange
			var _mockUserRepository = new Mock<IUserRepository>();
			var _mockCredentialRepository = new Mock<ICredentialRepository>();

			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, _mockCommunicationEventsLogic.Object, _mockUserTokenRepository.Object, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => manageUser.SetStarterProfile(null));
		}

		[Fact]
		public void GetStarterProfileOptions_Error_ArgumentNullException()
		{
			//Arrange
			var _mockUserRepository = new Mock<IUserRepository>();
			var _mockCredentialRepository = new Mock<ICredentialRepository>();

			IManageUserRegistrationEmail userRegistrationEmail = new ManageUserRegistrationEmail(userClaims, _mockEmailLogic.Object, _mockContactMechanismRepository.Object, _mockCommunicationEventsLogic.Object, _mockUserTokenRepository.Object, _mockManagePerson.Object, _mockUserLoginRepository.Object, _mockProductInternalSettingRepository.Object);
			ManageUser manageUser = new ManageUser(_mockUserRepository.Object, _mockCredentialRepository.Object, _mockUserLoginRepository.Object, null, userClaims);

			//Act

			//Assert
			Assert.Throws<ArgumentNullException>(() => manageUser.GetStarterProfileOptions(null));
		}
		#endregion
	}
}
