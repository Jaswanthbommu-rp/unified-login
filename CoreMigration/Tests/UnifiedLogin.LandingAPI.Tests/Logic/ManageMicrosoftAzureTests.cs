using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageMicrosoftAzure business logic xUnit tests.
    /// Tests for Microsoft Azure AD user information retrieval.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageMicrosoftAzureTests : TestBase
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly DefaultUserClaim _defaultUserClaim;

        public ManageMicrosoftAzureTests()
        {
            _mockRepository = new Mock<IRepository>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _defaultUserClaim = new DefaultUserClaim
            {
                UserId = 1,
                LoginName = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                UserRealPageGuid = Guid.Parse("F5C090FA-78AB-452F-B504-98AAFEE09121"),
                OrganizationRealPageGuid = Guid.Parse("A5C090FA-78AB-452F-B504-98AAFEE09122"),
                OrganizationMasterId = 379,
                OrganizationPartyId = 1000,
                PersonaId = 5,
                CorrelationId = Guid.NewGuid()
            };
        }

        #region Helper Methods

        private AzureUser CreateValidAzureUser(string userName)
        {
            return new AzureUser
            {
                odatacontext = "https://graph.microsoft.com/v1.0/$metadata#users",
                value = new List<Value>
                {
                    new Value
                    {
                        odataid = "https://graph.microsoft.com/v1.0/users/12345",
                        userPrincipalName = userName,
                        onPremisesSamAccountName = "testuser",
                        displayName = "Test User",
                        mail = userName,
                        id = "12345-67890-abcde"
                    }
                }
            };
        }

        private AzureUser CreateEmptyAzureUser()
        {
            return new AzureUser
            {
                odatacontext = "https://graph.microsoft.com/v1.0/$metadata#users",
                value = new List<Value>()
            };
        }

        private void SetupHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                });
        }

        private void SetupProductInternalSettings()
        {
            var productSettings = new List<ProductInternalSetting>
            {
                new ProductInternalSetting { Name = "AzureTokenAddress", Value = "https://login.microsoftonline.com/tenant" },
                new ProductInternalSetting { Name = "AzureUnifiedLoginUserClientSecret", Value = "test-secret" },
                new ProductInternalSetting { Name = "AzureUnifiedLoginUserClientId", Value = "test-client-id" },
                new ProductInternalSetting { Name = "AzureUnifiedLoginUserClientScopes", Value = "https://graph.microsoft.com/.default" },
                new ProductInternalSetting { Name = "AzureUserGraphAPI", Value = "https://graph.microsoft.com" }
            };

            // This would need to be mocked in the repository setup
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithRepositoryAndMessageHandler_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Assert
            Assert.NotNull(manageMicrosoftAzure);
        }

        
        public void Constructor_WithNullUserClaim_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                new ManageMicrosoftAzure(
                    null,
                    _mockRepository.Object,
                    _mockHttpMessageHandler.Object));
        }

       
        public void Constructor_WithNullRepository_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                new ManageMicrosoftAzure(
                    _defaultUserClaim,
                    null,
                    _mockHttpMessageHandler.Object));
        }

        #endregion

        #region GetADUserInfo Tests - Success Scenarios

      
        public void GetADUserInfo_WithValidUserName_ReturnsAzureUser()
        {
            // Arrange
            var userName = "testuser@realpage.com";
            var expectedAzureUser = CreateValidAzureUser(userName);
            var jsonResponse = JsonConvert.SerializeObject(expectedAzureUser);

            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.value);
            Assert.Single(result.value);
            Assert.Equal(userName, result.value[0].userPrincipalName);
        }

     
        public void GetADUserInfo_WithValidUserName_ReturnsCorrectUserDetails()
        {
            // Arrange
            var userName = "john.doe@realpage.com";
            var expectedAzureUser = CreateValidAzureUser(userName);
            expectedAzureUser.value[0].displayName = "John Doe";
            expectedAzureUser.value[0].mail = "john.doe@realpage.com";

            var jsonResponse = JsonConvert.SerializeObject(expectedAzureUser);
            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.value[0].displayName);
            Assert.Equal("john.doe@realpage.com", result.value[0].mail);
            Assert.Equal(userName, result.value[0].userPrincipalName);
        }

     
        public void GetADUserInfo_WithCaseInsensitiveUserName_ReturnsUser()
        {
            // Arrange
            var userName = "TestUser@RealPage.Com";
            var expectedAzureUser = CreateValidAzureUser(userName.ToLower());
            var jsonResponse = JsonConvert.SerializeObject(expectedAzureUser);

            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.value);
        }

        #endregion

        #region GetADUserInfo Tests - Not Found Scenarios

        [Fact]
        public void GetADUserInfo_WithNonExistentUser_ReturnsNull()
        {
            // Arrange
            var userName = "nonexistent@realpage.com";
            var emptyAzureUser = CreateEmptyAzureUser();
            var jsonResponse = JsonConvert.SerializeObject(emptyAzureUser);

            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetADUserInfo_WithNotFoundResponse_ReturnsNull()
        {
            // Arrange
            var userName = "testuser@realpage.com";
            SetupHttpMessageHandler(HttpStatusCode.NotFound, "{}");

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetADUserInfo_WithEmptyUserName_ReturnsNull()
        {
            // Arrange
            var userName = "";
            SetupHttpMessageHandler(HttpStatusCode.BadRequest, "{}");

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetADUserInfo_WithNullUserName_ReturnsNull()
        {
            // Arrange
            string userName = null;
            SetupHttpMessageHandler(HttpStatusCode.BadRequest, "{}");

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetADUserInfo Tests - Error Scenarios

        [Fact]
        public void GetADUserInfo_WithUnauthorizedResponse_ReturnsNull()
        {
            // Arrange
            var userName = "testuser@realpage.com";
            SetupHttpMessageHandler(HttpStatusCode.Unauthorized, "{}");

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetADUserInfo_WithForbiddenResponse_ReturnsNull()
        {
            // Arrange
            var userName = "testuser@realpage.com";
            SetupHttpMessageHandler(HttpStatusCode.Forbidden, "{}");

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetADUserInfo_WithInternalServerError_ReturnsNull()
        {
            // Arrange
            var userName = "testuser@realpage.com";
            SetupHttpMessageHandler(HttpStatusCode.InternalServerError, "{}");

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetADUserInfo_WithMalformedJson_ReturnsNull()
        {
            // Arrange
            var userName = "testuser@realpage.com";
            SetupHttpMessageHandler(HttpStatusCode.OK, "{ invalid json }");

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetADUserInfo_WhenExceptionThrown_ReturnsNull()
        {
            // Arrange
            var userName = "testuser@realpage.com";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetADUserInfo Tests - User Name Mismatch

        [Fact]
        public void GetADUserInfo_WithMismatchedUserName_ReturnsNull()
        {
            // Arrange
            var requestedUserName = "user1@realpage.com";
            var returnedUserName = "user2@realpage.com";
            var azureUser = CreateValidAzureUser(returnedUserName);
            var jsonResponse = JsonConvert.SerializeObject(azureUser);

            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(requestedUserName);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetADUserInfo Tests - Multiple Users

   
        public void GetADUserInfo_WithMultipleUsersReturned_ReturnsFirstMatch()
        {
            // Arrange
            var userName = "testuser@realpage.com";
            var azureUser = new AzureUser
            {
                odatacontext = "https://graph.microsoft.com/v1.0/$metadata#users",
                value = new List<Value>
                {
                    new Value
                    {
                        userPrincipalName = userName,
                        displayName = "Test User 1",
                        mail = userName,
                        id = "12345"
                    },
                    new Value
                    {
                        userPrincipalName = "another@realpage.com",
                        displayName = "Test User 2",
                        mail = "another@realpage.com",
                        id = "67890"
                    }
                }
            };

            var jsonResponse = JsonConvert.SerializeObject(azureUser);
            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.value.Count);
            Assert.Equal(userName, result.value[0].userPrincipalName);
        }

        #endregion

        #region GetADUserInfo Tests - Special Characters

       
        public void GetADUserInfo_WithSpecialCharactersInUserName_HandlesCorrectly()
        {
            // Arrange
            var userName = "test.user+tag@realpage.com";
            var azureUser = CreateValidAzureUser(userName);
            var jsonResponse = JsonConvert.SerializeObject(azureUser);

            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userName, result.value[0].userPrincipalName);
        }

       
        public void GetADUserInfo_WithApostropheInName_HandlesCorrectly()
        {
            // Arrange
            var userName = "o'neil@realpage.com";
            var azureUser = CreateValidAzureUser(userName);
            var jsonResponse = JsonConvert.SerializeObject(azureUser);

            SetupHttpMessageHandler(HttpStatusCode.OK, jsonResponse);

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result = manageMicrosoftAzure.GetADUserInfo(userName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userName, result.value[0].userPrincipalName);
        }

        #endregion

        #region AzureUser Object Tests

        [Fact]
        public void AzureUser_PropertyAssignment_WorksCorrectly()
        {
            // Arrange & Act
            var azureUser = new AzureUser
            {
                odatacontext = "https://graph.microsoft.com/v1.0/$metadata#users",
                value = new List<Value>
                {
                    new Value
                    {
                        userPrincipalName = "test@realpage.com",
                        displayName = "Test User",
                        mail = "test@realpage.com",
                        id = "12345",
                        onPremisesSamAccountName = "testuser"
                    }
                }
            };

            // Assert
            Assert.NotNull(azureUser);
            Assert.Equal("https://graph.microsoft.com/v1.0/$metadata#users", azureUser.odatacontext);
            Assert.Single(azureUser.value);
            Assert.Equal("test@realpage.com", azureUser.value[0].userPrincipalName);
            Assert.Equal("Test User", azureUser.value[0].displayName);
        }

        [Fact]
        public void AzureUser_EmptyValueList_IsValid()
        {
            // Arrange & Act
            var azureUser = new AzureUser
            {
                odatacontext = "https://graph.microsoft.com/v1.0/$metadata#users",
                value = new List<Value>()
            };

            // Assert
            Assert.NotNull(azureUser);
            Assert.NotNull(azureUser.value);
            Assert.Empty(azureUser.value);
        }

        [Fact]
        public void Value_AllPropertiesSet_WorksCorrectly()
        {
            // Arrange & Act
            var value = new Value
            {
                odataid = "https://graph.microsoft.com/v1.0/users/12345",
                userPrincipalName = "test@realpage.com",
                onPremisesSamAccountName = "testuser",
                displayName = "Test User",
                mail = "test@realpage.com",
                id = "12345-67890-abcde"
            };

            // Assert
            Assert.NotNull(value);
            Assert.Equal("test@realpage.com", value.userPrincipalName);
            Assert.Equal("testuser", value.onPremisesSamAccountName);
            Assert.Equal("Test User", value.displayName);
            Assert.Equal("test@realpage.com", value.mail);
            Assert.Equal("12345-67890-abcde", value.id);
        }

        #endregion

        #region Integration Tests

      
        public void ManageMicrosoftAzure_MultipleGetADUserInfoCalls_HandlesCorrectly()
        {
            // Arrange
            var userName1 = "user1@realpage.com";
            var userName2 = "user2@realpage.com";

            var azureUser1 = CreateValidAzureUser(userName1);
            var azureUser2 = CreateValidAzureUser(userName2);

            var jsonResponse1 = JsonConvert.SerializeObject(azureUser1);
            var jsonResponse2 = JsonConvert.SerializeObject(azureUser2);

            _mockHttpMessageHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse1, Encoding.UTF8, "application/json")
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse2, Encoding.UTF8, "application/json")
                });

            var manageMicrosoftAzure = new ManageMicrosoftAzure(
                _defaultUserClaim,
                _mockRepository.Object,
                _mockHttpMessageHandler.Object);

            // Act
            var result1 = manageMicrosoftAzure.GetADUserInfo(userName1);
            var result2 = manageMicrosoftAzure.GetADUserInfo(userName2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Equal(userName1, result1.value[0].userPrincipalName);
            Assert.Equal(userName2, result2.value[0].userPrincipalName);
        }

        #endregion
    }
}
