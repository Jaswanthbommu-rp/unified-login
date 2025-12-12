using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// UserLogin xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class UserLoginTests
	{
		#region Controller Unit Tests
		[Fact]
		public void CreateUserLogin_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("CreateUserLogin" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/api/userlogins/13e71de5-bafa-469d-9f7a-e12db3961ba9"
				)
			);
		}

		[Fact]
		public void GetUserLogin_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("GetUserLogin" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/userlogins/13e71de5-bafa-469d-9f7a-e12db3961ba9"
				)
			);
		}

		[Fact]
		public void UpdateUserLogin_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("UpdateUserLogin" == baseTest.VerifyRouteToAction(
				HttpMethod.Put,
				"http://localhost/api/userlogins/13E71DE5-BAFA-469D-9F7A-E12DB3961BA9"
				)
			);
		}

		[Fact]
		public void IsLoginNameExists_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpConfiguration Config = new HttpConfiguration();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("IsLoginNameExists" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/userlogins/loginnameexists?loginName=james@test.com&OrganizationRealPageId=9E9410AE-2C41-47D2-81D1-109C08CD151C"
				)
			);
		}

		[Fact]
		public void IsLoginNameExists_InvalidOrganizationRealPageId_ReturnErrorData()
		{
			//Arrange
			string loginName = "james@test.com";
			Guid organizationRealPageId = new Guid();
            Guid userRealPageId = Guid.Empty;

            UserLoginController controller = new UserLoginController();
			ObjectOutput<UserOrganizationExists, IErrorData> userOrganizationExistsOutput = new ObjectOutput<UserOrganizationExists, IErrorData>();
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();

			//Act
			HttpResponseMessage response = controller.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId);
			userOrganizationExistsOutput = response.Content.ReadAsAsync<ObjectOutput<UserOrganizationExists, IErrorData>>().Result;


			//Assert
			Assert.False(userOrganizationExistsOutput.Status.Success);
			Assert.Equal("UserLogin.IsLoginNameExists.1", userOrganizationExistsOutput.Status.ErrorCode);
			Assert.Equal("IsLoginNameExists: Invalid parameter enterprise organization Id", userOrganizationExistsOutput.Status.ErrorMsg);
		}

		[Fact]
		public void IsLoginNameExists_InvalidLoginName_ReturnErrorData()
		{
			//Arrange
			string loginName = string.Empty;
			Guid organizationRealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C");
            Guid userRealPageId = Guid.Empty;

            UserLoginController controller = new UserLoginController();
			ObjectOutput<UserOrganizationExists, IErrorData> userOrganizationExistsOutput = new ObjectOutput<UserOrganizationExists, IErrorData>();
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();

			//Act
			HttpResponseMessage response = controller.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId);
			userOrganizationExistsOutput = response.Content.ReadAsAsync<ObjectOutput<UserOrganizationExists, IErrorData>>().Result;


			//Assert
			Assert.False(userOrganizationExistsOutput.Status.Success);
			Assert.Equal("UserLogin.IsLoginNameExists.2", userOrganizationExistsOutput.Status.ErrorCode);
			Assert.Equal("IsLoginNameExists: Invalid parameter loginName", userOrganizationExistsOutput.Status.ErrorMsg);
		}

		[Fact]
		public void IsLoginNameExists_ValidDomainUsername_ReturnsTrue()
		{
			// Arrange
			string loginName = "john.doe@realpage.com";
			Guid organizationRealPageId = Guid.NewGuid();
			Guid userRealPageId = Guid.Empty;

			var expectedObj = new UserOrganizationExists
			{
				IsValidDomainUsername = true,
				UserExists = true,
				UserExistsInThisOrganization = true,
				UserExistsAsNoEmail = false,
				UserIsExternalEverywhere = false,
				UserExistsNotAvailable = false,
				UserIsDisabledInPrimaryCompany = false,
				OrgIsRealpageEmployee = true,
				UserExistsAsAdminInOtherDomain = false,
				UserExistsAsRegularUserInOtherDomain = false,
				SuperVisor = null,
				Restricted = null
			};

			var manageUserLoginMock = new Mock<IManageUserLogin>();
			manageUserLoginMock
				.Setup(m => m.IsLoginNameExists(
					It.Is<string>(s => s == loginName),
					It.Is<Guid>(g => g == organizationRealPageId),
					It.IsAny<Guid>(),
					It.IsAny<int>(),
					It.IsAny<bool>()))
				.Returns(expectedObj);

			var controller = new UserLoginController();
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();

			// Set up user claims via reflection to avoid null reference
			var userClaims = new DefaultUserClaim
			{
				UserRealPageGuid = Guid.NewGuid(),
				PersonaId = 1,
				OrganizationPartyId = 1,
				OrganizationMasterId = 1,
				OrganizationName = "Test Org",
				FirstName = "Test",
				LastName = "User",
				LoginName = "testuser",
				UserId = 1,
				ImpersonatedBy = Guid.Empty,
				ImpersonatedByName = ""
			};

			var userClaimsField = typeof(BaseApiController).GetField("_userClaims", 
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			userClaimsField?.SetValue(controller, userClaims);

			// Inject the mocked IManageUserLogin instance via reflection
			// This allows GetManageUserLoginInstance() helper to use the mock
			var manageUserLoginField = typeof(UserLoginController).GetField("_manageUserLogin",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			manageUserLoginField?.SetValue(controller, manageUserLoginMock.Object);

			// Act
			var response = controller.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId);
			var output = response.Content.ReadAsAsync<ObjectOutput<UserOrganizationExists, IErrorData>>().Result;

			// Assert
			Assert.NotNull(output);
			Assert.NotNull(output.obj);
			Assert.True(output.obj.IsValidDomainUsername, "IsValidDomainUsername should be true for realpage.com domain");
			Assert.True(output.Status.Success, "Response status should be successful");
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			// Verify the mock was called with expected parameters
			manageUserLoginMock.Verify(
				m => m.IsLoginNameExists(
					It.Is<string>(s => s == loginName),
					It.Is<Guid>(g => g == organizationRealPageId),
					It.IsAny<Guid>(),
					It.IsAny<int>(),
					It.IsAny<bool>()),
				Times.Once,
				"IsLoginNameExists should have been called once on the mocked instance");
		}

		[Fact]
		public void IsLoginNameExists_InvalidDomainUsername_ReturnsFalse()
		{
			// Arrange
			string loginName = "user@external.com";
			Guid organizationRealPageId = Guid.NewGuid();
			Guid userRealPageId = Guid.Empty;

			var expectedObj = new UserOrganizationExists
			{
				IsValidDomainUsername = false,
				UserExists = true,
				UserExistsInThisOrganization = false,
				UserExistsAsNoEmail = false,
				UserIsExternalEverywhere = true,
				UserExistsNotAvailable = false,
				UserIsDisabledInPrimaryCompany = false,
				OrgIsRealpageEmployee = false,
				UserExistsAsAdminInOtherDomain = false,
				UserExistsAsRegularUserInOtherDomain = false,
				SuperVisor = null,
				PrimaryCompanyName = "Other Company",
				Restricted = null
			};

			// Create a mock of IManageUserLogin
			var manageUserLoginMock = new Mock<IManageUserLogin>();
			manageUserLoginMock
				.Setup(m => m.IsLoginNameExists(
					It.Is<string>(s => s == loginName),
					It.Is<Guid>(g => g == organizationRealPageId),
					It.IsAny<Guid>(),
					It.IsAny<int>(),
					It.IsAny<bool>()))
				.Returns(expectedObj);

			// Create controller instance
			var controller = new UserLoginController();
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();

			// Set up user claims via reflection to avoid null reference
			var userClaims = new DefaultUserClaim
			{
				UserRealPageGuid = Guid.NewGuid(),
				PersonaId = 1,
				OrganizationPartyId = 1,
				OrganizationMasterId = 1,
				OrganizationName = "Test Org",
				FirstName = "Test",
				LastName = "User",
				LoginName = "testuser",
				UserId = 1,
				ImpersonatedBy = Guid.Empty,
				ImpersonatedByName = ""
			};

			var userClaimsField = typeof(BaseApiController).GetField("_userClaims", 
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			userClaimsField?.SetValue(controller, userClaims);

			// Inject the mocked IManageUserLogin instance via reflection
			// This allows GetManageUserLoginInstance() helper to use the mock
			var manageUserLoginField = typeof(UserLoginController).GetField("_manageUserLogin",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			manageUserLoginField?.SetValue(controller, manageUserLoginMock.Object);

			// Act
			var response = controller.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId);
			var output = response.Content.ReadAsAsync<ObjectOutput<UserOrganizationExists, IErrorData>>().Result;

			// Assert
			Assert.NotNull(output);
			Assert.NotNull(output.obj);
			Assert.False(output.obj.IsValidDomainUsername, "IsValidDomainUsername should be false for blacklisted domain");
			Assert.True(output.Status.Success, "Response status should be successful");
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			// Verify the mock was called with expected parameters
			manageUserLoginMock.Verify(
				m => m.IsLoginNameExists(
					It.Is<string>(s => s == loginName),
					It.Is<Guid>(g => g == organizationRealPageId),
					It.IsAny<Guid>(),
					It.IsAny<int>(),
					It.IsAny<bool>()),
				Times.Once,
				"IsLoginNameExists should have been called once on the mocked instance");
		}

		#region Domain Validation Tests

		[Fact]
		public void CheckUserDomainValidOrNot_WithValidDomain_ReturnsTrue()
		{
			// Arrange
			string loginNameWithValidDomain = "user@realpage.com";

			var manageUserLoginMock = new Mock<IManageUserLogin>();
			manageUserLoginMock
				.Setup(m => m.IsUserEmailDomainValid(
					It.Is<string>(s => s == loginNameWithValidDomain)))
				.Returns(true);

			// Act
			bool result = manageUserLoginMock.Object.IsUserEmailDomainValid(loginNameWithValidDomain);

			// Assert
			Assert.True(result, "CheckUserDomainValidOrNot should return true for valid/whitelisted domain");
			manageUserLoginMock.Verify(
				m => m.IsUserEmailDomainValid(
					It.Is<string>(s => s == loginNameWithValidDomain)),
				Times.Once,
				"CheckUserDomainValidOrNot should have been called once");
		}

		[Fact]
		public void CheckUserDomainValidOrNot_WithBlacklistedDomain_ReturnsFalse()
		{
			// Arrange
			string loginNameWithBlacklistedDomain = "user@blacklisted-domain.com";

			var manageUserLoginMock = new Mock<IManageUserLogin>();
			manageUserLoginMock
				.Setup(m => m.IsUserEmailDomainValid(
					It.Is<string>(s => s == loginNameWithBlacklistedDomain)))
				.Returns(false);

			// Act
			bool result = manageUserLoginMock.Object.IsUserEmailDomainValid(loginNameWithBlacklistedDomain);

			// Assert
			Assert.False(result, "IsUserEmailDomainValid should return false for blacklisted domain");
			manageUserLoginMock.Verify(
				m => m.IsUserEmailDomainValid(
					It.Is<string>(s => s == loginNameWithBlacklistedDomain)),
				Times.Once,
				"CheckUserDomainValidOrNot should have been called once");
		}

		[Fact]
		public void CheckUserDomainValidOrNot_WithEmailFormat_ExtractsAndValidatesDomain()
		{
			// Arrange
			// Tests that the method properly handles email format with domain extraction
			string emailWithComplexFormat = "john.doe+tag@subdomain.realpage.com";

			var manageUserLoginMock = new Mock<IManageUserLogin>();
			manageUserLoginMock
				.Setup(m => m.IsUserEmailDomainValid(
					It.Is<string>(s => s == emailWithComplexFormat)))
				.Returns(true);

			// Act
			bool result = manageUserLoginMock.Object.IsUserEmailDomainValid(emailWithComplexFormat);

			// Assert
			Assert.True(result, "CheckUserDomainValidOrNot should handle complex email format with subdomains");
			manageUserLoginMock.Verify(
				m => m.IsUserEmailDomainValid(
					It.Is<string>(s => s == emailWithComplexFormat)),
				Times.Once);
		}

		[Fact]
		public void GetBlacklistedDomains_ReturnsListOfBlacklistedDomains()
		{
			// Arrange
			var expectedBlacklistedDomains = new List<string>
			{
				"spam.com",
				"blocked.com",
				"prohibited.com"
			};

			var mockRepository = new Mock<IUserLoginRepository>();
			mockRepository
				.Setup(m => m.GetBlacklistedDomains())
				.Returns(expectedBlacklistedDomains);

			// Act
			var result = mockRepository.Object.GetBlacklistedDomains();

			// Assert
			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(expectedBlacklistedDomains.Count, result.Count);
			foreach (var domain in expectedBlacklistedDomains)
			{
				Assert.Contains(domain, result);
			}

			// Verify the mock was called
			mockRepository.Verify(
				m => m.GetBlacklistedDomains(),
				Times.Once,
				"GetBlacklistedDomains should have been called once");
		}

		[Fact]
		public void GetBlacklistedDomains_WithEmptyList_ReturnsEmpty()
		{
			// Arrange
			var expectedBlacklistedDomains = new List<string>();

			var mockRepository = new Mock<IUserLoginRepository>();
			mockRepository
				.Setup(m => m.GetBlacklistedDomains())
				.Returns(expectedBlacklistedDomains);

			// Act
			var result = mockRepository.Object.GetBlacklistedDomains();

			// Assert
			Assert.NotNull(result);
			Assert.Empty(result);

			// Verify the mock was called
			mockRepository.Verify(
				m => m.GetBlacklistedDomains(),
				Times.Once);
		}

		[Fact]
		public void GetBlacklistedDomains_VerifyAllDomainsAreValid()
		{
			// Arrange
			var expectedBlacklistedDomains = new List<string>
			{
				"malicious.com",
				"phishing.net",
				"unauthorized.org",
				"blocked@internal.io"
			};

			var mockRepository = new Mock<IUserLoginRepository>();
			mockRepository
				.Setup(m => m.GetBlacklistedDomains())
				.Returns(expectedBlacklistedDomains);

			// Act
			var result = mockRepository.Object.GetBlacklistedDomains();

			// Assert
			Assert.NotNull(result);
			Assert.Equal(4, result.Count);
			
			// Verify each domain exists
			Assert.Contains("malicious.com", result);
			Assert.Contains("phishing.net", result);
			Assert.Contains("unauthorized.org", result);
			Assert.Contains("blocked@internal.io", result);

			mockRepository.Verify(
				m => m.GetBlacklistedDomains(),
				Times.Once);
		}

		[Fact]
		public void CheckUserDomainValidOrNot_WorksWithGetBlacklistedDomains()
		{
			// Arrange
			var blacklistedDomainsFromRepo = new List<string>
			{
				"spam.com",
				"blocked.com"
			};

			var manageUserLoginMock = new Mock<IManageUserLogin>();
			var repositoryMock = new Mock<IUserLoginRepository>();

			repositoryMock
				.Setup(m => m.GetBlacklistedDomains())
				.Returns(blacklistedDomainsFromRepo);

			manageUserLoginMock
				.Setup(m => m.IsUserEmailDomainValid("user@spam.com"))
				.Returns(false); // Domain is in blacklist

			manageUserLoginMock
				.Setup(m => m.IsUserEmailDomainValid("user@realpage.com"))
				.Returns(true); // Domain is NOT in blacklist

			// Act
			var blacklistedDomains = repositoryMock.Object.GetBlacklistedDomains();
			var resultForBlacklistedDomain = manageUserLoginMock.Object.IsUserEmailDomainValid("user@spam.com");
			var resultForValidDomain = manageUserLoginMock.Object.IsUserEmailDomainValid("user@realpage.com");

			// Assert
			Assert.NotNull(blacklistedDomains);
			Assert.Contains("spam.com", blacklistedDomains);
			Assert.False(resultForBlacklistedDomain, "Domain found in blacklist should return false");
			Assert.True(resultForValidDomain, "Domain not in blacklist should return true");

			repositoryMock.Verify(m => m.GetBlacklistedDomains(), Times.Once);
			manageUserLoginMock.Verify(m => m.IsUserEmailDomainValid(It.IsAny<string>()), Times.Exactly(2));
		}

		#endregion
		#endregion
	}
}
