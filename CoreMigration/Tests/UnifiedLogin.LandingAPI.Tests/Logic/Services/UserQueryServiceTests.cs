using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using UnifiedLogin.BusinessLogic.Services.User;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;
using SO = UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Tests.Logic.Services;

/// <summary>
/// Unit tests for UserQueryService with proper async mocking
/// These tests execute in milliseconds, not minutes
/// </summary>
public class UserQueryServiceTests
{
    private readonly Mock<IRepositoryAsync> _mockRepository;
    private readonly Mock<ILogger<UserQueryService>> _mockLogger;
    private readonly UserQueryService _sut;

    public UserQueryServiceTests()
    {
        _mockRepository = new Mock<IRepositoryAsync>();
        _mockLogger = new Mock<ILogger<UserQueryService>>();
        _sut = new UserQueryService(_mockRepository.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_InitializesSuccessfully()
    {
        // Arrange & Act
        var service = new UserQueryService(_mockRepository.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserQueryService(null, _mockLogger.Object));

        Assert.Equal("repositoryAsync", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new UserQueryService(_mockRepository.Object, null));

        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region GetUserDetailsAsync Tests

    [Fact]
    public async Task GetUserDetailsAsync_WithValidPersonaId_ReturnsUserDetails()
    {
        // Arrange
        var personaId = 12345L;
        var expectedUser = new UserDetails
        {
            PersonaId = personaId,
            UserId = 100,
            FirstName = "John",
            LastName = "Doe",
            LoginName = "john.doe@test.com",
            RealPageId = Guid.NewGuid()
        };

        _mockRepository
            .Setup(x => x.GetOneAsync<UserDetails>(
                StoredProcNameConstants.SP_GetUserDetails,
                It.Is<object>(p => HasProperty(p, "personaId", personaId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _sut.GetUserDetailsAsync(personaId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(personaId, result.PersonaId);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);

        _mockRepository.Verify(x => x.GetOneAsync<UserDetails>(
            StoredProcNameConstants.SP_GetUserDetails,
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserDetailsAsync_WithValidRealPageId_ReturnsUserDetails()
    {
        // Arrange
        var realPageId = Guid.NewGuid().ToString();
        var expectedUser = new UserDetails
        {
            RealPageId = Guid.Parse(realPageId),
            FirstName = "Jane",
            LastName = "Smith"
        };

        _mockRepository
            .Setup(x => x.GetOneAsync<UserDetails>(
                StoredProcNameConstants.SP_GetUserDetails,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _sut.GetUserDetailsAsync(null, realPageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane", result.FirstName);
    }

    [Fact]
    public async Task GetUserDetailsAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetOneAsync<UserDetails>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDetails)null);

        // Act
        var result = await _sut.GetUserDetailsAsync(99999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetEnterpriseUserAsync Tests

    [Fact]
    public async Task GetEnterpriseUserAsync_WithValidUsername_ReturnsUser()
    {
        // Arrange
        var username = "testuser@test.com";
        var expectedUser = new SO.User
        {
            LoginId = username,
            Firstname = "Test",
            Lastname = "User",
            RealPageId = Guid.NewGuid()
        };

        _mockRepository
            .Setup(x => x.GetOneAsync<SO.User>(
                StoredProcNameConstants.SP_GetUserByLoginId,
                It.Is<object>(p => HasProperty(p, "loginid", username)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _sut.GetEnterpriseUserAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.LoginId);
        Assert.Equal("Test", result.Firstname);
    }

    [Fact]
    public async Task GetEnterpriseUserAsync_WithNullUsername_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _sut.GetEnterpriseUserAsync(null));
    }

    [Fact]
    public async Task GetEnterpriseUserAsync_WithEmptyUsername_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _sut.GetEnterpriseUserAsync(string.Empty));
    }

    #endregion

    #region CheckOrganizationAdminUserAsync Tests

    [Fact]
    public async Task CheckOrganizationAdminUserAsync_WithAdminUser_ReturnsTrue()
    {
        // Arrange
        var userRealPageId = Guid.NewGuid();
        var orgPartyId = 1000L;

        _mockRepository
            .Setup(x => x.GetOneAsync<int>(
                StoredProcNameConstants.SP_EnterpriseCheckOrgAdmin,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CheckOrganizationAdminUserAsync(userRealPageId, orgPartyId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckOrganizationAdminUserAsync_WithNonAdminUser_ReturnsFalse()
    {
        // Arrange
        var userRealPageId = Guid.NewGuid();
        var orgPartyId = 1000L;

        _mockRepository
            .Setup(x => x.GetOneAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _sut.CheckOrganizationAdminUserAsync(userRealPageId, orgPartyId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckOrganizationAdminUserAsync_WithEmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _sut.CheckOrganizationAdminUserAsync(Guid.Empty, 1000));
    }

    [Fact]
    public async Task CheckOrganizationAdminUserAsync_WithZeroOrgPartyId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _sut.CheckOrganizationAdminUserAsync(Guid.NewGuid(), 0));
    }

    #endregion

    #region GetNavigationMenuAsync Tests

    [Fact]
    public async Task GetNavigationMenuAsync_WithMockedRepo_ReturnsMenuEntries()
    {
        // Arrange
        var expectedMenu = new List<NavigationMenuEntry>
        {
            new() { NavigationMenuId = 1, Name = "Dashboard", Url = "/dashboard" },
            new() { NavigationMenuId = 2, Name = "Users", Url = "/users" },
            new() { NavigationMenuId = 3, Name = "Settings", Url = "/settings" }
        };

        _mockRepository
            .Setup(x => x.GetManyAsync<NavigationMenuEntry>(
                StoredProcNameConstants.SP_GetNavigationMenu,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMenu);

        // Act
        var result = await _sut.GetNavigationMenuAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, m => m.Name == "Dashboard");

        _mockRepository.Verify(x => x.GetManyAsync<NavigationMenuEntry>(
            StoredProcNameConstants.SP_GetNavigationMenu,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNavigationMenuAsync_CalledTwice_UsesCacheOnSecondCall()
    {
        // Arrange
        var menuEntries = new List<NavigationMenuEntry>
        {
            new() { NavigationMenuId = 1, Name = "Dashboard" }
        };

        _mockRepository
            .Setup(x => x.GetManyAsync<NavigationMenuEntry>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(menuEntries);

        // Act
        var result1 = await _sut.GetNavigationMenuAsync();
        var result2 = await _sut.GetNavigationMenuAsync();

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        // Should only call DB once due to caching
        _mockRepository.Verify(x => x.GetManyAsync<NavigationMenuEntry>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetSuperUserCountByOrganizationAsync Tests

    [Fact]
    public async Task GetSuperUserCountByOrganizationAsync_WithValidOrgId_ReturnsCount()
    {
        // Arrange
        var orgPartyId = 1000L;
        var expectedCount = 5L;

        _mockRepository
            .Setup(x => x.GetOneAsync<long>(
                StoredProcNameConstants.SP_GetSuperUsersCountByOrganization,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _sut.GetSuperUserCountByOrganizationAsync(orgPartyId);

        // Assert
        Assert.Equal(expectedCount, result);
    }

    #endregion

    #region Batch Operations Tests (.NET 10 Feature)

    [Fact]
    public async Task GetUserDetailsBatchAsync_WithMultiplePersonaIds_ReturnsAllUsers()
    {
        // Arrange
        var personaIds = new List<long> { 1, 2, 3 };

        _mockRepository
            .Setup(x => x.GetOneAsync<UserDetails>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string sp, object p, CancellationToken ct) =>
            {
                var personaId = GetPropertyValue<long>(p, "personaId");
                return new UserDetails
                {
                    PersonaId = personaId,
                    FirstName = $"User{personaId}",
                    LoginName = $"user{personaId}@test.com"
                };
            });

        // Act
        var result = await _sut.GetUserDetailsBatchAsync(personaIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.True(result.ContainsKey(1));
        Assert.True(result.ContainsKey(2));
        Assert.True(result.ContainsKey(3));
    }

    [Fact]
    public async Task GetUserDetailsBatchAsync_WithEmptyList_ReturnsEmptyDictionary()
    {
        // Arrange
        var personaIds = new List<long>();

        // Act
        var result = await _sut.GetUserDetailsBatchAsync(personaIds);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region IAsyncEnumerable Tests (.NET 10 Feature)

    [Fact]
    public async Task StreamUsersByOrganizationAsync_WithValidOrgId_StreamsUsers()
    {
        // Arrange
        var orgPartyId = 1000L;
        var expectedUsers = new List<UserDetails>
        {
            new() { PersonaId = 1, FirstName = "User1" },
            new() { PersonaId = 2, FirstName = "User2" },
            new() { PersonaId = 3, FirstName = "User3" }
        };

        _mockRepository
            .Setup(x => x.GetManyAsync<UserDetails>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUsers);

        // Act
        var streamedUsers = new List<UserDetails>();
        await foreach (var user in _sut.StreamUsersByOrganizationAsync(orgPartyId))
        {
            streamedUsers.Add(user);
        }

        // Assert
        Assert.Equal(3, streamedUsers.Count);
        Assert.Equal("User1", streamedUsers[0].FirstName);
    }

    [Fact]
    public async Task StreamUsersByOrganizationAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var orgPartyId = 1000L;
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var users = new List<UserDetails>
        {
            new() { PersonaId = 1, FirstName = "User1" }
        };

        _mockRepository
            .Setup(x => x.GetManyAsync<UserDetails>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var user in _sut.StreamUsersByOrganizationAsync(orgPartyId, cts.Token))
            {
                // Should throw before getting here
            }
        });
    }

    #endregion

    #region Helper Methods

    private static bool HasProperty(object obj, string propertyName, object expectedValue)
    {
        var prop = obj.GetType().GetProperty(propertyName);
        if (prop == null) return false;

        var value = prop.GetValue(obj);
        return Equals(value, expectedValue);
    }

    private static T GetPropertyValue<T>(object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName);
        if (prop == null) return default;

        return (T)prop.GetValue(obj);
    }

    #endregion
}