using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Maintenance;
using UnifiedLogin.SharedObjects.Product;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for OrganizationController (async refactor).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OrganizationControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IUserClaimsAccessor> _mockUserClaimsAccessor;
        private readonly Mock<IRepositoryResponse> _mockRepositoryResponse;
        private readonly Mock<IManageOrganizationAsync> _mockManageOrganization;
        private readonly Mock<IManageOrganizationProductAsync> _mockManageOrganizationProduct;
        private readonly Mock<IManageCustomFieldsAsync> _mockManageCustomFields;
        private readonly Mock<IManageUserLoginAsync> _mockManageUserLogin;
        private readonly Mock<IManagePartyRelationshipAsync> _mockManagePartyRelationship;
        private readonly Mock<IManageBlueBookAsync> _mockManageBlueBook;
        private readonly Mock<IManageProductAsync> _mockManageProduct;
        private readonly Mock<IManageCredentialAsync> _mockManageCredential;
        private readonly Mock<IManagePersonAsync> _mockManagePerson;
        private readonly Mock<IManagePersonaAsync> _mockManagePersona;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private OrganizationController _organizationController;

        #endregion

        #region Constructor

        public OrganizationControllerTests()
        {
            _mockUserClaimsAccessor = MockUserClaimsAccessor;
            _mockRepositoryResponse = new Mock<IRepositoryResponse>();
            _mockManageOrganization = new Mock<IManageOrganizationAsync>();
            _mockManageOrganizationProduct = new Mock<IManageOrganizationProductAsync>();
            _mockManageCustomFields = new Mock<IManageCustomFieldsAsync>();
            _mockManageUserLogin = new Mock<IManageUserLoginAsync>();
            _mockManagePartyRelationship = new Mock<IManagePartyRelationshipAsync>();
            _mockManageBlueBook = new Mock<IManageBlueBookAsync>();
            _mockManageProduct = new Mock<IManageProductAsync>();
            _mockManageCredential = new Mock<IManageCredentialAsync>();
            _mockManagePerson = new Mock<IManagePersonAsync>();
            _mockManagePersona = new Mock<IManagePersonaAsync>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _organizationController = new OrganizationController(
                _mockManageOrganization.Object,
                _mockManageOrganizationProduct.Object,
                _mockManageCustomFields.Object,
                _mockManageUserLogin.Object,
                _mockManagePartyRelationship.Object,
                _mockManageBlueBook.Object,
                _mockManageProduct.Object,
                _mockManageCredential.Object,
                _mockManagePerson.Object,
                _mockManagePersona.Object,
                _mockMemoryCache.Object,
                _mockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var controller = new OrganizationController(
                _mockManageOrganization.Object,
                _mockManageOrganizationProduct.Object,
                _mockManageCustomFields.Object,
                _mockManageUserLogin.Object,
                _mockManagePartyRelationship.Object,
                _mockManageBlueBook.Object,
                _mockManageProduct.Object,
                _mockManageCredential.Object,
                _mockManagePerson.Object,
                _mockManagePersona.Object,
                _mockMemoryCache.Object,
                _mockUserClaimsAccessor.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullUserClaimsAccessor_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new OrganizationController(
                _mockManageOrganization.Object,
                _mockManageOrganizationProduct.Object,
                _mockManageCustomFields.Object,
                _mockManageUserLogin.Object,
                _mockManagePartyRelationship.Object,
                _mockManageBlueBook.Object,
                _mockManageProduct.Object,
                _mockManageCredential.Object,
                _mockManagePerson.Object,
                _mockManagePersona.Object,
                _mockMemoryCache.Object,
                null!));
        }

        #endregion

        #region OrganizationCustomFields Tests

        [Fact]
        public async Task OrganizationCustomFields_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var customFields = new List<CustomField>
            {
                new CustomField { FieldId = 1, Name = "Field1" },
                new CustomField { FieldId = 2, Name = "Field2" }
            };

            _mockManageCustomFields
                .Setup(x => x.GetCustomFieldAsync(It.IsAny<IDictionary<object, object>>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(customFields);

            // Act
            var result = await _organizationController.OrganizationCustomFields(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ListResponse>(okResult.Value);
            Assert.Equal(2, response.TotalRows);
        }

        [Fact]
        public async Task OrganizationCustomFields_WithDataFilter_ReturnsOkResult()
        {
            // Arrange
            var datafilter = new RequestParameter();
            var customFields = new List<CustomField> { new CustomField { FieldId = 1 } };

            _mockManageCustomFields
                .Setup(x => x.GetCustomFieldAsync(It.IsAny<IDictionary<object, object>>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(customFields);

            // Act
            var result = await _organizationController.OrganizationCustomFields(datafilter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region GetOrganization Tests

        [Fact]
        public async Task GetOrganization_WithValidRealPageId_ReturnsOkWithOrganization()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var expectedOrg = new Organization { RealPageId = realPageId, Name = "Test Org" };

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(realPageId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedOrg);

            // Act
            var result = await _organizationController.GetOrganization(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var org = Assert.IsType<Organization>(okResult.Value);
            Assert.Equal("Test Org", org.Name);
        }

        [Fact]
        public async Task GetOrganization_WithNullRealPageId_ReturnsOrganizationList()
        {
            // Arrange
            var orgList = new List<Organization>
            {
                new Organization { Name = "Org1" },
                new Organization { Name = "Org2" }
            };

            _mockManageOrganization
                .Setup(x => x.GetOrganizationListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgList);

            // Act
            var result = await _organizationController.GetOrganization(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var orgs = Assert.IsAssignableFrom<IList<Organization>>(okResult.Value);
            Assert.Equal(2, orgs.Count);
        }

        [Fact]
        public async Task GetOrganization_WithInvalidRealPageId_ReturnsNotFound()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(realPageId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization)null!);

            // Act
            var result = await _organizationController.GetOrganization(realPageId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Not found", notFoundResult.Value);
        }

        #endregion

        #region ListOrganizationByEnterpriseUserId Tests

        [Fact]
        public async Task ListOrganizationByEnterpriseUserId_WithValidRealPageId_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var orgList = new List<Organization>
            {
                new Organization { RealPageId = Guid.NewGuid(), Name = "Org1" }
            };

            _mockManageUserLogin
                .Setup(x => x.ListOrganizationByEnterpriseUserIdAsync(realPageId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgList);

            _mockManagePartyRelationship
                .Setup(x => x.GetPartyRelationshipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyRelationship());

            // Act
            var result = await _organizationController.ListOrganizationByEnterpriseUserId(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<Organization, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task ListOrganizationByEnterpriseUserId_WithEmptyGuid_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.ListOrganizationByEnterpriseUserId(Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: realPageId", badRequestResult.Value);
        }

        [Fact]
        public async Task ListOrganizationByEnterpriseUserId_WhenNoOrgsFound_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManageUserLogin
                .Setup(x => x.ListOrganizationByEnterpriseUserIdAsync(realPageId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IList<Organization>)null!);

            // Act
            var result = await _organizationController.ListOrganizationByEnterpriseUserId(realPageId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        #endregion

        #region GetProductsByOrganization Tests

        [Fact]
        public async Task GetProductsByOrganization_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var org = new Organization { RealPageId = realPageId, PartyId = 1000 };
            var productList = new List<ProductUI> { new ProductUI { ProductId = 1, ProductName = "Product1" } };

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(realPageId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(org);

            _mockManageCredential
                .Setup(x => x.CheckPasswordExpirationAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CheckPasswordExpirationResponse { IsPasswordExpired = false });

            _mockManageProduct
                .Setup(x => x.GetProductsAsync(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productList);

            // Act
            var result = await _organizationController.GetProductsByOrganization(realPageId, false, false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProductsByOrganization_WithNullRealPageId_UsesClaimsOrgId()
        {
            // Arrange
            var claimsOrgId = Guid.NewGuid();
            _mockUserClaimsAccessor.Setup(x => x.OrganizationRealPageGuid).Returns(claimsOrgId);

            var org = new Organization { RealPageId = claimsOrgId };

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(claimsOrgId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(org);

            _mockManageCredential
                .Setup(x => x.CheckPasswordExpirationAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CheckPasswordExpirationResponse { IsPasswordExpired = false });

            _mockManageProduct
                .Setup(x => x.GetProductsAsync(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductUI>());

            // Act
            var result = await _organizationController.GetProductsByOrganization(null, false, false);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProductsByOrganization_WhenOrganizationNotFound_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(realPageId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Organization)null!);

            // Act
            var result = await _organizationController.GetProductsByOrganization(realPageId, false, false);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUI, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Organization not found!", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task GetProductsByOrganization_WithMergePersonaAccessAndNoPersona_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var org = new Organization { RealPageId = realPageId };

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(realPageId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(org);

            _mockManagePersona
                .Setup(x => x.GetPersonaAsync(It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Persona)null!);

            // Act
            var result = await _organizationController.GetProductsByOrganization(realPageId, true, false);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ProductUI, IErrorData>>(badRequestResult.Value);
            Assert.Equal("Persona not found!", output.Status.ErrorMsg);
        }

        #endregion

        #region AddProductToOrganization Tests

        [Fact]
        public async Task AddProductToOrganization_WithValidData_ReturnsCreated()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var enableDisableProducts = new EnableDisableProducts
            {
                AddProducts = new List<string> { "Product1" }
            };
            var org = new Organization { RealPageId = realPageId };

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(realPageId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(org);

            _mockManageOrganization
                .Setup(x => x.ParseProductAsync(It.IsAny<List<string>>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            _mockManageProduct
                .Setup(x => x.GetProductsAsync(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductUI>());

            _mockManageOrganizationProduct
                .Setup(x => x.CheckSharedProductsEnabledAsync(It.IsAny<IList<ProductUI>>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse());

            _mockManageOrganizationProduct
                .Setup(x => x.InsertUpdateOrganizationProductAsync(It.IsAny<Organization>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _organizationController.AddProductToOrganization(realPageId, enableDisableProducts);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task AddProductToOrganization_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var enableDisableProducts = new EnableDisableProducts { AddProducts = new List<string>() };

            // Act
            var result = await _organizationController.AddProductToOrganization(Guid.Empty, enableDisableProducts);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var status = Assert.IsType<Status<IErrorData>>(badRequestResult.Value);
            Assert.Equal("Invalid parameter: realPageId", status.ErrorMsg);
        }

        [Fact]
        public async Task AddProductToOrganization_WithNullProducts_ReturnsBadRequest()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var enableDisableProducts = new EnableDisableProducts { AddProducts = null };

            // Act
            var result = await _organizationController.AddProductToOrganization(realPageId, enableDisableProducts);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var status = Assert.IsType<Status<IErrorData>>(badRequestResult.Value);
            Assert.Equal("Products not found!", status.ErrorMsg);
        }

        #endregion

        #region DeleteProductFromOrganization Tests

        [Fact]
        public async Task DeleteProductFromOrganization_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var realPageId = Guid.NewGuid();
            var enableDisableProducts = new EnableDisableProducts
            {
                Removeproducts = new List<string> { "Product1" }
            };
            var org = new Organization { RealPageId = realPageId };

            _mockManageOrganization
                .Setup(x => x.GetOrganizationAsync(realPageId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(org);

            _mockManageOrganization
                .Setup(x => x.ParseProductAsync(It.IsAny<List<string>>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            _mockManageProduct
                .Setup(x => x.GetProductsAsync(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProductUI>());

            _mockManageOrganizationProduct
                .Setup(x => x.CheckSharedProductsEnabledAsync(It.IsAny<IList<ProductUI>>(), It.IsAny<List<int>>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse());

            _mockManageOrganizationProduct
                .Setup(x => x.DeleteProductsFromOrganizationAsync(It.IsAny<List<int>>(), It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _organizationController.DeleteProductFromOrganization(realPageId, enableDisableProducts);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProductFromOrganization_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var enableDisableProducts = new EnableDisableProducts { Removeproducts = new List<string>() };

            // Act
            var result = await _organizationController.DeleteProductFromOrganization(Guid.Empty, enableDisableProducts);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var status = Assert.IsType<Status<IErrorData>>(badRequestResult.Value);
            Assert.Equal("Invalid parameter: realPageId", status.ErrorMsg);
        }

        #endregion

        #region UpdateUsePrimaryPropertyForOrganizationProduct Tests

        [Fact]
        public async Task UpdateUsePrimaryPropertyForOrganizationProduct_WithValidData_ReturnsOkResult()
        {
            // Arrange
            const long organizationPartyId = 1000;
            const int productId = 1;

            _mockManageOrganization
                .Setup(x => x.UpdateUsePrimaryPropertyForOrganizationProductAsync(organizationPartyId, productId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _organizationController.UpdateUsePrimaryPropertyForOrganizationProduct(organizationPartyId, productId, true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateUsePrimaryPropertyForOrganizationProduct_WithZeroOrgPartyId_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.UpdateUsePrimaryPropertyForOrganizationProduct(0, 1, true);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("organizationPartyId not supplied", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUsePrimaryPropertyForOrganizationProduct_WithZeroProductId_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.UpdateUsePrimaryPropertyForOrganizationProduct(1000, 0, true);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("productId not supplied", badRequestResult.Value);
        }

        #endregion

        #region OrganizationType Tests

        [Fact]
        public async Task OrganizationType_WithData_ReturnsOkResult()
        {
            // Arrange
            var orgTypes = new List<OrganizationType>
            {
                new OrganizationType { OrganizationTypeId = 1, Name = "Type1" }
            };

            _mockManageOrganization
                .Setup(x => x.ListOrganizationTypeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgTypes);

            // Act
            var result = await _organizationController.OrganizationType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<OrganizationType, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task OrganizationType_WithNoData_ReturnsOkWithError()
        {
            // Arrange
            _mockManageOrganization
                .Setup(x => x.ListOrganizationTypeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<OrganizationType>)null!);

            // Act
            var result = await _organizationController.OrganizationType();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<OrganizationType, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("List OrganizationType: No data", output.Status.ErrorMsg);
        }

        #endregion

        #region GetOrganizationDomain Tests

        [Fact]
        public async Task GetOrganizationDomain_WithData_ReturnsOkResult()
        {
            // Arrange
            var orgDomains = new List<OrganizationDomain>
            {
                new OrganizationDomain { OrganizationDomainId = 1, Name = "Domain1" }
            };

            _mockManageOrganization
                .Setup(x => x.ListOrganizationDomainAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(orgDomains);

            // Act
            var result = await _organizationController.GetOrganizationDomain();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<OrganizationDomain, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task GetOrganizationDomain_WithNoData_ReturnsOkWithError()
        {
            // Arrange
            _mockManageOrganization
                .Setup(x => x.ListOrganizationDomainAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<OrganizationDomain>)null!);

            // Act
            var result = await _organizationController.GetOrganizationDomain();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<OrganizationDomain, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("List OrganizationDomain: No data", output.Status.ErrorMsg);
        }

        #endregion

        #region GetCompanyList Tests

        [Fact]
        public async Task GetCompanyList_WithValidParameters_ReturnsOkResult()
        {
            // Arrange
            var companyList = new List<CompanySetup>
            {
                new CompanySetup { OrganizationName = "Company1", TotalRecords = 1 }
            };

            _mockManageOrganization
                .Setup(x => x.GetCompanyListAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<IDictionary<object, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(companyList);

            // Act
            var result = await _organizationController.GetCompanyList(organizationName: "Test");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCompanyList_WithNoParameters_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.GetCompanyList();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("organizationName/Domain/BlueId not supplied ", badRequestResult.Value);
        }

        #endregion

        #region GetCompanyMasterByCustomerCompanyId Tests

        [Fact]
        public async Task GetCompanyMasterByCustomerCompanyId_WithValidId_ReturnsOkResult()
        {
            // Arrange
            const long customerCompanyId = 12345;
            var companyMaster = new CompanyMaster();

            _mockManageOrganization
                .Setup(x => x.SearchCompanyDetailsByCustomerCompanyIdAsync(customerCompanyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(companyMaster);

            // Act
            var result = await _organizationController.GetCompanyMasterByCustomerCompanyId(customerCompanyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCompanyMasterByCustomerCompanyId_WithZeroId_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.GetCompanyMasterByCustomerCompanyId(0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("CompanyMasterId not supplied", badRequestResult.Value);
        }

        #endregion

        #region GetPropertiesForCompany Tests

        [Fact]
        public async Task GetPropertiesForCompany_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var propertyList = new List<CompanyPropertySetup>
            {
                new CompanyPropertySetup { Property = new List<PropertySetup>() }
            };

            _mockManageOrganization
                .Setup(x => x.GetPropertiesForCompanyAsync(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<IDictionary<object, object>>(), It.IsAny<long>(),
                    It.IsAny<long>(), It.IsAny<bool?>(), It.IsAny<List<Guid>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(propertyList);

            // Act
            var result = await _organizationController.GetPropertiesForCompany(companyInstanceId, new List<Guid>());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetPropertiesForCompany_WithEmptyGuid_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.GetPropertiesForCompany(Guid.Empty, new List<Guid>());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Company Instance Id not supplied", badRequestResult.Value);
        }

        #endregion

        #region AddPropertyForOrganization Tests

        [Fact]
        public async Task AddPropertyForOrganization_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var property = new UPFMPropertyInstance
            {
                Name = "Property1",
                Domain = "Domain1",
                CustomerPropertyId = "123"
            };

            _mockManageOrganization
                .Setup(x => x.AddPropertyForOrganizationAsync(property, companyInstanceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _organizationController.AddPropertyForOrganization(property, companyInstanceId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task AddPropertyForOrganization_WithEmptyCompanyInstanceId_ReturnsBadRequest()
        {
            // Arrange
            var property = new UPFMPropertyInstance { Name = "Test", Domain = "Domain" };

            // Act
            var result = await _organizationController.AddPropertyForOrganization(property, Guid.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: companyInstanceID", badRequestResult.Value);
        }

        [Fact]
        public async Task AddPropertyForOrganization_WithNullProperty_ReturnsBadRequest()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();

            // Act
            var result = await _organizationController.AddPropertyForOrganization(null!, companyInstanceId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Null parameter: Property Object", badRequestResult.Value);
        }

        [Fact]
        public async Task AddPropertyForOrganization_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            var companyInstanceId = Guid.NewGuid();
            var property = new UPFMPropertyInstance { Name = "", Domain = "Domain" };

            // Act
            var result = await _organizationController.AddPropertyForOrganization(property, companyInstanceId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("PropertyName,Domain should not be empty", badRequestResult.Value);
        }

        #endregion

        #region DeleteProperty Tests

        [Fact]
        public async Task DeleteProperty_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var propertyInstanceId = Guid.NewGuid();
            var companyInstanceId = Guid.NewGuid();

            _mockManageOrganization
                .Setup(x => x.DeletePropertyForOrganizationAsync(propertyInstanceId, companyInstanceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _organizationController.DeleteProperty(propertyInstanceId, companyInstanceId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteProperty_WithEmptyPropertyInstanceId_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.DeleteProperty(Guid.Empty, Guid.NewGuid());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: propertyInstanceID", badRequestResult.Value);
        }

        #endregion

        #region SearchPropertyByBlueId Tests

        [Fact]
        public async Task SearchPropertyByBlueId_WithValidData_ReturnsOkResult()
        {
            // Arrange
            _mockManageOrganization
                .Setup(x => x.SearchPropertyDetailsByCustomerPropertyIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PropertyInstanceSearch());

            // Act
            var result = await _organizationController.SearchPropertyByBlueId("123", "456");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SearchPropertyByBlueId_WithEmptyCustomerPropertyId_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.SearchPropertyByBlueId("", "456");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: companyInstanceID", badRequestResult.Value);
        }

        #endregion

        #region GetProductStatusDetails Tests

        [Fact]
        public async Task GetProductStatusDetails_WithValidData_ReturnsOkResult()
        {
            // Arrange
            _mockManageOrganization
                .Setup(x => x.GetSourceProductDetailsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductPropertyDetails());

            // Act
            var result = await _organizationController.GetProductStatusDetails("123", "source");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProductStatusDetails_WithEmptyProductInstanceId_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.GetProductStatusDetails("", "source");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: companyInstanceID", badRequestResult.Value);
        }

        [Fact]
        public async Task GetProductStatusDetails_WithEmptySource_ReturnsBadRequest()
        {
            // Act
            var result = await _organizationController.GetProductStatusDetails("123", "");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameter: source", badRequestResult.Value);
        }

        #endregion

        #region RunCompanyDatabaseDeleteAndUDMCleanUp Tests

        [Fact]
        public async Task RunCompanyDatabaseDeleteAndUDMCleanUp_ReturnsOkResult()
        {
            // Arrange
            _mockManageOrganization
                .Setup(x => x.DeleteQueuedOrganizationsAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _organizationController.RunCompanyDatabaseDeleteAndUDMCleanUp();

            // Assert
            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region GetOrganizationIdentityProviderType Tests

        [Fact]
        public async Task GetOrganizationIdentityProviderType_WhenNoDataFound_ReturnsOkWithError()
        {
            // Arrange
            var realPageId = Guid.NewGuid();

            _mockManageOrganization
                .Setup(x => x.GetOrganizationIdentityProviderTypeAsync(realPageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IdentityProviderType>());

            // Act
            var result = await _organizationController.GetOrganizationIdentityProviderType(realPageId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IIdentityProviderType, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _organizationController = null!;
            base.Dispose();
        }

        #endregion
    }
}
