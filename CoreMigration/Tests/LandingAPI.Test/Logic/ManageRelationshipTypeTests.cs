using UnifiedLogin.BusinessLogic;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Landing;
using System.Net.Http;
using UnifiedLogin.SharedObjects;
using FluentAssertions;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    public class ManageRelationshipTypeTests
    {
        private Mock<IRepository> _mockRepository = new Mock<IRepository>();
        private Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
        private Persona _persona;
        private List<OrganizationType> _organizationTypeList;
        private List<OrganizationDomain> _organizationDomainList;
        private List<UserRelationShipType> _userRelationTypesListAdmin;
        private Organization _organization;
        private long _employeePersonaId = 1234;
        private long _externalPersonaId = 7890;
        private Organization _organizationExternal;
        private Persona _personaExternal;

        public ManageRelationshipTypeTests()
        {
            _organization = new Organization()
            {
                PartyId = 10639,
                Name = "RealPage Employee",
                RealPageId = new Guid("9D2471B7-468B-4BC2-8F84-08C9521F354A"),
                CreateDate = DateTime.Parse("2018-01-16 16:51:40.277"),
                BooksMasterId = -1,
                OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" }
            };
            _persona = new Persona()
            {
                PersonaId = _employeePersonaId,
                PersonPartyId = 10662,
                RealPageId = new Guid("9C155420-2B7A-40F7-A9AE-E71BC68EEC50"),
                OrganizationPartyId = _organization.PartyId,
                PersonaTypeId = 3,
                PersonaEnvironmentTypeId = 1,
                Name = "primary",
                FromDate = DateTime.Parse("2018-01-26 03:01:33.763"),
                ThruDate = null,
                IsDefault = false,
                UserId = 499,
                Organization = _organization
            };
            _organizationExternal = new Organization()
            {
                PartyId = 350,
                Name = "CF Real Estate Services",
                RealPageId = new Guid("A6C00C21-23F8-4E76-902D-B210E74F8717"),
                CreateDate = DateTime.Parse("2018-01-16 16:51:40.277"),
                BooksMasterId = -1,
                OrganizationDomain = new OrganizationDomain() { OrganizationDomainId = 1, Name = "Primary" }
            };
            _personaExternal = new Persona()
            {
                PersonaId = _externalPersonaId,
                PersonPartyId = 7895,
                RealPageId = new Guid("0200E973-3D92-4DD3-BEC5-A094C7D98E09"),
                OrganizationPartyId = _organizationExternal.PartyId,
                PersonaTypeId = 3,
                PersonaEnvironmentTypeId = 1,
                Name = "primary",
                FromDate = DateTime.Parse("2018-01-26 03:01:33.763"),
                ThruDate = null,
                IsDefault = false,
                UserId = 399,
                UserTypeId = 405,
                Organization = _organizationExternal
            };

            _organizationTypeList = new List<OrganizationType>()
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
            _organizationDomainList = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };

            _userRelationTypesListAdmin = new List<UserRelationShipType>()
            {
                new UserRelationShipType()
                {
                    Id = 2,
                    UserRelationshipName = "Employee",
                    Description = "Employee user with email format username",
                    PartyRoleTypeId = 401,
                    ThirdPartyRelationshipId = 4,
                    SortIndex =10
                },
                new UserRelationShipType()
                {
                    Id = 4,
                    UserRelationshipName = "System Administrator",
                    Description = "Company Super User",
                    PartyRoleTypeId = 402,
                    ThirdPartyRelationshipId = 4,
                    SortIndex =40
                 },
                 new UserRelationShipType()
                 {
                      Id = 8,
                    UserRelationshipName = "RealPage Employee",
                    Description = "Employee user with email format username",
                    PartyRoleTypeId = 403,
                    ThirdPartyRelationshipId = 4,
                    SortIndex =80
                 }

            };
        }

        [Fact]
        public void Employee_UserRelationShipTypeList()
        {
            //Arrange
            var defaultUserClaim = new DefaultUserClaim()
            {
                PersonaId = _employeePersonaId,
                OrganizationPartyId = 10639
            };

            _mockRepository
                .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + _persona.PersonaId + " }"))))
                .Returns(_persona);

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsPartyId(d, _organization.PartyId))))
                .Returns(_organization);

            _mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            _mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomainList);

            _mockRepository
                .Setup(m => m.GetMany<UserRelationShipType>(StoredProcNameConstants.SP_ListUserRelationshipTypes,
                It.Is<object>(d => TestIsPartyId(d, _organization.PartyId))))
                 .Returns(_userRelationTypesListAdmin);


            //Act
            var manageRelationTypes = new ManageRelationshipType(_mockRepository.Object, defaultUserClaim, mockMessageHandler.Object);
            var manageUserRelationTypeList = manageRelationTypes.GetUserRelationShipTypes();

            //Asserts
            manageUserRelationTypeList.Count().Should().Be(3);
        }
        [Fact]
        public void External_GetUserRelationShipType_Successful()
        {
            //Arrange
            var defaultUserClaim = new DefaultUserClaim()
            {
                PersonaId = _externalPersonaId,
                OrganizationPartyId = 350,
                IsRPEmployee = false
            };
            _mockRepository
                .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId = " + _personaExternal.PersonaId + " }"))))
                .Returns(_personaExternal);

            _mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestIsPartyId(d, _organizationExternal.PartyId))))
                .Returns(_organizationExternal);

            _mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            _mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomainList);

            _mockRepository
                .Setup(m => m.GetMany<UserRelationShipType>(StoredProcNameConstants.SP_ListUserRelationshipTypes,
                It.Is<object>(
                        d => TestIsPartyId(d, _organizationExternal.PartyId))))
                .Returns(_userRelationTypesListAdmin);

            //Act
            var manageRelationTypes = new ManageRelationshipType(_mockRepository.Object, defaultUserClaim, mockMessageHandler.Object);
            var manageUserRelationTypeList = manageRelationTypes.GetUserRelationShipTypes();

            //Asserts
            manageUserRelationTypeList.Count().Should().Be(2);
        }

        [Fact]
        public void GetUserRelationTypeIsNull()
        {

            //Arrange
            var defaultUserClaim = new DefaultUserClaim()
            {
                PersonaId = 1234,
                OrganizationPartyId = 10639
            };
            Persona nullPersona = null;
            _mockRepository
                .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(data => TestSqlParameter(data, "{ personaId =  1234 }"))))
                .Returns(nullPersona);


            //Act
            var manageRelationTypes = new ManageRelationshipType(_mockRepository.Object, defaultUserClaim, mockMessageHandler.Object);
            var manageUserRelationTypeList = manageRelationTypes.GetUserRelationShipTypes();

            //Asserts
            Assert.True(manageUserRelationTypeList == null);

        }

        public bool TestSqlParameter(object p, string value)
        {
            return value.Equals(p.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private bool TestIsPartyId(object obj, long partyid)
        {
            if (obj == null)
            {
                return false;
            }
            return obj.ToString().ToLower().Contains($"partyid = {partyid}");
        }
    }
}
