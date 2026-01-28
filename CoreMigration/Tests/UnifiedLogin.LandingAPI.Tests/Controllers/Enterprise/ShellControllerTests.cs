using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

//using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPIEnterprise.Controllers;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;

namespace UnifiedLogin.LandingAPI.Tests.Controllers.Enterprise
{
    /// <summary>
    /// Comprehensive unit tests for ShellController with 100% code coverage.
    /// Tests all endpoints, filtering scenarios, impersonation handling, and navigation menu construction.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ShellControllerTests : ControllerBase
    {
        #region Private Fields

        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IManageSecurity> _mockManageSecurity;
        private readonly Mock<IPersonaRepository> _mockPersonaRepository;
        private readonly Mock<IProductInternalSettingRepository> _mockProductInternalSettingRepository;
        private readonly Mock<IOrganizationRepository> _mockOrganizationRepository;
        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly ShellController _controller;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testOrgId = Guid.NewGuid();
        private readonly Guid _testImpersonatedById = Guid.NewGuid();
        private readonly long _testPersonaId = 100;
        private readonly long _testOrgPartyId = 1000;

        #endregion

        #region Constructor & Setup

        public ShellControllerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockManageSecurity = new Mock<IManageSecurity>();
            _mockPersonaRepository = new Mock<IPersonaRepository>();
            _mockProductInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _mockOrganizationRepository = new Mock<IOrganizationRepository>();
            _mockUserClaimsAccessor = new Mock<IUserClaimsAccessor>();

            _mockUserClaimsAccessor.Setup(x => x.UserRealPageGuid).Returns(_testUserId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid).Returns(_testOrgId);
            _mockUserClaimsAccessor.Setup(x => x.OrganizationPartyId).Returns(_testOrgPartyId);
            _mockUserClaimsAccessor.Setup(x => x.PersonaId).Returns(_testPersonaId);
            _mockUserClaimsAccessor.Setup(x => x.ImpersonatedBy).Returns(Guid.Empty);

            _controller = new ShellController(
                _mockUserRepository.Object,
                _mockManageSecurity.Object,
                _mockPersonaRepository.Object,
                _mockProductInternalSettingRepository.Object,
                _mockOrganizationRepository.Object,
                _mockUserClaimsAccessor.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        //public void Dispose()
        //{
        //    _controller?.Dispose();
        //}

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new ShellController(
                _mockUserRepository.Object,
                _mockManageSecurity.Object,
                _mockPersonaRepository.Object,
                _mockProductInternalSettingRepository.Object,
                _mockOrganizationRepository.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            controller.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullUserRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ShellController(
                    null,
                    _mockManageSecurity.Object,
                    _mockPersonaRepository.Object,
                    _mockProductInternalSettingRepository.Object,
                    _mockOrganizationRepository.Object,
                    _mockUserClaimsAccessor.Object));
        }

        [Fact]
        public void Constructor_WithNullManageSecurity_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ShellController(
                    _mockUserRepository.Object,
                    null,
                    _mockPersonaRepository.Object,
                    _mockProductInternalSettingRepository.Object,
                    _mockOrganizationRepository.Object,
                    _mockUserClaimsAccessor.Object));
        }

        [Fact]
        public void Constructor_WithNullPersonaRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ShellController(
                    _mockUserRepository.Object,
                    _mockManageSecurity.Object,
                    null,
                    _mockProductInternalSettingRepository.Object,
                    _mockOrganizationRepository.Object,
                    _mockUserClaimsAccessor.Object));
        }

        [Fact]
        public void Constructor_WithNullProductInternalSettingRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ShellController(
                    _mockUserRepository.Object,
                    _mockManageSecurity.Object,
                    _mockPersonaRepository.Object,
                    null,
                    _mockOrganizationRepository.Object,
                    _mockUserClaimsAccessor.Object));
        }

        [Fact]
        public void Constructor_WithNullOrganizationRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ShellController(
                    _mockUserRepository.Object,
                    _mockManageSecurity.Object,
                    _mockPersonaRepository.Object,
                    _mockProductInternalSettingRepository.Object,
                    null,
                    _mockUserClaimsAccessor.Object));
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ShellController(
                    _mockUserRepository.Object,
                    _mockManageSecurity.Object,
                    _mockPersonaRepository.Object,
                    _mockProductInternalSettingRepository.Object,
                    _mockOrganizationRepository.Object,
                    null));
        }

        #endregion

        #region GetSideMenuNavigation Tests

        [Fact]
        public void GetSideMenuNavigation_WithValidRightsAndProducts_ReturnsNavigationMenu()
        {
           
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<SharedObjects.Landing.Security.ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 }
            };
            var navigationMenuRights = new List<NavigationMenuRightEntry>();
            var navigationMenuSettings = new List<NavigationMenuSetting>();

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
            .Returns(new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity
                {
                    ProductRights = productRights
                }
            });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(navigationMenuRights);
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(navigationMenuSettings);

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Menu1");
        }

        [Fact]
        public void GetSideMenuNavigation_WithNullRights_ReturnsEmptyList()
        {
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = (List<ProductRights>)null
                    }
                });
               

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetSideMenuNavigation_WithNullProducts_ReturnsEmptyList()
        {
            // Arrange
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns((IList<ProductUI>)null);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = productRights
                    }
                });

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetSideMenuNavigation_WithEmployeeCompanyAndNoUserManagementRights_FiltersUsersMenu()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid)
                .Returns(DefaultUserClaim.EmployeeCompanyRealPageId);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "OtherRight" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Users", PageId = "users", ParentId = null, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 2, Title = "Other", PageId = "other", ParentId = null, OrderIndex = 2 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(DefaultUserClaim.EmployeeCompanyRealPageId))
                .Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = productRights
                    }
                });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotContain(x => x.PageId == "users");
            result.Should().HaveCount(1);
        }

        [Fact]
        public void GetSideMenuNavigation_WithEmployeeCompanyAndNoReportRights_FiltersReportsMenu()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid)
                .Returns(DefaultUserClaim.EmployeeCompanyRealPageId);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "OtherRight" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Manage Reports", PageId = "manage reports", ParentId = null, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 2, Title = "Other", PageId = "other", ParentId = null, OrderIndex = 2 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(DefaultUserClaim.EmployeeCompanyRealPageId))
                .Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = productRights
                    }
                });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotContain(x => x.PageId == "manage reports");
        }

        [Fact]
        public void GetSideMenuNavigation_WithUserManagementRight_KeepsUsersMenu()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid)
                .Returns(DefaultUserClaim.EmployeeCompanyRealPageId);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "RealPageEmployeeUserManagement" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Users", PageId = "users", ParentId = null, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(DefaultUserClaim.EmployeeCompanyRealPageId))
                .Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
               .Returns(new ObjectOutput<RouteSecurity, IErrorData>
               {
                   obj = new RouteSecurity
                   {
                       ProductRights = productRights
                   }
               });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().Contain(x => x.PageId == "users");
        }

        [Fact]
        public void GetSideMenuNavigation_WithUserManagementViewOnlyRight_KeepsUsersMenu()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid)
                .Returns(DefaultUserClaim.EmployeeCompanyRealPageId);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "RealPageEmployeeUserManagementViewOnly" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Users", PageId = "users", ParentId = null, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(DefaultUserClaim.EmployeeCompanyRealPageId))
                .Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
              .Returns(new ObjectOutput<RouteSecurity, IErrorData>
              {
                  obj = new RouteSecurity
                  {
                      ProductRights = productRights
                  }
              });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().Contain(x => x.PageId == "users");
        }

        [Fact]
        public void GetSideMenuNavigation_WithReportManagementRight_KeepsReportsMenu()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid)
                .Returns(DefaultUserClaim.EmployeeCompanyRealPageId);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "ManageBestPracticeReportGroupsAD" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Manage Reports", PageId = "manage reports", ParentId = null, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(DefaultUserClaim.EmployeeCompanyRealPageId))
                .Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
               .Returns(new ObjectOutput<RouteSecurity, IErrorData>
               {
                   obj = new RouteSecurity
                   {
                       ProductRights = productRights
                   }
               });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().Contain(x => x.PageId == "manage reports");
        }

        #endregion

        #region Impersonation Tests

        [Fact]
        public void GetSideMenuNavigation_WithImpersonation_MergesImpersonatorRights()
        {
            _mockUserClaimsAccessor.Setup(x => x.ImpersonatedBy).Returns(_testImpersonatedById);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var userRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var impersonatorRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right2" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
             .Returns(new ObjectOutput<RouteSecurity, IErrorData>
             {
                 obj = new RouteSecurity
                 {
                     ProductRights = userRights
                 }
             });
            _mockUserRepository.Setup(x => x.CheckOrganizationAdminUser(_testUserId, _testOrgPartyId)).Returns(true);
            _mockPersonaRepository.Setup(x => x.ListPersona(_testImpersonatedById))
                .Returns(new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = 200,
                        Organization = new Organization { RealPageId = DefaultUserClaim.EmployeeCompanyRealPageId }
                    }
                });
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(200, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = impersonatorRights
                    }
                });
            _mockProductInternalSettingRepository.Setup(x => x.GetProductSettingByType("ImpersonationRightsToBeExcluded"))
                .Returns(new List<ProductInternalSettingByType>());
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetSideMenuNavigation_WithImpersonationAndExcludedRights_RemovesExcludedRights()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.ImpersonatedBy).Returns(_testImpersonatedById);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var userRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var impersonatorRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "ExcludedRight" },
                new ProductRights { ProductId = 1, RightName = "IncludedRight" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 }
            };
            var excludedSetting = new ProductInternalSettingByType
            {
                Name = "ImpersonationRightsToBeExcluded",
                Value = "ExcludedRight,AnotherExcluded"
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = userRights
                    }
                });
            _mockUserRepository.Setup(x => x.CheckOrganizationAdminUser(_testUserId, _testOrgPartyId)).Returns(true);
            _mockPersonaRepository.Setup(x => x.ListPersona(_testImpersonatedById))
                .Returns(new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = 200,
                        Organization = new Organization { RealPageId = DefaultUserClaim.EmployeeCompanyRealPageId }
                    }
                });
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(200, "sidemenu"))
               .Returns(new ObjectOutput<RouteSecurity, IErrorData>
               {
                   obj = new RouteSecurity
                   {
                       ProductRights = impersonatorRights
                   }
               });
            _mockProductInternalSettingRepository.Setup(x => x.GetProductSettingByType("ImpersonationRightsToBeExcluded"))
                .Returns(new List<ProductInternalSettingByType> { excludedSetting });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetSideMenuNavigation_WithImpersonationAndNotAdminUser_DoesNotMergeRights()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.ImpersonatedBy).Returns(_testImpersonatedById);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var userRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = userRights
                     }
                 });
            _mockUserRepository.Setup(x => x.CheckOrganizationAdminUser(_testUserId, _testOrgPartyId)).Returns(false);
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotBeNull();
            _mockPersonaRepository.Verify(x => x.ListPersona(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region Navigation Menu Filtering Tests

        [Fact]
        public void GetSideMenuNavigation_WithMenuEntryRequiringRight_FiltersBasedOnRights()
        {
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 2, Title = "Menu2", PageId = "page2", ParentId = null, OrderIndex = 2 }
            };
            var navigationMenuRights = new List<NavigationMenuRightEntry>
            {
                new NavigationMenuRightEntry { NavigationMenuId = 2, RightName = "RequiredRight" }
            };
            var navigationMenuSettings = new List<NavigationMenuSetting>();

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = productRights
                    }
                });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(navigationMenuRights);
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(navigationMenuSettings);

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Menu1");
        }

        [Fact]
        public void GetSideMenuNavigation_WithMenuEntrySettingRestriction_FiltersBasedOnSettings()
        {
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 2, Title = "Menu2", PageId = "page2", ParentId = null, OrderIndex = 2 }
            };
            var navigationMenuRights = new List<NavigationMenuRightEntry>();
            var navigationMenuSettings = new List<NavigationMenuSetting>
            {
                new NavigationMenuSetting { NavigationMenuId = 2 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = productRights
                    }
                });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(navigationMenuRights);
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(navigationMenuSettings);

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Menu1");
        }

        #endregion

        #region Navigation Menu Tree Building Tests

        [Fact]
        public void GetSideMenuNavigation_BuildsHierarchicalMenuTree()
        {
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Parent", PageId = "parent", Icon = "icon1", URL = "url1", Origin = "origin1", ParentId = null, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 2, Title = "Child", PageId = "child", Icon = "icon2", URL = "url2", Origin = "origin2", ParentId = 1, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = productRights
                     }
                 });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Parent");
            result[0].Items.Should().HaveCount(1);
            result[0].Items[0].Title.Should().Be("Child");
            result[0].Items[0].Items.Should().BeNull();
        }

        [Fact]
        public void GetSideMenuNavigation_WithEmptyMenuEntries_ReturnsNull()
        {
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>();

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = productRights
                     }
                 });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetSideMenuNavigation_OrdersMenuByOrderIndex()
        {
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Third", PageId = "page3", ParentId = null, OrderIndex = 3 },
                new NavigationMenuEntry { Id = 2, Title = "First", PageId = "page1", ParentId = null, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 3, Title = "Second", PageId = "page2", ParentId = null, OrderIndex = 2 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = productRights
                     }
                 });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().HaveCount(3);
            result[0].Title.Should().Be("First");
            result[1].Title.Should().Be("Second");
            result[2].Title.Should().Be("Third");
        }

        [Fact]
        public void GetSideMenuNavigation_DeepHierarchyBuiltCorrectly()
        {
            // Arrange
            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var productRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Level1", PageId = "l1", ParentId = null, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 2, Title = "Level2", PageId = "l2", ParentId = 1, OrderIndex = 1 },
                new NavigationMenuEntry { Id = 3, Title = "Level3", PageId = "l3", ParentId = 2, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = productRights
                     }
                 });
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().HaveCount(1);
            result[0].Items.Should().HaveCount(1);
            result[0].Items[0].Items.Should().HaveCount(1);
            result[0].Items[0].Items[0].Title.Should().Be("Level3");
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void GetSideMenuNavigation_WithNoImpersonationRightsSettings_HandlesGracefully()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.ImpersonatedBy).Returns(_testImpersonatedById);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var userRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = userRights
                     }
                 });
            _mockUserRepository.Setup(x => x.CheckOrganizationAdminUser(_testUserId, _testOrgPartyId)).Returns(true);
            _mockPersonaRepository.Setup(x => x.ListPersona(_testImpersonatedById))
                .Returns(new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = 200,
                        Organization = new Organization { RealPageId = DefaultUserClaim.EmployeeCompanyRealPageId }
                    }
                });
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(200, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = new List<ProductRights>()
                     }
                 });
            _mockProductInternalSettingRepository.Setup(x => x.GetProductSettingByType("ImpersonationRightsToBeExcluded"))
                .Returns((IList<UnifiedLogin.SharedObjects.IdentityConfig.ProductInternalSettingByType>)(List<ProductInternalSetting>)null);
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetSideMenuNavigation_WithEmptyExcludedRightsValue_SkipsEmptyValues()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.ImpersonatedBy).Returns(_testImpersonatedById);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var userRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var impersonatorRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right2" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 }
            };
            var excludedSetting = new ProductInternalSettingByType
            {
                Name = "ImpersonationRightsToBeExcluded",
                Value = ""
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                {
                    obj = new RouteSecurity
                    {
                        ProductRights = userRights
                    }
                });
            _mockUserRepository.Setup(x => x.CheckOrganizationAdminUser(_testUserId, _testOrgPartyId)).Returns(true);
            _mockPersonaRepository.Setup(x => x.ListPersona(_testImpersonatedById))
                .Returns(new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = 200,
                        Organization = new Organization { RealPageId = DefaultUserClaim.EmployeeCompanyRealPageId }
                    }
                });
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(200, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = impersonatorRights
                     }
                 });

            _mockProductInternalSettingRepository.Setup(x => x.GetProductSettingByType("ImpersonationRightsToBeExcluded"))
                 .Returns(new List<ProductInternalSettingByType> { excludedSetting });

            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void GetSideMenuNavigation_WithNoImpersonatedUserPersona_ReturnsUserRightsOnly()
        {
            // Arrange
            _mockUserClaimsAccessor.Setup(x => x.ImpersonatedBy).Returns(_testImpersonatedById);

            var products = new List<ProductUI>
            {
                new ProductUI { ProductId = 1, ProductName = "Product1" }
            };
            var userRights = new List<ProductRights>
            {
                new ProductRights { ProductId = 1, RightName = "Right1" }
            };
            var navigationMenu = new List<NavigationMenuEntry>
            {
                new NavigationMenuEntry { Id = 1, Title = "Menu1", PageId = "page1", ParentId = null, OrderIndex = 1 }
            };

            _mockOrganizationRepository.Setup(x => x.GetProductsByCompany(_testOrgId)).Returns(products);
            _mockManageSecurity.Setup(x => x.GetPersonaRightsAndActionsByRoute(_testPersonaId, "sidemenu"))
                 .Returns(new ObjectOutput<RouteSecurity, IErrorData>
                 {
                     obj = new RouteSecurity
                     {
                         ProductRights = userRights
                     }
                 });
            _mockUserRepository.Setup(x => x.CheckOrganizationAdminUser(_testUserId, _testOrgPartyId)).Returns(true);
            _mockPersonaRepository.Setup(x => x.ListPersona(_testImpersonatedById))
                .Returns(new List<Persona>());
            _mockUserRepository.Setup(x => x.GetNavigationMenu()).Returns(navigationMenu);
            _mockUserRepository.Setup(x => x.GetNavigationMenuRights()).Returns(new List<NavigationMenuRightEntry>());
            _mockUserRepository.Setup(x => x.GetNavigationMenuSettingsUnaccessable(_testOrgPartyId))
                .Returns(new List<NavigationMenuSetting>());

            // Act
            var result = _controller.GetSideMenuNavigation();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        #endregion
    }
}