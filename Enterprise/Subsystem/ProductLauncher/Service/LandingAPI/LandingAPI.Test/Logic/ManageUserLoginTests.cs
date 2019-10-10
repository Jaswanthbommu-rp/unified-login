using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Castle.Components.DictionaryAdapter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    /// <summary>
    /// UserLogin xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
	public class ManageUserLoginTests
	{
        Mock<IRepository> _mockRepository = new Mock<IRepository>();

        DefaultUserClaim userClaims = new DefaultUserClaim()
		{
			//UserId = 1,
			LoginName = "MocTest",
			CorrelationId = Guid.NewGuid(),
			OrganizationName = "MocTest",
			OrganizationPartyId = 1,
			OrganizationRealPageGuid = Guid.NewGuid(),
			OrganizationMasterId = 1,
			UserRealPageGuid = Guid.NewGuid(),
            PersonaId = 33,

		};
        private Guid _organizationRealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C");
        private string _loginName = "james@test.com";
        private Guid _userRealPageId = new Guid("9E9410AE-1111-2222-3333-109C08CD151C");

        private IList<UserOrganization> _userOrganizationList;

        private UserLoginOnly _userLoginOnly;
        private UserLogin _userLogin;
        
        private List<Persona> _personaList = new List<Persona>();
        private IList<Right> _rightList;
        private IList<Role> _roleList;
        private Persona _superUserPersona;
        private Persona _regularUserPersona;

        private Person _person;

        private RepositoryResponse _linkToIdentityProviderResponse;

        private List<OrganizationStatus> _orgStatusList;

        public ManageUserLoginTests()
        {
            _userOrganizationList = new List<UserOrganization>()
            {
                new UserOrganization()
                {
                    Name = "SuperUser",
                    OrganizationPartyId = 3,
                    OrganizationRealPageId = _organizationRealPageId,
                    PartyRoleTypeId = 402,
                }
            };

            OrganizationStatus organizationStatus = new OrganizationStatus()
            {
                PartyId = userClaims.OrganizationPartyId,
                IsPending = true,
                IsActive = true,
                IsExpired = false,
                StatusTypeId = (int)UserUiStatusType.Active,
                Status = UserUiStatusType.Active,
                FromDate = new DateTime(2019,1,1)
            };

            _orgStatusList = new EditableList<OrganizationStatus>() {organizationStatus};
            
            _userLoginOnly = new UserLoginOnly() {UserId = 1234, LoginName = _loginName, RealPageId = _userRealPageId};
            _userLogin = new UserLogin() { UserId = 1234, LoginName = _loginName, RealPageId = _userRealPageId };

            _person = new Person(){ FirstName = "Bob", LastName = "Jones", };

            _rightList = new List<Right>()
            {
                new Right()
                {
                    RightId = 672,
                    RightName = "Access Reports",
                    RightValueTypeId = 47,
                    RightNickName = "reports.view"
                },
                new Right()
                {
                    RightId = 667,
                    RightName = "Create master properties",
                    RightValueTypeId = 36,
                    RightNickName = "property.create"
                }
            };

            _roleList = new List<Role>()
            {
                new Role()
                {
                    RoleID = 81,
                    Name = "Black-Book Director",
                    PersonaId = "505",
                    Right = _rightList
                }
            };

            _superUserPersona = new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Super User",
                Organization = new Organization() { RealPageId = _organizationRealPageId },
                PersonaId = 33,
                RealPageId = _userRealPageId,
                ThruDate = DateTime.UtcNow.AddDays(1),
                UserId = 11,
                Role = _roleList,
                hasResidentPortalUserAccess = true
            };

            _personaList.Add(_superUserPersona);

            _regularUserPersona = new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Regular User",
                Organization = new Organization() { RealPageId = _superUserPersona.Organization.RealPageId },
                PersonaId = 1234,
                RealPageId = _userRealPageId,
                ThruDate = DateTime.UtcNow.AddDays(5),
                UserId = 1234,
                Role = _roleList,
                hasResidentPortalUserAccess = false
            };

            _personaList.Add(_regularUserPersona);

            _linkToIdentityProviderResponse = new RepositoryResponse() {Id = 1234, ErrorMessage = ""};

            _mockRepository.Setup(m => m.GetMany<UserOrganization> (StoredProcNameConstants.SP_ListOrganizationByLoginName, It.IsAny<object>()))
                .Returns(_userOrganizationList);

            _mockRepository.Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(_userLoginOnly);

            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, It.IsAny<object>()))
                .Returns(_linkToIdentityProviderResponse);

            _mockRepository.Setup(m => m.GetOne<UserLogin>(StoredProcNameConstants.SP_GetUserLogin, It.IsAny<object>()))
                .Returns(_userLogin);
            //return repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, param);

        }


		#region Unit Tests
		[Fact]
		public void CreateUserLogin_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin manageUserLogin = new ManageUserLogin();
			Guid realPageId = new Guid();

			//Act
			UserLogin userLogin = new UserLogin()
			{
				LoginName = "test@test.com",
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime(),
                Password = "somepassword"
			};
			Exception exception = Record.Exception(() => manageUserLogin.CreateUserLogin(realPageId, userLogin));

			//Assert
			Assert.IsType<Exception>(exception);
			Assert.Equal("Invalid parameter realPageId.", exception.Message);
		}

		[Fact]
		public void CreateUserLogin_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			UserLogin userLogin = new UserLogin()
			{
				LoginName = "test@test.com",
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime(),
                Password = "somepassword"
			};
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });
			//Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(_mockRepository.Object, userClaims);
            IRepositoryResponse repositoryResponse = manageUserLogin.CreateUserLogin(realPageId, userLogin);

			//Assert
			Assert.True(repositoryResponse.Id == 1);
			Assert.True(repositoryResponse.ErrorMessage == "");
			Assert.True(repositoryResponse.RealPageId == Guid.Empty);
		}

		[Fact]
		public void GetUserLogin_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin manageUserLogin = new ManageUserLogin();
			Guid realPageId = new Guid();

			//Act

			//Assert
			Assert.Throws<Exception>(() => manageUserLogin.GetUserLoginOnly(realPageId));
		}
        
        [Fact]
		public void GetUserLoginOnly__RealPageIdNotExistinDatabase_ReturnEmptyObject()
		{
			//Arrange
			Type type = typeof(UserLoginOnly);
			
            //Act
            int NumberOfProperties = type.GetProperties().Length;
            IManageUserLogin manageUserLogin = new ManageUserLogin(_mockRepository.Object, userClaims);
            var userLogin = manageUserLogin.GetUserLoginOnly(_userRealPageId);

			//Assert
			Assert.Equal(userLogin.UserId, _userLoginOnly.UserId);
			Assert.Equal(userLogin.PartyId, _userLoginOnly.PartyId);
			Assert.Equal(userLogin.LoginName, _userLoginOnly.LoginName);
			Assert.Equal(userLogin.PasswordHash, _userLoginOnly.PasswordHash);
			Assert.Equal(NumberOfProperties, 14);

            userLogin = manageUserLogin.GetUserLoginOnly(_userLoginOnly.UserId);
            Assert.Equal(userLogin.UserId, _userLoginOnly.UserId);
            Assert.Equal(userLogin.PartyId, _userLoginOnly.PartyId);
            Assert.Equal(userLogin.LoginName, _userLoginOnly.LoginName);
            Assert.Equal(userLogin.PasswordHash, _userLoginOnly.PasswordHash);

        }

        [Fact]
		public void CreateUserLogin_InvalidUserLoginOnject_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin manageUserLogin = new ManageUserLogin();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");

			//Act
			UserLogin userLogin = null;
			Exception exception = Record.Exception(() => manageUserLogin.CreateUserLogin(realPageId, userLogin));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
			Assert.Equal("Null UserLogin.\r\nParameter name: userLogin", exception.Message);
		}

		[Fact]
		public void UpdateUserLogin_InvalidRealPageId_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin manageUserLogin = new ManageUserLogin();
			Guid realPageId = new Guid();

			//Act
			UserLogin userLogin = new UserLogin()
			{
				LoginName = "test@test.com",
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime()
			};
			Exception exception = Record.Exception(() => manageUserLogin.UpdateUserLogin(realPageId, userLogin));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void UpdateUserLogin_InvalidUserLoginObject_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin manageUserLogin = new ManageUserLogin();
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");

			//Act
			UserLogin userLogin = null;
			Exception exception = Record.Exception(() => manageUserLogin.UpdateUserLogin(realPageId, userLogin));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
			#endregion
		}

		[Fact]
		public void UpdateUserLogin_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			long orgPartyId = 350;
			UserLogin userLogin = new UserLogin()
			{
				LoginName = "test@test.com",
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime(),
                Password = "somepassword",
                IsActive = true,
                RealPageId = realPageId

            };

            IdentityProviderType _expectedIdentityProviderType = new IdentityProviderType()
            {
                AuthenticationType = "local"
            };

            IList<Activity> activityList = new List<Activity>() {new Activity() {ActivityCode = "1", Description = "Test Activity", ActivityTypeId = (int)ActivityType.NewUserRegistration, ActivityTokenExpirationMinutes = 60}};

            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

            _mockRepository.Setup(m => m.GetOne<string>(StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName, It.IsAny<object>()))
                .Returns("local");

            _mockRepository.Setup(m => m.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, It.IsAny<object>()))
                .Returns(activityList);

            _mockRepository.Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, It.IsAny<object>()))
                .Returns(_orgStatusList);

            _mockRepository.Setup(m => m.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, It.IsAny<object>()))
                .Returns(_personaList);

            _mockRepository.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = "+ _superUserPersona.PersonaId+" }"))))
                .Returns(_superUserPersona);

            _mockRepository.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + _regularUserPersona.PersonaId + " }"))))
                .Returns(_regularUserPersona);

            _mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductBatch, It.IsAny<object>()))
                .Returns(new RepositoryResponse() {Id = 322});

            _mockRepository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.IsAny<object>()))
                .Returns(_person);

            //Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(_mockRepository.Object, userClaims);
            IRepositoryResponse repositoryResponse = manageUserLogin.UpdateUserLogin(realPageId, userLogin);

			//Assert
			Assert.True(repositoryResponse.Id == 1);
			Assert.True(repositoryResponse.ErrorMessage == "");
			Assert.True(repositoryResponse.RealPageId == Guid.Empty);
		}

        public bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
		public void IsLoginNameExists_InvalidOrganizationRealPageId_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin userLoginLogic = new ManageUserLogin();

			//Act
			string loginName = "james@test.com";
			Guid organizationRealPageId = new Guid();
            Guid userRealPageId = Guid.Empty;

			//Assert
			Assert.Throws<ArgumentNullException>(() => userLoginLogic.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId));
		}

		[Fact]
		public void IsLoginNameExists_InvalidLoginName_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin userLoginLogic = new ManageUserLogin();

			//Act
			string loginName = string.Empty;
			Guid organizationRealPageId = new Guid("9E9410AE-2C41-47D2-81D1-109C08CD151C");
            Guid userRealPageId = Guid.Empty;

            //Assert
            Assert.Throws<Exception>(() => userLoginLogic.IsLoginNameExists(loginName, organizationRealPageId, userRealPageId));
		}

		[Fact]
		public void IsLoginNameExists_MockInputData_ReturnValidUserOrganizationExists()
		{
			//Arrange
            _mockRepository.Setup(m => m.GetOne(StoredProcNameConstants.SP_GetPerson, It.IsAny<object>()))
                .Returns(new Person());

            //Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(_mockRepository.Object, userClaims);
            IUserOrganizationExists userOrganizationExists = manageUserLogin.IsLoginNameExists(_loginName, _organizationRealPageId, _userRealPageId);

			//Assert
			Assert.True(
				userOrganizationExists.UserExists == true
				&& userOrganizationExists.UserExistsInThisOrganization == true
			);
		}

        //
        [Fact]
        public void LinkIdentityProviderToUserLogin_ValidAndErrors()
        {
            //Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(_mockRepository.Object, userClaims);
            var response = manageUserLogin.LinkIdentityProviderToUserLogin(24, 25, 2);

            //Assert
            Assert.True(
                string.IsNullOrEmpty(response.ErrorMessage)
                && response.Id == _linkToIdentityProviderResponse.Id
            );

            // Errors
            Exception exception = Record.Exception(() => manageUserLogin.LinkIdentityProviderToUserLogin(-1, 1, 1));

            //Assert
            Assert.IsType<Exception>(exception);
            Assert.Equal("Missing Persona Id.", exception.Message);

            exception = Record.Exception(() => manageUserLogin.LinkIdentityProviderToUserLogin(1, -1, 1));
            Assert.IsType<Exception>(exception);
            Assert.Equal("Missing UserLogin Id.", exception.Message);

            exception = Record.Exception(() => manageUserLogin.LinkIdentityProviderToUserLogin(1, 1, -1));
            Assert.IsType<Exception>(exception);
            Assert.Equal("Missing Contact Mechanism Id.", exception.Message);
        }
    }
}
