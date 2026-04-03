using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Foundation.DataAccess.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using MC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.MarketingCenter;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ProductIntegration
{
    [ExcludeFromCodeCoverage]
    public class ManageProductMarketingCenterTests
    {
        // ─── constants / fixtures ─────────────────────────────────────────────
        private const int ProductId = (int)ProductEnum.MarketingCenter;
        private const long EditorPersonaId = 100L;
        private const long UserPersonaId = 200L;
        private const long PartyId = 300L;

        private const string ApiEndpoint = "https://mc-api.test";
        private const string ApiUsername = "dXNlcg==";   // base64("user")
        private const string ApiPassword = "cGFzcw==";   // base64("pass")
        private const string ApiSourceId  = "SRC-001";
        private const string CompanySourceId = "9999";

        // ─── mocks ────────────────────────────────────────────────────────────
        private readonly Mock<IManagePersona> _managePersona;
        private readonly Mock<ISamlRepository> _samlRepository;
        private readonly Mock<IManageBlueBook> _blueBook;
        private readonly Mock<IProductRepository> _productRepository;
        private readonly Mock<IProductInternalSettingRepository> _productInternalSettingRepository;
        private readonly Mock<IRepository> _repository;

        private readonly DefaultUserClaim _userClaims;
        private readonly Persona _editorPersona;
        private readonly Persona _userPersona;

        public ManageProductMarketingCenterTests()
        {
            _userClaims = new DefaultUserClaim
            {
                LoginName        = "editor@test.com",
                CorrelationId    = Guid.NewGuid(),
                OrganizationName = "TestOrg",
                OrganizationPartyId       = PartyId,
                OrganizationRealPageGuid  = Guid.NewGuid(),
                OrganizationMasterId      = 1,
                UserRealPageGuid          = Guid.NewGuid(),
                PersonaId                 = EditorPersonaId,
                UserId                    = 1,
                ImpersonatedByName        = string.Empty
            };

            _editorPersona = new Persona
            {
                PersonaId  = EditorPersonaId,
                RealPageId = _userClaims.UserRealPageGuid,
                Organization = new Organization
                {
                    PartyId               = PartyId,
                    RealPageId            = _userClaims.OrganizationRealPageGuid,
                    BooksCustomerMasterId = 1,
                    OrganizationDomain    = new OrganizationDomain { Name = "testdomain.com" },
                    organizationType      = new OrganizationType { Name = "owner" }
                }
            };

            _userPersona = new Persona
            {
                PersonaId        = UserPersonaId,
                RealPageId       = Guid.NewGuid(),
                UserTypeId       = (int)UserTypeConstants.RegularUser,
                OrganizationPartyId = PartyId,
                Organization = new Organization
                {
                    PartyId               = PartyId,
                    RealPageId            = Guid.NewGuid(),
                    organizationType      = new OrganizationType { Name = "owner" },
                    RelationshipType      = "Other",
                    RoleNameFrom          = "Employee"
                }
            };

            _managePersona                   = new Mock<IManagePersona>();
            _samlRepository                  = new Mock<ISamlRepository>();
            _blueBook                        = new Mock<IManageBlueBook>();
            _productRepository               = new Mock<IProductRepository>();
            _productInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _repository                      = new Mock<IRepository>();

            // Default: editor persona verifies
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);
            _samlRepository.Setup(m => m.GetProductSamlDetails(It.IsAny<long>(), It.IsAny<int>()))
                           .Returns(new List<SamlAttributes>());
            _productInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
                .Returns(BuildRequiredSettings());

            // The unit-test base ctor (repository, messageHandler) creates real ProductInternalSettingRepository
            // and ProductRepository wrappers around IRepository before the derived ctor can override them.
            // Set up IRepository so those wrappers return valid data and don't crash the base ctor.
            _repository.Setup(m => m.GetMany<IC.ProductInternalSetting>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(BuildRequiredSettings());
            _repository.Setup(m => m.GetMany<GbProductMap>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<GbProductMap>
                       {
                           new GbProductMap { ProductId = ProductId, BooksProductCode = "MC" }
                       });

            _blueBook.Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { Source = "MC", CompanyInstanceSourceId = CompanySourceId }
                });

            // OrganizationRepository.ListOrganizationType() and ListOrganizationDomain() are called
            // via the real UserLoginRepository → OrganizationRepository path when IRepository is injected.
            // Must return List<T> explicitly — the runtime binder cannot convert OrganizationType[] to List<OrganizationType>.
            _repository.Setup(m => m.GetMany<OrganizationType>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<OrganizationType>
                       {
                           new OrganizationType { OrganizationTypeId = 1, Name = "owner" }
                       });
            _repository.Setup(m => m.GetMany<OrganizationDomain>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<OrganizationDomain>
                       {
                           new OrganizationDomain { OrganizationDomainId = 1, Name = "Primary" }
                       });

            // ManageMarketingCenterUser calls _userLoginRepository.ListOrganizationByEnterpriseUserId.
            // The real UserLoginRepository wraps IRepository; stub IRepository.GetMany<Organization> too.
            _repository.Setup(m => m.GetMany<Organization>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<Organization>
                       {
                           new Organization
                           {
                               PartyId          = PartyId,
                               RealPageId       = _userClaims.OrganizationRealPageGuid,
                               RelationshipType = "Other",
                               RoleNameFrom     = "Employee"
                           }
                       });
            // ManagePartyRelationship wraps _repository; return null so IsRegularUserNoEmail returns false.
            _repository.Setup(m => m.GetOne<IC.PartyRelationship>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns((IC.PartyRelationship)null);
            // GetUserActivityLogInfo calls _managePerson.GetPerson → _repository.GetOne<IC.Person>; must not return null.
            _repository.Setup(m => m.GetOne<IC.Person>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new IC.Person { FirstName = "Test", LastName = "User" });
            // UpdateUserProfile path calls _manageUserLogin.GetUserPersonaOrganization → GetMany<UserOrganization>.
            _repository.Setup(m => m.GetMany<UserOrganization>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<UserOrganization>());

            // UpdateProductSettingProductStatus (called by UnassignUser/UpdateUserProfile on Deleted/Inactive)
            // needs ListProductSettingType, GetUserLoginOnly, and GetUserOrganizationWithStatus.
            _productRepository.Setup(m => m.ListProductSettingType())
                               .Returns(new List<ProductSettingType>());
            // _manageUserLogin is a real ManageUserLogin wrapping _repository; stub GetOne<UserLoginOnly>.
            _repository.Setup(m => m.GetOne<UserLoginOnly>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new UserLoginOnly
                       {
                           UserId    = 1,
                           LoginName = "test@test.com",
                           RealPageId = _userClaims.UserRealPageGuid
                       });
            // _userLoginRepository is a real UserLoginRepository wrapping _repository; stub GetMany<OrganizationStatus>.
            _repository.Setup(m => m.GetMany<OrganizationStatus>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<OrganizationStatus>
                       {
                           new OrganizationStatus { PartyId = PartyId }
                       });
        }

        // ─── helpers ─────────────────────────────────────────────────────────

        private List<IC.ProductInternalSetting> BuildRequiredSettings(
            string endpoint    = ApiEndpoint,
            string sourceId    = ApiSourceId,
            string username    = ApiUsername,
            string password    = ApiPassword) =>
            new List<IC.ProductInternalSetting>
            {
                new IC.ProductInternalSetting { Name = "APIENDPOINT",               Value = endpoint  },
                new IC.ProductInternalSetting { Name = "MARKETINGCENTERAPISOURCEID", Value = sourceId  },
                new IC.ProductInternalSetting { Name = "APIUSERNAME",               Value = username  },
                new IC.ProductInternalSetting { Name = "APIPASSWORD",               Value = password  }
            };

        /// <summary>
        /// Builds a test instance of ManageProductMarketingCenter wired to a fake HTTP handler.
        /// </summary>
        private ManageProductMarketingCenter BuildSut(
            HttpMessageHandler httpHandler = null,
            IList<SamlAttributes> editorSamlAttribs = null,
            IList<SamlAttributes> userSamlAttribs   = null)
        {
            editorSamlAttribs = editorSamlAttribs ?? new List<SamlAttributes>();
            userSamlAttribs   = userSamlAttribs   ?? new List<SamlAttributes>();

            _samlRepository.Setup(m => m.GetProductSamlDetails(EditorPersonaId, ProductId))
                           .Returns(editorSamlAttribs);
            _samlRepository.Setup(m => m.GetProductSamlDetails(UserPersonaId, ProductId))
                           .Returns(userSamlAttribs);

            return new ManageProductMarketingCenter(
                editorRealPageId                : _userClaims.UserRealPageGuid,
                userClaims                      : _userClaims,
                httpMessageHandler              : httpHandler ?? new FakeHttpHandler(HttpStatusCode.OK, "[]"),
                productInternalSettingRepository: _productInternalSettingRepository.Object,
                managePersona                   : _managePersona.Object,
                samlRepository                  : _samlRepository.Object,
                manageBlueBook                  : _blueBook.Object,
                productRepository               : _productRepository.Object,
                repository                      : _repository.Object);
        }

        private static IList<SamlAttributes> EditorSaml(string productUserId = "12345") =>
            new List<SamlAttributes>
            {
                new SamlAttributes { Name = "USERID",        Value = productUserId },
                new SamlAttributes { Name = "PRODUCTUSERNAME", Value = "editor@mc.test" }
            };

        private static IList<SamlAttributes> UserSaml(
            string productUserId    = "67890",
            string productUsername  = "user@mc.test") =>
            new List<SamlAttributes>
            {
                new SamlAttributes { Name = "USERID",          Value = productUserId   },
                new SamlAttributes { Name = "PRODUCTUSERNAME", Value = productUsername }
            };

        // =====================================================================
        // ManageProductMarketingCenterHelpers.ToGBRoles
        // =====================================================================

        [Fact]
        public void ToGBRoles_ReturnsNull_WhenInputIsNull()
        {
            var result = ((IList<MC.Role>)null).ToGBRoles();
            Assert.Null(result);
        }

        [Fact]
        public void ToGBRoles_FiltersInactiveRoles()
        {
            var roles = new List<MC.Role>
            {
                new MC.Role { RoleId = 1, RoleName = "Active",   IsActive = true  },
                new MC.Role { RoleId = 2, RoleName = "Inactive", IsActive = false }
            };

            var result = roles.ToGBRoles();

            Assert.Single(result);
            Assert.Equal("1", result[0].ID);
            Assert.Equal("Active", result[0].Name);
        }

        [Fact]
        public void ToGBRoles_ReturnsEmptyList_WhenAllRolesInactive()
        {
            var roles = new List<MC.Role>
            {
                new MC.Role { RoleId = 1, RoleName = "Inactive", IsActive = false }
            };

            var result = roles.ToGBRoles();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ToGBRoles_MapsAllActiveRoles()
        {
            var roles = new List<MC.Role>
            {
                new MC.Role { RoleId = 10, RoleName = "Standard",  IsActive = true },
                new MC.Role { RoleId = 20, RoleName = "Corporate",  IsActive = true }
            };

            var result = roles.ToGBRoles();

            Assert.Equal(2, result.Count);
            // ToGBRoles sorts by role name alphabetically; "Corporate" < "Standard"
            Assert.Equal("20", result[0].ID);
            Assert.Equal("10", result[1].ID);
        }

        // =====================================================================
        // IsDuplicateEmailError (tested via UpdateUserProfile / observable behaviour)
        // =====================================================================

        [Fact]
        public void UpdateUserProfile_ReturnsDuplicateStop_WhenMCReturnsDuplicateEmailError()
        {
            var duplicateErrorBody = JsonConvert.SerializeObject(new
            {
                fieldErrors = new { Error = new { message = "Trying to save a duplicate emailAddress column" } }
            });

            // GET /contact/details → returns user details  (200)
            // PUT /contact/{id}   → returns 500 with duplicate email body
            var handler = new SequentialFakeHttpHandler(new[]
            {
                new FakeResponse(HttpStatusCode.OK,
                    JsonConvert.SerializeObject(new MC.MarketingCenterUserDetails
                    {
                        Id = 67890, CompanyId = 9999, ContactRoleId = 1, AssignNewProperty = false
                    })),
                new FakeResponse(HttpStatusCode.InternalServerError, duplicateErrorBody)
            });

            var sut = BuildSut(handler, EditorSaml(), UserSaml());
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UpdateUserProfile(EditorPersonaId, UserPersonaId);

            Assert.Equal(ProductBatchStatusType.Stop.ToString(), result);
        }

        [Fact]
        public void UpdateUserProfile_ReturnsError_WhenMCReturnsGenericFailure()
        {
            var handler = new SequentialFakeHttpHandler(new[]
            {
                new FakeResponse(HttpStatusCode.OK,
                    JsonConvert.SerializeObject(new MC.MarketingCenterUserDetails
                    {
                        Id = 67890, CompanyId = 9999, ContactRoleId = 1
                    })),
                new FakeResponse(HttpStatusCode.BadRequest, "{\"error\":\"some error\"}")
            });

            var sut = BuildSut(handler, EditorSaml(), UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UpdateUserProfile(EditorPersonaId, UserPersonaId);

            Assert.Contains("problem updating user profile", result);
        }

        [Fact]
        public void UpdateUserProfile_ReturnsUserNotFound_WhenGetDetailsFails()
        {
            // GET /contact/details → 404
            var handler = new FakeHttpHandler(HttpStatusCode.NotFound, "{}");

            var sut = BuildSut(handler, EditorSaml(), UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UpdateUserProfile(EditorPersonaId, UserPersonaId);

            Assert.Equal("User not found in product", result);
        }

        [Fact]
        public void UpdateUserProfile_ReturnsInvalidPersona_WhenEditorInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.UpdateUserProfile(EditorPersonaId, UserPersonaId);

            Assert.Equal("Invalid persona", result);
        }

        // =====================================================================
        // GetLoginName — null-safe impersonation path
        // =====================================================================

        [Fact]
        public void UnassignUser_UsesLoginName_WhenNotImpersonating()
        {
            // ImpersonatedByName is empty → GetLoginName returns _userClaims.LoginName
            // This exercises the GetLoginName() null-safe path (no IUserRepository call)
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
            var sut = BuildSut(handler, EditorSaml("77777"), UserSaml("88888"));
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            // SetMarketingCenterUserStatus → PUT returns 200 = success
            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            // Should succeed (deactivated) rather than fail due to NullReferenceException in GetLoginName
            Assert.Equal(string.Empty, result);
        }

        // =====================================================================
        // UnassignUser
        // =====================================================================

        [Fact]
        public void UnassignUser_ReturnsInvalidPersona_WhenEditorPersonaNull()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            Assert.Equal("Invalid persona", result);
        }

        [Fact]
        public void UnassignUser_ReturnsError_WhenEditorProductUserIdIsNull()
        {
            // No SAML attributes → _editorProductUserId = null → treated as invalid admin userId
            var sut = BuildSut(editorSamlAttribs: new List<SamlAttributes>(), userSamlAttribs: UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            Assert.Contains("Invalid admin userId", result);
        }

        [Fact]
        public void UnassignUser_ReturnsError_WhenEditorProductUserIdIsNonNumeric()
        {
            // _editorProductUserId is not parseable → treated as invalid admin userId
            var sut = BuildSut(
                editorSamlAttribs: new List<SamlAttributes>
                {
                    new SamlAttributes { Name = "USERID", Value = "NOT_A_NUMBER" }
                },
                userSamlAttribs: UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            Assert.Contains("Invalid admin userId", result);
        }

        [Fact]
        public void UnassignUser_ReturnsError_WhenProductUserIdIsZero()
        {
            // _productUserId = "0" → SetMarketingCenterUserStatus rejects mcUserId "0" → returns error
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
            var sut = BuildSut(
                handler,
                editorSamlAttribs: EditorSaml("12345"),
                userSamlAttribs  : new List<SamlAttributes>
                {
                    new SamlAttributes { Name = "USERID", Value = "0" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            Assert.Contains("UnassignUser errored", result);
        }

        [Fact]
        public void UnassignUser_ReturnsError_WhenSetStatusFails()
        {
            // Sequence: GET user status (IsUserIdValid) → 200, PUT status → 500
            var handler = new SequentialFakeHttpHandler(new[]
            {
                new FakeResponse(HttpStatusCode.OK, "{}"),
                new FakeResponse(HttpStatusCode.InternalServerError, "{\"error\":\"failed\"}")
            });
            var sut = BuildSut(handler, EditorSaml("12345"), UserSaml("67890"));
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            Assert.Contains("UnassignUser errored", result);
            Assert.Contains(UserPersonaId.ToString(), result);
        }

        [Fact]
        public void UnassignUser_ReturnsEmpty_WhenDeactivationSucceeds()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
            var sut = BuildSut(handler, EditorSaml("12345"), UserSaml("67890"));
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void UnassignUser_ReturnsInvalidPersona_WhenUserPersonaIsFromDifferentOrg()
        {
            var differentOrgPersona = new Persona
            {
                PersonaId    = UserPersonaId,
                RealPageId   = Guid.NewGuid(),
                Organization = new Organization { PartyId = 99999L }
            };
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(differentOrgPersona);

            var sut = BuildSut(editorSamlAttribs: EditorSaml());
            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId);

            Assert.Equal("Invalid user persona", result);
        }

        // =====================================================================
        // GetRequiredProductSetting — missing setting throws descriptive exception
        // =====================================================================

        [Fact]
        public void Constructor_Throws_WhenRequiredSettingMissing()
        {
            _productInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
                .Returns(new List<IC.ProductInternalSetting>());  // empty — missing all settings
            // The base ctor loads _productInternalSettingList from _repository (not the overridden mock above).
            // Override _repository too so the base ctor also sees empty settings.
            _repository.Setup(m => m.GetMany<IC.ProductInternalSetting>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<IC.ProductInternalSetting>());

            var ex = Assert.Throws<InvalidOperationException>(() =>
                BuildSut());

            Assert.Contains("APIENDPOINT", ex.Message);
        }

        [Fact]
        public void Constructor_Throws_WhenApiUsernameSettingMissing()
        {
            var settingsWithoutUsername = new List<IC.ProductInternalSetting>
            {
                new IC.ProductInternalSetting { Name = "APIENDPOINT",               Value = ApiEndpoint },
                new IC.ProductInternalSetting { Name = "MARKETINGCENTERAPISOURCEID", Value = ApiSourceId },
                // APIUSERNAME missing
                new IC.ProductInternalSetting { Name = "APIPASSWORD",               Value = ApiPassword }
            };
            _productInternalSettingRepository
                .Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
                .Returns(settingsWithoutUsername);
            // Override _repository too so the base ctor also sees settings without APIUSERNAME.
            _repository.Setup(m => m.GetMany<IC.ProductInternalSetting>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(settingsWithoutUsername);

            var ex = Assert.Throws<InvalidOperationException>(() => BuildSut());

            Assert.Contains("APIUSERNAME", ex.Message);
        }

        // =====================================================================
        // GetRoles — invalid persona
        // =====================================================================

        [Fact]
        public void GetRoles_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, 0, null);

            Assert.True(result.IsError);
            Assert.Equal("Invalid persona", result.ErrorReason);
        }

        [Fact]
        public void GetRoles_ReturnsRoles_WhenApiReturnsValidJson()
        {
            var rolesJson = JsonConvert.SerializeObject(new List<MC.Role>
            {
                new MC.Role { RoleId = 1, RoleName = "Standard", IsActive = true },
                new MC.Role { RoleId = 2, RoleName = "Admin",    IsActive = true }
            });

            var handler = new FakeHttpHandler(HttpStatusCode.OK, rolesJson);
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.GetRoles(EditorPersonaId, 0, null);

            Assert.False(result.IsError);
            Assert.Equal(2, result.Records.Count);
        }

        [Fact]
        public void GetRoles_ReturnsEmptyList_WhenApiReturnsNullBody()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "null");
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.GetRoles(EditorPersonaId, 0, null);

            Assert.False(result.IsError);
            Assert.Empty(result.Records);
        }

        [Fact]
        public void GetRoles_ReturnsError_WhenApiCallFails()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.InternalServerError, "{}");
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.GetRoles(EditorPersonaId, 0, null);

            Assert.True(result.IsError);
        }

        [Fact]
        public void GetRoles_FiltersOutInactiveRolesFromApiResponse()
        {
            var rolesJson = JsonConvert.SerializeObject(new List<MC.Role>
            {
                new MC.Role { RoleId = 1, RoleName = "Active",   IsActive = true  },
                new MC.Role { RoleId = 2, RoleName = "Inactive", IsActive = false }
            });

            var handler = new FakeHttpHandler(HttpStatusCode.OK, rolesJson);
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.GetRoles(EditorPersonaId, 0, null);

            Assert.Equal(1, result.Records.Count);
        }

        // =====================================================================
        // GetProperties
        // =====================================================================

        [Fact]
        public void GetProperties_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetProperties(EditorPersonaId, 0, null);

            Assert.True(result.IsError);
        }

        [Fact]
        public void GetProperties_ReturnsEmpty_WhenApiReturnsNoProperties()
        {
            // GetProperties(editorPersonaId, 0, null) makes exactly one HTTP call to /external/properties.
            // userPersonaId=0 → skips GetUserDetails; the API returns an empty property list.
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "[]");
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.GetProperties(EditorPersonaId, 0, null);

            Assert.False(result.IsError);
        }

        // =====================================================================
        // ManageMarketingCenterUser — validation / guard paths
        // =====================================================================

        [Fact]
        public void ManageMarketingCenterUser_ReturnsStop_WhenRoleListIsEmpty_NotSuperUser()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "[]");
            var sut = BuildSut(handler, EditorSaml(), UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            List<AdditionalParameters> additionalParams;
            var result = sut.ManageMarketingCenterUser(
                EditorPersonaId, UserPersonaId,
                RoleList: new List<int>(),          // empty
                PropertyList: new List<string> { "100" },
                IsAssignedNewPropertyByDefault: false,
                additionalParameters: out additionalParams);

            Assert.Equal(ProductBatchStatusType.Stop.ToString(), result);
        }

        [Fact]
        public void ManageMarketingCenterUser_ReturnsStop_WhenPropertyListIsEmpty_NotSuperUser()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "[]");
            var sut = BuildSut(handler, EditorSaml(), UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            List<AdditionalParameters> additionalParams;
            var result = sut.ManageMarketingCenterUser(
                EditorPersonaId, UserPersonaId,
                RoleList: new List<int> { 1 },
                PropertyList: new List<string>(),   // empty
                IsAssignedNewPropertyByDefault: false,
                additionalParameters: out additionalParams);

            Assert.Equal(ProductBatchStatusType.Stop.ToString(), result);
        }

        [Fact]
        public void ManageMarketingCenterUser_ReturnsStop_WhenNullRoleList()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "[]");
            var sut = BuildSut(handler, EditorSaml(), UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            List<AdditionalParameters> additionalParams;
            var result = sut.ManageMarketingCenterUser(
                EditorPersonaId, UserPersonaId,
                RoleList: null,                     // null
                PropertyList: new List<string> { "100" },
                IsAssignedNewPropertyByDefault: false,
                additionalParameters: out additionalParams);

            Assert.Equal(ProductBatchStatusType.Stop.ToString(), result);
        }

        [Fact]
        public void ManageMarketingCenterUser_ReturnsInvalidPersona_WhenEditorInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            List<AdditionalParameters> additionalParams;
            var result = sut.ManageMarketingCenterUser(
                EditorPersonaId, UserPersonaId,
                new List<int> { 1 }, new List<string> { "100" }, false, out additionalParams);

            Assert.Equal("Invalid persona", result);
        }

        [Fact]
        public void ManageMarketingCenterUser_SkipsInvalidPropertyId_DoesNotThrow()
        {
            // Roles API returns one role, properties API returns one property
            var rolesJson     = JsonConvert.SerializeObject(new List<MC.Role>
                { new MC.Role { RoleId = 1, RoleName = "Standard", IsActive = true } });
            var detailsJson   = JsonConvert.SerializeObject(new MC.MarketingCenterUserDetails
                { Id = 0, AssignedProperties = new List<MC.Property>() });
            var propertiesJson = detailsJson;

            var handler = new SequentialFakeHttpHandler(new[]
            {
                new FakeResponse(HttpStatusCode.OK, rolesJson),         // GetRoles
                new FakeResponse(HttpStatusCode.OK, propertiesJson),    // GetProperties (GetUserDetails)
                new FakeResponse(HttpStatusCode.OK, propertiesJson),    // GetProperties (API call)
                new FakeResponse(HttpStatusCode.OK, "{}"),              // Check user exists (GET)
                new FakeResponse(HttpStatusCode.OK, "{}"),              // POST/PUT create
            });

            var sut = BuildSut(handler, EditorSaml(), UserSaml());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            List<AdditionalParameters> additionalParams;
            var exception = Record.Exception(() =>
                sut.ManageMarketingCenterUser(
                    EditorPersonaId, UserPersonaId,
                    RoleList: new List<int> { 1 },
                    PropertyList: new List<string> { "INVALID_ID", "100" },   // one bad
                    IsAssignedNewPropertyByDefault: false,
                    additionalParameters: out additionalParams));

            Assert.Null(exception);
        }

        // =====================================================================
        // GetMCUniqueUserName — capped loop, returns empty after 100 attempts
        // =====================================================================

        [Fact]
        public void ManageMarketingCenterUser_ReturnsError_WhenAllUsernameAttemptsFail()
        {
            // Scenario: _productUsername is empty (no SAML USERID) → username generation
            //           Every CheckIfUserExistInProduct returns 200 (user exists) → all 100 slots taken

            // GetRoles and GetProperties succeed; remaining calls (CheckIfUserExistInProduct × 100)
            // return {} (200 OK) by default from SequentialFakeHttpHandler when exhausted.
            var rolesJson = JsonConvert.SerializeObject(new List<MC.Role>
                { new MC.Role { RoleId = 1, RoleName = "Standard", IsActive = true } });
            var handler = new SequentialFakeHttpHandler(new[]
            {
                new FakeResponse(HttpStatusCode.OK, rolesJson),  // GetRoles
                new FakeResponse(HttpStatusCode.OK, "[]"),        // GetProperties
            });

            // Provide editor SAML but NO user SAML → _productUsername = ""
            var sut = BuildSut(handler, EditorSaml(), new List<SamlAttributes>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            // Because SequentialFakeHttpHandler falls back to {} (200) after the initial responses,
            // CheckIfUserExistInProduct returns 200 for all attempts → username loop exhausted.

            List<AdditionalParameters> additionalParams;
            var result = sut.ManageMarketingCenterUser(
                EditorPersonaId, UserPersonaId,
                RoleList: new List<int> { 1 },
                PropertyList: new List<string> { "100" },
                IsAssignedNewPropertyByDefault: false,
                additionalParameters: out additionalParams);

            Assert.Contains("Unable to get username", result);
        }

        // =====================================================================
        // ChangeUserStatus
        // =====================================================================

        [Fact]
        public void ChangeUserStatus_ReturnsFalse_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.ChangeUserStatus(EditorPersonaId, "user@mc.test", "67890", false);

            Assert.False(result);
        }

        [Fact]
        public void ChangeUserStatus_ReturnsFalse_WhenCompanyIdInvalid()
        {
            _blueBook.Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(), It.IsAny<long>(),
        It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { Source = "MC", CompanyInstanceSourceId = "NOT_A_NUMBER" }
                });

            var sut = BuildSut(editorSamlAttribs: EditorSaml());
            var result = sut.ChangeUserStatus(EditorPersonaId, "user@mc.test", "67890", false);

            Assert.False(result);
        }

        [Fact]
        public void ChangeUserStatus_ReturnsTrue_WhenApiSucceeds()
        {
            // Sequence: GET user status (IsUserIdValid) → 200, PUT status → 200
            var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.ChangeUserStatus(EditorPersonaId, "user@mc.test", "67890", true);

            Assert.True(result);
        }

        // =====================================================================
        // GetRolesCount / GetRights
        // =====================================================================

        [Fact]
        public void GetRolesCount_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetRolesCount(EditorPersonaId);

            Assert.True(result.IsError);
        }

        [Fact]
        public void GetRights_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetRights(EditorPersonaId);

            Assert.True(result.IsError);
        }

        // =====================================================================
        // GetMigrationUsers
        // =====================================================================

        [Fact]
        public void GetMigrationUsers_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetMigrationUsers(EditorPersonaId, null);

            Assert.True(result.IsError);
        }

        [Fact]
        public void GetMigrationUsers_ReturnsCompanyError_WhenBlueBookCompanyInvalid()
        {
            _blueBook.Setup(m => m.GetCompanyMap(
                    It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { Source = "MC", CompanyInstanceSourceId = "BADID" }
                });

            var sut = BuildSut(editorSamlAttribs: EditorSaml());
            var result = sut.GetMigrationUsers(EditorPersonaId, null);

            Assert.True(result.IsError);
            Assert.Contains("Company Setup Error", result.ErrorReason);
        }

        [Fact]
        public void GetMigrationUsers_ReturnsError_WhenApiReturnsNull()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.InternalServerError, "{}");
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.GetMigrationUsers(EditorPersonaId, null);

            Assert.True(result.IsError);
        }

        // =====================================================================
        // UpdateUsersMigrationStatus
        // =====================================================================

        [Fact]
        public void UpdateUsersMigrationStatus_ReturnsFalse_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.UpdateUsersMigrationStatus(EditorPersonaId, new List<MigrateUser>());

            Assert.False(result.Status);
        }

        [Fact]
        public void UpdateUsersMigrationStatus_ReturnsCompanyError_WhenBlueBookCompanyInvalid()
        {
            _blueBook.Setup(m => m.GetCompanyMap(
                  It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { Source = "MC", CompanyInstanceSourceId = "NaN" }
                });

            var sut = BuildSut(editorSamlAttribs: EditorSaml());
            var result = sut.UpdateUsersMigrationStatus(EditorPersonaId, new List<MigrateUser>());

            Assert.False(result.Status);
            Assert.Contains("Company Setup Error", result.Message);
        }

        [Fact]
        public void UpdateUsersMigrationStatus_ReturnsFalse_WhenApiCallFails()
        {
            var handler = new FakeHttpHandler(HttpStatusCode.InternalServerError, "{}");
            var sut = BuildSut(handler, EditorSaml());

            var result = sut.UpdateUsersMigrationStatus(EditorPersonaId, new List<MigrateUser>
            {
                new MigrateUser { UnifiedLoginUserName = "test", LeadEmailAddress = "test@test.com" }
            });

            Assert.False(result.Status);
        }

        // =====================================================================
        // SuperUser path — "No Product Properties" guard
        // =====================================================================

        [Fact]
        public void ManageMarketingCenterUser_ReturnsStop_WhenSuperUserAndNoPropertiesInApi()
        {
            // Simulate super user: persona has product status IsActive=true and is in superuser list
            // The test relies on GetProperties returning empty → guard triggers
            var rolesJson = JsonConvert.SerializeObject(new List<MC.Role>
            {
                new MC.Role { RoleId = 5, RoleName = "Corporate Operations", IsActive = true }
            });
            var emptyDetailsJson = JsonConvert.SerializeObject(
                new MC.MarketingCenterUserDetails { AssignedProperties = new List<MC.Property>() });

            var handler = new SequentialFakeHttpHandler(new[]
            {
                new FakeResponse(HttpStatusCode.OK, rolesJson),          // GetRoles
                new FakeResponse(HttpStatusCode.OK, emptyDetailsJson),   // GetProperties / GetUserDetails
                new FakeResponse(HttpStatusCode.OK, emptyDetailsJson),   // GetProperties API
            });

            // IsSuperUser → requires _productRepository to indicate super user status
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                .Returns(new List<ProductSettingList>
                {
                    new ProductSettingList { Name = "IsSuperUser", Value = "1" }
                });

            var sut = BuildSut(handler, EditorSaml(), UserSaml());

            List<AdditionalParameters> additionalParams;
            var result = sut.ManageMarketingCenterUser(
                EditorPersonaId, UserPersonaId,
                RoleList: new List<int> { 5 },
                PropertyList: new List<string>(),
                IsAssignedNewPropertyByDefault: true,
                additionalParameters: out additionalParams);

            // Super user with no properties → Stop
            Assert.Equal(ProductBatchStatusType.Stop.ToString(), result);
        }
    }

    // =========================================================================
    // Fake HTTP infrastructure — self-contained, no external libraries needed
    // =========================================================================

    [ExcludeFromCodeCoverage]
    internal sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public FakeHttpHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content    = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            });
    }

    [ExcludeFromCodeCoverage]
    internal sealed class AlwaysOkHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
    }

    [ExcludeFromCodeCoverage]
    internal sealed class FakeResponse
    {
        public HttpStatusCode StatusCode { get; }
        public string Content { get; }
        public FakeResponse(HttpStatusCode statusCode, string content)
        {
            StatusCode = statusCode;
            Content    = content;
        }
    }

    [ExcludeFromCodeCoverage]
    internal sealed class SequentialFakeHttpHandler : HttpMessageHandler
    {
        private readonly FakeResponse[] _responses;
        private int _index;

        public SequentialFakeHttpHandler(FakeResponse[] responses)
        {
            _responses = responses;
            _index     = 0;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var r = _index < _responses.Length
                ? _responses[_index++]
                : new FakeResponse(HttpStatusCode.OK, "{}");

            return Task.FromResult(new HttpResponseMessage(r.StatusCode)
            {
                Content = new StringContent(r.Content, Encoding.UTF8, "application/json")
            });
        }
    }
}
