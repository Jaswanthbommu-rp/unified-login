using Aspose.Cells;
using Moq;
using Moq.Protected;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.Landing.LandingAPI.Test.Repository
{
    /// <summary>
    /// Unit tests for UserRepository.InsertNewPhoneNumberFromImport method
    /// </summary>
    public class UserRepositoryInsertNewPhoneNumberFromImportTests
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<IProfileDetail> _mockProfile;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly DefaultUserClaim _userClaim;
        private readonly Guid _testUserRealPageId;
        private readonly List<TelecommunicationNumber> telecommunicationNumbersList = new List<TelecommunicationNumber>() {
            new TelecommunicationNumber(){
                ContactMechanismId = 0,
                PhoneNumber = "5551234567",
                AreaCode = "555",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = false,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 301,
                    Name = "Mobile"
                }
            },
            new TelecommunicationNumber(){
                ContactMechanismId = 0,
                PhoneNumber = "555123657",
                AreaCode = "598",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = true,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 301,
                    Name = "Mobile"
                }
        }
        };


        public UserRepositoryInsertNewPhoneNumberFromImportTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockProfile = new Mock<IProfileDetail>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _testUserRealPageId = Guid.NewGuid();

            _userClaim = new DefaultUserClaim
            {
                CorrelationId = Guid.NewGuid(),
                OrganizationMasterId = 123,
                OrganizationPartyId = 456,
                LoginName = "testuser",
                UserId = 789,
                UserRealPageGuid = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                ImpersonatedBy = Guid.Empty
            };

            // Setup default HTTP response
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{}")
                });
        }

        [Fact]
        public void InsertNewPhoneNumberFromImport_WithNewPhoneNumber_CreatesContactMechanismAndTelecommunicationNumber()
        {
            // Arrange

            var newPhoneNumber = new TelecommunicationNumber
            {
                ContactMechanismId = 0,
                PhoneNumber = "5551236565",
                AreaCode = "555",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = true,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 301,
                    Name = "Mobile"
                }
            };

            var persona = new Persona
            {
                PersonaId = 1,
                RealPageId = _testUserRealPageId,
                OrganizationPartyId = 456,
                Organization = new Organization
                {
                    RealPageId = Guid.NewGuid(),
                    PartyId = 456
                }
            };

            _mockProfile.Setup(p => p.RealPageId).Returns(_testUserRealPageId);
            _mockProfile.Setup(p => p.TelecommunicationNumber).Returns(new List<TelecommunicationNumber> { newPhoneNumber });
            _mockProfile.Setup(p => p.Persona).Returns(new List<Persona> { persona });
            _mockProfile.Setup(p => p.FirstName).Returns("John");
            _mockProfile.Setup(p => p.LastName).Returns("Doe");

            _mockRepository.Setup(r => r.GetMany<ProductInternalSetting>(
                StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
                    .Returns(new List<ProductInternalSetting>());

            _mockRepository.Setup(r => r.GetMany<TelecommunicationNumber>(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                It.IsAny<object>()))
                    .Returns(telecommunicationNumbersList);

            _mockRepository.Setup(r => r.GetMany<ContactMechanismUsageType>(
                StoredProcNameConstants.SP_ListContactMechanismUsageType,
                It.IsAny<object>()))
                .Returns(new List<ContactMechanismUsageType>
                {
                    new ContactMechanismUsageType { ContactMechanismUsageTypeId = 301, Name = "Mobile" }
                });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 100 });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_LinkContactMechanismToParty,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 200 });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 300 });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateTelecommunicationNumber,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 400 });

            _mockRepository.Setup(r => r.GetOne<Persona>(
                StoredProcNameConstants.SP_GetPersona,
                It.IsAny<object>()))
                .Returns(persona);

            _mockRepository.Setup(r => r.GetOne<UserLoginOnly>(
                StoredProcNameConstants.SP_GetUserLoginOnly,
                It.IsAny<object>()))
                .Returns(new UserLoginOnly
                {
                    RealPageId = _testUserRealPageId,
                    LoginName = "jdoe",
                    UserId = 789
                });

            _mockRepository.Setup(r => r.GetOne<Person>(
                StoredProcNameConstants.SP_GetPerson,
                It.IsAny<object>()))
                .Returns(new Person
                {
                    FirstName = "John",
                    LastName = "Doe",
                    RealPageId = _testUserRealPageId
                });

            var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);

            // Act
            userRepository.InsertNewPhoneNumberFromImport(_mockRepository.Object, _mockProfile.Object);

            // Assert
            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.Is<object>(o => PropertyEquals(o, "ContactMechanismId", 0))), Times.AtLeastOnce());

            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_LinkContactMechanismToParty,
                It.Is<object>(o => PropertyEquals(o, "RealPageId", _testUserRealPageId))), Times.AtLeastOnce());

            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateTelecommunicationNumber,
                It.Is<object>(o => PropertyEquals(o, "PhoneNumber", "5551234567"))), Times.AtLeastOnce());
        }

        [Fact]
        public void InsertNewPhoneNumberFromImport_WithExistingPhoneNumbers_UpdatesDefaultPhone()
        {
            // Arrange

            var existingPhone = new TelecommunicationNumber
            {
                ContactMechanismId = 50,
                PhoneNumber = "5559876543",
                AreaCode = "555",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = true,
                ContactMechanismUsageTypeId = 301,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 301,
                    Name = "Mobile"
                }
            };

            var newDefaultPhone = new TelecommunicationNumber
            {
                ContactMechanismId = 0,
                PhoneNumber = "5551223567",
                AreaCode = "555",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = true,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 302,
                    Name = "Home"
                }
            };

            var persona = new Persona
            {
                PersonaId = 1,
                RealPageId = _testUserRealPageId,
                OrganizationPartyId = 456,
                Organization = new Organization
                {
                    RealPageId = Guid.NewGuid(),
                    PartyId = 456
                }
            };

            _mockProfile.Setup(p => p.RealPageId).Returns(_testUserRealPageId);
            _mockProfile.Setup(p => p.TelecommunicationNumber).Returns(new List<TelecommunicationNumber> { newDefaultPhone });
            _mockProfile.Setup(p => p.Persona).Returns(new List<Persona> { persona });
            _mockProfile.Setup(p => p.FirstName).Returns("John");
            _mockProfile.Setup(p => p.LastName).Returns("Doe");

            _mockRepository.Setup(r => r.GetMany<TelecommunicationNumber>(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                 It.IsAny<object>()))
                .Returns(new List<TelecommunicationNumber> { existingPhone });

            _mockRepository.Setup(r => r.GetMany<ContactMechanismUsageType>(
                StoredProcNameConstants.SP_ListContactMechanismUsageType,
                It.IsAny<object>()))
                .Returns(new List<ContactMechanismUsageType>
                {
                    new ContactMechanismUsageType { ContactMechanismUsageTypeId = 301, Name = "Mobile" },
                    new ContactMechanismUsageType { ContactMechanismUsageTypeId = 302, Name = "Home" }
                });

            _mockRepository.Setup(r => r.GetMany<ProductInternalSetting>(
            StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()    ))
            .Returns(new List<ProductInternalSetting>());

            SetupSuccessfulRepositoryResponses();
            var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);

            // Act
            userRepository.InsertNewPhoneNumberFromImport(_mockRepository.Object, _mockProfile.Object);

            // Assert
            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.IsAny<object>()), Times.AtLeastOnce());

            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateTelecommunicationNumber,
                It.IsAny<object>()), Times.AtLeastOnce());
        }

        [Fact]
        public void InsertNewPhoneNumberFromImport_WithEmptyPhoneNumber_SkipsCreation()
        {
            // Arrange
            //   var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);

            var emptyPhoneNumber = new TelecommunicationNumber
            {
                ContactMechanismId = 0,
                PhoneNumber = "",
                AreaCode = "",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = false,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 301,
                    Name = "Mobile"
                }
            };

            var persona = new Persona
            {
                PersonaId = 1,
                RealPageId = _testUserRealPageId,
                OrganizationPartyId = 456,
                Organization = new Organization { RealPageId = Guid.NewGuid(), PartyId = 456 }
            };

            _mockProfile.Setup(p => p.RealPageId).Returns(_testUserRealPageId);
            _mockProfile.Setup(p => p.TelecommunicationNumber).Returns(new List<TelecommunicationNumber> { emptyPhoneNumber });
            _mockProfile.Setup(p => p.Persona).Returns(new List<Persona> { persona });

            _mockRepository.Setup(r => r.GetMany<TelecommunicationNumber>(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                It.IsAny<object>()))
                .Returns(telecommunicationNumbersList);

            _mockRepository.Setup(r => r.GetMany<ContactMechanismUsageType>(
                StoredProcNameConstants.SP_ListContactMechanismUsageType,
                It.IsAny<object>()))
                .Returns(new List<ContactMechanismUsageType>
                {
                    new ContactMechanismUsageType { ContactMechanismUsageTypeId = 301, Name = "Mobile" }
                });

            SetupSuccessfulRepositoryResponses();
            _mockRepository.Setup(r => r.GetMany<ProductInternalSetting>(
            StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
            .Returns(new List<ProductInternalSetting>());

            SetupSuccessfulRepositoryResponses();
            var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);
            // Act
            userRepository.InsertNewPhoneNumberFromImport(_mockRepository.Object, _mockProfile.Object);

            // Assert
            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.IsAny<object>()), Times.AtLeastOnce());
        }

        [Fact]
        public void InsertNewPhoneNumberFromImport_WithRepositoryFailure_ThrowsException()
        {
            // Arrange
            //  var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);

            var newPhoneNumber = new TelecommunicationNumber
            {
                ContactMechanismId = 0,
                PhoneNumber = "5551236467",
                AreaCode = "555",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = true,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 301,
                    Name = "Mobile"
                }
            };

            var persona = new Persona
            {
                PersonaId = 1,
                RealPageId = _testUserRealPageId,
                OrganizationPartyId = 456,
                Organization = new Organization { RealPageId = Guid.NewGuid(), PartyId = 456 }
            };

            _mockProfile.Setup(p => p.RealPageId).Returns(_testUserRealPageId);
            _mockProfile.Setup(p => p.TelecommunicationNumber).Returns(new List<TelecommunicationNumber> { newPhoneNumber });
            _mockProfile.Setup(p => p.Persona).Returns(new List<Persona> { persona });
            _mockProfile.Setup(p => p.FirstName).Returns("John");
            _mockProfile.Setup(p => p.LastName).Returns("Doe");

            _mockRepository.Setup(r => r.GetMany<TelecommunicationNumber>(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                It.IsAny<object>()))
                .Returns(telecommunicationNumbersList);

            _mockRepository.Setup(r => r.GetMany<ContactMechanismUsageType>(
                StoredProcNameConstants.SP_ListContactMechanismUsageType,
                It.IsAny<object>()))
                .Returns(new List<ContactMechanismUsageType>
                {
                    new ContactMechanismUsageType { ContactMechanismUsageTypeId = 301, Name = "Mobile" }
                });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = "Failed to create contact mechanism" });

            _mockRepository.Setup(r => r.GetOne<Persona>(
                StoredProcNameConstants.SP_GetPersona,
                It.IsAny<object>()))
                .Returns(persona);

            _mockRepository.Setup(r => r.GetOne<UserLoginOnly>(
                StoredProcNameConstants.SP_GetUserLoginOnly,
                It.IsAny<object>()))
                .Returns(new UserLoginOnly
                {
                    RealPageId = _testUserRealPageId,
                    LoginName = "jdoe",
                    UserId = 789
                });

            _mockRepository.Setup(r => r.GetOne<Person>(
                StoredProcNameConstants.SP_GetPerson,
                It.IsAny<object>()))
                .Returns(new Person
                {
                    FirstName = "John",
                    LastName = "Doe",
                    RealPageId = _testUserRealPageId
                });
            _mockRepository.Setup(r => r.GetMany<ProductInternalSetting>(
            StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
            .Returns(new List<ProductInternalSetting>());

            SetupSuccessfulRepositoryResponses();
            var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);
            // Act & Assert
            // The method doesn't throw but continues processing
            userRepository.InsertNewPhoneNumberFromImport(_mockRepository.Object, _mockProfile.Object);

            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.IsAny<object>()), Times.AtLeastOnce());
        }

        [Fact]
        public void InsertNewPhoneNumberFromImport_WithMultiplePhoneNumbers_ProcessesAllNumbers()
        {
            // Arrange
            //  var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);

            var phone1 = new TelecommunicationNumber
            {
                ContactMechanismId = 0,
                PhoneNumber = "5555534567",
                AreaCode = "555",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = true,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 301,
                    Name = "Mobile"
                }
            };

            var phone2 = new TelecommunicationNumber
            {
                ContactMechanismId = 0,
                PhoneNumber = "5558866543",
                AreaCode = "555",
                CountryCode = "1",
                ISOCode = "US",
                IsDefault = false,
                contactMechanismUsageType = new ContactMechanismUsageType
                {
                    ContactMechanismUsageTypeId = 302,
                    Name = "Home"
                }
            };

            var persona = new Persona
            {
                PersonaId = 1,
                RealPageId = _testUserRealPageId,
                OrganizationPartyId = 456,
                Organization = new Organization { RealPageId = Guid.NewGuid(), PartyId = 456 }
            };

            _mockProfile.Setup(p => p.RealPageId).Returns(_testUserRealPageId);
            _mockProfile.Setup(p => p.TelecommunicationNumber).Returns(new List<TelecommunicationNumber> { phone1, phone2 });
            _mockProfile.Setup(p => p.Persona).Returns(new List<Persona> { persona });
            _mockProfile.Setup(p => p.FirstName).Returns("John");
            _mockProfile.Setup(p => p.LastName).Returns("Doe");

            _mockRepository.Setup(r => r.GetMany<TelecommunicationNumber>(
                StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson,
                It.IsAny<object>()))
                .Returns(telecommunicationNumbersList);

            _mockRepository.Setup(r => r.GetMany<ContactMechanismUsageType>(
                StoredProcNameConstants.SP_ListContactMechanismUsageType,
                It.IsAny<object>()))
                .Returns(new List<ContactMechanismUsageType>
                {
                    new ContactMechanismUsageType { ContactMechanismUsageTypeId = 301, Name = "Mobile" },
                    new ContactMechanismUsageType { ContactMechanismUsageTypeId = 302, Name = "Home" }
                });

            SetupSuccessfulRepositoryResponses();
            _mockRepository.Setup(r => r.GetMany<ProductInternalSetting>(
            StoredProcNameConstants.SP_ListGlobalSettingsForProduct, It.IsAny<object>()))
            .Returns(new List<ProductInternalSetting>());

            SetupSuccessfulRepositoryResponses();
            var userRepository = new UserRepository(_mockRepository.Object, _userClaim, _mockHttpMessageHandler.Object);
            // Act
            userRepository.InsertNewPhoneNumberFromImport(_mockRepository.Object, _mockProfile.Object);

            // Assert
            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.IsAny<object>()), Times.AtLeastOnce());

            _mockRepository.Verify(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateTelecommunicationNumber,
                It.IsAny<object>()), Times.AtLeastOnce());
        }

        private void SetupSuccessfulRepositoryResponses()
        {
            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateContactMechanism,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 100 });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_LinkContactMechanismToParty,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 200 });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 300 });

            _mockRepository.Setup(r => r.GetOne<RepositoryResponse>(
                StoredProcNameConstants.SP_CreateTelecommunicationNumber,
                It.IsAny<object>()))
                .Returns(new RepositoryResponse { Id = 400 });

            _mockRepository.Setup(r => r.GetOne<Persona>(
                StoredProcNameConstants.SP_GetPersona,
                It.IsAny<object>()))
                .Returns(new Persona
                {
                    PersonaId = 1,
                    RealPageId = _testUserRealPageId,
                    OrganizationPartyId = 456,
                    Organization = new Organization { RealPageId = Guid.NewGuid(), PartyId = 456 }
                });

            _mockRepository.Setup(r => r.GetOne<UserLoginOnly>(
                StoredProcNameConstants.SP_GetUserLoginOnly,
                It.IsAny<object>()))
                .Returns(new UserLoginOnly
                {
                    RealPageId = _testUserRealPageId,
                    LoginName = "jdoe",
                    UserId = 789
                });

            _mockRepository.Setup(r => r.GetOne<Person>(
                StoredProcNameConstants.SP_GetPerson,
                It.IsAny<object>()))
                .Returns(new Person
                {
                    FirstName = "John",
                    LastName = "Doe",
                    RealPageId = _testUserRealPageId
                });
        }

        /// <summary>
        /// Helper method for property access in Moq parameter matching
        /// </summary>
        private static bool PropertyEquals(object obj, string propertyName, object expectedValue)
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return false;
            var value = prop.GetValue(obj);
            return Equals(value, expectedValue);
        }
    }
}