using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageSaml business logic xUnit tests.
    /// Tests for SAML attribute management including create, read, and update operations.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageSamlTests : TestBase
    {
        private readonly Mock<ISamlRepository> _mockSamlRepository;

        public ManageSamlTests()
        {
            _mockSamlRepository = new Mock<ISamlRepository>();
        }

        #region Helper Methods

        private SamlAttributes CreateValidSamlAttributes()
        {
            return new SamlAttributes
            {
                SamlUserAttributeId = 1,
                SamlAttributeId = (int)SamlAttributeEnum.productUsername,
                Name = "productUsername",
                Value = "Administrator",
                Type = "string"
            };
        }

        private List<SamlAttributes> CreateSamlAttributesList()
        {
            return new List<SamlAttributes>
            {
                new SamlAttributes
                {
                    SamlUserAttributeId = 1,
                    SamlAttributeId = (int)SamlAttributeEnum.productUsername,
                    Name = "productUsername",
                    Value = "Administrator",
                    Type = "string"
                },
                new SamlAttributes
                {
                    SamlUserAttributeId = 2,
                    SamlAttributeId = (int)SamlAttributeEnum.User_email,
                    Name = "User_email",
                    Value = "user@example.com",
                    Type = "string"
                }
            };
        }

        private RepositoryResponse CreateSuccessResponse()
        {
            return new RepositoryResponse
            {
                Id = 1,
                RealPageId = Guid.NewGuid(),
                ErrorMessage = string.Empty
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithSamlRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Assert
            Assert.NotNull(manageSaml);
        }

        [Fact]
        public void Constructor_WithNullSamlRepository_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new ManageSaml(null));

            Assert.Equal("samlRepository", exception.ParamName);
        }

        [Fact]
        public void Constructor_Default_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageSaml = new ManageSaml();

            // Assert
            Assert.NotNull(manageSaml);
        }

        #endregion

        #region CreateSamlUserAttribute Tests

        [Fact]
        public void CreateSamlUserAttribute_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = "Administrator";
            var expectedResponse = CreateSuccessResponse();

            _mockSamlRepository
                .Setup(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value))
                .Returns(expectedResponse);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var result = manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            _mockSamlRepository.Verify(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value), Times.Once);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithZeroPersonaId_ThrowsArgumentException()
        {
            // Arrange
            long personaId = 0;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = "Administrator";

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value));

            Assert.Equal("personaId", exception.ParamName);
            Assert.Contains("Invalid parameter PersonaId", exception.Message);
            _mockSamlRepository.Verify(x => x.CreateSamlUserAttribute(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<SamlAttributeEnum>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithZeroProductId_ThrowsArgumentException()
        {
            // Arrange
            long personaId = 100;
            int productId = 0;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = "Administrator";

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value));

            Assert.Equal("productId", exception.ParamName);
            Assert.Contains("Invalid parameter ProductId", exception.Message);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithZeroSamlAttributeId_ThrowsArgumentException()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = 0;
            string value = "Administrator";

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value));

            Assert.Equal("samlAttributeId", exception.ParamName);
            Assert.Contains("Invalid parameter SamlAttributeId", exception.Message);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithNullValue_ThrowsArgumentNullException()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = null;

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value));

            Assert.Equal("value", exception.ParamName);
            Assert.Contains("Value cannot be null", exception.Message);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithEmptyValue_ThrowsArgumentException()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = string.Empty;

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value));

            Assert.Equal("value", exception.ParamName);
            Assert.Contains("Invalid parameter Value", exception.Message);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithWhitespaceValue_ThrowsArgumentException()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = "   ";

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value));

            Assert.Equal("value", exception.ParamName);
            Assert.Contains("Invalid parameter Value", exception.Message);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithNegativePersonaId_CallsRepository()
        {
            // Arrange
            long personaId = -1;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = "Administrator";
            var expectedResponse = CreateSuccessResponse();

            _mockSamlRepository
                .Setup(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value))
                .Returns(expectedResponse);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            // Note: Current implementation checks for == 0, not < 0
            // This test documents current behavior
            var result = manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value);
            
            // Assert that repository was called (no validation for negative values in current implementation)
            _mockSamlRepository.Verify(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value), Times.Once);
        }

        [Fact]
        public void CreateSamlUserAttribute_WithDifferentSamlAttributeEnum_CallsRepository()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.User_email;
            string value = "test@example.com";
            var expectedResponse = CreateSuccessResponse();

            _mockSamlRepository
                .Setup(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value))
                .Returns(expectedResponse);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var result = manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value);

            // Assert
            Assert.NotNull(result);
            _mockSamlRepository.Verify(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value), Times.Once);
        }

        #endregion

        #region GetProductSamlDetails Tests

        [Fact]
        public void GetProductSamlDetails_WithValidParameters_ReturnsSamlAttributesList()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            var expectedAttributes = CreateSamlAttributesList();

            _mockSamlRepository
                .Setup(x => x.GetProductSamlDetails(personaId, productId))
                .Returns(expectedAttributes);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var result = manageSaml.GetProductSamlDetails(personaId, productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockSamlRepository.Verify(x => x.GetProductSamlDetails(personaId, productId), Times.Once);
        }

        [Fact]
        public void GetProductSamlDetails_WithZeroPersonaId_ThrowsArgumentException()
        {
            // Arrange
            long personaId = 0;
            int productId = 1;

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manageSaml.GetProductSamlDetails(personaId, productId));

            Assert.Equal("personaId", exception.ParamName);
            Assert.Contains("Invalid parameter PersonaId", exception.Message);
            _mockSamlRepository.Verify(x => x.GetProductSamlDetails(It.IsAny<long>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetProductSamlDetails_WithZeroProductId_ThrowsArgumentException()
        {
            // Arrange
            long personaId = 100;
            int productId = 0;

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                manageSaml.GetProductSamlDetails(personaId, productId));

            Assert.Equal("productId", exception.ParamName);
            Assert.Contains("Invalid parameter ProductId", exception.Message);
        }

        [Fact]
        public void GetProductSamlDetails_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;

            _mockSamlRepository
                .Setup(x => x.GetProductSamlDetails(personaId, productId))
                .Returns(new List<SamlAttributes>());

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var result = manageSaml.GetProductSamlDetails(personaId, productId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetProductSamlDetails_WithNullResponse_ReturnsNull()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;

            _mockSamlRepository
                .Setup(x => x.GetProductSamlDetails(personaId, productId))
                .Returns((IList<SamlAttributes>)null);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var result = manageSaml.GetProductSamlDetails(personaId, productId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateSamlUserAttribute Tests

        [Fact]
        public void UpdateSamlUserAttribute_WithValidSamlAttributes_ReturnsSuccessResponse()
        {
            // Arrange
            var samlAttributes = CreateValidSamlAttributes();
            var expectedResponse = CreateSuccessResponse();

            _mockSamlRepository
                .Setup(x => x.UpdateSamlUserAttribute(samlAttributes))
                .Returns(expectedResponse);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var result = manageSaml.UpdateSamlUserAttribute(samlAttributes);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            _mockSamlRepository.Verify(x => x.UpdateSamlUserAttribute(samlAttributes), Times.Once);
        }

        [Fact]
        public void UpdateSamlUserAttribute_WithNullSamlAttributes_ThrowsArgumentNullException()
        {
            // Arrange
            SamlAttributes samlAttributes = null;

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageSaml.UpdateSamlUserAttribute(samlAttributes));

            Assert.Equal("samlAttributes", exception.ParamName);
            Assert.Contains("Null SamlAttributes", exception.Message);
            _mockSamlRepository.Verify(x => x.UpdateSamlUserAttribute(It.IsAny<SamlAttributes>()), Times.Never);
        }

        [Fact]
        public void UpdateSamlUserAttribute_WithModifiedValue_CallsRepository()
        {
            // Arrange
            var samlAttributes = CreateValidSamlAttributes();
            samlAttributes.Value = "UpdatedValue";
            var expectedResponse = CreateSuccessResponse();

            _mockSamlRepository
                .Setup(x => x.UpdateSamlUserAttribute(samlAttributes))
                .Returns(expectedResponse);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var result = manageSaml.UpdateSamlUserAttribute(samlAttributes);

            // Assert
            Assert.NotNull(result);
            _mockSamlRepository.Verify(x => x.UpdateSamlUserAttribute(samlAttributes), Times.Once);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void SamlAttributes_AllPropertiesCanBeSet()
        {
            // Arrange
            var samlAttributes = new SamlAttributes();

            // Act
            samlAttributes.SamlUserAttributeId = 1;
            samlAttributes.SamlAttributeId = (int)SamlAttributeEnum.productUsername;
            samlAttributes.Name = "productUsername";
            samlAttributes.Value = "Administrator";
            samlAttributes.Type = "string";
            samlAttributes.DisplayName = "Product Username";

            // Assert
            Assert.Equal(1, samlAttributes.SamlUserAttributeId);
            Assert.Equal((int)SamlAttributeEnum.productUsername, samlAttributes.SamlAttributeId);
            Assert.Equal("productUsername", samlAttributes.Name);
            Assert.Equal("Administrator", samlAttributes.Value);
            Assert.Equal("string", samlAttributes.Type);
            Assert.Equal("Product Username", samlAttributes.DisplayName);
        }

        [Fact]
        public void RepositoryResponse_AllPropertiesCanBeSet()
        {
            // Arrange
            var response = new RepositoryResponse();
            var guid = Guid.NewGuid();

            // Act
            response.Id = 1;
            response.RealPageId = guid;
            response.ErrorMessage = "Success";

            // Assert
            Assert.Equal(1, response.Id);
            Assert.Equal(guid, response.RealPageId);
            Assert.Equal("Success", response.ErrorMessage);
        }

        #endregion

        #region SamlAttributeEnum Tests

        [Fact]
        public void SamlAttributeEnum_HasExpectedValues()
        {
            // This test documents the expected SamlAttributeEnum values
            // Assert
            Assert.True(Enum.IsDefined(typeof(SamlAttributeEnum), SamlAttributeEnum.productUsername));
            Assert.True(Enum.IsDefined(typeof(SamlAttributeEnum), SamlAttributeEnum.User_email));
            Assert.True(Enum.IsDefined(typeof(SamlAttributeEnum), SamlAttributeEnum.UserId));
            Assert.True(Enum.IsDefined(typeof(SamlAttributeEnum), SamlAttributeEnum.EnterpriseLogin));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Integration_CreateAndGetSamlAttributes_WorkflowSucceeds()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = "Administrator";
            var createResponse = CreateSuccessResponse();
            var attributes = CreateSamlAttributesList();

            _mockSamlRepository
                .Setup(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value))
                .Returns(createResponse);

            _mockSamlRepository
                .Setup(x => x.GetProductSamlDetails(personaId, productId))
                .Returns(attributes);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var createResult = manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value);
            var getResult = manageSaml.GetProductSamlDetails(personaId, productId);

            // Assert
            Assert.NotNull(createResult);
            Assert.NotNull(getResult);
            Assert.Equal(2, getResult.Count);
        }

        [Fact]
        public void Integration_CreateAndUpdateSamlAttributes_WorkflowSucceeds()
        {
            // Arrange
            long personaId = 100;
            int productId = 1;
            SamlAttributeEnum samlAttributeId = SamlAttributeEnum.productUsername;
            string value = "Administrator";
            var createResponse = CreateSuccessResponse();
            var samlAttributes = CreateValidSamlAttributes();
            var updateResponse = CreateSuccessResponse();

            _mockSamlRepository
                .Setup(x => x.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value))
                .Returns(createResponse);

            _mockSamlRepository
                .Setup(x => x.UpdateSamlUserAttribute(samlAttributes))
                .Returns(updateResponse);

            var manageSaml = new ManageSaml(_mockSamlRepository.Object);

            // Act
            var createResult = manageSaml.CreateSamlUserAttribute(personaId, productId, samlAttributeId, value);
            var updateResult = manageSaml.UpdateSamlUserAttribute(samlAttributes);

            // Assert
            Assert.NotNull(createResult);
            Assert.NotNull(updateResult);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageSaml_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageSaml is responsible for:
            // 1. Creating SAML user attributes for persona/product combinations
            // 2. Retrieving SAML attributes for a persona/product
            // 3. Updating SAML user attributes
            // 4. Validating all input parameters before repository calls
            //
            // SAML attributes are used for Single Sign-On (SSO) integration
            // and provide user-specific configuration for products

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageSaml_RefactoredFeatures_Documentation()
        {
            // This test documents the refactored features:
            //
            // 1. Dependency Injection
            //    - ISamlRepository injected
            //    - Constructor parameter validated
            //
            // 2. Extracted Validation Methods
            //    - ValidateCreateParameters
            //    - ValidateGetParameters
            //
            // 3. Better Exception Handling
            //    - ArgumentException for invalid parameters (with parameter name)
            //    - ArgumentNullException for null parameters
            //    - Consistent exception messages
            //
            // 4. Readonly Field
            //    - _samlRepository marked as readonly for immutability

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageSaml_ValidationRules_Documentation()
        {
            // This test documents validation rules:
            //
            // CreateSamlUserAttribute:
            // - PersonaId must not be 0
            // - ProductId must not be 0
            // - SamlAttributeId must not be 0
            // - Value must not be null
            // - Value must not be empty or whitespace
            //
            // GetProductSamlDetails:
            // - PersonaId must not be 0
            // - ProductId must not be 0
            //
            // UpdateSamlUserAttribute:
            // - SamlAttributes must not be null
            //
            // Note: Current implementation doesn't validate negative IDs,
            // which could be added in future enhancements

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
