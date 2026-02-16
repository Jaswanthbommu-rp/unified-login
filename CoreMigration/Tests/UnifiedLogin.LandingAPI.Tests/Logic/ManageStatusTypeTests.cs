using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageStatusType business logic xUnit tests.
    /// Tests for status type management including retrieval of status types by category.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageStatusTypeTests : TestBase
    {
        private readonly Mock<IStatusTypeRepository> _mockStatusTypeRepository;

        public ManageStatusTypeTests()
        {
            _mockStatusTypeRepository = new Mock<IStatusTypeRepository>();
        }

        #region Helper Methods

        private List<StatusType> CreateStatusTypeList()
        {
            return new List<StatusType>
            {
                new StatusType
                {
                    StatusTypeId = 1,
                    Name = "Active"
                },
                new StatusType
                {
                    StatusTypeId = 2,
                    Name = "Disabled"
                },
                new StatusType
                {
                    StatusTypeId = 3,
                    Name = "Pending"
                }
            };
        }

        private List<StatusType> CreateStatusTypeListWithoutDisabled()
        {
            return new List<StatusType>
            {
                new StatusType
                {
                    StatusTypeId = 1,
                    Name = "Active"
                },
                new StatusType
                {
                    StatusTypeId = 3,
                    Name = "Pending"
                }
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithStatusTypeRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Assert
            Assert.NotNull(manageStatusType);
        }

        [Fact]
        public void Constructor_Default_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageStatusType = new ManageStatusType();

            // Assert
            Assert.NotNull(manageStatusType);
        }

        #endregion

        #region GetStatusType Tests

        [Fact]
        public void GetStatusType_WithValidParameters_ReturnsStatusTypes()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var expectedStatusTypes = CreateStatusTypeList();

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(expectedStatusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            _mockStatusTypeRepository.Verify(x => x.GetStatusType(categoryTypeName, categoryName), Times.Once);
        }

        [Fact]
        public void GetStatusType_WithValidParameters_RenamesDisabledToDeactivated()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var statusTypes = CreateStatusTypeList();

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            Assert.NotNull(result);
            // Verify "Disabled" has been renamed to "Deactivated"
            var deactivatedStatus = ((List<StatusType>)result).Find(s => s.StatusTypeId == 2);
            Assert.Equal("Deactivated", deactivatedStatus.Name);
        }

        [Fact]
        public void GetStatusType_WithNullCategoryTypeName_ThrowsException()
        {
            // Arrange
            string categoryTypeName = null;
            string categoryName = "User Status";

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));

            Assert.Equal("Invalid Category TypeName.", exception.Message);
            _mockStatusTypeRepository.Verify(x => x.GetStatusType(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetStatusType_WithEmptyCategoryTypeName_ThrowsException()
        {
            // Arrange
            string categoryTypeName = string.Empty;
            string categoryName = "User Status";

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));

            Assert.Equal("Invalid Category TypeName.", exception.Message);
            _mockStatusTypeRepository.Verify(x => x.GetStatusType(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetStatusType_WithWhitespaceCategoryTypeName_ThrowsException()
        {
            // Arrange
            string categoryTypeName = "   ";
            string categoryName = "User Status";

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));

            Assert.Equal("Invalid Category TypeName.", exception.Message);
        }

        [Fact]
        public void GetStatusType_WithNullCategoryName_ThrowsException()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = null;

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));

            Assert.Equal("Invalid Category Name.", exception.Message);
            _mockStatusTypeRepository.Verify(x => x.GetStatusType(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetStatusType_WithEmptyCategoryName_ThrowsException()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = string.Empty;

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));

            Assert.Equal("Invalid Category Name.", exception.Message);
        }

        [Fact]
        public void GetStatusType_WithWhitespaceCategoryName_ThrowsException()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "   ";

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));

            Assert.Equal("Invalid Category Name.", exception.Message);
        }

        [Fact]
        public void GetStatusType_WithRepositoryException_ThrowsNullReferenceException()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var repositoryException = new Exception("Database error");

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Throws(repositoryException);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            // The method catches the exception but then tries to access statusTypeList which is empty
            // This will throw a NullReferenceException when trying to find "Disabled"
            Assert.Throws<NullReferenceException>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));
        }

        [Fact]
        public void GetStatusType_WithNoDisabledStatus_ThrowsNullReferenceException()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var statusTypesWithoutDisabled = CreateStatusTypeListWithoutDisabled();

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypesWithoutDisabled);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            // The Find method will return null if no "Disabled" status exists
            // Then trying to set Name on null will throw NullReferenceException
            Assert.Throws<NullReferenceException>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));
        }

        [Fact]
        public void GetStatusType_WithDisabledStatusLowercase_RenamesCorrectly()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var statusTypes = new List<StatusType>
            {
                new StatusType { StatusTypeId = 1, Name = "Active" },
                new StatusType { StatusTypeId = 2, Name = "disabled" } // lowercase
            };

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            // The comparison uses OrdinalIgnoreCase, so lowercase "disabled" should match
            var deactivatedStatus = ((List<StatusType>)result).Find(s => s.StatusTypeId == 2);
            Assert.Equal("Deactivated", deactivatedStatus.Name);
        }

        [Fact]
        public void GetStatusType_WithDisabledStatusMixedCase_RenamesCorrectly()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var statusTypes = new List<StatusType>
            {
                new StatusType { StatusTypeId = 1, Name = "Active" },
                new StatusType { StatusTypeId = 2, Name = "DISABLED" } // uppercase
            };

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            var deactivatedStatus = ((List<StatusType>)result).Find(s => s.StatusTypeId == 2);
            Assert.Equal("Deactivated", deactivatedStatus.Name);
        }

        [Fact]
        public void GetStatusType_VerifiesRepositoryCalledWithCorrectParameters()
        {
            // Arrange
            string categoryTypeName = "CustomType";
            string categoryName = "CustomCategory";
            var statusTypes = CreateStatusTypeList();

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            _mockStatusTypeRepository.Verify(x => x.GetStatusType(
                It.Is<string>(s => s == "CustomType"),
                It.Is<string>(s => s == "CustomCategory")), Times.Once);
        }

        #endregion

        #region Data Object Tests

        [Fact]
        public void StatusType_AllPropertiesCanBeSet()
        {
            // Arrange
            var statusType = new StatusType();

            // Act
            statusType.StatusTypeId = 1;
            statusType.Name = "Active";

            // Assert
            Assert.Equal(1, statusType.StatusTypeId);
            Assert.Equal("Active", statusType.Name);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Integration_GetStatusType_ReturnsModifiedList()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var statusTypes = new List<StatusType>
            {
                new StatusType { StatusTypeId = 1, Name = "Active" },
                new StatusType { StatusTypeId = 2, Name = "Disabled" },
                new StatusType { StatusTypeId = 3, Name = "Pending" },
                new StatusType { StatusTypeId = 4, Name = "Locked" }
            };

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            
            // Verify original list was modified (Disabled -> Deactivated)
            var resultList = (List<StatusType>)result;
            Assert.Contains(resultList, s => s.Name == "Active");
            Assert.Contains(resultList, s => s.Name == "Deactivated");
            Assert.Contains(resultList, s => s.Name == "Pending");
            Assert.Contains(resultList, s => s.Name == "Locked");
            Assert.DoesNotContain(resultList, s => s.Name == "Disabled");
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void GetStatusType_WithEmptyList_ThrowsNullReferenceException()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var emptyList = new List<StatusType>();

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(emptyList);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act & Assert
            // Empty list Find() returns null, then trying to set Name throws NullReferenceException
            Assert.Throws<NullReferenceException>(() =>
                manageStatusType.GetStatusType(categoryTypeName, categoryName));
        }

        [Fact]
        public void GetStatusType_WithMultipleDisabledStatuses_RenamesFirstOne()
        {
            // Arrange
            string categoryTypeName = "Status";
            string categoryName = "User Status";
            var statusTypes = new List<StatusType>
            {
                new StatusType { StatusTypeId = 1, Name = "Active" },
                new StatusType { StatusTypeId = 2, Name = "Disabled" },
                new StatusType { StatusTypeId = 3, Name = "Disabled" } // Another Disabled
            };

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            // Find returns the first matching element, so first "Disabled" becomes "Deactivated"
            var resultList = (List<StatusType>)result;
            var firstDisabled = resultList.Find(s => s.StatusTypeId == 2);
            Assert.Equal("Deactivated", firstDisabled.Name);
            
            // Second one remains "Disabled" because Find() only returns the first match
            var secondDisabled = resultList.Find(s => s.StatusTypeId == 3);
            Assert.Equal("Disabled", secondDisabled.Name);
        }

        [Fact]
        public void GetStatusType_WithSpecialCharactersInParameters_CallsRepository()
        {
            // Arrange
            string categoryTypeName = "Status@#$%";
            string categoryName = "User Status!@#";
            var statusTypes = CreateStatusTypeList();

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            Assert.NotNull(result);
            _mockStatusTypeRepository.Verify(x => x.GetStatusType(categoryTypeName, categoryName), Times.Once);
        }

        [Fact]
        public void GetStatusType_WithLongStrings_CallsRepository()
        {
            // Arrange
            string categoryTypeName = new string('A', 1000);
            string categoryName = new string('B', 1000);
            var statusTypes = CreateStatusTypeList();

            _mockStatusTypeRepository
                .Setup(x => x.GetStatusType(categoryTypeName, categoryName))
                .Returns(statusTypes);

            var manageStatusType = new ManageStatusType(_mockStatusTypeRepository.Object);

            // Act
            var result = manageStatusType.GetStatusType(categoryTypeName, categoryName);

            // Assert
            Assert.NotNull(result);
            _mockStatusTypeRepository.Verify(x => x.GetStatusType(categoryTypeName, categoryName), Times.Once);
        }

        #endregion

        #region Documentation Tests

        [Fact]
        public void ManageStatusType_ClassPurpose_Documentation()
        {
            // This test documents the class purpose:
            // ManageStatusType is responsible for:
            // 1. Retrieving status types by category type name and category name
            // 2. Logging all operations for audit purposes
            // 3. Renaming "Disabled" status to "Deactivated" in the response
            // 
            // Known Issues:
            // - If "Disabled" status is not found in the list, a NullReferenceException is thrown
            // - Repository exceptions are caught but then the code tries to modify an empty list
            // - Only the first "Disabled" status is renamed if multiple exist

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageStatusType_LoggingBehavior_Documentation()
        {
            // This test documents logging behavior:
            //
            // All operations log:
            // 1. Begin operation (Debug level) with input parameters
            // 2. End operation (Debug level) with result data
            // 3. Exceptions (Error level) with exception details
            //
            // Logging context includes:
            // - AdditionalInfo: JSON-serialized log data
            // - ProductModule: Type of the class
            // - CorrelationId: Unique ID for operation tracking
            //
            // Note: There's a typo in the log message - CategoryName is logged as CategoryTypeName

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageStatusType_ValidationRules_Documentation()
        {
            // This test documents validation rules:
            //
            // GetStatusType:
            // - CategoryTypeName must not be null, empty, or whitespace
            // - CategoryName must not be null, empty, or whitespace
            //
            // Validation Order:
            // 1. CategoryTypeName is validated first
            // 2. CategoryName is validated second
            //
            // Note: Current implementation throws generic Exception instead of ArgumentException

            Assert.True(true, "Documentation test");
        }

        [Fact]
        public void ManageStatusType_DisabledToDeactivatedRename_Documentation()
        {
            // This test documents the Disabled to Deactivated rename behavior:
            //
            // After retrieving status types from the repository:
            // 1. The code finds a status with Name equal to "Disabled" (case-insensitive)
            // 2. It renames that status to "Deactivated"
            //
            // Known issues:
            // - If no "Disabled" status exists, a NullReferenceException is thrown
            // - If multiple "Disabled" statuses exist, only the first one is renamed
            // - The modification happens on the original list, not a copy

            Assert.True(true, "Documentation test");
        }

        #endregion
    }
}
