using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
    /// <summary>
    /// Profile xUnit tests
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RoleTypeTest : TestBase
    {
        
        private readonly List<RoleType> _userRoleTypes;
        private readonly List<RoleType> _allRoleTypes;
        private readonly List<OrganizationType> _organizationTypeList;
        private readonly List<OrganizationDomain> _organizationDomainList;
        private readonly List<Organization> _organizationList;

        private readonly Persona _normalUserPersona;
        private readonly Persona _operatorUserPersona;

        private readonly long _normalUserPersonaId = 12345;
        private readonly long _operatorUserPersonaId = 44444;

        private readonly long _operatorUserUserId = 9999;

        private readonly Guid _normalUserRealPageId = new Guid("11111111-5553-4444-6666-565656565656");
        private readonly Guid _operatorUserRealPageId = new Guid("22222222-5553-4444-6666-565656565656");
        private static Guid _organizationRealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static string _CompanyName = "CF Real Estate Services";
        private static int _companyPartyId = 54321;
        private static long _BooksMasterId = 2116;
        private static long _BooksCompanyMasterId = 379;
        private static int _organizationTypeId = 6;
        private static int _organizationDomainId = 1;

        /// <summary>
        /// 
        /// </summary>
        public RoleTypeTest()
        {
            _allRoleTypes = new List<RoleType>()
            {
                new RoleType() { PartyRoleTypeId = 312, ParentPartyRoleTypeId = 300, Name = "Chief Executive Officer" },
                new RoleType() { PartyRoleTypeId = 301, ParentPartyRoleTypeId = 300, Name = "Accounts Payable" },
                new RoleType() { PartyRoleTypeId = 343, ParentPartyRoleTypeId = 300, Name = "Property Manager" },
                new RoleType() { PartyRoleTypeId = 204, ParentPartyRoleTypeId = 200, Name = "Site" },
                new RoleType() { PartyRoleTypeId = 401, ParentPartyRoleTypeId = 400, Name = "User" },
                new RoleType() { PartyRoleTypeId = 402, ParentPartyRoleTypeId = 400, Name = "SuperUser" },
                new RoleType() { PartyRoleTypeId = 403, ParentPartyRoleTypeId = 400, Name = "RealPage Employee" },
                new RoleType() { PartyRoleTypeId = 404, ParentPartyRoleTypeId = 400, Name = "User (No Email)" },
                new RoleType() { PartyRoleTypeId = 405, ParentPartyRoleTypeId = 400, Name = "External User" },
            };

            _userRoleTypes = new List<RoleType>()
            {
                new RoleType() { PartyRoleTypeId = 401, ParentPartyRoleTypeId = 0, Name = "User" },
                new RoleType() { PartyRoleTypeId = 402, ParentPartyRoleTypeId = 0, Name = "SuperUser" },
                new RoleType() { PartyRoleTypeId = 404, ParentPartyRoleTypeId = 0, Name = "User (No Email)" },
                new RoleType() { PartyRoleTypeId = 405, ParentPartyRoleTypeId = 0, Name = "External User" },

            };

            _normalUserPersona = new Persona()
            {
                PersonaId = _normalUserPersonaId,
                RealPageId = _normalUserRealPageId,
                Organization = new Organization() { Name = "Test Company", PartyId = 1234 },
                Name = "Title",
                OrganizationPartyId = 1234,
                UserTypeId = 402
            };

            _operatorUserPersona = new Persona()
            {
                PersonaId = _operatorUserPersonaId,
                RealPageId = _operatorUserRealPageId,
                Organization = new Organization() { Name = "Test Company", PartyId = 1234 },
                Name = "Title",
                OrganizationPartyId = 1234,
                UserTypeId = 405
            };

            _organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = _organizationRealPageId,
                    CreateDate = _CreateDate,
                    Name = _CompanyName,
                    PartyId = _companyPartyId,
                    BooksMasterId = _BooksMasterId,
                    BooksCustomerMasterId = _BooksCompanyMasterId,
                    organizationType = new OrganizationType()
                    {
                        OrganizationTypeId = _organizationTypeId,
                        Name = "Multifamily",
                        CreateDate = new DateTime()
                    },
                    OrganizationTypeId = _organizationTypeId,
                    OrganizationDomainId = 1,
                    OrganizationDomain = new OrganizationDomain()
                    {
                        OrganizationDomainId = 1,
                        Name = "Primary",
                        CreateDate = new DateTime()
                    },
                }
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
        }

        [Fact]
        public void GetListRoleType_Success_NoCompanyPartyId_NoRoleType()
        {
            //Arrange
            var userClaims = new DefaultUserClaim() { OrganizationPartyId = 0, };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(data => TestSqlParameter(data, null))))
                .Returns(_allRoleTypes);


            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(data => TestSqlParameter(data, "{ personaId =  }"))))
                .Returns(_userRoleTypes);

            //Act
            var roleTypeController = new RoleTypeController(mockRepository.Object, mockHttpMessageHandler.Object, userClaims) { Request = new HttpRequestMessage(), Configuration = new HttpClient() };

            var response = roleTypeController.ListRoleType();

            var roleTypeResult = JsonConvert.DeserializeObject<ObjectListOutput<RoleType, IErrorData>>(response.Content.ReadAsStringAsync().Result);

            //Assert
            roleTypeResult.list.Count.Should().Be(_allRoleTypes.Count - 1);
        }

        [Fact]
        public void GetListRoleType_Success_WithCompanyPartyId_NoRoleType()
        {
            //Arrange
            var userClaims = new DefaultUserClaim() { OrganizationPartyId = 1234, };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(data => TestSqlParameter(data, null))))
                .Returns(_allRoleTypes);


            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(data => TestSqlParameter(data, "{ personaId =  }"))))
                .Returns(_userRoleTypes);

            //Act
            var roleTypeController = new RoleTypeController(mockRepository.Object, mockHttpMessageHandler.Object, userClaims) { Request = new HttpRequestMessage(), Configuration = new HttpClient() };

            var response = roleTypeController.ListRoleType();

            var roleTypeResult = JsonConvert.DeserializeObject<ObjectListOutput<RoleType, IErrorData>>(response.Content.ReadAsStringAsync().Result);

            //Assert
            roleTypeResult.list.Count.Should().Be(_allRoleTypes.Count);
        }

        [Fact]
        public void GetListRoleType_Success_WithCompanyPartyId_UserRoleType_NotOperator()
        {
            //Arrange
            var userClaims = new DefaultUserClaim() { OrganizationPartyId = _normalUserPersona.OrganizationPartyId, PersonaId = _normalUserPersona.PersonaId };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleTypeDependency, It.Is<object>(
                    data => TestSqlParameter(data, "{ PartyId = 1234, ParentRoleTypeID = 402 }"))))
                .Returns(_userRoleTypes);

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(
                    data => TestSqlParameter(data, "{ personaId = " + userClaims.PersonaId + " }"))))
                .Returns(_userRoleTypes);

            mockRepository
                .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(
                    d => d.ToString().Contains($"personaId = {userClaims.PersonaId}"))))
                .Returns(_normalUserPersona);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ RealPageId = , PartyId = 1234 }"))))
                .Returns(_organizationList[0]);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomainList);

            //Act
            var roleTypeController = new RoleTypeController(mockRepository.Object, mockHttpMessageHandler.Object, userClaims) { Request = new HttpRequestMessage(), Configuration = new HttpClient() };

            var response = roleTypeController.ListRoleType("user role");

            var roleTypeResult = JsonConvert.DeserializeObject<ObjectListOutput<RoleType, IErrorData>>(response.Content.ReadAsStringAsync().Result);

            //Assert
            roleTypeResult.list.Count.Should().Be(_userRoleTypes.Count);
        }


        [Fact]
        public void GetListRoleType_Success_WithCompanyPartyId_UserRoleType_Operator()
        {
            //Arrange
            var userClaims = new DefaultUserClaim() { UserId = (int)_operatorUserUserId, OrganizationPartyId = _operatorUserPersona.OrganizationPartyId, PersonaId = _operatorUserPersona.PersonaId };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var userLoginPersonaList = new List<UserLoginPersona>() { new UserLoginPersona()
            {
                PrimaryOrganization = true,
                IsDelegateAdmin = false,
                UserLoginId = _operatorUserUserId,
                UserLoginPersonaId = 12345
            }};

            var externalUserRelationship = new ExternalUserRelationship()
            {
                UserLoginPersonaId = 12345,
                OperatorCode = "Operator Code",
                OperatorValue = "Operator Value",
                ThirdPartyRelationShipId = 1
            };

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleTypeDependency, It.Is<object>(
                    data => TestSqlParameter(data, "{ PartyId = 1234, ParentRoleTypeID = " + _operatorUserPersona.UserTypeId + " }"))))
                .Returns(_userRoleTypes);

            mockRepository
                .Setup(m => m.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, It.Is<object>(
                    data => TestSqlParameter(data, "{ personaId = " + userClaims.PersonaId + " }"))))
                .Returns(_userRoleTypes);

            mockRepository
                .Setup(m => m.GetOne<Persona>(StoredProcNameConstants.SP_GetPersona, It.Is<object>(
                    d => d.ToString().Contains($"personaId = {userClaims.PersonaId}"))))
                .Returns(_operatorUserPersona);

            mockRepository
                .Setup(m => m.GetOne<Organization>(StoredProcNameConstants.SP_GetOrganization,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ RealPageId = , PartyId = 1234 }"))))
                .Returns(_organizationList[0]);

            mockRepository
                .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
                .Returns(_organizationTypeList);

            mockRepository
                .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
                .Returns(_organizationDomainList);

            mockRepository
                .Setup(m => m.GetMany<UserLoginPersona>(StoredProcNameConstants.SP_GetUserLoginPersona,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginId = " + userClaims.UserId + ", OrganizationPartyId = " + _operatorUserPersona.OrganizationPartyId + " }"))
                ))
                .Returns(userLoginPersonaList);
            mockRepository
                .Setup(m => m.GetOne<ExternalUserRelationship>(StoredProcNameConstants.SP_GetExternalUserRelationship,
                    It.Is<object>(
                        d => TestSqlParameter(d, "{ UserLoginPersonaId = " + userLoginPersonaList[0].UserLoginPersonaId + " }"))
                ))
                .Returns(externalUserRelationship);

            //Act
            var roleTypeController = new RoleTypeController(mockRepository.Object, mockHttpMessageHandler.Object, userClaims) { Request = new HttpRequestMessage(), Configuration = new HttpClient() };

            var response = roleTypeController.ListRoleType("user role");

            var roleTypeResult = JsonConvert.DeserializeObject<ObjectListOutput<RoleType, IErrorData>>(response.Content.ReadAsStringAsync().Result);

            //Assert
            roleTypeResult.list.Count.Should().Be(1);
            roleTypeResult.list[0].Name.Should().Be("External User");
        }

        private bool TestSqlParameter(object p, string value)
        {
            if (p == null && value == null) return true;

            return value.Equals(p?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
