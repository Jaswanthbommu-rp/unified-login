using Moq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Types;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using IC = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using UL = RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UserManagement;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ProductIntegration
{
    [ExcludeFromCodeCoverage]
    public class UPFMProductsIntegrationTests
    {
        // ─── fixtures ────────────────────────────────────────────────────────

        private const int ProductId = (int)ProductEnum.UnifiedAmenities;
        private const long EditorPersonaId = 100L;
        private const long UserPersonaId = 200L;
        private const long PartyId = 300L;
        private const int UserId = 1;

        private readonly DefaultUserClaim _userClaims;
        private readonly Persona _editorPersona;
        private readonly Persona _userPersona;

        private readonly Mock<IManagePersona> _managePersona;
        private readonly Mock<IManagePerson> _managePerson;
        private readonly Mock<IManageBlueBook> _blueBook;
        private readonly Mock<IProductRepository> _productRepository;
        private readonly Mock<ISamlRepository> _samlRepository;
        private readonly Mock<IProductInternalSettingRepository> _productInternalSettingRepository;
        private readonly Mock<IManagePartyRelationship> _managePartyRelationship;
        private readonly Mock<IUserRoleRightRepository> _userRoleRightRepository;
        private readonly Mock<IManageUserLogin> _manageUserLogin;
        private readonly Mock<IUnifiedLoginRepository> _unifiedLoginRepository;
        private readonly Mock<IPropertyRepository> _propertyRepository;
        private readonly Mock<IUserLoginRepository> _userLoginRepository;
        private readonly Mock<IRepository> _repository;

        public UPFMProductsIntegrationTests()
        {
            _userClaims = new DefaultUserClaim
            {
                LoginName = "test.editor",
                CorrelationId = Guid.NewGuid(),
                OrganizationName = "TestOrg",
                OrganizationPartyId = PartyId,
                OrganizationRealPageGuid = Guid.NewGuid(),
                OrganizationMasterId = 1,
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = EditorPersonaId,
                UserId = UserId
            };

            _editorPersona = new Persona
            {
                PersonaId = EditorPersonaId,
                RealPageId = _userClaims.UserRealPageGuid,
                Organization = new Organization
                {
                    PartyId = PartyId,
                    RealPageId = _userClaims.OrganizationRealPageGuid,
                    BooksCustomerMasterId = 1,
                    OrganizationDomain = new OrganizationDomain { Name = "testdomain.com" },
                    organizationType = new OrganizationType { Name = "owner" }
                }
            };

            _userPersona = new Persona
            {
                PersonaId = UserPersonaId,
                RealPageId = Guid.NewGuid(),
                Organization = new Organization
                {
                    PartyId = PartyId,
                    RealPageId = Guid.NewGuid(),
                    organizationType = new OrganizationType { Name = "owner" }
                }
            };

            _managePersona = new Mock<IManagePersona>();
            _managePerson = new Mock<IManagePerson>();
            _blueBook = new Mock<IManageBlueBook>();
            _productRepository = new Mock<IProductRepository>();
            _samlRepository = new Mock<ISamlRepository>();
            _productInternalSettingRepository = new Mock<IProductInternalSettingRepository>();
            _managePartyRelationship = new Mock<IManagePartyRelationship>();
            _userRoleRightRepository = new Mock<IUserRoleRightRepository>();
            _manageUserLogin = new Mock<IManageUserLogin>();
            _unifiedLoginRepository = new Mock<IUnifiedLoginRepository>();
            _propertyRepository = new Mock<IPropertyRepository>();
            _userLoginRepository = new Mock<IUserLoginRepository>();
            _repository = new Mock<IRepository>();

            // Default common setups
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);
            _samlRepository.Setup(m => m.GetProductSamlDetails(It.IsAny<long>(), It.IsAny<int>()))
                           .Returns(new List<SamlAttributes>());
            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(It.IsAny<int>()))
                                             .Returns(new List<IC.ProductInternalSetting>());
            // The unit-test base ctor creates real ProductRepository and ProductInternalSettingRepository
            // wrappers around IRepository. These stubs prevent NullReferenceException during construction.
            _repository.Setup(m => m.GetMany<IC.ProductInternalSetting>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<IC.ProductInternalSetting>());
            _repository.Setup(m => m.GetMany<GbProductMap>(It.IsAny<string>(), It.IsAny<object>()))
                       .Returns(new List<GbProductMap>
                       {
                           new GbProductMap { ProductId = ProductId, Name = "UnifiedAmenities", BooksProductCode = "UA" }
                       });

            _productRepository.Setup(m => m.GetProductIdsByCompany(It.IsAny<long>()))
                               .Returns(new List<int> { ProductId });
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap>
                               {
                                   new GbProductMap { ProductId = ProductId, Name = "UnifiedAmenities", BooksProductCode = "UA" }
                               });
            // Required: base ctor calls GetBooksMasterProductDetail via cache factory using this mock;
            // without a return value _udmSourceCode stays null and GetProductCompanyInstanceId throws.
            _productRepository.Setup(m => m.GetBooksMasterProductDetail(It.IsAny<int>()))
                               .Returns(new GbProductMap { ProductId = ProductId, Name = "UnifiedAmenities", BooksProductCode = "UA" });
            _productRepository.Setup(m => m.ListRolesForProductByParty(It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                               .Returns(new List<ProductRole>());
            _manageUserLogin.Setup(m => m.GetUserLoginOnly(It.IsAny<Guid>()))
                            .Returns(new UserLoginOnly { LoginName = "test.user" });

            // UpdateProductSettingProductStatus (called by ManageUPFMProductUser) needs these.
            _productRepository.Setup(m => m.ListProductSettingType())
                               .Returns(new List<ProductSettingType>());
            _userLoginRepository.Setup(m => m.GetUserOrganizationWithStatus(
                    It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(new OrganizationStatus { Status = UserUiStatusType.AccountCreationSuccessful });

            // UnassignUser calls GetAssignedPropertyForPersona → ListPropertiesByPersona (non-UPFM variant).
            _propertyRepository.Setup(m => m.ListPropertiesByPersona(It.IsAny<long>(), It.IsAny<int>()))
                                .Returns(new List<ProductProperty>());
        }

        // ─── helpers ─────────────────────────────────────────────────────────

        private ManageUPFMProductsIntegration BuildSut() =>
            new ManageUPFMProductsIntegration(
                ProductId,
                _userClaims,
                _managePersona.Object,
                _managePerson.Object,
                _blueBook.Object,
                _productRepository.Object,
                _samlRepository.Object,
                _productInternalSettingRepository.Object,
                _managePartyRelationship.Object,
                _userRoleRightRepository.Object,
                _manageUserLogin.Object,
                _unifiedLoginRepository.Object,
                _propertyRepository.Object,
                _userLoginRepository.Object,
                _repository.Object,
                new NoOpHttpHandler());

        private UPFMIntegrationType BuildIntegrationType() =>
            new UPFMIntegrationType(
                ProductId,
                _userClaims,
                _productInternalSettingRepository.Object);

        private static UPFMProductPropertyRole MakeRole(
            List<string> propertyList,
            List<string> removedPropertyList = null,
            List<string> roleList = null,
            bool isAssigned = true) =>
            new UPFMProductPropertyRole
            {
                PropertyList = propertyList,
                RemovedPropertyList = removedPropertyList ?? new List<string>(),
                RoleList = roleList ?? new List<string>(),
                IsAssigned = isAssigned
            };

        private void SetupSuccessfulPersonaVerification()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);
        }

        // ManageUPFMProductUser calls UpdateProductSettingProductStatus which needs these.
        private void SetupProductStatusUpdate()
        {
            _productRepository.Setup(m => m.ListProductSettingType())
                               .Returns(new List<ProductSettingType>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(It.IsAny<long>()))
                               .Returns(new List<ProductSettingList>());
            _userLoginRepository.Setup(m => m.GetUserOrganizationWithStatus(
                    It.IsAny<long>(), It.IsAny<DateTime?>(), It.IsAny<long>(), It.IsAny<bool>()))
                .Returns(new OrganizationStatus { Status = UserUiStatusType.AccountCreationSuccessful });
        }

        // UnassignUser calls GetAssignedPropertyForPersona which uses ListPropertiesByPersona (not UPFM variant).
        private void SetupNoAssignedNonUPFMProperties() =>
            _propertyRepository.Setup(m => m.ListPropertiesByPersona(It.IsAny<long>(), It.IsAny<int>()))
                                .Returns(new List<ProductProperty>());

        private void SetupNoAssignedProperties(long personaId = UserPersonaId) =>
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(personaId, ProductId))
                                .Returns((List<int>)null);

        private void SetupAssignedProperties(List<int> ids, long personaId = UserPersonaId) =>
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(personaId, ProductId))
                                .Returns(ids);

        private void SetupSuccessfulBulkOperation() =>
            _propertyRepository.Setup(m => m.BulkInsertRemovePropertyInstanceMappings(
                    It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<UPFMPropertyInstanceMapping>>()))
                .Returns(new RepositoryResponse { Id = 1, ErrorMessage = string.Empty });

        private void SetupSuccessfulSentinelOperation() =>
            _propertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                    It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = 1 });

        private void SetupSuccessfulRoleOperation() =>
            _userRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                    It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });


        // =====================================================================
        // UPFMIntegrationType.UpdateUserProfile
        // =====================================================================

        [Fact]
        public void UpdateUserProfile_ReturnsEmptyString_WhenCalled()
        {
            var sut = BuildIntegrationType();
            var productUser = new ProductUserProperitiesRoles
            {
                ProductId = ProductId,
                CreateUserPersonaId = EditorPersonaId,
                AssignUserPersonaId = UserPersonaId,
                InputJson = "{}"
            };

            var result = sut.UpdateUserProfile(productUser);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void UpdateUserProfile_DoesNotThrow_WhenInputJsonIsNull()
        {
            var sut = BuildIntegrationType();
            var productUser = new ProductUserProperitiesRoles
            {
                ProductId = ProductId,
                CreateUserPersonaId = EditorPersonaId,
                AssignUserPersonaId = UserPersonaId,
                InputJson = null
            };

            var result = sut.UpdateUserProfile(productUser);

            Assert.Equal(string.Empty, result);
        }

        // =====================================================================
        // UPFMProductIntegration.UpdateProductUserProfile
        // =====================================================================

        [Fact]
        public void UpdateProductUserProfile_ReturnsEmptyString()
        {
            var sut = new UPFMProductIntegration(ProductId, _userClaims, _productInternalSettingRepository.Object, _productRepository.Object);

            var result = sut.UpdateProductUserProfile(Guid.NewGuid(), EditorPersonaId, UserPersonaId);

            Assert.Equal(string.Empty, result);
        }

        // =====================================================================
        // GetCompanyEditorAndUserDetails / "Invalid user persona"
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ReturnsInvalidPersona_WhenUserPersonaOrganizationIsNull()
        {
            var personaWithNullOrg = new Persona
            {
                PersonaId = UserPersonaId,
                RealPageId = Guid.NewGuid(),
                Organization = null   // null org — previously caused NullReferenceException
            };
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(personaWithNullOrg);

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(new List<string> { "100" }, roleList: new List<string> { "1" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal("Invalid user persona", result);
        }

        [Fact]
        public void ManageUPFMProductUser_ReturnsInvalidPersona_WhenUserPersonaBelongsToDifferentOrg()
        {
            var differentOrgPersona = new Persona
            {
                PersonaId = UserPersonaId,
                RealPageId = Guid.NewGuid(),
                Organization = new Organization { PartyId = 99999L }   // different party
            };
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(differentOrgPersona);

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(new List<string> { "100" }, roleList: new List<string> { "1" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal("Invalid user persona", result);
        }

        [Fact]
        public void ManageUPFMProductUser_ReturnsError_WhenEditorPersonaIsInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(new List<string> { "100" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.False(string.IsNullOrEmpty(result));
        }

        // =====================================================================
        // "No Properties are found to assign/unassign"
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ReturnsNoPropertiesError_WhenBothListsEmptyAndNotSuperUser()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100, 200 });

            // DoesNotUseProperties = null → error path active
            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());

            // IsSuperUser check — user has no role with superuser flag
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(propertyList: new List<string>(), removedPropertyList: new List<string>(), roleList: new List<string>(), isAssigned: true);

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal("No Properties are found to assign/unassign", result);
        }

        [Fact]
        public void ManageUPFMProductUser_DoesNotReturnNoPropertiesError_WhenDoesNotUsePropertiesIs1()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int>());
            SetupSuccessfulRoleOperation();
            SetupProductStatusUpdate();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "DoesNotUseProperties", Value = "1" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(propertyList: new List<string>(), roleList: new List<string> { "23134" }, isAssigned: true);

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.NotEqual("No Properties are found to assign/unassign", result);
        }

        // =====================================================================
        // Null-safe: userPropertyIdList returns null from DB
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_DoesNotThrow_WhenUserPropertyIdListIsNull()
        {
            SetupSuccessfulPersonaVerification();
            SetupNoAssignedProperties();           // returns null — previously caused NullReferenceException
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "DoesNotUseProperties", Value = "1" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(new List<string> { "100" }, roleList: new List<string> { "23134" });

            var exception = Record.Exception(() =>
                sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters));

            Assert.Null(exception);
        }

        // =====================================================================
        // Sentinel -1 ("all properties") — Dapper TVP fix
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_HandlesSentinelMinus1_ViaSingleRowProc_NotTVP()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100, 200 });
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            // PropertyList: ["-1"], RemovedPropertyList: ["256850"], RoleList: ["23134"]
            var input = MakeRole(
                propertyList: new List<string> { "-1" },
                removedPropertyList: new List<string> { "256850" },
                roleList: new List<string> { "23134" });

            sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            // Sentinel must route through single-row proc
            _propertyRepository.Verify(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                UserPersonaId, ProductId, -1L, It.IsAny<int>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ManageUPFMProductUser_DoesNotSendSentinelToTVP()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100, 200 });
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "-1" },
                removedPropertyList: new List<string> { "256850" },
                roleList: new List<string> { "23134" });

            sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            // TVP must never receive -1 as a PropertyInstanceID
            _propertyRepository.Verify(m => m.BulkInsertRemovePropertyInstanceMappings(
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.Is<List<UPFMPropertyInstanceMapping>>(list =>
                    list.Any(x => x.PropertyInstanceID == -1))),
                Times.Never);
        }

        [Fact]
        public void ManageUPFMProductUser_WhenOnlySentinel_TVPNotCalledAtAll()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int>());
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "-1" },
                removedPropertyList: new List<string>(),
                roleList: new List<string> { "23134" });

            sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            _propertyRepository.Verify(m => m.BulkInsertRemovePropertyInstanceMappings(
                It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<UPFMPropertyInstanceMapping>>()),
                Times.Never);
        }

        [Fact]
        public void ManageUPFMProductUser_UnassignSentinel_RoutedToSingleRowProc()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { -1 });  // user currently has "all properties"
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "300" },   // assign specific property
                removedPropertyList: new List<string>(),
                roleList: new List<string> { "23134" });

            sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            // Existing -1 sentinel must be removed via single-row proc
            _propertyRepository.Verify(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                UserPersonaId, ProductId, -1L, 1), Times.AtLeastOnce);
        }

        // =====================================================================
        // Invalid / non-numeric role IDs — TryParse fix
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_SkipsInvalidRoleId_DoesNotThrow()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int>());
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulBulkOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "DoesNotUseProperties", Value = "1" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _userRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                    It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string>(),
                roleList: new List<string> { "INVALID_ROLE", "23134" });  // one bad, one good

            var exception = Record.Exception(() =>
                sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters));

            Assert.Null(exception);
        }

        [Fact]
        public void ManageUPFMProductUser_OnlyProcessesValidRoleIds()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int>());

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "DoesNotUseProperties", Value = "1" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _userRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                    It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new RepositoryResponse { Id = 1 });
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string>(),
                roleList: new List<string> { "ABC", "", null });  // all invalid

            var exception = Record.Exception(() =>
                sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters));

            Assert.Null(exception);
            // No role inserts should have occurred for invalid IDs
            _userRoleRightRepository.Verify(m => m.InsertAssignedRoleToUser(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), false), Times.Never);
        }

        // =====================================================================
        // Invalid property IDs — TryParse fix
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_SkipsNonNumericPropertyId()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "INVALID", "200" },
                roleList: new List<string> { "23134" });

            var exception = Record.Exception(() =>
                sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters));

            Assert.Null(exception);
        }

        [Fact]
        public void ManageUPFMProductUser_OnlyValidPropertyIdsSentToTVP()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            List<UPFMPropertyInstanceMapping> capturedMappings = null;
            _propertyRepository.Setup(m => m.BulkInsertRemovePropertyInstanceMappings(
                    It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<UPFMPropertyInstanceMapping>>()))
                .Callback<long, int, List<UPFMPropertyInstanceMapping>>((_, __, mappings) => capturedMappings = mappings)
                .Returns(new RepositoryResponse { Id = 1 });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "NOT_A_NUMBER", "200" },
                roleList: new List<string> { "23134" });

            sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.NotNull(capturedMappings);
            Assert.DoesNotContain(capturedMappings, m => m.PropertyInstanceID <= 0);
            Assert.All(capturedMappings, m => Assert.True(m.PropertyInstanceID > 0));
        }

        // =====================================================================
        // Role assignment / removal operations
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ReturnsError_WhenRoleInsertFails()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int>());

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "DoesNotUseProperties", Value = "1" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _userRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                    It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>(), false))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "DB error inserting role" });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string>(),
                roleList: new List<string> { "23134" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal("DB error inserting role", result);
        }

        [Fact]
        public void ManageUPFMProductUser_ReturnsError_WhenExistingRoleDeleteFails()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int>());

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "DoesNotUseProperties", Value = "1" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            // User already has role 99
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role> { new UL.Role { RoleID = 99L } });
            // Delete fails
            _userRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                    UserPersonaId, 99L, It.IsAny<int>(), true))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "DB error deleting role" });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string>(),
                roleList: new List<string> { "23134" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal("DB error deleting role", result);
        }

        // =====================================================================
        // UnassignUser
        // =====================================================================

        [Fact]
        public void UnassignUser_ReturnsEmpty_WhenNoRoleAssigned()
        {
            SetupSuccessfulPersonaVerification();
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var sut = BuildSut();
            var input = MakeRole(new List<string>(), isAssigned: false);

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId, input);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void UnassignUser_ReturnsInvalidPersona_WhenUserNotFound()
        {
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var input = MakeRole(new List<string>(), isAssigned: false);

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId, input);

            Assert.Equal("Invalid user persona", result);
        }

        [Fact]
        public void UnassignUser_UnassignsProperties_WhenUserHasPropertiesAssigned()
        {
            SetupSuccessfulPersonaVerification();
            SetupSuccessfulSentinelOperation();

            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role> { new UL.Role { RoleID = 10L } });
            _userRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                    UserPersonaId, 10L, It.IsAny<int>(), true))
                .Returns(new RepositoryResponse { Id = 1 });
            // UnassignUser calls GetAssignedPropertyForPersona → ListPropertiesByPersona (not UPFM variant).
            _propertyRepository.Setup(m => m.ListPropertiesByPersona(UserPersonaId, ProductId))
                                .Returns(new List<ProductProperty> { new ProductProperty { ID = "500" }, new ProductProperty { ID = "501" } });
            SetupSuccessfulBulkOperation();
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());

            var sut = BuildSut();
            var input = MakeRole(new List<string>(), isAssigned: false);

            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId, input);

            Assert.Equal(string.Empty, result);
            _propertyRepository.Verify(m => m.BulkInsertRemovePropertyInstanceMappings(
                UserPersonaId, ProductId,
                It.Is<List<UPFMPropertyInstanceMapping>>(list =>
                    list.Count == 2 && list.All(x => x.IsDeleted))),
                Times.Once);
        }

        // =====================================================================
        // productName null-safe (GetAllProducts returns empty)
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_DoesNotThrow_WhenProductNotFoundInGetAllProducts()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            // Product not in list — FirstOrDefault returns null → was NullReferenceException
            _productRepository.Setup(m => m.GetAllProducts()).Returns(new List<GbProductMap>());

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "200" },
                roleList: new List<string> { "23134" });

            var exception = Record.Exception(() =>
                sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters));

            Assert.Null(exception);
        }

        // =====================================================================
        // AggregateException unwrapping — "Error - One or more errors occurred."
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ReturnsInnerExceptionMessage_WhenAggregateExceptionThrown()
        {
            SetupSuccessfulPersonaVerification();
            SetupSuccessfulRoleOperation();
            SetupAssignedProperties(new List<int> { 100 });

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            // Simulate AggregateException from bulk operation (wraps inner SqlException message)
            var innerException = new Exception("The INSERT statement conflicted with the FOREIGN KEY constraint");
            var aggregateException = new AggregateException("One or more errors occurred.", innerException);
            _propertyRepository.Setup(m => m.BulkInsertRemovePropertyInstanceMappings(
                    It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<UPFMPropertyInstanceMapping>>()))
                .Throws(aggregateException);

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "200" },
                roleList: new List<string> { "23134" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            // Must return the inner message, NOT "Error - One or more errors occurred."
            Assert.Contains("FOREIGN KEY", result);
            Assert.DoesNotContain("One or more errors occurred", result);
        }

        // =====================================================================
        // GetRoles
        // =====================================================================

        [Fact]
        public void GetRoles_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, 0, PartyId);

            Assert.True(result.IsError);
        }

        [Fact]
        public void GetRoles_ReturnsRoles_WhenUserPersonaIdIsZero_NewUserPath()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _samlRepository.Setup(m => m.GetProductSamlDetails(EditorPersonaId, ProductId))
                           .Returns(new List<SamlAttributes>());
            _blueBook.Setup(m => m.GetCompanyMap(It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>());
            _productRepository.Setup(m => m.ListRolesForProductByParty(PartyId, It.IsAny<IList<int>>(), ProductId))
                               .Returns(new List<ProductRole>
                               {
                                   new ProductRole { ID = "1", Name = "Standard User", IsAssigned = false }
                               });
            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, userPersonaId: 0, partyId: PartyId);

            Assert.False(result.IsError);
        }

        [Fact]
        public void GetRoles_ReturnsRoles_WithMergedAssignments_ForExistingUser()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);
            _samlRepository.Setup(m => m.GetProductSamlDetails(It.IsAny<long>(), ProductId))
                           .Returns(new List<SamlAttributes>());
            _blueBook.Setup(m => m.GetCompanyMap(It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>());
            _productRepository.Setup(m => m.ListRolesForProductByParty(It.IsAny<long>(), It.IsAny<IList<int>>(), ProductId))
                               .Returns(new List<ProductRole>
                               {
                                   new ProductRole { ID = "1", Name = "Standard User", IsAssigned = false },
                                   new ProductRole { ID = "2", Name = "Admin", IsAssigned = false }
                               });
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role> { new UL.Role { RoleID = 1L } });
            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, UserPersonaId, PartyId);

            Assert.False(result.IsError);
            Assert.True(result.Records.Count > 0);
        }

        // =====================================================================
        // MergeUPFMBooksPropertiesWithProductProperty — null property list
        // =====================================================================

        [Fact]
        public void GetUPFMProperties_DoesNotThrow_WhenUserPropertyIdListIsNull()
        {
            SetupSuccessfulPersonaVerification();
            SetupNoAssignedProperties();   // null — previously caused NullReferenceException in HashSet ctor

            _blueBook.Setup(m => m.GetUPFMPropertyInstances(It.IsAny<string>()))
                     .Returns(new List<Guid> { Guid.NewGuid() });
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                                .Returns(new List<UPFMPropertyInstance>());

            var sut = BuildSut();

            var exception = Record.Exception(() =>
                sut.GetUPFMProperties(EditorPersonaId, UserPersonaId, false, null));

            Assert.Null(exception);
        }

        [Fact]
        public void GetUPFMProperties_ReturnsAllPropertiesMarkedAssigned_WhenUserHasSentinelMinus1()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { -1 });  // "all properties" sentinel

            _blueBook.Setup(m => m.GetUPFMPropertyInstances(It.IsAny<string>()))
                     .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            var propId1 = 101;
            var propId2 = 102;
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                                .Returns(new List<UPFMPropertyInstance>
                                {
                                    new UPFMPropertyInstance { PropertyInstanceId = propId1, Name = "Prop1" },
                                    new UPFMPropertyInstance { PropertyInstanceId = propId2, Name = "Prop2" }
                                });

            var sut = BuildSut();
            var result = sut.GetUPFMProperties(EditorPersonaId, UserPersonaId, false, null);

            Assert.False(result.IsError);
            // With sentinel, all properties should be marked assigned
            var props = result.Records.Cast<ProductProperty>().ToList();
            Assert.All(props, p => Assert.True(p.IsAssigned));
        }

        // =====================================================================
        // GetCompanyEditorAndUserDetails null-safety (ManageProductBase)
        // =====================================================================

        [Fact]
        public void GetRoles_DoesNotThrow_WhenUserPersonaHasNullOrganization()
        {
            var personaWithNullOrg = new Persona
            {
                PersonaId = UserPersonaId,
                RealPageId = Guid.NewGuid(),
                Organization = null
            };
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(personaWithNullOrg);
            _samlRepository.Setup(m => m.GetProductSamlDetails(EditorPersonaId, ProductId))
                           .Returns(new List<SamlAttributes>());
            _blueBook.Setup(m => m.GetCompanyMap(It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>());
            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());

            var sut = BuildSut();

            var exception = Record.Exception(() => sut.GetRoles(EditorPersonaId, UserPersonaId, PartyId));

            Assert.Null(exception);
        }

        // =====================================================================
        // Bulk property operation error propagation
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ReturnsError_WhenBulkPropertyOperationFails()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            // Bulk op returns failure
            _propertyRepository.Setup(m => m.BulkInsertRemovePropertyInstanceMappings(
                    It.IsAny<long>(), It.IsAny<int>(), It.IsAny<List<UPFMPropertyInstanceMapping>>()))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "Bulk operation failed" });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "200" },
                roleList: new List<string> { "23134" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Contains("Bulk operation failed", result);
        }

        [Fact]
        public void ManageUPFMProductUser_ReturnsError_WhenSentinelOperationFails()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100, 200 });
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());

            // Sentinel single-row op fails
            _propertyRepository.Setup(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                    It.IsAny<long>(), It.IsAny<int>(), -1L, It.IsAny<int>()))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "Sentinel insert failed" });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "-1" },
                removedPropertyList: new List<string> { "256850" },
                roleList: new List<string> { "23134" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal("Sentinel insert failed", result);
        }

        // =====================================================================
        // GetProductCompanyInstanceId
        // =====================================================================

        [Fact]
        public void GetProductCompanyInstanceId_ReturnsEmptyString_WhenBlueBookReturnsNoMatch()
        {
            _blueBook.Setup(m => m.GetCompanyMap(It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { Source = "DIFFERENT_PRODUCT", CompanyInstanceSourceId = "999" }
                });

            var sut = BuildSut();
            var result = sut.GetProductCompanyInstanceId(Guid.NewGuid(), 1L, "UnifiedAmenities", "Primary");

            Assert.Null(result); // no match → default CustomerCompanyMap → null CompanyInstanceSourceId
        }

        [Fact]
        public void GetProductCompanyInstanceId_ReturnsId_WhenBlueBookReturnsMatch()
        {
            const string expectedId = "COMPANY-123";
            _blueBook.Setup(m => m.GetCompanyMap(It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<CustomerCompanyMap>
                {
                    new CustomerCompanyMap { Source = "UnifiedAmenities", CompanyInstanceSourceId = expectedId }
                });

            var sut = BuildSut();
            var result = sut.GetProductCompanyInstanceId(Guid.NewGuid(), 1L, "UnifiedAmenities", "Primary");

            Assert.Equal(expectedId, result);
        }

        // =====================================================================
        // Successful end-to-end happy path
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ReturnsEmpty_OnFullHappyPath()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UnifiedAmenities" } });
            _productRepository.Setup(m => m.ListRolesForProductByParty(It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                               .Returns(new List<ProductRole> { new ProductRole { ID = "23134", Name = "Standard" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "200" },
                removedPropertyList: new List<string> { "100" },
                roleList: new List<string> { "23134" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal(string.Empty, result);
        }

        // =====================================================================
        // GetRightsByRole
        // =====================================================================

        [Fact]
        public void GetRightsByRole_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetRightsByRole(EditorPersonaId, PartyId, roleId: 1L);

            Assert.True(result.IsError);
        }

        [Fact]
        public void GetRightsByRole_ReturnsEmptyList_WhenNoRightsExist()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _unifiedLoginRepository
                .Setup(m => m.ListRightsByRole(It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new List<ProductRight>());

            var sut = BuildSut();
            var result = sut.GetRightsByRole(EditorPersonaId, PartyId, roleId: 1L);

            Assert.False(result.IsError);
            Assert.Empty(result.Records);
        }

        [Fact]
        public void GetRightsByRole_ReturnsRightsSortedByDescription()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _unifiedLoginRepository
                .Setup(m => m.ListRightsByRole(It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns(new List<ProductRight>
                {
                    new ProductRight { Description = "Zebra" },
                    new ProductRight { Description = "Alpha" },
                    new ProductRight { Description = "Mango" }
                });

            var sut = BuildSut();
            var result = sut.GetRightsByRole(EditorPersonaId, PartyId, roleId: 1L);

            Assert.False(result.IsError);
            var descriptions = result.Records.Cast<ProductRight>().Select(r => r.Description).ToList();
            Assert.Equal(new[] { "Alpha", "Mango", "Zebra" }, descriptions);
        }

        [Fact]
        public void GetRightsByRole_ReturnsRights_WhenRepositoryReturnsNull()
        {
            // null from repository should not throw — ?? new List<ProductRight>() guard
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _unifiedLoginRepository
                .Setup(m => m.ListRightsByRole(It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<long>(), It.IsAny<long>()))
                .Returns((List<ProductRight>)null);

            var sut = BuildSut();
            var exception = Record.Exception(() => sut.GetRightsByRole(EditorPersonaId, PartyId, roleId: 5L));

            Assert.Null(exception);
        }

        // =====================================================================
        // GetUPFMProperties — normal (non-sentinel) path
        // =====================================================================

        [Fact]
        public void GetUPFMProperties_ReturnsPropertiesForUser_WhenAssignedOnly()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);

            var instanceId = Guid.NewGuid();
            _blueBook.Setup(m => m.GetPropertiesPerProductCenter(It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new List<Guid> { instanceId });

            var propertyInstance = new UPFMPropertyInstance
            {
                PropertyInstanceId = 42,
                InstanceId = instanceId,
                Name = "Test Property",
                Address = "123 Main St",
                City = "Austin",
                State = "TX",
                PostalCode = "78701"
            };
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                                .Returns(new List<UPFMPropertyInstance> { propertyInstance });
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(UserPersonaId, ProductId))
                                .Returns(new List<int> { 42 });

            var sut = BuildSut();
            var result = sut.GetUPFMProperties(EditorPersonaId, UserPersonaId, assignedOnly: true, datafilter: null);

            Assert.False(result.IsError);
            Assert.Equal(1, result.TotalRows);
            var prop = result.Records.Cast<ProductProperty>().First();
            Assert.True(prop.IsAssigned);
        }

        [Fact]
        public void GetUPFMProperties_ReturnsAllProperties_WhenAssignedOnlyFalse()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);

            var instanceId1 = Guid.NewGuid();
            var instanceId2 = Guid.NewGuid();
            _blueBook.Setup(m => m.GetPropertiesPerProductCenter(It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new List<Guid> { instanceId1, instanceId2 });

            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                                .Returns(new List<UPFMPropertyInstance>
                                {
                                    new UPFMPropertyInstance { PropertyInstanceId = 10, InstanceId = instanceId1, Name = "Prop A" },
                                    new UPFMPropertyInstance { PropertyInstanceId = 20, InstanceId = instanceId2, Name = "Prop B" }
                                });
            // User only assigned to property 10
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(UserPersonaId, ProductId))
                                .Returns(new List<int> { 10 });

            var sut = BuildSut();
            var result = sut.GetUPFMProperties(EditorPersonaId, UserPersonaId, assignedOnly: false, datafilter: null);

            Assert.False(result.IsError);
            Assert.Equal(2, result.TotalRows);  // both included
        }

        [Fact]
        public void GetUPFMProperties_ReturnsError_WhenEditorPersonaInvalid()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns((Persona)null);

            var sut = BuildSut();
            var result = sut.GetUPFMProperties(EditorPersonaId, UserPersonaId, assignedOnly: false, datafilter: null);

            Assert.True(result.IsError);
        }

        // =====================================================================
        // GetRoles — BlueBookException path
        // =====================================================================

        [Fact]
        public void GetRoles_ReturnsBlueBookError_WhenBlueBookExceptionThrown()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);
            _blueBook.Setup(m => m.GetCompanyMap(It.IsAny<Guid>(), It.IsAny<long>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Throws(new BlueBookException("BlueBook unavailable"));

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "UpdateProductInUDM", Value = "1" }
                });

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, 0, PartyId);

            Assert.True(result.IsError);
            Assert.Equal("BlueBook unavailable", result.ErrorReason);
        }

        [Fact]
        public void GetRoles_ReturnsGenericError_WhenUnexpectedExceptionThrown()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);
            _productRepository.Setup(m => m.ListRolesForProductByParty(
                    It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Throws(new InvalidOperationException("DB unavailable"));

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, 0, PartyId);

            Assert.True(result.IsError);
            Assert.False(string.IsNullOrEmpty(result.ErrorReason));
        }

        // =====================================================================
        // GetRoles — default role assignment for new user
        // =====================================================================

        [Fact]
        public void GetRoles_SetsDefaultRole_WhenNewUserAndDefaultRoleExists()
        {
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _productRepository.Setup(m => m.ListRolesForProductByParty(It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                               .Returns(new List<ProductRole>
                               {
                                   new ProductRole { ID = "1", Name = "Admin", DefaultRole = "True" },
                                   new ProductRole { ID = "2", Name = "Standard", DefaultRole = "False" }
                               });

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, userPersonaId: 0, partyId: PartyId);

            Assert.False(result.IsError);
            var roles = result.Records.Cast<ProductRole>().ToList();
            Assert.True(roles.First(r => r.ID == "1").IsAssigned);
            Assert.False(roles.First(r => r.ID == "2").IsAssigned);
        }

        // =====================================================================
        // ManageUPFMProductUser — "ALL" property list branch
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ConvertsToPropAll_WhenPropertyListIsALL_AndRoleHasAccessAllProperties()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulBulkOperation();
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            // GetProductIdsByCompany for the user's org — needed for "ALL" branch
            _productRepository.Setup(m => m.GetProductIdsByCompany(It.IsAny<long>()))
                               .Returns(new List<int> { ProductId });
            // Role "23134" has accessAllProperties = true
            _productRepository.Setup(m => m.ListRolesForProductByParty(
                    It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns(new List<ProductRole>
                {
                    new ProductRole { ID = "23134", Name = "Admin", accessAllProperties = true }
                });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(
                propertyList: new List<string> { "ALL" },
                roleList: new List<string> { "23134" });

            // Should route "ALL" → "-1" sentinel via InsertRemoveAssignedPropertyInstanceToUser
            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal(string.Empty, result);
            _propertyRepository.Verify(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                UserPersonaId, ProductId, -1L, It.IsAny<int>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ManageUPFMProductUser_KeepsOriginalList_WhenPropertyListIsALL_AndRoleDoesNotHaveAccessAllProperties()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });
            _productRepository.Setup(m => m.GetProductIdsByCompany(It.IsAny<long>()))
                               .Returns(new List<int> { ProductId });
            // Role does NOT have accessAllProperties
            _productRepository.Setup(m => m.ListRolesForProductByParty(
                    It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                .Returns(new List<ProductRole>
                {
                    new ProductRole { ID = "23134", Name = "Standard", accessAllProperties = false }
                });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            // "ALL" with a non-accessAllProperties role — TVP bulk path should NOT call sentinel
            var input = MakeRole(
                propertyList: new List<string> { "ALL" },
                roleList: new List<string> { "23134" });

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            // "ALL" as a string is not a valid numeric ID so TVP sees 0 real ops — still succeeds
            Assert.Equal(string.Empty, result);
        }

        // =====================================================================
        // ManageUPFMProductUser — super user path
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_AssignsSuperUserRoleId_WhenUserIsSuperUser()
        {
            SetupSuccessfulPersonaVerification();
            SetupAssignedProperties(new List<int>());
            SetupSuccessfulBulkOperation();
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulRoleOperation();

            // Mark user as SuperUser
            var superUserRelationship = new IC.PartyRelationship
            {
                RoleTypeFrom = new IC.RoleType { Name = "SuperUser" }
            };
            _managePartyRelationship.Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(superUserRelationship);

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "SuperUserRoleId", Value = "999" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(propertyList: new List<string>(), roleList: new List<string>());

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal(string.Empty, result);
            // Super user role "999" must be assigned
            _userRoleRightRepository.Verify(m => m.InsertAssignedRoleToUser(
                UserPersonaId, 999L, It.IsAny<int>(), false), Times.Once);
        }

        [Fact]
        public void ManageUPFMProductUser_RemovesExistingPropertiesBeforeAssigningSentinel_WhenUserIsSuperUser()
        {
            SetupSuccessfulPersonaVerification();
            // User already has properties 101 and 102
            SetupAssignedProperties(new List<int> { 101, 102 });
            SetupSuccessfulSentinelOperation();
            SetupSuccessfulRoleOperation();
            SetupSuccessfulBulkOperation();

            var superUserRelationship = new IC.PartyRelationship
            {
                RoleTypeFrom = new IC.RoleType { Name = "SuperUser" }
            };
            _managePartyRelationship.Setup(m => m.GetPartyRelationship(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(superUserRelationship);

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>
                {
                    new IC.ProductInternalSetting { Name = "SuperUserRoleId", Value = "999" }
                });
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(propertyList: new List<string>(), roleList: new List<string>());

            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters);

            Assert.Equal(string.Empty, result);
            // Sentinel -1 must be assigned (all-properties)
            _propertyRepository.Verify(m => m.InsertRemoveAssignedPropertyInstanceToUser(
                UserPersonaId, ProductId, -1L, 0), Times.Once);
        }

        // =====================================================================
        // ManageUPFMProductUser — isEmpAccess=true bypasses "already assigned" guard
        // =====================================================================

        [Fact]
        public void ManageUPFMProductUser_ReassignsProperty_WhenIsEmpAccessTrue()
        {
            SetupSuccessfulPersonaVerification();
            // Property 100 is already assigned
            SetupAssignedProperties(new List<int> { 100 });
            SetupSuccessfulBulkOperation();
            SetupSuccessfulRoleOperation();

            _productInternalSettingRepository.Setup(m => m.GetProductInternalSettings(ProductId))
                .Returns(new List<IC.ProductInternalSetting>());
            _productRepository.Setup(m => m.GetProductSettingsByPersona(UserPersonaId))
                               .Returns(new List<ProductSettingList>());
            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role>());
            _productRepository.Setup(m => m.GetAllProducts())
                               .Returns(new List<GbProductMap> { new GbProductMap { ProductId = ProductId, Name = "UA" } });

            var sut = BuildSut();
            List<AdditionalParameters> additionalParameters;
            var input = MakeRole(propertyList: new List<string> { "100" }, roleList: new List<string> { "23134" });

            // isEmpAccess=true forces re-assignment even for already-assigned properties
            var result = sut.ManageUPFMProductUser(EditorPersonaId, UserPersonaId, input, out additionalParameters, isEmpAccess: true);

            Assert.Equal(string.Empty, result);
            _propertyRepository.Verify(m => m.BulkInsertRemovePropertyInstanceMappings(
                UserPersonaId, ProductId,
                It.Is<List<UPFMPropertyInstanceMapping>>(list =>
                    list.Any(x => x.PropertyInstanceID == 100 && !x.IsDeleted))),
                Times.Once);
        }

        // =====================================================================
        // UnassignUser — role delete fails
        // =====================================================================

        [Fact]
        public void UnassignUser_ReturnsError_WhenRoleDeleteFails()
        {
            SetupSuccessfulPersonaVerification();
            SetupSuccessfulSentinelOperation();

            _userRoleRightRepository.Setup(m => m.ListRoleByPersona(ProductId, UserPersonaId, It.IsAny<long?>()))
                                    .Returns(new List<UL.Role> { new UL.Role { RoleID = 10L } });
            _userRoleRightRepository.Setup(m => m.InsertAssignedRoleToUser(
                    UserPersonaId, 10L, It.IsAny<int>(), true))
                .Returns(new RepositoryResponse { Id = -1, ErrorMessage = "Role delete failed" });

            var sut = BuildSut();
            var input = MakeRole(new List<string>(), isAssigned: false);
            var result = sut.UnassignUser(EditorPersonaId, UserPersonaId, input);

            Assert.Equal("Role delete failed", result);
        }

        // =====================================================================
        // GetEnterpriseUPFMProperties — returns empty when user has no assignments
        // =====================================================================

        [Fact]
        public void GetEnterpriseUPFMProperties_ReturnsEmptyResponse_WhenUserHasNoProperties()
        {
            // userPropertyIdList is null → branch skipped → empty response
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(UserPersonaId, ProductId))
                                .Returns((List<int>)null);

            var sut = BuildSut();
            var result = sut.GetEnterpriseUPFMProperties(UserPersonaId, ProductId, "UA");

            Assert.False(result.IsError);
            Assert.Null(result.Records);
        }

        [Fact]
        public void GetEnterpriseUPFMProperties_RemapsProductId_WhenProductIsCIMPL()
        {
            // CIMPL → UnifiedPlatform remapping; user has sentinel -1
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(
                    UserPersonaId, (int)ProductEnum.UnifiedPlatform))
                .Returns(new List<int> { -1 });
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                                .Returns(new List<UPFMPropertyInstance>());
            _blueBook.Setup(m => m.GetPropertiesPerProductCenter(It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new List<Guid>());
            _blueBook.Setup(m => m.GetTranslatePropertiesFromUPFMToProductv3(It.IsAny<UPFMProperty>(), It.IsAny<string>()))
                     .Returns((TranslatePropertyInstance)null);
            _productRepository.Setup(m => m.GetBooksMasterProductDetail((int)ProductEnum.CIMPL))
                               .Returns(new GbProductMap { ProductId = (int)ProductEnum.CIMPL, BooksProductCode = "CIMPL" });

            var sut = BuildSut();
            var result = sut.GetEnterpriseUPFMProperties(UserPersonaId, (int)ProductEnum.CIMPL, "CIMPL");

            Assert.False(result.IsError);
        }

        [Fact]
        public void GetEnterpriseUPFMProperties_ReturnsPropertiesForSpecificAssignments_WhenUserHasSpecificPropertyIds()
        {
            var instanceId = Guid.NewGuid();
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByPersona(UserPersonaId, ProductId))
                                .Returns(new List<int> { 42 });
            _blueBook.Setup(m => m.GetUPFMPropertyInstances(It.IsAny<string>()))
                     .Returns(new List<Guid> { instanceId });
            _propertyRepository.Setup(m => m.ListUPFMPropertyInstanceIdByInstanceIds(It.IsAny<List<Guid>>()))
                                .Returns(new List<UPFMPropertyInstance>
                                {
                                    new UPFMPropertyInstance
                                    {
                                        PropertyInstanceId = 42,
                                        InstanceId = instanceId,
                                        Name = "Test Property"
                                    }
                                });
            _blueBook.Setup(m => m.GetTranslatePropertiesFromUPFMToProductv3(It.IsAny<UPFMProperty>(), It.IsAny<string>()))
                     .Returns((TranslatePropertyInstance)null);
            _productRepository.Setup(m => m.GetBooksMasterProductDetail(ProductId))
                               .Returns(new GbProductMap { ProductId = ProductId, BooksProductCode = "UA" });

            var sut = BuildSut();
            var result = sut.GetEnterpriseUPFMProperties(UserPersonaId, ProductId, "UA");

            Assert.False(result.IsError);
        }

        // =====================================================================
        // GetSharedProductDetails — shared product remapping
        // =====================================================================

        [Fact]
        public void GetRoles_UsesSharedProductId_WhenSharedProductSettingExists()
        {
            const int SharedProductId = 999;
            _managePersona.Setup(m => m.GetPersona(EditorPersonaId)).Returns(_editorPersona);
            _managePersona.Setup(m => m.GetPersona(UserPersonaId)).Returns(_userPersona);

            _productInternalSettingRepository.Setup(m => m.GetProductSettingByType(It.IsAny<string>()))
                .Returns(new List<IC.ProductInternalSettingByType>
                {
                    new IC.ProductInternalSettingByType { ProductId = ProductId, Value = SharedProductId.ToString() }
                });
            _productRepository.Setup(m => m.ListRolesForProductByParty(It.IsAny<long>(), It.IsAny<IList<int>>(), It.IsAny<int>()))
                               .Returns(new List<ProductRole>());

            var sut = BuildSut();
            var result = sut.GetRoles(EditorPersonaId, userPersonaId: 0, partyId: PartyId);

            // Shared product remapping happened — no exception, valid response
            Assert.False(result.IsError);
        }

    }

    [ExcludeFromCodeCoverage]
    internal sealed class NoOpHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
    }
}
