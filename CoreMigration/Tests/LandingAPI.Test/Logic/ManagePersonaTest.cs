using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UserManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	/// <summary>
	/// Persona xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ManagePersonaTest
    {
        #region Private Variables
        IManagePersona _managePersona;
        readonly Mock<IPersonaRepository> _mockPersonaRepository = new Mock<IPersonaRepository>();
	    private BooksMaster _booksMaster = new BooksMaster();

        private List<Persona> _personaList = new List<Persona>();
        private IList<Right> _rightList;
        private IList<Role> _roleList;

        private Persona _superUserPersona;
        private Persona _regularUserPersona;

        private IList<PersonaEnvironment> _personaEnvironments;

        private Guid _companyRealPageId;
        private Guid _superUserRealPageId;
        private Guid _regularUserRealPageId;

        #endregion

        public ManagePersonaTest()
        {
            AssertInitial();
        }

        private void AssertInitial()
        {
            //Arrange
            _booksMaster = new BooksMaster() {BlackBookId = 1234, BlueBookId = 4321};
            
            _companyRealPageId = new Guid("3C927308-372D-4728-A7E0-F896F8691618");
            _superUserRealPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");
            _regularUserRealPageId = new Guid("0D9CB762-8C18-4A20-B971-E0FAC891B2D8");

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
                Organization = new Organization() {RealPageId = _superUserRealPageId},
                PersonaId = 33,
                RealPageId = _superUserRealPageId,
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
                Organization = new Organization() {RealPageId = _superUserRealPageId},
                PersonaId = 1234,
                RealPageId = _regularUserRealPageId,
                ThruDate = DateTime.UtcNow.AddDays(5),
                UserId = 1234,
                Role = _roleList,
                hasResidentPortalUserAccess = false
            };

            _personaList.Add(_regularUserPersona);

            _personaEnvironments = new List<PersonaEnvironment>() {new PersonaEnvironment() {PersonaEnvironmentTypeId = 1, Name = "Production"}, new PersonaEnvironment() {PersonaEnvironmentTypeId = 2, Name = "UAT"}};
            
            //_mockOrganizationLogic
            //    .Setup(m => m.GetBooksCompanyMaster(_multifamilyCompanyRealPageId))
            //    .Returns(_booksMaster);
        }


        //[Fact]
        //public void GetActivePersonaId_ReturnPersonaId()
        //{
        //    //Arrange
        //    long personaId = _superUserPersona.PersonaId;
        //
        //    _mockPersonaRepository
        //        .Setup(m => m.GetActivePersonaId(_superUserRealPageId))
        //        .Returns(personaId);
        //
        //    _managePersona = new ManagePersona(_mockPersonaRepository.Object, _mockOrganizationLogic.Object);
        //
        //    //Act
        //
        //    //Assert
        //    Assert.True(personaId == _managePersona.GetActivePersonaId(_superUserRealPageId));
        //}

        [Fact]
        public void GetListPersona_ReturnPersonaList()
        {
            //Arrange
            _mockPersonaRepository
                .Setup(m => m.ListPersona(_superUserRealPageId))
                .Returns(_personaList);

            _managePersona = new ManagePersona(_mockPersonaRepository.Object);

            //Act

            //Assert
            IList<Persona> personaListResult = _managePersona.ListPersona(_superUserRealPageId);
            Assert.True(_personaList.Count == personaListResult.Count
                && _personaList[0].PersonaId == personaListResult[0].PersonaId
                && _personaList[0].RealPageId == personaListResult[0].RealPageId
                && _personaList[0].Name == personaListResult[0].Name
                && _personaList[0].UserId == personaListResult[0].UserId
                );
        }

        [Fact]
        public void GetListPersonaByOrganizationPartyId_ReturnPersonaList()
        {
            //Arrange
            _mockPersonaRepository
                .Setup(m => m.ListPersonaByOrganizationPartyId(_superUserPersona.PersonaId, It.IsAny<bool>(), null))
                .Returns(_personaList);

            _managePersona = new ManagePersona(_mockPersonaRepository.Object);

            //Act

            //Assert
            IList<Persona> personaListResult = _managePersona.ListPersonaByOrganizationPartyId(_superUserPersona.PersonaId, false);
            Assert.True(_personaList.Count == personaListResult.Count
                        && _personaList[0].PersonaId == personaListResult[0].PersonaId
                        && _personaList[0].RealPageId == personaListResult[0].RealPageId
                        && _personaList[0].Name == personaListResult[0].Name
                        && _personaList[0].UserId == personaListResult[0].UserId
            );
        }

        [Fact]
        public void UpdateActivePersona_ReturnEmptyResponse()
        {
            //Arrange
            RepositoryResponse rr = new RepositoryResponse() {ErrorMessage = ""};

            _mockPersonaRepository
                .Setup(m => m.UpdateActivePersona(It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns(rr);

            _managePersona = new ManagePersona(_mockPersonaRepository.Object);

            //Act

            //Assert
            Assert.True(String.IsNullOrEmpty(_managePersona.UpdateActivePersona(_superUserPersona.RealPageId, _superUserPersona.PersonaId).ErrorMessage));
        }

        [Fact]
        public void GetActivePersona_ReturnPersona()
        {
            //Arrange
            _mockPersonaRepository
                .Setup(m => m.GetActivePersonaId(_superUserRealPageId))
                .Returns(_superUserPersona.PersonaId);

            _mockPersonaRepository
                .Setup(m => m.GetPersona(It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(_superUserPersona);

            //Act
            _managePersona = new ManagePersona(_mockPersonaRepository.Object);
            Persona suPersona = _managePersona.GetActivePersona(_superUserPersona.RealPageId);

            //Assert
            //Assert.True(_superUserPersona.PersonaId == _managePersona.GetActivePersonaId(_superUserRealPageId));

            Assert.True(suPersona.PersonaId == _superUserPersona.PersonaId);
        }

        [Fact]
        public void GetPersonaEnvironmentType_ReturnTypes()
        {
            //Arrange
            _mockPersonaRepository
                .Setup(m => m.GetPersonaEnvironmentType())
                .Returns(_personaEnvironments);

            //Act
            _managePersona = new ManagePersona(_mockPersonaRepository.Object);
            IList<PersonaEnvironment> environments = _managePersona.GetPersonaEnvironmentType();
            
            //Assert
            Assert.True(environments.Count == _personaEnvironments.Count 
                && environments[0].PersonaEnvironmentTypeId == _personaEnvironments[0].PersonaEnvironmentTypeId
                && environments[0].Name == _personaEnvironments[0].Name
                && environments[1].PersonaEnvironmentTypeId == _personaEnvironments[1].PersonaEnvironmentTypeId
                && environments[1].Name == _personaEnvironments[1].Name
            );
        }


        [Fact]
        public void GetPersona_InvalidPersonaId_ExceptionThrown()
        {
            //Arrange
            long personaId = 0;

            _managePersona = new ManagePersona(_mockPersonaRepository.Object);

            //Act
            Exception exception = Record.Exception(() => _managePersona.GetPersona(personaId));

            //Assert
            Assert.IsType<Exception>(exception);
        }

        [Fact]
        public void GetPersona_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
			Type type = typeof(Persona);
            int numberOfProperties = 0;
            Guid realPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");
            long personaId = _superUserPersona.PersonaId;

			Persona expectedPersona = new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Super User",
                Organization = new Organization() { RealPageId = realPageId },
                PersonaId = personaId,
                RealPageId = realPageId,
                ThruDate = DateTime.UtcNow.AddDays(1),
				UserId = 11,
				Role = _roleList,
                hasResidentPortalUserAccess = true
            };

            _mockPersonaRepository
                .Setup(m => m.GetPersona(personaId, true))
                .Returns(expectedPersona);

            _managePersona = new ManagePersona(_mockPersonaRepository.Object);

            //Act
            Persona persona = _managePersona.GetPersona(personaId);
            numberOfProperties = type.GetProperties().Length;

            //Assert
            Assert.True(persona != null
                && persona.PersonaId == personaId
                && persona.RealPageId == realPageId
                && numberOfProperties >= 14);
        }

        [Fact]
        public void ListPersona_InvalidRealPageId_ExceptionThrown()
        {
            //Arrange
            Guid emptyRealPageId = Guid.Empty;

            _managePersona = new ManagePersona(_mockPersonaRepository.Object);

            //Act
            Exception exception = Record.Exception(() => _managePersona.ListPersona(emptyRealPageId));

            //Assert
            Assert.IsType<Exception>(exception);
        }

        [Fact]
        public void ListPersona_MockInputData_ReturnValidRepositoryResponseObject()
        {
            //Arrange
            Type type = typeof(Persona);
            int numberOfProperties = 0;
            Guid realPageId = new Guid("C9167175-0676-4546-BBA7-4A49D5809B1F");
            long personaId = 33;
            IList<Persona> expectedPersonaList = new List<Persona>();

			
            expectedPersonaList.Add(new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Super User",
                PersonaId = personaId,
                RealPageId = realPageId,
                ThruDate = DateTime.UtcNow.AddDays(1),
				UserId = 11,
				Role = _roleList
            });

            _mockPersonaRepository
                .Setup(m => m.ListPersona(realPageId))
                .Returns(expectedPersonaList);

            _managePersona = new ManagePersona(_mockPersonaRepository.Object);

            //Act
            IList<Persona> personaList = _managePersona.ListPersona(realPageId);
            numberOfProperties = type.GetProperties().Length;

            //Assert
            Assert.True(personaList != null
                && personaList.Count == 1
                && personaList == expectedPersonaList
                && numberOfProperties >= 13);
        }
    }
}
