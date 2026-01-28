using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for PersonController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PersonControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManagePerson> _mockManagePerson;
        private readonly Mock<IManageProfile> _mockManageProfile;
        private readonly Mock<IManagePersona> _mockManagePersona;
        private readonly Mock<IManageCustomFields> _mockManageCustomFields;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IManageUserLogin> _mockManageUserLogin;
        private readonly Mock<IManageUnifiedSettings> _mockManageUnifiedSettings;
        private PersonController _personController;

        #endregion

        #region Constructor

        public PersonControllerTests()
        {
            _mockManagePerson = new Mock<IManagePerson>();
            _mockManageProfile = new Mock<IManageProfile>();
            _mockManagePersona = new Mock<IManagePersona>();
            _mockManageCustomFields = new Mock<IManageCustomFields>();
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockManageUserLogin = new Mock<IManageUserLogin>();
            _mockManageUnifiedSettings = new Mock<IManageUnifiedSettings>();

            _personController = new PersonController(
                _mockManagePerson.Object,
                _mockManageProfile.Object,
                _mockManagePersona.Object,
                _mockManageCustomFields.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUserLogin.Object,
                _mockManageUnifiedSettings.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new PersonController(
                _mockManagePerson.Object,
                _mockManageProfile.Object,
                _mockManagePersona.Object,
                _mockManageCustomFields.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUserLogin.Object,
                _mockManageUnifiedSettings.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullManagePerson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonController(
                null!,
                _mockManageProfile.Object,
                _mockManagePersona.Object,
                _mockManageCustomFields.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUserLogin.Object,
                _mockManageUnifiedSettings.Object));
        }

        [Fact]
        public void Constructor_WithNullManageProfile_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonController(
                _mockManagePerson.Object,
                null!,
                _mockManagePersona.Object,
                _mockManageCustomFields.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUserLogin.Object,
                _mockManageUnifiedSettings.Object));
        }

        [Fact]
        public void Constructor_WithNullManagePersona_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonController(
                _mockManagePerson.Object,
                _mockManageProfile.Object,
                null!,
                _mockManageCustomFields.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUserLogin.Object,
                _mockManageUnifiedSettings.Object));
        }

        [Fact]
        public void Constructor_WithNullManageCustomFields_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonController(
                _mockManagePerson.Object,
                _mockManageProfile.Object,
                _mockManagePersona.Object,
                null!,
                _mockUserClaimsAccessor.Object,
                _mockManageUserLogin.Object,
                _mockManageUnifiedSettings.Object));
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonController(
                _mockManagePerson.Object,
                _mockManageProfile.Object,
                _mockManagePersona.Object,
                _mockManageCustomFields.Object,
                null!,
                _mockManageUserLogin.Object,
                _mockManageUnifiedSettings.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUserLogin_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonController(
                _mockManagePerson.Object,
                _mockManageProfile.Object,
                _mockManagePersona.Object,
                _mockManageCustomFields.Object,
                _mockUserClaimsAccessor.Object,
                null!,
                _mockManageUnifiedSettings.Object));
        }

        [Fact]
        public void Constructor_WithNullManageUnifiedSettings_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PersonController(
                _mockManagePerson.Object,
                _mockManageProfile.Object,
                _mockManagePersona.Object,
                _mockManageCustomFields.Object,
                _mockUserClaimsAccessor.Object,
                _mockManageUserLogin.Object,
                null!));
        }

        #endregion

        #region CreatePerson Tests

        [Fact]
        public async Task CreatePerson_WithValidPerson_ReturnsOkResult()
        {
            // Arrange
            var person = new Person { FirstName = "John", LastName = "Doe" };
            var expectedRealPageId = Guid.NewGuid();

            _mockManagePerson
                .Setup(x => x.CreatePerson(person))
                .Returns(new RepositoryResponse { Id = 1, RealPageId = expectedRealPageId });

            // Act
            var result = await _personController.CreatePerson(person);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var outputResult = Assert.IsType<Person.PersonOutputResult>(okResult.Value);
            Assert.Equal(expectedRealPageId, outputResult.RealPageId);
        }

        [Fact]
        public async Task CreatePerson_WithNullPerson_ReturnsBadRequest()
        {
            // Act
            var result = await _personController.CreatePerson(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: Person.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreatePerson_WhenRepositoryReturnsZeroId_ReturnsBadRequest()
        {
            // Arrange
            var person = new Person { FirstName = "John", LastName = "Doe" };
            const string errorMessage = "Failed to create person";

            _mockManagePerson
                .Setup(x => x.CreatePerson(person))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _personController.CreatePerson(person);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task CreatePerson_CallsServiceWithCorrectPerson()
        {
            // Arrange
            var person = new Person { FirstName = "Jane", LastName = "Smith" };

            _mockManagePerson
                .Setup(x => x.CreatePerson(person))
                .Returns(new RepositoryResponse { Id = 1, RealPageId = Guid.NewGuid() });

            // Act
            await _personController.CreatePerson(person);

            // Assert
            _mockManagePerson.Verify(x => x.CreatePerson(person), Times.Once);
        }

        #endregion

        #region GetPerson Tests

        [Fact]
        public async Task GetPerson_WithValidRealPageId_ReturnsOkWithPerson()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedPerson = new Person { FirstName = "John", LastName = "Doe" };

            _mockManagePerson
                .Setup(x => x.GetPerson(realPageId))
                .Returns(expectedPerson);

            // Act
            var result = await _personController.GetPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPerson, IErrorData>>(okResult.Value);
            Assert.NotNull(output.obj);
            Assert.True(output.Status.Success);
        }

        [Fact]
        public async Task GetPerson_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(userRealPageId);

            var expectedPerson = new Person { FirstName = "John", LastName = "Doe" };

            _mockManagePerson
                .Setup(x => x.GetPerson(userRealPageId))
                .Returns(expectedPerson);

            // Act
            var result = await _personController.GetPerson(Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPerson, IErrorData>>(okResult.Value);
            Assert.NotNull(output.obj);
            _mockManagePerson.Verify(x => x.GetPerson(userRealPageId), Times.Once);
        }

        [Fact]
        public async Task GetPerson_WithEmptyGuidAndEmptyUserClaims_ReturnsOkWithError()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);

            // Act
            var result = await _personController.GetPerson(Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPerson, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Person.GetPerson.1", output.Status.ErrorCode);
            Assert.Equal("Get Person: Invalid parameter enterprise User Id", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetPerson_WhenPersonNotFound_ReturnsOkWithError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManagePerson
                .Setup(x => x.GetPerson(realPageId))
                .Returns((Person)null!);

            // Act
            var result = await _personController.GetPerson(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPerson, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Person.GetPerson.2", output.Status.ErrorCode);
            Assert.Equal("Get Person: Invalid enterprise User Id", output.Status.ErrorMsg);
        }

        #endregion

        #region UpdatePerson Tests

        [Fact]
        public async Task UpdatePerson_WithValidData_ReturnsOkWithPerson()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { FirstName = "John", LastName = "Updated" };

            _mockManagePerson
                .Setup(x => x.UpdatePerson(realPageId, person))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _personController.UpdatePerson(realPageId, person);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPerson = Assert.IsType<Person>(okResult.Value);
            Assert.Equal("Updated", returnedPerson.LastName);
        }

        [Fact]
        public async Task UpdatePerson_WithEmptyGuid_UsesUserClaimsRealPageId()
        {
            // Arrange
            var userRealPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(userRealPageId);

            var person = new Person { FirstName = "John", LastName = "Doe" };

            _mockManagePerson
                .Setup(x => x.UpdatePerson(userRealPageId, person))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _personController.UpdatePerson(Guid.Empty, person);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockManagePerson.Verify(x => x.UpdatePerson(userRealPageId, person), Times.Once);
        }

        [Fact]
        public async Task UpdatePerson_WithEmptyGuidAndEmptyUserClaims_ReturnsBadRequest()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(Guid.Empty);
            var person = new Person { FirstName = "John", LastName = "Doe" };

            // Act
            var result = await _personController.UpdatePerson(Guid.Empty, person);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePerson_WithNullPerson_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            // Act
            var result = await _personController.UpdatePerson(realPageId, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: Person", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdatePerson_WhenRepositoryReturnsZeroId_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var person = new Person { FirstName = "John", LastName = "Doe" };
            const string errorMessage = "Failed to update person";

            _mockManagePerson
                .Setup(x => x.UpdatePerson(realPageId, person))
                .Returns(new RepositoryResponse { Id = 0, ErrorMessage = errorMessage });

            // Act
            var result = await _personController.UpdatePerson(realPageId, person);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(errorMessage, badRequestResult.Value);
        }

        #endregion

        #region ListPersons Tests

        [Fact]
        public async Task ListPersons_WithValidDataFilter_ReturnsOkWithList()
        {
            // Arrange
            var datafilter = new RequestParameter();
            var profileList = new List<ProfileDetail>
            {
                CreateValidProfileDetail(1),
                CreateValidProfileDetail(2)
            };

            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            _mockManageProfile
                .Setup(x => x.ListProfileDetails(It.IsAny<IDictionary<object, object>>(), null))
                .Returns(profileList);

            // Act
            var result = await _personController.ListPersons(datafilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectUserListOutput<ProfileDetail, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
        }

        [Fact]
        public async Task ListPersons_WithNullDataFilter_CreatesDefaultFilter()
        {
            // Arrange
            var profileList = new List<ProfileDetail>();

            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            _mockManageProfile
                .Setup(x => x.ListProfileDetails(It.IsAny<IDictionary<object, object>>(), null))
                .Returns(profileList);

            // Act
            var result = await _personController.ListPersons(null!);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectUserListOutput<ProfileDetail, IErrorData>>(okResult.Value);
            Assert.NotNull(output);
        }

        [Fact]
        public async Task ListPersons_CalculatesPagingSummaryCorrectly()
        {
            // Arrange
            var datafilter = new RequestParameter { Pages = new PageRequest { ResultsPerPage = 10 } };
            var profileList = new List<ProfileDetail>
            {
                new ProfileDetail { TotalRecords = 25, userLogin = new UserLogin { Status = UserUiStatusType.Active } }
            };

            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            _mockManageProfile
                .Setup(x => x.ListProfileDetails(It.IsAny<IDictionary<object, object>>(), null))
                .Returns(profileList);

            // Act
            var result = await _personController.ListPersons(datafilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectUserListOutput<ProfileDetail, IErrorData>>(okResult.Value);
            Assert.Equal(25, output.pagingSummary.TotalRecords);
            Assert.Equal(3, output.pagingSummary.TotalPages); // 25 / 10 = 2.5 -> ceil = 3
        }

        [Fact]
        public async Task ListPersons_SetsOrganizationHasProductAssignmentError()
        {
            // Arrange
            var datafilter = new RequestParameter();
            var profileList = new List<ProfileDetail>
            {
                new ProfileDetail
                {
                    TotalRecords = 1,
                    PersonaHasProductError = true,
                    userLogin = new UserLogin { Status = UserUiStatusType.Active }
                }
            };

            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            _mockManageProfile
                .Setup(x => x.ListProfileDetails(It.IsAny<IDictionary<object, object>>(), null))
                .Returns(profileList);

            // Act
            var result = await _personController.ListPersons(datafilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectUserListOutput<ProfileDetail, IErrorData>>(okResult.Value);
            Assert.True(output.OrganizationHasProductAssignmentError);
        }

        #endregion

        #region ListPersonsByOrg Tests

        [Fact]
        public async Task ListPersonsByOrg_WithValidData_ReturnsOkWithList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var datafilter = new RequestParameter { Pages = new PageRequest { ResultsPerPage = 10 } };
            var profileList = new List<ProfileDetail>
            {
                new ProfileDetail { TotalRecords = 5 }
            };

            _mockManageProfile
                .Setup(x => x.ListProfileDetails(It.IsAny<IDictionary<object, object>>(), realPageId))
                .Returns(profileList);

            // Act
            var result = await _personController.ListPersonsByOrg(realPageId, datafilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProfileDetail, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task ListPersonsByOrg_WithNullDataFilter_CreatesDefaultFilter()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var profileList = new List<ProfileDetail>();

            _mockManageProfile
                .Setup(x => x.ListProfileDetails(It.IsAny<IDictionary<object, object>>(), realPageId))
                .Returns(profileList);

            // Act
            var result = await _personController.ListPersonsByOrg(realPageId, null!);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region GetActivePersona Tests

        [Fact]
        public async Task GetActivePersona_WithValidRealPageId_ReturnsOkWithPersona()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedPersona = new Persona { PersonaId = 12345, Name = "Test Persona" };
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(100);
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            _mockManagePersona
                .Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 100))
                .Returns(expectedPersona);

            // Act
            var result = await _personController.GetActivePersona(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(okResult.Value);
            Assert.NotNull(output.obj);
            Assert.Equal(12345, output.obj.PersonaId);
        }

        [Fact]
        public async Task GetActivePersona_WithEmptyGuid_ReturnsOkWithError()
        {
            // Act
            var result = await _personController.GetActivePersona(Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Person.GetActivePersona.1", output.Status.ErrorCode);
            Assert.Equal("Get active persona: Invalid parameter enterprise User Id", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetActivePersona_WhenPersonaNotFound_ReturnsOkWithError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(100);
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            _mockManagePersona
                .Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 100))
                .Returns((Persona)null!);

            // Act
            var result = await _personController.GetActivePersona(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Person.GetActivePersona.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task GetActivePersona_WhenPersonaIdIsZero_ReturnsOkWithError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = new Persona { PersonaId = 0 };
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(100);
            _mockUserClaimsAccessor.Setup(x => x.GetUserClaim()).Returns(new DefaultUserClaim());

            _mockManagePersona
                .Setup(x => x.GetFirstAvailablePersonaByCompany(realPageId, 100))
                .Returns(persona);

            // Act
            var result = await _personController.GetActivePersona(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IPersona, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Person.GetActivePersona.2", output.Status.ErrorCode);
        }

        #endregion

        #region GetListOfPersona Tests

        [Fact]
        public async Task GetListOfPersona_WithValidRealPageId_ReturnsOkWithList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var personaList = new List<Persona>
            {
                new Persona { PersonaId = 1, Name = "Persona 1" },
                new Persona { PersonaId = 2, Name = "Persona 2" }
            };

            _mockManagePersona
                .Setup(x => x.ListPersona(realPageId))
                .Returns(personaList);

            // Act
            var result = await _personController.GetListOfPersona(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IList<Persona>>(okResult.Value);
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetListOfPersona_WithEmptyGuid_ReturnsBadRequest()
        {
            // Act
            var result = await _personController.GetListOfPersona(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task GetListOfPersona_CallsServiceWithCorrectRealPageId()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManagePersona
                .Setup(x => x.ListPersona(realPageId))
                .Returns(new List<Persona>());

            // Act
            await _personController.GetListOfPersona(realPageId);

            // Assert
            _mockManagePersona.Verify(x => x.ListPersona(realPageId), Times.Once);
        }

        #endregion

        #region Helper Methods

        private static ProfileDetail CreateValidProfileDetail(int index)
        {
            return new ProfileDetail
            {
                FirstName = $"FirstName{index}",
                LastName = $"LastName{index}",
                TotalRecords = 2,
                userLogin = new UserLogin
                {
                    LoginName = $"user{index}@test.com",
                    Status = UserUiStatusType.Active
                },
                SummaryCount = new SummaryCounts { TotalAssignedProducts = index },
                SuperVisorUser = new UserInfoLite()
            };
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _personController = null!;
            base.Dispose();
        }

        #endregion
    }
}













