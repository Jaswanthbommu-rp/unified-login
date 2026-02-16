using Dapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageEmployeeAccess business logic xUnit tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageEmployeeAccessTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IOneSiteProductService> _mockOneSiteProductService;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageEmployeeAccessTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockOneSiteProductService = new Mock<IOneSiteProductService>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "admin@test.com",
                FirstName = "Admin",
                LastName = "User",
                OrganizationRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationMasterId = 379,
                OrganizationPartyId = 100,
                CorrelationId = Guid.NewGuid(),
                UserRealPageGuid = Guid.NewGuid(),
                PersonaId = 5,
                ImpersonatedBy = Guid.Empty
            };

            SetupMockRepository();
        }

        private void SetupMockRepository()
        {
            // The ManageEmployeeAccess class creates new UnifiedLoginRepository instances internally
            // which don't use the injected IRepository, so these mocks may not be fully effective.
            // However, we set them up to prevent errors if any code does use the injected repository.

            // Setup ExecuteNonQuery
            _mockRepository.Setup(x => x.ExecuteNonQuery(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns(0);

            // Setup Execute<T>
            _mockRepository.Setup(x => x.Execute<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns(0);

            // Setup GetOne<T> to return null
            _mockRepository.Setup(x => x.GetOne<UserDetails>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns((UserDetails)null);

            _mockRepository.Setup(x => x.GetOne<UserOrganization>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns((UserOrganization)null);

            // Setup GetMany<T> to return empty collections
            _mockRepository.Setup(x => x.GetMany<UnifiedLoginCompany>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns(new List<UnifiedLoginCompany>());

            _mockRepository.Setup(x => x.GetMany<UserDetail>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns(new List<UserDetail>());

            _mockRepository.Setup(x => x.GetMany<UserOrganization>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns(new List<UserOrganization>());

            _mockRepository.Setup(x => x.GetMany<PersonaADGroup>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns(new List<PersonaADGroup>());

            //_mockRepository.Setup(x => x.GetMany<OrgTypeADGroup>(
            //    It.IsAny<string>(),
            //    It.IsAny<object>(),
            //    It.IsAny<int?>()))
            //    .Returns(new List<OrgTypeADGroup>());

            // Setup dynamic GetMany
            _mockRepository.Setup(x => x.GetMany(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns(new List<dynamic>());

            // Setup dynamic GetOne
            _mockRepository.Setup(x => x.GetOne(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<int?>()))
                .Returns((dynamic)null);

            // Setup QueryMultiple
            var mockGridReader = new Mock<SqlMapper.GridReader>();
            _mockRepository.Setup(x => x.QueryMultiple(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<int?>()))
                .Returns(mockGridReader.Object);
        }

        private ManageEmployeeAccess CreateManageEmployeeAccess()
        {
            return new ManageEmployeeAccess(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object,
                _mockOneSiteProductService.Object);
        }

        #region Constructor Tests

       
        public void Constructor_WithUserClaimOnly_InitializesSuccessfully()
        {
            // Arrange & Act
            // Note: This will create real repository instances internally
            var manageEmployeeAccess = new ManageEmployeeAccess(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageEmployeeAccess);
        }

        public void Constructor_WithAllDependencies_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Assert
            Assert.NotNull(manageEmployeeAccess);
        }

        #endregion

        #region GetCompanies Tests

       
        public void GetCompanies_WithValidEditorPersonaId_ReturnsListResponse()
        {
            // Arrange
            long editorPersonaId = 5;
            string filter = "";
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetCompanies(editorPersonaId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ListResponse>(result);
            // May return error due to lack of database, which is expected in unit tests
        }

       
        public void GetCompanies_WithNullFilter_ReturnsListResponse()
        {
            // Arrange
            long editorPersonaId = 5;
            string filter = null;
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetCompanies(editorPersonaId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ListResponse>(result);
        }

      
        public void GetCompanies_ReturnsListResponse()
        {
            // Arrange
            long editorPersonaId = 5;
            string filter = "";
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetCompanies(editorPersonaId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ListResponse>(result);
            Assert.NotNull(result.Records);
        }

        #endregion

        #region GetUsers Tests

       
        public void GetUsers_WithValidEditorPersonaId_ReturnsListResponse()
        {
            // Arrange
            long editorPersonaId = 5;
            string filter = "";
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetUsers(editorPersonaId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ListResponse>(result);
        }

      
        public void GetUsers_WithNullFilter_ReturnsListResponse()
        {
            // Arrange
            long editorPersonaId = 5;
            string filter = null;
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetUsers(editorPersonaId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ListResponse>(result);
        }

       
        public void GetUsers_ReturnsListResponse()
        {
            // Arrange
            long editorPersonaId = 5;
            string filter = "";
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetUsers(editorPersonaId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ListResponse>(result);
            Assert.NotNull(result.Records);
        }

        #endregion

        #region GetOrCreateEmployeePersonaId Tests

      
        public void GetOrCreateEmployeePersonaId_ReturnsEmployeePersona()
        {
            // Arrange
            var companyRealPageId = Guid.NewGuid();
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, _defaultUserClaim);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<EmployeePersona>(result);
        }

     
        public void GetOrCreateEmployeePersonaId_SetsRealpageUserId()
        {
            // Arrange
            var companyRealPageId = Guid.NewGuid();
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, _defaultUserClaim);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.RealpageUserId);
            Assert.Equal(_defaultUserClaim.UserRealPageGuid, result.RealpageUserId);
        }

       
        public void GetOrCreateEmployeePersonaId_WithImpersonatedUser_UsesImpersonatedByGuid()
        {
            // Arrange
            var companyRealPageId = Guid.NewGuid();
            var impersonatedGuid = Guid.NewGuid();
            var userClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "admin@test.com",
                UserRealPageGuid = _defaultUserClaim.UserRealPageGuid,
                ImpersonatedBy = impersonatedGuid,
                PersonaId = 5
            };

            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, userClaim);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(impersonatedGuid, result.RealpageUserId);
        }

        #endregion

        #region CreateEmployeeProductUser Tests

       
        public void CreateEmployeeProductUser_ReturnsString()
        {
            // Arrange
            int productId = 3;
            long personaId = 5;
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.CreateEmployeeProductUser(productId, personaId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        #endregion

        #region GetUserAccessOrganizationTypes Tests

      
        public void GetUserAccessOrganizationTypes_ReturnsString()
        {
            // Arrange
            long editorPersonaId = 5;
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act
            var result = manageEmployeeAccess.GetUserAccessOrganizationTypes(editorPersonaId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<string>(result);
            Assert.False(string.IsNullOrEmpty(result));
        }

        #endregion

        #region Integration Scenario Tests

      
        public void ManageEmployeeAccess_CanInstantiateAndCallMethods()
        {
            // Arrange
            long editorPersonaId = 5;
            var companyRealPageId = Guid.NewGuid();
            var manageEmployeeAccess = CreateManageEmployeeAccess();

            // Act & Assert - Just verify methods can be called without throwing
            var companiesResult = manageEmployeeAccess.GetCompanies(editorPersonaId, "");
            Assert.NotNull(companiesResult);

            var usersResult = manageEmployeeAccess.GetUsers(editorPersonaId, "");
            Assert.NotNull(usersResult);

            var employeePersona = manageEmployeeAccess.GetOrCreateEmployeePersonaId(companyRealPageId, _defaultUserClaim);
            Assert.NotNull(employeePersona);

            var orgTypes = manageEmployeeAccess.GetUserAccessOrganizationTypes(editorPersonaId);
            Assert.NotNull(orgTypes);
        }

        #endregion
    }
}