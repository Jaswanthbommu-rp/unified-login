using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageDashboardContent business logic xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManageDashboardContent
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageDashboardContentTests : TestBase
    {
        private readonly Mock<IManageProfile> _mockManageProfile;
        private readonly Mock<IManageProduct> _mockManageProduct;
        private readonly Mock<IManageSecurity> _mockManageSecurity;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageDashboardContentTests()
        {
            _mockManageProfile = new Mock<IManageProfile>();
            _mockManageProduct = new Mock<IManageProduct>();
            _mockManageSecurity = new Mock<IManageSecurity>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                OrganizationRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationMasterId = 379,
                OrganizationPartyId = 100,
                CorrelationId = Guid.NewGuid(),
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = 5
            };
        }

        private ManageDashboardContent CreateManageDashboardContent()
        {
            return new ManageDashboardContent(
                _defaultUserClaim,
                _mockManageProfile.Object,
                _mockManageProduct.Object,
                _mockManageSecurity.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithAllDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageDashboardContent = CreateManageDashboardContent();

            // Assert
            Assert.NotNull(manageDashboardContent);
        }

        [Fact]
        public void Constructor_WithDefaultUserClaimOnly_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageDashboardContent = new ManageDashboardContent(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageDashboardContent);
        }

        #endregion

        #region GetDashboardElementResponse Tests

        [Fact]
        public void GetDashboardElementResponse_WithNullRealPageId_ThrowsArgumentNullException()
        {
            // Arrange
            Guid realPageId = Guid.Empty;
            var persona = CreateTestPersona();
            var manageDashboardContent = CreateManageDashboardContent();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageDashboardContent.GetDashboardElementResponse(realPageId, persona));

            Assert.Equal("realPageId", exception.ParamName);
            Assert.Contains("Null realPageId", exception.Message);
        }

        [Fact]
        public void GetDashboardElementResponse_WithEmptyGuid_ThrowsArgumentNullException()
        {
            // Arrange
            Guid realPageId = Guid.Empty;
            var persona = CreateTestPersona();
            var manageDashboardContent = CreateManageDashboardContent();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageDashboardContent.GetDashboardElementResponse(realPageId, persona));

            Assert.Equal("realPageId", exception.ParamName);
        }

        [Fact]
        public void GetDashboardElementResponse_WithNullPersona_ThrowsArgumentNullException()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            Persona persona = null;
            var manageDashboardContent = CreateManageDashboardContent();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageDashboardContent.GetDashboardElementResponse(realPageId, persona));

            Assert.Equal("realPageId", exception.ParamName);
            Assert.Contains("Null persona", exception.Message);
        }

        [Fact]
        public void GetDashboardElementResponse_WithValidParameters_ReturnsDashboardElements()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.DashboardElements);
            Assert.NotNull(result.DashboardElements.ProfileDetail);
            Assert.NotNull(result.DashboardElements.ProfileDetail.AssignedProducts);
            Assert.NotNull(result.DashboardElements.Resources);
        }

        [Fact]
        public void GetDashboardElementResponse_VerifiesProfileDetailRetrieval()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            _mockManageProfile.Verify(
                x => x.GetProfileDetail(realPageId, persona.OrganizationPartyId, null, null, null, null),
                Times.Once);
            Assert.Equal(profileDetail, result.DashboardElements.ProfileDetail);
        }

        [Fact]
        public void GetDashboardElementResponse_VerifiesSecurityRetrieval()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            _mockManageSecurity.Verify(
                x => x.GetPersonaRightsAndActionsByRoute(persona.PersonaId, "dashboard"),
                Times.Once);
        }

        [Fact]
        public void GetDashboardElementResponse_VerifiesAssignedProductsRetrieval()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            _mockManageProduct.Verify(
                x => x.GetUserAssignedProductsByPersona(persona, null, routeSecurity.obj),
                Times.Once);
        }

        [Fact]
        public void GetDashboardElementResponse_AssignedProductsOrderedCorrectly()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            
            // Create products with different favorite status and names
            var assignedProducts = new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails { ProductId = 1, ProductName = "Zebra Product", IsFavorite = false },
                new PersonaProductUserDetails { ProductId = 2, ProductName = "Alpha Product", IsFavorite = true },
                new PersonaProductUserDetails { ProductId = 3, ProductName = "Beta Product", IsFavorite = false }
            };
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            var orderedProducts = result.DashboardElements.ProfileDetail.AssignedProducts;
            Assert.Equal(3, orderedProducts.Count);
            // First should be favorite (Alpha Product)
            Assert.True(orderedProducts[0].IsFavorite);
            Assert.Equal("Alpha Product", orderedProducts[0].ProductName);
            // Next two should be non-favorites ordered alphabetically
            Assert.False(orderedProducts[1].IsFavorite);
            Assert.Equal("Beta Product", orderedProducts[1].ProductName);
            Assert.False(orderedProducts[2].IsFavorite);
            Assert.Equal("Zebra Product", orderedProducts[2].ProductName);
        }

        [Fact]
        public void GetDashboardElementResponse_VerifiesResourcesRetrieval()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            _mockManageProduct.Verify(
                x => x.GetUserAssignedProductsByPersona(persona, ProductSelectType.ResourcesOnly, routeSecurity.obj),
                Times.Once);
            Assert.Equal(resources, result.DashboardElements.Resources);
        }

        [Fact]
        public void GetDashboardElementResponse_SetsTotalAssignedProductsCount()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            Assert.Equal(assignedProducts.Count, result.DashboardElements.ProfileDetail.SummaryCount.TotalAssignedProducts);
        }

        [Fact]
        public void GetDashboardElementResponse_SetsTotalAssignedPropertiesToZero()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            // TODO: Waiting for Master Data Management (black book) integration
            Assert.Equal(0, result.DashboardElements.ProfileDetail.SummaryCount.TotalAssignedProperties);
        }

        [Fact]
        public void GetDashboardElementResponse_SetsTotalAssignedRolesToZero()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            // TODO: Waiting for implementation of products and roles
            Assert.Equal(0, result.DashboardElements.ProfileDetail.SummaryCount.TotalAssignedRoles);
        }

        [Fact]
        public void GetDashboardElementResponse_WithEmptyAssignedProducts_ReturnsZeroCount()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var emptyProducts = new List<PersonaProductUserDetails>();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, emptyProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            Assert.Equal(0, result.DashboardElements.ProfileDetail.SummaryCount.TotalAssignedProducts);
            Assert.NotNull(result.DashboardElements.ProfileDetail.AssignedProducts);
            Assert.Empty(result.DashboardElements.ProfileDetail.AssignedProducts);
        }

        [Fact]
        public void GetDashboardElementResponse_WithEmptyResources_ReturnsEmptyList()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var emptyResources = new List<PersonaProductUserDetails>();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, emptyResources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            Assert.NotNull(result.DashboardElements.Resources);
            Assert.Empty(result.DashboardElements.Resources);
        }

        [Fact]
        public void GetDashboardElementResponse_WithMultipleFavorites_OrdersFavoritesAlphabetically()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            
            var assignedProducts = new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails { ProductId = 1, ProductName = "Zebra Favorite", IsFavorite = true },
                new PersonaProductUserDetails { ProductId = 2, ProductName = "Alpha Favorite", IsFavorite = true },
                new PersonaProductUserDetails { ProductId = 3, ProductName = "Delta Product", IsFavorite = false }
            };
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            var orderedProducts = result.DashboardElements.ProfileDetail.AssignedProducts;
            Assert.Equal("Alpha Favorite", orderedProducts[0].ProductName);
            Assert.Equal("Zebra Favorite", orderedProducts[1].ProductName);
            Assert.Equal("Delta Product", orderedProducts[2].ProductName);
        }

        [Fact]
        public void GetDashboardElementResponse_DashboardElementsNotNull()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var persona = CreateTestPersona();
            
            var profileDetail = CreateTestProfileDetail(realPageId, persona.OrganizationPartyId);
            var routeSecurity = CreateTestRouteSecurity();
            var assignedProducts = CreateTestAssignedProducts();
            var resources = CreateTestResources();

            SetupMockDependencies(persona, profileDetail, routeSecurity, assignedProducts, resources);

            var manageDashboardContent = CreateManageDashboardContent();

            // Act
            var result = manageDashboardContent.GetDashboardElementResponse(realPageId, persona);

            // Assert
            Assert.NotNull(result.DashboardElements);
            Assert.IsType<DashboardElements>(result.DashboardElements);
        }

        #endregion

        #region Helper Methods

        private Persona CreateTestPersona()
        {
            return new Persona
            {
                PersonaId = 5,
                UserId = 1,
                OrganizationPartyId = 100,
                Organization = new Organization
                {
                    PartyId = 100,
                    RealPageId = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                    Name = "Test Organization",
                    BooksMasterId = 379
                }
            };
        }

        private ProfileDetail CreateTestProfileDetail(Guid realPageId, long orgPartyId)
        {
            return new ProfileDetail
            {
                RealPageId = realPageId,
                FirstName = "Test",
                LastName = "User",
                userLogin = new UserLogin
                {
                    LoginName = "testuser@test.com"
                },
                AssignedProducts = new List<PersonaProductUserDetails>(),
                SummaryCount = new SummaryCounts
                {
                    TotalAssignedProducts = 0,
                    TotalAssignedProperties = 0,
                    TotalAssignedRoles = 0
                }
            };
        }

        private ObjectOutput<RouteSecurity, IErrorData> CreateTestRouteSecurity()
        {
            return new ObjectOutput<RouteSecurity, IErrorData>
            {
                obj = new RouteSecurity
                {
                    RouteId = "dashboard",
                    Rights = new List<string> { "ViewDashboard", "ViewProducts" }
                }
            };
        }

        private List<PersonaProductUserDetails> CreateTestAssignedProducts()
        {
            return new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails
                {
                    ProductId = 1,
                    ProductName = "OneSite",
                    IsFavorite = true,
                    PersonaId = 5,
                    OrganizationPartyId = 100
                },
                new PersonaProductUserDetails
                {
                    ProductId = 2,
                    ProductName = "Accounting",
                    IsFavorite = false,
                    PersonaId = 5,
                    OrganizationPartyId = 100
                },
                new PersonaProductUserDetails
                {
                    ProductId = 3,
                    ProductName = "Asset Optimization",
                    IsFavorite = false,
                    PersonaId = 5,
                    OrganizationPartyId = 100
                }
            };
        }

        private List<PersonaProductUserDetails> CreateTestResources()
        {
            return new List<PersonaProductUserDetails>
            {
                new PersonaProductUserDetails
                {
                    ProductId = 100,
                    ProductName = "Training Portal",
                    PersonaId = 5,
                    OrganizationPartyId = 100
                },
                new PersonaProductUserDetails
                {
                    ProductId = 101,
                    ProductName = "Documentation",
                    PersonaId = 5,
                    OrganizationPartyId = 100
                }
            };
        }

        private void SetupMockDependencies(
            Persona persona,
            ProfileDetail profileDetail,
            ObjectOutput<RouteSecurity, IErrorData> routeSecurity,
            List<PersonaProductUserDetails> assignedProducts,
            List<PersonaProductUserDetails> resources)
        {
            _mockManageSecurity
                .Setup(x => x.GetPersonaRightsAndActionsByRoute(persona.PersonaId, "dashboard"))
                .Returns(routeSecurity);

            _mockManageProfile
                .Setup(x => x.GetProfileDetail(It.IsAny<Guid>(), persona.OrganizationPartyId, null, null, null, null))
                .Returns(profileDetail);

            _mockManageProduct
                .Setup(x => x.GetUserAssignedProductsByPersona(persona, null, routeSecurity.obj))
                .Returns(assignedProducts);

            _mockManageProduct
                .Setup(x => x.GetUserAssignedProductsByPersona(persona, ProductSelectType.ResourcesOnly, routeSecurity.obj))
                .Returns(resources);
        }

        #endregion
    }
}
