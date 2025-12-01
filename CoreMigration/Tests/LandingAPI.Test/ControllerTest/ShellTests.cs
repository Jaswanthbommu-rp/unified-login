using Moq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
    [ExcludeFromCodeCoverage]
    public class ShellTests : TestBase
    {
        private DefaultUserClaim _userClaim = new DefaultUserClaim() { CorrelationId = Guid.NewGuid() };
        private DefaultUserClaim _userClaimImpersinate = new DefaultUserClaim() { ImpersonatedBy = Guid.NewGuid() };
        private List<OrganizationType> _organizationType;
        private List<Organization> _organizationList;
        private List<OrganizationDomain> _organizationDomain;
        private List<Persona> _personaList = new List<Persona>();
        private IList<ProductInternalSettingByType> _settings;
        private static DateTime _CreateDate = DateTime.MaxValue.ToUniversalTime();
        private static int _organizationTypeId = 6;
        private Persona EmployeePersona;
        private Guid _employeeUserRealPageId = new Guid("9E9410AE-1111-2222-3333-109C08CD151C");
        private static Guid _OrganizationRealPageId = new Guid("C802694D-5553-4527-8616-3C0F434AE62D");

        public ShellTests()
        {
            _settings = new List<ProductInternalSettingByType>()
            {
                new ProductInternalSettingByType
                {
                    ProductConfigurationId = "1413141",
                    Name = "ImpersonationRightsToBeExcluded",
                    Value = "InternalAdminaccessToUnifiedSettings,EmployeeAccesstoManageSettingsTemplates,AccessSettingsAdmin,EmployeeAccessUnifiedReportingAdminConsole",
                    ProductId = 3,
                    ProductName = "Unified Platform",
                    BooksProductCode = "UPFM"
                }
            };

            EmployeePersona = new Persona()
            {
                FromDate = DateTime.UtcNow,
                Name = "Super User",
                Organization = new Organization() { RealPageId = DefaultUserClaim.EmployeeCompanyRealPageId, PartyId = 12345 },
                PersonaId = 55346,
                RealPageId = _employeeUserRealPageId,
                ThruDate = DateTime.UtcNow.AddDays(1),
                OrganizationPartyId = 12345
            };

            _personaList.Add(EmployeePersona);

            _organizationDomain = new List<OrganizationDomain>()
            {
                new OrganizationDomain()
                {
                    OrganizationDomainId = 1,
                    Name = "Primary",
                    CreateDate = new DateTime()
                }
            };
            _organizationList = new List<Organization>()
            {
                new Organization()
                {
                    RealPageId = DefaultUserClaim.EmployeeCompanyRealPageId,
                    CreateDate = _CreateDate,
                    Name = "Onesite Test Company",
                    PartyId = 12345,
                    BooksMasterId = 123456,
                    BooksCustomerMasterId = 1234567,
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
                },
                new Organization()
                {
                    RealPageId = new Guid("22222222-2222-2222-2222-222222222222"),
                    CreateDate = _CreateDate,
                    Name = "Onesite Invalid Test Company",
                    PartyId = 54321,
                    BooksMasterId = 654321,
                    BooksCustomerMasterId = 7654321,
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
            _organizationType = new List<OrganizationType>()
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
        }

        [Fact]
        public void GetSideMenuStructureAndOrdering()
        {
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestOrganizationGuid(d, _OrganizationRealPageId))))
                .Returns(new List<ProductUI>() { new PersonaProductUserDetails() { ProductId = 1 }, new PersonaProductUserDetails() { ProductId = 56 }, new PersonaProductUserDetails() { ProductId = 8 } });

            mockRepository
                .Setup(m => m.GetMany<PersonaActionRight>(StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute, It.IsAny<object>()))
                .Returns(new List<PersonaActionRight>()
                {
                    new PersonaActionRight()
                    {
                        ObjectType = "Route",
                        Action = "Side Menu",
                        ProductId = 1
                    },
                    new PersonaActionRight()
                    {
                        ObjectType = "Right",
                        Action = "Dashboard",
                        ProductId = 8
                    }
                });

            mockRepository
                .Setup(m => m.GetMany<NavigationMenuEntry>(StoredProcNameConstants.SP_GetNavigationMenu, It.IsAny<object>()))
                .Returns(new List<NavigationMenuEntry>()
                {
                    new NavigationMenuEntry()
                    {
                        Id = 1,
                        Icon = "test icon 1",
                        OrderIndex = 1,
                        PageId = "page-id 1",
                        ParentId = null,
                        Title = "title 1",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 2,
                        Icon = "test icon 2",
                        OrderIndex = 0,
                        PageId = "page-id 2",
                        ParentId = null,
                        Title = "title 2",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 3,
                        Icon = "test icon 3",
                        OrderIndex = 0,
                        PageId = "page-id 3",
                        ParentId = 1,
                        Title = "title 3",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 4,
                        Icon = "test icon 4",
                        OrderIndex = 0,
                        PageId = "page-id 4",
                        ParentId = null,
                        Title = "title 4",
                        URL = "http://url/"
                    }
                });

            mockRepository
                .Setup(m => m.GetMany<NavigationMenuRightEntry>(StoredProcNameConstants.SP_GetNavigationMenuRights, It.IsAny<object>()))
                .Returns(new List<NavigationMenuRightEntry>()
                {
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 1,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 2,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 3,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 4,
                        RightName = "not valid"
                    }
                });

            ShellController shellController = new ShellController(mockRepository.Object, mockMessageHandler.Object, _userClaim)
            {
                _personaId = 1234
            };
            _userClaim.OrganizationRealPageGuid = _OrganizationRealPageId;
            var response = shellController.GetSideMenuNavigation();

            Assert.Equal(2, response.Count);
            Assert.Equal("title 2", response[0].Title);
            Assert.Null(response[0].Items);
            Assert.Equal("title 1", response[1].Title);
            Assert.Single(response[1].Items);
            Assert.Equal("title 3", response[1].Items[0].Title);
        }

        [Fact]
        public void GetSideMenuStructureForRealPageEmployee()
        {
            // Arrange
            Mock<HttpMessageHandler> mockMessageHandler = new Mock<HttpMessageHandler>();
            Mock<PersonaRepository> _personaRepository = new Mock<PersonaRepository>();

            mockRepository
                .Setup(m => m.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization,
                    It.Is<object>(
                        d => TestOrganizationGuid(d, _OrganizationRealPageId))))
                .Returns(new List<ProductUI>() { new PersonaProductUserDetails() { ProductId = 1 }, new PersonaProductUserDetails() { ProductId = 56 }, new PersonaProductUserDetails() { ProductId = 8 } });

            mockRepository
                .Setup(m => m.GetMany<PersonaActionRight>(StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute, It.Is<object>(d => TestPersonaIdAndRoute(d, 1234, "sidemenu"))))
                .Returns(new List<PersonaActionRight>()
                {
                    new PersonaActionRight()
                    {
                        ObjectType = "Route",
                        Action = "Side Menu",
                        ProductId = 1
                    },
                    new PersonaActionRight()
                    {
                        ObjectType = "Right",
                        Action = "Dashboard",
                        ProductId = 8
                    }
                });

            mockRepository
               .Setup(m => m.GetMany<PersonaActionRight>(StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute, It.Is<object>(d => TestPersonaIdAndRoute(d, 55346, "sidemenu"))))
               .Returns(new List<PersonaActionRight>()
               {
                   new PersonaActionRight()
                    {
                        ObjectType = "Route",
                        Action = "Side Menu"
                    },
                    new PersonaActionRight()
                    {
                        ObjectType = "Right",
                        Action = "InternalAdminaccessToUnifiedSettings"
                    },
                    new PersonaActionRight()
                    {
                        ObjectType = "Right",
                        Action = "EmployeeAccesstoManageSettingsTemplates"
                    },
                    new PersonaActionRight()
                    {
                        ObjectType = "Right",
                        Action = "Dashboard"
                    },
                    new PersonaActionRight()
                    {
                        ObjectType = "Right",
                        Action = "EditOwnProfile"
                    }
               });

            mockRepository
                .Setup(m => m.GetMany<NavigationMenuEntry>(StoredProcNameConstants.SP_GetNavigationMenu, It.IsAny<object>()))
                .Returns(new List<NavigationMenuEntry>()
                {
                    new NavigationMenuEntry()
                    {
                        Id = 1,
                        Icon = "test icon 1",
                        OrderIndex = 1,
                        PageId = "page-id 1",
                        ParentId = null,
                        Title = "title 1",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 2,
                        Icon = "test icon 2",
                        OrderIndex = 0,
                        PageId = "page-id 2",
                        ParentId = null,
                        Title = "title 2",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 3,
                        Icon = "test icon 3",
                        OrderIndex = 0,
                        PageId = "page-id 3",
                        ParentId = 1,
                        Title = "title 3",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 4,
                        Icon = "test icon 4",
                        OrderIndex = 0,
                        PageId = "page-id 4",
                        ParentId = null,
                        Title = "title 4",
                        URL = "http://url/"
                    },
                    new NavigationMenuEntry()
                    {
                        Id = 5,
                        Icon = "test icon 5",
                        OrderIndex = 0,
                        PageId = "page-id 5",
                        ParentId = null,
                        Title = "title 5",
                        URL = "http://url/"
                    }
                });

            mockRepository
                .Setup(m => m.GetMany<NavigationMenuRightEntry>(StoredProcNameConstants.SP_GetNavigationMenuRights, It.IsAny<object>()))
                .Returns(new List<NavigationMenuRightEntry>()
                {
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 1,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 2,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 3,
                        RightName = "Dashboard"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 2,
                        RightName = "Settings"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 4,
                        RightName = "not valid"
                    },
                    new NavigationMenuRightEntry()
                    {
                        NavigationMenuId = 5,
                        RightName = "EditOwnProfile"
                    }
                });

            mockRepository
               .Setup(m => m.GetMany<OrganizationType>(StoredProcNameConstants.SP_ListOrganizationType, null))
               .Returns(_organizationType);

            mockRepository
               .Setup(m => m.GetMany<Organization>(StoredProcNameConstants.SP_ListOrganizationByRealPageId, It.IsAny<object>()))
               .Returns(_organizationList);

            mockRepository
               .Setup(m => m.GetMany<OrganizationDomain>(StoredProcNameConstants.SP_ListOrganizationDomain, null))
               .Returns(_organizationDomain);

            mockRepository.Setup(m => m.GetMany<Persona>(StoredProcNameConstants.SP_ListPersona, It.IsAny<object>()))
                .Returns(_personaList);

            mockRepository
                .Setup(m => m.GetMany<ProductInternalSettingByType>(StoredProcNameConstants.SP_ListProductGlobalSettingsBySettingType, It.Is<object>(l => TestProductSettingType(l, "ImpersonationRightsToBeExcluded"))))
                .Returns(_settings.Where(p => p.Name == "ImpersonationRightsToBeExcluded").ToList());

            _userClaim.ImpersonatedBy = _employeeUserRealPageId;
            _userClaim.OrganizationRealPageGuid = _OrganizationRealPageId;
            ShellController shellController = new ShellController(mockRepository.Object, mockMessageHandler.Object,_userClaim )
            {
                _personaId = 1234

            };

            // Act
            var response = shellController.GetSideMenuNavigation();

            // Assest
            Assert.Equal(2, response.Count);
            Assert.Contains(response, x => x.Title == "title 2");
        }

        private bool TestProductSettingType(object obj, string value)
        {
            return obj.ToString().Equals($"{{ ProductSettingType = {value} }}");
        }
        private bool TestOrganizationGuid(object obj, Guid OrganizationRealPageId)
        {
            return obj.ToString().Equals($"{{ OrganizationRealPageId = {OrganizationRealPageId} }}");
        }
        private bool TestPersonaIdAndRoute(object obj, long personaId, string routeId)
        {
            return obj.ToString().ToLower().Contains($"{{ personaid = {personaId}, routeid = {routeId} }}");

        }

    }
}
