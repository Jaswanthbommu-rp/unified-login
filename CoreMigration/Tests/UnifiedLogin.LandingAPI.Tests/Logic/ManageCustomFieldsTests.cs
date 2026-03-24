using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageCustomFields business logic xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManageCustomFields
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageCustomFieldsTests : TestBase
    {
        private readonly Mock<ICustomFieldsRepositoryAsync> _mockCustomFieldsRepository;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageCustomFieldsTests()
        {
            _mockCustomFieldsRepository = new Mock<ICustomFieldsRepositoryAsync>();

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
                UserRealPageGuid = Guid.NewGuid()
            };
        }

        private ManageCustomFields CreateManageCustomFields()
        {
            return new ManageCustomFields(_mockCustomFieldsRepository.Object, _defaultUserClaim);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithRepositoryAndUserClaim_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageCustomFields = CreateManageCustomFields();

            // Assert
            Assert.NotNull(manageCustomFields);
        }

        [Fact]
        public void Constructor_WithUserClaimOnly_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageCustomFields = new ManageCustomFields(_defaultUserClaim);

            // Assert
            Assert.NotNull(manageCustomFields);
        }

        #endregion

        #region AddUpdateFieldValue Tests

        [Fact]
        public void AddUpdateFieldValue_WithZeroCreatedBy_ThrowsException()
        {
            // Arrange
            string validJson = "[{\"FieldId\":1,\"Value\":\"TestValue\"}]";
            long createdBy = 0;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageCustomFields.AddUpdateFieldValue(validJson, createdBy));

            Assert.Equal("Missing CreatedBy UserId.", exception.Message);
        }

        
        public void AddUpdateFieldValue_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string nullJson = null;
            long createdBy = 1;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageCustomFields.AddUpdateFieldValue(nullJson, createdBy));

            Assert.Equal("customFieldsValuesJson", exception.ParamName);
            Assert.Contains("Invalid user Custom Fields Json", exception.Message);
        }

        [Fact]
        public void AddUpdateFieldValue_WithEmptyJson_ThrowsArgumentNullException()
        {
            // Arrange
            string emptyJson = "";
            long createdBy = 1;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageCustomFields.AddUpdateFieldValue(emptyJson, createdBy));

            Assert.Equal("customFieldsValuesJson", exception.ParamName);
            Assert.Contains("Invalid user Custom Fields Json", exception.Message);
        }

        [Fact]
        public void AddUpdateFieldValue_WithWhitespaceJson_ThrowsArgumentNullException()
        {
            // Arrange
            string whitespaceJson = "   ";
            long createdBy = 1;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageCustomFields.AddUpdateFieldValue(whitespaceJson, createdBy));

            Assert.Equal("customFieldsValuesJson", exception.ParamName);
            Assert.Contains("Invalid user Custom Fields Json", exception.Message);
        }

        [Fact]
        public void AddUpdateFieldValue_WithInvalidJson_ThrowsArgumentNullException()
        {
            // Arrange
            string invalidJson = "not valid json";
            long createdBy = 1;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageCustomFields.AddUpdateFieldValue(invalidJson, createdBy));

            Assert.Equal("customFieldsValuesJson", exception.ParamName);
            Assert.Contains("Invalid user Custom Fields Json", exception.Message);
        }

        [Fact]
        public void AddUpdateFieldValue_WithValidParameters_ReturnsRepositoryResponse()
        {
            // Arrange
            string validJson = "[{\"FieldId\":1,\"OrganizationId\":100,\"UserLoginPersonaId\":1,\"Value\":\"TestValue\",\"Enabled\":true,\"Name\":\"CustomField1\",\"FieldTypeId\":1}]";
            long createdBy = 1;

            var expectedResponse = new RepositoryResponse
            {
                Id = 1,
                ErrorMessage = ""
            };

            _mockCustomFieldsRepository
                .Setup(x => x.AddUpdateFieldValue(validJson, createdBy))
                .Returns(expectedResponse);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.AddUpdateFieldValue(validJson, createdBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockCustomFieldsRepository.Verify(x => x.AddUpdateFieldValue(validJson, createdBy), Times.Once);
        }

        [Fact]
        public void AddUpdateFieldValue_WithRepositoryException_ReturnsEmptyResponse()
        {
            // Arrange
            string validJson = "[{\"FieldId\":1,\"OrganizationId\":100,\"UserLoginPersonaId\":1,\"Value\":\"TestValue\",\"Enabled\":true,\"Name\":\"CustomField1\",\"FieldTypeId\":1}]";
            long createdBy = 1;

            _mockCustomFieldsRepository
                .Setup(x => x.AddUpdateFieldValue(validJson, createdBy))
                .Throws(new Exception("Database error"));

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.AddUpdateFieldValue(validJson, createdBy);

            // Assert
            Assert.NotNull(result);
            // Exception is caught and logged, empty response returned
        }

        [Fact]
        public void AddUpdateFieldValue_WithMultipleFieldValues_ReturnsSuccessResponse()
        {
            // Arrange
            string validJson = "[{\"FieldId\":1,\"OrganizationId\":100,\"UserLoginPersonaId\":1,\"Value\":\"Value1\",\"Enabled\":true,\"Name\":\"Field1\",\"FieldTypeId\":1},{\"FieldId\":2,\"OrganizationId\":100,\"UserLoginPersonaId\":1,\"Value\":\"Value2\",\"Enabled\":true,\"Name\":\"Field2\",\"FieldTypeId\":1}]";
            long createdBy = 1;

            var expectedResponse = new RepositoryResponse
            {
                Id = 2,
                ErrorMessage = ""
            };

            _mockCustomFieldsRepository
                .Setup(x => x.AddUpdateFieldValue(validJson, createdBy))
                .Returns(expectedResponse);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.AddUpdateFieldValue(validJson, createdBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
        }

        #endregion

        #region GetCustomField with PartyId Tests

        [Fact]
        public void GetCustomField_WithZeroPartyId_ThrowsException()
        {
            // Arrange
            var globals = new Dictionary<object, object>();
            long partyId = 0;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageCustomFields.GetCustomField(globals, partyId));

            Assert.Equal("Missing Organization PartyId.", exception.Message);
        }

        [Fact]
        public void GetCustomField_WithValidPartyId_ReturnsCustomFieldList()
        {
            // Arrange
            var globals = new Dictionary<object, object>();
            long partyId = 100;

            var expectedFields = new List<CustomField>
            {
                new CustomField
                {
                    FieldId = 1,
                    OrganizationId = partyId,
                    Name = "CustomField1",
                    Description = "First custom field",
                    Enabled = true,
                    FieldTypeId = 1,
                    FieldTypeName = "Text",
                    Sequence = 1
                },
                new CustomField
                {
                    FieldId = 2,
                    OrganizationId = partyId,
                    Name = "CustomField2",
                    Description = "Second custom field",
                    Enabled = true,
                    FieldTypeId = 2,
                    FieldTypeName = "Number",
                    Sequence = 2
                }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomField(partyId, It.IsAny<RequestParameter>()))
                .Returns(expectedFields);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomField(globals, partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("CustomField1", result[0].Name);
            Assert.Equal("CustomField2", result[1].Name);
        }

        [Fact]
        public void GetCustomField_WithRequestParameter_PassesFilterToRepository()
        {
            // Arrange
            var requestParameter = new RequestParameter();
            var globals = new Dictionary<object, object>
            {
                { BaseType.RequestParameter, requestParameter }
            };
            long partyId = 100;

            var expectedFields = new List<CustomField>
            {
                new CustomField { FieldId = 1, Name = "FilteredField", Enabled = true }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomField(partyId, requestParameter))
                .Returns(expectedFields);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomField(globals, partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _mockCustomFieldsRepository.Verify(x => x.GetCustomField(partyId, requestParameter), Times.Once);
        }

        [Fact]
        public void GetCustomField_WithEmptyGlobals_ReturnsCustomFieldList()
        {
            // Arrange
            var globals = new Dictionary<object, object>();
            long partyId = 100;

            var expectedFields = new List<CustomField>
            {
                new CustomField { FieldId = 1, Name = "Field1", Enabled = true }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomField(partyId, It.IsAny<RequestParameter>()))
                .Returns(expectedFields);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomField(globals, partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void GetCustomField_WithRepositoryException_ReturnsEmptyList()
        {
            // Arrange
            var globals = new Dictionary<object, object>();
            long partyId = 100;

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomField(partyId, It.IsAny<RequestParameter>()))
                .Throws(new Exception("Database error"));

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomField(globals, partyId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetCustomField with BooksMasterId Tests

        [Fact]
        public void GetCustomField_WithZeroBooksMasterId_ThrowsException()
        {
            // Arrange
            var globals = new Dictionary<object, object>();
            long booksCustomerMasterId = 0;
            int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageCustomFields.GetCustomField(globals, booksCustomerMasterId, bookMasterTypeId));

            Assert.Equal("Missing Book Master Id.", exception.Message);
        }

        [Fact]
        public void GetCustomField_WithValidBooksMasterId_ReturnsEmptyList()
        {
            // Arrange - Note: The actual repository call is commented out in the source
            var globals = new Dictionary<object, object>();
            long booksCustomerMasterId = 379;
            int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomField(globals, booksCustomerMasterId, bookMasterTypeId);

            // Assert
            Assert.NotNull(result);
            // Returns empty list since repository call is commented out
            Assert.Empty(result);
        }

        [Fact]
        public void GetCustomField_WithBooksMasterIdAndRequestParameter_ReturnsEmptyList()
        {
            // Arrange
            var requestParameter = new RequestParameter();
            var globals = new Dictionary<object, object>
            {
                { BaseType.RequestParameter, requestParameter }
            };
            long booksCustomerMasterId = 379;
            int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomField(globals, booksCustomerMasterId, bookMasterTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetCustomFieldsValues Tests

        [Fact]
        public void GetCustomFieldsValues_WithZeroOrganizationPartyId_ThrowsException()
        {
            // Arrange
            long organizationPartyId = 0;
            var manageCustomFields = CreateManageCustomFields();

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageCustomFields.GetCustomFieldsValues(organizationPartyId));

            Assert.Equal("Missing organization partyId.", exception.Message);
        }

        [Fact]
        public void GetCustomFieldsValues_WithValidOrganizationPartyId_ReturnsCustomFieldValues()
        {
            // Arrange
            long organizationPartyId = 100;

            var expectedValues = new List<CustomFieldValue>
            {
                new CustomFieldValue
                {
                    FieldId = 1,
                    FieldValueId = 100,
                    OrganizationId = organizationPartyId,
                    UserLoginPersonaId = 1,
                    Name = "CustomField1",
                    Value = "Value1",
                    Enabled = true
                },
                new CustomFieldValue
                {
                    FieldId = 2,
                    FieldValueId = 101,
                    OrganizationId = organizationPartyId,
                    UserLoginPersonaId = 1,
                    Name = "CustomField2",
                    Value = "Value2",
                    Enabled = true
                }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, null, null))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Value1", result[0].Value);
            Assert.Equal("Value2", result[1].Value);
        }

        [Fact]
        public void GetCustomFieldsValues_WithUserLoginPersonaId_ReturnsUserSpecificValues()
        {
            // Arrange
            long organizationPartyId = 100;
            long userLoginPersonaId = 5;

            var expectedValues = new List<CustomFieldValue>
            {
                new CustomFieldValue
                {
                    FieldId = 1,
                    FieldValueId = 100,
                    OrganizationId = organizationPartyId,
                    UserLoginPersonaId = userLoginPersonaId,
                    Name = "UserField1",
                    Value = "UserValue1",
                    Enabled = true
                }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, null))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userLoginPersonaId, result[0].UserLoginPersonaId);
            _mockCustomFieldsRepository.Verify(
                x => x.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, null),
                Times.Once);
        }

        [Fact]
        public void GetCustomFieldsValues_WithZeroUserLoginPersonaId_TreatsAsNull()
        {
            // Arrange
            long organizationPartyId = 100;
            long userLoginPersonaId = 0;

            var expectedValues = new List<CustomFieldValue>();

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, null, null))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId);

            // Assert
            Assert.NotNull(result);
            _mockCustomFieldsRepository.Verify(
                x => x.GetCustomFieldsValues(organizationPartyId, null, null),
                Times.Once);
        }

        [Fact]
        public void GetCustomFieldsValues_WithNegativeUserLoginPersonaId_TreatsAsNull()
        {
            // Arrange
            long organizationPartyId = 100;
            long userLoginPersonaId = -1;

            var expectedValues = new List<CustomFieldValue>();

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, null, null))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId);

            // Assert
            Assert.NotNull(result);
            _mockCustomFieldsRepository.Verify(
                x => x.GetCustomFieldsValues(organizationPartyId, null, null),
                Times.Once);
        }

        [Fact]
        public void GetCustomFieldsValues_WithEnabledTrue_ReturnsEnabledFieldsOnly()
        {
            // Arrange
            long organizationPartyId = 100;
            bool enabled = true;

            var expectedValues = new List<CustomFieldValue>
            {
                new CustomFieldValue
                {
                    FieldId = 1,
                    FieldValueId = 100,
                    Name = "EnabledField",
                    Value = "EnabledValue",
                    Enabled = true
                }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, null, enabled))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId, null, enabled);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result[0].Enabled);
            _mockCustomFieldsRepository.Verify(
                x => x.GetCustomFieldsValues(organizationPartyId, null, enabled),
                Times.Once);
        }

        [Fact]
        public void GetCustomFieldsValues_WithEnabledFalse_ReturnsDisabledFieldsOnly()
        {
            // Arrange
            long organizationPartyId = 100;
            bool enabled = false;

            var expectedValues = new List<CustomFieldValue>
            {
                new CustomFieldValue
                {
                    FieldId = 2,
                    FieldValueId = 101,
                    Name = "DisabledField",
                    Value = "DisabledValue",
                    Enabled = false
                }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, null, enabled))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId, null, enabled);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.False(result[0].Enabled);
        }

        [Fact]
        public void GetCustomFieldsValues_WithAllParameters_ReturnsFilteredValues()
        {
            // Arrange
            long organizationPartyId = 100;
            long userLoginPersonaId = 5;
            bool enabled = true;

            var expectedValues = new List<CustomFieldValue>
            {
                new CustomFieldValue
                {
                    FieldId = 1,
                    FieldValueId = 100,
                    OrganizationId = organizationPartyId,
                    UserLoginPersonaId = userLoginPersonaId,
                    Name = "SpecificField",
                    Value = "SpecificValue",
                    Enabled = true
                }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, enabled))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, enabled);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(organizationPartyId, result[0].OrganizationId);
            Assert.Equal(userLoginPersonaId, result[0].UserLoginPersonaId);
            Assert.True(result[0].Enabled);
        }

        [Fact]
        public void GetCustomFieldsValues_WithRepositoryException_ReturnsEmptyList()
        {
            // Arrange
            long organizationPartyId = 100;

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, null, null))
                .Throws(new Exception("Database error"));

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCustomFieldsValues_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            long organizationPartyId = 100;

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, null, null))
                .Returns(new List<CustomFieldValue>());

            var manageCustomFields = CreateManageCustomFields();

            // Act
            var result = manageCustomFields.GetCustomFieldsValues(organizationPartyId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Integration Scenario Tests

        [Fact]
        public void ManageCustomFields_CompleteWorkflow_AddAndRetrieveCustomFields()
        {
            // Arrange
            long organizationPartyId = 100;
            long userLoginPersonaId = 5;
            long createdBy = 1;

            // Setup add/update
            string customFieldJson = "[{\"FieldId\":1,\"OrganizationId\":100,\"UserLoginPersonaId\":5,\"Value\":\"TestValue\",\"Enabled\":true,\"Name\":\"TestField\",\"FieldTypeId\":1}]";
            var addResponse = new RepositoryResponse { Id = 1, ErrorMessage = "" };

            _mockCustomFieldsRepository
                .Setup(x => x.AddUpdateFieldValue(customFieldJson, createdBy))
                .Returns(addResponse);

            // Setup retrieval
            var expectedValues = new List<CustomFieldValue>
            {
                new CustomFieldValue
                {
                    FieldId = 1,
                    OrganizationId = organizationPartyId,
                    UserLoginPersonaId = userLoginPersonaId,
                    Name = "TestField",
                    Value = "TestValue",
                    Enabled = true
                }
            };

            _mockCustomFieldsRepository
                .Setup(x => x.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, true))
                .Returns(expectedValues);

            var manageCustomFields = CreateManageCustomFields();

            // Act - Add
            var addResult = manageCustomFields.AddUpdateFieldValue(customFieldJson, createdBy);

            // Act - Retrieve
            var getResult = manageCustomFields.GetCustomFieldsValues(organizationPartyId, userLoginPersonaId, true);

            // Assert
            Assert.NotNull(addResult);
            Assert.Equal(1, addResult.Id);

            Assert.NotNull(getResult);
            Assert.Single(getResult);
            Assert.Equal("TestValue", getResult[0].Value);
        }

        #endregion
    }
}
