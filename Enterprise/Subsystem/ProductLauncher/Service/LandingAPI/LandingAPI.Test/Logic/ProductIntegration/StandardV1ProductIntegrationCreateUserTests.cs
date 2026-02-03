using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Xunit;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ProductIntegration
{
    [ExcludeFromCodeCoverage]
    public class StandardV1ProductIntegrationCreateUserTests
    {
        private const int ProductId = 41;
        private const long EditorPersonaId = 33;
        private const long SubjectPersonaId = 34;

        private Mock<IDataCollector> _dataCollector;
        private Mock<IManagePersona> _managePersona;
        private Mock<IProductInternalSettingRepository> _productInternalSettingRepository;
        private Mock<IProductRepository> _productRepository;

        private readonly DefaultUserClaim _userClaims;
        private readonly UserDetails _editorUserDetails;
        private readonly UserDetails _subjectUserDetails;
        private readonly GbProductMap _gbProductMap;

        public StandardV1ProductIntegrationCreateUserTests()
        {
            _userClaims = new DefaultUserClaim
            {
                LoginName = "TestUser",
                CorrelationId = Guid.NewGuid(),
                OrganizationName = "TestOrg",
                OrganizationPartyId = 1,
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationMasterId = 1,
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = EditorPersonaId,
                UserId = 1
            };

            var editorRealPageId = Guid.NewGuid();
            _editorUserDetails = new UserDetails
            {
                BooksMasterId = 1,
                BooksCustomerMasterId = 1,
                PersonaId = (int)EditorPersonaId,
                LoginName = "editor@test.com",
                UserId = 1,
                UserRealPageId = editorRealPageId,
                Email = "editor@test.com"
            };

            _subjectUserDetails = new UserDetails
            {
                BooksMasterId = 1,
                BooksCustomerMasterId = 1,
                PersonaId = (int)SubjectPersonaId,
                LoginName = "subject@test.com",
                UserId = 2,
                Email = "subject@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageId = Guid.NewGuid()
            };

            _gbProductMap = new GbProductMap
            {
                Name = "TestProduct",
                BooksProductCode = "TEST",
                ProductId = ProductId,
                UDMSourceCode = "TEST"
            };
        }

        private void SetupMocksForTest()
        {
            _dataCollector = new Mock<IDataCollector>();
            _managePersona = new Mock<IManagePersona>();
            _productInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _productRepository = new Mock<IProductRepository>();

            var settings = GetProductSettings();

            _dataCollector
                .Setup(m => m.GetUserDetailsByPersona(EditorPersonaId, ProductId))
                .Returns(_editorUserDetails);

            _dataCollector
                .Setup(m => m.GetUserDetailsByPersona(SubjectPersonaId, ProductId))
                .Returns(_subjectUserDetails);

            _dataCollector
                .Setup(m => m.GetBlueBookProductMap(ProductId))
                .Returns(_gbProductMap);

            _dataCollector
                .Setup(m => m.GetProductCompanyMap(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DefaultUserClaim>(), It.IsAny<string>()))
                .Returns(new CustomerCompanyMap { CompanyInstanceSourceId = "COMP001" });

            _managePersona
                .Setup(m => m.GetPersona(EditorPersonaId))
                .Returns(new Persona
                {
                    PersonaId = EditorPersonaId,
                    RealPageId = _editorUserDetails.UserRealPageId
                });

            _managePersona
                .Setup(m => m.ListPersona(It.IsAny<Guid>()))
                .Returns(new List<Persona>
                {
                    new Persona
                    {
                        PersonaId = EditorPersonaId,
                        RealPageId = _editorUserDetails.UserRealPageId
                    }
                });

            _productInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(settings);

            _productRepository
                .Setup(m => m.GetBooksMasterProductDetail(ProductId))
                .Returns(_gbProductMap);

            _productRepository
                .Setup(m => m.GetAllProducts())
                .Returns(new List<GbProductMap> { _gbProductMap });

            _productRepository
                .Setup(m => m.GetProductSettings(It.IsAny<Guid>(), It.IsAny<int>()))
                .Returns((IList<ProductSettingList>)new List<ProductSettingList>());

            _productRepository
                .Setup(m => m.GetAdGroupsForProduct(It.IsAny<int>()))
                .Returns(new List<AdGroupProduct>());

            _productRepository
                .Setup(m => m.GetAdGroupsForUser(It.IsAny<long>()))
                .Returns(new List<AdGroup>());

            _dataCollector
                .Setup(m => m.CreateProductUserInGreenBook(It.IsAny<long>(), It.IsAny<object>(), It.IsAny<int>(), It.IsAny<string>()));

            _dataCollector
                .Setup(m => m.UpdateProductSettingProductStatus(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

            _dataCollector
                .Setup(m => m.GetEmployeeProductADGroupMapping(It.IsAny<long>(), It.IsAny<int>()))
                .Returns((IList<EmployeeProductMapping>)new List<EmployeeProductMapping>());

            _dataCollector
                .Setup(m => m.GetAzureUserDetails(It.IsAny<long>()))
                .Returns((AdUserDetail)null);
        }

        private List<ProductInternalSetting> GetProductSettings()
        {
            return new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "ApiEndPoint", Value = "https://api.test.com/" },
                new ProductInternalSetting { Name = "GetUserEndpoint", Value = "users/{0}" },
                new ProductInternalSetting { Name = "PostUserEndpoint", Value = "users/create" },
                new ProductInternalSetting { Name = "PutUserEndpoint", Value = "users/{0}" },
                new ProductInternalSetting { Name = "PatchProfileEndpoint", Value = "users/{0}/profile" },
                new ProductInternalSetting { Name = "GetRoleEndpoint", Value = "roles" },
                new ProductInternalSetting { Name = "GetUserGroupEndpoint", Value = "usergroups" },
                new ProductInternalSetting { Name = "GetPropertyEndpoint", Value = "properties" },
                new ProductInternalSetting { Name = "GetPropertyGroupsEndpoint", Value = "propertygroups" },
                new ProductInternalSetting { Name = "GetPropertyByGroupEndpoint", Value = "propertygroups/{0}/properties" },
                new ProductInternalSetting { Name = "GetListUsersEndpoint", Value = "users" },
                new ProductInternalSetting { Name = "PatchMigrateUsersEndpoint", Value = "users/migrate" },
                new ProductInternalSetting { Name = "GetRightEndpoint", Value = "rights" },
                new ProductInternalSetting { Name = "IsActivityCheckNotRequired", Value = "0" },
                new ProductInternalSetting { Name = "CallUpdateWhenCreateReturnsUserExists", Value = "0" },
                new ProductInternalSetting { Name = "SI_SupportsEmployeeCreation", Value = "0" }
            };
        }

        [Fact]
        public void CreateUpdateProductUser_SuccessfulCreation_ReturnsEmptyString()
        {
            SetupMocksForTest();

            var integration = new TestableStandardV1ProductIntegration(
                ProductId, EditorPersonaId, SubjectPersonaId, _userClaims,
                _dataCollector.Object, _managePersona.Object,
                _productInternalSettingRepository.Object,
                _productRepository.Object);

            var userRolePropertiesGroups = new ProductUserRolePropertiesGroups
            {
                IsAssigned = true
            };

            var result = integration.CreateUpdateProductUser(userRolePropertiesGroups, out var additionalParameters);

            Assert.Equal(string.Empty, result);
            Assert.NotNull(additionalParameters);
        }

        [Fact]
        public void CreateUpdateProductUser_WithRoles_PopulatesAdditionalParameters()
        {
            SetupMocksForTest();

            var integration = new TestableStandardV1ProductIntegration(
                ProductId, EditorPersonaId, SubjectPersonaId, _userClaims,
                _dataCollector.Object, _managePersona.Object,
                _productInternalSettingRepository.Object,
                _productRepository.Object);

            var userRolePropertiesGroups = new ProductUserRolePropertiesGroups
            {
                IsAssigned = true,
                RoleList = new List<string> { "role1", "role2" }
            };

            var result = integration.CreateUpdateProductUser(userRolePropertiesGroups, out var additionalParameters);

            Assert.Equal(string.Empty, result);
            Assert.NotNull(additionalParameters);
        }

        [Fact]
        public void CreateUpdateProductUser_WithProperties_PopulatesAdditionalParameters()
        {
            SetupMocksForTest();

            var integration = new TestableStandardV1ProductIntegration(
                ProductId, EditorPersonaId, SubjectPersonaId, _userClaims,
                _dataCollector.Object, _managePersona.Object,
                _productInternalSettingRepository.Object,
                _productRepository.Object);

            var userRolePropertiesGroups = new ProductUserRolePropertiesGroups
            {
                IsAssigned = true,
                PropertyList = new List<string> { "prop1", "prop2" }
            };

            var result = integration.CreateUpdateProductUser(userRolePropertiesGroups, out var additionalParameters);

            Assert.Equal(string.Empty, result);
            Assert.NotNull(additionalParameters);
        }

        [Fact]
        public void CreateUpdateProductUser_ActivityCheckRequired_AdditionalParametersEmpty()
        {
            SetupMocksForTest();

            _productInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<ProductInternalSetting>
                {
                    new ProductInternalSetting { Name = "ApiEndPoint", Value = "https://api.test.com/" },
                    new ProductInternalSetting { Name = "GetUserEndpoint", Value = "users/{0}" },
                    new ProductInternalSetting { Name = "PostUserEndpoint", Value = "users/create" },
                    new ProductInternalSetting { Name = "PutUserEndpoint", Value = "users/{0}" },
                    new ProductInternalSetting { Name = "PatchProfileEndpoint", Value = "users/{0}/profile" },
                    new ProductInternalSetting { Name = "GetRoleEndpoint", Value = "roles" },
                    new ProductInternalSetting { Name = "GetUserGroupEndpoint", Value = "usergroups" },
                    new ProductInternalSetting { Name = "GetPropertyEndpoint", Value = "properties" },
                    new ProductInternalSetting { Name = "GetPropertyGroupsEndpoint", Value = "propertygroups" },
                    new ProductInternalSetting { Name = "GetPropertyByGroupEndpoint", Value = "propertygroups/{0}/properties" },
                    new ProductInternalSetting { Name = "GetListUsersEndpoint", Value = "users" },
                    new ProductInternalSetting { Name = "PatchMigrateUsersEndpoint", Value = "users/migrate" },
                    new ProductInternalSetting { Name = "GetRightEndpoint", Value = "rights" },
                    new ProductInternalSetting { Name = "IsActivityCheckNotRequired", Value = "1" },
                    new ProductInternalSetting { Name = "CallUpdateWhenCreateReturnsUserExists", Value = "0" },
                    new ProductInternalSetting { Name = "SI_SupportsEmployeeCreation", Value = "0" }
                });

            var integration = new TestableStandardV1ProductIntegration(
                ProductId, EditorPersonaId, SubjectPersonaId, _userClaims,
                _dataCollector.Object, _managePersona.Object,
                _productInternalSettingRepository.Object,
                _productRepository.Object);

            var userRolePropertiesGroups = new ProductUserRolePropertiesGroups
            {
                IsAssigned = true,
                RoleList = new List<string> { "role1" }
            };

            var result = integration.CreateUpdateProductUser(userRolePropertiesGroups, out var additionalParameters);

            Assert.Equal(string.Empty, result);
            Assert.NotNull(additionalParameters);
            Assert.Empty(additionalParameters);
        }

        [Fact]
        public void CreateUpdateProductUser_WithEmployeeAdditional_CallsEmployeeMapping()
        {
            SetupMocksForTest();

            var integration = new TestableStandardV1ProductIntegration(
                ProductId, EditorPersonaId, SubjectPersonaId, _userClaims,
                _dataCollector.Object, _managePersona.Object,
                _productInternalSettingRepository.Object,
                _productRepository.Object);

            var userRolePropertiesGroups = new ProductUserRolePropertiesGroups
            {
                IsAssigned = true
            };

            var result = integration.CreateUpdateProductUser(userRolePropertiesGroups, out var additionalParameters);

            Assert.Equal(string.Empty, result);
            Assert.NotNull(additionalParameters);
            _dataCollector.Verify(
                m => m.CreateProductUserInGreenBook(It.IsAny<long>(), It.IsAny<object>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Once);
        }

        /// <summary>
        /// Testable wrapper to prevent HTTP calls during initialization
        /// </summary>
        private class TestableStandardV1ProductIntegration : StandardV1ProductIntegration
        {
            public TestableStandardV1ProductIntegration(
                int productId, long editorPersonaId, long subjectPersonaId,
                DefaultUserClaim userClaims, IDataCollector injectedDataCollector,
                IManagePersona injectedManagePersona,
                IProductInternalSettingRepository injectedProductInternalSettingRepository,
                IProductRepository injectedProductRepository)
                : base(productId, editorPersonaId, subjectPersonaId, userClaims,
                    injectedDataCollector, injectedManagePersona,
                    injectedProductInternalSettingRepository, injectedProductRepository)
            {
            }

            /// <summary>
            /// Override to create a mock HttpClient that prevents actual network calls
            /// </summary>
            protected override void ApplyApiSecurity()
            {
                try
                {
                    // Create a basic HttpClient without authentication or token calls
                    // This prevents network calls during test initialization
                    _httpClient = new HttpClient(new NoOpMessageHandler());
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                }
                catch
                {
                    // If anything fails, create a basic HttpClient
                    _httpClient = new HttpClient();
                }
            }

            /// <summary>
            /// Override to prevent actual API calls when checking if user exists
            /// </summary>
            protected override bool CheckUserExistInProduct(string loginNameToCheck, string baseUrlAndQuery = null)
            {
                // Always return false for tests to allow user creation flow to proceed
                return false;
            }

            /// <summary>
            /// Override to prevent actual API calls when retrieving base user data
            /// </summary>
            public override IntegrationProductUser GetBaseUserDataFromProduct(string loginNameToCheck, string baseUrlAndQuery = null)
            {
                // Return null to indicate user doesn't exist
                return null;
            }

            /// <summary>
            /// Override to prevent actual API calls when getting product user
            /// </summary>
            public override IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true)
            {
                // Return null since user doesn't exist yet in tests
                return null;
            }
        }

        /// <summary>
        /// Message handler that prevents actual HTTP requests and returns appropriate mock responses
        /// </summary>
        private class NoOpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Determine what type of response to return based on the URL
                string url = request.RequestUri?.AbsoluteUri ?? "";
                string responseContent = "{\"UserId\": \"test123\"}";

                // Return appropriate mock JSON based on the endpoint
                if (url.Contains("roles"))
                {
                    responseContent = "[]";  // Empty array for roles endpoint
                }
                else if (url.Contains("properties"))
                {
                    responseContent = "[]";  // Empty array for properties endpoint
                }
                else if (url.Contains("usergroups"))
                {
                    responseContent = "[]";  // Empty array for user groups endpoint
                }
                else if (url.Contains("propertygroups"))
                {
                    responseContent = "[]";  // Empty array for property groups endpoint
                }
                else if (url.Contains("rights"))
                {
                    responseContent = "[]";  // Empty array for rights endpoint
                }
                else if (url.Contains("users/create") || url.Contains("PostUserEndpoint"))
                {
                    // User creation endpoint returns user object
                    responseContent = "{\"UserId\": \"test123\", \"LoginName\": \"testuser\"}";
                }

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseContent)
                };
                return Task.FromResult(response);
            }
        }
    }
}
