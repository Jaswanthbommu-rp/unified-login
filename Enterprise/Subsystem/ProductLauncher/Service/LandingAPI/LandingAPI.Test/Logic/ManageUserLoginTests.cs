using Moq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
    /// <summary>
    /// UserLogin xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
	public class ManageUserLoginTests : TestBase
	{
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;

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

        private List<OrganizationDomain> organizationDomainList;
        private List<OrganizationType> organizationTypeList;

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
            organizationDomainList = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };
            organizationTypeList = new List<OrganizationType>()
            {
                new OrganizationType()
                {
                    OrganizationTypeId = 6,
                    Name = "Multifamily",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 14,
                    Name = "Vendor",
                    CreateDate = new DateTime()
                },
                new OrganizationType()
                {
                    OrganizationTypeId = 7,
                    Name = "Other",
                    CreateDate = new DateTime()
                }
            };

            _orgStatusList = new List<OrganizationStatus>() {organizationStatus};
            
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

            mockRepository.Setup(m => m.GetMany<UserOrganization> (StoredProcNameConstants.SP_ListOrganizationByLoginName, It.IsAny<object>()))
                .Returns(_userOrganizationList);

            mockRepository.Setup(m => m.GetOne<UserLoginOnly>(StoredProcNameConstants.SP_GetUserLoginOnly, It.IsAny<object>()))
                .Returns(_userLoginOnly);

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkIdentityProviderToUserLogin, It.IsAny<object>()))
                .Returns(_linkToIdentityProviderResponse);

            mockRepository.Setup(m => m.GetOne<UserLogin>(StoredProcNameConstants.SP_GetUserLogin, It.IsAny<object>()))
                .Returns(_userLogin);

            mockRepository.Setup(m => m.GetMany<string>(StoredProcNameConstants.SP_GetBlacklistedDomains, It.IsAny<object>()))
                .Returns(new List<string>());

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        }

		#region Unit Tests
		[Fact]
		public void CreateUserLogin_InvalidrealPageId_ExceptionThrown()
		{
			//Arrange
			IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
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
            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });
            
            //Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
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
			IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
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
            IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
            var userLogin = manageUserLogin.GetUserLoginOnly(_userRealPageId);

			//Assert
			Assert.Equal(userLogin.UserId, _userLoginOnly.UserId);
			Assert.Equal(userLogin.PartyId, _userLoginOnly.PartyId);
			Assert.Equal(userLogin.LoginName, _userLoginOnly.LoginName);
			Assert.Equal(userLogin.PasswordHash, _userLoginOnly.PasswordHash);
			Assert.Equal(13, NumberOfProperties);

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
			IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
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
			IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
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
			IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");

			//Act
			UserLogin userLogin = null;
			Exception exception = Record.Exception(() => manageUserLogin.UpdateUserLogin(realPageId, userLogin));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void UpdateUserLogin_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			Guid realPageId = new Guid("13E71DE5-BAFA-469D-9F7A-E12DB3961BA9");
			UserLogin userLogin = new UserLogin()
			{
				LoginName = "test@test.com",
				FromDate = DateTime.UtcNow,
				ThruDate = DateTime.MaxValue.ToUniversalTime(),
                Password = "somepassword",
                IsActive = true,
                RealPageId = realPageId

            };

			IList<Organization> organizationList = new List<Organization>()
			{
				new Organization()
				{
					RealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D"),
					CreateDate = DateTime.MaxValue.ToUniversalTime(),
					Name = "CF Real Estate Services",
					PartyId = 54321,
					BooksMasterId = 2116,
					BooksCustomerMasterId = 379,
					OrganizationTypeId = 6,
					organizationType = new OrganizationType()
					{
						OrganizationTypeId = 6
					}
				}
			};

			IdentityProviderType _expectedIdentityProviderType = new IdentityProviderType()
            {
                AuthenticationType = "local"
            };

            IList<Activity> activityList = new List<Activity>() {new Activity() {ActivityCode = "1", Description = "Test Activity", ActivityTypeId = (int)ActivityType.NewUserRegistration, ActivityTokenExpirationMinutes = 60}};

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUserLogin, It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

            mockRepository.Setup(m => m.GetOne<string>(StoredProcNameConstants.SP_GetIdentityProviderTypeByLoginName, It.IsAny<object>()))
                .Returns("local");

            mockRepository.Setup(m => m.GetMany<Activity>(StoredProcNameConstants.SP_ListActivity, It.IsAny<object>()))
                .Returns(activityList);

            mockRepository.Setup(m => m.GetMany<OrganizationStatus>(StoredProcNameConstants.SP_ListOrganizationStatusByUserId, It.IsAny<object>()))
                .Returns(_orgStatusList);

            mockRepository.Setup(m => m.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, It.IsAny<object>()))
                .Returns(_personaList);

            mockRepository.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = "+ _superUserPersona.PersonaId+" }"))))
                .Returns(_superUserPersona);

            mockRepository.Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + _regularUserPersona.PersonaId + " }"))))
                .Returns(_regularUserPersona);

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateProductBatch, It.IsAny<object>()))
                .Returns(new RepositoryResponse() {Id = 322});

            mockRepository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.IsAny<object>()))
                .Returns(_person);

			mockRepository
				.Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
				.Returns(organizationList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(organizationTypeList);

            //Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
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
			IManageUserLogin userLoginLogic = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);

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
			IManageUserLogin userLoginLogic = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);

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
            List<RoleType> roleTypes = new List<RoleType>()
            {
                new RoleType() {PartyRoleTypeId = 401, Name = "User", ParentPartyRoleTypeId = 400},
                new RoleType() {PartyRoleTypeId = 402, Name = "SuperUser", ParentPartyRoleTypeId = 400},
                new RoleType() {PartyRoleTypeId = 403, Name = "RealPage Employee", ParentPartyRoleTypeId = 400},
                new RoleType() {PartyRoleTypeId = 404, Name = "User(No Email)", ParentPartyRoleTypeId = 400},
                new RoleType() {PartyRoleTypeId = 405, Name = "External User", ParentPartyRoleTypeId = 400}
            };

            Organization organizationList = new Organization()
            {
                RealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99"),
                CreateDate = DateTime.MaxValue.ToUniversalTime(),
                Name = "RealPage Employee",
                PartyId = 54321,
                BooksMasterId = 2116,
                BooksCustomerMasterId = 379,
                OrganizationTypeId = 6,
                OrganizationDomainId = 1,
                organizationType = new OrganizationType()
                {
                    OrganizationTypeId = 6
                }
            };
            UserInfoLite superVisor = new UserInfoLite()
            {
                FirstName = "FName",
                LastName = "LName",
                OrganizationPartyId = 54321,
                LoginName = "fname@lname.com",
                UserId = 1234,
                SuperVisorUserId = 234,
                IsReadOnly = true
            };

            Person p = new Person() {FirstName = "First", LastName = "Last", PartyId = 1234, RealPageId = _userRealPageId};

            //Arrange
            mockRepository.Setup(m => m.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, It.IsAny<object>()))
                .Returns(p);

            mockRepository.Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.IsAny<object>()))
                .Returns(roleTypes);

            mockRepository
               .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
               .Returns(organizationTypeList);
            mockRepository
               .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
               .Returns(organizationDomainList);
            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization, It.IsAny<object>()))
                .Returns(organizationList);
            mockRepository
                .Setup(m => m.GetOne<UserInfoLite>(StoredProcNameConstants.SP_GetSuperVisorId, It.IsAny<object>()))
                .Returns(superVisor);

            //Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
            UserOrganizationExists userOrganizationExists = manageUserLogin.IsLoginNameExists(_loginName, _organizationRealPageId, _userRealPageId);

			//Assert
			Assert.True(
				userOrganizationExists.UserExists == true
                && userOrganizationExists.UserExistsAsNoEmail == false
                && userOrganizationExists.UserExistsInThisOrganization == true
                && userOrganizationExists.UserExistsNotAvailable == false
                && userOrganizationExists.UserIsDisabledInPrimaryCompany == false
                && userOrganizationExists.UserIsExternalEverywhere == false
                && userOrganizationExists.OrgIsRealpageEmployee == true
            );

            RPObjectCache rpCache = new RPObjectCache();
            rpCache.BustCache();

            roleTypes = new List<RoleType>()
            {
                new RoleType() {PartyRoleTypeId = 401, Name = "User", ParentPartyRoleTypeId = 400},
                new RoleType() {PartyRoleTypeId = 402, Name = "SuperUser", ParentPartyRoleTypeId = 400},
                new RoleType() {PartyRoleTypeId = 403, Name = "RealPage Employee", ParentPartyRoleTypeId = 400},
                new RoleType() {PartyRoleTypeId = 404, Name = "User(No Email)", ParentPartyRoleTypeId = 400},
            };

            mockRepository.Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.IsAny<object>()))
                .Returns(roleTypes);

            userOrganizationExists = manageUserLogin.IsLoginNameExists(_loginName, _organizationRealPageId, _userRealPageId);
            Assert.True(
                userOrganizationExists.UserExists == true
                && userOrganizationExists.UserExistsAsNoEmail == false
                && userOrganizationExists.UserExistsInThisOrganization == true
                && userOrganizationExists.UserExistsNotAvailable == true
                && userOrganizationExists.UserIsDisabledInPrimaryCompany == false
                && userOrganizationExists.UserIsExternalEverywhere == false
                 && userOrganizationExists.OrgIsRealpageEmployee == true
            );
        }

        [Fact]
        public void LinkIdentityProviderToUserLogin_ValidAndErrors()
        {
            //Act
            IManageUserLogin manageUserLogin = new ManageUserLogin(mockRepository.Object, userClaims, _mockHttpMessageHandler.Object);
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

        #endregion
    }
}
